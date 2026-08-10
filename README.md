# AI-CV-COACH

**Stato:** 🚧 In sviluppo attivo — **prototipo HTML+JS completo e congelato** in `HTML+JS/`: MVP end-to-end sui 4 anelli (✅ profilo guidato · ✅ analisi annuncio · ✅ confronto + punteggio in stelle 0–5 · ✅ generazione 📄 CV-1 base + 🎯 CV-2 mirato + ✉️ lettera) e Fase B chiusa (mitigazione 2.2.4, turni contatti e patente, import di un CV in PDF, soglia di prudenza sotto 1,5 stelle, hard-gate sui requisiti eliminatori). **Aperta la Fase 3 — migrazione VB.NET**: il repo è ora organizzato in due aree — `HTML+JS/` (il prototipo validato) e `VB.NET/` (tutto lo sviluppo da qui in avanti) — e in `VB.NET/progetto/` è definito il **progetto dettagliato** dell'applicazione Windows 11 (exe singolo, GUI nativa, libreria di prompt .md, ricerca annunci con browser integrato, export DOCX/PDF, email di candidatura, server MCP integrato, multilingua IT/EN). Il **cancello T0 è chiuso** (2026-08-05): tutte le decisioni aperte hanno un esito definitivo — tra cui .NET 10 LTS, il nome utente «TrovaLavoro», i modelli Haiku 4.5 / Sonnet 5, i portali del primo rilascio. **La tappa T1 è chiusa** (2026-08-06): l'ambiente di sviluppo è installato e collaudato sulla postazione di sviluppo (SDK .NET 10.0.302 e Visual Studio 2026 Community), lo scheletro dell'applicazione si avvia e mostra la sua finestra, e la pubblicazione a **exe singolo autonomo** — il vincolo più rigido del progetto — è stata verificata sul campo. **Anche la tappa T2 è chiusa** (2026-08-07): il motore c'è — estrattore JSON, calcolo del match con pesi e soglie in un file di taratura, libreria dei prompt con il **Pool 1.00** (i 15 prompt del prototipo migrati, con manifest e impronte) e client dell'AI con timeout e ritentativo — e ha superato la batteria di **non-regressione contro il prototipo**, che resta il giudice: 65 collaudi automatici senza rete, più due confronti reali fatti girare in parallelo sul prototipo e sulla nuova app, con lo stesso modello da entrambe le parti. Il prompt che parte è identico *carattere per carattere* a quello del prototipo, e sugli stessi annunci le stelle coincidono — ⛔ eliminatorio compreso. **Ed è chiusa anche la tappa T3** (2026-08-09), il profilo: l'applicazione ora ha le sue due porte d'ingresso vere — la scheda del profilo modificabile campo per campo e il **dialogo guidato** che lo costruisce da zero in sette turni — più l'**import di un CV** da PDF, DOCX, TXT o MD e il salvataggio versionato, che conserva ogni versione confermata. Il collaudo di tappa è stato fatto in tre gambe: lo stesso CV entrato dalle quattro porte (stessi campi copiati, tutte le volte), il confronto col prototipo sull'import (trascrizioni con il **100% di righe in comune**) e il dialogo intero condotto con l'AI vera su una persona inventata, costruita apposta perché scattassero i due meccanismi che contano — **niente di ciò che l'utente dice va perso**, e ciò che non trova posto viene **dichiarato**, non fatto sparire. In tutto **190 collaudi automatici** senza rete e 8 con l'API vera. Qui è arrivato anche il primo **distacco voluto dal prototipo** (Pool 1.01): il prompt che legge un CV ora sa che un'attività sta in una sezione sola e che a decidere è la sua natura — un volontariato resta un volontariato anche se il CV lo stampa fra i lavori. **A valle di T3 è passata una revisione adversariale completa** (2026-08-09): una ventina di difetti eliminati uno a uno nel motore e nei pannelli — dai crash all'avvio ai buchi dell'anti-perdita, dal profilo corrotto che il primo «Salva» distruggeva al ramo PDF scoperto — e il **Pool 1.02**, che dà una casa alle lingue (competenze, riportate come dette, mai con livelli inventati), fa della città il domicilio, manda i patentini professionali in formazione e mette la guardia anti-injection nei sette turni del dialogo. La batteria senza rete è salita a **205 collaudi, tutti verdi**, e la validazione sul modello vero ha confermato il sigillo: il muletto atterra in formazione al primo colpo e le tre lingue del CV reale arrivano ricopiate alla lettera — dove il prototipo ne perdeva tre su tre. Prima uguale, poi meglio. **Ed è chiusa anche la tappa T4** (2026-08-11), la pipeline di candidatura, che è il cuore del prodotto: da un annuncio incollato l'applicazione ricava i requisiti, li confronta col profilo voce per voce, calcola le stelle, costruisce i **ponti onesti** sui requisiti scoperti e genera 🎯 CV mirato e ✉️ lettera, scrivendoli in **DOCX e PDF** — il DOCX componendo lo ZIP OOXML con i soli mattoni di .NET, il PDF facendo stampare la pagina al motore Chromium già presente in Windows: nessuna libreria di terze parti per i documenti. Tutto ciò che una candidatura produce atterra in **una cartella per opportunità**, in JSON leggibili, con annotata la versione di profilo da cui è nata. Il 2026-08-10 il **giro completo è stato percorso per la prima volta**, dall'inizio alla fine e con l'AI vera: dal CV reale di Mirco importato in PDF fino ai quattro file esportati. Il collaudo di tappa, come a T3, è stato fatto in tre gambe — il prototipo giudice (che sui due prompt dove è ancora il metro resta identico *carattere per carattere*, e sulla generazione produce gli **stessi quattro ponti** e nessuna invenzione), la pipeline reale end-to-end, e i file riletti **campo per campo**: 114 campi su 114 ritrovati nei sei documenti prodotti, con DOCX e PDF identici. In tutto **355 collaudi automatici** senza rete e 10 con l'API vera. Lungo la strada è nato anche uno **strumento di collaudo** che guida l'applicazione vera — la compila, la avvia, la fotografa e le preme i bottoni — perché certi difetti si vedono solo guardando il programma in faccia: il primo che ha scovato è un pannello che diceva «prima serve il tuo profilo» a un profilo appena salvato. La prossima tappa è **T5**, la ricerca degli annunci.

