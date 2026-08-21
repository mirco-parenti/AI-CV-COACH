# 09 — Il server MCP integrato

*Le funzioni dell'applicazione esposte, in modo controllato, ai client AI esterni
(Claude Desktop, Claude Code e qualunque altro client MCP): stesso motore, stessi
prompt, stessi dati — un'interfaccia in più.*

## 9.1 Cos'è, in due parole

MCP (Model Context Protocol) è un protocollo aperto con cui un'applicazione può
mettere a disposizione di un assistente AI dei **tool**: funzioni con nome, descrizione
e parametri, che l'assistente può chiamare. In pratica: con il server MCP attivo,
l'utente può stare in Claude Desktop e dire «confronta il mio profilo con questo
annuncio e dimmi se vale la pena candidarmi» — e Claude userà **la nostra pipeline**
(prompt del pool, punteggio deterministico, hard-gate) invece di improvvisare.

> **Da non confondere con il server MCP di collaudo** *(nato a T4c, 2026-08-10)*. In
> `strumenti/mcp-collaudi/` vive un secondo server MCP che **non è parte del prodotto**:
> non entra nell'eseguibile, non si distribuisce e non tocca la pipeline. Quello espone
> gli attrezzi per **provare** l'applicazione — compilare, far girare il banco, avviarla,
> fotografarla, premerle i bottoni, sceglierle una voce da una tendina o una riga da un
> elenco, risponderle quando chiede un file o una conferma. Questo capitolo
> parla invece dei tool che l'applicazione **offre** ai client AI dell'utente (tappa T8).
> Uno serve a chi costruisce il programma, l'altro a chi lo usa.

## 9.2 Come si avvia

- **Stesso exe, modalità dedicata**: `TrovaLavoro.exe --mcp`. Nessun secondo programma:
  il vincolo «un solo file» resta intatto.
- In questa modalità l'app non apre finestre: parla con il client via **stdio**
  (standard input/output), il trasporto MCP più semplice e senza rete. È il client
  (es. Claude Desktop) ad avviare il processo, secondo la sua configurazione:

```json
{
  "mcpServers": {
    "trovalavoro": {
      "command": "C:\\TrovaLavoro\\TrovaLavoro.exe",
      "args": ["--mcp"]
    }
  }
}
```

- Il percorso in `command` è semplicemente quello dove sta l'exe (che può essere una
  cartella qualsiasi — cap. 13.4).
- Il protocollo è JSON-RPC 2.0, e il programma lo implementa direttamente, senza
  librerie aggiuntive: è scambio di messaggi JSON su stdio, alla portata del motore che
  già maneggia JSON tutto il giorno. **Quali** messaggi, però, dipende da con chi si
  parla — la sezione qui sotto.
- **Un messaggio per riga, e nessun a capo dentro**: è l'incorniciatura dello stdio, e
  vale in entrambe le direzioni. Tutto UTF-8, il che sul nostro lato non è una formalità:
  questo programma parla italiano, e con la codepage di sistema gli accenti uscirebbero
  rotti dalla prima riga.
- **Su stdout passano solo i messaggi del protocollo.** La spec non lascia margini: il
  server *non deve* scrivere su stdout niente che non sia un messaggio MCP valido. Avvisi,
  diagnostica ed errori vanno su **stderr**, che è esplicitamente il posto giusto per i
  log e che i client raccolgono in un file a parte. Lì finiscono anche gli avvisi della
  riga di comando (cap. 11.1), che senza barra di stato non avrebbero dove andare.
- **Più richieste insieme** *(2026-08-19)*. Il filo che legge non si ferma mai:
  riconosce il messaggio, mette da parte il lavoro e torna ad ascoltare. Con i soli tool
  di lettura la differenza non si sarebbe vista — si apre un file e si è già finito — ma
  un `genera_cv` dura minuti, e un server che lo aspetta è un server **sordo**: non
  sentirebbe nemmeno il «lascia perdere» che il client manda proprio mentre quel lavoro è
  in corso. La conseguenza immediata è che **sull'uscita si scrive uno alla volta**: due
  risposte che escono insieme si intreccerebbero a metà riga, e la riga è la cornice del
  messaggio.
- **Un lavoro si può ritirare.** Il client manda `notifications/cancelled` con
  l'identificativo della richiesta, e quel lavoro si ferma dov'è. A una richiesta ritirata
  **non si risponde**: chi ha annullato non aspetta più niente su quell'identificativo, e
  mandargli un errore vorrebbe dire raccontargli un guasto che ha causato lui apposta. Un
  annullamento che arriva quando il lavoro è già finito non è un guasto ma la normalità:
  le due cose si sono incrociate sulla pipe.
