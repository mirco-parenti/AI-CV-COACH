# Idee future — AI-CV-COACH

Raccolta **unica** dei raffinamenti e delle idee per le fasi successive del progetto.
Non è codice né stato corrente: è il **backlog ragionato**. Lo **stato attuale** della
pipeline sta nel `README.md` (sezione *Stato*) e nell'ultimo `### Step` del
`diario_di_bordo.md`.

*Come si compila questo file (modalità):* una voce per idea, raggruppata per area; ogni
voce dice **cos'è · perché è futura · dove se ne parla** (puntatore al `diario_di_bordo.md`,
ai capitoli di `VB.NET/progetto/` o al prompt del pool interessato; le voci più vecchie
puntano a `prompt_design.md`, che è la stessa cosa detta quando la casa dei prompt era lì).
Le idee nuove si annotano con **data e motivo**. Le idee **realizzate** si spuntano (✅) e,
quando si accumulano, migrano nella sezione **«Realizzate»** in fondo: così il backlog
attivo qui sopra resta **solo-futuro** e non induce in errore. Aggiornato con "aggiorna-tutto".

## Gap del disegno top-down (Fase B/C)

Componenti previsti dall'architettura ma **non ancora costruiti**, identificati formalmente
nel disegno top-down. Dei tre individuati, la **2.2.4 (mitigazione)** e la **2.1.2 (import da
CV)** sono realizzate (vedi «Realizzate»); resta l'ultima fonte di estrazione. Il dettaglio
(cosa entra → esce, complessità, dove si innesta) è in `architettura.md` §8: qui resta solo il
puntatore.
- **Estrazione da LinkedIn / sito web (2.1.3)** — fetch di un link pubblico → stesso profilo
  JSON. Complessità alta. *(2026-06-15 — formalizzato nel disegno top-down; architettura.md §8.)*

Fuori perimetro ora: il **multi-annuncio** (un profilo confrontato con più annunci insieme)
— prospettiva futura, non gap dell'MVP. *(architettura.md §8.)*

