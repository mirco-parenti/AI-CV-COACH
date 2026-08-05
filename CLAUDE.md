# regole_di_progetto — AI-CV-COACH

Queste sono le **`regole_di_progetto`** di AI-CV-COACH: valgono **solo** in questo progetto.
Le mie regole valide in ogni progetto (`regole_globali`) stanno in `~/.claude/CLAUDE.md`.
Convenzione di scrittura: **"io" = Mirco, "tu" = tu, l'assistente.**

## Regole

1. **Sync prompt ↔ codice**: ogni prompt vive **identico** in `HTML+JS/prompt_design.md` e
   `HTML+JS/server.js`. Confronta **char-by-char** normalizzando CRLF/LF, e **verifica a ogni
   modifica** di un prompt. (Nella fase VB.NET la regola migrerà sulla libreria di prompt
   `.md`: sarà ridefinita quando quella nascerà.)
2. **Stile del diario** (`diario_di_bordo.md`): `### Step X.Y — titolo`, intro in
   corsivo, sezioni *Cosa ho fatto / Cosa ho imparato / Dove ho faticato / Cosa ho
   deciso e perché*, callout 💡, **prima persona** (io = Mirco), in italiano.
   **Non riscrivere gli step passati** (sono storia): lavoro nuovo = step nuovo.
3. **Asset durevoli vs usa-e-getta**: investi qualità sui **durevoli = PROMPT + SCHEMA**
   (`HTML+JS/prompt_design.md`), che migrano verso VB.NET. L'infrastruttura — `index.html`,
   l'aiutante Node, i `test-*.html` — è **impalcatura usa-e-getta**, non da studiare
   riga per riga.
4. **Documentazione senza duplicati**: i prompt **definitivi** stanno in
   `HTML+JS/prompt_design.md`, la **narrazione e le decisioni** nel `diario_di_bordo.md`.
   Non duplicare lo stesso contenuto nei due.
5. **Comando "aggiorna-tutto"**: quando dico **"aggiorna-tutto"**, aggiorna **tutti i
   file di progetto nel working tree (tracciati o no)** — incluso questo `CLAUDE.md` —
   al livello a cui siamo arrivati. Procedi **un file alla volta**, seguendo
   **scrupolosamente la modalità di compilazione specifica di ciascun file** (vedi la
   tabella **«Modalità di aggiornamento per file»** in fondo). È
   **severamente vietato confondere i contenuti di un file con quelli di un altro —
   idem per la forma e il mood** di compilazione. **Sempre doppio controllo.**
   È un **rito di verifica completo**: si ri-verifica **ogni** file, uno alla volta,
   **anche quelli già modificati nella stessa sessione**. Per ciascuno: **rilettura
   integrale** → modalità specifica → aggiornamento se serve (o conferma «non serve»
   **solo dopo** la verifica, mai prima) → doppio controllo. **Mai saltare un file** con
   la motivazione «già allineato».
   **Esclusi sempre**: `.env`, `.claude/`, `node_modules/` e tutto ciò che è gitignored;
   i file di config (`.gitignore`, `.gitattributes`, `package.json`) si toccano solo se
   serve. È **repo-scoped**: non tocca `~/.claude/CLAUDE.md` (regole_globali) né l'auto-memoria.
6. **Marker regole nuove**: quando emerge una possibile nuova `regola_di_progetto`,
   **proponimela in chat marcata con `🔖 REGOLA NUOVA/regole_di_progetto` e chiedimi conferma**; aggiungila
   a questo file **solo dopo il mio ok** (niente aggiunte autonome). Il marker resta
   accanto alla regola finché non è **ratificata** (con "aggiorna-tutto" o quando lo dico).
7. **Idee future**: tieni `idee_future.md` come **unica
   raccolta** dei raffinamenti/idee per le fasi successive. Quando emerge un'idea da
   segnare, **annotala lì** (con data e motivo) e **consultalo a inizio di ogni nuova
   fase**. È aggiornato da "aggiorna-tutto"; in questo file ne resta solo il puntatore.
