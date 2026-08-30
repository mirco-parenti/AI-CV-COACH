Imports System.Drawing
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi dello scudo che compare mentre l'AI lavora (cap. 03.8): la misura sullo
    ''' schermo e il disegno che gira.
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

        ''' <summary>Lo scudo sta in mezzo, e allo schermo giusto.</summary>
        ''' <remarks>
        ''' Il secondo monitor non è un capriccio: con due schermi il centro è quello dove
        ''' l'utente sta guardando, e un conto fatto sulle sole misure ci metterebbe lo
        ''' scudo sullo schermo di sinistra.
        ''' </remarks>
        <TestMethod>
        Public Sub LoScudoStaInMezzoAlloSchermoCheGliSiDa()

            Dim secondo As New Rectangle(1920, 0, 1920, 1080)
            Dim dove As Rectangle = ScudoDiCaricamento.RiquadroSulloSchermo(secondo)

            Assert.AreEqual(2880, dove.Left + dove.Width \ 2, "in mezzo per il lungo")
            Assert.AreEqual(540, dove.Top + dove.Height \ 2, "e per il largo")
            Assert.IsTrue(secondo.Contains(dove), "e tutto dentro il suo schermo")

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
        ' Attrezzi
        ' ==================================================================

        ''' <summary>La tela dipinta a un certo passo. Piccola: qui si contano i pixel.</summary>
        Private Shared Function Dipinta(passo As Integer) As Bitmap

            Dim misura As Size = ScudoDiCaricamento.MisuraSulloSchermo(New Size(600, 450))
            Dim tela As New Bitmap(misura.Width, misura.Height)

            Using disegno As Graphics = Graphics.FromImage(tela)
                disegno.Clear(Color.Transparent)
                ScudoDiCaricamento.Disegna(disegno, misura, passo)
            End Using

            Return tela

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
