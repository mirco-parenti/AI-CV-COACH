Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
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

                Dim azioni As Panel = Fascia(pannello)

                Assert.IsGreaterThanOrEqualTo(188, azioni.Height,
                                              "alta almeno quanto il logo sfonda nell'area centrale")
                Assert.AreEqual(273, azioni.Padding.Left, "e i bottoni cominciano dopo la sua larghezza")

                Dim primo As Button = Bottone(pannello, "btnImporta")
                Assert.IsGreaterThanOrEqualTo(273, primo.Left, "nessun bottone sotto il logo")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaFasciaCresceQuantoServeAiComandi()
            ' In compatta il logo sfonda molto meno (68 px), e i comandi in quello spazio
            ' non ci stanno: la fascia deve <b>crescere</b>, non schiacciarli uno sopra
            ' l'altro. Fino alla 0.3.018 l'altezza era quella del logo e basta, e i
            ' bottoni si sovrapponevano in silenzio.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.ImpostaIngombroLogo(New Size(130, 68))

                Dim azioni As Panel = Fascia(pannello)

                Assert.IsGreaterThan(68, azioni.Height, "più alta dell'ingombro, perché i comandi lo chiedono")
                Assert.AreEqual(142, azioni.Padding.Left, "e il rientro segue il logo compatto")
            End Using
        End Sub

        <TestMethod>
        Public Sub IComandiNonSiSovrappongonoMaiAQualunqueLarghezza()
            ' È il difetto che questa disposizione chiude, e l'unico invariante che vale
            ' la pena tenere fermo: alla larghezza minima della finestra i bottoni
            ' arrivavano a coprirsi per 676 px. Non si vedeva perché l'applicazione si
            ' apre massimizzata — ed è per questo che è rimasto lì per tre tappe.
            Using pannello As New TrovaLavoro.PannelloProfilo()

                For Each larghezza As Integer In {1106, 1150, 1250, 1350, 1600, 1920}

                    ' Sotto i 1350 px di finestra il logo passa in compatta (cap. 03.5), e
                    ' l'ingombro della fascia cambia con lui: il caso stretto non si ricava
                    ' da quello largo.
                    Dim ingombro As Size = If(larghezza < 1350, New Size(130, 68), New Size(261, 188))

                    pannello.Width = larghezza
                    pannello.ImpostaIngombroLogo(ingombro)

                    Dim comandi As Button() = ComandiDellaFascia(pannello)

                    For primo As Integer = 0 To comandi.Length - 2
                        For secondo As Integer = primo + 1 To comandi.Length - 1
                            Assert.IsFalse(comandi(primo).Bounds.IntersectsWith(comandi(secondo).Bounds),
                                           $"a {larghezza} px «{comandi(primo).Text}» copre «{comandi(secondo).Text}»")
                        Next
                    Next

                    Dim azioni As Panel = Fascia(pannello)
                    For Each comando As Button In comandi
                        Assert.IsGreaterThanOrEqualTo(azioni.Padding.Left, comando.Left,
                                                      $"a {larghezza} px «{comando.Text}» finisce sotto il logo")
                        Assert.IsGreaterThanOrEqualTo(comando.Bounds.Right, azioni.ClientSize.Width,
                                                      $"a {larghezza} px «{comando.Text}» esce dalla fascia")
                        Assert.IsGreaterThanOrEqualTo(0, comando.Top,
                                                      $"a {larghezza} px «{comando.Text}» esce dalla fascia in alto")
                    Next

                Next
            End Using
        End Sub

        <TestMethod>
        Public Sub AFinestraLargaIComandiRestanoDovErano()
            ' La disposizione nuova non deve cambiare il caso di sempre: l'applicazione si
            ' apre massimizzata, e lì i comandi stanno su una riga sola — il profilo a
            ' sinistra, le uscite a destra.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.Width = 1890
                pannello.ImpostaIngombroLogo(New Size(261, 188))

                Dim fondo As Integer = Bottone(pannello, "btnSalva").Top

                For Each nome As String In {"btnImporta", "btnImportaDaSito", "btnDialogo",
                                            "btnAggiornamento", "btnGeneraCv1", "btnEsportaBackup"}
                    Assert.AreEqual(fondo, Bottone(pannello, nome).Top, $"«{nome}» sulla stessa riga di «Salva profilo»")
                Next

                Assert.IsGreaterThan(Bottone(pannello, "btnEliminaProfilo").Top, fondo,
                                     "e l'eliminazione definitiva una riga sopra, mai in fila con loro")
            End Using
        End Sub

        <TestMethod>
        Public Sub LEliminazioneDefinitivaNonStaMaiInFilaConGliAltri()
            ' Il vuoto intorno è la sua prima difesa (cap. 11.5): non deve finire sotto il
            ' dito di chi sta salvando, a nessuna larghezza. Prima stava in riga con gli
            ' altri quando lo spazio mancava — cioè proprio quando i bottoni erano più
            ' vicini fra loro.
            Using pannello As New TrovaLavoro.PannelloProfilo()

                For Each larghezza As Integer In {1106, 1150, 1350, 1920}

                    pannello.Width = larghezza
                    pannello.ImpostaIngombroLogo(If(larghezza < 1350, New Size(130, 68), New Size(261, 188)))

                    Dim critico As Button = Bottone(pannello, "btnEliminaProfilo")

                    For Each comando As Button In ComandiDellaFascia(pannello)
                        If comando Is critico Then Continue For
                        Assert.AreNotEqual(critico.Top, comando.Top,
                                           $"a {larghezza} px «{comando.Text}» finisce sulla sua riga")
                    Next

                Next
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

        ' ==================================================================
        ' Correggere una voce a mano, tasto per tasto
        ' ==================================================================

        <TestMethod>
        Public Sub CorreggereUnCampoNonRicaricaGliAltriDellaScheda()

            ' Il difetto che questo collaudo sorveglia si vedeva solo digitando:
            ' scrivendo «abc» nel ruolo restava scritto «cba». La causa sta due passi
            ' più in là del sintomo. Aggiornare la riga dell'elenco vuol dire
            ' assegnarla, e WinForms per assegnarla la toglie e la rimette: nel mezzo
            ' la voce scelta sparisce e poi torna, e chi ascolta la selezione ricarica
            ' i campi. Ricaricarli mentre l'utente scrive gli riporta il cursore a
            ' zero, così la lettera dopo entra a sinistra della precedente.
            '
            ' Il cursore non si misura in un banco senza tastiera; il ricaricamento
            ' sì, e è lui la causa: se mentre correggo il ruolo anche le altre caselle
            ' della scheda vengono riscritte, quel giro è ripartito.
            Using pannello As New TrovaLavoro.PannelloProfilo()

                ' Senza handle la riga non passa dal controllo di Windows, e il giro
                ' che rompe tutto non parte nemmeno: il collaudo sarebbe verde per il
                ' motivo sbagliato.
                pannello.CreateControl()
                pannello.Mostra(ProfiloDiProva())

                Dim durata As TextBox = Casella(pannello, "txtDurata")
                Dim tipo As TextBox = Casella(pannello, "txtTipo")

                Dim ricariche As Integer = 0
                AddHandler durata.TextChanged, Sub() ricariche += 1
                AddHandler tipo.TextChanged, Sub() ricariche += 1

                Casella(pannello, "txtRuolo").Text = "Capo reparto"

                Assert.AreEqual(0, ricariche,
                                "correggendo il ruolo, le altre caselle della scheda non si toccano")
            End Using

        End Sub

        <TestMethod>
        Public Sub CorreggereIlRuoloAggiornaLaRigaDellElenco()

            ' La cintura del collaudo qui sopra: si poteva stare fermi anche non
            ' aggiornando più niente, e sarebbe stata una cura peggiore del male.
            ' L'elenco deve continuare a dire quello che c'è nella scheda.
            Using pannello As New TrovaLavoro.PannelloProfilo()

                pannello.CreateControl()
                pannello.Mostra(ProfiloDiProva())

                Casella(pannello, "txtRuolo").Text = "Capo reparto"

                Assert.AreEqual("Capo reparto — Romagna Logistica S.r.l.",
                                Elenco(pannello, "lstLavoro").Items(0).ToString(),
                                "la riga segue il campo")
                Assert.AreEqual(0, Elenco(pannello, "lstLavoro").SelectedIndex,
                                "e la voce scelta resta quella")
            End Using

        End Sub

        <TestMethod>
        Public Sub SenzaVoceSceltaICampiDellaSchedaNonSiScrivono()

            ' L'altra metà del difetto, e la più silenziosa: con l'elenco vuoto i campi
            ' erano scrivibili, ma quello che ci si scriveva non aveva dove andare —
            ' e al primo «Aggiungi» spariva senza che nessuno lo dicesse. Un campo che
            ' non può tenere niente si spegne, come l'«Elimina» accanto.
            Using pannello As New TrovaLavoro.PannelloProfilo()

                pannello.CreateControl()
                pannello.Mostra(New TrovaLavoro.Dati.Profilo())

                For Each nome As String In {"txtRuolo", "txtAzienda", "txtDurata", "txtTipo",
                                            "txtCosaFacevoLavoro", "txtQuando", "txtConChi",
                                            "txtCosaFacevoInformale", "txtCompetenza",
                                            "txtTitoloStudio", "txtIstituto", "txtAnno"}
                    Assert.IsTrue(Casella(pannello, nome).ReadOnly,
                                  $"«{nome}» non ha una voce dove scrivere")
                Next

                ' I dati personali non stanno in un elenco: quelli restano scrivibili
                ' anche su un profilo appena nato, ed è da lì che si comincia.
                For Each nome As String In {"txtNome", "txtEmail", "txtTelefono",
                                            "txtDomicilio", "txtLink"}
                    Assert.IsFalse(Casella(pannello, nome).ReadOnly,
                                   $"«{nome}» si scrive sempre")
                Next
            End Using

        End Sub

        <TestMethod>
        Public Sub ConLaVoceSceltaICampiDellaSchedaTornanoScrivibili()

            ' La controprova: spenti quando non servono, accesi quando servono. Senza
            ' questa, «spegnili tutti» passerebbe il collaudo qui sopra.
            Using pannello As New TrovaLavoro.PannelloProfilo()

                pannello.CreateControl()
                pannello.Mostra(ProfiloDiProva())

                For Each nome As String In {"txtRuolo", "txtAzienda", "txtDurata", "txtTipo",
                                            "txtCosaFacevoLavoro", "txtQuando", "txtConChi",
                                            "txtCosaFacevoInformale", "txtCompetenza",
                                            "txtTitoloStudio", "txtIstituto", "txtAnno"}
                    Assert.IsFalse(Casella(pannello, nome).ReadOnly,
                                   $"«{nome}» ha la sua voce e si scrive")
                Next
            End Using

        End Sub

        ' ==================================================================
        ' Il CV che arriva da una pagina (cap. 06.7 — T5d)
        ' ==================================================================

        <TestMethod>
        Public Async Function IlCvLettoDaUnaPaginaRiempieLaSchedaMaNonIlDisco() As Task

            ' È l'altra porta dello stesso mestiere: cambia da dove viene il testo, e da
            ' lì in poi la strada è quella dell'import da file — turno «importa_cv»,
            ' campi proposti, testo a disposizione, e su disco niente finché non si salva.
            Dim aiFinta As New StrutturatoreFinto()
            aiFinta.Dara(ProfiloDiRitorno())

            Await ConImportFintoAsync(aiFinta,
                Async Function(pannello, archivio) As Task

                    Dim primaDi As String = archivio.Carica().ComeTesto()

                    Await pannello.ImportaDaTestoAsync(TestoDiUnaPaginaProfilo(), "da linkedin.com")

                    ' All'AI è andato il turno del CV, col testo della pagina e nient'altro.
                    Assert.HasCount(1, aiFinta.Chiamate, "una sola chiamata")
                    Assert.AreEqual("importa_cv", aiFinta.Chiamate.Single().Turno,
                                    "lo stesso turno del CV in PDF: la fonte non cambia il prompt")
                    Assert.AreEqual(TestoDiUnaPaginaProfilo(), aiFinta.Chiamate.Single().Risposta)

                    ' Il profilo proposto è nei campi.
                    Assert.AreEqual("Mirco Parenti", Casella(pannello, "txtNome").Text)
                    Assert.AreEqual("Chiavari", Casella(pannello, "txtDomicilio").Text)
                    Assert.HasCount(1, Elenco(pannello, "lstCompetenze").Items)

                    ' E il testo originale resta a disposizione: è la prova con cui si
                    ' controlla che nel profilo non sia comparso nulla che lì non c'era.
                    Assert.Contains("Rossi S.p.A.", Casella(pannello, "txtTestoLetto").Text)

                    Assert.IsTrue(pannello.HaModificheNonSalvate, "resta da salvare")
                    Assert.Contains("linkedin.com", Etichetta(pannello, "lblStatoProfilo").Text,
                                    "e la scheda dice da dove arriva")

                    Assert.AreEqual(primaDi, archivio.Carica().ComeTesto(),
                                    "ma su disco non si è mosso niente")
                    Assert.HasCount(1, archivio.Versioni(), "e nello storico non è comparso nulla")

                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaChiaveIlCvDallaPaginaDiceCosaManca() As Task

            ' Il comando che l'ha mandato sta in un altro pannello, che di chiavi non sa
            ' niente: se qui non si dicesse perché non è successo nulla, l'utente
            ' cambierebbe pannello e troverebbe la scheda com'era, senza una parola.
            Dim ilProfiloDiPrima As String = Nothing

            Await ConProfiloSalvatoAsync(
                Async Function(pannello, archivio) As Task

                    ilProfiloDiPrima = Casella(pannello, "txtNome").Text

                    Await pannello.ImportaDaTestoAsync(TestoDiUnaPaginaProfilo(), "da linkedin.com")

                    Assert.Contains("chiave", Etichetta(pannello, "lblStatoProfilo").Text)
                    Assert.AreEqual(ilProfiloDiPrima, Casella(pannello, "txtNome").Text,
                                    "e la scheda è rimasta com'era")
                    Assert.IsFalse(pannello.HaModificheNonSalvate, "niente da salvare")

                End Function)

        End Function

        <TestMethod>
        Public Async Function IlBottoneDelSitoChiedeIlBrowserSenzaLeggereNiente() As Task

            ' La scheda del profilo è dove si sceglie la strada, ma la lettura vera sta in
            ' P3: qui non c'è nessuna pagina aperta da leggere, e infatti non si legge —
            ' si chiede il browser e basta.
            Dim aiFinta As New StrutturatoreFinto()

            Await ConImportFintoAsync(aiFinta,
                Function(pannello, archivio) As Task

                    Dim chiesto As Integer = 0
                    AddHandler pannello.ImportDaSitoRichiesto, Sub(mittente, argomenti) chiesto += 1

                    Assert.IsTrue(Bottone(pannello, "btnImportaDaSito").Enabled,
                                  "con l'AI in casa la strada è aperta")

                    Bottone(pannello, "btnImportaDaSito").PerformClick()

                    Assert.AreEqual(1, chiesto, "la scheda ha chiesto il browser")
                    Assert.IsEmpty(aiFinta.Chiamate, "senza spendere una chiamata all'AI")
                    Assert.IsFalse(pannello.HaModificheNonSalvate, "e senza toccare il profilo")

                    Return Task.CompletedTask

                End Function)

        End Function

        <TestMethod>
        Public Sub LeDuePorteDellImportDiconoQualeDelleDueSono()

            ' Due bottoni che dicono entrambi «Importa CV» non si scelgono: ognuno deve
            ' dire da dove legge. E stanno vicini, perché sono lo stesso mestiere — con la
            ' fascia disposta come nell'applicazione vera, cioè dopo l'ingombro del logo.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.ImpostaIngombroLogo(New Size(261, 188))

                Dim daFile As Button = Bottone(pannello, "btnImporta")
                Dim daSito As Button = Bottone(pannello, "btnImportaDaSito")

                Assert.AreEqual("IMPORTA CV DA UN FILE", daFile.Text)
                Assert.AreEqual("IMPORTA CV DA LINKEDIN", daSito.Text)

                Assert.IsGreaterThanOrEqualTo(daFile.Right, daSito.Left,
                                              "il sito viene dopo il file, e non gli finisce sopra")
                Assert.AreEqual(daFile.Top, daSito.Top, "sulla stessa riga")
            End Using

        End Sub

        <TestMethod>
        Public Sub SenzaChiaveNemmenoLaPortaDelSitoSiApre()

            ' Manda a leggere una pagina, e leggere una pagina passa dall'AI come leggere
            ' un file: spenta l'una, spenta l'altra — se no si arriverebbe in fondo alla
            ' strada per sentirsi dire lì che mancava la chiave.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Assert.IsFalse(Bottone(pannello, "btnImportaDaSito").Enabled, "anche il sito è spento")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub EliminareIlProfiloSvuotaLaSchedaEIlDisco()
            ' Cap. 11.5. Qui si chiama il metodo e non il bottone: il bottone apre una
            ' finestra modale, e di quella il banco non può aspettare la fine.
            ConProfiloSalvato(
                Sub(pannello, archivio)
                    Dim avvisata As Boolean = False
                    AddHandler pannello.ProfiloEliminato, Sub() avvisata = True

                    Assert.IsTrue(Bottone(pannello, "btnEliminaProfilo").Enabled,
                                  "col profilo su disco si può eliminare")

                    Assert.IsTrue(pannello.EliminaIlProfilo(), "l'eliminazione riesce")

                    Assert.IsFalse(archivio.Esiste, "su disco non è rimasto niente")
                    Assert.IsEmpty(Casella(pannello, "txtNome").Text, "i campi sono vuoti")
                    Assert.IsEmpty(Elenco(pannello, "lstLavoro").Items, "e le liste anche")
                    Assert.IsFalse(Bottone(pannello, "btnSalva").Enabled, "niente da salvare")
                    Assert.IsFalse(Bottone(pannello, "btnEliminaProfilo").Enabled, "e niente altro da eliminare")
                    Assert.Contains("eliminato", Etichetta(pannello, "lblStatoProfilo").Text,
                                    "la scheda dice cos'è successo")
                    Assert.Contains("candidature", Etichetta(pannello, "lblStatoProfilo").Text,
                                    "e dice anche cosa è rimasto")
                    Assert.IsTrue(avvisata, "la finestra viene avvertita, per svuotare il resto dell'applicazione")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaNienteDaEliminareIlBottoneESpento()
            ' Un bottone rosso che non ha niente da fare insegna solo a non fidarsi del
            ' colore. Ma quel che si sta scrivendo adesso è già qualcosa da buttare via.
            ConCartellaTemporanea(
                Sub(radice)
                    Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                          pannello As New TrovaLavoro.PannelloProfilo()

                        pannello.Collega(contesto)

                        Assert.IsFalse(Bottone(pannello, "btnEliminaProfilo").Enabled,
                                       "nessun profilo, niente da eliminare")

                        Casella(pannello, "txtNome").Text = "Luca Ferrari"

                        Assert.IsTrue(Bottone(pannello, "btnEliminaProfilo").Enabled,
                                      "ma quello che si sta scrivendo si può buttare via lo stesso")

                        pannello.EliminaIlProfilo()

                        Assert.IsEmpty(Casella(pannello, "txtNome").Text, "e la scheda si svuota")
                        Assert.Contains("non c'era ancora niente", Etichetta(pannello, "lblStatoProfilo").Text,
                                        "senza far temere una perdita che non c'è stata")
                    End Using
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LEliminazioneNonStaSottoIlDitoDelSalvataggio()
            ' Cap. 03.6: il solo bottone del pannello da cui non si torna indietro non
            ' sta in fila con quello che si preme cento volte. Quando la fascia è alta
            ' (modalità piena, cap. 03.5) sale di una riga, con il suo vuoto intorno.
            Using pannello As New TrovaLavoro.PannelloProfilo()
                pannello.ImpostaIngombroLogo(New Size(261, 188))

                Dim elimina As Button = Bottone(pannello, "btnEliminaProfilo")
                Dim salva As Button = Bottone(pannello, "btnSalva")

                Assert.IsLessThan(salva.Top, elimina.Bottom, "sta su una riga più in alto del salva")
                Assert.IsGreaterThanOrEqualTo(24, salva.Top - elimina.Bottom, "con del vuoto in mezzo")
                Assert.AreEqual(salva.Right, elimina.Right, "allineato al margine destro come gli altri")
            End Using
        End Sub

        ''' <summary>Quel che l'AI risponde sul testo della pagina: la forma di «importa_cv».</summary>
        Private Shared Function ProfiloDiRitorno() As String

            Return "{""nome"": ""Mirco Parenti""," &
                   " ""contatti"": {""email"": """", ""telefono"": """", ""citta"": ""Chiavari"", ""link"": """"}," &
                   " ""patente"": {""ha"": """", ""categorie"": []}," &
                   " ""esperienze_formali"": [{""ruolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A.""," &
                   " ""durata"": ""2023-2024"", ""cosa_facevo"": ""Carico e scarico merci"", ""tipo"": """"}]," &
                   " ""esperienze_informali"": []," &
                   " ""competenze"": [""Uso del muletto""]," &
                   " ""formazione"": [{""titolo"": ""Perito elettronico"", ""istituto"": ""ITIS Marconi""," &
                   " ""anno"": ""2019""}]}"

        End Function

        ''' <summary>Una pagina profilo come la legge il browser: il percorso e la fuffa intorno.</summary>
        Private Shared Function TestoDiUnaPaginaProfilo() As String

            Return "Mirco Parenti" & vbLf &
                   "Perito elettronico — Chiavari, Liguria" & vbLf &
                   "Esperienza" & vbLf &
                   "Magazziniere presso Rossi S.p.A. — 2023-2024." & vbLf &
                   "Persone che potresti conoscere · Annunci"

        End Function

        ''' <summary>
        ''' Come <see cref="ConProfiloSalvato"/>, ma con un lettore di CV <b>finto</b> in
        ''' mano al pannello: è così che un import intero si prova senza chiave e senza
        ''' rete (lo stesso gancio che P4 ha per la pipeline).
        ''' </summary>
        Private Shared Async Function ConImportFintoAsync(
            aiFinta As StrutturatoreFinto,
            prova As Func(Of TrovaLavoro.PannelloProfilo, ArchivioProfilo, Task)) As Task

            Await ConPannelloCollegatoAsync(New ImportProfilo(aiFinta), prova)

        End Function

        ''' <summary>Lo stesso, ma col motore vero: senza chiave, l'import non esiste.</summary>
        Private Shared Async Function ConProfiloSalvatoAsync(
            prova As Func(Of TrovaLavoro.PannelloProfilo, ArchivioProfilo, Task)) As Task

            Await ConPannelloCollegatoAsync(Nothing, prova)

        End Function

        Private Shared Async Function ConPannelloCollegatoAsync(
            importCv As ImportProfilo,
            prova As Func(Of TrovaLavoro.PannelloProfilo, ArchivioProfilo, Task)) As Task

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "pannello-profilo-" & Guid.NewGuid().ToString("N"))

            Try
                Dim archivio As New ArchivioProfilo(New CartellaDati(radice))
                archivio.Salva(ProfiloDiProva())

                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      pannello As New TrovaLavoro.PannelloProfilo()

                    pannello.CreateControl()
                    pannello.Collega(contesto, importCv)

                    Await prova(pannello, archivio)
                End Using

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Function

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

        ''' <summary>La fascia dei comandi in fondo al pannello.</summary>
        Private Shared Function Fascia(pannello As Control) As Panel
            Return DirectCast(pannello.Controls.Find("pnlAzioni", searchAllChildren:=True).Single(), Panel)
        End Function

        ''' <summary>
        ''' Tutti i bottoni della fascia, chiesti a lei e non elencati a mano: un comando
        ''' aggiunto domani entra da solo nei collaudi che verificano che non si sovrappongano.
        ''' </summary>
        Private Shared Function ComandiDellaFascia(pannello As Control) As Button()
            Return Fascia(pannello).Controls.OfType(Of Button)().ToArray()
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
