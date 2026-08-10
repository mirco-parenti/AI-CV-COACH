Imports System.IO
Imports System.Linq
Imports System.Net
Imports System.Text.Json.Nodes
Imports System.Text.RegularExpressions
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti

Namespace Documenti

    ''' <summary>
    ''' Collaudi della pagina HTML da cui nasce il PDF (cap. 05.5). Il PDF vero vuole il
    ''' motore di Windows e sta fra i collaudi reali; la pagina, invece, è una stringa —
    ''' e quasi tutto quello che può andare storto va storto qui.
    ''' </summary>
    ''' <remarks>
    ''' L'ultimo collaudo di questa classe è quello che vale di più: mette il DOCX e
    ''' l'HTML uno accanto all'altro e verifica che <b>dicano le stesse cose nello stesso
    ''' ordine</b>. È la promessa del cap. 05.3 — «un solo modello di contenuto, più
    ''' stampanti» — messa alla prova invece che dichiarata.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiScrittoreHtml

        Private Shared Function CvDiProva() As JsonObject

            Return CType(JsonNode.Parse("
                {
                  ""tipo"": ""cv_base"",
                  ""intestazione"": {
                    ""nome"": ""Luca Ferrari"", ""email"": ""luca.ferrari@example.it"",
                    ""telefono"": ""333 1234567"", ""citta"": ""Modena"",
                    ""link"": ""linkedin.com/in/lucaferrari"", ""patente"": ""B""
                  },
                  ""sommario"": ""Ho esperienza nel servizio di sala."",
                  ""esperienze_professionali"": [
                    { ""ruolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino"",
                      ""durata"": ""2019-2023"", ""descrizione"": ""Servizio ai tavoli e cassa."" }
                  ],
                  ""altre_esperienze"": [
                    { ""descrizione"": ""Volontariato alla sagra"", ""quando"": ""estate 2021"" }
                  ],
                  ""competenze"": [""Servizio ai tavoli"", ""Uso del muletto""],
                  ""formazione"": [
                    { ""titolo"": ""Diploma alberghiero"", ""istituto"": ""IPSSAR Modena"", ""anno"": ""2018"" }
                  ]
                }"), JsonObject)

        End Function

        ''' <summary>Il testo che si legge nella pagina, riga per riga, senza i tag.</summary>
        Private Shared Function TestoDellaPagina(html As String) As List(Of String)

            Dim corpo As String = html.Substring(html.IndexOf("<body>", StringComparison.Ordinal))

            Return WebUtility.HtmlDecode(Regex.Replace(corpo, "<[^>]+>", vbLf)).
                Split({vbLf}, StringSplitOptions.RemoveEmptyEntries Or StringSplitOptions.TrimEntries).
                ToList()

        End Function

        <TestMethod>
        Public Sub LaPaginaStaTuttaInSeStessa()

            Dim html As String = ScrittoreHtml.Componi(Impaginazione.PaginaCv(CvDiProva()))

            StringAssert.Contains(html, "<title>CV — Luca Ferrari</title>")
            StringAssert.Contains(html, "font-family: Calibri")

            ' Nessun riferimento a file esterni: la pagina nasce in memoria e non ha una
            ' cartella dove andare a cercarsi qualcosa.
            Assert.DoesNotContain("<link", html, "La pagina si porta dietro un file esterno.")
            Assert.DoesNotContain("<script", html, "La pagina si porta dietro uno script.")

            ' E nessun segnaposto rimasto vuoto.
            Assert.IsFalse(Regex.IsMatch(html, "\{\{[A-Z]+\}\}"), "C'è un segnaposto non riempito.")

        End Sub

        <TestMethod>
        Public Sub OgniBloccoPortaLaSuaClasse()

            Dim html As String = ScrittoreHtml.Componi(Impaginazione.PaginaCv(CvDiProva()))

            Dim classi As String() = Regex.Matches(html, "<p class=""([a-z-]+)""").
                Select(Function(m) m.Groups(1).Value).ToArray()

            CollectionAssert.AreEqual(
                {ScrittoreHtml.ClasseNome, ScrittoreHtml.ClasseRecapiti, ScrittoreHtml.ClasseRecapiti,
                 ScrittoreHtml.ClasseTesto,
                 ScrittoreHtml.ClasseSezione, ScrittoreHtml.ClasseVoceTitolo,
                 ScrittoreHtml.ClasseVoceDettaglio, ScrittoreHtml.ClasseTesto,
                 ScrittoreHtml.ClasseSezione, ScrittoreHtml.ClasseVoceTitolo, ScrittoreHtml.ClasseTesto,
                 ScrittoreHtml.ClasseSezione,
                 ScrittoreHtml.ClasseSezione, ScrittoreHtml.ClasseVoceTitolo,
                 ScrittoreHtml.ClasseVoceDettaglio},
                classi, String.Join(" | ", classi))

        End Sub

        <TestMethod>
        Public Sub LeCompetenzeSonoUnElencoVero()

            Dim html As String = ScrittoreHtml.Componi(Impaginazione.PaginaCv(CvDiProva()))

            StringAssert.Contains(html, "<ul>")
            StringAssert.Contains(html, "<li>Uso del muletto</li>")

        End Sub

        <TestMethod>
        Public Sub IlTestoDaEscaperEMessoAlSicuro()

            Dim cv As JsonObject = CvDiProva()
            CType(CType(cv("esperienze_professionali"), JsonArray)(0), JsonObject)("azienda") =
                JsonValue.Create("Rossi & Figli <S.p.A.>")

            Dim html As String = ScrittoreHtml.Componi(Impaginazione.PaginaCv(cv))

            StringAssert.Contains(html, "Rossi &amp; Figli &lt;S.p.A.&gt;")
            Assert.DoesNotContain("<S.p.A.>", html, "Un pezzo di testo è finito nella pagina come tag.")

        End Sub

        <TestMethod>
        Public Sub LaLetteraFinisceConLaFirma()

            Dim lettera As JsonNode = JsonNode.Parse(
                "{ ""apertura"": ""Spettabile Azienda,"", ""corpo"": ""Mi candido."", " &
                """chiusura"": ""Cordiali saluti,"", ""firma"": { ""nome"": ""Luca Ferrari"" } }")

            Dim html As String = ScrittoreHtml.Componi(Impaginazione.PaginaLettera(lettera))

            StringAssert.Contains(html, $"<p class=""{ScrittoreHtml.ClasseFirma}"">Luca Ferrari</p>")

        End Sub

        <TestMethod>
        Public Sub SenzaPaginaNonSiComponeNiente()
            Assert.Throws(Of ArgumentNullException)(Sub() ScrittoreHtml.Componi(Nothing))
        End Sub

        <TestMethod>
        Public Sub IlDocxELHtmlDiconoLeStesseCoseNelloStessoOrdine()

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(CvDiProva())

            Dim cartella As String = Path.Combine(Path.GetTempPath(),
                                                  "confronto-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(cartella)

            Try
                Dim percorso As String = Path.Combine(cartella, "documento.docx")
                ScrittoreDocx.Scrivi(pagina, percorso)

                Dim daDocx As List(Of String) = LettoreDocumenti.LeggiDocx(percorso).
                    Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries Or
                                          StringSplitOptions.TrimEntries).ToList()

                Dim daHtml As List(Of String) =
                    TestoDellaPagina(ScrittoreHtml.Componi(pagina))

                ' Se un giorno le due stampanti divergessero — una sezione in più di qua,
                ' un campo dimenticato di là — è qui che si vedrebbe, prima che sia
                ' l'utente ad accorgersene confrontando i suoi due file.
                CollectionAssert.AreEqual(daDocx, daHtml,
                    "Il DOCX e il PDF direbbero cose diverse." & vbLf &
                    $"DOCX: {String.Join(" | ", daDocx)}" & vbLf &
                    $"HTML: {String.Join(" | ", daHtml)}")

            Finally
                Directory.Delete(cartella, recursive:=True)
            End Try

        End Sub

    End Class

End Namespace
