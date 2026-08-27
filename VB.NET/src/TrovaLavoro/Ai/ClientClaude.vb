Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>Perché una chiamata all'AI non è andata a buon fine.</summary>
    Public Enum CausaErroreAi
        ''' <summary>L'API non è raggiungibile: connessione assente o caduta.</summary>
        Rete
        ''' <summary>L'attesa massima è scaduta senza risposta.</summary>
        Timeout
        ''' <summary>Chiave API assente, non valida o senza permessi.</summary>
        Chiave
        ''' <summary>L'API ha rifiutato la richiesta: è un errore nostro, non suo.</summary>
        Richiesta
        ''' <summary>
        ''' Il modello richiesto non esiste (più): è stato ritirato dal listino, o questa
        ''' chiave non può usarlo. Si distingue da <see cref="Richiesta"/> perché la cura
        ''' è una sola e precisa — sceglierne un altro nelle Impostazioni — mentre una
        ''' richiesta rifiutata è un difetto nostro, e mandare l'utente a cercarlo sarebbe
        ''' mandarlo a cercare quel che non troverà.
        ''' </summary>
        ModelloRitirato
        ''' <summary>Troppe richieste: si è superato il limite di frequenza.</summary>
        Limite
        ''' <summary>Guasto temporaneo dalla parte dell'API.</summary>
        Servizio
        ''' <summary>La risposta si è fermata contro il limite di token: è monca.</summary>
        Troncata
        ''' <summary>Il modello si è rifiutato di rispondere.</summary>
        Rifiuto
        ''' <summary>La risposta è arrivata, ma non ha la forma attesa.</summary>
        RispostaInattesa
    End Enum

    ''' <summary>
    ''' Un errore di chiamata all'AI, già scritto in italiano per l'utente (cap. 02.5).
    ''' Porta con sé la <see cref="Causa"/> perché l'interfaccia possa reagire di
    ''' conseguenza: una chiave sbagliata si risolve nelle Impostazioni, una rete caduta
    ''' col pulsante «Riprova».
    ''' </summary>
    Public Class ErroreAi
        Inherits Exception

        ''' <summary>Che tipo di errore è.</summary>
        Public ReadOnly Property Causa As CausaErroreAi

        ''' <summary>Quanto aspettare prima di riprovare, se l'API l'ha suggerito.</summary>
        Public ReadOnly Property AttesaSuggerita As TimeSpan?

        Public Sub New(causa As CausaErroreAi, messaggio As String,
                       Optional interna As Exception = Nothing,
                       Optional attesaSuggerita As TimeSpan? = Nothing)
            MyBase.New(messaggio, interna)
            Me.Causa = causa
            Me.AttesaSuggerita = attesaSuggerita
        End Sub

        ''' <summary>
        ''' Se ha senso riprovare da soli. Vale per gli inciampi di passaggio; non per
        ''' una richiesta malformata o una chiave sbagliata, che riprovando darebbero
        ''' esattamente lo stesso errore.
        ''' </summary>
        Public ReadOnly Property Ritentabile As Boolean
            Get
                Return Causa = CausaErroreAi.Rete OrElse Causa = CausaErroreAi.Timeout OrElse
                       Causa = CausaErroreAi.Limite OrElse Causa = CausaErroreAi.Servizio
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Un turno di conversazione, come lo vuole l'API: chi parla e cosa dice.
    ''' </summary>
    ''' <remarks>
    ''' Serve dal brainstorming in poi (T7c). Prima di allora nel progetto non c'erano
    ''' conversazioni vere: anche il dialogo guidato del profilo è fatto di turni
    ''' <b>indipendenti</b> — sette chiamate che non si ricordano l'una dell'altra, con
    ''' la memoria tenuta dal programma (cap. 02.5). Il brainstorming invece è una
    ''' chiacchierata sola che cresce, e va mandata come tale.
    ''' </remarks>
    Public Class TurnoChat

        ''' <summary>Chi parla: <c>user</c> oppure <c>assistant</c>.</summary>
        Public Property Ruolo As String

        ''' <summary>
        ''' Cosa dice: una stringa per il testo, un elenco di blocchi per i PDF.
        ''' L'API accetta entrambe le forme.
        ''' </summary>
        Public Property Contenuto As JsonNode

        ''' <summary>
        ''' Il contenuto come testo, quando è testo; vuoto quando è un elenco di blocchi.
        ''' </summary>
        Public ReadOnly Property Testo As String
            Get
                Return If(TryCast(Contenuto, JsonValue)?.ToString(), String.Empty)
            End Get
        End Property

        ''' <summary>Il ruolo di chi scrive all'AI.</summary>
        Public Const Utente As String = "user"

        ''' <summary>Il ruolo dell'AI.</summary>
        Public Const Assistente As String = "assistant"

        ''' <summary>Un turno dell'utente, dal suo testo.</summary>
        Public Shared Function DallUtente(testo As String) As TurnoChat
            Return New TurnoChat With {.Ruolo = Utente, .Contenuto = JsonValue.Create(If(testo, String.Empty))}
        End Function

        ''' <summary>Un turno dell'assistente, dal testo che aveva risposto.</summary>
        Public Shared Function DallAssistente(testo As String) As TurnoChat
            Return New TurnoChat With {.Ruolo = Assistente, .Contenuto = JsonValue.Create(If(testo, String.Empty))}
        End Function

    End Class

    ''' <summary>Quello che l'AI ha risposto, già sbucciato.</summary>
    Public Class RispostaAi

        ''' <summary>
        ''' Il motivo di fine che l'API dà alla risposta tagliata dal tetto dei token.
        ''' </summary>
        Public Const MotivoTroncata As String = "max_tokens"

        ''' <summary>Il testo prodotto dal modello: JSON da estrarre, o prosa.</summary>
        Public Property Testo As String

        ''' <summary>Il motivo per cui il modello ha smesso di scrivere.</summary>
        Public Property MotivoFine As String

        ''' <summary>
        ''' Vero se il modello ha smesso perché ha toccato il tetto dei token, e quindi
        ''' quello che ha scritto è monco.
        ''' </summary>
        ''' <remarks>
        ''' Sulla strada sincrona un troncamento è un errore e non arriva mai fin qui
        ''' (v. <see cref="ClientClaude.InterpretaRisposta"/>); in streaming invece è un
        ''' esito legittimo, perché il testo è già sotto gli occhi di chi legge — e allora
        ''' questa è la domanda che il pannello deve poter fare.
        ''' </remarks>
        Public ReadOnly Property Troncata As Boolean
            Get
                Return String.Equals(MotivoFine, MotivoTroncata, StringComparison.Ordinal)
            End Get
        End Property

        ''' <summary>Il modello che ha risposto davvero, come lo dichiara l'API.</summary>
        Public Property Modello As String

        ''' <summary>Token consumati in ingresso e in uscita, per tenere il conto.</summary>
        Public Property TokenIngresso As Integer
        Public Property TokenUscita As Integer

    End Class

    ''' <summary>
    ''' Le chiamate a <c>api.anthropic.com</c> (cap. 02.5). HTTPS dirette con
    ''' <see cref="HttpClient"/>, niente SDK, stesso corpo del prototipo: un solo
    ''' messaggio <c>user</c> col prompt già riempito.
    ''' </summary>
    ''' <remarks>
    ''' <para>A T2 erano <b>solo chiamate sincrone</b>: le risposte stanno fra i 1500 e i
    ''' 4000 token e si aspettano bene con un indicatore. Con <b>T7c</b> arriva anche lo
    ''' streaming (<see cref="ChiediInStreamingAsync"/>), perché finalmente c'è un
    ''' pannello che lo mostra man mano: il brainstorming. Le due strade convivono e la
    ''' sincrona <b>non è cambiata</b> — chi non ha niente da mostrare mentre aspetta non
    ''' ha niente da guadagnare, e la non-regressione resta appoggiata a lei.</para>
    ''' <para>Nessuna memoria lato modello: ogni chiamata è autonoma, il contesto lo
    ''' manda il programma.</para>
    ''' </remarks>
    Public Class ClientClaude
        Implements IDisposable

        ''' <summary>
        ''' Dove annotare quanto è costata ogni chiamata, se qualcuno tiene il conto
        ''' (<see cref="IDiarioChiamate"/>). <c>Nothing</c> è legittimo: il diario è
        ''' diagnostica, non una parte del funzionamento.
        ''' </summary>
        ''' <remarks>
        ''' Sta sul client e non nel costruttore dei mestieri perché il client ce l'hanno
        ''' già tutti: appenderlo qui li raggiunge tutti insieme senza toccare sei
        ''' costruttori. A scriverci però non è il client — è <see cref="MestiereAi"/>, che
        ''' è l'unico posto dove il prompt (col suo tetto e il suo nome) e la risposta (coi
        ''' suoi token) si trovano nella stessa riga.
        ''' </remarks>
        Public Property Diario As IDiarioChiamate

        ''' <summary>L'indirizzo dell'API dei messaggi.</summary>
        Public Const Indirizzo As String = "https://api.anthropic.com/v1/messages"

        ''' <summary>La versione dell'API dichiarata a ogni chiamata, come nel prototipo.</summary>
        Public Const VersioneApi As String = "2023-06-01"

        ''' <summary>La variabile d'ambiente da cui si legge la chiave.</summary>
        Public Const NomeVariabileChiave As String = "ANTHROPIC_API_KEY"

        Private Shared ReadOnly AttesaPredefinita As TimeSpan = TimeSpan.FromSeconds(120)

        ''' <summary>
        ''' Il limite di token di una risposta «normale»: fin qui basta
        ''' <see cref="TempoMassimo"/>, oltre l'attesa cresce in proporzione. È il
        ''' limite dei turni del dialogo, cioè della chiamata che non si può annullare
        ''' (cap. 02.6): la loro attesa resta quella di sempre.
        ''' </summary>
        Public Const TokenDiRiferimento As Integer = 4000
        Private Shared ReadOnly PausaPredefinita As TimeSpan = TimeSpan.FromSeconds(2)
        Private Shared ReadOnly PausaMassima As TimeSpan = TimeSpan.FromSeconds(30)
        Private Shared ReadOnly SilenzioPredefinito As TimeSpan = TimeSpan.FromSeconds(30)

        Private ReadOnly _http As HttpClient
        Private ReadOnly _chiave As String
        Private _smaltito As Boolean

        ''' <summary>La mappa livello → modello con cui questo client sta lavorando.</summary>
        Public ReadOnly Property ModelliInUso As Modelli

        ''' <summary>
        ''' Quanto si aspetta una risposta <b>normale</b> prima di dichiarare scaduta
        ''' l'attesa. I prompt che possono produrre molto più testo aspettano in
        ''' proporzione: v. <see cref="AttesaPer"/>.
        ''' </summary>
        Public Property TempoMassimo As TimeSpan

        ''' <summary>Quanto si aspetta prima dell'unico ritentativo.</summary>
        Public Property Pausa As TimeSpan

        ''' <summary>
        ''' In streaming, quanto silenzio si sopporta <b>fra un pezzo e l'altro</b> prima
        ''' di dichiarare scaduta l'attesa.
        ''' </summary>
        ''' <remarks>
        ''' Il metro cambia insieme al trasporto. Per una chiamata sincrona conta il
        ''' tempo totale, e cresce col limite del prompt (<see cref="AttesaPer"/>): finché
        ''' non arriva tutto non è arrivato niente. In streaming quella ragione decade —
        ''' se il testo sta comparendo, la chiamata sta funzionando, per lunga che sia la
        ''' risposta — e un tetto complessivo taglierebbe proprio le risposte lunghe
        ''' legittime. Quello che resta da riconoscere è il collegamento morto, e un
        ''' collegamento morto si vede dal <b>silenzio</b>. *(Deciso a T7c.)*
        ''' </remarks>
        Public Property SilenzioMassimo As TimeSpan

        ''' <param name="chiave">La chiave API.</param>
        ''' <param name="modelli">La mappa dei modelli; se omessa, i predefiniti.</param>
        ''' <param name="messaggero">
        ''' Il gestore HTTP da usare. Serve ai collaudi per far rispondere l'API senza
        ''' rete; in esercizio si lascia stare.
        ''' </param>
        Public Sub New(chiave As String, Optional modelli As Modelli = Nothing,
                       Optional messaggero As HttpMessageHandler = Nothing)

            If String.IsNullOrWhiteSpace(chiave) Then
                Throw New ErroreAi(CausaErroreAi.Chiave,
                    $"Manca la chiave API: definisci la variabile d'ambiente {NomeVariabileChiave}.")
            End If

            _chiave = chiave.Trim()
            ModelliInUso = If(modelli, Modelli.Predefiniti())

            _http = If(messaggero Is Nothing, New HttpClient(),
                       New HttpClient(messaggero, disposeHandler:=False))

            ' L'attesa massima la governiamo chiamata per chiamata: così si distingue
            ' l'attesa scaduta dall'annullamento chiesto dall'utente, che sono due
            ' cose diverse da dire e da trattare.
            _http.Timeout = System.Threading.Timeout.InfiniteTimeSpan

            TempoMassimo = AttesaPredefinita
            Pausa = PausaPredefinita
            SilenzioMassimo = SilenzioPredefinito

        End Sub

        ''' <summary>
        ''' La chiave letta dall'ambiente. A T2 è così: la chiave non tocca il disco e
        ''' non entra nel repo. Dalla 1.0 arriverà cifrata dalla cartella dati
        ''' (cap. 11), e questo metodo resterà il ripiego per il banco di collaudo.
        ''' </summary>
        Public Shared Function ChiaveDaAmbiente() As String

            Dim chiave As String = Environment.GetEnvironmentVariable(NomeVariabileChiave)
            If String.IsNullOrWhiteSpace(chiave) Then
                Throw New ErroreAi(CausaErroreAi.Chiave,
                    $"Manca la chiave API: definisci la variabile d'ambiente {NomeVariabileChiave}.")
            End If

            Return chiave.Trim()

        End Function

        ''' <summary>
        ''' Il corpo JSON della richiesta. È volutamente lo stesso del prototipo —
        ''' <c>model</c>, <c>max_tokens</c>, un messaggio <c>user</c> — e nient'altro:
        ''' niente <c>temperature</c>, niente prefill. Quei due, oltre a non servirci,
        ''' sono fra le rotture note del salto a Sonnet 5.
        ''' </summary>
        ''' <param name="contenuto">
        ''' Il contenuto del messaggio: una stringa per i turni di testo, un elenco di
        ''' blocchi per i PDF. L'API accetta entrambe le forme.
        ''' </param>
        Public Shared Function CorpoRichiesta(modello As ModelloConcreto, maxToken As Integer,
                                              contenuto As JsonNode) As JsonObject

            If contenuto Is Nothing Then Throw New ArgumentNullException(NameOf(contenuto))

            Return CorpoRichiesta(modello, maxToken,
                                  {New TurnoChat With {.Ruolo = TurnoChat.Utente, .Contenuto = contenuto}})

        End Function

        ''' <summary>
        ''' Il corpo JSON di una <b>conversazione</b>: gli stessi campi di sempre, con
        ''' più di un messaggio e — se lo si chiede — la richiesta di rispondere man
        ''' mano invece che tutto alla fine.
        ''' </summary>
        ''' <remarks>
        ''' Con un turno solo e <paramref name="flusso"/> spento il JSON prodotto è
        ''' <b>identico carattere per carattere</b> a quello di prima, e a dirlo non è
        ''' questo commento ma <c>IlCorpoEQuelloDelPrototipo</c>, che gli sta addosso dal
        ''' T2. Era la condizione per aggiungere lo streaming senza rimettere in gioco la
        ''' non-regressione: la strada nuova si affianca alla vecchia, non la riscrive.
        ''' </remarks>
        ''' <param name="turni">La conversazione, dal più vecchio al più recente.</param>
        ''' <param name="flusso">Se chiedere la risposta a pezzi (eventi SSE).</param>
        Public Shared Function CorpoRichiesta(modello As ModelloConcreto, maxToken As Integer,
                                              turni As IReadOnlyList(Of TurnoChat),
                                              Optional flusso As Boolean = False) As JsonObject

            If modello Is Nothing Then Throw New ArgumentNullException(NameOf(modello))
            If turni Is Nothing Then Throw New ArgumentNullException(NameOf(turni))
            If String.IsNullOrWhiteSpace(modello.Id) Then
                Throw New ArgumentException("Il modello non ha un identificativo.", NameOf(modello))
            End If
            If maxToken <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(maxToken),
                    "Il limite di token deve essere positivo: il prompt lo dichiara nei suoi metadati.")
            End If
            If turni.Count = 0 Then
                Throw New ArgumentException("Una richiesta senza messaggi non esiste.", NameOf(turni))
            End If

            Dim messaggi As New JsonArray()
            For Each turno As TurnoChat In turni
                If turno Is Nothing OrElse turno.Contenuto Is Nothing Then
                    Throw New ArgumentException("Un turno della conversazione è vuoto.", NameOf(turni))
                End If
                messaggi.Add(New JsonObject From {
                    {"role", If(turno.Ruolo, TurnoChat.Utente)},
                    {"content", turno.Contenuto}})
            Next

            Dim corpo As New JsonObject From {
                {"model", modello.Id},
                {"max_tokens", maxToken},
                {"messages", messaggi}}

            ' Lo streaming si dichiara solo quando serve: tacere lascia il corpo delle
            ' chiamate sincrone esattamente com'era.
            If flusso Then corpo("stream") = True

            ' Il ragionamento si dichiara solo quando la configurazione lo chiede: a
            ' modello spento di suo, tacere tiene la richiesta identica a quella del
            ' prototipo, che è il punto della non-regressione (cap. 14).
            If modello.RagionamentoEsteso.HasValue Then
                corpo("thinking") = New JsonObject From {
                    {"type", If(modello.RagionamentoEsteso.Value, "adaptive", "disabled")}}
            End If

            Return corpo

        End Function

        ''' <summary>
        ''' Sbuccia la risposta dell'API. Le due fini anomale — risposta troncata e
        ''' rifiuto del modello — diventano errori espliciti invece di scivolare avanti.
        ''' </summary>
        Public Shared Function InterpretaRisposta(testoJson As String) As RispostaAi

            Dim radice As JsonObject
            Try
                radice = TryCast(JsonNode.Parse(testoJson), JsonObject)
            Catch ex As JsonException
                Throw New ErroreAi(CausaErroreAi.RispostaInattesa,
                    "L'AI ha risposto qualcosa che non è JSON.", ex)
            End Try

            If radice Is Nothing Then
                Throw New ErroreAi(CausaErroreAi.RispostaInattesa,
                    "L'AI ha risposto un JSON che non è un oggetto.")
            End If

            Dim motivoFine As String = TryCast(radice("stop_reason"), JsonValue)?.ToString()

            ' Prima il motivo della fine, poi il testo. Una risposta troncata il testo
            ' ce l'ha — solo che è monco: passarlo avanti significa scoprirlo dopo, a
            ' valle, sotto forma di JSON invalido senza sapere perché.
            If String.Equals(motivoFine, RispostaAi.MotivoTroncata, StringComparison.Ordinal) Then
                Throw New ErroreAi(CausaErroreAi.Troncata,
                    "La risposta si è fermata contro il limite di token ed è incompleta. " &
                    "Alza il limite del prompt, oppure accorcia il testo in ingresso.")
            End If

            If String.Equals(motivoFine, "refusal", StringComparison.Ordinal) Then
                Throw New ErroreAi(CausaErroreAi.Rifiuto,
                    "Il modello si è rifiutato di rispondere a questa richiesta.")
            End If

            Dim testo As String = PrimoTesto(radice)
            If testo Is Nothing Then
                Throw New ErroreAi(CausaErroreAi.RispostaInattesa,
                    "La risposta dell'AI non contiene nessun blocco di testo.")
            End If

            Dim uso As JsonObject = TryCast(radice("usage"), JsonObject)

            Return New RispostaAi With {
                .Testo = testo,
                .MotivoFine = motivoFine,
                .Modello = TryCast(radice("model"), JsonValue)?.ToString(),
                .TokenIngresso = Intero(uso, "input_tokens"),
                .TokenUscita = Intero(uso, "output_tokens")}

        End Function

        ''' <summary>
        ''' Quanto aspettare una risposta che può arrivare a <paramref name="maxToken"/>
        ''' token: <see cref="TempoMassimo"/> fino a <see cref="TokenDiRiferimento"/>, e
        ''' in proporzione oltre.
        ''' </summary>
        ''' <remarks>
        ''' Finché le chiamate sono sincrone (lo streaming è di T7, cap. 02.5) il tempo
        ''' di risposta cresce col testo che l'AI scrive: un'attesa fissa trasformerebbe
        ''' un limite generoso in un timeout, cioè un troncamento <i>dichiarato</i> — che
        ''' <see cref="InterpretaRisposta"/> sa riconoscere — in nessuna risposta affatto.
        ''' La proporzione non è una stima della velocità del modello, che varia: è
        ''' semplicemente un'attesa che segue il limite invece di ignorarlo.
        ''' </remarks>
        Public Function AttesaPer(maxToken As Integer) As TimeSpan

            If maxToken <= TokenDiRiferimento Then Return TempoMassimo

            Return TimeSpan.FromTicks(
                CLng(TempoMassimo.Ticks * (maxToken / CDbl(TokenDiRiferimento))))

        End Function

        ''' <summary>
        ''' Chiede all'AI e restituisce la risposta. Un solo ritentativo automatico
        ''' sugli inciampi di passaggio (cap. 02.5), rispettando l'attesa suggerita
        ''' dall'API quando c'è.
        ''' </summary>
        ''' <param name="livello">Il livello dichiarato dal prompt: "semplice" o "ragionamento".</param>
        ''' <param name="contenuto">Il contenuto del messaggio utente.</param>
        ''' <param name="maxToken">Il limite di token della risposta, dai metadati del prompt.</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        Public Async Function ChiediAsync(livello As String, contenuto As JsonNode, maxToken As Integer,
                                          Optional annulla As CancellationToken = Nothing) As Task(Of RispostaAi)

            Dim modello As ModelloConcreto = ModelliInUso.PerLivello(livello)
            Dim testoCorpo As String = CorpoRichiesta(modello, maxToken, contenuto).ToJsonString()
            Dim attesa As TimeSpan = AttesaPer(maxToken)

            Dim primoErrore As ErroreAi = Nothing

            ' Due tentativi in tutto: quello buono e l'unico ritentativo.
            For tentativo As Integer = 1 To 2

                If tentativo > 1 Then
                    Await Task.Delay(QuantoAspettare(primoErrore), annulla).ConfigureAwait(False)
                End If

                Try
                    Return Await UnTentativoAsync(testoCorpo, modello.Id, attesa, annulla).ConfigureAwait(False)
                Catch ex As ErroreAi When ex.Ritentabile AndAlso tentativo = 1
                    primoErrore = ex
                End Try

            Next

            Throw primoErrore

        End Function

        ''' <summary>
        ''' Chiede all'AI usando un prompt del pool: livello e limite di token vengono
        ''' dai suoi metadati, così non si ripetono a ogni chiamata (cap. 04).
        ''' </summary>
        ''' <param name="prompt">Il prompt caricato dalla libreria.</param>
        ''' <param name="testoRiempito">Il suo corpo coi segnaposto già sostituiti.</param>
        Public Function ChiediAsync(prompt As Prompt, testoRiempito As String,
                                    Optional annulla As CancellationToken = Nothing) As Task(Of RispostaAi)

            If prompt Is Nothing Then Throw New ArgumentNullException(NameOf(prompt))

            Return ChiediAsync(prompt.Modello, JsonValue.Create(testoRiempito), prompt.MaxToken, annulla)

        End Function

        ''' <summary>
        ''' Chiede all'AI e consegna la risposta <b>man mano che arriva</b>: ogni pezzo di
        ''' testo passa per <paramref name="pezzo"/> appena il flusso lo porta, e alla
        ''' fine si restituisce la risposta intera come in una chiamata normale.
        ''' </summary>
        ''' <remarks>
        ''' Serve dove c'è qualcosa da guardare mentre l'AI scrive, cioè il brainstorming
        ''' (cap. 02.5): sulla generazione, che produce JSON, un tracciato che si srotola
        ''' non direbbe niente a nessuno.
        ''' </remarks>
        ''' <param name="livello">Il livello dichiarato dal prompt: "semplice" o "ragionamento".</param>
        ''' <param name="turni">La conversazione, dal più vecchio al più recente.</param>
        ''' <param name="maxToken">Il limite di token della risposta, dai metadati del prompt.</param>
        ''' <param name="pezzo">Dove consegnare ogni pezzo di testo appena arriva.</param>
        ''' <param name="annulla">Il gettone di chi interrompe.</param>
        Public Async Function ChiediInStreamingAsync(livello As String, turni As IReadOnlyList(Of TurnoChat),
                                                     maxToken As Integer, pezzo As Action(Of String),
                                                     Optional annulla As CancellationToken = Nothing) As Task(Of RispostaAi)

            Dim modello As ModelloConcreto = ModelliInUso.PerLivello(livello)
            Dim testoCorpo As String = CorpoRichiesta(modello, maxToken, turni, flusso:=True).ToJsonString()

            ' Quanti pezzi sono già comparsi sotto gli occhi di chi legge: da questo
            ' dipende se un inciampo si può ancora ritentare in silenzio.
            Dim arrivati As Integer = 0
            Dim consegna As Action(Of String) =
                Sub(testo)
                    arrivati += 1
                    pezzo?.Invoke(testo)
                End Sub

            Dim primoErrore As ErroreAi = Nothing

            For tentativo As Integer = 1 To 2

                If tentativo > 1 Then
                    Await Task.Delay(QuantoAspettare(primoErrore), annulla).ConfigureAwait(False)
                End If

                Try
                    Return Await UnFlussoAsync(testoCorpo, modello.Id, consegna, annulla).ConfigureAwait(False)

                    ' Il ritentativo automatico vale finché non è comparso niente. Dopo il
                    ' primo pezzo di testo riprovare vorrebbe dire o scrivere due volte la
                    ' risposta, o cancellare sotto gli occhi dell'utente qualcosa che stava
                    ' leggendo: meglio l'errore com'è, accanto a quel che era arrivato.
                    ' *(Deciso a T7c.)*
                Catch ex As ErroreAi When ex.Ritentabile AndAlso tentativo = 1 AndAlso arrivati = 0
                    primoErrore = ex
                End Try

            Next

            Throw primoErrore

        End Function

        ''' <summary>
        ''' Come sopra, ma con livello e limite di token presi dai metadati del prompt
        ''' (cap. 04). Il primo turno è il prompt già riempito; quelli dopo sono la
        ''' conversazione che ci è cresciuta sopra.
        ''' </summary>
        Public Function ChiediInStreamingAsync(prompt As Prompt, turni As IReadOnlyList(Of TurnoChat),
                                               pezzo As Action(Of String),
                                               Optional annulla As CancellationToken = Nothing) As Task(Of RispostaAi)

            If prompt Is Nothing Then Throw New ArgumentNullException(NameOf(prompt))

            Return ChiediInStreamingAsync(prompt.Modello, turni, prompt.MaxToken, pezzo, annulla)

        End Function

        ''' <summary>Un singolo tentativo di flusso: prepara, manda, legge man mano.</summary>
        Private Async Function UnFlussoAsync(testoCorpo As String, modello As String,
                                             consegna As Action(Of String),
                                             annulla As CancellationToken) As Task(Of RispostaAi)

            Using richiesta As New HttpRequestMessage(HttpMethod.Post, Indirizzo)

                richiesta.Content = New StringContent(testoCorpo, New UTF8Encoding(False), "application/json")
                richiesta.Headers.Add("x-api-key", _chiave)
                richiesta.Headers.Add("anthropic-version", VersioneApi)

                Using scadenza As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(annulla)

                    ' L'attesa si riarma a ogni pezzo che arriva (v. LeggiIlFlussoAsync):
                    ' quello che si sorveglia è il silenzio, non la durata.
                    scadenza.CancelAfter(SilenzioMassimo)

                    Dim risposta As HttpResponseMessage
                    Try
                        risposta = Await _http.SendAsync(richiesta, HttpCompletionOption.ResponseHeadersRead,
                                                         scadenza.Token).ConfigureAwait(False)
                    Catch ex As OperationCanceledException When Not annulla.IsCancellationRequested
                        Throw ErroreDiAttesa(SilenzioMassimo, ex)
                    Catch ex As HttpRequestException
                        Throw ErroreDiRete(ex)
                    End Try

                    Using risposta

                        If Not risposta.IsSuccessStatusCode Then
                            ' Un errore non arriva mai in forma di eventi: è una risposta
                            ' normale, e si legge e si classifica come tutte le altre.
                            Dim corpo As String
                            Try
                                corpo = Await risposta.Content.
                                    ReadAsStringAsync(scadenza.Token).ConfigureAwait(False)
                            Catch ex As IOException
                                Throw ErroreDiRete(ex)
                            End Try
                            Throw ErroreDaStato(risposta, corpo, modello)
                        End If

                        Return Await LeggiIlFlussoAsync(risposta, consegna, scadenza, annulla).ConfigureAwait(False)

                    End Using

                End Using
            End Using

        End Function

        ''' <summary>
        ''' Srotola il flusso: ogni evento è un pezzo di risposta, un dato di servizio o
        ''' la fine. Il testo si accumula e insieme si consegna, così chi guarda lo vede
        ''' comparire e chi chiama alla fine ha comunque la risposta intera.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>La protezione della rete qui è scritta apposta.</b> Nella chiamata
        ''' sincrona il corpo scende tutto dentro <c>SendAsync</c> e una caduta a metà
        ''' lettura è coperta di riflesso (v. <see cref="UnTentativoAsync"/>); leggendo a
        ''' pezzi il collegamento può spezzarsi proprio qui, ed è per questo che
        ''' <c>ResponseHeadersRead</c> vive solo su questa strada.</para>
        ''' <para><b>Una risposta troncata non è un errore, qui.</b> Nella chiamata
        ''' sincrona il troncamento fa fallire tutto, perché quello che resta è un JSON
        ''' monco da dare in pasto a un estrattore. In una conversazione invece il testo
        ''' arrivato è già sotto gli occhi dell'utente e si legge benissimo: il motivo
        ''' della fine si porta a casa in <see cref="RispostaAi.MotivoFine"/> e a dirlo è
        ''' il pannello. *(Deciso a T7c, conseguenza della regola sul ritentativo.)*</para>
        ''' </remarks>
        Private Async Function LeggiIlFlussoAsync(risposta As HttpResponseMessage, consegna As Action(Of String),
                                                  scadenza As CancellationTokenSource,
                                                  annulla As CancellationToken) As Task(Of RispostaAi)

            Dim testo As New StringBuilder()
            Dim esito As New RispostaAi()
            Dim finito As Boolean = False

            Try
                Using flusso As Stream = Await risposta.Content.
                    ReadAsStreamAsync(scadenza.Token).ConfigureAwait(False)

                    Using lettore As New LettoreSse(flusso)

                        Dim evento As EventoSse = Await lettore.ProssimoAsync(scadenza.Token).ConfigureAwait(False)

                        While evento IsNot Nothing

                            ' Ogni evento è un segno di vita: l'attesa riparte da capo.
                            ' Vale anche per i ping, che è ciò per cui esistono. Ne segue
                            ' che un server che pingasse in eterno senza mai finire terrebbe
                            ' aperta la conversazione: a chiuderla resta l'utente, che qui
                            ' il pulsante per interrompere ce l'ha (cap. 02.6).
                            scadenza.CancelAfter(SilenzioMassimo)

                            If InterpretaEvento(evento, testo, esito, consegna) Then
                                finito = True
                                Exit While
                            End If

                            evento = Await lettore.ProssimoAsync(scadenza.Token).ConfigureAwait(False)

                        End While

                    End Using
                End Using

            Catch ex As OperationCanceledException When Not annulla.IsCancellationRequested
                Throw ErroreDiAttesa(SilenzioMassimo, ex)
            Catch ex As IOException
                Throw ErroreDiRete(ex)
            Catch ex As HttpRequestException
                Throw ErroreDiRete(ex)
            End Try

            ' Il flusso è finito senza che l'AI dicesse «ho finito»: il collegamento si è
            ' spezzato mentre scriveva.
            If Not finito Then
                Throw New ErroreAi(CausaErroreAi.Rete,
                    "Il collegamento con l'AI si è interrotto mentre stava scrivendo.")
            End If

            esito.Testo = testo.ToString()

            ' Un rifiuto non porta testo: lì l'errore è tutta la risposta.
            If String.Equals(esito.MotivoFine, "refusal", StringComparison.Ordinal) AndAlso
               esito.Testo.Length = 0 Then
                Throw New ErroreAi(CausaErroreAi.Rifiuto,
                    "Il modello si è rifiutato di rispondere a questa richiesta.")
            End If

            Return esito

        End Function

        ''' <summary>
        ''' Cosa fare di un evento. Restituisce <c>True</c> quando l'AI ha dichiarato di
        ''' aver finito, e allora il flusso si chiude.
        ''' </summary>
        Private Shared Function InterpretaEvento(evento As EventoSse, testo As StringBuilder,
                                                 esito As RispostaAi, consegna As Action(Of String)) As Boolean

            ' Il «ping» tiene viva la connessione e non porta niente da leggere.
            If String.Equals(evento.Nome, "ping", StringComparison.Ordinal) OrElse
               String.IsNullOrEmpty(evento.Dati) Then Return False

            Dim dati As JsonObject
            Try
                dati = TryCast(JsonNode.Parse(evento.Dati), JsonObject)
            Catch ex As JsonException
                Throw New ErroreAi(CausaErroreAi.RispostaInattesa,
                    "L'AI ha mandato un evento che non è JSON.", ex)
            End Try

            If dati Is Nothing Then Return False

            Select Case evento.Nome

                Case "error"
                    Throw ErroreDaEvento(dati)

                Case "message_start"
                    Dim messaggio As JsonObject = TryCast(dati("message"), JsonObject)
                    If messaggio IsNot Nothing Then
                        esito.Modello = TryCast(messaggio("model"), JsonValue)?.ToString()
                        esito.TokenIngresso = Intero(TryCast(messaggio("usage"), JsonObject), "input_tokens")
                    End If

                Case "content_block_delta"
                    Dim delta As JsonObject = TryCast(dati("delta"), JsonObject)
                    If delta Is Nothing Then Return False

                    ' Col ragionamento acceso arrivano anche i pezzi del ragionamento:
                    ' non sono la risposta e non si mostrano — lo stesso criterio con cui
                    ' PrimoTesto salta quel blocco nella chiamata sincrona.
                    If Not String.Equals(TryCast(delta("type"), JsonValue)?.ToString(), "text_delta",
                                         StringComparison.Ordinal) Then Return False

                    Dim scritto As String = TryCast(delta("text"), JsonValue)?.ToString()
                    If String.IsNullOrEmpty(scritto) Then Return False

                    testo.Append(scritto)
                    consegna(scritto)

                Case "message_delta"
                    Dim delta As JsonObject = TryCast(dati("delta"), JsonObject)
                    If delta IsNot Nothing Then
                        Dim motivo As String = TryCast(delta("stop_reason"), JsonValue)?.ToString()
                        If Not String.IsNullOrEmpty(motivo) Then esito.MotivoFine = motivo
                    End If
                    ' I token in uscita si sanno solo alla fine: l'API li conta qui. Si
                    ' scrivono solo se il conteggio c'è davvero: un secondo message_delta
                    ' senza «usage» azzererebbe un numero già buono.
                    Dim conteggio As JsonObject = TryCast(dati("usage"), JsonObject)
                    If conteggio IsNot Nothing Then
                        esito.TokenUscita = Intero(conteggio, "output_tokens")
                    End If

                Case "message_stop"
                    Return True

            End Select

            Return False

        End Function

        ''' <summary>
        ''' Traduce un errore arrivato <b>dentro</b> il flusso. Ha le stesse cause di
        ''' quelli che arrivano come stato HTTP, perché per chi legge sono la stessa cosa:
        ''' cambia solo il momento in cui l'API se ne accorge.
        ''' </summary>
        Private Shared Function ErroreDaEvento(dati As JsonObject) As ErroreAi

            Dim errore As JsonObject = TryCast(dati("error"), JsonObject)
            Dim tipo As String = If(TryCast(errore?("type"), JsonValue)?.ToString(), String.Empty)
            Dim dettaglio As String = Sintesi(If(TryCast(errore?("message"), JsonValue)?.ToString(), String.Empty))

            Select Case tipo

                Case "overloaded_error", "api_error"
                    Return New ErroreAi(CausaErroreAi.Servizio,
                        $"L'AI ha un problema temporaneo.{dettaglio}")

                Case "rate_limit_error"
                    Return New ErroreAi(CausaErroreAi.Limite,
                        $"Troppe richieste all'AI in poco tempo.{dettaglio}")

                Case "authentication_error", "permission_error"
                    Return New ErroreAi(CausaErroreAi.Chiave,
                        $"L'AI ha rifiutato la chiave API.{dettaglio}")

                Case "invalid_request_error"
                    Return New ErroreAi(CausaErroreAi.Richiesta,
                        $"L'AI ha rifiutato la richiesta.{dettaglio}")

                Case "not_found_error"
                    ' Di qui non ci si passa quasi mai — un modello che non esiste si
                    ' scopre prima che il flusso cominci — ma se ci si passa la cura è
                    ' la stessa, e dirla diversamente sarebbe dire due cose per una.
                    Return New ErroreAi(CausaErroreAi.ModelloRitirato,
                        SpiegaIlModello(Nothing) & dettaglio)

            End Select

            Return New ErroreAi(CausaErroreAi.Servizio,
                $"L'AI ha interrotto la risposta con un errore.{dettaglio}")

        End Function

        ''' <summary>Un singolo tentativo: prepara, manda, classifica.</summary>
        ''' <param name="attesa">Il tempo concesso a questa chiamata (v. <see cref="AttesaPer"/>).</param>
        Private Async Function UnTentativoAsync(testoCorpo As String, modello As String,
                                                attesa As TimeSpan,
                                                annulla As CancellationToken) As Task(Of RispostaAi)

            Using richiesta As New HttpRequestMessage(HttpMethod.Post, Indirizzo)

                richiesta.Content = New StringContent(testoCorpo, New UTF8Encoding(False), "application/json")
                richiesta.Headers.Add("x-api-key", _chiave)
                richiesta.Headers.Add("anthropic-version", VersioneApi)

                Using scadenza As CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(annulla)

                    scadenza.CancelAfter(attesa)

                    Dim risposta As HttpResponseMessage
                    Try
                        risposta = Await _http.SendAsync(richiesta, scadenza.Token).ConfigureAwait(False)
                    Catch ex As OperationCanceledException When Not annulla.IsCancellationRequested
                        Throw ErroreDiAttesa(attesa, ex)
                    Catch ex As HttpRequestException
                        Throw ErroreDiRete(ex)
                    End Try

                    Using risposta

                        ' Qui non serve una seconda protezione: <see cref="HttpClient"/>
                        ' lavora con l'impostazione predefinita (ResponseContentRead) e
                        ' scarica tutto il corpo dentro SendAsync, dove il Catch qui
                        ' sopra copre già la connessione che cade a metà. Questa lettura
                        ' attinge a un buffer in memoria. Se un giorno si passasse a
                        ' ResponseHeadersRead — è la strada dello streaming, previsto per
                        ' T4/T7 — il collegamento potrebbe spezzarsi proprio qui, e la
                        ' protezione andrà aggiunta: a ricordarlo c'è
                        ' UnaRispostaCheSiSpezzaInLetturaEUnaCadutaDiRete, che diventa
                        ' rosso appena quella riga cambia.
                        ' *T7c: quel giorno è arrivato, ma su un'altra strada.* Lo
                        ' streaming ha il suo ResponseHeadersRead e la sua protezione
                        ' scritta apposta (LeggiIlFlussoAsync); qui non è cambiato nulla,
                        ' e il collaudo sentinella continua a sorvegliare questa riga.
                        Dim testo As String = Await risposta.Content.
                            ReadAsStringAsync(scadenza.Token).ConfigureAwait(False)

                        If Not risposta.IsSuccessStatusCode Then
                            Throw ErroreDaStato(risposta, testo, modello)
                        End If

                        Return InterpretaRisposta(testo)

                    End Using

                End Using
            End Using

        End Function

        ''' <summary>
        ''' L'attesa è scaduta: il gettone dell'utente non è stato tirato, quindi non è
        ''' un annullamento ma un timeout. Le parole stanno in un posto solo perché
        ''' l'attesa può scadere in due momenti — mandando la richiesta e leggendo la
        ''' risposta — e chi legge deve vedere sempre la stessa frase.
        ''' </summary>
        Private Shared Function ErroreDiAttesa(attesa As TimeSpan, ex As Exception) As ErroreAi

            Return New ErroreAi(CausaErroreAi.Timeout,
                $"L'AI non ha risposto entro {CInt(attesa.TotalSeconds)} secondi.", ex)

        End Function

        ''' <summary>Il collegamento non ha retto, in andata o in ritorno.</summary>
        Private Shared Function ErroreDiRete(ex As Exception) As ErroreAi

            Return New ErroreAi(CausaErroreAi.Rete,
                "Non riesco a raggiungere l'AI: controlla la connessione a Internet.", ex)

        End Function

        ''' <summary>Traduce uno stato HTTP di errore in un errore leggibile.</summary>
        Private Shared Function ErroreDaStato(risposta As HttpResponseMessage, corpo As String,
                                              modello As String) As ErroreAi

            Dim stato As Integer = CInt(risposta.StatusCode)
            Dim dettaglio As String = Sintesi(corpo)

            If ParlaDiUnModelloCheNonCE(stato, corpo) Then
                Return New ErroreAi(CausaErroreAi.ModelloRitirato, SpiegaIlModello(modello) & dettaglio)
            End If

            If stato = 401 OrElse stato = 403 Then
                Return New ErroreAi(CausaErroreAi.Chiave,
                    $"L'AI ha rifiutato la chiave API (HTTP {stato}).{dettaglio}")
            End If

            If stato = 429 Then
                Return New ErroreAi(CausaErroreAi.Limite,
                    $"Troppe richieste all'AI in poco tempo (HTTP 429).{dettaglio}",
                    Nothing, AttesaConsigliata(risposta))
            End If

            If stato >= 500 Then
                ' Anche sui guasti temporanei (529 «overloaded» compreso) l'API può
                ' suggerire quanto aspettare: ignorarlo farebbe ritentare contro lo
                ' stesso muro dopo la pausa fissa.
                Return New ErroreAi(CausaErroreAi.Servizio,
                    $"L'AI ha un problema temporaneo (HTTP {stato}).{dettaglio}",
                    Nothing, AttesaConsigliata(risposta))
            End If

            Return New ErroreAi(CausaErroreAi.Richiesta,
                $"L'AI ha rifiutato la richiesta (HTTP {stato}).{dettaglio}")

        End Function

        ''' <summary>
        ''' Se questa risposta d'errore parla di un modello che non c'è.
        ''' </summary>
        ''' <remarks>
        ''' Due segni, e basta uno. Il primo è il <b>404</b>: l'indirizzo a cui si scrive è
        ''' una costante di questa classe e non è mai cambiato, quindi «non trovato» non
        ''' può riferirsi alla porta — l'unica cosa nominata nella richiesta che possa non
        ''' esistere è il modello. Il secondo è il tipo dichiarato dall'API,
        ''' <c>not_found_error</c>, che vale anche se un giorno arrivasse con uno stato
        ''' diverso.
        ''' </remarks>
        Private Shared Function ParlaDiUnModelloCheNonCE(stato As Integer, corpo As String) As Boolean

            If stato = 404 Then Return True

            Return If(corpo, String.Empty).Contains("not_found_error", StringComparison.OrdinalIgnoreCase)

        End Function

        ''' <summary>
        ''' La riga da mostrare quando il modello non c'è più: cos'è successo, e la sola
        ''' strada per uscirne.
        ''' </summary>
        ''' <remarks>
        ''' Fino alla 1.0 questo caso finiva nel mucchio dei «rifiutata la richiesta», e chi
        ''' lo incontrava non aveva modo di sapere che la cura era a due clic di distanza: i
        ''' modelli si ritirano dal listino con l'unico preavviso di una data su una pagina
        ''' web, e il programma se ne accorge il giorno in cui smette di funzionare.
        ''' </remarks>
        Private Shared Function SpiegaIlModello(modello As String) As String

            Dim quale As String = If(String.IsNullOrWhiteSpace(modello), "richiesto", $"«{modello}»")

            Return $"Il modello {quale} non è più disponibile, oppure questa chiave API non " &
                   "può usarlo. Scegline un altro in Impostazioni, sotto «Sotto il cofano»."

        End Function

        ''' <summary>Quanto aspettare prima del ritentativo, entro un tetto ragionevole.</summary>
        Private Function QuantoAspettare(errore As ErroreAi) As TimeSpan

            Dim attesa As TimeSpan = Pausa
            If errore IsNot Nothing AndAlso errore.AttesaSuggerita.HasValue Then
                attesa = errore.AttesaSuggerita.Value
            End If

            ' Un'attesa suggerita spropositata bloccherebbe l'interfaccia più a lungo
            ' di quanto un utente sia disposto a restare fermo a guardare.
            If attesa > PausaMassima Then attesa = PausaMassima
            If attesa < TimeSpan.Zero Then attesa = TimeSpan.Zero

            Return attesa

        End Function

        ''' <summary>L'attesa suggerita dall'API con l'intestazione <c>Retry-After</c>.</summary>
        Private Shared Function AttesaConsigliata(risposta As HttpResponseMessage) As TimeSpan?

            Dim suggerimento = risposta.Headers.RetryAfter
            If suggerimento Is Nothing Then Return Nothing

            If suggerimento.Delta.HasValue Then Return suggerimento.Delta.Value

            If suggerimento.Date.HasValue Then
                Dim quanto As TimeSpan = suggerimento.Date.Value - DateTimeOffset.UtcNow
                If quanto > TimeSpan.Zero Then Return quanto
            End If

            Return Nothing

        End Function

        ''' <summary>
        ''' Il primo blocco di testo della risposta. Il prototipo prende il blocco in
        ''' posizione zero; qui si cerca per tipo, perché col ragionamento acceso il
        ''' primo blocco è il ragionamento e non la risposta. A ragionamento spento i
        ''' due modi coincidono, quindi la non-regressione non ne risente.
        ''' </summary>
        Private Shared Function PrimoTesto(radice As JsonObject) As String

            Dim blocchi As JsonArray = TryCast(radice("content"), JsonArray)
            If blocchi Is Nothing Then Return Nothing

            For Each blocco As JsonNode In blocchi
                Dim oggetto As JsonObject = TryCast(blocco, JsonObject)
                If oggetto Is Nothing Then Continue For
                If String.Equals(TryCast(oggetto("type"), JsonValue)?.ToString(), "text",
                                 StringComparison.Ordinal) Then
                    Return TryCast(oggetto("text"), JsonValue)?.ToString()
                End If
            Next

            Return Nothing

        End Function

        ''' <summary>Legge un intero, zero se assente o non numerico.</summary>
        Private Shared Function Intero(oggetto As JsonObject, chiave As String) As Integer

            If oggetto Is Nothing Then Return 0

            Dim valore As JsonValue = TryCast(oggetto(chiave), JsonValue)
            If valore Is Nothing Then Return 0

            Dim numero As Integer
            Return If(valore.TryGetValue(Of Integer)(numero), numero, 0)

        End Function

        ''' <summary>
        ''' Il corpo di un errore, ridotto a una riga leggibile: serve a capire cosa è
        ''' successo, non a riversare in faccia all'utente la risposta intera.
        ''' </summary>
        Private Shared Function Sintesi(corpo As String) As String

            If String.IsNullOrWhiteSpace(corpo) Then Return String.Empty

            Dim riga As String = corpo.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbTab, " ").Trim()
            If riga.Length > 300 Then riga = riga.Substring(0, 300) & "…"

            Return " " & riga

        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _smaltito Then Return
            _smaltito = True
            _http.Dispose()
            GC.SuppressFinalize(Me)
        End Sub

    End Class

End Namespace
