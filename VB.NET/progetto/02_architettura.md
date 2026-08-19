# 02 — Architettura dell'applicazione

*Come è fatta dentro: i blocchi, i dati che si scambiano, cosa migra dal prototipo e
cosa nasce nuovo. Il disegno funzionale (voci 2.x, anelli) resta quello di
`HTML+JS/architettura.md`; qui si progetta la sua incarnazione desktop.*

## 2.1 Vista d'insieme

Un solo processo, un solo exe. Dentro, sei blocchi principali:

```
┌───────────────────────────────────────────────────────────────┐
│  INTERFACCIA (WinForms, pannelli statici)                     │
│  P1 Home/Registro · P2 Profilo · P3 Ricerca (WebView2) ·      │
│  P4 Opportunità · P5 Dialogo/Brainstorm · P6 Documenti ·      │
│  P7 Email · P8 Impostazioni (finestra separata)               │
├───────────────────────────────────────────────────────────────┤
│  MOTORE (la logica, senza interfaccia)                        │
│  Orchestratore dei flussi · CalcoloMatch · EstrattoreJson     │
│  Stati delle opportunità · Regole (soglia, hard-gate)         │
├──────────────┬────────────────────┬───────────────────────────┤
│  AI          │  DOCUMENTI         │  POSTA                    │
│  ClientClaude│  lettura PDF/DOCX/ │  composizione email       │
│  Libreria-   │  TXT/MD; scrittura │  scrittura .eml           │
│  Prompt      │  DOCX/PDF; scans.  │  (bozza da inviare)       │
│  (pool .md)  │  cartella doc.     │                           │
├──────────────┴────────────────────┴───────────────────────────┤
│  DATI (cartella dati su disco)                                │
│  profilo.json · opportunita/ · registro.json · config         │
│  chiave API cifrata · backup JSON                             │
└───────────────────────────────────────────────────────────────┘
          ▲
          │ (stesso exe avviato con --mcp)
   SERVER MCP: espone le funzioni del MOTORE ai client AI esterni
```

Regola di separazione, semplice ma vincolante: **l'interfaccia non parla mai
direttamente con l'API AI né con il disco** — passa sempre dal motore. È ciò che
permette al server MCP (cap. 09) di riusare *tutte* le funzioni senza duplicarle: MCP e
pannelli sono due «facce» dello stesso motore.

## 2.2 La pipeline di artefatti (il cuore che migra)

Come nel prototipo, l'architettura è **data-centrica**: i blocchi si scambiano pochi
artefatti JSON ben definiti. Quelli ereditati:

| Artefatto | Prodotto da | Consumato da |
|---|---|---|
| **Profilo JSON** | dialogo guidato, import CV (da file o dalla pagina aperta nel browser), aggiornamento periodico | tutto il resto (unica *fonte di fatti*) |
| **Annuncio JSON** | analisi annuncio (da cattura WebView2, link o testo incollato) | confronto, generazione (come *segnale di mira*) |
| **Giudizi + punteggio** | confronto (AI) + CalcoloMatch (codice) | scheda match, mitigazione, generazione |
| **Mitigazioni JSON** | mitigazione (può essere lista vuota) | la ✉️ lettera e — da T7c — il **brainstorming**, che senza vedere i ponti non potrebbe farne scegliere uno |
| **CV JSON** (base/mirato) | generazione | scrittura DOCX/PDF, lettera (riferimento di coerenza) |
| **Lettera JSON** | generazione | corpo email, allegato |

Artefatti **nuovi** della fase desktop:

| Artefatto | Cos'è |
|---|---|
| **Preferenze di ricerca** | tipologie di lavoro, zona, contratto, parole chiave, ricerche salvate per portale |
| **Opportunità** | annuncio + fonte/link + stato (`nuova → interessante → generata → inviata → esito`, con `scartata` che chiude la strada) + tutti gli artefatti prodotti per esso |
| **Appunti di mira** | l'esito confermato del brainstorming (cosa enfatizzare, tono); orientano la generazione, non aggiungono fatti |
| **Registro candidature** | vista d'insieme delle opportunità e dei loro stati, con date. *È l'unico artefatto **derivato**: si ricava dalle cartelle-opportunità e si può buttare senza perdere niente (cap. 07.3, cap. 11.1)* |
| **Backup JSON** | profilo (+ storico candidature a scelta) esportabile e reimportabile |

La proprietà «un profilo, molti CV» resta: il profilo è uno, versionato nel tempo; ogni
opportunità ha i **suoi** CV-2 e lettera, riconducibili alla versione di profilo usata.

## 2.3 I moduli del codice

