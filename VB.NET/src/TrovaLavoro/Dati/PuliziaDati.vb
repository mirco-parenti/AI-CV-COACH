Imports System.IO
Imports System.Linq

Namespace Dati

    ''' <summary>
    ''' Le due pulizie delle Impostazioni (cap. 11.5): svuotare i dati di navigazione, e
    ''' mandare via tutto. Stanno qui e non nella finestra perché una cancellazione va
    ''' <b>collaudata</b>, e un banco non sa premere un bottone.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché «tutto» non vuol dire la cartella intera.</b> Il lucchetto
    ''' <c>dati.lock</c> è tenuto aperto in esclusiva dall'applicazione per tutta la
    ''' sessione (cap. 09.4): finché il programma vive, quel file non si lascia cancellare,
    ''' e provarci solleverebbe un errore proprio nel momento in cui l'utente si aspetta la
    ''' pulizia più radicale. Si manda via tutto il resto e lo si dichiara: un lucchetto
    ''' vuoto non è un dato dell'utente, e alla riapertura una cartella con dentro solo
    ''' quello è indistinguibile da un primo avvio.</para>
    ''' <para><b>Nessun cestino, nessun annulla.</b> È la stessa ragione per cui il gesto
    ''' costa una parola scritta a mano (cap. 11.5): quel che si cancella qui non torna, e
    ''' il modo di rendersene conto prima è la conferma, non un ripensamento dopo.</para>
    ''' </remarks>
    Public NotInheritable Class PuliziaDati

        ''' <summary>Il nome del lucchetto, l'unica cosa che sopravvive a «elimina tutto».</summary>
        Private Const NomeLucchetto As String = "dati.lock"

        Private ReadOnly _cartella As CartellaDati

        Public Sub New(cartella As CartellaDati)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            _cartella = cartella

        End Sub

        ''' <summary>
        ''' Manda via la cartella <c>webview2\</c>, dove il browser incorporato tiene
        ''' cronologia, cache e cookie della ricerca annunci (cap. 11.1).
        ''' </summary>
        ''' <returns><c>False</c> se non c'era niente da svuotare.</returns>
        Public Function SvuotaNavigazione() As Boolean

            If Not Directory.Exists(_cartella.CartellaWebView2) Then Return False

            Directory.Delete(_cartella.CartellaWebView2, recursive:=True)

            Return True

        End Function

        ''' <summary>
        ''' Manda via tutto il contenuto della cartella dati, lucchetto escluso: profilo e
        ''' storico, candidature e registro, chiave API, preferenze, taratura e modelli,
        ''' backup, diario dei consumi, dati di navigazione.
        ''' </summary>
        ''' <returns>Quante voci di primo livello sono state eliminate.</returns>
        Public Function EliminaTutto() As Integer

            If Not Directory.Exists(_cartella.Radice) Then Return 0

            Dim andati As Integer = 0

            For Each sottocartella As String In Directory.GetDirectories(_cartella.Radice)
                Directory.Delete(sottocartella, recursive:=True)
                andati += 1
            Next

            For Each percorso As String In Directory.GetFiles(_cartella.Radice)

                ' Il lucchetto resta: è nostro, non suo, e finché viviamo non si lascia
                ' cancellare comunque.
                If String.Equals(Path.GetFileName(percorso), NomeLucchetto,
                                 StringComparison.OrdinalIgnoreCase) Then Continue For

                File.Delete(percorso)
                andati += 1

            Next

            Return andati

        End Function

        ''' <summary>
        ''' Se nella cartella dati è rimasto qualcosa dell'utente. Serve ad accendere o
        ''' spegnere il bottone: uno rosso che non ha niente da fare insegna solo a non
        ''' fidarsi del colore (cap. 03.6, la stessa regola di «ELIMINA PROFILO»).
        ''' </summary>
        ''' <remarks>
        ''' Guarda i <b>file</b>, e in tutto l'albero, non le voci di primo livello: una
        ''' cartella vuota non è un dato. <see cref="CartellaDati.Assicura"/> ricrea
        ''' <c>profilo\</c>, <c>storico\</c>, <c>out\</c> e <c>opportunita\</c> appena
        ''' qualcuno tocca la cartella dati — anche subito dopo un'eliminazione totale — e
        ''' contando le voci il bottone si sarebbe riacceso su quattro cartelle vuote,
        ''' promettendo di eliminare un nulla.
        ''' </remarks>
        Public ReadOnly Property CEQualcosa As Boolean
            Get

                If Not Directory.Exists(_cartella.Radice) Then Return False

                Return Directory.EnumerateFiles(_cartella.Radice, "*", SearchOption.AllDirectories).
                       Any(Function(file) Not String.Equals(Path.GetFileName(file), NomeLucchetto,
                                                            StringComparison.OrdinalIgnoreCase))

            End Get
        End Property

    End Class

End Namespace
