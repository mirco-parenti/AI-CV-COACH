Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi della mappa livello → modello (cap. 02.5). Verificano che i
    ''' predefiniti siano quelli con cui si fa il confronto col prototipo, che il file
    ''' basti a cambiare modello senza ricompilare, che un file rotto non impedisca
    ''' l'avvio e che un livello sconosciuto si fermi subito.
    ''' </summary>
    <TestClass>
    Public Class CollaudiModelli

        <TestMethod>
        Public Sub PredefinitiAParitaColPrototipo()
            ' La batteria di T2 confronta la nuova app col prototipo a parità di
            ' modello: gli identificativi devono essere gli stessi di server.js.
            Dim m As Modelli = Modelli.Predefiniti()

            Assert.AreEqual("claude-haiku-4-5", m.ModelloSemplice.Id, "MODEL_SEMPLICE")
            Assert.AreEqual("claude-sonnet-4-6", m.ModelloRagionamento.Id, "MODEL_RAGIONAMENTO")
            Assert.AreEqual(OrigineModelli.Predefinita, m.Origine, "origine")
        End Sub

        <TestMethod>
        Public Sub PredefinitiNonDichiaranoIlRagionamento()
            ' Su Sonnet 4.6 il ragionamento è già spento: dichiararlo aggiungerebbe
            ' una differenza fra la nostra richiesta e quella del prototipo, proprio
            ' nel collaudo che serve a isolare le differenze di codice.
            Dim m As Modelli = Modelli.Predefiniti()

            Assert.IsFalse(m.ModelloSemplice.RagionamentoEsteso.HasValue, "semplice")
            Assert.IsFalse(m.ModelloRagionamento.RagionamentoEsteso.HasValue, "ragionamento")
        End Sub

        <TestMethod>
        Public Sub LaFormaBreveCambiaUnModelloSolo()
            ' Il secondo esperimento è una riga: si sposta il ragionamento su Sonnet 5
            ' e il livello semplice resta dov'era.
            Dim m As Modelli = Modelli.DaJson("{ ""ragionamento"": ""claude-sonnet-5"" }")

            Assert.AreEqual("claude-sonnet-5", m.ModelloRagionamento.Id, "ragionamento")
            Assert.AreEqual("claude-haiku-4-5", m.ModelloSemplice.Id, "semplice: resta il predefinito")
            Assert.AreEqual(OrigineModelli.File, m.Origine, "origine")
        End Sub

        <TestMethod>
        Public Sub LaFormaEstesaPortaAncheLInterruttore()
            Dim m As Modelli = Modelli.DaJson(
                "{ ""ragionamento"": { ""id"": ""claude-sonnet-5"", ""ragionamento_esteso"": false } }")

            Assert.AreEqual("claude-sonnet-5", m.ModelloRagionamento.Id, "identificativo")
            Assert.IsTrue(m.ModelloRagionamento.RagionamentoEsteso.HasValue, "l'interruttore è dichiarato")
            Assert.IsFalse(m.ModelloRagionamento.RagionamentoEsteso.Value, "ed è spento")
        End Sub

        <TestMethod>
        Public Sub FileAssenteRipiegaSuiPredefiniti()
            Dim m As Modelli = Modelli.Carica(Path.Combine(Path.GetTempPath(), "modelli-che-non-esistono.json"))

            Assert.AreEqual(OrigineModelli.Predefinita, m.Origine, "origine")
            Assert.IsNotNull(m.Avviso, "l'avviso per il log deve esserci")
            Assert.AreEqual("claude-sonnet-4-6", m.ModelloRagionamento.Id, "deve valere il predefinito")
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
            Assert.AreEqual("claude-sonnet-4-6", m.PerLivello("ragionamento").Id, "ragionamento")
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
