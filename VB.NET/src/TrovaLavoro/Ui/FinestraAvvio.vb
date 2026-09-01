Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' La schermata di avvio (cap. 03.4): il marchio a video mentre l'applicazione si
''' monta — cartella dati, lucchetto, libreria dei prompt, browser incorporato.
''' </summary>
''' <remarks>
''' <para><b>Perché ha un tempo minimo.</b> Misurata sulla macchina di sviluppo,
''' l'applicazione passa dal doppio clic alla finestra in <b>265-330 ms</b>: una
''' schermata legata al solo caricamento lampeggerebbe senza che nessuno la legga, che
''' è il difetto tipico degli splash fatti a naso. Resta perciò a video un minimo
''' garantito — <b>dieci secondi</b> dal 2026-09-01, su indicazione del tutor; erano
''' cinque, il tempo di leggere il marchio e il sottotitolo — e il caricamento le corre
''' sotto.</para>
''' <para><b>Ma il minimo vale per chi guarda, non per chi aspetta una risposta.</b>
''' Al primo avvio la chiave API si chiede prima di ogni altra cosa (cap. 11.3), e una
''' schermata <c>TopMost</c> davanti a quella domanda sarebbe un programma che sembra
''' bloccato. Chi sta per aprire una finestra che chiede qualcosa chiama
''' <see cref="ChiudiSubito"/>, e il minimo decade.</para>
''' <para><b>Non compare mai in modalità server</b> (<c>--mcp</c>, cap. 09): la
''' biforcazione di <c>Programma.Main</c> sta prima di ogni preparativo grafico, e
''' questa finestra nasce dopo. Un server che apre una finestra sulla macchina di chi lo
''' ha avviato sarebbe un difetto grave, non un vezzo.</para>
''' <para>Un clic la manda via, e dal 2026-09-01 anche <b>Invio</b>: chi conosce già il
''' programma non deve aspettare, e dieci secondi sono lunghi da aspettare col mouse in
''' mano. Perché Invio abbia bisogno di un filtro dei messaggi e non basti un
''' <c>KeyDown</c>, v. <see cref="PreFilterMessage"/>.</para>
''' </remarks>
Public Class FinestraAvvio
    Implements ISchermataDiAvvio
    Implements IMessageFilter

    ''' <summary>Quanto resta a video come minimo, se nessuno la manda via prima.</summary>
    ''' <remarks>
    ''' Dieci secondi dal 2026-09-01, su indicazione del tutor: erano cinque. È il minimo
    ''' <b>garantito</b>, non un'attesa imposta — un clic o un Invio la chiudono in
    ''' qualunque momento.
    ''' </remarks>
    Public Shared ReadOnly MinimoAVideo As TimeSpan = TimeSpan.FromSeconds(10)

    ''' <summary>Il messaggio di Windows «un tasto è stato premuto».</summary>
    Private Const WmKeyDown As Integer = &H100

    ''' <summary>
    ''' Quanta parte dello schermo può occupare al massimo. L'immagine è disegnata per
    ''' 800x702, che su un portatile piccolo — o su uno schermo al 150% — sarebbe metà
    ''' desktop: sotto questa quota si rimpicciolisce mantenendo le proporzioni.
    ''' </summary>
    Public Const QuotaSchermo As Double = 0.7

    ''' <summary>Da quando è a video. Un cronometro, non l'orologio: l'ora di sistema può cambiare.</summary>
    Private ReadOnly _daQuando As Stopwatch = Stopwatch.StartNew()

    Private ReadOnly _minimo As TimeSpan
    Private _attesa As Timer
    Private _chiusa As Boolean

    ''' <summary>Se se n'è già andata. Chiuderla di nuovo non fa niente.</summary>
    Public ReadOnly Property GiaChiusa As Boolean
        Get
            Return _chiusa
        End Get
    End Property

    ''' <summary>
    ''' Prepara la schermata attorno all'immagine data. È pubblica perché il banco la
    ''' costruisce e la interroga senza mostrarla.
    ''' </summary>
    ''' <param name="minimo">
    ''' Quanto restare a video come minimo. Serve al banco, che così prova le due strade
    ''' — «il minimo è passato» e «non ancora» — senza aspettare davvero: un collaudo che
    ''' misura un tempo vero è un collaudo che prima o poi cade da solo, e questa tappa
    ''' ne ha appena curato uno (regola 14).
    ''' </param>
    Public Sub New(immagine As Image, Optional minimo As TimeSpan? = Nothing)

        InitializeComponent()

        _minimo = If(minimo, MinimoAVideo)

        picSchermata.Image = immagine
        If immagine IsNot Nothing Then Me.ClientSize = MisuraDaMostrare(immagine.Size, SpazioDisponibile())

    End Sub

    ''' <summary>
    ''' Apre la schermata di avvio, o torna <c>Nothing</c> se il marchio non si è
    ''' caricato: un'immagine che manca non è una ragione per non partire.
    ''' </summary>
    Public Shared Function Mostra() As FinestraAvvio

        Dim immagine As Image = Marchio.SchermataDiAvvio
        If immagine Is Nothing Then Return Nothing

        Dim schermata As New FinestraAvvio(immagine)
        schermata.Show()

        ' Il ciclo dei messaggi non è ancora partito — Application.Run viene dopo — e
        ' senza questo la finestra resterebbe un rettangolo vuoto proprio nei millisecondi
        ' che deve coprire.
        schermata.Refresh()

        Return schermata

    End Function

    ''' <summary>
    ''' Quanto manca al minimo garantito. Mai negativo: chi ha già aspettato abbastanza
    ''' non aspetta «meno di zero», chiude e basta.
    ''' </summary>
    Public Shared Function AttesaRimasta(trascorso As TimeSpan, minimo As TimeSpan) As TimeSpan
        Dim rimasta As TimeSpan = minimo - trascorso
        Return If(rimasta > TimeSpan.Zero, rimasta, TimeSpan.Zero)
    End Function

    ''' <summary>
    ''' La misura con cui mostrarsi: quella dell'immagine, ridotta in proporzione se non
    ''' sta nella quota di schermo concessa.
    ''' </summary>
    Public Shared Function MisuraDaMostrare(originale As Size, spazio As Size) As Size

        If originale.Width <= 0 OrElse originale.Height <= 0 Then Return originale
        If spazio.Width <= 0 OrElse spazio.Height <= 0 Then Return originale

        Dim fattore As Double = Math.Min(spazio.Width * QuotaSchermo / originale.Width,
                                         spazio.Height * QuotaSchermo / originale.Height)
        If fattore >= 1.0 Then Return originale

        Return New Size(Math.Max(1, CInt(Math.Round(originale.Width * fattore))),
                        Math.Max(1, CInt(Math.Round(originale.Height * fattore))))

    End Function

    ''' <summary>
    ''' Appena è a video si mette in ascolto della tastiera di tutta l'applicazione.
    ''' </summary>
    Protected Overrides Sub OnShown(e As EventArgs)

        MyBase.OnShown(e)
        Application.AddMessageFilter(Me)

    End Sub

    ''' <summary>E smette appena se n'è andata, comunque se ne sia andata.</summary>
    ''' <remarks>
    ''' Qui e non dentro <see cref="ChiudiSubito"/>: un filtro dimenticato resta appeso al
    ''' ciclo dei messaggi per tutta la vita del programma, e questo si mangerebbe ogni
    ''' Invio dell'applicazione. Toglierlo dove la finestra è <b>chiusa davvero</b> copre
    ''' anche le strade che non passano di lì. Toglierne uno mai aggiunto non fa niente,
    ''' ed è il caso del banco, che la costruisce senza mostrarla.
    ''' </remarks>
    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)

        Application.RemoveMessageFilter(Me)
        MyBase.OnFormClosed(e)

    End Sub

    ''' <summary>Invio la manda via, da qualunque finestra lo si prema.</summary>
    ''' <remarks>
    ''' <para><b>Perché un filtro dei messaggi e non un <c>KeyDown</c> di questa
    ''' finestra.</b> La schermata è <c>TopMost</c> e resta davanti, ma il <b>fuoco della
    ''' tastiera</b> non ce l'ha quasi mai: <c>Programma.Main</c> la mostra e subito dopo
    ''' apre la finestra principale, che si attiva e si prende il fuoco mentre lo splash è
    ''' ancora lì. Un <c>KeyDown</c> — o <c>KeyPreview</c>, o <c>ProcessCmdKey</c> — su
    ''' questa finestra non scatterebbe mai, e il tasto sembrerebbe non funzionare a caso.
    ''' Il filtro vede invece i tasti di <b>tutto</b> il ciclo dei messaggi, cioè anche
    ''' quelli diretti alla finestra principale.</para>
    ''' <para>Il tasto si <b>consuma</b> (<c>True</c>): l'Invio che manda via la schermata
    ''' non deve arrivare anche al bottone che ha il fuoco dietro di lei.</para>
    ''' </remarks>
    Public Function PreFilterMessage(ByRef messaggio As Message) As Boolean _
        Implements IMessageFilter.PreFilterMessage

        If _chiusa OrElse messaggio.Msg <> WmKeyDown Then Return False
        If messaggio.WParam.ToInt32() <> CInt(Keys.Enter) Then Return False

        ChiudiSubito()

        Return True

    End Function

    ''' <inheritdoc/>
    Public Sub ChiudiSubito() Implements ISchermataDiAvvio.ChiudiSubito

        If _chiusa Then Return
        _chiusa = True

        _attesa?.Stop()
        _attesa?.Dispose()
        _attesa = Nothing

        Me.Close()

    End Sub

    ''' <inheritdoc/>
    Public Sub ChiudiQuandoPuoi() Implements ISchermataDiAvvio.ChiudiQuandoPuoi

        If _chiusa Then Return

        Dim rimasta As TimeSpan = AttesaRimasta(_daQuando.Elapsed, _minimo)
        If rimasta = TimeSpan.Zero Then
            ChiudiSubito()
            Return
        End If

        ' Già in attesa: la seconda richiesta non riavvia il conto, altrimenti chi
        ' chiede due volte la farebbe restare a video il doppio.
        If _attesa IsNot Nothing Then Return

        _attesa = New Timer() With {.Interval = Math.Max(1, CInt(Math.Ceiling(rimasta.TotalMilliseconds)))}
        AddHandler _attesa.Tick, AddressOf QuandoScadeIlMinimo
        _attesa.Start()

    End Sub

    Private Sub QuandoScadeIlMinimo(mittente As Object, e As EventArgs)
        ChiudiSubito()
    End Sub

    ''' <summary>Un clic in qualunque punto la manda via.</summary>
    Private Sub Cliccata(mittente As Object, e As EventArgs) Handles Me.Click, picSchermata.Click
        ChiudiSubito()
    End Sub

    ''' <summary>Lo schermo su cui si apre, o una misura nulla se non c'è (banco).</summary>
    Private Shared Function SpazioDisponibile() As Size
        Dim schermo As Screen = Screen.PrimaryScreen
        Return If(schermo Is Nothing, Size.Empty, schermo.WorkingArea.Size)
    End Function

End Class
