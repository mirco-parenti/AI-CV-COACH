Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Dati

    ''' <summary>I recapiti dell'utente, usati così come sono per l'intestazione del CV.</summary>
    Public Class ContattiProfilo
        Public Property Email As String = ""
        Public Property Telefono As String = ""
        ''' <summary>Il domicilio: indirizzo o località di residenza.</summary>
        Public Property Citta As String = ""
        ''' <summary>Un profilo professionale o sito personale.</summary>
        Public Property Link As String = ""
    End Class

    ''' <summary>
    ''' La patente di guida. Ha un turno tutto suo perché è spesso un requisito
    ''' eliminatorio degli annunci.
    ''' </summary>
    Public Class PatenteProfilo
        ''' <summary>«sì», «no», oppure «» finché l'utente non si è pronunciato.</summary>
        Public Property Ha As String = ""
        Public Property Categorie As New List(Of String)
    End Class

    ''' <summary>Un lavoro vero e proprio, con un ruolo e un datore (tirocini e stage inclusi).</summary>
    Public Class EsperienzaFormale
        Public Property Ruolo As String = ""
        Public Property Azienda As String = ""
        Public Property Durata As String = ""
        Public Property CosaFacevo As String = ""
        ''' <summary>«tirocinio» o «stage» solo se l'utente lo dichiara apertamente; mai dedotto.</summary>
        Public Property Tipo As String = ""
    End Class

    ''' <summary>
    ''' Un'attività che non è un lavoro vero e proprio: volontariato, una mano a
    ''' familiari o amici, una passione che ha insegnato qualcosa.
    ''' </summary>
    Public Class EsperienzaInformale
        Public Property CosaFacevo As String = ""
        Public Property Quando As String = ""
        Public Property ConChi As String = ""
    End Class

    ''' <summary>Un titolo di studio o un corso.</summary>
    Public Class VoceFormazione
        Public Property Titolo As String = ""
        Public Property Istituto As String = ""
        Public Property Anno As String = ""
    End Class

    ''' <summary>
    ''' Il profilo dell'utente: la <b>fonte di verità unica</b> da cui nasce tutto il
    ''' resto (cap. 02.2). Ricalca campo per campo lo schema del prototipo — è lo stesso
    ''' che i prompt del pool dichiarano in uscita.
    ''' </summary>
    ''' <remarks>
    ''' È tipizzato e non un <c>JsonNode</c> grezzo per tre motivi: il pannello P2 lo
    ''' mostra e lo fa correggere <i>campo per campo</i> (cap. 03.6), con
    ''' <c>Option Strict On</c> il nodo grezzo costerebbe verbosità e errori che si
    ''' scoprono solo a programma acceso, e il confine tipizzato è anche una guardia
    ''' anti-invenzione: un campo che il modello si inventa non entra di straforo nel
    ''' profilo, perché qui dentro c'è posto solo per i campi dello schema.
    '''
    ''' Verso l'esterno resta però un artefatto JSON: <see cref="VersoJson"/> lo
    ''' riporta nella forma che i prompt si aspettano, con le chiavi <b>nell'ordine del
    ''' prototipo</b>. Non è un dettaglio estetico: il profilo entra nei prompt di
    ''' confronto e generazione attraverso <c>LibreriaPrompt.ComeNelPrompt</c>, e la
    ''' parità carattere per carattere con la richiesta del prototipo è ciò che il
    ''' collaudo di non-regressione misura (cap. 14, T2).
    ''' </remarks>
    Public Class Profilo

        Public Property Nome As String = ""
        Public Property Contatti As New ContattiProfilo
        Public Property Patente As New PatenteProfilo
        Public Property EsperienzeFormali As New List(Of EsperienzaFormale)
        Public Property EsperienzeInformali As New List(Of EsperienzaInformale)
        Public Property Competenze As New List(Of String)
        Public Property Formazione As New List(Of VoceFormazione)

        ''' <summary>
        ''' Come va scritto il profilo su disco (cap. 11.1: «JSON con rientri, leggibili
        ''' in qualsiasi editor»). L'encoder predefinito di .NET scriverebbe ogni lettera
        ''' accentata come sequenza di escape — «Forlì» a codici — e un file così non è
        ''' più leggibile da chi lo apre a mano.
        ''' </summary>
        Private Shared ReadOnly OpzioniFile As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        ''' <summary>
        ''' Il profilo nella forma JSON degli artefatti, con le chiavi nell'ordine del
        ''' prototipo: nome, contatti, patente, esperienze formali, esperienze
        ''' informali, competenze, formazione.
        ''' </summary>
        Public Function VersoJson() As JsonObject

            Dim formali As New JsonArray()
            For Each e As EsperienzaFormale In EsperienzeFormali
                formali.Add(New JsonObject From {
                    {"ruolo", e.Ruolo},
                    {"azienda", e.Azienda},
                    {"durata", e.Durata},
                    {"cosa_facevo", e.CosaFacevo},
                    {"tipo", e.Tipo}})
            Next

            Dim informali As New JsonArray()
            For Each e As EsperienzaInformale In EsperienzeInformali
                informali.Add(New JsonObject From {
                    {"cosa_facevo", e.CosaFacevo},
                    {"quando", e.Quando},
                    {"con_chi", e.ConChi}})
            Next

            Dim studi As New JsonArray()
            For Each v As VoceFormazione In Formazione
                studi.Add(New JsonObject From {
                    {"titolo", v.Titolo},
                    {"istituto", v.Istituto},
                    {"anno", v.Anno}})
            Next

            Dim categorie As New JsonArray()
            For Each c As String In Patente.Categorie
                categorie.Add(c)
            Next

            Dim competenze As New JsonArray()
            For Each c As String In Me.Competenze
                competenze.Add(c)
            Next

            Return New JsonObject From {
                {"nome", Nome},
                {"contatti", New JsonObject From {
                    {"email", Contatti.Email},
                    {"telefono", Contatti.Telefono},
                    {"citta", Contatti.Citta},
                    {"link", Contatti.Link}}},
                {"patente", New JsonObject From {
                    {"ha", Patente.Ha},
                    {"categorie", categorie}}},
                {"esperienze_formali", formali},
                {"esperienze_informali", informali},
                {"competenze", competenze},
                {"formazione", studi}}

        End Function

        ''' <summary>Il testo da scrivere nel file <c>profilo.json</c>.</summary>
        Public Function ComeTesto() As String
            Return VersoJson().ToJsonString(OpzioniFile)
        End Function

        ''' <summary>
        ''' Ricostruisce il profilo da un artefatto JSON. È volutamente tollerante: un
        ''' campo assente vale «vuoto» e ciò che non appartiene allo schema viene
        ''' ignorato. Così un profilo salvato da una versione precedente si riapre lo
        ''' stesso, e un frammento incompleto dell'AI non fa cadere il programma.
        ''' </summary>
        ''' <param name="nodo">L'oggetto JSON del profilo.</param>
        Public Shared Function DaJson(nodo As JsonNode) As Profilo

            Dim radice As JsonObject = TryCast(nodo, JsonObject)
            If radice Is Nothing Then
                Throw New JsonException("Il profilo deve essere un oggetto JSON.")
            End If

            Dim p As New Profilo With {.Nome = Testo(radice, "nome")}

            Dim contatti As JsonObject = Oggetto(radice, "contatti")
            If contatti IsNot Nothing Then
                p.Contatti = New ContattiProfilo With {
                    .Email = Testo(contatti, "email"),
                    .Telefono = Testo(contatti, "telefono"),
                    .Citta = Testo(contatti, "citta"),
                    .Link = Testo(contatti, "link")}
            End If

            Dim patente As JsonObject = Oggetto(radice, "patente")
            If patente IsNot Nothing Then
                p.Patente = New PatenteProfilo With {
                    .Ha = Testo(patente, "ha"),
                    .Categorie = Testi(patente, "categorie")}
            End If

            For Each e As JsonObject In Oggetti(radice, "esperienze_formali")
                p.EsperienzeFormali.Add(New EsperienzaFormale With {
                    .Ruolo = Testo(e, "ruolo"),
                    .Azienda = Testo(e, "azienda"),
                    .Durata = Testo(e, "durata"),
                    .CosaFacevo = Testo(e, "cosa_facevo"),
                    .Tipo = Testo(e, "tipo")})
            Next

            For Each e As JsonObject In Oggetti(radice, "esperienze_informali")
                p.EsperienzeInformali.Add(New EsperienzaInformale With {
                    .CosaFacevo = Testo(e, "cosa_facevo"),
                    .Quando = Testo(e, "quando"),
                    .ConChi = Testo(e, "con_chi")})
            Next

            p.Competenze = Testi(radice, "competenze")

            For Each v As JsonObject In Oggetti(radice, "formazione")
                p.Formazione.Add(New VoceFormazione With {
                    .Titolo = Testo(v, "titolo"),
                    .Istituto = Testo(v, "istituto"),
                    .Anno = Testo(v, "anno")})
            Next

            Return p

        End Function

        ''' <summary>Ricostruisce il profilo dal testo di <c>profilo.json</c>.</summary>
        Public Shared Function DaTesto(testo As String) As Profilo
            Return DaJson(JsonNode.Parse(testo))
        End Function

        ''' <summary>Legge un campo di testo; assente o non testuale vale «vuoto».</summary>
        Private Shared Function Testo(oggetto As JsonObject, chiave As String) As String
            Dim valore As JsonValue = TryCast(oggetto(chiave), JsonValue)
            Return If(valore?.ToString(), "")
        End Function

        ''' <summary>Legge una lista di testi; assente o malformata vale lista vuota.</summary>
        Private Shared Function Testi(oggetto As JsonObject, chiave As String) As List(Of String)

            Dim lista As New List(Of String)
            Dim vettore As JsonArray = TryCast(oggetto(chiave), JsonArray)
            If vettore Is Nothing Then Return lista

            For Each voce As JsonNode In vettore
                Dim valore As JsonValue = TryCast(voce, JsonValue)
                If valore IsNot Nothing Then lista.Add(valore.ToString())
            Next

            Return lista

        End Function

        ''' <summary>Legge un oggetto annidato; <c>Nothing</c> se assente o malformato.</summary>
        Private Shared Function Oggetto(radice As JsonObject, chiave As String) As JsonObject
            Return TryCast(radice(chiave), JsonObject)
        End Function

        ''' <summary>Scorre le voci-oggetto di una lista, saltando quelle malformate.</summary>
        Private Shared Iterator Function Oggetti(radice As JsonObject, chiave As String) As IEnumerable(Of JsonObject)

            Dim vettore As JsonArray = TryCast(radice(chiave), JsonArray)
            If vettore Is Nothing Then Return

            For Each voce As JsonNode In vettore
                Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                If oggetto IsNot Nothing Then Yield oggetto
            Next

        End Function

    End Class

End Namespace
