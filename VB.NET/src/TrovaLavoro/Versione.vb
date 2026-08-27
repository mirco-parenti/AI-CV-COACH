''' <summary>
''' Versione dell'applicazione: unica fonte, mai duplicata altrove (cap. 13.5).
''' Formato maggiore.minore.build; ogni modifica al codice incrementa la build.
''' Anche il file di progetto legge da qui il numero che finisce nelle proprietà
''' dell'eseguibile, quindi la costante va lasciata su una riga sola.
''' </summary>
Public Module Versione

    ''' <summary>Numero di versione mostrato nell'interfaccia e nell'eseguibile.</summary>
    Public Const Numero As String = "1.0.000"

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

    ''' <summary>
    ''' Il codice del commit da cui questo eseguibile è stato costruito, o stringa vuota
    ''' se nessuno l'ha dichiarato.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché il numero di versione non basta.</b> Il 24 agosto 2026 due
    ''' eseguibili diversi hanno portato lo stesso «1.0.000»: il primo pubblicato alle
    ''' 17:52, il secondo con dentro tre cure delle 19:05 e 19:26. Niente li distingueva,
    ''' e il giro D ha collaudato quello sbagliato senza che nessuno potesse accorgersene
    ''' (reperto D-R1). Il numero di versione è un'etichetta che si scrive a mano; il
    ''' codice del commit è un fatto che il file si porta addosso.</para>
    ''' <para>Lo dichiara <c>publish.bat</c> al momento della pubblicazione, passandolo a
    ''' MSBuild come <c>-p:CodiceSorgente=…</c>; da lì diventa un attributo
    ''' <c>AssemblyMetadata</c> dell'eseguibile. Chi compila per provare non lo passa, e
    ''' allora qui non c'è: è giusto così, perché una compilazione di sviluppo non
    ''' corrisponde a nessun commit.</para>
    ''' </remarks>
    Public ReadOnly Property CodiceSorgente As String
        Get
            If _codiceSorgente Is Nothing Then _codiceSorgente = LeggiIlCodiceDichiarato()
            Return _codiceSorgente
        End Get
    End Property

    ''' <summary>
    ''' La riga che dice da quale sorgente nasce l'eseguibile che si ha in mano. Quando il
    ''' codice non c'è lo <b>dichiara</b> invece di sparire: una riga assente si legge come
    ''' «va tutto bene», ed è esattamente l'equivoco che ha prodotto D-R1.
    ''' </summary>
    Public Function RigaDelSorgente() As String
        Return RigaDelSorgente(CodiceSorgente)
    End Function

    ''' <summary>
    ''' La stessa riga, composta a partire da un codice qualunque. Esiste separata da
    ''' quella qui sopra per una ragione sola: <b>si puo' collaudare</b>. Il ramo che
    ''' conta e' quello del codice presente, e l'unica compilazione che il banco vede e'
    ''' quella di sviluppo, che un codice non ce l'ha mai — senza questa porta si
    ''' potrebbe provare solo meta' del comportamento, e sarebbe la meta' che non
    ''' serve.
    ''' </summary>
    Public Function RigaDelSorgente(codice As String) As String

        If String.IsNullOrWhiteSpace(codice) Then
            Return "Codice sorgente: non dichiarato (compilazione di sviluppo)"
        End If

        Return $"Codice sorgente: {codice.Trim()}"

    End Function

    ''' <summary>Il codice letto una volta sola; <c>Nothing</c> finché non si è letto.</summary>
    Private _codiceSorgente As String

    ''' <summary>
    ''' Cerca fra gli attributi dell'assembly quello che <c>publish.bat</c> vi ha scritto.
    ''' Non solleva mai: se la lettura non riesce l'eseguibile resta senza codice
    ''' dichiarato, che è la stessa cosa che dire «non lo so» — ed è vera.
    ''' </summary>
    Private Function LeggiIlCodiceDichiarato() As String

        Try
            Dim assemblea As Reflection.Assembly = Reflection.Assembly.GetExecutingAssembly()
            For Each dato As Reflection.AssemblyMetadataAttribute In
                assemblea.GetCustomAttributes(GetType(Reflection.AssemblyMetadataAttribute), False)

                If String.Equals(dato.Key, "CodiceSorgente", StringComparison.Ordinal) Then
                    Return If(dato.Value, "").Trim()
                End If

            Next
        Catch ex As Exception
            ' Un eseguibile che non sa dirsi non è rotto: tace, e lo dichiara.
        End Try

        Return ""

    End Function

End Module
