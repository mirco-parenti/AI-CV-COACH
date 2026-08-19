Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Dati

Namespace Mcp

    ''' <summary>
    ''' Un messaggio arrivato dal client, letto e riconosciuto (cap. 09.2): chi è, che
    ''' cosa chiede, e soprattutto <b>in quale era</b> parla.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>L'era si legge qui e non si ricorda.</b> Il protocollo moderno è senza
    ''' stato, e la spec dice esplicitamente che un processo stdio non è una
    ''' conversazione: sulla stessa pipe possono passare richieste che non c'entrano
    ''' niente fra loro. Ricordarsi «questo client è legacy» perché ha aperto con un
    ''' <c>initialize</c> sarebbe comodo e sbagliato.</para>
    ''' <para><b>Un messaggio storto non è un'eccezione.</b> Vale la stessa regola del
    ''' resto del programma: qui si torna comunque un oggetto — o <c>Nothing</c> se non
    ''' era nemmeno JSON — e chi ha chiamato decide quale errore scrivere.</para>
    ''' </remarks>
    Public Class RichiestaMcp

        Private Sub New()
        End Sub

        ''' <summary>
        ''' L'identificativo, da ricopiare nella risposta. È <c>Nothing</c> nelle
        ''' notifiche, che per definizione non ricevono risposta.
        ''' </summary>
        Public ReadOnly Property Id As JsonNode

        ''' <summary>Il metodo chiesto; <c>Nothing</c> se il messaggio non ne dichiara uno.</summary>
        Public ReadOnly Property Metodo As String

        ''' <summary>I parametri, o <c>Nothing</c>: molti metodi non ne vogliono.</summary>
        Public ReadOnly Property Parametri As JsonObject

        ''' <summary>Il <c>_meta</c> dei parametri, dove vive tutto ciò che riguarda il protocollo.</summary>
        Public ReadOnly Property Meta As JsonObject

        ''' <summary>L'era in cui questo messaggio è scritto.</summary>
        Public ReadOnly Property Era As EraMcp

        ''' <summary>
        ''' La versione dichiarata nel <c>_meta</c>; <c>Nothing</c> in un messaggio
        ''' legacy, dove la versione non viaggia con la richiesta.
        ''' </summary>
        Public ReadOnly Property VersioneDichiarata As String

        ''' <summary>
        ''' Una notifica: niente identificativo, e quindi <b>nessuna risposta</b>. Vale
        ''' anche quando non sappiamo che farcene — a una notifica non si può nemmeno
        ''' rispondere «non ti capisco».
        ''' </summary>
        Public ReadOnly Property ENotifica As Boolean
            Get
                Return Id Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' Se il client ha dichiarato le proprie capacità. Nell'era moderna è
        ''' obbligatorio su <b>ogni</b> richiesta, e senza il server non può sapere che
        ''' cosa gli è lecito chiedere in cambio.
        ''' </summary>
        Public ReadOnly Property CapacitaDichiarate As Boolean
            Get
                Return CampiJson.Nodo(Meta, ProtocolloMcp.ChiaveCapacitaClient) IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Legge una riga di stdin. Torna <c>Nothing</c> se non è JSON leggibile o se
        ''' non è nemmeno un oggetto: in quel caso non c'è niente da cui ricavare un
        ''' identificativo, e la risposta sarà un errore di lettura senza id.
        ''' </summary>
        Public Shared Function Leggi(riga As String) As RichiestaMcp

            If String.IsNullOrWhiteSpace(riga) Then Return Nothing

            Dim radice As JsonNode
            Try
                radice = JsonNode.Parse(riga)
            Catch e As JsonException
                Return Nothing
            End Try

            Dim messaggio As JsonObject = TryCast(radice, JsonObject)
            If messaggio Is Nothing Then Return Nothing

            Dim letta As New RichiestaMcp With {
                ._Metodo = CampiJson.Testo(messaggio, "method"),
                ._Parametri = TryCast(CampiJson.Nodo(messaggio, "params"), JsonObject)}

            ' L'identificativo può essere un testo o un numero, e non si interpreta:
            ' torna al client esattamente com'è arrivato. Un id nullo, che JSON-RPC
            ' ammetterebbe, MCP lo vieta: lo trattiamo come assente.
            Dim id As JsonNode = CampiJson.Nodo(messaggio, "id")
            If id IsNot Nothing AndAlso id.GetValueKind() <> JsonValueKind.Null Then letta._Id = id

            letta._Meta = TryCast(CampiJson.Nodo(letta.Parametri, "_meta"), JsonObject)
            letta._VersioneDichiarata = CampiJson.Testo(letta.Meta, ProtocolloMcp.ChiaveVersione)

            ' Ecco il riconoscimento, ed è tutto qui: chi dichiara la versione nel
            ' proprio _meta parla la lingua nuova, tutti gli altri la vecchia.
            letta._Era = If(letta.VersioneDichiarata Is Nothing, EraMcp.Legacy, EraMcp.Moderna)

            Return letta

        End Function

    End Class

End Namespace
