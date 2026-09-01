Imports System.IO
Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore
Imports TrovaLavoro.Web

Namespace Ui

    ''' <summary>
    ''' Collaudi del pannello P3 (cap. 06.2). Girano <b>senza browser</b>: il pannello si
    ''' collega senza motore, quindi la WebView non si accende e non serve né WebView2 né
    ''' un thread STA. Quello che si guarda qui è tutto ciò che sta <i>attorno</i> al
    ''' browser — i menù, la composizione della ricerca, il salvataggio su disco, lo stato
    ''' dei comandi — che è anche la parte che un collaudo può giudicare davvero.
    ''' </summary>
    ''' <remarks>
    ''' La navigazione vera ha il suo banco altrove (<c>CollaudiMotoreBrowser</c>, categoria
    ''' «Reale»): lì si accende una vista e si guarda se regge. Qui si verifica che al
    ''' browser arrivi l'indirizzo giusto — e che senza browser il pannello non caschi.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiPannelloRicerca

        ' Il nome del pannello dove si incolla un annuncio a mano vive in un posto solo,
        ' NomiUi.Confronto (cap. 03.4), e di là pescano sia il bottone della barra sia i
        ' messaggi che ce lo mandano. Quello che si guarda qui non è che la costante sia
        ' sé stessa — sarebbe una tautologia — ma che i messaggi di P3 continuino a
        ' pescarla invece di riscriversi il nome a mano: un letterale sbagliato in un
        ' messaggio è un utente che cerca un bottone che non esiste. Che il bottone vero
        ' porti davvero quel nome lo guarda CollaudiBarraDiNavigazione, dall'altro capo
        ' dello stesso anello.
        '
        ' Fino al 2026-08-30 l'atteso era una copia scritta qui, e furono proprio questi
        ' due collaudi a diventare rossi quando il bottone passò da «📋 Candidatura» al
        ' nome di oggi: è la storia che ha fatto nascere NomiUi.

        ' ==================================================================
        ' I menù e lo stato d'ingresso
        ' ==================================================================

        <TestMethod>
        Public Sub IMenuNasconoDaiPortaliEDalleRicercheSalvate()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    Dim portali As ComboBox = Menu(pannello, "cboPortali")
                    CollectionAssert.AreEqual(
                        contesto.Ricerche.Portali.Select(Function(p) p.Nome).ToArray(),
                        portali.Items.Cast(Of String)().ToArray(),
                        "i portali del contesto, nel loro ordine")
                    Assert.AreEqual(0, portali.SelectedIndex, "col primo già scelto")

                    ' Al primo avvio non c'è nessuna ricerca salvata: il menù e i suoi due
                    ' comandi restano spenti invece di stare accesi su niente.
                    Assert.IsEmpty(Menu(pannello, "cboSalvate").Items)
                    Assert.IsFalse(Menu(pannello, "cboSalvate").Enabled)
                    Assert.IsFalse(Bottone(pannello, "btnApri").Enabled)
                    Assert.IsFalse(Bottone(pannello, "btnDimentica").Enabled)

                    ' Cercare invece si può subito.
                    Assert.IsTrue(Bottone(pannello, "btnCerca").Enabled)
                    Assert.IsTrue(Bottone(pannello, "btnVai").Enabled)

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SenzaUnaPaginaDaLeggereLaCatturaRestaSpenta()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    ' Il pannello è collegato senza browser: non c'è nessuna pagina, e un
                    ' bottone che risponderebbe «non c'è niente da leggere» è peggio di un
                    ' bottone spento.
                    Assert.IsFalse(Bottone(pannello, "btnCattura").Enabled)

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IComandiDellaNavigazioneDiconoIlProprioNome()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    ' Due portano un simbolo al posto del testo e la casella dell'indirizzo
                    ' non ha un'etichetta accanto: senza un nome accessibile sarebbero
                    ' anonimi per chi non vede lo schermo. Il difetto è emerso guardando
                    ' l'applicazione vera (2026-08-12), dove lo strumento di collaudo non
                    ' riusciva a trovare la casella — e uno screen reader nemmeno.
                    Assert.AreEqual("Indietro", Bottone(pannello, "btnIndietro").AccessibleName)
                    Assert.AreEqual("Ricarica", Bottone(pannello, "btnRicarica").AccessibleName)
                    Assert.AreEqual("Indirizzo", Casella(pannello, "txtIndirizzo").AccessibleName)

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IComandiDelBrowserNonSiAccendonoSenzaBrowser()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    ' Il pannello è collegato senza motore: la vista non è accesa, e i due
                    ' comandi che hanno senso solo su una pagina aperta restano spenti.
                    Assert.IsFalse(Bottone(pannello, "btnIndietro").Enabled)
                    Assert.IsFalse(Bottone(pannello, "btnRicarica").Enabled)

                End Sub)

        End Sub

        ' ==================================================================
        ' Salvare e ritrovare una ricerca
        ' ==================================================================

        <TestMethod>
        Public Sub SalvareUnaRicercaLaScriveSuDiscoELaPortaNelMenu()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    Menu(pannello, "cboPortali").SelectedItem = "Indeed"
                    Casella(pannello, "txtCosa").Text = "perito elettronico"
                    Casella(pannello, "txtDove").Text = "Genova"

                    Bottone(pannello, "btnSalvaRicerca").PerformClick()

                    ' Nel menù, scelta.
                    Dim salvate As ComboBox = Menu(pannello, "cboSalvate")
                    Assert.HasCount(1, salvate.Items)
                    Assert.AreEqual("Indeed — perito elettronico, Genova",
                                    salvate.SelectedItem.ToString(),
                                    "il nome proposto dice portale, cosa e dove")
                    Assert.IsTrue(Bottone(pannello, "btnApri").Enabled, "e ora si può aprire")

                    ' Su disco, davvero: è ciò che la fa sopravvivere alla chiusura.
                    Assert.IsTrue(File.Exists(cartella.FileRicerche))
                    Assert.Contains("perito elettronico",
                                    File.ReadAllText(cartella.FileRicerche, Text.Encoding.UTF8))

                    ' E rileggendo il file da zero la ricerca c'è ancora.
                    Assert.HasCount(1, New ArchivioRicerche(cartella).Carica().Salvate)

                    Assert.Contains("salvata", Etichetta(pannello, "lblStatoRicerca").Text,
                                    "il pannello lo racconta")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SalvareDueVolteLaStessaRicercaNonRaddoppiaIlMenu()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    Menu(pannello, "cboPortali").SelectedItem = "Subito.it"
                    Casella(pannello, "txtCosa").Text = "magazziniere"
                    Casella(pannello, "txtDove").Text = "Chiavari"

                    Bottone(pannello, "btnSalvaRicerca").PerformClick()
                    Bottone(pannello, "btnSalvaRicerca").PerformClick()

                    Assert.HasCount(1, Menu(pannello, "cboSalvate").Items,
                                    "il nome proposto è prevedibile: la seconda sostituisce la prima")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaRicercaVuotaNonSiSalva()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    Bottone(pannello, "btnSalvaRicerca").PerformClick()

                    Assert.IsEmpty(Menu(pannello, "cboSalvate").Items)
                    Assert.IsFalse(File.Exists(cartella.FileRicerche),
                                   "e non si scrive niente su disco")
                    Assert.IsNotEmpty(Etichetta(pannello, "lblStatoRicerca").Text,
                                      "l'utente sa perché non è successo niente")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub AprireUnaRicercaSalvataRiportaISuoiValoriNelleCaselle()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    ' Una ricerca già in casa, come dopo un riavvio.
                    contesto.Ricerche.MettiDaParte(New RicercaSalvata With {
                        .Nome = "Muletto a Chiavari", .Portale = "Subito.it",
                        .Cosa = "magazziniere muletto", .Dove = "Chiavari"})

                    Dim pannelloDiRitorno As New PannelloRicerca()
                    Try
                        pannelloDiRitorno.Collega(contesto)

                        Assert.AreEqual("Muletto a Chiavari",
                                        Menu(pannelloDiRitorno, "cboSalvate").SelectedItem.ToString(),
                                        "riaprendo, la ricerca è già scelta")

                        Bottone(pannelloDiRitorno, "btnApri").PerformClick()

                        ' Senza browser non si naviga, ma i valori tornano nelle caselle:
                        ' è da lì che nasce la ricerca successiva, ritoccando questa.
                        Assert.AreEqual("Subito.it",
                                        Menu(pannelloDiRitorno, "cboPortali").SelectedItem.ToString())
                        Assert.AreEqual("magazziniere muletto",
                                        Casella(pannelloDiRitorno, "txtCosa").Text)
                        Assert.AreEqual("Chiavari", Casella(pannelloDiRitorno, "txtDove").Text)

                    Finally
                        pannelloDiRitorno.Dispose()
                    End Try

                End Sub)

        End Sub

        ' ==================================================================
        ' Le misure e i livelli della fascia dei comandi (cap. 03.2, 03.3)
        ' ==================================================================

        <TestMethod>
        Public Sub IComandiDellaRicercaSonoAltiComeGliAltriDellApplicazione()

            ' Fino al 2026-09-01 erano alti 26 pixel: gli unici sette bottoni fuori dal
            ' token in tutta l'applicazione, in mezzo a una fascia di caselle alte 23 e
            ' sotto due bottoni alti 32. L'atteso si prende da un bottone del pannello
            ' stesso e non dal numero 32, perché con AutoScaleMode.Font le misure crescono
            ' col carattere: quel che deve valere è che crescano <b>insieme</b>.
            Using pannello As New PannelloRicerca()

                Dim atteso As Integer = Bottone(pannello, "btnCattura").Height

                Assert.IsGreaterThanOrEqualTo(StileApp.BottoneStandard.Height, atteso,
                                              "il metro è il bottone standard del progetto")

                For Each nome As String In {"btnApri", "btnDimentica", "btnCerca", "btnSalvaRicerca",
                                            "btnIndietro", "btnRicarica", "btnVai"}
                    Assert.AreEqual(atteso, Bottone(pannello, nome).Height,
                                    $"«{nome}» è alto quanto gli altri comandi del pannello")
                Next

            End Using

        End Sub

        <TestMethod>
        Public Sub LeTreRigheDellaFasciaNonSiPestanoIPiedi()

            ' La proprietà che i sei pixel in più a bottone mettevano a rischio: fra una
            ' riga e l'altra ci deve restare almeno l'interlinea minima (cap. 03.2), e
            ' tutto deve stare dentro la fascia. Le righe si dichiarano qui perché è la
            ' sola cosa che il designer sa e il collaudo no.
            Dim righe As String()() = {
                New String() {"lblSalvate", "cboSalvate", "btnApri", "btnDimentica"},
                New String() {"lblPortale", "cboPortali", "lblCosa", "txtCosa",
                              "lblDove", "txtDove", "btnCerca", "btnSalvaRicerca"},
                New String() {"btnIndietro", "btnRicarica", "txtIndirizzo", "btnVai"}}

            Using pannello As New PannelloRicerca()

                Dim fascia As Panel = DirectCast(
                    pannello.Controls.Find("pnlComandi", searchAllChildren:=True).Single(), Panel)

                Dim cime As New List(Of Integer)
                Dim fondi As New List(Of Integer)

                For Each riga As String() In righe
                    Dim controlli As Control() = riga.Select(
                        Function(nome) fascia.Controls.Find(nome, searchAllChildren:=True).Single()).ToArray()

                    cime.Add(controlli.Min(Function(c) c.Top))
                    fondi.Add(controlli.Max(Function(c) c.Bottom))
                Next

                Assert.IsGreaterThanOrEqualTo(0, cime.First(), "la prima riga non esce dalla fascia in alto")

                For riga As Integer = 0 To righe.Length - 2
                    Assert.IsGreaterThanOrEqualTo(fondi(riga) + StileApp.InterlineaMinima, cime(riga + 1),
                                                  $"fra la riga {riga + 1} e la {riga + 2} manca l'interlinea")
                Next

                Assert.IsGreaterThanOrEqualTo(fondi.Last(), fascia.Height,
                                              "e l'ultima riga sta dentro la fascia")

            End Using

        End Sub

        <TestMethod>
        Public Sub LaCatturaNonEPiuLaGemellaDellImportDelCv()

            ' Due bottoni pieni dello stesso colore, affiancati, per due azioni diverse:
            ' dentro un sistema in cui il colore dice la conseguenza, quella era la coppia
            ' che lo smentiva. La cattura è l'azione principale del pannello (livello 3),
            ' l'import del CV una conferma senza rischio (livello 1).
            Using pannello As New PannelloRicerca()

                Assert.AreEqual(LivelloBottone.AzionePrincipale, Bottone(pannello, "btnCattura").Tag)
                Assert.AreEqual(LivelloBottone.SicuroPositivo, Bottone(pannello, "btnImportaCv").Tag)

                ' Senza una pagina aperta i due comandi sono spenti, e da spenti sono
                ' grigi tutti e due com'è giusto: il colore che li distingue è quello che
                ' si vede quando si possono premere.
                Bottone(pannello, "btnCattura").Enabled = True
                Bottone(pannello, "btnImportaCv").Enabled = True

                Assert.AreNotEqual(Bottone(pannello, "btnCattura").BackColor,
                                   Bottone(pannello, "btnImportaCv").BackColor,
                                   "e a occhio nudo non si somigliano più")

            End Using

        End Sub

        <TestMethod>
        Public Sub DimenticareUnaRicercaDiceCheCosaSparisceEChePuoTornare()

            ' Il testo della conferma di livello 5: il banco lo legge da qui, perché di una
            ' finestra modale non può aspettare la chiusura (come in P1).
            Dim domanda As String = PannelloRicerca.SpiegazioneDelDimenticare("Muletto a Chiavari")

            Assert.Contains("Muletto a Chiavari", domanda, "quale ricerca sparisce")
            Assert.Contains("candidature", domanda, "e che cosa non si tocca")
            Assert.Contains("si rifà", domanda, "e che una ricerca si può rifare")

        End Sub

        ' ==================================================================
        ' L'indirizzo che arriverebbe al browser
        ' ==================================================================

        <TestMethod>
        Public Sub LIndirizzoDellaRicercaEQuelloDelPortaleScelto()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    ' La composizione è del portale, non del pannello: qui si verifica che
                    ' il pannello peschi il portale giusto e gli passi le due caselle.
                    Dim portale As Portale = contesto.Ricerche.TrovaPortale("Indeed")

                    Assert.AreEqual("https://it.indeed.com/jobs?q=perito%20elettronico&l=Genova",
                                    portale.ComponiUrl("perito elettronico", "Genova"))

                End Sub)

        End Sub

        ' ==================================================================
        ' La cattura (cap. 06.4)
        ' ==================================================================

        <TestMethod>
        Public Async Function LaCatturaConsegnaTestoFonteELink() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Magazziniere - Rossi S.p.A. | Indeed",
                    .Indirizzo = "https://it.indeed.com/viewjob?jk=9f3c1a",
                    .Testo = TestoDiUnAnnuncio()}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    ' Con una pagina da leggere il comando è acceso: è la sola condizione.
                    Assert.IsTrue(Bottone(pannello, "btnCattura").Enabled)

                    Dim consegnato As AnnuncioCatturatoEventArgs = Nothing
                    AddHandler pannello.AnnuncioCatturato,
                        Sub(mittente, argomenti) consegnato = argomenti

                    Await pannello.CatturaAsync()

                    Assert.IsNotNull(consegnato, "la cattura non ha consegnato niente")
                    Assert.AreEqual(TestoDiUnAnnuncio(), consegnato.Testo, "il testo della pagina")
                    Assert.AreEqual("Indeed", consegnato.Fonte, "riconosciuto dal sito, non dallo schema")
                    Assert.AreEqual("https://it.indeed.com/viewjob?jk=9f3c1a", consegnato.Link)

                    ' E all'utente si dice cosa si è preso: è il modo più corto di
                    ' rispondere alla domanda «ma ha catturato quello giusto?».
                    Assert.Contains("Magazziniere", Etichetta(pannello, "lblStatoRicerca").Text)

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaPaginaSenzaTestoNonSiManda() As Task

            ' Una scheda vuota, una pagina non ancora caricata, un annuncio dentro un
            ' iframe: non si spende una chiamata all'AI per sentirsi dire quel che si sa.
            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {.Titolo = "Indeed", .Testo = "Caricamento…"}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As Boolean = False
                    AddHandler pannello.AnnuncioCatturato, Sub(mittente, argomenti) consegnato = True

                    Await pannello.CatturaAsync()

                    Assert.IsFalse(consegnato, "niente da analizzare, niente da consegnare")
                    Assert.IsNotEmpty(Etichetta(pannello, "lblStatoRicerca").Text,
                                      "ma l'utente sa perché non è successo niente")

                End Function)

        End Function

        ''' <summary>
        ''' Con la pagina ancora coperta dal banner dei cookie non si dice «aspetta che
        ''' finisca di caricarsi»: si dice di rispondere al banner <i>(2026-08-30)</i>.
        ''' </summary>
        ''' <remarks>
        ''' Il lettore adesso il banner lo lascia fuori, e quel che resta è troppo poco per
        ''' essere mandato: fin qui il rimedio era giusto. Sbagliato era il consiglio —
        ''' aspettare un caricamento che è finito da un pezzo, perché la pagina sta
        ''' aspettando <b>lui</b>. Lo stesso difetto del vicolo cieco di R5: un consiglio
        ''' corretto dato alla persona che ha un altro problema.
        ''' </remarks>
        <TestMethod>
        Public Async Function UnaPaginaCopertaDalConsensoDiceDiRispondereAlBanner() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Offerte di lavoro magazziniere | Indeed",
                    .Indirizzo = "https://it.indeed.com/jobs?q=magazziniere",
                    .Testo = "", .ConsensoAperto = True}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As Boolean = False
                    AddHandler pannello.AnnuncioCatturato, Sub(mittente, argomenti) consegnato = True

                    Await pannello.CatturaAsync()

                    Assert.IsFalse(consegnato, "sotto un banner non c'è nessun annuncio")

                    Dim riga As String = Etichetta(pannello, "lblStatoRicerca").Text

                    Assert.Contains("consenso ai cookie", riga, "si dice cosa sta succedendo")
                    Assert.Contains("Cattura annuncio", riga, "e quale bottone ripremere dopo")
                    Assert.DoesNotContain("finisca di caricarsi", riga,
                                          "non il consiglio che non porta da nessuna parte")
                    Assert.IsLessThan(200, riga.Length,
                                      "e corta: nelle due righe della riga grigia ci deve stare")

                End Function)

        End Function

        ''' <summary>
        ''' Lo stesso vale per l'import del CV, che legge la stessa pagina dall'altra porta:
        ''' lì il bottone da ripremere è il suo.
        ''' </summary>
        <TestMethod>
        Public Async Function AncheLImportDelCvDiceDiRispondereAlBanner() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "LinkedIn",
                    .Indirizzo = "https://www.linkedin.com/in/mario-rossi/",
                    .Testo = "", .ConsensoAperto = True}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As Boolean = False
                    AddHandler pannello.CvCatturato, Sub(mittente, argomenti) consegnato = True

                    Await pannello.ImportaCvAsync()

                    Assert.IsFalse(consegnato, "e nemmeno nessun CV")

                    Dim riga As String = Etichetta(pannello, "lblStatoRicerca").Text

                    Assert.Contains("consenso ai cookie", riga, "si dice cosa sta succedendo")
                    Assert.Contains("Importa CV", riga, "e il bottone è l'altro")
                    Assert.DoesNotContain("pagina profilo", riga,
                                          "aprire la pagina profilo qui non serve a niente")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaPaginaCheNonSiLasciaLeggereNonFaCadereIlPannello() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Guasto = New InvalidOperationException("la vista non risponde")}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As Boolean = False
                    AddHandler pannello.AnnuncioCatturato, Sub(mittente, argomenti) consegnato = True

                    Await pannello.CatturaAsync()

                    Assert.IsFalse(consegnato)
                    Assert.AreEqual(1, lettore.Letture, "ci ha provato")

                    Dim riga As Label = Etichetta(pannello, "lblStatoRicerca")

                    ' Il ripiego onesto: il testo si può sempre incollare a mano in P4.
                    Assert.Contains(NomiUi.Confronto, riga.Text)

                    ' Dal 2026-09-01 un guasto in questo pannello si vede e si legge: fino
                    ' ad allora finiva nel grigio delle didascalie, indistinguibile da
                    ' «Ricerca salvata» (v. Segnalazioni).
                    Assert.StartsWith(Segnalazioni.PrefissoErrore, riga.Text,
                                      "la riga dice che è un errore, non solo col colore")
                    Assert.AreEqual(StileApp.Pericolo, riga.ForeColor, "e il colore c'è lo stesso")

                End Function)

        End Function

        ''' <summary>
        ''' Un'attesa interrotta non è una pagina illeggibile: dirla così darebbe la colpa
        ''' al sito per una cosa che ha chiesto l'utente (T9d).
        ''' </summary>
        <TestMethod>
        Public Async Function UnaLetturaInterrottaNonDaLaColpaAllaPagina() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Guasto = New OperationCanceledException()}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Await pannello.CatturaAsync()

                    Dim riga As Label = Etichetta(pannello, "lblStatoRicerca")

                    Assert.Contains("annullata", riga.Text, "si dice com'è andata")
                    Assert.DoesNotContain("Non sono riuscita a leggere", riga.Text,
                                          "e non si accusa la pagina, che non c'entra")

                    ' L'altra metà della stessa onestà: chi ha fermato lui una lettura non
                    ' deve trovarsi davanti la parola «Errore» e una riga rossa. È quel che
                    ' tiene il prefisso una cosa che significa qualcosa (v. Segnalazioni).
                    Assert.DoesNotContain(Segnalazioni.PrefissoErrore, riga.Text,
                                          "un annullamento non è un errore")
                    Assert.AreEqual(StileApp.TestoSecondario, riga.ForeColor, "e non si tinge")

                End Function)

        End Function

        ''' <summary>
        ''' Dopo un errore, la riga torna grigia alla prima cosa che va bene.
        ''' </summary>
        ''' <remarks>
        ''' È il difetto che il rosso si porta dietro: da quando questa riga sa tingersi
        ''' (2026-09-01), se il colore non si riscrivesse a ogni giro resterebbe quello di
        ''' prima — e «Catturato: …» comparirebbe in rosso perché dieci secondi fa una
        ''' pagina non si era lasciata leggere. Nessun collaudo sul singolo messaggio se ne
        ''' accorgerebbe: il difetto vive nella <i>successione</i> di due messaggi, ed è
        ''' l'unico modo di vederlo.
        ''' </remarks>
        <TestMethod>
        Public Async Function DopoUnErroreLaRigaTornaGrigia() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Guasto = New InvalidOperationException("la vista non risponde")}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Await pannello.CatturaAsync()

                    Dim riga As Label = Etichetta(pannello, "lblStatoRicerca")
                    Assert.AreEqual(StileApp.Pericolo, riga.ForeColor, "prima è rossa")

                    ' Adesso la pagina si lascia leggere, ed è un annuncio buono.
                    lettore.Guasto = Nothing
                    lettore.Pagina = New PaginaLetta With {
                        .Titolo = "Magazziniere - Rossi S.p.A.",
                        .Indirizzo = "https://it.indeed.com/viewjob?jk=9f3c1a",
                        .Testo = TestoDiUnAnnuncio()}

                    Await pannello.CatturaAsync()

                    Assert.Contains("Catturato", riga.Text, "la cattura è andata")
                    Assert.AreEqual(StileApp.TestoSecondario, riga.ForeColor,
                                    "e il rosso di prima non le resta addosso")
                    Assert.DoesNotContain(Segnalazioni.PrefissoErrore, riga.Text,
                                          "né la parola")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaPaginaTroncataLoDichiara() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Annuncio lunghissimo",
                    .Indirizzo = "https://www.aziendarossi.it/lavora-con-noi",
                    .Testo = TestoDiUnAnnuncio(), .Troncato = True}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As AnnuncioCatturatoEventArgs = Nothing
                    AddHandler pannello.AnnuncioCatturato,
                        Sub(mittente, argomenti) consegnato = argomenti

                    Await pannello.CatturaAsync()

                    ' Si cattura lo stesso — un annuncio letto a metà è ancora un annuncio —
                    ' ma il taglio si dice: niente si perde in silenzio.
                    Assert.IsNotNull(consegnato)
                    Assert.AreEqual("aziendarossi.it", consegnato.Fonte, "un sito che non è fra i portali")
                    Assert.Contains("prima parte", Etichetta(pannello, "lblStatoRicerca").Text)

                End Function)

        End Function

        <TestMethod>
        Public Async Function LaPaginaConLElencoLoDiceInUnaFinestra() As Task

            ' Una pagina-risultati come la restituisce un portale: trenta annunci di fila,
            ' righe corte, le parole-spia ripetute a ogni voce.
            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Offerte di lavoro magazziniere | Indeed",
                    .Indirizzo = "https://it.indeed.com/jobs?q=magazziniere",
                    .Testo = TestoDiUnaLista()}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As Boolean = False
                    AddHandler pannello.AnnuncioCatturato, Sub(mittente, argomenti) consegnato = True

                    Dim finestre As New List(Of String)

                    Await pannello.CatturaAsync(Sub(testo) finestre.Add(testo))

                    Assert.IsFalse(consegnato, "un elenco non è un annuncio: non si consegna")

                    ' Il punto del collaudo. Prima l'avviso viveva solo nella riga grigia in
                    ' fondo, che è alta due righe e lo tagliava a metà: si vedeva il problema
                    ' e non la via d'uscita. Ora esce in una finestra che si deve chiudere.
                    Assert.HasCount(1, finestre, "una finestra, e una sola")
                    Assert.Contains("nuova finestra", finestre(0),
                                    "e dice come aprire il singolo annuncio")
                    Assert.Contains("Cattura annuncio", finestre(0), "e cosa fare dopo averlo aperto")
                    Assert.Contains("selezion", finestre(0), "più l'altra strada, quella della selezione")

                    ' Nella riga grigia resta la traccia, corta abbastanza da entrarci.
                    Dim riga As String = Etichetta(pannello, "lblStatoRicerca").Text
                    Assert.Contains("elenco degli annunci", riga, "la riga grigia dice cos'è successo")
                    Assert.IsLessThan(200, riga.Length, "ma corta: nelle due righe che ha ci deve stare")

                End Function)

        End Function

        ''' <summary>
        ''' Sulla pagina di un annuncio solo la cattura non si ferma, nemmeno se il testo
        ''' ha la forma di una lista.
        ''' </summary>
        ''' <remarks>
        ''' <para>È il vicolo cieco del 2026-08-30, e il collaudo che lo tiene chiuso. Su
        ''' Indeed un annuncio aperto <b>da solo</b> ha esattamente la forma che il giudizio
        ''' sul testo chiama «lista»: righe corte di elenco puntato, e in coda «Candidati» e
        ''' i lavori simili con i loro «giorni fa». La cattura lo rifiutava consigliando di
        ''' aprire il singolo annuncio — che era quel che l'utente aveva appena fatto — e il
        ''' messaggio tornava identico a ogni tentativo.</para>
        ''' <para>Il testo qui è <b>lo stesso</b> del collaudo che vede la lista, riga per
        ''' riga: cambia solo l'indirizzo. Se cambiasse anche il testo, i due collaudi non
        ''' direbbero più che è l'indirizzo a decidere — direbbero che due testi diversi
        ''' finiscono diversamente, che non è la stessa cosa.</para>
        ''' </remarks>
        <TestMethod>
        Public Async Function SullaPaginaDiUnAnnuncioLaCatturaNonSiFerma() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Magazziniere - Logistica Bianchi s.r.l. | Indeed",
                    .Indirizzo = "https://it.indeed.com/viewjob?jk=9f3a1c",
                    .Testo = TestoDiUnaLista()}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As String = Nothing
                    AddHandler pannello.AnnuncioCatturato,
                        Sub(mittente, argomenti) consegnato = argomenti.Testo

                    Dim finestre As New List(Of String)

                    Await pannello.CatturaAsync(Sub(testo) finestre.Add(testo))

                    Assert.IsEmpty(finestre, "l'indirizzo dice che è un annuncio: niente avviso")
                    Assert.IsNotNull(consegnato, "e l'annuncio si consegna")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnAnnuncioGiaCatturatoNonSiRianalizza() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Magazziniere - Rossi S.p.A.",
                    .Indirizzo = "https://it.indeed.com/viewjob?jk=9f3c1a",
                    .Testo = TestoDiUnAnnuncio()}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    ' Come se quella pagina fosse già stata catturata prima.
                    contesto.Opportunita.Salva(New Opportunita With {
                        .Annuncio = JsonNode.Parse("{""titolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A.""}"),
                        .Link = "https://it.indeed.com/viewjob?jk=9f3c1a"})

                    Dim consegnato As Boolean = False
                    AddHandler pannello.AnnuncioCatturato, Sub(mittente, argomenti) consegnato = True

                    Await pannello.CatturaAsync()

                    ' Niente seconda analisi — sarebbero due chiamate all'AI per riscrivere
                    ' quel che c'è già — e niente seconda cartella nella coda.
                    Assert.IsFalse(consegnato)
                    Assert.HasCount(1, contesto.Opportunita.Elenco())

                    ' Ma si dice dov'è la prima, e come rifarla se è quello che si vuole.
                    Dim detto As String = Etichetta(pannello, "lblStatoRicerca").Text
                    Assert.Contains("già catturato", detto)
                    Assert.Contains(NomiUi.Confronto, detto)

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnAltroAnnuncioDelloStessoPortaleSiCatturaLoStesso() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Indirizzo = "https://it.indeed.com/viewjob?jk=DIVERSO",
                    .Testo = TestoDiUnAnnuncio()}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    contesto.Opportunita.Salva(New Opportunita With {
                        .Annuncio = JsonNode.Parse("{""titolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A.""}"),
                        .Link = "https://it.indeed.com/viewjob?jk=9f3c1a"})

                    Dim consegnato As Boolean = False
                    AddHandler pannello.AnnuncioCatturato, Sub(mittente, argomenti) consegnato = True

                    Await pannello.CatturaAsync()

                    Assert.IsTrue(consegnato, "il doppione è la pagina, non il portale")

                End Function)

        End Function

        ' ==================================================================
        ' Il CV dalla pagina (cap. 06.7 — T5d)
        ' ==================================================================

        <TestMethod>
        Public Async Function IlCvLettoDallaPaginaVaAllaSchedaDelProfilo() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Mirco Parenti | LinkedIn",
                    .Indirizzo = "https://www.linkedin.com/in/mirco-parenti",
                    .Testo = TestoDiUnaPaginaProfilo()}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Assert.IsTrue(Bottone(pannello, "btnImportaCv").Enabled,
                                  "con una pagina aperta si può leggere")

                    Dim consegnato As CvCatturatoEventArgs = Nothing
                    AddHandler pannello.CvCatturato,
                        Sub(mittente, argomenti) consegnato = argomenti

                    Await pannello.ImportaCvAsync()

                    Assert.IsNotNull(consegnato, "la lettura non ha consegnato niente")
                    Assert.AreEqual(TestoDiUnaPaginaProfilo(), consegnato.Testo, "il testo della pagina")
                    Assert.AreEqual("linkedin.com", consegnato.Fonte,
                                    "il sito, che non è fra i portali di ricerca")
                    Assert.AreEqual("https://www.linkedin.com/in/mirco-parenti", consegnato.Link)

                    ' Chi preme si ritrova su un altro pannello: deve sapere che è apposta.
                    Assert.Contains("Profilo", Etichetta(pannello, "lblStatoRicerca").Text)

                End Function)

        End Function

        <TestMethod>
        Public Async Function LaStessaPaginaSiPuoRileggereQuanteVolteSiVuole() As Task

            ' Il divieto del doppione è una regola degli **annunci**: due candidature
            ' gemelle nella coda non servono a nessuno. Un profilo no — il proprio
            ' percorso cambia, e rileggerlo dopo averlo aggiornato è esattamente quel che
            ' si vuole fare. Qui si verifica che le due regole non si siano mescolate.
            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Mirco Parenti | LinkedIn",
                    .Indirizzo = "https://www.linkedin.com/in/mirco-parenti",
                    .Testo = TestoDiUnaPaginaProfilo()}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim quante As Integer = 0
                    AddHandler pannello.CvCatturato, Sub(mittente, argomenti) quante += 1

                    Await pannello.ImportaCvAsync()
                    Await pannello.ImportaCvAsync()

                    Assert.AreEqual(2, quante, "un profilo si rilegge quante volte si vuole")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaPaginaSenzaTestoNonDiventaUnProfilo() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {.Titolo = "LinkedIn", .Testo = "Caricamento…"}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As Boolean = False
                    AddHandler pannello.CvCatturato, Sub(mittente, argomenti) consegnato = True

                    Await pannello.ImportaCvAsync()

                    Assert.IsFalse(consegnato, "niente da strutturare, niente da consegnare")
                    Assert.Contains("pagina profilo", Etichetta(pannello, "lblStatoRicerca").Text,
                                    "e si dice cosa aprire, invece di lasciare l'utente a indovinare")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaPaginaIlleggibileRimandaAllAltraPortaDelloStessoMestiere() As Task

            Dim lettore As New LettorePaginaFinto With {
                .Guasto = New InvalidOperationException("la vista non risponde")}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As Boolean = False
                    AddHandler pannello.CvCatturato, Sub(mittente, argomenti) consegnato = True

                    Await pannello.ImportaCvAsync()

                    Assert.IsFalse(consegnato)
                    Assert.AreEqual(1, lettore.Letture, "ci ha provato")

                    ' Il ripiego onesto di questo comando non è «incolla in Candidatura»
                    ' come per l'annuncio: è l'altra porta dell'import, cioè il file.
                    Assert.Contains("file", Etichetta(pannello, "lblStatoRicerca").Text)

                End Function)

        End Function

        <TestMethod>
        Public Async Function LImportScorreLaPaginaPrimaDiLeggerla() As Task

            ' Il fatto misurato a T5d sulla pagina vera: senza scorrere esce l'intestazione
            ' e basta — una esperienza senza date né mansioni, zero studi, zero competenze —
            ' perché le sezioni entrano nel documento solo mentre si scende.
            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Mirco Parenti | LinkedIn",
                    .Indirizzo = "https://www.linkedin.com/in/mirco-parenti",
                    .Testo = TestoDiUnaPaginaProfilo()},
                .PaginaDopoScorrimento = New PaginaLetta With {
                    .Titolo = "Mirco Parenti | LinkedIn",
                    .Indirizzo = "https://www.linkedin.com/in/mirco-parenti",
                    .Testo = TestoDiUnaPaginaProfilo() & vbLf & TestoCheArrivaScendendo()}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Dim consegnato As CvCatturatoEventArgs = Nothing
                    AddHandler pannello.CvCatturato,
                        Sub(mittente, argomenti) consegnato = argomenti

                    Await pannello.ImportaCvAsync()

                    Assert.AreEqual(1, lettore.Scorrimenti, "ha scorso, e una volta sola")
                    Assert.IsNotNull(consegnato)
                    Assert.Contains("Diploma", consegnato.Testo,
                                    "e ha letto **dopo** aver scorso: c'è anche quel che si carica scendendo")

                    ' Quanto si è preso si dice: è il solo modo che l'utente ha di
                    ' accorgersi che alla strutturazione è andata poca roba.
                    Assert.Contains("caratteri", Etichetta(pannello, "lblStatoRicerca").Text)

                End Function)

        End Function

        <TestMethod>
        Public Async Function LaCatturaDellAnnuncioNonScorre() As Task

            ' La cattura è collaudata così com'è dal 2026-08-12, e legge la pagina che
            ' l'utente sta guardando: aggiungerle uno scorrimento cambierebbe, su tutti i
            ' portali, quello che finisce nell'analisi — senza che nessuno l'abbia chiesto.
            Dim lettore As New LettorePaginaFinto With {
                .Pagina = New PaginaLetta With {
                    .Titolo = "Magazziniere - Rossi S.p.A. | Indeed",
                    .Indirizzo = "https://it.indeed.com/viewjob?jk=9f3c1a",
                    .Testo = TestoDiUnAnnuncio()}}

            Await ConPannelloAsync(lettore,
                Async Function(pannello, contesto, cartella) As Task

                    Await pannello.CatturaAsync()

                    Assert.AreEqual(0, lettore.Scorrimenti, "la cattura legge la pagina com'è")
                    Assert.AreEqual(1, lettore.Letture)

                End Function)

        End Function

        <TestMethod>
        Public Async Function ArrivandoDalProfiloIlPannelloDiceCosaFare() As Task

            ' Chi preme «IMPORTA CV DA LINKEDIN» nella scheda del profilo si ritrova qui,
            ' in un pannello che si chiama «Ricerca» e che di profili non parla: senza
            ' questa frase avrebbe fatto un passo e non saprebbe qual è il successivo.
            ' Alla prima apertura lo direbbe anche la pagina di casa; alla seconda il
            ' browser è fermo su una pagina qualunque e non lo direbbe nessuno.
            Await ConPannelloAsync(Nothing,
                Async Function(pannello, contesto, cartella) As Task

                    Await pannello.ApriPerIlCvAsync()

                    Dim detto As String = Etichetta(pannello, "lblStatoRicerca").Text
                    Assert.Contains("pagina profilo", detto, "dice cosa aprire")
                    Assert.Contains("Importa CV da questa pagina", detto, "e cosa premere, con il suo nome")

                End Function)

        End Function

        <TestMethod>
        Public Sub SenzaUnaPaginaDaLeggereAncheLImportRestaSpento()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    ' Stessa regola della cattura, e per la stessa ragione: senza browser
                    ' non c'è niente da leggere, e il bottone lo dice stando spento.
                    Assert.IsFalse(Bottone(pannello, "btnImportaCv").Enabled)

                End Sub)

        End Sub

        ' ==================================================================
        ' Attrezzi
        ' ==================================================================

        ''' <summary>
        ''' Una pagina profilo come la rende <c>innerText</c>: il percorso della persona
        ''' insieme a tutto quello che il sito ci mette intorno.
        ''' </summary>
        Private Shared Function TestoDiUnaPaginaProfilo() As String

            Return "Mirco Parenti" & vbLf &
                   "Perito elettronico — Chiavari, Liguria" & vbLf &
                   "Esperienza" & vbLf &
                   "Magazziniere presso Rossi S.p.A. — 2023-2024. Carico e scarico merci, " &
                   "gestione dei documenti di trasporto, uso del muletto." & vbLf &
                   "Formazione" & vbLf &
                   "Diploma di perito elettronico, ITIS Marconi, 2019." & vbLf &
                   "Patentino del muletto, 2023." & vbLf &
                   "Persone che potresti conoscere · Annunci · Altro dal feed"

        End Function

        ''' <summary>
        ''' Le sezioni che un sito moderno aggiunge al documento <b>mentre si scende</b>:
        ''' finché non si scorre, per il lettore non esistono.
        ''' </summary>
        Private Shared Function TestoCheArrivaScendendo() As String

            Return "Formazione" & vbLf &
                   "Diploma di perito elettronico, ITIS Marconi, 2019." & vbLf &
                   "Licenze e certificazioni" & vbLf &
                   "Patentino del muletto, 2023."

        End Function

        ''' <summary>Un annuncio abbastanza lungo da valere una chiamata all'AI.</summary>
        Private Shared Function TestoDiUnAnnuncio() As String

            Return "Magazziniere addetto al carico e scarico merci. " &
                   "Cerchiamo una persona con esperienza di almeno un anno in magazzino, " &
                   "patentino del muletto in corso di validità e disponibilità ai turni. " &
                   "Sede di lavoro: Genova. Contratto a tempo determinato con possibilità di proroga."

        End Function

        ''' <summary>
        ''' Un pannello collegato a un contesto tutto suo, su una cartella dati temporanea,
        ''' <b>senza motore del browser</b>: la WebView resta spenta.
        ''' </summary>
        Private Shared Sub ConPannello(prova As Action(Of PannelloRicerca, ContestoApp, CartellaDati))

            ' Tutto quel che c'è dentro è sincrono: aspettare qui non blocca niente.
            ConPannelloAsync(Nothing,
                Function(pannello, contesto, cartella) As Task
                    prova(pannello, contesto, cartella)
                    Return Task.CompletedTask
                End Function).GetAwaiter().GetResult()

        End Sub

        ''' <summary>
        ''' Lo stesso pannello, con un lettore di pagina in mano: è così che la cattura si
        ''' prova senza WebView2 e senza un thread STA.
        ''' </summary>
        Private Shared Async Function ConPannelloAsync(
            lettore As ILettorePagina,
            prova As Func(Of PannelloRicerca, ContestoApp, CartellaDati, Task)) As Task

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "ricerca-" & Guid.NewGuid().ToString("N"))

            Using contesto As ContestoApp = ContestoApp.Monta(
                radice, "chiave-di-collaudo", Path.Combine(Path.GetTempPath(), "pool-inesistente"))

                Dim pannello As New PannelloRicerca()

                Try
                    pannello.Collega(contesto, lettore:=lettore)
                    Await prova(pannello, contesto, contesto.Cartella)
                Finally
                    pannello.Dispose()
                    If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
                End Try

            End Using

        End Function

        Private Shared Function Casella(pannello As Control, nome As String) As TextBox
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), TextBox)
        End Function

        Private Shared Function Bottone(pannello As Control, nome As String) As Button
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Etichetta(pannello As Control, nome As String) As Label
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

        Private Shared Function Menu(pannello As Control, nome As String) As ComboBox
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), ComboBox)
        End Function

        ''' <summary>
        ''' Il testo di una pagina che <b>sembra</b> una lista: righe corte tutte uguali e
        ''' le parole di servizio ripetute a ogni voce.
        ''' </summary>
        ''' <remarks>
        ''' Sta qui, e non dentro un collaudo, perché lo condividono i due che si guardano
        ''' in faccia: quello che sulla pagina-elenco vuole l'avviso, e quello che sulla
        ''' pagina di un annuncio lo vieta. Con lo stesso testo in mano, l'unica differenza
        ''' fra i due è l'indirizzo — che è precisamente ciò che si vuole dimostrare.
        ''' </remarks>
        Private Shared Function TestoDiUnaLista() As String

            Dim righe As New List(Of String)
            For i As Integer = 1 To 30
                righe.AddRange({$"Magazziniere addetto al carico {i}",
                                "Logistica Bianchi s.r.l. - Sestri Levante",
                                $"{i} giorni fa",
                                "Candidati"})
            Next

            Return String.Join(vbLf, righe)

        End Function


    End Class

End Namespace
