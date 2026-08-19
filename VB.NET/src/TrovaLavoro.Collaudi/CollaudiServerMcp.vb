Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Mcp
Imports TrovaLavoro.Motore

Namespace Mcp

    ''' <summary>
    ''' Collaudi del server MCP (cap. 09). Le cose da tenere ferme sono quattro: il
    ''' server risponde <b>a tutte e due le ere</b> del protocollo e non confonde le due
    ''' forme di risposta; una riga in entrata produce una riga sola in uscita; un tool
    ''' che non ce la fa non è un errore di protocollo; e da qui non si esce dalla
    ''' cartella dati.
    ''' </summary>
    ''' <remarks>
    ''' Il ciclo su stdio non serve: <see cref="ServerMcp.Rispondi"/> sta staccato
    ''' apposta, e si interroga come una funzione qualunque. Che poi quel ciclo giri
    ''' davvero dentro l'eseguibile, con le pipe vere, lo prova
    ''' <c>CollaudiServerMcpDalVivo</c> — sono due domande diverse e vanno tenute
    ''' separate.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiServerMcp

        Private Const ChiaveFinta As String = "chiave-di-collaudo"

        ''' <summary>Il nome della cartella-opportunità di prova, nella forma vera: data in testa.</summary>
        Private Const Candidatura As String = "2026-08-19_acme_sviluppatore"

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

        Private Shared Function CartellaTemporanea() As String
            Return Path.Combine(Path.GetTempPath(), "mcp-" & Guid.NewGuid().ToString("N"))
        End Function

        Private Shared Function Monta(radice As String) As ContestoApp
            Return ContestoApp.Monta(radice, ChiaveFinta, PoolInesistente())
        End Function

        ''' <summary>Un server sulla cartella dati indicata, con il diario buttato via.</summary>
        Private Shared Function Servitore(contesto As ContestoApp) As ServerMcp
            Return New ServerMcp(contesto, TextReader.Null, TextWriter.Null, TextWriter.Null)
        End Function

        ''' <summary>
        ''' Una richiesta pronta da mandare. Nell'era moderna porta il <c>_meta</c>
        ''' completo — versione e capacità — che è quel che il protocollo pretende su
        ''' ogni messaggio; in quella legacy non porta niente, come i client di allora.
        ''' </summary>
        Private Shared Function Richiesta(metodo As String, era As EraMcp,
                                          Optional parametri As JsonObject = Nothing,
                                          Optional id As JsonNode = Nothing,
                                          Optional versione As String = Nothing) As String

            Dim corpo As JsonObject = If(parametri, New JsonObject())

            If era = EraMcp.Moderna Then
                corpo("_meta") = New JsonObject From {
                    {ProtocolloMcp.ChiaveVersione, If(versione, ProtocolloMcp.VersioneModerna)},
                    {ProtocolloMcp.ChiaveCapacitaClient, New JsonObject()},
                    {ProtocolloMcp.ChiaveInfoClient, New JsonObject From {
                        {"name", "banco"}, {"version", "1.0"}}}}
            End If

            Dim messaggio As New JsonObject From {
                {"jsonrpc", "2.0"},
                {"id", If(id, JsonValue.Create(1))},
                {"method", metodo},
                {"params", corpo}}

            Return messaggio.ToJsonString()

        End Function

        ''' <summary>La risposta come oggetto JSON.</summary>
        Private Shared Function Letta(risposta As String) As JsonObject

            Assert.IsNotNull(risposta, "una risposta ci vuole")
            Return TryCast(JsonNode.Parse(risposta), JsonObject)

        End Function

        ''' <summary>Il <c>result</c> di una risposta riuscita; fa fallire il collaudo se è un errore.</summary>
        Private Shared Function Risultato(risposta As String) As JsonObject

            Dim messaggio As JsonObject = Letta(risposta)
            Assert.IsFalse(messaggio.ContainsKey("error"), $"non doveva essere un errore: {risposta}")

            Return TryCast(messaggio("result"), JsonObject)

        End Function

        ''' <summary>Il codice dell'errore; fa fallire il collaudo se la risposta è riuscita.</summary>
        Private Shared Function CodiceErrore(risposta As String) As Integer

            Dim messaggio As JsonObject = Letta(risposta)
            Dim guasto As JsonObject = TryCast(messaggio("error"), JsonObject)
            Assert.IsNotNull(guasto, $"doveva essere un errore: {risposta}")

            Return guasto("code").GetValue(Of Integer)()

        End Function

#Region "Le due ere"

        <TestMethod>
        Public Sub UnClientVecchioApreConInitializeEVieneServito()
            ' L'era legacy: la versione si negozia una volta, e se è una che sappiamo
            ' parlare si risponde con la stessa — spostare il client altrove senza motivo
            ' lo costringerebbe a decidere se restare.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim risposta As String = Servitore(contesto).Rispondi(
                    Richiesta("initialize", EraMcp.Legacy,
                              New JsonObject From {{"protocolVersion", "2025-11-25"}}))

                Dim esito As JsonObject = Risultato(risposta)
                Assert.AreEqual("2025-11-25", esito("protocolVersion").GetValue(Of String)(), "la versione chiesta")
                Assert.IsNotNull(esito("serverInfo"), "chi siamo")
                Assert.IsNotNull(TryCast(esito("capabilities"), JsonObject)("tools"), "sappiamo fare tool")
                Assert.IsFalse(esito.ContainsKey("resultType"),
                               "l'era vecchia non conosce resultType e non deve vederselo arrivare")

            End Using
        End Sub

        <TestMethod>
        Public Sub AUnaRevisioneVecchiaCheNonConosciamoSiRispondeConLaNostra()
            ' La regola dell'handshake: se non sappiamo parlare quella chiesta, si
            ' risponde con la più recente che conosciamo, e poi decide il client.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim risposta As String = Servitore(contesto).Rispondi(
                    Richiesta("initialize", EraMcp.Legacy,
                              New JsonObject From {{"protocolVersion", "1.0.0"}}))

                Assert.AreEqual(ProtocolloMcp.VersioneLegacy,
                                Risultato(risposta)("protocolVersion").GetValue(Of String)(),
                                "la nostra, non la sua")

            End Using
        End Sub

        <TestMethod>
        Public Sub UnClientNuovoEntraSenzaBussareEScopreLeDueEre()
            ' L'era moderna: nessun handshake, si chiama e basta. E il server dichiara
            ' tutte le versioni che parla, che è ciò che rende visibile la doppia porta.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim esito As JsonObject = Risultato(
                    Servitore(contesto).Rispondi(Richiesta("server/discover", EraMcp.Moderna)))

                Assert.AreEqual("complete", esito("resultType").GetValue(Of String)(), "il tipo del risultato")

                Dim versioni As String = esito("supportedVersions").ToJsonString()
                StringAssert.Contains(versioni, ProtocolloMcp.VersioneModerna, "la moderna")
                StringAssert.Contains(versioni, ProtocolloMcp.VersioneLegacy, "e la vecchia")

                Dim meta As JsonObject = TryCast(esito("_meta"), JsonObject)
                Assert.IsNotNull(meta(ProtocolloMcp.ChiaveInfoServer), "chi siamo, nel meta")

            End Using
        End Sub

        <TestMethod>
        Public Sub UnaVersioneCheNonParliamoTornaConLElencoDiQuelleCheParliamo()
            ' Senza l'elenco il client saprebbe solo di aver sbagliato, e non avrebbe
            ' modo di riprovare giusto.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim risposta As String = Servitore(contesto).Rispondi(
                    Richiesta("tools/list", EraMcp.Moderna, versione:="1999-01-01"))

                Assert.AreEqual(ProtocolloMcp.ErroreVersioneNonSupportata, CodiceErrore(risposta), "il codice giusto")
                StringAssert.Contains(risposta, ProtocolloMcp.VersioneModerna, "e l'elenco di quelle buone")

            End Using
        End Sub

        <TestMethod>
        Public Sub NellEraModernaLeCapacitaDelClientSonoObbligatorie()
            ' Una richiesta moderna senza capacità dichiarate è malformata, e la spec
            ' vuole che si dica con «parametri non validi».
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim monca As New JsonObject From {
                    {"jsonrpc", "2.0"},
                    {"id", 7},
                    {"method", "tools/list"},
                    {"params", New JsonObject From {
                        {"_meta", New JsonObject From {
                            {ProtocolloMcp.ChiaveVersione, ProtocolloMcp.VersioneModerna}}}}}}

                Assert.AreEqual(ProtocolloMcp.ErroreParametriNonValidi,
                                CodiceErrore(Servitore(contesto).Rispondi(monca.ToJsonString())),
                                "manca ciò che il client sa fare")

            End Using
        End Sub

        <TestMethod>
        Public Sub AllEraVecchiaNonSiChiedeQuelCheNonPuoDare()
            ' Un client legacy non manda nessun _meta: pretenderlo vorrebbe dire
            ' rifiutare tutti i client di ieri.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim esito As JsonObject = Risultato(
                    Servitore(contesto).Rispondi(Richiesta("tools/list", EraMcp.Legacy)))

                Assert.IsNotNull(esito("tools"), "i tool ci sono lo stesso")
                Assert.IsFalse(esito.ContainsKey("resultType"), "e la forma è quella di allora")

            End Using
        End Sub

#End Region

#Region "La vetrina dei tool"

        <TestMethod>
        Public Sub ITreToolDiLetturaSiPresentanoConLoroSchema()
            ' Cap. 09.3. Uno schema ci vuole sempre, anche per un tool che non chiede
            ' niente: un tool senza parametri non è un tool senza schema.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim tool As JsonArray = TryCast(
                    Risultato(Servitore(contesto).Rispondi(Richiesta("tools/list", EraMcp.Moderna)))("tools"),
                    JsonArray)

                Assert.AreEqual(3, tool.Count, "tre tool a T8a")

                Dim nomi As New List(Of String)
                For Each t As JsonNode In tool
                    Dim descritto As JsonObject = TryCast(t, JsonObject)
                    nomi.Add(descritto("name").GetValue(Of String)())
                    Assert.IsNotNull(descritto("description"), "a che serve")
                    Assert.IsNotNull(TryCast(descritto("inputSchema"), JsonObject)("type"), "e cosa vuole")
                Next

                CollectionAssert.AreEqual(
                    New List(Of String) From {CatalogoTool.LeggiProfilo, CatalogoTool.LeggiRegistro,
                                              CatalogoTool.LeggiOpportunita},
                    nomi, "nell'ordine dichiarato, che non deve ballare")

            End Using
        End Sub

        <TestMethod>
        Public Sub LElencoDeiToolSiPuoChiedereDueVolte()
            ' Un nodo JSON non sta in due alberi: se lo schema non si ricopiasse, la
            ' seconda richiesta troverebbe il nostro già appeso alla prima e scoppierebbe.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim server As ServerMcp = Servitore(contesto)
                Dim primo As String = server.Rispondi(Richiesta("tools/list", EraMcp.Moderna))
                Dim secondo As String = server.Rispondi(Richiesta("tools/list", EraMcp.Moderna))

                Assert.AreEqual(primo, secondo, "la stessa vetrina, due volte")

            End Using
        End Sub

        <TestMethod>
        Public Sub UnToolCheNonEsisteEUnErroreDiProtocollo()
            ' Cap. 09.2: chi ha chiamato ha sbagliato la richiesta, non i parametri, e
            ' dirgli «riprova» non lo aiuterebbe.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim risposta As String = Servitore(contesto).Rispondi(
                    Richiesta("tools/call", EraMcp.Moderna, New JsonObject From {{"name", "vola"}}))

                Assert.AreEqual(ProtocolloMcp.ErroreParametriNonValidi, CodiceErrore(risposta), "errore JSON-RPC")

            End Using
        End Sub

#End Region

#Region "I tool di lettura"

        <TestMethod>
        Public Sub SenzaProfiloIlToolLoDiceInveceDiRompersi()
            ' Un tool che non ce la fa risponde con un risultato normale marcato
            ' isError, il cui testo è scritto per essere letto da un modello.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim esito As JsonObject = Risultato(Servitore(contesto).Rispondi(
                    Richiesta("tools/call", EraMcp.Moderna,
                              New JsonObject From {{"name", CatalogoTool.LeggiProfilo}})))

                Assert.IsTrue(esito("isError").GetValue(Of Boolean)(), "l'ha detto")
                StringAssert.Contains(esito("content").ToJsonString(), "profilo", "e ha detto cosa manca")

            End Using
        End Sub

        <TestMethod>
        Public Sub IlProfiloSiConsegnaComEScrittoSuDisco()
            ' Il file è la fonte: rileggerlo e riscriverlo attraverso le classi del
            ' motore mostrerebbe la nostra interpretazione invece dei fatti.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Scrivi(contesto.Cartella.FileProfilo,
                       "{""nome"":""Mario Rossi"",""campo_che_non_conosciamo"":42}")

                Dim esito As JsonObject = Risultato(Servitore(contesto).Rispondi(
                    Richiesta("tools/call", EraMcp.Moderna,
                              New JsonObject From {{"name", CatalogoTool.LeggiProfilo}})))

                Assert.IsFalse(esito("isError").GetValue(Of Boolean)(), "è andata")

                Dim consegnato As JsonObject = TryCast(esito("structuredContent"), JsonObject)
                Assert.AreEqual("Mario Rossi", consegnato("nome").GetValue(Of String)(), "il nome")
                Assert.AreEqual(42, consegnato("campo_che_non_conosciamo").GetValue(Of Integer)(),
                                "e anche il campo che nessuna classe conosce")

                StringAssert.Contains(esito("content").ToJsonString(), "Mario Rossi",
                                      "lo stesso JSON anche nel blocco di testo, per chi legge solo quello")

            End Using
        End Sub

        <TestMethod>
        Public Sub IlRegistroRispondeAncheQuandoNonCeNessunaCandidatura()
            ' L'indice si ricostruisce dalle cartelle: se non ce ne sono, la risposta
            ' giusta è un elenco vuoto, non un guasto.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim esito As JsonObject = Risultato(Servitore(contesto).Rispondi(
                    Richiesta("tools/call", EraMcp.Moderna,
                              New JsonObject From {{"name", CatalogoTool.LeggiRegistro}})))

                Assert.IsFalse(esito("isError").GetValue(Of Boolean)(), "nessun guasto")

                Dim voci As JsonArray = TryCast(TryCast(esito("structuredContent"), JsonObject)("voci"), JsonArray)
                Assert.AreEqual(0, voci.Count, "e l'elenco è vuoto")

            End Using
        End Sub

        <TestMethod>
        Public Sub UnaCandidaturaSiConsegnaConTuttoQuelCheHaProdotto()
            ' Cap. 09.3: «tutti gli artefatti». Si raccolgono i .json che ci sono,
            ' invece di chiedere per nome quelli che il programma conosce.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                PreparaLaCandidatura(contesto)

                Dim esito As JsonObject = Risultato(Servitore(contesto).Rispondi(
                    Richiesta("tools/call", EraMcp.Moderna,
                              New JsonObject From {{"name", CatalogoTool.LeggiOpportunita},
                                                   {"arguments", New JsonObject From {{"cartella", Candidatura}}}})))

                Assert.IsFalse(esito("isError").GetValue(Of Boolean)(), "è andata")

                Dim raccolto As JsonObject = TryCast(esito("structuredContent"), JsonObject)
                Assert.AreEqual(Candidatura, raccolto("cartella").GetValue(Of String)(), "quale candidatura")
                Assert.AreEqual("Acme", TryCast(raccolto("annuncio"), JsonObject)("azienda").GetValue(Of String)(),
                                "l'annuncio")
                Assert.IsNotNull(raccolto("stato"), "lo stato")
                Assert.AreEqual("un_artefatto_futuro",
                                TryCast(raccolto("domani"), JsonObject)("chi_sono").GetValue(Of String)(),
                                "e anche un artefatto che nessuno ha dichiarato qui dentro")

                StringAssert.Contains(raccolto("documenti").ToJsonString(), "cv.docx",
                                      "i documenti impaginati, per nome")

            End Using
        End Sub

        <TestMethod>
        Public Sub UnaCandidaturaCheNonCESiDicePerNome()
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim esito As JsonObject = Risultato(Servitore(contesto).Rispondi(
                    Richiesta("tools/call", EraMcp.Moderna,
                              New JsonObject From {{"name", CatalogoTool.LeggiOpportunita},
                                                   {"arguments", New JsonObject From {{"cartella", "mai-esistita"}}}})))

                Assert.IsTrue(esito("isError").GetValue(Of Boolean)(), "non c'è")
                StringAssert.Contains(esito("content").ToJsonString(), "leggi_registro",
                                      "e si dice dove trovare i nomi buoni")

            End Using
        End Sub

        <TestMethod>
        Public Sub DaQuiNonSiEsceDallaCartellaDati()
            ' Qui arriva testo scritto da un modello, che può sbagliare e che qualcuno
            ' potrebbe aver istruito male: un nome è un nome (cap. 09.5).
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                For Each tentativo As String In New String() {"..", "..\..\Windows", "C:\Windows",
                                                              Candidatura & "/../..", "sotto/cartella"}

                    Dim esito As JsonObject = Risultato(Servitore(contesto).Rispondi(
                        Richiesta("tools/call", EraMcp.Moderna,
                                  New JsonObject From {{"name", CatalogoTool.LeggiOpportunita},
                                                       {"arguments", New JsonObject From {{"cartella", tentativo}}}})))

                    Assert.IsTrue(esito("isError").GetValue(Of Boolean)(), $"«{tentativo}» non deve passare")

                Next

            End Using
        End Sub

#End Region

#Region "Le regole del filo"

        <TestMethod>
        Public Sub UnaRispostaStaSuUnaRigaSola()
            ' È la riga a separare un messaggio dal successivo: un a capo in mezzo
            ' spezzerebbe il messaggio in due, e il client leggerebbe due metà illeggibili.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Scrivi(contesto.Cartella.FileProfilo, "{""nota"":""prima riga\nseconda riga""}")

                Dim risposte As String() = {
                    Servitore(contesto).Rispondi(Richiesta("tools/list", EraMcp.Moderna)),
                    Servitore(contesto).Rispondi(Richiesta("server/discover", EraMcp.Moderna)),
                    Servitore(contesto).Rispondi(Richiesta("tools/call", EraMcp.Moderna,
                        New JsonObject From {{"name", CatalogoTool.LeggiProfilo}}))}

                For Each risposta As String In risposte
                    Assert.AreEqual(-1, risposta.IndexOf(vbLf, StringComparison.Ordinal),
                                    "nessun a capo dentro il messaggio")
                    Assert.AreEqual(-1, risposta.IndexOf(vbCr, StringComparison.Ordinal),
                                    "nemmeno di quelli di Windows")
                Next

            End Using
        End Sub

        <TestMethod>
        Public Sub AUnaNotificaNonSiRisponde()
            ' Nemmeno per dire che non l'abbiamo capita: una notifica non ha
            ' identificativo, e una risposta senza destinatario confonderebbe il client.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim server As ServerMcp = Servitore(contesto)

                Assert.IsNull(server.Rispondi("{""jsonrpc"":""2.0"",""method"":""notifications/initialized""}"),
                              "il «sono pronto» dell'era vecchia")
                Assert.IsNull(server.Rispondi("{""jsonrpc"":""2.0"",""method"":""notifications/cancelled""," &
                                              """params"":{""requestId"":1}}"),
                              "e il «lascia perdere»")
                Assert.IsNull(server.Rispondi("{""jsonrpc"":""2.0"",""method"":""notifications/mai_sentita""}"),
                              "e anche una che non conosciamo")

            End Using
        End Sub

        <TestMethod>
        Public Sub UnaRigaCheNonEJsonNonUccideIlServer()
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim server As ServerMcp = Servitore(contesto)

                Assert.AreEqual(ProtocolloMcp.ErroreParse, CodiceErrore(server.Rispondi("{ questo non è JSON")),
                                "lo dice")
                Assert.IsNotNull(Risultato(server.Rispondi(Richiesta("tools/list", EraMcp.Moderna))),
                                 "e subito dopo lavora come prima")

            End Using
        End Sub

        <TestMethod>
        Public Sub UnMetodoCheNonConosciamoTornaMethodNotFound()
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Assert.AreEqual(ProtocolloMcp.ErroreMetodoIgnoto,
                                CodiceErrore(Servitore(contesto).Rispondi(Richiesta("balla/tango", EraMcp.Moderna))),
                                "il codice di «non so farlo»")

            End Using
        End Sub

        <TestMethod>
        Public Sub LIdentificativoTornaIndietroComEArrivato()
            ' JSON-RPC lo ammette testo o numero, e non si interpreta: è il client a
            ' doverlo riconoscere fra le risposte che gli arrivano.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Dim server As ServerMcp = Servitore(contesto)

                Dim idDiNumero As JsonObject = Letta(server.Rispondi(
                    Richiesta("tools/list", EraMcp.Moderna, id:=JsonValue.Create(99))))
                Assert.AreEqual(99, idDiNumero("id").GetValue(Of Integer)(), "il numero")

                Dim idDiTesto As JsonObject = Letta(server.Rispondi(
                    Richiesta("tools/list", EraMcp.Moderna, id:=JsonValue.Create("abc-1"))))
                Assert.AreEqual("abc-1", idDiTesto("id").GetValue(Of String)(), "e il testo")

            End Using
        End Sub

#End Region

        ''' <summary>Scrive un file, creando la cartella che lo ospita.</summary>
        Private Shared Sub Scrivi(percorso As String, contenuto As String)

            Directory.CreateDirectory(Path.GetDirectoryName(percorso))
            File.WriteAllText(percorso, contenuto, New UTF8Encoding(False))

        End Sub

        ''' <summary>
        ''' Una candidatura di prova sul disco, con un artefatto in più che il programma
        ''' non conosce: serve a dimostrare che si raccoglie quel che c'è, e non quel che
        ''' ci si aspetta.
        ''' </summary>
        Private Shared Sub PreparaLaCandidatura(contesto As ContestoApp)

            Dim cartella As String = Path.Combine(contesto.Cartella.CartellaOpportunita, Candidatura)

            Scrivi(Path.Combine(cartella, "annuncio.json"), "{""azienda"":""Acme"",""titolo"":""Sviluppatore""}")
            Scrivi(Path.Combine(cartella, "stato.json"),
                   "{""stato"":""nuova"",""azienda"":""Acme"",""titolo"":""Sviluppatore""}")
            Scrivi(Path.Combine(cartella, "domani.json"), "{""chi_sono"":""un_artefatto_futuro""}")
            Scrivi(Path.Combine(cartella, "out", "cv.docx"), "non è un vero docx, e qui non serve che lo sia")

        End Sub

    End Class

End Namespace
