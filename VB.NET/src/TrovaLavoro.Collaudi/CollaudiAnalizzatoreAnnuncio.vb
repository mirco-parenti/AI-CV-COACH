Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi dell'analizzatore dell'annuncio, il primo mestiere della pipeline di
    ''' candidatura (cap. 12). Girano <b>senza rete</b>, con lo stesso gestore HTTP finto
    ''' dei collaudi del client: la meccanica è già collaudata sotto
    ''' (<see cref="CollaudiMestiereAi"/>), qui si verifica ciò che è suo — il prompt
    ''' giusto, il testo dell'annuncio al posto giusto, e il suo nome nei messaggi.
    ''' </summary>
    <TestClass>
    Public Class CollaudiAnalizzatoreAnnuncio

        ''' <summary>Un annuncio corto ma vero nella forma, per i collaudi senza rete.</summary>
        Private Const AnnuncioDiProva As String =
            "Cercasi magazziniere per sede di Prato. Richiesto patentino muletto. " &
            "Gradita esperienza con gestionali di magazzino."

        ''' <summary>Una risposta dell'API che porta il testo indicato.</summary>
        Private Shared Function RispostaCon(testo As String) As String
            Return "{""model"":""claude-haiku-4-5"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":" &
                   JsonValue.Create(testo).ToJsonString() & "}]," &
                   """usage"":{""input_tokens"":10,""output_tokens"":5}}"
        End Function

        ''' <summary>L'analizzatore vero, col pool integrato e un'API finta dietro.</summary>
        Private Shared Function AnalizzatoreDiProva(finta As ApiFinta) As AnalizzatoreAnnuncio

            ' Pool integrato nell'eseguibile: si indica una cartella che non esiste.
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero

            Return New AnalizzatoreAnnuncio(libreria, client)

        End Function

        <TestMethod>
        Public Async Function AllAiArrivaIlPromptDellAnnuncioRiempito() As Task
            ' Il testo dell'annuncio deve finire dentro il suo prompt, e il resto della
            ' richiesta deve venire dai metadati del prompt (cap. 04): il mestiere non sa
            ' nulla di modelli né di limiti.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""titolo"":""Magazziniere""}")})

            Await AnalizzatoreDiProva(finta).AnalizzaAsync(AnnuncioDiProva)

            Dim corpo As JsonObject = CType(JsonNode.Parse(finta.UltimoCorpo), JsonObject)
            Dim mandato As String = corpo("messages")(0)("content").ToString()

            Assert.Contains("patentino muletto", mandato, "le parole dell'annuncio")
            Assert.Contains("ricavare dall'annuncio i requisiti", mandato, "il prompt dell'analisi")
            Assert.DoesNotContain("{{RISPOSTA_UTENTE}}", mandato, "nessun segnaposto rimasto")
            Assert.AreEqual("claude-haiku-4-5", corpo("model").ToString(), "il livello semplice del prompt")
            Assert.AreEqual(8000, CInt(corpo("max_tokens").GetValue(Of Integer)()), "il limite del prompt")
        End Function

        <TestMethod>
        Public Async Function LAnnuncioEsceGiaEstratto() As Task
            ' Il modello incornicia spesso il JSON in un recinto markdown: chi chiama
            ' deve ricevere l'annuncio, non il testo da sbucciare.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("```json" & vbLf &
                                     "{""titolo"": ""Magazziniere"", ""azienda"": ""Logistica Prato""}" & vbLf &
                                     "```")})

            Dim annuncio As JsonNode = Await AnalizzatoreDiProva(finta).AnalizzaAsync(AnnuncioDiProva)

            Assert.AreEqual("Magazziniere", annuncio("titolo").ToString(), "il titolo estratto")
            Assert.AreEqual("Logistica Prato", annuncio("azienda").ToString(), "e l'azienda del Pool 1.03")
        End Function

        <TestMethod>
        Public Async Function UnTestoVuotoNonArrivaAllAi() As Task
            ' La risposta la sappiamo già — lo schema vuoto — e non vale un'attesa né dei
            ' token: ci si ferma prima, dicendo all'utente cosa fare.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{}")})

            Dim errore As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() AnalizzatoreDiProva(finta).AnalizzaAsync("   " & vbLf & "  "))

            Assert.AreEqual(CausaErroreAi.Richiesta, errore.Causa, "è un errore nostro, non dell'API")
            Assert.Contains("incolla l'annuncio", errore.Message, "e dice cosa fare")
            Assert.AreEqual(0, finta.Chiamate, "senza aver chiamato l'AI")
        End Function

        <TestMethod>
        Public Async Function UnaRispostaIllegibileNominaLAnalisi() As Task
            ' L'etichetta del mestiere: l'utente deve leggere di che lavoro si parla, non
            ' l'identificativo di un file del pool.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("Non riesco ad analizzare questo annuncio.")})

            Dim errore As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() AnalizzatoreDiProva(finta).AnalizzaAsync(AnnuncioDiProva))

            Assert.AreEqual(CausaErroreAi.RispostaInattesa, errore.Causa, "causa")
            Assert.Contains("l'analisi dell'annuncio", errore.Message, "deve dire di quale lavoro si tratta")
            Assert.DoesNotContain("analisi_annuncio", errore.Message, "non il nome del file nel pool")
            Assert.Contains("Non riesco ad analizzare questo annuncio.", errore.Message,
                            "e riportare cosa ha risposto il modello")
        End Function

        ''' <summary>
        ''' L'unico collaudo di questa classe che chiama l'API vera. Verifica le due cose
        ''' che un annuncio inventato non può mostrare: che le <b>priorità</b> si
        ''' riconoscano dal senso della frase e non da una lista di parole, e che
        ''' l'<b>azienda</b> — il campo aggiunto col Pool 1.03 — arrivi davvero.
        ''' Categoria <b>Reale</b>: fuori dalla batteria di tutti i giorni, si lancia dove
        ''' c'è la chiave, da <c>VB.NET/src</c>, con
        ''' <c>dotnet test --settings TrovaLavoro.Collaudi/collaudi-reali.runsettings</c>.
        ''' </summary>
        <TestMethod, TestCategory("Reale")>
        Public Async Function LAnnuncioRealeDistingueRichiestoDaPreferenziale() As Task

            Dim chiave As String
            Try
                chiave = ClientClaude.ChiaveDaAmbiente()
            Catch ex As ErroreAi
                Assert.Inconclusive(
                    "Collaudo reale saltato: manca la chiave. Definisci " &
                    ClientClaude.NomeVariabileChiave & " nell'ambiente prima di lanciare il banco.")
                Return
            End Try

            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Using client As New ClientClaude(chiave)

                Dim analizzatore As New AnalizzatoreAnnuncio(libreria, client)

                Dim annuncio As JsonNode = Await analizzatore.AnalizzaAsync(
                    "Logistica Prato Srl ricerca un magazziniere per la sede di Prato." & vbLf &
                    "È richiesto il patentino per il muletto." & vbLf &
                    "Costituisce un plus la conoscenza dei gestionali di magazzino." & vbLf &
                    "Si offre contratto a tempo determinato, full time.")

                Assert.AreEqual("Logistica Prato Srl", annuncio("azienda").ToString(),
                                "l'azienda va riportata come la scrive l'annuncio")

                ' Il patentino è un paletto, il gestionale un desiderio: la differenza sta
                ' nel senso della frase («è richiesto» contro «costituisce un plus»).
                Dim priorita As New Dictionary(Of String, String)
                For Each lista As String In {"competenze_richieste", "esperienza_richiesta",
                                             "formazione_richiesta", "altri_requisiti"}
                    Dim voci As JsonArray = TryCast(annuncio(lista), JsonArray)
                    If voci Is Nothing Then Continue For
                    For Each voce As JsonNode In voci
                        priorita(voce("testo").ToString().ToLowerInvariant()) = voce("priorita").ToString()
                    Next
                Next

                Dim muletto As KeyValuePair(Of String, String) =
                    priorita.FirstOrDefault(Function(v) v.Key.Contains("muletto"))
                Assert.IsNotNull(muletto.Key, "il patentino deve essere fra i requisiti")
                Assert.AreEqual("richiesto", muletto.Value, "ed è un paletto")

                Dim gestionale As KeyValuePair(Of String, String) =
                    priorita.FirstOrDefault(Function(v) v.Key.Contains("gestional"))
                Assert.IsNotNull(gestionale.Key, "il gestionale deve essere fra i requisiti")
                Assert.AreEqual("preferenziale", gestionale.Value, "ed è un desiderio gradito")

            End Using

        End Function

    End Class

End Namespace
