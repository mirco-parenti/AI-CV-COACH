Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Collaudi del registro delle candidature (cap. 07.3): la vista d'insieme che
    ''' risponde alla domanda «a che punto sono?».
    ''' </summary>
    ''' <remarks>
    ''' Quel che c'è da provare qui non è tanto cosa il registro contiene, ma che
    ''' <b>non sia mai lui la verità</b>: le cartelle-opportunità restano la fonte, e
    ''' <c>registro.json</c> è un indice che si ricostruisce quando manca, quando non si
    ''' legge e quando non torna coi fatti. Metà di questi collaudi rompe apposta il file,
    ''' o il disco sotto il file, per vedere se l'elenco si rialza.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiRegistro

        Private Shared Sub ConArchivioTemporaneo(
            prova As Action(Of ArchivioRegistro, ArchivioOpportunita, CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "registro-" & Guid.NewGuid().ToString("N"))
            Dim cartella As New CartellaDati(radice)
            Dim opportunita As New ArchivioOpportunita(cartella)

            Try
                prova(New ArchivioRegistro(cartella, opportunita), opportunita, cartella)
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

        ''' <summary>
        ''' Una candidatura confrontata, con la sua provenienza e il suo punteggio: è la
        ''' forma in cui arriva al registro nel flusso vero.
        ''' </summary>
        Private Shared Function Candidatura(azienda As String, Optional giorno As Integer = 10) As Opportunita

            Dim o As New Opportunita With {
                .Creata = New Date(2026, 8, giorno, 9, 30, 0),
                .Fonte = "Indeed",
                .Link = $"https://it.indeed.com/viewjob?jk={azienda.ToLowerInvariant()}",
                .Annuncio = JsonNode.Parse(
                    $"{{""titolo"": ""Tecnico manutenzione"", ""azienda"": ""{azienda}""}}"),
                .Confronto = JsonNode.Parse("{""giudizi"": [{""requisito"": ""Patente B""}]}"),
                .Match = New RisultatoMatch With {
                    .MatchFinale = 82, .Stelle = 4.1, .GateEliminatorio = False}}

            o.Avanza(StatoOpportunita.Nuova, o.Creata)
            o.Avanza(StatoOpportunita.Interessante, o.Creata.AddMinutes(2))

            Return o

        End Function

        <TestMethod>
        Public Sub LIndiceDescriveLeCandidatureCheCiSono()
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    Dim dove As String = candidature.Salva(Candidatura("Rossi S.p.A."))

                    Dim letto As Registro = indice.Carica()

                    Assert.HasCount(1, letto.Voci)
                    Dim voce As VoceRegistro = letto.Voci.Single()

                    ' La cartella si segna col nome e non col percorso: la cartella dati
                    ' si può spostare, e un indice di percorsi assoluti smetterebbe di
                    ' valere (cap. 11.1, cap. 11.4).
                    Assert.AreEqual(Path.GetFileName(dove), voce.Cartella)
                    Assert.AreEqual("Rossi S.p.A.", voce.Azienda, "chi offre il posto")
                    Assert.AreEqual("Tecnico manutenzione", voce.Titolo, "per quale ruolo")
                    Assert.AreEqual("Indeed", voce.Fonte, "da dove veniva l'annuncio")
                    Assert.AreEqual(4.1, voce.Stelle, "le stelle del match")
                    Assert.IsFalse(voce.GateEliminatorio, "nessun ⛔")
                    Assert.AreEqual(StatoOpportunita.Interessante, voce.Stato, "a che punto è")
                    Assert.AreEqual(New Date(2026, 8, 10, 9, 32, 0),
                                    voce.DateStati(StatoOpportunita.Interessante), "e da quando")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaFileLIndiceSiRicostruisceEBasta()
            ' È il primo giro di ogni installazione: il registro non c'è ancora, e non è
            ' un'anomalia da raccontare.
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    candidature.Salva(Candidatura("Rossi S.p.A."))

                    Dim letto As Registro = indice.Carica()

                    Assert.IsTrue(letto.Rigenerato, "ricostruito dalle cartelle")
                    Assert.IsNull(letto.Avviso, "senza niente da dire")
                    Assert.HasCount(1, letto.Voci)
                End Sub)
        End Sub

        <TestMethod>
        Public Sub RicostruireNonScriveNiente()
            ' Chi apre l'applicazione e la richiude senza fare niente non deve trovarsi
            ' dei file nuovi: a salvare è chi decide, non chi guarda.
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    candidature.Salva(Candidatura("Rossi S.p.A."))

                    indice.Carica()

                    Assert.IsFalse(File.Exists(cartella.FileRegistro), "il file non è nato da solo")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnIndiceCheCombaciaSiLeggeDalFile()
            ' Se torna coi fatti, l'indice si usa com'è: è il senso di averlo.
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    candidature.Salva(Candidatura("Rossi S.p.A."))

                    Dim scritto As Registro = indice.Carica()
                    scritto.Voci.Single().Titolo = "Un titolo che solo il file conosce"
                    indice.Salva(scritto)

                    Dim riletto As Registro = indice.Carica()

                    Assert.IsFalse(riletto.Rigenerato, "non c'era niente da ricostruire")
                    Assert.AreEqual("Un titolo che solo il file conosce", riletto.Voci.Single().Titolo)
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnIndiceIlleggibileSiRifa()
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    candidature.Salva(Candidatura("Rossi S.p.A."))
                    cartella.Assicura()
                    File.WriteAllText(cartella.FileRegistro, "{ questo non è json")

                    Dim letto As Registro = indice.Carica()

                    Assert.IsTrue(letto.Rigenerato, "un indice illeggibile è un indice da rifare")
                    Assert.AreEqual("Rossi S.p.A.", letto.Voci.Single().Azienda)
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnaCartellaSparitaAManoFaRicostruire()
            ' L'utente è padrone dei suoi dati (cap. 11.1): può cancellare una cartella da
            ' Esplora file, e l'elenco non deve continuare a mostrare una candidatura che
            ' non c'è più.
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    Dim prima As String = candidature.Salva(Candidatura("Rossi S.p.A.", giorno:=10))
                    candidature.Salva(Candidatura("Bianchi S.r.l.", giorno:=11))

                    indice.Salva(indice.Carica())
                    Directory.Delete(prima, recursive:=True)

                    Dim letto As Registro = indice.Carica()

                    Assert.IsTrue(letto.Rigenerato, "l'indice non tornava coi fatti")
                    Assert.HasCount(1, letto.Voci)
                    Assert.AreEqual("Bianchi S.r.l.", letto.Voci.Single().Azienda, "resta quella che c'è")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnaCartellaComparsaAManoEntraNellIndice()
            ' Il verso opposto: una candidatura arrivata da un backup (cap. 11.4) deve
            ' comparire nell'elenco senza che nessuno glielo chieda.
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    candidature.Salva(Candidatura("Rossi S.p.A.", giorno:=10))
                    indice.Salva(indice.Carica())

                    candidature.Salva(Candidatura("Bianchi S.r.l.", giorno:=11))

                    Dim letto As Registro = indice.Carica()

                    Assert.IsTrue(letto.Rigenerato)
                    Assert.HasCount(2, letto.Voci)
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LOrdineEQuelloDelTempo()
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    candidature.Salva(Candidatura("Bianchi S.r.l.", giorno:=11))
                    candidature.Salva(Candidatura("Rossi S.p.A.", giorno:=10))

                    ' Il nome della cartella comincia con la data: l'ordine alfabetico è
                    ' già l'ordine del tempo, dalla più vecchia alla più recente.
                    Assert.AreEqual("Rossi S.p.A., Bianchi S.r.l.",
                                    String.Join(", ", indice.Carica().Voci.Select(Function(v) v.Azienda)))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnaCandidaturaRovinataNonPortaViaLeAltre()
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    Dim rovinata As String = candidature.Salva(Candidatura("Rossi S.p.A.", giorno:=10))
                    candidature.Salva(Candidatura("Bianchi S.r.l.", giorno:=11))

                    File.WriteAllText(Path.Combine(rovinata, ArchivioOpportunita.FileAnnuncio),
                                      "{ questo non è json")

                    Dim letto As Registro = indice.Carica()

                    Assert.HasCount(1, letto.Voci, "l'altra candidatura resta nell'elenco")

                    ' Ma sparire in silenzio no: chi cerca quella candidatura deve sapere
                    ' perché non la trova più.
                    Assert.Contains(Path.GetFileName(rovinata), letto.Avviso,
                                    "l'avviso dice quale non si è lasciata leggere")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub AnnotareAggiornaLaVoceESalvaSubito()
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    Dim rossi As Opportunita = Candidatura("Rossi S.p.A.")
                    candidature.Salva(rossi)
                    indice.Annota(rossi)

                    ' L'opportunità cresce: i documenti arrivano dopo il confronto, e
                    ' l'indice deve seguirla senza sdoppiarla.
                    rossi.Cv = JsonNode.Parse("{""intestazione"": {}}")
                    rossi.Avanza(StatoOpportunita.Generata)
                    candidature.Salva(rossi)

                    Dim letto As Registro = indice.Annota(rossi)

                    Assert.IsFalse(letto.Rigenerato, "è quello appena scritto")
                    Assert.HasCount(1, letto.Voci, "una sola candidatura")
                    Assert.AreEqual(StatoOpportunita.Generata, letto.Voci.Single().Stato)
                    Assert.IsTrue(File.Exists(cartella.FileRegistro), "e sta su disco")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IContatoriDiconoAChePuntoSiE()
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    Dim generata As Opportunita = Candidatura("Rossi S.p.A.", giorno:=10)
                    generata.Cv = JsonNode.Parse("{""intestazione"": {}}")
                    generata.Avanza(StatoOpportunita.Generata)
                    candidature.Salva(generata)

                    Dim scartata As Opportunita = Candidatura("Bianchi S.r.l.", giorno:=11)
                    scartata.Avanza(StatoOpportunita.Scartata)
                    candidature.Salva(scartata)

                    candidature.Salva(Candidatura("Verdi & C.", giorno:=12))

                    Dim letto As Registro = indice.Carica()

                    Assert.AreEqual(1, letto.Quante(StatoOpportunita.Generata))
                    Assert.AreEqual(1, letto.Quante(StatoOpportunita.Scartata))
                    Assert.AreEqual(1, letto.Quante(StatoOpportunita.Interessante))
                    Assert.AreEqual(0, letto.Quante(StatoOpportunita.Inviata), "T6 non è ancora arrivata")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LoStatoDedottoDiUnaCartellaDiT4EntraNellIndice()
            ' Le candidature scritte prima di T5c non dichiarano lo stato: nell'elenco
            ' devono comparire lo stesso, al posto giusto (cap. 11.1).
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    Dim vecchia As Opportunita = Candidatura("Rossi S.p.A.")
                    vecchia.Cv = JsonNode.Parse("{""intestazione"": {}}")
                    Dim dove As String = candidature.Salva(vecchia)

                    Dim stato As String = Path.Combine(dove, ArchivioOpportunita.FileStato)
                    Dim comeT4 As JsonObject = JsonNode.Parse(File.ReadAllText(stato)).AsObject()
                    comeT4.Remove("stato")
                    comeT4.Remove("date_stati")
                    File.WriteAllText(stato, comeT4.ToJsonString())

                    Assert.AreEqual(StatoOpportunita.Generata, indice.Carica().Voci.Single().Stato,
                                    "il CV c'è: quella candidatura è stata generata")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlFileSiRileggeComEStatoScritto()
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    candidature.Salva(Candidatura("Rossi S.p.A."))
                    indice.Salva(indice.Carica())

                    ' Leggibile senza l'app, coi rientri e gli accenti in chiaro (cap. 11.1).
                    Dim scritto As String = File.ReadAllText(cartella.FileRegistro)
                    Assert.Contains(vbLf & "  ""voci"":", scritto.Replace(vbCrLf, vbLf), "coi rientri")

                    Dim riletto As Registro = Registro.DaJson(scritto)
                    Dim voce As VoceRegistro = riletto.Voci.Single()

                    Assert.AreEqual("Rossi S.p.A.", voce.Azienda)
                    Assert.AreEqual(4.1, voce.Stelle)
                    Assert.AreEqual(StatoOpportunita.Interessante, voce.Stato)
                    Assert.AreEqual("https://it.indeed.com/viewjob?jk=rossi s.p.a.", voce.Link)
                    Assert.HasCount(2, voce.DateStati, "le due date dei passaggi")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnaVoceSenzaCartellaNonServeANiente()
            ' La cartella è l'identità della voce: senza, non si sa nemmeno di chi parla.
            Dim letto As Registro = Registro.DaJson(
                "{""voci"": [{""azienda"": ""Rossi S.p.A.""}, {""cartella"": ""2026-08-10_rossi""}]}")

            Assert.HasCount(1, letto.Voci)
            Assert.AreEqual("2026-08-10_rossi", letto.Voci.Single().Cartella)

        End Sub

        <TestMethod>
        Public Sub UnRegistroCheNonEUnOggettoSiDiceChiaramente()
            Assert.Throws(Of JsonException)(Function() Registro.DaJson("[1, 2, 3]"))
        End Sub

        <TestMethod>
        Public Sub IlDestinatarioDellaBozzaArrivaNellIndice()
            ' Cap. 07.3: l'indice deve saper rispondere «a chi ho scritto?» senza far
            ' aprire una candidatura alla volta. Il valore resta quello della bozza — qui
            ' si ricopia soltanto.
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    ' Attenzione al nome: in VB una locale che si chiama come la funzione
                    ' che la contiene la copre, e la chiamata viene letta come
                    ' un'indicizzazione (trappola già pagata in ContestoApp.MontaAi).
                    Dim daMandare As Opportunita = Candidatura("Rossi S.p.A.")
                    daMandare.Email = JsonNode.Parse(
                        "{""destinatario"": ""lavoro@rossi.it"", ""oggetto"": ""Candidatura""}")
                    candidature.Salva(daMandare)

                    Assert.AreEqual("lavoro@rossi.it", indice.Carica().Voci.Single().Destinatario,
                                    "il destinatario arriva dalla bozza")

                    ' E sopravvive al giro su disco, che è la ragione per cui sta nell'indice.
                    indice.Salva(indice.Carica())
                    Dim riletto As Registro = Registro.DaJson(File.ReadAllText(cartella.FileRegistro))

                    Assert.AreEqual("lavoro@rossi.it", riletto.Voci.Single().Destinatario,
                                    "e si rilegge dal file")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaBozzaIlDestinatarioRestaVuotoSenzaLamentarsi()
            ' Una candidatura si passeggia a lungo prima di avere un'email: finché la
            ' bozza non c'è il campo è vuoto, e non è un guasto da segnalare.
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    candidature.Salva(Candidatura("Bianchi S.r.l."))

                    Assert.IsNull(indice.Carica().Voci.Single().Destinatario,
                                  "nessuna bozza, nessun destinatario")
                End Sub)
        End Sub

        ' ==================================================================
        ' L'esito e il promemoria di follow-up (T9c, cap. 07.3)
        ' ==================================================================

        ''' <summary>Una candidatura spedita nel giorno indicato, senza esito.</summary>
        Private Shared Function Spedita(azienda As String, quando As Date) As VoceRegistro

            Dim voce As New VoceRegistro With {
                .Cartella = azienda.ToLowerInvariant().Replace(" ", "-"),
                .Stato = StatoOpportunita.Inviata,
                .Azienda = azienda}

            voce.DateStati(StatoOpportunita.Inviata) = quando

            Return voce

        End Function

        <TestMethod>
        Public Sub LEsitoEntraNellIndiceESiRilegge()
            ConArchivioTemporaneo(
                Sub(indice, candidature, cartella)
                    Dim andata As Opportunita = Candidatura("Rossi S.p.A.")
                    andata.Cv = JsonNode.Parse("{""intestazione"": {}}")
                    andata.Avanza(StatoOpportunita.Generata)
                    andata.Avanza(StatoOpportunita.Inviata)
                    andata.SegnaEsito(EsitoCandidatura.Colloquio)

                    candidature.Salva(andata)

                    Assert.AreEqual(EsitoCandidatura.Colloquio, indice.Carica().Voci.Single().Esito)

                    indice.Salva(indice.Carica())
                    Dim riletto As Registro = Registro.DaJson(File.ReadAllText(cartella.FileRegistro))

                    Assert.AreEqual(EsitoCandidatura.Colloquio, riletto.Voci.Single().Esito,
                                    "e si rilegge dal file, come lo stato")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnIndiceCheSiContraddiceNonInventaUnEsito()

            ' Il file si può correggere a mano (cap. 11.1). Qui dichiara un esito su una
            ' candidatura scartata: vince lo stato, che è il campo che tutti guardano.
            Dim scritta As JsonObject = TryCast(JsonNode.Parse(
                "{""cartella"": ""acme"", ""stato"": ""scartata"", ""esito"": ""assunto""}"), JsonObject)

            Dim voce As VoceRegistro = VoceRegistro.DaJson(scritta)

            Assert.AreEqual(StatoOpportunita.Scartata, voce.Stato)
            Assert.IsFalse(voce.Esito.HasValue)

        End Sub

        <TestMethod>
        Public Sub AspettaSoloChiHaSpeditoEnonHaAncoraSaputoNiente()

            Dim adesso As New Date(2026, 8, 21, 12, 0, 0)

            ' Spedita stamattina: zero giorni, e la soglia si tocca quando si tocca davvero.
            Assert.AreEqual(0, Spedita("Acme", adesso.AddHours(-3)).GiorniDiAttesa(adesso).Value)
            Assert.AreEqual(16, Spedita("Acme", adesso.AddDays(-16)).GiorniDiAttesa(adesso).Value)

            ' Chi non ha ancora spedito non aspetta una risposta: aspetta sé stesso.
            Dim ferma As New VoceRegistro With {.Cartella = "acme", .Stato = StatoOpportunita.Generata}
            ferma.DateStati(StatoOpportunita.Generata) = adesso.AddDays(-90)
            Assert.IsFalse(ferma.GiorniDiAttesa(adesso).HasValue)

            ' Chi ha saputo com'è andata ha finito di aspettare, anche se la notizia è brutta.
            Dim rifiutata As VoceRegistro = Spedita("Bianchi", adesso.AddDays(-40))
            rifiutata.Stato = StatoOpportunita.Esito
            rifiutata.Esito = EsitoCandidatura.Rifiutata
            Assert.IsFalse(rifiutata.GiorniDiAttesa(adesso).HasValue)

            ' Spedita senza la data dell'invio — un file corretto a mano — non conta
            ' duemila anni: non conta affatto.
            Dim senzaData As New VoceRegistro With {.Cartella = "verdi", .Stato = StatoOpportunita.Inviata}
            Assert.IsFalse(senzaData.GiorniDiAttesa(adesso).HasValue)

        End Sub

        <TestMethod>
        Public Sub IlPromemoriaMetteDavantiLaPiuVecchia()

            Dim adesso As New Date(2026, 8, 21, 12, 0, 0)

            Dim registro As New Registro
            registro.Voci.Add(Spedita("Acme", adesso.AddDays(-16)))
            registro.Voci.Add(Spedita("Bianchi", adesso.AddDays(-40)))
            registro.Voci.Add(Spedita("Verdi", adesso.AddDays(-3)))

            Dim ferme As List(Of VoceRegistro) = registro.DaSollecitare(14, adesso)

            Assert.HasCount(2, ferme, "quella di tre giorni fa non è ancora ferma")
            Assert.AreEqual("Bianchi", ferme(0).Azienda, "la più vecchia per prima")
            Assert.AreEqual("Acme", ferme(1).Azienda)

            ' Il giorno esatto della soglia entra: quattordici giorni di silenzio sono
            ' quattordici giorni di silenzio.
            Assert.HasCount(3, registro.DaSollecitare(3, adesso))

        End Sub

        <TestMethod>
        Public Sub ZeroGiorniSpegneIlPromemoria()

            Dim adesso As New Date(2026, 8, 21, 12, 0, 0)

            Dim registro As New Registro
            registro.Voci.Add(Spedita("Acme", adesso.AddDays(-400)))

            Assert.IsEmpty(registro.DaSollecitare(0, adesso), "zero vuol dire «non ricordarmelo»")
            Assert.IsEmpty(registro.DaSollecitare(-7, adesso))
            Assert.HasCount(1, registro.DaSollecitare(1, adesso))

        End Sub

    End Class

End Namespace
