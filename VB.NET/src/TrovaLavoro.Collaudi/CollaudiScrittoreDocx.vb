Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Xml.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti

Namespace Documenti

    ''' <summary>
    ''' Collaudi della stampante DOCX (cap. 05.4). La domanda che fanno è quella del
    ''' cap. 05.7: il documento che esce <b>dice quello che diceva il JSON</b>, campo per
    ''' campo?
    ''' </summary>
    ''' <remarks>
    ''' <para>Il modo di chiederlo è un cerchio chiuso: si scrive il <c>.docx</c> con
    ''' <see cref="ScrittoreDocx"/> e lo si rilegge con <see cref="LettoreDocumenti"/>,
    ''' che è il pezzo di programma che già oggi apre i CV dell'utente e non sa niente di
    ''' chi ha scritto il file. Se il documento fosse malformato, o se un campo si
    ''' perdesse per strada, il lettore lo direbbe.</para>
    ''' <para>Quello che questi collaudi <b>non</b> possono dire è se il file si apre
    ''' senza avvisi in Word e in LibreOffice: quello lo dicono Word e LibreOffice, ed è
    ''' la gamba C del collaudo di tappa (cap. 14).</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiScrittoreDocx

        ''' <summary>Lo stesso CV pieno dei collaudi dell'impaginazione.</summary>
        Private Shared Function CvDiProva() As JsonObject

            Return CType(JsonNode.Parse("
                {
                  ""tipo"": ""cv_mirato"",
                  ""intestazione"": {
                    ""nome"": ""Luca Ferrari"",
                    ""email"": ""luca.ferrari@example.it"",
                    ""telefono"": ""333 1234567"",
                    ""citta"": ""Modena"",
                    ""link"": ""linkedin.com/in/lucaferrari"",
                    ""patente"": ""B""
                  },
                  ""sommario"": ""Ho esperienza nel servizio di sala e in magazzino."",
                  ""esperienze_professionali"": [
                    { ""ruolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino"",
                      ""durata"": ""2019-2023"", ""descrizione"": ""Servizio ai tavoli e cassa."" },
                    { ""ruolo"": ""Magazziniere"", ""azienda"": ""Logistica Padana"",
                      ""durata"": ""2023-2025"", ""descrizione"": ""Carico e scarico merci."" }
                  ],
                  ""altre_esperienze"": [
                    { ""descrizione"": ""Volontariato alla sagra del paese"", ""quando"": ""estate 2021"" }
                  ],
                  ""competenze"": [""Servizio ai tavoli"", ""Uso del muletto""],
                  ""formazione"": [
                    { ""titolo"": ""Diploma alberghiero"", ""istituto"": ""IPSSAR Modena"", ""anno"": ""2018"" }
                  ]
                }"), JsonObject)

        End Function

        Private Shared Function LetteraDiProva() As JsonObject

            Return CType(JsonNode.Parse("
                {
                  ""tipo"": ""lettera_mirata"",
                  ""apertura"": ""Spettabile Azienda, mi candido per la posizione di Magazziniere."",
                  ""corpo"": ""Ho due anni di esperienza in magazzino e il patentino del muletto."",
                  ""chiusura"": ""Resto a disposizione. Cordiali saluti,"",
                  ""firma"": { ""nome"": ""Luca Ferrari"", ""email"": ""luca.ferrari@example.it"",
                               ""telefono"": ""333 1234567"" }
                }"), JsonObject)

        End Function

        ''' <summary>Scrive la pagina in una cartella temporanea e ne dà il percorso.</summary>
        Private Shared Sub ConDocx(pagina As PaginaDocumento, prova As Action(Of String))

            Dim cartella As String = Path.Combine(Path.GetTempPath(),
                                                  "docx-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(cartella)

            Try
                Dim percorso As String = Path.Combine(cartella, "documento.docx")
                ScrittoreDocx.Scrivi(pagina, percorso)
                prova(percorso)
            Finally
                Directory.Delete(cartella, recursive:=True)
            End Try

        End Sub

        ''' <summary>
        ''' Tutti i testi che il JSON contiene, quelli che devono ritrovarsi nel
        ''' documento. Il campo <c>tipo</c> resta fuori: dice che documento è, non è
        ''' contenuto da stampare.
        ''' </summary>
        Private Shared Function TestiDentro(nodo As JsonNode) As List(Of String)

            Dim raccolti As New List(Of String)

            Dim oggetto As JsonObject = TryCast(nodo, JsonObject)
            If oggetto IsNot Nothing Then
                For Each campo As KeyValuePair(Of String, JsonNode) In oggetto
                    If campo.Key <> "tipo" Then raccolti.AddRange(TestiDentro(campo.Value))
                Next
                Return raccolti
            End If

            Dim lista As JsonArray = TryCast(nodo, JsonArray)
            If lista IsNot Nothing Then
                For Each voce As JsonNode In lista
                    raccolti.AddRange(TestiDentro(voce))
                Next
                Return raccolti
            End If

            If nodo IsNot Nothing AndAlso nodo.GetValueKind() = Text.Json.JsonValueKind.String Then
                Dim testo As String = nodo.GetValue(Of String)()
                If Not String.IsNullOrWhiteSpace(testo) Then raccolti.Add(testo)
            End If

            Return raccolti

        End Function

        ''' <summary>Le parti dentro il pacchetto, coi loro nomi.</summary>
        Private Shared Function Parti(percorso As String) As List(Of String)

            Using archivio As ZipArchive = ZipFile.OpenRead(percorso)
                Return archivio.Entries.Select(Function(v) v.FullName).ToList()
            End Using

        End Function

        ''' <summary>Il contenuto XML di una parte del pacchetto.</summary>
        Private Shared Function Parte(percorso As String, nome As String) As XDocument

            Using archivio As ZipArchive = ZipFile.OpenRead(percorso)
                Using flusso As Stream = archivio.GetEntry(nome).Open()
                    Return XDocument.Load(flusso)
                End Using
            End Using

        End Function

        <TestMethod>
        Public Sub IlPacchettoHaLeSetteParti()

            ConDocx(Impaginazione.PaginaCv(CvDiProva()),
                Sub(percorso)
                    CollectionAssert.AreEquivalent(
                        {"[Content_Types].xml", "_rels/.rels", "docProps/core.xml",
                         "word/document.xml", "word/_rels/document.xml.rels",
                         "word/styles.xml", "word/numbering.xml"},
                        Parti(percorso))
                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlCvRilettoDiceQuelloCheDicevaIlJson()

            Dim cv As JsonObject = CvDiProva()

            ConDocx(Impaginazione.PaginaCv(cv),
                Sub(percorso)
                    Dim letto As String = LettoreDocumenti.LeggiDocx(percorso)

                    ' Campo per campo, come chiede il cap. 05.7: quello che il modello ha
                    ' scritto nel CV dev'essere nel file, tutto.
                    For Each atteso As String In TestiDentro(cv)
                        StringAssert.Contains(letto, atteso, $"Manca dal documento: «{atteso}»")
                    Next
                End Sub)

        End Sub

        <TestMethod>
        Public Sub LaLetteraRilettaDiceQuelloCheDicevaIlJson()

            Dim lettera As JsonObject = LetteraDiProva()

            ConDocx(Impaginazione.PaginaLettera(lettera),
                Sub(percorso)
                    Dim letto As String = LettoreDocumenti.LeggiDocx(percorso)

                    For Each atteso As String In TestiDentro(lettera)
                        StringAssert.Contains(letto, atteso, $"Manca dalla lettera: «{atteso}»")
                    Next
                End Sub)

        End Sub

        <TestMethod>
        Public Sub LeCompetenzeEsconoSenzaIlPallinoAttaccato()

            ConDocx(Impaginazione.PaginaCv(CvDiProva()),
                Sub(percorso)
                    Dim righe As String() = LettoreDocumenti.LeggiDocx(percorso).
                        Split({vbCrLf, vbLf}, StringSplitOptions.None)

                    ' Il pallino è nella numerazione, non nel testo: chi estrae il testo
                    ' del file — un ATS, o il collaudo qui sopra — trova la competenza
                    ' pulita, non «• Uso del muletto».
                    CollectionAssert.Contains(righe, "Uso del muletto")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlTestoDaEscaperNonRompeIlDocumento()

            Dim cv As JsonObject = CvDiProva()
            CType(CType(cv("esperienze_professionali"), JsonArray)(0), JsonObject)("azienda") =
                JsonValue.Create("Rossi & Figli <S.p.A.> «storica»")

            ConDocx(Impaginazione.PaginaCv(cv),
                Sub(percorso)
                    ' Se l'escape XML fosse fatto a mano, qui il file non si aprirebbe
                    ' nemmeno: è il motivo per cui il corpo si costruisce con XDocument.
                    StringAssert.Contains(LettoreDocumenti.LeggiDocx(percorso),
                                          "Rossi & Figli <S.p.A.> «storica»")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub OgniParagrafoPortaUnoStileDelModello()

            ConDocx(Impaginazione.PaginaCv(CvDiProva()),
                Sub(percorso)
                    Dim w As XNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main"
                    Dim documento As XDocument = Parte(percorso, "word/document.xml")

                    Dim stiliUsati As List(Of String) = documento.Descendants(w + "pStyle").
                        Select(Function(s) s.Attribute(w + "val").Value).Distinct().ToList()

                    Dim stiliDelModello As String() = Parte(percorso, "word/styles.xml").
                        Descendants(w + "style").
                        Select(Function(s) s.Attribute(w + "styleId").Value).ToArray()

                    ' Uno stile scritto nel corpo ma non dichiarato negli stili è un
                    ' paragrafo che Word impagina come gli pare, in silenzio.
                    For Each stile As String In stiliUsati
                        CollectionAssert.Contains(stiliDelModello, stile,
                                                  $"Lo stile «{stile}» non è nel modello.")
                    Next

                    Assert.IsTrue(documento.Descendants(w + "p").Any())
                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlTitoloEIlNomeEntranoNelleProprieta()

            ConDocx(Impaginazione.PaginaCv(CvDiProva()),
                Sub(percorso)
                    Dim dc As XNamespace = "http://purl.org/dc/elements/1.1/"
                    Dim proprieta As XDocument = Parte(percorso, "docProps/core.xml")

                    Assert.AreEqual("CV — Luca Ferrari", proprieta.Descendants(dc + "title").Single().Value)
                    Assert.AreEqual("Luca Ferrari", proprieta.Descendants(dc + "creator").Single().Value)
                End Sub)

        End Sub

        <TestMethod>
        Public Sub LoStessoContenutoDaSempreLoStessoFile()

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(CvDiProva())

            CollectionAssert.AreEqual(ScrittoreDocx.Componi(pagina), ScrittoreDocx.Componi(pagina))

        End Sub

        <TestMethod>
        Public Sub DopoLaScritturaCEIlSoloFileDefinitivo()

            ConDocx(Impaginazione.PaginaCv(CvDiProva()),
                Sub(percorso)
                    CollectionAssert.AreEqual(
                        {"documento.docx"},
                        Directory.GetFiles(Path.GetDirectoryName(percorso)).
                            Select(AddressOf Path.GetFileName).ToArray())
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaPaginaVuotaESempreUnDocumentoValido()

            ' Il caso del CV povero: il documento dev'essere comunque un .docx apribile,
            ' non un archivio a metà.
            ConDocx(Impaginazione.PaginaCv(JsonNode.Parse("{}")),
                Sub(percorso)
                    Assert.AreEqual("", LettoreDocumenti.LeggiDocx(percorso))
                End Sub)

        End Sub

        <TestMethod>
        Public Sub SenzaPaginaOSenzaPercorsoNonSiStampa()

            Assert.Throws(Of ArgumentNullException)(Sub() ScrittoreDocx.Componi(Nothing))
            Assert.Throws(Of ArgumentException)(
                Sub() ScrittoreDocx.Scrivi(New PaginaDocumento(), "  "))

        End Sub

    End Class

End Namespace
