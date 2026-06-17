# Idee future — AI-CV-COACH

Raccolta **unica** dei raffinamenti e delle idee per le fasi successive del progetto.
Non è codice né stato corrente: è il **backlog ragionato**. Lo **stato attuale** della
pipeline sta nel `README.md` (sezione *Stato*) e nell'ultimo `### Step` del
`diario_di_bordo.md`.

*Come si compila questo file (modalità):* una voce per idea, raggruppata per area; ogni
voce dice **cos'è · perché è futura · dove se ne parla** (puntatore a diario/prompt_design).
Le idee nuove si annotano con **data e motivo**. Le idee **realizzate** si spuntano (✅) e,
quando si accumulano, migrano nella sezione **«Realizzate»** in fondo: così il backlog
attivo qui sopra resta **solo-futuro** e non induce in errore. Aggiornato con "aggiorna-tutto".

## Gap del disegno top-down (Fase B/C)

Componenti previsti dall'architettura ma **non ancora costruiti**, identificati formalmente
nel disegno top-down. Dei tre individuati, la **2.2.4 (mitigazione)** è realizzata (vedi
«Realizzate»); restano i due di estrazione. Il dettaglio (cosa entra → esce, complessità,
dove si innesta) è in `architettura.md` §8: qui resta solo il puntatore.
- **Estrazione da CV preesistente (2.1.2)** — parsing di un CV (PDF/testo) → stesso profilo
  JSON. Complessità alta. *(2026-06-15 — formalizzato nel disegno top-down; architettura.md §8.)*
- **Estrazione da LinkedIn / sito web (2.1.3)** — fetch di un link pubblico → stesso profilo
  JSON. Complessità alta. *(2026-06-15 — formalizzato nel disegno top-down; architettura.md §8.)*

Fuori perimetro ora: il **multi-annuncio** (un profilo confrontato con più annunci insieme)
— prospettiva futura, non gap dell'MVP. *(architettura.md §8.)*

