Imports System.Drawing
Imports System.Windows.Forms

''' <summary>
''' Quanto pesa la conseguenza di un bottone (cap. 03.3). La saturazione del colore
''' cresce con il peso: chi guarda deve capire cosa succede <i>prima</i> di leggere
''' l'etichetta. Nel dubbio fra due livelli si sceglie il più alto.
''' </summary>
Public Enum LivelloBottone
    ''' <summary>Navigazione, annulla, chiudi.</summary>
    Neutro = 0
    ''' <summary>Conferme senza rischio: «Salva profilo», «Cattura annuncio».</summary>
    SicuroPositivo = 1
    ''' <summary>Aprire, sfogliare, vedere anteprime.</summary>
    Esplorativo = 2
    ''' <summary>Il bottone «avanti» del flusso: «Genera CV», «Confronta».</summary>
    AzionePrincipale = 3
    ''' <summary>Modifica di dati esistenti: «Sovrascrivi profilo», «Rigenera».</summary>
    Attenzione = 4
    ''' <summary>Eliminare, scartare.</summary>
    Distruttivo = 5
    ''' <summary>Inviare un'email, cancellazioni definitive: sempre dopo una conferma.</summary>
    Critico = 6
End Enum

''' <summary>
''' Token di design dell'applicazione (cap. 03.2): colori, font, spaziature.
''' Tutta l'interfaccia pesca da qui e solo da qui: nei form non compaiono mai
''' Color.FromArgb né New Font.
''' </summary>
Public Module StileApp

    ' --- Colori (cap. 03.2) ---

    ''' <summary>Testo normale, valori, titoli di sezione.</summary>
    Public ReadOnly TestoPrimario As Color = ColorTranslator.FromHtml("#212529")

    ''' <summary>Didascalie, suggerimenti, stati.</summary>
    Public ReadOnly TestoSecondario As Color = ColorTranslator.FromHtml("#6C757D")

    ''' <summary>Sfondo delle finestre.</summary>
    Public ReadOnly SfondoBase As Color = ColorTranslator.FromHtml("#F8F9FA")

    ''' <summary>Aree di lavoro: testi, anteprime, input.</summary>
    Public ReadOnly SfondoContenuto As Color = ColorTranslator.FromHtml("#FFFFFF")

    ''' <summary>Separatori e bordi da 1 px.</summary>
    Public ReadOnly BordoLeggero As Color = ColorTranslator.FromHtml("#DEE2E6")

    ''' <summary>Bordo dei controlli interattivi.</summary>
    Public ReadOnly BordoForte As Color = ColorTranslator.FromHtml("#CED4DA")

    ''' <summary>Focus, link, selezione (blu profondo).</summary>
    Public ReadOnly Accento As Color = ColorTranslator.FromHtml("#0B06B0")

    ''' <summary>Riga selezionata, hover.</summary>
    Public ReadOnly AccentoTenue As Color = ColorTranslator.FromHtml("#E4E7FB")

    ''' <summary>Fondo del bottone d'azione principale del pannello.</summary>
    Public ReadOnly FondoAzione As Color = ColorTranslator.FromHtml("#C0E8FF")

    ''' <summary>Titoli delle finestre e dei GroupBox, marker.</summary>
    Public ReadOnly RossoTitoli As Color = ColorTranslator.FromHtml("#FA0825")

    ''' <summary>Azioni sicure/positive, badge OK.</summary>
    Public ReadOnly Successo As Color = ColorTranslator.FromHtml("#28A745")

    ''' <summary>Azioni che modificano, badge attenzione.</summary>
    Public ReadOnly Avviso As Color = ColorTranslator.FromHtml("#FFC107")

    ''' <summary>Azioni distruttive, badge errore.</summary>
    Public ReadOnly Pericolo As Color = ColorTranslator.FromHtml("#DC3545")

    ''' <summary>Badge informativi.</summary>
    Public ReadOnly Informazione As Color = ColorTranslator.FromHtml("#17A2B8")

    ' --- Font (cap. 03.2) ---

    ''' <summary>Un solo font in tutta l'applicazione.</summary>
    Public Const NomeFont As String = "Segoe UI"

    ''' <summary>Font dei soli dati tecnici.</summary>
    Public Const NomeFontTecnico As String = "Consolas"

    ''' <summary>Titolo di finestra o di pannello (con RossoTitoli).</summary>
    Public ReadOnly FontTitoloFinestra As New Font(NomeFont, 16.0F, FontStyle.Bold)

    ''' <summary>Titolo di pannello più contenuto (con RossoTitoli).</summary>
    Public ReadOnly FontTitoloPannello As New Font(NomeFont, 14.0F, FontStyle.Bold)

    ''' <summary>Titolo di GroupBox (con RossoTitoli).</summary>
    Public ReadOnly FontTitoloGruppo As New Font(NomeFont, 9.0F, FontStyle.Bold)

    ''' <summary>Bottone d'azione principale del pannello (livello 3).</summary>
    Public ReadOnly FontAzionePrincipale As New Font(NomeFont, 9.75F, FontStyle.Bold)

    ''' <summary>Testo di lavoro e bottoni neutri.</summary>
    Public ReadOnly FontTesto As New Font(NomeFont, 9.0F)

    ''' <summary>
    ''' Il grassetto dei bottoni che pesano (livelli 1, 4, 5 e 6 della tabella 03.3).
    ''' Non è un ruolo nuovo: è il «bold» che quella tabella chiede, nel corpo del
    ''' testo di lavoro.
    ''' </summary>
    Public ReadOnly FontBottoneForte As New Font(NomeFont, 9.0F, FontStyle.Bold)

    ''' <summary>Didascalie e suggerimenti (con TestoSecondario).</summary>
    Public ReadOnly FontDidascalia As New Font(NomeFont, 8.0F)

    ''' <summary>Punteggi, log e altri dati tecnici.</summary>
    Public ReadOnly FontDatiTecnici As New Font(NomeFontTecnico, 8.5F)

    ' --- Spaziature e dimensioni: regola 14 / 12 / 8 (cap. 03.2) ---

    ''' <summary>Margine interno di GroupBox e riquadri.</summary>
    Public Const MargineRiquadro As Integer = 14

    ''' <summary>Distanza tra controlli affiancati.</summary>
    Public Const DistanzaControlli As Integer = 12

    ''' <summary>Distanza minima tra le righe.</summary>
    Public Const InterlineaMinima As Integer = 8

    ''' <summary>Bottone standard, testo breve.</summary>
    Public ReadOnly BottoneStandard As New Size(110, 32)

    ''' <summary>Bottone standard, testo medio.</summary>
    Public ReadOnly BottoneMedio As New Size(130, 32)

    ''' <summary>Bottone della barra superiore di navigazione.</summary>
    Public ReadOnly BottoneBarraSuperiore As New Size(110, 34)

    ' --- I livelli di conseguenza (cap. 03.3) ---

    ''' <summary>
    ''' Veste un bottone secondo il peso della sua conseguenza. Esiste per la regola
    ''' più esigente del capitolo 03: <i>due bottoni con la stessa funzione, in due
    ''' pannelli diversi, sono identici</i>. Ripetuto a mano in ogni designer, quel
    ''' «identici» sarebbe durato fino al terzo pannello.
    ''' </summary>
    ''' <remarks>
    ''' <para><c>FlatStyle.Flat</c> e <c>UseVisualStyleBackColor = False</c> valgono per
    ''' tutti i livelli: senza il secondo, Windows ridipinge il fondo di suo e il colore
    ''' scelto qui non si vede.</para>
    ''' <para>Proprio per questo il bottone si ridipinge anche quando viene <b>spento</b>:
    ''' un bottone piatto con un colore suo resta acceso all'occhio anche da disabilitato,
    ''' e sembrerebbe premibile quando non lo è. Il livello resta scritto nel
    ''' <c>Tag</c>, così quando torna abilitato ritrova il colore che gli spetta.</para>
    ''' </remarks>
    Public Sub VestiBottone(bottone As Button, livello As LivelloBottone)

        If bottone Is Nothing Then Throw New ArgumentNullException(NameOf(bottone))

        bottone.Tag = livello

        ' Rimuovere prima di aggiungere rende la vestizione ripetibile senza accumulare
        ' gestori sullo stesso bottone.
        RemoveHandler bottone.EnabledChanged, AddressOf BottoneAccesoOSpento
        AddHandler bottone.EnabledChanged, AddressOf BottoneAccesoOSpento

        Dipingi(bottone, livello)

    End Sub

    Private Sub BottoneAccesoOSpento(mittente As Object, e As EventArgs)

        Dim bottone As Button = TryCast(mittente, Button)
        If bottone Is Nothing OrElse Not TypeOf bottone.Tag Is LivelloBottone Then Return

        Dipingi(bottone, DirectCast(bottone.Tag, LivelloBottone))

    End Sub

    ''' <summary>Dà al bottone l'aspetto del suo livello, o quello spento se è disabilitato.</summary>
    Private Sub Dipingi(bottone As Button, livello As LivelloBottone)

        bottone.FlatStyle = FlatStyle.Flat
        bottone.UseVisualStyleBackColor = False
        bottone.Font = FontTesto
        bottone.ForeColor = TestoPrimario
        bottone.FlatAppearance.BorderSize = 1
        bottone.FlatAppearance.BorderColor = BordoLeggero

        ' Il font del livello si applica comunque: spento o acceso, un bottone che pesa
        ' resta scritto in grassetto e non cambia ingombro quando si riaccende.
        Select Case livello
            Case LivelloBottone.SicuroPositivo, LivelloBottone.Attenzione,
                 LivelloBottone.Distruttivo, LivelloBottone.Critico,
                 LivelloBottone.Esplorativo
                bottone.Font = FontBottoneForte
            Case LivelloBottone.AzionePrincipale
                bottone.Font = FontAzionePrincipale
        End Select

        If Not bottone.Enabled Then
            bottone.BackColor = SfondoBase
            bottone.ForeColor = TestoSecondario
            Return
        End If

        Select Case livello

            Case LivelloBottone.Neutro
                bottone.BackColor = SfondoContenuto

            Case LivelloBottone.SicuroPositivo
                bottone.BackColor = Successo
                bottone.ForeColor = SfondoContenuto
                bottone.FlatAppearance.BorderColor = Successo

            Case LivelloBottone.Esplorativo
                bottone.BackColor = AccentoTenue
                ' Il bordo è dell'accento e non grigio: il fondo tenue di questo livello,
                ' su una finestra già chiara, si legge quasi come quello di un bottone
                ' spento — ed è successo davvero, con «Esporta PDF» e «Esporta DOCX»
                ' creduti morti mentre funzionavano (T9d, 2026-08-22). Il colore del bordo
                ' è la differenza più piccola che si vede a colpo d'occhio, e lo avvicina
                ' al livello sopra — che d'accento ha il bordo e in più il fondo azzurro e
                ' il carattere grande: la parentela è voluta, l'esplorativo resta il fratello
                ' quieto dell'azione principale.
                '
                ' Il bordo però non è bastato: alla **seconda** prova dal vivo dello stesso
                ' giorno «Esporta PDF» e «Esporta DOCX» erano ancora letti come spenti. La
                ' ragione è che l'occhio giudica un bottone dal **testo** prima che dal
                ' contorno, e nero-su-azzurrino è esattamente ciò che nell'app significa
                ' spento (grigio su grigio): il fondo tenue non basta a smentirlo. Adesso le
                ' lettere sono d'accento, il contorno è doppio e il carattere è quello dei
                ' bottoni che pesano — tre segnali invece di uno, e nessuno tolto allo
                ' spento, che resta grigio in tutto.
                bottone.ForeColor = Accento
                bottone.FlatAppearance.BorderSize = 2
                bottone.FlatAppearance.BorderColor = Accento

            Case LivelloBottone.AzionePrincipale
                bottone.BackColor = FondoAzione
                bottone.FlatAppearance.BorderColor = Accento

            Case LivelloBottone.Attenzione
                bottone.BackColor = Avviso
                bottone.FlatAppearance.BorderColor = Avviso

            Case LivelloBottone.Distruttivo
                bottone.BackColor = Pericolo
                bottone.ForeColor = SfondoContenuto
                bottone.FlatAppearance.BorderColor = Pericolo

            Case Else ' Critico
                bottone.BackColor = RossoTitoli
                bottone.ForeColor = SfondoContenuto
                bottone.FlatAppearance.BorderColor = RossoTitoli

        End Select

    End Sub

End Module
