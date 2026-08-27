Imports System.Globalization
Imports System.Net
Imports System.Net.Http
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading

Namespace Motore

    ''' <summary>Come sta questa copia rispetto all'ultima pubblicata.</summary>
    Public Enum StatoVersione
        ''' <summary>È l'ultima: non c'è niente da scaricare.</summary>
        Aggiornata
        ''' <summary>Ne esiste una più recente.</summary>
        CeNEUnaNuova
        ''' <summary>Questa è più avanti di quella pubblicata: capita a chi la costruisce.</summary>
        PiuAvantiDelPubblicato
        ''' <summary>Non si è potuto sapere.</summary>
        NonSiSa
    End Enum

    ''' <summary>Com'è andata la domanda «ce n'è una nuova?».</summary>
    Public NotInheritable Class EsitoVersione

        Private Sub New(stato As StatoVersione, pubblicata As String, messaggio As String)
            Me.Stato = stato
            Me.Pubblicata = pubblicata
            Me.Messaggio = messaggio
        End Sub

        Friend Shared Function Confrontata(stato As StatoVersione, pubblicata As String,
                                           messaggio As String) As EsitoVersione
            Return New EsitoVersione(stato, pubblicata, messaggio)
        End Function

        Friend Shared Function NonSiSa(messaggio As String) As EsitoVersione
            Return New EsitoVersione(StatoVersione.NonSiSa, Nothing, messaggio)
        End Function

        Public ReadOnly Property Stato As StatoVersione

        ''' <summary>Il numero dell'ultima pubblicata; <c>Nothing</c> se non si è saputo.</summary>
        Public ReadOnly Property Pubblicata As String

        ''' <summary>La riga da mostrare, già pronta.</summary>
        Public ReadOnly Property Messaggio As String

    End Class

    ''' <summary>
    ''' Chiede a GitHub qual è l'ultima versione pubblicata (cap. 13.8).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Solo quando lo si chiede.</b> Il cap. 11.2 promette «niente telemetria,
    ''' niente aggiornamenti automatici silenziosi», e questa è l'unica chiamata del
    ''' programma che non va all'API di Anthropic: parte quando l'utente preme il bottone
    ''' in «Informazioni», mai all'avvio e mai da sola. Chi non lo preme non manda niente
    ''' a nessuno, ed è la ragione per cui la promessa resta vera anche adesso.</para>
    ''' <para><b>Che cosa esce.</b> Una richiesta HTTP a <c>api.github.com</c> senza
    ''' credenziali e senza nulla dell'utente: GitHub vede un indirizzo IP e il nome del
    ''' programma, come lo vedrebbe aprendo la pagina delle release col browser. È scritto
    ''' anche nell'informativa dentro l'applicazione, perché una cosa che esce dal PC si
    ''' dichiara prima, non dopo.</para>
    ''' <para><b>Non aggiorna niente.</b> Dice che c'è una versione nuova e dove sta; a
    ''' scaricarla e a sostituire l'eseguibile ci pensa la persona (cap. 13.8). Un
    ''' programma che si riscrive da solo è un programma di cui bisogna fidarsi di più di
    ''' quanto questo chieda.</para>
    ''' </remarks>
    Public NotInheritable Class ControlloVersione

        ''' <summary>L'ultima release pubblicata del repository.</summary>
        Public Const Indirizzo As String =
            "https://api.github.com/repos/mirco-parenti/AI-CV-COACH/releases/latest"

        ''' <summary>La pagina che si apre per scaricarla.</summary>
        Public Const PaginaDelleRelease As String =
            "https://github.com/mirco-parenti/AI-CV-COACH/releases/latest"

        ''' <summary>GitHub rifiuta le richieste che non dicono chi sono.</summary>
        Private Const ChiSono As String = "TrovaLavoro"

        ''' <summary>Quanto si aspetta prima di dire che non si è saputo.</summary>
        Public Shared ReadOnly Attesa As TimeSpan = TimeSpan.FromSeconds(15)

        Private Sub New()
        End Sub

        ''' <summary>Chiede e risponde. Non solleva: l'esito è il valore.</summary>
        ''' <param name="quiCE">La versione di questa copia; se omessa, quella compilata.</param>
        ''' <param name="messaggero">Il trasporto, che il banco sostituisce con un finto.</param>
        Public Shared Async Function ChiediAsync(Optional quiCE As String = Nothing,
                                                 Optional messaggero As HttpMessageHandler = Nothing,
                                                 Optional annulla As CancellationToken = Nothing) As Task(Of EsitoVersione)

            Dim http As HttpClient = If(messaggero Is Nothing,
                                        New HttpClient(),
                                        New HttpClient(messaggero, disposeHandler:=False))
            Try
                http.Timeout = Timeout.InfiniteTimeSpan

                Using tempo As New CancellationTokenSource(Attesa)
                    Using insieme As CancellationTokenSource =
                        CancellationTokenSource.CreateLinkedTokenSource(tempo.Token, annulla)

                        Using richiesta As New HttpRequestMessage(HttpMethod.Get, Indirizzo)

                            richiesta.Headers.Add("Accept", "application/vnd.github+json")
                            richiesta.Headers.Add("User-Agent", $"{ChiSono}/{Versione.Numero}")

                            Using risposta As HttpResponseMessage =
                                Await http.SendAsync(richiesta, insieme.Token).ConfigureAwait(False)

                                ' Un 404 su «releases/latest» non è un guasto: è GitHub che
                                ' dice che di release pubblicate non ce n'è nessuna. Dirlo
                                ' col numero — «il servizio ha risposto 404» — manda a
                                ' cercare un guasto dove non c'è, e capita esattamente a
                                ' chi ha in mano la prima versione (2026-08-27).
                                If risposta.StatusCode = HttpStatusCode.NotFound Then
                                    Return EsitoVersione.NonSiSa(
                                        "Non risulta pubblicata nessuna versione: quella che hai è " &
                                        "l'unica che esiste. Se cerchi il programma altrove, la pagina " &
                                        "delle versioni è quella qui sotto.")
                                End If

                                If Not risposta.IsSuccessStatusCode Then
                                    Return EsitoVersione.NonSiSa(
                                        $"Non sono riuscita a chiederlo (il servizio ha risposto {CInt(risposta.StatusCode)}). " &
                                        "Puoi guardare tu sulla pagina delle versioni.")
                                End If

                                Dim corpo As String =
                                    Await risposta.Content.ReadAsStringAsync(insieme.Token).ConfigureAwait(False)

                                Return Confronta(If(quiCE, Versione.Numero), NumeroPubblicato(corpo))

                            End Using

                        End Using

                    End Using
                End Using

            Catch ex As OperationCanceledException When Not annulla.IsCancellationRequested
                Return EsitoVersione.NonSiSa("Il servizio non ha risposto in tempo: riprova più tardi.")
            Catch ex As HttpRequestException
                Return EsitoVersione.NonSiSa("Non riesco a raggiungere GitHub: controlla la connessione a Internet.")
            Catch ex As IO.IOException
                Return EsitoVersione.NonSiSa("Non riesco a raggiungere GitHub: controlla la connessione a Internet.")
            Finally
                http.Dispose()
            End Try

        End Function

        ''' <summary>
        ''' Il numero di versione dentro la risposta di GitHub: è il nome del tag, tolta
        ''' la «v» che si usa per scriverlo (<c>v1.0</c>).
        ''' </summary>
        Public Shared Function NumeroPubblicato(corpo As String) As String

            Dim radice As JsonObject
            Try
                radice = TryCast(JsonNode.Parse(If(corpo, String.Empty)), JsonObject)
            Catch ex As JsonException
                Return Nothing
            End Try

            Dim tag As String = TryCast(radice?("tag_name"), JsonValue)?.ToString()
            If String.IsNullOrWhiteSpace(tag) Then Return Nothing

            Return tag.Trim().TrimStart("v"c, "V"c)

        End Function

        ''' <summary>
        ''' Come sta questa copia rispetto a quella pubblicata, e cosa dirne.
        ''' </summary>
        ''' <remarks>
        ''' <para>Il confronto è per <b>numeri</b>, non per stringhe: «1.0.000» e «v1.0»
        ''' sono la stessa versione scritta in due posti diversi — la costante di
        ''' <c>Versione.vb</c> e il tag di un rilascio — e confrontandole a caratteri
        ''' direbbero di no. I pezzi che mancano valgono zero, così «1.0» e «1.0.000»
        ''' combaciano.</para>
        ''' <para>Il terzo esito, «più avanti del pubblicato», non è un caso di scuola: è
        ''' quello di chi costruisce il programma, che ha in mano una versione che non è
        ''' ancora stata rilasciata. Dirgli «sei aggiornato» sarebbe falso e dirgli «ce n'è
        ''' una nuova» sarebbe assurdo.</para>
        ''' </remarks>
        Public Shared Function Confronta(quiCE As String, pubblicata As String) As EsitoVersione

            Dim qui As Integer() = Pezzi(quiCE)
            Dim la As Integer() = Pezzi(pubblicata)

            If qui Is Nothing OrElse la Is Nothing Then
                Return EsitoVersione.NonSiSa(
                    "La risposta non aveva la forma che mi aspettavo: puoi guardare tu sulla pagina delle versioni.")
            End If

            Dim verso As Integer = Paragone(qui, la)

            If verso < 0 Then
                Return EsitoVersione.Confrontata(StatoVersione.CeNEUnaNuova, pubblicata,
                    $"C'è la versione {pubblicata}: questa è la {quiCE}. Si scarica dalla pagina delle versioni " &
                    "e si sostituisce l'eseguibile; i tuoi dati restano dove sono.")
            End If

            If verso > 0 Then
                Return EsitoVersione.Confrontata(StatoVersione.PiuAvantiDelPubblicato, pubblicata,
                    $"Questa copia ({quiCE}) è più avanti dell'ultima pubblicata ({pubblicata}).")
            End If

            Return EsitoVersione.Confrontata(StatoVersione.Aggiornata, pubblicata,
                $"È l'ultima versione ({quiCE}): non c'è niente da scaricare.")

        End Function

        ''' <summary>I numeri di una versione, o <c>Nothing</c> se non se ne cavano.</summary>
        Private Shared Function Pezzi(versione As String) As Integer()

            If String.IsNullOrWhiteSpace(versione) Then Return Nothing

            Dim pezzo As String() = versione.Trim().TrimStart("v"c, "V"c).Split("."c)
            Dim numeri As New List(Of Integer)

            For Each uno As String In pezzo
                Dim quanto As Integer
                If Not Integer.TryParse(uno, NumberStyles.Integer, CultureInfo.InvariantCulture, quanto) Then
                    Return Nothing
                End If
                numeri.Add(quanto)
            Next

            Return If(numeri.Count = 0, Nothing, numeri.ToArray())

        End Function

        ''' <summary>Meno di zero se la prima è più vecchia; i pezzi che mancano valgono zero.</summary>
        Private Shared Function Paragone(qui As Integer(), la As Integer()) As Integer

            For posto As Integer = 0 To Math.Max(qui.Length, la.Length) - 1

                Dim a As Integer = If(posto < qui.Length, qui(posto), 0)
                Dim b As Integer = If(posto < la.Length, la(posto), 0)

                If a <> b Then Return a.CompareTo(b)

            Next

            Return 0

        End Function

    End Class

End Namespace
