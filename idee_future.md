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
nel disegno top-down. **Tutti e tre sono ora realizzati** — la 2.2.4 (mitigazione), la 2.1.2
(import da CV) e, dal 2026-08-14, la 2.1.3 (il profilo dalla propria pagina): stanno in
«Realizzate», e questa sezione resta come indice di un elenco che si è svuotato. Il dettaglio
(cosa entra → esce, complessità, dove si innesta) è in `architettura.md` §8: qui resta solo il
puntatore.

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
- ~~**Fonte-link per l'annuncio → browser incorporato (WebView2)**~~ — **realizzata a T5a-T5b**
  (vedi «Realizzate»).

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
- **Riordinare a mano le voci di un singolo documento**: R6 (2026-08-24) ha dato all'utente il
  gesto di **togliere** una voce da un CV senza toccare il profilo; il reperto ne nominava un
  secondo, **spostarla**, e non è entrato. Vale la stessa regola — il `cv.json` non si tocca,
  la scelta vive accanto — ma l'impronta da sola non basta: serve un **ordine** da mantenere, e
  va deciso che ne è di una voce che dopo un «Rigenera» non c'è più. Cugino della voce qui
  sopra e da non confondere: lì a riordinare sarebbe il confronto, qui la mano di chi si
  candida. *(2026-08-24 — quinto tempo di T9e; cap. 08.4.)*

## Fase VB.NET (applicazione desktop)

Idee emerse **scrivendo il progetto dettagliato** della Fase 3 (`VB.NET/progetto/`) e lì
dichiarate «rimandate» (cap. 15.3-15.4): qui il puntatore, perché il backlog resta uno.
- **Auto-update dell'applicazione** — per un'app personale è complessità senza guadagno;
  aggiornamento manuale (si sostituisce l'exe). *(2026-08-05 — progetto VB.NET, cap. 13.8/15.3.)*
  *Resta futura, e adesso serve meno: dal **2026-08-27** «Informazioni» ha «Cerca
  aggiornamenti», che chiede a GitHub qual è l'ultima versione pubblicata e apre la pagina da
  cui scaricarla (cap. 13.8). Metà del problema — sapere che è uscita — è risolta; l'altra
  metà, sostituire l'eseguibile da solo, chiede più fiducia di quanta ne serva qui.*
- ~~**Firma del codice**~~ — **migrata in `in_sospeso.md`** *(2026-08-27)*. Stava qui dal 5
  agosto con una condizione dentro: «quando l'app circolerà oltre il portfolio». La revisione
  del giro D dichiara quel momento arrivato e la mette fra le cose **indispensabili**: senza
  un certificato per organizzazione, SmartScreen ferma chiunque scarichi l'eseguibile. Da idea
  che poteva non farsi mai è diventata un debito che va fatto, e allora il suo posto è l'altro
  file (regole 7 e 13). *(2026-08-05 — cap. 13.6/15.3.)*
- **MCP: trasporto HTTP locale + invio con conferma** — la prima versione espone via stdio
  solo lettura e generazione; email e modifica del profilo restano nell'app.
  *(2026-08-05 — cap. 09/15.3.)* **Ridotta da T8c** *(2026-08-19)*: la metà «tool di
  scrittura» è arrivata — `salva_opportunita` ed `esporta_documento` scrivono nella cartella
  dati, sotto il lucchetto del cap. 09.4 — ma restano futuri i due pezzi veri di questa voce,
  cioè il **trasporto HTTP locale** (oggi solo stdio) e l'**invio dell'email via MCP**, che
  vorrebbe una conferma umana che su un canale senza interfaccia non si sa ancora dove
  mettere. Anche la **modifica del profilo** resta fuori di proposito: da MCP il profilo si
  legge e basta.
- **Un nome che distingua il server dalla finestra.** Il server MCP e l'applicazione sono lo
  **stesso eseguibile**, e con un client collegato in elenco processi compaiono due
  `TrovaLavoro.exe` identici: chi ne deve chiudere uno — una persona, o un attrezzo che
  chiude per nome — rischia di spegnere l'altro. Oggi si distinguono solo di sponda, perché
  il server non ha finestra e quindi non ha titolo. Un titolo di processo riconoscibile, o
  un argomento che si veda da fuori, toglierebbe la trappola alla radice invece di
  insegnarla. *(2026-08-21 — emersa dal collaudo di tappa di T8, con Claude Code come client
  MCP vero; cap. 09.)*
- **Follow-up assistito delle candidature** — oggi solo promemoria passivo nel registro;
  la generazione dell'email di sollecito è da valutare. *(2026-08-05 — cap. 15.4.)*
  **Il promemoria passivo è arrivato con T9c** *(2026-08-21)*: la Home dice quali spedite
  aspettano da più giorni della soglia scelta in P8. Resta futura la metà assistita — il
  testo del sollecito lo scrive l'utente, come il cap. 15.3 voleva — e adesso avrebbe dove
  attaccarsi: la candidatura segnalata, con la sua data e la sua bozza già in `email.json`.
- **Anche dopo un colloquio si aspetta** — il promemoria di T9c guarda le sole candidature
  `inviata` senza esito: chi ha fatto il colloquio e non ha più saputo niente esce
  dall'elenco, perché una risposta era arrivata. È una scelta di semplicità («nessun esito
  registrato» = «in attesa»), ma la seconda attesa esiste, e talvolta è la più lunga.
  Servirebbe far partire il conto anche dalla data dell'esito, per il solo colloquio.
  *(2026-08-21 — emersa costruendo T9c; cap. 07.3.)*
