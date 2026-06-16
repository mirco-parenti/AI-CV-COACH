# Architettura — AI-CV-COACH

Questo documento è il **disegno top-down** di AI-CV-COACH: cosa fa il sistema, da quali
funzioni è composto, quali dati vi fluiscono e con quali principi è costruito. È nato
*dopo* il codice — il progetto è cresciuto bottom-up (prima prompt e codice, poi il
disegno) — e mette ora per iscritto l'architettura implicita, così da farne una bussola
per i prossimi passi (colmare i gap, e in prospettiva migrare a VB.NET).

**Come si relaziona agli altri documenti** (niente duplicati — vedi `regole_di_progetto`):
- i **prompt definitivi e gli schemi** vivono in `prompt_design.md`: qui li si **cita**, non
  li si ricopia;
- lo **stato corrente** della pipeline vive nel `README.md` (sezione *Stato*) e nell'ultimo
  `### Step` del `diario_di_bordo.md`: qui **non** si duplica (così il documento non
  invecchia);
- la **narrazione delle decisioni** vive nel `diario_di_bordo.md`; il **backlog** in
  `idee_future.md`. Questo file descrive il **disegno**, non la cronaca né il futuro.

Vocabolario: il progetto parla storicamente di **anelli 1-4**; il tutor ha proposto una
scomposizione per **voci 2.x**. Questo documento tiene entrambi e ne dà la mappa esplicita
(sezione 3).

---

## 1. Introduzione

### 1.1 Cosa fa CV-COACH

CV-COACH accompagna una persona in tre momenti:
1. **costruire un profilo professionale strutturato** a partire da ciò che racconta di sé
   (un dialogo guidato);
2. **valutare quanto quel profilo combacia con un annuncio** di lavoro, con un punteggio
   orientativo e giustificato;
3. **generare i documenti per candidarsi** — un CV base, un CV mirato all'annuncio e una
   lettera di presentazione.

Il filo che tiene insieme i tre momenti è un vincolo: **tutto ciò che esce è fondato solo
su ciò che la persona ha dichiarato davvero**. Il sistema non inventa esperienze,
competenze o titoli, e non perde ciò che l'utente dice (le due bussole etiche, sotto).

### 1.2 A chi serve e quali problemi risolve

Serve a chi deve candidarsi e fatica a **mettere a fuoco e raccontare** ciò che sa fare —
in particolare chi ha un percorso fatto anche di **esperienze informali** (aiuti in
famiglia, volontariato, lavoretti, passioni) che un CV tradizionale tende a scartare, ma
che spesso fanno la differenza.

Problemi che affronta:
- **la pagina bianca**: trasformare un racconto libero in un profilo ordinato, senza
  chiedere alla persona di "scrivere un CV";
- **il match opaco**: dire in modo onesto e motivato *quanto* un profilo è adatto a un
  annuncio, e *dove* sono i punti di forza e le lacune;
- **la tentazione di gonfiare**: produrre CV e lettere convincenti **senza** inventare,
  che è esattamente ciò che rende un documento fragile a un colloquio.

### 1.3 Le tecnologie su cui si basa

In sintesi: front-end nel browser (HTML/CSS/JS), un **aiutante locale Node.js** che
custodisce la chiave API e fa da tramite verso l'LLM, e l'**API di un LLM** con due
modelli scelti per livello di compito. La fase target prevede la migrazione a un'unica
applicazione **VB.NET** su Windows 11. Il dettaglio (versioni, modelli, avvio) è nel
`README.md` ("Tecnologie previste") e in `prompt_design.md` ("Modelli usati"): qui non si
duplica. La vista a componenti è nella sezione 6; la dimensione evolutiva nella sezione 7.

### 1.4 Le due bussole etiche

Due vincoli **di prodotto** (non regole di lavoro) attraversano tutta l'architettura e
sono il metro di ogni scelta di design dei prompt:

- **Anti-invenzione** — nulla che la persona non abbia dichiarato entra negli output.
  Descritto nel `README.md` ("Vincolo etico principale") e codificato nei prompt di
  `prompt_design.md`.
