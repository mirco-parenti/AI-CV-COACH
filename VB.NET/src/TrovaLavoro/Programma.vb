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
        Application.Run(New FormPrincipale())
    End Sub

End Module
