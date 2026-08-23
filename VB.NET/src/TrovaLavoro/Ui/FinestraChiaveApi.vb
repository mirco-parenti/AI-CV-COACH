Imports System.Drawing
Imports System.Windows.Forms
Imports TrovaLavoro.Dati

''' <summary>
''' La finestra che chiede la chiave API (cap. 11.3). Compare all'avvio quando una
''' chiave non c'è — né cifrata nella cartella dati né nell'ambiente — e su richiesta
''' con l'argomento <c>--chiave</c>, per sostituirne una salvata storta.
''' </summary>
''' <remarks>
''' <para><b>Non è una porta chiusa.</b> «Non adesso» è una risposta legittima: senza
''' chiave l'applicazione si apre lo stesso e mostra quello che ha su disco — profilo,
''' candidature, documenti — e restano spente le sole funzioni che chiamano l'AI. Un
''' programma che non parte finché non gli si dà una credenziale sarebbe un ricatto,
''' oltre che una bugia sul suo funzionamento.</para>
''' <para><b>Non prova la chiave.</b> Verificarla vorrebbe dire spendere una chiamata
''' all'API in un momento in cui l'utente sta ancora entrando, e comunque non
''' distinguerebbe una chiave sbagliata da una rete che non c'è. Della forma si dice
''' quel che si vede — le chiavi di Anthropic cominciano per <c>sk-ant-</c> — ma non si
''' blocca: chi ha una chiave fatta in un altro modo la sa usare meglio di noi.</para>
''' <para>Si centra sullo <b>schermo</b> e non sul proprietario, a differenza di
''' <see cref="FinestraConfermaCritica"/>: quando compare, la finestra principale non è
''' ancora a video — nasce nel <c>Load</c>, prima che i pannelli si colleghino al motore
''' — e centrarsi su una finestra invisibile non vuol dire niente.</para>
''' </remarks>
Public Class FinestraChiaveApi

    ''' <summary>Da cosa cominciano le chiavi di Anthropic, per l'avvertimento di forma.</summary>
    Public Const PrefissoAtteso As String = "sk-ant-"

    ''' <summary>Quanto può essere larga la finestra, e quindi il testo che ci sta dentro.</summary>
    Private Const LarghezzaFinestra As Integer = 620

    ''' <summary>
    ''' La chiave che l'utente ha digitato, o <c>Nothing</c> se non ha salvato niente.
    ''' </summary>
    Public ReadOnly Property ChiaveDigitata As String

    ''' <summary>
    ''' Se la riga «adesso ne è salvata una» è in mostra, cioè se questa finestra sta
    ''' <b>sostituendo</b> una chiave invece di chiederne la prima.
    ''' </summary>
    ''' <remarks>
    ''' Lo stato sta qui e non in <c>lblSalvata.Visible</c> per una ragione che è già
    ''' costata un difetto: <see cref="Control.Visible"/> di un figlio dice se il
    ''' controllo <i>si vede davvero</i>, e su una finestra non ancora mostrata è sempre
    ''' falso — anche subito dopo averlo acceso. Il layout si calcola nel costruttore,
    ''' cioè proprio lì, e leggendo quella proprietà non avrebbe mai lasciato il posto a
    ''' questa riga.
    ''' </remarks>
    Public ReadOnly Property MostraLaChiaveInUso As Boolean

    ''' <summary>
    ''' Se l'avvertimento sulla forma della chiave è in mostra. Stessa ragione della
    ''' proprietà qui sopra: è lo stato voluto, non quello che si vede a video.
    ''' </summary>
    Public ReadOnly Property AvvertimentoDiForma As Boolean

    ''' <summary>
    ''' Prepara la finestra. È pubblica perché il banco di collaudo la costruisce e la
    ''' interroga senza mostrarla: di una finestra modale non si può aspettare la
    ''' chiusura.
    ''' </summary>
    ''' <param name="giaSalvata">
    ''' La chiave attualmente in uso, se ce n'è una: viene mostrata <b>mascherata</b>,
    ''' per far capire che quella nuova prende il posto di una che c'era. <c>Nothing</c>
    ''' al primo avvio.
    ''' </param>
    Public Sub New(Optional giaSalvata As String = Nothing)

        InitializeComponent()

        lblSpiegazione.Text =
            "TrovaLavoro chiama l'intelligenza artificiale di Anthropic per leggere il tuo CV, " &
            "analizzare gli annunci e scrivere i documenti. Per farlo gli serve la tua chiave API: " &
            "la trovi su console.anthropic.com, alla voce «API keys»." & vbLf & vbLf &
            "La chiave resta su questo computer, cifrata con la protezione dati di Windows e legata " &
            "al tuo account: copiata su un altro PC non si apre. Non compare mai per intero, né a " &
            "video né nella diagnostica del programma." & vbLf & vbLf &
            "Puoi anche non darla adesso: il programma si apre lo stesso e ti mostra quello che hai " &
            "già su disco — profilo, candidature, documenti — ma le funzioni che chiamano l'AI " &
            "restano spente."

        _MostraLaChiaveInUso = Not String.IsNullOrWhiteSpace(giaSalvata)

        If MostraLaChiaveInUso Then
            lblSalvata.Text = $"Adesso ne è salvata una: {ArchivioSegreti.Maschera(giaSalvata)}. " &
                              "Quella che scrivi qui prende il suo posto."
            lblSalvata.Visible = True
        End If

        lblRichiesta.Text = "Incolla qui la chiave:"

        lblForma.Text = $"Di solito le chiavi di Anthropic cominciano per «{PrefissoAtteso}». " &
                        "Questa non lo fa: se sei sicuro, salvala pure."

        Vesti()
        Disponi()

        ' Invio salva ed Esc rimanda: qui non c'è niente che non si disfaccia — una
        ' chiave sbagliata si riscrive — e chi incolla in una casella preme Invio.
        AcceptButton = btnSalva
        CancelButton = btnNonAdesso

    End Sub

    ''' <summary>
    ''' Mostra la finestra e restituisce la chiave digitata, o <c>Nothing</c> se
    ''' l'utente ha rimandato. È la porta da cui passano i chiamanti: la finestra si
    ''' smaltisce da sé.
    ''' </summary>
    Public Shared Function Chiedi(proprietario As IWin32Window,
                                  Optional giaSalvata As String = Nothing) As String

        ' Quel che conta è la chiave, non l'esito della finestra: a riempirla è il solo
        ' bottone «Salva», quindi Esc, la X e «Non adesso» tornano tutti Nothing senza
        ' che nessuno debba ricordarsi di controllare il DialogResult.
        Using finestra As New FinestraChiaveApi(giaSalvata)
            finestra.ShowDialog(proprietario)
            Return finestra.ChiaveDigitata
        End Using

    End Function

    ''' <summary>I colori e i font della finestra, tutti da <see cref="StileApp"/> (cap. 03.2).</summary>
    Private Sub Vesti()

        BackColor = StileApp.SfondoContenuto
        Font = StileApp.FontTesto

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli

        lblSpiegazione.ForeColor = StileApp.TestoPrimario
        lblSalvata.ForeColor = StileApp.TestoSecondario
        lblRichiesta.ForeColor = StileApp.TestoPrimario
        lblForma.ForeColor = StileApp.RossoTitoli

        txtChiave.BackColor = StileApp.SfondoContenuto
        chkMostra.ForeColor = StileApp.TestoSecondario

        ' Salvare la propria chiave non è un rischio e non è il passo avanti di un
        ' flusso: è una conferma senza conseguenze (livello 1, cap. 03.3).
        StileApp.VestiBottone(btnSalva, LivelloBottone.SicuroPositivo)
        StileApp.VestiBottone(btnNonAdesso, LivelloBottone.Neutro)

    End Sub

    ''' <summary>
    ''' Mette in colonna il testo, la casella e i bottoni. Come nella finestra di
    ''' conferma critica si fa qui e non nel designer, perché è la finestra ad adattarsi
    ''' al testo: con o senza la riga della chiave già salvata, l'altezza cambia.
    ''' </summary>
    Private Sub Disponi()

        Dim sinistra As Integer = StileApp.MargineRiquadro
        ' La larghezza di progetto in pixel veri: dichiararla cruda stringeva la finestra
        ' di un terzo mentre i testi dentro crescevano col DPI, e a mandare a capo il
        ' doppio delle righe era proprio questo (decisione 15.7).
        Dim larghezza As Integer = ScalaSchermo.InPixelDelloSchermo(LarghezzaFinestra, Me.DeviceDpi)
        Dim larghezzaUtile As Integer = larghezza - 2 * StileApp.MargineRiquadro

        lblSpiegazione.MaximumSize = New Size(larghezzaUtile, 0)
        lblSalvata.MaximumSize = New Size(larghezzaUtile, 0)
        lblRichiesta.MaximumSize = New Size(larghezzaUtile, 0)
        lblForma.MaximumSize = New Size(larghezzaUtile, 0)

        lblTitolo.Location = New Point(sinistra, StileApp.MargineRiquadro)
        lblSpiegazione.Location = New Point(sinistra, lblTitolo.Bottom + StileApp.DistanzaControlli)

        Dim dopoLaSpiegazione As Integer = lblSpiegazione.Bottom + StileApp.MargineRiquadro

        ' La riga della chiave già salvata c'è solo quando una chiave c'è: quando manca
        ' non lascia il suo buco, e la richiesta risale al suo posto.
        lblSalvata.Location = New Point(sinistra, dopoLaSpiegazione)
        Dim dopoLaRiga As Integer = If(MostraLaChiaveInUso,
                                       lblSalvata.Bottom + StileApp.MargineRiquadro,
                                       dopoLaSpiegazione)

        lblRichiesta.Location = New Point(sinistra, dopoLaRiga)
        txtChiave.Location = New Point(sinistra, lblRichiesta.Bottom + StileApp.InterlineaMinima)

        ' La spunta sta accanto alla casella, allineata al suo mezzo: è un interruttore
        ' di quella casella, non una riga a sé.
        chkMostra.Location = New Point(txtChiave.Right + StileApp.DistanzaControlli,
                                       txtChiave.Top + (txtChiave.Height - chkMostra.Height) \ 2)

        ' L'avvertimento di forma compare e sparisce mentre si scrive, ma il suo spazio
        ' resta occupato in ogni caso: una finestra che sussulta a ogni carattere
        ' incollato si legge peggio di una riga vuota.
        lblForma.Location = New Point(sinistra, txtChiave.Bottom + StileApp.InterlineaMinima)

        Dim riga As Integer = lblForma.Bottom + StileApp.MargineRiquadro
        btnNonAdesso.Location = New Point(larghezza - StileApp.MargineRiquadro - btnNonAdesso.Width, riga)
        btnSalva.Location = New Point(btnNonAdesso.Left - StileApp.DistanzaControlli - btnSalva.Width, riga)

        ' Un tetto sullo spazio che c'è, e lo scorrimento per quel che non ci sta: le due
        ' cose insieme, perché il tetto da solo taglierebbe e lo scorrimento da solo
        ' lascerebbe la finestra fuori schermo. Senza, a 150% questa finestra si dimensionava
        ' sul proprio contenuto e il sistema la troncava: quel che restava fuori cadeva fuori
        ' dalla <i>finestra</i>, non dallo schermo, e nessuno spostamento lo recuperava
        ' (cap. 03.4, decisione 15.7).
        Dim voluta As Integer = btnNonAdesso.Bottom + StileApp.MargineRiquadro
        Dim disponibile As Integer = ScalaSchermo.SpazioClienteDisponibile(
            Screen.FromControl(Me).WorkingArea.Height, Me.Height - Me.ClientSize.Height)

        Me.AutoScroll = ScalaSchermo.ServeScorrimento(voluta, disponibile)
        ClientSize = New Size(larghezza, ScalaSchermo.AltezzaSostenibile(voluta, disponibile))

    End Sub

    ''' <summary>
    ''' La guardia e l'avvertimento: il bottone si accende quando c'è qualcosa da
    ''' salvare, e la riga sulla forma compare quando quel qualcosa non somiglia a una
    ''' chiave di Anthropic. Compare, non impedisce.
    ''' </summary>
    Private Sub txtChiave_TextChanged(sender As Object, e As EventArgs) Handles txtChiave.TextChanged

        Dim scritta As String = txtChiave.Text.Trim()

        btnSalva.Enabled = scritta.Length > 0

        _AvvertimentoDiForma = scritta.Length > 0 AndAlso
                               Not scritta.StartsWith(PrefissoAtteso, StringComparison.OrdinalIgnoreCase)
        lblForma.Visible = AvvertimentoDiForma

    End Sub

    ''' <summary>
    ''' Mostra o nasconde quel che si sta scrivendo. Serve a chi la digita a mano e a chi
    ''' vuole controllare di aver incollato la cosa giusta: la mascheratura è contro gli
    ''' occhi di passaggio, non contro l'utente.
    ''' </summary>
    Private Sub chkMostra_CheckedChanged(sender As Object, e As EventArgs) Handles chkMostra.CheckedChanged
        txtChiave.UseSystemPasswordChar = Not chkMostra.Checked
    End Sub

    ''' <summary>
    ''' Prende quel che è scritto nella casella e lo consegna a
    ''' <see cref="ChiaveDigitata"/>; dice se c'era qualcosa da prendere.
    ''' </summary>
    ''' <remarks>
    ''' Sta in un metodo suo, staccato dal bottone, per la stessa ragione per cui «l'ho
    ''' spedita» è staccato dalla domanda che lo precede (cap. 07.1): è l'unico modo di
    ''' collaudarlo. Il bottone di una finestra mai mostrata <b>non si lascia premere</b>
    ''' — <c>PerformClick</c> vuole un controllo selezionabile, e senza finestra a video
    ''' nessun controllo lo è — e mostrare una modale in un banco vuol dire aspettare
    ''' per sempre.
    ''' </remarks>
    Public Function PrendiLaChiaveScritta() As Boolean

        ' Il bottone è spento a casella vuota, ma la guardia si ripete: Invio è legato a
        ' questo bottone, e un bottone spento premuto da tastiera non deve far salvare
        ' una chiave che non c'è.
        Dim scritta As String = txtChiave.Text.Trim()
        If scritta.Length = 0 Then Return False

        _ChiaveDigitata = scritta
        Return True

    End Function

    Private Sub btnSalva_Click(sender As Object, e As EventArgs) Handles btnSalva.Click

        If Not PrendiLaChiaveScritta() Then Return

        DialogResult = DialogResult.OK
        Close()

    End Sub

    Private Sub btnNonAdesso_Click(sender As Object, e As EventArgs) Handles btnNonAdesso.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

End Class
