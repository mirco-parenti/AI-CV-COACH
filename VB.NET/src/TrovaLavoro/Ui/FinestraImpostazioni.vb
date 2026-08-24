Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

''' <summary>
''' La finestra delle Impostazioni, il pannello P8 (cap. 03, tabella dei pannelli). Tiene
''' insieme quel che è <b>configurazione</b> e non un passo di un flusso: la chiave API, le
''' preferenze sui documenti, dove stanno le cartelle, cosa gira sotto il cofano, e le
''' pulizie del cap. 11.5.
''' </summary>
''' <remarks>
''' <para><b>Non ha OK e non ha Annulla</b>, ma un solo «Chiudi». Le preferenze si scrivono
''' <i>appena si cambiano</i>: ogni voce qui dentro si disfa con un secondo clic, quindi
''' uno stato «cambiato ma non ancora salvato» sarebbe solo un tranello in più da
''' ricordare. Le cose che invece non si disfano — le due pulizie — hanno la loro conferma
''' prima di partire, che è dove la difesa serve davvero.</para>
''' <para><b>Richiama invece di rifare.</b> La chiave passa dalla
''' <see cref="FinestraChiaveApi"/> del primo avvio, il backup dalla
''' <see cref="FinestraBackup"/> nata a T9a, l'eliminazione totale dalla
''' <see cref="FinestraConfermaCritica"/> che chiede la parola. Tre finestre che esistono
''' già e che qui non hanno un sosia: se un giorno cambia il modo di chiedere una chiave,
''' cambia in un posto solo.</para>
''' <para><b>Quel che non si tocca da qui.</b> La <i>cartella dati</i> si mostra e si apre
''' ma non si sposta: il lucchetto è preso all'avvio e per tutta la sessione (cap. 09.4),
''' e cambiarla a metà partita vorrebbe dire spostare file sotto i piedi di chi ci sta
''' scrivendo — la si sceglie all'avvio con <c>--dati</c>, che è il momento in cui nessuno
''' ci sta lavorando. I <i>modelli</i> e il <i>pool</i> si leggono e basta (cap. 11.6): i
''' primi vivono in <c>modelli.json</c> apposta perché cambiarli costi una riga e non una
''' nuova build, e il secondo si sigilla dal repo, non da un eseguibile distribuito. La
''' <i>taratura</i> non compare affatto, che è la sua regola da sempre.</para>
''' </remarks>
Public Class FinestraImpostazioni

    ''' <summary>Quanto è larga la finestra, e quindi il testo che ci sta dentro.</summary>
    Private Const LarghezzaFinestra As Integer = 660

    Private ReadOnly _contesto As ContestoApp
    Private ReadOnly _pulizia As PuliziaDati

    ''' <summary>Vero mentre si riempiono i controlli: gli eventi non devono salvare nulla.</summary>
    Private _sto As Boolean = True

    ''' <summary>
    ''' Se l'utente ha dato una chiave nuova: chi ha aperto la finestra deve <b>rimontare</b>
    ''' il motore, che è il modo di accendere i servizi che dalla chiave dipendono senza
    ''' inseguirli uno per uno.
    ''' </summary>
    Public ReadOnly Property ChiaveCambiata As Boolean = False

    ''' <summary>
    ''' Se l'utente ha chiesto di gestire i documenti: il giro vive in P7, dove si sa
    ''' aspettare l'AI e annullarla, e questa finestra ce lo manda invece di rifarlo.
    ''' </summary>
    Public ReadOnly Property VuoleGestireIDocumenti As Boolean = False

    ''' <summary>Se la cartella dati è stata svuotata: l'applicazione non può continuare com'era.</summary>
    Public ReadOnly Property DatiEliminati As Boolean = False

    ''' <summary>
    ''' Prepara la finestra. È pubblica perché il banco la costruisce e la interroga senza
    ''' mostrarla: di una finestra modale non si può aspettare la chiusura.
    ''' </summary>
    Public Sub New(contesto As ContestoApp)

        InitializeComponent()

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
        _contesto = contesto
        _pulizia = New PuliziaDati(contesto.Cartella)

        lblSpiegazione.Text =
            "Qui stanno le scelte che valgono per tutto il programma. Le preferenze si " &
            "salvano da sé, appena le cambi." & vbLf &
            "Quel che si legge e non si tocca è scritto in chiaro: sta in un file della " &
            "cartella dati, e da lì si corregge a mano."

        RaccontaTutto()
        Vesti()
        Disponi()

        _sto = False

        CancelButton = btnChiudi
        AcceptButton = Nothing

    End Sub

    ''' <summary>Riempie ogni sezione con quel che vale adesso.</summary>
    Private Sub RaccontaTutto()

        RaccontaLaChiave()
        RaccontaLePreferenze()
        RaccontaLeCartelle()
        RaccontaIlMotore()
        RaccontaCosaSiPuoPulire()

    End Sub

    ' ==================================================================
    ' Chiave API
    ' ==================================================================

    ''' <summary>
    ''' Dice se una chiave c'è, senza mostrarla. Di una chiave salvata si fa vedere solo
    ''' la coda: serve a riconoscerla, non a rileggerla (cap. 11.3).
    ''' </summary>
    Private Sub RaccontaLaChiave()

        Dim illeggibile As Boolean
        Dim chiave As String = _contesto.Segreti.LeggiChiaveApi(illeggibile)

        If illeggibile Then
            lblStatoChiave.Text = "C'è una chiave salvata, ma non si lascia decifrare su questo utente " &
                                  "di Windows. Riscrivila e tornerà a funzionare."
            btnCambiaChiave.Text = "Riscrivi la chiave…"
            Return
        End If

        If String.IsNullOrWhiteSpace(chiave) Then
            lblStatoChiave.Text = "Nessuna chiave salvata: senza, tutto ciò che passa dall'AI resta spento."
            btnCambiaChiave.Text = "Scrivi la chiave…"
            Return
        End If

        lblStatoChiave.Text = $"Chiave salvata ({Mascherata(chiave)}), cifrata per questo utente di Windows."
        btnCambiaChiave.Text = "Cambia la chiave…"

    End Sub

    ''' <summary>Le ultime quattro lettere, il resto coperto: quanto basta a riconoscerla.</summary>
    Private Shared Function Mascherata(chiave As String) As String

        Dim pulita As String = chiave.Trim()
        If pulita.Length <= 4 Then Return New String("•"c, pulita.Length)

        Return New String("•"c, 8) & pulita.Substring(pulita.Length - 4)

    End Function

    Private Sub btnCambiaChiave_Click(sender As Object, e As EventArgs) Handles btnCambiaChiave.Click

        Dim illeggibile As Boolean
        Dim digitata As String = FinestraChiaveApi.Chiedi(Me, _contesto.Segreti.LeggiChiaveApi(illeggibile))
        If digitata Is Nothing Then Return

        Try
            _contesto.Segreti.SalvaChiaveApi(digitata)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Racconta($"La chiave non si è potuta salvare ({ex.Message}): vale per questa sessione.",
                     StileApp.RossoTitoli)
        End Try

        _ChiaveCambiata = True
        RaccontaLaChiave()
        Racconta("Chiave aggiornata: i servizi che ne dipendono si riaccendono alla chiusura di questa finestra.",
                 StileApp.TestoSecondario)

    End Sub

    ' ==================================================================
    ' Le preferenze, che si salvano da sé
    ' ==================================================================

    ''' <summary>Porta nei controlli le preferenze in vigore.</summary>
    Private Sub RaccontaLePreferenze()

        cmbLingua.Items.Clear()
        cmbLingua.Items.Add(LinguaDocumenti.Nome(LinguaDocumenti.Italiano))
        cmbLingua.Items.Add(LinguaDocumenti.Nome(LinguaDocumenti.Inglese))

        cmbLingua.SelectedIndex =
            If(_contesto.Impostazioni.LinguaPredefinita = LinguaDocumenti.Inglese, 1, 0)

        chkRifinitura.Checked = _contesto.Impostazioni.RifinituraAttiva

        lblRifinituraNota.Text =
            "Spenta, i testi escono come li scrive il modello: è la stessa cosa che " &
            "succede quando una rifinitura fallisce, non una strada nuova."

        ' I limiti vengono dalla classe che li conosce, non da un numero ricopiato nel
        ' designer: il tetto è dichiarato una volta sola (cap. 07.3).
        numFollowUp.Minimum = 0
        numFollowUp.Maximum = Impostazioni.GiorniFollowUpMassimi
        numFollowUp.Value = Math.Max(numFollowUp.Minimum,
                                     Math.Min(numFollowUp.Maximum, _contesto.Impostazioni.GiorniFollowUp))

        lblFollowUpNota.Text =
            "Nella Home le candidature spedite e rimaste senza risposta si segnalano da " &
            "sole, con da quanti giorni aspettano. Zero spegne il promemoria."

    End Sub

    Private Sub cmbLingua_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbLingua.SelectedIndexChanged
        SalvaLePreferenze()
    End Sub

    Private Sub chkRifinitura_CheckedChanged(sender As Object, e As EventArgs) _
        Handles chkRifinitura.CheckedChanged
        SalvaLePreferenze()
    End Sub

    Private Sub numFollowUp_ValueChanged(sender As Object, e As EventArgs) _
        Handles numFollowUp.ValueChanged
        SalvaLePreferenze()
    End Sub

    ''' <summary>
    ''' Scrive le preferenze e le fa rileggere al contesto, così chi le usa vede quelle
    ''' nuove senza aspettare un riavvio.
    ''' </summary>
    Private Sub SalvaLePreferenze()

        If _sto Then Return

        Dim scelte As New Impostazioni With {
            .LinguaPredefinita = If(cmbLingua.SelectedIndex = 1,
                                    LinguaDocumenti.Inglese, LinguaDocumenti.Italiano),
            .RifinituraAttiva = chkRifinitura.Checked,
            .GiorniFollowUp = CInt(numFollowUp.Value)}

        Try
            _contesto.ArchivioImpostazioni.Salva(scelte)
            _contesto.RileggiLeImpostazioni()
            Racconta("Preferenze salvate.", StileApp.TestoSecondario)

            ' Salvare crea impostazioni.json, e su una cartella dati nuova quello è il
            ' primo dato che ci sia mai stato: il bottone che elimina tutto ha qualcosa da
            ' fare adesso, non alla prossima apertura della finestra.
            RaccontaCosaSiPuoPulire()

        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Racconta($"Le preferenze non si sono potute scrivere ({ex.Message}). " &
                     "Valgono per questa sessione, ma al prossimo avvio saranno quelle di prima.",
                     StileApp.RossoTitoli)
        End Try

    End Sub

    ' ==================================================================
    ' Le cartelle
    ' ==================================================================

    Private Sub RaccontaLeCartelle()

        lblCartellaDati.Text =
            "Cartella dati: " & _contesto.Cartella.Radice & vbLf &
            "Qui dentro sta tutto quel che il programma sa di te. Si sceglie all'avvio con " &
            "--dati: mentre l'applicazione è aperta tiene questi file per sé, e spostarli " &
            "adesso sarebbe come cambiare le fondamenta a casa abitata."

        If _contesto.Raccolta.CartellaUtilizzabile Then
            lblCartellaDocumenti.Text =
                "Cartella documenti: " & _contesto.Raccolta.Cartella & vbLf &
                $"{_contesto.Raccolta.Attestati().Count} attestati riconosciuti, da allegare alle email."
        Else
            lblCartellaDocumenti.Text =
                "Cartella documenti: nessuna scelta." & vbLf &
                "È la cartella dei tuoi attestati e diplomi: scegliendola, il programma li " &
                "riconosce e te li propone fra gli allegati."
        End If

    End Sub

    Private Sub btnApriCartellaDati_Click(sender As Object, e As EventArgs) Handles btnApriCartellaDati.Click

        Try
            _contesto.Cartella.Assicura()
            Process.Start(New ProcessStartInfo(_contesto.Cartella.Radice) With {.UseShellExecute = True})

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is System.ComponentModel.Win32Exception
            Racconta($"La cartella non si è lasciata aprire ({ex.Message}).", StileApp.RossoTitoli)
        End Try

    End Sub

    ''' <summary>
    ''' Segna che l'utente vuole sistemare i documenti e chiude: il giro vive in P7, dove
    ''' si sa aspettare l'AI e annullarla (cap. 05.2), e queste Impostazioni ce lo mandano
    ''' invece di rifarlo.
    ''' </summary>
    ''' <remarks>
    ''' È pubblica per la stessa ragione dei metodi della <see cref="FinestraBackup"/>: su
    ''' una finestra mai mostrata <c>PerformClick</c> non scatena niente, quindi il banco
    ''' non può collaudare un'azione che vive solo dentro il gestore di un bottone.
    ''' </remarks>
    Public Sub ChiediDiGestireIDocumenti()

        _VuoleGestireIDocumenti = True
        DialogResult = DialogResult.OK
        Close()

    End Sub

    Private Sub btnGestisciDocumenti_Click(sender As Object, e As EventArgs) Handles btnGestisciDocumenti.Click
        ChiediDiGestireIDocumenti()
    End Sub

    ' ==================================================================
    ' Sotto il cofano: si legge, non si tocca
    ' ==================================================================

    Private Sub RaccontaIlMotore()

        Dim daFile As String = If(_contesto.Modelli.Origine = Ai.OrigineModelli.File,
                                  "da modelli.json", "predefiniti")

        lblModelli.Text =
            $"Modelli AI ({daFile}):" & vbLf &
            $"   estrazione → {_contesto.Modelli.ModelloSemplice.Id}" & vbLf &
            $"   ragionamento → {_contesto.Modelli.ModelloRagionamento.Id}"

        If _contesto.Libreria Is Nothing Then
            lblPool.Text = "Pool dei prompt: non si è lasciato aprire."
            Return
        End If

        lblPool.Text = "Prompt: " & _contesto.Libreria.Etichetta

        ' L'asterisco nella versione dice che qualcosa non torna; qui c'è spazio per dire
        ' cosa. Chi sperimenta lo fa alla luce del sole (cap. 04.5).
        If _contesto.Libreria.Avviso IsNot Nothing Then
            lblPool.Text &= vbLf & _contesto.Libreria.Avviso
        End If

    End Sub

    Private Sub btnApriModelli_Click(sender As Object, e As EventArgs) Handles btnApriModelli.Click

        Dim percorsoModelli As String = _contesto.Cartella.FileModelli

        ' Il file può non esserci: finché nessuno l'ha scritto valgono i predefiniti
        ' (cap. 11.6). Allora si apre la cartella, che è il posto dove andrebbe messo.
        Dim daAprire As String = If(File.Exists(percorsoModelli), percorsoModelli, _contesto.Cartella.Radice)

        Try
            _contesto.Cartella.Assicura()
            Process.Start(New ProcessStartInfo(daAprire) With {.UseShellExecute = True})

            If daAprire <> percorsoModelli Then
                Racconta("Un modelli.json non c'è ancora: valgono i predefiniti. " &
                         "Ti ho aperto la cartella dove metterlo.", StileApp.TestoSecondario)
            End If

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is System.ComponentModel.Win32Exception
            Racconta($"Non si è lasciato aprire ({ex.Message}).", StileApp.RossoTitoli)
        End Try

    End Sub

    ' ==================================================================
    ' I tuoi dati
    ' ==================================================================

    Private Sub RaccontaCosaSiPuoPulire()

        btnSvuotaNavigazione.Enabled = Directory.Exists(_contesto.Cartella.CartellaWebView2)
        btnEliminaTutto.Enabled = _pulizia.CEQualcosa

    End Sub

    Private Sub btnBackup_Click(sender As Object, e As EventArgs) Handles btnBackup.Click

        Using finestra As New FinestraBackup(_contesto)
            finestra.ShowDialog(Me)
        End Using

        ' Un ripristino cambia quel che c'è nella cartella: le pulizie ne tengono conto.
        RaccontaCosaSiPuoPulire()

    End Sub

    Private Sub btnSvuotaNavigazione_Click(sender As Object, e As EventArgs) Handles btnSvuotaNavigazione.Click

        ' Livello 5: si spiega e si parte da «No», come lo «Scarta» di P4 (cap. 03.3).
        Dim risposta As DialogResult = MessageBox.Show(
            Me,
            "Butto via cronologia, cache e cookie del browser che usi per cercare annunci." & vbLf & vbLf &
            "Le tue candidature, il profilo e le ricerche salvate non si toccano: sparisce " &
            "solo quel che il browser si è ricordato per strada. Dovrai rifare gli accessi " &
            "ai portali dove eri entrato.",
            "Svuota i dati di navigazione", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2)

        If risposta <> DialogResult.Yes Then Return

        Try
            If _pulizia.SvuotaNavigazione() Then
                Racconta("Dati di navigazione svuotati.", StileApp.TestoSecondario)
            Else
                Racconta("Non c'era niente da svuotare.", StileApp.TestoSecondario)
            End If

        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            ' Il browser incorporato tiene i suoi file aperti finché P3 è vivo.
            Racconta($"Non si sono lasciati cancellare tutti ({ex.Message}): " &
                     "chiudi la ricerca annunci e riprova.", StileApp.RossoTitoli)
        End Try

        RaccontaCosaSiPuoPulire()

    End Sub

    Private Sub btnEliminaTutto_Click(sender As Object, e As EventArgs) Handles btnEliminaTutto.Click

        Dim confermato As Boolean = FinestraConfermaCritica.Chiedi(
            Me,
            "Elimina tutti i dati",
            "Sparisce tutto quello che il programma sa di te: il profilo con il suo storico e il " &
            "CV base, tutte le candidature con i documenti già scritti, il registro, la chiave " &
            "API, le preferenze, i dati di navigazione e i backup che hai lasciato nella " &
            "cartella dati." & vbLf & vbLf &
            "Restano i file che hai portato altrove — i backup su una chiavetta, i CV e le " &
            "lettere che hai esportato in un'altra cartella: quelli sono tuoi e stanno fuori " &
            "di qui." & vbLf & vbLf &
            "Non c'è un cestino e non c'è modo di tornare indietro. Alla riapertura il " &
            "programma riparte come il primo giorno.",
            "ELIMINA TUTTI I DATI")

        If Not confermato Then Return

        Try
            Dim andate As Integer = _pulizia.EliminaTutto()
            _DatiEliminati = True

            MessageBox.Show(
                Me,
                $"Fatto: {andate} voci eliminate." & vbLf & vbLf &
                "Chiudo l'applicazione: da qui in poi lavorerebbe su file che non ci sono più. " &
                "Riaprendola, ricomincia come il primo giorno.",
                "Dati eliminati", MessageBoxButtons.OK, MessageBoxIcon.Information)

            DialogResult = DialogResult.OK
            Close()

        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            Racconta($"Non si è potuto eliminare tutto ({ex.Message}). " &
                     "Qualcosa è ancora aperto: chiudi le altre finestre e riprova.",
                     StileApp.RossoTitoli)
            RaccontaCosaSiPuoPulire()
        End Try

    End Sub

    ' ==================================================================
    ' Contorno
    ' ==================================================================

    ''' <summary>Una riga di stato in fondo, come negli altri pannelli (cap. 03.8).</summary>
    Private Sub Racconta(testo As String, colore As Color)

        lblStato.Text = testo
        lblStato.ForeColor = colore

    End Sub

    Private Sub btnChiudi_Click(sender As Object, e As EventArgs) Handles btnChiudi.Click
        Close()
    End Sub

    ''' <summary>I colori e i font della finestra, tutti da <see cref="StileApp"/> (cap. 03.2).</summary>
    Private Sub Vesti()

        BackColor = StileApp.SfondoContenuto
        Font = StileApp.FontTesto

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli

        For Each sezione As Label In {lblSezioneChiave, lblSezioneDocumenti, lblSezioneCandidature,
                                      lblSezioneCartelle, lblSezioneMotore, lblSezioneDati}
            sezione.Font = StileApp.FontTitoloGruppo
            sezione.ForeColor = StileApp.TestoPrimario
        Next

        For Each testo As Label In {lblSpiegazione, lblStatoChiave, lblLingua, lblFollowUp,
                                    lblGiorni, lblCartellaDati, lblCartellaDocumenti}
            testo.ForeColor = StileApp.TestoPrimario
        Next

        For Each minore As Label In {lblRifinituraNota, lblFollowUpNota, lblModelli, lblPool}
            minore.ForeColor = StileApp.TestoSecondario
        Next

        chkRifinitura.ForeColor = StileApp.TestoPrimario
        chkRifinitura.BackColor = StileApp.SfondoContenuto

        cmbLingua.BackColor = StileApp.SfondoContenuto
        cmbLingua.ForeColor = StileApp.TestoPrimario

        numFollowUp.BackColor = StileApp.SfondoContenuto
        numFollowUp.ForeColor = StileApp.TestoPrimario

        StileApp.VestiBottone(btnCambiaChiave, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnApriCartellaDati, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnGestisciDocumenti, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnApriModelli, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnBackup, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnSvuotaNavigazione, LivelloBottone.Distruttivo)
        StileApp.VestiBottone(btnEliminaTutto, LivelloBottone.Critico)
        StileApp.VestiBottone(btnChiudi, LivelloBottone.Neutro)

    End Sub

    ''' <summary>
    ''' Mette in fila le sezioni nello spazio che lo schermo concede.
    ''' </summary>
    Private Sub Disponi()

        DisponiIn(ScalaSchermo.SpazioClienteDisponibile(
            Screen.FromControl(Me).WorkingArea.Height, Me.Height - Me.ClientSize.Height))

    End Sub

    ''' <summary>
    ''' Mette in fila le sezioni come se in altezza ci fosse questo spazio.
    ''' </summary>
    ''' <remarks>
    ''' <para>Un tetto sullo spazio che c'è, e lo scorrimento per quel che non ci sta: le
    ''' due cose insieme, perché il tetto da solo taglierebbe e lo scorrimento da solo
    ''' lascerebbe la finestra fuori schermo. Senza, a 150% questa finestra si dimensionava
    ''' sul proprio contenuto e il sistema la troncava: quel che restava fuori cadeva fuori
    ''' dalla <i>finestra</i>, non dallo schermo, e nessuno spostamento lo recuperava
    ''' (cap. 03.4, decisione 15.7).</para>
    ''' <para><b>Quando si scorre, la fila si fa due volte.</b> La barra verticale si
    ''' prende una fetta di larghezza, e il contenuto messo in fila senza saperlo le va a
    ''' finire sotto: allora si accende anche la barra orizzontale, che non ha niente da
    ''' mostrare. La seconda fila sta dentro quel che resta. La domanda non si riapre:
    ''' righe che vanno a capo prima possono solo far crescere l'altezza, e uno scorrimento
    ''' che serviva serve ancora.</para>
    ''' <para>Lo spazio si <b>riceve</b> invece di leggerlo qui dentro: quello vero lo
    ''' detta lo schermo su cui la finestra si apre, e un collaudo non può cambiare
    ''' schermo — mentre è proprio quando il contenuto non ci sta che questa disposizione
    ''' fa qualcosa di diverso.</para>
    ''' </remarks>
    Public Sub DisponiIn(altezzaDisponibile As Integer)

        ' La larghezza di progetto in pixel veri: dichiararla cruda stringeva la finestra
        ' di un terzo mentre i testi dentro crescevano col DPI, e a mandare a capo il
        ' doppio delle righe era proprio questo (decisione 15.7).
        Dim larghezza As Integer = ScalaSchermo.InPixelDelloSchermo(LarghezzaFinestra, Me.DeviceDpi)

        Dim voluta As Integer = MettiInFila(larghezza)
        Dim siScorre As Boolean = ScalaSchermo.ServeScorrimento(voluta, altezzaDisponibile)

        If siScorre Then
            voluta = MettiInFila(ScalaSchermo.LarghezzaSenzaLaBarra(
                larghezza, siScorre, SystemInformation.VerticalScrollBarWidth))
        End If

        Me.AutoScroll = siScorre
        ClientSize = New Size(larghezza, ScalaSchermo.AltezzaSostenibile(voluta, altezzaDisponibile))

    End Sub

    ''' <summary>
    ''' Mette i controlli in colonna dentro questa larghezza, e dice fin dove arrivano. Il
    ''' conto lo fa il codice e non il designer, perché i testi qui dentro cambiano
    ''' lunghezza con quel che c'è nella cartella dati.
    ''' </summary>
    Private Function MettiInFila(larghezza As Integer) As Integer

        Dim sinistra As Integer = StileApp.MargineRiquadro
        Dim larghezzaUtile As Integer = larghezza - 2 * StileApp.MargineRiquadro

        For Each testo As Label In {lblSpiegazione, lblStatoChiave, lblRifinituraNota,
                                    lblFollowUpNota, lblCartellaDati, lblCartellaDocumenti,
                                    lblModelli, lblPool, lblStato}
            testo.MaximumSize = New Size(larghezzaUtile, 0)
        Next

        lblTitolo.Location = New Point(sinistra, StileApp.MargineRiquadro)
        lblSpiegazione.Location = New Point(sinistra, lblTitolo.Bottom + StileApp.DistanzaControlli)

        lblSezioneChiave.Location = New Point(sinistra, lblSpiegazione.Bottom + StileApp.MargineRiquadro)
        lblStatoChiave.Location = New Point(sinistra, lblSezioneChiave.Bottom + StileApp.InterlineaMinima)
        btnCambiaChiave.Location = New Point(sinistra, lblStatoChiave.Bottom + StileApp.InterlineaMinima)

        lblSezioneDocumenti.Location = New Point(sinistra, btnCambiaChiave.Bottom + StileApp.MargineRiquadro)
        lblLingua.Location = New Point(sinistra, lblSezioneDocumenti.Bottom + StileApp.InterlineaMinima + 4)
        cmbLingua.Location = New Point(lblLingua.Right + StileApp.InterlineaMinima,
                                       lblSezioneDocumenti.Bottom + StileApp.InterlineaMinima)
        chkRifinitura.Location = New Point(sinistra, cmbLingua.Bottom + StileApp.DistanzaControlli)
        lblRifinituraNota.Location = New Point(sinistra, chkRifinitura.Bottom + StileApp.InterlineaMinima)

        lblSezioneCandidature.Location = New Point(sinistra, lblRifinituraNota.Bottom + StileApp.MargineRiquadro)
        lblFollowUp.Location = New Point(sinistra, lblSezioneCandidature.Bottom + StileApp.InterlineaMinima + 4)
        numFollowUp.Location = New Point(lblFollowUp.Right + StileApp.InterlineaMinima,
                                         lblSezioneCandidature.Bottom + StileApp.InterlineaMinima)
        lblGiorni.Location = New Point(numFollowUp.Right + StileApp.InterlineaMinima,
                                       lblFollowUp.Top)
        lblFollowUpNota.Location = New Point(sinistra, numFollowUp.Bottom + StileApp.InterlineaMinima)

        lblSezioneCartelle.Location = New Point(sinistra, lblFollowUpNota.Bottom + StileApp.MargineRiquadro)
        lblCartellaDati.Location = New Point(sinistra, lblSezioneCartelle.Bottom + StileApp.InterlineaMinima)
        btnApriCartellaDati.Location = New Point(sinistra, lblCartellaDati.Bottom + StileApp.InterlineaMinima)
        lblCartellaDocumenti.Location = New Point(sinistra, btnApriCartellaDati.Bottom + StileApp.DistanzaControlli)
        btnGestisciDocumenti.Location = New Point(sinistra, lblCartellaDocumenti.Bottom + StileApp.InterlineaMinima)

        lblSezioneMotore.Location = New Point(sinistra, btnGestisciDocumenti.Bottom + StileApp.MargineRiquadro)
        lblModelli.Location = New Point(sinistra, lblSezioneMotore.Bottom + StileApp.InterlineaMinima)
        lblPool.Location = New Point(sinistra, lblModelli.Bottom + StileApp.InterlineaMinima)
        btnApriModelli.Location = New Point(sinistra, lblPool.Bottom + StileApp.InterlineaMinima)

        lblSezioneDati.Location = New Point(sinistra, btnApriModelli.Bottom + StileApp.MargineRiquadro)
        btnBackup.Location = New Point(sinistra, lblSezioneDati.Bottom + StileApp.InterlineaMinima)
        btnSvuotaNavigazione.Location = New Point(sinistra, btnBackup.Bottom + StileApp.DistanzaControlli)

        ' L'azione che non si disfa sta lontana dalle altre: è una difesa, non una
        ' spaziatura (cap. 11.5, la stessa regola della fascia dei comandi).
        btnEliminaTutto.Location = New Point(sinistra,
                                             btnSvuotaNavigazione.Bottom + FasciaDeiComandi.StaccoDelCritico)

        lblStato.Location = New Point(sinistra, btnEliminaTutto.Bottom + StileApp.MargineRiquadro)

        btnChiudi.Location = New Point(larghezza - StileApp.MargineRiquadro - btnChiudi.Width,
                                       lblStato.Bottom + StileApp.MargineRiquadro)

        Return btnChiudi.Bottom + StileApp.MargineRiquadro

    End Function

End Class
