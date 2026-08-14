Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace Ai

    ''' <summary>
    ''' Collaudi di chi riconosce i documenti di una cartella (cap. 05.2). Girano
    ''' <b>senza rete</b>, con lo stesso gestore HTTP finto degli altri mestieri: la
    ''' meccanica comune è già collaudata sotto (<see cref="CollaudiMestiereAi"/>), qui si
    ''' verifica ciò che è suo — che nel prompt finisca l'elenco dei file, e che si
    ''' distingua «assaggio vuoto» da «assaggio impossibile».
    ''' </summary>
    <TestClass>
    Public Class CollaudiClassificatoreDocumenti

        Private Shared Function RispostaCon(testo As String) As String
            Return "{""model"":""claude-haiku-4-5"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":" &
                   JsonValue.Create(testo).ToJsonString() & "}]," &
                   """usage"":{""input_tokens"":10,""output_tokens"":5}}"
        End Function

        Private Shared Function ClassificatoreDiProva(finta As ApiFinta) As ClassificatoreDocumenti

            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero

            Return New ClassificatoreDocumenti(libreria, client)

        End Function

        Private Shared Function Trovato(nome As String, Optional assaggio As String = Nothing) As FileTrovato

            Return New FileTrovato With {
                .Nome = nome,
                .Percorso = "C:\documenti\" & nome,
                .Modificato = New Date(2026, 3, 1, 9, 0, 0),
                .Dimensione = 340L * 1024L,
                .Assaggio = assaggio}

        End Function

        <TestMethod>
        Public Async Function AllAiArrivaLElencoDeiFileNelSuoPrompt() As Task

            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("{""tipo"":""classificazione_documenti"",""documenti"":[]}")})

            Await ClassificatoreDiProva(finta).ClassificaAsync({
                Trovato("CV_2025.pdf"),
                Trovato("lettera.txt", "Spettabile azienda, mi chiamo Mario")})

            Dim corpo As JsonObject = CType(JsonNode.Parse(finta.UltimoCorpo), JsonObject)
            Dim mandato As String = corpo("messages")(0)("content").ToString()

            Assert.Contains("CV_2025.pdf", mandato, "i nomi dei file")
            Assert.Contains("Spettabile azienda", mandato, "e l'assaggio di chi ce l'ha")
            Assert.Contains("340 KB", mandato, "la dimensione, come la scriverebbe una persona")
            Assert.Contains("2026-03-01", mandato, "e la data")
            Assert.Contains("li stai smistando", mandato, "il prompt della classificazione")
            Assert.DoesNotContain("{{DOCUMENTI}}", mandato, "nessun segnaposto rimasto")
            Assert.AreEqual("claude-haiku-4-5", corpo("model").ToString(), "il livello semplice del prompt")
        End Function

        <TestMethod>
        Public Async Function UnAssaggioCheNonCESiDichiara() As Task

            ' Un campo vuoto sembrerebbe un documento senza testo — cioè
            ' un'informazione — mentre vuol dire che il testo non si è potuto leggere.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("{""documenti"":[]}")})

            Await ClassificatoreDiProva(finta).ClassificaAsync({Trovato("Attestato.pdf")})

            Dim mandato As String = CType(JsonNode.Parse(finta.UltimoCorpo), JsonObject)(
                "messages")(0)("content").ToString()

            Assert.Contains("assaggio_non_disponibile", mandato, "lo si dichiara")
            Assert.DoesNotContain("""assaggio"":", mandato, "e non si finge di averlo")
        End Function

        <TestMethod>
        Public Async Function LaClassificazioneEsceGiaEstratta() As Task

            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("```json" & vbLf &
                                     "{""cv_piu_recente"": ""CV_2025.pdf"", ""documenti"": [" &
                                     "{""nome"": ""CV_2025.pdf"", ""categoria"": ""cv"", ""motivo"": ""ok""}]}" & vbLf &
                                     "```")})

            Dim risposta As JsonNode = Await ClassificatoreDiProva(finta).ClassificaAsync({Trovato("CV_2025.pdf")})

            Assert.AreEqual("CV_2025.pdf", risposta("cv_piu_recente").ToString(), "il CV più recente")
            Assert.AreEqual("cv", risposta("documenti")(0)("categoria").ToString(), "e la categoria")
        End Function

        <TestMethod>
        Public Async Function UnaCartellaSenzaNienteNonSiMandaAllAi() As Task

            ' Costerebbe un'attesa e dei token per farsi rispondere «non c'è nulla».
            Dim finta As New ApiFinta()

            Dim errore As ErroreAi = Await Assert.ThrowsExactlyAsync(Of ErroreAi)(
                Function() ClassificatoreDiProva(finta).ClassificaAsync(New FileTrovato() {}))

            Assert.AreEqual(0, finta.Chiamate, "l'AI non è stata disturbata")
            Assert.Contains("non ci sono file leggibili", errore.Message, "e si dice perché")
        End Function

    End Class

End Namespace
