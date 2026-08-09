Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>
    ''' Chi sa ricavare il testo di un CV in PDF. È un'interfaccia per lo stesso motivo
    ''' di <see cref="IStrutturatoreTurni"/>: è la porta da cui l'import si stacca
    ''' dall'AI. In esercizio dietro c'è <see cref="TrascrittorePdf"/> con l'API vera,
    ''' nel banco di collaudo un sostituto che restituisce testo già pronto.
    ''' </summary>
    Public Interface ITrascrittorePdf

        ''' <summary>Trascrive il PDF indicato e restituisce il suo testo.</summary>
        ''' <param name="percorsoPdf">Il file da leggere.</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        Function TrascriviAsync(percorsoPdf As String,
                                Optional annulla As CancellationToken = Nothing) As Task(Of String)

    End Interface

    ''' <summary>
    ''' Il trascrittore vero: manda il PDF all'API dentro un blocco <c>document</c> e si
    ''' fa restituire il testo, come faceva il prototipo (cap. 05.1). Il PDF è l'unico
    ''' formato in ingresso che il disco non basta a leggere.
    ''' </summary>
    ''' <remarks>
    ''' <para>Il messaggio ha la forma del prototipo, e nell'ordine del prototipo: prima
    ''' il documento, poi l'istruzione di trascrizione. Il prompt è
    ''' <c>trascrizione_pdf</c> del pool, che dichiara da sé livello di modello e limite
    ''' di token — qui non si ripetono, come per i turni.</para>
    ''' <para>Il metodo è quello validato anche sui CV a due colonne (step 1.36): il
    ''' modello di estrazione legge il documento impaginato e ne riporta il testo
    ''' nell'ordine giusto, cosa che un estrattore meccanico su due colonne sbaglia.</para>
    ''' </remarks>
    Public Class TrascrittorePdf
        Implements ITrascrittorePdf

        ''' <summary>L'identificativo del prompt nel pool.</summary>
        Public Const IdPrompt As String = "trascrizione_pdf"

        ''' <summary>
        ''' Quanto può pesare al massimo un PDF: è il limite dell'API per un documento
        ''' (cap. 05.1). Oltre, la chiamata verrebbe rifiutata dopo aver caricato tutto:
        ''' meglio dirlo prima. L'altro limite dichiarato — le 100 pagine — non si può
        ''' contare senza aprire il PDF, e resta un errore che arriva dall'API.
        ''' </summary>
        Public Const DimensioneMassima As Long = 32L * 1024L * 1024L

        Private ReadOnly _libreria As LibreriaPrompt
        Private ReadOnly _client As ClientClaude

        ''' <param name="libreria">Il pool da cui viene il prompt di trascrizione.</param>
        ''' <param name="client">Il client dell'AI, già con la sua chiave.</param>
        Public Sub New(libreria As LibreriaPrompt, client As ClientClaude)

            If libreria Is Nothing Then Throw New ArgumentNullException(NameOf(libreria))
            If client Is Nothing Then Throw New ArgumentNullException(NameOf(client))

            _libreria = libreria
            _client = client

        End Sub

        ''' <inheritdoc/>
        Public Async Function TrascriviAsync(percorsoPdf As String,
                                             Optional annulla As CancellationToken = Nothing) _
                                             As Task(Of String) Implements ITrascrittorePdf.TrascriviAsync

            If String.IsNullOrWhiteSpace(percorsoPdf) Then
                Throw New ArgumentException("Manca il percorso del PDF.", NameOf(percorsoPdf))
            End If

            Dim prompt As Prompt = CaricaPrompt()

            ' Il PDF viaggia codificato in base64 dentro la richiesta: va letto tutto in
            ' memoria, ed è il motivo per cui il limite di dimensione si controlla prima.
            ' La lettura è asincrona e da qui in poi si sta sul thread pool: la codifica
            ' base64 di decine di MB non deve congelare l'interfaccia.
            Dim dati As Byte() = Await File.ReadAllBytesAsync(percorsoPdf, annulla).ConfigureAwait(False)

            Dim contenuto As New JsonArray(
                New JsonObject From {
                    {"type", "document"},
                    {"source", New JsonObject From {
                        {"type", "base64"},
                        {"media_type", "application/pdf"},
                        {"data", Convert.ToBase64String(dati)}}}},
                New JsonObject From {
                    {"type", "text"},
                    {"text", prompt.Corpo}})

            Dim uscita As RispostaAi = Await _client.ChiediAsync(
                prompt.Modello, contenuto, prompt.MaxToken, annulla).ConfigureAwait(False)

            ' Il prompt dichiara «uscita: testo»: qui non c'è JSON da estrarre, quello
            ' che torna è già il testo del CV.
            Return If(uscita.Testo, String.Empty)

        End Function

        ''' <summary>
        ''' Carica il prompt traducendo gli inciampi del pool in <see cref="ErroreAi"/>:
        ''' un pool esterno ritoccato male deve produrre il messaggio in italiano dei
        ''' pannelli, non un'eccezione grezza a metà import (il pool è modificabile per
        ''' design, cap. 04.2).
        ''' </summary>
        Private Function CaricaPrompt() As Prompt

            Try
                Return _libreria.Carica(IdPrompt)
            Catch ex As Exception When TypeOf ex Is InvalidDataException OrElse
                                       TypeOf ex Is FileNotFoundException OrElse
                                       TypeOf ex Is IOException
                Throw New ErroreAi(CausaErroreAi.Richiesta,
                    $"Il prompt «{IdPrompt}» del pool non è utilizzabile: {ex.Message}", ex)
            End Try

        End Function

    End Class

End Namespace
