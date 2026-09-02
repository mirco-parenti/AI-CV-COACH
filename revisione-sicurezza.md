# Revisione di sicurezza — report finale (Fase 1)

*Report dedicato della Fase 1 della revisione di finalizzazione (ramo
`feature/finalizzazione`, chiuso il 2026-09-01). Nasce per **lanciare la fase di fix**:
raccoglie il quadro completo — cosa è solido, i rilievi con la loro verifica, i fix già
applicati nel working tree e quelli da fare — così che ogni voce si possa approvare e
chiudere una per una. La traccia di lavoro delle tre fasi resta
`revisione-finalizzazione.md`; questo file la integra, non la sostituisce, e come lei
può sparire a PR integrata.*

## Perimetro e threat model (come deciso in apertura di fase)

- **Perimetro**: prodotto `VB.NET/src/`, strumenti di sviluppo (`strumenti/`),
  prototipo `HTML+JS/` (congelato: i suoi fix sono voce a parte della PR, li accetta
  Mirco).
- **Threat model**: macchina Windows mono-utente fidata; un altro utente della stessa
  macchina è fuori scope (DPAPI `CurrentUser` copre la chiave). Avversari considerati:
  **contenuto web ostile** (annunci), **file CV ostili**, **output del modello**,
  **rete** (MITM), **client MCP**.

## Cosa è risultato solido (verificato alla fonte, nessun intervento)

- **Chiave API**: DPAPI scope utente, mascheratura a video, redazione regex nel
  `DiarioTecnico`, nessuna chiave nel repo.
- **Rete**: solo HTTPS in uscita, nessun bypass TLS; il server MCP del prodotto è
  **stdio** e non apre porte.
