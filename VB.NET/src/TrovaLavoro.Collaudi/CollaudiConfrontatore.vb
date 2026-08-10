Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi del confrontatore, il secondo mestiere della pipeline (cap. 12) e
    ''' l'unico che porta due prompt. Girano <b>senza rete</b>. La meccanica è già
    ''' collaudata sotto (<see cref="CollaudiMestiereAi"/>): qui si verifica ciò che è
    ''' suo — che ognuno dei due lavori parta col prompt e i segnaposti giusti, che i due
    ''' non si confondano fra loro, e che gli artefatti entrino nel prompt <b>come nel
    ''' prototipo</b>, che su questi due è ancora il metro (cap. 04.7).
    ''' </summary>
    <TestClass>
    Public Class CollaudiConfrontatore

        ''' <summary>Una risposta dell'API che porta il testo indicato.</summary>
        Private Shared Function RispostaCon(testo As String) As String
            Return "{""model"":""claude-sonnet-4-6"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":" &
                   JsonValue.Create(testo).ToJsonString() & "}]," &
                   """usage"":{""input_tokens"":10,""output_tokens"":5}}"
        End Function

        ''' <summary>Il confrontatore vero, col pool integrato e un'API finta dietro.</summary>
        Private Shared Function ConfrontatoreDiProva(finta As ApiFinta) As Confrontatore

            ' Pool integrato nell'eseguibile: si indica una cartella che non esiste.
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero

            Return New Confrontatore(libreria, client)

        End Function

        ''' <summary>Il testo del messaggio utente che è partito davvero.</summary>
        Private Shared Function Mandato(finta As ApiFinta) As String
            Return JsonNode.Parse(finta.UltimoCorpo)("messages")(0)("content").ToString()
        End Function

        <TestMethod>
        Public Async Function AlConfrontoArrivanoProfiloEAnnuncio() As Task

            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""giudizi"":[]}")})

            Await ConfrontatoreDiProva(finta).ConfrontaAsync(
                CasiDiCollaudo.Profilo(), CasiDiCollaudo.Annuncio(CasiDiCollaudo.Compatibile))

            Dim corpo As JsonObject = CType(JsonNode.Parse(finta.UltimoCorpo), JsonObject)
            Dim testo As String = corpo("messages")(0)("content").ToString()

            Assert.Contains("confronta un profilo professionale", testo, "il prompt del confronto")
            Assert.DoesNotContain("{{PROFILO}}", testo, "nessun segnaposto rimasto")
            Assert.DoesNotContain("{{ANNUNCIO}}", testo, "nessun segnaposto rimasto")

            ' Gli artefatti devono entrare come li scriveva il prototipo: due spazi di
            ' rientro e accenti lasciati lettere. È la condizione della parità (cap. 04.7).
            Assert.Contains("  ""nome"":", testo, "il profilo indentato come JSON.stringify(x, null, 2)")
            Assert.Contains("Forlì", testo, "e con gli accenti in chiaro")

            ' Livello e limite vengono dai metadati del prompt, non dal mestiere.
            Assert.AreEqual("claude-sonnet-4-6", corpo("model").ToString(), "il livello di ragionamento")
            Assert.AreEqual(16000, CInt(corpo("max_tokens").GetValue(Of Integer)()), "il limite del prompt")

        End Function

        <TestMethod>
        Public Async Function AllaMitigazioneArrivanoProfiloEGiudizi() As Task

            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""mitigazioni"":[]}")})

            Await ConfrontatoreDiProva(finta).MitigaAsync(
                CasiDiCollaudo.Profilo(), CasiDiCollaudo.Giudizi(CasiDiCollaudo.Eliminatorio))

            Dim corpo As JsonObject = CType(JsonNode.Parse(finta.UltimoCorpo), JsonObject)
            Dim testo As String = corpo("messages")(0)("content").ToString()

            Assert.Contains("argomenti di MITIGAZIONE", testo, "il prompt della mitigazione")
            Assert.Contains("Patente C", testo, "i giudizi, coi gap da mitigare")
            Assert.DoesNotContain("{{GIUDIZI}}", testo, "nessun segnaposto rimasto")
            Assert.AreEqual(8000, CInt(corpo("max_tokens").GetValue(Of Integer)()), "il limite del suo prompt")

        End Function

        <TestMethod>
        Public Async Function IGiudiziEsconoGiaEstratti() As Task

            ' Il modello incornicia spesso il JSON in un recinto markdown.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("```json" & vbLf &
                                     "{""giudizi"": [{""esito"": ""soddisfatto""}], " &
                                     """numero_complessivo"": 88}" & vbLf & "```")})

            Dim esito As JsonNode = Await ConfrontatoreDiProva(finta).ConfrontaAsync(
                CasiDiCollaudo.Profilo(), CasiDiCollaudo.Annuncio(CasiDiCollaudo.Compatibile))

            Assert.AreEqual("soddisfatto", esito("giudizi")(0)("esito").ToString(), "il giudizio estratto")
            Assert.AreEqual(88, esito("numero_complessivo").GetValue(Of Integer)(), "e il numero d'insieme")

        End Function

        <TestMethod>
        Public Async Function UnaListaVuotaDiMitigazioniEUnEsitoLegittimo() As Task

            ' «Se nessun gap è mitigabile, mitigazioni è una lista vuota»: tacere è
            ' l'esito corretto del prompt, non un fallimento da tradurre in errore.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""mitigazioni"": []}")})

            Dim esito As JsonNode = Await ConfrontatoreDiProva(finta).MitigaAsync(
                CasiDiCollaudo.Profilo(), CasiDiCollaudo.Giudizi(CasiDiCollaudo.Compatibile))

            Assert.IsEmpty(CType(esito("mitigazioni"), JsonArray), "la lista vuota deve arrivare così com'è")

        End Function

        <TestMethod>
        Public Async Function SenzaUnArtefattoNonSiChiamaLAi() As Task

            ' Un artefatto che manca diventerebbe un «null» scritto nel prompt, e l'AI
            ' risponderebbe a vuoto: meglio fermarsi, e dirlo.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{}")})

            Dim errore As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() ConfrontatoreDiProva(finta).ConfrontaAsync(CasiDiCollaudo.Profilo(), Nothing))

            Assert.AreEqual(CausaErroreAi.Richiesta, errore.Causa, "è un errore nostro, non dell'API")
            Assert.Contains("l'annuncio", errore.Message, "deve dire cosa manca")
            Assert.AreEqual(0, finta.Chiamate, "senza aver chiamato l'AI")

        End Function

        <TestMethod>
        Public Async Function IDueLavoriSiNominanoInModoDiverso() As Task

            ' Il rischio di un mestiere con due prompt: che l'utente si veda nominare il
            ' lavoro sbagliato. L'etichetta di ciascuno deve essere la sua.
            Dim finta As New ApiFinta(
                New Passo With {.Corpo = RispostaCon("non è JSON")},
                New Passo With {.Corpo = RispostaCon("non è JSON")})

            Dim mestiere As Confrontatore = ConfrontatoreDiProva(finta)

            Dim suConfronto As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() mestiere.ConfrontaAsync(
                    CasiDiCollaudo.Profilo(), CasiDiCollaudo.Annuncio(CasiDiCollaudo.Compatibile)))

            Dim suMitigazione As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() mestiere.MitigaAsync(
                    CasiDiCollaudo.Profilo(), CasiDiCollaudo.Giudizi(CasiDiCollaudo.Compatibile)))

            Assert.Contains("il confronto col profilo", suConfronto.Message, "il primo lavoro")
            Assert.DoesNotContain("mitigazioni", suConfronto.Message, "e non l'altro")

            Assert.Contains("la ricerca delle mitigazioni", suMitigazione.Message, "il secondo lavoro")
            Assert.DoesNotContain("confronto col profilo", suMitigazione.Message, "e non l'altro")

        End Function

    End Class

End Namespace
