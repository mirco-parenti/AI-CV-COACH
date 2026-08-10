Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>
    ''' Chi sa mettere un profilo davanti a un annuncio: prima i <b>giudizi</b> voce per
    ''' voce (anello 3), poi — quando serve — le <b>mitigazioni</b> sui gap. È
    ''' un'interfaccia per la stessa ragione delle altre: è la porta da cui la pipeline
    ''' si stacca dall'AI, e senza quella porta non si collauderebbe senza rete.
    ''' </summary>
    ''' <remarks>
    ''' Due prompt, un mestiere solo (cap. 02): la mitigazione non è un lavoro a sé, è il
    ''' secondo tempo dello stesso — parte dai gap che i giudizi hanno appena trovato e
    ''' non ha senso senza di loro. Il punteggio, invece, non si chiede all'AI: lo calcola
    ''' <c>CalcoloMatch</c> dai giudizi, ed è deterministico.
    ''' </remarks>
    Public Interface IConfrontatore

        ''' <summary>
        ''' Giudica il profilo contro l'annuncio, voce per voce.
        ''' </summary>
        ''' <param name="profilo">Il profilo del candidato (anello 1).</param>
        ''' <param name="annuncio">L'annuncio strutturato (anello 2).</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        ''' <returns>I giudizi con la valutazione d'insieme, già estratti.</returns>
        Function ConfrontaAsync(profilo As JsonNode, annuncio As JsonNode,
                                Optional annulla As CancellationToken = Nothing) As Task(Of JsonNode)

        ''' <summary>
        ''' Costruisce gli argomenti di mitigazione sui gap che i giudizi hanno trovato.
        ''' </summary>
        ''' <param name="profilo">Il profilo del candidato: l'unica fonte di fatti.</param>
        ''' <param name="giudizi">
        ''' La <b>lista</b> dei giudizi dell'anello 3, come la restituisce
        ''' <see cref="ConfrontaAsync"/>: dice dove sono i gap.
        ''' </param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        ''' <returns>
        ''' Le mitigazioni, già estratte. Una lista vuota è un esito legittimo: se
        ''' nessun gap ha un ponte onesto, il prompt tace — ed è ciò che deve fare.
        ''' </returns>
        Function MitigaAsync(profilo As JsonNode, giudizi As JsonNode,
                             Optional annulla As CancellationToken = Nothing) As Task(Of JsonNode)

    End Interface

    ''' <summary>
    ''' Il confrontatore vero: il secondo dei tre mestieri di T4, e l'unico che porta due
    ''' prompt. Come gli altri non ha logica sua — è la fila comune di
    ''' <see cref="MestiereAi"/> con addosso i suoi segnaposti e i suoi nomi.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Attenzione a come si serializzano gli artefatti.</b> Profilo, annuncio e
    ''' giudizi entrano nei prompt passando da <see cref="LibreriaPrompt.ComeNelPrompt"/>,
    ''' che riproduce il <c>JSON.stringify(x, null, 2)</c> del prototipo — due spazi di
    ''' rientro e accenti lasciati lettere. Non è un dettaglio estetico: <c>confronto</c>
    ''' e <c>mitigazione</c> sono i due prompt su cui vale ancora la <b>parità carattere
    ''' per carattere</b> col prototipo (cap. 04.7), che è l'ancora della
    ''' non-regressione. Serializzare in un altro modo la spezzerebbe senza che nessuno
    ''' se ne accorga.</para>
    ''' </remarks>
    Public Class Confrontatore
        Inherits MestiereAi
        Implements IConfrontatore

        ''' <summary>Gli identificativi dei due prompt nel pool.</summary>
        Public Const IdPromptConfronto As String = "confronto"
        Public Const IdPromptMitigazione As String = "mitigazione"

        ''' <summary>I segnaposti che i due prompt dichiarano (cap. 04.3).</summary>
        Public Const SegnapostoProfilo As String = "PROFILO"
        Public Const SegnapostoAnnuncio As String = "ANNUNCIO"
        Public Const SegnapostoGiudizi As String = "GIUDIZI"

        ''' <summary>Come si chiamano i due lavori nei messaggi all'utente.</summary>
        Private Const EtichettaConfronto As String = "il confronto col profilo"
        Private Const EtichettaMitigazione As String = "la ricerca delle mitigazioni"

        ''' <param name="libreria">Il pool da cui vengono i due prompt.</param>
        ''' <param name="client">Il client dell'AI, già con la sua chiave.</param>
        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)
            MyBase.New(libreria, client)
        End Sub

        ''' <inheritdoc/>
        Public Function ConfrontaAsync(profilo As JsonNode, annuncio As JsonNode,
                                       Optional annulla As CancellationToken = Nothing) _
                                       As Task(Of JsonNode) Implements IConfrontatore.ConfrontaAsync

            Esigi(profilo, "il profilo", EtichettaConfronto)
            Esigi(annuncio, "l'annuncio", EtichettaConfronto)

            Return EseguiAsync(IdPromptConfronto, EtichettaConfronto,
                New Dictionary(Of String, String) From {
                    {SegnapostoProfilo, LibreriaPrompt.ComeNelPrompt(profilo)},
                    {SegnapostoAnnuncio, LibreriaPrompt.ComeNelPrompt(annuncio)}}, annulla)

        End Function

        ''' <inheritdoc/>
        Public Function MitigaAsync(profilo As JsonNode, giudizi As JsonNode,
                                    Optional annulla As CancellationToken = Nothing) _
                                    As Task(Of JsonNode) Implements IConfrontatore.MitigaAsync

            Esigi(profilo, "il profilo", EtichettaMitigazione)
            Esigi(giudizi, "i giudizi del confronto", EtichettaMitigazione)

            Return EseguiAsync(IdPromptMitigazione, EtichettaMitigazione,
                New Dictionary(Of String, String) From {
                    {SegnapostoProfilo, LibreriaPrompt.ComeNelPrompt(profilo)},
                    {SegnapostoGiudizi, LibreriaPrompt.ComeNelPrompt(giudizi)}}, annulla)

        End Function

    End Class

End Namespace
