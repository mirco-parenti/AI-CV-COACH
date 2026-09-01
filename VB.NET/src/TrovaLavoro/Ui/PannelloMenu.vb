Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.Drawing.Text
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
''' Pannello P0 — il menu d'ingresso: sei bottoni a pillola su un fondo avorio, col nome
''' del prodotto e il suo sottotitolo in cima. È la schermata su cui l'applicazione si
''' apre.
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
''' <para><b>Il marchio qui dietro non c'è.</b> Fra il 2026-08-30 e il 2026-09-01 questo
''' fondo ha portato un <b>mega stemma</b> Aviolab dietro la colonna dei bottoni: è stato
''' tolto, e lo stemma è tornato a vivere in un posto solo, il pannello del logo
''' flottante in basso a sinistra (cap. 03.5). Un marchio che compare in tre punti della
''' stessa schermata non si legge tre volte, si legge come rumore; e uno stemma grande
''' quanto l'area, per quanto sbiadito, è uno sfondo con cui i bottoni devono comunque
''' contendere. Restano il nome e l'avorio, che sono già il marchio.</para>
''' <para><b>Lo sfondo si dipinge, non si carica.</b> Dal 2026-08-30 (sera) il fondo non
''' è più il banner del marchio ma due cose disegnate a video: l'avorio di
''' <see cref="StileApp.FondoMenu"/>, e in cima <see cref="Titolo"/> e
''' <see cref="Sottotitolo"/> — le stesse due righe del banner, centrate sull'asse e senza
''' il timbro che sul banner sta a destra del nome. Il vantaggio non è estetico: un fondo
''' dipinto <b>segue la finestra</b>, mentre un'immagine sola può solo starci dentro o
''' essere tagliata, e su una finestra panoramica il banner quasi quadrato lasciava blu ai
''' lati.</para>
''' <para><b>Il velo bianco è rimasto, e prende le due righe.</b> Sul banner schiariva
''' l'immagine intera; qui non si stende sul fondo — <c>#FFFAF0</c> velato diventa
''' <c>#FFFDF8</c>, che è lo stesso colore, e l'avorio va lasciato esatto — ma su
''' <b>quel che ci sta sopra</b>: nome e sottotitolo. Sono sfondo tutti e due, e sfondo
''' vuol dire stare dietro ai bottoni senza contendere.</para>
''' <para><b>Come si stende un velo sul nome e non sull'avorio.</b> Le due righe si
''' disegnano <b>a piena forza su una tela a parte</b>, e la tela si appoggia sull'avorio
''' con l'opacità abbassata in una volta sola. La via corta — scrivere già con un colore
''' trasparente — qui non funziona: il nome è un contorno nero <i>e</i> un riempimento
''' bianco sullo stesso percorso, e col bianco trasparente il nero di sotto riaffiorerebbe
''' dentro le lettere, che verrebbero grigie ai bordi invece che bianche. L'altra via
''' corta — un rettangolo bianco steso sopra tutto, com'era sul banner — schiarirebbe
''' anche l'avorio.</para>
''' <para><b>Perché la disposizione è a mano e non un <c>TableLayoutPanel</c>.</b> I
''' bottoni devono restare centrati e proporzionati su finestre molto diverse — e stare
''' lontani dal pannello del logo, che è flottante sull'angolo in basso a sinistra
''' (cap. 03.5). Un contenitore automatico ridistribuisce lo spazio, non lo riserva.</para>
''' </remarks>
Public Class PannelloMenu
    Implements IPannelloArea

    ''' <summary>Il nome del prodotto, com'è scritto sul banner del marchio.</summary>
    Public Const Titolo As String = "TrovaLavoro"

    ''' <summary>La riga sotto il nome, com'è scritta sul banner del marchio.</summary>
    Public Const Sottotitolo As String =
        "Crea il tuo miglior CV e rispondi subito all'annuncio di lavoro perfetto per te!"

    ''' <summary>La famiglia con cui è scritto il nome, se la macchina ce l'ha.</summary>
    ''' <remarks>
    ''' Le due famiglie del banner. Portano il peso <b>nel nome</b> — «Black», «Semibold»
    ''' — e non sono stili di «Segoe UI»: vanno chieste così, e in stile normale. Se su
    ''' una macchina non ci fossero si ripiega su <see cref="StileApp.NomeFont"/> in
    ''' grassetto, che è la stessa famiglia di tutta l'interfaccia.
    ''' </remarks>
    Private Const NomeFontTitolo As String = "Segoe UI Black"

    ''' <summary><inheritdoc cref="NomeFontTitolo" path="/summary"/></summary>
    Private Const NomeFontSottotitolo As String = "Segoe UI Semibold"

    ''' <summary>Quanto è coprente il velo bianco sopra il nome, da 0 a 255.</summary>
    ''' <remarks>
    ''' Scelto guardandolo a video quando il fondo era il banner, e rimasto lo stesso da
    ''' allora: a 140 le due righe si leggono e stanno dietro ai bottoni senza contendere.
    ''' È la sola manopola dell'effetto — alzarlo le sbianca, abbassarlo le riporta avanti.
    ''' </remarks>
    Private Const VeloBianco As Integer = 140

    ''' <summary>Quanta parte dell'altezza si prende la fascia del nome, in cima.</summary>
    ''' <remarks>
    ''' Sul banner nome e sottotitolo occupavano i primi 356 px su 1348, poco oltre un
    ''' quarto: qui la fascia è un filo più alta perché deve contenere anche il
    ''' <see cref="FrazioneRespiro">respiro</see> che stacca il sottotitolo dal primo
    ''' bottone. Sotto di lei comincia la colonna.
    ''' </remarks>
    Private Const FrazioneFasciaDelTesto As Double = 0.32

    ''' <summary>Quanta parte della fascia resta vuota in fondo, per staccare il primo bottone.</summary>
    Private Const FrazioneRespiro As Double = 0.18

    ''' <summary>Quanta parte della larghezza si prende il nome.</summary>
    ''' <remarks>
    ''' Il corpo del carattere non è un numero fisso: si ricava da quanto deve venire
    ''' <b>largo</b> il testo, così il nome cresce con la finestra invece di restare un
    ''' francobollo su uno schermo grande. Le due frazioni sono quelle del banner,
    ''' misurate sul master: il nome ne copriva il 63% e il sottotitolo il 90%, ma il
    ''' banner è quasi quadrato e qui l'area è panoramica — riportarle tali e quali
    ''' avrebbe dato un nome alto quanto mezza fascia.
    ''' </remarks>
    Private Const FrazioneLarghezzaTitolo As Double = 0.4

    ''' <summary><inheritdoc cref="FrazioneLarghezzaTitolo" path="/summary"/></summary>
    Private Const FrazioneLarghezzaSottotitolo As Double = 0.74

    ''' <summary>Quanto è spesso il contorno nero, in frazione del corpo.</summary>
    ''' <remarks>
    ''' Misurato sul banner: il nome bianco è alto 109 px e il suo contorno ne aggiunge
    ''' una ventina per parte. A quello spessore il contorno salda fra loro le lettere e
    ''' fa della scritta un blocco unico, che è il carattere del marchio — e su un fondo
    ''' chiaro è anche la sola ragione per cui un testo bianco si legge.
    ''' </remarks>
    Private Const FrazioneContorno As Single = 0.2

    ''' <summary>Larghezza dei bottoni quando c'è tutto lo spazio che vogliono.</summary>
    ''' <remarks>
    ''' <para>Tre misure in due giorni, e ognuna aveva ragione sul suo sfondo: nati 460×58,
    ''' cresciuti a 690×87 perché sul banner sembravano piccoli, e tornati a <b>420×53</b>
    ''' il 2026-08-30 (sera), quando il fondo è diventato il mega stemma. A 690 la colonna
    ''' lo copriva per intero, e di uno scudo alto quanto tutta la zona si vedevano tre
    ''' strisce negli stacchi fra una pillola e l'altra — che a occhio non sembrava un
    ''' marchio, sembrava un difetto.</para>
    ''' <para>Lo stemma dietro non c'è più dal 2026-09-01, e con lui se n'è andata la
    ''' ragione per cui questo numero era sceso a 420. Il numero è rimasto: allargare la
    ''' colonna adesso sarebbe una terza misura decisa <b>calcolando</b>, e queste si
    ''' decidono guardando. Chi la riguarderà a video parta da qui sapendo che 420 non
    ''' difende più niente.</para>
    ''' <para>Il <b>rapporto</b> fra i due lati non si è mosso (7,9), e non è un vezzo: il
    ''' corpo del testo segue l'altezza del bottone, quindi stringere la sola larghezza
    ''' avrebbe lasciato lettere da bottone grande dentro un bottone corto.</para>
    ''' <para>Sotto questa misura non si scende a cuor leggero: a misura piena il nome più
    ''' lungo — «Confronta ANNUNCIO - CV / Match 1-5 ⭐» — vuole 379 px, e qui ne restano
    ''' quaranta di margine. A sorvegliarlo c'è il banco, che a 260 diventa rosso.</para>
    ''' </remarks>
    Private Const LarghezzaBottone As Integer = 420

    ''' <summary>Sotto questa larghezza i bottoni non scendono, per stretta che sia la finestra.</summary>
    Private Const LarghezzaMinimaBottone As Integer = 240

    ''' <summary><inheritdoc cref="LarghezzaBottone" path="/summary"/></summary>
    Private Const AltezzaBottone As Integer = 53

    ''' <summary>Sotto questa altezza un bottone non scende: il testo ci deve stare.</summary>
    Private Const AltezzaMinimaBottone As Integer = 34

    ''' <summary>Quanto spazio fra un bottone e il successivo, a misura piena.</summary>
    Private Const DistanzaBottoni As Integer = 16

    ''' <summary>
    ''' Di quanto la colonna sta più in alto del centro, in altezze di bottone.
    ''' </summary>
    ''' <remarks>
    ''' <para>Un bottone e tre quarti, deciso guardando. Il centro geometrico della zona
    ''' non è il centro che l'occhio si aspetta: sotto la colonna non c'è niente fino al
    ''' bordo, sopra c'è il nome, e una colonna esattamente centrata sembra scivolata in
    ''' basso. È lo stesso genere di correzione con cui si compone una pagina — il blocco
    ''' si alza un poco, perché il margine di sotto pesa meno di quello di sopra.</para>
    ''' <para>Questa ragione regge da sola, ma non è quella che ha fissato il
    ''' <b>quanto</b>: il metro fu lo <b>stemma</b> di sfondo, e in particolare le sue due
    ''' stelle laterali, che il primo tasto non doveva coprire. Ci si arrivò per gradi —
    ''' mezzo bottone, uno, poi 496, 483, 469, 456 — fermandosi a uno e tre quarti, col
    ''' primo tasto a 469 e quaranta pixel di aria sopra le stelle. Dal 2026-09-01 lo
    ''' stemma dietro non c'è più, e quel metro è sparito con lui: il numero resta come
    ''' correzione dell'occhio, non più come misura di qualcosa. Se lo si vuole rifare, si
    ''' rifà guardando la colonna sull'avorio nudo, che è un'altra cosa da guardare.</para>
    ''' <para><i>Due stime a occhio furono smentite dal conto dei pixel, in mezz'ora: «a un
    ''' bottone ne copre una dozzina» (ne copriva zero) e «a uno e mezzo le stelle restano
    ''' libere sotto» (restavano dietro il secondo tasto). Un numero che nasce guardando va
    ''' poi misurato, o resta un'opinione scritta in un commento.</i></para>
    ''' </remarks>
    Private Const RialzoColonna As Double = 1.75

    ''' <summary>Quanto spazio resta libero ai lati della colonna dei bottoni.</summary>
    Private Const MargineLaterale As Integer = StileApp.MargineRiquadro * 2

    ''' <summary>Una riga del nome già dimensionata sulla finestra di adesso.</summary>
    Private Structure RigaDelNome

        ''' <summary>Il corpo del carattere, in pixel.</summary>
        Public Corpo As Single

        ''' <summary>Quanto è alta la riga a quel corpo, in pixel, contorno escluso.</summary>
        Public Altezza As Single

    End Structure

    ''' <summary>Quanto spazio si prende il logo flottante (cap. 03.5).</summary>
    Private _ingombroLogo As Size

    ''' <summary>Il nome già disegnato, a piena forza, senza il velo.</summary>
    ''' <remarks>
    ''' Tenuto da parte e non rifatto a ogni ridisegno: le due righe vogliono due
    ''' misurazioni di testo e due percorsi contornati, che a ogni pennellata del fondo
    ''' sarebbero uno spreco. Si rifà solo quando cambia la misura della finestra.
    ''' </remarks>
    Private _sfondo As Bitmap

    ''' <summary>Per quale misura <see cref="_sfondo"/> è stato disegnato.</summary>
    Private _misuraSfondo As Size

    ''' <summary>Alzato quando si sceglie una delle sei voci.</summary>
    Public Event VoceScelta As EventHandler(Of VoceDelMenuEventArgs)

    Public Sub New()

        InitializeComponent()

        ' Lo sfondo lo dipinge OnPaintBackground: senza doppio buffer, ridimensionare la
        ' finestra farebbe lampeggiare l'immagine sotto i bottoni.
        SetStyle(ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw, True)

        Me.BackColor = StileApp.FondoMenu

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
    ''' <para>La colonna non è centrata sull'area ma sulla
    ''' <see cref="ZonaSottoIlNome">zona sotto il nome</see>: centrandola sull'area, il
    ''' primo bottone finiva davanti al nome del marchio. E resta comunque <b>al netto del
    ''' logo</b> flottante, che sull'angolo in basso a sinistra si prende il suo
    ''' spazio. Dentro la zona sta poi <see cref="RialzoColonna">un bottone e tre quarti
    ''' più in alto</see> del centro, che è una correzione dell'occhio e non del
    ''' conto.</para>
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
        Dim zona As Rectangle = ZonaSottoIlNome(Me.ClientSize)

        Dim sinistra As Integer = zona.Left + (zona.Width - larghezza) \ 2

        ' Centrata nella zona, e poi alzata: di quanto lo dice RialzoColonna.
        Dim cima As Integer = zona.Top + (zona.Height - totale) \ 2 -
                              CInt(Math.Round(altezza * RialzoColonna))

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
    ''' Quanto è alta la fascia in cima in cui stanno nome e sottotitolo.
    ''' </summary>
    ''' <remarks>
    ''' È una frazione dell'altezza e non la misura del testo, e la differenza conta: la
    ''' geometria dei bottoni si decide a ogni ridimensionamento, dove un
    ''' <c>Graphics</c> non c'è — e su un controllo mai mostrato non si può nemmeno
    ''' chiedere. Il testo si adatta poi alla fascia, non viceversa.
    ''' </remarks>
    Public Shared Function FasciaDelTesto(area As Size) As Integer

        If area.Height <= 0 Then Return 0

        Return CInt(Math.Floor(area.Height * FrazioneFasciaDelTesto))

    End Function

    ''' <summary>Dove finisce il sottotitolo, dentro la fascia.</summary>
    ''' <remarks>
    ''' Sotto di lì la fascia è vuota: è il respiro che stacca il nome dal primo bottone.
    ''' È il limite entro cui il blocco delle due righe si centra e oltre il quale non
    ''' sconfina, e sta qui invece che dentro il disegno del testo perché è una misura
    ''' della <b>geometria</b> del pannello: si può chiedere senza uno schermo, come la
    ''' fascia da cui discende.
    ''' </remarks>
    Public Shared Function FineDelNome(area As Size) As Integer

        Return CInt(Math.Floor(FasciaDelTesto(area) * (1.0 - FrazioneRespiro)))

    End Function

    ''' <summary>
    ''' Lo spazio sotto il nome, dove si centra la colonna dei bottoni.
    ''' </summary>
    ''' <remarks>
    ''' Pubblica e <c>Shared</c> perché il banco la interroga senza costruire il pannello
    ''' e senza uno schermo. È l'erede della «zona dentro la cornice» del banner, e nasce
    ''' dalla stessa esigenza: lasciare scoperto quel che sta scritto in cima. Quel conto
    ''' fu sbagliato <b>due volte</b> in un pomeriggio, e tutte e due le volte se ne
    ''' accorse solo l'occhio, guardando una fotografia.
    ''' </remarks>
    Public Shared Function ZonaSottoIlNome(area As Size) As Rectangle

        If area.Width <= 0 OrElse area.Height <= 0 Then Return Rectangle.Empty

        Dim cima As Integer = FasciaDelTesto(area)

        Return New Rectangle(0, cima, area.Width, Math.Max(1, area.Height - cima))

    End Function

    Protected Overrides Sub OnResize(e As EventArgs)

        MyBase.OnResize(e)
        DisponiIBottoni()

    End Sub

    ''' <summary>
    ''' Dipinge il fondo: l'avorio, e sopra il nome passato sotto il velo.
    ''' </summary>
    ''' <remarks>
    ''' L'avorio lo mette già <see cref="Control.BackColor"/>, che la base dipinge per
    ''' prima, e resta del colore esatto che è: il velo tocca solo la tela che ci si
    ''' appoggia sopra. Se quella tela non si potesse disegnare resterebbe l'avorio nudo,
    ''' che è pur sempre un menu — la stessa promessa che <see cref="FinestraAvvio"/> fa
    ''' per la schermata di avvio.
    ''' </remarks>
    Protected Overrides Sub OnPaintBackground(e As PaintEventArgs)

        MyBase.OnPaintBackground(e)

        Dim strato As Bitmap = StratoDelloSfondo()
        If strato Is Nothing Then Return

        Using attributi As New ImageAttributes()

            Dim matrice As New ColorMatrix()
            matrice.Matrix33 = (255.0F - VeloBianco) / 255.0F
            attributi.SetColorMatrix(matrice)

            e.Graphics.DrawImage(strato, New Rectangle(Point.Empty, strato.Size),
                                 0, 0, strato.Width, strato.Height, GraphicsUnit.Pixel, attributi)

        End Using

    End Sub

    ''' <summary>
    ''' La tela col nome a piena forza, rifatta solo se la finestra è cambiata.
    ''' </summary>
    Private Function StratoDelloSfondo() As Bitmap

        If Me.ClientSize.Width <= 0 OrElse Me.ClientSize.Height <= 0 Then Return Nothing
        If _sfondo IsNot Nothing AndAlso _misuraSfondo = Me.ClientSize Then Return _sfondo

        _sfondo?.Dispose()
        _sfondo = New Bitmap(Me.ClientSize.Width, Me.ClientSize.Height, PixelFormat.Format32bppArgb)
        _misuraSfondo = Me.ClientSize

        Using g As Graphics = Graphics.FromImage(_sfondo)
            DisegnaIlNome(g)
        End Using

        Return _sfondo

    End Function

    ''' <summary>Il nome e il sottotitolo, centrati sull'asse nella fascia in cima.</summary>
    ''' <remarks>
    ''' <para>Il blocco delle due righe si centra nella fascia <b>meno il respiro</b>, che
    ''' è lo stacco dal primo bottone: senza, il sottotitolo finiva appoggiato alla prima
    ''' pillola.</para>
    ''' <para>Se il blocco non ci sta — finestra bassa, o una traduzione più lunga — le due
    ''' righe calano <b>insieme</b> fino a starci, invece di sconfinare sui bottoni.</para>
    ''' </remarks>
    Private Sub DisegnaIlNome(g As Graphics)

        Dim area As Size = Me.ClientSize
        Dim fascia As Integer = FasciaDelTesto(area)
        If fascia <= 0 OrElse area.Width <= 0 Then Return

        Dim stileTitolo As FontStyle = FontStyle.Regular
        Dim stileSotto As FontStyle = FontStyle.Regular

        Using famigliaTitolo As FontFamily = FamigliaPerIlNome(NomeFontTitolo, stileTitolo),
              famigliaSotto As FontFamily = FamigliaPerIlNome(NomeFontSottotitolo, stileSotto)

            Dim rigaTitolo As RigaDelNome = RigaPerLarghezza(
                g, Titolo, famigliaTitolo, stileTitolo, CSng(area.Width * FrazioneLarghezzaTitolo))
            Dim rigaSotto As RigaDelNome = RigaPerLarghezza(
                g, Sottotitolo, famigliaSotto, stileSotto, CSng(area.Width * FrazioneLarghezzaSottotitolo))

            Dim contornoTitolo As Single = rigaTitolo.Corpo * FrazioneContorno
            Dim contornoSotto As Single = rigaSotto.Corpo * FrazioneContorno
            Dim stacco As Single = rigaSotto.Altezza * 0.35F

            Dim blocco As Single =
                rigaTitolo.Altezza + contornoTitolo + stacco + rigaSotto.Altezza + contornoSotto

            ' Lo spazio in cui il blocco si centra: la fascia, meno il respiro in fondo.
            Dim utile As Single = FineDelNome(area)

            If blocco > utile AndAlso blocco > 0.0F Then
                Dim fattore As Single = utile / blocco
                rigaTitolo.Corpo *= fattore
                rigaTitolo.Altezza *= fattore
                rigaSotto.Corpo *= fattore
                rigaSotto.Altezza *= fattore
                contornoTitolo *= fattore
                contornoSotto *= fattore
                stacco *= fattore
                blocco = utile
            End If

            Dim cima As Single = Math.Max(0.0F, (utile - blocco) / 2.0F)
            Dim asse As Single = area.Width / 2.0F

            g.SmoothingMode = SmoothingMode.AntiAlias
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit

            ScriviContornato(g, Titolo, famigliaTitolo, stileTitolo, rigaTitolo.Corpo,
                             contornoTitolo, asse, cima + contornoTitolo / 2.0F)

            ScriviContornato(g, Sottotitolo, famigliaSotto, stileSotto, rigaSotto.Corpo,
                             contornoSotto, asse,
                             cima + rigaTitolo.Altezza + contornoTitolo + stacco + contornoSotto / 2.0F)

        End Using

    End Sub

    ''' <summary>
    ''' La famiglia chiesta se la macchina ce l'ha, altrimenti quella di casa in grassetto.
    ''' </summary>
    ''' <remarks>
    ''' «Segoe UI Black» e «Segoe UI Semibold» sono famiglie a sé e il peso ce l'hanno
    ''' già nel nome: chiedere loro anche il grassetto le ingrasserebbe una seconda volta,
    ''' per finta. Il ripiego invece il grassetto lo vuole, perché «Segoe UI» normale su
    ''' un titolo grande sembrerebbe sbiadito.
    ''' </remarks>
    Private Shared Function FamigliaPerIlNome(nome As String, ByRef stile As FontStyle) As FontFamily

        Try
            Dim famiglia As New FontFamily(nome)
            If famiglia.IsStyleAvailable(FontStyle.Regular) Then
                stile = FontStyle.Regular
                Return famiglia
            End If
            famiglia.Dispose()
        Catch ex As ArgumentException
            ' Su questa macchina quella famiglia non c'è: si ripiega, non si muore.
        End Try

        stile = FontStyle.Bold
        Return New FontFamily(StileApp.NomeFont)

    End Function

    ''' <summary>Il corpo che rende il testo largo quanto si vuole, e quanto viene alto.</summary>
    ''' <remarks>
    ''' Si misura una volta sola a un corpo di riferimento e si scala: le metriche di un
    ''' carattere crescono in proporzione al corpo, quindi cercare la misura per tentativi
    ''' darebbe lo stesso numero dopo molte più chiamate a GDI+. Si misura con
    ''' <c>Graphics.MeasureString</c> perché a scrivere è <c>GraphicsPath.AddString</c>, e
    ''' i due parlano la stessa lingua: misurare con l'attrezzo dell'altro darebbe un
    ''' testo largo quanto non ci si aspetta.
    ''' </remarks>
    Private Shared Function RigaPerLarghezza(g As Graphics, testo As String, famiglia As FontFamily,
                                             stile As FontStyle, voluta As Single) As RigaDelNome

        Const Riferimento As Single = 100.0F

        Dim riga As New RigaDelNome With {.Corpo = Riferimento, .Altezza = Riferimento}
        If voluta <= 0.0F OrElse String.IsNullOrEmpty(testo) Then Return riga

        Using prova As New Font(famiglia, Riferimento, stile, GraphicsUnit.Pixel),
              formato As New StringFormat(StringFormat.GenericTypographic)

            Dim misura As SizeF = g.MeasureString(testo, prova, Integer.MaxValue, formato)
            If misura.Width <= 0.0F Then Return riga

            riga.Corpo = Riferimento * voluta / misura.Width
            riga.Altezza = misura.Height * riga.Corpo / Riferimento

        End Using

        Return riga

    End Function

    ''' <summary>Scrive una riga bianca contornata di nero, centrata sull'asse.</summary>
    ''' <remarks>
    ''' Il contorno si disegna <b>prima</b> del riempimento e sullo stesso percorso: una
    ''' penna larga dipinge metà dentro e metà fuori, e riempiendo dopo si riprende la
    ''' metà che aveva invaso le lettere. All'incontrario il nero mangerebbe il bianco.
    ''' </remarks>
    Private Shared Sub ScriviContornato(g As Graphics, testo As String, famiglia As FontFamily,
                                        stile As FontStyle, corpo As Single, contorno As Single,
                                        asse As Single, cima As Single)

        If corpo <= 0.0F OrElse String.IsNullOrEmpty(testo) Then Return

        Using percorso As New GraphicsPath()

            Using formato As New StringFormat(StringFormat.GenericTypographic)
                formato.Alignment = StringAlignment.Center
                percorso.AddString(testo, famiglia, CInt(stile), corpo,
                                   New PointF(asse, cima), formato)
            End Using

            If contorno >= 1.0F Then
                Using penna As New Pen(Color.Black, contorno)
                    ' Giunti tondi: a questo spessore un giunto a punta farebbe spuntare
                    ' aghi neri dai vertici delle lettere.
                    penna.LineJoin = LineJoin.Round
                    g.DrawPath(penna, percorso)
                End Using
            End If

            g.FillPath(Brushes.White, percorso)

        End Using

    End Sub

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
