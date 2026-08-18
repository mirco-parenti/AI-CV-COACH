Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Dati

Namespace Ai

    ''' <summary>
    ''' Il genere di prosa da rifinire (cap. 08.3). Non è una sfumatura: sono tre forme
    ''' diverse, e a ognuna corrisponde un prompt diverso nel pool.
    ''' </summary>
    ''' <remarks>
    ''' Un prompt solo che le contenesse tutte e tre lascerebbe al modello una scelta di
    ''' forma da fare, ed è esattamente il punto in cui il Pool 1.05 e il 1.07 hanno
    ''' sbagliato: fra una regola generale e una forma concreta da imitare, vince la forma.
    ''' Qui ogni prompt ne contiene una sola, così non c'è niente da scegliere.
    ''' </remarks>
    Public Enum GenereProsa

        ''' <summary>Il sommario di un CV: poche frasi in prima persona.</summary>
        Sintesi = 0

        ''' <summary>Le descrizioni delle esperienze: frasi nominali, una riga l'una.</summary>
        Frasi = 1

        ''' <summary>Il corpo di una lettera o di un'email: prosa distesa.</summary>
        Prosa = 2

    End Enum

    ''' <summary>Un pezzo di prosa da rifinire: l'etichetta con cui tornerà, e il testo.</summary>
    ''' <remarks>
    ''' L'<see cref="Id"/> non lo legge il modello per capirci qualcosa — gli si chiede
    ''' solo di ricopiarlo identico. Serve a noi per rimettere ogni testo al posto da cui
    ''' l'abbiamo preso, senza fidarci dell'ordine in cui torna.
    ''' </remarks>
    Public Class PezzoDiProsa

        Public Property Id As String
        Public Property Testo As String

    End Class

    ''' <summary>
    ''' Chi rifinisce la prosa già scritta: la passata anti-slop del cap. 08. Cambia come
    ''' un testo suona, mai che cosa dice.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Riceve solo testo.</b> Niente profilo, niente annuncio, niente giudizi: un
    ''' prompt che non ha fonti non può aggiungere fatti. L'anti-invenzione, qui, prima
    ''' ancora che scritta nel prompt è garantita da ciò che nella richiesta non entra —
    ''' come i campi-fatto del CV, che restano fuori e perciò non si possono toccare.</para>
    ''' <para><b>Non perde mai un testo.</b> Il dizionario che torna contiene una voce per
    ''' ogni pezzo chiesto: quella rifinita se è arrivata utilizzabile, altrimenti il testo
    ''' di partenza. Un'AI che dimentica un pezzo, ne inventa uno o lo restituisce vuoto
    ''' non deve poter cancellare una riga del CV di qualcuno.</para>
    ''' </remarks>
    Public Interface IRifinitore

        ''' <summary>
        ''' Rifinisce i pezzi dati, tutti dello stesso genere, in una chiamata sola.
        ''' </summary>
        ''' <param name="pezzi">I testi da rifinire; quelli vuoti si scartano da soli.</param>
        ''' <param name="genere">Quale forma tenere, cioè quale prompt del pool usare.</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        ''' <param name="lingua">La lingua del testo: <c>it</c> o <c>en</c> (cap. 10).</param>
        ''' <returns>Per ogni id chiesto, il testo da usare adesso.</returns>
        Function RifinisciAsync(pezzi As IEnumerable(Of PezzoDiProsa), genere As GenereProsa,
                                Optional annulla As CancellationToken = Nothing,
                                Optional lingua As String = "it") _
                                As Task(Of IReadOnlyDictionary(Of String, String))

    End Interface

    ''' <summary>
    ''' Il rifinitore vero: come gli altri mestieri non ha logica sua — è la fila comune di
    ''' <see cref="MestiereAi"/> con addosso il suo segnaposto e i suoi nomi.
    ''' </summary>
    Public Class Rifinitore
        Inherits MestiereAi
        Implements IRifinitore

        ''' <summary>Gli identificativi dei tre prompt nel pool.</summary>
        Public Const IdPromptSintesi As String = "umanizzazione_sintesi"
        Public Const IdPromptFrasi As String = "umanizzazione_frasi"
        Public Const IdPromptProsa As String = "umanizzazione_prosa"

        ''' <summary>Il segnaposto che i tre prompt dichiarano (cap. 04.3).</summary>
        Public Const SegnapostoPezzi As String = "PEZZI"

        ''' <param name="libreria">Il pool da cui vengono i prompt.</param>
        ''' <param name="client">Il client dell'AI, già con la sua chiave.</param>
        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)
            MyBase.New(libreria, client)
        End Sub

        ''' <inheritdoc/>
        Public Async Function RifinisciAsync(pezzi As IEnumerable(Of PezzoDiProsa), genere As GenereProsa,
                                             Optional annulla As CancellationToken = Nothing,
                                             Optional lingua As String = "it") _
                                             As Task(Of IReadOnlyDictionary(Of String, String)) _
                                             Implements IRifinitore.RifinisciAsync

            Dim daFare As List(Of PezzoDiProsa) = If(pezzi, Enumerable.Empty(Of PezzoDiProsa)()).
                Where(Function(p) p IsNot Nothing AndAlso
                                  Not String.IsNullOrWhiteSpace(p.Id) AndAlso
                                  Not String.IsNullOrWhiteSpace(p.Testo)).
                ToList()

            ' Un documento senza prosa da rifinire esiste — un profilo scarno, un CV senza
            ' esperienze descritte — e non è un caso da gestire: è una chiamata all'AI da
            ' non fare, cioè un'attesa e dei token risparmiati per sapere una cosa che si
            ' sa già.
            If daFare.Count = 0 Then
                Return New Dictionary(Of String, String)()
            End If

            ' Si parte da com'era: quel che l'AI riporterà bene sovrascriverà, il resto
            ' resta il testo di partenza. È il ripiego promesso dall'interfaccia, ed è
            ' anche il motivo per cui non c'è nessun caso «pezzo mancante» da trattare
            ' dopo.
            Dim esito As New Dictionary(Of String, String)(StringComparer.Ordinal)
            For Each pezzo As PezzoDiProsa In daFare
                esito(pezzo.Id) = pezzo.Testo
            Next

            Dim risposta As JsonNode = Await EseguiAsync(
                IdPrompt(genere), Etichetta(genere),
                New Dictionary(Of String, String) From {
                    {SegnapostoPezzi, LibreriaPrompt.ComeNelPrompt(ComeLista(daFare))}},
                annulla, lingua).ConfigureAwait(False)

            Raccogli(risposta, esito)

            Return esito

        End Function

        ''' <summary>I pezzi come li vede il prompt: una lista di <c>id</c> e <c>testo</c>.</summary>
        Private Shared Function ComeLista(pezzi As IEnumerable(Of PezzoDiProsa)) As JsonArray

            Dim lista As New JsonArray()

            For Each pezzo As PezzoDiProsa In pezzi
                lista.Add(New JsonObject From {
                    {"id", pezzo.Id},
                    {"testo", pezzo.Testo}})
            Next

            Return lista

        End Function

        ''' <summary>
        ''' Prende dalla risposta i testi utilizzabili e li mette al posto dei loro.
        ''' </summary>
        ''' <remarks>
        ''' Tre cose si scartano in silenzio, e sono le tre in cui dar retta all'AI
        ''' peggiorerebbe il documento: un <c>id</c> che non avevamo chiesto (inventato, o
        ''' storpiato), un testo vuoto dove il nostro non lo era, e una risposta che non ha
        ''' nemmeno la forma di una lista. In tutti e tre resta il testo di partenza, che è
        ''' già buono: la rifinitura è un miglioramento facoltativo, non un passaggio da cui
        ''' un documento possa uscire peggiore di com'è entrato.
        ''' </remarks>
        Private Shared Sub Raccogli(risposta As JsonNode, esito As Dictionary(Of String, String))

            Dim rifiniti As JsonArray = Elenco(risposta)
            If rifiniti Is Nothing Then Return

            For Each voce As JsonNode In rifiniti

                Dim pezzo As JsonObject = TryCast(voce, JsonObject)
                If pezzo Is Nothing Then Continue For

                Dim id As String = CampiJson.Testo(pezzo, "id")
                If String.IsNullOrWhiteSpace(id) OrElse Not esito.ContainsKey(id) Then Continue For

                Dim testo As String = CampiJson.Testo(pezzo, "testo")
                If String.IsNullOrWhiteSpace(testo) Then Continue For

                esito(id) = testo

            Next

        End Sub

        ''' <summary>La lista dei pezzi rifiniti, comunque il modello l'abbia incartata.</summary>
        Private Shared Function Elenco(risposta As JsonNode) As JsonArray

            Dim lista As JsonArray = TryCast(risposta, JsonArray)
            If lista IsNot Nothing Then Return lista

            Dim oggetto As JsonObject = TryCast(risposta, JsonObject)
            If oggetto Is Nothing Then Return Nothing

            Return TryCast(CampiJson.Nodo(oggetto, "pezzi"), JsonArray)

        End Function

        ''' <summary>Il prompt del genere.</summary>
        Private Shared Function IdPrompt(genere As GenereProsa) As String

            Select Case genere
                Case GenereProsa.Sintesi : Return IdPromptSintesi
                Case GenereProsa.Frasi : Return IdPromptFrasi
                Case Else : Return IdPromptProsa
            End Select

        End Function

        ''' <summary>Come si chiama questo lavoro nelle parole dell'utente (v. <see cref="MestiereAi"/>).</summary>
        Private Shared Function Etichetta(genere As GenereProsa) As String

            Select Case genere
                Case GenereProsa.Sintesi : Return "la rifinitura del sommario"
                Case GenereProsa.Frasi : Return "la rifinitura delle descrizioni"
                Case Else : Return "la rifinitura del testo"
            End Select

        End Function

    End Class

End Namespace
