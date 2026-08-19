Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Mcp
Imports TrovaLavoro.Motore

Namespace Mcp

    ''' <summary>
    ''' Collaudi dei due tool che cambiano qualcosa nella cartella dati (cap. 09.3, T8c):
    ''' <c>salva_opportunita</c> e <c>esporta_documento</c>.
    ''' </summary>
    ''' <remarks>
    ''' Questi non chiamano l'AI nemmeno per sbaglio — impaginano e scrivono file — quindi
    ''' qui si collauda tutto, non solo la porta. Le due domande che contano: che il
    ''' <b>lucchetto</b> li fermi quando la cartella è di qualcun altro, e che il
    ''' <b>punteggio</b> resti del programma anche quando arriva da fuori già pronto.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiToolDiScrittura

        Private Const ChiaveFinta As String = "chiave-di-collaudo"

        Private Shared Function CartellaTemporanea() As String
            Return Path.Combine(Path.GetTempPath(), "scrittura-" & Guid.NewGuid().ToString("N"))
        End Function

        ''' <summary>Un motore su una cartella usa-e-getta, ripulita alla fine.</summary>
        Private Shared Async Function ConMotore(prova As Func(Of ContestoApp, Task)) As Task

            Dim radice As String = CartellaTemporanea()

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, ChiaveFinta)
                    Await prova(contesto)
                End Using
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Function

        Private Shared Async Function Chiama(contesto As ContestoApp, nome As String,
                                             argomenti As JsonObject) As Task(Of EsitoTool)

            Return Await New CatalogoTool(contesto).EseguiAsync(nome, argomenti)

        End Function

        Private Shared Function Annuncio() As JsonNode

            Return JsonNode.Parse(
                "{ ""titolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino"", ""lingua"": ""it"" }")

        End Function

        Private Shared Function Cv() As JsonNode

            Return JsonNode.Parse(
                "{ ""tipo"": ""cv_mirato"", " &
                """intestazione"": { ""nome"": ""Luca Ferrari"", ""email"": ""luca@example.it"" }, " &
                """sommario"": ""Ho esperienza nel servizio di sala."", " &
                """competenze"": [""Servizio ai tavoli""] }")

        End Function

        ''' <summary>Un confronto già fatto: serve a ogni candidatura che porti dei documenti.</summary>
        Private Shared Function Confronto() As JsonNode

            Return JsonNode.Parse(
                "{ ""giudizi"": [ { ""requisito"": ""Servizio ai tavoli"", ""esito"": ""soddisfatto"" } ], " &
                """numero_complessivo"": 8 }")

        End Function

        ''' <summary>Il nome della cartella nata da un salvataggio riuscito.</summary>
        Private Shared Function CartellaDi(esito As EsitoTool) As String

            Assert.IsFalse(esito.Fallito, $"doveva riuscire: {esito.Spiegazione}")
            Return TryCast(esito.Dati, JsonObject)("cartella").GetValue(Of String)()

        End Function

#Region "salva_opportunita"

        <TestMethod>
        Public Async Function SenzaAnnuncioNonSiSalvaNiente() As Task

            Await ConMotore(
                Async Function(contesto)
                    Dim esito As EsitoTool = Await Chiama(
                        contesto, CatalogoTool.SalvaOpportunita, New JsonObject())

                    Assert.IsTrue(esito.Fallito, "un'opportunità senza annuncio non è un'opportunità")
                    StringAssert.Contains(esito.Spiegazione, "annuncio", "e si dice cosa manca")
                End Function)

        End Function

        <TestMethod>
        Public Async Function ConIlSoloAnnuncioNasceUnaCandidaturaNuova() As Task

            Await ConMotore(
                Async Function(contesto)
                    Dim esito As EsitoTool = Await Chiama(
                        contesto, CatalogoTool.SalvaOpportunita,
                        New JsonObject From {{"annuncio", Annuncio()}})

                    Dim dove As String = CartellaDi(esito)

                    Assert.AreEqual("nuova", TryCast(esito.Dati, JsonObject)("stato").GetValue(Of String)(),
                                    "senza confronto lo stato è «nuova» (cap. 07.3)")
                    Assert.IsTrue(Directory.Exists(
                        Path.Combine(contesto.Cartella.CartellaOpportunita, dove)),
                        "e la cartella c'è davvero")
                End Function)

        End Function

        <TestMethod>
        Public Async Function LeStelleLeCalcolaIlProgrammaAncheQui() As Task
            ' Il punto che vale l'intero tool: chi chiama può consegnare quel che vuole,
            ' ma il punteggio si rifà dai giudizi. Qui arriva un confronto che si
            ' auto-dichiara perfetto e porta un requisito eliminatorio non soddisfatto:
            ' l'hard-gate deve scattare lo stesso (cap. 09.5).
            Await ConMotore(
                Async Function(contesto)
                    Dim confronto As JsonNode = JsonNode.Parse(
                        "{ ""giudizi"": [ { ""requisito"": ""Patente C"", ""esito"": ""non soddisfatto"", " &
                        """eliminatorio"": true } ], ""numero_complessivo"": 10, " &
                        """match"": { ""stelle"": 5 } }")

                    Dim esito As EsitoTool = Await Chiama(
                        contesto, CatalogoTool.SalvaOpportunita,
                        New JsonObject From {{"annuncio", Annuncio()}, {"confronto", confronto}})

                    Dim salvato As JsonObject = TryCast(esito.Dati, JsonObject)
                    Assert.AreEqual("interessante", salvato("stato").GetValue(Of String)(),
                                    "col confronto la candidatura è «interessante»")

                    Dim match As JsonObject = TryCast(salvato("match"), JsonObject)
                    Assert.IsNotNull(match, "il punteggio dev'esserci")

                    Assert.IsTrue(match("gate_eliminatorio").GetValue(Of Boolean)(),
                                  "il requisito eliminatorio non soddisfatto impone il tetto")
                    Assert.IsTrue(match("stelle").GetValue(Of Double)() < 5,
                                  "e le cinque stelle dichiarate da fuori non contano niente")
                End Function)

        End Function

        <TestMethod>
        Public Async Function ConIDocumentiLaCandidaturaEGenerata() As Task

            Await ConMotore(
                Async Function(contesto)
                    Dim esito As EsitoTool = Await Chiama(
                        contesto, CatalogoTool.SalvaOpportunita,
                        New JsonObject From {{"annuncio", Annuncio()}, {"confronto", Confronto()},
                                             {"cv", Cv()}})

                    Assert.AreEqual("generata", TryCast(esito.Dati, JsonObject)("stato").GetValue(Of String)(),
                                    "lo stato lo dice quel che c'è, non chi chiama")
                End Function)

        End Function

        <TestMethod>
        Public Async Function DeiDocumentiSenzaIlConfrontoDaCuiNasconoNonSiSalvano() As Task
            ' Non è pignoleria: la macchina degli stati (cap. 07.3) dice che da «nuova» non
            ' si passa a «generata», e ha ragione — un CV mirato nasce dai giudizi. Una
            ' candidatura così sarebbe una storia che non può essere successa.
            Await ConMotore(
                Async Function(contesto)
                    Dim esito As EsitoTool = Await Chiama(
                        contesto, CatalogoTool.SalvaOpportunita,
                        New JsonObject From {{"annuncio", Annuncio()}, {"cv", Cv()}})

                    Assert.IsTrue(esito.Fallito, "un CV senza confronto non si mette in coda")
                    StringAssert.Contains(esito.Spiegazione, "confronto", "e si dice cosa serve")
                End Function)

        End Function

#End Region

#Region "Il lucchetto"

        <TestMethod>
        Public Async Function ConLaCartellaInUsoNonSiScriveESiDiceCheFare() As Task
            ' È l'applicazione aperta sullo schermo, quasi sempre. Il messaggio deve dire
            ' che cosa fare — chiudere la finestra — e che il resto continua a funzionare.
            Await ConMotore(
                Async Function(contesto)
                    Using altro As LucchettoDati = LucchettoDati.Prendi(contesto.Cartella)

                        Assert.IsNotNull(altro, "il lucchetto se lo prende qualcun altro")

                        Dim esito As EsitoTool = Await Chiama(
                            contesto, CatalogoTool.SalvaOpportunita,
                            New JsonObject From {{"annuncio", Annuncio()}})

                        Assert.IsTrue(esito.Fallito, "e allora da qui non si scrive")
                        StringAssert.Contains(esito.Spiegazione, "Chiudi la finestra", "si dice che fare")
                        StringAssert.Contains(esito.Spiegazione, "leggi_registro",
                                              "e che i tool di lettura funzionano lo stesso")

                    End Using
                End Function)

        End Function

        <TestMethod>
        Public Async Function RilasciatoIlLucchettoLaScritturaRiprende() As Task
            ' Il rovescio: chiusa l'applicazione, il server torna a poter scrivere. Senza
            ' questa prova, «non si scrive» potrebbe voler dire «non si scrive mai più».
            Await ConMotore(
                Async Function(contesto)
                    Dim altro As LucchettoDati = LucchettoDati.Prendi(contesto.Cartella)

                    Dim fermato As EsitoTool = Await Chiama(
                        contesto, CatalogoTool.SalvaOpportunita,
                        New JsonObject From {{"annuncio", Annuncio()}})
                    Assert.IsTrue(fermato.Fallito, "prima no")

                    altro.Dispose()

                    Dim passato As EsitoTool = Await Chiama(
                        contesto, CatalogoTool.SalvaOpportunita,
                        New JsonObject From {{"annuncio", Annuncio()}})
                    Assert.IsFalse(passato.Fallito, "e adesso sì")
                End Function)

        End Function

        <TestMethod>
        Public Async Function ILettoriNonSiFermanoPerIlLucchetto() As Task
            ' La promessa fatta nel messaggio d'errore va mantenuta, o è una bugia.
            Await ConMotore(
                Async Function(contesto)
                    Using altro As LucchettoDati = LucchettoDati.Prendi(contesto.Cartella)

                        Dim esito As EsitoTool = Await Chiama(
                            contesto, CatalogoTool.LeggiRegistro, New JsonObject())

                        Assert.IsFalse(esito.Fallito, "leggere non è scrivere")

                    End Using
                End Function)

        End Function

#End Region

#Region "esporta_documento"

        <TestMethod>
        Public Async Function SiImpaginaInDocxQuelCheLaCandidaturaHa() As Task

            Await ConMotore(
                Async Function(contesto)
                    Dim dove As String = CartellaDi(Await Chiama(
                        contesto, CatalogoTool.SalvaOpportunita,
                        New JsonObject From {{"annuncio", Annuncio()}, {"confronto", Confronto()},
                                             {"cv", Cv()}}))

                    Dim esito As EsitoTool = Await Chiama(
                        contesto, CatalogoTool.EsportaDocumento,
                        New JsonObject From {{"cartella", dove}})

                    Assert.IsFalse(esito.Fallito, $"doveva riuscire: {esito.Spiegazione}")

                    Dim prodotti As JsonArray = TryCast(TryCast(esito.Dati, JsonObject)("documenti"), JsonArray)
                    Assert.HasCount(1, prodotti, "il CV, e non la lettera che non c'è")
                    StringAssert.EndsWith(prodotti(0).GetValue(Of String)(), ".docx",
                                          "da qui escono DOCX: il PDF vuole la finestra")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaDocumentiDaImpaginareSiDiceComeFarli() As Task

            Await ConMotore(
                Async Function(contesto)
                    Dim dove As String = CartellaDi(Await Chiama(
                        contesto, CatalogoTool.SalvaOpportunita,
                        New JsonObject From {{"annuncio", Annuncio()}}))

                    Dim esito As EsitoTool = Await Chiama(
                        contesto, CatalogoTool.EsportaDocumento,
                        New JsonObject From {{"cartella", dove}})

                    Assert.IsTrue(esito.Fallito, "non c'è niente da impaginare")
                    StringAssert.Contains(esito.Spiegazione, "genera_cv", "e si dice da dove si comincia")
                End Function)

        End Function

        <TestMethod>
        Public Async Function DaQuiNonSiEsceDallaCartellaDati() As Task
            ' Stessa regola dei tool di lettura, e stessa funzione che la applica: qui
            ' arriva testo composto da un modello, e questo tool *scrive* (cap. 09.5).
            Await ConMotore(
                Async Function(contesto)
                    For Each tentativo As String In New String() {"..", "..\..\Windows", "C:\Windows",
                                                                  "sotto/cartella"}

                        Dim esito As EsitoTool = Await Chiama(
                            contesto, CatalogoTool.EsportaDocumento,
                            New JsonObject From {{"cartella", tentativo}})

                        Assert.IsTrue(esito.Fallito, $"«{tentativo}» non deve passare")

                    Next
                End Function)

        End Function

#End Region

    End Class

End Namespace
