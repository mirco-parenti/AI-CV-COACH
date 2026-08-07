Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Ai

    ''' <summary>Da dove arrivano i modelli in uso.</summary>
    Public Enum OrigineModelli
        ''' <summary>La coppia che il programma porta dentro di sé.</summary>
        Predefinita
        ''' <summary>Il file modelli.json della cartella dati.</summary>
        File
    End Enum

    ''' <summary>
    ''' Il modello concreto che sta dietro a un livello del pool. I prompt non
    ''' nominano mai un modello: dichiarano un <i>livello</i> ("semplice" o
    ''' "ragionamento", cap. 04) ed è qui che il livello diventa un identificativo
    ''' vero per l'API.
    ''' </summary>
    Public Class ModelloConcreto

        ''' <summary>L'identificativo per l'API, es. <c>claude-haiku-4-5</c>.</summary>
        Public Property Id As String

        ''' <summary>
        ''' Cosa dichiarare all'API a proposito del ragionamento esteso. Tre stati e
        ''' non due, di proposito:
        ''' <list type="bullet">
        ''' <item><c>Nothing</c> — non se ne parla: il corpo della richiesta resta
        ''' identico a quello del prototipo, che il campo non lo manda affatto.</item>
        ''' <item><c>False</c> — spento esplicitamente.</item>
        ''' <item><c>True</c> — acceso, in modalità adattiva.</item>
        ''' </list>
        ''' La differenza fra il primo e il secondo stato non è cosmetica: su Sonnet 4.6
        ''' il ragionamento è già spento di suo, quindi dichiararlo aggiungerebbe una
        ''' differenza fra le due richieste proprio nel collaudo che serve a isolare le
        ''' differenze di codice. Su Sonnet 5 il valore predefinito è opposto, e lì
        ''' l'interruttore va acceso: <c>max_tokens</c> limita ragionamento e risposta
        ''' <b>insieme</b>, e i nostri limiti (1500–4000) sono cuciti addosso alla sola
        ''' risposta — senza spegnerlo, le risposte si troncano senza errore.
        ''' </summary>
        Public Property RagionamentoEsteso As Boolean?

    End Class

    ''' <summary>
    ''' La mappa livello → modello (cap. 02.5). Sta fuori dal codice per la stessa
    ''' ragione della taratura: cambiare modello deve costare una riga, non una nuova
    ''' build da reinstallare su due macchine.
    ''' </summary>
    Public Class Modelli

        ''' <summary>Il livello dei compiti meccanici: estrazioni e strutturazioni.</summary>
        Public Const Semplice As String = "semplice"

        ''' <summary>Il livello del confronto, della mitigazione e della generazione.</summary>
        Public Const Ragionamento As String = "ragionamento"

        ''' <summary>Il modello dietro il livello "semplice".</summary>
        Public Property ModelloSemplice As ModelloConcreto

        ''' <summary>Il modello dietro il livello "ragionamento".</summary>
        Public Property ModelloRagionamento As ModelloConcreto

        ''' <summary>Da dove vengono i valori in uso.</summary>
        Public Property Origine As OrigineModelli = OrigineModelli.Predefinita

        ''' <summary>
        ''' Motivo per cui si è ripiegato sui predefiniti, da annotare nel log;
        ''' <c>Nothing</c> se non c'è stato alcun ripiego.
        ''' </summary>
        Public Property Avviso As String

        ''' <summary>
        ''' I modelli predefiniti. Sono quelli del prototipo — Haiku 4.5 per le
        ''' estrazioni, Sonnet 4.6 per il ragionamento — perché la batteria di
        ''' non-regressione di T2 confronta la nuova app col prototipo <b>a parità di
        ''' modello</b>: così una differenza nei risultati è una differenza di codice,
        ''' non del modello sotto.
        ''' </summary>
        ''' <remarks>
        ''' Il modello di prodotto resta <b>Sonnet 5</b> (cap. 15, voce 6): il salto è
        ''' il secondo esperimento, si fa da <c>modelli.json</c> senza ricompilare, e
        ''' quando avrà superato il confronto diventerà lui il predefinito.
        ''' </remarks>
        Public Shared Function Predefiniti() As Modelli
            Return New Modelli With {
                .ModelloSemplice = New ModelloConcreto With {.Id = "claude-haiku-4-5"},
                .ModelloRagionamento = New ModelloConcreto With {.Id = "claude-sonnet-4-6"},
                .Origine = OrigineModelli.Predefinita
            }
        End Function

        ''' <summary>
        ''' Il percorso del file dei modelli nella cartella dati predefinita. Dov'è la
        ''' cartella lo sa <see cref="Dati.CartellaDati"/>, non questa classe (cap. 11.1).
        ''' </summary>
        Public Shared ReadOnly Property PercorsoPredefinito As String
            Get
                Return Dati.CartellaDati.Predefinita().FileModelli
            End Get
        End Property

        ''' <summary>
        ''' Carica i modelli dal file indicato. Se il file manca o è illeggibile
        ''' restituisce i predefiniti annotando il motivo in <see cref="Avviso"/>: un
        ''' file di configurazione corrotto non deve impedire l'avvio.
        ''' </summary>
        ''' <param name="percorso">Il file da leggere; se omesso, quello della cartella dati.</param>
        Public Shared Function Carica(Optional percorso As String = Nothing) As Modelli

            Dim file As String = If(percorso, PercorsoPredefinito)

            If Not IO.File.Exists(file) Then
                Dim m As Modelli = Predefiniti()
                m.Avviso = $"Modelli non configurati in «{file}»: uso quelli predefiniti."
                Return m
            End If

            Try
                Return DaJson(IO.File.ReadAllText(file, Text.Encoding.UTF8))
            Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                       OrElse TypeOf ex Is UnauthorizedAccessException
                Dim m As Modelli = Predefiniti()
                m.Avviso = $"Modelli illeggibili in «{file}» ({ex.Message}): uso quelli predefiniti."
                Return m
            End Try

        End Function

        ''' <summary>
        ''' Costruisce la mappa da un testo JSON. Ogni livello assente ricade sul
        ''' predefinito, così un file parziale — quello che cambia un modello solo —
        ''' resta utilizzabile. Ogni livello accetta due forme:
        ''' <code>
        ''' { "semplice": "claude-haiku-4-5",
        '''   "ragionamento": { "id": "claude-sonnet-5", "ragionamento_esteso": false } }
        ''' </code>
        ''' La forma breve è la riga secca per un esperimento; quella estesa serve
        ''' quando c'è anche l'interruttore del ragionamento da dichiarare.
        ''' </summary>
        Public Shared Function DaJson(testo As String) As Modelli

            Dim radice As JsonObject = TryCast(JsonNode.Parse(testo), JsonObject)
            If radice Is Nothing Then
                Throw New JsonException("La configurazione dei modelli deve essere un oggetto JSON.")
            End If

            Dim m As Modelli = Predefiniti()
            m.Origine = OrigineModelli.File

            m.ModelloSemplice = LeggiModello(radice, Semplice, m.ModelloSemplice)
            m.ModelloRagionamento = LeggiModello(radice, Ragionamento, m.ModelloRagionamento)

            Return m

        End Function

        ''' <summary>Legge un livello, lasciando il predefinito se assente o vuoto.</summary>
        Private Shared Function LeggiModello(radice As JsonObject, chiave As String,
                                             predefinito As ModelloConcreto) As ModelloConcreto

            Dim nodo As JsonNode = radice(chiave)
            If nodo Is Nothing Then Return predefinito

            ' Forma breve: il solo identificativo.
            Dim valore As JsonValue = TryCast(nodo, JsonValue)
            If valore IsNot Nothing Then
                Dim id As String = valore.ToString()
                If String.IsNullOrWhiteSpace(id) Then Return predefinito
                Return New ModelloConcreto With {.Id = id.Trim()}
            End If

            ' Forma estesa: identificativo più interruttore del ragionamento.
            Dim oggetto As JsonObject = TryCast(nodo, JsonObject)
            If oggetto Is Nothing Then Return predefinito

            Dim idEsteso As String = TryCast(oggetto("id"), JsonValue)?.ToString()
            If String.IsNullOrWhiteSpace(idEsteso) Then Return predefinito

            Dim ragionamento As Boolean?
            Dim interruttore As JsonValue = TryCast(oggetto("ragionamento_esteso"), JsonValue)
            If interruttore IsNot Nothing Then
                Dim acceso As Boolean
                If interruttore.TryGetValue(Of Boolean)(acceso) Then ragionamento = acceso
            End If

            Return New ModelloConcreto With {.Id = idEsteso.Trim(), .RagionamentoEsteso = ragionamento}

        End Function

        ''' <summary>
        ''' Il modello concreto per un livello del pool. Un livello sconosciuto è un
        ''' errore di scrittura del prompt e si ferma subito, con il nome del livello
        ''' in chiaro: scoprirlo a metà di un flusso costerebbe molto di più.
        ''' </summary>
        ''' <param name="livello">Il valore del metadato <c>modello:</c> del prompt.</param>
        Public Function PerLivello(livello As String) As ModelloConcreto

            Select Case If(livello, String.Empty).Trim().ToLowerInvariant()
                Case Semplice
                    Return ModelloSemplice
                Case Ragionamento
                    Return ModelloRagionamento
                Case Else
                    Throw New ArgumentException(
                        $"Livello di modello sconosciuto: «{livello}». I livelli sono «{Semplice}» e «{Ragionamento}».",
                        NameOf(livello))
            End Select

        End Function

    End Class

End Namespace
