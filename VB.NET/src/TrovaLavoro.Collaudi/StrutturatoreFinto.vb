Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai

''' <summary>
''' Lo strutturatore che i collaudi mettono al posto dell'AI: restituisce i frammenti
''' che il collaudo ha preparato, nell'ordine in cui li ha messi, e annota cosa gli è
''' stato chiesto. È ciò che permette di far girare il dialogo intero — sette turni,
''' anti-perdita, passata finale — senza rete e senza spendere un token.
''' </summary>
Friend Class StrutturatoreFinto
    Implements IStrutturatoreTurni

    ''' <summary>Una chiamata ricevuta: per quale turno, e con quali parole.</summary>
    Friend Class Chiamata
        Public Property Turno As String
        Public Property Risposta As String
    End Class

    ''' <summary>Le risposte preparate: testo JSON, oppure un'eccezione da sollevare.</summary>
    Private ReadOnly _preparate As New Queue(Of Object)

    ''' <summary>Tutto ciò che è stato chiesto, nell'ordine.</summary>
    Public ReadOnly Property Chiamate As New List(Of Chiamata)

    ''' <summary>Prepara il prossimo frammento (forma fluente: si concatenano).</summary>
    Public Function Dara(frammentoJson As String) As StrutturatoreFinto
        _preparate.Enqueue(frammentoJson)
        Return Me
    End Function

    ''' <summary>Prepara un errore al posto del prossimo frammento.</summary>
    Public Function Fallira(errore As Exception) As StrutturatoreFinto
        _preparate.Enqueue(errore)
        Return Me
    End Function

    ''' <summary>I turni chiesti finora, in ordine, come una sola stringa leggibile.</summary>
    Public Function TurniChiesti() As String
        Return String.Join(" → ", Chiamate.Select(Function(c) c.Turno))
    End Function

    Public Function StrutturaAsync(turno As String, risposta As String,
                                   Optional annulla As CancellationToken = Nothing) _
                                   As Task(Of JsonNode) Implements IStrutturatoreTurni.StrutturaAsync

        Chiamate.Add(New Chiamata With {.Turno = turno, .Risposta = risposta})

        If _preparate.Count = 0 Then
            ' Meglio un errore chiaro che un collaudo che prosegue a caso: vuol dire che
            ' il dialogo ha chiesto una cosa in più rispetto a quelle previste.
            Throw New InvalidOperationException(
                $"Il collaudo non ha preparato una risposta per il turno «{turno}» " &
                $"(chiamate finora: {TurniChiesti()}).")
        End If

        Dim prossima As Object = _preparate.Dequeue()

        Dim errore As Exception = TryCast(prossima, Exception)
        If errore IsNot Nothing Then Throw errore

        Return Task.FromResult(JsonNode.Parse(CStr(prossima)))

    End Function

End Class
