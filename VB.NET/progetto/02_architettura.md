# 02 — Architettura dell'applicazione

*Come è fatta dentro: i blocchi, i dati che si scambiano, cosa migra dal prototipo e
cosa nasce nuovo. Il disegno funzionale (voci 2.x, anelli) resta quello di
`HTML+JS/architettura.md`; qui si progetta la sua incarnazione desktop.*

## 2.1 Vista d'insieme

Un solo processo, un solo exe. Dentro, sei blocchi principali:

```
┌───────────────────────────────────────────────────────────────┐
│  INTERFACCIA (WinForms, pannelli statici)                     │
│  P1 Home/Registro · P2 Profilo · P3 Ricerca (WebView2) ·      │
│  P4 Opportunità · P5 Dialogo/Brainstorm · P6 Documenti ·      │
│  P7 Email · P8 Impostazioni (finestra separata)               │
├───────────────────────────────────────────────────────────────┤
│  MOTORE (la logica, senza interfaccia)                        │
│  Orchestratore dei flussi · CalcoloMatch · EstrattoreJson     │
│  Stati delle opportunità · Regole (soglia, hard-gate)         │
├──────────────┬────────────────────┬───────────────────────────┤
│  AI          │  DOCUMENTI         │  POSTA                    │
│  ClientClaude│  lettura PDF/DOCX/ │  composizione email       │
│  Libreria-   │  TXT/MD; scrittura │  scrittura .eml           │
│  Prompt      │  DOCX/PDF; scans.  │  (bozza da inviare)       │
│  (pool .md)  │  cartella doc.     │                           │
├──────────────┴────────────────────┴───────────────────────────┤
│  DATI (cartella dati su disco)                                │
│  profilo.json · opportunita/ · registro.json · config         │
│  chiave API cifrata · backup JSON                             │
└───────────────────────────────────────────────────────────────┘
          ▲
          │ (stesso exe avviato con --mcp)
   SERVER MCP: espone le funzioni del MOTORE ai client AI esterni
```

Regola di separazione, semplice ma vincolante: **l'interfaccia non parla mai
direttamente con l'API AI né con il disco** — passa sempre dal motore. È ciò che
permette al server MCP (cap. 09) di riusare *tutte* le funzioni senza duplicarle: MCP e
pannelli sono due «facce» dello stesso motore.

## 2.2 La pipeline di artefatti (il cuore che migra)

Come nel prototipo, l'architettura è **data-centrica**: i blocchi si scambiano pochi
artefatti JSON ben definiti. Quelli ereditati:

| Artefatto | Prodotto da | Consumato da |
|---|---|---|
| **Profilo JSON** | dialogo guidato, import CV, aggiornamento periodico | tutto il resto (unica *fonte di fatti*) |
| **Annuncio JSON** | analisi annuncio (da cattura WebView2, link o testo incollato) | confronto, generazione (come *segnale di mira*) |
| **Giudizi + punteggio** | confronto (AI) + CalcoloMatch (codice) | scheda match, mitigazione, generazione |
| **Mitigazioni JSON** | mitigazione (può essere lista vuota) | solo la ✉️ lettera |
| **CV JSON** (base/mirato) | generazione | scrittura DOCX/PDF, lettera (riferimento di coerenza) |
| **Lettera JSON** | generazione | corpo email, allegato |

Artefatti **nuovi** della fase desktop:

| Artefatto | Cos'è |
|---|---|
| **Preferenze di ricerca** | tipologie di lavoro, zona, contratto, parole chiave, ricerche salvate per portale |
| **Opportunità** | annuncio + fonte/link + stato (`nuova → interessante → generata → inviata → chiusa/scartata`) + tutti gli artefatti prodotti per esso |
| **Appunti di mira** | l'esito confermato del brainstorming (cosa enfatizzare, tono); orientano la generazione, non aggiungono fatti |
| **Registro candidature** | vista d'insieme delle opportunità e dei loro stati, con date |
| **Backup JSON** | profilo (+ storico candidature a scelta) esportabile e reimportabile |

