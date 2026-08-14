Imports System.IO

Namespace Dati

    ''' <summary>
    ''' La mappa della cartella dati (cap. 11.1): l'unico posto che sa dove vivono i
    ''' file dell'utente. Prima di questa classe ogni componente si ricostruiva il
    ''' percorso per conto suo — <c>taratura.json</c> di là, <c>modelli.json</c> di
    ''' qua — e la stessa riga compariva due volte; con T3, che è la prima tappa che
    ''' <b>scrive</b> (profilo e storico), le copie sarebbero diventate tre.
    ''' </summary>
    ''' <remarks>
    ''' È una classe con istanze e non un modulo con una radice globale perché la
    ''' cartella è <i>modificabile al primo avvio o nelle Impostazioni</i> (cap. 11.1):
    ''' la radice arriva da fuori, non è una costante. Il collaudo si serve della
    ''' stessa porta e lavora in una cartella temporanea, così la <c>%APPDATA%</c> vera
    ''' non viene mai toccata.
    '''
    ''' Qui stanno solo i percorsi che il programma usa oggi. Gli altri del cap. 11.1
    ''' — opportunità, registro, log, backup, segreti, configurazione — si aggiungono
    ''' con la tappa che li scrive: un percorso nominato prima di avere un file vero è
    ''' solo un nome da tenere allineato.
    ''' </remarks>
    Public Class CartellaDati

        ''' <summary>Il nome della cartella dell'applicazione dentro <c>%APPDATA%</c>.</summary>
        Private Const NomeCartella As String = "TrovaLavoro"

        ''' <summary>La cartella dati su cui è costruita questa mappa.</summary>
        Public ReadOnly Property Radice As String

        ''' <summary>
        ''' Costruisce la mappa su una radice qualunque: quella dell'utente, o una
        ''' cartella temporanea nei collaudi.
        ''' </summary>
        ''' <param name="radice">La cartella dati; non può essere vuota.</param>
        Public Sub New(radice As String)

            If String.IsNullOrWhiteSpace(radice) Then
                Throw New ArgumentException("La cartella dati non può essere vuota.", NameOf(radice))
            End If

            ' Percorso assoluto e normalizzato: i confronti fra percorsi (e i messaggi
            ' d'errore) non devono dipendere da come è stata scritta la radice.
            _Radice = Path.GetFullPath(radice)

        End Sub

        ''' <summary>La cartella dati predefinita: <c>%APPDATA%\TrovaLavoro</c> (cap. 11.1).</summary>
        Public Shared ReadOnly Property RadicePredefinita As String
            Get
                Return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    NomeCartella)
            End Get
        End Property

        ''' <summary>La mappa sulla cartella dati predefinita.</summary>
        Public Shared Function Predefinita() As CartellaDati
            Return New CartellaDati(RadicePredefinita)
        End Function

        ''' <summary>
        ''' Se questa mappa lavora nella cartella di sempre (cap. 11.1). Serve a chi deve
        ''' <b>dirlo</b>: un'applicazione avviata su un'altra radice — con <c>--dati</c>,
        ''' o un domani dalle Impostazioni — non deve poter essere scambiata per quella
        ''' che tiene i dati veri.
        ''' </summary>
        Public ReadOnly Property SullaRadicePredefinita As Boolean
            Get
                Return String.Equals(SenzaBarraFinale(Radice),
                                     SenzaBarraFinale(RadicePredefinita),
                                     StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

        ''' <summary>
        ''' Il percorso in forma confrontabile. <c>C:\dati</c> e <c>C:\dati\</c> sono la
        ''' stessa cartella, ma non la stessa stringa: senza questo, una radice scritta
        ''' con la barra finale sembrerebbe diversa dalla predefinita anche quando è lei.
        ''' </summary>
        Private Shared Function SenzaBarraFinale(percorso As String) As String
            Return Path.TrimEndingDirectorySeparator(Path.GetFullPath(percorso))
        End Function

        ''' <summary>I numeri del calcolo delle stelle (cap. 11.6).</summary>
        Public ReadOnly Property FileTaratura As String
            Get
                Return Path.Combine(Radice, "taratura.json")
            End Get
        End Property

        ''' <summary>La mappa livello → modello AI (cap. 11.6, cap. 02.5).</summary>
        Public ReadOnly Property FileModelli As String
            Get
                Return Path.Combine(Radice, "modelli.json")
            End Get
        End Property

        ''' <summary>
        ''' I portali e le ricerche salvate (cap. 06.3, cap. 11.1). È il primo file di
        ''' questa mappa che tiene insieme <b>dati di prodotto</b> — la tabella dei
        ''' portali — e <b>roba dell'utente</b>: le ricerche che si è messo da parte.
        ''' </summary>
        Public ReadOnly Property FileRicerche As String
            Get
                Return Path.Combine(Radice, "ricerche.json")
            End Get
        End Property

        ''' <summary>La cartella del profilo e del suo storico.</summary>
        Public ReadOnly Property CartellaProfilo As String
            Get
                Return Path.Combine(Radice, "profilo")
            End Get
        End Property

        ''' <summary>Il profilo corrente: la fonte di verità unica (cap. 11.1).</summary>
        Public ReadOnly Property FileProfilo As String
            Get
                Return Path.Combine(CartellaProfilo, "profilo.json")
            End Get
        End Property

        ''' <summary>
        ''' Le copie datate del profilo, una per versione confermata: è ciò che rende
        ''' un CV già inviato ancora spiegabile a profilo evoluto (cap. 11.1).
        ''' </summary>
        Public ReadOnly Property CartellaStorico As String
            Get
                Return Path.Combine(CartellaProfilo, "storico")
            End Get
        End Property

        ''' <summary>
        ''' Il 📄 CV base: sta <b>col profilo</b> e non con le opportunità, perché nasce
        ''' senza alcun annuncio ed è il ritratto del profilo in forma di CV (cap. 11.1).
        ''' </summary>
        Public ReadOnly Property FileCvBase As String
            Get
                Return Path.Combine(CartellaProfilo, "cv_base.json")
            End Get
        End Property

        ''' <summary>
        ''' I file del 📄 CV base (<c>.docx</c> e <c>.pdf</c>): stanno accanto al profilo,
        ''' come il <c>cv_base.json</c> da cui nascono, perché quel CV non appartiene a
        ''' nessuna candidatura (cap. 11.1).
        ''' </summary>
        Public ReadOnly Property CartellaOutProfilo As String
            Get
                Return Path.Combine(CartellaProfilo, "out")
            End Get
        End Property

        ''' <summary>
        ''' La casa delle candidature: una cartella per opportunità, col nome parlante
        ''' (data, azienda, ruolo). Nasce a T4, la vista d'insieme arriva a T5 — i
        ''' documenti però atterrano subito nella loro casa definitiva (cap. 11.1).
        ''' </summary>
        Public ReadOnly Property CartellaOpportunita As String
            Get
                Return Path.Combine(Radice, "opportunita")
            End Get
        End Property

        ''' <summary>
        ''' La vista d'insieme delle candidature (cap. 07.3, cap. 11.1). È l'unico file di
        ''' questa mappa che non contiene niente di suo: tutto quello che dice sta già
        ''' nelle cartelle-opportunità, e se manca o non torna si <b>rigenera</b> da lì
        ''' (v. <see cref="ArchivioRegistro"/>).
        ''' </summary>
        Public ReadOnly Property FileRegistro As String
            Get
                Return Path.Combine(Radice, "registro.json")
            End Get
        End Property

        ''' <summary>
        ''' Dove il motore del browser tiene i suoi dati di navigazione (cap. 11.1). Serve
        ''' alla stampa PDF, che passa da WebView2, e la useranno il browser integrato
        ''' (T5) e la «pulizia dati» delle Impostazioni (T9).
        ''' </summary>
        ''' <remarks>
        ''' Non è fra le cartelle che <see cref="Assicura"/> crea, ed è voluto: la crea il
        ''' motore alla prima stampa. Un'applicazione aperta e richiusa senza stampare
        ''' niente non deve lasciarsi dietro il profilo di un browser mai usato.
        ''' </remarks>
        Public ReadOnly Property CartellaWebView2 As String
            Get
                Return Path.Combine(Radice, "webview2")
            End Get
        End Property

        ''' <summary>
        ''' Crea le cartelle nominate qui sopra se non ci sono già. È l'unico punto che
        ''' le crea: chi scrive un file la chiama prima e non si preoccupa d'altro.
        ''' </summary>
        Public Sub Assicura()

            ' CreateDirectory crea anche i livelli intermedi e non protesta se la
            ' cartella esiste già: nominarle tutte serve al lettore, non al codice.
            Directory.CreateDirectory(Radice)
            Directory.CreateDirectory(CartellaProfilo)
            Directory.CreateDirectory(CartellaStorico)
            Directory.CreateDirectory(CartellaOutProfilo)
            Directory.CreateDirectory(CartellaOpportunita)

        End Sub

    End Class

End Namespace
