Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi del lettore dei documenti (cap. 05.1). Girano tutti <b>senza rete</b> e
    ''' senza dipendere da un file di nessuno: i documenti se li fabbrica il collaudo,
    ''' byte per byte, così le due cose che qui possono andare storte — la codifica di un
    ''' file di testo e la struttura di un <c>.docx</c> — si verificano davvero invece di
    ''' essere date per buone.
    ''' </summary>
    <TestClass>
    Public Class CollaudiLettoreDocumenti

        <TestMethod>
        Public Sub RiconosceIFormatiDallEstensione()
            ' Il riconoscimento non deve essere schizzinoso sulle maiuscole: chi esporta
            ' un CV si ritrova spesso un .PDF, e sarebbe assurdo rifiutarlo per questo.
            Assert.AreEqual(FormatoDocumento.Pdf, LettoreDocumenti.Formato("cv.pdf"), "pdf")
            Assert.AreEqual(FormatoDocumento.Pdf, LettoreDocumenti.Formato("CV_MIRCO.PDF"), "PDF maiuscolo")
            Assert.AreEqual(FormatoDocumento.Docx, LettoreDocumenti.Formato("cv.docx"), "docx")
            Assert.AreEqual(FormatoDocumento.Testo, LettoreDocumenti.Formato("cv.txt"), "txt")
            Assert.AreEqual(FormatoDocumento.Testo, LettoreDocumenti.Formato("cv.Md"), "md")
            Assert.AreEqual(FormatoDocumento.NonSupportato, LettoreDocumenti.Formato("cv.odt"), "odt")
            Assert.AreEqual(FormatoDocumento.NonSupportato, LettoreDocumenti.Formato("cv.doc"),
                            "il vecchio .doc non è un .docx")
            Assert.AreEqual(FormatoDocumento.NonSupportato, LettoreDocumenti.Formato(""), "niente")
        End Sub

        <TestMethod>
        Public Sub IlTestoUtf8SiLeggeConGliAccenti()
            ' Con e senza BOM: il file scritto da un editor moderno è UTF-8, e il BOM c'è
            ' o non c'è a seconda dell'editor. In entrambi i casi «Forlì» deve restare
            ' «Forlì», e il BOM non deve finire dentro il testo.
            Dim testo As String = "Mario Rossi — Forlì" & vbLf & "Perito elettrotecnico, città di residenza: Forlì."

            ConFileDiProva(".txt", New UTF8Encoding(encoderShouldEmitUTF8Identifier:=True).GetBytes(testo),
                Sub(percorso)
                    Assert.AreEqual(testo, LettoreDocumenti.LeggiTesto(percorso), "UTF-8 con BOM")
                End Sub)

            ConFileDiProva(".txt", New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False).GetBytes(testo),
                Sub(percorso)
                    Assert.AreEqual(testo, LettoreDocumenti.LeggiTesto(percorso), "UTF-8 senza BOM")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlTestoAnsiNonDiventaCaratteriStrani()
            ' Un .txt salvato da un programma vecchio non è UTF-8: i suoi accenti sono un
            ' byte solo. Leggerlo come UTF-8 darebbe il classico «citt?» — che poi
            ' finirebbe dritto nel profilo.
            Dim byteAnsi As Byte() = {
                CByte(Asc("c")), CByte(Asc("i")), CByte(Asc("t")), CByte(Asc("t")), &HE0,
                CByte(Asc(":")), CByte(Asc(" ")),
                CByte(Asc("F")), CByte(Asc("o")), CByte(Asc("r")), CByte(Asc("l")), &HEC}

            ConFileDiProva(".txt", byteAnsi,
                Sub(percorso)
                    Assert.AreEqual("città: Forlì", LettoreDocumenti.LeggiTesto(percorso),
                                    "gli accenti ANSI vanno letti come tali")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LApostrofoDiWordSopravviveAllAnsi()
            ' Gli accenti da soli non bastano a dire se la codifica ANSI è letta bene:
            ' quelli Latin-1 e Windows-1252 li scrivono uguali. La differenza sta nei 32
            ' byte alti, dove Windows tiene l'apostrofo curvo che Word infila in ogni
            ' «l'esperienza», le virgolette e il trattino lungo. Letti come Latin-1
            ' diventerebbero segni invisibili in mezzo alle parole — e nessuno se ne
            ' accorgerebbe finché quel testo non arriva al modello.
            Dim byteAnsi As Byte() = {
                CByte(Asc("l")), &H92, CByte(Asc("e")), CByte(Asc("s")), CByte(Asc("p")),
                CByte(Asc("e")), CByte(Asc("r")), CByte(Asc("o")), CByte(Asc(" ")),
                &H93, CByte(Asc("s")), CByte(Asc("i")), &H94, CByte(Asc(" ")), &H96,
                CByte(Asc(" ")), &H80}

            ConFileDiProva(".txt", byteAnsi,
                Sub(percorso)
                    Assert.AreEqual("l" & ChrW(&H2019) & "espero " &
                                    ChrW(&H201C) & "si" & ChrW(&H201D) & " " &
                                    ChrW(&H2013) & " " & ChrW(&H20AC),
                                    LettoreDocumenti.LeggiTesto(percorso),
                                    "apostrofo, virgolette, trattino ed euro di Windows")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlMarkdownArrivaComEScritto()
            ' Il .md si usa così com'è: le sue intestazioni sono struttura che aiuta il
            ' modello a capire il CV, e non vanno tolte (cap. 05.1).
            Dim testo As String = "# Mario Rossi" & vbLf & vbLf & "## Esperienze" & vbLf & "- Magazziniere"

            ConFileDiProva(".md", Encoding.UTF8.GetBytes(testo),
                Sub(percorso)
                    Assert.AreEqual(testo, LettoreDocumenti.LeggiTesto(percorso), "il markdown resta intero")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlDocxDaParagrafiETabelleNellOrdineDelDocumento()
            ' Il punto del .docx: l'ordine. Un CV impaginato con una tabella va letto
            ' riga per riga come lo si legge a occhio, non con le parti sparpagliate.
            Dim corpo As String =
                Paragrafo("Mario Rossi") &
                Paragrafo("Magazziniere") &
                "<w:tbl>" &
                    "<w:tr><w:tc>" & Paragrafo("2019-2024") & "</w:tc><w:tc>" & Paragrafo("Logistica Rossi") & "</w:tc></w:tr>" &
                    "<w:tr><w:tc>" & Paragrafo("2015-2019") & "</w:tc><w:tc>" & Paragrafo("Bar Centrale") & "</w:tc></w:tr>" &
                "</w:tbl>" &
                Paragrafo("Patente B")

            ConFileDiProva(".docx", DocxDiProva(corpo),
                Sub(percorso)
                    Dim righe As String() = LettoreDocumenti.LeggiTesto(percorso).
                        Split({vbCrLf, vbLf}, StringSplitOptions.None)

                    Assert.AreEqual("Mario Rossi", righe(0), "primo paragrafo")
                    Assert.AreEqual("Magazziniere", righe(1), "secondo paragrafo")
                    Assert.AreEqual("2019-2024" & vbTab & "Logistica Rossi", righe(2),
                                    "prima riga della tabella, celle separate da tabulazione")
                    Assert.AreEqual("2015-2019" & vbTab & "Bar Centrale", righe(3), "seconda riga")
                    Assert.AreEqual("Patente B", righe(4), "il paragrafo dopo la tabella")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlDocxPrendeSoloIlTestoCheSiLegge()
            ' Dentro un .docx ci sono anche cose che a schermo non compaiono: i codici dei
            ' campi e il testo cancellato con le revisioni. Se finissero nel CV, il
            ' profilo si porterebbe dentro roba che l'utente non ha mai scritto.
            Dim corpo As String =
                "<w:p><w:r><w:t>Mario</w:t></w:r><w:r><w:t xml:space=""preserve""> Rossi</w:t></w:r></w:p>" &
                "<w:p><w:r><w:delText>Disoccupato</w:delText></w:r><w:r><w:t>Magazziniere</w:t></w:r></w:p>" &
                "<w:p><w:r><w:instrText>PAGE \* MERGEFORMAT</w:instrText></w:r></w:p>" &
                "<w:p><w:r><w:t>Via Roma 1</w:t><w:tab/><w:t>Forlì</w:t></w:r></w:p>" &
                "<w:sdt><w:sdtContent>" & Paragrafo("Competenze: uso del muletto") & "</w:sdtContent></w:sdt>"

            ConFileDiProva(".docx", DocxDiProva(corpo),
                Sub(percorso)
                    Dim testo As String = LettoreDocumenti.LeggiTesto(percorso)

                    Assert.Contains("Mario Rossi", testo, "i pezzi di un paragrafo si uniscono")
                    Assert.Contains("Magazziniere", testo, "il testo vero c'è")
                    Assert.DoesNotContain("Disoccupato", testo, "il testo cancellato con le revisioni no")
                    Assert.DoesNotContain("MERGEFORMAT", testo, "e nemmeno i codici dei campi")
                    Assert.Contains("Via Roma 1" & vbTab & "Forlì", testo, "la tabulazione resta")
                    Assert.Contains("uso del muletto", testo,
                                    "e il testo dentro un controllo contenuto non si perde")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnDocxCheNonEUnArchivioLoDiceInChiaro()
            ' Il caso vero: un .doc del vecchio Word rinominato in .docx. L'errore deve
            ' dire cosa fare, non «End of Central Directory record could not be found».
            ConFileDiProva(".docx", Encoding.UTF8.GetBytes("Questo non è un archivio ZIP."),
                Sub(percorso)
                    Dim errore As InvalidDataException = Assert.Throws(Of InvalidDataException)(
                        Sub() LettoreDocumenti.LeggiTesto(percorso))

                    Assert.Contains(".docx", errore.Message, "deve suggerire il salvataggio in .docx")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnArchivioSenzaIlCorpoDelDocumentoLoDice()
            ' Uno ZIP valido ma che di Word non ha niente: succede con i .zip rinominati.
            Dim memoria As New MemoryStream()
            Using archivio As New ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen:=True)
                archivio.CreateEntry("appunti.txt")
            End Using

            ConFileDiProva(".docx", memoria.ToArray(),
                Sub(percorso)
                    Dim errore As InvalidDataException = Assert.Throws(Of InvalidDataException)(
                        Sub() LettoreDocumenti.LeggiTesto(percorso))

                    Assert.Contains("word/document.xml", errore.Message, "deve dire cosa manca")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlPdfNonSiLeggeDalDiscoEQuiSiDiceDiChiEIlCompito()
            ' Il PDF è l'unico formato che il disco non basta a leggere: fermarsi qui, con
            ' un messaggio che nomina chi lo fa, evita di cercare il bug altrove.
            ConFileDiProva(".pdf", Encoding.UTF8.GetBytes("%PDF-1.7"),
                Sub(percorso)
                    Dim errore As NotSupportedException = Assert.Throws(Of NotSupportedException)(
                        Sub() LettoreDocumenti.LeggiTesto(percorso))

                    Assert.Contains("ITrascrittorePdf", errore.Message, "deve indicare chi trascrive il PDF")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnFormatoNonAccettatoDiceQualiSonoQuelliBuoni()
            ConFileDiProva(".odt", Encoding.UTF8.GetBytes("documento di LibreOffice"),
                Sub(percorso)
                    Dim errore As NotSupportedException = Assert.Throws(Of NotSupportedException)(
                        Sub() LettoreDocumenti.LeggiTesto(percorso))

                    Assert.Contains("PDF, DOCX, TXT e MD", errore.Message, "l'elenco dei formati accettati")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnFileVuotoDaTestoVuoto()
            ' Non è un errore di lettura: è un file senza niente dentro. A dire che così
            ' non è un CV ci pensa l'import, con la sua soglia.
            ConFileDiProva(".txt", Array.Empty(Of Byte)(),
                Sub(percorso)
                    Assert.AreEqual("", LettoreDocumenti.LeggiTesto(percorso), "testo vuoto, non un'eccezione")
                End Sub)
        End Sub

        ''' <summary>Un paragrafo di Word con dentro il testo indicato.</summary>
        Private Shared Function Paragrafo(testo As String) As String
            Return $"<w:p><w:r><w:t>{testo}</w:t></w:r></w:p>"
        End Function

        ''' <summary>
        ''' Un <c>.docx</c> fabbricato qui: un archivio ZIP con dentro il solo
        ''' <c>word/document.xml</c>, che è l'unica parte che il lettore guarda.
        ''' </summary>
        Private Shared Function DocxDiProva(corpoXml As String) As Byte()

            Dim memoria As New MemoryStream()

            Using archivio As New ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen:=True)
                Dim parte As ZipArchiveEntry = archivio.CreateEntry("word/document.xml")
                Using scrittore As New StreamWriter(parte.Open(), New UTF8Encoding(False))
                    scrittore.Write(
                        "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
                        "<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">" &
                        "<w:body>" & corpoXml & "</w:body></w:document>")
                End Using
            End Using

            Return memoria.ToArray()

        End Function

        ''' <summary>Scrive i byte in un file temporaneo con l'estensione data, e poi lo toglie.</summary>
        Private Shared Sub ConFileDiProva(estensione As String, contenuto As Byte(), prova As Action(Of String))

            Dim percorso As String = Path.Combine(
                Path.GetTempPath(), "documento-" & Guid.NewGuid().ToString("N") & estensione)

            File.WriteAllBytes(percorso, contenuto)
            Try
                prova(percorso)
            Finally
                If File.Exists(percorso) Then File.Delete(percorso)
            End Try

        End Sub

    End Class

End Namespace