La proprietà «un profilo, molti CV» resta: il profilo è uno, versionato nel tempo; ogni
opportunità ha i **suoi** CV-2 e lettera, riconducibili alla versione di profilo usata.

## 2.3 I moduli del codice

Nomi indicativi (il dettaglio fine si stabilizza in implementazione), pensati perché
ogni modulo abbia **un compito solo**:

- **`Ui/`** — i Form e i pannelli (cap. 03). Nessuna logica: raccolgono input, mostrano
  risultati, chiamano il motore.
- **`Motore/`** —
  - `ContestoApp`: monta il motore all'avvio — cartella dati, pool, numeri, client AI,
    archivio — e **non solleva mai**: ciò che non si può montare resta spento e *detto*
    (cap. 03.8). È il punto in cui i pannelli trovano tutto già pronto;
  - `DialogoProfilo` + `Mossa`: la macchina a mosse del dialogo guidato (v. 2.4);
  - `ImportProfilo`: i due passi dell'import di un CV, dal file al profilo;
  - `Orchestratore`: conduce i flussi del cap. 12 (quale passo viene dopo quale);
  - `CalcoloMatch`: la trascrizione **fedele** di `calcolaMatch` del prototipo — stessi
    pesi (richiesto=5, preferenziale=1, importanza alta/media/bassa=5/3/1, contesto=0,2,
    fallback=3), stesso clamp asimmetrico −20/+10, stesso tetto eliminatorio a 20/100,
    stesse note. È codice deterministico: dato lo stesso input, stesso punteggio del
    prototipo (questo è anche il suo collaudo, cap. 14);
  - `EstrattoreJson`: la versione VB di `estraiJson` — toglie il recinto markdown,
    tenta il parse, e solo in caso di errore ritaglia dal primo `{` all'ultimo `}`;
  - `StatiOpportunita`: la macchina a stati del registro.
- **`Ai/`** —
  - `LibreriaPrompt`: carica i `.md` dal pool, ne legge i metadati, riempie i
    segnaposto (cap. 04);
  - `ClientClaude`: le chiamate HTTPS a `api.anthropic.com` (v. 2.5);
  - `StrutturatoreTurni`, `TrascrittorePdf`: i due mestieri che mettono in fila pool,
    client ed estrattore. Ognuno ha la **sua interfaccia** (`IStrutturatoreTurni`,
    `ITrascrittorePdf`), e non per gusto di astrazione: è la porta da cui il motore si
    stacca dall'AI, e senza quella porta il dialogo e l'import non si potrebbero
    collaudare tutti interi senza rete (cap. 14, T3).
- **`Documenti/`** — lettura (PDF via API con blocco `document`, DOCX/TXT/MD in
  locale), scrittura (DOCX e PDF), scansione e classificazione della cartella
  documenti (cap. 05).
  *In implementazione la lettura è nata altrove, e la divisione ha una ragione*: quella
  che tocca il disco sta in `Dati/LettoreDocumenti` (DOCX/TXT/MD), quella che passa
  dall'API in `Ai/TrascrittorePdf`. Questa cartella resta il posto della **scrittura**
  (DOCX/PDF, T4) e della scansione della cartella documenti (T6).
- **`Posta/`** — composizione dell'email e scrittura del file `.eml` marcato come bozza
  da inviare (cap. 07). *Nella 1.0 non spedisce:* né `.msg` né SMTP (cap. 15, voci 8 e 9),
  quindi questo componente non tocca alcuna credenziale.
- **`Dati/`** — cartella dati, profilo e sue versioni, opportunità, registro,
  configurazione, chiave API cifrata, backup (cap. 11).
- **`Mcp/`** — il server MCP: traduce le richieste del protocollo in chiamate al
  motore (cap. 09).

## 2.4 Cosa migra dal prototipo, e come

