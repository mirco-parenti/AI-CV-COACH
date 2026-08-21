Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi delle preferenze dell'utente (cap. 03, pannello P8): il file
    ''' <c>impostazioni.json</c> che nasce alla prima chiusura delle Impostazioni, si
    ''' rilegge uguale, e quando manca o è storto lascia lavorare il programma lo stesso.
    ''' </summary>
    <TestClass>
    Public Class CollaudiImpostazioni

        <TestMethod>
        Public Sub SenzaFileValgonoIPredefinitiCheSonoQuelliDiIeri()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)

                    Assert.IsFalse(archivio.Esiste, "prima del primo giro in P8 il file non c'è")

                    Dim impostazioni As Impostazioni = archivio.Carica()

                    ' I predefiniti non sono una scelta neutra qualsiasi: sono esattamente
                    ' ciò che l'applicazione faceva prima che le Impostazioni esistessero.
                    ' Chi non le apre mai non deve accorgersi che sono arrivate.
                    Assert.AreEqual(LinguaDocumenti.Italiano, impostazioni.LinguaPredefinita)
                    Assert.IsTrue(impostazioni.RifinituraAttiva, "la rifinitura era sempre accesa")
                    Assert.AreEqual(OrigineImpostazioni.Predefinite, impostazioni.Origine)

                    ' E il file che non c'è non si annota: è la normalità, non un guasto.
                    Assert.IsNull(impostazioni.Avviso)

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SalvaERilegge()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)

                    archivio.Salva(New Impostazioni With {
                        .LinguaPredefinita = LinguaDocumenti.Inglese,
                        .RifinituraAttiva = False})

                    Assert.IsTrue(archivio.Esiste, "il file deve esserci")

                    Dim rilette As Impostazioni = archivio.Carica()

                    Assert.AreEqual(LinguaDocumenti.Inglese, rilette.LinguaPredefinita)
                    Assert.IsFalse(rilette.RifinituraAttiva)
                    Assert.AreEqual(OrigineImpostazioni.File, rilette.Origine)
                    Assert.IsNull(rilette.Avviso, "ciò che abbiamo scritto noi si rilegge senza rimostranze")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlFileSiScriveInteroAncheQuandoNienteECambiato()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)

                    archivio.Salva(Impostazioni.Predefinite())

                    Dim scritto As String = File.ReadAllText(cartella.FileImpostazioni)

                    ' Un file che elencasse solo ciò che è stato cambiato non direbbe
                    ' all'utente quali altre manopole esistono.
                    Assert.Contains("lingua_predefinita", scritto)
                    Assert.Contains("rifinitura_attiva", scritto)

                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnFileIlleggibileNonImpedisceDiLavorare()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)

                    cartella.Assicura()
                    File.WriteAllText(cartella.FileImpostazioni, "{ questo non è JSON")

                    Dim impostazioni As Impostazioni = archivio.Carica()

                    Assert.AreEqual(OrigineImpostazioni.Predefinite, impostazioni.Origine)
                    Assert.IsTrue(impostazioni.RifinituraAttiva, "si torna ai predefiniti")
                    Assert.IsNotNull(impostazioni.Avviso, "e stavolta lo si dice")
                    Assert.Contains(cartella.FileImpostazioni, impostazioni.Avviso,
                                    "l'avviso dice anche quale file non si è lasciato leggere")

                    ' Il file rotto resta dov'è: non si cancella la roba di qualcun altro.
                    Assert.IsTrue(File.Exists(cartella.FileImpostazioni))

                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaLinguaCheNonSappiamoScrivereSiScartaSenzaTravolgereLAltraVoce()

            ' È la differenza dichiarata con la taratura (cap. 11.6), dove una mappa storta
            ' si scarta intera: là le voci si compongono in un punteggio solo, qui le due
            ' preferenze non si parlano, e buttare via anche quella buona sarebbe una
            ' perdita gratuita.
            Dim lette As Impostazioni = Impostazioni.DaJson(
                "{ ""lingua_predefinita"": ""de"", ""rifinitura_attiva"": false }")

            Assert.AreEqual(LinguaDocumenti.Italiano, lette.LinguaPredefinita,
                            "la lingua storta torna al predefinito")
            Assert.IsFalse(lette.RifinituraAttiva,
                           "ma la preferenza buona che le stava accanto è sopravvissuta")
            Assert.IsNotNull(lette.Avviso)
            Assert.Contains("lingua_predefinita", lette.Avviso, "l'avviso dice quale voce è caduta")

        End Sub

        <TestMethod>
        Public Sub UnaRifinituraCheNonEVeroNeFalsoSiScartaSenzaTravolgereLaLingua()

            Dim lette As Impostazioni = Impostazioni.DaJson(
                "{ ""lingua_predefinita"": ""en"", ""rifinitura_attiva"": ""forse"" }")

            Assert.AreEqual(LinguaDocumenti.Inglese, lette.LinguaPredefinita,
                            "la lingua buona resta")
            Assert.IsTrue(lette.RifinituraAttiva, "la rifinitura storta torna al predefinito, cioè accesa")
            Assert.IsNotNull(lette.Avviso)
            Assert.Contains("rifinitura_attiva", lette.Avviso)

        End Sub

        <TestMethod>
        Public Sub UnFileACuiMancaUnaVoceTieneQuellaCheCE()

            Dim lette As Impostazioni = Impostazioni.DaJson("{ ""rifinitura_attiva"": false }")

            Assert.AreEqual(LinguaDocumenti.Italiano, lette.LinguaPredefinita,
                            "la voce assente vale il predefinito")
            Assert.IsFalse(lette.RifinituraAttiva, "quella scritta vale")
            Assert.IsNull(lette.Avviso, "una voce non scritta non è una voce sbagliata")

        End Sub

        <TestMethod>
        Public Sub LaLinguaSiRileggeAncheScrittaStrana()

            ' Un file corretto a mano può avere maiuscole o spazi: sono le stesse due
            ' lingue, e rifiutarle sarebbe pedanteria.
            Dim lette As Impostazioni = Impostazioni.DaJson("{ ""lingua_predefinita"": "" EN "" }")

            Assert.AreEqual(LinguaDocumenti.Inglese, lette.LinguaPredefinita)
            Assert.IsNull(lette.Avviso)

        End Sub

        ''' <summary>Un archivio su una cartella temporanea, che si porta via tutto alla fine.</summary>
        Private Shared Sub ConArchivioTemporaneo(prova As Action(Of ArchivioImpostazioni, CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "impostazioni-" & Guid.NewGuid().ToString("N"))

            Dim cartella As New CartellaDati(radice)

            Try
                prova(New ArchivioImpostazioni(cartella), cartella)
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

    End Class

End Namespace
