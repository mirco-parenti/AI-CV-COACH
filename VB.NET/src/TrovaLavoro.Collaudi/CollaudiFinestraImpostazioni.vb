Imports System.IO
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi della finestra delle Impostazioni, il pannello P8 (T9b, cap. 03).
    ''' </summary>
    ''' <remarks>
    ''' <para>Quel che qui può rompersi davvero non è come sono disposti i controlli, ma
    ''' tre promesse. La prima: le preferenze <b>si salvano da sé</b>, e chi le usa le vede
    ''' subito — è la ragione per cui la finestra non ha un OK. La seconda, il suo rovescio:
    ''' <b>aprire</b> la finestra non deve salvare niente, o il solo guardare le
    ''' impostazioni riscriverebbe il file. La terza: la chiave API si <b>riconosce senza
    ''' rileggerla</b> (cap. 11.3).</para>
    ''' <para>Come le altre finestre modali, si costruisce e si interroga senza mostrarla.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraImpostazioni

        Private Const ChiaveFinta As String = "sk-ant-finta-0000-CODA"

        ''' <summary>
        ''' Il fondo è l'avorio delle destinazioni, non il bianco delle finestre.
        ''' </summary>
        ''' <remarks>
        ''' Le Impostazioni sono la settima porta del menu e si aprono in una finestra solo
        ''' per come sono fatte dentro (cap. 03.2): dal 2026-08-31, quando l'avorio è
        ''' entrato nelle sei pagine, restare bianche voleva dire essere l'unica
        ''' destinazione fredda. È un colore, e un colore sbagliato non rompe niente: se
        ''' non lo guarda il banco, se ne accorge solo chi apre proprio questa finestra.
        ''' </remarks>
        <TestMethod>
        Public Sub IlFondoEQuelloDelleDestinazioni()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        Assert.AreEqual(StileApp.FondoCasella, finestra.BackColor,
                                        "la finestra")

                        For Each nome As String In {"pnlContenuto", "pnlFascia", "chkRifinitura"}
                            Assert.AreEqual(
                                StileApp.FondoCasella,
                                finestra.Controls.Find(nome, searchAllChildren:=True).Single().BackColor,
                                $"«{nome}» è rimasto bianco")
                        Next

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub AprirlaNonScriveNiente()

            ConMotore(
                Sub(contesto)

                    Assert.IsFalse(contesto.ArchivioImpostazioni.Esiste, "prima non c'è nessun file")

                    Using finestra As New FinestraImpostazioni(contesto)
                        Assert.IsNotNull(finestra)
                    End Using

                    ' Riempire i controlli fa scattare gli eventi di cambiamento: se non
                    ' fossero zittiti, il solo aprire le Impostazioni scriverebbe il file
                    ' e la prossima lettura direbbe «viene dal file» invece di «predefiniti».
                    Assert.IsFalse(contesto.ArchivioImpostazioni.Esiste,
                                   "guardare le impostazioni non è cambiarle")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub CambiareLaLinguaSalvaSubitoEIlContestoLoSa()

            ConMotore(
                Sub(contesto)

                    Assert.AreEqual(LinguaDocumenti.Italiano, contesto.Impostazioni.LinguaPredefinita)

                    Using finestra As New FinestraImpostazioni(contesto)
                        Tendina(finestra, "cmbLingua").SelectedIndex = 1
                    End Using

                    Assert.IsTrue(contesto.ArchivioImpostazioni.Esiste, "il file è nato")
                    Assert.AreEqual(LinguaDocumenti.Inglese, contesto.Impostazioni.LinguaPredefinita,
                                    "e il contesto ha già riletto: nessuno deve riavviare")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SpegnereLaRifinituraSalvaSubitoELaRifinituraSiSpegne()

            ConMotore(
                Sub(contesto)

                    Assert.IsTrue(contesto.Rifinitura.Accesa, "di fabbrica è accesa")

                    Using finestra As New FinestraImpostazioni(contesto)
                        Casella(finestra, "chkRifinitura").Checked = False
                    End Using

                    Assert.IsFalse(contesto.Impostazioni.RifinituraAttiva)

                    ' Il punto vero: non che il file dica «false», ma che il mestiere che
                    ' rifinisce lo sappia — ed è la stessa istanza di prima, non una nuova.
                    Assert.IsFalse(contesto.Rifinitura.Accesa,
                                   "l'interruttore vale subito, senza riavvio")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub LaChiaveSiRiconosceMaNonSiRilegge()

            ConMotore(
                Sub(contesto)

                    contesto.Segreti.SalvaChiaveApi(ChiaveFinta)

                    Using finestra As New FinestraImpostazioni(contesto)

                        Dim detto As String = Etichetta(finestra, "lblStatoChiave").Text

                        ' Della chiave si mostra quanto basta a riconoscerla, e nulla di più
                        ' (cap. 11.3): una finestra che la ristampasse in chiaro sarebbe un
                        ' modo elegante di lasciarla su uno screenshot.
                        Assert.DoesNotContain(ChiaveFinta, detto, "la chiave non si rilegge mai per intero")
                        Assert.DoesNotContain("sk-ant-finta", detto, "nemmeno il suo inizio")
                        Assert.Contains("CODA", detto, "ma la coda sì, o non si riconoscerebbe")
                        Assert.Contains("•", detto, "il resto è coperto")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SenzaChiaveLoDiceEIlBottoneCambiaMestiere()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        Assert.Contains("Nessuna chiave", Etichetta(finestra, "lblStatoChiave").Text)
                        Assert.Contains("Scrivi", Comando(finestra, "btnCambiaChiave").Text,
                                        "non si «cambia» una chiave che non c'è")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IBottoniDistruttiviSonoSpentiQuandoNonCENiente()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        ' Un bottone rosso che non ha niente da fare insegna solo a non
                        ' fidarsi del colore (cap. 03.6).
                        Assert.IsFalse(Comando(finestra, "btnSvuotaNavigazione").Enabled,
                                       "non c'è nessuna cartella di navigazione")
                        Assert.IsFalse(Comando(finestra, "btnEliminaTutto").Enabled,
                                       "e non c'è nessun dato da eliminare")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub ConDeiDatiIlBottoneCriticoSiAccende()

            ConMotore(
                Sub(contesto)

                    contesto.Segreti.SalvaChiaveApi(ChiaveFinta)

                    Using finestra As New FinestraImpostazioni(contesto)
                        Assert.IsTrue(Comando(finestra, "btnEliminaTutto").Enabled,
                                      "ora qualcosa da eliminare c'è")
                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SalvareUnaPreferenzaAccendeIlBottoneCheEliminaTutto()

            ' Trovato dal vivo il 2026-08-21, e nessun collaudo lo vedeva: i collaudi
            ' guardavano lo stato dei bottoni all'apertura, quando la cartella era ancora
            ' vuota. Su una cartella dati nuova, impostazioni.json è il primo dato che ci
            ' sia mai stato — e chi l'ha appena creato deve poterlo mandare via senza
            ' chiudere e riaprire la finestra.
            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        Assert.IsFalse(Comando(finestra, "btnEliminaTutto").Enabled,
                                       "all'apertura non c'era niente")

                        Tendina(finestra, "cmbLingua").SelectedIndex = 1

                        Assert.IsTrue(Comando(finestra, "btnEliminaTutto").Enabled,
                                      "il file appena scritto è un dato come gli altri")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub DiceCosaGiraSottoIlCofano()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        Assert.Contains("predefiniti", Etichetta(finestra, "lblModelli").Text,
                                        "da dove viene la scelta dei modelli")

                        ' Fino alla 1.0 questo collaudo difendeva il contrario — nessun
                        ' controllo per cambiare i modelli, si toccano da modelli.json — e
                        ' la revisione del giro D ha rovesciato la decisione: chi il
                        ' programma non l'ha scritto non apre un file JSON. Il file resta
                        ' il posto dove la scelta vive (cap. 11.6), e le due tendine hanno
                        ' i loro collaudi più sotto.
                        Assert.IsNotEmpty(finestra.Controls.Find("cmbModelloSemplice", searchAllChildren:=True),
                                          "i modelli adesso si scelgono da qui")

                        ' Quel che di sola lettura è rimasto: il pool si sigilla dal repo e
                        ' non da un eseguibile distribuito, e la taratura non compare affatto.
                        Assert.IsNotEmpty(Etichetta(finestra, "lblPool").Text, "il pool si legge")
                        Assert.IsEmpty(finestra.Controls.Find("txtTaratura", searchAllChildren:=True),
                                       "e la taratura non compare affatto")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub LaCartellaDatiSiMostraMaNonSiCambia()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        Assert.Contains(contesto.Cartella.Radice, Etichetta(finestra, "lblCartellaDati").Text,
                                        "dove sono i miei file si legge")
                        Assert.Contains("--dati", Etichetta(finestra, "lblCartellaDati").Text,
                                        "e come si sceglie")

                        ' Cambiarla a caldo vorrebbe dire spostare file sotto i piedi di chi
                        ' ci sta scrivendo, col lucchetto già preso (cap. 09.4).
                        Assert.IsEmpty(finestra.Controls.Find("btnCambiaCartellaDati", searchAllChildren:=True))

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub ChiedereIDocumentiNonLiGestisceQui()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        Assert.IsFalse(finestra.VuoleGestireIDocumenti, "all'apertura non ha chiesto niente")

                        ' Il giro dei documenti vuole l'AI e la sa aspettare: è mestiere di
                        ' P7, e queste Impostazioni ci mandano invece di rifarlo.
                        finestra.ChiediDiGestireIDocumenti()

                        Assert.IsTrue(finestra.VuoleGestireIDocumenti, "l'ha chiesto a chi di dovere")

                    End Using

                End Sub)

        End Sub

        ''' <summary>Un contesto vero su una cartella temporanea, che si porta via tutto alla fine.</summary>
        Private Shared Sub ConMotore(prova As Action(Of ContestoApp))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "finestra-impostazioni-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, ChiaveFinta)
                    prova(contesto)
                End Using
            Finally
                CartelleDiProva.PortaVia(radice)
            End Try

        End Sub

        <TestMethod>
        Public Sub LaSogliaDelPromemoriaSiCambiaESiSalvaSubito()

            ConMotore(
                Sub(contesto)

                    Assert.AreEqual(14, contesto.Impostazioni.GiorniFollowUp, "il valore di casa")

                    Using finestra As New FinestraImpostazioni(contesto)

                        Dim giorni As NumericUpDown = Numerico(finestra, "numFollowUp")

                        Assert.AreEqual(14D, giorni.Value, "la finestra mostra quel che vale adesso")
                        Assert.AreEqual(0D, giorni.Minimum, "zero spegne il promemoria, e si deve poter scrivere")
                        Assert.AreEqual(CDec(Impostazioni.GiorniFollowUpMassimi), giorni.Maximum)

                        giorni.Value = 7D
                    End Using

                    Assert.AreEqual(7, contesto.Impostazioni.GiorniFollowUp,
                                    "salvata appena cambiata, senza OK da premere")

                End Sub)

        End Sub

        Private Shared Function Numerico(finestra As Control, nome As String) As NumericUpDown
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), NumericUpDown)
        End Function

        Private Shared Function Etichetta(finestra As Control, nome As String) As Label
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

        Private Shared Function Comando(finestra As Control, nome As String) As Button
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Tendina(finestra As Control, nome As String) As ComboBox
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), ComboBox)
        End Function

        Private Shared Function Casella(finestra As Control, nome As String) As CheckBox
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), CheckBox)
        End Function

        ' ==================================================================
        ' La barra di scorrimento che non ne chiama una seconda (2026-08-24)
        ' ==================================================================

        ''' <summary>
        ''' Curato lo scorrimento verticale (R11), a 150% compariva <b>anche</b> quello
        ''' orizzontale: la barra verticale si prende una fetta di larghezza, e il
        ''' contenuto messo in fila senza saperlo le finisce sotto. Nessun comando diventa
        ''' irraggiungibile — è rifinitura — ma una barra che non ha niente da mostrare
        ''' dice a chi guarda che qualcosa è fuori posto.
        ''' </summary>
        <TestMethod>
        Public Sub QuandoSiScorreNienteFinisceSottoLaBarra()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        ' Un'altezza che il contenuto non può rispettare: è la condizione in
                        ' cui la finestra scorre, e l'unica in cui quella barra esiste.
                        finestra.DisponiIn(200)

                        Assert.IsTrue(finestra.SiScorre, "con così poco spazio si scorre")

                        Dim quantoResta As Integer =
                            finestra.ClientSize.Width - SystemInformation.VerticalScrollBarWidth

                        ' A scorrere è il pannello del contenuto, non più la finestra: la
                        ' barra si prende la sua fetta lì dentro (2026-08-27).
                        Dim contenuto As Control =
                            finestra.Controls.Find("pnlContenuto", searchAllChildren:=False).Single()

                        For Each controllo As Control In contenuto.Controls
                            Assert.IsTrue(controllo.Right <= quantoResta,
                                          $"«{controllo.Name}» arriva a {controllo.Right}, " &
                                          $"oltre i {quantoResta} che restano accanto alla barra")
                        Next

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub ChiudiRestaInVistaAncheQuandoIlContenutoNonCiSta()

            ' Guardando la finestra a occhio, il 2026-08-27: appena aperta su uno schermo
            ' da 1080, «Chiudi» stava 145 pixel sotto il bordo — c'era, era acceso, e
            ' l'elenco dei controlli lo confermava, ma per premerlo bisognava prima
            ' scoprire che la finestra si scorreva. Ora vive nella fascia, che non scorre.
            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        finestra.DisponiIn(200)
                        finestra.PerformLayout()

                        Dim chiudi As Button = Comando(finestra, "btnChiudi")
                        Dim fascia As Control = chiudi.Parent

                        Assert.IsTrue(finestra.SiScorre, "con così poco spazio si scorre")
                        Assert.AreEqual("pnlFascia", fascia.Name,
                                        "«Chiudi» non sta nel pannello che scorre")
                        Assert.IsTrue(fascia.Top > 0,
                                      "la fascia ha preso il suo posto in fondo alla finestra")
                        Assert.IsTrue(fascia.Top + chiudi.Bottom <= finestra.ClientSize.Height,
                                      $"«Chiudi» arriva a {fascia.Top + chiudi.Bottom}, " &
                                      $"oltre i {finestra.ClientSize.Height} della finestra")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub QuandoCiStaTuttoNonSiRiservaNiente()

            ' Il rovescio, e serve: una riserva presa sempre stringerebbe la finestra di
            ' tre pixel per una barra che non c'è, tutte le volte.
            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        finestra.DisponiIn(4000)

                        Assert.IsFalse(finestra.SiScorre, "con tutto questo spazio non si scorre")

                        Assert.AreEqual(finestra.ClientSize.Width - StileApp.MargineRiquadro,
                                        Comando(finestra, "btnChiudi").Right,
                                        "«Chiudi» sta al suo margine, senza riserve per una barra che non c'è")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub LEliminazioneDiTuttoHaUnaFasciaTuttaSua()

            ' Cap. 11.5: il vuoto intorno è la prima difesa di un'azione critica, e in
            ' fascia dei comandi quella regola vale già — riga tutta sua, staccata,
            ' allineata dall'altra parte. Qui, fino al 2026-09-01, ce n'era solo metà: il
            ' bottone rosso scuro stava nella stessa colonna del rosso di sopra, alla
            ' stessa larghezza e a un dito di distanza, cioè proprio dove finisce un clic
            ' scivolato. Adesso ha il vuoto sopra e sotto, e non condivide la colonna.
            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        finestra.DisponiIn(4000)

                        Dim critico As Button = Comando(finestra, "btnEliminaTutto")
                        Dim distruttivo As Button = Comando(finestra, "btnSvuotaNavigazione")
                        Dim stato As Control = finestra.Controls.Find("lblStato", searchAllChildren:=True).Single()

                        Assert.AreEqual(LivelloBottone.Critico, critico.Tag, "è il livello 6")

                        Assert.IsGreaterThanOrEqualTo(distruttivo.Bottom + FasciaDeiComandi.StaccoDelCritico,
                                                      critico.Top, "il vuoto sopra")
                        Assert.IsGreaterThanOrEqualTo(critico.Bottom + FasciaDeiComandi.StaccoDelCritico,
                                                      stato.Top, "e il vuoto sotto")

                        Assert.IsGreaterThanOrEqualTo(distruttivo.Right, critico.Left,
                                                      "e non sta nella colonna del bottone rosso di sopra")

                    End Using

                End Sub)

        End Sub

        ' ==================================================================
        ' Quanto è costato (2026-08-27)
        ' ==================================================================

        Private Shared Function ContoDiProva(ParamArray righe As String()) As ContoDoppio
            Return ContoDelleChiamate.DalleRighe(
                righe, TrovaLavoro.Ai.Listino.Predefinito(), New Date(2026, 8, 27))
        End Function

        Private Shared Function RigaDiProva(modello As String, ingresso As Integer, uscita As Integer) As String
            Return $"2026-08-26 10:00:00;confronto;{modello};4000;{ingresso};{uscita};12,5;end_turn"
        End Function

        <TestMethod>
        Public Sub SenzaChiamateNonSiInventaUnoZero()

            Dim detto As String = FinestraImpostazioni.InParole(ContoDiProva())

            StringAssert.Contains(detto, "Nessuna chiamata", "si dice che non c'è ancora niente")
            Assert.IsFalse(detto.Contains("$0,00"), "e non si scrive un totale che sembra un fatto")

        End Sub

        <TestMethod>
        Public Sub IlContoDiceChiamateTokenESpesa()

            Dim detto As String = FinestraImpostazioni.InParole(
                ContoDiProva(RigaDiProva("claude-haiku-4-5", 1000000, 0)))

            StringAssert.Contains(detto, "1 chiamata", "una sola, e al singolare")
            StringAssert.Contains(detto, "$", "la spesa in dollari, che è la valuta della fattura")
            StringAssert.Contains(detto, "30 giorni", "e la finestra recente")

        End Sub

        <TestMethod>
        Public Sub SottoIlCentesimoNonSiScriveZero()

            ' «$0,00» si legge come «gratis», e non è la stessa cosa di «pochissimo».
            Dim detto As String = FinestraImpostazioni.InParole(
                ContoDiProva(RigaDiProva("claude-haiku-4-5", 100, 10)))

            StringAssert.Contains(detto, "meno di $0,01", "si dice quanto è piccolo, non zero")

        End Sub

        <TestMethod>
        Public Sub IlBucoSiDichiara()

            ' Un totale che tace su una parte delle chiamate sembra completo: è il modo
            ' più educato di dire una cifra sbagliata.
            Dim detto As String = FinestraImpostazioni.InParole(
                ContoDiProva(RigaDiProva("claude-haiku-4-5", 1000000, 0),
                             RigaDiProva("claude-domani-1", 9000000, 9000000)))

            StringAssert.Contains(detto, "non conosco il prezzo", "si dice che una non è valutata")
            StringAssert.Contains(detto, "stima", "e che il resto è comunque una stima")

        End Sub

        <TestMethod>
        Public Sub IlBottoneDelConteggioSiAccendeSoloSeCEQualcosa()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        ' Cartella dati appena nata: nessuna chiamata all'AI è mai partita.
                        Assert.IsFalse(Comando(finestra, "btnApriChiamate").Enabled,
                                       "non si apre un file che non esiste")
                        StringAssert.Contains(Etichetta(finestra, "lblConsumo").Text, "Nessuna chiamata")

                    End Using

                End Sub)

        End Sub

        ' ==================================================================
        ' Le tendine dei modelli (2026-08-27, dalla revisione del giro D)
        ' ==================================================================

        ''' <summary>Un elenco finto come quello che l'API restituisce.</summary>
        Private Shared Function ElencoFinto(ParamArray id As String()) As Func(Of Task(Of TrovaLavoro.Ai.EsitoElenco))
            Return Function() Task.FromResult(
                TrovaLavoro.Ai.EsitoElenco.Riuscito(id.Select(Function(uno) New TrovaLavoro.Ai.ModelloDisponibile(uno, "")).ToArray()))
        End Function

        <TestMethod>
        Public Sub AprirlaNonScriveModelliJson()

            ConMotore(
                Sub(contesto)

                    Assert.IsFalse(File.Exists(contesto.Cartella.FileModelli), "prima non c'è nessun file")

                    Using finestra As New FinestraImpostazioni(contesto)
                        Assert.IsNotNull(finestra)
                    End Using

                    ' Riempire una tendina fa scattare il suo evento: senza la guardia, il
                    ' solo guardare le impostazioni scriverebbe modelli.json e la
                    ' provenienza direbbe «dal file» invece di «predefiniti».
                    Assert.IsFalse(File.Exists(contesto.Cartella.FileModelli),
                                   "guardare i modelli non è cambiarli")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub LeTendineMostranoQuelChEInVigore()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        ' Prima ancora di chiedere l'elenco all'API: senza rete la
                        ' finestra si apre lo stesso, coi modelli che il programma
                        ' conosce da sé.
                        Assert.AreEqual("claude-sonnet-5", Scelto(finestra, "cmbModelloRagionamento"),
                                        "il ragionamento")
                        Assert.AreEqual("claude-haiku-4-5", Scelto(finestra, "cmbModelloSemplice"),
                                        "le elaborazioni testuali")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SceglierUnModelloLoScriveEValeSubito()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        ' Il nome della locale non può essere «tendina»: coprirebbe la
                        ' funzione omonima, e VB leggerebbe la chiamata come un indice.
                        Dim ragionamento As ComboBox = Tendina(finestra, "cmbModelloRagionamento")
                        ragionamento.SelectedIndex = Posto(ragionamento, "claude-haiku-4-5")

                        ' In vigore: è l'oggetto che il client interroga a ogni chiamata,
                        ' e la prossima deve partire col modello nuovo.
                        Assert.AreEqual("claude-haiku-4-5",
                                        contesto.Modelli.PerLivello(TrovaLavoro.Ai.Modelli.Ragionamento).Id,
                                        "vale già, senza riavviare niente")

                    End Using

                    ' Su disco: se restasse solo in memoria, al riavvio tornerebbe
                    ' indietro da solo senza che nessuno capisca perché.
                    Assert.IsTrue(File.Exists(contesto.Cartella.FileModelli), "il file è nato")
                    Assert.AreEqual("claude-haiku-4-5",
                                    TrovaLavoro.Ai.Modelli.Carica(contesto.Cartella.FileModelli).ModelloRagionamento.Id,
                                    "su disco")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub LaSceltaDelRagionamentoNonTrascinaLAltroLivello()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        ' Il nome della locale non può essere «tendina»: coprirebbe la
                        ' funzione omonima, e VB leggerebbe la chiamata come un indice.
                        Dim ragionamento As ComboBox = Tendina(finestra, "cmbModelloRagionamento")
                        ragionamento.SelectedIndex = Posto(ragionamento, "claude-haiku-4-5")

                        Assert.AreEqual("claude-haiku-4-5", Scelto(finestra, "cmbModelloSemplice"),
                                        "l'altra tendina non si è mossa")
                        Assert.AreEqual("claude-haiku-4-5",
                                        contesto.Modelli.PerLivello(TrovaLavoro.Ai.Modelli.Semplice).Id,
                                        "e il livello semplice è quello di prima")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Async Function SenzaElencoRestanoIModelliConosciuti() As Task

            Await ConMotoreAsync(
                Async Function(contesto)

                    Using finestra As New FinestraImpostazioni(
                        contesto, Function() Task.FromResult(TrovaLavoro.Ai.EsitoElenco.Fallito(TrovaLavoro.Ai.CausaErroreAi.Rete)))

                        Await finestra.AggiornaLElencoDeiModelli()

                        Assert.AreEqual(2, Tendina(finestra, "cmbModelloRagionamento").Items.Count,
                                        "i due modelli conosciuti, e si sceglie lo stesso")
                        StringAssert.Contains(Etichetta(finestra, "lblModelli").Text, "connessione",
                                              "e la riga dice perché l'elenco è corto")

                    End Using

                End Function)

        End Function

        <TestMethod>
        Public Async Function LElencoArrivatoAllungaLeTendineSenzaCambiareLaScelta() As Task

            Await ConMotoreAsync(
                Async Function(contesto)

                    Using finestra As New FinestraImpostazioni(
                        contesto, ElencoFinto("claude-sonnet-5", "claude-haiku-4-5", "claude-opus-4-8"))

                        Await finestra.AggiornaLElencoDeiModelli()

                        Assert.AreEqual(3, Tendina(finestra, "cmbModelloRagionamento").Items.Count,
                                        "adesso ci sono i modelli veri")
                        Assert.AreEqual("claude-sonnet-5", Scelto(finestra, "cmbModelloRagionamento"),
                                        "e la scelta di prima è rimasta quella")
                        Assert.IsFalse(File.Exists(contesto.Cartella.FileModelli),
                                       "rifare le tendine non è scegliere: niente è stato scritto")

                    End Using

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnModelloRitiratoRestaNellaTendina() As Task

            Await ConMotoreAsync(
                Async Function(contesto)

                    ' L'API non lo offre più, ma è quello scritto in modelli.json e quello
                    ' che parte a ogni chiamata: se la tendina lo omettesse mostrerebbe
                    ' come scelto un modello diverso da quello vero.
                    contesto.Modelli.CambiaModello(TrovaLavoro.Ai.Modelli.Ragionamento, "claude-sonnet-4-6",
                                                   contesto.Cartella.FileModelli)

                    Using finestra As New FinestraImpostazioni(
                        contesto, ElencoFinto("claude-sonnet-5", "claude-haiku-4-5"))

                        Await finestra.AggiornaLElencoDeiModelli()

                        Assert.AreEqual("claude-sonnet-4-6", Scelto(finestra, "cmbModelloRagionamento"),
                                        "quello in uso si vede, anche se ritirato")

                    End Using

                End Function)

        End Function

        Private Shared Function Scelto(finestra As Control, nome As String) As String
            Dim voce As TrovaLavoro.Ai.ModelloDisponibile = TryCast(Tendina(finestra, nome).SelectedItem, TrovaLavoro.Ai.ModelloDisponibile)
            Return If(voce Is Nothing, Nothing, voce.Id)
        End Function

        Private Shared Function Posto(tendina As ComboBox, id As String) As Integer
            For quale As Integer = 0 To tendina.Items.Count - 1
                If DirectCast(tendina.Items(quale), TrovaLavoro.Ai.ModelloDisponibile).Id = id Then Return quale
            Next
            Throw New AssertFailedException($"«{id}» non è fra le voci della tendina.")
        End Function

        ''' <summary>Come <see cref="ConMotore"/>, per una prova che deve aspettare l'AI.</summary>
        Private Shared Async Function ConMotoreAsync(prova As Func(Of ContestoApp, Task)) As Task

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "finestra-impostazioni-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, ChiaveFinta)
                    Await prova(contesto)
                End Using
            Finally
                CartelleDiProva.PortaVia(radice)
            End Try

        End Function

    End Class

End Namespace
