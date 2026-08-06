Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Motore

Namespace NonRegressione

    ''' <summary>
    ''' La seconda gamba della non-regressione di T2 (cap. 14): <b>il confronto reale</b>,
    ''' con l'API vera e il prototipo acceso come giudice. Gli stessi due casi vanno a
    ''' <c>POST /confronta</c> del prototipo e alla pipeline della nuova app — pool,
    ''' client, estrattore, calcolo — e i due esiti si mettono a confronto.
    ''' </summary>
    ''' <remarks>
    ''' <para>Due chiamate allo stesso modello con lo stesso prompt non danno mai giudizi
    ''' identici: il pass/fail netto sta perciò dove le cose sono deterministiche — i
    ''' <b>giudizi prodotti dal prototipo</b>, passati al nostro <c>CalcoloMatch</c>,
    ''' devono dare gli stessi identici numeri, le stesse stelle e la stessa nota. Il
    ''' resto — quanto si somigliano i due giudizi dell'AI — si misura con una
    ''' tolleranza dichiarata e si legge nel rapporto salvato in <c>casi/reale/</c>.</para>
    ''' <para>Questi collaudi sono nella categoria <b>Reale</b> e non girano con la
    ''' batteria normale: vogliono la chiave API e il prototipo in ascolto, quindi si
    ''' eseguono a mano su aviolab03 con
    ''' <c>dotnet test --filter TestCategory=Reale</c>.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiConfrontoReale

        ''' <summary>L'endpoint del prototipo: <c>npm start</c> dentro <c>HTML+JS/</c>.</summary>
        Private Const Prototipo As String = "http://localhost:3000/confronta"

        ''' <summary>
        ''' Quanto possono discostarsi le stelle dei due esiti. Mezza stella è il passo
        ''' del voto mostrato all'utente: sotto questa soglia i due confronti dicono la
        ''' stessa cosa, sopra direbbero all'utente due cose diverse.
        ''' </summary>
        Private Const TolleranzaStelle As Double = 0.5

        <TestMethod, TestCategory("Reale")>
        Public Async Function IlConfrontoRealeRegge_Compatibile() As Task
            Await EseguiCasoAsync(CasiDiCollaudo.Compatibile)
        End Function

        <TestMethod, TestCategory("Reale")>
        Public Async Function IlConfrontoRealeRegge_Eliminatorio() As Task
            Await EseguiCasoAsync(CasiDiCollaudo.Eliminatorio)
        End Function

        ''' <summary>Il giro completo su un caso: prototipo, app, confronto, rapporto.</summary>
        Private Shared Async Function EseguiCasoAsync(caso As String) As Task

            Dim chiave As String = ChiaveOppureRinuncia()

            Dim daPrototipo As JsonObject = Await ChiediAlPrototipoAsync(caso)
            Dim daApp As JsonObject = Await ChiediAllAppAsync(caso, chiave)

            ' --- Parità deterministica: stessi giudizi, stesso calcolo. ---------------
            Dim giudiziPrototipo As JsonArray = TryCast(daPrototipo("giudizi"), JsonArray)
            Assert.IsNotNull(giudiziPrototipo, $"[{caso}] il prototipo non ha restituito giudizi")

            Dim ricalcolato As RisultatoMatch = CalcoloMatch.Calcola(
                giudiziPrototipo, daPrototipo("numero_llm"))

            VerificaStessoCalcolo(caso, daPrototipo, ricalcolato)

            ' --- Equivalenza di giudizio: due chiamate diverse, stessa risposta. ------
            Dim stellePrototipo As Double? = Numero(daPrototipo("stelle"))
            Dim stelleApp As Double? = Numero(daApp("stelle"))
            Dim gatePrototipo As Boolean = Vero(daPrototipo("gate_eliminatorio"))
            Dim gateApp As Boolean = Vero(daApp("gate_eliminatorio"))

            Dim rapporto As JsonObject = Scrivi(caso, daPrototipo, daApp, ricalcolato)
            Console.WriteLine(rapporto.ToJsonString(New JsonSerializerOptions With {.WriteIndented = True}))

            Assert.AreEqual(gatePrototipo, gateApp,
                $"[{caso}] il requisito eliminatorio è stato visto da uno solo dei due")

            Assert.IsNotNull(stellePrototipo, $"[{caso}] il prototipo non ha dato le stelle")
            Assert.IsNotNull(stelleApp, $"[{caso}] l'app non ha dato le stelle")
            Assert.IsLessThanOrEqualTo(TolleranzaStelle, Math.Abs(stellePrototipo.Value - stelleApp.Value),
                $"[{caso}] stelle troppo distanti: prototipo {stellePrototipo}, app {stelleApp}")

        End Function

        ''' <summary>
        ''' Il cuore del pass/fail: sui <b>giudizi del prototipo</b> il nostro calcolo
        ''' deve dare esattamente ciò che ha dato il suo, campo per campo.
        ''' </summary>
        Private Shared Sub VerificaStessoCalcolo(caso As String, atteso As JsonObject,
                                                 ottenuto As RisultatoMatch)

            Assert.AreEqual(Intero(atteso("score_base")), ottenuto.ScoreBase, $"[{caso}] score_base")
            Assert.AreEqual(Numero(atteso("numero_llm")), ottenuto.NumeroLlm, $"[{caso}] numero_llm")
            Assert.AreEqual(Intero(atteso("match_finale")), ottenuto.MatchFinale, $"[{caso}] match_finale")
            Assert.AreEqual(Numero(atteso("stelle")), ottenuto.Stelle, $"[{caso}] stelle")
            Assert.AreEqual(Vero(atteso("scarto_tagliato")), ottenuto.ScartoTagliato, $"[{caso}] scarto_tagliato")
            Assert.AreEqual(Vero(atteso("gate_eliminatorio")), ottenuto.GateEliminatorio, $"[{caso}] gate_eliminatorio")
            Assert.AreEqual(Testo(atteso("nota")), ottenuto.Nota, $"[{caso}] nota")

        End Sub

        ''' <summary>Il confronto come lo fa il prototipo, dal suo endpoint.</summary>
        Private Shared Async Function ChiediAlPrototipoAsync(caso As String) As Task(Of JsonObject)

            Dim richiesta As New JsonObject From {
                {"profilo", CasiDiCollaudo.Profilo()},
                {"annuncio", CasiDiCollaudo.Annuncio(caso)}}

            Using http As New HttpClient()

                http.Timeout = TimeSpan.FromMinutes(3)

                Dim risposta As HttpResponseMessage
                Try
                    risposta = Await http.PostAsync(Prototipo,
                        New StringContent(richiesta.ToJsonString(), New UTF8Encoding(False), "application/json"))
                Catch ex As HttpRequestException
                    Assert.Inconclusive(
                        "Il prototipo non risponde su " & Prototipo & ": avvialo con «npm start» " &
                        "dentro HTML+JS/ (la chiave sta nel suo .env). " & ex.Message)
                    Return Nothing
                End Try

                Using risposta

                    Dim corpo As String = Await risposta.Content.ReadAsStringAsync()

                    Assert.IsTrue(risposta.IsSuccessStatusCode,
                        $"[{caso}] il prototipo ha risposto HTTP {CInt(risposta.StatusCode)}: {corpo}")

                    Return TryCast(JsonNode.Parse(corpo), JsonObject)

                End Using
            End Using

        End Function

        ''' <summary>
        ''' Lo stesso confronto fatto dalla nuova app, mettendo in fila per la prima
        ''' volta i quattro pezzi di T2: pool → client → estrattore → calcolo.
        ''' </summary>
        Private Shared Async Function ChiediAllAppAsync(caso As String, chiave As String) As Task(Of JsonObject)

            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri()
            Dim confronto As Prompt = libreria.Carica("confronto")
            Dim testoRiempito As String = CasiDiCollaudo.PromptConfronto(libreria, caso)

            Using client As New ClientClaude(chiave)

                Dim risposta As RispostaAi = Await client.ChiediAsync(confronto, testoRiempito)
                Dim giro1 As JsonObject = TryCast(EstrattoreJson.Estrai(risposta.Testo), JsonObject)

                Assert.IsNotNull(giro1, $"[{caso}] la risposta dell'AI non è un oggetto JSON")

                Dim giudizi As JsonArray = TryCast(giro1("giudizi"), JsonArray)
                Assert.IsNotNull(giudizi, $"[{caso}] la risposta dell'AI non contiene i giudizi")

                Dim punteggio As RisultatoMatch = CalcoloMatch.Calcola(giudizi, giro1("numero_complessivo"))

                ' La stessa forma che restituisce l'endpoint del prototipo, così i due
                ' esiti si leggono affiancati senza tradurre nulla.
                Return New JsonObject From {
                    {"giudizi", giudizi.DeepClone()},
                    {"lettura_insieme", If(Testo(giro1("lettura_insieme")), String.Empty)},
                    {"score_base", JsonValore(punteggio.ScoreBase)},
                    {"numero_llm", JsonValore(punteggio.NumeroLlm)},
                    {"match_finale", JsonValore(punteggio.MatchFinale)},
                    {"stelle", JsonValore(punteggio.Stelle)},
                    {"scarto_tagliato", JsonValue.Create(punteggio.ScartoTagliato)},
                    {"gate_eliminatorio", JsonValue.Create(punteggio.GateEliminatorio)},
                    {"nota", If(punteggio.Nota Is Nothing, Nothing, JsonValue.Create(punteggio.Nota))},
                    {"modello", JsonValue.Create(risposta.Modello)},
                    {"token", New JsonObject From {
                        {"ingresso", JsonValue.Create(risposta.TokenIngresso)},
                        {"uscita", JsonValue.Create(risposta.TokenUscita)}}}}

            End Using

        End Function

        ''' <summary>
        ''' Salva l'esito in <c>casi/reale/</c>: è la prova che il collaudo è stato
        ''' fatto, e resta come caso di regressione riusabile senza rete.
        ''' </summary>
        Private Shared Function Scrivi(caso As String, daPrototipo As JsonObject, daApp As JsonObject,
                                       ricalcolato As RisultatoMatch) As JsonObject

            Dim rapporto As New JsonObject From {
                {"caso", caso},
                {"quando", DateTime.Now.ToString("yyyy-MM-dd HH:mm")},
                {"parita_deterministica", New JsonObject From {
                    {"che_cosa", "I giudizi del prototipo, ricalcolati da CalcoloMatch della nuova app."},
                    {"match_finale", JsonValore(ricalcolato.MatchFinale)},
                    {"stelle", JsonValore(ricalcolato.Stelle)},
                    {"gate_eliminatorio", JsonValue.Create(ricalcolato.GateEliminatorio)},
                    {"nota", If(ricalcolato.Nota Is Nothing, Nothing, JsonValue.Create(ricalcolato.Nota))}}},
                {"prototipo", daPrototipo.DeepClone()},
                {"app", daApp.DeepClone()}}

            Dim cartella As String = Path.Combine(CasiDiCollaudo.Cartella, "reale")
            Directory.CreateDirectory(cartella)

            File.WriteAllText(
                Path.Combine(cartella, $"confronto_{caso}.json"),
                rapporto.ToJsonString(New JsonSerializerOptions With {
                    .WriteIndented = True,
                    .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}) & vbLf,
                New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Return rapporto

        End Function

        ''' <summary>La chiave dall'ambiente; senza, il collaudo non fallisce: rinuncia.</summary>
        Private Shared Function ChiaveOppureRinuncia() As String

            Try
                Return ClientClaude.ChiaveDaAmbiente()
            Catch ex As ErroreAi
                Assert.Inconclusive(
                    "Collaudo reale saltato: manca la chiave. Definisci " &
                    ClientClaude.NomeVariabileChiave & " nell'ambiente prima di lanciare il banco.")
                Return Nothing
            End Try

        End Function

        ' --- Letture prudenti dei campi JSON, che possono anche essere null. ---------

        Private Shared Function Numero(nodo As JsonNode) As Double?
            Dim valore As JsonValue = TryCast(nodo, JsonValue)
            If valore Is Nothing Then Return Nothing
            Dim quanto As Double
            Return If(valore.TryGetValue(Of Double)(quanto), CType(quanto, Double?), Nothing)
        End Function

        Private Shared Function Intero(nodo As JsonNode) As Integer?
            Dim quanto As Double? = Numero(nodo)
            Return If(quanto.HasValue, CType(CInt(quanto.Value), Integer?), Nothing)
        End Function

        Private Shared Function Vero(nodo As JsonNode) As Boolean
            Dim valore As JsonValue = TryCast(nodo, JsonValue)
            If valore Is Nothing Then Return False
            Dim booleano As Boolean
            Return valore.TryGetValue(Of Boolean)(booleano) AndAlso booleano
        End Function

        Private Shared Function Testo(nodo As JsonNode) As String
            Return TryCast(nodo, JsonValue)?.ToString()
        End Function

        Private Shared Function JsonValore(numero As Integer?) As JsonNode
            Return If(numero.HasValue, JsonValue.Create(numero.Value), Nothing)
        End Function

        Private Shared Function JsonValore(numero As Double?) As JsonNode
            Return If(numero.HasValue, JsonValue.Create(numero.Value), Nothing)
        End Function

    End Class

End Namespace
