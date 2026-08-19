Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Dati

    ''' <summary>Di che genere è un documento della cartella dell'utente (cap. 05.2).</summary>
    Public Enum CategoriaDocumento

        ''' <summary>Buste paga, contratti, appunti: la maggior parte di una cartella vera.</summary>
        Altro = 0

        ''' <summary>Un curriculum della persona.</summary>
        Cv = 1

        ''' <summary>Un certificato, un attestato, un patentino: si allega a una candidatura.</summary>
        Attestato = 2

        ''' <summary>Una lettera di presentazione già scritta.</summary>
        Lettera = 3

    End Enum

    ''' <summary>
    ''' Un documento della cartella dell'utente con la categoria che gli è stata
    ''' riconosciuta, e il perché (cap. 05.2).
    ''' </summary>
    ''' <remarks>
    ''' Il <see cref="Nome"/> è <b>relativo</b> alla cartella — «attestati\HACCP.pdf» —
    ''' e non assoluto: la cartella si può spostare, e un elenco di percorsi assoluti
    ''' smetterebbe di valere quel giorno lì. È la stessa scelta degli allegati della
    ''' bozza (v. <c>AllegatoScelto</c>).
    ''' </remarks>
    Public Class DocumentoClassificato

        Public Property Nome As String
        Public Property Categoria As CategoriaDocumento = CategoriaDocumento.Altro

        ''' <summary>Perché è finito in quella categoria: una riga, per correggere a colpo d'occhio.</summary>
        Public Property Motivo As String = ""

        ''' <summary>
        ''' Se la categoria l'ha decisa l'utente. Serve a non cancellargli il lavoro: una
        ''' rilettura della cartella riclassifica quel che l'AI aveva detto, non quel che
        ''' ha corretto una persona (cap. 05.2, «niente entra senza conferma»).
        ''' </summary>
        Public Property Corretto As Boolean

        Public Function ComeJson() As JsonObject

            Return New JsonObject From {
                {"nome", Nome},
                {"categoria", NomeDi(Categoria)},
                {"motivo", Motivo},
                {"corretto", Corretto}}

        End Function

        ''' <summary>Rilegge un documento; <c>Nothing</c> se non dice nemmeno come si chiama.</summary>
        Public Shared Function DaJson(scritto As JsonObject) As DocumentoClassificato

            If scritto Is Nothing Then Return Nothing

            Dim nome As String = CampiJson.Testo(scritto, "nome")
            If String.IsNullOrWhiteSpace(nome) Then Return Nothing

            Return New DocumentoClassificato With {
                .Nome = nome.Trim(),
                .Categoria = CategoriaDa(CampiJson.Testo(scritto, "categoria")),
                .Motivo = If(CampiJson.Testo(scritto, "motivo"), ""),
                .Corretto = CampiJson.Vero(scritto, "corretto")}

        End Function

        ''' <summary>La categoria come si scrive nel JSON e nel prompt.</summary>
        Public Shared Function NomeDi(categoria As CategoriaDocumento) As String

            Select Case categoria
                Case CategoriaDocumento.Cv : Return "cv"
                Case CategoriaDocumento.Attestato : Return "attestato"
                Case CategoriaDocumento.Lettera : Return "lettera"
                Case Else : Return "altro"
            End Select

        End Function

        ''' <summary>
        ''' La categoria da come è scritta. Tutto ciò che non si riconosce è
        ''' <see cref="CategoriaDocumento.Altro"/>: è la categoria che non promette
        ''' niente, e quindi l'unica sicura da attribuire per sbaglio.
        ''' </summary>
        Public Shared Function CategoriaDa(scritta As String) As CategoriaDocumento

            Select Case If(scritta, "").Trim().ToLowerInvariant()
                Case "cv" : Return CategoriaDocumento.Cv
                Case "attestato" : Return CategoriaDocumento.Attestato
                Case "lettera" : Return CategoriaDocumento.Lettera
                Case Else : Return CategoriaDocumento.Altro
            End Select

        End Function

        ''' <summary>La categoria come si legge a video (cap. 03.2: l'interfaccia è in italiano).</summary>
        Public Shared Function ComeSiLegge(categoria As CategoriaDocumento) As String

            Select Case categoria
                Case CategoriaDocumento.Cv : Return "CV"
                Case CategoriaDocumento.Attestato : Return "attestato"
                Case CategoriaDocumento.Lettera : Return "lettera"
                Case Else : Return "altro"
            End Select

        End Function

    End Class

    ''' <summary>
    ''' La cartella documenti dell'utente e quel che ci si è riconosciuto dentro
    ''' (cap. 05.2): vive in <c>documenti.json</c>, nella cartella dati.
    ''' </summary>
    ''' <remarks>
    ''' <para>Della cartella si tiene il <b>percorso assoluto</b>, ed è l'unico posto del
    ''' programma in cui succede: quella cartella è dell'utente e sta dove vuole lui —
    ''' spesso fuori da <c>%APPDATA%</c>, magari su un altro disco. Se un giorno non c'è
    ''' più, non è un guasto: si dice e si chiede di sceglierne un'altra.</para>
    ''' <para>Qui non c'è nessun file <b>copiato</b>: la raccolta è un elenco di nomi con
    ''' una categoria. Gli attestati si allegano leggendoli da dove sono, e cancellarne
    ''' uno dalla sua cartella lo toglie dagli allegati proposti — che è quel che l'utente
    ''' si aspetta di aver fatto.</para>
    ''' </remarks>
    Public Class RaccoltaDocumenti

        ''' <summary>Rientri e accenti in chiaro: il file è fatto per essere letto a mano.</summary>
        Private Shared ReadOnly FormatoLeggibile As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        ''' <summary>La cartella scelta dall'utente; <c>Nothing</c> finché non ne sceglie una.</summary>
        Public Property Cartella As String

        ''' <summary>Quando la cartella è stata letta l'ultima volta.</summary>
        Public Property Letta As Date

        ''' <summary>Quale dei CV sembra il più aggiornato (cap. 05.2, passo 3).</summary>
        Public Property CvPiuRecente As String = ""

        ''' <summary>I documenti riconosciuti, uno per file.</summary>
        Public ReadOnly Property Documenti As New List(Of DocumentoClassificato)

        ''' <summary>Se una cartella è stata scelta e c'è ancora.</summary>
        Public ReadOnly Property CartellaUtilizzabile As Boolean
            Get
                Return Not String.IsNullOrWhiteSpace(Cartella) AndAlso Directory.Exists(Cartella)
            End Get
        End Property

        ''' <summary>Quel che si sa di un file; <c>Nothing</c> se non è mai stato classificato.</summary>
        Public Function Riconosciuto(nome As String) As DocumentoClassificato

            If String.IsNullOrWhiteSpace(nome) Then Return Nothing

            Return Documenti.FirstOrDefault(
                Function(d) String.Equals(d.Nome, nome, StringComparison.OrdinalIgnoreCase))

        End Function

        ''' <summary>
        ''' Il percorso del CV che la classificazione ha indicato come il più recente
        ''' (cap. 05.2, passo 3), se c'è ed è ancora al suo posto; <c>Nothing</c> altrimenti.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>È la porta «qui c'è tutto» del profilo</b>, l'altra metà del mestiere di
        ''' questa cartella <i>(chiusa il 2026-08-19)</i>: il CV più recente questa raccolta
        ''' lo indicava da T6, ma non lo leggeva nessuno e chi importava un CV doveva
        ''' ritrovarselo a mano fra i propri file.</para>
        ''' <para><b>Che il file esista si guarda adesso</b>, non quando fu classificato: qui
        ''' non c'è nessun file copiato, e fra la scansione e oggi quel CV può essere stato
        ''' spostato o cancellato. Proporre di importare un file che non c'è più sarebbe
        ''' peggio che non proporre niente.</para>
        ''' </remarks>
        Public Function PercorsoDelCvPiuRecente() As String

            If Not CartellaUtilizzabile Then Return Nothing
            If String.IsNullOrWhiteSpace(CvPiuRecente) Then Return Nothing
            If Riconosciuto(CvPiuRecente) Is Nothing Then Return Nothing

            Dim percorso As String = Path.Combine(Cartella, CvPiuRecente)
            Return If(File.Exists(percorso), percorso, Nothing)

        End Function

        ''' <summary>
        ''' Gli attestati: quelli che si propongono come allegati di una candidatura
        ''' (cap. 07.1). In ordine di nome, come si vedono nella cartella.
        ''' </summary>
        Public Function Attestati() As List(Of DocumentoClassificato)

            Return Documenti.
                Where(Function(d) d.Categoria = CategoriaDocumento.Attestato).
                OrderBy(Function(d) d.Nome, StringComparer.CurrentCultureIgnoreCase).
                ToList()

        End Function

        ''' <summary>
        ''' Rimette l'elenco in pari coi file che ci sono <b>adesso</b> nella cartella:
        ''' quelli spariti escono, quelli nuovi entrano come «altro», in attesa di essere
        ''' riconosciuti.
        ''' </summary>
        ''' <remarks>
        ''' È la stessa regola degli allegati della bozza (cap. 07.1): delle categorie ci
        ''' si fida, dei file no. Un documento cancellato dalla cartella non deve tornare
        ''' in vita perché un elenco lo nominava, e uno appena messo lì non deve restare
        ''' invisibile perché nessuno l'ha ancora classificato.
        ''' </remarks>
        ''' <returns>Quanti file non erano mai stati visti prima.</returns>
        Public Function AllineaAiFile(trovati As IEnumerable(Of FileTrovato)) As Integer

            Dim mai As Integer = 0
            Dim aggiornati As New List(Of DocumentoClassificato)

            For Each file_ As FileTrovato In If(trovati, Enumerable.Empty(Of FileTrovato)())

                If file_ Is Nothing OrElse String.IsNullOrWhiteSpace(file_.Nome) Then Continue For

                Dim gia As DocumentoClassificato = Riconosciuto(file_.Nome)

                If gia Is Nothing Then
                    mai += 1
                    gia = New DocumentoClassificato With {
                        .Nome = file_.Nome,
                        .Categoria = CategoriaDocumento.Altro,
                        .Motivo = "Mai letto: sta nella cartella ma non è ancora stato riconosciuto."}
                End If

                aggiornati.Add(gia)

            Next

            Documenti.Clear()
            Documenti.AddRange(aggiornati)

            If Riconosciuto(CvPiuRecente) Is Nothing Then CvPiuRecente = ""

            Return mai

        End Function

        ''' <summary>
        ''' Prende la proposta del prompt <c>classifica_documenti</c> e la applica ai file
        ''' trovati adesso.
        ''' </summary>
        ''' <remarks>
        ''' <para>Quel che l'utente ha <b>corretto a mano</b> non si tocca: rileggere la
        ''' cartella serve a riconoscere i file nuovi, non a rimettere in discussione una
        ''' decisione che una persona ha già preso (cap. 05.2).</para>
        ''' <para>Un nome che l'AI si è inventato — o che nel frattempo è sparito — viene
        ''' scartato: si classifica quel che c'è nella cartella, non quel che compare in
        ''' una risposta.</para>
        ''' </remarks>
        Public Sub PrendiLaProposta(proposta As JsonNode, trovati As IEnumerable(Of FileTrovato))

            AllineaAiFile(trovati)

            Dim oggetto As JsonObject = TryCast(proposta, JsonObject)
            If oggetto Is Nothing Then Return

            Dim elenco As JsonArray = TryCast(oggetto("documenti"), JsonArray)
            If elenco IsNot Nothing Then
                For Each voce As JsonNode In elenco

                    Dim detto As DocumentoClassificato = DocumentoClassificato.DaJson(TryCast(voce, JsonObject))
                    If detto Is Nothing Then Continue For

                    Dim nostro As DocumentoClassificato = Riconosciuto(detto.Nome)
                    If nostro Is Nothing OrElse nostro.Corretto Then Continue For

                    nostro.Categoria = detto.Categoria
                    nostro.Motivo = detto.Motivo

                Next
            End If

            Dim piuRecente As String = CampiJson.Testo(oggetto, "cv_piu_recente")
            If Not String.IsNullOrWhiteSpace(piuRecente) AndAlso Riconosciuto(piuRecente) IsNot Nothing Then
                CvPiuRecente = piuRecente.Trim()
            End If

        End Sub

        Public Function ComeJson() As JsonObject

            Dim elenco As New JsonArray()
            For Each documento As DocumentoClassificato In Documenti
                elenco.Add(documento.ComeJson())
            Next

            Return New JsonObject From {
                {"cartella", Cartella},
                {"letta", CampiJson.Quando(Letta)},
                {"cv_piu_recente", CvPiuRecente},
                {"documenti", elenco}}

        End Function

        ''' <summary>Rilegge la raccolta da un JSON già letto.</summary>
        Public Shared Function DaJson(scritta As JsonNode) As RaccoltaDocumenti

            Dim oggetto As JsonObject = TryCast(scritta, JsonObject)
            If oggetto Is Nothing Then Return Nothing

            Dim raccolta As New RaccoltaDocumenti With {
                .Cartella = CampiJson.Testo(oggetto, "cartella"),
                .Letta = CampiJson.Istante(oggetto, "letta"),
                .CvPiuRecente = If(CampiJson.Testo(oggetto, "cv_piu_recente"), "")}

            Dim elenco As JsonArray = TryCast(oggetto("documenti"), JsonArray)
            If elenco IsNot Nothing Then
                For Each voce As JsonNode In elenco
                    Dim documento As DocumentoClassificato =
                        DocumentoClassificato.DaJson(TryCast(voce, JsonObject))

                    If documento IsNot Nothing Then raccolta.Documenti.Add(documento)
                Next
            End If

            Return raccolta

        End Function

        ''' <summary>
        ''' La raccolta scritta dal file, o una vuota. Non solleva mai: senza cartella
        ''' documenti l'applicazione funziona uguale — è una comodità, non un ingranaggio.
        ''' </summary>
        Public Shared Function Carica(percorso As String) As RaccoltaDocumenti

            If String.IsNullOrWhiteSpace(percorso) OrElse Not File.Exists(percorso) Then
                Return New RaccoltaDocumenti()
            End If

            Try
                Return If(DaJson(JsonNode.Parse(File.ReadAllText(percorso))), New RaccoltaDocumenti())
            Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                Return New RaccoltaDocumenti()
            End Try

        End Function

        ''' <summary>La raccolta come si scrive su disco, con i rientri.</summary>
        Public Function ComeTesto() As String
            Return ComeJson().ToJsonString(FormatoLeggibile)
        End Function

    End Class

End Namespace
