# Diario di bordo – AI-CV-COACH

Questo file raccoglie appunti a caldo durante lo sviluppo del progetto.
Serve a tenere traccia di cosa è stato fatto, cosa ho imparato, quali
difficoltà ho incontrato e come le ho risolte. È il materiale grezzo da
cui verrà costruita la relazione finale.

Lingua: italiano. Stile: sincero e sintetico.

---

## Fase 0 – Setup e analisi preliminare

### Step 0.1 – Preparazione strumenti

Cosa ho fatto:
- Verificato e installato gli strumenti di base: Python 3.14.5, Git 2.54.0, VS Code 1.121.0.
- Configurato Git durante l'installazione (branch di default "main", editor, PATH, gestione credenziali).

Cosa ho imparato:
- Cos'è un PATH e perché serve per usare i comandi da terminale.
- La differenza tra avere un programma installato e averlo "riconosciuto" dal sistema.

Difficoltà:
- All'inizio i comandi git e code non erano riconosciuti dal terminale; risolto installando i programmi e aggiungendoli al PATH.

### Step 0.2 – GitHub e configurazione Git

Cosa ho fatto:
- Creato l'account GitHub (username mirco-parenti).
- Configurato nome ed email globali di Git.
- Creato il repository pubblico AI-CV-COACH.
- Iniziato a usare GitHub Desktop come interfaccia grafica.

Cosa ho imparato:
- Differenza tra Git (locale) e GitHub (online e pubblico).
- Cos'è un repository: una cartella di progetto con cronologia versionata.

### Step 0.3 – Struttura iniziale e primo commit

Cosa ho fatto:
- Aperto il repository in VS Code.
- Creato i file di documentazione: README.md, research_notes.md, prompt_design.md.
- Eseguito il primo commit ("Add initial project documentation") e fatto push su GitHub.
- Verificato online che i file fossero presenti e il README formattato correttamente.

Cosa ho imparato:
- Differenza tra "salvare un file" e fare un "commit" (salvataggio ragionato nella cronologia).
- Cosa fa il "push" (carica i commit locali su GitHub).
- L'importanza di verificare sempre il risultato invece di darlo per scontato.

Difficoltà:
- Avviso sui line endings (LF/CRLF) in GitHub Desktop: capito che è normale su Windows e non è un problema.

---

Note generali

Da aggiornare alla fine di ogni fase.

---

### Step 0.5 – Analisi di progetti simili

**Progetti analizzati (dettaglio completo in research_notes.md)**
1. Resume-Matcher (srbhr) — https://github.com/srbhr/Resume-Matcher
   Lente: vincolo anti-invenzione (master resume, fonte chiusa).
2. resume-job-matcher (sliday) — https://github.com/sliday/resume-job-matcher
   Lente: scoring (media pesata, voto dato dall'LLM).
3. Resume-Parser (Sajjad-Amjad) — https://github.com/Sajjad-Amjad/Resume-Parser
   Lente: output strutturato (JSON) ed estrazione dati.
4. ResumeLM (olyaiy) — https://github.com/olyaiy/resume-lm
   Lente: gestione dati utente e pattern "AI propone, utente conferma".

**Cosa ho fatto**
- Imparato a cercare progetti su GitHub e a valutarli con criteri rapidi
  (stelle, data ultimo aggiornamento, descrizione).
- Analizzato 4 progetti open source simili ad AI-CV-COACH, usando una
  griglia di analisi e usando ogni progetto come "lente" per un tema:
  1. Resume-Matcher -> vincolo anti-invenzione.
  2. resume-job-matcher -> come funziona lo scoring.
  3. Resume-Parser -> output strutturato (JSON).
  4. ResumeLM -> gestione dati utente e dialogo.
- Documentato ogni progetto in research_notes.md con commit separati.

**Cosa ho imparato**
- La pipeline del progetto ha 4 anelli: struttura i dati (estrazione) ->
  confronta profilo e annuncio -> assegna punteggio -> genera output.
- Esistono due famiglie di scoring: una affidata all'LLM (coglie le
  sfumature ma e meno trasparente e potenzialmente incoerente) e una
  matematica/NLP (trasparente e verificabile ma rigida).
- Nella famiglia LLM il punteggio puo essere una media pesata di piu
  componenti (es. match 75% + qualita 25%), normalizzata 0-100.
- L'output strutturato (JSON) serve a due scopi: non far inventare l'AI
  (compito chiuso) e rendere i dati confrontabili da un programma.
- Il pattern "human-in-the-loop": l'AI propone, l'utente conferma.
- Distinzione "AI-enabled" vs "AI-first": l'AI e uno strumento di supporto
  discreto al servizio dell'utente, non il padrone del processo.

**💡 Mie intuizioni / scelte ragionate**
- Ho capito che i principi anti-invenzione (fonte chiusa, compiti ristretti,
  output strutturato, conferma dell'utente) non sono separati: sono facce
  della stessa idea, cioe togliere liberta all'AI per ancorarla alla realta.
- Ho collegato i 4 progetti in un'unica catena: l'output strutturato non e
  solo difesa anti-invenzione, e anche la precondizione che rende possibile
  il confronto e quindi lo scoring. Senza dati strutturati non c'e match.
- Ho scelto per lo scoring la "famiglia A" (LLM che da il voto), perche
  coglie le sfumature ed e piu semplice da implementare al nostro livello,
  accettando che il punteggio sia presentato come orientativo.
- Ho ragionato sui limiti della famiglia A: i punteggi possono essere
  incoerenti (stesso input, voti un po' diversi) e un "voto aperto" lascia
  spazio all'invenzione. Mitigazione: chiedere all'AI di giustificare il
  punteggio elencando i requisiti soddisfatti e non soddisfatti, per
  ancorarlo a fatti verificabili.
- Ho progettato il meccanismo di conferma nel dialogo: dopo ogni risposta,
  l'AI mostra l'output parziale strutturato; l'utente conferma, corregge il
  singolo campo, o ripete. Solo dopo la conferma il dato entra nel profilo.
- Ho affinato quel meccanismo: meglio permettere la correzione del singolo
  campo invece di ripetere tutta la domanda; e l'AI deve strutturare solo
  cio che l'utente ha detto, senza arricchire o gonfiare le risposte.
- Ho individuato come priorita della Fase 1 il dialogo guidato, per due
  motivi: e il primo anello della pipeline (gli altri dipendono da lui, e
  il profilo e la fonte chiusa da cui tutto pesca) ed e l'elemento
  distintivo e raro del progetto, quindi e un requisito fondante.

**Dove ho faticato / cosa non era ovvio**
- All'inizio non era ovvio che "tirare le somme" e "scrivere il diario"
  fossero due attivita diverse: prima si ragiona, poi si mette per iscritto.
- Capire la differenza tra le due famiglie di scoring ha richiesto un
  ragionamento sui rispettivi pregi e limiti.
- Ho dovuto distinguere il ruolo del diario (appunti grezzi di pensiero) da
  quello di research_notes.md (archivio dettagliato dei dati sui progetti).

**Cosa ho deciso e perche**
- Scelto Resume-Matcher come primo progetto da analizzare, applicando criteri
  di qualita: molte stelle (maturita) e aggiornamento recente (progetto vivo).
- Approfondito lo scoring come secondo tema, perche e un deliverable del
  progetto e uno dei punti piu delicati da progettare.
- Aggiunto un terzo progetto sull'output strutturato e un quarto sul dialogo,
  poi chiusa la rosa a 4 (range richiesto 3-5): meglio pochi progetti
  analizzati a fondo che molti in modo superficiale.
- Stabilito un metodo di lavoro: prima si ragiona ("tirare le somme"), poi si
  mette per iscritto nel diario; il diario si aggiorna a fine step/fase, non
  a ogni micro-passo.
- Deciso di evidenziare nel diario le intuizioni e scelte ragionate con
  un'etichetta dedicata, perche sono la parte piu utile per la relazione.

**Nota su come procederemo (da ricordare per la Fase 1)**
Il dialogo guidato in Fase 1 sara una versione minima (MVP): poche domande,
anche senza tutto il meccanismo di conferma raffinato, giusto per ottenere un
profilo grezzo con cui far girare il resto della pipeline. Si raffina dopo.

**Nota di posizionamento (per la relazione finale)**
Quasi tutti i progetti analizzati partono da un CV gia esistente. Costruire
il profilo tramite dialogo guidato a domande successive e raro: e quindi un
elemento distintivo e originale di AI-CV-COACH.

---

### Riflessione di chiusura

**Cos'e stata la Fase 0**
Una fase di osservazione e preparazione, senza ancora costruire nulla di
visibile. Lo scopo era studiare e capire le funzioni gia esistenti in
progetti online simili al mio e capire come, combinandole, possano dare
forma alla mia web app. In parallelo ho configurato gli strumenti di lavoro
e ricevuto un'infarinatura su molti aspetti tecnici che affrontero piu
concretamente in seguito. Ho anche imparato a strutturare cronologicamente
il lavoro, per arrivare per gradi a una versione funzionante dell'app con
le caratteristiche desiderate.

**Con cosa arrivo alla Fase 1**
- Una pipeline ragionata da seguire (struttura -> confronta -> valuta ->
  genera), non piu solo un'idea generica di progetto.
- Idee concrete su come affrontare i problemi che la mia app deve risolvere,
  in particolare il vincolo anti-invenzione.
- Un repository ordinato con documentazione, analisi e diario, e l'abitudine
  al ciclo commit/push verificato.
- La chiarezza che la priorita della Fase 1 e il dialogo guidato (in versione
  minima/MVP), primo anello della pipeline ed elemento distintivo del progetto.

**Il metodo di lavoro**
Mi sono trovato molto bene con l'approccio step-by-step, i commit frequenti,
il diario di bordo e l'abitudine a ragionare prima di mettere per iscritto.
Decido di mantenere lo stesso metodo nella Fase 1.

**Stato: Fase 0 completata.**

---

## Fase 1 – Prototipo base v0

### Appunti di apertura della Fase 1

**Decisione di metodo: ruoli e divisione del lavoro**
A partire dalla Fase 1 il lavoro si sviluppa con tre figure distinte:
- Io (Mirco) sono il regista: decido cosa fare, in che ordine, e do l'ok
  a ogni passo. Resto il responsabile della comprensione e delle scelte.
- Claude (chat) e il tutor: progettazione, concetti, ragionamento sulle
  scelte, prompt design, documentazione, somme di fine step/fase.
- Claude Code e l'assistente operativo: scrittura concreta di codice ed
  esecuzione di comandi, dopo che il pezzo e stato pensato a parole.

Regola d'oro: prima ragioniamo in chat, poi se serve costruiamo con
Claude Code. Mai il contrario. Da test fatto in autonomia
(claude-code-test-1) ho gia adottato l'accordo "ogni decisione di Claude
Code mi viene sottoposta prima dell'esecuzione: accetto, rifiuto, modifico,
studio". Mantengo questo accordo per tutta la Fase 1.

**💡 Mia idea progettuale: lista delle "domande in sospeso"**
Durante il dialogo guidato, se l'AI non riesce a strutturare bene una
risposta dell'utente, oppure se l'utente non sa o evita la domanda, la
domanda viene messa da parte in una lista interna delle "domande in
sospeso" invece di insistere sul momento. Il dialogo prosegue.
A fine dialogo si analizzano le domande accumulate e si pone un secondo
giro mirato, con domande riformulate in base ai concetti delle questioni
rimaste aperte, per provare a colmare i vuoti.

Perche e utile:
- E un'altra applicazione del principio "AI propone, utente conferma":
  l'AI dichiara apertamente quando non ha capito invece di riempire il
  profilo con dati approssimati. Difesa anti-invenzione.
- Rende il dialogo meno opprimente: ci si sposta dalla domanda difficile
  e si torna dopo, quando l'utente ha "scaldato i muscoli" parlando di se.
- Aggiunge una funzione che nessuno dei 4 progetti analizzati gestiva
  esplicitamente: e un altro elemento distintivo di AI-CV-COACH.

Sfumature da rispettare:
- La lista deve restare piccola: troppe domande in sospeso = segnale che
  il dialogo e troppo lungo o invasivo, da ripensare.
- Il secondo giro si fa una volta sola, non in loop infinito.
- Distinguere due casi: (A) l'AI non ha capito -> ha senso riformulare;
  (B) l'utente ha saltato o non sa -> va rispettato.

Nota tecnica: non e una "cartella" del filesystem, e una lista interna
del programma (concetto di "pending_questions" o "coda di fallback").
Il design preciso si vedra in prompt_design.md quando ci arriveremo.

**Punto di partenza della Fase 1**
Dialogo guidato in versione MVP (minimo funzionante): poche domande, anche
senza tutto il meccanismo di conferma e senza ancora la lista delle domande
in sospeso. L'obiettivo del primo giro e ottenere un profilo grezzo con cui
far girare il resto della pipeline. Il raffinamento viene dopo.

---

### Step 1.1 – Struttura del profilo utente

**Cosa ho fatto**
Definita la struttura del profilo utente per l'MVP: il "modulo" che il
dialogo guidato dovra riempire. La struttura e volutamente minimale per
restare in scala MVP.

Campi (nomi tecnici JSON):
- nome
- esperienze_formali (lista; ogni voce: ruolo, azienda, durata,
  cosa_facevo)
- esperienze_informali (lista; ogni voce: cosa_facevo, quando, con_chi,
  tutti opzionali)
- competenze (lista di stringhe)
- formazione (lista; ogni voce: titolo, istituto, anno)

**Cosa ho imparato**
- Distinzione fondamentale tra "nome tecnico" del campo (breve, in
  inglese o italiano pulito, senza spazi, lo vede solo il programma)
  e "testo visibile" all'utente (naturale, descrittivo, lungo a piacere).
  Sono due decisioni separate per la stessa cosa.
- Il concetto di "schema dati": prima di costruire un form o un dialogo,
  si decide quali caselle esistono. Poi si formulano le domande per
  riempirle. L'ordine inverso porta a chiedere cose a caso.

**💡 Mia intuizione / scelta ragionata: le esperienze informali**
Ho proposto di aggiungere un campo dedicato per esperienze non formali
(lavoretti, aiuti in famiglia, volontariato, esperienze brevi senza titoli
o certificati). Motivazioni:
- Il pubblico realistico di AI-CV-COACH include molte persone con
  percorsi non lineari, per cui un profilo con solo "esperienze formali"
  esclude buona parte della loro vita lavorativa reale.
- E un'ulteriore difesa anti-invenzione strutturale: senza un posto
  adatto per le esperienze vaghe, l'AI sarebbe tentata di "promuoverle"
  a esperienze formali per farle entrare nei campi disponibili, creando
  invenzioni (es. "aiutavo mio zio idraulico" -> "Idraulico, 3 mesi").
  Avere il campo dedicato evita questa pressione.

Sfumature decise:
- Nome tecnico scelto: esperienze_informali (parallelo a esperienze_formali).
- Struttura interna: leggera, tutti i sotto-campi opzionali, perche e la
  natura del campo. L'utente compila solo quello che ricorda.
- Per la generazione CV in fase successiva: queste esperienze andranno
  trattate con cautela, senza "promuoverle" automaticamente a esperienze
  formali. La formulazione precisa del prompt verra decisa quando saremo
  nel contesto della generazione CV.

**Dove ho faticato / cosa non era ovvio**
Ho inizialmente proposto come nome del campo una frase descrittiva
("competenze acquisite anche tramite esperienze pratiche non formali").
Ragionando, ho capito che era un "testo visibile all'utente", non un
"nome tecnico" da JSON. Distinzione utile per il futuro.

**Cosa ho deciso e perche**
- Partire da 5 campi essenziali, non di piu: in MVP, meno e meglio.
  Aggiungeremo (contatti, lingue, ecc.) solo dopo aver visto la pipeline
  girare.
- Tenere le esperienze informali separate da quelle formali: non sono
  esperienze "di serie B", sono di natura diversa e vanno gestite con
  regole diverse.
- Rimandare la formulazione precisa dei testi visibili al momento in cui
  servono davvero (prompt di generazione CV), invece di tararli a vuoto
  adesso.

### Step 1.2 — Struttura del dialogo guidato (MVP)

*Definito lo scheletro del dialogo, non ancora i testi delle domande.*

**Cosa ho fatto**
Ho definito la struttura del dialogo guidato dell'MVP con quattro decisioni:
1. Forma: ordine fisso + un argomento per turno + loop di conferma.
2. Ordine: i cinque campi nella sequenza dello schema (nome → esperienze formali → esperienze informali → competenze → formazione); le competenze restano un turno dedicato (Strada A), non derivate.
3. Tipi di turno: due soli — *singolo* (per `nome`) e *ripetibile* (per gli altri quattro campi-lista), con domanda-ponte "altro o procediamo?" a fine di ogni voce.
4. Risposta storta (confusa, vuota o saltata): comportamento unico — l'AI non insiste, dichiara in modo neutro che lascia vuoto, e procede.

Non ho ancora scritto il testo preciso delle domande: lo farò nel contesto giusto (principio "frasi visibili nel contesto").

**Cosa ho imparato**
Che "la forma più semplice" e "la forma più vicina all'MVP" sono la stessa cosa: meno parti mobili significa insieme meno codice e più fedeltà al concetto di MVP. E che qui il lavoro anti-invenzione spesso non lo fa il codice ma la formulazione della domanda (vedi competenze).

**Dove ho faticato / cosa non era ovvio**
Non avevo notato che "una domanda per turno" non bastava come categoria. Tre campi su cinque sono liste (posso avere più esperienze, più competenze, più titoli), quindi quel turno deve poter raccogliere più voci. Da qui la distinzione tra turno singolo e turno ripetibile, che all'inizio non avevo in mente.

**Cosa ho deciso e perché**
- Ordine fisso invece che adattivo: meno logica da costruire e, soprattutto, l'adattivo tende a guidare l'utente verso le risposte — contrario al vincolo anti-invenzione.
- Strada A per le competenze (turno dedicato ancorato via testo) invece di derivarle: tiene il campo riempito da risposta diretta dell'utente, senza logica nuova nell'MVP.
- Due tipi di turno: imposti dai campi-lista, servono la domanda-ponte "altro o procediamo?".
- Comportamento unico sui vuoti: è il "default sicuro" applicato al dialogo (vuoto è meglio di inventato) e lascia la porta aperta a pending_questions.

💡 *Mia intuizione / scelta ragionata* — Strada A con ancoraggio via testo: il lavoro anti-invenzione sulle competenze lo sposto sulla formulazione della domanda, non su una regola nel codice. Il punto più delicato (chiedere "che competenze hai?" a freddo invita al gonfiamento) si disinnesca scegliendo bene le parole, non aggiungendo logica.

💡 *Mia intuizione / scelta ragionata* — I "vuoti" dell'MVP non sono sprechi. Quando aggiungerò pending_questions, saranno proprio le voci da riprendere nel secondo giro. L'MVP fa la versione povera della stessa idea; il raffinamento la versione ricca. Niente da buttare.

### Step 1.3 — Testi del dialogo: nome + turni-esperienza (MVP)

*Scritto lo scheletro testuale dei primi tre turni. Mancano competenze e formazione, rimandati a sessione dedicata (le competenze sono il punto più delicato per l'anti-invenzione).*

**Cosa ho fatto**
Scritto i testi di tre turni: `nome` (singolo) e i due turni-esperienza `esperienze_formali` e `esperienze_informali` (ripetibili). Per i ripetibili ho definito i tre pezzi — apertura, conferma a scheda, reminder-ponte — e un "patto" esplicito che annuncia il meccanismo (una alla volta, ti mostro cosa ho capito, confermi o correggi, poi la prossima). I testi sono in prompt_design.md.

**Cosa ho imparato**
- La coerenza tra turni è di *funzione*, non di *lettera*: stesso patto, forma adattata al contesto (gli incipit "Partiamo…" e "Raccontami ora…" cambiano, il meccanismo no).
- La domanda d'apertura fa lavoro anti-invenzione *diverso* a seconda del campo: nelle formali frena il gonfiamento, nelle informali dà il permesso. Stesso scheletro, insidia opposta.
- Chiedere di raccontare una esperienza alla volta non serve solo all'ordine: serve a orientare l'utente a restare sui fatti e a ridurre il margine in cui l'LLM potrebbe inventare.

**Dove ho faticato / cosa non era ovvio**
Mi è venuto il dubbio se l'AI riuscisse a distinguere più esperienze raccontate insieme in un unico blocco. La risposta è che ci riuscirebbe, ma abbiamo capito che è meglio non chiederglielo: farglielo fare aprirebbe spazio a interpretazioni e quindi a invenzioni. Meglio orientare l'utente a procedere una alla volta e tenere il compito dell'AI ristretto.

**Cosa ho deciso e perché**
- Apertura stretta nelle formali (solo lavori, studi/corsi dopo): non caricare l'AI di cernita, restare fedeli all'ordine fisso deciso allo Step 1.2.
- Conferma a scheda strutturata a vista: serve il controllo dell'utente e rende visibile il campo vuoto (anti-invenzione).
- Campo vuoto = `(non specificata)`; normalizzazione leggera (riordina sì, traduci-in-CV no); tre esiti disponibili ma non recitati (invito aperto, meccanica nel prompt).
- Patto esplicito in tutti i turni ripetibili: il meccanismo va spiegato la prima volta che compare, non la seconda.
- "Una alla volta": invito gentile nel testo + regola di prompt (l'AI lavora la prima voce, le altre rientrano dal "altro o procediamo?").

💡 *Mia intuizione / scelta ragionata* — L'insidia anti-invenzione è opposta nei due turni-esperienza: gonfiamento nelle formali, "suggerire troppo" nelle informali. Per questo negli esempi delle informali uso categorie larghe (aiutare un familiare, dare una mano in associazioni) invece di attività specifiche: invitare senza imboccare.

💡 *Mia intuizione / scelta ragionata* — Far raccontare una esperienza alla volta non è un limite tecnico dell'AI (saprebbe separarle), ma una scelta di togliere margine di interpretazione, e quindi di invenzione. Stessa logica della cernita studi/lavori: orientare l'utente serve a tenere l'LLM sui fatti.

### Step 1.4 — Turni competenze e formazione (MVP)

Competenze e formazione meritano un passo dedicato, separato dagli altri turni del dialogo. La ragione è il vincolo anti-invenzione: il turno delle competenze è il punto più esposto al rischio di gonfiamento dell'intero dialogo. Chiedere "che competenze hai?" a freddo invita l'utente a vendersi e l'AI a incassare abilità generiche mai dimostrate. Per questo ho scelto di non trattarlo in fretta in coda agli altri turni, ma di dedicargli (insieme a formazione, l'ultimo turno rimasto) uno step a sé, da progettare con cura particolare.

**Cosa ho fatto**
Ho progettato gli ultimi due turni del dialogo, competenze e formazione. Con questo l'anello 1 (raccolta del profilo via dialogo guidato) è completo a livello di testi. Competenze: turno ad ancoraggio leggero (è l'utente a dichiararle, non l'AI a proporle), con raccolta in blocco e una conferma anti-dimenticanza a un giro. Formazione: ricalco del turno esperienze formali, più la chiusura del dialogo, che mostra un riepilogo del profilo e ribadisce un'ultima volta il vincolo anti-invenzione.

**Cosa ho imparato**
Che non tutti i turni "ripetibili" sono uguali: le competenze hanno una meccanica diversa (raccolta in blocco invece che una voce alla volta) perché il campo è una lista di stringhe, non di oggetti ricchi come le esperienze. La struttura del turno deve seguire la natura del dato, non un unico stampo. E che il vincolo "un profilo, molti CV" ha conseguenze pratiche fin dentro la frase di chiusura: mi ha costretto a rinunciare al "CV neutrale" e scegliere un riepilogo.

**Dove ho faticato / cosa non era ovvio**
Due punti. Primo: volevo far generare a fine dialogo un "CV neutrale" da usare come base per i CV personalizzati. Sembrava utile, ma sfondava il principio "un profilo, molti CV" — un CV è un output, non una sorgente. Ci ho ragionato e ho capito che quello che cercavo davvero era un modo di vedere il risultato concreto, e per quello basta un riepilogo del profilo. Secondo: avevo aperto le competenze con "ultima cosa", ma non era l'ultima — mancava la formazione. Una promessa sbagliata all'utente, corretta solo perché mi sono fermato a guardare la frase nella sua posizione reale.

**Cosa ho deciso e perché**
- Ancoraggio leggero per le competenze (non forte): l'AI non propone competenze a partire dalle esperienze, le dichiara l'utente. Più snello per l'MVP e più fedele all'anti-invenzione (l'AI che propone è l'AI che inventa).
- Raccolta in blocco per le competenze: imposta dalla natura del campo (stringhe brevi), evita un ping-pong assurdo di conferme una alla volta.
- Niente CV neutrale, ma riepilogo del profilo: rispetta "un profilo, molti CV" e non anticipa l'anello 4 (generazione).
- Formazione come ricalco delle esperienze formali: nessuna logica nuova, rischio invenzione basso perché un titolo di studio è un fatto verificabile.

💡 *Mia intuizione / scelta ragionata* — La forma di un turno deve seguire la natura del suo campo. Competenze (lista di stringhe) → raccolta in blocco; esperienze e formazione (liste di oggetti) → una voce alla volta. Non ho forzato tutti i turni nello stesso stampo solo per coerenza apparente: la coerenza vera è di funzione, non di forma.

💡 *Mia intuizione / scelta ragionata* — Il riepilogo del profilo al posto del CV neutrale non è solo una rinuncia per rispettare un principio: è una scelta migliore. Dà all'utente lo stesso senso di concretezza ("ecco cosa ho costruito"), ma fa anche intuire che il valore vero dell'app viene dopo — allineare il proprio profilo a un annuncio specifico. Un CV finito alla fine del dialogo avrebbe "chiuso"; il riepilogo apre.

### Step 1.5 — Primo gesto implementativo: rete di sicurezza per la chiave (.gitignore)

*Aperta la parte implementativa della Fase 1. Primo "codice" messo nel repo: non una funzione, ma la protezione che viene prima di tutto il resto.*

**Cosa ho fatto**
Posato il primo mattone operativo del progetto: creato e committato il file `.gitignore` con due righe (`.env` e `node_modules/`). Prima di farlo ho deciso l'architettura con cui l'app userà la chiave API di Anthropic (fornita da Riccardo). Ho verificato ogni passaggio con i miei occhi — contenuto del file con `cat`, presenza e posizione con `ls -a`, commit con `git log` — invece di fidarmi di quanto dichiarava lo strumento.

**Cosa ho imparato**
- Esistono due "superfici" di esposizione diverse, che all'inizio confondevo: il *repository* su GitHub (il codice pubblico, la cronologia) e l'*app in esecuzione* (quello che il browser di un visitatore carica). Il `.gitignore` protegge la prima; l'architettura con l'aiutante Node protegge la seconda. Sono difese diverse per problemi diversi.
- L'ordine dei gesti conta: la rete di sicurezza va tesa *prima* che esista qualcosa da proteggere. Creando il `.gitignore` adesso, quando aggiungerò il `.env` con la chiave dentro Git lo ignorerà già — non ci sarà mai un istante in cui la chiave rischia di finire in un commit.
- La differenza tra i terminali del sistema: aprendo una scheda nuova mi si è presentato Windows PowerShell invece di Ubuntu, e i comandi non funzionavano. Sono due ambienti diversi; i comandi che uso (`ls -a`, `cat`, `git` così come li scrivo) vivono in Ubuntu.

**Dove ho faticato / cosa non era ovvio**
Aprire una scheda col `+` non dà Ubuntu ma il terminale di default (PowerShell), e da lì niente andava. Ho dovuto scegliere Ubuntu dal menu a tendina e poi spostarmi a mano dentro il repo con `cd`, perché la scheda nuova partiva dalla home dell'utente e non dalla cartella del progetto.

**Cosa ho deciso e perché**
- Architettura con aiutante Node (la chiave dietro un piccolo server locale, non esposta nel browser) invece della via più semplice (chiamata diretta dal browser): l'app è destinata a essere mostrata al pubblico, meglio nascere puliti che rattoppare dopo. Il costo (un pezzo Node in più) è piccolo, perché Node è già installato sulla macchina.
- `.gitignore` come primissimo gesto, prima di qualunque codice o chiave: la protezione prima dell'oggetto da proteggere.
- Delegato commit e push a Claude Code, ma a nome mio e con titolo/descrizione decisi da me: prima ho verificato l'identità Git locale (`mirco-parenti`) per assicurarmi che il commit uscisse a mio nome e non sotto l'account di Riccardo.

💡 *Mia intuizione / scelta ragionata* — Le due superfici di esposizione (repo vs app in esecuzione) sono il modo giusto per ragionare sulla sicurezza della chiave. Documentare tutto su GitHub, da solo, non bastava a giustificare l'aiutante Node: il repo era già protetto dal `.gitignore`. Quello che l'aiutante Node previene è un'altra cosa — la chiave visibile nel browser dell'app pubblicata. Capire *quale* problema sto risolvendo evita di scegliere una soluzione giusta per il motivo sbagliato.

💡 *Mia intuizione / scelta ragionata* — Delegare a Claude Code la *scrittura* di un commit è comodo, ma il confine è il *controllo*: leggere cosa sta per fare prima di confermare resta compito mio. Stavolta è filata liscia perché la posta era bassa (un file da due righe); quando i commit toccheranno codice vero, quel "leggo prima di confermare" sarà la rete vera, non un passaggio formale.

### Step 1.6 — Decisione di architettura a due fasi (HTML+Node → VB.NET) e allineamento del README

*Chiarita la rotta tecnologica del progetto e aggiornata la documentazione di conseguenza. Nessun codice scritto: è uno step di decisione e di messa in ordine.*

**Cosa ho fatto**
Deciso l'impianto tecnologico del progetto su due fasi: il prototipo (Fasi 1–2) usa un frontend in HTML più un aiutante locale in Node (utility temporanea che custodisce la chiave API e fa da tramite verso l'API LLM); in Fase 3, a prototipo consolidato, il progetto migrerà su un'unica applicazione VB.NET sotto Windows 11. Ho ristrutturato la sezione "Tecnologie previste" del `README.md` distinguendo le due fasi, allineato la fase 3 della roadmap (non più "backend Python" ma migrazione a VB.NET) e aggiunto una riga di copyright. Stabilita inoltre una regola sulla paternità dei commit.

**Cosa ho imparato**
- Il frontend e il "motore" sono pezzi staccabili, collegati solo da un'interfaccia (la coppia richiesta/risposta verso l'AI): posso sostituire il primo senza toccare il secondo. È questo che rende la migrazione possibile senza buttare il lavoro di valore.
- Una pagina HTML nel browser non può custodire un segreto: tutto ciò che la pagina "sa" è visibile a chi la apre. Per questo il prototipo ha bisogno dell'aiutante Node come custode della chiave. Un programma vero come VB.NET potrà invece tenere la chiave e chiamare l'API da sé, rendendo l'aiutante superfluo.
- Il valore del progetto (il dialogo guidato, i prompt, il vincolo anti-invenzione) vive sopra la tecnologia: non è né HTML né VB.NET, e migra intatto.

**Dove ho faticato / cosa non era ovvio**
Allo Step 1.5 avevo motivato la scelta dell'aiutante Node dicendo che "l'app è destinata a essere mostrata al pubblico". Ragionando sulla migrazione ho dovuto precisare quel ragionamento: la versione pubblica sarà quella VB.NET, mentre l'attuale frontend HTML + aiutante Node è impalcatura temporanea, pensata per girare in locale sul mio PC. Quindi oggi l'aiutante Node non serve "perché l'app è pubblica" (non lo è ancora), ma per un motivo più ristretto e tecnico: anche solo per provare le chiamate in locale, il browser non può tenere la chiave. Non ho riscritto lo Step 1.5: resta com'era, perché il diario deve mostrare come il pensiero è evoluto, non far finta di aver capito tutto subito.

**Cosa ho deciso e perché**
- Architettura a due fasi: HTML + aiutante Node per il prototipo (Fasi 1–2), VB.NET in Fase 3. La migrazione è collocata a prototipo consolidato, non subito: prima valido l'idea e i prompt con l'MVP nell'ambiente che ho già montato (WSL/Node), poi reimplemento nella tecnologia finale.
- Aiutante Node trattato come utility temporanea e usa-e-getta, non come materia di studio: lo configuro tramite Claude Code, non lo approfondisco riga per riga; alla migrazione sparisce. Mi basta sapere accenderlo e spegnerlo.
- README aggiornato in forma programmatica ("prevede"), non al presente ("usa"): documenta la decisione senza dichiarare pezzi che ancora non esistono.
- Paternità dei commit: i commit riportano esclusivamente la mia paternità (© 2026 Mirco Parenti), senza co-autorship di Claude se non richiesta esplicitamente.

💡 *Mia intuizione / scelta ragionata* — Trattare l'aiutante Node come "impalcatura da cantiere" è economia, non pigrizia: studiarne il codice sarebbe tempo sprecato, visto che alla migrazione verrà smontato. Metto l'attenzione dove resta valore (il dialogo), non dove verrà buttato.

💡 *Mia intuizione / scelta ragionata* — La migrazione HTML→VB.NET non mi spaventa più da quando ho capito il confine tra "faccia" e "motore": butterò solo il frontend HTML e l'aiutante Node, mentre il cuore del progetto (dialogo, prompt, schema, anti-invenzione) resta. Non butto via tutto, butto via l'impalcatura.

### Step 1.7 — Prima esecuzione reale: impalcatura e strutturazione dei turni

*Primo pezzo di progetto vivo: l'impalcatura per far girare la strutturazione con chiamata reale all'AI, e il collaudo dei turni sul campo.*

**Cosa ho fatto**
Delegato a Claude Code il montaggio dell'impalcatura Node.js (il server locale e il collegamento alla chiave API). Creato io il file `.env` con la chiave. Poi ho fatto girare i turni di strutturazione e li ho testati io, dalla test-page HTML. Ho lavorato su un branch parallelo (`step-2-prompt-router`); a fine lavoro ho fatto il merge in `main`, ho eliminato il branch usato e ho aperto un nuovo branch (`step-3-orchestrazione-frontend`) per procedere. Ora `main` è aggiornato e sono pronto sul nuovo branch.

**Cosa ho imparato / verificato**
- L'anti-invenzione tiene sul campo. L'ho messa alla prova io, scrivendo input volutamente sporchi e imprecisi, più volte: l'AI ha sempre strutturato solo ciò che avevo davvero detto, senza inventare, gonfiare o riempire i vuoti. Vedere la regola reggere su input reali è diverso dall'averla decisa a tavolino.
- La rete del `.gitignore` ha retto anche delegando il lavoro a Claude Code. Il file `.env` vive sul disco con la chiave dentro, ma Git non lo traccia (verificato: non compare tra i file versionati) perché il `.gitignore` lo esclude. Quindi la chiave non è mai entrata in un commit né è finita su GitHub: che il file esista sul disco non significa che Git lo veda.

**Dove ho faticato / cosa non era ovvio**
Avevo deciso di costruire solo il turno `nome`. Claude Code, in autonomia, ha realizzato tutti e cinque i turni — ma uno alla volta: prima di procedere con il turno successivo me lo sottoponeva, e io lo modificavo o lo confermavo. Lo scope è quindi cresciuto oltre il `nome` previsto, ma è rimasto sotto il mio controllo, turno per turno.

**Cosa ho deciso e perché**
- I prompt di strutturazione dei quattro turni diversi da `nome` restano per ora dentro `server.js`, in forma provvisoria: andranno in `prompt_design.md` solo una volta definitivi. Il design deve contenere i prompt finali, non le bozze di prova.
- Due rifiniture tecniche segnate come lavoro in arrivo: togliere il fence markdown che il modello a volte aggiunge attorno al JSON (le marcature di blocco-codice), così il programma può leggere il JSON pulito; e riportare i turni-lista a "una voce per turno", coerentemente con la struttura decisa agli Step 1.2–1.3.

💡 *Mia intuizione / scelta ragionata* — Delego l'esecuzione, non il controllo né le decisioni di design. Claude Code può scrivere il codice e proporre, ma cosa si costruisce, in che ordine e con quali scelte di fondo resta una mia responsabilità: il mio compito è guidare e leggere ciò che produce, non firmare a scatola chiusa.

💡 *Mia intuizione / scelta ragionata* — Il valore del progetto (i prompt, il dialogo) deve finire nel design una volta che è definitivo, non nel codice usa-e-getta che sto usando ora solo per testare e collaudare il processo. La test-page e l'aiutante Node sono impalcatura temporanea verso VB.NET; `prompt_design.md` è ciò che resta.

### Step 1.8 — Orchestrazione del dialogo nel front-end (anello 1 completo end-to-end)

*I cinque turni, prima isolati, ora si concatenano in un'unica conversazione che riempie il profilo. Primo anello della pipeline funzionalmente completo.*

**Cosa ho fatto**
Trasformato la test-page in un dialogo guidato vero. Ho fatto costruire i cinque turni dell'orchestrazione uno alla volta, decidendo io per ciascuno lo stile di raccolta dati (più campi per esperienze e formazione, stringa singola in lista per le competenze, ecc.), lo stile di ripetizione della domanda e il comportamento sui campi che l'IA non capisce (restano vuoti). Il dialogo parte dal nome, percorre i cinque turni nell'ordine fisso, mostra le schede di conferma, gestisce Sì/Correggi, fa il loop "altro o procediamo?", raccoglie le competenze in blocco e chiude con un riepilogo leggibile del profilo. Lavorato sul branch `step-3-orchestrazione-frontend` (commit `36e3ed6`); il merge in `main` lo faccio a step chiuso.

**Cosa ho imparato**
- La distinzione tra come il profilo *vive* e come lo *mostro*: dentro il programma il profilo è un oggetto JSON (formato tecnico, con graffe e virgolette); all'utente lo presento come riepilogo leggibile (box con etichette). Stesso dato, due forme — la stessa distinzione "nome tecnico vs testo visibile" dello Step 1.1, applicata all'intero profilo invece che al singolo campo.
- Concatenare turni che funzionano da soli non è automatico: serve uno "stato" unico (il profilo) che cresce di turno in turno e un motore che sa quale turno viene dopo. I turni isolati erano i mattoni; l'orchestrazione è la malta.

**Dove ho faticato / cosa non era ovvio**
Rispetto allo Step 1.7 ho tenuto un ritmo più stretto: invece di validare un blocco già fatto, ho fatto costruire e impostare ogni turno uno alla volta, decidendo io le opzioni di ciascuno prima di passare al successivo. È costato più tempo, ma mi ha tenuto il controllo del design — ho capito che con Claude Code il "uno alla volta" non è lentezza inutile, è il modo in cui resto io a decidere la forma.

**Cosa ho deciso e perché**
- Conferma in blocco confermata sul campo: se in un turno-lista racconto più voci insieme, la scheda le mostra tutte (un box per voce) e le confermo in un colpo solo, non una per una. Coerente con la natura dei campi-lista (Step 1.4).
- Riepilogo finale leggibile, non JSON grezzo: la chiusura mostra il profilo in forma umana (box con etichette, vuoti come `(non specificata)`). Chiude il "sospeso" dello Step 1.4 sul formato del riepilogo: il JSON resta forma interna (visibile in console per il debug), l'utente vede il riepilogo leggibile.
- Tre rifiniture lasciate fuori per scelta MVP: rimozione di singole competenze (c'è solo l'aggiunta), routing a linguaggio naturale (si usano i bottoni), editing campo-per-campo. Il design le prevede; le rimando.
- I quattro prompt di strutturazione diversi da `nome` restano provvisori in `server.js`: andranno in `prompt_design.md` solo da definitivi.

💡 *Mia intuizione / scelta ragionata* — L'anello 1 (raccolta del profilo via dialogo) è il pezzo distintivo e raro del progetto, ed è il primo a diventare funzionante end-to-end. Tenere il ritmo "un turno alla volta, decido io" proprio su questo anello è stato giusto: è il cuore originale di AI-CV-COACH, il punto dove non valeva la pena delegare le scelte di forma.

💡 *Mia intuizione / scelta ragionata* — Il riepilogo leggibile non è "il JSON vestito bene": è il confine tra il sistema e la persona. Il programma lavora in JSON perché gli serve preciso e confrontabile; l'utente vede il riepilogo perché gli serve comprensibile. Tenere le due forme separate è la stessa disciplina anti-invenzione di sempre — ogni cosa nel formato giusto per chi la usa.

### Step 1.9 — Allineamento della documentazione e maturazione nell'uso di Claude Code

*Step di consolidamento, senza nuove funzioni: ho riportato i prompt definitivi nel design e riordinato la documentazione. Il valore vero di questo passo, più che nel codice, sta nel modo in cui ho gestito Claude Code — e nel fatto che ormai lavoro quasi solo lì.*

**Cosa ho fatto**
Allineato `prompt_design.md` al codice già validato: portati i quattro prompt che vivevano solo in `server.js` dentro il documento di design, aggiornata la regola sui "più voci insieme" alla scelta presa allo Step 1.8 (estrai-tutte + conferma in blocco), e aggiunta la sezione "Problemi e mitigazioni" come elenco sintetico che rimanda al diario. Tutto su un branch dedicato, verificato e poi fuso in `main`.

**Cosa ho imparato — il passaggio da Claude (chat) a Claude Code**
All'inizio della Fase 1 avevo separato i ruoli: ragionare il design con Claude chat, poi costruire con Claude Code. In pratica i due ruoli si sono fusi: ormai progetto e implemento nello stesso posto, dentro Claude Code, che propone le scelte, mi fa le domande giuste e poi scrive. È più fluido — un filo solo invece di due — ma sposta tutto il peso del controllo su di me: devo leggere e approvare passo per passo, perché è sparita la "tappa di ragionamento" separata che prima faceva da filtro.

**Dove ho faticato / cosa non era ovvio**
Che Claude Code, se non lo sorvegli, "deriva". In questa sessione l'ho colto su due cose concrete: i messaggi di commit erano finiti in italiano invece che in inglese (la convenzione del mio repo) e portavano un co-autore "Claude" che non volevo. Le ho fatte correggere e ho fissato la regola. Lezione: delegare l'esecuzione non è delegare il controllo — e il controllo è reale solo quando becchi un errore vero, non quando fila tutto liscio.

**Cosa ho deciso e perché**
- Documentazione: `prompt_design.md` tiene i prompt **definitivi e aggiornati**, il diario tiene la **narrazione**; niente duplicati (i problemi già raccontati nel diario li ho solo richiamati nella nuova sezione, non riscritti).
- Commit: d'ora in poi **titolo e descrizione in inglese, solo paternità mia**; quelli già pubblicati li lascio come sono (riscrivere storia già su GitHub non vale il disturbo).
- Branch dedicato anche per sole modifiche di documentazione: stesso metodo degli step di codice, `main` resta sempre pubblicabile.

💡 *Mia intuizione / scelta ragionata* — La fusione dei due "Claude" in uno solo non cambia il mio ruolo: resto il regista. Anzi, lo rende più impegnativo, perché senza la tappa intermedia di Claude chat sono io l'unico filtro tra la proposta e il codice. Lavorare bene con Claude Code è meno "fare domande" e più "saper verificare".

💡 *Mia intuizione / scelta ragionata* — Far auto-verificare Claude Code (controlli automatici sui file, confronto prompt-codice carattere per carattere) è diventato parte del metodo: non mi fido di "fatto", chiedo la prova. È la versione operativa del "verifico con i miei occhi" dello Step 0.3.

### Step 1.10 — Anello 2: progettazione dell'analisi annuncio (schema, prompt e studio dello stato dell'arte)

*Primo passo del secondo anello della pipeline: dato il testo di un annuncio di lavoro, ricavarne una versione strutturata. Ho progettato schema e prompt da zero, poi ho studiato i migliori estrattori open-source per validare e affinare le mie scelte — imparando, non copiando.*

**Cosa ho fatto**
Progettato lo schema dell'annuncio e il prompt di estrazione, su un branch dedicato, in più tappe con un commit a ogni decisione chiusa. Poi ho fatto fare a Claude Code una ricerca mirata sui migliori estrattori open-source su GitHub, restringendo super-selettivamente fino al progetto più calzante (`amazon-science/job-posting-structure`), e ho usato le sue lezioni per affinare il nostro prompt — riscrivendo tutto con parole mie, mai copiando.

**Cosa ho imparato**
- La differenza tra "estrarre da file" (PDF, OCR, NER addestrato) e "strutturare testo con un prompt LLM": sono problemi diversi, e il nostro è il secondo. Questo ha cambiato quali progetti erano davvero rilevanti — i classici "resume parser" risolvono un problema che noi non abbiamo.
- Il principio idea/espressione del diritto d'autore: si possono assimilare i *concetti* altrui, non la loro forma. "Imparo, non incollo" non è solo etica, è anche il percorso legalmente sicuro (il copyright tutela l'espressione, non le idee).
- Validare le proprie scelte contro lo stato dell'arte dà fiducia: i tool migliori, indipendentemente da noi, fanno ciò che avevamo già deciso (required/preferred, "non menzionato → vuoto", scarto degli input che non sono annunci).

