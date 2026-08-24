# 11 — Dati, sicurezza e backup

*Dove vivono i dati dell'utente, come sono protetti i segreti, come si salva e si
ripristina tutto. File JSON leggibili, niente database: la trasparenza è parte del
prodotto.*

## 11.1 La cartella dati

Default: `%APPDATA%\TrovaLavoro`, scelta all'avvio con `--dati`. *(Il disegno la dava
anche «modificabile nelle Impostazioni»; a T9b, 2026-08-21, le Impostazioni la **mostrano
e la aprono, ma non la spostano**: il lucchetto è preso all'avvio e per tutta la sessione
— cap. 09.4 — e cambiarla a metà partita vorrebbe dire spostare i file sotto i piedi di
chi ci sta scrivendo. L'avvio è il momento in cui nessuno ci lavora, ed è lì che la
scelta resta.)*

```
TrovaLavoro\
├── impostazioni.json      le preferenze dell'utente: lingua predefinita dei documenti,
│                          interruttore della rifinitura e giorni del promemoria di
│                          follow-up (cap. 03, P8; cap. 07.3; v. 11.6)
├── ricerche.json          preferenze di ricerca, ricerche salvate e tabella dei portali
├── taratura.json          soglia, pesi e limiti del match (v. 11.6)
├── modelli.json           mappa livello → modello AI (v. 11.6, cap. 02.5)
├── documenti.json         la cartella documenti dell'utente e cosa c'è dentro (cap. 05.2)
├── segreti.bin            chiave API Anthropic, cifrata (v. 11.3)
├── profilo\
│   ├── profilo.json       il profilo corrente (fonte di verità unica)
│   ├── cv_base.json       il 📄 CV-1 base: è del profilo, non di un annuncio
│   ├── out\               i file del CV base (.docx .pdf)
│   └── storico\           una copia datata per ogni versione confermata
├── opportunita\
│   └── 2026-08-05_rossi-spa_tecnico-manutenzione\
│       ├── annuncio.json · giudizi.json · mitigazioni.json
│       ├── appunti.json   (esito confermato del brainstorming)
│       ├── cv.json · lettera.json
│       ├── email.json     (la bozza dell'email: destinatario, oggetto, corpo,
│       │                   allegati spuntati e la lingua in cui è nata — cap. 07.1)
│       ├── stato.json     (stato, date, lingua, versione profilo, fonte e link,
│       │                   i testi di **prima** della rifinitura — cap. 08.6 — e,
│       │                   da T9c, l'esito quando c'è, con la data dell'ultima
│       │                   notizia — cap. 07.3)
│       └── out\           (i file prodotti: .docx .pdf .eml)
├── registro.json          l'indice delle candidature: comodo, ma rigenerabile
├── webview2\              profilo di navigazione del browser integrato
├── chiamate_ai.csv        quanto è costata ogni chiamata all'AI (v. sotto)
├── dati.lock              vuoto: dice che qualcuno sta scrivendo qui (v. sotto)
├── log\app.log            log tecnico (senza segreti, v. 11.3)
└── backup\                gli export JSON (v. 11.4)
```

- **`config.json` non è mai nato, e `impostazioni.json` non è il suo erede** *(T9b,
  2026-08-21)*. Il disegno prevedeva un `config.json` che tenesse «modelli AI, cartelle»:
  le due cose hanno poi trovato case migliori — i modelli in `modelli.json`, che è del
  cap. 11.6 e si corregge come la taratura; la cartella documenti in `documenti.json`,
  insieme a quel che ci si è riconosciuto dentro; la cartella dati negli argomenti
  d'avvio, perché sceglierla dopo vorrebbe dire spostarla sotto i piedi di chi ci scrive.
  Quel che restava senza casa erano le **preferenze**, che nel disegno di allora non
  c'erano ancora: le tiene questo file, col nome in italiano come tutti i suoi vicini.
- **Un'opportunità = una cartella**: tutto ciò che riguarda una candidatura sta
  insieme, apribile anche a mano con Esplora file. Il nome della cartella è parlante
  (data + azienda + ruolo).
