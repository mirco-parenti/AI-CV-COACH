Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Dati

    ''' <summary>Da dove arriva la tabella delle ricerche in uso.</summary>
    Public Enum OrigineRicerche
        ''' <summary>I portali che il programma porta dentro di sé (cap. 06.3).</summary>
        Predefinita
        ''' <summary>Il file <c>ricerche.json</c> della cartella dati.</summary>
        File
    End Enum

    ''' <summary>
    ''' Un portale su cui si cerca lavoro: un nome e l'<b>indirizzo parametrizzato</b> della
    ''' sua pagina di risultati (cap. 06.3). Due segnaposto soli — <c>{cosa}</c> e
    ''' <c>{dove}</c> — perché è tutto quello che serve a comporre una ricerca, e ogni
    ''' segnaposto in più sarebbe una riga di codice in più per chi aggiunge un portale.
    ''' </summary>
    Public Class Portale

        Public Property Nome As String

        ''' <summary>L'indirizzo con i segnaposto, es. <c>https://…/jobs?q={cosa}&amp;l={dove}</c>.</summary>
        Public Property Schema As String

        ''' <summary>
        ''' L'indirizzo da aprire nel browser, coi segnaposto sostituiti. I valori vengono
        ''' codificati per l'indirizzo: uno spazio o una lettera accentata nel nome di un
        ''' ruolo non deve rompere la query.
        ''' </summary>
        Public Function ComponiUrl(cosa As String, dove As String) As String

            Return If(Schema, String.Empty).
                Replace("{cosa}", Uri.EscapeDataString(If(cosa, String.Empty).Trim())).
                Replace("{dove}", Uri.EscapeDataString(If(dove, String.Empty).Trim()))

        End Function

    End Class

    ''' <summary>
    ''' Una ricerca che l'utente ha messo da parte: il portale e i <b>valori</b>, non
    ''' l'indirizzo già composto. Così cambiare zona o parole chiave non chiede di
    ''' riscrivere un indirizzo a mano, e il giorno che un portale cambia la forma dei suoi
    ''' link basta correggere il suo schema perché tornino a funzionare tutte.
    ''' </summary>
    Public Class RicercaSalvata

        ''' <summary>Come la chiama l'utente: è ciò che compare nel menù di P3.</summary>
        Public Property Nome As String

        ''' <summary>Il nome del portale su cui gira.</summary>
        Public Property Portale As String

        ''' <summary>Cosa si cerca: ruolo, parole chiave.</summary>
        Public Property Cosa As String

        ''' <summary>Dove: città, provincia, zona.</summary>
        Public Property Dove As String

    End Class

    ''' <summary>
    ''' La tabella dei portali e le ricerche salvate dell'utente: il contenuto di
    ''' <c>ricerche.json</c> (cap. 06.3, cap. 11.1). I portali sono <b>dati, non codice</b> —
    ''' aggiungerne uno è una riga nel file, non una nuova build.
    ''' </summary>
    ''' <remarks>
    ''' <para>Vale la regola di <c>taratura.json</c> e <c>modelli.json</c> (cap. 11.6): se il
    ''' file manca, è illeggibile o è parziale, si usano i valori che il programma porta
    ''' dentro di sé e lo si <b>annota</b>. Un file rotto non impedisce di cercare lavoro.</para>
    ''' <para><b>Solo <c>http</c> e <c>https</c>.</b> Uno schema con un altro protocollo
    ''' viene scartato e detto: <c>ricerche.json</c> è un file di testo che chiunque può
    ''' modificare, e finisce dritto nella barra di un browser vero — un
    ''' <c>file://</c> o un <c>javascript:</c> lì dentro non è una configurazione, è una
    ''' sorpresa. È la stessa cautela con cui la taratura rifiuta i numeri fuori
    ''' intervallo, applicata all'unico valore di questo file che non è un numero.</para>
    ''' <para>Le ricerche salvate <b>non</b> si validano allo stesso modo: sono parole
    ''' dell'utente, e l'unica cosa che si pretende è che dicano su quale portale girano.</para>
    ''' </remarks>
    Public Class Ricerche

        ''' <summary>I portali del primo rilascio (cap. 06.3; cap. 15, voce 7).</summary>
        ''' <remarks>
        ''' <para><b>Verificati sul campo il 2026-08-12</b>, come il cap. 14 chiede a T5:
        ''' aperti uno per uno nel browser integrato, perché è l'unico modo di sapere se un
        ''' portale ha cambiato la forma dei suoi link — o se esiste ancora.</para>
        ''' <para>Ed era il caso: <b>InfoJobs non esiste più</b>. Al suo indirizzo risponde
        ''' un avviso che dichiara la piattaforma «ufficialmente chiusa e non più
        ''' disponibile», e al suo posto è entrato <b>Jooble</b>, un aggregatore che
        ''' raccoglie anche gli annunci delle agenzie per il lavoro — dove sta gran parte
        ''' delle offerte per i ruoli operativi che il nostro utente cerca (cap. 15,
        ''' voce 7, rivista con Mirco).</para>
        ''' <para>Jooble ha anche confermato sul campo la ragione del browser integrato
        ''' (cap. 06.1): lo stesso indirizzo, chiesto da un programma qualunque, torna
        ''' <c>403</c> con la sfida di Cloudflare; chiesto dalla WebView, apre i risultati.
        ''' </para>
        ''' </remarks>
        Public Shared Function Predefinita() As Ricerche

            Return New Ricerche With {
                .Portali = New List(Of Portale) From {
                    New Portale With {.Nome = "Indeed",
                                      .Schema = "https://it.indeed.com/jobs?q={cosa}&l={dove}"},
                    New Portale With {.Nome = "Jooble",
                                      .Schema = "https://it.jooble.org/SearchResult?ukw={cosa}&rgns={dove}"},
                    New Portale With {.Nome = "Subito.it",
                                      .Schema = "https://www.subito.it/annunci-italia/vendita/offerte-lavoro/?q={cosa}+{dove}"},
                    New Portale With {.Nome = "Cerca «lavora con noi»",
                                      .Schema = "https://www.bing.com/search?q={cosa}+{dove}+lavora+con+noi"}},
                .Salvate = New List(Of RicercaSalvata),
                .Origine = OrigineRicerche.Predefinita}

        End Function

        ''' <summary>I portali su cui si può cercare.</summary>
        Public Property Portali As List(Of Portale)

        ''' <summary>Le ricerche messe da parte dall'utente, nell'ordine in cui le ha salvate.</summary>
        Public Property Salvate As List(Of RicercaSalvata)

        ''' <summary>Da dove viene ciò che si sta usando.</summary>
        Public Property Origine As OrigineRicerche = OrigineRicerche.Predefinita

        ''' <summary>
        ''' Motivo del ripiego o delle voci scartate, da annotare (cap. 11.6);
        ''' <c>Nothing</c> se non c'è niente da dire.
        ''' </summary>
        Public Property Avviso As String

        ''' <summary>Il portale con quel nome, o <c>Nothing</c>. Il nome non distingue le maiuscole.</summary>
        Public Function TrovaPortale(nome As String) As Portale

            If String.IsNullOrWhiteSpace(nome) Then Return Nothing

            Return Portali.FirstOrDefault(
                Function(p) String.Equals(p.Nome, nome.Trim(), StringComparison.OrdinalIgnoreCase))

        End Function

        ''' <summary>
        ''' Da dove viene un indirizzo: il <b>nome del portale</b> se è uno dei nostri,
        ''' altrimenti il <b>sito</b> nudo e crudo (cap. 06.4). Vuoto se non è un indirizzo.
        ''' </summary>
        ''' <remarks>
        ''' Serve alla cattura, che deve scrivere una provenienza leggibile accanto
        ''' all'annuncio. Il confronto è sul sito e non sull'indirizzo intero, perché la
        ''' pagina di <i>un</i> annuncio non somiglia allo schema di ricerca del suo
        ''' portale: <c>it.indeed.com/jobs?q=…</c> cerca, <c>it.indeed.com/viewjob?jk=…</c>
        ''' è l'annuncio, e sono lo stesso portale. Il <c>www.</c> non conta: chi scrive un
        ''' portale in <c>ricerche.json</c> non deve indovinare quale forma userà il sito.
        ''' </remarks>
        Public Function FonteDi(indirizzo As String) As String

            Dim sito As String = SitoDi(indirizzo)
            If sito = "" Then Return String.Empty

            Dim suo As Portale = Portali.FirstOrDefault(
                Function(p) String.Equals(SitoDi(p.Schema), sito, StringComparison.OrdinalIgnoreCase))

            Return If(suo IsNot Nothing, suo.Nome, sito)

        End Function

        ''' <summary>Il sito di un indirizzo, senza <c>www.</c>; vuoto se non è un indirizzo.</summary>
        Private Shared Function SitoDi(indirizzo As String) As String

            Dim letto As Uri = Nothing
            If String.IsNullOrWhiteSpace(indirizzo) OrElse
               Not Uri.TryCreate(indirizzo.Trim(), UriKind.Absolute, letto) Then Return String.Empty

            Dim sito As String = letto.Host
            If sito.StartsWith("www.", StringComparison.OrdinalIgnoreCase) Then sito = sito.Substring(4)

            Return sito

        End Function

        ''' <summary>
        ''' Mette da parte una ricerca. Se una con lo stesso nome c'è già la <b>sostituisce</b>
        ''' al suo posto nell'elenco, invece di aggiungerne una gemella: due voci uguali nel
        ''' menù di P3 non aiuterebbero nessuno a scegliere.
        ''' </summary>
        Public Sub MettiDaParte(ricerca As RicercaSalvata)

            If ricerca Is Nothing Then Throw New ArgumentNullException(NameOf(ricerca))
            If String.IsNullOrWhiteSpace(ricerca.Nome) Then
                Throw New ArgumentException("Una ricerca salvata vuole un nome.", NameOf(ricerca))
            End If
            If TrovaPortale(ricerca.Portale) Is Nothing Then
                Throw New ArgumentException(
                    $"Il portale «{ricerca.Portale}» non è fra quelli conosciuti.", NameOf(ricerca))
            End If

            ' Ripulita prima di entrare: i confronti guardano il nome senza spazi ai bordi,
            ' e tenere quello grezzo darebbe due voci «diverse» che si leggono uguali —
            ' più un nome che cambia da solo passando dal file, dove viene ripulito.
            Dim pulita As New RicercaSalvata With {
                .Nome = ricerca.Nome.Trim(),
                .Portale = TrovaPortale(ricerca.Portale).Nome,
                .Cosa = If(ricerca.Cosa, String.Empty).Trim(),
                .Dove = If(ricerca.Dove, String.Empty).Trim()}

            Dim gemella As Integer = Salvate.FindIndex(
                Function(s) String.Equals(s.Nome, pulita.Nome, StringComparison.OrdinalIgnoreCase))

            If gemella >= 0 Then
                Salvate(gemella) = pulita
            Else
                Salvate.Add(pulita)
            End If

        End Sub

        ''' <summary>Toglie la ricerca con quel nome; dice se c'era.</summary>
        Public Function Dimentica(nome As String) As Boolean

            Dim quante As Integer = Salvate.RemoveAll(
                Function(s) String.Equals(s.Nome, If(nome, String.Empty).Trim(),
                                          StringComparison.OrdinalIgnoreCase))

            Return quante > 0

        End Function

        ''' <summary>
        ''' Carica dal file indicato; se manca o non si legge, i valori predefiniti con
        ''' l'avviso del ripiego (cap. 11.6).
        ''' </summary>
        Public Shared Function Carica(percorso As String) As Ricerche

            If String.IsNullOrWhiteSpace(percorso) Then
                Throw New ArgumentException("Manca il percorso del file.", NameOf(percorso))
            End If

            If Not IO.File.Exists(percorso) Then
                Dim r As Ricerche = Predefinita()
                r.Avviso = $"Ricerche non trovate in «{percorso}»: uso i portali predefiniti."
                Return r
            End If

            Try
                Return DaJson(IO.File.ReadAllText(percorso, Text.Encoding.UTF8))
            Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                       OrElse TypeOf ex Is UnauthorizedAccessException
                Dim r As Ricerche = Predefinita()
                r.Avviso = $"Ricerche illeggibili in «{percorso}» ({ex.Message}): uso i portali predefiniti."
                Return r
            End Try

        End Function

        ''' <summary>
        ''' Costruisce la tabella da un testo JSON. Una sezione assente ricade sul
        ''' predefinito, così un file parziale resta utilizzabile.
        ''' </summary>
        Public Shared Function DaJson(testo As String) As Ricerche

            Dim radice As JsonObject = TryCast(JsonNode.Parse(testo), JsonObject)
            If radice Is Nothing Then
                Throw New JsonException("Le ricerche devono essere un oggetto JSON.")
            End If

            Dim r As Ricerche = Predefinita()
            r.Origine = OrigineRicerche.File

            Dim scartati As New List(Of String)

            LeggiPortali(radice, r, scartati)
            LeggiSalvate(radice, r, scartati)

            If scartati.Count > 0 Then
                r.Avviso = "Ricerche: " & String.Join("; ", scartati) & "."
            End If

            Return r

        End Function

        ''' <summary>
        ''' I portali dichiarati nel file. Se il file ne dichiara almeno uno valido, quelli
        ''' <b>sostituiscono</b> i predefiniti: chi scrive la propria tabella vuole la
        ''' propria, non la propria più la nostra.
        ''' </summary>
        Private Shared Sub LeggiPortali(radice As JsonObject, dentro As Ricerche,
                                        scartati As List(Of String))

            Dim elenco As JsonArray = TryCast(radice("portali"), JsonArray)
            If elenco Is Nothing Then Return

            Dim portali As New List(Of Portale)

            For Each voce As JsonNode In elenco

                Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                Dim nome As String = Testo(oggetto, "nome")
                Dim schema As String = Testo(oggetto, "schema")

                If String.IsNullOrWhiteSpace(nome) OrElse String.IsNullOrWhiteSpace(schema) Then
                    scartati.Add("un portale senza nome o senza schema è stato scartato")
                    Continue For
                End If

                If Not IndirizzoDelWeb(schema) Then
                    scartati.Add($"lo schema del portale «{nome}» non è un indirizzo http o https")
                    Continue For
                End If

                portali.Add(New Portale With {.Nome = nome.Trim(), .Schema = schema.Trim()})

            Next

            If portali.Count > 0 Then dentro.Portali = portali

        End Sub

        ''' <summary>
        ''' Le ricerche salvate. Una che nomina un portale sconosciuto viene scartata e
        ''' detta: aprirla porterebbe da nessuna parte, e sparire in silenzio dal menù
        ''' sarebbe peggio.
        ''' </summary>
        Private Shared Sub LeggiSalvate(radice As JsonObject, dentro As Ricerche,
                                        scartati As List(Of String))

            Dim elenco As JsonArray = TryCast(radice("salvate"), JsonArray)
            If elenco Is Nothing Then Return

            Dim salvate As New List(Of RicercaSalvata)

            For Each voce As JsonNode In elenco

                Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                Dim nome As String = Testo(oggetto, "nome")
                Dim portale As String = Testo(oggetto, "portale")

                If String.IsNullOrWhiteSpace(nome) Then
                    scartati.Add("una ricerca salvata senza nome è stata scartata")
                    Continue For
                End If

                If dentro.TrovaPortale(portale) Is Nothing Then
                    scartati.Add($"la ricerca «{nome.Trim()}» nomina il portale sconosciuto «{portale}»")
                    Continue For
                End If

                salvate.Add(New RicercaSalvata With {
                    .Nome = nome.Trim(),
                    .Portale = dentro.TrovaPortale(portale).Nome,
                    .Cosa = If(Testo(oggetto, "cosa"), String.Empty).Trim(),
                    .Dove = If(Testo(oggetto, "dove"), String.Empty).Trim()})

            Next

            dentro.Salvate = salvate

        End Sub

        ''' <summary>Il file come va scritto: le due sezioni, in ordine di lettura.</summary>
        Public Function ComeJson() As JsonObject

            Return New JsonObject From {
                {"portali", New JsonArray(
                    Portali.Select(Function(p) CType(New JsonObject From {
                        {"nome", p.Nome}, {"schema", p.Schema}}, JsonNode)).ToArray())},
                {"salvate", New JsonArray(
                    Salvate.Select(Function(s) CType(New JsonObject From {
                        {"nome", s.Nome}, {"portale", s.Portale},
                        {"cosa", s.Cosa}, {"dove", s.Dove}}, JsonNode)).ToArray())}}

        End Function

        ''' <summary>
        ''' Un indirizzo del web, e nient'altro: <c>http</c> o <c>https</c>, assoluto.
        ''' I segnaposto si sostituiscono prima del controllo, perché <c>{cosa}</c> fra le
        ''' graffe non è un carattere valido in un indirizzo e boccerebbe ogni schema.
        ''' </summary>
        Private Shared Function IndirizzoDelWeb(schema As String) As Boolean

            Dim provato As String = schema.Trim().
                Replace("{cosa}", "cosa").Replace("{dove}", "dove")

            Dim indirizzo As Uri = Nothing
            If Not Uri.TryCreate(provato, UriKind.Absolute, indirizzo) Then Return False

            Return indirizzo.Scheme = Uri.UriSchemeHttp OrElse indirizzo.Scheme = Uri.UriSchemeHttps

        End Function

        ''' <summary>Un campo di testo, <c>Nothing</c> se manca o non è testo.</summary>
        Private Shared Function Testo(oggetto As JsonObject, nome As String) As String

            If oggetto Is Nothing Then Return Nothing

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(nome, valore) OrElse valore Is Nothing Then
                Return Nothing
            End If

            Return If(valore.GetValueKind() = JsonValueKind.String, valore.GetValue(Of String)(), Nothing)

        End Function

    End Class

End Namespace
