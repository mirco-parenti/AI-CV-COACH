# Server MCP di collaudo

*Gli attrezzi con cui l'assistente prova l'applicazione da sé: compila, fa girare il
banco, avvia TrovaLavoro, la guarda in faccia con una schermata e le preme i bottoni.
Serve a rispondere con i fatti a domande come «perché questo bottone non fa niente?»,
invece che con un'ipotesi.*

Nato il **2026-08-10**, durante T4c, il giorno in cui «i bottoni non fanno nulla» si è
rivelato «i bottoni erano spenti, e il motivo era scritto all'angolo opposto dello
schermo»: una cosa che nessun collaudo del banco poteva dire, perché il banco vede lo
stato dei controlli, non come si vedono.

## Cos'è, e cosa non è

- **È** un server MCP locale, su `http://127.0.0.1:3300/mcp`, con una manciata di
  attrezzi **scritti uno per uno**. Nessuna dipendenza npm: Node ≥ 20 e basta, come il
  prototipo.
- **Non è** un'esecuzione di comandi arbitrari: se serve un attrezzo nuovo, lo si
  aggiunge a `server.mjs`. È il patto di questo strumento — i comandi si accumulano
  mentre il progetto cresce, e restano ripetibili.
- **Non è** parte del prodotto: sta fuori da `VB.NET/`, non entra nell'eseguibile e non
  va distribuito.

## Come si accende

```bash
node strumenti/mcp-collaudi/server.mjs      # resta in ascolto sulla 3300
```

Claude Code lo trova già registrato in `.mcp.json` (in radice), ma **lo carica
all'avvio**: se il server nasce a sessione già aperta, gli attrezzi compaiono solo dopo
un riavvio di Claude Code. Nel frattempo si può parlare al server con `curl`, che è
esattamente quello che ha fatto l'assistente il primo giorno:

```bash
curl -s -X POST http://127.0.0.1:3300/mcp -H 'content-type: application/json' \
  -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'
```

**Dopo aver toccato `server.mjs` il server va riacceso**, altrimenti continua a
rispondere col codice di prima — e si finisce per diagnosticare una modifica che non è mai
entrata in servizio. Gli **script `.ps1`, invece, no**: il server li lancia da capo a ogni
chiamata, e una modifica a `interfaccia.ps1` o a `schermata.ps1` è viva al comando
successivo. *(Verificato il 2026-08-29 curando `clic`: le prove giravano su un server
acceso da prima, e le modifiche si vedevano subito. Quel che il riavvio serve a rileggere
sono le **descrizioni** degli attrezzi, che stanno in `server.mjs`.)* Il server si spegne
**dalla porta**, non dal nome:

```bash
fuser -k 3300/tcp && node strumenti/mcp-collaudi/server.mjs &
```

Cercarlo per nome (`pkill -f mcp-collaudi/server.mjs`) sembra più naturale e invece è una
trappola: il pattern compare anche nella riga di comando che lo sta cercando, così il
comando **uccide sé stesso** prima di arrivare al server.

## Su quale eseguibile si prova

Dal **2026-08-30** uno solo: `TrovaLavoro.exe` **sul Desktop**. È un file unico e autonomo
(il runtime .NET dentro, gli stessi parametri del rilascio, cap. 13.2) che `compila` rifà
in una manciata di secondi chiamando [`../aggiorna-riferimento.bat`](../README.md).

Perché la differenza conta: prima si provava la build di `bin/Release`, e Mirco — a
sessione chiusa — apriva un'altra cosa. Due file omonimi, due versioni possibili, e niente
che lo dicesse. Adesso **quel che si prova qui e quel che apre lui sono lo stesso file**, e
l'eseguibile porta dentro di sé il commit da cui nasce (`+modificato` se l'albero di lavoro
era sporco), che si legge in «Informazioni su…».

Un effetto collaterale gradito: la compilazione intermedia va in `%TEMP%`, non in
`bin\Release`. Quel file lì lo tiene bloccato il server MCP del **prodotto**, ed era il
motivo per cui `compila` non si poteva chiamare senza sacrificare i tool della sessione.
Adesso si può.

## Gli attrezzi

