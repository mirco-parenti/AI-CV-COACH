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

        ''' <summary>
        ''' I passi che l'avanzamento conta: la mitigazione sta dentro il confronto, e
        ''' ciascuno dei due documenti conta due passi — scriverlo e rifinirlo (T7b).
        ''' </summary>
        Public Const PassiTotali As Integer = 6

        ''' <summary>I passi quando la rifinitura non c'è: i due documenti si scrivono e basta.</summary>
        Public Const PassiSenzaRifinitura As Integer = 4

        Private ReadOnly _analizzatore As IAnalizzatoreAnnuncio
        Private ReadOnly _confrontatore As IConfrontatore
        Private ReadOnly _generatore As IGeneratore
        Private ReadOnly _rifinitura As Rifinitura
        Private ReadOnly _linguaPredefinita As Func(Of String)
        Private ReadOnly _taratura As Taratura

        ''' <param name="analizzatore">Il mestiere che struttura l'annuncio.</param>
        ''' <param name="confrontatore">Il mestiere che giudica e mitiga.</param>
        ''' <param name="generatore">Il mestiere che scrive i documenti.</param>
        ''' <param name="taratura">I numeri del calcolo; se omessa, quelli di prodotto.</param>
        ''' <param name="rifinitura">
        ''' La passata anti-slop (cap. 08). Se manca, i documenti restano come l'AI li ha
        ''' scritti: la fila funziona uguale, un gradino più in basso.
        ''' </param>
        Public Sub New(analizzatore As IAnalizzatoreAnnuncio,
                       confrontatore As IConfrontatore,
                       generatore As IGeneratore,
                       Optional taratura As Taratura = Nothing,
                       Optional rifinitura As Rifinitura = Nothing,
                       Optional linguaPredefinita As Func(Of String) = Nothing)

            If analizzatore Is Nothing Then Throw New ArgumentNullException(NameOf(analizzatore))
            If confrontatore Is Nothing Then Throw New ArgumentNullException(NameOf(confrontatore))
            If generatore Is Nothing Then Throw New ArgumentNullException(NameOf(generatore))

            _analizzatore = analizzatore
            _confrontatore = confrontatore
            _generatore = generatore
            _taratura = taratura
            _rifinitura = rifinitura

            ' Come per la rifinitura: è una domanda, non un valore. La preferenza si
            ' cambia dalle Impostazioni a programma acceso, e la candidatura che nasce
            ' dopo dev'essere già della lingua nuova.
            _linguaPredefinita = If(linguaPredefinita, Function() LinguaDocumenti.Italiano)

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
            Dim opportunita As New Opportunita With {
                .Annuncio = annuncio, .Creata = ora,
                .Lingua = LinguaDocumenti.PerDocumenti(LinguaDellAnnuncio(annuncio),
                                                       _linguaPredefinita())}
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

            ' La lingua è dell'opportunità, non della chiamata: l'ha proposta l'analisi e
            ' può averla cambiata l'utente in P6 (cap. 10.1). Qui si legge e basta — così
            ' rigenerare i documenti dopo un ripensamento li rifà nella lingua nuova.
            Dim lingua As String = LinguaDocumenti.PerDocumenti(opportunita.Lingua)

            ' Si riparte da zero: quel che si sta per scrivere non ha ancora un «prima», e
            ' tenersi quello dei documenti di ieri farebbe raccontare a P6 un confronto fra
            ' testi che non sono mai stati uno la rifinitura dell'altro.
            opportunita.PrimaDellaRifinitura = Nothing

            ' Gli appunti di mira, se il brainstorming ce n'è stato (T7c, cap. 12 A6.4):
            ' orientano l'enfasi come fanno annuncio e giudizi, e come loro non aggiungono
            ' un solo fatto. I «fatti nuovi» detti in chat restano fuori — li toglie
            ' AppuntiDiMira, che è il posto dove quella regola è scritta una volta sola.
            Dim appunti As JsonNode = AppuntiDiMira.PerIlPrompt(opportunita.Appunti)

            Annuncia(avanzamento, 3, "Scrivo il CV mirato")
            opportunita.Cv = Await _generatore.GeneraCvMiratoAsync(
                profiloJson, opportunita.Annuncio, giudizi, annulla, lingua, appunti).ConfigureAwait(False)

            Await RifinisciAsync(opportunita, "cv", 4, "Rifinisco il CV",
                                 Function() _rifinitura.DelCvAsync(opportunita.Cv, lingua, annulla),
                                 avanzamento).ConfigureAwait(False)

            ' La lettera riceve il CV **già rifinito**: le serve come riferimento di
            ' coerenza, e dargli quello grezzo vorrebbe dire farle raccontare la stessa
            ' storia con parole che nel CV non ci sono più.
            Annuncia(avanzamento, If(ConRifinitura, 5, 4), "Scrivo la lettera")
            opportunita.Lettera = Await _generatore.GeneraLetteraAsync(
                profiloJson, opportunita.Annuncio, giudizi, opportunita.Cv,
                ElencoMitigazioni(opportunita.Mitigazioni), annulla, lingua, appunti).ConfigureAwait(False)

            Await RifinisciAsync(opportunita, "lettera", 6, "Rifinisco la lettera",
                                 Function() _rifinitura.DellaLetteraAsync(opportunita.Lettera, lingua, annulla),
                                 avanzamento).ConfigureAwait(False)

            ' I documenti ci sono: la candidatura è «generata» (cap. 07.3). Rigenerarli
            ' non è un passaggio nuovo, e la data resta quella della prima volta.
            If opportunita.Stato = StatoOpportunita.Interessante Then
                opportunita.Avanza(StatoOpportunita.Generata)
            End If

        End Function

        ''' <summary>
        ''' Una passata di rifinitura su un documento appena scritto (cap. 08), con la sua
        ''' regola d'oro: <b>non può far fallire la generazione</b>.
        ''' </summary>
        ''' <remarks>
        ''' <para>La rifinitura è un miglioramento facoltativo: se l'AI inciampa proprio lì,
        ''' il documento che resta è quello che ha appena finito di scrivere — buono, solo
        ''' non rifinito. Far cadere l'intera candidatura per questo vorrebbe dire buttare
        ''' via un CV già pronto e chiedere all'utente di rifare tutto da capo, altre
        ''' attese comprese.</para>
        ''' <para>L'annullamento invece passa: <see cref="OperationCanceledException"/> non
        ''' è un inciampo dell'AI, è l'utente che ha premuto Annulla, e assorbirlo qui
        ''' vorrebbe dire continuare a lavorare dopo che ha chiesto di smettere.</para>
        ''' </remarks>
        ''' <param name="quale">Sotto quale nome il «prima» va annotato: <c>cv</c> o <c>lettera</c>.</param>
        ''' <param name="passata">Il lavoro vero; si costruisce solo se la rifinitura c'è.</param>
        Private Async Function RifinisciAsync(opportunita As Opportunita, quale As String,
                                              passo As Integer, cosa As String,
                                              passata As Func(Of Task(Of JsonObject)),
                                              avanzamento As IProgress(Of AvanzamentoPipeline)) As Task

            If _rifinitura Is Nothing Then Return

            Annuncia(avanzamento, passo, cosa)

            Try
                Annota(opportunita, quale, Await passata().ConfigureAwait(False))

            Catch ex As ErroreAi
                ' Non si tace: il passo dice cos'è successo, e il documento resta grezzo.
                Annuncia(avanzamento, passo, $"{cosa}: non ci sono riuscita, tengo il testo com'è")
            End Try

        End Function

        ''' <summary>Mette il «prima» di un documento accanto a quello degli altri.</summary>
        Private Shared Sub Annota(opportunita As Opportunita, quale As String, prima As JsonObject)

            If prima Is Nothing Then Return

            Dim tutti As JsonObject = TryCast(opportunita.PrimaDellaRifinitura, JsonObject)

            If tutti Is Nothing Then
                tutti = New JsonObject()
                opportunita.PrimaDellaRifinitura = tutti
            End If

            tutti(quale) = prima

        End Sub

        ''' <summary>Se questa fila ha una passata anti-slop montata.</summary>
        Private ReadOnly Property ConRifinitura As Boolean
            Get
                Return _rifinitura IsNot Nothing
            End Get
        End Property

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
        ''' La lingua che l'analisi ha riconosciuto nell'annuncio (cap. 10.2), o vuoto.
        ''' </summary>
        ''' <remarks>
        ''' Vuoto non è un guasto: lo restituiscono gli annunci analizzati prima del
        ''' Pool 1.06, quando quel campo non esisteva. È la ragione per cui a decidere cosa
        ''' farne è <see cref="LinguaDocumenti"/> e non questa funzione, che si limita a
        ''' riferire cosa c'era scritto.
        ''' </remarks>
        Private Shared Function LinguaDellAnnuncio(annuncio As JsonNode) As String

            Dim radice As JsonObject = TryCast(annuncio, JsonObject)
            If radice Is Nothing Then Return String.Empty

            Return CampiJson.Testo(radice, "lingua")

        End Function

        ''' <summary>
        ''' Se fra i giudizi c'è almeno un gap da provare a mitigare. Gli esiti che
        ''' contano sono quelli del prompt: «in parte» e «non soddisfatto». «Non
        ''' determinabile» non è una lacuna — non c'era modo di saperlo — e mitigarlo
        ''' sarebbe inventare.
        ''' </summary>
        ''' <remarks>
        ''' Visibile fuori dalla fila perché la usa anche il tool <c>mitiga</c> del server
        ''' MCP (cap. 09.3), dove la mitigazione si chiede da sola invece che come secondo
        ''' tempo del confronto. La regola su quando c'è qualcosa da mitigare dev'essere
        ''' <b>una</b>: due copie finirebbero per rispondere diverso alla stessa domanda.
        ''' </remarks>
        Friend Shared Function CiSonoGap(giudizi As JsonArray) As Boolean

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
        ''' <remarks>Visibile fuori dalla fila per il tool <c>confronta</c>, come <see cref="CiSonoGap"/>.</remarks>
        Friend Shared Function NumeroComplessivo(confronto As JsonNode) As JsonNode

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

        Private Sub Annuncia(avanzamento As IProgress(Of AvanzamentoPipeline),
                             passo As Integer, cosa As String)

            ' Il totale dipende da com'è montata la fila: annunciare «4 di 6» a chi la
            ' rifinitura non ce l'ha vorrebbe dire promettere due passi che non arriveranno.
            avanzamento?.Report(New AvanzamentoPipeline With {
                .Passo = passo,
                .Totale = If(ConRifinitura, PassiTotali, PassiSenzaRifinitura),
                .Cosa = cosa})

        End Sub

    End Class

End Namespace