**Dove ho faticato / cosa non era ovvio**
- I "due secchi" di Amazon (requisiti `required` vs `preferred`) sembravano un upgrade, ma per il nostro caso erano un downgrade: perdono il terzo stato "non specificata" che a noi serve per non inventare. Capirlo non era ovvio.
- Tenere il filo lungo dello studio senza disperdermi: il panorama era pieno di progetti simili ma non centrati, e ho dovuto più volte chiedere di restringere ("super-selettivo") per arrivare a ciò che ci serviva davvero.

**Cosa ho deciso e perché**
- **Schema a due zone**: un *nucleo confrontabile* col profilo (`competenze_richieste`, `esperienza_richiesta`, `formazione_richiesta`) e dei *campi di contesto* (`titolo`, `sede`, `contratto`, `mansioni`, `benefit`). Il nucleo deve "rispecchiare" il profilo, perché è ciò che renderà possibile il match dell'anello 3.
- **Priorità a tre valori** (`richiesto` / `preferenziale` / `non specificata`), assegnata **solo se l'annuncio lo dichiara esplicitamente**; altrimenti `non specificata` (default sicuro, niente invenzione).
- **`contratto` come sotto-oggetto** (tipo, durata, orario, retribuzione), riempito solo per ciò che l'annuncio dichiara; **`sede` come lista** (un annuncio può avere più sedi); **`mansioni` e `benefit`** come campi di contesto distinti (cosa si farà vs extra oltre la paga).
- **Prompt unico per l'MVP**, non decomposto, ma diviso in **5 sezioni numerate** pensate come futuri sotto-prompt: la via decomposta (più prompt separati) è più potente ma avanzata, la rimando preparando però il terreno.
- **Tenere il nostro "priorità per-voce" invece dei "due secchi"**: i due secchi costringono a una scelta binaria che diventa invenzione quando l'annuncio non qualifica un requisito; il nostro modo, con tre stati, resta fedele. Ho però assimilato il *pensiero* dei due secchi come guida nel prompt ("ragiona a secchi") — il bene della loro idea senza la loro rigidità.
- **Anni di esperienza come ibrido**: `anni` come numero quando l'annuncio lo indica, `testo` sempre con la frase; e quando non serve esperienza l'output è la voce "Nessuna esperienza richiesta".
- **Niente flag booleani** (remote, full_time, ecc.): ridondanti con `sede`/`contratto`, sarebbero "pomposi". **Niente salary/wage separati**: è una distinzione USA; teniamo la retribuzione come la descrive l'annuncio.
- **Taxonomy mapping**: tecnica nuova trovata nello studio (mappare le skill su una tassonomia ESCO/O*NET). Potentissima per il **match (anello 3)**, ma in tensione con l'anti-invenzione in fase di estrazione → l'ho messa in memoria per riprenderla all'anello 3, con tanto di fonti.
- **Vincolo legale chiarito**: assimiliamo i concetti, non copiamo codice/testo/tassonomia altrui (`amazon-science/job-posting-structure` è CC BY-NC-SA, uso non commerciale).

💡 *Mia intuizione / scelta ragionata* — Validare *prima* di costruire: ho voluto cercare lo stato dell'arte prima di sfoderare le energie sull'implementazione, per non innamorarmi di una soluzione senza aver guardato come fanno i migliori. È costato tempo, ma mi ha dato sia conferme sia idee.

💡 *Mia intuizione / scelta ragionata* — Il design giusto dipende dall'uso, non dall'autorevolezza della fonte. Ho tenuto la nostra scelta contro quella di un colosso come Amazon perché il nostro contesto è opposto al loro: loro misurano statistiche su milioni di annunci (forzare in due secchi va bene), noi rappresentiamo fedelmente un singolo annuncio per un singolo utente (la fedeltà conta più della pulizia aggregata).

💡 *Mia intuizione / scelta ragionata* — Preparare il futuro senza pagarlo ora: prompt unico per l'MVP, ma a sezioni nette, così quando lo spezzetterò in più prompt separati la strada sarà già tracciata.

### Step 1.11 — Studio sul campo del miglior estrattore di annunci: validazioni e upgrade dei delimitatori

*Ho aperto il cofano del miglior estrattore di annunci open-source (`amazon-science/job-posting-structure`, che usa Claude Haiku come noi): prompt ed esempio reale input→output, per confrontarlo col nostro. Ne escono due conferme forti e un piccolo upgrade.*

**Cosa ho fatto**
Studiato in profondità il caso che avevo eletto migliore: ho fatto leggere a Claude Code il prompt di estrazione e l'esempio reale (annuncio Amazon "SDE II" → JSON estratto), e li ho confrontati col nostro prompt dell'annuncio. Niente copiato — solo concetti.

**Cosa ho imparato / verificato (le testimonianze)**
- **Validazione 1 — gli "anni" ibridi.** Il loro output rappresenta l'esperienza come numero ("3+ years" → 3): proprio la scelta che avevamo preso. Conferma.
- **Validazione 2 (la più importante) — il "leakage" dei due secchi.** Nell'annuncio Amazon il titolo di studio sta SOLO tra i requisiti *preferenziali*, ma nel loro JSON compare ANCHE tra gli *obbligatori* (e l'esperienza "3 anni" viene duplicata nei due secchi). La struttura a due secchi, costringendo a riempire i campi di entrambi, ha indotto il modello a **inventare/duplicare**. È la prova sul campo che la nostra scelta — priorità per-voce con il terzo stato `non specificata` — **evita un errore che persino un tool di livello-ricerca commette**.
- **Validazione 3 — le skill da tassonomia sono invenzione rispetto al testo.** Le `skills` nel loro output ("Mobile Development", "Leadership"…) non sono nell'annuncio: sono mappate da una tassonomia O*NET. Conferma che il taxonomy mapping va tenuto per il match (anello 3), non per l'estrazione.

