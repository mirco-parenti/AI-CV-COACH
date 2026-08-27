Imports System.Threading
Imports System.Windows.Forms
Imports TrovaLavoro.Mcp
Imports TrovaLavoro.Motore

''' <summary>
''' Punto d'ingresso dell'applicazione: prepara l'ambiente grafico e apre la
''' finestra principale. Il DPI è SystemAware come da cap. 03.4, e va impostato
''' prima che nasca qualunque finestra.
''' </summary>
''' <remarks>
''' Da T8 le strade sono due e si separano qui, alla prima riga: con <c>--mcp</c>
''' (cap. 09) non nasce nessuna finestra e il programma serve un client AI su stdio.
''' La biforcazione sta prima di ogni preparativo grafico apposta — impostare il DPI o
''' gli stili in un processo che non mostrerà mai niente sarebbe lavoro buttato, e
''' peggio: la rete per le eccezioni impreviste dirotterebbe gli errori verso una
''' finestra che nessuno vedrebbe.
''' </remarks>
Friend Module Programma

    ''' <param name="argomenti">
    ''' La riga di comando, letta da <see cref="ArgomentiAvvio"/> (cap. 11.1). Qui non si
    ''' decide niente su di essa: gli argomenti arrivano alla finestra, che è l'unica a
    ''' sapere come si mostra un avviso a chi guarda.
    ''' </param>
    <STAThread>
    Friend Sub Main(argomenti As String())

        Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi(argomenti)

        If letti.ModalitaMcp Then

            ' Anche il server ha bisogno della sua rete, e non può essere la stessa: qui
            ' non ci sono finestre. Un guasto si racconta su stderr — dove il client
            ' raccoglie il diario — e si esce con un codice che dice che è andata male,
            ' invece di morire con lo stack trace di .NET su un canale che nessuno legge.
            ' (2026-08-27, dalla revisione del giro D.)
            Try
                ServiIlClient(letti)
            Catch ex As Exception
                Console.Error.WriteLine("TrovaLavoro: " & UltimaRete.MessaggioPerIlDiario(ex))
                Dati.DiarioTecnico.Corrente?.AnnotaGuasto("il ciclo del server MCP", ex)
                Environment.ExitCode = 1
            End Try

            Return

        End If

        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' L'ultima rete: un'eccezione che nessun gestore ha fermato non deve
        ' presentarsi come la finestra tecnica di .NET, in inglese e piena di stack
        ' trace — questo programma parla italiano fino in fondo (cap. 02.5). Il
        ' profilo su disco è al sicuro per costruzione (scritture atomiche, cap. 11.1).
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf MostraErroreImprevisto

        ' ThreadException però è solo il ciclo dei messaggi. Un'eccezione che scoppia su
        ' un altro filo, o dentro un Task che nessuno ha atteso, gli passa accanto — e fa
        ' morire il processo con la finestra tecnica di .NET, cioè esattamente quel che
        ' le due righe qui sopra esistono per evitare. Le tre reti insieme coprono i tre
        ' modi in cui un'eccezione può arrivare fin qui.
        ' (2026-08-27, dalla revisione del giro D.)
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf MostraErroreDalDominio
        AddHandler TaskScheduler.UnobservedTaskException, AddressOf ScordaIlTaskNonAtteso

        ' La schermata di avvio copre il montaggio (cap. 03.4). Nasce di qua e non dentro
        ' la finestra principale perché deve essere a video *prima* che il montaggio
        ' cominci: quel che accade nel Load — cartella dati, lucchetto, libreria, browser
        ' — accade a finestra ancora invisibile, cioè davanti a uno schermo vuoto. È lei
        ' poi a dire quando toglierla, che è l'unica parte con una regola vera.
        _schermataDiAvvio = FinestraAvvio.Mostra()

        Application.Run(New FormPrincipale(letti, _schermataDiAvvio))
    End Sub

    ''' <summary>
    ''' La schermata di avvio in corso, se ce n'è una: serve alla rete delle eccezioni,
    ''' che deve poterla togliere prima di mostrare un errore.
    ''' </summary>
    Private _schermataDiAvvio As FinestraAvvio

    ''' <summary>
    ''' La modalità server MCP (cap. 09): niente finestre, si parla con il client che ha
    ''' avviato il processo e si esce quando quello chiude l'ingresso.
    ''' </summary>
    ''' <remarks>
    ''' Tutto ciò che l'applicazione con le finestre direbbe nella barra di stato — gli
    ''' avvisi sugli argomenti, il resoconto del montaggio, la chiave che manca — qui
    ''' finisce nel diario su <c>stderr</c>, che il client raccoglie in un file a parte.
    ''' Su <c>stdout</c> non passa niente che non sia protocollo, ed è la sola ragione
    ''' per cui vale la pena ripeterlo.
    ''' </remarks>
    Private Sub ServiIlClient(argomenti As ArgomentiAvvio)

        Using contesto As ContestoApp = ContestoApp.Monta(argomenti.RadiceDati)

            Dim server As ServerMcp = ServerMcp.SuStdio(contesto)

            For Each avviso As String In argomenti.Avvisi
                server.Annota(avviso)
            Next

            ' Una chiave da ridigitare vorrebbe una finestra, e qui non ce ne sono: si
            ' dice e si tira dritto, come per ogni altra cosa che non si può rispettare.
            If argomenti.ChiediLaChiave Then
                server.Annota($"L'argomento «{ArgomentiAvvio.OpzioneChiave}» non vale in modalità " &
                              "server: la chiave API si digita nella finestra dell'applicazione.")
            End If

            ' Il resoconto intero, non i soli avvisi: nel diario di un server ci sta
            ' anche la normalità — quale cartella dati, quale pool, quali modelli — che
            ' nella barra di stato sarebbe rumore e qui è la prima cosa che si va a
            ' guardare quando qualcosa non torna.
            For Each nota As String In contesto.Note
                server.Annota(nota)
            Next

            ' Il punto d'ingresso di un programma Windows non è asincrono, e qui è
            ' l'unico posto in cui l'attesa si fa a mano: si aspetta il ciclo del server,
            ' che finisce quando il client chiude l'ingresso. Nessun rischio di stallo —
            ' in un WinExe avviato così non c'è nessun contesto di sincronizzazione a cui
            ' le continuazioni debbano tornare, perché non c'è nessuna finestra.
            server.ServiAsync().GetAwaiter().GetResult()

        End Using

    End Sub

    Private Sub MostraErroreImprevisto(mittente As Object, e As ThreadExceptionEventArgs)
        MostraLErrore(e.Exception)
    End Sub

    ''' <summary>La rete del dominio: quel che ThreadException non vede (v. Main).</summary>
    Private Sub MostraErroreDalDominio(mittente As Object, e As UnhandledExceptionEventArgs)
        MostraLErrore(TryCast(e.ExceptionObject, Exception))
    End Sub

    ''' <summary>
    ''' Un <c>Task</c> andato a male che nessuno stava aspettando. Non c'è niente da
    ''' mostrare — quel risultato non interessava più a nessuno — ma va dichiarato
    ''' «osservato»: altrimenti .NET lo rilancia al passaggio del raccoglitore, e il
    ''' programma muore per un'eccezione che nessuno aveva chiesto.
    ''' </summary>
    Private Sub ScordaIlTaskNonAtteso(mittente As Object, e As UnobservedTaskExceptionEventArgs)
        e.SetObserved()
        Dati.DiarioTecnico.Corrente?.AnnotaGuasto("un compito che nessuno stava aspettando", e.Exception)
    End Sub

    ''' <summary>Mostra l'errore all'utente, dopo aver tolto di mezzo la schermata di avvio.</summary>
    Private Sub MostraLErrore(eccezione As Exception)

        ' Prima di tutto il diario: la finestra qui sotto aspetta un clic, e fra il
        ' guasto e quel clic può succedere di tutto — compreso che l'utente chiuda la
        ' finestra senza leggerla. Quel che è scritto su disco resta.
        Dati.DiarioTecnico.Corrente?.AnnotaGuasto("l'ultima rete", eccezione)

        ' Poi la schermata di avvio, se è ancora lì: sta sopra ogni altra finestra, e un
        ' messaggio d'errore che nessuno vede è peggio dell'errore.
        _schermataDiAvvio?.ChiudiSubito()

        MessageBox.Show(UltimaRete.MessaggioImprevisto(eccezione),
                        "TrovaLavoro", MessageBoxButtons.OK, MessageBoxIcon.Error)

    End Sub

End Module
