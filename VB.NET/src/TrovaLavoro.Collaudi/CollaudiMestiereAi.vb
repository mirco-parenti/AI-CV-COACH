Imports System.IO
Imports System.Net.Http
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi della meccanica comune ai mestieri AI (<see cref="MestiereAi"/>).
    ''' Girano <b>senza rete</b>, con lo stesso gestore HTTP finto dei collaudi del
    ''' client. Quello che si verifica qui non è come risponde l'AI, ma le due cose che
    ''' un pezzo condiviso rischia di rovinare per tutti insieme: che i messaggi
    ''' d'errore restino quelli giusti — sono testo che l'utente legge, non log — e che
    ''' un mestiere con più segnaposti li porti tutti fino all'AI.
    ''' </summary>
    <TestClass>
    Public Class CollaudiMestiereAi

        ''' <summary>Una risposta dell'API che porta il testo indicato.</summary>
        Private Shared Function RispostaCon(testo As String) As String
            Return "{""model"":""claude-haiku-4-5"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":" &
                   JsonValue.Create(testo).ToJsonString() & "}]," &
                   """usage"":{""input_tokens"":10,""output_tokens"":5}}"
        End Function

        ''' <summary>Un mestiere qualunque, col pool integrato e un'API finta dietro.</summary>
        Private Shared Function CreaMestiere(finta As ApiFinta) As MestiereDiProva

            ' Pool integrato nell'eseguibile: si indica una cartella che non esiste.
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero

            Return New MestiereDiProva(libreria, client)

        End Function

        <TestMethod>
        Public Async Function TuttiISegnapostiArrivanoNelMandato() As Task
            ' I mestieri di T4 non hanno più un segnaposto solo come i turni: il
            ' confronto ne vuole due, la lettera cinque. Devono arrivare tutti, e il
            ' frammento deve uscire di qui già estratto.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("```json" & vbLf & "{""match_totale"": 72}" & vbLf & "```")})

            Dim frammento As JsonNode = Await CreaMestiere(finta).ProvaAsync(
                "confronto", "il confronto col profilo",
                New Dictionary(Of String, String) From {
                    {"PROFILO", "{""nome"":""Luca Ferrari""}"},
                    {"ANNUNCIO", "{""ruolo"":""Magazziniere""}"}})

            Assert.AreEqual(72, frammento("match_totale").GetValue(Of Integer)(), "il frammento estratto")

            Dim mandato As String = JsonNode.Parse(finta.UltimoCorpo)("messages")(0)("content").ToString()
            Assert.Contains("Luca Ferrari", mandato, "il profilo")
            Assert.Contains("Magazziniere", mandato, "e l'annuncio")
            Assert.DoesNotContain("{{PROFILO}}", mandato, "nessun segnaposto rimasto")
            Assert.DoesNotContain("{{ANNUNCIO}}", mandato, "nessun segnaposto rimasto")
        End Function

        <TestMethod>
        Public Async Function UnDatoCheMancaNonAccusaIlPool() As Task
            ' Un segnaposto senza valore ha due origini che dall'esterno non si
            ' distinguono — un dato che il mestiere ha dimenticato, o un segnaposto
            ' aggiunto a mano nel pool — e nessuna delle due è «il prompt non è
            ' utilizzabile»: il pool, qui, è quello integrato e sta benissimo.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{}")})

            Dim errore As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() CreaMestiere(finta).ProvaAsync(
                    "confronto", "il confronto col profilo",
                    New Dictionary(Of String, String) From {{"PROFILO", "{}"}}))

            Assert.AreEqual(CausaErroreAi.Richiesta, errore.Causa, "è un errore nostro, non dell'API")
            Assert.Contains("il confronto col profilo", errore.Message, "deve dire di quale lavoro si tratta")
            Assert.Contains("ANNUNCIO", errore.Message, "e quale dato manca")
            Assert.DoesNotContain("non è utilizzabile", errore.Message,
                                  "senza accusare il pool, che è a posto")
            Assert.AreEqual(0, finta.Chiamate, "e non deve aver chiamato l'AI")
        End Function

        <TestMethod>
        Public Async Function LErroreDelClientArrivaConLeSueParole() As Task
            ' Il vincolo che tiene in piedi la voce dell'applicazione: gli ErroreAi che
            ' salgono dal client sono già scritti in italiano per l'utente (cap. 02.5) e
            ' devono attraversare la meccanica comune intatti. Se un giorno il Catch qui
            ' dentro si allargasse a Exception, l'utente smetterebbe di leggere
            ' «controlla la connessione» e questo collaudo diventerebbe rosso.
            Dim finta As New ApiFinta(
                New Passo With {.Eccezione = New HttpRequestException("no such host")},
                New Passo With {.Eccezione = New HttpRequestException("no such host")})

            Dim errore As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() CreaMestiere(finta).ProvaAsync(
                    "cv_base", "la generazione del CV base",
                    New Dictionary(Of String, String) From {{"PROFILO", "{}"}}))

            Assert.AreEqual(CausaErroreAi.Rete, errore.Causa, "la causa del client, non riscritta")
            Assert.Contains("connessione", errore.Message, "e le sue parole")
            Assert.DoesNotContain("la generazione del CV base", errore.Message,
                                  "nessun involucro nostro attorno")
        End Function

        <TestMethod>
        Public Async Function UnLivelloDiModelloScrittoMaleNonFaCadereIlMestiere() As Task
            ' Il pool esterno è modificabile per design (cap. 04.2): un «modello:
            ' fantasia» scritto a mano deve fermarsi qui come messaggio leggibile. È
            ' l'inciampo che il trascrittore del PDF, prima della meccanica comune,
            ' lasciava passare come eccezione grezza.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{}")})

            Dim rotto As New Prompt With {
                .Id = "inventato", .Modello = "fantasia", .MaxToken = 1000,
                .Uscita = "json", .Corpo = "Testo.", .Segnaposto = Array.Empty(Of String)()}

            Dim errore As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() CreaMestiere(finta).ChiediProvaAsync(rotto))

            Assert.AreEqual(CausaErroreAi.Richiesta, errore.Causa, "è un errore nostro, non dell'API")
            Assert.Contains("inventato", errore.Message, "deve nominare il prompt")
            Assert.Contains("fantasia", errore.Message, "e il livello che non esiste")
            Assert.AreEqual(0, finta.Chiamate, "senza aver chiamato l'AI")
        End Function

    End Class

    ''' <summary>
    ''' Un mestiere che non esiste nel prodotto: serve solo a mettere alla prova la
    ''' meccanica comune da fuori, come farà ognuno dei mestieri veri.
    ''' </summary>
    Friend Class MestiereDiProva
        Inherits MestiereAi

        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)
            MyBase.New(libreria, client)
        End Sub

        ''' <summary>La fila intera, esposta al banco.</summary>
        Public Function ProvaAsync(idPrompt As String, etichetta As String,
                                   valori As IDictionary(Of String, String)) As Task(Of JsonNode)
            Return EseguiAsync(idPrompt, etichetta, valori)
        End Function

        ''' <summary>Il solo passo della chiamata, per provare un prompt scritto male.</summary>
        Public Function ChiediProvaAsync(prompt As Prompt) As Task(Of RispostaAi)
            Return ChiediAsync(prompt, JsonValue.Create("ciao"))
        End Function

    End Class

End Namespace
