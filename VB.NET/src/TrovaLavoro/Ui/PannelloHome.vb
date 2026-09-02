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
''' Una candidatura è stata eliminata dalla coda: la sua cartella non c'è più (cap. 11.5).
''' </summary>
''' <remarks>
''' Porta il <b>percorso</b> e non la candidatura, ed è il punto: di quella non resta più
''' niente da consegnare. Serve ai pannelli che potrebbero averla in mano per riconoscere
''' se è la loro e lasciarla andare.
''' </remarks>
Public Class CandidaturaEliminataEventArgs
    Inherits EventArgs

    Public Sub New(cartella As String)
        Me.Cartella = cartella
    End Sub

    ''' <summary>La cartella che è stata mandata via, in forma piena.</summary>
    Public ReadOnly Property Cartella As String

End Class

''' <summary>
''' Pannello P1 — la Home (cap. 03.6): lo stato del profilo, la coda delle opportunità
''' con stelle e stati, i contatori e le scorciatoie ai flussi. È il cruscotto che
''' risponde alla domanda del cap. 07.3, «a che punto sono?».
''' </summary>
''' <remarks>
''' <para><b>Qui si guarda, e si toglie di mezzo.</b> Il pannello non calcola niente e
''' non cambia lo stato di nessuna candidatura: legge il registro (cap. 07.3), lo mostra,
''' e quando si sceglie una riga passa la palla a P4, che è dove la candidatura si tocca.
''' Nemmeno lo scarto sta qui — sta nella scheda che si sta guardando.</para>
''' <para>L'unica cosa che da qui si fa, e non solo si guarda, è <b>eliminare</b>
''' (cap. 11.5). Non è uno strappo alla regola di sopra: eliminare non decide niente
''' <i>sulla</i> candidatura, la toglie dall'archivio — e dopo non resterebbe nessuna
''' scheda da cui farlo. La coda è anche il solo posto da cui si vedono tutte insieme,
''' che è come ci si accorge del doppione e della prova da ripulire.</para>
''' <para><b>Chiude il debito di T4</b>: «la coda dell'opportunità non si riapre»
''' (<c>in_sospeso.md</c>). Da T4 ogni candidatura finiva nella sua cartella e ci restava
''' senza che l'interfaccia sapesse tornarci: la promessa «tutto riapribile» del
''' cap. 12.7 era mantenuta sul disco e non a video. Da qui si riapre.</para>
''' </remarks>
Public Class PannelloHome
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

    ''' <summary>Quante stelle ha la scala: quelle piene e quelle vuote insieme.</summary>
    Private Const StelleDellaScala As Integer = 5

    ''' <summary>Sotto questa larghezza la colonna del ruolo non scende, per stretta che sia la finestra.</summary>
    Private Const LarghezzaMinimaRuolo As Integer = 260

    ''' <summary>Quanto è alta la fascia dei filtri quando il promemoria non c'è, e quando c'è.</summary>
    Private Const AltezzaFiltroDaSolo As Integer = 34
    Private Const AltezzaFiltroConPromemoria As Integer = 56

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
    Private Const FiltroDaSollecitare As Integer = 3
    Private Const FiltroScartate As Integer = 4

    ''' <summary>
    ''' La prima voce del filtro «Stelle»: quella che non filtra niente. Le altre non hanno
    ''' una costante ciascuna perché <b>sono</b> il loro numero — la voce all'indice 3 è
    ''' «almeno 3 stelle» — ed è quel che rende la tendina estendibile senza toccare il
    ''' codice che filtra.
    ''' </summary>
    Private Const StelleQualunque As Integer = 0

    Private ReadOnly _suggerimenti As New ToolTip()

    Private _contesto As ContestoApp

    ''' <summary>L'indice come si è letto l'ultima volta; <c>Nothing</c> finché non si legge.</summary>
    Private _registro As Registro

    ''' <summary>
    ''' Le candidature che aspettano da troppo, com'erano al momento di disegnare la coda
    ''' (cap. 07.3): in ordine di attesa, la più vecchia per prima.
    ''' </summary>
    ''' <remarks>
    ''' Si calcolano <b>una volta sola</b> per giro di disegno, e non tre — una per il
    ''' filtro, una per le righe da segnare, una per la riga del promemoria. Non è
    ''' risparmio: è che quei tre conti devono dire la stessa cosa, e a mezzanotte in
    ''' punto tre chiamate a <c>Date.Now</c> non la direbbero.
    ''' </remarks>
    Private _solleciti As New List(Of VoceRegistro)

    ''' <summary>Le cartelle di <see cref="_solleciti"/>, per segnare le righe senza ricercare.</summary>
    Private ReadOnly _daSollecitare As New HashSet(Of String)(StringComparer.Ordinal)

    ''' <summary>Per quale colonna è ordinata la coda, e in che verso.</summary>
    Private _ordinePer As Integer = ColonnaQuando
    Private _discendente As Boolean = True

    ''' <summary>L'utente vuole vedere o correggere il proprio profilo: si va in P2.</summary>
    Public Event ProfiloRichiesto As EventHandler

    ''' <summary>L'utente vuole cercare un annuncio nuovo: si va in P3.</summary>
    Public Event RicercaRichiesta As EventHandler

    ''' <summary>L'utente ha scelto una candidatura della coda: si va in P4, con lei.</summary>
    Public Event CandidaturaScelta As EventHandler(Of CandidaturaSceltaEventArgs)

    ''' <summary>
    ''' Una candidatura è stata eliminata: chi ce l'aveva in mano deve dimenticarla
    ''' (cap. 11.5).
    ''' </summary>
    Public Event CandidaturaEliminata As EventHandler(Of CandidaturaEliminataEventArgs)

    Public Sub New()

        InitializeComponent()

        ' Come negli altri pannelli: il ToolTip nasce qui e muore col pannello.
        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        VestiIBottoni()
        RiempiIlFiltro()
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
            '
            ' Qui la riga porta la parola davanti, perché questa etichetta fa due mestieri:
            ' di solito dice chi sei e di quando è il profilo, e in questo solo caso dice
            ' che non si è potuto leggere. Distinguerli col colore soltanto è la cosa che
            ' il 2026-09-01 si è smesso di fare (v. Segnalazioni). Il soggetto è tornato
            ' nella frase — «Il profilo c'è» e non «C'è» — perché dopo un prefisso una
            ' frase deve reggersi da sola.
            lblProfilo.Text = Segnalazioni.PrefissoErrore &
                              $"Il profilo c'è, ma non si lascia leggere: {ex.Message}" & vbLf &
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
            RaccontaUnErrore($"Non riesco a leggere le candidature: {ex.Message}")
            Return
        End Try

        ' L'avviso dell'indice dice che una parte delle cartelle non si è lasciata leggere:
        ' la coda c'è e funziona, solo non è completa. È un «Attenzione», non un «Errore»
        ' (v. Segnalazioni). Senza avviso la riga si svuota, e una riga vuota non si
        ' prefissa: sarebbe una parola sola, senza niente dietro.
        If _registro.Avviso Is Nothing Then
            RaccontaLoStato(String.Empty, StileApp.TestoSecondario)
        Else
            RaccontaUnAvviso(_registro.Avviso)
        End If

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

        ' Prima di filtrare: il filtro «Da sollecitare» e le righe segnate guardano lo
        ' stesso elenco, e va calcolato una volta sola (v. _solleciti).
        RicalcolaISolleciti()

        lvwCoda.BeginUpdate()
        lvwCoda.Items.Clear()

        For Each voce As VoceRegistro In Ordinate(Filtrate())
            lvwCoda.Items.Add(RigaDellaVoce(voce, AttesaDaSegnalare(voce)))
        Next

        lvwCoda.EndUpdate()

        MostraIContatori(quanteSeNeVedono:=lvwCoda.Items.Count)
        MostraIlPromemoria()

    End Sub

    ''' <summary>
    ''' Rifà l'elenco delle candidature ferme da troppo, con la soglia scelta nelle
    ''' Impostazioni (cap. 07.3; cap. 11).
    ''' </summary>
    Private Sub RicalcolaISolleciti()

        _solleciti = If(_registro Is Nothing,
                        New List(Of VoceRegistro),
                        _registro.DaSollecitare(GiorniDiFollowUp(), Date.Now))

        _daSollecitare.Clear()
        For Each voce As VoceRegistro In _solleciti
            _daSollecitare.Add(voce.Cartella)
        Next

    End Sub

    ''' <summary>
    ''' Dopo quanti giorni di silenzio si ricorda una candidatura spedita; zero quando il
    ''' promemoria è spento o le impostazioni non sono ancora arrivate.
    ''' </summary>
    Private Function GiorniDiFollowUp() As Integer

        If _contesto Is Nothing OrElse _contesto.Impostazioni Is Nothing Then Return 0

        Return _contesto.Impostazioni.GiorniFollowUp

    End Function

    ''' <summary>
    ''' Da quanti giorni questa riga aspetta, se è una di quelle da sollecitare;
    ''' <c>Nothing</c> per tutte le altre.
    ''' </summary>
    Private Function AttesaDaSegnalare(voce As VoceRegistro) As Integer?

        If voce Is Nothing OrElse Not _daSollecitare.Contains(voce.Cartella) Then Return Nothing

        Return voce.GiorniDiAttesa(Date.Now)

    End Function

    ''' <summary>
    ''' La riga del promemoria di follow-up, sotto i contatori (cap. 07.3). Quando non c'è
    ''' niente da ricordare <b>sparisce</b>, e la fascia dei filtri torna alta una riga:
    ''' un avviso che occupa spazio anche da spento insegna a non guardarlo.
    ''' </summary>
    ''' <remarks>
    ''' È <b>passivo</b> (cap. 15.3): dice quali aspettano, e il sollecito lo scrive
    ''' l'utente. Manda al filtro invece di elencarle tutte, perché la coda è già lì
    ''' sotto ed è il posto dove si guardano.
    ''' </remarks>
    Private Sub MostraIlPromemoria()

        If _solleciti.Count = 0 Then
            lblPromemoria.Visible = False
            pnlFiltro.Height = AltezzaFiltroDaSolo
            Return
        End If

        Dim giorni As Integer = GiorniDiFollowUp()
        Dim piuVecchia As VoceRegistro = _solleciti(0)
        Dim attesa As Integer = If(piuVecchia.GiorniDiAttesa(Date.Now), giorni)

        If _solleciti.Count = 1 Then
            lblPromemoria.Text = $"⏳ Una candidatura spedita aspetta una risposta da {attesa} giorni: " &
                                 $"{DiChiParla(piuVecchia)}."
        Else
            lblPromemoria.Text = $"⏳ {_solleciti.Count} candidature spedite aspettano da più di {giorni} " &
                                 $"giorni; la più vecchia da {attesa} ({DiChiParla(piuVecchia)}). " &
                                 "Il filtro «Da sollecitare» le mostra tutte."
        End If

        lblPromemoria.Visible = True
        pnlFiltro.Height = AltezzaFiltroConPromemoria

    End Sub

    ''' <summary>Come si nomina una candidatura in una frase: azienda e ruolo, se ci sono.</summary>
    Private Shared Function DiChiParla(voce As VoceRegistro) As String

        Dim azienda As String = If(String.IsNullOrWhiteSpace(voce.Azienda), Nothing, voce.Azienda.Trim())
        Dim ruolo As String = If(String.IsNullOrWhiteSpace(voce.Titolo), Nothing, voce.Titolo.Trim())

        If azienda IsNot Nothing AndAlso ruolo IsNot Nothing Then Return $"{azienda} — {ruolo}"
        If azienda IsNot Nothing Then Return azienda
        If ruolo IsNot Nothing Then Return ruolo

        ' Un annuncio anonimo non ha né l'una né l'altro: resta il nome della cartella,
        ' che è come l'utente la ritrova su disco.
        Return voce.Cartella

    End Function

    ''' <summary>
    ''' I contatori del cap. 07.3. Le <b>inviate</b> sono entrate con T6, che è la tappa
    ''' che le fa salire: prima quel numero sarebbe stato sempre zero, e un contatore che
    ''' non può muoversi non conta niente.
    ''' </summary>
    ''' <param name="quanteSeNeVedono">
    ''' Quante righe la coda mostra adesso. I contatori restano sul <b>totale</b> — dicono
    ''' a che punto si è, non cosa si sta guardando — ma quando i filtri ne nascondono una
    ''' parte va detto: una coda che si accorcia senza spiegazione fa credere che una
    ''' candidatura sia sparita.
    ''' </param>
    Private Sub MostraIContatori(quanteSeNeVedono As Integer)

        If _registro Is Nothing OrElse _registro.Voci.Count = 0 Then
            ' Una coda vuota che dice solo di essere vuota lascia fermo chi la guarda: qui
            ' si dice anche il gesto per cominciare, e sono i due modi che ci sono. Il
            ' comando si nomina come lo porta scritto il bottone qui sotto; la scheda del
            ' confronto si nomina con la prima parola sola — il suo nome per esteso
            ' (v. NomiUi.Confronto) manderebbe questa riga a capo, e la
            ' fascia dei filtri è alta una riga.
            lblContatori.Text = $"Nessuna candidatura, per ora. Comincia da «{btnNuovaRicerca.Text}», " &
                                "o incolla un annuncio in «Confronta»."
            Return
        End If

        Dim daCompletare As Integer = _registro.Quante(StatoOpportunita.Nuova) +
                                      _registro.Quante(StatoOpportunita.Interessante)

        Dim voci As New List(Of String) From {
            $"{daCompletare} da completare",
            Contate(_registro.Quante(StatoOpportunita.Generata), "generata", "generate"),
            Contate(_registro.Quante(StatoOpportunita.Inviata), "inviata", "inviate"),
            $"{_registro.Quante(StatoOpportunita.Esito)} con esito",
            Contate(_registro.Quante(StatoOpportunita.Scartata), "scartata", "scartate")}

        If quanteSeNeVedono < _registro.Voci.Count Then
            voci.Add($"ne vedi {quanteSeNeVedono} su {_registro.Voci.Count}")
        End If

        lblContatori.Text = String.Join(" · ", voci)

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

    ''' <summary>
    ''' Le voci che i due filtri lasciano passare: prima lo stato, poi le stelle. Sono due
    ''' domande diverse — <i>a che punto sono</i> e <i>quanto valgono</i> — e il cap. 07.3
    ''' le vuole entrambe.
    ''' </summary>
    Private Function Filtrate() As IEnumerable(Of VoceRegistro)

        Dim minime As Integer? = StelleMinime
        If minime Is Nothing Then Return PerStato()

        ' Una candidatura non ancora confrontata non ha stelle, e con un filtro sulle
        ' stelle non passa: non è che valga poco, è che non lo sappiamo ancora. Sparisce
        ' senza rumore, ed è per questo che i contatori dicono quante se ne stanno vedendo.
        Return PerStato().Where(Function(v) v.Stelle.HasValue AndAlso v.Stelle.Value >= minime.Value)

    End Function

    ''' <summary>
    ''' Il minimo di stelle che il filtro lascia passare; <c>Nothing</c> quando non filtra.
    ''' </summary>
    Private ReadOnly Property StelleMinime As Integer?
        Get
            If cboStelle.SelectedIndex <= StelleQualunque Then Return Nothing
            Return cboStelle.SelectedIndex
        End Get
    End Property

    ''' <summary>Le voci che il filtro «Mostra» lascia passare.</summary>
    Private Function PerStato() As IEnumerable(Of VoceRegistro)

        If _registro Is Nothing Then Return Enumerable.Empty(Of VoceRegistro)()

        Select Case cboMostra.SelectedIndex

            Case FiltroDaCompletare
                Return _registro.Voci.Where(Function(v) v.Stato = StatoOpportunita.Nuova OrElse
                                                        v.Stato = StatoOpportunita.Interessante)
            Case FiltroGenerate
                Return _registro.Voci.Where(Function(v) v.Stato = StatoOpportunita.Generata)

            Case FiltroDaSollecitare
                ' Non si rifà il conto: è lo stesso elenco che ha già segnato le righe e
                ' scritto la riga del promemoria (v. RicalcolaISolleciti).
                Return _solleciti

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

                ' Da T9c due candidature «con esito» non sono più la stessa cosa: a parità
                ' di stato decide com'è finita, nell'ordine dell'enum — colloquio,
                ' rifiutata, assunto.
                If esito = 0 Then esito = Nullable.Compare(Of EsitoCandidatura)(a.Esito, b.Esito)

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
    ''' <param name="giorniDiAttesa">
    ''' Da quanti giorni questa candidatura aspetta, se è una di quelle da sollecitare;
    ''' <c>Nothing</c> per tutte le altre (cap. 07.3).
    ''' </param>
    ''' <remarks>
    ''' Da T9c la colonna «Stato» dice l'<b>esito</b> quando c'è — «Rifiutata», non «Con
    ''' esito» — e per le candidature ferme aggiunge da quanto aspettano. Il colore le fa
    ''' notare, ma è il numero a dire perché sono lì: un colore da solo si può leggere
    ''' come «importante» tanto quanto «in ritardo».
    ''' </remarks>
    Private Shared Function RigaDellaVoce(voce As VoceRegistro, giorniDiAttesa As Integer?) As ListViewItem

        Dim aCheStato As String = EsitiCandidatura.EtichettaDi(voce.Stato, voce.Esito)
        If giorniDiAttesa.HasValue Then aCheStato &= $" · {giorniDiAttesa.Value} gg"

        Dim riga As New ListViewItem({
            MatchScritto(voce),
            AziendaDi(voce),
            RuoloDi(voce),
            aCheStato,
            If(String.IsNullOrWhiteSpace(voce.Fonte), "incollato a mano", voce.Fonte),
            QuandoScritto(voce.Aggiornata)}) With {.Tag = voce}

        If voce.Stato = StatoOpportunita.Scartata Then riga.ForeColor = StileApp.TestoSecondario

        ' L'azzurro dei badge qui farebbe da inchiostro, e dietro non c'è un badge ma
        ' l'avorio della coda: valeva 2,93 a 1. È il colore informativo scritto per essere
        ' letto (6,36), non quello fatto per stare sotto il bianco.
        If giorniDiAttesa.HasValue Then riga.ForeColor = StileApp.InformazioneTesto

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
            RaccontaUnErrore($"«{voce.Cartella}» non si è lasciata riaprire: {ex.Message}")
        End Try

    End Sub

    Private Sub btnApriCandidatura_Click(sender As Object, e As EventArgs) Handles btnApriCandidatura.Click
        ApriLaCandidaturaScelta()
    End Sub

    ''' <summary>Il doppio clic sulla riga è la strada breve per lo stesso posto.</summary>
    Private Sub lvwCoda_DoubleClick(sender As Object, e As EventArgs) Handles lvwCoda.DoubleClick
        ApriLaCandidaturaScelta()
    End Sub

    ''' <summary>
    ''' Manda via la candidatura scelta, cartella e tutto (cap. 11.5). Prima lo chiede — e
    ''' lo chiede dicendo <b>che cosa</b> sparisce: è una conferma di livello 5, dove il
    ''' bottone che esegue porta il verbo dell'azione invece di un «Sì»
    ''' (<see cref="FinestraConferma"/>).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>L'ordine conta.</b> Prima si cancella la cartella, che è la verità; poi si
    ''' dice ai pannelli di dimenticarla, perché uno di loro potrebbe averla in mano e
    ''' riscriverla al primo comando; per ultimo si rilegge la coda, che dell'archivio è
    ''' solo il riflesso. Se la cancellazione fallisce non si è detto niente a nessuno, e
    ''' quel che si vede a video è ancora vero.</para>
    ''' <para>Il registro non si tocca qui: <see cref="Aggiorna"/> lo rilegge, si accorge
    ''' che non combacia più con le cartelle e lo rimette su disco da sé (v.
    ''' <c>LeggiIlRegistro</c>).</para>
    ''' </remarks>
    Private Sub EliminaLaCandidaturaScelta()

        Dim voce As VoceRegistro = VoceScelta
        If voce Is Nothing OrElse _contesto Is Nothing Then Return

        Dim dove As String = Path.Combine(_contesto.Cartella.CartellaOpportunita, voce.Cartella)

        If Not FinestraConferma.Chiedi(Me, "Elimina la candidatura",
                                       SpiegazioneDellEliminazione(voce), "Confermo") Then Return

        Try
            _contesto.Opportunita.Elimina(dove)

        Catch ex As Exception When TypeOf ex Is IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            RaccontaUnErrore($"«{voce.Cartella}» non si è lasciata eliminare: {ex.Message}")
            Return
        End Try

        RaiseEvent CandidaturaEliminata(Me, New CandidaturaEliminataEventArgs(dove))

        Aggiorna()

        ' Dopo Aggiorna, non prima: rileggere il registro riscrive questa stessa riga.
        RaccontaLoStato($"Eliminata: {EtichettaDellaVoce(voce)}.", StileApp.TestoSecondario)

    End Sub

    Private Sub btnEliminaCandidatura_Click(sender As Object, e As EventArgs) _
        Handles btnEliminaCandidatura.Click

        EliminaLaCandidaturaScelta()

    End Sub

    ''' <summary>
    ''' Il testo della conferma: che cosa sparisce, detto per esteso, con sopra la
    ''' candidatura di cui si parla. La riga d'intestazione non è un ornamento — «la tua
    ''' cartella», da sola, si potrebbe leggere come tutta la cartella dati.
    ''' </summary>
    ''' <remarks>
    ''' È pubblica perché la legge il banco: premere il bottone aprirebbe una finestra
    ''' modale, e di quella un collaudo non può aspettare la chiusura — la stessa ragione
    ''' per cui P2 espone <c>EliminaIlProfilo</c> accanto al bottone che la apre.
    ''' </remarks>
    Public Shared Function SpiegazioneDellEliminazione(voce As VoceRegistro) As String

        Return EtichettaDellaVoce(voce) & vbLf & vbLf &
               "Stai eliminando la tua cartella: annuncio, giudizi, CV mirato, lettera di " &
               "presentazione ed email verranno cancellati dalla memoria. Non si torna " &
               "indietro. Confermi?"

    End Function

    ''' <summary>Chi era, in una riga: le due colonne di mezzo della coda, unite.</summary>
    Private Shared Function EtichettaDellaVoce(voce As VoceRegistro) As String
        Return $"{AziendaDi(voce)} — {RuoloDi(voce)}"
    End Function

    ''' <summary>
    ''' L'azienda com'è scritta quando l'annuncio non la dichiara. Sta in un posto solo
    ''' perché si legge in due — nella riga della coda e nella conferma che chiede di
    ''' eliminarla — e una candidatura che nell'elenco si chiama in un modo e nella domanda
    ''' in un altro è una candidatura che chi guarda non riconosce.
    ''' </summary>
    Private Shared Function AziendaDi(voce As VoceRegistro) As String
        Return If(String.IsNullOrWhiteSpace(voce.Azienda), "(azienda non dichiarata)", voce.Azienda)
    End Function

    ''' <summary>Il ruolo, con lo stesso ripiego dell'azienda e per la stessa ragione.</summary>
    Private Shared Function RuoloDi(voce As VoceRegistro) As String
        Return If(String.IsNullOrWhiteSpace(voce.Titolo), "(ruolo non dichiarato)", voce.Titolo)
    End Function


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

    ''' <summary>
    ''' I due filtri fanno la stessa cosa: rifanno la coda e rivedono i comandi — quello
    ''' che apre una candidatura si spegne se la riga scelta non è più in vista.
    ''' </summary>
    Private Sub Filtro_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cboMostra.SelectedIndexChanged, cboStelle.SelectedIndexChanged

        RiempiLaCoda()
        AggiornaComandi()

    End Sub

    Private Sub btnApriProfilo_Click(sender As Object, e As EventArgs) Handles btnApriProfilo.Click
        RaiseEvent ProfiloRichiesto(Me, EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Porta l'elenco fuori dall'applicazione (cap. 07.3). Esce <b>quel che si vede</b>,
    ''' nell'ordine in cui lo si vede: chi ha appena filtrato e ordinato la coda si aspetta
    ''' quel foglio lì, non tutto il registro rimescolato. Che sia una vista e non
    ''' l'archivio intero lo dice il file stesso, in testa (v. <see cref="EsportazioneRegistro"/>).
    ''' </summary>
    Private Sub btnEsportaRegistro_Click(sender As Object, e As EventArgs) Handles btnEsportaRegistro.Click

        Dim voci As List(Of VoceRegistro) = Ordinate(Filtrate()).ToList()
        If voci.Count = 0 Then Return

        Using scelta As New SaveFileDialog()

            scelta.Title = "Esporta l'elenco delle candidature"
            scelta.Filter = "Foglio di calcolo (*.csv)|*.csv|Riepilogo leggibile (*.md)|*.md"
            scelta.FilterIndex = 1
            scelta.FileName = $"candidature-{Date.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.csv"
            scelta.AddExtension = True

            If scelta.ShowDialog(Me) <> DialogResult.OK Then Return

            ' Il formato lo decide l'estensione del file, non la voce scelta nel menù: chi
            ' scrive «riepilogo.md» a mano vuole un markdown, qualunque cosa dicesse il
            ' filtro quando ha cominciato a scrivere.
            Dim formato As FormatoEsportazione =
                If(Path.GetExtension(scelta.FileName).Equals(".md", StringComparison.OrdinalIgnoreCase),
                   FormatoEsportazione.Markdown, FormatoEsportazione.Csv)

            Try
                Scrivi(scelta.FileName, EsportazioneRegistro.Componi(voci, formato, CosaSiStaGuardando(voci.Count)))

                RaccontaLoStato($"Elenco esportato: {voci.Count} " &
                                $"{If(voci.Count = 1, "candidatura", "candidature")} in «{scelta.FileName}».",
                                StileApp.TestoSecondario)

            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException OrElse
                                       TypeOf ex Is NotSupportedException

                RaccontaUnErrore($"Non sono riuscita a scrivere «{scelta.FileName}»: {ex.Message}")
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' Scrive il riepilogo. Il CSV vuole il <b>segno d'ordine dei byte</b> in testa:
    ''' senza, Excel legge un file UTF-8 con la tabella caratteri di sistema e «perché»
    ''' diventa «perchÃ©» in ogni riga. Il markdown no — lì il segno sarebbe solo un
    ''' carattere strano prima del titolo.
    ''' </summary>
    Private Shared Sub Scrivi(percorso As String, testo As String)

        Dim conSegno As Boolean = Not percorso.EndsWith(EsportazioneRegistro.Estensione(FormatoEsportazione.Markdown),
                                                        StringComparison.OrdinalIgnoreCase)

        File.WriteAllText(percorso, testo, New System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier:=conSegno))

    End Sub

    ''' <summary>
    ''' Cosa si sta guardando, detto in una riga: la data, i filtri in vigore e quante
    ''' candidature sono finite nel foglio. Va in testa al riepilogo leggibile, perché un
    ''' elenco senza il suo perimetro, riletto fra sei mesi, si scambia per l'elenco intero.
    ''' </summary>
    Private Function CosaSiStaGuardando(quante As Integer) As String

        Dim pezzi As New List(Of String) From {
            $"Esportato il {Date.Now.ToString("d MMMM yyyy", CultureInfo.CurrentCulture)}"}

        If cboMostra.SelectedIndex <> FiltroTutte Then
            pezzi.Add($"stato: {cboMostra.SelectedItem}")
        End If

        If StelleMinime IsNot Nothing Then
            pezzi.Add($"stelle: {cboStelle.SelectedItem}")
        End If

        Dim intero As Integer = If(_registro Is Nothing, quante, _registro.Voci.Count)
        pezzi.Add($"{quante} su {intero}")

        Return String.Join(" · ", pezzi)

    End Function

    Private Sub btnNuovaRicerca_Click(sender As Object, e As EventArgs) Handles btnNuovaRicerca.Click
        RaiseEvent RicercaRichiesta(Me, EventArgs.Empty)
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
            ' «Elimina candidatura» è in fondo di proposito: si accende con la stessa riga
            ' scelta che accende «Apri la candidatura», ed è la ragione per cui non deve
            ' stargli accanto — la stessa per cui in P2 «ELIMINA PROFILO» sta all'estremo
            ' opposto rispetto a «Salva profilo» (cap. 03.6).
            _comandi.ASinistra(btnApriCandidatura, btnEsportaRegistro, btnEliminaCandidatura)
            _comandi.ADestra(btnNuovaRicerca)
        End If

        _comandi.Disponi(Math.Max(AltezzaMinimaAzioni, _ingombroLogo.Height))

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
        StileApp.VestiBottone(btnEsportaRegistro, LivelloBottone.Esplorativo)

        ' Eliminare una candidatura manda via la sua cartella e non si disfa: è il livello
        ' 5 (cap. 03.3), lo stesso dello «Scarta» di P4 — che però non cancella niente.
        StileApp.VestiBottone(btnEliminaCandidatura, LivelloBottone.Distruttivo)

    End Sub

    ''' <summary>
    ''' Le voci dei due filtri, nell'ordine delle costanti qui sopra. Quelle delle stelle
    ''' dicono «almeno», che è la domanda vera di chi guarda una coda lunga: non «quali
    ''' valgono 3» ma «quali valgono <b>da 3 in su</b>».
    ''' </summary>
    Private Sub RiempiIlFiltro()

        cboMostra.Items.AddRange(New Object() {"Tutte", "Da completare", "Generate",
                                               "Da sollecitare", "Scartate"})
        cboMostra.SelectedIndex = FiltroTutte

        cboStelle.Items.Add("tutte")
        For quante As Integer = 1 To StelleDellaScala
            cboStelle.Items.Add($"almeno {quante} ★")
        Next
        cboStelle.SelectedIndex = StelleQualunque

    End Sub

    Private Sub RaccontaLoStato(testo As String, colore As Color)

        lblStatoHome.Text = testo
        lblStatoHome.ForeColor = colore

    End Sub

    ''' <summary>
    ''' Una riga che dice che qualcosa non è riuscito: la parola e il colore insieme
    ''' (v. <see cref="Segnalazioni"/>).
    ''' </summary>
    Private Sub RaccontaUnErrore(testo As String)

        RaccontaLoStato(Segnalazioni.PrefissoErrore & testo, StileApp.Pericolo)

    End Sub

    ''' <summary>
    ''' Una riga che dice che qualcosa manca o è arrivato a metà: stesso colore
    ''' dell'errore, parola diversa — qui non è caduto niente.
    ''' </summary>
    Private Sub RaccontaUnAvviso(testo As String)

        RaccontaLoStato(Segnalazioni.PrefissoAvviso & testo, StileApp.Pericolo)

    End Sub

    ''' <summary>
    ''' Decide, in un posto solo, che cosa si può fare adesso. La domanda è una sola: c'è
    ''' una riga scelta da aprire?
    ''' </summary>
    Private Sub AggiornaComandi()

        Dim scelta As VoceRegistro = VoceScelta

        btnApriCandidatura.Enabled = scelta IsNot Nothing

        ' Rosso e premibile solo su una riga scelta: senza, non si saprebbe nemmeno che
        ' cosa si sta per mandare via. È la stessa regola dell'«ELIMINA PROFILO» di P2 —
        ' un bottone rosso che non ha niente da fare insegna a non fidarsi del colore.
        btnEliminaCandidatura.Enabled = scelta IsNot Nothing

        _suggerimenti.SetToolTip(btnEliminaCandidatura,
            If(scelta Is Nothing, "Scegli prima una candidatura nell'elenco: si elimina quella.", Nothing))

        btnApriProfilo.Enabled = _contesto IsNot Nothing
        btnNuovaRicerca.Enabled = _contesto IsNot Nothing

        ' Si esporta quel che si vede: con la coda vuota — nessuna candidatura, o un filtro
        ' che non lascia passare niente — non c'è niente da esportare, e un bottone acceso
        ' che produce un file vuoto è peggio di uno spento che dice perché.
        btnEsportaRegistro.Enabled = lvwCoda.Items.Count > 0

    End Sub

End Class
