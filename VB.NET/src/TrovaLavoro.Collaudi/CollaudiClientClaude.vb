Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi del client dell'AI (cap. 02.5). Girano <b>senza rete</b>: al posto
    ''' dell'API c'è un gestore HTTP finto che risponde ciò che il collaudo gli dice.
    ''' Verificano le tre cose che contano — che la richiesta sia identica a quella del
    ''' prototipo, che le fini anomale non passino inosservate, e che si ritenti una
    ''' volta sola e solo quando ha senso.
    ''' </summary>
    <TestClass>
    Public Class CollaudiClientClaude

        ''' <summary>Una risposta come quella vera, ridotta all'osso.</summary>
        Private Shared Function Buona() As String
            Return "{""model"":""claude-sonnet-4-6"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":""{\""nome\"":\""Mirco\""}""}]," &
                   """usage"":{""input_tokens"":120,""output_tokens"":42}}"
        End Function

        Private Shared Function ClientDiProva(finta As ApiFinta, Optional modelli As Modelli = Nothing) As ClientClaude
            Dim client As New ClientClaude("chiave-di-prova", modelli, finta)
            client.Pausa = TimeSpan.Zero   ' i collaudi non stanno ad aspettare davvero
            Return client
        End Function

        ' ------------------------------------------------------------------
        ' Il corpo della richiesta
        ' ------------------------------------------------------------------

        <TestMethod>
        Public Sub IlCorpoEQuelloDelPrototipo()
            ' Confronto char-by-char con la forma che manda server.js: model,
            ' max_tokens, un solo messaggio user. Niente altro.
            Dim corpo As JsonObject = ClientClaude.CorpoRichiesta(
                New ModelloConcreto With {.Id = "claude-sonnet-4-6"}, 4000, JsonValue.Create("ciao"))

            Assert.AreEqual(
                "{""model"":""claude-sonnet-4-6"",""max_tokens"":4000," &
                """messages"":[{""role"":""user"",""content"":""ciao""}]}",
                corpo.ToJsonString(), "il corpo deve essere quello del prototipo")
        End Sub

        <TestMethod>
        Public Sub SenzaInterruttoreNonSiParlaDiRagionamento()
            ' Su Sonnet 4.6 il ragionamento è già spento: tacere tiene la richiesta
            ' identica a quella del prototipo.
            Dim corpo As JsonObject = ClientClaude.CorpoRichiesta(
                New ModelloConcreto With {.Id = "claude-sonnet-4-6"}, 1500, JsonValue.Create("ciao"))

            Assert.IsFalse(corpo.ContainsKey("thinking"), "il campo non deve proprio esserci")
            Assert.IsFalse(corpo.ContainsKey("temperature"), "il prototipo non manda temperature")
        End Sub

        <TestMethod>
        Public Sub ConLInterruttoreSpentoSiDichiaraSpento()
            ' È l'interruttore da accendere al salto su Sonnet 5, dove il ragionamento
            ' è attivo di default e max_tokens limita ragionamento e risposta insieme.
            Dim corpo As JsonObject = ClientClaude.CorpoRichiesta(
                New ModelloConcreto With {.Id = "claude-sonnet-5", .RagionamentoEsteso = False},
                4000, JsonValue.Create("ciao"))

            Assert.AreEqual("disabled", corpo("thinking")("type").ToString(), "thinking.type")
        End Sub

        <TestMethod>
        Public Sub ConLInterruttoreAccesoSiChiedeAdattivo()
            Dim corpo As JsonObject = ClientClaude.CorpoRichiesta(
                New ModelloConcreto With {.Id = "claude-sonnet-5", .RagionamentoEsteso = True},
                4000, JsonValue.Create("ciao"))

            Assert.AreEqual("adaptive", corpo("thinking")("type").ToString(), "thinking.type")
        End Sub

        <TestMethod>
        Public Sub IlContenutoPuoEssereUnElencoDiBlocchi()
            ' La trascrizione di un PDF manda un blocco document più un'istruzione.
            Dim blocchi As New JsonArray(
                New JsonObject From {{"type", "text"}, {"text", "Trascrivi"}})

            Dim corpo As JsonObject = ClientClaude.CorpoRichiesta(
                New ModelloConcreto With {.Id = "claude-haiku-4-5"}, 4000, blocchi)

            Assert.AreEqual(
                "{""model"":""claude-haiku-4-5"",""max_tokens"":4000,""messages"":" &
                "[{""role"":""user"",""content"":[{""type"":""text"",""text"":""Trascrivi""}]}]}",
                corpo.ToJsonString(), "il contenuto a blocchi deve passare così com'è")
        End Sub

        <TestMethod>
        Public Sub UnLimiteDiTokenAssenteSiFermaSubito()
            Assert.Throws(Of ArgumentOutOfRangeException)(
                Sub() ClientClaude.CorpoRichiesta(
                    New ModelloConcreto With {.Id = "claude-haiku-4-5"}, 0, JsonValue.Create("ciao")),
                "un limite a zero doveva sollevare")
        End Sub

        ' ------------------------------------------------------------------
        ' La lettura della risposta
        ' ------------------------------------------------------------------

        <TestMethod>
        Public Sub LeggeTestoEConteggi()
            Dim r As RispostaAi = ClientClaude.InterpretaRisposta(Buona())

            Assert.AreEqual("{""nome"":""Mirco""}", r.Testo, "il testo del modello")
            Assert.AreEqual("end_turn", r.MotivoFine, "motivo della fine")
            Assert.AreEqual("claude-sonnet-4-6", r.Modello, "modello che ha risposto")
            Assert.AreEqual(120, r.TokenIngresso, "token in ingresso")
            Assert.AreEqual(42, r.TokenUscita, "token in uscita")
        End Sub

        <TestMethod>
        Public Sub UnaRispostaTroncataLoDiceInChiaro()
            ' Il rischio del salto a Sonnet 5: la risposta si ferma contro il limite e
            ' arriva monca. Senza questo controllo si scoprirebbe a valle, come JSON
            ' invalido, senza sapere perché.
            Dim troncata As String =
                "{""stop_reason"":""max_tokens"",""content"":[{""type"":""text"",""text"":""{\""nome\"": ""}]}"

            Dim errore As ErroreAi = Assert.Throws(Of ErroreAi)(
                Sub() ClientClaude.InterpretaRisposta(troncata),
                "una risposta troncata doveva sollevare")

            Assert.AreEqual(CausaErroreAi.Troncata, errore.Causa, "causa")
            Assert.IsFalse(errore.Ritentabile, "riprovare darebbe di nuovo lo stesso troncamento")
        End Sub

        <TestMethod>
        Public Sub UnRifiutoDelModelloDiventaUnErrore()
            Dim rifiuto As String = "{""stop_reason"":""refusal"",""content"":[]}"

            Dim errore As ErroreAi = Assert.Throws(Of ErroreAi)(
                Sub() ClientClaude.InterpretaRisposta(rifiuto),
                "un rifiuto doveva sollevare")

            Assert.AreEqual(CausaErroreAi.Rifiuto, errore.Causa, "causa")
        End Sub

        <TestMethod>
        Public Sub PrendeIlPrimoBloccoDiTesto()
            ' Col ragionamento acceso il blocco in posizione zero è il ragionamento:
            ' il prototipo prende lo zero, noi cerchiamo per tipo.
            Dim conRagionamento As String =
                "{""stop_reason"":""end_turn"",""content"":[" &
                "{""type"":""thinking"",""thinking"":""...""}," &
                "{""type"":""text"",""text"":""la risposta""}]}"

            Assert.AreEqual("la risposta", ClientClaude.InterpretaRisposta(conRagionamento).Testo)
        End Sub

        <TestMethod>
        Public Sub UnaRispostaSenzaTestoNonPassaInosservata()
            Assert.Throws(Of ErroreAi)(
                Sub() ClientClaude.InterpretaRisposta("{""stop_reason"":""end_turn"",""content"":[]}"),
                "senza blocchi di testo doveva sollevare")

            Assert.Throws(Of ErroreAi)(
                Sub() ClientClaude.InterpretaRisposta("non sono JSON"),
                "una risposta non-JSON doveva sollevare")
        End Sub

        ' ------------------------------------------------------------------
        ' La chiamata: intestazioni, errori, ritentativo
        ' ------------------------------------------------------------------

        <TestMethod>
        Public Async Function MandaChiaveEVersioneComeIlPrototipo() As Task
            Dim finta As New ApiFinta(New Passo With {.Stato = 200, .Corpo = Buona()})

            Using client As ClientClaude = ClientDiProva(finta)
                Dim r As RispostaAi = Await client.ChiediAsync(
                    Modelli.Ragionamento, JsonValue.Create("ciao"), 4000)

                Assert.AreEqual("{""nome"":""Mirco""}", r.Testo, "testo")
            End Using

            Assert.AreEqual("chiave-di-prova", finta.UltimaChiave, "x-api-key")
            Assert.AreEqual(ClientClaude.VersioneApi, finta.UltimaVersione, "anthropic-version")
            Assert.Contains("""model"":""claude-sonnet-4-6""", finta.UltimoCorpo,
                            "deve usare il modello del livello richiesto")
        End Function

        <TestMethod>
        Public Async Function RitentaUnaVoltaSulGuastoTemporaneo() As Task
            ' Un 500 è un inciampo di passaggio: si riprova una volta, e basta.
            Dim finta As New ApiFinta(
                New Passo With {.Stato = 500, .Corpo = "{""error"":""overloaded""}"},
                New Passo With {.Stato = 200, .Corpo = Buona()})

            Using client As ClientClaude = ClientDiProva(finta)
                Dim r As RispostaAi = Await client.ChiediAsync(
                    Modelli.Semplice, JsonValue.Create("ciao"), 1500)

                Assert.AreEqual("{""nome"":""Mirco""}", r.Testo, "la seconda volta è andata")
            End Using

            Assert.AreEqual(2, finta.Chiamate, "un tentativo più un ritentativo")
        End Function

        <TestMethod>
        Public Async Function RitentaAncheSuTroppeRichieste() As Task
            Dim finta As New ApiFinta(
                New Passo With {.Stato = 429, .Corpo = "{}", .RetryAfter = TimeSpan.FromSeconds(0)},
                New Passo With {.Stato = 200, .Corpo = Buona()})

            Using client As ClientClaude = ClientDiProva(finta)
                Await client.ChiediAsync(Modelli.Semplice, JsonValue.Create("ciao"), 1500)
            End Using

            Assert.AreEqual(2, finta.Chiamate, "un tentativo più un ritentativo")
        End Function

        <TestMethod>
        Public Async Function NonRitentaSuUnErroreNostro() As Task
            ' Un 400 riprovato darebbe esattamente lo stesso 400.
            Dim finta As New ApiFinta(
                New Passo With {.Stato = 400, .Corpo = "{""error"":""max_tokens too large""}"},
                New Passo With {.Stato = 200, .Corpo = Buona()})

            Using client As ClientClaude = ClientDiProva(finta)
                Try
                    Await client.ChiediAsync(Modelli.Semplice, JsonValue.Create("ciao"), 1500)
                    Assert.Fail("un 400 doveva sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Richiesta, ex.Causa, "causa")
                    Assert.Contains("max_tokens too large", ex.Message,
                                    "il dettaglio dell'API serve a capire cosa è successo")
                End Try
            End Using

            Assert.AreEqual(1, finta.Chiamate, "nessun ritentativo")
        End Function

        <TestMethod>
        Public Async Function UnaChiaveRifiutataSiRiconosce() As Task
            Dim finta As New ApiFinta(New Passo With {.Stato = 401, .Corpo = "{""error"":""invalid x-api-key""}"})

            Using client As ClientClaude = ClientDiProva(finta)
                Try
                    Await client.ChiediAsync(Modelli.Semplice, JsonValue.Create("ciao"), 1500)
                    Assert.Fail("un 401 doveva sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Chiave, ex.Causa, "causa")
                End Try
            End Using

            Assert.AreEqual(1, finta.Chiamate, "una chiave sbagliata resta sbagliata")
        End Function

        <TestMethod>
        Public Async Function DueGuastiDiFilaArrivanoAllUtente() As Task
            Dim finta As New ApiFinta(
                New Passo With {.Stato = 503, .Corpo = "{}"},
                New Passo With {.Stato = 503, .Corpo = "{}"})

            Using client As ClientClaude = ClientDiProva(finta)
                Try
                    Await client.ChiediAsync(Modelli.Semplice, JsonValue.Create("ciao"), 1500)
                    Assert.Fail("due guasti di fila dovevano sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Servizio, ex.Causa, "causa")
                End Try
            End Using

            Assert.AreEqual(2, finta.Chiamate, "due tentativi, non tre")
        End Function

        <TestMethod>
        Public Async Function LaReteCadutaSiDiceInItaliano() As Task
            Dim finta As New ApiFinta(
                New Passo With {.Eccezione = New HttpRequestException("no such host")},
                New Passo With {.Eccezione = New HttpRequestException("no such host")})

            Using client As ClientClaude = ClientDiProva(finta)
                Try
                    Await client.ChiediAsync(Modelli.Semplice, JsonValue.Create("ciao"), 1500)
                    Assert.Fail("la rete caduta doveva sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Rete, ex.Causa, "causa")
                    Assert.Contains("connessione", ex.Message, "il messaggio è per l'utente")
                End Try
            End Using
        End Function

        <TestMethod>
        Public Async Function LAttesaScadutaDiventaUnTimeout() As Task
            Dim finta As New ApiFinta(
                New Passo With {.Ritardo = TimeSpan.FromSeconds(30), .Stato = 200, .Corpo = Buona()},
                New Passo With {.Ritardo = TimeSpan.FromSeconds(30), .Stato = 200, .Corpo = Buona()})

            Using client As ClientClaude = ClientDiProva(finta)
                client.TempoMassimo = TimeSpan.FromMilliseconds(50)
                Try
                    Await client.ChiediAsync(Modelli.Semplice, JsonValue.Create("ciao"), 1500)
                    Assert.Fail("l'attesa scaduta doveva sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Timeout, ex.Causa, "causa")
                End Try
            End Using
        End Function

        <TestMethod>
        Public Sub LAttesaCresceConIlLimiteDiTokenDelPrompt()
            ' Dal Pool 1.03 i limiti sono alti perché un CV grande non venga troncato
            ' (CHANGELOG del pool). Senza streaming, un'attesa fissa trasformerebbe quel
            ' limite in un timeout: nessuna risposta invece di una troncata e dichiarata.
            Using client As ClientClaude = ClientDiProva(New ApiFinta())

                client.TempoMassimo = TimeSpan.FromSeconds(120)

                ' Fino al limite di una risposta normale l'attesa non cambia: è la
                ' promessa fatta ai turni del dialogo, che non si possono annullare.
                Assert.AreEqual(TimeSpan.FromSeconds(120), client.AttesaPer(1500), "un turno breve")
                Assert.AreEqual(TimeSpan.FromSeconds(120),
                                client.AttesaPer(ClientClaude.TokenDiRiferimento), "al limite esatto")

                ' Oltre, cresce in proporzione.
                Assert.AreEqual(TimeSpan.FromSeconds(480), client.AttesaPer(16000), "un CV mirato")
                Assert.AreEqual(TimeSpan.FromSeconds(960), client.AttesaPer(32000), "un PDF da trascrivere")

            End Using
        End Sub

        <TestMethod>
        Public Async Function LAnnullamentoDellUtenteNonEUnErrore() As Task
            ' Il pulsante Annulla (cap. 02.6) non deve travestirsi da guasto.
            Dim finta As New ApiFinta(
                New Passo With {.Ritardo = TimeSpan.FromSeconds(30), .Stato = 200, .Corpo = Buona()})

            Using gettone As New CancellationTokenSource()
                Using client As ClientClaude = ClientDiProva(finta)
                    Dim chiamata As Task(Of RispostaAi) = client.ChiediAsync(
                        Modelli.Semplice, JsonValue.Create("ciao"), 1500, gettone.Token)

                    gettone.Cancel()

                    Try
                        Await chiamata
                        Assert.Fail("l'annullamento doveva interrompere la chiamata")
                    Catch ex As OperationCanceledException
                        ' È esattamente quello che deve succedere.
                    End Try
                End Using
            End Using
        End Function

        <TestMethod>
        Public Sub UnaChiaveVuotaSiFermaSubito()
            Assert.Throws(Of ErroreAi)(
                Sub()
                    Dim c As New ClientClaude("   ")
                    c.Dispose()
                End Sub,
                "una chiave vuota doveva sollevare")
        End Sub

        <TestMethod>
        Public Sub LaChiaveSiLeggeDallAmbiente()
            Dim precedente As String = Environment.GetEnvironmentVariable(ClientClaude.NomeVariabileChiave)
            Try
                Environment.SetEnvironmentVariable(ClientClaude.NomeVariabileChiave, "  sk-di-prova  ")
                Assert.AreEqual("sk-di-prova", ClientClaude.ChiaveDaAmbiente(), "letta e ripulita")

                Environment.SetEnvironmentVariable(ClientClaude.NomeVariabileChiave, Nothing)
                Dim errore As ErroreAi = Assert.Throws(Of ErroreAi)(
                    Sub() ClientClaude.ChiaveDaAmbiente(),
                    "senza variabile doveva sollevare")
                Assert.AreEqual(CausaErroreAi.Chiave, errore.Causa, "causa")
                Assert.Contains(ClientClaude.NomeVariabileChiave, errore.Message,
                                "il messaggio deve dire quale variabile definire")
            Finally
                Environment.SetEnvironmentVariable(ClientClaude.NomeVariabileChiave, precedente)
            End Try
        End Sub

    End Class

    ''' <summary>Cosa deve rispondere l'API finta a un tentativo.</summary>
    Friend Class Passo
        Public Property Stato As Integer = 200
        Public Property Corpo As String = "{}"
        Public Property Eccezione As Exception
        Public Property Ritardo As TimeSpan
        Public Property RetryAfter As TimeSpan?
    End Class

    ''' <summary>
    ''' Un'API finta: risponde ciò che il collaudo le ha messo in fila e tiene il conto
    ''' delle chiamate ricevute. È il modo per collaudare timeout e ritentativi senza
    ''' rete, senza chiave e senza aspettare davvero.
    ''' </summary>
    Friend Class ApiFinta
        Inherits HttpMessageHandler

        Private ReadOnly _passi As Queue(Of Passo)

        ''' <summary>Quante volte è stata chiamata: è il conto dei tentativi.</summary>
        Public Property Chiamate As Integer

        Public Property UltimoCorpo As String
        Public Property UltimaChiave As String
        Public Property UltimaVersione As String

        Public Sub New(ParamArray passi As Passo())
            _passi = New Queue(Of Passo)(passi)
        End Sub

        Protected Overrides Async Function SendAsync(richiesta As HttpRequestMessage,
                                                     annulla As CancellationToken) As Task(Of HttpResponseMessage)

            Chiamate += 1

            If richiesta.Content IsNot Nothing Then
                UltimoCorpo = Await richiesta.Content.ReadAsStringAsync(annulla).ConfigureAwait(False)
            End If

            Dim valori As IEnumerable(Of String) = Nothing
            If richiesta.Headers.TryGetValues("x-api-key", valori) Then UltimaChiave = valori.First()
            If richiesta.Headers.TryGetValues("anthropic-version", valori) Then UltimaVersione = valori.First()

            Dim passo As Passo = If(_passi.Count > 0, _passi.Dequeue(), New Passo())

            If passo.Ritardo > TimeSpan.Zero Then
                Await Task.Delay(passo.Ritardo, annulla).ConfigureAwait(False)
            End If

            If passo.Eccezione IsNot Nothing Then Throw passo.Eccezione

            Dim risposta As New HttpResponseMessage(CType(passo.Stato, HttpStatusCode)) With {
                .Content = New StringContent(If(passo.Corpo, "{}"), Encoding.UTF8, "application/json")}

            If passo.RetryAfter.HasValue Then
                risposta.Headers.RetryAfter = New RetryConditionHeaderValue(passo.RetryAfter.Value)
            End If

            Return risposta

        End Function

    End Class

End Namespace
