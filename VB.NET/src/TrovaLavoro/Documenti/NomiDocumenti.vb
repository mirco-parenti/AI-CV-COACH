Imports System.Globalization
Imports System.Text

Namespace Documenti

    ''' <summary>
    ''' Come si chiamano i file che il programma produce (cap. 05.6): nomi <b>parlanti e
    ''' stabili</b>, che dicano da soli di chi è il CV, per quale azienda e di che
    ''' giorno.
    ''' </summary>
    ''' <remarks>
    ''' <para>Il nome di un documento non è un dettaglio: quel file finisce allegato a
    ''' un'email, scaricato da chi lo riceve e ritrovato mesi dopo in una cartella piena
    ''' di altri CV. <c>CV_Luca_Ferrari_Rossi_2026-08-10.docx</c> si riconosce a colpo
    ''' d'occhio; <c>documento(3).docx</c> no.</para>
    ''' <para>Il nome esce <b>senza estensione</b>: lo stesso documento va in DOCX e in
    ''' PDF, e i due file devono chiamarsi uguali per finire vicini quando la cartella si
    ''' ordina per nome.</para>
    ''' <para>Gli accenti si sciolgono nella lettera che li regge, come nei nomi delle
    ''' cartelle-opportunità: un file che viaggia per email, su una chiavetta o dentro un
    ''' portale non deve dipendere da come quel sistema tratta le lettere accentate.</para>
    ''' </remarks>
    Public Class NomiDocumenti

        ''' <summary>La lingua che non si scrive nel nome, perché è quella di casa.</summary>
        Public Const LinguaPredefinita As String = "it"

        ''' <summary>Le estensioni dei due formati in cui esce ogni documento.</summary>
        Public Const EstensioneDocx As String = ".docx"
        Public Const EstensionePdf As String = ".pdf"

        ''' <summary>Quanto può essere lungo un pezzo del nome (nome, azienda).</summary>
        Private Const Massimo As Integer = 40

        ''' <summary>
        ''' Il nome del file di un CV, senza estensione. L'azienda si lascia fuori quando
        ''' non c'è: è il caso del 📄 CV base, che un'azienda non ce l'ha (cap. 05.6).
        ''' </summary>
        ''' <param name="nome">Il nome della persona, come sta nel CV.</param>
        ''' <param name="azienda">L'azienda dell'annuncio; vuota per il CV base.</param>
        ''' <param name="quando">Il giorno del documento.</param>
        ''' <param name="lingua">La lingua del documento; l'italiano non si scrive.</param>
        Public Shared Function Cv(nome As String, azienda As String, quando As Date,
                                  Optional lingua As String = LinguaPredefinita) As String

            Return Insieme({"CV", Pezzo(nome), Sigla(lingua), Pezzo(azienda), Giorno(quando)})

        End Function

        ''' <summary>
        ''' Il nome del file di una lettera, senza estensione. Il nome della persona non
        ''' c'è, come dice il cap. 05.6: la lettera si riconosce dall'azienda a cui è
        ''' indirizzata.
        ''' </summary>
        Public Shared Function Lettera(azienda As String, quando As Date,
                                       Optional lingua As String = LinguaPredefinita) As String

            Return Insieme({"Lettera", Sigla(lingua), Pezzo(azienda), Giorno(quando)})

        End Function

        ''' <summary>I pezzi che ci sono, uniti da un trattino basso.</summary>
        Private Shared Function Insieme(pezzi As IEnumerable(Of String)) As String

            Dim scritto As New List(Of String)
            For Each pezzo As String In pezzi
                If pezzo.Length > 0 Then scritto.Add(pezzo)
            Next

            Return String.Join("_", scritto)

        End Function

        ''' <summary>
        ''' La sigla della lingua, in maiuscolo, per i documenti che non sono in italiano
        ''' (cap. 05.6: <c>CV_Mirco_Parenti_EN_…</c>). Arriva con T7; il nome sa già
        ''' aspettarla, perché è più facile scriverlo adesso che ricordarsene poi.
        ''' </summary>
        Private Shared Function Sigla(lingua As String) As String

            If String.IsNullOrWhiteSpace(lingua) Then Return String.Empty
            If lingua.Trim().Equals(LinguaPredefinita, StringComparison.OrdinalIgnoreCase) Then
                Return String.Empty
            End If

            Return Pezzo(lingua).ToUpperInvariant()

        End Function

        ''' <summary>Il giorno, scritto in modo che l'ordine alfabetico sia quello del tempo.</summary>
        Private Shared Function Giorno(quando As Date) As String
            Return quando.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        End Function

        ''' <summary>
        ''' Un pezzo di nome ridotto a ciò che qualunque disco e qualunque programma di
        ''' posta accettano: lettere senza accento, cifre e trattini bassi. «Rossi &amp;
        ''' Figli S.p.A.» diventa «Rossi_Figli_S_p_A», e si taglia a
        ''' <see cref="Massimo"/> caratteri perché il percorso intero resti maneggevole.
        ''' </summary>
        Private Shared Function Pezzo(testo As String) As String

            If String.IsNullOrWhiteSpace(testo) Then Return String.Empty

            ' La normalizzazione è ciò che salva le lettere accentate: senza, «ì» è un
            ' carattere solo che il filtro qui sotto butterebbe insieme al suo accento
            ' («Ferrarì» diventerebbe «Ferrar»); con essa diventa «i» più un segno a
            ' parte, e a passare è la lettera.
            Dim sciolto As String = testo.Trim().Normalize(NormalizationForm.FormD)

            Dim costruito As New StringBuilder()
            For Each c As Char In sciolto

                If Char.IsLetterOrDigit(c) AndAlso AscW(c) < 128 Then
                    costruito.Append(c)
                ElseIf costruito.Length > 0 AndAlso costruito(costruito.Length - 1) <> "_"c Then
                    costruito.Append("_"c)
                End If

                If costruito.Length >= Massimo Then Exit For

            Next

            Return costruito.ToString().Trim("_"c)

        End Function

    End Class

End Namespace
