Imports System.IO
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi della chiave API cifrata (cap. 11.3). Le cose che qui possono rompersi
    ''' davvero sono tre: che la chiave si riprenda uguale a com'era stata data; che un
    ''' file guasto <b>non</b> faccia cadere l'avvio, perché senza chiave l'applicazione
    ''' si apre lo stesso; e che sul disco non ci finisca in chiaro, che è la ragione per
    ''' cui questa classe esiste.
    ''' </summary>
    ''' <remarks>
    ''' La cifratura è quella di Windows legata all'utente corrente: il banco gira con
    ''' l'account di chi lo lancia, quindi salvare e rileggere nella stessa sessione è
    ''' esattamente il caso vero. Il caso «altro utente» non si può inscenare qui, ma ha
    ''' la stessa forma di un file guasto — DPAPI rifiuta un blob che non è suo — ed è
    ''' quello che si collauda.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiArchivioSegreti

        Private Const ChiaveFinta As String = "sk-ant-collaudo-non-vera-0000-9876"

        Private Shared Function CartellaTemporanea() As CartellaDati
            Return New CartellaDati(Path.Combine(Path.GetTempPath(), "segreti-" & Guid.NewGuid().ToString("N")))
        End Function

        Private Shared Sub Pulisci(cartella As CartellaDati)
            If Directory.Exists(cartella.Radice) Then Directory.Delete(cartella.Radice, recursive:=True)
        End Sub

        <TestMethod>
        Public Sub LaChiaveSalvataSiRilegge()
            Dim cartella As CartellaDati = CartellaTemporanea()
            Try
                Dim archivio As New ArchivioSegreti(cartella)
                Assert.IsFalse(archivio.Esiste, "prima non c'è niente")

                archivio.SalvaChiaveApi(ChiaveFinta)

                Dim illeggibile As Boolean
                Assert.IsTrue(archivio.Esiste, "adesso il file c'è")
                Assert.AreEqual(ChiaveFinta, archivio.LeggiChiaveApi(illeggibile), "e torna com'era")
                Assert.IsFalse(illeggibile, "senza intoppi")
            Finally
                Pulisci(cartella)
            End Try
        End Sub

        <TestMethod>
        Public Sub SulDiscoLaChiaveNonSiLegge()
            ' È il punto di tutto il capitolo 11.3: chi apre segreti.bin con un editor non
            ' deve trovarci la chiave. Si controlla sia nei byte grezzi sia nel testo,
            ' perché una scrittura non cifrata si vedrebbe in entrambi.
            Dim cartella As CartellaDati = CartellaTemporanea()
            Try
                Dim archivio As New ArchivioSegreti(cartella)
                archivio.SalvaChiaveApi(ChiaveFinta)

                Dim cifrato As Byte() = File.ReadAllBytes(cartella.FileSegreti)
                Dim comeTesto As String = Encoding.UTF8.GetString(cifrato)

                Assert.DoesNotContain(ChiaveFinta, comeTesto, "la chiave non sta in chiaro nel file")
                Assert.DoesNotContain("sk-ant", comeTesto, "nemmeno il suo inizio")
                Assert.IsGreaterThan(ChiaveFinta.Length, cifrato.Length, "e il cifrato non è il testo tale e quale")
            Finally
                Pulisci(cartella)
            End Try
        End Sub

        <TestMethod>
        Public Sub SenzaFileNonCEChiaveENonCEAnomalia()
            ' Il primo avvio: non c'è nessun file, e non è un guasto. La differenza fra
            ' «non l'ho mai salvata» e «non si apre» la legge chi monta il motore.
            Dim cartella As CartellaDati = CartellaTemporanea()
            Dim illeggibile As Boolean = True

            Assert.IsNull(New ArchivioSegreti(cartella).LeggiChiaveApi(illeggibile), "niente chiave")
            Assert.IsFalse(illeggibile, "e niente da segnalare")
            Assert.IsFalse(Directory.Exists(cartella.Radice), "leggere non crea niente")
        End Sub

        <TestMethod>
        Public Sub UnFileGuastoNonSolleva()
            ' Un blob che DPAPI rifiuta: è la forma che ha il file copiato da un altro PC
            ' o salvato da un altro account di Windows. Non deve far cadere l'avvio, ma
            ' non deve nemmeno passare per un «non ce l'ho»: l'utente quel file lo vede.
            Dim cartella As CartellaDati = CartellaTemporanea()
            Try
                cartella.Assicura()
                File.WriteAllBytes(cartella.FileSegreti, New Byte() {1, 2, 3, 4, 5, 6, 7, 8})

                Dim illeggibile As Boolean
                Assert.IsNull(New ArchivioSegreti(cartella).LeggiChiaveApi(illeggibile), "niente chiave")
                Assert.IsTrue(illeggibile, "ma c'è qualcosa da dire")
            Finally
                Pulisci(cartella)
            End Try
        End Sub

        <TestMethod>
        Public Sub UnFileVuotoValeComeGuasto()
            ' Zero byte non si decifrano nemmeno: è il residuo di una scrittura andata a
            ' vuoto, e trattarlo come «non ce l'ho» nasconderebbe il perché.
            Dim cartella As CartellaDati = CartellaTemporanea()
            Try
                cartella.Assicura()
                File.WriteAllBytes(cartella.FileSegreti, Array.Empty(Of Byte)())

                Dim illeggibile As Boolean
                Assert.IsNull(New ArchivioSegreti(cartella).LeggiChiaveApi(illeggibile), "niente chiave")
                Assert.IsTrue(illeggibile, "e lo si dice")
            Finally
                Pulisci(cartella)
            End Try
        End Sub

        <TestMethod>
        Public Sub RisalvarlaPrendeIlPostoDellaPrecedente()
            Dim cartella As CartellaDati = CartellaTemporanea()
            Try
                Dim archivio As New ArchivioSegreti(cartella)
                archivio.SalvaChiaveApi(ChiaveFinta)
                archivio.SalvaChiaveApi("sk-ant-la-seconda-4321")

                Dim illeggibile As Boolean
                Assert.AreEqual("sk-ant-la-seconda-4321", archivio.LeggiChiaveApi(illeggibile), "vale l'ultima")
                Assert.IsFalse(File.Exists(cartella.FileSegreti & ".tmp"), "e il temporaneo non resta in giro")
            Finally
                Pulisci(cartella)
            End Try
        End Sub

        <TestMethod>
        Public Sub GliSpaziAiBordiNonEntranoNellaChiave()
            ' Una chiave incollata si porta dietro spazi e a capo: entrerebbero
            ' nell'intestazione HTTP e la chiamata fallirebbe per un motivo incomprensibile.
            Dim cartella As CartellaDati = CartellaTemporanea()
            Try
                Dim archivio As New ArchivioSegreti(cartella)
                archivio.SalvaChiaveApi("  " & ChiaveFinta & vbLf)

                Dim illeggibile As Boolean
                Assert.AreEqual(ChiaveFinta, archivio.LeggiChiaveApi(illeggibile), "netta")
            Finally
                Pulisci(cartella)
            End Try
        End Sub

        <TestMethod>
        Public Sub UnaChiaveVuotaNonSiSalva()
            Dim cartella As CartellaDati = CartellaTemporanea()

            Dim archivio As New ArchivioSegreti(cartella)

            Assert.Throws(Of ArgumentException)(Sub() archivio.SalvaChiaveApi("   "))
            Assert.IsFalse(Directory.Exists(cartella.Radice), "e non lascia niente dietro di sé")
        End Sub

        <TestMethod>
        Public Sub CancellarlaLaTogliePerDavveroEdEIndolore()
            Dim cartella As CartellaDati = CartellaTemporanea()
            Try
                Dim archivio As New ArchivioSegreti(cartella)
                archivio.SalvaChiaveApi(ChiaveFinta)

                archivio.Cancella()
                Assert.IsFalse(archivio.Esiste, "il file è sparito")

                archivio.Cancella()
                Assert.IsFalse(archivio.Esiste, "e ripeterlo non è un errore")
            Finally
                Pulisci(cartella)
            End Try
        End Sub

        <TestMethod>
        Public Sub LaMascheraMostraSoloIBordi()
            ' «sk-ant-…ultime 4 cifre» (cap. 11.3): quel tanto che basta a riconoscerla
            ' senza poterla usare.
            Dim mascherata As String = ArchivioSegreti.Maschera(ChiaveFinta)

            Assert.StartsWith("sk-ant-", mascherata, "l'inizio si riconosce")
            Assert.EndsWith("9876", mascherata, "e le ultime quattro")
            Assert.DoesNotContain("collaudo", mascherata, "il mezzo no")
            Assert.AreEqual("—", ArchivioSegreti.Maschera(Nothing), "di niente non si mostra niente")
            Assert.DoesNotContain("corta", ArchivioSegreti.Maschera("corta"),
                                  "e di una troppo corta nemmeno le ultime cifre")
        End Sub

    End Class

End Namespace
