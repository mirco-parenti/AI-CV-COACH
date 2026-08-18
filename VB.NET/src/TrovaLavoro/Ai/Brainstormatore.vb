Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>
    ''' Chi ragiona con l'utente su <b>una</b> opportunità e, quando la conversazione ha
    ''' prodotto qualcosa, ne distilla gli <b>appunti di mira</b> (cap. 12, A6).
    ''' </summary>
    ''' <remarks>
    ''' Due prompt, un mestiere solo, come per <see cref="IConfrontatore"/>: gli appunti
    ''' non sono un lavoro a sé, sono il secondo tempo dello stesso — nascono da quella
    ''' conversazione e senza di lei non esistono.
    ''' </remarks>
    Public Interface IBrainstormatore

        ''' <summary>
        ''' Il turno successivo della conversazione, consegnato <b>man mano che arriva</b>.
        ''' </summary>
        ''' <param name="profilo">Il candidato: l'unica fonte di fatti su di lui.</param>
        ''' <param name="annuncio">L'annuncio strutturato: il bersaglio.</param>
        ''' <param name="giudizi">Il confronto già fatto: dove si regge e dove si scopre.</param>
        ''' <param name="mitigazioni">
        ''' I ponti onesti già costruiti sui gap. Una lista vuota è legittima: vuol dire
        ''' che su quei gap non c'era niente di onesto da dire.
        ''' </param>
        ''' <param name="battute">
        ''' I turni già scambiati, dal più vecchio al più recente. Vuoto al primo giro: lì
        ''' è l'AI ad aprire, con quello che vede.
        ''' </param>
        ''' <param name="pezzo">Dove consegnare ogni pezzo di risposta appena arriva.</param>
        ''' <param name="annulla">Il gettone di chi interrompe (cap. 02.6).</param>
        ''' <returns>
        ''' Il turno intero a flusso finito, col <b>motivo</b> per cui è finito: una
        ''' risposta che ha toccato il tetto dei token si legge lo stesso, ma chi la
        ''' mostra deve poterlo dichiarare (<see cref="RispostaAi.Troncata"/>).
        ''' </returns>
        Function ConversaAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                              mitigazioni As JsonNode, battute As IReadOnlyList(Of TurnoChat),
                              pezzo As Action(Of String),
                              Optional annulla As CancellationToken = Nothing) As Task(Of RispostaAi)

        ''' <summary>
        ''' Distilla dalla conversazione gli appunti confermabili, e a parte i fatti che
        ''' l'utente ha dichiarato e nel profilo non risultano.
        ''' </summary>
        ''' <param name="conversazione">La trascrizione dei turni, in ordine.</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        ''' <returns>
        ''' Gli appunti già estratti. Una lista vuota è un esito legittimo: se la
        ''' conversazione non ha prodotto niente di operativo, il prompt tace.
        ''' </returns>
        Function AppuntiAsync(conversazione As String,
                             Optional annulla As CancellationToken = Nothing) As Task(Of JsonNode)

    End Interface

    ''' <summary>
    ''' Il brainstormatore vero: il primo mestiere del progetto che <b>conversa</b> invece
    ''' di chiedere una cosa sola e finire lì.
    ''' </summary>
    ''' <remarks>
    ''' <para>Come gli altri non ha logica sua: è la fila comune di
    ''' <see cref="MestiereAi"/> con addosso i suoi segnaposti e i suoi nomi. La differenza
    ''' è il trasporto — <see cref="MestiereAi.EseguiInStreamingAsync"/> invece di
    ''' <c>EseguiAsync</c> — perché qui c'è un pannello che mostra il testo mentre
    ''' compare, ed è l'unico posto del prodotto dove lo streaming paga (cap. 02.5).</para>
    ''' <para><b>La conversazione non si conserva</b> (cap. 15.4): quello che resta sono
    ''' gli appunti confermati. Questo mestiere infatti non ricorda niente da un turno
    ''' all'altro — la memoria è di chi lo chiama, come per ogni altra chiamata all'AI.</para>
    ''' </remarks>
    Public Class Brainstormatore
        Inherits MestiereAi
        Implements IBrainstormatore

        ''' <summary>Gli identificativi dei due prompt nel pool (cap. 04.3).</summary>
        Public Const IdPromptBrainstorm As String = "brainstorm"
        Public Const IdPromptAppunti As String = "appunti_di_mira"

        ''' <summary>I segnaposti che i due prompt dichiarano.</summary>
        Public Const SegnapostoProfilo As String = "PROFILO"
        Public Const SegnapostoAnnuncio As String = "ANNUNCIO"
        Public Const SegnapostoGiudizi As String = "GIUDIZI"
        Public Const SegnapostoMitigazioni As String = "MITIGAZIONI"
        Public Const SegnapostoConversazione As String = "CONVERSAZIONE"

        ''' <summary>Come si chiamano i due lavori nei messaggi all'utente.</summary>
        Private Const EtichettaConversazione As String = "il ragionamento sull'opportunità"
        Private Const EtichettaAppunti As String = "gli appunti di mira"

        ''' <param name="libreria">Il pool da cui vengono i due prompt.</param>
        ''' <param name="client">Il client dell'AI, già con la sua chiave.</param>
        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)
            MyBase.New(libreria, client)
        End Sub

        ''' <inheritdoc/>
        Public Function ConversaAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                      mitigazioni As JsonNode, battute As IReadOnlyList(Of TurnoChat),
                                      pezzo As Action(Of String),
                                      Optional annulla As CancellationToken = Nothing) _
                                      As Task(Of RispostaAi) Implements IBrainstormatore.ConversaAsync

            Esigi(profilo, "il profilo", EtichettaConversazione)
            Esigi(annuncio, "l'annuncio", EtichettaConversazione)
            Esigi(giudizi, "i giudizi del confronto", EtichettaConversazione)

            Return EseguiInStreamingAsync(IdPromptBrainstorm, EtichettaConversazione,
                New Dictionary(Of String, String) From {
                    {SegnapostoProfilo, LibreriaPrompt.ComeNelPrompt(profilo)},
                    {SegnapostoAnnuncio, LibreriaPrompt.ComeNelPrompt(annuncio)},
                    {SegnapostoGiudizi, LibreriaPrompt.ComeNelPrompt(giudizi)},
                    {SegnapostoMitigazioni, LibreriaPrompt.ComeNelPrompt(If(mitigazioni, NessunPonte()))}},
                battute, pezzo, annulla)

        End Function

        ''' <inheritdoc/>
        Public Function AppuntiAsync(conversazione As String,
                                     Optional annulla As CancellationToken = Nothing) _
                                     As Task(Of JsonNode) Implements IBrainstormatore.AppuntiAsync

            If String.IsNullOrWhiteSpace(conversazione) Then
                Throw New ErroreAi(CausaErroreAi.Richiesta,
                    $"Non sono riuscita a preparare {EtichettaAppunti}: non si è ancora detto niente.")
            End If

            Return EseguiAsync(IdPromptAppunti, EtichettaAppunti,
                New Dictionary(Of String, String) From {
                    {SegnapostoConversazione, conversazione}}, annulla)

        End Function

        ''' <summary>
        ''' Le mitigazioni quando non ce ne sono. Non è la stessa cosa che tacere: al
        ''' prompt va detto che i ponti sono stati cercati e non trovati, perché è un dato
        ''' del caso — non un pezzo di contesto che si è perso per strada.
        ''' </summary>
        Private Shared Function NessunPonte() As JsonNode

            Return New JsonObject From {{"mitigazioni", New JsonArray()}}

        End Function

    End Class

End Namespace