Nomi indicativi (il dettaglio fine si stabilizza in implementazione), pensati perché
ogni modulo abbia **un compito solo**:

- **`Ui/`** — i Form e i pannelli (cap. 03). Nessuna logica: raccolgono input, mostrano
  risultati, chiamano il motore.
- **`Motore/`** —
  - `ContestoApp`: monta il motore all'avvio — cartella dati, pool, numeri, client AI,
    archivio — e **non solleva mai**: ciò che non si può montare resta spento e *detto*
    (cap. 03.8). È il punto in cui i pannelli trovano tutto già pronto.
    *Un'eccezione, trovata implementando T4c (2026-08-10): la **stampante PDF** e
    l'archivio dei documenti che la usa li costruisce la **finestra principale**, non il
    contesto. La stampa passa da una WebView2, che vuole il thread dell'interfaccia, e il
    contesto lo montano anche i collaudi — fuori da qualunque finestra. Metterla lì
    avrebbe legato il motore all'interfaccia proprio nel punto in cui il motore serve
    senza. **A T5a la stessa regola vale per il `MotoreBrowser`**, che la finestra
    costruisce per prima e passa sia alla stampante sia al pannello Ricerca: l'ambiente
    WebView2 dell'applicazione è **uno solo**, e questo è il punto in cui lo si garantisce;*
  - `DialogoProfilo` + `Mossa`: la macchina a mosse del dialogo guidato (v. 2.4);
  - `ImportProfilo`: i due passi dell'import di un CV, dal file al profilo. *La seconda
    porta — da un **testo già letto altrove** — era stata lasciata pronta a T3 e resta
    inutilizzata fino a T5d, quando ci entra la pagina del browser: la strutturazione non
    ha dovuto imparare niente, perché non ha mai saputo da dove venisse il testo;*
  - `Orchestratore`: conduce i flussi del cap. 12 (quale passo viene dopo quale).
    *Nasce a T4 nella sua prima forma concreta, `PipelineCandidatura`: annuncio →
    confronto → punteggio → mitigazione → i tre documenti. È il posto in cui il codice
    dice l'ordine dei passi una volta sola, invece di lasciarlo scritto nel pannello che
    li chiama — la stessa ragione per cui il dialogo passa da `Mossa` (v. 2.4);*
  - `Opportunita` *(T4)*: l'artefatto che tiene insieme una candidatura — annuncio,
    giudizi, punteggio, mitigazioni, i due documenti, e da quale versione di profilo
    sono nati. È ciò che `Dati/ArchivioOpportunita` scrive nella cartella del cap. 11.1.
    *Da T5b porta anche **da dove viene**: fonte e link della pagina catturata
    (cap. 11.1), e sa dire se l'annuncio è uscito **vuoto** dall'analisi — cioè se quella
    pagina un annuncio non lo conteneva (cap. 06.4);*
  - `VistaConfronto`, `VistaAnnuncio` *(T4c)*: le due **viste di sola lettura** promesse
    a T4a. Gli artefatti nuovi restano JSON grezzo — il profilo è tipizzato perché P2 lo
    edita campo per campo, un annuncio e dei giudizi si mostrano e basta — ma un pannello
    che li disegna non deve mettersi a rovistare fra i campi: la vista traduce una volta
    sola, e chi disegna riceve righe già pronte. Sono di sola lettura anche nel senso che
    non decidono niente: le stelle le calcola `CalcoloMatch`, i giudizi sono dell'AI;
  - `CalcoloMatch`: la trascrizione **fedele** di `calcolaMatch` del prototipo — stessi
    pesi (richiesto=5, preferenziale=1, importanza alta/media/bassa=5/3/1, contesto=0,2,
    fallback=3), stesso clamp asimmetrico −20/+10, stesso tetto eliminatorio a 20/100,
    stesse note. È codice deterministico: dato lo stesso input, stesso punteggio del
    prototipo (questo è anche il suo collaudo, cap. 14);
  - `EstrattoreJson`: la versione VB di `estraiJson` — toglie il recinto markdown,
    tenta il parse, e solo in caso di errore ritaglia dal primo `{` all'ultimo `}`;
  - `StatiOpportunita`: la macchina a stati del registro. *Costruita a T5c (2026-08-13) e
    nata in **`Dati/`** invece che qui, insieme a ciò che legge e scrive: uno stato che
    esiste solo per essere conservato in `stato.json` e riletto da lì appartiene ai dati
    più che al motore. Con lei `Dati/Registro` — la voce, l'indice e il suo archivio — e
    `Dati/CampiJson`, i lettori difensivi estratti perché adesso lo stesso `stato.json` lo
    leggono in due (l'opportunità e il registro) e devono capirlo allo stesso modo. Il
    motore ci mette del suo solo dove c'è una decisione: `Opportunita.Avanza`, che rifiuta
    i passaggi che il ciclo di vita non prevede, e la pipeline che li fa scattare.*
- **`Ai/`** —
  - `LibreriaPrompt`: carica i `.md` dal pool, ne legge i metadati, riempie i
    segnaposto (cap. 04);
  - `ClientClaude`: le chiamate HTTPS a `api.anthropic.com` (v. 2.5);
  - `StrutturatoreTurni`, `TrascrittorePdf`: i due mestieri che mettono in fila pool,
    client ed estrattore. Ognuno ha la **sua interfaccia** (`IStrutturatoreTurni`,
    `ITrascrittorePdf`), e non per gusto di astrazione: è la porta da cui il motore si
    stacca dall'AI, e senza quella porta il dialogo e l'import non si potrebbero
    collaudare tutti interi senza rete (cap. 14, T3).
  - *A T4 la famiglia cresce di tre* — `AnalizzatoreAnnuncio` (da testo a annuncio
    JSON), `Confrontatore` (giudizi e, quando serve, mitigazioni) e `Generatore` (📄 CV-1,
    🎯 CV-2, ✉️ lettera) — ognuno con la sua interfaccia, per la stessa ragione di prima:
    la pipeline intera dev'essere collaudabile senza rete. Sono tre e non uno perché il
    motore li chiama in **tre momenti diversi** del flusso e i collaudi devono poter
    sostituire un momento per volta; la meccanica che hanno in comune — carica il prompt,
    riempi i segnaposto, chiama, estrai il JSON — sta sotto, scritta una volta sola.
  - *A T6 se ne aggiungono due, con la stessa forma* — `CompositoreEmail`, che dalla
    ✉️ lettera ricava oggetto e corpo del messaggio (cap. 07.1), e
    `ClassificatoreDocumenti`, che smista la cartella documenti dell'utente (cap. 05.2).
    Nessuno dei due sta nella pipeline di T4, e non è una dimenticanza: la fila si percorre
    da sé fino ai documenti, mentre da lì in poi decide l'utente — a chi mandare, quali
    allegati, se spedire.
  - *A T7b (2026-08-18) ne arriva un settimo, e stavolta in due pezzi* —
    `Ai/Rifinitore` è il mestiere che parla con l'AI (tre prompt, uno per genere di prosa,
    cap. 08.2), e `Motore/Rifinitura` è l'unico posto che sa **quali** campi di un documento
    sono prosa e di che genere: estrae quelli, li manda a rifinire e rimette i testi al loro
    posto. La divisione non è simmetria: al modello arrivano **solo i campi-prosa**, quindi
    nomi, aziende e date non entrano nella richiesta e non possono tornarne cambiati. Chi
    decide cosa mandare appartiene al motore; chi lo manda, all'AI.
  - *A T7c (2026-08-18) l'ottavo, e con la stessa divisione in due* — `Ai/Brainstormatore`
    porta i due prompt del ragionamento (la conversazione e la distillazione degli appunti),
    e `Motore/Brainstorming` tiene la **conversazione**: sa che il contesto viaggia una
    volta sola nel primo messaggio, che i turni si alternano, e che due battute dell'utente
    di fila si **uniscono** invece di perdersi. Accanto nasce `Motore/AppuntiDiMira`, che è
    il posto dove sta scritta una volta sola la regola più importante della tappa: ai prompt
    che scrivono arrivano gli appunti e **mai** i fatti dichiarati in chat. Nel trasporto
    compare anche `Ai/FlussoSse`, che sa solo la grammatica degli eventi e niente di Claude
    (cap. 02.5).