## Front-end & pipeline
- **`estraiFrammento` (front-end) robusto al preambolo**: il gemello front-end di
  `estraiJson` — in `index.html`, per leggere i frammenti — ha ancora il limite che lato
  server è stato chiuso (Step 1.35): toglie il **recinto** ```` ```json ```` ma non un
  eventuale **preambolo in prosa** prima del JSON. Stesso ripiego possibile (primo `{` …
  ultimo `}`). Impalcatura usa-e-getta, bassa priorità (i frammenti del front-end partono
  in pratica già con `{`). *(2026-08-04 — il buco lato server è chiuso, Step 1.35; resta il
  gemello front-end.)* **Non si farà più**: dal 2026-08-06 il prototipo è congelato, e nella
  fase desktop di estrattori ce n'è **uno solo** — `EstrattoreJson`, portato a T2 con i sei
  casi dello Step 1.35 (cap. 15.5 la dà per assorbita). Resta qui come storia.
- **Import da CV (2.1.2) — raffinamenti**: l'import è realizzato (vedi «Realizzate»); restano
  aperti: (a) **PDF scannerizzati / immagine** — oggi danno poco o niente testo, si offre
  l'incolla-testo come ripiego; OCR o lettura **multimodale** del PDF sono rimandati; (b)
  ~~**trascrizione su Sonnet**~~ **valutata sui dati e archiviata (2026-08-04)**: il passo 1
  su Haiku legge il PDF come blocco `document` (comprensione visiva, non estrazione lineare);
  provato su due CV **a due colonne** (uno con colonne allineate riga per riga, il caso-trappola)
  → trascrizione **pulita**, ordine per colonna, nessun interlacciamento né perdita. Haiku basta:
  **non** si sale a Sonnet (sarebbe costo senza beneficio). Se un domani un layout estremo esce
  sporco, il test si rifà in pochi minuti; resta comunque fuori il caso **PDF scannerizzati/immagine**
  del punto (a). (c) ~~**editing campo-per-campo** del profilo importato~~ — **realizzato a
  T3c** nella scheda P2 (vedi «Realizzate»): la singola voce ora si corregge, senza
  ricominciare; (d)
  **limite dimensione del PDF** — nessun tetto esplicito lato server (l'API rifiuta comunque oltre
  ~32 MB). *(2026-08-03 — emersi realizzando 2.1.2, diario Step 1.33; (b) valutata 2026-08-04.)*
- **Fonte-link per l'annuncio → browser incorporato (WebView2)**: leggere l'annuncio da un **link**
  invece di incollarne il testo (anello 2, fonte alternativa gemella dell'import da CV). Il
  meccanismo `web_fetch` dell'API è stato **provato e accantonato**: apre solo le pagine a **HTML
  statico** (career/ATS), ma **non** i portali renderizzati in **JavaScript** o dietro login/anti-bot
  (LinkedIn, Indeed, Infojobs), cioè quasi tutti quelli reali — nell'MVP risultava inutile, quindi
  rimosso. Pista pulita per la **Fase VB.NET**: un **WebView2** (browser Edge/Chromium nativo di
  Windows 11) in cui l'utente naviga e si logga **come sé**; l'app legge il **DOM già renderizzato**
  della pagina — JS risolto (è un vero browser) e muro anti-bot aggirato (sessione reale dell'utente:
  nessuno scraping, nessun ToS violato). A valle nulla cambia: il testo estratto va ad
  `analisi_annuncio` (invariato), come per l'incolla-testo. Lo stesso meccanismo abiliterebbe anche
  la **2.1.3** (profilo da LinkedIn). *(2026-08-04 — provato `web_fetch` sull'annuncio-da-link:
  limite JS, reso alla Fase VB.NET.)*

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
  la patente. Da soppesare con la sensibilità del dato. Dal **Pool 1.02** *(2026-08-09)*
  «la città» è senza ambiguità il **domicilio, una sola**: il giorno in cui si confronterà,
  il dato raccolto è già quello giusto.
  *(2026-06-17 — domicilio raccolto come recapito, Step 1.30-1.32; la confrontabilità è futura.)*
- **Le lingue come campo proprio del profilo** (`lingue: [{lingua, livello}]`), confrontabile
  come la patente. Oggi **non hanno un posto**: nessun prompt del pool le nomina, così
  «Inglese B2» finisce fra le competenze o svanisce a seconda del giro — misurato a T3 su
  cinque letture dello stesso CV: 3, 0, 2, 2, 2 lingue. È lo stesso mattone della patente
  applicato a un dato che gli annunci chiedono di continuo, ed è il seguito naturale
  dell'«estensione a specchio di `altri_requisiti`» qui sopra. Il **rimedio immediato** —
  dare alle lingue un posto nei prompt, come competenze — è stato **fatto col Pool 1.02**:
  le lingue si riportano come dette, mai con un livello inventato, e sul CV reale arrivano
  3 su 3 dove il prototipo ne perdeva 3 su 3. Quando arriverà il campo proprio, le stringhe
  messe fra le competenze diventeranno un tappabuchi da rimuovere a mano.
  *(2026-08-09 — emersa dal collaudo di tappa di T3; rimedio immediato realizzato lo stesso
  giorno con la revisione adversariale, Pool 1.02.)*
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
  **Il caso reale è arrivato a T3** *(2026-08-09)*: nel dialogo con l'AI vera il «patentino
  per il muletto» finisce fuori tutte le volte — il turno della patente lo instrada alle
  competenze, quello delle competenze lo rimanda alla formazione, e la guardia lo scarta
  dichiarandolo. Il meccanismo fa esattamente il suo mestiere e il risultato è comunque
  sbagliato. Una domanda sola all'utente — «questo dove lo metto?» — lo salverebbe. Da
  soppesare con il rimedio più economico, che è dare a quel genere di qualifica un posto
  nei prompt (v. `in_sospeso.md`): il primo costa un turno in più, il secondo tre righe.
  **Il rimedio economico è stato fatto** *(2026-08-09, Pool 1.02)*: i patentini
  professionali hanno ora una casa dichiarata (formazione) in tutti i prompt, e nel
  collaudo reale il muletto atterra al primo colpo. L'idea della collocazione manuale
  resta per i contenuti di confine veri, quelli che nessuna casa dichiarata può prevedere.
- **Rifiniture MVP rimandate**: rimozione di singole competenze, routing a linguaggio
  naturale (oggi bottoni). L'**editing campo-per-campo** è realizzato (vedi «Realizzate»).
  *(Diario Step 1.8.)*
- **Formato del riepilogo leggibile** del profilo: da decidere insieme all'interfaccia.
  *(prompt_design.md, turno formazione, "Sospeso".)*

## Match & punteggio (anello 3)
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

## Fase VB.NET (applicazione desktop)

Idee emerse **scrivendo il progetto dettagliato** della Fase 3 (`VB.NET/progetto/`) e lì
dichiarate «rimandate» (cap. 15.3-15.4): qui il puntatore, perché il backlog resta uno.
- **Auto-update dell'applicazione** — per un'app personale è complessità senza guadagno;
  aggiornamento manuale (si sostituisce l'exe). *(2026-08-05 — progetto VB.NET, cap. 13.8/15.3.)*
- **Firma del codice** (certificato, per evitare l'avviso SmartScreen) — quando l'app
  circolerà oltre il portfolio. *(2026-08-05 — cap. 13.6/15.3.)*
- **MCP: trasporto HTTP locale + tool di scrittura/invio con conferma** — la prima versione
  espone via stdio solo lettura e generazione; email e modifica del profilo restano
  nell'app. *(2026-08-05 — cap. 09/15.3.)*
- **Follow-up assistito delle candidature** — oggi solo promemoria passivo nel registro;
  la generazione dell'email di sollecito è da valutare. *(2026-08-05 — cap. 15.4.)*
- **Multi-profilo nella stessa installazione** — il disegno è mono-profilo; da valutare
  se serve. *(2026-08-05 — cap. 15.4.)*
- **Terze lingue oltre IT/EN** — il pool le ammette per costruzione (varianti `.fr.md`…);
  fuori perimetro. *(2026-08-05 — cap. 10.5/15.3.)*

Idee emerse dalla **revisione adversariale** *(2026-08-09)*: difetti veri ma rimandati con
motivo, o rifiniture che farebbero salire l'app di classe.
- **Annullabilità del turno di dialogo** — a T3 deciso «non annullabile: due secondi non
  valgono il prezzo» (Step 2.6); la revisione ha mostrato il caso brutto: su rete degradata
  un turno può tenere P5 bloccato fino a ~4 minuti (timeout 120 s × 2 tentativi) senza via
  d'uscita. Serve il gettone d'annullo come l'import. *(2026-08-09 — revisione adversariale;
  è la prima voce «per un'app di classe superiore» del resoconto.)*
- **Validazione di range della taratura** — la revisione ha reso robusto il *formato*
  (mappa storta → si scarta intera, valgono i predefiniti), ma i *valori* restano non
  validati: un `"clamp_su": -50` entra zitto e falsa le stelle. *(2026-08-09 — revisione
  adversariale.)*
- **Guardia anti-injection su `confronto`, `mitigazione` e `trascrizione_pdf`** — il Pool
  1.02 la mette nei sette turni del dialogo; su `confronto` e `mitigazione` romperebbe la
  parità carattere per carattere col prototipo, che è il metro della non-regressione: è una
  **decisione**, non una toppa, e per ora la parità vale di più. *(2026-08-09 — revisione
  adversariale.)*
- **Rifiniture UX di P2 e import** — Ctrl+S per salvare il profilo dalla tastiera; un
  indicatore visivo di avanzamento durante l'import (oggi solo testo). *(2026-08-09 —
  revisione adversariale.)*
- **«Madrelingua» perso sull'italiano nell'import** — il CV vero dichiara l'italiano
  madrelingua e la voce non entra fra le competenze; filo minore del caso lingue.
  *(2026-08-09 — revisione adversariale.)*
- **La trappola latente di `Sigilla`** — se un domani il CHANGELOG del pool contenesse una
  riga `---`, il sigillo la scambierebbe per una chiusura di frontmatter; oggi non succede,
  ma è il genere di trappola che scatta fra un anno. *(2026-08-09 — revisione adversariale.)*

## Collaudi e non-regressione (Fase VB.NET)

Idee emerse **costruendo la batteria di T2** (cap. 14), quando il prototipo ha fatto da
giudice per la prima volta.
- **Parità del prompt estesa a tutto il pool** — oggi il banco verifica carattere per
  carattere un prompt solo, quello del **confronto**: è il più esigente (due artefatti
  iniettati, accenti, JSON annidato) e quello su cui una differenza si vedrebbe. Degli
  altri quattordici il banco controlla che si carichino e che i metadati siano quelli
  giusti, **non** che il testo sia ancora parola per parola quello del prototipo.
  Estendere il generatore degli attesi a tutti sarebbe meccanico — i turni del profilo
  hanno un segnaposto solo — e chiuderebbe l'ultimo pezzo affidato alla rilettura.
  *(2026-08-07 — emersa chiudendo la non-regressione di T2.)*
- **Un confronto reale fra lingue diverse** — profilo in italiano, annuncio in inglese: il
  cap. 10.2 lo dà come caso di collaudo del multilingua, ma i due casi reali di T2 sono
  entrambi in italiano. Da aggiungere quando arriverà T7, dove serve davvero.
  *(2026-08-07 — emersa a T2, rimandata alla tappa che la riguarda.)*
- **Un banco che pilota l'interfaccia vera** — la prova a video di T3 è stata fatta guidando
  l'applicazione dall'esterno con gli appigli di accessibilità di Windows (gli stessi che usa
  un lettore di schermo): l'app si avvia, il dialogo si conduce nel pannello, il profilo si
  salva, e lungo la strada si catturano le schermate. Ha funzionato, e ha trovato cose che il
  banco non vede. Oggi però è uno **script usa-e-getta fuori dal repo**: farlo diventare un
  collaudo vero vorrebbe dire decidere dove vive, come si lancia e — soprattutto — che cosa
  *asserisce*, visto che il giudizio su come una schermata appare resta di chi guarda. Da
  valutare quando i pannelli saranno più d'uno o due. *(2026-08-09 — emersa dalla gamba C del
  collaudo di tappa di T3.)*
- **Città pass/fail anche in `CollaudiFormatiReale`** — con il Pool 1.02 la città è tornata
  un verdetto secco nel collaudo reale dell'import (`CollaudoReale`); il banco gemello dei
  quattro formati usa ancora il criterio vecchio. Allineamento meccanico. *(2026-08-09 —
  revisione adversariale.)*

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
- ✅ **Import da CV in PDF (2.1.2)** — seconda fonte del profilo, alternativa al dialogo: Claude
  legge il PDF e ne trascrive il testo (endpoint `/leggi-pdf`, passo 1), poi il turno `importa_cv`
  lo struttura nello **stesso profilo JSON** dell'anello 1 (passo 2), con conferma dell'utente.
  Prompt in `prompt_design.md`, cablato in `server.js` e integrato in `index.html` (bivio iniziale
  dialogo/import); banco `test-cv-import.html`. Entrambi i passi su Haiku.
  *(2026-08-03; diario Step 1.33; architettura.md §2.1/§6/§8.)*
- ✅ **Turni contatti + patente (anello 1) + patente confrontabile** — due turni distinti:
  `contatti` (email, telefono, città, link) e `patente` (domanda dedicata, con ri-domanda
  della categoria e default «non posseduta» se l'utente conferma senza dichiararla). Il campo
  `patente: { ha, categorie }` è **confrontabile** nell'anello 3 (esce da `non determinabile`);
  recapiti e patente nell'intestazione di 📄 CV-1 / 🎯 CV-2 e nella firma della ✉️ lettera.
  **Primo mattone** del "profilo a specchio di `altri_requisiti`".
  *(2026-06-17; diario Step 1.30-1.31; prompt_design.md, turni `contatti`/`patente` e schema CV/lettera.)*
- ✅ **Soglia di match prima di generare** — dopo il confronto (anello 3), sotto **1,5 stelle**
  su 5 il front-end **sconsiglia** (senza impedire) la generazione di 🎯 CV-2 e ✉️ lettera:
  avviso onesto + due scelte («Genera comunque» / «Mi fermo qui»), scelta finale all'utente.
  Costante `SOGLIA_STELLE_GENERAZIONE` in `index.html`. *(2026-08-04; diario Step 1.35.)*
- ✅ **`estraiJson` robusto al preambolo (lato server)** — la funzione usata da tutti gli
  endpoint ora, se il `JSON.parse` fallisce per prosa attorno al JSON, ripiega ritagliando dal
  primo `{` all'ultimo `}` (percorso felice invariato; se neanche così è valido, rilancia
  l'errore). Chiude i 502 quando il modello «si mette a spiegare». Resta aperto il gemello
  front-end `estraiFrammento` (vedi backlog «Front-end & pipeline»). *(2026-08-04; diario Step 1.35.)*
- ✅ **Editing campo-per-campo del profilo** — la scheda P2 dell'applicazione desktop mostra
  tutte le sezioni del profilo **campo per campo, modificabili**: si corregge la singola voce
  invece di riconfermare o ricominciare. Chiude insieme il punto (c) dell'import da CV e una
  delle «rifiniture MVP rimandate» del dialogo, e vale per il profilo qualunque sia la porta
  da cui è entrato — dialogo o CV. Il salvataggio resta un gesto esplicito dell'utente e
  **versiona**: ogni conferma lascia la sua copia datata nello storico.
  *(2026-08-07, T3c; diario Step 2.6; `VB.NET/progetto/03` P2 e `11.1`.)*
- ✅ **Hard-gate (requisito eliminatorio)** — un requisito *davvero* squalificante non soddisfatto
  non pesa soltanto: mette un **tetto** al match. Ogni giudizio del confronto porta un flag
  `eliminatorio` (booleano, deciso dall'LLM: `true` solo per i requisiti tassativi/escludenti, nel
  dubbio `false`); se almeno uno è `eliminatorio` con esito `non soddisfatto`, il codice cratera il
  match a **≤ 20/100 (≤ 1 stella)**, `finale = min(finale, 20)`, con nota esplicita. Deterministico,
  applicato dopo il clamp, in sinergia con la soglia B (≤1 stella → generazione sconsigliata). La
  **patente** è il primo caso reale (patente C richiesta, candidato senza → gate). Prompt+schema in
  `prompt_design.md`, `calcolaMatch` in `server.js`, nota+⛔ in `index.html`, colonna "Elim." nel
  banco `test-confronto.html`. *(2026-08-04; diario Step 1.37.)*
