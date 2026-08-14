Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' La finestra delle azioni di <b>livello 6</b> (cap. 03.3): quelle che non si disfano.
''' Dice cosa sparisce e cosa resta, e per accendere il bottone chiede di <b>ridigitare
''' una parola</b> — il nome dell'applicazione, come vuole il cap. 11.5.
''' </summary>
''' <remarks>
''' <para>Non è una <see cref="MessageBox"/> e non poteva esserlo: un Sì/No si preme di
''' riflesso, e qui la mano deve scrivere. È la differenza fra «sei sicuro?», che l'utente
''' impara a scacciare come una mosca, e un gesto che chiede di essere voluto.</para>
''' <para>Nasce per «ELIMINA PROFILO - DEFINITIVO» (P2, cap. 11.5) ma non sa nulla di
''' profili: titolo, spiegazione, etichetta del bottone e parola arrivano da fuori. Le
''' eliminazioni delle Impostazioni (T9) useranno questa, e non un'altra finestra che le
''' somiglia.</para>
''' <para>Invio <b>non</b> conferma — non c'è nessun <c>AcceptButton</c> — ed Esc annulla:
''' la scorciatoia esiste solo per la via d'uscita. Il confronto della parola ignora le
''' maiuscole e gli spazi ai bordi: quel che serve è che l'utente la scriva, non che
''' indovini dove sta la maiuscola interna di «TrovaLavoro».</para>
''' </remarks>
Public Class FinestraConfermaCritica

    ''' <summary>La parola che si chiede di ridigitare: il nome che l'utente conosce (cap. 11.5).</summary>
    Public Const ParolaPredefinita As String = "TrovaLavoro"

    ''' <summary>Quanto può essere larga la finestra, e quindi il testo che ci sta dentro.</summary>
    Private Const LarghezzaFinestra As Integer = 620

    Private ReadOnly _parola As String

    ''' <summary>
    ''' Prepara la finestra. È pubblica perché il banco di collaudo la costruisce e la
    ''' interroga senza mostrarla: di una finestra modale non si può aspettare la chiusura.
    ''' </summary>
    ''' <param name="titolo">Cosa sta per succedere, in una riga.</param>
    ''' <param name="spiegazione">Cosa sparisce e cosa resta: è il testo che l'utente deve leggere.</param>
    ''' <param name="etichettaAzione">Il bottone che esegue, detto con il verbo dell'azione.</param>
    ''' <param name="parola">La parola da ridigitare; di norma il nome dell'applicazione.</param>
    Public Sub New(titolo As String, spiegazione As String, etichettaAzione As String,
                   Optional parola As String = ParolaPredefinita)

        InitializeComponent()

        _parola = If(String.IsNullOrWhiteSpace(parola), ParolaPredefinita, parola.Trim())

        lblTitolo.Text = titolo
        lblSpiegazione.Text = spiegazione
        lblRichiesta.Text = $"Per procedere scrivi qui sotto la parola «{_parola}»." & vbLf &
                            "Finché non è scritta esatta, il bottone resta spento."
        btnAzione.Text = etichettaAzione

        Vesti()
        Disponi()

        ' Esc chiude senza fare niente; non c'è nessun bottone legato a Invio, perché
        ' quel tasto lo si preme uscendo dalla casella, per abitudine.
        CancelButton = btnAnnulla
        AcceptButton = Nothing

    End Sub

    ''' <summary>
    ''' Mostra la finestra e dice se l'utente ha confermato. È la porta da cui passano i
    ''' chiamanti: la finestra si smaltisce da sé.
    ''' </summary>
    Public Shared Function Chiedi(proprietario As IWin32Window, titolo As String, spiegazione As String,
                                  etichettaAzione As String,
                                  Optional parola As String = ParolaPredefinita) As Boolean

        Using finestra As New FinestraConfermaCritica(titolo, spiegazione, etichettaAzione, parola)
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
        lblRichiesta.ForeColor = StileApp.TestoSecondario

        txtParola.BackColor = StileApp.SfondoContenuto

        StileApp.VestiBottone(btnAzione, LivelloBottone.Critico)
        StileApp.VestiBottone(btnAnnulla, LivelloBottone.Neutro)

    End Sub

    ''' <summary>
    ''' Mette in colonna il testo, la casella e i bottoni. Si fa qui e non nel designer
    ''' perché la spiegazione è lunga quanto serve: è la finestra ad adattarsi al testo,
    ''' non il testo a doversi accorciare per stare nella finestra.
    ''' </summary>
    Private Sub Disponi()

        Dim sinistra As Integer = StileApp.MargineRiquadro
        Dim larghezzaUtile As Integer = LarghezzaFinestra - 2 * StileApp.MargineRiquadro

        lblSpiegazione.MaximumSize = New Size(larghezzaUtile, 0)
        lblRichiesta.MaximumSize = New Size(larghezzaUtile, 0)

        lblTitolo.Location = New Point(sinistra, StileApp.MargineRiquadro)
        lblSpiegazione.Location = New Point(sinistra, lblTitolo.Bottom + StileApp.DistanzaControlli)
        lblRichiesta.Location = New Point(sinistra, lblSpiegazione.Bottom + StileApp.MargineRiquadro)
        txtParola.Location = New Point(sinistra, lblRichiesta.Bottom + StileApp.InterlineaMinima)

        ' I bottoni in fondo a destra, con l'annulla più a destra di quello che esegue:
        ' il posto d'onore va alla via d'uscita, non all'azione che non si disfa.
        Dim riga As Integer = txtParola.Bottom + StileApp.MargineRiquadro
        btnAnnulla.Location = New Point(LarghezzaFinestra - StileApp.MargineRiquadro - btnAnnulla.Width, riga)
        btnAzione.Location = New Point(btnAnnulla.Left - StileApp.DistanzaControlli - btnAzione.Width, riga)

        ClientSize = New Size(LarghezzaFinestra, btnAnnulla.Bottom + StileApp.MargineRiquadro)

    End Sub

    ''' <summary>
    ''' La guardia: il bottone si accende solo quando la parola è quella. Vale mentre si
    ''' scrive, così l'utente vede da sé quando il gesto è compiuto.
    ''' </summary>
    Private Sub txtParola_TextChanged(sender As Object, e As EventArgs) Handles txtParola.TextChanged
        btnAzione.Enabled = String.Equals(txtParola.Text.Trim(), _parola, StringComparison.OrdinalIgnoreCase)
    End Sub

    Private Sub btnAzione_Click(sender As Object, e As EventArgs) Handles btnAzione.Click

        ' Il bottone è spento quando la parola non torna, ma la guardia si ripete qui:
        ' un click programmatico non passa dall'interfaccia, e questa è l'ultima porta
        ' prima di un'azione che non si disfa.
        If Not btnAzione.Enabled Then Return

        DialogResult = DialogResult.OK
        Close()

    End Sub

    Private Sub btnAnnulla_Click(sender As Object, e As EventArgs) Handles btnAnnulla.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

End Class
