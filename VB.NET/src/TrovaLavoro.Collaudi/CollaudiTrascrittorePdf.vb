Imports System.IO
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi del trascrittore dei PDF (cap. 05.1). Girano <b>senza rete</b>, con la
    ''' stessa API finta dei collaudi del client: qui non si verifica come legge il
    ''' modello — quello lo dice il collaudo reale — ma che la richiesta che parte abbia
    ''' la forma del prototipo, perché è quella la forma già validata sui CV veri,
    ''' comprese le due colonne (step 1.36).
    ''' </summary>
    <TestClass>
    Public Class CollaudiTrascrittorePdf

        ''' <summary>I byte che fanno da PDF: al trascrittore basta che siano dei byte.</summary>
        Private Shared ReadOnly BytePdf As Byte() = Encoding.ASCII.GetBytes("%PDF-1.7 finto")

        ''' <summary>Una risposta dell'API che porta il testo indicato.</summary>
        Private Shared Function RispostaCon(testo As String) As String
            Return "{""model"":""claude-haiku-4-5"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":" &
                   JsonValue.Create(testo).ToJsonString() & "}]," &
                   """usage"":{""input_tokens"":10,""output_tokens"":5}}"
        End Function

        ''' <summary>Il trascrittore vero, col pool integrato e un'API finta dietro.</summary>
        Private Shared Function TrascrittoreDiProva(finta As ApiFinta) As TrascrittorePdf

            ' Pool integrato nell'eseguibile: si indica una cartella che non esiste.
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero

            Return New TrascrittorePdf(libreria, client)

        End Function

        <TestMethod>
        Public Async Function IlPdfPartePrimaDellIstruzione() As Task
            ' La forma del prototipo: il messaggio ha due blocchi, il documento e poi il
            ' prompt di trascrizione. L'ordine non è un dettaglio — è quello con cui il
            ' metodo è stato validato — e il resto della richiesta viene dai metadati del
            ' prompt, non da numeri scritti qui (cap. 04).
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("Mario Rossi, magazziniere")})

            Await ConPdfDiProva(Function(percorso) TrascrittoreDiProva(finta).TrascriviAsync(percorso))

            Dim corpo As JsonObject = CType(JsonNode.Parse(finta.UltimoCorpo), JsonObject)
            Dim blocchi As JsonArray = CType(corpo("messages")(0)("content"), JsonArray)

            Assert.HasCount(2, blocchi, "documento e istruzione")
            Assert.AreEqual("document", blocchi(0)("type").ToString(), "prima il documento")
            Assert.AreEqual("base64", blocchi(0)("source")("type").ToString(), "mandato in base64")
            Assert.AreEqual("application/pdf", blocchi(0)("source")("media_type").ToString(), "dichiarato PDF")
            Assert.AreEqual(Convert.ToBase64String(BytePdf), blocchi(0)("source")("data").ToString(),
                            "e sono proprio i byte del file")

            Assert.AreEqual("text", blocchi(1)("type").ToString(), "poi l'istruzione")
            Assert.Contains("trascrive fedelmente", blocchi(1)("text").ToString(),
                            "che è il prompt trascrizione_pdf del pool")

            Assert.AreEqual("claude-haiku-4-5", corpo("model").ToString(), "il livello semplice del prompt")
            ' Il limite è quello del prompt, e dal Pool 1.03 non è più quello del
            ' prototipo: a 4000 token un CV di venti pagine si troncava a metà.
            Assert.AreEqual(32000, CInt(corpo("max_tokens").GetValue(Of Integer)()),
                            "il limite del prompt")
        End Function

        <TestMethod>
        Public Async Function TornaIlTestoTrascrittoCosiComE() As Task
            ' Il prompt dichiara «uscita: testo»: qui non c'è JSON da sbucciare, e il
            ' testo non va toccato — è la trascrizione fedele del CV.
            Dim trascritto As String = "MARIO ROSSI" & vbLf & "Magazziniere" & vbLf & "Forlì"
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon(trascritto)})

            Dim uscita As String = Await ConPdfDiProva(
                Function(percorso) TrascrittoreDiProva(finta).TrascriviAsync(percorso))

            Assert.AreEqual(trascritto, uscita, "il testo trascritto, intero")
        End Function

        <TestMethod>
        Public Async Function UnFileCheNonCEsisteNonDiventaUnaChiamata() As Task
            ' Meglio accorgersene sul disco che dopo aver aperto una connessione: un
            ' tentativo sbagliato costa comunque tempo e token.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("...")})

            Await Assert.ThrowsAsync(Of FileNotFoundException)(
                Function() TrascrittoreDiProva(finta).TrascriviAsync(
                    Path.Combine(Path.GetTempPath(), "cv-che-non-esiste.pdf")))

            Assert.AreEqual(0, finta.Chiamate, "e l'AI non deve essere stata chiamata")
        End Function

        ''' <summary>Fa girare la prova su un PDF finto, e poi lo toglie.</summary>
        Private Shared Async Function ConPdfDiProva(prova As Func(Of String, Task(Of String))) As Task(Of String)

            Dim percorso As String = Path.Combine(
                Path.GetTempPath(), "cv-" & Guid.NewGuid().ToString("N") & ".pdf")

            File.WriteAllBytes(percorso, BytePdf)
            Try
                Return Await prova(percorso)
            Finally
                If File.Exists(percorso) Then File.Delete(percorso)
            End Try

        End Function

    End Class

End Namespace
