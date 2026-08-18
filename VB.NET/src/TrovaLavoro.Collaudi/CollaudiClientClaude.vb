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

        ''' <summary>Il client davanti a un'API che risponde a pezzi.</summary>
        Private Shared Function ClientInAscolto(finta As ApiCheFluisce) As ClientClaude
            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero
            Return client
        End Function

        ''' <summary>Un evento come lo manda l'API: nome, dati, riga vuota.</summary>
        Private Shared Function Evento(nome As String, dati As String) As String
            Return $"event: {nome}" & vbLf & $"data: {dati}" & vbLf & vbLf
        End Function

        ''' <summary>L'evento con cui il messaggio comincia: modello e token in ingresso.</summary>
        Private Shared Function Apertura() As String
            Return Evento("message_start",
                          "{""type"":""message_start"",""message"":{""model"":""claude-sonnet-4-6""," &
                          """usage"":{""input_tokens"":120}}}")
        End Function

        ''' <summary>Un pezzo di testo che arriva.</summary>
        Private Shared Function Pezzo(testo As String) As String
            Return Evento("content_block_delta",
                          "{""type"":""content_block_delta"",""index"":0,""delta"":" &
                          "{""type"":""text_delta"",""text"":""" & testo & """}}")
        End Function

        ''' <summary>La chiusura: perché ha smesso, quanto ha scritto, e il sipario.</summary>
        Private Shared Function Chiusura(Optional motivo As String = "end_turn") As String
            Return Evento("message_delta",
                          "{""type"":""message_delta"",""delta"":{""stop_reason"":""" & motivo & """}," &
                          """usage"":{""output_tokens"":42}}") &
                   Evento("message_stop", "{""type"":""message_stop""}")
        End Function

        ''' <summary>I turni di una conversazione, come li passa il brainstorming.</summary>
        Private Shared Function Conversazione(ParamArray testi As String()) As IReadOnlyList(Of TurnoChat)
            Dim turni As New List(Of TurnoChat)
            For i As Integer = 0 To testi.Length - 1
                turni.Add(If(i Mod 2 = 0, TurnoChat.DallUtente(testi(i)), TurnoChat.DallAssistente(testi(i))))
            Next
            Return turni
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
        Public Async Function UnaRispostaCheSiSpezzaInLetturaEUnaCadutaDiRete() As Task
            ' Una connessione che si spezza mentre il corpo scende deve diventare un
            ' errore di rete come gli altri — stesse parole, ritentabile — e non
            ' un'eccezione grezza in faccia all'utente (cap. 02.5). Oggi lo è già, e non
            ' per merito di un Catch scritto apposta: HttpClient scarica tutto il corpo
            ' dentro SendAsync (ResponseContentRead, l'impostazione predefinita), dove la
            ' rete è protetta. Questo collaudo sta qui come sentinella: il giorno in cui
            ' si passerà a ResponseHeadersRead per lo streaming (T4/T7), la lettura del
            ' corpo uscirà scoperta e questo diventerà rosso.
            Dim finta As New ApiCheSiSpezza()

            Using client As New ClientClaude("chiave-di-prova", Nothing, finta)

                client.Pausa = TimeSpan.Zero

                Try
                    Await client.ChiediAsync(Modelli.Semplice, JsonValue.Create("ciao"), 1500)
                    Assert.Fail("il corpo interrotto doveva sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Rete, ex.Causa, "causa")
                    Assert.Contains("connessione", ex.Message, "il messaggio è per l'utente")
                End Try

                Assert.AreEqual(2, finta.Chiamate, "ed è ritentabile, come ogni caduta di rete")

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

        ' ------------------------------------------------------------------
        ' Lo streaming (T7c): la conversazione e la risposta che arriva a pezzi
        ' ------------------------------------------------------------------

        <TestMethod>
        Public Sub UnaConversazioneMetteITurniInFilaEChiedeIlFlusso()
            Dim corpo As JsonObject = ClientClaude.CorpoRichiesta(
                New ModelloConcreto With {.Id = "claude-sonnet-4-6"}, 2000,
                Conversazione("il contesto", "apro io", "e io rispondo"), flusso:=True)

            Assert.AreEqual(
                "{""model"":""claude-sonnet-4-6"",""max_tokens"":2000,""messages"":[" &
                "{""role"":""user"",""content"":""il contesto""}," &
                "{""role"":""assistant"",""content"":""apro io""}," &
                "{""role"":""user"",""content"":""e io rispondo""}],""stream"":true}",
                corpo.ToJsonString(), "i turni in fila, e la richiesta di rispondere man mano")
        End Sub

        <TestMethod>
        Public Sub UnTurnoSoloSenzaFlussoEIlCorpoDiSempre()
            ' La strada nuova non deve cambiare la vecchia di un carattere: la
            ' non-regressione è appoggiata a lei (cap. 14).
            Dim conTurni As JsonObject = ClientClaude.CorpoRichiesta(
                New ModelloConcreto With {.Id = "claude-sonnet-4-6"}, 4000, Conversazione("ciao"))

            Dim comeSempre As JsonObject = ClientClaude.CorpoRichiesta(
                New ModelloConcreto With {.Id = "claude-sonnet-4-6"}, 4000, JsonValue.Create("ciao"))

            Assert.AreEqual(comeSempre.ToJsonString(), conTurni.ToJsonString(), "stesso corpo")
            Assert.IsFalse(conTurni.ContainsKey("stream"), "senza streaming il campo non c'è proprio")
        End Sub

        <TestMethod>
        Public Async Function IPezziArrivanoManManoEPoiCELaRispostaIntera() As Task
            Dim finta As New ApiCheFluisce(New PassoFlusso With {
                .Pezzi = {Apertura(), Pezzo("Guardando "), Pezzo("il tuo profilo"), Chiusura()}})

            Dim visti As New List(Of String)

            Using client As ClientClaude = ClientInAscolto(finta)
                Dim r As RispostaAi = Await client.ChiediInStreamingAsync(
                    Modelli.Ragionamento, Conversazione("il contesto"), 2000, Sub(p) visti.Add(p))

                Assert.HasCount(2, visti, "due pezzi consegnati man mano")
                Assert.AreEqual("Guardando ", visti(0), "il primo appena arrivato")
                Assert.AreEqual("Guardando il tuo profilo", r.Testo, "e alla fine il testo intero")
                Assert.AreEqual("end_turn", r.MotivoFine, "motivo della fine")
                Assert.AreEqual("claude-sonnet-4-6", r.Modello, "modello che ha risposto")
                Assert.AreEqual(120, r.TokenIngresso, "token in ingresso")
                Assert.AreEqual(42, r.TokenUscita, "token in uscita")
            End Using

            Assert.Contains("""stream"":true", finta.UltimoCorpo, "e l'ha chiesto a pezzi")
        End Function

        <TestMethod>
        Public Async Function IlRagionamentoNonSiMostra() As Task
            ' Col ragionamento acceso arrivano anche i suoi pezzi: non sono la risposta,
            ' come nella chiamata sincrona non lo è il blocco «thinking».
            Dim ragiona As String = Evento("content_block_delta",
                "{""type"":""content_block_delta"",""index"":0,""delta"":" &
                "{""type"":""thinking_delta"",""thinking"":""sto pensando""}}")

            Dim finta As New ApiCheFluisce(New PassoFlusso With {
                .Pezzi = {Apertura(), ragiona, Pezzo("la risposta"), Chiusura()}})

            Dim visti As New List(Of String)

            Using client As ClientClaude = ClientInAscolto(finta)
                Dim r As RispostaAi = Await client.ChiediInStreamingAsync(
                    Modelli.Ragionamento, Conversazione("ciao"), 2000, Sub(p) visti.Add(p))

                Assert.AreEqual("la risposta", r.Testo, "solo il testo vero")
                Assert.HasCount(1, visti, "e a video è comparso solo quello")
            End Using
        End Function

        <TestMethod>
        Public Async Function UnErroreDentroIlFlussoSiRiconosceComeGliAltri() As Task
            ' L'API può accorgersi di essere in affanno dopo aver aperto il flusso: per
            ' chi legge è lo stesso guasto di un 529, e si ritenta come sempre — qui
            ' nessun testo era ancora comparso.
            Dim guasto As String = Evento("error",
                "{""type"":""error"",""error"":{""type"":""overloaded_error"",""message"":""Overloaded""}}")

            Dim finta As New ApiCheFluisce(
                New PassoFlusso With {.Pezzi = {Apertura(), guasto}},
                New PassoFlusso With {.Pezzi = {Apertura(), Pezzo("ci siamo"), Chiusura()}})

            Using client As ClientClaude = ClientInAscolto(finta)
                Dim r As RispostaAi = Await client.ChiediInStreamingAsync(
                    Modelli.Ragionamento, Conversazione("ciao"), 2000, Nothing)

                Assert.AreEqual("ci siamo", r.Testo, "la seconda volta è andata")
            End Using

            Assert.AreEqual(2, finta.Chiamate, "un tentativo più un ritentativo")
        End Function

        <TestMethod>
        Public Async Function DopoIlPrimoPezzoNonSiRitentaPiu() As Task
            ' La regola di T7c: appena qualcosa è comparso sotto gli occhi di chi legge,
            ' riprovare vorrebbe dire riscrivere la risposta da capo o cancellargliela.
            Dim finta As New ApiCheFluisce(
                New PassoFlusso With {.Pezzi = {Apertura(), Pezzo("ho cominciato")}, .RompiAllaFine = True},
                New PassoFlusso With {.Pezzi = {Apertura(), Pezzo("e ricomincio"), Chiusura()}})

            Dim visti As New List(Of String)

            Using client As ClientClaude = ClientInAscolto(finta)
                Try
                    Await client.ChiediInStreamingAsync(
                        Modelli.Ragionamento, Conversazione("ciao"), 2000, Sub(p) visti.Add(p))
                    Assert.Fail("il flusso spezzato doveva sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Rete, ex.Causa, "causa")
                End Try
            End Using

            Assert.AreEqual(1, finta.Chiamate, "nessun ritentativo dopo il primo pezzo")
            Assert.AreEqual("ho cominciato", String.Concat(visti), "e quel che era arrivato resta")
        End Function

        <TestMethod>
        Public Async Function PrimaDelPrimoPezzoSiRitentaComeSempre() As Task
            ' Il rovescio della regola: se non è comparso ancora niente, il ritentativo
            ' non ha nessun effetto collaterale e vale la pena farlo.
            Dim finta As New ApiCheFluisce(
                New PassoFlusso With {.Pezzi = {Apertura()}, .RompiAllaFine = True},
                New PassoFlusso With {.Pezzi = {Apertura(), Pezzo("eccomi"), Chiusura()}})

            Using client As ClientClaude = ClientInAscolto(finta)
                Dim r As RispostaAi = Await client.ChiediInStreamingAsync(
                    Modelli.Ragionamento, Conversazione("ciao"), 2000, Nothing)

                Assert.AreEqual("eccomi", r.Testo, "la seconda volta è andata")
            End Using

            Assert.AreEqual(2, finta.Chiamate, "un tentativo più un ritentativo")
        End Function

        <TestMethod>
        Public Async Function UnFlussoCheFinisceSenzaSalutareEUnaCadutaDiRete() As Task
            ' Gli eventi finiscono ma l'AI non ha mai detto «ho finito»: il collegamento
            ' si è spezzato mentre scriveva, e qui la protezione è scritta apposta —
            ' leggendo a pezzi non c'è nessun SendAsync a coprirla.
            Dim finta As New ApiCheFluisce(
                New PassoFlusso With {.Pezzi = {Apertura(), Pezzo("a metà frase")}},
                New PassoFlusso With {.Pezzi = {Apertura(), Pezzo("mai chiesto")}})

            Using client As ClientClaude = ClientInAscolto(finta)
                Try
                    Await client.ChiediInStreamingAsync(
                        Modelli.Ragionamento, Conversazione("ciao"), 2000, Nothing)
                    Assert.Fail("un flusso troncato doveva sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Rete, ex.Causa, "causa")
                    Assert.Contains("interrotto", ex.Message, "il messaggio è per l'utente")
                End Try
            End Using

            Assert.AreEqual(1, finta.Chiamate, "e non si ritenta: il testo era già comparso")
        End Function

        <TestMethod>
        Public Async Function IlSilenzioTroppoLungoDiventaUnAttesaScaduta() As Task
            ' In streaming il metro non è la durata ma il silenzio: una risposta lunga
            ' che continua ad arrivare va benissimo, una connessione morta no.
            Dim finta As New ApiCheFluisce(New PassoFlusso With {
                .Pezzi = {Apertura(), Pezzo("comincio")},
                .AttesaFinale = TimeSpan.FromSeconds(30)})

            Using client As ClientClaude = ClientInAscolto(finta)
                client.SilenzioMassimo = TimeSpan.FromMilliseconds(80)

                Try
                    Await client.ChiediInStreamingAsync(
                        Modelli.Ragionamento, Conversazione("ciao"), 2000, Nothing)
                    Assert.Fail("il silenzio doveva scadere")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Timeout, ex.Causa, "causa")
                End Try
            End Using
        End Function

        <TestMethod>
        Public Async Function UnaRispostaLungaMaVivaNonScadeMai() As Task
            ' Il rovescio del collaudo qui sopra, ed è quello che dimostra la regola: una
            ' risposta che dura **più** del silenzio concesso deve arrivare in fondo,
            ' purché non taccia mai per tutto quel silenzio. Senza questo, il giorno in cui
            ' l'attesa tornasse a essere un tetto complessivo la batteria resterebbe verde.
            Dim finta As New ApiCheFluisce(New PassoFlusso With {
                .Pezzi = {Apertura(), Pezzo("uno "), Pezzo("due "), Pezzo("tre"), Chiusura()},
                .RitardoFraPezzi = TimeSpan.FromMilliseconds(120)})

            Using client As ClientClaude = ClientInAscolto(finta)

                ' Quattro attese da 120 ms: mezzo secondo abbondante in tutto, contro un
                ' secondo di silenzio concesso. Il margine è largo di proposito — la
                ' batteria intera gira in parallelo, e una pausa da 120 ms su una macchina
                ' occupata può allungarsi parecchio: un collaudo che misura il tempo o si
                ' dà spazio o diventa ballerino, e uno ballerino non lo guarda più nessuno.
                client.SilenzioMassimo = TimeSpan.FromMilliseconds(1000)

                Dim r As RispostaAi = Await client.ChiediInStreamingAsync(
                    Modelli.Ragionamento, Conversazione("ciao"), 2000, Nothing)

                Assert.AreEqual("uno due tre", r.Testo, "la risposta è arrivata tutta")
            End Using
        End Function

        <TestMethod>
        Public Async Function UnErroreDopoIlTestoArrivaCosiComEE() As Task
            ' Il caso che chiude il cerchio della regola sul ritentativo: non una caduta
            ' di rete, ma un errore **dichiarato dall'API** dentro il flusso, quando del
            ' testo è già comparso. È ritentabile per natura, e proprio per questo non si
            ' ritenta.
            Dim guasto As String = Evento("error",
                "{""type"":""error"",""error"":{""type"":""overloaded_error"",""message"":""Overloaded""}}")

            Dim finta As New ApiCheFluisce(
                New PassoFlusso With {.Pezzi = {Apertura(), Pezzo("ho cominciato"), guasto}},
                New PassoFlusso With {.Pezzi = {Apertura(), Pezzo("e ricomincio"), Chiusura()}})

            Using client As ClientClaude = ClientInAscolto(finta)
                Try
                    Await client.ChiediInStreamingAsync(
                        Modelli.Ragionamento, Conversazione("ciao"), 2000, Nothing)
                    Assert.Fail("l'errore nel flusso doveva sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Servizio, ex.Causa, "causa")
                    Assert.IsTrue(ex.Ritentabile, "di suo sarebbe ritentabile")
                End Try
            End Using

            Assert.AreEqual(1, finta.Chiamate, "ma non si ritenta: il testo era già a video")
        End Function

        <TestMethod>
        Public Async Function InChatUnaRispostaTroncataNonEUnErrore() As Task
            ' All'opposto della chiamata sincrona, dove il troncamento lascia un JSON
            ' monco: qui il testo arrivato si legge benissimo ed è già sotto gli occhi
            ' dell'utente. Il motivo della fine si porta a casa, e a dirlo è il pannello.
            Dim finta As New ApiCheFluisce(New PassoFlusso With {
                .Pezzi = {Apertura(), Pezzo("questa frase si ferma a"), Chiusura("max_tokens")}})

            Using client As ClientClaude = ClientInAscolto(finta)
                Dim r As RispostaAi = Await client.ChiediInStreamingAsync(
                    Modelli.Ragionamento, Conversazione("ciao"), 2000, Nothing)

                Assert.AreEqual("questa frase si ferma a", r.Testo, "il testo arrivato resta")
                Assert.AreEqual("max_tokens", r.MotivoFine, "ma il motivo si sa")
            End Using
        End Function

        <TestMethod>
        Public Async Function UnaChiaveRifiutataSiRiconosceAncheInStreaming() As Task
            ' Un errore HTTP non arriva mai come flusso di eventi: è una risposta
            ' normale, e si classifica con le stesse parole di sempre.
            Dim finta As New ApiCheFluisce(New PassoFlusso With {
                .Stato = 401, .Corpo = "{""error"":""invalid x-api-key""}"})

            Using client As ClientClaude = ClientInAscolto(finta)
                Try
                    Await client.ChiediInStreamingAsync(
                        Modelli.Ragionamento, Conversazione("ciao"), 2000, Nothing)
                    Assert.Fail("un 401 doveva sollevare")
                Catch ex As ErroreAi
                    Assert.AreEqual(CausaErroreAi.Chiave, ex.Causa, "causa")
                End Try
            End Using

            Assert.AreEqual(1, finta.Chiamate, "una chiave sbagliata resta sbagliata")
        End Function

        <TestMethod>
        Public Async Function ChiInterrompeIlBrainstormingNonRiceveUnErrore() As Task
            ' A differenza del turno del dialogo guidato, qui interrompere si può
            ' (cap. 02.6): non c'è nessuna mossa a metà, solo una risposta più corta.
            Dim finta As New ApiCheFluisce(New PassoFlusso With {
                .Pezzi = {Apertura(), Pezzo("sto scrivendo")},
                .AttesaFinale = TimeSpan.FromSeconds(30)})

            Using gettone As New CancellationTokenSource()
                Using client As ClientClaude = ClientInAscolto(finta)

                    Dim visti As New List(Of String)
                    Dim chiamata As Task(Of RispostaAi) = client.ChiediInStreamingAsync(
                        Modelli.Ragionamento, Conversazione("ciao"), 2000,
                        Sub(p)
                            visti.Add(p)
                            gettone.Cancel()
                        End Sub, gettone.Token)

                    Try
                        Await chiamata
                        Assert.Fail("l'interruzione doveva fermare il flusso")
                    Catch ex As OperationCanceledException
                        ' È esattamente quello che deve succedere.
                    End Try

                    Assert.AreEqual("sto scrivendo", String.Concat(visti), "e quel che era arrivato resta")

                End Using
            End Using
        End Function

        <TestMethod>
        Public Sub UnaConversazioneVuotaNonSiManda()
            Assert.Throws(Of ArgumentException)(
                Sub() ClientClaude.CorpoRichiesta(
                    New ModelloConcreto With {.Id = "claude-sonnet-4-6"}, 2000,
                    New List(Of TurnoChat)()),
                "una richiesta senza messaggi doveva sollevare")
        End Sub

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

    ''' <summary>
    ''' Un'API che risponde 200 e poi si spezza mentre il corpo si legge: è la caduta di
    ''' connessione che arriva dopo le intestazioni, quella che <see cref="ApiFinta"/>
    ''' non sa simulare perché consegna il corpo tutto insieme.
    ''' </summary>
    Friend Class ApiCheSiSpezza
        Inherits HttpMessageHandler

        ''' <summary>Quante volte è stata chiamata: è il conto dei tentativi.</summary>
        Public Property Chiamate As Integer

        Protected Overrides Function SendAsync(richiesta As HttpRequestMessage,
                                               annulla As CancellationToken) As Task(Of HttpResponseMessage)

            Chiamate += 1

            Return Task.FromResult(New HttpResponseMessage(HttpStatusCode.OK) With {
                .Content = New StreamContent(New FlussoCheSiSpezza())})

        End Function

    End Class

    ''' <summary>Un flusso che al primo tentativo di lettura non c'è più.</summary>
    Friend Class FlussoCheSiSpezza
        Inherits IO.Stream

        Public Overrides Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            Throw New IO.IOException("La connessione si è chiusa a metà risposta.")
        End Function

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Length As Long
            Get
                Throw New NotSupportedException()
            End Get
        End Property

        Public Overrides Property Position As Long
            Get
                Throw New NotSupportedException()
            End Get
            Set(value As Long)
                Throw New NotSupportedException()
            End Set
        End Property

        Public Overrides Sub Flush()
        End Sub

        Public Overrides Function Seek(offset As Long, origin As IO.SeekOrigin) As Long
            Throw New NotSupportedException()
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New NotSupportedException()
        End Sub

        Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
            Throw New NotSupportedException()
        End Sub

    End Class

    ''' <summary>Cosa deve consegnare l'API finta che risponde a pezzi.</summary>
    Friend Class PassoFlusso

        ''' <summary>Lo stato HTTP: diverso da 200 vuol dire risposta normale, non flusso.</summary>
        Public Property Stato As Integer = 200

        ''' <summary>I pezzi di flusso, nell'ordine in cui arrivano sul filo.</summary>
        Public Property Pezzi As String() = Array.Empty(Of String)()

        ''' <summary>Il corpo di una risposta di errore, che non è mai un flusso.</summary>
        Public Property Corpo As String = "{}"

        ''' <summary>Se il collegamento cade dopo l'ultimo pezzo consegnato.</summary>
        Public Property RompiAllaFine As Boolean

        ''' <summary>
        ''' Quanto passa fra un pezzo e il successivo. Serve a collaudare che l'attesa si
        ''' <b>riarmi</b>: senza, una risposta lunga si potrebbe consegnare solo tutta
        ''' d'un fiato, e un tetto complessivo travestito da silenzio non si vedrebbe.
        ''' </summary>
        Public Property RitardoFraPezzi As TimeSpan

        ''' <summary>Quanto tace dopo l'ultimo pezzo, per far scadere il silenzio.</summary>
        Public Property AttesaFinale As TimeSpan

    End Class

    ''' <summary>
    ''' Un'API che risponde <b>a pezzi</b>, come fa quella vera in streaming. Serve
    ''' perché <see cref="ApiFinta"/> consegna il corpo tutto insieme: con lei un flusso
    ''' sarebbe collaudato solo per finta, e la cosa da collaudare è proprio che i pezzi
    ''' arrivino uno alla volta.
    ''' </summary>
    Friend Class ApiCheFluisce
        Inherits HttpMessageHandler

        Private ReadOnly _passi As Queue(Of PassoFlusso)

        ''' <summary>Quante volte è stata chiamata: è il conto dei tentativi.</summary>
        Public Property Chiamate As Integer

        Public Property UltimoCorpo As String

        Public Sub New(ParamArray passi As PassoFlusso())
            _passi = New Queue(Of PassoFlusso)(passi)
        End Sub

        Protected Overrides Async Function SendAsync(richiesta As HttpRequestMessage,
                                                     annulla As CancellationToken) As Task(Of HttpResponseMessage)

            Chiamate += 1

            If richiesta.Content IsNot Nothing Then
                UltimoCorpo = Await richiesta.Content.ReadAsStringAsync(annulla).ConfigureAwait(False)
            End If

            Dim passo As PassoFlusso = If(_passi.Count > 0, _passi.Dequeue(), New PassoFlusso())

            If passo.Stato <> 200 Then
                Return New HttpResponseMessage(CType(passo.Stato, HttpStatusCode)) With {
                    .Content = New StringContent(If(passo.Corpo, "{}"), Encoding.UTF8, "application/json")}
            End If

            Dim contenuto As New StreamContent(
                New FlussoAPezzi(passo.Pezzi, passo.RompiAllaFine, passo.AttesaFinale,
                                 passo.RitardoFraPezzi))
            contenuto.Headers.ContentType = New MediaTypeHeaderValue("text/event-stream")

            Return New HttpResponseMessage(HttpStatusCode.OK) With {.Content = contenuto}

        End Function

    End Class

    ''' <summary>
    ''' Un flusso che consegna quello che ha, un pezzo per lettura — e che, se glielo si
    ''' chiede, alla fine tace o si spezza.
    ''' </summary>
    Friend Class FlussoAPezzi
        Inherits IO.Stream

        Private ReadOnly _pezzi As Queue(Of Byte())
        Private ReadOnly _rompiAllaFine As Boolean
        Private ReadOnly _attesaFinale As TimeSpan
        Private ReadOnly _ritardoFraPezzi As TimeSpan

        Private _corrente As Byte() = Array.Empty(Of Byte)()
        Private _da As Integer
        Private _consegnati As Integer

        Public Sub New(pezzi As IEnumerable(Of String), rompiAllaFine As Boolean, attesaFinale As TimeSpan,
                       Optional ritardoFraPezzi As TimeSpan = Nothing)
            _pezzi = New Queue(Of Byte())(pezzi.Select(Function(p) Encoding.UTF8.GetBytes(p)))
            _rompiAllaFine = rompiAllaFine
            _attesaFinale = attesaFinale
            _ritardoFraPezzi = ritardoFraPezzi
        End Sub

        ' VB non sa scrivere una funzione Async che restituisce ValueTask: si avvolge il
        ' Task vero, che invece Async lo sa fare.
        Public Overrides Function ReadAsync(buffer As Memory(Of Byte),
                                            Optional annulla As CancellationToken = Nothing) As ValueTask(Of Integer)
            Return New ValueTask(Of Integer)(LeggiAsync(buffer, annulla))
        End Function

        Private Async Function LeggiAsync(buffer As Memory(Of Byte), annulla As CancellationToken) As Task(Of Integer)

            If _da >= _corrente.Length Then

                If _pezzi.Count = 0 Then
                    If _rompiAllaFine Then
                        Throw New IO.IOException("La connessione si è chiusa mentre l'AI scriveva.")
                    End If
                    If _attesaFinale > TimeSpan.Zero Then
                        Await Task.Delay(_attesaFinale, annulla).ConfigureAwait(False)
                    End If
                    Return 0
                End If

                ' Il primo pezzo parte subito; gli altri si fanno aspettare, ognuno il suo
                ' intervallo. È il modo di far durare una risposta più del silenzio
                ' concesso senza mai tacere per tutto quel silenzio.
                If _consegnati > 0 AndAlso _ritardoFraPezzi > TimeSpan.Zero Then
                    Await Task.Delay(_ritardoFraPezzi, annulla).ConfigureAwait(False)
                End If

                _corrente = _pezzi.Dequeue()
                _da = 0
                _consegnati += 1

            End If

            Dim quanti As Integer = Math.Min(buffer.Length, _corrente.Length - _da)
            _corrente.AsMemory(_da, quanti).CopyTo(buffer)
            _da += quanti

            Return quanti

        End Function

        Public Overrides Function Read(buffer As Byte(), offset As Integer, count As Integer) As Integer
            Return LeggiAsync(buffer.AsMemory(offset, count), CancellationToken.None).
                GetAwaiter().GetResult()
        End Function

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Length As Long
            Get
                Throw New NotSupportedException()
            End Get
        End Property

        Public Overrides Property Position As Long
            Get
                Throw New NotSupportedException()
            End Get
            Set(value As Long)
                Throw New NotSupportedException()
            End Set
        End Property

        Public Overrides Sub Flush()
        End Sub

        Public Overrides Function Seek(offset As Long, origin As IO.SeekOrigin) As Long
            Throw New NotSupportedException()
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New NotSupportedException()
        End Sub

        Public Overrides Sub Write(buffer As Byte(), offset As Integer, count As Integer)
            Throw New NotSupportedException()
        End Sub

    End Class

End Namespace
