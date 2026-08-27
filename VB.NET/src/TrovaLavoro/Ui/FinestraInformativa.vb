Imports System.Drawing
Imports System.Windows.Forms

''' <summary>Un capitolo dell'informativa: un titolo e quel che c'è da sapere.</summary>
Public NotInheritable Class VoceInformativa

    Public Sub New(titolo As String, testo As String)
        Me.Titolo = titolo
        Me.Testo = testo
    End Sub

    Public ReadOnly Property Titolo As String
    Public ReadOnly Property Testo As String

End Class

''' <summary>
''' «Come funziona, e cosa esce dal tuo PC»: l'informativa e le istruzioni d'uso dentro
''' l'applicazione (cap. 11.2, cap. 13).
''' </summary>
''' <remarks>
''' <para><b>Perché esiste.</b> Il progetto sa da sempre che cosa esce dal PC e che cosa
''' no — è una tabella del cap. 11.2 — ma chi sta davanti alla finestra no, e finché
''' l'unico utente ero io la differenza non si vedeva. La revisione del giro D l'ha
''' chiamata col suo nome: un programma che manda testi a un servizio esterno e chiede una
''' chiave a pagamento deve dirlo <b>prima</b>, e dirlo dove l'utente è, non in un
''' capitolo di progetto.</para>
''' <para><b>Compare una volta sola</b>, al primo avvio, subito prima della finestra della
''' chiave: è il momento in cui si decide se fidarsi, e informare dopo quel momento è
''' informare tardi. Poi non torna più da sé — si riapre da «Informazioni» e dalle
''' Impostazioni, quando la si cerca.</para>
''' <para><b>Il testo sta in un posto solo</b> (<see cref="Voci"/>), fuori dai controlli
''' che lo mostrano: così si collauda senza aprire una finestra, e soprattutto si può
''' verificare che dica la verità su quel che il codice fa davvero. Un'informativa che
''' promette più del programma è peggio di nessuna informativa.</para>
''' </remarks>
Public Class FinestraInformativa

    ''' <summary>Quanto è larga la finestra, e quindi il testo che ci sta dentro.</summary>
    Private Const LarghezzaFinestra As Integer = 660

    Private ReadOnly _righe As New List(Of Label)

    Public Sub New()

        InitializeComponent()

        BackColor = StileApp.SfondoContenuto
        Font = StileApp.FontTesto

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli
        lblTitolo.Text = "Come funziona, e cosa esce dal tuo PC"

        StileApp.VestiBottone(btnChiudi, LivelloBottone.Neutro)

        Scrivi()
        Disponi()

        AcceptButton = btnChiudi
        CancelButton = btnChiudi

    End Sub

    ''' <summary>
    ''' Che cosa dice l'informativa. È un elenco e non un testo unico perché ogni voce
    ''' risponde a una domanda diversa, e chi cerca una risposta sola deve trovarla senza
    ''' leggere tutto.
    ''' </summary>
    ''' <remarks>
    ''' Ogni riga qui dentro deve corrispondere a qualcosa che il codice fa davvero: è il
    ''' motivo per cui questo elenco è collaudato insieme al resto invece di vivere in un
    ''' file di testo che nessuno rilegge quando il programma cambia.
    ''' </remarks>
    Public Shared Function Voci() As IReadOnlyList(Of VoceInformativa)

        Return New List(Of VoceInformativa) From {
            New VoceInformativa("A che serve",
                "TrovaLavoro prepara candidature: racconti una volta chi sei, poi per ogni annuncio " &
                "il programma lo confronta col tuo profilo, ti dà un punteggio in stelle e — se vale " &
                "la pena — scrive un CV e una lettera su misura, più l'email con gli allegati già " &
                "pronti." & vbLf &
                "Non inventa mai un'esperienza che tu non abbia dichiarato: se per quell'annuncio " &
                "manca qualcosa, te lo dice invece di riempirlo."),
            New VoceInformativa("Serve una chiave API, e si paga a consumo",
                "Il programma non ha un abbonamento suo: usa la tua chiave di Anthropic, che paghi " &
                "tu a consumo, in dollari, direttamente a loro. Quanto costa un giro dipende da " &
                "quanto sono lunghi i tuoi testi." & vbLf &
                "Il conto di quel che è passato finora lo trovi in Impostazioni, sotto «Quanto è " &
                "costato»: è una stima ai prezzi di listino, e la verità resta la fattura di Anthropic."),
            New VoceInformativa("Che cosa esce dal tuo PC",
                "All'AI di Anthropic vanno solo i testi che le dai da lavorare: il tuo profilo, " &
                "l'annuncio, un CV in PDF quando chiedi di trascriverlo. Viaggiano cifrati, e servono " &
                "a produrre la risposta che aspetti." & vbLf &
                "Ai portali di lavoro va quello che va sempre navigando: le pagine che apri nel " &
                "browser incorporato. Le credenziali le digiti tu sul loro sito e il programma non le vede." & vbLf &
                "A GitHub va una richiesta sola, e solo se premi «Cerca aggiornamenti» in " &
                "«Informazioni»: serve a sapere se c'è una versione nuova."),
            New VoceInformativa("Che cosa non esce",
                "Tutto il resto resta sul tuo computer: le candidature, i documenti prodotti, le " &
                "email, le impostazioni, il diario tecnico dei guasti." & vbLf &
                "Non c'è telemetria, non ci sono statistiche d'uso, non c'è nessun invio automatico " &
                "e il programma non si aggiorna da solo." & vbLf &
                "L'email di candidatura non parte da qui: viene scritta come file e la spedisci tu " &
                "col tuo programma di posta, dopo averla riletta."),
            New VoceInformativa("Dove restano i tuoi dati",
                "In una cartella tua, che le Impostazioni ti aprono con un clic. Dentro ci sono file " &
                "leggibili con qualunque editor: sei padrone dei tuoi dati anche senza questo programma." & vbLf &
                "L'unica eccezione è la chiave API, che è cifrata per il tuo utente di Windows: non è " &
                "leggibile da un altro account né da un altro PC." & vbLf &
                "Dalle Impostazioni puoi fare un backup di tutto, ripristinarlo, o cancellare ogni cosa."),
            New VoceInformativa("Il giro, in breve",
                "1. Racconti il tuo profilo — a voce, rispondendo alle domande, oppure importando un CV." & vbLf &
                "2. Trovi o incolli un annuncio, e il programma lo analizza." & vbLf &
                "3. Il confronto ti dà le stelle e ti spiega dove sei forte e dove no." & vbLf &
                "4. Se decidi di candidarti, nascono CV e lettera su misura, e l'email da spedire." & vbLf &
                "Ogni testo generato lo puoi rileggere e riscrivere a mano prima che diventi un documento.")}

    End Function

    ''' <summary>Apre l'informativa davanti a chi l'ha chiesta.</summary>
    Public Shared Sub Mostra(proprietario As IWin32Window)
        Using finestra As New FinestraInformativa()
            finestra.ShowDialog(proprietario)
        End Using
    End Sub

    ''' <summary>Le etichette costruite, per il banco.</summary>
    Public ReadOnly Property Righe As IReadOnlyList(Of Label)
        Get
            Return _righe
        End Get
    End Property

    ''' <summary>Tutto il testo che la finestra mostra, di seguito. Per il banco.</summary>
    Public ReadOnly Property TestoIntero As String
        Get
            Return String.Join(vbLf, _righe.Select(Function(riga) riga.Text))
        End Get
    End Property

    ''' <summary>Costruisce un'etichetta per ogni titolo e per ogni testo.</summary>
    Private Sub Scrivi()

        For Each voce As VoceInformativa In Voci()

            _righe.Add(NuovaRiga(voce.Titolo, StileApp.FontTitoloGruppo, StileApp.TestoPrimario))
            _righe.Add(NuovaRiga(voce.Testo, StileApp.FontTesto, StileApp.TestoPrimario))

        Next

    End Sub

    Private Function NuovaRiga(testo As String, carattere As Font, colore As Color) As Label

        Dim riga As New Label With {
            .AutoSize = True,
            .Text = testo,
            .Font = carattere,
            .ForeColor = colore}

        Controls.Add(riga)
        Return riga

    End Function

    ''' <summary>Mette in fila i capitoli nello spazio che lo schermo concede.</summary>
    Private Sub Disponi()

        DisponiIn(ScalaSchermo.SpazioClienteDisponibile(
            Screen.FromControl(Me).WorkingArea.Height, Me.Height - Me.ClientSize.Height))

    End Sub

    ''' <summary>
    ''' Mette in fila i capitoli come se in altezza ci fosse questo spazio. Stessa regola
    ''' delle Impostazioni: un tetto sullo spazio che c'è e lo scorrimento per il resto,
    ''' e quando si scorre la fila si rifà dentro la larghezza che la barra lascia
    ''' (cap. 03.4).
    ''' </summary>
    Public Sub DisponiIn(altezzaDisponibile As Integer)

        Dim larghezza As Integer = ScalaSchermo.InPixelDelloSchermo(LarghezzaFinestra, Me.DeviceDpi)

        Dim voluta As Integer = MettiInFila(larghezza)
        Dim siScorre As Boolean = ScalaSchermo.ServeScorrimento(voluta, altezzaDisponibile)

        If siScorre Then
            voluta = MettiInFila(ScalaSchermo.LarghezzaSenzaLaBarra(
                larghezza, siScorre, SystemInformation.VerticalScrollBarWidth))
        End If

        Me.AutoScroll = siScorre
        ClientSize = New Size(larghezza, ScalaSchermo.AltezzaSostenibile(voluta, altezzaDisponibile))

    End Sub

    Private Function MettiInFila(larghezza As Integer) As Integer

        Dim sinistra As Integer = StileApp.MargineRiquadro
        Dim larghezzaUtile As Integer = larghezza - 2 * StileApp.MargineRiquadro

        lblTitolo.MaximumSize = New Size(larghezzaUtile, 0)
        lblTitolo.Location = New Point(sinistra, StileApp.MargineRiquadro)

        Dim sotto As Integer = lblTitolo.Bottom

        For quale As Integer = 0 To _righe.Count - 1

            Dim riga As Label = _righe(quale)
            riga.MaximumSize = New Size(larghezzaUtile, 0)

            ' Un titolo di capitolo prende aria sopra; il suo testo sta attaccato a lui,
            ' o le due cose sembrerebbero due voci separate.
            Dim stacco As Integer = If(quale Mod 2 = 0, StileApp.MargineRiquadro, StileApp.InterlineaMinima)
            riga.Location = New Point(sinistra, sotto + stacco)
            sotto = riga.Bottom

        Next

        btnChiudi.Location = New Point(larghezza - StileApp.MargineRiquadro - btnChiudi.Width,
                                       sotto + StileApp.MargineRiquadro)

        Return btnChiudi.Bottom + StileApp.MargineRiquadro

    End Function

    Private Sub Chiudi(mittente As Object, e As EventArgs) Handles btnChiudi.Click
        Close()
    End Sub

End Class
