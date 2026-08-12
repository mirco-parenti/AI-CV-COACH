Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace Motore

    ''' <summary>A che punto è la pipeline: serve a chi mostra l'attesa.</summary>
    Public Class AvanzamentoPipeline

        ''' <summary>Il passo in corso, da 1 a <see cref="Totale"/>.</summary>
        Public Property Passo As Integer

        ''' <summary>Quanti passi in tutto.</summary>
        Public Property Totale As Integer

        ''' <summary>Cosa si sta facendo, in italiano: è testo da mostrare.</summary>
        Public Property Cosa As String

        Public Overrides Function ToString() As String
            Return $"{Cosa} ({Passo} di {Totale})"
        End Function

    End Class

    ''' <summary>
    ''' L'ordine dei passi di una candidatura, scritto una volta sola: annuncio →
    ''' confronto → punteggio → mitigazione → i due documenti (cap. 02, cap. 12).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché i passi sono anche pubblici uno per uno.</b> Il flusso reale non è
    ''' un bottone solo: l'utente legge il confronto, guarda le stelle e <i>poi</i> decide
    ''' se generare i documenti (cap. 12, A5→A7). Chi conduce l'interfaccia chiama i passi
    ''' quando l'utente li chiede; <see cref="EseguiTuttoAsync"/> è la comodità di chi li
    ''' vuole in fila — il collaudo, e il giorno in cui ci sarà un «fai tutto».</para>
    ''' <para><b>Il punteggio non si chiede all'AI.</b> Lo calcola
    ''' <see cref="CalcoloMatch"/> dai giudizi, ed è deterministico: stessi giudizi,
    ''' stesse stelle, sempre. L'AI dà il suo numero d'insieme, che entra nel calcolo
    ''' come un ingrediente fra gli altri.</para>
    ''' </remarks>
    Public Class PipelineCandidatura

        ''' <summary>I passi che l'avanzamento conta: la mitigazione sta dentro il confronto.</summary>
        Public Const PassiTotali As Integer = 4

        Private ReadOnly _analizzatore As IAnalizzatoreAnnuncio
        Private ReadOnly _confrontatore As IConfrontatore
        Private ReadOnly _generatore As IGeneratore
        Private ReadOnly _taratura As Taratura

        ''' <param name="analizzatore">Il mestiere che struttura l'annuncio.</param>
        ''' <param name="confrontatore">Il mestiere che giudica e mitiga.</param>
        ''' <param name="generatore">Il mestiere che scrive i documenti.</param>
        ''' <param name="taratura">I numeri del calcolo; se omessa, quelli di prodotto.</param>
        Public Sub New(analizzatore As IAnalizzatoreAnnuncio,
                       confrontatore As IConfrontatore,
                       generatore As IGeneratore,
                       Optional taratura As Taratura = Nothing)

            If analizzatore Is Nothing Then Throw New ArgumentNullException(NameOf(analizzatore))
            If confrontatore Is Nothing Then Throw New ArgumentNullException(NameOf(confrontatore))
            If generatore Is Nothing Then Throw New ArgumentNullException(NameOf(generatore))

            _analizzatore = analizzatore
            _confrontatore = confrontatore
            _generatore = generatore
            _taratura = taratura

        End Sub

        ''' <summary>
        ''' Passo 1 — dal testo di un annuncio a un'opportunità che lo contiene.
        ''' </summary>
        Public Async Function AnalizzaAsync(testoAnnuncio As String,
                                            Optional annulla As CancellationToken = Nothing) _
                                            As Task(Of Opportunita)

            Dim annuncio As JsonNode = Await _analizzatore.AnalizzaAsync(testoAnnuncio, annulla).
                ConfigureAwait(False)

            ' L'opportunità nasce «nuova» (cap. 07.3): c'è l'annuncio, non ancora un
            ' confronto. Lo stesso istante fa da data di nascita e da data del primo
            ' stato, che sono la stessa cosa detta due volte solo se le si scrive diverse.
            Dim ora As Date = Date.Now
            Dim opportunita As New Opportunita With {.Annuncio = annuncio, .Creata = ora}
            opportunita.Avanza(StatoOpportunita.Nuova, ora)

            Return opportunita

        End Function

        ''' <summary>
        ''' Passo 2 — i giudizi, il punteggio e, se ci sono gap, le mitigazioni.
        ''' </summary>
        ''' <remarks>
        ''' La mitigazione <b>si salta</b> quando nessun giudizio è «in parte» o «non
        ''' soddisfatto»: senza gap la lista uscirebbe vuota per forza, e chiederlo
        ''' all'AI costerebbe un'attesa e dei token per sapere una cosa che si sa già.
        ''' Il prototipo la chiamava sempre; questa è una differenza voluta, e
        ''' deterministica — non dipende da come il modello si sente quel giorno.
        ''' </remarks>
        Public Async Function ConfrontaAsync(opportunita As Opportunita, profilo As Profilo,
                                             Optional annulla As CancellationToken = Nothing) As Task

            Esigi(opportunita)
            If profilo Is Nothing Then Throw New ArgumentNullException(NameOf(profilo))

            Dim profiloJson As JsonNode = ComeJson(profilo)

            opportunita.Confronto = Await _confrontatore.ConfrontaAsync(
                profiloJson, opportunita.Annuncio, annulla).ConfigureAwait(False)

            Dim giudizi As JsonArray = opportunita.Giudizi()
            opportunita.Match = CalcoloMatch.Calcola(giudizi, NumeroComplessivo(opportunita.Confronto), _taratura)

            If CiSonoGap(giudizi) Then
                opportunita.Mitigazioni = Await _confrontatore.MitigaAsync(
                    profiloJson, giudizi, annulla).ConfigureAwait(False)
            Else
                opportunita.Mitigazioni = New JsonObject From {{"mitigazioni", New JsonArray()}}
            End If

            ' Confrontata vuol dire «interessante» (cap. 07.3): le stelle ci sono e adesso
            ' si può decidere. Un'opportunità già andata avanti — i documenti scritti, o
            ' uno scarto deciso — non torna indietro perché la si riconfronta.
            If opportunita.Stato = StatoOpportunita.Nuova Then
                opportunita.Avanza(StatoOpportunita.Interessante)
            End If

        End Function

        ''' <summary>
        ''' Passo 3 e 4 — il 🎯 CV mirato e la lettera, in quest'ordine perché la lettera
        ''' riceve il CV e deve raccontare la stessa storia.
        ''' </summary>
        Public Async Function GeneraAsync(opportunita As Opportunita, profilo As Profilo,
                                          Optional avanzamento As IProgress(Of AvanzamentoPipeline) = Nothing,
                                          Optional annulla As CancellationToken = Nothing) As Task

            Esigi(opportunita)
            If profilo Is Nothing Then Throw New ArgumentNullException(NameOf(profilo))

            If Not opportunita.Confrontata Then
                Throw New InvalidOperationException(
                    "I documenti si generano dopo il confronto: prima ConfrontaAsync.")
            End If

            Dim profiloJson As JsonNode = ComeJson(profilo)
            Dim giudizi As JsonArray = opportunita.Giudizi()

            Annuncia(avanzamento, 3, "Scrivo il CV mirato")
            opportunita.Cv = Await _generatore.GeneraCvMiratoAsync(
                profiloJson, opportunita.Annuncio, giudizi, annulla).ConfigureAwait(False)

            Annuncia(avanzamento, 4, "Scrivo la lettera")
            opportunita.Lettera = Await _generatore.GeneraLetteraAsync(
                profiloJson, opportunita.Annuncio, giudizi, opportunita.Cv,
                ElencoMitigazioni(opportunita.Mitigazioni), annulla).ConfigureAwait(False)

            ' I documenti ci sono: la candidatura è «generata» (cap. 07.3). Rigenerarli
            ' non è un passaggio nuovo, e la data resta quella della prima volta.
            If opportunita.Stato = StatoOpportunita.Interessante Then
                opportunita.Avanza(StatoOpportunita.Generata)
            End If

        End Function

        ''' <summary>
        ''' Tutti i passi in fila, per chi li vuole in un colpo solo.
        ''' </summary>
        ''' <param name="testoAnnuncio">L'annuncio come lo si è avuto.</param>
        ''' <param name="profilo">Il profilo del candidato.</param>
        ''' <param name="avanzamento">Chi mostra a che punto siamo; può mancare.</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        Public Async Function EseguiTuttoAsync(testoAnnuncio As String, profilo As Profilo,
                                               Optional avanzamento As IProgress(Of AvanzamentoPipeline) = Nothing,
                                               Optional annulla As CancellationToken = Nothing) _
                                               As Task(Of Opportunita)

            Annuncia(avanzamento, 1, "Leggo l'annuncio")
            Dim opportunita As Opportunita = Await AnalizzaAsync(testoAnnuncio, annulla).ConfigureAwait(False)

            Annuncia(avanzamento, 2, "Confronto il profilo con l'annuncio")
            Await ConfrontaAsync(opportunita, profilo, annulla).ConfigureAwait(False)

            Await GeneraAsync(opportunita, profilo, avanzamento, annulla).ConfigureAwait(False)

            Return opportunita

        End Function

        ''' <summary>
        ''' Se fra i giudizi c'è almeno un gap da provare a mitigare. Gli esiti che
        ''' contano sono quelli del prompt: «in parte» e «non soddisfatto». «Non
        ''' determinabile» non è una lacuna — non c'era modo di saperlo — e mitigarlo
        ''' sarebbe inventare.
        ''' </summary>
        Private Shared Function CiSonoGap(giudizi As JsonArray) As Boolean

            If giudizi Is Nothing Then Return False

            For Each g As JsonNode In giudizi

                Dim voce As JsonObject = TryCast(g, JsonObject)
                If voce Is Nothing Then Continue For

                Dim esito As JsonNode = Nothing
                If Not voce.TryGetPropertyValue("esito", esito) OrElse esito Is Nothing Then Continue For
                If esito.GetValueKind() <> Text.Json.JsonValueKind.String Then Continue For

                Select Case esito.GetValue(Of String)().Trim().ToLowerInvariant()
                    Case "in parte", "non soddisfatto"
                        Return True
                End Select

            Next

            Return False

        End Function

        ''' <summary>
        ''' La sola lista delle mitigazioni, che è ciò che il prompt della lettera
        ''' riceve: l'esito del mestiere è l'oggetto <c>{ mitigazioni: [...] }</c>.
        ''' </summary>
        Private Shared Function ElencoMitigazioni(esito As JsonNode) As JsonNode

            Dim oggetto As JsonObject = TryCast(esito, JsonObject)
            If oggetto Is Nothing Then Return esito   ' già una lista, o niente

            Dim voci As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue("mitigazioni", voci) Then Return Nothing

            Return voci

        End Function

        ''' <summary>La stima d'insieme dell'AI, da passare al calcolo.</summary>
        Private Shared Function NumeroComplessivo(confronto As JsonNode) As JsonNode

            Dim oggetto As JsonObject = TryCast(confronto, JsonObject)
            If oggetto Is Nothing Then Return Nothing

            Dim numero As JsonNode = Nothing
            oggetto.TryGetPropertyValue("numero_complessivo", numero)
            Return numero

        End Function

        ''' <summary>Il profilo tipizzato come lo vedono i prompt: JSON e basta.</summary>
        Private Shared Function ComeJson(profilo As Profilo) As JsonNode
            Return JsonNode.Parse(profilo.ComeTesto())
        End Function

        Private Shared Sub Esigi(opportunita As Opportunita)

            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))

            If opportunita.Annuncio Is Nothing Then
                Throw New InvalidOperationException(
                    "L'opportunità non ha ancora un annuncio: prima AnalizzaAsync.")
            End If

        End Sub

        Private Shared Sub Annuncia(avanzamento As IProgress(Of AvanzamentoPipeline),
                                    passo As Integer, cosa As String)

            avanzamento?.Report(New AvanzamentoPipeline With {
                .Passo = passo, .Totale = PassiTotali, .Cosa = cosa})

        End Sub

    End Class

End Namespace
