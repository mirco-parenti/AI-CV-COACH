Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms

''' <summary>
''' Le sei destinazioni del menu d'ingresso. Il pannello non sa dove portano: lo dice a
''' chi lo ascolta, che è la finestra principale.
''' </summary>
Public Enum VoceDelMenu

    ''' <summary>P1 — la coda delle candidature, col cruscotto del profilo.</summary>
    Candidature

    ''' <summary>P2 — il profilo, e sotto il 📄 CV base che ne discende.</summary>
    ProfiloECvBase

    ''' <summary>P3 — la ricerca sui portali, col browser incorporato.</summary>
    RicercaOnline

    ''' <summary>P4 — l'annuncio e il confronto col CV, da cui escono le stelle.</summary>
    ''' <remarks>
    ''' Il nome della voce dice ancora come ci si entra — incollando un annuncio a mano,
    ''' senza rete — mentre dal 2026-08-30 l'etichetta del bottone dice cosa ne esce, il
    ''' punteggio di somiglianza. Sono due facce dello stesso pannello e nessuna delle due
    ''' è sbagliata; il nome interno non si è mosso perché a muoverlo si toccavano la
    ''' finestra principale e i collaudi per un cambiamento che è solo di parole a video.
    ''' </remarks>
    IncollaOffline

    ''' <summary>P6 — i documenti già scritti: rileggerli, rifarli, esportarli.</summary>
    Documentazione

    ''' <summary>P8 — le impostazioni, che sono una finestra e non un pannello.</summary>
    Impostazioni

End Enum

''' <summary>Quale voce del menu è stata scelta.</summary>
Public Class VoceDelMenuEventArgs
    Inherits EventArgs

    Public Sub New(voce As VoceDelMenu)
        Me.Voce = voce
    End Sub

    ''' <summary>La destinazione scelta.</summary>
    Public ReadOnly Property Voce As VoceDelMenu

End Class

''' <summary>
''' Pannello P0 — il menu d'ingresso: sei bottoni a pillola su un fondo che è il marchio
''' schiarito. È la schermata su cui l'applicazione si apre.
''' </summary>
''' <remarks>
''' <para><b>Non sostituisce la barra, la precede.</b> La barra in alto resta dov'è e
''' continua a portare agli stessi pannelli: questo è il posto da cui si comincia, non
''' un secondo modo di navigare che le fa concorrenza. Per tornarci c'è la voce
''' «🎮 Menu», prima in barra — senza, dal menu si esce e non si rientra più.</para>
''' <para><b>Il pannello non sa dove portano i bottoni.</b> Alza
''' <see cref="VoceScelta"/> e si ferma lì. Le destinazioni sono di
''' <c>FormPrincipale</c>, che è l'unico posto in cui è già scritto come si passa da un
''' pannello all'altro — e uno dei sei (le Impostazioni) non è nemmeno un pannello, ma
''' una finestra che si apre sopra.</para>
''' <para><b>Lo sfondo è il banner, e non è stato ritoccato.</b> L'immagine incorporata è
''' identica al file dei definitivi; il velo che la schiarisce è un rettangolo bianco
''' semitrasparente dipinto qui sopra, a ogni ridisegno. Così il marchio nel repository
''' resta uno solo, e decidere quanto è chiaro il fondo non vuol dire rigenerare un PNG.
''' Il banner <b>si vede intero</b>: sta tutto dentro l'area, centrato, senza tagli —
''' nome e timbro compresi. Attorno resta il blu del marchio, che è anche il colore della
''' cornice del banner: lo stacco non si vede. La prima versione lo faceva invece
''' <i>riempire</i>, tagliando quel che avanzava, e in cima restava una striscia di
''' lettere mozzate: un marchio tagliato è peggio di un marchio piccolo.</para>
''' <para><b>Perché la disposizione è a mano e non un <c>TableLayoutPanel</c>.</b> I
''' bottoni devono restare centrati e proporzionati su finestre molto diverse — e stare
''' lontani dal pannello del logo, che è flottante sull'angolo in basso a sinistra
''' (cap. 03.5). Un contenitore automatico ridistribuisce lo spazio, non lo riserva.</para>
''' </remarks>
Public Class PannelloMenu
    Implements IPannelloArea

    ''' <summary>Quanto è coprente il velo bianco sopra l'immagine, da 0 a 255.</summary>
    ''' <remarks>
    ''' Scelto guardandolo a video: a 140 il marchio si legge tutto — nome, timbro,
    ''' illustrazione — e sta dietro ai bottoni senza contendere. È la sola manopola
    ''' dell'effetto: alzarlo lo sbianca, abbassarlo lo riporta avanti.
    ''' </remarks>
    Private Const VeloBianco As Integer = 140

    ''' <summary>Larghezza dei bottoni quando c'è tutto lo spazio che vogliono.</summary>
    ''' <remarks>
    ''' Una volta e mezza la misura con cui erano nati (460×58): a quella, sullo sfondo
    ''' del banner, sembravano piccoli. È il genere di cosa che si decide guardando, non
    ''' calcolando.
    ''' </remarks>
    Private Const LarghezzaBottone As Integer = 690

    ''' <summary>Sotto questa larghezza i bottoni non scendono, per stretta che sia la finestra.</summary>
    Private Const LarghezzaMinimaBottone As Integer = 240

    ''' <summary><inheritdoc cref="LarghezzaBottone" path="/summary"/></summary>
    Private Const AltezzaBottone As Integer = 87

    ''' <summary>Sotto questa altezza un bottone non scende: il testo ci deve stare.</summary>
    Private Const AltezzaMinimaBottone As Integer = 34

    ''' <summary>Quanto spazio fra un bottone e il successivo, a misura piena.</summary>
    Private Const DistanzaBottoni As Integer = 16

    ''' <summary>Quanto spazio resta libero ai lati della colonna dei bottoni.</summary>
    Private Const MargineLaterale As Integer = StileApp.MargineRiquadro * 2

    ''' <summary>Quanto è grande il banner incorporato, in pixel.</summary>
    ''' <remarks>
    ''' Scritta qui invece di chiederla all'immagine, e non per pigrizia: leggere
    ''' <c>Image.Size</c> è una chiamata a GDI+ sull'oggetto <b>condiviso</b> che
    ''' <see cref="Marchio.SfondoDelMenu"/> restituisce sempre uguale, e GDI+ non tollera
    ''' che due thread lo tocchino insieme — «Object is currently in use elsewhere». Nel
    ''' programma il pannello è uno solo e non capita; nel banco, dove i collaudi girano in
    ''' parallelo, due finestre lo hanno chiesto nello stesso istante e il secondo è
    ''' esploso. La geometria non ha bisogno dell'immagine: le basta sapere quanto è
    ''' grande, e quella è una costante. Che la risorsa vera misuri davvero così lo
    ''' sorveglia il banco.
    ''' </remarks>
    Public Shared ReadOnly MisuraDelMaster As New Size(1536, 1348)

    ''' <summary>
    ''' Il riquadro che la cornice gialla racchiude, in pixel del master.
    ''' </summary>
    ''' <remarks>
    ''' <para>È lo spazio dentro cui i bottoni si centrano, e sono numeri <b>misurati sul
    ''' PNG</b>, non stimati a occhio: cercando il giallo del filetto (<c>#E2E44E</c>) sul
    ''' master, i due tratti orizzontali stanno a x 13-32 e 1503-1523, i due verticali a
    ''' y 335-355 e 1320-1340. Dentro, quindi, resta da 33 a 1502 e da 356 a 1319.</para>
    ''' <para>Serve a lasciare scoperti <b>nome e sottotitolo</b>, che stanno sopra la
    ''' cornice: centrando la colonna sull'area, o anche solo sotto la fascia del testo, il
    ''' primo bottone finiva addosso al sottotitolo. Se un giorno il marchio cambia
    ''' impaginazione, questi quattro numeri si rimisurano allo stesso modo.</para>
    ''' </remarks>
    Private Shared ReadOnly RiquadroDentroLaCornice As New Rectangle(33, 356, 1470, 964)

    ''' <summary>Quanto spazio si prende il logo flottante (cap. 03.5).</summary>
    Private _ingombroLogo As Size

    ''' <summary>Alzato quando si sceglie una delle sei voci.</summary>
    Public Event VoceScelta As EventHandler(Of VoceDelMenuEventArgs)

    Public Sub New()

        InitializeComponent()

        ' Lo sfondo lo dipinge OnPaintBackground: senza doppio buffer, ridimensionare la
        ' finestra farebbe lampeggiare l'immagine sotto i bottoni.
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw, True)

        Me.BackColor = StileApp.FondoMarchio

    End Sub

    ''' <inheritdoc/>
    Public Sub ImpostaIngombroLogo(ingombro As Size) Implements IPannelloArea.ImpostaIngombroLogo

        _ingombroLogo = ingombro
        DisponiIBottoni()

    End Sub

    ''' <summary>I sei bottoni nell'ordine in cui si leggono.</summary>
    ''' <remarks>
    ''' L'ordine è quello dell'uso: prima si guarda dove si è (le candidature), poi si
    ''' cura la propria materia prima (il profilo), poi si cerca — online o incollando —
    ''' e infine si lavora sui documenti. Le impostazioni per ultime, come sempre.
    ''' </remarks>
    Private Function IBottoni() As BottoneMenu()

        Return New BottoneMenu() {btnVoceCandidature, btnVoceProfiloCv, btnVoceRicercaOnline,
                                  btnVoceIncollaOffline, btnVoceDocumentazione, btnVoceImpostazioni}

    End Function

    ''' <summary>
    ''' Mette i sei bottoni in colonna al centro, restringendoli se la finestra non basta.
    ''' </summary>
    ''' <remarks>
    ''' <para>L'altezza si calcola prima della larghezza, perché è lei a mancare per prima:
    ''' sei bottoni alti 58 con 16 di stacco vogliono 428 px, e l'area centrale su una
    ''' finestra piccola ne ha meno. Quando lo spazio non basta, bottoni e stacchi calano
    ''' insieme fino ai loro minimi — sotto i quali la colonna esce dal pannello ed è
    ''' meglio che esca in basso, dove non c'è niente, che sopra il logo.</para>
    ''' <para>La colonna non è centrata sull'area ma sull'<b>illustrazione</b>, cioè sul
    ''' banner meno la sua fascia del testo (v. <see cref="ZonaDelDisegno"/>): centrandola
    ''' sull'area, il primo bottone finiva davanti al nome del marchio. E resta comunque
    ''' <b>al netto del logo</b> flottante, che sull'angolo in basso a sinistra si prende
    ''' il suo spazio.</para>
    ''' </remarks>
    Private Sub DisponiIBottoni()

        Dim bottoni As BottoneMenu() = IBottoni()
        If bottoni.Length = 0 OrElse Me.ClientSize.Width <= 0 Then Return

        ' Lo spazio verticale utile: tolto quel che il logo sfonda nell'area centrale.
        Dim altezzaUtile As Integer = Me.ClientSize.Height - _ingombroLogo.Height - StileApp.MargineRiquadro * 2
        If altezzaUtile < AltezzaMinimaBottone Then altezzaUtile = AltezzaMinimaBottone

        Dim altezza As Integer = AltezzaBottone
        Dim distanza As Integer = DistanzaBottoni

        Dim serve As Integer = altezza * bottoni.Length + distanza * (bottoni.Length - 1)
        If serve > altezzaUtile Then
            ' Si stringe in proporzione, ma nessuno dei due scende sotto il suo minimo.
            Dim fattore As Double = altezzaUtile / CDbl(serve)
            altezza = Math.Max(AltezzaMinimaBottone, CInt(Math.Floor(altezza * fattore)))
            distanza = Math.Max(StileApp.InterlineaMinima, CInt(Math.Floor(distanza * fattore)))
        End If

        Dim larghezza As Integer = Math.Min(LarghezzaBottone, Me.ClientSize.Width - MargineLaterale * 2)
        larghezza = Math.Max(LarghezzaMinimaBottone, larghezza)

        Dim totale As Integer = altezza * bottoni.Length + distanza * (bottoni.Length - 1)
        Dim zona As Rectangle = ZonaDelDisegno()

        Dim sinistra As Integer = zona.Left + (zona.Width - larghezza) \ 2
        Dim cima As Integer = zona.Top + (zona.Height - totale) \ 2

        ' Le due guardie: non sopra il bordo, e non addosso al logo flottante — ma il logo
        ' sta nell'angolo in <b>basso a sinistra</b>, e a schermo largo la colonna gli passa
        ' lontano. Tenerne conto sempre, com'era la prima versione, spingeva i bottoni in
        ' cima e il primo finiva sopra il sottotitolo del marchio: la guardia sbagliata
        ' rovinava proprio la cosa che il centraggio era andato a salvare.
        Dim ultimaCimaUtile As Integer = Me.ClientSize.Height - StileApp.MargineRiquadro - totale
        If sinistra < _ingombroLogo.Width Then
            ultimaCimaUtile = Math.Min(ultimaCimaUtile,
                                       Me.ClientSize.Height - _ingombroLogo.Height - totale)
        End If

        cima = Math.Max(StileApp.MargineRiquadro, Math.Min(cima, ultimaCimaUtile))

        For Each bottone As BottoneMenu In bottoni
            bottone.SetBounds(sinistra, cima, larghezza, altezza)
            ' Il corpo del testo segue l'altezza del bottone: rimpicciolendo la finestra,
            ' un corpo fisso finirebbe per non starci più dentro.
            Dim corpo As Single = Math.Max(9.0F, altezza * 0.26F)
            If Math.Abs(bottone.Font.Size - corpo) > 0.1F Then
                Dim vecchio As Font = bottone.Font
                bottone.Font = New Font(StileApp.NomeFont, corpo, FontStyle.Bold)
                vecchio.Dispose()
            End If
            cima += altezza + distanza
        Next

    End Sub

    ''' <summary>Il bottone che porta a una voce, o <c>Nothing</c> se la voce non c'è.</summary>
    Private Function BottoneDella(voce As VoceDelMenu) As BottoneMenu

        Select Case voce
            Case VoceDelMenu.Candidature : Return btnVoceCandidature
            Case VoceDelMenu.ProfiloECvBase : Return btnVoceProfiloCv
            Case VoceDelMenu.RicercaOnline : Return btnVoceRicercaOnline
            Case VoceDelMenu.IncollaOffline : Return btnVoceIncollaOffline
            Case VoceDelMenu.Documentazione : Return btnVoceDocumentazione
            Case VoceDelMenu.Impostazioni : Return btnVoceImpostazioni
            Case Else : Return Nothing
        End Select

    End Function

    ''' <summary>
    ''' Accende o spegne una voce. Chi non ha uno stato da dare non ne cambia nessuno.
    ''' </summary>
    ''' <remarks>
    ''' Lo stato non nasce qui: lo detta la finestra, che lo legge dai bottoni della
    ''' barra. Mentre l'AI lavora la barra si chiude, e il menu deve chiudersi con lei —
    ''' altrimenti sarebbe la scorciatoia con cui uscire da una porta appena sbarrata.
    ''' </remarks>
    Public Sub ImpostaStato(voce As VoceDelMenu, acceso As Boolean?)

        If Not acceso.HasValue Then Return

        Dim bottone As BottoneMenu = BottoneDella(voce)
        If bottone IsNot Nothing Then bottone.Enabled = acceso.Value

    End Sub

    ''' <summary>
    ''' Dove sta, a video, lo spazio dentro la cornice gialla del banner.
    ''' </summary>
    ''' <remarks>
    ''' Si prende <see cref="RiquadroDentroLaCornice"/> — misurato sul master — e lo si
    ''' scala come è stata scalata l'immagine. Se lo sfondo non c'è, la zona è tutta
    ''' l'area e la colonna torna semplicemente al centro: senza immagine non c'è nessun
    ''' titolo da lasciare scoperto.
    ''' </remarks>
    Private Function ZonaDelDisegno() As Rectangle

        ' Si chiede solo *se* lo sfondo c'è: quanto è grande lo dice MisuraDelMaster, per
        ' la ragione scritta là sopra.
        If Marchio.SfondoDelMenu Is Nothing Then Return New Rectangle(Point.Empty, Me.ClientSize)

        Return ZonaDentroLaCornice(MisuraDelMaster, Me.ClientSize)

    End Function

    ''' <summary>
    ''' Lo spazio dentro la cornice gialla, in coordinate dell'area: dove i bottoni si
    ''' centrano.
    ''' </summary>
    ''' <remarks>
    ''' Condivisa come <see cref="RiquadroDelloSfondo"/>, e per la stessa ragione: è
    ''' geometria, e il banco la interroga senza costruire il pannello. Qui la ragione è
    ''' più forte che altrove — questo conto è stato sbagliato <b>due volte</b> in un
    ''' pomeriggio (prima centrando sull'area, poi lasciando che la guardia del logo
    ''' spingesse in alto la colonna), e tutte e due le volte se n'è accorto solo l'occhio,
    ''' guardando una fotografia. Adesso se ne accorge anche il banco.
    ''' </remarks>
    Public Shared Function ZonaDentroLaCornice(immagine As Size, area As Size) As Rectangle

        Dim riquadro As Rectangle = RiquadroDelloSfondo(immagine, area)
        If riquadro.Width <= 0 OrElse riquadro.Height <= 0 Then Return riquadro

        Dim orizzontale As Double = riquadro.Width / CDbl(MisuraDelMaster.Width)
        Dim verticale As Double = riquadro.Height / CDbl(MisuraDelMaster.Height)

        Return New Rectangle(
            riquadro.X + CInt(RiquadroDentroLaCornice.X * orizzontale),
            riquadro.Y + CInt(RiquadroDentroLaCornice.Y * verticale),
            Math.Max(1, CInt(RiquadroDentroLaCornice.Width * orizzontale)),
            Math.Max(1, CInt(RiquadroDentroLaCornice.Height * verticale)))

    End Function

    Protected Overrides Sub OnResize(e As EventArgs)

        MyBase.OnResize(e)
        DisponiIBottoni()

    End Sub

    ''' <summary>
    ''' Dipinge il fondo: l'immagine che riempie l'area, e sopra il velo che la schiarisce.
    ''' </summary>
    ''' <remarks>
    ''' Se la risorsa non c'è resta il blu del marchio, che è già il <c>BackColor</c> del
    ''' pannello: un'immagine che manca non è una ragione per non mostrare il menu — la
    ''' stessa promessa che <see cref="FinestraAvvio"/> fa per la schermata di avvio.
    ''' </remarks>
    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)

        MyBase.OnPaintBackground(e)

        Dim sfondo As Image = Marchio.SfondoDelMenu
        If sfondo Is Nothing OrElse Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then Return

        e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic
        e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality
        e.Graphics.DrawImage(sfondo, RiquadroDelloSfondo(sfondo.Size, Me.ClientSize))

        Using velo As New SolidBrush(Color.FromArgb(VeloBianco, Color.White))
            e.Graphics.FillRectangle(velo, Me.ClientRectangle)
        End Using

    End Sub

    ''' <summary>
    ''' Dove finisce l'immagine per <b>starci tutta</b> dentro l'area senza deformarsi: si
    ''' ingrandisce o si riduce fino a toccare il lato che le sta più stretto, e resta
    ''' centrata.
    ''' </summary>
    ''' <remarks>
    ''' È la stessa regola della schermata di avvio, che pure deve starci dentro: si
    ''' prende il fattore <b>più piccolo</b> dei due. Il fattore più grande — riempire
    ''' tagliando — era la scelta di partenza, ed è durata il tempo di guardarla: un
    ''' banner tagliato a metà del nome non è uno sfondo, è un marchio rotto. È pubblica e
    ''' condivisa perché il banco la interroga senza costruire il pannello, e senza uno
    ''' schermo.
    ''' </remarks>
    ''' <param name="immagine">Quanto è grande l'immagine.</param>
    ''' <param name="area">Quanto è grande lo spazio in cui deve stare.</param>
    Public Shared Function RiquadroDelloSfondo(immagine As Size, area As Size) As Rectangle

        If immagine.Width <= 0 OrElse immagine.Height <= 0 Then Return New Rectangle(Point.Empty, area)
        If area.Width <= 0 OrElse area.Height <= 0 Then Return Rectangle.Empty

        Dim fattore As Double = Math.Min(area.Width / CDbl(immagine.Width),
                                         area.Height / CDbl(immagine.Height))

        ' Si arrotonda per difetto: arrotondando per eccesso l'immagine sborderebbe di un
        ' pixel, che è proprio la cosa che qui non deve succedere.
        Dim larghezza As Integer = Math.Max(1, CInt(Math.Floor(immagine.Width * fattore)))
        Dim altezza As Integer = Math.Max(1, CInt(Math.Floor(immagine.Height * fattore)))

        Return New Rectangle((area.Width - larghezza) \ 2,
                             (area.Height - altezza) \ 2,
                             larghezza, altezza)

    End Function

    Private Sub btnCandidature_Click(sender As Object, e As EventArgs) Handles btnVoceCandidature.Click
        RaiseEvent VoceScelta(Me, New VoceDelMenuEventArgs(VoceDelMenu.Candidature))
    End Sub

    Private Sub btnProfiloCv_Click(sender As Object, e As EventArgs) Handles btnVoceProfiloCv.Click
        RaiseEvent VoceScelta(Me, New VoceDelMenuEventArgs(VoceDelMenu.ProfiloECvBase))
    End Sub

    Private Sub btnRicercaOnline_Click(sender As Object, e As EventArgs) Handles btnVoceRicercaOnline.Click
        RaiseEvent VoceScelta(Me, New VoceDelMenuEventArgs(VoceDelMenu.RicercaOnline))
    End Sub

    Private Sub btnIncollaOffline_Click(sender As Object, e As EventArgs) Handles btnVoceIncollaOffline.Click
        RaiseEvent VoceScelta(Me, New VoceDelMenuEventArgs(VoceDelMenu.IncollaOffline))
    End Sub

    Private Sub btnDocumentazione_Click(sender As Object, e As EventArgs) Handles btnVoceDocumentazione.Click
        RaiseEvent VoceScelta(Me, New VoceDelMenuEventArgs(VoceDelMenu.Documentazione))
    End Sub

    Private Sub btnImpostazioni_Click(sender As Object, e As EventArgs) Handles btnVoceImpostazioni.Click
        RaiseEvent VoceScelta(Me, New VoceDelMenuEventArgs(VoceDelMenu.Impostazioni))
    End Sub

End Class
