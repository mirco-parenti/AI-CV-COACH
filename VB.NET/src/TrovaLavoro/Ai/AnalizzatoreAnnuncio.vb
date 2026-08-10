Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>
    ''' Chi sa trasformare il testo di un annuncio nel suo JSON. È un'interfaccia per la
    ''' stessa ragione di <see cref="IStrutturatoreTurni"/>: è la porta da cui la
    ''' pipeline di candidatura (cap. 12) si stacca dall'AI. In esercizio dietro c'è
    ''' <see cref="AnalizzatoreAnnuncio"/> con l'API vera, nel banco un sostituto che
    ''' restituisce annunci già pronti — così la pipeline intera si collauda senza rete.
    ''' </summary>
    Public Interface IAnalizzatoreAnnuncio

        ''' <summary>
        ''' Analizza il testo di un annuncio e ne ricava requisiti, contesto e contratto.
        ''' </summary>
        ''' <param name="testoAnnuncio">
        ''' L'annuncio come lo si è avuto: incollato a mano (T4) o catturato dalla pagina
        ''' del portale (T5). Al prompt arriva così com'è, senza ripuliture nostre.
        ''' </param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        ''' <returns>L'annuncio strutturato, già estratto.</returns>
        Function AnalizzaAsync(testoAnnuncio As String,
                               Optional annulla As CancellationToken = Nothing) As Task(Of JsonNode)

    End Interface

    ''' <summary>
    ''' L'analizzatore vero: il primo dei tre mestieri di T4. Non ha logica sua — è la
    ''' fila comune di <see cref="MestiereAi"/> con addosso il suo prompt e il suo nome
    ''' nelle parole dell'utente.
    ''' </summary>
    ''' <remarks>
    ''' Quando il testo non è un annuncio — una pagina di elenco, una home, un login — il
    ''' prompt risponde con lo schema a campi tutti vuoti invece di inventarsi un posto
    ''' di lavoro. Non è un errore e qui non diventa tale: riconoscerlo e dirlo con garbo
    ''' (cap. 06.4) è compito di chi conduce il flusso, non di chi struttura il testo.
    ''' </remarks>
    Public Class AnalizzatoreAnnuncio
        Inherits MestiereAi
        Implements IAnalizzatoreAnnuncio

        ''' <summary>L'identificativo del prompt nel pool.</summary>
        Public Const IdPrompt As String = "analisi_annuncio"

        ''' <summary>Il segnaposto che il prompt dichiara (cap. 04.3).</summary>
        Public Const SegnapostoAnnuncio As String = "RISPOSTA_UTENTE"

        ''' <summary>Come si chiama questo lavoro nei messaggi all'utente.</summary>
        Private Const Etichetta As String = "l'analisi dell'annuncio"

        ''' <param name="libreria">Il pool da cui viene il prompt di analisi.</param>
        ''' <param name="client">Il client dell'AI, già con la sua chiave.</param>
        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)
            MyBase.New(libreria, client)
        End Sub

        ''' <inheritdoc/>
        Public Function AnalizzaAsync(testoAnnuncio As String,
                                      Optional annulla As CancellationToken = Nothing) _
                                      As Task(Of JsonNode) Implements IAnalizzatoreAnnuncio.AnalizzaAsync

            ' Un annuncio vuoto non si manda all'AI: costerebbe un'attesa e dei token per
            ' farsi restituire lo schema vuoto, quando la risposta la sappiamo già.
            If String.IsNullOrWhiteSpace(testoAnnuncio) Then
                Throw New ErroreAi(CausaErroreAi.Richiesta,
                    "Non c'è nessun testo da analizzare: incolla l'annuncio e riprova.")
            End If

            Return EseguiAsync(IdPrompt, Etichetta,
                New Dictionary(Of String, String) From {
                    {SegnapostoAnnuncio, testoAnnuncio}}, annulla)

        End Function

    End Class

End Namespace
