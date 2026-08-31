Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Windows.Forms

''' <summary>
''' La finestra che porta lo <see cref="ScudoDiCaricamento">scudo</see>, la sua ruota e
''' la barra che si riempie in mezzo allo schermo mentre l'AI lavora (cap. 03.8).
''' </summary>
''' <remarks>
''' <para><b>Perché è una finestra a strati (<i>layered</i>) e non un controllo.</b> Lo
''' scudo ha i bordi morbidi e il fondo trasparente: appoggiarlo su una finestra normale
''' vorrebbe dire scegliere un colore da rendere invisibile
''' (<c>TransparencyKey</c>), e attorno al disegno resterebbe l'alone di quel colore —
''' il difetto che il marchio ha già pagato una volta. Una finestra a strati riceve
''' invece l'immagine <b>con il suo canale alfa</b>: il disegno si posa sullo schermo
''' com'è, e sotto si continua a vedere quel che c'era.</para>
''' <para><b>Non ruba niente a nessuno</b>: non prende il fuoco
''' (<c>WS_EX_NOACTIVATE</c>), non compare fra le finestre da scorrere con Alt-Tab
''' (<c>WS_EX_TOOLWINDOW</c>) e <b>non intercetta i clic</b>
''' (<c>WS_EX_TRANSPARENT</c>) — che è la cosa che più conta, perché mentre l'AI lavora
''' il bottone «Annulla» dev'essere premibile e sta proprio lì sotto.</para>
''' <para>È di <see cref="Form.Owner">proprietà</see> della finestra principale, così
''' segue la sua sorte: si minimizza con lei e non resta a galleggiare su un programma
''' chiuso.</para>
''' </remarks>
Friend NotInheritable Class FinestraDiCaricamento

    Inherits Form

    Private Const StileAStrati As Integer = &H80000
    Private Const StileCheLasciaPassareIClic As Integer = &H20
    Private Const StileDiServizio As Integer = &H80
    Private Const StileCheNonSiAttiva As Integer = &H8000000

    ''' <summary>La miscela per alfa: <c>AC_SRC_ALPHA</c> su <c>AC_SRC_OVER</c>.</summary>
    Private Const MiscelaSopra As Byte = 0
    Private Const MiscelaConAlfa As Byte = 1
    Private Const DallaSorgente As Integer = 2

    ''' <summary>Quanti scatti resta piena la barra prima che lo scudo se ne vada.</summary>
    ''' <remarks>
    ''' Tre scatti da ottanta millisecondi: un quarto di secondo scarso, quanto basta
    ''' perché l'occhio veda la barra arrivare in fondo. Di più sarebbe un ritardo fra il
    ''' lavoro finito e la finestra libera, e chi aspetta da un minuto non merita altra
    ''' attesa per un'animazione.
    ''' </remarks>
    Private Const ScattiDelCompimento As Integer = 3

    Private ReadOnly _battito As New Timer() With {
        .Interval = ScudoDiCaricamento.IntervalloInMillisecondi}

    Private _passo As Integer

    ''' <summary>Quando è cominciata l'attesa: è da qui che la barra sa dove arrivare.</summary>
    Private _iniziata As Date

    ''' <summary>Se l'AI ha già risposto e si sta mostrando la barra piena.</summary>
    Private _inCompimento As Boolean

    ''' <summary>Quanti scatti mancano alla sparizione, durante il compimento.</summary>
    Private _scattiRimasti As Integer

    Public Sub New()

        FormBorderStyle = FormBorderStyle.None
        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual
        TopMost = True
        ControlBox = False
        Text = String.Empty

        AddHandler _battito.Tick, AddressOf UnoScatto

    End Sub

    Protected Overrides ReadOnly Property CreateParams As CreateParams
        Get
            Dim parametri As CreateParams = MyBase.CreateParams
            parametri.ExStyle = parametri.ExStyle Or StileAStrati Or
                                StileCheLasciaPassareIClic Or StileDiServizio Or
                                StileCheNonSiAttiva
            Return parametri
        End Get
    End Property

    ''' <summary>Comparire non vuol dire prendersi il fuoco di chi sta lavorando.</summary>
    Protected Overrides ReadOnly Property ShowWithoutActivation As Boolean
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' Mette lo scudo in mezzo allo schermo di quella finestra e lo fa girare.
    ''' </summary>
    ''' <remarks>
    ''' Lo schermo è quello dove sta la finestra principale, non «il primo»: con due
    ''' monitor, il centro dello schermo è il centro di quello che l'utente sta
    ''' guardando. Si prende l'area <b>intera</b> e non quella di lavoro, perché il
    ''' centro dello schermo è il centro dello schermo — la barra delle applicazioni
    ''' sposterebbe lo scudo in su di mezza sua altezza.
    ''' </remarks>
    Public Sub Accendi(finestraPrincipale As Form)

        If finestraPrincipale Is Nothing Then Return

        Dim dove As Rectangle = ScudoDiCaricamento.RiquadroSulloSchermo(
            Screen.FromControl(finestraPrincipale).Bounds)
        If dove.IsEmpty Then Return

        ' Un'attesa già in corso non ricomincia da capo. Non è prudenza teorica: chi
        ' accende lo scudo è la stessa riga che spegne la barra di navigazione, e quella
        ' passa di qui più volte nella stessa attesa. Con la sola ruota non si vedeva —
        ' un pallino vale l'altro — mentre la barra si vedrebbe benissimo tornare a zero
        ' a metà strada, che è il modo più rapido di non essere creduta mai più.
        If Not Visible OrElse _inCompimento Then
            _passo = 0
            _iniziata = Date.Now
        End If

        _inCompimento = False
        SetBounds(dove.Left, dove.Top, dove.Width, dove.Height)

        If Not Visible Then Show(finestraPrincipale)

        Ridisegna()
        _battito.Start()

    End Sub

    ''' <summary>
    ''' L'AI ha risposto: la barra scatta in fondo, si vede piena un istante, poi lo scudo
    ''' se ne va. Non si distrugge niente: la prossima attesa è vicina.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Quell'istante è ciò che rende vera la barra.</b> Per tutta l'attesa il
    ''' riempimento è una stima — onesta, ma una stima — e si ferma al 95%
    ''' (<see cref="ScudoDiCaricamento.RiempimentoMassimo"/>): l'ultimo pezzo lo riempie
    ''' il fatto, cioè la risposta arrivata. Se lo scudo sparisse nello stesso
    ''' millisecondo, quel pezzo non lo vedrebbe nessuno, e agli occhi di chi guarda la
    ''' barra resterebbe una cosa che in fondo non ci arriva mai.</para>
    ''' <para>A contare l'istante è il battito che già c'è, non un secondo timer. La ruota
    ''' intanto continua a girare, ed è voluto: un fermo immagine, anche di un quarto di
    ''' secondo, si legge come un blocco.</para>
    ''' </remarks>
    Public Sub Spegni()

        If Not Visible Then
            _battito.Stop()
            Return
        End If

        If _inCompimento Then Return

        _inCompimento = True
        _scattiRimasti = ScattiDelCompimento

        Ridisegna()
        _battito.Start()

    End Sub

    Private Sub UnoScatto(mittente As Object, e As EventArgs)

        _passo = (_passo + 1) Mod ScudoDiCaricamento.Pallini

        If _inCompimento Then

            _scattiRimasti -= 1

            If _scattiRimasti <= 0 Then
                _battito.Stop()
                _inCompimento = False
                If Visible Then Hide()
                Return
            End If

        End If

        Ridisegna()

    End Sub

    ''' <summary>Quanta barra è piena adesso: la curva del tempo, o tutta se è finita.</summary>
    Private Function QuotaPiena() As Double

        If _inCompimento Then Return 1.0

        Return ScudoDiCaricamento.Riempimento(Date.Now - _iniziata)

    End Function

    Private Sub Ridisegna()

        If Not IsHandleCreated OrElse Width <= 0 OrElse Height <= 0 Then Return

        Using tela As New Bitmap(Width, Height, PixelFormat.Format32bppArgb)

            Using disegno As Graphics = Graphics.FromImage(tela)
                disegno.Clear(Color.Transparent)
                ScudoDiCaricamento.Disegna(disegno, tela.Size, _passo, QuotaPiena())
            End Using

            PosaLaTela(tela)

        End Using

    End Sub

    ''' <summary>
    ''' Consegna a Windows l'immagine con il suo alfa.
    ''' </summary>
    ''' <remarks>
    ''' Gli oggetti grafici di Windows non li raccoglie nessuno: ogni <c>CreateDC</c>,
    ''' ogni <c>GetHbitmap</c> va restituito a mano, e qui si passa di qui dodici volte al
    ''' secondo — una dimenticanza non si vedrebbe subito, si vedrebbe dopo un'ora di
    ''' lavoro come un programma che rallenta. Perciò la restituzione sta in un
    ''' <c>Finally</c>, e la vecchia <c>bitmap</c> torna al suo posto prima di cancellare
    ''' la nostra.
    ''' </remarks>
    Private Sub PosaLaTela(tela As Bitmap)

        Dim schermo As IntPtr = GetDC(IntPtr.Zero)
        Dim memoria As IntPtr = IntPtr.Zero
        Dim nostra As IntPtr = IntPtr.Zero
        Dim vecchia As IntPtr = IntPtr.Zero

        Try
            memoria = CreateCompatibleDC(schermo)
            If memoria = IntPtr.Zero Then Return

            nostra = tela.GetHbitmap(Color.FromArgb(0))
            vecchia = SelectObject(memoria, nostra)

            Dim quiSopra As New PUNTO(Left, Top)
            Dim quanto As New MISURA(tela.Width, tela.Height)
            Dim daCapo As New PUNTO(0, 0)

            Dim miscela As New MISCELA With {
                .Operazione = MiscelaSopra,
                .Bandiere = 0,
                .AlfaDiTutto = 255,
                .FormatoAlfa = MiscelaConAlfa}

            UpdateLayeredWindow(Handle, schermo, quiSopra, quanto,
                                memoria, daCapo, 0, miscela, DallaSorgente)

        Finally
            If nostra <> IntPtr.Zero Then
                SelectObject(memoria, vecchia)
                DeleteObject(nostra)
            End If
            If memoria <> IntPtr.Zero Then DeleteDC(memoria)
            ReleaseDC(IntPtr.Zero, schermo)
        End Try

    End Sub

    Protected Overrides Sub Dispose(disposing As Boolean)

        If disposing Then
            RemoveHandler _battito.Tick, AddressOf UnoScatto
            _battito.Stop()
            _battito.Dispose()
        End If

        MyBase.Dispose(disposing)

    End Sub

    ' ==================================================================
    ' Quel che serve a Windows
    ' ==================================================================

    <StructLayout(LayoutKind.Sequential)>
    Private Structure PUNTO
        Public X As Integer
        Public Y As Integer
        Public Sub New(x As Integer, y As Integer)
            Me.X = x
            Me.Y = y
        End Sub
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure MISURA
        Public Larghezza As Integer
        Public Altezza As Integer
        Public Sub New(larghezza As Integer, altezza As Integer)
            Me.Larghezza = larghezza
            Me.Altezza = altezza
        End Sub
    End Structure

    <StructLayout(LayoutKind.Sequential, Pack:=1)>
    Private Structure MISCELA
        Public Operazione As Byte
        Public Bandiere As Byte
        Public AlfaDiTutto As Byte
        Public FormatoAlfa As Byte
    End Structure

    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function UpdateLayeredWindow(
        finestra As IntPtr, schermo As IntPtr, ByRef dove As PUNTO, ByRef quanto As MISURA,
        sorgente As IntPtr, ByRef daDove As PUNTO, colore As Integer,
        ByRef miscela As MISCELA, bandiere As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Private Shared Function GetDC(finestra As IntPtr) As IntPtr
    End Function

    <DllImport("user32.dll")>
    Private Shared Function ReleaseDC(finestra As IntPtr, contesto As IntPtr) As Integer
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function CreateCompatibleDC(contesto As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function DeleteDC(contesto As IntPtr) As Boolean
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function SelectObject(contesto As IntPtr, oggetto As IntPtr) As IntPtr
    End Function

    <DllImport("gdi32.dll")>
    Private Shared Function DeleteObject(oggetto As IntPtr) As Boolean
    End Function

End Class
