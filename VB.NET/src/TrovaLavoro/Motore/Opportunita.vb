Imports System.Text.Json.Nodes
Imports TrovaLavoro.Dati

Namespace Motore

    ''' <summary>
    ''' Una candidatura, tutta insieme: l'annuncio, i giudizi del confronto, il punteggio,
    ''' le mitigazioni, i due documenti generati, e da quale versione di profilo sono
    ''' nati (cap. 02, cap. 11.1). È ciò che <c>Dati/ArchivioOpportunita</c> scrive nella
    ''' sua cartella.
    ''' </summary>
    ''' <remarks>
    ''' <para>Gli artefatti restano <b>JSON grezzo</b> e non diventano classi: a
    ''' differenza del profilo — che P2 edita campo per campo, e per questo è tipizzato —
    ''' questi il programma li riceve dall'AI, li passa al prompt successivo e li scrive
    ''' su disco senza mai entrarci dentro. Tipizzarli vorrebbe dire scrivere e mantenere
    ''' lo stesso schema due volte, in VB e nel prompt, per poi doverli comunque
    ''' riserializzare identici. Le viste tipizzate arrivano dove servono davvero:
    ''' l'impaginazione del CV (T4b) e l'elenco dei giudizi da mostrare (T4c).</para>
    ''' <para>Un'opportunità <b>può essere incompleta</b>, ed è normale: nel flusso reale
    ''' (cap. 12, A5→A7) l'utente guarda il confronto e decide se generare i documenti.
    ''' Fra un passo e l'altro l'opportunità esiste già, con dentro solo ciò che è stato
    ''' fatto finora.</para>
    ''' </remarks>
    Public Class Opportunita

        ''' <summary>L'annuncio strutturato (anello 2).</summary>
        Public Property Annuncio As JsonNode

        ''' <summary>
        ''' L'esito del confronto (anello 3) così come l'AI l'ha dato: la lista
        ''' <c>giudizi</c>, la <c>lettura_insieme</c> e il <c>numero_complessivo</c>.
        ''' </summary>
        Public Property Confronto As JsonNode

        ''' <summary>
        ''' Il punteggio calcolato dai giudizi: è deterministico e si potrebbe
        ''' ricalcolare, ma si conserva perché è il giudizio <b>di quel giorno, con quella
        ''' taratura</b> — quello in base a cui è stata presa la decisione di candidarsi.
        ''' </summary>
        Public Property Match As RisultatoMatch

        ''' <summary>I ponti onesti sui gap. Una lista vuota è un esito legittimo.</summary>
        Public Property Mitigazioni As JsonNode

        ''' <summary>Il 🎯 CV mirato su questo annuncio.</summary>
        Public Property Cv As JsonNode

        ''' <summary>La lettera di presentazione.</summary>
        Public Property Lettera As JsonNode

        ''' <summary>
        ''' Com'erano i testi <b>prima</b> della rifinitura anti-slop (T7b, cap. 08.4):
        ''' <c>{ "cv": { "sommario": "…" }, "lettera": { "corpo": "…" } }</c>. Solo i campi
        ''' davvero cambiati, e <c>Nothing</c> se non ne è cambiato nessuno.
        ''' </summary>
        ''' <remarks>
        ''' <para>Sta qui e non dentro i documenti per una ragione precisa: il 🎯 CV mirato
        ''' viene passato al prompt della lettera e la lettera a quello dell'email, e un
        ''' campo di servizio scritto dentro il documento finirebbe dritto in quelle
        ''' richieste, dove non ha niente da fare.</para>
        ''' <para>Si conservano i testi di prima e non i documenti interi: è ciò che serve
        ''' al prima/dopo di P6 e a tornare indietro, e pesa una frazione.</para>
        ''' </remarks>
        Public Property PrimaDellaRifinitura As JsonNode

        ''' <summary>
        ''' La bozza dell'email: destinatario, oggetto, corpo e allegati scelti (T6,
        ''' cap. 07.1). È l'ultima cosa che l'utente <b>tocca a mano</b> prima di uscire
        ''' dal programma, e per questo si salva: senza, riaprire la candidatura domani
        ''' vorrebbe dire riscrivere il destinatario e rifare le spunte agli allegati.
        ''' </summary>
        ''' <remarks>
        ''' Che ci sia una bozza <b>non</b> vuol dire che sia partita: a spedire è il
        ''' programma di posta dell'utente, e l'unica prova che l'app può avere è la sua
        ''' parola (cap. 07.3). Lo stato «inviata» viene da lì, mai da questo campo.
        ''' </remarks>
        Public Property Email As JsonNode

        ''' <summary>
        ''' La versione di profilo da cui i documenti sono nati (il nome che
        ''' <c>ArchivioProfilo.Salva</c> restituisce): è ciò che tiene spiegabile un CV
        ''' già inviato anche a profilo evoluto (cap. 11.1).
        ''' </summary>
        Public Property VersioneProfilo As String

        ''' <summary>
        ''' La lingua dei documenti. A T4 è sempre l'italiano: il campo <c>lingua</c>
        ''' dell'annuncio e le varianti <c>en</c> dei prompt arrivano con T7 (cap. 10).
        ''' </summary>
        Public Property Lingua As String = "it"

        ''' <summary>
        ''' Da dove viene l'annuncio: il nome del portale se è uno di quelli conosciuti,
        ''' altrimenti il sito (cap. 06.4; cap. 12, A4). Vuoto per un annuncio incollato a
        ''' mano, che una provenienza non ce l'ha.
        ''' </summary>
        Public Property Fonte As String

        ''' <summary>
        ''' L'indirizzo della pagina da cui l'annuncio è stato catturato. È ciò che
        ''' permette di tornare all'originale mesi dopo — quando l'annuncio non c'è più,
        ''' resta almeno la prova di dove stava.
        ''' </summary>
        Public Property Link As String

        ''' <summary>Quando l'opportunità è nata e quando è stata toccata l'ultima volta.</summary>
        Public Property Creata As Date
        Public Property Aggiornata As Date

        ''' <summary>
        ''' A che punto è la candidatura (cap. 07.3). Si cambia con <see cref="Avanza"/>,
        ''' che è l'unico a conoscere le strade lecite; il valore si assegna direttamente
        ''' solo quando lo si <b>rilegge</b> da disco, dove il passaggio è già avvenuto.
        ''' </summary>
        Public Property Stato As StatoOpportunita = StatoOpportunita.Nuova

        ''' <summary>
        ''' Quando l'opportunità è entrata in ciascuno stato: è la storia che il registro
        ''' racconta («inviata il 12/08, nessun esito registrato», cap. 07.3).
        ''' </summary>
        ''' <remarks>
        ''' Uno stato per volta, perché la macchina non torna mai indietro: la data qui
        ''' dentro è quella del <b>primo</b> ingresso, e ripassare dallo stesso stato non
        ''' la riscrive. L'ingresso in <see cref="StatoOpportunita.Nuova"/> coincide con
        ''' <see cref="Creata"/>, che è scritta comunque.
        ''' </remarks>
        Public ReadOnly Property DateStati As New Dictionary(Of StatoOpportunita, Date)

        ''' <summary>
        ''' Dove è stata scritta su disco; <c>Nothing</c> finché non lo è. Lo riempie
        ''' l'archivio, che è l'unico a sapere come si chiama la cartella.
        ''' </summary>
        Public Property Cartella As String

        ''' <summary>Il nome di chi offre il posto, dall'annuncio; vuoto se anonimo.</summary>
        Public ReadOnly Property Azienda As String
            Get
                Return DallAnnuncio("azienda")
            End Get
        End Property

        ''' <summary>Il ruolo, dall'annuncio.</summary>
        Public ReadOnly Property Titolo As String
            Get
                Return DallAnnuncio("titolo")
            End Get
        End Property

        ''' <summary>
        ''' Se l'annuncio è uscito <b>vuoto</b>: è così che il prompt <c>analisi_annuncio</c>
        ''' dice «questo testo non è un annuncio di lavoro» — una pagina di elenco, una
        ''' home, una schermata di accesso (cap. 06.4).
        ''' </summary>
        ''' <remarks>
        ''' <para>La domanda si fa <b>conservativa</b>: vuoto vuol dire che non c'è né il
        ''' ruolo, né una mansione, né uno solo dei quattro gruppi di requisiti. Basta una
        ''' di quelle cose perché l'annuncio valga la pena — un annuncio scarno esiste, e
        ''' rifiutarlo sarebbe peggio che analizzarlo. Il rischio si sceglie da questa
        ''' parte: meglio un'analisi in più che una cattura buona buttata via.</para>
        ''' <para>Non guarda azienda, sede, contratto e benefit: sono contorno, e una
        ''' pagina di elenco quelli può averli (il nome del portale, la città della
        ''' ricerca) proprio mentre di annuncio non ne ha nessuno.</para>
        ''' </remarks>
        Public ReadOnly Property AnnuncioVuoto As Boolean
            Get
                If Annuncio Is Nothing Then Return True

                If Not String.IsNullOrWhiteSpace(Titolo) Then Return False

                For Each lista As String In {"competenze_richieste", "esperienza_richiesta",
                                             "formazione_richiesta", "altri_requisiti", "mansioni"}
                    If Not ListaVuota(lista) Then Return False
                Next

                Return True
            End Get
        End Property

        ''' <summary>
        ''' Se il confronto è stato fatto: è la domanda che decide se si può passare alla
        ''' generazione (cap. 12, A6).
        ''' </summary>
        Public ReadOnly Property Confrontata As Boolean
            Get
                Return Giudizi() IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Porta la candidatura allo stato successivo, annotando quando (cap. 07.3).
        ''' </summary>
        ''' <param name="nuovo">Dove si va; dev'essere una strada lecita da dove si è.</param>
        ''' <param name="quando">L'istante del passaggio; se omesso, adesso.</param>
        ''' <remarks>
        ''' <para><b>Restare dov'è non è un errore</b>: rigenerare i documenti di
        ''' un'opportunità già generata è una cosa che si fa, e non è un passaggio nuovo —
        ''' la data del primo ingresso resta quella. Si annota però se mancava, ed è così
        ''' che una candidatura appena nata prende la data della sua prima casella.</para>
        ''' <para>Una strada che la macchina non prevede solleva: non è un caso d'uso
        ''' dell'utente, è un errore di chi programma. L'utente non può chiedere una
        ''' transizione illecita — i comandi che non si possono premere sono spenti
        ''' (cap. 03.8).</para>
        ''' </remarks>
        Public Sub Avanza(nuovo As StatoOpportunita, Optional quando As Date = Nothing)

            Dim istante As Date = If(quando = Nothing, Date.Now, quando)

            If nuovo = Stato Then
                If Not DateStati.ContainsKey(nuovo) Then DateStati(nuovo) = istante
                Return
            End If

            If Not StatiOpportunita.Consentita(Stato, nuovo) Then
                Throw New InvalidOperationException(
                    $"Da «{StatiOpportunita.Nome(Stato)}» non si passa a " &
                    $"«{StatiOpportunita.Nome(nuovo)}» (cap. 07.3).")
            End If

            Stato = nuovo
            DateStati(nuovo) = istante

        End Sub

        ''' <summary>
        ''' La sola lista dei giudizi, che è ciò che i prompt successivi ricevono;
        ''' <c>Nothing</c> se il confronto non c'è o non ha la forma attesa.
        ''' </summary>
        Public Function Giudizi() As JsonArray

            If Confronto Is Nothing Then Return Nothing
            Dim oggetto As JsonObject = TryCast(Confronto, JsonObject)
            If oggetto Is Nothing Then Return Nothing

            Dim voci As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue("giudizi", voci) Then Return Nothing

            Return TryCast(voci, JsonArray)

        End Function

        ''' <summary>
        ''' Se una lista dell'annuncio è assente o senza voci. Un campo che non è una lista
        ''' conta come vuoto: l'AI ha risposto fuori schema, e su una forma che non
        ''' riconosciamo non si può dire che ci sia qualcosa.
        ''' </summary>
        Private Function ListaVuota(campo As String) As Boolean

            Dim oggetto As JsonObject = TryCast(Annuncio, JsonObject)
            If oggetto Is Nothing Then Return True

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, valore) Then Return True

            Dim voci As JsonArray = TryCast(valore, JsonArray)
            Return voci Is Nothing OrElse voci.Count = 0

        End Function

        ''' <summary>Un campo di testo dell'annuncio, vuoto se manca o non è un testo.</summary>
        Private Function DallAnnuncio(campo As String) As String

            Dim oggetto As JsonObject = TryCast(Annuncio, JsonObject)
            If oggetto Is Nothing Then Return String.Empty

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, valore) OrElse valore Is Nothing Then
                Return String.Empty
            End If

            Return If(valore.GetValueKind() = Text.Json.JsonValueKind.String,
                      valore.GetValue(Of String)(), String.Empty)

        End Function

    End Class

End Namespace
