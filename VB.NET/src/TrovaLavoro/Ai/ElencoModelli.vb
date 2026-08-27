Imports System.Net.Http
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading

Namespace Ai

    ''' <summary>
    ''' Un modello che l'API dichiara disponibile: l'identificativo con cui si chiama e il
    ''' nome con cui lo si riconosce.
    ''' </summary>
    Public NotInheritable Class ModelloDisponibile

        Public Sub New(id As String, nome As String)
            Me.Id = If(id, String.Empty).Trim()
            Me.Nome = If(nome, String.Empty).Trim()
        End Sub

        ''' <summary>L'identificativo per l'API, es. <c>claude-sonnet-5</c>.</summary>
        Public ReadOnly Property Id As String

        ''' <summary>Il nome leggibile, es. «Claude Sonnet 5»; vuoto se l'API non lo dice.</summary>
        Public ReadOnly Property Nome As String

        ''' <summary>Come si legge in una tendina: il nome quando c'è, e sempre l'identificativo.</summary>
        ''' <remarks>
        ''' L'identificativo non si nasconde mai dietro il nome, nemmeno quando il nome è
        ''' più bello: è quello che finisce in <c>modelli.json</c>, quello che compare in
        ''' un messaggio d'errore e quello che si cerca nel listino. Un utente che vede
        ''' solo «Claude Sonnet 5» non saprebbe che cosa scrivere nel file.
        ''' </remarks>
        Public Overrides Function ToString() As String
            If String.IsNullOrWhiteSpace(Nome) OrElse Nome = Id Then Return Id
            Return $"{Nome} ({Id})"
        End Function

    End Class

    ''' <summary>Com'è andata la richiesta dell'elenco dei modelli.</summary>
    Public NotInheritable Class EsitoElenco

        Private Sub New(riuscita As Boolean, modelli As IReadOnlyList(Of ModelloDisponibile),
                        causa As CausaErroreAi)
            Me.Riuscita = riuscita
            Me.Modelli = If(modelli, New List(Of ModelloDisponibile)())
            Me.Causa = causa
        End Sub

        ''' <summary>L'elenco è arrivato.</summary>
        Public Shared Function Riuscito(modelli As IReadOnlyList(Of ModelloDisponibile)) As EsitoElenco
            Return New EsitoElenco(True, modelli, CausaErroreAi.Rete)
        End Function

        ''' <summary>L'elenco non è arrivato, e questo è il perché.</summary>
        Public Shared Function Fallito(causa As CausaErroreAi) As EsitoElenco
            Return New EsitoElenco(False, Nothing, causa)
        End Function

        Public ReadOnly Property Riuscita As Boolean

        ''' <summary>I modelli disponibili; vuoto quando non è riuscita.</summary>
        Public ReadOnly Property Modelli As IReadOnlyList(Of ModelloDisponibile)

        ''' <summary>Perché non è arrivato; senza significato quando è riuscita.</summary>
        Public ReadOnly Property Causa As CausaErroreAi

    End Class

    ''' <summary>
    ''' Chiede all'API quali modelli esistono, per poterli offrire nelle Impostazioni
    ''' invece di tenerne una lista compilata dentro (cap. 02.5, cap. 11.6).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché non una lista nel codice.</b> Un elenco compilato invecchia a ogni
    ''' modello nuovo e richiede una build per aggiornarsi — mentre tutto il senso di
    ''' <c>modelli.json</c> è che cambiare modello costi una riga. Chiedendolo all'API
    ''' l'elenco è sempre quello vero, e un modello <b>ritirato</b> sparisce da sé dalla
    ''' tendina invece di restare lì a promettere una chiamata che fallirebbe.</para>
    ''' <para><b>Non costa niente.</b> È la stessa porta di <see cref="ProvaChiave"/> —
    ''' l'elenco dei modelli non consuma token — e per la stessa ragione non lascia riga
    ''' in <c>chiamate_ai.csv</c>: lì si annota quel che si paga.</para>
    ''' <para><b>Non solleva mai e non blocca niente.</b> L'esito è il valore. Senza rete,
    ''' senza chiave o con una chiave rifiutata si torna con la causa in mano e chi ha
    ''' chiesto ripiega sui modelli che già conosce: una tendina non è un servizio da cui
    ''' dipenda il lavoro.</para>
    ''' </remarks>
    Public NotInheritable Class ElencoModelli

        ''' <summary>
        ''' L'elenco dei modelli. Il tetto è largo perché qui l'elenco serve intero: la
        ''' porta della prova chiede invece un solo modello, che le basta.
        ''' </summary>
        Public Const Indirizzo As String = "https://api.anthropic.com/v1/models?limit=100"

        ''' <summary>Quanto si aspetta prima di dire che la rete non c'è.</summary>
        Public Shared ReadOnly Attesa As TimeSpan = TimeSpan.FromSeconds(20)

        Private Sub New()
        End Sub

        ''' <summary>Chiede l'elenco e dice com'è andata. Non solleva: l'esito è il valore.</summary>
        ''' <param name="chiave">La chiave API con cui chiedere.</param>
        ''' <param name="messaggero">Il trasporto, che il banco sostituisce con un finto.</param>
        Public Shared Async Function ChiediAsync(chiave As String,
                                                 Optional messaggero As HttpMessageHandler = Nothing,
                                                 Optional annulla As CancellationToken = Nothing) As Task(Of EsitoElenco)

            If String.IsNullOrWhiteSpace(chiave) Then Return EsitoElenco.Fallito(CausaErroreAi.Chiave)

            Dim http As HttpClient = If(messaggero Is Nothing,
                                        New HttpClient(),
                                        New HttpClient(messaggero, disposeHandler:=False))
            Try
                http.Timeout = Timeout.InfiniteTimeSpan

                Using tempo As New CancellationTokenSource(Attesa)
                    Using insieme As CancellationTokenSource =
                        CancellationTokenSource.CreateLinkedTokenSource(tempo.Token, annulla)

                        Using richiesta As New HttpRequestMessage(HttpMethod.Get, Indirizzo)

                            richiesta.Headers.Add("x-api-key", chiave.Trim())
                            richiesta.Headers.Add("anthropic-version", ClientClaude.VersioneApi)

                            Using risposta As HttpResponseMessage =
                                Await http.SendAsync(richiesta, insieme.Token).ConfigureAwait(False)

                                If Not risposta.IsSuccessStatusCode Then
                                    Return EsitoElenco.Fallito(
                                        ProvaChiave.DallaRisposta(risposta.StatusCode).Causa)
                                End If

                                Dim corpo As String =
                                    Await risposta.Content.ReadAsStringAsync(insieme.Token).ConfigureAwait(False)

                                Return DalCorpo(corpo)

                            End Using

                        End Using

                    End Using
                End Using

            Catch ex As OperationCanceledException When Not annulla.IsCancellationRequested
                Return EsitoElenco.Fallito(CausaErroreAi.Timeout)
            Catch ex As HttpRequestException
                Return EsitoElenco.Fallito(CausaErroreAi.Rete)
            Catch ex As IO.IOException
                Return EsitoElenco.Fallito(CausaErroreAi.Rete)
            Finally
                http.Dispose()
            End Try

        End Function

        ''' <summary>
        ''' I modelli dentro la risposta dell'API. Una risposta che non ha la forma attesa
        ''' non è un elenco vuoto — sarebbe come dire «non esiste nessun modello» — ma un
        ''' fallimento dichiarato, così chi chiama ripiega su quel che conosce.
        ''' </summary>
        Public Shared Function DalCorpo(corpo As String) As EsitoElenco

            Dim dati As JsonArray
            Try
                dati = TryCast(TryCast(JsonNode.Parse(If(corpo, String.Empty)), JsonObject)?("data"), JsonArray)
            Catch ex As JsonException
                Return EsitoElenco.Fallito(CausaErroreAi.RispostaInattesa)
            End Try

            If dati Is Nothing Then Return EsitoElenco.Fallito(CausaErroreAi.RispostaInattesa)

            Dim trovati As New List(Of ModelloDisponibile)

            For Each voce As JsonNode In dati

                Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                If oggetto Is Nothing Then Continue For

                Dim id As String = TryCast(oggetto("id"), JsonValue)?.ToString()
                If String.IsNullOrWhiteSpace(id) Then Continue For

                trovati.Add(New ModelloDisponibile(id, TryCast(oggetto("display_name"), JsonValue)?.ToString()))

            Next

            ' Un elenco vuoto arrivato bene è una risposta che non serve a niente: la
            ' tendina resterebbe senza voci proprio mentre l'API dice di stare bene.
            If trovati.Count = 0 Then Return EsitoElenco.Fallito(CausaErroreAi.RispostaInattesa)

            Return EsitoElenco.Riuscito(trovati)

        End Function

        ''' <summary>
        ''' I modelli che il programma conosce senza chiedere niente a nessuno: i due in
        ''' vigore e i due predefiniti. È il ripiego quando l'elenco vero non arriva —
        ''' pochi, ma sicuramente giusti.
        ''' </summary>
        Public Shared Function Conosciuti(inUso As Modelli) As IReadOnlyList(Of ModelloDisponibile)

            Dim predefiniti As Modelli = Modelli.Predefiniti()

            Return Distinti({
                inUso?.ModelloRagionamento?.Id,
                inUso?.ModelloSemplice?.Id,
                predefiniti.ModelloRagionamento.Id,
                predefiniti.ModelloSemplice.Id})

        End Function

        ''' <summary>
        ''' L'elenco con dentro, garantito, il modello che si sta usando adesso.
        ''' </summary>
        ''' <remarks>
        ''' Un modello <b>ritirato</b> sparisce dall'elenco dell'API mentre resta scritto
        ''' in <c>modelli.json</c> e in uso a ogni chiamata. Se la tendina lo omettesse
        ''' mostrerebbe come scelto un modello diverso da quello vero, e chi guarda non
        ''' avrebbe modo di accorgersene: sta in cima proprio perché è quello da
        ''' sostituire.
        ''' </remarks>
        Public Shared Function ConQuelloInUso(elenco As IReadOnlyList(Of ModelloDisponibile),
                                              idInUso As String) As IReadOnlyList(Of ModelloDisponibile)

            Dim voci As New List(Of ModelloDisponibile)(If(elenco, New List(Of ModelloDisponibile)()))
            Dim cercato As String = If(idInUso, String.Empty).Trim()

            If cercato.Length = 0 Then Return voci
            If voci.Any(Function(v) v.Id = cercato) Then Return voci

            voci.Insert(0, New ModelloDisponibile(cercato, String.Empty))
            Return voci

        End Function

        ''' <summary>Perché l'elenco vero non è arrivato, in una riga per chi guarda.</summary>
        Public Shared Function Perche(causa As CausaErroreAi) As String

            Select Case causa
                Case CausaErroreAi.Chiave
                    Return "senza una chiave API valida non posso chiedere quali modelli ci sono"
                Case CausaErroreAi.Rete, CausaErroreAi.Timeout
                    Return "l'elenco aggiornato richiede la connessione a Internet"
                Case CausaErroreAi.Limite, CausaErroreAi.Servizio
                    Return "il servizio non ha potuto rispondere adesso"
                Case Else
                    Return "l'elenco aggiornato non è arrivato"
            End Select

        End Function

        ''' <summary>Gli identificativi non vuoti, una volta sola, nell'ordine dato.</summary>
        Private Shared Function Distinti(id As IEnumerable(Of String)) As IReadOnlyList(Of ModelloDisponibile)

            Dim visti As New HashSet(Of String)
            Dim voci As New List(Of ModelloDisponibile)

            For Each uno As String In id
                Dim pulito As String = If(uno, String.Empty).Trim()
                If pulito.Length > 0 AndAlso visti.Add(pulito) Then
                    voci.Add(New ModelloDisponibile(pulito, String.Empty))
                End If
            Next

            Return voci

        End Function

    End Class

End Namespace
