Imports System.IO
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' La prima gamba della non-regressione di T2 (cap. 14): <b>la richiesta che
    ''' parte</b>. Sugli stessi artefatti, il prompt che l'app costruisce dal pool deve
    ''' essere identico a quello che il prototipo costruisce nel codice — e con lui il
    ''' modello e il limite di token.
    ''' </summary>
    ''' <remarks>
    ''' È la gamba che dà senso all'altra. Se la richiesta è identica <b>a parità di
    ''' modello</b>, ogni differenza negli esiti è varianza del modello e non del nostro
    ''' codice: è esattamente ciò che il collaudo di tappa deve poter affermare. In più
    ''' questa batteria non chiede né chiave né rete, quindi resta verde per sempre,
    ''' anche quando il prototipo non sarà più avviabile.
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
                CasiDiCollaudo.PromptAtteso(CasiDiCollaudo.Compatibile),
                CasiDiCollaudo.PromptConfronto(PoolIntegrato(), CasiDiCollaudo.Compatibile))

            Assert.IsNull(differenza, $"Caso «compatibile»: il prompt {differenza}")

        End Sub

        <TestMethod>
        Public Sub IlPromptDelConfrontoECaratterePerCarattereQuelloDelPrototipo_Eliminatorio()

            Dim differenza As String = CasiDiCollaudo.PrimaDifferenza(
                CasiDiCollaudo.PromptAtteso(CasiDiCollaudo.Eliminatorio),
                CasiDiCollaudo.PromptConfronto(PoolIntegrato(), CasiDiCollaudo.Eliminatorio))

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
        Public Sub IlConfrontoUsaIlModelloEIlLimiteDelPrototipo()

            ' Il prototipo: MODEL_RAGIONAMENTO = claude-sonnet-4-6, MAX_TOKENS_CONFRONTO = 4000.
            Dim confronto As Prompt = PoolIntegrato().Carica("confronto")
            Dim modello As ModelloConcreto = Modelli.Predefiniti().PerLivello(confronto.Modello)

            Assert.AreEqual("claude-sonnet-4-6", modello.Id, "il modello del confronto")
            Assert.AreEqual(4000, confronto.MaxToken, "il limite di token del confronto")

            ' E il corpo della richiesta non deve dichiarare nulla sul ragionamento:
            ' su Sonnet 4.6 è già spento, e tacere tiene la richiesta identica.
            Dim corpo As JsonObject = ClientClaude.CorpoRichiesta(
                modello, confronto.MaxToken,
                JsonValue.Create(CasiDiCollaudo.PromptConfronto(PoolIntegrato(), CasiDiCollaudo.Compatibile)))

            Assert.IsFalse(corpo.ContainsKey("thinking"), "niente thinking, come il prototipo")
            Assert.AreEqual(3, corpo.Count, "solo model, max_tokens e messages")

        End Sub

    End Class

End Namespace
