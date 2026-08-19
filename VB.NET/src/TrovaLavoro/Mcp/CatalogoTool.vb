Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Mcp

    ''' <summary>
    ''' Com'è andata una chiamata a un tool. Le due strade non sono la stessa cosa detta
    ''' in due modi: <see cref="Riuscito"/> porta dei dati, <see cref="NonRiuscito"/>
    ''' porta una frase — scritta per essere letta da un modello, che da lì può
    ''' correggere il tiro e riprovare (cap. 09.2).
    ''' </summary>
    Public Class EsitoTool

        Private Sub New()
        End Sub

        ''' <summary>Il JSON prodotto; <c>Nothing</c> se la chiamata non è riuscita.</summary>
        Public ReadOnly Property Dati As JsonNode

        ''' <summary>Perché non si è potuto fare; <c>Nothing</c> se è andata bene.</summary>
        Public ReadOnly Property Spiegazione As String

        ''' <summary>Se la chiamata non ha potuto fare il suo lavoro.</summary>
        Public ReadOnly Property Fallito As Boolean
            Get
                Return Spiegazione IsNot Nothing
            End Get
        End Property

        Public Shared Function Riuscito(dati As JsonNode) As EsitoTool
            Return New EsitoTool With {._Dati = dati}
        End Function

        Public Shared Function NonRiuscito(spiegazione As String) As EsitoTool
            Return New EsitoTool With {._Spiegazione = spiegazione}
        End Function

        ''' <summary>
        ''' L'esito nella forma che <c>tools/call</c> si aspetta. Il risultato viaggia due
        ''' volte — strutturato in <c>structuredContent</c> e serializzato in un blocco di
        ''' testo — perché è quel che la spec chiede per i client che sanno leggere solo
        ''' la seconda forma.
        ''' </summary>
        Public Function ComeRisultato() As JsonObject

            If Fallito Then
                Return New JsonObject From {
                    {"content", New JsonArray From {Blocco(Spiegazione)}},
                    {"isError", True}}
            End If

            Dim risultato As New JsonObject From {
                {"content", New JsonArray From {Blocco(ProtocolloMcp.Compatto(Dati))}},
                {"isError", False}}

            If Dati IsNot Nothing Then risultato("structuredContent") = Dati.DeepClone()

            Return risultato

        End Function

        Private Shared Function Blocco(testo As String) As JsonObject

            Return New JsonObject From {
                {"type", "text"},
                {"text", If(testo, String.Empty)}}

        End Function

    End Class

    ''' <summary>
    ''' Un tool come il client lo vede: il nome con cui si chiama, due righe per capire a
    ''' che serve, e lo schema di quel che vuole in ingresso.
    ''' </summary>
    Public Class DefinizioneTool

        Public Sub New(nome As String, titolo As String, descrizione As String, schema As JsonObject)

            _Nome = nome
            _Titolo = titolo
            _Descrizione = descrizione
            _Schema = schema

        End Sub

        Public ReadOnly Property Nome As String
        Public ReadOnly Property Titolo As String
        Public ReadOnly Property Descrizione As String

        ''' <summary>Lo schema JSON dei parametri: sempre un oggetto, mai nullo.</summary>
        Public ReadOnly Property Schema As JsonObject

        ''' <summary>
        ''' La definizione come va in <c>tools/list</c>. Lo schema si ricopia a ogni
        ''' giro: un nodo JSON non sta in due alberi, e la seconda richiesta di elenco
        ''' troverebbe il nostro già appeso alla prima.
        ''' </summary>
        Public Function ComeJson() As JsonObject

            Return New JsonObject From {
                {"name", Nome},
                {"title", Titolo},
                {"description", Descrizione},
                {"inputSchema", Schema.DeepClone()}}

        End Function

    End Class

    ''' <summary>
    ''' I tool che il server offre (cap. 09.3): la vetrina che risponde a
    ''' <c>tools/list</c> e lo smistamento di <c>tools/call</c>.
    ''' </summary>
    ''' <remarks>
    ''' <para>Ci sono i tre di sola lettura (T8a) e i sette che passano dall'AI (T8b).
    ''' Quelli che <b>scrivono</b> arrivano con T8c, insieme al lucchetto della cartella
    ''' dati (cap. 09.4): finché non ci sono, nessun tool cambia un file dell'utente, ed è
    ''' il motivo per cui il lucchetto può ancora aspettare.</para>
    ''' <para><b>I tool dell'AI restano in vetrina anche senza chiave</b>, e falliscono
    ''' dicendo perché. Nasconderli sarebbe peggio: il client tiene l'elenco da parte e
    ''' non gli abbiamo promesso nessun avviso di cambiamento, e un tool che sparisce si
    ''' annuncia come «Unknown tool» — che a un modello dice «hai sbagliato nome», cioè lo
    ''' manda a cercare l'errore dove non è.</para>
    ''' <para><b>L'ordine è quello di dichiarazione e non cambia</b>: la spec chiede un
    ''' elenco stabile, perché i client lo tengono da parte e i modelli lo si ritrova in
    ''' testa alla conversazione. I nuovi si aggiungono in fondo.</para>
    ''' </remarks>
    Public Class CatalogoTool

        Public Const LeggiProfilo As String = "leggi_profilo"
        Public Const LeggiRegistro As String = "leggi_registro"
        Public Const LeggiOpportunita As String = "leggi_opportunita"

        Public Const AnalizzaAnnuncio As String = "analizza_annuncio"
        Public Const Confronta As String = "confronta"
        Public Const Mitiga As String = "mitiga"
        Public Const StrutturaCv As String = "struttura_cv"
        Public Const GeneraCv As String = "genera_cv"
        Public Const GeneraLettera As String = "genera_lettera"
        Public Const RifinisciTesto As String = "rifinisci_testo"

        Public Const SalvaOpportunita As String = "salva_opportunita"
        Public Const EsportaDocumento As String = "esporta_documento"

        Private ReadOnly _lettura As ToolDiLettura
        Private ReadOnly _ai As ToolDiAi
        Private ReadOnly _scrittura As ToolDiScrittura
        Private ReadOnly _definizioni As New List(Of DefinizioneTool)

        Public Sub New(contesto As ContestoApp)

            _lettura = New ToolDiLettura(contesto)
            _ai = New ToolDiAi(contesto)
            _scrittura = New ToolDiScrittura(contesto)

            _definizioni.Add(New DefinizioneTool(
                LeggiProfilo, "Leggi il profilo",
                "Restituisce il profilo professionale salvato: dati anagrafici, esperienze, " &
                "studi, competenze, lingue. È la materia prima di ogni CV e di ogni confronto " &
                "con un annuncio.",
                SenzaParametri()))

            _definizioni.Add(New DefinizioneTool(
                LeggiRegistro, "Leggi il registro delle candidature",
                "Elenca tutte le opportunità con il loro stato, le stelle del confronto, il " &
                "destinatario e le date. Il campo «cartella» di ogni voce è il nome con cui " &
                "chiedere il dettaglio a leggi_opportunita.",
                SenzaParametri()))

            _definizioni.Add(New DefinizioneTool(
                LeggiOpportunita, "Leggi una candidatura",
                "Restituisce tutto quel che una candidatura ha prodotto: l'annuncio analizzato, " &
                "i giudizi del confronto, le mitigazioni, gli appunti di mira, il CV, la lettera, " &
                "la bozza dell'email, lo stato, e i nomi dei documenti già impaginati.",
                New JsonObject From {
                    {"type", "object"},
                    {"properties", New JsonObject From {
                        {"cartella", New JsonObject From {
                            {"type", "string"},
                            {"description", "Il nome della cartella dell'opportunità, come lo dà leggi_registro."}}}}},
                    {"required", New JsonArray From {"cartella"}},
                    {"additionalProperties", False}}))

            _definizioni.Add(New DefinizioneTool(
                AnalizzaAnnuncio, "Analizza un annuncio",
                "Legge il testo di un annuncio di lavoro e ne ricava la forma strutturata: " &
                "azienda, ruolo, requisiti (segnalando quali sono eliminatori), sede, contratto, " &
                "lingua. È il primo passo di ogni candidatura.",
                Schema(New JsonObject From {
                    {"testo", Testo("Il testo dell'annuncio, copiato dal portale.")}},
                    "testo")))

            _definizioni.Add(New DefinizioneTool(
                Confronta, "Confronta il profilo con un annuncio",
                "Mette il profilo salvato davanti a un annuncio e giudica requisito per requisito, " &
                "poi calcola il punteggio in stelle. Attenzione: le stelle le calcola il programma " &
                "dai giudizi, non il modello — non si possono negoziare a parole, e un requisito " &
                "eliminatorio non soddisfatto impone comunque un tetto.",
                Schema(New JsonObject From {
                    {"annuncio", Oggetto("L'annuncio strutturato, come lo restituisce analizza_annuncio.")}},
                    "annuncio")))

            _definizioni.Add(New DefinizioneTool(
                Mitiga, "Cerca le mitigazioni sui gap",
                "Dai giudizi di un confronto ricava gli argomenti onesti da spendere sui punti " &
                "deboli, usando solo fatti che il profilo dichiara. Una lista vuota è un esito " &
                "legittimo: se un gap non ha nessun ponte onesto, tace invece di inventarne uno.",
                Schema(New JsonObject From {
                    {"giudizi", Lista("La lista dei giudizi: sta nel campo «giudizi» del confronto.")}},
                    "giudizi")))

            _definizioni.Add(New DefinizioneTool(
                StrutturaCv, "Leggi un CV e proponi un profilo",
                "Prende il testo di un CV e ne ricava una proposta di profilo strutturato. " &
                "Non salva niente: sostituire il profilo è irreversibile e si fa " &
                "dall'applicazione, dove l'utente vede cosa sta accettando.",
                Schema(New JsonObject From {
                    {"testo", Testo("Il testo del CV, trascritto o incollato.")}},
                    "testo")))

            _definizioni.Add(New DefinizioneTool(
                GeneraCv, "Genera un CV",
                "Scrive il CV a partire dal profilo salvato. Con «annuncio» e «giudizi» esce il CV " &
                "mirato su quella posizione; senza, il CV base. In nessuno dei due casi aggiunge " &
                "esperienze che il profilo non dichiara: mette in risalto, non inventa. Il testo " &
                "esce già ripulito dalle formule vuote, come quello dell'applicazione.",
                Schema(New JsonObject From {
                    {"annuncio", Oggetto("L'annuncio strutturato. Ometterlo insieme a «giudizi» dà il CV base.")},
                    {"giudizi", Lista("La lista dei giudizi che dà confronta. Va insieme ad «annuncio».")},
                    {"appunti", Oggetto("Appunti su cosa mettere in risalto. Non aggiungono fatti.")},
                    {"lingua", Lingua()}})))

            _definizioni.Add(New DefinizioneTool(
                GeneraLettera, "Genera la lettera di presentazione",
                "Scrive la lettera di presentazione, coerente con il CV mirato già generato e " &
                "onesta sui punti deboli che le mitigazioni sanno nominare. Il testo esce già " &
                "ripulito dalle formule vuote.",
                Schema(New JsonObject From {
                    {"annuncio", Oggetto("L'annuncio strutturato.")},
                    {"giudizi", Lista("La lista dei giudizi che dà confronta.")},
                    {"cv", Oggetto("Il CV mirato già generato: la lettera deve raccontare la stessa storia.")},
                    {"mitigazioni", Oggetto("Le mitigazioni, se ci sono. Senza, la lettera tace sui gap.")},
                    {"appunti", Oggetto("Appunti su cosa mettere in risalto. Non aggiungono fatti.")},
                    {"lingua", Lingua()}},
                    "annuncio", "giudizi", "cv")))

            _definizioni.Add(New DefinizioneTool(
                RifinisciTesto, "Ripulisci un testo",
                "Passa un testo di prosa al setaccio delle formule vuote e dei tic di scrittura " &
                "automatica, senza cambiare i fatti che dice. Se non c'è niente da togliere, " &
                "restituisce il testo com'era.",
                Schema(New JsonObject From {
                    {"testo", Testo("Il testo di prosa da ripulire.")},
                    {"lingua", Lingua()}},
                    "testo")))

            _definizioni.Add(New DefinizioneTool(
                SalvaOpportunita, "Metti una candidatura nella coda",
                "Salva una candidatura nella cartella dati: l'annuncio e tutto quello che di " &
                "quella candidatura è già stato prodotto — confronto, mitigazioni, CV, lettera. " &
                "Il punteggio in stelle non si accetta da fuori: se arriva un confronto, lo " &
                "ricalcola il programma dai giudizi. Restituisce il nome della cartella, quello " &
                "con cui poi si chiede leggi_opportunita o esporta_documento.",
                Schema(New JsonObject From {
                    {"annuncio", Oggetto("L'annuncio strutturato, come lo dà analizza_annuncio.")},
                    {"confronto", Oggetto("Il confronto intero, come lo dà confronta nel campo «confronto».")},
                    {"mitigazioni", Oggetto("Le mitigazioni, come le dà mitiga.")},
                    {"appunti", Oggetto("Appunti di mira, se ce ne sono.")},
                    {"cv", Oggetto("Il CV generato, come lo dà genera_cv nel campo «cv».")},
                    {"lettera", Oggetto("La lettera generata, come la dà genera_lettera.")},
                    {"lingua", Lingua()}},
                    "annuncio")))

            _definizioni.Add(New DefinizioneTool(
                EsportaDocumento, "Impagina i documenti di una candidatura",
                "Scrive in formato DOCX il CV e la lettera di una candidatura già salvata, " &
                "nella sottocartella «out» di quella candidatura. Il PDF non si fa da qui: " &
                "richiede il browser incorporato dell'applicazione, e si esporta dalla finestra.",
                Schema(New JsonObject From {
                    {"cartella", Testo("Il nome della cartella dell'opportunità, come lo dà leggi_registro.")}},
                    "cartella")))

        End Sub

        ''' <summary>I tool, nell'ordine in cui vanno elencati.</summary>
        Public ReadOnly Property Definizioni As IReadOnlyList(Of DefinizioneTool)
            Get
                Return _definizioni
            End Get
        End Property

        ''' <summary>L'elenco pronto per <c>tools/list</c>.</summary>
        Public Overridable Function Elenco() As JsonArray

            Dim elencati As New JsonArray()

            For Each tool As DefinizioneTool In _definizioni
                elencati.Add(tool.ComeJson())
            Next

            Return elencati

        End Function

        ''' <summary>
        ''' Se il tool esiste. Chiamarne uno che non c'è è un errore <b>di protocollo</b>,
        ''' non un tool che non ce la fa: chi chiama ha sbagliato la richiesta, non i
        ''' parametri.
        ''' </summary>
        Public Overridable Function Conosce(nome As String) As Boolean

            If nome Is Nothing Then Return False

            For Each tool As DefinizioneTool In _definizioni
                If tool.Nome.Equals(nome, StringComparison.Ordinal) Then Return True
            Next

            Return False

        End Function

        ''' <summary>
        ''' Esegue il tool. Chi chiama ha già verificato che esista con
        ''' <see cref="Conosce"/>.
        ''' </summary>
        ''' <param name="annulla">
        ''' Il gettone con cui il client ritira la richiesta (cap. 09.2). I tre tool di
        ''' lettura non lo guardano — leggono un file e hanno finito prima che serva —
        ''' ma quelli che passano dall'AI sì, ed è il motivo per cui la firma lo porta.
        ''' </param>
        Public Overridable Async Function EseguiAsync(nome As String, argomenti As JsonObject,
                                                     Optional annulla As CancellationToken = Nothing) _
                                                     As Task(Of EsitoTool)

            Select Case nome

                Case LeggiProfilo
                    Return _lettura.LeggiProfilo()

                Case LeggiRegistro
                    Return _lettura.LeggiRegistro()

                Case LeggiOpportunita
                    Return _lettura.LeggiOpportunita(CampiJson.Testo(argomenti, "cartella"))

                Case AnalizzaAnnuncio
                    Return Await _ai.AnalizzaAnnuncio(argomenti, annulla).ConfigureAwait(False)

                Case Confronta
                    Return Await _ai.Confronta(argomenti, annulla).ConfigureAwait(False)

                Case Mitiga
                    Return Await _ai.Mitiga(argomenti, annulla).ConfigureAwait(False)

                Case StrutturaCv
                    Return Await _ai.StrutturaCv(argomenti, annulla).ConfigureAwait(False)

                Case GeneraCv
                    Return Await _ai.GeneraCv(argomenti, annulla).ConfigureAwait(False)

                Case GeneraLettera
                    Return Await _ai.GeneraLettera(argomenti, annulla).ConfigureAwait(False)

                Case RifinisciTesto
                    Return Await _ai.RifinisciTesto(argomenti, annulla).ConfigureAwait(False)

                Case SalvaOpportunita
                    Return Await _scrittura.SalvaOpportunita(argomenti, annulla).ConfigureAwait(False)

                Case EsportaDocumento
                    Return Await _scrittura.EsportaDocumento(argomenti, annulla).ConfigureAwait(False)

                Case Else
                    ' Non ci si arriva passando da Conosce: se ci si arriva, è perché un
                    ' tool è stato dichiarato e non collegato, ed è meglio dirlo che
                    ' rispondere il vuoto.
                    Return EsitoTool.NonRiuscito($"Il tool «{nome}» è dichiarato ma non fa ancora niente.")

            End Select

        End Function

        ''' <summary>
        ''' Lo schema di un tool che non vuole niente in ingresso: «un oggetto, e niente
        ''' dentro». Uno schema ci vuole comunque — un tool senza parametri non è un tool
        ''' senza schema — e dichiararlo chiuso dice al client che non c'è nient'altro da
        ''' cercare.
        ''' </summary>
        Private Shared Function SenzaParametri() As JsonObject

            Return New JsonObject From {
                {"type", "object"},
                {"additionalProperties", False}}

        End Function

        ''' <summary>
        ''' Lo schema di un tool con dei parametri. Si dichiara <b>chiuso</b> come quello
        ''' senza: un campo in più che nessuno guarda è un malinteso che resta lì, e dirlo
        ''' subito costa una riga.
        ''' </summary>
        ''' <param name="proprieta">I parametri, con la loro descrizione.</param>
        ''' <param name="obbligatori">Quali non si possono omettere; nessuno, se non se ne passa.</param>
        Private Shared Function Schema(proprieta As JsonObject, ParamArray obbligatori() As String) As JsonObject

            Dim scritto As New JsonObject From {
                {"type", "object"},
                {"properties", proprieta},
                {"additionalProperties", False}}

            If obbligatori IsNot Nothing AndAlso obbligatori.Length > 0 Then

                Dim elenco As New JsonArray()
                For Each nome As String In obbligatori
                    elenco.Add(nome)
                Next

                scritto("required") = elenco

            End If

            Return scritto

        End Function

        Private Shared Function Testo(descrizione As String) As JsonObject
            Return New JsonObject From {{"type", "string"}, {"description", descrizione}}
        End Function

        Private Shared Function Oggetto(descrizione As String) As JsonObject
            Return New JsonObject From {{"type", "object"}, {"description", descrizione}}
        End Function

        Private Shared Function Lista(descrizione As String) As JsonObject
            Return New JsonObject From {{"type", "array"}, {"description", descrizione}}
        End Function

        ''' <summary>
        ''' La lingua di un documento. Le due che il pool sa scrivere si dichiarano
        ''' nell'elenco: un client che chiedesse il francese si sentirebbe rispondere in
        ''' inglese, e vale la pena che lo sappia prima invece di scoprirlo dal risultato.
        ''' </summary>
        Private Shared Function Lingua() As JsonObject

            Return New JsonObject From {
                {"type", "string"},
                {"enum", New JsonArray From {"it", "en"}},
                {"description", "La lingua del documento. Se si omette, italiano."}}

        End Function

    End Class

End Namespace
