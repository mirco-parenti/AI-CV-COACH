Imports System.Diagnostics
Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore

''' <summary>
''' Pannello P6 — i documenti (cap. 03.6; cap. 12, A7): il 🎯 CV mirato e la ✉️ lettera
''' accanto all'annuncio da cui nascono, oppure il 📄 CV base accanto a una colonna vuota,
''' perché quello un annuncio non ce l'ha.
''' </summary>
''' <remarks>
''' <para><b>Nato intero nella struttura, riempito una tappa alla volta</b> (cap. 03.6):
''' anteprime ed esportazioni funzionavano dal primo giorno, «Prepara email» si è acceso
''' con T6, la scelta della lingua con T7 (il prima/dopo dell'anti-slop si accese lì e fu
''' tolto a T9d, e con lui la sua casella). Chi guarda l'applicazione a metà strada deve
''' vedere dove sta andando, e i comandi che ancora non fanno niente si mostrano spenti,
''' non nascosti.</para>
''' <para><b>Le anteprime passano dalla pagina di blocchi</b>, non dal JSON: le stampanti
''' sono tre — DOCX, PDF e questa, a video — e leggono tutte lo stesso modello (cap. 05.3).
''' Se l'anteprima leggesse il JSON per conto suo, mostrerebbe un documento che i file non
''' contengono, proprio dove l'utente controlla.</para>
''' <para><b>Rientrare non rigenera.</b> Un'opportunità che ha già i suoi documenti li
''' mostra e basta: rifarli costa un'attesa e dei token, e li cambierebbe sotto il naso di
''' chi li aveva già letti. A rifarli è «Rigenera», che lo dichiara. Da T7d la regola vale
''' anche per il 📄 CV base, che fino ad allora era l'unico documento a rinascere a ogni
''' visita — e l'unico che, senza AI, non si potesse più riesportare.</para>
''' </remarks>
Public Class PannelloDocumenti
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

    ''' <summary>Il titolo delle finestrelle di conferma: il nome che l'utente conosce.</summary>
    Private Const NomeProdotto As String = "TrovaLavoro"

    Private ReadOnly _suggerimenti As New ToolTip()

    Private _contesto As ContestoApp
    Private _pipeline As PipelineCandidatura
    Private _generatore As IGeneratore

    ''' <summary>La passata anti-slop sul 📄 CV base; <c>Nothing</c> quando l'AI non c'è.</summary>
    Private _rifinitura As Rifinitura

    ''' <summary>
    ''' Chi mette i documenti al loro posto. Lo costruisce la finestra, non il motore: si
    ''' porta dietro la stampante PDF, che vive sul thread dell'interfaccia.
    ''' </summary>
    Private _documenti As ArchivioDocumenti

    ''' <summary>La candidatura in mostra; <c>Nothing</c> se si sta guardando il 📄 CV base.</summary>
    Private _candidatura As Opportunita

    ''' <summary>Il 📄 CV base in mostra; <c>Nothing</c> se si sta guardando una candidatura.</summary>
    Private _cvBase As JsonNode

    ''' <summary>
    ''' Se il pannello sta sulla strada del 📄 CV base, anche quando un CV ancora non c'è
    ''' — lo si sta aspettando, o la generazione è andata storta.
    ''' </summary>
    ''' <remarks>
    ''' Prima bastava guardare <c>_cvBase</c>, e non bastava davvero: nell'attesa i due
    ''' campi sono <b>tutti e due</b> vuoti, e il pannello non sapeva più dire quale dei
    ''' due documenti stesse arrivando — così intitolava «🎯 CV mirato» la colonna di un
    ''' CV base, e teneva spento il «Rigenera» che era l'unico modo di riprovare dopo un
    ''' errore. La strada percorsa è un dato suo, e va tenuto (T7d).
    ''' </remarks>
    Private _sulCvBase As Boolean

    ''' <summary>
    ''' In che lingua è — o sarà — il 📄 CV base (T7d, cap. 10.3). Per una candidatura la
    ''' lingua è dell'opportunità e viaggia con lei; il CV base un'opportunità non ce l'ha,
    ''' e la sua lingua sta nel <c>cv_base.json</c> da cui si rilegge.
    ''' </summary>
    Private _linguaCvBase As String = LinguaDocumenti.Italiano

    ''' <summary>
    ''' Vero quando l'anti-slop è inciampato e il 📄 CV base mostrato è quello grezzo.
    ''' Va detto: un documento uscito senza la passata che il programma promette non deve
    ''' passare per un documento a cui non c'era niente da correggere (cap. 08.4).
    ''' </summary>
    Private _ilCvBaseNonERifinito As Boolean

    ''' <summary>
    ''' Da quale versione del profilo nasce il 📄 CV base in mostra, e quando fu scritto
    ''' (T9d, cap. 08.4).
    ''' </summary>
    ''' <remarks>
    ''' Prima non servivano: il CV base si salvava solo appena generato, e allora la
    ''' versione è l'ultima e il momento è adesso. Da quando i suoi testi si possono
    ''' riscrivere a mano lo si risalva anche <b>dopo</b>, e ripassare quei due valori
    ''' come se il CV nascesse ora vorrebbe dire riscriverne la storia: la provenienza
    ''' diventerebbe un profilo che non è quello da cui nacque, e la data direbbe «l'ho
    ''' scritto oggi» di un documento di ieri, proprio dove l'utente decide se fidarsene.
    ''' </remarks>
    Private _versioneDelCvBase As String

    ''' <inheritdoc cref="_versioneDelCvBase"/>
    Private _generatoIlCvBase As Date?

    ''' <summary>
    ''' I campi di prosa che l'utente ha riscritto a mano nel 📄 CV base in mostra (R7,
    ''' 2026-08-23): la copia in mano al pannello di quel che sta in <c>cv_base.json</c>.
    ''' </summary>
    ''' <remarks>
    ''' <para>Fino a R7 qui c'era <c>_modificatiAMano</c>, un booleano solo per tutto il
    ''' pannello e azzerato a ogni rientro. Serviva a dire, nella conferma di «Rigenera»,
    ''' che a sparire sarebbe stato anche il lavoro dell'utente — e lo diceva finché non si
    ''' cambiava pagina: dal rientro in poi la rigenerazione se lo portava via in silenzio.
    ''' Il collaudo dal vivo di T9e l'ha trovato così (R7).</para>
    ''' <para>La memoria adesso vive dove vive il documento: dentro
    ''' <see cref="Opportunita"/> per una candidatura, in <c>cv_base.json</c> per il CV
    ''' base — che in memoria un oggetto suo non ce l'ha, ed è la ragione di questo campo.
    ''' Il costo temuto quando si scelse il booleano non c'è stato: backup e ripristino
    ''' copiano i file grezzi (cap. 11.4) e i tool di lettura li restituiscono com'è
    ''' (cap. 09.3), quindi il blocco nuovo viaggia senza che nessuno dei due lo sappia.</para>
    ''' </remarks>
    Private ReadOnly _riscrittureDelCvBase As New RiscrittureAMano

    ''' <summary>
    ''' Le voci che l'utente ha lasciato fuori dal 📄 CV base (R6): la copia in mano al
    ''' pannello di quel che sta scritto in <c>cv_base.json</c>, che a differenza di una
    ''' candidatura non ha un oggetto suo in memoria.
    ''' </summary>
    Private ReadOnly _vociTolteDalCvBase As New VociTolte

    ''' <summary>
    ''' Vero mentre è il pannello a muovere la tendina della lingua, non l'utente.
    ''' </summary>
    ''' <remarks>
    ''' Senza, mostrare una candidatura scriverebbe sulla candidatura stessa la lingua che
    ''' le si sta leggendo addosso — e la scrittura, passando dal salvataggio, toccherebbe
    ''' il disco a ogni apertura. Un dato si legge o si scrive, non tutte e due nello
    ''' stesso gesto.
    ''' </remarks>
    Private _tendinaInAllestimento As Boolean

    Private _annulla As CancellationTokenSource

    ''' <summary>
    ''' Il comando che ha avviato l'attesa in corso: durante il lavoro è <b>lui</b> a fare
    ''' da «Annulla». <c>Nothing</c> quando non c'è nessuna attesa.
    ''' </summary>
    ''' <remarks>
    ''' Le attese di questo pannello sono tre — il 📄 CV base, i documenti della
    ''' candidatura, il riallineo della ✉️ lettera — e due comandi diversi le
    ''' rappresentano. Ricordarsi <i>quale</i> è la differenza fra un bottone che annulla
    ''' quel che si è chiesto e un bottone che annulla qualcos'altro: chi ha premuto
    ''' «Rigenera la lettera» deve ritrovare l'annullo lì, sotto lo stesso dito.
    ''' </remarks>
    Private _comandoDellAttesa As Button

    ''' <summary>
    ''' L'ultima cartella in cui si è esportato, per non ripartire dal Desktop a ogni giro.
    ''' È <b>condivisa</b> perché il pannello è uno per area ma la scelta è dell'utente, non
    ''' del documento che sta guardando, e vale finché l'applicazione resta aperta.
    ''' </summary>
    Private Shared _ultimaCartellaScelta As String

    ''' <summary>
    ''' L'utente vuole tornare da dov'era venuto. Dove sia, il pannello non lo sa — a P6
    ''' si arriva da due strade, l'opportunità (P4) e la scheda del profilo (P2), e a
    ''' ricordarsi quale è stata è la finestra, che è l'unica a saper navigare.
    ''' </summary>
    Public Event TornaIndietro As EventHandler

    ''' <summary>
    ''' L'utente vuole preparare l'email di questa candidatura: si va in P7 (cap. 12, A8).
    ''' Come sempre il pannello non conosce nessuno — dice cosa vuole, e chi sa navigare
    ''' se ne occupa.
    ''' </summary>
    Public Event EmailRichiesta As EventHandler

    ''' <summary>L'AI ha cominciato o finito di lavorare: la barra si blocca (cap. 02.6).</summary>
    Public Event LavoroAiCambiato As EventHandler

    Public Sub New()

        InitializeComponent()

        ' La cornice sta addosso alla tendina, e la misura giusta si sa solo adesso:
        ' l'altezza di una ComboBox la impone il carattere — a DPI diversi cambia — e una
        ' cornice disegnata a occhio nel progettista lascerebbe una banda blu scoperta.
        pnlCorniceDocumento.Size = New Size(cmbDocumento.Width + 4, cmbDocumento.Height + 4)

        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        VestiIBottoni()
        DichiaraLeTappeCheMancano()
        AggiornaComandi()

    End Sub

    ''' <summary>Collega il pannello al motore e a chi sa scrivere i file.</summary>
    ''' <param name="contesto">Il motore montato all'avvio.</param>
    ''' <param name="documenti">
    ''' Chi scrive DOCX e PDF: lo costruisce la finestra, perché la stampante PDF vuole il
    ''' thread dell'interfaccia. Senza stampante si scrivono i soli DOCX, e il pannello lo
    ''' dice quando succede.
    ''' </param>
    ''' <param name="pipeline">Chi genera i documenti di una candidatura; il banco passa la sua.</param>
    ''' <param name="generatore">Chi scrive il 📄 CV base, che dalla pipeline non passa.</param>
    ''' <param name="rifinitura">
    ''' La passata anti-slop sul 📄 CV base (cap. 08): per i documenti di una candidatura
    ''' ci pensa la pipeline, questo è l'unico testo che nasce qui.
    ''' </param>
    Public Sub Collega(contesto As ContestoApp, documenti As ArchivioDocumenti,
                       Optional pipeline As PipelineCandidatura = Nothing,
                       Optional generatore As IGeneratore = Nothing,
                       Optional rifinitura As Rifinitura = Nothing)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
        If documenti Is Nothing Then Throw New ArgumentNullException(NameOf(documenti))

        _contesto = contesto
        _documenti = documenti
        _pipeline = If(pipeline, contesto.Pipeline)
        _generatore = If(generatore, contesto.Generatore)
        _rifinitura = If(rifinitura, contesto.Rifinitura)

        AggiornaComandi()

    End Sub

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

    ''' <summary>
    ''' Toglie dalla vista il 📄 CV base, perché il profilo da cui nasceva è stato
    ''' eliminato (cap. 11.5) e il suo file non c'è più.
    ''' </summary>
    ''' <remarks>
    ''' I documenti di una <b>candidatura</b> non si toccano, e non è una dimenticanza:
    ''' quelli sono suoi, stanno nella sua cartella e l'eliminazione del profilo non li
    ''' riguarda. Qui sparisce solo ciò che del profilo era il ritratto.
    ''' </remarks>
    Public Sub DimenticaIlCvBase()

        If AiAlLavoro OrElse _cvBase Is Nothing Then Return

        ' Si lascia anche la <i>strada</i> del CV base, non solo il documento: restando su
        ' di essa la colonna direbbe «non è ancora stato scritto» — che è falso, era
        ' scritto — proprio mentre lo stato racconta che è stato eliminato. Qui non c'è
        ' più niente da guardare, e il pannello torna vuoto.
        _sulCvBase = False
        LasciaIlCvBase()
        Mostra()
        RaccontaLoStato("Il profilo è stato eliminato: il 📄 CV base che era qui non c'è più.",
                        StileApp.TestoSecondario)

    End Sub

    ''' <summary>
    ''' Lascia andare i documenti in mostra se erano di una candidatura appena eliminata
    ''' (cap. 11.5), e dice se l'ha fatto.
    ''' </summary>
    ''' <remarks>
    ''' Il gemello di <see cref="DimenticaIlCvBase"/> preso dal capo opposto: lì spariva il
    ''' profilo e i documenti delle candidature restavano, qui sparisce una candidatura e
    ''' resta il 📄 CV base. La ragione però è più stringente di una vista da ripulire:
    ''' «Rigenera» e le esportazioni scrivono <b>nella cartella della candidatura</b>, e su
    ''' un documento sopravvissuto alla sua cartella ricreerebbero quel che è stato appena
    ''' cancellato.
    ''' </remarks>
    Public Function Dimentica(cartella As String) As Boolean

        If AiAlLavoro OrElse _candidatura Is Nothing Then Return False

        If Not String.Equals(_candidatura.Cartella, cartella, StringComparison.OrdinalIgnoreCase) Then
            Return False
        End If

        _candidatura = Nothing
        _sulCvBase = False

        AllestisciLaTendinaDeiDocumenti()
        Mostra()

        RaccontaLoStato("La candidatura di cui erano questi documenti è stata eliminata.",
                        StileApp.TestoSecondario)

        Return True

    End Function

    ''' <summary>
    ''' Lascia andare il 📄 CV base che era in mostra: il documento, le riscritture a mano e
    ''' la provenienza che gli apparteneva.
    ''' </summary>
    ''' <remarks>
    ''' Stanno insieme perché si perdono insieme: una provenienza sopravvissuta al suo
    ''' documento finirebbe addosso al prossimo, che nasce da un altro profilo e in un
    ''' altro momento.
    ''' </remarks>
    Private Sub LasciaIlCvBase()

        _cvBase = Nothing
        _versioneDelCvBase = Nothing
        _generatoIlCvBase = Nothing
        _riscrittureDelCvBase.Dimentica()
        _vociTolteDalCvBase.Dimentica()

    End Sub

    ' ==================================================================
    ' I due modi di arrivare qui
    ' ==================================================================

    ''' <summary>
    ''' Mostra i documenti di una candidatura, generandoli se non ci sono ancora. È la
    ''' strada che arriva da P4 (cap. 12, A7).
    ''' </summary>
    Public Async Function MostraLaCandidaturaAsync(candidatura As Opportunita) As Task

        If candidatura Is Nothing Then Throw New ArgumentNullException(NameOf(candidatura))
        If AiAlLavoro Then Return

        _candidatura = candidatura
        _sulCvBase = False
        LasciaIlCvBase()

        AllestisciLaTendinaDeiDocumenti()
        Mostra()

        ' Già generati: si guardano e basta. A rifarli c'è «Rigenera», che lo dichiara.
        If candidatura.Cv IsNot Nothing AndAlso candidatura.Lettera IsNot Nothing Then

            ' Salvo quando la lettera è rimasta indietro (R7): allora la prima cosa da
            ' dire è quella, perché è l'unica che l'utente non può vedere da sé — le due
            ' colonne sono lì, tutt'e due piene, e sembrano d'accordo.
            If candidatura.LetteraDaRiallineare Then
                RaccontaUnAvviso("Il 🎯 CV l'hai riscritto tu: la ✉️ lettera racconta ancora la storia di prima." & vbLf &
                                 "Con «Rigenera la lettera» la riscrivo su quello che hai lasciato tu.")
                Return
            End If

            RaccontaLoStato("Rileggili con calma: se qualcosa non ti convince, puoi rigenerarli.",
                            StileApp.TestoSecondario)
            Return
        End If

        Await GeneraLaCandidaturaAsync().ConfigureAwait(True)

        ' Come per il CV base: quello che è appena stato scritto va in elenco.
        AllestisciLaTendinaDeiDocumenti()

    End Function

    ''' <summary>
    ''' Mostra il 📄 CV base, generandolo se non ce n'è ancora uno: dal solo profilo, senza
    ''' alcun annuncio. È la strada che arriva dalla scheda P2 (cap. 12.2, punto 5).
    ''' </summary>
    ''' <remarks>
    ''' <b>Rientrare non rigenera</b>, qui come per una candidatura (v. in testa alla
    ''' classe). Fino a T7d questa metà della regola non c'era: il CV base viveva nella sola
    ''' memoria del pannello, così ogni rientro ne riscriveva uno nuovo — un'altra attesa,
    ''' altri token, e un testo diverso da quello che l'utente aveva già approvato ed
    ''' esportato. Il <c>cv_base.json</c> stava lì accanto dal primo giorno.
    ''' </remarks>
    Public Async Function MostraIlCvBaseAsync() As Task

        If AiAlLavoro Then Return

        _candidatura = Nothing
        _sulCvBase = True
        LasciaIlCvBase()
        ' Un 📄 CV-1 base che nasce adesso parte dalla lingua preferita (cap. 03, P8);
        ' uno già salvato tiene la sua, che si rilegge poco più giù.
        _linguaCvBase = If(_contesto Is Nothing,
                           LinguaDocumenti.Italiano, _contesto.Impostazioni.LinguaPredefinita)

        AllestisciLaTendinaDeiDocumenti()
        Mostra()

        If RipescaIlCvBase() Then Return

        Await GeneraIlCvBaseAsync().ConfigureAwait(True)

        ' Un documento appena nato è un documento in più da elencare: la tendina va
        ' rifatta adesso, o resterebbe indietro proprio su quello che si sta guardando.
        AllestisciLaTendinaDeiDocumenti()

    End Function

    ''' <summary>
    ''' Mette in mostra il 📄 CV base già scritto, se ce n'è uno, senza chiamare l'AI.
    ''' </summary>
    ''' <returns>
    ''' Vero se il pannello ha già di che mostrare — o di che spiegare — e non c'è niente
    ''' da generare.
    ''' </returns>
    Private Function RipescaIlCvBase() As Boolean

        If _contesto Is Nothing OrElse Not _contesto.Archivio.Esiste Then Return False

        Dim salvato As Dati.CvBase

        Try
            salvato = _contesto.Archivio.CaricaCvBase()

        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                   OrElse TypeOf ex Is UnauthorizedAccessException
            ' Come per il profilo (cap. 11.1): un file che non si lascia leggere si
            ' dichiara, non si scavalca in silenzio rigenerando — chi legge deve sapere
            ' che su disco c'era qualcosa.
            RaccontaUnErrore($"Il 📄 CV base c'è ma non si lascia leggere: {ex.Message}" & vbLf &
                             "Con «Rigenera» lo riscrivo da capo.")
            Return True
        End Try

        If salvato Is Nothing OrElse salvato.Cv Is Nothing Then Return False

        _cvBase = salvato.Cv
        _linguaCvBase = LinguaDocumenti.PerDocumenti(salvato.Lingua)

        ' Da dove viene e di quando è: si tengono perché il CV può essere risalvato dopo
        ' — la modifica a mano dei suoi testi — e quei due valori sono la sua storia, non
        ' quella del salvataggio (cap. 08.4).
        _versioneDelCvBase = salvato.VersioneProfilo
        _generatoIlCvBase = If(salvato.Generato = Nothing, CType(Nothing, Date?), salvato.Generato)

        ' E con loro quel che l'utente ci aveva riscritto a mano (R7): senza, «Rigenera»
        ' tornerebbe a portarselo via in silenzio a ogni rientro in P6.
        _riscrittureDelCvBase.Prendi(salvato.Riscritture)
        _vociTolteDalCvBase.Prendi(salvato.Tolte)

        Mostra()
        RaccontaLoStato(DaDoveViene(salvato), StileApp.TestoSecondario)

        Return True

    End Function

    ''' <summary>
    ''' Di quale profilo è il ritratto il CV che si sta guardando, e di quando.
    ''' </summary>
    ''' <remarks>
    ''' Se il profilo è cambiato dopo, il pannello lo <b>dice</b> invece di rigenerare di
    ''' soppiatto: è la promessa scritta sopra <see cref="Dati.CvBase"/>, ed è la scelta
    ''' dell'utente: quel CV potrebbe essere quello che ha già spedito.
    ''' </remarks>
    Private Function DaDoveViene(salvato As Dati.CvBase) As String

        Dim quando As String = If(salvato.Generato = Nothing, "prima d'ora",
                                  "il " & salvato.Generato.ToString("d MMMM yyyy",
                                                                    CultureInfo.CurrentCulture))

        Dim testo As String = $"Questo 📄 CV base l'ho scritto {quando}{InQuestaLingua()}."

        If ProfiloCambiatoDopo(salvato.VersioneProfilo) Then
            Return testo & vbLf &
                   "Da allora hai cambiato il profilo: con «Rigenera» lo riscrivo su quello di oggi."
        End If

        Return testo & vbLf & "Puoi esportarlo così com'è, o riscriverlo con «Rigenera»."

    End Function

    ''' <summary>
    ''' Perché questa candidatura non si può riscrivere, o <c>Nothing</c> se si può: il
    ''' profilo con cui fu confrontata non c'è più.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Cosa succedeva senza.</b> Alla generazione arrivano tre cose: il profilo
    ''' di <i>oggi</i>, il CV della candidatura e i giudizi del suo confronto — questi due
    ''' di <i>allora</i>. Se il profilo di allora è stato eliminato e rifatto, i tre pezzi
    ''' parlano di due persone diverse, e il modello risponde con delle spiegazioni invece
    ''' che col documento chiesto: quel che l'utente leggeva era «l'AI ha risposto in una
    ''' forma che non riesco a leggere», che manda a cercare il guasto dalla parte
    ''' sbagliata.</para>
    ''' <para><b>Perché la versione mancante e non quella diversa.</b> Un profilo che
    ''' <i>cresce</i> cambia versione a ogni salvataggio, e con lui i vecchi documenti
    ''' restano spiegabili: fermarsi lì sarebbe un avviso a ogni giro, per un caso che
    ''' funziona (v. <see cref="Dati.ArchivioProfilo.CELaVersione"/>).</para>
    ''' <para><b>Perché non si propone di rifare il confronto.</b> Perché non si può: una
    ''' candidatura già confrontata non si riconfronta, e l'unica strada onesta è rifarla
    ''' dal suo annuncio. Indicare un gesto che non esiste sarebbe peggio del silenzio.</para>
    ''' </remarks>
    Private Function MotivoProfiloSparito() As String

        If _sulCvBase OrElse _candidatura Is Nothing OrElse _contesto Is Nothing Then Return Nothing
        If _contesto.Archivio.CELaVersione(_candidatura.VersioneProfilo) Then Return Nothing

        Return "Questa candidatura è stata confrontata con un profilo che adesso non c'è più: " &
               "quello di allora è stato eliminato, e i giudizi e i documenti raccontano ancora lui." &
               vbLf &
               "Riscriverli sul profilo di oggi mescolerebbe due storie diverse. Per averne di " &
               $"nuovi, rifai la candidatura da «{NomiUi.Confronto}», incollando di nuovo l'annuncio."

    End Function

    ''' <summary>Se il profilo ha avuto altre versioni dopo quella da cui nacque il CV.</summary>
    Private Function ProfiloCambiatoDopo(versioneDelCv As String) As Boolean

        If String.IsNullOrEmpty(versioneDelCv) Then Return False

        Dim ultima As String = _contesto.Archivio.Versioni().LastOrDefault()
        If String.IsNullOrEmpty(ultima) Then Return False

        Return Not String.Equals(ultima, versioneDelCv, StringComparison.Ordinal)

    End Function

    ''' <summary>Scrive il 📄 CV base da capo, nella lingua scelta.</summary>
    Private Async Function GeneraIlCvBaseAsync() As Task

        If _generatore Is Nothing Then
            RaccontaUnAvviso(MotivoSenzaAi())
            Return
        End If

        Dim profilo As Profilo = LeggiIlProfilo()
        If profilo Is Nothing Then Return

        Using filo As New CancellationTokenSource()

            _annulla = filo
            LavoroInCorso(True, btnRigenera)

            Try
                RaccontaLoStato($"Scrivo il tuo 📄 CV base{InQuestaLingua()}…", StileApp.TestoSecondario)

                _cvBase = Await _generatore.GeneraCvBaseAsync(
                    JsonNode.Parse(profilo.ComeTesto()), filo.Token, _linguaCvBase).ConfigureAwait(True)

                ' Il CV di prima non c'è più, e con lui le riscritture a mano che aveva.
                ' Si dimenticano qui e non prima di chiamare: se la generazione fosse
                ' andata storta, a video sarebbe rimasto quello vecchio — con dentro il
                ' lavoro dell'utente — e la prossima conferma avrebbe smesso di nominarlo.
                ' Le voci lasciate fuori invece restano: togliere una voce non è un
                ' testo vecchio da incollare in un documento nuovo, è una scelta su cosa
                ' questo CV deve dire — e vale anche sul CV rifatto (R6).
                _riscrittureDelCvBase.Dimentica()

                Await RifinisciIlCvBaseAsync(filo.Token).ConfigureAwait(True)

                Mostra()
                RaccontaLoStato(ArchiviaIlCvBase(), StileApp.TestoSecondario)

            Catch ex As OperationCanceledException
                RaccontaLoStato("Generazione annullata: non ho scritto niente.", StileApp.TestoSecondario)

            Catch ex As ErroreAi
                RaccontaUnErrore(ex.Message)

            Finally
                _annulla = Nothing
                LavoroInCorso(False)
            End Try

        End Using

    End Function

    ''' <summary>
    ''' «in inglese» quando lo è, niente quando è la lingua di casa: l'italiano è il
    ''' predefinito e dirlo ogni volta sarebbe rumore.
    ''' </summary>
    Private Function InQuestaLingua() As String

        Return If(_linguaCvBase = LinguaDocumenti.Inglese, " in inglese", String.Empty)

    End Function

    ''' <summary>
    ''' I due passi della generazione: il 🎯 CV mirato e la ✉️ lettera, in quest'ordine
    ''' perché la lettera riceve il CV e deve raccontare la stessa storia.
    ''' </summary>
    Private Async Function GeneraLaCandidaturaAsync() As Task

        If _pipeline Is Nothing Then
            RaccontaUnAvviso(MotivoSenzaAi())
            Return
        End If

        ' L'ultimo cancello prima dell'AI, e vale anche per la generazione che parte da
        ' sola aprendo una candidatura senza documenti: quelli di «Rigenera» e della
        ' lingua stanno più a monte perché là c'è dell'altro da non fare.
        Dim sparito As String = MotivoProfiloSparito()
        If sparito IsNot Nothing Then
            RaccontaUnAvviso(sparito)
            Return
        End If

        Dim profilo As Profilo = LeggiIlProfilo()
        If profilo Is Nothing Then Return

        Using filo As New CancellationTokenSource()

            _annulla = filo
            LavoroInCorso(True, btnRigenera)

            Try
                Dim avanzamento As New Progress(Of AvanzamentoPipeline)(
                    Sub(passo) RaccontaLoStato($"{passo.Cosa}… ({passo.Passo} di {passo.Totale})",
                                               StileApp.TestoSecondario))

                Await _pipeline.GeneraAsync(_candidatura, profilo, avanzamento, filo.Token).ConfigureAwait(True)

                Mostra()
                RaccontaLoStato(ArchiviaLaCandidatura(), StileApp.TestoSecondario)

            Catch ex As OperationCanceledException
                ' Quel che era già arrivato resta nell'opportunità: se il CV c'era e la
                ' lettera no, si riprende da lì invece di rifare tutto.
                Mostra()
                RaccontaLoStato("Generazione annullata: quello che era già pronto è rimasto.",
                                StileApp.TestoSecondario)

            Catch ex As ErroreAi
                Mostra()
                RaccontaUnErrore(ex.Message & vbLf & "Puoi riprovare con «Rigenera».")

            Finally
                _annulla = Nothing
                LavoroInCorso(False)
            End Try

        End Using

    End Function

    ''' <summary>Scrive l'opportunità aggiornata e racconta com'è andata.</summary>
    Private Function ArchiviaLaCandidatura() As String

        Try
            _contesto.Opportunita.Salva(_candidatura)
            AnnotaNelRegistro()
            Return "CV e lettera sono pronti, e li ho salvati con la candidatura." & vbLf &
                   "Esportali in DOCX o PDF quando ti vanno bene."

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Return $"CV e lettera sono pronti, ma non sono riuscita a salvarli ({ex.Message})."
        End Try

    End Function

    ''' <summary>
    ''' Tiene in riga la vista d'insieme dopo che la cartella è stata scritta: da qui la
    ''' candidatura passa a «generata», ed è quello che la Home deve mostrare (cap. 07.3).
    ''' </summary>
    ''' <remarks>
    ''' Come in P4, il suo <c>Try</c> è separato: un indice che non si lascia scrivere non
    ''' è un documento perso, e dirlo come se lo fosse sarebbe un falso allarme.
    ''' </remarks>
    Private Sub AnnotaNelRegistro()

        Try
            _contesto.Registro.Annota(_candidatura)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
        End Try

    End Sub

    ''' <summary>
    ''' La passata anti-slop sul 📄 CV base (cap. 08), che dalla pipeline non passa.
    ''' </summary>
    ''' <remarks>
    ''' Stessa regola che vale dentro la fila di una candidatura: un inciampo della
    ''' rifinitura non butta via un CV già scritto. Resta quello grezzo, che è buono — solo
    ''' non rifinito. L'annullamento invece passa: è l'utente che ha chiesto di smettere.
    ''' </remarks>
    ''' <returns>I testi di prima, o <c>Nothing</c> se non è cambiato niente.</returns>
    Private Async Function RifinisciIlCvBaseAsync(annulla As CancellationToken) As Task(Of JsonNode)

        _ilCvBaseNonERifinito = False

        If _rifinitura Is Nothing Then Return Nothing

        RaccontaLoStato("Rifinisco il testo del 📄 CV base…", StileApp.TestoSecondario)

        Try
            ' Nella lingua del CV, non in quella di casa: l'anti-slop ha le sue varianti
            ' (cap. 04.6), e passargli l'italiano su un testo inglese vorrebbe dire
            ' correggere una prosa con le regole di un'altra lingua.
            Return Await _rifinitura.DelCvAsync(
                _cvBase, _linguaCvBase, annulla).ConfigureAwait(True)

        Catch ex As ErroreAi
            ' Come nella pipeline (cap. 08.6): non si tace. Il CV c'è lo stesso, ed è
            ' buono — solo non rifinito — ma a dirlo dev'essere lei, non il silenzio.
            _ilCvBaseNonERifinito = True
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' La riga da aggiungere in coda al racconto quando l'anti-slop è inciampato, con
    ''' le stesse parole della pipeline (cap. 08.6); vuota quando non c'è niente da dire.
    ''' </summary>
    Private Function LaRifinituraSeENonRiuscita() As String

        If Not _ilCvBaseNonERifinito Then Return String.Empty

        Return vbLf & "La rifinitura non è riuscita: tengo il testo com'è."

    End Function

    ''' <summary>
    ''' La riga da aggiungere a una conferma che sostituisce i testi, quando fra quelli
    ''' che spariscono c'è anche il lavoro dell'utente; vuota quando non ne ha fatto.
    ''' </summary>
    ''' <remarks>
    ''' <para>«Vengono sostituiti» è vero lo stesso, ma di un testo scritto dall'AI si sa
    ''' che si rifà premendo di nuovo, e di uno scritto a mano no: è l'unico caso in cui il
    ''' costo di un sì non è un'attesa, è del lavoro perso (cap. 03.3, livello 4).</para>
    ''' <para><b>Da R7 dice quali</b>, e li legge dai file invece che dalla sessione. Prima
    ''' era una riga sola e generica, e soprattutto scadeva: bastava tornare al profilo e
    ''' rientrare perché il pannello dimenticasse di averlo scritto, e da lì in poi
    ''' «Rigenera» non nominava più niente. Un elenco costa una riga in più nella domanda e
    ''' vale un ripensamento: «Sommario, Esperienza 2» è un lavoro che si riconosce, «le
    ''' modifiche che hai fatto» no.</para>
    ''' </remarks>
    ''' <remarks>
    ''' È pubblica per la stessa ragione di <see cref="ConfermaLeRiscrittureAsync"/>: il
    ''' banco un <c>MessageBox</c> non lo sa aprire né leggere, e questa riga è la sola
    ''' parte di quella domanda che possa sbagliare.
    ''' </remarks>
    Public Function AncheQuelliRiscrittiAMano() As String

        Dim campi As New List(Of String)

        If _sulCvBase Then
            campi.AddRange(_riscrittureDelCvBase.Campi)
        ElseIf _candidatura IsNot Nothing Then
            campi.AddRange(_candidatura.RiscrittureDelCv.Campi)
            campi.AddRange(_candidatura.RiscrittureDellaLettera.Campi)
        End If

        Dim detto As String = String.Empty

        If campi.Count > 0 Then
            ' Come si leggono lo sa Rifinitura, che è l'unico posto a conoscere i campi di
            ' prosa: un secondo elenco di nomi qui divergerebbe al primo campo nuovo.
            Dim nomi As String = String.Join(", ", campi.Select(AddressOf Rifinitura.ComeSiLegge))
            detto &= vbLf & "Compresi i testi che hai riscritto a mano: " & nomi & "."
        End If

        ' Le voci lasciate fuori non si perdono — restano fuori anche dal CV rifatto (R6)
        ' — ma chi sta per rigenerare deve saperlo lo stesso: il documento nuovo non sarà
        ' il CV intero, e senza questa riga sembrerebbe che manchi qualcosa.
        Dim quante As Integer = VociLasciateFuori().Impronte.Count

        If quante > 0 Then
            detto &= vbLf & If(quante = 1,
                               "La voce che hai lasciato fuori resta fuori anche dal CV rifatto.",
                               $"Le {quante} voci che hai lasciato fuori restano fuori anche dal CV rifatto.")
        End If

        Return detto

    End Function

    ''' <summary>Le voci lasciate fuori dal documento in mostra adesso (R6).</summary>
    Private Function VociLasciateFuori() As VociTolte

        If _sulCvBase Then Return _vociTolteDalCvBase
        If _candidatura IsNot Nothing Then Return _candidatura.VociTolteDalCv

        Return New VociTolte()

    End Function

    ''' <summary>Scrive il 📄 CV base accanto al profilo, annotando da quale versione nasce.</summary>
    Private Function ArchiviaIlCvBase() As String

        Try
            ' La versione e il momento si tengono anche in memoria: da qui in avanti il CV
            ' può essere risalvato senza rinascere (cap. 08.4), e allora vanno ripassati
            ' questi e non quelli di allora.
            _versioneDelCvBase = _contesto.Archivio.Versioni().LastOrDefault()
            _generatoIlCvBase = Date.Now

            _contesto.Archivio.SalvaCvBase(_cvBase, _versioneDelCvBase,
                                           _linguaCvBase, _generatoIlCvBase, _riscrittureDelCvBase,
                                           _vociTolteDalCvBase)
            Return $"Il 📄 CV base è pronto{InQuestaLingua()} ed è salvato col tuo profilo." & vbLf &
                   "Esportalo in DOCX o PDF quando ti va bene." & LaRifinituraSeENonRiuscita()

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Return $"Il 📄 CV base è pronto, ma non sono riuscita a salvarlo ({ex.Message})." &
                   LaRifinituraSeENonRiuscita()
        End Try

    End Function

    ' ==================================================================
    ' Esportare
    ' ==================================================================

    ''' <summary>
    ''' Scrive i file nel formato chiesto, ne mette una copia dove dice l'utente e racconta
    ''' dove sono finiti. Restituisce i file su cui l'utente può mettere le mani.
    ''' </summary>
    ''' <param name="formati">Quali file scrivere: solo DOCX, solo PDF o entrambi.</param>
    ''' <param name="destinazione">
    ''' La cartella scelta dall'utente, dove va la copia; <c>Nothing</c> per fermarsi alla
    ''' cartella della candidatura — ed è così che lo chiama il banco, che una finestra di
    ''' scelta cartella non la sa chiudere.
    ''' </param>
    ''' <param name="confermaSostituzione">
    ''' Chi chiede il permesso di sostituire i file che in quella cartella ci sono già,
    ''' ricevendone i nomi; <c>Nothing</c> vuol dire sostituirli senza chiedere.
    ''' </param>
    ''' <remarks>
    ''' <para><b>I file nascono sempre nella cartella della candidatura</b> (cap. 05.6), e
    ''' solo dopo se ne copia una coppia dove l'utente li vuole. Di lì li prende l'email di
    ''' P7, che gli allegati li legge da quella cartella e non da altrove: portare i file
    ''' via invece di copiarli lascerebbe l'email senza niente da allegare.</para>
    ''' <para><b>La scelta della cartella e l'apertura di Esplora risorse stanno fuori di
    ''' qui</b> — le fa <see cref="EsportaConSceltaAsync"/> — per lo stesso taglio di
    ''' <see cref="ModificaITesti"/>: quel che ha dentro una finestra modale il banco non
    ''' lo può provare, quindi la parte che vale la pena provare non ne ha nessuna.</para>
    ''' <para><b>Chi rifiuta la sostituzione non perde niente</b>: i documenti appena
    ''' scritti restano nella cartella della candidatura, e il messaggio lo dice. È l'unico
    ''' caso in cui si torna a mani vuote avendo comunque lavorato.</para>
    ''' </remarks>
    Public Async Function EsportaAsync(formati As FormatiDocumento,
                                       Optional destinazione As String = Nothing,
                                       Optional confermaSostituzione As Func(Of IReadOnlyList(Of String), Boolean) = Nothing) _
                                       As Task(Of IReadOnlyList(Of String))

        Dim niente As IReadOnlyList(Of String) = New List(Of String)()

        If _documenti Is Nothing OrElse Not CEQualcosaDaEsportare() Then Return niente

        Cursor = Cursors.AppStarting

        Try
            Dim scritti As IReadOnlyList(Of String) = Await ScriviAsync(formati).ConfigureAwait(True)

            If scritti.Count = 0 Then
                RaccontaLoStato("Non c'era ancora niente da esportare.", StileApp.TestoSecondario)
                Return niente
            End If

            Dim finiti As IReadOnlyList(Of String) = CopiaDove(scritti, destinazione, confermaSostituzione)

            If finiti.Count = 0 Then
                RaccontaLoStato(
                    "Non ho sostituito niente: i file che c'erano sono rimasti come stavano." & vbLf &
                    $"I documenti aggiornati sono in «{Path.GetDirectoryName(scritti(0))}».",
                    StileApp.TestoSecondario)
                Return niente
            End If

            RaccontaLoStato(
                "Ho scritto:" & vbLf & String.Join(vbLf, finiti.Select(AddressOf Path.GetFileName)) & vbLf &
                $"in «{Path.GetDirectoryName(finiti(0))}».",
                StileApp.TestoSecondario)

            Return finiti

        Catch ex As ErroreStampa
            ' Il messaggio si porta dietro il ripiego onesto: il DOCX c'è comunque
            ' (cap. 13.3).
            RaccontaUnErrore(ex.Message)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            RaccontaUnErrore($"Non sono riuscita a scrivere i file: {ex.Message}")

        Finally
            Cursor = Cursors.Default
        End Try

        Return niente

    End Function

    ''' <summary>
    ''' Porta una copia dei file appena scritti nella cartella scelta e restituisce i
    ''' percorsi finali: quelli copiati, o gli originali se non c'è nessuna cartella dove
    ''' copiarli. Elenco vuoto se l'utente ha negato la sostituzione.
    ''' </summary>
    Private Shared Function CopiaDove(scritti As IReadOnlyList(Of String),
                                      destinazione As String,
                                      confermaSostituzione As Func(Of IReadOnlyList(Of String), Boolean)) _
                                      As IReadOnlyList(Of String)

        If String.IsNullOrWhiteSpace(destinazione) Then Return scritti

        Directory.CreateDirectory(destinazione)

        ' Esportare dentro la cartella dove i file sono appena nati non è un errore: è la
        ' stessa cartella, e copiare un file su se stesso fallirebbe. Non c'è niente da
        ' fare, e i percorsi buoni sono già quelli.
        If String.Equals(Path.GetFullPath(Path.GetDirectoryName(scritti(0))).TrimEnd(Path.DirectorySeparatorChar),
                         Path.GetFullPath(destinazione).TrimEnd(Path.DirectorySeparatorChar),
                         StringComparison.OrdinalIgnoreCase) Then Return scritti

        Dim gia As List(Of String) = scritti.
            Select(Function(f) Path.GetFileName(f)).
            Where(Function(nome) File.Exists(Path.Combine(destinazione, nome))).
            ToList()

        If gia.Count > 0 AndAlso confermaSostituzione IsNot Nothing AndAlso Not confermaSostituzione(gia) Then
            Return New List(Of String)()
        End If

        Dim copiati As New List(Of String)

        For Each origine As String In scritti
            Dim arrivo As String = Path.Combine(destinazione, Path.GetFileName(origine))
            File.Copy(origine, arrivo, overwrite:=True)
            copiati.Add(arrivo)
        Next

        Return copiati

    End Function

    ''' <summary>
    ''' L'esportazione come la vede l'utente: chiede dove salvare, scrive, e apre Esplora
    ''' risorse sui file appena fatti. È il metodo che premono i due bottoni.
    ''' </summary>
    ''' <remarks>
    ''' <b>Aprire la cartella alla fine è il pezzo che mancava</b> (cap. 05.6, il bottone
    ''' «Apri cartella» promesso e mai fatto): finché l'unico segno dell'esportazione era
    ''' una riga di testo in fondo al pannello, premere «Esporta» sembrava non fare niente,
    ''' e i file finivano in una cartella che l'utente non aveva scelto e non sapeva
    ''' trovare. Ora li sceglie lui e se li vede davanti.
    ''' </remarks>
    Public Async Function EsportaConSceltaAsync(formati As FormatiDocumento) As Task

        If _documenti Is Nothing OrElse Not CEQualcosaDaEsportare() Then Return

        Dim dove As String = ScegliDoveSalvare()
        If String.IsNullOrEmpty(dove) Then Return

        Dim finiti As IReadOnlyList(Of String) =
            Await EsportaAsync(formati, dove, AddressOf SostituiscoIFileCheCiSono).ConfigureAwait(True)

        If finiti.Count > 0 Then MostraNellEsploraRisorse(finiti(0))

    End Function

    ''' <summary>
    ''' Chiede all'utente dove salvare i documenti; <c>Nothing</c> se ha rinunciato.
    ''' </summary>
    ''' <remarks>
    ''' Si parte dal <b>Desktop</b> — dove chi esporta un CV lo vuole, e dove lo ritrova
    ''' senza cercarlo — e dalla seconda volta in poi dall'ultima cartella scelta. È un
    ''' ricordo della sessione, non una preferenza: le preferenze sono quelle che l'utente
    ''' dichiara in P8 (v. <see cref="Impostazioni"/>), e questa non l'ha mai dichiarata,
    ''' l'ha solo usata.
    ''' </remarks>
    Private Function ScegliDoveSalvare() As String

        Using dialogo As New FolderBrowserDialog()

            dialogo.Description = "Scegli dove salvare i documenti."
            dialogo.UseDescriptionForTitle = True
            dialogo.ShowNewFolderButton = True
            dialogo.SelectedPath = DaDoveSiParte()

            If dialogo.ShowDialog(Me) <> DialogResult.OK Then Return Nothing

            _ultimaCartellaScelta = dialogo.SelectedPath
            Return dialogo.SelectedPath

        End Using

    End Function

    ''' <summary>La cartella su cui si apre la scelta: l'ultima usata, o il Desktop.</summary>
    Private Shared Function DaDoveSiParte() As String

        If Not String.IsNullOrEmpty(_ultimaCartellaScelta) AndAlso
           Directory.Exists(_ultimaCartellaScelta) Then Return _ultimaCartellaScelta

        Return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)

    End Function

    ''' <summary>Chiede se sostituire i file che nella cartella scelta ci sono già.</summary>
    Private Function SostituiscoIFileCheCiSono(nomi As IReadOnlyList(Of String)) As Boolean

        Return MessageBox.Show(
            "In quella cartella ci sono già:" & vbLf &
            String.Join(vbLf, nomi) & vbLf & vbLf &
            "Li sostituisco con quelli nuovi?",
            NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) = DialogResult.Yes

    End Function

    ''' <summary>
    ''' Apre Esplora risorse sulla cartella dei file esportati, col primo già selezionato.
    ''' </summary>
    ''' <remarks>
    ''' Se non si apre non è successo niente di grave — i file ci sono lo stesso e il
    ''' messaggio dice dove — quindi un guasto qui non deve arrivare all'utente come un
    ''' errore dell'esportazione, che invece è andata a buon fine.
    ''' </remarks>
    Private Shared Sub MostraNellEsploraRisorse(percorso As String)

        Try
            Process.Start(New ProcessStartInfo("explorer.exe", $"/select,""{percorso}""") With {
                .UseShellExecute = True})

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is System.ComponentModel.Win32Exception OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' Nessuna parola: l'esportazione è riuscita, e il pannello l'ha già detto.
        End Try

    End Sub

    ''' <summary>
    ''' La candidatura che si sta guardando; <c>Nothing</c> quando qui c'è il 📄 CV base,
    ''' che di candidature non ne ha nessuna. Serve a chi porta il flusso avanti (P7).
    ''' </summary>
    Public ReadOnly Property Candidatura As Opportunita
        Get
            Return _candidatura
        End Get
    End Property

    ''' <summary>Chi dei due si sta guardando decide dove vanno i file (cap. 05.6).</summary>
    Private Function ScriviAsync(formati As FormatiDocumento) As Task(Of IReadOnlyList(Of String))

        If _cvBase IsNot Nothing Then
            Return _documenti.ScriviCvBaseAsync(_cvBase, Nothing, formati, _linguaCvBase)
        End If

        ' Senza cartella non c'è un «dove»: l'opportunità arriva qui già salvata da P4, ma
        ' se così non fosse la si salva adesso invece di rifiutare l'esportazione — e una
        ' cartella nata qui deve comparire nella Home come tutte le altre.
        If String.IsNullOrEmpty(_candidatura.Cartella) Then
            _contesto.Opportunita.Salva(_candidatura)
            AnnotaNelRegistro()
        End If

        Return _documenti.ScriviCandidaturaAsync(
            _candidatura, formati, DocumentiDaTendina(cmbEsporta.SelectedIndex))

    End Function

    Private Function CEQualcosaDaEsportare() As Boolean
        Return _cvBase IsNot Nothing OrElse (_candidatura IsNot Nothing AndAlso _candidatura.Cv IsNot Nothing)
    End Function

    ' ==================================================================
    ' Riscrivere a mano
    ' ==================================================================

    ''' <summary>
    ''' Apre la modifica a mano dei testi e, se qualcosa è stato riscritto, lo mette a
    ''' video e per iscritto (T9d, cap. 08.4).
    ''' </summary>
    ''' <remarks>
    ''' È il metodo che preme il bottone, e le sue due metà stanno fuori di qui — quali
    ''' documenti si aprono e cosa si fa di quel che torna — perché una finestra modale il
    ''' banco non la sa chiudere: è lo stesso taglio di
    ''' <see cref="PannelloDialogo.TrasformaInAppuntiAsync"/>.
    ''' </remarks>
    Public Async Function ModificaITestiAsync() As Task

        If AiAlLavoro OrElse Not CEQualcosaDaRiscrivere() Then Return

        Dim fuori As List(Of String) = Nothing
        Dim fatte As List(Of RiscritturaFatta) =
            FinestraModificaTesti.Chiedi(FindForm(), DocumentiDaRiscrivere(), fuori)

        ' Due lavori diversi tornano dalla stessa finestra, e l'uno senza l'altro è
        ' possibile: si può uscire avendo solo tolto una voce, senza aver riscritto niente.
        Dim cambiate As Boolean = SegnaLeVociLasciateFuori(fuori)

        If fatte.Count = 0 AndAlso Not cambiate Then Return

        Await ConfermaLeRiscrittureAsync(fatte, cambiate).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' Mette a video e per iscritto i testi appena riscritti a mano.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Si salva subito</b>, come per la lingua (v. <see cref="RicordaLaLingua"/>):
    ''' fra la riscrittura e la prossima azione l'utente può chiudere l'applicazione, e un
    ''' lavoro perso in silenzio è peggio di un lavoro non offerto.</para>
    ''' <para><b>Gli export non hanno bisogno di sapere niente</b>: leggono lo stesso
    ''' documento che la finestra ha appena riscritto (cap. 05.6), e così l'email di P7 e i
    ''' tool del server (cap. 09.3). Il testo entra da un posto solo, e da lì lo trovano
    ''' tutti.</para>
    ''' <para><b>E da R7 si annota anche chi l'ha scritto</b>: quali campi, in quale
    ''' documento e quando. È quell'annotazione — non un booleano di sessione — a far
    ''' sopravvivere l'avviso di «Rigenera» al primo cambio di pannello, e a dire alla
    ''' lettera che il CV da cui discende è cambiato sotto di lei.</para>
    ''' </remarks>
    ''' <summary>
    ''' Mette per iscritto le voci che l'utente ha lasciato fuori dal documento (R6).
    ''' </summary>
    ''' <remarks>
    ''' La finestra dice qual è il taglio <b>di adesso</b>, non cosa è cambiato: si
    ''' confronta con quello di prima, perché la data di questo elenco è ciò che dice alla
    ''' lettera che il CV è cambiato sotto di lei, e una data che si muove a ogni apertura
    ''' della finestra accenderebbe la spia anche quando nessuno ha toccato niente.
    ''' </remarks>
    ''' <returns>Vero se il taglio è cambiato davvero.</returns>
    Public Function SegnaLeVociLasciateFuori(fuori As IList(Of String)) As Boolean

        If fuori Is Nothing Then Return False

        Dim dove As VociTolte = VociLasciateFuori()
        Dim prima As New HashSet(Of String)(dove.Impronte, StringComparer.Ordinal)
        Dim adesso As New HashSet(Of String)(fuori, StringComparer.Ordinal)

        If prima.SetEquals(adesso) Then Return False

        Dim quando As Date = Date.Now

        For Each impronta As String In prima.Except(adesso).ToList()
            dove.Rimetti(impronta, quando)
        Next

        For Each impronta As String In adesso.Except(prima).ToList()
            dove.Togli(impronta, quando)
        Next

        Return True

    End Function

    Public Async Function ConfermaLeRiscrittureAsync(fatte As IList(Of RiscritturaFatta),
                                                     Optional vociCambiate As Boolean = False) As Task

        Dim quante As Integer = If(fatte Is Nothing, 0, fatte.Count)

        If quante = 0 AndAlso Not vociCambiate Then Return

        If quante > 0 Then Annota(fatte)

        Mostra()
        RaccontaLoStato(ArchiviaLeRiscritture(quante), StileApp.TestoSecondario)

        ' Toccato il CV, la lettera che ne discende è rimasta indietro: si riallinea da
        ' sé, subito e una volta sola (R7). Non è un automatismo di comodo — è il rimedio
        ' al silenzio che il collaudo dal vivo ha trovato: una lettera che continuava a
        ' raccontare la storia vecchia senza che niente lo dicesse.
        '
        ' Senza AI non parte, e non lo dice: chi ha appena salvato un testo suo merita la
        ' conferma del salvataggio, non un errore rosso al posto suo per un lavoro che non
        ' aveva chiesto. Il disallineamento resta detto dalla spia, che è lì col suo
        ' comando e non ha bisogno di nessuno.
        ' Vale per le due strade: toccare un testo del CV e togliergli una voce lo
        ' cambiano allo stesso modo agli occhi della lettera (R6).
        If _pipeline IsNot Nothing AndAlso
           (vociCambiate OrElse fatte.Any(Function(f) f.Ruolo = RuoloDocumento.Cv)) Then
            Await RiallineaLaLetteraAsync().ConfigureAwait(True)
        End If

    End Function

    ''' <summary>
    ''' Riscrive la ✉️ lettera sul 🎯 CV com'è adesso (R7).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Chiede prima solo dove c'è qualcosa da perdere</b>: se anche la lettera
    ''' era stata riscritta a mano, rifarla cancella quel lavoro, e allora si domanda —
    ''' altrimenti si fa e basta, perché è quel che l'utente vuole in ogni caso e una
    ''' domanda in più su un testo che nessuno ha toccato è solo un ostacolo.</para>
    ''' <para><b>Se l'AI non ce la fa non si insiste</b>: lo si dice, e la spia resta
    ''' accesa col suo comando. Un riallineo che fallisce in silenzio riporterebbe
    ''' esattamente al difetto da cui questa funzione nasce.</para>
    ''' </remarks>
    Public Async Function RiallineaLaLetteraAsync() As Task

        If AiAlLavoro OrElse _sulCvBase OrElse _candidatura Is Nothing Then Return
        If _candidatura.Cv Is Nothing OrElse _candidatura.Lettera Is Nothing Then Return

        If _pipeline Is Nothing Then
            RaccontaUnAvviso(MotivoSenzaAi())
            Return
        End If

        Dim sparito As String = MotivoProfiloSparito()
        If sparito IsNot Nothing Then
            RaccontaUnAvviso(sparito)
            Return
        End If

        If _candidatura.RiscrittureDellaLettera.CEQualcosa AndAlso
           MessageBox.Show(
               "Il CV è cambiato: riscrivo la lettera perché racconti la stessa storia?" & vbLf &
               "Anche la lettera l'avevi riscritta a mano, e quel testo verrebbe sostituito." & vbLf &
               "Se dici di no, resta com'è e te lo ricordo io con «Rigenera la lettera».",
               NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
               MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then

            Mostra()
            RaccontaLoStato("La lettera resta com'era: quando vuoi, «Rigenera la lettera» la riallinea.",
                            StileApp.TestoSecondario)
            Return

        End If

        Dim profilo As Profilo = LeggiIlProfilo()
        If profilo Is Nothing Then Return

        Using filo As New CancellationTokenSource()

            _annulla = filo
            LavoroInCorso(True, btnRigeneraLettera)

            Try
                Dim avanzamento As New Progress(Of AvanzamentoPipeline)(
                    Sub(passo) RaccontaLoStato($"{passo.Cosa}… ({passo.Passo} di {passo.Totale})",
                                               StileApp.TestoSecondario))

                Await _pipeline.RigeneraLetteraAsync(_candidatura, profilo, avanzamento, filo.Token).
                    ConfigureAwait(True)

                Mostra()
                RaccontaLoStato(ArchiviaIlRiallineo(), StileApp.TestoSecondario)

            Catch ex As OperationCanceledException
                Mostra()
                RaccontaLoStato("Non ho riscritto la lettera: è rimasta com'era.", StileApp.TestoSecondario)

            Catch ex As ErroreAi
                Mostra()
                RaccontaUnErrore(ex.Message & vbLf &
                                 "La lettera è rimasta com'era: riprova con «Rigenera la lettera».")

            Finally
                _annulla = Nothing
                LavoroInCorso(False)
            End Try

        End Using

    End Function

    ''' <summary>Scrive la candidatura con la lettera appena riallineata, e lo racconta.</summary>
    Private Function ArchiviaIlRiallineo() As String

        Try
            _contesto?.Opportunita.Salva(_candidatura)
            Return "Ho riscritto la ✉️ lettera sul CV come l'hai lasciato tu." & vbLf &
                   "Rileggila: è lei che parla per prima a chi legge."

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Return $"Ho riscritto la ✉️ lettera, ma non sono riuscita a salvarla ({ex.Message})."
        End Try

    End Function

    ''' <summary>
    ''' Segna nei documenti quali campi l'utente ha riscritto a mano, e quando (R7).
    ''' </summary>
    ''' <remarks>
    ''' L'istante è <b>uno solo</b> per tutta la sessione di riscrittura, anche se i campi
    ''' sono sei: è il momento in cui l'utente ha premuto «Salva», e sminuzzarlo in sei
    ''' istanti diversi non direbbe niente di più a nessuno.
    ''' </remarks>
    Private Sub Annota(fatte As IEnumerable(Of RiscritturaFatta))

        Dim adesso As Date = Date.Now

        If _sulCvBase Then

            For Each fatta As RiscritturaFatta In fatte
                _riscrittureDelCvBase.Annota(fatta.Id, adesso)
            Next

            Return

        End If

        If _candidatura Is Nothing Then Return

        For Each ruolo As RuoloDocumento In {RuoloDocumento.Cv, RuoloDocumento.Lettera}

            _candidatura.SegnaRiscritture(
                ruolo,
                fatte.Where(Function(f) f.Ruolo = ruolo).Select(Function(f) f.Id),
                adesso)

        Next

    End Sub

    ''' <summary>
    ''' I documenti in mostra aperti alla riscrittura, ciascuno col «prima» della sua
    ''' rifinitura.
    ''' </summary>
    ''' <remarks>
    ''' Sono due per una candidatura — il 🎯 CV mirato e la ✉️ lettera — e uno solo per il
    ''' 📄 CV base, che una lettera non ce l'ha. Un documento non ancora scritto non
    ''' compare: non c'è niente da riscrivere finché non esiste.
    ''' </remarks>
    Public Function DocumentiDaRiscrivere() As List(Of DocumentoDaRiscrivere)

        Dim aperti As New List(Of DocumentoDaRiscrivere)

        If _sulCvBase Then

            If _cvBase IsNot Nothing Then
                aperti.Add(New DocumentoDaRiscrivere With {
                    .Documento = _cvBase, .Ruolo = RuoloDocumento.Cv,
                    .Tolte = _vociTolteDalCvBase, .Riscritte = _riscrittureDelCvBase})
            End If

            Return aperti

        End If

        If _candidatura Is Nothing Then Return aperti

        If _candidatura.Cv IsNot Nothing Then
            aperti.Add(New DocumentoDaRiscrivere With {
                .Documento = _candidatura.Cv, .Ruolo = RuoloDocumento.Cv,
                .Tolte = _candidatura.VociTolteDalCv,
                .Riscritte = _candidatura.RiscrittureDelCv})
        End If

        If _candidatura.Lettera IsNot Nothing Then
            aperti.Add(New DocumentoDaRiscrivere With {
                .Documento = _candidatura.Lettera, .Ruolo = RuoloDocumento.Lettera,
                .Riscritte = _candidatura.RiscrittureDellaLettera})
        End If

        Return aperti

    End Function

    ''' <summary>
    ''' Se c'è qualcosa da riscrivere: un documento in mostra che abbia dentro della prosa.
    ''' </summary>
    ''' <remarks>
    ''' Non basta che un documento ci sia: un CV senza sommario e senza descrizioni — tutto
    ''' fatti, che di qui non si toccano — aprirebbe una finestra vuota. Chi risponde è
    ''' <see cref="Rifinitura"/>, che è l'unico posto che sa quali campi sono prosa.
    ''' </remarks>
    Private Function CEQualcosaDaRiscrivere() As Boolean

        ' Da R6 (2026-08-24) non basta più chiedere della prosa: un CV tutto fatti — che
        ' prima apriva una finestra vuota, e per questo il bottone restava spento — ha
        ' comunque delle voci che si possono lasciare fuori da questo documento.
        Return DocumentiDaRiscrivere().Any(
            Function(aperto) Rifinitura.CampiDiProsa(aperto.Documento).Count > 0 OrElse
                             VociDelCv.Elenca(aperto.Documento).Count > 0)

    End Function

    ''' <summary>Scrive il documento riscritto dov'è di casa, e racconta com'è andata.</summary>
    Private Function ArchiviaLeRiscritture(riscritti As Integer) As String

        Dim quanti As String = If(riscritti = 1, "il testo che hai riscritto",
                                  $"i {riscritti} testi che hai riscritto")

        If _contesto Is Nothing Then Return $"Ho messo {quanti} nel documento."

        Try
            ' Il CV base torna nel suo file con la storia che aveva: la modifica a mano non
            ' lo fa rinascere da un altro profilo né in un altro giorno (cap. 08.4).
            If _sulCvBase Then
                _contesto.Archivio.SalvaCvBase(_cvBase, _versioneDelCvBase,
                                               _linguaCvBase, _generatoIlCvBase,
                                               _riscrittureDelCvBase)
            Else
                _contesto.Opportunita.Salva(_candidatura)
            End If

            Return $"Ho salvato {quanti}: è quello che esce adesso in DOCX, in PDF e nell'email." & vbLf &
                   "Vale per questo documento: se un fatto è sbagliato, correggilo nel profilo — " &
                   "è di lì che viene, e alla prossima rigenerazione tornerebbe com'era."

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Return $"Ho messo {quanti} nel documento, ma non sono riuscita a salvarlo ({ex.Message})."
        End Try

    End Function

    ' ==================================================================
    ' Quel che si vede
    ' ==================================================================

    ''' <summary>
    ''' Mette a video le tre colonne. La colonna dell'annuncio, per il 📄 CV base, resta
    ''' senza annuncio — e lo dice, invece di sembrare un riquadro che non ha caricato.
    ''' </summary>
    Private Sub Mostra()

        ' Il bottone del ritorno dice dove riporta, e le due strade per arrivare qui sono
        ' due: un'etichetta sbagliata manderebbe l'utente in un pannello dove non è mai
        ' stato.
        btnTornaIndietro.Text = If(_candidatura Is Nothing, "◀ Torna al profilo", "◀ Torna all'opportunità")

        ' Il nome della colonna si decide dalla strada percorsa, non dal fatto che il
        ' documento sia già arrivato: nell'attesa non c'è ancora niente da mostrare, ma si
        ' sa benissimo quale dei due si sta aspettando.
        If _sulCvBase Then
            lblCv.Text = "📄 CV base"
            AllestisciLaTendina(_linguaCvBase)
            txtAnnuncio.Text = "Il 📄 CV base non nasce da un annuncio: è il ritratto del tuo profilo."
            txtCv.Text = Anteprima(_cvBase,
                                   Function(d) Impaginazione.PaginaCv(d, _linguaCvBase,
                                                                      _vociTolteDalCvBase),
                                   "Il CV non è ancora stato scritto.")
            txtLettera.Text = "La lettera si scrive su un annuncio: qui non ce n'è uno."
            AggiornaComandi()
            Return
        End If

        lblCv.Text = "🎯 CV mirato"

        If _candidatura Is Nothing Then
            txtAnnuncio.Clear()
            txtCv.Clear()
            txtLettera.Clear()
            AggiornaComandi()
            Return
        End If

        Dim lingua As String = LinguaDocumenti.PerDocumenti(_candidatura.Lingua)

        AllestisciLaTendina(lingua)

        txtAnnuncio.Text = VistaAnnuncio.Riassunto(_candidatura.Annuncio)

        ' L'anteprima si impagina nella lingua della candidatura: è la stessa pagina di
        ' blocchi che finirà nel DOCX e nel PDF (cap. 05.3), e vederla qui con le etichette
        ' sbagliate vorrebbe dire scoprire l'errore solo aprendo il file.
        txtCv.Text = Anteprima(_candidatura.Cv,
                               Function(d) Impaginazione.PaginaCv(d, lingua,
                                                                  _candidatura.VociTolteDalCv),
                               "Il CV non è ancora stato scritto.")
        txtLettera.Text = Anteprima(_candidatura.Lettera,
                                    Function(d) Impaginazione.PaginaLettera(d, lingua),
                                    "La lettera non è ancora stata scritta.")

        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Mette la tendina sulla lingua della candidatura senza che questo conti come una
    ''' scelta dell'utente.
    ''' </summary>
    Private Sub AllestisciLaTendina(lingua As String)

        _tendinaInAllestimento = True

        Try
            cmbLingua.SelectedIndex = If(lingua = LinguaDocumenti.Inglese, 1, 0)
        Finally
            _tendinaInAllestimento = False
        End Try

    End Sub

    ''' <summary>La lingua che la tendina sta mostrando.</summary>
    Private Function LinguaDallaTendina() As String

        Return If(cmbLingua.SelectedIndex = 1, LinguaDocumenti.Inglese, LinguaDocumenti.Italiano)

    End Function

    ''' <summary>
    ''' Quali documenti la tendina «Esporta:» sta chiedendo (R8, 2026-08-23). È
    ''' <c>Shared</c> e prende l'indice invece di leggere il controllo, così il banco può
    ''' provarla senza una finestra: la traduzione fra una posizione in un elenco e una
    ''' scelta è il punto in cui si sbaglia, non il resto.
    ''' </summary>
    Public Shared Function DocumentiDaTendina(indice As Integer) As DocumentiDaScrivere

        Select Case indice
            Case 1 : Return DocumentiDaScrivere.Cv
            Case 2 : Return DocumentiDaScrivere.Lettera
            Case Else : Return DocumentiDaScrivere.Entrambi
        End Select

    End Function

    ''' <summary>Un documento come si legge, o il motivo per cui non c'è ancora.</summary>
    ''' <remarks>
    ''' Fino a T9d qui si attaccava in coda il prima/dopo della rifinitura, quando la
    ''' casella di P6 era spuntata. Quel confronto adesso non c'è più da nessuna parte:
    ''' è stato tolto del tutto poche ore dopo averlo finito (2026-08-22), misurando che
    ''' su una candidatura vera cambiava cinque parole e nessun fatto — un confronto che
    ''' non si riesce a distinguere non è una garanzia che si usa, è un comando in più da
    ''' capire. Delle tre cose promesse ne restano due, accettare e riscrivere a mano, e
    ''' la seconda vive nella finestra «Modifica i testi» (cap. 08.4).
    ''' </remarks>
    Private Function Anteprima(documento As JsonNode,
                               impagina As Func(Of JsonNode, PaginaDocumento),
                               seManca As String) As String

        If documento Is Nothing Then Return seManca

        Return ScrittoreTesto.Componi(impagina(documento))

    End Function

    ''' <summary>
    ''' Il profilo su disco, o <c>Nothing</c> col motivo già raccontato: è la stessa
    ''' lettura di P4, e la stessa promessa — un profilo illeggibile non si sovrascrive
    ''' mai (cap. 11.1).
    ''' </summary>
    Private Function LeggiIlProfilo() As Profilo

        If _contesto Is Nothing OrElse Not _contesto.Archivio.Esiste Then
            RaccontaLoStato("Prima serve il tuo profilo: costruiscilo nella scheda «Profilo».",
                            StileApp.TestoSecondario)
            Return Nothing
        End If

        Try
            Return _contesto.Archivio.Carica()

        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                   OrElse TypeOf ex Is UnauthorizedAccessException
            RaccontaUnErrore($"Il profilo c'è ma non si lascia leggere: {ex.Message}")
            Return Nothing
        End Try

    End Function

    ' ==================================================================
    ' I bottoni
    ' ==================================================================

    Private Sub btnTornaIndietro_Click(sender As Object, e As EventArgs) Handles btnTornaIndietro.Click
        RaiseEvent TornaIndietro(Me, EventArgs.Empty)
    End Sub

    Private Async Sub btnRigenera_Click(sender As Object, e As EventArgs) Handles btnRigenera.Click

        ' Ad attesa in corso lo stesso bottone serve ad annullarla, come «Analizza» in P4
        ' e l'import in P2. Sta prima della conferma: qui non si sta buttando via niente,
        ' si sta rinunciando a scrivere.
        If AiAlLavoro Then
            AnnullaIlLavoro()
            Return
        End If

        ' Rigenerare butta via testi che l'utente potrebbe aver già letto e approvato, e
        ' costa un'altra attesa: si chiede prima (cap. 03.3, livello 4).
        If MessageBox.Show(
            "Vuoi che li riscriva da capo?" & vbLf &
            "I testi che vedi adesso vengono sostituiti." & AncheQuelliRiscrittiAMano(),
            NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return

        Await RigeneraAsync()

    End Sub

    ''' <summary>Riscrive da capo quello che si sta guardando.</summary>
    Public Async Function RigeneraAsync() As Task

        If AiAlLavoro Then Return

        ' Rigenerare il CV base vuol dire riscriverlo: si va dritti alla generazione,
        ' senza ripescare da disco quello che si sta buttando.
        If _sulCvBase Then
            Await GeneraIlCvBaseAsync().ConfigureAwait(True)
            Return
        End If

        If _candidatura Is Nothing Then Return

        ' Prima di buttare via quel che c'è: se non si può riscrivere, i documenti di
        ' allora sono tutto quel che resta di quella candidatura.
        Dim sparito As String = MotivoProfiloSparito()
        If sparito IsNot Nothing Then
            RaccontaUnAvviso(sparito)
            Return
        End If

        ' I documenti di prima si buttano adesso: se la generazione fallisce a metà, il
        ' pannello non deve mostrare un CV vecchio accanto a una lettera nuova.
        _candidatura.Cv = Nothing
        _candidatura.Lettera = Nothing
        _candidatura.RiscrittureDelCv.Dimentica()
        _candidatura.RiscrittureDellaLettera.Dimentica()
        _candidatura.LetteraGenerata = Nothing
        Mostra()

        Await GeneraLaCandidaturaAsync().ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' La lingua della candidatura è cambiata: i documenti si riscrivono (cap. 10.1).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché rigenera invece di prendere solo nota.</b> Se la scelta restasse
    ''' un'impostazione muta, il pannello si troverebbe con una candidatura «in inglese» e
    ''' due documenti scritti in italiano: l'anteprima li impagina nella lingua della
    ''' candidatura, e ne uscirebbe un CV col testo italiano sotto «Work experience». Legare
    ''' la tendina alla rigenerazione è ciò che impedisce ai due di divergere, e costa una
    ''' domanda invece di un campo nuovo in <c>stato.json</c> che dica in che lingua sono i
    ''' documenti che ci sono.</para>
    ''' <para>Se documenti non ce ne sono ancora, non c'è niente da riscrivere e niente da
    ''' chiedere: si prende nota e si va avanti.</para>
    ''' </remarks>
    Private Async Sub cmbLingua_SelectedIndexChanged(sender As Object, e As EventArgs) _
                                                     Handles cmbLingua.SelectedIndexChanged

        If _tendinaInAllestimento Then Return

        ' Il 📄 CV base ha la stessa regola, con un padrone diverso: la sua lingua non sta
        ' in un'opportunità — che non ha — ma nel suo cv_base.json (T7d, cap. 10.3).
        If _sulCvBase Then
            Await CambiaLinguaDelCvBaseAsync(LinguaDallaTendina()).ConfigureAwait(True)
            Return
        End If

        If _candidatura Is Nothing Then Return

        Dim scelta As String = LinguaDallaTendina()
        Dim comEra As String = LinguaDocumenti.PerDocumenti(_candidatura.Lingua)
        If scelta = comEra Then Return

        Dim nome As String = LinguaDocumenti.Nome(scelta).ToLowerInvariant()

        If _candidatura.Cv Is Nothing AndAlso _candidatura.Lettera Is Nothing Then
            _candidatura.Lingua = scelta
            RicordaLaLingua()
            RaccontaLoStato($"Quando li scriverò, saranno in {nome}.", StileApp.TestoSecondario)
            Return
        End If

        ' Qui la guardia sta prima della domanda: chiedere «li riscrivo in inglese?» per poi
        ' dire che non si possono riscrivere sarebbe una domanda a vuoto. La tendina torna
        ' dov'era, come quando è l'utente a dire di no.
        Dim sparito As String = MotivoProfiloSparito()
        If sparito IsNot Nothing Then
            AllestisciLaTendina(comEra)
            RaccontaUnAvviso(sparito)
            Return
        End If

        If MessageBox.Show(
            $"Vuoi che riscriva CV e lettera in {nome}?" & vbLf &
            "I testi che vedi adesso vengono sostituiti." & AncheQuelliRiscrittiAMano(),
            NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then

            ' Rifiutando, la tendina torna dov'era: lasciarla sulla lingua nuova
            ' racconterebbe una candidatura che non esiste.
            AllestisciLaTendina(comEra)
            Return

        End If

        _candidatura.Lingua = scelta
        RicordaLaLingua()

        Await RigeneraAsync().ConfigureAwait(True)

    End Sub

    ''' <summary>
    ''' La lingua del 📄 CV base è cambiata: lo si riscrive (T7d, cap. 10.3).
    ''' </summary>
    ''' <remarks>
    ''' Non c'è niente da mettere per iscritto prima della generazione, ed è la differenza
    ''' con la candidatura: là la lingua è un dato dell'opportunità, che vive su disco e
    ''' compare nel registro anche prima che i documenti esistano; qui vive dentro il
    ''' <c>cv_base.json</c>, e senza un CV quel file non c'è. Una scelta presa e non ancora
    ''' usata resta di questa sessione — non c'è nessun posto onesto in cui scriverla.
    ''' </remarks>
    Private Async Function CambiaLinguaDelCvBaseAsync(scelta As String) As Task

        If scelta = _linguaCvBase Then Return

        Dim nome As String = LinguaDocumenti.Nome(scelta).ToLowerInvariant()

        ' Niente da sostituire, niente da chiedere: si prende nota per la prossima volta.
        If _cvBase Is Nothing Then
            _linguaCvBase = scelta
            RaccontaLoStato($"Quando lo scriverò, sarà in {nome}.", StileApp.TestoSecondario)
            Return
        End If

        If MessageBox.Show(
            $"Vuoi che riscriva il 📄 CV base in {nome}?" & vbLf &
            "Il testo che vedi adesso viene sostituito." & AncheQuelliRiscrittiAMano(),
            NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then

            ' Come per la candidatura: rifiutando, la tendina torna dov'era, o
            ' racconterebbe un CV che non esiste.
            AllestisciLaTendina(_linguaCvBase)
            Return

        End If

        ' Il CV di prima si butta adesso, come fa la candidatura: se la generazione va
        ' storta a metà, il pannello non deve restare con un testo italiano impaginato
        ' sotto «Work experience» — e la conferma ha appena detto che viene sostituito.
        _linguaCvBase = scelta
        LasciaIlCvBase()
        Mostra()

        Await GeneraIlCvBaseAsync().ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' Mette per iscritto la lingua scelta, subito.
    ''' </summary>
    ''' <remarks>
    ''' La lingua è uno dei dati che il registro tiene (cap. 07.3), e la lezione di T6 vale
    ''' anche qui: <b>chi cambia un dato dell'indice deve annotarlo</b>, o la Home continua
    ''' a raccontare quello di prima. Si scrive adesso e non alla generazione, perché fra i
    ''' due momenti l'utente può chiudere l'applicazione — e una scelta persa in silenzio è
    ''' peggio di una scelta non offerta.
    ''' </remarks>
    Private Sub RicordaLaLingua()

        If _contesto Is Nothing Then Return

        Try
            _contesto.Opportunita.Salva(_candidatura)
            AnnotaNelRegistro()

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' La scelta vale comunque per questa sessione: dei documenti nella lingua
            ' giusta è quello che conta, e il salvataggio ha già la sua voce quando
            ' fallisce alla generazione.
        End Try

    End Sub

    Private Async Sub btnEsportaDocx_Click(sender As Object, e As EventArgs) Handles btnEsportaDocx.Click
        Await EsportaConSceltaAsync(FormatiDocumento.Docx)
    End Sub

    Private Async Sub btnEsportaPdf_Click(sender As Object, e As EventArgs) Handles btnEsportaPdf.Click
        Await EsportaConSceltaAsync(FormatiDocumento.Pdf)
    End Sub

    ' ==================================================================
    ' La tendina dei documenti (T9d)
    ' ==================================================================

    ''' <summary>
    ''' Una voce della tendina: quale documento apre, e come si chiama a video.
    ''' </summary>
    Private NotInheritable Class VoceDocumento

        ''' <summary>La cartella della candidatura; <c>Nothing</c> per il 📄 CV base.</summary>
        Public ReadOnly Cartella As String

        Public ReadOnly Etichetta As String

        Public Sub New(cartella As String, etichetta As String)
            Me.Cartella = cartella
            Me.Etichetta = etichetta
        End Sub

        Public Overrides Function ToString() As String
            Return Etichetta
        End Function

    End Class

    ''' <summary>
    ''' Riempie la tendina coi documenti che <b>esistono davvero</b> e ci segna quello in
    ''' mostra.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Solo documenti già scritti</b>: il 📄 CV base compare se è salvato su disco,
    ''' una candidatura se ha almeno un documento. Elencare un CV base che non c'è ancora
    ''' vorrebbe dire far partire una generazione — un'attesa e dei token — a chi credeva di
    ''' spostarsi fra due schermate.</para>
    ''' <para><b>Dal più recente</b>: l'archivio li dà dal più vecchio perché il nome della
    ''' cartella comincia con la data, ma chi cerca un documento cerca quasi sempre l'ultimo
    ''' su cui ha lavorato.</para>
    ''' </remarks>
    Private Sub AllestisciLaTendinaDeiDocumenti()

        If _contesto Is Nothing Then Return

        cmbDocumento.Items.Clear()

        If CESuDiscoUnCvBase() Then cmbDocumento.Items.Add(New VoceDocumento(Nothing, "📄 CV base"))

        For Each cartella As String In _contesto.Opportunita.Elenco().Reverse()

            Dim voce As Opportunita = LeggiSenzaLamentarsi(cartella)
            If voce Is Nothing OrElse (voce.Cv Is Nothing AndAlso voce.Lettera Is Nothing) Then Continue For

            Dim azienda As String = If(String.IsNullOrWhiteSpace(voce.Azienda),
                                       "(azienda non dichiarata)", voce.Azienda.Trim())

            cmbDocumento.Items.Add(New VoceDocumento(cartella, $"🎯 {azienda}"))

        Next

        SegnaNellaTendinaQuelloInMostra()

    End Sub

    ''' <summary>Mette la spunta sulla voce del documento che si sta guardando.</summary>
    Private Sub SegnaNellaTendinaQuelloInMostra()

        For Each voce As VoceDocumento In cmbDocumento.Items

            Dim eQuesta As Boolean =
                If(_sulCvBase,
                   voce.Cartella Is Nothing,
                   voce.Cartella IsNot Nothing AndAlso _candidatura IsNot Nothing AndAlso
                   String.Equals(voce.Cartella, _candidatura.Cartella, StringComparison.OrdinalIgnoreCase))

            If eQuesta Then
                cmbDocumento.SelectedItem = voce
                Return
            End If

        Next

        cmbDocumento.SelectedIndex = -1

    End Sub

    ''' <summary>
    ''' Il documento scelto dalla tendina prende il posto di quello in mostra.
    ''' </summary>
    ''' <remarks>
    ''' È <c>SelectionChangeCommitted</c> e non <c>SelectedIndexChanged</c>: scatta solo
    ''' quando a scegliere è stato l'utente, e non quando è il pannello a segnare la voce
    ''' del documento che sta già mostrando — che sarebbe un rientro dentro se stesso.
    ''' </remarks>
    Private Async Sub cmbDocumento_SelectionChangeCommitted(sender As Object, e As EventArgs) _
                                                            Handles cmbDocumento.SelectionChangeCommitted
        Await ApriDallaTendinaAsync(cmbDocumento.SelectedIndex)
    End Sub

    ''' <summary>
    ''' Apre il documento che sta alla riga indicata della tendina. È pubblica perché il
    ''' banco una scelta col mouse non la sa fare, e la scelta è tutto quel che c'è da
    ''' provare qui.
    ''' </summary>
    Public Async Function ApriDallaTendinaAsync(indice As Integer) As Task

        Dim scelta As VoceDocumento = Nothing
        If indice >= 0 AndAlso indice < cmbDocumento.Items.Count Then
            scelta = TryCast(cmbDocumento.Items(indice), VoceDocumento)
        End If

        If scelta Is Nothing OrElse AiAlLavoro Then
            SegnaNellaTendinaQuelloInMostra()
            Return
        End If

        If scelta.Cartella Is Nothing Then
            Await MostraIlCvBaseAsync().ConfigureAwait(True)
            Return
        End If

        Dim candidatura As Opportunita = LeggiSenzaLamentarsi(scelta.Cartella)

        If candidatura Is Nothing Then
            RaccontaUnErrore("Quel documento non si è lasciato leggere: riprova dalla Home.")
            AllestisciLaTendinaDeiDocumenti()
            Return
        End If

        Await MostraLaCandidaturaAsync(candidatura).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' Riallestisce la tendina ogni volta che la si apre: fra un giro e l'altro possono
    ''' essere nati documenti nuovi, e un elenco vecchio è peggio di nessun elenco.
    ''' </summary>
    Private Sub cmbDocumento_DropDown(sender As Object, e As EventArgs) Handles cmbDocumento.DropDown
        AllestisciLaTendinaDeiDocumenti()
    End Sub

    ''' <summary>
    ''' Apre i documenti «da fermo», come chiede la voce ⟨📄 Documenti⟩ della barra: quello
    ''' già in mostra se c'è, altrimenti il 📄 CV base salvato, altrimenti l'ultima
    ''' candidatura che abbia un documento.
    ''' </summary>
    ''' <remarks>
    ''' <b>Non chiama mai l'AI</b>: senza niente da mostrare il pannello resta vuoto con la
    ''' sua spiegazione. Entrare in una schermata è un gesto di navigazione, e una
    ''' navigazione che fa partire una generazione sarebbe una spesa non chiesta.
    ''' </remarks>
    Public Async Function ApriQualcosaAsync() As Task

        If AiAlLavoro Then Return

        If _cvBase IsNot Nothing OrElse _candidatura IsNot Nothing Then
            AllestisciLaTendinaDeiDocumenti()
            Mostra()
            Return
        End If

        If CESuDiscoUnCvBase() Then
            Await MostraIlCvBaseAsync().ConfigureAwait(True)
            Return
        End If

        Dim ultima As Opportunita = UltimaConDocumenti()

        If ultima IsNot Nothing Then
            Await MostraLaCandidaturaAsync(ultima).ConfigureAwait(True)
            Return
        End If

        AllestisciLaTendinaDeiDocumenti()
        Mostra()
        RaccontaLoStato(
            "Non c'è ancora nessun documento: genera il 📄 CV base dalla scheda «Profilo», " &
            "oppure apri una candidatura dalla Home.",
            StileApp.TestoSecondario)

    End Function

    ''' <summary>L'ultima candidatura che abbia almeno un documento scritto.</summary>
    Private Function UltimaConDocumenti() As Opportunita

        If _contesto Is Nothing Then Return Nothing

        For Each cartella As String In _contesto.Opportunita.Elenco().Reverse()
            Dim voce As Opportunita = LeggiSenzaLamentarsi(cartella)
            If voce IsNot Nothing AndAlso (voce.Cv IsNot Nothing OrElse voce.Lettera IsNot Nothing) Then
                Return voce
            End If
        Next

        Return Nothing

    End Function

    ''' <summary>
    ''' Se sul disco c'è un 📄 CV base già scritto. Un file illeggibile qui vale come
    ''' assente: la tendina è navigazione, e chi deve raccontare il guasto è chi apre il
    ''' documento davvero (v. <see cref="RipescaIlCvBase"/>).
    ''' </summary>
    Private Function CESuDiscoUnCvBase() As Boolean

        If _contesto Is Nothing OrElse Not _contesto.Archivio.Esiste Then Return False

        Try
            Return _contesto.Archivio.CaricaCvBase() IsNot Nothing

        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                   OrElse TypeOf ex Is UnauthorizedAccessException
            Return False
        End Try

    End Function

    ''' <summary>Una candidatura letta da disco, o <c>Nothing</c> se non si lascia leggere.</summary>
    Private Function LeggiSenzaLamentarsi(cartella As String) As Opportunita

        Try
            Return _contesto.Opportunita.Carica(cartella)

        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                   OrElse TypeOf ex Is UnauthorizedAccessException
            Return Nothing
        End Try

    End Function

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
            _comandi.ASinistra(btnTornaIndietro, btnRigenera, btnRigeneraLettera, btnModificaTesti)
            _comandi.ADestra(btnPreparaEmail, btnEsportaPdf, btnEsportaDocx)
        End If

        _comandi.Disponi(Math.Max(AltezzaMinimaAzioni, _ingombroLogo.Height))

    End Sub

    Private Async Sub btnModificaTesti_Click(sender As Object, e As EventArgs) Handles btnModificaTesti.Click
        Await ModificaITestiAsync()
    End Sub

    Private Async Sub btnRigeneraLettera_Click(sender As Object, e As EventArgs) Handles btnRigeneraLettera.Click

        ' Come «Rigenera» qui accanto: mentre riscrive, questo bottone è l'annulla.
        If AiAlLavoro Then
            AnnullaIlLavoro()
            Return
        End If

        Await RiallineaLaLetteraAsync()

    End Sub

    Private Sub btnPreparaEmail_Click(sender As Object, e As EventArgs) Handles btnPreparaEmail.Click
        RaiseEvent EmailRichiesta(Me, EventArgs.Empty)
    End Sub

    Private Sub PannelloDocumenti_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        DisponiLeAzioni()
    End Sub

    Private Sub VestiIBottoni()

        StileApp.VestiBottone(btnTornaIndietro, LivelloBottone.Neutro)

        ' Rigenerare sostituisce testi già scritti: modifica dati esistenti.
        StileApp.VestiBottone(btnRigenera, LivelloBottone.Attenzione)

        ' E riscrivere a mano pure, con la differenza che a scrivere è l'utente.
        StileApp.VestiBottone(btnModificaTesti, LivelloBottone.Attenzione)

        ' Riallineare la lettera è una rigenerazione, e ne ha il livello: sostituisce un
        ' testo che c'è già (R7).
        StileApp.VestiBottone(btnRigeneraLettera, LivelloBottone.Attenzione)

        StileApp.VestiBottone(btnEsportaDocx, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnEsportaPdf, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnPreparaEmail, LivelloBottone.AzionePrincipale)

    End Sub

    ''' <summary>
    ''' Ciò che arriverà, visibile e spento, col suo suggerimento (cap. 03.8). Questo è il
    ''' pannello che più di ogni altro mostra dove il progetto sta andando.
    ''' </summary>
    Private Sub DichiaraLeTappeCheMancano()

        ' La lingua non sta più qui: T7 è arrivata, e la tendina la accende
        ' AggiornaComandi quando c'è un documento a cui appartenga — una candidatura, o
        ' il 📄 CV base da T7d.
        cmbLingua.SelectedIndex = 0

        ' «CV e lettera»: chi non tocca la tendina ottiene quel che otteneva prima di R8.
        cmbEsporta.SelectedIndex = 0

        ' Il prima/dopo della rifinitura non sta più qui, e non sta più da nessuna parte: la
        ' casella che lo accendeva è stata tolta a T9d, insieme al confronto che mostrava.

        ' «Prepara email» non sta più qui: T6 è arrivata, e il bottone lo accende
        ' AggiornaComandi quando c'è una candidatura con la sua lettera.

    End Sub

    Private Sub RaccontaLoStato(testo As String, colore As Color)

        lblStatoDocumenti.Text = testo
        lblStatoDocumenti.ForeColor = colore

    End Sub

    ''' <summary>
    ''' Una riga che dice che qualcosa non è riuscito: la parola e il colore insieme
    ''' (v. <see cref="Segnalazioni"/>).
    ''' </summary>
    Private Sub RaccontaUnErrore(testo As String)

        RaccontaLoStato(Segnalazioni.PrefissoErrore & testo, StileApp.Pericolo)

    End Sub

    ''' <summary>
    ''' Una riga che dice che qualcosa manca o non torna — la chiave, il profilo di allora,
    ''' una lettera rimasta indietro: stesso colore dell'errore, parola diversa. Qui non è
    ''' caduto niente, e chiamarlo errore manderebbe a cercare un guasto che non c'è.
    ''' </summary>
    Private Sub RaccontaUnAvviso(testo As String)

        RaccontaLoStato(Segnalazioni.PrefissoAvviso & testo, StileApp.Pericolo)

    End Sub

    ''' <summary>Perché non si può generare: la chiave, o i prompt.</summary>
    Private Function MotivoSenzaAi() As String

        If _contesto IsNot Nothing AndAlso _contesto.Libreria Is Nothing Then
            Return "I prompt non si sono caricati: senza di loro non posso scrivere né il CV né la lettera."
        End If

        Return $"Per scrivere i documenti serve la chiave API ({ClientClaude.NomeVariabileChiave})."

    End Function

    ''' <param name="comando">
    ''' Il comando che ha chiesto questa attesa: durante il lavoro diventa il suo
    ''' «Annulla». Si passa solo cominciando; finendo, non c'è più nessuna attesa da
    ''' rappresentare.
    ''' </param>
    Private Sub LavoroInCorso(inCorso As Boolean, Optional comando As Button = Nothing)

        _comandoDellAttesa = If(inCorso, comando, Nothing)

        Cursor = If(inCorso, Cursors.AppStarting, Cursors.Default)

        AggiornaComandi()
        RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)

    End Sub

    ''' <summary>
    ''' Decide, in un posto solo, che cosa si può fare adesso: si esporta quando c'è un
    ''' documento, si rigenera quando c'è qualcosa da rifare, e mentre l'AI scrive non si
    ''' fa nessuna delle due.
    ''' </summary>
    Private Sub AggiornaComandi()

        Dim occupato As Boolean = AiAlLavoro
        Dim conDocumento As Boolean = CEQualcosaDaEsportare()

        btnEsportaDocx.Enabled = conDocumento AndAlso Not occupato
        btnEsportaPdf.Enabled = conDocumento AndAlso Not occupato

        ' A lavoro in corso il comando che l'ha chiesto cambia mestiere: è l'annulla
        ' dell'attesa, come «Analizza» in P4 e l'import in P2 (cap. 12.7 — le operazioni
        ' lunghe sono annullabili). Fino al 2026-09-01 qui non c'era nessuna via per
        ' fermare una generazione: `AnnullaIlLavoro` esisteva e la chiamava solo la
        ' chiusura della finestra, cioè si annullava soltanto chiudendo il programma.
        Dim annullaLaGenerazione As Boolean = occupato AndAlso _comandoDellAttesa Is btnRigenera

        btnRigenera.Text = If(annullaLaGenerazione, "Annulla", "Rigenera")

        ' Anche dopo una generazione andata storta: è da qui che si riprova, e un
        ' «Rigenera» spento lascerebbe come unica via il ritorno al profilo.
        btnRigenera.Enabled = annullaLaGenerazione OrElse
                              (Not occupato AndAlso
                               (_sulCvBase OrElse _candidatura IsNot Nothing) AndAlso
                               (_pipeline IsNot Nothing OrElse _generatore IsNot Nothing))

        btnTornaIndietro.Enabled = Not occupato

        ' La spia di R7: c'è solo quando la ✉️ lettera è rimasta indietro rispetto al 🎯 CV
        ' riscritto a mano. Non è un comando spento in attesa — è un comando che, quando
        ' non serve, non deve esserci affatto (cap. 03.3), e la fascia sa già non tenergli
        ' il posto. Chi lo vede sa due cose in una: che c'è un disallineamento, e come si
        ' chiude.
        Dim daRiallineare As Boolean = Not _sulCvBase AndAlso
                                       _candidatura IsNot Nothing AndAlso
                                       _candidatura.LetteraDaRiallineare

        ' Anche questa attesa si annulla, e dal comando che l'ha chiesta: sparire mentre
        ' il lavoro che si è appena chiesto è in volo lascerebbe l'attesa senza uscita.
        Dim annullaIlRiallineo As Boolean = occupato AndAlso _comandoDellAttesa Is btnRigeneraLettera
        Dim inFascia As Boolean = daRiallineare OrElse annullaIlRiallineo

        If btnRigeneraLettera.Visible <> inFascia Then
            btnRigeneraLettera.Visible = inFascia
            DisponiLeAzioni()
        End If

        btnRigeneraLettera.Text = If(annullaIlRiallineo, "Annulla", "⚠ Rigenera la lettera")
        btnRigeneraLettera.Enabled = annullaIlRiallineo OrElse
                                     (daRiallineare AndAlso Not occupato AndAlso _pipeline IsNot Nothing)

        If daRiallineare Then
            _suggerimenti.SetToolTip(btnRigeneraLettera,
                "Hai riscritto il CV a mano: la lettera racconta ancora la storia di prima." & vbLf &
                "Premi qui e la riscrivo su quello che hai lasciato tu. Il CV non lo tocco.")
        End If

        ' Si riscrive quel che c'è già, e solo la sua prosa (cap. 08.4): su un pannello
        ' vuoto non c'è niente da aprire, e mentre l'AI scrive nemmeno — riscriverebbero
        ' in due lo stesso documento.
        btnModificaTesti.Enabled = Not occupato AndAlso CEQualcosaDaRiscrivere()

        If btnModificaTesti.Enabled Then
            _suggerimenti.SetToolTip(btnModificaTesti,
                "Riscrivi tu il sommario, le descrizioni delle esperienze e il corpo della lettera." & vbLf &
                "I fatti restano quelli del profilo.")
        ElseIf conDocumento Then
            _suggerimenti.SetToolTip(btnModificaTesti,
                "Qui non c'è prosa da riscrivere: in questo documento ci sono solo fatti.")
        Else
            _suggerimenti.SetToolTip(btnModificaTesti,
                "Prima genera i documenti: si riscrive un testo che c'è.")
        End If

        ' La lingua si sceglie su un documento, e i documenti sono due (T7d, cap. 10.3):
        ' quelli della candidatura, dove la lingua è dell'opportunità, e il 📄 CV base,
        ' dove è sua e sta nel cv_base.json. Su un pannello vuoto non c'è né l'una né
        ' l'altro, e la tendina resta spenta.
        cmbLingua.Enabled = Not occupato AndAlso (_sulCvBase OrElse _candidatura IsNot Nothing)
        lblLingua.Enabled = cmbLingua.Enabled

        ' «Esporta:» ha senso solo dove i documenti sono due. Sul 📄 CV-1 base la lettera
        ' non esiste, quindi non si sceglie niente e la tendina sparisce invece di restare
        ' lì spenta a far domandare perché (cap. 03.3: quel che è acceso deve sembrarlo, e
        ' quel che non serve non deve nemmeno esserci).
        Dim siSceglie As Boolean = Not _sulCvBase AndAlso _candidatura IsNot Nothing
        cmbEsporta.Visible = siSceglie
        lblEsporta.Visible = siSceglie
        cmbEsporta.Enabled = siSceglie AndAlso Not occupato
        lblEsporta.Enabled = cmbEsporta.Enabled

        ' La tendina dei documenti è navigazione, e la navigazione si ferma mentre l'AI
        ' lavora: cambiare documento sotto una chiamata in volo è lo stesso difetto che
        ' T9d ha chiuso sulla barra in alto (cap. 03.4).
        cmbDocumento.Enabled = Not occupato AndAlso _contesto IsNot Nothing
        lblDocumento.Enabled = cmbDocumento.Enabled
        _suggerimenti.SetToolTip(cmbDocumento,
            "Salta a un altro documento già scritto: il 📄 CV base o quelli di un'altra candidatura.")

        _suggerimenti.SetToolTip(lblLingua, Nothing)

        If _sulCvBase Then
            _suggerimenti.SetToolTip(cmbLingua,
                "La lingua del 📄 CV base. Cambiandola lo riscrivo: il tuo profilo resta com'è.")
        ElseIf _candidatura IsNot Nothing Then
            _suggerimenti.SetToolTip(cmbLingua,
                "La lingua di CV e lettera. Cambiandola li riscrivo: il tuo profilo resta com'è.")
        Else
            _suggerimenti.SetToolTip(cmbLingua,
                "La lingua si sceglie su un documento: prima aprine uno.")
        End If

        ' L'email nasce dalla lettera (cap. 07.1): senza, non c'è niente da scrivere. E il
        ' 📄 CV base non porta a nessuna email, perché non ha un'azienda a cui mandarla.
        btnPreparaEmail.Enabled = Not occupato AndAlso
                                  _candidatura IsNot Nothing AndAlso
                                  _candidatura.Lettera IsNot Nothing

        If btnPreparaEmail.Enabled Then
            _suggerimenti.SetToolTip(btnPreparaEmail, Nothing)
        ElseIf _cvBase IsNot Nothing Then
            _suggerimenti.SetToolTip(btnPreparaEmail,
                "Il CV base non si manda a nessuno: l'email nasce da una candidatura.")
        Else
            _suggerimenti.SetToolTip(btnPreparaEmail, "Prima genera i documenti: l'email nasce dalla lettera.")
        End If

    End Sub

End Class
