# regole_di_progetto — AI-CV-COACH

Queste sono le **`regole_di_progetto`** di AI-CV-COACH: valgono **solo** in questo progetto.
Le mie regole valide in ogni progetto (`regole_globali`) stanno in `~/.claude/CLAUDE.md`.
Convenzione di scrittura: **"io" = Mirco, "tu" = tu, l'assistente.**

## Regole

1. **Il pool è la casa dei prompt**: ogni prompt vive in **un posto solo**, il suo file
   `.md` nel pool (`VB.NET/src/TrovaLavoro/prompt-pool/`), e da lì entra nell'eseguibile
   come risorsa. Non ci sono più due copie da tenere identiche: al loro posto c'è il
   **manifest**, che porta l'impronta SHA-256 di ogni file, calcolata sul testo
   normalizzato a LF. Perciò **ogni modifica a un prompt si chiude col rito del bump**
   (cap. 04.5): si aggiorna `versione_pool`, si rigenerano le impronte (`Sigilla`), si
   annota il cambiamento. Un file fuori impronta **non blocca niente** — il caricatore è
   trasparente, non poliziesco — ma la versione si mostra con l'asterisco: chi sperimenta
   lo fa alla luce del sole, chi distribuisce fa il bump. *(Riscritta il 2026-08-06: fino
   a Pool 1.00 la regola valeva sulla doppia copia `prompt_design.md` ↔ `server.js` del
   prototipo, che non esiste più.)*
2. **Stile del diario** (`diario_di_bordo.md`): `### Step X.Y — titolo`, intro in
   corsivo, sezioni *Cosa ho fatto / Cosa ho imparato / Dove ho faticato / Cosa ho
   deciso e perché*, callout 💡, **prima persona** (io = Mirco), in italiano.
   **Non riscrivere gli step passati** (sono storia): lavoro nuovo = step nuovo.
3. **Asset durevoli vs usa-e-getta**: investi qualità sui **durevoli = PROMPT + SCHEMA**,
   che ora vivono nel **pool** (cap. 04). Sono l'unica cosa migrata pari pari dal
   prototipo, e sopravvivranno anche a questa fase. L'impalcatura del prototipo —
   `index.html`, l'aiutante Node, i `test-*.html` — è **usa-e-getta e ormai congelata**:
   non si studia riga per riga. Nella fase VB.NET quella distinzione non si applica più
   allo stesso modo: il codice di `VB.NET/src/` non è impalcatura, è il prodotto.
4. **Documentazione senza duplicati**: i prompt **definitivi** stanno nel **pool**, il
   **disegno** in `VB.NET/progetto/`, la **narrazione e le decisioni** nel
   `diario_di_bordo.md`. Non duplicare lo stesso contenuto in due posti: quando poi
   divergono, chi legge non sa quale dei due sia quello vero.
5. **Comando "aggiorna-tutto"**: quando dico **"aggiorna-tutto"**, aggiorna **tutti i
   file di progetto nel working tree (tracciati o no)** — incluso questo `CLAUDE.md` —
   al livello a cui siamo arrivati. Il perimetro è la **fase viva**: tutto `VB.NET/`
   (progetto *e* codice), `strumenti/` (gli attrezzi di sviluppo, dal 2026-08-10),
   `immagini/` (gli asset del marchio, dal 2026-08-22), `README.md`,
   `GUIDA.md` (la guida per chi usa il programma, dal 2026-08-27),
   `LICENSE` (dal 2026-08-26), `diario_di_bordo.md`,
   `idee_future.md`, `in_sospeso.md`, questo
   `CLAUDE.md` e ogni altra regola o documentazione di progetto che nascerà.
   **`HTML+JS/` è fuori dal rito** *(dal 2026-08-06)*: il prototipo è congelato, non è
   più la fase in cui lavoro, e riverificarlo a ogni giro costerebbe senza rendere. Si
   tocca **solo** per manutenzione esplicitamente richiesta — e in quel caso valgono
   ancora le sue regole di sempre (regola 1 sul sync prompt ↔ codice, regola 3 sugli
   asset durevoli). Procedi **un file alla volta**, seguendo
   **scrupolosamente la modalità di compilazione specifica di ciascun file** (vedi la
   tabella **«Modalità di aggiornamento per file»** in fondo). È
   **severamente vietato confondere i contenuti di un file con quelli di un altro —
   idem per la forma e il mood** di compilazione. **Sempre doppio controllo.**
   È un **rito di verifica completo**: si ri-verifica **ogni** file, uno alla volta,
   **anche quelli già modificati nella stessa sessione**. Per ciascuno: **rilettura
   integrale** → modalità specifica → aggiornamento se serve (o conferma «non serve»
   **solo dopo** la verifica, mai prima) → doppio controllo. **Mai saltare un file** con
   la motivazione «già allineato».
   **Esclusi sempre**: `HTML+JS/` (prototipo congelato, vedi sopra), `.env`, `.claude/`,
   `node_modules/` e tutto ciò che è gitignored; i file di config (`.gitignore`,
   `.gitattributes`, `package.json`) si toccano solo se serve. È **repo-scoped**: non
   tocca `~/.claude/CLAUDE.md` (regole_globali) né l'auto-memoria.
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
13. **Cose in sospeso**: tieni `in_sospeso.md` come **unica raccolta** di ciò che è
   **già dentro il perimetro** ma è rimasto indietro. Alla **chiusura di ogni tappa**
   annota lì cosa resta e perché, invece di lasciarlo solo dentro i capitoli;
   **consultalo a inizio di ogni tappa nuova** per vedere se qualcosa si può chiudere
   adesso. È aggiornato da "aggiorna-tutto"; in questo file ne resta solo il puntatore.
   Gemella della regola 7 e da tenere distinta: in `idee_future.md` un'idea può non
   farsi mai, qui una voce **va fatta**. *(Ratificata 2026-08-07.)*
