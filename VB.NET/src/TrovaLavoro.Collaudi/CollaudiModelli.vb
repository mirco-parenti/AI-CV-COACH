Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi della mappa livello → modello (cap. 02.5). Verificano che i
    ''' predefiniti siano quelli di prodotto, che il file basti a cambiare modello
    ''' senza ricompilare, che un file rotto non impedisca l'avvio e che un livello
    ''' sconosciuto si fermi subito.
    ''' </summary>
    <TestClass>
    Public Class CollaudiModelli

        <TestMethod>
        Public Sub PredefinitiDiProdotto()
            ' Haiku 4.5 per le estrazioni — lo stesso del prototipo, ed è tuttora
            ' l'ultimo della sua fascia — e Sonnet 5 per il ragionamento, che a Sonnet
            ' 4.6 succede fra i modelli correnti.
            Dim m As Modelli = Modelli.Predefiniti()

            Assert.AreEqual("claude-haiku-4-5", m.ModelloSemplice.Id, "livello semplice")
            Assert.AreEqual("claude-sonnet-5", m.ModelloRagionamento.Id, "livello di ragionamento")
            Assert.AreEqual(OrigineModelli.Predefinita, m.Origine, "origine")
        End Sub

        <TestMethod>
        Public Sub SoloIlRagionamentoDichiaraLInterruttore()
            ' Sul livello semplice si tace: Haiku 4.5 è quello del prototipo e tacere
            ' tiene la richiesta identica alla sua. Sul ragionamento invece si dichiara
            ' spento, perché Sonnet 5 lo accenderebbe di suo e max_tokens limita
            ' ragionamento e risposta insieme.
            Dim m As Modelli = Modelli.Predefiniti()

            Assert.IsFalse(m.ModelloSemplice.RagionamentoEsteso.HasValue, "semplice: non si dichiara")
            Assert.IsTrue(m.ModelloRagionamento.RagionamentoEsteso.HasValue, "ragionamento: si dichiara")
            Assert.IsFalse(m.ModelloRagionamento.RagionamentoEsteso.Value, "ed è spento")
        End Sub

        <TestMethod>
        Public Sub LaFormaBreveCambiaUnModelloSolo()
            ' Una riga sposta il ragionamento — qui all'indietro, sul modello del
            ' prototipo — e il livello semplice resta dov'era.
            Dim m As Modelli = Modelli.DaJson("{ ""ragionamento"": ""claude-sonnet-4-6"" }")

            Assert.AreEqual("claude-sonnet-4-6", m.ModelloRagionamento.Id, "ragionamento")
            Assert.AreEqual("claude-haiku-4-5", m.ModelloSemplice.Id, "semplice: resta il predefinito")
            Assert.AreEqual(OrigineModelli.File, m.Origine, "origine")
        End Sub

        <TestMethod>
        Public Sub LaFormaBreveNonPortaLInterruttoreDelPredefinito()
            ' La forma breve dichiara un identificativo e basta: l'interruttore del
            ' predefinito non deve sopravvivergli, o si spegnerebbe il ragionamento di
            ' un modello scelto apposta per usarlo.
            Dim m As Modelli = Modelli.DaJson("{ ""ragionamento"": ""claude-opus-4-8"" }")

            Assert.AreEqual("claude-opus-4-8", m.ModelloRagionamento.Id, "identificativo")
            Assert.IsFalse(m.ModelloRagionamento.RagionamentoEsteso.HasValue, "niente interruttore")
        End Sub

        <TestMethod>
        Public Sub LaFormaEstesaPortaAncheLInterruttore()
            Dim m As Modelli = Modelli.DaJson(
                "{ ""ragionamento"": { ""id"": ""claude-sonnet-5"", ""ragionamento_esteso"": true } }")

            Assert.AreEqual("claude-sonnet-5", m.ModelloRagionamento.Id, "identificativo")
            Assert.IsTrue(m.ModelloRagionamento.RagionamentoEsteso.HasValue, "l'interruttore è dichiarato")
            Assert.IsTrue(m.ModelloRagionamento.RagionamentoEsteso.Value, "ed è acceso")
        End Sub

        <TestMethod>
        Public Sub FileAssenteRipiegaSuiPredefiniti()
            Dim m As Modelli = Modelli.Carica(Path.Combine(Path.GetTempPath(), "modelli-che-non-esistono.json"))

            Assert.AreEqual(OrigineModelli.Predefinita, m.Origine, "origine")
            Assert.IsNotNull(m.Avviso, "l'avviso per il log deve esserci")
            Assert.AreEqual("claude-sonnet-5", m.ModelloRagionamento.Id, "deve valere il predefinito")
        End Sub

        <TestMethod>
        Public Sub FileCorrottoRipiegaSuiPredefiniti()
            ' Una configurazione illeggibile non deve impedire l'avvio: si dice e si
            ' tira dritto coi valori di prodotto.
            Dim percorso As String = Path.Combine(Path.GetTempPath(), "modelli-corrotti.json")
            File.WriteAllText(percorso, "{ questo non è JSON")
            Try
                Dim m As Modelli = Modelli.Carica(percorso)

                Assert.AreEqual(OrigineModelli.Predefinita, m.Origine, "origine")
                Assert.IsNotNull(m.Avviso, "l'avviso per il log deve esserci")
                Assert.AreEqual("claude-haiku-4-5", m.ModelloSemplice.Id, "deve valere il predefinito")
            Finally
                File.Delete(percorso)
            End Try
        End Sub

        <TestMethod>
        Public Sub IlLivelloSiRiconosceComunqueScritto()
            ' Il livello arriva dai metadati di un prompt, scritti a mano.
            Dim m As Modelli = Modelli.Predefiniti()

            Assert.AreEqual("claude-haiku-4-5", m.PerLivello("  Semplice ").Id, "spazi e maiuscole")
            Assert.AreEqual("claude-sonnet-5", m.PerLivello("ragionamento").Id, "ragionamento")
        End Sub

        <TestMethod>
        Public Sub UnLivelloSconosciutoSiFermaSubito()
            ' Meglio adesso, col nome in chiaro, che a metà di un flusso.
            Dim m As Modelli = Modelli.Predefiniti()

            Assert.Throws(Of ArgumentException)(
                Sub() m.PerLivello("potentissimo"),
                "un livello inventato doveva sollevare")
        End Sub

        ' ==================================================================
        ' Scrivere il file dalle Impostazioni (2026-08-27)
        ' ==================================================================

        <TestMethod>
        Public Sub CambiareUnLivelloNonToccaIlResto()

            ' Le Impostazioni conoscono i due identificativi e nient'altro: l'interruttore
            ' del ragionamento e qualunque campo messo lì a mano non sono roba loro.
            Dim prima As String =
                "{ ""semplice"": ""claude-haiku-4-5""," &
                "  ""ragionamento"": { ""id"": ""claude-sonnet-5"", ""ragionamento_esteso"": true," &
                "                      ""nota"": ""esperimento"" } }"

            Dim dopo As String = Modelli.ConLivello(prima, Modelli.Ragionamento, "claude-opus-4-8", Nothing)
            Dim riletti As Modelli = Modelli.DaJson(dopo)

            Assert.AreEqual("claude-opus-4-8", riletti.ModelloRagionamento.Id, "l'identificativo è cambiato")
            Assert.IsTrue(riletti.ModelloRagionamento.RagionamentoEsteso.Value,
                          "l'interruttore è rimasto acceso com'era")
            StringAssert.Contains(dopo, "esperimento", "e il campo che non capisco è ancora lì")
            Assert.AreEqual("claude-haiku-4-5", riletti.ModelloSemplice.Id, "l'altro livello non si tocca")

        End Sub

        <TestMethod>
        Public Sub LaFormaBreveRestaBreve()

            ' Chi ha scritto quel file a mano deve ritrovarlo come lo aveva lasciato.
            Dim dopo As String = Modelli.ConLivello(
                "{ ""semplice"": ""claude-haiku-4-5"" }", Modelli.Semplice, "claude-haiku-9", Nothing)

            Assert.AreEqual("claude-haiku-9", Modelli.DaJson(dopo).ModelloSemplice.Id)
            Assert.IsFalse(dopo.Contains("ragionamento_esteso"), "nessun campo comparso dal nulla")

        End Sub

        <TestMethod>
        Public Sub UnLivelloAssenteSiScriveConLInterruttoreInVigore()

            ' Il caso più insidioso, ed è quello di tutti i giorni: il file non c'è
            ' ancora. Il predefinito del ragionamento dichiara l'interruttore SPENTO, e
            ' scrivere il solo identificativo lo riporterebbe a «non dichiarato» — cioè
            ' acceso, su Sonnet 5 — troncando le risposte senza errore. Cambiare modello
            ' non deve cambiare di nascosto una seconda cosa.
            Dim inVigore As Modelli = Modelli.Predefiniti()

            Dim dopo As String = Modelli.ConLivello(Nothing, Modelli.Ragionamento, "claude-sonnet-5-1",
                                                    inVigore.ModelloRagionamento.RagionamentoEsteso)
            Dim riletti As Modelli = Modelli.DaJson(dopo)

            Assert.AreEqual("claude-sonnet-5-1", riletti.ModelloRagionamento.Id, "il modello nuovo")
            Assert.IsTrue(riletti.ModelloRagionamento.RagionamentoEsteso.HasValue,
                          "l'interruttore è dichiarato")
            Assert.IsFalse(riletti.ModelloRagionamento.RagionamentoEsteso.Value,
                           "e vale quel che valeva: spento")

        End Sub

        <TestMethod>
        Public Sub UnFileCheNonSiCapisceNonSiSostituisce()

            ' Quel file è dell'utente: riscriverlo sopra perché non lo si capisce sarebbe
            ' il modo più veloce di perdere quel che c'era dentro.
            Assert.Throws(Of Text.Json.JsonException)(
                Sub() Modelli.ConLivello("[1,2,3]", Modelli.Semplice, "claude-haiku-4-5", Nothing),
                "un JSON che non è un oggetto doveva sollevare")

        End Sub

        <TestMethod>
        Public Sub CambiareModelloValeSuDiscoESubito()

            ConFileDiProva(
                Sub(percorso)

                    Dim inVigore As Modelli = Modelli.Predefiniti()

                    inVigore.CambiaModello(Modelli.Ragionamento, "claude-opus-4-8", percorso)

                    ' In vigore: è l'oggetto che il client interroga a ogni chiamata, e
                    ' la prossima deve partire col modello nuovo senza riavviare niente.
                    Assert.AreEqual("claude-opus-4-8", inVigore.PerLivello(Modelli.Ragionamento).Id,
                                    "in vigore")
                    Assert.AreEqual(OrigineModelli.File, inVigore.Origine,
                                    "e la provenienza adesso è il file")

                    ' Su disco: se restasse solo in memoria, al riavvio tornerebbe
                    ' indietro da solo senza che nessuno capisca perché.
                    Assert.AreEqual("claude-opus-4-8",
                                    Modelli.Carica(percorso).ModelloRagionamento.Id, "su disco")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnDiscoCheRifiutaNonCambiaNienteInVigore()

            ConFileDiProva(
                Sub(percorso)

                    ' Un percorso impossibile: il file di prova fa da cartella, e nessuna
                    ' cartella può nascere lì dentro.
                    File.WriteAllText(percorso, "{}")
                    Dim impossibile As String = Path.Combine(percorso, "dentro", "modelli.json")

                    Dim inVigore As Modelli = Modelli.Predefiniti()

                    Assert.Throws(Of IOException)(
                        Sub() inVigore.CambiaModello(Modelli.Semplice, "claude-haiku-9", impossibile),
                        "il disco doveva rifiutare")

                    Assert.AreEqual("claude-haiku-4-5", inVigore.PerLivello(Modelli.Semplice).Id,
                                    "e in vigore non è cambiato niente")
                    Assert.AreEqual(OrigineModelli.Predefinita, inVigore.Origine, "nemmeno la provenienza")

                End Sub)

        End Sub

        ''' <summary>Un percorso in una cartella usa-e-getta, portata via alla fine.</summary>
        Private Shared Sub ConFileDiProva(prova As Action(Of String))

            Dim radice As String = Path.Combine(Path.GetTempPath(), "modelli-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(radice)

            Try
                prova(Path.Combine(radice, "modelli.json"))
            Finally
                CartelleDiProva.PortaVia(radice)
            End Try

        End Sub

    End Class

End Namespace