- **`Documenti/`** — lettura (PDF via API con blocco `document`, DOCX/TXT/MD in
  locale), scrittura (DOCX e PDF), scansione e classificazione della cartella
  documenti (cap. 05).
  *In implementazione la lettura è nata altrove, e la divisione ha una ragione*: quella
  che tocca il disco sta in `Dati/LettoreDocumenti` (DOCX/TXT/MD), quella che passa
  dall'API in `Ai/TrascrittorePdf`. Questa cartella resta il posto della **scrittura**
  (DOCX/PDF, T4) e della scansione della cartella documenti (T6). *A T4 ci nascono
  `ScrittoreDocx` (lo ZIP OOXML del cap. 05.4), `StampantePdf` (la WebView2 fuori
  schermo del cap. 05.5) e il **modello di impaginazione** che li precede entrambi: un
  CV JSON diventa prima una pagina, e solo dopo un file. Le due stampanti partono dallo
  stesso contenuto e non si consultano fra loro — è la regola «un modello di contenuto,
  più stampanti» del cap. 05.*
  *Implementando (T4b) la famiglia è risultata di cinque*: `Impaginazione` produce la
  `PaginaDocumento` a blocchi, `ScrittoreDocx` e `ScrittoreHtml` la disegnano nei due
  formati — l'HTML è la pagina che `StampantePdf` fa stampare alla WebView — e
  `NomiDocumenti` decide come si chiamano i file (cap. 05.6). Sopra tutti sta
  `ArchivioDocumenti`, l'unico che sappia **dove** vanno: è quello che i pannelli di T4c
  chiameranno, e scrive prima tutti i DOCX e poi tutti i PDF, perché la stampa PDF è la
  sola che dipenda da un pezzo di Windows che potrebbe mancare. *La «vista tipizzata del
  CV» promessa qui a T4a è la pagina stessa*: una classe intermedia che ricalcasse lo
  schema del CV, per poi tradurlo comunque in blocchi, sarebbe stata un terzo modello da
  tenere allineato agli altri due.
