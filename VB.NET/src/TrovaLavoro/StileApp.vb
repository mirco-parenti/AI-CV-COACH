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

    ''' <summary>Titoli delle finestre e dei pannelli, marker.</summary>
    ''' <remarks>
    ''' È il rosso del marchio, ed è acceso: come inchiostro fa <b>3,73 a 1</b> sul fondo
    ''' delle pagine e 4,10 sul bianco, sotto il 4,5 che WCAG chiede a un testo piccolo.
    ''' Per questo dal 2026-09-01 gli restano i titoli <b>grandi</b> — 14 e 16 punti, dove
    ''' la soglia è 3 — mentre i titoli di gruppo, che sono 9 punti, passano a
    ''' <see cref="RossoCritico"/>: all'occhio è lo stesso rosso di famiglia, ma si legge.
    ''' </remarks>
    Public ReadOnly RossoTitoli As Color = ColorTranslator.FromHtml("#FA0825")

    ''' <summary>Azioni sicure/positive, badge OK.</summary>
    ''' <remarks>
    ''' Dal 2026-09-01 è più scuro del <c>#28A745</c> da cui viene, e per una ragione che
    ''' valeva in tutti e due i versi in cui questo verde si usa. Sotto il <b>bianco</b> —
    ''' il bottone di livello 1 (cap. 03.3) e la casella «🎮 Menu» della barra (cap. 03.4),
    ''' che è dove il token si vede più spesso — faceva <b>3,13 a 1</b>, lontano dal 4,5;
    ''' adesso fa <b>5,14</b>. E come <b>inchiostro</b> — l'esito «Assunto 🎉» di P4 — sui
    ''' fondi chiari faceva 2,85, cioè meno di quanto costi leggerlo: adesso fa 4,68 sul
    ''' fondo delle pagine, 4,94 dentro le caselle e 5,14 sul bianco. Resta un verde solo
    ''' per tutti i suoi usi: un colore che fa sia da fondo sia da testo deve reggere nei
    ''' due versi, e sdoppiarlo avrebbe messo due verdi di famiglia nella stessa schermata.
    ''' </remarks>
    Public ReadOnly Successo As Color = ColorTranslator.FromHtml("#1E7E34")

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

    ''' <summary>Il verde della barra che si riempie mentre l'AI lavora (cap. 03.8).</summary>
    ''' <remarks>
    ''' <para>È un colore <b>preso in prestito da fuori</b>, e apposta: non appartiene alla
    ''' tavolozza del marchio — il verde di famiglia è <see cref="Successo"/> — ma è quello
    ''' della barra di avanzamento di Windows, campionato pixel per pixel dall'immagine che
    ''' Mirco ha portato il 2026-08-31. La ragione è che una barra che si riempie non è
    ''' decorazione ma un <b>segno che si legge senza impararlo</b>: chi la vede in mezzo
    ''' allo schermo sa già cosa vuol dire, perché l'ha vista mille volte di quel verde lì.
    ''' Dipingerla del blu Aviolab l'avrebbe resa una striscia da interpretare.</para>
    ''' <para>Sta comunque qui, come tutti gli altri: un colore scritto a mano dentro un
    ''' disegno è un colore che nessuno ritrova più.</para>
    ''' </remarks>
    Public ReadOnly VerdeDiAttesa As Color = ColorTranslator.FromHtml("#0D7C0D")

    ''' <summary>
    ''' Il verde chiaro sulla testa del riempimento: è lui a farlo sembrare acceso.
    ''' </summary>
    ''' <remarks>
    ''' Nell'immagine di Mirco sembra un effetto fluorescente diffuso; misurandola, è una
    ''' sfumatura che comincia un ottavo prima della punta e arriva fin qui. Non è un
    ''' vezzo: è la parte che si muove, e schiarirla è come si vede che si muove.
    ''' </remarks>
    Public ReadOnly VerdeInTesta As Color = ColorTranslator.FromHtml("#34A936")

    ''' <summary>Il verde in basso: la barra non è di un verde solo.</summary>
    ''' <remarks>
    ''' Anche in verticale c'è una sfumatura, appena percettibile: scura sopra, un filo
    ''' più chiara sull'ultimo terzo. È quel che dà spessore alla striscia invece di
    ''' lasciarla piatta, ed è misurata sull'immagine come le altre.
    ''' </remarks>
    Public ReadOnly VerdeSulFondo As Color = ColorTranslator.FromHtml("#378B35")

    ''' <summary>Il fondo grigio della barra: la parte che deve ancora riempirsi.</summary>
    Public ReadOnly FondoDiAttesa As Color = ColorTranslator.FromHtml("#E4E6E6")

    ''' <summary>Il filetto attorno alla barra, che le dà un contorno sullo schermo.</summary>
    Public ReadOnly BordoDiAttesa As Color = ColorTranslator.FromHtml("#CACBCC")

    ''' <summary>Azioni che modificano, badge attenzione.</summary>
    Public ReadOnly Avviso As Color = ColorTranslator.FromHtml("#FFC107")

    ''' <summary>Azioni distruttive, badge errore.</summary>
    Public ReadOnly Pericolo As Color = ColorTranslator.FromHtml("#DC3545")

    ''' <summary>Il fondo delle azioni critiche (livello 6) e l'inchiostro dei titoli di gruppo.</summary>
    ''' <remarks>
    ''' <para>Nasce il 2026-09-01 e toglie a <see cref="RossoTitoli"/> il mestiere che non
    ''' era suo. Il livello 6 (cap. 03.3) portava il rosso del <b>marchio</b>, e ne
    ''' venivano due guai insieme: il bianco sopra faceva 4,10 a 1, e soprattutto la
    ''' saturazione <i>smetteva</i> di crescere col peso — il rosso acceso dei titoli è più
    ''' chiaro del <see cref="Pericolo"/> di livello 5, così il gesto più grave si vestiva
    ''' del colore meno grave. Questo rosso è più scuro di tutti e due e il bianco sopra ci
    ''' fa <b>7,35</b>: la scala torna a salire, e il rosso del marchio resta ai titoli.</para>
    ''' <para>Lo stesso colore fa da <b>inchiostro</b> ai titoli di gruppo (9 punti), dove
    ''' vale da 6,70 sul fondo delle pagine a 7,35 sul bianco: all'occhio è ancora il rosso
    ''' di casa, e finalmente si legge. Un colore solo per le due parti, perché sono la
    ''' stessa cosa detta due volte — il rosso quando non deve gridare ma pesare.</para>
    ''' </remarks>
    Public ReadOnly RossoCritico As Color = ColorTranslator.FromHtml("#B00013")

    ''' <summary>Badge informativi.</summary>
    ''' <remarks>
    ''' È un fondo, non un inchiostro: su fondo chiaro fa <b>2,77 a 1</b>. A scrivere
    ''' l'informazione c'è <see cref="InformazioneTesto"/>.
    ''' </remarks>
    Public ReadOnly Informazione As Color = ColorTranslator.FromHtml("#17A2B8")

    ''' <summary>Il testo informativo: l'azzurro dei badge, portato al buio.</summary>
    ''' <remarks>
    ''' Nasce il 2026-09-01 perché <see cref="Informazione"/> faceva due mestieri e uno
    ''' non lo sapeva fare: dietro un badge è il fondo di un pannellino con sopra il
    ''' bianco, ma in Home era diventato il colore di <i>lettere</i> — il promemoria dei
    ''' solleciti e le righe «da sollecitare» della coda — e lì valeva <b>2,77</b> sul fondo
    ''' delle pagine e 2,93 dentro la coda, cioè quasi il doppio sotto la soglia. Questo ne
    ''' fa <b>6,03</b> e <b>6,36</b> sugli stessi due fondi. Sono due token e non uno per la
    ''' ragione di sempre in questa tabella: un colore che sta <i>sotto</i> e un colore che
    ''' sta <i>sopra</i> sono due ruoli, e chiedere a uno solo di fare tutti e due significa
    ''' sceglierne uno che va bene per nessuno dei due.
    ''' </remarks>
    Public ReadOnly InformazioneTesto As Color = ColorTranslator.FromHtml("#0F6674")

    ' --- Font (cap. 03.2) ---

    ''' <summary>Un solo font in tutta l'applicazione.</summary>
    Public Const NomeFont As String = "Segoe UI"

    ''' <summary>Font dei soli dati tecnici.</summary>
    Public Const NomeFontTecnico As String = "Consolas"

    ''' <summary>Titolo di finestra o di pannello (con RossoTitoli).</summary>
    Public ReadOnly FontTitoloFinestra As New Font(NomeFont, 16.0F, FontStyle.Bold)

    ''' <summary>Titolo di pannello più contenuto (con RossoTitoli).</summary>
    Public ReadOnly FontTitoloPannello As New Font(NomeFont, 14.0F, FontStyle.Bold)

    ''' <summary>Titolo di GroupBox (con <see cref="RossoCritico"/>: a 9 punti il rosso
    ''' del marchio non arriva alla soglia — v. <see cref="RossoTitoli"/>).</summary>
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
    ''' <remarks>
    ''' Dal 2026-09-01 sono 9 punti e non più 8, cioè il corpo del testo di lavoro. Otto
    ''' punti sono circa 10,7 pixel a 96 DPI, e sotto gli 11 nessun contrasto basta: la
    ''' didascalia porta il grigio più chiaro della tavolozza (<see cref="TestoSecondario"/>,
    ''' che sta appena sopra la soglia) e le due economie si sommavano proprio sul testo
    ''' che spiega perché un bottone è spento. Un punto in più costa qualche pixel di
    ''' altezza a ogni riga di aiuto; non leggerla costa l'aiuto intero.
    ''' </remarks>
    Public ReadOnly FontDidascalia As New Font(NomeFont, 9.0F)

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
    ''' <remarks>
    ''' <para>È il primo gradino della <b>scala delle misure</b> dei bottoni: dal
    ''' 2026-09-01, su indicazione del tutor, un bottone non prende più la larghezza che
    ''' la sua scritta chiede — la prende da questa scala, e la scritta ci sta dentro.
    ''' Prima c'erano ventiquattro larghezze diverse scritte a mano nei designer (40, 80,
    ''' 90, 100, 110, 120, 130, 134, 140, 150, 160, 162, 170, 176, 180, 190, 200, 210,
    ''' 220, 230, 240, 260, 280, 300): ognuna nata misurando il proprio testo, e nessuna
    ''' che dicesse niente a chi ne aggiungeva una nuova.</para>
    ''' <para>I gradini sono cinque più quello dell'icona, e ognuno è una <b>misura che
    ''' esisteva già</b> fra quelle ventiquattro — non un numero nuovo deciso adesso: si
    ''' sale al primo gradino che contiene la scritta, mai si scende, così nessuna
    ''' etichetta si accorcia passando alla scala.</para>
    ''' </remarks>
    Public ReadOnly BottoneStandard As New Size(110, 32)

    ''' <summary>Bottone standard, testo medio (secondo gradino).</summary>
    Public ReadOnly BottoneMedio As New Size(130, 32)

    ''' <summary>Terzo gradino: una frase breve — «Fallo riscrivere», «Salva la chiave».</summary>
    ''' <remarks>
    ''' È la larghezza già più diffusa nei designer prima della scala (dodici bottoni su
    ''' cinquanta la portavano), ed è per questo che il gradino cade qui e non altrove.
    ''' </remarks>
    Public ReadOnly BottoneLargo As New Size(190, 32)

    ''' <summary>Quarto gradino: una frase — «Documenti da allegare…».</summary>
    Public ReadOnly BottoneMoltoLargo As New Size(240, 32)

    ''' <summary>Ultimo gradino: una riga intera — «Come funziona, e cosa esce dal tuo PC».</summary>
    ''' <remarks>
    ''' Oltre questo non si sale: una scritta che non ci stesse dentro non vuole un
    ''' gradino nuovo, vuole essere più corta.
    ''' </remarks>
    Public ReadOnly BottoneMassimo As New Size(300, 32)

    ''' <summary>Il bottone che porta un segno solo e non una parola: «◀», «⟳».</summary>
    ''' <remarks>
    ''' Sta fuori dalla scala perché non è una scritta corta, è un'icona: metterlo al
    ''' primo gradino farebbe due quadrati da 110 nella barra del browser, dove accanto
    ''' c'è l'indirizzo e lo spazio è di quello.
    ''' </remarks>
    Public ReadOnly BottoneIcona As New Size(40, 32)

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

    ''' <summary>
    ''' Quanta parte della larghezza si prende la colonna dei comandi in una finestra
    ''' divisa in due (cap. 03.4): un terzo ai bottoni, due terzi ai testi.
    ''' </summary>
    ''' <remarks>
    ''' <para>Nasce il 2026-09-01, su indicazione del tutor, per le <b>Impostazioni</b>:
    ''' lì i bottoni stavano <i>sotto</i> il paragrafo che spiega a cosa servono, ognuno
    ''' largo quanto la propria scritta, e la colonna veniva sfrangiata — «button
    ''' disordinati e grossi quanto la scritta che c'è dentro». Adesso la finestra è divisa
    ''' in due: a sinistra i titoli e i testi, a destra i comandi, ciascuno all'altezza
    ''' della sua sezione.</para>
    ''' <para>È una <b>frazione</b> e non una misura in pixel apposta: la colonna dei
    ''' comandi è la larghezza unica di quei bottoni, e una frazione della finestra scala
    ''' col DPI insieme a lei — un numero fisso no, e a 150% darebbe una colonna stretta
    ''' dentro una finestra cresciuta.</para>
    ''' </remarks>
    Public Const FrazioneColonnaDeiComandi As Double = 1.0 / 3.0

    ''' <summary>Il bottone della barra che porta un segno solo: il «?» dell'aiuto.</summary>
    ''' <remarks>
    ''' Nasce il 2026-09-01 col bottone d'aiuto (cap. 03.4). È il gemello alto di
    ''' <see cref="BottoneIcona"/>: stessa larghezza, l'altezza della barra — che è una
    ''' riga sola e non ammette un bottone di misura diversa dagli altri.
    ''' </remarks>
    Public ReadOnly BottoneBarraSuperioreIcona As New Size(40, 34)

    ''' <summary>Quanto sono smussati gli angoli dei bottoni disegnati a mano, in pixel.</summary>
    ''' <remarks>
    ''' Appena smussati, non tondi: la pillola del menu di prima aveva raggio pari a metà
    ''' altezza, e su sei bottoni in colonna faceva una fila di losanghe. Otto pixel su
    ''' cinquantatré sono la differenza fra un rettangolo duro e un rettangolo gentile —
    ''' erano sei fino al 2026-09-01, e sei si vedevano appena su un bottone così largo.
    ''' <para>Sta qui e non dentro il disegno perché è una misura come le altre di questo
    ''' capitolo: se un domani nascesse un secondo controllo disegnato a mano, dovrebbe
    ''' avere gli stessi angoli senza che nessuno se lo ricordi.</para>
    ''' </remarks>
    Public Const RaggioAngolo As Single = 8.0F

    ' --- Come reagisce al mouse quel che si dipinge da sé (cap. 03.3) ---

    ''' <summary>Di quanto si scurisce un fondo quando il mouse ci passa sopra.</summary>
    ''' <remarks>
    ''' <b>Sul chiaro ci si accende scurendo</b>, non schiarendo: è la manopola che
    ''' <c>BottoneMenu</c> gira dal 2026-08-30, e resta sua soltanto. Per un giorno —
    ''' il 2026-09-01, mattina — l'hanno girata anche i bottoni vestiti da qui, tramite
    ''' <c>FlatAppearance</c>; il passaggio a <see cref="FlatStyle.Standard"/> dello stesso
    ''' giorno (su indicazione del tutor) ha tolto <c>FlatAppearance</c> di mezzo, e sotto
    ''' il puntatore quei bottoni fanno adesso quel che fa un bottone di Windows.
    ''' </remarks>
    Public Const ScurimentoSopra As Integer = 18

    ''' <summary>Di quanto si scurisce mentre è premuto.</summary>
    ''' <remarks>
    ''' Il doppio del passaggio, perché i due momenti dicono cose diverse — «il mouse è
    ''' qui» e «lo stai premendo» — e fra i due la differenza si deve vedere.
    ''' </remarks>
    Public Const ScurimentoPremuto As Integer = 36

    ''' <summary>Lo stesso colore spostato verso il nero, restando nei limiti.</summary>
    Public Function Scurito(colore As Color, di As Integer) As Color

        Return Color.FromArgb(colore.A,
                              Math.Clamp(colore.R - di, 0, 255),
                              Math.Clamp(colore.G - di, 0, 255),
                              Math.Clamp(colore.B - di, 0, 255))

    End Function

    ' --- I livelli di conseguenza (cap. 03.3) ---

    ''' <summary>
    ''' Veste un bottone secondo il peso della sua conseguenza. Esiste per la regola
    ''' più esigente del capitolo 03: <i>due bottoni con la stessa funzione, in due
    ''' pannelli diversi, sono identici</i>. Ripetuto a mano in ogni designer, quel
    ''' «identici» sarebbe durato fino al terzo pannello.
    ''' </summary>
    ''' <remarks>
    ''' <para><c>FlatStyle.Standard</c> e <c>UseVisualStyleBackColor = False</c> valgono per
    ''' tutti i livelli: senza il secondo, Windows ridipinge il fondo di suo e il colore
    ''' scelto qui non si vede.</para>
    ''' <para><b>Standard e non più Flat</b>, dal 2026-09-01 e su indicazione del tutor: il
    ''' bottone torna a essere un bottone di Windows, con il rilievo che lo dichiara
    ''' premibile prima di qualunque colore. Quel che i livelli dicono resta — il
    ''' <b>fondo</b>, l'<b>inchiostro</b> e il <b>carattere</b> — mentre sparisce tutto ciò
    ''' che passava da <c>FlatAppearance</c>, che <c>Standard</c> non guarda affatto: i
    ''' contorni disegnati (l'accento del livello 2) e il fondo che si scuriva sotto il
    ''' puntatore. Restano scritte qui le due manopole dello scurimento perché
    ''' <c>BottoneMenu</c>, che si dipinge da sé, continua a girarle.</para>
    ''' <para>Il bottone si ridipinge anche quando viene <b>spento</b>:
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

        bottone.FlatStyle = FlatStyle.Standard
        bottone.UseVisualStyleBackColor = False
        bottone.Font = FontTesto
        bottone.ForeColor = TestoPrimario

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

            Case LivelloBottone.Esplorativo
                bottone.BackColor = AccentoTenue
                ' Il fondo tenue di questo livello, su una finestra già chiara, si legge
                ' quasi come quello di un bottone spento — ed è successo davvero, con
                ' «Esporta PDF» e «Esporta DOCX» creduti morti mentre funzionavano (T9d,
                ' 2026-08-22). La ragione è che l'occhio giudica un bottone dal **testo**
                ' prima che dal contorno, e nero-su-azzurrino è esattamente ciò che
                ' nell'app significa spento (grigio su grigio): il fondo tenue non basta a
                ' smentirlo. Le lettere d'accento e il carattere dei bottoni che pesano
                ' sono i due segnali che glielo smentiscono, e nessuno dei due è tolto allo
                ' spento, che resta grigio in tutto.
                '
                ' Il terzo segnale era il contorno doppio d'accento, e dal 2026-09-01 non
                ' c'è più: FlatStyle.Standard il contorno se lo disegna da sé e
                ' FlatAppearance non lo guarda. Ne restano due su tre.
                bottone.ForeColor = Accento

            Case LivelloBottone.AzionePrincipale
                bottone.BackColor = FondoAzione

            Case LivelloBottone.Attenzione
                bottone.BackColor = Avviso

            Case LivelloBottone.Distruttivo
                bottone.BackColor = Pericolo
                bottone.ForeColor = SfondoContenuto

            Case Else ' Critico
                ' Il fondo è il rosso grave e non quello del marchio: la saturazione
                ' cresce col peso (03.3), e il rosso acceso dei titoli è più chiaro del
                ' Pericolo di livello 5 — il gesto più grave si vestiva del colore meno
                ' grave, e col bianco sopra faceva 4,10 a 1. Questo ne fa 7,35, e il rosso
                ' del marchio resta dov'è di casa: i titoli.
                bottone.BackColor = RossoCritico
                bottone.ForeColor = SfondoContenuto

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
    ''' <para><b>Il pannello aperto si vede dal fondo pieno.</b> Fra il 2026-08-30 e il
    ''' 2026-09-01 lo diceva la <b>cornice</b> — doppia e d'accento, con le lettere dello
    ''' stesso blu — e la cornice era disegnata da <c>FlatAppearance</c>. Col passaggio a
    ''' <see cref="FlatStyle.Standard"/> (su indicazione del tutor) quella cornice non
    ''' esiste più: <c>Standard</c> il contorno se lo disegna da sé e non lo lascia
    ''' scegliere. Restavano le sole lettere d'accento su fondo azzurro, che è la
    ''' differenza più debole della barra; il segnale è passato perciò al <b>fondo</b>, che
    ''' <c>Standard</c> rispetta: la casella aperta è l'unica <b>piena del blu d'accento,
    ''' con le lettere bianche</b>. Vale per tutte e sette, verde compreso — quando si sta
    ''' guardando il menu d'ingresso è la casella «🎮 Menu» a vestirsi così, e il verde
    ''' torna appena si va altrove: dove si è pesa più del ruolo.</para>
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

        bottone.FlatStyle = FlatStyle.Standard
        bottone.UseVisualStyleBackColor = False

        ' Il carattere è quello dei bottoni che pesano, e vale per tutte: la fila in cima è
        ' l'indice del programma, e i suoi sette nomi si leggono di sfuggita mentre si sta
        ' facendo altro — come le voci del menu d'ingresso, che sono in grassetto per la
        ' stessa ragione. Non dipende né dal ruolo né dall'essere aperta o accesa: una
        ' casella che cambia carattere cambia ingombro, e la fila si muoverebbe sotto gli
        ' occhi a ogni clic e a ogni chiamata all'AI.
        bottone.Font = FontBottoneForte

        If Not bottone.Enabled Then
            bottone.BackColor = SfondoBase
            bottone.ForeColor = TestoSecondario
            Return
        End If

        ' Dove si è, prima di che parte fa: la casella aperta è l'unica piena del blu
        ' d'accento, e le lettere bianche sono le sole leggibili su quel blu.
        If attiva Then
            bottone.BackColor = Accento
            bottone.ForeColor = SfondoContenuto
            Return
        End If

        If ruolo = RuoloBarra.RitornoAlMenu Then
            bottone.BackColor = Successo
            bottone.ForeColor = SfondoContenuto
        Else
            bottone.BackColor = FondoAzione
            bottone.ForeColor = TestoPrimario
        End If

    End Sub

End Module