| Attrezzo | Cosa fa |
|---|---|
| `compila` | Rifà l'**eseguibile di riferimento sul Desktop** — un file solo, autonomo, dall'ultimo codice — e restituisce gli errori del compilatore. Chiude prima il riferimento, se è aperto (il server MCP del prodotto resta vivo). Se fallisce, sul Desktop resta quello di prima. |
| `collaudi` | Fa girare il banco (`dotnet test`, Release); con `filtro` ne esegue solo una parte. **Non chiude niente**: compila in `banco/` invece che in `bin/`, così convive con l'applicazione aperta e col server MCP del prodotto. |
| `avvia_app` | Avvia il **riferimento sul Desktop** con la chiave API presa dal `.env` del prototipo. Con `dati` lo fa partire su una **cartella usa-e-getta** (`--dati`), che è il modo di provare ciò che cancella senza toccare i dati veri. |
| `stato_app` · `chiudi_app` | Se è viva; e la chiude — **solo le finestre**, mai il server MCP del prodotto, che ha lo stesso nome di processo. |
| `schermata` | Riprende la finestra dell'applicazione (o tutto il desktop) e restituisce il PNG. Se non riesce a portarla davanti lo **dichiara**: la fotografia riprende quel rettangolo di schermo, e potrebbe ritrarre la finestra che le sta sopra. |
| `ridimensiona` | Cambia la misura della finestra, o la rimette massimizzata. È il modo di guardare i difetti di impaginazione che si vedono **solo stretti**. |
| `controlli` | Elenca bottoni, caselle e schede dicendo per ciascuno se è **acceso o SPENTO**; dei menù a tendina dice anche **la voce che mostrano**, e marca `[pagina]` quel che è del sito aperto nel browser. |
| `clic` · `scrivi` | Preme un controllo per etichetta; scrive in una casella. Se il controllo è spento — o la casella è di sola lettura — lo dichiara invece di fingere. **Prima di muovere il puntatore pretende due cose**: il primo piano *verificato*, e che il pixel da premere appartenga all'applicazione (non fuori schermo, non coperto). Se una manca, dice che **non** ha premuto e perché. |
| `scegli_voce` | Sceglie una voce in un menù a tendina — il portale in «Cerca su» — aprendolo e cliccandoci dentro come farebbe una persona, e poi verificando che il menù la mostri davvero. Senza la voce, le **elenca**. Stesse due pretese di `clic` prima di ogni colpo di mouse. |
| `scegli_riga` | Sceglie una riga di una lista — la coda delle candidature in P1 — cercandola per un pezzo di quel che c'è scritto **in una qualsiasi delle sue celle**, e verificando poi che risulti scelta. Se la finestra ha **più liste** si dice quale con `lista`: un pezzo del suo nome («Lasciate fuori») o il numero che dice `controlli`; senza, cerca in tutte e si ferma se il testo combacia in più d'una. Una riga **oltre la piega** la porta in vista prima di premerla. Senza il testo, le **elenca** — tutte, lista per lista; con `doppio`, fa il doppio clic (che nella coda apre la candidatura). Stesse due pretese di `clic` prima di ogni colpo di mouse. |
| `rispondi_finestra` | Risponde a una finestra di messaggio premendo il bottone per nome («Sì», «No», «OK»). Senza il bottone **legge cosa chiede** e quali scelte dà, così si sa cosa si sta per confermare. |
| `scegli_file` | Risponde alla finestra di scelta file che l'applicazione ha aperto: il file da prendere (anche in forma `/mnt/c/…`), oppure `annulla`. |
| `cartella_dati` | Cosa l'applicazione ha scritto su disco: profilo, storico, opportunità, documenti. |
| `aspetta_che` | Aspetta una condizione invece di alternare `clic` e `controlli` a mano. Due modalità: lo **stato** di un controllo (`nome` + `stato` «acceso»/«spento», con il loop dentro un'unica PowerShell) oppure il **contenuto** di un file (`file`, con `contiene` o senza — basta che compaia o cambi), puro Node. |

Con questi, il **giro completo** si percorre senza mani: importare un CV, salvare il
profilo, incollare un annuncio, analizzarlo, generare CV e lettera, esportarli. È stato
fatto per la prima volta il 2026-08-10. Dal **2026-08-12** ci si aggiunge la ricerca:
scegliere un portale nel menù, scrivere cosa e dove, premere «Cerca» e guardare la pagina
che arriva — il giro con cui si è chiuso il buco dichiarato di T5a, su tutti e quattro i
portali. Con **T5c**, lo stesso giorno, arriva anche la Home: scegliere una candidatura
nella coda e riaprirla, e rispondere alle conferme — la prima cosa provata con
`rispondi_finestra` è stato uno «Scarta» a cui si è risposto **No**, che è il modo di
collaudare un comando distruttivo su dati veri senza distruggere niente.

## Quel che ancora non sa fare

- **Le voci di un menù contestuale non si premono.** *(2026-08-21, T9c.)* Il menù di
  «Com'è andata…» in P4 si apre benissimo con `clic` sul bottone, le sue voci compaiono
  nella fotografia — e `clic` su «Colloquio» risponde **«Premuto»** senza che succeda
  niente: il menù resta aperto, la spunta dov'era, il disco intatto. Due volte di fila, e
  nemmeno la tastiera aiuta, perché il fuoco non è dell'applicazione. Costa un'ora se non
  lo si sa, perché tutto punta a un gestore che non parte — e invece parte: a dimostrarlo
  è bastato un **clic vero del mouse** alle coordinate della voce, e l'esito è finito su
  disco al primo colpo. Finché non gli si insegna: le voci si premono così
  (`SetCursorPos` + `mouse_event` da PowerShell, coordinate lette sulla fotografia a
  schermo intero), e il filo fra la voce e l'azione si copre nel **banco**, dove una
  `ToolStripMenuItem` ha `PerformClick`.
- **Il valore di un NumericUpDown non si legge né si scrive.** *(2026-08-21, T9c.)* La
  casella dei giorni in P8 compare nell'elenco come due bottoni «SU» e «GIÙ», per giunta
  marcati **SPENTI**, e il numero non si vede da nessuna parte: `scrivi` non ha dove
  scrivere. Si fa come sopra — clic vero sulla casella e `SendKeys` per il valore — e si
  verifica il risultato leggendo `impostazioni.json`, che è la fonte.
- **I bottoni-scelta non compaiono nell'elenco.** *(2026-08-21, T9a.)* `controlli` cerca fra
  i tipi che servono — bottoni, caselle, tendine, liste, schede — e i **RadioButton** non
  sono fra quelli: la finestra di backup mostra le sue due scelte («solo il profilo» /
  «tutto») e l'elenco non le nomina, così non si sa quale delle due è spuntata né la si può
  cambiare con `clic`. Finché non gli si insegna, quella parte si guarda con una
  fotografia — che è esattamente come è stata provata la finestra — e si collauda nel banco,
  dove la scelta ha un metodo suo.
- **Il contenuto delle caselle di testo non si legge.** L'elenco dice la voce dei menù,
  ma non cosa c'è scritto in una casella: per leggere l'indirizzo del browser serve
  ancora una fotografia.
- **Non sa scorrere dentro una casella.** *(2026-08-18, il collaudo di T7b.)* Quel che sta
  sotto la piega non si vede e non si legge: il prima/dopo della rifinitura in P6 compare in
  **coda** alle colonne, e della sezione si è potuta fotografare solo l'intestazione. Il
  confronto campo per campo è stato fatto sui file JSON — che è la fonte giusta per i
  contenuti — ma «si legge bene?» resta una domanda a cui questo attrezzo non sa rispondere,
  e infatti è rimasta in `in_sospeso.md`.
- **Le fotografie non si confrontano fra loro.** Non c'è un «com'era prima»: due
  schermate della stessa finestra si guardano una dopo l'altra, e a dire se qualcosa si è
  spostato è l'occhio di chi legge. Per i difetti di geometria conviene perciò accompagnare
  la fotografia con un collaudo del banco che misuri la stessa cosa in numeri.
- **La finestra di scelta *cartella* non si pilota.** *(2026-08-14, T6.)* `scegli_file`
  sa rispondere alla scelta di un file — cerca la casella `Edit` 1148 — ma la finestra che
  chiede una **cartella** (P7, «Documenti da allegare…») quella casella non ce l'ha: ha una
  casella «Cartella:» con un altro identificativo, e i bottoni si chiamano «Selezione
  cartella» e «Annulla». `rispondi_finestra` la **legge** (dice cosa chiede e che bottoni
  ha) ma non sa scriverci dentro, e UI Automation non la vede nemmeno come finestra —
  stessa storia della scelta file. Finché non le si insegna, la cartella si registra
  scrivendo a mano `documenti.json` nella cartella dati e si prova il resto del giro con
  «Fai rileggere la cartella»: è così che T6 è stata collaudata.

- **A DPI alto le fotografie escono virtualizzate.** *(2026-08-23, il giro B del collaudo
  di T9e.)* `schermata` non si dichiara DPI-aware, e Windows le consegna il desktop già
  rimpicciolito: con lo schermo al 150% ha reso un PNG di **1295×687** di una finestra che
  a video misurava **1942×1030** pixel veri. Su un'immagine così un testo un po' sgranato
  somiglia moltissimo a un testo mal disegnato, e si finisce per annotare difetti che non
  esistono — o per non vedere quelli che ci sono. Finché non impara a dichiararsi
  DPI-aware prima di premere il grilletto, a un DPI diverso da 96 le misure si prendono con
  un attrezzo indipendente: per il giro B sono bastati quattro script PowerShell
  usa-e-getta che chiamano `SetProcessDPIAware()` e leggono i rettangoli da UI Automation.

- **`schermata` può fotografare la finestra sbagliata, e non lo dice.** *(2026-08-27,
  provando le tre voci finali del giro D.)* L'applicazione era viva — `controlli` ne
  elencava i bottoni, uno per uno, compresi quelli di una finestra modale appena aperta —
  e `schermata` ha restituito tre volte di fila **il terminale in primo piano**, con la
  didascalia «Finestra ripresa: 1936 × 1048», che è la misura del terminale e non quella
  dell'applicazione. Nemmeno `schermo_intero` mostrava TrovaLavoro: la finestra c'era ma
  stava sotto altre. Il guaio è che l'esito **sembra buono** — arriva un PNG, con tanto di
  misure — e chi guarda deve accorgersi da sé che quella non è l'applicazione. Due modi per
  non cascarci: **guardare la didascalia**, perché una misura che non somiglia a nessuna
  finestra dell'app è già la risposta, e **fidarsi di `controlli`**, che legge da UI
  Automation e in quella sessione diceva la verità — è così che si è potuto verificare, senza
  vedere niente, che l'informativa compare al primo avvio e non al secondo. Quando le due
  cose si sommano — questa e la voce qui sopra sul `clic` — lo strumento resta capace di
  **interrogare** l'applicazione e non più di **guardarla** o di **premerla**: allora si
  dichiara la riserva e si passa la mano a chi ha gli occhi davanti allo schermo, invece di
  chiamare fatta una prova che non si è fatta.

- **A DPI alto `clic` dichiara «Premuto» senza aver premuto.** *(2026-08-23, il quinto tempo
  di T9e.)* È la sorella cattiva della voce qui sopra, e va saputa prima di fidarsi di un
  esito. Con lo schermo al 150%, `clic` su «Svuota i dati di navigazione» ha risposto
  **«Premuto »** — e non era successo niente: nessuna finestra di conferma, nessun file
  toccato. Non è la trappola nota del clic che finisce a chi ha il fuoco: quella si vede,
  perché qualcos'altro reagisce. Qui lo strumento **riferisce un successo che non c'è stato**,
  e chi legge la risposta annota «bottone premuto, non succede nulla» — cioè un difetto del
  prodotto che il prodotto non ha. Ci si è arrivati solo perché la conferma attesa non
  compariva; se il bottone non avesse avuto una conferma, il falso reperto sarebbe finito nel
  registro. **Come si fa invece**: si trova il bottone con UI Automation da un processo
  dichiarato DPI-aware, si porta davanti la sua finestra, si calcola il **centro** del suo
  rettangolo e si clicca lì con `SetCursorPos` + `mouse_event`. Così il bottone è stato premuto
  al primo colpo. Alla conferma, invece, `rispondi_finestra` ha risposto benissimo: **legge e
  preme via UI Automation**, e il DPI non lo tocca. La regola pratica è quella: a DPI diverso
  da 96, degli attrezzi che muovono il puntatore non ci si fida, di quelli che passano da UI
  Automation sì. **Nota del 2026-08-29**: le due guardie nuove di `clic` (primo piano
  verificato, pixel che appartiene all'applicazione) **non** coprono questo caso. A DPI alto
  le coordinate virtualizzate cadono quasi sempre dentro la finestra lo stesso, solo su un
  altro punto: `WindowFromPoint` risponde «è tua» e il colpo parte, addosso a un altro
  controllo. Qui l'unica difesa resta quella scritta sopra — a DPI diverso da 96, degli
  attrezzi che muovono il puntatore non ci si fida.

## Le trappole già pagate

- **Un argomento che passa da `bash -lc` non è un argomento: è codice.** *(Trovata dalla
  revisione di sicurezza della finalizzazione — rilievo R4 — e curata il 2026-09-01.)*
  Ogni comando di questo server si eseguiva così: si **componeva una stringa** e la si dava
  a `spawn("bash", ["-lc", …])`. Dentro quella stringa i chiamanti interpolavano anche gli
  argomenti degli attrezzi — il `filtro` di `collaudi`, la cartella `dati` di `avvia_app`, il
  `percorso` di `scegli_file` e di `cartella_dati`, il `file` di `aspetta_che` — protetti da
  virgolette doppie e nient'altro. Ma le virgolette non sono una difesa: basta che il valore
  ne contenga una per uscirne, e da lì il resto è una riga di shell come tutte. Falsificato
  proprio così, sul sito di `cartella_dati`: chiedendo la cartella
  `/tmp"; touch /tmp/PROVA-INIEZIONE-R4; echo "` la forma vecchia **crea il file**, e non lo
  dice a nessuno; la nuova risponde «La cartella dati non esiste ancora» e lascia l'argomento
  dov'era. È un server di sviluppo, in ascolto solo su `127.0.0.1` e senza autenticazione:
  il rischio vero è basso, ma il modo di scrivere queste chiamate era sbagliato lo stesso, e
  chi copia il codice di un attrezzo per farne uno nuovo si porta dietro il difetto.

  **Adesso di bash non ce n'è più nemmeno uno**: `esegui` vuole il programma e i suoi
  argomenti in un **array**, e li dà a `spawn` così come sono — un `;` o un backtick dentro
  un valore resta un carattere qualunque. Le tre cose che la shell faceva davvero hanno
  ciascuna la sua strada: la cartella di lavoro è l'opzione `cartella` (era `cd … &&`),
  l'unione di stderr nell'uscita è `unisciErrori` (era `2>&1`), e il `2>/dev/null` non serve
  perché gli errori stanno in un campo loro che chi non li vuole non guarda. I `wslpath`
  sparsi ovunque sono diventati due funzioni, `versoWindows` e `versoWsl`. Due dettagli che
  vale la pena sapere prima di toccare questo codice: senza shell, un programma **che non
  esiste** non è più un codice 127 sull'uscita ma un evento `error` che, se nessuno lo
  ascolta, fa cadere il server intero (`esegui` lo ascolta e risponde `codice: -1`); e gli
  eseguibili si cercano nel `PATH` che il processo Node ha ereditato, non più in quello di
  una shell di login — in WSL è lo stesso, perché i percorsi di Windows ce li mette
  l'interoperabilità, ma un server avviato da un ambiente spoglio non troverebbe più
  `powershell.exe`. Quel che il figlio riceve, invece, **non è cambiato di una virgola**:
  l'array di argomenti è identico a quello che bash gli passava, e i percorsi
  `\\wsl.localhost\…` arrivano a PowerShell interi come prima — provato.

- **Di due elenchi nella stessa finestra ne guidava uno solo.** *(Debito annotato il
  2026-08-24 con R6; **curato il 2026-08-30**.)* `scegli_riga` raccoglieva tutte le liste
  della finestra e poi prendeva `$tabelle[0]`: la prima e basta. In «Modifica i testi», che
  ne ha due affiancate, la colonna «Lasciate fuori» non si raggiungeva — il giro del
  «Togli →/← Rimetti» andò provato **a mano**, e da allora quella metà di finestra nessun
  collaudo automatico la toccava. Il difetto non era nell'accessibilità, come si era
  scritto: un nome le due liste ce l'hanno già, perché WinForms presta a un controllo il
  testo dell'etichetta che lo precede. Mancava solo il modo di **dire quale**.

  Adesso le liste si contano tutte, `controlli` le numera (*«per «scegli_riga» è la lista
  2»*) e si sceglie con `lista`, per nome o per numero. Senza `lista` la riga si cerca in
  tutte: se combacia in **una** si agisce, se combacia in **due** l'attrezzo si ferma e
  dice dove — premere nella lista sbagliata sarebbe lo stesso peccato del «Premuto» che non
  aveva premuto. Falsificata rimettendo `[0]`: la stessa chiamata risponde «Non c'è nessuna
  lista numero 2», e senza `lista` «Nessuna riga contiene «Competenza 16»» — che è
  esattamente com'era prima, con la riga a un palmo di distanza e invisibile.

- **Una riga oltre la piega si cliccava dov'era scritta, non dov'era.** *(Trovata il
  2026-08-30 provando la cura qui sopra, curata lo stesso giorno.)* UI Automation il
  rettangolo di una riga lo dichiara **anche quando la riga sta fuori dalla parte visibile
  della lista**: della 25ª di 26 diceva `y = 586` mentre la lista finiva a `610`, e della
  prima `y = 130` mentre la lista cominciava a `370`. Il clic partiva lì, cadeva **dentro
  la finestra** — quindi la guardia del pixel rispondeva «è tua» — e non sceglieva niente.
  Adesso, prima di colpire, la riga si porta in vista con `ScrollItemPattern.ScrollIntoView()`:
  è scorrere la lista, non sceglierla, quindi nessun evento dell'applicazione scatta e la
  scelta resta quella del mouse.

- **Una finestra che non è una finestra rende cieco tutto l'attrezzo.** *(2026-08-30.)* La
  radice su cui ogni azione lavora è la finestra del processo con l'**handle più alto**.
  Quel giorno, accanto alla finestra vera (handle 131420), è comparso per qualche minuto un
  `Pane` senza controlli con handle **657034**: `controlli` ha risposto tre volte «nessun
  controllo: l'applicazione è aperta?» con l'applicazione aperta e visibile, `schermata` ha
  fotografato un rettangolo di 267 × 25 pixel e `ridimensiona` ha ridimensionato quello.
  Adesso fra le finestre di primo livello si tengono solo le **Window**, e se non ce n'è
  nessuna si ripiega su ciò che c'è. **Il sintomo è quello di un'applicazione chiusa**: se
  gli attrezzi diventano ciechi tutti insieme, prima di riavviare guarda che cosa c'è
  davvero sotto la scrivania —
  `FindAll(Children, ProcessId)` e il `ControlType` di ciascuna. *(Il `Pane` era transitorio
  e non si è saputo riprodurre: la cura è ragionata sul dump, e questa è la sola delle tre
  che non si è potuta falsificare.)*

- **Il «Premuto» che non aveva premuto.** *(Trappola del 2026-08-27, guardando a occhio le
  tre cose nuove; **curata il 2026-08-29**.)* Per tutta la sua vita `clic` aveva riferito un
  successo ogni volta che **trovava** il controllo, qualunque cosa poi succedesse al colpo.
  Quel giorno «Chiudi» delle Impostazioni stava a **y = 1177** su un'area di lavoro alta
  1032 — 145 pixel **sotto il bordo dello schermo** — e due tentativi hanno risposto
  «Premuto» senza che accadesse niente; un terzo è finito a Esplora file, passato davanti fra
  una fotografia e l'altra. Col DPI non c'entrava nulla: lo schermo era al 100 %.

  Adesso ogni attrezzo che muove il puntatore — `clic`, `scrivi`, `scegli_voce`,
  `scegli_riga` — prima di colpire pretende **due cose**, e se una manca dichiara che non ha
  premuto:
  1. **il primo piano, verificato**: `SetForegroundWindow` preceduta da un **colpo di ALT**
     (senza, Windows lo rifiuta a un processo che non è già davanti) e poi controllata con
     `GetForegroundWindow`, fino a tre tentativi. Il confronto è sul **processo**, non sulla
     singola finestra: una finestra di messaggio dell'applicazione è lei quanto la principale.
  2. **che il pixel sia suo**: `WindowFromPoint` sul punto da premere, e di nuovo confronto
     per processo. Una domanda sola copre i due modi di sbagliare bersaglio — il controllo
     fuori dallo schermo e l'altra finestra passata davanti — e resta lecito il clic sui
     popup che l'applicazione apre **fuori** dalla sua finestra, come la tendina di un menù.

  Falsificata lo stesso giorno rimettendo il codice di prima: sullo stesso bottone spinto
  fuori schermo risponde «Premuto «Backup…»», e il colpo — che `SetCursorPos` **clampa** al
  bordo — finisce sulla **barra delle applicazioni**, dove alla prova ha minimizzato
  l'applicazione. Non era soltanto un successo riferito a vuoto: era un clic a caso su
  Windows. *(Il vecchio consiglio — «prima di credere a un “Premuto”, chiedi a `controlli`
  se è successo qualcosa» — resta buono lo stesso: vale per tutto ciò che le due guardie
  non possono vedere, a cominciare dal DPI alto qui sopra.)*

- **Il primo `clic` su un bottone che apre una finestra non la apre** *(2026-08-21, T9b)*.
  Premendo «⚙ Impostazioni» su un'applicazione appena avviata lo strumento risponde
  «Premuto», e non si apre niente: né la finestra, né un errore. Al **secondo** `clic`
  compare. Succede allo stesso modo con i bottoni **dentro** una finestra già aperta —
  «ELIMINA TUTTI I DATI», che chiama la conferma critica, ha voluto anche lui due colpi.
  Il primo `Invoke` sembra andarsene nel dare il fuoco alla finestra. Costa un'ora se non
  lo si sa, perché il sospetto cade sull'applicazione: si va a cercare un gestore che non
  parte, e invece parte benissimo — a dimostrarlo è bastato mettergli in cima un
  `MessageBox` e vederlo comparire al primo colpo. Perciò: quando un `clic` dovrebbe
  aprire una finestra e la fotografia non la mostra, **ripremi prima di indagare**; e per
  sapere se la finestra c'è, `controlli` la elenca in cima (le finestre di messaggio le
  legge invece `rispondi_finestra` senza argomenti).
- **I percorsi.** `dotnet.exe` è un eseguibile Windows: un percorso alla maniera di WSL
  (`/mnt/c/…`) MSBuild lo scambia per un'opzione (errore `MSB1001`). I progetti si
  nominano perciò relativi a `VB.NET/src`, e i comandi si eseguono da lì.
- **La finestra massimizzata.** Portare una finestra in primo piano con `SW_RESTORE` la
  **rimpicciolisce** se era massimizzata: si finiva per fotografare l'applicazione in
  una misura che l'utente non aveva mai scelto, con difetti di impaginazione che
  esistevano solo nella fotografia. Si ripristina solo ciò che è davvero ridotto a icona.
- **L'etichetta che si spaccia per la casella.** UI Automation dà a una casella il nome
  della sua etichetta, e l'etichetta nell'albero viene prima: chiedere «Incolla qui il
  testo dell'annuncio» restituiva la *scritta*, non la casella. Ci si scrive dentro senza
  che si veda niente e ci si clicca sopra senza che il fuoco arrivi — e si finisce per
  incolpare l'applicazione. Perciò `Trova` cerca fra i **tipi che servono**: caselle per
  scrivere, bottoni per premere.
- **Gli argomenti non reggono due righe di comando.** Passavano da bash e poi da
  PowerShell, e gli apostrofi sparivano per strada: `dell'annuncio` arrivava come
  un'etichetta vuota. Ora viaggiano in un file JSON — che sta in `/tmp`, cioè
  `\\wsl.localhost\…` per Windows, e va passato fra apici **singoli**: fra doppi, bash si
  mangia una delle due barre iniziali.
- **Uno script `.ps1` senza BOM.** PowerShell 5.1 lo legge come ANSI: gli accenti si
  guastano, e `«$etichetta»` diventa il nome di variabile `$etichettaÂ` — inesistente,
  così il messaggio esce senza l'etichetta e sembra un difetto di ricerca. Gli script qui
  dentro **si salvano con il BOM**, e dichiarano `[Console]::OutputEncoding` a UTF-8
  perché gli accenti arrivino interi a chi legge.
- **La finestra di scelta file non si pilota con UI Automation.** Il processo continua ad
  avere una sola finestra di primo livello, e l'albero UIA di quel dialogo espone solo la
  parte alta: «Nome file», «Apri» e «Annulla» non ci sono. Si passa dalle finestre native
  (casella `Edit` 1148, bottone 1 per confermare, 2 per annullare): è quel che fa
  `scegli_file`.
- **La pagina web ha i nostri stessi nomi.** La WebView2 di P3 racconta a UI Automation
  tutto quello che mostra, e i portali hanno bottoni che si chiamano come i comandi
  dell'applicazione. Con Jooble aperto, i bottoni «Cerca» erano **due** — quello del sito
  e il nostro — e quello del sito, nell'albero, veniva prima: `clic` premeva il bottone
  della pagina, che rifaceva la ricerca sul portale, e riferiva «Premuto «Cerca»». Il
  pannello sembrava rotto e non lo era. Si distinguono dal `FrameworkId`, che per la
  pagina è `Chrome` e per i nostri `WinForm`: i controlli del sito si prendono **solo se
  non c'è nient'altro**, e quando succede l'attrezzo lo **dichiara** — premere qualcosa
  nella pagina è legittimo (accettare i cookie, aprire un risultato), ma chi legge deve
  sapere che non era un comando nostro.
- **Ogni menù a tendina si porta dentro un bottone**, la sua freccia, che Windows in
  italiano chiama «Apri». In P3 i bottoni di nome «Apri» sono così **tre**, e i due finti
  vengono prima di quello vero: senza la stessa regola di sopra, `clic «Apri»` apriva una
  tendina e diceva di aver premuto il bottone. La freccia si apre con `scegli_voce`.
- **Un menù di sola lettura (`DropDownList`) non ha il `ValuePattern`**, e il suo `Name`
  è l'etichetta accanto — «Cerca su» — che non cambia mai. La voce che mostra si chiede
  allo schema di selezione, o a Windows con `WM_GETTEXT`: è uno dei pochi messaggi che il
  sistema porta anche fuori dal processo che chiede, insieme al suo buffer.
- **Una voce si sceglie col mouse**, non con `CB_SETCURSEL` né con la `Select()` dello
  schema: quelle cambiano la voce mostrata **senza avvisare l'applicazione**, che così non
  esegue niente mentre l'attrezzo canta vittoria. Vale la stessa regola dei bottoni. E la
  prova non è il clic: è **rileggere** la voce dopo, e confrontarla con quella chiesta.
- **`avvia_app` torna prima che la finestra ci sia.** Il processo parte, ma per qualche
  secondo non ha ancora una finestra, e il primo comando risponde «TrovaLavoro non ha una
  finestra aperta» — poi la catena di comandi prosegue lo stesso, perché l'attrezzo ha
  risposto: è l'applicazione a non essere pronta, non il server a essere caduto. **Prima
  di guidare l'applicazione si chiede `controlli`** e si guarda se risponde.
- **`controlli` torna vuoto anche quando l'applicazione è vivissima.** *(2026-08-15, il
  collaudo di T7a.)* La risposta «nessun controllo: l'applicazione è aperta?» è una
  **domanda**, non una diagnosi, e va letta così: capita mentre l'albero UI Automation è
  momentaneamente irraggiungibile — la WebView2 di P3 che si sta inizializzando, o un
  pannello che si ricostruisce dopo un'attesa dell'AI. È successo tre volte in un giro
  solo, e ogni volta la chiamata **subito dopo** ha elencato i controlli regolarmente.
  Perciò non è la prova che qualcosa sia andato storto e **non si riavvia niente**: si
  chiede `stato_app` — che legge il processo, non l'albero — e se il processo c'è si
  richiama `controlli`. La cosa da non fare è credere alla domanda e rifare l'avvio, che
  sull'applicazione vera vuol dire perdere quel che non era ancora stato salvato.