- **`Web/`** *(nata a T5a, cresciuta a T5b)* — quel che riguarda il browser dentro
  l'applicazione (cap. 06). Ci stanno `MotoreBrowser`, che accende e custodisce
  l'**unico** ambiente WebView2 — lo chiedono la stampa PDF, che lavora fuori schermo, e
  il pannello Ricerca, che sta in piena vista — e `LettorePagina`, che porta fuori dal DOM
  titolo, indirizzo e testo visibile della pagina aperta. Anche quest'ultimo ha la sua
  interfaccia (`ILettorePagina`), per la stessa ragione dei mestieri dell'AI: le decisioni
  della cattura — il testo basta? da che portale viene? l'avevamo già presa? — devono
  potersi collaudare senza pretendere WebView2 e un thread STA.
  *A T5d il lettore impara una seconda cosa: **scendere** per la pagina prima di leggerla
  (`ScorriAsync`). Su un sito moderno le sezioni entrano nel documento mentre si scorre, e
  chi legge com'è si porta via l'intestazione credendola tutto (cap. 06.7). Resta un
  compito del lettore, non del pannello, perché è mestiere del DOM; ma **chi lo chiama
  decide se serve**: lo chiede l'import del CV, non la cattura dell'annuncio.*
- **`Posta/`** — composizione dell'email e scrittura del file `.eml` marcato come bozza
  da inviare (cap. 07). *Nella 1.0 non spedisce:* né `.msg` né SMTP (cap. 15, voci 8 e 9),
  quindi questo componente non tocca alcuna credenziale.
  *In implementazione (T6) quella cartella non è nata*, e la ragione è la stessa della
  lettura dei documenti: i tre pezzi appartengono a tre mestieri che esistevano già.
  `Ai/CompositoreEmail` è un mestiere dell'AI come gli altri, `Motore/BozzaEmail` è
  l'artefatto che l'utente corregge e che si salva in `email.json`, e
  `Documenti/ScrittoreEml` è **una stampante**: parte da un contenuto e produce un file,
  come `ScrittoreDocx` e `StampantePdf`. Una cartella `Posta/` avrebbe raccolto tre cose
  che non si somigliano, solo perché finiscono nella stessa email.
- **`Dati/`** — cartella dati, profilo e sue versioni, opportunità, registro,
  configurazione, chiave API cifrata, backup (cap. 11).
  *A T6 ci nascono i due pezzi che il capitolo prometteva da tempo*: `ArchivioSegreti`,
  che cifra la chiave API con la protezione dati di Windows (cap. 11.3), e la coppia
  `RaccoltaDocumenti` + `ArchivioRaccoltaDocumenti`, che tiene la cartella documenti
  dell'utente e le categorie riconosciute (cap. 05.2). Con loro `ScansioneDocumenti`, che
  legge quella cartella e ne assaggia i file: sta **qui** e non in `Documenti/` per la
  stessa divisione di sempre — tocca il disco e non chiama nessuno, come
  `LettoreDocumenti`, mentre `Documenti/` resta il posto di chi **scrive**.
