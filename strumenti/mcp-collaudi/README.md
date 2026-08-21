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

**Dopo aver toccato `server.mjs` o gli script il server va riacceso**, altrimenti
continua a rispondere col codice di prima — e si finisce per diagnosticare una modifica
che non è mai entrata in servizio. Si spegne **dalla porta**, non dal nome:

```bash
fuser -k 3300/tcp && node strumenti/mcp-collaudi/server.mjs &
```

Cercarlo per nome (`pkill -f mcp-collaudi/server.mjs`) sembra più naturale e invece è una
trappola: il pattern compare anche nella riga di comando che lo sta cercando, così il
comando **uccide sé stesso** prima di arrivare al server.

## Gli attrezzi

| Attrezzo | Cosa fa |
|---|---|
| `compila` | Compila TrovaLavoro in Release. Chiude prima l'applicazione, che altrimenti tiene bloccato l'exe. |
| `collaudi` | Fa girare il banco (`dotnet test`); con `filtro` ne esegue solo una parte. |
| `avvia_app` | Avvia TrovaLavoro.exe con la chiave API presa dal `.env` del prototipo. Con `dati` lo fa partire su una **cartella usa-e-getta** (`--dati`), che è il modo di provare ciò che cancella senza toccare i dati veri. |
| `stato_app` · `chiudi_app` | Se è viva; e la chiude. |
| `schermata` | Riprende la finestra dell'applicazione (o tutto il desktop) e restituisce il PNG. |
| `ridimensiona` | Cambia la misura della finestra, o la rimette massimizzata. È il modo di guardare i difetti di impaginazione che si vedono **solo stretti**. |
| `controlli` | Elenca bottoni, caselle e schede dicendo per ciascuno se è **acceso o SPENTO**; dei menù a tendina dice anche **la voce che mostrano**, e marca `[pagina]` quel che è del sito aperto nel browser. |
| `clic` · `scrivi` | Preme un controllo per etichetta; scrive in una casella. Se il controllo è spento lo dichiara invece di fingere. |
| `scegli_voce` | Sceglie una voce in un menù a tendina — il portale in «Cerca su» — aprendolo e cliccandoci dentro come farebbe una persona, e poi verificando che il menù la mostri davvero. Senza la voce, le **elenca**. |
| `scegli_riga` | Sceglie una riga di una lista — la coda delle candidature in P1 — cercandola per un pezzo di quel che c'è scritto **in una qualsiasi delle sue celle**, e verificando poi che risulti scelta. Senza il testo, le **elenca**; con `doppio`, fa il doppio clic (che nella coda apre la candidatura). |
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

## Le trappole già pagate

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
- **La fotografia può ritrarre la finestra sbagliata.** *(2026-08-14, T5d.)* `schermata`
  porta l'applicazione davanti e poi riprende **quel che sta davanti**: se il sistema non
  le ha ancora dato il primo piano, quel che sta davanti è la finestra da cui si stava
  lavorando — il terminale — e ne esce una fotografia che sembra un difetto
  dell'applicazione mentre è solo la finestra sbagliata. Il rimedio è chiamarla **due
  volte di fila** con `porta_in_primo_piano`: la seconda trova l'applicazione già davanti.
  Non capita sempre — dipende da chi aveva il fuoco un attimo prima, e nella sessione del
  14/08 la prima chiamata è andata a segno — ed è proprio l'intermittenza a renderla
  insidiosa: prima di credere a quel che si vede, si guarda **di chi** è la finestra
  fotografata.
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
  di tappa di T8.)* `compila` e `chiudi_app` fanno `taskkill.exe /IM TrovaLavoro.exe /F`:
  chiudono per **nome**, non per processo. Da quando esiste il server MCP del **prodotto**
  (cap. 09), un client che ce l'abbia registrato — Claude Code, per dirne uno — tiene in vita
  un secondo `TrovaLavoro.exe` senza finestra, e quei due attrezzi lo spengono insieme
  all'applicazione, facendo cadere il client che stava collaudando. Finché l'attrezzo non
  impara a scegliere il PID, **non si chiamano `compila` e `chiudi_app` mentre un client
  esterno parla col server MCP del prodotto**. Per distinguerli: da PowerShell,
  `Get-Process TrovaLavoro | Select Id, StartTime, MainWindowTitle` — il server è quello
  **senza titolo di finestra**; per chiudere solo l'applicazione si usa `CloseMainWindow()`,
  che è la chiusura gentile, non `taskkill /F`.
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