- **`cartella_dati` guarda sempre la cartella predefinita.** *(2026-08-18, il collaudo di
  T7c.)* L'applicazione avviata con `dati` scrive nella cartella usa-e-getta — lo dicono il
  titolo della finestra e la fascia in fondo — ma `cartella_dati` continua a elencare
  `%AppData%\TrovaLavoro`. Chi legge la sua risposta si vede davanti i file dei **dati
  veri** e li scambia per quelli della prova: è il modo più diretto di credere che una
  prova abbia funzionato quando ha scritto altrove, o di allarmarsi perché «i file di
  prima sono ancora lì». Finché l'attrezzo non impara la cartella scelta, dopo un
  `avvia_app` con `dati` i file si guardano da bash nella cartella che si è indicata.
- **In P5 la casella di risposta prende il nome dell'ultima bolla.** *(2026-08-18.)* È
  l'etichetta che si spaccia per la casella, vista dalla chat: nell'albero UIA la casella
  non ha nome suo e si porta dietro quello del testo che la precede — che in una
  conversazione è l'intera ultima risposta dell'AI, anche mezza pagina. Perciò `scrivi`
  vuole un pezzo di *quella*, non «La tua risposta…». E c'è un seguito: se quel pezzo
  contiene un **apostrofo**, la casella non si trova più — l'apostrofo non sopravvive al
  passaggio degli argomenti (v. sopra). Si sceglie un tratto della bolla che ne sia privo:
  è l'unica cosa che serve sapere, e fa risparmiare un quarto d'ora.