- **`Mcp/`** — il server MCP: traduce le richieste del protocollo in chiamate al
  motore (cap. 09).
  *Nata a **T8** (2026-08-19), in tre gambe, e la divisione ricalca quella di sempre:*
  `ProtocolloMcp` e `RichiestaMcp` sanno la grammatica del protocollo e niente del
  prodotto; `CatalogoTool` dichiara i dodici strumenti e il loro schema; `ToolDiLettura`,
  `ToolDiAi` e `ToolDiScrittura` sono i tre gruppi che chiamano il motore. `ServerMcp`
  tiene il ciclo su stdio, ma il metodo che **risponde a un messaggio** sta fuori da quel
  ciclo, così il banco può interrogarlo senza avviare un processo. Nessun pezzo di motore è
  stato duplicato: quel che i tool fanno è comporre gli stessi mestieri che usano i
  pannelli. *Accanto, in `Dati/`, nasce `LucchettoDati` (cap. 09.4), e in `Motore/` —
  col pagamento dei debiti del 2026-08-19 — `FiloGrafico`, il filo STA con la pompa dei
  messaggi che permette di stampare un PDF da un processo che finestre non ne ha: era una
  classe del banco dai tempi di T4b, ed è passata al prodotto il giorno in cui è servita
  anche a lui.*

## 2.4 Cosa migra dal prototipo, e come

| Asset del prototipo | Destino in VB.NET |
|---|---|
| I 15 prompt + schemi (`prompt_design.md`) | diventano i file della **libreria prompt** (cap. 04), contenuto invariato salvo adattamento dei segnaposto |
| `calcolaMatch` + costanti (server.js) | `Motore/CalcoloMatch`, trascrizione 1:1 verificata con casi di collaudo identici |
| `estraiJson` (server.js) | `Motore/EstrattoreJson`, stessa strategia (percorso felice intatto, ripiego solo nel catch) |
| Criterio due modelli (Haiku estrazione / Sonnet ragionamento) | metadato `modello:` di ogni prompt del pool (cap. 04) |
| Macchina a stati del dialogo + magazzino `pending` (index.html) | rinasce nel motore (`Motore/DialogoProfilo`) — stessa logica e **stessi testi**: schede di conferma, instradamento `altrove`, guardia anti-rimbalzo, «lasciato fuori» esplicito. Là ogni passo disegnava da sé la pagina; qui produce una **`Mossa`** (cosa dire, cosa mostrare, cosa aspettarsi) e chi la mostra torna con una risposta o una scelta. La conseguenza è doppia: il pannello si limita a disegnare, e il dialogo intero si collauda senza interfaccia (cap. 14, T3). *Dal 2026-08-09 (revisione adversariale) la `Mossa` porta anche le **eco ancorate**: le parole dell'utente riproposte nel punto giusto della sequenza — prima del verdetto, non dopo — così il pannello disegna nell'ordine in cui il dialogo pensa, e nella passata finale ogni recupero ha la sua eco* |
| Soglia 1,5 stelle (index.html) | costante del motore, stesso comportamento (sconsiglia, non impedisce) |
| Banchi `test-*.html` | rinascono come collaudi del cap. 14 (per-anello, sugli stessi casi) |

Gli endpoint HTTP del prototipo **spariscono**: erano il confine tra browser e Node;
qui il confine equivalente è la firma delle funzioni del motore (e, verso l'esterno,
i tool MCP).

## 2.5 Le chiamate all'AI

- **Trasporto**: HTTPS dirette a `https://api.anthropic.com/v1/messages` con
  `HttpClient` di .NET — niente SDK, come il prototipo (header `x-api-key` +
  `anthropic-version`). Il corpo è lo stesso del prototipo: un solo messaggio `user`
  con il prompt già riempito; per i PDF, il blocco `document` in base64. *«Lo stesso»
  è stato preso alla lettera e verificato a T2: sugli stessi artefatti il prompt che
  parte è identico carattere per carattere al suo (cap. 14).*
- **Modelli**: due livelli come nel prototipo — `MODELLO_SEMPLICE` (estrazioni:
  **Claude Haiku 4.5**, `claude-haiku-4-5`) e `MODELLO_RAGIONAMENTO` (confronto,
  mitigazione, generazione, brainstorming: **Claude Sonnet 5**, `claude-sonnet-5`). Ci si
  è arrivati **in due tempi**, come previsto. Primo tempo: la batteria di non-regressione
  di T2 è girata su **Sonnet 4.6** (`claude-sonnet-4-6`), lo stesso del prototipo, perché
  **a parità di modello** una differenza nei risultati è una differenza di *codice* — ed
  è **passata il 2026-08-07**, con i due confronti reali che danno le stesse stelle da una
  parte e dall'altra. Secondo tempo, **il 2026-08-18**: Sonnet 4.6 è passato fra i modelli
  superati del listino e Sonnet 5 è diventato il predefinito compilato, con l'interruttore
  del ragionamento dichiarato spento (voce sotto). Costa anche meno: $2/$10 per MTok
  contro $3/$15. Sul livello semplice invece non si è mosso nulla — **Haiku 4.5 è tuttora
  l'ultimo della sua fascia**, non esiste un Haiku più recente a cui salire.
  Da qui in avanti il prototipo resta congelato su Sonnet 4.6: sul ragionamento non è più
  un metro a parità di modello, ma un **termine di paragone** (cap. 04.7).
