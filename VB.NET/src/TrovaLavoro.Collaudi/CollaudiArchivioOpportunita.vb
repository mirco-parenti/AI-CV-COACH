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
        Public Sub UnaCartellaCheNonCESiDiceChiaramente()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Assert.Throws(Of DirectoryNotFoundException)(
                        Function() archivio.Carica(Path.Combine(cartella.CartellaOpportunita, "mai-esistita")))
                End Sub)
        End Sub

    End Class

End Namespace
