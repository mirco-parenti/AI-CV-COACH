Imports System.IO
Imports System.Linq
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Collaudi dell'archivio delle opportunità (cap. 11.1). T4 genera i primi documenti
    ''' veri del progetto: qui si verifica che atterrino in una cartella che si ritrova —
    ''' col nome parlante, senza sovrascriversi fra loro — e che riaprirla restituisca
    ''' quello che c'era, compresa un'opportunità rimasta a metà.
    ''' </summary>
    <TestClass>
    Public Class CollaudiArchivioOpportunita

        Private Shared Sub ConArchivioTemporaneo(prova As Action(Of ArchivioOpportunita, CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "archivio-opportunita-" & Guid.NewGuid().ToString("N"))
            Dim cartella As New CartellaDati(radice)
            Try
                prova(New ArchivioOpportunita(cartella), cartella)
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

        ''' <summary>Un'opportunità completa, come esce dalla pipeline.</summary>
        Private Shared Function OpportunitaDiProva() As Opportunita

            Return New Opportunita With {
                .Creata = New Date(2026, 8, 10, 9, 30, 0),
                .VersioneProfilo = "2026-08-10_092500",
                .Annuncio = JsonNode.Parse(
                    "{""titolo"": ""Tecnico manutenzione"", ""azienda"": ""Rossi S.p.A.""}"),
                .Confronto = JsonNode.Parse(
                    "{""giudizi"": [{""requisito"": ""Patente B"", ""esito"": ""soddisfatto""}]," &
                    """lettura_insieme"": ""Buon profilo"", ""numero_complessivo"": 82}"),
                .Match = New RisultatoMatch With {
                    .MatchFinale = 82, .Stelle = 4.1, .ScoreBase = 80, .NumeroLlm = 82,
                    .ScartoTagliato = False, .GateEliminatorio = False, .Nota = Nothing},
                .Mitigazioni = JsonNode.Parse("{""mitigazioni"": []}"),
                .Cv = JsonNode.Parse("{""intestazione"": {""nome"": ""Luca Ferrari""}}"),
                .Lettera = JsonNode.Parse("{""corpo"": ""Gentile azienda…""}")}

        End Function

        <TestMethod>
        Public Sub IlPrimaDellaRifinituraViaggiaConLoStato()

            ' T7b, cap. 08.4: i testi di prima stanno nello stato.json e non dentro i
            ' documenti, perché il CV mirato finisce nel prompt della lettera e la lettera
            ' in quello dell'email — un campo di servizio scritto nel documento arriverebbe
            ' fin dentro quelle richieste.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim o As Opportunita = OpportunitaDiProva()
                    o.PrimaDellaRifinitura = JsonNode.Parse(
                        "{""cv"": {""sommario"": ""com'era il sommario""}," &
                        """lettera"": {""corpo"": ""com'era il corpo""}}")

                    Dim dove As String = archivio.Salva(o)

                    Assert.DoesNotContain("com'era", File.ReadAllText(Path.Combine(dove, "cv.json")),
                                          "il documento resta il documento")

                    Dim riletta As Opportunita = archivio.Carica(dove)

                    Assert.AreEqual("com'era il sommario",
                                    riletta.PrimaDellaRifinitura("cv")("sommario").GetValue(Of String)(),
                                    "il prima del CV si rilegge")
                    Assert.AreEqual("com'era il corpo",
                                    riletta.PrimaDellaRifinitura("lettera")("corpo").GetValue(Of String)(),
                                    "e quello della lettera")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub SenzaRifinituraLoStatoNonHaQuelCampo()

            ' Scriverlo vuoto direbbe «è stata fatta e non ha toccato niente» esattamente
            ' come non scriverlo — e un file si legge meglio senza righe che non raccontano.
            ' Vale anche per le cartelle nate prima di T7b, che si riaprono uguali.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())

                    Assert.DoesNotContain("""rifinitura""", File.ReadAllText(Path.Combine(dove, "stato.json")),
                                          "nessun campo scritto per simmetria")
                    Assert.IsNull(archivio.Carica(dove).PrimaDellaRifinitura,
                                  "e riaprendola non c'è niente da mostrare")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub LaCartellaHaIlNomeParlante()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())

                    ' Data, azienda e ruolo, ridotti a qualcosa che qualunque disco
                    ' accetta e un umano riconosce (cap. 11.1).
                    Assert.AreEqual("2026-08-10_rossi-s-p-a_tecnico-manutenzione",
                                    Path.GetFileName(dove), "il nome della cartella")
                    Assert.IsTrue(Directory.Exists(dove), "la cartella deve esserci")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub OgniArtefattoNelSuoFile()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())

                    For Each atteso As String In {"annuncio.json", "giudizi.json", "mitigazioni.json",
                                                  "cv.json", "lettera.json", "stato.json"}
                        Assert.IsTrue(File.Exists(Path.Combine(dove, atteso)), $"manca {atteso}")
                    Next

                    ' Leggibile senza l'app: rientri e accenti in chiaro (cap. 11.1).
                    Dim scritto As String = File.ReadAllText(Path.Combine(dove, "annuncio.json"))
                    Assert.Contains(vbLf & "  ""titolo"":", scritto.Replace(vbCrLf, vbLf), "coi rientri")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SiScriveSoloCioCheCE()
            ' Nel flusso reale l'utente guarda il confronto e decide se proseguire
            ' (cap. 12, A5→A7): fra un passo e l'altro l'opportunità è a metà, e un file
            ' vuoto scritto per simmetria sarebbe una bugia sul disco.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim aMeta As Opportunita = OpportunitaDiProva()
                    aMeta.Cv = Nothing
                    aMeta.Lettera = Nothing

                    Dim dove As String = archivio.Salva(aMeta)

                    Assert.IsTrue(File.Exists(Path.Combine(dove, "giudizi.json")), "i giudizi ci sono")
                    Assert.IsFalse(File.Exists(Path.Combine(dove, "cv.json")), "il CV no")
                    Assert.IsFalse(File.Exists(Path.Combine(dove, "lettera.json")), "la lettera nemmeno")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SalvaERilegge()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim originale As Opportunita = OpportunitaDiProva()
                    Dim dove As String = archivio.Salva(originale)

                    Dim riletta As Opportunita = archivio.Carica(dove)

                    Assert.AreEqual("Rossi S.p.A.", riletta.Azienda, "l'azienda")
                    Assert.AreEqual("Tecnico manutenzione", riletta.Titolo, "il ruolo")
                    Assert.AreEqual("2026-08-10_092500", riletta.VersioneProfilo,
                                    "la versione di profilo da cui sono nati i documenti")
                    Assert.AreEqual("it", riletta.Lingua, "la lingua")
                    Assert.AreEqual(New Date(2026, 8, 10, 9, 30, 0), riletta.Creata, "quando è nata")
                    Assert.HasCount(1, riletta.Giudizi(), "i giudizi")
                    Assert.AreEqual("Gentile azienda…", riletta.Lettera("corpo").ToString(), "la lettera")

                    ' Il punteggio si riconserva perché è il giudizio di quel giorno, con
                    ' quella taratura: ricalcolarlo domani potrebbe dare un altro numero.
                    Assert.AreEqual(82, riletta.Match.MatchFinale, "il match")
                    Assert.AreEqual(4.1, riletta.Match.Stelle, "le stelle")
                    Assert.IsFalse(riletta.Match.GateEliminatorio, "nessun eliminatorio")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnaSecondaCandidaturaAllaStessaAziendaNonSovrascriveLaPrima()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim prima As String = archivio.Salva(OpportunitaDiProva())
                    Dim seconda As String = archivio.Salva(OpportunitaDiProva())

                    Assert.AreNotEqual(prima, seconda, "due cartelle diverse")
                    Assert.EndsWith("_2", Path.GetFileName(seconda), "la seconda prende il progressivo")
                    Assert.HasCount(2, archivio.Elenco(), "e l'elenco ne vede due")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SalvareDueVolteLaStessaOpportunitaNonNeCreaUnaSeconda()
            ' L'opportunità cresce: prima il confronto, poi i documenti. È la stessa
            ' candidatura, e deve restare nella stessa cartella.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim o As Opportunita = OpportunitaDiProva()
                    o.Cv = Nothing
                    o.Lettera = Nothing

                    Dim prima As String = archivio.Salva(o)

                    o.Cv = JsonNode.Parse("{""intestazione"": {""nome"": ""Luca Ferrari""}}")
                    Dim dopo As String = archivio.Salva(o)

                    Assert.AreEqual(prima, dopo, "la stessa cartella")
                    Assert.HasCount(1, archivio.Elenco(), "una sola opportunità")
                    Assert.IsTrue(File.Exists(Path.Combine(dopo, "cv.json")), "col CV arrivato dopo")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnAnnuncioAnonimoHaComunqueUnaCartella()
            ' «Azienda leader del settore» non ha un nome da riportare, e il prompt lascia
            ' il campo vuoto invece di inventarlo (Pool 1.03): la cartella deve nascere lo
            ' stesso, e non chiamarsi solo con la data.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim anonima As Opportunita = OpportunitaDiProva()
                    anonima.Annuncio = JsonNode.Parse("{""titolo"": """", ""azienda"": """"}")

                    Dim dove As String = archivio.Salva(anonima)

                    Assert.AreEqual("2026-08-10_opportunita", Path.GetFileName(dove), "il nome di ripiego")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LaProvenienzaSiScriveESiRilegge()
            ' Da T5b un annuncio catturato porta con sé da dove viene (cap. 06.4): il
            ' link è ciò che permette di tornare all'originale mesi dopo.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim catturata As Opportunita = OpportunitaDiProva()
                    catturata.Fonte = "Indeed"
                    catturata.Link = "https://it.indeed.com/viewjob?jk=9f3c1a"

                    Dim riletta As Opportunita = archivio.Carica(archivio.Salva(catturata))

                    Assert.AreEqual("Indeed", riletta.Fonte, "da che portale veniva")
                    Assert.AreEqual("https://it.indeed.com/viewjob?jk=9f3c1a", riletta.Link, "e da quale pagina")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnOpportunitaScrittaPrimaDiT5bSiRiapreLoStesso()
            ' Le opportunità di T4 hanno uno stato.json senza «fonte» né «link»: riaprirle
            ' non deve rompersi, e non deve inventare una provenienza che non c'era.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())
                    Dim stato As String = Path.Combine(dove, ArchivioOpportunita.FileStato)

                    Dim vecchio As JsonObject = TryCast(JsonNode.Parse(File.ReadAllText(stato)), JsonObject)
                    vecchio.Remove("fonte")
                    vecchio.Remove("link")
                    File.WriteAllText(stato, vecchio.ToJsonString())

                    Dim riletta As Opportunita = archivio.Carica(dove)

                    Assert.IsNull(riletta.Fonte, "nessuna provenienza inventata")
                    Assert.IsNull(riletta.Link)
                    Assert.AreEqual("Rossi S.p.A.", riletta.Azienda, "e il resto si rilegge come sempre")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LoStessoIndirizzoSiRitrova()
            ' È così che la cattura sa di aver già preso quella pagina, e non rianalizza
            ' due volte lo stesso annuncio (cap. 06.4).
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim catturata As Opportunita = OpportunitaDiProva()
                    catturata.Link = "https://it.indeed.com/viewjob?jk=9f3c1a"

                    Dim dove As String = archivio.Salva(catturata)

                    Assert.AreEqual(dove, archivio.CercaPerLink("https://it.indeed.com/viewjob?jk=9f3c1a"))
                    Assert.IsNull(archivio.CercaPerLink("https://it.indeed.com/viewjob?jk=unaltro"),
                                  "un altro annuncio non è un doppione")

                    ' Le opportunità nate prima di T5b non hanno link: non devono mai
                    ' rispondere «ci sono già io».
                    For Each niente As String In {Nothing, "", "   "}
                        Assert.IsNull(archivio.CercaPerLink(niente))
                    Next
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnoStatoRovinatoAManoNonImpedisceDiCatturare()
            ' L'utente è padrone dei suoi file (cap. 11.1): uno stato.json illeggibile si
            ' scavalca, invece di far cadere la cattura che sta solo facendo una domanda.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim rovinata As Opportunita = OpportunitaDiProva()
                    Dim dove As String = archivio.Salva(rovinata)
                    File.WriteAllText(Path.Combine(dove, ArchivioOpportunita.FileStato), "{ questo non è json")

                    Assert.IsNull(archivio.CercaPerLink("https://it.indeed.com/viewjob?jk=9f3c1a"))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LoStatoEIsuoiPassaggiSiScrivonoESiRileggono()
            ' Il campo che T4 non aveva e T5c porta (cap. 07.3, cap. 11.1).
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim generata As Opportunita = OpportunitaDiProva()
                    generata.Avanza(StatoOpportunita.Interessante, New Date(2026, 8, 10, 9, 31, 0))
                    generata.Avanza(StatoOpportunita.Generata, New Date(2026, 8, 10, 9, 40, 0))

                    Dim riletta As Opportunita = archivio.Carica(archivio.Salva(generata))

                    Assert.AreEqual(StatoOpportunita.Generata, riletta.Stato, "a che punto è")
                    Assert.HasCount(2, riletta.DateStati, "e quando ci è arrivata")
                    Assert.AreEqual(New Date(2026, 8, 10, 9, 40, 0),
                                    riletta.DateStati(StatoOpportunita.Generata))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnOpportunitaScrittaPrimaDiT5cSiRiapreColSuoStatoDedotto()
            ' Le cartelle di T4 non dichiarano lo stato: non si migrano, si guarda cosa
            ' hanno dentro (deciso con Mirco il 2026-08-12).
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim aMeta As Opportunita = OpportunitaDiProva()
                    aMeta.Cv = Nothing
                    aMeta.Lettera = Nothing

                    Assert.AreEqual(StatoOpportunita.Interessante, ComeT4(archivio, aMeta).Stato,
                                    "ci sono i giudizi: è stata confrontata")

                    Dim intera As Opportunita = OpportunitaDiProva()
                    Assert.AreEqual(StatoOpportunita.Generata, ComeT4(archivio, intera).Stato,
                                    "c'è il CV: è stata generata")

                    Dim appenaLetta As Opportunita = OpportunitaDiProva()
                    appenaLetta.Confronto = Nothing
                    appenaLetta.Mitigazioni = Nothing
                    appenaLetta.Cv = Nothing
                    appenaLetta.Lettera = Nothing
                    Assert.AreEqual(StatoOpportunita.Nuova, ComeT4(archivio, appenaLetta).Stato,
                                    "c'è solo l'annuncio: è nuova")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnoStatoScrittoAManoCheNonEsisteNonSiIndovina()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())
                    Dim stato As String = Path.Combine(dove, ArchivioOpportunita.FileStato)

                    Dim corretto As JsonObject = TryCast(JsonNode.Parse(File.ReadAllText(stato)), JsonObject)
                    corretto("stato") = "quasi fatta"
                    File.WriteAllText(stato, corretto.ToJsonString())

                    ' Non è una dichiarazione che possiamo credere: si torna ai fatti.
                    Assert.AreEqual(StatoOpportunita.Generata, archivio.Carica(dove).Stato)
                End Sub)
        End Sub

        ''' <summary>
        ''' Salva l'opportunità e le toglie di dosso ciò che T4 non scriveva, poi la
        ''' rilegge: è il modo di avere in mano una cartella com'era prima di T5c.
        ''' </summary>
        Private Shared Function ComeT4(archivio As ArchivioOpportunita, o As Opportunita) As Opportunita

            Dim dove As String = archivio.Salva(o)
            Dim stato As String = Path.Combine(dove, ArchivioOpportunita.FileStato)

            Dim vecchio As JsonObject = TryCast(JsonNode.Parse(File.ReadAllText(stato)), JsonObject)
            vecchio.Remove("stato")
            vecchio.Remove("date_stati")
            File.WriteAllText(stato, vecchio.ToJsonString())

            Return archivio.Carica(dove)

        End Function

        <TestMethod>
        Public Sub UnaCartellaCheNonCESiDiceChiaramente()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Assert.Throws(Of DirectoryNotFoundException)(
                        Function() archivio.Carica(Path.Combine(cartella.CartellaOpportunita, "mai-esistita")))
                End Sub)
        End Sub

    End Class

End Namespace
