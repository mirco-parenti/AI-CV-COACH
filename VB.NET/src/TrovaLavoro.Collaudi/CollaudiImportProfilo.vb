Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi dell'import di un CV già esistente (voce 2.1.2, cap. 05.1). Girano
    ''' <b>senza rete</b>: al posto dell'AI ci sono i due sostituti — trascrittore e
    ''' strutturatore — così quello che si verifica è ciò che questa classe decide
    ''' davvero: quale strada prende ogni formato, quando ci si ferma e cosa si dice a
    ''' chi legge. L'unico collaudo con l'API vera è in fondo, nella categoria
    ''' <b>Reale</b>, e usa un CV vero.
    ''' </summary>
    <TestClass>
    Public Class CollaudiImportProfilo

        ''' <summary>Un CV in chiaro, abbastanza lungo da superare la soglia dell'import.</summary>
        Private Const CvDiProva As String =
            "MARIO ROSSI" & vbLf &
            "Via Roma 1, Forlì — mario.rossi@example.it — 333 1234567" & vbLf & vbLf &
            "ESPERIENZE" & vbLf &
            "2019-2024 Magazziniere presso Logistica Rossi: carico e scarico merci, uso del muletto." & vbLf &
            "2015-2019 Barista presso Bar Centrale: servizio al banco, cassa, chiusura serale." & vbLf & vbLf &
            "FORMAZIONE" & vbLf &
            "2015 Diploma di perito elettrotecnico, ITIS Marconi." & vbLf & vbLf &
            "PATENTE B, automunito."

        ''' <summary>Il frammento che l'AI restituirebbe per quel CV.</summary>
        Private Shared Function ProfiloStrutturato() As String
            Return CasiDiCollaudo.Profilo().ToJsonString()
        End Function

        <TestMethod>
        Public Async Function DalPdfEsceUnProfiloPassandoPerIDuePassi() As Task
            ' I due passi del prototipo: prima il testo (l'AI trascrive il PDF), poi la
            ' struttura (il turno importa_cv). Il secondo deve ricevere esattamente il
            ' testo del primo: è lì che si perderebbe metà CV senza accorgersene.
            Dim trascrittore As New TrascrittoreFinto(CvDiProva)
            Dim strutturatore As New StrutturatoreFinto()
            strutturatore.Dara(ProfiloStrutturato())

            Dim esito As EsitoImport = Await ConFileDiProva(".pdf", "%PDF-1.7 finto",
                Function(percorso) New ImportProfilo(strutturatore, trascrittore).DaFileAsync(percorso))

            Assert.HasCount(1, trascrittore.Chiamate, "il PDF passa dal trascrittore")
            Assert.HasCount(1, strutturatore.Chiamate, "e poi dallo strutturatore")
            Assert.AreEqual(ImportProfilo.TurnoImport, strutturatore.Chiamate(0).Turno,
                            "con il turno importa_cv, quello del pool")
            Assert.AreEqual(CvDiProva, strutturatore.Chiamate(0).Risposta,
                            "e con il testo trascritto, tutto intero")

            Assert.AreEqual("Luca Ferrari", esito.Profilo.Nome, "il profilo proposto")
            Assert.AreEqual(FormatoDocumento.Pdf, esito.Formato, "il formato riconosciuto")
            Assert.AreEqual(CvDiProva, esito.TestoLetto,
                            "e il testo di partenza resta insieme al profilo, per poter controllare")
        End Function

        <TestMethod>
        Public Async Function DalDocxNonSiDisturbaLAi() As Task
            ' Il .docx si legge dal disco: chiamare l'AI per trascriverlo sarebbe tempo e
            ' token spesi per niente.
            Dim trascrittore As New TrascrittoreFinto("non dovrei essere chiamato")
            Dim strutturatore As New StrutturatoreFinto()
            strutturatore.Dara(ProfiloStrutturato())

            Dim esito As EsitoImport = Await ConDocxDiProva(
                Function(percorso) New ImportProfilo(strutturatore, trascrittore).DaFileAsync(percorso))

            Assert.IsEmpty(trascrittore.Chiamate, "il trascrittore non c'entra col .docx")
            Assert.Contains("Magazziniere", strutturatore.Chiamate(0).Risposta,
                            "allo strutturatore arriva il testo letto dal documento")
            Assert.AreEqual(FormatoDocumento.Docx, esito.Formato, "formato docx")
            Assert.IsNotNull(esito.Profilo, "e il profilo esce lo stesso")
        End Function

        <TestMethod>
        Public Async Function DalTxtEsceUnProfilo() As Task
            Dim strutturatore As New StrutturatoreFinto()
            strutturatore.Dara(ProfiloStrutturato())

            Dim esito As EsitoImport = Await ConFileDiProva(".txt", CvDiProva,
                Function(percorso) New ImportProfilo(strutturatore).DaFileAsync(percorso))

            Assert.AreEqual(CvDiProva, esito.TestoLetto, "il testo letto dal disco")
            Assert.AreEqual(FormatoDocumento.Testo, esito.Formato, "formato testo")
            Assert.AreEqual("Luca Ferrari", esito.Profilo.Nome, "il profilo proposto")
        End Function

        <TestMethod>
        Public Async Function UnPdfScannerizzatoSiFermaEProponeIlRipiego() As Task
            ' Il caso limite dichiarato al cap. 05.1: un PDF che è solo un'immagine
            ' restituisce pochissimo testo. Strutturarlo darebbe un profilo vuoto senza
            ' dire perché — e per giunta pagando una seconda chiamata.
            Dim trascrittore As New TrascrittoreFinto("Mario Rossi" & vbLf & "CV")
            Dim strutturatore As New StrutturatoreFinto()

            Dim errore As ErroreImport = Await Assert.ThrowsAsync(Of ErroreImport)(
                Function() ConFileDiProva(".pdf", "%PDF-1.7 finto",
                    Function(percorso) New ImportProfilo(strutturatore, trascrittore).DaFileAsync(percorso)))

            Assert.AreEqual(CausaImport.TestoTroppoCorto, errore.Causa, "causa")
            Assert.Contains("scannerizzato", errore.Message, "deve dire cosa è successo")
            Assert.Contains("incollare", errore.Message, "e offrire il ripiego onesto")
            Assert.IsEmpty(strutturatore.Chiamate, "senza spendere una chiamata per il vuoto")
        End Function

        <TestMethod>
        Public Async Function UnFormatoNonAccettatoSiFermaPrimaDiToccareIlDisco() As Task
            Dim strutturatore As New StrutturatoreFinto()

            Dim errore As ErroreImport = Await Assert.ThrowsAsync(Of ErroreImport)(
                Function() New ImportProfilo(strutturatore).DaFileAsync("C:\CV\curriculum.odt"))

            Assert.AreEqual(CausaImport.FormatoNonSupportato, errore.Causa, "causa")
            Assert.Contains("PDF, DOCX, TXT o MD", errore.Message, "dice quali formati accetta")
            Assert.IsEmpty(strutturatore.Chiamate, "e non chiama nessuno")
        End Function

        <TestMethod>
        Public Async Function UnFileCheNonCEsisteLoDiceSubito() As Task
            Dim strutturatore As New StrutturatoreFinto()

            Dim errore As ErroreImport = Await Assert.ThrowsAsync(Of ErroreImport)(
                Function() New ImportProfilo(strutturatore).DaFileAsync(
                    Path.Combine(Path.GetTempPath(), "cv-che-non-esiste.txt")))

            Assert.AreEqual(CausaImport.FileMancante, errore.Causa, "causa")
        End Function

        <TestMethod>
        Public Async Function UnPdfOltreIlLimiteSiFermaPrimaDiCaricarlo() As Task
            ' Il limite è dell'API: superarlo significherebbe leggere in memoria decine di
            ' MB, codificarli in base64 e farseli rifiutare. Meglio dirlo prima, con la
            ' misura vera e il ripiego.
            Dim trascrittore As New TrascrittoreFinto("mai arrivato qui")

            Dim percorso As String = Path.Combine(
                Path.GetTempPath(), "cv-enorme-" & Guid.NewGuid().ToString("N") & ".pdf")

            ' File «vuoto» ma dichiarato lungo: non serve scrivere davvero 33 MB.
            Using flusso As New FileStream(percorso, FileMode.CreateNew)
                flusso.SetLength(TrascrittorePdf.DimensioneMassima + 1)
            End Using

            Try
                Dim errore As ErroreImport = Await Assert.ThrowsAsync(Of ErroreImport)(
                    Function() New ImportProfilo(New StrutturatoreFinto(), trascrittore).DaFileAsync(percorso))

                Assert.AreEqual(CausaImport.FileTroppoGrande, errore.Causa, "causa")
                Assert.Contains("32 MB", errore.Message, "dice qual è il limite")
                Assert.IsEmpty(trascrittore.Chiamate, "e il file non viene nemmeno letto")
            Finally
                If File.Exists(percorso) Then File.Delete(percorso)
            End Try
        End Function

        <TestMethod>
        Public Async Function SenzaTrascrittoreIlPdfLoDiceInveceDiCadere() As Task
            ' L'import si può costruire anche senza AI per i PDF (è così nei collaudi del
            ' resto del motore): in quel caso il PDF deve fermarsi con un messaggio, non
            ' con un riferimento nullo.
            Dim errore As ErroreImport = Await Assert.ThrowsAsync(Of ErroreImport)(
                Function() ConFileDiProva(".pdf", "%PDF-1.7 finto",
                    Function(percorso) New ImportProfilo(New StrutturatoreFinto()).DaFileAsync(percorso)))

            Assert.AreEqual(CausaImport.FormatoNonSupportato, errore.Causa, "causa")
            Assert.Contains("DOCX, TXT o MD", errore.Message, "e propone le altre strade")
        End Function

        <TestMethod>
        Public Async Function UnDocumentoIllegibileDiventaUnErroreLeggibile() As Task
            ' Il .doc rinominato in .docx: il messaggio del lettore deve arrivare fino a
            ' chi legge, senza diventare un errore tecnico di ZIP.
            Dim errore As ErroreImport = Await Assert.ThrowsAsync(Of ErroreImport)(
                Function() ConFileDiProva(".docx", "questo non è un archivio ZIP, è un vecchio .doc",
                    Function(percorso) New ImportProfilo(New StrutturatoreFinto()).DaFileAsync(percorso)))

            Assert.AreEqual(CausaImport.DocumentoIllegibile, errore.Causa, "causa")
            Assert.Contains(".docx", errore.Message, "col suggerimento di risalvarlo")
        End Function

        <TestMethod>
        Public Async Function DalTestoIncollatoEscePureUnProfilo() As Task
            ' È il ripiego di tutti i casi limite, e sarà anche la porta del profilo
            ' LinkedIn (T5b): stessa strada, senza passare dal disco.
            Dim strutturatore As New StrutturatoreFinto()
            strutturatore.Dara(ProfiloStrutturato())

            Dim esito As EsitoImport = Await New ImportProfilo(strutturatore).DaTestoAsync(CvDiProva)

            Assert.AreEqual(ImportProfilo.TurnoImport, strutturatore.Chiamate(0).Turno, "sempre importa_cv")
            Assert.AreEqual("Luca Ferrari", esito.Profilo.Nome, "il profilo proposto")
            Assert.AreEqual("testo incollato", esito.Origine, "e si sa da dove viene")
        End Function

        <TestMethod>
        Public Async Function UnaRispostaCheNonEUnProfiloLoDice() As Task
            ' L'AI ha risposto un JSON valido ma che profilo non è: meglio dirlo che
            ' proporre all'utente un profilo vuoto nato da un malinteso.
            Dim strutturatore As New StrutturatoreFinto()
            strutturatore.Dara("[""non sono un profilo""]")

            Dim errore As ErroreImport = Await Assert.ThrowsAsync(Of ErroreImport)(
                Function() New ImportProfilo(strutturatore).DaTestoAsync(CvDiProva))

            Assert.AreEqual(CausaImport.ProfiloIllegibile, errore.Causa, "causa")
            Assert.Contains("dialogo guidato", errore.Message, "e resta la strada del dialogo")
        End Function

        <TestMethod>
        Public Async Function IlProfiloProdottoNonVieneSalvatoDaSolo() As Task
            ' Regola del cap. 05.2: niente entra nel profilo senza conferma. L'import
            ' propone, non scrive: a salvare sarà il pannello, dopo che l'utente ha visto.
            Dim strutturatore As New StrutturatoreFinto()
            strutturatore.Dara(ProfiloStrutturato())

            Dim radice As String = Path.Combine(Path.GetTempPath(), "import-" & Guid.NewGuid().ToString("N"))
            Dim cartella As New CartellaDati(radice)
            Dim archivio As New ArchivioProfilo(cartella)

            Try
                Await New ImportProfilo(strutturatore).DaTestoAsync(CvDiProva)

                Assert.IsFalse(archivio.Esiste, "l'import non deve aver scritto nessun profilo")
                Assert.IsEmpty(archivio.Versioni(), "né lasciato versioni nella storia")
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try
        End Function

        ''' <summary>
        ''' L'unico collaudo di questa classe che chiama l'API vera, ed è il collaudo di
        ''' tappa dichiarato al cap. 14 per T3: un <b>CV vero</b>, in PDF, deve arrivare
        ''' fino a un profilo con dentro le cose che il CV dice.
        ''' </summary>
        ''' <remarks>
        ''' <para>Il CV è un dato personale e non sta nel repo: il collaudo lo cerca nella
        ''' cartella indicata dalla variabile d'ambiente <c>CV_DI_PROVA</c> e, se non la
        ''' trova, si dichiara inconcludente invece di fallire — sull'altra postazione non
        ''' c'è.</para>
        ''' <para>Categoria <b>Reale</b>: si lancia dove c'è la chiave, da
        ''' <c>VB.NET/src</c>, con
        ''' <c>dotnet test --settings TrovaLavoro.Collaudi/collaudi-reali.runsettings</c>.
        ''' Da WSL, perché le due variabili arrivino all'eseguibile Windows, vanno
        ''' elencate in <c>WSLENV</c> — quella del percorso col suffisso <c>/p</c>, che ne
        ''' traduce la forma: <c>WSLENV=ANTHROPIC_API_KEY:CV_DI_PROVA/p</c>.</para>
        ''' </remarks>
        <TestMethod, TestCategory("Reale")>
        Public Async Function IlCvVeroInPdfDiventaUnProfilo() As Task

            Dim cartella As String = Environment.GetEnvironmentVariable("CV_DI_PROVA")
            If String.IsNullOrWhiteSpace(cartella) OrElse Not Directory.Exists(cartella) Then
                Assert.Inconclusive(
                    "Collaudo reale saltato: definisci CV_DI_PROVA con la cartella che contiene " &
                    "il CV in PDF da importare.")
                Return
            End If

            Dim pdf As String = Directory.EnumerateFiles(cartella, "*.pdf").OrderBy(Function(f) f).FirstOrDefault()
            If pdf Is Nothing Then
                Assert.Inconclusive($"Collaudo reale saltato: in «{cartella}» non c'è nessun PDF.")
                Return
            End If

            Dim chiave As String
            Try
                chiave = ClientClaude.ChiaveDaAmbiente()
            Catch ex As ErroreAi
                Assert.Inconclusive(
                    "Collaudo reale saltato: manca la chiave. Definisci " &
                    ClientClaude.NomeVariabileChiave & " nell'ambiente prima di lanciare il banco.")
                Return
            End Try

            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Using client As New ClientClaude(chiave)

                Dim importatore As New ImportProfilo(
                    New StrutturatoreTurni(libreria, client),
                    New TrascrittorePdf(libreria, client))

                Dim esito As EsitoImport = Await importatore.DaFileAsync(pdf)
                Dim profilo As TrovaLavoro.Dati.Profilo = esito.Profilo

                ' Cosa si può affermare senza sapere cosa c'è scritto in quel CV: che la
                ' trascrizione ha prodotto un CV intero e non una briciola, e che il
                ' profilo ne ha raccolto le parti principali.
                Assert.IsGreaterThan(1000, esito.TestoLetto.Length,
                                     "dal PDF deve uscire il testo di un CV, non poche righe")
                Assert.IsNotEmpty(profilo.Nome, "il nome")
                Assert.IsNotEmpty(profilo.Contatti.Email, "un recapito")
                Assert.IsGreaterThan(0, profilo.EsperienzeFormali.Count, "almeno un'esperienza")
                Assert.IsGreaterThan(0, profilo.Formazione.Count, "almeno un titolo di studio")

                ' L'anti-invenzione non si può affermare con un Assert: si guarda. Il
                ' riassunto serve a questo, e resta nel rapporto del collaudo.
                Dim riassunto As New StringBuilder()
                riassunto.AppendLine($"CV importato: {Path.GetFileName(pdf)}")
                riassunto.AppendLine($"Testo trascritto: {esito.TestoLetto.Length} caratteri")
                riassunto.AppendLine($"Nome: {profilo.Nome}")
                riassunto.AppendLine($"Esperienze formali: {profilo.EsperienzeFormali.Count}")
                riassunto.AppendLine($"Esperienze informali: {profilo.EsperienzeInformali.Count}")
                riassunto.AppendLine($"Competenze: {profilo.Competenze.Count}")
                riassunto.AppendLine($"Formazione: {profilo.Formazione.Count}")
                riassunto.AppendLine($"Patente: {profilo.Patente.Ha} {String.Join(", ", profilo.Patente.Categorie)}")
                Console.WriteLine(riassunto.ToString())

            End Using

        End Function

        ''' <summary>Fa girare la prova su un file di prova col testo dato, e poi lo toglie.</summary>
        Private Shared Async Function ConFileDiProva(estensione As String, contenuto As String,
                                                     prova As Func(Of String, Task(Of EsitoImport))) As Task(Of EsitoImport)

            Dim percorso As String = Path.Combine(
                Path.GetTempPath(), "cv-" & Guid.NewGuid().ToString("N") & estensione)

            File.WriteAllText(percorso, contenuto, New UTF8Encoding(False))
            Try
                Return Await prova(percorso)
            Finally
                If File.Exists(percorso) Then File.Delete(percorso)
            End Try

        End Function

        ''' <summary>Come sopra, ma con un <c>.docx</c> vero: un archivio con dentro il CV.</summary>
        Private Shared Async Function ConDocxDiProva(prova As Func(Of String, Task(Of EsitoImport))) As Task(Of EsitoImport)

            Dim percorso As String = Path.Combine(
                Path.GetTempPath(), "cv-" & Guid.NewGuid().ToString("N") & ".docx")

            File.WriteAllBytes(percorso, DocxCol(CvDiProva))
            Try
                Return Await prova(percorso)
            Finally
                If File.Exists(percorso) Then File.Delete(percorso)
            End Try

        End Function

        ''' <summary>Un <c>.docx</c> minimo che contiene il testo dato, una riga per paragrafo.</summary>
        Private Shared Function DocxCol(testo As String) As Byte()

            Dim paragrafi As String = String.Concat(
                testo.Split(vbLf).Select(Function(riga) $"<w:p><w:r><w:t>{riga}</w:t></w:r></w:p>"))

            Dim memoria As New MemoryStream()

            Using archivio As New IO.Compression.ZipArchive(memoria, IO.Compression.ZipArchiveMode.Create, leaveOpen:=True)
                Dim parte As IO.Compression.ZipArchiveEntry = archivio.CreateEntry("word/document.xml")
                Using scrittore As New StreamWriter(parte.Open(), New UTF8Encoding(False))
                    scrittore.Write(
                        "<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>" &
                        "<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">" &
                        "<w:body>" & paragrafi & "</w:body></w:document>")
                End Using
            End Using

            Return memoria.ToArray()

        End Function

    End Class

End Namespace
