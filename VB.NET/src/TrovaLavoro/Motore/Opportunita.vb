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

        ''' <summary>
        ''' Gli appunti di mira confermati dall'utente nel brainstorming (T7c, cap. 12
        ''' A6.4): <c>{ "appunti": [ { "tipo", "testo", "da" } ], "fatti_nuovi": [ … ] }</c>.
        ''' <c>Nothing</c> finché non se n'è mai parlato.
        ''' </summary>
        ''' <remarks>
        ''' Orientano la generazione e non aggiungono fatti: al prompt del 🎯 CV mirato e
        ''' a quello della lettera arriva la sola lista <c>appunti</c>, mai i
        ''' <c>fatti_nuovi</c> — quelli sono cose dette in chat che nel profilo non
        ''' risultano, e nei documenti non devono entrare da questa porta
        ''' (<see cref="AppuntiDiMira"/>).
        ''' </remarks>
        Public Property Appunti As JsonNode

        ''' <summary>Il 🎯 CV mirato su questo annuncio.</summary>
        Public Property Cv As JsonNode

        ''' <summary>La lettera di presentazione.</summary>
        Public Property Lettera As JsonNode

        ''' <summary>
        ''' I campi di prosa che l'utente ha riscritto <b>a mano</b> nel 🎯 CV mirato e
        ''' nella ✉️ lettera (R7, 2026-08-23): quel che di questi documenti non l'ha
        ''' scritto l'AI.
        ''' </summary>
        ''' <remarks>
        ''' Vivono qui, e da qui vanno nello <c>stato.json</c>, perché sono una proprietà
        ''' <b>del documento</b> e non della sessione: il pannello che li mostrava se ne
        ''' dimenticava al primo rientro, e con lui se ne dimenticava l'avviso di
        ''' «Rigenera» (v. <see cref="RiscrittureAMano"/>).
        ''' </remarks>
        Public ReadOnly Property RiscrittureDelCv As New RiscrittureAMano

        ''' <inheritdoc cref="RiscrittureDelCv"/>
        Public ReadOnly Property RiscrittureDellaLettera As New RiscrittureAMano

        ''' <summary>
        ''' Quando la ✉️ lettera è stata scritta dall'AI l'ultima volta; la data vuota se
        ''' non è mai stata scritta o se il file viene da prima di R7.
        ''' </summary>
        ''' <remarks>
        ''' Non è la data di <see cref="Aggiornata"/>, che si muove a ogni salvataggio:
        ''' serve a rispondere a una domanda sola — la lettera che c'è ha visto o no quel
        ''' che l'utente ha riscritto nel CV (<see cref="LetteraDaRiallineare"/>). La
        ''' scrive chi la lettera la genera, cioè la pipeline, che è il posto unico da cui
        ''' esce (cap. 02).
        ''' </remarks>
        Public Property LetteraGenerata As Date


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
        ''' Com'è finita, quando lo si sa (cap. 07.3); <c>Nothing</c> finché nessuno l'ha
        ''' registrato — ed è il silenzio da cui il promemoria di follow-up capisce che
        ''' quella candidatura sta ancora aspettando.
        ''' </summary>
        ''' <remarks>
        ''' Ha senso solo dentro lo stato <see cref="StatoOpportunita.Esito"/>, e a
        ''' tenerli d'accordo è <see cref="SegnaEsito"/>: come per lo stato, il valore si
        ''' assegna direttamente solo quando lo si <b>rilegge</b> da disco.
        ''' </remarks>
        Public Property Esito As EsitoCandidatura?

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
        ''' Se la ✉️ lettera non ha ancora visto quel che l'utente ha riscritto a mano nel
        ''' 🎯 CV: è la spia di disallineamento che P6 accende (R7, cap. 08.4).
        ''' </summary>
        ''' <remarks>
        ''' <para>La regola sta qui e in nessun altro posto, perché è una sola e va detta
        ''' una volta: c'è una lettera, nel CV c'è del testo scritto a mano, e la lettera è
        ''' <b>più vecchia</b> di quel testo. Il verso conta: il CV racconta la storia, la
        ''' lettera la ripete — riscrivere la lettera non disallinea il CV.</para>
        ''' <para>Una lettera senza data — nata prima di R7, o arrivata da
        ''' <c>salva_opportunita</c> (cap. 09.3) — vale come «non sappiamo quando»: se nel
        ''' CV ci sono riscritture, la spia si accende. È il verso prudente, e costa un
        ''' avviso in più a chi non ne aveva bisogno; l'altro costerebbe una lettera che
        ''' dice ancora «trasloco» senza che nessuno lo dica.</para>
        ''' </remarks>
        Public ReadOnly Property LetteraDaRiallineare As Boolean
            Get

                If Lettera Is Nothing Then Return False
                If Not RiscrittureDelCv.CEQualcosa Then Return False

                Return LetteraGenerata = Nothing OrElse LetteraGenerata < RiscrittureDelCv.Quando

            End Get
        End Property

        ''' <summary>Le riscritture a mano di uno dei due documenti.</summary>
        Public Function Riscritture(ruolo As RuoloDocumento) As RiscrittureAMano

            Return If(ruolo = RuoloDocumento.Lettera, RiscrittureDellaLettera, RiscrittureDelCv)

        End Function

        ''' <summary>
        ''' Annota che l'utente ha riscritto a mano dei campi di prosa in uno dei due
        ''' documenti (R7).
        ''' </summary>
        ''' <param name="ruolo">Quale documento ha toccato.</param>
        ''' <param name="campi">Gli id dei campi riscritti, come li conosce <c>Rifinitura</c>.</param>
        ''' <param name="quando">L'istante della riscrittura; se omesso, adesso.</param>
        Public Sub SegnaRiscritture(ruolo As RuoloDocumento, campi As IEnumerable(Of String),
                                    Optional quando As Date = Nothing)

            If campi Is Nothing Then Return

            Dim istante As Date = If(quando = Nothing, Date.Now, quando)
            Dim dove As RiscrittureAMano = Riscritture(ruolo)

            For Each id As String In campi
                dove.Annota(id, istante)
            Next

        End Sub

        ''' <summary>
        ''' Annota che la ✉️ lettera è stata appena scritta dall'AI: da questo istante ha
        ''' visto il CV com'è adesso, e la spia di disallineamento si spegne.
        ''' </summary>
        ''' <remarks>
        ''' Le riscritture a mano <b>della lettera</b> se ne vanno qui, e non altrove: il
        ''' testo che l'utente ci aveva scritto non c'è più, l'ha sostituito quello nuovo.
        ''' Quelle del CV invece restano — il CV nessuno l'ha toccato.
        ''' </remarks>
        Public Sub SegnaLetteraGenerata(Optional quando As Date = Nothing)

            LetteraGenerata = If(quando = Nothing, Date.Now, quando)
            RiscrittureDellaLettera.Dimentica()

        End Sub

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
        ''' Registra com'è finita, o toglie l'esito registrato per sbaglio (cap. 07.3).
        ''' </summary>
        ''' <param name="scelto">
        ''' L'esito da segnare; <c>Nothing</c> per toglierlo, e allora la candidatura
        ''' torna a essere <see cref="StatoOpportunita.Inviata"/>, cioè in attesa.
        ''' </param>
        ''' <param name="quando">L'istante in cui lo si registra; se omesso, adesso.</param>
        ''' <remarks>
        ''' <para><b>Perché non basta <see cref="Avanza"/>.</b> L'esito è la sola parte
        ''' del ciclo di vita che si <b>corregge</b>: non è un fatto che il programma
        ''' osserva, è una dichiarazione di chi si è candidato (cap. 07.3), e una
        ''' dichiarazione sbagliata si rimangia. Perciò da qui si passa da «rifiutata» a
        ''' «colloquio» e si torna anche indietro a «inviata», due strade che la macchina
        ''' degli stati non prevede — e che non deve prevedere, perché per tutto il resto
        ''' vale ancora che indietro non si torna.</para>
        ''' <para><b>La data è quella dell'ultimo esito registrato</b>, ed è l'unica
        ''' eccezione alla regola di <see cref="DateStati"/> (dove vale il primo
        ''' ingresso): una candidatura entrata «in colloquio» a settembre e diventata
        ''' «assunto» a novembre, raccontata con la data di settembre, direbbe una cosa
        ''' falsa proprio nel punto in cui la storia è finita.</para>
        ''' <para><b>Restare senza esito dove non ce n'era</b> non è un errore e non fa
        ''' niente: è il caso di chi apre il menù e riconferma «in attesa».</para>
        ''' </remarks>
        Public Sub SegnaEsito(scelto As EsitoCandidatura?, Optional quando As Date = Nothing)

            Dim istante As Date = If(quando = Nothing, Date.Now, quando)

            If Not scelto.HasValue Then
                If Stato <> StatoOpportunita.Esito Then Return

                Stato = StatoOpportunita.Inviata
                Esito = Nothing
                DateStati.Remove(StatoOpportunita.Esito)
                Return
            End If

            If Stato <> StatoOpportunita.Inviata AndAlso Stato <> StatoOpportunita.Esito Then
                Throw New InvalidOperationException(
                    $"Una candidatura «{StatiOpportunita.Nome(Stato)}» non ha ancora un esito: " &
                    "prima va spedita (cap. 07.3).")
            End If

            If Stato = StatoOpportunita.Inviata Then Avanza(StatoOpportunita.Esito, istante)

            Esito = scelto
            DateStati(StatoOpportunita.Esito) = istante

        End Sub

        ''' <summary>
        ''' La sola lista dei giudizi, che è ciò che i prompt successivi ricevono;
        ''' <c>Nothing</c> se il confronto non c'è o non ha la forma attesa.
        ''' </summary>
        Public Function Giudizi() As JsonArray
            Return GiudiziDi(Confronto)
        End Function

        ''' <summary>
        ''' La lista dei giudizi dentro un confronto qualunque, anche di un confronto che
        ''' non appartiene a nessuna opportunità: è il caso del server MCP, dove
        ''' <c>confronta</c> è una porta a sé e non il passo di una candidatura
        ''' (cap. 09.3).
        ''' </summary>
        ''' <returns><c>Nothing</c> se il confronto non c'è o non ha la forma attesa.</returns>
        Public Shared Function GiudiziDi(confronto As JsonNode) As JsonArray

            If confronto Is Nothing Then Return Nothing
            Dim oggetto As JsonObject = TryCast(confronto, JsonObject)
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
