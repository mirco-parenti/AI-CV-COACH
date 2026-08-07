Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Xml
Imports System.Xml.Linq

Namespace Dati

    ''' <summary>I formati che l'app sa prendere in ingresso (cap. 05.1).</summary>
    Public Enum FormatoDocumento
        ''' <summary>Non è uno dei formati accettati.</summary>
        NonSupportato
        ''' <summary>Testo semplice: <c>.txt</c> e <c>.md</c>. Si legge dal disco.</summary>
        Testo
        ''' <summary>Documento di Word: si apre come archivio ZIP.</summary>
        Docx
        ''' <summary>PDF: il disco non basta, lo trascrive l'AI (<c>ITrascrittorePdf</c>).</summary>
        Pdf
    End Enum

    ''' <summary>
    ''' Il lettore dei documenti dell'utente: da un file sul disco tira fuori il testo,
    ''' che è l'unica cosa di cui l'import ha bisogno (cap. 05.1).
    ''' </summary>
    ''' <remarks>
    ''' <para>Di rete qui non ce n'è, e di librerie esterne nemmeno: il testo semplice si
    ''' legge com'è, il <c>.docx</c> è un archivio ZIP e lo aprono gli strumenti standard
    ''' di .NET. Il PDF è l'unico che non si legge dal disco — lì il testo lo ricava il
    ''' modello di estrazione — e infatti questa classe si limita a riconoscerlo.</para>
    ''' <para>Conseguenza voluta: tutto ciò che sta qui dentro si collauda senza chiave e
    ''' senza spendere un token.</para>
    ''' </remarks>
    Public Class LettoreDocumenti

        ''' <summary>Il namespace di WordprocessingML: le parti di un <c>.docx</c> stanno lì.</summary>
        Private Shared ReadOnly W As XNamespace =
            "http://schemas.openxmlformats.org/wordprocessingml/2006/main"

        ''' <summary>La parte dell'archivio che contiene il corpo del documento.</summary>
        Private Const ParteDocumento As String = "word/document.xml"

        ''' <summary>
        ''' UTF-8 che <b>protesta</b> invece di sostituire i byte che non capisce: è così
        ''' che si distingue un file davvero UTF-8 da uno salvato con la codifica di
        ''' sistema, senza doverlo indovinare.
        ''' </summary>
        Private Shared ReadOnly Utf8Severo As New UTF8Encoding(
            encoderShouldEmitUTF8Identifier:=False, throwOnInvalidBytes:=True)

        ''' <summary>Riconosce il formato dall'estensione del nome.</summary>
        Public Shared Function Formato(percorso As String) As FormatoDocumento

            If String.IsNullOrWhiteSpace(percorso) Then Return FormatoDocumento.NonSupportato

            Select Case Path.GetExtension(percorso).ToLowerInvariant()
                Case ".txt", ".md" : Return FormatoDocumento.Testo
                Case ".docx" : Return FormatoDocumento.Docx
                Case ".pdf" : Return FormatoDocumento.Pdf
                Case Else : Return FormatoDocumento.NonSupportato
            End Select

        End Function

        ''' <summary>
        ''' Il testo di un documento leggibile dal disco: <c>.txt</c>, <c>.md</c> o
        ''' <c>.docx</c>. Sul PDF si ferma di proposito, dicendo di chi è il compito.
        ''' </summary>
        Public Shared Function LeggiTesto(percorso As String) As String

            Select Case Formato(percorso)

                Case FormatoDocumento.Testo
                    Return LeggiTestoSemplice(percorso)

                Case FormatoDocumento.Docx
                    Return LeggiDocx(percorso)

                Case FormatoDocumento.Pdf
                    Throw New NotSupportedException(
                        "Il testo di un PDF non si legge dal disco: lo trascrive il modello di " &
                        "estrazione (cap. 05.1). Usa ITrascrittorePdf.")

                Case Else
                    Throw New NotSupportedException(
                        $"«{Path.GetFileName(percorso)}»: formato non gestito. " &
                        "I formati accettati sono PDF, DOCX, TXT e MD.")

            End Select

        End Function

        ''' <summary>
        ''' Legge un file di testo scegliendo la codifica invece di darla per scontata:
        ''' prima il BOM se c'è, poi UTF-8 (che oggi è la regola), e solo se i byte non
        ''' sono UTF-8 validi si ripiega sulla codifica ANSI di Windows — che è come
        ''' escono ancora i file salvati da vecchi programmi. Sbagliare qui si vede
        ''' subito: le lettere accentate diventano caratteri strani.
        ''' </summary>
        Public Shared Function LeggiTestoSemplice(percorso As String) As String

            Dim byteFile As Byte() = File.ReadAllBytes(percorso)

            If byteFile.Length = 0 Then Return String.Empty

            ' 1. Il BOM: quando c'è, è il file stesso a dire in che codifica è scritto.
            If byteFile.Length >= 3 AndAlso byteFile(0) = &HEF AndAlso
               byteFile(1) = &HBB AndAlso byteFile(2) = &HBF Then
                Return Encoding.UTF8.GetString(byteFile, 3, byteFile.Length - 3)
            End If

            If byteFile.Length >= 2 AndAlso byteFile(0) = &HFF AndAlso byteFile(1) = &HFE Then
                Return Encoding.Unicode.GetString(byteFile, 2, byteFile.Length - 2)
            End If

            If byteFile.Length >= 2 AndAlso byteFile(0) = &HFE AndAlso byteFile(1) = &HFF Then
                Return Encoding.BigEndianUnicode.GetString(byteFile, 2, byteFile.Length - 2)
            End If

            ' 2. Niente BOM: si prova UTF-8 severo. Se i byte lo sono davvero, è finita.
            Try
                Return Utf8Severo.GetString(byteFile)
            Catch ex As DecoderFallbackException
                ' 3. Non erano UTF-8: allora è un file nella codifica ANSI di Windows.
                Return DaAnsi(byteFile)
            End Try

        End Function

        ''' <summary>
        ''' Decodifica i byte come ANSI di Windows (codepage 1252).
        ''' </summary>
        ''' <remarks>
        ''' <c>Encoding.GetEncoding(1252)</c> qui non si può usare: in .NET moderno le
        ''' codepage non ci sono, vogliono un pacchetto in più, e questo programma di
        ''' dipendenze non ne ha nessuna. Latin-1, che .NET ha, coincide con la 1252 in
        ''' tutto tranne una finestra di 32 byte — quella dove Windows tiene l'apostrofo
        ''' e le virgolette curve di Word, il trattino lungo, l'euro. Sono proprio i
        ''' caratteri che un CV italiano ha di sicuro, e con Latin-1 diventerebbero
        ''' segni invisibili in mezzo alle parole: si rimettono a posto qui.
        ''' </remarks>
        Private Shared Function DaAnsi(byteFile As Byte()) As String

            Dim caratteri As Char() = Encoding.Latin1.GetString(byteFile).ToCharArray()

            For posizione As Integer = 0 To caratteri.Length - 1
                Dim codice As Integer = AscW(caratteri(posizione))
                If codice >= &H80 AndAlso codice <= &H9F Then
                    caratteri(posizione) = FinestraWindows(codice - &H80)
                End If
            Next

            Return New String(caratteri)

        End Function

        ''' <summary>
        ''' I 32 caratteri che Windows-1252 mette da &amp;H80 a &amp;H9F, dove Latin-1
        ''' non ha niente di stampabile. Le cinque posizioni che nemmeno Windows usa
        ''' restano com'erano. La tabella è ferma dagli anni Novanta e non cambierà.
        ''' </summary>
        Private Shared ReadOnly FinestraWindows As Char() = {
            ChrW(&H20AC), ChrW(&H81), ChrW(&H201A), ChrW(&H192),
            ChrW(&H201E), ChrW(&H2026), ChrW(&H2020), ChrW(&H2021),
            ChrW(&H2C6), ChrW(&H2030), ChrW(&H160), ChrW(&H2039),
            ChrW(&H152), ChrW(&H8D), ChrW(&H17D), ChrW(&H8F),
            ChrW(&H90), ChrW(&H2018), ChrW(&H2019), ChrW(&H201C),
            ChrW(&H201D), ChrW(&H2022), ChrW(&H2013), ChrW(&H2014),
            ChrW(&H2DC), ChrW(&H2122), ChrW(&H161), ChrW(&H203A),
            ChrW(&H153), ChrW(&H9D), ChrW(&H17E), ChrW(&H178)}

        ''' <summary>
        ''' Il testo di un <c>.docx</c>. Un documento di Word è un archivio ZIP: dentro,
        ''' <c>word/document.xml</c> è il corpo, e da lì si prendono i paragrafi e le
        ''' tabelle <b>nell'ordine del documento</b> (cap. 05.1). Niente librerie
        ''' esterne: bastano ZIP e XML, che .NET ha già.
        ''' </summary>
        Public Shared Function LeggiDocx(percorso As String) As String

            Dim nome As String = Path.GetFileName(percorso)
            Dim documento As XDocument = Nothing

            Try
                Using archivio As ZipArchive = ZipFile.OpenRead(percorso)

                    Dim parte As ZipArchiveEntry = archivio.GetEntry(ParteDocumento)

                    If parte IsNot Nothing Then
                        Using flusso As Stream = parte.Open()
                            documento = XDocument.Load(flusso)
                        End Using
                    End If

                End Using

            Catch ex As InvalidDataException
                ' ZipFile protesta così quando il file non è un archivio: succede a chi
                ' rinomina in .docx un .doc vecchio, che è tutt'altro formato.
                Throw New InvalidDataException(
                    $"«{nome}» non si apre come documento di Word: il file non è un archivio valido. " &
                    "Se è un .doc del vecchio Word, riaprilo e salvalo in .docx.", ex)

            Catch ex As XmlException
                Throw New InvalidDataException(
                    $"«{nome}»: il corpo del documento di Word è illeggibile.", ex)
            End Try

            If documento Is Nothing Then
                Throw New InvalidDataException(
                    $"«{nome}» non sembra un documento di Word: dentro non c'è {ParteDocumento}.")
            End If

            Dim corpo As XElement = documento.Root?.Element(W + "body")
            If corpo Is Nothing Then
                Throw New InvalidDataException(
                    $"«{Path.GetFileName(percorso)}»: il documento di Word non ha un corpo da leggere.")
            End If

            Dim testo As New StringBuilder()
            ScorriBlocchi(corpo, testo)

            Return testo.ToString().TrimEnd()

        End Function

        ''' <summary>
        ''' Scorre i blocchi di un contenitore — il corpo del documento o una cella —
        ''' nell'ordine in cui stanno scritti: i paragrafi diventano righe, le tabelle
        ''' righe di celle separate da tabulazione. È ricorsiva perché una cella può
        ''' contenerne un'altra.
        ''' </summary>
        Private Shared Sub ScorriBlocchi(contenitore As XElement, testo As StringBuilder)

            For Each blocco As XElement In contenitore.Elements()

                If blocco.Name = W + "p" Then
                    testo.AppendLine(TestoParagrafo(blocco))

                ElseIf blocco.Name = W + "tbl" Then
                    For Each riga As XElement In blocco.Elements(W + "tr")
                        Dim celle As IEnumerable(Of String) =
                            riga.Elements(W + "tc").Select(AddressOf TestoCella)
                        testo.AppendLine(String.Join(vbTab, celle))
                    Next

                ElseIf blocco.Name = W + "sdt" Then
                    ' I modelli di CV di Word avvolgono spesso interi pezzi in un
                    ' «controllo contenuto»: il testo vero sta un livello più sotto.
                    Dim dentro As XElement = blocco.Element(W + "sdtContent")
                    If dentro IsNot Nothing Then ScorriBlocchi(dentro, testo)

                End If

            Next

        End Sub

        ''' <summary>
        ''' Il testo di una cella: i suoi blocchi, appiattiti su una riga sola, perché a
        ''' separare le celle ci pensa la tabulazione.
        ''' </summary>
        Private Shared Function TestoCella(cella As XElement) As String

            Dim dentro As New StringBuilder()
            ScorriBlocchi(cella, dentro)

            Return String.Join(" ", dentro.ToString().
                Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries Or
                                    StringSplitOptions.TrimEntries))

        End Function

        ''' <summary>
        ''' Il testo di un paragrafo. Si prendono solo i pezzi di testo veri
        ''' (<c>w:t</c>): fuori restano da sé i campi calcolati e le parti cancellate con
        ''' le revisioni, che nel file ci sono ma nel documento non si leggono.
        ''' </summary>
        Private Shared Function TestoParagrafo(paragrafo As XElement) As String

            Dim riga As New StringBuilder()

            For Each pezzo As XElement In paragrafo.Descendants()

                If pezzo.Name = W + "t" Then
                    riga.Append(pezzo.Value)
                ElseIf pezzo.Name = W + "tab" Then
                    riga.Append(vbTab)
                ElseIf pezzo.Name = W + "br" OrElse pezzo.Name = W + "cr" Then
                    riga.Append(" "c)
                End If

            Next

            Return riga.ToString().Trim()

        End Function

    End Class

End Namespace
