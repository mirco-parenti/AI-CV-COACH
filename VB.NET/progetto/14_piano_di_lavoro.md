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

### T1 — Lo scheletro che parte — ✔ **CHIUSO il 2026-08-06**
Installazione dell'**SDK .NET 10** su entrambe le postazioni: **fatta su aviolab03 il
2026-08-06** (SDK 10.0.302 + runtime 10.0.10, con Visual Studio 2026 Community 18.5 e il
workload desktop già presenti); resta la postazione del tutor. Soluzione Visual Studio
in `VB.NET/src/` (`TrovaLavoro.sln`);
`FormPrincipale` con barra superiore, area centrale vuota e **pannello logo** (segnaposto
tipografico «TL», versione + pool); modulo `StileApp` (token del cap. 03); `Versione.vb`;
**proprietà dell'eseguibile** (prodotto TrovaLavoro, società Aviolab AI, © 2026 Aviolab
AI); **prova immediata della pubblicazione single-file** autonoma e non compressa (il
vincolo più rigido si verifica subito, non alla fine), con **misura di dimensione e
tempo di avvio**. La catena è già stata provata **a vuoto** il 2026-08-06 su un WinForms
VB appena creato — un solo file da 116 MB, avviato e chiuso — quindi a T1 resta da
ripeterla sull'app vera.
**Collaudo:** l'exe pubblicato parte su un PC pulito e mostra la finestra con il logo
e «Ver. 0.1.001 · Pool —»; la scheda «Dettagli» del file riporta Aviolab AI.
**Fatto:** build pulita, **publish single-file autonomo non compresso da 116 MB** (sotto
la stima di 150–180 MB), **avvio in ~0,26 s a freddo**, proprietà dell'eseguibile
verificate. Il logo è lo scudo Aviolab incorporato in forma binaria nel sorgente, non
il segnaposto tipografico previsto (cap. 15, voce 4). **Restano in coda a Mirco**: la
prova dell'exe su un PC davvero pulito, l'icona dell'eseguibile e l'SDK sulla postazione
del tutor.

### T2 — Il motore e il pool — ✔ **CHIUSO il 2026-08-07**
`Ai/LibreriaPrompt` (pool esterno + integrato, manifest, segnaposto);
`Ai/ClientClaude` (**chiamate sincrone**, retry, timeout) verso **Haiku 4.5**
(estrazione) e **Sonnet 4.6** (ragionamento), cioè gli **stessi modelli del prototipo**:
il confronto si fa a parità di modello, così una differenza nei risultati è una
differenza di codice e non del modello sotto. Il salto a **Sonnet 5** (cap. 15, voce 6)
è il **secondo esperimento**: si fa da `modelli.json` senza ricompilare e porta con sé
l'interruttore del ragionamento esteso, che lì va acceso (cap. 02.5). Lo **streaming non
è di questa tappa**: arriva con T4/T7, quando ci sarà un pannello che lo mostra.
`Motore/EstrattoreJson`; `Motore/CalcoloMatch` che legge i valori da `taratura.json`
(cap. 11.6); migrazione dei 15 prompt del prototipo nel pool (`Pool 1.00`).
**Collaudo:** batteria di **non-regressione contro il prototipo** — stessi input degli
step 1.35–1.37 (i 6 casi / 16 verifiche dell'hard-gate, i 6 casi di `estraiJson`, un
confronto reale) → stessi numeri, stesse stelle, stesse note. Il confronto reale vuole
la chiave API **e** il prototipo che gira come giudice: si esegue perciò **solo su
aviolab03**, dove la chiave c'è.
**Fatto:** **65 collaudi verdi** senza rete più i **2 reali** su aviolab03. La batteria
è cresciuta di una gamba che il piano non aveva previsto — la **parità della richiesta**:
sugli stessi artefatti il prompt costruito dal pool è identico *carattere per carattere*
a quello che il prototipo costruisce nel codice (10 596 e 10 568 caratteri). È ciò che dà
valore all'altra gamba: se la richiesta è la stessa **a parità di modello**, una
differenza negli esiti è del modello e non del codice. Il confronto reale, su
`claude-sonnet-4-6` da entrambe le parti, ha dato 4,6 stelle in entrambi sull'annuncio
compatibile e 0,9 con il ⛔ in entrambi su quello con la patente C; e i giudizi del
prototipo, ricalcolati da `CalcoloMatch`, restituiscono i suoi numeri identici, nota
doppia del gate compresa. Casi, attesi ed esiti stanno in
`VB.NET/src/TrovaLavoro.Collaudi/casi/`. **T3 può iniziare.**

