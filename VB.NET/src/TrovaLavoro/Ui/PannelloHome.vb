Imports System.Drawing
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Windows.Forms
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

''' <summary>
''' L'utente ha scelto una candidatura dalla coda: eccola, già riletta dal disco.
''' </summary>
Public Class CandidaturaSceltaEventArgs
    Inherits EventArgs

    Public Sub New(candidatura As Opportunita)
        Me.Candidatura = candidatura
    End Sub

    ''' <summary>La candidatura come stava nella sua cartella: annuncio, giudizi, documenti.</summary>
    Public ReadOnly Property Candidatura As Opportunita

End Class

''' <summary>
''' Pannello P1 — la Home (cap. 03.6): lo stato del profilo, la coda delle opportunità
''' con stelle e stati, i contatori e le scorciatoie ai flussi. È il cruscotto che
''' risponde alla domanda del cap. 07.3, «a che punto sono?».
''' </summary>
''' <remarks>
''' <para><b>Qui si guarda, non si decide.</b> Il pannello non calcola niente e non
''' cambia lo stato di nessuna candidatura: legge il registro (cap. 07.3), lo mostra, e
''' quando si sceglie una riga passa la palla a P4, che è dove la candidatura si tocca.
''' Nemmeno lo scarto sta qui — sta nella scheda che si sta guardando.</para>
''' <para><b>Chiude il debito di T4</b>: «la coda dell'opportunità non si riapre»
''' (<c>in_sospeso.md</c>). Da T4 ogni candidatura finiva nella sua cartella e ci restava
''' senza che l'interfaccia sapesse tornarci: la promessa «tutto riapribile» del
''' cap. 12.7 era mantenuta sul disco e non a video. Da qui si riapre.</para>
''' </remarks>
Public Class PannelloHome
    Implements IPannelloArea

    ''' <summary>Sotto questa altezza la fascia delle azioni non scende: i bottoni ci devono stare.</summary>
    Private Const AltezzaMinimaAzioni As Integer = 60

    ''' <summary>Quante stelle ha la scala: quelle piene e quelle vuote insieme.</summary>
    Private Const StelleDellaScala As Integer = 5

    ''' <summary>Sotto questa larghezza la colonna del ruolo non scende, per stretta che sia la finestra.</summary>
    Private Const LarghezzaMinimaRuolo As Integer = 260

    ' Le colonne della coda, nell'ordine in cui il designer le dichiara: servono a
    ' sapere per quale si sta ordinando.
    Private Const ColonnaMatch As Integer = 0
    Private Const ColonnaAzienda As Integer = 1
    Private Const ColonnaRuolo As Integer = 2
    Private Const ColonnaStato As Integer = 3
    Private Const ColonnaFonte As Integer = 4
    Private Const ColonnaQuando As Integer = 5

    ' Le voci del filtro «Mostra», nell'ordine in cui entrano nella tendina.
    Private Const FiltroTutte As Integer = 0
    Private Const FiltroDaCompletare As Integer = 1
    Private Const FiltroGenerate As Integer = 2
    Private Const FiltroScartate As Integer = 3

    Private ReadOnly _suggerimenti As New ToolTip()

    Private _contesto As ContestoApp

    ''' <summary>L'indice come si è letto l'ultima volta; <c>Nothing</c> finché non si legge.</summary>
    Private _registro As Registro

    ''' <summary>Per quale colonna è ordinata la coda, e in che verso.</summary>
    Private _ordinePer As Integer = ColonnaQuando
    Private _discendente As Boolean = True

    ''' <summary>L'utente vuole vedere o correggere il proprio profilo: si va in P2.</summary>
    Public Event ProfiloRichiesto As EventHandler

    ''' <summary>L'utente vuole cercare un annuncio nuovo: si va in P3.</summary>
    Public Event RicercaRichiesta As EventHandler

    ''' <summary>L'utente ha scelto una candidatura della coda: si va in P4, con lei.</summary>
    Public Event CandidaturaScelta As EventHandler(Of CandidaturaSceltaEventArgs)

    Public Sub New()

        InitializeComponent()

        ' Come negli altri pannelli: il ToolTip nasce qui e muore col pannello.
        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        VestiIBottoni()
        RiempiIlFiltro()
        DichiaraLeTappeCheMancano()
        AggiornaComandi()

    End Sub

    ''' <summary>Collega il cruscotto al motore e mostra subito com'è messo il mondo.</summary>
    Public Sub Collega(contesto As ContestoApp)

        If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))

        _contesto = contesto
        Aggiorna()

    End Sub

    ''' <summary>
    ''' Ogni volta che il cruscotto torna in vista si rilegge tutto (cap. 03.8).
    ''' </summary>
    ''' <remarks>
    ''' Qui la regola morde più che altrove: fra un'occhiata e l'altra l'utente ha
    ''' analizzato un annuncio, generato dei documenti, scartato una candidatura o salvato
    ''' il profilo. Un cruscotto che mostra la situazione di dieci minuti fa è peggio di
    ''' un cruscotto vuoto, perché sembra vero.
    ''' </remarks>
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)

        MyBase.OnVisibleChanged(e)

        If Visible AndAlso _contesto IsNot Nothing Then Aggiorna()

    End Sub

    ''' <summary>
    ''' Rilegge il profilo e il registro e rifà quel che si vede. È pubblico perché lo
    ''' chiama anche il banco, che di un pannello nascosto non può aspettare che compaia.
    ''' </summary>
    Public Sub Aggiorna()

        MostraIlProfilo()
        LeggiIlRegistro()
        RiempiLaCoda()
        AggiornaComandi()

    End Sub

    ''' <summary>La candidatura scelta nella coda; <c>Nothing</c> se non ce n'è una.</summary>
    Public ReadOnly Property VoceScelta As VoceRegistro
        Get
            Return TryCast(lvwCoda.SelectedItems.Cast(Of ListViewItem)().FirstOrDefault()?.Tag, VoceRegistro)
        End Get
    End Property

    ' ==================================================================
    ' Lo stato del profilo
    ' ==================================================================

    ''' <summary>
    ''' La prima domanda del cruscotto: il profilo c'è, ed è di quando? Senza di lui non
    ''' si fa nient'altro, e il bottone accanto cambia mestiere — si costruisce, o si apre.
    ''' </summary>
    Private Sub MostraIlProfilo()

        If _contesto Is Nothing Then Return

        If Not _contesto.Archivio.Esiste Then
            lblProfilo.Text = "Non c'è ancora, ed è da lì che si comincia: da un CV che hai già, " &
                              "oppure costruendolo insieme parlando."
            lblProfilo.ForeColor = StileApp.TestoPrimario
            btnApriProfilo.Text = "Costruisci il profilo"
            Return
        End If

        btnApriProfilo.Text = "Apri il profilo"

        Try
            Dim profilo As Profilo = _contesto.Archivio.Carica()
            Dim quando As Date? = _contesto.Archivio.UltimoSalvataggio

            lblProfilo.Text = If(String.IsNullOrWhiteSpace(profilo.Nome), "Il tuo profilo", profilo.Nome.Trim()) &
                              If(quando.HasValue,
                                 $" — salvato il {quando.Value:dd/MM/yyyy} alle {quando.Value:HH:mm}",
                                 String.Empty)
            lblProfilo.ForeColor = StileApp.TestoPrimario

        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                   OrElse TypeOf ex Is UnauthorizedAccessException
            ' Il profilo è la sola cosa che l'utente non può rigenerare (cap. 11.1): se
            ' non si legge lo si dice subito, e si manda dove c'è scritto come rimediare.
            lblProfilo.Text = $"C'è, ma non si lascia leggere: {ex.Message}" & vbLf &
                              "Aprilo dalla scheda «Profilo»: lì trovi come rimediare."
            lblProfilo.ForeColor = StileApp.Pericolo
        End Try

    End Sub

    ' ==================================================================
    ' La coda delle candidature
    ' ==================================================================

    ''' <summary>
    ''' Rilegge l'indice e, se è stato ricostruito, lo rimette su disco: chi guarda
    ''' l'elenco è anche chi lo tiene in riga.
    ''' </summary>
    Private Sub LeggiIlRegistro()

        If _contesto Is Nothing Then Return

        Try
            _registro = _contesto.Registro.Carica()

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' Carica non solleva per un indice rotto — al massimo lo rifà — ma la cartella
            ' dati potrebbe non lasciarsi leggere affatto. Allora la coda resta vuota e lo
            ' si dice, invece di mostrare un elenco vuoto che sembra «non hai candidature».
            _registro = Nothing
            RaccontaLoStato($"Non riesco a leggere le candidature: {ex.Message}", StileApp.Pericolo)
            Return
        End Try

        RaccontaLoStato(If(_registro.Avviso, String.Empty),
                        If(_registro.Avviso Is Nothing, StileApp.TestoSecondario, StileApp.Pericolo))

        Try
            _contesto.Registro.SalvaSeServe(_registro)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' Un indice che non si lascia scrivere non toglie niente a nessuno: le
            ' candidature sono nelle loro cartelle, e la prossima volta si rifà.
        End Try

    End Sub

    ''' <summary>Rifà le righe della coda: prima il filtro, poi l'ordine, poi il disegno.</summary>
    Private Sub RiempiLaCoda()

        lvwCoda.BeginUpdate()
        lvwCoda.Items.Clear()

        For Each voce As VoceRegistro In Ordinate(Filtrate())
            lvwCoda.Items.Add(RigaDellaVoce(voce))
        Next

        lvwCoda.EndUpdate()

        MostraIContatori()

    End Sub

    ''' <summary>
    ''' I contatori del cap. 07.3. Le <b>inviate</b> non ci sono: quel numero sarebbe
    ''' sempre zero finché T6 non arriva, e un contatore che non può muoversi non conta
    ''' niente — lo aggiungerà la tappa che lo fa salire.
    ''' </summary>
    Private Sub MostraIContatori()

        If _registro Is Nothing OrElse _registro.Voci.Count = 0 Then
            lblContatori.Text = "Nessuna candidatura, per ora."
            Return
        End If

        Dim daCompletare As Integer = _registro.Quante(StatoOpportunita.Nuova) +
                                      _registro.Quante(StatoOpportunita.Interessante)

        lblContatori.Text = String.Join(" · ", {
            $"{daCompletare} da completare",
            Contate(_registro.Quante(StatoOpportunita.Generata), "generata", "generate"),
            Contate(_registro.Quante(StatoOpportunita.Scartata), "scartata", "scartate")})

    End Sub

    ''' <summary>
    ''' Un contatore con la sua parola concordata: «1 generata», non «1 generate».
    ''' </summary>
    ''' <remarks>
    ''' Visto sull'applicazione vera al collaudo di T5c (2026-08-12), quando il primo scarto
    ''' ha fatto comparire «1 scartate». Il numero è quasi sempre più di uno e la stonatura
    ''' non si nota quasi mai — ma l'uno capita proprio la prima volta che si prova una cosa,
    ''' che è il momento in cui l'utente guarda con più attenzione. «Da completare» non ha
    ''' bisogno di questo: vale già per uno e per molti.
    ''' </remarks>
    Private Shared Function Contate(quante As Integer, singolare As String, plurale As String) As String

        Return $"{quante} {If(quante = 1, singolare, plurale)}"

    End Function

    ''' <summary>Le voci che il filtro «Mostra» lascia passare.</summary>
    Private Function Filtrate() As IEnumerable(Of VoceRegistro)

        If _registro Is Nothing Then Return Enumerable.Empty(Of VoceRegistro)()

        Select Case cboMostra.SelectedIndex

            Case FiltroDaCompletare
                Return _registro.Voci.Where(Function(v) v.Stato = StatoOpportunita.Nuova OrElse
                                                        v.Stato = StatoOpportunita.Interessante)
            Case FiltroGenerate
                Return _registro.Voci.Where(Function(v) v.Stato = StatoOpportunita.Generata)

            Case FiltroScartate
                Return _registro.Voci.Where(Function(v) v.Stato = StatoOpportunita.Scartata)

            Case Else
                Return _registro.Voci

        End Select

    End Function

    ''' <summary>
    ''' Le voci nell'ordine scelto cliccando un'intestazione (cap. 07.3: l'elenco è
    ''' ordinabile). L'ordine si fa <b>qui</b> e non con un comparatore della lista: così
    ''' le righe si disegnano già in ordine, e il banco può leggerlo senza chiedere niente
    ''' a Windows.
    ''' </summary>
    Private Function Ordinate(voci As IEnumerable(Of VoceRegistro)) As List(Of VoceRegistro)

        Dim elenco As New List(Of VoceRegistro)(voci)

        elenco.Sort(AddressOf ConfrontaSecondoLaColonna)
        If _discendente Then elenco.Reverse()

        Return elenco

    End Function

    ''' <summary>
    ''' Il confronto fra due righe secondo la colonna scelta. A parità decide il nome
    ''' della cartella: senza, due candidature con le stesse stelle si scambierebbero di
    ''' posto a ogni ridisegno.
    ''' </summary>
    Private Function ConfrontaSecondoLaColonna(a As VoceRegistro, b As VoceRegistro) As Integer

        Dim esito As Integer

        Select Case _ordinePer

            Case ColonnaMatch
                esito = Nullable.Compare(Of Double)(a.Stelle, b.Stelle)

            Case ColonnaAzienda
                esito = String.Compare(a.Azienda, b.Azienda, StringComparison.CurrentCultureIgnoreCase)

            Case ColonnaRuolo
                esito = String.Compare(a.Titolo, b.Titolo, StringComparison.CurrentCultureIgnoreCase)

            Case ColonnaStato
                esito = a.Stato.CompareTo(b.Stato)

            Case ColonnaFonte
                esito = String.Compare(a.Fonte, b.Fonte, StringComparison.CurrentCultureIgnoreCase)

            Case Else
                esito = a.Aggiornata.CompareTo(b.Aggiornata)

        End Select

        If esito <> 0 Then Return esito

        Return String.Compare(a.Cartella, b.Cartella, StringComparison.Ordinal)

    End Function

    ''' <summary>
    ''' Una riga della coda. Le scartate si scrivono in grigio: restano nell'elenco —
    ''' scartare non è cancellare — ma non devono pesare sull'occhio come quelle vive.
    ''' </summary>
    Private Shared Function RigaDellaVoce(voce As VoceRegistro) As ListViewItem

        Dim riga As New ListViewItem({
            MatchScritto(voce),
            If(String.IsNullOrWhiteSpace(voce.Azienda), "(azienda non dichiarata)", voce.Azienda),
            If(String.IsNullOrWhiteSpace(voce.Titolo), "(ruolo non dichiarato)", voce.Titolo),
            StatiOpportunita.Etichetta(voce.Stato),
            If(String.IsNullOrWhiteSpace(voce.Fonte), "incollato a mano", voce.Fonte),
            QuandoScritto(voce.Aggiornata)}) With {.Tag = voce}

        If voce.Stato = StatoOpportunita.Scartata Then riga.ForeColor = StileApp.TestoSecondario

        Return riga

    End Function

    ''' <summary>
    ''' Il match in una colonna stretta: le stelle, il numero, e il ⛔ quando un requisito
    ''' eliminatorio è rimasto scoperto (cap. 07.3). È più corto della scritta di P4 —
    ''' che ha una riga tutta per sé e dice «su 5» — ed è voluto: qui è una colonna.
    ''' </summary>
    Private Shared Function MatchScritto(voce As VoceRegistro) As String

        If Not voce.Stelle.HasValue Then Return "—"

        Dim piene As Integer = Math.Max(0, Math.Min(StelleDellaScala, CInt(Math.Floor(voce.Stelle.Value))))

        Return New String("★"c, piene) & New String("☆"c, StelleDellaScala - piene) &
               " " & voce.Stelle.Value.ToString("0.0", CultureInfo.CurrentCulture) &
               If(voce.GateEliminatorio, " ⛔", String.Empty)

    End Function

    ''' <summary>Una data come si legge in una colonna; il trattino se non c'è.</summary>
    Private Shared Function QuandoScritto(istante As Date) As String

        If istante = Nothing Then Return "—"

        Return istante.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture)

    End Function

    ' ==================================================================
    ' I comandi
    ' ==================================================================

    ''' <summary>
    ''' Riapre la candidatura scelta: la rilegge dalla sua cartella e la consegna alla
    ''' finestra, che la porta in P4. È il debito di T4 che si chiude (cap. 12.7).
    ''' </summary>
    Private Sub ApriLaCandidaturaScelta()

        Dim voce As VoceRegistro = VoceScelta
        If voce Is Nothing OrElse _contesto Is Nothing Then Return

        Dim dove As String = Path.Combine(_contesto.Cartella.CartellaOpportunita, voce.Cartella)

        Try
            RaiseEvent CandidaturaScelta(
                Me, New CandidaturaSceltaEventArgs(_contesto.Opportunita.Carica(dove)))

        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                   OrElse TypeOf ex Is UnauthorizedAccessException
            RaccontaLoStato($"«{voce.Cartella}» non si è lasciata riaprire: {ex.Message}",
                            StileApp.Pericolo)
        End Try

    End Sub

    Private Sub btnApriCandidatura_Click(sender As Object, e As EventArgs) Handles btnApriCandidatura.Click
        ApriLaCandidaturaScelta()
    End Sub

    ''' <summary>Il doppio clic sulla riga è la strada breve per lo stesso posto.</summary>
    Private Sub lvwCoda_DoubleClick(sender As Object, e As EventArgs) Handles lvwCoda.DoubleClick
        ApriLaCandidaturaScelta()
    End Sub

    Private Sub lvwCoda_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lvwCoda.SelectedIndexChanged

        AggiornaComandi()

    End Sub

    ''' <summary>
    ''' Cliccando un'intestazione si ordina per quella colonna; ricliccandola si gira il
    ''' verso. Una colonna nuova parte dal verso che si vuole quasi sempre: le stelle e le
    ''' date dalla più alta e dalla più recente, i nomi dalla A.
    ''' </summary>
    Private Sub lvwCoda_ColumnClick(sender As Object, e As ColumnClickEventArgs) Handles lvwCoda.ColumnClick
        OrdinaPer(e.Column)
    End Sub

    ''' <summary>
    ''' Ordina la coda per quella colonna; richiamandola sulla stessa gira il verso. È il
    ''' metodo che il clic sull'intestazione chiama, ed è pubblico per la stessa ragione
    ''' per cui in P4 lo è «analizza»: il banco non ha un modo di far cliccare
    ''' l'intestazione di una lista, e questa è la cosa che il clic fa.
    ''' </summary>
    Public Sub OrdinaPer(colonna As Integer)

        If colonna = _ordinePer Then
            _discendente = Not _discendente
        Else
            _ordinePer = colonna
            _discendente = (colonna = ColonnaMatch OrElse colonna = ColonnaQuando)
        End If

        RiempiLaCoda()

    End Sub

    Private Sub cboMostra_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cboMostra.SelectedIndexChanged

        RiempiLaCoda()
        AggiornaComandi()

    End Sub

    Private Sub btnApriProfilo_Click(sender As Object, e As EventArgs) Handles btnApriProfilo.Click
        RaiseEvent ProfiloRichiesto(Me, EventArgs.Empty)
    End Sub

    Private Sub btnNuovaRicerca_Click(sender As Object, e As EventArgs) Handles btnNuovaRicerca.Click
        RaiseEvent RicercaRichiesta(Me, EventArgs.Empty)
    End Sub

    ' ==================================================================
    ' Aspetto, spazio e stato dei comandi
    ' ==================================================================

    ''' <inheritdoc/>
    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        ' Come negli altri pannelli: a cedere il posto al logo è la fascia delle azioni,
        ' dove ci sono bottoni e non dati (v. IPannelloArea).
        pnlAzioni.Height = Math.Max(AltezzaMinimaAzioni, ingombro.Height)
        pnlAzioni.Padding = New Padding(ingombro.Width + StileApp.DistanzaControlli, 0, 0, 0)

        DisponiLeAzioni()

    End Sub

    ''' <summary>Le vie laterali a sinistra, l'avanti a destra: come in P4.</summary>
    Private Sub DisponiLeAzioni()

        Dim riga As Integer = pnlAzioni.Height - StileApp.MargineRiquadro - StileApp.BottoneStandard.Height

        Dim sinistra As Integer = pnlAzioni.Padding.Left
        For Each bottone As Button In {btnApriCandidatura, btnAggiornaProfilo}
            bottone.Location = New Point(sinistra, riga)
            sinistra += bottone.Width + StileApp.DistanzaControlli
        Next

        btnNuovaRicerca.Location = New Point(
            pnlAzioni.ClientSize.Width - StileApp.MargineRiquadro - btnNuovaRicerca.Width, riga)

    End Sub

    Private Sub PannelloHome_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize

        DisponiLeAzioni()
        AdattaLeColonne()

    End Sub

    ''' <summary>
    ''' Dà al <b>ruolo</b> lo spazio che avanza. Le altre colonne hanno una misura che
    ''' basta a sé — le stelle, uno stato, una data — mentre il ruolo è la voce che si
    ''' allunga davvero («Addetto/a Delivery Area Nord Ovest…»), ed è quella che si vuole
    ''' leggere per intero.
    ''' </summary>
    ''' <remarks>
    ''' Senza questo, su uno schermo largo la tabella restava della misura del designer e
    ''' metà pannello era bianco, con i ruoli tagliati a metà: visto sull'applicazione vera
    ''' il 2026-08-12, che è l'unico posto in cui una cosa così si vede.
    ''' </remarks>
    Private Sub AdattaLeColonne()

        Dim fisse As Integer = colMatch.Width + colAzienda.Width +
                               colStato.Width + colFonte.Width + colQuando.Width

        colRuolo.Width = Math.Max(LarghezzaMinimaRuolo,
                                  lvwCoda.ClientSize.Width - fisse - SystemInformation.VerticalScrollBarWidth)

    End Sub

    Private Sub VestiIBottoni()

        ' Dal cruscotto l'azione che porta avanti è cercare un annuncio nuovo: è lei il
        ' bottone principale del pannello (cap. 03.3).
        StileApp.VestiBottone(btnNuovaRicerca, LivelloBottone.AzionePrincipale)

        StileApp.VestiBottone(btnApriCandidatura, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnApriProfilo, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnAggiornaProfilo, LivelloBottone.Esplorativo)

    End Sub

    ''' <summary>Le voci del filtro «Mostra», nell'ordine delle costanti qui sopra.</summary>
    Private Sub RiempiIlFiltro()

        cboMostra.Items.AddRange(New Object() {"Tutte", "Da completare", "Generate", "Scartate"})
        cboMostra.SelectedIndex = FiltroTutte

    End Sub

    ''' <summary>
    ''' Le scorciatoie che il cap. 03.6 promette sono due, e una non è ancora arrivata:
    ''' resta visibile e spenta, col suo tooltip, come in P2 (cap. 03.8).
    ''' </summary>
    Private Sub DichiaraLeTappeCheMancano()

        btnAggiornaProfilo.Enabled = False
        _suggerimenti.SetToolTip(btnAggiornaProfilo,
            "La sessione di aggiornamento del profilo arriva più avanti (flusso D).")

    End Sub

    Private Sub RaccontaLoStato(testo As String, colore As Color)

        lblStatoHome.Text = testo
        lblStatoHome.ForeColor = colore

    End Sub

    ''' <summary>
    ''' Decide, in un posto solo, che cosa si può fare adesso. La domanda è una sola: c'è
    ''' una riga scelta da aprire?
    ''' </summary>
    Private Sub AggiornaComandi()

        btnApriCandidatura.Enabled = VoceScelta IsNot Nothing
        btnApriProfilo.Enabled = _contesto IsNot Nothing
        btnNuovaRicerca.Enabled = _contesto IsNot Nothing

    End Sub

End Class
