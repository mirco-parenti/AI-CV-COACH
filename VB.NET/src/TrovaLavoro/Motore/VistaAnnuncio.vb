Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Motore

    ''' <summary>
    ''' L'annuncio strutturato rimesso in forma di testo leggibile: è ciò che P4 mostra
    ''' nella colonna «annuncio estratto» (cap. 03.6), accanto ai giudizi.
    ''' </summary>
    ''' <remarks>
    ''' <para>Si mostra l'annuncio <b>come l'AI l'ha capito</b>, non il testo incollato:
    ''' se l'estrazione ha frainteso qualcosa, l'utente lo vede lì — accanto ai giudizi
    ''' che ne discendono — invece di scoprirlo dai documenti generati.</para>
    ''' <para>Le sezioni vuote non si scrivono: un annuncio che non chiede formazione non
    ''' deve mostrare un titoletto «Formazione richiesta» seguito dal nulla, che si legge
    ''' come un'informazione persa.</para>
    ''' </remarks>
    Public Class VistaAnnuncio

        ''' <summary>Le quattro liste di requisiti, con l'etichetta con cui si mostrano.</summary>
        Private Shared ReadOnly Requisiti As (Campo As String, Titolo As String)() = {
            ("competenze_richieste", "Competenze richieste"),
            ("esperienza_richiesta", "Esperienza richiesta"),
            ("formazione_richiesta", "Formazione richiesta"),
            ("altri_requisiti", "Altri requisiti")}

        ''' <summary>Le liste di sole voci di testo (contesto).</summary>
        Private Shared ReadOnly Elenchi As (Campo As String, Titolo As String)() = {
            ("mansioni", "Mansioni"),
            ("benefit", "Benefit")}

        ''' <summary>
        ''' Il riassunto leggibile dell'annuncio; stringa vuota se non c'è niente da
        ''' mostrare.
        ''' </summary>
        Public Shared Function Riassunto(annuncio As JsonNode) As String

            Dim oggetto As JsonObject = TryCast(annuncio, JsonObject)
            If oggetto Is Nothing Then Return String.Empty

            Dim scritto As New StringBuilder()

            Intestazione(scritto, oggetto)

            ' La variabile del ciclo non si chiama «lista»: in VB coprirebbe la funzione
            ' Lista qui sotto, e le chiamate diventerebbero indicizzazioni.
            For Each gruppo In Requisiti
                Sezione(scritto, gruppo.Titolo, VociConPriorita(oggetto, gruppo.Campo))
            Next

            For Each gruppo In Elenchi
                Sezione(scritto, gruppo.Titolo, Voci(oggetto, gruppo.Campo))
            Next

            Return scritto.ToString().TrimEnd()

        End Function

        ''' <summary>
        ''' L'indirizzo a cui candidarsi, se l'annuncio lo scriveva; stringa vuota
        ''' altrimenti (cap. 07.1).
        ''' </summary>
        ''' <remarks>
        ''' <para>È la metà della promessa che T6 aveva lasciato aperta: il cap. 07.1 dice
        ''' «se l'annuncio conteneva un indirizzo, viene proposto», e fino al Pool 1.06 non
        ''' c'era niente da proporre perché l'analisi non estraeva recapiti. L'altra metà —
        ''' <b>il programma non inventa mai un indirizzo</b> — sta nel prompt, che vieta di
        ''' dedurlo, e qui: quello che non c'è resta vuoto, e a scriverlo è l'utente.</para>
        ''' <para>Si legge solo l'<c>email</c>. Il <c>riferimento</c> — la persona, l'ufficio,
        ''' il codice della posizione — l'annuncio lo dà spesso, ma non è un indirizzo a cui
        ''' si spedisce: metterlo nella casella del destinatario darebbe un'email che non
        ''' parte.</para>
        ''' </remarks>
        Public Shared Function IndirizzoPerCandidarsi(annuncio As JsonNode) As String

            Dim oggetto As JsonObject = TryCast(annuncio, JsonObject)
            If oggetto Is Nothing Then Return String.Empty

            Dim trovato As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue("contatto", trovato) Then Return String.Empty

            Dim contatto As JsonObject = TryCast(trovato, JsonObject)
            If contatto Is Nothing Then Return String.Empty

            Return Testo(contatto, "email")

        End Function

        ''' <summary>Chi cerca, per cosa, dove e a quali condizioni.</summary>
        Private Shared Sub Intestazione(scritto As StringBuilder, annuncio As JsonObject)

            Riga(scritto, "Titolo", Testo(annuncio, "titolo"))
            Riga(scritto, "Azienda", Testo(annuncio, "azienda"))
            Riga(scritto, "Sede", String.Join(" · ", Voci(annuncio, "sede")))
            Riga(scritto, "Contratto", Contratto(annuncio))

        End Sub

        ''' <summary>Le quattro voci del contratto in fila, saltando quelle che l'annuncio tace.</summary>
        Private Shared Function Contratto(annuncio As JsonObject) As String

            Dim oggetto As JsonObject = TryCast(Nodo(annuncio, "contratto"), JsonObject)
            If oggetto Is Nothing Then Return String.Empty

            Dim pezzi As New List(Of String)

            For Each campo As String In {"tipo", "durata", "orario", "retribuzione"}
                Dim valore As String = Testo(oggetto, campo)
                If valore <> "" Then pezzi.Add(valore)
            Next

            Return String.Join(" · ", pezzi)

        End Function

        ''' <summary>
        ''' Le voci di una lista di requisiti: il testo, e fra parentesi quanto pesa —
        ''' quando l'annuncio l'ha davvero dichiarato.
        ''' </summary>
        Private Shared Function VociConPriorita(annuncio As JsonObject, campo As String) As List(Of String)

            Dim raccolte As New List(Of String)

            For Each voce As JsonNode In Lista(annuncio, campo)

                Dim oggetto As JsonObject = TryCast(voce, JsonObject)

                ' Una lista di requisiti fatta di sole stringhe non è la forma che il
                ' prompt chiede, ma se arriva si mostra lo stesso: il pannello non è il
                ' posto in cui litigare col modello.
                If oggetto Is Nothing Then
                    Dim solaVoce As String = ComeTesto(voce)
                    If solaVoce <> "" Then raccolte.Add(solaVoce)
                    Continue For
                End If

                Dim scritta As String = Testo(oggetto, "testo")
                If scritta = "" Then Continue For

                Dim anni As String = Testo(oggetto, "anni")
                If anni <> "" Then scritta &= $" ({anni})"

                Dim priorita As String = Testo(oggetto, "priorita")
                If priorita <> "" AndAlso priorita.Trim().ToLowerInvariant() <> "non specificata" Then
                    scritta &= $" — {priorita}"
                End If

                raccolte.Add(scritta)

            Next

            Return raccolte

        End Function

        ''' <summary>Le voci di una lista di testi (sede, mansioni, benefit).</summary>
        Private Shared Function Voci(annuncio As JsonObject, campo As String) As List(Of String)

            Dim raccolte As New List(Of String)

            For Each voce As JsonNode In Lista(annuncio, campo)
                Dim scritta As String = ComeTesto(voce)
                If scritta <> "" Then raccolte.Add(scritta)
            Next

            Return raccolte

        End Function

        Private Shared Sub Sezione(scritto As StringBuilder, titolo As String, elencate As List(Of String))

            If elencate.Count = 0 Then Return

            If scritto.Length > 0 Then scritto.AppendLine()
            scritto.AppendLine(titolo)

            For Each voce As String In elencate
                scritto.AppendLine("• " & voce)
            Next

        End Sub

        Private Shared Sub Riga(scritto As StringBuilder, etichetta As String, valore As String)

            If String.IsNullOrWhiteSpace(valore) Then Return
            scritto.AppendLine($"{etichetta}: {valore}")

        End Sub

        Private Shared Function Lista(annuncio As JsonObject, campo As String) As JsonArray

            Dim voci As JsonArray = TryCast(Nodo(annuncio, campo), JsonArray)
            Return If(voci, New JsonArray())

        End Function

        Private Shared Function Nodo(oggetto As JsonObject, campo As String) As JsonNode

            Dim valore As JsonNode = Nothing
            If oggetto Is Nothing OrElse Not oggetto.TryGetPropertyValue(campo, valore) Then Return Nothing
            Return valore

        End Function

        Private Shared Function Testo(oggetto As JsonObject, campo As String) As String
            Return ComeTesto(Nodo(oggetto, campo))
        End Function

        ''' <summary>Il valore come testo: i numeri si scrivono, tutto il resto è vuoto.</summary>
        Private Shared Function ComeTesto(valore As JsonNode) As String

            If valore Is Nothing Then Return String.Empty

            Select Case valore.GetValueKind()
                Case JsonValueKind.String : Return valore.GetValue(Of String)().Trim()
                Case JsonValueKind.Number : Return valore.ToJsonString()
                Case Else : Return String.Empty
            End Select

        End Function

    End Class

End Namespace
