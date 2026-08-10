Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>
    ''' Chi scrive i documenti della candidatura (anello 4): il 📄 <b>CV base</b> dal solo
    ''' profilo, il 🎯 <b>CV mirato</b> su un annuncio, e la <b>lettera</b> che li tiene
    ''' insieme. È un'interfaccia per la stessa ragione delle altre: è la porta da cui la
    ''' pipeline si stacca dall'AI.
    ''' </summary>
    ''' <remarks>
    ''' Tre prompt, un mestiere solo: sono lo stesso lavoro — trasformare il profilo in
    ''' un documento — a tre distanze diverse dall'annuncio. E tutti e tre hanno la stessa
    ''' regola sopra ogni altra: <b>il profilo è l'unica fonte di fatti</b>. Annuncio,
    ''' giudizi, CV e mitigazioni orientano l'enfasi e la coerenza, non aggiungono nulla
    ''' che il profilo non dichiari.
    ''' </remarks>
    Public Interface IGeneratore

        ''' <summary>
        ''' Genera il 📄 <b>CV base</b>: dal solo profilo, senza un annuncio di riferimento.
        ''' </summary>
        ''' <param name="profilo">Il profilo del candidato (anello 1).</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        Function GeneraCvBaseAsync(profilo As JsonNode,
                                   Optional annulla As CancellationToken = Nothing) As Task(Of JsonNode)

        ''' <summary>
        ''' Genera il 🎯 <b>CV mirato</b> su un annuncio: stessi fatti del CV base, messi
        ''' in risalto secondo ciò che quell'annuncio chiede.
        ''' </summary>
        ''' <param name="profilo">Il profilo: l'unica fonte di fatti.</param>
        ''' <param name="annuncio">L'annuncio strutturato (anello 2): il segnale di mira.</param>
        ''' <param name="giudizi">La lista dei giudizi dell'anello 3.</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        Function GeneraCvMiratoAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                     Optional annulla As CancellationToken = Nothing) As Task(Of JsonNode)

        ''' <summary>
        ''' Genera la <b>lettera</b> di presentazione, coerente col CV mirato e onesta sui
        ''' gap che le mitigazioni sanno nominare.
        ''' </summary>
        ''' <param name="profilo">Il profilo: l'unica fonte di fatti.</param>
        ''' <param name="annuncio">L'annuncio strutturato.</param>
        ''' <param name="giudizi">La lista dei giudizi dell'anello 3.</param>
        ''' <param name="cv">Il 🎯 CV mirato già generato: riferimento di coerenza, non fonte.</param>
        ''' <param name="mitigazioni">
        ''' I ponti onesti sui gap. <b>Facoltativi</b>: se non ce ne sono, la lettera si
        ''' scrive lo stesso e tace sui gap — mai bloccare la generazione per la loro
        ''' assenza (è il comportamento del prototipo, cap. 14).
        ''' </param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        Function GeneraLetteraAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                    cv As JsonNode, mitigazioni As JsonNode,
                                    Optional annulla As CancellationToken = Nothing) As Task(Of JsonNode)

    End Interface

    ''' <summary>
    ''' Il generatore vero: il terzo dei tre mestieri di T4, e quello con più prompt. Come
    ''' gli altri non ha logica sua — è la fila comune di <see cref="MestiereAi"/> con
    ''' addosso i suoi segnaposti e i suoi nomi.
    ''' </summary>
    Public Class Generatore
        Inherits MestiereAi
        Implements IGeneratore

        ''' <summary>Gli identificativi dei tre prompt nel pool.</summary>
        Public Const IdPromptCvBase As String = "cv_base"
        Public Const IdPromptCvMirato As String = "cv_mirato"
        Public Const IdPromptLettera As String = "lettera"

        ''' <summary>I segnaposti che i tre prompt dichiarano (cap. 04.3).</summary>
        Public Const SegnapostoProfilo As String = "PROFILO"
        Public Const SegnapostoAnnuncio As String = "ANNUNCIO"
        Public Const SegnapostoGiudizi As String = "GIUDIZI"
        Public Const SegnapostoCv As String = "CV"
        Public Const SegnapostoMitigazioni As String = "MITIGAZIONI"

        ''' <summary>Come si chiamano i tre lavori nei messaggi all'utente.</summary>
        Private Const EtichettaCvBase As String = "la generazione del CV base"
        Private Const EtichettaCvMirato As String = "la generazione del CV mirato"
        Private Const EtichettaLettera As String = "la generazione della lettera"

        ''' <param name="libreria">Il pool da cui vengono i tre prompt.</param>
        ''' <param name="client">Il client dell'AI, già con la sua chiave.</param>
        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)
            MyBase.New(libreria, client)
        End Sub

        ''' <inheritdoc/>
        Public Function GeneraCvBaseAsync(profilo As JsonNode,
                                          Optional annulla As CancellationToken = Nothing) _
                                          As Task(Of JsonNode) Implements IGeneratore.GeneraCvBaseAsync

            Esigi(profilo, "il profilo", EtichettaCvBase)

            Return EseguiAsync(IdPromptCvBase, EtichettaCvBase,
                New Dictionary(Of String, String) From {
                    {SegnapostoProfilo, LibreriaPrompt.ComeNelPrompt(profilo)}}, annulla)

        End Function

        ''' <inheritdoc/>
        Public Function GeneraCvMiratoAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                            Optional annulla As CancellationToken = Nothing) _
                                            As Task(Of JsonNode) Implements IGeneratore.GeneraCvMiratoAsync

            Esigi(profilo, "il profilo", EtichettaCvMirato)
            Esigi(annuncio, "l'annuncio", EtichettaCvMirato)
            Esigi(giudizi, "i giudizi del confronto", EtichettaCvMirato)

            Return EseguiAsync(IdPromptCvMirato, EtichettaCvMirato,
                New Dictionary(Of String, String) From {
                    {SegnapostoProfilo, LibreriaPrompt.ComeNelPrompt(profilo)},
                    {SegnapostoAnnuncio, LibreriaPrompt.ComeNelPrompt(annuncio)},
                    {SegnapostoGiudizi, LibreriaPrompt.ComeNelPrompt(giudizi)}}, annulla)

        End Function

        ''' <inheritdoc/>
        Public Function GeneraLetteraAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                           cv As JsonNode, mitigazioni As JsonNode,
                                           Optional annulla As CancellationToken = Nothing) _
                                           As Task(Of JsonNode) Implements IGeneratore.GeneraLetteraAsync

            Esigi(profilo, "il profilo", EtichettaLettera)
            Esigi(annuncio, "l'annuncio", EtichettaLettera)
            Esigi(giudizi, "i giudizi del confronto", EtichettaLettera)
            Esigi(cv, "il CV mirato", EtichettaLettera)

            ' Le mitigazioni sono le uniche facoltative, e non per distrazione: se nessun
            ' gap ha un ponte onesto la lista è vuota per progetto, e una lettera che non
            ' si scrive per questo sarebbe un difetto. In loro assenza si manda [], come
            ' fa il prototipo, e la lettera tacerà sui gap.
            Return EseguiAsync(IdPromptLettera, EtichettaLettera,
                New Dictionary(Of String, String) From {
                    {SegnapostoProfilo, LibreriaPrompt.ComeNelPrompt(profilo)},
                    {SegnapostoAnnuncio, LibreriaPrompt.ComeNelPrompt(annuncio)},
                    {SegnapostoGiudizi, LibreriaPrompt.ComeNelPrompt(giudizi)},
                    {SegnapostoCv, LibreriaPrompt.ComeNelPrompt(cv)},
                    {SegnapostoMitigazioni, LibreriaPrompt.ComeNelPrompt(If(mitigazioni, New JsonArray()))}},
                annulla)

        End Function

    End Class

End Namespace
