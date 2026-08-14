Imports System.IO
Imports System.Linq

Namespace Dati

    ''' <summary>
    ''' Un file trovato nella cartella documenti: quel che serve a riconoscerlo senza
    ''' ancora averlo letto per intero (cap. 05.2, passi 1 e 2).
    ''' </summary>
    Public Class FileTrovato

        ''' <summary>Il nome relativo alla cartella: «HACCP.pdf», «attestati\muletto.pdf».</summary>
        Public Property Nome As String

        ''' <summary>Dove sta davvero, adesso.</summary>
        Public Property Percorso As String

        Public Property Modificato As Date
        Public Property Dimensione As Long

        ''' <summary>
        ''' Le prime righe del testo, o <c>Nothing</c> quando non si è potuto leggerlo.
        ''' </summary>
        ''' <remarks>
        ''' Per i <b>PDF</b> è sempre <c>Nothing</c>, ed è una scelta: il disco non basta a
        ''' leggerli — servirebbe una trascrizione dell'AI per ciascuno (cap. 05.1) — e in
        ''' una cartella di documenti i PDF sono quasi tutto. Una chiamata per file
        ''' costerebbe minuti e denaro per smistare della carta. Il prompt lo sa e se ne fa
        ''' carico: giudica sul nome e, quando non basta, lo dice invece di indovinare.
        ''' </remarks>
        Public Property Assaggio As String

    End Class

    ''' <summary>
    ''' La lettura della cartella documenti dell'utente (cap. 05.2): elenca i file
    ''' leggibili e ne assaggia il testo, senza chiamare nessuno.
    ''' </summary>
    ''' <remarks>
    ''' Non entra oltre le <b>sottocartelle di primo livello</b>, come dice il capitolo:
    ''' una cartella «documenti» qualunque ha dentro di tutto, e scendere all'infinito
    ''' vorrebbe dire proporre a un'azienda un file pescato chissà dove.
    ''' </remarks>
    Public Class ScansioneDocumenti

        ''' <summary>
        ''' Quanti file al massimo si mandano a classificare. Non è un limite tecnico ma
        ''' una misura: l'elenco entra in un prompt, e una cartella con trecento file lo
        ''' farebbe scoppiare. Quel che resta fuori <b>si dice</b> (v. <c>lasciatiFuori</c>).
        ''' </summary>
        Public Const MassimoFile As Integer = 60

        ''' <summary>Quanto testo si assaggia: «la prima pagina», in caratteri (cap. 05.2).</summary>
        Public Const CaratteriAssaggio As Integer = 600

        ''' <summary>
        ''' Oltre questa dimensione il file non si assaggia: leggerlo tutto per prenderne
        ''' seicento caratteri costerebbe più di quel che rende.
        ''' </summary>
        Private Const DimensioneMassimaAssaggio As Long = 2L * 1024L * 1024L

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Legge la cartella. Non solleva: una cartella sparita o un file che non si
        ''' lascia aprire sono cose che capitano, e nessuna di loro è un guasto del
        ''' programma.
        ''' </summary>
        ''' <param name="cartella">La cartella scelta dall'utente.</param>
        ''' <param name="lasciatiFuori">
        ''' Quanti file leggibili sono rimasti fuori per via del <see cref="MassimoFile"/>.
        ''' Chi chiama lo deve dire: un elenco troncato in silenzio si legge come «nella
        ''' cartella non c'era altro».
        ''' </param>
        Public Shared Function Leggi(cartella As String, ByRef lasciatiFuori As Integer) As List(Of FileTrovato)

            lasciatiFuori = 0

            Dim trovati As New List(Of FileTrovato)
            If String.IsNullOrWhiteSpace(cartella) OrElse Not Directory.Exists(cartella) Then Return trovati

            Dim percorsi As List(Of String) = PercorsiLeggibili(cartella)

            If percorsi.Count > MassimoFile Then
                lasciatiFuori = percorsi.Count - MassimoFile
                percorsi = percorsi.Take(MassimoFile).ToList()
            End If

            For Each percorso As String In percorsi

                Dim informazioni As FileInfo

                Try
                    informazioni = New FileInfo(percorso)
                Catch ex As Exception When TypeOf ex Is IOException OrElse
                                           TypeOf ex Is UnauthorizedAccessException
                    Continue For
                End Try

                trovati.Add(New FileTrovato With {
                    .Nome = Path.GetRelativePath(cartella, percorso),
                    .Percorso = percorso,
                    .Modificato = informazioni.LastWriteTime,
                    .Dimensione = informazioni.Length,
                    .Assaggio = Assaggia(percorso, informazioni.Length)})

            Next

            Return trovati

        End Function

        ''' <summary>
        ''' I file dei formati che l'applicazione sa leggere, nella cartella e nelle sue
        ''' sottocartelle di primo livello, in ordine di nome.
        ''' </summary>
        Private Shared Function PercorsiLeggibili(cartella As String) As List(Of String)

            Dim percorsi As New List(Of String)

            Try
                percorsi.AddRange(Directory.EnumerateFiles(cartella))

                For Each sotto As String In Directory.EnumerateDirectories(cartella)
                    Try
                        percorsi.AddRange(Directory.EnumerateFiles(sotto))
                    Catch ex As Exception When TypeOf ex Is IOException OrElse
                                               TypeOf ex Is UnauthorizedAccessException
                        ' Una sottocartella che non si lascia aprire non ferma le altre.
                    End Try
                Next

            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                Return percorsi
            End Try

            Return percorsi.
                Where(Function(p) LettoreDocumenti.Formato(p) <> FormatoDocumento.NonSupportato).
                OrderBy(Function(p) Path.GetFileName(p), StringComparer.CurrentCultureIgnoreCase).
                ToList()

        End Function

        ''' <summary>
        ''' Le prime righe del file, o <c>Nothing</c> se non si possono avere dal disco.
        ''' </summary>
        Private Shared Function Assaggia(percorso As String, dimensione As Long) As String

            If dimensione > DimensioneMassimaAssaggio Then Return Nothing
            If LettoreDocumenti.Formato(percorso) = FormatoDocumento.Pdf Then Return Nothing

            Dim testo As String

            Try
                testo = LettoreDocumenti.LeggiTesto(percorso)
            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException OrElse
                                       TypeOf ex Is InvalidDataException OrElse
                                       TypeOf ex Is NotSupportedException
                Return Nothing
            End Try

            If String.IsNullOrWhiteSpace(testo) Then Return Nothing

            testo = testo.Trim()
            If testo.Length <= CaratteriAssaggio Then Return testo

            Return testo.Substring(0, CaratteriAssaggio) & "…"

        End Function

    End Class

End Namespace