| Asset del prototipo | Destino in VB.NET |
|---|---|
| I 15 prompt + schemi (`prompt_design.md`) | diventano i file della **libreria prompt** (cap. 04), contenuto invariato salvo adattamento dei segnaposto |
| `calcolaMatch` + costanti (server.js) | `Motore/CalcoloMatch`, trascrizione 1:1 verificata con casi di collaudo identici |
| `estraiJson` (server.js) | `Motore/EstrattoreJson`, stessa strategia (percorso felice intatto, ripiego solo nel catch) |
| Criterio due modelli (Haiku estrazione / Sonnet ragionamento) | metadato `modello:` di ogni prompt del pool (cap. 04) |
| Macchina a stati del dialogo + magazzino `pending` (index.html) | rinasce nel motore (`Motore/DialogoProfilo`) — stessa logica e **stessi testi**: schede di conferma, instradamento `altrove`, guardia anti-rimbalzo, «lasciato fuori» esplicito. Là ogni passo disegnava da sé la pagina; qui produce una **`Mossa`** (cosa dire, cosa mostrare, cosa aspettarsi) e chi la mostra torna con una risposta o una scelta. La conseguenza è doppia: il pannello si limita a disegnare, e il dialogo intero si collauda senza interfaccia (cap. 14, T3) |
| Soglia 1,5 stelle (index.html) | costante del motore, stesso comportamento (sconsiglia, non impedisce) |
| Banchi `test-*.html` | rinascono come collaudi del cap. 14 (per-anello, sugli stessi casi) |

Gli endpoint HTTP del prototipo **spariscono**: erano il confine tra browser e Node;
qui il confine equivalente è la firma delle funzioni del motore (e, verso l'esterno,
i tool MCP).

## 2.5 Le chiamate all'AI

- **Trasporto**: HTTPS dirette a `https://api.anthropic.com/v1/messages` con
  `HttpClient` di .NET — niente SDK, come il prototipo (header `x-api-key` +
  `anthropic-version`). Il corpo è lo stesso del prototipo: un solo messaggio `user`
  con il prompt già riempito; per i PDF, il blocco `document` in base64. *«Lo stesso»
  è stato preso alla lettera e verificato a T2: sugli stessi artefatti il prompt che
  parte è identico carattere per carattere al suo (cap. 14).*
- **Modelli**: due livelli come nel prototipo — `MODELLO_SEMPLICE` (estrazioni:
  **Claude Haiku 4.5**, `claude-haiku-4-5`) e `MODELLO_RAGIONAMENTO` (confronto,
  mitigazione, generazione, brainstorming). Il modello di prodotto per il ragionamento è
  **Claude Sonnet 5** (`claude-sonnet-5`), scelto il 2026-08-05 riverificando il listino
  (cap. 15, voce 6): succede a Sonnet 4.6 allo stesso prezzo. Ci si arriva però **in due
  tempi**. La batteria di non-regressione di T2 gira su **Sonnet 4.6**
  (`claude-sonnet-4-6`), lo stesso del prototipo: **a parità di modello** una differenza
  nei risultati è una differenza di *codice*, che è esattamente ciò che quel collaudo
  deve misurare — ed è **passata il 2026-08-07**, con i due confronti reali che danno le
  stesse stelle da una parte e dall'altra. Il salto a Sonnet 5 è il **secondo
  esperimento** e si misura da solo, dopo; quando l'avrà superato diventerà lui il
  predefinito.
- **I nomi dei modelli non sono cablati nel codice**: i prompt dichiarano un *livello*
  ("semplice" o "ragionamento", cap. 04) e la mappa livello → modello vive in
  `modelli.json` nella cartella dati, gemello di `taratura.json` — predefiniti dentro il
  programma, file che li scavalca, ripiego dichiarato se il file è illeggibile. Cambiare
  modello, o fare il secondo esperimento, costa **una riga**, non una nuova build.