8. **Compilazione di un file**: prima
   di compilare qualunque file, rileggilo per intero, capiscine contenuto, stile e
   struttura; poi aggiornalo solo se serve. Se lo aggiorni devi farlo con lo stesso stile
   e la stessa struttura. Poi passa al successivo. Se non serve aggiornarlo, passa
   direttamente al successivo.
9. **Nomenclatura dei due CV (anello 4)**: in chat distingui **sempre** i due output
   della generazione con marker chiari e distinti — **📄 CV-1** (base, generato dopo
   l'anello 1) e **🎯 CV-2** (mirato, generato dopo l'anello 3). Usali ogni volta che
   parli dei due CV.
10. **Riservatezza TTR-SUITE**: nessun dettaglio implementativo di TTR-SUITE nei file
   di questo repo (pubblico) — niente nomi di file/classi, architetture o meccanismi
   interni. Il family feeling si specifica come design **proprio** di AI-CV-COACH
   (`VB.NET/progetto/03_interfaccia_grafica.md`). Eventuali strumenti della suite si
   includono **solo come eseguibili binari**, solo se veramente utili. *(Ratificata
   2026-08-05.)*
11. **Due postazioni, un proprietario**: il repo vive sotto `mirco-parenti` (unico
   proprietario); `rpsnoopy` è collaborator. Ogni macchina committa con la **propria
   identità git** (aviolab03 = mirco-parenti; la macchina del tutor = Riccardo
   Parenti): l'attribuzione resta onesta. Regola d'oro di sessione: `git pull`
   all'inizio, push alla fine. Per le tappe di implementazione (T1–T9) si lavora su
   rami e **il merge su `main` lo fa Mirco**; per documentazione e diario è ammesso
   il push diretto. *(Ratificata 2026-08-05.)*
12. **Attribuzione dei commit**: ogni messaggio di
   commit (e ogni corpo di PR) si chiude con la sola riga `(c) 2026 Aviolab AI`.
   **Nessuna menzione dell'assistente o dello strumento usato** — niente
   `Co-Authored-By: Claude`, niente «Generated with Claude Code». L'identità git resta
   quella della postazione (regola 11). *(Ratificata 2026-08-05.)*

## Contesto del progetto (fatti stabili + puntatori, niente stato copiato)

- **Due aree del repo** (dal 2026-08-05): `HTML+JS/` = il prototipo web validato (fase MVP,
  congelato salvo manutenzione); `VB.NET/` = tutto lo sviluppo da qui in avanti (mandato
  verbatim in `VB.NET/PROMPT_DI_INCARICO.md`, progetto dettagliato in `VB.NET/progetto/`).
  I file trasversali (questo `CLAUDE.md`, `README.md`, `diario_di_bordo.md`,
  `idee_future.md`) restano in radice.
- **Come far girare il prototipo**: Node ≥ 20.12, **niente dipendenze npm**, chiave in
  `HTML+JS/.env` (`ANTHROPIC_API_KEY`, gitignored). Avvio: `npm start` **dentro
  `HTML+JS/`** → `http://localhost:3000`.
  Endpoint: `POST /struttura` (turni del profilo, analisi annuncio e import da CV `importa_cv`), `POST /leggi-pdf` (trascrizione di un CV in PDF), `POST /confronta`, `POST /mitiga`, `POST /genera-cv` e `POST /genera-lettera`. Stop del server: `fuser -k 3000/tcp`.
- **Stato e pipeline (fonte viva)**: per pipeline e stato aggiornati vedi
  `README.md` (sezione *Stato*) e l'ultimo `### Step` del `diario_di_bordo.md`.
  **Non duplicare qui lo stato** (così questo file non va mai stantio).
- **Architettura (puntatore)**: il disegno **top-down** del sistema — funzioni (voci 2.x ↔
  anelli 1-4), vista-dati ("un profilo, molti CV"), principi trasversali, runtime e gap
  aperti — è in `HTML+JS/architettura.md`. È la bussola per progettare i componenti mancanti; il
  file è **statico-strutturale** (si aggiorna solo quando cambia il disegno, non lo stato).
