Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Non-regressione di <c>EstrattoreJson</c> contro il prototipo (cap. 14, T2).
    ''' Sono i sei casi con cui <c>estraiJson</c> è stato validato nello Step 1.35:
    ''' JSON pulito, con recinto, preambolo, coda, recinto+preambolo, spazzatura.
    ''' </summary>
    <TestClass>
    Public Class CollaudiEstrattoreJson

        ' Un oggetto del dominio, con accento: verifica di passaggio che i sorgenti
        ' siano letti come UTF-8 (CodePage 65001) e non con la codepage di sistema.
        Private Const JsonAtteso As String = "{""match_finale"": 72, ""nota"": ""Sede a Forlì""}"

        ''' <summary>Controlla che l'albero estratto porti i valori giusti.</summary>
        Private Shared Sub VerificaContenuto(estratto As JsonNode, caso As String)
            Assert.IsNotNull(estratto, $"Caso «{caso}»: nessun JSON estratto.")
            Assert.AreEqual(72, estratto("match_finale").GetValue(Of Integer)(),
                            $"Caso «{caso}»: match_finale diverso.")
            Assert.AreEqual("Sede a Forlì", estratto("nota").GetValue(Of String)(),
                            $"Caso «{caso}»: nota diversa (accento o codifica).")
        End Sub

        <TestMethod>
        Public Sub Caso1_JsonPulito()
            ' Il percorso felice: il testo è già JSON e non deve toccare il ripiego.
            VerificaContenuto(EstrattoreJson.Estrai(JsonAtteso), "JSON pulito")
        End Sub

        <TestMethod>
        Public Sub Caso2_ConRecinto()
            ' Il caso più comune: il modello incornicia la risposta in ```json ... ```
            Dim testo As String = "```json" & vbLf & JsonAtteso & vbLf & "```"
            VerificaContenuto(EstrattoreJson.Estrai(testo), "con recinto")
        End Sub

        <TestMethod>
        Public Sub Caso3_Preambolo()
            ' "Si mette a spiegare" prima del JSON: è il 502 chiuso nello Step 1.35.
            Dim testo As String = "Certo! Ecco il confronto richiesto:" & vbLf & vbLf & JsonAtteso
            VerificaContenuto(EstrattoreJson.Estrai(testo), "preambolo")
        End Sub

        <TestMethod>
        Public Sub Caso4_Coda()
            ' Commento aggiunto dopo il JSON.
            Dim testo As String = JsonAtteso & vbLf & vbLf & "Spero che l'analisi ti sia utile."
            VerificaContenuto(EstrattoreJson.Estrai(testo), "coda")
        End Sub

        <TestMethod>
        Public Sub Caso5_RecintoPiuPreambolo()
            ' I due disturbi insieme: il recinto non è più in testa, quindi la prima
            ' regex non lo toglie e il recupero deve passare per il ritaglio.
            Dim testo As String = "Ecco quanto richiesto:" & vbLf &
                                  "```json" & vbLf & JsonAtteso & vbLf & "```"
            VerificaContenuto(EstrattoreJson.Estrai(testo), "recinto + preambolo")
        End Sub

        <TestMethod>
        Public Sub Caso6_Spazzatura()
            ' Nessuna graffa in vista: non c'è niente da salvare e l'errore del
            ' percorso felice viene rilanciato, come nel prototipo.
            Dim testo As String = "Mi dispiace, non ho capito la richiesta."
            Assert.Throws(Of JsonException)(
                Sub() EstrattoreJson.Estrai(testo),
                "Caso «spazzatura»: doveva sollevare JsonException.")
        End Sub

    End Class

End Namespace
