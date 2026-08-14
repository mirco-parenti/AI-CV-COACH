Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Collaudi della mappa della cartella dati (cap. 11.1). Due cose da tenere ferme:
    ''' i percorsi discendono tutti da una sola radice, e la radice si può scavalcare —
    ''' senza questo, un collaudo che scrive andrebbe a toccare la cartella vera
    ''' dell'utente.
    ''' </summary>
    <TestClass>
    Public Class CollaudiCartellaDati

        <TestMethod>
        Public Sub LaRadicePredefinitaStaSottoAppData()
            ' «Default: %APPDATA%\TrovaLavoro» (cap. 11.1).
            Dim attesa As String = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "TrovaLavoro")

            Assert.AreEqual(attesa, CartellaDati.RadicePredefinita, "radice predefinita")
            Assert.AreEqual(attesa, CartellaDati.Predefinita().Radice, "radice della mappa predefinita")
        End Sub

        <TestMethod>
        Public Sub IPercorsiSeguonoLaMappaDelCapitolo11()
            ' L'albero del cap. 11.1: i due file dei numeri in radice, il profilo e il
            ' suo storico nella sottocartella «profilo».
            Dim cartella As New CartellaDati(Path.Combine(Path.GetTempPath(), "cartella-dati-finta"))
            Dim radice As String = cartella.Radice

            Assert.AreEqual(Path.Combine(radice, "taratura.json"), cartella.FileTaratura, "taratura.json")
            Assert.AreEqual(Path.Combine(radice, "modelli.json"), cartella.FileModelli, "modelli.json")
            Assert.AreEqual(Path.Combine(radice, "profilo"), cartella.CartellaProfilo, "profilo\")
            Assert.AreEqual(Path.Combine(radice, "profilo", "profilo.json"), cartella.FileProfilo, "profilo.json")
            Assert.AreEqual(Path.Combine(radice, "profilo", "storico"), cartella.CartellaStorico, "storico\")
        End Sub

        <TestMethod>
        Public Sub LaRadiceSiPuoScavalcare()
            ' È la porta che serve ai collaudi che scrivono: la %APPDATA% vera non si
            ' tocca mai. Ogni percorso deve stare sotto la radice data.
            Dim scelta As String = Path.Combine(Path.GetTempPath(), "cartella-dati-scelta")
            Dim cartella As New CartellaDati(scelta)

            Assert.AreNotEqual(CartellaDati.RadicePredefinita, cartella.Radice, "non deve ricadere sul default")
            For Each percorso As String In {cartella.FileTaratura, cartella.FileModelli,
                                            cartella.CartellaProfilo, cartella.FileProfilo,
                                            cartella.CartellaStorico}
                Assert.StartsWith(cartella.Radice, percorso, "ogni percorso sta sotto la radice")
            Next
        End Sub

        <TestMethod>
        Public Sub LaRadiceDiSempreSiRiconosce()
            ' Da qui dipende se l'applicazione dichiara nel titolo di lavorare altrove
            ' (cap. 11.1): dirlo quando non serve stancherebbe, non dirlo quando serve
            ' farebbe scambiare una cartella di prova per quella dei dati veri.
            Assert.IsTrue(CartellaDati.Predefinita().SullaRadicePredefinita, "la predefinita è sé stessa")

            Dim altrove As New CartellaDati(Path.Combine(Path.GetTempPath(), "cartella-dati-altrove"))
            Assert.IsFalse(altrove.SullaRadicePredefinita, "una cartella di prova non è quella di sempre")
        End Sub

        <TestMethod>
        Public Sub LaBarraFinaleNonFaUnAltraCartella()
            ' «…\TrovaLavoro» e «…\TrovaLavoro\» sono la stessa cartella ma non la stessa
            ' stringa: senza normalizzare, avviare sulla propria cartella scrivendola con
            ' la barra finale farebbe comparire l'avviso «stai lavorando altrove».
            Dim conBarra As New CartellaDati(CartellaDati.RadicePredefinita & Path.DirectorySeparatorChar)

            Assert.IsTrue(conBarra.SullaRadicePredefinita, "è sempre lei")
        End Sub

        <TestMethod>
        Public Sub UnaRadiceVuotaVieneRifiutata()
            ' Una radice vuota produrrebbe percorsi relativi alla cartella di lavoro:
            ' meglio l'errore subito che file dell'utente sparsi accanto all'eseguibile.
            Assert.Throws(Of ArgumentException)(
                Sub()
                    Dim inutile As New CartellaDati("   ")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub AssicuraCreaLeCartelleEdEIndolore()
            ' Chi scrive un file chiama Assicura e non si preoccupa d'altro; ripeterlo
            ' su cartelle già esistenti non deve essere un errore.
            Dim radice As String = Path.Combine(Path.GetTempPath(), "cartella-dati-" & Guid.NewGuid().ToString("N"))
            Dim cartella As New CartellaDati(radice)
            Try
                cartella.Assicura()

                Assert.IsTrue(Directory.Exists(cartella.Radice), "la radice")
                Assert.IsTrue(Directory.Exists(cartella.CartellaProfilo), "profilo\")
                Assert.IsTrue(Directory.Exists(cartella.CartellaStorico), "profilo\storico\")

                cartella.Assicura()
                Assert.IsTrue(Directory.Exists(cartella.CartellaStorico), "ripetibile senza danno")
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub TaraturaEModelliLeggonoDallaStessaCartella()
            ' È il collaudo che giustifica la rifattorizzazione: finché i due percorsi
            ' erano ricopiati a mano, nulla impediva che divergessero in silenzio.
            Dim predefinita As CartellaDati = CartellaDati.Predefinita()

            Assert.AreEqual(predefinita.FileTaratura, Taratura.PercorsoPredefinito, "taratura.json")
            Assert.AreEqual(predefinita.FileModelli, Modelli.PercorsoPredefinito, "modelli.json")
        End Sub

    End Class

End Namespace
