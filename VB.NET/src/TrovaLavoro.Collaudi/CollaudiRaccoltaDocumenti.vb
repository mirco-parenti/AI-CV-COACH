Imports System.IO
Imports System.Linq
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi della cartella documenti dell'utente (cap. 05.2): l'elenco di quel che ci
    ''' si è riconosciuto dentro, e come si tiene in pari coi file veri.
    ''' </summary>
    ''' <remarks>
    ''' Le cose che qui possono rompersi davvero sono tre, e sono tutte promesse del
    ''' capitolo: che una correzione fatta a mano <b>resti</b>; che un file cancellato dalla
    ''' cartella sparisca anche dall'elenco; e che nessun nome inventato da una risposta
    ''' dell'AI diventi un documento da allegare.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiRaccoltaDocumenti

        Private Shared Function Trovato(nome As String) As FileTrovato
            Return New FileTrovato With {.Nome = nome, .Percorso = "C:\documenti\" & nome}
        End Function

        Private Shared Function ConDocumenti(ParamArray nomi As String()) As RaccoltaDocumenti

            Dim raccolta As New RaccoltaDocumenti With {.Cartella = "C:\documenti"}
            raccolta.AllineaAiFile(nomi.Select(AddressOf Trovato).ToList())

            Return raccolta

        End Function

        <TestMethod>
        Public Sub UnFileMaiVistoEntraComeAltro()
            ' «Altro» è la categoria che non promette niente, e quindi l'unica che si può
            ' attribuire prima di aver capito: un file appena messo nella cartella non deve
            ' finire fra gli allegati proposti solo perché è comparso.
            Dim raccolta As RaccoltaDocumenti = ConDocumenti("CV.pdf", "busta_paga.pdf")

            Assert.HasCount(2, raccolta.Documenti, "tutti e due sono nell'elenco")
            Assert.AreEqual(CategoriaDocumento.Altro, raccolta.Documenti(0).Categoria, "nessuno è già qualcosa")
            Assert.IsEmpty(raccolta.Attestati(), "e niente da proporre come allegato")
        End Sub

        <TestMethod>
        Public Sub UnFileSparitoEsceDallElenco()
            ' Dei nomi ci si fida, dei file no (cap. 07.1): quel che non c'è più nella
            ' cartella non deve tornare in vita perché un elenco lo nominava.
            Dim raccolta As RaccoltaDocumenti = ConDocumenti("CV.pdf", "HACCP.pdf")
            raccolta.Documenti(1).Categoria = CategoriaDocumento.Attestato

            Dim mai As Integer = raccolta.AllineaAiFile({Trovato("CV.pdf")})

            Assert.HasCount(1, raccolta.Documenti, "resta solo quello che c'è")
            Assert.AreEqual("CV.pdf", raccolta.Documenti(0).Nome)
            Assert.AreEqual(0, mai, "e non c'era nessun file nuovo")
            Assert.IsEmpty(raccolta.Attestati(), "l'attestato sparito non si propone più")
        End Sub

        <TestMethod>
        Public Sub LaPropostaDellAiRiempieLeCategorie()
            Dim raccolta As RaccoltaDocumenti = ConDocumenti("CV_2025.pdf", "HACCP.pdf", "bolletta.pdf")

            raccolta.PrendiLaProposta(JsonNode.Parse(
                "{""cv_piu_recente"": ""CV_2025.pdf"", ""documenti"": [" &
                "{""nome"": ""CV_2025.pdf"", ""categoria"": ""cv"", ""motivo"": ""nome e contatti in testa""}," &
                "{""nome"": ""HACCP.pdf"", ""categoria"": ""attestato"", ""motivo"": ""rilasciato da un ente""}," &
                "{""nome"": ""bolletta.pdf"", ""categoria"": ""altro"", ""motivo"": ""non riguarda il lavoro""}]}"),
                {Trovato("CV_2025.pdf"), Trovato("HACCP.pdf"), Trovato("bolletta.pdf")})

            Assert.AreEqual(CategoriaDocumento.Cv, raccolta.Riconosciuto("CV_2025.pdf").Categoria, "il CV")
            Assert.AreEqual("CV_2025.pdf", raccolta.CvPiuRecente, "ed è anche il più recente")
            Assert.HasCount(1, raccolta.Attestati(), "un attestato da proporre")
            Assert.AreEqual("HACCP.pdf", raccolta.Attestati()(0).Nome)
            Assert.Contains("ente", raccolta.Riconosciuto("HACCP.pdf").Motivo, "col suo perché")
        End Sub

        <TestMethod>
        Public Sub UnaCorrezioneDellUtenteNonSiRiscrive()
            ' Rileggere la cartella serve a riconoscere i file nuovi, non a rimettere in
            ' discussione quel che una persona ha già deciso (cap. 05.2).
            Dim raccolta As RaccoltaDocumenti = ConDocumenti("attestato_senza_nome.pdf")

            raccolta.Documenti(0).Categoria = CategoriaDocumento.Attestato
            raccolta.Documenti(0).Corretto = True

            raccolta.PrendiLaProposta(JsonNode.Parse(
                "{""documenti"": [{""nome"": ""attestato_senza_nome.pdf"", ""categoria"": ""altro""," &
                """motivo"": ""l'assaggio non mostra abbastanza""}]}"),
                {Trovato("attestato_senza_nome.pdf")})

            Assert.AreEqual(CategoriaDocumento.Attestato, raccolta.Documenti(0).Categoria,
                            "vale quel che ha detto l'utente")
            Assert.HasCount(1, raccolta.Attestati(), "e resta fra gli allegabili")
        End Sub

        <TestMethod>
        Public Sub UnNomeCheNellaCartellaNonCEVieneScartato()
            ' Il prompt dice di non inventare nomi, ma non è su quella promessa che si
            ' regge la cosa: si classifica quel che c'è nella cartella, non quel che
            ' compare in una risposta.
            Dim raccolta As RaccoltaDocumenti = ConDocumenti("CV.pdf")

            raccolta.PrendiLaProposta(JsonNode.Parse(
                "{""cv_piu_recente"": ""CV_inventato.pdf"", ""documenti"": [" &
                "{""nome"": ""CV.pdf"", ""categoria"": ""cv"", ""motivo"": ""ok""}," &
                "{""nome"": ""attestato_inventato.pdf"", ""categoria"": ""attestato"", ""motivo"": ""ok""}]}"),
                {Trovato("CV.pdf")})

            Assert.HasCount(1, raccolta.Documenti, "l'elenco resta quello dei file veri")
            Assert.IsEmpty(raccolta.Attestati(), "niente attestati inventati da allegare")
            Assert.IsEmpty(raccolta.CvPiuRecente, "e nemmeno un CV più recente che non esiste")
        End Sub

        <TestMethod>
        Public Sub LaRaccoltaSiRilegge()
            Dim raccolta As RaccoltaDocumenti = ConDocumenti("CV.pdf", "HACCP.pdf")

            raccolta.Documenti(1).Categoria = CategoriaDocumento.Attestato
            raccolta.Documenti(1).Corretto = True
            raccolta.CvPiuRecente = "CV.pdf"
            raccolta.Letta = New Date(2026, 8, 14, 21, 30, 0)

            Dim riletta As RaccoltaDocumenti = RaccoltaDocumenti.DaJson(JsonNode.Parse(raccolta.ComeTesto()))

            Assert.AreEqual("C:\documenti", riletta.Cartella, "la cartella")
            Assert.AreEqual(New Date(2026, 8, 14, 21, 30, 0), riletta.Letta, "quando è stata letta")
            Assert.AreEqual("CV.pdf", riletta.CvPiuRecente, "il CV più recente")
            Assert.HasCount(2, riletta.Documenti, "i documenti")
            Assert.AreEqual(CategoriaDocumento.Attestato, riletta.Riconosciuto("HACCP.pdf").Categoria)
            Assert.IsTrue(riletta.Riconosciuto("HACCP.pdf").Corretto, "e che l'aveva detto l'utente")
        End Sub

        <TestMethod>
        Public Sub UnFileDiRaccoltaRottoNonFaCadereNiente()
            ' Senza cartella documenti l'applicazione funziona uguale: è una comodità, non
            ' un ingranaggio. Un file guasto vale «non ne ho una».
            Dim percorso As String = Path.Combine(Path.GetTempPath(), "raccolta-" & Guid.NewGuid().ToString("N"))

            Try
                File.WriteAllText(percorso, "{ questo non è JSON")

                Dim letta As RaccoltaDocumenti = RaccoltaDocumenti.Carica(percorso)

                Assert.IsNotNull(letta, "una raccolta c'è comunque")
                Assert.IsEmpty(letta.Documenti, "vuota")
                Assert.IsFalse(letta.CartellaUtilizzabile, "e senza cartella")
            Finally
                If File.Exists(percorso) Then File.Delete(percorso)
            End Try
        End Sub

        <TestMethod>
        Public Sub SenzaFileLaRaccoltaEVuotaENonEUnErrore()
            Dim mai As RaccoltaDocumenti = RaccoltaDocumenti.Carica(
                Path.Combine(Path.GetTempPath(), "raccolta-che-non-ce-" & Guid.NewGuid().ToString("N")))

            Assert.IsNotNull(mai)
            Assert.IsFalse(mai.CartellaUtilizzabile, "nessuna cartella scelta")
        End Sub

        <TestMethod>
        Public Sub UnaCartellaSparitaSiRiconosce()
            ' La cartella è dell'utente e sta dove vuole lui: un disco staccato o una
            ' cartella rinominata sono cose che capitano, e vanno dette invece di
            ' presentarsi come un elenco di allegati che non si aprono.
            Dim raccolta As New RaccoltaDocumenti With {
                .Cartella = Path.Combine(Path.GetTempPath(), "cartella-che-non-ce-" & Guid.NewGuid().ToString("N"))}

            Assert.IsFalse(raccolta.CartellaUtilizzabile, "non c'è")
            Assert.IsTrue(New RaccoltaDocumenti With {.Cartella = Path.GetTempPath()}.CartellaUtilizzabile,
                          "una che c'è invece sì")
        End Sub

        <TestMethod>
        Public Sub LArchivioScriveERilegge()
            Dim radice As String = Path.Combine(Path.GetTempPath(), "documenti-" & Guid.NewGuid().ToString("N"))
            Dim cartella As New CartellaDati(radice)

            Try
                Dim archivio As New ArchivioRaccoltaDocumenti(cartella)
                Assert.IsFalse(archivio.Esiste, "prima non c'è niente")

                Dim raccolta As RaccoltaDocumenti = ConDocumenti("HACCP.pdf")
                raccolta.Documenti(0).Categoria = CategoriaDocumento.Attestato
                archivio.Salva(raccolta)

                Assert.IsTrue(archivio.Esiste, "adesso il file c'è")
                Assert.HasCount(1, archivio.Carica().Attestati(), "e l'attestato torna")
                Assert.IsFalse(File.Exists(cartella.FileDocumenti & ".tmp"), "senza lasciare temporanei")
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        ''' <summary>
        ''' Una cartella vera con dentro dei file veri: qui non bastano i nomi, perché quel
        ''' che si collauda è proprio se il file c'è ancora.
        ''' </summary>
        Private Shared Sub ConCartellaVera(prova As Action(Of String))

            Dim cartella As String = Path.Combine(Path.GetTempPath(), "raccolta-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(cartella)

            Try
                prova(cartella)
            Finally
                CartelleDiProva.PortaVia(cartella)
            End Try

        End Sub

        ''' <summary>Una raccolta su una cartella vera, col suo CV già riconosciuto.</summary>
        Private Shared Function ConCvPiuRecente(cartella As String, nome As String) As RaccoltaDocumenti

            File.WriteAllText(Path.Combine(cartella, nome), "un curriculum qualunque")

            Dim raccolta As New RaccoltaDocumenti With {.Cartella = cartella}
            raccolta.AllineaAiFile({New FileTrovato With {.Nome = nome,
                                                          .Percorso = Path.Combine(cartella, nome)}})
            raccolta.Riconosciuto(nome).Categoria = CategoriaDocumento.Cv
            raccolta.CvPiuRecente = nome

            Return raccolta

        End Function

        <TestMethod>
        Public Sub IlCvPiuRecenteSiTrovaDaSolo()
            ' La porta «qui c'è tutto» del profilo (cap. 05.2): il CV che la classificazione
            ' ha indicato si ritrova col suo percorso intero, pronto per l'import, senza che
            ' nessuno debba ricercarlo fra i propri file.
            ConCartellaVera(
                Sub(cartella)
                    Dim raccolta As RaccoltaDocumenti = ConCvPiuRecente(cartella, "CV_2025.pdf")

                    Assert.AreEqual(Path.Combine(cartella, "CV_2025.pdf"),
                                    raccolta.PercorsoDelCvPiuRecente(), "il percorso intero")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnCvCancellatoDopoLaScansioneNonSiPropone()
            ' Qui non c'è nessun file copiato: fra la classificazione e oggi quel CV può
            ' essere stato spostato o buttato, e proporre di importare un file che non c'è
            ' più è peggio che non proporre niente.
            ConCartellaVera(
                Sub(cartella)
                    Dim raccolta As RaccoltaDocumenti = ConCvPiuRecente(cartella, "CV_2025.pdf")
                    File.Delete(Path.Combine(cartella, "CV_2025.pdf"))

                    Assert.IsNull(raccolta.PercorsoDelCvPiuRecente(), "il file non c'è più")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaUnCvRiconosciutoNonSiPropone()
            ' Due modi di non avere niente da proporre, e sono diversi: nessuno ha mai detto
            ' quale sia il più recente, oppure lo ha detto nominando un file che in questa
            ' raccolta non esiste — che è quel che succederebbe se il nome se lo inventasse
            ' una risposta dell'AI.
            ConCartellaVera(
                Sub(cartella)
                    Dim raccolta As RaccoltaDocumenti = ConCvPiuRecente(cartella, "CV_2025.pdf")

                    raccolta.CvPiuRecente = ""
                    Assert.IsNull(raccolta.PercorsoDelCvPiuRecente(), "nessuno ha detto quale")

                    raccolta.CvPiuRecente = "CV_inventato.pdf"
                    Assert.IsNull(raccolta.PercorsoDelCvPiuRecente(), "e un nome mai classificato non vale")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaLaCartellaNonCEPortaDaAprire()
            ' La cartella si sceglie una volta e resta registrata: se nel frattempo è stata
            ' spostata o staccata (una chiavetta), la porta si chiude in silenzio invece di
            ' proporre un percorso che non porta da nessuna parte.
            Dim raccolta As New RaccoltaDocumenti With {
                .Cartella = Path.Combine(Path.GetTempPath(), "cartella-che-non-ce-" & Guid.NewGuid().ToString("N")),
                .CvPiuRecente = "CV.pdf"}

            Assert.IsNull(raccolta.PercorsoDelCvPiuRecente(), "senza cartella non si propone niente")
        End Sub

    End Class

End Namespace
