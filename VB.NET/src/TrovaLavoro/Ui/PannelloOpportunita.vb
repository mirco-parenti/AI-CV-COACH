Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

''' <summary>
''' Pannello P4 — l'opportunità (cap. 03.6; cap. 12, A5): l'annuncio come l'AI l'ha
''' capito, il match in stelle, i giudizi voce per voce e le note che li spiegano.
''' </summary>
''' <remarks>
''' <para><b>La fascia d'ingresso</b> in cima è la porta da cui un annuncio entra: fino a
''' T5 è l'unica, e anche dopo resta la strada di chi ha in mano un testo e basta
''' (cap. 12.3). A confronto avvenuto si richiude, per lasciare spazio ai giudizi.</para>
''' <para><b>«Analizza» fa due passi, non uno.</b> Fra l'analisi dell'annuncio e il
''' confronto l'utente non deve decidere niente: la decisione vera — generare o no i
''' documenti — viene dopo, quando le stelle si vedono (cap. 12, A5→A7). I due passi si
''' chiamano perciò di fila, e l'attesa dice a che punto è.</para>
''' <para><b>Qui non si giudica.</b> I giudizi sono dell'AI, il punteggio è di
''' <see cref="CalcoloMatch"/>, e ciò che si mostra è la vista di sola lettura
''' <see cref="VistaConfronto"/>: il pannello disegna, non decide.</para>
''' </remarks>
Public Class PannelloOpportunita
    Implements IPannelloArea

    ''' <summary>Sotto questa altezza la fascia delle azioni non scende: i bottoni ci devono stare.</summary>
    Private Const AltezzaMinimaAzioni As Integer = 60

    ''' <summary>Quante stelle ha la scala: quelle piene e quelle vuote insieme.</summary>
    Private Const StelleDellaScala As Integer = 5

    Private ReadOnly _suggerimenti As New ToolTip()

    Private _contesto As ContestoApp

    ''' <summary>
    ''' Chi conduce i passi. Di norma è quella del motore; il banco ne passa una coi
    ''' mestieri finti e fa girare il pannello senza rete.
    ''' </summary>
    Private _pipeline As PipelineCandidatura

    ''' <summary>La candidatura in mostra; <c>Nothing</c> finché non se n'è analizzata una.</summary>
    Private _opportunita As Opportunita

    ''' <summary>Il confronto come lo si disegna; <c>Nothing</c> se non c'è ancora.</summary>
    Private _vista As VistaConfronto

    ''' <summary>Il filo per annullare l'attesa; <c>Nothing</c> se non c'è niente in volo.</summary>
    Private _annulla As CancellationTokenSource

    ''' <summary>
    ''' Se la fascia d'ingresso è aperta. È un campo e non si legge da
    ''' <c>pnlIngresso.Visible</c>: in WinForms la visibilità di un figlio è <b>falsa</b>
    ''' finché il pannello che lo contiene non è a sua volta mostrato — e i pannelli
    ''' dell'area centrale nascono tutti nascosti (cap. 03.4). Chiedendolo al controllo,
    ''' all'avvio la fascia risultava chiusa mentre era spalancata, e «Nuovo annuncio»
    ''' compariva acceso quando non aveva niente da riaprire.
    ''' </summary>
    Private _fasciaAperta As Boolean = True

    ''' <summary>
    ''' L'utente chiede i documenti per l'opportunità in mostra: la finestra porta in
    ''' vista P6. Il pannello non conosce gli altri pannelli — dice cosa vuole.
    ''' </summary>
    Public Event DocumentiRichiesti As EventHandler

    ''' <summary>
    ''' L'AI ha cominciato o finito di lavorare: «mentre l'AI lavora non si esce» vale
    ''' anche per la barra di navigazione, che questo pannello non può spegnere da sé
    ''' (cap. 02.6).
    ''' </summary>
    Public Event LavoroAiCambiato As EventHandler

    Public Sub New()

        InitializeComponent()

        ' Come in P2: il ToolTip nasce qui e muore col pannello, invece di restare un
        ' componente orfano nel designer.
        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        VestiIBottoni()
        DichiaraLeTappeCheMancano()
        MostraLaValutazione(Nothing)
        AggiornaComandi()

    End Sub

    ''' <summary>Collega il pannello al motore e dice da dove si comincia.</summary>
    ''' <param name="contesto">Il motore montato all'avvio.</param>
    ''' <param name="pipeline">
    ''' Chi conduce i passi. Di norma si omette ed è quella del motore; il banco passa qui
    ''' la sua, coi mestieri finti, e fa girare il pannello senza rete.
    ''' </param>
    Public Sub Collega(contesto As ContestoApp, Optional pipeline As PipelineCandidatura = Nothing)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))

        _contesto = contesto
        _pipeline = If(pipeline, contesto.Pipeline)

        RaccontaDaDoveSiComincia()
        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Ogni volta che il pannello entra in vista torna a chiedere com'è messo il mondo.
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Collega"/> passa una volta sola, all'avvio, e lì il profilo poteva non
    ''' esserci ancora. Ma il primo giro vero è proprio quello: si importa il CV in P2, lo
    ''' si salva, e si viene qui — dove fino a ieri si leggeva «prima serve il tuo profilo»
    ''' con «Analizza» spento, per un profilo appena salvato. La condizione la sa
    ''' l'archivio, che guarda il disco a ogni domanda: bastava tornare a chiedergliela.
    ''' Se un'opportunità è già in mostra, il racconto non si tocca: quella riga dice a che
    ''' punto è il confronto, e riscriverla come se si ricominciasse sarebbe una bugia.
    ''' </remarks>
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)

        MyBase.OnVisibleChanged(e)

        If Not Visible OrElse _contesto Is Nothing Then Return

        If _opportunita Is Nothing Then RaccontaDaDoveSiComincia()
        AggiornaComandi()

    End Sub

    ''' <summary>L'opportunità in mostra; <c>Nothing</c> se non ce n'è ancora una.</summary>
    Public ReadOnly Property Candidatura As Opportunita
        Get
            Return _opportunita
        End Get
    End Property

    ''' <summary>Se in questo momento una chiamata all'AI è in volo.</summary>
    Public ReadOnly Property AiAlLavoro As Boolean
        Get
            Return _annulla IsNot Nothing
        End Get
    End Property

    ''' <summary>Annulla l'attesa in corso, se c'è: è la via pulita della chiusura.</summary>
    Public Sub AnnullaIlLavoro()
        _annulla?.Cancel()
    End Sub

    ' ==================================================================
    ' I due passi: leggere l'annuncio, confrontarlo col profilo
    ' ==================================================================

    ''' <summary>
    ''' Legge l'annuncio incollato e lo confronta col profilo. È il metodo che preme
    ''' «Analizza»; il banco lo chiama direttamente, perché di un gestore di clic non si
    ''' può aspettare la fine.
    ''' </summary>
    Public Async Function AnalizzaLAnnuncioAsync() As Task

        If AiAlLavoro Then Return

        Dim testo As String = txtAnnuncio.Text.Trim()
        If testo = "" Then
            RaccontaLoStato("Incolla il testo dell'annuncio, poi premi «Analizza».", StileApp.TestoSecondario)
            Return
        End If

        ' Un testo incollato non ha provenienza: arriva da un'email, da un messaggio, da
        ' uno screenshot letto altrove. Fonte e link restano vuoti, e va bene così.
        Await AvviaIPassiAsync(testo, fonte:=Nothing, link:=Nothing).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' L'annuncio catturato dalla pagina che l'utente stava guardando (cap. 06.4; cap. 12,
    ''' A4): stessa strada del testo incollato, con in più la provenienza.
    ''' </summary>
    ''' <remarks>
    ''' Il testo <b>si vede</b>: entra nella casella della fascia d'ingresso, dove l'utente
    ''' può leggerlo, correggerlo e rilanciare. È la stessa onestà con cui il resto del
    ''' programma mostra ciò che manda all'AI — una cattura che analizzasse in silenzio
    ''' qualcosa di invisibile chiederebbe di fidarsi al buio.
    ''' </remarks>
    Public Async Function AnalizzaIlCatturatoAsync(testo As String, fonte As String, link As String) As Task

        If AiAlLavoro Then Return

        FasciaDIngresso(aperta:=True)
        txtAnnuncio.Text = If(testo, String.Empty)

        Await AvviaIPassiAsync(If(testo, String.Empty).Trim(), fonte, link).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' Le condizioni che valgono comunque — l'AI, il profilo — e poi i due passi.
    ''' </summary>
    Private Async Function AvviaIPassiAsync(testo As String, fonte As String, link As String) As Task

        If testo = "" Then Return

        If _pipeline Is Nothing Then
            RaccontaLoStato(MotivoSenzaAi(), StileApp.Pericolo)
            Return
        End If

        Dim profilo As Profilo = LeggiIlProfilo()
        If profilo Is Nothing Then Return

        Await ConducoIDuePassiAsync(_pipeline, testo, profilo, fonte, link).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' I due passi con la loro attesa: l'annuncio, poi il confronto. Ogni inciampo si
    ''' racconta dov'è successo, e quel che si è già ottenuto non si butta.
    ''' </summary>
    Private Async Function ConducoIDuePassiAsync(pipeline As PipelineCandidatura,
                                                 testo As String, profilo As Profilo,
                                                 fonte As String, link As String) As Task

        Using filo As New CancellationTokenSource()

            _annulla = filo
            LavoroInCorso(True)

            Try
                RaccontaLoStato("Leggo l'annuncio… (1 di 2)", StileApp.TestoSecondario)
                Dim candidatura As Opportunita = Await pipeline.AnalizzaAsync(testo, filo.Token).ConfigureAwait(True)

                candidatura.Fonte = fonte
                candidatura.Link = link

                ' Lo schema vuoto è il modo in cui il prompt dice «questo non è un
                ' annuncio» (cap. 06.4): ci si ferma qui, senza pagare il confronto e
                ' senza scrivere su disco una candidatura che non esiste. Il testo resta
                ' nella casella, che è da dove si riprova.
                If candidatura.AnnuncioVuoto Then
                    RaccontaLoStato(NonSembraUnAnnuncio(link), StileApp.TestoSecondario)
                    Return
                End If

                RaccontaLoStato("Confronto l'annuncio con il tuo profilo… (2 di 2)", StileApp.TestoSecondario)
                Await pipeline.ConfrontaAsync(candidatura, profilo, filo.Token).ConfigureAwait(True)

                candidatura.VersioneProfilo = VersioneInUso()

                _opportunita = candidatura
                MostraLOpportunita()
                FasciaDIngresso(aperta:=False)

                RaccontaLoStato(Archivia(candidatura), StileApp.TestoSecondario)

            Catch ex As OperationCanceledException
                RaccontaLoStato("Analisi annullata: non ho scritto niente.", StileApp.TestoSecondario)

            Catch ex As ErroreAi
                ' Il messaggio è già quello da mostrare, e il testo incollato resta dov'è:
                ' si riprova senza doverlo ritrovare.
                RaccontaLoStato(ex.Message & vbLf & "Il testo dell'annuncio è ancora qui: puoi riprovare.",
                                StileApp.Pericolo)

            Finally
                _annulla = Nothing
                LavoroInCorso(False)
            End Try

        End Using

    End Function

    ''' <summary>
    ''' Il rifiuto garbato di cap. 06.4, detto in modo diverso a seconda di come il testo
    ''' è arrivato: chi ha catturato una pagina va rimandato al singolo annuncio, chi ha
    ''' incollato un testo va rimandato al testo. Dire «pagina di elenco» a chi non ha
    ''' aperto nessuna pagina sarebbe un consiglio che non si può seguire.
    ''' </summary>
    Private Shared Function NonSembraUnAnnuncio(link As String) As String

        If String.IsNullOrWhiteSpace(link) Then
            Return "In questo testo non ho trovato un annuncio di lavoro. " &
                   "Controlla di aver incollato l'annuncio per intero, poi riprova."
        End If

        Return "In questa pagina non ho trovato un annuncio: sembra un elenco di risultati, " &
               "una home o una schermata di accesso. Apri il singolo annuncio e ricattura."

    End Function

    ''' <summary>
    ''' Scrive l'opportunità nella sua cartella e ne restituisce il racconto per la barra.
    ''' Se la scrittura non riesce, i giudizi restano a video lo stesso: quello che si è
    ''' pagato all'AI non si butta per un disco che non collabora — ma non si finge
    ''' nemmeno che sia al sicuro.
    ''' </summary>
    Private Function Archivia(candidatura As Opportunita) As String

        If _contesto Is Nothing Then Return RiassuntoDelMatch()

        Try
            _contesto.Opportunita.Salva(candidatura)
            Return RiassuntoDelMatch() & vbLf & $"Salvata in «{candidatura.Cartella}»."

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            Return RiassuntoDelMatch() & vbLf &
                   $"Attenzione: non sono riuscita a salvarla su disco ({ex.Message})."
        End Try

    End Function

    ''' <summary>
    ''' La versione di profilo con cui si sta confrontando: è ciò che tiene spiegabile un
    ''' CV già inviato anche a profilo evoluto (cap. 11.1). L'ultima versione dello
    ''' storico è quella corrente, perché il profilo si salva sempre da una sola porta.
    ''' </summary>
    Private Function VersioneInUso() As String

        If _contesto Is Nothing Then Return Nothing

        Try
            Return _contesto.Archivio.Versioni().LastOrDefault()

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' Non sapere da quale versione nasce è un peccato veniale: non vale il
            ' prezzo di fermare un confronto già pagato.
            Return Nothing
        End Try

    End Function

    ''' <summary>
    ''' Il profilo su disco, o <c>Nothing</c> se non c'è o non si lascia leggere — con il
    ''' motivo già raccontato. Senza profilo non c'è niente da confrontare: è la scheda P2
    ''' il posto da cui si comincia (cap. 12, A2).
    ''' </summary>
    Private Function LeggiIlProfilo() As Profilo

        If _contesto Is Nothing OrElse Not _contesto.Archivio.Esiste Then
            RaccontaLoStato(
                "Prima serve il tuo profilo: costruiscilo nella scheda «Profilo», poi torna qui.",
                StileApp.TestoSecondario)
            Return Nothing
        End If

        Try
            Return _contesto.Archivio.Carica()

        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                   OrElse TypeOf ex Is UnauthorizedAccessException
            RaccontaLoStato(
                $"Il profilo c'è ma non si lascia leggere: {ex.Message}" & vbLf &
                "Aprilo dalla scheda «Profilo»: lì trovi come rimediare.",
                StileApp.Pericolo)
            Return Nothing
        End Try

    End Function

    ' ==================================================================
    ' Quel che si vede: annuncio, stelle, giudizi
    ' ==================================================================

    ''' <summary>Mette a video l'opportunità: l'annuncio letto e la sua valutazione.</summary>
    Private Sub MostraLOpportunita()

        txtAnnuncioLetto.Text = VistaAnnuncio.Riassunto(_opportunita.Annuncio)
        MostraLaValutazione(VistaConfronto.Da(_opportunita))

    End Sub

    ''' <summary>
    ''' Disegna la valutazione: le stelle grandi, le note che le spiegano, i giudizi voce
    ''' per voce. Con <c>Nothing</c> ripulisce, che è come nasce il pannello.
    ''' </summary>
    Private Sub MostraLaValutazione(vista As VistaConfronto)

        _vista = vista

        lvwGiudizi.BeginUpdate()
        lvwGiudizi.Items.Clear()

        If vista Is Nothing Then
            lblStelle.Text = ""
            MostraLaNota("", StileApp.TestoSecondario)
            lvwGiudizi.EndUpdate()
            MostraLaLetturaDInsieme()
            AggiornaComandi()
            Return
        End If

        lblStelle.Text = StelleScritte(vista.Stelle)

        For Each giudizio As GiudizioMostrato In vista.Giudizi
            lvwGiudizi.Items.Add(RigaDelGiudizio(giudizio))
        Next

        lvwGiudizi.EndUpdate()

        MostraLaNota(NotaDaMostrare(vista), If(vista.GateEliminatorio OrElse vista.Sconsigliata,
                                               StileApp.Pericolo, StileApp.TestoSecondario))
        MostraLaLetturaDInsieme()
        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Una riga dell'elenco. Il ⛔ sta in coda al requisito, come nel prototipo, e il
    ''' colore fa il resto: rosso per il paletto rimasto scoperto — è lui che ha messo il
    ''' tetto al punteggio — e grigio per ciò che non si è potuto valutare.
    ''' </summary>
    Private Shared Function RigaDelGiudizio(giudizio As GiudizioMostrato) As ListViewItem

        Dim requisito As String = giudizio.Requisito
        If giudizio.Eliminatorio Then requisito &= " ⛔"

        Dim riga As New ListViewItem({giudizio.Simbolo, requisito, giudizio.Peso, giudizio.NomeEsito}) With {
            .Tag = giudizio}

        If giudizio.Eliminatorio AndAlso giudizio.Esito = EsitoGiudizio.NonSoddisfatto Then
            riga.ForeColor = StileApp.Pericolo
        ElseIf giudizio.Esito = EsitoGiudizio.NonDeterminabile Then
            riga.ForeColor = StileApp.TestoSecondario
        End If

        Return riga

    End Function

    ''' <summary>
    ''' Le stelle come si leggono: quelle piene, quelle vuote, e il numero esatto accanto.
    ''' </summary>
    Private Shared Function StelleScritte(quante As Double?) As String

        If Not quante.HasValue Then Return "Match non calcolabile"

        Dim piene As Integer = CInt(Math.Floor(quante.Value))
        piene = Math.Max(0, Math.Min(StelleDellaScala, piene))

        Return New String("★"c, piene) & New String("☆"c, StelleDellaScala - piene) &
               "   " & quante.Value.ToString("0.0", CultureInfo.CurrentCulture) & " su 5"

    End Function

    ''' <summary>
    ''' Cosa c'è da sapere sul punteggio: la nota del calcolo — lo scarto tagliato, il
    ''' tetto del requisito eliminatorio — e, sotto soglia, il consiglio di non
    ''' candidarsi. Che resta un consiglio: la scelta è dell'utente (cap. 12, A5.3).
    ''' </summary>
    Private Shared Function NotaDaMostrare(vista As VistaConfronto) As String

        Dim righe As New List(Of String)

        If Not String.IsNullOrWhiteSpace(vista.Nota) Then righe.Add(vista.Nota)

        If vista.Sconsigliata Then
            righe.Add("Con un match così basso i documenti verrebbero comunque onesti, ma poco " &
                      "spendibili su questo annuncio: puoi generarli lo stesso oppure fermarti qui.")
        End If

        Return String.Join(vbLf, righe)

    End Function

    Private Sub MostraLaNota(testo As String, colore As Color)

        lblNota.Text = testo
        lblNota.ForeColor = colore

    End Sub

    ''' <summary>
    ''' Il riquadro in basso quando nessuna riga è scelta: la sintesi onesta del match.
    ''' </summary>
    Private Sub MostraLaLetturaDInsieme()

        lblEtichettaSpiegazione.Text = "Lettura d'insieme"
        txtSpiegazione.Text = If(_vista Is Nothing, "", _vista.LetturaInsieme)

    End Sub

    ''' <summary>
    ''' Scegliendo una riga, lo stesso riquadro racconta perché quell'esito: è il posto in
    ''' cui l'utente verifica che il giudizio sia ancorato al profilo e non inventato.
    ''' </summary>
    Private Sub lvwGiudizi_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lvwGiudizi.SelectedIndexChanged

        Dim scelta As ListViewItem = lvwGiudizi.SelectedItems.Cast(Of ListViewItem)().FirstOrDefault()
        Dim giudizio As GiudizioMostrato = TryCast(scelta?.Tag, GiudizioMostrato)

        If giudizio Is Nothing Then
            MostraLaLetturaDInsieme()
            Return
        End If

        lblEtichettaSpiegazione.Text = "Perché"
        txtSpiegazione.Text = If(String.IsNullOrWhiteSpace(giudizio.Spiegazione),
                                 "L'AI non ha motivato questa voce.", giudizio.Spiegazione)

    End Sub

    ' ==================================================================
    ' I bottoni
    ' ==================================================================

    Private Async Sub btnAnalizza_Click(sender As Object, e As EventArgs) Handles btnAnalizza.Click

        ' Ad attesa in corso lo stesso bottone serve ad annullarla, come l'import in P2.
        If AiAlLavoro Then
            AnnullaIlLavoro()
            Return
        End If

        Await AnalizzaLAnnuncioAsync()

    End Sub

    ''' <summary>
    ''' Riapre la fascia per un altro annuncio. Non c'è niente da confermare: quella in
    ''' mostra è già nella sua cartella, e resta lì.
    ''' </summary>
    Private Sub btnNuovoAnnuncio_Click(sender As Object, e As EventArgs) Handles btnNuovoAnnuncio.Click

        FasciaDIngresso(aperta:=True)

        txtAnnuncio.Clear()
        txtAnnuncio.Focus()

        RaccontaLoStato("Incolla il testo del prossimo annuncio, poi premi «Analizza».",
                        StileApp.TestoSecondario)

    End Sub

    Private Sub btnGeneraDocumenti_Click(sender As Object, e As EventArgs) Handles btnGeneraDocumenti.Click
        RaiseEvent DocumentiRichiesti(Me, EventArgs.Empty)
    End Sub

    Private Sub txtAnnuncio_TextChanged(sender As Object, e As EventArgs) Handles txtAnnuncio.TextChanged
        AggiornaComandi()
    End Sub

    ' ==================================================================
    ' Aspetto, spazio e stato dei comandi
    ' ==================================================================

    ''' <inheritdoc/>
    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        ' Come in P2 e P5: a cedere il posto al logo è la fascia delle azioni, dove ci
        ' sono bottoni e non dati (v. IPannelloArea).
        pnlAzioni.Height = Math.Max(AltezzaMinimaAzioni, ingombro.Height)
        pnlAzioni.Padding = New Padding(ingombro.Width + StileApp.DistanzaControlli, 0, 0, 0)

        DisponiLeAzioni()

    End Sub

    ''' <summary>Mette i bottoni sul fondo della fascia: le vie laterali a sinistra, l'avanti a destra.</summary>
    Private Sub DisponiLeAzioni()

        Dim riga As Integer = pnlAzioni.Height - StileApp.MargineRiquadro - StileApp.BottoneStandard.Height

        Dim sinistra As Integer = pnlAzioni.Padding.Left
        For Each bottone As Button In {btnNuovoAnnuncio, btnBrainstorm, btnScarta}
            bottone.Location = New Point(sinistra, riga)
            sinistra += bottone.Width + StileApp.DistanzaControlli
        Next

        btnGeneraDocumenti.Location = New Point(
            pnlAzioni.ClientSize.Width - StileApp.MargineRiquadro - btnGeneraDocumenti.Width, riga)

    End Sub

    Private Sub PannelloOpportunita_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        DisponiLeAzioni()
    End Sub

    ''' <summary>Apre o richiude la fascia in cima (cap. 03.6).</summary>
    Private Sub FasciaDIngresso(aperta As Boolean)

        _fasciaAperta = aperta
        pnlIngresso.Visible = aperta

    End Sub

    Private Sub VestiIBottoni()

        StileApp.VestiBottone(btnAnalizza, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnGeneraDocumenti, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnNuovoAnnuncio, LivelloBottone.Neutro)
        StileApp.VestiBottone(btnBrainstorm, LivelloBottone.Esplorativo)

        ' Scartare un'opportunità la butta via: pesa quanto un'eliminazione.
        StileApp.VestiBottone(btnScarta, LivelloBottone.Distruttivo)

    End Sub

    ''' <summary>
    ''' I bottoni delle tappe che verranno restano visibili e spenti, con scritto quando
    ''' arrivano (cap. 03.8): chi guarda l'applicazione a metà strada deve capire dove sta
    ''' andando.
    ''' </summary>
    Private Sub DichiaraLeTappeCheMancano()

        btnBrainstorm.Enabled = False
        _suggerimenti.SetToolTip(btnBrainstorm,
                                 "Il ragionamento sull'opportunità, con gli appunti di mira, arriva con la tappa T7.")

        btnScarta.Enabled = False
        _suggerimenti.SetToolTip(btnScarta,
                                 "Scartare un'opportunità vuole il registro delle candidature, che arriva con la tappa T5.")

    End Sub

    ''' <summary>
    ''' Scrive sotto «Analizza» perché non si può premere — e non scrive niente quando si
    ''' può. Le tre ragioni sono diverse e si dicono in modo diverso: manca la chiave,
    ''' manca il profilo, manca il testo. Solo l'ultima è colpa del momento; le prime due
    ''' mandano da qualche parte.
    ''' </summary>
    Private Sub DiciPercheNonSiPuoAnalizzare(occupato As Boolean, conAi As Boolean, conProfilo As Boolean)

        If occupato Then
            lblPerchePento.Text = ""
            Return
        End If

        If Not conAi Then
            lblPerchePento.Text = "Manca la chiave API: senza, l'annuncio non si può leggere."
            lblPerchePento.ForeColor = StileApp.Pericolo

        ElseIf Not conProfilo Then
            lblPerchePento.Text = "Prima il profilo: apri la scheda «Profilo» e salvalo."
            lblPerchePento.ForeColor = StileApp.Pericolo

        ElseIf txtAnnuncio.Text.Trim() = "" Then
            lblPerchePento.Text = "Incolla il testo qui a sinistra."
            lblPerchePento.ForeColor = StileApp.TestoSecondario

        Else
            lblPerchePento.Text = ""
        End If

    End Sub

    Private Sub RaccontaLoStato(testo As String, colore As Color)

        lblStatoOpportunita.Text = testo
        lblStatoOpportunita.ForeColor = colore

    End Sub

    ''' <summary>Il match in una riga, per la barra di stato del pannello.</summary>
    Private Function RiassuntoDelMatch() As String

        If _vista Is Nothing Then Return ""

        Dim conteggio As Integer = _vista.Giudizi.Count
        Return $"Confronto fatto: {StelleScritte(_vista.Stelle)} su {conteggio} " &
               If(conteggio = 1, "voce giudicata.", "voci giudicate.")

    End Function

    ''' <summary>Da dove si comincia, detto appena il pannello si collega al motore.</summary>
    Private Sub RaccontaDaDoveSiComincia()

        If _pipeline Is Nothing Then
            RaccontaLoStato(MotivoSenzaAi(), StileApp.Pericolo)
            Return
        End If

        If Not _contesto.Archivio.Esiste Then
            RaccontaLoStato(
                "Prima serve il tuo profilo: costruiscilo nella scheda «Profilo», poi torna qui.",
                StileApp.TestoSecondario)
            Return
        End If

        RaccontaLoStato("Incolla il testo di un annuncio e premi «Analizza».", StileApp.TestoSecondario)

    End Sub

    ''' <summary>Perché l'analisi non si può fare: la chiave, o i prompt.</summary>
    Private Function MotivoSenzaAi() As String

        If _contesto IsNot Nothing AndAlso _contesto.Libreria Is Nothing Then
            Return "I prompt non si sono caricati: senza di loro non posso né leggere l'annuncio né confrontarlo."
        End If

        Return $"Per analizzare un annuncio serve la chiave API ({ClientClaude.NomeVariabileChiave}): " &
               "l'annuncio e il confronto passano dall'AI."

    End Function

    Private Sub LavoroInCorso(inCorso As Boolean)

        Cursor = If(inCorso, Cursors.AppStarting, Cursors.Default)

        AggiornaComandi()
        RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)

    End Sub

    ''' <summary>
    ''' Decide, in un posto solo, che cosa si può fare adesso. Le domande sono: l'AI è
    ''' disponibile, c'è un profilo con cui confrontare, c'è del testo da leggere, e c'è
    ''' già qualcosa in volo?
    ''' </summary>
    Private Sub AggiornaComandi()

        Dim occupato As Boolean = AiAlLavoro
        Dim conAi As Boolean = _pipeline IsNot Nothing
        Dim conProfilo As Boolean = _contesto IsNot Nothing AndAlso _contesto.Archivio.Esiste

        ' A lavoro in corso il bottone cambia mestiere: è l'annulla dell'attesa
        ' (cap. 12.7 — le operazioni lunghe sono annullabili).
        btnAnalizza.Text = If(occupato, "Annulla", "Analizza")
        btnAnalizza.Enabled = occupato OrElse
                              (conAi AndAlso conProfilo AndAlso txtAnnuncio.Text.Trim() <> "")

        ' Un bottone spento senza una ragione a portata d'occhio si legge come
        ' un'applicazione rotta: la ragione si scrive sotto il bottone, dove chi voleva
        ' premerlo sta già guardando.
        DiciPercheNonSiPuoAnalizzare(occupato, conAi, conProfilo)

        txtAnnuncio.ReadOnly = occupato
        txtAnnuncio.BackColor = If(occupato, StileApp.SfondoBase, StileApp.SfondoContenuto)

        btnNuovoAnnuncio.Enabled = Not occupato AndAlso Not _fasciaAperta

        ' I documenti si chiedono dopo aver visto le stelle: è la decisione che sta in
        ' mezzo al flusso (cap. 12, A5→A7). Sotto soglia il bottone resta acceso — si
        ' sconsiglia, non si impedisce.
        btnGeneraDocumenti.Enabled = Not occupato AndAlso
                                     _opportunita IsNot Nothing AndAlso _opportunita.Confrontata

    End Sub

End Class
