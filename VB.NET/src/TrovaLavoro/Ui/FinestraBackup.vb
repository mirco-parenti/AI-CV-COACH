Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Windows.Forms
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

''' <summary>
''' La finestra di backup e ripristino, la funzione F7 (cap. 11.4). Si apre dal pannello
''' del profilo e tiene insieme le due metà della stessa funzione: portare via i propri
''' dati, e rimetterli al loro posto.
''' </summary>
''' <remarks>
''' <para><b>Le due metà stanno in una finestra sola</b> perché sono lo stesso gesto visto
''' nei due versi, e chi cerca «come si ripristina» lo cerca dove ha esportato. Erano due
''' bottoni possibili nella fascia di P2, ma il ripristino ha bisogno di far leggere
''' <i>cosa sovrascrive</i> prima di toccare qualcosa, e quello un bottone non lo sa fare.</para>
''' <para><b>L'anteprima non è una formalità</b> (cap. 11.4, passo 2): finché non si sceglie
''' un file, «Ripristina» resta spento; quando lo si sceglie, si legge cosa contiene e cosa
''' prende il posto di cosa. Solo dopo si conferma — e la conferma parte da «no».</para>
''' <para><b>Il lucchetto non si tocca qui</b>: l'applicazione con le finestre lo tiene per
''' tutta la sessione (cap. 09.4), quindi mentre questa finestra è aperta nessun altro
''' processo sta scrivendo negli stessi file.</para>
''' </remarks>
Public Class FinestraBackup

    ''' <summary>Quanto è larga la finestra, e quindi il testo che ci sta dentro.</summary>
    Private Const LarghezzaFinestra As Integer = 660

    Private ReadOnly _contesto As ContestoApp

    ''' <summary>Il backup scelto per il ripristino; <c>Nothing</c> finché non se ne apre uno.</summary>
    Private _letto As Dati.Backup

    ''' <summary>
    ''' Se il profilo su disco è cambiato per mano di questa finestra: chi l'ha aperta
    ''' deve rileggerlo, o continuerebbe a mostrare quello di prima.
    ''' </summary>
    Public ReadOnly Property ProfiloRipristinato As Boolean = False

    ''' <summary>
    ''' Prepara la finestra. È pubblica perché il banco la costruisce e la interroga
    ''' senza mostrarla: di una finestra modale non si può aspettare la chiusura.
    ''' </summary>
    Public Sub New(contesto As ContestoApp)

        InitializeComponent()

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
        _contesto = contesto

        lblSpiegazione.Text =
            "Un backup è un solo file .json, leggibile anche senza questo programma. " &
            "Dentro non ci finiscono la chiave API né i documenti già impaginati: quelli sono " &
            "file normali e si copiano da sé." & vbLf &
            "Tienilo dove vuoi — anche su una chiavetta: è il senso di averlo."

        lblComeSiRipristina.Text =
            "Scegli un file di backup: prima ti dico cosa contiene e cosa sostituisce, e solo " &
            "dopo la tua conferma tocco qualcosa. Il profilo di adesso finisce comunque nello " &
            "storico, e le candidature che il backup non nomina restano dove sono."

        RaccontaCosaCE()
        Vesti()
        Disponi()

        CancelButton = btnChiudi
        AcceptButton = Nothing

    End Sub

    ' ==================================================================
    ' Esportare
    ' ==================================================================

    ''' <summary>Che cosa esce, secondo quel che è spuntato adesso.</summary>
    Public ReadOnly Property Scelta As ContenutoBackup
        Get
            Return If(rdoTutto.Checked, ContenutoBackup.Tutto, ContenutoBackup.SoloProfilo)
        End Get
    End Property

    ''' <summary>
    ''' Scrive il backup nel file indicato e lo racconta. È la porta che usa il bottone
    ''' dopo il dialogo di salvataggio, ed è anche quella da cui passa il banco.
    ''' </summary>
    ''' <returns><c>False</c> se il file non si è potuto scrivere; il motivo è nella riga di stato.</returns>
    Public Function EsportaVerso(percorso As String) As Boolean

        Try
            Dim fatto As Dati.Backup = _contesto.Backup.Componi(Scelta)
            _contesto.Backup.Scrivi(fatto, percorso)

            RaccontaLoStato($"Backup scritto: «{percorso}» — {String.Join(", ", CosaCEDentro(fatto))}.",
                            StileApp.TestoSecondario)
            Return True

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is NotSupportedException

            RaccontaLoStato($"Non sono riuscita a scrivere «{percorso}»: {ex.Message}", StileApp.Pericolo)
            Return False

        End Try

    End Function

    Private Sub btnEsporta_Click(sender As Object, e As EventArgs) Handles btnEsporta.Click

        Using scelta As New SaveFileDialog()

            scelta.Title = "Esporta un backup"
            scelta.Filter = "Backup di TrovaLavoro (*.json)|*.json"
            scelta.FileName = ArchivioBackup.NomeProposto(Me.Scelta, Date.Now)
            scelta.AddExtension = True

            ' La cartella «backup» della cartella dati è la proposta, non l'obbligo: si
            ' crea adesso, perché un dialogo che si apre su una cartella che non esiste
            ' riporta l'utente da tutt'altra parte.
            Try
                Directory.CreateDirectory(_contesto.Cartella.CartellaBackup)
                scelta.InitialDirectory = _contesto.Cartella.CartellaBackup
            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                ' Se non si crea, si apre dove vuole Windows: non è un motivo per fermarsi.
            End Try

            If scelta.ShowDialog(Me) <> DialogResult.OK Then Return

            EsportaVerso(scelta.FileName)

        End Using

    End Sub

    Private Sub Scelta_CheckedChanged(sender As Object, e As EventArgs) _
        Handles rdoSoloProfilo.CheckedChanged, rdoTutto.CheckedChanged

        RaccontaCosaCE()

    End Sub

    ' ==================================================================
    ' Ripristinare
    ' ==================================================================

    ''' <summary>
    ''' Apre un file di backup e ne mostra l'anteprima. Non scrive niente: è il passo 2
    ''' del cap. 11.4.
    ''' </summary>
    ''' <returns><c>False</c> se quel file non è un backup leggibile; il perché è nella riga di stato.</returns>
    Public Function Apri(percorso As String) As Boolean

        Try
            _letto = ArchivioBackup.Leggi(percorso)

        Catch ex As InvalidDataException
            Return NienteDaRipristinare(ex.Message)

        Catch ex As JsonException
            Return NienteDaRipristinare(
                $"«{Path.GetFileName(percorso)}» non si lascia leggere come JSON: " &
                "o non è un backup, o è stato rovinato per strada.")

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException

            Return NienteDaRipristinare($"Non sono riuscita a leggere «{percorso}»: {ex.Message}")

        End Try

        Dim detto As AnteprimaRipristino = _contesto.Backup.Anteprima(_letto)

        txtAnteprima.Text = String.Join(vbCrLf,
            {$"Il backup del {detto.Data.ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("it-IT"))} contiene:"}.
            Concat(detto.CosaContiene().Select(Function(r) "  • " & r)).
            Concat({"", "Che cosa cambia sul disco:"}).
            Concat(detto.CosaSovrascrive().Select(Function(r) "  • " & r)))

        btnRipristina.Enabled = True
        RaccontaLoStato($"Letto «{Path.GetFileName(percorso)}». " &
                        "Guarda cosa contiene, poi decidi.", StileApp.TestoSecondario)

        Return True

    End Function

    ''' <summary>
    ''' Rimette al loro posto i dati del backup aperto. Ci si arriva dopo la conferma:
    ''' chi chiama questo metodo ha già chiesto all'utente.
    ''' </summary>
    Public Function Ripristina() As EsitoRipristino

        If _letto Is Nothing Then
            Throw New InvalidOperationException("Non c'è nessun backup aperto da ripristinare.")
        End If

        Try
            Dim esito As EsitoRipristino = _contesto.Backup.Ripristina(_letto)

            If esito.ProfiloRipristinato Then _ProfiloRipristinato = True

            RaccontaLoStato(ComEAndata(esito),
                            If(esito.Rifiutati.Count > 0, StileApp.Pericolo, StileApp.TestoSecondario))

            ' Un backup già ripristinato non si ripristina due volte di fila per un click
            ' distratto: per rifarlo si riapre il file, che è un gesto voluto.
            btnRipristina.Enabled = False

            Return esito

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException

            RaccontaLoStato($"Il ripristino si è fermato: {ex.Message}", StileApp.Pericolo)
            Return Nothing

        End Try

    End Function

    Private Sub btnScegli_Click(sender As Object, e As EventArgs) Handles btnScegli.Click

        Using scelta As New OpenFileDialog()

            scelta.Title = "Scegli il backup da ripristinare"
            scelta.Filter = "Backup di TrovaLavoro (*.json)|*.json"
            scelta.CheckFileExists = True

            If Directory.Exists(_contesto.Cartella.CartellaBackup) Then
                scelta.InitialDirectory = _contesto.Cartella.CartellaBackup
            End If

            If scelta.ShowDialog(Me) <> DialogResult.OK Then Return

            Apri(scelta.FileName)

        End Using

    End Sub

    Private Sub btnRipristina_Click(sender As Object, e As EventArgs) Handles btnRipristina.Click

        If _letto Is Nothing Then Return

        ' La conferma parte da «no»: qui si sovrascrive roba dell'utente, e il tasto
        ' Invio premuto per abitudine non deve poter decidere al posto suo (cap. 03.3).
        Dim detto As AnteprimaRipristino = _contesto.Backup.Anteprima(_letto)

        Dim risposta As DialogResult = MessageBox.Show(
            "Sto per rimettere al loro posto i dati di questo backup." & vbLf & vbLf &
            String.Join(vbLf, detto.CosaSovrascrive().Select(Function(r) "• " & r)) & vbLf & vbLf &
            "Procedo?",
            "TrovaLavoro", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2)

        If risposta <> DialogResult.Yes Then
            RaccontaLoStato("Non ho toccato niente.", StileApp.TestoSecondario)
            Return
        End If

        Ripristina()

    End Sub

    Private Sub btnChiudi_Click(sender As Object, e As EventArgs) Handles btnChiudi.Click
        DialogResult = DialogResult.OK
        Close()
    End Sub

    ' ==================================================================
    ' Quel che si legge
    ' ==================================================================

    ''' <summary>
    ''' Che cosa finirebbe nel backup, com'è la cartella dati adesso. Si dice <b>prima</b>
    ''' di esportare, perché «solo il profilo» e «tutto» siano due scelte informate e non
    ''' due parole.
    ''' </summary>
    Private Sub RaccontaCosaCE()

        ' La guardia serve davvero: spuntando la casella nel designer, l'evento scatta
        ' dentro InitializeComponent — cioè prima che il contesto sia arrivato.
        If _contesto Is Nothing Then Return

        Dim pezzi As New List(Of String)

        Dim quando As Date? = _contesto.Archivio.UltimoSalvataggio
        pezzi.Add(If(quando.HasValue,
                     $"profilo salvato il {quando.Value.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("it-IT"))}",
                     "nessun profilo salvato"))

        Dim versioni As Integer = _contesto.Archivio.Versioni().Count
        If versioni > 0 Then pezzi.Add($"{versioni} {If(versioni = 1, "versione", "versioni")} nello storico")

        If Scelta = ContenutoBackup.Tutto Then

            Dim candidature As Integer = _contesto.Opportunita.Elenco().Count
            pezzi.Add($"{candidature} {If(candidature = 1, "candidatura", "candidature")}")

        End If

        lblCosaCE.Text = "Nel file finirebbe: " & String.Join(", ", pezzi) & "."

    End Sub

    ''' <summary>Che cosa un backup appena scritto si porta dentro, in una riga.</summary>
    Private Shared Function CosaCEDentro(fatto As Dati.Backup) As IEnumerable(Of String)

        Dim pezzi As New List(Of String)

        If fatto.Profilo IsNot Nothing Then pezzi.Add("il profilo")
        If fatto.Storico.Count > 0 Then pezzi.Add($"{fatto.Storico.Count} versioni")
        If fatto.CvBase IsNot Nothing Then pezzi.Add("il CV base")
        If fatto.Opportunita.Count > 0 Then pezzi.Add($"{fatto.Opportunita.Count} candidature")

        If pezzi.Count = 0 Then pezzi.Add("niente, perché nella cartella dati non c'è ancora nulla")

        Return pezzi

    End Function

    ''' <summary>Com'è andato il ripristino, detto all'utente.</summary>
    Private Shared Function ComEAndata(esito As EsitoRipristino) As String

        Dim pezzi As New List(Of String)

        If esito.ProfiloRipristinato Then pezzi.Add("profilo rimesso al suo posto")
        If esito.VersioniAggiunte > 0 Then pezzi.Add($"{esito.VersioniAggiunte} versioni tornate nello storico")
        If esito.CvBaseRipristinato Then pezzi.Add("CV base ripristinato")
        If esito.CandidatureRipristinate > 0 Then pezzi.Add($"{esito.CandidatureRipristinate} candidature")

        Dim detto As String = If(pezzi.Count = 0,
                                 "Il backup non conteneva niente da rimettere a posto.",
                                 "Fatto: " & String.Join(", ", pezzi) & ".")

        If Not String.IsNullOrEmpty(esito.ProfiloMessoInSalvo) Then
            detto &= $" Il profilo di prima è nello storico, come «{esito.ProfiloMessoInSalvo}»."
        End If

        ' Quel che il backup conteneva e non si è scritto non si tace: è l'unico modo per
        ' accorgersi di un file costruito male.
        If esito.Rifiutati.Count > 0 Then
            detto &= $" Non ho scritto {esito.Rifiutati.Count} " &
                     $"{If(esito.Rifiutati.Count = 1, "voce", "voci")} con un nome che non è un nome di file: " &
                     String.Join(", ", esito.Rifiutati) & "."
        End If

        Return detto

    End Function

    Private Function NienteDaRipristinare(perche As String) As Boolean

        _letto = Nothing
        txtAnteprima.Text = ""
        btnRipristina.Enabled = False
        RaccontaLoStato(perche, StileApp.Pericolo)

        Return False

    End Function

    ''' <summary>La riga di stato in fondo: una cosa alla volta, l'ultima che è successa.</summary>
    Private Sub RaccontaLoStato(testo As String, colore As Color)

        lblStato.Text = testo
        lblStato.ForeColor = colore

    End Sub

    ' ==================================================================
    ' Aspetto
    ' ==================================================================

    ''' <summary>I colori e i font, tutti da <see cref="StileApp"/> (cap. 03.2).</summary>
    Private Sub Vesti()

        BackColor = StileApp.SfondoContenuto
        Font = StileApp.FontTesto

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli

        For Each sezione As Label In {lblSezioneEsporta, lblSezioneRipristina}
            sezione.Font = StileApp.FontTitoloPannello
            sezione.ForeColor = StileApp.RossoTitoli
        Next

        lblSpiegazione.ForeColor = StileApp.TestoPrimario
        lblComeSiRipristina.ForeColor = StileApp.TestoPrimario
        lblCosaCE.ForeColor = StileApp.TestoSecondario
        lblStato.ForeColor = StileApp.TestoSecondario

        txtAnteprima.BackColor = StileApp.SfondoContenuto

        StileApp.VestiBottone(btnEsporta, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnScegli, LivelloBottone.Esplorativo)

        ' Livello 5 e non 6: sovrascrive dati esistenti, ma il profilo di prima finisce
        ' nello storico e le candidature non nominate restano — non è una cancellazione
        ' definitiva, e chiedere di ridigitare una parola qui sarebbe un allarme che grida
        ' più forte di quanto il gesto meriti (cap. 03.3).
        StileApp.VestiBottone(btnRipristina, LivelloBottone.Distruttivo)
        StileApp.VestiBottone(btnChiudi, LivelloBottone.Neutro)

    End Sub

    ''' <summary>
    ''' Mette in colonna le due sezioni. Si fa a codice e non nel designer perché le
    ''' spiegazioni sono lunghe quanto serve: è la finestra ad adattarsi al testo.
    ''' </summary>
    Private Sub Disponi()

        Dim sinistra As Integer = StileApp.MargineRiquadro
        ' La larghezza di progetto in pixel veri: dichiararla cruda stringeva la finestra
        ' di un terzo mentre i testi dentro crescevano col DPI, e a mandare a capo il
        ' doppio delle righe era proprio questo (decisione 15.7).
        Dim larghezza As Integer = ScalaSchermo.InPixelDelloSchermo(LarghezzaFinestra, Me.DeviceDpi)
        Dim larghezzaUtile As Integer = larghezza - 2 * StileApp.MargineRiquadro

        For Each testo As Label In {lblSpiegazione, lblComeSiRipristina, lblCosaCE, lblStato}
            testo.MaximumSize = New Size(larghezzaUtile, 0)
        Next

        lblTitolo.Location = New Point(sinistra, StileApp.MargineRiquadro)
        lblSpiegazione.Location = New Point(sinistra, lblTitolo.Bottom + StileApp.DistanzaControlli)

        lblSezioneEsporta.Location = New Point(sinistra, lblSpiegazione.Bottom + StileApp.MargineRiquadro)
        rdoSoloProfilo.Location = New Point(sinistra, lblSezioneEsporta.Bottom + StileApp.InterlineaMinima)
        rdoTutto.Location = New Point(sinistra, rdoSoloProfilo.Bottom + StileApp.InterlineaMinima)
        lblCosaCE.Location = New Point(sinistra, rdoTutto.Bottom + StileApp.InterlineaMinima)
        btnEsporta.Location = New Point(sinistra, lblCosaCE.Bottom + StileApp.DistanzaControlli)

        lblSezioneRipristina.Location = New Point(sinistra, btnEsporta.Bottom + StileApp.MargineRiquadro * 2)
        lblComeSiRipristina.Location = New Point(sinistra, lblSezioneRipristina.Bottom + StileApp.InterlineaMinima)
        btnScegli.Location = New Point(sinistra, lblComeSiRipristina.Bottom + StileApp.DistanzaControlli)

        txtAnteprima.Location = New Point(sinistra, btnScegli.Bottom + StileApp.DistanzaControlli)
        txtAnteprima.Size = New Size(larghezzaUtile, 150)

        btnRipristina.Location = New Point(sinistra, txtAnteprima.Bottom + StileApp.DistanzaControlli)
        lblStato.Location = New Point(sinistra, btnRipristina.Bottom + StileApp.MargineRiquadro)

        ' Il chiudi in fondo a destra: la via d'uscita ha il posto d'onore, come nelle
        ' altre finestre del programma.
        Dim riga As Integer = lblStato.Bottom + StileApp.MargineRiquadro
        btnChiudi.Location = New Point(larghezza - StileApp.MargineRiquadro - btnChiudi.Width, riga)

        ' Un tetto sullo spazio che c'è, e lo scorrimento per quel che non ci sta: le due
        ' cose insieme, perché il tetto da solo taglierebbe e lo scorrimento da solo
        ' lascerebbe la finestra fuori schermo. Senza, a 150% questa finestra si dimensionava
        ' sul proprio contenuto e il sistema la troncava: quel che restava fuori cadeva fuori
        ' dalla <i>finestra</i>, non dallo schermo, e nessuno spostamento lo recuperava
        ' (cap. 03.4, decisione 15.7).
        Dim voluta As Integer = btnChiudi.Bottom + StileApp.MargineRiquadro
        Dim disponibile As Integer = ScalaSchermo.SpazioClienteDisponibile(
            Screen.FromControl(Me).WorkingArea.Height, Me.Height - Me.ClientSize.Height)

        Me.AutoScroll = ScalaSchermo.ServeScorrimento(voluta, disponibile)
        ClientSize = New Size(larghezza, ScalaSchermo.AltezzaSostenibile(voluta, disponibile))

    End Sub

End Class
