Imports System.IO
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi della lettura della cartella documenti (cap. 05.2, passi 1 e 2): quali
    ''' file si guardano, fin dove si scende, e quanto testo se ne assaggia.
    ''' </summary>
    ''' <remarks>
    ''' Qui non si chiama nessuno: la scansione è tutta disco, ed è quel che la rende
    ''' ripetibile. La cartella di prova si costruisce ogni volta e si butta.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiScansioneDocumenti

        Private _cartella As String

        <TestInitialize>
        Public Sub Prepara()
            _cartella = Path.Combine(Path.GetTempPath(), "scansione-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(_cartella)
        End Sub

        <TestCleanup>
        Public Sub Pulisci()
            If Directory.Exists(_cartella) Then Directory.Delete(_cartella, recursive:=True)
        End Sub

        Private Sub Scrivi(nome As String, contenuto As String)

            Dim percorso As String = Path.Combine(_cartella, nome)
            Directory.CreateDirectory(Path.GetDirectoryName(percorso))
            File.WriteAllText(percorso, contenuto)

        End Sub

        Private Function Leggi() As List(Of FileTrovato)
            Dim fuori As Integer
            Return ScansioneDocumenti.Leggi(_cartella, fuori)
        End Function

        <TestMethod>
        Public Sub SiGuardanoSoloIFormatiCheSiSannoLeggere()
            ' Cap. 05.1: PDF, DOCX, TXT, MD. Una foto o una busta paga in .xlsx non si
            ' possono né leggere né riconoscere, e proporle sarebbe rumore.
            Scrivi("CV.txt", "Mario Rossi")
            Scrivi("appunti.md", "# note")
            Scrivi("foto.jpg", "non un documento")
            Scrivi("conti.xlsx", "nemmeno")

            Dim trovati As List(Of FileTrovato) = Leggi()

            Assert.HasCount(2, trovati, "solo i due leggibili")
            Assert.IsTrue(trovati.Any(Function(f) f.Nome = "CV.txt"), "il txt")
            Assert.IsTrue(trovati.Any(Function(f) f.Nome = "appunti.md"), "il md")
        End Sub

        <TestMethod>
        Public Sub SiScendeDiUnLivelloSolo()
            ' Cap. 05.2: sottocartelle di primo livello. Scendere all'infinito vorrebbe
            ' dire proporre a un'azienda un file pescato chissà dove.
            Scrivi("CV.txt", "Mario Rossi")
            Scrivi(Path.Combine("attestati", "haccp.txt"), "attestato")
            Scrivi(Path.Combine("attestati", "vecchi", "2011.txt"), "troppo in fondo")

            Dim trovati As List(Of FileTrovato) = Leggi()

            Assert.HasCount(2, trovati, "la cartella e le sue figlie, non le nipoti")
            Assert.IsTrue(trovati.Any(Function(f) f.Nome = Path.Combine("attestati", "haccp.txt")),
                          "il nome dice anche in quale sottocartella sta")
        End Sub

        <TestMethod>
        Public Sub DelTestoSiAssaggianoLePrimeRighe()
            Scrivi("lettera.txt", New String("a"c, ScansioneDocumenti.CaratteriAssaggio * 2))
            Scrivi("corto.md", "poche parole")

            Dim trovati As List(Of FileTrovato) = Leggi()
            Dim lungo As FileTrovato = trovati.Single(Function(f) f.Nome = "lettera.txt")
            Dim corto As FileTrovato = trovati.Single(Function(f) f.Nome = "corto.md")

            Assert.AreEqual(ScansioneDocumenti.CaratteriAssaggio + 1, lungo.Assaggio.Length,
                            "tagliato, con i puntini che dicono che continua")
            Assert.EndsWith("…", lungo.Assaggio, "e si vede che è tagliato")
            Assert.AreEqual("poche parole", corto.Assaggio, "quello corto arriva intero")
        End Sub

        <TestMethod>
        Public Sub DeiPdfNonSiAssaggiaNiente()
            ' Il disco non basta a leggerli: servirebbe una trascrizione dell'AI per
            ' ciascuno (cap. 05.1), e in una cartella di documenti i PDF sono quasi tutto.
            ' Il prompt lo sa e giudica sul nome.
            Scrivi("Attestato_HACCP_2019.pdf", "%PDF-1.4 roba binaria")

            Dim trovato As FileTrovato = Leggi().Single()

            Assert.AreEqual("Attestato_HACCP_2019.pdf", trovato.Nome, "il file c'è")
            Assert.IsNull(trovato.Assaggio, "ma senza assaggio")
            Assert.IsGreaterThan(0, trovato.Dimensione, "la dimensione però si sa")
        End Sub

        <TestMethod>
        Public Sub OltreIlTettoQuelCheRestaFuoriSiDice()
            ' Un elenco troncato in silenzio si legge come «nella cartella non c'era
            ' altro»: chi ha duecento file deve sapere quanti ne ho guardati.
            For indice As Integer = 1 To ScansioneDocumenti.MassimoFile + 5
                Scrivi($"documento-{indice:000}.txt", "roba")
            Next

            Dim fuori As Integer
            Dim trovati As List(Of FileTrovato) = ScansioneDocumenti.Leggi(_cartella, fuori)

            Assert.HasCount(ScansioneDocumenti.MassimoFile, trovati, "si guardano i primi")
            Assert.AreEqual(5, fuori, "e si dice quanti sono rimasti fuori")
        End Sub

        <TestMethod>
        Public Sub UnaCartellaCheNonCENonEUnGuasto()
            Dim fuori As Integer

            Assert.IsEmpty(ScansioneDocumenti.Leggi(
                Path.Combine(Path.GetTempPath(), "mai-" & Guid.NewGuid().ToString("N")), fuori),
                "niente da leggere")
            Assert.AreEqual(0, fuori, "e niente da dire")
            Assert.IsEmpty(ScansioneDocumenti.Leggi(Nothing, fuori), "nemmeno senza cartella")
        End Sub

        <TestMethod>
        Public Sub IFileSiLegonoInOrdineDiNome()
            ' L'ordine è quello che l'utente vede nella sua cartella: è così che ritrova
            ' quel che cerca nell'elenco, e così che il tetto taglia in modo prevedibile.
            Scrivi("zeta.txt", "z")
            Scrivi("alfa.txt", "a")
            Scrivi("Mezzo.txt", "m")

            Dim nomi As List(Of String) = Leggi().Select(Function(f) f.Nome).ToList()

            Assert.AreEqual("alfa.txt", nomi(0))
            Assert.AreEqual("Mezzo.txt", nomi(1), "le maiuscole non fanno un ordine a parte")
            Assert.AreEqual("zeta.txt", nomi(2))
        End Sub

    End Class

End Namespace
