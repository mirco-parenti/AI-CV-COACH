Imports System.IO
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' La prima gamba della non-regressione di T2 (cap. 14): <b>la richiesta che
    ''' parte</b>. Sugli stessi artefatti, il prompt che l'app costruisce dal pool deve
    ''' essere identico a quello che il prototipo costruisce nel codice.
    ''' </summary>
    ''' <remarks>
    ''' È la gamba che dà senso all'altra, e il <b>testo</b> del prompt continua a
    ''' reggerla carattere per carattere. Quello che non regge più è la parità di
    ''' <b>modello</b>: il ragionamento dell'app è passato a Sonnet 5 mentre il prototipo
    ''' resta congelato su Sonnet 4.6, perciò da qui in avanti una differenza negli esiti
    ''' può venire tanto dal modello quanto dal codice, e il prototipo è un termine di
    ''' paragone, non più un metro (cap. 04.7). Ciò che questi due collaudi difendono
    ''' adesso è il confine: <b>il modello e l'interruttore del ragionamento sono l'unica
    ''' cosa che diverge</b>, e i limiti di token non scendono sotto i suoi. In più questa
    ''' batteria non chiede né chiave né rete, quindi resta verde per sempre, anche quando
    ''' il prototipo non sarà più avviabile.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiParitaPrompt

        Private Shared Function PoolIntegrato() As LibreriaPrompt
            ' Il pool che finisce nell'eseguibile, non una cartella di prova.
            Return LibreriaPrompt.Apri(Path.Combine(Path.GetTempPath(), "pool-inesistente"))
        End Function

        <TestMethod>
        Public Sub IlPromptDelConfrontoECaratterePerCarattereQuelloDelPrototipo_Compatibile()

            Dim differenza As String = CasiDiCollaudo.PrimaDifferenza(
                CasiDiCollaudo.PromptAtteso("confronto", CasiDiCollaudo.Compatibile),
                CasiDiCollaudo.PromptConfronto(PoolIntegrato(), CasiDiCollaudo.Compatibile))

            Assert.IsNull(differenza, $"Caso «compatibile»: il prompt {differenza}")

        End Sub

        <TestMethod>
        Public Sub IlPromptDelConfrontoECaratterePerCarattereQuelloDelPrototipo_Eliminatorio()

            Dim differenza As String = CasiDiCollaudo.PrimaDifferenza(
                CasiDiCollaudo.PromptAtteso("confronto", CasiDiCollaudo.Eliminatorio),
                CasiDiCollaudo.PromptConfronto(PoolIntegrato(), CasiDiCollaudo.Eliminatorio))

            Assert.IsNull(differenza, $"Caso «eliminatorio»: il prompt {differenza}")

        End Sub

        <TestMethod>
        Public Sub IlPromptDellaMitigazioneECaratterePerCarattereQuelloDelPrototipo_Compatibile()

            ' La seconda metà del mestiere del confronto: fino a T4 la parità copriva un
            ' prompt solo dei due, e la mitigazione entrava in produzione senza rete di
            ' protezione (voce presa dentro T4, cap. 14).
            Dim differenza As String = CasiDiCollaudo.PrimaDifferenza(
                CasiDiCollaudo.PromptAtteso("mitigazione", CasiDiCollaudo.Compatibile),
                CasiDiCollaudo.PromptMitigazione(PoolIntegrato(), CasiDiCollaudo.Compatibile))

            Assert.IsNull(differenza, $"Caso «compatibile»: il prompt {differenza}")

        End Sub

        <TestMethod>
        Public Sub IlPromptDellaMitigazioneECaratterePerCarattereQuelloDelPrototipo_Eliminatorio()

            Dim differenza As String = CasiDiCollaudo.PrimaDifferenza(
                CasiDiCollaudo.PromptAtteso("mitigazione", CasiDiCollaudo.Eliminatorio),
                CasiDiCollaudo.PromptMitigazione(PoolIntegrato(), CasiDiCollaudo.Eliminatorio))

            Assert.IsNull(differenza, $"Caso «eliminatorio»: il prompt {differenza}")

        End Sub

        <TestMethod>
        Public Sub GliAccentiEGliApostrofiRestanoInChiaro()

            ' Il punto per cui questa gamba esiste: l'encoder predefinito di .NET
            ' scriverebbe gli accenti a codici, e il prompt non sarebbe più quello con
            ' cui il prototipo è stato validato.
            Dim scritto As String = LibreriaPrompt.ComeNelPrompt(CasiDiCollaudo.Profilo())

            Assert.Contains("Forlì", scritto, "gli accenti devono restare lettere")
            Assert.Contains("dell'azienda", scritto, "gli apostrofi devono restare apostrofi")
            Assert.DoesNotContain("\u00", scritto, "nessuna sequenza di escape nel testo")

        End Sub

        <TestMethod>
        Public Sub GliArtefattiSonoIndentatiComeNelPrototipo()

            ' JSON.stringify(x, null, 2): due spazi, e una riga per campo.
            Dim scritto As String = LibreriaPrompt.ComeNelPrompt(
                JsonNode.Parse("{""a"":1,""b"":{""c"":[1,2]}}"))

            Assert.AreEqual(
                "{" & vbLf &
                "  ""a"": 1," & vbLf &
                "  ""b"": {" & vbLf &
                "    ""c"": [" & vbLf &
                "      1," & vbLf &
                "      2" & vbLf &
                "    ]" & vbLf &
                "  }" & vbLf &
                "}",
                scritto.Replace(vbCrLf, vbLf), "l'indentazione deve essere quella di JavaScript")

        End Sub

        <TestMethod>
        Public Sub IlConfrontoDivergeDalPrototipoSoloNelModello()

            ' Il prototipo: MODEL_RAGIONAMENTO = claude-sonnet-4-6, MAX_TOKENS_CONFRONTO = 4000.
            ' Il ragionamento dell'app non è più il suo, ed è la sola cosa che diverge.
            Dim confronto As Prompt = PoolIntegrato().Carica("confronto")
            Dim modello As ModelloConcreto = Modelli.Predefiniti().PerLivello(confronto.Modello)

            Assert.AreEqual("claude-sonnet-5", modello.Id, "il modello del confronto")

            ' Nemmeno il limite è più il suo, e il distacco è voluto (Pool 1.03): il
            ' prototipo si fermava a 4000 e su un annuncio ricco di requisiti troncava.
            ' Un tetto più alto non cambia la richiesta né la risposta a parità di
            ' contenuto: sposta solo il punto in cui il modello verrebbe interrotto —
            ' perciò qui si verifica che non scenda, non che coincida.
            Assert.IsGreaterThanOrEqualTo(4000, confronto.MaxToken,
                                          "il limite di token del confronto non deve scendere sotto il suo")

            ' Il corpo guadagna un campo solo rispetto al suo, e per necessità: Sonnet 5
            ' accenderebbe il ragionamento di suo, e max_tokens limita ragionamento e
            ' risposta insieme. Tutto il resto della richiesta resta quello di sempre.
            Dim corpo As JsonObject = ClientClaude.CorpoRichiesta(
                modello, confronto.MaxToken,
                JsonValue.Create(CasiDiCollaudo.PromptConfronto(PoolIntegrato(), CasiDiCollaudo.Compatibile)))

            Assert.AreEqual(4, corpo.Count, "model, max_tokens, messages e thinking")
            Assert.AreEqual("disabled", corpo("thinking")("type").ToString(), "il ragionamento è spento")

        End Sub

        <TestMethod>
        Public Sub LaMitigazioneDivergeDalPrototipoSoloNelModello()

            ' Il prototipo: MODEL_RAGIONAMENTO = claude-sonnet-4-6, MAX_TOKENS_MITIGAZIONE = 2000.
            ' Stesso ragionamento del confronto sul limite: 2000 token bastavano per una
            ' lista corta di ponti, ma è il tetto sotto cui una risposta ricca verrebbe
            ' troncata — perciò si verifica che non scenda, non che coincida.
            Dim mitigazione As Prompt = PoolIntegrato().Carica("mitigazione")
            Dim modello As ModelloConcreto = Modelli.Predefiniti().PerLivello(mitigazione.Modello)

            Assert.AreEqual("claude-sonnet-5", modello.Id, "il modello della mitigazione")
            Assert.IsGreaterThanOrEqualTo(2000, mitigazione.MaxToken,
                                          "il limite di token della mitigazione non deve scendere sotto il suo")

        End Sub

    End Class

End Namespace
