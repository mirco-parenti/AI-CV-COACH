# 14 — Piano di lavoro

*Le tappe dell'implementazione, ciascuna con il suo collaudo. Regola del piano: alla
fine di **ogni** tappa l'applicazione si avvia, si mostra e si prova — il montaggio è
incrementale, mai «tutto rotto per settimane». Ogni tappa chiusa produce anche il suo
Step nel diario di bordo.*

## Le tappe

### T0 — Ratifica del progetto *(cancello d'ingresso)*
Il capitolo 15 viene discusso e svuotato (o le voci restanti vengono dichiarate
esplicitamente «rimandate»). Nessuna riga di codice prima di questo punto.
**Fatto quando:** i documenti 01–15 sono confermati dall'utente.

### T1 — Lo scheletro che parte
Soluzione Visual Studio in `VB.NET/src/`; `FormPrincipale` con barra superiore, area
centrale vuota e **pannello logo** (versione + pool); modulo `StileApp` (token del
cap. 03); `Versione.vb`; **prova immediata della pubblicazione single-file** (il
vincolo più rigido si verifica subito, non alla fine).
**Collaudo:** l'exe pubblicato parte su un PC pulito e mostra la finestra con il logo
e «Ver. 0.1.001 · Pool —».

### T2 — Il motore e il pool
`Ai/LibreriaPrompt` (pool esterno + integrato, manifest, segnaposto);
`Ai/ClientClaude` (chiamate sincrone + streaming, retry, timeout);
`Motore/EstrattoreJson`; `Motore/CalcoloMatch`; migrazione dei 15 prompt del
prototipo nel pool (`Pool 1.00`).
**Collaudo:** batteria di **non-regressione contro il prototipo** — stessi input degli
step 1.35–1.37 (i 6 casi / 16 verifiche dell'hard-gate, i 6 casi di `estraiJson`, un
confronto reale) → stessi numeri, stesse stelle, stesse note.

### T3 — Il profilo (F1)
Pannello P2 (scheda campo-per-campo) e P5 (dialogo); import da file PDF/TXT/MD/DOCX;
dialogo guidato completo (turni, conferme, anti-perdita, «lasciato fuori»);
salvataggio versionato del profilo.
**Collaudo:** import del CV reale di Mirco nei formati disponibili + dialogo completo
da zero; il profilo JSON risultante regge il confronto con quello del prototipo.

### T4 — La pipeline di candidatura (F3 + F4 + F5 in italiano)
Analisi annuncio da testo incollato; confronto con stelle, note e ⛔; pannello P4;
generazione CV-1/CV-2/lettera in italiano; mitigazione; export **DOCX e PDF**
(scrittore OOXML + stampa via WebView2); pannello P6.
**Collaudo:** end-to-end su un annuncio vero: dal testo incollato ai file DOCX/PDF
aperti in Word/LibreOffice, con verifica campo-per-campo del contenuto.

### T5 — La ricerca annunci (F2) e il registro (F6)
Pannello P3 con WebView2, ricerche salvate, cattura dell'annuncio, coda delle
opportunità; cartelle-opportunità su disco; pannello P1 Home con registro e stati.
**Collaudo:** su Indeed/LinkedIn/InfoJobs reali: login manuale, cattura di annunci
veri, rifiuto garbato delle pagine-elenco; riapertura dell'app con stato intatto.

### T6 — Le email (F5 completo)
Composizione con allegati suggeriti (inclusa la scansione della cartella documenti,
cap. 05.2); salvataggio `.eml`; `.msg` se c'è Outlook; invio SMTP con conferma;
aggiornamento del registro.
**Collaudo:** un'`.eml` aperta e spedita da un client vero; un invio SMTP reale verso
una casella di prova; verifica che i segreti non compaiano né su disco in chiaro né
nei log.

### T7 — Multilingua e qualità (F4 completo)
Varianti `en` dei prompt di generazione; campo `lingua` nell'analisi; prompt di
rifinitura anti-slop con prima/dopo in P6; brainstorming (P5) con streaming e appunti
di mira.
**Collaudo:** candidatura completa su un annuncio in inglese; lettura critica dei
testi prodotti (nessun tic da AI, nessun fatto inventato, nessun errore introdotto).

### T8 — Il server MCP (F8)
Modalità `--mcp` su stdio; i tool del cap. 09; lucchetto di scrittura.
**Collaudo:** da Claude Desktop/Code: `tools/list` corretto, un confronto e una
generazione via MCP con risultati identici a quelli dell'interfaccia.

### T9 — Rifinitura e rilascio 1.0
Backup/ripristino (F7); Impostazioni complete; pulizia dati; gestione errori rivista
pannello per pannello; **collaudo generale condotto da Mirco su candidature reali**;
aggiornamento del diario e del README; demo (video) per il portfolio; tag `v1.0`.
**Collaudo finale:** la checklist «Problemi e mitigazioni» ereditata dal prototipo
(`HTML+JS/prompt_design.md`) ripercorsa punto per punto sulla nuova app.

## Ordine e dipendenze

```
T0 ─► T1 ─► T2 ─► T3 ─► T4 ─► T5 ─► T6 ─► T9
                        ├────► T7 (dopo T4)
                        └────► T8 (dopo T4, in parallelo a T6-T7)
```

T7 e T8 dipendono solo dalla pipeline (T4): se serve comprimere i tempi, si possono
scalare senza toccare il percorso principale T5–T6.

## Regole di conduzione

- **Una tappa = un ramo di lavoro breve**, chiusa con collaudo, diario e bump di
  versione; niente tappe lasciate a metà mentre se ne apre un'altra.
- **Il pool si tocca col rito** (cap. 04.5): ogni modifica ai prompt in corso d'opera
  è un bump di pool annotato, anche in sviluppo.
- **Il prototipo resta il giudice**: finché la nuova app non supera i collaudi di
  non-regressione (T2), nessuna «miglioria» ai prompt o ai pesi — prima uguale, poi
  meglio.
- Le voci del backlog storico (`idee_future.md`) **entrate nel perimetro** sono
  segnate nel cap. 15.5; le altre restano lì e non si infilano di soppiatto nelle
  tappe.
