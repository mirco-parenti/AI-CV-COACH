Imports System.Drawing
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Motore

''' <summary>
''' Pannello P5 — il dialogo guidato (cap. 03.6; cap. 12, flusso B): la conversazione
''' che costruisce il profilo da zero, per chi un CV non ce l'ha.
''' </summary>
''' <remarks>
''' <para>Qui dentro non c'è <b>nessuna</b> regola del dialogo: l'ordine dei turni, le
''' schede di conferma, l'anti-perdita e il riepilogo stanno tutti in
''' <see cref="DialogoProfilo"/>, che è collaudato senza interfaccia. Questo pannello fa
''' due sole cose — disegna la <see cref="Mossa"/> che riceve e riporta indietro quello
''' che l'utente scrive o sceglie.</para>
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

    ''' <summary>Chiede alla finestra di riportare in vista la scheda del profilo.</summary>
    Public Event TornaAlProfilo As EventHandler

    ''' <summary>
    ''' Dice alla finestra che il profilo raccolto è pronto da consegnare a P2; il
    ''' profilo si legge da <see cref="ProfiloCostruito"/>.
    ''' </summary>
    Public Event ProfiloPronto As EventHandler

    Public Sub New()

        InitializeComponent()

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

    Private Async Sub btnRicomincia_Click(sender As Object, e As EventArgs) Handles btnRicomincia.Click

        ' Quello che si butta è il racconto dell'utente: si chiede sempre.
        If _dialogo IsNot Nothing Then
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

        If inCorso Then RaccontaLoStato("Sto leggendo la tua risposta…", StileApp.TestoSecondario)

        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Disegna una mossa: prima quello che l'assistente dice, poi le parole dell'utente
    ''' che si stanno recuperando, poi le schede, e infine si prepara a ricevere.
    ''' </summary>
    Private Sub Mostra(mossa As Mossa)

        If mossa Is Nothing Then Return
        _mossa = mossa

        flpConversazione.SuspendLayout()

        For Each detto As String In mossa.Detto
            AggiungiBollaAssistente(detto)
        Next

        ' L'eco viene dopo tutto ciò che l'assistente ha detto e prima delle schede: è la
        ' frase dell'utente che il dialogo ha appena annunciato di voler recuperare, e le
        ' schede sono quello che ne ha ricavato.
        RimettiInBoccaLEco(mossa)

        For Each scheda As Scheda In mossa.Schede
            AggiungiScheda(scheda)
        Next

        flpConversazione.ResumeLayout()
        ScorriInFondo()

        PreparaLaRisposta(mossa)

    End Sub

    ''' <summary>Le parole dell'utente che il dialogo sta ripescando da un turno passato.</summary>
    Private Sub RimettiInBoccaLEco(mossa As Mossa)

        If String.IsNullOrWhiteSpace(mossa.EcoUtente) Then Return
        AggiungiBollaUtente(mossa.EcoUtente)

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

    Private Async Sub btnInvia_Click(sender As Object, e As EventArgs) Handles btnInvia.Click
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

        Await InviaLaRispostaAsync()

    End Sub

    ' ==================================================================
    ' Le bolle della conversazione
    ' ==================================================================

    ''' <summary>Quello che dice l'assistente: a sinistra, su fondo chiaro.</summary>
    Private Sub AggiungiBollaAssistente(testo As String)

        AggiungiBolla({RigaDiTesto(testo, StileApp.FontTesto, StileApp.TestoPrimario)},
                      StileApp.SfondoContenuto, StileApp.BordoForte, aDestra:=False, nome:="bollaAssistente")

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
        AggiungiBolla(righe.ToArray(), StileApp.SfondoContenuto, StileApp.Accento,
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

    Private Sub btnPortaNelProfilo_Click(sender As Object, e As EventArgs) Handles btnPortaNelProfilo.Click
        RaiseEvent ProfiloPronto(Me, EventArgs.Empty)
    End Sub

    Private Sub btnTornaAlProfilo_Click(sender As Object, e As EventArgs) Handles btnTornaAlProfilo.Click
        RaiseEvent TornaAlProfilo(Me, EventArgs.Empty)
    End Sub

    ' ==================================================================
    ' Aspetto, spazio e stato dei comandi
    ' ==================================================================

    ''' <inheritdoc/>
    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        ' Come in P2: a cedere il posto al logo è la fascia delle azioni, dove ci sono
        ' bottoni e non dati (v. IPannelloArea).
        pnlAzioni.Height = Math.Max(AltezzaMinimaAzioni, ingombro.Height)
        pnlAzioni.Padding = New Padding(ingombro.Width + StileApp.DistanzaControlli, 0, 0, 0)

        DisponiLeAzioni()

    End Sub

    ''' <summary>Mette i bottoni sul fondo della fascia: si torna indietro a sinistra, si va avanti a destra.</summary>
    Private Sub DisponiLeAzioni()

        Dim riga As Integer = pnlAzioni.Height - StileApp.MargineRiquadro - StileApp.BottoneStandard.Height

        Dim sinistra As Integer = pnlAzioni.Padding.Left
        For Each bottone As Button In {btnTornaAlProfilo, btnRicomincia}
            bottone.Location = New Point(sinistra, riga)
            sinistra += bottone.Width + StileApp.DistanzaControlli
        Next

        btnPortaNelProfilo.Location = New Point(
            pnlAzioni.ClientSize.Width - StileApp.MargineRiquadro - btnPortaNelProfilo.Width, riga)

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

        Dim conDialogo As Boolean = _dialogo IsNot Nothing
        Dim chiedeUnaRisposta As Boolean = conDialogo AndAlso _mossa IsNot Nothing AndAlso
                                           _mossa.Tipo = TipoMossa.ChiediRisposta

        ' Casella e bottone dei turni compaiono solo quando c'è davvero da scrivere: una
        ' casella accesa mentre il dialogo aspetta un bottone inviterebbe a fare la cosa
        ' sbagliata.
        txtRisposta.Visible = chiedeUnaRisposta
        btnInvia.Visible = chiedeUnaRisposta
        txtRisposta.ReadOnly = _occupato
        txtRisposta.BackColor = If(_occupato, StileApp.SfondoBase, StileApp.SfondoContenuto)
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

End Class
