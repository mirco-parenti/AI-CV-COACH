Imports System.Drawing
Imports System.IO
Imports System.Linq
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
                ' l'applicazione **già** usa per ogni didascalia — TestoSecondario su
                ' SfondoBase. Le scritte del marchio devono leggersi almeno quanto tutte
                ' le altre; se la tavolozza va rivista, si rivede là e questo collaudo si
                ' adegua da sé. Fino al 2026-08-30 quella coppia valeva 4,45 a 1, un
                ' centesimo sotto il 4,5, e il metro relativo serviva anche a non fingere
                ' che passasse: adesso passa (4,57), e a sorvegliarlo c'è il collaudo qui
                ' sotto.
                Dim soglia As Double = Contrasto(StileApp.TestoSecondario, StileApp.SfondoBase)

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

        End Sub

        ''' <summary>
        ''' Il pannello del logo ha il suo filo nero, e il filo gira tutt'intorno.
        ''' </summary>
        ''' <remarks>
        ''' <para>Si fotografa il pannello vero e si guardano i pixel del bordo, invece di
        ''' rileggere il codice che li disegna: un contorno è una cosa che o si vede o non
        ''' si vede, e le due maniere di sbagliarlo non lasciano altra traccia. La prima è
        ''' disegnare il rettangolo sulle misure piene invece che a <c>-1</c>, e allora due
        ''' lati su quattro cadono fuori dall'area. La seconda è più insidiosa: le tre
        ''' etichette sono larghe quanto il pannello e hanno il fondo opaco, e i figli si
        ''' disegnano dopo il genitore — alla larghezza piena mangiano il filo verticale
        ''' <b>solo alle righe che occupano</b>. Il contorno resterebbe lì, interrotto tre
        ''' volte, e a occhio sembrerebbe intero. Per questo fra i punti misurati ce n'è
        ''' uno preso apposta all'altezza del nome.</para>
        ''' <para>Il pannello va costruito (<c>CreateControl</c>) o non ha finestra da
        ''' fotografare; costruire un figlio costruisce anche la finestra principale, ma
        ''' <b>non</b> ne fa girare il <c>Load</c> — quello lo chiama solo chi la mostra.
        ''' </para>
        ''' <para><b>Quel che la fotografia NON può vedere, e perché il rientro si misura
        ''' invece di guardarlo.</b> Su una finestra mai mostrata i figli sono
        ''' <c>Visible = False</c> per eredità, quindi <c>CreateControl</c> non dà loro una
        ''' finestra e <c>DrawToBitmap</c> non li stampa: nella foto ci sono solo il fondo
        ''' e il filo, zero pixel d'altro. Detto altrimenti, questa foto **non può**
        ''' accorgersi di un'etichetta che copre il contorno, e provandolo apposta —
        ''' rimettendo le etichette a tutta larghezza — il collaudo restava verde. Il
        ''' rischio è vero lo stesso: nel programma vero le etichette si vedono eccome. Per
        ''' questo il rientro si verifica sui <b>bordi</b>, che sono veri anche senza una
        ''' finestra, e non sui pixel. Le due misure guardano due cose diverse: la foto
        ''' che il filo ci sia e sia chiuso a <c>-1</c>, la geometria che nessuno glielo
        ''' vada sopra.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub IlPannelloDelMarchioHaIlSuoContorno()

            Using form As New FormPrincipale()

                Dim pannello As Control =
                    form.Controls.Find("pnlLogo", searchAllChildren:=True).Single()

                pannello.CreateControl()

                Dim destra As Integer = pannello.Width - 1
                Dim fondo As Integer = pannello.Height - 1

                Using foto As New Bitmap(pannello.Width, pannello.Height)

                    pannello.DrawToBitmap(foto, New Rectangle(0, 0, pannello.Width, pannello.Height))

                    Dim atteso As Integer = StileApp.BordoMarchio.ToArgb()

                    ' Si guarda il perimetro **tutto**, non qualche punto scelto: dove il
                    ' filo si interrompe dipende da dove capitano le etichette, e quello
                    ' cambia con la modalità (piena o compatta) e con il DPI della
                    ' macchina. Un collaudo che sceglie i punti a mano sceglie anche, senza
                    ' saperlo, di non guardare dove il difetto sarebbe.
                    For x As Integer = 0 To destra
                        Assert.AreEqual(atteso, foto.GetPixel(x, 0).ToArgb(),
                                        $"il filo di sopra si interrompe in x={x}")
                        Assert.AreEqual(atteso, foto.GetPixel(x, fondo).ToArgb(),
                                        $"il filo di sotto si interrompe in x={x}")
                    Next

                    For y As Integer = 0 To fondo
                        Assert.AreEqual(atteso, foto.GetPixel(0, y).ToArgb(),
                                        $"il filo di sinistra si interrompe in y={y}")
                        Assert.AreEqual(atteso, foto.GetPixel(destra, y).ToArgb(),
                                        $"il filo di destra si interrompe in y={y}")
                    Next

                    ' E dentro no: un contorno che riempie non è un contorno.
                    Assert.AreEqual(pannello.BackColor.ToArgb(),
                                    foto.GetPixel(4, fondo - 4).ToArgb(),
                                    "appena dentro il filo il fondo è quello del pannello")

                End Using

            End Using

        End Sub

        ''' <summary>
        ''' Nessuna scritta del pannello va a finire sopra il filo del contorno, in
        ''' nessuna delle due modalità.
        ''' </summary>
        ''' <remarks>
        ''' <para>È la metà del collaudo che la fotografia non può fare (v.
        ''' <see cref="IlPannelloDelMarchioHaIlSuoContorno"/>): i figli si misurano dai
        ''' loro bordi, che esistono anche su una finestra mai mostrata, invece che dai
        ''' pixel, che non esistono.</para>
        ''' <para><b>Perché due modalità e non una.</b> Il pannello si dispone in due modi
        ''' — pieno e compatto — e i due non riguardano gli stessi controlli: in compatto
        ''' restano lo stemma rimpicciolito e la sola riga della versione, e nome e
        ''' copyright si nascondono. Provare una modalità sola vuol dire non guardare le
        ''' due etichette che l'altra dispone. La larghezza che sceglie la modalità si
        ''' misura in unità di progetto, cioè scalata col DPI: 4000 pixel sono modalità
        ''' piena fino al 280%, 900 sono compatta sempre. Un numero al limite avrebbe
        ''' collaudato una cosa diversa sulla macchina del tutor.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub NessunaScrittaVaSopraIlContorno()

            Using form As New FormPrincipale()

                ' Piena: ci stanno tutte e tre le scritte.
                ControllaIlRientro(form, 4000, {"lblMarchio", "lblVersione", "lblCopyright"})

                ' Compatta: il pannello si stringe e dispone la sola riga della versione.
                ControllaIlRientro(form, 900, {"lblVersione"})

            End Using

        End Sub

        ''' <summary>
        ''' Porta la finestra alla larghezza chiesta e verifica che le scritte indicate
        ''' stiano dentro il pannello del logo, lasciando libero il pixel del contorno.
        ''' </summary>
        Private Shared Sub ControllaIlRientro(form As Form, larghezza As Integer, scritte As String())

            form.ClientSize = New Size(larghezza, 800)

            Dim pannello As Control =
                form.Controls.Find("pnlLogo", searchAllChildren:=True).Single()

            For Each nome As String In scritte

                Dim scritta As Control =
                    pannello.Controls.Find(nome, searchAllChildren:=False).Single()

                Assert.IsGreaterThanOrEqualTo(1, scritta.Left,
                    $"a {larghezza} px «{nome}» arriva sul filo di sinistra")
                Assert.IsLessThanOrEqualTo(pannello.Width - 1, scritta.Right,
                    $"a {larghezza} px «{nome}» arriva sul filo di destra")
                Assert.IsGreaterThanOrEqualTo(1, scritta.Top,
                    $"a {larghezza} px «{nome}» arriva sul filo di sopra")
                Assert.IsLessThanOrEqualTo(pannello.Height - 1, scritta.Bottom,
                    $"a {larghezza} px «{nome}» arriva sul filo di sotto")

            Next

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
