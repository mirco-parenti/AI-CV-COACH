# Idee future — AI-CV-COACH

Raccolta **unica** dei raffinamenti e delle idee per le fasi successive del progetto.
Non è codice né stato corrente: è il **backlog ragionato**. Lo **stato attuale** della
pipeline sta nel `README.md` (sezione *Stato*) e nell'ultimo `### Step` del
`diario_di_bordo.md`.

*Come si compila questo file (modalità):* una voce per idea, raggruppata per area; ogni
voce dice **cos'è · perché è futura · dove se ne parla** (puntatore a diario/prompt_design).
Le idee nuove si annotano con **data e motivo**. Aggiornato con "aggiorna-tutto".

## Front-end & pipeline
- **Integrazione front-end**: oggi `index.html` fa solo l'anello 1; gli anelli 2 e 3
  vivono solo nelle `test-*.html`. Unirli in un unico flusso utente reale
  (dialogo → incolla annuncio → match in stelle). *(Backlog handoff.)*

## Profilo, annuncio & schema
- **Estensione del profilo** a specchio di `altri_requisiti` (domicilio, disponibilità,
  patente, automunito, età, iscrizione albo, idoneità): rende ricavabili nel match dati
  che oggi escono `non determinabile`. **Non è un prerequisito** (`altri_requisiti` è già
  confrontabile). Attenzione alla **sensibilità** dei dati personali (domicilio, età);
  probabile nuovo turno nell'anello 1. *(Diario Step 1.14.)*
- **Campi profilo aggiuntivi**: contatti, lingue, link — da valutare dopo l'MVP.
  *(prompt_design.md, note schema profilo.)*
- **Campi annuncio aggiuntivi**: `livello` (impiegato/operaio/quadro), `settore` — fuori
  per ora (schema snello). *(prompt_design.md, "Da valutare in futuro".)*
- **Decomposizione dei prompt**: il prompt unico dell'annuncio è già diviso in 5 sezioni
  numerate, pensate per diventare sotto-prompt separati. *(Diario Step 1.10.)*

## Dialogo (anello 1)
- **pending_questions**: accantonare le domande saltate o non strutturabili e riprenderle
  in un secondo giro a fine dialogo. Pianificato, non costruito. *(Diario apertura Fase 1,
  Step 1.2.)*
- **Rifiniture MVP rimandate**: rimozione di singole competenze, routing a linguaggio
  naturale (oggi bottoni), editing campo-per-campo. *(Diario Step 1.8.)*
- **Formato del riepilogo leggibile** del profilo: da decidere insieme all'interfaccia.
  *(prompt_design.md, turno formazione, "Sospeso".)*

## Match & punteggio (anello 3)
- **Hard-gate**: un requisito *davvero* squalificante non azzera il punteggio (tetto del
  clamp −20). Trattarlo come tetto rigido che cratera il match è rimandato.
  *(Diario Step 1.16-1.17; prompt_design.md, "Limite noto".)*
- **Taxonomy mapping (ESCO/O*NET)**: mappare le skill su una tassonomia standard per il
  match. Scartato per l'MVP (il match semantico lo fa l'LLM); utile per analisi su grandi
  volumi, non per il singolo match. Fonti GitHub da riprendere:
  - `nestauk/ojd_daps_skills`
  - `KonstantinosPetrakis/esco-skill-extractor`
  - `amazon-science/job-posting-structure` (componente *SkillsTaxonomyAI*)
  *(Diario Step 1.10-1.11-1.15.)*

## Generazione (anello 4)
- **Testo visibile per le esperienze informali** nel CV: come presentarle senza
  "promuoverle" a formali. *(prompt_design.md, note schema profilo — da affrontare nel
  design dell'anello 4.)*