- **Un pezzo di parola non è una somiglianza.** Il ripiego «per contenuto» di `clic`
  prendeva il primo nome che *conteneva* il testo cercato: «Cerca», col pannello Profilo
  davanti, trovava il bottone di navigazione «🔍 Ri**cerca**» e ci spostava. Il 2026-08-12
  questo ha portato una catena di comandi a scrivere «magazziniere» nella casella «Cosa
  facevo» del profilo vero e a catturare la pagina sbagliata: **quel che si scrive
  sostituisce quel che c'era** (è un `SetValue`, non un'aggiunta), e solo il fatto che il
  profilo non fosse ancora stato salvato ha evitato il danno su disco. Ora il ripiego
  pretende una **parola intera** — «Analizza» trova ancora «Analizza…», «Ricerca» trova
  ancora «🔍 Ricerca», ma «Cerca» non trova più niente e lo dice. Restano due regole di
  condotta: **un comando alla volta, con `controlli` in mezzo a dire dove siamo**; e se
  una prova ha toccato dati veri senza volerlo, si chiude con `chiudi_app` — che è
  `taskkill /F` e **non** passa dal salvataggio — e si verifica la data del file su disco.
- **Una lista in vista dettagli non ha nome, e le sue righe hanno il nome sbagliato.**
  *(2026-08-12, T5c.)* La coda delle candidature di P1 arriva a UI Automation come una
  **`Table` senza nome** — cercarla per etichetta è impossibile — con una `ListItem` per
  riga; e il `Name` della riga è la **sola prima colonna**, che lì dentro sono le stelle:
  chiedere «Rossi S.p.A.» fra i nomi delle righe non trova niente, perché in quel campo
  c'è scritto «★☆☆☆☆ 1,0». La riga si cerca perciò nel testo di **tutte** le sue celle,
  che nell'albero sono dei `Text` sotto la riga. Per la stessa ragione `controlli`, di una
  lista, non dice il nome ma **quante righe ha**.