- **La cartella nasce a T4, la vista d'insieme a T5** *(deciso il 2026-08-10, aprendo
  T4)*. La coda delle opportunità, il `registro.json` e la macchina a stati sono di T5c,
  ma T4 produce già annuncio, giudizi, mitigazioni, CV e lettera: quella roba deve
  atterrare da qualche parte. La scelta è di **scriverla subito nella sua casa
  definitiva** — la cartella qui sopra, con i nomi qui sopra — e non in un ripiego
  temporaneo. A T4 la cartella nasce dunque completa di tutto tranne ciò che la tappa
  non ha ancora: nessun `registro.json`, e uno `stato.json` che porta le date, la lingua
  e la versione di profilo usata ma non ancora lo stato del ciclo di vita. Così T5
  aggiunge una vista sopra dati che ci sono già, invece di dover migrare quelli
  prodotti nel frattempo. È lo stesso principio dell'app che «riapre dov'era»
  (cap. 12.7): un documento generato e non ritrovabile domani sarebbe un documento
  perso, e T4 genera i primi documenti veri del progetto.
- **La vista è arrivata, e non ha toccato niente di quel che c'era** *(T5c,
  2026-08-13)*. `registro.json` esiste, ma è **solo un indice**: la verità sono le
  cartelle. Si ricostruisce quando manca, quando non si legge o quando non torna coi fatti
  su disco — così una cartella copiata a mano compare da sola nell'elenco, e una cancellata
  sparisce senza lasciare una voce fantasma. Lo `stato.json` di oggi porta in più il campo
  **`stato`** e il blocco **`date_stati`**, con la data di ogni passaggio; ma le cartelle
  nate prima **non sono state riscritte per uniformarle**: il loro stato si **deduce dai
  file che hanno** ogni volta che si legge (cap. 07.3). Il principio è quello del punto qui
  sopra guardato dall'altra parte: se non ritoccare i file dell'utente costa una deduzione
  in lettura, la deduzione è il prezzo giusto — un campo scritto a sua insaputa dentro
  tutte le cartelle già fatte sarebbe stato un'invasione, e per giunta irreversibile.
- **Da dove veniva l'annuncio si conserva** *(T5b, 2026-08-12)*: lo `stato.json` di
  un'opportunità catturata porta anche la **fonte** (il nome del portale se lo
  conosciamo, altrimenti il sito) e il **link** della pagina. È quello che permette di
  tornare all'originale mesi dopo — e quando l'annuncio non ci sarà più, resta almeno la
  prova di dove stava. Le opportunità scritte prima di T5b non hanno i due campi e si
  riaprono senza: non si inventa una provenienza che non c'era. Il link è anche
  l'identità con cui la cattura riconosce di aver già preso quella pagina (cap. 06.4).
- **Un documento riscritto a mano se lo ricorda** *(R7, 2026-08-23)*: lo `stato.json` di
  una candidatura annota **quali** campi di prosa ha riscritto l'utente e quando (blocco
  `riscritture`, uno per il CV e uno per la lettera), più la data dell'ultima lettera scritta
  dall'AI (`lettera_generata`); per il 📄 CV-1 base gli stessi id stanno in `cv_base.json`.
  Servono a tre cose che prima non si potevano fare (cap. 08.4): dire **quali** testi una
  rigenerazione butterebbe via, accorgersi che la lettera è rimasta indietro rispetto al CV, e
  far sapere al prompt della lettera quali parole vengono dalla persona invece che dal modello.
  Come sempre qui dentro, **i blocchi tacciono quando non hanno niente da dire**: una
  candidatura che nessuno ha toccato a mano resta scritta esattamente com'era, e una scritta
  prima di R7 si riapre come «mai toccata» invece di indovinare una storia che nessuno ha
  registrato. Backup e ripristino non hanno avuto bisogno di sapere niente: copiano i file
  grezzi (11.4), e i tool di lettura li restituiscono com'è (cap. 09.3).
- **E si ricorda anche quali voci ha lasciato fuori** *(R6, 2026-08-24)*: accanto alle
  riscritture, il blocco `voci_tolte` — le **impronte** delle voci che l'utente non vuole in
  *quel* documento, con la data in cui le ha tolte (cap. 08.4); nello `stato.json` per una
  candidatura, in `cv_base.json` per il 📄 CV-1. Il `cv.json` **resta intero**: si annota la
  scelta, non si taglia il documento — è per questo che rimettere una voce non costa niente e
  che un «Rigenera» non se la porta via. Come il suo gemello, il blocco **tace quando non ha
  niente da dire**: un CV che nessuno ha sfrondato si riapre esattamente com'era, e uno
  scritto prima di R6 vale «documento intero» invece di indovinare una scelta che nessuno ha
  fatto. Backup e ripristino, ancora una volta, non hanno avuto bisogno di sapere niente. Il
  tool `leggi_opportunita`, che invece il CV lo restituisce, adesso lo restituisce
  **filtrato**: su disco il documento è intero, ma chi legge da fuori deve vederlo **come lo
  vede l'utente**, o le due porte dell'applicazione racconterebbero due documenti diversi — e
  la seconda proprio quello che è stato scartato (cap. 09.3).