- **Input**: PDF non parsato localmente (va in base64 all'API); DOCX letto per entry
  nominata (niente zip-slip), `XDocument` con DTD proibite.
- **Sink dei documenti**: HTML sempre da `HtmlEncode` (`ScrittoreHtml.vb`, ogni testo
  dinamico); DOCX scritto con `XmlWriter` (escape automatico,
  `ScrittoreDocx.vb:275`); EML con a capo tolti da oggetto e indirizzi e corpo in
  base64 (`ScrittoreEml.vb`); il PDF nasce dall'HTML già codificato
  (`StampantePdf.vb:202`, `NavigateToString` su pagina senza script).
- **Percorsi da contenuti AI**: sempre sanitizzati (`Sillabario` in
  `ArchivioOpportunita`, `NomiDocumenti.Pezzo`); guardia anti-traversal **unica e
  condivisa** fra tool MCP di lettura e scrittura
  (`Mcp/ToolDiLettura.vb:169-190`: solo nome di cartella, niente percorsi, niente
  `.`/`..`).
- **Processi**: nessuna shell, nessun argomento da input non fidato nel prodotto.
- **Email**: il destinatario **lo digita l'utente** — il programma non lo propone mai
  (`Motore/BozzaEmail.vb:70`) — quindi un annuncio ostile non può dirottare la
  candidatura verso un indirizzo suo.
- **Prompt del pool**: ogni testo non fidato (annuncio, CV, conversazione, pezzi da
  riscrivere) entra nei prompt **delimitato da tag** (`<annuncio>`, `<cv>`,
  `<profilo>`, …) con l'istruzione esplicita di trattarlo come dato e mai come
  istruzioni (riga 12-13 di ciascun prompt del pool).

## R2 — Il raggio d'azione della prompt injection (verifica completata il 2026-09-01)

*Era l'unico rilievo con verifica ancora aperta: la mappa completa dei sink che
l'output del modello può raggiungere quando i prompt hanno dentro testo ostile. La
verifica è stata rifatta da capo in questa sessione, file alla mano.*

**Ingressi del testo non fidato**: la cattura dell'annuncio
(`Web/LettorePagina.vb` — testo, titolo e indirizzo della pagina), l'import del CV da
PDF/DOCX/pagina web, e — di riflesso — tutto ciò che da lì entra nell'archivio e nei
prompt successivi.

**Sink raggiungibili dall'output del modello, uno per uno**:

| Sink | Percorso | Esito |
|---|---|---|
| Nomi di file e cartelle | azienda/titolo → `Sillabario` (slug) e `NomiDocumenti.Pezzo` | ✅ Sanitizzati; nel working tree anche il tetto di 40 caratteri sullo slug (v. F1-già-applicati) |
| Pagina HTML / PDF | CV e lettera → `ScrittoreHtml` → `StampantePdf` (WebView2) | ✅ Ogni testo passa da `HtmlEncode`; il modello di pagina non ha script: non c'è iniezione di markup né di JavaScript |
| DOCX | `ScrittoreDocx` | ✅ `XmlWriter` fa l'escape da sé |
| EML | oggetto, corpo, nomi allegato → `ScrittoreEml` | ✅ CR/LF tolti da oggetto, indirizzi e (fix nel working tree) nomi allegato; corpo in base64: l'header injection non passa |
| Destinatario email | — | ✅ Non esiste come sink AI: lo digita l'utente (`BozzaEmail.vb:70`) |
| Interfaccia | caselle e etichette WinForms | ✅ Testo puro, nessun markup interpretato |
| Archivio | JSON nella cartella dati | ✅ Contenuto inerte; i percorsi sono decisi dal programma |
| Client MCP | `leggi_registro`, `leggi_opportunita`, … consegnano l'archivio **tale e quale** | ⚠ Il testo ostile arriva nel contesto del **client LLM** (iniezione di secondo ordine) — v. sotto |

**Verdetto**: dal testo ostile **non esiste un percorso** verso esecuzione di codice,
scrittura fuori dalla cartella dati, intestazioni email nascoste o esfiltrazione senza
un'azione dell'utente. Restano due rischi **residui, di natura non codificabile**:

1. **Inganno sul contenuto**: un annuncio ostile può orientare il testo di CV e
   lettera. Mitigato dal vincolo anti-invenzione dei prompt e dal fatto che ogni
   documento passa dalla revisione dell'utente prima di partire. Inerente alla classe
   di prodotto.
2. **Iniezione di secondo ordine via MCP**: un client LLM che legge i tool di lettura
   riceve nel proprio contesto testo scritto da chi ha pubblicato l'annuncio. La
   difesa spetta al client (è il suo threat model); dal lato nostro va **documentato
   il confine di fiducia** — stessa cura già decisa per R3.

**Gravità assegnata**: Bassa (documentare). Nessun fix di codice necessario: la
delimitazione nei prompt c'è già, i sink sono chiusi.

## Stato dei rilievi

| # | Dove | Rilievo | Gravità | Stato |
|---|------|---------|---------|-------|
| R1 | `Mcp/ServerMcp.vb` | Eccezioni complete su stderr senza redazione: il client MCP le raccoglie nei suoi log | Media | ✅ **Fix nel working tree**: `Annota` (`ServerMcp.vb:526`) passa ora da `DiarioTecnico.SenzaSegreti`, e **tutte** le scritture su stderr (`:219`, `:378` comprese) passano da `Annota`. **Residuo minore**: `ex.Message` nelle risposte d'errore JSON-RPC (`:221`, `:379`) esce non redatto — v. fase di fix |
| R2 | testo web → prompt → sink | Prompt injection dal web: raggio d'azione | Bassa (documentare) | ✅ **Verifica completata** (v. sopra): sink chiusi, restano i due residui da documentare |
| R3 | `Mcp/CatalogoTool.vb`, `ToolDiScrittura.vb` | Tool MCP di scrittura senza conferma umana; `esporta_backup` scrive comunque solo nella cartella dati | Bassa (documentare) | ✅ Verificato; da documentare il confine di fiducia (si fonde con R2-residuo 2) |
| R4 | `strumenti/mcp-collaudi/server.mjs` | `spawn("bash", ["-lc", …])` con argomenti interpolati → command injection; HTTP su 127.0.0.1:3300 senza autenticazione | Bassa | ✅ Confermato; **fix da fare** (strumento di sviluppo, fuori dal prodotto) |
| R5 | `HTML+JS/server.js:1248` | `listen` senza host → bind su tutte le interfacce, chiunque in rete consuma la chiave API | Media (quando il prototipo gira) | ✅ **Fix nel working tree**: `listen(PORT, "127.0.0.1")`. Prototipo congelato: voce a parte della PR, la accetta Mirco |
| — | `Documenti/ScrittoreEml.vb:273` | *(emerso in verifica)* I nomi allegato tutti-ASCII saltavano la ripulitura CR/LF: header injection via nome file | Media | ✅ **Fix nel working tree**: `PerIntestazione` passa da `SenzaACapo` |
| — | `Dati/ArchivioOpportunita.vb:455` | *(emerso in verifica)* Slug di azienda+titolo senza tetto: un titolo abnorme sfonda la lunghezza massima del percorso | Bassa | ✅ **Fix nel working tree**: tetto a 40 caratteri, come promesso dal riepilogo del metodo |

**Note minori confermate** (nessun intervento): manca `packages.lock.json` (una sola
dipendenza NuGet); i dati personali in chiaro in `%APPDATA%` sono scelta dichiarata
del prodotto («l'utente padrone dei suoi dati») — al più una riga nella GUIDA.

## Piano della fase di fix

*I quattro fix «nel working tree» sono già scritti ma **non ancora approvati voce per
voce né committati**: il processo concordato (nessun fix senza approvazione) chiede
che la fase di fix parta da lì. Ordine proposto:*

**S1 — Ratifica dei fix già nel working tree** (R1, R5, EML, slug).
- Approvazione voce per voce; poi **collaudi falsificabili** per i tre fix del
  prodotto (regola 14): una chiave finta dentro un'eccezione non deve arrivare su
  stderr del server MCP (rosso togliendo `SenzaSegreti` da `Annota`); un nome
  allegato ASCII con CR/LF dentro non deve aprire un'intestazione (rosso togliendo
  `SenzaACapo` da `PerIntestazione`); uno slug da titolo abnorme resta ≤ 40 (rosso
  togliendo il tetto). Oggi **nessuno dei tre fix ha un collaudo**.
- Commit del prodotto; il fix di `server.js` in un **commit separato** (prototipo
  congelato: voce a parte della PR, la accetta Mirco).

**S2 — Residuo R1**: le risposte d'errore JSON-RPC (`ServerMcp.vb:221` e `:379`)
avvolgono `ex.Message` in `DiarioTecnico.SenzaSegreti`. Una riga per sito, stesso
collaudo di S1 esteso alla risposta.

**S3 — R4, strumento di collaudo** (`strumenti/mcp-collaudi/server.mjs`): gli
argomenti dei tool non passano più da una stringa `bash -lc` interpolata ma da
`spawn` senza shell con argomenti separati (`esegui()` a `:80`; interpolazioni a
`:278`, `:557`, `:725`). Rischio reale basso (server locale di sviluppo), fix
semplice; la trappola pagata si annota nel suo `README.md` come da regola di
progetto.

**S4 — Documentazione del confine di fiducia MCP** (chiude R2-residuo e R3): nel
cap. 09 (e nel `README.md` se serve una riga): i tool di lettura consegnano testo
proveniente da annunci web non fidati — la difesa dall'iniezione di secondo ordine
spetta al client; i tool `genera_*` spendono token senza conferma umana ed
`esporta_backup` scrive solo dentro la cartella dati. Più la riga in GUIDA sui dati
in chiaro, se la si vuole.

**Chiusura di fase**: banco completo verde (`dotnet test TrovaLavoro.sln`), poi la
Fase 1 si dichiara chiusa in `revisione-finalizzazione.md`.

## Esito della fase di fix (2026-09-01, eseguita con tre agenti sotto coordinamento)

Tutti e quattro i blocchi eseguiti, banco **1343 verdi / 0 rossi**, quattro commit sul
ramo `feature/finalizzazione`:

- **S1+S2 → `4d3b4a9`** *(Secrets stay home, headers stay closed, names stay short)*:
  fix del prodotto (redazione di `Annota` **e** delle due risposte d'errore JSON-RPC;
  `SenzaACapo` sui nomi allegato; tetto 40 allo slug) coi quattro collaudi nuovi,
  **tutti falsificati e visti rossi** — le righe sono nel registro `falsificazioni.md`.
  Rompendo il tetto dello slug cade prima l'`IOException` del sistema operativo
  dell'asserzione: esattamente il danno che il tetto previene.
  **Dichiarazione (regola 14)**: il fix S2 è applicato in **due** punti, ma il
  collaudo ne falsifica uno solo — il sito di `RispondiAsync` (`ServerMcp.vb:386`).
  Il gemello in `ServiLaRichiestaAsync` (`:226`) non è raggiungibile da un collaudo:
  `RispondiAsync` cattura già ogni eccezione al suo interno, e quel `Catch` esterno
  scatta solo per un guasto di `Scrivi`/`Consegna`, non provocabile da fuori. Il
  codice lì è identico al sito sorvegliato (stessa riga, stessa `SenzaSegreti`).
- **R5 → `dcc879e`** *(The prototype listens only to its own machine)*: commit a parte
  per il prototipo congelato, voce sua nella PR, la accetta Mirco.
- **S3 → `227fde8`** *(The test server stops speaking shell)*: i siti col passaggio da
  shell erano **cinque**, non tre come stimato; tolti tutti, trappola annotata nel
  README dello strumento. La falsificazione ha dimostrato l'iniezione reale (la forma
  vecchia piazzava un file in silenzio). **Riserva** (regola 15): il giro dal vivo con
  un client vero non si è potuto fare — server occupato da un'altra sessione — ed è
  annotato in `in_sospeso.md`.