- **Una finestra di messaggio è la stessa classe della scelta file** (`#32770`), e come
  quella non si pilota con UI Automation. Distinguerle serve, perché vogliono due attrezzi
  diversi: la scelta file ha la **casella del nome** (`Edit` 1148), una finestra di
  messaggio no — ed è così che `controlli` e `clic` dicono quale dei due usare, invece di
  mandare a provare quello sbagliato mentre la finestra blocca tutto il resto. I bottoni,
  poi, **non hanno un numero che si possa sapere in anticipo** (dipende da quali bottoni
  la finestra mostra): si riconoscono dal loro testo, ripulito della `&` dell'acceleratore.
- **Un argomento obbligatorio che manca era peggio di un errore.** *(2026-08-12, T5c.)*
  Chiamando `clic` col nome di argomento sbagliato — `etichetta` invece di `nome` — il
  server accettava lo stesso, e l'etichetta arrivava **vuota**: un'etichetta vuota combacia
  con qualunque nome, così il ripiego «a parola intera» prendeva il **primo controllo della
  finestra**. La risposta è stata «Premuto «🏠 Home»» — un successo riferito per una cosa
  mai chiesta, che su un'altra schermata sarebbe stato un comando premuto davvero. Ora
  `tools/call` **rifiuta** la chiamata a cui manca un argomento `required`, e nel dirlo
  **elenca quelli che l'attrezzo vuole**: è l'informazione con cui si rimedia al primo
  colpo, invece di indovinare. Chi parla col server via `curl` i nomi giusti li vede da
  `tools/list`, ed è lì che conviene guardare **prima**.
