Imports System.Drawing
Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Motore

''' <summary>Per cosa il pannello della conversazione è in uso adesso (cap. 03, P5).</summary>
Public Enum ModoDialogo

    ''' <summary>Il dialogo guidato che costruisce il profilo (T3, cap. 12 flusso B).</summary>
    Profilo

    ''' <summary>Il ragionamento su una candidatura (T7c, cap. 12 A6).</summary>
    Brainstorming

End Enum

''' <summary>
''' Pannello P5 — la conversazione (cap. 03.6). Un pannello solo per <b>due</b> mestieri
''' diversi: il dialogo guidato che costruisce il profilo da zero (cap. 12, flusso B) e
''' il ragionamento su una candidatura, con gli appunti di mira (cap. 12, A6).
''' </summary>
''' <remarks>
''' <para>Qui dentro non c'è <b>nessuna</b> regola delle due conversazioni: l'ordine dei
''' turni, le schede di conferma, l'anti-perdita e il riepilogo stanno in
''' <see cref="DialogoProfilo"/>, e la conduzione del ragionamento in
''' <see cref="Motore.Brainstorming"/> — tutti e due collaudati senza interfaccia. Questo
''' pannello fa due sole cose: disegna quello che riceve e riporta indietro quello che
''' l'utente scrive o sceglie.</para>
''' <para><b>Le due modalità non si mescolano</b> (v. <see cref="ModoDialogo"/>): i
''' controlli sono gli stessi — bolle, casella, i tre bottoni in fondo — ma cambiano nome
''' e destinazione, e in ogni momento ne è viva una sola. Ricostruire un secondo pannello
''' gemello per una chat che è identica in tutto tranne le etichette sarebbe stato un
''' doppione da tenere allineato per sempre.</para>
''' <para><b>Solo il ragionamento arriva in streaming</b> (cap. 02.5): là la bolla
''' dell'assistente cresce sotto gli occhi mentre il testo arriva, e si può interrompere.
''' I turni del dialogo guidato no — sono corti, e una mossa a metà lascerebbe la
''' macchina in uno stato che non esiste (cap. 02.6).</para>
''' <para>La conversazione è l'unica parte dell'interfaccia che nasce a runtime, perché
''' non si sa quante bolle avrà. I <b>bottoni delle scelte</b> invece no: sono tre, fissi
''' nel designer, perché le mosse non ne offrono mai di più (cap. 03.1, punto 6). A
''' cambiare sono etichetta, livello di colore e posizione.</para>
''' <para><b>Niente va su disco da qui.</b> Alla fine il profilo raccolto si porta nella
''' scheda P2, dove l'utente lo controlla e lo salva: la conferma prima della scrittura è
''' una sola, e sta là (cap. 12.7).</para>
''' </remarks>
Public Class PannelloDialogo
    Implements IPannelloArea

    ''' <summary>Sotto questa altezza la fascia delle azioni non scende: i bottoni ci devono stare.</summary>
    Private Const AltezzaMinimaAzioni As Integer = 60

    ''' <summary>Quanto spazio si prende il logo flottante (cap. 03.5).</summary>
    Private _ingombroLogo As Size

    ''' <summary>
    ''' La fascia dei comandi in fondo (cap. 03.4). Nasce alla prima disposizione e non nel
    ''' costruttore: i bottoni che le si dichiarano esistono solo dopo
    ''' <c>InitializeComponent</c>.
    ''' </summary>
    Private _comandi As FasciaDeiComandi

    ''' <summary>Il titolo delle finestrelle di conferma: il nome che l'utente conosce.</summary>
    Private Const NomeProdotto As String = "TrovaLavoro"

    ''' <summary>Quanta larghezza può prendersi una bolla: il resto è l'aria che la fa leggere come tale.</summary>
    Private Const QuotaLarghezzaBolla As Double = 0.72

    ''' <summary>Sotto questa larghezza una bolla non scende, per stretta che sia la finestra.</summary>
    Private Const LarghezzaMinimaBolla As Integer = 240

    ''' <summary>Margine interno della bolla e distanza fra una bolla e la successiva.</summary>
    Private Const RientroBolla As Integer = 10
    Private Const DistanzaFraBolle As Integer = 8

    ''' <summary>Gli identificativi delle tre scelte in mostra; vuoti se il bottone è spento.</summary>
    Private ReadOnly _idScelte As String() = {Nothing, Nothing, Nothing}

    Private _contesto As ContestoApp

    ''' <summary>La conversazione in corso; <c>Nothing</c> se non è ancora cominciata.</summary>
    Private _dialogo As DialogoProfilo

    ''' <summary>
    ''' Chi struttura le risposte del dialogo in corso. Si tiene da parte perché
    ''' «Ricomincia» deve ripartire con lo stesso, anche quando non è quello del contesto.
    ''' </summary>
    Private _strutturatore As IStrutturatoreTurni

    ''' <summary>L'ultima mossa mostrata: dice cosa il dialogo sta aspettando adesso.</summary>
    Private _mossa As Mossa

    ''' <summary>Se in questo momento si sta aspettando l'AI.</summary>
    Private _occupato As Boolean

    ''' <summary>Se il profilo di questo dialogo è già stato consegnato alla scheda P2.</summary>
    Private _consegnato As Boolean

    ''' <summary>Per quale dei due mestieri il pannello è in uso adesso.</summary>
    Private _modo As ModoDialogo = ModoDialogo.Profilo

    ''' <summary>Il ragionamento in corso; <c>Nothing</c> in modalità profilo.</summary>
    Private _brainstorming As Motore.Brainstorming

    ''' <summary>
    ''' Il gettone con cui si interrompe il turno in streaming; <c>Nothing</c> quando non
    ''' c'è niente in volo.
    ''' </summary>
    Private _annulla As CancellationTokenSource

    ''' <summary>
    ''' La riga di testo della bolla che sta crescendo mentre l'AI scrive, e il suo
    ''' contenitore da rimettere in forma a ogni pezzo.
    ''' </summary>
    Private _rigaViva As Label
    Private _bollaViva As Panel

    ''' <summary>
    ''' La stessa risposta <b>com'è arrivata</b>, segni del Markdown compresi. A video ci
    ''' va spianata (v. <see cref="ProsaDellAssistente"/>), ma la ripulitura vuole il testo
    ''' intero: un <c>**</c> spezzato fra due pezzi, spianato pezzo per pezzo, non si
    ''' riconoscerebbe mai. Perciò i pezzi si accumulano qui, e la bolla si riscrive.
    ''' </summary>
    Private _grezzoDellaBollaViva As String

    ''' <summary>
    ''' Quanto erano larghi nel disegno i due bottoni che cambiano nome col mestiere del
    ''' pannello: è il minimo sotto cui non si scende quando il testo è più corto.
    ''' </summary>
    Private _larghezzaUscita As Integer
    Private _larghezzaConclusione As Integer

    ''' <summary>Chiede alla finestra di riportare in vista la scheda del profilo.</summary>
    Public Event TornaAlProfilo As EventHandler

    ''' <summary>
    ''' Dice alla finestra che il profilo raccolto è pronto da consegnare a P2; il
    ''' profilo si legge da <see cref="ProfiloCostruito"/>.
    ''' </summary>
    Public Event ProfiloPronto As EventHandler

    ''' <summary>
    ''' L'AI del dialogo ha cominciato o finito di lavorare. Serve alla finestra:
    ''' «mentre l'AI lavora non si esce» vale anche per la barra di navigazione, che
    ''' questo pannello non può spegnere da sé.
    ''' </summary>
    Public Event LavoroAiCambiato As EventHandler

    ''' <summary>
    ''' Chiede alla finestra di riportare in vista la candidatura di cui si stava
    ''' ragionando. È il gemello di <see cref="TornaAlProfilo"/> per l'altra modalità.
    ''' </summary>
    Public Event TornaAllaCandidatura As EventHandler

    ''' <summary>
    ''' Gli appunti di mira sono stati confermati e salvati con la candidatura: la
    ''' finestra riporti l'utente lì, dove adesso c'è qualcosa di nuovo.
    ''' </summary>
    Public Event AppuntiConfermati As EventHandler

    Public Sub New()

        InitializeComponent()

        _larghezzaUscita = btnTornaAlProfilo.Width
        _larghezzaConclusione = btnPortaNelProfilo.Width

        VestiIBottoni()
        AggiornaComandi()

    End Sub

    ''' <summary>Collega il pannello al motore; il dialogo si apre solo quando serve.</summary>
    Public Sub Collega(contesto As ContestoApp)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
        _contesto = contesto

        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Il profilo raccolto dalla conversazione; vuoto se non è ancora cominciata.
    ''' </summary>
    ''' <remarks>
    ''' È una <b>copia</b>. Da qui in poi il profilo è della scheda P2, e le correzioni
    ''' che l'utente ci farà lì non devono rientrare di soppiatto nella conversazione,
    ''' che nel frattempo è ancora viva e potrebbe consegnarlo una seconda volta.
    ''' </remarks>
    Public ReadOnly Property ProfiloCostruito As Dati.Profilo
        Get
            If _dialogo Is Nothing Then Return New Dati.Profilo()
            Return Dati.Profilo.DaJson(_dialogo.Profilo.VersoJson())
        End Get
    End Property

    ''' <summary>
    ''' Se c'è una conversazione cominciata e non arrivata in fondo. La finestra lo
    ''' chiede prima di chiudersi: quello che l'utente ha raccontato finora vive solo in
    ''' memoria, e chiudere lo butterebbe via in silenzio.
    ''' </summary>
    Public ReadOnly Property HaUnDialogoInCorso As Boolean
        Get
            Return _dialogo IsNot Nothing AndAlso Not _dialogo.Finito
        End Get
    End Property

    ''' <summary>
    ''' Se c'è un racconto arrivato in fondo ma mai portato nella scheda P2. È il caso
    ''' peggiore della chiusura: il dialogo è «finito», quindi non risulta in corso, ma
    ''' il profilo costruito vive solo in memoria — chiudere adesso lo perderebbe tutto,
    ''' in silenzio.
    ''' </summary>
    Public ReadOnly Property HaUnRaccontoNonConsegnato As Boolean
        Get
            Return _dialogo IsNot Nothing AndAlso _dialogo.Finito AndAlso Not _consegnato
        End Get
    End Property

    ''' <summary>Se in questo momento una chiamata all'AI del dialogo è in volo.</summary>
    Public ReadOnly Property AiAlLavoro As Boolean
        Get
            Return _occupato
        End Get
    End Property

    ''' <summary>
    ''' La finestra segna qui che la scheda P2 ha accettato il profilo consegnato: da
    ''' questo momento il racconto è al sicuro, e chiudere o ricominciare non minaccia
    ''' più niente.
    ''' </summary>
    Public Sub SegnaConsegnato()
        _consegnato = True
    End Sub

    ' ==================================================================
    ' Aprire, riprendere, ricominciare
    ' ==================================================================

    ''' <summary>
    ''' Apre la conversazione, o <b>riprende</b> quella già cominciata: uscire dal
    ''' pannello e rientrare non azzera niente (cap. 12.7 — mai un vicolo cieco).
    ''' </summary>
    ''' <param name="strutturatore">
    ''' Chi struttura le risposte. Di norma si omette e si usa quello del motore; il
    ''' banco di collaudo passa qui il suo finto, e fa girare il dialogo senza rete.
    ''' </param>
    Public Async Function ApriIlDialogoAsync(Optional strutturatore As IStrutturatoreTurni = Nothing) As Task

        ' Si torna al dialogo del profilo: se il pannello stava ragionando su una
        ' candidatura, da qui in poi non lo fa più.
        PassaAlModo(ModoDialogo.Profilo)

        If _dialogo IsNot Nothing Then Return

        Dim chiStruttura As IStrutturatoreTurni = If(strutturatore, _contesto?.Strutturatore)
        If chiStruttura Is Nothing Then
            RaccontaLoStato(
                $"Per costruire il profilo parlando serve la chiave API ({ClientClaude.NomeVariabileChiave}): " &
                "ogni risposta passa dall'AI.",
                StileApp.Pericolo)
            Return
        End If

        _strutturatore = chiStruttura
        _dialogo = New DialogoProfilo(chiStruttura)
        _consegnato = False

        SvuotaLaConversazione()
        Await EseguiAsync(Function() _dialogo.AvviaAsync()).ConfigureAwait(True)

    End Function

    ''' <summary>Butta la conversazione e ne apre una nuova, dal primo turno.</summary>
    Public Function RicominciaAsync() As Task

        Dim chiStruttura As IStrutturatoreTurni = _strutturatore
        _dialogo = Nothing
        _mossa = Nothing

        Return ApriIlDialogoAsync(chiStruttura)

    End Function

    ''' <summary>
    ''' Butta la conversazione <b>senza aprirne un'altra</b>: il profilo da cui nasceva è
    ''' stato eliminato (cap. 11.5), e quel che l'utente aveva raccontato non ha più un
    ''' posto dove andare.
    ''' </summary>
    ''' <remarks>
    ''' Non è «Ricomincia»: quello riparte subito col primo turno, e qui ripartire
    ''' vorrebbe dire chiamare l'AI per un racconto che nessuno ha chiesto — davanti a un
    ''' utente che ha appena detto di voler sparire. Mentre l'AI lavora non si tocca
    ''' niente: la mossa in arrivo troverebbe un dialogo che non c'è più.
    ''' </remarks>
    Public Sub Dimentica()

        If _occupato Then Return

        _dialogo = Nothing
        _mossa = Nothing
        _consegnato = False

        ' Anche il ragionamento se ne va: era appoggiato a quel profilo, ed era lui
        ' l'unica fonte di fatti su cui stavamo ragionando.
        _brainstorming = Nothing
        PassaAlModo(ModoDialogo.Profilo)

        SvuotaLaConversazione()
        RaccontaLoStato("Il profilo è stato eliminato: qui non è rimasto niente di quel racconto.",
                        StileApp.TestoSecondario)
        AggiornaComandi()

    End Sub

    ' ==================================================================
    ' Il ragionamento su una candidatura (T7c)
    ' ==================================================================

    ''' <summary>La candidatura di cui si sta ragionando; <c>Nothing</c> in modalità profilo.</summary>
    Public ReadOnly Property CandidaturaInEsame As Opportunita
        Get
            Return _brainstorming?.Candidatura
        End Get
    End Property

    ''' <summary>
    ''' Apre il ragionamento su una candidatura, o <b>riprende</b> quello già cominciato
    ''' sulla stessa: uscire e rientrare non azzera niente, come nel dialogo guidato.
    ''' </summary>
    ''' <remarks>
    ''' Su una candidatura <b>diversa</b> si ricomincia da capo, ma non di nascosto: la
    ''' conversazione di prima riguardava un altro annuncio e non serve più, però era
    ''' lavoro dell'utente e va chiesto prima di buttarlo.
    ''' </remarks>
    ''' <param name="candidatura">L'opportunità da mettere sul tavolo, già confrontata.</param>
    ''' <param name="mestiere">
    ''' Chi parla con l'AI. Di norma si omette e si usa quello del motore; il banco passa
    ''' qui il suo finto e fa girare il ragionamento senza rete.
    ''' </param>
    Public Async Function ApriIlBrainstormingAsync(candidatura As Opportunita,
                                                   Optional mestiere As IBrainstormatore = Nothing) As Task

        If candidatura Is Nothing Then Throw New ArgumentNullException(NameOf(candidatura))

        Dim chiRagiona As IBrainstormatore = If(mestiere, _contesto?.Brainstorm)
        If chiRagiona Is Nothing Then
            PassaAlModo(ModoDialogo.Brainstorming)
            RaccontaLoStato(
                $"Per ragionare su una candidatura serve la chiave API ({ClientClaude.NomeVariabileChiave}).",
                StileApp.Pericolo)
            Return
        End If

        ' Già aperto su questa candidatura: si riprende dov'era.
        If _brainstorming IsNot Nothing AndAlso _brainstorming.Candidatura Is candidatura Then
            PassaAlModo(ModoDialogo.Brainstorming)
            Return
        End If

        If _brainstorming IsNot Nothing AndAlso _brainstorming.Cominciato Then
            Dim risposta As DialogResult = MessageBox.Show(
                $"Stavamo ragionando su «{_brainstorming.Candidatura.Titolo}»." & vbLf &
                "Se passiamo a questa candidatura, quella conversazione si chiude.",
                NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)

            If risposta <> DialogResult.Yes Then Return
        End If

        PassaAlModo(ModoDialogo.Brainstorming)

        _brainstorming = New Motore.Brainstorming(chiRagiona, candidatura, ProfiloPerIlRagionamento())

        SvuotaLaConversazione()
        AggiungiIlCartelloDellaCandidatura(candidatura)

        ' Apre l'AI, con quello che vede: una chat vuota non dice da dove cominciare.
        Await UnTurnoDiRagionamentoAsync(Function(annulla, pezzo) _brainstorming.ApriAsync(pezzo, annulla)).
            ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' Il profilo da mettere sul tavolo del ragionamento: quello salvato su disco, che è
    ''' l'unica fonte di fatti (cap. 02).
    ''' </summary>
    Private Function ProfiloPerIlRagionamento() As JsonNode

        Return _contesto?.Archivio.Carica()?.VersoJson()

    End Function

    ''' <summary>
    ''' Butta il ragionamento e ne apre uno nuovo sulla stessa candidatura.
    ''' </summary>
    Private Async Function RicominciaIlRagionamentoAsync() As Task

        Dim candidatura As Opportunita = _brainstorming?.Candidatura
        If candidatura Is Nothing Then Return

        _brainstorming = Nothing

        Await ApriIlBrainstormingAsync(candidatura).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' Fa fare un turno al ragionamento mentre la bolla dell'assistente cresce a video.
    ''' </summary>
    ''' <remarks>
    ''' <para>Qui l'attesa <b>si può interrompere</b>, all'opposto del turno del dialogo
    ''' guidato (cap. 02.6): là una mossa a metà lascerebbe la macchina in uno stato che
    ''' non esiste, qui resta solo una risposta più corta — e il testo già arrivato è roba
    ''' buona, che rimane dov'è.</para>
    ''' <para>Un errore non butta via niente: si dice in una bolla e la conversazione
    ''' resta in piedi, pronta a riprovare.</para>
    ''' </remarks>
    Private Async Function UnTurnoDiRagionamentoAsync(
        passo As Func(Of CancellationToken, Action(Of String), Task)) As Task

        Occupato(True)
        IniziaLaBollaViva()

        Using gettone As New CancellationTokenSource()

            _annulla = gettone

            Try
                Await passo(gettone.Token, AddressOf PezzoArrivato).ConfigureAwait(True)

                ' Il gemello del «(interrotto)» qui sotto: là a fermare la frase è stato
                ' l'utente, qui il tetto dei token del prompt. In tutt'e due i casi quello
                ' che è arrivato resta a video, e in tutt'e due va detto che manca il resto.
                If If(_brainstorming?.UltimoTurnoTroncato, False) Then
                    ChiudiLaBollaViva()
                    AggiungiBollaAssistente("(fermata qui: ha raggiunto il limite di lunghezza)")
                End If

            Catch ex As OperationCanceledException
                ' L'ha fermata l'utente: quel che era arrivato resta, e si dice che è
                ' rimasto a metà — una risposta troncata che non lo dichiara è peggio di
                ' nessuna risposta.
                ChiudiLaBollaViva()
                AggiungiBollaAssistente("(interrotto)")

            Catch ex As ErroreAi
                ChiudiLaBollaViva()
                AggiungiBollaAssistente(ex.Message)
                AggiungiBollaAssistente("Riprova pure: quello che ci siamo detti è ancora qui.")

            Finally
                _annulla = Nothing
                ChiudiLaBollaViva()
                Occupato(False)
            End Try

        End Using

    End Function

    ''' <summary>Ferma il turno in corso, se ce n'è uno.</summary>
    Public Sub Interrompi()

        _annulla?.Cancel()

    End Sub

    ''' <summary>
    ''' Manda al ragionamento quello che c'è nella casella. È il gemello di
    ''' <see cref="InviaLaRispostaAsync"/> per l'altra modalità, e come quello il banco lo
    ''' chiama direttamente.
    ''' </summary>
    Public Async Function InviaAlRagionamentoAsync() As Task

        If _occupato OrElse _brainstorming Is Nothing Then Return

        Dim testo As String = txtRisposta.Text.Trim()
        If testo = "" Then Return

        txtRisposta.Clear()
        AggiungiBollaUtente(testo)

        Await UnTurnoDiRagionamentoAsync(
            Function(annulla, pezzo) _brainstorming.RispondiAsync(testo, pezzo, annulla)).ConfigureAwait(True)

    End Function

    ''' <summary>
    ''' Distilla gli appunti, li fa confermare e — se l'utente conferma — li salva con la
    ''' candidatura (cap. 12, A6.3-4).
    ''' </summary>
    Public Async Function TrasformaInAppuntiAsync() As Task

        Dim distillati As AppuntiDiMira = Await DistillaGliAppuntiAsync().ConfigureAwait(True)
        If distillati Is Nothing Then Return

        Dim scelti As AppuntiDiMira = FinestraAppunti.Chiedi(FindForm(), distillati)
        If scelti Is Nothing Then Return

        ConfermaGliAppunti(scelti)

    End Function

    ''' <summary>
    ''' Chiede all'AI di distillare gli appunti dalla conversazione.
    ''' </summary>
    ''' <remarks>
    ''' Sta in un metodo pubblico, staccato dalla finestra di conferma che gli va dietro,
    ''' perché è l'unico modo di collaudarlo: una finestra modale il banco non la sa
    ''' chiudere (v. <see cref="FinestraAppunti.RiscriviLAppunto"/>, che è lo stesso
    ''' problema visto dall'altra parte).
    ''' </remarks>
    ''' <returns>
    ''' Gli appunti proposti, oppure <c>Nothing</c> se non c'è niente da confermare —
    ''' perché la conversazione non ha prodotto niente di operativo, o perché la chiamata
    ''' è andata storta. In tutti e due i casi l'utente l'ha già letto in una bolla.
    ''' </returns>
    Public Async Function DistillaGliAppuntiAsync() As Task(Of AppuntiDiMira)

        If _occupato OrElse _brainstorming Is Nothing OrElse Not _brainstorming.SiPuoDistillare Then
            Return Nothing
        End If

        Occupato(True)
        RaccontaLoStato("Sto rileggendo quello che ci siamo detti…", StileApp.TestoSecondario)

        Dim distillati As AppuntiDiMira

        Try
            distillati = Await _brainstorming.AppuntiAsync().ConfigureAwait(True)

        Catch ex As ErroreAi
            AggiungiBollaAssistente(ex.Message)
            Return Nothing

        Finally
            Occupato(False)
        End Try

        If distillati Is Nothing OrElse distillati.Vuoti Then
            AggiungiBollaAssistente(
                "Da questa conversazione non è venuto fuori niente di operativo da annotare. " &
                "Continuiamo pure a ragionare, oppure vai avanti così: i documenti si scrivono lo stesso.")
            Return Nothing
        End If

        Return distillati

    End Function

    ''' <summary>
    ''' Scrive gli appunti confermati nella cartella della candidatura e lo racconta.
    ''' </summary>
    Public Sub ConfermaGliAppunti(scelti As AppuntiDiMira)

        Dim candidatura As Opportunita = _brainstorming.Candidatura
        candidatura.Appunti = scelti.VersoJson()

        Try
            _contesto?.Opportunita.Salva(candidatura)

        Catch ex As Exception When TypeOf ex Is IO.IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            AggiungiBollaAssistente($"Non sono riuscita a salvare gli appunti: {ex.Message}")
            Return
        End Try

        Dim quanti As Integer = scelti.Appunti.Count
        AggiungiBollaAssistente(
            If(quanti = 1, "Ho annotato un appunto per questa candidatura.",
                           $"Ho annotato {quanti} appunti per questa candidatura.") &
            " Guideranno la scrittura del 🎯 CV mirato e della ✉️ lettera quando li genererai.")

        RaiseEvent AppuntiConfermati(Me, EventArgs.Empty)

    End Sub

    ''' <summary>
    ''' Il cartello che apre la conversazione: di quale candidatura si sta parlando. Senza,
    ''' due ragionamenti su due annunci diversi si somiglierebbero troppo.
    ''' </summary>
    Private Sub AggiungiIlCartelloDellaCandidatura(candidatura As Opportunita)

        Dim scheda As New Scheda With {.Titolo = candidatura.Titolo}
        scheda.Righe.Add(New RigaScheda With {.Etichetta = "Azienda", .Valore = candidatura.Azienda})

        If candidatura.Match IsNot Nothing Then
            scheda.Righe.Add(New RigaScheda With {
                .Etichetta = "Match", .Valore = $"{candidatura.Match.Stelle:0.0} su 5"})
        End If

        AggiungiScheda(scheda)

    End Sub

    ' ==================================================================
    ' La bolla che cresce mentre l'AI scrive
    ' ==================================================================

    ''' <summary>
    ''' Apre una bolla dell'assistente vuota, pronta a riempirsi pezzo per pezzo.
    ''' </summary>
    Private Sub IniziaLaBollaViva()

        AggiungiBollaAssistente("")

        _grezzoDellaBollaViva = String.Empty
        _bollaViva = TryCast(flpConversazione.Controls(flpConversazione.Controls.Count - 1), Panel)
        _rigaViva = _bollaViva?.Controls.OfType(Of Panel)().FirstOrDefault()?.
            Controls.OfType(Of Panel)().FirstOrDefault()?.
            Controls.OfType(Of Label)().FirstOrDefault()

    End Sub

    ''' <summary>
    ''' Un pezzo di risposta è arrivato.
    ''' </summary>
    ''' <remarks>
    ''' Arriva dal thread che legge il flusso, non da quello della finestra: toccare un
    ''' controllo da lì è il modo classico di far cadere un'interfaccia Windows Forms, e
    ''' per questo il pezzo si fa riportare a casa prima di comparire.
    ''' </remarks>
    Private Sub PezzoArrivato(pezzo As String)

        If InvokeRequired Then
            BeginInvoke(New Action(Of String)(AddressOf PezzoArrivato), pezzo)
            Return
        End If

        CresciLaBolla(pezzo)

    End Sub

    ''' <summary>Allunga la bolla viva e tiene la conversazione in fondo.</summary>
    Private Sub CresciLaBolla(pezzo As String)

        If _rigaViva Is Nothing OrElse String.IsNullOrEmpty(pezzo) Then Return

        _grezzoDellaBollaViva &= pezzo
        _rigaViva.Text = ProsaDellAssistente.SenzaMarkdown(_grezzoDellaBollaViva)

        DisponiBolla(_bollaViva)
        ScorriInFondo()

    End Sub

    ''' <summary>
    ''' Chiude la bolla viva. Se non ci è arrivato dentro niente — un errore prima del
    ''' primo pezzo — la toglie: una bolla vuota a video è un fantasma.
    ''' </summary>
    Private Sub ChiudiLaBollaViva()

        ' Si guarda il testo com'è arrivato e non quello a video: una risposta fatta di
        ' soli segni — una riga orizzontale e nulla più — a video è vuota, ma qualcosa era
        ' arrivato, e la bolla non è un fantasma da togliere.
        If _bollaViva IsNot Nothing AndAlso String.IsNullOrEmpty(_grezzoDellaBollaViva) Then
            flpConversazione.Controls.Remove(_bollaViva)
            _bollaViva.Dispose()
        End If

        _bollaViva = Nothing
        _rigaViva = Nothing
        _grezzoDellaBollaViva = Nothing

    End Sub

    ''' <summary>
    ''' Passa il pannello da un mestiere all'altro: cambiano i titoli, i nomi dei bottoni
    ''' e dove portano. Quello che non è di questo modo si spegne e si dimentica.
    ''' </summary>
    Private Sub PassaAlModo(modo As ModoDialogo)

        _modo = modo

        If modo = ModoDialogo.Brainstorming Then

            lblTitolo.Text = "Ragioniamo su questa candidatura"
            lblSottotitolo.Text =
                "Ho davanti il tuo profilo, l'annuncio e il confronto già fatto. Decidiamo cosa mettere " &
                "davanti e come nominare quello che manca: i fatti restano quelli del tuo profilo."

            btnTornaAlProfilo.Text = "Torna alla candidatura"
            btnPortaNelProfilo.Text = "Trasforma in appunti"
            txtRisposta.PlaceholderText = "Scrivi quello che pensi…"

            NascondiLeScelte()
            AdattaAlTesto()

        Else

            lblTitolo.Text = "Costruiamo il tuo profilo"
            lblSottotitolo.Text =
                "Un argomento alla volta: rispondi con parole tue, e prima di registrare qualcosa " &
                "ti mostro cosa ho capito."

            btnTornaAlProfilo.Text = "Torna al profilo"
            btnPortaNelProfilo.Text = "Porta nel profilo"
            txtRisposta.PlaceholderText = "La tua risposta…"

            AdattaAlTesto()

        End If

        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Dà ai due bottoni che cambiano nome la larghezza che il loro testo chiede, senza
    ''' scendere sotto quella del disegno.
    ''' </summary>
    ''' <remarks>
    ''' Trovato guardando l'applicazione in faccia nel collaudo di T7c: «Torna alla
    ''' candidatura» non sta dove stava «Torna al profilo», e a video si leggeva «Torna
    ''' alla». Un bottone tagliato a metà non è un dettaglio estetico — è un comando che
    ''' non dice più dove porta, e nessun collaudo del banco poteva accorgersene, perché
    ''' il banco vede il testo del bottone, non quanto ne entra.
    ''' </remarks>
    Private Sub AdattaAlTesto()

        btnTornaAlProfilo.Width = Math.Max(_larghezzaUscita, btnTornaAlProfilo.PreferredSize.Width)
        btnPortaNelProfilo.Width = Math.Max(_larghezzaConclusione, btnPortaNelProfilo.PreferredSize.Width)

        DisponiLeAzioni()

    End Sub

    Private Async Sub btnRicomincia_Click(sender As Object, e As EventArgs) Handles btnRicomincia.Click

        If _modo = ModoDialogo.Brainstorming Then

            ' Anche qui quello che si butta è lavoro dell'utente, e gli appunti già
            ' confermati non lo mettono al riparo: sono l'esito di quello che si è detto
            ' fin qui, non della conversazione che ricomincerebbe.
            If _brainstorming IsNot Nothing AndAlso _brainstorming.Cominciato Then
                Dim conferma As DialogResult = MessageBox.Show(
                    "Vuoi ricominciare il ragionamento da capo?" & vbLf &
                    "Quello che ci siamo detti finora va perso.",
                    NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)

                If conferma <> DialogResult.Yes Then Return
            End If

            Await RicominciaIlRagionamentoAsync()
            Return

        End If

        ' Quello che si butta è il racconto dell'utente: si chiede sempre — tranne
        ' quando il racconto è finito e già consegnato alla scheda: lì è al sicuro, e
        ' minacciare una perdita che non esiste insegnerebbe a ignorare gli avvisi.
        If _dialogo IsNot Nothing AndAlso Not (_dialogo.Finito AndAlso _consegnato) Then
            Dim risposta As DialogResult = MessageBox.Show(
                "Vuoi ricominciare il dialogo da capo?" & vbLf &
                "Quello che mi hai raccontato finora va perso.",
                NomeProdotto, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)

            If risposta <> DialogResult.Yes Then Return
        End If

        Await RicominciaAsync()

    End Sub

    ' ==================================================================
    ' Il giro della conversazione: risposte, scelte, attesa dell'AI
    ' ==================================================================

    ''' <summary>
    ''' Manda al dialogo quello che c'è nella casella. È il metodo che preme «Invia»; il
    ''' banco lo chiama direttamente, perché di un gestore di clic non si può aspettare
    ''' la fine.
    ''' </summary>
    Public Async Function InviaLaRispostaAsync() As Task

        If Not SiPuoRispondere() Then Return

        Dim testo As String = txtRisposta.Text.Trim()
        If testo = "" Then Return

        txtRisposta.Clear()
        AggiungiBollaUtente(testo)

        Await EseguiAsync(Function() _dialogo.RispondiAsync(testo)).ConfigureAwait(True)

    End Function

    ''' <summary>Manda al dialogo una delle scelte in mostra (v. <see cref="Scelte"/>).</summary>
    Public Async Function ScegliAsync(idScelta As String) As Task

        If _occupato OrElse _dialogo Is Nothing OrElse String.IsNullOrEmpty(idScelta) Then Return
        If _mossa Is Nothing OrElse _mossa.Tipo <> TipoMossa.ChiediScelta Then Return

        AggiungiBollaUtente(EtichettaDellaScelta(idScelta))

        Await EseguiAsync(Function() _dialogo.ScegliAsync(idScelta)).ConfigureAwait(True)

    End Function

    Private Function SiPuoRispondere() As Boolean

        If _occupato OrElse _dialogo Is Nothing Then Return False
        Return _mossa IsNot Nothing AndAlso _mossa.Tipo = TipoMossa.ChiediRisposta

    End Function

    ''' <summary>
    ''' Fa fare un passo al dialogo mentre il pannello aspetta. L'attesa non è
    ''' annullabile: un turno è una chiamata corta — a essere lunga era la lettura di un
    ''' CV intero, e quella l'annulla si porta dietro (P2).
    ''' </summary>
    Private Async Function EseguiAsync(passo As Func(Of Task(Of Mossa))) As Task

        Occupato(True)

        Try
            Mostra(Await passo().ConfigureAwait(True))

        Catch ex As ErroreAi
            ' Il dialogo si difende da sé dagli errori dell'AI e li racconta dentro la
            ' mossa; se ne arriva uno fin qui, si dice e si riprova la stessa cosa.
            AggiungiBollaAssistente(ex.Message)
            AggiungiBollaAssistente("Riprova pure: la domanda è sempre quella.")
            PreparaLaRisposta(_mossa)

        Finally
            Occupato(False)
        End Try

    End Function

    Private Sub Occupato(inCorso As Boolean)

        _occupato = inCorso
        Cursor = If(inCorso, Cursors.AppStarting, Cursors.Default)

        If inCorso Then
            RaccontaLoStato(If(_modo = ModoDialogo.Brainstorming,
                               "Sto pensando… puoi fermarmi quando vuoi.",
                               "Sto leggendo la tua risposta…"),
                            StileApp.TestoSecondario)
        ElseIf _modo = ModoDialogo.Brainstorming Then
            RaccontaLoStato(
                "Questa conversazione non si conserva: quello che resta sono gli appunti che confermi.",
                StileApp.TestoSecondario)
        End If

        AggiornaComandi()
        RaiseEvent LavoroAiCambiato(Me, EventArgs.Empty)

    End Sub

    ''' <summary>
    ''' Disegna una mossa: le bolle dell'assistente nell'ordine in cui le ha dette, con
    ''' ogni eco dell'utente al punto in cui la mossa l'ha ancorata (anti-perdita: le
    ''' parole ripescate si rivedono <i>prima</i> del loro esito, non dopo), poi le
    ''' schede, e infine si prepara a ricevere.
    ''' </summary>
    Private Sub Mostra(mossa As Mossa)

        If mossa Is Nothing Then Return
        _mossa = mossa

        flpConversazione.SuspendLayout()

        Dim disegnate As Integer = 0
        For Each eco As EcoMossa In mossa.Echi
            While disegnate < Math.Min(eco.DopoDetti, mossa.Detto.Count)
                AggiungiBollaAssistente(mossa.Detto(disegnate))
                disegnate += 1
            End While
            If Not String.IsNullOrWhiteSpace(eco.Testo) Then AggiungiBollaUtente(eco.Testo)
        Next
        While disegnate < mossa.Detto.Count
            AggiungiBollaAssistente(mossa.Detto(disegnate))
            disegnate += 1
        End While

        For Each scheda As Scheda In mossa.Schede
            AggiungiScheda(scheda)
        Next

        flpConversazione.ResumeLayout()
        ScorriInFondo()

        PreparaLaRisposta(mossa)

    End Sub

    ''' <summary>Prepara la zona in basso a ciò che la mossa aspetta: scrivere, scegliere, o niente.</summary>
    Private Sub PreparaLaRisposta(mossa As Mossa)

        If mossa Is Nothing Then Return

        Select Case mossa.Tipo

            Case TipoMossa.ChiediRisposta
                txtRisposta.PlaceholderText = If(mossa.SuggerimentoCasella, "La tua risposta…")
                NascondiLeScelte()
                RaccontaLoStato(
                    "Il profilo non è ancora salvato: alla fine lo porti nella scheda e lo salvi tu.",
                    StileApp.TestoSecondario)

            Case TipoMossa.ChiediScelta
                MostraLeScelte(mossa.Scelte)
                RaccontaLoStato(
                    "Il profilo non è ancora salvato: alla fine lo porti nella scheda e lo salvi tu.",
                    StileApp.TestoSecondario)

            Case Else ' Fine
                NascondiLeScelte()
                RaccontaLoStato(
                    "Ho finito di raccogliere." & vbLf &
                    "Porta il profilo nella scheda: lì lo controlli campo per campo e lo salvi.",
                    StileApp.TestoSecondario)

        End Select

        AggiornaComandi()

    End Sub

    ' ==================================================================
    ' I bottoni delle scelte
    ' ==================================================================

    ''' <summary>
    ''' Mette in mostra le scelte della mossa sui tre bottoni fissi: etichetta, livello
    ''' di conseguenza (cap. 03.3) e posizione in fila.
    ''' </summary>
    Private Sub MostraLeScelte(scelte As List(Of Scelta))

        Dim bottoni As Button() = BottoniDelleScelte()
        Dim sinistra As Integer = 0

        For i As Integer = 0 To bottoni.Length - 1

            Dim bottone As Button = bottoni(i)

            If i >= scelte.Count Then
                _idScelte(i) = Nothing
                bottone.Visible = False
                Continue For
            End If

            Dim scelta As Scelta = scelte(i)
            _idScelte(i) = scelta.Id
            bottone.Text = scelta.Etichetta
            StileApp.VestiBottone(bottone, LivelloDellaScelta(scelta))

            bottone.Location = New Point(sinistra, txtRisposta.Top)
            bottone.Visible = True

            sinistra += Math.Max(bottone.MinimumSize.Width, bottone.PreferredSize.Width) +
                        StileApp.DistanzaControlli

        Next

    End Sub

    Private Sub NascondiLeScelte()

        For i As Integer = 0 To _idScelte.Length - 1
            _idScelte(i) = Nothing
        Next

        For Each bottone As Button In BottoniDelleScelte()
            bottone.Visible = False
        Next

    End Sub

    ''' <summary>
    ''' Quanto pesa una scelta. Scartare qualcosa che l'utente ha detto è distruttivo;
    ''' la scelta principale è il «avanti» del flusso; le altre aprono una strada.
    ''' </summary>
    Private Shared Function LivelloDellaScelta(scelta As Scelta) As LivelloBottone

        If scelta.Id = Motore.Scelte.Scarta Then Return LivelloBottone.Distruttivo
        If scelta.Principale Then Return LivelloBottone.AzionePrincipale
        Return LivelloBottone.Esplorativo

    End Function

    ''' <summary>Come si chiamava, per l'utente, la scelta che ha appena fatto.</summary>
    Private Function EtichettaDellaScelta(idScelta As String) As String

        Dim scelta As Scelta = _mossa?.Scelte.FirstOrDefault(Function(s) s.Id = idScelta)
        Return If(scelta?.Etichetta, idScelta)

    End Function

    Private Function BottoniDelleScelte() As Button()
        Return {btnScelta1, btnScelta2, btnScelta3}
    End Function

    Private Async Sub Scelta_Click(sender As Object, e As EventArgs) _
        Handles btnScelta1.Click, btnScelta2.Click, btnScelta3.Click

        Dim indice As Integer = Array.IndexOf(BottoniDelleScelte(), TryCast(sender, Button))
        If indice < 0 Then Return

        Await ScegliAsync(_idScelte(indice))

    End Sub

    ''' <summary>
    ''' Il bottone in fondo alla casella: manda quello che si è scritto, e durante un
    ''' turno del ragionamento diventa invece il modo di fermarlo.
    ''' </summary>
    Private Async Sub btnInvia_Click(sender As Object, e As EventArgs) Handles btnInvia.Click

        If _modo = ModoDialogo.Brainstorming Then

            If _occupato Then
                Interrompi()
                Return
            End If

            Await InviaAlRagionamentoAsync()
            Return

        End If

        Await InviaLaRispostaAsync()

    End Sub

    ''' <summary>
    ''' Invio manda, Maiusc+Invio va a capo: le risposte sono racconti di poche righe, e
    ''' spostare la mano sul bottone a ogni turno stanca.
    ''' </summary>
    Private Async Sub txtRisposta_KeyDown(sender As Object, e As KeyEventArgs) Handles txtRisposta.KeyDown

        If e.KeyCode <> Keys.Enter OrElse e.Shift Then Return

        ' Senza questo la casella multilinea si prende comunque il suo a capo.
        e.SuppressKeyPress = True

        If _modo = ModoDialogo.Brainstorming Then
            Await InviaAlRagionamentoAsync()
            Return
        End If

        Await InviaLaRispostaAsync()

    End Sub

    ' ==================================================================
    ' Le bolle della conversazione
    ' ==================================================================

    ''' <summary>Quello che dice l'assistente: a sinistra, su fondo chiaro.</summary>
    Private Sub AggiungiBollaAssistente(testo As String)

        AggiungiBolla({RigaDiTesto(testo, StileApp.FontTesto, StileApp.TestoPrimario)},
                      StileApp.FondoCasella, StileApp.BordoForte, aDestra:=False, nome:="bollaAssistente")

    End Sub

    ''' <summary>Quello che dice l'utente: a destra, con il colore della selezione.</summary>
    Private Sub AggiungiBollaUtente(testo As String)

        AggiungiBolla({RigaDiTesto(testo, StileApp.FontTesto, StileApp.TestoPrimario)},
                      StileApp.AccentoTenue, StileApp.Accento, aDestra:=True, nome:="bollaUtente")

    End Sub

    ''' <summary>
    ''' Una scheda di conferma: il «ho capito questo: giusto?» del cap. 12. Si distingue
    ''' dalle bolle di conversazione perché è il punto in cui l'utente decide, e deve
    ''' saltare all'occhio fra le altre.
    ''' </summary>
    Private Sub AggiungiScheda(scheda As Scheda)

        Dim righe As New List(Of Label)

        If Not String.IsNullOrEmpty(scheda.Titolo) Then
            righe.Add(RigaDiTesto(scheda.Titolo, StileApp.FontTitoloGruppo, StileApp.RossoTitoli))
        End If

        For Each riga As RigaScheda In scheda.Righe
            righe.Add(RigaDiTesto(TestoDellaRiga(riga), StileApp.FontTesto, StileApp.TestoPrimario))
        Next

        ' Fondo come le bolle dell'assistente, ma bordo dell'accento: è lui a dire che
        ' questa non è una frase, è la cosa su cui l'utente deve pronunciarsi.
        AggiungiBolla(righe.ToArray(), StileApp.FondoCasella, StileApp.Accento,
                      aDestra:=False, nome:="scheda")

    End Sub

    ''' <summary>«Ruolo: Magazziniere», oppure «• Uso del muletto» nelle schede a elenco.</summary>
    Private Shared Function TestoDellaRiga(riga As RigaScheda) As String

        If riga.Etichetta <> "" Then Return $"{riga.Etichetta}: {riga.Valore}"
        Return "• " & riga.Valore

    End Function

    Private Shared Function RigaDiTesto(testo As String, carattere As Font, colore As Color) As Label

        Return New Label With {
            .AutoSize = True,
            .Font = carattere,
            .ForeColor = colore,
            .Text = If(testo, "")}

    End Function

    ''' <summary>
    ''' Mette una bolla in fondo alla conversazione. Ogni bolla è fatta di tre pannelli:
    ''' la <b>riga</b>, larga quanto l'area, che serve ad allineare a sinistra o a destra;
    ''' la <b>cornice</b>, che è il bordo di 1 px; e il <b>fondo</b> colorato con dentro
    ''' le righe di testo.
    ''' </summary>
    Private Sub AggiungiBolla(righe As Label(), sfondo As Color, bordo As Color,
                              aDestra As Boolean, nome As String)

        Dim fondo As New Panel With {
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .BackColor = sfondo,
            .Padding = New Padding(RientroBolla)}

        For Each riga As Label In righe
            fondo.Controls.Add(riga)
        Next

        Dim cornice As New Panel With {
            .AutoSize = True,
            .AutoSizeMode = AutoSizeMode.GrowAndShrink,
            .BackColor = bordo,
            .Name = nome,
            .Padding = New Padding(1)}

        ' Il fondo va spostato di un pixel: un Panel non è un contenitore che dispone i
        ' figli, e da fermo a (0,0) lascerebbe scoperti solo i due lati opposti — che a
        ' video sono un'ombra storta, non un bordo.
        fondo.Location = New Point(1, 1)
        cornice.Controls.Add(fondo)

        ' Lo stacco fra una bolla e l'altra sta *dentro* la riga, non nel suo margine:
        ' così l'altezza di una riga è tutto quello che quella riga occupa, e quanto
        ' scorrere resta una somma di altezze — senza margini da rincorrere.
        Dim contenitore As New Panel With {
            .Margin = New Padding(0),
            .Name = "riga",
            .Tag = aDestra}
        contenitore.Controls.Add(cornice)

        flpConversazione.Controls.Add(contenitore)
        DisponiBolla(contenitore)

    End Sub

    ''' <summary>
    ''' Dà a una bolla la sua forma: il testo va a capo entro la larghezza concessa, i
    ''' pannelli si stringono attorno, e la bolla si accosta al suo lato.
    ''' </summary>
    Private Sub DisponiBolla(contenitore As Panel)

        Dim cornice As Panel = TryCast(contenitore.Controls.Cast(Of Control)().FirstOrDefault(), Panel)
        Dim fondo As Panel = TryCast(cornice?.Controls.Cast(Of Control)().FirstOrDefault(), Panel)
        If fondo Is Nothing Then Return

        Dim larghezzaTesto As Integer = LarghezzaMassimaBolla() - RientroBolla * 2 - 2
        Dim alto As Integer = RientroBolla

        For Each riga As Label In fondo.Controls.OfType(Of Label)()
            riga.MaximumSize = New Size(larghezzaTesto, 0)
            riga.Location = New Point(RientroBolla, alto)
            alto += riga.PreferredSize.Height + 2
        Next

        ' I due pannelli hanno AutoSize: il conto lo rifanno da soli, ma va chiesto
        ' adesso, perché la larghezza della riga si legge subito dopo.
        fondo.PerformLayout()
        cornice.PerformLayout()

        contenitore.Size = New Size(Math.Max(cornice.Width, LarghezzaDisponibile()),
                                    cornice.Height + DistanzaFraBolle)
        cornice.Left = If(CBool(contenitore.Tag), contenitore.Width - cornice.Width, 0)

    End Sub

    ''' <summary>Rifà il layout di tutte le bolle: serve a ogni cambio di larghezza.</summary>
    Private Sub RidisponiLeBolle()

        If flpConversazione.Controls.Count = 0 Then Return

        flpConversazione.SuspendLayout()
        For Each contenitore As Panel In flpConversazione.Controls.OfType(Of Panel)()
            DisponiBolla(contenitore)
        Next
        flpConversazione.ResumeLayout()

    End Sub

    ''' <summary>
    ''' Quanto è larga una riga di conversazione: tutta l'area, meno l'aria attorno —
    ''' che è il rientro del pannello di scorrimento, non del flusso delle bolle.
    ''' </summary>
    Private Function LarghezzaDisponibile() As Integer

        Return Math.Max(LarghezzaMinimaBolla, flpConversazione.ClientSize.Width)

    End Function

    Private Function LarghezzaMassimaBolla() As Integer

        Return Math.Max(LarghezzaMinimaBolla, CInt(LarghezzaDisponibile() * QuotaLarghezzaBolla))

    End Function

    ''' <summary>
    ''' Porta la conversazione in fondo dopo ogni mossa. Non basta chiedere di «portare
    ''' in vista» l'ultima bolla: quello si ferma al bordo del controllo e lascia fuori
    ''' il margine, così l'ultima riga resta tagliata a metà proprio quando è quella che
    ''' l'utente deve leggere.
    ''' </summary>
    Private Sub ScorriInFondo()

        If flpConversazione.Controls.Count = 0 Then Return

        pnlScorrimento.PerformLayout()

        ' L'elenco delle bolle è alto quanto il suo contenuto: quello che c'è da
        ' scorrere è la sua altezza, più l'aria attorno, meno la finestrella che se ne
        ' vede.
        Dim daScorrere As Integer = flpConversazione.Height + pnlScorrimento.Padding.Vertical -
                                    pnlScorrimento.ClientSize.Height
        If daScorrere <= 0 Then Return

        pnlScorrimento.AutoScrollPosition = New Point(0, daScorrere)

    End Sub

    ''' <summary>Butta via le bolle e smaltisce i controlli: una conversazione nuova parte pulita.</summary>
    Private Sub SvuotaLaConversazione()

        flpConversazione.SuspendLayout()

        For Each figlio As Control In flpConversazione.Controls.Cast(Of Control)().ToList()
            flpConversazione.Controls.Remove(figlio)
            figlio.Dispose()
        Next

        flpConversazione.ResumeLayout()

    End Sub

    ' ==================================================================
    ' Consegnare il profilo, tornare indietro
    ' ==================================================================

    Private Async Sub btnPortaNelProfilo_Click(sender As Object, e As EventArgs) Handles btnPortaNelProfilo.Click

        If _modo = ModoDialogo.Brainstorming Then
            Await TrasformaInAppuntiAsync()
            Return
        End If

        RaiseEvent ProfiloPronto(Me, EventArgs.Empty)

    End Sub

    Private Sub btnTornaAlProfilo_Click(sender As Object, e As EventArgs) Handles btnTornaAlProfilo.Click

        If _modo = ModoDialogo.Brainstorming Then
            RaiseEvent TornaAllaCandidatura(Me, EventArgs.Empty)
            Return
        End If

        RaiseEvent TornaAlProfilo(Me, EventArgs.Empty)

    End Sub

    ' ==================================================================
    ' Aspetto, spazio e stato dei comandi
    ' ==================================================================

    ''' <inheritdoc/>
    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        ' A cedere il posto al logo è la fascia delle azioni, dove ci sono bottoni e non
        ' dati (v. IPannelloArea). L'altezza la decide la fascia stessa: almeno quella che
        ' il logo sfonda, di più se i comandi devono andare a capo (cap. 03.4).
        _ingombroLogo = ingombro
        pnlAzioni.Padding = New Padding(ingombro.Width + StileApp.DistanzaControlli, 0, 0, 0)

        DisponiLeAzioni()

    End Sub

    ''' <summary>
    ''' Rifà la disposizione dei comandi in fondo al pannello. La geometria la sa la
    ''' <see cref="FasciaDeiComandi"/>, che è di tutti i pannelli; qui resta la sola cosa
    ''' che sa questo pannello — <b>quali</b> comandi vanno da che parte.
    ''' </summary>
    Private Sub DisponiLeAzioni()

        If _comandi Is Nothing Then
            _comandi = New FasciaDeiComandi(pnlAzioni)
            _comandi.ASinistra(btnTornaAlProfilo, btnRicomincia)
            _comandi.ADestra(btnPortaNelProfilo)
        End If

        _comandi.Disponi(Math.Max(AltezzaMinimaAzioni, _ingombroLogo.Height))

    End Sub

    Private Sub PannelloDialogo_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        DisponiLeAzioni()
        RidisponiLeBolle()

    End Sub

    Private Sub VestiIBottoni()

        StileApp.VestiBottone(btnTornaAlProfilo, LivelloBottone.Neutro)

        ' Ricominciare butta via quello che l'utente ha già raccontato: pesa.
        StileApp.VestiBottone(btnRicomincia, LivelloBottone.Attenzione)

        StileApp.VestiBottone(btnInvia, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnPortaNelProfilo, LivelloBottone.AzionePrincipale)

        For Each bottone As Button In BottoniDelleScelte()
            StileApp.VestiBottone(bottone, LivelloBottone.Esplorativo)
        Next

    End Sub

    Private Sub RaccontaLoStato(testo As String, colore As Color)

        lblStatoDialogo.Text = testo
        lblStatoDialogo.ForeColor = colore

    End Sub

    ''' <summary>
    ''' Decide, in un posto solo, che cosa si può fare adesso. Le domande sono: c'è una
    ''' conversazione, che cosa sta aspettando, e l'AI sta già lavorando?
    ''' </summary>
    Private Sub AggiornaComandi()

        If _modo = ModoDialogo.Brainstorming Then
            AggiornaIComandiDelRagionamento()
            Return
        End If

        Dim conDialogo As Boolean = _dialogo IsNot Nothing
        Dim chiedeUnaRisposta As Boolean = conDialogo AndAlso _mossa IsNot Nothing AndAlso
                                           _mossa.Tipo = TipoMossa.ChiediRisposta

        ' Casella e bottone dei turni compaiono solo quando c'è davvero da scrivere: una
        ' casella accesa mentre il dialogo aspetta un bottone inviterebbe a fare la cosa
        ' sbagliata.
        txtRisposta.Visible = chiedeUnaRisposta
        btnInvia.Visible = chiedeUnaRisposta

        ' A dialogo concluso (o mai cominciato) la zona della risposta si ritira del
        ' tutto: lasciarla lì vuota apriva una fascia morta fra l'ultima bolla e i
        ' bottoni, proprio nel momento del riepilogo.
        pnlRisposta.Visible = conDialogo AndAlso _mossa IsNot Nothing AndAlso
                              _mossa.Tipo <> TipoMossa.Fine
        txtRisposta.ReadOnly = _occupato
        txtRisposta.BackColor = If(_occupato, StileApp.FondoPagina, StileApp.FondoCasella)
        btnInvia.Enabled = Not _occupato

        For Each bottone As Button In BottoniDelleScelte()
            bottone.Enabled = Not _occupato
        Next

        ' Mentre l'AI lavora non si esce e non si ricomincia: la mossa in arrivo
        ' finirebbe in un pannello che l'utente non sta più guardando.
        btnTornaAlProfilo.Enabled = Not _occupato
        btnRicomincia.Enabled = conDialogo AndAlso Not _occupato

        btnPortaNelProfilo.Enabled = conDialogo AndAlso _dialogo.Finito AndAlso Not _occupato

    End Sub

    ''' <summary>
    ''' Che cosa si può fare adesso nel ragionamento. Le domande sono le stesse dell'altra
    ''' modalità — c'è una conversazione, l'AI sta lavorando — più una: si è già detto
    ''' abbastanza da poterne distillare qualcosa?
    ''' </summary>
    Private Sub AggiornaIComandiDelRagionamento()

        Dim conConversazione As Boolean = _brainstorming IsNot Nothing

        ' Qui si scrive sempre: non ci sono turni che aspettano un bottone, è una chat.
        pnlRisposta.Visible = conConversazione
        txtRisposta.Visible = conConversazione
        btnInvia.Visible = conConversazione

        txtRisposta.ReadOnly = _occupato
        txtRisposta.BackColor = If(_occupato, StileApp.FondoPagina, StileApp.FondoCasella)

        ' Mentre l'AI scrive il bottone non si spegne: diventa il modo di fermarla. È la
        ' differenza che lo streaming si porta dietro — c'è qualcosa da interrompere,
        ' perché c'è qualcosa che sta già comparendo (cap. 02.6).
        btnInvia.Enabled = True
        btnInvia.Text = If(_occupato, "Interrompi", "Invia")
        StileApp.VestiBottone(btnInvia, If(_occupato, LivelloBottone.Attenzione, LivelloBottone.AzionePrincipale))

        btnTornaAlProfilo.Enabled = Not _occupato
        btnRicomincia.Enabled = conConversazione AndAlso Not _occupato

        ' Senza almeno una battuta dell'utente non c'è niente da distillare: il prompt
        ' risponderebbe una lista vuota, e sarebbe un'attesa pagata per un nulla di fatto.
        btnPortaNelProfilo.Enabled = conConversazione AndAlso Not _occupato AndAlso
                                     _brainstorming.SiPuoDistillare

    End Sub

End Class
