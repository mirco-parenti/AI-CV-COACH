Imports System.IO
Imports System.Linq
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Collaudi di backup e ripristino, la funzione F7 (cap. 11.4). Qui non si verifica
    ''' che il programma sappia scrivere un file: si verifica che quel file <b>riporti
    ''' indietro</b> quello che c'era — e che nel farlo non porti via ciò che non deve
    ''' (la chiave API), non cancelli ciò che non nomina, e non scriva fuori casa se il
    ''' file lo hanno costruito male.
    ''' </summary>
    <TestClass>
    Public Class CollaudiBackup

        ' ==================================================================
        ' Esportare
        ' ==================================================================

        <TestMethod>
        Public Sub EsportaERipristinaRimetteTuttoAlSuoPosto()

            ' Il giro completo, che è la promessa intera del cap. 11.4: si esporta, si
            ' perde tutto, si ripristina, e i file tornano identici carattere per
            ' carattere. Il confronto è sul testo e non sugli oggetti apposta: un backup
            ' che «riapre uguale» ma riscrive diverso è un backup che ha già cominciato a
            ' perdere qualcosa.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)

                    Dim primaProfilo As String = File.ReadAllText(cartella.FileProfilo)
                    Dim primaCvBase As String = File.ReadAllText(cartella.FileCvBase)
                    Dim primaVersioni As Integer = profilo.Versioni().Count
                    Dim primaCartelle As List(Of String) = opportunita.Elenco().
                        Select(Function(c) Path.GetFileName(c)).OrderBy(Function(n) n).ToList()
                    Dim primaAnnuncio As String = File.ReadAllText(
                        Path.Combine(opportunita.Elenco()(0), ArchivioOpportunita.FileAnnuncio))

                    Dim fileBackup As String = Path.Combine(cartella.Radice, "prova.json")
                    archivio.Scrivi(archivio.Componi(ContenutoBackup.Tutto), fileBackup)

                    ' La perdita: come una cartella dati che sparisce con il disco.
                    Directory.Delete(cartella.CartellaProfilo, recursive:=True)
                    Directory.Delete(cartella.CartellaOpportunita, recursive:=True)

                    Dim esito As EsitoRipristino = archivio.Ripristina(ArchivioBackup.Leggi(fileBackup))

                    Assert.IsTrue(esito.ProfiloRipristinato, "il profilo deve essere tornato")
                    Assert.AreEqual(primaProfilo, File.ReadAllText(cartella.FileProfilo),
                                    "e deve essere identico a com'era")
                    Assert.AreEqual(primaCvBase, File.ReadAllText(cartella.FileCvBase),
                                    "il 📄 CV-1 base viaggia col profilo")
                    Assert.HasCount(primaVersioni, profilo.Versioni(),
                                    "lo storico torna con tutte le sue versioni")
                    Assert.AreEqual(primaCartelle.Count, esito.CandidatureRipristinate,
                                    "e tutte le candidature")
                    CollectionAssert.AreEqual(primaCartelle,
                                              opportunita.Elenco().Select(Function(c) Path.GetFileName(c)).
                                                  OrderBy(Function(n) n).ToList(),
                                              "con le stesse cartelle di prima")
                    Assert.AreEqual(primaAnnuncio,
                                    File.ReadAllText(Path.Combine(opportunita.Elenco()(0),
                                                                  ArchivioOpportunita.FileAnnuncio)),
                                    "e gli stessi artefatti dentro")
                    Assert.IsEmpty(esito.Rifiutati, "niente da rifiutare, in un backup fatto da noi")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlBackupDelSoloProfiloLasciaFuoriLeCandidature()

            ' Le due scelte del cap. 11.4 sono diverse davvero: «solo profilo» è chi
            ' sono, non dove mi sono candidato.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)

                    Dim fatto As Backup = archivio.Componi(ContenutoBackup.SoloProfilo)

                    Assert.IsNotNull(fatto.Profilo, "il profilo c'è")
                    Assert.IsGreaterThan(0, fatto.Storico.Count, "con il suo storico")
                    Assert.IsNotNull(fatto.CvBase, "e il CV base, che appartiene al profilo")
                    Assert.IsEmpty(fatto.Opportunita, "le candidature no")
                    Assert.IsNull(fatto.Registro, "e nemmeno il loro indice")

                    CollectionAssert.AreEqual({"profilo", "storico", "cv_base"}.ToList(),
                                              fatto.Contenuto().ToList(),
                                              "e l'intestazione lo dichiara")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub LaChiaveApiNonEntraNelBackup()

            ' Cap. 11.3: un backup gira via email o chiavetta, e non deve poter portare
            ' con sé una credenziale. Dopo un ripristino la chiave si rimette a mano, ed
            ' è il prezzo giusto.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)
                    File.WriteAllText(cartella.FileSegreti, "sk-ant-CHIAVE-SEGRETISSIMA")

                    Dim fileBackup As String = Path.Combine(cartella.Radice, "prova.json")
                    archivio.Scrivi(archivio.Componi(ContenutoBackup.Tutto), fileBackup)

                    Assert.DoesNotContain("CHIAVE-SEGRETISSIMA", File.ReadAllText(fileBackup),
                                          "la chiave API non viaggia nel backup")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IDocumentiImpaginatiRestanoFuoriDalBackup()

            ' I DOCX e i PDF della cartella «out» sono file normali: si copiano da sé, e
            ' si rigenerano con un bottone. Infilarli qui dentro gonfierebbe il backup di
            ' roba che non è dato (cap. 11.4).
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)

                    Dim dove As String = Path.Combine(opportunita.Elenco()(0),
                                                      ArchivioOpportunita.NomeCartellaOut)
                    Directory.CreateDirectory(dove)
                    File.WriteAllText(Path.Combine(dove, "cv.json"), "{""documento"": ""IMPAGINATO""}")

                    Dim fatto As Backup = archivio.Componi(ContenutoBackup.Tutto)

                    Assert.DoesNotContain("IMPAGINATO", fatto.ComeJson().ToJsonString(),
                                          "quel che sta in «out» non entra nel backup")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnArtefattoRovinatoNonFermaIlBackup()

            ' L'utente è padrone dei suoi file: uno rovinato a mano non deve impedire di
            ' salvare tutto il resto. Quel che non si legge non entra, e resta dov'è.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)
                    File.WriteAllText(Path.Combine(opportunita.Elenco()(0),
                                                   ArchivioOpportunita.FileGiudizi), "{ questo non è JSON")

                    Dim fatto As Backup = archivio.Componi(ContenutoBackup.Tutto)

                    Assert.HasCount(2, fatto.Opportunita, "le candidature ci sono lo stesso")
                    Assert.IsFalse(fatto.Opportunita(0).File.ContainsKey(ArchivioOpportunita.FileGiudizi),
                                   "meno il file che non si lascia leggere")
                    Assert.IsTrue(fatto.Opportunita(0).File.ContainsKey(ArchivioOpportunita.FileAnnuncio),
                                  "gli altri artefatti della stessa cartella entrano")

                End Sub)

        End Sub

        ' ==================================================================
        ' Rileggere: l'intestazione prima del corpo
        ' ==================================================================

        <TestMethod>
        Public Sub UnBackupDiFormatoPiuNuovoSiRifiuta()

            ' Il campo «formato_backup» serve a questo: un file scritto da una versione
            ' futura si rifiuta intero, invece di essere letto a metà.
            Dim errore As InvalidDataException = Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() Backup.DaTesto("{""formato_backup"": 99, ""app"": ""TrovaLavoro""}"))

            Assert.Contains("99", errore.Message, "dice che formato ha trovato")
            Assert.Contains("Aggiorna", errore.Message, "e cosa deve fare l'utente")

        End Sub

        <TestMethod>
        Public Sub UnFileCheNonEUnBackupSiRifiuta()

            ' Il caso vero: nel dialogo si sceglie per sbaglio un altro JSON — un
            ' annuncio, un CV, il registro. Si deve fermare qui, con il nome del file
            ' ancora in mano all'utente.
            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() Backup.DaTesto("{""titolo"": ""Tecnico manutenzione""}"))

            Assert.ThrowsExactly(Of InvalidDataException)(
                Sub() Backup.DaTesto("[1, 2, 3]"))

        End Sub

        ' ==================================================================
        ' Ripristinare
        ' ==================================================================

        <TestMethod>
        Public Sub IlRipristinoMetteInSalvoIlProfiloDiPrima()

            ' Cap. 11.4, passo 3: il ripristino non deve mai poter distruggere l'unico
            ' profilo buono. Quello di prima finisce nello storico *prima* che si scriva
            ' il nuovo, e da lì si riprende.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)
                    Dim fileBackup As String = Path.Combine(cartella.Radice, "prova.json")
                    archivio.Scrivi(archivio.Componi(ContenutoBackup.SoloProfilo), fileBackup)

                    ' Il profilo cambia dopo il backup: è la situazione in cui ripristinare
                    ' fa perdere qualcosa, se nessuno mette in salvo quel che c'era.
                    Dim cambiato As TrovaLavoro.Dati.Profilo = profilo.Carica()
                    cambiato.Nome = "Nome cambiato dopo il backup"
                    profilo.Salva(cambiato)

                    Dim esito As EsitoRipristino = archivio.Ripristina(ArchivioBackup.Leggi(fileBackup))

                    Assert.IsNotNull(esito.ProfiloMessoInSalvo, "la versione messa in salvo si dichiara")

                    Dim messoInSalvo As TrovaLavoro.Dati.Profilo =
                        profilo.CaricaVersione(esito.ProfiloMessoInSalvo)

                    Assert.AreEqual("Nome cambiato dopo il backup", messoInSalvo.Nome,
                                    "nello storico c'è il profilo di un attimo fa")
                    Assert.AreNotEqual("Nome cambiato dopo il backup", profilo.Carica().Nome,
                                       "e al suo posto c'è quello del backup")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlRipristinoNonScriveFuoriDallaCartellaDati()

            ' Un file di backup si può scrivere a mano. Un nome di cartella che è un
            ' percorso travestito non deve poter far scrivere l'applicazione dove vuole
            ' chi ha costruito il file: si rifiuta, e lo si dice in chiaro.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)

                    Dim costruito As New Backup With {.Data = Date.Now}
                    Dim cattiva As New OpportunitaInBackup With {.Cartella = "..\..\fuori"}
                    cattiva.File("annuncio.json") = JsonNode.Parse("{""titolo"": ""fuori casa""}")
                    costruito.Opportunita.Add(cattiva)

                    Dim conNomeStrano As New OpportunitaInBackup With {.Cartella = "2026-08-21_prova"}
                    conNomeStrano.File("..\segreti.bin") = JsonNode.Parse("{""x"": 1}")
                    conNomeStrano.File("appunti.txt") = JsonNode.Parse("{""x"": 1}")
                    conNomeStrano.File("annuncio.json") = JsonNode.Parse("{""titolo"": ""questo va bene""}")
                    costruito.Opportunita.Add(conNomeStrano)

                    Dim esito As EsitoRipristino = archivio.Ripristina(costruito)

                    Assert.Contains("..\..\fuori", esito.Rifiutati, "la cartella travestita si rifiuta")
                    Assert.IsTrue(esito.Rifiutati.Any(Function(r) r.EndsWith("..\segreti.bin")),
                                  "e il file travestito pure")
                    Assert.IsTrue(esito.Rifiutati.Any(Function(r) r.EndsWith("appunti.txt")),
                                  "come tutto ciò che non è un artefatto JSON")

                    Assert.IsFalse(Directory.Exists(Path.Combine(cartella.Radice, "..", "..", "fuori")),
                                   "fuori dalla cartella dati non è stato scritto niente")
                    Assert.IsTrue(File.Exists(Path.Combine(cartella.CartellaOpportunita,
                                                           "2026-08-21_prova", "annuncio.json")),
                                  "mentre quel che ha un nome buono viene scritto")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlRipristinoNonCancellaQuelloCheIlBackupNonNomina()

            ' Ripristinare non è «riporta il disco a quel giorno»: è «rimetti al loro
            ' posto queste cose». Le candidature nate dopo il backup restano dove sono —
            ' cancellare è un altro gesto, e costa una parola scritta a mano (cap. 11.5).
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)
                    Dim fileBackup As String = Path.Combine(cartella.Radice, "prova.json")
                    archivio.Scrivi(archivio.Componi(ContenutoBackup.Tutto), fileBackup)

                    ' Una candidatura nuova, che nel backup non c'è.
                    Dim dopo As Opportunita = OpportunitaDiProva("Verdi S.r.l.", New Date(2026, 8, 20, 11, 0, 0))
                    Dim suaCartella As String = Path.GetFileName(opportunita.Salva(dopo))

                    archivio.Ripristina(ArchivioBackup.Leggi(fileBackup))

                    Assert.HasCount(3, opportunita.Elenco(), "le candidature sono ancora tre")
                    Assert.IsTrue(opportunita.Elenco().Any(Function(c) Path.GetFileName(c) = suaCartella),
                                  "compresa quella che il backup non nominava")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlRegistroSiRicostruisceDalleCartelle()

            ' Cap. 07.3: le cartelle sono la fonte di verità, l'indice è rigenerabile. Dopo
            ' un ripristino sul disco c'è un insieme che nessun registro salvato conosce —
            ' le candidature tornate più quelle che c'erano già — e l'unica cosa giusta è
            ' rifarlo da lì.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)
                    Dim fileBackup As String = Path.Combine(cartella.Radice, "prova.json")
                    archivio.Scrivi(archivio.Componi(ContenutoBackup.Tutto), fileBackup)

                    opportunita.Salva(OpportunitaDiProva("Verdi S.r.l.", New Date(2026, 8, 20, 11, 0, 0)))
                    Directory.Delete(opportunita.Elenco()(0), recursive:=True)

                    archivio.Ripristina(ArchivioBackup.Leggi(fileBackup))

                    Dim registro As New ArchivioRegistro(cartella, opportunita)
                    Dim letto As Registro = registro.Carica()

                    Assert.IsFalse(letto.Rigenerato,
                                   "l'indice scritto dal ripristino combacia già: non c'è niente da rigenerare")
                    Assert.HasCount(3, letto.Voci, "e conta tutte le cartelle che ci sono adesso")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaVersioneDelloStoricoNonSiSovrascrive()

            ' Il nome di una versione è la sua data al secondo: due file omonimi sono lo
            ' stesso istante. Fra la copia venuta da fuori e l'originale di casa, vince
            ' quello di casa.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    profilo.Salva(ProfiloDiProva("Luca Ferrari"))
                    Dim versione As String = profilo.Versioni()(0)
                    Dim comEra As String = File.ReadAllText(
                        Path.Combine(cartella.CartellaStorico, versione & ".json"))

                    Assert.IsFalse(profilo.RiportaNelloStorico(versione, "{""nome"": ""venuto da fuori""}"),
                                   "una versione che c'è già non si riscrive")
                    Assert.AreEqual(comEra, File.ReadAllText(
                                    Path.Combine(cartella.CartellaStorico, versione & ".json")),
                                    "e il file resta quello di prima")

                End Sub)

        End Sub

        ' ==================================================================
        ' L'anteprima: cosa contiene, cosa sovrascrive
        ' ==================================================================

        <TestMethod>
        Public Sub LAnteprimaDiceCosaSovrascriveECosaResta()

            ' È il passo 2 del cap. 11.4, e la parte che l'utente legge prima di
            ' confermare. Non «sei sicuro?», ma cosa esattamente prende il posto di cosa.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    RiempiLaCartella(profilo, opportunita)
                    Dim fatto As Backup = archivio.Componi(ContenutoBackup.Tutto)

                    ' Dopo il backup: una candidatura nuova, che il file non nomina.
                    opportunita.Salva(OpportunitaDiProva("Verdi S.r.l.", New Date(2026, 8, 20, 11, 0, 0)))

                    Dim detto As AnteprimaRipristino = archivio.Anteprima(fatto)

                    Assert.HasCount(2, detto.CandidatureRiscritte, "due candidature si riscrivono")
                    Assert.AreEqual(1, detto.CandidatureCheRestano, "e una resta dov'è")

                    Dim contiene As String = String.Join(" | ", detto.CosaContiene())
                    Assert.Contains("Il profilo", contiene, "dice che c'è il profilo")
                    Assert.Contains("2 candidature", contiene, "e quante candidature")

                    Dim sovrascrive As String = String.Join(" | ", detto.CosaSovrascrive())
                    Assert.Contains("storico", sovrascrive,
                                    "promette che il profilo di adesso finisce nello storico")
                    Assert.Contains("non cancella quello che il backup non nomina", sovrascrive,
                                    "e che quel che il backup non nomina non si tocca")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub LAnteprimaDiceIlGiornoDelProfiloNonQuelloDelloStorico()

            ' Difetto trovato dal vivo il 2026-08-21: la data del profilo nel backup si
            ' deduceva dall'ultima versione dello storico, che è la stessa cosa quasi
            ' sempre — ma davanti a uno storico più vecchio del profilo corrente
            ' l'anteprima annunciava «il profilo, come era il 17/08» di uno salvato oggi.
            ' Adesso la data viaggia nel file, scritta insieme al profilo.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    Dim oggi As String = profilo.Salva(ProfiloDiProva("Luca Ferrari"))

                    ' Lo storico dev'essere più vecchio del profilo corrente, o il caso non
                    ' è quello vero: è la cartella dati in cui l'ultima versione è di
                    ' giorni fa mentre il profilo su disco è stato riscritto oggi.
                    File.Delete(Path.Combine(cartella.CartellaStorico, oggi & ".json"))
                    profilo.RiportaNelloStorico("2026-08-17_125003", "{""nome"": ""Luca di allora""}")

                    Dim detto As AnteprimaRipristino = archivio.Anteprima(archivio.Componi(ContenutoBackup.SoloProfilo))

                    Assert.IsTrue(detto.DataDelProfilo.HasValue, "la data del profilo si sa")
                    Assert.AreEqual(Date.Today, detto.DataDelProfilo.Value.Date,
                                    "ed è quella del profilo, non quella della versione più vecchia")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnBackupSenzaLaDataDelProfiloSiLeggeLoStesso()

            ' Il campo è nato dopo il formato 1 e non lo cambia: un file scritto prima si
            ' legge come sempre, e dice solo una cosa in meno — la data torna a dedursi
            ' dallo storico.
            Dim letto As Backup = Backup.DaTesto(
                "{""formato_backup"": 1, ""app"": ""TrovaLavoro"", ""data"": ""2026-08-20T10:00:00""," &
                """profilo"": {""nome"": ""Luca Ferrari""}," &
                """storico"": [{""versione"": ""2026-08-17_125003"", ""profilo"": {""nome"": ""Luca Ferrari""}}]}")

            Assert.IsNotNull(letto.Profilo, "il profilo si legge")
            Assert.IsFalse(letto.ProfiloSalvato.HasValue, "la data non c'è, e non è un errore")

            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    Assert.AreEqual(New Date(2026, 8, 17), archivio.Anteprima(letto).DataDelProfilo.Value.Date,
                                    "senza il campo si ricade sull'ultima versione dello storico")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub LAnteprimaDiUnBackupSuUnaCartellaVuota()

            ' Il caso del PC nuovo: non c'è niente da sovrascrivere, e dirlo è diverso dal
            ' tacere.
            ConCartellaTemporanea(
                Sub(cartella, archivio, profilo, opportunita)

                    Dim altrove As String = Path.Combine(Path.GetTempPath(),
                                                         "backup-origine-" & Guid.NewGuid().ToString("N"))
                    Try
                        Dim daltraParte As New CartellaDati(altrove)
                        Dim profiloLa As New ArchivioProfilo(daltraParte)
                        Dim opportunitaLa As New ArchivioOpportunita(daltraParte)
                        RiempiLaCartella(profiloLa, opportunitaLa)

                        Dim fatto As Backup = New ArchivioBackup(
                            daltraParte, profiloLa, opportunitaLa,
                            New ArchivioRegistro(daltraParte, opportunitaLa)).Componi(ContenutoBackup.Tutto)

                        Dim sovrascrive As String = String.Join(" | ", archivio.Anteprima(fatto).CosaSovrascrive())

                        Assert.Contains("un profilo non c'è", sovrascrive,
                                        "su una cartella vuota non si sostituisce niente")

                    Finally
                        CartelleDiProva.PortaVia(altrove)
                    End Try

                End Sub)

        End Sub

        ' ==================================================================
        ' Il banco
        ' ==================================================================

        ''' <summary>
        ''' Una cartella dati usa-e-getta con tutti gli archivi montati sopra: quella
        ''' vera dell'utente non si tocca mai.
        ''' </summary>
        Private Shared Sub ConCartellaTemporanea(
            prova As Action(Of CartellaDati, ArchivioBackup, ArchivioProfilo, ArchivioOpportunita))

            Dim radice As String = Path.Combine(Path.GetTempPath(), "backup-" & Guid.NewGuid().ToString("N"))
            Dim cartella As New CartellaDati(radice)
            Dim profilo As New ArchivioProfilo(cartella)
            Dim opportunita As New ArchivioOpportunita(cartella)
            Dim registro As New ArchivioRegistro(cartella, opportunita)

            Try
                prova(cartella, New ArchivioBackup(cartella, profilo, opportunita, registro), profilo, opportunita)
            Finally
                CartelleDiProva.PortaVia(radice)
            End Try

        End Sub

        ''' <summary>Un profilo salvato con storico e CV base, e due candidature.</summary>
        Private Shared Sub RiempiLaCartella(profilo As ArchivioProfilo, opportunita As ArchivioOpportunita)

            Dim versione As String = profilo.Salva(ProfiloDiProva("Luca Ferrari"))

            ' Una seconda versione: lo storico serve a raccontare i cambiamenti, e con una
            ' sola versione non si vedrebbe se il ripristino le riporta tutte.
            Dim cresciuto As TrovaLavoro.Dati.Profilo = ProfiloDiProva("Luca Ferrari")
            cresciuto.Competenze.Add("saldatura TIG")
            versione = profilo.Salva(cresciuto)

            profilo.SalvaCvBase(JsonNode.Parse("{""intestazione"": {""nome"": ""Luca Ferrari""}}"), versione)

            opportunita.Salva(OpportunitaDiProva("Rossi S.p.A.", New Date(2026, 8, 10, 9, 30, 0)))
            opportunita.Salva(OpportunitaDiProva("Bianchi S.n.c.", New Date(2026, 8, 12, 15, 0, 0)))

        End Sub

        Private Shared Function ProfiloDiProva(nome As String) As TrovaLavoro.Dati.Profilo

            Dim fatto As TrovaLavoro.Dati.Profilo = TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo())
            fatto.Nome = nome

            Return fatto

        End Function

        Private Shared Function OpportunitaDiProva(azienda As String, quando As Date) As Opportunita

            Return New Opportunita With {
                .Creata = quando,
                .VersioneProfilo = "2026-08-10_092500",
                .Annuncio = JsonNode.Parse($"{{""titolo"": ""Tecnico manutenzione"", ""azienda"": ""{azienda}""}}"),
                .Confronto = JsonNode.Parse(
                    "{""giudizi"": [{""requisito"": ""Patente B"", ""esito"": ""soddisfatto""}]," &
                    """numero_complessivo"": 82}"),
                .Match = New RisultatoMatch With {
                    .MatchFinale = 82, .Stelle = 4.1, .ScoreBase = 80, .NumeroLlm = 82,
                    .ScartoTagliato = False, .GateEliminatorio = False, .Nota = Nothing}}

        End Function

    End Class

End Namespace
