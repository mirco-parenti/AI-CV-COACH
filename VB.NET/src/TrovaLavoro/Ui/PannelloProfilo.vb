Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Threading
Imports System.Windows.Forms
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

''' <summary>
''' Pannello P2 — il profilo (cap. 03.6): tutte le sezioni del profilo JSON, sezione per
''' sezione e campo per campo, <b>modificabili</b>. È la scheda che il prototipo non
''' aveva: là un errore di lettura si correggeva rifacendo il turno, qui si corregge la
''' singola voce.
''' </summary>
''' <remarks>
''' <para>Il pannello non parla mai con il disco né con l'AI: chiede al
''' <see cref="ContestoApp"/> (cap. 02.1). Qui dentro c'è solo il mestiere
''' dell'interfaccia — mostrare, raccogliere, chiedere conferma.</para>
''' <para>Le liste e i campi di dettaglio sono la risposta a un problema di forma: un
''' profilo ha un numero qualunque di esperienze, ma la struttura dei pannelli è statica
''' (cap. 03.1, punto 6). Perciò i controlli sono <b>uno solo per campo</b> e mostrano la
''' voce selezionata; a cambiare è il contenuto, mai la struttura.</para>
''' <para>Le correzioni entrano nel profilo in memoria mentre si scrive, ma <b>su disco
''' niente si muove finché non si preme «Salva profilo»</b> (cap. 12.7). Vale anche per
''' quello che propone l'import: è una proposta finché l'utente non la conferma.</para>
''' </remarks>
Public Class PannelloProfilo
    Implements IPannelloArea

    ''' <summary>Le tre risposte possibili sulla patente, nell'ordine della casella.</summary>
    Private Const PatenteNonIndicata As String = "non indicata"
    Private Const PatenteSi As String = "sì"
    Private Const PatenteNo As String = "no"

    ''' <summary>Sotto questa altezza la fascia delle azioni non scende: i bottoni ci devono stare.</summary>
    Private Const AltezzaMinimaAzioni As Integer = 60

    ''' <summary>Il titolo delle finestrelle di conferma: il nome che l'utente conosce.</summary>
    Private Const NomeProdotto As String = "TrovaLavoro"

    Private ReadOnly _suggerimenti As New ToolTip()

    Private _contesto As ContestoApp

    ''' <summary>Il profilo mostrato adesso: cambia mentre si scrive, non è ancora su disco.</summary>
    Private _profilo As New Profilo

    ''' <summary>
    ''' Quanti riempimenti sono in corso. È un <b>contatore</b> e non un interruttore
    ''' perché i riempimenti si annidano: mostrare un profilo riempie gli elenchi, e
    ''' scegliere una riga di un elenco riempie a sua volta i campi della scheda. Con un
    ''' interruttore, il primo riempimento interno che finisce riaprirebbe la guardia
    ''' mentre quello esterno è ancora a metà, e il resto del caricamento verrebbe
    ''' scambiato per una correzione dell'utente.
    ''' </summary>
    Private _riempimenti As Integer

    Private _modificato As Boolean

    ''' <summary>Il filo per annullare la lettura di un CV; <c>Nothing</c> se non è in corso.</summary>
    Private _annullaImport As CancellationTokenSource

    Public Sub New()

        InitializeComponent()

        VestiIBottoni()
        PreparaLaPatente()

        ' La scheda del testo letto compare solo dopo un import: senza un CV alle
        ' spalle sarebbe una scheda vuota che promette qualcosa che non c'è.
        tabSezioni.TabPages.Remove(tabTestoLetto)

        DichiaraLeTappeCheMancano()
        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Collega il pannello al motore e mostra il profilo che c'è su disco.
    ''' </summary>
    Public Sub Collega(contesto As ContestoApp)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
        _contesto = contesto

        CaricaIlProfilo()
        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Se ci sono correzioni che su disco non ci sono ancora. La finestra lo chiede
    ''' prima di chiudersi: il profilo è l'unica cosa che l'utente non può rifare da capo.
    ''' </summary>
    Public ReadOnly Property HaModificheNonSalvate As Boolean
        Get
            Return _modificato
        End Get
    End Property

    ''' <inheritdoc/>
    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        ' La fascia delle azioni si prende tutta l'altezza che il logo sfonda nell'area
        ' centrale, e comincia dopo la sua larghezza: così nessun dato finisce coperto,
        ' e a cedere il posto sono i bottoni, che di spazio ne chiedono poco.
        pnlAzioni.Height = Math.Max(AltezzaMinimaAzioni, ingombro.Height)
        pnlAzioni.Padding = New Padding(ingombro.Width + StileApp.DistanzaControlli, 0, 0, 0)

        DisponiLeAzioni()

    End Sub

    ''' <summary>
    ''' Mette i bottoni sul fondo della fascia, quelli del profilo a sinistra e quelli
    ''' che portano altrove a destra. Si rifà a ogni cambio di ingombro e a ogni
    ''' ridimensionamento, perché entrambi cambiano il rettangolo disponibile.
    ''' </summary>
    Private Sub DisponiLeAzioni()

        Dim riga As Integer = pnlAzioni.Height - StileApp.MargineRiquadro - StileApp.BottoneStandard.Height

        Dim sinistra As Integer = pnlAzioni.Padding.Left
        For Each bottone As Button In {btnImporta, btnDialogo, btnAggiornamento}
            bottone.Location = New Point(sinistra, riga)
            sinistra += bottone.Width + StileApp.DistanzaControlli
        Next

        Dim destra As Integer = pnlAzioni.ClientSize.Width - StileApp.MargineRiquadro
        For Each bottone As Button In {btnSalva, btnEsportaBackup, btnGeneraCv1}
            destra -= bottone.Width
            bottone.Location = New Point(destra, riga)
            destra -= StileApp.DistanzaControlli
        Next

    End Sub

    Private Sub PannelloProfilo_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        DisponiLeAzioni()
    End Sub

    ' ==================================================================
    ' Il profilo: caricarlo, mostrarlo, raccontarne lo stato
    ' ==================================================================

    ''' <summary>
    ''' Legge il profilo dall'archivio. I tre casi sono davvero tre e vanno detti in
    ''' modo diverso: non l'ho ancora fatto, eccolo, oppure c'è ma è rotto — quest'ultimo
    ''' non ripiega mai su un profilo vuoto (cap. 11.1), perché sovrascriverlo sarebbe il
    ''' modo peggiore di perdere il lavoro di chi l'ha costruito.
    ''' </summary>
    Private Sub CaricaIlProfilo()

        If Not _contesto.Archivio.Esiste Then
            Mostra(New Profilo())
            RaccontaLoStato(
                "Non hai ancora un profilo." & vbLf &
                "Importalo dal tuo CV, oppure costruiscilo con il dialogo.",
                StileApp.TestoSecondario)
            Return
        End If

        Try
            Mostra(_contesto.Archivio.Carica())
            RaccontaLoStato(RiassuntoDelProfilo(), StileApp.TestoSecondario)

        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                   OrElse TypeOf ex Is UnauthorizedAccessException
            ' Il file resta dov'è, intatto: si dice cosa non va e dove guardare.
            Mostra(New Profilo())
            RaccontaLoStato(
                $"Il profilo c'è ma non si lascia leggere: {ex.Message}" & vbLf &
                $"Il file è {_contesto.Cartella.FileProfilo}: correggilo a mano, oppure reimporta il CV.",
                StileApp.Pericolo)
        End Try

    End Sub

    ''' <summary>Mostra un profilo: i dati personali e le quattro liste.</summary>
    Public Sub Mostra(profilo As Profilo)

        If profilo Is Nothing Then Throw New ArgumentNullException(NameOf(profilo))

        Riempiendo(
            Sub()
                _profilo = profilo

                txtNome.Text = profilo.Nome
                txtEmail.Text = profilo.Contatti.Email
                txtTelefono.Text = profilo.Contatti.Telefono
                txtDomicilio.Text = profilo.Contatti.Citta
                txtLink.Text = profilo.Contatti.Link

                cmbPatente.SelectedItem = If(profilo.Patente.Ha = PatenteSi OrElse profilo.Patente.Ha = PatenteNo,
                                             profilo.Patente.Ha, PatenteNonIndicata)
                txtCategorie.Text = String.Join(", ", profilo.Patente.Categorie)

                RiempiElenco(lstLavoro, profilo.EsperienzeFormali.Select(AddressOf EtichettaLavoro))
                RiempiElenco(lstInformali, profilo.EsperienzeInformali.Select(AddressOf EtichettaInformale))
                RiempiElenco(lstCompetenze, profilo.Competenze)
                RiempiElenco(lstStudi, profilo.Formazione.Select(AddressOf EtichettaStudio))

                MostraLaVoceScelta()
            End Sub)

        AggiornaComandi()

    End Sub

    ''' <summary>Il testo del riquadro in alto a destra: cos'è questo profilo, in una riga.</summary>
    Private Function RiassuntoDelProfilo() As String

        Dim quando As Date? = _contesto.Archivio.UltimoSalvataggio
        Dim intestazione As String =
            If(quando.HasValue,
               $"Salvato il {quando.Value.ToString("d MMMM yyyy 'alle' HH:mm", CultureInfo.CurrentCulture)}",
               "Profilo presente")

        Return intestazione & vbLf &
               $"{Conteggio(_profilo.EsperienzeFormali.Count, "esperienza di lavoro", "esperienze di lavoro")} · " &
               $"{Conteggio(_profilo.EsperienzeInformali.Count, "esperienza informale", "esperienze informali")} · " &
               $"{Conteggio(_profilo.Competenze.Count, "competenza", "competenze")} · " &
               $"{Conteggio(_profilo.Formazione.Count, "titolo di studio", "titoli di studio")}"

    End Function

    Private Shared Function Conteggio(quanti As Integer, singolare As String, plurale As String) As String
        Return $"{quanti} {If(quanti = 1, singolare, plurale)}"
    End Function

    Private Sub RaccontaLoStato(testo As String, colore As Color)
        lblStatoProfilo.Text = testo
        lblStatoProfilo.ForeColor = colore
    End Sub

    ' ==================================================================
    ' Le liste e le schede di dettaglio
    ' ==================================================================

    Private Shared Sub RiempiElenco(elenco As ListBox, voci As IEnumerable(Of String))

        elenco.BeginUpdate()
        elenco.Items.Clear()
        For Each voce As String In voci
            elenco.Items.Add(voce)
        Next
        elenco.EndUpdate()

        If elenco.Items.Count > 0 Then elenco.SelectedIndex = 0

    End Sub

    ''' <summary>Come si chiama un'esperienza di lavoro nell'elenco: il ruolo, e dove.</summary>
    Private Shared Function EtichettaLavoro(voce As EsperienzaFormale) As String

        Dim ruolo As String = If(voce.Ruolo <> "", voce.Ruolo, "(ruolo non indicato)")
        Return If(voce.Azienda <> "", $"{ruolo} — {voce.Azienda}", ruolo)

    End Function

    Private Shared Function EtichettaInformale(voce As EsperienzaInformale) As String
        Return If(voce.CosaFacevo <> "", Accorcia(voce.CosaFacevo), "(esperienza senza descrizione)")
    End Function

    Private Shared Function EtichettaStudio(voce As VoceFormazione) As String

        Dim titolo As String = If(voce.Titolo <> "", voce.Titolo, "(titolo non indicato)")
        Return If(voce.Anno <> "", $"{titolo} ({voce.Anno})", titolo)

    End Function

    ''' <summary>Una riga di elenco non è un racconto: si taglia sulla prima riga utile.</summary>
    Private Shared Function Accorcia(testo As String) As String

        Dim riga As String = testo.Replace(vbCrLf, " ").Replace(vbLf, " ").Trim()
        If riga.Length <= 70 Then Return riga

        Return riga.Substring(0, 69).TrimEnd() & "…"

    End Function

    ''' <summary>
    ''' Esegue un riempimento tenendo alzata la guardia, anche se dentro ne comincia un
    ''' altro: è ciò che distingue «lo sta scrivendo il programma» da «lo sta scrivendo
    ''' l'utente», e senza questa distinzione ogni caricamento verrebbe scambiato per una
    ''' correzione da salvare.
    ''' </summary>
    Private Sub Riempiendo(riempimento As Action)

        _riempimenti += 1
        Try
            riempimento()
        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ''' <summary>Se in questo momento è il programma a scrivere nei campi.</summary>
    Private ReadOnly Property InCaricamento As Boolean
        Get
            Return _riempimenti > 0
        End Get
    End Property

    ''' <summary>Rimette nei campi la voce selezionata di ogni sezione.</summary>
    Private Sub MostraLaVoceScelta()

        MostraLavoroScelto()
        MostraInformaleScelta()
        MostraCompetenzaScelta()
        MostraStudioScelto()

    End Sub

    Private Sub MostraLavoroScelto()

        Dim voce As EsperienzaFormale = VoceScelta(lstLavoro, _profilo.EsperienzeFormali)

        Riempiendo(
            Sub()
                txtRuolo.Text = If(voce?.Ruolo, "")
                txtAzienda.Text = If(voce?.Azienda, "")
                txtDurata.Text = If(voce?.Durata, "")
                txtTipo.Text = If(voce?.Tipo, "")
                txtCosaFacevoLavoro.Text = If(voce?.CosaFacevo, "")
            End Sub)

    End Sub

    Private Sub MostraInformaleScelta()

        Dim voce As EsperienzaInformale = VoceScelta(lstInformali, _profilo.EsperienzeInformali)

        Riempiendo(
            Sub()
                txtQuando.Text = If(voce?.Quando, "")
                txtConChi.Text = If(voce?.ConChi, "")
                txtCosaFacevoInformale.Text = If(voce?.CosaFacevo, "")
            End Sub)

    End Sub

    Private Sub MostraCompetenzaScelta()

        Dim indice As Integer = lstCompetenze.SelectedIndex

        Riempiendo(
            Sub()
                txtCompetenza.Text = If(indice >= 0 AndAlso indice < _profilo.Competenze.Count,
                                        _profilo.Competenze(indice), "")
            End Sub)

    End Sub

    Private Sub MostraStudioScelto()

        Dim voce As VoceFormazione = VoceScelta(lstStudi, _profilo.Formazione)

        Riempiendo(
            Sub()
                txtTitoloStudio.Text = If(voce?.Titolo, "")
                txtIstituto.Text = If(voce?.Istituto, "")
                txtAnno.Text = If(voce?.Anno, "")
            End Sub)

    End Sub

    Private Sub lstLavoro_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lstLavoro.SelectedIndexChanged
        MostraLavoroScelto()
        AggiornaComandi()
    End Sub

    Private Sub lstInformali_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lstInformali.SelectedIndexChanged
        MostraInformaleScelta()
        AggiornaComandi()
    End Sub

    Private Sub lstCompetenze_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lstCompetenze.SelectedIndexChanged
        MostraCompetenzaScelta()
        AggiornaComandi()
    End Sub

    Private Sub lstStudi_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lstStudi.SelectedIndexChanged
        MostraStudioScelto()
        AggiornaComandi()
    End Sub

    ''' <summary>La voce selezionata, o <c>Nothing</c> se la lista è vuota o disallineata.</summary>
    Private Shared Function VoceScelta(Of T As Class)(elenco As ListBox, voci As List(Of T)) As T

        Dim indice As Integer = elenco.SelectedIndex
        If indice < 0 OrElse indice >= voci.Count Then Return Nothing

        Return voci(indice)

    End Function

    ' ==================================================================
    ' Le correzioni dell'utente
    ' ==================================================================

    Private Sub DatiPersonali_Cambiati(sender As Object, e As EventArgs) _
        Handles txtNome.TextChanged, txtEmail.TextChanged, txtTelefono.TextChanged,
                txtDomicilio.TextChanged, txtLink.TextChanged, txtCategorie.TextChanged,
                cmbPatente.SelectedIndexChanged

        If InCaricamento Then Return

        _profilo.Nome = txtNome.Text.Trim()
        _profilo.Contatti.Email = txtEmail.Text.Trim()
        _profilo.Contatti.Telefono = txtTelefono.Text.Trim()
        _profilo.Contatti.Citta = txtDomicilio.Text.Trim()
        _profilo.Contatti.Link = txtLink.Text.Trim()

        Dim risposta As String = If(TryCast(cmbPatente.SelectedItem, String), PatenteNonIndicata)
        _profilo.Patente.Ha = If(risposta = PatenteNonIndicata, "", risposta)
        _profilo.Patente.Categorie = Categorie(txtCategorie.Text)

        SegnaModificato()

    End Sub

    ''' <summary>Le categorie della patente scritte in fila: «B, C» diventa due voci.</summary>
    Private Shared Function Categorie(testo As String) As List(Of String)

        Return testo.Split({","c, ";"c}).
            Select(Function(c) c.Trim()).
            Where(Function(c) c <> "").
            ToList()

    End Function

    Private Sub CampiLavoro_Cambiati(sender As Object, e As EventArgs) _
        Handles txtRuolo.TextChanged, txtAzienda.TextChanged, txtDurata.TextChanged,
                txtTipo.TextChanged, txtCosaFacevoLavoro.TextChanged

        If InCaricamento Then Return

        Dim voce As EsperienzaFormale = VoceScelta(lstLavoro, _profilo.EsperienzeFormali)
        If voce Is Nothing Then Return

        voce.Ruolo = txtRuolo.Text.Trim()
        voce.Azienda = txtAzienda.Text.Trim()
        voce.Durata = txtDurata.Text.Trim()
        voce.Tipo = txtTipo.Text.Trim()
        voce.CosaFacevo = txtCosaFacevoLavoro.Text.Trim()

        RinominaVoce(lstLavoro, EtichettaLavoro(voce))
        SegnaModificato()

    End Sub

    Private Sub CampiInformale_Cambiati(sender As Object, e As EventArgs) _
        Handles txtQuando.TextChanged, txtConChi.TextChanged, txtCosaFacevoInformale.TextChanged

        If InCaricamento Then Return

        Dim voce As EsperienzaInformale = VoceScelta(lstInformali, _profilo.EsperienzeInformali)
        If voce Is Nothing Then Return

        voce.Quando = txtQuando.Text.Trim()
        voce.ConChi = txtConChi.Text.Trim()
        voce.CosaFacevo = txtCosaFacevoInformale.Text.Trim()

        RinominaVoce(lstInformali, EtichettaInformale(voce))
        SegnaModificato()

    End Sub

    Private Sub CampoCompetenza_Cambiato(sender As Object, e As EventArgs) Handles txtCompetenza.TextChanged

        If InCaricamento Then Return

        Dim indice As Integer = lstCompetenze.SelectedIndex
        If indice < 0 OrElse indice >= _profilo.Competenze.Count Then Return

        _profilo.Competenze(indice) = txtCompetenza.Text.Trim()

        RinominaVoce(lstCompetenze, _profilo.Competenze(indice))
        SegnaModificato()

    End Sub

    Private Sub CampiStudio_Cambiati(sender As Object, e As EventArgs) _
        Handles txtTitoloStudio.TextChanged, txtIstituto.TextChanged, txtAnno.TextChanged

        If InCaricamento Then Return

        Dim voce As VoceFormazione = VoceScelta(lstStudi, _profilo.Formazione)
        If voce Is Nothing Then Return

        voce.Titolo = txtTitoloStudio.Text.Trim()
        voce.Istituto = txtIstituto.Text.Trim()
        voce.Anno = txtAnno.Text.Trim()

        RinominaVoce(lstStudi, EtichettaStudio(voce))
        SegnaModificato()

    End Sub

    ''' <summary>
    ''' Riscrive la riga selezionata dell'elenco quando i suoi campi cambiano: l'elenco
    ''' deve dire sempre quello che c'è nella scheda, non quello che c'era prima.
    ''' </summary>
    Private Shared Sub RinominaVoce(elenco As ListBox, etichetta As String)

        Dim indice As Integer = elenco.SelectedIndex
        If indice < 0 Then Return

        ' Riscrivere una riga con lo stesso testo la farebbe solo sfarfallare.
        If String.Equals(elenco.Items(indice).ToString(), etichetta, StringComparison.Ordinal) Then Return

        elenco.Items(indice) = etichetta

    End Sub

    Private Sub SegnaModificato()

        _modificato = True
        AggiornaComandi()

    End Sub

    ' ==================================================================
    ' Aggiungere e togliere voci
    ' ==================================================================

    Private Sub btnAggiungiLavoro_Click(sender As Object, e As EventArgs) Handles btnAggiungiLavoro.Click

        _profilo.EsperienzeFormali.Add(New EsperienzaFormale())
        AggiungiInFondo(lstLavoro, EtichettaLavoro(_profilo.EsperienzeFormali.Last()))
        txtRuolo.Focus()

    End Sub

    Private Sub btnAggiungiInformale_Click(sender As Object, e As EventArgs) Handles btnAggiungiInformale.Click

        _profilo.EsperienzeInformali.Add(New EsperienzaInformale())
        AggiungiInFondo(lstInformali, EtichettaInformale(_profilo.EsperienzeInformali.Last()))
        txtCosaFacevoInformale.Focus()

    End Sub

    Private Sub btnAggiungiCompetenza_Click(sender As Object, e As EventArgs) Handles btnAggiungiCompetenza.Click

        _profilo.Competenze.Add("")
        AggiungiInFondo(lstCompetenze, "(competenza da scrivere)")
        txtCompetenza.Focus()

    End Sub

    Private Sub btnAggiungiStudio_Click(sender As Object, e As EventArgs) Handles btnAggiungiStudio.Click

        _profilo.Formazione.Add(New VoceFormazione())
        AggiungiInFondo(lstStudi, EtichettaStudio(_profilo.Formazione.Last()))
        txtTitoloStudio.Focus()

    End Sub

    ''' <summary>Aggiunge la riga nuova, la seleziona e segna il profilo come modificato.</summary>
    Private Sub AggiungiInFondo(elenco As ListBox, etichetta As String)

        elenco.Items.Add(etichetta)
        elenco.SelectedIndex = elenco.Items.Count - 1

        SegnaModificato()

    End Sub

    Private Sub btnEliminaLavoro_Click(sender As Object, e As EventArgs) Handles btnEliminaLavoro.Click
        EliminaVoce(lstLavoro, _profilo.EsperienzeFormali, "questa esperienza di lavoro")
    End Sub

    Private Sub btnEliminaInformale_Click(sender As Object, e As EventArgs) Handles btnEliminaInformale.Click
        EliminaVoce(lstInformali, _profilo.EsperienzeInformali, "questa esperienza informale")
    End Sub

    Private Sub btnEliminaCompetenza_Click(sender As Object, e As EventArgs) Handles btnEliminaCompetenza.Click
        EliminaVoce(lstCompetenze, _profilo.Competenze, "questa competenza")
    End Sub

    Private Sub btnEliminaStudio_Click(sender As Object, e As EventArgs) Handles btnEliminaStudio.Click
        EliminaVoce(lstStudi, _profilo.Formazione, "questo titolo di studio")
    End Sub

    ''' <summary>
    ''' Toglie la voce selezionata, dopo averlo chiesto: è un'azione distruttiva
    ''' (livello 5, cap. 03.3) e quello che si cancella è il racconto dell'utente.
    ''' </summary>
    Private Sub EliminaVoce(Of T)(elenco As ListBox, voci As List(Of T), come As String)

        Dim indice As Integer = elenco.SelectedIndex
        If indice < 0 OrElse indice >= voci.Count Then Return

        Dim risposta As DialogResult = MessageBox.Show(
            $"Vuoi togliere {come} dal profilo?" & vbLf & vbLf & elenco.Items(indice).ToString(),
            NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)

        If risposta <> DialogResult.Yes Then Return

        voci.RemoveAt(indice)
        elenco.Items.RemoveAt(indice)

        If elenco.Items.Count > 0 Then
            elenco.SelectedIndex = Math.Min(indice, elenco.Items.Count - 1)
        Else
            ' Senza voci non c'è niente da mostrare: i campi si svuotano da soli.
            MostraLaVoceScelta()
        End If

        SegnaModificato()

    End Sub

    ' ==================================================================
    ' Salvare
    ' ==================================================================

    Private Sub btnSalva_Click(sender As Object, e As EventArgs) Handles btnSalva.Click

        Try
            _contesto.Archivio.Salva(_profilo)

            _modificato = False
            AggiornaComandi()
            RaccontaLoStato(RiassuntoDelProfilo(), StileApp.TestoSecondario)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' Il profilo buono di prima è ancora al suo posto (cap. 11.1): si dice cosa
            ' è successo, e quello che l'utente ha scritto resta nei campi.
            RaccontaLoStato(
                $"Non sono riuscita a salvare il profilo: {ex.Message}" & vbLf &
                $"La cartella è {_contesto.Cartella.CartellaProfilo}. Le tue correzioni sono ancora qui.",
                StileApp.Pericolo)
        End Try

    End Sub

    ' ==================================================================
    ' Importare da un CV
    ' ==================================================================

    Private Async Sub btnImporta_Click(sender As Object, e As EventArgs) Handles btnImporta.Click

        ' A lettura in corso lo stesso bottone serve ad annullarla.
        If _annullaImport IsNot Nothing Then
            _annullaImport.Cancel()
            Return
        End If

        If Not PossoSostituireIlProfilo() Then Return

        Dim percorso As String = ChiediIlFileDelCv()
        If percorso Is Nothing Then Return

        Using filo As New CancellationTokenSource()

            _annullaImport = filo
            LetturaInCorso(True)
            RaccontaLoStato(
                $"Sto leggendo «{Path.GetFileName(percorso)}»…" & vbLf &
                "Può volerci qualche secondo: il CV passa dall'AI.",
                StileApp.TestoSecondario)

            Try
                Dim esito As EsitoImport = Await _contesto.ImportCv.DaFileAsync(percorso, filo.Token)

                Mostra(esito.Profilo)
                MostraIlTestoLetto(esito.TestoLetto)

                _modificato = True
                RaccontaLoStato(
                    $"Profilo proposto da «{Path.GetFileName(percorso)}»." & vbLf &
                    "Controllalo campo per campo — la scheda «Testo letto» ha l'originale — poi salvalo.",
                    StileApp.TestoSecondario)

            Catch ex As OperationCanceledException
                RaccontaLoStato("Lettura annullata: il profilo è rimasto com'era.", StileApp.TestoSecondario)

            Catch ex As Exception When TypeOf ex Is ErroreImport OrElse TypeOf ex Is ErroreAi
                ' Questi messaggi sono già scritti per l'utente, e dove esiste una via
                ' d'uscita se la portano dietro (cap. 05.1).
                RaccontaLoStato(ex.Message, StileApp.Pericolo)

            Finally
                _annullaImport = Nothing
                LetturaInCorso(False)
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' Chiede il file da importare. I formati sono quelli che il motore sa leggere
    ''' (cap. 05.1); nient'altro compare nell'elenco, per non far scegliere un file che
    ''' poi verrebbe rifiutato.
    ''' </summary>
    ''' <returns>Il percorso scelto, o <c>Nothing</c> se l'utente ha rinunciato.</returns>
    Private Shared Function ChiediIlFileDelCv() As String

        Using scelta As New OpenFileDialog()
            scelta.Title = "Scegli il CV da importare"
            scelta.Filter = "CV (*.pdf;*.docx;*.txt;*.md)|*.pdf;*.docx;*.txt;*.md"
            scelta.CheckFileExists = True

            If scelta.ShowDialog() <> DialogResult.OK Then Return Nothing
            Return scelta.FileName
        End Using

    End Function

    ''' <summary>
    ''' Se c'è del lavoro non salvato, chiede prima di sostituirlo: un import riscrive
    ''' tutti i campi, e ciò che l'utente aveva corretto sparirebbe senza preavviso.
    ''' </summary>
    Private Function PossoSostituireIlProfilo() As Boolean

        If Not _modificato Then Return True

        Return MessageBox.Show(
            "Hai correzioni al profilo che non hai ancora salvato." & vbLf &
            "Importare un CV le sostituisce tutte. Vuoi procedere?",
            NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2) = DialogResult.Yes

    End Function

    ''' <summary>
    ''' Mette il testo del CV nella sua scheda, che da adesso resta a disposizione. È la
    ''' prova a cui l'utente può tornare per controllare che nel profilo non sia comparso
    ''' nulla che nel CV non c'era.
    ''' </summary>
    Private Sub MostraIlTestoLetto(testo As String)

        txtTestoLetto.Text = testo

        If Not tabSezioni.TabPages.Contains(tabTestoLetto) Then
            tabSezioni.TabPages.Add(tabTestoLetto)
        End If

    End Sub

    ''' <summary>Il pannello mentre l'AI lavora: niente si tocca, e si può rinunciare.</summary>
    Private Sub LetturaInCorso(inCorso As Boolean)

        btnImporta.Text = If(inCorso, "Annulla lettura", "Importa da un CV…")
        Cursor = If(inCorso, Cursors.AppStarting, Cursors.Default)

        AggiornaComandi()

    End Sub

    ' ==================================================================
    ' Aspetto e stato dei comandi
    ' ==================================================================

    Private Sub VestiIBottoni()

        For Each bottone As Button In {btnImporta, btnDialogo, btnAggiornamento, btnEsportaBackup,
                                       btnAggiungiLavoro, btnAggiungiInformale,
                                       btnAggiungiCompetenza, btnAggiungiStudio}
            StileApp.VestiBottone(bottone, LivelloBottone.Esplorativo)
        Next

        For Each bottone As Button In {btnEliminaLavoro, btnEliminaInformale,
                                       btnEliminaCompetenza, btnEliminaStudio}
            StileApp.VestiBottone(bottone, LivelloBottone.Distruttivo)
        Next

        StileApp.VestiBottone(btnGeneraCv1, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnSalva, LivelloBottone.SicuroPositivo)

    End Sub

    ''' <summary>
    ''' Riempie la casella della patente e la porta sul valore di partenza. Va fatto a
    ''' guardia alzata: scegliere il valore predefinito è il programma che prepara il
    ''' pannello, non l'utente che risponde — senza, il profilo nascerebbe «modificato»
    ''' prima ancora di essere mostrato, con il bottone «Salva» acceso a vuoto.
    ''' </summary>
    Private Sub PreparaLaPatente()

        Riempiendo(
            Sub()
                cmbPatente.Items.Add(PatenteNonIndicata)
                cmbPatente.Items.Add(PatenteSi)
                cmbPatente.Items.Add(PatenteNo)
                cmbPatente.SelectedItem = PatenteNonIndicata
            End Sub)

    End Sub

    ''' <summary>
    ''' I bottoni che appartengono a tappe successive restano visibili e spenti, con
    ''' scritto quando arrivano: un bottone che sparisce non insegna niente, uno spento
    ''' che si spiega dice come è fatto il programma.
    ''' </summary>
    Private Sub DichiaraLeTappeCheMancano()

        _suggerimenti.SetToolTip(btnAggiornamento,
            "La sessione di aggiornamento del profilo arriva più avanti (flusso D).")
        _suggerimenti.SetToolTip(btnGeneraCv1, "La generazione dei documenti arriva con la tappa T4.")
        _suggerimenti.SetToolTip(btnEsportaBackup, "Backup e ripristino arrivano con la tappa T9.")

    End Sub

    ''' <summary>
    ''' Decide, in un posto solo, che cosa si può fare adesso. Le tre domande sono: c'è
    ''' il motore, c'è qualcosa da salvare, e l'AI sta già lavorando?
    ''' </summary>
    Private Sub AggiornaComandi()

        Dim conMotore As Boolean = _contesto IsNot Nothing
        Dim occupato As Boolean = _annullaImport IsNot Nothing

        ' A lettura in corso il bottone dell'import resta acceso: è l'annulla.
        btnImporta.Enabled = occupato OrElse (conMotore AndAlso _contesto.AiDisponibile)
        If Not occupato AndAlso conMotore AndAlso Not _contesto.AiDisponibile Then
            _suggerimenti.SetToolTip(btnImporta,
                $"Per leggere un CV serve la chiave API ({ClientClaude.NomeVariabileChiave}).")
        End If

        ' Il dialogo guidato si accende quando il suo pannello esiste.
        btnDialogo.Enabled = False
        btnAggiornamento.Enabled = False
        btnGeneraCv1.Enabled = False
        btnEsportaBackup.Enabled = False

        ' Salvare ha senso solo se c'è qualcosa di nuovo da mettere su disco: una
        ' conferma a vuoto lascerebbe nello storico una versione identica alla
        ' precedente, e lo storico serve a raccontare i cambiamenti.
        btnSalva.Enabled = conMotore AndAlso _modificato AndAlso Not occupato

        For Each bottone As Button In {btnAggiungiLavoro, btnAggiungiInformale,
                                       btnAggiungiCompetenza, btnAggiungiStudio}
            bottone.Enabled = Not occupato
        Next

        ' Togliere ha senso solo se c'è una voce scelta: un «Elimina» acceso davanti a
        ' un elenco vuoto prometterebbe un'azione che non può avvenire.
        btnEliminaLavoro.Enabled = Not occupato AndAlso lstLavoro.SelectedIndex >= 0
        btnEliminaInformale.Enabled = Not occupato AndAlso lstInformali.SelectedIndex >= 0
        btnEliminaCompetenza.Enabled = Not occupato AndAlso lstCompetenze.SelectedIndex >= 0
        btnEliminaStudio.Enabled = Not occupato AndAlso lstStudi.SelectedIndex >= 0

        For Each casella As TextBox In CampiDelProfilo()
            casella.ReadOnly = occupato
            casella.BackColor = If(occupato, StileApp.SfondoBase, StileApp.SfondoContenuto)
        Next

        cmbPatente.Enabled = Not occupato

    End Sub

    Private Function CampiDelProfilo() As TextBox()

        Return {txtNome, txtEmail, txtTelefono, txtDomicilio, txtLink, txtCategorie,
                txtRuolo, txtAzienda, txtDurata, txtTipo, txtCosaFacevoLavoro,
                txtQuando, txtConChi, txtCosaFacevoInformale, txtCompetenza,
                txtTitoloStudio, txtIstituto, txtAnno}

    End Function

End Class
