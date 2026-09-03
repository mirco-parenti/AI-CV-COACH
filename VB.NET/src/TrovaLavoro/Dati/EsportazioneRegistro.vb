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

        ''' <summary>
        ''' Le colonne del riepilogo, nell'ordine in cui escono. «esito» è nata il
        ''' 2026-09-03 insieme a quella della Home, e «stato» ha cambiato mestiere con lei:
        ''' dice a che punto è la procedura, non più il nome dello stato interno.
        ''' </summary>
        Private ReadOnly Intestazioni As String() = {
            "stelle", "eliminatorio", "azienda", "ruolo", "stato", "esito", "fonte", "link",
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
        ''' <param name="documentiObsoleti">
        ''' Le cartelle le cui candidature hanno documenti nati da un profilo che non è più
        ''' quello di oggi. Arrivano <b>già decise</b> da chi chiama, perché la risposta sta
        ''' nello storico del profilo: questo modulo non tocca il disco, ed è la ragione per
        ''' cui i collaudi possono leggerlo senza montare niente. Chi non le passa ottiene
        ''' un riepilogo che di obsolescenza non parla, invece di uno che la nega.
        ''' </param>
        Public Function Componi(voci As IEnumerable(Of VoceRegistro),
                                formato As FormatoEsportazione,
                                Optional contesto As String = Nothing,
                                Optional documentiObsoleti As ISet(Of String) = Nothing) As String

            Dim elenco As New List(Of VoceRegistro)
            If voci IsNot Nothing Then elenco.AddRange(voci)

            If formato = FormatoEsportazione.Markdown Then
                Return ComeMarkdown(elenco, contesto, documentiObsoleti)
            End If

            Return ComeCsv(elenco, documentiObsoleti)

        End Function

        ''' <summary>Se i documenti di questa voce risultano di ieri.</summary>
        ''' <remarks>
        ''' Non può chiamarsi «Obsoleti»: il parametro omonimo di chi la chiama la coprirebbe,
        ''' e in VB la chiamata verrebbe letta come un indice sull'insieme (BC30516, già visto
        ''' altre volte in questo progetto).
        ''' </remarks>
        Private Function DocumentiDiIeri(voce As VoceRegistro, quali As ISet(Of String)) As Boolean

            Return quali IsNot Nothing AndAlso Not String.IsNullOrEmpty(voce.Cartella) AndAlso
                   quali.Contains(voce.Cartella)

        End Function

        ''' <summary>L'estensione che spetta a un formato.</summary>
        Public Function Estensione(formato As FormatoEsportazione) As String
            Return If(formato = FormatoEsportazione.Markdown, ".md", ".csv")
        End Function

        Private Function ComeCsv(voci As List(Of VoceRegistro), obsoleti As ISet(Of String)) As String

            Dim testo As New StringBuilder()
            testo.AppendLine(String.Join(Separatore, Intestazioni))

            For Each voce As VoceRegistro In voci
                testo.AppendLine(String.Join(
                    Separatore, Campi(voce, DocumentiDiIeri(voce, obsoleti)).Select(AddressOf ProtettoPerCsv)))
            Next

            Return testo.ToString()

        End Function

        Private Function ComeMarkdown(voci As List(Of VoceRegistro), contesto As String,
                                      obsoleti As ISet(Of String)) As String

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
                testo.AppendLine("| " & String.Join(
                    " | ", Campi(voce, DocumentiDiIeri(voce, obsoleti)).Select(AddressOf ProtettoPerMarkdown)) & " |")
            Next

            Return testo.ToString()

        End Function

        ''' <summary>I campi di una voce, nell'ordine delle intestazioni.</summary>
        ''' <param name="documentiObsoleti">
        ''' Se i documenti di questa voce non vengono dal profilo di oggi: la stessa cosa
        ''' che nella coda fa diventare rossa la cella.
        ''' </param>
        Private Function Campi(voce As VoceRegistro, documentiObsoleti As Boolean) As String()

            ' Le due colonne del «a che punto sono» escono dagli stessi due posti da cui le
            ' prende la Home — «esce quel che si vede» (cap. 07.3). Dal 2026-09-03 sono due:
            ' quel che si legge dalla cartella e quel che dichiara l'utente. (Il commento
            ' sta qui e non dentro le graffe: in VB una riga di solo commento in mezzo a un
            ' initializer spezza la continuazione implicita.)
            Return New String() {
                If(voce.Stelle.HasValue, voce.Stelle.Value.ToString("0.0", CultureInfo.CurrentCulture), ""),
                If(voce.GateEliminatorio, "sì", "no"),
                If(voce.Azienda, ""),
                If(voce.Titolo, ""),
                StatiOpportunita.Procedura(voce, documentiObsoleti),
                EsitiCandidatura.ComEAndata(voce.Stato, voce.Esito),
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

            Dim valore As String = NonUnaFormula(If(campo, ""))

            If valore.IndexOfAny({";"c, """"c, ControlChars.Cr, ControlChars.Lf}) < 0 Then Return valore

            Return """" & valore.Replace("""", """""") & """"

        End Function

        ''' <summary>
        ''' Un campo che comincia con <c>=</c>, <c>+</c>, <c>-</c> o <c>@</c>, preceduto da un
        ''' apostrofo — che nei fogli di calcolo vuol dire «questo è testo».
        ''' </summary>
        ''' <remarks>
        ''' Il titolo e il nome dell'azienda arrivano dall'annuncio, cioè da un testo che ha
        ''' scritto qualcun altro. Aprendo il CSV in Excel, una cella che comincia con uno di
        ''' quei quattro caratteri non è un titolo: è una <b>formula</b>, e viene eseguita. Le
        ''' virgolette del CSV non bastano, perché proteggono le colonne e non il foglio.
        ''' L'apostrofo si vede, ed è un prezzo che pago volentieri: capita su pochissimi
        ''' titoli, e l'alternativa è un file che esegue quel che l'annuncio gli ha scritto
        ''' dentro. <i>(2026-08-27, dalla revisione del giro D.)</i>
        ''' </remarks>
        Private Function NonUnaFormula(valore As String) As String

            If valore.Length = 0 Then Return valore
            If "=+-@".IndexOf(valore(0)) < 0 Then Return valore
            Return "'" & valore

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