**Cosa ho deciso e perché**
- **Integrato l'unico upgrade utile: i delimitatori a tag.** Avvolgo il testo dell'annuncio tra `<annuncio>` e `</annuncio>` e dico all'AI di trattare ciò che sta lì dentro solo come dato, mai come istruzioni. Più robusto del placeholder tra virgolette, e una piccola difesa contro testi-annuncio che provano a "parlare" al modello.
- **Confermato tutto il resto senza modifiche:** numeri (anni sì, salary no), niente flag booleani, distinzione mansioni/requisiti già presente, decomposizione e taxonomy/embedding rimandati (post-MVP / anello 3).

💡 *Mia intuizione / scelta ragionata* — Vedere un colosso sbagliare *proprio dove noi abbiamo scelto bene* vale più di mille conferme teoriche: il nostro design "povero" (per-voce, tre stati) batte il loro "ricco" (due secchi) sul nostro terreno, la fedeltà. Non è fortuna: è che abbiamo progettato per il nostro uso, non per il loro.

💡 *Mia intuizione / scelta ragionata* — Studiare lo stato dell'arte non serve solo a copiare il meglio: serve anche a capirne i limiti, e a riconoscere quando una nostra scelta è già superiore. Il valore vero di questo studio è stato più nelle conferme che nelle novità.

### Step 1.12 — Anello 2 dal design al codice: cablaggio, collaudo su annunci reali e la priorità "secondo il senso"

*Primo gesto implementativo dell'anello 2: ho portato il prompt dell'annuncio dal design al codice e l'ho provato su annunci veri. Dal collaudo è nata una rifinitura importante della priorità.*

**Cosa ho fatto**
Cablato il prompt `analisi_annuncio` nel server (registro `PROMPTS`), alzato `MAX_TOKENS` (l'output dell'annuncio è più lungo dei frammenti del profilo) e creato una piccola pagina di test (`test-annuncio.html`), senza toccare il dialogo (`index.html`). Poi ho incollato annunci reali e guardato cosa estrae.

**Cosa ho imparato — la priorità "secondo il senso"**
Sul campo è emerso il punto più interessante. All'inizio la regola era "priorità solo se esplicita, altrimenti `non specificata`". Ma un annuncio vero ("Cercasi Montatore Falegname con esperienza", "Titolo di studio: Scuola dell'obbligo") non *scrive* "obbligatorio" — eppure è **palese** che quei requisiti servono. Ho capito che la regola giusta è **comprendere il senso, non solo le parole**:
- se è palese che un requisito è obbligatorio → `richiesto` (anche senza la parola esplicita);
- se c'è un attenuante esplicito ("esperienza di basso livello", "anche minima", "gradito") → `preferenziale`;
- `non specificata` solo quando davvero non si capisce.
La differenza tra "con esperienza" (palese → `richiesto`) e "con esperienza di basso livello" (attenuante → `preferenziale`) è la presenza di un attenuante.

**Dove ho faticato / cosa non era ovvio**
La regola "astratta" non bastava: avevo scritto "in dubbio non marcare richiesto", ma il modello — giustamente — leggeva i requisiti palesi come obbligatori. Invece di forzarlo verso `preferenziale` (che sarebbe stato sbagliato), ho cambiato la regola per allinearla al senso. Lezione: a volte è la regola a doversi adeguare al buon senso del modello, non il contrario.

**Cosa ho deciso e perché**
- **La priorità si valuta dal senso, non solo dalla lettera** (vedi sopra). Tolto il "con esperienza" generico dai preferenziali.
- **Il prompt vive in due file** — `prompt_design.md` (design) e `server.js` (codice) — tenuti **identici carattere per carattere**, con verifica a ogni modifica. La nota-campo `priorita` invece sta solo nel design.
- **Collaudo prima di chiudere:** validato su due annunci, uno con requisiti *palesi* e uno con sezioni *esplicite* "Requisiti/Preferenziali" — entrambi corretti.
- **Campi futuri** (livello, settore) segnati come "da valutare" nel design, non aggiunti ora.

💡 *Mia intuizione / scelta ragionata* — "Comprendere il senso oltre il testo" non contraddice l'anti-invenzione: non aggiungo requisiti che non ci sono (quello resta vietato), ma *interpreto correttamente la priorità* di quelli che ci sono. Estrarre solo il vero, sì; ma del vero capirne il peso.

💡 *Mia intuizione / scelta ragionata* — Il collaudo su input reali vale più di mille regole a tavolino: la rifinitura della priorità non l'avrei trovata leggendo il prompt, l'ho vista solo guardando cosa usciva da un annuncio vero.

### Step 1.13 — Casi limite dal campo: precedenza testo-su-sezione e il campo "altri_requisiti"

*Due annunci reali hanno fatto emergere due limiti dell'estrazione, entrambi corretti: la priorità che si fa ingannare dalle sezioni, e i requisiti che non rientrano in nessuna delle tre dimensioni.*

**Cosa ho fatto**
Testati altri annunci reali e analizzato l'output voce per voce. Da un annuncio per contabile sono usciti due problemi, che ho sistemato nel prompt (in `prompt_design.md` e `server.js`, tenuti identici) e ri-validato sul campo.

**Cosa ho imparato / deciso — i due casi**
- **Precedenza del testo sulla sezione (priorità).** L'annuncio scriveva "PROFIS - plus la conoscenza" *fuori* da una sezione "Requisiti preferenziali" presente altrove. Il modello, fidandosi della struttura, l'aveva messo `richiesto`. Ma "plus" dice che è facoltativo → `preferenziale`. Ho aggiunto una **regola di precedenza**: il segnale testuale della singola voce vince sul contesto della sezione. E ho precisato che "plus" è solo *uno dei tanti* segnali: il modello deve riconoscere il senso ("vantaggio gradito ma non obbligatorio"), non spuntare parole da una lista chiusa. Più un esempio concreto (PROFIS) nel prompt.
- **Il nuovo campo `altri_requisiti`.** Lo stesso annuncio aveva, tra i preferenziali, "Domicilio in zona limitrofa allo Studio" — sparito dall'output, perché non è una competenza, un'esperienza o un titolo, e lo schema non aveva un posto per i requisiti di altra natura. Ho aggiunto `altri_requisiti` (lista di `{ testo, priorita }`) per domicilio, disponibilità (turni/weekend/trasferte), patente, automunito, età, iscrizione a un albo, idoneità: cose che il candidato deve soddisfare ma che non si confrontano col profilo.

**Dove ho faticato / cosa non era ovvio**
Il secondo problema non l'avevo notato a occhio: l'ho trovato applicando il **riflesso anti-omissione** che avevamo deciso ("a ogni controllo chiediti: sto dimenticando qualcosa?"). Confrontando l'output con l'annuncio riga per riga, è saltato fuori che un requisito esplicito era stato silenziosamente scartato. Quel riflesso non è formalità: trova cose vere.

**Cosa ho deciso e perché**
- Priorità: il **senso e il testo della singola voce** vengono prima della sezione; "plus"/attenuanti riconosciuti per significato, non per parole esatte.
- Schema: quattro tipi di requisito (i tre confrontabili + `altri_requisiti` non confrontabile), con la regola anti-duplicazione aggiornata.
- Validato sul campo: PROFIS → `preferenziale`, domicilio → `altri_requisiti`, nessuna regressione sui casi già buoni.

💡 *Mia intuizione / scelta ragionata* — Gli annunci veri sono una miniera di casi limite che a tavolino non immagini (un "plus" fuori posto, un domicilio tra i requisiti). Ogni annuncio nuovo affina la regola: la qualità del prompt si costruisce sul campo, un caso alla volta.

💡 *Mia intuizione / scelta ragionata* — "Comprendere il senso oltre il testo" e "verificare anche testualmente" non si contraddicono: vanno insieme. Leggo il significato (PROFIS è un plus → facoltativo) e insieme controllo che nessuna parola dell'annuncio sia andata persa (il domicilio). Senso e letteralità, non l'uno o l'altro.

### Step 1.14 — `altri_requisiti` è confrontabile: relabel e profilo da estendere

*Ripensandoci: i requisiti di `altri_requisiti` (domicilio, patente, disponibilità…) sono eccome confrontabili col candidato. L'avevo etichettato "non confrontabile" troppo in fretta — il limite è il profilo, non l'annuncio.*

**Cosa ho deciso e perché**
Allo Step 1.13 avevo introdotto `altri_requisiti` chiamandolo "non confrontabile col profilo". Era una semplificazione sbagliata: domicilio, patente, automunito, disponibilità sono spesso paletti veri ed è esattamente ciò che vuoi confrontare col candidato ("automunito richiesto" → ce l'ha?). Il vero limite non è l'annuncio, ma il **profilo**, che oggi non cattura questi dati. Ho quindi rietichettato `altri_requisiti` come **confrontabile** (in `prompt_design.md` e `server.js`, tenuti in sync), con la nota che il profilo va esteso per supportarlo. L'estrazione non cambia: cambia il significato che gli diamo.

**Cosa rimando (e perché)**
L'estensione del profilo — una sezione per domicilio, patente, disponibilità, automunito, ecc., a specchio di `altri_requisiti` — la farò **nell'anello 3 (match)**, quando servirà davvero per il confronto. Da fare con la solita anti-invenzione e con un occhio alla sensibilità dei dati personali (domicilio, età).

💡 *Mia intuizione / scelta ragionata* — "Non confrontabile" non era una proprietà dell'annuncio, era una mancanza del mio profilo. Distinguere "il dato non è matchabile" da "non ho ancora dove confrontarlo" evita di chiudere porte che sono solo da aprire più avanti.

### Step 1.15 — Avvio anello 3: il match semantico lo fa l'LLM, non una tassonomia

*Aperto il terzo anello (confronto profilo↔annuncio + punteggio). La prima decisione di fondo: come gestire il "stessa competenza, parole diverse".*

**Cosa ho deciso e perché**
Il match deve risolvere il problema "stessa competenza, parole diverse" ("me la cavo alla cassa" deve combaciare con "uso del registratore di cassa"). Avevo in tasca il **taxonomy mapping** (mappare le skill su una tassonomia standard ESCO/O*NET). Ma ho deciso di **delegare il matching semantico all'LLM** invece di integrare una tassonomia esterna, per tre motivi:
- l'LLM **capisce il contesto e qualunque fraseggio**, incluso il linguaggio informale dei nostri utenti, dove una tassonomia "professionale" arranca;
- una tassonomia/embedding è **rigida** (limitata dalla sua copertura);
- integrarla davvero sarebbe un **detour fuori dal nostro stack** (Node + Claude; ESCO/O*NET è roba Python/ML/dati), per giunta in parte usa-e-getta verso VB.NET.
La tassonomia formale resta un **raffinamento futuro** (categorie standardizzate, utili per statistiche su grandi volumi, non per il singolo match).

**Cosa ho imparato**
Che a volte la soluzione "più semplice" è anche **più affidabile** per il nostro caso: delegare la comprensione all'LLM batte una lista di embedding preimpostata, perché si adatta. Con un'accortezza: l'LLM può **sovra-matchare** (giudicare equivalenti cose che non lo sono). Per questo l'anti-invenzione vale anche nel match — l'LLM deve **giustificare** ogni giudizio, con granularità *soddisfatto / in parte / non soddisfatto*, ancorato al testo reale del profilo; e il punteggio resta **orientativo** (famiglia A, scelta dello Step 0.5).

**Cosa ho fatto**
Registrata la decisione nella sezione "Confronto profilo-annuncio" di `prompt_design.md` e aggiornata la nota di memoria sul taxonomy mapping. Scopo MVP: confronto sulle tre dimensioni confrontabili (competenze, esperienza, formazione); `altri_requisiti` (richiede l'estensione del profilo) e la tassonomia formale rimandati. Il prompt di confronto e il calcolo del punteggio sono il prossimo passo.

💡 *Mia intuizione / scelta ragionata* — Non confondere lo strumento con l'obiettivo. L'obiettivo era "match robusto sui sinonimi", non "usare ESCO/O*NET". Visto che l'LLM raggiunge l'obiettivo da solo, la tassonomia esterna diventa un costo senza un beneficio che oggi mi serve.

💡 *Mia intuizione / scelta ragionata* — L'anti-invenzione non riguarda solo l'estrazione: vale anche nel match. Un LLM che "decide" che due cose combaciano può inventare un'equivalenza che non c'è. Per questo gli chiederò di giustificare ogni giudizio e lo ancorerò al testo del profilo: anche *giudicare*, non solo estrarre, deve restare fedele ai fatti.

### Step 1.16 — Il motore del confronto: due giri (LLM → codice) e la formula del punteggio

*Progettato per intero il cuore dell'anello 3: come LLM e codice si dividono il lavoro nel match, e come nasce il punteggio. Niente codice ancora — prima il ragionamento, come sempre.*

**Cosa ho fatto**
Disegnato l'architettura del confronto e la formula del punteggio, fissandole nella sezione "Confronto profilo-annuncio" di `prompt_design.md`. Allineato `altri_requisiti` come pienamente confrontabile in tutti i file (intro schema, prompt, nota campo, `server.js` — sync verificato) e aggiornata la nota di memoria.

**Cosa ho deciso e perché**
- **Due giri in sequenza stretta.** *Giro 1 — LLM (controllo generale)*: da solo e prima, sui **due JSON già estratti** (profilo dell'anello 1 + annuncio dell'anello 2 — **non** i testi grezzi); ragiona con senso e logica su tutto e consegna il lavoro finito: giudizi per-requisito (`soddisfatto / in parte / non soddisfatto` + spiegazione, confrontati contro il **profilo intero**), una lettura d'insieme e un **suo numero complessivo**. *Giro 2 — Codice*: solo dopo, prende quell'output e produce il punteggio. Mai il contrario, mai in parallelo.
- **Ibrido: l'LLM comprende, il codice rende consistente.** L'LLM fa ciò che sa fare (cogliere equivalenze, contesto, senso); il codice fa ciò che sa fare (sommare in modo deterministico e trasparente). Così il punteggio eredita solo la variabilità dei giudizi a stati discreti, non quella di un numero "a sensazione".
- **`altri_requisiti` dentro al nucleo confrontabile, pari importanza.** Corretto rispetto allo Step 1.15 (dove l'avevo rimandato dall'MVP): domicilio, patente, automunito sono spesso **paletti decisivi**, quindi pesano come gli altri tre. L'estensione del profilo a specchio diventa un raffinamento futuro, **non** un prerequisito al match.
- **La formula.** Punti per esito `1 / 0.5 / 0`; peso per priorità `richiesto 5 / preferenziale 1`; le voci `non specificata` le pesa l'**LLM caso per caso** (importanza `alta 5 / media 3 / bassa 1`, col 3 come fallback dell'incertezza vera). La categoria non pesa, le quattro del nucleo sono pari. `score_base` deterministico dai giudizi; poi si **fonde** il numero dell'LLM come correzione **limitata**: `clamp(−20, +10)`. Il **contesto** entra anch'esso nel voto, ma pesa **1/5** del nucleo (Mirco ha voluto considerarlo). E un **quarto esito**, `non determinabile` — per ciò che non si può dire o che non si "soddisfa" (un benefit offerto, le condizioni di contratto, un dato assente dal profilo) — viene **escluso dal conteggio**, per non inventare un verdetto.

**Dove ho faticato / cosa non era ovvio**
Sul peso di `altri_requisiti` ho sbagliato direzione: l'avevo messo a "pesa meno" del nucleo, e Mirco mi ha corretto — è di **prioritaria importanza** (un "automunito richiesto" può escluderti). Bonificato ovunque. Lì ho anche capito che la differenza di peso la deve fare la **priorità** (richiesto vs preferenziale), non la categoria.

**Cosa rimando (e perché)**
Il **prompt del Giro 1** e lo **schema di output** sono ora scritti in `prompt_design.md` (5 sezioni, 4 esiti). Resta il **cablaggio in `server.js`** con un **nuovo endpoint** (qui gli input sono due, non il singolo `{turno, risposta}`) e una pagina di test, più il codice del Giro 2 (la formula). L'estensione del profilo a specchio di `altri_requisiti`. E un **limite noto**: un requisito *davvero* squalificante non azzera il punteggio (tetto −20) — trattarlo come paletto rigido è un raffinamento futuro, segnalato per non nasconderlo.

💡 *Mia intuizione / scelta ragionata* — Dividere "chi giudica" da "chi conta" risolve il vecchio limite della famiglia A (il voto incoerente): l'LLM porta la comprensione, il codice porta la riproducibilità. Stessi giudizi → stesso punteggio.

💡 *Mia intuizione / scelta ragionata* — L'asimmetria `−20 / +10` è l'anti-invenzione messa in numeri: l'AI è libera di **abbassare** il match quando fiuta un paletto (sicuro), ma può **alzarlo** solo di poco (gonfiare è il rischio). E quando il clamp taglia, lo **mostriamo**: il dissenso forte diventa una nota, non sparisce nella matematica.

💡 *Mia intuizione / scelta ragionata* — Il confronto lavora su **strutturato ↔ strutturato**, mai su testo grezzo. L'estrazione (con i suoi anticorpi) ha già fatto il suo lavoro: il match si fida dei dati puliti, costa meno token e non rischia di re-inventare rileggendo l'annuncio.

💡 *Mia intuizione / scelta ragionata* — Il quarto esito `non determinabile` (escluso dal conteggio) è anti-invenzione allo stato puro: di fronte a ciò che il profilo non dice, o a ciò che non è "soddisfacibile" (un benefit, una clausola di contratto), la risposta onesta è "non si sa" — e una cosa che non si sa non deve né premiare né punire il punteggio. Forzare un sì/no lì sarebbe inventare. Così il contesto può entrare nel voto senza sporcarlo di verdetti finti.

💡 *Mia intuizione / scelta ragionata* — Un peso fisso per i requisiti `non specificata` (il "3 piatto") buttava via ciò che l'LLM sa già fare: capire che, per un cuoco, un "HACCP" buttato lì conta e un "Photoshop" buttato lì no. Mirco ha posto la condizione giusta: il 3 non è una scorciatoia — l'LLM deve prima *ragionare sull'intenzione* della frase (non fermarsi al testo), e solo se davvero non la coglie ripiega su "media". È di nuovo la nostra bussola: in estrazione si resta fedeli al testo, ma nel match si comprende il senso — e qui pesare un requisito ambiguo È comprendere il senso.

### Step 1.17 — Anello 3 in funzione: cablaggio, validazione sul campo e voto in stelle

*L'anello 3 smette di essere progetto su carta: lo cablo in `server.js`, lo provo su 9 combinazioni reali, e i test mi costringono a sistemare il punteggio. Alla fine il match diventa un voto in stelle.*

**Cosa ho fatto**
- Cablato il confronto in `server.js`: nuovo endpoint `POST /confronta` (due input: profilo + annuncio), il prompt del Giro 1 in sync col `prompt_design`, il Giro 2 (`calcolaMatch`) col calcolo deterministico, e la pagina `test-confronto.html`.
- Validato sul campo: 9 combinazioni (3 profili × 3 annunci) estratti dalle pipeline 1 e 2.
- Aggiunto il **passo finale**: il `finale` (0–100) diventa un voto in **stelle 0–5** (un decimale) — il match definitivo.

**Dove ho faticato / cosa il test ha rotto**
Il primo giro di validazione ha mostrato un punteggio **bimodale**: `score_base` o ~96 o 0, mai in mezzo, e tutti i match positivi appiattiti a ~76. Un profilo debole (Anna su un entry-level) prendeva **76 come** un magazziniere perfetto. Causa, trovata ispezionando i giudizi: l'LLM marcava le **lacune** del profilo come `non determinabile` (escluse dal conteggio) invece che `non soddisfatto` → le mancanze sparivano e la base si gonfiava.

**Cosa ho deciso e perché**
- **Confine `non soddisfatto` / `non determinabile` per-dimensione.** `non determinabile` = «non avevo modo di saperlo» (altri_requisiti non ancora raccolti, contesto lato-offerta, requisito dichiarato assente), NON «non l'ha detto». Una competenza/esperienza/formazione non dichiarata — dimensioni che raccogliamo apposta nel dialogo — è `non soddisfatto`. Ri-test: discriminazione tornata (Anna×entry 76 → 51).
- **Sentinel "Nessuna esperienza richiesta" escluso nel codice** (deterministico): l'LLM lo neutralizzava a intermittenza (una volta sì, una no). Spostato in `calcolaMatch` — non è un requisito da soddisfare. Anna×entry 51 → 15.
- **Clamp tenuto a −20/+10.** L'avevo messo in dubbio, ma sui dati post-fix fa il suo lavoro: àncora la base (ora onesta) e lascia all'AI un nudge bilanciato. I due casi in cui scatta tirano in direzioni opposte e vengono frenati bene (Giulia tenuta su a 72, Anna tenuta giù a 15); allargarlo peggiorerebbe uno dei due.
- **`altri_requisiti richiesto` → `non determinabile` è corretto, non un difetto.** La patente B di Marco non era ricavabile dai dati raccolti: tenerla `non determinabile` è l'anti-invenzione che volevamo (l'AI resta cauta sui match forti, e va bene). Estendere il profilo è un miglioramento futuro, non la toppa a un bug.

**Esito**
Colonna entry da `[76, 78, 76]` (piatta) a **`[3.6, 3.1, 0.8]`** stelle: buon fit > sovraqualificato-fuori-ruolo > debole. Mismatch a 0.3. Il sistema ora distingue la **qualità** del match, non solo match/non-match.

💡 *Mia intuizione / scelta ragionata* — "Prima il test, poi la valutazione." Far girare il sistema su casi veri ha trovato in cinque minuti un difetto che a tavolino non avevo visto: la base che satura. Nessuna quantità di ragionamento sostituisce un dato reale che ti contraddice.

💡 *Mia intuizione / scelta ragionata* — `non determinabile` faceva due lavori opposti ("non ho modo di saperlo" e "non l'ha detto") travestiti da uno. Separarli ha sbloccato tutto. Certi bug non sono nel codice ma in un concetto che porta due significati sotto la stessa etichetta.

💡 *Mia intuizione / scelta ragionata* — Avevo puntato il dito sul clamp; il colpevole era la base. I dati hanno detto "il clamp va bene", e gli ho creduto invece di toccarlo per forza. Rivedere non vuol dire per forza cambiare.

### Step 1.18 — Due modelli per due livelli di compito: Haiku per estrarre, Sonnet per ragionare

*Fin qui un solo modello (Haiku 4.5) faceva tutto. Lo sdoppio: i compiti meccanici restano su Haiku, il confronto semantico dell'anello 3 sale a Sonnet 4.6.*

**Cosa ho fatto**
- In `server.js` ho sostituito l'unica costante `MODEL` con due: `MODEL_SEMPLICE = "claude-haiku-4-5"` e `MODEL_RAGIONAMENTO = "claude-sonnet-4-6"`. La funzione `chiamaAnthropic` ora accetta il modello come terzo parametro (default Haiku).
- L'estrazione (anello 1 — turni del profilo; anello 2 — analisi annuncio) continua su Haiku. Il confronto (anello 3, `/confronta`) passa esplicitamente `MODEL_RAGIONAMENTO`.
- Allineata la documentazione: nota "Modelli usati" e richiamo inline nel Giro dell'LLM in `prompt_design.md`, riga "Tecnologie previste" nel `README.md`.

**Cosa ho deciso e perché**
- **Il modello segue la profondità del compito, non il contrario.** Estrarre nome/requisiti è un compito ristretto a output strutturato: Haiku è veloce ed economico e basta. Il match semantico — cogliere equivalenze ("me la cavo alla cassa" ↔ "uso del registratore di cassa"), pesare requisiti ambigui, leggere l'insieme — è ragionamento vero: lì Sonnet ripaga il costo maggiore.
- **Default = Haiku, ragionamento = scelta esplicita.** Il default del parametro è Haiku, così ogni nuovo turno di estrazione eredita il modello giusto senza interventi; Sonnet si attiva solo dove serve, passandolo a mano. Una sola leva da ricordare.

**Dove ho faticato / cosa non era ovvio**
Niente di tecnicamente difficile — un parametro e due costanti. Il punto vero era *dove* tracciare il confine: non "Sonnet ovunque per sicurezza" (costo e lentezza inutili sui compiti meccanici), ma neanche "Haiku ovunque per risparmio" (il match ne soffre). Il confine giusto coincide con quello che il progetto già traccia da sempre: estrazione vs comprensione.

