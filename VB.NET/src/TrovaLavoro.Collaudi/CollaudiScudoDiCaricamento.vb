Imports System.Drawing
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi dello scudo che compare mentre l'AI lavora (cap. 03.8): la misura sullo
    ''' schermo, il disegno che gira e la barra che si riempie sotto di lui.
    ''' </summary>
    ''' <remarks>
    ''' Non aprono nessuna finestra e non vogliono la macchina: si disegna su un
    ''' <c>Bitmap</c>, che è tutto quello che serve per chiedersi se lo scudo ci sta, se è
    ''' storto e se la ruota gira. La finestra a strati che porta questo disegno sullo
    ''' schermo si guarda invece con gli occhi — è codice di Windows, non di logica.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiScudoDiCaricamento

        Private Shared ReadOnly Pieno As New Size(1920, 1080)

        <TestMethod>
        Public Sub LoScudoStaDentroIDueLimitiChiestiDaMirco()

            Dim misura As Size = ScudoDiCaricamento.MisuraSulloSchermo(Pieno)

            Assert.IsLessThanOrEqualTo(CInt(1920 * ScudoDiCaricamento.QuotaOrizzontale), misura.Width,
                                       "in orizzontale non passa i due decimi dello schermo")
            Assert.IsLessThanOrEqualTo(CInt(1080 * ScudoDiCaricamento.QuotaVerticale), misura.Height,
                                       "in verticale non passa i due sesti")

        End Sub

        ''' <summary>
        ''' Lo scudo non si stira per riempire il rettangolo: il marchio non si deforma.
        ''' </summary>
        ''' <remarks>
        ''' È la ragione per cui le due quote sono limiti e non misure. Su 1920 × 1080 il
        ''' rettangolo chiesto sarebbe 384 × 360 — più largo che alto — mentre lo scudo è
        ''' più alto che largo: riempirlo vorrebbe dire schiacciarlo di un quarto.
        ''' </remarks>
        <TestMethod>
        Public Sub LoScudoNonSiDeformaMai()

            Dim suo As Rectangle = LogoAviolab.ScudoDentroLaTela
            Dim giusto As Double = suo.Width / CDbl(suo.Height)

            For Each schermo As Size In {Pieno, New Size(1366, 768), New Size(3840, 1080),
                                         New Size(1000, 2000), New Size(1280, 1024)}

                Dim misura As Size = ScudoDiCaricamento.MisuraSulloSchermo(schermo)
                Dim venuto As Double = misura.Width / CDbl(misura.Height)

                Assert.IsLessThan(0.01, Math.Abs(venuto - giusto) / giusto,
                                  $"su {schermo.Width}×{schermo.Height} lo scudo è storto")

                Assert.IsLessThanOrEqualTo(CInt(schermo.Width * ScudoDiCaricamento.QuotaOrizzontale),
                                           misura.Width, "e non sfora in larghezza")
                Assert.IsLessThanOrEqualTo(CInt(schermo.Height * ScudoDiCaricamento.QuotaVerticale),
                                           misura.Height, "né in altezza")
            Next

        End Sub

        <TestMethod>
        Public Sub SuUnoSchermoStrettoEAltoComandaLaLarghezza()

            ' Due decimi di 1000 sono 200; due sesti di 2000 sarebbero 666, e non ci
            ' stanno: lo scudo si ferma a quel che permette la larghezza.
            Dim misura As Size = ScudoDiCaricamento.MisuraSulloSchermo(New Size(1000, 2000))

            Assert.AreEqual(200, misura.Width, "la larghezza è quella chiesta, tutta")
            Assert.IsLessThan(666, misura.Height, "e l'altezza è quella che ne consegue")

        End Sub

        <TestMethod>
        Public Sub SuUnoSchermoLargoEBassoComandaLAltezza()

            ' Due sesti di 1080 sono 360, e sono meno di quel che la larghezza
            ' concederebbe (768): è l'altezza a decidere.
            Dim misura As Size = ScudoDiCaricamento.MisuraSulloSchermo(New Size(3840, 1080))

            Assert.AreEqual(360, misura.Height, "l'altezza è quella chiesta, tutta")
            Assert.IsLessThan(768, misura.Width, "e la larghezza resta indietro")

        End Sub

        ''' <summary>Lo scudo sta in mezzo — un filo più su — e allo schermo giusto.</summary>
        ''' <remarks>
        ''' <para>Il secondo monitor non è un capriccio: con due schermi il centro è quello
        ''' dove l'utente sta guardando, e un conto fatto sulle sole misure ci metterebbe lo
        ''' scudo sullo schermo di sinistra.</para>
        ''' <para>In verticale il centro non è 540 ma <b>510</b>: il complesso sta
        ''' <see cref="ScudoDiCaricamento.AlzataInPixel">trenta pixel</see> più in alto del
        ''' centro, perché a video una figura appesa esattamente a metà sembra cadere in
        ''' basso <i>(chiesto da Mirco il 2026-08-31, guardandolo: venti, e altri dieci
        ''' quando il complesso si è allungato con la barra)</i>.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub LoScudoStaInMezzoAlloSchermoCheGliSiDa()

            Dim secondo As New Rectangle(1920, 0, 1920, 1080)
            Dim dove As Rectangle = ScudoDiCaricamento.RiquadroSulloSchermo(secondo)

            Assert.AreEqual(2880, dove.Left + dove.Width \ 2, "in mezzo per il lungo")
            Assert.AreEqual(540 - ScudoDiCaricamento.AlzataInPixel, dove.Top + dove.Height \ 2,
                            "e per il largo, meno l'alzata")
            Assert.IsTrue(secondo.Contains(dove), "e tutto dentro il suo schermo")

        End Sub

        ''' <summary>L'alzata è un'alzata: verso l'alto, e della misura giusta.</summary>
        ''' <remarks>
        ''' Da sola l'asserzione qui sopra passerebbe anche con lo scudo abbassato di venti
        ''' pixel, se il numero fosse scritto con lo stesso segno sbagliato in tutti e due i
        ''' posti. Qui il confronto è con il centro vero dello schermo, e il verso si
        ''' guarda per quello che è.
        ''' </remarks>
        <TestMethod>
        Public Sub LAlzataVaVersoLAlto()

            Dim schermo As New Rectangle(0, 0, 1920, 1080)
            Dim dove As Rectangle = ScudoDiCaricamento.RiquadroSulloSchermo(schermo)

            Dim centroDelloSchermo As Integer = 1080 \ 2
            Dim centroDelloScudo As Integer = dove.Top + dove.Height \ 2

            Assert.IsLessThan(centroDelloSchermo, centroDelloScudo, "sta più in alto del centro")
            Assert.AreEqual(ScudoDiCaricamento.AlzataInPixel, centroDelloSchermo - centroDelloScudo,
                            "e di esattamente l'alzata chiesta")

        End Sub

        <TestMethod>
        Public Sub SenzaSchermoNonSiDisegnaNiente()

            Assert.AreEqual(Size.Empty, ScudoDiCaricamento.MisuraSulloSchermo(New Size(0, 0)))
            Assert.AreEqual(Rectangle.Empty, ScudoDiCaricamento.RiquadroSulloSchermo(Rectangle.Empty))

        End Sub

        <TestMethod>
        Public Sub LoScudoSiVedeDavvero()

            Using tela As Bitmap = Dipinta(0)

                Assert.IsGreaterThan(tela.Width * tela.Height \ 4, PixelDipinti(tela),
                                     "più di un quarto della tela è disegnato: lo scudo c'è")

            End Using

        End Sub

        ''' <summary>
        ''' La ruota gira, e dopo un giro intero torna esattamente com'era.
        ''' </summary>
        ''' <remarks>
        ''' Le due metà si tengono per mano: la prima dice che qualcosa si muove, la
        ''' seconda che si muove <b>in tondo</b>. Senza la prima, un disegno immobile
        ''' passerebbe il collaudo del giro; senza la seconda, una ruota che scivola di
        ''' mezzo pallino a ogni giro sembrerebbe girare benissimo.
        ''' </remarks>
        <TestMethod>
        Public Sub LaRuotaGiraEDopoUnGiroTornaAlPuntoDiPartenza()

            Using fermo As Bitmap = Dipinta(0),
                  unoDopo As Bitmap = Dipinta(1),
                  giroIntero As Bitmap = Dipinta(ScudoDiCaricamento.Pallini)

                Assert.IsFalse(SonoUguali(fermo, unoDopo), "uno scatto dopo il disegno è cambiato")
                Assert.IsTrue(SonoUguali(fermo, giroIntero), "e dopo dodici è tornato quello di prima")

            End Using

        End Sub

        ' ==================================================================
        ' La barra che si riempie
        ' ==================================================================

        ''' <summary>
        ''' La barra si aggiunge sotto lo scudo, e non gliela toglie da sopra.
        ''' </summary>
        ''' <remarks>
        ''' È il difetto che questo pezzo poteva fare più facilmente: dare alla barra il
        ''' posto che era dello scudo invece di chiedere una tela più alta. Sarebbe passato
        ''' inosservato — lo scudo si sarebbe visto lo stesso, appena più basso e con la
        ''' ruota scentrata di qualche pixel — e nessun altro collaudo se ne accorgerebbe.
        ''' </remarks>
        <TestMethod>
        Public Sub LaBarraSiAggiungeSottoLoScudoESenzaRubargliNiente()

            For Each schermo As Size In {Pieno, New Size(1366, 768), New Size(1000, 2000)}

                Dim scudo As Size = ScudoDiCaricamento.MisuraSulloSchermo(schermo)
                Dim complesso As Size = ScudoDiCaricamento.MisuraDelComplesso(schermo)

                Assert.AreEqual(scudo.Width, complesso.Width,
                                "il complesso è largo quanto lo scudo")
                Assert.IsGreaterThan(scudo.Height, complesso.Height,
                                     "ed è più alto, perché sotto c'è la barra")
                Assert.AreEqual(scudo.Height, ScudoDiCaricamento.AltezzaDelloScudo(complesso),
                                "e lo scudo dentro di lui è rimasto alto uguale")

                ' Che fra i due ci sia dell'aria non lo diceva nessuno, e azzerando lo
                ' stacco il banco restava verde: la barra si sarebbe incollata al piede
                ' dello scudo — cioè sarebbe diventata parte del marchio — senza che
                ' niente se ne accorgesse. Trovato falsificando, il 2026-08-31; e la
                ' prima asserzione scritta per chiuderlo restò verde a sua volta, perché
                ' il pavimento di due pixel dello stacco bastava a contentarla.
                '
                ' La soglia è i tre quarti dello spessore, e il quarto che manca ha una
                ' storia: la prima versione chiedeva l'intero — «tanta aria quanto la
                ' barra è spessa» — e il giorno stesso Mirco ha voluto la barra di 22
                ' pixel contro i 21 dello stacco. Il collaudo diceva rosso, ma la cosa
                ' sbagliata era lui: la proprietà da difendere è che l'aria non
                ' scompaia, non che vinca il confronto per un pixel. Una soglia scritta
                ' più stretta di quel che difende boccia il lavoro buono, e chi la
                ' incontra impara ad allargarla invece di ascoltarla.
                Assert.IsGreaterThanOrEqualTo(
                    ScudoDiCaricamento.SpessoreDellaBarra(scudo.Width) * 3 \ 4,
                    ScudoDiCaricamento.DistaccoDellaBarra(scudo.Width),
                    "fra lo scudo e la barra non c'è più aria a sufficienza")

            Next

        End Sub

        ''' <summary>La barra è larga quanto lo scudo: né più corta né più lunga.</summary>
        ''' <remarks>
        ''' Chiesto così da Mirco il 2026-08-31 — «uguale identica alla larghezza dello
        ''' scudo, sicuramente non di meno» — ed è anche la ragione per cui la finestra è
        ''' larga quanto lo scudo e basta: la barra ci si appoggia dentro per intero.
        ''' </remarks>
        <TestMethod>
        Public Sub LaBarraELargaQuantoLoScudo()

            Using tela As Bitmap = DipintaSu(Grande, 0, 1.0)

                Dim riga As Color() = RigaDellaBarra(tela)
                Dim primo As Integer = -1
                Dim ultimo As Integer = -1

                For x As Integer = 0 To riga.Length - 1
                    If riga(x).A > 0 Then
                        If primo < 0 Then primo = x
                        ultimo = x
                    End If
                Next

                Assert.AreEqual(0, primo, "la barra comincia al bordo sinistro dello scudo")
                Assert.AreEqual(riga.Length - 1, ultimo, "e finisce a quello destro")

            End Using

        End Sub

        ''' <summary>Prima che l'attesa cominci non c'è niente da mostrare.</summary>
        <TestMethod>
        Public Sub PrimaDiCominciareLaBarraEVuota()

            Assert.AreEqual(0.0, ScudoDiCaricamento.Riempimento(TimeSpan.Zero))
            Assert.AreEqual(0.0, ScudoDiCaricamento.Riempimento(TimeSpan.FromSeconds(-3)),
                            "e un tempo all'indietro non la riempie di certo")

        End Sub

        ''' <summary>
        ''' La barra cresce sempre, e non torna mai indietro.
        ''' </summary>
        ''' <remarks>
        ''' Una barra che indietreggia è peggio di nessuna barra: dice che il lavoro fatto
        ''' è stato disfatto, che qui non succede mai.
        ''' </remarks>
        <TestMethod>
        Public Sub LaBarraCresceESoloInAvanti()

            Dim prima As Double = 0.0

            For secondi As Integer = 1 To 300

                Dim adesso As Double = ScudoDiCaricamento.Riempimento(TimeSpan.FromSeconds(secondi))

                Assert.IsGreaterThan(prima, adesso,
                                     $"al secondo {secondi} la barra non è andata avanti")
                prima = adesso

            Next

        End Sub

        ''' <summary>
        ''' Da sola non arriva mai in fondo: l'ultimo pezzo lo riempie la risposta dell'AI.
        ''' </summary>
        ''' <remarks>
        ''' Il collaudo guarda anche il caso assurdo — un'attesa di un'ora — perché è lì
        ''' che una curva sbagliata si tradirebbe: il 95% dev'essere un tetto, non un
        ''' traguardo che prima o poi si taglia.
        ''' </remarks>
        <TestMethod>
        Public Sub LaBarraNonArrivaMaiInFondoDaSola()

            For Each secondi As Double In {1.0, 30.0, 120.0, 600.0, 3600.0}

                Assert.IsLessThan(1.0, ScudoDiCaricamento.Riempimento(TimeSpan.FromSeconds(secondi)),
                                  $"dopo {secondi} secondi la barra si è già dichiarata finita")
                Assert.IsLessThanOrEqualTo(ScudoDiCaricamento.RiempimentoMassimo,
                                           ScudoDiCaricamento.Riempimento(TimeSpan.FromSeconds(secondi)),
                                           $"dopo {secondi} secondi ha passato il suo tetto")

            Next

        End Sub

        ''' <summary>
        ''' La curva è quella che Mirco ha scelto guardandola scritta in numeri.
        ''' </summary>
        ''' <remarks>
        ''' Sono i quattro punti che gli sono stati messi davanti il 2026-08-31, con le
        ''' attese vere dell'applicazione accanto: cinque secondi (un'analisi appena
        ''' cominciata), quindici, trentacinque (un confronto) e sessanta (un CV con la
        ''' lettera). Se un domani si tocca la forma della curva, questo collaudo dice
        ''' subito se si è toccato anche quel che era stato deciso.
        ''' </remarks>
        <TestMethod>
        Public Sub IlRiempimentoSegueLaCurvaSceltaDaMirco()

            Dim attesi As New Dictionary(Of Integer, Double) From {
                {5, 0.32}, {15, 0.62}, {35, 0.84}, {60, 0.92}}

            For Each punto As KeyValuePair(Of Integer, Double) In attesi

                Dim venuto As Double = ScudoDiCaricamento.Riempimento(
                    TimeSpan.FromSeconds(punto.Key))

                Assert.IsLessThan(0.02, Math.Abs(venuto - punto.Value),
                                  $"a {punto.Key} secondi la barra è al {venuto:P0} " &
                                  $"invece che al {punto.Value:P0}")

            Next

        End Sub

        ''' <summary>Quel che è dipinto è lungo quanto la quota dice.</summary>
        ''' <remarks>
        ''' Qui si contano i pixel verdi di una riga vera, non si rifà il conto: fra la
        ''' quota e quel che si vede a video ci sono due arrotondamenti e un filetto di
        ''' bordo, ed è proprio lì che un pezzo di barra si perde.
        ''' </remarks>
        <TestMethod>
        Public Sub LaBarraDipintaELungaQuantoLaQuotaDice()

            For Each quota As Double In {0.0, 0.25, 0.5, 0.95, 1.0}

                Using tela As Bitmap = DipintaSu(Grande, 0, quota)

                    Dim riga As Color() = RigaDellaBarra(tela)
                    Dim verdi As Integer = QuantiVerdi(riga)
                    Dim atteso As Double = (riga.Length - 2) * quota

                    Assert.IsLessThanOrEqualTo(2.0, Math.Abs(verdi - atteso),
                                               $"a quota {quota:P0} sono verdi {verdi} pixel " &
                                               $"su {riga.Length - 2}, non {atteso:F0}")

                End Using

            Next

        End Sub

        ''' <summary>
        ''' Il verde è quello campionato sulla barra vera, e la punta è più chiara.
        ''' </summary>
        ''' <remarks>
        ''' Sono le due cose che Mirco ha chiesto guardando l'immagine: il colore
        ''' «identico», e quello schiarirsi verso la punta che sembra un effetto
        ''' fluorescente. Il corpo si guarda lontano dalla testa, dove la sfumatura non
        ''' arriva ancora.
        ''' </remarks>
        <TestMethod>
        Public Sub IlVerdeEQuelloCampionatoELaPuntaEPiuChiara()

            Using tela As Bitmap = DipintaSu(Grande, 0, 0.9)

                Dim riga As Color() = RigaDellaBarra(tela)
                Dim ultimoVerde As Integer = 0

                For x As Integer = 0 To riga.Length - 1
                    If EVerde(riga(x)) Then ultimoVerde = x
                Next

                Dim corpo As Color = riga(2)
                Dim punta As Color = riga(ultimoVerde)

                Assert.AreEqual(StileApp.VerdeDiAttesa.ToArgb(), corpo.ToArgb(),
                                "il corpo della barra non è il verde campionato")
                Assert.IsGreaterThan(corpo.GetBrightness(), punta.GetBrightness(),
                                     "la punta non è più chiara del corpo")

            End Using

        End Sub

        ''' <summary>
        ''' A barra vuota il posto della barra c'è lo stesso, e non ci arriva lo scudo.
        ''' </summary>
        ''' <remarks>
        ''' Due cose in una. Che la barra vuota si veda comunque — altrimenti nei primi
        ''' istanti d'attesa comparirebbe dal nulla — e che il disegno dello scudo si
        ''' fermi dove deve: se sfondasse in basso, in quella riga ci sarebbero pixel del
        ''' marchio invece del grigio.
        ''' </remarks>
        <TestMethod>
        Public Sub ABarraVuotaRestaIlSuoPostoGrigio()

            Using tela As Bitmap = DipintaSu(Grande, 0, 0.0)

                Dim riga As Color() = RigaDellaBarra(tela)

                ' Dal secondo al penultimo: il primo e l'ultimo sono il filetto laterale,
                ' che è grigio anche lui ma di un altro grigio.
                For x As Integer = 1 To riga.Length - 2
                    Assert.AreEqual(StileApp.FondoDiAttesa.ToArgb(), riga(x).ToArgb(),
                                    $"a barra vuota il pixel {x} non è del suo grigio")
                Next

            End Using

        End Sub

        ' ==================================================================
        ' Attrezzi
        ' ==================================================================

        ''' <summary>Uno schermo su cui la barra è alta abbastanza da contarne i pixel.</summary>
        Private Shared ReadOnly Grande As New Size(1200, 900)

        ''' <summary>La tela dipinta a un certo passo. Piccola: qui si contano i pixel.</summary>
        Private Shared Function Dipinta(passo As Integer) As Bitmap
            Return DipintaSu(New Size(600, 450), passo, 0.0)
        End Function

        ''' <summary>La tela del complesso — scudo, ruota e barra — su un certo schermo.</summary>
        Private Shared Function DipintaSu(schermo As Size, passo As Integer,
                                          quotaPiena As Double) As Bitmap

            Dim misura As Size = ScudoDiCaricamento.MisuraDelComplesso(schermo)
            Dim tela As New Bitmap(misura.Width, misura.Height)

            Using disegno As Graphics = Graphics.FromImage(tela)
                disegno.Clear(Color.Transparent)
                ScudoDiCaricamento.Disegna(disegno, misura, passo, quotaPiena)
            End Using

            Return tela

        End Function

        ''' <summary>
        ''' La riga di pixel a metà altezza della barra, dal primo all'ultimo.
        ''' </summary>
        ''' <remarks>
        ''' A metà altezza e non altrove perché la barra ha un filetto di bordo sopra e
        ''' sotto: una riga presa sul bordo direbbe grigio anche a barra piena.
        ''' </remarks>
        Private Shared Function RigaDellaBarra(tela As Bitmap) As Color()

            Dim spessore As Integer = ScudoDiCaricamento.SpessoreDellaBarra(tela.Width)
            Dim y As Integer = tela.Height - spessore + spessore \ 2
            Dim riga(tela.Width - 1) As Color

            For x As Integer = 0 To tela.Width - 1
                riga(x) = tela.GetPixel(x, y)
            Next

            Return riga

        End Function

        ''' <summary>Se un pixel è del verde della barra, comunque sfumato.</summary>
        Private Shared Function EVerde(colore As Color) As Boolean
            Return colore.G > colore.R AndAlso colore.G > colore.B AndAlso colore.G > 60
        End Function

        Private Shared Function QuantiVerdi(riga As Color()) As Integer

            Dim quanti As Integer = 0

            For Each colore As Color In riga
                If EVerde(colore) Then quanti += 1
            Next

            Return quanti

        End Function

        Private Shared Function PixelDipinti(tela As Bitmap) As Integer

            Dim quanti As Integer = 0

            For y As Integer = 0 To tela.Height - 1
                For x As Integer = 0 To tela.Width - 1
                    If tela.GetPixel(x, y).A > 0 Then quanti += 1
                Next
            Next

            Return quanti

        End Function

        Private Shared Function SonoUguali(una As Bitmap, altra As Bitmap) As Boolean

            If una.Size <> altra.Size Then Return False

            For y As Integer = 0 To una.Height - 1
                For x As Integer = 0 To una.Width - 1
                    If una.GetPixel(x, y) <> altra.GetPixel(x, y) Then Return False
                Next
            Next

            Return True

        End Function

    End Class

End Namespace