- **I nomi dei modelli non sono cablati nel codice**: i prompt dichiarano un *livello*
  ("semplice" o "ragionamento", cap. 04) e la mappa livello → modello vive in
  `modelli.json` nella cartella dati, gemello di `taratura.json` — predefiniti dentro il
  programma, file che li scavalca, ripiego dichiarato se il file è illeggibile. Cambiare
  modello — in avanti come all'indietro, ed è così che si è fatto il secondo tempo —
  costa **una riga**, non una nuova build.
- **L'interruttore del ragionamento esteso.** Haiku 4.5 lo tiene spento di suo e il
  prototipo non ne parla affatto: sul livello semplice la richiesta **non dichiara nulla**,
  e così resta identica a quella del prototipo. Su Sonnet 5 il valore predefinito è
  opposto, e lì l'interruttore è dichiarato — **spento** (`thinking: {"type": "disabled"}`,
  ed è l'unico campo per cui la richiesta del ragionamento diverge dalla sua) — perché
  **`max_tokens` limita ragionamento e risposta insieme**: i nostri limiti — 1500–4000
  fino al Pool 1.02, **4000–32000 dal Pool 1.03** (cap. 04.4) — sono cuciti addosso alla
  sola risposta, quindi passare a Sonnet 5 senza spegnerlo tronca le risposte **senza
  errore** dell'API — e un confronto troncato produce JSON invalido. I tetti più larghi
  lasciano margine anche a un ragionamento acceso, ma non cambiano la scelta: il margine
  è per il contenuto dell'utente, non per il pensiero del modello.
  Perciò in `modelli.json` l'interruttore ha **tre** stati e non due: *non dichiarato*
  (richiesta identica al prototipo), *spento*, *acceso*. Vale anche la seconda
  avvertenza del cap. 15: Sonnet 5 conta i token in modo diverso e a parità di testo ne
  usa circa il 30% in più — è un cambio di tokenizzatore, quindi pesa **anche
  sull'input**, non solo sul limite di risposta.
- **La chiave API**: `ClientClaude` la riceve già pronta, non va a cercarsela. Alla tappa
  T2 arriva dalla variabile d'ambiente `ANTHROPIC_API_KEY` — non tocca il disco e non
  entra nel repo; dalla 1.0 arriverà cifrata dalla cartella dati (cap. 11).
  *Arrivata a T6 (2026-08-14)*: chi la cerca è `ContestoApp`, che guarda in tre posti e in
  quest'ordine — quella dichiarata da chi avvia (la porta del banco), il file cifrato della
  cartella dati, la variabile d'ambiente di sempre. Il file viene prima perché è la volontà
  più recente dell'utente; perché la precedenza non diventi una sorpresa muta, la
  provenienza finisce nel resoconto d'avvio con la chiave **mascherata** (cap. 11.3).
