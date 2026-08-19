Imports System.Text.Json.Nodes
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
    ''' <para>A T8a ci sono i tre di sola lettura. I tool che passano dall'AI arrivano con
    ''' T8b e quelli che scrivono con T8c, insieme al lucchetto della cartella dati
    ''' (cap. 09.4): finché non ci sono, il server non ha modo di toccare niente, ed è il
    ''' motivo per cui questa prima tappa può fare a meno del lucchetto senza rischiare
    ''' nulla.</para>
    ''' <para><b>L'ordine è quello di dichiarazione e non cambia</b>: la spec chiede un
    ''' elenco stabile, perché i client lo tengono da parte e i modelli lo si ritrova in
    ''' testa alla conversazione.</para>
    ''' </remarks>
    Public Class CatalogoTool

        Public Const LeggiProfilo As String = "leggi_profilo"
        Public Const LeggiRegistro As String = "leggi_registro"
        Public Const LeggiOpportunita As String = "leggi_opportunita"

        Private ReadOnly _lettura As ToolDiLettura
        Private ReadOnly _definizioni As New List(Of DefinizioneTool)

        Public Sub New(contesto As ContestoApp)

            _lettura = New ToolDiLettura(contesto)

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

        End Sub

        ''' <summary>I tool, nell'ordine in cui vanno elencati.</summary>
        Public ReadOnly Property Definizioni As IReadOnlyList(Of DefinizioneTool)
            Get
                Return _definizioni
            End Get
        End Property

        ''' <summary>L'elenco pronto per <c>tools/list</c>.</summary>
        Public Function Elenco() As JsonArray

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
        Public Function Conosce(nome As String) As Boolean

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
        Public Function Esegui(nome As String, argomenti As JsonObject) As EsitoTool

            Select Case nome

                Case LeggiProfilo
                    Return _lettura.LeggiProfilo()

                Case LeggiRegistro
                    Return _lettura.LeggiRegistro()

                Case LeggiOpportunita
                    Return _lettura.LeggiOpportunita(CampiJson.Testo(argomenti, "cartella"))

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

    End Class

End Namespace
