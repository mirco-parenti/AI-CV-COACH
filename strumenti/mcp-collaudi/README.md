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
| `controlli` | Elenca bottoni, caselle e schede dicendo per ciascuno se è **acceso o SPENTO**; dei menù a tendina dice anche **la voce che mostrano**, e marca `[pagina]` quel che è del sito aperto nel browser. |
| `clic` · `scrivi` | Preme un controllo per etichetta; scrive in una casella. Se il controllo è spento lo dichiara invece di fingere. |
| `scegli_voce` | Sceglie una voce in un menù a tendina — il portale in «Cerca su» — aprendolo e cliccandoci dentro come farebbe una persona, e poi verificando che il menù la mostri davvero. Senza la voce, le **elenca**. |
| `scegli_riga` | Sceglie una riga di una lista — la coda delle candidature in P1 — cercandola per un pezzo di quel che c'è scritto **in una qualsiasi delle sue celle**, e verificando poi che risulti scelta. Senza il testo, le **elenca**; con `doppio`, fa il doppio clic (che nella coda apre la candidatura). |
| `rispondi_finestra` | Risponde a una finestra di messaggio premendo il bottone per nome («Sì», «No», «OK»). Senza il bottone **legge cosa chiede** e quali scelte dà, così si sa cosa si sta per confermare. |
| `scegli_file` | Risponde alla finestra di scelta file che l'applicazione ha aperto: il file da prendere (anche in forma `/mnt/c/…`), oppure `annulla`. |
| `cartella_dati` | Cosa l'applicazione ha scritto su disco: profilo, storico, opportunità, documenti. |

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

- **Le attese.** Non c'è un `aspetta_che` (che il bottone si accenda, che l'attesa
  finisca): per ora si alterna `clic` e `controlli`.
- **Il contenuto delle caselle di testo non si legge.** L'elenco dice la voce dei menù,
  ma non cosa c'è scritto in una casella: per leggere l'indirizzo del browser serve
  ancora una fotografia.
- **La finestra non si ridimensiona.** *(Emerso a T5d, 2026-08-14.)* L'applicazione si
  apre massimizzata e lì resta: un difetto di impaginazione che si vede solo a finestra
  stretta — alla `MinimumSize` dichiarata, per esempio — con questi attrezzi non si
  guarda. Si può ancora rifare a mano il conto che fa il codice, ed è quel che è stato
  fatto per la fascia dei comandi di P2 (`in_sospeso.md`), ma un conto non è una
  fotografia. *E ha una trappola sua, pagata il 2026-08-14 rifacendolo*: sotto i 1350 px
  di larghezza il pannello del logo passa in **modalità compatta** e si stringe da 261 a
  130 px, quindi la fascia comincia a 142 invece che a 273. Chi misura il caso stretto
  usando la larghezza del logo a finestra grande sbaglia di 131 px — cioè proprio nel caso
  che sta misurando. La prova che il conto è giusto è che riproduca i numeri già a verbale
  per il caso noto, prima di applicarlo a quello nuovo.

## Le trappole già pagate

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
- **La tendina va sempre richiusa**, anche quando qualcosa va storto (in un `finally`):
  lasciata aperta blocca tutte le chiamate dopo, e l'errore sembra dell'applicazione.
  Va anche portata avanti la finestra **prima** di aprirla: una tendina aperta mentre la
  finestra sta dietro si richiude da sola appena la si mette in primo piano.
