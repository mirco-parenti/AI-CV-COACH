Imports System.Drawing
Imports System.IO
Imports System.IO.Compression
Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi del pannello P6 (cap. 03.6; cap. 12, A7). Girano <b>senza rete e senza
    ''' stampante</b>: i mestieri sono finti e l'archivio dei documenti nasce senza
    ''' <c>StampantePdf</c>, che vorrebbe una WebView e il thread dell'interfaccia. Il PDF
    ''' ha il suo banco a parte (<c>CollaudiStampaPdf</c>); qui si guarda il pannello.
    ''' </summary>
    ''' <remarks>
    ''' Le domande sono tre: che le anteprime mostrino quello che i file conterranno, che
    ''' rientrare non faccia rigenerare niente, e che i due bottoni d'esportazione
    ''' scrivano ognuno il proprio formato — nella cartella giusta, che per il 📄 CV base
    ''' non è quella di una candidatura.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiPannelloDocumenti

        Private Const CvMirato As String =
            "{""tipo"": ""cv_mirato"", ""intestazione"": {""nome"": ""Luca Ferrari"", ""citta"": ""Modena""}," &
            """sommario"": ""Quattro anni di magazzino."", ""competenze"": [""Uso del muletto""]}"

        Private Const CvBase As String =
            "{""tipo"": ""cv_base"", ""intestazione"": {""nome"": ""Luca Ferrari"", ""citta"": ""Modena""}," &
            """sommario"": ""Il ritratto del profilo."", ""competenze"": [""Uso del muletto""]}"

        Private Const Lettera As String =
            "{""tipo"": ""lettera_mirata"", ""apertura"": ""Spettabile Azienda,""," &
            """corpo"": ""Ho quattro anni di magazzino."", ""chiusura"": ""Cordiali saluti,""," &
            """firma"": {""nome"": ""Luca Ferrari""}}"

        Private Const AnnuncioLetto As String =
            "{""titolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A."", ""sede"": [""Forlì""]}"

        ' ==================================================================
        ' I documenti di una candidatura
        ' ==================================================================

        <TestMethod>
        Public Async Function LaCandidaturaSiGeneraESiLeggeNelleTreColonne() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraLaCandidaturaAsync(Confrontata(contesto))

                    Assert.AreEqual("cv_mirato → lettera", generatore.LavoriChiesti(),
                                    "il CV prima della lettera, che deve raccontare la stessa storia")

                    Assert.Contains("Magazziniere", Casella(pannello, "txtAnnuncio").Text, "l'annuncio da cui nascono")
                    Assert.Contains("Quattro anni di magazzino.", Casella(pannello, "txtCv").Text, "il CV")
                    Assert.Contains("Spettabile Azienda,", Casella(pannello, "txtLettera").Text, "e la lettera")
                    Assert.AreEqual("🎯 CV mirato", Etichetta(pannello, "lblCv").Text, "col nome giusto in cima")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IDocumentiGeneratiRestanoNellaCartellaDellaCandidatura() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsTrue(File.Exists(Path.Combine(candidatura.Cartella, ArchivioOpportunita.FileCv)),
                                  "il CV è salvato con la candidatura")
                    Assert.IsTrue(File.Exists(Path.Combine(candidatura.Cartella, ArchivioOpportunita.FileLettera)),
                                  "e la lettera pure")
                End Function)

        End Function

        <TestMethod>
        Public Async Function RientrareNonRigeneraNiente() As Task

            ' Rifarli costa un'attesa e dei token, e cambierebbe sotto il naso di chi li
            ' aveva già letti: a rifarli c'è «Rigenera», che lo dichiara.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("cv_mirato → lettera", generatore.LavoriChiesti(),
                                    "due chiamate in tutto, non quattro")
                    Assert.Contains("puoi rigenerarli", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "e il pannello dice come si rifanno")
                End Function)

        End Function

        <TestMethod>
        Public Async Function RigenerareRiscriveDaCapo() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera).
                       Dara("{""tipo"": ""cv_mirato"", ""intestazione"": {""nome"": ""Luca Ferrari""}," &
                            """sommario"": ""Riscritto da capo.""}").Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraLaCandidaturaAsync(Confrontata(contesto))
                    Await pannello.RigeneraAsync()

                    Assert.AreEqual("cv_mirato → lettera → cv_mirato → lettera", generatore.LavoriChiesti(),
                                    "il giro si rifà tutto")
                    Assert.Contains("Riscritto da capo.", Casella(pannello, "txtCv").Text, "e a video c'è il nuovo")
                End Function)

        End Function

        ' ==================================================================
        ' Il 📄 CV base
        ' ==================================================================

        <TestMethod>
        Public Async Function IlCvBaseSiGeneraSenzaAnnuncioELoDice() As Task

            ' Cap. 03.6: per il CV base la colonna dell'annuncio resta senza annuncio — e
            ' lo spiega, invece di sembrare un riquadro che non ha caricato.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()

                    Assert.AreEqual("cv_base", generatore.LavoriChiesti(), "un prompt solo, dal profilo")
                    Assert.AreEqual("📄 CV base", Etichetta(pannello, "lblCv").Text, "il nome in cima cambia")
                    Assert.Contains("Il ritratto del profilo.", Casella(pannello, "txtCv").Text, "il CV c'è")
                    Assert.Contains("non nasce da un annuncio", Casella(pannello, "txtAnnuncio").Text,
                                    "e la colonna vuota dice perché è vuota")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlCvBaseSiSalvaColProfiloEAnnotaLaVersione() As Task

            ' Il CV base è del profilo, non di una candidatura (cap. 11.1): sta là, e
            ' porta scritto da quale versione è nato.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()

                    Dim salvato As TrovaLavoro.Dati.CvBase = contesto.Archivio.CaricaCvBase()
                    Assert.IsNotNull(salvato, "il cv_base.json c'è")
                    Assert.AreEqual(contesto.Archivio.Versioni().Last(), salvato.VersioneProfilo,
                                    "con la versione di profilo da cui è nato")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlCvBaseSiEsportaAccantoAlProfilo() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()
                    Await pannello.EsportaAsync(FormatiDocumento.Docx)

                    Dim scritti As String() = Directory.GetFiles(contesto.Cartella.CartellaOutProfilo, "*.docx")
                    Assert.HasCount(1, scritti, "il DOCX è accanto al profilo, non in una cartella-opportunità")

                    ' Il nome del CV base dice di chi è e di quando è, e non nomina
                    ' nessuna azienda — perché un'azienda non c'è (cap. 05.6).
                    Dim nome As String = Path.GetFileName(scritti(0))
                    Assert.StartsWith("CV_", nome, "si riconosce che è un CV")
                    Assert.Contains("Luca", nome, "e di chi è")
                    Assert.DoesNotContain("Rossi", nome, "nessuna azienda: questo CV non nasce da un annuncio")
                End Function)

        End Function

        <TestMethod>
        Public Async Function RientrareSulCvBaseNonLoRigenera() As Task

            ' T7d: l'altra metà di RientrareNonRigeneraNiente. Fino a T7c il 📄 CV base era
            ' l'unico documento a rinascere a ogni visita — un'altra attesa, altri token, e
            ' un testo diverso da quello che l'utente aveva già letto ed esportato.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()
                    Await pannello.MostraIlCvBaseAsync()

                    Assert.AreEqual("cv_base", generatore.LavoriChiesti(), "una chiamata sola, non due")
                    Assert.Contains("Il ritratto del profilo.", Casella(pannello, "txtCv").Text,
                                    "e a video c'è sempre il suo CV")
                    Assert.Contains("l'ho scritto", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "col pannello che dice di quando è")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaAiIlCvBaseGiaScrittoSiRileggeESiEsporta() As Task

            ' Il difetto che ha fatto nascere T7d: il cv_base.json stava su disco e i due
            ' bottoni d'esportazione erano spenti, perché l'unica strada per rivederlo
            ' passava dall'AI. Un documento già scritto si riesporta anche senza rete.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    GiaScritto(contesto, "it")

                    Await pannello.MostraIlCvBaseAsync()

                    Assert.Contains("Il ritratto del profilo.", Casella(pannello, "txtCv").Text,
                                    "il CV di ieri è a video senza aver chiamato nessuno")
                    Assert.IsTrue(Bottone(pannello, "btnEsportaDocx").Enabled, "e si può esportare")
                    Assert.IsTrue(Bottone(pannello, "btnEsportaPdf").Enabled, "in tutti e due i formati")
                    Assert.DoesNotContain("chiave API", Etichetta(pannello, "lblStatoDocumenti").Text,
                                          "senza lamentarsi di una chiave che qui non serve")

                    Await pannello.EsportaAsync(FormatiDocumento.Docx)

                    Assert.HasCount(1, Directory.GetFiles(contesto.Cartella.CartellaOutProfilo, "*.docx"),
                                    "e il file esce davvero")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnCvBaseIlleggibileSiDichiaraInveceDiRigenerarsi() As Task

            ' La stessa promessa del profilo (cap. 11.1), dall'altra parte: un file che non
            ' si lascia leggere si dichiara. Rigenerare qui sarebbe il modo più elegante di
            ' nascondere la notizia — a video comparirebbe un CV nuovo di zecca, e che su
            ' disco ce ne fosse un altro, danneggiato, non lo saprebbe più nessuno.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    File.WriteAllText(contesto.Cartella.FileCvBase, "{ questo non è JSON")

                    Await pannello.MostraIlCvBaseAsync()

                    Assert.AreEqual("", generatore.LavoriChiesti(), "l'AI non è stata chiamata")

                    Dim stato As Label = Etichetta(pannello, "lblStatoDocumenti")
                    Assert.Contains("non si lascia leggere", stato.Text, "il pannello lo dice")
                    Assert.AreEqual(StileApp.Pericolo, stato.ForeColor,
                                    "col colore di chi non può funzionare")

                    Assert.IsTrue(Bottone(pannello, "btnRigenera").Enabled,
                                  "e da qui si può riprovare, che è l'unica via d'uscita")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnCvBaseDiUnProfiloVecchioLoDiceInveceDiRifarsi() As Task

            ' La promessa scritta sopra Dati.CvBase: «poter dire che è di una versione
            ' precedente invece di rigenerarlo di soppiatto». Quel CV potrebbe essere
            ' quello che l'utente ha già spedito: la scelta di rifarlo è sua.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    contesto.Archivio.SalvaCvBase(JsonNode.Parse(CvBase), "2026-01-01_000000")

                    Await pannello.MostraIlCvBaseAsync()

                    Dim stato As String = Etichetta(pannello, "lblStatoDocumenti").Text
                    Assert.Contains("hai cambiato il profilo", stato, "il pannello lo dice")
                    Assert.Contains("Rigenera", stato, "e dice come si rimedia")
                    Assert.Contains("Il ritratto del profilo.", Casella(pannello, "txtCv").Text,
                                    "intanto il CV che c'è resta a video")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlCvBaseInIngleseEsceColNomeELeEtichetteDellaSuaLingua() As Task

            ' Cap. 10.4 e cap. 05.6: una lingua sola decide le etichette stampate e la
            ' sigla nel nome, o si otterrebbe un CV_..._EN_ con «Formazione» dentro.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    GiaScritto(contesto, "en")

                    Await pannello.MostraIlCvBaseAsync()
                    Await pannello.EsportaAsync(FormatiDocumento.Docx)

                    Dim nome As String = Path.GetFileName(
                        Directory.GetFiles(contesto.Cartella.CartellaOutProfilo, "*.docx").Single())

                    Assert.Contains("EN", nome, "la sigla della lingua è nel nome del file")
                End Function)

        End Function

        ' ==================================================================
        ' La rifinitura anti-slop e il suo prima/dopo (T7b, cap. 08.4)
        ' ==================================================================

        <TestMethod>
        Public Async Function IlCvBasePassaDallaRifinituraECiSiRicordaComEra() As Task

            ' Il 📄 CV base non passa dalla pipeline: nasce qui, e qui deve passare
            ' dall'anti-slop come gli altri documenti.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Dim rifinitore As RifinitoreFinto = New RifinitoreFinto().
                Dara("sommario", "Il ritratto del profilo, riscritto.")

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()

                    Assert.AreEqual("sommario", rifinitore.Passate.Single().Id(),
                                    "il sommario è partito, e i campi-fatto no")

                    Assert.Contains("Il ritratto del profilo, riscritto.",
                                    Casella(pannello, "txtCv").Text, "a video c'è il testo rifinito")

                    Dim salvato As TrovaLavoro.Dati.CvBase = contesto.Archivio.CaricaCvBase()
                    Assert.AreEqual("Il ritratto del profilo, riscritto.",
                                    salvato.Cv("sommario").GetValue(Of String)(),
                                    "e nel file c'è il testo rifinito, il solo che si conserva")
                End Function,
                rifinitore)

        End Function

        ''' <summary>
        ''' Quando l'anti-slop inciampa il CV resta grezzo — ed è buono — ma la cosa si
        ''' <b>dice</b>: la pipeline lo fa da T7b (cap. 08.6), qui fino a T9d si taceva.
        ''' </summary>
        <TestMethod>
        Public Async Function SeLaRifinituraInciampaIlCvBaseRestaGrezzoELoDice() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Dim rifinitore As New RifinitoreFinto With {
                .Fallira = New ErroreAi(CausaErroreAi.Servizio, "L'AI non risponde.")}

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()

                    Assert.Contains("Il ritratto del profilo.", Casella(pannello, "txtCv").Text,
                                    "il CV c'è lo stesso, col testo grezzo")

                    Assert.Contains("La rifinitura non è riuscita",
                                    Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "e chi ha l'anti-slop acceso non deve crederlo rifinito")
                End Function,
                rifinitore)

        End Function

        <TestMethod>
        Public Async Function IlBottoneDelRitornoDiceDoveRiporta() As Task

            ' A P6 si arriva da due strade — l'opportunità e la scheda del profilo — e il
            ' bottone deve nominare quella giusta: un'etichetta sbagliata manderebbe
            ' l'utente in un pannello dove non è mai stato (cap. 12.7, mai un vicolo cieco).
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase).Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()
                    Assert.AreEqual("◀ Torna al profilo", Bottone(pannello, "btnTornaIndietro").Text,
                                    "dal CV base si torna al profilo, che è dove è nato")

                    Await pannello.MostraLaCandidaturaAsync(Confrontata(contesto))
                    Assert.AreEqual("◀ Torna all'opportunità", Bottone(pannello, "btnTornaIndietro").Text,
                                    "e dalla candidatura si torna alla sua scheda")
                End Function)

        End Function

        ' ==================================================================
        ' Esportare
        ' ==================================================================

        <TestMethod>
        Public Async Function OgniBottoneEsportaIlSuoFormato() As Task

            ' Chi vuole solo il DOCX non deve ritrovarsi anche un PDF che non ha chiesto —
            ' e senza stampante il PDF non si può proprio fare, il che va detto e non
            ' fatto passare per una scelta.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Await pannello.EsportaAsync(FormatiDocumento.Docx)

                    Dim uscita As String = ArchivioOpportunita.CartellaOut(candidatura)
                    Assert.HasCount(2, Directory.GetFiles(uscita, "*.docx"), "il CV e la lettera in DOCX")
                    Assert.IsEmpty(Directory.GetFiles(uscita, "*.pdf"), "e nessun PDF non richiesto")

                    Assert.Contains(".docx", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "il pannello dice quali file ha scritto")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaDocumentiNonSiEsportaNiente() As Task

            Await ConPannelloAsync(
                New GeneratoreFinto,
                Async Function(pannello, contesto, documenti)
                    Assert.IsFalse(Bottone(pannello, "btnEsportaDocx").Enabled, "non c'è ancora niente da esportare")
                    Assert.IsFalse(Bottone(pannello, "btnEsportaPdf").Enabled, "in nessuno dei due formati")

                    ' Le cartelle dei dati esistono già (le crea il salvataggio del
                    ' profilo): quel che non deve comparire è un file.
                    Await pannello.EsportaAsync(FormatiDocumento.Docx)

                    Assert.IsEmpty(Directory.GetFiles(contesto.Cartella.CartellaOutProfilo),
                                   "e chiamarla lo stesso non scrive niente")
                End Function)

        End Function

        <TestMethod>
        Public Async Function EsportareDoveDiceLUtenteLasciaLaCopiaCheServeAllEmail() As Task

            ' T9d (2026-08-22): i file nascono nella cartella della candidatura e di lì li
            ' prende P7 per allegarli all'email. La cartella scelta dall'utente riceve una
            ' copia; portarli via invece di copiarli lascerebbe l'email a mani vuote.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim scelta As String = Path.Combine(contesto.Cartella.Radice, "scelta-dall-utente")

                    Dim finiti As IReadOnlyList(Of String) =
                        Await pannello.EsportaAsync(FormatiDocumento.Docx, scelta)

                    Assert.HasCount(2, Directory.GetFiles(scelta, "*.docx"),
                                    "il CV e la lettera stanno dove l'utente li ha chiesti")
                    Assert.HasCount(2, Directory.GetFiles(ArchivioOpportunita.CartellaOut(candidatura), "*.docx"),
                                    "e restano anche nella cartella della candidatura, dove P7 li cerca")
                    Assert.HasCount(2, finiti, "tornano i file su cui l'utente può mettere le mani")
                    Assert.Contains("scelta-dall-utente", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "e il pannello dice dove sono finiti")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlPermessoDiSostituireSiChiedeSoloQuandoServeENegarloNonToccaNiente() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraLaCandidaturaAsync(Confrontata(contesto))

                    Dim scelta As String = Path.Combine(contesto.Cartella.Radice, "scelta-dall-utente")
                    Dim chiesto As Boolean = False

                    ' Cartella vuota: non c'è niente da sostituire, e una domanda che non
                    ' ha oggetto è solo un clic in più.
                    Await pannello.EsportaAsync(FormatiDocumento.Docx, scelta,
                                                Function(nomi)
                                                    chiesto = True
                                                    Return True
                                                End Function)

                    Assert.IsFalse(chiesto, "la prima volta non si chiede niente a nessuno")

                    ' Adesso in quella cartella i file ci sono: al posto del primo metto
                    ' qualcosa di riconoscibile, che deve sopravvivere al rifiuto.
                    Dim primo As String = Directory.GetFiles(scelta, "*.docx").First()
                    File.WriteAllText(primo, "questo è il file di ieri")

                    Dim finiti As IReadOnlyList(Of String) =
                        Await pannello.EsportaAsync(FormatiDocumento.Docx, scelta,
                                                    Function(nomi)
                                                        chiesto = True
                                                        Return False
                                                    End Function)

                    Assert.IsTrue(chiesto, "la seconda volta sì: quei file verrebbero sostituiti")
                    Assert.AreEqual("questo è il file di ieri", File.ReadAllText(primo),
                                    "e chi dice di no se li ritrova come stavano")
                    Assert.IsEmpty(finiti, "non si torna con file che non sono stati scritti")
                    Assert.Contains("Non ho sostituito", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "il pannello lo dice, invece di far credere a un'esportazione riuscita")
                End Function)

        End Function

        <TestMethod>
        Public Async Function ColPermessoIFileVengonoSostituiti() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraLaCandidaturaAsync(Confrontata(contesto))

                    Dim scelta As String = Path.Combine(contesto.Cartella.Radice, "scelta-dall-utente")

                    Await pannello.EsportaAsync(FormatiDocumento.Docx, scelta)

                    Dim primo As String = Directory.GetFiles(scelta, "*.docx").First()
                    File.WriteAllText(primo, "questo è il file di ieri")

                    Await pannello.EsportaAsync(FormatiDocumento.Docx, scelta, Function(nomi) True)

                    ' Un DOCX è un archivio: quello vero pesa, la riga di prima no.
                    Assert.IsGreaterThan(1000, New FileInfo(primo).Length,
                                         "al posto della riga di ieri c'è di nuovo un documento")
                End Function)

        End Function

        <TestMethod>
        Public Async Function EsportareNellaCartellaDoveIFileNasconoNonLiCopiaSuSeStessi() As Task

            ' Copiare un file su se stesso non si può, e chi sceglie proprio la cartella
            ' della candidatura non sta sbagliando niente: i file sono già dove li vuole.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim uscita As String = ArchivioOpportunita.CartellaOut(candidatura)

                    Dim finiti As IReadOnlyList(Of String) =
                        Await pannello.EsportaAsync(FormatiDocumento.Docx, uscita)

                    Assert.HasCount(2, finiti, "l'esportazione riesce lo stesso")
                    Assert.HasCount(2, Directory.GetFiles(uscita, "*.docx"), "e i file sono quelli, non il doppio")
                    Assert.Contains(".docx", Etichetta(pannello, "lblStatoDocumenti").Text, "senza un errore da raccontare")
                End Function)

        End Function


        ' ==================================================================
        ' La tendina dei documenti e la voce della barra (T9d)
        ' ==================================================================

        <TestMethod>
        Public Async Function LaTendinaElencaSoloIDocumentiCheEsistono() As Task

            ' Elencare un documento non ancora scritto vorrebbe dire far partire una
            ' generazione a chi credeva di spostarsi fra due schermate.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase).Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)

                    ' Una candidatura salvata su disco ma senza documenti: in tendina non
                    ' deve comparire, perché aprirla non mostrerebbe niente.
                    Confrontata(contesto)

                    Await pannello.MostraIlCvBaseAsync()

                    Dim tendina As ComboBox = Scelta(pannello, "cmbDocumento")
                    Assert.HasCount(1, tendina.Items, "c'è solo il CV base, che è l'unico documento scritto")
                    Assert.Contains("CV base", tendina.Items(0).ToString(), "ed è lui")
                    Assert.AreEqual(0, tendina.SelectedIndex, "segnato come quello in mostra")

                End Function)

        End Function

        <TestMethod>
        Public Async Function DallaTendinaSiSaltaDaUnDocumentoAllAltro() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)

                    ' Prima il CV base (che si salva su disco), poi una candidatura coi
                    ' suoi documenti già scritti.
                    Await pannello.MostraIlCvBaseAsync()

                    Dim candidatura As Opportunita = Confrontata(contesto)
                    candidatura.Cv = JsonNode.Parse(CvMirato)
                    candidatura.Lettera = JsonNode.Parse(Lettera)
                    contesto.Opportunita.Salva(candidatura)

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim tendina As ComboBox = Scelta(pannello, "cmbDocumento")
                    Assert.HasCount(2, tendina.Items, "adesso i documenti sono due")

                    Dim rigaDelCvBase As Integer = -1
                    For i As Integer = 0 To tendina.Items.Count - 1
                        If tendina.Items(i).ToString().Contains("CV base") Then rigaDelCvBase = i
                    Next
                    Assert.IsTrue(rigaDelCvBase >= 0, "il CV base è in elenco")

                    ' Il salto: si sceglie la riga del CV base e il pannello ci va, senza
                    ' passare dalla Home e senza chiamare l'AI (il CV base è già scritto).
                    Dim chiamatePrima As Integer = generatore.LingueChieste.Count
                    Await pannello.ApriDallaTendinaAsync(rigaDelCvBase)

                    Assert.Contains("CV base", Etichetta(pannello, "lblCv").Text, "si sta guardando il CV base")
                    Assert.HasCount(chiamatePrima, generatore.LingueChieste, "e non è costato una generazione")

                End Function)

        End Function

        <TestMethod>
        Public Async Function EntrareDaiDocumentiNonChiamaMaiLAi() As Task

            ' La voce «📄 Documenti» della barra è navigazione: senza niente da mostrare il
            ' pannello resta vuoto e lo spiega, invece di mettersi a generare.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)

                    Await pannello.ApriQualcosaAsync()

                    Assert.IsEmpty(generatore.LingueChieste, "nessuna generazione partita da sola")
                    Assert.Contains("Non c'è ancora nessun documento",
                                    Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "e il pannello dice come farne uno")

                End Function)

        End Function

        ' ==================================================================
        ' Quando l'AI non c'è, e le tappe che verranno
        ' ==================================================================

        <TestMethod>
        Public Async Function SenzaChiaveNonSiGeneraELoDice() As Task

            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()

                    Dim stato As Label = Etichetta(pannello, "lblStatoDocumenti")
                    Assert.Contains("chiave API", stato.Text, "lo dice")
                    Assert.AreEqual(StileApp.Pericolo, stato.ForeColor, "col colore di chi non può funzionare")

                    ' Da T7d la colonna non resta muta: si è sulla strada del CV base, e
                    ' quella strada dice a che punto è invece di sembrare non caricata.
                    Assert.Contains("non è ancora stato scritto", Casella(pannello, "txtCv").Text,
                                    "e al posto del CV c'è scritto che un CV non c'è")
                End Function)

        End Function

        <TestMethod>
        Public Sub CioCheArriveraSiVedeSpentoEDiceQuando()

            ' Cap. 03.8: è il pannello che più di ogni altro mostra dove sta andando il
            ' progetto, e lo mostra spegnendo, non nascondendo.
            Using pannello As New PannelloDocumenti()

                ' Da T7d nemmeno la tendina è più «quel che arriverà»: è spenta perché su
                ' un pannello vuoto non c'è nessun documento a cui quella lingua appartenga.
                Assert.IsFalse(Scelta(pannello, "cmbLingua").Enabled,
                               "niente lingua, che senza documenti non ha padrone")
                ' La casella del prima/dopo stava qui fino a T9d: quel confronto si guarda
                ' adesso dentro «Modifica i testi», col bottone che va avanti e indietro.
                ' Al suo posto c'è la tendina dei documenti, spenta finché il pannello non
                ' è collegato a una cartella dati da cui elencarli.
                Assert.IsFalse(Scelta(pannello, "cmbDocumento").Enabled,
                               "e niente tendina dei documenti, che senza dati non ha niente da elencare")
                Assert.IsFalse(Bottone(pannello, "btnPreparaEmail").Enabled, "l'email arriva con T6")

                Assert.AreEqual(LivelloBottone.AzionePrincipale,
                                DirectCast(Bottone(pannello, "btnPreparaEmail").Tag, LivelloBottone),
                                "e resta l'avanti del flusso, anche da spento")
            End Using

        End Sub

        <TestMethod>
        Public Sub LaFasciaDelleAzioniLasciaIlPostoAlLogo()

            Using pannello As New PannelloDocumenti()
                pannello.ImpostaIngombroLogo(New Size(261, 188))

                Dim azioni As Panel = DirectCast(
                    pannello.Controls.Find("pnlAzioni", searchAllChildren:=True).Single(), Panel)

                Assert.AreEqual(188, azioni.Height, "alta quanto il logo sfonda nell'area centrale")
                Assert.AreEqual(273, azioni.Padding.Left, "e i bottoni cominciano dopo la sua larghezza")
                Assert.IsGreaterThanOrEqualTo(273, Bottone(pannello, "btnTornaIndietro").Left,
                                              "nessun bottone sotto il logo")
            End Using

        End Sub

        <TestMethod>
        Public Async Function EliminatoIlProfiloIlCvBaseSparisceDallaVista() As Task

            ' Cap. 11.5: il 📄 CV base è il ritratto del profilo, e con lui se ne va.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()
                    Assert.Contains("Il ritratto del profilo.", Casella(pannello, "txtCv").Text, "prima c'è")

                    pannello.DimenticaIlCvBase()

                    Assert.IsEmpty(Casella(pannello, "txtCv").Text, "e dopo l'anteprima è vuota")
                    Assert.Contains("eliminato", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "con detto il perché")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IDocumentiDiUnaCandidaturaNonSeNeVannoColProfilo() As Task

            ' L'altra metà della stessa regola, ed è quella che pesa di più: i documenti
            ' di una candidatura sono suoi, stanno nella sua cartella, e l'eliminazione
            ' del profilo non li riguarda.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraLaCandidaturaAsync(Confrontata(contesto))
                    Dim prima As String = Casella(pannello, "txtCv").Text

                    pannello.DimenticaIlCvBase()

                    Assert.AreEqual(prima, Casella(pannello, "txtCv").Text,
                                    "il 🎯 CV mirato della candidatura resta dov'era")
                End Function)

        End Function

        ' ==================================================================
        ' Il banco
        ' ==================================================================

        ' ==================================================================
        ' La lingua dei documenti (T7a, cap. 10.1)
        ' ==================================================================

        <TestMethod>
        Public Async Function LaTendinaSegueLaLinguaDellaCandidatura() As Task

            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "en")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("Inglese", Scelta(pannello, "cmbLingua").Text,
                                    "la tendina dice la lingua della candidatura")
                    Assert.IsTrue(Scelta(pannello, "cmbLingua").Enabled,
                                  "e su una candidatura si può cambiare")

                    ' Il pezzo che conta: l'anteprima è la stessa pagina di blocchi che
                    ' finirà nei file, quindi qui si vede già se le etichette seguono. Il
                    ' CV di prova ha le sole competenze, e quella sezione basta a dirlo.
                    Assert.Contains("Skills", Casella(pannello, "txtCv").Text,
                                    "le etichette del CV parlano inglese")
                    Assert.DoesNotContain("Competenze", Casella(pannello, "txtCv").Text,
                                          "e non c'è rimasta nessuna etichetta italiana")
                End Function)

        End Function

        <TestMethod>
        Public Async Function MostrareUnaCandidaturaNonNeCambiaLaLingua() As Task

            ' Leggere un dato non è scriverlo: allestire la tendina non deve far scattare
            ' la scelta dell'utente — che salva su disco e annota nel registro.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "en")
                    Dim quandoFuScritta As Date = File.GetLastWriteTimeUtc(
                        Path.Combine(candidatura.Cartella, "stato.json"))

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("en", candidatura.Lingua, "la lingua resta quella")
                    Assert.AreEqual(quandoFuScritta,
                                    File.GetLastWriteTimeUtc(Path.Combine(candidatura.Cartella, "stato.json")),
                                    "e il file su disco non è stato riscritto per averlo solo guardato")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlCvBaseNasceNellaLinguaDiCasa() As Task

            ' L'italiano resta il predefinito: chi non tocca niente ottiene quello che ha
            ' sempre ottenuto (cap. 10.1).
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()

                    Assert.AreEqual("it", generatore.LingueChieste.Single(),
                                    "il CV base si scrive nella lingua di casa")
                    Assert.AreEqual("Italiano", Scelta(pannello, "cmbLingua").Text, "e la tendina lo dice")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SulCvBaseLaLinguaSiPuoScegliere() As Task

            ' T7d, cap. 10.3: la lingua si sceglie su un documento, e il 📄 CV base è un
            ' documento. Fino a T7c la tendina era spenta su di lui, perché cambiarla
            ' voleva dire rigenerare *una candidatura* — che il CV base non ha.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()

                    Assert.IsTrue(Scelta(pannello, "cmbLingua").Enabled,
                                  "sul CV base la tendina è accesa come su una candidatura")
                End Function)

        End Function

        <TestMethod>
        Public Async Function LaTendinaSegueLaLinguaDelCvBaseSalvato() As Task

            ' Riaprendolo, il CV base va impaginato con le etichette della sua lingua: è la
            ' stessa pagina di blocchi che finirà nei file, e mostrarla in italiano
            ' significherebbe scoprire l'errore solo aprendo il DOCX (cap. 10.4).
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    GiaScritto(contesto, "en")

                    Await pannello.MostraIlCvBaseAsync()

                    Assert.AreEqual("Inglese", Scelta(pannello, "cmbLingua").Text,
                                    "la tendina dice la lingua del CV che si sta guardando")
                    Assert.Contains("Skills", Casella(pannello, "txtCv").Text,
                                    "e le etichette dell'anteprima parlano inglese")
                    Assert.DoesNotContain("Competenze", Casella(pannello, "txtCv").Text,
                                          "senza nessuna etichetta italiana rimasta")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlCvBaseSiRiscriveNellaLinguaCheHa() As Task

            ' «Rigenera» non cambia lingua da sé: riscrive quello che c'è, com'era —
            ' altrimenti un CV inglese tornerebbe italiano al primo ripensamento.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    GiaScritto(contesto, "en")

                    Await pannello.MostraIlCvBaseAsync()
                    Assert.IsEmpty(generatore.LingueChieste, "riaprendolo non si è generato niente")

                    Await pannello.RigeneraAsync()

                    Assert.AreEqual("en", generatore.LingueChieste.Single(),
                                    "e riscrivendolo si resta in inglese")
                    Assert.AreEqual("en", contesto.Archivio.CaricaCvBase().Lingua,
                                    "col file che continua a dirlo")
                End Function)

        End Function

        <TestMethod>
        <Timeout(15000)>
        Public Async Function SenzaCvBaseCambiareLinguaNonChiedeNiente() As Task

            ' Il gemello di SenzaDocumentiCambiareLinguaNonChiedeNiente, dall'altra parte:
            ' non c'è nessun testo da sostituire, quindi non c'è niente da chiedere. Il
            ' tetto di tempo è la rete di sicurezza — una conferma aperta qui bloccherebbe
            ' il banco intero invece di farlo fallire.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)

                    ' Senza generatore la generazione non parte: il pannello resta sulla
                    ' strada del CV base senza averne uno, che è il caso da provare.
                    Await pannello.MostraIlCvBaseAsync()

                    Scelta(pannello, "cmbLingua").SelectedIndex = 1

                    Assert.Contains("sarà in inglese", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "si prende nota per la prossima volta, e lo si dice")
                End Function)

        End Function

        <TestMethod>
        <Timeout(15000)>
        Public Async Function SenzaDocumentiCambiareLinguaNonChiedeNiente() As Task

            ' Non c'è niente da riscrivere, quindi non c'è niente da chiedere: si prende
            ' nota e si va avanti. Il tetto di tempo è la rete di sicurezza del caso
            ' opposto — una finestra di conferma aperta qui bloccherebbe il banco intero
            ' invece di farlo fallire.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)

                    ' Senza pipeline la generazione non parte: la candidatura resta senza
                    ' documenti, che è proprio il caso da provare.
                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Assert.AreEqual("it", candidatura.Lingua, "si parte dall'italiano")

                    Scelta(pannello, "cmbLingua").SelectedIndex = 1

                    Assert.AreEqual("en", candidatura.Lingua, "la scelta è passata alla candidatura")
                    Assert.AreEqual("en", contesto.Opportunita.Carica(candidatura.Cartella).Lingua,
                                    "ed è già su disco, perché fra qui e la generazione si può chiudere tutto")
                End Function)

        End Function

        ' ==================================================================
        ' I testi riscritti a mano (T9d, cap. 08.4)
        ' ==================================================================

        <TestMethod>
        Public Async Function SiRiscrivonoIDueDocumentiInMostra() As Task

            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "it")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim aperti As List(Of DocumentoDaRiscrivere) = pannello.DocumentiDaRiscrivere()

                    Assert.HasCount(2, aperti, "il 🎯 CV mirato e la ✉️ lettera")
                    Assert.AreSame(candidatura.Cv, aperti(0).Documento, "il CV è quello vero, non una copia")
                    Assert.AreSame(candidatura.Lettera, aperti(1).Documento, "e la lettera pure")

                End Function)

        End Function

        <TestMethod>
        Public Async Function DelCvBaseSiRiscriveIlSoloDocumentoCheCE() As Task

            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    GiaScritto(contesto, "it")

                    Await pannello.MostraIlCvBaseAsync()

                    Assert.HasCount(1, pannello.DocumentiDaRiscrivere(),
                                    "un documento solo: una lettera il CV base non ce l'ha")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlTestoRiscrittoSiSalvaSubitoConLaCandidatura() As Task

            ' Fra la riscrittura e la prossima azione l'utente può chiudere tutto: un
            ' lavoro perso in silenzio è peggio di un lavoro non offerto (v. la lingua).
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "it")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim aperti As List(Of DocumentoDaRiscrivere) = pannello.DocumentiDaRiscrivere()
                    Assert.IsTrue(Rifinitura.Riscrivi(aperti(0).Documento, "sommario", "L'ho riscritto io."),
                                  "è la stessa scrittura che fa la finestra")

                    Await pannello.ConfermaLeRiscrittureAsync(
                        {New RiscritturaFatta With {.Ruolo = RuoloDocumento.Cv, .Id = "sommario"}})

                    Assert.AreEqual("L'ho riscritto io.",
                                    contesto.Opportunita.Carica(candidatura.Cartella).Cv("sommario").GetValue(Of String)(),
                                    "il testo è già su disco")
                    Assert.Contains("L'ho riscritto io.", Casella(pannello, "txtCv").Text,
                                    "e l'anteprima mostra quello che i file conterranno")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlTestoRiscrittoEQuelloCheEsce() As Task

            ' Gli export non sanno niente della modifica a mano: leggono lo stesso
            ' documento (cap. 05.6). È l'unica prova che quel «da qui in poi esce questo»
            ' non sia una promessa scritta solo nel messaggio.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    GiaScritto(contesto, "it")
                    Await pannello.MostraIlCvBaseAsync()

                    Rifinitura.Riscrivi(pannello.DocumentiDaRiscrivere()(0).Documento,
                                        "sommario", "Questo l'ho scritto io.")
                    Await pannello.ConfermaLeRiscrittureAsync(
                        {New RiscritturaFatta With {.Ruolo = RuoloDocumento.Cv, .Id = "sommario"}})

                    Await pannello.EsportaAsync(FormatiDocumento.Docx)

                    Dim scritti As String() = Directory.GetFiles(contesto.Cartella.CartellaOutProfilo, "*.docx")
                    Assert.HasCount(1, scritti, "il file c'è")
                    Assert.Contains("Questo l'ho scritto io.", TestoDelDocx(scritti(0)),
                                    "e dentro c'è il testo dell'utente, non quello dell'AI")
                End Function)

        End Function

        <TestMethod>
        Public Async Function RiscrivereIlCvBaseNonNeRiscriveLaStoria() As Task

            ' Il CV è nato ieri da un altro profilo: correggerne una frase oggi non lo fa
            ' rinascere. Se la provenienza e la data si spostassero, il pannello direbbe
            ' «l'ho scritto oggi» — e «da allora hai cambiato il profilo» smetterebbe di
            ' comparire proprio quando serve.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    GiaScrittoIeri(contesto)
                    Await pannello.MostraIlCvBaseAsync()

                    Rifinitura.Riscrivi(pannello.DocumentiDaRiscrivere()(0).Documento,
                                        "sommario", "L'ho riscritto io.")
                    Await pannello.ConfermaLeRiscrittureAsync(
                        {New RiscritturaFatta With {.Ruolo = RuoloDocumento.Cv, .Id = "sommario"}})

                    Dim salvato As TrovaLavoro.Dati.CvBase = contesto.Archivio.CaricaCvBase()

                    Assert.AreEqual("L'ho riscritto io.", salvato.Cv("sommario").GetValue(Of String)(),
                                    "il testo nuovo è su disco")
                    Assert.AreEqual("profilo-di-ieri", salvato.VersioneProfilo,
                                    "e nasce ancora dal profilo da cui nacque")
                    Assert.AreEqual(New Date(2026, 8, 15, 9, 30, 0), salvato.Generato,
                                    "scritto quando fu scritto")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SiRiscriveSoloDoveCEDellaProsa() As Task

            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)

                    Assert.IsFalse(Bottone(pannello, "btnModificaTesti").Enabled,
                                   "sul pannello vuoto non c'è niente da riscrivere")

                    ' Un CV di soli fatti: nome e competenze vengono dal profilo, e di qui
                    ' non si toccano.
                    contesto.Archivio.SalvaCvBase(
                        JsonNode.Parse("{""tipo"": ""cv_base"", ""intestazione"": {""nome"": ""Luca Ferrari""}}"),
                        contesto.Archivio.Versioni().LastOrDefault(), "it")

                    Await pannello.MostraIlCvBaseAsync()

                    Assert.IsTrue(Bottone(pannello, "btnEsportaDocx").Enabled, "il documento c'è e si esporta")
                    Assert.IsFalse(Bottone(pannello, "btnModificaTesti").Enabled,
                                   "ma di prosa da riscrivere non ne ha")
                End Function)

        End Function

        <TestMethod>
        Public Async Function MentreLAiScriveNonSiRiscriveAMano() As Task

            ' Il caso in cui la guardia conta davvero: la lettera manca e il CV c'è già,
            ' quindi della prosa da riscrivere ce ne sarebbe — ma una chiamata è in volo.
            ' Senza guardia riscriverebbero in due lo stesso documento, e resterebbe quel
            ' che arriva per ultimo.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)

                    Dim candidatura As Opportunita = Confrontata(contesto)
                    candidatura.Cv = JsonNode.Parse(CvMirato)

                    Dim durante As Boolean? = Nothing

                    AddHandler pannello.LavoroAiCambiato,
                        Sub()
                            If pannello.AiAlLavoro AndAlso Not durante.HasValue Then
                                durante = Bottone(pannello, "btnModificaTesti").Enabled
                            End If
                        End Sub

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsTrue(durante.HasValue, "l'AI è stata chiamata davvero")
                    Assert.IsFalse(durante.Value, "e col CV già a video il bottone era spento lo stesso")
                    Assert.IsTrue(Bottone(pannello, "btnModificaTesti").Enabled, "finito il giro si riapre")
                End Function)

        End Function

        ''' <summary>
        ''' Un 📄 CV base già scritto su disco, nella lingua data e sull'ultima versione di
        ''' profilo: è il caso in cui P6 lo ripesca invece di rigenerarlo (T7d).
        ''' </summary>
        Private Shared Sub GiaScritto(contesto As ContestoApp, lingua As String)

            contesto.Archivio.SalvaCvBase(JsonNode.Parse(CvBase),
                                          contesto.Archivio.Versioni().LastOrDefault(), lingua)

        End Sub

        ''' <summary>
        ''' Un 📄 CV base scritto <b>ieri</b>, da una versione di profilo che si riconosce
        ''' a vista: serve a vedere se un risalvataggio gli riscrive la storia (T9d).
        ''' </summary>
        Private Shared Sub GiaScrittoIeri(contesto As ContestoApp)

            contesto.Archivio.SalvaCvBase(JsonNode.Parse(CvBase), "profilo-di-ieri", "it",
                                          New Date(2026, 8, 15, 9, 30, 0))

        End Sub

        ''' <summary>
        ''' Una candidatura con i documenti già scritti, nella lingua data: è il caso in
        ''' cui P6 mostra e basta, senza generare niente.
        ''' </summary>
        ' ==================================================================
        ' La lettera rimasta indietro, e l'avviso che non scade (R7, 2026-08-23)
        ' ==================================================================

        <TestMethod>
        Public Async Function LAvvisoDiRigeneraDiceQualiTestiSiPerdono() As Task

            ' Prima diceva «comprese le modifiche che hai fatto a mano», e solo finché non
            ' si cambiava pannello. «Sommario» è un lavoro che si riconosce.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "it")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsEmpty(pannello.AncheQuelliRiscrittiAMano(),
                                   "chi non ha riscritto niente non si sente dire niente")

                    Await pannello.ConfermaLeRiscrittureAsync(
                        {New RiscritturaFatta With {.Ruolo = RuoloDocumento.Cv, .Id = "sommario"}})

                    Assert.Contains("Sommario", pannello.AncheQuelliRiscrittiAMano(),
                                    "l'avviso nomina il campo")
                End Function)

        End Function

        <TestMethod>
        Public Async Function LAvvisoDiRigeneraNominaAncheLeVociLasciateFuori() As Task

            ' R6: le voci lasciate fuori non si perdono — restano fuori anche dal CV
            ' rifatto — ma chi sta per rigenerare deve saperlo lo stesso, o il
            ' documento nuovo sembrerebbe mancante di qualcosa che nessuno ha chiesto
            ' di togliere. Il singolare e il plurale sono la parte che si sbaglia più
            ' facilmente copiando la riga delle riscritture a mano.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "it")
                    candidatura.Cv = JsonNode.Parse(
                        "{""tipo"": ""cv_mirato"", ""intestazione"": {""nome"": ""Luca Ferrari""}," &
                        """competenze"": [""Uso del muletto"", ""Gestione del magazzino""]}")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim fuori As New List(Of String) From {"competenze¦uso del muletto"}
                    pannello.SegnaLeVociLasciateFuori(fuori)

                    Assert.Contains("La voce che hai lasciato fuori resta fuori anche dal CV rifatto.",
                                    pannello.AncheQuelliRiscrittiAMano(), "il singolare con una voce sola")

                    fuori.Add("competenze¦gestione del magazzino")
                    pannello.SegnaLeVociLasciateFuori(fuori)

                    Assert.Contains("Le 2 voci che hai lasciato fuori restano fuori anche dal CV rifatto.",
                                    pannello.AncheQuelliRiscrittiAMano(), "il plurale con più di una")
                End Function)

        End Function

        <TestMethod>
        Public Async Function LAvvisoNonScadeAlRientroInP6() As Task

            ' È il difetto di R7 preso di petto: prima bastava tornare al profilo e
            ' rientrare perché il pannello dimenticasse, e da lì «Rigenera» si portava via
            ' il lavoro dell'utente senza nominarlo.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "it")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Rifinitura.Riscrivi(pannello.DocumentiDaRiscrivere()(0).Documento,
                                        "sommario", "L'ho riscritto io.")
                    Await pannello.ConfermaLeRiscrittureAsync(
                        {New RiscritturaFatta With {.Ruolo = RuoloDocumento.Cv, .Id = "sommario"}})

                    ' Si esce e si rientra da capo, rileggendo la candidatura da disco:
                    ' è esattamente quel che fa la barra di navigazione.
                    Await pannello.MostraIlCvBaseAsync()
                    Await pannello.MostraLaCandidaturaAsync(
                        contesto.Opportunita.Carica(candidatura.Cartella))

                    Assert.Contains("Sommario", pannello.AncheQuelliRiscrittiAMano(),
                                    "l'avviso c'è ancora, perché adesso l'annotazione sta nel file")
                End Function)

        End Function

        <TestMethod>
        Public Async Function LaSpiaDellaLetteraCompareSoloQuandoServe() As Task

            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "it")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsFalse(Bottone(pannello, "btnRigeneraLettera").Visible,
                                   "documenti d'accordo: il comando non deve nemmeno esserci")

                    candidatura.SegnaLetteraGenerata(New Date(2026, 8, 23, 9, 0, 0))
                    candidatura.SegnaRiscritture(RuoloDocumento.Cv, {"sommario"},
                                                 New Date(2026, 8, 23, 18, 40, 0))
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsTrue(Bottone(pannello, "btnRigeneraLettera").Visible,
                                  "la lettera è rimasta indietro: la spia si accende")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SulCvBaseLaSpiaDellaLetteraNonEsiste() As Task

            ' Il 📄 CV-1 base una lettera non ce l'ha: non c'è niente che possa restare
            ' indietro.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    GiaScritto(contesto, "it")
                    Await pannello.MostraIlCvBaseAsync()

                    Await pannello.ConfermaLeRiscrittureAsync(
                        {New RiscritturaFatta With {.Ruolo = RuoloDocumento.Cv, .Id = "sommario"}})

                    Assert.IsFalse(Bottone(pannello, "btnRigeneraLettera").Visible)
                End Function)

        End Function

        <TestMethod>
        Public Async Function RiscrivereIlCvRiallineaLaLetteraDaSolo() As Task

            ' Il rimedio al silenzio: chiusa la finestra, la lettera si rifà da sé sul CV
            ' come l'ha lasciato l'utente — una volta sola, senza chiedere niente a chi la
            ' lettera non l'aveva toccata.
            Dim generatore As New GeneratoreFinto
            generatore.Dara("{""tipo"": ""lettera_mirata"", ""corpo"": ""Ho traslocato elefanti.""}")

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "it")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Rifinitura.Riscrivi(pannello.DocumentiDaRiscrivere()(0).Documento,
                                        "sommario", "Ho traslocato elefanti.")
                    Await pannello.ConfermaLeRiscrittureAsync(
                        {New RiscritturaFatta With {.Ruolo = RuoloDocumento.Cv, .Id = "sommario"}})

                    Assert.AreEqual("Ho traslocato elefanti.",
                                    candidatura.Lettera("corpo").GetValue(Of String)(),
                                    "la lettera è stata riscritta")
                    Assert.IsFalse(candidatura.LetteraDaRiallineare, "e la spia è spenta")
                    Assert.AreEqual("Ho traslocato elefanti.",
                                    contesto.Opportunita.Carica(candidatura.Cartella).Lettera("corpo").
                                        GetValue(Of String)(),
                                    "e quella nuova è già su disco")
                End Function)

        End Function

        <TestMethod>
        Public Async Function RiscrivereLaLetteraNonFaPartireNessunaRigenerazione() As Task

            ' Il verso conta: la lettera discende dal CV, non il contrario. Rifare qualcosa
            ' qui vorrebbe dire cancellare il testo appena scritto dall'utente.
            Dim generatore As New GeneratoreFinto

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "it")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Await pannello.ConfermaLeRiscrittureAsync(
                        {New RiscritturaFatta With {.Ruolo = RuoloDocumento.Lettera, .Id = "corpo"}})

                    Assert.IsEmpty(generatore.LavoriChiesti(), "nessuna chiamata all'AI")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaAiIlSalvataggioDelTestoRestaUnaBuonaNotizia() As Task

            ' Senza chiave il riallineo non può partire, e non deve trasformare la conferma
            ' di un salvataggio riuscito in un errore rosso: il disallineamento lo racconta
            ' la spia, che è lì col suo comando.
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = GiaScritta(contesto, "it")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Await pannello.ConfermaLeRiscrittureAsync(
                        {New RiscritturaFatta With {.Ruolo = RuoloDocumento.Cv, .Id = "sommario"}})

                    Assert.Contains("Ho salvato", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "la riga parla del salvataggio")
                    Assert.Contains("nel profilo", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "e dice dove si correggono i fatti")
                End Function)

        End Function

        Private Shared Function GiaScritta(contesto As ContestoApp, lingua As String) As Opportunita

            Dim candidatura As Opportunita = Confrontata(contesto)

            candidatura.Lingua = lingua
            candidatura.Cv = JsonNode.Parse(CvMirato)
            candidatura.Lettera = JsonNode.Parse(Lettera)
            contesto.Opportunita.Salva(candidatura)

            Return candidatura

        End Function

        ''' <summary>
        ''' Un pannello collegato a un motore vero — cartella temporanea, nessuna chiave —
        ''' con un profilo salvato, il generatore finto che gli si vuol dare e un archivio
        ''' documenti <b>senza stampante</b>.
        ''' </summary>
        ' ==================================================================
        ' La candidatura eliminata dalla Home (cap. 11.5)
        ' ==================================================================

        <TestMethod>
        Public Async Function IDocumentiLascianoAndareLaCandidaturaEliminata() As Task

            ' Qui il rischio è il più concreto dei tre pannelli: «Rigenera» e le
            ' esportazioni scrivono nella cartella della candidatura, e su un documento
            ' sopravvissuto alla sua cartella la ricreerebbero.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim dove As String = candidatura.Cartella

                    Assert.IsFalse(pannello.Dimentica(dove & "-di-un-altra"),
                                   "una candidatura che non è la sua non lo riguarda")
                    Assert.IsNotNull(pannello.Candidatura, "e infatti la sua ce l'ha ancora")

                    contesto.Opportunita.Elimina(dove)

                    Assert.IsTrue(pannello.Dimentica(dove), "questa invece era proprio la sua")
                    Assert.IsNull(pannello.Candidatura, "e non la tiene più in mano")
                    Assert.AreEqual(String.Empty, Casella(pannello, "txtCv").Text,
                                    "le colonne tornano vuote")
                End Function)

        End Function

        Private Shared Async Function ConPannelloAsync(
                generatore As GeneratoreFinto,
                prova As Func(Of PannelloDocumenti, ContestoApp, ArchivioDocumenti, Task),
                Optional rifinitore As RifinitoreFinto = Nothing) As Task

            Dim radice As String = Path.Combine(
                Path.GetTempPath(), "pannello-documenti-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      pannello As New PannelloDocumenti()

                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))

                    Dim documenti As New ArchivioDocumenti(contesto.Cartella)
                    Dim pipeline As PipelineCandidatura = Nothing

                    ' Senza chiave il contesto non monta l'AI, e con lei nemmeno la
                    ' rifinitura: chi la vuole collaudare passa la sua, come fa col
                    ' generatore (T7b).
                    Dim rifinitura As Rifinitura =
                        If(rifinitore Is Nothing, Nothing, New Rifinitura(rifinitore))

                    If generatore IsNot Nothing Then
                        pipeline = New PipelineCandidatura(New AnalizzatoreFinto, New ConfrontatoreFinto,
                                                           generatore, Nothing, rifinitura)
                    End If

                    pannello.CreateControl()
                    pannello.Collega(contesto, documenti, pipeline, generatore, rifinitura)

                    Await prova(pannello, contesto, documenti)
                End Using

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Function

        ''' <summary>Un'opportunità già confrontata e già salvata, come arriva da P4.</summary>
        Private Shared Function Confrontata(contesto As ContestoApp) As Opportunita

            Dim candidatura As New Opportunita With {
                .Annuncio = JsonNode.Parse(AnnuncioLetto),
                .Confronto = JsonNode.Parse(
                    "{""giudizi"": [{""requisito"": ""Uso del muletto"", ""esito"": ""soddisfatto""}]," &
                    """lettura_insieme"": ""In linea."", ""numero_complessivo"": 90}"),
                .Creata = New Date(2026, 8, 10)}

            contesto.Opportunita.Salva(candidatura)
            Return candidatura

        End Function

        ''' <summary>Il testo dentro un <c>.docx</c>: l'XML del documento, tag compresi.</summary>
        ''' <remarks>
        ''' Basta per chiedersi «questa frase c'è dentro?», che è l'unica domanda che il
        ''' banco fa a un file impaginato: come sia impaginato lo guarda
        ''' <c>CollaudiImpaginazione</c>, e come sia fatto lo ZIP <c>CollaudiScrittoreDocx</c>.
        ''' </remarks>
        Private Shared Function TestoDelDocx(percorso As String) As String

            Using archivio As ZipArchive = ZipFile.OpenRead(percorso)
                Using lettore As New StreamReader(archivio.GetEntry("word/document.xml").Open())
                    Return lettore.ReadToEnd()
                End Using
            End Using

        End Function

        Private Shared Function Casella(pannello As Control, nome As String) As TextBox
            Return Casella(Of TextBox)(pannello, nome)
        End Function

        Private Shared Function Casella(Of T As Control)(pannello As Control, nome As String) As T
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), T)
        End Function

        Private Shared Function Bottone(pannello As Control, nome As String) As Button
            Return Casella(Of Button)(pannello, nome)
        End Function

        Private Shared Function Etichetta(pannello As Control, nome As String) As Label
            Return Casella(Of Label)(pannello, nome)
        End Function

        Private Shared Function Scelta(pannello As Control, nome As String) As ComboBox
            Return Casella(Of ComboBox)(pannello, nome)
        End Function

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

        ' --- La tendina «Esporta:» (R8, 2026-08-23) --------------------------------------

        <TestMethod>
        Public Sub LaTendinaDiEsportazioneDiceQuelloCheMostra()

            Assert.AreEqual(DocumentiDaScrivere.Entrambi, PannelloDocumenti.DocumentiDaTendina(0),
                            "la prima voce è «CV e lettera»")
            Assert.AreEqual(DocumentiDaScrivere.Cv, PannelloDocumenti.DocumentiDaTendina(1),
                            "la seconda è «Solo il CV»")
            Assert.AreEqual(DocumentiDaScrivere.Lettera, PannelloDocumenti.DocumentiDaTendina(2),
                            "la terza è «Solo la lettera»")

        End Sub

        <TestMethod>
        Public Sub UnaTendinaSenzaSceltaEsportaTutto()

            ' -1 è quel che risponde una ComboBox su cui nessuno ha ancora scelto: da lì non
            ' deve uscire «esporta niente», ma il comportamento di sempre.
            Assert.AreEqual(DocumentiDaScrivere.Entrambi, PannelloDocumenti.DocumentiDaTendina(-1),
                            "senza scelta si esporta tutto, come prima di R8")
            Assert.AreEqual(DocumentiDaScrivere.Entrambi, PannelloDocumenti.DocumentiDaTendina(99),
                            "e un indice che non esiste non inventa una terza via")

        End Sub

        ' ==================================================================
        ' La candidatura di un profilo che non c'è più (2026-08-24)
        ' ==================================================================

        ''' <summary>Una versione di profilo che nello storico non c'è mai stata.</summary>
        Private Const ProfiloSparito As String = "2026-07-01_090000"

        ''' <summary>
        ''' Alla generazione arrivano il profilo di <b>oggi</b> e i giudizi di
        ''' <b>allora</b>: se quel profilo è stato eliminato e rifatto, i due parlano di due
        ''' persone diverse e il modello risponde con delle spiegazioni invece che col
        ''' documento — che l'utente leggeva come «l'AI ha risposto in una forma che non
        ''' riesco a leggere». Adesso non ci si arriva.
        ''' </summary>
        <TestMethod>
        Public Async Function UnaCandidaturaDiUnProfiloSparitoNonSiGenera() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)
                    candidatura.VersioneProfilo = ProfiloSparito

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsEmpty(generatore.LavoriChiesti(), "l'AI non è stata chiamata affatto")
                    Assert.Contains("non c'è più", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "e il pannello dice cos'è successo")
                    Assert.Contains("rifai la candidatura", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "e cosa si può fare")
                End Function)

        End Function

        <TestMethod>
        Public Async Function RigenerareNonButtaViaIDocumentiDiUnProfiloSparito() As Task

            ' Quei documenti sono tutto quel che resta di quella candidatura: «Rigenera» li
            ' azzera prima di riscriverli, e fermarsi dopo li avrebbe distrutti per niente.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Assert.AreEqual("cv_mirato → lettera", generatore.LavoriChiesti(), "i documenti ci sono")

                    ' E adesso il profilo di allora sparisce.
                    candidatura.VersioneProfilo = ProfiloSparito
                    Await pannello.RigeneraAsync()

                    Assert.AreEqual("cv_mirato → lettera", generatore.LavoriChiesti(),
                                    "l'AI non è stata chiamata una seconda volta")
                    Assert.IsNotNull(candidatura.Cv, "e il CV di allora è ancora lì")
                    Assert.IsNotNull(candidatura.Lettera, "e la lettera pure")
                End Function)

        End Function

        <TestMethod>
        Public Async Function LaLetteraNonSiRiallineaSuUnProfiloSparito() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    candidatura.VersioneProfilo = ProfiloSparito

                    Await pannello.RiallineaLaLetteraAsync()

                    Assert.AreEqual("cv_mirato → lettera", generatore.LavoriChiesti(),
                                    "la lettera non è stata riscritta")
                    Assert.Contains("non c'è più", Etichetta(pannello, "lblStatoDocumenti").Text)
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnProfiloSoloCresciutoNonFermaLaRigenerazione() As Task

            ' Il rovescio, ed è il caso frequente: il profilo cambia versione a ogni
            ' salvataggio, e i vecchi documenti restano spiegabili. Fermarsi anche qui
            ' sarebbe un avviso a ogni giro, per qualcosa che funziona.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera).Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)
                    candidatura.VersioneProfilo = contesto.Archivio.Versioni().Last()

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    ' Il profilo cresce: versione nuova, ma quella di allora è ancora lì.
                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))
                    Await pannello.RigeneraAsync()

                    Assert.AreEqual("cv_mirato → lettera → cv_mirato → lettera",
                                    generatore.LavoriChiesti(), "il giro si rifà tutto, come sempre")
                End Function)

        End Function

        <TestMethod>
        <Timeout(15000)>
        Public Async Function CambiareLinguaSuUnProfiloSparitoNonChiedeNiente() As Task

            ' Chiedere «li riscrivo in inglese?» per poi rispondere che non si possono
            ' riscrivere sarebbe una domanda a vuoto, e la lingua nuova sarebbe già finita
            ' su disco. Il tetto di tempo è la rete di sicurezza: senza la guardia, qui si
            ' apre una conferma e il banco si fermerebbe invece di diventare rosso.
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvMirato).Dara(Lettera)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Dim candidatura As Opportunita = Confrontata(contesto)

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    candidatura.VersioneProfilo = ProfiloSparito

                    Scelta(pannello, "cmbLingua").SelectedIndex = 1

                    Assert.AreEqual("it", candidatura.Lingua, "la lingua non è cambiata")
                    Assert.AreEqual("Italiano", Scelta(pannello, "cmbLingua").Text,
                                    "e la tendina è tornata dov'era")
                    Assert.Contains("non c'è più", Etichetta(pannello, "lblStatoDocumenti").Text,
                                    "al posto della domanda c'è il motivo")
                    Assert.AreEqual("cv_mirato → lettera", generatore.LavoriChiesti(),
                                    "e l'AI non è stata richiamata")
                End Function)

        End Function

    End Class

End Namespace
