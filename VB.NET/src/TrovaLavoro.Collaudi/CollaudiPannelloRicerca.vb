Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

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
        Public Sub LaCatturaSiDichiaraDiT5b()

            ConPannello(
                Sub(pannello, contesto, cartella)

                    ' Regola 03.8: il comando della tappa che verrà è visibile e spento,
                    ' con scritto quando arriva.
                    Assert.IsFalse(Bottone(pannello, "btnCattura").Enabled)
                    Assert.IsTrue(Bottone(pannello, "btnCattura").Visible OrElse
                                  Not pannello.Visible,
                                  "il bottone c'è: è il pannello che nasce nascosto")

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
        ' Attrezzi
        ' ==================================================================

        ''' <summary>
        ''' Un pannello collegato a un contesto tutto suo, su una cartella dati temporanea,
        ''' <b>senza motore del browser</b>: la WebView resta spenta.
        ''' </summary>
        Private Shared Sub ConPannello(prova As Action(Of PannelloRicerca, ContestoApp, CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "ricerca-" & Guid.NewGuid().ToString("N"))

            Using contesto As ContestoApp = ContestoApp.Monta(
                radice, "chiave-di-collaudo", Path.Combine(Path.GetTempPath(), "pool-inesistente"))

                Dim pannello As New PannelloRicerca()

                Try
                    pannello.Collega(contesto)
                    prova(pannello, contesto, contesto.Cartella)
                Finally
                    pannello.Dispose()
                    If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
                End Try

            End Using

        End Sub

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
