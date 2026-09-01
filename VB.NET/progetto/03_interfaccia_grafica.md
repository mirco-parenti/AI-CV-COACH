# 03 — Interfaccia grafica

*Il design dell'applicazione: principi, colori, font, struttura delle finestre e dei
pannelli. È il sistema di design **proprio** di AI-CV-COACH, pensato per dare lo stesso
«family feeling» delle applicazioni di casa Aviolab. Nota di riservatezza: qui non si
descrive nessun software aziendale; si specifica, in autonomo, l'aspetto che questa
applicazione deve avere.*

## 3.1 Principi

1. **L'interfaccia guida, non decora.** L'utente di riferimento non è un tecnico: davanti
   a ogni bottone si chiede «cosa mi succede se lo premo?». L'interfaccia deve
   rispondere da sola, con il colore e la posizione, prima ancora del testo.
2. **Il colore di un bottone dice la conseguenza dell'azione**, non il marchio: un'azione
   sicura è verde, una esplorativa è tenue, una distruttiva è rossa. Il colore del brand
   vive nei **titoli grandi** e negli accenti — e dal 2026-09-01 in **nessun** bottone,
   nemmeno in quello critico, che ha il suo rosso grave (3.3).
3. **Flat totale**: niente gradienti, ombre, bordi 3D. Bordi sottili da 1 px, superfici
   piatte, `FlatStyle.Flat`.
4. **Un solo font**: Segoe UI ovunque (Consolas solo per dati tecnici). Mai font di
   sistema vecchio stile.
5. **Coerenza assoluta**: due bottoni con la stessa funzione, in due pannelli diversi,
   sono identici. Nel dubbio sul «peso» di un'azione, si sale di livello (più cautela).
6. **Pannelli statici**: ogni schermata è disegnata nel designer di Visual Studio,
   con controlli dichiarati staticamente. A runtime cambiano solo i contenuti
   (testi, liste, anteprime, visibilità), mai la struttura.
   **Unica eccezione, dichiarata: le bolle della conversazione** (P5). Una chat non ha un
   numero di righe noto in anticipo, e nessuna lista di sistema sa mostrare bolle di
   larghezza variabile allineate a destra e a sinistra: lì i controlli nascono a runtime
   dentro il flusso. Tutto il resto del pannello — casella, «Invia», i **tre bottoni di
   scelta**, la fascia delle azioni — è dichiarato nel designer, e le scelte del turno si
   limitano a cambiare etichetta, colore e visibilità di quei tre. *(Deciso in T3c,
   2026-08-07.)*

## 3.2 Token di design

Tutti i colori e i font dell'applicazione vengono **da questa tabella e solo da questa**
(nel codice: un modulo `StileApp` con le costanti; vietato `Color.FromArgb` sparso nei form).

### Colori

