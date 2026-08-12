Imports System.Linq
Imports System.Text.Json
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi della tabella dei portali e delle ricerche salvate (cap. 06.3): il
    ''' contenuto di <c>ricerche.json</c>, letto, scritto e composto in indirizzi veri.
    ''' </summary>
    ''' <remarks>
    ''' Le domande sono quattro: che i predefiniti bastino a partire, che un file scritto a
    ''' mano possa sostituirli senza una nuova build, che ciò che non ha senso <b>non entri
    ''' zitto</b> — uno schema che non è un indirizzo del web, una ricerca che nomina un
    ''' portale che non c'è — e che l'indirizzo composto regga spazi e accenti.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiRicerche

        ' ==================================================================
        ' I predefiniti
        ' ==================================================================

        <TestMethod>
        Public Sub IPortaliPredefinitiSonoQuelliDelPrimoRilascio()

            Dim ricerche As Ricerche = Ricerche.Predefinita()

            ' Jooble sta dov'era InfoJobs: il 2026-08-12 la verifica sul campo ha trovato
            ' quella piattaforma chiusa (cap. 15, voce 7, rivista).
            CollectionAssert.AreEquivalent(
                {"Indeed", "Jooble", "Subito.it", "Cerca «lavora con noi»"},
                ricerche.Portali.Select(Function(p) p.Nome).ToArray(),
                "i portali del cap. 15, voce 7, più la ricerca generica")

            Assert.IsEmpty(ricerche.Salvate, "al primo avvio non c'è nessuna ricerca salvata")
            Assert.AreEqual(OrigineRicerche.Predefinita, ricerche.Origine)

            ' Tutti gli schemi sono indirizzi del web e sanno dove mettere le parole
            ' dell'utente: se un predefinito non lo facesse, il menù di P3 avrebbe una
            ' voce che non porta da nessuna parte.
            For Each portale As Portale In ricerche.Portali
                StringAssert.StartsWith(portale.Schema, "https://", $"«{portale.Nome}»")
                Assert.Contains("{cosa}", portale.Schema, $"«{portale.Nome}» non sa cosa cercare")
            Next

        End Sub

        ' ==================================================================
        ' L'indirizzo composto
        ' ==================================================================

        <TestMethod>
        Public Sub LIndirizzoCompostoCodificaSpaziEAccenti()

            Dim portale As New Portale With {
                .Nome = "Prova", .Schema = "https://esempio.it/cerca?q={cosa}&dove={dove}"}

            Assert.AreEqual("https://esempio.it/cerca?q=perito%20elettronico&dove=Forl%C3%AC",
                            portale.ComponiUrl("perito elettronico", "Forlì"),
                            "spazi e accenti vanno codificati, o la query si rompe")

        End Sub

        <TestMethod>
        Public Sub UnaZonaNonDettaLasciaIlPostoVuoto()

            Dim portale As New Portale With {
                .Nome = "Prova", .Schema = "https://esempio.it/cerca?q={cosa}&dove={dove}"}

            ' Cercare senza dire dove è legittimo: il segnaposto sparisce, non resta lì
            ' scritto fra le graffe in mezzo all'indirizzo.
            Assert.AreEqual("https://esempio.it/cerca?q=magazziniere&dove=",
                            portale.ComponiUrl("magazziniere", Nothing))

        End Sub

        ' ==================================================================
        ' Il file scritto a mano
        ' ==================================================================

        <TestMethod>
        Public Sub IPortaliDelFileSostituisconoIPredefiniti()

            Dim ricerche As Ricerche = Ricerche.DaJson("
                {
                  ""portali"": [
                    { ""nome"": ""Il mio portale"", ""schema"": ""https://esempio.it/jobs?q={cosa}"" }
                  ]
                }")

            Assert.HasCount(1, ricerche.Portali,
                            "chi scrive la propria tabella vuole la propria, non la propria più la nostra")
            Assert.AreEqual("Il mio portale", ricerche.Portali(0).Nome)
            Assert.AreEqual(OrigineRicerche.File, ricerche.Origine)
            Assert.IsNull(ricerche.Avviso, "un file in ordine non ha niente da dire")

        End Sub

        <TestMethod>
        Public Sub UnaSezioneAssenteRicadeSulPredefinito()

            ' Un file che dichiara solo le ricerche salvate resta utilizzabile: i portali
            ' sono quelli di sempre.
            Dim ricerche As Ricerche = Ricerche.DaJson("
                {
                  ""salvate"": [
                    { ""nome"": ""Muletto a Genova"", ""portale"": ""indeed"",
                      ""cosa"": ""magazziniere"", ""dove"": ""Genova"" }
                  ]
                }")

            Assert.HasCount(4, ricerche.Portali, "i portali predefiniti sono rimasti")
            Assert.HasCount(1, ricerche.Salvate)

            ' Il portale scritto in minuscolo è lo stesso portale, e viene ricondotto al
            ' suo nome vero: nel menù di P3 non deve comparire «indeed».
            Assert.AreEqual("Indeed", ricerche.Salvate(0).Portale)

        End Sub

        <TestMethod>
        Public Sub UnoSchemaCheNonEUnIndirizzoDelWebNonEntra()

            Dim ricerche As Ricerche = Ricerche.DaJson("
                {
                  ""portali"": [
                    { ""nome"": ""Buono"", ""schema"": ""https://esempio.it/jobs?q={cosa}"" },
                    { ""nome"": ""Disco"", ""schema"": ""file:///C:/Windows/system32"" },
                    { ""nome"": ""Trappola"", ""schema"": ""javascript:alert(1)"" },
                    { ""nome"": ""Senza schema"" }
                  ]
                }")

            CollectionAssert.AreEquivalent(
                {"Buono"}, ricerche.Portali.Select(Function(p) p.Nome).ToArray(),
                "solo http e https finiscono nella barra di un browser vero")

            Assert.IsNotNull(ricerche.Avviso, "le voci scartate non spariscono in silenzio")
            Assert.Contains("Disco", ricerche.Avviso)
            Assert.Contains("Trappola", ricerche.Avviso)

        End Sub

        <TestMethod>
        Public Sub UnaRicercaSuUnPortaleSconosciutoNonEntra()

            Dim ricerche As Ricerche = Ricerche.DaJson("
                {
                  ""salvate"": [
                    { ""nome"": ""Va bene"", ""portale"": ""Indeed"", ""cosa"": ""magazziniere"" },
                    { ""nome"": ""Va a vuoto"", ""portale"": ""PortaleCheNonCe"", ""cosa"": ""x"" }
                  ]
                }")

            CollectionAssert.AreEquivalent(
                {"Va bene"}, ricerche.Salvate.Select(Function(s) s.Nome).ToArray(),
                "una ricerca che non sa dove andare non va nel menù")

            Assert.Contains("Va a vuoto", ricerche.Avviso, "e lo si dice, invece di farla sparire")

        End Sub

        <TestMethod>
        Public Sub UnFileCheNonEUnOggettoSiFaSentire()

            ' Qui si solleva invece di ripiegare: a ripiegare — e a dirlo — è chi legge il
            ' file dal disco (Ricerche.Carica), che sa anche da quale percorso viene.
            Assert.Throws(Of JsonException)(Sub() Ricerche.DaJson("[1, 2, 3]"))

        End Sub

        ' ==================================================================
        ' Mettere da parte e dimenticare
        ' ==================================================================

        <TestMethod>
        Public Sub UnaRicercaCollaStessoNomeSostituisceLaGemella()

            Dim ricerche As Ricerche = Ricerche.Predefinita()

            ricerche.MettiDaParte(New RicercaSalvata With {
                .Nome = "La mia", .Portale = "Indeed", .Cosa = "magazziniere", .Dove = "Genova"})
            ricerche.MettiDaParte(New RicercaSalvata With {
                .Nome = "la mia", .Portale = "Subito.it", .Cosa = "muletto", .Dove = "Chiavari"})

            Assert.HasCount(1, ricerche.Salvate, "due voci uguali nel menù non aiutano nessuno")
            Assert.AreEqual("Subito.it", ricerche.Salvate(0).Portale, "vince l'ultima")

        End Sub

        <TestMethod>
        Public Sub UnaRicercaSenzaNomeOSuUnPortaleIgnotoNonSiSalva()

            Dim ricerche As Ricerche = Ricerche.Predefinita()

            Assert.Throws(Of ArgumentException)(
                Sub() ricerche.MettiDaParte(New RicercaSalvata With {
                    .Nome = "  ", .Portale = "Indeed"}),
                "senza nome non si ritrova più")

            Assert.Throws(Of ArgumentException)(
                Sub() ricerche.MettiDaParte(New RicercaSalvata With {
                    .Nome = "Va a vuoto", .Portale = "PortaleCheNonCe"}),
                "un portale che non c'è porta da nessuna parte")

            Assert.IsEmpty(ricerche.Salvate)

        End Sub

        <TestMethod>
        Public Sub UnaRicercaEntraRipulita()

            Dim ricerche As Ricerche = Ricerche.Predefinita()

            ricerche.MettiDaParte(New RicercaSalvata With {
                .Nome = "  Perito a Genova  ", .Portale = "indeed",
                .Cosa = " perito elettronico ", .Dove = " Genova "})

            Dim salvata As RicercaSalvata = ricerche.Salvate(0)

            Assert.AreEqual("Perito a Genova", salvata.Nome, "niente spazi ai bordi nel menù")
            Assert.AreEqual("Indeed", salvata.Portale, "il portale col suo nome vero")
            Assert.AreEqual("perito elettronico", salvata.Cosa)
            Assert.AreEqual("Genova", salvata.Dove)

            ' Ripulita adesso o ripulita rileggendo il file, il nome deve essere lo
            ' stesso: altrimenti la ricerca cambierebbe nome da sola al riavvio.
            Assert.AreEqual(salvata.Nome,
                            Ricerche.DaJson(ricerche.ComeJson().ToJsonString()).Salvate(0).Nome)

        End Sub

        <TestMethod>
        Public Sub DimenticareDiceSeCEraQualcosaDaDimenticare()

            Dim ricerche As Ricerche = Ricerche.Predefinita()
            ricerche.MettiDaParte(New RicercaSalvata With {.Nome = "La mia", .Portale = "Indeed"})

            Assert.IsTrue(ricerche.Dimentica("LA MIA"), "il nome non distingue le maiuscole")
            Assert.IsEmpty(ricerche.Salvate)
            Assert.IsFalse(ricerche.Dimentica("La mia"), "la seconda volta non c'è più niente")

        End Sub

        ' ==================================================================
        ' Da dove viene un annuncio catturato
        ' ==================================================================

        <TestMethod>
        Public Sub LaFonteEIlNomeDelPortaleQuandoLoConosciamo()

            Dim ricerche As Ricerche = Ricerche.Predefinita()

            ' La pagina di **un** annuncio non somiglia allo schema di ricerca del suo
            ' portale: quel che le lega è il sito, ed è quello che si guarda.
            Assert.AreEqual("Indeed", ricerche.FonteDi("https://it.indeed.com/viewjob?jk=9f3c1a"))

            ' Il «www.» non conta: chi scrive un portale in ricerche.json non deve
            ' indovinare quale forma userà il sito nei suoi link.
            Assert.AreEqual("Subito.it",
                            ricerche.FonteDi("https://www.subito.it/offerte-lavoro/magazziniere-genova-123"))

            Assert.AreEqual("Jooble", ricerche.FonteDi("https://it.jooble.org/jdp/-482913"))

        End Sub

        <TestMethod>
        Public Sub DaUnSitoSconosciutoLaFonteEIlSito()

            ' Il flusso C (cap. 12.3): un link arrivato per email, da un sito che non è
            ' fra i nostri portali. Una provenienza c'è lo stesso, ed è il sito.
            Assert.AreEqual("aziendarossi.it",
                            Ricerche.Predefinita().FonteDi("https://www.aziendarossi.it/lavora-con-noi/magazziniere"))

        End Sub

        <TestMethod>
        Public Sub QuelCheNonEUnIndirizzoNonHaFonte()

            Dim ricerche As Ricerche = Ricerche.Predefinita()

            For Each niente As String In {Nothing, "", "   ", "it.indeed.com/jobs", "una frase qualunque"}
                Assert.AreEqual(String.Empty, ricerche.FonteDi(niente),
                                $"«{niente}» non è un indirizzo assoluto")
            Next

        End Sub

        ' ==================================================================
        ' Andata e ritorno
        ' ==================================================================

        <TestMethod>
        Public Sub QuelloCheSiScriveSiRilegge()

            Dim prima As Ricerche = Ricerche.Predefinita()
            prima.MettiDaParte(New RicercaSalvata With {
                .Nome = "Perito a Genova", .Portale = "Indeed",
                .Cosa = "perito elettronico", .Dove = "Genova"})

            Dim dopo As Ricerche = Ricerche.DaJson(prima.ComeJson().ToJsonString())

            CollectionAssert.AreEqual(
                prima.Portali.Select(Function(p) $"{p.Nome}|{p.Schema}").ToArray(),
                dopo.Portali.Select(Function(p) $"{p.Nome}|{p.Schema}").ToArray())

            Assert.HasCount(1, dopo.Salvate)
            Assert.AreEqual("Perito a Genova", dopo.Salvate(0).Nome)
            Assert.AreEqual("perito elettronico", dopo.Salvate(0).Cosa)
            Assert.AreEqual("Genova", dopo.Salvate(0).Dove)

        End Sub

    End Class

End Namespace
