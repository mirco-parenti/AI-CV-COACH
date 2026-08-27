Namespace Motore

    ''' <summary>
    ''' Un'attesa con un tetto: aspetta un compito, ma non per sempre.
    ''' </summary>
    ''' <remarks>
    ''' <para>Nasce il 2026-08-27, dalla revisione del giro D. La stampa in PDF aspettava
    ''' un evento del browser dentro un <c>TaskCompletionSource</c>: se quell'evento non
    ''' fosse mai arrivato — il processo del browser che muore in silenzio, una pagina che
    ''' non finisce di caricare — l'attesa sarebbe rimasta appesa, e con lei la finestra.
    ''' Non è un caso che si sia mai visto; è un caso che non ha un modo di finire.</para>
    ''' <para>Sta qui, fuori dalla stampante, per una ragione sola: <b>si può
    ''' collaudare</b>. Un tetto che scatta non si prova con una WebView2, si prova con un
    ''' compito che non finisce mai — e quello si scrive in tre righe.</para>
    ''' </remarks>
    Public Module Attese

        ''' <summary>
        ''' Aspetta <paramref name="compito"/> al massimo per <paramref name="tetto"/>, e
        ''' se scade solleva <see cref="TimeoutException"/> dicendo che cosa stava
        ''' aspettando.
        ''' </summary>
        ''' <remarks>
        ''' Il compito <b>non</b> viene annullato: non tutti sanno esserlo, e quello del
        ''' browser non lo sa. Scaduto il tetto lo si smette di aspettare, che è la sola
        ''' cosa che serve a chi sta davanti alla finestra.
        ''' </remarks>
        Public Async Function EntroIlTetto(Of T)(compito As Task(Of T), tetto As TimeSpan,
                                                cosaAspettavo As String) As Task(Of T)

            If compito Is Nothing Then Throw New ArgumentNullException(NameOf(compito))
            If tetto <= TimeSpan.Zero Then
                Throw New ArgumentOutOfRangeException(NameOf(tetto), "Il tetto dev'essere un tempo vero.")
            End If

            Dim scaduto As Task = Task.Delay(tetto)

            If Await Task.WhenAny(compito, scaduto).ConfigureAwait(False) Is scaduto Then
                Throw New TimeoutException(Scaduta(cosaAspettavo, tetto))
            End If

            Return Await compito.ConfigureAwait(False)

        End Function

        ''' <summary>Come sopra, per un compito che non restituisce niente.</summary>
        Public Async Function EntroIlTetto(compito As Task, tetto As TimeSpan,
                                           cosaAspettavo As String) As Task

            If compito Is Nothing Then Throw New ArgumentNullException(NameOf(compito))
            If tetto <= TimeSpan.Zero Then
                Throw New ArgumentOutOfRangeException(NameOf(tetto), "Il tetto dev'essere un tempo vero.")
            End If

            Dim scaduto As Task = Task.Delay(tetto)

            If Await Task.WhenAny(compito, scaduto).ConfigureAwait(False) Is scaduto Then
                Throw New TimeoutException(Scaduta(cosaAspettavo, tetto))
            End If

            Await compito.ConfigureAwait(False)

        End Function

        ''' <summary>Il messaggio di un tetto scaduto: che cosa si aspettava, e per quanto.</summary>
        Public Function Scaduta(cosaAspettavo As String, tetto As TimeSpan) As String

            Dim che As String = If(String.IsNullOrWhiteSpace(cosaAspettavo), "L'attesa", cosaAspettavo.Trim())
            Return $"{che} non è finita entro {CInt(Math.Round(tetto.TotalSeconds))} secondi."

        End Function

    End Module

End Namespace
