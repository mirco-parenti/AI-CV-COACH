# 14 — Piano di lavoro

*Le tappe dell'implementazione, ciascuna con il suo collaudo. Regola del piano: alla
fine di **ogni** tappa l'applicazione si avvia, si mostra e si prova — il montaggio è
incrementale, mai «tutto rotto per settimane». Ogni tappa chiusa produce anche il suo
Step nel diario di bordo.*

## Le tappe

### T0 — Ratifica del progetto *(cancello d'ingresso)* — ✔ **CHIUSO il 2026-08-05**
Il capitolo 15 è stato discusso voce per voce e **svuotato**: tutte le decisioni hanno
un esito definitivo, le voci restanti sono dichiarate rimandate con la loro motivazione
(cap. 15.6). Otto decisioni si discostano dalla proposta originale e sono già state
riportate nei capitoli interessati.
**Fatto:** i documenti 01–15 sono confermati. **T1 può iniziare.**

### T1 — Lo scheletro che parte
Installazione dell'**SDK .NET 10** su entrambe le postazioni (oggi assente: è il primo
passo materiale). Soluzione Visual Studio in `VB.NET/src/` (`TrovaLavoro.sln`);
`FormPrincipale` con barra superiore, area centrale vuota e **pannello logo** (segnaposto
tipografico «TL», versione + pool); modulo `StileApp` (token del cap. 03); `Versione.vb`;
**proprietà dell'eseguibile** (prodotto TrovaLavoro, società Aviolab AI, © 2026 Aviolab
AI); **prova immediata della pubblicazione single-file** autonoma e non compressa (il
vincolo più rigido si verifica subito, non alla fine), con **misura di dimensione e
tempo di avvio**.
**Collaudo:** l'exe pubblicato parte su un PC pulito e mostra la finestra con il logo
e «Ver. 0.1.001 · Pool —»; la scheda «Dettagli» del file riporta Aviolab AI.

### T2 — Il motore e il pool
`Ai/LibreriaPrompt` (pool esterno + integrato, manifest, segnaposto);
`Ai/ClientClaude` (chiamate sincrone + streaming, retry, timeout) verso **Haiku 4.5**
(estrazione) e **Sonnet 5** (ragionamento) — attenzione ai due comportamenti nuovi di
Sonnet 5: ragionamento esteso attivo di default e conteggio token più alto di circa il
30%, entrambi da considerare nel dimensionare il limite di risposta;
`Motore/EstrattoreJson`; `Motore/CalcoloMatch` che legge i valori da `taratura.json`
(cap. 11.6); migrazione dei 15 prompt del prototipo nel pool (`Pool 1.00`).
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
Verifica sul campo degli schemi di indirizzo dei portali del primo rilascio.
**Collaudo:** su **Indeed / InfoJobs / Subito.it** reali: login manuale dove serve,
cattura di annunci veri, rifiuto garbato delle pagine-elenco; riapertura dell'app con
stato intatto.

### T5b — Il profilo da LinkedIn (voce 2.1.3)
Piccola coda di T5: cattura della **propria** pagina profilo LinkedIn dal browser
integrato e invio alla strutturazione `importa_cv` già esistente (cap. 06.7). Nessun
componente nuovo — riusa cattura e prompt di T5 e T3.
**Collaudo:** dalla pagina profilo reale di Mirco esce un profilo JSON coerente con
quello ottenuto dal suo CV in PDF.

### T6 — Le email (F5 completo)
Composizione con allegati suggeriti (inclusa la scansione della cartella documenti,
cap. 05.2); scrittura del file `.eml` con intestazione `X-Unsent`; conferma dell'avvenuto
invio da parte dell'utente e aggiornamento del registro.
*Tappa alleggerita dalle decisioni del 2026-08-05:* niente `.msg`, niente invio SMTP,
quindi niente pannello di configurazione del server né password di posta da custodire
(cap. 15, voci 8 e 9).
**Collaudo:** un'`.eml` generata, aperta in un programma di posta vero e spedita da lì,
con allegati integri; verifica che la chiave API non compaia né su disco in chiaro né
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
T0 ─► T1 ─► T2 ─► T3 ─► T4 ─► T5 ─► T5b ─► T6 ─► T9
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