- **Modelli (puntatore)**: quali modelli si usano e con che criterio (estrazione vs
  ragionamento) è in `HTML+JS/prompt_design.md` ("Modelli usati") e nelle costanti
  `MODEL_SEMPLICE` / `MODEL_RAGIONAMENTO` di `HTML+JS/server.js`.
- **Bussola etica del prodotto (puntatore)**: il vincolo **anti-invenzione** è descritto
  in `README.md` ("Vincolo etico principale") ed è codificato nei prompt di
  `HTML+JS/prompt_design.md` — **rispettalo quando progetti/modifichi i prompt**. È una regola
  **del prodotto**, non una mia regola di lavoro.
- **Anti-perdita (puntatore)**: il gemello simmetrico dell'anti-invenzione — nulla di ciò
  che l'utente dichiara va **perso** se detto nel turno sbagliato (campo `altrove`:
  instradamento ad altri turni, conferma dell'utente, e per l'irriducibile un esplicito
  "lasciato fuori", mai una perdita silenziosa). Descritto in `HTML+JS/prompt_design.md`
  ("Convenzione anti-perdita: il campo `altrove`") e narrato nel `diario_di_bordo.md`
  (Step 1.26). Anche questa è una regola **del prodotto**.
- **Idee/raffinamenti futuri (puntatore)**: il backlog ragionato per le fasi successive
  è in `idee_future.md`.

## Modalità di aggiornamento per file (per «aggiorna-tutto»)

Riferimento operativo per la regola #5: **come** va trattato ciascun file. Quando nasce
un file nuovo, aggiungi qui la sua riga.

| File | Come aggiornarlo con "aggiorna-tutto" |
|---|---|
| `README.md` | Aggiorna la sezione **Stato** (riga in cima + "## Stato del progetto") e "Tecnologie previste" se cambiano; non riscrivere il resto della presentazione. |
| `HTML+JS/prompt_design.md` | Prompt **definitivi** + schema; prompt **identici** a `server.js` (sync char-by-char). |
| `HTML+JS/server.js` | Codice del motore; prompt **identici** a `prompt_design.md`. |
| `diario_di_bordo.md` | **Aggiungi un nuovo `### Step X.Y`** (intro corsivo, sezioni, 💡, prima persona). **Mai** riscrivere gli step passati. |
| `idee_future.md` | Aggiungi le idee nuove / spunta quelle realizzate con ✅ + puntatore; quando si accumulano, raccoglile in una sezione «Realizzate» in fondo (così il backlog attivo resta solo-futuro e non induce in errore); non copiare lo stato. |
| `CLAUDE.md` | Ratifica i marker confermati (togli 🔖); riflette regole e contesti aggiornati. |
| `HTML+JS/research_notes.md` | **Statico**: solo se c'è nuova ricerca su progetti simili. |
| `HTML+JS/architettura.md` | **Statico-strutturale**: si tocca solo quando cambia il **disegno** (un gap chiuso, un componente nuovo, un confine spostato), **non** per lo stato corrente (che rimanda a README/diario). Mai riscrivere l'impianto concordato col tutor senza richiesta esplicita. |
| `HTML+JS/index.html` e ogni `HTML+JS/test-*.html` | Impalcatura usa-e-getta: solo se è cambiato il front-end/test (qualità minore). |
| `VB.NET/PROMPT_DI_INCARICO.md` | **Statico**: è il mandato verbatim della fase VB.NET, **mai** riscritto; si estende solo con nuove integrazioni verbatim dell'utente. |
| `VB.NET/progetto/*.md` | Progetto dettagliato della fase VB.NET: si aggiorna quando una **decisione di progetto** cambia o matura (design-first: finché non si implementa, questi file sono la verità del disegno). Rispetta indice e struttura dei capitoli. |
| `.gitignore`, `.gitattributes`, `HTML+JS/package.json` | Config: solo se serve un cambiamento concreto. |
| `.env`, `.claude/`, `node_modules/`, gitignored | **MAI** toccati. |
