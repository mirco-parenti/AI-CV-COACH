Imports System.Net
Imports System.Net.Http
Imports System.Threading

Namespace Ai

    ''' <summary>Come è andata la prova di una chiave API.</summary>
    ''' <remarks>
    ''' Nasce il 2026-08-27, dalla revisione del giro D. Fino alla 1.0 la finestra della
    ''' chiave dichiarava di <b>non</b> provarla, con due ragioni buone: costerebbe una
    ''' chiamata, e non distinguerebbe una chiave sbagliata da una rete che non c'è. La
    ''' prima ragione è caduta — l'elenco dei modelli non consuma token —, la seconda non
    ''' era vera: l'API risponde <c>401</c> a una chiave rifiutata e non risponde affatto
    ''' quando la rete manca, e sono due cose diversissime da dire a chi sta entrando.
    ''' </remarks>
    Public NotInheritable Class EsitoProva

        Public Sub New(riuscita As Boolean, causa As CausaErroreAi)
            Me.Riuscita = riuscita
            Me.Causa = causa
        End Sub

        ''' <summary>Se la chiave è stata accettata dall'API.</summary>
        Public ReadOnly Property Riuscita As Boolean

        ''' <summary>Perché non ha funzionato; senza significato quando è riuscita.</summary>
        Public ReadOnly Property Causa As CausaErroreAi

        ''' <summary>La riga da mostrare all'utente, già pronta.</summary>
        Public ReadOnly Property Messaggio As String
            Get
                Return ProvaChiave.Spiega(Riuscita, Causa)
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Prova una chiave API senza spendere token: chiede all'API l'elenco dei modelli,
    ''' che è la chiamata più leggera che esista e che l'autenticazione la controlla lo
    ''' stesso.
    ''' </summary>
    ''' <remarks>
    ''' Sta fuori da <see cref="ClientClaude"/> perché serve <b>prima</b> di lui: nel
    ''' momento in cui si prova, quella chiave non è ancora stata salvata e nessun client
    ''' è stato costruito con lei.
    ''' </remarks>
    Public NotInheritable Class ProvaChiave

        ''' <summary>L'elenco dei modelli: la chiamata che non consuma niente.</summary>
        Public Const Indirizzo As String = "https://api.anthropic.com/v1/models?limit=1"

        ''' <summary>Quanto si aspetta una risposta prima di dire che la rete non c'è.</summary>
        Public Shared ReadOnly Attesa As TimeSpan = TimeSpan.FromSeconds(20)

        Private Sub New()
        End Sub

        ''' <summary>Prova la chiave e dice com'è andata. Non solleva: l'esito è il valore.</summary>
        ''' <param name="chiave">La chiave da provare, così com'è stata digitata.</param>
        ''' <param name="messaggero">Il trasporto, che il banco sostituisce con un finto.</param>
        Public Shared Async Function ProvaAsync(chiave As String,
                                                Optional messaggero As HttpMessageHandler = Nothing,
                                                Optional annulla As CancellationToken = Nothing) As Task(Of EsitoProva)

            If String.IsNullOrWhiteSpace(chiave) Then Return New EsitoProva(False, CausaErroreAi.Chiave)

            Dim http As HttpClient = If(messaggero Is Nothing,
                                        New HttpClient(),
                                        New HttpClient(messaggero, disposeHandler:=False))
            Try
                http.Timeout = Timeout.InfiniteTimeSpan

                Using tempo As New CancellationTokenSource(Attesa)
                    Using insieme As CancellationTokenSource =
                        CancellationTokenSource.CreateLinkedTokenSource(tempo.Token, annulla)

                        Using richiesta As New HttpRequestMessage(HttpMethod.Get, Indirizzo)

                            richiesta.Headers.Add("x-api-key", chiave.Trim())
                            richiesta.Headers.Add("anthropic-version", ClientClaude.VersioneApi)

                            Using risposta As HttpResponseMessage =
                                Await http.SendAsync(richiesta, insieme.Token).ConfigureAwait(False)

                                Return DallaRisposta(risposta.StatusCode)

                            End Using

                        End Using

                    End Using
                End Using

            Catch ex As OperationCanceledException When Not annulla.IsCancellationRequested
                Return New EsitoProva(False, CausaErroreAi.Timeout)
            Catch ex As HttpRequestException
                Return New EsitoProva(False, CausaErroreAi.Rete)
            Catch ex As IO.IOException
                Return New EsitoProva(False, CausaErroreAi.Rete)
            Finally
                http.Dispose()
            End Try

        End Function

        ''' <summary>Che cosa dice uno stato HTTP a proposito della chiave.</summary>
        Public Shared Function DallaRisposta(stato As HttpStatusCode) As EsitoProva

            Dim numero As Integer = CInt(stato)

            If numero >= 200 AndAlso numero < 300 Then Return New EsitoProva(True, CausaErroreAi.Rete)
            If numero = 401 OrElse numero = 403 Then Return New EsitoProva(False, CausaErroreAi.Chiave)
            If numero = 429 Then Return New EsitoProva(False, CausaErroreAi.Limite)
            If numero >= 500 Then Return New EsitoProva(False, CausaErroreAi.Servizio)

            Return New EsitoProva(False, CausaErroreAi.Richiesta)

        End Function

        ''' <summary>
        ''' La riga da mostrare. Le cause si distinguono davvero: «la chiave non va bene» e
        ''' «non riesco a raggiungere il servizio» portano a due gesti diversi, e dirle
        ''' allo stesso modo manderebbe l'utente a cercare la chiave nuova che non serve.
        ''' </summary>
        Public Shared Function Spiega(riuscita As Boolean, causa As CausaErroreAi) As String

            If riuscita Then Return "La chiave funziona."

            Select Case causa
                Case CausaErroreAi.Chiave
                    Return "Questa chiave non è stata accettata: controlla di averla copiata per intero."
                Case CausaErroreAi.Rete
                    Return "Non riesco a raggiungere il servizio: controlla la connessione. La chiave non è stata provata."
                Case CausaErroreAi.Timeout
                    Return "Il servizio non ha risposto in tempo. La chiave non è stata provata."
                Case CausaErroreAi.Limite
                    Return "Il servizio ha chiesto di riprovare più tardi: la chiave sembra buona, ma adesso non posso confermarlo."
                Case CausaErroreAi.Servizio
                    Return "Il servizio ha un problema suo, non la tua chiave. Riprova fra un po'."
                Case Else
                    Return "Non sono riuscita a provare la chiave."
            End Select

        End Function

    End Class

End Namespace