AI-CV-COACH è una web app sperimentale sviluppata come progetto di stage presso Aviolab AI.

L'obiettivo del progetto è aiutare un utente a costruire un profilo professionale strutturato, analizzare un annuncio di lavoro e generare un CV mirato basato esclusivamente su esperienze, competenze e informazioni realmente fornite dall'utente.

## Stato del progetto

Questo è un progetto di apprendimento sviluppato in modo trasparente nell'ambito del mio tirocinio presso Aviolab AI. Il repository è organizzato in **due aree**: `HTML+JS/` contiene il prototipo web validato (codice, prompt e architettura della fase MVP), `VB.NET/` contiene la fase attuale — il progetto dettagliato e, a seguire, lo sviluppo dell'applicazione Windows. Il repository documenta sia il codice sia il metodo di lavoro, incluso un diario di bordo (`diario_di_bordo.md`) che traccia decisioni e ragionamenti a ogni passo e un documento di architettura (`HTML+JS/architettura.md`) che disegna il sistema dall'alto — funzioni, dati e principi. Il progetto dettagliato della fase attuale vive nei capitoli di `VB.NET/progetto/`, che sono la verità del disegno finché non è implementato.

Ogni fase del progetto viene chiusa con documentazione, riflessioni e cronologia di commit verificabile, prima di passare alla successiva.

## Obiettivi principali

- Raccogliere e organizzare il profilo professionale dell'utente.
- Analizzare un annuncio di lavoro.
- Confrontare profilo e annuncio.
- Calcolare un punteggio di match orientativo.
- Generare un CV mirato senza inventare informazioni.
- Documentare il processo di progettazione, sviluppo e uso dell'AI.

## Fasi previste

1. Fase 0 – Setup e analisi preliminare.
2. Fase 1 – Prototipo base v0.
3. Fase 2 – Profile Manager guidato v1.
4. Fase 3 – Migrazione a un'applicazione VB.NET sotto Windows 11 (in corso — progetto dettagliato in `VB.NET/progetto/`).

## Vincolo etico principale

L'applicazione non deve inventare esperienze, competenze, titoli di studio o risultati non presenti nel profilo reale dell'utente.

## Tecnologie previste

Il progetto attraversa due fasi tecnologiche distinte.

**Fase MVP (prototipo concluso, in `HTML+JS/`):**

- Frontend in HTML, CSS e JavaScript, eseguito nel browser.
- Un aiutante locale in Node.js: utility di servizio che custodisce la chiave API e fa da tramite verso l'API LLM, in modo che la chiave non sia mai esposta nel browser.
- API di un LLM per la strutturazione del profilo e le elaborazioni, con due modelli scelti per livello di compito: **Claude Haiku 4.5** per l'estrazione (profilo e annuncio) e **Claude Sonnet 4.6** per il confronto semantico profilo-annuncio, che richiede ragionamento più profondo. L'LLM legge anche i **PDF** (input documentale dell'API) per l'import di un CV esistente.

**Fase target (progetto definito, implementazione avviata, in `VB.NET/`):**

- Migrazione a un'unica applicazione VB.NET sotto Windows 11, che assolverà sia il frontend sia la chiamata diretta all'API LLM, rendendo superfluo l'aiutante Node.
- Piattaforma: **.NET 10 LTS** (supporto attivo fino a novembre 2028) con interfaccia **Windows Forms**, distribuita come **un solo `.exe`** autonomo che ingloba il runtime, senza installazione né DLL a fianco.
- I documenti in uscita non richiedono **nessuna libreria esterna**: il **DOCX** nasce componendo lo ZIP OOXML con gli strumenti standard di .NET, il **PDF** facendo stampare una pagina HTML al motore Edge/Chromium già presente in Windows 11 (WebView2) — quindi con testo selezionabile e font incorporati, senza dipendere da Word.
- I **prompt** escono dal codice: vivono in una **libreria di file `.md`** — il *pool* — inglobata nell'eseguibile, con manifest e impronte per accorgersi di ogni modifica. Restano due i modelli, per livello di compito: **Claude Haiku 4.5** per l'estrazione e un modello di ragionamento per il confronto e la generazione. Quest'ultimo oggi è **Claude Sonnet 4.6**, lo stesso del prototipo, perché il collaudo di non-regressione si fa a parità di modello: così una differenza nei risultati è una differenza di codice, non del modello sotto. Il passaggio a **Claude Sonnet 5** è l'esperimento successivo, e costa una riga di configurazione — i nomi dei modelli non sono cablati nel codice, quindi cambiarli non richiede una nuova build.

In entrambe le fasi: Git e GitHub per il versionamento.

---

© 2026 Mirco Parenti
