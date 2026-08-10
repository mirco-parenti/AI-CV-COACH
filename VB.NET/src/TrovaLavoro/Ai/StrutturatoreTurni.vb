Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>
    ''' Chi sa trasformare la risposta di un turno nel suo frammento JSON. È
    ''' un'interfaccia e non una classe sola perché è la porta da cui il dialogo
    ''' (cap. 12, flusso B) si stacca dall'AI: in esercizio dietro c'è
    ''' <see cref="StrutturatoreTurni"/> con il pool e l'API vera, nel banco di collaudo
    ''' c'è un sostituto che restituisce frammenti già pronti — così la macchina del
    ''' dialogo si collauda tutta, senza rete e senza spendere un token.
    ''' </summary>
    Public Interface IStrutturatoreTurni

        ''' <summary>
        ''' Struttura la risposta dell'utente per un turno.
        ''' </summary>
        ''' <param name="turno">
        ''' L'identificativo del turno, che è anche quello del prompt nel pool:
        ''' <c>nome</c>, <c>contatti</c>, <c>patente</c>, <c>esperienze_formali</c>,
        ''' <c>esperienze_informali</c>, <c>competenze</c>, <c>formazione</c>.
        ''' </param>
        ''' <param name="risposta">Le parole dell'utente, così come le ha scritte.</param>
        ''' <returns>Il frammento JSON prodotto dall'AI, già estratto.</returns>
        Function StrutturaAsync(turno As String, risposta As String,
                                Optional annulla As CancellationToken = Nothing) As Task(Of JsonNode)

    End Interface

    ''' <summary>
    ''' Lo strutturatore vero: prende il prompt del turno dal pool, ci mette dentro la
    ''' risposta dell'utente, chiede all'AI ed estrae il JSON dal testo che torna. È la
    ''' fila comune a tutti i mestieri (<see cref="MestiereAi"/>) con addosso l'unica
    ''' cosa che è sua: il turno, che qui fa da identificativo del prompt <b>e</b> da
    ''' nome del lavoro nei messaggi all'utente.
    ''' </summary>
    Public Class StrutturatoreTurni
        Inherits MestiereAi
        Implements IStrutturatoreTurni

        ''' <summary>Il segnaposto che tutti i prompt di turno dichiarano (cap. 04.3).</summary>
        Public Const SegnapostoRisposta As String = "RISPOSTA_UTENTE"

        ''' <param name="libreria">Il pool da cui vengono i prompt dei turni.</param>
        ''' <param name="client">Il client dell'AI, già con la sua chiave.</param>
        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)
            MyBase.New(libreria, client)
        End Sub

        ''' <inheritdoc/>
        Public Function StrutturaAsync(turno As String, risposta As String,
                                       Optional annulla As CancellationToken = Nothing) _
                                       As Task(Of JsonNode) Implements IStrutturatoreTurni.StrutturaAsync

            ' L'etichetta è il modo in cui questo lavoro si chiama nelle parole
            ' dell'utente: se qualcosa va storto, la bolla del dialogo dirà «il turno
            ' «nome»» e non un identificativo di file.
            Return EseguiAsync(turno, $"il turno «{turno}»",
                New Dictionary(Of String, String) From {
                    {SegnapostoRisposta, If(risposta, String.Empty)}}, annulla)

        End Function

    End Class

End Namespace