14. **Un collaudo si prova a farlo fallire prima di dirlo buono**: quando un collaudo
   nasce per sorvegliare un meccanismo preciso, **falsifica apposta** ciò che deve
   difendere — rompi il codice sotto — e guarda se diventa **rosso**. Un collaudo verde
   dice che il codice fa quel che ti aspetti; solo un collaudo che hai **visto** diventare
   rosso dice che si accorgerebbe del contrario. Vale soprattutto per i collaudi che
   misurano una **proprietà** — concorrenza, esclusione, limiti di tempo — dove il caso
   normale passa comunque e la prova può essere verde **per il motivo sbagliato**: allora
   dichiara anche **quali** collaudi sono caduti falsificando, perché quelli rimasti verdi
   coprono altro. *(Ratificata 2026-08-19, dopo tre casi in due giorni: il ciclo del
   server reso di nuovo seriale (T8b), il lucchetto reso permissivo (T8c) e il client
   dello streaming privato del riarmo del silenzio — dove il collaudo restò verde ed era
   cieco.)*
15. **Una tappa col collaudo rimandato è «chiusa con riserva», e la riserva si scrive**:
   il cap. 14 dice già che una tappa si chiude **con** il suo collaudo, e rimandarlo a volte
   è inevitabile — manca una macchina, manca un client, tocca a un'altra sessione. Quel che
   non è ammesso è chiamarla **chiusa** lo stesso: si dichiara la riserva (che cosa non è
   stato provato, e perché), si annota in `in_sospeso.md`, e la tappa resta **aperta con
   riserva** finché quel collaudo non è fatto. Un collaudo di tappa non è la cerimonia che
   ratifica ciò che si è già verificato: è l'unica parte che verifica ciò che non si sapeva
   di non aver verificato. *(Ratificata 2026-08-21: T8 fu dichiarata chiusa il 19 agosto col
   suo collaudo spostato in `in_sospeso.md`, e due giorni dopo quel collaudo trovò due
   difetti che 817 collaudi verdi non potevano vedere — uno vive nello spazio fra le due
   porte, l'altro è il modello che disobbedisce a una riga del prompt.)*
16. **Una tappa si chiude rileggendo anche quel che aveva *promesso***: la regola 15 dice di
   dichiarare la riserva, questa dice **dove cercarla**. Alla chiusura si torna all'elenco con
   cui la tappa era stata **aperta** — la sua riga nel cap. 14, impegno per impegno — e si
   spunta ciascuno, non solo quelli che qualche tempo ha raccolto per strada. Perché un
   impegno che **nessun tempo ha preso in mano** non lascia traccia da nessuna parte: non è un
   difetto, non è un debito annotato in `in_sospeso.md`, non è un collaudo rosso. Semplicemente
   non esiste, e sparisce in silenzio — mentre tutto il resto del metodo (i collaudi, le
   falsificazioni, il rito, `in_sospeso.md`) sorveglia solo ciò che si è **fatto**. Quel che
   manca si annota in `in_sospeso.md` come le altre riserve. *(Ratificata 2026-08-24,
   chiudendo T9e: la **demo (video) per il portfolio**, dichiarata da T9 il 5 agosto nella
   stessa riga del diario, del README e del tag `v1.0`, era rimasta invisibile per diciannove
   giorni. Non l'ha trovata nessun controllo su quel che era stato fatto: l'ha trovata la
   rilettura di quel che era stato promesso.)*
17. **«fammi vedde» vuol dire *aprimi l'app*, e
   aggiornata**: quando dico **«fammi vedde»** (o «fammi vedere», «aprimi l'app»), non mi
   mandi schermate né percorsi di immagini: **mi apri l'eseguibile che sta sul Desktop**,
   `C:\Users\Mirco Parenti\Desktop\TrovaLavoro.exe`, dopo esserti **assicurata prima** che
   porti dentro **tutte** le modifiche — quelle di questa sessione, quelle appena fatte e
   quelle delle sessioni precedenti, committate o no. Si rifà con
   `strumenti\aggiorna-riferimento.bat` (da WSL: `cmd.exe /c "<percorso completo>"`), che
   compila in `%TEMP%` — così il file bloccato dal server MCP non c'entra —, chiude
   **solo** l'app del Desktop riconoscendola dal percorso, e dichiara versione, commit e
   impronta di quel che ha appena scritto: quella riga è la prova che l'exe è aggiornato,
   e va guardata. Il riferimento sul Desktop **dev'essere pronto all'uso in ogni momento e
   sempre aggiornato all'ultima modifica**: è *l'app*, non una copia di comodo, e un exe
   vecchio che si apre senza dirlo è peggio di nessun exe. *(Dettata il 2026-08-31,
   ratificata il 2026-09-01.)*

## Contesto del progetto (fatti stabili + puntatori, niente stato copiato)

- **Due aree del repo** (dal 2026-08-05): `HTML+JS/` = il prototipo web validato (fase MVP,
  congelato salvo manutenzione); `VB.NET/` = tutto lo sviluppo da qui in avanti (mandato
  verbatim in `VB.NET/PROMPT_DI_INCARICO.md`, progetto dettagliato in `VB.NET/progetto/`).
  I file trasversali (questo `CLAUDE.md`, `README.md`, `diario_di_bordo.md`,
  `idee_future.md`) restano in radice.
- **Come far girare il prototipo** (serve ancora: è il **giudice** della non-regressione,
  cap. 14 — le batterie di T2 e T3 sono passate, ma i collaudi reali restano rieseguibili
  e gli attesi del banco si rigenerano da `server.js`; a T3 ha fatto da giudice
  sull'import di un CV e su un turno del dialogo. Attenzione: da **Pool 1.01** su
  `importa_cv`, dal **Pool 1.02** anche sui sette turni del profilo e dal **Pool 1.03** su
  `analisi_annuncio`, non è più il metro
  — è il termine di paragone, cap. 04.7; il metro carattere-per-carattere resta su
  `confronto` e `mitigazione`, ma dal **2026-08-18** solo sul **testo che parte**: il
  ragionamento dell'app è passato a Sonnet 5 mentre il prototipo resta congelato su
  Sonnet 4.6, quindi sugli **esiti** una differenza può venire dal modello quanto dal
  codice, e lì termine di paragone lo è ovunque): Node ≥ 20.12,
  **niente dipendenze npm**, chiave in
  `HTML+JS/.env` (`ANTHROPIC_API_KEY`, gitignored — il file va lì, perché il server
  cerca il `.env` nella cartella da cui parte). Avvio: `npm start` **dentro
  `HTML+JS/`** → `http://localhost:3000`.
  Endpoint: `POST /struttura` (turni del profilo, analisi annuncio e import da CV `importa_cv`), `POST /leggi-pdf` (trascrizione di un CV in PDF), `POST /confronta`, `POST /mitiga`, `POST /genera-cv` e `POST /genera-lettera`. Stop del server: `fuser -k 3000/tcp`.
- **Stato e pipeline (fonte viva)**: per pipeline e stato aggiornati vedi
  `README.md` (sezione *Stato*) e l'ultimo `### Step` del `diario_di_bordo.md`.
  **Non duplicare qui lo stato** (così questo file non va mai stantio).
- **Architettura (puntatore)**: la bussola viva è `VB.NET/progetto/02_architettura.md` —
  componenti, chiamate all'AI (§2.5), concorrenza, dove vive lo stato. Il disegno
  **top-down** originale — funzioni (voci 2.x ↔ anelli 1-4), vista-dati ("un profilo,
  molti CV"), principi trasversali — resta in `HTML+JS/architettura.md`: è la radice da
  cui nasce il progetto VB.NET e si legge ancora per capire il *perché*, ma il *come* di
  oggi sta nei capitoli di `VB.NET/progetto/`. Entrambi sono **statico-strutturali**: si
  toccano quando cambia il disegno, non per lo stato corrente.
- **Modelli (puntatore)**: il criterio dei due livelli (estrazione vs ragionamento) e i
  modelli concreti stanno in `VB.NET/progetto/02_architettura.md` §2.5. I prompt del pool
  dichiarano solo il **livello** (`modello: semplice|ragionamento`); i modelli veri sono i
  predefiniti di `VB.NET/src/TrovaLavoro/Ai/Modelli.vb`, scavalcabili da `modelli.json`
  nella cartella dati. Nel prototipo erano le costanti `MODEL_SEMPLICE` /
  `MODEL_RAGIONAMENTO` di `HTML+JS/server.js`, che restano il termine di paragone del
  collaudo di non-regressione.
- **Bussola etica del prodotto (puntatore)**: il vincolo **anti-invenzione** è descritto
  in `README.md` ("Vincolo etico principale") ed è **codificato dentro i prompt del pool**
  — **rispettalo quando progetti/modifichi un prompt**. È una regola **del prodotto**, non
  una mia regola di lavoro.
- **Anti-perdita (puntatore)**: il gemello simmetrico dell'anti-invenzione — nulla di ciò
  che l'utente dichiara va **perso** se detto nel turno sbagliato (campo `altrove`:
  instradamento ad altri turni, conferma dell'utente, e per l'irriducibile un esplicito
  "lasciato fuori", mai una perdita silenziosa). Vive nei prompt del pool, è narrato nel
  `diario_di_bordo.md` (Step 1.26) e la sua origine è documentata in
  `HTML+JS/prompt_design.md` ("Convenzione anti-perdita: il campo `altrove`"). Anche
  questa è una regola **del prodotto**.
- **Idee/raffinamenti futuri (puntatore)**: il backlog ragionato per le fasi successive
  è in `idee_future.md`.
- **Cose rimaste indietro (puntatore)**: ciò che è **già nel perimetro** ma aspetta il
  momento, una macchina o una mano — il collaudo su un PC pulito, i documenti aperti in
  Word, i difetti di scala a DPI alti — sta in `in_sospeso.md`. Da non confondere con
  `idee_future.md`: lì un'idea può non farsi mai, qui una voce **va fatta**.
- **Lo strumento di collaudo (puntatore)** *(dal 2026-08-10)*: in `strumenti/mcp-collaudi/`
  c'è un server MCP locale con cui **provare l'applicazione vera** — compilarla, far girare
  il banco, avviarla (anche su una **cartella dati usa-e-getta**, dal 2026-08-14),
  fotografarla, **ridimensionarne la finestra**, elencare i controlli dicendo se sono accesi
  e che voce
  mostrano i menù, premerli, scrivere in una casella, scegliere una voce da una tendina o
  una riga da un elenco, rispondere alla finestra di scelta file e alle finestre di
  conferma (leggendo prima cosa chiedono) e — dal 2026-08-18 — **aspettare che una
  condizione si avveri** invece di guardare a intervalli. Non parte da solo
  (`node strumenti/mcp-collaudi/server.mjs`) e **non** è il server MCP del prodotto, che è
  il cap. 09 ed è stato costruito a T8 *(2026-08-19)*: due server distinti, da non
  confondere quando si parla di «MCP» in questo repo. Il server **del prodotto** ha avuto il
  suo collaudo da un client vero il **2026-08-21**, e il client vero è **Claude Code stesso**:
  registrandolo fra i suoi server MCP, i tool (dodici allora, **tredici** da T9a) compaiono
  come `mcp__trovalavoro__*` e
  si possono chiamare come qualunque altro strumento. La registrazione vive **fuori dal repo**
  (nella configurazione di Claude Code) e i server si caricano **solo all'avvio**: se i tool
  non ci sono, serve un riavvio della sessione, non una modifica al codice. Come si
  accende, cosa sa fare e le
  trappole già pagate stanno nel suo `README.md`: **leggilo prima di usarlo**, sono ore
  risparmiate.

## Modalità di aggiornamento per file (per «aggiorna-tutto»)

Riferimento operativo per la regola #5: **come** va trattato ciascun file. Quando nasce
un file nuovo, aggiungi qui la sua riga.

| File | Come aggiornarlo con "aggiorna-tutto" |
|---|---|
| `README.md` | Aggiorna la sezione **Stato** (riga in cima + "## Stato del progetto") e "Tecnologie previste" se cambiano; non riscrivere il resto della presentazione. |
| `GUIDA.md` | **Guida di chi usa il programma**, non di chi lo costruisce: si aggiorna quando cambia qualcosa che l'utente **vede o fa** — un requisito, un passo del primo avvio, un messaggio d'errore, dove finiscono i dati. Non ci entra nulla di implementativo, e non si duplica lo stato del progetto: quello sta nel README. *(dal 2026-08-27.)* |
| `LICENSE` | **Statico**: la licenza del repository. Si tocca solo se la licenza cambia davvero. *(dal 2026-08-26.)* |
| `diario_di_bordo.md` | **Aggiungi un nuovo `### Step X.Y`** (intro corsivo, sezioni, 💡, prima persona). **Mai** riscrivere gli step passati. |
| `idee_future.md` | Aggiungi le idee nuove / spunta quelle realizzate con ✅ + puntatore; quando si accumulano, raccoglile in una sezione «Realizzate» in fondo (così il backlog attivo resta solo-futuro e non induce in errore); non copiare lo stato. |
| `in_sospeso.md` | Aggiungi le voci rimaste indietro nella tappa appena chiusa (cosa manca · perché · dove ne parla il progetto, con la data); sposta in «Chiuse» quelle risolte, dicendo come. Solo cose **già dentro il perimetro**: le idee da valutare restano in `idee_future.md`. Non copiare lo stato. |
| `CLAUDE.md` | Ratifica i marker confermati (togli 🔖); riflette regole e contesti aggiornati. |
| `VB.NET/PROMPT_DI_INCARICO.md` | **Statico**: è il mandato verbatim della fase VB.NET, **mai** riscritto; si estende solo con nuove integrazioni verbatim dell'utente. |
| `VB.NET/progetto/*.md` | Progetto dettagliato della fase VB.NET: si aggiorna quando una **decisione di progetto** cambia o matura (design-first: finché non si implementa, questi file sono la verità del disegno). Rispetta indice e struttura dei capitoli. |
| `VB.NET/src/**` | Codice e collaudi. **Non si riscrive per allineamento**: si tocca quando una decisione di progetto o una tappa lo richiede. Commenti e nomi in italiano come il codice già presente; ogni modifica passa da `dotnet test` **prima** di dirsi fatta. Le cartelle `obj/` e `bin/` non si toccano mai. |
| `immagini/**` | Gli asset del marchio che il repo usa davvero (testata del README, disegno sorgente) più il loro `LEGGIMI.md`. **Non si riscrivono per allineamento**: un PNG si tocca solo se il marchio cambia, e allora si rigenera dalla ricetta (`VB.NET/progetto/prompt-logo.md`). La lavorazione — scarti, formati social, editor del lettering — **resta fuori dal repo** e non entra mai col rito. |
| `VB.NET/progetto/prompt-logo.md` | **Statico**: è la ricetta verbatim con cui il marchio è stato generato, non un capitolo di progetto. Si tocca solo se il marchio cambia davvero, annotando la revisione come fa il file stesso. |
| `strumenti/**` | Attrezzi di sviluppo, **fuori dal prodotto** (non entrano nell'exe, non si distribuiscono). Stessa regola del codice: non si riscrivono per allineamento, si toccano quando servono. Il loro `README.md` però sì: ogni trappola nuova pagata sul campo va scritta lì, perché è il solo posto in cui quel sapere sopravvive. |
| `HTML+JS/**` | **Fuori dal rito** *(dal 2026-08-06)*: prototipo congelato. Si tocca solo per manutenzione che chiedo esplicitamente; in quel caso valgono le regole 1 e 3. |
| `situazione-*.txt` in radice | **Mai aggiornate**: sono fotografie di una situazione a una certa data (come uno Step del diario), e riscriverle ne distruggerebbe il senso. Si toccano solo per cancellarle, se e quando lo dico. |
| `revisione-*.md`, `istruzioni-fix-ui.md`, `fix-ui-avanzamento.md` | **Mai aggiornati** *(dal 2026-09-01)*: sono i registri della revisione di finalizzazione — traccia delle tre fasi, report di sicurezza, mandato e log dell'orchestrazione dei fix UI. Documenti **di lavoro**, non documentazione del prodotto: come le `situazione-*.txt` valgono per la data che portano, e riscriverli ne cancellerebbe il senso. Si toccano solo per cancellarli, a pull request integrata. Quel che di loro deve sopravvivere è già nel `diario_di_bordo.md` e in `in_sospeso.md`. |
| `.gitignore`, `.gitattributes` | Config: solo se serve un cambiamento concreto. |
| `.mcp.json` | Config dello **strumento di collaudo** (dichiara il server MCP locale su `127.0.0.1:3300`, vedi `strumenti/mcp-collaudi/`): si tocca solo se cambia il modo di accenderlo. Non è configurazione del prodotto. |
| `.env`, `.claude/`, `node_modules/`, gitignored | **MAI** toccati. |
