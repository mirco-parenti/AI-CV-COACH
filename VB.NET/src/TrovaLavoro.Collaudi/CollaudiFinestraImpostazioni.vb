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
        Public Sub DiceCosaGiraSottoIlCofanoSenzaLasciarloToccare()

            ConMotore(
                Sub(contesto)

                    Using finestra As New FinestraImpostazioni(contesto)

                        Dim motore As String = Etichetta(finestra, "lblModelli").Text

                        Assert.Contains(contesto.Modelli.ModelloSemplice.Id, motore,
                                        "il modello dell'estrazione si legge")
                        Assert.Contains(contesto.Modelli.ModelloRagionamento.Id, motore,
                                        "e quello del ragionamento")
                        Assert.Contains("predefiniti", motore, "e da dove viene")

                        ' Nessun controllo per cambiarli: si toccano da modelli.json, che
                        ' è la regola del cap. 11.6 — una riga, non una nuova build.
                        Assert.IsEmpty(finestra.Controls.Find("cmbModelloSemplice", searchAllChildren:=True))
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

    End Class

End Namespace