- **La fotografia può ritrarre la finestra sbagliata.** *(2026-08-14, T5d; **curata il
  2026-08-29**.)* `schermata` porta l'applicazione davanti e poi riprende **quel che sta
  davanti**: se il sistema non le ha ancora dato il primo piano, quel che sta davanti è la
  finestra da cui si stava lavorando — il terminale — e ne esce una fotografia che sembra un
  difetto dell'applicazione mentre è solo la finestra sbagliata. Il rimedio era chiamarla
  **due volte di fila**; adesso lo fa lo script, con la stessa ricetta di `clic` — colpo di
  ALT, `SetForegroundWindow`, verifica con `GetForegroundWindow`, fino a tre tentativi — e se
  proprio non ci riesce **lo scrive nella risposta**, dicendo chi è rimasto davanti. Resta
  vero il vecchio consiglio, perché la fotografia arriva comunque: prima di credere a quel
  che si vede, si guarda **di chi** è la finestra ritratta.

  *(Curandola è saltato fuori un secondo difetto, invisibile finché lo script non ha avuto
  una parola con l'accento da scrivere: `schermata.ps1` non impostava l'`OutputEncoding`, e
  il primo messaggio accentato è uscito «l� c'� rimasta». Adesso ce l'ha, come
  `interfaccia.ps1`.)*
- **Il caso stretto non si ricava da quello largo.** *(2026-08-14, misurando la fascia dei
  comandi di P2.)* Sotto i **1350 px** di larghezza il pannello del logo passa in modalità
  compatta e si stringe da 261 a 130 px: la fascia dei comandi comincia a 142 invece che a
  273, e i conti fatti sui numeri della finestra grande sbagliano di 131 px — cioè proprio
  nel caso che si sta misurando. Vale anche per `ridimensiona`: chiedere 1150 px e
  aspettarsi il layout di prima in piccolo è il modo di diagnosticare un difetto che non
  c'è, o di non vedere quello che c'è. Da qui la regola: **prima si ridimensiona, poi si
  fotografa**, e non si crede a nessun numero che non sia stato riletto dopo.
- **La finestra non scende sotto la sua `MinimumSize`** (1150x600, cap. 03.4): chiedere
  1000 px dà 1150 e Windows non protesta. `ridimensiona` rilegge sempre la misura vera e
  la dichiara, perché una misura chiesta e non ottenuta è il modo più silenzioso di
  guardare la cosa sbagliata.
- **La tendina va sempre richiusa**, anche quando qualcosa va storto (in un `finally`):
  lasciata aperta blocca tutte le chiamate dopo, e l'errore sembra dell'applicazione.
  Va anche portata avanti la finestra **prima** di aprirla: una tendina aperta mentre la
  finestra sta dietro si richiude da sola appena la si mette in primo piano.
- **Un `testhost` che sopravvive blocca la compilazione dopo.** *(2026-08-14, T6.)* Se
  `collaudi` viene interrotto — un timeout della chiamata, una sessione chiusa a metà — il
  processo `testhost.exe` che faceva girare il banco **resta vivo** e tiene bloccata
  `TrovaLavoro.Collaudi\bin\…\TrovaLavoro.dll`. Il giro successivo fallisce con dieci
  tentativi di copia e un `MSB3027`, e l'errore parla di file bloccati: sembra un guasto
  del progetto ed è un avanzo del giro di prima. Si chiude dal nome, perché il nome ce
  l'ha solo lui: `taskkill.exe /F /IM testhost.exe`, e poi si rifà il banco. Vale anche
  fra una sessione e l'altra — il processo sopravvive alla chiusura di chi l'ha lanciato.
- **Aspettare un bottone non è aspettare che il lavoro finisca.** *(2026-08-18, il collaudo
  di T7b.)* Un giro intero di generazione è stato letto sui **testi vecchi** credendoli
  nuovi, per due errori che si sommano. Il primo: **«Genera CV + lettera» su una candidatura
  già generata non rigenera niente** — apre P6 e mostra i documenti che ci sono; a rifarli è
  «Rigenera», con la sua conferma. Il secondo: «Rigenera» è **acceso prima e dopo**, quindi
  un'attesa che guarda quel bottone finisce subito e sembra riuscita. Nemmeno la **data del
  file** basta, perché l'applicazione risalva la cartella intera anche solo cambiando lingua
  dalla tendina: il file cambia data senza cambiare contenuto. L'unica condizione onesta è
  il **contenuto** — si legge `lettera.json` e si aspetta che il testo non sia più quello di
  prima. È anche la misura giusta per il futuro `aspetta_che` (`idee_future.md`).
