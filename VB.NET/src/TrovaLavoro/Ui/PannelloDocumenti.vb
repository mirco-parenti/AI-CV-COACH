Imports System.Drawing
Imports System.IO
Imports System.Linq
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
''' <para><b>Intero nella struttura, parziale nelle funzioni</b> (cap. 03.6): anteprime ed
''' esportazioni funzionano; la scelta della lingua e il prima/dopo dell'anti-slop sono
''' lì spenti — arrivano con T7 — e «Prepara email» con T6. Chi guarda l'applicazione a
''' metà strada deve vedere dove sta andando.</para>
''' <para><b>Le anteprime passano dalla pagina di blocchi</b>, non dal JSON: le stampanti
''' sono tre — DOCX, PDF e questa, a video — e leggono tutte lo stesso modello (cap. 05.3).
''' Se l'anteprima leggesse il JSON per conto suo, mostrerebbe un documento che i file non
''' contengono, proprio dove l'utente controlla.</para>
''' <para><b>Rientrare non rigenera.</b> Un'opportunità che ha già i suoi documenti li
''' mostra e basta: rifarli costa un'attesa e dei token, e li cambierebbe sotto il naso di
''' chi li aveva già letti. A rifarli è «Rigenera», che lo dichiara.</para>
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

    ''' <summary>
    ''' Chi mette i documenti al loro posto. Lo costruisce la finestra, non il motore: si
    ''' porta dietro la stampante PDF, che vive sul thread dell'interfaccia.
    ''' </summary>
    Private _documenti As ArchivioDocumenti

    ''' <summary>La candidatura in mostra; <c>Nothing</c> se si sta guardando il 📄 CV base.</summary>
    Private _candidatura As Opportunita

    ''' <summary>Il 📄 CV base in mostra; <c>Nothing</c> se si sta guardando una candidatura.</summary>
    Private _cvBase As JsonNode

    Private _annulla As CancellationTokenSource

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
    Public Sub Collega(contesto As ContestoApp, documenti As ArchivioDocumenti,
                       Optional pipeline As PipelineCandidatura = Nothing,
                       Optional generatore As IGeneratore = Nothing)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
        If documenti Is Nothing Then Throw New ArgumentNullException(NameOf(documenti))

        _contesto = contesto
        _documenti = documenti
        _pipeline = If(pipeline, contesto.Pipeline)
        _generatore = If(generatore, contesto.Generatore)

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

        _cvBase = Nothing
        Mostra()
        RaccontaLoStato("Il profilo è stato eliminato: il 📄 CV base che era qui non c'è più.",
                        StileApp.TestoSecondario)

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
        _cvBase = Nothing

        Mostra()

        ' Già generati: si guardano e basta. A rifarli c'è «Rigenera», che lo dichiara.
        If candidatura.Cv IsNot Nothing AndAlso candidatura.Lettera IsNot Nothing Then
            RaccontaLoStato("Rileggili con calma: se qualcosa non ti convince, puoi rigenerarli.",
                            StileApp.TestoSecondario)
            Return
        End If

        Await GeneraLaCandidaturaAsync().ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' Genera e mostra il 📄 CV base: dal solo profilo, senza alcun annuncio. È la strada
    ''' che arriva dalla scheda P2 (cap. 12.2, punto 5).
    ''' </summary>
    Public Async Function MostraIlCvBaseAsync() As Task

        If AiAlLavoro Then Return

        _candidatura = Nothing
        _cvBase = Nothing

        Mostra()

        If _generatore Is Nothing Then
            RaccontaLoStato(MotivoSenzaAi(), StileApp.Pericolo)
            Return
        End If

        Dim profilo As Profilo = LeggiIlProfilo()
        If profilo Is Nothing Then Return

        Using filo As New CancellationTokenSource()

            _annulla = filo
            LavoroInCorso(True)

            Try
                RaccontaLoStato("Scrivo il tuo 📄 CV base…", StileApp.TestoSecondario)

                _cvBase = Await _generatore.GeneraCvBaseAsync(
                    JsonNode.Parse(profilo.ComeTesto()), filo.Token).ConfigureAwait(True)

                Mostra()
                RaccontaLoStato(ArchiviaIlCvBase(), StileApp.TestoSecondario)

            Catch ex As OperationCanceledException
                RaccontaLoStato("Generazione annullata: non ho scritto niente.", StileApp.TestoSecondario)

            Catch ex As ErroreAi
                RaccontaLoStato(ex.Message, StileApp.Pericolo)

            Finally
                _annulla = Nothing
                LavoroInCorso(False)
            End Try

        End Using

    End Function

    ''' <summary>
    ''' I due passi della generazione: il 🎯 CV mirato e la ✉️ lettera, in quest'ordine
    ''' perché la lettera riceve il CV e deve raccontare la stessa storia.
    ''' </summary>
    Private Async Function GeneraLaCandidaturaAsync() As Task

        If _pipeline Is Nothing Then
            RaccontaLoStato(MotivoSenzaAi(), StileApp.Pericolo)
            Return
        End If

        Dim profilo As Profilo = LeggiIlProfilo()
        If profilo Is Nothing Then Return

        Using filo As New CancellationTokenSource()

            _annulla = filo
            LavoroInCorso(True)

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
                RaccontaLoStato(ex.Message & vbLf & "Puoi riprovare con «Rigenera».", StileApp.Pericolo)

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

    ''' <summary>Scrive il 📄 CV base accanto al profilo, annotando da quale versione nasce.</summary>
    Private Function ArchiviaIlCvBase() As String

        Try
            _contesto.Archivio.SalvaCvBase(_cvBase, _contesto.Archivio.Versioni().LastOrDefault())
            Return "Il 📄 CV base è pronto ed è salvato col tuo profilo." & vbLf &
                   "Esportalo in DOCX o PDF quando ti va bene."

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Return $"Il 📄 CV base è pronto, ma non sono riuscita a salvarlo ({ex.Message})."
        End Try

    End Function

    ' ==================================================================
    ' Esportare
    ' ==================================================================

    ''' <summary>
    ''' Scrive i file nel formato chiesto e dice quali sono. È il metodo che premono i due
    ''' bottoni d'esportazione; il banco lo chiama direttamente.
    ''' </summary>
    Public Async Function EsportaAsync(formati As FormatiDocumento) As Task

        If _documenti Is Nothing OrElse Not CEQualcosaDaEsportare() Then Return

        Cursor = Cursors.AppStarting

        Try
            Dim scritti As IReadOnlyList(Of String) = Await ScriviAsync(formati).ConfigureAwait(True)

            If scritti.Count = 0 Then
                RaccontaLoStato("Non c'era ancora niente da esportare.", StileApp.TestoSecondario)
                Return
            End If

            RaccontaLoStato(
                "Ho scritto:" & vbLf & String.Join(vbLf, scritti.Select(AddressOf Path.GetFileName)) & vbLf &
                $"in «{Path.GetDirectoryName(scritti(0))}».",
                StileApp.TestoSecondario)

        Catch ex As ErroreStampa
            ' Il messaggio si porta dietro il ripiego onesto: il DOCX c'è comunque
            ' (cap. 13.3).
            RaccontaLoStato(ex.Message, StileApp.Pericolo)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            RaccontaLoStato($"Non sono riuscita a scrivere i file: {ex.Message}", StileApp.Pericolo)

        Finally
            Cursor = Cursors.Default
        End Try

    End Function

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
            Return _documenti.ScriviCvBaseAsync(_cvBase, Nothing, formati)
        End If

        ' Senza cartella non c'è un «dove»: l'opportunità arriva qui già salvata da P4, ma
        ' se così non fosse la si salva adesso invece di rifiutare l'esportazione — e una
        ' cartella nata qui deve comparire nella Home come tutte le altre.
        If String.IsNullOrEmpty(_candidatura.Cartella) Then
            _contesto.Opportunita.Salva(_candidatura)
            AnnotaNelRegistro()
        End If

        Return _documenti.ScriviCandidaturaAsync(_candidatura, formati)

    End Function

    Private Function CEQualcosaDaEsportare() As Boolean
        Return _cvBase IsNot Nothing OrElse (_candidatura IsNot Nothing AndAlso _candidatura.Cv IsNot Nothing)
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
        btnTornaIndietro.Text = If(_candidatura Is Nothing, "Torna al profilo", "Torna all'opportunità")

        If _cvBase IsNot Nothing Then
            lblCv.Text = "📄 CV base"
            txtAnnuncio.Text = "Il 📄 CV base non nasce da un annuncio: è il ritratto del tuo profilo."
            txtCv.Text = ScrittoreTesto.Componi(Impaginazione.PaginaCv(_cvBase))
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

        txtAnnuncio.Text = VistaAnnuncio.Riassunto(_candidatura.Annuncio)
        txtCv.Text = Anteprima(_candidatura.Cv, AddressOf Impaginazione.PaginaCv,
                               "Il CV non è ancora stato scritto.")
        txtLettera.Text = Anteprima(_candidatura.Lettera, AddressOf Impaginazione.PaginaLettera,
                                    "La lettera non è ancora stata scritta.")

        AggiornaComandi()

    End Sub

    ''' <summary>Un documento come si legge, o il motivo per cui non c'è ancora.</summary>
    Private Shared Function Anteprima(documento As JsonNode,
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
            RaccontaLoStato($"Il profilo c'è ma non si lascia leggere: {ex.Message}", StileApp.Pericolo)
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

        ' Rigenerare butta via testi che l'utente potrebbe aver già letto e approvato, e
        ' costa un'altra attesa: si chiede prima (cap. 03.3, livello 4).
        If MessageBox.Show(
            "Vuoi che li riscriva da capo?" & vbLf &
            "I testi che vedi adesso vengono sostituiti.",
            NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) <> DialogResult.Yes Then Return

        Await RigeneraAsync()

    End Sub

    ''' <summary>Riscrive da capo quello che si sta guardando.</summary>
    Public Async Function RigeneraAsync() As Task

        If AiAlLavoro Then Return

        If _cvBase IsNot Nothing OrElse _candidatura Is Nothing Then
            Await MostraIlCvBaseAsync().ConfigureAwait(True)
            Return
        End If

        ' I documenti di prima si buttano adesso: se la generazione fallisce a metà, il
        ' pannello non deve mostrare un CV vecchio accanto a una lettera nuova.
        _candidatura.Cv = Nothing
        _candidatura.Lettera = Nothing
        Mostra()

        Await GeneraLaCandidaturaAsync().ConfigureAwait(True)

    End Function

    Private Async Sub btnEsportaDocx_Click(sender As Object, e As EventArgs) Handles btnEsportaDocx.Click
        Await EsportaAsync(FormatiDocumento.Docx)
    End Sub

    Private Async Sub btnEsportaPdf_Click(sender As Object, e As EventArgs) Handles btnEsportaPdf.Click
        Await EsportaAsync(FormatiDocumento.Pdf)
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
            _comandi.ASinistra(btnTornaIndietro, btnRigenera)
            _comandi.ADestra(btnPreparaEmail, btnEsportaPdf, btnEsportaDocx)
        End If

        _comandi.Disponi(Math.Max(AltezzaMinimaAzioni, _ingombroLogo.Height))

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

        StileApp.VestiBottone(btnEsportaDocx, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnEsportaPdf, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnPreparaEmail, LivelloBottone.AzionePrincipale)

    End Sub

    ''' <summary>
    ''' Ciò che arriverà, visibile e spento, col suo suggerimento (cap. 03.8). Questo è il
    ''' pannello che più di ogni altro mostra dove il progetto sta andando.
    ''' </summary>
    Private Sub DichiaraLeTappeCheMancano()

        cmbLingua.SelectedIndex = 0
        cmbLingua.Enabled = False
        _suggerimenti.SetToolTip(cmbLingua, "I documenti in inglese arrivano con la tappa T7.")
        _suggerimenti.SetToolTip(lblLingua, "I documenti in inglese arrivano con la tappa T7.")

        chkRifinitura.Enabled = False
        _suggerimenti.SetToolTip(chkRifinitura,
                                 "La rifinitura anti-slop, col prima/dopo, arriva con la tappa T7.")

        ' «Prepara email» non sta più qui: T6 è arrivata, e il bottone lo accende
        ' AggiornaComandi quando c'è una candidatura con la sua lettera.

    End Sub

    Private Sub RaccontaLoStato(testo As String, colore As Color)

        lblStatoDocumenti.Text = testo
        lblStatoDocumenti.ForeColor = colore

    End Sub

    ''' <summary>Perché non si può generare: la chiave, o i prompt.</summary>
    Private Function MotivoSenzaAi() As String

        If _contesto IsNot Nothing AndAlso _contesto.Libreria Is Nothing Then
            Return "I prompt non si sono caricati: senza di loro non posso scrivere né il CV né la lettera."
        End If

        Return $"Per scrivere i documenti serve la chiave API ({ClientClaude.NomeVariabileChiave})."

    End Function

    Private Sub LavoroInCorso(inCorso As Boolean)

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

        btnRigenera.Enabled = Not occupato AndAlso
                              (_cvBase IsNot Nothing OrElse _candidatura IsNot Nothing) AndAlso
                              (_pipeline IsNot Nothing OrElse _generatore IsNot Nothing)

        btnTornaIndietro.Enabled = Not occupato

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
