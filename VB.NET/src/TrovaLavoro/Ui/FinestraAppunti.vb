Imports System.Linq
Imports System.Windows.Forms
Imports TrovaLavoro.Motore

''' <summary>Cosa l'utente ha deciso davanti agli appunti proposti.</summary>
Public Enum EsitoAppunti

    ''' <summary>Ha chiuso senza confermare: non si salva niente.</summary>
    Annullato

    ''' <summary>Va bene così: gli appunti spuntati si salvano con la candidatura.</summary>
    Confermato

End Enum

''' <summary>
''' La <b>scheda di conferma</b> del cap. 12, A6.3: quello che l'AI ha distillato dal
''' ragionamento, prima che una sola riga arrivi ai prompt che scrivono 🎯 CV e ✉️ lettera.
''' </summary>
''' <remarks>
''' <para>Somiglia di proposito alla finestra dei documenti (T6): elenco con le spunte,
''' e la riga scelta si corregge nella casella sotto. Tutta l'applicazione mostra gli
''' elenchi così, e una griglia editabile sarebbe l'unico controllo del suo genere.</para>
''' <para><b>I fatti nuovi stanno a parte e non si spuntano.</b> Non sono appunti: sono
''' cose che l'utente ha detto parlando e che nel suo profilo non risultano. Nei documenti
''' non possono entrare — i fatti vengono dal profilo, sempre — ma non devono nemmeno
''' sparire in silenzio: restano qui, dichiarati, perché li porti nel profilo se sono
''' veri. È lo stesso mestiere del campo <c>altrove</c> nei turni del dialogo.</para>
''' <para><b>Qui non si chiama l'AI</b> e non si scrive su disco: la finestra raccoglie
''' una decisione e la restituisce. A salvare è il pannello.</para>
''' </remarks>
Public Class FinestraAppunti

    Private ReadOnly _appunti As AppuntiDiMira

    ''' <summary>Quante volte si sta riempiendo l'elenco: gli eventi di riflesso non contano.</summary>
    Private _riempimenti As Integer

    ''' <summary>Cosa ha deciso l'utente; <see cref="EsitoAppunti.Annullato"/> finché non decide.</summary>
    Public ReadOnly Property Esito As EsitoAppunti = EsitoAppunti.Annullato

    ''' <summary>
    ''' Prepara la finestra sugli appunti proposti. È pubblica perché il banco la
    ''' costruisce e la interroga senza mostrarla.
    ''' </summary>
    ''' <param name="appunti">
    ''' Gli appunti come li ha distillati l'AI. Le correzioni si scrivono <b>qui dentro</b>;
    ''' se l'esito è <see cref="EsitoAppunti.Annullato"/> non ne resta traccia, perché
    ''' questo oggetto muore con la conversazione.
    ''' </param>
    Public Sub New(appunti As AppuntiDiMira)

        InitializeComponent()

        If appunti Is Nothing Then Throw New ArgumentNullException(NameOf(appunti))
        _appunti = appunti

        lblSpiegazione.Text =
            "Ecco cosa ho ricavato dal nostro ragionamento. Spunta gli appunti da tenere: " &
            "guideranno la scrittura del 🎯 CV mirato e della ✉️ lettera." & vbLf &
            "Un appunto dice cosa mettere in risalto e con che tono — i fatti restano quelli " &
            "del tuo profilo, e da qui non se ne aggiungono."

        lblFattiTitolo.Text =
            "Cose che hai detto e che nel profilo non ci sono. Non entrano in questa candidatura: " &
            "se sono vere, aggiungile al profilo e saranno tue per tutte le prossime."

        Vesti()
        MostraGliAppunti()
        MostraIFattiNuovi()

        ' Esc annulla; nessun bottone appeso a Invio, perché si scrive nella casella
        ' dell'appunto e un Invio di passaggio chiuderebbe la finestra a metà lavoro.
        CancelButton = btnAnnulla
        AcceptButton = Nothing

    End Sub

    ''' <summary>
    ''' Mostra la finestra e dice cosa l'utente ha deciso.
    ''' </summary>
    ''' <returns>
    ''' Gli appunti da tenere — solo quelli spuntati, con le correzioni — oppure
    ''' <c>Nothing</c> se ha annullato.
    ''' </returns>
    Public Shared Function Chiedi(proprietario As IWin32Window, appunti As AppuntiDiMira) As AppuntiDiMira

        Using finestra As New FinestraAppunti(appunti)

            finestra.ShowDialog(proprietario)
            If finestra.Esito <> EsitoAppunti.Confermato Then Return Nothing

            Return finestra.Scelti()

        End Using

    End Function

    ''' <summary>I colori e i font, tutti da <see cref="StileApp"/> (cap. 03.2).</summary>
    Private Sub Vesti()

        BackColor = StileApp.SfondoContenuto
        Font = StileApp.FontTesto

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli

        lblSpiegazione.ForeColor = StileApp.TestoPrimario
        lblModifica.ForeColor = StileApp.TestoPrimario
        lblFattiTitolo.ForeColor = StileApp.TestoSecondario

        lvwAppunti.BackColor = StileApp.SfondoContenuto
        lvwFatti.BackColor = StileApp.SfondoBase
        txtAppunto.BackColor = StileApp.SfondoContenuto

        StileApp.VestiBottone(btnConferma, LivelloBottone.SicuroPositivo)
        StileApp.VestiBottone(btnAnnulla, LivelloBottone.Neutro)

    End Sub

    ''' <summary>
    ''' Riempie l'elenco degli appunti, tutti spuntati.
    ''' </summary>
    ''' <remarks>
    ''' Spuntati di partenza perché sono la proposta dell'AI su una conversazione che
    ''' l'utente ha appena fatto: togliere è più veloce che rimettere, e chi è d'accordo
    ''' conferma e basta.
    ''' </remarks>
    Private Sub MostraGliAppunti()

        _riempimenti += 1

        Try
            lvwAppunti.Items.Clear()

            For Each appunto As AppuntoDiMira In _appunti.Appunti
                Dim riga As New ListViewItem({
                    TipiDiAppunto.Etichetta(appunto.Tipo), appunto.Testo, appunto.Da})
                riga.Checked = True
                lvwAppunti.Items.Add(riga)
            Next

        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ''' <summary>Mette in mostra i fatti nuovi; se non ce ne sono, la zona sparisce.</summary>
    Private Sub MostraIFattiNuovi()

        lvwFatti.Items.Clear()

        For Each fatto As String In _appunti.FattiNuovi
            lvwFatti.Items.Add(New ListViewItem(fatto))
        Next

        Dim cene As Boolean = _appunti.FattiNuovi.Count > 0
        lblFattiTitolo.Visible = cene
        lvwFatti.Visible = cene

    End Sub

    ''' <summary>L'appunto scelto nell'elenco, o <c>Nothing</c> se non ce n'è uno.</summary>
    Public ReadOnly Property AppuntoScelto As AppuntoDiMira
        Get
            Dim indice As Integer = IndiceScelto
            If indice < 0 OrElse indice >= _appunti.Appunti.Count Then Return Nothing

            Return _appunti.Appunti(indice)
        End Get
    End Property

    ''' <summary>La riga scelta, o -1.</summary>
    Private ReadOnly Property IndiceScelto As Integer
        Get
            If lvwAppunti.SelectedIndices.Count = 0 Then Return -1
            Return lvwAppunti.SelectedIndices(0)
        End Get
    End Property

    ''' <summary>
    ''' Riscrive il testo di un appunto.
    ''' </summary>
    ''' <remarks>
    ''' Sta in un metodo pubblico, staccato dalla casella, perché è l'unico modo di
    ''' collaudarlo: su una finestra mai mostrata i controlli non si lasciano pilotare
    ''' (v. <see cref="FinestraDocumenti.CambiaLaCategoria"/>).
    ''' </remarks>
    ''' <returns>Falso se quell'indice non esiste o il testo è vuoto.</returns>
    Public Function RiscriviLAppunto(indice As Integer, testo As String) As Boolean

        If indice < 0 OrElse indice >= _appunti.Appunti.Count Then Return False
        If String.IsNullOrWhiteSpace(testo) Then Return False

        Dim appunto As AppuntoDiMira = _appunti.Appunti(indice)
        appunto.Testo = testo.Trim()

        ' Un appunto riscritto non nasce più da quello che aveva detto l'AI: la colonna
        ' «da dove nasce» diventerebbe una citazione sbagliata, ed è meglio che dica il
        ' vero — che a scriverlo è stato l'utente.
        appunto.Da = "L'hai scritto tu."

        _riempimenti += 1

        Try
            If indice < lvwAppunti.Items.Count Then
                lvwAppunti.Items(indice).SubItems(1).Text = appunto.Testo
                lvwAppunti.Items(indice).SubItems(2).Text = appunto.Da
            End If
        Finally
            _riempimenti -= 1
        End Try

        Return True

    End Function

    ''' <summary>Spunta o toglie la spunta a un appunto. Anche questo per il banco.</summary>
    ''' <returns>Falso se quell'indice non esiste.</returns>
    Public Function Tieni(indice As Integer, tenere As Boolean) As Boolean

        If indice < 0 OrElse indice >= lvwAppunti.Items.Count Then Return False

        lvwAppunti.Items(indice).Checked = tenere
        Return True

    End Function

    ''' <summary>
    ''' Gli appunti da tenere: solo quelli spuntati, con le correzioni fatte. I fatti
    ''' nuovi li accompagnano sempre — non sono una scelta dell'utente, sono un promemoria.
    ''' </summary>
    Public Function Scelti() As AppuntiDiMira

        Dim tenuti As New List(Of AppuntoDiMira)

        For i As Integer = 0 To Math.Min(lvwAppunti.Items.Count, _appunti.Appunti.Count) - 1
            If lvwAppunti.Items(i).Checked Then tenuti.Add(_appunti.Appunti(i))
        Next

        Return _appunti.Solo(tenuti)

    End Function

    Private Sub lvwAppunti_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lvwAppunti.SelectedIndexChanged

        Dim appunto As AppuntoDiMira = AppuntoScelto

        _riempimenti += 1

        Try
            txtAppunto.Enabled = appunto IsNot Nothing
            txtAppunto.Text = If(appunto?.Testo, String.Empty)
        Finally
            _riempimenti -= 1
        End Try

    End Sub

    Private Sub txtAppunto_TextChanged(sender As Object, e As EventArgs) Handles txtAppunto.TextChanged

        If _riempimenti > 0 Then Return

        RiscriviLAppunto(IndiceScelto, txtAppunto.Text)

    End Sub

    Private Sub btnConferma_Click(sender As Object, e As EventArgs) Handles btnConferma.Click
        Chiudi(EsitoAppunti.Confermato, DialogResult.OK)
    End Sub

    Private Sub btnAnnulla_Click(sender As Object, e As EventArgs) Handles btnAnnulla.Click
        Chiudi(EsitoAppunti.Annullato, DialogResult.Cancel)
    End Sub

    Private Sub Chiudi(esito As EsitoAppunti, risultato As DialogResult)

        _Esito = esito
        DialogResult = risultato
        Close()

    End Sub

End Class