## Front-end & pipeline
- **Estrazione JSON robusta al preambolo**: `estraiJson` (lato server, usato da tutti gli
  endpoint) toglie il **recinto** ```` ```json ```` ma non un eventuale **preambolo in
  prosa** prima del JSON; in quel caso `JSON.parse` fallisce e l'endpoint risponde 502.
  Sui prompt rigidi (`/struttura`, `/genera-cv`) non capita in pratica; può capitare quando
  il modello "si mette a spiegare" (visto su `/confronta` con un annuncio fuori-schema).
  Idea: estrarre il blocco JSON anche con prosa attorno (primo `{` … ultimo `}`). Non è un
  prerequisito (con input ben formati gli endpoint partono già con `{`).
  *(2026-06-11 — emerso testando il 📄 CV-1 e introducendo la validazione JSON lato server.)*

## Profilo, annuncio & schema
- **Estensione del profilo** a specchio di `altri_requisiti` (domicilio, disponibilità,
  patente, automunito, età, iscrizione albo, idoneità): rende ricavabili nel match dati
  che oggi escono `non determinabile`. **Primo mattone realizzato** — la **patente** è
  raccolta e confrontabile (vedi «Realizzate»). Il **domicilio** è ora raccolto come
  **recapito** (campo `citta`) ma **non ancora confrontato**; restano da rendere confrontabili
  domicilio, età, ecc. La **disponibilità** (turni, trasferte) per ora **non si raccoglie**
  (decisione 2026-06-17). **Non è un prerequisito** (`altri_requisiti` è già confrontabile).
  Attenzione alla **sensibilità** dei dati personali (domicilio, età); ogni nuovo dato è un
  possibile turno nell'anello 1. *(Diario Step 1.14 e 1.30-1.32.)*
- **Domicilio confrontabile / automunito**: il **domicilio** è ora chiesto e raccolto nel
  turno contatti (campo `citta`), ma resta **solo recapito, non confrontato**; renderlo un
  dato di match (vicinanza alla sede) e aggiungere l'**automunito** è il passo naturale dopo
  la patente. Da soppesare con la sensibilità del dato.
  *(2026-06-17 — domicilio raccolto come recapito, Step 1.30-1.32; la confrontabilità è futura.)*
- **Campi annuncio aggiuntivi**: `livello` (impiegato/operaio/quadro), `settore` — fuori
  per ora (schema snello). *(prompt_design.md, "Da valutare in futuro".)*
- **Decomposizione dei prompt**: il prompt unico dell'annuncio è già diviso in 5 sezioni
  numerate, pensate per diventare sotto-prompt separati. *(Diario Step 1.10.)*

## Dialogo (anello 1)
- **pending_questions**: accantonare le domande saltate o non strutturabili e riprenderle
  in un secondo giro a fine dialogo. Pianificato, non costruito. È **cugino** dell'anti-perdita
  (Step 1.26) ma caso diverso: lì recupero contenuto dato nel turno sbagliato, qui una
  *domanda* saltata. *(Diario apertura Fase 1, Step 1.2.)*
- **Collocazione manuale degli "esclusi"**: oggi un frammento che nessun turno sa
  strutturare (es. "Vittoria Concorso Servizio Civile 2022", contenuto di confine) viene
  **dichiarato "lasciato fuori"** — terminazione garantita, perdita visibile, niente
  ping-pong (guard anti-rimbalzo **già realizzato**: nello smaltimento non si ri-parcheggia,
  Step 1.26). Idea futura: invece di lasciarlo fuori, far **scegliere all'utente** in quale
  sezione collocarlo (o crearne una voce a mano). *(2026-06-12 — emersa nel collaudo
  headless dell'anti-perdita, Step 1.26.)*
- **Rifiniture MVP rimandate**: rimozione di singole competenze, routing a linguaggio
  naturale (oggi bottoni), editing campo-per-campo. *(Diario Step 1.8.)*
- **Formato del riepilogo leggibile** del profilo: da decidere insieme all'interfaccia.
  *(prompt_design.md, turno formazione, "Sospeso".)*

## Match & punteggio (anello 3)
- **Hard-gate**: un requisito *davvero* squalificante non azzera il punteggio (tetto del
  clamp −20). Trattarlo come tetto rigido che cratera il match è rimandato. La **patente**
  ora confrontabile è il candidato naturale a un futuro hard-gate (oggi pesa solo come
  priorità). *(Diario Step 1.16-1.17 e 1.30; prompt_design.md, "Limite noto".)*
- **Taxonomy mapping (ESCO/O*NET)**: mappare le skill su una tassonomia standard per il
  match. Scartato per l'MVP (il match semantico lo fa l'LLM); utile per analisi su grandi
  volumi, non per il singolo match. Fonti GitHub da riprendere:
  - `nestauk/ojd_daps_skills`
  - `KonstantinosPetrakis/esco-skill-extractor`
  - `amazon-science/job-posting-structure` (componente *SkillsTaxonomyAI*)
  *(Diario Step 1.10-1.11-1.15.)*

## Generazione (anello 4)
- **Riordino dinamico delle sezioni nel CV mirato**: nell'MVP l'ordine delle sezioni è
  **fisso** in entrambi i CV (base e mirato) e il "mirare" avviene nel **contenuto**
  (sommario, dettaglio delle voci); far variare ordine/enfasi delle sezioni in base al
  confronto (anello 3) è rimandato. *(2026-06-09 — deciso nel design dell'anello 4:
  ordine fisso per semplicità e verificabilità.)*
- **Omissione mirata di contenuti off-target nel 🎯 CV-2**: nell'MVP il CV-2 **tiene tutte
  le voci** del profilo e mira solo con l'**enfasi** (cosa il sommario mette in risalto,
  quanto dettaglio dà a una descrizione), a ordine fisso. Poter **omettere** le voci non
  pertinenti all'annuncio renderebbe il CV più incisivo (come i CV mirati veri), ma
  l'omissione è già una *scelta* che complica la verifica 1:1 e apre una porta
  all'anti-invenzione "per sottrazione". Rimandato. *(2026-06-11 — deciso nel design del
  CV-2, bivio 3: tenere tutto, ri-pesare l'enfasi.)*
- **Soglia di match prima di generare**: per un match molto basso (es. ~0 stelle) il
  🎯 CV-2 e la ✉️ lettera escono *onesti ma inutili come candidatura* — la lettera diventa
  una "non-candidatura" che dichiara solo ciò che manca. Idea: sotto una soglia, **avvisare
  l'utente** (o sconsigliare la generazione) invece di produrre comunque, lasciando comunque
  a lui la scelta finale. *(2026-06-11 — emerso nel primo collaudo con il CV reale di Mirco
  contro un annuncio lontanissimo, Operatore Subacqueo, match 0,1 stelle; diario Step 1.25.)*

## Realizzate

Idee del backlog ormai costruite. Si tengono qui (con il puntatore a dove sono narrate o
implementate) per non perdere la storia, fuori dal backlog attivo qui sopra.
- ✅ **Mitigazione e sintesi (2.2.4)** — bridging argomentativo onesto fra anello 3 e anello 4
  (dai gap del match → argomenti di equivalenza funzionale ancorati al profilo). Prompt +
  schema in `prompt_design.md`, cablata in `server.js` (endpoint `/mitiga`) e `index.html`; la
  consuma la **sola ✉️ lettera**. Provata e raffinata (tace sui ponti deboli, niente
  speculazione sul possesso, esclude il `contesto`). *(2026-06-15/16; diario Step 1.28-1.29;
  architettura.md §2.2.4/§6.)*
- ✅ **Integrazione front-end** — i quattro anelli sono un unico flusso in `index.html`
  (dialogo profilo → bivio 📄 CV-1 / annuncio → confronto in stelle → 🎯 CV-2 → ✉️ lettera).
  Le `test-*.html` restano come banchi di prova per-anello. *(diario Step 1.24.)*
- ✅ **Anti-perdita con instradamento (`altrove`) + tirocinio (`tipo`)** — ciò che si accenna
  nel turno sbagliato non si perde più (frammenti verbatim in `altrove`, riproposti e da
  confermare nel turno giusto: in avanti, o nella passata finale all'indietro); uno
  stage/tirocinio è marcato e reso esplicito nel CV senza spacciarlo per impiego.
  *(diario Step 1.26; prompt_design.md, "Convenzione anti-perdita" e nota schema `tipo`.)*
- ✅ **Testo visibile per le esperienze informali** nel CV — sezione `altre_esperienze`
  (`descrizione` che fonde `cosa_facevo`+`con_chi`, più `quando`; mai `ruolo`/`azienda`) con
  la regola anti-promozione, validata in 📄 CV-1 e 🎯 CV-2. *(prompt_design.md, schema CV e
  regole d'uso; diario Step 1.20.)*
- ✅ **Turni contatti + patente (anello 1) + patente confrontabile** — due turni distinti:
  `contatti` (email, telefono, città, link) e `patente` (domanda dedicata, con ri-domanda
  della categoria e default «non posseduta» se l'utente conferma senza dichiararla). Il campo
  `patente: { ha, categorie }` è **confrontabile** nell'anello 3 (esce da `non determinabile`);
  recapiti e patente nell'intestazione di 📄 CV-1 / 🎯 CV-2 e nella firma della ✉️ lettera.
  **Primo mattone** del "profilo a specchio di `altri_requisiti`".
  *(2026-06-17; diario Step 1.30-1.31; prompt_design.md, turni `contatti`/`patente` e schema CV/lettera.)*
