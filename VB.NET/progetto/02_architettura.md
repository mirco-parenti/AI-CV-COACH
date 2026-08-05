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
│  ClientClaude│  lettura PDF/DOCX/ │  .eml · .msg (Outlook)    │
│  Libreria-   │  TXT/MD; scrittura │  invio SMTP               │
│  Prompt      │  DOCX/PDF; scans.  │                           │
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
  - `ClientClaude`: le chiamate HTTPS a `api.anthropic.com` (v. 2.5).
- **`Documenti/`** — lettura (PDF via API con blocco `document`, DOCX/TXT/MD in
  locale), scrittura (DOCX e PDF), scansione e classificazione della cartella
  documenti (cap. 05).
- **`Posta/`** — composizione email, salvataggio `.eml`, salvataggio `.msg` via
  Outlook quando presente, invio SMTP (cap. 07).
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
| Macchina a stati del dialogo + magazzino `pending` (index.html) | rinasce nel motore (`Orchestratore`) — stessa logica: schede di conferma, instradamento `altrove`, guard anti-rimbalzo, «lasciato fuori» esplicito |
| Soglia 1,5 stelle (index.html) | costante del motore, stesso comportamento (sconsiglia, non impedisce) |
| Banchi `test-*.html` | rinascono come collaudi del cap. 14 (per-anello, sugli stessi casi) |

Gli endpoint HTTP del prototipo **spariscono**: erano il confine tra browser e Node;
qui il confine equivalente è la firma delle funzioni del motore (e, verso l'esterno,
i tool MCP).

## 2.5 Le chiamate all'AI

- **Trasporto**: HTTPS dirette a `https://api.anthropic.com/v1/messages` con
  `HttpClient` di .NET — niente SDK, come il prototipo (header `x-api-key` +
  `anthropic-version`). Il corpo è lo stesso del prototipo: un solo messaggio `user`
  con il prompt già riempito; per i PDF, il blocco `document` in base64.
- **Modelli**: due livelli come nel prototipo — `MODELLO_SEMPLICE` (estrazioni; oggi
  Claude Haiku 4.5) e `MODELLO_RAGIONAMENTO` (confronto, mitigazione, generazione,
  brainstorming; oggi Claude Sonnet 4.6). I nomi modello **non sono cablati nel
  codice**: stanno nei metadati dei prompt e in configurazione, così un cambio di
  modello non richiede una nuova build. All'avvio dell'implementazione si riverifica
  quali siano i modelli correnti più adatti.
- **Sincrono o streaming**: le **estrazioni** restano sincrone (risposta breve, attesa
  accettabile con indicatore). Il **brainstorming** e la **generazione** usano lo
  streaming (`stream: true`, eventi SSE): il testo compare man mano, l'attesa percepita
  crolla. È l'unica vera novità di trasporto rispetto al prototipo.
- **Robustezza**: timeout esplicito per chiamata; un solo retry automatico su errore di
  rete o HTTP 429/5xx (con pausa); ogni errore arriva all'utente in italiano, con la
  possibilità di riprovare. Le risposte JSON passano da `EstrattoreJson`; se il JSON
  resta invalido si mostra il testo grezzo in un riquadro «cosa ha risposto il
  modello», mai un crash.
- **Nessuna memoria lato modello**: come nel prototipo, ogni chiamata è autonoma; la
  memoria (profilo, conversazione di brainstorming) vive nel programma, che a ogni
  turno manda il contesto necessario.

## 2.6 Concorrenza e reattività

Tutte le chiamate AI, di rete e su file girano **fuori dal thread dell'interfaccia**
(`Async/Await`): la finestra non si congela mai. Ogni operazione lunga ha indicatore di
attesa e pulsante Annulla (che annulla la richiesta, non lo stato già salvato).
Un'operazione per volta per opportunità: niente code parallele nascoste, il flusso
resta comprensibile.

## 2.7 Dove vive lo stato

Tutto lo stato persistente sta nella **cartella dati** (cap. 11), in file JSON
leggibili. L'app è l'unica a scriverli; niente database, niente registro di Windows
(salvo l'associazione minima di configurazione se servisse). Chiudere l'app e riaprirla
riporta esattamente dov'eravamo: lo stato in memoria è sempre ricostruibile dal disco.
