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

    ''' <summary>Quante volte gli è stato chiesto di scorrere.</summary>
    Public Property Scorrimenti As Integer

    ''' <summary>
    ''' Quel che si legge <b>dopo</b> aver scorso, se il collaudo vuole una pagina che
    ''' cresce scendendo. <c>Nothing</c> per una pagina che è già tutta lì.
    ''' </summary>
    ''' <remarks>
    ''' Serve a provare la cosa che a T5d si è vista sul campo e che nessun collaudo
    ''' vedrebbe da sé: su un sito che carica mentre si scende, leggere senza aver scorso
    ''' dà un profilo dimezzato — e la differenza fra le due letture è tutta la ragione per
    ''' cui lo scorrimento esiste.
    ''' </remarks>
    Public Property PaginaDopoScorrimento As PaginaLetta

    Public Function LeggiAsync() As Task(Of PaginaLetta) Implements ILettorePagina.LeggiAsync

        Letture += 1

        If Guasto IsNot Nothing Then Throw Guasto

        If Scorrimenti > 0 AndAlso PaginaDopoScorrimento IsNot Nothing Then
            Return Task.FromResult(PaginaDopoScorrimento)
        End If

        Return Task.FromResult(Pagina)

    End Function

    Public Function ScorriAsync() As Task Implements ILettorePagina.ScorriAsync

        Scorrimenti += 1

        Return Task.CompletedTask

    End Function

End Class
