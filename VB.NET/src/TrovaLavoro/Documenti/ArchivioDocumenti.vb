Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Documenti

    ''' <summary>
    ''' Chi mette i documenti al loro posto: prende un CV o una lettera in JSON, li fa
    ''' impaginare, li stampa nei due formati e li scrive dove devono stare — accanto al
    ''' profilo il 📄 CV base, nella cartella della candidatura tutto il resto
    ''' (cap. 05.6, cap. 11.1).
    ''' </summary>
    ''' <remarks>
    ''' <para>È il punto in cui si incontrano le tre cose che T4b ha costruito — il
    ''' modello di impaginazione, le due stampanti, i nomi dei file — e l'unico che sappia
    ''' <b>dove</b> vanno i documenti. I pannelli di T4c chiameranno questo, non le
    ''' stampanti.</para>
    ''' <para><b>Prima tutti i DOCX, poi tutti i PDF</b>, e non per ordine mentale: la
    ''' stampa PDF è l'unica che dipende da un pezzo di Windows che potrebbe mancare
    ''' (cap. 13.3). Facendola per ultima, il giorno in cui non si potesse fare l'utente
    ''' avrebbe comunque in mano i suoi documenti in formato Word.</para>
    ''' <para><b>Senza stampante non si stampa</b>, e non è un guasto: chi monta il
    ''' motore decide se il PDF si può fare, e questo archivio scrive quello che può,
    ''' dicendo con l'elenco che restituisce quali file esistono davvero. È la stessa
    ''' regola dell'archivio delle opportunità: si scrive solo ciò che c'è.</para>
    ''' </remarks>
    Public Class ArchivioDocumenti

        Private ReadOnly _cartella As CartellaDati
        Private ReadOnly _stampante As StampantePdf

        ''' <param name="cartella">La mappa della cartella dati.</param>
        ''' <param name="stampante">
        ''' La stampante PDF; se manca, si scrivono i soli DOCX.
        ''' </param>
        Public Sub New(cartella As CartellaDati, Optional stampante As StampantePdf = Nothing)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))

            _cartella = cartella
            _stampante = stampante

        End Sub

        ''' <summary>
        ''' Scrive il 📄 <b>CV base</b> in <c>profilo\out</c> e restituisce i file scritti.
        ''' </summary>
        ''' <param name="cv">Il CV base JSON.</param>
        ''' <param name="quando">Il giorno da mettere nel nome; oggi, se non si dice.</param>
        Public Function ScriviCvBaseAsync(cv As JsonNode,
                                          Optional quando As Date = Nothing) As Task(Of IReadOnlyList(Of String))

            If cv Is Nothing Then Throw New ArgumentNullException(NameOf(cv))

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(cv)
            Dim giorno As Date = If(quando = Nothing, Date.Today, quando)

            ' Nessuna azienda: il CV base non nasce da un annuncio, e il suo nome lo dice
            ' non nominandone nessuna (cap. 05.6).
            Dim nome As String = NomiDocumenti.Cv(DiChiE(pagina), String.Empty, giorno)

            Return ScriviAsync(_cartella.CartellaOutProfilo,
                               New List(Of Lavoro) From {New Lavoro(pagina, nome)})

        End Function

        ''' <summary>
        ''' Scrive i documenti di una candidatura — il 🎯 CV mirato e la ✉️ lettera — nella
        ''' cartella <c>out</c> dell'opportunità, e restituisce i file scritti.
        ''' </summary>
        ''' <remarks>
        ''' Si scrive quello che c'è: un'opportunità con il solo CV genera il solo CV. Nel
        ''' flusso reale i due documenti arrivano insieme, ma una generazione interrotta a
        ''' metà non deve lasciare a mani vuote.
        ''' </remarks>
        ''' <param name="opportunita">La candidatura, già salvata su disco.</param>
        Public Function ScriviCandidaturaAsync(opportunita As Opportunita) As Task(Of IReadOnlyList(Of String))

            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))

            Dim lavori As New List(Of Lavoro)

            If opportunita.Cv IsNot Nothing Then
                Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(opportunita.Cv)
                lavori.Add(New Lavoro(pagina, NomiDocumenti.Cv(
                    DiChiE(pagina), opportunita.Azienda, opportunita.Creata, opportunita.Lingua)))
            End If

            If opportunita.Lettera IsNot Nothing Then
                lavori.Add(New Lavoro(
                    Impaginazione.PaginaLettera(opportunita.Lettera),
                    NomiDocumenti.Lettera(opportunita.Azienda, opportunita.Creata, opportunita.Lingua)))
            End If

            Return ScriviAsync(ArchivioOpportunita.CartellaOut(opportunita), lavori)

        End Function

        ''' <summary>Un documento da scrivere: la sua pagina e come si chiamerà.</summary>
        Private NotInheritable Class Lavoro

            Public ReadOnly Pagina As PaginaDocumento
            Public ReadOnly Nome As String

            Public Sub New(pagina As PaginaDocumento, nome As String)
                Me.Pagina = pagina
                Me.Nome = nome
            End Sub

        End Class

        ''' <summary>
        ''' Scrive i documenti nella cartella indicata: prima tutti i DOCX, poi — se c'è
        ''' una stampante — tutti i PDF.
        ''' </summary>
        Private Async Function ScriviAsync(cartella As String,
                                           lavori As List(Of Lavoro)) As Task(Of IReadOnlyList(Of String))

            Dim scritti As New List(Of String)
            If lavori.Count = 0 Then Return scritti

            Directory.CreateDirectory(cartella)

            For Each lavoro As Lavoro In lavori
                Dim percorso As String = Path.Combine(cartella, lavoro.Nome & NomiDocumenti.EstensioneDocx)
                ScrittoreDocx.Scrivi(lavoro.Pagina, percorso)
                scritti.Add(percorso)
            Next

            If _stampante Is Nothing Then Return scritti

            For Each lavoro As Lavoro In lavori
                Dim percorso As String = Path.Combine(cartella, lavoro.Nome & NomiDocumenti.EstensionePdf)
                ' ConfigureAwait(True): la stampante vive sul thread dell'interfaccia e
                ' lì deve restare (v. StampantePdf).
                Await _stampante.StampaAsync(lavoro.Pagina, percorso).ConfigureAwait(True)
                scritti.Add(percorso)
            Next

            Return scritti

        End Function

        ''' <summary>
        ''' Di chi è il documento: il nome che l'impaginazione ha già ricavato dal CV.
        ''' Si legge dalla pagina e non un'altra volta dal JSON, perché il nome sul file e
        ''' il nome stampato dentro devono essere lo stesso nome.
        ''' </summary>
        Private Shared Function DiChiE(pagina As PaginaDocumento) As String

            Dim intestazione As Blocco = pagina.Blocchi.Find(Function(b) b.Genere = GenereBlocco.Nome)
            Return If(intestazione Is Nothing, String.Empty, intestazione.Testo)

        End Function

    End Class

End Namespace