### T3 — Il profilo (F1) — ✔ **CHIUSO il 2026-08-09**
Pannello P2 (scheda campo-per-campo) e P5 (dialogo); import da file PDF/TXT/MD/DOCX;
dialogo guidato completo (turni, conferme, anti-perdita, «lasciato fuori»);
salvataggio versionato del profilo.
**Collaudo:** import del CV reale di Mirco nei formati disponibili + dialogo completo
da zero; il profilo JSON risultante regge il confronto con quello del prototipo.

La tappa è stata **spezzata in tre** il 2026-08-07, motore prima dell'interfaccia:
**T3a** il profilo nel motore (`Dati/CartellaDati`, `Dati/Profilo`,
`Dati/ArchivioProfilo`, `Ai/StrutturatoreTurni`, `Motore/Mossa` e
`Motore/DialogoProfilo`, la macchina a mosse); **T3b** l'import
(`Dati/LettoreDocumenti`, `Ai/TrascrittorePdf`, `Motore/ImportProfilo`); **T3c** i
pannelli (`Motore/ContestoApp`, `Ui/PannelloProfilo`, `Ui/PannelloDialogo`,
`Ui/IPannelloArea`). La ragione è quella del cap. 02: il dialogo passa da `Mossa` invece
di disegnare da sé la pagina, e questo permette di collaudarlo tutto **prima** che esista
un pannello che lo mostri.

**Fatto:** **190 collaudi verdi** senza rete e **8 reali** su aviolab03. Il **collaudo di
tappa** è stato condotto in tre gambe, disegnate con Mirco perché le tre domande sono
diverse:

- **A — le quattro porte** (`CollaudiFormatiReale`): lo stesso CV entra da PDF, DOCX, TXT
  e MD e ne deve uscire lo stesso profilo. Tre giri: campi copiati identici fra le quattro
  strade tutte le volte, anti-invenzione pulita ovunque, testo in comune col PDF 100%
  (TXT e MD) e 83,9% (DOCX). *Limite dichiarato*: i tre compagni del PDF sono fabbricati
  dalla sua trascrizione, quindi provano le **strade di lettura**, non l'impaginazione di
  Word (`in_sospeso.md`).
- **B — il prototipo come giudice** (`CollaudiImportReale`): trascrizioni di 3228 caratteri
  e 60 righe da entrambe le parti, **100% di righe in comune** in tutti i giri; campi
  copiati sempre uguali. Qui è nato il **primo distacco voluto** dal prototipo: il
  **Pool 1.01** ha detto a `importa_cv` che un'attività sta in una sezione sola e che a
  decidere è la sua natura, non la sezione del CV in cui è stampata. Su quel prompt il
  prototipo non è più il metro, è il termine di paragone di ciò che l'app fa meglio.
- **C — il dialogo da zero** (`CollaudiDialogoReale`): i sette turni condotti con l'AI vera
  su una traccia inventata, costruita perché **anti-perdita** e **«lasciato fuori»**
  scattino di proposito. Tre giri: ordine dei turni sempre rispettato, **zero** frammenti
  instradati altrove e mai più ricomparsi, zero invenzioni, profilo pieno, salvato e
  riletto identico. E poi la prova che il banco non può fare: l'applicazione **avviata
  davvero**, il dialogo condotto dentro P5 e il profilo salvato dalla sua scheda.

**T4 può iniziare.**

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
  meglio. *Cancello passato il 2026-08-07*: da qui una miglioria è ammessa, ma resta
  una scelta da motivare e da far passare dal rito del bump (cap. 04.5) — e chi la fa
  sappia che allontana i prompt dagli attesi del banco, che vanno rigenerati.
- Le voci del backlog storico (`idee_future.md`) **entrate nel perimetro** sono
  segnate nel cap. 15.5; le altre restano lì e non si infilano di soppiatto nelle
  tappe.
