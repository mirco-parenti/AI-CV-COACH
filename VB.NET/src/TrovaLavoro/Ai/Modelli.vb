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
        ''' Quanto costa un milione di token, modello per modello. Vive qui perché vive
        ''' nello stesso file: chi cambia modello vuole poter dire, nella riga accanto,
        ''' quanto costa quello nuovo (v. <see cref="Listino"/>).
        ''' </summary>
        Public Property Prezzi As Listino = Listino.Predefinito()

        ''' <summary>
        ''' Motivo per cui si è ripiegato sui predefiniti, da annotare nel log;
        ''' <c>Nothing</c> se non c'è stato alcun ripiego.
        ''' </summary>
        Public Property Avviso As String

        ''' <summary>
        ''' I modelli predefiniti: <b>Haiku 4.5</b> per le estrazioni e <b>Sonnet 5</b>
        ''' per il ragionamento (cap. 02.5, cap. 15 voce 6). Haiku 4.5 è tuttora
        ''' l'ultimo della sua fascia — il salto riguarda il solo ragionamento, dove
        ''' Sonnet 4.6 è passato fra i modelli superati e Sonnet 5 gli succede a un
        ''' prezzo più basso.
        ''' </summary>
        ''' <remarks>
        ''' Sul ragionamento il predefinito <b>non è più quello del prototipo</b>, che
        ''' resta congelato su Sonnet 4.6: da qui in avanti la batteria di T2 non misura
        ''' più una parità di modello, e una differenza nei risultati può venire tanto
        ''' dal codice quanto dal modello sotto. È il passo che il cap. 02.5 chiamava
        ''' «il secondo esperimento», fatto. Il livello semplice invece la parità la
        ''' conserva: Haiku 4.5 è lo stesso da entrambe le parti.
        ''' L'interruttore del ragionamento è dichiarato <b>spento</b> di proposito:
        ''' Sonnet 5 lo terrebbe acceso di suo e <c>max_tokens</c> limita ragionamento e
        ''' risposta insieme, così i limiti del pool — cuciti addosso alla sola risposta
        ''' — troncherebbero senza errore.
        ''' </remarks>
        Public Shared Function Predefiniti() As Modelli
            Return New Modelli With {
                .ModelloSemplice = New ModelloConcreto With {.Id = "claude-haiku-4-5"},
                .ModelloRagionamento = New ModelloConcreto With {.Id = "claude-sonnet-5",
                                                                 .RagionamentoEsteso = False},
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
            m.Prezzi = Listino.Sopra(Listino.Predefinito(), radice)

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

        ''' <summary>UTF-8 senza BOM, come gli altri JSON dell'applicazione.</summary>
        Private Shared ReadOnly Codifica As New Text.UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        ''' <summary>Rientri e accenti in chiaro: il file è fatto per essere letto a mano.</summary>
        Private Shared ReadOnly FormatoLeggibile As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        ''' <summary>
        ''' Il testo di <c>modelli.json</c> con un livello cambiato, e <b>tutto il resto
        ''' com'era</b>.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>Si riscrive un campo, non il file.</b> Le Impostazioni conoscono i
        ''' due identificativi e nient'altro: l'interruttore del ragionamento, un commento
        ''' o una chiave che qualcuno ha messo lì per un esperimento non sono roba loro, e
        ''' riscrivere il file da zero li cancellerebbe senza dirlo. Perciò si parte dal
        ''' testo che c'è e si tocca solo il campo richiesto.</para>
        ''' <para><b>La forma si conserva.</b> Un livello scritto nella forma breve resta
        ''' breve, uno esteso resta esteso col resto dei suoi campi: chi ha scritto quel
        ''' file a mano lo ritrova come lo aveva lasciato.</para>
        ''' <para><b>Un livello che nel file non c'è si scrive con quel che vale
        ''' adesso</b>, interruttore compreso. Non è un dettaglio: il predefinito del
        ''' ragionamento dichiara l'interruttore <b>spento</b> (cap. 02.5), e scrivere il
        ''' solo identificativo lo riporterebbe a «non dichiarato» — cioè acceso, su
        ''' Sonnet 5 — troncando le risposte senza errore. Cambiare modello non deve
        ''' cambiare di nascosto una seconda cosa.</para>
        ''' <para>Un testo che non è un oggetto JSON <b>solleva</b> invece di essere
        ''' sostituito: quel file è dell'utente, e riscriverlo sopra perché non si capisce
        ''' sarebbe il modo più veloce di perdere quel che c'era dentro.</para>
        ''' </remarks>
        ''' <param name="testoEsistente">Il contenuto attuale del file; vuoto se non c'è ancora.</param>
        ''' <param name="livello">"semplice" o "ragionamento".</param>
        ''' <param name="id">L'identificativo del modello nuovo.</param>
        ''' <param name="interruttoreInVigore">
        ''' L'interruttore del ragionamento che vale adesso per quel livello, da scrivere
        ''' solo se il livello nel file ancora non c'è.
        ''' </param>
        Public Shared Function ConLivello(testoEsistente As String, livello As String, id As String,
                                          interruttoreInVigore As Boolean?) As String

            Dim chiave As String = LivelloValido(livello)

            If String.IsNullOrWhiteSpace(id) Then
                Throw New ArgumentException("L'identificativo del modello non può essere vuoto.", NameOf(id))
            End If

            Dim nuovo As String = id.Trim()
            Dim radice As JsonObject

            If String.IsNullOrWhiteSpace(testoEsistente) Then
                radice = New JsonObject()
            Else
                radice = TryCast(JsonNode.Parse(testoEsistente), JsonObject)
                If radice Is Nothing Then
                    Throw New JsonException("La configurazione dei modelli deve essere un oggetto JSON.")
                End If
            End If

            Dim esistente As JsonNode = radice(chiave)

            ' Forma estesa: si sostituisce il solo identificativo e il resto resta dov'è.
            Dim oggetto As JsonObject = TryCast(esistente, JsonObject)
            If oggetto IsNot Nothing Then
                oggetto("id") = JsonValue.Create(nuovo)
                Return radice.ToJsonString(FormatoLeggibile)
            End If

            ' Forma breve già presente: resta breve.
            If TryCast(esistente, JsonValue) IsNot Nothing Then
                radice(chiave) = JsonValue.Create(nuovo)
                Return radice.ToJsonString(FormatoLeggibile)
            End If

            ' Assente: si scrive quel che vale adesso, interruttore compreso quando c'è.
            If interruttoreInVigore.HasValue Then
                radice(chiave) = New JsonObject From {
                    {"id", JsonValue.Create(nuovo)},
                    {"ragionamento_esteso", JsonValue.Create(interruttoreInVigore.Value)}}
            Else
                radice(chiave) = JsonValue.Create(nuovo)
            End If

            Return radice.ToJsonString(FormatoLeggibile)

        End Function

        ''' <summary>
        ''' Cambia il modello di un livello: sul disco e <b>in vigore</b>, subito.
        ''' </summary>
        ''' <remarks>
        ''' <para>Questa mappa è l'oggetto che il client dell'AI tiene in mano e interroga
        ''' a ogni chiamata (<see cref="PerLivello"/>): cambiarlo qui dentro vuol dire che
        ''' la prossima chiamata parte già col modello nuovo, senza riavviare il
        ''' programma. Una chiamata già in volo ha invece il suo modello in mano da prima,
        ''' e finisce con quello — che è l'unico esito sensato: cambiarle il modello a metà
        ''' non si potrebbe comunque.</para>
        ''' <para>Il file si scrive per intero solo dopo averlo composto in memoria: se il
        ''' disco si rifiuta, solleva e <b>niente</b> è cambiato, né lì né qui. Il
        ''' contrario — un modello in vigore che il file non conferma — durerebbe fino al
        ''' riavvio e poi tornerebbe indietro da solo, senza che nessuno capisca perché.</para>
        ''' </remarks>
        ''' <param name="percorso">Il file da aggiornare; se omesso, quello della cartella dati.</param>
        Public Sub CambiaModello(livello As String, id As String, Optional percorso As String = Nothing)

            Dim chiave As String = LivelloValido(livello)
            Dim file As String = If(percorso, PercorsoPredefinito)

            Dim testo As String = If(IO.File.Exists(file), IO.File.ReadAllText(file, Text.Encoding.UTF8), Nothing)
            Dim aggiornato As String = ConLivello(testo, chiave, id, PerLivello(chiave).RagionamentoEsteso)

            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(file))

            Dim temporaneo As String = file & ".tmp"
            IO.File.WriteAllText(temporaneo, aggiornato, Codifica)
            IO.File.Move(temporaneo, file, overwrite:=True)

            ' Il file adesso dice questo, quindi la provenienza è lui: dichiarare ancora
            ' «predefiniti» farebbe raccontare alle Impostazioni una storia vecchia di un
            ' istante.
            Dim cambiato As New ModelloConcreto With {
                .Id = id.Trim(),
                .RagionamentoEsteso = PerLivello(chiave).RagionamentoEsteso}

            If chiave = Semplice Then ModelloSemplice = cambiato Else ModelloRagionamento = cambiato
            Origine = OrigineModelli.File
            Avviso = Nothing

        End Sub

        ''' <summary>Il nome del livello, normalizzato; solleva se non è uno dei due.</summary>
        Private Shared Function LivelloValido(livello As String) As String

            Dim pulito As String = If(livello, String.Empty).Trim().ToLowerInvariant()
            If pulito = Semplice OrElse pulito = Ragionamento Then Return pulito

            Throw New ArgumentException(
                $"Livello di modello sconosciuto: «{livello}». I livelli sono «{Semplice}» e «{Ragionamento}».",
                NameOf(livello))

        End Function

        ''' <summary>
        ''' Il modello concreto per un livello del pool. Un livello sconosciuto è un
        ''' errore di scrittura del prompt e si ferma subito, con il nome del livello
        ''' in chiaro: scoprirlo a metà di un flusso costerebbe molto di più.
        ''' </summary>
        ''' <param name="livello">Il valore del metadato <c>modello:</c> del prompt.</param>
        Public Function PerLivello(livello As String) As ModelloConcreto

            If LivelloValido(livello) = Semplice Then Return ModelloSemplice
            Return ModelloRagionamento

        End Function

    End Class

End Namespace
