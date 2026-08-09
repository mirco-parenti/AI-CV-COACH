Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi del pannello P2 (cap. 03.6). Un pannello non si collauda come una
    ''' funzione, ma le due cose che qui possono rompersi davvero sono verificabili
    ''' benissimo: che il profilo finisca <b>tutto</b> nei controlli giusti, e che la
    ''' voce selezionata in una lista sia quella di cui si vedono i campi — un indice
    ''' disallineato metterebbe sotto gli occhi dell'utente i dati di un'altra
    ''' esperienza, che in una scheda «campo per campo» è il peggio che possa capitare.
    ''' </summary>
    ''' <remarks>
    ''' I controlli si raggiungono per <b>nome</b>, con <c>Controls.Find</c>: sono privati
    ''' all'assembly, e va bene così — questo banco vede il pannello come lo vede
    ''' l'utente. In più, cercarli per nome verifica di sfuggita che i nomi siano quelli
    ''' della convenzione del cap. 03.7.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiPannelloProfilo

        <TestMethod>
        Public Sub IlProfiloFinisceTuttoNeiControlli()
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.Mostra(ProfiloDiProva())

                Assert.AreEqual("Luca Ferrari", Casella(pannello, "txtNome").Text, "nome")
                Assert.AreEqual("luca.ferrari@example.it", Casella(pannello, "txtEmail").Text, "email")
                Assert.AreEqual("Forlì", Casella(pannello, "txtDomicilio").Text, "domicilio")
                Assert.AreEqual("B", Casella(pannello, "txtCategorie").Text, "categorie della patente")

                Assert.HasCount(2, Elenco(pannello, "lstLavoro").Items, "esperienze di lavoro")
                Assert.HasCount(1, Elenco(pannello, "lstInformali").Items, "esperienze informali")
                Assert.HasCount(4, Elenco(pannello, "lstCompetenze").Items, "competenze")
                Assert.HasCount(2, Elenco(pannello, "lstStudi").Items, "titoli di studio")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaVoceElencataDiceRuoloEAzienda()
            ' L'elenco deve bastare a riconoscere una voce senza aprirla.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.Mostra(ProfiloDiProva())

                Assert.AreEqual("Addetto al magazzino — Romagna Logistica S.r.l.",
                                Elenco(pannello, "lstLavoro").Items(0).ToString(), "prima esperienza")
            End Using
        End Sub

        <TestMethod>
        Public Sub ISuoiCampiSonoQuelliDellaVoceSelezionata()
            ' Il collaudo che conta: cambiare riga deve cambiare tutti i campi, non
            ' alcuni. Una scheda che mostra il ruolo di una voce e l'azienda di un'altra
            ' sarebbe peggio di una scheda vuota.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.Mostra(ProfiloDiProva())

                Assert.AreEqual("Addetto al magazzino", Casella(pannello, "txtRuolo").Text,
                                "all'inizio si vede la prima voce")

                Elenco(pannello, "lstLavoro").SelectedIndex = 1

                Assert.AreEqual("Commesso di reparto", Casella(pannello, "txtRuolo").Text, "ruolo")
                Assert.AreEqual("Supermercati Bertozzi", Casella(pannello, "txtAzienda").Text, "azienda")
                Assert.AreEqual("1 anno", Casella(pannello, "txtDurata").Text, "durata")
                Assert.Contains("Rifornimento degli scaffali", Casella(pannello, "txtCosaFacevoLavoro").Text,
                                "cosa facevo")
            End Using
        End Sub

        <TestMethod>
        Public Sub UnProfiloVuotoNonRompeNiente()
            ' È lo stato del primo avvio: nessuna lista, nessuna voce selezionata,
            ' nessun campo da mostrare.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.Mostra(New TrovaLavoro.Dati.Profilo())

                Assert.IsEmpty(Casella(pannello, "txtNome").Text, "nome")
                Assert.IsEmpty(Elenco(pannello, "lstLavoro").Items, "nessuna esperienza")
                Assert.IsEmpty(Casella(pannello, "txtRuolo").Text, "e nessun dettaglio da mostrare")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaSchedaDelTestoLettoCompareSoloDopoUnImport()
            ' Senza un CV alle spalle sarebbe una scheda vuota che promette qualcosa che
            ' non c'è.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                Dim schede As TabControl = DirectCast(
                    pannello.Controls.Find("tabSezioni", searchAllChildren:=True).Single(), TabControl)

                Assert.HasCount(4, schede.TabPages, "le quattro sezioni del profilo")
                Assert.IsFalse(schede.TabPages.Cast(Of TabPage)().Any(Function(s) s.Name = "tabTestoLetto"),
                               "e non la scheda del testo letto")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaFasciaDelleAzioniLasciaIlPostoAlLogo()
            ' Il vincolo geometrico del cap. 03.5: il logo flottante copre l'angolo in
            ' basso a sinistra, e lì non può finire nulla di vivo.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.ImpostaIngombroLogo(New Size(261, 188))

                Dim azioni As Panel = DirectCast(
                    pannello.Controls.Find("pnlAzioni", searchAllChildren:=True).Single(), Panel)

                Assert.AreEqual(188, azioni.Height, "alta quanto il logo sfonda nell'area centrale")
                Assert.AreEqual(273, azioni.Padding.Left, "e i bottoni cominciano dopo la sua larghezza")

                Dim primo As Button = DirectCast(
                    pannello.Controls.Find("btnImporta", searchAllChildren:=True).Single(), Button)
                Assert.IsGreaterThanOrEqualTo(273, primo.Left, "nessun bottone sotto il logo")
            End Using
        End Sub

        <TestMethod>
        Public Sub InModalitaCompattaLaFasciaNonSchiacciaIBottoni()
            ' In compatta il logo sfonda molto meno (68 px): la fascia non deve scendere
            ' sotto l'altezza che serve a un bottone con i suoi margini.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.ImpostaIngombroLogo(New Size(130, 68))

                Dim azioni As Panel = DirectCast(
                    pannello.Controls.Find("pnlAzioni", searchAllChildren:=True).Single(), Panel)

                Assert.AreEqual(68, azioni.Height, "quanto l'ingombro, che qui è già sufficiente")
                Assert.AreEqual(142, azioni.Padding.Left, "e il rientro segue il logo compatto")
            End Using
        End Sub

        <TestMethod>
        Public Sub CollegatoAlMotoreMostraIlProfiloSalvato()
            ' Il percorso vero dell'avvio: dall'archivio su disco fino ai campi.
            ConCartellaTemporanea(
                Sub(radice)
                    Dim cartella As New CartellaDati(radice)
                    Call New ArchivioProfilo(cartella).Salva(ProfiloDiProva())

                    Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                          pannello As New TrovaLavoro.PannelloProfilo()

                        pannello.Collega(contesto)

                        Assert.AreEqual("Luca Ferrari", Casella(pannello, "txtNome").Text, "il profilo salvato")
                        Assert.HasCount(2, Elenco(pannello, "lstLavoro").Items, "con le sue esperienze")
                        Assert.Contains("Salvato il", Etichetta(pannello, "lblStatoProfilo").Text,
                                        "e la scheda dice quando è stato salvato")
                    End Using
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnProfiloRottoSiDiceSenzaSvuotareNiente()
            ' Cap. 11.1: un profilo illeggibile non ripiega su un profilo vuoto. Prima
            ' di ogni altra cosa se ne mette in salvo una copia — da qui in poi basta
            ' un tasto per armare «Salva», che sovrascriverebbe il file da recuperare —
            ' e il pannello dice cosa non va e dove sta la copia, in rosso.
            ConCartellaTemporanea(
                Sub(radice)
                    Dim cartella As New CartellaDati(radice)
                    cartella.Assicura()
                    File.WriteAllText(cartella.FileProfilo, "{ questo non è JSON")

                    Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                          pannello As New TrovaLavoro.PannelloProfilo()

                        pannello.Collega(contesto)

                        Dim stato As Label = Etichetta(pannello, "lblStatoProfilo")
                        Assert.Contains("non si lascia leggere", stato.Text, "lo dice")
                        Assert.Contains("copia di sicurezza", stato.Text, "e dice della copia")
                        Assert.AreEqual(StileApp.Pericolo, stato.ForeColor, "con il colore dell'errore")

                        Assert.IsEmpty(Casella(pannello, "txtNome").Text, "niente dati inventati al posto suoi")
                        Assert.IsTrue(File.Exists(cartella.FileProfilo), "e il file rotto resta lì da recuperare")

                        Dim copie As String() = Directory.GetFiles(cartella.CartellaProfilo, "profilo.rotto-*.json")
                        Assert.HasCount(1, copie, "la copia di sicurezza c'è, una sola")
                        Assert.AreEqual("{ questo non è JSON", File.ReadAllText(copie(0)),
                                        "ed è il contenuto rotto, intatto")
                        Assert.Contains(copie(0), stato.Text, "il messaggio la nomina per esteso")
                    End Using
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LeCorrezioniFinisconoNelProfiloSalvato()
            ' Il giro completo della scheda: si corregge un campo dei dati personali e
            ' uno di una voce, si salva, e su disco c'è quello che si è scritto.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Casella(pannello, "txtNome").Text = "Luca Ferrari"
                    Casella(pannello, "txtTelefono").Text = "333 1234567"
                    Casella(pannello, "txtRuolo").Text = "Capo turno di magazzino"

                    Assert.IsTrue(pannello.HaModificheNonSalvate, "prima di salvare c'è del lavoro in sospeso")

                    Bottone(pannello, "btnSalva").PerformClick()

                    Assert.IsFalse(pannello.HaModificheNonSalvate, "dopo il salvataggio no")

                    Dim riletto As TrovaLavoro.Dati.Profilo = archivio.Carica()
                    Assert.AreEqual("333 1234567", riletto.Contatti.Telefono, "il recapito corretto")
                    Assert.AreEqual("Capo turno di magazzino", riletto.EsperienzeFormali(0).Ruolo, "e il ruolo")
                    Assert.AreEqual("Supermercati Bertozzi", riletto.EsperienzeFormali(1).Azienda,
                                    "le altre voci restano com'erano")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub NienteVaSuDiscoSenzaLaConferma()
            ' Cap. 12.7: nessun dato entra nel profilo senza un passaggio esplicito. Chi
            ' corregge e non salva non deve trovarsi il file cambiato.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Dim primaDi As String = archivio.Carica().ComeTesto()

                    Casella(pannello, "txtNome").Text = "Nome scritto per sbaglio"

                    Assert.AreEqual(primaDi, archivio.Carica().ComeTesto(), "il file non si è mosso")
                    Assert.HasCount(1, archivio.Versioni(), "e nello storico non è comparso nulla")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub OgniSalvataggioAggiungeUnaVersione()
            ' Lo storico è ciò che rende spiegabile un CV già inviato (cap. 11.1).
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Casella(pannello, "txtNome").Text = "Luca F."
                    Bottone(pannello, "btnSalva").PerformClick()

                    Assert.HasCount(2, archivio.Versioni(), "quella di partenza più questa")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LElencoSegueLaCorrezione()
            ' La riga dell'elenco deve dire quello che c'è nella scheda, non quello che
            ' c'era prima: se no si sceglie una voce guardando un'etichetta bugiarda.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Casella(pannello, "txtRuolo").Text = "Capo turno"

                    Assert.StartsWith("Capo turno", Elenco(pannello, "lstLavoro").Items(0).ToString(),
                                      "la riga si è aggiornata")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub CambiareVoceNonPerdeLaCorrezione()
            ' Le correzioni entrano nel profilo mentre si scrive, non quando si salva:
            ' altrimenti passare a un'altra voce le butterebbe via.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Casella(pannello, "txtRuolo").Text = "Capo turno"

                    Elenco(pannello, "lstLavoro").SelectedIndex = 1
                    Elenco(pannello, "lstLavoro").SelectedIndex = 0

                    Assert.AreEqual("Capo turno", Casella(pannello, "txtRuolo").Text, "è ancora lì")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnaVoceNuovaNasceVuotaEGiaSelezionata()
            ' «Aggiungi» deve portare l'utente esattamente dove si scrive la voce nuova.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    ' Si lavora sulla scheda delle esperienze di lavoro perché è quella
                    ' aperta: un bottone dentro una linguetta chiusa non è premibile, né
                    ' qui né per l'utente.
                    Bottone(pannello, "btnAggiungiLavoro").PerformClick()

                    ' Attenzione al nome: in VB le maiuscole non distinguono, e una
                    ' variabile «elenco» oscurerebbe la funzione «Elenco» qui sotto.
                    Dim lavori As ListBox = Elenco(pannello, "lstLavoro")
                    Assert.HasCount(3, lavori.Items, "un'esperienza in più")
                    Assert.AreEqual(2, lavori.SelectedIndex, "ed è quella selezionata")
                    Assert.IsEmpty(Casella(pannello, "txtRuolo").Text, "che nasce vuota")

                    Casella(pannello, "txtRuolo").Text = "Autista consegne"
                    Bottone(pannello, "btnSalva").PerformClick()

                    Dim riletto As TrovaLavoro.Dati.Profilo = archivio.Carica()
                    Assert.HasCount(3, riletto.EsperienzeFormali, "salvata con le altre")
                    Assert.AreEqual("Autista consegne", riletto.EsperienzeFormali(2).Ruolo, "con quello che ho scritto")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LaPatenteSiScriveComeLaLeggonoIPrompt()
            ' «B, C» in un campo solo diventa due categorie nel profilo: è la forma che
            ' il confronto con gli annunci si aspetta, ed è spesso il requisito
            ' eliminatorio.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Casella(pannello, "txtCategorie").Text = "B, C"
                    Bottone(pannello, "btnSalva").PerformClick()

                    Dim patente As PatenteProfilo = archivio.Carica().Patente
                    Assert.AreEqual("sì", patente.Ha, "la risposta resta quella del profilo di partenza")
                    CollectionAssert.AreEqual(New List(Of String) From {"B", "C"}, patente.Categorie,
                                              "e le due categorie sono distinte")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LeVociVuoteNonFinisconoSuDisco()
            ' Un «Aggiungi» premuto e mai riempito non deve arrivare nei prompt come
            ' un'esperienza fantasma: al salvataggio le voci tutte vuote si potano,
            ' dal profilo e dalla lista.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Bottone(pannello, "btnAggiungiLavoro").PerformClick()
                    Bottone(pannello, "btnSalva").PerformClick()

                    Assert.HasCount(2, archivio.Carica().EsperienzeFormali,
                                    "la voce mai riempita non è entrata")
                    Assert.HasCount(2, Elenco(pannello, "lstLavoro").Items,
                                    "e la lista si è ripulita")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnNoAllaPatenteNonSiPortaDietroLeCategorie()
            ' «no» con accanto ["B"] è una contraddizione che finirebbe nei prompt del
            ' confronto: le categorie valgono solo con la patente dichiarata, e la
            ' casella si spegne per non invitare a scrivere ciò che non verrà tenuto.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Dim scelta As ComboBox =
                        DirectCast(pannello.Controls.Find("cmbPatente", searchAllChildren:=True).Single(), ComboBox)

                    Assert.IsTrue(Casella(pannello, "txtCategorie").Enabled, "con «sì» si scrivono")

                    scelta.SelectedItem = "no"
                    Assert.IsFalse(Casella(pannello, "txtCategorie").Enabled, "con «no» la casella si spegne")

                    Bottone(pannello, "btnSalva").PerformClick()
                    Dim patente As PatenteProfilo = archivio.Carica().Patente
                    Assert.AreEqual("no", patente.Ha, "il no è salvato")
                    Assert.IsEmpty(patente.Categorie, "senza categorie residue")

                    ' Tornando su «sì» le categorie scritte prima ricompaiono da sole:
                    ' il testo della casella non era stato buttato.
                    scelta.SelectedItem = "sì"
                    Assert.IsTrue(Casella(pannello, "txtCategorie").Enabled, "la casella si riaccende")

                    Bottone(pannello, "btnSalva").PerformClick()
                    Assert.HasCount(1, archivio.Carica().Patente.Categorie, "e la categoria è tornata")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SiPuoTogliereSoloUnaVoceScelta()
            ' Un «Elimina» acceso davanti a un elenco vuoto prometterebbe un'azione che
            ' non può avvenire: il bottone deve seguire la selezione.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Assert.IsTrue(Bottone(pannello, "btnEliminaLavoro").Enabled,
                                  "con una voce scelta si può togliere")

                    Elenco(pannello, "lstLavoro").SelectedIndex = -1

                    Assert.IsFalse(Bottone(pannello, "btnEliminaLavoro").Enabled,
                                   "senza selezione no")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnProfiloAppenaApertoNonHaNienteDaSalvare()
            ' Il pannello non deve nascere «modificato»: se no il bottone «Salva» sarebbe
            ' acceso a vuoto e la chiusura chiederebbe di salvare correzioni mai fatte.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Assert.IsFalse(pannello.HaModificheNonSalvate, "niente in sospeso")
                    Assert.IsFalse(Bottone(pannello, "btnSalva").Enabled, "e il salva è spento")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaChiaveLImportNonSiPuoNemmenoTentare()
            ' Un bottone che non può funzionare non deve poter essere premuto: senza
            ' chiave l'AI non c'è, e leggere un CV passa dall'AI.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Assert.IsFalse(Bottone(pannello, "btnImporta").Enabled, "l'import è spento")
                    Assert.IsFalse(Bottone(pannello, "btnSalva").Enabled, "e senza correzioni anche il salva")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaChiaveNemmenoIlDialogoSiPuoAprire()
            ' Il dialogo guidato struttura ogni risposta con l'AI: senza chiave il
            ' bottone non deve promettere una conversazione che si fermerebbe subito.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Assert.IsFalse(Bottone(pannello, "btnDialogo").Enabled, "il dialogo è spento")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlProfiloCostruitoParlandoEntraNellaSchedaMaNonSuDisco()
            ' Il patto del passaggio P5 → P2 (cap. 12.7): il dialogo propone, la scheda
            ' mostra, e a scrivere su disco è solo l'utente con «Salva profilo».
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Dim primaDi As String = archivio.Carica().ComeTesto()

                    Dim raccolto As New TrovaLavoro.Dati.Profilo With {.Nome = "Anna Rossi"}
                    raccolto.Competenze.Add("Uso del muletto")

                    Assert.IsTrue(pannello.ProponiProfilo(raccolto, "dal dialogo guidato"),
                                  "senza correzioni in sospeso la scheda lo accetta senza chiedere")

                    Assert.AreEqual("Anna Rossi", Casella(pannello, "txtNome").Text, "è nei campi")
                    Assert.HasCount(1, Elenco(pannello, "lstCompetenze").Items, "con quello che ha raccolto")
                    Assert.IsTrue(pannello.HaModificheNonSalvate, "resta da salvare")
                    Assert.IsTrue(Bottone(pannello, "btnSalva").Enabled, "e il bottone è acceso")

                    Assert.AreEqual(primaDi, archivio.Carica().ComeTesto(), "ma su disco non si è mosso niente")
                    Assert.HasCount(1, archivio.Versioni(), "e nello storico non è comparso nulla")
                End Sub)
        End Sub

        ''' <summary>
        ''' Prepara una cartella dati con dentro il profilo del banco e un pannello già
        ''' collegato al motore: è lo stato in cui l'utente trova la scheda quando apre
        ''' l'applicazione avendo già un profilo.
        ''' </summary>
        Private Shared Sub ConProfiloSalvato(prova As Action(Of TrovaLavoro.PannelloProfilo, ArchivioProfilo))

            ConCartellaTemporanea(
                Sub(radice)
                    Dim archivio As New ArchivioProfilo(New CartellaDati(radice))
                    archivio.Salva(ProfiloDiProva())

                    Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                          pannello As New TrovaLavoro.PannelloProfilo()

                        ' Senza handle le linguette non sono «realizzate» e i bottoni che
                        ' stanno dentro non rispondono al clic: nel banco vanno create a
                        ' mano, perché qui il pannello non è appeso a nessuna finestra.
                        pannello.CreateControl()

                        pannello.Collega(contesto)
                        prova(pannello, archivio)
                    End Using
                End Sub)

        End Sub

        Private Shared Function ProfiloDiProva() As TrovaLavoro.Dati.Profilo
            Return TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo())
        End Function

        Private Shared Function Bottone(pannello As Control, nome As String) As Button
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

        ''' <summary>Una cartella dati usa e getta: quella vera dell'utente non si tocca.</summary>
        Private Shared Sub ConCartellaTemporanea(prova As Action(Of String))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "pannello-profilo-" & Guid.NewGuid().ToString("N"))
            Try
                prova(radice)
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

        Private Shared Function Etichetta(pannello As Control, nome As String) As Label
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

        Private Shared Function Casella(pannello As Control, nome As String) As TextBox
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), TextBox)
        End Function

        Private Shared Function Elenco(pannello As Control, nome As String) As ListBox
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), ListBox)
        End Function

    End Class

End Namespace