- **L'interruttore del ragionamento esteso.** Sonnet 4.6 lo tiene spento di suo e il
  prototipo non ne parla affatto: finché si resta lì la richiesta **non dichiara nulla**,
  e così resta identica a quella del prototipo. Su Sonnet 5 il valore predefinito è
  opposto, e lì l'interruttore va acceso (`thinking: {"type": "disabled"}`), perché
  **`max_tokens` limita ragionamento e risposta insieme**: i nostri limiti (1500–4000)
  sono cuciti addosso alla sola risposta, quindi passare a Sonnet 5 senza spegnerlo
  tronca le risposte **senza errore** — e un confronto troncato produce JSON invalido.
  Perciò in `modelli.json` l'interruttore ha **tre** stati e non due: *non dichiarato*
  (richiesta identica al prototipo), *spento*, *acceso*. Vale anche la seconda
  avvertenza del cap. 15: Sonnet 5 conta i token in modo diverso e a parità di testo ne
  usa circa il 30% in più — è un cambio di tokenizzatore, quindi pesa **anche
  sull'input**, non solo sul limite di risposta.
- **La chiave API**: `ClientClaude` la riceve già pronta, non va a cercarsela. Alla tappa
  T2 arriva dalla variabile d'ambiente `ANTHROPIC_API_KEY` — non tocca il disco e non
  entra nel repo; dalla 1.0 arriverà cifrata dalla cartella dati (cap. 11).
- **Sincrono, per ora**: tutte le chiamate sono **sincrone**. Le risposte stanno fra i
  1500 e i 4000 token e si aspettano bene con un indicatore, quindi lo streaming a T2
  non pagherebbe la sua complessità. Lo **streaming** (`stream: true`, eventi SSE) arriva
  con **T4/T7**, dove serve davvero — generazione e brainstorming, cioè i punti in cui il
  testo lungo compare man mano e l'attesa percepita crolla — e si aggiunge quando c'è un
  pannello che lo mostra. È l'unica vera novità di trasporto rispetto al prototipo.
- **Robustezza**: timeout esplicito per chiamata; un solo retry automatico su errore di
  rete o HTTP 429/5xx (con pausa, rispettando l'attesa che l'API suggerisce quando la
  suggerisce); nessun retry sugli errori nostri — una richiesta malformata o una chiave
  sbagliata, riprovata, dà lo stesso errore. Ogni errore arriva all'utente in italiano,
  con la possibilità di riprovare. Si guarda anche **perché il modello ha smesso di
  scrivere**: se si è fermato contro il limite di token la risposta è monca e lo si dice
  subito, invece di lasciarlo scoprire a valle sotto forma di JSON invalido senza sapere
  perché. Le risposte JSON passano da `EstrattoreJson`; se il JSON resta invalido si
  mostra il testo grezzo in un riquadro «cosa ha risposto il modello», mai un crash.
- **Nessuna memoria lato modello**: come nel prototipo, ogni chiamata è autonoma; la
  memoria (profilo, conversazione di brainstorming) vive nel programma, che a ogni
  turno manda il contesto necessario.

## 2.6 Concorrenza e reattività

Tutte le chiamate AI, di rete e su file girano **fuori dal thread dell'interfaccia**
(`Async/Await`): la finestra non si congela mai. Ogni operazione lunga ha indicatore di
attesa e pulsante Annulla (che annulla la richiesta, non lo stato già salvato).

**Un'eccezione dichiarata, decisa in T3c**: il turno del dialogo (P5) si aspetta e basta.
Annullare a metà lascerebbe la macchina a mosse in uno stato che non esiste — la risposta
consegnata ma la mossa mai ricevuta — e per un'attesa di un paio di secondi non vale il
prezzo. L'import di un CV, che dura molto di più, l'Annulla ce l'ha.
Un'operazione per volta per opportunità: niente code parallele nascoste, il flusso
resta comprensibile.

## 2.7 Dove vive lo stato

Tutto lo stato persistente sta nella **cartella dati** (cap. 11), in file JSON
leggibili. L'app è l'unica a scriverli; niente database, niente registro di Windows
(salvo l'associazione minima di configurazione se servisse). Chiudere l'app e riaprirla
riporta esattamente dov'eravamo: lo stato in memoria è sempre ricostruibile dal disco.
