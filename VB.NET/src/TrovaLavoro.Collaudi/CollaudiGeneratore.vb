Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi del generatore, il terzo mestiere della pipeline (cap. 12) e quello con
    ''' più prompt. Girano <b>senza rete</b>. La meccanica è già collaudata sotto
    ''' (<see cref="CollaudiMestiereAi"/>): qui si verifica ciò che è suo — che ognuno dei
    ''' tre documenti parta col suo prompt e con <b>tutti</b> i suoi artefatti, che i tre
    ''' non si confondano fra loro, e che l'unico ingresso facoltativo resti facoltativo.
    ''' </summary>
    <TestClass>
    Public Class CollaudiGeneratore

        ''' <summary>Il 🎯 CV mirato già generato, come lo riceverebbe la lettera.</summary>
        Private Shared Function CvMirato() As JsonNode
            Return JsonNode.Parse(
                "{""intestazione"": {""nome"": ""Luca Ferrari""}," &
                """sintesi"": ""Sintesi mirata sul ruolo di magazziniere""}")
        End Function

        ''' <summary>I ponti onesti sui gap, come li restituisce la mitigazione.</summary>
        Private Shared Function Mitigazioni() As JsonNode
            Return JsonNode.Parse(
                "[{""requisito_gap"": ""Patente C indispensabile""," &
                """elemento_profilo"": ""Patente B usata ogni giorno per le consegne""," &
                """ponte"": ""Non ha la C, ma guida per lavoro da anni""}]")
        End Function

        ''' <summary>Una risposta dell'API che porta il testo indicato.</summary>
        Private Shared Function RispostaCon(testo As String) As String
            Return "{""model"":""claude-sonnet-5"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":" &
                   JsonValue.Create(testo).ToJsonString() & "}]," &
                   """usage"":{""input_tokens"":10,""output_tokens"":5}}"
        End Function

        ''' <summary>Il generatore vero, col pool integrato e un'API finta dietro.</summary>
        Private Shared Function GeneratoreDiProva(finta As ApiFinta) As Generatore

            ' Pool integrato nell'eseguibile: si indica una cartella che non esiste.
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero

            Return New Generatore(libreria, client)

        End Function

        ''' <summary>Il testo del messaggio utente che è partito davvero.</summary>
        Private Shared Function Mandato(finta As ApiFinta) As String
            Return JsonNode.Parse(finta.UltimoCorpo)("messages")(0)("content").ToString()
        End Function

        <TestMethod>
        Public Async Function AlCvBaseArrivaSoloIlProfilo() As Task

            ' 📄 CV-1: nasce dopo l'anello 1, quando un annuncio non c'è ancora. Se ci
            ' finisse dentro un annuncio, non sarebbe più il CV base.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""intestazione"":{}}")})

            Await GeneratoreDiProva(finta).GeneraCvBaseAsync(CasiDiCollaudo.Profilo())

            Dim corpo As JsonObject = CType(JsonNode.Parse(finta.UltimoCorpo), JsonObject)
            Dim testo As String = corpo("messages")(0)("content").ToString()

            Assert.Contains("genera in formato JSON un CV a partire dal profilo", testo,
                            "il prompt del CV base")
            Assert.Contains("Luca Ferrari", testo, "il profilo")
            Assert.DoesNotContain("<annuncio>", testo, "nessun annuncio: è il CV base")
            Assert.DoesNotContain("{{PROFILO}}", testo, "nessun segnaposto rimasto")

            Assert.AreEqual("claude-sonnet-5", corpo("model").ToString(), "il livello di ragionamento")
            Assert.AreEqual(16000, CInt(corpo("max_tokens").GetValue(Of Integer)()), "il limite del prompt")

        End Function

        <TestMethod>
        Public Async Function AlCvMiratoArrivanoIlProfiloLAnnuncioEIGiudizi() As Task

            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""intestazione"":{}}")})

            Await GeneratoreDiProva(finta).GeneraCvMiratoAsync(
                CasiDiCollaudo.Profilo(),
                CasiDiCollaudo.Annuncio(CasiDiCollaudo.Eliminatorio),
                CasiDiCollaudo.Giudizi(CasiDiCollaudo.Eliminatorio))

            Dim testo As String = Mandato(finta)

            Assert.Contains("CV mirato a uno specifico annuncio", testo, "il prompt del CV mirato")
            Assert.Contains("Luca Ferrari", testo, "il profilo")
            Assert.Contains("Patente C", testo, "l'annuncio coi suoi requisiti")
            Assert.Contains("""esito"":", testo, "e i giudizi dell'anello 3")
            Assert.DoesNotContain("{{", testo, "nessun segnaposto rimasto")

        End Function

        <TestMethod>
        Public Async Function AllaLetteraArrivanoTuttiECinqueGliArtefatti() As Task

            ' È il prompt con più ingressi di tutto il progetto: dimenticarne uno non
            ' farebbe fallire nulla, produrrebbe soltanto una lettera più povera.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""corpo"":""...""}")})

            Await GeneratoreDiProva(finta).GeneraLetteraAsync(
                CasiDiCollaudo.Profilo(),
                CasiDiCollaudo.Annuncio(CasiDiCollaudo.Eliminatorio),
                CasiDiCollaudo.Giudizi(CasiDiCollaudo.Eliminatorio),
                CvMirato(), Mitigazioni())

            Dim corpo As JsonObject = CType(JsonNode.Parse(finta.UltimoCorpo), JsonObject)
            Dim testo As String = corpo("messages")(0)("content").ToString()

            Assert.Contains("lettera di presentazione", testo, "il prompt della lettera")
            Assert.Contains("Romagna Logistica", testo, "1/5 il profilo")
            Assert.Contains("Patente C indispensabile", testo, "2/5 l'annuncio")
            Assert.Contains("""spiegazione"":", testo, "3/5 i giudizi")
            Assert.Contains("Sintesi mirata sul ruolo di magazziniere", testo, "4/5 il CV mirato")
            Assert.Contains("Patente B usata ogni giorno", testo, "5/5 le mitigazioni")
            Assert.DoesNotContain("{{", testo, "nessun segnaposto rimasto")

            Assert.AreEqual(4000, CInt(corpo("max_tokens").GetValue(Of Integer)()), "il limite del suo prompt")

        End Function

        <TestMethod>
        Public Async Function ITestiRiscrittiAManoArrivanoAllaLetteraNelLoroBlocco() As Task

            ' R7: è l'unico blocco, oltre al profilo, che il prompt della lettera tratta
            ' come fonte di fatti — quelle parole non le ha scritte un modello, le ha
            ' scritte la persona. Se si fermassero per strada, la lettera continuerebbe a
            ' raccontare la storia di prima.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""corpo"":""...""}")})

            Await GeneratoreDiProva(finta).GeneraLetteraAsync(
                CasiDiCollaudo.Profilo(),
                CasiDiCollaudo.Annuncio(CasiDiCollaudo.Compatibile),
                CasiDiCollaudo.Giudizi(CasiDiCollaudo.Compatibile),
                CvMirato(), Nothing, Nothing, "it", Nothing,
                JsonNode.Parse("[{""campo"": ""Sommario"", ""testo"": ""Ho traslocato elefanti.""}]"))

            Assert.Contains("Ho traslocato elefanti.", Mandato(finta), "il testo dell'utente arriva")
            Assert.Contains("<riscritture>", Mandato(finta), "dentro il blocco che lo dichiara suo")

        End Function

        <TestMethod>
        Public Async Function SenzaRiscrittureIlBloccoArrivaVuoto() As Task

            ' Il caso normale, ed è la maggioranza: un CV uscito tutto dall'AI. Il blocco
            ' c'è ma è vuoto — la stessa regola delle mitigazioni e degli appunti, perché
            ' un segnaposto senza valore fa fallire la richiesta prima ancora di partire.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""corpo"":""...""}")})

            Await GeneratoreDiProva(finta).GeneraLetteraAsync(
                CasiDiCollaudo.Profilo(),
                CasiDiCollaudo.Annuncio(CasiDiCollaudo.Compatibile),
                CasiDiCollaudo.Giudizi(CasiDiCollaudo.Compatibile),
                CvMirato(), Nothing)

            Assert.Contains("<riscritture>" & vbLf & "[]" & vbLf & "</riscritture>", Mandato(finta),
                            "col blocco vuoto invece che assente")

        End Function

        <TestMethod>
        Public Async Function SenzaMitigazioniLaLetteraSiScriveLoStesso() As Task

            ' Le mitigazioni sono l'unico ingresso facoltativo: la lista vuota è un esito
            ' legittimo del prompt precedente, e una lettera che non si scrive per questo
            ' sarebbe un difetto. Al prompt arriva [], come fa il prototipo.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""corpo"":""...""}")})

            Await GeneratoreDiProva(finta).GeneraLetteraAsync(
                CasiDiCollaudo.Profilo(),
                CasiDiCollaudo.Annuncio(CasiDiCollaudo.Compatibile),
                CasiDiCollaudo.Giudizi(CasiDiCollaudo.Compatibile),
                CvMirato(), Nothing)

            Assert.AreEqual(1, finta.Chiamate, "la lettera si genera lo stesso")
            Assert.Contains("<mitigazioni>" & vbLf & "[]" & vbLf & "</mitigazioni>", Mandato(finta),
                            "col blocco vuoto invece che assente")

        End Function

        <TestMethod>
        Public Async Function SenzaIlCvMiratoLaLetteraNonParte() As Task

            ' Il CV, invece, non è facoltativo: senza di lui la lettera racconterebbe una
            ' storia diversa da quella del CV allegato, ed è proprio ciò che deve evitare.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{}")})

            Dim errore As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() GeneratoreDiProva(finta).GeneraLetteraAsync(
                    CasiDiCollaudo.Profilo(),
                    CasiDiCollaudo.Annuncio(CasiDiCollaudo.Compatibile),
                    CasiDiCollaudo.Giudizi(CasiDiCollaudo.Compatibile),
                    Nothing, Mitigazioni()))

            Assert.AreEqual(CausaErroreAi.Richiesta, errore.Causa, "è un errore nostro, non dell'API")
            Assert.Contains("il CV mirato", errore.Message, "deve dire cosa manca")
            Assert.AreEqual(0, finta.Chiamate, "senza aver chiamato l'AI")

        End Function

        <TestMethod>
        Public Async Function ITreLavoriSiNominanoInModoDiverso() As Task

            ' Come per il confrontatore: con più prompt sotto lo stesso mestiere, il
            ' rischio è che l'utente si veda nominare il lavoro sbagliato.
            Dim finta As New ApiFinta(
                New Passo With {.Corpo = RispostaCon("non è JSON")},
                New Passo With {.Corpo = RispostaCon("non è JSON")},
                New Passo With {.Corpo = RispostaCon("non è JSON")})

            Dim mestiere As Generatore = GeneratoreDiProva(finta)

            Dim profilo As JsonNode = CasiDiCollaudo.Profilo()
            Dim annuncio As JsonNode = CasiDiCollaudo.Annuncio(CasiDiCollaudo.Compatibile)
            Dim giudizi As JsonNode = CasiDiCollaudo.Giudizi(CasiDiCollaudo.Compatibile)

            Dim suBase As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() mestiere.GeneraCvBaseAsync(profilo))

            Dim suMirato As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() mestiere.GeneraCvMiratoAsync(profilo, annuncio, giudizi))

            Dim suLettera As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() mestiere.GeneraLetteraAsync(profilo, annuncio, giudizi, CvMirato(), Nothing))

            Assert.Contains("la generazione del CV base", suBase.Message, "il primo lavoro")
            Assert.DoesNotContain("mirato", suBase.Message, "e non gli altri")

            Assert.Contains("la generazione del CV mirato", suMirato.Message, "il secondo lavoro")
            Assert.DoesNotContain("lettera", suMirato.Message, "e non gli altri")

            Assert.Contains("la generazione della lettera", suLettera.Message, "il terzo lavoro")
            Assert.DoesNotContain("CV base", suLettera.Message, "e non gli altri")

        End Function

    End Class

End Namespace
