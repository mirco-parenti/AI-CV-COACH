Imports System.Drawing
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi dell'indicatore che compare mentre l'AI lavora (cap. 03.8): la misura
    ''' sullo schermo, la ruota che gira e la barra che si riempie sotto di lei.
    ''' </summary>
    ''' <remarks>
    ''' Non aprono nessuna finestra e non vogliono la macchina: si disegna su un
    ''' <c>Bitmap</c>, che è tutto quello che serve per chiedersi se l'indicatore ci sta,
    ''' dove sta e se la ruota gira. La finestra a strati che porta questo disegno sullo
    ''' schermo si guarda invece con gli occhi — è codice di Windows, non di logica.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiScudoDiCaricamento

        Private Shared ReadOnly Pieno As New Size(1920, 1080)

        ''' <summary>
        ''' Adesso i due limiti valgono per <b>tutto</b> quel che si vede, barra compresa.
        ''' </summary>
        ''' <remarks>
        ''' Finché sotto la ruota c'era lo stemma, i limiti misuravano lui e il complesso
        ''' li sforava: la barra si aggiungeva dopo, e su uno schermo comune il disegno
        ''' veniva alto 397 px contro i 360 dichiarati. Nessun collaudo se ne accorgeva,
        ''' perché tutti guardavano la misura dello stemma. Qui si guarda il complesso, che
        ''' è la cosa che occupa lo schermo.
        ''' </remarks>
        <TestMethod>
        Public Sub LIndicatoreStaDentroIDueLimitiChiestiDaMirco()

            For Each schermo As Size In {Pieno, New Size(1366, 768), New Size(3840, 1080),
                                         New Size(1000, 2000), New Size(1280, 1024)}

                Dim misura As Size = ScudoDiCaricamento.MisuraDelComplesso(schermo)

                Assert.IsLessThanOrEqualTo(CInt(schermo.Width * ScudoDiCaricamento.QuotaOrizzontale),
                                           misura.Width,
                                           $"su {schermo.Width}×{schermo.Height} passa i due decimi in larghezza")
                Assert.IsLessThanOrEqualTo(CInt(schermo.Height * ScudoDiCaricamento.QuotaVerticale),
                                           misura.Height,
                                           $"su {schermo.Width}×{schermo.Height} passa il quarto in altezza")

            Next

        End Sub

        <TestMethod>
        Public Sub SuUnoSchermoStrettoEAltoComandaLaLarghezza()

            ' Due decimi di 1000 sono 200; un quarto di 2000 lascerebbe passare un
            ' indicatore largo il triplo: si ferma a quel che permette la larghezza.
            Dim misura As Size = ScudoDiCaricamento.MisuraDelComplesso(New Size(1000, 2000))

            Assert.AreEqual(200, misura.Width, "la larghezza è quella chiesta, tutta")
            Assert.IsLessThan(500, misura.Height, "e l'altezza resta molto sotto il suo limite")

        End Sub

        <TestMethod>
        Public Sub SuUnoSchermoLargoEBassoComandaLAltezza()

            ' Un quarto di 1080 sono 270, e sono meno di quel che la larghezza
            ' concederebbe (768): è l'altezza a decidere.
            Dim misura As Size = ScudoDiCaricamento.MisuraDelComplesso(New Size(3840, 1080))

            Assert.IsLessThanOrEqualTo(270, misura.Height, "l'altezza chiesta non si sfora")
            Assert.IsGreaterThan(265, misura.Height, "ma ci si arriva vicino: è lei a comandare")
            Assert.IsLessThan(768, misura.Width, "e la larghezza resta indietro")

        End Sub

        ''' <summary>L'indicatore sta in mezzo — un filo più su — e allo schermo giusto.</summary>
        ''' <remarks>
        ''' <para>Il secondo monitor non è un capriccio: con due schermi il centro è quello
        ''' dove l'utente sta guardando, e un conto fatto sulle sole misure ci metterebbe
        ''' l'indicatore sullo schermo di sinistra.</para>
        ''' <para>In verticale il centro non è 540 ma <b>520</b>: il complesso sta
        ''' <see cref="ScudoDiCaricamento.AlzataInPixel">venti pixel</see> più in alto del
        ''' centro, perché a video una figura appesa esattamente a metà sembra cadere in
        ''' basso. Erano trenta finché il complesso portava anche lo stemma ed era alto 397;
        ''' tolto quello è alto 269, e la stessa frazione della sua altezza fa venti.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub LIndicatoreStaInMezzoAlloSchermoCheGliSiDa()

            Dim secondo As New Rectangle(1920, 0, 1920, 1080)
            Dim dove As Rectangle = ScudoDiCaricamento.RiquadroSulloSchermo(secondo)

            Assert.AreEqual(2880, dove.Left + dove.Width \ 2, "in mezzo per il lungo")
            Assert.AreEqual(540 - ScudoDiCaricamento.AlzataInPixel, dove.Top + dove.Height \ 2,
                            "e per il largo, meno l'alzata")
            Assert.IsTrue(secondo.Contains(dove), "e tutto dentro il suo schermo")

        End Sub

        ''' <summary>L'alzata è un'alzata: verso l'alto, e della misura giusta.</summary>
        ''' <remarks>
        ''' Da sola l'asserzione qui sopra passerebbe anche con l'indicatore abbassato di
        ''' venti pixel, se il numero fosse scritto con lo stesso segno sbagliato in tutti e
        ''' due i posti. Qui il confronto è con il centro vero dello schermo, e il verso si
        ''' guarda per quello che è.
        ''' </remarks>
        <TestMethod>
        Public Sub LAlzataVaVersoLAlto()

            Dim schermo As New Rectangle(0, 0, 1920, 1080)
            Dim dove As Rectangle = ScudoDiCaricamento.RiquadroSulloSchermo(schermo)

            Dim centroDelloSchermo As Integer = 1080 \ 2
            Dim centroDellIndicatore As Integer = dove.Top + dove.Height \ 2

            Assert.IsLessThan(centroDelloSchermo, centroDellIndicatore, "sta più in alto del centro")
            Assert.AreEqual(ScudoDiCaricamento.AlzataInPixel, centroDelloSchermo - centroDellIndicatore,
                            "e di esattamente l'alzata chiesta")

        End Sub

        <TestMethod>
        Public Sub SenzaSchermoNonSiDisegnaNiente()

            Assert.AreEqual(0, ScudoDiCaricamento.LarghezzaSulloSchermo(New Size(0, 0)))
            Assert.AreEqual(Size.Empty, ScudoDiCaricamento.MisuraDelComplesso(New Size(0, 0)))
            Assert.AreEqual(Rectangle.Empty, ScudoDiCaricamento.RiquadroSulloSchermo(Rectangle.Empty))

        End Sub

        ''' <summary>
        ''' La ruota si vede, e riempie la fetta di tela che le è stata data.
        ''' </summary>
        ''' <remarks>
        ''' <para>È il collaudo che difende il conto rifatto il 2026-09-01. Finché sotto la
        ''' ruota c'era lo stemma, la sua fetta era alta quanto <b>lui</b> e la ruota ci
        ''' nuotava dentro: è larga poco più di due terzi, quindi restava un dito di vuoto
        ''' sopra e sotto che lo stemma riempiva. Tolto lo stemma quel vuoto sarebbe rimasto
        ''' vuoto, e la barra sarebbe sembrata scivolata via da sola.</para>
        ''' <para>Si guarda tutto quel che sta <b>sopra la barra</b> e non la sola fetta:
        ''' cercare la ruota dove ci si aspetta che sia direbbe soltanto che lì c'è
        ''' qualcosa. Rimettendo la fetta alta com'era, la ruota comincia sessanta pixel
        ''' più in basso e questo collaudo diventa rosso — provato.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub LaRuotaRiempieLaSuaFettaDiTela()

            ' Dichiarata qui e non presa dalla classe: un metro copiato dalla cosa da
            ' misurare non può dire che è sbagliata. La ruota è larga il suo raggio più il
            ' pallino, per due — e siccome è tonda, è alta altrettanto.
            Const QuotaAttesa As Double = (0.3 + 0.055) * 2.0

            Using tela As Bitmap = DipintaSu(Grande, 0, 0.0)

                Dim alta As Integer = CInt(Math.Round(tela.Width * QuotaAttesa))
                Dim sopraLaBarra As Integer =
                    tela.Height - ScudoDiCaricamento.SpessoreDellaBarra(tela.Width) - 1

                Dim cima As Integer = Integer.MaxValue
                Dim fondo As Integer = -1

                For y As Integer = 0 To sopraLaBarra
                    For x As Integer = 0 To tela.Width - 1
                        If tela.GetPixel(x, y).A > 0 Then
                            If y < cima Then cima = y
                            fondo = y
                        End If
                    Next
                Next

                Assert.IsGreaterThan(-1, fondo, "sopra la barra la ruota si vede")
                Assert.IsLessThanOrEqualTo(1, cima,
                                           $"la ruota comincia a {cima} invece che in cima alla tela")
                Assert.IsGreaterThanOrEqualTo(alta * 9 \ 10, fondo,
                                              $"e finisce a {fondo}, ben prima dei {alta} della sua fetta")

            End Using

        End Sub

        ''' <summary>
        ''' Dietro la ruota non c'è più il marchio: la sua fetta è quasi tutta vuota.
        ''' </summary>
        ''' <remarks>
        ''' È il rovescio di un collaudo che c'era: fino al 2026-09-01 diceva che più di un
        ''' quarto della tela era dipinto, ed era il modo di sapere che lo stemma c'era.
        ''' Adesso difende il contrario, ed è la stessa proprietà guardata dall'altra parte:
        ''' dodici pallini su una tela grande lasciano scoperto quasi tutto, un marchio alto
        ''' quanto la fetta la riempirebbe per metà. Rimettendolo, questo diventa rosso.
        ''' </remarks>
        <TestMethod>
        Public Sub DietroLaRuotaNonCePiuIlMarchio()

            Using tela As Bitmap = DipintaSu(Grande, 0, 0.0)

                Dim alta As Integer = ScudoDiCaricamento.AltezzaDellaRuota(tela.Width)
                Dim dipinti As Integer = 0

                For y As Integer = 0 To alta - 1
                    For x As Integer = 0 To tela.Width - 1
                        If tela.GetPixel(x, y).A > 0 Then dipinti += 1
                    Next
                Next

                Assert.IsGreaterThan(0, dipinti, "i pallini ci sono")
                Assert.IsLessThan(tela.Width * alta \ 4, dipinti,
                                  "dietro la ruota c'è qualcosa di grande: il marchio è tornato")

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
        ''' La barra si aggiunge sotto la ruota, e non le toglie niente da sopra.
        ''' </summary>
        ''' <remarks>
        ''' È il difetto che questo pezzo poteva fare più facilmente: dare alla barra il
        ''' posto che era della ruota invece di chiedere una tela più alta. Sarebbe passato
        ''' inosservato — la ruota si vedrebbe lo stesso, appena scentrata — e nessun altro
        ''' collaudo se ne accorgerebbe.
        ''' </remarks>
        <TestMethod>
        Public Sub LaBarraSiAggiungeSottoLaRuotaESenzaRubargliNiente()

            ' Dichiarata qui e non presa dalla classe: il raggio più il pallino, per due.
            Const QuotaDellaRuotaAttesa As Double = (0.3 + 0.055) * 2.0

            For Each schermo As Size In {Pieno, New Size(1366, 768), New Size(1000, 2000)}

                Dim complesso As Size = ScudoDiCaricamento.MisuraDelComplesso(schermo)
                Dim ruota As Integer = ScudoDiCaricamento.AltezzaDellaRuota(complesso.Width)
                Dim spessore As Integer = ScudoDiCaricamento.SpessoreDellaBarra(complesso.Width)
                Dim distacco As Integer = ScudoDiCaricamento.DistaccoDellaBarra(complesso.Width)

                Assert.AreEqual(CInt(Math.Round(complesso.Width * QuotaDellaRuotaAttesa)), ruota,
                                $"su {schermo.Width}×{schermo.Height} la ruota non è alta quanto è larga")
                Assert.AreEqual(complesso.Height, ruota + distacco + spessore,
                                "il complesso è la ruota più l'aria più la barra: niente avanza e niente si sovrappone")

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
                Assert.IsGreaterThanOrEqualTo(spessore * 3 \ 4, distacco,
                                              "fra la ruota e la barra non c'è più aria a sufficienza")

            Next

        End Sub

        ''' <summary>
        ''' Fra il piede della ruota e la barra l'aria si vede davvero.
        ''' </summary>
        ''' <remarks>
        ''' L'asserzione qui sopra confronta due numeri, e due numeri d'accordo fra loro non
        ''' dicono che a video ci sia dello spazio: lo direbbero anche se la ruota fosse
        ''' disegnata più in basso della sua fetta. Qui si risale dalla barra finché si
        ''' incontra il primo pixel dipinto — il piede della ruota — e si misura il vuoto
        ''' che resta in mezzo.
        ''' </remarks>
        <TestMethod>
        Public Sub FraLaRuotaELaBarraLAriaSiVede()

            Using tela As Bitmap = DipintaSu(Grande, 0, 1.0)

                Dim spessore As Integer = ScudoDiCaricamento.SpessoreDellaBarra(tela.Width)
                Dim cimaDellaBarra As Integer = tela.Height - spessore

                Dim piede As Integer = -1

                For y As Integer = cimaDellaBarra - 1 To 0 Step -1
                    If RigaDipinta(tela, y) Then
                        piede = y
                        Exit For
                    End If
                Next

                Assert.IsGreaterThan(-1, piede, "sopra la barra la ruota c'è")
                Assert.IsGreaterThanOrEqualTo(spessore * 3 \ 4, cimaDellaBarra - piede - 1,
                                              "fra il piede della ruota e la barra l'aria è sparita")

            End Using

        End Sub

        ''' <summary>La barra è larga quanto tutto l'indicatore: né più corta né più lunga.</summary>
        ''' <remarks>
        ''' Chiesto così da Mirco il 2026-08-31 — «uguale identica alla larghezza dello
        ''' scudo, sicuramente non di meno» — ed è anche la ragione per cui la finestra è
        ''' larga quanto la barra e basta: lei ci si appoggia dentro per intero, e la ruota,
        ''' che è più stretta, ci sta comoda al centro.
        ''' </remarks>
        <TestMethod>
        Public Sub LaBarraELargaQuantoTuttoLIndicatore()

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

                Assert.AreEqual(0, primo, "la barra comincia al bordo sinistro della tela")
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

        ''' <summary>La tela del complesso — la ruota e la barra — su un certo schermo.</summary>
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

        ''' <summary>Se in quella riga della tela c'è almeno un pixel dipinto.</summary>
        Private Shared Function RigaDipinta(tela As Bitmap, y As Integer) As Boolean

            For x As Integer = 0 To tela.Width - 1
                If tela.GetPixel(x, y).A > 0 Then Return True
            Next

            Return False

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
