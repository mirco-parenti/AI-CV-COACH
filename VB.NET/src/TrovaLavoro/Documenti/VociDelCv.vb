Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Dati

Namespace Documenti

    ''' <summary>
    ''' Una voce elencata di un CV — un'esperienza, una competenza, un titolo di studio —
    ''' vista come qualcosa che si può <b>togliere da questo documento</b> (R6, cap. 08.4).
    ''' </summary>
    Public Class VoceDelCv

        ''' <summary>La lista del CV in cui sta: <c>competenze</c>, <c>formazione</c>…</summary>
        Public Property Sezione As String

        ''' <summary>Dove sta adesso dentro quella lista, contando da zero.</summary>
        Public Property Indice As Integer

        ''' <summary>Chi è questa voce, scritto in modo che si riconosca anche domani.</summary>
        Public Property Impronta As String

        ''' <summary>Come si chiama nell'elenco a video: «Esperienza 2», «Competenza 5».</summary>
        Public Property Etichetta As String

        ''' <summary>Cosa dice, in una riga: serve a riconoscerla senza aprirla.</summary>
        Public Property Riepilogo As String

    End Class

    ''' <summary>
    ''' Le voci elencate di un CV: quali sono, come si chiamano, e come si toglie dal
    ''' documento quelle che l'utente non ci vuole (R6, 2026-08-24).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché un posto solo.</b> Un CV lo leggono in molti: l'anteprima di P6, il
    ''' DOCX, il PDF, l'HTML, il prompt della lettera e i tool del server MCP. I primi
    ''' quattro passano già tutti da <see cref="Impaginazione.PaginaCv"/>, gli altri due
    ''' leggono il JSON com'è. Se ognuno decidesse da sé quali voci mostrare, una voce
    ''' tolta sparirebbe dal PDF e resterebbe nella lettera — e chi legge non saprebbe
    ''' quale dei due dice la verità. Perciò il taglio si fa <b>qui</b>, e tutti passano
    ''' di qui.</para>
    ''' <para><b>Perché per contenuto e non per posizione.</b> Gli id della rifinitura
    ''' (<c>esperienza.1</c>) contano le voci: vanno benissimo per dire «la descrizione
    ''' della seconda esperienza», perché in quel momento la seconda esperienza è quella.
    ''' Ma una voce tolta deve restare tolta anche dopo un «Rigenera», e il documento
    ''' nuovo lo scrive il modello: basta che ne salti una, o che il profilo cambi, perché
    ''' l'indice 2 sia un'altra esperienza — e si toglierebbe la voce sbagliata senza che
    ''' nessuno se ne accorga. L'impronta guarda invece i <b>fatti</b> della voce, che
    ''' vengono dal profilo e non cambiano quando il modello riscrive.</para>
    ''' <para><b>Perché non la descrizione.</b> Nell'impronta non entra mai la prosa: è la
    ''' parte che il modello riscrive a ogni giro, e che l'utente può riscrivere a mano
    ''' (R7). Un'impronta che la contenesse non si riaggancerebbe più dopo la prima
    ''' rigenerazione. Per le «altre esperienze», che di fatti hanno il solo
    ''' <c>quando</c>, si usa quello; e se manca anche lui, allora sì la descrizione — con
    ''' il limite che ne consegue, dichiarato qui invece che scoperto dall'utente.</para>
    ''' </remarks>
    Public Module VociDelCv

        ''' <summary>Le liste del CV che contengono voci togliibili, nell'ordine in cui si leggono.</summary>
        Public ReadOnly Sezioni As String() =
            {"esperienze_professionali", "altre_esperienze", "competenze", "formazione"}

        ''' <summary>
        ''' Separa i pezzi dentro un'impronta. È una barra spezzata perché nei fatti di un
        ''' CV non compare: un separatore che può comparire nel testo fonde due voci
        ''' diverse nella stessa impronta.
        ''' </summary>
        Private Const Separatore As String = "¦"

        ''' <summary>
        ''' Le voci di questo CV, in ordine di lettura. Un CV senza liste ne restituisce
        ''' nessuna, che è la risposta giusta per una lettera o per un documento vuoto.
        ''' </summary>
        Public Function Elenca(cv As JsonNode) As List(Of VoceDelCv)

            Dim voci As New List(Of VoceDelCv)
            Dim radice As JsonObject = TryCast(cv, JsonObject)
            If radice Is Nothing Then Return voci

            For Each sezione As String In Sezioni

                Dim lista As JsonArray = TryCast(CampiJson.Nodo(radice, sezione), JsonArray)
                If lista Is Nothing Then Continue For

                For indice As Integer = 0 To lista.Count - 1

                    Dim impronta As String = ImprontaDi(sezione, lista(indice))
                    If impronta Is Nothing Then Continue For

                    voci.Add(New VoceDelCv With {
                        .Sezione = sezione,
                        .Indice = indice,
                        .Impronta = impronta,
                        .Etichetta = $"{NomeDellaSezione(sezione)} {indice + 1}",
                        .Riepilogo = RiepilogoDi(sezione, lista(indice))})

                Next

            Next

            Return voci

        End Function

        ''' <summary>
        ''' Il CV come va letto adesso: una copia senza le voci che l'utente ha tolto. Il
        ''' documento originale non si tocca — su disco resta intero, ed è per questo che
        ''' rimettere una voce non costa una rigenerazione.
        ''' </summary>
        Public Function ComeSiVede(cv As JsonNode, tolte As VociTolte) As JsonNode

            If cv Is Nothing Then Return Nothing
            If tolte Is Nothing OrElse Not tolte.CEQualcosa Then Return cv

            Dim copia As JsonNode = cv.DeepClone()
            Dim radice As JsonObject = TryCast(copia, JsonObject)
            If radice Is Nothing Then Return copia

            For Each sezione As String In Sezioni

                Dim lista As JsonArray = TryCast(CampiJson.Nodo(radice, sezione), JsonArray)
                If lista Is Nothing Then Continue For

                ' All'indietro: togliendo dal fondo, gli indici di quel che resta da
                ' guardare non si spostano sotto i piedi.
                For indice As Integer = lista.Count - 1 To 0 Step -1

                    Dim impronta As String = ImprontaDi(sezione, lista(indice))
                    If impronta IsNot Nothing AndAlso tolte.Contiene(impronta) Then
                        lista.RemoveAt(indice)
                    End If

                Next

            Next

            Return copia

        End Function

        ''' <summary>
        ''' Chi è una voce, per contenuto. <c>Nothing</c> se non ha niente che la
        ''' distingua: una voce senza fatti non si può nominare, e toglierla vorrebbe dire
        ''' toglierne un'altra per sbaglio.
        ''' </summary>
        Public Function ImprontaDi(sezione As String, voce As JsonNode) As String

            If voce Is Nothing Then Return Nothing

            Dim pezzi As List(Of String)

            Select Case sezione

                Case "competenze"
                    ' Una competenza è già una stringa nuda: è lei stessa il suo fatto.
                    pezzi = New List(Of String) From {TestoDi(voce)}

                Case "esperienze_professionali"
                    Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                    If oggetto Is Nothing Then Return Nothing
                    pezzi = New List(Of String) From {
                        CampiJson.Testo(oggetto, "ruolo"),
                        CampiJson.Testo(oggetto, "azienda"),
                        CampiJson.Testo(oggetto, "durata")}

                Case "formazione"
                    Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                    If oggetto Is Nothing Then Return Nothing
                    pezzi = New List(Of String) From {
                        CampiJson.Testo(oggetto, "titolo"),
                        CampiJson.Testo(oggetto, "istituto"),
                        CampiJson.Testo(oggetto, "anno")}

                Case "altre_esperienze"
                    Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                    If oggetto Is Nothing Then Return Nothing
                    Dim quando As String = CampiJson.Testo(oggetto, "quando")
                    ' Il «quando» viene dal profilo e resta; la descrizione la riscrive il
                    ' modello. Si ripiega su di lei solo per non lasciare la voce senza
                    ' nome — e allora l'esclusione vale per il documento di adesso.
                    pezzi = New List(Of String) From {
                        If(quando.Trim().Length > 0, quando, CampiJson.Testo(oggetto, "descrizione"))}

                Case Else
                    Return Nothing

            End Select

            Dim scritti As List(Of String) = pezzi.Select(AddressOf Normalizza).
                                                   Where(Function(p) p.Length > 0).ToList()

            If scritti.Count = 0 Then Return Nothing

            Return sezione & Separatore & String.Join(Separatore, scritti)

        End Function

        ''' <summary>
        ''' Toglie dal testo quel che cambia senza cambiare la voce: spazi doppi, spazi ai
        ''' bordi, maiuscole. Due scritture della stessa cosa devono dare la stessa
        ''' impronta, o l'esclusione si perde al primo ritocco tipografico del modello.
        ''' </summary>
        Private Function Normalizza(testo As String) As String

            If String.IsNullOrWhiteSpace(testo) Then Return String.Empty

            Dim pulito As New Text.StringBuilder()
            Dim spazioPrima As Boolean = False

            For Each c As Char In testo.Trim()

                If Char.IsWhiteSpace(c) Then
                    If Not spazioPrima Then pulito.Append(" "c)
                    spazioPrima = True
                Else
                    pulito.Append(Char.ToLowerInvariant(c))
                    spazioPrima = False
                End If

            Next

            Return pulito.ToString().Trim()

        End Function

        ''' <summary>Cosa dice la voce, in una riga sola.</summary>
        Private Function RiepilogoDi(sezione As String, voce As JsonNode) As String

            Dim oggetto As JsonObject = TryCast(voce, JsonObject)

            Select Case sezione

                Case "competenze"
                    Return TestoDi(voce)

                Case "esperienze_professionali"
                    If oggetto Is Nothing Then Return String.Empty
                    Return Unisci(" — ", CampiJson.Testo(oggetto, "ruolo"),
                                         CampiJson.Testo(oggetto, "azienda"))

                Case "formazione"
                    If oggetto Is Nothing Then Return String.Empty
                    Dim titolo As String = CampiJson.Testo(oggetto, "titolo")
                    Dim anno As String = CampiJson.Testo(oggetto, "anno")
                    Return If(anno.Trim().Length > 0, $"{titolo} ({anno})", titolo)

                Case "altre_esperienze"
                    If oggetto Is Nothing Then Return String.Empty
                    Return Unisci(" · ", CampiJson.Testo(oggetto, "quando"),
                                         CampiJson.Testo(oggetto, "descrizione"))

                Case Else
                    Return String.Empty

            End Select

        End Function

        ''' <summary>Come si chiama una voce di questa sezione, al singolare.</summary>
        Private Function NomeDellaSezione(sezione As String) As String

            Select Case sezione
                Case "esperienze_professionali" : Return "Esperienza"
                Case "altre_esperienze" : Return "Altra esperienza"
                Case "competenze" : Return "Competenza"
                Case "formazione" : Return "Titolo di studio"
                Case Else : Return "Voce"
            End Select

        End Function

        Private Function TestoDi(voce As JsonNode) As String

            If voce Is Nothing OrElse voce.GetValueKind() <> JsonValueKind.String Then Return String.Empty
            Return voce.GetValue(Of String)()

        End Function

        Private Function Unisci(mezzo As String, ParamArray pezzi As String()) As String

            Return String.Join(mezzo, pezzi.Where(Function(p) Not String.IsNullOrWhiteSpace(p)).
                                            Select(Function(p) p.Trim()))

        End Function

    End Module

End Namespace
