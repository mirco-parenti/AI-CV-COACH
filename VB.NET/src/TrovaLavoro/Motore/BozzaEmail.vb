Imports System.IO
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Dati

Namespace Motore

    ''' <summary>Da dove viene un allegato, e quindi dove lo si ritrova.</summary>
    Public Enum OrigineAllegato

        ''' <summary>Un documento generato per questa candidatura: sta nella sua <c>out\</c>.</summary>
        Candidatura

        ''' <summary>Un file della cartella documenti dell'utente (cap. 05.2).</summary>
        Documenti

    End Enum

    ''' <summary>
    ''' Un file che parte con l'email: il nome, da dove viene e se è spuntato.
    ''' </summary>
    ''' <remarks>
    ''' Si salva il <b>nome</b> e non il percorso, per la stessa ragione per cui il registro
    ''' salva il nome della cartella e non il suo percorso (cap. 11.1): la cartella dati si
    ''' può spostare e si esporta nei backup, e un elenco pieno di percorsi assoluti
    ''' smetterebbe di valere il giorno in cui quei percorsi cambiano. Dove cercarlo lo dice
    ''' l'<see cref="Origine"/>.
    ''' </remarks>
    Public Class AllegatoScelto

        Public Property Nome As String
        Public Property Origine As OrigineAllegato = OrigineAllegato.Candidatura

        ''' <summary>Se l'utente lo vuole davvero: la spunta dell'elenco (cap. 07.1).</summary>
        Public Property Scelto As Boolean = True

        Public Function ComeJson() As JsonObject

            Return New JsonObject From {
                {"nome", Nome},
                {"da", If(Origine = OrigineAllegato.Documenti, "documenti", "candidatura")},
                {"scelto", Scelto}}

        End Function

        ''' <summary>Rilegge un allegato; <c>Nothing</c> se non dice nemmeno come si chiama.</summary>
        Public Shared Function DaJson(scritto As JsonObject) As AllegatoScelto

            If scritto Is Nothing Then Return Nothing

            Dim nome As String = CampiJson.Testo(scritto, "nome")
            If String.IsNullOrWhiteSpace(nome) Then Return Nothing

            Return New AllegatoScelto With {
                .Nome = nome,
                .Origine = If(CampiJson.Testo(scritto, "da") = "documenti",
                              OrigineAllegato.Documenti, OrigineAllegato.Candidatura),
                .Scelto = CampiJson.Vero(scritto, "scelto", quandoManca:=True)}

        End Function

    End Class

    ''' <summary>
    ''' La bozza dell'email di candidatura (cap. 07.1): quel che l'utente vede in P7 e
    ''' modifica prima di preparare il messaggio. Vive in <c>email.json</c>, dentro la
    ''' cartella dell'opportunità.
    ''' </summary>
    ''' <remarks>
    ''' <para>È l'unica parte di una candidatura in cui l'AI propone e l'utente <b>scrive
    ''' davvero</b>: il destinatario lo digita lui (il programma non lo inventa mai), gli
    ''' allegati li spunta lui, e oggetto e corpo li può riscrivere. Per questo si salva —
    ''' senza, riaprire la candidatura domani vorrebbe dire rifare tutto.</para>
    ''' <para>Che la bozza esista non vuol dire che sia partita: lo stato «inviata» lo
    ''' dichiara l'utente, e sta nello <c>stato.json</c> (cap. 07.3).</para>
    ''' </remarks>
    Public Class BozzaEmail

        ''' <summary>L'indirizzo dell'azienda; vuoto finché l'utente non lo scrive.</summary>
        Public Property Destinatario As String = ""

        Public Property Oggetto As String = ""
        Public Property Corpo As String = ""

        ''' <summary>
        ''' La lingua in cui l'AI ha scritto questo messaggio (cap. 10.1); vuota per le
        ''' bozze salvate prima che la si annotasse.
        ''' </summary>
        ''' <remarks>
        ''' Si conserva perché la lingua della candidatura può cambiare <b>dopo</b>: chi
        ''' rigenera i documenti in inglese dalla tendina di P6 si ritroverebbe l'email
        ''' italiana di ieri, ripresa in silenzio come se fosse quella giusta. Senza questo
        ''' campo non c'è modo di accorgersene, perché una bozza è solo testo e il testo non
        ''' dichiara in che lingua è.
        ''' </remarks>
        Public Property Lingua As String = ""

        ''' <summary>I file candidati a partire, spuntati o no.</summary>
        Public ReadOnly Property Allegati As New List(Of AllegatoScelto)

        ''' <summary>Solo quelli spuntati, nell'ordine in cui si vedono.</summary>
        Public Function AllegatiScelti() As List(Of AllegatoScelto)

            Dim scelti As New List(Of AllegatoScelto)
            For Each allegato As AllegatoScelto In Allegati
                If allegato.Scelto Then scelti.Add(allegato)
            Next

            Return scelti

        End Function

        ''' <summary>
        ''' Il percorso di un allegato, ricomposto adesso da chi le cartelle le conosce.
        ''' </summary>
        ''' <param name="allegato">L'allegato da ritrovare.</param>
        ''' <param name="cartellaCandidatura">La cartella dell'opportunità (non la sua <c>out\</c>).</param>
        ''' <param name="cartellaDocumenti">La cartella documenti dell'utente; può mancare.</param>
        Public Shared Function PercorsoDi(allegato As AllegatoScelto,
                                          cartellaCandidatura As String,
                                          Optional cartellaDocumenti As String = Nothing) As String

            If allegato Is Nothing OrElse String.IsNullOrWhiteSpace(allegato.Nome) Then Return Nothing

            If allegato.Origine = OrigineAllegato.Documenti Then
                If String.IsNullOrWhiteSpace(cartellaDocumenti) Then Return Nothing
                Return Path.Combine(cartellaDocumenti, allegato.Nome)
            End If

            If String.IsNullOrWhiteSpace(cartellaCandidatura) Then Return Nothing
            Return Path.Combine(cartellaCandidatura, ArchivioOpportunita.NomeCartellaOut, allegato.Nome)

        End Function

        Public Function ComeJson() As JsonObject

            Dim elenco As New JsonArray()
            For Each allegato As AllegatoScelto In Allegati
                elenco.Add(allegato.ComeJson())
            Next

            Return New JsonObject From {
                {"destinatario", Destinatario},
                {"oggetto", Oggetto},
                {"corpo", Corpo},
                {"lingua", Lingua},
                {"allegati", elenco}}

        End Function

        ''' <summary>
        ''' Rilegge la bozza da <c>email.json</c>. Un file che non si lascia leggere non
        ''' fa cadere niente: si riparte da una bozza vuota, e l'AI la riscrive.
        ''' </summary>
        Public Shared Function DaJson(scritta As JsonNode) As BozzaEmail

            Dim oggetto As JsonObject = TryCast(scritta, JsonObject)
            If oggetto Is Nothing Then Return Nothing

            Dim bozza As New BozzaEmail With {
                .Destinatario = If(CampiJson.Testo(oggetto, "destinatario"), ""),
                .Oggetto = If(CampiJson.Testo(oggetto, "oggetto"), ""),
                .Corpo = If(CampiJson.Testo(oggetto, "corpo"), ""),
                .Lingua = If(CampiJson.Testo(oggetto, "lingua"), "")}

            Dim elenco As JsonArray = TryCast(oggetto("allegati"), JsonArray)
            If elenco IsNot Nothing Then
                For Each voce As JsonNode In elenco
                    Dim allegato As AllegatoScelto = AllegatoScelto.DaJson(TryCast(voce, JsonObject))
                    If allegato IsNot Nothing Then bozza.Allegati.Add(allegato)
                Next
            End If

            Return bozza

        End Function

        ''' <summary>
        ''' La bozza che l'AI propone, letta dalla risposta del prompt
        ''' <c>email_candidatura</c>. Gli allegati non vengono da lì: li sa il programma,
        ''' che ha appena scritto i documenti.
        ''' </summary>
        Public Shared Function DallaProposta(proposta As JsonNode) As BozzaEmail

            Dim oggetto As JsonObject = TryCast(proposta, JsonObject)
            If oggetto Is Nothing Then Return New BozzaEmail()

            Return New BozzaEmail With {
                .Oggetto = If(CampiJson.Testo(oggetto, "oggetto"), ""),
                .Corpo = ConGliACapoDiWindows(CampiJson.Testo(oggetto, "corpo"))}

        End Function

        ''' <summary>
        ''' Gli a capo come li vuole una casella di Windows. L'AI scrive <c>\n</c>, e una
        ''' <c>TextBox</c> multiriga i ritorni a capo li mostra solo se sono CRLF: senza
        ''' questo, il messaggio compare tutto attaccato — «Cordiali saluti,Mirco Parenti» —
        ''' e chi lo rilegge crede che sia stato scritto così.
        ''' </summary>
        ''' <remarks>
        ''' Si normalizza <b>qui</b>, alla porta da cui il testo dell'AI entra: è il punto in
        ''' cui vale una volta sola per tutti — quel che si vede, quel che si salva e quel che
        ''' parte. (Il messaggio di posta rinormalizza comunque a CRLF per conto suo,
        ''' v. <c>ScrittoreEml</c>: là è il formato a pretenderlo, qui è chi legge.)
        ''' </remarks>
        Private Shared Function ConGliACapoDiWindows(testo As String) As String
            Return If(testo, "").Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Replace(vbLf, vbCrLf)
        End Function

    End Class

End Namespace
