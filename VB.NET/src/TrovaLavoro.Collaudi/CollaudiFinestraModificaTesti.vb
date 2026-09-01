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

        ' ==================================================================
        ' La scelta resta dov'era (2026-08-24)
        ' ==================================================================

        ''' <summary>
        ''' Sceglie una riga, come farebbe un clic.
        ''' </summary>
        ''' <remarks>
        ''' La riga scelta prima si toglie a mano: i due elenchi hanno
        ''' <c>MultiSelect = False</c> e nell'applicazione ci pensa Windows, ma su una
        ''' finestra mai mostrata quel controllo non c'è ancora e resterebbero scelte in
        ''' due. Qui si fa quel che farebbe lui, non qualcosa di diverso.
        ''' </remarks>
        Private Shared Sub Scegli(elenco As ListView, riga As Integer)

            For Each voce As ListViewItem In elenco.Items
                voce.Selected = False
            Next

            elenco.Items(riga).Selected = True

        End Sub

        ''' <summary>
        ''' Come si chiama la riga scelta, o vuoto se non è scelto niente.
        ''' </summary>
        ''' <remarks>
        ''' Si scorrono le righe invece di chiedere <c>SelectedItems</c> per la stessa
        ''' ragione per cui lo fa la finestra: su un elenco mai nato quella scorciatoia
        ''' risponde «niente scelto» qualunque cosa sia scelta.
        ''' </remarks>
        Private Shared Function Scelta(elenco As ListView) As String

            For Each riga As ListViewItem In elenco.Items
                If riga.Selected Then Return riga.Text
            Next

            Return String.Empty

        End Function

        ''' <summary>
        ''' Togliere una voce rifà gli elenchi, e una ricostruzione non ha memoria: la
        ''' scelta ripartiva dalla prima riga, così chi toglieva la sesta voce di dieci si
        ''' ritrovava in cima, e chi ne toglieva tre di fila doveva ricercare il punto ogni
        ''' volta.
        ''' </summary>
        <TestMethod>
        Public Sub TogliendoUnaVoceLaSceltaNonTornaInCima()

            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Scegli(Elenco(finestra), 2)

                Assert.IsTrue(finestra.Togli(finestra.ImprontaDi("Esperienza 2")), "la si toglie")

                Assert.AreEqual("Competenza 1", Scelta(Elenco(finestra)),
                                "la scelta è su chi ha preso quel posto, non sul sommario in cima")

            End Using

        End Sub

        <TestMethod>
        Public Sub TogliendoLUltimaVoceLaSceltaCadeSullUltimaRimasta()

            ' Il posto che quella riga occupava adesso non esiste più: si scende di uno,
            ' invece di risalire in cima.
            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Scegli(Elenco(finestra), 3)

                Assert.IsTrue(finestra.Togli(finestra.ImprontaDi("Competenza 1")), "la si toglie")

                Assert.AreEqual("Esperienza 2", Scelta(Elenco(finestra)), "l'ultima rimasta")

            End Using

        End Sub

        <TestMethod>
        Public Sub RimettendoUnaVoceLaSceltaADestraNonTornaInCima()

            ' Lo stesso, dall'altra parte: chi rimette dentro le voci una a una lavora
            ' nell'elenco di destra, e anche lì la fila si accorcia sotto le sue mani.
            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                finestra.Togli(finestra.ImprontaDi("Esperienza 1"))
                finestra.Togli(finestra.ImprontaDi("Esperienza 2"))
                finestra.Togli(finestra.ImprontaDi("Competenza 1"))

                Scegli(Fuori(finestra), 1)

                Assert.IsTrue(finestra.Rimetti(finestra.ImprontaDi("Esperienza 2")), "la si rimette")

                Assert.AreEqual("Competenza 1", Scelta(Fuori(finestra)),
                                "a destra la scelta è su chi ha preso quel posto")

            End Using

        End Sub

        <TestMethod>
        Public Sub LaRigaSiRitrovaPerIdentitaENonPerPosizione()

            ' Una voce che rientra sposta tutte quelle che vengono dopo: cercare la riga
            ' scelta al numero in cui stava riporterebbe su un'altra voce, con lo stesso
            ' aplomb di una scelta giusta.
            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                finestra.Togli(finestra.ImprontaDi("Esperienza 1"))

                ' Adesso a sinistra ci sono Sommario, Esperienza 2, Competenza 1.
                Scegli(Elenco(finestra), 1)
                Assert.AreEqual("Esperienza 2", Scelta(Elenco(finestra)), "è lei che si sta guardando")

                Assert.IsTrue(finestra.Rimetti(finestra.ImprontaDi("Esperienza 1")), "l'altra rientra")

                Assert.AreEqual("Esperienza 2", Scelta(Elenco(finestra)),
                                "l'ha spostata di un posto, ma la riga scelta è sempre la sua")

            End Using

        End Sub

        ' ==================================================================
        ' Il segno ✎: chi ha scritto questo testo (R7)
        ' ==================================================================

        ''' <summary>Un documento che si riapre con dei campi già riscritti a mano (R7).</summary>
        Private Shared Function ConRiscritture(documento As JsonNode,
                                               ParamArray campi As String()) As List(Of DocumentoDaRiscrivere)

            Dim riscritte As New RiscrittureAMano

            For Each id As String In campi
                riscritte.Annota(id, New Date(2026, 8, 23, 18, 40, 0))
            Next

            Return New List(Of DocumentoDaRiscrivere) From {
                New DocumentoDaRiscrivere With {.Documento = documento, .Riscritte = riscritte}}

        End Function

        ''' <summary>Il segno nella terza colonna di una riga: «✎», o niente.</summary>
        Private Shared Function Segno(elenco As ListView, riga As Integer) As String

            Return elenco.Items(riga).SubItems(2).Text

        End Function

        ''' <summary>
        ''' Il ✎ valeva per «riscritto in questo giro», e riaprendo la finestra spariva: di
        ''' un testo scritto dall'utente il giorno prima l'elenco diceva che non l'aveva
        ''' mai toccato, mentre l'avviso di «Rigenera» — che i file li legge — continuava a
        ''' promettere che quel testo si sarebbe perso. Due risposte alla stessa domanda.
        ''' </summary>
        <TestMethod>
        Public Sub IlSegnoRestaSuUnTestoRiscrittoInUnAltroGiro()

            Using finestra As New FinestraModificaTesti(ConRiscritture(Cv(), "sommario"))

                Assert.AreEqual("✎", Segno(Elenco(finestra), 0), "il sommario l'ha scritto l'utente")
                Assert.AreEqual(String.Empty, Segno(Elenco(finestra), 1), "l'esperienza no")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnTestoGiaRiscrittoNonSiRimetteNelDocumentoDaSolo()

            ' Il segno dice chi ha scritto quel testo; a decidere cosa torna nel documento
            ' resta quel che è cambiato adesso. Nel file quel testo c'è già, e riscriverlo
            ' farebbe contare come «modificato» un documento che nessuno ha toccato.
            Using finestra As New FinestraModificaTesti(ConRiscritture(Cv(), "sommario"))

                Assert.IsEmpty(finestra.Applica(), "niente da rimettere")

            End Using

        End Sub

        <TestMethod>
        Public Sub SenzaAnnotazioniIlSegnoCompareSoloDopoLaRiscrittura()

            ' Un documento che nessuno ha mai toccato a mano non si porta dietro segni: le
            ' candidature scritte prima di R7 si riaprono così, e vale come «mai toccate».
            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Assert.AreEqual(String.Empty, Segno(Elenco(finestra), 0),
                                "all'apertura non l'ha riscritto nessuno")

                Assert.IsTrue(finestra.Riscrivi(0, "L'ho riscritto io."), "lo riscrive adesso")

                Assert.AreEqual("✎", Segno(Elenco(finestra), 0), "e il segno compare")

            End Using

        End Sub

        <TestMethod>
        Public Sub QuandoNonCiStaSiScorreInveceDiTagliare()

            ' A 150% i testi crescono e la finestra cresce con loro, ma non oltre lo
            ' spazio che c'è: il tetto e lo scorrimento vanno insieme, o quel che resta
            ' fuori cade fuori dalla finestra e nessuno spostamento lo recupera
            ' (decisione 15.7).
            Using finestra As New FinestraModificaTesti(Aperti(Cv(), Lettera()))

                finestra.DisponiIn(200)

                Assert.IsTrue(finestra.AutoScroll, "con questo spazio si scorre")
                Assert.IsLessThanOrEqualTo(200, finestra.ClientSize.Height,
                                           "e la finestra sta nello spazio che c'è")

            End Using

        End Sub

        <TestMethod>
        Public Sub LElencoDiDestraArrivaFinoAlMargine()

            ' I due elenchi si spartiscono la larghezza della finestra invece di stare su
            ' due costanti: è quel che a DPI alti mandava quello di destra oltre il bordo.
            Using finestra As New FinestraModificaTesti(Aperti(Cv(), Lettera()))

                finestra.DisponiIn(4000)

                Assert.IsFalse(finestra.AutoScroll, "con tutto questo spazio non si scorre")
                Assert.AreEqual(finestra.ClientSize.Width - StileApp.MargineRiquadro,
                                Fuori(finestra).Bounds.Right, "e l'elenco di destra finisce al margine")

            End Using

        End Sub

    End Class

End Namespace