- **Anti-perdita** — il gemello simmetrico: nulla di ciò che la persona dichiara va
  **perso** per essere stato detto nel turno sbagliato. Realizzato col campo `altrove`
  (instradamento ad altri turni + conferma dell'utente). Descritto in `prompt_design.md`
  ("Convenzione anti-perdita: il campo `altrove`") e narrato nel `diario_di_bordo.md`
  (Step 1.26).

L'una vieta di **aggiungere**, l'altra vieta di **togliere**: insieme delimitano lo spazio
in cui il sistema può lavorare sul profilo.

---

## 2. Le funzioni fondamentali

Le funzioni del sistema, nella scomposizione proposta dal tutor. Per ognuna: **cosa fa ·
cosa entra → cosa esce · dove vive oggi · stato**. I prompt e gli schemi non sono ricopiati
qui: il riferimento è la sezione corrispondente di `prompt_design.md`.

### 2.1 Estrazione di profilo

Costruire il **profilo strutturato** (lo schema in sezione 4) a partire da una o più fonti.
Oggi esiste una sola fonte d'ingresso (il dialogo); le altre due sono gap aperti.

| Voce | Cosa fa | Entra → Esce | Dove vive oggi | Stato |
|---|---|---|---|---|
| **2.1.1** Dialogo guidato | Raccoglie il profilo conversando, un argomento per turno, con conferma a vista e anti-perdita | risposte libere dell'utente → **profilo JSON** | Anello 1 (`/struttura`, turni in `prompt_design.md`) | ✅ Completo |
| **2.1.2** Estrazione da CV preesistente | Ricava lo stesso profilo JSON da un CV già pronto (PDF/testo) | file/testo CV → **profilo JSON** | — | ❌ Gap (vedi §8) |
| **2.1.3** Estrazione da LinkedIn / sito web | Ricava lo stesso profilo JSON da un link pubblico | URL → **profilo JSON** | — | ❌ Gap (vedi §8) |

Le tre voci sono **fonti alternative dello stesso artefatto**: qualunque sia l'ingresso,
l'uscita è il medesimo profilo JSON. È questa identità di output a rendere il resto della
pipeline indipendente da *come* il profilo è stato raccolto (vedi sezione 4).

### 2.2 Valutazione del match con l'annuncio

Confrontare il profilo con un annuncio e produrre un giudizio strutturato, un punteggio
orientativo e gli argomenti che colmano i divari (la mitigazione 2.2.4).

| Voce | Cosa fa | Entra → Esce | Dove vive oggi | Stato |
|---|---|---|---|---|
| **2.2.1** Estrazione da annuncio | Struttura il testo dell'annuncio in requisiti + contesto | testo annuncio → **annuncio JSON** | Anello 2 (`/struttura`, "Analisi annuncio" in `prompt_design.md`) | ✅ Completo |
| **2.2.2** Estrazione dal profilo | *(vedi nota sotto)* | — | il profilo JSON dell'anello 1 | ✅ (già disponibile) |
| **2.2.3** Comparazione + forza/debolezza | Giudica voce per voce il match e calcola il punteggio in stelle | profilo JSON + annuncio JSON → **giudizi JSON + punteggio** | Anello 3 (`/confronta`) | ✅ Completo |
| **2.2.4** Mitigazione e sintesi | Per ogni gap, cerca nel profilo un elemento funzionalmente affine e ne costruisce l'argomento | profilo + giudizi (focus su gap) → **mitigazioni JSON** | `/mitiga` (`promptMitigazione` in `server.js`) | ✅ Completo |

**Nota su 2.2.2 ("estrazione dal profilo").** Non è un'estrazione a sé: il confronto
(2.2.3) riceve **direttamente il profilo JSON già strutturato** dall'anello 1, non un testo
da ri-estrarre. Nella traccia del tutor la voce segna il fatto che il match ha **due
ingressi già strutturati** (profilo + annuncio): l'"estrazione del profilo" è avvenuta a
monte, nell'anello 1. Nessun componente nuovo da costruire qui.

**Sulla 2.2.4 (mitigazione).** È un componente **distinto**, da non confondere
con il clamp del punteggio (anello 3). La mitigazione è **bridging argomentativo**: dati i
gap (esiti `non soddisfatto` / `in parte`), l'AI cerca nel profilo un elemento reale
funzionalmente vicino al requisito mancante e costruisce l'argomento esplicito (es. *"non
sono laureato, ma ho una lunga esperienza di programmazione sul campo"*). Resta dentro
l'anti-invenzione: **se l'equivalenza funzionale non esiste nel profilo, tace**. Il suo
output alimenta la generazione: per **scelta di design** lo consuma la **sola ✉️ lettera**
(il bridging ha senso retorico nella lettera, non nel CV); il 🎯 CV-2 resta sobrio e tace
sui gap.

### 2.3 Emissione di documenti per il contatto con l'offerente

Generare i documenti di candidatura. Tutti hanno **il profilo come unica fonte di fatti**;
annuncio e giudizi sono solo il **segnale di mira** (cosa mettere in risalto).

| Voce | Cosa fa | Entra → Esce | Dove vive oggi | Stato |
|---|---|---|---|---|
| **2.3.1** CV base (📄 CV-1) | Trasforma il solo profilo in un CV sobrio | profilo JSON → **CV JSON** (`tipo: cv_base`) | Anello 4 (`/genera-cv`) | ✅ Completo |
| **2.3.2** CV mirato (🎯 CV-2) | CV orientato all'annuncio: stessa verità, enfasi spostata | profilo + annuncio + giudizi → **CV JSON** (`tipo: cv_mirato`) | Anello 4 (`/genera-cv`) | ✅ Completo |
| **2.3.3** Lettera di presentazione (✉️) | Lettera mirata, motivata nel tono ma ancorata ai fatti | profilo + annuncio + giudizi + CV-2 → **lettera JSON** | Anello 4 (`/genera-lettera`) | ✅ Completo |

I due CV si distinguono in modo inequivocabile dal campo `tipo` (`cv_base` /
`cv_mirato`) e in chat dai marker 📄 CV-1 / 🎯 CV-2 (`regole_di_progetto` #9). La lettera usa
il CV-2 solo come **riferimento di coerenza** (stessa storia), mai come fonte di fatti.

---

## 3. Mappa anelli ↔ voci, e il flusso

Il ponte fra i due vocabolari, e l'ordine in cui i componenti si compongono.

| Anello (vocabolario del progetto) | Voce/i (vocabolario del tutor) |
|---|---|
| **Anello 1** — dialogo profilo | 2.1.1 (+ 2.1.2 / 2.1.3 come fonti future dello stesso output) |
| **Anello 2** — analisi annuncio | 2.2.1 |
| **Anello 3** — confronto + punteggio | 2.2.2 (ingresso) + 2.2.3 |
| *(nuovo)* — **mitigazione** | 2.2.4 |
| **Anello 4** — generazione | 2.3.1 + 2.3.2 + 2.3.3 |

**Flusso end-to-end** (con la mitigazione inserita dove mancherà di meno):

```
[2.1.x] fonte profilo ─→ profilo JSON ──┬─────────────────────────────→ [2.3.1] 📄 CV-1
                                        │
              testo annuncio ─→ [2.2.1] annuncio JSON
                                        │
        profilo JSON + annuncio JSON ─→ [2.2.3] giudizi JSON + punteggio (stelle)
                                        │
                          [2.2.4 NUOVO] mitigazione (profilo + gap → argomenti)
                                        │
   profilo + annuncio + giudizi ─→ [2.3.2] 🎯 CV-2 ─→ [2.3.3] ✉️ lettera (+ mitigazioni)
```

Il 📄 CV-1 si dirama già dopo il profilo (non richiede l'annuncio); 🎯 CV-2 e ✉️ lettera
stanno a valle del confronto. La mitigazione si colloca **tra anello 3 e anello 4**: nasce
dai giudizi e arricchisce la **sola ✉️ lettera** (il 🎯 CV-2 resta sobrio).

---

## 4. Vista-dati: gli artefatti (un profilo, molti CV)

La scomposizione per funzioni descrive i **verbi** (estrai, confronta, genera); ma il cuore
di CV-COACH sono i **sostantivi** — le strutture dati che fluiscono. Qui l'architettura è
data-centrica: i componenti sono produttori/consumatori di pochi artefatti JSON ben
definiti.

| Artefatto | Prodotto da | Consumato da | Schema in `prompt_design.md` |
|---|---|---|---|
| **Profilo JSON** | 2.1.x (oggi: anello 1) | 2.2.3, 2.2.4, 2.3.1, 2.3.2, 2.3.3 | "Struttura dati del profilo utente" |
| **Annuncio JSON** | 2.2.1 (anello 2) | 2.2.3, 2.3.2, 2.3.3 | "Analisi annuncio di lavoro" |
| **Giudizi + punteggio** | 2.2.3 (anello 3) | 2.2.4, 2.3.2, 2.3.3 | "Confronto profilo-annuncio" |
| **Mitigazioni JSON** | 2.2.4 | 2.3.3 (✉️ lettera) | "Mitigazione e sintesi (2.2.4)" |
| **CV JSON** (base / mirato) | 2.3.1 / 2.3.2 | front-end (impaginazione); CV-2 → 2.3.3 | "Generazione del CV" |
| **Lettera JSON** | 2.3.3 | front-end (impaginazione) | "Generazione lettera di presentazione" |

Due proprietà architetturali che da qui emergono e che la vista per-funzioni non mostra:

- **Un profilo, molti CV.** Il profilo è la **fonte di verità unica e riusabile**: da esso
  nascono il CV-1, e — per ogni annuncio — un CV-2 e una lettera diversi. Il profilo **non
  è un CV** (il riepilogo a fine dialogo è leggibile, ma resta il dato sorgente); i CV sono
  *proiezioni* mirate dello stesso profilo. Questa separazione è ciò che permette di mirare
  senza mai duplicare o alterare la verità di base.
- **Il profilo come hub disaccoppiante.** Poiché 2.1.1/2.1.2/2.1.3 producono **lo stesso**
  profilo JSON, tutto ciò che sta a valle è indifferente alla fonte. Aggiungere una nuova
  fonte (CV preesistente, LinkedIn) **non tocca** il match né la generazione: è il valore
  architetturale dello schema condiviso.

**Fonte di fatti vs segnale di mira** — distinzione portante della generazione: nel CV-2 e
nella lettera **solo il profilo è fonte di fatti**; annuncio, giudizi (e domani le
mitigazioni) dicono *cosa evidenziare*, non *cosa è vero*. È l'anti-invenzione applicata al
flusso dei dati.

---

## 5. Principi architetturali trasversali

Scelte che non appartengono a una singola funzione ma le attraversano tutte. Sono il "DNA"
del sistema; ogni nuovo componente dovrebbe ereditarle.

1. **Output strutturato (JSON) come unico formato di scambio.** Ogni anello parla agli
   altri in JSON conforme a uno schema. Rende il sistema componibile e verificabile, e la
   "memoria" del profilo vive nel **codice**, non nel modello.
2. **Compito ristretto = meno spazio per inventare.** Ogni prompt fa **una** cosa (un
   turno, un'estrazione, un confronto, una generazione). Restringere il compito è esso
   stesso una difesa anti-invenzione, oltre che la premessa per decomporre i prompt in
   futuro.
3. **Architettura ibrida: l'LLM comprende, il codice rende consistente.** Esemplata
   nell'anello 3 — l'LLM dà i giudizi per-voce e un numero d'insieme; il **codice** calcola
   il punteggio deterministico (stessi giudizi → stesso voto) e riconcilia il numero
   dell'LLM con un clamp limitato e asimmetrico. Il giudizio semantico al modello, la
   consistenza al codice.
4. **Due modelli per livello di compito.** Haiku per l'**estrazione** (anelli 1-2, compiti
   meccanici); Sonnet per il **ragionamento** (confronto dell'anello 3 e generazione
   dell'anello 4). Default Haiku; il ragionamento profondo si attiva esplicitamente.
   Dettaglio in `prompt_design.md` ("Modelli usati").
5. **Le due bussole etiche nel design dei prompt.** Anti-invenzione e anti-perdita
   (sezione 1.4) non sono controlli a posteriori: sono **cablate nei prompt** (guardie
   incrociate fra turni, default sicuro per i campi vuoti, distinzione `non soddisfatto` vs
   `non determinabile`, campo `altrove`).
6. **Normalizzazione leggera, mai traduzione.** Quando l'AI ripulisce le parole
   dell'utente o dell'annuncio, riordina la **forma** senza toccare il **contenuto**:
   niente gergo professionale, niente significati aggiunti o tolti.

---

## 6. Vista runtime / componenti

I componenti fisici e i confini fra loro (chi parla con chi, dove passano i dati).

```
┌─────────────┐   risposte / annuncio / richieste   ┌──────────────────┐   prompt   ┌─────────┐
│  FRONT-END  │ ─────────────────────────────────→  │  AIUTANTE NODE   │ ────────→  │   LLM   │
│ (browser)   │ ←─────────────────────────────────  │  (server.js)     │ ←────────  │ (Claude)│
│ index.html  │            JSON strutturato          │  custodisce la   │   JSON     └─────────┘
└─────────────┘                                      │  chiave API      │
   impagina CV/lettera,                              └──────────────────┘
   tiene lo stato del dialogo                          endpoint HTTP:
   e il magazzino `pending`                            /struttura · /confronta
   (anti-perdita)                                      /genera-cv · /genera-lettera
```

- **Front-end** (`index.html`, impalcatura usa-e-getta): conduce il dialogo, tiene lo
  **stato** (turni, conferme, magazzino `pending` dell'anti-perdita), impagina gli output.
  Consuma il campo `altrove` instradando i frammenti ai turni giusti.
- **Aiutante Node** (`server.js`): unico a conoscere la chiave API; espone gli endpoint,
  inserisce i dati nei prompt, valida/estrae il JSON di risposta. Non tiene stato di
  sessione: è un tramite.
- **LLM** (Claude): esegue i prompt; non ha memoria fra le chiamate (la memoria è nei dati
  che il front-end conserva e ripassa).

I **confini** del sistema sono gli **endpoint**: `/struttura` (anelli 1 e 2),
`/confronta` (anello 3), `/mitiga` (mitigazione 2.2.4), `/genera-cv` e `/genera-lettera`
(anello 4). Per la 2.2.4 si è scelto un **endpoint dedicato** `/mitiga` (input profilo +
giudizi), non l'estensione di `/confronta`: un confine per compito.

---

## 7. Vista evolutiva (MVP → VB.NET)

Il progetto attraversa due fasi tecnologiche, e il disegno tiene esplicita la linea fra ciò
che **migra** e ciò che è **impalcatura**:

| | Asset **durevole** (migra a VB.NET) | **Impalcatura** usa-e-getta |
|---|---|---|
| Cosa | **Prompt + schemi** (`prompt_design.md`) | `index.html`, l'aiutante Node, i `test-*.html` |
| Perché | Sono il valore: codificano logica e bussole etiche, indipendenti dallo stack | Servono a far girare l'MVP nel browser; spariscono nella fase target |

Nella **fase target**, l'app VB.NET unica assolve sia il front-end sia la chiamata diretta
all'LLM, rendendo superfluo l'aiutante Node. Implicazione architetturale: i confini della
sezione 6 (gli endpoint) sono un dettaglio dell'MVP; ciò che resta invariante è la
**pipeline di artefatti** (sezione 4) e i **prompt** che la realizzano. Progettare i gap
(Fase B) sul piano di prompt+schema — non di codice — è coerente con questa linea.

---

## 8. I gap aperti

I componenti previsti dal disegno ma non ancora costruiti. Qui se ne fissa solo
**identità e stato**: la **progettazione** (prompt + schema) è la Fase B; l'**adattamento
del codice** la Fase C.

La **2.2.4 (mitigazione)** — primo gap — è ora **chiusa**: progettata, cablata (endpoint
`/mitiga`, lettera a 5 blocchi) e provata end-to-end (vedi §2.2.4). Restano aperti:

| Voce | Cosa manca | Complessità | Note / puntatori |
|---|---|---|---|
| **2.1.2** Estrazione da CV preesistente | Parsing di un CV (PDF/testo) → stesso profilo JSON | **Alta** | Si innesta sull'hub-profilo (sezione 4): a valle nulla cambia |
| **2.1.3** Estrazione da LinkedIn / web | Fetch di un link pubblico → stesso profilo JSON | **Alta** | Come sopra; attenzione a robustezza del fetch e dati personali |

**Fuori perimetro ora:** il **multi-annuncio** (un profilo confrontato con più annunci in
una volta) — prospettiva futura, non gap dell'MVP.

Altri raffinamenti già annotati (estensione del profilo a specchio di `altri_requisiti`,
turno contatti, pending_questions, riordino/omissione nel CV-2, soglia di match,
robustezza dell'estrazione JSON, ecc.) sono nel **backlog ragionato** `idee_future.md`: non
si duplicano qui.
