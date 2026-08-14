Imports System.Globalization
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Dati

Namespace Ai

    ''' <summary>
    ''' Chi riconosce, in una cartella di documenti, quali sono CV, attestati o lettere
    ''' (cap. 05.2). È un'interfaccia come gli altri mestieri, per la stessa ragione: il
    ''' pannello si stacca dall'AI e il banco prova il giro intero senza chiave e senza
    ''' rete.
    ''' </summary>
    ''' <remarks>
    ''' <b>Non legge i documenti: li smista.</b> Riceve un elenco — nome, data,
    ''' dimensione e un assaggio di testo dove il disco lo concede — e restituisce una
    ''' proposta di categoria per ciascuno. Nulla di quel che dice entra in un profilo o
    ''' in un'email prima che una persona l'abbia confermato (cap. 05.2, passo 4).
    ''' </remarks>
    Public Interface IClassificatoreDocumenti

        ''' <summary>
        ''' Propone una categoria per ogni file dell'elenco.
        ''' </summary>
        ''' <param name="documenti">I file trovati nella cartella; almeno uno.</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        Function ClassificaAsync(documenti As IEnumerable(Of FileTrovato),
                                 Optional annulla As CancellationToken = Nothing) As Task(Of JsonNode)

    End Interface

    ''' <summary>
    ''' Il classificatore vero: come gli altri mestieri non ha logica sua — è la fila
    ''' comune di <see cref="MestiereAi"/> con addosso il suo segnaposto.
    ''' </summary>
    Public Class ClassificatoreDocumenti
        Inherits MestiereAi
        Implements IClassificatoreDocumenti

        ''' <summary>L'identificativo del prompt nel pool (Pool 1.04).</summary>
        Public Const IdPrompt As String = "classifica_documenti"

        ''' <summary>Il segnaposto che il prompt dichiara (cap. 04.3).</summary>
        Public Const SegnapostoDocumenti As String = "DOCUMENTI"

        ''' <summary>Come si chiama questo lavoro nei messaggi all'utente.</summary>
        Private Const Etichetta As String = "il riconoscimento dei documenti"

        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)
            MyBase.New(libreria, client)
        End Sub

        ''' <inheritdoc/>
        Public Function ClassificaAsync(documenti As IEnumerable(Of FileTrovato),
                                        Optional annulla As CancellationToken = Nothing) _
                                        As Task(Of JsonNode) Implements IClassificatoreDocumenti.ClassificaAsync

            Dim elenco As JsonArray = ComeElenco(documenti)

            ' Una cartella senza niente da smistare non si manda a smistare: costerebbe
            ' un'attesa e dei token per farsi rispondere «non c'è nulla».
            If elenco.Count = 0 Then
                Throw New ErroreAi(CausaErroreAi.Richiesta,
                    $"Non sono riuscita a preparare {Etichetta}: nella cartella non ci sono file leggibili.")
            End If

            Return EseguiAsync(IdPrompt, Etichetta,
                New Dictionary(Of String, String) From {
                    {SegnapostoDocumenti, LibreriaPrompt.ComeNelPrompt(elenco)}},
                annulla)

        End Function

        ''' <summary>
        ''' L'elenco come lo vede il prompt. L'assaggio compare <b>solo se c'è</b>: un
        ''' campo vuoto sembrerebbe un documento senza testo — cioè un'informazione — e
        ''' invece vuol dire che il testo non si è potuto leggere (i PDF, quasi sempre).
        ''' </summary>
        Private Shared Function ComeElenco(documenti As IEnumerable(Of FileTrovato)) As JsonArray

            Dim elenco As New JsonArray()
            If documenti Is Nothing Then Return elenco

            For Each documento As FileTrovato In documenti

                If documento Is Nothing OrElse String.IsNullOrWhiteSpace(documento.Nome) Then Continue For

                Dim voce As New JsonObject From {
                    {"nome", documento.Nome},
                    {"modificato", CampiJson.Quando(documento.Modificato)},
                    {"dimensione", ComeSiLegge(documento.Dimensione)}}

                If String.IsNullOrWhiteSpace(documento.Assaggio) Then
                    voce("assaggio_non_disponibile") = JsonValue.Create(True)
                Else
                    voce("assaggio") = JsonValue.Create(documento.Assaggio)
                End If

                elenco.Add(voce)

            Next

            Return elenco

        End Function

        ''' <summary>La dimensione come la scriverebbe una persona: «340 KB», «1,2 MB».</summary>
        Private Shared Function ComeSiLegge(byte_ As Long) As String

            If byte_ < 1024L Then Return $"{byte_} byte"
            If byte_ < 1024L * 1024L Then Return $"{byte_ \ 1024L} KB"

            Return (byte_ / (1024.0 * 1024.0)).ToString("0.#", CultureInfo.InvariantCulture) & " MB"

        End Function

    End Class

End Namespace
