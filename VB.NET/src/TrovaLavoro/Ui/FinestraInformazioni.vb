Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' «Informazioni su…» (cap. 03.4): chi è questo programma, che versione è, con quale
''' libreria di prompt sta lavorando. Si apre dal pannello del logo, in basso a sinistra
''' (cap. 03.5), che è il posto dove quei numeri si vanno già a cercare.
''' </summary>
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
    Public Sub New(etichettaPool As String)

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

        StileApp.VestiBottone(btnChiudi, LivelloBottone.Neutro)

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

    ''' <summary>Apre la finestra davanti a chi l'ha chiesta.</summary>
    Public Shared Sub Mostra(proprietario As IWin32Window, etichettaPool As String)
        Using finestra As New FinestraInformazioni(etichettaPool)
            finestra.ShowDialog(proprietario)
        End Using
    End Sub

    ''' <summary>
    ''' Chiude il vuoto lasciato dall'immagine mancante, tirando su le righe e
    ''' accorciando la finestra della stessa misura.
    ''' </summary>
    Private Sub TogliLoSpazioDellImmagine()

        Dim salto As Integer = picMarchio.Height + StileApp.DistanzaControlli
        picMarchio.Visible = False

        For Each figlio As Control In New Control() {lblVersione, lblCopyright, btnChiudi}
            figlio.Top -= salto
        Next

        Me.ClientSize = New Size(Me.ClientSize.Width, Me.ClientSize.Height - salto)

    End Sub

    Private Sub Chiudi(mittente As Object, e As EventArgs) Handles btnChiudi.Click
        Me.Close()
    End Sub

End Class
