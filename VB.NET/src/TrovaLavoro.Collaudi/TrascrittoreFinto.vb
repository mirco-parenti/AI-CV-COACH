Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai

''' <summary>
''' Il trascrittore che i collaudi mettono al posto dell'AI quando l'ingresso è un PDF:
''' restituisce il testo che il collaudo gli ha preparato e annota cosa gli è stato
''' chiesto. Serve a far girare l'import intero — formato, soglie, strutturazione —
''' senza rete e senza spendere un token.
''' </summary>
Friend Class TrascrittoreFinto
    Implements ITrascrittorePdf

    Private ReadOnly _testo As String
    Private ReadOnly _errore As Exception

    ''' <summary>I file di cui è stata chiesta la trascrizione, nell'ordine.</summary>
    Public ReadOnly Property Chiamate As New List(Of String)

    ''' <param name="testo">Il testo che questo trascrittore farà finta di leggere.</param>
    Public Sub New(testo As String)
        _testo = testo
    End Sub

    ''' <param name="errore">L'errore da sollevare invece di trascrivere.</param>
    Public Sub New(errore As Exception)
        _errore = errore
    End Sub

    Public Function TrascriviAsync(percorsoPdf As String,
                                   Optional annulla As CancellationToken = Nothing) _
                                   As Task(Of String) Implements ITrascrittorePdf.TrascriviAsync

        Chiamate.Add(percorsoPdf)

        If _errore IsNot Nothing Then Throw _errore

        Return Task.FromResult(_testo)

    End Function

End Class
