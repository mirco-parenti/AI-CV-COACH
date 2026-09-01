Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi del marchio incorporato (cap. 13.5) e della schermata di avvio
    ''' (cap. 03.4): che le risorse ci siano davvero dentro l'eseguibile, e che la
    ''' schermata sappia quando togliersi.
    ''' </summary>
    ''' <remarks>
    ''' <para>Le risorse si controllano qui e non all'avvio dell'applicazione perché una
    ''' risorsa dimenticata nel file di progetto è un errore di chi costruisce, non un
    ''' guaio di chi usa: il programma parte lo stesso, con l'icona di sistema, ed è il
    ''' banco a doversene accorgere prima che esca.</para>
    ''' <para>Il tempo minimo non si aspetta mai davvero: si passa dal costruttore, così
    ''' le due strade — «il minimo è passato» e «non ancora» — si provano in zero
    ''' millisecondi. Un collaudo che misura un'attesa vera cade da solo il giorno che la
    ''' macchina è occupata, e in questa tappa uno l'ha già fatto.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiMarchio

        <TestMethod>
        Public Sub LIconaEDentroLEseguibile()

            Using flusso As Stream = Marchio.Risorsa("TrovaLavoro.ico")
                Assert.IsNotNull(flusso, "l'icona è incorporata nell'eseguibile")
            End Using

            Assert.IsNotNull(Marchio.Icona, "e si legge come icona vera")

        End Sub

        <TestMethod>
        Public Sub LIconaHaLeMisureCheWindowsCerca()

            ' Le sette misure dichiarate: 16 per l'elenco fitto, 256 per l'anteprima
            ' grande. Ne bastano meno per «funzionare», ma allora Windows scala una
            ' misura vicina e l'icona piccola diventa una macchia.
            Dim attese As Integer() = {16, 24, 32, 48, 64, 128, 256}

            Using flusso As Stream = Marchio.Risorsa("TrovaLavoro.ico")

                Dim intestazione(5) As Byte
                Assert.AreEqual(6, flusso.Read(intestazione, 0, 6), "l'intestazione si legge")
                Assert.AreEqual(1, BitConverter.ToInt16(intestazione, 2), "è un'icona, non un cursore")

                Dim quante As Integer = BitConverter.ToInt16(intestazione, 4)
                Assert.AreEqual(attese.Length, quante, "quante misure porta dentro")

                For Each atteso As Integer In attese
                    Dim voce(15) As Byte
                    flusso.Read(voce, 0, 16)
                    ' Nel formato ICO il 256 si scrive 0: un byte non ci arriva.
                    Dim lato As Integer = If(voce(0) = 0, 256, CInt(voce(0)))
                    Assert.AreEqual(atteso, lato, $"la misura attesa {atteso}")
                    Assert.IsGreaterThan(0, BitConverter.ToInt32(voce, 8), "e non è vuota")
                Next

            End Using

        End Sub

        ''' <summary>
        ''' Le tre scritte del pannello del logo si leggono sul fondo che il pannello ha.
        ''' </summary>
        ''' <remarks>
        ''' Il collaudo non guarda i colori uno per uno — quelli sono la tabella del
        ''' cap. 03.2 — ma il legame fra due scelte che si fanno in due punti diversi del
        ''' designer: il fondo del pannello e il colore delle sue scritte. Cambiare il
        ''' primo e dimenticare il secondo non rompe niente e non spegne nessun collaudo:
        ''' il testo resta lì, dello stesso colore di prima, e diventa semplicemente
        ''' illeggibile. È successo il 2026-08-30, provando un fondo blu: su quel blu
        ''' <c>TestoPrimario</c> faceva 1,2 a 1, dove la soglia è 4,5. Il fondo è poi
        ''' tornato chiaro, ma la misura resta — vale per qualunque fondo si scelga.
        ''' </remarks>
        <TestMethod>
        Public Sub LeScritteDelMarchioSiLeggonoSulLoroFondo()

            Using form As New FormPrincipale()

                Dim pannello As Control =
                    form.Controls.Find("pnlLogo", searchAllChildren:=True).Single()

                Dim etichette As Label() = pannello.Controls.OfType(Of Label)().ToArray()
                Assert.IsGreaterThanOrEqualTo(3, etichette.Length,
                                              "nome, versione e copyright: le tre scritte ci sono")

                ' La soglia non è il 4,5 di WCAG per decreto: è il contrasto che
                ' l'applicazione **già** usa per una didascalia sul fondo peggiore che le
                ' capita — TestoSecondario sul fondo caldo delle pagine. Le scritte del
                ' marchio devono leggersi almeno quanto tutte le altre; se la tavolozza va
                ' rivista, si rivede là e questo collaudo si adegua da sé. Fino al
                ' 2026-08-30 la coppia di riferimento valeva 4,45 a 1, un centesimo sotto
                ' il 4,5, e il metro relativo serviva anche a non fingere che passasse; a
                ' sorvegliare il 4,5 vero c'è il collaudo qui sotto.
                '
                ' Il fondo del metro era SfondoBase (4,71) fino al 2026-09-01, quando il
                ' pannello del logo ha smesso di avere un fondo suo per prendere quello di
                ' chi gli sta sotto: sulle pagine è FondoPagina, dove la stessa coppia fa
                ' 4,52. Non è un peggioramento delle scritte del marchio — è il fondo
                ' dell'applicazione che si è scaldato il 2026-08-31, e su quel fondo ogni
                ' didascalia delle sei pagine sta già.
                Dim soglia As Double = Contrasto(StileApp.TestoSecondario, StileApp.FondoPagina)

                For Each etichetta As Label In etichette
                    Assert.IsGreaterThanOrEqualTo(
                        soglia, Contrasto(etichetta.ForeColor, pannello.BackColor),
                        $"«{etichetta.Name}» si legge peggio di una didascalia qualunque")
                Next

            End Using

        End Sub

        ''' <summary>
        ''' La coppia con cui si scrive ogni didascalia sta sopra la soglia WCAG.
        ''' </summary>
        ''' <remarks>
        ''' <para><c>TestoSecondario</c> su <c>SfondoBase</c> non è una coppia fra le
        ''' tante: è quella con cui l'applicazione scrive <b>ogni</b> didascalia, ogni
        ''' suggerimento e ogni stato. Fino al 2026-08-30 valeva <b>4,45 a 1</b> — un
        ''' centesimo sotto il 4,5 che WCAG 2 chiede a un testo piccolo — e nessuno se ne
        ''' accorgeva, perché l'unico collaudo che misurasse un contrasto usava proprio
        ''' quella coppia come metro: un metro non può dire di sé stesso che è corto. Il
        ''' grigio è passato a <c>#6A737C</c>, tre punti più scuro, e adesso fa 4,57.</para>
        ''' <para>Si guarda anche il fondo bianco delle aree di lavoro, dove quella coppia
        ''' era già a posto (4,69, oggi 4,82). Una tavolozza si aggiusta guardando il fondo
        ''' peggiore, ma si collauda su tutti e due: una cura può spostare il problema
        ''' invece di toglierlo, e su due fondi diversi lo spostamento si vede.</para>
        ''' <para>Dal 2026-08-31 i fondi sono <b>quattro</b>: le pagine hanno preso
        ''' l'avorio della soglia — <c>FondoPagina</c> e <c>FondoCasella</c> — e il primo
        ''' dei due è il più scuro di tutti, quindi il peggiore. Guardare solo i due di
        ''' prima sarebbe stato peggio che non guardare: il collaudo sarebbe restato verde
        ''' misurando un fondo che le didascalie non hanno più sotto. È servito: col grigio
        ''' di ieri quel fondo faceva 4,39, e la tavolozza si è mossa per questo.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub OgniDidascaliaSiLeggeQuantoWcagChiede()

            ' Qui la soglia è il 4,5 di WCAG e basta: è il collaudo che deve dire se la
            ' tavolozza ci arriva, e un metro preso dalla tavolozza stessa non potrebbe.
            Const soglia As Double = 4.5

            Assert.IsGreaterThanOrEqualTo(
                soglia, Contrasto(StileApp.TestoSecondario, StileApp.SfondoBase),
                "le didascalie sul fondo delle finestre")

            Assert.IsGreaterThanOrEqualTo(
                soglia, Contrasto(StileApp.TestoSecondario, StileApp.SfondoContenuto),
                "le didascalie sulle aree di lavoro")

            Assert.IsGreaterThanOrEqualTo(
                soglia, Contrasto(StileApp.TestoSecondario, StileApp.FondoPagina),
                "le didascalie sul fondo delle pagine")

            Assert.IsGreaterThanOrEqualTo(
                soglia, Contrasto(StileApp.TestoSecondario, StileApp.FondoCasella),
                "le didascalie dentro le caselle delle pagine")

        End Sub

        ''' <summary>
        ''' Ogni bottone colorato porta un testo che si legge, e la scala dei livelli sale
        ''' fino in fondo.
        ''' </summary>
        ''' <remarks>
        ''' <para>La didascalia era la coppia più <b>usata</b>, non l'unica, e guardare solo
        ''' lei ha lasciato passare per undici tappe due fondi che non erano nemmeno di
        ''' confine <i>(2026-09-01)</i>: il verde di <see cref="StileApp.Successo"/> col
        ''' bianco sopra faceva <b>3,13 a 1</b> — ed è il fondo di «Salva profilo» e della
        ''' casella «🎮 Menu» — e il rosso del livello 6 ne faceva 4,10.</para>
        ''' <para>Si misurano i bottoni <b>vestiti</b> e non i colori della tabella, perché
        ''' le due cose si possono rompere separatamente: un token può essere a posto e un
        ''' livello pescare quello sbagliato. È esattamente il difetto che c'era — il livello
        ''' 6 portava il rosso del <b>marchio</b>, che è di casa nei titoli.</para>
        ''' <para>Nella stessa prova sta la <b>scala</b>: «la saturazione cresce con il peso
        ''' della conseguenza» (cap. 03.3) smetteva di crescere all'ultimo gradino, e il
        ''' gesto da cui non si torna indietro si vestiva del colore meno grave. Si guarda
        ''' quanto bianco ci si legge sopra, che è la stessa domanda detta al contrario: più
        ''' scuro è il fondo, più il bianco stacca. E si guarda <b>quella</b> e non
        ''' <c>Color.GetBrightness</c>, che è la luminosità HSL e su questi due rossi dice il
        ''' contrario: per lei <c>#FA0825</c> è più scuro di <c>#DC3545</c>, mentre l'occhio
        ''' — e WCAG — vedono l'opposto. Scritta con quel metro la prova restava verde anche
        ''' rimettendo il rosso del marchio al livello 6, cioè era verde per il motivo
        ''' sbagliato.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub OgniBottoneColoratoPortaUnTestoCheSiLegge()

            Const soglia As Double = 4.5

            For Each livello As LivelloBottone In [Enum].GetValues(GetType(LivelloBottone))
                Using bottone As New Button()

                    StileApp.VestiBottone(bottone, livello)

                    Assert.IsGreaterThanOrEqualTo(
                        soglia, Contrasto(bottone.ForeColor, bottone.BackColor),
                        $"il testo del livello {livello} sul suo fondo")

                End Using
            Next

            For Each ruolo As RuoloBarra In [Enum].GetValues(GetType(RuoloBarra))
                For Each aperta As Boolean In {False, True}
                    Using casella As New Button()

                        StileApp.VestiBottoneBarra(casella, ruolo, aperta)

                        Assert.IsGreaterThanOrEqualTo(
                            soglia, Contrasto(casella.ForeColor, casella.BackColor),
                            $"il testo della casella {ruolo}, aperta={aperta}")

                    End Using
                Next
            Next

            Using distruttivo As New Button(), critico As New Button()

                StileApp.VestiBottone(distruttivo, LivelloBottone.Distruttivo)
                StileApp.VestiBottone(critico, LivelloBottone.Critico)

                Assert.IsGreaterThan(
                    Contrasto(StileApp.SfondoContenuto, distruttivo.BackColor),
                    Contrasto(StileApp.SfondoContenuto, critico.BackColor),
                    "il fondo del livello 6 è più scuro di quello del livello 5")

            End Using

        End Sub

        ''' <summary>
        ''' E ogni inchiostro colorato si legge su tutti i fondi su cui viene scritto.
        ''' </summary>
        ''' <remarks>
        ''' È l'altra direzione della prova qui sopra, e va tenuta separata perché è un
        ''' difetto di un'altra specie: un colore nato per stare <b>sotto</b> il bianco
        ''' finisce a fare da <b>lettere</b> su un fondo chiaro, e lì non regge più. Nel
        ''' 2026-09-01 ce n'erano due così — l'azzurro informativo, in Home usato per il
        ''' promemoria dei solleciti e le righe da sollecitare (2,77 a 1), e il verde
        ''' dell'esito «assunto» in P4 (2,85) — e per il primo è nato un token apposta.
        ''' <para>Si provano tutti e quattro i fondi: un inchiostro può reggere sul bianco
        ''' delle finestre e cadere sull'avorio delle pagine, che parte già più scuro.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub OgniInchiostroColoratoSiLeggeSuiFondiDellApplicazione()

            Const soglia As Double = 4.5

            Dim fondiChiari As Color() = {StileApp.SfondoBase, StileApp.SfondoContenuto,
                                          StileApp.FondoPagina, StileApp.FondoCasella}

            For Each fondo As Color In fondiChiari

                Dim dove As String = ColorTranslator.ToHtml(fondo)

                Assert.IsGreaterThanOrEqualTo(
                    soglia, Contrasto(StileApp.RossoCritico, fondo),
                    $"il rosso dei titoli di gruppo su {dove}")

                Assert.IsGreaterThanOrEqualTo(
                    soglia, Contrasto(StileApp.InformazioneTesto, fondo),
                    $"il testo informativo su {dove}")

                Assert.IsGreaterThanOrEqualTo(
                    soglia, Contrasto(StileApp.Successo, fondo),
                    $"il verde dell'esito «assunto» su {dove}")

            Next

        End Sub

        ''' <summary>
        ''' Il pannello del logo non ha nessun contorno: si fonde con quel che ha sotto.
        ''' </summary>
        ''' <remarks>
        ''' <para>Fino al 2026-09-01 attorno al pannello girava un <b>filo nero</b>, e
        ''' questo collaudo era il suo guardiano: fotografava il pannello e contava i pixel
        ''' del perimetro, perché un contorno è una cosa che o si vede o non si vede. Il
        ''' tutor l'ha fatto togliere quel giorno — via il filo, via il fondo proprio — e
        ''' adesso il collaudo difende l'<b>assenza</b>, con la stessa fotografia e lo
        ''' stesso perimetro: dove c'era il filo si deve leggere il fondo del pannello, in
        ''' tutti e quattro i lati. Rimettere la penna nera lo fa cadere a ogni pixel.</para>
        ''' <para>Il pannello va costruito (<c>CreateControl</c>) o non ha finestra da
        ''' fotografare; costruire un figlio costruisce anche la finestra principale, ma
        ''' <b>non</b> ne fa girare il <c>Load</c> — quello lo chiama solo chi la mostra.
        ''' Per la stessa ragione nella foto ci sono solo il fondo e quel che il pannello
        ''' dipinge da sé: i figli, invisibili per eredità su una finestra mai mostrata,
        ''' non si stampano.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub IlPannelloDelMarchioNonHaContorno()

            Using form As New FormPrincipale()

                Dim pannello As Control =
                    form.Controls.Find("pnlLogo", searchAllChildren:=True).Single()

                pannello.CreateControl()

                Dim destra As Integer = pannello.Width - 1
                Dim fondo As Integer = pannello.Height - 1

                Using foto As New Bitmap(pannello.Width, pannello.Height)

                    pannello.DrawToBitmap(foto, New Rectangle(0, 0, pannello.Width, pannello.Height))

                    Dim atteso As Integer = pannello.BackColor.ToArgb()

                    ' Il perimetro **tutto**, non qualche punto scelto: un filo rimasto
                    ' anche su un lato solo è il riquadro che si voleva togliere.
                    For x As Integer = 0 To destra
                        Assert.AreEqual(atteso, foto.GetPixel(x, 0).ToArgb(),
                                        $"in cima, in x={x}, c'è ancora un contorno")
                        Assert.AreEqual(atteso, foto.GetPixel(x, fondo).ToArgb(),
                                        $"in fondo, in x={x}, c'è ancora un contorno")
                    Next

                    For y As Integer = 0 To fondo
                        Assert.AreEqual(atteso, foto.GetPixel(0, y).ToArgb(),
                                        $"a sinistra, in y={y}, c'è ancora un contorno")
                        Assert.AreEqual(atteso, foto.GetPixel(destra, y).ToArgb(),
                                        $"a destra, in y={y}, c'è ancora un contorno")
                    Next

                End Using

            End Using

        End Sub

        ''' <summary>
        ''' Il pannello del logo porta il fondo di quel che gli sta sotto, e cambia con lui.
        ''' </summary>
        ''' <remarks>
        ''' <para>È l'altra metà della rifinitura del 2026-09-01: tolto il filo, un fondo
        ''' <b>suo</b> lascerebbe comunque un rettangolo visibile: il riquadro senza il
        ''' bordo, che è quasi peggio. E il fondo sotto non è uno solo — avorio nel menu
        ''' d'ingresso, caldo nelle sei pagine — quindi non basta scegliere bene una volta:
        ''' il pannello lo deve <b>seguire</b>.</para>
        ''' <para>Si guarda il valore di riposo del designer, che è quello delle pagine, e
        ''' poi si apre il menu d'ingresso dalla strada della finestra: se
        ''' <c>MostraPannello</c> smettesse di riallineare il fondo, il logo tornerebbe a
        ''' essere un rettangolo grigino sull'avorio.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub IlPannelloDelMarchioPrendeIlFondoDiChiGliStaSotto()

            Using form As New FormPrincipale()

                Dim pannello As Control =
                    form.Controls.Find("pnlLogo", searchAllChildren:=True).Single()
                Dim centrale As Control =
                    form.Controls.Find("pnlAreaCentrale", searchAllChildren:=True).Single()

                Assert.AreEqual(centrale.BackColor, pannello.BackColor,
                                "a riposo è il fondo dell'area centrale")

                Dim menu As Control = form.Controls.Find("pnlMenu", searchAllChildren:=True).Single()
                Dim bottone As Button =
                    DirectCast(form.Controls.Find("btnMenu", searchAllChildren:=True).Single(), Button)

                ApriIlPannelloDaFuori(form, menu, bottone)

                Assert.AreEqual(menu.BackColor, pannello.BackColor,
                                "aperto il menu d'ingresso, prende l'avorio della soglia")
                Assert.AreNotEqual(centrale.BackColor, pannello.BackColor,
                                   "che è un altro colore: se fossero uguali questo collaudo non direbbe niente")

            End Using

        End Sub

        ''' <summary>
        ''' Lo stemma non invita più a un clic, perché non risponde più a nessuno.
        ''' </summary>
        ''' <remarks>
        ''' <para>Fino al 2026-09-01 il pannello del logo era la <b>porta</b> di
        ''' «Informazioni su…»: cliccandolo — su una qualunque delle sue cinque parti — si
        ''' apriva quella finestra, e per dirlo portava il puntatore a mano e il
        ''' suggerimento «Informazioni su TrovaLavoro». Su indicazione del tutor la porta è
        ''' stata tolta: lo stemma è un'insegna, e «Informazioni su…» si raggiunge dalle
        ''' Impostazioni (v. <c>CollaudiFinestraImpostazioni</c>).</para>
        ''' <para>Qui si difende l'<b>assenza</b>, e si difende quel che l'utente vede: la
        ''' mano e il suggerimento sono l'invito, e un invito sopra qualcosa che non
        ''' risponde è peggio di nessun invito — è la lezione di T9d («quel che è acceso
        ''' deve sembrarlo») letta al contrario. Il gestore del clic non lascia traccia che
        ''' si possa interrogare da fuori; l'invito sì, e rimetterne anche solo metà fa
        ''' cadere questo collaudo.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub LoStemmaNonInvitaPiuAUnClic()

            Using form As New FormPrincipale()

                Dim pannello As Control =
                    form.Controls.Find("pnlLogo", searchAllChildren:=True).Single()

                Dim suggerimenti As ToolTip = SuggerimentiDellaFinestra(form)

                Dim parti As New List(Of Control) From {pannello}
                parti.AddRange(pannello.Controls.Cast(Of Control)())

                Assert.IsGreaterThanOrEqualTo(4, parti.Count,
                    "il pannello e le sue parti: se fossero meno, il collaudo guarderebbe quasi niente")

                For Each parte As Control In parti

                    Assert.AreNotEqual(Cursors.Hand, parte.Cursor,
                                       $"«{parte.Name}» promette ancora un clic col puntatore a mano")

                    Assert.IsEmpty(suggerimenti.GetToolTip(parte),
                                   $"«{parte.Name}» promette ancora un clic col suggerimento")

                Next

            End Using

        End Sub

        ''' <summary>
        ''' Il fornitore di suggerimenti della finestra principale. Non è un controllo e non
        ''' si trova con <c>Controls.Find</c>: è un componente, e da fuori si arriva solo al
        ''' campo che lo tiene — come già si fa qui sotto per <c>MostraPannello</c>.
        ''' </summary>
        Private Shared Function SuggerimentiDellaFinestra(form As Form) As ToolTip

            Dim campo As FieldInfo = form.GetType().GetField(
                "_ttSuggerimenti", BindingFlags.Instance Or BindingFlags.NonPublic)

            Assert.IsNotNull(campo, "la finestra ha ancora il suo fornitore di suggerimenti")

            Return DirectCast(campo.GetValue(form), ToolTip)

        End Function

        ''' <summary>
        ''' Chiama <c>MostraPannello</c> per la strada che percorre la finestra quando si
        ''' preme una casella della barra.
        ''' </summary>
        Private Shared Sub ApriIlPannelloDaFuori(form As Form, pannello As Control, bottone As Button)

            Dim mostra As MethodInfo = form.GetType().GetMethod(
                "MostraPannello", BindingFlags.Instance Or BindingFlags.NonPublic)

            Assert.IsNotNull(mostra, "la finestra ha ancora il suo MostraPannello")

            mostra.Invoke(form, New Object() {pannello, bottone})

        End Sub

        ''' <summary>
        ''' Il rapporto di contrasto WCAG fra due colori: 1 se sono identici, 21 fra
        ''' bianco e nero. Sotto 4,5 un testo piccolo non si legge.
        ''' </summary>
        Private Shared Function Contrasto(uno As Color, altro As Color) As Double

            Dim a As Double = Luminanza(uno)
            Dim b As Double = Luminanza(altro)

            Return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05)

        End Function

        ''' <summary>La luminanza relativa di un colore, come la definisce WCAG 2.</summary>
        Private Shared Function Luminanza(colore As Color) As Double

            Return 0.2126 * Canale(colore.R) +
                   0.7152 * Canale(colore.G) +
                   0.0722 * Canale(colore.B)

        End Function

        ''' <summary>Un canale da 0-255 portato in luce lineare.</summary>
        Private Shared Function Canale(valore As Byte) As Double

            Dim c As Double = valore / 255.0

            Return If(c <= 0.03928, c / 12.92, Math.Pow((c + 0.055) / 1.055, 2.4))

        End Function

        <TestMethod>
        Public Sub LaSchermataDiAvvioEDentroLEseguibile()

            Dim immagine As Image = Marchio.SchermataDiAvvio

            Assert.IsNotNull(immagine, "la schermata di avvio è incorporata")
            Assert.AreEqual(800, immagine.Width, "larghezza come disegnata")
            Assert.AreEqual(702, immagine.Height, "altezza come disegnata")

        End Sub

        <TestMethod>
        Public Sub LaSchermataSiLeggeDueVolteSenzaRicaricarla()

            ' Le due porte che la mostrano — l'avvio e «Informazioni su…» — possono
            ' capitare nella stessa sessione: se ogni lettura ne facesse una copia, la
            ' seconda troverebbe un'immagine liberata dalla prima.
            Assert.AreSame(Marchio.SchermataDiAvvio, Marchio.SchermataDiAvvio,
                           "è sempre la stessa immagine")

        End Sub

        <TestMethod>
        Public Sub UnaRisorsaCheNonCEsisteNonFaCadereNiente()

            Using flusso As Stream = Marchio.Risorsa("questa-non-esiste.png")
                Assert.IsNull(flusso, "chi non c'è torna Nothing, non un'eccezione")
            End Using

        End Sub

        <TestMethod>
        Public Sub ChiHaGiaAspettatoAbbastanzaNonAspettaAncora()

            Assert.AreEqual(TimeSpan.Zero,
                            FinestraAvvio.AttesaRimasta(TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(1)),
                            "chi ha già superato il minimo non aspetta")

            Assert.AreEqual(TimeSpan.Zero,
                            FinestraAvvio.AttesaRimasta(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)),
                            "e nemmeno chi ci è arrivato esatto")

            Assert.AreEqual(TimeSpan.FromMilliseconds(400),
                            FinestraAvvio.AttesaRimasta(TimeSpan.FromMilliseconds(600), TimeSpan.FromSeconds(1)),
                            "a chi è arrivato prima resta la differenza")

        End Sub

        <TestMethod>
        Public Sub SuUnoSchermoGrandeLaSchermataRestaComeDisegnata()

            Dim misura As Size = FinestraAvvio.MisuraDaMostrare(New Size(800, 702), New Size(1920, 1080))

            Assert.AreEqual(New Size(800, 702), misura, "sta comoda: non si tocca")

        End Sub

        <TestMethod>
        Public Sub SuUnoSchermoPiccoloSiRimpicciolisceSenzaDeformarsi()

            ' 1024x768: 800 di larghezza sono già oltre la quota concessa.
            Dim misura As Size = FinestraAvvio.MisuraDaMostrare(New Size(800, 702), New Size(1024, 768))

            Assert.IsLessThan(800, misura.Width, "si è ridotta")
            Assert.IsLessThanOrEqualTo(CInt(1024 * FinestraAvvio.QuotaSchermo), misura.Width,
                                       "sta nella quota di schermo concessa")
            Assert.IsLessThanOrEqualTo(CInt(768 * FinestraAvvio.QuotaSchermo), misura.Height,
                                       "in altezza come in larghezza")

            Dim proporzioneOriginale As Double = 800.0 / 702.0
            Dim proporzioneRidotta As Double = misura.Width / CDbl(misura.Height)
            Assert.AreEqual(proporzioneOriginale, proporzioneRidotta, 0.01,
                            "le proporzioni non si toccano: l'immagine non si schiaccia")

        End Sub

        <TestMethod>
        Public Sub SenzaUnoSchermoLaMisuraRestaQuellaDellImmagine()

            Assert.AreEqual(New Size(800, 702),
                            FinestraAvvio.MisuraDaMostrare(New Size(800, 702), Size.Empty),
                            "senza spazio noto non si inventa una riduzione")

        End Sub

        ''' <summary>Il minimo garantito è quello che il capitolo promette: dieci secondi.</summary>
        ''' <remarks>
        ''' Erano cinque fino al 2026-09-01, quando il tutor li ha raddoppiati. È l'unico
        ''' numero di questa finestra che nessun altro collaudo può vedere: tutti gli altri
        ''' si passano un minimo <b>loro</b> dal costruttore, apposta per non aspettare
        ''' davvero, e resterebbero verdi anche se il valore predefinito sparisse.
        ''' </remarks>
        <TestMethod>
        Public Sub IlMinimoAVideoEQuelloDichiarato()

            Assert.AreEqual(TimeSpan.FromSeconds(10), FinestraAvvio.MinimoAVideo,
                            "dieci secondi, come dice il cap. 03.4")

        End Sub

        ''' <summary>Invio manda via la schermata, e si mangia il tasto.</summary>
        ''' <remarks>
        ''' <para>Il tasto arriva da un <b>filtro dei messaggi</b> e non da un
        ''' <c>KeyDown</c>, perché la schermata il fuoco della tastiera non ce l'ha quasi
        ''' mai: la finestra principale si apre e si attiva mentre lo splash è ancora a
        ''' video (v. <c>FinestraAvvio.PreFilterMessage</c>). Il collaudo entra dalla stessa
        ''' porta da cui entrerebbe Windows, costruendo il messaggio a mano: non serve né
        ''' mostrare la finestra né avere una tastiera.</para>
        ''' <para>Si guarda anche che il tasto sia <b>consumato</b>: l'Invio che manda via
        ''' la schermata non deve arrivare anche al bottone che ha il fuoco dietro di lei.
        ''' </para>
        ''' </remarks>
        <TestMethod>
        Public Sub InvioMandaViaLaSchermataSenzaAspettareIlMinimo()

            Const WmKeyDown As Integer = &H100

            Using schermata As New FinestraAvvio(Nothing, TimeSpan.FromMinutes(10))

                Dim altroTasto As Message = Message.Create(
                    IntPtr.Zero, WmKeyDown, New IntPtr(CInt(Keys.A)), IntPtr.Zero)

                Assert.IsFalse(schermata.PreFilterMessage(altroTasto),
                               "gli altri tasti passano oltre, non sono suoi")
                Assert.IsFalse(schermata.GiaChiusa, "e non la mandano via")

                Dim invio As Message = Message.Create(
                    IntPtr.Zero, WmKeyDown, New IntPtr(CInt(Keys.Enter)), IntPtr.Zero)

                Assert.IsTrue(schermata.PreFilterMessage(invio), "l'Invio se lo tiene")
                Assert.IsTrue(schermata.GiaChiusa, "e la manda via, minimo o non minimo")

            End Using

        End Sub

        ''' <summary>E il clic la manda via come ha sempre fatto.</summary>
        ''' <remarks>
        ''' <para>Il clic c'era da prima di Invio, e il tutor l'ha voluto <b>dichiarato</b>:
        ''' con il minimo passato da cinque a dieci secondi, la via d'uscita di sempre
        ''' diventa più importante di prima, e quel che non è scritto da nessuna parte si
        ''' perde alla prima riscrittura.</para>
        ''' <para>Si chiama il gestore per riflesso e non <c>PerformClick</c>: su una
        ''' finestra mai mostrata quello non fa niente e non lo dice. Il collaudo prova
        ''' quindi che <b>al clic la schermata si chiude</b>, non che il clic ci arrivi:
        ''' l'aggancio è la clausola <c>Handles</c>, che il compilatore verifica —
        ''' <c>picSchermata</c> che sparisse o cambiasse nome non compilerebbe.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub IlClicMandaViaLaSchermataSenzaAspettareIlMinimo()

            Using schermata As New FinestraAvvio(Nothing, TimeSpan.FromMinutes(10))

                Dim cliccata As MethodInfo = GetType(FinestraAvvio).GetMethod(
                    "Cliccata", BindingFlags.Instance Or BindingFlags.NonPublic)

                Assert.IsNotNull(cliccata, "«Cliccata» è la porta da cui il clic la chiude")

                cliccata.Invoke(schermata, New Object() {Nothing, EventArgs.Empty})

                Assert.IsTrue(schermata.GiaChiusa, "un clic la manda via, minimo o non minimo")

            End Using

        End Sub

        ''' <summary>E una volta andata via non si mangia più niente.</summary>
        ''' <remarks>
        ''' Il filtro si toglie alla chiusura vera (<c>OnFormClosed</c>), ma nel banco la
        ''' finestra non si mostra e non si chiude davvero: questa guardia è sulla prima
        ''' riga del filtro, quella che si ferma se la schermata è già andata. Senza,
        ''' <c>ChiudiSubito</c> verrebbe chiamato a ogni Invio del programma.
        ''' </remarks>
        <TestMethod>
        Public Sub UnaSchermataGiaChiusaNonSiMangiaPiuLInvio()

            Const WmKeyDown As Integer = &H100

            Using schermata As New FinestraAvvio(Nothing, TimeSpan.FromMinutes(10))

                schermata.ChiudiSubito()

                Dim invio As Message = Message.Create(
                    IntPtr.Zero, WmKeyDown, New IntPtr(CInt(Keys.Enter)), IntPtr.Zero)

                Assert.IsFalse(schermata.PreFilterMessage(invio),
                               "l'Invio torna a chi lo aspettava")

            End Using

        End Sub

        <TestMethod>
        Public Sub ChiudiSubitoNonAspettaIlMinimo()

            Using schermata As New FinestraAvvio(Nothing, TimeSpan.FromMinutes(10))

                Assert.IsFalse(schermata.GiaChiusa, "appena nata è a video")

                schermata.ChiudiSubito()

                Assert.IsTrue(schermata.GiaChiusa, "va via subito, minimo o non minimo")

            End Using

        End Sub

        <TestMethod>
        Public Sub ConIlMinimoScadutoLaChiusuraNormaleEImmediata()

            Using schermata As New FinestraAvvio(Nothing, TimeSpan.Zero)

                schermata.ChiudiQuandoPuoi()

                Assert.IsTrue(schermata.GiaChiusa, "niente da aspettare: si chiude")

            End Using

        End Sub

        <TestMethod>
        Public Sub ConIlMinimoNonScadutoLaChiusuraNormaleAspetta()

            Using schermata As New FinestraAvvio(Nothing, TimeSpan.FromMinutes(10))

                schermata.ChiudiQuandoPuoi()

                Assert.IsFalse(schermata.GiaChiusa,
                               "il minimo non è passato: resta a video invece di lampeggiare")

            End Using

        End Sub

        <TestMethod>
        Public Sub ChiudereDueVolteNonFaDanno()

            Using schermata As New FinestraAvvio(Nothing, TimeSpan.Zero)

                schermata.ChiudiSubito()
                schermata.ChiudiSubito()
                schermata.ChiudiQuandoPuoi()

                Assert.IsTrue(schermata.GiaChiusa, "chiusa e richiusa, resta chiusa")

            End Using

        End Sub

        <TestMethod>
        Public Sub LaSchermataEQuellaCheLaFinestraPrincipaleSaChiedere()

            ' Il contratto che tiene insieme le due parti: la finestra principale non
            ' conosce FinestraAvvio, conosce l'interfaccia.
            Using schermata As New FinestraAvvio(Nothing, TimeSpan.Zero)
                Assert.IsInstanceOfType(Of ISchermataDiAvvio)(schermata)
            End Using

        End Sub

    End Class

End Namespace
