''' <summary>
''' Versione dell'applicazione: unica fonte, mai duplicata altrove (cap. 13.5).
''' Formato maggiore.minore.build; ogni modifica al codice incrementa la build.
''' Anche il file di progetto legge da qui il numero che finisce nelle proprietà
''' dell'eseguibile, quindi la costante va lasciata su una riga sola.
''' </summary>
Public Module Versione

    ''' <summary>Numero di versione mostrato nell'interfaccia e nell'eseguibile.</summary>
    Public Const Numero As String = "0.3.017"

End Module