- **S4 → `a1ff30f`** *(The trust boundary is written where the client will read it)*:
  sottosezione «Il confine di fiducia col client» in coda al §9.5 del cap. 09, frase
  sul rovescio dei dati in chiaro nella GUIDA.

Con questo la Fase 1 è **chiusa con una sola riserva** (il giro dal vivo di S3).

**Riserva chiusa il giorno stesso** *(2026-09-01)*: il thread dei fix UI ha riavviato il
server di collaudo col codice di `227fde8` e il giro a vista di F2-1+F2-2 è passato tutto
dal codice senza shell — `compila`, `avvia`, `stato_app`, otto `clic`, `rispondi_finestra`
(lettura e risposta), sei `schermata`, `chiudi_app` — con ogni attrezzo che risponde come
prima, accenti compresi. Dettaglio in `in_sospeso.md`, sezione «Chiuse». La Fase 1 è
**chiusa senza riserve**.

## Verifica di questa sessione

- Rilettura alla fonte di: `Web/LettorePagina.vb`, `Documenti/ScrittoreEml.vb`,
  `Documenti/ScrittoreHtml.vb`, `Documenti/StampantePdf.vb` (rendering),
  `Documenti/ScrittoreDocx.vb` (escape), `Mcp/ServerMcp.vb` (tutti i punti stderr),
  `Mcp/ToolDiLettura.vb` (guardia percorsi), `Motore/BozzaEmail.vb` (destinatario),
  prompt del pool (delimitazione), diff completo del working tree.
- Banco (`dotnet test TrovaLavoro.sln`) eseguito sul working tree con i quattro fix
  dentro: **verde — 1335 passati, 0 falliti** (2026-09-01). I fix non rompono nulla;
  i collaudi che li *sorvegliano* però non esistono ancora (v. S1).

*(c) 2026 Aviolab AI — documento di lavoro della revisione, non documentazione del prodotto.*
