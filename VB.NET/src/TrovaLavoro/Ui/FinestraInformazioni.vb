Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' «Informazioni su…» (cap. 03.4): chi è questo programma, che versione è, con quale
''' libreria di prompt sta lavorando. Si apre dalle <see cref="FinestraImpostazioni">
''' Impostazioni</see>, accanto a «Come funziona…»: sono le due voci che parlano <i>del
''' programma</i> invece che delle scelte che lo governano.
''' </summary>
''' <remarks>
''' <para>Fino al 2026-09-01 la porta era il <b>pannello del logo</b> in basso a sinistra
''' (cap. 03.5), che è il posto dove versione e pool si vanno già a cercare. Su indicazione
''' del tutor quel clic è stato tolto — lo stemma è un'insegna — e la finestra è passata
''' alle Impostazioni, perché «Cerca aggiornamenti» e «Copia diagnostica» vivono qui dentro
''' e senza una porta si sarebbero perse in silenzio.</para>
''' </remarks>
''' <remarks>
''' <para><b>Non ripete le Impostazioni.</b> La cartella dati, i modelli, le preferenze
''' e le pulizie stanno in P8, e una seconda vetrina degli stessi valori sarebbe la
''' solita copia destinata a divergere. Qui c'è il marchio, la riga di versione —
''' composta da <see cref="Versione.Riga"/>, la stessa del pannello logo — e il
''' copyright.</para>
''' <para>La riga del pool arriva da fuori e non si legge da sola: chi apre la finestra
''' ha già il contesto montato, e la libreria sa dire da sé se è quella incorporata,
''' quella esterna o una con file ritoccati (cap. 04.5).</para>
''' </remarks>
Public Class FinestraInformazioni

    ''' <summary>
    ''' Prepara la finestra. È pubblica perché il banco la costruisce e la interroga
    ''' senza mostrarla: di una finestra modale non si può aspettare la chiusura.
    ''' </summary>
    ''' <param name="etichettaPool">
    ''' L'etichetta della libreria dei prompt in vigore, o <c>Nothing</c> se non si è
    ''' aperta: allora vale il «Pool —» dell'anomalia totale, come nel pannello logo.
    ''' </param>
    ''' <param name="diagnostica">
    ''' Che cosa mettere negli appunti quando si chiede «Copia diagnostica», o
    ''' <c>Nothing</c> se in questo momento non c'è niente da copiare — e allora il
    ''' bottone non compare affatto, invece di comparire e non fare niente.
    ''' </param>
    ''' <param name="controllo">
    ''' Chi va a chiedere qual è l'ultima versione pubblicata; se omesso la chiede
    ''' davvero, a GitHub. Il banco ce ne mette uno finto. Non può chiamarsi «versione»:
    ''' coprirebbe il modulo <see cref="Versione"/>, che due righe più giù serve.
    ''' </param>
    Public Sub New(etichettaPool As String, Optional diagnostica As Func(Of String) = Nothing,
                   Optional controllo As Func(Of Task(Of Motore.EsitoVersione)) = Nothing)

        _diagnostica = diagnostica
        _versione = If(controllo, Function() Motore.ControlloVersione.ChiediAsync())

        InitializeComponent()

        Me.BackColor = StileApp.SfondoContenuto

        picMarchio.Image = Marchio.SchermataDiAvvio
        _marchioInMostra = picMarchio.Image IsNot Nothing

        lblVersione.Font = StileApp.FontTesto
        lblVersione.ForeColor = StileApp.TestoPrimario
        lblVersione.Text = Versione.Riga(If(String.IsNullOrWhiteSpace(etichettaPool), "Pool —", etichettaPool))

        lblCopyright.Font = StileApp.FontDidascalia
        lblCopyright.ForeColor = StileApp.TestoSecondario
        lblCopyright.Text = "© 2026 Aviolab AI — Tutti i diritti riservati"

        lblSorgente.Font = StileApp.FontDidascalia
        lblSorgente.ForeColor = StileApp.TestoSecondario
        lblSorgente.Text = Versione.RigaDelSorgente()

        StileApp.VestiBottone(btnChiudi, LivelloBottone.Neutro)
        StileApp.VestiBottone(btnCopiaDiagnostica, LivelloBottone.Neutro)
        StileApp.VestiBottone(btnControllaVersione, LivelloBottone.Neutro)
        StileApp.VestiBottone(btnComeFunziona, LivelloBottone.Esplorativo)

        lblEsitoVersione.Font = StileApp.FontDidascalia
        lblEsitoVersione.ForeColor = StileApp.TestoSecondario
        btnCopiaDiagnostica.Visible = _diagnostica IsNot Nothing

        ' Se il marchio non si è caricato, la finestra non mostra un rettangolo blu
        ' vuoto: l'immagine sparisce e tutto il resto sale al suo posto.
        If Not _marchioInMostra Then TogliLoSpazioDellImmagine()

        Me.AcceptButton = btnChiudi
        Me.CancelButton = btnChiudi

    End Sub

    ''' <summary>
    ''' La riga di versione così com'è mostrata. È pubblica per il banco: di una
    ''' finestra modale non si può aspettare la chiusura, e quel che vale la pena
    ''' controllare va letto da fuori senza mostrarla (cap. 03.4).
    ''' </summary>
    Public ReadOnly Property RigaDiVersione As String
        Get
            Return lblVersione.Text
        End Get
    End Property

    ''' <summary>La riga del copyright così com'è mostrata.</summary>
    Public ReadOnly Property RigaDiCopyright As String
        Get
            Return lblCopyright.Text
        End Get
    End Property

    ''' <summary>
    ''' La riga che dice da quale commit nasce questo eseguibile, così com'è mostrata
    ''' (cap. 13.9). Sta qui e non nelle Impostazioni perché è la domanda che ci si fa
    ''' con un exe in mano — «questo che ho davanti è davvero quello che credo?» — e
    ''' questa è la finestra che risponde a chi è quel programma.
    ''' </summary>
    Public ReadOnly Property RigaDelCodiceSorgente As String
        Get
            Return lblSorgente.Text
        End Get
    End Property

    ''' <summary>
    ''' L'immagine del marchio in mostra, o <c>Nothing</c> se non si è caricata — e
    ''' allora la finestra si è già accorciata dello spazio che avrebbe occupato.
    ''' </summary>
    ''' <remarks>
    ''' Lo stato sta in un campo e non si rilegge da <c>picMarchio.Visible</c>, per la
    ''' ragione già scritta in <see cref="FinestraChiaveApi"/>:
    ''' <see cref="Control.Visible"/> di un figlio dice se il controllo <i>si vede
    ''' davvero</i>, e su una finestra mai mostrata è falso anche subito dopo averlo
    ''' acceso. Qui serve quel che la finestra ha deciso di mostrare, non quel che in
    ''' questo istante è a video — e il banco la interroga senza mostrarla.
    ''' </remarks>
    Public ReadOnly Property ImmagineDelMarchio As Image
        Get
            Return If(_marchioInMostra, picMarchio.Image, Nothing)
        End Get
    End Property

    ''' <summary>Se il marchio è nel disegno della finestra (v. la proprietà qui sopra).</summary>
    Private ReadOnly _marchioInMostra As Boolean

    ''' <summary>
    ''' Chiude il vuoto lasciato dall'immagine mancante, tirando su le righe e
    ''' accorciando la finestra della stessa misura.
    ''' </summary>
    Private Sub TogliLoSpazioDellImmagine()

        Dim salto As Integer = picMarchio.Height + StileApp.DistanzaControlli
        picMarchio.Visible = False

        For Each figlio As Control In New Control() _
            {lblVersione, lblCopyright, lblSorgente, lblEsitoVersione,
             btnControllaVersione, btnCopiaDiagnostica, btnComeFunziona, btnChiudi}
            figlio.Top -= salto
        Next

        Me.ClientSize = New Size(Me.ClientSize.Width, Me.ClientSize.Height - salto)

    End Sub

    ' ==================================================================
    ' «Cerca aggiornamenti» (2026-08-27, dalla revisione del giro D)
    ' ==================================================================

    ''' <summary>Chi va a chiedere qual è l'ultima versione pubblicata.</summary>
    ''' <remarks>
    ''' È una porta e non una chiamata diretta perché il banco ci mette un finto: senza,
    ''' collaudare questo bottone vorrebbe dire uscire su Internet a ogni <c>dotnet test</c>.
    ''' </remarks>
    Private ReadOnly _versione As Func(Of Task(Of Motore.EsitoVersione))

    ''' <summary>L'ultimo esito mostrato, o stringa vuota. È per il banco.</summary>
    Public ReadOnly Property EsitoDellaVersione As String
        Get
            Return lblEsitoVersione.Text
        End Get
    End Property

    ''' <summary>
    ''' Chiede se c'è una versione nuova e lo dice.
    ''' </summary>
    ''' <remarks>
    ''' <para>Parte solo di qui, cioè da un bottone premuto: mai all'avvio, mai da sola.
    ''' Il cap. 11.2 promette «niente aggiornamenti automatici silenziosi», e una domanda
    ''' che l'utente pone è il contrario del silenzio.</para>
    ''' <para>Sta in un metodo suo, staccato dal bottone, per la ragione di sempre: di una
    ''' finestra modale il banco non può aspettare la chiusura.</para>
    ''' </remarks>
    Public Async Function ControllaLaVersione() As Task

        btnControllaVersione.Enabled = False
        Mostra("Sto guardando…")

        Try
            Dim esito As Motore.EsitoVersione = Await _versione().ConfigureAwait(True)

            If esito Is Nothing Then
                Mostra("Non sono riuscita a chiederlo.")
            Else
                Mostra(esito.Messaggio)
                _cENeUnaNuova = esito.Stato = Motore.StatoVersione.CeNEUnaNuova
            End If

        Catch ex As Exception
            ' Una versione che non si sa non è un guasto del programma: è un esito.
            Mostra("Non sono riuscita a chiederlo: riprova più tardi.")
            Dati.DiarioTecnico.Corrente?.AnnotaGuasto("il controllo della versione", ex)
        Finally
            If Not IsDisposed Then btnControllaVersione.Enabled = True
        End Try

    End Function

    ''' <summary>Se l'ultimo controllo ha trovato una versione più recente.</summary>
    Public ReadOnly Property CENeUnaNuova As Boolean
        Get
            Return _cENeUnaNuova
        End Get
    End Property

    Private _cENeUnaNuova As Boolean

    Private Sub Mostra(riga As String)
        lblEsitoVersione.Text = riga
    End Sub

    Private Async Sub ControllaVersione(mittente As Object, e As EventArgs) _
        Handles btnControllaVersione.Click

        Await ControllaLaVersione()

        ' Il posto da cui si scarica si apre solo se c'è qualcosa da scaricare: aprirlo
        ' sempre manderebbe l'utente su una pagina che gli ripete quello che ha già.
        If Not _cENeUnaNuova Then Return

        Try
            Process.Start(New ProcessStartInfo(Motore.ControlloVersione.PaginaDelleRelease) _
                          With {.UseShellExecute = True})
        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse
                                   TypeOf ex Is System.ComponentModel.Win32Exception
            Mostra(lblEsitoVersione.Text & vbLf & Motore.ControlloVersione.PaginaDelleRelease)
        End Try

    End Sub

    Private Sub ApriLInformativa(mittente As Object, e As EventArgs) Handles btnComeFunziona.Click
        FinestraInformativa.Mostra(Me)
    End Sub

    Private Sub Chiudi(mittente As Object, e As EventArgs) Handles btnChiudi.Click
        Me.Close()
    End Sub

    ''' <summary>Che cosa copiare quando si chiede la diagnostica, o <c>Nothing</c>.</summary>
    Private ReadOnly _diagnostica As Func(Of String)

    ''' <summary>
    ''' Se in questa finestra c'è qualcosa da copiare. È per il banco: il bottone
    ''' <c>Visible</c> di una finestra mai mostrata è sempre falso, per la ragione già
    ''' scritta più su a proposito del marchio.
    ''' </summary>
    Public ReadOnly Property PuoCopiareLaDiagnostica As Boolean
        Get
            Return _diagnostica IsNot Nothing
        End Get
    End Property

    ''' <summary>
    ''' Mette la diagnostica negli appunti e lo dice sul bottone stesso, che è il posto
    ''' dove chi ha appena premuto sta già guardando.
    ''' </summary>
    Private Sub CopiaLaDiagnostica(mittente As Object, e As EventArgs) Handles btnCopiaDiagnostica.Click

        If _diagnostica Is Nothing Then Return

        Try
            Clipboard.SetText(_diagnostica())
            btnCopiaDiagnostica.Text = "Copiata"
        Catch ex As Exception
            ' Gli appunti sono di tutto il sistema, e ogni tanto qualcun altro li tiene:
            ' non è un guasto di questo programma, e non merita una finestra d'errore.
            btnCopiaDiagnostica.Text = "Non ci riesco"
        End Try

    End Sub

End Class
