Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi della porta che chiede all'API quali modelli esistono (2026-08-27, dalla
    ''' revisione del giro D). Serve alle tendine delle Impostazioni, e la promessa da
    ''' difendere è che <b>non è un servizio da cui dipenda il lavoro</b>: quando non
    ''' arriva si ripiega sui modelli conosciuti, e in nessun caso si solleva.
    ''' </summary>
    <TestClass>
    Public Class CollaudiElencoModelli

        Private Const ChiaveFinta As String = "sk-ant-finta-0000-CODA"

        ''' <summary>Una rete finta che risponde sempre lo stesso, o si rompe sempre allo stesso modo.</summary>
        Private NotInheritable Class ReteFinta
            Inherits HttpMessageHandler

            Private ReadOnly _stato As HttpStatusCode?
            Private ReadOnly _corpo As String
            Private ReadOnly _guasto As Exception

            Public Sub New(stato As HttpStatusCode, Optional corpo As String = "")
                _stato = stato
                _corpo = corpo
            End Sub

            Public Sub New(guasto As Exception)
                _guasto = guasto
            End Sub

            Public Property UltimoIndirizzo As String

            Protected Overrides Function SendAsync(richiesta As HttpRequestMessage,
                                                   annulla As CancellationToken) As Task(Of HttpResponseMessage)

                UltimoIndirizzo = richiesta.RequestUri.ToString()

                If _guasto IsNot Nothing Then Return Task.FromException(Of HttpResponseMessage)(_guasto)

                Return Task.FromResult(New HttpResponseMessage(_stato.Value) With {
                    .Content = New StringContent(_corpo)})

            End Function

        End Class

        ''' <summary>Una risposta come quella vera dell'API, ridotta a due modelli.</summary>
        ''' <summary>
        ''' Com'è fatto davvero l'elenco dell'API: Haiku 4.5 lo dichiara <b>datato</b>,
        ''' non con l'alias. Questa costante è nata il 2026-08-27 perché quella qui sotto
        ''' era più gentile della realtà — scriveva <c>claude-haiku-4-5</c> — e con un
        ''' dato di prova così nessun collaudo poteva accorgersi che l'alias e la versione
        ''' datata venivano trattati come due modelli diversi.
        ''' </summary>
        Private Const RispostaComeQuellaVera As String =
            "{""data"":[" &
            "{""type"":""model"",""id"":""claude-sonnet-5"",""display_name"":""Claude Sonnet 5""}," &
            "{""type"":""model"",""id"":""claude-haiku-4-5-20251001"",""display_name"":""Claude Haiku 4.5""}]," &
            """has_more"":false}"

        Private Const RispostaVera As String =
            "{""data"":[" &
            "{""type"":""model"",""id"":""claude-sonnet-5"",""display_name"":""Claude Sonnet 5""}," &
            "{""type"":""model"",""id"":""claude-haiku-4-5"",""display_name"":""Claude Haiku 4.5""}]," &
            """has_more"":false}"

        ' ==================================================================
        ' Leggere la risposta
        ' ==================================================================

        <TestMethod>
        Public Sub DalCorpoLeggeIdENome()

            Dim esito As EsitoElenco = ElencoModelli.DalCorpo(RispostaVera)

            Assert.IsTrue(esito.Riuscita, "una risposta buona")
            Assert.AreEqual(2, esito.Modelli.Count, "due modelli")
            Assert.AreEqual("claude-sonnet-5", esito.Modelli(0).Id, "identificativo")
            Assert.AreEqual("Claude Sonnet 5", esito.Modelli(0).Nome, "nome leggibile")

        End Sub

        <TestMethod>
        Public Sub LIdentificativoSiVedeSempre()

            ' In una tendina il nome è più bello, ma è l'identificativo che finisce in
            ' modelli.json e nei messaggi d'errore: nasconderlo lascerebbe l'utente senza
            ' la sola cosa che gli serve scrivere.
            Assert.AreEqual("Claude Sonnet 5 (claude-sonnet-5)",
                            New ModelloDisponibile("claude-sonnet-5", "Claude Sonnet 5").ToString(),
                            "col nome")

            Assert.AreEqual("claude-sonnet-5",
                            New ModelloDisponibile("claude-sonnet-5", "").ToString(),
                            "senza nome, l'identificativo da solo")

        End Sub

        <TestMethod>
        Public Sub UnaRispostaSenzaDatiNonEUnElencoVuoto()

            ' Dire «nessun modello disponibile» sarebbe una bugia costruita su una
            ' risposta che non si è capita: si dichiara di non sapere, e chi chiama
            ' ripiega su quel che conosce.
            For Each storta As String In {"", "non è json", "{}", "{""data"":42}", "[]"}
                Dim esito As EsitoElenco = ElencoModelli.DalCorpo(storta)
                Assert.IsFalse(esito.Riuscita, $"«{storta}» non è un elenco")
                Assert.AreEqual(CausaErroreAi.RispostaInattesa, esito.Causa, $"«{storta}»: la causa")
            Next

        End Sub

        <TestMethod>
        Public Sub UnElencoVuotoArrivatoBeneNonServeANiente()

            Dim esito As EsitoElenco = ElencoModelli.DalCorpo("{""data"":[]}")

            Assert.IsFalse(esito.Riuscita, "una tendina senza voci non è un esito riuscito")

        End Sub

        <TestMethod>
        Public Sub UnaVoceSenzaIdentificativoSiSalta()

            Dim esito As EsitoElenco = ElencoModelli.DalCorpo(
                "{""data"":[{""display_name"":""Senza id""},{""id"":""claude-haiku-4-5""}]}")

            Assert.IsTrue(esito.Riuscita, "le altre voci restano buone")
            Assert.AreEqual(1, esito.Modelli.Count, "la voce monca non entra")
            Assert.AreEqual("claude-haiku-4-5", esito.Modelli(0).Id)

        End Sub

        ' ==================================================================
        ' La chiamata vera, con una rete finta
        ' ==================================================================

        <TestMethod>
        Public Async Function ChiedeAllaPortaCheNonConsumaToken() As Task

            Dim rete As New ReteFinta(HttpStatusCode.OK, RispostaVera)

            Dim esito As EsitoElenco = Await ElencoModelli.ChiediAsync(ChiaveFinta, rete)

            Assert.IsTrue(esito.Riuscita, "l'elenco è arrivato")
            Assert.IsTrue(rete.UltimoIndirizzo.StartsWith("https://api.anthropic.com/v1/models"),
                          "è la porta dell'elenco, che non consuma token")

        End Function

        <TestMethod>
        Public Async Function SenzaChiaveNonSiChiamaNessuno() As Task

            Dim rete As New ReteFinta(HttpStatusCode.OK, RispostaVera)

            Dim esito As EsitoElenco = Await ElencoModelli.ChiediAsync("   ", rete)

            Assert.IsFalse(esito.Riuscita, "senza chiave non si chiede")
            Assert.AreEqual(CausaErroreAi.Chiave, esito.Causa, "e si dice perché")
            Assert.IsNull(rete.UltimoIndirizzo, "nessuna chiamata è partita")

        End Function

        <TestMethod>
        Public Async Function UnaChiaveRifiutataSiDistingueDaUnaReteAssente() As Task

            ' È la stessa distinzione che ha reso possibile «Prova la chiave»: portano a
            ' due gesti diversi, e dirle allo stesso modo manderebbe a cercare una chiave
            ' nuova che non serve.
            Dim rifiutata As EsitoElenco =
                Await ElencoModelli.ChiediAsync(ChiaveFinta, New ReteFinta(HttpStatusCode.Unauthorized))
            Dim senzaRete As EsitoElenco =
                Await ElencoModelli.ChiediAsync(ChiaveFinta, New ReteFinta(New HttpRequestException("giù")))

            Assert.AreEqual(CausaErroreAi.Chiave, rifiutata.Causa, "401: la chiave")
            Assert.AreEqual(CausaErroreAi.Rete, senzaRete.Causa, "collegamento assente: la rete")
            Assert.AreNotEqual(ElencoModelli.Perche(rifiutata.Causa), ElencoModelli.Perche(senzaRete.Causa),
                               "e le due righe non si somigliano")

        End Function

        <TestMethod>
        Public Async Function UnGuastoDiReteNonSolleva() As Task

            ' Una tendina non è un servizio da cui dipenda il lavoro: se non arriva,
            ' l'esito è il valore e le Impostazioni si aprono lo stesso.
            For Each rete As ReteFinta In {New ReteFinta(New HttpRequestException("giù")),
                                           New ReteFinta(New IO.IOException("cavo")),
                                           New ReteFinta(HttpStatusCode.InternalServerError)}

                Dim esito As EsitoElenco = Await ElencoModelli.ChiediAsync(ChiaveFinta, rete)
                Assert.IsFalse(esito.Riuscita, "non è riuscita, ma non ha sollevato")
                Assert.AreEqual(0, esito.Modelli.Count, "e l'elenco resta vuoto, non nullo")
            Next

        End Function

        ' ==================================================================
        ' Il ripiego, e il modello ritirato
        ' ==================================================================

        <TestMethod>
        Public Sub IConosciutiSonoQuelliInUsoPiuIPredefiniti()

            Dim inUso As Modelli = Modelli.DaJson("{ ""ragionamento"": ""claude-opus-4-8"" }")

            Dim voci As IReadOnlyList(Of ModelloDisponibile) = ElencoModelli.Conosciuti(inUso)
            Dim id As String() = voci.Select(Function(v) v.Id).ToArray()

            CollectionAssert.Contains(id, "claude-opus-4-8", "quello in uso")
            CollectionAssert.Contains(id, "claude-sonnet-5", "il predefinito del ragionamento")
            CollectionAssert.Contains(id, "claude-haiku-4-5", "quello del livello semplice, che è anche il predefinito")
            Assert.AreEqual(3, voci.Count, "e nessuna ripetizione: haiku vale per due ed è scritto una volta")

        End Sub

        <TestMethod>
        Public Sub NellElencoCEsempreQuelloCheSiStaUsando()

            ' Il caso che conta è il modello ritirato: sparisce dall'elenco dell'API e
            ' resta scritto in modelli.json. Se la tendina lo omettesse mostrerebbe come
            ' scelto un modello diverso da quello vero, e nessuno se ne accorgerebbe.
            Dim daApi As IReadOnlyList(Of ModelloDisponibile) = ElencoModelli.DalCorpo(RispostaVera).Modelli

            Dim conRitirato As IReadOnlyList(Of ModelloDisponibile) =
                ElencoModelli.ConQuelloInUso(daApi, "claude-sonnet-4-6")

            Assert.AreEqual(3, conRitirato.Count, "l'elenco si allunga di uno")
            Assert.AreEqual("claude-sonnet-4-6", conRitirato(0).Id, "e sta in cima: è quello da sostituire")

        End Sub

        <TestMethod>
        Public Sub LAliasNonSiRaddoppiaConLaSuaVersioneDatata()

            ' Il difetto che ha fatto nascere IdModello: il programma chiede
            ' claude-haiku-4-5, l'API elenca claude-haiku-4-5-20251001, e la tendina
            ' mostrava Haiku 4.5 due volte — una col nome e una con l'identificativo
            ' crudo, senza modo di capire che era lo stesso modello (2026-08-27).
            Dim daApi As IReadOnlyList(Of ModelloDisponibile) =
                ElencoModelli.DalCorpo(RispostaComeQuellaVera).Modelli

            Dim voci As IReadOnlyList(Of ModelloDisponibile) =
                ElencoModelli.ConQuelloInUso(daApi, "claude-haiku-4-5")

            Assert.AreEqual(2, voci.Count, "l'elenco non si allunga: è lo stesso modello")
            Assert.AreEqual("claude-haiku-4-5-20251001", voci(1).Id, "e resta quello che l'API dichiara")
            Assert.AreEqual("Claude Haiku 4.5", voci(1).Nome, "col suo nome, non con l'identificativo crudo")

        End Sub

        <TestMethod>
        Public Sub UnRitiratoEntraLoStessoAncheOraCheSiRiconosconoGliAlias()

            ' La cura non deve costare la promessa di prima: di un modello ritirato
            ' nell'elenco non c'è nessuna versione, e lui in cima ci va.
            Dim daApi As IReadOnlyList(Of ModelloDisponibile) =
                ElencoModelli.DalCorpo(RispostaComeQuellaVera).Modelli

            Dim voci As IReadOnlyList(Of ModelloDisponibile) =
                ElencoModelli.ConQuelloInUso(daApi, "claude-sonnet-4-6")

            Assert.AreEqual(3, voci.Count, "l'elenco si allunga di uno")
            Assert.AreEqual("claude-sonnet-4-6", voci(0).Id, "e sta in cima: è quello da sostituire")

        End Sub

        <TestMethod>
        Public Sub UnModelloGiaNellElencoNonSiRaddoppia()

            Dim daApi As IReadOnlyList(Of ModelloDisponibile) = ElencoModelli.DalCorpo(RispostaVera).Modelli

            Assert.AreEqual(2, ElencoModelli.ConQuelloInUso(daApi, "claude-haiku-4-5").Count)

        End Sub

    End Class

End Namespace
