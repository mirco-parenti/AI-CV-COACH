Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Text.Encodings.Web

Namespace Mcp

    ''' <summary>
    ''' Le due ere del protocollo MCP (cap. 09.2). Non sono due versioni della stessa
    ''' conversazione: sono due modi diversi di aprirla, e si riconoscono messaggio per
    ''' messaggio.
    ''' </summary>
    Public Enum EraMcp

        ''' <summary>
        ''' Fino alla revisione <c>2025-11-25</c>: si comincia con un <c>initialize</c>,
        ''' e da lì in poi la versione e le capacità si danno per note.
        ''' </summary>
        Legacy

        ''' <summary>
        ''' Dalla revisione <c>2026-07-28</c>: nessun handshake, e ogni richiesta porta
        ''' con sé versione e capacità nel proprio <c>_meta</c>.
        ''' </summary>
        Moderna

    End Enum

    ''' <summary>
    ''' Le costanti del protocollo e il modo di confezionare una risposta (cap. 09.2).
    ''' Qui non si decide niente su <i>cosa</i> rispondere — quello è mestiere del
    ''' <see cref="ServerMcp"/> e del <see cref="CatalogoTool"/> — ma solo su come si
    ''' impacchetta, che è l'unica cosa a cambiare fra le due ere.
    ''' </summary>
    Public NotInheritable Class ProtocolloMcp

        ''' <summary>La revisione moderna che il server parla.</summary>
        Public Const VersioneModerna As String = "2026-07-28"

        ''' <summary>
        ''' La revisione legacy con cui si risponde a un <c>initialize</c> quando quella
        ''' chiesta dal client non è fra quelle note: la spec dice di rispondere con la
        ''' più recente che si sa parlare, ed è questa.
        ''' </summary>
        Public Const VersioneLegacy As String = "2025-11-25"

        ''' <summary>
        ''' Le revisioni legacy che riconosciamo per nome. Fra loro la differenza, per i
        ''' pochi metodi che ci servono (<c>tools/list</c> e <c>tools/call</c>), non
        ''' esiste: elencarle serve solo a poter rispondere «sì, parlo la tua» invece di
        ''' spostare il client su un'altra senza motivo.
        ''' </summary>
        Private Shared ReadOnly LegacyNote As String() =
            {"2025-11-25", "2025-06-18", "2025-03-26", "2024-11-05"}

        ''' <summary>La chiave di <c>_meta</c> con la versione del protocollo: è lei a dire l'era.</summary>
        Public Const ChiaveVersione As String = "io.modelcontextprotocol/protocolVersion"

        ''' <summary>Le capacità del client, obbligatorie su ogni richiesta moderna.</summary>
        Public Const ChiaveCapacitaClient As String = "io.modelcontextprotocol/clientCapabilities"

        ''' <summary>Chi è il client: facoltativo, e comunque solo da mostrare.</summary>
        Public Const ChiaveInfoClient As String = "io.modelcontextprotocol/clientInfo"

        ''' <summary>Chi siamo noi, nel <c>_meta</c> di ogni risposta moderna.</summary>
        Public Const ChiaveInfoServer As String = "io.modelcontextprotocol/serverInfo"

        ''' <summary>Il nome con cui il server si presenta (quello della configurazione del client).</summary>
        Public Const NomeServer As String = "trovalavoro"

        ' I codici standard di JSON-RPC 2.0, più quello che MCP ha aggiunto per la
        ' versione non supportata. Non se ne inventano altri: la fascia da -32020 a
        ' -32099 è riservata alla specifica, e usarla per conto proprio vorrebbe dire
        ' dire una cosa che al client ne suona un'altra.
        Public Const ErroreParse As Integer = -32700
        Public Const ErroreRichiestaNonValida As Integer = -32600
        Public Const ErroreMetodoIgnoto As Integer = -32601
        Public Const ErroreParametriNonValidi As Integer = -32602
        Public Const ErroreInterno As Integer = -32603
        Public Const ErroreVersioneNonSupportata As Integer = -32022

        ''' <summary>
        ''' Come si scrive un messaggio sul filo: <b>su una riga sola</b>, perché è la
        ''' riga a separare un messaggio dal successivo, e con gli accenti in chiaro
        ''' invece che in codice — il JSON standard scappa comunque i caratteri di
        ''' controllo, quindi un a capo dentro un testo resta scritto <c>\n</c> e non
        ''' spezza niente.
        ''' </summary>
        Private Shared ReadOnly SuUnaRiga As New JsonSerializerOptions With {
            .WriteIndented = False,
            .Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        Private Sub New()
        End Sub

        ''' <summary>Un nodo JSON su una riga sola, pronto da mandare o da mettere in un blocco di testo.</summary>
        Public Shared Function Compatto(nodo As JsonNode) As String

            If nodo Is Nothing Then Return "null"
            Return nodo.ToJsonString(SuUnaRiga)

        End Function

        ''' <summary>Se una revisione legacy è fra quelle che riconosciamo per nome.</summary>
        Public Shared Function LegacyConosciuta(versione As String) As Boolean

            If String.IsNullOrWhiteSpace(versione) Then Return False

            For Each nota As String In LegacyNote
                If nota.Equals(versione.Trim(), StringComparison.Ordinal) Then Return True
            Next

            Return False

        End Function

        ''' <summary>
        ''' Le versioni che il server dichiara di parlare, moderne e legacy insieme: è
        ''' quel che va in <c>server/discover</c> e nell'errore di versione non
        ''' supportata.
        ''' </summary>
        Public Shared Function VersioniSupportate() As JsonArray

            Dim elenco As New JsonArray From {VersioneModerna}

            For Each nota As String In LegacyNote
                elenco.Add(nota)
            Next

            Return elenco

        End Function

        ''' <summary>Nome e versione del programma, come si presentano al client.</summary>
        Public Shared Function InfoServer() As JsonObject

            Return New JsonObject From {
                {"name", NomeServer},
                {"title", "TrovaLavoro"},
                {"version", Versione.Numero}}

        End Function

        ''' <summary>
        ''' Una risposta riuscita. Nell'era moderna il risultato dichiara il proprio tipo
        ''' e si porta dietro chi l'ha prodotto; in quella legacy nessuna delle due cose
        ''' esiste, e aggiungerle sarebbe roba che il client non sa leggere.
        ''' </summary>
        ''' <param name="id">L'identificativo della richiesta, ricopiato tale e quale.</param>
        Public Shared Function Risposta(id As JsonNode, contenuto As JsonObject, era As EraMcp) As JsonObject

            Dim risultato As JsonObject = If(contenuto, New JsonObject())

            If era = EraMcp.Moderna Then
                risultato("resultType") = "complete"
                risultato("_meta") = New JsonObject From {{ChiaveInfoServer, InfoServer()}}
            End If

            Return New JsonObject From {
                {"jsonrpc", "2.0"},
                {"id", Ricopia(id)},
                {"result", risultato}}

        End Function

        ''' <summary>
        ''' Un errore di protocollo: la richiesta non si è potuta nemmeno prendere in
        ''' considerazione. <b>Non</b> è il modo di dire che un tool non ce l'ha fatta —
        ''' quello è un risultato normale marcato <c>isError</c> (cap. 09.2), e la
        ''' differenza conta perché di là il modello può leggere e correggersi, di qua no.
        ''' </summary>
        Public Shared Function Errore(id As JsonNode, codice As Integer, messaggio As String,
                                      Optional dati As JsonNode = Nothing) As JsonObject

            Dim guasto As New JsonObject From {
                {"code", codice},
                {"message", messaggio}}

            If dati IsNot Nothing Then guasto("data") = dati

            Return New JsonObject From {
                {"jsonrpc", "2.0"},
                {"id", Ricopia(id)},
                {"error", guasto}}

        End Function

        ''' <summary>
        ''' L'errore di versione, con l'elenco di quelle che parliamo: senza quell'elenco
        ''' il client sa solo di aver sbagliato, e non ha modo di riprovare giusto.
        ''' </summary>
        Public Shared Function ErroreDiVersione(id As JsonNode, chiesta As String) As JsonObject

            Dim dati As New JsonObject From {{"supported", VersioniSupportate()}}
            If chiesta IsNot Nothing Then dati("requested") = chiesta

            Return Errore(id, ErroreVersioneNonSupportata, "Unsupported protocol version", dati)

        End Function

        ''' <summary>
        ''' Un nodo JSON non sta in due alberi: se lo si appende dov'è già appeso, si
        ''' solleva. L'identificativo della richiesta va quindi ricopiato prima di
        ''' metterlo nella risposta.
        ''' </summary>
        Private Shared Function Ricopia(nodo As JsonNode) As JsonNode

            If nodo Is Nothing Then Return Nothing
            Return nodo.DeepClone()

        End Function

    End Class

End Namespace