- **`scrivi` perde gli a capo.** *(2026-08-18.)* Un testo su più righe — un annuncio
  incollato in P4 — arriva nella casella **tutto attaccato**: «S.r.l.Sede di lavoro».
  All'analisi dell'AI non è importato (l'annuncio è stato letto giusto lo stesso), ma se
  quel che si sta provando dipende dalla forma del testo, questa è una differenza fra ciò
  che si crede di aver incollato e ciò che c'è davvero nella casella.
- **La fotografia può ritrarre un suggerimento.** *(2026-08-18.)* Subito dopo aver premuto
  una casella che ha un tooltip, la finestra in primo piano del processo **è il tooltip**:
  `schermata` ha restituito due volte di fila un'immagine di 412×25 px con dentro la
  scritta del suggerimento, che a prima vista sembra un guasto dell'attrezzo. Non lo è, ed
  è cugina della trappola qui sopra sulla finestra sbagliata: si riprende con
  `schermo_intero`, oppure si aspetta che il suggerimento sparisca.
- **Aspettare lo stato di «Rigenera» non è aspettare che rigeneri.** *(2026-08-18, nascita
  di `aspetta_che`.)* È la stessa storia di sopra («Aspettare un bottone non è aspettare
  che il lavoro finisca»), ma vale la pena ripeterla qui perché ora c'è un attrezzo apposta
  e la tentazione di usarlo male è più vicina: un bottone come «Rigenera» è **acceso sia
  prima sia dopo** il clic, quindi `aspetta_che nome=Rigenera stato=acceso` si soddisfa
  **subito**, nell'istante stesso in cui parte — e chi legge l'esito («condizione
  soddisfatta dopo 0,3 secondi») ci crede, mentre il lavoro AI è appena cominciato. La
  strada affidabile per aspettare la **fine** di un lavoro AI è la modalità «file» di
  `aspetta_che`, puntata sul documento che quel lavoro produce (`contiene`, o anche senza:
  basta che cambi rispetto a com'era prima); non basta nemmeno guardare la **data** del
  file, perché l'applicazione può risalvare l'intera cartella per un motivo che non ha
  niente a che fare col lavoro appena finito (cambiare lingua dalla tendina, per dirne
  una). In alternativa, se non c'è un file comodo da guardare, si aspetta **due volte**: prima
  che il bottone si **spenga** (`stato=spento`), poi che si **riaccenda** (`stato=acceso`)
  — è il solo modo di usare la modalità «controllo» senza raccontarsi una bugia.
- **Premere «Interrompi» mentre l'AI scrive: l'attesa e il clic devono stare nello stesso
  comando.** *(2026-08-19, chiudendo la voce rimasta da T7c.)* Qui l'attrezzo giusto è la
  modalità «controllo» e non quella «file» — non c'è nessun file da guardare, e «Interrompi»
  è uno dei rari bottoni che è acceso **solo** mentre il lavoro è in volo, quindi
  `aspetta_che nome=Interrompi stato=acceso` dice la verità. Il punto è un altro: fra
  l'`aspetta_che` che ritorna e il `clic` c'è la latenza di **due chiamate separate**, e una
  risposta del ragionamento può essere finita in quel mezzo — al primo tentativo è andata
  proprio così, il bottone era già tornato «Invia» e l'attrezzo ha risposto «non ho trovato
  Interrompi». La ricetta che funziona: **una sola invocazione** che fa `aspetta_che` e
  subito dopo `clic`, senza tornare indietro a chi la guida. E la domanda va scelta perché la
  risposta duri: una che chieda un elenco lungo e ragionato compra i secondi che servono.
  Riuscito così alla seconda prova, con il clic a **3,9 secondi** dall'invio, e l'esito è
  quello promesso — il testo già arrivato resta a video, la bolla si marca «(interrotto)»,
  nessun errore.
- **Per far fallire l'AI non si toglie la chiave: se ne mette una finta.** *(2026-08-18,
  provando dal vivo il cambio lingua del 📄 CV-1 base.)* Sembra ovvio che, per vedere cosa
  succede quando una chiamata all'AI va male, basti avviare l'applicazione senza chiave. Non
  funziona, e fa perdere un giro intero: senza chiave `ContestoApp.MontaAi` esce subito,
  `AiDisponibile` resta falso e i bottoni che portano ai pannelli dell'AI restano **spenti**
  — non si fallisce a metà, si **resta fuori**, e al pannello che si voleva guardare non ci
  si arriva nemmeno. Serve una chiave **presente ma non valida** (una stringa qualunque:
  `sk-ant-questa-chiave-non-esiste`): così il motore si monta, i pannelli si aprono, la
  chiamata parte davvero e l'API la rifiuta con un 401 — che è esattamente il ramo
  «l'AI fallisce» che si voleva percorrere. Attenzione, per una prova del genere l'attrezzo
  `avvia_app` **non** va bene: carica sempre la chiave vera dal `.env` del prototipo, e
  vanificherebbe la prova. L'exe si lancia a mano, con la variabile d'ambiente che si vuole
  (ricordando `WSLENV`) e sempre con `--dati` su una cartella usa-e-getta.
- **Chiudere per nome ammazza anche il server MCP del prodotto.** *(2026-08-21, dal collaudo
  di tappa di T8. **Dal 2026-08-30 non lo fa più nessuno dei tre.** `compila` e `chiudi_app`
  sono guariti la mattina — passano da `chiudi-finestre.ps1`, che guarda la riga di comando e
  lascia vivere chi ha `--mcp`. Provato con una finestra aperta davvero: chiusa lei, il server
  è rimasto. `collaudi` è guarito la sera, e in un altro modo: non chiude più niente affatto,
  perché non compila più dove il server tiene bloccato — vedi il punto qui sotto. Quel che
  segue resta scritto perché è la storia, e perché il modo di distinguere i due processi
  serve ancora.)* I tre attrezzi facevano `taskkill.exe /IM TrovaLavoro.exe /F`:
  chiudono per **nome**, non per processo. Da quando esiste il server MCP del **prodotto**
  (cap. 09), un client che ce l'abbia registrato — Claude Code, per dirne uno — tiene in vita
  un secondo `TrovaLavoro.exe` senza finestra, e quei tre attrezzi lo spengono insieme
  all'applicazione, facendo cadere il client che stava collaudando. Finché l'attrezzo non
  impara a scegliere il PID, **non si chiamano `compila`, `collaudi` e `chiudi_app` mentre
  un client esterno parla col server MCP del prodotto**. Dei tre il più insidioso è
  `collaudi`, perché lo si lancia per abitudine e non sembra un attrezzo che chiude niente:
  anche il banco ricompila, e per ricompilare deve liberare l'exe. *(Aggiunto `collaudi`
  il 2026-08-23: fa lo stesso `taskkill` degli altri due, e l'elenco lo dimenticava.)* Per distinguerli: da PowerShell,
  `Get-Process TrovaLavoro | Select Id, StartTime, MainWindowTitle` — il server è quello
  **senza titolo di finestra**; per chiudere solo l'applicazione si usa `CloseMainWindow()`,
  che è la chiusura gentile, non `taskkill /F`.
- **Il banco non ha bisogno di `bin/`, e chiedere una cartella qualunque non basta.**
  *(2026-08-30, sera.)* La cura del punto qui sopra per `collaudi` è una riga:
  `dotnet test -c Release -p:BaseOutputPath='banco\'`. Compilando altrove, l'exe di
  `bin\Release` non viene toccato, il server MCP del prodotto se lo tiene bloccato quanto
  vuole, e il banco gira **in Release** con l'applicazione aperta e i tool della sessione
  vivi: 1289 verdi provati così, senza chiudere niente.
  **La trappola sta nel percorso.** Il primo tentativo mandava l'output in
  `C:\Temp\collaudi-trovalavoro\`, che è fuori dal repo e sembra la scelta più pulita: la
  compilazione riesce — «Errori: 0» — e poi cadono **dieci** collaudi con
  `DirectoryNotFoundException: Non trovo la cartella dei casi`. Il banco trova i suoi casi
  **risalendo** da `AppContext.BaseDirectory` fino al `.vbproj` (`CasiDiCollaudo.Cartella`), e
  da `C:\Temp` non risale a niente. Il percorso va **relativo** — `banco\`, senza radice —
  così ogni progetto scrive dentro di sé e la risalita continua a funzionare. È una diagnosi
  che costa cara perché il messaggio d'errore del `BaseOutputPath` non parla: dice che manca
  una cartella, e sembra un guasto del banco. La cartella è ignorata da git
  (`VB.NET/src/.gitignore`).
- **Un `Await` che attraversa un thread dentro una finestra del banco non torna mai.**
  *(2026-09-01, il blocco F2-2 dei fix UI.)* Costruire un controllo installa sul thread il
  contesto di sincronizzazione di Windows Forms, che rimanda ogni ritorno da un `Await`
  alla pompa di messaggi della finestra — e nel banco la pompa non c'è, perché le finestre
  si costruiscono e non si mostrano. Finché le finte rispondono in modo sincrono l'`Await`
  non sospende e il problema non può presentarsi; il primo collaudo che ha attraversato
  davvero un thread (il `Task.Run` dell'export del backup) non è diventato rosso: si è
  **piantato**, dieci minuti senza un esito — e un collaudo appeso non è rosso, è niente.
  La cura sta in `ConMotoreAsync` (`TrovaLavoro.Collaudi/CollaudiFinestraBackup.vb`): si
  spegne `WindowsFormsSynchronizationContext.AutoInstall` **e** si azzera il contesto per
  il tempo della prova, rimettendo poi entrambi dov'erano. Azzerare il contesto e basta
  non basta: il primo controllo costruito lo rimette, che è precisamente il senso di
  «AutoInstall» — ed è una manopola del **processo**, non del thread. Il codice di
  produzione resta con `ConfigureAwait(True)`, che lì è la cosa giusta: nell'applicazione
  vera la pompa gira, e la riga di stato va scritta dal thread che possiede la finestra.
- **Davanti alla sola informativa del primo avvio gli attrezzi dicono «non ha una
  finestra aperta».** *(2026-09-01, il giro delle rifiniture del tutor.)* Al primo avvio
  su una cartella dati vergine l'informativa modale si apre dal `Load`, prima che la
  finestra principale esista: il processo ha **una sola** finestra di primo livello —
  l'informativa, `ControlType.Window`, coi suoi bottoni — eppure `controlli` e `clic`
  hanno risposto «TrovaLavoro non ha una finestra aperta», tre volte su tre, mentre una
  UIA scritta a mano la vedeva benissimo: un `FindAll` sui figli della radice con
  `ProcessIdProperty` l'ha elencata e un `InvokePattern` ha premuto «Ho capito» al primo
  colpo. Quale filtro dello script la scarti non si è ancora capito — non è il `Pane`
  della trappola qui sopra: il tipo è `Window` — e finché non lo si trova, il ripiego è
  quello: PowerShell diretto, finestre del processo per `ProcessId`, bottone per nome.
- **Rimettere a posto un file falsificato con `mv` non fa ricompilare.** *(2026-08-24, dal
  quinto tempo di T9e.)* Falsificare vuol dire rompere apposta il codice e guardare se il
  collaudo diventa rosso (regola 14): si mette da parte l'originale, si guasta il file, si
  lancia il banco, si ripristina. Se il ripristino si fa con `mv file.bak file`, l'orario
  del file torna **indietro** — `mv` conserva l'mtime dell'originale — e MSBuild, che
  confronta i tempi, vede un sorgente più vecchio della DLL e **salta la compilazione**:
  `compila` e `collaudi` dicono «riuscita» su un binario che è ancora quello guasto. Il
  banco va rosso su un collaudo appena visto verde, e il sospetto cade sul codice invece
  che sullo strumento. Si ripristina con **`cp file.bak file`**, che l'orario lo aggiorna.
  Vale la pena aggiungere che se ne esce **strumentando** — facendo stampare al confronto i
  valori veri — e non ipotizzando: quando la realtà smentisce due volte di fila, la cosa da
  mettere in dubbio è la misura, non il codice.
- **Avviare l'applicazione da WSL con `cmd.exe /c start` resta appeso.** *(2026-08-21.)*
  `cmd.exe /c start "" "…\TrovaLavoro.exe"` non ritorna: la chiamata sembra bloccata e finisce
  in background solo allo scadere del timeout. Funziona invece invocare l'eseguibile
  direttamente con `nohup … &`, poi aspettare qualche secondo e **verificare il processo**
  invece di fidarsi. Serve quando l'applicazione va lanciata a mano, fuori da `avvia_app` —
  per esempio per lasciarla viva mentre un client MCP esterno le contende i dati.
- **`dati.lock` che c'è non vuol dire lucchetto tenuto.** *(2026-08-21.)* Il file è vuoto
  (0 byte) e **resta su disco** anche dopo che l'applicazione lo ha rilasciato: quel che conta
  è la presa esclusiva, che vive nel sistema operativo e non nel file. Vederlo nella cartella
  dati dice che l'app è passata di lì, non che è ancora aperta. Per sapere se è **tenuto**
  serve un secondo processo che provi davvero a prenderlo.
- **`chiamate_ai.csv` confronta due strade a colpi di token, senza leggere il codice.**
  *(2026-08-21.)* Il diario dei consumi nella cartella dati registra prompt, modello, tetto,
  token e percentuale di **ogni** chiamata, da qualunque porta arrivi — finestra o server MCP
  del prodotto. Per stabilire se due strade fanno davvero lo stesso mestiere non serve
  rileggersi il montaggio del motore: si fa lo stesso gesto dalle due parti e si guardano le
  righe nuove. Nel collaudo di T8 l'analisi dell'annuncio è uscita **identica al token** dalle
  due porte, e quella riga ha dimostrato più di mezza giornata di lettura del codice.
- **`GetDpiForSystem` risponde 96 a chi non si è dichiarato DPI-aware.** *(2026-08-23, il
  giro B del collaudo di T9e.)* Portata la scala di Windows al 150% e fatta la
  disconnessione che serve a farla valere davvero, la prima misura diceva ancora **96** —
  cioè «la disconnessione non è servita a niente» — ed era falso: quel numero è il valore di
  compatibilità che il sistema riserva a chi il DPI non lo capisce, e chi misurava non aveva
  chiamato `SetProcessDPIAware()`. Si rischia di buttare via un collaudo che invece si poteva
  fare, o di rifare una disconnessione per niente. Le controprove costano un comando e vanno
  prese **prima** di concludere: `Screen.Bounds` virtualizzato (1280×720 su uno schermo
  1920×1080) e `LogPixels` nel registro di Windows (144 = 150%).
- **Il clic a coordinate assolute arriva a chi ha il fuoco, non a chi si è fotografato.**
  *(2026-08-23, il giro B del collaudo di T9e.)* Le voci di menù e le caselle che UI
  Automation non pilota (vedi sopra) si premono con un clic vero alle coordinate lette sulla
  fotografia — ma se fra la fotografia e il clic una console PowerShell passa in primo piano,
  il clic va **al terminale**, e non lo dice nessuno: l'attrezzo risponde «premuto» e
  l'applicazione non ha ricevuto niente. È successo davvero, e il punto di collaudo è andato
  perso in silenzio. Si porta **prima** la finestra dell'applicazione in primo piano, si
  verifica che ci sia andata (`GetForegroundWindow`), e si clicca in coordinate **relative a
  lei** — così la stessa prova vale a qualunque posizione della finestra.
- **Il fattore di scala non è ×1,5, e non è lo stesso nelle due direzioni.** *(2026-08-23, il
  giro B del collaudo di T9e.)* Con lo schermo al 150% viene naturale calcolare che una
  misura di progetto diventi il 150% di sé stessa. WinForms però scala con
  `AutoScaleMode.Font`, cioè sul rapporto fra i font, e i due rapporti sono **diversi**: nel
  giro B la larghezza è cresciuta ×1,42 e l'altezza ×1,605, così il minimo della finestra
  principale — 1150×600 di progetto — è diventato 1633×963 pixel veri, che in unità logiche
  fa **1088,7×642**: più **stretto** del minimo dichiarato. Chi si calcola la soglia attesa
  moltiplicando per il DPI la sbaglia, e poi crede a un difetto o ne scusa uno vero per il
  motivo sbagliato. Il fattore si **misura**: si chiede una misura più piccola del minimo e
  si rilegge il rettangolo che Windows concede.
