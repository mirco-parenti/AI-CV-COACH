Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Text.Json
Imports System.Threading.Tasks
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
''' ci sta lavorando. Il <i>pool</i> si legge e basta: si sigilla dal repo, non da un
''' eseguibile distribuito (cap. 04.5). La <i>taratura</i> non compare affatto, che è la
''' sua regola da sempre.</para>
''' <para><b>I modelli invece si scelgono</b> <i>(2026-08-27, dalla revisione del giro
''' D)</i>. Due tendine, una per livello, e la scelta finisce in <c>modelli.json</c> —
''' che resta il posto dove vive, perché cambiare modello continui a costare una riga
''' anche senza aprire le Impostazioni (cap. 11.6). Fino alla 1.0 quel file si poteva
''' solo aprire a mano: bastava a me, non a chi il programma non l'ha scritto. Le tendine
''' <b>non</b> mostrano l'interruttore del ragionamento esteso, che resta nel file: è la
''' manopola di chi sa cosa sta facendo, e chi la cerca sa già dove guardare.</para>
''' </remarks>
Public Class FinestraImpostazioni

    ''' <summary>Quanto è larga la finestra, e quindi il testo che ci sta dentro.</summary>
    Private Const LarghezzaFinestra As Integer = 660

    Private ReadOnly _contesto As ContestoApp
    Private ReadOnly _pulizia As PuliziaDati

    ''' <summary>
    ''' Chi va a chiedere all'API quali modelli esistono. È una porta e non una chiamata
    ''' diretta per la stessa ragione della prova della chiave: il banco ci mette un finto
    ''' e collauda le tendine senza rete (cap. 14).
    ''' </summary>
    Private ReadOnly _elenco As Func(Of Task(Of Ai.EsitoElenco))

    ''' <summary>I modelli che l'API dichiara disponibili; <c>Nothing</c> finché non arrivano.</summary>
    Private _disponibili As IReadOnlyList(Of Ai.ModelloDisponibile)

    ''' <summary>Perché l'elenco vero non c'è; stringa vuota quando c'è o quando si sta chiedendo.</summary>
    Private _perche As String = String.Empty

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
    ''' <param name="elenco">
    ''' Chi chiede all'API l'elenco dei modelli; se omesso lo chiede davvero, con la
    ''' chiave salvata. Il banco ce ne mette uno finto.
    ''' </param>
    Public Sub New(contesto As ContestoApp, Optional elenco As Func(Of Task(Of Ai.EsitoElenco)) = Nothing)

        InitializeComponent()

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
        _contesto = contesto
        _pulizia = New PuliziaDati(contesto.Cartella)
        _elenco = If(elenco, AddressOf contesto.ModelliDisponibiliAsync)

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
        RaccontaIlConsumo()
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

    Private Sub btnComeFunziona_Click(sender As Object, e As EventArgs) Handles btnComeFunziona.Click
        FinestraInformativa.Mostra(Me)
    End Sub

    Private Sub btnCambiaChiave_Click(sender As Object, e As EventArgs) Handles btnCambiaChiave.Click

        Dim illeggibile As Boolean
        Dim digitata As String = FinestraChiaveApi.Chiedi(Me, _contesto.Segreti.LeggiChiaveApi(illeggibile),
                                                     Function(daProvare As String) Ai.ProvaChiave.ProvaAsync(daProvare))
        If digitata Is Nothing Then Return

        Try
            _contesto.Segreti.SalvaChiaveApi(digitata)
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            RaccontaUnErrore($"La chiave non si è potuta salvare ({ex.Message}): vale per questa sessione.")
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
            RaccontaUnErrore($"Le preferenze non si sono potute scrivere ({ex.Message}). " &
                             "Valgono per questa sessione, ma al prossimo avvio saranno quelle di prima.")
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
            RaccontaUnErrore($"La cartella non si è lasciata aprire ({ex.Message}).")
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
    ' Sotto il cofano: i modelli si scelgono, il pool si legge
    ' ==================================================================

    Private Sub RaccontaIlMotore()

        RiempiLeTendine()
        RaccontaDaDoveVengono()

    End Sub

    ''' <summary>
    ''' Le due righe sotto le tendine: dove vive la scelta, e da dove viene l'elenco.
    ''' </summary>
    ''' <remarks>
    ''' Sta staccata dal riempimento, e non per ordine: chi ha appena scelto un modello
    ''' deve aggiornare <b>queste righe</b> e non le tendine. Rifare le tendine da dentro
    ''' l'evento di una tendina è un anello chiuso — riempire fa scattare l'evento, che
    ''' riempie — e lo si è visto per davvero, falsificando le due guardie insieme: non
    ''' un file scritto per sbaglio, ma una ricorsione senza fondo. Meglio non avere il
    ''' ciclo che sorvegliarlo.
    ''' </remarks>
    Private Sub RaccontaDaDoveVengono()

        Dim dove As String = If(_contesto.Modelli.Origine = Ai.OrigineModelli.File,
                                "La scelta è scritta in modelli.json, nella cartella dati.",
                                "Nessuno li ha ancora scelti: valgono i predefiniti, e la prima scelta scriverà modelli.json.")

        lblModelli.Text = dove & vbLf & ProvenienzaDellElenco()

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

    ''' <summary>Da dove viene l'elenco che le tendine offrono, detto a chi guarda.</summary>
    Private Function ProvenienzaDellElenco() As String

        If _disponibili IsNot Nothing Then Return "Nelle tendine ci sono i modelli che l'AI dichiara oggi."
        If _perche.Length > 0 Then Return $"Nelle tendine ci sono solo i modelli che conosco: {_perche}."

        Return "Sto chiedendo all'AI quali modelli ci sono…"

    End Function

    ''' <summary>
    ''' Chiede l'elenco dei modelli e rifà le tendine con quel che è arrivato.
    ''' </summary>
    ''' <remarks>
    ''' <para>Sta in un metodo suo, staccato dall'evento della finestra, per la ragione di
    ''' sempre: di una finestra modale il banco non può aspettare la chiusura, e questo è
    ''' l'unico modo di collaudarlo senza mostrarla (cap. 14).</para>
    ''' <para><b>Le tendine non restano spente ad aspettare.</b> Nascono subito coi
    ''' modelli che il programma conosce da sé, e l'elenco vero le rifà quando arriva:
    ''' senza rete si sceglie lo stesso fra quelli, e con la rete lenta non c'è una
    ''' finestra bloccata a fissare il vuoto. Il prezzo è che l'elenco può allungarsi un
    ''' istante dopo l'apertura, ed è un prezzo piccolo — la scelta di prima resta
    ''' selezionata.</para>
    ''' </remarks>
    Public Async Function AggiornaLElencoDeiModelli() As Task

        Dim esito As Ai.EsitoElenco = Nothing

        Try
            esito = Await _elenco().ConfigureAwait(True)
        Catch ex As Exception
            ' Un elenco che non arriva non è un guasto del programma: è un esito, e la
            ' finestra funziona lo stesso. Chi guarda merita una riga, non un errore.
            Dati.DiarioTecnico.Corrente?.AnnotaGuasto("l'elenco dei modelli disponibili", ex)
        End Try

        ' Nel frattempo la finestra può essere stata chiusa: toccare i controlli di una
        ' finestra smaltita solleverebbe, e per giunta in un punto che nessuno guarda.
        If IsDisposed Then Return

        If esito IsNot Nothing AndAlso esito.Riuscita Then
            _disponibili = esito.Modelli
            _perche = String.Empty
        Else
            _disponibili = Nothing
            _perche = Ai.ElencoModelli.Perche(If(esito Is Nothing, Ai.CausaErroreAi.RispostaInattesa, esito.Causa))
        End If

        RaccontaIlMotore()
        Disponi()

    End Function

    ''' <summary>Rifà le due tendine, tenendo scelto quel che è in vigore.</summary>
    Private Sub RiempiLeTendine()

        Dim offerti As IReadOnlyList(Of Ai.ModelloDisponibile) =
            If(_disponibili, Ai.ElencoModelli.Conosciuti(_contesto.Modelli))

        ' Riempire una tendina fa scattare il suo evento: senza questa guardia, aprire le
        ' Impostazioni scriverebbe modelli.json due volte senza che nessuno abbia scelto
        ' niente. Si ripristina invece di azzerare, perché qui si passa anche durante la
        ' costruzione, quando la guardia è già alzata per tutti.
        Dim guardiaDiPrima As Boolean = _sto
        _sto = True

        Try
            Riempi(cmbModelloRagionamento, offerti, _contesto.Modelli.ModelloRagionamento.Id)
            Riempi(cmbModelloSemplice, offerti, _contesto.Modelli.ModelloSemplice.Id)
        Finally
            _sto = guardiaDiPrima
        End Try

    End Sub

    ''' <summary>Una tendina con dentro questi modelli, e scelto quello in uso.</summary>
    Private Shared Sub Riempi(tendina As ComboBox, offerti As IReadOnlyList(Of Ai.ModelloDisponibile),
                              idInUso As String)

        Dim voci As IReadOnlyList(Of Ai.ModelloDisponibile) = Ai.ElencoModelli.ConQuelloInUso(offerti, idInUso)

        tendina.Items.Clear()
        For Each voce As Ai.ModelloDisponibile In voci
            tendina.Items.Add(voce)
        Next

        ' Si sceglie per modello e non per identificativo: quello in uso è l'alias
        ' (claude-haiku-4-5), quello elencato dall'API è la sua versione datata
        ' (claude-haiku-4-5-20251001), e cercando l'uguaglianza esatta la tendina restava
        ' senza niente di scelto proprio sul modello di casa (2026-08-27).
        For posto As Integer = 0 To voci.Count - 1
            If Ai.IdModello.StessoModello(voci(posto).Id, idInUso) Then
                tendina.SelectedIndex = posto
                Return
            End If
        Next

    End Sub

    Private Sub cmbModelloRagionamento_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbModelloRagionamento.SelectedIndexChanged
        CambiaIlModello(Ai.Modelli.Ragionamento, cmbModelloRagionamento, "il ragionamento")
    End Sub

    Private Sub cmbModelloSemplice_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbModelloSemplice.SelectedIndexChanged
        CambiaIlModello(Ai.Modelli.Semplice, cmbModelloSemplice, "le elaborazioni testuali")
    End Sub

    ''' <summary>
    ''' Porta la scelta nel file e in vigore. Come ogni preferenza di questa finestra si
    ''' salva da sé, appena si cambia.
    ''' </summary>
    Private Sub CambiaIlModello(livello As String, tendina As ComboBox, perChe As String)

        If _sto Then Return

        Dim scelto As Ai.ModelloDisponibile = TryCast(tendina.SelectedItem, Ai.ModelloDisponibile)
        If scelto Is Nothing Then Return

        Try
            _contesto.Modelli.CambiaModello(livello, scelto.Id, _contesto.Cartella.FileModelli)

        Catch ex As JsonException
            RiempiLeTendine()
            RaccontaUnErrore("Il file modelli.json c'è ma non si lascia leggere: aprilo e correggilo " &
                             "(o cancellalo, e torneranno i predefiniti). La scelta non è stata cambiata.")
            Return

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' La tendina torna su quel che vale davvero: lasciarla sul modello nuovo
            ' direbbe che il cambio è avvenuto, e non è avvenuto né qui né sul disco.
            RiempiLeTendine()
            RaccontaUnErrore($"La scelta non si è potuta salvare ({ex.Message}): resta quella di prima.")
            Return
        End Try

        RaccontaDaDoveVengono()
        Disponi()
        Racconta($"D'ora in poi {perChe} usa {scelto.Id}: vale già dalla prossima chiamata all'AI.",
                 StileApp.TestoSecondario)

    End Sub

    Private Async Sub Mostrata(mittente As Object, e As EventArgs) Handles Me.Shown
        Await AggiornaLElencoDeiModelli()
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
            RaccontaUnErrore($"Non si è lasciato aprire ({ex.Message}).")
        End Try

    End Sub

    ' ==================================================================
    ' Quanto è costato (2026-08-27, dalla revisione del giro D)
    ' ==================================================================

    ''' <summary>
    ''' Il conto dell'uso dell'AI, letto da <c>chiamate_ai.csv</c>.
    ''' </summary>
    ''' <remarks>
    ''' Sta qui e non sotto gli occhi mentre si lavora: chi vuole sapere viene a vedere,
    ''' chi lavora non ha un contatore che gli gira davanti — che è il modo migliore di
    ''' rendere ansiogeno un programma che costa pochi centesimi al giro.
    ''' </remarks>
    Private Sub RaccontaIlConsumo()

        Dim conto As ContoDoppio = ContoDelleChiamate.Leggi(
            _contesto.Cartella.FileChiamateAi, _contesto.Modelli.Prezzi, Date.Now)

        btnApriChiamate.Enabled = conto.Tutte.CEQualcosa
        lblConsumo.Text = InParole(conto)

    End Sub

    ''' <summary>Il conto in una manciata di righe. Sta fuori dalla finestra per collaudarlo.</summary>
    Public Shared Function InParole(conto As ContoDoppio) As String

        If conto Is Nothing OrElse Not conto.Tutte.CEQualcosa Then
            Return "Nessuna chiamata all'AI, per ora: qui comparirà quanto costa usarlo."
        End If

        Dim righe As New List(Of String) From {
            $"Da sempre: {Chiamate(conto.Tutte)}, {Token(conto.Tutte)}, {Spesa(conto.Tutte)}." &
            If(conto.Tutte.DalGiorno.HasValue, $" La prima il {conto.Tutte.DalGiorno.Value:d MMMM yyyy}.", ""),
            $"Ultimi {conto.GiorniRecenti} giorni: {Chiamate(conto.Recenti)}, {Spesa(conto.Recenti)}."}

        ' Il buco si dichiara: un totale che tace su una parte delle chiamate sembra
        ' completo, ed è il modo più educato di dire una cifra sbagliata.
        If conto.Tutte.SenzaPrezzo > 0 Then
            righe.Add($"Di {conto.Tutte.SenzaPrezzo} chiamate non conosco il prezzo del modello: " &
                      "i loro token sono contati, i loro soldi no.")
        End If

        righe.Add("È una stima ai prezzi di listino: non sa di sconti né della cache dei " &
                  "prompt, e la verità resta la fattura di Anthropic.")

        Return String.Join(vbLf, righe)

    End Function

    Private Shared Function Chiamate(conto As Conto) As String
        Return If(conto.Chiamate = 1, "1 chiamata", $"{conto.Chiamate:N0} chiamate")
    End Function

    Private Shared Function Token(conto As Conto) As String
        Return $"{conto.TokenIngresso + conto.TokenUscita:N0} token"
    End Function

    Private Shared Function Spesa(conto As Conto) As String

        ' Sotto il centesimo non si scrive «$0,00», che si legge come «gratis»: si dice
        ' che è meno di un centesimo, che è la cosa vera.
        If conto.Chiamate > conto.SenzaPrezzo AndAlso conto.Spesa < 0.01D Then Return "meno di $0,01"

        Return $"≈ ${conto.Spesa:N2}"

    End Function

    Private Sub btnApriChiamate_Click(sender As Object, e As EventArgs) Handles btnApriChiamate.Click

        Dim percorso As String = _contesto.Cartella.FileChiamateAi

        Try
            Process.Start(New ProcessStartInfo(percorso) With {.UseShellExecute = True})
            Racconta("Ogni riga è una chiamata: si apre in un foglio di calcolo e si ordina per colonna.",
                     StileApp.TestoSecondario)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is System.ComponentModel.Win32Exception
            RaccontaUnErrore($"Non si è lasciato aprire ({ex.Message}).")
        End Try

    End Sub

    ' ==================================================================
    ' I tuoi dati
    ' ==================================================================

    Private Sub RaccontaCosaSiPuoPulire()

        btnBackup.Enabled = True
        btnSvuotaNavigazione.Enabled = Directory.Exists(_contesto.Cartella.CartellaWebView2)
        btnEliminaTutto.Enabled = _pulizia.CEQualcosa

    End Sub

    ''' <summary>
    ''' Chiude i tre comandi della sezione «I tuoi dati» mentre una pulizia è in corso.
    ''' </summary>
    ''' <remarks>
    ''' Riaprirli non tocca a questo metodo ma a <see cref="RaccontaCosaSiPuoPulire"/>, che
    ''' li riaccende <b>secondo quel che è rimasto</b>: dopo un'eliminazione riuscita non
    ''' c'è più niente da eliminare, e riaccendere alla cieca il bottone rosso direbbe il
    ''' contrario. Sono tre e non due perché lavorano sugli stessi file: un backup avviato
    ''' in mezzo a un'eliminazione leggerebbe una cartella che sta sparendo.
    ''' </remarks>
    Private Sub ChiudiIComandiDeiDati()

        btnBackup.Enabled = False
        btnSvuotaNavigazione.Enabled = False
        btnEliminaTutto.Enabled = False

    End Sub

    Private Sub btnBackup_Click(sender As Object, e As EventArgs) Handles btnBackup.Click

        Using finestra As New FinestraBackup(_contesto)
            finestra.ShowDialog(Me)
        End Using

        ' Un ripristino cambia quel che c'è nella cartella: le pulizie ne tengono conto.
        RaccontaCosaSiPuoPulire()

    End Sub

    ''' <remarks>
    ''' <b>La cancellazione va su un altro filo.</b> La cartella del browser sono migliaia
    ''' di file piccoli, e cancellarli sul thread dell'interfaccia vuol dire una finestra
    ''' che smette di rispondere proprio mentre dice di stare lavorando — con Windows che
    ''' le sbianca sopra il suo «non risponde». Qui restano i comandi chiusi e la riga che
    ''' racconta, che è il patto delle attese di questo programma (cap. 03.8).
    ''' </remarks>
    Private Async Sub btnSvuotaNavigazione_Click(sender As Object, e As EventArgs) Handles btnSvuotaNavigazione.Click

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

        ChiudiIComandiDeiDati()
        Racconta("Sto svuotando i dati di navigazione…", StileApp.TestoSecondario)

        Dim detto As String
        Dim andataStorta As Boolean = False

        Try
            detto = If(Await Task.Run(Function() _pulizia.SvuotaNavigazione()).ConfigureAwait(True),
                       "Dati di navigazione svuotati.",
                       "Non c'era niente da svuotare.")

        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            ' Il browser incorporato tiene i suoi file aperti finché P3 è vivo.
            detto = $"Non si sono lasciati cancellare tutti ({ex.Message}): " &
                    "chiudi la ricerca annunci e riprova."
            andataStorta = True
        End Try

        ' Nel frattempo la finestra può essere stata chiusa: toccare i controlli di una
        ' finestra smaltita solleverebbe, e per giunta in un punto che nessuno guarda.
        If IsDisposed Then Return

        ' Com'è andata si porta dietro il modo di dirlo: qui non si sceglie più un colore,
        ' si sceglie fra due voci — e la parola e il colore viaggiano insieme.
        If andataStorta Then
            RaccontaUnErrore(detto)
        Else
            Racconta(detto, StileApp.TestoSecondario)
        End If

        RaccontaCosaSiPuoPulire()

    End Sub

    ''' <remarks>
    ''' Come lo svuotamento qui sopra, e con più ragione: qui sparisce l'intera cartella
    ''' dati — profilo, storico, candidature con i loro documenti, backup. Il mestiere va
    ''' su un altro filo, e la finestra resta viva a dirlo.
    ''' </remarks>
    Private Async Sub btnEliminaTutto_Click(sender As Object, e As EventArgs) Handles btnEliminaTutto.Click

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

        ChiudiIComandiDeiDati()
        Racconta("Sto eliminando i tuoi dati…", StileApp.TestoSecondario)

        Dim andate As Integer
        Dim guasto As Exception = Nothing

        Try
            andate = Await Task.Run(Function() _pulizia.EliminaTutto()).ConfigureAwait(True)

        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
            guasto = ex
        End Try

        ' Nel frattempo la finestra può essere stata chiusa: toccare i controlli di una
        ' finestra smaltita solleverebbe, e per giunta in un punto che nessuno guarda.
        If IsDisposed Then Return

        If guasto IsNot Nothing Then
            RaccontaUnErrore($"Non si è potuto eliminare tutto ({guasto.Message}). " &
                             "Qualcosa è ancora aperto: chiudi le altre finestre e riprova.")
            RaccontaCosaSiPuoPulire()
            Return
        End If

        _DatiEliminati = True

        MessageBox.Show(
            Me,
            $"Fatto: {andate} voci eliminate." & vbLf & vbLf &
            "Chiudo l'applicazione: da qui in poi lavorerebbe su file che non ci sono più. " &
            "Riaprendola, ricomincia come il primo giorno.",
            "Dati eliminati", MessageBoxButtons.OK, MessageBoxIcon.Information)

        DialogResult = DialogResult.OK
        Close()

    End Sub

    ' ==================================================================
    ' Contorno
    ' ==================================================================

    ''' <summary>Una riga di stato in fondo, come negli altri pannelli (cap. 03.8).</summary>
    Private Sub Racconta(testo As String, colore As Color)

        lblStato.Text = testo
        lblStato.ForeColor = colore

    End Sub

    ''' <summary>
    ''' Una riga che dice che qualcosa non è riuscito: la parola e il colore insieme
    ''' (v. <see cref="Segnalazioni"/>).
    ''' </summary>
    ''' <remarks>
    ''' Fino al 2026-09-01 queste righe erano <c>RossoTitoli</c>, che è il rosso del
    ''' <b>marchio</b>: nato per i titoli grandi, come testo piccolo non arriva alla soglia
    ''' di leggibilità — e comunque diceva «guasto» col solo colore.
    ''' </remarks>
    Private Sub RaccontaUnErrore(testo As String)

        Racconta(Segnalazioni.PrefissoErrore & testo, StileApp.Pericolo)

    End Sub

    Private Sub btnChiudi_Click(sender As Object, e As EventArgs) Handles btnChiudi.Click
        Close()
    End Sub

    ''' <summary>I colori e i font della finestra, tutti da <see cref="StileApp"/> (cap. 03.2).</summary>
    ''' <remarks>
    ''' Il fondo è <see cref="StileApp.FondoCasella"/> e non il bianco delle altre
    ''' finestre: dal 2026-08-31 l'avorio della soglia è entrato nelle pagine, e questa —
    ''' che è la settima porta del menu, anche se si apre in una finestra invece che in un
    ''' pannello — sarebbe stata l'unica destinazione a restare bianca. Le altre finestre
    ''' non la seguono: quelle sono momenti (una conferma, un backup, una chiave da
    ''' scrivere), non posti dove si sta.
    ''' </remarks>
    Private Sub Vesti()

        BackColor = StileApp.FondoCasella
        Font = StileApp.FontTesto

        ' Un filo sopra la fascia: senza, il testo che le scorre sotto sembra tagliato a
        ' metà da niente, invece che passare dietro a qualcosa (2026-08-27).
        pnlContenuto.BackColor = StileApp.FondoCasella
        pnlFascia.BackColor = StileApp.FondoCasella
        AddHandler pnlFascia.Paint,
            Sub(mittente As Object, disegno As PaintEventArgs)
                Using filo As New Pen(StileApp.BordoLeggero)
                    disegno.Graphics.DrawLine(filo, 0, 0, pnlFascia.Width, 0)
                End Using
            End Sub

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli

        For Each sezione As Label In {lblSezioneChiave, lblSezioneDocumenti, lblSezioneCandidature,
                                      lblSezioneCartelle, lblSezioneMotore, lblSezioneConsumo,
                                      lblSezioneDati}
            sezione.Font = StileApp.FontTitoloGruppo
            sezione.ForeColor = StileApp.TestoPrimario
        Next

        For Each testo As Label In {lblSpiegazione, lblStatoChiave, lblLingua, lblFollowUp,
                                    lblGiorni, lblCartellaDati, lblCartellaDocumenti,
                                    lblModelloRagionamento, lblModelloSemplice}
            testo.ForeColor = StileApp.TestoPrimario
        Next

        For Each minore As Label In {lblRifinituraNota, lblFollowUpNota, lblModelli, lblPool,
                                     lblConsumo}
            minore.ForeColor = StileApp.TestoSecondario
        Next

        chkRifinitura.ForeColor = StileApp.TestoPrimario
        chkRifinitura.BackColor = StileApp.FondoCasella

        For Each tendina As ComboBox In {cmbLingua, cmbModelloRagionamento, cmbModelloSemplice}
            tendina.BackColor = StileApp.SfondoContenuto
            tendina.ForeColor = StileApp.TestoPrimario
        Next

        numFollowUp.BackColor = StileApp.SfondoContenuto
        numFollowUp.ForeColor = StileApp.TestoPrimario

        StileApp.VestiBottone(btnComeFunziona, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnCambiaChiave, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnApriCartellaDati, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnGestisciDocumenti, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnApriModelli, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnApriChiamate, LivelloBottone.Esplorativo)
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

        ' La fascia si misura sul bottone che porta: a DPI alti cresce con lui.
        pnlFascia.Height = 2 * StileApp.MargineRiquadro + btnChiudi.Height

        Dim voluta As Integer = MettiInFila(larghezza) + pnlFascia.Height
        Dim siScorre As Boolean = ScalaSchermo.ServeScorrimento(voluta, altezzaDisponibile)

        If siScorre Then
            voluta = MettiInFila(ScalaSchermo.LarghezzaSenzaLaBarra(
                larghezza, siScorre, SystemInformation.VerticalScrollBarWidth)) + pnlFascia.Height
        End If

        ' Scorre il contenuto, non la finestra: la fascia in fondo deve restare dov'è
        ' anche quando sopra di lei si scorre (2026-08-27).
        pnlContenuto.AutoScroll = siScorre
        ClientSize = New Size(larghezza, ScalaSchermo.AltezzaSostenibile(voluta, altezzaDisponibile))

        ' La barra di scorrimento vive dentro il contenuto e non tocca la fascia: qui
        ' «Chiudi» sta al suo margine, sempre.
        btnChiudi.Location = New Point(larghezza - StileApp.MargineRiquadro - btnChiudi.Width,
                                       StileApp.MargineRiquadro)

    End Sub

    ''' <summary>
    ''' Vero quando il contenuto non ci sta e scorre. Pubblica per il banco: adesso a
    ''' scorrere è il pannello di dentro, e da fuori non si vedrebbe più.
    ''' </summary>
    Public ReadOnly Property SiScorre As Boolean
        Get
            Return pnlContenuto.AutoScroll
        End Get
    End Property

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
                                    lblModelloRagionamento, lblModelloSemplice,
                                    lblModelli, lblPool, lblConsumo, lblStato}
            testo.MaximumSize = New Size(larghezzaUtile, 0)
        Next

        ' Le tendine dei modelli si prendono tutta la larghezza: un identificativo con
        ' accanto il nome leggibile non sta in una casella da 180 pixel, e una tendina che
        ' tronca proprio l'id è una tendina che nasconde la cosa che conta.
        For Each tendina As ComboBox In {cmbModelloRagionamento, cmbModelloSemplice}
            tendina.Width = larghezzaUtile
        Next

        lblTitolo.Location = New Point(sinistra, StileApp.MargineRiquadro)
        lblSpiegazione.Location = New Point(sinistra, lblTitolo.Bottom + StileApp.DistanzaControlli)

        btnComeFunziona.Location = New Point(sinistra, lblSpiegazione.Bottom + StileApp.DistanzaControlli)

        lblSezioneChiave.Location = New Point(sinistra, btnComeFunziona.Bottom + StileApp.MargineRiquadro)
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

        ' Etichetta sopra e tendina sotto, e non affiancate come la lingua: qui
        ' l'etichetta è una frase e il valore un identificativo lungo, e in fila
        ' finirebbero fuori dalla larghezza di progetto proprio a DPI alti.
        lblModelloRagionamento.Location = New Point(sinistra, lblSezioneMotore.Bottom + StileApp.InterlineaMinima)
        cmbModelloRagionamento.Location = New Point(sinistra, lblModelloRagionamento.Bottom + StileApp.InterlineaMinima)
        lblModelloSemplice.Location = New Point(sinistra, cmbModelloRagionamento.Bottom + StileApp.DistanzaControlli)
        cmbModelloSemplice.Location = New Point(sinistra, lblModelloSemplice.Bottom + StileApp.InterlineaMinima)

        lblModelli.Location = New Point(sinistra, cmbModelloSemplice.Bottom + StileApp.InterlineaMinima)
        lblPool.Location = New Point(sinistra, lblModelli.Bottom + StileApp.InterlineaMinima)
        btnApriModelli.Location = New Point(sinistra, lblPool.Bottom + StileApp.InterlineaMinima)

        lblSezioneConsumo.Location = New Point(sinistra, btnApriModelli.Bottom + StileApp.MargineRiquadro)
        lblConsumo.Location = New Point(sinistra, lblSezioneConsumo.Bottom + StileApp.InterlineaMinima)
        btnApriChiamate.Location = New Point(sinistra, lblConsumo.Bottom + StileApp.InterlineaMinima)

        lblSezioneDati.Location = New Point(sinistra, btnApriChiamate.Bottom + StileApp.MargineRiquadro)
        btnBackup.Location = New Point(sinistra, lblSezioneDati.Bottom + StileApp.InterlineaMinima)
        btnSvuotaNavigazione.Location = New Point(sinistra, btnBackup.Bottom + StileApp.DistanzaControlli)

        ' L'azione che non si disfa sta lontana dalle altre: è una difesa, non una
        ' spaziatura (cap. 11.5, la stessa regola della fascia dei comandi). Lì un comando
        ' critico prende una riga tutta sua, staccata dal resto e allineata dall'altra
        ' parte, «così non finisce mai sotto il dito di chi sta premendo il comando
        ' accanto»; qui, fino al 2026-09-01, di quella regola c'era solo il vuoto sopra — e
        ' il vuoto sopra non basta quando il bottone sta nella stessa colonna, alla stessa
        ' larghezza e appena sotto un altro bottone rosso. Adesso ha il vuoto da tutti e due
        ' i lati e sta al margine opposto: un clic scivolato sotto «Svuota i dati di
        ' navigazione» trova il fondo della finestra, non l'eliminazione di tutto.
        btnEliminaTutto.Location = New Point(larghezza - StileApp.MargineRiquadro - btnEliminaTutto.Width,
                                             btnSvuotaNavigazione.Bottom + FasciaDeiComandi.StaccoDelCritico)

        lblStato.Location = New Point(sinistra, btnEliminaTutto.Bottom + FasciaDeiComandi.StaccoDelCritico)

        ' «Chiudi» non sta più in coda al contenuto: vive nella fascia, che non scorre.
        Return lblStato.Bottom + StileApp.MargineRiquadro

    End Function

End Class