- **Si esce quando stdin si chiude, e quel che è in volo si annulla.** È il segnale di
  spegnimento primario e l'unico portabile: il client chiude l'ingresso e aspetta che il
  processo termini. Un server che non lo onora si fa ammazzare a forza, ed è un modo
  brutto di finire. Da quel momento nessuna risposta ha più dove andare, quindi i lavori
  ancora in corso si fermano invece di essere portati a termine: macinare un CV che
  nessuno leggerà sarebbe solo un modo più lento di morire, e intanto la chiave
  dell'utente continuerebbe a pagarlo.

### Le due ere del protocollo (2026-08-19)

*Questa parte sostituisce quel che il capitolo diceva fino al 2026-08-18 — «i tre passi
canonici `initialize`, `tools/list`, `tools/call`». Non era sbagliato quando è stato
scritto: era il protocollo di allora.*

Il **28 luglio 2026** MCP ha cambiato pelle. La revisione `2026-07-28` ha tolto
l'handshake e ha reso il protocollo **senza stato**: non c'è più una sessione da aprire,
ogni richiesta si autodescrive. Le due ere hanno un nome nella spec stessa — **legacy**
(`2025-11-25` e precedenti, quelle con l'handshake) e **moderna** (`2026-07-28` e
successive).

| | **Legacy** (fino a `2025-11-25`) | **Moderna** (da `2026-07-28`) |
|---|---|---|
| Apertura | `initialize`, poi `notifications/initialized` | **nessuna**: si entra chiamando |
| Versione del protocollo | negoziata una volta per sessione | dichiarata **a ogni richiesta**, in `_meta` |
| Chi sei / cosa sai fare | scambiati nell'handshake | in `_meta` a ogni richiesta |
| Scoperta | dalla risposta di `initialize` | `server/discover`, che i server **devono** implementare |
| Forma del risultato | `result` | `result` con dentro `resultType` |

**Il programma le parla tutte e due** (nella spec: un server *dual-era*). Non è
abbondanza: la matrice di compatibilità dice che un client legacy davanti a un server solo
moderno **fallisce e basta** — i client vecchi non hanno modo di saltare avanti — e che un
client moderno davanti a un server solo legacy fallisce a sua volta. Scegliere un'era sola
vuol dire scommettere su quale delle due parli il client che l'utente ha installato oggi,
sapendo che fra sei mesi la risposta sarà diversa. Il costo della doppia porta è modesto e
si paga una volta: **i tool sono identici nelle due ere** — `tools/list` e `tools/call`
hanno lo stesso nome e la stessa forma — e cambia solo come si entra.

**L'era si riconosce a ogni messaggio, senza ricordarsi niente**: se la richiesta porta la
versione del protocollo in `_meta` è moderna, se arriva un `initialize` è legacy. È la
strada che la spec descrive per i server dual-era, ed è anche l'unica onesta su un
trasporto dove il processo non è una conversazione: il client può intrecciare sulla stessa
pipe richieste che non c'entrano nulla fra loro.

**Le risposte dei tool** portano il risultato due volte, ed è voluto: il JSON strutturato
in `structuredContent`, per chi lo sa leggere, e lo stesso JSON serializzato in un blocco
di testo, che è quel che la spec chiede per compatibilità.

**Il server si presenta da sé, e un client vero lo mostra.** Sia `initialize` sia
`server/discover` portano un campo `instructions`: poche righe che dicono a chi arriva che
mestiere fa questo server e — soprattutto — quali sono i suoi limiti veri, cioè che **il
punteggio in stelle lo calcola il programma e non si negozia a parole** e che nessun testo
prodotto inventa esperienze che il profilo non dichiara. Sono i due vincoli del prodotto
detti a un lettore che non è una persona: un modello che arriva da fuori non ha letto il
`README.md`, e senza quelle righe proverebbe a trattare sul voto. È anche la superficie che
il banco **non può** esercitare, perché non ha un modello a cui mostrarla — il primo a
riceverla e a mostrarla davvero è stato un client vero, nel collaudo di tappa di T8
*(2026-08-21, cap. 14)*.

**Due specie di errore, e non vanno confuse.** Un tool che non esiste o una richiesta
malformata sono **errori di protocollo** e tornano come errori JSON-RPC. Un tool che
esiste ma non può fare il suo lavoro — la chiave API non c'è, l'opportunità chiesta non
esiste, l'app tiene il lucchetto — risponde invece con un risultato normale marcato
`isError`, il cui testo è scritto per essere **letto da chi ha chiamato**, che qui è un
modello: così può correggersi da solo invece di limitarsi a riferire un guasto. È la
stessa idea del messaggio onesto che l'applicazione applica già a video (cap. 03.8),
spostata su un interlocutore diverso.

