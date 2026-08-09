Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>Perché una chiamata all'AI non è andata a buon fine.</summary>
    Public Enum CausaErroreAi
        ''' <summary>L'API non è raggiungibile: connessione assente o caduta.</summary>
        Rete
        ''' <summary>L'attesa massima è scaduta senza risposta.</summary>
        Timeout
        ''' <summary>Chiave API assente, non valida o senza permessi.</summary>
        Chiave
        ''' <summary>L'API ha rifiutato la richiesta: è un errore nostro, non suo.</summary>
        Richiesta
        ''' <summary>Troppe richieste: si è superato il limite di frequenza.</summary>
        Limite
        ''' <summary>Guasto temporaneo dalla parte dell'API.</summary>
        Servizio
        ''' <summary>La risposta si è fermata contro il limite di token: è monca.</summary>
        Troncata
        ''' <summary>Il modello si è rifiutato di rispondere.</summary>
        Rifiuto
        ''' <summary>La risposta è arrivata, ma non ha la forma attesa.</summary>
        RispostaInattesa
    End Enum

    ''' <summary>
    ''' Un errore di chiamata all'AI, già scritto in italiano per l'utente (cap. 02.5).
    ''' Porta con sé la <see cref="Causa"/> perché l'interfaccia possa reagire di
    ''' conseguenza: una chiave sbagliata si risolve nelle Impostazioni, una rete caduta
    ''' col pulsante «Riprova».
    ''' </summary>
    Public Class ErroreAi
        Inherits Exception

        ''' <summary>Che tipo di errore è.</summary>
        Public ReadOnly Property Causa As CausaErroreAi

        ''' <summary>Quanto aspettare prima di riprovare, se l'API l'ha suggerito.</summary>
        Public ReadOnly Property AttesaSuggerita As TimeSpan?

        Public Sub New(causa As CausaErroreAi, messaggio As String,
                       Optional interna As Exception = Nothing,
                       Optional attesaSuggerita As TimeSpan? = Nothing)
            MyBase.New(messaggio, interna)
            Me.Causa = causa
            Me.AttesaSuggerita = attesaSuggerita
        End Sub

        ''' <summary>
        ''' Se ha senso riprovare da soli. Vale per gli inciampi di passaggio; non per
        ''' una richiesta malformata o una chiave sbagliata, che riprovando darebbero
        ''' esattamente lo stesso errore.
        ''' </summary>
        Public ReadOnly Property Ritentabile As Boolean
            Get
                Return Causa = CausaErroreAi.Rete OrElse Causa = CausaErroreAi.Timeout OrElse
                       Causa = CausaErroreAi.Limite OrElse Causa = CausaErroreAi.Servizio
            End Get
        End Property

    End Class

    ''' <summary>Quello che l'AI ha risposto, già sbucciato.</summary>
    Public Class RispostaAi

        ''' <summary>Il testo prodotto dal modello: JSON da estrarre, o prosa.</summary>
        Public Property Testo As String

        ''' <summary>Il motivo per cui il modello ha smesso di scrivere.</summary>
        Public Property MotivoFine As String

        ''' <summary>Il modello che ha risposto davvero, come lo dichiara l'API.</summary>
        Public Property Modello As String

        ''' <summary>Token consumati in ingresso e in uscita, per tenere il conto.</summary>
        Public Property TokenIngresso As Integer
        Public Property TokenUscita As Integer

    End Class

    ''' <summary>
    ''' Le chiamate a <c>api.anthropic.com</c> (cap. 02.5). HTTPS dirette con
    ''' <see cref="HttpClient"/>, niente SDK, stesso corpo del prototipo: un solo
    ''' messaggio <c>user</c> col prompt già riempito.
    ''' </summary>
    ''' <remarks>
    ''' <para>A T2 sono <b>solo chiamate sincrone</b>: le risposte stanno fra i 1500 e i
    ''' 4000 token e si aspettano bene con un indicatore. Lo streaming arriva con T4/T7,
    ''' quando ci sarà un pannello che lo mostra man mano.</para>
    ''' <para>Nessuna memoria lato modello: ogni chiamata è autonoma, il contesto lo
    ''' manda il programma.</para>
    ''' </remarks>
    Public Class ClientClaude
        Implements IDisposable

        ''' <summary>L'indirizzo dell'API dei messaggi.</summary>
        Public Const Indirizzo As String = "https://api.anthropic.com/v1/messages"

        ''' <summary>La versione dell'API dichiarata a ogni chiamata, come nel prototipo.</summary>
        Public Const VersioneApi As String = "2023-06-01"

        ''' <summary>La variabile d'ambiente da cui si legge la chiave.</summary>
        Public Const NomeVariabileChiave As String = "ANTHROPIC_API_KEY"

        Private Shared ReadOnly AttesaPredefinita As TimeSpan = TimeSpan.FromSeconds(120)
        Private Shared ReadOnly PausaPredefinita As TimeSpan = TimeSpan.FromSeconds(2)
        Private Shared ReadOnly PausaMassima As TimeSpan = TimeSpan.FromSeconds(30)

        Private ReadOnly _http As HttpClient
        Private ReadOnly _chiave As String
        Private _smaltito As Boolean

        ''' <summary>La mappa livello → modello con cui questo client sta lavorando.</summary>
        Public ReadOnly Property ModelliInUso As Modelli

        ''' <summary>Quanto si aspetta una risposta prima di dichiarare scaduta l'attesa.</summary>
        Public Property TempoMassimo As TimeSpan

        ''' <summary>Quanto si aspetta prima dell'unico ritentativo.</summary>
        Public Property Pausa As TimeSpan

        ''' <param name="chiave">La chiave API.</param>
        ''' <param name="modelli">La mappa dei modelli; se omessa, i predefiniti.</param>
        ''' <param name="messaggero">
        ''' Il gestore HTTP da usare. Serve ai collaudi per far rispondere l'API senza
        ''' rete; in esercizio si lascia stare.
        ''' </param>
        Public Sub New(chiave As String, Optional modelli As Modelli = Nothing,
                       Optional messaggero As HttpMessageHandler = Nothing)

            If String.IsNullOrWhiteSpace(chiave) Then
                Throw New ErroreAi(CausaErroreAi.Chiave,
                    $"Manca la chiave API: definisci la variabile d'ambiente {NomeVariabileChiave}.")
            End If

            _chiave = chiave.Trim()
            ModelliInUso = If(modelli, Modelli.Predefiniti())

            _http = If(messaggero Is Nothing, New HttpClient(),
                       New HttpClient(messaggero, disposeHandler:=False))

            ' L'attesa massima la governiamo chiamata per chiamata: così si distingue
            ' l'attesa scaduta dall'annullamento chiesto dall'utente, che sono due
            ' cose diverse da dire e da trattare.
            _http.Timeout = System.Threading.Timeout.InfiniteTimeSpan

            TempoMassimo = AttesaPredefinita
            Pausa = PausaPredefinita

        End Sub

        ''' <summary>
        ''' La chiave letta dall'ambiente. A T2 è così: la chiave non tocca il disco e
        ''' non entra nel repo. Dalla 1.0 arriverà cifrata dalla cartella dati
        ''' (cap. 11), e questo metodo resterà il ripiego per il banco di collaudo.
        ''' </summary>
        Public Shared Function ChiaveDaAmbiente() As String

            Dim chiave As String = Environment.GetEnvironmentVariable(NomeVariabileChiave)
            If String.IsNullOrWhiteSpace(chiave) Then
                Throw New ErroreAi(CausaErroreAi.Chiave,
                    $"Manca la chiave API: definisci la variabile d'ambiente {NomeVariabileChiave}.")
            End If

            Return chiave.Trim()

        End Function

        ''' <summary>
        ''' Il corpo JSON della richiesta. È volutamente lo stesso del prototipo —
        ''' <c>model</c>, <c>max_tokens</c>, un messaggio <c>user</c> — e nient'altro:
        ''' niente <c>temperature</c>, niente prefill. Quei due, oltre a non servirci,
        ''' sono fra le rotture note del salto a Sonnet 5.
        ''' </summary>
        ''' <param name="contenuto">
        ''' Il contenuto del messaggio: una stringa per i turni di testo, un elenco di
        ''' blocchi per i PDF. L'API accetta entrambe le forme.
        ''' </param>
        Public Shared Function CorpoRichiesta(modello As ModelloConcreto, maxToken As Integer,
                                              contenuto As JsonNode) As JsonObject

            If modello Is Nothing Then Throw New ArgumentNullException(NameOf(modello))
            If contenuto Is Nothing Then Throw New ArgumentNullException(NameOf(contenuto))
            If String.IsNullOrWhiteSpace(modello.Id) Then
                Throw New ArgumentException("Il modello non ha un identificativo.", NameOf(modello))
            End If
            If maxToken <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(maxToken),
                    "Il limite di token deve essere positivo: il prompt lo dichiara nei suoi metadati.")
            End If

            Dim corpo As New JsonObject From {
                {"model", modello.Id},
                {"max_tokens", maxToken},
                {"messages", New JsonArray(New JsonObject From {
                    {"role", "user"},
                    {"content", contenuto}})}}

            ' Il ragionamento si dichiara solo quando la configurazione lo chiede: a
            ' modello spento di suo, tacere tiene la richiesta identica a quella del
            ' prototipo, che è il punto della non-regressione (cap. 14).
            If modello.RagionamentoEsteso.HasValue Then
                corpo("thinking") = New JsonObject From {
                    {"type", If(modello.RagionamentoEsteso.Value, "adaptive", "disabled")}}
            End If

            Return corpo

        End Function

        ''' <summary>
        ''' Sbuccia la risposta dell'API. Le due fini anomale — risposta troncata e
        ''' rifiuto del modello — diventano errori espliciti invece di scivolare avanti.
        ''' </summary>
        Public Shared Function InterpretaRisposta(testoJson As String) As RispostaAi

            Dim radice As JsonObject
            Try
                radice = TryCast(JsonNode.Parse(testoJson), JsonObject)
            Catch ex As JsonException
                Throw New ErroreAi(CausaErroreAi.RispostaInattesa,
                    "L'AI ha risposto qualcosa che non è JSON.", ex)
            End Try

            If radice Is Nothing Then
                Throw New ErroreAi(CausaErroreAi.RispostaInattesa,
                    "L'AI ha risposto un JSON che non è un oggetto.")
            End If

            Dim motivoFine As String = TryCast(radice("stop_reason"), JsonValue)?.ToString()

            ' Prima il motivo della fine, poi il testo. Una risposta troncata il testo
            ' ce l'ha — solo che è monco: passarlo avanti significa scoprirlo dopo, a
            ' valle, sotto forma di JSON invalido senza sapere perché.
            If String.Equals(motivoFine, "max_tokens", StringComparison.Ordinal) Then
                Throw New ErroreAi(CausaErroreAi.Troncata,
                    "La risposta si è fermata contro il limite di token ed è incompleta. " &
                    "Alza il limite del prompt, oppure accorcia il testo in ingresso.")
            End If

            If String.Equals(motivoFine, "refusal", StringComparison.Ordinal) Then
                Throw New ErroreAi(CausaErroreAi.Rifiuto,
                    "Il modello si è rifiutato di rispondere a questa richiesta.")
            End If

            Dim testo As String = PrimoTesto(radice)
            If testo Is Nothing Then
                Throw New ErroreAi(CausaErroreAi.RispostaInattesa,
                    "La risposta dell'AI non contiene nessun blocco di testo.")
            End If

            Dim uso As JsonObject = TryCast(radice("usage"), JsonObject)

            Return New RispostaAi With {
                .Testo = testo,
                .MotivoFine = motivoFine,
                .Modello = TryCast(radice("model"), JsonValue)?.ToString(),
                .TokenIngresso = Intero(uso, "input_tokens"),
                .TokenUscita = Intero(uso, "output_tokens")}

        End Function

        ''' <summary>
        ''' Chiede all'AI e restituisce la risposta. Un solo ritentativo automatico
        ''' sugli inciampi di passaggio (cap. 02.5), rispettando l'attesa suggerita
        ''' dall'API quando c'è.
        ''' </summary>
        ''' <param name="livello">Il livello dichiarato dal prompt: "semplice" o "ragionamento".</param>
        ''' <param name="contenuto">Il contenuto del messaggio utente.</param>
        ''' <param name="maxToken">Il limite di token della risposta, dai metadati del prompt.</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        Public Async Function ChiediAsync(livello As String, contenuto As JsonNode, maxToken As Integer,
                                          Optional annulla As CancellationToken = Nothing) As Task(Of RispostaAi)

            Dim modello As ModelloConcreto = ModelliInUso.PerLivello(livello)
            Dim testoCorpo As String = CorpoRichiesta(modello, maxToken, contenuto).ToJsonString()

            Dim primoErrore As ErroreAi = Nothing

            ' Due tentativi in tutto: quello buono e l'unico ritentativo.
            For tentativo As Integer = 1 To 2

                If tentativo > 1 Then
                    Await Task.Delay(QuantoAspettare(primoErrore), annulla).ConfigureAwait(False)
                End If

                Try
                    Return Await UnTentativoAsync(testoCorpo, annulla).ConfigureAwait(False)
                Catch ex As ErroreAi When ex.Ritentabile AndAlso tentativo = 1
                    primoErrore = ex
                End Try

            Next

            Throw primoErrore

        End Function

        ''' <summary>
        ''' Chiede all'AI usando un prompt del pool: livello e limite di token vengono
        ''' dai suoi metadati, così non si ripetono a ogni chiamata (cap. 04).
        ''' </summary>
        ''' <param name="prompt">Il prompt caricato dalla libreria.</param>
        ''' <param name="testoRiempito">Il suo corpo coi segnaposto già sostituiti.</param>
        Public Function ChiediAsync(prompt As Prompt, testoRiempito As String,
                                    Optional annulla As CancellationToken = Nothing) As Task(Of RispostaAi)

            If prompt Is Nothing Then Throw New ArgumentNullException(NameOf(prompt))

            Return ChiediAsync(prompt.Modello, JsonValue.Create(testoRiempito), prompt.MaxToken, annulla)

        End Function

        ''' <summary>Un singolo tentativo: prepara, manda, classifica.</summary>
        Private Async Function UnTentativoAsync(testoCorpo As String,
                                                annulla As CancellationToken) As Task(Of RispostaAi)

            Using richiesta As New HttpRequestMessage(HttpMethod.Post, Indirizzo)

                richiesta.Content = New StringContent(testoCorpo, New UTF8Encoding(False), "application/json")
                richiesta.Headers.Add("x-api-key", _chiave)
                richiesta.Headers.Add("anthropic-version", VersioneApi)

                Using scadenza As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(annulla)

                    scadenza.CancelAfter(TempoMassimo)

                    Dim risposta As HttpResponseMessage
                    Try
                        risposta = Await _http.SendAsync(richiesta, scadenza.Token).ConfigureAwait(False)
                    Catch ex As OperationCanceledException When Not annulla.IsCancellationRequested
                        ' Il gettone dell'utente non è stato tirato: allora è scaduta l'attesa.
                        Throw New ErroreAi(CausaErroreAi.Timeout,
                            $"L'AI non ha risposto entro {CInt(TempoMassimo.TotalSeconds)} secondi.", ex)
                    Catch ex As HttpRequestException
                        Throw New ErroreAi(CausaErroreAi.Rete,
                            "Non riesco a raggiungere l'AI: controlla la connessione a Internet.", ex)
                    End Try

                    Using risposta

                        Dim testo As String = Await risposta.Content.
                            ReadAsStringAsync(scadenza.Token).ConfigureAwait(False)

                        If Not risposta.IsSuccessStatusCode Then
                            Throw ErroreDaStato(risposta, testo)
                        End If

                        Return InterpretaRisposta(testo)

                    End Using

                End Using
            End Using

        End Function

        ''' <summary>Traduce uno stato HTTP di errore in un errore leggibile.</summary>
        Private Shared Function ErroreDaStato(risposta As HttpResponseMessage, corpo As String) As ErroreAi

            Dim stato As Integer = CInt(risposta.StatusCode)
            Dim dettaglio As String = Sintesi(corpo)

            If stato = 401 OrElse stato = 403 Then
                Return New ErroreAi(CausaErroreAi.Chiave,
                    $"L'AI ha rifiutato la chiave API (HTTP {stato}).{dettaglio}")
            End If

            If stato = 429 Then
                Return New ErroreAi(CausaErroreAi.Limite,
                    $"Troppe richieste all'AI in poco tempo (HTTP 429).{dettaglio}",
                    Nothing, AttesaConsigliata(risposta))
            End If

            If stato >= 500 Then
                ' Anche sui guasti temporanei (529 «overloaded» compreso) l'API può
                ' suggerire quanto aspettare: ignorarlo farebbe ritentare contro lo
                ' stesso muro dopo la pausa fissa.
                Return New ErroreAi(CausaErroreAi.Servizio,
                    $"L'AI ha un problema temporaneo (HTTP {stato}).{dettaglio}",
                    Nothing, AttesaConsigliata(risposta))
            End If

            Return New ErroreAi(CausaErroreAi.Richiesta,
                $"L'AI ha rifiutato la richiesta (HTTP {stato}).{dettaglio}")

        End Function

        ''' <summary>Quanto aspettare prima del ritentativo, entro un tetto ragionevole.</summary>
        Private Function QuantoAspettare(errore As ErroreAi) As TimeSpan

            Dim attesa As TimeSpan = Pausa
            If errore IsNot Nothing AndAlso errore.AttesaSuggerita.HasValue Then
                attesa = errore.AttesaSuggerita.Value
            End If

            ' Un'attesa suggerita spropositata bloccherebbe l'interfaccia più a lungo
            ' di quanto un utente sia disposto a restare fermo a guardare.
            If attesa > PausaMassima Then attesa = PausaMassima
            If attesa < TimeSpan.Zero Then attesa = TimeSpan.Zero

            Return attesa

        End Function

        ''' <summary>L'attesa suggerita dall'API con l'intestazione <c>Retry-After</c>.</summary>
        Private Shared Function AttesaConsigliata(risposta As HttpResponseMessage) As TimeSpan?

            Dim suggerimento = risposta.Headers.RetryAfter
            If suggerimento Is Nothing Then Return Nothing

            If suggerimento.Delta.HasValue Then Return suggerimento.Delta.Value

            If suggerimento.Date.HasValue Then
                Dim quanto As TimeSpan = suggerimento.Date.Value - DateTimeOffset.UtcNow
                If quanto > TimeSpan.Zero Then Return quanto
            End If

            Return Nothing

        End Function

        ''' <summary>
        ''' Il primo blocco di testo della risposta. Il prototipo prende il blocco in
        ''' posizione zero; qui si cerca per tipo, perché col ragionamento acceso il
        ''' primo blocco è il ragionamento e non la risposta. A ragionamento spento i
        ''' due modi coincidono, quindi la non-regressione non ne risente.
        ''' </summary>
        Private Shared Function PrimoTesto(radice As JsonObject) As String

            Dim blocchi As JsonArray = TryCast(radice("content"), JsonArray)
            If blocchi Is Nothing Then Return Nothing

            For Each blocco As JsonNode In blocchi
                Dim oggetto As JsonObject = TryCast(blocco, JsonObject)
                If oggetto Is Nothing Then Continue For
                If String.Equals(TryCast(oggetto("type"), JsonValue)?.ToString(), "text",
                                 StringComparison.Ordinal) Then
                    Return TryCast(oggetto("text"), JsonValue)?.ToString()
                End If
            Next

            Return Nothing

        End Function

        ''' <summary>Legge un intero, zero se assente o non numerico.</summary>
        Private Shared Function Intero(oggetto As JsonObject, chiave As String) As Integer

            If oggetto Is Nothing Then Return 0

            Dim valore As JsonValue = TryCast(oggetto(chiave), JsonValue)
            If valore Is Nothing Then Return 0

            Dim numero As Integer
            Return If(valore.TryGetValue(Of Integer)(numero), numero, 0)

        End Function

        ''' <summary>
        ''' Il corpo di un errore, ridotto a una riga leggibile: serve a capire cosa è
        ''' successo, non a riversare in faccia all'utente la risposta intera.
        ''' </summary>
        Private Shared Function Sintesi(corpo As String) As String

            If String.IsNullOrWhiteSpace(corpo) Then Return String.Empty

            Dim riga As String = corpo.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbTab, " ").Trim()
            If riga.Length > 300 Then riga = riga.Substring(0, 300) & "…"

            Return " " & riga

        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _smaltito Then Return
            _smaltito = True
            _http.Dispose()
            GC.SuppressFinalize(Me)
        End Sub

    End Class

End Namespace
