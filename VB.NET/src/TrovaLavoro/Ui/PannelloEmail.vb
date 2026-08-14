Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore

''' <summary>
''' Pannello P7 — l'email di candidatura (cap. 03.6, cap. 07.1): destinatario, oggetto,
''' corpo e allegati da spuntare, e il bottone che scrive il file <c>.eml</c> e lo apre nel
''' programma di posta.
''' </summary>
''' <remarks>
''' <para><b>Qui il programma si ferma un passo prima.</b> Non esiste un bottone «Invia»:
''' l'app prepara una bozza e la consegna al programma di posta dell'utente, dove è già
''' autenticato (cap. 07.2). L'ultima parola è sua, e anche la prova che sia partita: «l'ho
''' spedita» è una sua dichiarazione, non un esito tecnico che l'app possa osservare.</para>
''' <para><b>È il pannello dove l'utente scrive davvero.</b> Il destinatario lo digita lui
''' — il programma non lo inventa mai — e oggetto e corpo, che l'AI propone, si correggono
''' a mano. Per questo la bozza si salva (<c>email.json</c>): riaprire la candidatura
''' domani non deve voler dire ricominciare.</para>
''' </remarks>
Public Class PannelloEmail
    Implements IPannelloArea

    ''' <summary>Sotto questa altezza la fascia delle azioni non scende: i bottoni ci devono stare.</summary>
    Private Const AltezzaMinimaAzioni As Integer = 60

    ''' <summary>Quanto spazio si prende il logo flottante (cap. 03.5).</summary>
    Private _ingombroLogo As Size

    ''' <summary>La fascia dei comandi in fondo (cap. 03.4).</summary>
    Private _comandi As FasciaDeiComandi

    Private ReadOnly _suggerimenti As New ToolTip()

    Private _contesto As ContestoApp

    ''' <summary>Chi scrive il messaggio; <c>Nothing</c> quando l'AI non c'è.</summary>
    Private _compositore As ICompositoreEmail

    ''' <summary>La candidatura a cui l'email appartiene; <c>Nothing</c> finché non ne arriva una.</summary>
    Private _candidatura As Opportunita

    ''' <summary>La bozza mostrata adesso: cambia mentre si scrive, e si salva a ogni passo che conta.</summary>
    Private _bozza As New BozzaEmail

    ''' <summary>Come in P2: un contatore, perché i riempimenti si annidano.</summary>
    Private _riempimenti As Integer

    ''' <summary>Il filo per annullare la scrittura in corso; <c>Nothing</c> se non ce n'è una.</summary>
    Private _annulla As CancellationTokenSource

    ''' <summary>Si torna ai documenti della candidatura (P6).</summary>
    Public Event TornaAiDocumenti As EventHandler

    ''' <summary>L'AI ha cominciato o finito: la finestra blocca la barra (cap. 02.6).</summary>
    Public Event LavoroAiCambiato As EventHandler

    ''' <summary>
    ''' La candidatura è passata a «inviata»: la Home deve rileggere, e il cruscotto
    ''' mostrare il numero nuovo (cap. 07.3).
    ''' </summary>
    Public Event CandidaturaInviata As EventHandler

    Public Sub New()

        InitializeComponent()

        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        VestiIBottoni()
        AggiornaComandi()

    End Sub

    ''' <summary>Collega il pannello al motore.</summary>
    ''' <param name="compositore">
    ''' Chi scrive il messaggio. Di norma si omette ed è quello del contesto; il banco
    ''' passa qui il suo, e prova il pannello intero senza chiave e senza rete — come P2
    ''' fa con l'import e P6 con la pipeline.
    ''' </param>
    Public Sub Collega(contesto As ContestoApp, Optional compositore As ICompositoreEmail = Nothing)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
        _contesto = contesto
        _compositore = If(compositore, contesto.Email)

        AggiornaComandi()

    End Sub

    ''' <summary>Se in questo momento l'AI sta scrivendo l'email.</summary>
    Public ReadOnly Property AiAlLavoro As Boolean
        Get
            Return _annulla IsNot Nothing
        End Get
    End Property

    ''' <summary>Annulla la scrittura in corso, se c'è: è la via pulita della chiusura.</summary>
    Public Sub AnnullaIlLavoro()
        _annulla?.Cancel()
    End Sub

    ' ==================================================================
    ' L'arrivo: una candidatura con i suoi documenti
    ' ==================================================================

    ''' <summary>
    ''' Mostra l'email di una candidatura. Se una bozza c'è già su disco si riapre
    ''' <b>com'era</b> — comprese le correzioni fatte a mano — e l'AI non viene disturbata:
    ''' riscrivere sopra il lavoro di ieri sarebbe il modo peggiore di essere utili.
    ''' </summary>
    Public Async Function MostraLaCandidaturaAsync(candidatura As Opportunita) As Task

        _candidatura = candidatura
        If _candidatura Is Nothing Then Return

        RiempiGliAllegati()

        Dim salvata As BozzaEmail = BozzaEmail.DaJson(_candidatura.Email)

        If salvata IsNot Nothing Then
            RiprendiLaBozza(salvata)
            Racconta("Bozza ripresa da dove l'avevi lasciata.", StileApp.TestoSecondario)
            AggiornaComandi()
            Return
        End If

        Await ScriviLaBozzaAsync()

    End Function

    ''' <summary>
    ''' Chiede all'AI oggetto e corpo, e li mette nei campi. Gli allegati spuntati fanno
    ''' parte della richiesta: il messaggio li nomina, e nominarne uno che non parte
    ''' sarebbe un'email che si smentisce da sola (cap. 07.1).
    ''' </summary>
    Private Async Function ScriviLaBozzaAsync() As Task

        If _candidatura Is Nothing OrElse _contesto Is Nothing Then Return

        If _compositore Is Nothing Then
            Racconta("Senza chiave API non posso scrivere il messaggio: puoi scriverlo a mano qui sotto.",
                     StileApp.Pericolo)
            Return
        End If

        If _candidatura.Lettera Is Nothing Then
            Racconta("Questa candidatura non ha ancora una lettera: l'email nasce da lì (cap. 07.1).",
                     StileApp.Pericolo)
            Return
        End If

        _annulla = New CancellationTokenSource()
        RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)
        AggiornaComandi()
        Racconta("Sto scrivendo il messaggio…", StileApp.TestoSecondario)

        Try
            Dim proposta = Await _compositore.ComponiAsync(
                _candidatura.Lettera, _candidatura.Annuncio,
                _bozza.AllegatiScelti().Select(Function(a) a.Nome).ToList(), _annulla.Token)

            Dim scritta As BozzaEmail = BozzaEmail.DallaProposta(proposta)

            Riempiendo(
                Sub()
                    txtOggetto.Text = scritta.Oggetto
                    txtCorpo.Text = scritta.Corpo
                End Sub)

            _bozza.Oggetto = scritta.Oggetto
            _bozza.Corpo = scritta.Corpo

            ' Il destinatario non si tocca: non viene dall'AI, e se l'utente l'ha già
            ' scritto riscriverlo sarebbe cancellargli il lavoro.
            Racconta("Messaggio scritto. Rileggilo: è quello che l'azienda legge per primo.",
                     StileApp.TestoSecondario)

        Catch ex As OperationCanceledException
            Racconta("Scrittura annullata.", StileApp.TestoSecondario)

        Catch ex As ErroreAi
            Racconta(ex.Message, StileApp.Pericolo)

        Finally
            _annulla?.Dispose()
            _annulla = Nothing
            RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)
            AggiornaComandi()
        End Try

    End Function

    ''' <summary>
    ''' Rimette nei campi una bozza salvata. Gli allegati non si sostituiscono: quelli veri
    ''' sono i file che <b>ci sono adesso</b> su disco: della bozza si riprendono le
    ''' <b>spunte</b>, e un file sparito nel frattempo non torna in vita per un elenco.
    ''' </summary>
    Private Sub RiprendiLaBozza(salvata As BozzaEmail)

        _bozza.Destinatario = salvata.Destinatario
        _bozza.Oggetto = salvata.Oggetto
        _bozza.Corpo = salvata.Corpo

        For Each allegato As AllegatoScelto In _bozza.Allegati
            Dim scelto As AllegatoScelto = salvata.Allegati.FirstOrDefault(
                Function(a) String.Equals(a.Nome, allegato.Nome, StringComparison.OrdinalIgnoreCase))

            If scelto IsNot Nothing Then allegato.Scelto = scelto.Scelto
        Next

        Riempiendo(
            Sub()
                txtDestinatario.Text = _bozza.Destinatario
                txtOggetto.Text = _bozza.Oggetto
                txtCorpo.Text = _bozza.Corpo
                MostraLeSpunte()
            End Sub)

    End Sub

    ' ==================================================================
    ' Gli allegati
    ' ==================================================================

    ''' <summary>
    ''' Riempie l'elenco con i documenti generati per questa candidatura: i file che stanno
    ''' nella sua <c>out\</c>. Sono già spuntati — sono il motivo per cui l'email esiste.
    ''' </summary>
    ''' <remarks>
    ''' Gli attestati della cartella documenti (cap. 05.2) si aggiungeranno qui accanto: la
    ''' bozza sa già distinguerli, e l'elenco è costruito per accoglierli.
    ''' </remarks>
    Private Sub RiempiGliAllegati()

        _bozza.Allegati.Clear()

        For Each percorso As String In DocumentiDellaCandidatura()
            _bozza.Allegati.Add(New AllegatoScelto With {
                .Nome = Path.GetFileName(percorso),
                .Origine = OrigineAllegato.Candidatura,
                .Scelto = ConvieneAllegarlo(percorso)})
        Next

        Riempiendo(
            Sub()
                lstAllegati.Items.Clear()
                For Each allegato As AllegatoScelto In _bozza.Allegati
                    lstAllegati.Items.Add(allegato.Nome, allegato.Scelto)
                Next
            End Sub)

    End Sub

    ''' <summary>I file prodotti per questa candidatura, in ordine di nome.</summary>
    Private Function DocumentiDellaCandidatura() As IEnumerable(Of String)

        If _candidatura Is Nothing OrElse String.IsNullOrWhiteSpace(_candidatura.Cartella) Then
            Return Enumerable.Empty(Of String)()
        End If

        Dim cartella As String = Path.Combine(_candidatura.Cartella, ArchivioOpportunita.NomeCartellaOut)
        If Not Directory.Exists(cartella) Then Return Enumerable.Empty(Of String)()

        ' Il .eml già scritto non si allega a sé stesso: sarebbe un messaggio dentro il
        ' messaggio, e la seconda volta che si preme «Prepara» ci finirebbe dentro.
        Return Directory.EnumerateFiles(cartella).
                         Where(Function(f) Not f.EndsWith(ScrittoreEml.Estensione, StringComparison.OrdinalIgnoreCase)).
                         OrderBy(Function(f) Path.GetFileName(f), StringComparer.CurrentCultureIgnoreCase)

    End Function

    ''' <summary>
    ''' Quali documenti conviene spuntare da soli. Il <b>PDF</b> sì: è il formato che si
    ''' apre uguale dappertutto, ed è quello che si manda a un'azienda. Il DOCX resta lì,
    ''' spento, per chi lo vuole — certi annunci lo chiedono espressamente (cap. 07.1).
    ''' </summary>
    Private Shared Function ConvieneAllegarlo(percorso As String) As Boolean
        Return Path.GetExtension(percorso).Equals(NomiDocumenti.EstensionePdf, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub MostraLeSpunte()

        For indice As Integer = 0 To Math.Min(lstAllegati.Items.Count, _bozza.Allegati.Count) - 1
            lstAllegati.SetItemChecked(indice, _bozza.Allegati(indice).Scelto)
        Next

    End Sub

    Private Sub lstAllegati_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles lstAllegati.ItemCheck

        If _riempimenti > 0 Then Return
        If e.Index < 0 OrElse e.Index >= _bozza.Allegati.Count Then Return

        _bozza.Allegati(e.Index).Scelto = (e.NewValue = CheckState.Checked)

        ' Il messaggio nomina gli allegati: cambiarli senza rifarlo scrivere lascia un
        ' testo che promette un file che non parte più. Non si riscrive da sé — sarebbe
        ' una chiamata all'AI a ogni spunta — ma si dice.
        Racconta("Allegati cambiati: se il messaggio li nomina, fallo riscrivere.", StileApp.TestoSecondario)

    End Sub

    ' ==================================================================
    ' I campi che l'utente scrive
    ' ==================================================================

    Private Sub txtDestinatario_TextChanged(sender As Object, e As EventArgs) Handles txtDestinatario.TextChanged

        If _riempimenti > 0 Then Return
        _bozza.Destinatario = txtDestinatario.Text
        AggiornaComandi()

    End Sub

    Private Sub txtOggetto_TextChanged(sender As Object, e As EventArgs) Handles txtOggetto.TextChanged

        If _riempimenti > 0 Then Return
        _bozza.Oggetto = txtOggetto.Text
        AggiornaComandi()

    End Sub

    Private Sub txtCorpo_TextChanged(sender As Object, e As EventArgs) Handles txtCorpo.TextChanged

        If _riempimenti > 0 Then Return
        _bozza.Corpo = txtCorpo.Text
        AggiornaComandi()

    End Sub

    ''' <summary>Come in P2: mentre si riempie, quel che cambia non è una correzione dell'utente.</summary>
    Private Sub Riempiendo(cosa As Action)

        _riempimenti += 1
        Try
            cosa()
        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ' ==================================================================
    ' I comandi
    ' ==================================================================

    Private Async Sub btnRiscrivi_Click(sender As Object, e As EventArgs) Handles btnRiscrivi.Click
        Await ScriviLaBozzaAsync()
    End Sub

    ''' <summary>
    ''' Scrive il file <c>.eml</c> nella cartella della candidatura e lo apre nel programma
    ''' di posta predefinito (cap. 12, A8). Da qui in poi il programma non tocca più niente.
    ''' </summary>
    Private Sub btnPreparaEmail_Click(sender As Object, e As EventArgs) Handles btnPreparaEmail.Click

        If _candidatura Is Nothing Then Return

        Dim percorso As String = PercorsoDelMessaggio()

        Try
            ScrittoreEml.Scrivi(percorso,
                                MittenteDalProfilo(), _bozza.Destinatario,
                                _bozza.Oggetto, _bozza.Corpo, AllegatiDaMandare())

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException

            Racconta($"Non sono riuscita a scrivere il messaggio: {ex.Message}", StileApp.Pericolo)
            Return
        End Try

        SalvaLaBozza()

        Try
            Process.Start(New ProcessStartInfo(percorso) With {.UseShellExecute = True})
            Racconta("Messaggio preparato e aperto nel tuo programma di posta. " &
                     "Quando l'hai spedito, torna qui e dimmelo.", StileApp.TestoSecondario)

        Catch ex As Exception When TypeOf ex Is Win32Exception OrElse
                                   TypeOf ex Is InvalidOperationException

            ' Il file c'è comunque, ed è la cosa che conta: Windows non sa con quale
            ' programma aprirlo, e dirglielo è una cosa che l'utente può fare.
            Racconta($"Il messaggio è scritto in «{percorso}», ma Windows non sa con quale " &
                     "programma aprire un file .eml. Aprilo dal tuo programma di posta.",
                     StileApp.Pericolo)
        End Try

        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' L'utente dichiara di aver spedito: è l'unica prova che il programma può avere
    ''' (cap. 07.3). Da qui la candidatura passa a «inviata», con la data di adesso.
    ''' </summary>
    Private Sub btnHoSpedito_Click(sender As Object, e As EventArgs) Handles btnHoSpedito.Click

        If _candidatura Is Nothing OrElse _contesto Is Nothing Then Return

        Dim risposta As DialogResult = MessageBox.Show(
            "Hai spedito la candidatura dal tuo programma di posta?" & vbLf & vbLf &
            "Se rispondi di sì la segno come inviata, con la data di adesso. " &
            "Il programma non può saperlo da solo: a spedire è il tuo programma di posta.",
            "TrovaLavoro", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)

        If risposta <> DialogResult.Yes Then Return

        SegnaComeInviata()

    End Sub

    ''' <summary>
    ''' L'atto, separato dalla domanda che lo precede: la conferma sta nel bottone, qui c'è
    ''' quel che succede dopo un sì. È anche l'unico modo di collaudarlo — una
    ''' <c>MessageBox</c> in un banco resta lì ad aspettare per sempre.
    ''' </summary>
    Public Sub SegnaComeInviata()

        If _candidatura Is Nothing OrElse _contesto Is Nothing Then Return

        Try
            SalvaLaBozza(avanzaAInviata:=True)

            Racconta($"Segnata come inviata il {Date.Now:dd/MM/yyyy} alle {Date.Now:HH:mm}.",
                     StileApp.TestoSecondario)

            RaiseEvent CandidaturaInviata(Me, EventArgs.Empty)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is InvalidOperationException

            Racconta($"Non sono riuscita a segnarla come inviata: {ex.Message}", StileApp.Pericolo)
        End Try

        AggiornaComandi()

    End Sub

    Private Sub btnTornaAiDocumenti_Click(sender As Object, e As EventArgs) Handles btnTornaAiDocumenti.Click

        ' Uscendo si salva quel che c'è: il destinatario scritto a metà e le spunte sono
        ' lavoro dell'utente, e nessuno gliel'ha chiesto di confermarlo.
        SalvaLaBozza()
        RaiseEvent TornaAiDocumenti(Me, EventArgs.Empty)

    End Sub

    ''' <summary>
    ''' Salva la bozza dentro la candidatura, e con lei tutta la cartella.
    ''' </summary>
    ''' <param name="avanzaAInviata">
    ''' Se, insieme, la candidatura passa allo stato «inviata». È un parametro e non due
    ''' metodi perché il salvataggio è uno solo: scrivere lo stato senza la bozza — o il
    ''' contrario — lascerebbe su disco due verità diverse.
    ''' </param>
    Private Sub SalvaLaBozza(Optional avanzaAInviata As Boolean = False)

        If _candidatura Is Nothing OrElse _contesto Is Nothing Then Return

        _candidatura.Email = _bozza.ComeJson()

        If avanzaAInviata AndAlso _candidatura.Stato <> StatoOpportunita.Inviata Then
            _candidatura.Avanza(StatoOpportunita.Inviata, Date.Now)
        End If

        _contesto.Opportunita.Salva(_candidatura)

    End Sub

    ''' <summary>Dove va scritto il messaggio: accanto ai documenti che porta con sé.</summary>
    Private Function PercorsoDelMessaggio() As String

        Dim nome As String = NomiDocumenti.Lettera(_candidatura.Azienda, _candidatura.Creata, _candidatura.Lingua).
            Replace("Lettera", "Email")

        Return Path.Combine(_candidatura.Cartella, ArchivioOpportunita.NomeCartellaOut,
                            nome & ScrittoreEml.Estensione)

    End Function

    ''' <summary>Gli allegati spuntati, già risolti in file veri.</summary>
    Private Function AllegatiDaMandare() As List(Of AllegatoEmail)

        Dim daMandare As New List(Of AllegatoEmail)

        For Each allegato As AllegatoScelto In _bozza.AllegatiScelti()

            Dim percorso As String = BozzaEmail.PercorsoDi(allegato, _candidatura.Cartella)

            ' Un file sparito nel frattempo non ferma il messaggio: l'utente ha appena
            ' guardato l'elenco, e bloccare tutto per un file in meno sarebbe sproporzionato.
            ' Che manchi si vede — l'elenco lo dirà alla prossima apertura.
            If percorso IsNot Nothing AndAlso File.Exists(percorso) Then
                daMandare.Add(New AllegatoEmail(percorso, allegato.Nome))
            End If

        Next

        Return daMandare

    End Function

    ''' <summary>
    ''' L'indirizzo di chi si candida, preso dal profilo. Se il profilo non ce l'ha, il
    ''' messaggio parte senza mittente e a metterlo sarà il programma di posta, che
    ''' l'account ce l'ha: meglio così che inventarne uno.
    ''' </summary>
    Private Function MittenteDalProfilo() As String

        If _contesto Is Nothing OrElse Not _contesto.Archivio.Esiste Then Return ""

        Try
            Return If(_contesto.Archivio.Carica().Contatti?.Email, "")
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is InvalidDataException
            Return ""
        End Try

    End Function

    ' ==================================================================
    ' Aspetto, spazio e stato dei comandi
    ' ==================================================================

    ''' <inheritdoc/>
    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        _ingombroLogo = ingombro
        pnlAzioni.Padding = New Padding(ingombro.Width + StileApp.DistanzaControlli, 0, 0, 0)

        DisponiLeAzioni()

    End Sub

    Private Sub DisponiLeAzioni()

        If _comandi Is Nothing Then
            _comandi = New FasciaDeiComandi(pnlAzioni)
            _comandi.ASinistra(btnTornaAiDocumenti, btnRiscrivi)
            _comandi.ADestra(btnHoSpedito, btnPreparaEmail)
        End If

        _comandi.Disponi(Math.Max(AltezzaMinimaAzioni, _ingombroLogo.Height))

    End Sub

    Private Sub PannelloEmail_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        DisponiLeAzioni()
    End Sub

    Private Sub VestiIBottoni()

        StileApp.VestiBottone(btnPreparaEmail, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnHoSpedito, LivelloBottone.SicuroPositivo)
        StileApp.VestiBottone(btnTornaAiDocumenti, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnRiscrivi, LivelloBottone.Esplorativo)

    End Sub

    ''' <summary>
    ''' Chi può fare cosa, adesso. Le due condizioni che contano: senza un messaggio non
    ''' c'è niente da preparare, e senza averlo preparato non c'è niente da spedire — un
    ''' «l'ho spedita» premuto prima segnerebbe come inviata una candidatura che non è mai
    ''' uscita di qui.
    ''' </summary>
    Private Sub AggiornaComandi()

        Dim occupato As Boolean = AiAlLavoro
        Dim conCandidatura As Boolean = _candidatura IsNot Nothing

        btnRiscrivi.Enabled = conCandidatura AndAlso Not occupato AndAlso
                              _compositore IsNot Nothing

        btnPreparaEmail.Enabled = conCandidatura AndAlso Not occupato AndAlso
                                  Not String.IsNullOrWhiteSpace(_bozza.Corpo)

        btnHoSpedito.Enabled = conCandidatura AndAlso Not occupato AndAlso MessaggioGiaScritto()

        btnTornaAiDocumenti.Enabled = Not occupato

        If Not btnPreparaEmail.Enabled AndAlso conCandidatura AndAlso Not occupato Then
            _suggerimenti.SetToolTip(btnPreparaEmail, "Prima serve un messaggio: fallo scrivere o scrivilo tu.")
        Else
            _suggerimenti.SetToolTip(btnPreparaEmail, Nothing)
        End If

        If Not btnHoSpedito.Enabled AndAlso conCandidatura AndAlso Not occupato Then
            _suggerimenti.SetToolTip(btnHoSpedito, "Prima prepara l'email: si spedisce dal programma di posta.")
        Else
            _suggerimenti.SetToolTip(btnHoSpedito, Nothing)
        End If

    End Sub

    ''' <summary>Se il file del messaggio esiste già su disco.</summary>
    Private Function MessaggioGiaScritto() As Boolean

        If _candidatura Is Nothing OrElse String.IsNullOrWhiteSpace(_candidatura.Cartella) Then Return False
        Return File.Exists(PercorsoDelMessaggio())

    End Function

    Private Sub Racconta(testo As String, colore As Color)

        lblStatoEmail.Text = testo
        lblStatoEmail.ForeColor = colore

    End Sub

End Class
