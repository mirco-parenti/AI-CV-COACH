Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Motore

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
    ''' risposta dell'utente, chiede all'AI ed estrae il JSON dal testo che torna. Sono
    ''' i tre pezzi già collaudati a T2 — <see cref="LibreriaPrompt"/>,
    ''' <see cref="ClientClaude"/>, <see cref="EstrattoreJson"/> — messi in fila; qui
    ''' non c'è logica nuova, e non deve essercene.
    ''' </summary>
    ''' <remarks>
    ''' Livello di modello e limite di token non si ripetono qui: li dichiara il prompt
    ''' nei suoi metadati (cap. 04), e <c>ChiediAsync</c> li legge da lì. È anche il
    ''' motivo per cui il turno non deve sapere nulla dei modelli.
    ''' </remarks>
    Public Class StrutturatoreTurni
        Implements IStrutturatoreTurni

        ''' <summary>Il segnaposto che tutti i prompt di turno dichiarano (cap. 04.3).</summary>
        Public Const SegnapostoRisposta As String = "RISPOSTA_UTENTE"

        Private ReadOnly _libreria As LibreriaPrompt
        Private ReadOnly _client As ClientClaude

        ''' <param name="libreria">Il pool da cui vengono i prompt dei turni.</param>
        ''' <param name="client">Il client dell'AI, già con la sua chiave.</param>
        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)

            If libreria Is Nothing Then Throw New ArgumentNullException(NameOf(libreria))
            If client Is Nothing Then Throw New ArgumentNullException(NameOf(client))

            _libreria = libreria
            _client = client

        End Sub

        ''' <inheritdoc/>
        Public Async Function StrutturaAsync(turno As String, risposta As String,
                                             Optional annulla As CancellationToken = Nothing) _
                                             As Task(Of JsonNode) Implements IStrutturatoreTurni.StrutturaAsync

            Dim prompt As Prompt = _libreria.Carica(turno)

            Dim testo As String = LibreriaPrompt.Riempi(prompt, New Dictionary(Of String, String) From {
                {SegnapostoRisposta, If(risposta, String.Empty)}})

            Dim uscita As RispostaAi = Await _client.ChiediAsync(prompt, testo, annulla).ConfigureAwait(False)

            Try
                Return EstrattoreJson.Estrai(uscita.Testo)
            Catch ex As JsonException
                ' Cap. 02.5: mai un crash davanti a una risposta illeggibile. Il testo
                ' grezzo viaggia nel messaggio perché l'utente possa vedere «cosa ha
                ' risposto il modello» invece di un errore muto.
                Throw New ErroreAi(CausaErroreAi.RispostaInattesa,
                    $"L'AI ha risposto in una forma che non riesco a leggere per il turno «{turno}». " &
                    $"Ecco cosa ha risposto: {uscita.Testo}", ex)
            End Try

        End Function

    End Class

End Namespace
