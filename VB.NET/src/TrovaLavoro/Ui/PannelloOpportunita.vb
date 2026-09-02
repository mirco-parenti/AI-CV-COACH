Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

''' <summary>
''' Pannello P4 — l'opportunità (cap. 03.6; cap. 12, A5): l'annuncio come l'AI l'ha
''' capito, il match in stelle, i giudizi voce per voce e le note che li spiegano.
''' </summary>
''' <remarks>
''' <para><b>La fascia d'ingresso</b> in cima è la porta da cui un annuncio entra: fino a
''' T5 è l'unica, e anche dopo resta la strada di chi ha in mano un testo e basta
''' (cap. 12.3). A confronto avvenuto si richiude, per lasciare spazio ai giudizi.</para>
''' <para><b>«Analizza» fa due passi, non uno.</b> Fra l'analisi dell'annuncio e il
''' confronto l'utente non deve decidere niente: la decisione vera — generare o no i
''' documenti — viene dopo, quando le stelle si vedono (cap. 12, A5→A7). I due passi si
''' chiamano perciò di fila, e l'attesa dice a che punto è.</para>
''' <para><b>Qui non si giudica.</b> I giudizi sono dell'AI, il punteggio è di
''' <see cref="CalcoloMatch"/>, e ciò che si mostra è la vista di sola lettura
''' <see cref="VistaConfronto"/>: il pannello disegna, non decide.</para>
''' </remarks>
Public Class PannelloOpportunita
    Implements IPannelloArea

    ''' <summary>Sotto questa altezza la fascia delle azioni non scende: i bottoni ci devono stare.</summary>
    Private Const AltezzaMinimaAzioni As Integer = 60

    ''' <summary>Quanto spazio si prende il logo flottante (cap. 03.5).</summary>
    Private _ingombroLogo As Size

    ''' <summary>
    ''' La fascia dei comandi in fondo (cap. 03.4). Nasce alla prima disposizione e non nel
    ''' costruttore: i bottoni che le si dichiarano esistono solo dopo
    ''' <c>InitializeComponent</c>.
    ''' </summary>
    Private _comandi As FasciaDeiComandi

    ''' <summary>Quante stelle ha la scala: quelle piene e quelle vuote insieme.</summary>
    Private Const StelleDellaScala As Integer = 5

    Private ReadOnly _suggerimenti As New ToolTip()

    ''' <summary>
    ''' Il menù di «Com'è andata…» (cap. 07.3). Nasce qui e non nel Designer, come il
    ''' <see cref="_suggerimenti"/>: le sue voci sono quelle dell'enum
    ''' <see cref="EsitoCandidatura"/>, e un elenco che si ricopia a mano in un file
    ''' generato è un elenco che prima o poi diverge.
    ''' </summary>
    Private ReadOnly _menuEsito As New ContextMenuStrip()

    Private _contesto As ContestoApp

    ''' <summary>
    ''' Chi conduce i passi. Di norma è quella del motore; il banco ne passa una coi
    ''' mestieri finti e fa girare il pannello senza rete.
    ''' </summary>
    Private _pipeline As PipelineCandidatura

    ''' <summary>La candidatura in mostra; <c>Nothing</c> finché non se n'è analizzata una.</summary>
    Private _opportunita As Opportunita

    ''' <summary>Il confronto come lo si disegna; <c>Nothing</c> se non c'è ancora.</summary>
    Private _vista As VistaConfronto

    ''' <summary>Il filo per annullare l'attesa; <c>Nothing</c> se non c'è niente in volo.</summary>
    Private _annulla As CancellationTokenSource

    ''' <summary>
    ''' Se la fascia d'ingresso è aperta. È un campo e non si legge da
    ''' <c>pnlIngresso.Visible</c>: in WinForms la visibilità di un figlio è <b>falsa</b>
    ''' finché il pannello che lo contiene non è a sua volta mostrato — e i pannelli
    ''' dell'area centrale nascono tutti nascosti (cap. 03.4). Chiedendolo al controllo,
    ''' all'avvio la fascia risultava chiusa mentre era spalancata, e «Nuovo annuncio»
    ''' compariva acceso quando non aveva niente da riaprire.
    ''' </summary>
    Private _fasciaAperta As Boolean = True

    ''' <summary>
    ''' Se l'attesa in corso è un <b>riconfronto</b>, e non una delle altre.
    ''' </summary>
    ''' <remarks>
    ''' Serve a una cosa sola: tenere a video «⚠ Riconfronta» mentre il lavoro che ha
    ''' chiesto è in volo, perché faccia da annulla. Non si può dedurre da
    ''' <see cref="DaRiconfrontare"/> — quella resta vera per tutta la durata della chiamata,
    ''' e direbbe di sì anche per un'attesa cominciata da un altro comando.
    ''' </remarks>
    Private _riconfrontoInVolo As Boolean

    ''' <summary>
    ''' L'utente chiede i documenti per l'opportunità in mostra: la finestra porta in
    ''' vista P6. Il pannello non conosce gli altri pannelli — dice cosa vuole.
    ''' </summary>
    Public Event DocumentiRichiesti As EventHandler

    ''' <summary>
    ''' L'utente vuole ragionare su questa candidatura prima di generare: la finestra
    ''' porta in vista P5, in modalità brainstorming (T7c, cap. 12 A6).
    ''' </summary>
    Public Event BrainstormRichiesto As EventHandler

    ''' <summary>
    ''' L'AI ha cominciato o finito di lavorare: «mentre l'AI lavora non si esce» vale
    ''' anche per la barra di navigazione, che questo pannello non può spegnere da sé
    ''' (cap. 02.6).
    ''' </summary>
    Public Event LavoroAiCambiato As EventHandler

    Public Sub New()

        InitializeComponent()

        ' Come in P2: il ToolTip nasce qui e muore col pannello, invece di restare un
        ' componente orfano nel designer.
        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        VestiIBottoni()
        SpiegaIlRagionamento()
        MostraLaValutazione(Nothing)
        AggiornaComandi()

    End Sub

    ''' <summary>Collega il pannello al motore e dice da dove si comincia.</summary>
    ''' <param name="contesto">Il motore montato all'avvio.</param>
    ''' <param name="pipeline">
    ''' Chi conduce i passi. Di norma si omette ed è quella del motore; il banco passa qui
    ''' la sua, coi mestieri finti, e fa girare il pannello senza rete.
    ''' </param>
    Public Sub Collega(contesto As ContestoApp, Optional pipeline As PipelineCandidatura = Nothing)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))

        _contesto = contesto
        _pipeline = If(pipeline, contesto.Pipeline)

        RaccontaDaDoveSiComincia()
        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Ogni volta che il pannello entra in vista torna a chiedere com'è messo il mondo.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Collega"/> passa una volta sola, all'avvio, e lì il profilo poteva non
    ''' esserci ancora. Ma il primo giro vero è proprio quello: si importa il CV in P2, lo
    ''' si salva, e si viene qui — dove fino a ieri si leggeva «prima serve il tuo profilo»
    ''' con «Analizza» spento, per un profilo appena salvato. La condizione la sa
    ''' l'archivio, che guarda il disco a ogni domanda: bastava tornare a chiedergliela.
    ''' Se un'opportunità è già in mostra, il racconto non si tocca: quella riga dice a che
    ''' punto è il confronto, e riscriverla come se si ricominciasse sarebbe una bugia.
    ''' </remarks>
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)

        MyBase.OnVisibleChanged(e)

        If Not Visible OrElse _contesto Is Nothing Then Return

        If _opportunita Is Nothing Then RaccontaDaDoveSiComincia()
        AggiornaComandi()

    End Sub

    ''' <summary>L'opportunità in mostra; <c>Nothing</c> se non ce n'è ancora una.</summary>
    Public ReadOnly Property Candidatura As Opportunita
        Get
            Return _opportunita
        End Get
    End Property

    ''' <summary>Se in questo momento una chiamata all'AI è in volo.</summary>
    Public ReadOnly Property AiAlLavoro As Boolean
        Get
            Return _annulla IsNot Nothing
        End Get
    End Property

    ''' <summary>Annulla l'attesa in corso, se c'è: è la via pulita della chiusura.</summary>
    Public Sub AnnullaIlLavoro()
        _annulla?.Cancel()
    End Sub

    ' ==================================================================
    ' I due passi: leggere l'annuncio, confrontarlo col profilo
    ' ==================================================================

    ''' <summary>
    ''' Legge l'annuncio incollato e lo confronta col profilo. È il metodo che preme
    ''' «Analizza»; il banco lo chiama direttamente, perché di un gestore di clic non si
    ''' può aspettare la fine.
    ''' </summary>
    Public Async Function AnalizzaLAnnuncioAsync() As Task

        If AiAlLavoro Then Return

        ' Da T9c. La candidatura riaperta al solo annuncio salta il primo passo: l'annuncio
        ' è già letto e strutturato, e rileggerlo costerebbe una chiamata per riottenere
        ' quello che c'è già. Il riconfronto è lo stesso secondo passo su una candidatura che
        ' il primo l'aveva già fatto, ma non passa più di qui: ha il suo bottone
        ' (btnRiconfronta), perché questo vive nella fascia d'ingresso ed è chiusa proprio
        ' quando servirebbe (v. DaRiconfrontare).
        If DaConfrontare() Then
            Await ConfrontaLaRiapertaAsync().ConfigureAwait(True)
            Return
        End If

        Dim testo As String = txtAnnuncio.Text.Trim()
        If testo = "" Then
            RaccontaLoStato("Incolla il testo dell'annuncio, poi premi «Analizza».", StileApp.TestoSecondario)
            Return
        End If

        ' Un testo incollato non ha provenienza: arriva da un'email, da un messaggio, da
        ' uno screenshot letto altrove. Fonte e link restano vuoti, e va bene così.
        Await AvviaIPassiAsync(testo, fonte:=Nothing, link:=Nothing).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' L'annuncio catturato dalla pagina che l'utente stava guardando (cap. 06.4; cap. 12,
    ''' A4): stessa strada del testo incollato, con in più la provenienza.
    ''' </summary>
    ''' <remarks>
    ''' Il testo <b>si vede</b>: entra nella casella della fascia d'ingresso, dove l'utente
    ''' può leggerlo, correggerlo e rilanciare. È la stessa onestà con cui il resto del
    ''' programma mostra ciò che manda all'AI — una cattura che analizzasse in silenzio
    ''' qualcosa di invisibile chiederebbe di fidarsi al buio.
    ''' </remarks>
    Public Async Function AnalizzaIlCatturatoAsync(testo As String, fonte As String, link As String) As Task

        If AiAlLavoro Then Return

        FasciaDIngresso(aperta:=True)

        ' Nella casella ci va con gli a capo di Windows: il lettore di pagine cuce i pezzi
        ' con \n (cap. 06.4) e una casella multiriga i \n non li mostra — la pagina intera
        ' comparirebbe in un blocco unico, illeggibile proprio dove si promette all'utente
        ' che potrà rileggerla e correggerla. All'AI e su disco il testo va come sta.
        txtAnnuncio.Text = TestoDaMostrare.ConGliACapoDiWindows(testo)

        Await AvviaIPassiAsync(If(testo, String.Empty).Trim(), fonte, link).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' Le condizioni che valgono comunque — l'AI, il profilo — e poi i due passi.
    ''' </summary>
    Private Async Function AvviaIPassiAsync(testo As String, fonte As String, link As String) As Task

        If testo = "" Then Return

        If _pipeline Is Nothing Then
            RaccontaUnAvviso(MotivoSenzaAi())
            Return
        End If

        Dim profilo As Profilo = LeggiIlProfilo()
        If profilo Is Nothing Then Return

        Await ConducoIDuePassiAsync(_pipeline, testo, profilo, fonte, link).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' I due passi con la loro attesa: l'annuncio, poi il confronto. Ogni inciampo si
    ''' racconta dov'è successo, e quel che si è già ottenuto non si butta.
    ''' </summary>
    Private Async Function ConducoIDuePassiAsync(pipeline As PipelineCandidatura,
                                                 testo As String, profilo As Profilo,
                                                 fonte As String, link As String) As Task

        Using filo As New CancellationTokenSource()

            _annulla = filo
            LavoroInCorso(True)

            Try
                RaccontaLoStato("Leggo l'annuncio… (1 di 2)", StileApp.TestoSecondario)
                Dim candidatura As Opportunita = Await pipeline.AnalizzaAsync(testo, filo.Token).ConfigureAwait(True)

                candidatura.Fonte = fonte
                candidatura.Link = link

                ' Lo schema vuoto è il modo in cui il prompt dice «questo non è un
                ' annuncio» (cap. 06.4): ci si ferma qui, senza pagare il confronto e
                ' senza scrivere su disco una candidatura che non esiste. Il testo resta
                ' nella casella, che è da dove si riprova.
                If candidatura.AnnuncioVuoto Then
                    RaccontaLoStato(NonSembraUnAnnuncio(link), StileApp.TestoSecondario)
                    Return
                End If

                RaccontaLoStato("Confronto l'annuncio con il tuo profilo… (2 di 2)", StileApp.TestoSecondario)
                Await pipeline.ConfrontaAsync(candidatura, profilo, filo.Token).ConfigureAwait(True)

                candidatura.VersioneProfilo = VersioneInUso()

                _opportunita = candidatura
                MostraLOpportunita()
                FasciaDIngresso(aperta:=False)

                RaccontaLoStato(Archivia(candidatura, RiassuntoDelMatch()), StileApp.TestoSecondario)

            Catch ex As OperationCanceledException
                RaccontaLoStato("Analisi annullata: non ho scritto niente.", StileApp.TestoSecondario)

            Catch ex As ErroreAi
                ' Il messaggio è già quello da mostrare, e il testo incollato resta dov'è:
                ' si riprova senza doverlo ritrovare.
                RaccontaUnErrore(ex.Message & vbLf & "Il testo dell'annuncio è ancora qui: puoi riprovare.")

            Finally
                _annulla = Nothing
                LavoroInCorso(False)
            End Try

        End Using

    End Function

    ''' <summary>
    ''' Se quel che manca a questa candidatura è <b>solo</b> il confronto: allora
    ''' «Analizza» cambia mestiere e diventa «Confronta» (cap. 07.3).
    ''' </summary>
    ''' <remarks>
    ''' <para>È il vicolo cieco trovato dal collaudo di tappa di T8 (2026-08-21). Una
    ''' candidatura ferma allo stato <see cref="StatoOpportunita.Nuova"/> — oggi le sa
    ''' creare solo il server MCP, perché l'applicazione archivia dopo il confronto — si
    ''' riapriva e non si poteva proseguire: la casella dell'incolla è vuota, e «Analizza»
    ''' si accende solo se lì dentro c'è del testo. Il motore sapeva già farlo, perché
    ''' <c>ConfrontaAsync</c> vuole l'annuncio <b>già strutturato</b>: mancava il gesto.</para>
    ''' <para><b>Il testo incollato ha la precedenza.</b> Chi scrive qualcosa nella casella
    ''' vuole leggere un annuncio nuovo, non ripescare quello di prima: allora il bottone
    ''' torna a essere «Analizza» e fa i suoi due passi su una candidatura nuova.</para>
    ''' </remarks>
    Private Function DaConfrontare() As Boolean

        If _opportunita Is Nothing OrElse _opportunita.Confrontata Then Return False
        If _opportunita.Stato = StatoOpportunita.Scartata Then Return False
        If _opportunita.AnnuncioVuoto Then Return False

        Return txtAnnuncio.Text.Trim() = ""

    End Function

    ''' <summary>
    ''' Che cosa non torna fra questa candidatura e il profilo di <b>oggi</b>, o
    ''' <c>Nothing</c> se sono in pari.
    ''' </summary>
    ''' <remarks>
    ''' <para>I casi sono due e non vanno confusi, perché uno è più grave dell'altro. Il
    ''' profilo di allora può essere <b>sparito</b> — eliminato e rifatto, cap. 11.5 — e
    ''' allora i giudizi raccontano un'altra persona; oppure può essere <b>cresciuto</b>, e
    ''' allora raccontano la stessa persona di ieri. Si guarda prima lo sparito: quando una
    ''' versione non c'è più anche <see cref="Dati.ArchivioProfilo.CambiatoDopo"/> risponde
    ''' di sì, e il caso grave verrebbe raccontato con le parole di quello lieve.</para>
    ''' <para><b>Perché il caso «cresciuto» adesso si dice.</b> Per un anno intero il
    ''' programma ha taciuto: dirlo a ogni salvataggio del profilo sarebbe stato un avviso a
    ''' ogni giro, per un caso in cui i documenti restano spiegabili. Ma un salvataggio può
    ''' cambiare un <b>requisito eliminatorio</b> — la patente è quello che l'ha fatto
    ''' vedere — e allora le stelle a video non sono vecchie: sono <b>un'altra risposta</b>,
    ''' e l'utente le legge per decidere se candidarsi. Un punteggio sbagliato che tace è
    ''' peggio di un avviso di troppo.</para>
    ''' <para>Una candidatura senza versione annotata non è disallineata: non si sa da dove
    ''' venga, e di un dubbio non si fa un allarme (v. <c>CELaVersione</c>, che di una
    ''' versione vuota risponde di sì).</para>
    ''' </remarks>
    Private Function ProfiloDisallineato() As String

        If _opportunita Is Nothing OrElse _contesto Is Nothing Then Return Nothing
        If Not _opportunita.Confrontata Then Return Nothing

        Dim versione As String = _opportunita.VersioneProfilo

        If Not _contesto.Archivio.CELaVersione(versione) Then
            Return "Questa candidatura è stata confrontata con un profilo che adesso non c'è " &
                   "più: quello di allora è stato eliminato, e le stelle e i giudizi che vedi " &
                   "raccontano ancora lui."
        End If

        If _contesto.Archivio.CambiatoDopo(versione) Then
            Return "Hai cambiato il profilo dopo questo confronto: le stelle e i giudizi che " &
                   "vedi sono quelli di allora, e se è cambiato un requisito eliminatorio — la " &
                   "patente, un titolo, una disponibilità — il punteggio di oggi sarebbe un altro."
        End If

        Return Nothing

    End Function

    ''' <summary>
    ''' Accende la spia accanto alle stelle: se il punteggio a video è ancora la risposta
    ''' del profilo di oggi.
    ''' </summary>
    ''' <remarks>
    ''' <para>Questo pannello lo <b>diceva già</b>, dal 2026-09-02: nella riga di stato in
    ''' alto a destra, in una finestra alla riapertura, e nel bottone «Riconfronta». Tre modi
    ''' che hanno tutti lo stesso limite — arrivano una volta, e chi li ha già letti o chiusi
    ''' torna a guardare le stelle senza più niente accanto. La spia non annuncia:
    ''' <b>resta</b>, ed è la differenza fra un avviso e un'etichetta.</para>
    ''' <para>Il colore da solo non basta e qui meno che mai (cap. 03.8): la parola c'è
    ''' sempre, e il perché sta nel suggerimento — con la distinzione fra il profilo
    ''' cresciuto e quello rifatto da capo, che <see cref="ProfiloDisallineato"/> continua a
    ''' fare per i suoi messaggi lunghi.</para>
    ''' </remarks>
    Private Sub MostraLaSpiaDelProfilo()

        Dim spia As LetturaSpia = SpiaDelProfilo.Spenta

        If _opportunita IsNot Nothing AndAlso _contesto IsNot Nothing Then
            spia = SpiaDelProfilo.Leggi(_contesto.Archivio,
                                        _opportunita.VersioneProfilo,
                                        _opportunita.Confrontata)
        End If

        lblSpiaProfilo.Text = spia.Scritta
        lblSpiaProfilo.ForeColor = spia.Colore
        _suggerimenti.SetToolTip(lblSpiaProfilo, spia.Perche)

    End Sub

    ''' <summary>
    ''' Se questa candidatura si può <b>riconfrontare</b>: è già stata confrontata, ma con
    ''' un profilo che non è più quello di oggi. Allora in fondo al pannello compare
    ''' «⚠ Riconfronta».
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Che cosa rovescia.</b> Fino al 2026-09-02 una candidatura confrontata non
    ''' si riconfrontava per scelta, e l'unica strada era rifarla da capo incollando di
    ''' nuovo l'annuncio. Solo che l'annuncio <b>è già lì</b>, letto e strutturato nella sua
    ''' cartella: rifarlo a mano era chiedere all'utente di ricopiare una cosa che il
    ''' programma ha, e di pagare all'AI una lettura già pagata. Il motore sapeva già farlo —
    ''' <c>ConfrontaAsync</c> vuole l'annuncio già strutturato — e come a T9c mancava solo
    ''' il gesto.</para>
    ''' <para><b>Perché solo quando il profilo è cambiato.</b> Un «Riconfronta» sempre acceso
    ''' inviterebbe a ripagare una risposta che non cambierebbe: a parità di profilo e di
    ''' annuncio il confronto è lo stesso. Il gesto compare quando ha una ragione, e la
    ''' ragione si legge accanto al bottone.</para>
    ''' <para><b>Dove sta il bottone, e perché è cambiato posto lo stesso giorno.</b> Il
    ''' gesto era nato come <b>quarto mestiere di «Analizza»</b>, ed era irraggiungibile:
    ''' «Analizza» vive dentro la fascia d'ingresso, e una candidatura già confrontata — cioè
    ''' l'unica su cui il riconfronto esista — si riapre con quella fascia <b>chiusa</b>. Chi
    ''' chiudeva la finestra della riapertura restava senza nessun modo di rifare i conti, e
    ''' la finestra intanto prometteva che «il bottone Riconfronta resta dov'è». Ora il gesto
    ''' ha un comando suo in fondo al pannello, <c>btnRiconfronta</c>, che <b>c'è solo quando
    ''' serve</b> — la stessa regola di «⚠ Rigenera la lettera» in P6 (cap. 03.3: quel che
    ''' non serve non deve nemmeno esserci). Il quarto mestiere di «Analizza» è stato tolto:
    ''' due bottoni con lo stesso nome a video sono peggio di nessuno.</para>
    ''' <para>Sulla scartata no: quella è chiusa (cap. 07.3), e rifarle i conti sarebbe
    ''' lavorare per niente — la stessa ragione per cui non le si scrive un CV. E se
    ''' nella casella c'è del testo incollato, quello ha la precedenza: chi scrive lì vuole
    ''' un annuncio nuovo, non ripescare questo.</para>
    ''' </remarks>
    Private Function DaRiconfrontare() As Boolean

        If _opportunita Is Nothing OrElse Not _opportunita.Confrontata Then Return False
        If _opportunita.Stato = StatoOpportunita.Scartata Then Return False
        If _opportunita.AnnuncioVuoto Then Return False
        If txtAnnuncio.Text.Trim() <> "" Then Return False

        Return ProfiloDisallineato() IsNot Nothing

    End Function

    ''' <summary>Se questa candidatura ha già un 🎯 CV o una ✉️ lettera scritti.</summary>
    Private Function ConDocumentiScritti() As Boolean

        Return _opportunita IsNot Nothing AndAlso
               (_opportunita.Cv IsNot Nothing OrElse _opportunita.Lettera IsNot Nothing)

    End Function

    ''' <summary>Se questa candidatura è già partita: spedita, o con un esito.</summary>
    ''' <remarks>
    ''' Conta perché il registro <b>non</b> è uno storico: si rigenera dalle cartelle delle
    ''' candidature (<see cref="Dati.Registro"/>), quindi mostra le stelle di adesso, non
    ''' quelle di quando la candidatura è stata mandata. Riconfrontarne una già partita fa
    ''' sparire da ogni posto il punteggio con cui è stata spedita — e quello, a differenza
    ''' dei giudizi, non serviva più a decidere: serviva a ricordare.
    ''' </remarks>
    Private Function GiaPartita() As Boolean

        Return _opportunita IsNot Nothing AndAlso
               (_opportunita.Stato = StatoOpportunita.Inviata OrElse
                _opportunita.Stato = StatoOpportunita.Esito)

    End Function

    ''' <summary>
    ''' Che cosa succede riconfrontando, e soprattutto che cosa <b>non</b> succede: la
    ''' conferma del cap. 12.7 dice sempre anche quel che resta al suo posto, perché chi
    ''' legge «rifai il confronto» può temere di perdere tutto il resto insieme.
    ''' </summary>
    Private Function SpiegazioneDelRiconfronto() As String

        Dim testo As String =
            "Rifaccio il confronto fra questo annuncio e il tuo profilo di oggi. L'annuncio " &
            "non lo rileggo: quello resta com'è." & vbLf & vbLf &
            "Le stelle e i giudizi che vedi adesso vengono sostituiti dai nuovi, e quelli di " &
            "prima non si recuperano."

        If ConDocumentiScritti() Then
            testo &= vbLf & vbLf &
                     "Il 🎯 CV e la ✉️ lettera già scritti invece non si toccano: restano dove " &
                     "sono e si possono ancora esportare. Nascono dai giudizi di prima — per " &
                     "allinearli al confronto nuovo, rigenerali da «Documenti»."
        End If

        ' Su una candidatura già partita c'è una perdita in più, e va detta per nome: il
        ' registro non tiene uno storico dei punteggi, li rilegge dalle cartelle. Dopo il
        ' riconfronto, con quante stelle questa candidatura è stata spedita non è più scritto
        ' da nessuna parte — e quel numero non serviva a decidere, serviva a ricordare.
        If GiaPartita() Then
            testo &= vbLf & vbLf &
                     "Attenzione: questa candidatura è già partita. Le stelle con cui l'hai " &
                     "mandata non restano da nessuna parte — nemmeno nella Home, che le " &
                     "rilegge da qui."
        End If

        Return testo

    End Function

    ''' <summary>
    ''' Confronta col profilo l'annuncio di una candidatura riaperta, senza rileggerlo:
    ''' è il solo secondo passo dei due (cap. 07.3; cap. 12, A5).
    ''' </summary>
    ''' <remarks>
    ''' È pubblica per la stessa ragione di <see cref="AnalizzaLAnnuncioAsync"/>: di un
    ''' gestore di clic il banco non può aspettare la fine.
    ''' </remarks>
    Public Async Function ConfrontaLaRiapertaAsync() As Task

        If AiAlLavoro Then Return
        If _opportunita Is Nothing Then Return

        ' Da T9c questa porta serviva alla sola candidatura rimasta senza giudizi. Dal
        ' 2026-09-02 porta anche il riconfronto, e passa di qui perché è lo stesso identico
        ' lavoro: l'annuncio è già strutturato, il profilo è quello di oggi. L'unica
        ' differenza è che stavolta dei giudizi ci sono già, e vanno sostituiti.
        '
        ' La conferma non si chiede qui. Sta nel gesto — il clic sul bottone, e la finestra
        ' che si apre alla riapertura — come per lo scarto: questo metodo è pubblico perché
        ' lo chiama il banco, e una modale aperta qui dentro lascerebbe appeso un collaudo
        ' che nessuno può venire a chiudere.
        Dim riconfronto As Boolean = DaRiconfrontare()

        If _opportunita.Confrontata AndAlso Not riconfronto Then Return

        If _pipeline Is Nothing Then
            RaccontaUnAvviso(MotivoSenzaAi())
            Return
        End If

        Dim profilo As Profilo = LeggiIlProfilo()
        If profilo Is Nothing Then Return

        Dim candidatura As Opportunita = _opportunita

        Using filo As New CancellationTokenSource()

            _annulla = filo
            _riconfrontoInVolo = riconfronto
            LavoroInCorso(True)

            Try
                RaccontaLoStato("Confronto l'annuncio con il tuo profilo…", StileApp.TestoSecondario)
                Await _pipeline.ConfrontaAsync(candidatura, profilo, filo.Token).ConfigureAwait(True)

                candidatura.VersioneProfilo = VersioneInUso()

                MostraLOpportunita()
                FasciaDIngresso(aperta:=False)

                Dim racconto As String = Archivia(candidatura, RiassuntoDelMatch())

                ' I documenti nati dal confronto di prima non si toccano — potrebbero essere
                ' quelli già spediti, ed è la stessa promessa che P6 fa al 📄 CV base — ma
                ' non possono restare zitti: da adesso raccontano giudizi che non esistono
                ' più, e chi esporta senza saperlo manda un testo scritto su un'altra misura.
                If riconfronto AndAlso ConDocumentiScritti() Then
                    RaccontaUnAvviso(racconto & vbLf &
                                     "Il 🎯 CV e la ✉️ lettera di prima restano dove sono, ma nascono " &
                                     "dai giudizi di allora: rigenerali da «Documenti» per allinearli.")
                Else
                    RaccontaLoStato(racconto, StileApp.TestoSecondario)
                End If

            Catch ex As OperationCanceledException
                ' Su disco non è cambiato niente: la cartella è quella di prima, com'era —
                ' col suo annuncio, e con i giudizi di prima se ne aveva.
                RaccontaLoStato("Confronto annullato: la candidatura resta com'era.",
                                StileApp.TestoSecondario)

            Catch ex As ErroreAi
                RaccontaUnErrore(ex.Message & vbLf & "La candidatura resta com'era: puoi riprovare.")

            Finally
                _annulla = Nothing

                ' Prima di rivedere i comandi, non dopo: è AggiornaComandi a decidere se
                ' «⚠ Riconfronta» resta a video, e da qui in poi non fa più da annulla.
                _riconfrontoInVolo = False
                LavoroInCorso(False)
            End Try

        End Using

    End Function

    ''' <summary>
    ''' Il rifiuto garbato di cap. 06.4, detto in modo diverso a seconda di come il testo
    ''' è arrivato: chi ha catturato una pagina va rimandato al singolo annuncio, chi ha
    ''' incollato un testo va rimandato al testo. Dire «pagina di elenco» a chi non ha
    ''' aperto nessuna pagina sarebbe un consiglio che non si può seguire.
    ''' </summary>
    Private Shared Function NonSembraUnAnnuncio(link As String) As String

        If String.IsNullOrWhiteSpace(link) Then
            Return "In questo testo non ho trovato un annuncio di lavoro. " &
                   "Controlla di aver incollato l'annuncio per intero, poi riprova."
        End If

        Return "In questa pagina non ho trovato un annuncio: sembra un elenco di risultati, " &
               "una home o una schermata di accesso. Apri il singolo annuncio e ricattura."

    End Function

    ''' <summary>
    ''' Scrive l'opportunità nella sua cartella e ne restituisce il racconto per la barra.
    ''' Se la scrittura non riesce, i giudizi restano a video lo stesso: quello che si è
    ''' pagato all'AI non si butta per un disco che non collabora — ma non si finge
    ''' nemmeno che sia al sicuro.
    ''' </summary>
    ''' <param name="riassunto">
    ''' Cosa è appena successo, detto da chi lo sa: il match dopo un confronto, lo scarto
    ''' dopo uno scarto. Qui si aggiunge solo dove la candidatura è finita.
    ''' </param>
    Private Function Archivia(candidatura As Opportunita, riassunto As String) As String

        If _contesto Is Nothing Then Return riassunto

        Try
            _contesto.Opportunita.Salva(candidatura)
            AnnotaNelRegistro(candidatura)
            Return riassunto & vbLf & $"Salvata in «{candidatura.Cartella}»."

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Return riassunto & vbLf &
                   $"Attenzione: non sono riuscita a salvarla su disco ({ex.Message})."
        End Try

    End Function

    ''' <summary>
    ''' Tiene in riga la vista d'insieme dopo che la cartella è stata scritta (cap. 07.3).
    ''' </summary>
    ''' <remarks>
    ''' Ha il suo <c>Try</c> apposta, separato da quello di <see cref="Archivia"/>: un
    ''' indice che non si lascia scrivere <b>non</b> è una candidatura persa — quella sta
    ''' nella sua cartella — e dirlo all'utente come se lo fosse sarebbe un falso allarme.
    ''' La Home se lo rigenera da sé alla prossima occhiata.
    ''' </remarks>
    Private Sub AnnotaNelRegistro(candidatura As Opportunita)

        Try
            _contesto.Registro.Annota(candidatura)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
        End Try

    End Sub

    ''' <summary>
    ''' Rimette in mostra una candidatura già scritta su disco, com'era: l'annuncio letto,
    ''' le stelle, i giudizi (cap. 12.7). Non chiama l'AI e non cambia niente — è una
    ''' riapertura, ed è la strada che arriva dalla coda della Home.
    ''' </summary>
    Public Sub RiapriLaCandidatura(candidatura As Opportunita)

        If candidatura Is Nothing Then Throw New ArgumentNullException(NameOf(candidatura))
        If AiAlLavoro Then Return

        _opportunita = candidatura
        txtAnnuncio.Clear()

        If candidatura.Confrontata Then
            MostraLOpportunita()
            FasciaDIngresso(aperta:=False)
        Else
            ' Una candidatura rimasta al solo annuncio non ha giudizi da mostrare: si
            ' riapre con la fascia aperta, che è da dove si riprende.
            MostraLaValutazione(Nothing)
            txtAnnuncioLetto.Text = VistaAnnuncio.Riassunto(candidatura.Annuncio)
            FasciaDIngresso(aperta:=True)
        End If

        ' Da T9c: alla riaperta senza giudizi si dice che strada le resta, invece di
        ' lasciarla in un pannello che sembra non avere comandi per lei.
        Dim racconto As String = $"Riaperta da «{Path.GetFileName(If(candidatura.Cartella, String.Empty))}». "

        If Not candidatura.Confrontata AndAlso Not candidatura.AnnuncioVuoto Then
            racconto &= "L'annuncio c'è, il confronto col tuo profilo no: premi «Confronta»."
        Else
            racconto &= RiassuntoDelMatch()
        End If

        ' Dal 2026-09-02 la riga dice anche se il profilo non è più quello del confronto, e
        ' quando lo dice smette di essere una didascalia: prende la parola e il colore
        ' dell'avviso, come ogni riga che segnala qualcosa che non torna (cap. 03.8).
        Dim disallineato As String = ProfiloDisallineato()

        If disallineato Is Nothing Then
            RaccontaLoStato(racconto, StileApp.TestoSecondario)
        Else
            RaccontaUnAvviso(racconto & vbLf & disallineato)
        End If

        AggiornaComandi()

        ProponiIlRiconfronto()

    End Sub

    ''' <summary>
    ''' Se il profilo non è più quello con cui questa candidatura fu confrontata, lo dice
    ''' con una finestra e offre il gesto per rimettersi in pari.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché una finestra e non la sola riga di stato.</b> La riga c'è già e resta
    ''' — ma è la stessa riga che dice «riaperta da…», e chi riapre una candidatura guarda le
    ''' stelle grandi in mezzo allo schermo, non la didascalia sotto. Le stelle però sono
    ''' esattamente il numero che non vale più: lasciarle leggere in silenzio è il difetto,
    ''' e un avviso che si può non vedere non lo cura. Vale la spesa dell'interruzione perché
    ''' il caso è raro (succede solo dopo che il profilo è cambiato) e perché la finestra
    ''' non chiede soltanto di prendere atto: porta il gesto con sé.</para>
    ''' <para><b>Perché non dentro la riapertura.</b> Chi apre la candidatura è la Home, che
    ''' sta ancora eseguendo il suo clic: una modale aperta lì fermerebbe il pannello a metà
    ''' disegno, e la finestra parlerebbe di una candidatura che chi legge non ha ancora
    ''' visto. Con <c>BeginInvoke</c> si programma per subito dopo — a candidatura a video,
    ''' con le sue stelle sotto gli occhi.</para>
    ''' <para><b>Perché solo se c'è il gesto.</b> Su una candidatura scartata il profilo può
    ''' essere cambiato quanto vuole: quella è chiusa (cap. 07.3), non le si rifanno i conti,
    ''' e una finestra che dice una cosa a cui non si può rimediare è solo un ostacolo fra
    ''' l'utente e quel che voleva guardare.</para>
    ''' <para><b>Perché si guarda il Form e non l'handle.</b> L'handle qui c'è sempre, anche
    ''' nel banco: i collaudi chiamano <c>CreateControl()</c> apposta, perché senza controlli
    ''' realizzati metà delle proprietà mentono. Quel che il banco <b>non</b> ha è una
    ''' finestra attorno al pannello — e quella è la condizione vera per aprire una modale,
    ''' non un trucco per riconoscere i collaudi: una finestra che si apre ha bisogno di una
    ''' finestra a cui appartenere. Fidarsi dell'handle avrebbe lasciato il banco appeso a
    ''' una finestra che nessuno può venire a chiudere, che è peggio di un collaudo rosso
    ''' perché non finisce mai. Quel che si collauda è la <b>decisione</b> —
    ''' <see cref="ProfiloDisallineato"/> e <see cref="DaRiconfrontare"/> — non la finestra
    ''' che ne segue.</para>
    ''' </remarks>
    ''' <remarks>
    ''' <para><b>E perché non su una candidatura già partita.</b> L'avviso esiste per una
    ''' ragione precisa: le stelle servono a decidere se candidarsi, e su un profilo vecchio
    ''' possono dire il numero sbagliato. Ma se la candidatura è già stata spedita quella
    ''' decisione è presa, e la finestra si metterebbe fra l'utente e una pratica che sta
    ''' solo riguardando — per proporgli per giunta un gesto che gli farebbe perdere il
    ''' punteggio con cui l'ha mandata. Il bottone «⚠ Riconfronta» resta in fondo al
    ''' pannello per chi lo vuole davvero, con la sua conferma che lo dice.</para>
    ''' <para><b>E perché la finestra non è più l'unica via.</b> Chi risponde «Annulla» a
    ''' questa domanda deve poterla rifare dopo. Fino al 2026-09-02 non poteva: il gesto
    ''' stava su «Analizza», che qui non è nemmeno a video, e chiudendo la finestra la
    ''' candidatura restava con le sue stelle vecchie e nessun modo di rifarle. Adesso la
    ''' finestra propone e il bottone <b>resta</b>: proporre una volta e restare disponibile
    ''' sono due mestieri diversi, e servono tutti e due (v. <see cref="DaRiconfrontare"/>).</para>
    ''' </remarks>
    Private Sub ProponiIlRiconfronto()

        If Not IsHandleCreated OrElse FindForm() Is Nothing Then Return
        If Not DaRiconfrontare() OrElse GiaPartita() Then Return

        BeginInvoke(New Action(AddressOf ChiediSeRiconfrontare))

    End Sub

    ''' <summary>La domanda vera e propria, a candidatura già a video.</summary>
    Private Async Sub ChiediSeRiconfrontare()

        If AiAlLavoro OrElse Not DaRiconfrontare() Then Return

        Dim motivo As String = ProfiloDisallineato()
        If motivo Is Nothing Then Return

        If Not FinestraConferma.Chiedi(
            Me, "Il profilo non è più quello del confronto",
            motivo & vbLf & vbLf &
            "Posso rifare il confronto adesso, sull'annuncio che è già qui: non lo rileggo, " &
            "quindi è una chiamata sola, e le stelle tornano a dire quello che direbbero oggi." &
            vbLf & vbLf &
            "Se preferisci farlo più tardi, il bottone «⚠ Riconfronta» resta in fondo alla " &
            "pagina finché il profilo non torna in pari.",
            "Riconfronta ora") Then Return

        Await ConfrontaLaRiapertaAsync()

    End Sub

    ''' <summary>
    ''' Lascia andare la candidatura in mostra se è quella appena eliminata dalla Home
    ''' (cap. 11.5), e dice se l'ha fatto: il pannello torna com'era prima di aprirne una.
    ''' </summary>
    ''' <remarks>
    ''' <para>Non è riguardo per l'occhio. Finché quell'oggetto resta qui, il primo comando
    ''' che archivia lo riscriverebbe su disco — cioè <b>ricreerebbe la cartella</b> appena
    ''' cancellata, con dentro quel che il pannello ha in memoria. Chi elimina si aspetta
    ''' che sparisca, non che risorga al clic dopo.</para>
    ''' <para>Con una chiamata all'AI in volo non si tocca niente e si risponde
    ''' <c>False</c>. Il caso non si dà — mentre l'AI lavora la barra è bloccata e in Home
    ''' non ci si arriva (cap. 02.6) — e strappare il tavolo sotto un'attesa sarebbe un
    ''' modo nuovo di rompere una cosa che funziona.</para>
    ''' </remarks>
    Public Function Dimentica(cartella As String) As Boolean

        If AiAlLavoro OrElse _opportunita Is Nothing Then Return False

        If Not String.Equals(_opportunita.Cartella, cartella, StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If

        _opportunita = Nothing

        txtAnnuncio.Clear()
        txtAnnuncioLetto.Clear()
        MostraLaValutazione(Nothing)
        FasciaDIngresso(aperta:=True)

        RaccontaLoStato("La candidatura che era qui è stata eliminata: incolla il testo di " &
                        "un altro annuncio e premi «Analizza».", StileApp.TestoSecondario)

        AggiornaComandi()

        Return True

    End Function

    ''' <summary>
    ''' La versione di profilo con cui si sta confrontando: è ciò che tiene spiegabile un
    ''' CV già inviato anche a profilo evoluto (cap. 11.1). L'ultima versione dello
    ''' storico è quella corrente, perché il profilo si salva sempre da una sola porta.
    ''' </summary>
    Private Function VersioneInUso() As String

        If _contesto Is Nothing Then Return Nothing

        Try
            Return _contesto.Archivio.Versioni().LastOrDefault()

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' Non sapere da quale versione nasce è un peccato veniale: non vale il
            ' prezzo di fermare un confronto già pagato.
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' Il profilo su disco, o <c>Nothing</c> se non c'è o non si lascia leggere — con il
    ''' motivo già raccontato. Senza profilo non c'è niente da confrontare: è la scheda P2
    ''' il posto da cui si comincia (cap. 12, A2).
    ''' </summary>
    Private Function LeggiIlProfilo() As Profilo

        If _contesto Is Nothing OrElse Not _contesto.Archivio.Esiste Then
            RaccontaLoStato(
                "Prima serve il tuo profilo: costruiscilo nella scheda «Profilo», poi torna qui.",
                StileApp.TestoSecondario)
            Return Nothing
        End If

        Try
            Return _contesto.Archivio.Carica()

        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                   OrElse TypeOf ex Is UnauthorizedAccessException
            RaccontaUnErrore(
                $"Il profilo c'è ma non si lascia leggere: {ex.Message}" & vbLf &
                "Aprilo dalla scheda «Profilo»: lì trovi come rimediare.")
            Return Nothing
        End Try

    End Function

    ' ==================================================================
    ' Quel che si vede: annuncio, stelle, giudizi
    ' ==================================================================

    ''' <summary>Mette a video l'opportunità: l'annuncio letto e la sua valutazione.</summary>
    Private Sub MostraLOpportunita()

        txtAnnuncioLetto.Text = VistaAnnuncio.Riassunto(_opportunita.Annuncio)
        MostraLaValutazione(VistaConfronto.Da(_opportunita))

    End Sub

    ''' <summary>
    ''' Disegna la valutazione: le stelle grandi, le note che le spiegano, i giudizi voce
    ''' per voce. Con <c>Nothing</c> ripulisce, che è come nasce il pannello.
    ''' </summary>
    Private Sub MostraLaValutazione(vista As VistaConfronto)

        _vista = vista

        lvwGiudizi.BeginUpdate()
        lvwGiudizi.Items.Clear()

        If vista Is Nothing Then
            lblStelle.Text = ""
            MostraLaSpiaDelProfilo()
            MostraLaNota("", StileApp.TestoSecondario)
            lvwGiudizi.EndUpdate()
            MostraLaLetturaDInsieme()
            MostraLoStatoDellaCandidatura()
            AggiornaComandi()
            Return
        End If

        lblStelle.Text = StelleScritte(vista.Stelle)
        MostraLaSpiaDelProfilo()

        For Each giudizio As GiudizioMostrato In vista.Giudizi
            lvwGiudizi.Items.Add(RigaDelGiudizio(giudizio))
        Next

        lvwGiudizi.EndUpdate()

        MostraLaNota(NotaDaMostrare(vista), If(vista.GateEliminatorio OrElse vista.Sconsigliata,
                                               StileApp.Pericolo, StileApp.TestoSecondario))
        MostraLaLetturaDInsieme()
        MostraLoStatoDellaCandidatura()
        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' A che punto è questa candidatura, accanto alle stelle (cap. 07.3): riaprendola
    ''' dalla Home è la prima cosa che si vuole sapere dopo il punteggio. Con la data del
    ''' passaggio, quando c'è — le candidature scritte prima di T5c non ce l'hanno.
    ''' </summary>
    Private Sub MostraLoStatoDellaCandidatura()

        If _opportunita Is Nothing Then
            lblStatoCandidatura.Text = ""
            Return
        End If

        Dim quando As Date

        ' Da T9c. Chi guarda non pensa per stati: pensa «rifiutata», non «con esito»
        ' (cap. 07.3). La parola è la stessa che la Home mette nella sua colonna.
        Dim scritto As String = EsitiCandidatura.EtichettaDi(_opportunita.Stato, _opportunita.Esito)

        If _opportunita.DateStati.TryGetValue(_opportunita.Stato, quando) Then
            scritto &= $" il {quando:dd/MM/yyyy}"
        End If

        lblStatoCandidatura.Text = scritto
        lblStatoCandidatura.ForeColor = ColoreDelloStato()

    End Sub

    ''' <summary>
    ''' Una riga dell'elenco. Il ⛔ sta in coda al requisito, come nel prototipo, e il
    ''' colore fa il resto: rosso per il paletto rimasto scoperto — è lui che ha messo il
    ''' tetto al punteggio — e grigio per ciò che non si è potuto valutare.
    ''' </summary>
    Private Shared Function RigaDelGiudizio(giudizio As GiudizioMostrato) As ListViewItem

        Dim requisito As String = giudizio.Requisito
        If giudizio.Eliminatorio Then requisito &= " ⛔"

        Dim riga As New ListViewItem({giudizio.Simbolo, requisito, giudizio.Peso, giudizio.NomeEsito}) With {
            .Tag = giudizio}

        If giudizio.Eliminatorio AndAlso giudizio.Esito = EsitoGiudizio.NonSoddisfatto Then
            riga.ForeColor = StileApp.Pericolo
        ElseIf giudizio.Esito = EsitoGiudizio.NonDeterminabile Then
            riga.ForeColor = StileApp.TestoSecondario
        End If

        Return riga

    End Function

    ''' <summary>
    ''' Le stelle come si leggono: quelle piene, quelle vuote, e il numero esatto accanto.
    ''' </summary>
    Private Shared Function StelleScritte(quante As Double?) As String

        If Not quante.HasValue Then Return "Match non calcolabile"

        Dim piene As Integer = CInt(Math.Floor(quante.Value))
        piene = Math.Max(0, Math.Min(StelleDellaScala, piene))

        Return New String("★"c, piene) & New String("☆"c, StelleDellaScala - piene) &
               "   " & quante.Value.ToString("0.0", CultureInfo.CurrentCulture) & " su 5"

    End Function

    ''' <summary>
    ''' Cosa c'è da sapere sul punteggio: la nota del calcolo — lo scarto tagliato, il
    ''' tetto del requisito eliminatorio — e, sotto soglia, il consiglio di non
    ''' candidarsi. Che resta un consiglio: la scelta è dell'utente (cap. 12, A5.3).
    ''' </summary>
    Private Shared Function NotaDaMostrare(vista As VistaConfronto) As String

        Dim righe As New List(Of String)

        If Not String.IsNullOrWhiteSpace(vista.Nota) Then righe.Add(vista.Nota)

        If vista.Sconsigliata Then
            righe.Add("Con un match così basso i documenti verrebbero comunque onesti, ma poco " &
                      "spendibili su questo annuncio: puoi generarli lo stesso oppure fermarti qui.")
        End If

        Return String.Join(vbLf, righe)

    End Function

    Private Sub MostraLaNota(testo As String, colore As Color)

        lblNota.Text = testo
        lblNota.ForeColor = colore

    End Sub

    ''' <summary>
    ''' Il riquadro in basso quando nessuna riga è scelta: la sintesi onesta del match.
    ''' </summary>
    Private Sub MostraLaLetturaDInsieme()

        lblEtichettaSpiegazione.Text = "Lettura d'insieme"
        txtSpiegazione.Text = If(_vista Is Nothing, "", _vista.LetturaInsieme)

    End Sub

    ''' <summary>
    ''' Scegliendo una riga, lo stesso riquadro racconta perché quell'esito: è il posto in
    ''' cui l'utente verifica che il giudizio sia ancorato al profilo e non inventato.
    ''' </summary>
    Private Sub lvwGiudizi_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lvwGiudizi.SelectedIndexChanged

        Dim scelta As ListViewItem = lvwGiudizi.SelectedItems.Cast(Of ListViewItem)().FirstOrDefault()
        Dim giudizio As GiudizioMostrato = TryCast(scelta?.Tag, GiudizioMostrato)

        If giudizio Is Nothing Then
            MostraLaLetturaDInsieme()
            Return
        End If

        lblEtichettaSpiegazione.Text = "Perché"
        txtSpiegazione.Text = If(String.IsNullOrWhiteSpace(giudizio.Spiegazione),
                                 "L'AI non ha motivato questa voce.", giudizio.Spiegazione)

    End Sub

    ' ==================================================================
    ' I bottoni
    ' ==================================================================

    Private Async Sub btnAnalizza_Click(sender As Object, e As EventArgs) Handles btnAnalizza.Click

        ' Ad attesa in corso lo stesso bottone serve ad annullarla, come l'import in P2.
        If AiAlLavoro Then
            AnnullaIlLavoro()
            Return
        End If

        Await AnalizzaLAnnuncioAsync()

    End Sub

    ''' <summary>
    ''' Rifà il confronto col profilo di oggi, sull'annuncio che è già nella cartella.
    ''' </summary>
    ''' <remarks>
    ''' Il riconfronto sostituisce stelle e giudizi che l'utente ha già pagato: si chiede
    ''' prima, e si chiede qui — nel gesto, come per lo scarto (cap. 12.7) — perché il lavoro
    ''' vero è un metodo pubblico che chiama anche il banco, e una modale aperta là dentro
    ''' lascerebbe appeso un collaudo che nessuno può venire a chiudere.
    ''' </remarks>
    Private Async Sub btnRiconfronta_Click(sender As Object, e As EventArgs) Handles btnRiconfronta.Click

        ' Anche questa attesa si annulla, e dal comando che l'ha chiesta: sparire mentre il
        ' lavoro che si è appena chiesto è in volo lascerebbe l'attesa senza uscita — e qui
        ' «Analizza», che altrove fa da annulla, non è nemmeno a video.
        If AiAlLavoro Then
            AnnullaIlLavoro()
            Return
        End If

        If Not DaRiconfrontare() Then Return

        If Not FinestraConferma.Chiedi(Me, "Rifai il confronto",
                                       SpiegazioneDelRiconfronto(), "Riconfronta") Then Return

        Await ConfrontaLaRiapertaAsync()

    End Sub

    ''' <summary>
    ''' Riapre la fascia per un altro annuncio. Non c'è niente da confermare: quella in
    ''' mostra è già nella sua cartella, e resta lì.
    ''' </summary>
    Private Sub btnNuovoAnnuncio_Click(sender As Object, e As EventArgs) Handles btnNuovoAnnuncio.Click

        FasciaDIngresso(aperta:=True)

        txtAnnuncio.Clear()
        txtAnnuncio.Focus()

        RaccontaLoStato("Incolla il testo del prossimo annuncio, poi premi «Analizza».",
                        StileApp.TestoSecondario)

    End Sub

    Private Sub btnGeneraDocumenti_Click(sender As Object, e As EventArgs) Handles btnGeneraDocumenti.Click
        RaiseEvent DocumentiRichiesti(Me, EventArgs.Empty)
    End Sub

    Private Sub btnBrainstorm_Click(sender As Object, e As EventArgs) Handles btnBrainstorm.Click
        RaiseEvent BrainstormRichiesto(Me, EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Scarta la candidatura in mostra (cap. 07.3): la dà per chiusa, senza cancellare
    ''' niente. Si chiede conferma prima — è una decisione che non si disfa, e la regola
    ''' del cap. 12.7 è che nessuno scriva senza un passaggio esplicito.
    ''' </summary>
    ''' <remarks>
    ''' Dal 2026-09-01 lo chiede la <see cref="FinestraConferma"/> e non più una
    ''' <c>MessageBox</c> con Sì e No: lo scarto pesa quanto l'eliminazione di una
    ''' candidatura in P1 — è una strada che non si ripercorre — e due conferme dello
    ''' stesso livello non possono avere due forme diverse (cap. 03.3).
    ''' </remarks>
    Private Sub btnScarta_Click(sender As Object, e As EventArgs) Handles btnScarta.Click

        If _opportunita Is Nothing OrElse AiAlLavoro Then Return

        If Not FinestraConferma.Chiedi(Me, "Scarta la candidatura",
                                       SpiegazioneDelloScarto(), "Confermo") Then Return

        _opportunita.Avanza(StatoOpportunita.Scartata)

        RaccontaLoStato(Archivia(_opportunita, "Scartata: la do per chiusa."),
                        StileApp.TestoSecondario)

        MostraLoStatoDellaCandidatura()
        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Il testo della conferma: che cosa succede scartando, e soprattutto che cosa
    ''' <b>non</b> succede — qui non sparisce niente, e chi legge «scarta» può temere il
    ''' contrario.
    ''' </summary>
    ''' <remarks>
    ''' È pubblica perché la legge il banco, come <c>PannelloHome.SpiegazioneDellEliminazione</c>:
    ''' premere il bottone aprirebbe una finestra modale, e di quella un collaudo non può
    ''' aspettare la chiusura.
    ''' </remarks>
    Public Shared Function SpiegazioneDelloScarto() As String

        Return "Non cancello niente: la candidatura resta nella sua cartella e la ritrovi " &
               "nella Home, con i documenti che le hai fatto scrivere." & vbLf & vbLf &
               "Ma la do per chiusa, e da uno scarto non si torna indietro. Confermi?"

    End Function

    ''' <summary>
    ''' Di che colore si scrive a che punto è. Il rosso resta allo scarto — è l'unica
    ''' strada senza ritorno — e il verde va all'assunzione: sono i due estremi, e in
    ''' mezzo il grigio di tutto il resto, perché «rifiutata» non è un guasto del
    ''' programma da segnalare in rosso.
    ''' </summary>
    Private Function ColoreDelloStato() As Color

        If _opportunita Is Nothing Then Return StileApp.TestoSecondario
        If _opportunita.Stato = StatoOpportunita.Scartata Then Return StileApp.Pericolo

        If _opportunita.Esito.HasValue AndAlso _opportunita.Esito.Value = EsitoCandidatura.Assunto Then
            Return StileApp.Successo
        End If

        Return StileApp.TestoSecondario

    End Function

    Private Sub txtAnnuncio_TextChanged(sender As Object, e As EventArgs) Handles txtAnnuncio.TextChanged
        AggiornaComandi()
    End Sub

    ' ==================================================================
    ' Com'è andata (cap. 07.3)
    ' ==================================================================

    ''' <summary>
    ''' Apre il menù degli esiti sotto il bottone. Le voci si ricompongono a ogni
    ''' apertura, perché la spunta dice qual è l'esito <b>di adesso</b>.
    ''' </summary>
    Private Sub btnEsito_Click(sender As Object, e As EventArgs) Handles btnEsito.Click

        If _opportunita Is Nothing OrElse AiAlLavoro Then Return

        ' Si apre <b>sopra</b> il bottone, non sotto: «Com'è andata…» sta nella fascia in
        ' fondo al pannello, e un menù che scende da lì finisce fuori dalla finestra, sulla
        ' barra di Windows. Visto alla prima prova dal vivo (2026-08-21).
        MenuDegliEsiti().Show(btnEsito, New Point(0, 0), ToolStripDropDownDirection.AboveRight)

    End Sub

    ''' <summary>
    ''' Il menù degli esiti, con le voci di adesso: prima l'attesa, poi i tre esiti veri,
    ''' separati da una riga.
    ''' </summary>
    ''' <remarks>
    ''' <para>«In attesa» sta in cima e non è un esito: è il modo di <b>togliere</b> quello
    ''' registrato per sbaglio (cap. 07.3). Il separatore serve a dire che le due cose non
    ''' sono dello stesso genere — sotto c'è com'è andata, sopra c'è che non si sa ancora.
    ''' Le voci si rifanno a ogni apertura, perché la spunta dice l'esito <b>di adesso</b>.</para>
    ''' <para>È pubblica per il banco, e non è una comodità: la voce di un menù contestuale
    ''' non si preme da fuori — lo strumento di collaudo risponde «Premuto» e il gestore non
    ''' parte (2026-08-21, T9c) — così il filo fra la voce scelta e
    ''' <see cref="SegnaLEsito"/> resterebbe l'unico pezzo di questa strada che nessuno
    ''' prova.</para>
    ''' </remarks>
    Public Function MenuDegliEsiti() As ContextMenuStrip

        _menuEsito.Items.Clear()

        AggiungiVoceDiEsito("In attesa — nessuna risposta", Nothing)
        _menuEsito.Items.Add(New ToolStripSeparator())

        For Each esito As EsitoCandidatura In [Enum].GetValues(Of EsitoCandidatura)()
            AggiungiVoceDiEsito(EsitiCandidatura.Etichetta(esito), esito)
        Next

        Return _menuEsito

    End Function

    Private Sub AggiungiVoceDiEsito(testo As String, quale As EsitoCandidatura?)

        Dim voce As New ToolStripMenuItem(testo) With {.Checked = EQuelloDiAdesso(quale)}

        AddHandler voce.Click, Sub(mittente As Object, evento As EventArgs) SegnaLEsito(quale)

        _menuEsito.Items.Add(voce)

    End Sub

    ''' <summary>Se quella voce è l'esito che la candidatura ha adesso.</summary>
    ''' <remarks>
    ''' Scritto per esteso e non con un <c>=</c> fra due <c>Nullable</c>: quel confronto,
    ''' quando uno dei due è <c>Nothing</c>, non vale né vero né falso — e in un <c>If</c>
    ''' di VB finirebbe per valere falso proprio nel caso che qui interessa di più.
    ''' </remarks>
    Private Function EQuelloDiAdesso(quale As EsitoCandidatura?) As Boolean

        If _opportunita Is Nothing Then Return False
        If Not quale.HasValue Then Return Not _opportunita.Esito.HasValue

        Return _opportunita.Esito.HasValue AndAlso _opportunita.Esito.Value = quale.Value

    End Function

    ''' <summary>
    ''' Registra com'è andata, o toglie l'esito segnato per sbaglio, e scrive su disco
    ''' (cap. 07.3).
    ''' </summary>
    ''' <param name="scelto">L'esito scelto nel menù; <c>Nothing</c> per «in attesa».</param>
    ''' <remarks>
    ''' <para>Non c'è nessuna conferma da dare, e non è una dimenticanza: l'esito si
    ''' cambia con un secondo clic sullo stesso menù, e chiedere «sei sicuro?» per una
    ''' cosa che si disfa da sé insegna solo a rispondere sì senza leggere. La conferma
    ''' resta dov'è servita — lo scarto, che non si disfa.</para>
    ''' <para>È pubblica perché un menù contestuale il banco non lo può premere: senza
    ''' questa porta, tutto ciò che sta dietro al clic resterebbe fuori dai collaudi.</para>
    ''' </remarks>
    Public Sub SegnaLEsito(scelto As EsitoCandidatura?)

        If _opportunita Is Nothing OrElse AiAlLavoro Then Return

        ' Riconfermare quello che c'è già non è un cambiamento: non si riscrive la
        ' cartella e non si sposta la data di quando lo si era saputo.
        If EQuelloDiAdesso(scelto) Then Return

        _opportunita.SegnaEsito(scelto)

        Dim riassunto As String = If(scelto.HasValue,
                                     $"Segnata come «{EsitiCandidatura.Etichetta(scelto.Value)}».",
                                     "Esito tolto: torna in attesa di una risposta.")

        RaccontaLoStato(Archivia(_opportunita, riassunto), StileApp.TestoSecondario)

        MostraLoStatoDellaCandidatura()
        AggiornaComandi()

    End Sub

    ' ==================================================================
    ' Aspetto, spazio e stato dei comandi
    ' ==================================================================

    ''' <inheritdoc/>
    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        ' A cedere il posto al logo è la fascia delle azioni, dove ci sono bottoni e non
        ' dati (v. IPannelloArea). L'altezza la decide la fascia stessa: almeno quella che
        ' il logo sfonda, di più se i comandi devono andare a capo (cap. 03.4).
        _ingombroLogo = ingombro
        pnlAzioni.Padding = New Padding(ingombro.Width + StileApp.DistanzaControlli, 0, 0, 0)

        DisponiLeAzioni()

    End Sub

    ''' <summary>
    ''' Rifà la disposizione dei comandi in fondo al pannello. La geometria la sa la
    ''' <see cref="FasciaDeiComandi"/>, che è di tutti i pannelli; qui resta la sola cosa
    ''' che sa questo pannello — <b>quali</b> comandi vanno da che parte.
    ''' </summary>
    Private Sub DisponiLeAzioni()

        If _comandi Is Nothing Then
            _comandi = New FasciaDeiComandi(pnlAzioni)
            _comandi.ASinistra(btnNuovoAnnuncio, btnRiconfronta, btnBrainstorm, btnEsito, btnScarta)
            _comandi.ADestra(btnGeneraDocumenti)
        End If

        _comandi.Disponi(Math.Max(AltezzaMinimaAzioni, _ingombroLogo.Height))

    End Sub

    Private Sub PannelloOpportunita_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        DisponiLeAzioni()
    End Sub

    ''' <summary>Apre o richiude la fascia in cima (cap. 03.6).</summary>
    Private Sub FasciaDIngresso(aperta As Boolean)

        _fasciaAperta = aperta
        pnlIngresso.Visible = aperta

    End Sub

    Private Sub VestiIBottoni()

        StileApp.VestiBottone(btnAnalizza, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnGeneraDocumenti, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnNuovoAnnuncio, LivelloBottone.Neutro)
        StileApp.VestiBottone(btnBrainstorm, LivelloBottone.Esplorativo)

        ' Rifare il confronto butta via stelle e giudizi già pagati: pesa come «⚠ Rigenera
        ' la lettera» in P6, e porta lo stesso segno.
        StileApp.VestiBottone(btnRiconfronta, LivelloBottone.Attenzione)

        ' Segnare com'è andata non consuma niente e si disfa: è un comando neutro.
        StileApp.VestiBottone(btnEsito, LivelloBottone.Neutro)

        ' Scartare un'opportunità la butta via: pesa quanto un'eliminazione.
        StileApp.VestiBottone(btnScarta, LivelloBottone.Distruttivo)

    End Sub

    ''' <summary>
    ''' Cosa fa il bottone del ragionamento.
    ''' </summary>
    ''' <remarks>
    ''' Qui stava <c>DichiaraLeTappeCheMancano</c>, che teneva il bottone spento dicendo
    ''' con quale tappa sarebbe arrivato (cap. 03.8). La tappa è arrivata — T7c — e al
    ''' posto della promessa c'è la spiegazione di cosa succede premendolo.
    ''' </remarks>
    Private Sub SpiegaIlRagionamento()

        _suggerimenti.SetToolTip(btnBrainstorm,
                                 "Ragiona con l'AI su questa candidatura: cosa mettere davanti e come " &
                                 "nominare quello che manca. Quel che decidete diventa gli appunti che " &
                                 "guideranno 🎯 CV mirato e ✉️ lettera.")

    End Sub

    ''' <summary>
    ''' Scrive sotto «Analizza» perché non si può premere — e non scrive niente quando si
    ''' può. Le tre ragioni sono diverse e si dicono in modo diverso: manca la chiave,
    ''' manca il profilo, manca il testo. Solo l'ultima è colpa del momento; le prime due
    ''' mandano da qualche parte.
    ''' </summary>
    Private Sub DiciPercheNonSiPuoAnalizzare(occupato As Boolean, conAi As Boolean,
                                             conProfilo As Boolean, soloIlConfronto As Boolean)

        If occupato Then
            lblPerchePento.Text = ""
            Return
        End If

        If Not conAi Then
            ' Da T9c il bottone può essere «Confronta», e allora il motivo va detto con la
            ' parola di quel mestiere: chi non deve leggere niente non capirebbe perché
            ' gli si parla di un annuncio da leggere.
            lblPerchePento.Text = If(soloIlConfronto,
                                     "Manca la chiave API: senza, il confronto non si può fare.",
                                     "Manca la chiave API: senza, l'annuncio non si può leggere.")
            lblPerchePento.ForeColor = StileApp.Pericolo

        ElseIf Not conProfilo Then
            lblPerchePento.Text = "Prima il profilo: apri la scheda «Profilo» e salvalo."
            lblPerchePento.ForeColor = StileApp.Pericolo

        ElseIf soloIlConfronto Then
            ' Il bottone è acceso e dice già cosa fa: non c'è niente da spiegare.
            lblPerchePento.Text = ""

        ElseIf txtAnnuncio.Text.Trim() = "" Then
            lblPerchePento.Text = "Incolla il testo qui a sinistra."
            lblPerchePento.ForeColor = StileApp.TestoSecondario

        Else
            lblPerchePento.Text = ""
        End If

    End Sub

    Private Sub RaccontaLoStato(testo As String, colore As Color)

        lblStatoOpportunita.Text = testo
        lblStatoOpportunita.ForeColor = colore

    End Sub

    ''' <summary>
    ''' Una riga che dice che qualcosa non è riuscito: la parola e il colore insieme
    ''' (v. <see cref="Segnalazioni"/>).
    ''' </summary>
    Private Sub RaccontaUnErrore(testo As String)

        RaccontaLoStato(Segnalazioni.PrefissoErrore & testo, StileApp.Pericolo)

    End Sub

    ''' <summary>
    ''' Una riga che dice che qualcosa manca prima ancora di provare — la chiave, i
    ''' prompt: stesso colore dell'errore, parola diversa.
    ''' </summary>
    Private Sub RaccontaUnAvviso(testo As String)

        RaccontaLoStato(Segnalazioni.PrefissoAvviso & testo, StileApp.Pericolo)

    End Sub

    ''' <summary>Il match in una riga, per la barra di stato del pannello.</summary>
    Private Function RiassuntoDelMatch() As String

        If _vista Is Nothing Then Return ""

        ' Il trattino e non un secondo «su»: le stelle si scrivono già «4,3 su 5», e
        ' «4,3 su 5 su 5 voci giudicate» è una frase che inciampa. Si è vista riaprendo una
        ' candidatura da cinque voci sull'applicazione vera (2026-08-12).
        Dim conteggio As Integer = _vista.Giudizi.Count
        Return $"Confronto fatto: {StelleScritte(_vista.Stelle)} — {conteggio} " &
               If(conteggio = 1, "voce giudicata.", "voci giudicate.")

    End Function

    ''' <summary>Da dove si comincia, detto appena il pannello si collega al motore.</summary>
    Private Sub RaccontaDaDoveSiComincia()

        If _pipeline Is Nothing Then
            RaccontaUnAvviso(MotivoSenzaAi())
            Return
        End If

        If Not _contesto.Archivio.Esiste Then
            RaccontaLoStato(
                "Prima serve il tuo profilo: costruiscilo nella scheda «Profilo», poi torna qui.",
                StileApp.TestoSecondario)
            Return
        End If

        RaccontaLoStato("Incolla il testo di un annuncio e premi «Analizza».", StileApp.TestoSecondario)

    End Sub

    ''' <summary>Perché l'analisi non si può fare: la chiave, o i prompt.</summary>
    Private Function MotivoSenzaAi() As String

        If _contesto IsNot Nothing AndAlso _contesto.Libreria Is Nothing Then
            Return "I prompt non si sono caricati: senza di loro non posso né leggere l'annuncio né confrontarlo."
        End If

        Return $"Per analizzare un annuncio serve la chiave API ({ClientClaude.NomeVariabileChiave}): " &
               "l'annuncio e il confronto passano dall'AI."

    End Function

    Private Sub LavoroInCorso(inCorso As Boolean)

        Cursor = If(inCorso, Cursors.AppStarting, Cursors.Default)

        AggiornaComandi()
        RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)

    End Sub

    ''' <summary>
    ''' Decide, in un posto solo, che cosa si può fare adesso. Le domande sono: l'AI è
    ''' disponibile, c'è un profilo con cui confrontare, c'è del testo da leggere, e c'è
    ''' già qualcosa in volo?
    ''' </summary>
    Private Sub AggiornaComandi()

        Dim occupato As Boolean = AiAlLavoro
        Dim conAi As Boolean = _pipeline IsNot Nothing
        Dim conProfilo As Boolean = _contesto IsNot Nothing AndAlso _contesto.Archivio.Esiste

        ' A lavoro in corso il bottone cambia mestiere: è l'annulla dell'attesa
        ' (cap. 12.7 — le operazioni lunghe sono annullabili). Da T9c ne ha un terzo:
        ' sulla candidatura riaperta al solo annuncio diventa «Confronta» e fa il secondo
        ' passo da solo, che è la strada da cui prima non si usciva (v. DaConfrontare).
        ' Il nome non può essere «daConfrontare»: in VB le maiuscole non distinguono, e
        ' una locale così coprirebbe la funzione DaConfrontare() qui sopra — la chiamata
        ' verrebbe letta come un indice su questo Boolean appena dichiarato. È la trappola
        ' che il progetto ha già pagato in StatiOpportunita.Consentita.
        Dim soloIlConfronto As Boolean = DaConfrontare()

        btnAnalizza.Text = If(occupato, "Annulla", If(soloIlConfronto, "Confronta", "Analizza"))

        btnAnalizza.Enabled = occupato OrElse
                              (conAi AndAlso conProfilo AndAlso
                               (soloIlConfronto OrElse txtAnnuncio.Text.Trim() <> ""))

        ' Un bottone spento senza una ragione a portata d'occhio si legge come
        ' un'applicazione rotta: la ragione si scrive sotto il bottone, dove chi voleva
        ' premerlo sta già guardando.
        DiciPercheNonSiPuoAnalizzare(occupato, conAi, conProfilo, soloIlConfronto)

        ' Il riconfronto, dal 2026-09-02: la candidatura confrontata con un profilo che non
        ' è più quello di oggi. Il nome della locale non può essere «daRiconfrontare» per la
        ' ragione scritta qui sopra — coprirebbe DaRiconfrontare(), e la chiamata verrebbe
        ' letta come un indice su questo Boolean.
        '
        ' Il comando c'è solo quando serve e non occupa posto da spento (cap. 03.3, come
        ' «⚠ Rigenera la lettera» in P6). Resta però finché la sua attesa è in volo: sparire
        ' mentre il lavoro che ha chiesto sta girando lascerebbe l'attesa senza uscita.
        Dim rifareIlConfronto As Boolean = DaRiconfrontare()
        Dim annullaIlRiconfronto As Boolean = occupato AndAlso _riconfrontoInVolo
        Dim inFascia As Boolean = rifareIlConfronto OrElse annullaIlRiconfronto

        If btnRiconfronta.Visible <> inFascia Then
            btnRiconfronta.Visible = inFascia
            DisponiLeAzioni()
        End If

        btnRiconfronta.Text = If(annullaIlRiconfronto, "Annulla", "⚠ Riconfronta")
        btnRiconfronta.Enabled = annullaIlRiconfronto OrElse
                                 (rifareIlConfronto AndAlso Not occupato AndAlso
                                  conAi AndAlso conProfilo)

        If rifareIlConfronto Then
            _suggerimenti.SetToolTip(btnRiconfronta,
                "Le stelle che vedi sono la risposta del profilo di allora." & vbLf &
                "Premi qui e rifaccio il confronto su quello di oggi: l'annuncio è già " &
                "nella cartella, non lo rileggo.")
        End If

        txtAnnuncio.ReadOnly = occupato
        txtAnnuncio.BackColor = If(occupato, StileApp.FondoPagina, StileApp.FondoCasella)

        btnNuovoAnnuncio.Enabled = Not occupato AndAlso Not _fasciaAperta

        ' I documenti si chiedono dopo aver visto le stelle: è la decisione che sta in
        ' mezzo al flusso (cap. 12, A5→A7). Sotto soglia il bottone resta acceso — si
        ' sconsiglia, non si impedisce — ma su una candidatura scartata no: quella è
        ' chiusa, e scriverle un CV sarebbe lavorare per niente.
        btnGeneraDocumenti.Enabled = Not occupato AndAlso
                                     _opportunita IsNot Nothing AndAlso _opportunita.Confrontata AndAlso
                                     _opportunita.Stato <> StatoOpportunita.Scartata

        ' Il pannello dei documenti si rifiuta di scrivere su un confronto che non è più
        ' quello del profilo di oggi (cap. 08.4), e fa bene: un CV mirato sceglie cosa
        ' mettere in risalto in base ai giudizi, e quelli sarebbero di ieri. Il bottone
        ' resta premibile — di là si va anche solo a guardare i documenti già scritti — ma
        ' qui si dice prima, perché qui c'è il rimedio: è il bottone accanto.
        If btnGeneraDocumenti.Enabled AndAlso rifareIlConfronto Then
            _suggerimenti.SetToolTip(btnGeneraDocumenti,
                "I giudizi sono quelli del profilo di prima, e sono loro a decidere cosa il CV " &
                "mette in risalto." & vbLf &
                "Prima si rifà il match: premi «⚠ Riconfronta» qui accanto, poi i documenti si " &
                "possono scrivere.")
        Else
            _suggerimenti.SetToolTip(btnGeneraDocumenti, Nothing)
        End If

        ' Ragionare ha senso quando c'è già un confronto: prima non ci sarebbe niente di
        ' cui parlare, e il prompt vuole i giudizi (cap. 12, A6.2). Qui l'AI serve davvero
        ' — a differenza dei documenti, che si possono riaprire anche senza — e su una
        ' candidatura scartata si tace, per la stessa ragione per cui non le si scrive un CV.
        btnBrainstorm.Enabled = Not occupato AndAlso conAi AndAlso
                                _opportunita IsNot Nothing AndAlso _opportunita.Confrontata AndAlso
                                _opportunita.Stato <> StatoOpportunita.Scartata

        ' Scartare si può finché c'è ancora una strada per lo scarto: è la macchina degli
        ' stati a dirlo (cap. 07.3), non un elenco di casi scritto qui.
        btnScarta.Enabled = Not occupato AndAlso _opportunita IsNot Nothing AndAlso
                            StatiOpportunita.Consentita(_opportunita.Stato, StatoOpportunita.Scartata)

        ' Com'è andata si segna dopo l'invio: prima non c'è niente che possa essere andato
        ' in un modo o nell'altro (cap. 07.3). Da «esito» il menù serve ancora, perché una
        ' dichiarazione si corregge — ed è l'unico posto da cui si torna indietro.
        btnEsito.Enabled = Not occupato AndAlso _opportunita IsNot Nothing AndAlso
                           (_opportunita.Stato = StatoOpportunita.Inviata OrElse
                            _opportunita.Stato = StatoOpportunita.Esito)

        If Not btnEsito.Enabled AndAlso Not occupato AndAlso _opportunita IsNot Nothing Then
            _suggerimenti.SetToolTip(btnEsito,
                                     "Si segna dopo aver spedito la candidatura: prima non c'è " &
                                     "ancora niente da registrare.")
        Else
            _suggerimenti.SetToolTip(btnEsito, Nothing)
        End If

    End Sub

End Class
