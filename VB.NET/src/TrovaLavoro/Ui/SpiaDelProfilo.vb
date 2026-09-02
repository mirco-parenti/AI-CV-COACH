Imports System.Drawing

''' <summary>
''' In che stato è la spia del profilo: la lucina che dice se quel che si sta guardando —
''' una candidatura, un punteggio, un documento — è nato dal profilo di <b>oggi</b>.
''' </summary>
Public Enum StatoSpia

    ''' <summary>
    ''' Non c'è niente da giudicare, e la spia non si accende affatto.
    ''' </summary>
    ''' <remarks>
    ''' È il terzo stato, e non è un dettaglio: una candidatura mai confrontata non ha
    ''' nessuna versione annotata, e <see cref="Dati.ArchivioProfilo.CambiatoDopo"/> di una
    ''' versione vuota risponde <c>False</c> — cioè, tradotto in lucine, <b>verde</b>. Ma
    ''' un verde lì direbbe «allineata» di una cosa che non esiste, e sarebbe una bugia
    ''' detta con la faccia rassicurante. Di un dubbio non si fa un allarme, ma nemmeno una
    ''' promessa: la spia resta spenta.
    ''' </remarks>
    Spenta = 0

    ''' <summary>È nato dal profilo di oggi.</summary>
    Allineato = 1

    ''' <summary>È nato da un profilo che non è più quello di oggi.</summary>
    Disallineato = 2

End Enum

''' <summary>
''' Come si mostra la spia in un punto preciso: la parola, il colore e il perché.
''' </summary>
Public NotInheritable Class LetturaSpia

    Friend Sub New(stato As StatoSpia, parola As String, colore As Color, perche As String)
        _stato = stato
        _parola = parola
        _colore = colore
        _perche = perche
    End Sub

    Private ReadOnly _stato As StatoSpia
    Private ReadOnly _parola As String
    Private ReadOnly _colore As Color
    Private ReadOnly _perche As String

    ''' <summary>Lo stato, per chi deve decidere qualcosa invece che scriverlo.</summary>
    Public ReadOnly Property Stato As StatoSpia
        Get
            Return _stato
        End Get
    End Property

    ''' <summary>Se c'è qualcosa da mostrare: a spia spenta non si scrive niente.</summary>
    Public ReadOnly Property Accesa As Boolean
        Get
            Return _stato <> StatoSpia.Spenta
        End Get
    End Property

    ''' <summary>La parola sola, senza il pallino.</summary>
    Public ReadOnly Property Parola As String
        Get
            Return _parola
        End Get
    End Property

    ''' <summary>Pallino e parola insieme, come si leggono a video.</summary>
    Public ReadOnly Property Scritta As String
        Get
            If Not Accesa Then Return String.Empty
            Return SpiaDelProfilo.Pallino & " " & _parola
        End Get
    End Property

    ''' <summary>L'inchiostro del pallino e della parola.</summary>
    Public ReadOnly Property Colore As Color
        Get
            Return _colore
        End Get
    End Property

    ''' <summary>Il perché per esteso, da mettere nel suggerimento.</summary>
    Public ReadOnly Property Perche As String
        Get
            Return _perche
        End Get
    End Property

End Class

