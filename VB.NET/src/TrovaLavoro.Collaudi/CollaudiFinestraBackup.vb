Imports System.IO
Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi della finestra di backup e ripristino (F7, cap. 11.4). Quel che qui può
    ''' rompersi davvero non è la scrittura di un file — quella la sorveglia
    ''' <see cref="Dati.ArchivioBackup"/> — ma la <b>guardia</b>: che «Ripristina» non si
    ''' possa premere prima di aver letto cosa sovrascrive, e che un file scelto per
    ''' sbaglio si fermi con una spiegazione invece di diventare un ripristino a metà.
    ''' </summary>
    ''' <remarks>
    ''' Come le altre finestre modali, si costruisce e si interroga <b>senza mostrarla</b>:
    ''' per questo l'export, l'apertura e il ripristino hanno un metodo pubblico ciascuno,
    ''' che è anche quello che i bottoni chiamano dopo i loro dialoghi.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraBackup

        Private Const ChiaveFinta As String = "chiave-di-collaudo"

        <TestMethod>
        Public Sub AllAperturaDiceCosaFinirebbeNelFile()

            ' Le due scelte devono essere informate: «solo profilo» e «tutto» sono due
            ' parole finché non si vede che cosa cambia fra loro.
            ConMotore(
                Sub(contesto)

                    ConUnProfiloEUnaCandidatura(contesto)

                    Using finestra As New FinestraBackup(contesto)

                        Assert.Contains("profilo salvato il", Etichetta(finestra, "lblCosaCE").Text,
                                        "dice di che giorno è il profilo")
                        Assert.DoesNotContain("candidatura", Etichetta(finestra, "lblCosaCE").Text,
                                              "col solo profilo le candidature non c'entrano")

                        Bottone(finestra, "rdoTutto").Checked = True

                        Assert.Contains("1 candidatura", Etichetta(finestra, "lblCosaCE").Text,
                                        "scelto «tutto», la candidatura si conta")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub FinchéNonSiApreUnBackupNonSiRipristina()

            ' La guardia del cap. 11.4: prima si legge cosa contiene, poi si decide. Un
            ' bottone acceso a vuoto insegnerebbe a premerlo senza guardare.
            ConMotore(
                Sub(contesto)
                    Using finestra As New FinestraBackup(contesto)

                        Assert.IsFalse(Comando(finestra, "btnRipristina").Enabled,
                                       "senza un backup aperto non c'è niente da rimettere a posto")

                    End Using
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnFileCheNonEUnBackupSiSpiegaEIlBottoneRestaSpento()

            ' Il caso vero: nel dialogo si sceglie un altro JSON. Deve fermarsi qui, e
            ' dire perché — non «errore imprevisto».
            ConMotore(
                Sub(contesto)

                    contesto.Cartella.Assicura()
                    Dim intruso As String = Path.Combine(contesto.Cartella.Radice, "annuncio.json")
                    File.WriteAllText(intruso, "{""titolo"": ""Tecnico manutenzione""}")

                    Using finestra As New FinestraBackup(contesto)

                        Assert.IsFalse(finestra.Apri(intruso), "questo file non è un backup")
                        Assert.IsFalse(Comando(finestra, "btnRipristina").Enabled, "e il bottone resta spento")
                        Assert.Contains("formato_backup", Etichetta(finestra, "lblStato").Text,
                                        "si dice cosa manca perché sia un backup")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnFileRovinatoNonPassaPerUnBackupVuoto()

            ' Un file troncato a metà da una chiavetta staccata: non deve diventare «un
            ' backup che non contiene niente», che sarebbe il modo peggiore di dirlo.
            ConMotore(
                Sub(contesto)

                    contesto.Cartella.Assicura()
                    Dim rotto As String = Path.Combine(contesto.Cartella.Radice, "mezzo.json")
                    File.WriteAllText(rotto, "{""formato_backup"": 1, ""profilo"": { ""nome"": ")

                    Using finestra As New FinestraBackup(contesto)

                        Assert.IsFalse(finestra.Apri(rotto), "un JSON troncato non si legge")
                        Assert.Contains("rovinato", Etichetta(finestra, "lblStato").Text,
                                        "e si dice che è rovinato, non che è vuoto")

                    End Using

                End Sub)

        End Sub

        <TestMethod>
        Public Async Function ApertoIlBackupLAnteprimaDiceCosaContieneECosaSovrascrive() As Task

            ' È il passo 2 del cap. 11.4, quello che l'utente legge prima di confermare.
            Await ConMotoreAsync(
                Async Function(contesto)

                    ConUnProfiloEUnaCandidatura(contesto)
                    Dim dove As String = Path.Combine(contesto.Cartella.Radice, "prova.json")

                    Using finestra As New FinestraBackup(contesto)

                        Bottone(finestra, "rdoTutto").Checked = True
                        Assert.IsTrue(Await finestra.EsportaVersoAsync(dove), "il backup si scrive")
                        Assert.IsTrue(finestra.Apri(dove), "e si rilegge")

                        Dim anteprima As String = Casella(finestra, "txtAnteprima").Text

                        Assert.Contains("Il profilo", anteprima, "dice cosa c'è dentro")
                        Assert.Contains("1 candidatura", anteprima, "compresa la candidatura")
                        Assert.Contains("storico", anteprima,
                                        "e promette che il profilo di adesso finisce nello storico")
                        Assert.IsTrue(Comando(finestra, "btnRipristina").Enabled,
                                      "adesso il ripristino si può premere")

                    End Using

                End Function)

        End Function

        <TestMethod>
        Public Async Function IlGiroCompletoDallaFinestraRiportaIlProfilo() As Task

            ' Esporta, perdi tutto, ripristina — passando dalle stesse porte che usano i
            ' bottoni. E la finestra dichiara che il profilo su disco è cambiato: senza
            ' quella riga, il pannello continuerebbe a mostrare quello di prima.
            Await ConMotoreAsync(
                Async Function(contesto)

                    ConUnProfiloEUnaCandidatura(contesto)
                    Dim dove As String = Path.Combine(contesto.Cartella.Radice, "prova.json")

                    Using finestra As New FinestraBackup(contesto)

                        Bottone(finestra, "rdoTutto").Checked = True
                        Await finestra.EsportaVersoAsync(dove)

                        Directory.Delete(contesto.Cartella.CartellaProfilo, recursive:=True)
                        Assert.IsFalse(contesto.Archivio.Esiste, "il profilo è perso")

                        finestra.Apri(dove)
                        Dim esito As EsitoRipristino = Await finestra.RipristinaAsync()

                        Assert.IsTrue(esito.ProfiloRipristinato, "il ripristino dice di averlo fatto")
                        Assert.IsTrue(contesto.Archivio.Esiste, "e il profilo è tornato sul disco")
                        Assert.IsTrue(finestra.ProfiloRipristinato,
                                      "la finestra lo dichiara a chi l'ha aperta")
                        Assert.Contains("Fatto", Etichetta(finestra, "lblStato").Text, "e lo racconta")
                        Assert.IsFalse(Comando(finestra, "btnRipristina").Enabled,
                                       "lo stesso backup non si ripristina due volte per un click distratto")

                    End Using

                End Function)

        End Function

        <TestMethod>
        Public Async Function LeVociRifiutateSiDicono() As Task

            ' Se un backup contiene un nome che non è un nome di file, non si scrive — e
            ' non si tace: è l'unico modo che l'utente ha di accorgersene.
            Await ConMotoreAsync(
                Async Function(contesto)

                    Dim costruito As New TrovaLavoro.Dati.Backup With {.Data = Date.Now}
                    Dim cattiva As New OpportunitaInBackup With {.Cartella = "..\..\fuori"}
                    cattiva.File("annuncio.json") = JsonNode.Parse("{""titolo"": ""fuori casa""}")
                    costruito.Opportunita.Add(cattiva)

                    contesto.Cartella.Assicura()
                    Dim dove As String = Path.Combine(contesto.Cartella.Radice, "costruito.json")
                    contesto.Backup.Scrivi(costruito, dove)

                    Using finestra As New FinestraBackup(contesto)

                        finestra.Apri(dove)
                        Await finestra.RipristinaAsync()

                        Assert.Contains("non è un nome di file", Etichetta(finestra, "lblStato").Text,
                                        "quel che si è rifiutato si dice")
                        Assert.Contains("..\..\fuori", Etichetta(finestra, "lblStato").Text,
                                        "con il nome che non andava bene")

                    End Using

                End Function)

        End Function

        ' ==================================================================
        ' Il banco
        ' ==================================================================

        Private Shared Sub ConMotore(prova As Action(Of ContestoApp))

            Dim radice As String = Path.Combine(Path.GetTempPath(), "finestra-backup-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, ChiaveFinta)
                    prova(contesto)
                End Using
            Finally
                CartelleDiProva.PortaVia(radice)
            End Try

        End Sub

        ''' <summary>
        ''' Come <see cref="ConMotore"/>, per una prova che deve aspettare il disco:
        ''' l'export e il ripristino lavorano su un altro filo, e di là si torna con un
        ''' <c>Await</c>.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>Il contesto di sincronizzazione si mette da parte</b>, e senza questa
        ''' riga il banco non finirebbe più. Costruire un controllo installa sul thread il
        ''' contesto di Windows Forms, che rimanda ogni ritorno da un <c>Await</c> alla
        ''' <i>pompa di messaggi</i> della finestra; qui la pompa non c'è — le finestre si
        ''' costruiscono e non si mostrano — e quel ritorno resterebbe in coda per sempre,
        ''' con la prova ferma ad aspettarlo. Provato: il banco intero si è piantato al
        ''' primo export.</para>
        ''' <para>Fuori di qui il rimando è giusto ed è quel che serve: nell'applicazione
        ''' vera la pompa gira, e la riga di stato dev'essere scritta dal thread che
        ''' possiede la finestra. Perciò si mette da parte <b>qui</b>, per il tempo della
        ''' prova, e si rimette dov'era: il thread è condiviso con i collaudi che verranno
        ''' dopo.</para>
        ''' <para><b>L'auto-installazione è una manopola del processo</b>, non del thread:
        ''' spegnendola si spegne per tutti finché la prova non l'ha rimessa. Non fa danno
        ''' — nel banco nessuno pompa messaggi, quindi un collaudo che nel frattempo
        ''' costruisce una finestra sta meglio senza — ma è bene saperlo prima di
        ''' copiare queste righe altrove.</para>
        ''' </remarks>
        Private Shared Async Function ConMotoreAsync(prova As Func(Of ContestoApp, Task)) As Task

            Dim radice As String = Path.Combine(Path.GetTempPath(), "finestra-backup-" & Guid.NewGuid().ToString("N"))
            Dim quelloDiPrima As SynchronizationContext = SynchronizationContext.Current
            Dim siInstallavaDaSe As Boolean = WindowsFormsSynchronizationContext.AutoInstall

            ' Toglierlo e basta non basterebbe: il primo controllo costruito lo rimette
            ' proprio perché non c'è (è quel che vuol dire «AutoInstall»).
            WindowsFormsSynchronizationContext.AutoInstall = False
            SynchronizationContext.SetSynchronizationContext(Nothing)

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, ChiaveFinta)
                    Await prova(contesto)
                End Using
            Finally
                WindowsFormsSynchronizationContext.AutoInstall = siInstallavaDaSe
                SynchronizationContext.SetSynchronizationContext(quelloDiPrima)
                CartelleDiProva.PortaVia(radice)
            End Try

        End Function

        Private Shared Sub ConUnProfiloEUnaCandidatura(contesto As ContestoApp)

            contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))

            contesto.Opportunita.Salva(New Opportunita With {
                .Creata = New Date(2026, 8, 10, 9, 30, 0),
                .Annuncio = JsonNode.Parse("{""titolo"": ""Tecnico manutenzione"", ""azienda"": ""Rossi S.p.A.""}")})

        End Sub

        Private Shared Function Etichetta(finestra As Control, nome As String) As Label
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

        Private Shared Function Comando(finestra As Control, nome As String) As Button
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Bottone(finestra As Control, nome As String) As RadioButton
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), RadioButton)
        End Function

        Private Shared Function Casella(finestra As Control, nome As String) As TextBox
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), TextBox)
        End Function

    End Class

End Namespace
