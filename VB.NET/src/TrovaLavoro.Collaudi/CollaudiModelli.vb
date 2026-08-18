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

    End Class

End Namespace
