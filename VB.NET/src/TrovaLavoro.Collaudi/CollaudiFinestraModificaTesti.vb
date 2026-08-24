Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Dati

Namespace Ui

    ''' <summary>
    ''' Collaudi della modifica a mano dei testi di P6 (T9d, cap. 08.4). Quello che qui
    ''' può rompersi davvero è dove finisce il testo: un campo riscritto che entrasse nella
    ''' voce sbagliata — o che entrasse nel documento pur avendo l'utente annullato —
    ''' sarebbe una bugia scritta nel file che poi si spedisce.
    ''' </summary>
    ''' <remarks>
    ''' Come le altre finestre modali, si costruisce e si interroga <b>senza mostrarla</b>:
    ''' per questo riscrivere, ripristinare e applicare hanno un metodo ciascuno, che è poi
    ''' quello che i controlli chiamano.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraModificaTesti

        Private Shared Function Cv() As JsonNode

            Return JsonNode.Parse(
                "{""tipo"": ""cv_mirato""," &
                """intestazione"": {""nome"": ""Luca Ferrari""}," &
                """sommario"": ""Ho esperienza nel servizio di sala.""," &
                """esperienze_professionali"": [" &
                "  {""ruolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino""," &
                "   ""descrizione"": ""Servizio ai tavoli""}," &
                "  {""ruolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A.""," &
                "   ""descrizione"": ""Carico e scarico merci""}]," &
                """competenze"": [""HACCP""]}")

        End Function

        Private Shared Function Lettera() As JsonNode

            Return JsonNode.Parse(
                "{""tipo"": ""lettera_mirata""," &
                """apertura"": ""Spettabile Azienda,""," &
                """corpo"": ""Mi candido perché ho esperienza di sala.""," &
                """chiusura"": ""Cordiali saluti,""}")

        End Function

        ' ==================================================================
        ' Le voci che si lasciano fuori dal documento (R6)
        ' ==================================================================

        <TestMethod>
        Public Sub UnaVoceSenzaProsaStaNellElencoMaNonFraITesti()

            ' Una competenza è un fatto: viene dal profilo e di qui non si riscrive. Ma
            ' nell'elenco c'è, perché da questo documento la si può lasciare fuori.
            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Assert.AreEqual(3, finestra.Quanti, "i testi restano il sommario e le due esperienze")
                Assert.HasCount(4, Elenco(finestra).Items, "ma a video c'è anche la competenza")
                Assert.AreEqual("Competenza 1", Elenco(finestra).Items(3).Text, "in fondo, dopo la prosa")

            End Using

        End Sub

        <TestMethod>
        Public Sub TogliereUnaVoceLaSpostaNellAltroElenco()

            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Assert.IsTrue(finestra.Togli(finestra.ImprontaDi("Competenza 1")),
                              "la competenza si può togliere")

                Assert.HasCount(3, Elenco(finestra).Items, "dall'elenco di sinistra è sparita")
                Assert.HasCount(1, Fuori(finestra).Items, "ed è comparsa in quello di destra")
                Assert.AreEqual("Competenza 1", Fuori(finestra).Items(0).Text, "ed è lei")
                Assert.Contains("competenze¦haccp", finestra.VociFuori(), "e la finestra lo dichiara a chi salva")

            End Using

        End Sub

        <TestMethod>
        Public Sub RimettereUnaVoceLaRiportaNelDocumento()

            ' L'andata e il ritorno: senza il ritorno, l'unico modo di rimettere una voce
            ' sarebbe rigenerare il documento, cioè perdere anche tutto il resto.
            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Dim quale As String = finestra.ImprontaDi("Competenza 1")
                finestra.Togli(quale)

                Assert.IsTrue(finestra.Rimetti(quale), "e si rimette")

                Assert.HasCount(4, Elenco(finestra).Items, "è tornata a sinistra")
                Assert.IsEmpty(Fuori(finestra).Items, "e a destra non c'è più niente")
                Assert.IsEmpty(finestra.VociFuori(), "chi salva non deve togliere niente")

            End Using

        End Sub

        <TestMethod>
        Public Sub IlSommarioNonSiToglie()

            ' Un CV senza sommario non è un CV con una voce in meno: è un CV rotto. Il
            ' sommario e il corpo della lettera si riscrivono, e basta.
            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Assert.AreEqual("Sommario", Elenco(finestra).Items(0).Text, "è proprio lui")
                Assert.IsNull(finestra.ImprontaDi("Sommario"), "non è una voce da togliere")
                Assert.IsFalse(finestra.Togli(finestra.ImprontaDi("Sommario")), "e non si toglie")
                Assert.IsEmpty(finestra.VociFuori(), "niente è uscito dal documento")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnaVoceCheInQuestoDocumentoNonCEsisteNonSiToglie()

            ' L'impronta è una stringa, e una stringa può arrivare da qualunque parte: da
            ' un altro documento, da un file scritto a mano, da un giro precedente. Se la
            ' finestra la accettasse senza guardare, l'elenco delle voci lasciate fuori si
            ' riempirebbe di fantasmi — voci che nessuno vede e che nessuno può rimettere,
            ' perché a video non compaiono da nessuna delle due parti.
            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Assert.IsFalse(finestra.Togli("competenze¦saldatura a filo continuo"),
                               "questa competenza in questo CV non c'è")
                Assert.IsEmpty(finestra.VociFuori(), "e non è finita nell'elenco di quelle tolte")

            End Using

        End Sub

        <TestMethod>
        Public Sub QuelCheEraGiaFuoriSiRitrovaFuori()

            ' Chi riapre la finestra deve ritrovare il taglio che aveva scelto: se le voci
            ' tolte tornassero dentro a ogni apertura, la memoria su disco non servirebbe
            ' a niente e il lavoro andrebbe rifatto ogni volta.
            Dim tolte As New VociTolte()
            tolte.Togli("competenze¦haccp", New Date(2026, 8, 24))

            Dim documenti As New List(Of DocumentoDaRiscrivere) From {
                New DocumentoDaRiscrivere With {.Documento = Cv(), .Tolte = tolte}}

            Using finestra As New FinestraModificaTesti(documenti)

                Assert.HasCount(3, Elenco(finestra).Items, "a sinistra la competenza non c'è")
                Assert.HasCount(1, Fuori(finestra).Items, "sta di là, dov'era stata messa")
                Assert.Contains("competenze¦haccp", finestra.VociFuori(), "e ci resta se non si tocca niente")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnEsperienzaSiToglieERiscriveDallaStessaRiga()

            ' Le due cose che si fanno qui dentro non fanno due righe: «Esperienza 1» è
            ' una sola, e porta con sé il suo testo e la sua impronta.
            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Assert.AreEqual("Esperienza 1", Elenco(finestra).Items(1).Text)
                Assert.IsTrue(finestra.Riscrivi(1, "Servizio ai tavoli e alla cassa."),
                              "la sua prosa si riscrive")
                Assert.IsTrue(finestra.Togli(finestra.ImprontaDi("Esperienza 1")),
                              "e la sua voce si toglie")

                Assert.HasCount(3, Elenco(finestra).Items, "una riga in meno a sinistra")
                Assert.Contains("esperienze_professionali¦cameriere¦trattoria da gino",
                                finestra.VociFuori(), "ed è quella tolta")

            End Using

        End Sub

        Private Shared Function Fuori(finestra As Control) As ListView
            Return DirectCast(finestra.Controls.Find("lvwFuori", searchAllChildren:=True).Single(), ListView)
        End Function

        ''' <summary>I documenti da riscrivere, col «prima» che si vuole dare a ciascuno.</summary>
        Private Shared Function Aperti(ParamArray documenti As JsonNode()) As List(Of DocumentoDaRiscrivere)

            Return documenti.Select(
                Function(d) New DocumentoDaRiscrivere With {.Documento = d}).ToList()

        End Function

        Private Shared Function Descrizione(documento As JsonNode, indice As Integer) As String
            Return documento("esperienze_professionali")(indice)("descrizione").GetValue(Of String)()
        End Function

        Private Shared Function Testo(documento As JsonNode, campo As String) As String
            Return documento(campo).GetValue(Of String)()
        End Function

        Private Shared Function Elenco(finestra As Control) As ListView
            Return DirectCast(finestra.Controls.Find("lvwCampi", searchAllChildren:=True).Single(), ListView)
        End Function

        <TestMethod>
        Public Sub IDueDocumentiDiUnaCandidaturaStannoInUnElencoSolo()

            ' Il CV e la lettera si riscrivono nello stesso posto: l'etichetta dice già di
            ' quale documento è ogni campo, e due finestre per un gesto solo sarebbero due
            ' conferme da dare.
            Using finestra As New FinestraModificaTesti(Aperti(Cv(), Lettera()))

                Assert.AreEqual(4, finestra.Quanti, "sommario, due esperienze e il corpo")

                Assert.AreEqual("Sommario", finestra.Etichetta(0), "prima il CV")
                Assert.AreEqual("Esperienza 1", finestra.Etichetta(1))
                Assert.AreEqual("Esperienza 2", finestra.Etichetta(2))
                Assert.AreEqual("Corpo della lettera", finestra.Etichetta(3), "poi la lettera")

                ' A video le righe sono cinque, non quattro: da R6 (2026-08-24) l'elenco
                ' mostra anche le voci che si possono lasciare fuori dal documento, e la
                ' competenza «HACCP» è una di quelle — si toglie, non si riscrive, e
                ' infatti fra i campi di prosa non compare.
                Assert.HasCount(5, Elenco(finestra).Items,
                                "i quattro testi più la competenza, che è una voce togliibile")

            End Using

        End Sub

        <TestMethod>
        Public Sub RiscrivereNonToccaIlDocumentoFinoAlSalva()

            ' È la promessa dell'«Annulla»: quello che si è scritto muore con la finestra.
            Dim documento As JsonNode = Cv()

            Using finestra As New FinestraModificaTesti(Aperti(documento))

                Assert.IsTrue(finestra.Riscrivi(0, "L'ho riscritto io."), "riscritto in finestra")
                Assert.AreEqual("L'ho riscritto io.", finestra.Testo(0), "e la finestra lo mostra")

                Assert.AreEqual("Ho esperienza nel servizio di sala.", Testo(documento, "sommario"),
                                "ma il documento è ancora quello di prima")

            End Using

        End Sub

        <TestMethod>
        Public Sub ApplicaMetteNelDocumentoSoloICampiCambiati()

            Dim documento As JsonNode = Cv()

            Using finestra As New FinestraModificaTesti(Aperti(documento))

                finestra.Riscrivi(2, "L'ho riscritta io.")

                Assert.AreEqual(1, finestra.Applica().Count, "uno solo è stato toccato")

                Assert.AreEqual("L'ho riscritta io.", Descrizione(documento, 1), "ed è finito nella voce giusta")
                Assert.AreEqual("Servizio ai tavoli", Descrizione(documento, 0), "l'altra esperienza è intatta")
                Assert.AreEqual("Ho esperienza nel servizio di sala.", Testo(documento, "sommario"),
                                "e il sommario pure")

            End Using

        End Sub

        <TestMethod>
        Public Sub RiscrivereConLoStessoTestoNonContaComeUnaModifica()

            ' Chi apre, guarda e chiude senza cambiare niente non ha modificato niente: un
            ' documento «modificato» da nessuno si farebbe risalvare a ogni visita.
            Dim documento As JsonNode = Cv()

            Using finestra As New FinestraModificaTesti(Aperti(documento))

                finestra.Riscrivi(0, "Ho esperienza nel servizio di sala.")

                Assert.AreEqual(0, finestra.Applica().Count, "niente da scrivere")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnaCasellaSvuotataNonCancellaIlTesto()

            Dim documento As JsonNode = Cv()

            Using finestra As New FinestraModificaTesti(Aperti(documento))

                Assert.IsFalse(finestra.Riscrivi(0, "   "), "il vuoto si rifiuta")
                Assert.AreEqual("Ho esperienza nel servizio di sala.", finestra.Testo(0),
                                "e nella finestra resta quello che c'era")

                Assert.AreEqual(0, finestra.Applica().Count, "niente è cambiato")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnDocumentoSenzaProsaNonPortaRighe()

            ' Un CV tutto fatti — nome, competenze, titoli — non ha niente da riscrivere: la
            ' finestra non lo nega, semplicemente non ha campi da mostrare, ed è il pannello
            ' a non aprirla affatto.
            Dim soloFatti As JsonNode = JsonNode.Parse(
                "{""tipo"": ""cv_base"", ""intestazione"": {""nome"": ""Luca Ferrari""}," &
                """competenze"": [""HACCP""]}")

            Using finestra As New FinestraModificaTesti(Aperti(soloFatti))

                Assert.AreEqual(0, finestra.Quanti, "nessun campo di prosa")
                Assert.AreEqual(0, finestra.Applica().Count, "e niente da applicare")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnaRigaCheNonCEsisteNonFaDanno()

            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Assert.IsFalse(finestra.Riscrivi(-1, "Prima della prima."), "prima del primo")
                Assert.IsFalse(finestra.Riscrivi(9, "Dopo l'ultima."), "dopo l'ultimo")
                Assert.AreEqual(String.Empty, finestra.Etichetta(9), "né si legge il nome di una riga che non c'è")

            End Using

        End Sub

    End Class

End Namespace
