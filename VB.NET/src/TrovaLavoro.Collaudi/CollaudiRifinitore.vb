Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi della passata anti-slop (T7b, cap. 08). Girano <b>senza rete</b>, con lo
    ''' stesso gestore HTTP finto degli altri mestieri: la meccanica comune è già collaudata
    ''' sotto (<see cref="CollaudiMestiereAi"/>), qui si verifica ciò che è suo — che ogni
    ''' <b>genere</b> peschi il suo prompt, che la <b>lingua</b> scelga la variante giusta, e
    ''' soprattutto che <b>nessun testo si perda</b> qualunque cosa risponda il modello.
    ''' </summary>
    ''' <remarks>
    ''' L'ultimo punto è il motivo per cui questa batteria esiste. Un rifinitore che
    ''' restituisce vuoto, che si dimentica un pezzo o che ne inventa uno non deve poter
    ''' cancellare una riga del CV di qualcuno: in tutti quei casi resta il testo di
    ''' partenza, che era già buono. La rifinitura è un miglioramento facoltativo, e da un
    ''' passaggio facoltativo un documento non può uscire peggiore di com'è entrato.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiRifinitore

        Private Shared Function RispostaCon(testo As String) As String
            Return "{""model"":""claude-sonnet-4-6"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":" &
                   JsonValue.Create(testo).ToJsonString() & "}]," &
                   """usage"":{""input_tokens"":10,""output_tokens"":5}}"
        End Function

        ''' <summary>Il rifinitore vero, col pool integrato e un'API finta dietro.</summary>
        Private Shared Function RifinitoreDiProva(finta As ApiFinta) As Rifinitore

            ' Pool integrato nell'eseguibile: si indica una cartella che non esiste.
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero

            Return New Rifinitore(libreria, client)

        End Function

        Private Shared Function Pezzo(id As String, testo As String) As PezzoDiProsa
            Return New PezzoDiProsa With {.Id = id, .Testo = testo}
        End Function

        ''' <summary>Il testo del messaggio utente che è partito davvero.</summary>
        Private Shared Function Mandato(finta As ApiFinta) As String
            Return JsonNode.Parse(finta.UltimoCorpo)("messages")(0)("content").ToString()
        End Function

        Private Shared Function ApiCheRisponde(pezzi As String) As ApiFinta
            Return New ApiFinta(New Passo With {
                .Corpo = RispostaCon("{""tipo"":""rifinitura"",""pezzi"":[" & pezzi & "]}")})
        End Function

        <TestMethod>
        Public Async Function OgniGenereChiamaIlSuoPrompt() As Task

            ' Il cuore della decisione di T7b: tre forme, tre prompt. Se il genere non
            ' scegliesse il file, le descrizioni delle esperienze verrebbero rifinite con le
            ' regole del sommario — e da frasi nominali diventerebbero frasi.
            For Each caso In {(GenereProsa.Sintesi, "Un sommario di CV"),
                              (GenereProsa.Frasi, "le descrizioni delle esperienze"),
                              (GenereProsa.Prosa, "il corpo di una lettera")}

                Dim finta As ApiFinta = ApiCheRisponde("{""id"":""x"",""testo"":""fatto""}")

                Await RifinitoreDiProva(finta).RifinisciAsync({Pezzo("x", "un testo")}, caso.Item1)

                Assert.Contains(caso.Item2, Mandato(finta),
                                $"il genere {caso.Item1} deve caricare il suo prompt")
            Next

        End Function

        <TestMethod>
        Public Async Function ChiedendoLIngleseParteIlPromptInglese() As Task

            Dim finta As ApiFinta = ApiCheRisponde("{""id"":""sommario"",""testo"":""done""}")

            Await RifinitoreDiProva(finta).RifinisciAsync(
                {Pezzo("sommario", "I have four years of warehouse experience.")},
                GenereProsa.Sintesi, Nothing, "en")

            Dim testo As String = Mandato(finta)

            Assert.Contains("A CV summary is a few first-person sentences", testo,
                            "il prompt inglese del sommario")
            Assert.Contains("em dash", testo, "coi tic dell'inglese, che non sono quelli dell'italiano")
            Assert.DoesNotContain("lineetta lunga", testo, "nessuna traccia della variante italiana")
            Assert.DoesNotContain("{{", testo, "nessun segnaposto rimasto")

        End Function

        <TestMethod>
        Public Async Function NelPromptEntraSoloIlTesto() As Task

            ' L'anti-invenzione qui prima ancora che scritta è strutturale: quel che non
            ' entra nella richiesta non può tornarne cambiato.
            Dim finta As ApiFinta = ApiCheRisponde("{""id"":""sommario"",""testo"":""ok""}")

            Await RifinitoreDiProva(finta).RifinisciAsync(
                {Pezzo("sommario", "Ho lavorato in magazzino.")}, GenereProsa.Sintesi)

            Dim testo As String = Mandato(finta)

            Assert.Contains("Ho lavorato in magazzino.", testo, "il testo da rifinire c'è")
            Assert.Contains("""id"": ""sommario""", testo, "con la sua etichetta")

        End Function

        <TestMethod>
        Public Async Function SenzaPezziNonSiDisturbaLAi() As Task

            ' Un CV senza esperienze descritte esiste. Chiedere all'AI di rifinire il nulla
            ' costerebbe un'attesa e dei token per sapere una cosa che si sa già.
            Dim finta As New ApiFinta()

            Dim esito As IReadOnlyDictionary(Of String, String) =
                Await RifinitoreDiProva(finta).RifinisciAsync(
                    {Pezzo("vuoto", "   "), Pezzo("", "senza id"), Nothing}, GenereProsa.Frasi)

            Assert.AreEqual(0, finta.Chiamate, "nessuna chiamata all'API")
            Assert.IsEmpty(esito, "e niente da rimettere a posto")

        End Function

        <TestMethod>
        Public Async Function UnPezzoDimenticatoDallAiRestaComEra() As Task

            ' Il ripiego che rende la rifinitura innocua: l'AI risponde solo per uno dei due.
            Dim finta As ApiFinta = ApiCheRisponde("{""id"":""a"",""testo"":""rifinito""}")

            Dim esito As IReadOnlyDictionary(Of String, String) =
                Await RifinitoreDiProva(finta).RifinisciAsync(
                    {Pezzo("a", "primo"), Pezzo("b", "secondo")}, GenereProsa.Frasi)

            Assert.AreEqual("rifinito", esito("a"), "quello tornato si usa")
            Assert.AreEqual("secondo", esito("b"), "quello dimenticato resta com'era")

        End Function

        <TestMethod>
        Public Async Function UnPezzoTornatoVuotoNonCancellaNiente() As Task

            Dim finta As ApiFinta = ApiCheRisponde(
                "{""id"":""a"",""testo"":""""},{""id"":""b"",""testo"":""   ""}")

            Dim esito As IReadOnlyDictionary(Of String, String) =
                Await RifinitoreDiProva(finta).RifinisciAsync(
                    {Pezzo("a", "primo"), Pezzo("b", "secondo")}, GenereProsa.Prosa)

            Assert.AreEqual("primo", esito("a"), "il vuoto non è una rifinitura")
            Assert.AreEqual("secondo", esito("b"), "e nemmeno gli spazi")

        End Function

        <TestMethod>
        Public Async Function UnIdInventatoDallAiSiScarta() As Task

            ' Un id che non abbiamo chiesto non ha un posto dove andare: accettarlo
            ' vorrebbe dire scrivere un testo in un campo scelto dal modello.
            Dim finta As ApiFinta = ApiCheRisponde(
                "{""id"":""a"",""testo"":""rifinito""},{""id"":""sommario"",""testo"":""intruso""}")

            Dim esito As IReadOnlyDictionary(Of String, String) =
                Await RifinitoreDiProva(finta).RifinisciAsync({Pezzo("a", "primo")}, GenereProsa.Sintesi)

            Assert.HasCount(1, esito, "torna una voce sola: quella chiesta")
            Assert.AreEqual("rifinito", esito("a"), "e quella è giusta")

        End Function

        <TestMethod>
        Public Async Function UnaRispostaSenzaListaLasciaTuttoComEra() As Task

            ' Cap. 02.5: mai un crash davanti a una risposta inattesa. Qui in più c'è che
            ' un documento non deve peggiorare per colpa di una risposta storta.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("{""tipo"":""rifinitura"",""esito"":""fatto""}")})

            Dim esito As IReadOnlyDictionary(Of String, String) =
                Await RifinitoreDiProva(finta).RifinisciAsync({Pezzo("a", "primo")}, GenereProsa.Prosa)

            Assert.AreEqual("primo", esito("a"), "il testo di partenza")

        End Function

    End Class

End Namespace