- **Sincrono, salvo dove si guarda**: quasi tutte le chiamate sono **sincrone**. Le risposte
  stanno fra i 1500 e i 4000 token e si aspettano bene con un indicatore, quindi lo streaming
  a T2 non pagherebbe la sua complessità. Lo **streaming** (`stream: true`, eventi SSE) arriva
  con **T4/T7**, dove serve davvero — generazione e brainstorming, cioè i punti in cui il
  testo lungo compare man mano e l'attesa percepita crolla — e si aggiunge quando c'è un
  pannello che lo mostra. È l'unica vera novità di trasporto rispetto al prototipo.
  *Deciso il 2026-08-10, aprendo T4: lo streaming va **tutto a T7**, e T4 resta sincrona.*
  *Arrivato a **T7c** (2026-08-18), e su una sola strada: il **ragionamento** sulla
  candidatura (cap. 12, A6), che è l'unico posto dove qualcuno legge mentre l'AI scrive.*
  La grammatica del formato vive in una classe che non sa niente di Claude
  (`Ai/FlussoSse.vb`), così il pezzo più delicato si collauda con delle stringhe; il
  significato degli eventi lo dà il client. Con lo streaming nascono anche le **conversazioni
  vere** (`TurnoChat`): fino a T7b ogni chiamata mandava un solo messaggio `user`, e perfino i
  sette turni del dialogo guidato sono chiamate indipendenti. Tre regole nuove, tutte
  conseguenze del guardare mentre arriva:
  - **il ritentativo automatico vale solo prima del primo pezzo.** Dopo, riprovare vorrebbe
    dire scrivere due volte la risposta o cancellare sotto gli occhi di chi legge: l'errore
    arriva com'è, accanto a quello che era arrivato;
  - **l'attesa si misura sul silenzio**, non sulla durata. La proporzione col `max_token`
    (v. sotto) serviva perché finché non arriva tutto non è arrivato niente; se il testo
    compare, la chiamata sta funzionando per lunga che sia, e un tetto complessivo taglierebbe
    proprio le risposte lunghe legittime. Quel che resta da riconoscere è il collegamento
    morto, e un collegamento morto si vede dal silenzio fra un evento e l'altro;
  - **una risposta troncata, in chat, non è un errore.** Sincrona lo è — lascia un JSON monco
    da dare a un estrattore; qui il testo arrivato si legge, e il motivo della fine si porta a
    casa perché lo dica il pannello.
  La ragione sta in **cosa** scorrerebbe. La generazione di T4 non produce prosa: produce
  **JSON strutturato**, e un JSON che si srotola a video con le sue graffe e le sue
  virgolette non dice nulla a chi guarda — non è un CV che si scrive sotto gli occhi, è
  un tracciato che scorre. Il valore vero dello streaming è sul **brainstorming**, che è
  conversazione in prosa, e quello è T7. Metterlo a T4 significherebbe pagare gli eventi
  SSE e la gestione delle risposte parziali nel punto in cui rendono meno. A T4 l'attesa
  si copre invece con un avanzamento che dice **a che punto siamo** («genero la lettera —
  3 di 3»): l'informazione utile lì non è quali caratteri stanno arrivando, è quante
  chiamate mancano.
- **Robustezza**: timeout esplicito per chiamata — e **proporzionato al `max_token` del
  prompt** *(deciso col Pool 1.03, 2026-08-10)*: fino al limite di una risposta normale
  l'attesa è quella di sempre, oltre cresce insieme al tetto. Senza streaming il tempo di
  risposta cresce col testo che l'AI scrive, e un'attesa fissa trasformerebbe un limite
  generoso in un timeout: nessuna risposta invece di una troncata e dichiarata. La soglia
  è tarata sul limite dei turni del dialogo, cioè sull'unica chiamata che non si può
  annullare (v. 2.6): la loro attesa non è cambiata di un secondo. Poi un solo retry
  automatico su errore di rete o HTTP 429/5xx (con pausa, rispettando l'attesa che l'API
  suggerisce quando la suggerisce); nessun retry sugli errori nostri — una richiesta malformata o una chiave
  sbagliata, riprovata, dà lo stesso errore. Ogni errore arriva all'utente in italiano,
  con la possibilità di riprovare. Si guarda anche **perché il modello ha smesso di
  scrivere**: se si è fermato contro il limite di token la risposta è monca e lo si dice
  subito, invece di lasciarlo scoprire a valle sotto forma di JSON invalido senza sapere
  perché. Le risposte JSON passano da `EstrattoreJson`; se il JSON resta invalido si
  mostra il testo grezzo in un riquadro «cosa ha risposto il modello», mai un crash.
- **Quanto è costata ogni chiamata si annota** *(2026-08-18)*: ogni chiamata lascia una
  riga in `chiamate_ai.csv` nella cartella dati (cap. 11.1) — quale prompt, il tetto che
  quel prompt dichiara, i token andati e venuti, e la **percentuale del tetto** consumata.
  Serve a una cosa sola: ritarare i `max_token` del pool (cap. 04.4) sui numeri veri invece
  che a naso, e accorgersi che un tetto sta diventando stretto **prima** che una risposta si
  tronchi, non dopo. È nato dal cambio di modello — Sonnet 5 conta circa il 30% di token in
  più a parità di testo, e i tetti erano cuciti su Sonnet 4.6 — ma serve a ogni cambio che
  verrà. *Il primo verdetto è arrivato il **2026-08-19**, da un giro completo dentro
  l'applicazione (tredici chiamate, dall'import del CV all'email): il tetto più sollecitato
  è `email_candidatura` al **27,1%**, poi `umanizzazione_sintesi` al 25,0% e
  `umanizzazione_frasi` al 18,2%, e ogni riga si chiude con `end_turn` — **nessun tetto da
  alzare**, nessun troncamento. La misura ha quindi risparmiato la modifica che era nata per
  giustificare, che è il modo migliore in cui può finire.* Tre proprietà lo tengono innocuo: **non è un dato dell'utente** (nessun testo,
  nessun profilo, nessuna risposta: solo nomi di prompt e numeri), **cancellarlo non perde
  niente**, e **non deve mai far fallire una chiamata** — se il file non si lascia scrivere
  si perde la riga e si tira dritto, perché una candidatura persa per non aver potuto
  annotare quanto è costata sarebbe assurda. A scrivere non è il client ma il **mestiere**
  (cap. 02.3, `Ai/`): è l'unico posto in cui il prompt — col suo nome e col suo tetto — e la
  risposta — coi suoi token — si trovano nella stessa riga.