''' <summary>
''' La spia del profilo (cap. 03.8): dato ciò con cui una cosa è nata, dice se quella cosa
''' è ancora in pari col profilo di oggi — e come dirlo.
''' </summary>
''' <remarks>
''' <para><b>Perché esiste un posto solo.</b> Dal 2026-09-02 la domanda «il profilo è
''' cambiato?» vive in <see cref="Dati.ArchivioProfilo.CambiatoDopo"/>, e la sua gemella
''' «il profilo di allora c'è ancora?» in <see cref="Dati.ArchivioProfilo.CELaVersione"/>.
''' Questo modulo non le rifà: le <b>chiama</b>, e aggiunge l'unica cosa che ancora mancava
''' — come si <i>mostra</i> quella risposta. Quattro punti la chiedono (l'elenco della
''' Home, la candidatura aperta, i documenti, il 📄 CV base) e se ognuno se la disegnasse
''' da sé, fra un mese direbbero quattro cose leggermente diverse della stessa situazione:
''' è così che nascono le interfacce in cui non ci si fida di nessun colore.</para>
''' <para><b>Perché due stati e non tre.</b> Sotto, i casi di disallineamento restano due e
''' pesano diversamente: il profilo può essere <b>cresciuto</b> — stessa persona, dati di
''' ieri — oppure essere stato <b>rifatto da capo</b>, e allora quei documenti raccontano
''' un'altra persona. Il colore però è uno solo, deciso il 2026-09-02: chi guarda deve
''' capire in un decimo di secondo se può fidarsi, e un semaforo con due gialli diversi non
''' è più veloce di un cartello. La differenza non si perde, si sposta: sta nel
''' <see cref="LetturaSpia.Perche"/>, che è dove uno la va a cercare quando gli serve.</para>
''' <para><b>Il colore non basta mai</b> (cap. 03.8). La spia porta sempre la sua parola —
''' «profilo usato: corrente», «profilo usato: obsoleto» — e non conta sul rosso e sul
''' verde per dire la cosa: quei due colori sono esattamente la coppia che una persona su
''' dodici non separa, e il pallino da solo lascerebbe fuori proprio lei.</para>
''' <para><b>I due inchiostri sono misurati, non scelti a occhio.</b> Il verde è
''' <see cref="StileApp.Successo"/> (4,94:1 sull'avorio delle pagine). Il rosso <i>non</i>
''' è <see cref="StileApp.Pericolo"/>, che sarebbe il rosso dei badge: su quello stesso
''' avorio vale 4,35:1 e non arriva alla soglia di 4,5. È
''' <see cref="StileApp.RossoCritico"/>, che vale 7,07:1 — e la spia più importante è
''' proprio quella che deve leggersi.</para>
''' </remarks>
Public Module SpiaDelProfilo

    ''' <summary>Il tondino della lucina.</summary>
    Public Const Pallino As String = "●"

    ''' <summary>Quel che dice la spia verde.</summary>
    ''' <remarks>
    ''' Le due parole sono state riscritte il 2026-09-02, guardandole a video: prima
    ''' dicevano «profilo allineato» e «profilo disallineato». Dicono la stessa cosa, ma la
    ''' dicono di sé — «allineato» chiede a chi legge di indovinare *a che cosa*, e su una
    ''' riga di tabella, di fianco a un punteggio, quella domanda non si fa. Adesso il
    ''' soggetto è nominato («profilo usato») e lo stato è un aggettivo che non ha bisogno
    ''' del suo complemento: <b>corrente</b> o <b>obsoleto</b>.
    ''' </remarks>
    Public Const ParolaAllineato As String = "profilo usato: corrente"

    ''' <summary>Quel che dice la spia rossa.</summary>
    Public Const ParolaDisallineato As String = "profilo usato: obsoleto"

    ''' <summary>La lettura a spia spenta: niente da scrivere, niente da colorare.</summary>
    Public ReadOnly Spenta As New LetturaSpia(StatoSpia.Spenta, String.Empty,
                                              StileApp.TestoSecondario, Nothing)

    ''' <summary>
    ''' Come sta messa una cosa nata dalla versione di profilo indicata.
    ''' </summary>
    ''' <param name="archivio">L'archivio del profilo, che sa quali versioni ci sono state.</param>
    ''' <param name="versione">
    ''' La versione con cui quella cosa fu prodotta. Vuota vuol dire «non annotata», e allora
    ''' la spia resta <b>spenta</b>: non si sa da dove venga, e su un dubbio non si accende
    ''' né un allarme né una rassicurazione.
    ''' </param>
    ''' <param name="ceQualcosa">
    ''' Se una cosa da giudicare c'è davvero: una candidatura confrontata, un CV scritto. A
    ''' <c>False</c> la spia è spenta senza nemmeno guardare la versione — su ciò che non è
    ''' ancora stato fatto non c'è niente da dire.
    ''' </param>
    ''' <param name="giaCambiato">
    ''' Se il profilo è <b>già</b> cambiato sotto gli occhi di chi guarda, senza che nessuna
    ''' versione lo racconti ancora: sono le correzioni scritte a video e non salvate.
    ''' Nessun archivio può saperlo — su disco non è successo niente — e chi ha quel dato lo
    ''' porta qui invece di tenersi una sua regola parallela. Vale il rosso, e vale subito:
    ''' l'utente sta guardando un CV che non corrisponde più a quel che ha appena scritto.
    ''' </param>
    Public Function Leggi(archivio As Dati.ArchivioProfilo,
                          versione As String,
                          ceQualcosa As Boolean,
                          Optional giaCambiato As Boolean = False) As LetturaSpia

        If archivio Is Nothing OrElse Not ceQualcosa Then Return Spenta

        If giaCambiato Then
            Return Rossa("Hai corretto il profilo e non l'hai ancora salvato: quel che vedi " &
                         "è nato da com'era prima.")
        End If

        If String.IsNullOrEmpty(versione) Then Return Spenta

        ' Prima lo sparito, poi il cresciuto: quando una versione non c'è più anche
        ' CambiatoDopo risponde di sì, e il caso grave verrebbe raccontato con le parole di
        ' quello lieve.
        If Not archivio.CELaVersione(versione) Then
            Return Rossa("Il profilo con cui è stato prodotto non c'è più: quello di allora " &
                         "è stato eliminato, e quel che vedi racconta ancora lui.")
        End If

        If archivio.CambiatoDopo(versione) Then
            Return Rossa("Hai cambiato il profilo dopo: quel che vedi è di allora, e se nel " &
                         "frattempo è cambiato un requisito eliminatorio — la patente, un " &
                         "titolo, una disponibilità — oggi la risposta sarebbe un'altra.")
        End If

        Return New LetturaSpia(StatoSpia.Allineato, ParolaAllineato, StileApp.Successo,
                               "È nato dal profilo di oggi: quel che vedi è in pari.")

    End Function

    ''' <summary>La spia rossa, col suo perché.</summary>
    Private Function Rossa(perche As String) As LetturaSpia

        Return New LetturaSpia(StatoSpia.Disallineato, ParolaDisallineato,
                               StileApp.RossoCritico, perche)

    End Function

End Module