- **Insegnare allo strumento di collaudo i menù contestuali e i numerici** — oggi `clic`
  sulle voci di un `ContextMenuStrip` risponde «Premuto» senza premere niente, e di un
  `NumericUpDown` si vedono solo le due frecce: entrambe le cose si aggirano con un clic
  vero del mouse alle coordinate lette su una fotografia, che funziona ma non si legge come
  un collaudo. *(2026-08-21 — pagata a T9c; `strumenti/mcp-collaudi/README.md`.)*
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
  è la prima voce «per un'app di classe superiore» del resoconto.)* **Aggiornata a T7c
  (2026-08-18)**: nello stesso pannello, il **ragionamento** su una candidatura si interrompe
  — c'è il gettone, il bottone «Interrompi» e il testo già arrivato che resta. Quella metà è
  fatta, e non per gentilezza: là si interrompe una risposta, qui si interromperebbe una
  **mossa** della macchina del dialogo, che resterebbe in uno stato che non esiste (cap. 02.6).
  Il meccanismo però adesso c'è ed è collaudato: chi riprenderà questa voce non deve
  inventarlo, deve decidere cosa fare della mossa a metà.
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
- **Il bottone spento che non si distingue da uno acceso** — in `StileApp` un controllo
  spento resta su fondo chiaro sopra uno sfondo quasi bianco: la differenza c'è, ma a
  colpo d'occhio si legge male, e il cap. 03.8 promette che chi guarda capisca **subito**
  cosa può premere. Non è una toppa locale: tocca ogni bottone dell'applicazione, quindi
  è una scelta di design da prendere una volta sola (un grigio più netto? il testo più
  tenue? il bordo che sparisce?). Segnalata a Mirco, in attesa di decisione.
  *(2026-08-10 — emersa a T4c, guardando i pannelli nuovi nella prova a video.)*