- **Nessuna memoria lato modello**: come nel prototipo, ogni chiamata è autonoma; la
  memoria (profilo, conversazione di brainstorming) vive nel programma, che a ogni
  turno manda il contesto necessario.

## 2.6 Concorrenza e reattività

Tutte le chiamate AI, di rete e su file girano **fuori dal thread dell'interfaccia**
(`Async/Await`): la finestra non si congela mai. Ogni operazione lunga ha indicatore di
attesa e pulsante Annulla (che annulla la richiesta, non lo stato già salvato).

**Un'eccezione dichiarata, decisa in T3c**: il turno del dialogo (P5) si aspetta e basta.
Annullare a metà lascerebbe la macchina a mosse in uno stato che non esiste — la risposta
consegnata ma la mossa mai ricevuta — e per un'attesa di un paio di secondi non vale il
prezzo. L'import di un CV, che dura molto di più, l'Annulla ce l'ha.

*E un'eccezione all'eccezione, decisa a **T7c** (2026-08-18)*: nello stesso pannello, il
**ragionamento** su una candidatura **si interrompe**. La ragione dell'eccezione di T3c era
la macchina a mosse, non il pannello: qui non c'è nessuna mossa da ricevere, e interrompere
lascia solo una risposta più corta — con il testo già arrivato che resta dov'è, dichiarato
interrotto. È anche l'unica attesa del programma che si guarda mentre passa, e un'attesa che
si guarda va potuta fermare. Il bottone non è nuovo: durante il turno «Invia» **diventa**
«Interrompi», perché è lo stesso posto dove la mano sta già.
Un'operazione per volta per opportunità: niente code parallele nascoste, il flusso
resta comprensibile.

*Fuori dalla finestra la regola si rovescia, ed è voluto (**T8b**, 2026-08-19): il ciclo
del server MCP serve **più richieste insieme**. Qui non c'è un utente che guarda un
pannello per volta ma un client che può chiedere due cose e annullarne una, e un ciclo che
aspettasse la fine di un `genera_cv` non sarebbe lento — sarebbe **sordo**, incapace di
leggere il messaggio con cui gli si dice di smettere. Il prezzo è un solo scrittore
sull'uscita, perché la riga è la cornice del messaggio (cap. 09.2).*

*Aggiornamento del 2026-08-09 (revisione adversariale)*: l'eccezione regge nel caso
normale ma ha un caso brutto — su rete degradata un turno può trattenere P5 fino a
~4 minuti (timeout 120 s × 2 tentativi) senza via d'uscita; l'annullabilità del turno è
annotata in `idee_future.md`. Intanto la regola «mentre l'AI lavora non si esce» è
diventata davvero globale: durante una chiamata si blocca anche la **barra di
navigazione**, dalla quale si poteva uscire dal pannello e lanciare un import
concorrente.

## 2.7 Dove vive lo stato

Tutto lo stato persistente sta nella **cartella dati** (cap. 11), in file JSON
leggibili. Fino a T8b l'app ne era l'unica scrittrice; *da **T8c** (2026-08-19) gli
scrittori sono **due** — la finestra e il server MCP — ed è per questo che nasce il
lucchetto del cap. 09.4: non per proteggere i byte, che si scrivono già in modo atomico,
ma perché nessuno dei due riscriva sopra quel che l'altro ha cambiato senza che se ne
accorga.* Niente database, niente registro di Windows
(salvo l'associazione minima di configurazione se servisse). Chiudere l'app e riaprirla
riporta esattamente dov'eravamo: lo stato in memoria è sempre ricostruibile dal disco.

*Provato per la prima volta a T5c (2026-08-13), e non su dati finti: l'applicazione chiusa
e riaperta sulla cartella dati vera ha ritrovato tutte le candidature con i loro stati.
La frase qui sopra dice però qualcosa di più forte di «i file restano», e T5c ne è la
dimostrazione: `registro.json` è **ricostruibile** dalle cartelle, quindi il disco non
contiene solo lo stato — contiene abbastanza da rifarlo. Un file d'appoggio che non si
potesse rigenerare sarebbe uno stato che vive in un posto solo, cioè il contrario di
questa regola.*
