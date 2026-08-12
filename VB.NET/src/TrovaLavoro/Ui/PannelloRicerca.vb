Imports System.Drawing
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.Web.WebView2.Core
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore
Imports TrovaLavoro.Web

''' <summary>
''' Un annuncio catturato dalla pagina che l'utente stava guardando: il testo da
''' analizzare, e da dove viene (cap. 06.4).
''' </summary>
Public Class AnnuncioCatturatoEventArgs
    Inherits EventArgs

    Public Sub New(testo As String, fonte As String, link As String)

        Me.Testo = testo
        Me.Fonte = fonte
        Me.Link = link

    End Sub

    ''' <summary>Il testo visibile della pagina: è ciò che andrà all'analisi.</summary>
    Public ReadOnly Property Testo As String

    ''' <summary>Il portale, se è uno dei nostri; altrimenti il sito.</summary>
    Public ReadOnly Property Fonte As String

    ''' <summary>L'indirizzo della pagina catturata.</summary>
    Public ReadOnly Property Link As String

End Class

''' <summary>
''' Pannello P3 — la ricerca degli annunci (cap. 06; cap. 12, A3-A4): un browser vero
''' dentro l'applicazione, dove naviga l'utente, con le ricerche salvate sopra e la
''' cattura sotto.
''' </summary>
''' <remarks>
''' <para><b>Chi naviga è l'utente.</b> Il programma prepara gli indirizzi di ricerca e
''' apre le pagine che gli si chiede; non raccoglie niente in automatico, non fa login al
''' posto di nessuno e non vede le credenziali, che l'utente digita nel browser come
''' farebbe in Edge (cap. 06.1, cap. 06.6). È la bussola etica della funzione, e qui è
''' anche un fatto di codice: l'unica navigazione che parte da sola è la <b>pagina di
''' casa</b>, che è scritta da noi e non tocca la rete.</para>
''' <para><b>Il motore si accende quando il pannello entra in vista</b>, non alla nascita
''' del controllo: un'applicazione aperta e chiusa senza mai cercare non deve lasciarsi
''' dietro il profilo di un browser mai usato (v. <c>CartellaDati.CartellaWebView2</c>), e
''' il banco di collaudo può costruire il pannello e provarne i comandi senza pretendere
''' WebView2 e un thread STA.</para>
''' <para><b>L'ambiente non è suo</b>: lo chiede al <see cref="MotoreBrowser"/>, che è
''' l'unico dell'applicazione e lo condivide con la stampa dei PDF. Il cancello di T5a
''' (2026-08-12) ha misurato perché: due ambienti sulla stessa cartella di navigazione
''' stanno buoni solo finché nessuno tocca loro un'opzione, e quando si rompono lo fanno
''' con un <c>Class not registered</c> che non spiega niente.</para>
''' <para><b>Le finestre nuove restano dentro.</b> Sui portali molti risultati aprono una
''' scheda nuova: in una WebView2 senza gestore quel clic non fa <i>niente</i>, e l'utente
''' penserebbe che il programma è rotto. Qui la richiesta viene presa e la pagina si apre
''' nella stessa vista — dove il bottone «indietro» sa riportarlo ai risultati.</para>
''' </remarks>
Public Class PannelloRicerca
    Implements IPannelloArea

    ''' <summary>Sotto questa altezza la fascia delle azioni non scende: i bottoni ci devono stare.</summary>
    Private Const AltezzaMinimaAzioni As Integer = 64

    ''' <summary>
    ''' Sotto questi caratteri la pagina non si manda all'analisi. Non è un modo di
    ''' indovinare se è un annuncio — quello lo dice l'AI (cap. 06.4) — ma di non spendere
    ''' una chiamata e un'attesa per una pagina che non ha testo: una scheda vuota, una
    ''' pagina che non si è caricata, un annuncio dentro un <c>iframe</c> che il lettore
    ''' non vede. Il più corto degli annunci veri sta comodamente sopra.
    ''' </summary>
    Private Const MinimoTestoDellaPagina As Integer = 200

    Private ReadOnly _suggerimenti As New ToolTip()

    Private _contesto As ContestoApp

    ''' <summary>L'unico motore del browser dell'applicazione.</summary>
    Private _motore As MotoreBrowser

    ''' <summary>I portali e le ricerche salvate: lo stesso oggetto che ha il contesto.</summary>
    Private _ricerche As Ricerche

    ''' <summary>
    ''' Chi legge la pagina aperta. Di norma nasce dalla vista al momento della cattura;
    ''' il banco ne passa uno finto e prova la cattura senza WebView2.
    ''' </summary>
    Private _lettore As ILettorePagina

    ''' <summary>Se la vista è già accesa: l'accensione si fa una volta per sessione.</summary>
    Private _accesa As Boolean

    ''' <summary>
    ''' Se il motore ha rifiutato di accendersi. Da lì in poi i comandi che navigano
    ''' restano spenti: premerli non farebbe niente, e un bottone che non fa niente è
    ''' peggio di un bottone spento che si spiega.
    ''' </summary>
    Private _browserFuoriUso As Boolean

    ''' <summary>
    ''' L'utente ha catturato un annuncio dalla pagina aperta: la finestra lo porta alla
    ''' scheda della candidatura. Il pannello non conosce gli altri pannelli — dice cosa
    ''' ha in mano (come <c>PannelloOpportunita.DocumentiRichiesti</c>).
    ''' </summary>
    Public Event AnnuncioCatturato As EventHandler(Of AnnuncioCatturatoEventArgs)

    Public Sub New()

        InitializeComponent()

        ' Come negli altri pannelli: il ToolTip nasce qui e muore col pannello.
        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        VestiIBottoni()

        _suggerimenti.SetToolTip(btnCattura,
            "Legge l'annuncio dalla pagina aperta e lo manda all'analisi.")

        AggiornaComandi()

    End Sub

    ''' <summary>Collega il pannello al motore dell'applicazione e al browser.</summary>
    ''' <param name="contesto">Il motore montato all'avvio: da lì vengono le ricerche.</param>
    ''' <param name="motore">
    ''' Il motore del browser. Il banco lo omette: senza di lui il pannello si costruisce e
    ''' i suoi comandi si provano, ma non si naviga.
    ''' </param>
    ''' <param name="lettore">
    ''' Chi legge la pagina. Di norma si omette ed è la vista stessa; il banco passa qui il
    ''' suo, e prova la cattura senza pretendere WebView2 (v. <see cref="ILettorePagina"/>).
    ''' </param>
    Public Sub Collega(contesto As ContestoApp, Optional motore As MotoreBrowser = Nothing,
                       Optional lettore As ILettorePagina = Nothing)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))

        _contesto = contesto
        _motore = motore
        _lettore = lettore
        _ricerche = contesto.Ricerche

        RiempiIMenu()
        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Il pannello entra in vista: accende il browser, se non lo è già, e mostra la pagina
    ''' di casa. Non solleva — un browser che non parte è un avviso nel pannello, non un
    ''' crash dell'applicazione (cap. 03.8).
    ''' </summary>
    Public Async Function ApriAsync() As Task

        If _accesa OrElse _browserFuoriUso OrElse _motore Is Nothing Then Return

        Racconta("Accendo il browser…")

        Try
            Await vista.EnsureCoreWebView2Async(Await _motore.AmbienteAsync()).ConfigureAwait(True)

        Catch ex As ErroreBrowser When ex.MotoreAssente
            FuoriUso("Per cercare gli annunci serve il componente WebView2 di Windows, che su " &
                     "questo computer non c'è. Puoi comunque incollare il testo di un annuncio " &
                     "nel pannello Candidatura.")
            Return

        Catch ex As Exception
            ' Qui il guasto arriva davvero: il cancello di T5a ha mostrato che accendere
            ' l'ambiente riesce quasi sempre, ed è l'accensione della vista a cadere.
            FuoriUso($"Il browser integrato non si è avviato ({ex.Message}). Puoi comunque " &
                     "incollare il testo di un annuncio nel pannello Candidatura.")
            Return

        End Try

        _accesa = True

        AddHandler vista.CoreWebView2.NewWindowRequested, AddressOf AllaFinestraNuova
        AddHandler vista.CoreWebView2.HistoryChanged, AddressOf AllaStoriaCambiata
        AddHandler vista.NavigationCompleted, AddressOf AFineNavigazione
        AddHandler vista.SourceChanged, AddressOf AllIndirizzoCambiato

        vista.CoreWebView2.NavigateToString(PaginaDiCasa())

        Racconta(Nothing)
        AggiornaComandi()

    End Function

    ' ==================================================================
    ' Le ricerche
    ' ==================================================================

    ''' <summary>Riempie i due menù: i portali conosciuti e le ricerche messe da parte.</summary>
    Private Sub RiempiIMenu()

        cboPortali.Items.Clear()
        For Each portale As Portale In _ricerche.Portali
            cboPortali.Items.Add(portale.Nome)
        Next
        If cboPortali.Items.Count > 0 Then cboPortali.SelectedIndex = 0

        RiempiLeSalvate(Nothing)

    End Sub

    ''' <summary>
    ''' Rifà il menù delle ricerche salvate, riportandoci sopra quella indicata (o la
    ''' prima, se c'è).
    ''' </summary>
    Private Sub RiempiLeSalvate(daSelezionare As String)

        cboSalvate.Items.Clear()
        For Each salvata As RicercaSalvata In _ricerche.Salvate
            cboSalvate.Items.Add(salvata.Nome)
        Next

        If cboSalvate.Items.Count = 0 Then Return

        Dim quale As Integer = cboSalvate.Items.IndexOf(If(daSelezionare, String.Empty))
        cboSalvate.SelectedIndex = Math.Max(0, quale)

    End Sub

    ''' <summary>La ricerca salvata scelta nel menù, o <c>Nothing</c>.</summary>
    Private Function SalvataScelta() As RicercaSalvata

        If cboSalvate.SelectedItem Is Nothing Then Return Nothing

        Return _ricerche.Salvate.FirstOrDefault(
            Function(s) s.Nome = cboSalvate.SelectedItem.ToString())

    End Function

    ''' <summary>Il portale scelto nel menù, o <c>Nothing</c>.</summary>
    Private Function PortaleScelto() As Portale

        If cboPortali.SelectedItem Is Nothing Then Return Nothing

        Return _ricerche.TrovaPortale(cboPortali.SelectedItem.ToString())

    End Function

    Private Async Sub btnCerca_Click(sender As Object, e As EventArgs) Handles btnCerca.Click

        Dim portale As Portale = PortaleScelto()
        If portale Is Nothing Then Return

        If String.IsNullOrWhiteSpace(txtCosa.Text) AndAlso String.IsNullOrWhiteSpace(txtDove.Text) Then
            Racconta("Dimmi almeno cosa cerchi, o dove: una ricerca vuota non porta da nessuna parte.")
            Return
        End If

        Await NavigaAsync(portale.ComponiUrl(txtCosa.Text, txtDove.Text))

    End Sub

    Private Async Sub btnApri_Click(sender As Object, e As EventArgs) Handles btnApri.Click

        Dim salvata As RicercaSalvata = SalvataScelta()
        If salvata Is Nothing Then Return

        ' Aprire una ricerca salvata riporta i suoi valori nelle caselle: da lì si può
        ' ritoccarla e cercare di nuovo, che è come nascono le ricerche seguenti.
        cboPortali.SelectedItem = salvata.Portale
        txtCosa.Text = salvata.Cosa
        txtDove.Text = salvata.Dove

        Dim portale As Portale = _ricerche.TrovaPortale(salvata.Portale)
        If portale Is Nothing Then Return

        Await NavigaAsync(portale.ComponiUrl(salvata.Cosa, salvata.Dove))

    End Sub

    Private Sub btnSalvaRicerca_Click(sender As Object, e As EventArgs) Handles btnSalvaRicerca.Click

        Dim portale As Portale = PortaleScelto()
        If portale Is Nothing Then Return

        If String.IsNullOrWhiteSpace(txtCosa.Text) AndAlso String.IsNullOrWhiteSpace(txtDove.Text) Then
            Racconta("Una ricerca senza cosa né dove non vale la pena di salvarla.")
            Return
        End If

        Dim ricerca As New RicercaSalvata With {
            .Nome = NomeProposto(portale.Nome, txtCosa.Text, txtDove.Text),
            .Portale = portale.Nome,
            .Cosa = txtCosa.Text,
            .Dove = txtDove.Text}

        _ricerche.MettiDaParte(ricerca)

        If Not Scrivi() Then Return

        RiempiLeSalvate(ricerca.Nome.Trim())
        AggiornaComandi()
        Racconta($"Ricerca salvata: «{ricerca.Nome.Trim()}».")

    End Sub

    Private Sub btnDimentica_Click(sender As Object, e As EventArgs) Handles btnDimentica.Click

        Dim salvata As RicercaSalvata = SalvataScelta()
        If salvata Is Nothing Then Return

        ' Cancellare è di livello 5: si chiede prima (cap. 03.3, cap. 12.7).
        Dim risposta As DialogResult = MessageBox.Show(
            $"Dimentico la ricerca «{salvata.Nome}»?" & vbLf &
            "Le ricerche salvate si possono rifare in qualunque momento.",
            "Ricerca", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2)

        If risposta <> DialogResult.Yes Then Return

        _ricerche.Dimentica(salvata.Nome)

        If Not Scrivi() Then Return

        RiempiLeSalvate(Nothing)
        AggiornaComandi()
        Racconta($"Ricerca dimenticata: «{salvata.Nome}».")

    End Sub

    ''' <summary>
    ''' Il nome che si propone per una ricerca nuova: portale, cosa e dove. È prevedibile
    ''' di proposito — chi salva due volte la stessa ricerca ritrova una voce sola, perché
    ''' il nome coincide (v. <c>Ricerche.MettiDaParte</c>).
    ''' </summary>
    Private Shared Function NomeProposto(portale As String, cosa As String, dove As String) As String

        Dim parti As String() = {cosa, dove}.
            Where(Function(p) Not String.IsNullOrWhiteSpace(p)).
            Select(Function(p) p.Trim()).ToArray()

        Return $"{portale} — {String.Join(", ", parti)}"

    End Function

    ''' <summary>
    ''' Scrive le ricerche su disco; dice se c'è riuscita. Un disco che non si lascia
    ''' scrivere non deve far cadere il pannello: si racconta e si va avanti con quello che
    ''' c'è in memoria.
    ''' </summary>
    Private Function Scrivi() As Boolean

        Try
            _contesto.ArchivioRicerche.Salva(_ricerche)
            Return True

        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Racconta($"Non sono riuscita a salvare le ricerche ({ex.Message}). " &
                     "Per questa sessione valgono comunque.")
            Return False

        End Try

    End Function

    ' ==================================================================
    ' La navigazione
    ' ==================================================================

    Private Async Sub btnVai_Click(sender As Object, e As EventArgs) Handles btnVai.Click
        Await VaiAllIndirizzoScritto()
    End Sub

    Private Async Function VaiAllIndirizzoScritto() As Task

        Dim scritto As String = txtIndirizzo.Text.Trim()
        If scritto.Length = 0 Then Return

        ' Un indirizzo incollato senza «https://» davanti è la norma quando si copia da
        ' un'email o da un messaggio: si completa invece di rifiutarlo.
        If Not scritto.Contains("://") Then scritto = "https://" & scritto

        Dim indirizzo As Uri = Nothing
        If Not Uri.TryCreate(scritto, UriKind.Absolute, indirizzo) OrElse
           (indirizzo.Scheme <> Uri.UriSchemeHttp AndAlso indirizzo.Scheme <> Uri.UriSchemeHttps) Then
            Racconta("Quello non sembra l'indirizzo di una pagina web.")
            Return
        End If

        Await NavigaAsync(indirizzo.AbsoluteUri)

    End Function

    ''' <summary>Porta la vista all'indirizzo indicato, accendendo il browser se serve.</summary>
    Private Async Function NavigaAsync(indirizzo As String) As Task

        Await ApriAsync().ConfigureAwait(True)

        If Not _accesa Then Return

        Racconta(Nothing)
        vista.CoreWebView2.Navigate(indirizzo)

    End Function

    ' ==================================================================
    ' La cattura
    ' ==================================================================

    Private Async Sub btnCattura_Click(sender As Object, e As EventArgs) Handles btnCattura.Click
        Await CatturaAsync()
    End Sub

    ''' <summary>
    ''' Legge la pagina che l'utente sta guardando e la consegna all'analisi (cap. 06.4).
    ''' </summary>
    ''' <remarks>
    ''' <para>È il metodo che preme «Cattura annuncio»; il banco lo chiama direttamente,
    ''' perché di un gestore di clic non si può aspettare la fine (come in P4).</para>
    ''' <para><b>Qui non si giudica se sia un annuncio</b>: quello lo dice l'analisi, e il
    ''' pannello non ha nessun titolo per indovinarlo dal testo. L'unica cosa che si
    ''' controlla è che una pagina da mandare ci sia — perché mandare il nulla costerebbe
    ''' un'attesa e una chiamata per sentirsi dire quel che si sapeva già.</para>
    ''' </remarks>
    Public Async Function CatturaAsync() As Task

        Dim lettore As ILettorePagina = LettoreDellaPagina()
        If lettore Is Nothing Then Return

        Racconta("Leggo la pagina…")

        Dim pagina As PaginaLetta

        Try
            pagina = Await lettore.LeggiAsync().ConfigureAwait(True)

        Catch ex As Exception
            ' Una pagina che non si lascia leggere non è un crash: è un annuncio che si
            ' potrà sempre incollare a mano in Candidatura.
            Racconta($"Non sono riuscita a leggere questa pagina ({ex.Message}). " &
                     "Puoi copiarne il testo e incollarlo nel pannello Candidatura.")
            Return

        End Try

        Dim testo As String = If(pagina?.Testo, String.Empty).Trim()

        If testo.Length < MinimoTestoDellaPagina Then
            Racconta("In questa pagina non c'è testo da leggere. Aspetta che finisca di " &
                     "caricarsi, oppure apri la pagina di un singolo annuncio.")
            Return
        End If

        ' Premere due volte sulla stessa pagina non deve produrre due candidature
        ' identiche: costerebbe due chiamate all'AI per riscrivere quel che c'è già, e
        ' lascerebbe nella coda di T5c due voci che l'utente non sa distinguere.
        Dim gia As String = GiaCatturato(pagina.Indirizzo)
        If gia IsNot Nothing Then
            Racconta($"Questo annuncio l'avevi già catturato: è fra le tue opportunità, in «{gia}». " &
                     "Per rileggerlo da capo, incolla il testo nel pannello Candidatura.")
            Return
        End If

        Racconta(Racconto(pagina))

        RaiseEvent AnnuncioCatturato(
            Me, New AnnuncioCatturatoEventArgs(testo, _ricerche.FonteDi(pagina.Indirizzo), pagina.Indirizzo))

    End Function

    ''' <summary>
    ''' Il nome della cartella in cui quell'indirizzo è già stato catturato, o
    ''' <c>Nothing</c> se è la prima volta.
    ''' </summary>
    Private Function GiaCatturato(indirizzo As String) As String

        If _contesto Is Nothing Then Return Nothing

        Try
            Dim dove As String = _contesto.Opportunita.CercaPerLink(indirizzo)
            Return If(dove Is Nothing, Nothing, IO.Path.GetFileName(dove))

        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' Un disco che non risponde non deve impedire una cattura: al massimo si
            ' finisce con un doppione, che è molto meno grave di un annuncio perso.
            Return Nothing

        End Try

    End Function

    ''' <summary>
    ''' Chi può leggere la pagina adesso: quello che il banco ha passato, o la vista se è
    ''' accesa. <c>Nothing</c> quando non c'è niente da leggere.
    ''' </summary>
    Private Function LettoreDellaPagina() As ILettorePagina

        If _lettore IsNot Nothing Then Return _lettore
        If Not _accesa Then Return Nothing

        Return New LettorePagina(vista.CoreWebView2)

    End Function

    ''' <summary>
    ''' Cosa si è preso, detto all'utente. Il titolo della pagina è il modo più corto di
    ''' rispondere alla domanda «ma ha catturato quello giusto?»; il taglio, se c'è stato,
    ''' si dichiara — nel progetto niente si perde in silenzio.
    ''' </summary>
    Private Shared Function Racconto(pagina As PaginaLetta) As String

        Dim titolo As String = If(pagina.Titolo, String.Empty).Trim()

        Return If(titolo = "", "Pagina catturata.", $"Catturato: «{titolo}».") &
               If(pagina.Troncato,
                  $" La pagina era più lunga di {LettorePagina.MassimoCaratteri} caratteri: " &
                  "all'analisi va la prima parte.",
                  String.Empty)

    End Function

    Private Sub btnIndietro_Click(sender As Object, e As EventArgs) Handles btnIndietro.Click

        If _accesa AndAlso vista.CoreWebView2.CanGoBack Then vista.CoreWebView2.GoBack()

    End Sub

    Private Sub btnRicarica_Click(sender As Object, e As EventArgs) Handles btnRicarica.Click

        If _accesa Then vista.CoreWebView2.Reload()

    End Sub

    ''' <summary>
    ''' Invio nelle caselle: nell'indirizzo vale «Vai», nelle parole della ricerca vale
    ''' «Cerca». Chi cerca lavoro scrive e batte Invio, non cerca il bottone.
    ''' </summary>
    Private Async Sub txtIndirizzo_KeyDown(sender As Object, e As KeyEventArgs) Handles txtIndirizzo.KeyDown

        If e.KeyCode <> Keys.Enter Then Return

        e.SuppressKeyPress = True
        Await VaiAllIndirizzoScritto()

    End Sub

    Private Sub CasellaDellaRicerca_KeyDown(sender As Object, e As KeyEventArgs) _
        Handles txtCosa.KeyDown, txtDove.KeyDown

        If e.KeyCode <> Keys.Enter Then Return

        e.SuppressKeyPress = True
        If btnCerca.Enabled Then btnCerca.PerformClick()

    End Sub

    ''' <summary>
    ''' Una pagina che vorrebbe aprirsi in una finestra nuova si apre <b>qui</b>: senza
    ''' questo, sui portali il clic su un risultato non farebbe niente.
    ''' </summary>
    Private Sub AllaFinestraNuova(mittente As Object, argomenti As CoreWebView2NewWindowRequestedEventArgs)

        argomenti.Handled = True
        vista.CoreWebView2.Navigate(argomenti.Uri)

    End Sub

    Private Sub AllaStoriaCambiata(mittente As Object, argomenti As Object)
        AggiornaComandi()
    End Sub

    Private Sub AllIndirizzoCambiato(mittente As Object, argomenti As CoreWebView2SourceChangedEventArgs)
        MostraLIndirizzoCorrente()
    End Sub

    Private Sub AFineNavigazione(mittente As Object, argomenti As CoreWebView2NavigationCompletedEventArgs)

        MostraLIndirizzoCorrente()
        AggiornaComandi()

        If argomenti.IsSuccess Then Return

        Racconta("La pagina non si è caricata. Controlla il collegamento a Internet, " &
                 "o riprova con «⟳».")

    End Sub

    ''' <summary>
    ''' Scrive nella casella dove siamo — ma non mentre l'utente ci sta digitando dentro:
    ''' la stessa casella serve per andare, e sovrascrivere ciò che sta scrivendo sarebbe
    ''' il modo più sicuro di fargliela odiare.
    ''' </summary>
    Private Sub MostraLIndirizzoCorrente()

        If Not _accesa OrElse txtIndirizzo.Focused Then Return

        Dim dove As String = vista.CoreWebView2.Source

        ' La pagina di casa non ha un indirizzo da mostrare: è roba nostra, arrivata da
        ' una stringa, e Chromium la chiama «about:blank».
        txtIndirizzo.Text = If(dove = "about:blank", String.Empty, dove)

    End Sub

    ' ==================================================================
    ' Lo stato del pannello
    ' ==================================================================

    ''' <summary>Accende e spegne i comandi secondo quello che c'è davvero da fare.</summary>
    Private Sub AggiornaComandi()

        Dim cIlBrowser As Boolean = Not _browserFuoriUso

        Dim quantePortali As Boolean = cboPortali.Items.Count > 0
        btnCerca.Enabled = cIlBrowser AndAlso quantePortali
        btnSalvaRicerca.Enabled = quantePortali

        Dim cEUnaSalvata As Boolean = cboSalvate.Items.Count > 0
        btnApri.Enabled = cIlBrowser AndAlso cEUnaSalvata
        btnDimentica.Enabled = cEUnaSalvata
        cboSalvate.Enabled = cEUnaSalvata

        btnVai.Enabled = cIlBrowser
        txtIndirizzo.Enabled = cIlBrowser
        btnRicarica.Enabled = cIlBrowser AndAlso _accesa
        btnIndietro.Enabled = cIlBrowser AndAlso _accesa AndAlso vista.CoreWebView2.CanGoBack

        ' Si cattura da una pagina: finché non ce n'è una aperta il comando resta spento,
        ' che è più onesto di un bottone che risponde «non c'è niente da leggere».
        btnCattura.Enabled = LettoreDellaPagina() IsNot Nothing

    End Sub

    ''' <summary>Il browser non c'è: si dice una volta e i comandi che navigano si spengono.</summary>
    Private Sub FuoriUso(perche As String)

        _browserFuoriUso = True
        Racconta(perche)
        AggiornaComandi()

    End Sub

    ''' <summary>La riga di stato del pannello; <c>Nothing</c> la svuota.</summary>
    Private Sub Racconta(testo As String)
        lblStatoRicerca.Text = If(testo, String.Empty)
    End Sub

    ''' <summary>
    ''' La pagina che si vede aprendo il pannello: scritta da noi, <b>senza rete</b>. Dice
    ''' in tre righe come si usa e ricorda che il login si fa qui dentro — le due cose che
    ''' un utente non può indovinare da solo guardando un browser vuoto.
    ''' </summary>
    Private Function PaginaDiCasa() As String

        Dim portali As String = String.Join(", ", _ricerche.Portali.Select(Function(p) p.Nome))

        Return "<!doctype html><html lang=""it""><head><meta charset=""utf-8"">" &
               "<style>" &
               "body{font-family:'Segoe UI',sans-serif;color:#212529;background:#FFFFFF;" &
               "margin:0;padding:48px 56px;line-height:1.55}" &
               "h1{color:#FA0825;font-size:20px;margin:0 0 6px}" &
               "p.sotto{color:#6C757D;font-size:13px;margin:0 0 28px}" &
               "ol{font-size:14px;padding-left:22px;margin:0 0 28px}" &
               "li{margin-bottom:10px}" &
               "p.nota{color:#6C757D;font-size:12px;border-top:1px solid #DEE2E6;padding-top:14px;margin:0}" &
               "</style></head><body>" &
               "<h1>Cerca gli annunci</h1>" &
               "<p class=""sotto"">Qui dentro navighi tu: il programma non raccoglie niente da solo.</p>" &
               "<ol>" &
               "<li>Scegli un portale, scrivi <b>cosa</b> cerchi e <b>dove</b>, e premi <b>Cerca</b>. " &
               $"I portali pronti sono: {portali}.</li>" &
               "<li>Oppure incolla il <b>link</b> di un annuncio nella casella dell'indirizzo e premi <b>Vai</b>.</li>" &
               "<li>Quando hai davanti la pagina di <b>un</b> annuncio, premi <b>Cattura annuncio</b>: " &
               "il testo va all'analisi e l'annuncio entra fra le tue opportunità.</li>" &
               "</ol>" &
               "<p class=""nota"">Se un portale ti chiede di accedere, fallo qui come faresti in un " &
               "browser qualunque: la sessione resta su questo computer e l'applicazione non vede " &
               "la tua password. Le ricerche che salvi restano anche dopo aver chiuso.</p>" &
               "</body></html>"

    End Function

    ' ==================================================================
    ' Vestizione e geometria
    ' ==================================================================

    Private Sub VestiIBottoni()

        StileApp.VestiBottone(btnCerca, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnSalvaRicerca, LivelloBottone.SicuroPositivo)
        StileApp.VestiBottone(btnCattura, LivelloBottone.SicuroPositivo)
        StileApp.VestiBottone(btnApri, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnVai, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnIndietro, LivelloBottone.Neutro)
        StileApp.VestiBottone(btnRicarica, LivelloBottone.Neutro)

        ' Dimenticare una ricerca butta via qualcosa che l'utente ha fatto: pesa quanto
        ' un'eliminazione, e si chiede prima.
        StileApp.VestiBottone(btnDimentica, LivelloBottone.Distruttivo)

    End Sub

    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        ' Come negli altri pannelli: a cedere il posto al logo è la fascia delle azioni,
        ' dove ci sono bottoni e non dati (v. IPannelloArea).
        pnlAzioni.Height = Math.Max(AltezzaMinimaAzioni, ingombro.Height)
        pnlAzioni.Padding = New Padding(ingombro.Width + StileApp.DistanzaControlli, 0, 0, 0)

        DisponiLeAzioni()

    End Sub

    ''' <summary>Mette la cattura sul fondo della fascia, col suo racconto a destra.</summary>
    Private Sub DisponiLeAzioni()

        Dim riga As Integer = pnlAzioni.Height - StileApp.MargineRiquadro - StileApp.BottoneStandard.Height

        btnCattura.Location = New Point(pnlAzioni.Padding.Left, riga)

        Dim doveComincia As Integer = btnCattura.Right + StileApp.DistanzaControlli
        lblStatoRicerca.Location = New Point(doveComincia, riga + 6)
        lblStatoRicerca.Width = Math.Max(0, pnlAzioni.ClientSize.Width -
                                            StileApp.MargineRiquadro - doveComincia)

    End Sub

    Private Sub PannelloRicerca_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        DisponiLeAzioni()
    End Sub

End Class
