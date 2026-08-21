Imports System.Globalization
Imports System.Linq
Imports System.Text

Namespace Dati

    ''' <summary>I due modi in cui il registro esce dall'applicazione (cap. 07.3).</summary>
    Public Enum FormatoEsportazione

        ''' <summary>Per un foglio di calcolo: una riga per candidatura, niente commenti.</summary>
        Csv

        ''' <summary>Per essere letto e incollato in un documento: tabella, titolo, contesto.</summary>
        Markdown

    End Enum

    ''' <summary>
    ''' Il registro in un riepilogo leggibile fuori dall'applicazione (cap. 07.3). Non è un
    ''' backup — quello è JSON, cap. 11.4, e serve a tornare indietro: questo serve a
    ''' <b>raccontare</b>, ed è la ragione per cui il capitolo lo promette fin dal primo
    ''' rilascio. Per Mirco documenta nero su bianco l'uso reale del prodotto nella propria
    ''' ricerca di lavoro.
    ''' </summary>
    ''' <remarks>
    ''' Qui non si tocca il disco e non si sa niente di finestre: si compone del testo. Chi
    ''' lo chiama decide dove finisce, e i collaudi possono guardarlo senza scrivere niente.
    ''' </remarks>
    Public Module EsportazioneRegistro

        ''' <summary>
        ''' Il punto e virgola invece della virgola. Non è una libertà: Excel apre un `.csv`
        ''' usando il separatore di elenco della lingua di Windows, che in italiano è il
        ''' punto e virgola — con la virgola l'intero foglio finirebbe in una colonna sola.
        ''' E siccome le stelle si scrivono «4,1», la virgola sarebbe pure ambigua.
        ''' </summary>
        Public Const Separatore As String = ";"

        ''' <summary>
        ''' Le date in forma ISO. Un `14/08/2026` verrebbe letto in un modo diverso da ogni
        ''' programma che lo apre — e al contrario di quel che si crede, l'ordine
        ''' alfabetico di questo formato è anche l'ordine cronologico.
        ''' </summary>
        Private Const Quando As String = "yyyy-MM-dd HH:mm"

        Private ReadOnly Intestazioni As String() = {
            "stelle", "eliminatorio", "azienda", "ruolo", "stato", "fonte", "link",
            "lingua", "creata", "aggiornata", "cartella"}

        ''' <summary>
        ''' Il riepilogo, pronto da scrivere su un file.
        ''' </summary>
        ''' <param name="voci">Le candidature da riportare, nell'ordine in cui vanno lette.</param>
        ''' <param name="formato">Per il foglio di calcolo o per essere letto.</param>
        ''' <param name="contesto">
        ''' Una riga che dice <i>cosa</i> si sta guardando — il filtro in vigore, di norma.
        ''' Entra solo nel markdown: un CSV con una frase in cima non è più una tabella, e
        ''' il foglio di calcolo se ne accorgerebbe con una colonna sballata.
        ''' </param>
        Public Function Componi(voci As IEnumerable(Of VoceRegistro),
                                formato As FormatoEsportazione,
                                Optional contesto As String = Nothing) As String

            Dim elenco As New List(Of VoceRegistro)
            If voci IsNot Nothing Then elenco.AddRange(voci)

            If formato = FormatoEsportazione.Markdown Then Return ComeMarkdown(elenco, contesto)
            Return ComeCsv(elenco)

        End Function

        ''' <summary>L'estensione che spetta a un formato.</summary>
        Public Function Estensione(formato As FormatoEsportazione) As String
            Return If(formato = FormatoEsportazione.Markdown, ".md", ".csv")
        End Function

        Private Function ComeCsv(voci As List(Of VoceRegistro)) As String

            Dim testo As New StringBuilder()
            testo.AppendLine(String.Join(Separatore, Intestazioni))

            For Each voce As VoceRegistro In voci
                testo.AppendLine(String.Join(Separatore, Campi(voce).Select(AddressOf ProtettoPerCsv)))
            Next

            Return testo.ToString()

        End Function

        Private Function ComeMarkdown(voci As List(Of VoceRegistro), contesto As String) As String

            Dim testo As New StringBuilder()
            testo.AppendLine("# Registro delle candidature")
            testo.AppendLine()

            If Not String.IsNullOrWhiteSpace(contesto) Then
                testo.AppendLine($"*{contesto}*")
                testo.AppendLine()
            End If

            If voci.Count = 0 Then
                ' Una tabella con la sola intestazione sembrerebbe un errore di
                ' esportazione, e chi la legge fra sei mesi non saprebbe di quale dei due
                ' si tratta: quel che manca si dice a parole.
                testo.AppendLine("Nessuna candidatura da riportare.")
                Return testo.ToString()
            End If

            testo.AppendLine("| " & String.Join(" | ", Intestazioni) & " |")
            testo.AppendLine("|" & String.Join("|", Intestazioni.Select(Function(i) "---")) & "|")

            For Each voce As VoceRegistro In voci
                testo.AppendLine("| " & String.Join(" | ", Campi(voce).Select(AddressOf ProtettoPerMarkdown)) & " |")
            Next

            Return testo.ToString()

        End Function

        ''' <summary>I campi di una voce, nell'ordine delle intestazioni.</summary>
        Private Function Campi(voce As VoceRegistro) As String()

            ' Da T9c la colonna «stato» porta la stessa parola che si legge nella Home —
            ' «esce quel che si vede» (cap. 07.3), e «Rifiutata» dice quel che «Con esito»
            ' tace. (Il commento sta qui e non dentro le graffe: in VB una riga di solo
            ' commento in mezzo a un initializer spezza la continuazione implicita.)
            Return New String() {
                If(voce.Stelle.HasValue, voce.Stelle.Value.ToString("0.0", CultureInfo.CurrentCulture), ""),
                If(voce.GateEliminatorio, "sì", "no"),
                If(voce.Azienda, ""),
                If(voce.Titolo, ""),
                EsitiCandidatura.EtichettaDi(voce.Stato, voce.Esito),
                If(voce.Fonte, ""),
                If(voce.Link, ""),
                If(voce.Lingua, ""),
                voce.Creata.ToString(Quando, CultureInfo.InvariantCulture),
                voce.Aggiornata.ToString(Quando, CultureInfo.InvariantCulture),
                If(voce.Cartella, "")}

        End Function

        ''' <summary>
        ''' Un campo come lo vuole un CSV: fra virgolette se contiene il separatore, delle
        ''' virgolette o un a capo, e con le virgolette raddoppiate. Il titolo di un
        ''' annuncio contiene di tutto, e un punto e virgola lasciato libero sposterebbe
        ''' tutte le colonne di quella riga.
        ''' </summary>
        Private Function ProtettoPerCsv(campo As String) As String

            Dim valore As String = If(campo, "")

            If valore.IndexOfAny({";"c, """"c, ControlChars.Cr, ControlChars.Lf}) < 0 Then Return valore

            Return """" & valore.Replace("""", """""") & """"

        End Function

        ''' <summary>
        ''' Un campo come lo vuole una tabella markdown: le barre verticali si proteggono e
        ''' gli a capo diventano spazi, altrimenti la riga si spezza in due e la tabella si
        ''' rompe da lì in giù.
        ''' </summary>
        Private Function ProtettoPerMarkdown(campo As String) As String

            Return If(campo, "").
                Replace("|", "\|").
                Replace(ControlChars.CrLf, " ").
                Replace(ControlChars.Cr, " "c).
                Replace(ControlChars.Lf, " "c)

        End Function

    End Module

End Namespace
