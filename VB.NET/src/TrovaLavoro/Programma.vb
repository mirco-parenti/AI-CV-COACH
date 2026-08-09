Imports System.Threading
Imports System.Windows.Forms

''' <summary>
''' Punto d'ingresso dell'applicazione: prepara l'ambiente grafico e apre la
''' finestra principale. Il DPI è SystemAware come da cap. 03.4, e va impostato
''' prima che nasca qualunque finestra.
''' </summary>
Friend Module Programma

    <STAThread>
    Friend Sub Main()
        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        ' L'ultima rete: un'eccezione che nessun gestore ha fermato non deve
        ' presentarsi come la finestra tecnica di .NET, in inglese e piena di stack
        ' trace — questo programma parla italiano fino in fondo (cap. 02.5). Il
        ' profilo su disco è al sicuro per costruzione (scritture atomiche, cap. 11.1).
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, AddressOf MostraErroreImprevisto

        Application.Run(New FormPrincipale())
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