- **Il profilo è versionato**: ogni modifica confermata (editing, sessione di
  aggiornamento) salva una copia datata nello storico; `stato.json` di ogni
  opportunità annota **con quale versione** del profilo furono generati i documenti.
  Così un CV inviato resta sempre spiegabile, anche a profilo evoluto.
- **Il 📄 CV-1 base sta col profilo, non con le opportunità** *(deciso il 2026-08-10:
  il disegno non gli aveva mai dato una casa, e T4 è la tappa che lo genera)*. Nasce
  senza alcun annuncio, da P2, ed è il ritratto del profilo in forma di CV: metterlo in
  una cartella-opportunità vorrebbe dire legarlo a una candidatura che non ha. Vive
  quindi accanto al profilo da cui discende — che è poi la vista-dati «un profilo, molti
  CV» del cap. 02.2 presa alla lettera: il CV base è del profilo, i CV mirati sono delle
  opportunità. Come `stato.json`, anche `cv_base.json` annota **da quale versione di
  profilo** è nato: se il profilo poi cambia, il CV base non viene cancellato di
  nascosto né aggiornato di soppiatto — resta lì e l'app dice che è di una versione
  precedente, lasciando a chi legge la scelta se rigenerarlo. *(La promessa è **mantenuta
  da T7d**, 2026-08-18: fino a T7c il file veniva scritto e non riletto da nessuno, e
  ogni visita a P6 ne generava uno nuovo — così quella versione precedente non si vedeva
  mai, perché non arrivava mai a video.)* Dalla stessa tappa `cv_base.json` annota anche
  **in che lingua** è scritto: senza, riaprendolo lo si impaginerebbe con le etichette
  della lingua sbagliata, e indovinarla dal testo non è un mestiere dell'archivio. I file
  nati prima il campo non ce l'hanno e valgono **italiano**, con la stessa regola delle
  candidature di prima di T7a (cap. 10.1).
- **Un profilo corrotto si mette in salvo, mai in pericolo** *(2026-08-09, revisione
  adversariale)*: se `profilo.json` non si legge, l'app ne copia **subito** una fotografia
  (`profilo.rotto-<data>.json`) prima che qualunque «Salva» possa sovrascriverlo, e il
  messaggio dice dove sta la copia. La promessa «il file resta lì da recuperare» ora è
  mantenuta anche se l'utente preme un tasto qualsiasi.
