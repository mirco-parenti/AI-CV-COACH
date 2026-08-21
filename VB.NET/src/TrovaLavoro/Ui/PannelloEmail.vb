Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore

''' <summary>
''' Pannello P7 — l'email di candidatura (cap. 03.6, cap. 07.1): destinatario, oggetto,
''' corpo e allegati da spuntare, e il bottone che scrive il file <c>.eml</c> e lo apre nel
''' programma di posta.
''' </summary>
''' <remarks>
''' <para><b>Qui il programma si ferma un passo prima.</b> Non esiste un bottone «Invia»:
''' l'app prepara una bozza e la consegna al programma di posta dell'utente, dove è già
''' autenticato (cap. 07.2). L'ultima parola è sua, e anche la prova che sia partita: «l'ho
''' spedita» è una sua dichiarazione, non un esito tecnico che l'app possa osservare.</para>
''' <para><b>È il pannello dove l'utente scrive davvero.</b> Il destinatario lo digita lui
''' — il programma non lo inventa mai — e oggetto e corpo, che l'AI propone, si correggono
''' a mano. Per questo la bozza si salva (<c>email.json</c>): riaprire la candidatura
''' domani non deve voler dire ricominciare.</para>
''' </remarks>
Public Class PannelloEmail
    Implements IPannelloArea, IPannelloCheSalvaUscendo

    ''' <summary>Sotto questa altezza la fascia delle azioni non scende: i bottoni ci devono stare.</summary>
    Private Const AltezzaMinimaAzioni As Integer = 60

    ''' <summary>Quanto spazio si prende il logo flottante (cap. 03.5).</summary>
    Private _ingombroLogo As Size

    ''' <summary>La fascia dei comandi in fondo (cap. 03.4).</summary>
    Private _comandi As FasciaDeiComandi

    Private ReadOnly _suggerimenti As New ToolTip()

    Private _contesto As ContestoApp

    ''' <summary>Chi scrive il messaggio; <c>Nothing</c> quando l'AI non c'è.</summary>
    Private _compositore As ICompositoreEmail

    ''' <summary>Chi riconosce i documenti della cartella; <c>Nothing</c> quando l'AI non c'è.</summary>
    Private _classificatore As IClassificatoreDocumenti

    ''' <summary>La passata anti-slop (cap. 08); <c>Nothing</c> quando l'AI non c'è.</summary>
    Private _rifinitura As Rifinitura

    ''' <summary>La candidatura a cui l'email appartiene; <c>Nothing</c> finché non ne arriva una.</summary>
    Private _candidatura As Opportunita

    ''' <summary>
    ''' Vero quando il destinatario in casella l'ha proposto l'annuncio invece di
    ''' scriverlo l'utente: serve a dirglielo, perché un indirizzo comparso da solo in
    ''' un'email che sta per partire va spiegato (cap. 07.1).
    ''' </summary>
    Private _destinatarioVieneDallAnnuncio As Boolean

    ''' <summary>
    ''' Vero quando l'anti-slop è inciampato e il corpo mostrato è quello grezzo del
    ''' compositore: chi ha la rifinitura accesa deve sapere che questa volta non c'è
    ''' stata, o crederebbe rifinito un testo che non lo è (cap. 08.4).
    ''' </summary>
    Private _ilCorpoNonERifinito As Boolean

    ''' <summary>La bozza mostrata adesso: cambia mentre si scrive, e si salva a ogni passo che conta.</summary>
    Private _bozza As New BozzaEmail

    ''' <summary>Come in P2: un contatore, perché i riempimenti si annidano.</summary>
    Private _riempimenti As Integer

    ''' <summary>Il filo per annullare la scrittura in corso; <c>Nothing</c> se non ce n'è una.</summary>
    Private _annulla As CancellationTokenSource

    ''' <summary>Si torna ai documenti della candidatura (P6).</summary>
    Public Event TornaAiDocumenti As EventHandler

    ''' <summary>L'AI ha cominciato o finito: la finestra blocca la barra (cap. 02.6).</summary>
    Public Event LavoroAiCambiato As EventHandler

    ''' <summary>
    ''' La candidatura è passata a «inviata»: la Home deve rileggere, e il cruscotto
    ''' mostrare il numero nuovo (cap. 07.3).
    ''' </summary>
    Public Event CandidaturaInviata As EventHandler

    Public Sub New()

        InitializeComponent()

        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        VestiIBottoni()
        AggiornaComandi()

    End Sub

    ''' <summary>Collega il pannello al motore.</summary>
    ''' <param name="compositore">
    ''' Chi scrive il messaggio. Di norma si omette ed è quello del contesto; il banco
    ''' passa qui il suo, e prova il pannello intero senza chiave e senza rete — come P2
    ''' fa con l'import e P6 con la pipeline.
    ''' </param>
    ''' <param name="classificatore">
    ''' Chi riconosce i documenti della cartella dell'utente (cap. 05.2). Come sopra: di
    ''' norma è quello del contesto, il banco passa il suo.
    ''' </param>
    Public Sub Collega(contesto As ContestoApp, Optional compositore As ICompositoreEmail = Nothing,
                       Optional classificatore As IClassificatoreDocumenti = Nothing,
                       Optional rifinitura As Rifinitura = Nothing)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
        _contesto = contesto
        _compositore = If(compositore, contesto.Email)
        _classificatore = If(classificatore, contesto.Classificatore)
        _rifinitura = If(rifinitura, contesto.Rifinitura)

        AggiornaComandi()

    End Sub

    ''' <summary>Se in questo momento l'AI sta scrivendo l'email.</summary>
    Public ReadOnly Property AiAlLavoro As Boolean
        Get
            Return _annulla IsNot Nothing
        End Get
    End Property

    ''' <summary>Annulla la scrittura in corso, se c'è: è la via pulita della chiusura.</summary>
    Public Sub AnnullaIlLavoro()
        _annulla?.Cancel()
    End Sub

    ' ==================================================================
    ' L'arrivo: una candidatura con i suoi documenti
    ' ==================================================================

    ''' <summary>
    ''' Mostra l'email di una candidatura. Se una bozza c'è già su disco si riapre
    ''' <b>com'era</b> — comprese le correzioni fatte a mano — e l'AI non viene disturbata:
    ''' riscrivere sopra il lavoro di ieri sarebbe il modo peggiore di essere utili.
    ''' </summary>
    Public Async Function MostraLaCandidaturaAsync(candidatura As Opportunita) As Task

        _candidatura = candidatura
        If _candidatura Is Nothing Then Return

        ' Il pannello si riusa da una candidatura all'altra: quel che si sapeva del
        ' destinatario di prima non vale per questa.
        _destinatarioVieneDallAnnuncio = False
        _suggerimenti.SetToolTip(txtDestinatario, Nothing)

        ' E nemmeno quello che le si era scritto. Sotto, la bozza si riempie in due modi
        ' soli — ripresa dal disco, oppure scritta dall'AI — e ScriviLaBozzaAsync ha due
        ' uscite anticipate legittime: manca la chiave, manca la lettera. In quei due casi,
        ' senza questa riga, oggetto e corpo resterebbero quelli della candidatura di
        ' prima: visibili a video sotto il nome di questa, e scritti nel suo email.json al
        ' primo salvataggio. Gli allegati non servono qui: RiempiGliAllegati li rifà.
        _bozza = New BozzaEmail()

        Riempiendo(
            Sub()
                txtDestinatario.Text = ""
                txtOggetto.Text = ""
                txtCorpo.Text = ""
            End Sub)

        RiempiGliAllegati()

        Dim salvata As BozzaEmail = BozzaEmail.DaJson(_candidatura.Email)

        If salvata IsNot Nothing Then
            RiprendiLaBozza(salvata)

            If InUnAltraLingua(salvata) Then
                Racconta("Questa bozza è in " & LinguaDocumenti.Nome(salvata.Lingua).ToLowerInvariant() &
                         ", ma i documenti adesso sono in " &
                         LinguaDocumenti.Nome(_candidatura.Lingua).ToLowerInvariant() &
                         ": premi «Fallo riscrivere» per rifare il messaggio nella loro lingua.",
                         StileApp.Pericolo)
            Else
                Racconta("Bozza ripresa da dove l'avevi lasciata.", StileApp.TestoSecondario)
            End If

            AggiornaComandi()
            Return
        End If

        ' Primo arrivo: se l'annuncio portava un indirizzo, si propone (cap. 07.1). Solo
        ' qui, e mai sopra una bozza ripresa: lì il destinatario è già passato per le mani
        ' dell'utente, e sostituirglielo sarebbe cancellargli una decisione.
        ProponiIlDestinatario()

        Await ScriviLaBozzaAsync()

    End Function

    ''' <summary>
    ''' Vero quando la bozza salvata è in una lingua e i documenti della candidatura in
    ''' un'altra: succede cambiando la tendina di P6 dopo aver già preparato l'email.
    ''' </summary>
    ''' <remarks>
    ''' <para>Non la si riscrive da sé: il messaggio può essere passato per le mani
    ''' dell'utente, e rifarglielo senza chiedere sarebbe cancellargli il lavoro — la stessa
    ''' ragione per cui una bozza salvata non viene mai sovrascritta all'arrivo. Si dice
    ''' com'è, e il bottone per rifarla è lì accanto.</para>
    ''' <para>Una bozza scritta prima che la lingua si annotasse non ce l'ha: allora non si
    ''' sa, e non sapere non è un buon motivo per allarmare.</para>
    ''' </remarks>
    Private Function InUnAltraLingua(salvata As BozzaEmail) As Boolean

        If salvata Is Nothing OrElse String.IsNullOrWhiteSpace(salvata.Lingua) Then Return False
        If _candidatura Is Nothing Then Return False

        Return LinguaDocumenti.PerDocumenti(salvata.Lingua) <>
               LinguaDocumenti.PerDocumenti(_candidatura.Lingua)

    End Function

    ''' <summary>
    ''' La passata anti-slop sul corpo del messaggio (cap. 08), che come altrove non può
    ''' far fallire quel che è già scritto: se inciampa, resta il testo del compositore.
    ''' </summary>
    Private Async Function RifinisciIlMessaggioAsync(corpo As String, lingua As String,
                                                     annulla As CancellationToken) As Task(Of String)

        _ilCorpoNonERifinito = False

        If _rifinitura Is Nothing Then Return corpo

        Racconta("Rifinisco il messaggio…", StileApp.TestoSecondario)

        Try
            Return Await _rifinitura.DelTestoAsync(corpo, lingua, annulla)

        Catch ex As ErroreAi
            ' Come nella pipeline (cap. 08.6): non si tace. Il messaggio c'è lo stesso,
            ' ma è il testo grezzo, e chi legge deve saperlo.
            _ilCorpoNonERifinito = True
            Return corpo
        End Try

    End Function

    ''' <summary>
    ''' Chiede all'AI oggetto e corpo, e li mette nei campi. Gli allegati spuntati fanno
    ''' parte della richiesta: il messaggio li nomina, e nominarne uno che non parte
    ''' sarebbe un'email che si smentisce da sola (cap. 07.1).
    ''' </summary>
    Private Async Function ScriviLaBozzaAsync() As Task

        If _candidatura Is Nothing OrElse _contesto Is Nothing Then Return

        If _compositore Is Nothing Then
            Racconta("Senza chiave API non posso scrivere il messaggio: puoi scriverlo a mano qui sotto.",
                     StileApp.Pericolo)
            Return
        End If

        If _candidatura.Lettera Is Nothing Then
            Racconta("Questa candidatura non ha ancora una lettera: l'email nasce da lì (cap. 07.1).",
                     StileApp.Pericolo)
            Return
        End If

        _annulla = New CancellationTokenSource()
        RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)
        AggiornaComandi()
        Racconta("Sto scrivendo il messaggio…", StileApp.TestoSecondario)

        Try
            ' L'email si scrive nella lingua della candidatura, che è quella in cui la
            ' lettera è già scritta: la fonte è la stessa che consulta P6, così le due
            ' schermate non possono raccontare due lingue diverse (cap. 10.1).
            Dim lingua As String = LinguaDocumenti.PerDocumenti(_candidatura.Lingua)

            Dim proposta = Await _compositore.ComponiAsync(
                _candidatura.Lettera, _candidatura.Annuncio,
                _bozza.AllegatiScelti().Select(Function(a) a.Nome).ToList(), _annulla.Token,
                lingua)

            Dim scritta As BozzaEmail = BozzaEmail.DallaProposta(proposta)

            ' Anche il messaggio passa dall'anti-slop (cap. 07.1): nasce da una lettera già
            ' rifinita, ma il corpo lo riscrive l'AI da capo, e i tic rientrano dalla
            ' finestra. L'oggetto no: è una formula dettata parola per parola dal prompt
            ' (Pool 1.07), e rifinirla vorrebbe dire disfarla.
            scritta.Corpo = Await RifinisciIlMessaggioAsync(scritta.Corpo, lingua, _annulla.Token)

            Riempiendo(
                Sub()
                    txtOggetto.Text = scritta.Oggetto
                    txtCorpo.Text = scritta.Corpo
                End Sub)

            _bozza.Oggetto = scritta.Oggetto
            _bozza.Corpo = scritta.Corpo

            ' La lingua si annota adesso, insieme al testo che l'ha usata: è quella che
            ' domani dirà se questa bozza vale ancora per i documenti che ci sono.
            _bozza.Lingua = lingua

            ' Il destinatario non si tocca: non viene dall'AI, e se l'utente l'ha già
            ' scritto riscriverlo sarebbe cancellargli il lavoro.
            Racconta("Messaggio scritto. Rileggilo: è quello che l'azienda legge per primo." &
                     If(_destinatarioVieneDallAnnuncio,
                        vbLf & "Il destinatario l'ho preso dall'annuncio: controllalo prima di mandare.",
                        "") &
                     If(_ilCorpoNonERifinito,
                        vbLf & "La rifinitura non è riuscita: tengo il testo com'è.",
                        ""),
                     StileApp.TestoSecondario)

        Catch ex As OperationCanceledException
            Racconta("Scrittura annullata.", StileApp.TestoSecondario)

        Catch ex As ErroreAi
            Racconta(ex.Message, StileApp.Pericolo)

        Finally
            _annulla?.Dispose()
            _annulla = Nothing
            RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)
            AggiornaComandi()
        End Try

    End Function

    ''' <summary>
    ''' Mette nel destinatario l'indirizzo che l'annuncio dichiarava, se ne aveva uno.
    ''' </summary>
    ''' <remarks>
    ''' <b>Proporre non è decidere.</b> L'indirizzo finisce nella casella dove l'utente
    ''' scrive, modificabile e cancellabile come se l'avesse digitato lui, e il pannello lo
    ''' dice invece di lasciarlo comparire dal nulla: chi manda una candidatura deve sapere
    ''' <i>perché</i> quel destinatario è lì. Quando l'annuncio non porta niente non si
    ''' scrive niente e non si dice niente — un «non ho trovato l'indirizzo» a ogni email
    ''' sarebbe rumore, visto che la maggior parte degli annunci non lo pubblica.
    ''' </remarks>
    Private Sub ProponiIlDestinatario()

        If _candidatura Is Nothing Then Return

        Dim proposto As String = VistaAnnuncio.IndirizzoPerCandidarsi(_candidatura.Annuncio)
        If proposto.Length = 0 Then Return

        _bozza.Destinatario = proposto
        _destinatarioVieneDallAnnuncio = True

        Riempiendo(Sub() txtDestinatario.Text = proposto)

        ' Il perché sta sulla casella e non nella barra di stato: la barra racconta
        ' l'ultima cosa successa, e fra un istante la scrittura del messaggio ci scriverà
        ' sopra la sua. Un suggerimento resta lì finché quel destinatario è lì.
        _suggerimenti.SetToolTip(txtDestinatario,
                                 "Questo indirizzo l'ho preso dall'annuncio, non l'ho dedotto: controllalo.")

    End Sub

    ''' <summary>
    ''' Rimette nei campi una bozza salvata. Gli allegati non si sostituiscono: quelli veri
    ''' sono i file che <b>ci sono adesso</b> su disco: della bozza si riprendono le
    ''' <b>spunte</b>, e un file sparito nel frattempo non torna in vita per un elenco.
    ''' </summary>
    Private Sub RiprendiLaBozza(salvata As BozzaEmail)

        _bozza.Destinatario = salvata.Destinatario
        _bozza.Oggetto = salvata.Oggetto
        _bozza.Corpo = salvata.Corpo
        _bozza.Lingua = salvata.Lingua

        For Each allegato As AllegatoScelto In _bozza.Allegati
            Dim scelto As AllegatoScelto = salvata.Allegati.FirstOrDefault(
                Function(a) String.Equals(a.Nome, allegato.Nome, StringComparison.OrdinalIgnoreCase))

            If scelto IsNot Nothing Then allegato.Scelto = scelto.Scelto
        Next

        Riempiendo(
            Sub()
                txtDestinatario.Text = _bozza.Destinatario
                txtOggetto.Text = _bozza.Oggetto
                txtCorpo.Text = _bozza.Corpo
                MostraLeSpunte()
            End Sub)

    End Sub

    ' ==================================================================
    ' Gli allegati
    ' ==================================================================

    ''' <summary>
    ''' Riempie l'elenco: prima i documenti generati per questa candidatura — i file della
    ''' sua <c>out\</c>, già spuntati, perché sono il motivo per cui l'email esiste — e poi
    ''' gli <b>attestati</b> della cartella documenti (cap. 05.2), spenti.
    ''' </summary>
    ''' <remarks>
    ''' Gli attestati arrivano spenti di proposito: quali provino qualcosa <i>per questo
    ''' annuncio</i> lo sa l'utente, e allegare a un'azienda tutti i certificati che una
    ''' persona possiede è il modo di far leggere nessuno. Il programma li mette a portata
    ''' di mano; a sceglierli è lui.
    ''' </remarks>
    Private Sub RiempiGliAllegati()

        _bozza.Allegati.Clear()

        For Each percorso As String In DocumentiDellaCandidatura()
            _bozza.Allegati.Add(New AllegatoScelto With {
                .Nome = Path.GetFileName(percorso),
                .Origine = OrigineAllegato.Candidatura,
                .Scelto = ConvieneAllegarlo(percorso)})
        Next

        For Each attestato As DocumentoClassificato In AttestatiDaProporre()
            _bozza.Allegati.Add(New AllegatoScelto With {
                .Nome = attestato.Nome,
                .Origine = OrigineAllegato.Documenti,
                .Scelto = False})
        Next

        Riempiendo(
            Sub()
                lstAllegati.Items.Clear()
                For Each allegato As AllegatoScelto In _bozza.Allegati
                    lstAllegati.Items.Add(EtichettaAllegato(allegato), allegato.Scelto)
                Next
            End Sub)

    End Sub

    ''' <summary>
    ''' Gli attestati riconosciuti nella cartella documenti che <b>ci sono ancora</b>.
    ''' </summary>
    ''' <remarks>
    ''' L'elenco su disco dice cosa c'era l'ultima volta che la cartella è stata letta; a
    ''' dire cosa c'è adesso è solo il disco. Un attestato cancellato nel frattempo non
    ''' deve comparire fra le cose che si possono allegare: è la stessa regola con cui gli
    ''' allegati della bozza si rifanno dai file veri (cap. 07.1).
    ''' </remarks>
    Private Function AttestatiDaProporre() As List(Of DocumentoClassificato)

        If _contesto Is Nothing OrElse Not _contesto.Raccolta.CartellaUtilizzabile Then
            Return New List(Of DocumentoClassificato)
        End If

        Return _contesto.Raccolta.Attestati().
            Where(Function(a) File.Exists(Path.Combine(_contesto.Raccolta.Cartella, a.Nome))).
            ToList()

    End Function

    ''' <summary>
    ''' Come si legge un allegato nell'elenco. Quelli che vengono dalla cartella documenti
    ''' lo dicono: nella stessa lista ci sono file appena generati per questa candidatura e
    ''' file che l'utente ha da anni, e sapere da dove viene ciascuno è ciò che permette di
    ''' spuntarli con cognizione.
    ''' </summary>
    Private Shared Function EtichettaAllegato(allegato As AllegatoScelto) As String

        If allegato.Origine <> OrigineAllegato.Documenti Then Return allegato.Nome

        Return $"{allegato.Nome}  (dai tuoi documenti)"

    End Function

    ''' <summary>I file prodotti per questa candidatura, in ordine di nome.</summary>
    Private Function DocumentiDellaCandidatura() As IEnumerable(Of String)

        If _candidatura Is Nothing OrElse String.IsNullOrWhiteSpace(_candidatura.Cartella) Then
            Return Enumerable.Empty(Of String)()
        End If

        Dim cartella As String = Path.Combine(_candidatura.Cartella, ArchivioOpportunita.NomeCartellaOut)
        If Not Directory.Exists(cartella) Then Return Enumerable.Empty(Of String)()

        ' Il .eml già scritto non si allega a sé stesso: sarebbe un messaggio dentro il
        ' messaggio, e la seconda volta che si preme «Prepara» ci finirebbe dentro.
        Return Directory.EnumerateFiles(cartella).
                         Where(Function(f) Not f.EndsWith(ScrittoreEml.Estensione, StringComparison.OrdinalIgnoreCase)).
                         OrderBy(Function(f) Path.GetFileName(f), StringComparer.CurrentCultureIgnoreCase)

    End Function

    ''' <summary>
    ''' Quali documenti conviene spuntare da soli. Il <b>PDF</b> sì: è il formato che si
    ''' apre uguale dappertutto, ed è quello che si manda a un'azienda. Il DOCX resta lì,
    ''' spento, per chi lo vuole — certi annunci lo chiedono espressamente (cap. 07.1).
    ''' </summary>
    Private Shared Function ConvieneAllegarlo(percorso As String) As Boolean
        Return Path.GetExtension(percorso).Equals(NomiDocumenti.EstensionePdf, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Sub MostraLeSpunte()

        For indice As Integer = 0 To Math.Min(lstAllegati.Items.Count, _bozza.Allegati.Count) - 1
            lstAllegati.SetItemChecked(indice, _bozza.Allegati(indice).Scelto)
        Next

    End Sub

    Private Sub lstAllegati_ItemCheck(sender As Object, e As ItemCheckEventArgs) Handles lstAllegati.ItemCheck

        If _riempimenti > 0 Then Return
        If e.Index < 0 OrElse e.Index >= _bozza.Allegati.Count Then Return

        _bozza.Allegati(e.Index).Scelto = (e.NewValue = CheckState.Checked)

        ' Il messaggio nomina gli allegati: cambiarli senza rifarlo scrivere lascia un
        ' testo che promette un file che non parte più. Non si riscrive da sé — sarebbe
        ' una chiamata all'AI a ogni spunta — ma si dice.
        Racconta("Allegati cambiati: se il messaggio li nomina, fallo riscrivere.", StileApp.TestoSecondario)

    End Sub

    ' ==================================================================
    ' I campi che l'utente scrive
    ' ==================================================================

    Private Sub txtDestinatario_TextChanged(sender As Object, e As EventArgs) Handles txtDestinatario.TextChanged

        If _riempimenti > 0 Then Return

        _bozza.Destinatario = txtDestinatario.Text

        ' Se l'utente ci mette mano, l'indirizzo è suo: il suggerimento che diceva «l'ho
        ' preso dall'annuncio» diventerebbe una bugia, e va tolto insieme al motivo.
        If _destinatarioVieneDallAnnuncio Then
            _destinatarioVieneDallAnnuncio = False
            _suggerimenti.SetToolTip(txtDestinatario, Nothing)
        End If

        AggiornaComandi()

    End Sub

    Private Sub txtOggetto_TextChanged(sender As Object, e As EventArgs) Handles txtOggetto.TextChanged

        If _riempimenti > 0 Then Return
        _bozza.Oggetto = txtOggetto.Text
        AggiornaComandi()

    End Sub

    Private Sub txtCorpo_TextChanged(sender As Object, e As EventArgs) Handles txtCorpo.TextChanged

        If _riempimenti > 0 Then Return
        _bozza.Corpo = txtCorpo.Text
        AggiornaComandi()

    End Sub

    ''' <summary>Come in P2: mentre si riempie, quel che cambia non è una correzione dell'utente.</summary>
    Private Sub Riempiendo(cosa As Action)

        _riempimenti += 1
        Try
            cosa()
        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ' ==================================================================
    ' I comandi
    ' ==================================================================

    Private Async Sub btnRiscrivi_Click(sender As Object, e As EventArgs) Handles btnRiscrivi.Click
        Await ScriviLaBozzaAsync()
    End Sub

    ''' <summary>
    ''' Scrive il file <c>.eml</c> nella cartella della candidatura e lo apre nel programma
    ''' di posta predefinito (cap. 12, A8). Da qui in poi il programma non tocca più niente.
    ''' </summary>
    Private Sub btnPreparaEmail_Click(sender As Object, e As EventArgs) Handles btnPreparaEmail.Click

        Dim percorso As String = PreparaIlMessaggio()
        If percorso Is Nothing Then Return

        Try
            Process.Start(New ProcessStartInfo(percorso) With {.UseShellExecute = True})
            Racconta("Messaggio preparato e aperto nel tuo programma di posta. " &
                     "Quando l'hai spedito, torna qui e dimmelo.", StileApp.TestoSecondario)

        Catch ex As Exception When TypeOf ex Is Win32Exception OrElse
                                   TypeOf ex Is InvalidOperationException

            ' Il file c'è comunque, ed è la cosa che conta: Windows non sa con quale
            ' programma aprirlo, e dirglielo è una cosa che l'utente può fare.
            Racconta($"Il messaggio è scritto in «{percorso}», ma Windows non sa con quale " &
                     "programma aprire un file .eml. Aprilo dal tuo programma di posta.",
                     StileApp.Pericolo)
        End Try

        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Scrive il messaggio e salva la bozza, <b>senza aprirlo</b>; restituisce il percorso
    ''' del file, o <c>Nothing</c> se non si è potuto scrivere (e in quel caso lo dice).
    ''' </summary>
    ''' <remarks>
    ''' Sta in un metodo suo per la stessa ragione di <see cref="SegnaComeInviata"/>: è
    ''' l'unico modo di collaudarlo. Il bottone fa **due** cose — scrive il file e lo
    ''' consegna al programma di posta — e un banco che preme il bottone apre il client
    ''' dell'utente per davvero, su un file di prova che poi cancella. Visto sul serio il
    ''' 2026-08-15: cinque finestre di Outlook rimaste aperte su un «Email_Rossi_S_p_A…»
    ''' che nel frattempo non esisteva più. L'effetto verso il mondo esterno resta nel
    ''' gestore del bottone, dove nessun collaudo lo tocca.
    ''' </remarks>
    Public Function PreparaIlMessaggio() As String

        If _candidatura Is Nothing Then Return Nothing

        Dim percorso As String = PercorsoDelMessaggio()

        Try
            ScrittoreEml.Scrivi(percorso,
                                MittenteDalProfilo(), _bozza.Destinatario,
                                _bozza.Oggetto, _bozza.Corpo, AllegatiDaMandare())

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException

            Racconta($"Non sono riuscita a scrivere il messaggio: {ex.Message}", StileApp.Pericolo)
            Return Nothing
        End Try

        SalvaLaBozza()
        Return percorso

    End Function

    ''' <summary>
    ''' L'utente dichiara di aver spedito: è l'unica prova che il programma può avere
    ''' (cap. 07.3). Da qui la candidatura passa a «inviata», con la data di adesso.
    ''' </summary>
    Private Sub btnHoSpedito_Click(sender As Object, e As EventArgs) Handles btnHoSpedito.Click

        If _candidatura Is Nothing OrElse _contesto Is Nothing Then Return

        Dim risposta As DialogResult = MessageBox.Show(
            "Hai spedito la candidatura dal tuo programma di posta?" & vbLf & vbLf &
            "Se rispondi di sì la segno come inviata, con la data di adesso. " &
            "Il programma non può saperlo da solo: a spedire è il tuo programma di posta.",
            "TrovaLavoro", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)

        If risposta <> DialogResult.Yes Then Return

        SegnaComeInviata()

    End Sub

    ''' <summary>
    ''' L'atto, separato dalla domanda che lo precede: la conferma sta nel bottone, qui c'è
    ''' quel che succede dopo un sì. È anche l'unico modo di collaudarlo — una
    ''' <c>MessageBox</c> in un banco resta lì ad aspettare per sempre.
    ''' </summary>
    Public Sub SegnaComeInviata()

        If _candidatura Is Nothing OrElse _contesto Is Nothing Then Return

        Try
            SalvaLaBozza(avanzaAInviata:=True)

            Racconta($"Segnata come inviata il {Date.Now:dd/MM/yyyy} alle {Date.Now:HH:mm}.",
                     StileApp.TestoSecondario)

            RaiseEvent CandidaturaInviata(Me, EventArgs.Empty)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is InvalidOperationException

            Racconta($"Non sono riuscita a segnarla come inviata: {ex.Message}", StileApp.Pericolo)
        End Try

        AggiornaComandi()

    End Sub

    ' ==================================================================
    ' La cartella documenti dell'utente (cap. 05.2)
    ' ==================================================================

    Private Async Sub btnDocumenti_Click(sender As Object, e As EventArgs) Handles btnDocumenti.Click
        Await GestisciIDocumentiAsync()
    End Sub

    ''' <summary>
    ''' Il giro della cartella documenti: sceglierla, farla leggere, confermare quel che
    ''' ci si è riconosciuto dentro. Alla fine gli attestati confermati compaiono fra gli
    ''' allegati proposti, spenti.
    ''' </summary>
    ''' <remarks>
    ''' <para>Il giro sta <b>qui</b> e non nella finestra perché è qui che si sa aspettare
    ''' l'AI e annullarla (cap. 02.6): la finestra chiede — «rileggi», «cambia cartella» —
    ''' e il pannello esegue, poi la riapre. Un ciclo, non una catena di finestre che si
    ''' aprono l'una sull'altra.</para>
    ''' <para>Su «annulla» la raccolta si <b>rilegge da disco</b>: le correzioni si
    ''' scrivono nell'oggetto vivo mentre l'utente le fa, e chi annulla si aspetta che non
    ''' resti niente.</para>
    ''' </remarks>
    Public Async Function GestisciIDocumentiAsync() As Task

        If _contesto Is Nothing Then Return

        If Not _contesto.Raccolta.CartellaUtilizzabile Then
            If Not Await ScegliLaCartellaAsync() Then Return
        End If

        Dim finito As Boolean = False

        While Not finito

            Select Case FinestraDocumenti.Chiedi(Me, _contesto.Raccolta)

                Case EsitoDocumenti.Confermato
                    SalvaLaRaccolta()
                    RiempiGliAllegati()
                    MostraLeSpunte()
                    Racconta($"Documenti confermati: {_contesto.Raccolta.Attestati().Count} attestati " &
                             "da spuntare qui sopra quando servono.", StileApp.TestoSecondario)
                    finito = True

                Case EsitoDocumenti.Rileggere
                    Await LeggiLaCartellaAsync()

                Case EsitoDocumenti.CambiaCartella
                    If Not Await ScegliLaCartellaAsync() Then finito = True

                Case Else
                    ' Annullato: quel che è stato toccato non era ancora suo.
                    _contesto.RileggiLaRaccolta()
                    finito = True

            End Select

        End While

    End Function

    ''' <summary>
    ''' Chiede la cartella all'utente e la fa leggere. Falso se ha rinunciato.
    ''' </summary>
    Private Async Function ScegliLaCartellaAsync() As Task(Of Boolean)

        Dim scelta As String

        Using dialogo As New FolderBrowserDialog()

            dialogo.Description = "Scegli la cartella dove tieni i tuoi documenti: CV, attestati, certificati."
            dialogo.UseDescriptionForTitle = True
            dialogo.ShowNewFolderButton = False

            If _contesto.Raccolta.CartellaUtilizzabile Then
                dialogo.SelectedPath = _contesto.Raccolta.Cartella
            End If

            If dialogo.ShowDialog(Me) <> DialogResult.OK Then Return False
            scelta = dialogo.SelectedPath

        End Using

        ' Cartella nuova, elenco nuovo: le categorie di prima erano di altri file, e
        ' tenerle vorrebbe dire proporre come attestato un nome che qui non c'è.
        If Not String.Equals(scelta, _contesto.Raccolta.Cartella, StringComparison.OrdinalIgnoreCase) Then
            _contesto.Raccolta.Documenti.Clear()
            _contesto.Raccolta.CvPiuRecente = ""
        End If

        _contesto.Raccolta.Cartella = scelta

        Await LeggiLaCartellaAsync()
        Return True

    End Function

    ''' <summary>
    ''' Legge la cartella e, se l'AI c'è, le fa proporre una categoria per ogni file.
    ''' Senza AI la lettura si fa lo stesso: i file si vedono, e a dire cos'è ciascuno
    ''' resta l'utente.
    ''' </summary>
    Private Async Function LeggiLaCartellaAsync() As Task

        Dim lasciatiFuori As Integer
        Dim trovati As List(Of FileTrovato) = ScansioneDocumenti.Leggi(
            _contesto.Raccolta.Cartella, lasciatiFuori)

        _contesto.Raccolta.AllineaAiFile(trovati)
        _contesto.Raccolta.Letta = Date.Now

        If trovati.Count = 0 Then
            Racconta("In quella cartella non ci sono file che io sappia leggere (PDF, DOCX, TXT, MD).",
                     StileApp.Pericolo)
            Return
        End If

        ' Un elenco troncato in silenzio si leggerebbe come «nella cartella non c'era
        ' altro»: chi ha duecento file deve sapere che ne ho guardati sessanta.
        If lasciatiFuori > 0 Then
            Racconta($"La cartella ha più di {ScansioneDocumenti.MassimoFile} documenti: " &
                     $"ne ho letti i primi {trovati.Count} in ordine di nome, {lasciatiFuori} sono rimasti fuori.",
                     StileApp.Pericolo)
        End If

        If _classificatore Is Nothing Then
            Racconta("Senza chiave API non posso riconoscerli: l'elenco c'è, la categoria mettila tu.",
                     StileApp.Pericolo)
            Return
        End If

        _annulla = New CancellationTokenSource()
        RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)
        AggiornaComandi()
        Racconta($"Sto guardando i {trovati.Count} documenti della cartella…", StileApp.TestoSecondario)

        Try
            Dim proposta = Await _classificatore.ClassificaAsync(trovati, _annulla.Token)
            _contesto.Raccolta.PrendiLaProposta(proposta, trovati)

            Racconta("Ecco cosa ho riconosciuto: correggi quel che ho sbagliato e conferma.",
                     StileApp.TestoSecondario)

        Catch ex As OperationCanceledException
            Racconta("Lettura annullata: l'elenco resta com'era.", StileApp.TestoSecondario)

        Catch ex As ErroreAi
            Racconta(ex.Message, StileApp.Pericolo)

        Finally
            _annulla?.Dispose()
            _annulla = Nothing
            RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)
            AggiornaComandi()
        End Try

    End Function

    ''' <summary>Scrive la raccolta su disco, dicendolo se non ci riesce.</summary>
    Private Sub SalvaLaRaccolta()

        Try
            _contesto.ArchivioRaccolta.Salva(_contesto.Raccolta)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException

            Racconta($"Non sono riuscita a salvare l'elenco dei documenti: {ex.Message}", StileApp.Pericolo)
        End Try

    End Sub

    Private Sub btnTornaAiDocumenti_Click(sender As Object, e As EventArgs) Handles btnTornaAiDocumenti.Click

        ' Uscendo si salva quel che c'è: il destinatario scritto a metà e le spunte sono
        ' lavoro dell'utente, e nessuno gliel'ha chiesto di confermarlo.
        SalvaLaBozza()
        RaiseEvent TornaAiDocumenti(Me, EventArgs.Empty)

    End Sub

    ''' <summary>
    ''' Dalla barra di navigazione in cima si esce da qui senza passare dal bottone qui
    ''' sopra: la bozza si salva lo stesso (<see cref="IPannelloCheSalvaUscendo"/>).
    ''' </summary>
    ''' <remarks>
    ''' <para>Vale la stessa ragione del bottone — destinatario, spunte e messaggio
    ''' riscritto sono lavoro dell'utente — con un'aggravante: da lì la perdita era
    ''' <b>silenziosa</b>, e rientrando P7 rileggeva <c>email.json</c> mostrando la bozza
    ''' vecchia come se fosse l'ultima. Il messaggio riscritto, poi, era costato una
    ''' chiamata all'AI.</para>
    ''' <para>Mentre l'AI scrive non si salva: quello che c'è in memoria è una bozza a
    ''' metà, e metterla su disco sopra quella buona sarebbe peggio del male. Non dovrebbe
    ''' potersi verificare — la barra è spenta finché il lavoro è in corso — ma questo
    ''' metodo non può contare su chi lo chiama.</para>
    ''' <para>Non solleva mai: qui si sta cambiando pannello, e un disco che non si lascia
    ''' scrivere non deve inchiodare la navigazione. Si dice nella fascia di stato, dove il
    ''' pannello dice le altre cose che non gli riescono.</para>
    ''' </remarks>
    Private Sub SalvaUscendo() Implements IPannelloCheSalvaUscendo.SalvaUscendo

        If AiAlLavoro Then Return

        Try
            SalvaLaBozza()

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException

            Racconta($"Non sono riuscita a salvare la bozza: {ex.Message}", StileApp.Pericolo)
        End Try

    End Sub

    ''' <summary>
    ''' Salva la bozza dentro la candidatura, e con lei tutta la cartella.
    ''' </summary>
    ''' <param name="avanzaAInviata">
    ''' Se, insieme, la candidatura passa allo stato «inviata». È un parametro e non due
    ''' metodi perché il salvataggio è uno solo: scrivere lo stato senza la bozza — o il
    ''' contrario — lascerebbe su disco due verità diverse.
    ''' </param>
    Private Sub SalvaLaBozza(Optional avanzaAInviata As Boolean = False)

        If _candidatura Is Nothing OrElse _contesto Is Nothing Then Return

        _candidatura.Email = _bozza.ComeJson()

        ' Si avanza solo se la macchina degli stati lo consente, e non «se non è già
        ' inviata»: da T9c una candidatura può essere andata **oltre** — ha un esito — e
        ' ripremere «L'ho spedita» per rimandare la stessa email chiedeva un passo
        ' all'indietro che non esiste (cap. 07.3). Chiederlo sollevava.
        If avanzaAInviata AndAlso
           StatiOpportunita.Consentita(_candidatura.Stato, StatoOpportunita.Inviata) Then
            _candidatura.Avanza(StatoOpportunita.Inviata, Date.Now)
        End If

        _contesto.Opportunita.Salva(_candidatura)

        If avanzaAInviata Then AnnotaNelRegistro()

    End Sub

    ''' <summary>
    ''' Mette lo stato nuovo anche nell'<b>indice</b> delle candidature, come fanno P4
    ''' quando scarta e P6 quando genera.
    ''' </summary>
    ''' <remarks>
    ''' <para>Senza questo, la cartella dice «inviata» e la Home continua a mostrare
    ''' «generata»: l'indice si fida di sé stesso finché l'insieme delle cartelle combacia,
    ''' e un cambio di stato dentro una cartella non lo fa scattare (cap. 07.3). La verità
    ''' resta sul disco, ma quel che l'utente guarda no — e una promessa mantenuta solo
    ''' dove non si guarda non è mantenuta (cap. 12.7).</para>
    ''' <para>Il suo <c>Try</c> è separato da quello del salvataggio, come in P4 e P6: un
    ''' indice che non si lascia scrivere non è una candidatura persa, e dirlo come se lo
    ''' fosse sarebbe un falso allarme.</para>
    ''' </remarks>
    Private Sub AnnotaNelRegistro()

        Try
            _contesto.Registro.Annota(_candidatura)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
        End Try

    End Sub

    ''' <summary>Dove va scritto il messaggio: accanto ai documenti che porta con sé.</summary>
    Private Function PercorsoDelMessaggio() As String

        Dim nome As String = NomiDocumenti.Lettera(_candidatura.Azienda, _candidatura.Creata, _candidatura.Lingua).
            Replace("Lettera", "Email")

        Return Path.Combine(_candidatura.Cartella, ArchivioOpportunita.NomeCartellaOut,
                            nome & ScrittoreEml.Estensione)

    End Function

    ''' <summary>Gli allegati spuntati, già risolti in file veri.</summary>
    Private Function AllegatiDaMandare() As List(Of AllegatoEmail)

        Dim daMandare As New List(Of AllegatoEmail)

        For Each allegato As AllegatoScelto In _bozza.AllegatiScelti()

            Dim percorso As String = BozzaEmail.PercorsoDi(
                allegato, _candidatura.Cartella, _contesto?.Raccolta.Cartella)

            ' Un file sparito nel frattempo non ferma il messaggio: l'utente ha appena
            ' guardato l'elenco, e bloccare tutto per un file in meno sarebbe sproporzionato.
            ' Che manchi si vede — l'elenco lo dirà alla prossima apertura.
            If percorso IsNot Nothing AndAlso File.Exists(percorso) Then
                daMandare.Add(New AllegatoEmail(percorso, allegato.Nome))
            End If

        Next

        Return daMandare

    End Function

    ''' <summary>
    ''' L'indirizzo di chi si candida, preso dal profilo. Se il profilo non ce l'ha, il
    ''' messaggio parte senza mittente e a metterlo sarà il programma di posta, che
    ''' l'account ce l'ha: meglio così che inventarne uno.
    ''' </summary>
    Private Function MittenteDalProfilo() As String

        If _contesto Is Nothing OrElse Not _contesto.Archivio.Esiste Then Return ""

        Try
            Return If(_contesto.Archivio.Carica().Contatti?.Email, "")
        Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is InvalidDataException
            Return ""
        End Try

    End Function

    ' ==================================================================
    ' Aspetto, spazio e stato dei comandi
    ' ==================================================================

    ''' <inheritdoc/>
    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        _ingombroLogo = ingombro
        pnlAzioni.Padding = New Padding(ingombro.Width + StileApp.DistanzaControlli, 0, 0, 0)

        DisponiLeAzioni()

    End Sub

    Private Sub DisponiLeAzioni()

        If _comandi Is Nothing Then
            _comandi = New FasciaDeiComandi(pnlAzioni)
            _comandi.ASinistra(btnTornaAiDocumenti, btnRiscrivi, btnDocumenti)
            _comandi.ADestra(btnHoSpedito, btnPreparaEmail)
        End If

        _comandi.Disponi(Math.Max(AltezzaMinimaAzioni, _ingombroLogo.Height))

    End Sub

    Private Sub PannelloEmail_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        DisponiLeAzioni()
    End Sub

    Private Sub VestiIBottoni()

        StileApp.VestiBottone(btnPreparaEmail, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnHoSpedito, LivelloBottone.SicuroPositivo)
        StileApp.VestiBottone(btnTornaAiDocumenti, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnRiscrivi, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnDocumenti, LivelloBottone.Esplorativo)

    End Sub

    ''' <summary>
    ''' Chi può fare cosa, adesso. Le due condizioni che contano: senza un messaggio non
    ''' c'è niente da preparare, e senza averlo preparato non c'è niente da spedire — un
    ''' «l'ho spedita» premuto prima segnerebbe come inviata una candidatura che non è mai
    ''' uscita di qui.
    ''' </summary>
    Private Sub AggiornaComandi()

        Dim occupato As Boolean = AiAlLavoro
        Dim conCandidatura As Boolean = _candidatura IsNot Nothing

        btnRiscrivi.Enabled = conCandidatura AndAlso Not occupato AndAlso
                              _compositore IsNot Nothing

        btnPreparaEmail.Enabled = conCandidatura AndAlso Not occupato AndAlso
                                  Not String.IsNullOrWhiteSpace(_bozza.Corpo)

        btnHoSpedito.Enabled = conCandidatura AndAlso Not occupato AndAlso MessaggioGiaScritto()

        btnTornaAiDocumenti.Enabled = Not occupato

        ' La cartella documenti non dipende dalla candidatura: si può sistemare anche
        ' prima di avere un'email da mandare, ed è quel che conviene fare una volta sola.
        btnDocumenti.Enabled = Not occupato AndAlso _contesto IsNot Nothing

        If Not btnPreparaEmail.Enabled AndAlso conCandidatura AndAlso Not occupato Then
            _suggerimenti.SetToolTip(btnPreparaEmail, "Prima serve un messaggio: fallo scrivere o scrivilo tu.")
        Else
            _suggerimenti.SetToolTip(btnPreparaEmail, Nothing)
        End If

        If Not btnHoSpedito.Enabled AndAlso conCandidatura AndAlso Not occupato Then
            _suggerimenti.SetToolTip(btnHoSpedito, "Prima prepara l'email: si spedisce dal programma di posta.")
        Else
            _suggerimenti.SetToolTip(btnHoSpedito, Nothing)
        End If

    End Sub

    ''' <summary>Se il file del messaggio esiste già su disco.</summary>
    Private Function MessaggioGiaScritto() As Boolean

        If _candidatura Is Nothing OrElse String.IsNullOrWhiteSpace(_candidatura.Cartella) Then Return False
        Return File.Exists(PercorsoDelMessaggio())

    End Function

    Private Sub Racconta(testo As String, colore As Color)

        lblStatoEmail.Text = testo
        lblStatoEmail.ForeColor = colore

    End Sub

End Class
