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
            ServiIlClient(letti)
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

        Application.Run(New FormPrincipale(letti))
    End Sub

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

        MessageBox.Show(
            "È successo qualcosa che il programma non aveva previsto:" & vbLf &
            e.Exception.Message & vbLf & vbLf &
            "Il tuo profilo salvato su disco non è stato toccato. " &
            "Se il problema si ripete, chiudi e riapri il programma.",
            "TrovaLavoro", MessageBoxButtons.OK, MessageBoxIcon.Error)

    End Sub

End Module
