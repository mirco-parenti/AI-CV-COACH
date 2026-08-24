''' <summary>
''' Versione dell'applicazione: unica fonte, mai duplicata altrove (cap. 13.5).
''' Formato maggiore.minore.build; ogni modifica al codice incrementa la build.
''' Anche il file di progetto legge da qui il numero che finisce nelle proprietà
''' dell'eseguibile, quindi la costante va lasciata su una riga sola.
''' </summary>
Public Module Versione

    ''' <summary>Numero di versione mostrato nell'interfaccia e nell'eseguibile.</summary>
    Public Const Numero As String = "0.3.046"

    ''' <summary>
    ''' La riga che l'utente legge: versione dell'applicazione e versione della libreria
    ''' dei prompt, separate dal punto mediano (cap. 03.5). Sta qui e non nei due posti
    ''' che la mostrano — il pannello del logo e «Informazioni su…» — perché due copie
    ''' della stessa riga divergono al primo ritocco.
    ''' </summary>
    ''' <param name="etichettaPool">
    ''' L'etichetta della libreria, che dichiara da sé sorgente e stato: «Pool 1.12»,
    ''' «Pool 1.12 (integrato)», «Pool 1.12*». «Pool —» quando non si è aperta affatto.
    ''' </param>
    Public Function Riga(etichettaPool As String) As String
        Return $"Ver. {Numero} · {etichettaPool}"
    End Function

End Module