## 9.3 I tool esposti

Prima versione — tutti i tool leggono/scrivono la **stessa cartella dati** dell'app:

| Tool | Cosa fa | Scrive dati? |
|---|---|---|
| `leggi_profilo` | restituisce il profilo JSON corrente | no |
| `leggi_registro` | elenco opportunità con stati, stelle, date — e da **T9c** l'esito, quando c'è | no |
| `leggi_opportunita` | tutti gli artefatti di una singola opportunità | no |
| `analizza_annuncio` | testo annuncio → Annuncio JSON (stesso prompt del pool) | no |
| `confronta` | profilo + annuncio → giudizi, stelle, note (con clamp e hard-gate) | no |
| `mitiga` | profilo + giudizi → mitigazioni oneste (anche lista vuota) | no |
| `struttura_cv` | testo di un CV (trascritto o incollato) → proposta di profilo JSON, senza salvarla | no |
| `genera_cv` | CV-1 o CV-2 in JSON, lingua a scelta, appunti di mira come parametro facoltativo | no |
| `genera_lettera` | lettera in JSON, lingua a scelta | no |
| `rifinisci_testo` | passata anti-slop su un testo di prosa | no |
| `esporta_documento` | il CV e la lettera di una candidatura salvata → file DOCX e/o PDF nella sua cartella | sì (nuovi file) |
| `salva_opportunita` | mette in coda una candidatura: l'annuncio e quel che di essa è già stato prodotto | sì |
| `esporta_backup` | scrive un backup JSON nella cartella `backup\`: il profilo con storico e CV base, oppure tutto (registro e candidature) | sì (nuovo file) |

**Una riga di diagnostica, e la cartella che nasce** *(2026-08-19)*. La colonna «scrive
dati» parla dei **dati dell'utente** — profilo, registro, candidature, documenti — e per i
sette tool che passano dall'AI resta «no»: nessuno di loro tocca niente di tutto ciò. Ogni
chiamata all'AI però annota una riga in `chiamate_ai.csv` (cap. 11.1), esattamente come
quando a chiamare è l'applicazione: quale prompt, quanto è costato, quanta parte del
proprio tetto ha consumato. È **diagnostica, non roba dell'utente** (cap. 11.1, «l'unico
file che non è dell'utente»), ed è la misura con cui si ritarano i `max_token` sui numeri
veri invece che a naso — tanto più adesso che il livello di ragionamento è passato a
Sonnet 5 (cap. 02.5). La conseguenza va detta perché è visibile: alla prima chiamata via
MCP la **cartella dati nasce**, anche se prima non c'era. Il file si scrive in fondo e non
si riscrive mai, quindi non è la scrittura che il lucchetto (§9.4) deve proteggere.

**I documenti escono rifiniti** *(2026-08-19)*. `genera_cv` e `genera_lettera` fanno
passare quel che hanno scritto dalla passata anti-slop (cap. 08), come fa la fila dentro
l'applicazione. Costa una seconda chiamata, e si paga volentieri: il CV chiesto da un
client MCP dev'essere **lo stesso** che si otterrebbe dalla finestra. Una differenza di
qualità fra le due porte non la dichiarerebbe nessuno, e si scoprirebbe mesi dopo
confrontando due documenti senza capire perché uno dei due è più piatto. Chi vuole la
passata su un testo suo ha `rifinisci_testo`, che è la stessa cosa offerta da sola.
*E da **T9b** (2026-08-21) «lo stesso che si otterrebbe dalla finestra» include anche il
caso in cui la finestra la rifinitura non la fa: l'interruttore delle Impostazioni
(cap. 08.4) vive nella cartella dati, che il server legge come la legge l'applicazione, e
vale perciò da tutte e due le porte. Spento, di qui escono documenti col testo grezzo — ed
è la risposta giusta: un interruttore valido solo nell'interfaccia farebbe divergere le due
porte proprio sul testo, che è la cosa che questo paragrafo esiste per impedire.*

**Da qui escono anche i PDF, e `salva_opportunita` prende tutto** *(2026-08-19; la prima
metà riscritta lo stesso giorno)*. Due cose che la tabella da sola non spiega. La prima:
`esporta_documento` scrive **DOCX, PDF o tutti e due**, e chi non dice il formato li ottiene
tutti e due — la stessa regola dei testi rifiniti, cioè che da qui esca quel che sarebbe
uscito dalla finestra. *Fino al pomeriggio del 2026-08-19 questo capitolo diceva l'opposto:
«solo DOCX, perché il PDF si stampa nel browser incorporato, che vuole una finestra, e in
`--mcp` il programma biforca prima di ogni preparativo grafico». La frase era imprecisa, e a
smentirla è stato il **banco di collaudo**, che stampa PDF veri da un processo di test dove
finestre non ce n'è nessuna: quel che il motore pretende non è una finestra visibile ma un
**filo STA con la sua pompa di messaggi**, e quello si accende anche in modalità server
(`FiloGrafico`, nato in T4b fra i collaudi e portato nel prodotto adesso). Il PDF è stato poi
prodotto davvero dall'eseguibile vero avviato con `--mcp`.* Il prezzo resta, e va detto:
accendere il motore del browser costa qualche secondo, e chi ha fretta chiede `docx`. Se il
motore non c'è, i DOCX si consegnano lo stesso dicendo perché il PDF manca — chi aveva chiesto
il solo PDF invece riceve un fallimento, perché è quello che è. La seconda: `salva_opportunita` accetta
**l'annuncio e tutti gli artefatti** — confronto, mitigazioni, CV, lettera — e non il solo
annuncio come diceva la prima stesura di questo capitolo. Con il solo annuncio, quel che i
tool dell'AI producono non avrebbe dove andare: si potrebbe generare un CV via MCP senza
poterlo mettere da nessuna parte, e `esporta_documento` non avrebbe mai niente da
impaginare. Due regole del prodotto valgono comunque, e sono il motivo per cui questo tool
non è un semplice «scrivi quel che ti do»: le **stelle si ricalcolano** dai giudizi invece
di accettarle da fuori (§9.5), e dei documenti **senza il confronto da cui nascono** si
rifiutano — la macchina degli stati (cap. 07.3) non ammette il salto da «nuova» a
«generata», e ha ragione: un 🎯 CV mirato nasce dai giudizi.

**`esporta_backup` è arrivato con T9a** *(2026-08-21; la nota di attesa è del 2026-08-19)*.
Era annunciato dalla prima stesura e rimasto vuoto per una dipendenza, non per una
dimenticanza: espone la funzione F7, che si costruisce a T9 (cap. 11.4), e finché quella
non c'era il bottone dell'applicazione era visibile e spento. Adesso c'è, e il tool con
lui. Due cose che la riga della tabella non dice. La prima: **espone le stesse due scelte
della finestra** — il solo profilo, oppure tutto — perché sono la stessa funzione vista da
due porte, e una porta che ne offrisse una sola costringerebbe a chiudere l'applicazione
per avere l'altra; chi non sceglie ha il **profilo**, che è il dato che l'utente non può
rifare da capo. La seconda: **il ripristino non si fa da qui**. Rimettere a posto un backup
sovrascrive roba dell'utente e passa dall'anteprima che dice *cosa* sostituisce *cosa*: sta
nell'applicazione, dov'è l'utente a guardarla, per la stessa ragione per cui non esiste un
tool che cambia il profilo. Il file finisce nella cartella `backup\` con un nome che porta
il giorno, e se quel nome è occupato prende un progressivo invece di cancellare il backup
di stamattina.

**Fuori dalla prima versione, di proposito**: la modifica del profilo via MCP, azione
irreversibile che resta nell'app dove c'è la conferma visiva dell'utente (livello 6 del
cap. 03). Potrà entrare in seguito, con un meccanismo di conferma esplicita.
Quanto all'**invio di email**, la questione non si pone più nemmeno nell'app: nella 1.0
il programma non spedisce, prepara un file `.eml` (cap. 07 e cap. 15, voce 9).

**Non sono tool, e il perché**: il **brainstorming** non serve come tool — il client
MCP è già un assistente conversazionale; gli bastano gli artefatti (`leggi_*`) e il
parametro «appunti di mira» dei tool di generazione. La **classificazione della
cartella documenti** è un'operazione interattiva locale (scelta file + conferma a
video) e resta nell'app. L'**export leggibile del registro** è coperto da
`leggi_registro`: la formattazione la fa il client come preferisce.

## 9.4 Convivenza con l'app aperta

Due processi che scrivono gli stessi file sono un problema classico. Regola semplice:

- la cartella dati ha un **lucchetto di scrittura** (file di lock): lo prende chi
  scrive per primo;
- se l'app con finestre è aperta, il server MCP funziona lo stesso ma i tool marcati
  «scrive dati» rispondono con un messaggio onesto («l'app è aperta: chiudi la
  finestra o usa i tool di sola lettura»);
- viceversa, l'app all'avvio segnala se un server MCP sta scrivendo.

Niente sincronizzazioni sofisticate: un solo scrittore alla volta, dichiarato.

### Com'è fatto, e perché così (2026-08-19)

**Il pericolo vero non è la corsa sui byte.** Le scritture di questo programma sono già
atomiche una per una (cap. 11.1): se due processi salvassero lo stesso file nello stesso
istante, ne resterebbe uno dei due, intero. Il danno è un altro, e non lo segnalerebbe
nessun file corrotto — l'applicazione tiene aperti in memoria un profilo o una
candidatura, il server MCP li cambia sul disco sotto di lei, e al primo salvataggio
l'applicazione riscrive sopra quel che non ha mai visto. È una **perdita silenziosa**, ed
è la sola ragione per cui questo lucchetto esiste.

**Il lucchetto è il file tenuto aperto in esclusiva, non il suo contenuto.** Dentro
`dati.lock` non c'è niente: nessun numero di processo, nessuna ora. È una scelta e non
una pigrizia — chi lo tiene lo dichiara al sistema operativo, e quando quel processo
muore, comunque muoia (chiuso, in crash, ammazzato dal Gestione attività), è **Windows a
rilasciarlo**. Un lucchetto scritto andrebbe invece *ripulito* da qualcuno, e prima o poi
lascerebbe l'utente chiuso fuori dai propri dati per un file rimasto lì da un crash di tre
settimane prima. Il file resta anche dopo il rilascio, vuoto: cancellarlo aprirebbe una
gara con chi lo sta riaprendo in quel momento, e zero byte non danno fastidio a nessuno.

**Chi lo prende, e per quanto, non è simmetrico** — perché non lo sono i due che scrivono:

| | Quando lo prende | Per quanto |
|---|---|---|
| L'app con finestre | all'avvio, appena montato il motore | **tutta la sessione**: l'utente scrive quando gli pare, e fra un salvataggio e l'altro tiene comunque in mano dei dati che nessun altro deve muovere |
| Il server MCP | quando un tool sta per scrivere, **dopo** aver controllato i parametri | **la sola scrittura**, poi lo rilascia: non ricorda niente fra una richiesta e l'altra, quindi non ha niente da proteggere nel frattempo |

Il server lo prende il più tardi possibile di proposito: tenerlo mentre si guarda un JSON
malformato vorrebbe dire fermare l'applicazione dell'utente per un errore di chi ha
chiamato.

**Se non si riesce a prenderlo, le due parti reagiscono diverso**, e anche questo segue da
chi sono. Il server MCP **rifiuta la scrittura** e lo dice in modo che un modello possa
farci qualcosa — che chiudere la finestra è la strada, e che i tool di lettura e quelli
dell'AI continuano a funzionare. L'applicazione invece **parte lo stesso** e mette un
avviso nella barra di stato: fermare l'utente sulla porta di casa sua sarebbe
sproporzionato, ma se poi due salvataggi si accavallano deve poter dire che gliel'avevamo
detto.

## 9.5 Sicurezza

- **Solo locale**: il trasporto stdio non apre porte di rete; il server esiste solo
  come processo figlio del client sullo stesso PC. Un eventuale trasporto HTTP locale è
  rimandato (cap. 15) e comunque nascerebbe legato a `localhost`. È anche il motivo per
  cui qui non c'è nessuna autenticazione da implementare: l'impalcatura di autorizzazione
  di MCP riguarda i trasporti HTTP, e la spec dice espressamente che su stdio non si
  applica — le credenziali si prendono dall'ambiente, che per noi vuol dire la chiave
  cifrata nella configurazione.
- **La chiave API resta dove sta**: cifrata nella configurazione dell'app (cap. 11).
  Il client MCP non la vede mai: chiama i tool, e sono i tool a usare la chiave.
- **Stessi limiti etici**: i tool passano dagli stessi prompt del pool — anti-invenzione
  e hard-gate valgono anche quando a chiamare è un'AI esterna. Il punteggio non si può
  «convincere»: è calcolato dal codice.

## 9.6 Perché ne vale la pena

- **Dimostrabilità**: per il portfolio, un'app che è anche server MCP mostra competenza
  su un protocollo che il mercato sta adottando adesso.
- **Automazioni personali**: l'utente evoluto può orchestrare valutazioni in serie
  («questi 5 annunci: quali passano le 3 stelle?») dal suo assistente preferito.
- **Architettura onesta**: se il motore è davvero separato dall'interfaccia (cap. 02),
  esporlo via MCP è un'aggiunta sottile, ed è anche la **prova** che la separazione
  regge.
