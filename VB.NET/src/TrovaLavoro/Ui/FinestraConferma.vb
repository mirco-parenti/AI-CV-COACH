Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' La finestra delle azioni di <b>livello 5</b> (cap. 03.3): quelle che non si disfano
''' ma si ripetono. Dice che cosa sparisce, e per procedere chiede un clic sul <b>verbo</b>
''' dell'azione — non su un «Sì».
''' </summary>
''' <remarks>
''' <para><b>Perché non una <see cref="MessageBox"/></b>, che pure basta allo «Scarta» di
''' P4: lì i due tasti sono Sì e No, e un «Sì» risponde alla domanda. Qui il bottone che
''' esegue porta il nome di quel che fa, e risponde alla <i>conseguenza</i> — che è la riga
''' che si vuole far leggere.</para>
''' <para><b>Non è la sorella maggiore</b> <see cref="FinestraConfermaCritica"/>, e non
''' deve diventarlo: quella è di livello 6 e chiede di <b>riscrivere una parola</b> perché
''' difende gesti che si fanno una volta sola — il profilo, tutti i dati. Questa difende un
''' gesto che si ripete, come ripulire la coda da una prova o da un doppione, e farlo
''' costare una parola a mano insegnerebbe solo a scriverla senza leggere (cap. 11.5).</para>
''' <para>Come lì, <b>Invio non conferma</b> — non c'è nessun <c>AcceptButton</c> — ed Esc
''' annulla: la scorciatoia esiste solo per la via d'uscita. E il fuoco parte
''' sull'<b>annulla</b>, che ha il <c>TabIndex</c> più basso fra i due: è il tasto che si
''' può premere per sbaglio senza pentirsene.</para>
''' </remarks>
Public Class FinestraConferma

    ''' <summary>Quanto può essere larga la finestra, e quindi il testo che ci sta dentro.</summary>
    Private Const LarghezzaFinestra As Integer = 620

    ''' <summary>
    ''' Prepara la finestra. È pubblica perché il banco di collaudo la costruisce e la
    ''' interroga senza mostrarla: di una finestra modale non si può aspettare la chiusura.
    ''' </summary>
    ''' <param name="titolo">Cosa sta per succedere, in una riga.</param>
    ''' <param name="spiegazione">Cosa sparisce: è il testo che l'utente deve leggere.</param>
    ''' <param name="etichettaAzione">Il bottone che esegue, detto con il verbo dell'azione.</param>
    Public Sub New(titolo As String, spiegazione As String, etichettaAzione As String)

        InitializeComponent()

        lblTitolo.Text = titolo
        lblSpiegazione.Text = spiegazione
        btnAzione.Text = etichettaAzione

        Vesti()
        Disponi()

        CancelButton = btnAnnulla
        AcceptButton = Nothing

    End Sub

    ''' <summary>
    ''' Mostra la finestra e dice se l'utente ha confermato. È la porta da cui passano i
    ''' chiamanti: la finestra si smaltisce da sé.
    ''' </summary>
    Public Shared Function Chiedi(proprietario As IWin32Window, titolo As String,
                                  spiegazione As String, etichettaAzione As String) As Boolean

        Using finestra As New FinestraConferma(titolo, spiegazione, etichettaAzione)
            Return finestra.ShowDialog(proprietario) = DialogResult.OK
        End Using

    End Function

    ''' <summary>I colori e i font della finestra, tutti da <see cref="StileApp"/> (cap. 03.2).</summary>
    Private Sub Vesti()

        BackColor = StileApp.SfondoContenuto
        Font = StileApp.FontTesto

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli

        lblSpiegazione.ForeColor = StileApp.TestoPrimario

        StileApp.VestiBottone(btnAzione, LivelloBottone.Distruttivo)
        StileApp.VestiBottone(btnAnnulla, LivelloBottone.Neutro)

    End Sub

    ''' <summary>
    ''' Mette in colonna il testo e i due bottoni. Si fa qui e non nel designer perché la
    ''' spiegazione è lunga quanto serve: è la finestra ad adattarsi al testo, non il testo
    ''' a doversi accorciare per stare nella finestra.
    ''' </summary>
    Private Sub Disponi()

        Dim sinistra As Integer = StileApp.MargineRiquadro

        ' La larghezza di progetto in pixel veri, come nella sorella maggiore: dichiararla
        ' cruda stringerebbe la finestra mentre i testi dentro crescono col DPI, ed è
        ' proprio questo a mandare a capo il doppio delle righe (decisione 15.7).
        Dim larghezza As Integer = ScalaSchermo.InPixelDelloSchermo(LarghezzaFinestra, Me.DeviceDpi)
        Dim larghezzaUtile As Integer = larghezza - 2 * StileApp.MargineRiquadro

        lblSpiegazione.MaximumSize = New Size(larghezzaUtile, 0)

        lblTitolo.Location = New Point(sinistra, StileApp.MargineRiquadro)
        lblSpiegazione.Location = New Point(sinistra, lblTitolo.Bottom + StileApp.DistanzaControlli)

        ' I bottoni in fondo a destra, con l'annulla più a destra di quello che esegue: il
        ' posto d'onore va alla via d'uscita, non all'azione che non si disfa.
        Dim riga As Integer = lblSpiegazione.Bottom + StileApp.MargineRiquadro
        btnAnnulla.Location = New Point(larghezza - StileApp.MargineRiquadro - btnAnnulla.Width, riga)
        btnAzione.Location = New Point(btnAnnulla.Left - StileApp.DistanzaControlli - btnAzione.Width, riga)

        ' Un tetto sullo spazio che c'è, e lo scorrimento per quel che non ci sta: le due
        ' cose insieme, per la ragione spiegata in FinestraConfermaCritica (cap. 03.4,
        ' decisione 15.7).
        Dim voluta As Integer = btnAnnulla.Bottom + StileApp.MargineRiquadro
        Dim disponibile As Integer = ScalaSchermo.SpazioClienteDisponibile(
            Screen.FromControl(Me).WorkingArea.Height, Me.Height - Me.ClientSize.Height)

        Me.AutoScroll = ScalaSchermo.ServeScorrimento(voluta, disponibile)
        ClientSize = New Size(larghezza, ScalaSchermo.AltezzaSostenibile(voluta, disponibile))

    End Sub

    Private Sub btnAzione_Click(sender As Object, e As EventArgs) Handles btnAzione.Click

        DialogResult = DialogResult.OK
        Close()

    End Sub

    Private Sub btnAnnulla_Click(sender As Object, e As EventArgs) Handles btnAnnulla.Click

        DialogResult = DialogResult.Cancel
        Close()

    End Sub

End Class
