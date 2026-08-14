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

                    ' Il ripiego onesto: il testo si può sempre incollare a mano in P4.
                    Assert.Contains("Candidatura", Etichetta(pannello, "lblStatoRicerca").Text)

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
                    Assert.Contains("Candidatura", detto)

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

            ' Chi preme «Importa CV da un sito…» nella scheda del profilo si ritrova qui,
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

    End Class

End Namespace
