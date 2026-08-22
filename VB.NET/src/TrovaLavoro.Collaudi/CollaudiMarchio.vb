Imports System.Drawing
Imports System.IO
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

        <TestMethod>
        Public Sub LaSchermataDiAvvioEDentroLEseguibile()

            Dim immagine As Image = Marchio.SchermataDiAvvio

            Assert.IsNotNull(immagine, "la schermata di avvio è incorporata")
            Assert.AreEqual(800, immagine.Width, "larghezza come disegnata")
            Assert.AreEqual(648, immagine.Height, "altezza come disegnata")

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

            Dim misura As Size = FinestraAvvio.MisuraDaMostrare(New Size(800, 648), New Size(1920, 1080))

            Assert.AreEqual(New Size(800, 648), misura, "sta comoda: non si tocca")

        End Sub

        <TestMethod>
        Public Sub SuUnoSchermoPiccoloSiRimpicciolisceSenzaDeformarsi()

            ' 1024x768: 800 di larghezza sono già oltre la quota concessa.
            Dim misura As Size = FinestraAvvio.MisuraDaMostrare(New Size(800, 648), New Size(1024, 768))

            Assert.IsLessThan(800, misura.Width, "si è ridotta")
            Assert.IsLessThanOrEqualTo(CInt(1024 * FinestraAvvio.QuotaSchermo), misura.Width,
                                       "sta nella quota di schermo concessa")
            Assert.IsLessThanOrEqualTo(CInt(768 * FinestraAvvio.QuotaSchermo), misura.Height,
                                       "in altezza come in larghezza")

            Dim proporzioneOriginale As Double = 800.0 / 648.0
            Dim proporzioneRidotta As Double = misura.Width / CDbl(misura.Height)
            Assert.AreEqual(proporzioneOriginale, proporzioneRidotta, 0.01,
                            "le proporzioni non si toccano: l'immagine non si schiaccia")

        End Sub

        <TestMethod>
        Public Sub SenzaUnoSchermoLaMisuraRestaQuellaDellImmagine()

            Assert.AreEqual(New Size(800, 648),
                            FinestraAvvio.MisuraDaMostrare(New Size(800, 648), Size.Empty),
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