| Token | Hex | Uso |
|---|---|---|
| `TestoPrimario` | `#212529` | testo normale, valori, titoli di sezione |
| `TestoSecondario` | `#68717A` | didascalie, suggerimenti, stati *(era `#6C757D` fino al 2026-08-30: su `SfondoBase` faceva 4,45 : 1, un centesimo sotto la soglia WCAG di 4,5, ed è la coppia di **ogni** didascalia. Dal 2026-08-31 scende di altri due punti, perché il fondo delle pagine si è scaldato e col grigio di ieri faceva 4,39: ora fa **4,52** su `FondoPagina`, 4,71 su `SfondoBase`, 4,77 su `FondoCasella` e 4,88 su `SfondoContenuto`)* |
| `SfondoBase` | `#F8F9FA` | sfondo delle finestre |
| `SfondoContenuto` | `#FFFFFF` | aree di lavoro (testi, anteprime, input) |
| `FondoPagina` | `#F8F4EB` | fondo delle **pagine** che si aprono dal menu *(dal 2026-08-31: è `FondoMenu` con sopra lo stesso velo che porta `SfondoBase` sotto il bianco — sette punti di rosso, sei di verde, cinque di blu — e lo stacco resta quello di prima, 0,053 di luminanza contro 0,054)* |
| `FondoCasella` | `#FFFAF0` | le aree di lavoro **dentro** quelle pagine — caselle, elenchi, schede — e il fondo della finestra **Impostazioni** *(dal 2026-08-31: è `FondoMenu` preso di peso, e sta a parte per la ragione per cui `FondoMarchio` sta accanto ad `Accento` — sono due ruoli)* |
| `BordoLeggero` | `#DEE2E6` | separatori e bordi da 1 px |
| `BordoForte` | `#CED4DA` | bordo dei controlli interattivi |
| `FondoMenu` | `#FFFAF0` | fondo del menu d'ingresso (cap. 03.6) *(dal 2026-08-30 sera: un avorio caldo, al posto del banner che del menu era lo sfondo intero)* |
| `Accento` | `#0B06B0` | focus, link, selezione (blu profondo) |
| `AccentoTenue` | `#E4E7FB` | riga selezionata, hover |
| `FondoAzione` | `#C0E8FF` | fondo del bottone d'azione principale del pannello |
| `RossoTitoli` | `#FA0825` | titoli **grandi** delle finestre e dei pannelli (14 e 16 pt), marker *(dal 2026-09-01 non è più l'inchiostro dei titoli di GroupBox: a 9 punti fa 3,73 : 1 sul fondo delle pagine e 4,10 sul bianco, sotto il 4,5 che WCAG chiede al testo piccolo, e quelli passano a `RossoCritico`. Sui titoli grandi la soglia è 3, e lì il rosso del marchio la supera)* |
| `Successo` | `#1E7E34` | azioni sicure/positive, badge OK *(dal 2026-09-01: il `#28A745` da cui viene reggeva il bianco sopra a **3,13 : 1** — ed è il fondo del livello 1 e della casella «🎮 Menu» della barra, cioè dove il token si vede più spesso — mentre come **inchiostro**, l'esito «Assunto 🎉» di P4, faceva 2,85 sui fondi chiari. Adesso fa **5,14** sotto il bianco, e da 4,68 a 5,14 come testo. Resta un verde solo per i due usi: un colore che fa da fondo e da lettere deve reggere nei due versi, e sdoppiarlo metterebbe due verdi di famiglia nella stessa schermata)* |
| `ArgentoDiAttesa` | `#E2E8F0` | i pallini della ruota dell'attesa (cap. 03.8) *(dal 2026-08-30)* |
| `OmbraDiAttesa` | `#4A5568` | il contorno di quei pallini *(dal 2026-08-30; fino al 2026-09-01 serviva a staccarli dalle parti chiare dello stemma, che non c'è più — resta perché un pallino d'argento vuole comunque un profilo)* |
| `VerdeDiAttesa` | `#0D7C0D` | il corpo della barra che si riempie sotto la ruota (cap. 03.8) *(dal 2026-08-31)* |
| `VerdeSulFondo` | `#378B35` | il fondo di quella barra, dove la sfumatura verticale schiarisce *(dal 2026-08-31)* |
| `VerdeInTesta` | `#34A936` | la punta accesa del riempimento, che è la parte che si vede muoversi *(dal 2026-08-31)* |
| `FondoDiAttesa` | `#E4E6E6` | il grigio della barra ancora da riempire *(dal 2026-08-31)* |
| `BordoDiAttesa` | `#CACBCC` | il filetto attorno alla barra, che la stacca dal desktop *(dal 2026-08-31)* |
| `Avviso` | `#FFC107` | azioni che modificano, badge attenzione |
| `Pericolo` | `#DC3545` | azioni distruttive, badge errore |
| `RossoCritico` | `#B00013` | fondo delle azioni **critiche** (livello 6) e inchiostro dei **titoli di GroupBox** (9 pt) *(dal 2026-09-01: toglie a `RossoTitoli` due mestieri che non erano suoi. Come fondo del livello 6 il rosso del marchio dava 4,10 : 1 col bianco sopra, ed era più chiaro del `Pericolo` di livello 5 — il gesto più grave si vestiva del colore meno grave; qui il bianco fa **7,35** e la scala torna a salire col peso. Come inchiostro a 9 punti vale da 6,70 sul fondo delle pagine a 7,35 sul bianco: all'occhio è ancora il rosso di casa, e si legge)* |
| `Informazione` | `#17A2B8` | badge informativi, e **come fondo** soltanto (sopra ci va il bianco): da inchiostro su fondo chiaro fa 2,77 : 1 |
| `InformazioneTesto` | `#0F6674` | le **lettere** informative: il promemoria dei solleciti e le righe «da sollecitare» della coda in Home *(dal 2026-09-01: `Informazione` faceva due mestieri e uno non lo sapeva fare — da inchiostro valeva 2,77 sul fondo delle pagine e 2,93 dentro la coda, quasi il doppio sotto la soglia. Questo ne fa **6,03** e **6,36**. Sono due token e non uno per la ragione di sempre in questa tabella: un colore che sta sotto e un colore che sta sopra sono due ruoli)* |

**Due famiglie di fondi, e non è un doppione** *(dal 2026-08-31)*. `SfondoBase`/`SfondoContenuto`
— il grigino e il bianco — restano i fondi delle **finestre**: impostazioni, backup, conferme,
la barra in cima, i bottoni spenti. `FondoPagina`/`FondoCasella` sono i loro gemelli caldi e
valgono nelle **pagine**, cioè i pannelli che si aprono dal menu. La ragione è che l'avorio è
la soglia: l'applicazione si apriva su un menu caldo e mandava, premendo un bottone, dentro
sei stanze grigie. Adesso l'avorio entra con l'utente e resta finché lavora, mentre le finestre
— che sono momenti, non stanze — conservano il fondo di sempre. L'eccezione, dichiarata,
sono le **Impostazioni**: quella è la settima porta del menu, e si apre in una finestra
solo per come è fatta dentro; sarebbe stata l'unica destinazione a restare bianca, e prende
`FondoCasella` come fondo pieno (non ha la coppia, perché è tutta una superficie sola). Il velo fra i due fondi di
ciascuna coppia è **lo stesso**, così la geografia di una schermata non cambia: si sposta la
temperatura, non il disegno. Fanno eccezione, per scelta, la barra superiore, la fascia
inferiore e il pannello del logo, che stanno **sopra** ogni pagina e sono le stesse in tutte,
menu compreso.

### Font

| Ruolo | Font |
|---|---|
| Titolo finestra/pannello | Segoe UI 14–16 **Bold**, colore `RossoTitoli` |
| Titolo GroupBox | Segoe UI 9 **Bold**, colore `RossoCritico` *(dal 2026-09-01: a 9 punti il rosso del marchio non arriva alla soglia — v. la tabella dei colori)* |
| Bottone d'azione principale | Segoe UI 9.75 **Bold** |
| Testo di lavoro e bottoni neutri | Segoe UI 9 |
| Didascalie / hint | Segoe UI **9**, colore `TestoSecondario` *(dal 2026-09-01: erano 8, cioè circa 10,7 pixel a 96 DPI, e sotto gli 11 nessun contrasto basta. La didascalia porta il grigio più chiaro della tavolozza, che sta appena sopra la soglia, e le due economie si sommavano proprio sul testo che spiega perché un bottone è spento: un punto in più costa qualche pixel per riga, non leggerla costa l'aiuto intero)* |
| Dati tecnici (punteggi, log) | Consolas 8.5 |

*Confermato nella stessa revisione: i titoli restano in **Bold**. A 14 e 16 punti il
grassetto non serve a farli leggere — a quel corpo si leggono comunque — ma a dire che sono
titoli: qui dentro il peso è il segno della gerarchia, e toglierlo lascerebbe la gerarchia
al solo colore, che è esattamente quel che il resto di questa revisione ha smesso di fare.*

### Spaziature e dimensioni (regola 14 / 12 / 8)

- **14 px** di margine interno nei GroupBox e nei riquadri;
- **12 px** di distanza tra controlli affiancati;
- **8 px** minimo tra le righe (14–16 dove serve respiro).
- **La scala delle misure dei bottoni** *(dal 2026-09-01, su indicazione del tutor)*: un
  bottone non prende più la larghezza che la sua scritta chiede, la prende da questa scala,
  e sale al primo gradino che contiene la scritta — mai scende. I gradini sono
  **110×32** (`BottoneStandard`, una parola), **130×32** (`BottoneMedio`, due),
  **190×32** (`BottoneLargo`, una frase breve), **240×32** (`BottoneMoltoLargo`, una frase)
  e **300×32** (`BottoneMassimo`, una riga intera); fuori scala sta **40×32**
  (`BottoneIcona`), per il bottone che porta un segno solo e non una parola. La barra
  superiore ha i suoi due, **110×34** e **210×34**, più **40×34**
  (`BottoneBarraSuperioreIcona`) per il «?» dell'aiuto che le sta in coda (3.4), perché quella
  fila è alta un pixel di più e deve restare una riga sola. *Prima di quel giorno le larghezze scritte a mano nei
  designer erano **ventiquattro**, ognuna misurata sul proprio testo: nessuna sbagliata da
  sola, e nessuna che dicesse qualcosa a chi ne aggiungeva una nuova. I cinque gradini non
  sono numeri nuovi — sono cinque di quelle ventiquattro, scelte fra le più usate.*
  **Oltre l'ultimo gradino non si sale**: una scritta che non ci stesse dentro non vuole un
  gradino nuovo, vuole essere più corta.
- **8 px** di raggio agli angoli dei bottoni **disegnati a mano** (`RaggioAngolo`): appena
  smussati, non tondi. Sta fra le misure di questo capitolo e non dentro il disegno del
  menu per la ragione di sempre — se un domani nascesse un secondo controllo disegnato a
  mano dovrebbe avere gli stessi angoli senza che nessuno se lo ricordi. *(Erano 6 fino al
  2026-09-01, e su un bottone largo mezzo pannello si vedevano appena.)*

**Una finestra divisa in due: due terzi ai testi, un terzo ai comandi** *(dal 2026-09-01, su
indicazione del tutor)*. `FrazioneColonnaDeiComandi` vale **un terzo**, ed è la larghezza
unica dei bottoni di sezione delle Impostazioni (3.4). È una frazione e non una misura in
pixel apposta: la colonna scala col DPI insieme alla finestra, mentre un numero fisso
darebbe, a 150%, una colonna stretta dentro una finestra cresciuta.

**I bottoni sono bottoni di Windows** *(dal 2026-09-01, su indicazione del tutor)*. Ogni
bottone dell'applicazione è `FlatStyle.Standard`, non più `Flat`: torna il rilievo di
sistema, che dichiara «questo si preme» prima di qualunque colore. Dei livelli di 3.3
restano il **fondo**, l'**inchiostro** e il **carattere**; sparisce tutto ciò che passava da
`FlatAppearance`, che `Standard` non guarda affatto — i contorni scelti a mano e il fondo che
si scuriva sotto il puntatore. La conseguenza va detta e non nascosta: il livello 2
(esplorativo) perde il suo contorno doppio d'accento e resta con due segnali su tre (lettere
d'accento e carattere forte), e la barra superiore perde la cornice che diceva quale pannello
fosse aperto — v. 3.4, dove quel segnale è passato al fondo.

**Come reagisce al mouse quel che si dipinge da sé**. Il bottone del menu d'ingresso — che è
disegnato a mano e non da Windows — si **scurisce** di 18 al passaggio del puntatore
(`ScurimentoSopra`) e di 36 mentre è premuto (`ScurimentoPremuto`): il doppio, perché «il
mouse è qui» e «lo stai premendo» sono due cose diverse e fra le due la differenza si deve
vedere. Sul chiaro ci si accende scurendo, non schiarendo. *Per un giorno — il 2026-09-01,
mattina — la stessa manopola l'hanno girata anche i bottoni di sistema, tramite
`FlatAppearance`; il passaggio a `Standard` dello stesso giorno l'ha resa inerte per loro, e
sotto il puntatore fanno adesso quel che fa un bottone di Windows.*

*Le misure di questa sezione sono state **riguardate e confermate** nella revisione di
finalizzazione del 2026-09-01, e la conferma vale quanto un cambiamento: i bottoni restano
alti **32 px** — è la misura standard di un desktop, mentre i 44 px che le linee guida
d'accessibilità raccomandano sono la regola del **dito** su uno schermo che si tocca, non
del puntatore su uno schermo che si guarda; la griglia resta
**14 / 12 / 8**, che è già una progressione coerente e non chiede di diventare 16/12/8; e le
caselle di testo restano alte **23–24 px**, l'altezza che Windows Forms dà loro col font di
questo capitolo.*

## 3.3 I livelli di conseguenza dei bottoni

Ogni bottone dell'applicazione appartiene a **uno** di questi livelli. La saturazione
del colore cresce con il peso della conseguenza:

| Livello | Quando | Aspetto |
|---|---|---|
| **0 — Neutro** | navigazione, annulla, chiudi | bianco, bordo `BordoLeggero`, Segoe UI 9 |
| **1 — Sicuro positivo** | conferme senza rischio («Salva profilo», «Salva questa ricerca», «L'ho spedita») | fondo `Successo`, testo bianco, bold *(«Cattura annuncio» stava fra gli esempi di questa riga fino al 2026-09-01: è passata al livello 3, v. sotto)* |
| **2 — Esplorativo leggero** | aprire, sfogliare, vedere anteprime | fondo `AccentoTenue`, **testo `Accento`** e carattere grassetto. *Il bordo era `BordoForte` fino al 2026-08-22: su una finestra già chiara quel fondo tenue col contorno grigio si legge come un bottone **spento**, e nella prova dal vivo di T9d «Esporta PDF» e «Esporta DOCX» sono stati creduti morti mentre funzionavano. Il bordo d'accento è la differenza più piccola che si veda a colpo d'occhio, e imparenta il livello 2 col 3 — che d'accento ha il bordo e in più il fondo azzurro e il carattere grande.* *Il bordo da solo non è bastato: alla **seconda** prova dal vivo dello stesso giorno gli stessi due bottoni erano ancora letti come spenti, e la ragione è che l'occhio giudica un bottone dal **testo** prima che dal contorno — nero su azzurrino è esattamente ciò che qui dentro significa spento (grigio su grigio), e un fondo tenue non basta a smentirlo. Dal 2026-08-22 le lettere sono d'accento, il contorno è doppio e il carattere è quello dei bottoni che pesano: **tre segnali invece di uno**, e nessuno tolto allo spento, che resta grigio in tutto. Vale per ogni livello 2 dell'applicazione — Home, Profilo, Email, Dialogo, Opportunità, Documenti e le tre finestre — perché è un sistema e non una riparazione locale* *Dal **2026-09-01**, col passaggio a `FlatStyle.Standard` (3.2), i segnali tornano **due**: il contorno doppio d'accento non si può più scegliere — `Standard` il bordo se lo disegna da sé — e restano le lettere d'accento e il carattere forte, che sono i due che l'occhio legge per primi. La conseguenza va tenuta d'occhio a video: è proprio su questo livello che il difetto di T9d era comparso due volte.* |
| **3 — Azione principale del pannello** | il bottone «avanti» del flusso («Genera CV», «Confronta») | fondo `FondoAzione`, Segoe UI 9.75 Bold *(il bordo `Accento` è caduto il 2026-09-01 con il passaggio a `FlatStyle.Standard`, v. 3.2)* |
| **4 — Attenzione** | modifica dati esistenti («Sovrascrivi profilo», «Rigenera») | fondo `Avviso`, testo scuro, bold |
| **5 — Distruttivo** | eliminare un'opportunità, scartare | fondo `Pericolo`, testo bianco, bold |
| **6 — Critico** | inviare un'email, cancellazioni definitive | fondo `RossoCritico`, testo bianco, bold — sempre preceduto da conferma |

Regole: `FlatStyle.Standard`, `UseVisualStyleBackColor = False` ovunque *(era `Flat` fino al
2026-09-01, v. 3.2)*; il rosso del **brand** non veste nessun bottone (è dei titoli, 3.2);
nel dubbio tra due livelli si sceglie il più alto. E la **larghezza** non viene dalla
scritta ma dalla scala delle misure (3.2).

**La saturazione torna a crescere col peso** *(2026-09-01)*. Fino a qui il livello 6 portava
`RossoTitoli`, cioè il rosso **del marchio**, e ne venivano due guai insieme: il bianco
sopra faceva 4,10 : 1, e soprattutto quel rosso acceso è più **chiaro** del `Pericolo` di
livello 5 — il gesto più grave si vestiva del colore meno grave, e la scala di questa
tabella si rompeva proprio all'ultimo gradino. Adesso il fondo è `RossoCritico`, più scuro
di tutti e due; il rosso del marchio resta dov'è di casa, nei titoli.

**Il livello 3 si conta per fascia, non per pannello** *(precisato 2026-09-01)*. «Azione
principale del pannello» va letta come «azione principale della **sua fascia**»: P3 ne ha
due, e non è un'eccezione — la fascia della ricerca ha «Cerca», la fascia dei comandi in
fondo ha «Cattura annuncio», e ognuna delle due è il gesto avanti di quel gruppo. Un
pannello con due fasce che fanno due mestieri diversi ha due azioni principali; due bottoni
di livello 3 **nella stessa fascia** restano invece l'errore che questa riga vieta.

**Correzioni di assegnazione della stessa revisione**, ognuna col suo perché:
«Ripristina» della finestra di backup è **livello 4** e non 5 — sovrascrive il profilo di
adesso con quello del backup, cioè modifica, non cancella; «Fallo riscrivere» di P7 è
**livello 4**, e chiede conferma solo quando in casella c'è del lavoro dell'utente (quando
il testo è tutto dell'AI il costo di un sì sarebbe un'attesa, non del lavoro perso);
«Fai rileggere la cartella» della finestra documenti è **livello 2**, perché rilegge e non
tocca niente; «Cattura annuncio» di P3 è **livello 3** (v. sopra) e «Importa CV da questa
pagina», che le sta accanto, resta **livello 1**.

**Le conferme di livello 5 passano tutte dalla `FinestraConferma`** *(2026-09-01)*, quella
col verbo «Confermo» descritta in 3.4: l'eliminazione di una voce del profilo in P2, il
«Dimentica» di una ricerca salvata in P3 e lo «Scarta» di P4 chiedevano ancora con una
`MessageBox`, cioè con un Sì che risponde alla domanda invece che alla conseguenza. Un
livello ha una sola forma di conferma, o non è un livello.

**E un comando di livello 6 sta su una riga tutta sua anche fuori dai pannelli**
*(2026-09-01)*. La regola della fascia dei comandi (3.4) valeva solo lì: nelle Impostazioni
«ELIMINA TUTTI I DATI» stava nella stessa colonna, alla stessa larghezza e appena sotto
«Svuota i dati di navigazione», con il solo vuoto sopra a difenderlo. Adesso ha il vuoto da
tutti e due i lati, e la difesa è **verticale**: riga sua, stacco doppio sopra e sotto,
stessa larghezza e stessa colonna degli altri comandi.

*Nella stessa giornata la forma è stata provata in un altro modo e corretta, ed è una
lezione sul travaso delle regole. La prima cura lo spostava al **margine opposto**, com'è in
fascia dei comandi; ma là la fascia è una banda **orizzontale larga**, e il salto da una
parte all'altra si legge come una scelta. Nella colonna stretta delle Impostazioni lo stesso
salto si è letto come un avanzo — «ELIMINA TUTTI I DATI che galleggia a destra in mezzo al
vuoto», dalla fotografia del tutor. La difesa non è il salto: è il **vuoto**. Dove la banda è
larga il vuoto si può prendere di lato, dove è stretta si prende sopra e sotto.*

## 3.4 Architettura delle finestre

Una **finestra principale** (`FormPrincipale`) più finestre secondarie di servizio.
Niente barra dei menu classica: la navigazione sta in una **barra superiore di bottoni
con icona** (FontAwesome.Sharp); i menu contestuali (tasto destro) usano voci con emoji
(`✏️ Rinomina`, `🗑️ Elimina`, `📤 Esporta…`).

```
┌────────────────────────────────────────────────────────────────────┐
│ BARRA SUPERIORE   [🎮 Menu] [🏠 Le mie candidature] [👤 Profilo]   │
│      [🔍 Ricerca] [Confronta ★ ANNUNCIO - CV] [▤ Documenti]       │
│      [⚙ Impostazioni]                            stato AI …        │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│                    AREA CENTRALE                                   │
│   (un pannello per funzione, uno solo visibile per volta:          │
│    P0 Menu · P1 Home · P2 Profilo · P3 Ricerca · P4 Opportunità ·  │
│    P5 Dialogo/Brainstorm · P6 Documenti · P7 Email)                │
│                                                                    │
├──────────────┬─────────────────────────────────────────────────────┤
│  ┌────────┐  │  BARRA DI STATO   solo mentre l'AI lavora, o se     │
│  │ LOGO   │  │                   all'avvio c'è un avviso           │
│  │ AI-CV- │  │                                                     │
│  │ COACH  │  │                                                     │
│  └────────┘  │                                                     │
└──────────────┴─────────────────────────────────────────────────────┘
```

*La voce **«▤ Documenti» è di T9d** (2026-08-22). Fino ad allora P6 non era una
destinazione ma il passo successivo di un flusso — ci si arrivava dalla candidatura o dal
profilo — e per rileggere un CV scritto il giorno prima bisognava ripassare dalla Home e
riaprire la candidatura giusta col doppio clic. Adesso la voce porta a P6 «da fermo» e
dentro il pannello una tendina **«Documento:»** elenca il 📄 CV-1 base e le candidature che
hanno davvero un documento scritto, così da lì si raggiunge qualunque testo senza rifare la
strada. Due conseguenze volute: **entrare nei documenti non chiama mai l'AI** — senza niente
da mostrare il pannello resta vuoto e lo spiega, perché una navigazione che fa partire una
generazione è una spesa non chiesta — e in barra, quando si guardano i documenti, si accende
**sempre** «▤ Documenti», da qualunque strada si sia arrivati, mentre il «Torna indietro»
riporta dove si era (candidatura, profilo o Home).*

**I due segni della barra** *(dal 2026-08-31)*. La stella del confronto è **★** (U+2605)
e il foglio dei documenti **▤** (U+25A4): prima erano ⭐ e 📄, e a cambiarli è stato come
Windows li disegna. I bottoni della barra passano da GDI, che le emoji a colori non le sa
fare: ogni simbolo finisce al font di ripiego, dove i glifi non hanno la stessa statura né
lo stesso peso. Misurati a 9 punti in grassetto, la casa fa 11 pixel d'altezza, la lente e
l'ingranaggio 10, il busto 9 — quanto una maiuscola — e ⭐ ne faceva **6**, cioè l'altezza
di una «x»: accanto ad «ANNUNCIO» tutto maiuscolo non sembrava un'icona, sembrava un
refuso. ★ ne fa 8 ed è dentro Segoe UI, quindi prende anche il grassetto vero del bottone;
in più è la stessa stella con cui la colonna «Match» scrive il punteggio, e quel bottone
promette proprio quello. Il foglio invece era già alto (10 pixel): il suo difetto era il
**tratto**, un contorno sottile in mezzo a glifi pieni, e ▤ è pieno. A sorvegliare la
statura c'è un collaudo — nessun simbolo della barra può stare tutto dentro l'altezza di
una «x» — mentre la pienezza resta cosa da guardare: a dieci pixel l'antialias riempie il
contorno quanto il pieno, e misurarla direbbe il falso. *Nel menu (3.6) la stellina resta
⭐: là il bottone è disegnato a mano, con un corpo molto più grande, e il difetto non c'è.*

**I colori della barra** *(dal 2026-08-30 sera)*. La barra non è una fila di comandi
qualunque: è l'**indice** dell'applicazione, e le sue caselle sono le stesse sei voci del
menu d'ingresso (3.6) più la porta di casa. Prende perciò i colori di quel menu, invece
delle sette caselle bianche di prima: **azzurro `FondoAzione`** le sei destinazioni, con
la cornice `BordoForte` e le lettere scure — lo stesso vestito dei bottoni di P0 —
e **verde `Successo`** la casella «🎮 Menu», col testo bianco. Chi passa
dal menu alla barra ritrova le stesse cose, e la casella che riporta indietro si distingue
dalle sei che portano avanti **prima** di leggerne l'etichetta. *Fino al 2026-09-01 la
casella verde portava anche un contorno suo, `BordoSuccesso`; con il passaggio a
`FlatStyle.Standard` (3.2) i contorni scelti a mano non si disegnano più, e quel token è
uscito dalla tavolozza.*

**Dove si è lo dice il fondo pieno** *(dal 2026-09-01, su indicazione del tutor)*. La
casella del pannello aperto è l'unica **piena del blu d'accento, con le lettere bianche**,
e vale per tutte e sette: quando si sta guardando il menu d'ingresso è la casella «🎮 Menu»
a vestirsi così, e il verde torna appena si va altrove — dove si è pesa più del ruolo. Il
segnale ha cambiato natura due volte in due giorni, e vale la pena dire perché. Finché la
barra era bianca il pannello aperto si riconosceva dal **fondo lilla** (`AccentoTenue`);
passato il riposo all'azzurro quel lilla era quasi lo stesso colore, e l'evidenza andò alla
**cornice** — doppia e d'accento, con le lettere dello stesso blu. Quella cornice però la
disegnava `FlatAppearance`, e da `Standard` non esiste più: restavano le sole lettere
d'accento su fondo azzurro, cioè la differenza più debole della barra affidata all'unica
cosa che dovesse dire «sei qui». Il fondo è un mezzo che `Standard` rispetta, e il blu
pieno lo dice da lontano.

*Che le sette caselle abbiano tutte un **fondo pieno** è stato riguardato il 2026-09-01 e
confermato: una fila di sette colori accesi sembra molto, ma i colori sono **due** — azzurro
le destinazioni, verde il ritorno — e dicono una cosa sola, «di che specie è questa casella».
A distinguere quella aperta c'è adesso un **terzo** colore, il blu d'accento, che è più scuro
di tutti e due e non si confonde con nessuno. Smorzare il fondo delle altre per farla
risaltare rifarebbe la fila bianca da cui questa scelta è nata.*

E tutte e sette scrivono in **grassetto** (`FontBottoneForte`), come le voci del menu
d'ingresso e per la stessa ragione: sono l'indice del programma, nomi che si leggono di
sfuggita mentre si sta facendo altro, non comandi che si vanno a cercare. Il carattere non
cambia mai — né aprendo un pannello né quando la barra si spegne — perché cambiarlo cambia
l'ingombro, e una fila che si muove sotto gli occhi a ogni clic è peggio del segnale che si
sarebbe guadagnato.

Il resto vale come per ogni bottone colorato dell'applicazione: **spento si smorza**
(3.3), perché mentre l'AI lavora la barra si spegne tutta (cap. 02.6) e sette caselle
colorate che restano colorate direbbero che si può ancora andare via. Ruolo e stato di
ogni casella stanno nel suo `Tag`, così quando la barra si riapre ognuna ritrova il colore
che le spetta. E il carattere non dipende dall'essere accesa ma solo dall'essere aperta:
una casella che si spegne non deve cambiare ingombro, o la fila si muoverebbe ogni volta
che parte una chiamata.

- La macro-struttura è un `TableLayoutPanel` (righe: barra superiore fissa, area
  centrale elastica, fascia inferiore **alta zero finché non serve**, v. sotto); dentro ogni banda, `Panel` a coordinate
  fisse. I pannelli P1–P7 sono **UserControl disegnati nel designer**, impilati
  nell'area centrale e mostrati uno alla volta: struttura statica, nessun controllo
  creato a runtime.
- **La fascia di stato c'è solo quando parla** *(dal 2026-08-30)*. A riposo non dice più
  «Pronto»: sparisce, e con lei si azzera la riga della tabella che la ospita — nasconderla
  soltanto lascerebbe il buco, alto uguale e dello stesso colore chiaro, cioè la striscia
  che si voleva togliere. Torna quando ha qualcosa da dire: mentre l'AI lavora
  («L'AI sta lavorando…», in `Accento`, col conto dei secondi dopo i primi dieci) e
  all'avvio quando c'è un avviso — una cartella dati diversa da quella di sempre, un
  argomento della riga di comando non rispettato, o il lucchetto in mano a qualcun altro.
  È lo stesso principio della riga dei solleciti in Home (cap. 07.3): un avviso che occupa
  spazio anche da spento insegna a non guardarlo, e quando poi ha davvero qualcosa da dire
  non lo si vede più. Conseguenza da tenere presente: **l'ingombro che il pannello del logo
  dichiara ai pannelli cambia con lei** (cap. 03.5), perché sparita la fascia il logo
  sfonda nell'area centrale di tutta la propria altezza.
- **La barra ha cinque bottoni, non quattro** *(deciso in T4c, 2026-08-10)*: fra Ricerca
  e Impostazioni c'è il bottone che porta a P4 — **📋 Candidatura** quando è nato,
  **«Confronta ★ ANNUNCIO - CV»** dal 2026-08-30, che è la stessa porta chiamata col
  nome di quel che ne esce invece che con quello del pannello. Nel disegno originale a P4 si
  arrivava dalla coda delle opportunità in Home, ma la Home è di T5c e l'incolla-testo è
  già di T4: senza una porta propria, il pannello che questa tappa costruisce non
  sarebbe raggiungibile. Resta anche dopo T5 — un annuncio arriva spesso da un'email,
  senza passare dal registro (cap. 12.3).
- **Non ogni pannello ha un bottone in barra.** P5 (dialogo), P6 (documenti) e P7 (email)
  sono **passi di un flusso**, non destinazioni: ci si arriva dal pannello che li
  precede, e il bottone della barra resta acceso su quello — P5 e il 📄 CV-1 base sotto
  «Profilo», P6 di una candidatura sotto «Candidatura». Il bottone «torna indietro» di
  quei pannelli riporta **da dove si è venuti**, e lo dice nell'etichetta: a P6 si arriva
  da due strade, e mandare l'utente in un pannello dove non è mai stato sarebbe il vicolo
  cieco che il cap. 12.7 vieta. *(T4c, 2026-08-10.)*
- Finestra principale: `MinimumSize` 1150×600, DPI `SystemAware`, sfondo `SfondoBase`.
- **Si apre a misura, non a tutto schermo** *(2026-09-01, su indicazione del tutor)*. Fino a
  quel giorno l'avvio era **massimizzato**, e il difetto peggiore che ne veniva non era
  estetico: nessuno vedeva mai l'applicazione a una misura diversa, e le finestre piccole non
  le provava nessuno — è così che la colonna del menu d'ingresso ha potuto salire sul nome
  del prodotto per due giorni senza che nessun collaudo o sguardo se ne accorgesse (3.6). La
  regola d'apertura è una funzione pura (`ScalaSchermo.MisuraDiApertura`): si parte in stato
  **normale**, grandi al massimo **1920×1024**, centrati sull'area di lavoro; su uno schermo
  che quel tetto non lo contiene si prende l'area di lavoro e basta, e il `MinimumSize` vince
  comunque su tutto. Massimizzare resta un gesto dell'utente: cambia lo stato d'**apertura**,
  non quello che si può fare dopo.
  *Il tetto è in **unità di progetto**, come il minimo e come la soglia della compatta, e si
  converte in pixel dello schermo prima di confrontarlo con l'area di lavoro (decisione
  15.7): a 150% la finestra si apre una volta e mezza più grande, perché deve contenere lo
  stesso disegno. Trattarlo da pixel veri aprirebbe, su uno schermo grande a scala alta, una
  finestra che a video mostra 1280 unità di progetto — poco più del minimo, e proprio sulle
  macchine con lo schermo più grande.*
- **In coda alla barra c'è un «?», e non è l'ottava casella** *(2026-09-01, su indicazione
  del tutor)*. Riapre l'informativa «Come funziona, e cosa esce dal tuo PC» (cap. 11.2), che
  fino a quel giorno compariva **una volta sola** — al primo avvio — e da lì in poi si
  ritrovava soltanto in fondo alle Impostazioni: chi si domanda cosa esce dal proprio
  computer se lo domanda mentre lavora, non mentre configura. Sta lassù perché è lì che si
  cerca un aiuto, ma della barra non condivide il mestiere: la barra è l'**indice dei
  pannelli** e quello non porta a un pannello. Per questo è **stretto** (`BottoneBarraSuperioreIcona`,
  40×34) e vestito da **neutro** invece che dell'azzurro delle destinazioni — sette caselle
  più una che sembra l'ottava sarebbe stato peggio del buco che chiude — e per questo è
  l'unico bottone della barra che **resta acceso mentre l'AI lavora**: non fa uscire da
  nessuna parte, e il momento in cui ci si chiede cosa stia succedendo è proprio quello in
  cui qualcosa sta succedendo.
  *Dentro l'informativa, un bottone «Credits» aggiunge in coda i crediti del prodotto —
  proprietario, autore, e le tecnologie su cui poggia: .NET con Windows Forms, WebView2 per
  il browser incorporato, le API Claude di Anthropic per il motore. Stanno **dentro l'aiuto**
  e non in una finestra loro perché sono la stessa domanda in due tempi, «che cos'è questo
  programma» e «di chi è»; e nominano quel che il prodotto **è**, mai come è stato costruito
  (regola di progetto 12).*
- **La fascia dei comandi va a capo** *(2026-08-14)*. In fondo a ogni pannello sta una
  fascia con due file di bottoni: i comandi di quel pannello a sinistra, quelli che portano
  altrove a destra. Fino alla 0.3.018 ognuna si disponeva per conto suo — una da sinistra,
  una da destra — e quando lo spazio finiva **si incontravano a metà strada**: alla
  `MinimumSize` fino a 676 px di bottoni sopra altri bottoni. Non si vedeva perché
  l'applicazione si apre massimizzata, ed è rimasto lì per tre tappe, peggiorando a ogni
  bottone aggiunto. Ora la fascia si comporta come qualunque barra di comandi: se le due
  file non ci stanno insieme si dispongono su **righe diverse** — quelle che portano
  altrove restano in fondo, dove «Salva profilo» si cerca — e una fila troppo lunga si
  spezza a sua volta. La **fascia cresce in altezza** quanto le righe richiedono: a
  rimetterci è l'area dei dati, che di spazio ne ha, e non la leggibilità dei comandi. Le
  azioni di **livello 6** restano su una riga tutta loro (v. cap. 11.5): il vuoto intorno è
  la loro prima difesa, e non si baratta per due righe di spazio.
  *Il rimedio però poggia su un pavimento che a DPI alti non c'è* **(collaudo dal vivo di
  T9e, 2026-08-23)**: `MinimumSize` viene scalata da `AutoScaleMode.Font` in modo
  **asimmetrico** — ×1,42 in larghezza contro ×1,605 in altezza — così a 150% la finestra
  scende fino a **1088,7 px logici**, 61 sotto il minimo dichiarato. In quei 61 px «Modifica
  i testi» e «Prepara email» tornano a incontrarsi a metà strada, per 77×44 px, e il clic
  finisce al bottone sbagliato: è il difetto del 2026-08-14 che rientra da una porta che
  allora non si guardava. **Curato lo stesso giorno** (decisione 15.7): il minimo di progetto
  si rimette in pixel veri nello `Shown`, quando la scalatura automatica ha già detto la sua,
  e la finestra si ferma dove deve — misurata **1725×900**, che sono i 1150×600 esatti. La
  sovrapposizione, però, era figlia di un altro numero: la fascia calcolava lo spazio
  disponibile col vecchio ingombro del logo, credendone **125 px in più** di quelli veri, e
  concludeva che i due comandi ci stavano su una riga. Dichiarato l'ingombro misurato, va a
  capo da sé: «Modifica i testi» e «Prepara email» sono tornati distanti 90 px, su righe
  diverse. Il minimo non è stato alzato — a difendere quei bottoni non erano i 15 px di
  margine, era il ritorno a capo che non scattava.
- Finestre secondarie (Impostazioni, Primo avvio, Anteprima file, Informazioni su…):
  dialoghi a bordo fisso, sfondo bianco, titolo Segoe UI 14 Bold in `RossoTitoli`.
  *Il «Primo avvio» di questo elenco non è mai nato come finestra a sé: a T6 si è ridotto
  alla sola domanda della chiave (cap. 11.3, cap. 12 A1), e la schermata di T9e non è la
  sua erede — quella compare a **ogni** avvio e non chiede niente.*
- **La prima finestra secondaria è la conferma critica** *(2026-08-14)*: `FinestraConfermaCritica`,
  il dialogo delle azioni di livello 6. Elenca cosa sparisce e cosa resta, e per accendere
  il bottone chiede di **ridigitare una parola** (il nome dell'app, come vuole il
  cap. 11.5). Non è una `MessageBox` perché un Sì/No si preme di riflesso: qui la mano
  deve scrivere. Nasce per «ELIMINA PROFILO - DEFINITIVO» in P2, ma è generica —
  «Elimina tutti i dati» e «Elimina un'opportunità» delle Impostazioni (T9) useranno
  questa. Invio non conferma, Esc annulla. *Delle due promesse, la prima è stata
  mantenuta e la seconda **rovesciata il 2026-08-31**: eliminare una candidatura non passa
  di qui e non sta nelle Impostazioni — v. la voce seguente e il cap. 11.5.*
- **La conferma di livello 5** *(2026-08-31)*: `FinestraConferma`, la sorella minore della
  precedente. Stessa forma — titolo, il testo che dice cosa sparisce, due bottoni, Invio
  che non conferma, Esc che annulla, il fuoco che parte sull'annulla — ma **niente parola
  da riscrivere**: il bottone che esegue è acceso da subito e porta il **verbo**
  dell'azione, «Confermo». È la differenza fra i due livelli detta in una finestra: il 6
  difende gesti che si fanno una volta nella vita e chiede la mano; il 5 difende gesti che
  si **ripetono** — ripulire la coda da una prova, da un doppione — e a quelli una parola
  da riscrivere insegnerebbe soltanto a scriverla senza leggere. Non è una `MessageBox`
  per una ragione sua, diversa da quella della sorella: quella sa dire solo Sì e No, e un
  «Sì» risponde alla domanda mentre «Confermo» risponde alla **conseguenza**. Nasce per
  «Elimina candidatura» in P1 (cap. 11.5).
  *Dal **2026-09-01** è la conferma di **tutto** il livello 5, non di un bottone solo:
  ci passano anche l'eliminazione di una voce del profilo in P2, il «Dimentica» di una
  ricerca salvata in P3 e lo «Scarta» di P4, che chiedevano ancora con una `MessageBox`.
  Una forma di conferma per livello, o il livello non dice più niente (3.3).*
- **Altre due finestre secondarie con T6** *(2026-08-14)*. `FinestraChiaveApi` chiede la
  chiave API quando non c'è (cap. 11.3): compare **prima** che i pannelli si colleghino al
  motore, con la casella mascherata e la spunta per rileggere quel che si è incollato, e
  «Non adesso» è una risposta legittima — senza chiave l'applicazione si apre lo stesso e
  restano spente le sole funzioni che chiamano l'AI. Si centra sullo **schermo** e non sul
  proprietario, perché quando compare la finestra principale non è ancora a video.
  `FinestraDocumenti` è la **conferma umana** del cap. 05.2: mostra i file riconosciuti
  nella cartella documenti con categoria e motivo, e la categoria si corregge scegliendo la
  riga e poi la voce in una tendina — non con una griglia editabile, che sarebbe stato
  l'unico controllo del suo genere in tutta l'applicazione. Non chiama l'AI: «Fai rileggere
  la cartella» e «Cambia cartella» **tornano al pannello**, che è chi sa aspettare e
  annullare (cap. 02.6).
  `FinestraAppunti` *(2026-08-18, T7c)* è la conferma degli **appunti di mira** distillati
  dal ragionamento (cap. 12, A6.3), e somiglia di proposito alla precedente: elenco con le
  spunte, e la riga scelta si corregge nella casella sotto invece che dentro la griglia.
  Due cose sono sue soltanto. La prima: gli appunti arrivano **già spuntati**, perché sono
  la proposta dell'AI su una conversazione appena fatta e togliere è più veloce che
  rimettere. La seconda: sotto l'elenco c'è una **seconda lista che non si spunta** — le
  cose dette in chat che nel profilo non risultano. Non sono appunti e non entrano nei
  documenti; stanno lì con scritto perché, che è l'unico modo di non perderle senza usarle.
  Come le altre, non chiama l'AI: raccoglie una decisione e la restituisce al pannello.
- **Con T9e arrivano le due che non chiedono niente a nessuno** *(2026-08-22)*. La
  **schermata di avvio** (`FinestraAvvio`) copre il montaggio: senza bordi, centrata sullo
  schermo, sopra tutto, e con dentro il marchio disegnato per 800×702. Perché abbia senso
  ha un **tempo minimo** a video (**dieci secondi**, dal 2026-09-01; erano cinque dal
  2026-08-30), ed è una misura e non
  un gusto: dal doppio clic alla finestra passano **265–330 ms**, e una schermata legata al
  solo caricamento lampeggerebbe senza che nessuno la legga. Ma il minimo vale per chi
  guarda, non per chi deve rispondere: chi sta per aprire una finestra che chiede qualcosa
  — al primo avvio la chiave API, che nasce dentro il `Load` — la manda via **subito**, e
  provato dal vivo succede a 438 ms, molto prima della sua scadenza. Su uno schermo
  piccolo, o scalato, si rimpicciolisce per stare dentro il **70%** dell'area di lavoro
  senza deformarsi. **Un clic la chiude, e dal 2026-09-01 anche Invio**: le due vie
  d'uscita sono pari e vanno dette insieme — il clic c'era da sempre e con il minimo
  raddoppiato conta il doppio, e quel che non è scritto da nessuna parte si perde alla
  prima riscrittura.
  *Il minimo è stato riguardato il 2026-09-01 e **raddoppiato**, su indicazione del tutor:
  dieci secondi sono molto lunghi per il costume corrente (una schermata d'avvio sta di
  norma un secondo e mezzo o due), ed è una scelta di chi possiede il prodotto — la marca
  vuole quel tempo. Proprio perché sono lunghi le vie d'uscita diventano tre: il clic di
  sempre, **Invio**, e chi sta per aprire una finestra che chiede qualcosa la manda via da
  sé. L'Invio arriva da un **filtro dei messaggi** e non da un `KeyDown` della schermata,
  ed è una necessità e non un vezzo: la schermata è `TopMost` ma il fuoco della tastiera non
  ce l'ha quasi mai — la finestra principale si apre e si attiva mentre lei è ancora a
  video — e un tasto legato alla sola schermata non scatterebbe mai. Il filtro si toglie
  quando la finestra si chiude: uno dimenticato si mangerebbe ogni Invio del programma.*
  Nasce in `Programma.Main`, **dopo** la biforcazione del server: in modalità `--mcp` non
  esiste, come nessun'altra finestra (cap. 09). La seconda è **«Informazioni su…»**
  (`FinestraInformazioni`), e non ripete le Impostazioni: il marchio, la riga di versione e
  il copyright — cartella dati, modelli e preferenze stanno in P8, e una seconda vetrina
  degli stessi valori sarebbe la solita copia destinata a divergere. La porta è il
  **pannello del logo** (cap. 03.5). *(Dal 2026-08-26 mostra anche il **commit** da cui
  l'eseguibile nasce, con «Copia diagnostica» quando c'è un guasto da raccontare — cap.
  13.9, cap. 11.1 — e dal 2026-08-27 ha due bottoni: «Cerca aggiornamenti» e
  l'informativa. Restano **porte**, non vetrine: nessuno di quei comandi ripete un valore
  che P8 mostra già.)*
- **E con T9b arriva la più grande, che è anche l'unica che non chiede niente**
  *(2026-08-21)*. `FinestraImpostazioni` è P8 (v. la tabella dei pannelli): non raccoglie
  una decisione per restituirla a chi l'ha aperta, ma **fa** — scrive le preferenze appena
  si cambiano, apre cartelle, cancella. Per questo non ha un OK, e per questo le tre cose
  che non si disfano passano da altrettante finestre già esistenti invece che da sue
  varianti. Restituisce al `FormPrincipale` **tre notizie**, non un esito: se è arrivata
  una chiave nuova (allora il motore si rimonta e i pannelli si ricollegano), se l'utente
  vuole i documenti (allora si va in P7), se i dati sono stati eliminati (allora
  l'applicazione si chiude, perché da lì in poi lavorerebbe su file che non ci sono più).
  Una cosa l'ha insegnata al banco: su una finestra mai mostrata `PerformClick` non
  scatena niente, quindi ogni azione che valga la pena collaudare deve avere un **metodo
  pubblico** — è la stessa regola che la `FinestraBackup` aveva già scoperto a T9a.
  *A 150% è risultata la finestra più fragile dell'applicazione* **(collaudo dal vivo di
  T9e, 2026-08-23, il reperto più grave del giro)**: si dimensiona sul **proprio contenuto**
  senza guardare quanto schermo ci sia, e non ha **nessun `AutoScroll`**. Il sistema la
  tronca a 682×1106 — è il suo massimo — mentre il contenuto arriva a y=1384 su un'area di
  lavoro alta 1008: restano fuori «Apri modelli.json», «Backup…», «Svuota i dati di
  navigazione», «ELIMINA TUTTI I DATI» e perfino il bottone «Chiudi», e non è un problema di
  posizione — quei comandi cadono **fuori dalla finestra**, non solo fuori dallo schermo,
  quindi nessuno spostamento li recupera. **Curata lo stesso giorno** (decisione 15.7), in tre
  mosse che vanno insieme: la larghezza dichiarata passa in pixel veri — cruda stringeva la
  finestra di un terzo mentre i testi dentro crescevano col DPI, ed era lì che nasceva metà
  dell'altezza di troppo — un tetto la ferma sull'area di lavoro, e l'`AutoScroll` rende
  raggiungibile quel che resta sotto. Misurata **1012×1008**, con «Backup…», «Svuota i dati di
  navigazione», «ELIMINA TUTTI I DATI» e «Chiudi» raggiungibili scorrendo. La stessa riga
  viveva identica in altre **tre** finestre — Backup, ChiaveApi, ConfermaCritica — e sono state
  curate tutte, non solo quella caduta: la ChiaveApi è quella che al primo avvio compare prima
  della finestra principale, e troncata lì non si sarebbe potuta inserire la chiave.
  *E il 2026-09-01 la stessa cura arriva alle **ultime tre** finestre che ne erano rimaste
  fuori — `FinestraAppunti`, `FinestraDocumenti` e `FinestraModificaTesti`, che si
  dimensionavano anche loro sul proprio contenuto senza guardare quanto schermo ci fosse — e
  P7 dichiara il proprio `AutoScaleMode` come tutti gli altri pannelli, che è la riga da cui
  dipende se il contenuto cresce col DPI o resta indietro. La decisione 15.7 vale adesso su
  ogni finestra dell'applicazione. La **prova a 150%** di queste tre non è stata fatta e
  resta in riserva (`in_sospeso.md`): la cura è la stessa già misurata sulle altre, ma
  «stessa cura» non è «misurata».*
  *E quella cura aveva un difetto suo, che si è visto usandola* **(rifiniture prima del
  giro D, 2026-08-24)**: con l'`AutoScroll` acceso la barra verticale si prende una fetta
  di larghezza, e la fila dei controlli — disposta sulla larghezza intera — le finisce
  sotto; compariva così anche una barra **orizzontale**, che non aveva niente da mostrare.
  Quella barra vale **17 px** a 96 DPI e **26** a 150%, contro i **14** del margine di
  disegno: non bastano a coprirla in nessuno dei due casi. Adesso, quando lo scorrimento
  serve, la fila si dispone **due volte** — la seconda dentro la larghezza che resta tolta
  la barra (`ScalaSchermo.LarghezzaSenzaLaBarra`) — e la riserva si prende **solo** in quel
  caso: toglierla sempre stringerebbe il contenuto per difendersi da una barra che non c'è.
  La domanda non si riapre, perché righe che vanno a capo prima possono solo far crescere
  l'altezza. Nella stessa cura `Disponi` è diventata **`DisponiIn(altezzaDisponibile)`**:
  l'altezza si **riceve** invece di leggerla dallo schermo, perché un banco non può
  cambiare schermo — ed è esattamente il caso in cui questa finestra fa qualcosa di diverso.

- **La quinta finestra è l'unica che parla all'utente invece di chiedergli qualcosa**
  *(2026-08-27, dalla revisione del giro D)*. `FinestraInformativa` — «Come funziona, e cosa
  esce dal tuo PC» — dice a chi usa il programma quel che il cap. 11.2 dice a chi lo
  costruisce: che cosa va all'AI, che cosa non esce affatto, dove restano i dati, che spesa
  comporta un giro. **Compare una volta sola**, al primo avvio e **prima** della finestra
  della chiave, perché è lì che si decide se fidarsi; poi si riapre solo chiedendola, da
  «Informazioni» e da P8. Che sia già comparsa se lo ricorda `impostazioni.json`
  (cap. 11.1), e se quel file non si lascia scrivere l'informativa **ricompare**: il dubbio
  va a favore di chi deve essere informato.
  Due scelte di costruzione. Il **testo sta in un posto solo** — `Voci()`, una funzione
  condivisa che restituisce titoli e paragrafi — e i controlli nascono da lì a runtime:
  così si collauda senza aprire niente, e un collaudo verifica che l'informativa **nomini
  ogni porta** da cui qualcosa esce. Una porta nuova dimenticata nell'informativa diventa un
  rosso invece di una bugia. La **disposizione è quella di P8**, tetto sull'area di lavoro
  più `AutoScroll`, e per la stessa ragione: è una finestra di testo, ed è il testo che a
  DPI alti cresce fino a non starci.
- **In «Informazioni» arrivano due bottoni** *(stesso giorno)*: «Cerca aggiornamenti», che
  chiede a GitHub qual è l'ultima versione pubblicata e **parte solo premendolo**
  (cap. 13.8), e «Come funziona, e cosa esce dal tuo PC», che riapre l'informativa. La
  finestra continua a non ripetere le Impostazioni: sono due porte, non due vetrine degli
  stessi valori.

- **Guardate a occhio, quelle due finestre avevano quattro difetti** *(2026-08-27, Step 2.50)*.
  Erano provate al banco e mai **viste**, ed è la specie di riserva che solo un paio d'occhi
  scioglie. In «Informazioni» la riga del copyright e la fila dei bottoni stavano sulla stessa
  banda: «Cerca aggiornamenti» si leggeva a metà e, nei tre quarti coperti, **non si poteva
  premere** — il clic lo prendeva la scritta davanti, che a video non sembra un controllo. Ora
  le tre righe di testo, i due comandi, l'esito e la coppia «Come funziona»/«Chiudi» stanno su
  bande separate, e **due collaudi** difendono la disposizione: nessun controllo ne copre un
  altro, nessuno esce dalla finestra. Sono collaudi che si scrivono una volta e valgono per
  sempre, perché confrontano rettangoli e non pixel.
- **In P8 la fascia dei comandi non scorre** *(stesso giorno)*. La finestra si apre alta quanto
  l'area di lavoro e il suo contenuto è più alto: «Chiudi» nasceva **145 pixel sotto il bordo**
  dello schermo, e per premerlo bisognava prima scoprire che la finestra si scorreva. Adesso a
  scorrere è un pannello di dentro, e in fondo resta ferma una fascia con «Chiudi» — separata
  da un filo, perché il testo che le passa sotto non sembri tagliato da niente. È la stessa
  regola dei pannelli principali (3.4, la fascia dei comandi), applicata alla finestra che ne
  aveva più bisogno. Le due pulizie restano dov'erano, dentro «I tuoi dati»: un'azione critica
  si spiega col contesto in cui sta, e in una fascia sempre a video sarebbe sempre a portata.
- **P8 è divisa in due colonne: due terzi ai testi, un terzo ai comandi** *(2026-09-01, su
  indicazione del tutor)*. Fino a quel giorno era una colonna sola — titolo di sezione, il
  paragrafo che spiega, e **sotto** il bottone, ognuno largo quanto la propria scritta — e
  dalla fotografia del tutor ne veniva «una colonna di button disordinati e grossi quanto la
  scritta che c'è dentro»: otto bottoni, otto larghezze. Adesso i comandi stanno **in colonna
  a destra**, tutti della **stessa larghezza** — quella della colonna, che è un terzo della
  finestra (`FrazioneColonnaDeiComandi`, 3.2) — e ciascuno **all'altezza della sua sezione**,
  così il bottone si legge accanto a quel che dice a cosa serve invece che dopo. Il testo
  resta nei suoi due terzi e non gli passa più sotto.
  *Restano nel flusso del testo, e non è una dimenticanza: la tendina della lingua, il
  numerico dei giorni e le due tendine dei modelli. Non sono comandi ma **valori** — si
  leggono insieme alla frase che li introduce, e «italiano» accanto a «Lingua dei documenti»
  è una riga sola che si capisce; spostarli a destra spezzerebbe la frase per farne una
  colonna. «ELIMINA TUTTI I DATI» invece nella colonna ci va, con il suo vuoto di difesa
  sopra e sotto (3.3), e «Chiudi» resta nella fascia in fondo, che non scorre.*
  *La colonna unica ha chiesto una scritta più corta: «Come funziona, e cosa esce dal tuo PC»
  voleva 216 px dove la colonna ne dà 210, ed è diventata **«Come funziona…»** — qui e in
  «Informazioni su…», che apre la stessa finestra: due nomi per la stessa porta sono il modo
  più economico di credere che siano due porte. Il titolo per esteso resta dov'è sempre
  stato, dentro la finestra che si apre.*

## 3.5 Il pannello del logo (in basso a sinistra)

Elemento identitario irrinunciabile, presente in ogni momento nell'angolo in basso a
sinistra della finestra principale — ed è, dal **2026-09-01**, l'**unico** posto in cui il
marchio vive:

```
┌──────────────────────────┐
│        [immagine]        │   PictureBox 101×101, scudo Aviolab AI
│                          │   (in forma binaria nel sorgente)
│       AVIOLAB AI         │   Segoe UI 16 Bold, TestoPrimario, centrato
│  Ver. 1.0.012 · Pool 1.03│   Segoe UI 9, TestoSecondario, centrato
│ ©2026 Aviolab AI - Tutti │   Segoe UI 9, TestoSecondario, centrato
│   i diritti riservati    │
└──────────────────────────┘   nessun contorno, e il fondo è quello di sotto
```

> Il pannello in basso a sinistra è il **marchio aziendale**: sotto lo scudo compare
> **sempre e solo «AVIOLAB AI»** *(precisato 2026-08-06)*. **Il nome mostrato
> all'utente resta «TrovaLavoro»** (cap. 15, voce 3) nella barra del titolo; il
> sottotitolo «Crea il tuo miglior CV e rispondi subito all'annuncio di lavoro
> perfetto per te!» compare nella finestra «Informazioni su…» e nel primo avvio
> *(cambiato il 2026-08-22 insieme al banner del logo: prima era «e candidati con
> il CV giusto, senza fatica»)*. *AI-CV-COACH* resta il nome del progetto e del
> repository, non del prodotto.

**Il marchio sta in un posto solo, e questo è quel posto** *(2026-09-01, revisione di
finalizzazione col tutor — e **rovescia** due decisioni dei due giorni precedenti)*. Fra il
30 e il 31 agosto lo stemma Aviolab era arrivato in altri due punti: il **mega stemma**
dietro la colonna dei bottoni del menu d'ingresso (3.6) e lo **scudo dell'attesa** in mezzo
allo schermo (3.8). Le tre presenze erano state dichiarate «irrinunciabili»; ne bastava una.
La ragione generale è che un marchio ripetuto tre volte nella stessa applicazione non si
legge tre volte: si legge come rumore, e alla terza non lo guarda più nessuno. E i due posti
nuovi erano sbagliati per una ragione ciascuno. Nel menu lo stemma stava **dietro i
bottoni**, cioè dietro l'unica cosa che lì si deve guardare, e per vedersi obbligava la
colonna a stringersi. Nell'attesa prendeva il centro dello schermo con qualcosa che
dell'attesa non dice niente: chi guarda sta aspettando, non ammirando — un'attesa non è il
posto in cui firmarsi — e la ruota, per stargli addosso, doveva difendere i suoi pallini
d'argento dal blu, dal bianco e dal giallo delle stelle. Restano perciò l'avorio, il nome e
il sottotitolo nel menu, e in mezzo allo schermo un indicatore **neutro**: la ruota di
pallini con la sua barra. Il marchio torna qui, dov'è di casa e dov'è a video in ogni
momento — la presenza continua vale più della somma delle tre.

- `Panel` di circa **261×216 px**, ancorato **Bottom+Left**, aggiunto al form come
  elemento flottante sopra la struttura (così sopravvive ai ridimensionamenti). **Il fondo
  non è suo: è quello del pannello che gli sta sotto** *(dal 2026-09-01, su indicazione del
  tutor)* — avorio `FondoMenu` nel menu d'ingresso, caldo `FondoPagina` nelle sei pagine —
  e si riallinea a ogni cambio di pannello. Un colore fisso si fonderebbe con uno dei due e
  lascerebbe un rettangolo visibile sull'altro, che è poi il riquadro che si è tolto.
  *Quelle due misure sono anche l'ingombro che la finestra dichiara ai pannelli
  (`IPannelloArea`), e a DPI alti non descrivono più il pannello vero* **(collaudo dal vivo
  di T9e, 2026-08-23)**: a 150% misura **373×360**, e la differenza **sfonda nell'area
  viva** — copre due righe della coda in P1, l'angolo basso-sinistro della casella
  «Annuncio» in P6, e 71 px del bottone di sinistra in entrambi i pannelli. Nessuno dei 995
  collaudi verdi poteva vederlo: a 96 DPI quei numeri sono giusti. **Curato lo stesso giorno**
  (decisione 15.7): l'ingombro non si dichiara più ripetendo le costanti, si **misura** sul
  pannello vero, a ogni ridimensionamento. Una costante che copia un valore già posseduto dal
  runtime è destinata a divergere da lui, e a DPI alti divergeva di 112 px in larghezza.
  **Niente contorno e niente fondo suo: il pannello si fonde con la schermata**
  *(2026-09-01, su indicazione del tutor)*. Fra il 30 agosto e quel giorno il pannello ha
  portato un **filo nero da 1 px** dipinto nel `Paint`, e un fondo `SfondoBase` che lo
  staccava da tutto il resto: era un riquadro appoggiato sopra l'area. Adesso non è un
  riquadro, è il marchio posato sull'angolo — via il filo, via il fondo proprio. *Ne è
  caduto anche il token `BordoMarchio` (`#000000`), che non aveva altri lettori, e il
  **rientro di 1 px per lato** delle tre scritte: rientravano perché il loro fondo opaco
  avrebbe mangiato il filo verticale alle sole righe che occupano — un contorno interrotto
  tre volte, che a occhio sembra intero. Tolto il filo, è caduto il motivo del rientro.*
- La riga versione mostra **due numeri**: la versione dell'applicazione e la versione
  della **libreria prompt** caricata (cap. 04), separate dal punto mediano « · ».
  Il numero di pool dichiara anche sorgente e stato: `Pool 1.03` (cartella esterna),
  `Pool 1.03 (integrato)` (copia incorporata nell'exe), `Pool 1.03*` (file modificati
  rispetto al manifest — cap. 04.5). «Pool —» può comparire solo in caso di anomalia
  totale, e l'app la spiega.
- **Modalità compatta**: sotto ~1350 px di larghezza restano solo l'immagine (ridotta) e
  la versione, per liberare spazio.
  *A 150% questa modalità è **irraggiungibile*** **(stesso collaudo)**: la soglia confronta
  `ClientSize.Width`, che è in pixel fisici, con un numero pensato a 96 DPI, e servirebbero
  900 px logici per farla scattare — meno del minimo che la finestra concede. È codice morto
  a DPI alti, ed è il motivo per cui il pannello resta grande proprio quando lo spazio
  manca. **Curata lo stesso giorno** (decisione 15.7): la soglia si confronta in unità di
  progetto e non in pixel dello schermo. La conseguenza è voluta ed è visibile a occhio: su uno
  schermo 1920 al 150% la finestra massimizzata vale 1280 unità, quindi la compatta **scatta
  sempre** — ed è giusto che scatti, perché a quella scala lo spazio per il contenuto è quello
  di uno schermo 1280.
- Il logo è lo **scudo di Aviolab AI** e vive **in forma binaria dentro il sorgente**
  (PNG 256×256 codificato Base64 in `LogoAviolab.vb`): nel repository e accanto all'exe
  non esiste nessun file immagine. *(Deciso 2026-08-06 in T1 — cap. 15, voce 4.)*
- Il numero di versione dell'app vive in **un solo file sorgente** (`Versione.vb`, una
  costante), mai duplicato altrove; ogni modifica al codice lo incrementa.
- **Il pannello si clicca, e apre «Informazioni su…»** *(T9e, 2026-08-22)*. Vale su tutte
  le sue parti — immagine, nome, versione, copyright — con la mano sul puntatore e il
  suggerimento, perché una porta senza maniglia non è una porta: è la lezione di T9d,
  «quel che è acceso deve sembrarlo», applicata a un riquadro che per undici tappe è stato
  solo un'insegna. La porta è questa e non un bottone in barra: qui versione e pool si
  vanno già a leggere, e il gesto è quello che si prova per primo.
- La riga «Ver. … · Pool …» la compone **`Versione.Riga`**, non chi la mostra: le finestre
  che la scrivono sono due — questo pannello e «Informazioni su…» — e due copie della
  stessa riga divergono al primo ritocco.

## 3.6 I pannelli, uno per uno

| ID | Pannello | Contenuto principale |
|---|---|---|
| **P1 Home** | cruscotto | stato del profilo (esiste? aggiornato quando?), coda opportunità con stelle e stati, scorciatoie ai flussi («Nuova ricerca», «Aggiorna profilo»). *Costruito a T5c (2026-08-13), ed è il pannello su cui l'applicazione ora **si apre**. La coda è una lista in vista dettagli con sei colonne — **Match** (stelle, numero e ⛔), **Azienda**, **Ruolo**, **Stato**, **Da dove**, **Aggiornata** — ordinabile cliccando un'intestazione, con un filtro «Mostra» (Tutte · Da completare · Generate · Scartate) e i contatori del cap. 07.3; quello delle **inviate** è entrato con **T6** *(2026-08-14)*, cioè con la tappa che lo fa salire — prima sarebbe stato fermo a zero, e un contatore che non può muoversi non conta niente. *Con T6 la coda si filtra anche **per stelle** («almeno N»), e i due filtri si intersecano invece di sostituirsi: rispondono a domande diverse. Da qui l'elenco si può anche **esportare** in CSV o markdown (cap. 07.3).* Le **scartate restano** nell'elenco, scritte in grigio: scartare non è cancellare. Una candidatura si riapre dal bottone «Apri la candidatura» o dal doppio clic sulla riga. **Qui si guarda, non si decide**: il pannello non cambia lo stato di nessuna candidatura — nemmeno lo scarto sta qui, ma nella scheda che si sta guardando *(emendato il 2026-08-31: v. in fondo a questa riga)*. Senza profilo il cruscotto lo dice e il bottone accanto cambia mestiere («Costruisci il profilo» invece di «Apri il profilo»), perché da lì si comincia; «Aggiorna profilo» è lì spento col suo tooltip (flusso D), per la regola 3.8* *A **T9c** (2026-08-21) la Home impara a dire chi aspetta da troppo (cap. 07.3): sotto i contatori compare una riga — «⏳ 2 candidature spedite aspettano da più di 14 giorni; la più vecchia da 21 (Acme — Magazziniere)» — le righe interessate portano nella colonna «Stato» **da quanti giorni** aspettano («Inviata · 20 gg») e si isolano con la voce «Da sollecitare» del filtro «Mostra». Quando non c'è niente da ricordare la riga **sparisce** e la fascia dei filtri torna alta com'era, perché un avviso che occupa spazio anche da spento insegna a non guardarlo. La stessa colonna «Stato» dice ora l'**esito** quando c'è — «Rifiutata», non «Con esito» — e ai contatori se n'è aggiunto uno, «con esito», che prima sarebbe stato fermo a zero* *Il **2026-08-31** la Home prende il suo unico gesto che non è guardare: **«Elimina candidatura» (L5)**, al posto di «Aggiorna profilo» — un bottone che stava lì spento da T5c per una tappa mai arrivata (il flusso D), e che a furia di non accendersi mai aveva smesso di significare «arriverà» per significare «non funziona». Il principio in cima a questa riga si **emenda invece di aggirarlo**: qui si guarda, **e si toglie di mezzo**. Eliminare non decide niente *sulla* candidatura — la toglie dall'archivio (cap. 11.5) — e dopo non resterebbe nessuna scheda da cui farlo; la coda è anche il solo posto da cui si vedono tutte insieme, che è come ci si accorge del doppione e della prova da ripulire. È acceso **solo con una riga scelta**, come «Apri la candidatura», e sta **in fondo alla fila** proprio per quello: due bottoni che si accendono insieme e fanno il contrario non vanno sotto lo stesso dito — la stessa ragione per cui in P2 «ELIMINA PROFILO» sta all'estremo opposto rispetto a «Salva profilo». La conferma è di **livello 5** e passa dalla `FinestraConferma` (3.4): dice **quale** candidatura sparisce — azienda e ruolo, com'è scritta la riga — e che cosa c'era nella cartella, e si accetta con un clic su «Confermo». Dopo, i tre pannelli che potevano averla in mano — la scheda, i documenti, l'email — la **dimenticano**: non per pulire la vista, ma perché ognuno di loro scrive nella cartella della candidatura, e su un oggetto sopravvissuto alla propria cartella la ricreerebbero* |
| **P0 Menu** | menu d'ingresso | **sei bottoni azzurri** su un fondo avorio, col nome del prodotto e il sottotitolo in cima (e nient'altro dietro, dal 2026-09-01 — v. in fondo a questa riga): «Le mie candidature» (P1), «Profilo e CV base» (P2), «Ricerca annuncio — ONLINE» (P3), «Confronta ANNUNCIO - CV / Match 1-5 ⭐» (P4), «Elabora Documentazione» (P6), «Impostazioni» (P8). *Nato il 2026-08-30.* È il pannello su cui l'applicazione **si apre**, e non sostituisce la barra: la precede. La barra resta e porta agli stessi posti; per tornare qui c'è la voce **«🎮 Menu»**, prima in barra — senza, dal menu si esce e non si rientra. **Il menu non sa dove portano i suoi bottoni**: preme quello corrispondente della barra, così i cammini restano quelli di prima per costruzione (dietro «Ricerca annuncio» c'è l'accensione del browser, dietro «Impostazioni» una finestra che rimonta il motore: riscriverli qui sarebbe stata una seconda strada destinata a divergere). Per la stessa ragione il menu **si spegne insieme alla barra** mentre l'AI lavora: gli stati non si riscrivono, si leggono dai bottoni. Lo sfondo, dal **2026-08-30 (sera)**, non è più il banner: si **dipinge**. Sono tre cose — l'avorio di `FondoMenu`, il **mega stemma** Aviolab dietro la colonna dei bottoni, e in cima le stesse due righe del banner (nome e sottotitolo), **centrate sull'asse** e senza il timbro che là sta a destra del nome. Il guadagno non è estetico: un fondo dipinto **segue la finestra**, mentre un'immagine può solo starci dentro o essere tagliata — e su una finestra panoramica il banner quasi quadrato lasciava blu ai lati. Il velo bianco resta e **li prende tutti e tre**: sull'avorio non si stende (`#FFFAF0` velato dà `#FFFDF8`, che è lo stesso colore, e il fondo va lasciato esatto) ma su quel che ci sta sopra — stemma, nome e sottotitolo insieme, perché sono tutti e tre sfondo e sfondo vuol dire stare dietro ai bottoni senza contendere. Tecnicamente: gli elementi si disegnano **a piena forza su una tela a parte**, e la tela si appoggia sull'avorio con l'opacità abbassata in una volta sola. Le due vie corte non funzionano — disegnare ciascuno con un colore già trasparente farebbe riaffiorare, dentro le lettere, il contorno nero sotto il riempimento bianco; un rettangolo bianco steso sopra tutto, com'era sul banner, schiarirebbe anche l'avorio. La colonna si centra ora nello spazio **sotto il nome**, che è l'erede della vecchia «zona dentro la cornice». Nella stessa sera i bottoni passano da **690×87 a 420×53**, ed è la stessa decisione vista dall'altra parte: con la colonna larga come prima lo scudo ci spariva sotto e se ne vedevano solo le strisce fra un bottone e l'altro — che non sembrava un marchio, sembrava un difetto. Il rapporto fra i due lati resta quello (7,9) perché il corpo del testo segue l'altezza: stringere la sola larghezza avrebbe lasciato lettere da bottone grande in un bottone corto. Il pavimento è il nome più lungo, «Confronta ANNUNCIO - CV / Match 1-5 ⭐», che a misura piena vuole 379 px: ne restano quaranta di margine, e il banco lo sorveglia. Nella stessa sera cambia anche **l'aspetto** dei bottoni: da pillola blu col bordo giallo e il testo bianco contornato a **rettangolo appena smussato**, riempito di `FondoAzione` con la cornice di `BordoForte`, un filo di `SfondoContenuto` fra le due e il testo in `TestoPrimario` — quattro token già in tabella, nessun colore nuovo. La ragione è la stessa di tutto il resto: cambiato il fondo, sei blocchi blu davanti a uno stemma blu si contendevano lo sguardo, e la cosa più scura della schermata erano i bottoni invece del marchio. Ne segue anche che al passaggio del mouse il riempimento ora **si scurisce** invece di schiarirsi: su un azzurro già chiarissimo, schiarire non si vedrebbe. Lo stemma, infine, non si ferma dove cominciano i bottoni ma **risale nel respiro** e arriva poco sotto il sottotitolo: quel vuoto serve a staccare il nome dalla prima pillola, non al marchio, e lasciarglielo significava una striscia di avorio che non era di nessuno. *Il 2026-08-30, guardandolo a video, quattro voci cambiano nome: il primo bottone e la seconda voce della barra si chiamano ora **allo stesso modo** («Le mie candidature»), e così il quarto bottone e il quinto della barra («Confronta ANNUNCIO - CV»), perché portano allo stesso pannello e sentirseli chiamare in due modi diversi è il modo più economico di credere che siano due posti; il quarto passa da «Incolla annuncio — OFFLINE» a **«Confronta ANNUNCIO - CV / Match 1-5 ⭐»**, che è la stessa porta chiamata col nome di quel che ne esce invece che con quello di come ci si entra; il quinto si accorcia in «Elabora Documentazione». La destinazione non cambia per nessuno: il menu preme sempre il bottone della barra, e i cammini restano quelli.* *E il **2026-09-01**, con la revisione di finalizzazione, il **mega stemma se ne va**: il fondo resta l'avorio con il nome e il sottotitolo, e il marchio torna a vivere nel solo pannello del logo (3.5). Stava **dietro i bottoni**, cioè dietro l'unica cosa che qui si deve guardare, e per quanto sbiadito era pur sempre uno sfondo con cui contendere. Due conseguenze da tenere presenti. La prima: la misura dei bottoni **resta 420×53**, ma non è più quella misura per la ragione che l'aveva decisa — a stringerli era stato lo scudo dietro, che non c'è più; il numero si tiene perché a video funziona, e il pavimento resta il nome più lungo. La seconda: il velo bianco adesso prende **due** cose e non tre, nome e sottotitolo, e la ragione è la stessa di prima — sono sfondo, e sfondo vuol dire stare dietro ai bottoni senza contendere.* *E sempre il **2026-09-01**, su indicazione del tutor, la **scritta impara a fermarsi**. Nome e sottotitolo scalavano già con la larghezza del pannello — il 40% e il 74% — ma senza fermi, e la **fascia** che li ospita era una frazione dell'altezza: un terzo. Le due cose insieme facevano un manifesto su una finestra alta e stretta, perché il nome cresceva dentro una fascia che cresceva con l'altezza, mentre i bottoni di crescere si erano già fermati a 420×53. Adesso le due frazioni si applicano a una **larghezza di riferimento** — la larghezza del pannello fermata fra **950 e 1500 px** — e la **fascia segue lei** (il 13% della larghezza di riferimento), con la vecchia frazione dell'altezza rimasta a fare da tetto perché su una finestra bassa la fascia non si prenda comunque più di un terzo. Il minimo oggi non morde — la finestra non scende sotto i 1150 px — e sta lì per il giorno che quel minimo cambiasse. Il **velo bianco** non cambia: si stende sulla tela del testo, quindi segue la scritta per costruzione, qualunque misura prenda. I numeri sono un primo taglio deciso al conto e **vanno riguardati a video**, che è come si decidono le misure di questo capitolo.* *E lo stesso giorno, da una fotografia del tutor a 1136×593, il difetto che stava sotto: a finestra piccola la colonna dei bottoni **si sovrapponeva** al nome — «TrovaLavoro» spuntava da dietro le prime due voci e il sottotitolo tagliava in mezzo ai bottoni — e in fondo restava una striscia di pixel tagliati. Il centraggio era giusto; era sbagliata la **guardia** che tratteneva la colonna: guardava il bordo del pannello (14 px) invece della fine della fascia, e il rialzo dell'occhio (`RialzoColonna`) la faceva salire fin lì. Adesso l'ordine verticale — nome, sottotitolo, colonna — è garantito da un confronto e non da un centraggio che di solito viene bene: il pavimento della colonna è la fine della fascia, lo spazio si conta **dalla fascia al pavimento di sotto** (il bordo, o la cima del logo se la colonna gli passa sopra), e i bottoni si stringono fino ai loro minimi per starci. Ne segue che il **rialzo cede il passo**: dove lo spazio non basta la colonna si appoggia alla fascia e quella gentilezza non si vede più — nessuna gentilezza dell'occhio vale una sovrapposizione. Sotto la misura minima della finestra, che il `MinimumSize` non lascia raggiungere, la colonna esce comunque in basso: fra uscire dove non c'è niente e salire sul nome, esce in basso. Il difetto è rimasto invisibile per due giorni perché l'applicazione **si apriva massimizzata** (3.4): nessuno vedeva mai una finestra piccola.* |
| **P2 Profilo** | scheda del profilo | tutte le sezioni del profilo JSON **campo per campo, modificabili**; bottoni: «IMPORTA CV DA UN FILE» (L2), «IMPORTA CV DA LINKEDIN» (L2), «COSTRUISCI IL TUO CV - DIALOGO GUIDATO» (L2), «Sessione di aggiornamento» (L2), «Genera 📄 CV-1 base» (L3), «Esporta backup» (L2), **«Salva profilo» (L1)** — è la sola porta da cui il profilo entra nell'archivio, anche quando arriva dal dialogo (cap. 12.2) — e **«ELIMINA PROFILO - DEFINITIVO» (L6)**, che è la porta opposta. *Dalla revisione adversariale (2026-08-09): il salvataggio **pota le voci mai riempite** (un «Aggiungi» lasciato vuoto non diventa un'esperienza fantasma nei prompt), le categorie della patente valgono solo con il «sì», e la scheda del testo letto compare solo finché il profilo mostrato è quello importato.* *A T5d (2026-08-14) l'import diventa **due bottoni** invece di uno, e ognuno dice da dove legge: il vecchio si chiama ora «da un file…», il nuovo «da un sito…». Il secondo non legge niente — **porta in P3**, dove vive il browser, e il pannello che lo accoglie dice cosa fare. La scelta della strada sta qui perché è qui che si costruisce il profilo ed è qui che la si cerca; l'atto sta là perché è là che c'è una pagina aperta. Sono spenti insieme, per la stessa ragione: senza chiave nessuna delle due strade arriva da nessuna parte, e mandare l'utente in fondo a un corridoio per dirglielo lì sarebbe scortese* *Il 2026-08-14 le **tre porte del profilo cambiano nome** (i nomi qui sopra sono i nuovi; la nota di T5d racconta com'erano quando sono nate): maiuscolo e più espliciti, perché sono la prima cosa che un utente nuovo guarda in questo pannello. Due conseguenze da tenere presenti. La prima: «IMPORTA CV DA LINKEDIN» **promette meno di quello che fa** — porta in P3, dove il browser legge qualunque pagina che racconti un percorso, e a T5d si era deciso apposta di non controllare che il sito fosse LinkedIn (cap. 06.7); il nome sceglie il caso d'uso vero, e a dire il resto resta il pannello che accoglie. La seconda: il bottone del dialogo è passato da 200 a **300 px** perché l'etichetta non ci stava, e la fila di sinistra si è allungata di altrettanto — il che peggiora la sovrapposizione a finestra stretta annotata in `in_sospeso.md` (3.4)* *Il 2026-08-14 compare **«ELIMINA PROFILO - DEFINITIVO» (L6)**, la porta opposta al salvataggio: manda via la cartella `profilo\` intera — profilo, storico, 📄 CV-1 base e i suoi file — e **non tocca le candidature**, che restano nella Home con il loro registro (cap. 11.5). Sta nella fascia delle azioni **con «Salva profilo», ma all'estremo opposto della fila**: è il solo bottone del pannello da cui non si torna indietro, e non deve stare sotto il dito di chi sta salvando. Prima di eseguire passa dalla `FinestraConfermaCritica` (3.4), che elenca cosa sparisce e cosa resta e vuole la parola `TrovaLavoro` scritta a mano; è acceso solo quando c'è davvero qualcosa da eliminare — un profilo su disco o delle correzioni nei campi — perché un bottone rosso che non ha niente da fare insegna solo a non fidarsi del colore. Quando l'eliminazione avviene, si svuota **tutta l'applicazione**: i campi e la scheda «Testo letto» di P2, il dialogo guidato di P5 (che altrimenti riproporrebbe il profilo appena cancellato), il 📄 CV-1 base in mostra in P6 — mai i documenti di una candidatura — e la Home rilegge* *A T7d (2026-08-18) «Genera 📄 CV-1 base» **genera solo la prima volta**: se un CV-1 base c'è già, porta in P6 e lo mostra. Il verbo dell'etichetta promette più di quel che fa, ed è di proposito — è la stessa scelta di «Genera CV + lettera» in P4, dove il bottone è la porta di quei documenti e non solo il comando che li crea; a rifarli, in tutti e due i casi, c'è «Rigenera», che lo dichiara e lo chiede* *Il 2026-08-19 «IMPORTA CV DA UN FILE» smette di partire dal nulla: se la cartella documenti è stata classificata (cap. 05.2) e il CV più recente esiste ancora, il pannello **lo propone per nome** in una domanda a tre uscite — «Sì, usa questo», «No, scelgo io un altro file», «Annulla». È la porta «qui c'è tutto» che il capitolo dei documenti prometteva: si **propone e non si prende**, perché la conferma umana resta il passo che decide, e l'esistenza del file si verifica **al momento di proporlo** — fra la classificazione e l'import quel CV può essere stato spostato o buttato. Nessun controllo nuovo nel pannello: è una finestra di sistema, e il layout di P2 resta quello validato a video a T3* *A T9a (2026-08-21) «Esporta backup» si accende e diventa **«Backup…»**: apre la finestra di backup e ripristino (F7, cap. 11.4), che tiene insieme le due metà della stessa funzione. L'etichetta perde il verbo perché il bottone non esporta soltanto, e un bottone che dicesse «Esporta» per poi offrire anche il ritorno indietro nasconderebbe metà di quel che fa. Prima di aprirla, le correzioni non salvate si fanno confermare: nel backup finisce il profilo che sta **su disco**, e un ripristino le sostituirebbe senza preavviso; chiusa la finestra dopo un ripristino, il pannello **rilegge il profilo** invece di continuare a mostrare quello di prima* *Il 2026-08-24, chiudendo i reperti del collaudo dal vivo, due cure che il pannello aspettava dal principio. La prima: in **sei delle diciotto caselle** — Ruolo, Azienda, Cosa facevo (informale), Competenza, Titolo, Anno — scrivere «abc» lasciava a video «cba». Sono esattamente le caselle che alimentano l'**etichetta di una riga d'elenco**: riscrivere quella riga con `Items(i) = …` non la modifica, la **toglie e la rimette**, e nel farlo ripristina la selezione — un `SelectedIndexChanged` che nessuno ha chiesto, prima con la riga assente (e i campi si svuotano) e poi con la riga tornata. Chi lo ascolta ricarica i campi sotto la mano che scrive, e ricaricare un campo riporta il cursore a zero: la lettera dopo cade a sinistra di quella prima. La rinomina adesso avviene **dentro la guardia `Riempiendo`**, la stessa che già zittiva i gestori quando a scrivere è il programma. La seconda: senza nessuna voce scelta le caselle della scheda erano **scrivibili ma senza destinazione** — il testo veniva buttato in silenzio, e il primo «Aggiungi» ripuliva la casella. Adesso sono **in sola lettura finché una voce non è scelta**, esattamente come «Elimina» è spento davanti a un elenco vuoto (3.8); i dati personali, che nel profilo non stanno in un elenco, restano scrivibili sempre* *Il 2026-08-30 il pannello guadagna una **quinta scheda, «📄 CV base»**: il profilo mostrato nella forma di un CV, **senza chiamare l'AI**. Si rifà a ogni battuta di tasto dal profilo che si sta correggendo — aggiungi un'esperienza e compare mentre la scrivi — perché non è salvata da nessuna parte: non è un documento, è un riflesso. Si può fare perché lo schema del CV base è quasi tutto **campi-fatto ricopiati** dal profilo (nome, recapiti, patente, ruolo, azienda, durata, competenze, formazione), e i due soli campi-prosa restano dichiaratamente vuoti: il `sommario` e le `descrizione` riformulate le scrive l'AI con «Genera 📄 CV-1 base», che è lì sotto. La differenza fra le due cose è precisamente **cosa ci mette il modello**. Impagina con lo stesso `Impaginazione.PaginaCv` del documento vero, e una riga di stato dice se il 📄 CV-1 dell'AI esiste, di quando è e se il profilo è cambiato dopo — **riferisce, non decide**: rigenerare resta una scelta dell'utente, com'è in P6 dal principio, perché quel CV potrebbe essere quello già spedito* |
| **P3 Ricerca** | browser integrato | WebView2 a tutta area; sopra: barra con ricerche salvate (ComboBox), campo link, bottone **«Cattura annuncio»** (L1); sotto: ultima cattura con esito. *Costruito a T5a (2026-08-12), la barra di sopra è risultata di tre righe invece di una, e la cattura è scesa in fondo: **ricerche salvate** (menù + «Apri» + «Dimentica»); **la ricerca nuova** (menù «Cerca su» dei portali, «cosa», «dove», «Cerca», «Salva questa ricerca»); **la navigazione** («◀», «⟳», casella dell'indirizzo, «Vai»). «Cattura annuncio» sta nella fascia delle azioni in basso, accanto alla riga che racconta l'esito — dov'è il bottone principale in tutti gli altri pannelli. I tre comandi senza etichetta dichiarano il proprio nome accessibile: senza, per chi non vede lo schermo sarebbero anonimi. La prima pagina che si apre è **scritta da noi e non tocca la rete**: dice in tre righe come si usa il pannello, ed è l'unica navigazione che parte da sola.* *A T5d (2026-08-14) accanto alla cattura compare **«Importa CV da questa pagina» (L1)**, che legge la pagina aperta — di norma la propria pagina profilo — e la porta a P2. Sono due bottoni e non uno con due usi, perché il testo va in due direzioni diverse: uno all'analisi dell'annuncio, l'altro alla scheda del profilo. La pagina di casa guadagna il suo quarto punto, e la riga di stato dice quanti caratteri sono stati letti — è il solo modo che l'utente ha di accorgersi che alla strutturazione è andata poca roba prima di guardare un profilo dimezzato e crederlo intero* *Il 2026-08-30 l'avviso di R5 — «questa è la pagina con l'elenco, non un annuncio» — esce dalla riga di stato e diventa una **finestra a comparsa**, e dice una cosa che prima non diceva: come si apre il singolo annuncio, cioè **clic destro sul titolo e «Apri collegamento in una nuova finestra»** (che il pannello intercetta e apre qui dentro, cap. 06.5). Il motivo è di misura, non di tono: quel testo chiede sei righe e la riga di stato ne ha due, quindi arrivava tagliato — e la metà che si perdeva era proprio la via d'uscita. Nella riga di stato resta la versione corta, come traccia. È il primo avviso di questo pannello che si deve chiudere: gli altri restano dove stavano, perché dicono cos'è successo, non cosa fare.* |
| **P4 Opportunità** | dettaglio candidatura | annuncio estratto, **stelle 0–5 grandi**, elenco giudizi (✓ ~ ✗ ?) con ⛔ sugli eliminatori, note di clamp/gate, lettura d'insieme; bottoni: «Brainstorm» (L2), «Genera CV+lettera» (L3), «Scarta» (L5). *Deciso aprendo T4 (2026-08-10): il pannello nasce con in cima una **fascia d'ingresso** — casella multiriga «incolla qui il testo dell'annuncio» e bottone «Analizza» (L3) — perché a T4 quella è l'**unica** porta da cui un annuncio entra: la cattura dal browser è T5 e il flusso C (cap. 12.3) la dà già come ripiego permanente, non provvisorio. A T5 la fascia non sparisce: si affianca alla cattura, e resta la strada di chi ha in mano un testo e basta. A cattura avvenuta la fascia si richiude, per non rubare spazio ai giudizi.* *Ed è andata così (T5b, 2026-08-12), con un di più che non era scritto: l'annuncio catturato in P3 **entra proprio in quella casella**, e si vede. Non c'erano due strade da tenere allineate — ce n'è una sola, e chi guarda può leggere il testo che è stato mandato all'AI, correggerlo e rilanciare* *E a T5c (2026-08-13) il pannello ha preso il suo stato: in cima, accanto alle stelle, la scheda dice a che punto è quella candidatura, e da dove è stata riaperta. **«Scarta» si accende**, con la conferma che spiega cosa succede davvero («non cancello niente: resta nella sua cartella e la ritrovi nella Home. Ma la do per chiusa, e da uno scarto non si torna indietro»); dopo, la scheda **resta a video** con i comandi spenti — sparire sarebbe stato togliere all'utente la cosa che ha appena guardato* *A **T7c** (2026-08-18) si accende anche **«Brainstorm»**, che stava lì spento da T4 con scritto per quale tappa. Le condizioni sono tre e ognuna dice qualcosa: serve un **confronto già fatto** (prima non ci sarebbe niente di cui parlare, e il prompt vuole i giudizi), serve l'**AI** — a differenza dei documenti, che si riaprono anche senza — e la candidatura non deve essere **scartata**, per la stessa ragione per cui non le si scrive più un CV. Premendolo si va in P5, che per l'occasione fa l'altro dei suoi due mestieri* *A **T9c** (2026-08-21) arrivano due gesti che mancavano. Il primo è **«Com'è andata…» (L2)**, accanto agli altri comandi: apre un menù con «In attesa — nessuna risposta», poi i tre esiti — Colloquio · Rifiutata · Assunto 🎉 — e la spunta su quello di adesso (cap. 07.3). È acceso **solo da «inviata» in poi**, perché prima non c'è niente che possa essere andato in un modo o nell'altro, e non chiede conferma: un esito si disfa con un secondo clic sullo stesso menù, e la conferma resta dov'è servita — sullo scarto. Il menù si apre **sopra** il bottone: da lì in giù finirebbe fuori dalla finestra. Il secondo gesto non è un bottone nuovo ma un mestiere in più di «Analizza», che sulla candidatura riaperta **ferma al solo annuncio** diventa **«Confronta»** e fa il secondo passo da solo — è il vicolo cieco trovato dal collaudo di T8 (oggi quelle candidature le sa creare solo il server MCP, cap. 09.3): l'annuncio era già strutturato e rileggerlo sarebbe costato una chiamata per riottenere quel che c'era. Un testo incollato nella casella ha la precedenza e il bottone torna «Analizza», perché chi scrive lì vuole un annuncio nuovo* |
| **P5 Dialogo** | conversazione | pannello chat riusato per tre scopi: dialogo guidato del profilo, sessione di aggiornamento, brainstorming sull'opportunità; schede di conferma inline per i turni del profilo. Bolle a destra per l'utente e a sinistra per l'assistente, casella multiriga (**Invio manda, Maiusc+Invio va a capo**) e tre bottoni di scelta; l'attesa dell'AI **non è annullabile** — una mossa a metà lascerebbe il dialogo in uno stato che non esiste. Uscire non azzera il dialogo, che si riprende dov'era: ad azzerarlo è solo «Ricomincia». *(Sei decisioni di T3c, 2026-08-07.)* *Dalla revisione adversariale (2026-08-09): a dialogo concluso la fascia della risposta **si ritira** — niente zona morta sotto l'ultima bolla — e durante una chiamata si blocca anche la barra di navigazione (cap. 02.6)* *A **T7c** (2026-08-18) il pannello fa davvero i due mestieri che questa riga prometteva dall'inizio, e senza controlli nuovi: cambiano titolo, sottotitolo e i nomi dei tre comandi in fondo — «Torna alla candidatura» e «Trasforma in appunti» al posto delle porte del profilo — e in ogni momento **ne è vivo uno solo**. Le differenze del ragionamento stanno tutte in ciò che lo streaming si porta dietro: la bolla dell'assistente **cresce** mentre il testo arriva invece di comparire finita; «Invia» durante l'attesa **diventa «Interrompi»**, perché lì la mano sta già e c'è qualcosa da fermare (02.6); non ci sono schede né bottoni di scelta, perché non c'è nessuna macchina a mosse — si scrive e basta. «Trasforma in appunti» resta **spento finché l'utente non ha detto la sua**: distillare una conversazione in cui ha parlato solo l'AI costerebbe un'attesa per farsi rispondere una lista vuota. Un difetto trovato guardando e non misurando, il giorno stesso: i due comandi rinominati erano larghi quanto le etichette **di prima**, e a video si leggeva «Torna alla» — ora la misura segue il testo, senza scendere sotto quella del disegno* *Dal 2026-08-18 il pannello dice anche l'altra metà di quello che il cap. 02.5 gli affidava: una risposta fermata dal **tetto dei token** lascia a video il testo arrivato e sotto la riga «(fermata qui: ha raggiunto il limite di lunghezza)», gemella del «(interrotto)» che segue l'interruzione dell'utente — una frase che si spezza senza dirlo sembra una frase finita* |
| **P6 Documenti** | anteprima e rifinitura | anteprima del CV e della lettera affiancate all'annuncio (per il 📄 CV-1 base, generato senza annuncio, la colonna annuncio resta vuota); scelta lingua IT/EN; prima/dopo della rifinitura anti-slop; bottoni: «Esporta DOCX» «Esporta PDF» (L2), «Prepara email» (L3). *A T4 (2026-08-10) il pannello nasce **intero nella struttura e parziale nelle funzioni**: anteprime ed esportazioni funzionano, mentre la scelta di lingua e il prima/dopo dell'anti-slop sono lì spenti con il loro tooltip «arriva a T7», e «Prepara email» spento con «arriva a T6». È la regola 3.8 applicata al pannello che più di ogni altro mostra dove il progetto sta andando* *A T6 (2026-08-14) **«Prepara email» si accende** e porta a P7: restano spenti i due comandi di T7* *A T7a (2026-08-15) si accende la **tendina della lingua**, e a T7b (2026-08-18) la casella del **prima/dopo**, che si abilita quando c'è un confronto da mostrare — spuntarla mette in coda a ogni colonna i campi cambiati, prima e dopo, e non tocca ciò che si esporta (cap. 08.4). Il pannello non ha più comandi spenti in attesa di una tappa* *A T7d (2026-08-18) il pannello impara a **rileggere il 📄 CV-1 base da disco** invece di rigenerarlo a ogni visita: «rientrare non rigenera» valeva per le candidature e non per lui, e la conseguenza era che i due bottoni d'esportazione restavano spenti su un CV che esisteva — per riesportarlo bisognava rifarlo, e senza AI non si poteva affatto. Ora il pannello dice **di quando è** il CV che mostra e, se il profilo è cambiato da allora, lo dichiara invece di rifarlo di nascosto (cap. 11.1). Dalla stessa tappa la **tendina della lingua è accesa anche sul CV-1 base**, con la stessa semantica che ha sulla candidatura — cambiarla lo riscrive, previa conferma — perché la lingua è una proprietà del documento e il CV-1 base è un documento (cap. 10.1)* *A T9d (2026-08-22) compare **«Modifica i testi» (L4)**, fra «Rigenera» e i due export: apre una finestra con l'elenco dei campi di **prosa** — sommario, descrizioni delle esperienze, corpo della lettera — la casella per riscrivere quello scelto e il «Ripristina il testo non rifinito», acceso sui soli campi che l'anti-slop aveva cambiato. È l'ultima delle tre cose che il cap. 08.4 prometteva davanti al prima/dopo. **Le tre caselle del pannello restano in sola lettura**, e non è una rinuncia: mostrano la pagina di blocchi che finirà nel DOCX e nel PDF (cap. 05.3), non il documento — renderle scrivibili vorrebbe dire ricostruire il JSON da un testo impaginato che si porta dietro anche il prima/dopo. Si riscrive **solo la prosa scritta dall'AI**: i fatti — nomi, aziende, date, competenze, titoli — vengono dal profilo, e cambiarli qui li farebbe divergere in silenzio da chi li custodisce. Quel che si scrive entra nel documento al «Salva» e finisce **subito** su disco; da lì lo trovano gli export, l'email di P7 e i tool del server, che leggono lo stesso JSON. Il bottone è spento mentre l'AI lavora, come tutta la fascia, e su un documento che di prosa non ne ha* *Sempre a T9d (2026-08-22), dopo la prova dal vivo, i due **export chiedono dove salvare**: una scelta cartella che parte dal Desktop — e dalla seconda volta dall'ultima cartella usata — poi scrivono, e alla fine **aprono Esplora risorse** sul file appena fatto. Prima l'unico segno era una riga di testo in fondo al pannello e i file finivano nella cartella dati, che l'utente non aveva scelto e non sapeva trovare: premere «Esporta» sembrava non fare niente. I file continuano a nascere nella cartella della candidatura — di lì li prende P7 per allegarli — e in quella scelta ne va una **copia**; se lì ci sono già file con quel nome si chiede prima di sostituirli (cap. 05.6). Nella stessa passata **il prima/dopo della rifinitura è stato tolto del tutto** — la casella di qui e il «Ripristina» della finestra — perché su dati veri quel confronto mostra ritocchi che l'utente non distingue (cap. 08.4): un comando in più da capire, non una garanzia che si usa. Al posto liberato c'è la tendina **«Documento:»**, che è l'altra metà della voce «📄 Documenti» in barra* *E dopo la prova dal vivo quella tendina ha preso il posto che le tocca: era nata a destra della lingua, larga uguale e scritta uguale, ed è stata trovata «poco visibile» proprio mentre è **la porta da cui il pannello si usa**. Dal 2026-08-22 apre la fascia — etichetta «📄 Documento:» in grassetto d'accento, tendina larga 416 px col carattere dell'azione principale, dentro una **cornice d'accento** di 2 px — e la lingua le scala a destra. L'ordine è quello della lettura: prima **quale** documento si guarda, poi in che lingua. La fascia cresce da 40 a 48 px; la finestra ha una larghezza minima di 1150 px, quindi la lingua non esce mai dal bordo* *A **R7** (2026-08-23) la fascia dei comandi guadagna **«⚠ Rigenera la lettera» (L4)**, che non è un bottone spento in attesa: **c'è solo quando serve**, cioè quando il 🎯 CV-2 è stato riscritto a mano dopo l'ultima ✉️ lettera, e altrimenti non esiste affatto (3.3, «quel che non serve non deve nemmeno esserci»). È insieme la spia e il rimedio: chi lo vede sa che c'è un disallineamento e come si chiude. Perché potesse comparire e sparire senza lasciare buchi, la `FasciaDeiComandi` ha imparato che **un comando nascosto non occupa posto** — prima nessun pannello ne aveva, e la geometria dava per scontato che i bottoni dichiarati fossero tutti a video. Dalla stessa cura l'avviso di «Rigenera» **elenca i testi** riscritti a mano invece di nominarli in blocco, e non scade più cambiando pannello (cap. 08.4)* *A **R6** (2026-08-24) la finestra «Modifica i testi» prende un **secondo elenco**, e con lui il pannello decide anche **cosa** il documento dice, non solo come. A sinistra «Nel documento», a destra «Lasciate fuori», in mezzo «Togli →» e «← Rimetti». Un CV generato porta tutto quel che il profilo dichiara, ma un CV mandato a un'azienda non è l'inventario di una vita: tre esperienze su dieci a quell'annuncio non dicono niente, e chi si candida le vuole lasciare fuori — **senza toglierle dal profilo**, dove continuano a valere per le altre candidature (cap. 08.4). Un'esperienza è **una riga sola** che si riscrive e si toglie; il sommario e il corpo della lettera non si tolgono affatto, perché sono il documento e non una sua voce. Il bottone di P6 si accende adesso anche su un CV fatto di **soli fatti** — competenze e titoli, nessuna prosa — che prima non apriva niente. Due asperità trovate provandola sui dati veri e curate lì: un'intestazione di colonna tagliata e una barra di scorrimento orizzontale che non serviva a niente* *Lo stesso giorno, aprendo le rifiniture prima del giro D, altri due difetti della stessa finestra. Il segno **✎** conosceva solo le riscritture del **giro corrente**, mentre l'avviso di «Rigenera» — che legge il disco — ne conosceva anche di più vecchie: chi riapriva un documento riscritto ieri lo trovava senza segno, e intanto l'avviso continuava a promettere che l'avrebbe perso. Ora la finestra riceve, con ogni documento, i campi che vi risultano **già** riscritti a mano (R7, `RiscrittureAMano.Contiene`), e il segno vuol dire «questo l'hai scritto tu», in questo giro o in uno di prima — cosa poi rientri nel documento al «Salva» resta un'altra domanda, e la risposta è solo ciò che si tocca adesso (cap. 08.4). Il secondo difetto: la selezione nei due elenchi, che a ogni «Togli» e a ogni «Rimetti» si rifanno da capo, tornava in cima. Adesso la riga scelta si ritrova per **identità** — la stessa voce, che intanto è passata di elenco — e solo quando non c'è più la scelta cade su chi ha preso il suo posto, o sull'ultima rimasta; vale per tutti e due gli elenchi, perché chi rimette dentro le voci una per una lavora in quello di destra. Per poterlo collaudare la finestra ha smesso di chiedere a `SelectedItems` chi è scelto: quella strada risponde solo a elenco già nato, cioè al banco mai, e il primo collaudo scritto era verde senza provare niente* |
| **P7 Email** | composizione | destinatario, oggetto, corpo, elenco allegati (con quelli suggeriti dalla cartella documenti), bottone **«Prepara l'email»** (L3): scrive il file `.eml` e lo apre nel programma di posta predefinito. Al ritorno, la domanda «l'hai spedita?» per aggiornare il registro. *Nella 1.0 non esiste un bottone «Invia»: a spedire è il programma di posta dell'utente (cap. 07).* *Costruito a T6 (2026-08-14). Nella fascia dei comandi, a sinistra: «◀ Torna ai documenti» (L2), «Fallo riscrivere» (L2) e **«Documenti da allegare…»** (L2), che apre il giro della cartella documenti (cap. 05.2); a destra «L'ho spedita» (L1) e «Prepara l'email» (L3). Il destinatario resta **vuoto** finché non lo scrive l'utente. L'elenco «Cosa allego» mette prima i documenti generati per questa candidatura — il PDF già spuntato, il DOCX spento — e poi gli **attestati** della cartella documenti, spenti e marcati «(dai tuoi documenti)», perché in una lista sola convivono file nati un minuto fa e file che l'utente ha da anni. «L'ho spedita» è spento finché il messaggio non è stato preparato: un `.eml` che non esiste non può essere partito* *A T7b (2026-08-18) il pannello impara a dire una cosa in più: se la bozza ripresa è in una **lingua diversa** da quella dei documenti — succede cambiando la tendina di P6 dopo aver già preparato l'email — al posto di «Bozza ripresa da dove l'avevi lasciata» compare l'avviso, che nomina le due lingue e manda a «Fallo riscrivere». Il messaggio **resta quello di prima**: riscriverlo da sé cancellerebbe un testo che può essere già passato per le mani dell'utente, ed è la stessa ragione per cui una bozza salvata non viene mai sovrascritta all'arrivo (cap. 07.1)* *Dal 2026-08-18 la bozza si salva anche uscendo dalla **barra di navigazione**, e non più solo dal «◀ Torna ai documenti»: prima da lì si perdeva in silenzio (3.8). Mentre l'AI scrive non si salva niente — una bozza a metà messa sopra quella buona sarebbe peggio del male* |
| **P8 Impostazioni** | finestra separata | chiave API (mascherata), cartella dati, cartella documenti, modelli AI, lingua predefinita output, interruttore della rifinitura anti-slop (cap. 08.4), gestione del pool («Sigilla pool», dettaglio dei file modificati — cap. 04.5), export/import backup (cap. 11.4), pulizia dati («Svuota dati di navigazione», eliminazioni — cap. 11.5). *Niente sezione «account di posta»: l'app non spedisce.* *Resta di T9, ma due delle sue voci hanno già una casa provvisoria da T6: la **chiave API** si digita nella finestra del primo avvio e si rifà riavviando con `--chiave`, la **cartella documenti** si sceglie da P7. Quando il pannello arriverà, le assorbirà entrambe — e sarà lui il posto naturale, perché sono configurazione e non passi di un flusso* *A T9a (2026-08-21) anche la voce **export/import backup** ha trovato casa prima del suo pannello: la finestra c'è, si apre da P2, e quando P8 arriverà la **richiamerà** invece di rifarne una che le somiglia — come già la `FinestraConfermaCritica` fa con le eliminazioni* **Costruito a T9b** *(2026-08-21)*: la finestra c'è e il bottone della barra, spento da mesi, si accende. **Non ha OK né Annulla, ma un solo «Chiudi»**: le preferenze si scrivono appena si cambiano, perché ogni voce qui dentro si disfa con un secondo clic e uno stato «cambiato ma non salvato» sarebbe solo un tranello in più; le cose che invece non si disfano — le due pulizie — hanno la loro conferma prima di partire, che è dove la difesa serve. **Richiama e non rifà**, come il capitolo prometteva: la chiave passa dalla finestra del primo avvio, il backup da quella di T9a, l'eliminazione totale dalla `FinestraConfermaCritica`. Tre voci previste sono però cambiate di natura, e ognuna per una ragione sua: la **cartella dati** si mostra e si apre ma non si sposta (il lucchetto è preso all'avvio, cap. 09.4 — v. cap. 11.1); i **modelli** e il **pool** si leggono e basta, perché `modelli.json` esiste apposta perché cambiarli costi una riga e non una nuova build (cap. 11.6) e il manifest del pool si sigilla dal repo, non da un eseguibile distribuito (cap. 04.5) — *dei due, la prima è stata rovesciata il 2026-08-27: v. in fondo a questa riga*; la **cartella documenti** si vede qui ma si gestisce in **P7**, dove quel giro sa aspettare l'AI e annullarla (cap. 05.2) — le Impostazioni ci mandano, come P2 manda in P3 per l'import da un sito. Delle eliminazioni del cap. 11.5 P8 tiene le due **generali**; quella di una singola opportunità sta dove l'opportunità si guarda* *E il 2026-08-31 quella promessa è stata mantenuta: «Elimina candidatura» sta in **P1**, la coda — che è dove le opportunità si guardano tutte insieme — e non qui (cap. 11.5)* *A **T9c** (2026-08-21) si aggiunge la sezione **«Candidature spedite»**, con l'unica preferenza che quel giro chiedeva: «Ricordamele se non rispondono entro N giorni», quattordici di casa, **zero per spegnere** il promemoria (cap. 07.3). Sta fra i documenti e le cartelle perché è una preferenza come la lingua — si scrive appena si cambia, e vale subito: la Home la rilegge alla prima occhiata, senza riavvio**Il 2026-08-27, dalla revisione del giro D, P8 cambia in tre punti. I **modelli** smettono di essere di sola lettura: due tendine, una per livello, con l'elenco chiesto all'API e la scelta scritta in `modelli.json` (cap. 11.6) — il file resta la casa, ma non è più l'unica porta, perché chi il programma non l'ha scritto non apre un JSON. Nasce la sezione **«Quanto è costato»**, che legge il `chiamate_ai.csv` e dice chiamate, token e stima in dollari (cap. 13.11). E in cima, sotto la spiegazione, c'è il bottone che apre l'**informativa** (v. sotto). Il **pool** invece resta di sola lettura, per la ragione di sempre: si sigilla dal repo, non da un eseguibile distribuito.* |

Ogni pannello ha in alto il **titolo in `RossoTitoli`** e un sottotitolo grigio che dice
a che punto del flusso siamo («Passo 2 di 4 — Confronto»).

## 3.7 Naming dei controlli (per il designer)

Prefissi standard, nome semantico in PascalCase: `pnl` (Panel), `btn` (Button),
`lbl` (Label), `txt` (TextBox), `cmb` (ComboBox), `chk` (CheckBox), `pic` (PictureBox),
`grp` (GroupBox), `lst`/`lvw` (liste), `wv` (WebView2), `cms` (menu contestuale),
`tmr` (Timer), `num` (NumericUpDown, da T9c). Esempi: `btnCatturaAnnuncio`, `lblStelleMatch`, `wvRicerca`,
`pnlLogo`, `lblVersione`.

## 3.8 Feedback e stati

- **Badge di stato** (pannellino 115×26 con etichetta bold centrata): verde OK, azzurro
  info, giallo attenzione, rosso errore — usati per lo stato delle opportunità e delle
  chiamate AI.
- Operazioni AI in corso: indicatore nella barra di stato — che **compare per questo** e
  se ne va appena finito — + testo in streaming dove previsto (cap. 02); mai una
  finestra bloccata.
- **L'indicatore dell'attesa, al centro dello schermo** *(dal 2026-08-30; **neutro** dal
  2026-09-01)*. I due segnali di prima erano entrambi
  **piccoli**: la rotellina del puntatore, che si vede solo se il mouse è sopra la
  finestra e lo si sta guardando, e la riga che si muove in fondo alla fascia di stato,
  che è alta due righe in un angolo. Un'attesa di trenta secondi va detta anche a chi
  guarda da lontano, o si è alzato: mentre l'AI lavora compare perciò, **al centro dello
  schermo**, una **ruota di dodici pallini d'argento** che gira — quello che si è sempre
  visto girare, disegnato invece che animato: i pallini stanno fermi sul loro cerchio, e a
  girare è quale di loro è acceso — con sotto la barra che si riempie.
  - **Senza marchio** *(2026-09-01)*. Fra il 30 agosto e questa data sotto la ruota c'era
    lo **stemma Aviolab**, grande un terzo dello schermo, e questo punto si chiamava «lo
    scudo di caricamento». È stato tolto: il marchio vive nel solo pannello del logo
    (3.5), e un'attesa non è il posto in cui firmarsi — chi guarda sta aspettando, non
    ammirando. Lo stemma qui faceva due danni: prendeva il centro dello schermo con
    qualcosa che dell'attesa non dice niente, e obbligava la ruota a stargli addosso, dove
    i pallini d'argento dovevano difendersi dal blu, dal bianco e dal giallo delle stelle.
    Adesso la ruota sta sul suo sfondo e basta. *(Nel codice il nome della classe è rimasto
    quello dei giorni dello stemma: rinominarla toccherebbe finestra, banco e finestra
    principale, ed è un giro a parte.)*
  - **La misura**: al massimo **due decimi** dello schermo in orizzontale e **un quarto**
    in verticale. Sono due **limiti**, non due misure: la figura ha le sue proporzioni — la
    ruota è tonda, e la barra le sta sotto larga quanto tutto — e si prende la più grande
    che ci sta dentro tutti e due. Su uno schermo comune, 1920 × 1080, fanno **316 × 269**.
    *Il limite verticale era **due sesti** finché misurava il solo stemma, e il complesso lo
    sforava perché la barra si aggiungeva dopo (297 × 397 contro i 360 dichiarati). Adesso
    vale per **tutto quel che si vede**, ed è sceso a un quarto per una ragione precisa:
    tolto lo stemma la figura si è abbassata di molto, e col vecchio limite sarebbe stata la
    sola larghezza a comandare — l'indicatore sarebbe cresciuto di un terzo, e con lui la
    barra che era stata tarata guardandola.*
  - **Compare solo se l'attesa è un'attesa** *(2026-09-01)*: la finestra si apre dopo
    **300 ms** di lavoro, e se la chiamata si chiude prima non si vede affatto. Non tutte le
    chiamate all'AI durano mezzo minuto, e una figura grande un quarto dello schermo che
    compare e sparisce in due decimi di secondo non si legge come «sto lavorando»: si legge
    come un lampo, cioè come un difetto. Trecento millisecondi sono la misura sotto la quale
    chi guarda non ha ancora fatto in tempo a chiedersi se il programma abbia sentito il
    clic. Spegnere invece è immediato. Una soglia **già in corsa** non si riavvia, per la
    stessa ragione per cui la barra non ricomincia da zero (v. sotto): chi accende
    l'indicatore è la riga che spegne la barra di navigazione, e quella passa di lì più
    volte nella stessa attesa.
  - **Niente riguardo per la preferenza di sistema «riduci il movimento»**, ed è una
    decisione e non una dimenticanza *(2026-09-01)*: la ruota è il **solo** movimento
    dell'applicazione — non ci sono transizioni, comparse, scorrimenti animati — ed è anche
    l'unica cosa che dice che il programma non è bloccato. Onorare la preferenza qui
    vorrebbe dire spegnere l'unico segnale d'attesa per chi l'ha attivata, o inventare per
    lui un secondo indicatore fermo: due strade che costano più di quel che rendono su un
    movimento lento, piccolo e senza lampeggi.
  - **Non ruba niente**: non prende il fuoco, non compare in Alt-Tab e soprattutto **non
    intercetta i clic** — mentre l'AI lavora il bottone «Annulla» dev'essere premibile, ed
    è proprio lì sotto. È una finestra a strati (*layered*), non un controllo, perché il
    disegno ha i bordi morbidi e il fondo trasparente: appoggiarlo su una finestra normale
    lascerebbe attorno l'alone del colore reso invisibile.
  - **Sta sopra lo schermo di chi guarda**, non «sul primo»: con due monitor il centro
    dello schermo è il centro di quello dove sta la finestra principale.
  - **Venti pixel più in alto del centro.** Non è la correzione di un conto sbagliato — il
    centro geometrico è esatto — ma di come lo legge l'occhio: una figura appesa
    esattamente a metà di un rettangolo sembra cadere verso il basso. L'alzata vale per il
    **complesso** — ruota e barra insieme: si sposta la finestra, non il disegno dentro di
    lei, o la barra scivolerebbe addosso alla ruota. *Il numero è stato **rifatto** quando
    lo stemma se n'è andato, non lasciato dov'era. I trenta pixel chiesti da Mirco il
    2026-08-31 guardando a video valevano per un complesso alto 397 px, ed erano il 7,6%
    della sua altezza; quello di oggi è alto 269, e la stessa frazione fa venti. Che venti
    sia anche il numero scelto la prima volta, quando sotto lo stemma non c'era ancora la
    barra, è una conferma e non la ragione. Resta in pixel e non in frazione perché è un
    numero deciso **guardando**, e va riguardato: il conto dice da dove ripartire, non che
    sia giusto.*
  - **La barra che si riempie** *(dal 2026-08-31, chiesta da Mirco)*. Sotto la ruota,
    **larga quanto tutto l'indicatore** — la ruota no, ed è quel che le dà un principio e
    una fine — spessa il 7,4% di quella larghezza, **ventitré pixel** su uno schermo
    comune, e staccata dal piede della ruota del 7%: misure in frazione e non in
    pixel, o su un monitor 4K sarebbero un filo di capello. *(Lo spessore è passato dal
    5,5% al 6,5% al 7,4% nella stessa sera del 31 agosto, guardando la barra a video:
    quanto dev'essere spessa perché si veda da lontano non lo dice nessun conto. Che
    resti un pixel sopra lo stacco va bene — quel che serve è che dell'aria ci sia, non
    che vinca il confronto.)* I colori sono quelli della barra di avanzamento di sistema,
    campionati pixel per pixel dall'immagine che Mirco ha portato: corpo `#0D7C0D`, punta
    `#34A936` sull'ultimo ottavo (quel che nella barra vera sembra un effetto
    fluorescente, ed è invece la parte che si vede muoversi), fondo `#E4E6E6` col suo
    filetto. È un prestito consapevole da fuori tavolozza: una barra che si riempie è un
    segno che si legge **senza impararlo**, e dipingerla del blu Aviolab l'avrebbe resa
    una striscia da interpretare.
  - **Perché è dipinta e non è un `ProgressBar`**: questa finestra non ha controlli, è
    un'unica immagine col suo alfa consegnata a Windows in un colpo solo, e un figlio
    dentro non finirebbe mai in quell'immagine.
  - **Avanza col tempo, e non arriva mai in fondo da sola.** Quanto durerà una chiamata
    all'AI non lo sa nessuno — venticinque secondi un'analisi, trentacinque un confronto,
    cinquantasette un CV con la lettera, il doppio su una rete lenta — e nemmeno lo
    streaming lo saprebbe: il numero di parole che verranno non è noto a chi le sta
    scrivendo. La barra dice perciò l'unica cosa che può dire senza mentire: **più tempo
    passa, meno ne resta in proporzione**. Cresce in fretta all'inizio e rallenta, e si
    ferma al **95%** (un terzo a cinque secondi, due terzi a quindici, l'84% a
    trentacinque, il 92% a un minuto — i numeri che Mirco ha scelto vedendoli scritti).
    L'ultimo 5% lo riempie il **fatto**: l'AI ha risposto. Allora la barra scatta a uno,
    **resta piena tre battiti** — un quarto di secondo scarso, o quel compimento non lo
    vedrebbe nessuno — e sparisce con la ruota.
  - **Un'attesa già in corso non fa ricominciare la barra.** Chi accende l'indicatore è la
    stessa riga che spegne la barra di navigazione, e quella passa di lì più volte nella
    stessa attesa: con la sola ruota non si vedeva — un pallino vale l'altro — mentre una
    barra che torna a zero a metà strada non verrebbe più creduta.
  - Il filo che lo accende è **uno solo** — lo stesso che spegne la barra di navigazione
    (cap. 02.6) e fa parlare la fascia di stato — così i tre segnali non possono
    smentirsi a vicenda. *E dal **2026-09-01** quel filo raccoglie anche l'**import del CV**
    in P2, che era l'ultima chiamata all'AI a passare per conto proprio: lì la lettura di un
    PDF può durare quanto un confronto, e l'unico segno era il cursore che cambia forma.
    Un filo unico che ne lascia fuori uno non è un filo unico: è un filo con un'eccezione,
    e l'eccezione è sempre quella che si dimentica.*
- **Un'attesa lunga si annulla dal comando che l'ha avviata** *(2026-09-01)*. Vale ora anche
  per P6 e P7, che erano rimasti gli ultimi a farsi aspettare senza via d'uscita: mentre
  l'AI scrive, «Rigenera» e «⚠ Rigenera la lettera» in P6 e «Fallo riscrivere» in P7
  diventano **«Annulla»** e fermano il lavoro. È il modo di P4 e di P2, e la ragione per cui
  l'annulla sta **in quel** bottone e non in uno nuovo è che lì la mano sta già: chi ha
  appena premuto guarda ancora il punto in cui ha premuto. Resta fuori il turno del dialogo
  guidato (P5), per la ragione di sempre — lì si interromperebbe una **mossa** della
  macchina, non una risposta (cap. 02.6).
- Errori: messaggio in italiano nel contesto in cui è avvenuto (non solo un popup),
  sempre con un'azione possibile («Riprova», «Salta»). *Dal 2026-08-09 c'è anche
  l'ultima rete: un gestore globale delle eccezioni in italiano (in `Programma`),
  perché nemmeno l'imprevisto vero deve mostrare la finestra di crash di .NET.*
- **Un guasto porta la parola, non solo il colore** *(2026-09-01)*. Le righe di stato che
  dicono che qualcosa non va cominciano con **«Errore — »** o **«Attenzione — »**, e le due
  parole stanno in un posto solo (il modulo `Segnalazioni`), portate nelle righe dai metodi
  che mettono insieme parola e colore in un gesto unico — così non esiste il caso di una
  riga rossa senza parola. Prima il guasto si distingueva dallo stato per il **solo colore**,
  e in P3 nemmeno per quello: là tutto finiva nel grigio delle didascalie, e «Non sono
  riuscita a salvare le ricerche» pesava quanto «Ricerca salvata». Un colore che porta da
  solo un'informazione è ciò che WCAG 1.4.1 chiede di non fare, e la ragione non è formale:
  c'è chi i due rossi non li distingue, chi legge con un lettore di schermo, chi guarda lo
  schermo di traverso al sole.
  Le **parole sono due** perché le righe rosse non raccontano tutte lo stesso fatto. Alcune
  dicono che qualcosa **è andato storto** — un file che non si scrive, l'AI che risponde
  male, una pagina che non si legge — e quelle sono errori. Altre dicono che qualcosa
  **manca o non torna** prima ancora di provare: la chiave API che non c'è, un backup
  rimesso solo in parte, una bozza scritta in un'altra lingua. Chiamare «Errore» la mancanza
  della chiave sarebbe una bugia — non darla è una risposta legittima (cap. 11.3) — e dare
  la stessa parola a due fatti diversi la svuota. Il colore resta lo stesso: per l'occhio la
  gravità non cambia, cambia di che cosa si parla. *E sono parole e non icone per la lezione
  già pagata dalla barra (3.4): i controlli li disegna GDI, che le emoji a colori non le sa
  fare, e un lettore di schermo legge una parola invece di annunciare «segno di attenzione».*
- **I bottoni delle tappe non ancora arrivate si vedono, spenti, con un tooltip che dice
  in che tappa arriveranno** — non si nascondono. Chi guarda l'applicazione a metà strada
  deve capire dove sta andando, e un bottone che sparisce e ricompare è più confondente di
  uno spento che si spiega. Stessa regola per ciò che manca all'avvio: senza chiave API il
  bottone del dialogo è spento e il motivo è scritto, non lasciato indovinare (cap. 02.5).
  *(Deciso in T3c, 2026-08-07.)*
- **Il motivo per cui un bottone è spento si scrive sotto quel bottone**, non altrove
  nella schermata. *(Imparato in T4c, 2026-08-10.)* Il motivo per cui «Analizza» non si
  poteva premere c'era, ed era corretto — ma stava nell'angolo opposto dello schermo,
  dove nessuno guardava: chi voleva premere il bottone stava guardando il bottone, e
  l'applicazione sembrava rotta. Una spiegazione che non si trova non è una spiegazione.
- **Un contatore concorda la parola col numero.** *(Imparato in T5c, 2026-08-13.)* I
  contatori della Home dicevano «1 scartate», perché il numero era una variabile e la
  parola no. Il caso «uno» sembra il meno importante — è quasi sempre più di uno — ma
  capita proprio la **prima volta** che l'utente fa una cosa, cioè quando guarda con più
  attenzione, e una stonatura lì fa sembrare improvvisato tutto il resto.
- **Un pannello che torna in vista rilegge le proprie condizioni.** *(Imparato in T4c,
  2026-08-10.)* I pannelli dell'area centrale nascono all'avvio e restano vivi, nascosti,
  per tutta la sessione: ciò che hanno chiesto al motore la prima volta può non essere più
  vero. È successo con P4, che continuava a dire «prima serve il tuo profilo» a un
  profilo appena importato e salvato — proprio sul percorso del primo avvio, dove quel
  messaggio è l'unica cosa che l'utente legge.
- **E un pannello che cambia soggetto si svuota, non si limita a riempirsi.**
  *(Imparato il 2026-08-18.)* È il gemello della voce qui sopra, con un difetto peggiore
  dentro: P7 arrivava su una candidatura nuova e scriveva oggetto e corpo **solo quando
  l'AI glieli dava**, ma quella scrittura ha due uscite anticipate legittime — manca la
  chiave, manca la lettera. In quei due casi restava a video il messaggio della
  candidatura **di prima**, sotto il nome di questa, e finiva nel suo `email.json` al
  primo salvataggio. Chi arriva azzera **tutto** ciò che riempirà a condizione, non solo i
  campi che sa già di dover cambiare: un campo lasciato com'era non è «vuoto», è pieno di
  roba di qualcun altro.
- **Un pannello che si lascia mette al sicuro quello che ha in mano.** *(Imparato il
  2026-08-18.)* Da un pannello si esce in due modi: dai **suoi** bottoni, che lui
  controlla, e dalla **barra di navigazione** in cima, che non passa da lui. Chi salvava
  solo sul primo perdeva tutto sul secondo, e **in silenzio**: la volta dopo il pannello
  rilegge il disco e mostra la versione vecchia come se fosse l'ultima. L'ha insegnato la
  bozza dell'email in P7 — destinatario scritto a mano, spunte degli allegati e un
  messaggio riscritto che era costato una chiamata all'AI. Ora la finestra principale,
  prima di nascondere un pannello, chiede a chi ha del lavoro in sospeso di metterlo al
  sicuro (`IPannelloCheSalvaUscendo`), e a dichiararlo è **solo chi ce l'ha** — oggi il
  solo P7 — perché un metodo vuoto ripetuto in sei pannelli non direbbe a nessuno chi
  lavora davvero. Non è la conferma d'uscita del cap. 11.5: lì si domanda e si può
  rispondere di no, qui non si chiede niente — quel che l'utente ha scritto è suo e si
  salva.
