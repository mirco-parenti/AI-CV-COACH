Imports System.Threading.Tasks
Imports TrovaLavoro.Web

''' <summary>
''' Il lettore della pagina, finto: risponde con la pagina che il collaudo gli ha messo in
''' mano, o inciampa se gli si è chiesto di inciampare.
''' </summary>
''' <remarks>
''' È ciò che permette di provare la <b>cattura</b> — il testo basta? da che portale viene?
''' cosa si racconta all'utente? — senza WebView2 e senza un thread STA, come
''' <see cref="StrutturatoreFinto"/> fa col dialogo e i mestieri finti con l'AI. La lettura
''' vera è una riga di JavaScript, e ha il suo collaudo nella categoria «Reale».
''' </remarks>
Friend Class LettorePaginaFinto
    Implements ILettorePagina

    ''' <summary>Quel che risponderà a chi gli chiede la pagina.</summary>
    Public Property Pagina As New PaginaLetta()

    ''' <summary>Se invece deve fallire; <c>Nothing</c> per rispondere normalmente.</summary>
    Public Property Guasto As Exception

    ''' <summary>Quante volte gli è stata chiesta la pagina.</summary>
    Public Property Letture As Integer

    Public Function LeggiAsync() As Task(Of PaginaLetta) Implements ILettorePagina.LeggiAsync

        Letture += 1

        If Guasto IsNot Nothing Then Throw Guasto

        Return Task.FromResult(Pagina)

    End Function

End Class