💡 *Mia intuizione / scelta ragionata* — La stessa linea che separa "estrarre fedele al testo" da "comprendere il senso" (la bussola dell'anello 3) separa anche i due modelli. Non è una coincidenza: dove il compito cambia natura, cambia anche lo strumento giusto. Pagare Sonnet sull'estrazione sarebbe sprecare ragionamento dove serve solo precisione.

### Step 1.19 — Setting di governance: regole, memoria e il comando "aggiorna-tutto"

*Uno step non di codice ma di metodo: ho separato e fissato le regole di lavoro, riordinato la memoria e definito come tenere allineati i file. Un setup importante, prima di tornare a costruire (anello 4).*

**Cosa ho fatto**
- Separato le regole in due cassetti: **`regole_globali`** (`~/.claude/CLAUDE.md`, valide in ogni mio progetto) e **`regole_di_progetto`** (`CLAUDE.md` nella repo, solo AI-CV-COACH).
- Creato **`idee_future.md`** come raccolta unica dei raffinamenti futuri, consolidando un backlog prima sparso (handoff, memoria, diario).
- Ripulito l'auto-memoria dai doppioni confluiti nelle regole.
- Definito il comando **"aggiorna-tutto"** con una **tabella "modalità per file"**, e un **marker** per le regole nuove.

**Cosa ho imparato**
- La differenza tra **le mie regole** (come lavoro con l'IA) e le **regole del prodotto** (l'anti-invenzione, che vive nei prompt): vanno tenute separate anche se si somigliano.
- Concetti git che non avevo chiari: **working tree**, file **tracciati/non tracciati**, **merge fast-forward**.
- Che lo **scope** è tutto: regola universale → globale, regola specifica → progetto.

**Dove ho faticato / cosa non era ovvio**
- All'inizio avevo messo tutto in un'unica lista; solo distinguendo lo scope ho sciolto l'inghippo.
- Ho dovuto stanare regole già scritte e sparse (es. la sicurezza nell'eliminare un branch) per non perderle.

**Cosa ho deciso e perché**
- Ogni regola in una sola casa (globale vs progetto), niente duplicati.
- "aggiorna-tutto" lavora sul **working tree** ed **esclude** i file sensibili/gitignored.
- Tengo uno Step anche per il metodo: questo progetto è la mia avventura nello studio dell'IA, e il *come* lavoro ne fa parte.

💡 *Mia intuizione / scelta ragionata* — Separare le mie regole da quelle del prodotto non è pignoleria: è ciò che mi farà riusare lo stesso metodo su ogni progetto futuro senza trascinarmi dietro le specificità di questo. Lo scope è la chiave.

### Step 1.20 — Anello 4: due CV (base e mirato) e lo schema, prima dei prompt

*Dopo lo Step di governance ho aperto l'anello 4, la generazione. Niente codice ancora: ho ragionato l'architettura con l'assistente e fissato lo schema dati, tenendo i prompt per il passo successivo. Un anello delicato, perché è dove il vincolo anti-invenzione rischia di più.*

**Cosa ho fatto**
- Deciso **due percorsi**: **📄 CV-1** (base, generato dal solo profilo dopo l'anello 1) e **🎯 CV-2** (mirato, generato dopo l'anello 3 e orientato all'annuncio). Chi vuole solo un CV si ferma al primo; chi cerca *quel* posto fa il percorso completo.
- Fissato la **fonte di verità**: sempre il **profilo** (anello 1, JSON). Nel CV-2 il CV-1 entra **solo come riferimento di stile**, mai come fonte di fatti.
- Scelto la **forma dell'output**: **JSON a sezioni**, con l'impaginazione lasciata al front-end (impalcatura usa-e-getta).
- Definito le **sezioni** del CV e scritto lo **schema JSON** in `prompt_design.md` (vuoto + esempio + note + regole d'uso), sul principio **campi-fatto ricopiati / campi-prosa generati ma vincolati**.
- Aggiunto la regola di progetto **#9** (marker 📄 CV-1 / 🎯 CV-2 in chat) e annotato in `idee_future.md` il **turno contatti** e il **riordino dinamico delle sezioni**.

**Cosa ho imparato**
- La distinzione **campi-fatto / campi-prosa** è la leva che rende l'anti-invenzione **verificabile**: i fatti si controllano 1:1 col profilo, la prosa resta confinata a due campi.
- Ogni passaggio attraverso l'LLM è un'occasione di **deriva dal vero**: per questo il CV-2 non si fida del CV-1 per i fatti, ma solo per lo stile.

**Dove ho faticato / cosa non era ovvio**
- La tensione della **modifica manuale del CV-1**: se l'utente corregge lì un *fatto*, resta intrappolato nel CV-1 e il CV-2 lo ignorerebbe. L'ho sciolta decidendo che nel CV-1 si rifinisce la **forma**; i **fatti** si correggono nel profilo (anello 1), unica casa della verità.

**Cosa ho deciso e perché**
- **Schema prima dei prompt**, e **prima CV-1, poi CV-2**: il mirato si appoggia al base, progettarlo al buio non avrebbe senso. Testerò il CV-1 sui casi reali **prima** di disegnare il CV-2 (test prima, valutazione dopo).
- **Ordine delle sezioni fisso** nell'MVP: il "mirare" vive nel **contenuto** (cosa il sommario mette in risalto, quanto dettaglio do a un'esperienza), non nel riordino.

💡 *Mia intuizione / scelta ragionata* — Il valore dell'anello 4 sta quasi tutto in **due campi di prosa**: `sommario` e `descrizione`. Tutto il resto del CV è verità ricopiata dal profilo. Sapere *dove* si concentra il rischio mi dice anche dove concentrare l'attenzione quando scriverò i prompt.

### Step 1.21 — Il 📄 CV-1 alla prova del campo, e una convenzione: il server garantisce JSON pulito

*Avevo cablato il 📄 CV-1 ma non l'avevo ancora provato sul serio. Questo Step è prima di tutto una verifica sul campo (test prima, valutazione dopo), e poi un piccolo intervento di robustezza nato da un dubbio che credevo fosse un'incoerenza — e che i dati hanno corretto a metà strada.*

**Cosa ho fatto**
- Fatto girare il 📄 CV-1 su **due profili reali** scelti per stressare i punti deboli: uno **medio** (esperienze formali + informali + competenze + formazione) e uno **scarno** (quasi vuoto). Doppio controllo campo per campo contro il profilo.
- Verificato l'**anti-invenzione**: i campi-fatto (nome, ruolo, azienda, durata, competenze, formazione) ricopiati 1:1, e la **trappola del `cosa_facevo` vuoto** → la `descrizione` è rimasta vuota, niente invenzione. Sul profilo scarno il sommario è rimasto breve, senza gonfiare.
- Introdotto una **convenzione unica — "il server garantisce JSON pulito"**: un helper `inviaJsonModello` su `/struttura` e `/genera-cv` che **valida lato server** (riusa `estraiJson`), ri-serializza pulito se il JSON è valido e risponde **502 col grezzo** se il modello tronca o malforma. Prima i due endpoint restituivano il testo del modello *verbatim* e si affidavano al client per togliere il recinto ` ```json `.
- Annotato in `idee_future.md` un **limite latente condiviso**: `estraiJson` toglie il recinto ma non un eventuale **preambolo in prosa** prima del JSON.

**Cosa ho imparato**
- La distinzione **campi-fatto / campi-prosa** rende l'anti-invenzione *verificabile a colpo d'occhio*: ho controllato i fatti 1:1 e ho concentrato l'attenzione sui soli due campi-prosa. Sul campo ha tenuto.
- **Onestà coi dati, anche contro me stesso**: ero partito convinto che ci fosse un'incoerenza tra gli endpoint (credevo che `/struttura` pulisse il JSON lato server). I fatti mi hanno smentito — `/struttura` e `/genera-cv` erano *entrambi* verbatim; solo `/confronta` parsa lato server, e per necessità (deve calcolare il punteggio). Così ho cambiato la **motivazione** dell'intervento da "coerenza" (falsa) a "robustezza" (vera).

**Dove ho faticato / cosa non era ovvio**
- Separare **coerenza** da **robustezza**: l'intervento non sanava un'incoerenza (non c'era), ma aggiungeva una garanzia utile soprattutto su `/genera-cv`, l'endpoint con output grande e quindi più esposto al troncamento. Ho dovuto correggere il tiro a metà.
- Un **502 su `/confronta`** durante i test mi ha fatto temere una regressione. Diagnosi prima di concludere: era il modello che aggiungeva un *preambolo in prosa* su un mio annuncio **fuori-schema** di test — comportamento pre-esistente, non una mia regressione (quell'handler non l'ho toccato).

**Cosa ho deciso e perché**
- Convenzione **su entrambi** gli endpoint di generazione, non solo su `/genera-cv`: pulirne uno solo l'avrebbe reso il diverso del gruppo. `/confronta` resta com'è perché già parsa per i suoi calcoli.
- Il limite del **preambolo** va in `idee_future`, non sistemato ora: con input ben formati non si presenta, e non voglio allargare lo scope prima di disegnare il 🎯 CV-2.

💡 *Mia intuizione / scelta ragionata* — La lezione vera di questo Step non è tecnica ma di **metodo**: ho seguito i dati anche quando contraddicevano la mia diagnosi iniziale, e l'intervento ne è uscito migliore — non un rattoppo di "coerenza" presunta, ma una garanzia di "robustezza" reale, motivata bene.

### Step 1.22 — Il 🎯 CV-2 mirato: la mira nell'enfasi, mai nell'invenzione

*Chiuso il 📄 CV-1, ho aperto il secondo CV: quello che punta a un annuncio. Qui il rischio anti-invenzione è al massimo, perché "mirare" tenta di stiracchiare i fatti per farli combaciare. Ho ragionato a lungo il design prima di scrivere una riga, poi ho costruito e testato — e il test ha ripagato subito.*

**Cosa ho fatto**
- Ragionato e fissato il **design del 🎯 CV-2** sciogliendo tre bivi: **ingressi** = `profilo` (fatti) + `annuncio` (bersaglio) + `giudizi` dell'anello 3 (segnale di mira), **niente CV-1**; la **mira vive nell'enfasi** (soprattutto nel sommario), non nel riordino; i contenuti off-target si **tengono tutti**, ri-pesati (l'omissione è andata in `idee_future`).
- Sciolto un nodo concettuale: la parola "fonte" nascondeva **due cose** — la **fonte dei fatti** (solo il profilo) e la **fonte della mira** (annuncio + anello 3). Annuncio e giudizi *non* aggiungono nulla al CV: dicono solo *dove puntare i riflettori*.
- Deciso di **non passare il 📄 CV-1** in ingresso: il suo unico ruolo previsto era lo stile, ma lo stile è già nel prompt; darlo in pasto avrebbe solo aggiunto rischio di contaminazione dei fatti. (Rivede la nota dello Step 1.20.)
- Scritto il **prompt del 🎯 CV-2** (`cv_mirato`), identico in `prompt_design.md` e `server.js` (sync char-by-char), e **cablato `/genera-cv`** perché smisti da solo: col solo profilo → 📄 CV-1, con profilo+annuncio+giudizi → 🎯 CV-2 (con 400 se mancano i pezzi del mirato).
- Aggiunto **`test-cv-mirato.html`** come pagina **separata** (su richiesta): due input, esegue anello 3 poi anello 4, mostra match + CV.

**Cosa ho imparato**
- La distinzione **fatti / mira** è ciò che rende il CV mirato difendibile: la mira sposta solo l'enfasi su fatti veri, e il guard-rail più importante è che il CV **taccia sui gap** invece di inventare "competenze trasferibili" per coprirli.
- **Test prima, valutazione dopo, ancora una volta ripagato.** Il primo giro ha scovato **due bug nel prompt**: chiamavo l'esito `"parziale"` invece di `"in parte"` (vocabolario reale dei giudizi) e mi appoggiavo a `importanza`, che è **vuota proprio sui requisiti del nucleo** — il peso lì lo dà `priorita`. Senza il test sul campo sarebbero passati inosservati.

**Dove ho faticato / cosa non era ovvio**
- Guardare *dentro* i `giudizi` invece di assumerne la forma: solo ispezionando l'output reale dell'anello 3 ho visto che `priorita` e `importanza` valgono per gruppi di voci diversi (nucleo vs contesto). Una struttura che credevo uniforme non lo era.

**Cosa ho deciso e perché**
- **Un solo endpoint** `/genera-cv` che smista per ingressi, invece di due rotte separate: la "generazione" è un anello solo, e un endpoint in più sarebbe altra impalcatura da migrare a VB.NET.
- **Due pagine di test distinte** (`test-cv.html` e `test-cv-mirato.html`) invece di una sola con un interruttore: tengono separati i due percorsi (base e mirato) e si leggono più chiare.

💡 *Mia intuizione / scelta ragionata* — Il 🎯 CV-2 ha confermato sul campo l'intuizione dello Step 1.20: tutto il valore (e tutto il rischio) si concentra nel **sommario**. Lì si è vista la mira funzionare — apre con i requisiti `richiesto`+`soddisfatto`, retrocede il resto — *senza* nominare né compensare ciò che mancava. La mira giusta non è dire di più: è scegliere cosa dire per primo, tra le cose vere.

### Step 1.23 — La ✉️ lettera di presentazione: motivata nel tono, fedele nei fatti

*L'ultimo pezzo della generazione. La lettera è il formato dove l'anti-invenzione fa più male — una lettera di presentazione è persuasiva per natura — e proprio per questo è stato il design più delicato. La chiave l'ha data una distinzione netta che ho deciso con l'assistente: l'atteggiamento si può esprimere, i fatti no.*

**Cosa ho fatto**
- Ragionato il design e sciolto il nodo centrale: la lettera **suona motivata** (volontà, interesse, entusiasmo, enfasi sui punti di forza) ma ogni **fatto** viene solo dal profilo. La distinzione operativa è **atteggiamento** (ammesso, è il tono) vs **fatti** (esperienze, competenze, titoli, storie: solo dal profilo, mai inventati).
- Fissato gli **ingressi**: `profilo` (unica fonte di fatti) + `annuncio` (bersaglio) + `giudizi` (mira) + il `🎯 CV-2` come **riferimento di coerenza** (stessa storia del CV), mai come fonte di fatti.
- Scelto l'**output a blocchi** (`apertura`, `corpo`, `chiusura`, `firma`): isola il `corpo`, dove vivono le affermazioni da verificare, dalle formule di cortesia.
- Scritto il **prompt** (`lettera_mirata`), identico in `prompt_design.md` e `server.js` (sync char-by-char), e cablato il nuovo endpoint **`/genera-lettera`**.
- Aggiunto **`test-lettera.html`** come pagina separata: esegue il flusso intero (anello 3 → 🎯 CV-2 → ✉️ lettera).

**Cosa ho imparato**
- La regola **atteggiamento/fatti** è la versione più affilata del principio campi-prosa: la prosa può portare *tono* motivazionale, ma la *sostanza* resta bloccata al profilo. Sul campo ha tenuto: la lettera dice "sono motivata, convinta di poter contribuire" (atteggiamento) e poi solo fatti reali (Conad, cassa, ragioneria, lavoro in squadra).
- Il guard-rail del **tacere sui gap** vale per la lettera ancora più che per il CV: nel test la lettera non ha nominato né l'inglese (non soddisfatto) né la disponibilità weekend, e non li ha compensati inventando.

**Dove ho faticato / cosa non era ovvio**
- Tarare *quanto* entusiasmo concedere senza scivolare nell'invenzione: il confine non è la quantità di calore, ma la sua natura — interesse generico per il ruolo sì, motivazioni biografiche inventate ("ho sempre sognato di…") no.
- Accettare il **CV-2 in ingresso**: ribaltava la scelta fatta per il CV-2 (dove non passavo il CV-1). L'ho accettato perché qui la ragione è la **coerenza** tra lettera e CV, e il CV-2 è già vincolato ai fatti — col paletto esplicito che resta riferimento di stile/coerenza, non fonte di fatti.

**Cosa ho deciso e perché**
- **Endpoint dedicato** `/genera-lettera` invece di estendere `/genera-cv`: l'output è un documento diverso (lettera, non CV), e tenerli separati è più leggibile (anche se entrambi sono impalcatura che non migra a VB.NET).
- **Pagina di test separata** `test-lettera.html`, coerente con la scelta di tenere distinti i percorsi di prova.

💡 *Mia intuizione / scelta ragionata* — La lettera è la prova che l'anti-invenzione non è un freno alla persuasione: si può **proporre con convinzione** restando veri. Il trucco non è inventare entusiasmo su fatti finti, ma mettere calore vero attorno a fatti reali. Con questo si chiude tutta la generazione dell'anello 4: 📄 CV-1, 🎯 CV-2 e ✉️ lettera.

### Step 1.24 — Il flusso unico: tutti gli anelli in un solo dialogo, e una voce-fantasma del confronto

*Il momento in cui i pezzi smettono di essere pezzi. Fino a ieri l'anello 1 viveva in `index.html` e gli anelli 2-3-4 solo nelle `test-*.html`: bancali di prova separati. Qui li ho cuciti in un unico flusso utente reale — e proprio collaudandolo dal vivo, cliccando come farebbe un utente, è saltata fuori una piccola invenzione che gli endpoint da soli non mostravano.*

**Cosa ho fatto**
- Integrato i **quattro anelli in un solo flusso** dentro `index.html`: dialogo del profilo → bivio (📄 CV-1 base / 🎯 miro a un annuncio) → analisi annuncio → confronto in stelle → 🎯 CV-2 mirato → ✉️ lettera. Il server resta senza stato; la memoria del profilo e il flusso del dialogo vivono nel browser.
- **Collaudato end-to-end nel browser**, non solo via endpoint: ho fatto pilotare il flusso cliccando i bottoni, con un profilo di prova e un annuncio costruito apposta per chiedere cose **assenti** dal profilo (inglese, disponibilità weekend), per stressare l'anti-invenzione.
- Trovato e corretto una **voce-fantasma nel confronto**: con `esperienza_richiesta` vuota, l'anello 3 ogni tanto allucinava il sentinel "Nessuna esperienza richiesta". Corretto su **due livelli** (cintura e bretelle): prompt di confronto rinforzato (lista vuota → nessun giudizio, niente segnaposto; sync char-by-char `prompt_design.md`↔`server.js`) + filtro difensivo in `mostraMatch` (il sentinel non si mostra mai all'utente).

**Cosa ho imparato**
- **"Pezzi che passano da soli" ≠ "flusso che funziona".** Gli endpoint erano già verdi via curl; la voce-fantasma l'ho vista solo guidando i click. I difetti stanno nelle giunture, non nei singoli mattoni.
- Il sentinel "Nessuna esperienza richiesta" è **legittimo solo quando è l'annuncio a dichiarare l'assenza** di esperienza; quando la lista è semplicemente vuota, ricrearlo è invenzione. La differenza tra *"dichiarato assente"* e *"non presente"*.
- L'anti-invenzione **tiene a valle**: nel collaudo né il 🎯 CV-2 né la ✉️ lettera hanno millantato inglese o disponibilità weekend.

**Dove ho faticato / cosa non era ovvio**
- Capire che la voce-fantasma è **intermittente** (un'allucinazione dell'LLM), non un difetto deterministico: due chiamate identiche al confronto davano esiti diversi. È questa l'incertezza che mi ha spinto al doppio livello.
- **Isolare dove nasceva**: l'analisi annuncio era sana (`esperienza_richiesta` vuota, "rapporto con il pubblico" giustamente fra le competenze); era il confronto a inventare. L'ho capito solo guardando il JSON reale dei due anelli, non a logica.

**Cosa ho deciso e perché**
- **Correzione a due livelli.** Il **prompt** (asset durevole, migra a VB.NET) per togliere la causa a monte; il **filtro in `index.html`** (impalcatura) come rete deterministica, perché un LLM non garantisce mai il 100% e il sentinel, per l'utente, è comunque rumore già escluso dal punteggio.
- **Flusso unico nel solo `index.html`**, lasciando le `test-*.html` come banchi di prova per-anello: l'integrazione non sostituisce i test isolati, li affianca.

💡 *Mia intuizione / scelta ragionata* — L'MVP è completo **end-to-end**: i quattro anelli sono un solo dialogo nel browser. La lezione che mi porto: la verifica vera non è "gli endpoint rispondono", è "l'utente clicca e arriva in fondo". Il difetto che contava è emerso solo lì, alla giuntura — e si è risolto meglio mettendo una cintura sull'asset durevole *e* le bretelle sull'impalcatura.

### Step 1.25 — Il primo collaudo con un CV vero: un bug nelle competenze e l'anti-invenzione al caso estremo

*Fino a ieri avevo provato la pipeline con profili inventati per l'occasione. Qui ho fatto la cosa più ovvia e più rivelatrice: le ho dato in pasto il mio CV reale. L'ho usato come fonte per compilare tutti i turni dell'anello 1 — uno per turno, rispondendo in linguaggio naturale come farei davvero — e ho percorso l'intera catena fino ai tre output. Bastava usare dati veri per far saltare fuori ciò che i dati finti nascondevano.*

**Cosa ho fatto**
- Ho compilato i cinque turni del profilo (nome, esperienze formali e informali, competenze, formazione) a partire dal mio CV, poi ho generato i tre output dell'anello 4: 📄 CV-1, 🎯 CV-2 e ✉️ lettera.
- Come annuncio-bersaglio per il CV-2 e la lettera ho scelto **di proposito** una posizione lontanissima dal mio profilo — **Operatore Tecnico Subacqueo** in un'azienda di acquacoltura — per stressare l'anti-invenzione proprio dove sarebbe stato più "comodo" gonfiare.

**Cosa ho imparato**
- Il match è uscito **0,1 stelle (2/100)**: corretto, i due profili sono incompatibili. E i tre output non hanno inventato nulla: il 🎯 CV-2 e la ✉️ lettera **dichiarano apertamente** l'assenza di brevetti, certificazioni ed esperienza in mare. L'anti-invenzione regge anche al caso estremo, quello in cui non c'è niente da spendere.
- L'anello 3 ha usato bene il "non determinabile" per i dati che il mio profilo non raccoglie (patente nautica, sede): non li ha dati né per presenti né per assenti.

**Dove ho faticato / cosa non era ovvio**
- Il turno **competenze scartava le qualità personali**: serietà, affidabilità, capacità organizzative, gestione dello stress sparivano. Le leggeva come "modi di essere", non come competenze. La causa era una sola formula nel prompt di estrazione — *"di saper fare"* — che restringeva il campo all'abilità operativa e tagliava fuori i tratti caratteriali.

**Cosa ho deciso e perché**
- Ho **allargato il perimetro** del campo competenze nel prompt di estrazione: ora include esplicitamente abilità pratiche, competenze trasversali **e** qualità personali, con esempi. Correzione **identica** in `server.js` e `prompt_design.md` (sync char-by-char) e **riverificata** sullo stesso input: le qualità ora entrano (da 23 a 31 voci).
- Ho lasciato com'è il **testo visibile** del dialogo ("cosa ti senti di saper fare? Anche cose pratiche e concrete…"): è copy UX, già abbastanza inclusivo (l'esempio mostra "Essere ordinato"). La rifinitura fine dei testi visibili la rimando alle fasi successive.
- Ho **annotato un'idea** in `idee_future.md`: sotto una soglia di match, avvisare prima di generare CV-2 e lettera — per un match ~0 gli output sono onesti ma inutili come candidatura.

💡 *Mia intuizione / scelta ragionata* — La verifica vera non è "passa coi dati di prova", è "passa col mio CV". Una sola formula nel prompt — *"saper fare"* — tagliava fuori metà di ciò che sono come persona, e l'ho vista solo perché ho usato dati reali. I bug più veri non li trovi nei casi che costruisci apposta: li trovi quando metti dentro te stesso.

### Step 1.26 — L'anti-perdita: niente si butta nel turno sbagliato, e il tirocinio resta un tirocinio

*Ancora il mio CV reale, ancora due crepe alle giunture. Nel turno delle esperienze informali avevo descritto il volontariato alla Croce Verde infilandoci dentro anche un concorso vinto e un corso: nel resoconto finale erano spariti. E uno "stage programmato" era stato registrato come un impiego dipendente qualunque. Due sintomi, una radice sola: la tassonomia a turni è pulita per la macchina, ma io non racconto a compartimenti stagni. Così ho aggiunto al dialogo un meccanismo che non perde mai niente, e un modo per dire "questo è un tirocinio" senza spacciarlo per un posto fisso.*

**Cosa ho fatto**
- **Anti-perdita con instradamento (campo `altrove`)**: ogni turno-contenuto (esperienze formali/informali, competenze, formazione) ora, oltre al suo campo, restituisce un campo `altrove` dove finiscono — **verbatim, con le mie parole** — i frammenti che ho accennato in quel turno ma che sono di un'altra categoria. Il front-end li accantona in un magazzino `pending` e me li **ripropone strutturati e da confermare** quando si apre il turno giusto (instradamento *in avanti*) o, se quel turno è già passato, in una **passata finale** prima del riepilogo (instradamento *all'indietro*).
- **Tassonomia condivisa identica** nei quattro prompt di turno (le stesse quattro definizioni di categoria): è il metro unico con cui ogni turno classifica l'overflow. Sync char-by-char `prompt_design.md` ↔ `server.js` verificato a macchina (4/4 OK).
- **Tirocinio esplicito (campo `tipo`)**: `esperienze_formali` ha un sotto-campo opzionale `tipo`, riempito a `"tirocinio"`/`"stage"` **solo se lo dichiaro apertamente** (mai dedotto). Resta nella stessa sezione degli altri lavori, ma l'anello 4 lo rende esplicito nel ruolo del CV ("Stage — …") e non lo presenta come impiego dipendente.
- **Collaudo sui miei casi reali** (server vivo, non solo a logica): lo stage Aviolab → `tipo: "stage"`; la Croce Verde → il volontariato nelle informali **e** il corso + il servizio civile salvati in `altrove.formazione` (niente perso); un cameriere normale → `tipo: ""` (nessun falso tirocinio); il 📄 CV-1 dello stage → ruolo "Stage — Test e sviluppo applicazioni AI".

**Cosa ho imparato**
- **L'anti-perdita è importante quanto l'anti-invenzione.** Finora avevo blindato il "non aggiungere"; questi casi mostrano il rovescio: il "non perdere". Sono due facce della stessa fedeltà ai miei dati.
- Il modo per **azzerare l'errore di instradamento** non è rendere l'LLM infallibile: è togliergli ogni potere decisionale silenzioso. **L'LLM propone, io dispongo**: nessun frammento entra nel profilo senza la mia conferma nel turno di destinazione. È lo stesso patto dell'anti-invenzione, applicato allo spostamento.
- Un **tirocinio non è formazione**: è un'esperienza (formativa, ma esperienza). Va con i lavori, non con i titoli — ma marcato, perché non è un impiego come gli altri.

**Dove ho faticato / cosa non era ovvio**
- Capire che la mia richiesta ("lo reinserisce da solo nel turno giusto, per ogni turno") **non era la versione leggera** che mi era stata proposta (segnalare e basta): era il **parcheggio vero**, con stato che attraversa i turni. Più lavoro, ma è quello che volevo.
- La piega dell'"ogni turno": l'instradamento **in avanti** è facile; quello **all'indietro** (un lavoro citato mentre parlo di formazione, con le esperienze già chiuse) ha richiesto una **passata finale** dedicata.
- Una classificazione **opinabile** osservata nel collaudo: "Vittoria Concorso Servizio Civile 2022" è finita in `formazione`, mentre per me è più un'esperienza. Non l'ho forzata nel prompt: regge la rete di sicurezza (alla formazione la vedo e posso correggerla o scartarla).
- **Il bug che il collaudo ha scovato.** Per provare il flusso a click senza un browser ho montato un **harness headless** che carica il *vero* `<script>` di `index.html` in un mini-DOM e lo pilota contro il server reale. Lì è saltato fuori un difetto che a logica non avevo visto: *"Servizio Civile 2022"* — che nessun turno sa strutturare in una voce — entrava in **ping-pong infinito** fra «esperienze» e «formazione», perché a ogni fallimento veniva **re-instradato altrove** e la passata finale lo ripescava all'infinito.

**Cosa ho deciso e perché**
- **Frammenti verbatim, non strutturati**: il turno che *nota* l'overflow copia solo le mie parole e le classifica; a strutturarle sarà il turno di destinazione col suo prompt. Così l'unica decisione dell'LLM è "di che categoria è?", e nessun prompt deve conoscere lo schema degli altri.
- **`tipo` nel profilo (schema durevole), non nello schema d'uscita del CV**: il segnale macchina-leggibile vive nel profilo; l'anello 4 lo legge e lo rende nel ruolo. Così non ho toccato lo schema del CV, né `mostraCv`, né le `test-cv*.html`: raggio d'impatto minimo.
- **Blocco `altrove` identico al 100%** in tutti i turni (la regola è "ciò che è di questo turno → campo principale; il resto → altrove"): sync banale e un solo metro di classificazione.
- **Terminazione per costruzione (dopo il collaudo)**: nello smaltimento un frammento è **consumato una volta sola** e non rientra mai in `pending`; ciò che non si struttura **non rimbalza**, lo dichiaro **"lasciato fuori"**. Ho scelto *terminazione garantita + perdita visibile* invece di *re-instradamento perfetto*: per un contenuto di confine come il servizio civile l'unico modo per non avvitarsi è fermarsi e dirlo. Ri-collaudato: converge fino alla generazione, 6/6 verdi (stage con `tipo`, corso in avanti, magazziniere all'indietro, `pending` svuotato). La scelta più ricca — far collocare all'utente gli "esclusi" — è annotata in `idee_future`.

💡 *Mia intuizione / scelta ragionata* — Il filo che tiene insieme tutto il prodotto è la **fedeltà ai miei dati**, e ha due nemici simmetrici: aggiungere ciò che non ho detto (invenzione) e perdere ciò che ho detto (smarrimento). Finora avevo guardato solo il primo. La lezione di questo step è che la stessa rete — *l'LLM propone, io confermo* — li ferma entrambi: non lascia entrare il falso e non lascia uscire il vero.

### Step 1.27 — Il disegno top-down: mettere per iscritto l'architettura nata di fatto

*Il mio tutor ha osservato una cosa giusta: ho costruito CV-COACH dal basso — prima i prompt, gli schemi, gli anelli; l'architettura è emersa strada facendo, mai disegnata. Mi ha dato una traccia strutturale (introduzione, funzioni fondamentali, emissione documenti) e mi ha chiesto di svilupparla in un documento completo, per poi, in un secondo tempo, riallineare il progetto a quel disegno. Questo step è il primo passo: il documento, non ancora il riallineamento del codice.*

**Cosa ho fatto**
- **Nuovo file `architettura.md`**: ho sviluppato la traccia del tutor in un disegno top-down completo. Per ogni funzione una griglia fissa — *cosa fa · cosa entra → cosa esce · dove vive oggi · stato* — con la mappa esplicita fra il mio vocabolario (**anelli 1-4**) e il suo (**voci 2.x**).
- **Le funzioni e i loro buchi**: ho mappato dove ogni voce vive nel progetto e ho marcato i **tre gap** — la **mitigazione (2.2.4)**, resa componente esplicito tra anello 3 e anello 4, e le due fonti di profilo mancanti (**2.1.2** estrazione da CV preesistente, **2.1.3** da LinkedIn/web).
- **Quattro viste che la traccia funzionale non copriva**: una **vista-dati** ("un profilo, molti CV": il profilo come hub disaccoppiante da cui tutto si dirama), i **principi trasversali** (JSON come scambio, compito ristretto, architettura ibrida, due modelli, le due bussole etiche, normalizzazione leggera), una **vista runtime** (front-end ↔ aiutante Node ↔ LLM, gli endpoint come confini) e una **vista evolutiva** (cosa migra verso VB.NET — prompt+schema — vs cosa è impalcatura).
- **Niente duplicati**: il documento **rimanda** a `prompt_design.md` per prompt e schemi, a `README`/`diario` per lo stato, a `idee_future.md` per il backlog — non li ricopia. Verificato a macchina che `/struttura` serve davvero sia l'anello 1 sia l'anello 2 (così l'avevo scritto). Aggiunta la riga di `architettura.md` alla tabella di `CLAUDE.md` con modalità **statico-strutturale**.

**Cosa ho imparato**
- **Una scomposizione per funzioni è una sola vista, non l'architettura intera.** La traccia del tutor è ottima come asse funzionale (i *verbi*: estrai, confronta, genera), ma da sola non bastava: mancava la vista dei *sostantivi* — gli artefatti dati che fluiscono. Per questo progetto i dati **sono** l'architettura.
- **Disegnare dopo aver costruito ha un vantaggio**: il disegno non è un'ipotesi, è la fotografia di scelte già validate sul campo. Scrivendolo ho dovuto dare un nome a cose che facevo senza nominarle — "un profilo, molti CV", "fonte di fatti vs segnale di mira", "l'LLM comprende, il codice rende consistente".

**Dove ho faticato / cosa non era ovvio**
- Decidere **quanto** aggiungere alla traccia senza gonfiarla: è un MVP di tirocinio, non serve un trattato di architettura. Ho scelto la proporzione — vista-dati e principi come sezioni piene (sono il cuore), runtime ed evolutiva come sezioni brevi.
- Il rischio del **documento-bussola stantio**: un'architettura disallineata dal codice è peggio di nessuna. Da qui la scelta sulla modalità di aggiornamento (sotto).

**Cosa ho deciso e perché**
- **`architettura.md` indipendente dai prompt e dallo stato**: vive al livello del disegno e punta agli altri file, così non invecchia a ogni step e non duplica nulla (regola di progetto #4).
- **Incluso in "aggiorna-tutto", ma in modalità *statico-strutturale***: avevo pensato di lasciarlo fuori (è anche una bozza per il tutor, con vita propria), ma escluderlo rischiava di farlo restare indietro proprio quando i gap si chiuderanno (Fase C, ❌ → ✅). Soluzione: dentro l'inventario, ma toccato **solo quando cambia il disegno**, mai per lo stato corrente. Stessa logica conservativa di `research_notes.md`.
- **Il documento prima del codice**: niente modifiche a `server.js` o `prompt_design.md` finché il disegno non è approvato (anche dal tutor). Inverto il metodo bottom-up usato finora — il codice seguirà l'architettura, non il contrario.

💡 *Mia intuizione / scelta ragionata* — Costruire dal basso mi ha dato un sistema che **funziona**; disegnare dall'alto mi dà un sistema che **so spiegare**. Non sono in conflitto: il bottom-up ha trovato le soluzioni, il top-down ne rivela la forma e dove mancano pezzi. Il documento non cambia una riga di codice, ma cambia cosa vedo quando lo guardo — ed è da lì che parte il lavoro che resta.

### Step 1.28 — La mitigazione (2.2.4): nominare un gap senza mentire

*Con l'architettura approvata dal tutor, ho aperto la Fase B — chiudere i gap del disegno — partendo dal più abbordabile: la **mitigazione**. È il componente fra il confronto (anello 3) e la generazione (anello 4) che, dati i requisiti che non possiedo, cerca nel mio profilo qualcosa di funzionalmente affine e ne costruisce l'argomento (il classico "non sono laureato, ma ho una lunga esperienza sul campo"). Questo step è progettazione: prompt e schema, non ancora codice.*

**Cosa ho fatto**
- **Progettato il produttore** in `prompt_design.md`: nuovo artefatto `mitigazioni` (lista di `{ requisito_gap, categoria, esito_origine, elemento_profilo, ponte }`) + il prompt che lo genera, su **Sonnet** (serve cogliere equivalenze funzionali, come nel confronto). Ingressi: profilo + i giudizi dell'anello 3; lavora **solo** sui gap reali (`non soddisfatto` / `in parte`).
- **Connessa la mitigazione alla ✉️ lettera**: aggiunto il 5° blocco `<mitigazioni>` al prompt della lettera e riscritta la regola sui gap — da «la lettera tace sui gap» a «**tace sui gap non mitigabili; usa le mitigazioni fornite per nominare onestamente un gap e il suo ponte**».
- **Aggiorna-tutto**: propagato il lavoro a `architettura.md` (decisione di design: le mitigazioni le consuma la sola lettera), `README` (Stato), `idee_future.md` (gap 2.2.4 spuntato come progettato), e questa pagina.

**Cosa ho imparato**
- La mitigazione è il **gemello onesto** dell'anti-invenzione: l'anti-invenzione vieta di aggiungere ciò che non ho; la mitigazione mi lascia *valorizzare* ciò che ho di affine, ma a una condizione ferrea — **non nascondere mai l'assenza** del requisito. "Non ho X, ma ho Y" è onesto; "ho X" sarebbe una bugia.
- **Tacere è una risposta valida.** Se per un gap non c'è nel profilo nulla di davvero affine, il componente non produce niente: meglio nessun argomento che uno forzato. L'ho codificato come comportamento atteso (lista vuota ammessa), non come fallimento.

**Dove ho faticato / cosa non era ovvio**
- La **tensione con una regola esistente**: il 🎯 CV-2 e la lettera "tacciono sui gap". La mitigazione invece il gap lo *nomina*. L'ho sciolta separando i due documenti — il bridging ha senso retorico nella **lettera**, non nel CV: così la lettera consuma le mitigazioni, il CV-2 resta sobrio (decisione A).
- La **sincronizzazione prompt↔codice** (regola di progetto #1): cambiando il prompt della lettera ho creato una divergenza con `server.js`. Cablarla subito avrebbe **rotto** l'anello 4 funzionante (le mitigazioni non esistono finché non c'è il loro cablaggio). Ho scelto di non toccare il codice e di **documentare** la divergenza come differita alla Fase C, invece di nasconderla.

**Cosa ho deciso e perché**
- **Le mitigazioni le consuma la sola lettera** (CV-2 sobrio): il CV resta un documento di fatti; la lettera è il luogo dove un argomento "non ho X, ma ho Y" suona naturale e onesto.
- **Incluso anche `altri_requisiti`** (patente, automunito, domicilio…), ma con l'onestà come paletto: se non ho un requisito lo dico, e porto un dato affine solo se è davvero nel profilo. Niente affinità spacciata per possesso.
- **Materia prima, non prosa pronta**: il componente fornisce il *nesso logico*, non la frase finita — la prosa resta compito dell'anello 4 (principio del compito ristretto).
- **Progettazione prima del codice**: coerente con la Fase A, ho fissato prompt e schema; il cablaggio (endpoint + lettera a 5 blocchi) sarà la Fase C.

💡 *Mia intuizione / scelta ragionata* — La mitigazione mi ha mostrato che l'onestà non è solo "non aggiungere": è anche **non sottrarre**. Il sistema sa già non inventare ciò che non ho; ora sa anche dichiarare apertamente ciò che mi manca e, accanto, ciò che ho di vicino. È più difficile da scrivere di una bugia — e vale esattamente per questo.

### Step 1.29 — La mitigazione al lavoro: cablaggio, prova sul campo e tre fix di onestà

*Avevo progettato la mitigazione (Step 1.28) lasciando il cablaggio "a dopo". Ho deciso invece di chiuderlo subito: scrivere il codice, farlo girare davvero e vedere cosa produce. È andata come dovrebbe andare — il primo test ha mostrato difetti che a tavolino non avevo visto, e li ho corretti.*

**Cosa ho fatto**
- **Cablaggio (Fase C)**: nuovo endpoint dedicato **`/mitiga`** in `server.js` (input profilo + giudizi → `mitigazioni`, su Sonnet), lettera portata a **cinque blocchi** col blocco `<mitigazioni>`, e `index.html` che chiama `/mitiga` prima della lettera. Prompt **identici** fra `prompt_design.md` e `server.js`, verificato char-by-char con uno script che neutralizza i segnaposto `${JSON.stringify}`.
- **Prova sul campo** (test prima, valutazione dopo): profilo magazziniere contro un annuncio di logistica con gap voluti (SAP, diploma di ragioneria, patentino muletto, patente). Pipeline reale `/confronta → /mitiga → /genera-lettera`.
- **Tre fix dopo il test**: (1) **tace** quando l'affinità è debole invece di produrre una voce che si auto-confuta; (2) **niente speculazione** sul possesso ("forse il patentino ce l'ha ma non l'ha scritto"); (3) **esclude il `contesto`** (mansioni, sede…), che non è una lacuna del candidato.

**Cosa ho imparato**
- **Il sistema vero insegna più del ragionamento a tavolino.** Avevo scritto "tacere è corretto", ma alla prova il modello riempiva *ogni* gap, usando il campo `ponte` per spiegare che il ponte non c'era. Il difetto non si vedeva sulla carta: si è visto solo facendolo girare. Dopo il fix, le mitigazioni sono scese da 5 a 3 e l'unico appiglio debole (Excel) è giustamente sparito.
- **Un LLM tende a riempire.** "Non produrre niente" è un comportamento che va **insegnato esplicitamente** e con una soglia alta ("regge a un colloquio o mi arrampico sugli specchi?"), altrimenti il modello preferisce sempre dire qualcosa.

**Dove ho faticato / cosa non era ovvio**
- La **sincronizzazione char-by-char** prompt↔codice: i due testi sono identici tranne i punti d'inserimento dei JSON. Ho scritto una verifica che li normalizza, così "identico" è controllabile a macchina e non a occhio.
- Il **confine fra ponte onesto e ponte forzato**: è un giudizio di grado, non una regola secca. La soglia alta + il divieto di voci auto-confutanti lo rendono governabile, ma il caso limite (il diploma di ragioneria "coperto" dal rigore numerico del magazzino) resta un giudizio fine.

**Cosa ho deciso e perché**
- **Endpoint dedicato `/mitiga`** (non estendere `/confronta`): un confine per compito, testabile da solo, e `/confronta` non paga la mitigazione quando non serve.
- **Mitigazione pigra nel front-end**: si calcola solo se l'utente vuole la lettera, e se fallisce la lettera si fa comunque (tace sui gap). Non blocco mai la generazione per un componente accessorio.
- **Soglia alta e onestà cablata nel prompt**: meglio una lista vuota che un argomento che non regge; e mai trasformare un'assenza in un "forse ce l'ha".

💡 *Mia intuizione / scelta ragionata* — La parte di valore non è stata scrivere il prompt, ma **guardarlo sbagliare e correggerlo sui dati**. La prima versione sembrava perfetta finché non l'ho vista all'opera: lì ho capito che "tacere" non era stato insegnato abbastanza forte. Il test non ha confermato il mio lavoro — l'ha migliorato. È la differenza tra "credo che funzioni" e "ho visto cosa fa".

### Step 1.30 — Contatti e patente: i recapiti nel CV, e la patente che entra nel match

*Due dati mancavano da sempre: i recapiti dell'utente (il CV usciva senza email né telefono) e la patente, che il confronto liquidava come `non determinabile` perché non la raccoglievamo — pur essendo spesso un paletto decisivo. Ho deciso di chiuderli insieme, in tre fasi, perché vivono nello stesso turno ma seguono regole opposte: i contatti non si confrontano mai, la patente sì.*

**Cosa ho fatto**
- **Fase 1 — raccolta**: un nuovo turno `contatti` nell'anello 1 (recapiti: email, telefono, città, link) e un campo dedicato `patente: { ha, categorie }` nel profilo. La domanda chiede la patente **esplicitamente** (mai dedotta); se l'utente dichiara di averla ma non dice la categoria, una **ri-domanda** la chiede una seconda volta — e raccoglie tutte le categorie, perché se ne può avere più d'una.
- **Fase 2 — confronto**: ho reso la patente **confrontabile** nell'anello 3. Ora esce da `non determinabile` e si giudica: `ha:"sì"` + categoria richiesta presente → soddisfatto; categoria chiesta ma assente → non soddisfatto; `ha:"no"` → non soddisfatto; non dichiarata → resta `non determinabile`. I contatti, al contrario, ho istruito il prompt a **non confrontarli mai**: sono recapiti, non requisiti.
- **Fase 3 — generazione**: recapiti e patente sono entrati nell'intestazione di 📄 CV-1 e 🎯 CV-2 (la patente solo se posseduta) e nella firma della ✉️ lettera (nome + email + telefono). Sono **campi-fatto**, ricopiati dal profilo come già il nome — non li scrive l'LLM di testa sua.
- **Verifica senza browser**: per provare il front-end non potevo installare Chromium (mancavano librerie di sistema). Ho scritto uno **shim DOM minimale** in Node — poche decine di righe — che monta lo `<script>` vero di `index.html` e gli dà un `document` finto, con `fetch` puntato al server reale. Così ho guidato l'intero dialogo fino a CV e lettera e ho verificato i comportamenti nuovi (9 asserzioni verdi), senza un browser e senza dipendenze.

**Cosa ho imparato**
- **Stesso turno, regole opposte.** Contatti e patente arrivano insieme, ma uno è un recapito (mai giudicato) e l'altro un requisito (giudicato). Tenerli separati fin dallo schema (`contatti` vs `patente`) ha reso il resto lineare: il confronto sa cosa ignorare e cosa pesare.
- **Meglio prevenire il caso-limite che gestirlo.** Sul "patente posseduta ma categoria ignota" stavo per inventare una regola di confronto; la scelta giusta è stata **chiudere il buco a monte** con la ri-domanda, così il caso quasi non si presenta (e se resta, è `in parte`, onesto).
- **Non serve un browser per testare la logica di un front-end.** La parte fragile non è il CSS, è il *flusso*: turni, conferme, la ri-domanda condizionale, la costruzione del DOM. Uno shim leggero la esercita tutta, contro il server vero, in pochi secondi.

**Dove ho faticato / cosa non era ovvio**
- Il **doppio uso della città**: è un recapito (intestazione) ma anche un potenziale dato di match (domicilio). Per ora la raccolgo come contatto e **non** la confronto — il "domicilio confrontabile" resta un'idea futura, per non aprire la questione sensibilità dei dati personali adesso.
- La **sincronizzazione** dei tanti prompt toccati (confronto, due CV, lettera) fra `prompt_design.md` e `server.js`: ho esteso lo script di verifica char-by-char alle righe nuove, distinguendo le parti-prompt (sincronizzate) dalle parti di sola documentazione del `.md` (lo schema-esempio, che in `server.js` non esiste).

**Cosa ho deciso e perché**
- **Patente solo confrontabile, non squalificante.** In questo giro la patente entra nel match col suo peso di priorità; l'**hard-gate** (un requisito che cratera il punteggio) resta un'idea futura separata: una cosa alla volta.
- **Recapiti come campi-fatto nello schema**, ricopiati dall'LLM, non composti dal front-end. Così l'output JSON di CV e lettera è autosufficiente e migrerà pulito a VB.NET, coerente con il principio "asset durevoli = prompt + schema".
- **Patente nel CV solo se posseduta**: un "Patente: no" in un CV è rumore. Se `ha` non è "sì", il campo resta vuoto e il front-end lo omette — stessa logica delle sezioni vuote.

💡 *Mia intuizione / scelta ragionata* — Il momento chiave è stato quando, sul caso "categoria ignota", invece di chiedermi "che voto dargli nel confronto?" mi sono chiesto "perché quel dato manca?". Spostare il problema **dalla valutazione alla raccolta** ha eliminato il caso-limite invece di gestirlo: una ri-domanda in più nel dialogo vale più di una regola fine nel match. È la patente, ma è anche il primo mattone del "profilo a specchio degli `altri_requisiti`": il dialogo che si allunga di un passo per rendere confrontabile ciò che prima si perdeva.

### Step 1.31 — La prova in mano all'utente: una domanda alla volta, e il silenzio che vale «no»

*Avevo "verificato" io il turno contatti+patente col mio shim e dato per chiuso lo Step 1.30. Poi l'ho fatto provare a Mirco nel browser, ed è bastato un giro per far emergere due cose che il mio test non coglieva: una domanda che chiede due cose insieme confonde, e una patente "non indicata" che poi viene ignorata nel match è un buco. Due correzioni, dalla mano di chi usa il dialogo, non da chi lo scrive.*

**Cosa ho fatto**
- **Una domanda, una cosa**: ho separato il vecchio turno unico in **due turni distinti** — prima i `contatti` (recapiti), poi la `patente` con una domanda dedicata. Ho spezzato anche l'estrazione: `PROMPTS.contatti` (solo recapiti) e un nuovo `PROMPTS.patente` (possesso + categorie), identici fra `prompt_design.md` e `server.js`.
- **Il silenzio confermato vale «no»**: se la scheda della patente mostra "non indicata" e l'utente **conferma senza correggere**, il programma fissa `ha:"no"`. Da lì la patente è trattata come **non posseduta** (nel match: `non soddisfatto`, non più `non determinabile`).
- **Verifica a due run** (di nuovo con lo shim DOM, senza browser): un profilo con patente B (turno separato → CV e lettera) e uno che non la dichiara (default `no` → match `non soddisfatto`). Dodici asserzioni verdi; due rosse erano bug delle mie asserzioni, non del codice.

**Cosa ho imparato**
- **Il test dell'autore non sostituisce il test dell'utente.** Il mio shim verificava che il codice facesse quello che *avevo scritto*; Mirco, usandolo, ha visto che quello che avevo scritto non era quello che *serviva*. Sono due livelli diversi di "funziona".
- **Il silenzio a una domanda esplicita è una risposta.** Una cosa è un dato *mai chiesto* (`non determinabile`, "non avevo modo di saperlo"); un'altra è un dato *chiesto e non dato* dopo aver visto la scheda: lì il silenzio confermato è un «no». La distinzione vive tutta nella **conferma**, non nell'estrazione.

**Dove ho faticato / cosa non era ovvio**
- L'**anti-perdita con due turni**: se uno nomina la patente mentre dà i contatti, l'estrazione contatti non la cattura. Non è una perdita, però: la domanda **immediatamente successiva** è proprio la patente, quindi gliela si richiede comunque. È il flusso stesso a fare da rete, non un instradamento `altrove`.
- **Dove mettere il default «no»**: la tentazione era farlo decidere all'estrazione. Ho tenuto l'estrazione **onesta** (`""` se l'utente non si pronuncia) e ho messo l'interpretazione nel **front-end, alla conferma**: così la scheda mostra "non indicata" (correggibile) e solo il "procedi" la trasforma in "no".

**Cosa ho deciso e perché**
- **Due prompt, due turni**: un compito per prompt e una domanda per turno. Più chiaro per l'utente, e ogni prompt resta corto e mirato (meno spazio per sbagliare).
- **Default `no` alla conferma, non all'estrazione**: separa il *fatto* (cosa ha detto l'utente) dall'*interpretazione* (cosa ne deduco se conferma il silenzio). Il fatto resta verificabile, l'interpretazione è esplicita e reversibile.

💡 *Mia intuizione / scelta ragionata* — Lo Step 1.29 me l'aveva già detto con la mitigazione: *guardare il sistema all'opera insegna più del ragionamento a tavolino*. Qui la lezione è salita di un piano — non basta che lo guardi girare **io**, deve guardarlo **chi lo userà**. La mia verifica era corretta e inutile insieme: confermava il disegno giusto del problema sbagliato. Le due correzioni migliori di oggi non sono uscite dal codice, ma da Mirco che digitava nel browser.

### Step 1.32 — Il domicilio nei contatti, e una disponibilità che per ora resta fuori

*Sistemati contatti e patente, ho deciso quale altro dato dell'anello-1 valeva la pena raccogliere subito. Dalla lista del "profilo a specchio degli `altri_requisiti`" ho preso il **domicilio** (utile e poco invasivo) e ho lasciato fuori la **disponibilità** (turni, trasferte): un campo alla volta, solo quelli che servono davvero ora.*

**Cosa ho fatto**
- **Domicilio nel turno contatti**: invece di aggiungere un campo nuovo, ho riusato **lo stesso campo** che già raccoglieva la città (`citta`), cambiando la **domanda** e l'**etichetta** da "città" a "**domicilio**" (l'utente ci mette anche la città). Resta un **recapito non confrontato**, come gli altri contatti: alimenta l'intestazione del CV, l'anello 3 non lo giudica.
- **Disponibilità: fuori per ora.** L'ho lasciata esplicitamente nel backlog (`idee_future.md`), non raccolta.
- Aggiornati prompt (`contatti`, identico fra `prompt_design.md` e `server.js`), testo visibile e scheda in `index.html`; verificato con `node --check`, sync ≡ 13/13 e un test reale di `/struttura` ("Abito in via Roma 5, Genova…" → `citta` = il domicilio).

**Cosa ho imparato**
- **Estendere il profilo non vuol dire aggiungere campi.** Il domicilio è "la città vista come indirizzo": lo stesso campo, una domanda diversa. Riusare il contenitore evita di gonfiare lo schema per una distinzione che all'uso non serve.
- **Recapito ≠ requisito.** Il domicilio *potrebbe* diventare confrontabile (vicinanza alla sede), ma oggi lo tengo come semplice recapito: non tutto ciò che si raccoglie deve entrare nel match. Tenere separati i due usi mantiene l'invariante "i contatti non si confrontano".

**Dove ho faticato / cosa non era ovvio**
- Il **doppio uso** della città/domicilio: è la stessa informazione (dove vivi), ma serve a due cose diverse (recapito in intestazione vs potenziale requisito di zona). Ho scelto di servirne **una sola** ora, segnando l'altra come futura, per non aprire la questione sensibilità dei dati prima del tempo.

**Cosa ho deciso e perché**
- **Stesso campo, domanda diversa**: niente campo nuovo, chiave `citta` invariata, domanda ed etichetta in "domicilio". Minimo cambiamento, massima chiarezza per chi risponde.
- **Domicilio = recapito, non confrontabile (per ora); disponibilità non raccolta**: scelgo cosa entra nel profilo per **valore concreto subito**, non per completezza teorica della lista `altri_requisiti`.

💡 *Mia intuizione / scelta ragionata* — La tentazione, davanti a una lista (domicilio, disponibilità, automunito, età…), è raccoglierla tutta "per completezza". Ho fatto il contrario: un dato perché serve adesso (il domicilio, in intestazione), uno fuori perché ora non serve (la disponibilità). Lo schema cresce per bisogno reale, non per simmetria con l'elenco dei requisiti.

### Step 1.33 — Importare un CV in PDF: Claude lo legge, il profilo resta lo stesso

*Fin qui il profilo si costruiva in un solo modo: il dialogo. Ma chi ha già un CV in PDF non ha voglia di rifare tutto a voce — gli basterebbe trascinarlo nell'app. È la voce 2.1.2 del disegno top-down (una fonte alternativa dello stesso profilo). L'ho chiusa, ma la scelta vera non è stata "come gestire il PDF": è stata capire che il PDF non deve toccare il cuore del sistema.*

**Cosa ho fatto**
- **Due passi separati, un compito per prompt.** Passo 1: Claude **legge il PDF** (input `document` dell'API) e ne **trascrive fedelmente** il testo — endpoint dedicato `/leggi-pdf`. Passo 2: un prompt nuovo, `importa_cv`, prende quel **testo** e lo struttura nell'**intero profilo JSON** (stesso schema dell'anello 1) — passa dal solito `/struttura`, con un turno in più. I due prompt vivono identici in `prompt_design.md` e `server.js` (sync char-by-char verificata).
- **Integrazione nel front-end**: all'avvio ora c'è un **bivio** — «costruiamolo insieme» (il dialogo di sempre) oppure «ho già un CV in PDF». Se importi, il profilo estratto ti viene **mostrato per conferma** prima di procedere; da lì in poi confronto, CV e lettera sono quelli di sempre. Banco di prova a parte (`test-cv-import.html`), come per gli altri anelli.
- **Verifica**: `node --check`, sync dei due prompt (identici al carattere), e una prova reale di `importa_cv` su un CV di test — nome, contatti, patente B, un tirocinio marcato come tale, competenze e formazione tutti al posto giusto; e "automunito", che nel mio schema non ha un campo, **non** inventato da nessuna parte. Poi il collaudo end-to-end nel browser.

**Cosa ho imparato**
- **Separare "leggere" da "strutturare" ripaga.** La trascrizione e la strutturazione sono due mestieri diversi: tenerli in due prompt distinti dà due difese anti-invenzione separate e, soprattutto, lascia il passo 2 (`testo → profilo`) **identico** a come sarebbe con un testo incollato a mano. È l'asset durevole, e migra a VB.NET senza sapere nulla del PDF.
- **Il vincolo "niente dipendenze" riguarda il server, non il problema.** Mi ero bloccato sull'idea che leggere un PDF volesse dire aggiungere una libreria all'aiutante Node. La via d'uscita è stata far leggere il PDF **a Claude** (lo fa nativamente via API): il server resta a zero dipendenze e il confine "PDF → testo" è un pezzo isolato e sostituibile.

**Dove ho faticato / cosa non era ovvio**
- **Dove mettere la lettura del PDF.** Tre strade — parsing nel server (romperebbe "niente dipendenze"), parsing nel browser (una libreria front-end), o il PDF direttamente a Claude. Ho scelto la terza perché tiene il server pulito e il pezzo durevole invariato; le altre restano possibili domani.
- **Il profilo intero è più lungo di un frammento.** I turni di dialogo restituiscono un pezzetto; `importa_cv` restituisce **tutto** il profilo in un colpo, e con i 1500 token di default rischiava di troncarsi. Ho dato a questo turno un tetto più alto, lasciando invariati gli altri.

**Cosa ho deciso e perché**
- **Claude legge il PDF, in due passi** (trascrizione + strutturazione), non un unico prompt "PDF → profilo": un compito per prompt, e il passo durevole resta indipendente dalla fonte.
- **Haiku per entrambi i passi**: è estrazione, non ragionamento profondo. Se un CV reale multi-colonna dovesse uscire sporco nella trascrizione, salirò il **solo** passo 1 a Sonnet — decisione da prendere sui dati, non a priori.
- **Conferma umana dopo l'import**: il profilo estratto si mostra e si fa confermare (l'AI propone, l'utente dispone). L'editing campo-per-campo resta un'idea futura.
- **Ripiego onesto per i PDF-immagine**: una scansione senza testo dà poco o niente; invece di far finta di niente, l'app lo dice e offre l'incolla-testo. OCR e lettura multimodale del PDF restano nel backlog.

💡 *Mia intuizione / scelta ragionata* — Il momento chiave non è stato tecnico. Davanti al PDF la domanda ovvia era «come lo leggo?»; quella giusta era «dove lo faccio entrare, senza che tocchi il resto?». La risposta era già nell'architettura: il profilo è l'**hub disaccoppiante**, e finché una nuova fonte produce *lo stesso* profilo, confronto e generazione non se ne accorgono. Così ho aggiunto un intero modo di costruire il profilo cambiando, a valle, esattamente zero. Ho lavorato sul prompt (che dura), non sul PDF (che è impalcatura).

### Step 1.34 — L'annuncio da un link: web_fetch prova e scarta, la strada è WebView2

*Dopo l'import da CV mi è venuta la voglia gemella: invece di incollare il testo dell'annuncio, incollarne il link. Sembrava una feature gratis — e invece è stata una lezione su cosa `web_fetch` può e non può fare. L'ho costruita, provata sul campo, e poi rimossa: la casa giusta è un'altra, ed è nel futuro VB.NET.*

**Cosa ho fatto**
- **Stesso ragionamento del PDF (2.1.2), applicato all'annuncio.** Il link è solo una fonte diversa dello *stesso* testo dell'annuncio: due passi (`/leggi-link` → `analisi_annuncio` **invariato**), con lo strumento **`web_fetch`** dell'API a fare da lettore — così l'aiutante Node resta a zero dipendenze e non tocca URL arbitrari. Bivio nel front-end (📋 testo / 🔗 link), banco dedicato, ripiego onesto (`NESSUN_ANNUNCIO` → incolla-testo).
- **Verificato prima di cantare vittoria**: `node --check`, sync char-by-char del prompt nuovo, smoke test di routing/validazione a costo zero, poi la prova vera nel browser.
- **La prova ha parlato**: nella pratica gli annunci veri stanno su portali in JavaScript (LinkedIn, Indeed, Infojobs), dove `web_fetch` **non arriva** (prende l'HTML grezzo, che lì è quasi vuoto); le pagine a HTML statico che funzionano sono rare. Feature **del tutto inutile** nell'MVP.
- **Rimosso tutto** (ripristino pulito di `server.js`, `index.html`, `prompt_design.md`; banco eliminato); tenuta **solo** la pista **WebView2** in `idee_future.md`.

**Cosa ho imparato**
- **Il muro vero non è il login: è il JavaScript.** `web_fetch` legge l'HTML grezzo, che sui portali moderni è quasi vuoto — il contenuto lo costruisce il browser. Anche superando login e anti-bot, senza JS-rendering non c'è nulla da leggere.
- **La domanda giusta è "chi accede".** Un bot che scarica una pagina → muro (tecnico e ToS). Io, loggato, che guardo l'annuncio nel mio browser → nessun muro: sto solo leggendo ciò che ho già davanti. È la stessa idea dell'incolla-testo, portata un passo più in là.

**Dove ho faticato / cosa non era ovvio**
- **Sembrava "gratis".** L'istinto diceva: basta incollare un link. Solo provandola ho visto che senza JS-rendering è inutile sui siti che contano. Meglio scoperto sul campo che a tavolino — i dati battono l'ipotesi.

**Cosa ho deciso e perché**
- **Non spedire ciò che i dati dicono inutile.** Rimossa dall'MVP invece di lasciarla come bottone che quasi sempre fallisce: onestà verso l'utente e MVP snello.
- **La fonte-link si fa bene in VB.NET, con WebView2.** Un browser Edge/Chromium nativo in cui navigo e mi loggo **come me**; l'app legge il **DOM già renderizzato** — JS risolto (è un browser vero), muro anti-bot aggirato (sessione mia), e a valle `analisi_annuncio` **invariato** (stessa architettura). Abilita anche la 2.1.3 (profilo da LinkedIn). Annotata in `idee_future.md`.

💡 *Mia intuizione / scelta ragionata* — Come per il PDF, la domanda non era «come leggo il link?» ma «dove faccio entrare questa fonte, e con quale lettore?». Stavolta la risposta è stata anche un **no**: `web_fetch` non è il lettore giusto perché non vede il JavaScript. Il lettore giusto è il **browser dell'utente** — e quello arriva in VB.NET. Aver lavorato sul disegno (fonte → stesso testo → `analisi_annuncio` invariato) fa sì che il «no» di oggi non butti via nulla: il giorno del WebView2, a valle, cambia zero.

### Step 1.35 — Due buchi chiusi: la soglia che sconsiglia, e il JSON che sopravvive alla prosa

*Dopo l'annuncio-da-link (scartato) avevo voglia di rientrare in carreggiata chiudendo due debolezze che avevo già visto sul campo, non idee nuove. La prima l'avevo incontrata al collaudo con il mio CV contro un annuncio lontanissimo (Operatore Subacqueo, 0,1 stelle): il 🎯 CV-2 e la ✉️ lettera uscivano onesti ma inutili come candidatura. La seconda l'avevo vista quando il modello, su un annuncio fuori-schema, «si metteva a spiegare» prima del JSON e l'endpoint crollava con un 502. Due buchi piccoli, ma di quelli che sporcano l'esperienza vera.*

**Cosa ho fatto**
- **Soglia di match prima di generare (B).** Nel front-end, subito dopo il confronto (anello 3), se il match è **sotto 1,5 stelle su 5** non spingo più la generazione: la **sconsiglio con onestà** («verrebbero comunque onesti, ma poco spendibili come candidatura») e lascio a me la scelta finale con due bottoni — «Genera comunque il CV mirato» e «Mi fermo qui». Sopra soglia, tutto come prima. Un solo gate, all'ingresso del ramo mirato. La soglia è una costante dichiarata (`SOGLIA_STELLE_GENERAZIONE`).
- **`estraiJson` robusto al preambolo (C).** Lato server, la funzione che tutti gli endpoint usano toglieva solo il recinto ```` ```json ```` e faceva `JSON.parse`: un preambolo in prosa prima del `{` la mandava in 502. Ora provo il parse come prima e, **solo se fallisce**, ripiego ritagliando dal primo `{` all'ultimo `}` e riprovo; se neanche così è JSON valido, rilancio l'errore originale.
- **Verificato prima di cantare vittoria**: `node --check` su `server.js` e sul JS estratto da `index.html`; test funzionale di `estraiJson` su sei casi (JSON pulito, con recinto, preambolo, coda, recinto+preambolo, spazzatura); rilettura della logica del gate. Doppio controllo, esiti concordi.

**Cosa ho imparato**
- **Il percorso felice non si tocca.** Per il JSON robusto la scelta giusta è stata mettere il ripiego **dentro il `catch`**: gli input ben formati (che partono già con `{`) fanno esattamente ciò che facevano prima, e il recupero scatta solo quando serve. Robustezza aggiunta senza rischiare regressioni.
- **Sconsigliare non è impedire.** La soglia non blocca nulla: rende visibile una verità (match basso → candidatura debole) e lascia la decisione a me. È la stessa onestà dell'anti-invenzione, applicata all'esperienza d'uso invece che ai contenuti.

**Dove ho faticato / cosa non era ovvio**
- **Che soglia?** Il valore non è un dettaglio tecnico ma una scelta di prodotto: troppo alta intralcia match legittimi-ma-modesti, troppo bassa avvisa solo i casi-limite. Ho scelto **1,5 stelle** come compromesso — più largo del solo 0,1 del collaudo, ma non invadente.
- **Fin dove arriva C.** Il gemello front-end (`estraiFrammento`) ha lo stesso limite di preambolo, ma il buco del backlog era quello **server** (`estraiJson`, usata da tutti gli endpoint): l'ho chiuso lì e ho annotato l'altro in `idee_future.md`, senza allargare lo scope di mia iniziativa.

**Cosa ho deciso e perché**
- **Un solo gate, all'ingresso del ramo mirato.** Se scelgo «Genera comunque», il ramo prosegue intero (CV-2 → lettera) senza altri avvisi: basta una volta e non intralcio.
- **Chiudere i buchi già visti prima di aprire fronti nuovi.** B e C erano cose «quasi finite»; le idee più corpose (domicilio confrontabile, passo 1 dell'import su Sonnet) restano nel backlog, da soppesare dopo.

💡 *Mia intuizione / scelta ragionata* — Nessuna delle due è una feature che si vede: sono due modi di **non mentire per omissione**. La soglia non nasconde che un match è debole; il parser non finge un 502 quando il JSON c'era, solo avvolto di parole. Piccole, ma nella direzione giusta: un MVP che dice la verità anche quando è scomoda.

### Step 1.36 — Il CV a due colonne su Haiku: un test che finisce con «va già bene»

*Avevo in backlog un dubbio sull'import da CV (2.1.2, punto b): il passo 1 — la trascrizione del PDF — gira su Haiku, economico; ma se un CV con layout a due colonne uscisse mescolato, avrei dovuto salire a Sonnet. Invece di deciderlo a tavolino, ho fatto quello che mi viene naturale: prima far girare il sistema com'è, poi valutare sui dati. Questo Step non produce codice: produce una decisione motivata.*

**Cosa ho fatto**
- **Ho costruito il banco da zero.** Nessun CV a due colonne a portata di mano e nessuno strumento PDF installato: ho scritto due CV in HTML con layout a due colonne (uno "a blocchi", uno a **tabella con le due colonne allineate riga per riga** — il caso-trappola che spinge a leggere in orizzontale), li ho convertiti in PDF con Chrome headless e li ho dati al passo 1 (`/leggi-pdf`) **su Haiku**, così com'è oggi. Il testo lo avevo scritto io: quindi conoscevo già la verità da confrontare, senza bisogno di estrarla dal PDF.
- **Ho confrontato output e verità.** In **entrambi** i casi Haiku ha trascritto tutto, nell'ordine logico per colonna (prima la sinistra intera, poi la destra), **senza interlacciare** e senza perdere nulla — nomi, date e cifre esatti. Doppio controllo con due layout indipendenti, incluso il caso-trappola: esiti concordi.

**Cosa ho imparato**
- **Haiku non "estrae testo", legge il documento.** Il PDF gli arriva come blocco `document`: usa comprensione visiva, non un estrattore lineare che seguirebbe l'ordine dei byte. È per questo che le colonne non si mescolano — il modello *vede* la pagina.
- **Conoscere la verità a monte semplifica il test.** Creando io il CV, il ground truth era il mio sorgente: mi sono risparmiato di installare strumenti PDF per ri-estrarlo. La strada più corta era anche la più pulita.

**Dove ho faticato / cosa non era ovvio**
- **L'ambiente non collaborava.** Niente `poppler`, niente moduli PDF, `sudo` che chiede la password: la tentazione era installare mezza toolchain. Il giro giusto è stato spostare il problema — generare il PDF con un browser che c'era già (Chrome su Windows) e usare il mio HTML come verità.

**Cosa ho deciso e perché**
- **Non salgo a Sonnet: Haiku basta.** I dati dicono che per i CV a due colonne a testo nativo la trascrizione è pulita; salire a Sonnet sarebbe **costo senza beneficio**. Ho archiviato il punto (b) del backlog come *valutato*, tenendo onestamente fuori ciò che **non** ho testato: i **PDF scannerizzati/immagine** (già limite noto, punto a) e i layout estremi. Se un domani ne salta fuori uno sporco, il banco si rifà in pochi minuti.

💡 *Mia intuizione / scelta ragionata* — La cosa più difficile qui non è stata tecnica: è stata resistere alla voglia di "migliorare" qualcosa che già funziona. Il mio metodo dice di credere ai dati anche quando dicono *non toccare niente* — e un test che si chiude con «va bene così» vale quanto uno che scopre un bug: mi ha risparmiato una spesa inutile e mi ha lasciato una prova, non un'impressione.

### Step 1.37 — L'hard-gate: quando un requisito manca, il match deve dirlo davvero

*Prima di iniziare a pensare alla migrazione VB.NET volevo ultimare le cose sul lato Node, e c'era un limite noto che mi dava fastidio: un requisito davvero squalificante — la patente C per un autista, l'iscrizione a un albo — non craterava il match. Il clamp poteva abbassarlo di 20 punti al massimo, così un candidato senza il paletto insormontabile poteva comunque uscire a 2-3 stelle. Un match che mente per ottimismo. Con Mirco (io) abbiamo deciso: un solo intervento di sostanza sul durevole, l'hard-gate, e poi si valida e si migra.*

**Cosa ho fatto**
- **Un flag nuovo, tenuto separato dal peso.** Ho aggiunto ai giudizi del confronto un campo `eliminatorio` (booleano). Non è un terzo livello di priorità: la priorità dice *quanto pesa* un requisito, l'eliminatorietà dice *se è un cancello*. Due assi distinti. Lo assegna l'LLM del confronto leggendo il senso dell'annuncio ("indispensabile", "tassativo", "richiesto per legge"); **nel dubbio, false** — non voglio craterare a caso.
- **Il tetto nel codice, deterministico.** In `calcolaMatch`, se almeno un requisito è `eliminatorio` con esito `non soddisfatto`, il match finale non può superare **20/100 (≤ 1 stella)**: `finale = min(finale, 20)`, applicato *dopo* il clamp, con una nota esplicita che dice quale requisito ha fatto scattare il tetto. Prompt e schema in `prompt_design.md`, identici in `server.js` (sync char-by-char verificato); nota e marcatore ⛔ mostrati anche nel front-end e nel banco dell'anello 3.
- **Doppio controllo, nel tempo e nel metodo.** Primo controllo: test automatico su `calcolaMatch` estratto dal file vero, sei casi / sedici verifiche (gate attivo, gate ma requisito soddisfatto, flag come stringa, match già basso che non va *alzato*, gate + clamp con note coerenti) — tutti verdi. Secondo controllo indipendente: prova end-to-end reale contro `/confronta` (Sonnet) con un candidato patente B e un annuncio "patente C indispensabile" → l'LLM ha marcato ⛔ solo la patente, il match è crollato da un conteggio 68 a **1 stella**. Poi la prova nel browser, gate attivo e controprova: quando la patente richiesta è la B che il candidato ha, nessun ⛔ e match a 4,5 stelle. I tre esiti concordano.

**Cosa ho imparato**
- **Peso e cancello sono cose diverse.** La tentazione era alzare il peso dei requisiti squalificanti. Ma un peso, per quanto alto, resta una media: annega in mezzo agli altri. Un deal-breaker non è "molto importante", è **discreto**: o passi o non passi. Serviva un meccanismo a gradino (un tetto), non un peso più grande.
- **Due onestà che si incastrano.** L'hard-gate cratera a ≤1 stella; la soglia B (Step 1.35) sotto 1,5 stelle sconsiglia la generazione. Non le avevo pensate insieme, ma si completano: il gate rende vero il numero, la soglia lo traduce in un consiglio. L'ho visto dal vivo — gate a 1 stella e subito l'avviso "genera comunque / mi fermo qui".

**Dove ho faticato / cosa non era ovvio**
- **Le note che restano coerenti.** Quando scattano insieme clamp e gate, la nota del clamp cita un "match finale" che poi il gate sovrascrive. Ho spostato il calcolo così che i numeri nella nota siano quelli definitivi: prima cosa *darebbe* il confronto puro, poi il tetto "a prescindere dal resto". Una riga di ordine in più per non confondere l'utente con due numeri in conflitto.
- **Non craterare ciò che non è una lacuna piena.** Il gate scatta solo su `non soddisfatto`, mai su `in parte` (qualcosa c'è) o `non determinabile` (non so): abbassare il match per un dubbio sarebbe l'opposto della prudenza anti-invenzione.

**Cosa ho deciso e perché**
- **Tetto a ≤1 stella, non azzeramento.** Zero sembrerebbe un errore di calcolo e cancellerebbe il resto del profilo, che esiste. ≤1 stella dice "incompatibile per questo annuncio" restando un numero credibile, e si aggancia alla soglia B.
- **Un solo intervento sul durevole, poi si migra.** L'hard-gate era l'unica cosa di sostanza che valeva la pena chiudere sul lato Node prima di VB.NET; il resto è impalcatura (si rifà là) o è rimandato di proposito. Ultimare, non aggiungere.

💡 *Mia intuizione / scelta ragionata* — La domanda giusta non era «quanto deve pesare un requisito squalificante?» ma «un requisito squalificante è una questione di peso?». La risposta è no: è un cancello. Aver separato i due concetti — peso e cancello — ha reso il codice più semplice *e* il match più onesto. E la cosa che mi piace di più è che l'onestà di oggi si aggancia a quella di ieri: il gate e la soglia B, nate separate, dicono la stessa verità da due punti diversi.

### Step 2.1 — Il repo si sdoppia: il prototipo va in bacheca e la Fase 3 si scrive prima del codice

*Lo Step 1.37 si era chiuso con «ultimare, non aggiungere: poi si migra». Oggi il tutor mi ha consegnato il mandato della migrazione — un'applicazione VB.NET per Windows 11, un solo exe, con dentro tutto quello che il prototipo sa fare più le funzioni che al prototipo mancavano (ricerca degli annunci sui portali, email di candidatura, registro, e altre ancora). E una regola di metodo netta: prima si scrive il progetto, a livello mio — da perito, non da ingegnere — e finché non siamo convinti non si scrive una riga di codice. Il numero degli step cambia serie: la Fase 3 apre il 2.x.*

**Cosa ho fatto**
- **Riorganizzato il repo in due aree.** Tutto il prototipo (server, pagina, banchi di prova, `prompt_design.md`, `architettura.md`, note di ricerca) è migrato in `HTML+JS/` con `git mv` (la storia dei file resta leggibile: git li vede come rinomine, non come cancella-e-ricrea). La nuova area `VB.NET/` ospita da oggi tutto lo sviluppo. In radice restano i documenti che attraversano le fasi: README, questo diario, `idee_future.md` e le regole di progetto — perché la storia è una, anche se le tecnologie sono due.
- **Conservato il mandato tal quale.** Il prompt d'incarico del tutor è salvato verbatim (refusi compresi: è un documento storico, non un testo da abbellire) in `VB.NET/PROMPT_DI_INCARICO.md`, insieme alle integrazioni arrivate nella stessa giornata: il server MCP integrato, il multilingua italiano/inglese, l'export JSON del profilo per backup, e il vincolo di riservatezza sulla suite aziendale.
- **Scritto il progetto dettagliato.** Sedici documenti in `VB.NET/progetto/`: visione e perimetro, architettura, interfaccia (con il sistema di design e il pannello del logo con versione e pool), la libreria dei prompt in file `.md`, i documenti in ingresso e uscita (PDF/TXT/MD/DOCX dentro, DOCX/PDF fuori), la ricerca annunci col browser integrato, le email e il registro delle candidature, la rifinitura anti-slop, il server MCP, il multilingua, dati/sicurezza/backup, i flussi utente, la distribuzione a exe singolo, il piano di lavoro a tappe T0–T9 e — capitolo che mi piace più di tutti — le **decisioni aperte**, dove ogni scelta che spetta a noi è scritta con la sua proposta.

**Cosa ho imparato**
- **Progettare è un mestiere diverso dal costruire.** Nel prototipo decidevo scrivendo codice e guardando cosa succedeva; qui ho dovuto decidere *senza* poter provare, e ogni «lo vedremo dopo» andava dichiarato come decisione aperta, non nascosto sotto il tappeto. Il capitolo 15 è nato proprio per questo: è l'elenco onesto di ciò che non è ancora deciso.
- **Il «no» di ieri era un investimento.** Lo Step 1.34 aveva provato e scartato il prelievo diretto degli annunci; quella bocciatura motivata è diventata, pari pari, il cuore del capitolo sulla ricerca: il browser integrato dove l'utente naviga come sé stesso e il programma legge la pagina che lui sta guardando. Non ho buttato via niente: il disegno a valle non cambia di una virgola.
- **Family feeling non vuol dire copiare.** L'applicazione deve somigliare a quelle di casa Aviolab, ma la suite aziendale è proprietaria e questo repo è il mio portfolio pubblico: la soluzione è stata specificare il design come sistema *proprio* di AI-CV-COACH — colori, font, livelli dei bottoni, il pannello del logo — senza raccontare com'è fatto il software di casa. Il risultato visivo sarà familiare; il documento è autonomo.

**Dove ho faticato / cosa non era ovvio**
- **Tracciare il confine tra le due aree.** `architettura.md` è la bussola concettuale anche per la fase nuova: la tentazione era tenerlo in radice. Ha vinto l'idea che sia la fotografia del prototipo (runtime Node compreso), quindi sta in `HTML+JS/`; il progetto VB.NET lo cita come fondamento e ne estende il disegno nei propri capitoli.
- **Dire addio alla regola sync char-by-char.** Era la regola #1 del progetto: prompt identici in due file, confronto carattere per carattere. Nel disegno nuovo il prompt vive in un posto solo (il pool di `.md` con manifest e versione) e quella regola muore *perché sparisce il doppione che la rendeva necessaria*. Mi ci è voluto un momento per accettare che eliminare una regola potesse essere un progresso.

**Cosa ho deciso e perché**
- **Design-first con cancello.** Il piano parte da una tappa T0 che non è codice: è lo svuotamento del capitolo 15 insieme al tutor. Finché lì c'è una voce non discussa, non si implementa. Preferisco un cancello dichiarato a un «intanto comincio» che poi decide da solo.
- **Il pool di prompt in chiaro, con la versione accanto al logo.** I prompt sono l'asset del progetto e stanno in file `.md` leggibili, versionati tutti insieme (`Pool 1.03`), integrati nell'exe con una cartella esterna facoltativa per la messa a punto. Trasparenti di proposito: nel mio caso, il pool *è* il portfolio.
- **Le proposte le ho messe per iscritto.** Exe autonomo che ingloba il runtime (copi un file e funziona), `.eml` come uscita email di riferimento, tool MCP di sola lettura/generazione nella prima versione: su ogni bivio il progetto porta una proposta motivata, così la discussione con il tutor parte da qualcosa e non da un foglio bianco.

💡 *Mia intuizione / scelta ragionata* — Scrivere il progetto dettagliato è stato il vero esame del prototipo: ogni funzione che sapevo spiegare a livello di perito era una funzione capita davvero; dove l'inchiostro si inceppava, lì c'era una decisione che credevo presa e non lo era. Il codice verrà dopo, ma il collaudo del pensiero è già cominciato.

### Step 2.2 — Il cancello T0 si chiude: tredici decisioni prese, alcune contro la mia proposta

*Lo Step 2.1 aveva piantato un cancello davanti al codice: finché il capitolo 15 avesse una voce non discussa, non si implementa. Oggi ho ripassato quel capitolo voce per voce alla postazione del tutor, listino e documentazione alla mano. Il capitolo è svuotato: ogni decisione ha un esito definitivo, e otto di esse si discostano dalla proposta che avevo scritto — segno che discutere serviva davvero. Il cancello T0 è chiuso; T1 può partire.*

**Cosa ho fatto**
- **Ripassato il capitolo 15 una voce alla volta**, riportando l'esito definitivo accanto a ogni proposta. Dove l'esito cambia la proposta, la colonna dice *perché*: niente scelte mute.
- **Riportato le decisioni che cambiano negli altri capitoli.** Ogni scelta che si discosta dalla proposta è stata propagata dove viveva il disegno: .NET 10 nei capitoli distribuzione e piano; il nome utente nell'interfaccia e nella distribuzione; Sonnet 5 nell'architettura; i portali e il profilo LinkedIn nella ricerca annunci e nel piano; il `.msg` e l'SMTP fuori nelle email; la taratura del match nei dati. Il §15.6 tiene la tabella di questo allineamento, così si controlla che nessun capitolo resti indietro.
- **Segnato T0 come chiuso** nel piano (cap. 14): «i documenti 01–15 sono confermati, T1 può iniziare».

**Cosa ho imparato**
- **Verificare batte ricordare.** Non ho preso le date di supporto e i listini a memoria: le ho controllate. .NET 8 esce di supporto il 10/11/2026 (già in sola manutenzione), .NET 10 è LTS fino al 2028 — quindi partire dalla LTS che scade fra pochi mesi sarebbe stato un debito nato vecchio. Stessa cosa per l'invio email: l'autenticazione con password su SMTP è rifiutata al 100% da Microsoft dal 30/04/2026, quindi «uso la mia casella» non era più un'opzione, non un mio timore.
- **Un nome non è un dettaglio.** «AI-CV-COACH» resta il nome del progetto, del repo, dei documenti — ma ciò che l'utente legge diventa «TrovaLavoro», sul modello di TrovaPrezzi: ricordabile e comprensibile a chi non è tecnico. Ho imparato a tenere separate due identità che credevo una sola: quella per me e quella per chi userà il programma.

**Dove ho faticato / cosa non era ovvio**
- **Accettare gli otto «⚠️ Cambiata».** Avevo scritto proposte motivate, e vederne otto su dieci ribaltate all'inizio sembrava una bocciatura. Poi ho capito che era il contrario: la proposta secca era servita a far partire la discussione da qualcosa, e ogni cambio portava un fatto verificato che da solo non avevo. Il capitolo 15 non è un elenco di miei errori, è il verbale di una decisione presa in due.
- **Distinguere «fuori dalla 1.0» da «mai».** L'SMTP diretto e il `.msg` escono dalla prima versione, ma non li ho cancellati: sono migrati nel 15.3 con la porta socchiusa (l'SMTP rientra solo con OAuth 2.0, che è un progetto a sé). E in senso opposto, il profilo da LinkedIn — che era un'idea futura — è stato *promosso dentro* la 1.0, perché il browser incorporato a T5 lo rende quasi solo un pulsante in più. Rimandare e promuovere sono due mosse, non una porta che si chiude.

**Cosa ho deciso e perché**
- **Il cancello si chiude solo a capitolo svuotato.** Non «quasi tutte» le decisioni: tutte, con le rimandate dichiarate una per una con la loro motivazione. È la regola che mi ero dato nello Step 2.1, e l'ho rispettata invece di aggirarla con un «il resto lo vedo strada facendo».
- **La taratura del match esce dal codice ma resta invisibile all'utente.** Soglia e pesi vivono in un file di taratura nella cartella dati (ritoccarli durante le prove non costa una build), ma nessun pannello li espone: le stelle devono restare confrontabili fra annunci, e un utente che sposta i pesi romperebbe proprio quella comparabilità.
- **Attribuzione dei commit messa nero su bianco.** Da qui in avanti ogni commit si chiude con la sola riga `(c) 2026 Aviolab AI`, senza menzione dello strumento: l'ho ratificata come regola di progetto (regola 12), coerente con l'identità git delle due postazioni (regola 11).

💡 *Mia intuizione / scelta ragionata* — Il valore del cancello non era fermare il codice, era costringermi a trasformare ogni «lo deciderò dopo» in una decisione con una data e un motivo. Otto proposte su dieci sono cambiate: se avessi cominciato a scrivere codice sulla prima stesura, avrei costruito otto volte sulla sabbia. Ora la sabbia è diventata pietra, e T1 può appoggiarcisi.

### Step 2.3 — L'ambiente di T1: l'SDK c'era già, ma volevo la prova (e la misura)

*Il piano diceva che il primo passo materiale di T1 era installare l'SDK .NET 10, «oggi assente». Sono partito per installarlo e ho fatto la cosa che il mio metodo mi impone prima di ogni lavoro: guardare com'è la macchina adesso, invece di fidarmi di quello che avevo scritto ieri. L'SDK c'era. Ma la scoperta interessante non è stata quella: è stato ciò che ho trovato provando davvero a produrre l'exe unico, il vincolo più rigido di tutto il progetto.*

**Cosa ho fatto**
- **Prima ho guardato, poi ho installato.** Sulla mia postazione (aviolab03) c'erano già l'**SDK 10.0.204** e i runtime 10.0.8, arrivati insieme a **Visual Studio 2026 Community 18.5** con il workload desktop: il documento diceva «assente» perché era stato scritto prima. Ho verificato la cosa da tre parti indipendenti — `dotnet --list-sdks`, la cartella `C:\Program Files\dotnet\sdk` e l'inventario dei componenti di Visual Studio — e i tre esiti concordavano.
- **Ho comunque installato l'SDK «mio».** `winget` non vedeva alcun pacchetto .NET 10 registrato (quello di Visual Studio non lo è), quindi ho fatto l'installazione pulita della **10.0.302**, che porta anche i runtime **10.0.10**. Le due bande (10.0.2xx e 10.0.3xx) convivono senza sovrascriversi: non ho perso nulla di ciò che Visual Studio si era portato dietro.
- **Ho collaudato la catena da zero**, non l'inventario: progetto WinForms VB appena creato → `dotnet build -c Release` (0 errori, 0 avvisi) → pubblicazione **single-file, autonoma e non compressa** → **116 MB in un file solo** → e poi l'ho **avviato davvero**, controllando che il processo girasse, prima di chiuderlo. Verificato di passaggio anche che il runtime **WebView2** sia già nel sistema: due capitoli del progetto (ricerca annunci e stampa PDF) lo danno per scontato, ed è giusto saperlo prima e non a T5.

**Cosa ho imparato**
- **Un solo file non è gratis: te lo devi chiedere.** Alla prima pubblicazione, accanto all'exe è comparso il file dei simboli `.pdb`. Nessun errore, tutto «riuscito» — ma il vincolo che mi ero dato («un exe, nessuna DLL a fianco») era già violato al primo tentativo, in silenzio. Serve `DebugType = none` nei parametri: l'ho aggiunto al capitolo 13, dove vivono i comandi di pubblicazione, così non dipenderà dalla memoria di nessuno.
- **La stima regge, e ora ha un pavimento.** Una finestra **vuota** pesa già 116 MB, perché dentro c'è tutto il runtime .NET. I 150–180 MB stimati per l'app completa non erano pessimismo: erano realismo.

**Dove ho faticato / cosa non era ovvio**
- **L'installazione ferma senza dire perché.** Il comando è rimasto appeso dieci minuti senza scrivere una riga. Non era lento e non era rotto: c'era una finestra di **Controllo dell'account utente** che aspettava un clic e che dal terminale non si vedeva. L'ho capita guardando i processi attivi — c'era `consent.exe` — e da lì è bastato approvare. Mi ha insegnato che «bloccato» non è una diagnosi: è l'inizio di una.
- **Provare che parte è un'altra cosa che compilare.** Il primo tentativo di avviare l'exe da riga di comando è finito con un «Accesso negato» che non c'entrava con il programma. Invece di accontentarmi del «ha compilato», ho cambiato strada finché non ho visto il processo vivo nell'elenco: la differenza tra un file prodotto e un programma che parte è tutta lì.

**Cosa ho deciso e perché**
- **Verificare la macchina prima di eseguire il piano.** Il documento diceva «assente» ed era invecchiato di un giorno. Non ho corretto il piano a memoria: ho misurato, poi ho scritto nei capitoli 13 e 14 cosa c'è davvero, con le versioni precise.
- **Provare il vincolo più rigido subito, anche a vuoto.** T1 prevede il publish di prova sull'app vera; farlo **prima** su un progetto vuoto è costato dieci minuti e ha già fruttato un parametro mancante nel progetto. Il vincolo che può far saltare tutto va toccato all'inizio, quando correggere costa una riga.
- **La postazione del tutor resta da fare.** Lì c'è ancora l'SDK 9: l'ho lasciato scritto nei capitoli invece di dare l'ambiente per «sistemato» dopo aver sistemato solo il mio.

💡 *Mia intuizione / scelta ragionata* — Sarei potuto uscire da qui con un «ambiente installato ✔» in due minuti, e sarebbe stato vero. Invece la mezz'ora spesa a costruire un programma finto, pubblicarlo e aprirlo mi ha dato tre cose che l'installazione da sola non dava: un parametro mancante nel progetto, un numero vero al posto di una stima, e la certezza che il pezzo più rischioso della distribuzione funziona **oggi**, non «quando ci arriveremo». È lo stesso metodo dello Step 1.36 — prima far girare il sistema, poi valutare — applicato a un ambiente invece che a un prompt.

### Step 2.4 — T1: lo scheletro che parte, e un logo che ho dovuto correggere tre volte

*Lo Step 2.3 aveva collaudato l'ambiente su un programma finto. Adesso tocca all'app vera: la soluzione Visual Studio, la finestra con la sua barra e il suo pannello logo, l'exe di rilascio. È la tappa in cui il progetto smette di essere solo documenti — e infatti la parte che mi ha impegnato di più non è stata il codice, ma tre giri di correzioni su una cosa che nel piano occupava mezza riga: il logo.*

**Cosa ho fatto**
- **Lo scheletro, costruito tutto insieme e poi ripassato file per file.** `TrovaLavoro.sln` con il progetto WinForms su .NET 10, `FormPrincipale` (barra superiore di navigazione, area centrale ancora vuota, barra di stato, pannello logo flottante in basso a sinistra), il modulo `StileApp` con **tutti** i token del capitolo 03 — colori, font, spaziature — così nessun form potrà più inventarsi un colore per conto suo.
- **La versione in un posto solo.** `Versione.vb` porta la costante; il file di progetto la **rilegge da lì** con un'espressione MSBuild e ne ricava versione di file, di assembly e informativa. Non esiste un secondo posto dove il numero possa restare indietro.
- **Le proprietà dell'eseguibile**: prodotto TrovaLavoro, società Aviolab AI, copyright © 2026 Aviolab AI, tutte visibili nella scheda «Dettagli» di Windows.
- **`publish.bat`**, che produce il rilascio sempre con gli stessi parametri del capitolo 13 — e che se nel PATH trova un SDK vecchio ripiega su quello installato nel profilo utente, invece di fallire con un messaggio da decifrare.
- **Il collaudo vero**: build senza errori né avvisi, pubblicazione a **file singolo autonomo da 116 MB**, avvio in **~0,26 secondi** a freddo, «Dettagli» verificati. La cartella di pubblicazione conteneva **un solo file**, come deve essere.

**Cosa ho imparato**
- **Un'app «vuota» pesa quanto il runtime che si porta dietro.** I 116 MB dell'app vera sono gli stessi 116 MB del progetto finto dello Step 2.3: tutto il peso è .NET, e lo scheletro non aggiunge nulla di percepibile. Il margine sulla stima 150–180 MB è quindi interamente a disposizione delle funzioni che verranno.
- **«Family feeling» non si specifica a parole.** Nel progetto avevo scritto «logo in basso a sinistra con versione e pool». Sembrava sufficiente. Alla prova a schermo non lo era per niente: la prima resa portava un segnaposto tipografico «TL», che è esattamente ciò che avevo scritto e non ciò che volevo.

**Dove ho faticato / cosa non era ovvio**
- **Tre giri sul logo.** Ho dovuto correggere in corsa: prima il segnaposto «TL» sostituito dallo **scudo Aviolab** vero, poi la scritta sotto lo scudo — che deve essere **sempre e solo «AVIOLAB AI»**, perché quel pannello è il marchio aziendale e non il nome del prodotto — e infine la forma in cui l'immagine vive nel progetto. Nessuna di queste tre cose stava nel capitolo 03 scritto da me: le ho viste solo guardando la finestra.
- **Un © che sembrava sbagliato e non lo era.** Nella verifica da terminale il copyright compariva come «c 2026»: sembrava un carattere perso per strada. Era solo la resa della console. Controllato **al byte**, nel file il simbolo giusto c'era. Mi sono segnato la lezione: quando lo strumento di controllo è più povero del dato che controlla, il difetto può essere dello strumento.

**Cosa ho deciso e perché**
- **Il logo vive in forma binaria dentro il sorgente** (PNG codificato in Base64), non come file accanto all'exe né come risorsa esterna. Il vincolo del progetto è «un solo file»: un'immagine da copiare a fianco lo avrebbe incrinato alla prima distribuzione. Ho aggiornato i capitoli 03 e 13 e ho chiuso come «superata» la voce del capitolo 15 che prevedeva il segnaposto.
- **Il marchio in basso a sinistra è di Aviolab, il nome del prodotto sta nella barra del titolo.** Due identità distinte, che avevo confuso in una: «AVIOLAB AI» sotto lo scudo, «TrovaLavoro» come titolo della finestra, AI-CV-COACH come nome del progetto e del repository.
- **Il collaudo su un PC davvero pulito resta da fare.** L'ho verificato sulla macchina di sviluppo, che ha l'SDK installato: non è la stessa cosa. L'ho lasciato scritto nel piano come cosa in coda a me, insieme all'icona dell'exe e all'SDK sulla postazione del tutor, invece di dichiarare la tappa perfetta.

💡 *Mia intuizione / scelta ragionata* — La parte «difficile» di T1 doveva essere la pubblicazione a file singolo, e invece era già risolta dallo Step 2.3. La parte che è costata davvero è quella che nel piano pesava mezza riga: come deve **apparire** il programma. Ci ho letto una cosa sul confine tra me e l'assistente: il codice si può delegare e poi rivedere, il gusto no — quello devo metterlo io, guardando lo schermo, e nessuna specifica scritta bene lo sostituisce.

### Step 2.5 — T2: il motore, e un prompt identico carattere per carattere

*Questa è la tappa che temevo di più: portare in VB.NET il cuore che nel prototipo funzionava già — l'estrattore JSON, il calcolo del match, i quindici prompt, il client dell'AI — senza che nulla cambiasse per strada. La regola che mi ero dato era «prima uguale, poi meglio», con il prototipo nel ruolo di giudice. Alla fine il giudice ha assolto la nuova app, ma per due volte durante la strada mi ha fermato — e quelle due volte sono la parte interessante di questo Step.*

**Cosa ho fatto**
- **Portato i quattro pezzi del motore**, uno per commit: `EstrattoreJson` (stessa strategia del prototipo: percorso felice intatto, ritaglio dal primo `{` all'ultimo `}` solo dentro il *catch*), `CalcoloMatch` con i suoi numeri sollevati fuori dal codice in un file di taratura, la **libreria dei prompt** con i quindici prompt migrati nel **Pool 1.00** (manifest, impronte SHA-256, pool esterno che ha la precedenza su quello incorporato) e `ClientClaude` con timeout, un solo ritentativo e i due livelli di modello letti da configurazione.
- **Costruito la batteria di non-regressione su due gambe.** La prima non tocca la rete: sugli stessi dati, il prompt che l'app costruisce dal pool deve essere **identico carattere per carattere** a quello che il prototipo costruisce nel suo codice. La seconda è il confronto vero: gli stessi due annunci mandati al prototipo acceso e alla nuova app, stesso modello da entrambe le parti.
- **Fatto girare il collaudo reale** con il prototipo in funzione come giudice. Sull'annuncio compatibile: **4,6 stelle** da una parte e dall'altra. Su quello che chiede la patente C a un candidato che ha solo la B: **0,9 stelle e il ⛔** da entrambe. E il colpo che volevo davvero: i **giudizi prodotti dal prototipo**, dati in pasto al *mio* calcolo del match, restituiscono i suoi numeri identici — nota doppia del gate compresa.

**Cosa ho imparato**
- **Lo stesso JSON per una macchina può essere un altro prompt per il modello.** Alla prima prova di parità, il testo che partiva dalla mia app scriveva «Forlì» con una sequenza di codici al posto della lettera accentata: JSON perfettamente valido, identico per qualunque parser — e diverso per il modello, che quel prompt lo *legge*. Era il comportamento predefinito di .NET, che protegge da usi in cui il JSON finisce dentro una pagina web; qui il JSON finisce dentro un prompt, e la protezione diventava una differenza. Da lì la regola scritta nel capitolo 04: gli artefatti si scrivono come li scriveva il prototipo, accenti e apostrofi in chiaro.
- **Dove mettere il pass/fail quando in mezzo c'è un'AI.** Due chiamate allo stesso modello con lo stesso prompt non danno mai la stessa risposta: pretendere numeri identici sarebbe stato un collaudo destinato a lampeggiare a caso. Il confronto secco l'ho messo dove le cose sono deterministiche — i giudizi del prototipo ricalcolati dal mio codice — e per il resto ho dichiarato una tolleranza: mezza stella, che è il passo del voto che l'utente vede.
- **Il rito che si morde la coda.** Il capitolo 04 dice che ogni modifica ai prompt si chiude annotando il changelog del pool. Scrivendolo mi sono accorto che il sigillo avrebbe messo nel manifest anche *lui*: sigilli, annoti il changelog, e il pool appena sigillato risulta modificato. Ho fatto in modo che nel manifest entrino solo i prompt veri, riconosciuti dall'intestazione di metadati.

**Dove ho faticato / cosa non era ovvio**
- **La differenza che non era una regressione.** La prima esecuzione della parità è fallita all'ultimo carattere su 10 596: il mio prompt finiva con un a capo, quello del prototipo no. Non era un difetto della migrazione — era che nel prototipo i prompt erano stringhe dentro il codice, e qui sono file di testo, e ogni editor l'a capo finale lo mette da sé. Potevo dichiararlo irrilevante nel collaudo; ho preferito togliere gli a capo in coda al testo che parte, così ciò che arriva al modello dipende dal contenuto e non da come è stato salvato il file.
- **Due comandi che si sommano invece di sostituirsi.** I collaudi reali stanno in una categoria a parte, perché vogliono la chiave e il prototipo acceso e non possono girare tutti i giorni. Ho scoperto che il filtro passato a riga di comando **si somma** a quello del file di impostazioni invece di rimpiazzarlo: chiedere «solo i reali» mentre il file diceva «mai i reali» dava un insieme vuoto, senza errori e senza spiegazioni. Risolto con un secondo file di impostazioni, gemello del primo.

**Cosa ho deciso e perché**
- **I casi di collaudo sono inventati, ma scritti come veri.** Il repository è pubblico: un profilo vero non renderebbe il collaudo più solido, solo più esposto. Ho costruito un candidato di fantasia — magazziniere, patente B, patentino per il muletto — con accenti e apostrofi messi **apposta**, perché è lì che una differenza di codifica salta fuori. Ed è saltata fuori davvero.
- **Gli esiti reali entrano nel repository.** Sono pochi chilobyte di JSON, e sono due cose insieme: la prova che il collaudo è stato fatto davvero, e dei casi riutilizzabili domani senza rete e senza chiave.
- **Il pannello logo ora dichiara il pool.** Da T1 mostrava «Pool —» perché il pool non esisteva; adesso esiste, e la riga dice `Pool 1.00 (integrato)` — con l'asterisco se qualche file è fuori impronta. Era già scritto nel capitolo 03: mi mancava solo la cosa da mostrare.

💡 *Mia intuizione / scelta ragionata* — La gamba che dà valore a tutta la batteria è quella che il piano non aveva previsto: verificare che la **richiesta** sia identica, prima ancora di guardare le risposte. Perché se il prompt che parte è lo stesso e il modello è lo stesso, allora una differenza nei risultati è per forza colpa del mio codice — ed è esattamente ciò che un collaudo di non-regressione deve saper dire. Senza quella gamba avrei potuto solo constatare che due risposte si somigliano, che è una frase molto più debole e che avrebbe nascosto, sotto la somiglianza, proprio l'errore degli accenti.

### Step 2.6 — T3: il profilo, e il motore costruito prima della finestra che lo mostra

*T3 è la prima tappa in cui l'utente vede qualcosa di suo: la scheda del profilo e il dialogo che lo costruisce da zero. L'ho spezzata in tre pezzi — prima il motore, poi l'import, poi i pannelli — e l'ordine non è stato un vezzo: mi ha permesso di collaudare il dialogo intero, tutti e sette i turni con l'anti-perdita, quando ancora non esisteva una sola casella di testo in cui scriverlo.*

**Cosa ho fatto**
- **T3a — il profilo nel motore.** Il profilo smette di essere un JSON qualunque e diventa un tipo con i suoi campi (`Dati/Profilo`), con un archivio che lo **versiona**: ogni salvataggio confermato lascia una copia datata nello storico, così un CV già inviato resta spiegabile anche a profilo cambiato. E soprattutto `Motore/DialogoProfilo`: la macchina a stati del prototipo portata nel motore, con gli stessi testi parola per parola.
- **T3b — l'import.** Un CV che esiste già entra da quattro porte: il PDF passa dall'AI che lo trascrive, il DOCX è un archivio ZIP da cui si estrae il testo con i mattoni standard di .NET, TXT e MD si leggono dal disco. Da lì in poi la strada è una sola.
- **T3c — i pannelli.** `Ui/PannelloProfilo` (P2), che mostra il profilo campo per campo e lo salva; `Ui/PannelloDialogo` (P5), la conversazione a bolle; e `Motore/ContestoApp`, che all'avvio monta tutto — cartella dati, pool, numeri, client dell'AI — e **non solleva mai**: quello che non si può montare resta spento e detto.

**Cosa ho imparato**
- **Mettere un oggetto in mezzo cambia cosa si può collaudare.** Nel prototipo ogni passo del dialogo disegnava da sé la pagina: logica e aspetto erano la stessa cosa, e per provare l'anti-perdita bisognava cliccare. Qui ogni passo produce una **`Mossa`** — cosa dire, cosa mostrare, cosa aspettarsi — e chi la mostra torna con una risposta. La conseguenza l'ho vista subito: il dialogo intero si prova senza interfaccia e senza rete, con frammenti preparati al posto dell'AI.
- **Un contenitore che scorre non sa quanto è alto il suo contenuto.** L'ultima bolla della chat era sempre tagliata. Ho scoperto, scrivendo i numeri su file da dentro il programma, che il contenitore che avevo scelto dichiarava un'altezza sottostimata di 24 pixel e un massimo di scorrimento fermo al valore di fabbrica. Non era un mio errore di conto: era il componente che quel conto non lo fa. Cambiato l'impianto, il calcolo torna giusto da solo.

**Dove ho faticato / cosa non era ovvio**
- **Tre difetti che il banco non poteva vedere.** Bordo della bolla disegnato su due lati soli, bolle bianche su fondo bianco, ultima bolla tagliata: nessuno dei tre sarebbe mai emerso da un collaudo automatico, perché nessuno dei tre riguarda ciò che il programma *calcola*. Li ho visti guardando la finestra.
- **La chiave che c'era e non c'era.** L'applicazione avviata dal desktop diceva «manca la chiave API» mentre la stessa chiave, nello stesso momento, funzionava per i collaudi. Sono due ambienti diversi: quello di Windows non è quello in cui lavoro. Non è un difetto del programma, ma è esattamente il messaggio che vedrebbe un utente — e infatti il programma lo dice chiaro e resta in piedi, con le funzioni AI spente.

**Cosa ho deciso e perché**
- **Dal dialogo il profilo non va su disco.** P5 lo consegna alla scheda P2, e a salvarlo è l'utente da lì. Volevo una sola porta verso l'archivio: due punti che scrivono lo stesso file sono due punti in cui può nascere la stessa incoerenza.
- **L'attesa dell'AI non è annullabile, l'import sì.** Annullare a metà un turno lascerebbe la macchina a mosse in uno stato che non esiste; e per due secondi non vale il prezzo. L'import di un CV, che dura molto di più, l'Annulla ce l'ha.
- **Uscire dal dialogo non lo azzera.** Si riprende da dov'era: solo «Ricomincia» butta via tutto, e lo dice.
- **I bottoni delle tappe che verranno restano visibili, spenti, con un tooltip che dice quando arriveranno.** Un bottone che sparisce e ricompare confonde più di uno spento che si spiega.

💡 *Mia intuizione / scelta ragionata* — La tentazione era partire dalla finestra, perché è la parte che si vede. Ho fatto il contrario, e il guadagno è arrivato nel punto che non mi aspettavo: quando finalmente ho aperto il pannello, i difetti che ho trovato erano **tutti** di aspetto — bordi, colori, altezze — e nessuno di logica. La logica l'avevo già interrogata cento volte senza schermo. Costruire nell'ordine giusto non fa risparmiare tempo: fa in modo che, quando qualcosa si rompe, tu sappia già dove non è.

### Step 2.7 — Il collaudo di tappa di T3: tre gambe, e un collaudo verde che misurava sé stesso

*Il capitolo 14 chiedeva a T3 due cose: importare il mio CV vero nei formati disponibili e fare un dialogo completo da zero. Le ho trasformate in tre gambe, perché le domande erano tre e diverse: le quattro porte portano allo stesso profilo? l'app legge il CV come lo leggeva il prototipo? il dialogo regge davanti a un modello vero? La terza è quella che mi ha insegnato di più, e non per il motivo che pensavo.*

**Cosa ho fatto**
- **Gamba A — lo stesso CV dalle quattro porte.** Word su questa macchina non c'è, quindi i tre compagni del PDF li ho fabbricati io dal testo trascritto: un DOCX con tabella d'intestazione e roba dentro il file che a schermo non si legge, un TXT in codifica ANSI con i fine riga di Windows, un MD con la sua marcatura. Tre giri: campi copiati identici fra le quattro strade tutte le volte.
- **Gamba B — il prototipo come giudice.** Stesso CV, due import in parallelo: **3228 caratteri e 60 righe** da entrambe le parti, **100% di righe in comune**, tutte le volte.
- **Gamba C — il dialogo con l'AI vera.** Ho scritto una traccia per una persona che non esiste, Anna Ricci, con **quattro trappole**: quattro cose dette apposta nel turno sbagliato, due che appartengono a un turno futuro e due a un turno già passato. Tre giri, poi la prova che il banco non può fare — l'applicazione avviata davvero, il dialogo condotto dentro il pannello, il profilo salvato dalla sua scheda.

**Cosa ho imparato**
- **Un collaudo verde può misurare sé stesso.** Il primo giro della gamba C è passato. Poi ho letto il rapporto: il conduttore automatico aveva risposto «La B.» alla domanda sulla patente e si era tenuto in tasca la battuta della traccia, facendo slittare di un turno tutte quelle dopo. Riconosceva la ri-domanda da «di che categoria» — parole che stanno *anche* nell'apertura di quel turno. Il dialogo si era comportato benissimo, quindi il collaudo era verde: solo che non stava misurando il dialogo. Ho aggiunto un controllo che confronta ogni battuta con il turno che l'ha davvero strutturata — e a dirlo non è il conduttore, è il programma.
- **Il metro sbagliato misura la cosa sbagliata.** Il primo giro della gamba A dava il 43% di righe in comune fra il markdown e il PDF, e sembrava un disastro. Stavo contando i trattini degli elenchi: appaiando le righe per sole lettere e cifre, lo stesso file sale al 100%. Il difetto era nel metro.
- **Togliere un difetto e spostarlo si somigliano molto.** La gamba B aveva trovato un volontariato contato due volte, fra i lavori e fra le esperienze informali. Ho scritto la regola «ogni attività in una sezione sola» e il doppione è sparito — ma una lettura su cinque ha risolto l'ambiguità dalla parte sbagliata, promuovendo il volontariato a impiego, perché quel CV lo stampa sotto «esperienza lavorativa». È servita una seconda regola: a decidere è la **natura** dell'attività, non la sezione in cui il CV la stampa.

**Dove ho faticato / cosa non era ovvio**
- **Il campo che lampeggia.** Il mio CV porta due indirizzi, residenza e domicilio, e nessun prompt dice quale sia «la città». A trascrizione identica bit per bit, tre esecuzioni di fila hanno dato tre risposte diverse. Non è una regressione: è un caso di confine che il prompt non decide. L'ho tolto dal pass/fail e l'ho messo nel rapporto con un ⚠️, dove si legge.
- **Un dato buono che si perde, dichiarandolo.** Nel dialogo, il «patentino per il muletto» finisce ogni volta fuori dal profilo: il turno della patente lo manda alle competenze, quello delle competenze lo rimanda alla formazione, e la guardia che impedisce ai frammenti di rimbalzare all'infinito lo scarta. Il meccanismo funziona **esattamente** come l'ho disegnato, e il risultato è comunque sbagliato — un patentino è una qualifica, ed è pure il genere di cosa che un annuncio chiede. L'ho annotato: è materia della prossima decisione sui prompt, non di una toppa.

**Cosa ho deciso e perché**
- **Il numero delle esperienze informali esce dal pass/fail, il doppione ci entra.** Quanto finemente distillare un racconto in voci è un giudizio, come per le competenze; ma la stessa attività contata due volte è **sempre** sbagliata, e la tolleranza sul conteggio se la mangerebbe in silenzio.
- **Il primo distacco voluto dal prototipo.** Su `importa_cv` ho fatto il bump al **Pool 1.01** con le due regole nuove: da lì in poi, su quel prompt, il prototipo non è più il metro — è il termine di paragone di ciò che l'app fa meglio. Avrei potuto chiudere la tappa «prima uguale» e rimandare, ma chiudere con un doppione noto sarebbe stato chiuderla male.
- **Dove va un rapporto lo decide di chi sono i dati.** Le gambe A e B girano sul mio CV vero: i loro rapporti si scrivono accanto al CV, fuori dal repository. La gamba C gira su una persona inventata: il suo rapporto sta nel repo, e si legge come una conversazione.

💡 *Mia intuizione / scelta ragionata* — La lezione della gamba C vale oltre questo collaudo. Un collaudo automatico non verifica «che il programma funzioni»: verifica che *una certa domanda* riceva la risposta attesa. Se la domanda si sposta senza che nessuno se ne accorga — perché è chi conduce la prova ad essersi spostato — il verde continua ad accendersi e non significa più niente. Da qui in avanti, quando scrivo un collaudo che *pilota* qualcosa, ci metto dentro anche il controllo che il pilota sia rimasto in carreggiata. È lo stesso doppio controllo che mi ero dato come regola di lavoro, applicato allo strumento invece che al risultato.

### Step 2.8 — La revisione adversariale: gli errori tolti uno a uno, e il Pool 1.02

*T3 si era chiusa bene, ma con dei difetti noti messi a verbale: il patentino che rimbalzava fino a perdersi, la città che lampeggiava, le lingue senza una casa. Prima di aprire T4 ho voluto una passata diversa da tutte le precedenti — non aggiungere niente, solo trovare ed eliminare errori. Il mandato all'assistente è stato «tutto tu, anche il codice», con una condizione: il metodo doveva essere il mio doppio controllo, fatto sul serio.*

**Cosa ho fatto**
- **Fatto girare il sistema com'era, prima di toccarlo**: 190 collaudi su 190, tutti verdi. Poi la revisione su due binari indipendenti — una lettura integrale di tutti i file di prodotto e, in parallelo, **quattro revisori con quattro lenti diverse** (concorrenza e interfaccia, robustezza di lettura/scrittura, integrazione fra i pezzi, pool dei prompt) — e il confronto fra i due esiti, con ogni segnalazione verificata sul codice prima di diventare una correzione.
- **Eliminata una ventina di difetti in ordine di priorità.** I peggiori perdevano dati o buttavano giù l'app: un valore storto nel file di taratura la faceva cadere all'avvio; un dialogo finito ma non consegnato spariva chiudendo la finestra, senza avviso; un profilo corrotto su disco veniva sovrascritto dal primo «Salva» che prometteva di lasciarlo lì; il ramo PDF dell'import girava fuori dalla rete di protezione, e una chiavetta smontata mostrava la finestra di crash di .NET. Poi tre buchi veri dell'anti-perdita — l'`altrove` della patente promesso dal prompt e buttato dal codice, quello delle correzioni che spariva zitto, il turno del nome che era l'unico a non averlo — e la messe di mezzo: la barra che aggirava il «mentre l'AI lavora non si esce», le eco che arrivavano dopo il verdetto, le voci vuote salvate come vere.
- **Chiuso il Pool 1.02** con il rito completo del bump: le **lingue** sono competenze e si riportano come dette, mai con un livello inventato; la **città è il domicilio**, una sola; i **patentini professionali stanno in formazione**, e ogni prompt lo dice; il turno del nome ha il suo blocco `altrove`; la guardia anti-injection presidia i sette turni.
- **Validato sul modello vero**: un dialogo intero di 25 mosse in passo perfetto, zero invenzioni, e il patentino del muletto atterrato in formazione al primo colpo — dove prima si perdeva tre volte su tre. Sull'import del CV vero: le tre lingue ricopiate alla lettera contro le zero del prototipo, e la città che è il domicilio. La batteria è salita a **205 collaudi, tutti verdi**, parità col prototipo compresa.

**Cosa ho imparato**
- **Una promessa scritta in un prompt è un contratto che il codice deve onorare.** Il prompt dei contatti prometteva di instradare la patente detta nel turno sbagliato; il magazzino del codice conosceva quattro categorie e quella non c'era, così il dato spariva mantenendo la promessa a parole. L'anti-perdita vive in due posti — i prompt e il codice — e va collaudata come una cosa sola, perché ciascuno dei due da solo può essere «giusto» mentre l'insieme perde.
- **I difetti che costano di più abitano le cuciture, non le funzioni.** Nessuno dei quattro errori critici stava in una funzione sbagliata: stavano nella chiusura della finestra, nel file corrotto, nel ramo non protetto — i punti dove il flusso finisce o s'inceppa, quelli che nessun collaudo del percorso felice attraversa mai.
- **Una batteria tutta verde non dice «non ci sono errori»: dice che le domande fatte hanno risposte giuste.** I 190 verdi di partenza convivevano con una ventina di difetti reali. La revisione adversariale è il gesto di fare domande nuove — e infatti la batteria, dopo, di domande ne fa quindici in più.

**Dove ho faticato / cosa non era ovvio**
- **Decidere cosa *non* toccare.** La guardia anti-injection manca su `confronto` e `mitigazione`, ma quei due prompt sono il metro della parità carattere per carattere col prototipo: toccarli significa rinunciare al giudice della non-regressione, e la scelta spetta a una decisione, non a una toppa. Stesso ragionamento sul pannello del logo a DPI alti — correggerlo alla cieca, senza uno schermo su cui verificare, rischiava di rompere un layout validato a video — e sul «corso senza nome» che una volta su tre sparisce al turno contatti: l'istruzione nel prompt c'è già, insistere rischia di peggiorare altro.

**Cosa ho deciso e perché**
- **La città torna un pass/fail.** Allo Step 2.7 l'avevo tolta dal verdetto perché il prompt non decideva quale fosse «la» città; ora il Pool 1.02 decide — è il domicilio — e un controllo che lampeggia è tornato un controllo che morde: la residenza, oggi, boccerebbe.
- **Il distacco dal prototipo si allarga, ma il metro resta.** Su `importa_cv` e sui turni del dialogo l'app ormai fa meglio del prototipo, e la differenza è voluta e documentata nel changelog del pool; `confronto` e `mitigazione` restano intatti apposta, perché la parità carattere per carattere è l'ancora che rende oneste tutte le altre differenze.
- **Il ramo si chiude subito.** Merge fast-forward in `main`, push, branch eliminato: la revisione non è una fase, è una passata — quello che ha trovato è dentro, quello che ha rimandato sta scritto in `in_sospeso.md` e `idee_future.md`, e si riparte puliti verso T4.

💡 *Mia intuizione / scelta ragionata* — Il doppio controllo «nel metodo» ha mostrato qui il suo valore vero: la mia lettura integrale e i quattro revisori indipendenti non hanno trovato le stesse cose, e nessuno dei due binari da solo sarebbe bastato. Non è ridondanza, è **diversità**: due modi diversi di guardare vedono errori diversi, e il confronto fra i due esiti è quello che trasforma un mucchio di segnalazioni in un elenco verificato. È la stessa ragione per cui il collaudo reale non sostituisce la batteria e la batteria non sostituisce il collaudo reale: ogni strumento vede solo ciò che sa guardare.

### Step 2.9 — T4: la pipeline che fa il mestiere, e la prima volta che il programma scrive un CV

*T4 è la tappa che rende l'applicazione un prodotto: fino a ieri sapeva raccogliere un profilo, da oggi sa leggere un annuncio, dire quanto mi somiglia e scrivere il CV e la lettera per quel posto. L'ho spezzata in tre come T3 — prima il motore, poi le stampanti, poi i pannelli — e stavolta la ragione non era solo il collaudo: le tre parti hanno tre giudici diversi.*

**Cosa ho fatto**
- **T4a — i tre mestieri e la catena.** `AnalizzatoreAnnuncio`, `Confrontatore` e `Generatore`: tre e non uno, perché il flusso li chiama in tre momenti diversi e i collaudi devono poter sostituire un momento per volta. Sotto, la meccanica che hanno in comune — carica il prompt, riempi i segnaposto, chiama, estrai il JSON — scritta una volta sola. Sopra, `PipelineCandidatura`, che mette i passi in fila e sa che la **mitigazione si salta** quando non c'è nessun gap da mitigare: il prototipo la chiamava sempre, e chiedeva ponti su un fiume che non c'era. Ogni candidatura atterra nella sua cartella, con annotata la versione di profilo da cui è nata.
- **T4b — le due stampanti.** Il DOCX componendo lo ZIP OOXML con i soli mattoni di .NET, il PDF facendo stampare una pagina HTML alla WebView2 fuori schermo. In mezzo, la scoperta che vale più delle due stampanti: una **pagina di blocchi**. Il CV JSON non arriva ai formati, prima diventa una pagina — un nome, una riga di recapiti, un titolo, un elenco — e ogni stampante sa solo come si disegna un blocco. Così i due file dicono la stessa cosa **per costruzione**, non per diligenza.
- **T4c — i due pannelli.** P4, dove un annuncio si incolla e «Analizza» fa due passi in fila perché in mezzo non c'è niente da decidere; P6, con le tre colonne e i due export. E un bottone nuovo in barra, 📋 Candidatura: senza, il pannello che avevo appena costruito non era raggiungibile da nessuna parte.

**Cosa ho imparato**
- **Un modello di contenuto in mezzo vale più di due formati fatti bene.** Avrei potuto scrivere due stampanti che leggono il JSON ciascuna per conto suo: funzionava prima e si rompeva dopo, il giorno in cui una delle due avesse imparato una sezione nuova. Con la pagina di blocchi la domanda «i due file dicono la stessa cosa?» smette di essere una speranza da verificare a ogni giro.
- **Il posto in cui una cosa si costruisce non è sempre il posto in cui vive.** La stampante PDF vuole il thread dell'interfaccia, perché dentro ha un browser; ma il motore lo montano anche i collaudi, che una finestra non ce l'hanno. Metterla nel motore avrebbe legato tutto il resto all'esistenza di una finestra, per un dettaglio di una sola classe.
- **Quando i bottoni «non fanno nulla», il banco non ti aiuta.** Il banco vede lo *stato* dei controlli, non come si *vedono*. È lì che ho chiesto all'assistente uno strumento: un server locale che compila, avvia l'applicazione vera, la fotografa, elenca i controlli dicendo se sono accesi e li preme. Non asserisce niente — guarda, e lascia il giudizio a me.

**Dove ho faticato / cosa non era ovvio**
- **Il difetto era il messaggio, non il bottone.** «Analizza» era spento e sembrava rotto. Era spento **giustamente**, perché su questa macchina non c'era nessun profilo salvato, e il motivo era pure scritto — nell'angolo opposto dello schermo, dove nessuno guardava. Chi vuole premere un bottone sta guardando quel bottone: la spiegazione sta lì sotto, o non esiste.
- **VB ha un modo tutto suo di dirti che stai coprendo un nome.** Una variabile locale non può chiamarsi come la funzione che la contiene, e se si chiama come un'altra funzione la copre in silenzio: `Valore(valore)` smette di essere una chiamata e diventa un'indicizzazione. Tre errori di compilazione dello stesso genere, tutti in mezz'ora.

**Cosa ho deciso e perché**
- **Gli artefatti nuovi restano JSON grezzo.** Il profilo è tipizzato perché P2 lo edita campo per campo; un annuncio e dei giudizi si mostrano e basta. Dove serviva leggerli si sono fatte due **viste di sola lettura**, che traducono una volta per chi disegna.
- **Lo streaming va tutto a T7.** La generazione di T4 non produce prosa, produce JSON: un tracciato che scorre a video con le sue graffe non dice niente a chi guarda. L'attesa qui si copre dicendo **a che punto siamo**, non quali caratteri stanno arrivando.
- **Il 📄 CV-1 base sta col profilo, non con le opportunità.** Nasce senza annuncio: metterlo in una cartella-candidatura vorrebbe dire legarlo a una candidatura che non ha.

💡 *Mia intuizione / scelta ragionata* — La cosa che mi porto dietro da questa tappa non è una classe: è che il programma, per la prima volta, ha prodotto un documento che potrei mandare a un'azienda. Fino a T3 il valore era tutto in ciò che *non* faceva — non inventare, non perdere. Qui, per la prima volta, si vede in ciò che fa. E il modo in cui l'ho costruita — modello di contenuto in mezzo, mestieri separati, pannelli per ultimi — è tutto figlio di una sola domanda che mi sono fatto a ogni bivio: se fra sei mesi cambio una cosa sola, quante altre devo ricordarmi di cambiare insieme?

### Step 2.10 — Il collaudo di tappa di T4: il giro intero, e un metro che pretendeva troppo

*Il capitolo 14 chiedeva a T4 tre gambe: il prototipo come giudice, la pipeline reale dall'inizio alla fine, e i file riletti campo per campo. Ma prima c'era una cosa che non avevo mai fatto — percorrere il giro **intero**, dal mio CV in PDF fino ai documenti esportati, senza mai uscire dall'applicazione. È lì che il collaudo ha smesso di essere una verifica ed è diventato un uso.*

**Cosa ho fatto**
- **Il giro completo, dall'interfaccia.** Importa da un CV → il mio `CV_Mirco_Parenti_GENOVA.pdf` → profilo proposto e salvato → Candidatura → annuncio incollato → «Analizza» → **1,4 su 5** su 14 voci giudicate → mitigazioni → 🎯 CV-2 e lettera → export DOCX e PDF. Poi, dal profilo, anche il 📄 CV-1 base con i suoi due file. Tutto con i bottoni veri, premuti dallo strumento nato a T4c.
- **Gamba A — il prototipo giudice.** Sui due prompt dove è ancora il metro, `confronto` e `mitigazione`, il prompt che parte è identico **carattere per carattere**. Sulla generazione, dove nessun collaudo automatico arriva, ho dato gli stessi input alle due parti: stessa forma, **gli stessi quattro ponti** sui gap, stessi conteggi nel CV, nessuna invenzione da nessuna delle due parti, e le due lettere che nominano gli stessi tre limiti.
- **Gamba C — i file, campo per campo.** Ogni stringa del JSON ricercata nel file davvero prodotto: il DOCX riaperto da LibreOffice, il PDF riletto dai suoi stream. **114 campi su 114**, e i due formati identici. Poi ho cambiato una lettera al nome di un'azienda per vedere il confronto diventare rosso: un verde che non sa diventare rosso non è una prova.

**Cosa ho imparato**
- **L'anti-invenzione si vede quando il match è basso, non quando è alto.** 1,4 stelle su 5 è il caso in cui un programma compiacente si metterebbe a gonfiare. La lettera che è uscita dice da sé che Python non lo conosco, che l'inglese scritto è un B1 e che il mio diploma non è un tecnico — e lo dice **portando** un elemento vero accanto a ogni mancanza. Non è prudenza: è il modo di essere credibili.
- **Un collaudo può pretendere più di quanto il prompt prometta.** Il collaudo dei quattro formati confrontava la città alla lettera fra le quattro letture, ed è diventato rosso perché dal PDF usciva «Carasco» e dal DOCX «Carasco (GE)». Sono andato a guardare i file: **tutti e quattro** scrivono «Carasco (GE)». Nessuna delle due letture aveva perso niente — il prompt dice *quale* indirizzo prendere, non *come* scriverlo.
- **Anche gli attrezzi di misura sono codice.** Ho scritto la regola nuova sulla città e le ho dato subito un banco tutto suo, che prima non esisteva. Ha bocciato la mia prima versione: col semplice contenimento sarebbe passata anche «Carasco Genova», cioè due città insieme — che è proprio quello che il prompt vieta.

**Dove ho faticato / cosa non era ovvio**
- **L'etichetta che si spacciava per la casella.** Per tre tentativi il testo dell'annuncio «entrava» e la casella restava vuota. Windows dà a una casella il nome della sua etichetta, e l'etichetta nell'albero viene prima: stavo scrivendo nella scritta. È il genere di errore che fa incolpare il programma provato invece dello strumento che lo prova.
- **La finestra di scelta del file non si lascia pilotare.** Il processo continua ad avere una sola finestra, e l'albero di accessibilità di quel dialogo mostra solo la parte alta: «Nome file», «Apri» e «Annulla» non ci sono proprio. Si passa dalle finestre native, chiamandole per numero.

**Cosa ho deciso e perché**
- **Fra le strade la città non si confronta più alla lettera**, ma resta un pass/fail dove conta: contro il **CV**, che è il metro vero. Fra due letture si ammette la sola sigla della provincia — «Carasco» dentro «Carasco (GE)» passa, «Genova» contro «Carasco» no.
- **Il difetto trovato a video l'ho chiuso subito, con due collaudi.** P4 chiedeva «c'è un profilo?» una volta sola, all'avvio: chi importava il CV e passava a Candidatura si sentiva dire «prima serve il tuo profilo» per un profilo appena salvato — proprio sul percorso del primo avvio. Ora ogni pannello che torna in vista rilegge le proprie condizioni.
- **L'annuncio del collaudo non era vero, e sta scritto.** Era verosimile, l'ho costruito io con requisiti misti. Un annuncio vero porta sigle, sezioni fuori posto e righe legali che nessuno inventa: quella prova si fa a T5, quando la cattura dal browser esisterà, ed è annotata fra le cose in sospeso invece che data per fatta.

💡 *Mia intuizione / scelta ragionata* — Le tre gambe hanno confermato quello che dovevano confermare, ma la cosa che ha insegnato di più è arrivata dal rosso: un collaudo che pretendeva più di quanto il prodotto avesse promesso. È l'errore opposto a quello dello Step 2.7 — lì il collaudo misurava sé stesso ed era verde per il motivo sbagliato, qui era rosso per il motivo sbagliato — e insieme dicono la stessa cosa: **prima di credere a un verdetto bisogna sapere quale domanda sta facendo**. Da qui in avanti, quando un collaudo diventa rosso, la prima cosa che guardo non è il codice: è se la domanda era giusta.