Idee emerse **costruendo T5** *(2026-08-12)*: la ricerca degli annunci e la cattura dalla
pagina, cioè il primo pezzo di programma che tocca il mondo esterno per davvero.
- **La cattura non vede dentro un `iframe`** — legge il testo del documento in cima, e quello
  di una cornice annidata è un'altra pagina. Sui portali del primo rilascio la pagina di un
  singolo annuncio non ne usa, e se un domani uno cambiasse idea il difetto si dichiara da
  sé (il testo torna troppo corto e la cattura lo dice, invece di mandare all'analisi
  un'accozzaglia di menù). Leggere anche le cornici è il raffinamento, da fare **se e quando**
  un portale lo renderà necessario. *(2026-08-12 — limite dichiarato in `LettorePagina`.)*
- **Il doppione si riconosce solo sull'indirizzo esatto** — lo stesso annuncio raggiunto per
  due strade con parametri di tracciamento diversi (`?from=serp`, `&utm_…`) conta come due.
  Il confronto è alla lettera apposta, perché sbagliare da quella parte costa un doppione e
  sbagliare dall'altra costa una cattura buona rifiutata; normalizzare l'indirizzo prima di
  confrontarlo è il passo successivo, e vuole conoscere quali parametri sono davvero
  ininfluenti per ciascun portale. *(2026-08-12 — limite dichiarato in
  `ArchivioOpportunita.CercaPerLink`.)*
- **Un portale che cambia forma non lo scopre nessuno** — gli indirizzi dei portali sono dati
  in `ricerche.json`, verificati a mano il 2026-08-12 (ed è così che si è scoperto che
  InfoJobs aveva chiuso). Finché sono quattro va bene; se diventassero molti, servirebbe un
  modo di accorgersene senza riprovarli uno per uno. *(2026-08-12 — emersa verificando i
  portali del primo rilascio, cap. 06.3.)*
- **Ripescare uno scarto dall'interfaccia** — a T5c lo scarto è **terminale**: si conferma, e
  da lì l'applicazione non torna indietro. La cartella però **resta su disco** con tutto
  dentro, quindi il dato per ripensarci c'è già; manca solo il comando, che vorrebbe una sua
  domanda («a quale stato torna, quello di prima?») e un posto dove metterlo, dato che la
  Home è il pannello in cui si guarda e non si decide. Rimandata di proposito il 2026-08-12,
  ragionando con Mirco: un'uscita di sicurezza costruita insieme alla porta rischia di
  togliere peso alla conferma, che è ciò che rende lo scarto una decisione.
  *(2026-08-12 — decisa aprendo T5c; cap. 07.3; diario Step 2.13.)*

Idee emerse **costruendo T5d** *(2026-08-14)*: la pagina che diventa profilo.
- **Un import che fonde invece di sostituire.** Oggi ogni import — da file o da pagina —
  **sostituisce** il profilo intero: è la regola giusta quando la fonte è il proprio CV, ma
  la pagina di un sito è quasi sempre più povera del CV, e sostituire vuol dire perdere. Sul
  profilo vero, provato il 2026-08-14, la differenza era netta: dalla pagina è uscita una sola
  esperienza contro le tre del CV, e nessun recapito, perché LinkedIn quei campi non li
  pubblica. L'applicazione si comporta onestamente — propone e non salva, e il vecchio profilo
  resta su disco finché non si preme «Salva» — ma chi non guarda bene può salvare un profilo
  dimezzato credendo di averlo aggiornato. Il seguito naturale è una **fusione dichiarata**:
  campo per campo, «questo lo tengo, questo lo aggiungo, questo lo sostituisco», che è poi lo
  stesso meccanismo differenziale della **sessione di aggiornamento** (flusso D, cap. 12.4) —
  conviene disegnarli insieme quando arriverà quella. *(2026-08-14 — emersa dal collaudo di
  T5d sul profilo vero; cap. 06.7, cap. 12.4.)*

Idee emerse **costruendo T6** *(2026-08-14)*: l'email di candidatura e la cartella dei
documenti dell'utente.
- **L'assaggio dei PDF, che oggi non c'è.** La classificazione della cartella documenti
  (cap. 05.2) giudica i PDF **dal solo nome**: leggerne il testo vorrebbe dire una
  trascrizione dell'AI per ciascuno, e in una cartella vera i PDF sono quasi tutto. Sul
  campo è bastato — nove file su nove riconosciuti — ma un file dal nome opaco
  (`doc1.pdf`, `scan_0007.pdf`) finisce in «altro» e tocca all'utente ripescarlo. Due
  strade il giorno che servisse: leggere la prima pagina **senza AI** (vuole un lettore di
  PDF, che oggi il progetto non ha e non vuole), oppure trascrivere **solo i dubbi** —
  quelli che il primo giro mette in «altro» dicendo che l'assaggio non bastava. La seconda
  costa poco ed è già preparata dal prompt, che quel caso lo dichiara.
  *(2026-08-14 — decisa costruendo `ScansioneDocumenti`; cap. 05.2.)*
- **Attestati proposti in base all'annuncio.** Oggi gli attestati riconosciuti compaiono
  **tutti** fra gli allegati, spenti, e a scegliere è l'utente. Il cap. 07.1 li chiama
  «pertinenti», e la pertinenza a quel punto è nota: i requisiti dell'annuncio sono già
  stati letti, e un patentino del muletto davanti a un annuncio che lo chiede è una
  risposta, non un file. Farlo vorrebbe dire una passata in più (o un campo in più nella
  classificazione) e una scelta di interfaccia — evidenziare, non spuntare da soli, perché
  l'ultima parola resta di chi si candida. *(2026-08-14 — emersa chiudendo T6; cap. 07.1.)*
- ~~**Il CV più recente della cartella, che nessuno usa ancora**~~ — **realizzata il
  2026-08-19** (vedi «Realizzate»).

Idee emerse **collaudando T7b** *(2026-08-18)*: la passata anti-slop provata con l'AI vera.
- **Una rifinitura che non ha cambiato niente e una che non ci è riuscita si somigliano
  troppo.** Se il modello restituisce il testo identico — che è una risposta **giusta**, e i
  prompt la dichiarano tale — nello `stato.json` non compare nulla, esattamente come quando
  la chiamata è fallita e la pipeline ha tenuto il testo grezzo. Il passo di avanzamento lo
  dice («non ci sono riuscita, tengo il testo com'è»), ma è un messaggio che passa e se ne
  va: chi guarda dopo non lo trova più. Nel collaudo è costato una mezz'ora di dubbio, e ha
  richiesto di leggere il codice della pipeline per escludere il guasto. Il rimedio piccolo
  è annotare l'esito accanto al «prima» (rifinito / invariato / non riuscito); quello grande
  è mostrarlo in P6 accanto alla casella del prima/dopo. Nessuno dei due è urgente finché la
  rifinitura non fallisce quasi mai — ma il giorno che fallisse spesso, oggi non lo si
  vedrebbe. *(2026-08-18 — emersa dal collaudo di T7b; cap. 08.4.)*

Idee emerse dal **collaudo dal vivo di T9e** *(2026-08-23)*: l'applicazione provata a caccia
di difetti prima della 1.0.
- **Riconoscere il testo di un annuncio per densità, non portale per portale.** Sulle
  pagine-risultati la cattura prende lista e dettaglio insieme, in silenzio (reperto R5): per
  la 1.0 si è deciso di riconoscere quella forma di pagina e di preferire il testo
  **selezionato** dall'utente quando c'è. Resta fuori la terza via, che è anche la più
  generale: scegliere il sottoalbero della pagina più «articolo» per densità di testo
  rispetto ai collegamenti — il principio dietro *Readability*, assimilato e non copiato.
  Cambierebbe la cattura su **ogni** portale e andrebbe misurata su ciascuno, che è troppo
  adesso; ma è la cura vera se un domani i portali moltiplicano le pagine che ingannano la
  lettura di oggi. *(2026-08-23 — collaudo dal vivo di T9e, giro C; cap. 06.4;
  `LettorePagina`.)*
- **Spezzare i pannelli monolitici, rompere il ciclo `Dati` ↔ `Motore`, tipizzare gli
  artefatti JSON.** Tre debiti di **struttura** che la revisione del giro D elenca insieme e
  mette per ultimi, come opzionali. Il primo si misura: `Ui/PannelloDocumenti.vb` è di **2188
  righe** (verificato il 2026-08-27), e sotto non ha presentatori che si possano collaudare
  senza aprire una finestra. Gli altri due sono la dipendenza circolare fra `Dati` e `Motore`
  e gli artefatti JSON che viaggiano come testo invece che come tipi. Nessuno dei tre si vede
  usando il programma: sono manutenibilità, cioè il costo della **prossima** modifica, non di
  questa. E il motivo per cui restano qui invece di passare fra i debiti è lo stesso che tiene
  ferme le costanti di `StileApp`: il banco quasi non ha collaudi d'interfaccia, quindi
  rimettere mano a una quindicina di file di UI oggi costerebbe più di quanto protegga.
  *(2026-08-27 — revisione del giro D; cap. 02, cap. 03.)*

Idee emerse dalla **revisione UI di finalizzazione** *(2026-09-01)*: sono le due **deroghe**
dichiarate in quella revisione, cioè rilievi veri che si è deciso di **non** chiudere adesso
e che restano qui come lavoro futuro, con il motivo per cui non è di oggi.
- **L'applicazione è mono-lingua, e la localizzazione è un lavoro a sé.** Tutte le stringhe
  dell'interfaccia stanno **nel codice**, in italiano, e non esiste nessun `.resx`: è una
  scelta e non una dimenticanza — l'utente di riferimento è italiano, e un secondo strato di
  risorse su una quindicina di file di UI costa oggi senza rendere niente. Il giorno che
  servisse un'altra lingua il lavoro è tutto lì e si sa già qual è: estrarre le stringhe in
  risorse, una per file, e far scegliere la cultura all'avvio. Da non confondere con la
  **lingua dei documenti** (IT/EN), che è cosa diversa e c'è già da T7a: quella riguarda ciò
  che l'AI scrive, questa ciò che l'applicazione dice.
  *(2026-09-01 — deroga B8 della revisione UI di finalizzazione; cap. 03, cap. 10.5.)*
- **Le Impostazioni a schede.** P8 è oggi una finestra sola con **sette sezioni** una sotto
  l'altra e lo scorrimento, ed è coerente con quel che quella finestra è — una finestra che
  **fa** e non raccoglie una decisione (cap. 03.4): si scorre, si cambia una cosa, è già
  scritta. Riorganizzarla a **tab** darebbe una mappa più netta a chi ci entra la prima
  volta, ma nasconderebbe sei sezioni su sette a chi cerca una voce di cui non ricorda il
  nome — che è il caso vero — e vorrebbe rifare la disposizione appena curata a DPI alti
  (decisione 15.7). È un raffinamento, non un difetto: si valuta quando le sezioni saranno
  troppe perché lo scorrimento le tenga.
  *(2026-09-01 — deroga B10 della stessa revisione; cap. 03.4, cap. 11.6.)*
- **Rispettare la preferenza di sistema per il movimento ridotto.** Windows ha un
  interruttore («Effetti di animazione») con cui chi soffre di cinetosi, o semplicemente non
  li sopporta, spegne le animazioni: lo scudo d'attesa non lo legge, e la sua comparsa è
  l'unica animazione del programma. Il rilievo U20 lo chiedeva insieme alla soglia
  anti-flash; **la soglia si è fatta, questo no**, ed è una scelta dichiarata: l'attesa vera
  è la ruota che gira, che non si può togliere senza togliere l'informazione, e spegnere la
  sola comparsa cambierebbe pochissimo a fronte di una preferenza che nessuno ha mai chiesto
  qui. Resta un raffinamento di accessibilità vero, e il giorno che si faccia costa una
  lettura di `SystemInformation` e un ramo.
  *(2026-09-01 — metà non approvata del rilievo U20 della revisione UI; cap. 03.8.)*
- **Un token `AvvisoTesto`, gemello di `InformazioneTesto`.** Il giro degli errori (U4) ha
  dato al testo *informativo* un colore leggibile a 9 pt — `InformazioneTesto`, perché
  `Informazione` come inchiostro faceva ≈2,8:1 — e ha lasciato l'ambra `Avviso` ai soli
  badge, dove sta su un fondo e non è testo. Se un giorno servirà **scrivere** un avviso (non
  un errore: qualcosa di intermedio, «attento a questo») quel colore oggi non c'è, e la
  tentazione sarà riusare l'ambra da fondo come inchiostro — che è precisamente il difetto
  curato in `PannelloProfilo.vb`. Il token costa tre righe e una misura di contrasto; si fa
  quando ci sarà la prima riga che lo vuole, non prima.
  *(2026-09-01 — proposta dell'implementatore del blocco F2-3, non fatta; cap. 03.2.)*

## Collaudi e non-regressione (Fase VB.NET)

Idee emerse **costruendo la batteria di T2** (cap. 14), quando il prototipo ha fatto da
giudice per la prima volta.
- **Parità del prompt estesa a tutto il pool** — ~~oggi il banco verifica carattere per
  carattere un prompt solo, quello del **confronto**~~ … *idea **rivista dai fatti** e
  chiusa così com'è (2026-08-10, T4)*. La parità è stata estesa a **`mitigazione`**, e lì
  si ferma: **due** prompt, non tutti e quindici. Il motivo è che «estendere a tutti» era
  meccanico ma sbagliato di premessa — un collaudo di parità ha senso solo dove il
  prototipo è ancora il **metro**, e restano solo `confronto` e `mitigazione` (cap. 04.7).
  Su `importa_cv` (Pool 1.01), sugli otto del profilo (Pool 1.02) e su `analisi_annuncio`
  (Pool 1.03) il distacco è **voluto**, e sui tre della generazione arriverà: inchiodarli
  al testo del prototipo avrebbe trasformato il collaudo in una gabbia, bocciando proprio
  i miglioramenti che vogliamo poter fare. *(2026-08-07 — emersa chiudendo la
  non-regressione di T2; rivista il 2026-08-10 aprendo T4a.)*
- **Un confronto reale fra lingue diverse** — profilo in italiano, annuncio in inglese: il
  cap. 10.2 lo dà come caso di collaudo del multilingua, ma i due casi reali di T2 sono
  entrambi in italiano. Da aggiungere quando arriverà T7, dove serve davvero.
  *(2026-08-07 — emersa a T2, rimandata alla tappa che la riguarda.)*
  **T7 è arrivata e il caso è stato percorso, ma a mano** *(2026-08-15)*: il collaudo di
  tappa di T7a ha portato un annuncio inglese vero (Fedrigoni, da Indeed) contro il profilo
  italiano, e il comportamento è quello voluto — requisiti in inglese, lettura d'insieme e
  ponti in italiano. Nella **batteria** però non è entrato: `CollaudiConfrontoReale` ha
  ancora i suoi due casi, entrambi italiani. Aggiungerne un terzo in inglese costa un file
  di caso e nessuna riga di codice, e renderebbe ripetibile quello che oggi si rifà a mano.
- ~~**Un `aspetta_che` per lo strumento di collaudo**~~ — **realizzato il 2026-08-18**
  (vedi «Realizzate»).
- ~~**Un attrezzo per rispondere alle finestre di messaggio**~~ — **realizzato a T5c**
  (vedi «Realizzate»).
- **Rispondere anche alla finestra di scelta di una cartella** — lo strumento sa pilotare
  la scelta di un **file** (cerca la casella del nome, che nel dialogo di Windows ha un
  identificativo noto), ma la finestra che chiede una **cartella** quella casella non ce
  l'ha: ha una «Cartella:» diversa, e i bottoni si chiamano «Selezione cartella» e
  «Annulla». La legge — dice cosa chiede e che bottoni ha — ma non sa scriverci dentro,
  così il bottone «Documenti da allegare…» di P7 è l'unico di T6 che ho dovuto lasciare da
  premere a mano. *(2026-08-14 — emersa collaudando T6; limite dichiarato nel README dello
  strumento.)*
- **Un collaudo che veda se un'etichetta ci sta nel suo bottone** — il banco confronta il
  testo dei bottoni **carattere per carattere** e resta verde anche quando quel testo, a
  schermo, esce mozzato: la larghezza è un numero fisso nel `Designer` e nessuno la confronta
  con la frase che ci deve stare dentro. Windows Forms sa dire quanto spazio vorrebbe un
  controllo (`PreferredSize`), quindi il controllo costa poco: per ogni bottone dei pannelli,
  «il testo ci sta?». Oggi la verifica l'ho fatta guardando la finestra, che funziona ma non
  si ripete da sé. *(2026-08-14 — emersa rinominando i tre bottoni del Profilo, diario Step
  2.15: l'etichetta nuova del dialogo non entrava nei 200 px e i 465 collaudi non se ne sono
  accorti.)* **Il meccanismo è nato a T7c** *(2026-08-18)*, per lo stesso difetto visto una
  seconda volta: `INomiNuoviCiStannoDentroILoroBottoni` confronta `PreferredSize.Width` con la
  larghezza vera dei due bottoni del brainstorming. Resta l'idea com'era — **estenderlo a tutti
  i pannelli**: oggi la misura c'è dove il difetto è stato visto, non dove potrebbe ripresentarsi.

- **Il seme del profilo di prova versionato accanto agli altri casi** — il CV finto da cui si
  rifà in un minuto il profilo di prova (Crash Bandicoot) vive in `C:\Temp`, fuori dal repo: se
  quella cartella si svuota, il profilo si rifà a mano leggendo il diario. Accanto ci sarebbe
  già il posto giusto — `casi/giro-d/cv_luca_ferrari.pdf` è esattamente la stessa cosa, fatta
  bene: un dato finto e parlante, versionato, che chiunque può rimettere in circolo. Costa un
  file e nessuna riga di codice. *(2026-08-30 — emersa ripulendo le cartelle dati, Step 2.56.)*
- **Un attrezzo che ripulisca una cartella dati dai dati veri** — la ripulitura del 30 agosto è
  stata fatta con uno script usa-e-getta, e le quattro trappole che ha pagato per strada — gli
  escape del JSON che si mangiano una lettera, i confini di parola che non scattano dopo `\n`,
  l'underscore che è carattere di parola, il protocollo preteso in un link che non ce l'ha —
  sono esattamente quelle che si ripagherebbero la volta dopo. Se dovesse servire ancora (una
  cartella nuova, una demo da preparare, una macchina da consegnare) vale la pena che stia in
  `strumenti/` col suo caso di prova, invece che in `C:\Temp`. *(2026-08-30 — Step 2.56.)*

## Realizzate

Idee del backlog ormai costruite. Si tengono qui (con il puntatore a dove sono narrate o
implementate) per non perdere la storia, fuori dal backlog attivo qui sopra.
- ✅ **Riconfrontare una candidatura già confrontata** — «Analizza» prende il suo **quarto
  mestiere** e diventa **«Riconfronta»** quando la candidatura è stata confrontata con un
  profilo che non è più quello: rifà il solo secondo passo sull'annuncio già strutturato,
  senza rileggerlo e senza chiedere all'utente di ricopiare un testo che il programma ha già
  nella sua cartella. Il gesto compare **solo quando ha una ragione** — a parità di profilo il
  confronto direbbe le stesse cose — e mai sulla scartata. Nasce da un uso vero: cambiata la
  **patente**, cioè un requisito **eliminatorio**, le candidature di prima mostravano ancora
  le stelle di allora e non c'era modo di rifarle. Per questo copre anche il caso «profilo
  **cresciuto**», che il progetto aveva scelto di tacere per non dare un avviso a ogni giro:
  fra due salvataggi può cambiare un eliminatorio, e allora quei giudizi non sono vecchi —
  sono un'altra risposta. La cautela annotata qui («quel numero non deve cambiare sotto le
  mani senza che nessuno lo dica») è diventata la **conferma** prima del gesto, che elenca
  cosa viene sostituito e cosa resta; sulla candidatura **già partita** la finestra non si
  apre da sola e la conferma avverte che il punteggio con cui è stata spedita non resterà da
  nessuna parte, perché il registro lo rilegge dalle cartelle invece di conservarlo.
  *(annotata il 2026-08-24, realizzata il 2026-09-02 dopo un giro dal vivo; cap. 03 P4,
  cap. 08; `PannelloOpportunita.DaRiconfrontare`, `ArchivioProfilo.CambiatoDopo`.)*
- ✅ **Omissione mirata di contenuti off-target nel 🎯 CV-2** — realizzata, e in una forma che
  scioglie l'obiezione che l'aveva rimandata. Il dubbio del 2026-06-11 era che omettere fosse
  «già una *scelta*», che complica la verifica 1:1 e apre una porta all'anti-invenzione **per
  sottrazione**: vero finché a scegliere è il modello. A scegliere invece è **l'utente**, voce
  per voce, da «Modifica i testi» — e allora non è una porta aperta ma la stessa libertà che
  ha di riscrivere un testo: dei suoi fatti decide anche quali raccontare a chi. Il documento
  non si taglia (il `cv.json` resta intero, la scelta vive accanto), la voce si riconosce per
  l'impronta dei suoi **fatti** e non per posizione, e il filtro è uno solo per la finestra e
  per il server MCP. Resta futuro il gemello vero dell'idea originaria — l'omissione decisa
  dal **confronto** — e resta futuro il **riordino a mano** (v. «Generazione (anello 4)»).
  *(2026-08-24, R6, quinto tempo di T9e; cap. 08.4; cap. 11.1; `Documenti/VociDelCv.vb`.)*
- ✅ **`pending_questions`** — le domande saltate o rimaste senza risposta si riprendono, una
  volta sola, **prima del riepilogo** e chiedendo il permesso («Vuoi provarci ora?» → *Ci
  provo* / *Lasciamo così*). Vale per i quattro turni-contenuto, gli unici che passano dallo
  stato «niente colto»; i turni singoli restano fuori, perché lì il vuoto si conferma, si
  rilegge nel riepilogo e si corregge dall'editing del profilo. Resta **cugina**
  dell'anti-perdita (Step 1.26) e non la stessa cosa: lì si recupera *contenuto* dato nel
  turno sbagliato, qui una *domanda* a cui non si era risposto — e infatti una domanda che
  l'anti-perdita ha già riempito non viene richiesta. È agganciata alla stessa passata finale,
  con la stessa disciplina del tentativo unico che la fa convergere. *(2026-08-22, terzo tempo
  di T9e: era la voce 4 della checklist «Problemi e mitigazioni», ratificata fuori dalla 1.0
  al mattino e costruita il pomeriggio; cap. 14, sezione T9; `DialogoProfilo.RiprendiSaltateAsync`.)*
  **E il 2026-08-24 la voce 4 si completa**, col quinto tempo: quella sopra era la metà del
  **turno saltato**, questa è la metà della **voce mezza vuota**. Una voce entrata con vuoto un
  campo che pesa nel CV — *ruolo, durata, cosa facevo* per un'esperienza formale, *cosa facevo*
  per una informale, il *titolo* per la formazione — se lo sente chiedere **in linea**, subito
  dopo la conferma, sul modello della ri-domanda della categoria patente, che era già questa
  stessa cosa fatta per un turno solo. Stessa disciplina della gemella: **occasione unica** (la
  voce esce dall'elenco quando la domanda è offerta, non quando riesce) e **terminazione** (della
  risposta si prende solo il campo che mancava, mai una voce nuova). E niente entra a metà: la
  risposta completa il frammento *prima* che la voce entri nel profilo. *(cap. 12.2;
  `DialogoProfilo.ProssimoApprofondimento`.)*
- ✅ **Il CV più recente della cartella, che nessuno usava** — la porta «qui c'è tutto» del
  profilo (cap. 05.2) è aperta: premendo «IMPORTA CV DA UN FILE» il programma **propone per
  nome** il CV che la classificazione aveva già indicato come il più aggiornato, e lascia tre
  uscite — usalo, scelgo io un altro file, lascia stare. Il ragionamento c'era da T6 e il dato
  pure, salvato in `documenti.json`: mancava **la riga che andasse a prenderlo**, ed è tutto
  ciò che separava un lavoro fatto da un lavoro che arriva all'utente. Si propone e non si
  prende, perché la conferma umana è il passo 4 del capitolo; e che quel file esista si guarda
  **al momento di proporlo**, non quando lo si era classificato, perché nel frattempo può
  essere stato spostato o buttato. *(2026-08-19, passata sui debiti prima di T9; diario Step
  2.35; `RaccoltaDocumenti.PercorsoDelCvPiuRecente`, `PannelloProfilo.CvDaImportare`.)*
- ✅ **Un `aspetta_che` per lo strumento di collaudo** — l'attrezzo aspetta una condizione
  invece di far alternare `clic` e `controlli` a mano, con un tetto di tempo e un esito che
  distingue in chiaro «soddisfatta dopo N secondi» da «TIMEOUT». Ha **due modalità**, e la
  seconda è quella che conta: lo **stato di un controllo** (acceso/spento) e il **contenuto
  di un file** (che compaia, che cambi, che contenga una stringa). Il backlog l'aveva
  previsto — «la condizione giusta è il contenuto di un file, non un controllo che torna
  premibile» — e il motivo è rimasto vero: «Rigenera» è acceso sia prima sia dopo il clic,
  quindi aspettarne lo stato si soddisfa subito senza che il lavoro sia finito. È scritto
  nella descrizione dell'attrezzo e nelle trappole del suo README, perché è il modo naturale
  di usarlo male. Il ciclo gira **dentro** una sola invocazione di PowerShell: avviarlo a
  ogni tentativo costerebbe più dell'attesa stessa. *(2026-08-18; nata a T4c, maturata a
  T7b; `strumenti/mcp-collaudi/README.md`.)*
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
- ✅ **Un banco che pilota l'interfaccia vera** — è diventato il **server MCP di collaudo**
  (`strumenti/mcp-collaudi/`, cap. 09.1), nato a T4c dal giorno in cui «i bottoni non fanno
  nulla» non si poteva diagnosticare dal banco. Le tre domande aperte hanno avuto risposta:
  **dove vive** — nel repo, fuori da `VB.NET/`, perché non è parte del prodotto; **come si
  lancia** — un server locale con attrezzi scritti uno per uno, nessun comando arbitrario;
  **cosa asserisce** — niente, di proposito. Non è un collaudo: è uno strumento che *guarda*
  (compila, avvia, fotografa, elenca i controlli dicendo se sono accesi, preme, scrive,
  risponde alla finestra di scelta file) e lascia il giudizio a chi guarda, che era proprio
  il nodo per cui l'idea era rimasta ferma. Col suo aiuto il giro completo di T4 è stato
  percorso dall'interfaccia, senza mani. *(2026-08-10, T4c; cap. 09.1 e 13.7.)*
- ✅ **Fonte-link per l'annuncio → browser incorporato (WebView2)** — l'annuncio si prende
  dalla **pagina**, non più solo dal testo incollato. Il `web_fetch` dell'API era stato provato
  e accantonato nell'MVP: apriva le pagine a HTML statico, ma non i portali renderizzati in
  JavaScript o dietro login e anti-bot, cioè quasi tutti quelli veri. La pista indicata allora
  era un browser dentro l'applicazione, ed è quella che si è percorsa: **T5a** ha messo la
  WebView2 in piena vista nel pannello Ricerca, dove l'utente naviga e si logga **come sé**;
  **T5b** legge dal DOM già renderizzato titolo, indirizzo e testo visibile e li manda ad
  `analisi_annuncio`, **invariato**, esattamente come per l'incolla-testo. La previsione del
  2026-08-04 ha avuto anche la sua prova numerica: lo stesso indirizzo di Jooble chiesto da un
  programma qualunque torna `403` con la sfida di Cloudflare, chiesto dalla WebView apre i
  risultati. Restano fuori due rifiniture, annotate nel backlog attivo (le cornici `iframe` e
  la normalizzazione dell'indirizzo). *(2026-08-12, T5a-T5b; diario Step 2.11 e 2.12;
  cap. 06.1/06.4.)*
- ✅ **Estrazione da LinkedIn / sito web (2.1.3)** — l'ultima delle tre fonti di estrazione del
  disegno top-down, e quella che era stata data per «complessità alta»: il costo vero è stato
  **un bottone in P3 e uno in P2**. L'idea originale era un *fetch* di un link pubblico, e non
  avrebbe funzionato — un profilo si costruisce in JavaScript e sta dietro un accesso — mentre
  la strada aperta da T5b sì: si legge la pagina che l'utente **sta guardando**, dopo che si è
  autenticato come sé, e la si manda a `importa_cv`, che non ha dovuto imparare niente perché
  non ha mai saputo da dove venisse il testo. Una sola cosa il disegno non poteva prevederla, e
  l'ha insegnata la pagina vera: **va scorsa prima di leggerla**, o ne esce l'intestazione e
  basta (2196 caratteri contro 9681). *(2026-08-14, T5d; diario Step 2.14; cap. 06.7;
  architettura.md §8.)*
- ✅ **Un attrezzo per rispondere alle finestre di messaggio** — è `rispondi_finestra`, nato a
  T5c perché lo scarto va confermato e una `MessageBox` aperta blocca tutto il resto. Preme il
  bottone per **nome** («Sì», «No», «OK») e non per numero, come si era ipotizzato: quali
  bottoni mostri una finestra di messaggio non si sa in anticipo, mentre il testo c'è sempre
  (ripulito della `&` dell'acceleratore). Senza argomenti **legge cosa chiede** e quali scelte
  dà, il che è la parte che conta davvero: si sa cosa si sta per confermare invece di premere
  al buio. Ha anche insegnato a distinguere le due finestre che Windows chiama allo stesso
  modo — la scelta file ha la casella del nome, una finestra di messaggio no. La prima cosa
  provata con lui è stata uno «Scarta» a cui si è risposto **No**: è così che si collauda un
  comando distruttivo sui dati veri di qualcuno senza distruggere niente.
  *(2026-08-13, T5c; `strumenti/mcp-collaudi/README.md`; diario Step 2.13.)*
- ✅ **Validazione di range della taratura** — ogni numero letto da `taratura.json` dichiara
  ora l'intervallo in cui ha senso, e un valore fuori scala viene **scartato e annotato**
  invece di entrare zitto: un `"clamp_su": -50` non falsa più le stelle. Fatta a T4 perché è
  la prima tappa che usa `CalcoloMatch` davvero dentro l'applicazione. *(2026-08-10, T4;
  cap. 11.6, con tre collaudi.)*
- ✅ **Città pass/fail anche in `CollaudiFormatiReale`** — chiusa, ma **non come previsto**:
  non era un allineamento meccanico. Il collaudo di tappa di T4 ha mostrato che la stessa
  riga «Carasco (GE)», identica in tutti e quattro i file, torna a volte con la provincia e a
  volte senza — due letture **entrambe fedeli**, perché il prompt dice quale indirizzo
  prendere, non come scriverlo. Fra le quattro strade si chiede quindi che sia la **stessa
  città** (ammessa la sola sigla di provincia); il pass/fail vero resta quello contro il CV.
  È nato lì il primo banco degli **attrezzi di misura**, che ha subito bocciato la prima
  versione della regola. *(2026-08-11; `CollaudoReale.StessaCitta`, `CollaudiMetroReale`.)*
- ✅ **La trappola latente di `Sigilla`** — chiusa col suo collaudo: nel manifest entrano solo
  i file che hanno l'intestazione di metadati, quindi un `---` dentro il CHANGELOG non viene
  più scambiato per una chiusura di frontmatter. *(2026-08-10, T4; cap. 04.5.)*
- ✅ **Hard-gate (requisito eliminatorio)** — un requisito *davvero* squalificante non soddisfatto
  non pesa soltanto: mette un **tetto** al match. Ogni giudizio del confronto porta un flag
  `eliminatorio` (booleano, deciso dall'LLM: `true` solo per i requisiti tassativi/escludenti, nel
  dubbio `false`); se almeno uno è `eliminatorio` con esito `non soddisfatto`, il codice cratera il
  match a **≤ 20/100 (≤ 1 stella)**, `finale = min(finale, 20)`, con nota esplicita. Deterministico,
  applicato dopo il clamp, in sinergia con la soglia B (≤1 stella → generazione sconsigliata). La
  **patente** è il primo caso reale (patente C richiesta, candidato senza → gate). Prompt+schema in
  `prompt_design.md`, `calcolaMatch` in `server.js`, nota+⛔ in `index.html`, colonna "Elim." nel
  banco `test-confronto.html`. *(2026-08-04; diario Step 1.37.)*
