Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi di «Informazioni su…» (cap. 03.4) e della riga di versione che mostra
    ''' (cap. 03.5). Quel che qui può rompersi davvero è la riga: dice all'utente con
    ''' quale libreria di prompt sta lavorando, ed è la prima cosa che si guarda quando
    ''' un esito non torna.
    ''' </summary>
    ''' <remarks>
    ''' La finestra si costruisce e si interroga <b>senza mostrarla</b>, come le altre
    ''' modali del progetto.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraInformazioni

        <TestMethod>
        Public Sub LaRigaDiVersioneHaUnaFonteSola()

            Assert.AreEqual($"Ver. {Versione.Numero} · Pool 1.12 (integrato)",
                            Versione.Riga("Pool 1.12 (integrato)"),
                            "la riga la compone Versione, non chi la mostra")

        End Sub

        <TestMethod>
        Public Sub LaFinestraMostraVersioneEPool()

            Using finestra As New FinestraInformazioni("Pool 1.12 (integrato)")

                Assert.AreEqual(Versione.Riga("Pool 1.12 (integrato)"), finestra.RigaDiVersione,
                                "la stessa riga del pannello del logo")
                StringAssert.Contains(finestra.RigaDiCopyright, "Aviolab AI",
                                      "e il marchio di chi l'ha fatto")

            End Using

        End Sub

        <TestMethod>
        Public Sub SenzaLibreriaLoDiceInveceDiTacere()

            ' «Pool —» è l'anomalia totale del cap. 03.5: la libreria non si è aperta
            ' affatto. Meglio dirlo che mostrare una riga monca.
            For Each niente As String In New String() {Nothing, "", "   "}

                Using finestra As New FinestraInformazioni(niente)
                    StringAssert.Contains(finestra.RigaDiVersione, "Pool —",
                                          "senza libreria la riga lo dichiara")
                End Using

            Next

        End Sub

        <TestMethod>
        Public Sub LaFinestraPortaIlMarchioInAlto()

            Using finestra As New FinestraInformazioni("Pool 1.12")

                Assert.IsNotNull(finestra.ImmagineDelMarchio, "l'immagine del marchio c'è, e si vede")
                Assert.AreSame(Marchio.SchermataDiAvvio, finestra.ImmagineDelMarchio,
                               "ed è la stessa dell'avvio, non una seconda copia")

            End Using

        End Sub

        ''' <summary>
        ''' Il reperto D-R1 del giro D: due eseguibili con lo stesso «1.0.000» dentro,
        ''' contenuto diverso, e niente che li distinguesse. Da qui in poi il file dice
        ''' da quale commit nasce — e questi collaudi guardano che lo dica davvero.
        ''' </summary>
        <TestMethod>
        Public Sub LaRigaDelSorgenteDiceIlCommit()

            Assert.AreEqual("Codice sorgente: 23f4df7", Versione.RigaDelSorgente("23f4df7"),
                            "il codice del commit, per esteso e senza abbellimenti")

        End Sub

        <TestMethod>
        Public Sub SenzaCommitLaRigaLoDichiaraInveceDiSparire()

            ' Una riga che sparisce si legge come «va tutto bene». È l'equivoco che ha
            ' prodotto D-R1: quel che non si sa va detto, non taciuto.
            For Each niente As String In New String() {Nothing, "", "   "}

                StringAssert.Contains(Versione.RigaDelSorgente(niente), "non dichiarato",
                                      "senza codice la riga lo dice")

            Next

        End Sub

        <TestMethod>
        Public Sub UnAlberoSporcoSeLoPortaDietro()

            ' publish.bat marca il codice quando ci sono modifiche non committate: quel
            ' marchio deve arrivare fino all'utente, perché è la differenza fra «questo
            ' file è il commit abc1234» e «questo file assomiglia al commit abc1234».
            StringAssert.Contains(Versione.RigaDelSorgente("23f4df7+modificato"), "+modificato",
                                  "il marchio dell'albero sporco non si perde per strada")

        End Sub

        <TestMethod>
        Public Sub LaFinestraMostraDaQualeSorgenteNasce()

            Using finestra As New FinestraInformazioni("Pool 1.13")

                Assert.AreEqual(Versione.RigaDelSorgente(), finestra.RigaDelCodiceSorgente,
                                "la finestra mostra la riga che compone Versione, non una sua copia")
                StringAssert.Contains(finestra.RigaDelCodiceSorgente, "Codice sorgente",
                                      "e si capisce che cos'è senza doverlo indovinare")

            End Using

        End Sub

        <TestMethod>
        Public Sub IlBottoneDellaDiagnosticaCEQuandoCEQualcosaDaCopiare()

            ' Un bottone che c'è e non fa niente è peggio di un bottone che non c'è.
            Using senzaNiente As New FinestraInformazioni("Pool 1.13")
                Assert.IsFalse(senzaNiente.PuoCopiareLaDiagnostica,
                               "senza contesto montato non c'è diagnostica da dare")
            End Using

            Using conQualcosa As New FinestraInformazioni("Pool 1.13", Function() "il foglietto")
                Assert.IsTrue(conQualcosa.PuoCopiareLaDiagnostica, "con il contesto, sì")
            End Using

        End Sub

        ' ==================================================================
        ' «Cerca aggiornamenti» (2026-08-27, dalla revisione del giro D)
        ' ==================================================================

        <TestMethod>
        Public Sub AprirlaNonChiedeNienteANessuno()

            ' È la promessa del cap. 11.2 — «niente aggiornamenti automatici silenziosi» —
            ' e qui è un fatto verificabile: la sola apertura della finestra non deve far
            ' partire nessuna chiamata verso l'esterno.
            Dim volte As Integer = 0

            Using finestra As New FinestraInformazioni(
                "Pool 1.13", Nothing,
                Function()
                    volte += 1
                    Return Task.FromResult(TrovaLavoro.Motore.ControlloVersione.Confronta("1.0.000", "1.0"))
                End Function)

                Assert.IsNotNull(finestra)

            End Using

            Assert.AreEqual(0, volte, "nessuno ha chiesto niente a nessuno")

        End Sub

        <TestMethod>
        Public Async Function QuandoLoChiediDiceComEMessa() As Task

            Using finestra As New FinestraInformazioni(
                "Pool 1.13", Nothing,
                Function() Task.FromResult(TrovaLavoro.Motore.ControlloVersione.Confronta("1.0.000", "1.4")))

                Await finestra.ControllaLaVersione()

                StringAssert.Contains(finestra.EsitoDellaVersione, "1.4", "dice qual è l'ultima")
                Assert.IsTrue(finestra.CENeUnaNuova, "e sa che ce n'è una nuova")

            End Using

        End Function

        <TestMethod>
        Public Async Function UnControlloCheVaAMaleNonFaCadereLaFinestra() As Task

            Using finestra As New FinestraInformazioni(
                "Pool 1.13", Nothing,
                Function() Task.FromException(Of TrovaLavoro.Motore.EsitoVersione)(New InvalidOperationException("boom")))

                Await finestra.ControllaLaVersione()

                Assert.IsFalse(String.IsNullOrWhiteSpace(finestra.EsitoDellaVersione), "una riga la dice")
                Assert.IsFalse(finestra.CENeUnaNuova, "e non promette aggiornamenti che non sa")

            End Using

        End Function

        ' ==================================================================
        ' Come sta insieme (2026-08-27, guardando la finestra a occhio)
        ' ==================================================================

        ''' <summary>
        ''' I controlli di una finestra, coi rettangoli che occupano davvero. Il banco non
        ''' mostra le modali, e <c>Visible</c> su una finestra mai mostrata è falso per
        ''' tutti: quel che si può leggere sono i <c>Bounds</c>, che ci sono lo stesso.
        ''' </summary>
        Private Shared Function Rettangoli(finestra As FinestraInformazioni) As List(Of KeyValuePair(Of String, Rectangle))

            Dim presi As New List(Of KeyValuePair(Of String, Rectangle))

            For Each figlio As Control In finestra.Controls
                ' Senza immagine la finestra si accorcia e tutto sale: il riquadro del
                ' marchio resta dov'era, e conterebbe come una sovrapposizione che a
                ' video non c'è.
                If TypeOf figlio Is PictureBox AndAlso finestra.ImmagineDelMarchio Is Nothing Then Continue For
                presi.Add(New KeyValuePair(Of String, Rectangle)(figlio.Name, figlio.Bounds))
            Next

            Return presi

        End Function

        <TestMethod>
        Public Sub NessunControlloNeCopreUnAltro()

            ' Il difetto che ha fatto nascere questo collaudo: le tre righe di testo e la
            ' fila dei bottoni stavano sulla stessa banda, e la riga del copyright
            ' copriva «Cerca aggiornamenti» — che si leggeva a metà e, nei tre quarti
            ' coperti, non si poteva nemmeno premere, perché il clic lo prendeva la
            ' scritta davanti. Al banco non si vedeva niente: i controlli c'erano tutti,
            ' col loro nome, accesi.
            Using finestra As New FinestraInformazioni("Pool 1.13 (integrato)", Function() "diagnostica")

                Dim presi As List(Of KeyValuePair(Of String, Rectangle)) = Rettangoli(finestra)

                For primo As Integer = 0 To presi.Count - 2
                    For secondo As Integer = primo + 1 To presi.Count - 1

                        ' IntersectsWith e non Intersect().IsEmpty: due controlli che si
                        ' toccano sul bordo danno un rettangolo comune alto zero, che
                        ' però «vuoto» per Rectangle non è — lo è solo quello tutto a
                        ' zero — e il collaudo accuserebbe una sovrapposizione che non c'è.
                        Assert.IsFalse(presi(primo).Value.IntersectsWith(presi(secondo).Value),
                                       $"«{presi(primo).Key}» e «{presi(secondo).Key}» si sovrappongono " &
                                       $"({presi(primo).Value} e {presi(secondo).Value}): a video uno copre l'altro")

                    Next
                Next

            End Using

        End Sub

        <TestMethod>
        Public Sub TuttoStaDentroLaFinestra()

            ' Un comando fuori dal bordo non è un difetto minore: è un comando che non
            ' c'è, e nessuno può accorgersene leggendo l'elenco dei controlli.
            Using finestra As New FinestraInformazioni("Pool 1.13 (integrato)", Function() "diagnostica")

                Dim dentro As Rectangle = finestra.ClientRectangle

                For Each preso As KeyValuePair(Of String, Rectangle) In Rettangoli(finestra)
                    Assert.IsTrue(dentro.Contains(preso.Value),
                                  $"«{preso.Key}» {preso.Value} esce dalla finestra {dentro}")
                Next

            End Using

        End Sub

    End Class


End Namespace
