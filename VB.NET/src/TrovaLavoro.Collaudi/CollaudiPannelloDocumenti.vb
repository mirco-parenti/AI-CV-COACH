Imports System.Drawing
Imports System.IO
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
                    Assert.AreEqual("Torna al profilo", Bottone(pannello, "btnTornaIndietro").Text,
                                    "dal CV base si torna al profilo, che è dove è nato")

                    Await pannello.MostraLaCandidaturaAsync(Confrontata(contesto))
                    Assert.AreEqual("Torna all'opportunità", Bottone(pannello, "btnTornaIndietro").Text,
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
                    Assert.IsEmpty(Casella(pannello, "txtCv").Text, "e niente CV")
                End Function)

        End Function

        <TestMethod>
        Public Sub CioCheArriveraSiVedeSpentoEDiceQuando()

            ' Cap. 03.8: è il pannello che più di ogni altro mostra dove sta andando il
            ' progetto, e lo mostra spegnendo, non nascondendo.
            Using pannello As New PannelloDocumenti()

                Assert.IsFalse(Scelta(pannello, "cmbLingua").Enabled, "la lingua arriva con T7")
                Assert.IsFalse(Casella(Of CheckBox)(pannello, "chkRifinitura").Enabled, "l'anti-slop pure")
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
        Public Async Function IlCvBaseNonHaUnaLinguaDaScegliere() As Task

            ' Cap. 10.1: la lingua è una proprietà della candidatura. Il 📄 CV base non
            ' nasce da un annuncio e non si genera da questo pannello, quindi la tendina
            ' resta spenta — col suo motivo, non muta (cap. 03.8).
            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConPannelloAsync(
                generatore,
                Async Function(pannello, contesto, documenti)
                    Await pannello.MostraIlCvBaseAsync()

                    Assert.IsFalse(Scelta(pannello, "cmbLingua").Enabled, "la tendina è spenta")
                    Assert.AreEqual("it", generatore.LingueChieste.Single(),
                                    "e il CV base si scrive nella lingua di casa")
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

        ''' <summary>
        ''' Una candidatura con i documenti già scritti, nella lingua data: è il caso in
        ''' cui P6 mostra e basta, senza generare niente.
        ''' </summary>
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
        Private Shared Async Function ConPannelloAsync(
                generatore As GeneratoreFinto,
                prova As Func(Of PannelloDocumenti, ContestoApp, ArchivioDocumenti, Task)) As Task

            Dim radice As String = Path.Combine(
                Path.GetTempPath(), "pannello-documenti-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      pannello As New PannelloDocumenti()

                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))

                    Dim documenti As New ArchivioDocumenti(contesto.Cartella)
                    Dim pipeline As PipelineCandidatura = Nothing

                    If generatore IsNot Nothing Then
                        pipeline = New PipelineCandidatura(New AnalizzatoreFinto, New ConfrontatoreFinto, generatore)
                    End If

                    pannello.CreateControl()
                    pannello.Collega(contesto, documenti, pipeline, generatore)

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

    End Class

End Namespace
