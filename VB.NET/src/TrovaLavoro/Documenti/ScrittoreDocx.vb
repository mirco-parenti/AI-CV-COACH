Imports System.IO
Imports System.IO.Compression
Imports System.Reflection
Imports System.Text
Imports System.Xml
Imports System.Xml.Linq

Namespace Documenti

    ''' <summary>
    ''' La stampante DOCX: da una <see cref="PaginaDocumento"/> a un documento di Word
    ''' vero, composto come archivio ZIP con gli strumenti standard di .NET
    ''' (cap. 05.4). Nessuna libreria esterna, nessun bisogno che Word sia installato.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Com'è fatto un .docx.</b> È uno ZIP con dentro poche parti XML: l'elenco
    ''' dei contenuti, due file di relazioni, gli stili, la numerazione degli elenchi, le
    ''' proprietà del documento e il corpo. Cinque di queste sono <b>uguali per ogni
    ''' documento che il programma scrive</b> e stanno come risorse incorporate nella
    ''' cartella <c>modello-docx</c>: è lì che vive la tipografia, e si ritocca senza
    ''' toccare una riga di codice. Le due che dipendono dai dati — il corpo e il titolo —
    ''' si costruiscono qui con <see cref="XDocument"/>, che è anche il modo di non
    ''' doversi ricordare a mano che una <c>&amp;</c> in un nome di azienda va scritta
    ''' <c>&amp;amp;</c>.</para>
    ''' <para><b>Lo stesso contenuto dà sempre lo stesso file.</b> Le date di modifica
    ''' delle voci dell'archivio sono fissate a un istante costante invece che a «adesso»:
    ''' due stampe dello stesso CV producono byte identici, il che rende dimostrabile che
    ''' a cambiare un documento è stato il contenuto e non il momento in cui l'hai
    ''' salvato. Le date che l'utente vede in Word restano quelle del file su disco.</para>
    ''' </remarks>
    Public Class ScrittoreDocx

        ''' <summary>Il namespace di WordprocessingML: il corpo del documento vive lì.</summary>
        Private Shared ReadOnly W As XNamespace =
            "http://schemas.openxmlformats.org/wordprocessingml/2006/main"

        ''' <summary>I due namespace delle proprietà del documento (titolo e autore).</summary>
        Private Shared ReadOnly Cp As XNamespace =
            "http://schemas.openxmlformats.org/package/2006/metadata/core-properties"
        Private Shared ReadOnly Dc As XNamespace = "http://purl.org/dc/elements/1.1/"

        ''' <summary>Gli stili dichiarati in <c>modello-docx/styles.xml</c>.</summary>
        Public Const StileNome As String = "Nome"
        Public Const StileRecapiti As String = "Recapiti"
        Public Const StileSezione As String = "Sezione"
        Public Const StileVoceTitolo As String = "VoceTitolo"
        Public Const StileVoceDettaglio As String = "VoceDettaglio"
        Public Const StileTesto As String = "Testo"
        Public Const StilePunto As String = "Punto"
        Public Const StileFirma As String = "Firma"

        ''' <summary>Dove finisce, dentro lo ZIP, ognuna delle parti fisse del modello.</summary>
        Private Shared ReadOnly PartiFisse As New Dictionary(Of String, String) From {
            {"content-types.xml", "[Content_Types].xml"},
            {"rels.xml", "_rels/.rels"},
            {"document-rels.xml", "word/_rels/document.xml.rels"},
            {"settings.xml", "word/settings.xml"},
            {"styles.xml", "word/styles.xml"},
            {"numbering.xml", "word/numbering.xml"}}

        ''' <summary>Il prefisso con cui le parti fisse entrano nell'eseguibile.</summary>
        Private Const PrefissoRisorsa As String = "modello-docx/"

        ''' <summary>
        ''' L'istante scritto sulle voci dell'archivio. È il più antico che il formato ZIP
        ''' sappia scrivere, e vale come «non significativo»: qui la data non è un dato,
        ''' è solo ciò che rendeva il file diverso da sé stesso a ogni stampa.
        ''' </summary>
        Private Shared ReadOnly IstanteFisso As New DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero)

        ''' <summary>UTF-8 <b>senza BOM</b>: le parti di un pacchetto OOXML si scrivono così.</summary>
        Private Shared ReadOnly Codifica As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        ''' <summary>
        ''' Il documento come sequenza di byte: è la forma che serve ai collaudi e a
        ''' chiunque debba mandarlo altrove senza passare dal disco.
        ''' </summary>
        ''' <param name="pagina">La pagina da stampare.</param>
        Public Shared Function Componi(pagina As PaginaDocumento) As Byte()

            If pagina Is Nothing Then Throw New ArgumentNullException(NameOf(pagina))

            Using memoria As New MemoryStream()

                Using archivio As New ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen:=True)

                    ' L'elenco dei contenuti va per primo: è la convenzione dei pacchetti
                    ' OPC, e i lettori più severi ci contano.
                    AggiungiTesto(archivio, PartiFisse("content-types.xml"), Risorsa("content-types.xml"))
                    AggiungiTesto(archivio, PartiFisse("rels.xml"), Risorsa("rels.xml"))
                    AggiungiXml(archivio, "docProps/core.xml", Proprieta(pagina))
                    AggiungiXml(archivio, "word/document.xml", Documento(pagina))
                    AggiungiTesto(archivio, PartiFisse("document-rels.xml"), Risorsa("document-rels.xml"))
                    AggiungiTesto(archivio, PartiFisse("settings.xml"), Risorsa("settings.xml"))
                    AggiungiTesto(archivio, PartiFisse("styles.xml"), Risorsa("styles.xml"))
                    AggiungiTesto(archivio, PartiFisse("numbering.xml"), Risorsa("numbering.xml"))

                End Using

                Return memoria.ToArray()

            End Using

        End Function

        ''' <summary>
        ''' Scrive il documento su disco, in modo atomico: prima un file temporaneo, poi
        ''' lo spostamento sul nome definitivo. Un salvataggio interrotto non lascia mai
        ''' al suo posto un CV troncato.
        ''' </summary>
        ''' <param name="pagina">La pagina da stampare.</param>
        ''' <param name="percorso">Il file <c>.docx</c> da scrivere.</param>
        Public Shared Sub Scrivi(pagina As PaginaDocumento, percorso As String)

            If String.IsNullOrWhiteSpace(percorso) Then
                Throw New ArgumentException("Manca il percorso del documento.", NameOf(percorso))
            End If

            Dim contenuto As Byte() = Componi(pagina)
            Dim temporaneo As String = percorso & ".tmp"

            File.WriteAllBytes(temporaneo, contenuto)
            File.Move(temporaneo, percorso, overwrite:=True)

        End Sub

        ''' <summary>
        ''' Il corpo del documento: ogni blocco della pagina diventa uno o più paragrafi,
        ''' ognuno col suo stile. È l'unico punto in cui si decide che aspetto ha un
        ''' blocco in un DOCX — e il gemello della stessa scelta fatta in HTML per il PDF.
        ''' </summary>
        Private Shared Function Documento(pagina As PaginaDocumento) As XDocument

            Dim corpo As New XElement(W + "body")

            For Each blocco As Blocco In pagina.Blocchi

                Select Case blocco.Genere

                    Case GenereBlocco.Nome
                        corpo.Add(Paragrafo(StileNome, blocco.Testo))

                    Case GenereBlocco.Recapiti
                        corpo.Add(Paragrafo(StileRecapiti,
                                            String.Join(Impaginazione.Separatore, blocco.Voci)))

                    Case GenereBlocco.Sezione
                        corpo.Add(Paragrafo(StileSezione, blocco.Testo))

                    Case GenereBlocco.Paragrafo
                        corpo.Add(Paragrafo(StileTesto, blocco.Testo))

                    Case GenereBlocco.Voce
                        ' I tre pezzi di una voce sono tre paragrafi, e quello che manca
                        ' non lascia una riga vuota: le voci senza dettaglio esistono.
                        If blocco.Testo.Length > 0 Then corpo.Add(Paragrafo(StileVoceTitolo, blocco.Testo))
                        If blocco.Dettaglio.Length > 0 Then corpo.Add(Paragrafo(StileVoceDettaglio, blocco.Dettaglio))
                        If blocco.Descrizione.Length > 0 Then corpo.Add(Paragrafo(StileTesto, blocco.Descrizione))

                    Case GenereBlocco.Elenco
                        For Each voce As String In blocco.Voci
                            corpo.Add(Paragrafo(StilePunto, voce))
                        Next

                    Case GenereBlocco.Firma
                        corpo.Add(Paragrafo(StileFirma, blocco.Testo))

                End Select

            Next

            corpo.Add(Foglio())

            Return New XDocument(
                New XDeclaration("1.0", "UTF-8", "yes"),
                New XElement(W + "document",
                             New XAttribute(XNamespace.Xmlns + "w", W.NamespaceName),
                             corpo))

        End Function

        ''' <summary>Un paragrafo con il suo stile e il suo testo.</summary>
        Private Shared Function Paragrafo(stile As String, testo As String) As XElement

            Return New XElement(W + "p",
                New XElement(W + "pPr",
                    New XElement(W + "pStyle", New XAttribute(W + "val", stile))),
                New XElement(W + "r",
                    New XElement(W + "t",
                        New XAttribute(XNamespace.Xml + "space", "preserve"), testo)))

        End Function

        ''' <summary>
        ''' Il foglio: A4 in verticale con margini di 2 cm. Le misure sono in ventesimi di
        ''' punto, che è l'unità di OOXML — 11906 × 16838 è l'A4, 1134 sono 2 cm.
        ''' </summary>
        Private Shared Function Foglio() As XElement

            Return New XElement(W + "sectPr",
                New XElement(W + "pgSz",
                             New XAttribute(W + "w", "11906"),
                             New XAttribute(W + "h", "16838")),
                New XElement(W + "pgMar",
                             New XAttribute(W + "top", "1134"),
                             New XAttribute(W + "right", "1134"),
                             New XAttribute(W + "bottom", "1134"),
                             New XAttribute(W + "left", "1134"),
                             New XAttribute(W + "header", "708"),
                             New XAttribute(W + "footer", "708"),
                             New XAttribute(W + "gutter", "0")))

        End Function

        ''' <summary>
        ''' Le proprietà del documento: il titolo che Windows mostra nella scheda
        ''' «Dettagli» e l'autore, che è la persona del CV — non il programma.
        ''' </summary>
        Private Shared Function Proprieta(pagina As PaginaDocumento) As XDocument

            Dim radice As New XElement(Cp + "coreProperties",
                New XAttribute(XNamespace.Xmlns + "cp", Cp.NamespaceName),
                New XAttribute(XNamespace.Xmlns + "dc", Dc.NamespaceName),
                New XElement(Dc + "title", pagina.Titolo))

            Dim nome As Blocco = pagina.Blocchi.Find(Function(b) b.Genere = GenereBlocco.Nome)
            If nome IsNot Nothing Then radice.Add(New XElement(Dc + "creator", nome.Testo))

            Return New XDocument(New XDeclaration("1.0", "UTF-8", "yes"), radice)

        End Function

        ''' <summary>Una parte fissa del modello, dall'eseguibile.</summary>
        Private Shared Function Risorsa(nome As String) As String

            Dim assembly As Assembly = GetType(ScrittoreDocx).Assembly
            Dim percorsoRisorsa As String = PrefissoRisorsa & nome

            Using flusso As Stream = assembly.GetManifestResourceStream(percorsoRisorsa)

                If flusso Is Nothing Then
                    ' Come per il pool integrato: se manca, il programma è stato
                    ' costruito male, e non c'è ripiego che possa rimediare.
                    Throw New InvalidOperationException(
                        $"Il modello del documento è incompleto: manca «{nome}».")
                End If

                Using lettore As New StreamReader(flusso, Encoding.UTF8)
                    Return lettore.ReadToEnd()
                End Using

            End Using

        End Function

        ''' <summary>Aggiunge allo ZIP una parte già scritta.</summary>
        Private Shared Sub AggiungiTesto(archivio As ZipArchive, nome As String, contenuto As String)

            Using flusso As Stream = Voce(archivio, nome).Open()
                Dim byteParte As Byte() = Codifica.GetBytes(contenuto)
                flusso.Write(byteParte, 0, byteParte.Length)
            End Using

        End Sub

        ''' <summary>Aggiunge allo ZIP una parte costruita adesso.</summary>
        Private Shared Sub AggiungiXml(archivio As ZipArchive, nome As String, parte As XDocument)

            Dim impostazioni As New XmlWriterSettings With {
                .Indent = False,
                .Encoding = Codifica,
                .OmitXmlDeclaration = False}

            Using flusso As Stream = Voce(archivio, nome).Open()
                Using scrittore As XmlWriter = XmlWriter.Create(flusso, impostazioni)
                    parte.Save(scrittore)
                End Using
            End Using

        End Sub

        ''' <summary>Una voce dell'archivio, con la sua data resa insignificante.</summary>
        Private Shared Function Voce(archivio As ZipArchive, nome As String) As ZipArchiveEntry

            Dim nuova As ZipArchiveEntry = archivio.CreateEntry(nome, CompressionLevel.Optimal)
            nuova.LastWriteTime = IstanteFisso

            Return nuova

        End Function

    End Class

End Namespace
