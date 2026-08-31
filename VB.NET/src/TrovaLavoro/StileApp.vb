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
''' Che parte fa una casella della barra superiore (cap. 03.4). Non è un livello di
''' conseguenza — la navigazione non ha conseguenze, e i livelli della tabella 03.3
''' misurano proprio quelle: è il posto che una casella occupa in quella fila.
''' </summary>
Public Enum RuoloBarra
    ''' <summary>Una delle sei porte dei pannelli: l'azzurro del menu d'ingresso.</summary>
    Destinazione = 0
    ''' <summary>La casella che riporta al menu d'ingresso: il verde.</summary>
    RitornoAlMenu = 1
End Enum

''' <summary>
''' Token di design dell'applicazione (cap. 03.2): colori, font, spaziature.
''' Tutta l'interfaccia pesca da qui e solo da qui: nei form non compaiono mai
''' Color.FromArgb né New Font.
''' </summary>
Public Module StileApp

    ' --- Colori (cap. 03.2) ---

    ''' <summary>
    ''' Il blu del marchio: è il fondo della schermata di avvio e dell'immagine
    ''' che apre «Informazioni su…» (cap. 13.5). Sta qui perché nessun form scriva un
    ''' colore a mano — e perché la finestra ce l'ha sotto l'immagine, dove si vedrebbe
    ''' subito una banda di un blu leggermente diverso.
    ''' <para>Dal 2026-08-30 è il blu Aviolab, lo stesso di <see cref="Accento"/>: il
    ''' banner nuovo porta cornice e fasce di quel blu, e il vecchio blu notte
    ''' <c>#000C38</c> — che alla tavolozza dello stemma non apparteneva — lasciava
    ''' una banda visibile sotto l'immagine. Restano due costanti perché sono due
    ''' ruoli: se un domani il marchio cambia fondo, cambia questa e non l'accento.</para>
    ''' </summary>
    Public ReadOnly FondoMarchio As Color = ColorTranslator.FromHtml("#0B06B0")

    ''' <summary>Il fondo del menu d'ingresso (cap. 03.6): un avorio caldo.</summary>
    ''' <remarks>
    ''' Non e' un bianco. <c>#FFFAF0</c> porta dentro un velo di giallo che scalda la
    ''' schermata su cui l'applicazione si apre, e la stacca dal <see cref="SfondoBase"/>
    ''' grigino delle finestre di lavoro: il menu non e' una finestra di lavoro, e' la
    ''' soglia. Dal 2026-08-30 prende il posto del banner, che del menu era lo sfondo
    ''' intero; il marchio ci resta come <b>mega stemma</b> dietro i bottoni, e il nome
    ''' come scritta in cima (cap. 03.6).
    ''' <para>Su questo fondo il velo bianco del menu non ha quasi presa — lo porterebbe
    ''' a <c>#FFFDF8</c>, che a occhio e' lo stesso colore — e infatti li' il velo non
    ''' serve a schiarire il fondo ma solo cio' che ci sta sopra.</para>
    ''' </remarks>
    Public ReadOnly FondoMenu As Color = ColorTranslator.FromHtml("#FFFAF0")

    ''' <summary>Il filo che contorna il pannello del logo (cap. 03.5).</summary>
    ''' <remarks>
    ''' Nero pieno, e non <see cref="BordoLeggero"/>: quel grigio serve a separare due
    ''' aree della stessa finestra, questo a dire che il marchio è una cosa a parte,
    ''' appoggiata sopra. È l'unico nero della tavolozza — <see cref="TestoPrimario"/> è
    ''' un grigio scurissimo, non nero — ed è nero perché un contorno più chiaro sul
    ''' fondo chiaro del pannello si sarebbe visto appena.
    ''' </remarks>
    Public ReadOnly BordoMarchio As Color = ColorTranslator.FromHtml("#000000")

    ''' <summary>Testo normale, valori, titoli di sezione.</summary>
    Public ReadOnly TestoPrimario As Color = ColorTranslator.FromHtml("#212529")

    ''' <summary>Didascalie, suggerimenti, stati.</summary>
    ''' <remarks>
    ''' Dal 2026-08-30 non è più <c>#6C757D</c>, il grigio da cui viene: su
    ''' <see cref="SfondoBase"/> quello faceva <b>4,45 a 1</b>, un centesimo sotto il
    ''' 4,5 che WCAG 2 chiede a un testo piccolo — ed è la coppia con cui l'applicazione
    ''' scrive <i>ogni</i> didascalia, non un caso di confine. Tre punti più scuro in
    ''' tutto, che a occhio non si distinguono, lo portarono a <b>4,57</b>.
    ''' <para>Dal 2026-08-31 ne scende altri due, a <c>#68717A</c>, e per la stessa
    ''' ragione a un giro di distanza: le pagine hanno preso il fondo caldo
    ''' (<see cref="FondoPagina"/>), che parte già sotto il bianco, e lì il grigio di ieri
    ''' faceva <b>4,39</b>. Adesso fa <b>4,52</b> sul fondo delle pagine, 4,71 su
    ''' <see cref="SfondoBase"/> e 4,77 su <see cref="FondoCasella"/>: il fondo si è
    ''' scaldato, la didascalia si è scurita, e la coppia resta sopra la soglia su
    ''' <b>tutti e quattro</b> i fondi su cui l'applicazione la scrive.</para>
    ''' </remarks>
    Public ReadOnly TestoSecondario As Color = ColorTranslator.FromHtml("#68717A")

    ''' <summary>Sfondo delle finestre.</summary>
    Public ReadOnly SfondoBase As Color = ColorTranslator.FromHtml("#F8F9FA")

    ''' <summary>Aree di lavoro: testi, anteprime, input.</summary>
    Public ReadOnly SfondoContenuto As Color = ColorTranslator.FromHtml("#FFFFFF")

    ''' <summary>Il fondo delle pagine che si aprono dal menu (cap. 03.6).</summary>
    ''' <remarks>
    ''' <see cref="SfondoBase"/> scaldato: è <see cref="FondoMenu"/> con sopra lo stesso
    ''' velo che porta il fondo delle finestre sotto il bianco — sette punti di rosso, sei
    ''' di verde, cinque di blu — e viene lo stesso identico stacco di prima (0,053 di
    ''' luminanza contro 0,054). Dal 2026-08-31 le sei pagine non sono più isole grigie
    ''' aperte da una soglia avorio: l'avorio entra con loro, e il velo resta a dire dove
    ''' finisce la pagina e comincia l'area di lavoro.
    ''' <para>Non sostituisce <see cref="SfondoBase"/>, gli sta accanto: quel grigino resta
    ''' il fondo delle <b>finestre</b> — impostazioni, backup, conferme — e il fondo dei
    ''' bottoni spenti, che di questo giro non erano.</para>
    ''' </remarks>
    Public ReadOnly FondoPagina As Color = ColorTranslator.FromHtml("#F8F4EB")

    ''' <summary>Le aree di lavoro dentro quelle pagine: caselle, elenchi, schede.</summary>
    ''' <remarks>
    ''' È <see cref="FondoMenu"/> preso di peso — lo stesso avorio, lo stesso codice — e
    ''' sta qui come costante propria per la ragione per cui <see cref="FondoMarchio"/> sta
    ''' accanto ad <see cref="Accento"/>: sono due ruoli, e se un domani la soglia cambia
    ''' colore non deve trascinarsi dietro ogni casella dell'applicazione.
    ''' <para>Gemello caldo di <see cref="SfondoContenuto"/>, che resta il bianco delle
    ''' finestre — con una sola eccezione, e dichiarata: le <b>Impostazioni</b>, che sono
    ''' la settima porta del menu e si aprono in una finestra solo per come sono fatte,
    ''' prendono questo fondo come le altre sei destinazioni.</para>
    ''' </remarks>
    Public ReadOnly FondoCasella As Color = ColorTranslator.FromHtml("#FFFAF0")

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

    ''' <summary>Il contorno che va col fondo <see cref="Successo"/> (cap. 03.4).</summary>
    ''' <remarks>
    ''' Un verde scurissimo, non il nero e non il verde stesso: serve alla casella
    ''' «🎮 Menu» della barra superiore, che è l'unica di quella fila a portare un fondo
    ''' pieno e ha bisogno di un bordo per restare una forma sul bianco della barra.
    ''' Contornarla del suo stesso verde la lascerebbe senza contorno; contornarla di nero
    ''' la farebbe un corpo estraneo, perché il nero qui dentro è del solo marchio
    ''' (<see cref="BordoMarchio"/>). Questo è il verde di famiglia, portato al buio.
    ''' </remarks>
    Public ReadOnly BordoSuccesso As Color = ColorTranslator.FromHtml("#0A2C11")

    ''' <summary>I pallini che girano sullo scudo mentre l'AI lavora (cap. 03.8).</summary>
    ''' <remarks>
    ''' Un argento freddo, non il bianco: sullo scudo passa sopra il blu, il rosso e il
    ''' giallo del marchio, e il bianco puro sparirebbe proprio sulle stelle. Gli fa da
    ''' contorno <see cref="OmbraDiAttesa"/>, che è quel che lo stacca dalle parti chiare —
    ''' un pallino argento su fondo giallo, senza, non si vede affatto.
    ''' </remarks>
    Public ReadOnly ArgentoDiAttesa As Color = ColorTranslator.FromHtml("#E2E8F0")

    ''' <summary>Il contorno dei pallini dell'attesa: un grigio di piombo (cap. 03.8).</summary>
    Public ReadOnly OmbraDiAttesa As Color = ColorTranslator.FromHtml("#4A5568")

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

    ''' <summary>Bottone della barra superiore che porta un nome lungo.</summary>
    ''' <remarks>
    ''' Ne esistono due — «Le mie candidature» e «Confronta ★ ANNUNCIO - CV» — e a 110
    ''' pixel il nome ci finiva tagliato: un bottone della barra non manda a capo e non
    ''' mette i puntini, taglia e basta. Allargarli non è un'eccezione allo stile, è la
    ''' stessa misura con la larghezza che quei testi chiedono; l'altezza resta identica,
    ''' perché la barra deve restare una riga sola. La larghezza la decide il <b>più
    ''' lungo dei due</b> e vale per entrambi: due bottoni larghi uguale sono una barra,
    ''' due larghi quasi uguale sono una svista.
    ''' </remarks>
    Public ReadOnly BottoneBarraSuperioreLargo As New Size(210, 34)

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

    ' --- La barra superiore (cap. 03.4) ---

    ''' <summary>
    ''' Veste una casella della barra di navigazione: il verde del ritorno al menu, o
    ''' l'azzurro delle sei destinazioni, con la cornice d'accento se è quella aperta.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché non uno dei livelli di 03.3.</b> Quelli dicono quanto pesa la
    ''' conseguenza di un bottone, e una navigazione non pesa niente: fino al 2026-08-30
    ''' la barra era infatti tutta di livello 0 — bianca — e l'unico segno del pannello
    ''' aperto era un fondo lilla. La barra però non è una fila di comandi qualunque: è
    ''' l'indice dell'applicazione, e le sue caselle sono le stesse sei voci del menu
    ''' d'ingresso più la porta di casa. Prendono perciò i colori di quel menu — azzurro
    ''' le sei voci, verde il ritorno — così chi passa dal menu alla barra ritrova le
    ''' stesse cose, invece di trovarne sette bianche tutte uguali.</para>
    ''' <para><b>Il pannello aperto si vede dalla cornice, non dal fondo.</b> Con il
    ''' riposo azzurro il lilla di prima non si distinguerebbe più: adesso la casella
    ''' aperta tiene il suo azzurro e prende cornice doppia e lettere del blu d'accento —
    ''' due dei tre segnali del livello 2 (03.3), che qui dentro vogliono già dire «questo
    ''' è vivo». Il terzo, il grassetto, non può più distinguerla: da quando la barra è
    ''' tutta in grassetto ce l'hanno già tutte.</para>
    ''' <para><b>Lo spento si smorza.</b> Vale qui la ragione di
    ''' <see cref="VestiBottone"/>: un bottone piatto con un colore suo resta acceso
    ''' all'occhio anche da disabilitato, e mentre l'AI lavora la barra si spegne tutta
    ''' (cap. 02.6). Il ruolo e lo stato restano scritti nel <c>Tag</c>, così quando la
    ''' barra si riapre ogni casella ritrova il colore che le spetta.</para>
    ''' </remarks>
    Public Sub VestiBottoneBarra(bottone As Button, ruolo As RuoloBarra, attiva As Boolean)

        If bottone Is Nothing Then Throw New ArgumentNullException(NameOf(bottone))

        bottone.Tag = New VesteDiBarra(ruolo, attiva)

        ' Rimuovere prima di aggiungere rende la vestizione ripetibile: MostraPannello la
        ' rifà a ogni cambio di pannello, e i gestori si accumulerebbero.
        RemoveHandler bottone.EnabledChanged, AddressOf CasellaAccesaOSpenta
        AddHandler bottone.EnabledChanged, AddressOf CasellaAccesaOSpenta

        DipingiLaCasella(bottone, ruolo, attiva)

    End Sub

    ''' <summary>Ruolo e stato di una casella della barra, tenuti nel suo <c>Tag</c>.</summary>
    Private Structure VesteDiBarra

        Public ReadOnly Ruolo As RuoloBarra
        Public ReadOnly Attiva As Boolean

        Public Sub New(ruolo As RuoloBarra, attiva As Boolean)
            Me.Ruolo = ruolo
            Me.Attiva = attiva
        End Sub

    End Structure

    Private Sub CasellaAccesaOSpenta(mittente As Object, e As EventArgs)

        Dim bottone As Button = TryCast(mittente, Button)
        If bottone Is Nothing OrElse Not TypeOf bottone.Tag Is VesteDiBarra Then Return

        Dim veste As VesteDiBarra = DirectCast(bottone.Tag, VesteDiBarra)
        DipingiLaCasella(bottone, veste.Ruolo, veste.Attiva)

    End Sub

    ''' <summary>Dà alla casella l'aspetto del suo ruolo, o quello spento se è disabilitata.</summary>
    Private Sub DipingiLaCasella(bottone As Button, ruolo As RuoloBarra, attiva As Boolean)

        bottone.FlatStyle = FlatStyle.Flat
        bottone.UseVisualStyleBackColor = False

        ' Il carattere è quello dei bottoni che pesano, e vale per tutte: la fila in cima è
        ' l'indice del programma, e i suoi sette nomi si leggono di sfuggita mentre si sta
        ' facendo altro — come le voci del menu d'ingresso, che sono in grassetto per la
        ' stessa ragione. Non dipende né dal ruolo né dall'essere aperta o accesa: una
        ' casella che cambia carattere cambia ingombro, e la fila si muoverebbe sotto gli
        ' occhi a ogni clic e a ogni chiamata all'AI.
        bottone.Font = FontBottoneForte
        bottone.FlatAppearance.BorderSize = If(attiva, 2, 1)

        If Not bottone.Enabled Then
            bottone.BackColor = SfondoBase
            bottone.ForeColor = TestoSecondario
            bottone.FlatAppearance.BorderColor = BordoLeggero
            Return
        End If

        If ruolo = RuoloBarra.RitornoAlMenu Then
            bottone.BackColor = Successo
            bottone.ForeColor = SfondoContenuto
            bottone.FlatAppearance.BorderColor = BordoSuccesso
        Else
            bottone.BackColor = FondoAzione
            bottone.ForeColor = If(attiva, Accento, TestoPrimario)
            bottone.FlatAppearance.BorderColor = If(attiva, Accento, BordoForte)
        End If

    End Sub

End Module

