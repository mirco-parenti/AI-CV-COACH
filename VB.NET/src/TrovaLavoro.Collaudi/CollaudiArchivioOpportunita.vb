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
        Public Sub LoStatoNonPortaPiuIlTestoDiPrimaDellaRifinitura()

            ' Il campo «rifinitura» si è scritto da T7b a T9d: teneva i testi da cui
            ' l'anti-slop era partito, per il prima/dopo di P6. Tolto quello, il testo di
            ' prima non serve più a nessuno — e un dato che nessuno legge è un dato che
            ' invecchia in silenzio dentro i file dell'utente (2026-08-22).
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())

                    Assert.DoesNotContain("""rifinitura""", File.ReadAllText(Path.Combine(dove, "stato.json")),
                                          "il campo non si scrive più")
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
        Public Sub UnTitoloAbnormeNonSfondaIlNomeDellaCartella()
            ' Azienda e titolo vengono dall'annuncio, cioè da un testo che l'utente non ha
            ' scritto: senza un tetto un titolo lungo si riverserebbe tutto nel percorso,
            ' e la cartella diventerebbe una che il disco fatica a reggere.
            ' (Revisione di sicurezza, 2026-09-01.)
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim azienda As String = String.Concat(Enumerable.Repeat("Rossi Industrie Riunite ", 20))
                    Dim titolo As String = String.Concat(Enumerable.Repeat("Tecnico manutenzione impianti ", 20))

                    Dim abnorme As Opportunita = OpportunitaDiProva()
                    abnorme.Annuncio = JsonNode.Parse($"{{""titolo"": ""{titolo}"", ""azienda"": ""{azienda}""}}")

                    Dim nome As String = Path.GetFileName(archivio.Salva(abnorme))

                    ' La data in testa è nostra e non si tocca: il tetto vale sui due pezzi
                    ' che arrivano dall'annuncio.
                    For Each pezzo As String In nome.Split("_"c).Skip(1)
                        Assert.IsLessThanOrEqualTo(40, pezzo.Length,
                                                   $"«{pezzo}» sfora il tetto dei 40 caratteri")
                    Next
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

        <TestMethod>
        Public Sub LeRiscrittureAManoTornanoDalDisco()

            ' R7. È tutto il punto della cura: l'avviso di «Rigenera» deve sopravvivere a
            ' un rientro in P6, e per farlo deve sopravvivere a un giro su disco.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim o As Opportunita = OpportunitaDiProva()
                    o.SegnaRiscritture(RuoloDocumento.Cv, {"sommario", "esperienza.1"},
                                       New Date(2026, 8, 23, 18, 40, 0))
                    o.SegnaLetteraGenerata(New Date(2026, 8, 23, 9, 0, 0))

                    Dim riletta As Opportunita = archivio.Carica(archivio.Salva(o))

                    Assert.AreEqual("sommario, esperienza.1", String.Join(", ", riletta.RiscrittureDelCv.Campi),
                                    "i campi riscritti a mano")
                    Assert.AreEqual(New Date(2026, 8, 23, 18, 40, 0), riletta.RiscrittureDelCv.Quando,
                                    "e quando l'utente ci ha messo mano")
                    Assert.AreEqual(New Date(2026, 8, 23, 9, 0, 0), riletta.LetteraGenerata,
                                    "la lettera è di stamattina")
                    Assert.IsTrue(riletta.LetteraDaRiallineare,
                                  "quindi riaprendo la candidatura la spia è ancora accesa")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaCandidaturaMaiToccataAManoNonPortaIlBloccoNuovo()

            ' La promessa fatta ai file già scritti (cap. 11.1): chi non ha niente da
            ' dichiarare resta sul disco esattamente com'era.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())
                    Dim scritto As String = File.ReadAllText(Path.Combine(dove, ArchivioOpportunita.FileStato))

                    Assert.DoesNotContain("riscritture", scritto, "nessun blocco delle riscritture")
                    Assert.DoesNotContain("lettera_generata", scritto, "e nessuna data della lettera")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaCartellaScrittaPrimaDiR7SiRiapreComeMaiToccataAMano()

            ' Non si deduce all'indietro una storia che nessuno ha registrato: una
            ' candidatura vecchia non è «riscritta a mano», è una di cui non si sa nulla.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim o As Opportunita = OpportunitaDiProva()
                    o.SegnaRiscritture(RuoloDocumento.Cv, {"sommario"}, New Date(2026, 8, 23, 18, 40, 0))

                    Dim dove As String = archivio.Salva(o)
                    Dim stato As String = Path.Combine(dove, ArchivioOpportunita.FileStato)

                    Dim vecchio As JsonObject = TryCast(JsonNode.Parse(File.ReadAllText(stato)), JsonObject)
                    vecchio.Remove("riscritture")
                    vecchio.Remove("lettera_generata")
                    File.WriteAllText(stato, vecchio.ToJsonString())

                    Dim riletta As Opportunita = archivio.Carica(dove)

                    Assert.IsFalse(riletta.RiscrittureDelCv.CEQualcosa, "nessuna riscrittura")
                    Assert.IsFalse(riletta.LetteraDaRiallineare, "e nessuna spia accesa senza motivo")
                End Sub)

        End Sub

        ' ==================================================================
        ' Le voci tolte dal CV (R6, 2026-08-24)
        ' ==================================================================

        <TestMethod>
        Public Sub LeVociTolteTornanoDalDisco()

            ' Gemella di LeRiscrittureAManoTornanoDalDisco, e per lo stesso motivo:
            ' il taglio che l'utente ha scelto su questo CV deve sopravvivere a un
            ' rientro in P6, e per farlo deve prima sopravvivere a un giro su disco.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim o As Opportunita = OpportunitaDiProva()
                    o.VociTolteDalCv.Togli("competenze¦uso del muletto", New Date(2026, 8, 24, 10, 0, 0))

                    Dim riletta As Opportunita = archivio.Carica(archivio.Salva(o))

                    Assert.IsTrue(riletta.VociTolteDalCv.Contiene("competenze¦uso del muletto"),
                                  "l'impronta della voce tolta")
                    Assert.AreEqual(New Date(2026, 8, 24, 10, 0, 0), riletta.VociTolteDalCv.Quando,
                                    "e quando l'utente l'ha tolta")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaCartellaScrittaPrimaDiR6SiRiapreComeDocumentoIntero()

            ' Come UnaCartellaScrittaPrimaDiR7SiRiapreComeMaiToccataAMano: uno
            ' stato.json senza «voci_tolte» — tutti quelli scritti prima di R6 — non
            ' deve inventare un taglio che nessuno ha mai fatto.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim o As Opportunita = OpportunitaDiProva()
                    o.VociTolteDalCv.Togli("competenze¦uso del muletto", New Date(2026, 8, 24, 10, 0, 0))

                    Dim dove As String = archivio.Salva(o)
                    Dim stato As String = Path.Combine(dove, ArchivioOpportunita.FileStato)

                    Dim vecchio As JsonObject = TryCast(JsonNode.Parse(File.ReadAllText(stato)), JsonObject)
                    vecchio.Remove("voci_tolte")
                    File.WriteAllText(stato, vecchio.ToJsonString())

                    Dim riletta As Opportunita = archivio.Carica(dove)

                    Assert.IsFalse(riletta.VociTolteDalCv.CEQualcosa,
                                   "documento intero, com'era prima che R6 esistesse")
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

        ''' <summary>
        ''' Le date su disco portano il loro fuso, e le vecchie si leggono ancora. Prima del
        ''' 2026-08-27 si scriveva l'ora locale nuda: rileggendola non si sapeva più di dove
        ''' fosse, e <c>Assert.AreEqual</c> fra due date non se ne accorge — confronta i
        ''' tick, non il <c>Kind</c>. Per questo qui il <c>Kind</c> si guarda apposta.
        ''' <i>(Revisione del giro D.)</i>
        ''' </summary>
        <TestMethod>
        Public Sub LaDataScrittaSuDiscoPortaIlSuoFuso()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())

                    Dim scritto As String = File.ReadAllText(Path.Combine(dove, "stato.json"))
                    Assert.Contains("2026-08-10 09:30:00", scritto, "l'istante resta leggibile a occhio")
                    Assert.IsTrue(Text.RegularExpressions.Regex.IsMatch(
                                      scritto, "2026-08-10 09:30:00[+-]\d\d:\d\d"),
                                  "e si porta dietro il fuso di chi l'ha scritto")

                    Dim riletta As Opportunita = archivio.Carica(dove)
                    Assert.AreEqual(New Date(2026, 8, 10, 9, 30, 0), riletta.Creata, "lo stesso istante")
                    Assert.AreEqual(DateTimeKind.Local, riletta.Creata.Kind,
                                    "e si sa ancora di dove sia: era ora locale, e resta ora locale")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LeDateScritteDallaVersionePrecedenteSiLeggonoAncora()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())
                    Dim percorsoStato As String = Path.Combine(dove, "stato.json")

                    ' Il formato di prima: la stessa data, senza fuso. È così che sono
                    ' scritti i file già sul disco dell'utente, e i backup già fatti.
                    File.WriteAllText(percorsoStato, Text.RegularExpressions.Regex.Replace(
                        File.ReadAllText(percorsoStato), "(2026-08-10 09:30:00)[+-]\d\d:\d\d", "$1"))

                    Dim riletta As Opportunita = archivio.Carica(dove)

                    Assert.AreEqual(New Date(2026, 8, 10, 9, 30, 0), riletta.Creata,
                                    "un dato che smette di leggersi è un dato perso")
                    Assert.AreEqual(DateTimeKind.Local, riletta.Creata.Kind,
                                    "e la si prende per quel che era: ora locale")
                End Sub)
        End Sub
        ' ==================================================================
        ' Eliminare una candidatura (cap. 11.5)
        ' ==================================================================

        <TestMethod>
        Public Sub EliminareUnaCandidaturaPortaViaTuttaLaSuaCartella()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())

                    ' Anche i documenti già esportati, che stanno nella sottocartella
                    ' out\: sono la cosa che l'utente ha in mano, e lasciarli sarebbe
                    ' cancellare a metà.
                    Directory.CreateDirectory(Path.Combine(dove, "out"))
                    File.WriteAllText(Path.Combine(dove, "out", "cv.pdf"), "finto")

                    Assert.IsTrue(archivio.Elimina(dove), "c'era qualcosa da mandare via")

                    Assert.IsFalse(Directory.Exists(dove), "la cartella non c'è più")
                    Assert.IsEmpty(archivio.Elenco(), "e l'archivio non la elenca")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub EliminareUnaCartellaCheNonCEDiceDiNoSenzaFarsiMale()

            ' L'utente è padrone dei suoi file (cap. 11.1): può averla mandata via da
            ' Esplora file un minuto prima. Non è un errore da sollevare, è che non
            ' c'era niente da fare.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim mai As String = Path.Combine(cartella.CartellaOpportunita,
                                                     "2026-08-10_mai-esistita")

                    Assert.IsFalse(archivio.Elimina(mai))
                End Sub)

        End Sub

        <TestMethod>
        Public Sub NonSiEliminaNienteFuoriDallaCartellaDelleCandidature()

            ' La guardia non è diffidenza verso chi chiama: il parametro è un percorso, e
            ' un percorso sbagliato qui non sbaglia un dato — porta via una cartella
            ' dell'utente che non c'entra niente.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim dove As String = archivio.Salva(OpportunitaDiProva())

                    Dim profilo As String = cartella.CartellaProfilo
                    Directory.CreateDirectory(profilo)

                    Assert.ThrowsExactly(Of ArgumentException)(Function() archivio.Elimina(profilo))
                    Assert.IsTrue(Directory.Exists(profilo), "e non l'ha toccata")

                    ' Nemmeno una cartella più in giù: «out\» sta dentro una candidatura,
                    ' non dentro «opportunita\», e la si manda via solo con lei.
                    Dim dentro As String = Path.Combine(dove, "out")
                    Directory.CreateDirectory(dentro)

                    Assert.ThrowsExactly(Of ArgumentException)(Function() archivio.Elimina(dentro))
                    Assert.IsTrue(Directory.Exists(dentro), "nemmeno quella")
                End Sub)

        End Sub


    End Class

End Namespace
