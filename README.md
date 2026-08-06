# AI-CV-COACH

**Stato:** 🚧 In sviluppo attivo — **prototipo HTML+JS completo e congelato** in `HTML+JS/`: MVP end-to-end sui 4 anelli (✅ profilo guidato · ✅ analisi annuncio · ✅ confronto + punteggio in stelle 0–5 · ✅ generazione 📄 CV-1 base + 🎯 CV-2 mirato + ✉️ lettera) e Fase B chiusa (mitigazione 2.2.4, turni contatti e patente, import di un CV in PDF, soglia di prudenza sotto 1,5 stelle, hard-gate sui requisiti eliminatori). **Aperta la Fase 3 — migrazione VB.NET**: il repo è ora organizzato in due aree — `HTML+JS/` (il prototipo validato) e `VB.NET/` (tutto lo sviluppo da qui in avanti) — e in `VB.NET/progetto/` è definito il **progetto dettagliato** dell'applicazione Windows 11 (exe singolo, GUI nativa, libreria di prompt .md, ricerca annunci con browser integrato, export DOCX/PDF, email di candidatura, server MCP integrato, multilingua IT/EN). Il **cancello T0 è chiuso** (2026-08-05): tutte le decisioni aperte hanno un esito definitivo — tra cui .NET 10 LTS, il nome utente «TrovaLavoro», i modelli Haiku 4.5 / Sonnet 5, i portali del primo rilascio. **La tappa T1 è iniziata** (2026-08-06): l'ambiente di sviluppo è installato e collaudato sulla postazione di sviluppo (SDK .NET 10.0.302 e Visual Studio 2026 Community), e la pubblicazione a **exe singolo autonomo** — il vincolo più rigido del progetto — è stata verificata sul campo su un progetto di prova.

AI-CV-COACH è una web app sperimentale sviluppata come progetto di stage presso Aviolab AI.

L'obiettivo del progetto è aiutare un utente a costruire un profilo professionale strutturato, analizzare un annuncio di lavoro e generare un CV mirato basato esclusivamente su esperienze, competenze e informazioni realmente fornite dall'utente.

## Stato del progetto

Questo è un progetto di apprendimento sviluppato in modo trasparente nell'ambito del mio tirocinio presso Aviolab AI. Il repository è organizzato in **due aree**: `HTML+JS/` contiene il prototipo web validato (codice, prompt e architettura della fase MVP), `VB.NET/` contiene la fase attuale — il progetto dettagliato e, a seguire, lo sviluppo dell'applicazione Windows. Il repository documenta sia il codice sia il metodo di lavoro, incluso un diario di bordo (`diario_di_bordo.md`) che traccia decisioni e ragionamenti a ogni passo e un documento di architettura (`HTML+JS/architettura.md`) che disegna il sistema dall'alto — funzioni, dati e principi.

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

In entrambe le fasi: Git e GitHub per il versionamento.

---

© 2026 Mirco Parenti