- **La cartella dati si può dire all'avvio** *(2026-08-14)*: `TrovaLavoro.exe --dati
  "D:\prova"` fa vivere tutta l'applicazione in un'altra radice. Nasce da un bisogno di
  collaudo — provare una funzione che **cancella** senza mettere in gioco i dati veri
  (cap. 11.5) — ma è la stessa porta che servirà alle Impostazioni, dove la cartella è
  dichiarata «modificabile» fin dalla prima riga di questo capitolo. Tre cose la rendono
  onesta invece che pericolosa. **Si vede**: quando la radice non è quella predefinita
  l'applicazione lo dice sempre, nel **titolo della finestra** e nella barra di stato
  all'avvio — scambiare una cartella di prova per quella vera è precisamente l'errore che
  questa comodità renderebbe possibile. **Non impedisce di partire**: una radice
  illeggibile ripiega sulla predefinita dicendolo (cap. 03.8), come già faceva chiunque
  passasse una radice storta. **Non pretende di conoscere il futuro**: un argomento
  sconosciuto viene detto e ignorato, perché la riga di comando cresce con le tappe (T8
  aggiungerà `--mcp`, cap. 09) e un eseguibile che rifiuta di partire per una parola in
  più è peggio di uno che spiega.
- **Un file che punta fuori di qui** *(T6, 2026-08-14)*: `documenti.json` conserva il
  **percorso assoluto** della cartella documenti dell'utente (cap. 05.2), ed è l'unico
  della cartella dati a farlo. È una necessità, non una svista — quella cartella è sua e
  sta dove vuole lui, spesso su un altro disco — ma porta due conseguenze dichiarate: se
  la cartella sparisce l'applicazione **lo dice all'avvio** invece di proporre allegati che
  non si aprono, e un backup ripristinato su un altro PC troverà quel percorso senza
  senso. Dentro non ci sono copie di file: solo nomi (relativi alla cartella) con la loro
  categoria.
- **L'unico file che non è dell'utente** *(2026-08-18)*: `chiamate_ai.csv`. Ogni chiamata
  all'AI ci lascia una riga — quale prompt, il tetto di token che quel prompt dichiara,
  quanti token sono andati e venuti, e la **percentuale del tetto** consumata — e serve a
  **noi**, non al programma: è così che i `max_token` del pool si ritarano sui numeri veri
  invece che a naso (cap. 02.5, cap. 04.4). Dentro non c'è niente di suo: nessun testo,
  nessun campo del profilo, nessuna risposta. Da qui tre conseguenze volute:
  **cancellarlo non perde niente** (l'applicazione funziona identica senza), **non fa mai
  fallire una chiamata** — se il disco è pieno o il file è aperto altrove si perde la riga
  e si tira dritto — e nel backup **non entra**, come non ci entrano il log e i segreti. È
  anche l'unico file della cartella che non è JSON, e per un motivo: un CSV si apre in un
  foglio di calcolo e si **ordina per una colonna**, che è esattamente il gesto per cui
  esiste.
- **Il file che non contiene niente** *(2026-08-19)*: `dati.lock`, il lucchetto di
  scrittura (cap. 09.4). È vuoto per scelta, e non per pigrizia: quel che dichiara non è
  scritto dentro, è il fatto stesso che un processo lo **tenga aperto in esclusiva**. Così
  chi scrive lo dichiara al sistema operativo, e quando quel processo muore — chiuso, in
  crash o ammazzato — è Windows a rilasciarlo. Un lucchetto con dentro un numero di
  processo e un'ora andrebbe invece *ripulito* da qualcuno, e prima o poi lascerebbe
  l'utente chiuso fuori dai propri dati per un file rimasto lì dopo un crash di tre
  settimane prima. Resta sul disco anche a lucchetto libero, di zero byte:
  cancellarlo aprirebbe una gara con chi lo sta riaprendo in quell'istante. Come
  `chiamate_ai.csv`, **nel backup non entra**: non è un dato, è lo stato di un momento.
- Formati **JSON con rientri**, leggibili in qualsiasi editor: l'utente è padrone dei
  suoi dati anche senza l'app. *Con due eccezioni, e si vedono dal nome*: `segreti.bin` è
  l'unico file **non leggibile**, ed è il punto — gli altri sono dati dell'utente, quello
  è una credenziale (v. 11.3) — e `dati.lock`, che non ha niente da leggere.

## 11.2 Cosa esce dal PC (e cosa no)

| Dato | Esce? | Verso dove |
|---|---|---|
| Testi per l'elaborazione (profilo, annuncio, PDF da trascrivere) | sì | solo API Anthropic, via HTTPS |
| Pagine dei portali visitate nel browser incorporato | sì, come in un browser qualunque | i portali stessi; le credenziali le digita l'utente e l'app non le vede (cap. 06.6) |
| Tutto il resto (registro, documenti, email, configurazione, log) | **no** | — |

L'email di candidatura **non esce dal programma**: viene scritta come file `.eml` nella
cartella dell'opportunità, e a spedirla è il programma di posta dell'utente
(cap. 15, voce 9).

Niente telemetria, niente servizi del produttore, niente aggiornamenti automatici
silenziosi.

## 11.3 I segreti

- La **chiave API Anthropic** è cifrata con la protezione dati di Windows (DPAPI)
  **legata all'utente che l'ha salvata**: il file `segreti.bin` copiato su un altro PC o
  letto da un altro account non si decifra. È il compromesso giusto per un'app
  personale: robusto, senza inventare crittografia in proprio. *(È rimasta l'unica
  credenziale del programma: senza invio SMTP non c'è più alcuna password di posta da
  custodire — cap. 15, voce 9.)*
- Nell'interfaccia la chiave è sempre mascherata (`sk-ant-…ultime 4 cifre`).
- Il **log** non contiene mai segreti né testi integrali dei documenti: registra
  eventi, esiti e codici di errore (una funzione di redazione maschera qualunque campo
  il cui nome contenga «key» o «password» prima della scrittura).
- I backup JSON (11.4) **non contengono i segreti**, di proposito: dopo un ripristino
  la chiave API va reinserita. Un backup che gira via email o chiavetta non deve poter
  bruciare la chiave.

### Com'è stato costruito (T6, 2026-08-14)

- **Nessun pacchetto in più.** La protezione dati di Windows sta già dentro .NET per le
  applicazioni desktop: cifrare la chiave non ha aggiunto una sola dipendenza al vincolo
  «un solo exe» (cap. 01.6). Al blob si aggiunge un'entropia fissa dell'applicazione, che
  **non è un segreto** — sta nell'eseguibile come starebbe in qualunque programma — e non
  serve a irrobustire: fa fallire subito la decifratura di un file che non è nostro.
- **Dove si digita, finché le Impostazioni non ci sono.** Una finestra al **primo avvio**,
  prima che i pannelli si colleghino al motore (cap. 03.4). «Alla prima chiamata all'AI»
  sarebbe stato più elegante e non poteva funzionare: senza chiave i pannelli spengono i
  bottoni che la userebbero, quindi quella prima chiamata non sarebbe mai arrivata.
  Per **sostituirla** si riavvia con `--chiave`, che fa ricomparire la finestra anche
  quando una chiave c'è già; l'argomento **non prende un valore**, perché una chiave
  scritta sulla riga di comando resterebbe nella cronologia della shell e nell'elenco dei
  processi.
- **Tre posti dove cercarla, in ordine dichiarato**: quella indicata da chi avvia (la
  porta del banco), il file cifrato, la variabile d'ambiente `ANTHROPIC_API_KEY` che
  reggeva da T2. Il file viene prima perché è la volontà più recente dell'utente; perché
  la precedenza non diventi una sorpresa muta — «ho cambiato la variabile e non succede
  niente» — la **provenienza** finisce nel resoconto d'avvio, con la chiave mascherata.
- **Il file che c'è ma non si apre non è un «non ce l'ho».** È il caso di `segreti.bin`
  copiato su un altro PC o salvato da un altro account di Windows: DPAPI lo rifiuta, ed è
  quel che promette. L'utente però quel file lo vede su disco e lo crede buono, quindi
  quel caso torna **a parte** e diventa un avviso che spiega perché va reinserita.
- **Senza chiave si entra lo stesso.** «Non adesso» è una risposta legittima: profilo,
  candidature e documenti si leggono, e restano spente le sole funzioni che chiamano l'AI
  (cap. 03.8). Un programma che non parte finché non gli si dà una credenziale sarebbe un
  ricatto, oltre che una bugia sul suo funzionamento.
- **La chiave non si prova.** Verificarla costerebbe una chiamata proprio mentre l'utente
  sta entrando, e comunque non distinguerebbe una chiave sbagliata da una rete che non
  c'è. Della **forma** si dice quel che si vede — le chiavi di Anthropic cominciano per
  `sk-ant-` — ma si avverte senza impedire: chi ne ha una fatta in un altro modo la sa
  usare meglio di noi.

## 11.4 Backup e ripristino (F7)

**Esporta** (dal pannello Profilo o dalle Impostazioni):

- contenuto a scelta: **solo profilo** (con storico) oppure **profilo + registro +
  opportunità** (gli artefatti JSON; i file in `out\` restano fuori dal JSON e si
  copiano a parte, sono già file normali);
- un solo file `.json` con intestazione di versione:

```json
{
  "formato_backup": 1,
  "app": "TrovaLavoro",
  "data": "2026-08-05T18:30:00",
  "contenuto": ["profilo", "storico", "registro", "opportunita"],
  "profilo": { … },
  "storico": [ … ],
  "registro": { … },
  "opportunita": [ … ]
}
```

**Importa**:

1. l'app legge il file, ne verifica formato e versione;
2. mostra **cosa contiene** e **cosa sovrascriverebbe** («il backup ha un profilo del
   3 agosto; quello attuale è del 5 agosto: vuoi davvero sostituirlo?»);
3. solo alla conferma scrive, e prima di scrivere salva l'attuale nello storico
   (il ripristino non deve mai poter distruggere l'unico profilo buono).

Il campo `formato_backup` permette ai programmi futuri di leggere i backup vecchi:
il numero cresce solo quando lo schema cambia davvero.

### Com'è stato costruito (T9a, 2026-08-21)

- **Nel backup entrano i file grezzi, non gli oggetti.** Il profilo, il CV base e gli
  artefatti di ogni candidatura si copiano **così come stanno sul disco**, non
  ricostruendoli dalle classi del programma. La differenza si vede solo nel caso che conta:
  un campo che questa versione non modella — scritto a mano, o da un programma futuro —
  passando per gli oggetti sparirebbe nel viaggio, e un backup che perde qualcosa non è un
  backup. L'unica eccezione è il **registro**, che è un indice rigenerabile (cap. 07.3) e
  infatti al ritorno **si ricostruisce dalle cartelle** invece di essere ricopiato: dopo un
  ripristino sul disco c'è un insieme che nessun indice salvato conosce — le candidature
  tornate più quelle che c'erano già.
- **Il ripristino rimette a posto, non riporta il disco a quel giorno.** Le candidature che
  il backup non nomina **restano dove sono**, e l'anteprima lo dice prima della conferma.
  Un ripristino che cancellasse il non-nominato sarebbe una perdita silenziosa che nessuno
  ha chiesto: cancellare è un altro gesto, sta nelle Impostazioni e costa una parola
  scritta a mano (cap. 11.5).
- **Prima si mette in salvo, poi si scrive.** Il profilo di adesso finisce nello storico
  *prima* che quello del backup prenda il suo posto (passo 3 qui sopra), e la versione con
  cui è stato archiviato si dichiara all'utente: da lì si riprende. Vale anche quando il
  profilo corrente non si lascia leggere — allora è la copia `profilo.rotto-…` a fare da
  rete (cap. 11.1).
- **I nomi che arrivano da fuori sono nomi di file, non percorsi.** Un file di backup si
  può scrivere a mano, e uno costruito male — `..\..\fuori` al posto del nome di una
  cartella — non deve poter far scrivere l'applicazione dove vuole chi l'ha costruito. Chi
  non passa il controllo viene **rifiutato e detto in chiaro**, non ignorato in silenzio.
  Che il controllo non fosse decorativo l'ha dimostrato la falsificazione (regola 14):
  reso permissivo, il collaudo ha scritto davvero un file fuori dalla cartella dati.
- **Una finestra sola per le due metà.** Esportare e ripristinare stanno insieme perché
  sono lo stesso gesto nei due versi, e chi cerca «come si ripristina» lo cerca dove ha
  esportato. Erano due bottoni possibili nella fascia di P2, ma il ripristino deve far
  **leggere cosa sovrascrive** prima di toccare qualcosa, e quello un bottone non lo sa
  fare: finché non si sceglie un file, «Ripristina» resta spento.
- **Livello 5, non 6** (cap. 03.3). Il ripristino sovrascrive dati esistenti, ma il profilo
  di prima finisce nello storico e le candidature non nominate restano: non è una
  cancellazione definitiva. Chiedere di ridigitare `TrovaLavoro` qui sarebbe un allarme che
  grida più forte di quanto il gesto meriti — e allarmi così, ripetuti dove non servono,
  insegnano a scacciarli anche dove servono. La conferma però parte da **«no»**.
- **Il CV base viaggia col profilo**, non con le candidature: è il suo ritratto in forma di
  CV (cap. 11.1), e infatti è anche quello che se ne va quando il profilo si elimina
  (cap. 11.5). I documenti impaginati delle cartelle `out\` restano invece **fuori**: sono
  file normali, si copiano da sé e si rigenerano con un bottone.
- **La stessa funzione si affaccia anche dal server MCP**, con il tool `esporta_backup` che
  aspettava questa tappa per nascere (cap. 09.3). Da lì si esporta e basta: il ritorno
  indietro resta dove c'è l'utente a guardarlo.

## 11.5 Pulizia e diritto all'oblio

Dalle Impostazioni: «Svuota dati di navigazione» (cartella `webview2\`), «Elimina
un'opportunità» (la sua cartella, con conferma di livello 5), «Elimina tutti i dati»
(l'intera cartella dati, conferma di livello 6 con nome dell'app da ridigitare).
L'app non lascia nulla in giro fuori dalla cartella dati, quindi la disinstallazione
è: cancellare l'exe e, se si vuole, la cartella dati.

**«ELIMINA PROFILO - DEFINITIVO», dal pannello Profilo** *(2026-08-14)*. Le eliminazioni
qui sopra stanno nelle Impostazioni e arrivano con T9; questa è la loro sorella mirata,
e sta in P2 accanto a «Salva profilo» perché è lì che il profilo si guarda e si corregge.
Il perimetro è **la cartella `profilo\` intera**: `profilo.json`, lo `storico\`, il
`cv_base.json` con la sua `out\`, le copie `profilo.rotto-*` e ogni temporaneo rimasto.
Non un file alla volta ma la cartella, perché «definitivo» non diventi una mezza verità
la prima volta che qualcuno mette un file nuovo lì dentro.

Cosa **non** tocca — ed è la parte da dire all'utente prima che confermi: le candidature
(`opportunita\`), il `registro.json` che le indicizza, le ricerche salvate, la taratura,
i modelli, i dati di navigazione. Chi elimina il profilo si toglie di dosso il proprio
racconto, non il lavoro di ricerca già fatto: sono due decisioni diverse e restano due
gesti diversi.

Due conseguenze accettate a occhi aperti:

- lo `stato.json` di ogni candidatura annota la **versione di profilo** con cui i suoi
  documenti furono scritti (v. 11.1); sparito lo storico, quei riferimenti restano
  scritti ma non puntano più a niente. La candidatura si riapre lo stesso — i suoi
  documenti sono nella sua cartella — ma «con quale profilo fu scritto questo CV» non ha
  più risposta. È il prezzo di un'eliminazione che si dichiara definitiva, e chi la
  sceglie vuole proprio quello;
- il 📄 CV-1 base se ne va con il profilo, e non perché sta nella sua cartella: contiene
  nome, contatti ed esperienze. Lasciarlo lì vorrebbe dire non aver cancellato niente.

**La conferma è di livello 6** (cap. 03.3), la prima del progetto: una finestra che
elenca cosa sparisce e cosa resta, e per procedere chiede di **ridigitare `TrovaLavoro`**
— lo stesso gesto che il paragrafo qui sopra riserva a «Elimina tutti i dati». Il bottone
che elimina resta spento finché la parola non è esatta; il tasto Invio non lo preme, e
Esc chiude senza fare niente. Non c'è cestino e non c'è «annulla»: è la ragione per cui
il gesto costa una parola scritta a mano.

### Com'è stato costruito (T9b, 2026-08-21)

Le due pulizie generali sono arrivate col pannello che le ospita, e stanno in
`Dati/PuliziaDati` e non dentro la finestra: **una cancellazione va collaudata**, e un
banco non sa premere un bottone. «Svuota i dati di navigazione» è di **livello 5** e si
conferma come lo «Scarta» di P4 — una domanda che parte da «No» e dice cosa sparisce
davvero (le sessioni sui portali, non le candidature). «ELIMINA TUTTI I DATI» è di
**livello 6** e passa dalla `FinestraConfermaCritica` che già serviva «ELIMINA PROFILO»:
la stessa parola da riscrivere a mano, e non una finestra nuova che le somigliasse.

**«Tutto» vuol dire tutto tranne il lucchetto**, e va detto: `dati.lock` è tenuto aperto
in esclusiva per tutta la sessione (cap. 09.4), quindi con il programma vivo non si
lascerebbe cancellare comunque, e provarci solleverebbe un errore proprio nel gesto più
radicale. Non è un dato dell'utente, e alla riapertura una cartella con dentro solo quello
è indistinguibile da un primo avvio. Dopo l'eliminazione **l'applicazione si chiude**: da
lì in poi ogni pannello lavorerebbe su file che non ci sono più.

*Un difetto trovato provandolo dal vivo, che nessun collaudo vedeva*: il bottone si
accende se «c'è qualcosa da eliminare», e contando le **voci** della cartella si sarebbe
riacceso subito dopo un'eliminazione totale — perché `CartellaDati.Assicura` ricrea
`profilo\`, `storico\`, `out\` e `opportunita\` vuote appena qualcuno tocca la cartella
dati. Un bottone rosso che promette di eliminare quattro cartelle vuote insegna solo a non
fidarsi del colore: adesso la domanda guarda i **file**, in tutto l'albero.

## 11.6 I due file dei numeri: taratura e modelli

`taratura.json` contiene i numeri del calcolo delle stelle: soglia 1,5, pesi 5 e 1,
correzione della mitigazione limitata fra −20 e +10, tetto a 20 punti, e la regola del
requisito eliminatorio (⛔ → massimo una stella). Sono valori **di prodotto, non
preferenze**: l'interfaccia non li mostra e non li lascia toccare (cap. 15, voce 10).

Il motivo di tenerli fuori dall'interfaccia è che le stelle devono restare confrontabili
fra un annuncio e l'altro: se l'utente potesse spostare la soglia, il punteggio
smetterebbe di misurare quanto è adatto a quel posto e comincerebbe a misurare quanto è
ottimista quel giorno. Il motivo di tenerli fuori dal **codice** è opposto e pratico:
durante la messa a punto ritoccare un valore deve costare una riga, non una nuova
versione da ricompilare e reinstallare su due macchine.

Se il file manca o è illeggibile, il programma usa i valori predefiniti che porta
dentro di sé e lo annota nel log: una taratura corrotta non deve impedire l'avvio.
*Precisato il 2026-08-09 (revisione adversariale), quando un valore non numerico
dentro una mappa dei pesi faceva cadere l'app all'avvio*: una mappa storta **si scarta
intera** e valgono i predefiniti — tenere solo le voci buone avrebbe falsato il
punteggio in silenzio, che è peggio di un ripiego dichiarato.

*E dal 2026-08-10 (T4) c'è anche la **validazione di range**, che fino a lì era un'idea
futura*: ogni numero letto dichiara l'intervallo in cui ha senso — il clamp verso il
basso fra −100 e 0, quello verso l'alto fra 0 e 100 — e un valore fuori scala, per quanto
ben formato, **viene scartato e annotato** invece di entrare zitto. Era il momento
giusto: T4 è la prima tappa che usa `CalcoloMatch` sul serio dentro l'applicazione, e da
qui in avanti un `"clamp_su": -50` non sposterebbe un numero in un collaudo, sposterebbe
le stelle che l'utente legge prima di decidere se candidarsi.

**`modelli.json` è il suo gemello** (cap. 02.5): tiene la mappa livello → modello, e
per ciascun livello anche l'interruttore del ragionamento esteso, che ha **tre** stati
— non dichiarato, spento, acceso. Vale la stessa regola dei predefiniti: un file
assente, parziale o illeggibile non impedisce l'avvio, si ricade sui valori interni e
lo si annota. Anche qui il motivo è pratico: cambiare modello — o fare il secondo
esperimento su Sonnet 5 — deve costare una riga, non una nuova build da reinstallare
su due macchine. *(Realizzato a T2 in `Ai/Modelli.vb`.)*

**E c'è un terzo file che di numeri non ne ha: `impostazioni.json`** *(T9b,
2026-08-21)*. Vale la pena dire perché non sta qui dentro, visto che si legge allo stesso
modo — ripiego sui predefiniti, avviso quando ci si cade, e nessun avvio impedito. Perché
è l'**opposto**: la taratura è di prodotto e l'interfaccia non la mostra, per non lasciare
che il punteggio misuri l'ottimismo di quel giorno invece dell'attinenza al posto; le
preferenze sono scelte che *solo* l'utente può fare — in che lingua scrive di solito, se
vuole che una macchina gli ritocchi la prosa, e dopo quanti giorni di silenzio vuole che gli
si ricordi una candidatura spedita *(quest'ultima da **T9c**, 2026-08-21: `giorni_follow_up`,
quattordici di casa e zero per spegnere il promemoria — cap. 07.3)*. Si toccano con mani
diverse, e quindi stanno in file diversi. Con una differenza anche in lettura: una mappa di
taratura storta si scarta **intera**, perché le sue voci si compongono in un punteggio solo e
tenerne metà lo falserebbe in silenzio; le tre preferenze invece non si parlano, e una lingua
scritta male non dice niente sulla rifinitura — si scarta quella e si tengono le altre.

A leggerli all'avvio è `Motore/ContestoApp` *(da T3c, 2026-08-07)*: entrambi i file, con
l'avviso di ripiego quando si cade sui predefiniti. Prima esistevano i lettori ma non
li chiamava nessuno, e un file di taratura messo nella cartella dati non avrebbe avuto
alcun effetto — senza che niente lo dicesse.
