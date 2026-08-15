Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>
    ''' Chi scrive l'email di candidatura (cap. 07.1): oggetto e corpo, ricavati dalla
    ''' <b>lettera già generata</b>. È un'interfaccia come gli altri mestieri, per la
    ''' stessa ragione: è la porta da cui il pannello si stacca dall'AI e il banco può
    ''' provare un giro intero senza chiave e senza rete.
    ''' </summary>
    ''' <remarks>
    ''' Non è un quarto generatore di documenti: gli altri partono dal profilo, questo
    ''' parte da un documento già scritto e lo <b>accorcia</b>. La catena è profilo →
    ''' lettera → email, e ogni anello stringe invece di aggiungere: è così che
    ''' l'anti-invenzione continua a valere anche dove il profilo non arriva più.
    ''' </remarks>
    Public Interface ICompositoreEmail

        ''' <summary>
        ''' Ricava oggetto e corpo dell'email dalla lettera.
        ''' </summary>
        ''' <param name="lettera">La ✉️ lettera generata: l'unica fonte di fatti.</param>
        ''' <param name="annuncio">L'annuncio: serve solo a nominare il ruolo nell'oggetto.</param>
        ''' <param name="allegati">
        ''' I nomi dei file che partiranno. Servono al rimando («in allegato trovi…»), e
        ''' se non ce ne sono il rimando non si scrive: un'email che nomina un allegato
        ''' che non parte si smentisce da sola.
        ''' </param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        ''' <param name="lingua">
        ''' La lingua dell'email: <c>it</c> o <c>en</c> (cap. 10). È la stessa della
        ''' candidatura, e quindi della lettera da cui l'email nasce: un oggetto italiano
        ''' sopra un corpo inglese è l'ibrido che il collaudo di T7a ha trovato.
        ''' </param>
        Function ComponiAsync(lettera As JsonNode, annuncio As JsonNode,
                              allegati As IEnumerable(Of String),
                              Optional annulla As CancellationToken = Nothing,
                              Optional lingua As String = "it") As Task(Of JsonNode)

    End Interface

    ''' <summary>
    ''' Il compositore vero: come gli altri mestieri non ha logica sua — è la fila comune
    ''' di <see cref="MestiereAi"/> con addosso i suoi segnaposti e i suoi nomi.
    ''' </summary>
    Public Class CompositoreEmail
        Inherits MestiereAi
        Implements ICompositoreEmail

        ''' <summary>L'identificativo del prompt nel pool (Pool 1.04).</summary>
        Public Const IdPrompt As String = "email_candidatura"

        ''' <summary>I segnaposti che il prompt dichiara (cap. 04.3).</summary>
        Public Const SegnapostoLettera As String = "LETTERA"
        Public Const SegnapostoAnnuncio As String = "ANNUNCIO"
        Public Const SegnapostoAllegati As String = "ALLEGATI"

        ''' <summary>Come si chiama questo lavoro nei messaggi all'utente.</summary>
        Private Const Etichetta As String = "la scrittura dell'email"

        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)
            MyBase.New(libreria, client)
        End Sub

        ''' <inheritdoc/>
        Public Function ComponiAsync(lettera As JsonNode, annuncio As JsonNode,
                                     allegati As IEnumerable(Of String),
                                     Optional annulla As CancellationToken = Nothing,
                                     Optional lingua As String = "it") _
                                     As Task(Of JsonNode) Implements ICompositoreEmail.ComponiAsync

            Esigi(lettera, "la lettera", Etichetta)
            Esigi(annuncio, "l'annuncio", Etichetta)

            Return EseguiAsync(IdPrompt, Etichetta,
                New Dictionary(Of String, String) From {
                    {SegnapostoLettera, LibreriaPrompt.ComeNelPrompt(lettera)},
                    {SegnapostoAnnuncio, LibreriaPrompt.ComeNelPrompt(annuncio)},
                    {SegnapostoAllegati, LibreriaPrompt.ComeNelPrompt(ComeElenco(allegati))}},
                annulla, lingua)

        End Function

        ''' <summary>
        ''' I nomi degli allegati in forma di elenco JSON. Nessun allegato è un elenco
        ''' <b>vuoto</b> e non un segnaposto lasciato indietro: il prompt sa cosa farne —
        ''' non scrive il rimando — mentre un segnaposto vuoto lo lascerebbe indovinare.
        ''' </summary>
        Private Shared Function ComeElenco(allegati As IEnumerable(Of String)) As JsonArray

            Dim elenco As New JsonArray()
            If allegati Is Nothing Then Return elenco

            For Each nome As String In allegati
                If String.IsNullOrWhiteSpace(nome) Then Continue For
                elenco.Add(JsonValue.Create(nome.Trim()))
            Next

            Return elenco

        End Function

    End Class

End Namespace
