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

### Step 2.11 — T5a: un browser dentro l'applicazione, e i portali come dati

*T5 è la tappa che va a prendere gli annunci dove stanno. L'ho spezzata in tre come le altre, ma con un criterio diverso: a T3 e T4 avevo messo il motore prima dell'interfaccia, qui il motore **è** l'interfaccia — una cattura non esiste senza una pagina vera aperta in un browser vero, e una WebView2 non si sostituisce con un finto come ho fatto per l'AI. Il taglio segue allora l'ordine in cui le cose si accendono. T5a è la prima: il browser, i portali, le ricerche salvate.*

**Cosa ho fatto**
- **Un motore del browser solo, per tutta l'applicazione.** La WebView2 in casa mia c'era già da T4b, ma fuori schermo, per stampare i PDF. Adesso ne serve una in piena vista, e le due devono convivere: c'è un `MotoreBrowser` che accende **un** ambiente sulla stessa cartella di navigazione e lo dà a chi lo chiede. Si accende alla prima richiesta e non alla nascita, così chi apre e richiude il programma senza cercare né stampare non si ritrova una cartella `webview2\` in casa.
- **I portali come dati, non come codice.** `ricerche.json` porta la tabella dei portali — un nome e un indirizzo con due segnaposto, `{cosa}` e `{dove}` — e le ricerche che metto da parte. Aggiungerne uno è una riga in un file, non una build. Vale la regola di `taratura.json`: se il file manca o è rotto si usano i valori di dentro e **lo si dice**. Con una cautela in più: uno schema che non sia `http` o `https` viene scartato, perché quella stringa finisce dritta nella barra di un browser vero.
- **Il pannello P3.** Il browser a tutta area, sopra le ricerche salvate e la riga per cercare, sotto la cattura. La pagina che si apre per prima è scritta da me e **non tocca la rete**: dice in tre righe come si usa e ricorda che il login si fa lì dentro. E le pagine che vorrebbero aprirsi in una scheda nuova — sui portali, quasi ogni risultato — si aprono **qui**: senza, quel clic non farebbe niente e il programma sembrerebbe rotto.

**Cosa ho imparato**
- **Il muro non è dove sembra.** Prima di scrivere il pannello ho voluto misurare se browser in vista e stampa fuori schermo potessero convivere. Due ambienti sulla stessa cartella *convivono*, finché hanno le stesse opzioni; se divergono, la creazione del secondo **riesce lo stesso** e il guasto arriva dopo, quando una vista prova ad accendersi, con un `Class not registered` che a chi legge non dice niente. Non ho quindi «fatto funzionare due ambienti»: ne ho lasciato uno solo, e quella divergenza è diventata impossibile invece che possibile e indecifrabile.
- **Un portale può chiudere.** Il capitolo 14 chiedeva di verificare sul campo gli indirizzi dei portali del primo rilascio, e ho scoperto che **InfoJobs non esiste più**: al suo indirizzo c'è un avviso che dichiara la piattaforma chiusa. Al suo posto è entrato Jooble, un aggregatore che raccoglie anche le agenzie per il lavoro — dove sta gran parte delle offerte per i ruoli che cerco io.
- **Perché serve un browser vero, misurato e non argomentato.** Lo stesso indirizzo di Jooble, chiesto da un programma qualunque, torna `403` con la sfida di Cloudflare; chiesto dalla WebView, apre i risultati. È la ragione del capitolo 6.1, che fino a ieri era una previsione e adesso è un numero.

**Dove ho faticato / cosa non era ovvio**
- **Un guasto che non si fa succedere.** Ho provato tre modi per far fallire l'accensione dell'ambiente — una cartella occupata da un file con quel nome, un percorso con i due punti in mezzo, un'unità che non esiste — e se li è presi tutti. Il ramo d'errore resta nel codice perché il giorno che scatterà sarà il giorno peggiore, ma **nessun collaudo lo attraversa**, e sta scritto: chi lo tocca lo sappia.
- **Tre comandi anonimi.** «◀», «⟳» e la casella dell'indirizzo non hanno un'etichetta accanto, e per Windows non avevano nome. Me ne sono accorto perché il mio strumento non li trovava — ma uno screen reader nemmeno. Adesso il nome ce l'hanno.

**Cosa ho deciso e perché**
- **L'unica navigazione che parte da sola è la pagina di casa**, che è roba mia e non chiede niente a nessuno. Chi naviga è l'utente: il programma non raccoglie in automatico, non fa login al posto suo e non vede le credenziali.
- **«Cattura annuncio» resta visibile e spento**, col cartellino di quando arriva. È la regola 03.8: un bottone che sparisce non insegna niente, uno spento che si spiega dice dove sta andando il programma.
- **Il buco l'ho dichiarato invece di chiuderlo di corsa.** Il giro che parte dal **menù dei portali** — scegli, scrivi, premi «Cerca» — non l'avevo mai provato sull'applicazione vera: il mio strumento sa premere bottoni e scrivere nelle caselle, ma non sceglie una voce da una tendina. Indeed l'avevo provato per caso, perché è il primo ed è già selezionato; gli altri tre solo dalla casella dell'indirizzo.

💡 *Mia intuizione / scelta ragionata* — Questa tappa mi ha insegnato che «l'ho provato» ha dei gradi. Avrei potuto chiudere T5a dicendo che i portali funzionavano — ed era vero, li avevo aperti tutti e quattro — nascondendo che l'ultimo pezzo di strada, quello che farà l'utente, l'avevo saltato perché il mio attrezzo non ci arrivava. Scrivere il buco nero su bianco è costato una riga; averlo scoperto sei mesi dopo, da un utente, sarebbe costato la fiducia. **Il limite di uno strumento non è una scusa: è una voce del collaudo che manca.**

### Step 2.12 — T5b: l'annuncio si prende dalla pagina, e uno strumento che mi ha mentito

*T5b è il pezzo per cui esiste il browser: premere «Cattura annuncio» e mandare all'analisi quello che ho davanti, invece di copiarlo a mano. È anche la tappa in cui ho imparato che un attrezzo di collaudo sbagliato è peggio di un attrezzo che manca — perché uno che manca lo sai, uno sbagliato ti risponde.*

**Cosa ho fatto**
- **Prima l'attrezzo, e il buco di T5a chiuso.** Ho insegnato al mio strumento a scegliere una voce da una tendina: la apre, ci clicca dentro **col mouse** e poi rilegge il menù per confermare. Col mouse perché gli altri due modi — il messaggio nativo di Windows, o la `Select()` dello schema di accessibilità — cambiano la voce mostrata **senza avvisare l'applicazione**: il collaudo canterebbe vittoria su qualcosa che non è successo. Poi il giro sui quattro portali dal menù, sull'applicazione vera: Indeed, Jooble, Subito.it e la ricerca generica aprono tutti la pagina giusta.
- **La lettura della pagina.** Una riga di JavaScript porta fuori titolo, indirizzo e testo visibile, in **un solo viaggio** — tre letture separate potrebbero cadere su tre pagine diverse, se nel frattempo si naviga. Il testo è `innerText` e non `textContent`: è «seleziona tutto → copia», e lascia fuori quello che il foglio di stile nasconde. C'è un tetto ai caratteri, e se taglia lo dice.
- **La provenienza.** Ogni opportunità catturata porta con sé **fonte e link**: il nome del portale quando lo conosciamo, il sito quando no, e l'indirizzo esatto della pagina. È quello che permette di tornare all'originale mesi dopo.
- **Il rifiuto garbato, e niente doppioni.** Se dall'analisi esce lo schema vuoto — è così che il prompt dice «questo non è un annuncio» — ci si ferma lì: nessun confronto pagato, niente scritto su disco, il testo lasciato dov'è. E premere due volte sulla stessa pagina non produce due candidature gemelle.

**Cosa ho imparato**
- **Uno strumento che mente costa più di uno che manca.** Con Jooble aperto, i bottoni chiamati «Cerca» erano **due**: il nostro e quello del sito, che nell'albero viene prima. Lo strumento premeva quello del portale, il portale rifaceva la sua ricerca, e l'applicazione non si muoveva — mentre il collaudo riferiva «premuto». Ho passato dieci minuti a cercare un difetto in un pannello sano. Lo stesso vale per le tendine, che si portano dentro un bottone tutto loro — la freccia, che Windows chiama «Apri» — e in P3 gli «Apri» erano tre, due finti e tutti e due prima di quello vero.
- **Su Indeed la lista *è* l'annuncio.** Il capitolo 6.4 dà per scontato che una pagina di risultati non contenga un annuncio, e quindi che l'analisi torni vuota. Su Indeed non succede, e non è un difetto: quel portale tiene **sempre** un annuncio aperto nel riquadro di destra, e il suo testo fa parte della pagina. La cattura prende l'annuncio che sto guardando, che è la cosa giusta — ma la promessa scritta nel capitolo vale solo per i portali fatti a griglia, e su Subito infatti il rifiuto è scattato al primo colpo.
- **Un annuncio vero non somiglia a uno inventato.** Il debito lasciato da T4 era proprio questo, e adesso è chiuso: quello pescato da Indeed portava dentro il banner dei cookie, i filtri, i menù e i titoli degli altri annunci, e l'analisi ne ha tirato fuori il ruolo giusto con la sua azienda e il suo contratto.

**Dove ho faticato / cosa non era ovvio**
- **Ho quasi rovinato il mio profilo con un comando.** Ho lanciato una catena di comandi senza aspettare che l'applicazione fosse pronta: il primo è andato a vuoto, gli altri sono partiti lo stesso, e «Cerca» — col pannello Profilo davanti — ha trovato il bottone di navigazione «🔍 Ri**cerca**», perché quelle cinque lettere stanno dentro l'altra parola. Risultato: «magazziniere» scritto sopra il contenuto di una casella del profilo vero. Su disco non è arrivato niente, perché non avevo salvato — ma è stato un caso, non una precauzione. Adesso la ricerca «per contenuto» pretende una **parola intera**: un pezzo di parola non è una somiglianza, è una coincidenza.
- **JSON dentro JSON.** Il copione restituisce una stringa, e chi lo esegue consegna la *rappresentazione JSON* di quel risultato: gli strati da togliere sono due, non uno. Se ne toglie uno solo si ottiene una stringa con le virgolette e le fughe dentro, che sembra un testo e non lo è.

**Cosa ho deciso e perché**
- **Il testo catturato si vede.** Non parte in silenzio verso l'AI: entra nella casella di P4, quella in cui si incolla a mano, e da lì l'analisi è la stessa di sempre. Un posto solo dove si analizza, e soprattutto la possibilità di **leggere quello che ho mandato**. Una cattura che analizzasse qualcosa di invisibile chiederebbe di fidarsi al buio, e questo programma non lo chiede mai.
- **Il doppione si ferma nella cattura, non nell'archivio.** Due candidature allo stesso posto sono legittime — magari per due ruoli diversi — e l'archivio deve continuare a tenerle entrambe. Ma la stessa **pagina** catturata due volte è un errore, e lì l'identità è esatta: è l'indirizzo. Il confronto è alla lettera, così se sbaglia sbaglia dalla parte giusta — un doppione in più è meno grave di una cattura buona rifiutata.
- **La cucitura per il banco.** La lettura della pagina sta dietro un'interfaccia, così il banco prova tutte le decisioni della cattura senza pretendere WebView2. La lettura vera ha due collaudi «Reale» che accendono il motore su una pagina scritta da me: nessuna rete, quindi ripetibili — un collaudo che aprisse un portale misurerebbe il portale, e diventerebbe rosso il giorno che quello cambia una classe CSS.

💡 *Mia intuizione / scelta ragionata* — Il capitolo 6.4 l'avevo scritto mesi fa immaginando i portali, e la realtà l'ha corretto in un punto: su Indeed non esiste la pagina di solo elenco che quel punto dà per scontata. Non è un difetto del programma né un errore del disegno — è che un disegno fatto a tavolino descrive **i portali che avevo in testa**, e i portali veri sono più disordinati. La lezione che mi porto è che il collaudo sul campo non serve a confermare il disegno: serve a scoprire dove il disegno parlava di un mondo che non c'è. E quando succede si corregge la carta, non il territorio.

### Step 2.13 — T5c: la Home, e le candidature che si ritrovano

*L'ultima gamba di T5 è quella che chiude un debito vecchio di tre giorni: da T4 ogni candidatura finiva nella sua cartella e ci restava, ma per tornarci dovevo incollare di nuovo l'annuncio. I dati c'erano, l'interfaccia non sapeva la strada. Con T5c la strada c'è — ed è la Home, il primo pannello che vedo quando apro il programma.*

**Cosa ho fatto**
- **La macchina degli stati.** Sei stati (nuova, interessante, generata, inviata, esito, scartata), le transizioni lecite scritte in un posto solo, e la data di ogni passaggio dentro `stato.json`. Chi prova a saltare da uno stato a un altro che il ciclo non prevede si sente dire di no dal motore, non dal pannello.
- **Il registro, che è un indice e non un archivio.** `registro.json` esiste per aprire la Home senza leggere venti cartelle, ma la verità restano **le cartelle**: se manca, se non si legge o se non torna coi fatti su disco, si rifà da capo. Chi guarda l'elenco è anche chi lo tiene in riga.
- **Il pannello P1.** La coda con stelle, azienda, ruolo, stato, provenienza e data; si ordina cliccando un'intestazione, si filtra con «Mostra», e in cima ci sono i contatori e lo stato del profilo. Una candidatura si riapre col doppio clic o col suo bottone, e torna in P4 com'era. Il programma adesso **si apre qui**.
- **«Scarta».** In P4, rosso, con una conferma che dice cosa succede davvero: non cancello niente, la cartella resta e la ritrovi nella Home, ma la do per chiusa e non si torna indietro. Dopo, la scheda resta a video coi comandi spenti.

**Cosa ho imparato**
- **Le mie sei candidature erano già lì, e non le ho toccate.** Le cartelle scritte da T4 e T5b il campo `stato` non ce l'hanno — nasce adesso — e sono comparse nella coda con lo stato **dedotto dai file che hanno**: se ci sono i documenti è generata, se ci sono i giudizi è interessante. Alla fine del collaudo ho riletto i sei `stato.json` uno per uno: nessuno era stato riscritto.
- **Un indice che si rigenera è un indice di cui mi posso fidare.** L'ho provato copiando a mano una cartella dentro `opportunita\`: è comparsa da sola nell'elenco. Poi l'ho cancellata: è sparita, senza lasciare una riga fantasma. Non è un dettaglio tecnico — è la differenza fra un file d'appoggio e un secondo posto in cui vive il dato.
- **Un comando distruttivo si collauda dicendo di no.** Le sei candidature nella mia cartella dati sono vere. Per provare «Scarta» ho risposto **No** su una di quelle e ho verificato che su disco non fosse cambiato niente; per il **Sì** mi sono fabbricato una candidatura di prova, l'ho scartata davvero e poi l'ho cancellata. Provare sul serio non vuol dire provare sui dati veri.

**Dove ho faticato / cosa non era ovvio**
- **Il mio strumento mi ha mentito una seconda volta.** Gli ho chiesto di premere «Apri la candidatura» sbagliando il nome dell'argomento: invece di rifiutare, ha accettato la chiamata con l'etichetta **vuota** — e un'etichetta vuota assomiglia a tutto, così ha premuto il **primo bottone della finestra** e mi ha risposto «Premuto «🏠 Home»». Stavolta non ha fatto danni perché ero già in Home, ma è lo stesso difetto dello Step 2.12 visto da un'altra parte. Adesso una chiamata a cui manca un argomento obbligatorio viene **rifiutata**, e il rifiuto dice quali argomenti servono.
- **Una lista non ha nome, e le sue righe hanno il nome sbagliato.** La coda arriva all'accessibilità di Windows come una tabella **senza nome**, e il nome di ogni riga è la sua **prima colonna** — che lì sono le stelle. Cercare «Rossi S.p.A.» fra le righe non trovava niente, perché in quel campo c'è scritto «★☆☆☆☆ 1,0». Le righe si cercano nel testo di tutte le celle.
- **«1 scartate».** Il primo scarto della mia vita col programma ha fatto comparire quella riga nei contatori: il numero era una variabile, la parola no. Sembra il difetto più piccolo del mondo, e infatti si vede **solo** la prima volta che uno fa una cosa — cioè quando guarda con più attenzione.

**Cosa ho deciso e perché**
- **Le cartelle sono la verità, il registro è comodità.** Se un giorno l'indice si corrompe, non ho perso niente: si butta e si rifà. Il contrario — un indice che tiene qualcosa che le cartelle non hanno — avrebbe voluto dire due posti da tenere allineati, e so già come finisce.
- **Non riscrivo i file dell'utente per uniformarli.** Aggiungere il campo `stato` dentro le cartelle già fatte sarebbe costato tre righe e mi avrebbe risparmiato la deduzione. Ma sono file **miei** in senso letterale, e un programma che li ritocca a mia insaputa — per comodità sua — è un programma di cui mi fiderei meno. La deduzione in lettura è il prezzo, e lo pago.
- **`inviata` ed `esito` esistono ma non si raggiungono.** Sono di T6, la tappa dell'email. Li ho messi comunque nello schema perché quando arriverà T6 aggiunga dei passaggi e non una migrazione dei file scritti fino a lì. Per la stessa ragione **non ho messo il contatore delle inviate**: fino a T6 direbbe sempre zero, e un contatore che non può muoversi non conta niente.
- **Nella Home si guarda, non si decide.** Lo scarto sta nella scheda della candidatura, non nell'elenco: una decisione che non si disfa non si prende da una lista, dove il dito è veloce e il contesto è una riga. E lo scarto è **terminale** — ripescarlo dall'interfaccia è finito in `idee_future.md`, la cartella intanto resta su disco per chi ci ripensa davvero.

💡 *Mia intuizione / scelta ragionata* — La decisione che ha reso tutto il resto più semplice è stata dichiarare `registro.json` **derivato**. Non è un'idea originale — è la regola per cui una cosa vera si tiene in un posto solo — ma qui ho visto cosa ci si guadagna davvero: mi ha permesso di **collaudare copiando e cancellando cartelle a mano**, invece di dover credere all'applicazione sulla parola. Un dato che vive in due posti non è ridondante: sono due dati, che prima o poi si contraddicono, e quel giorno nessuno sa quale dei due sia quello vero. Il registro invece non può contraddire niente, perché non sa niente che le cartelle non sappiano già.

### Step 2.14 — T5d: il CV preso dalla pagina che sto guardando

*La coda di T5 è la voce 2.1.3, quella che nel backlog era ferma da giugno con scritto accanto «complessità alta»: costruire il profilo dalla mia pagina LinkedIn invece che da un CV. È costata due bottoni. Non perché fosse facile, ma perché la strada era già stata fatta da qualcun altro — cioè da me, tre tappe fa.*

**Cosa ho fatto**
- **Il bottone che legge la pagina.** In P3, accanto a «Cattura annuncio», ora c'è **«Importa CV da questa pagina»**: legge la pagina aperta e la consegna alla scheda del profilo, che la struttura col turno `importa_cv` — lo stesso che legge un CV in PDF. Il pool **non l'ho toccato**: il prompt non sa da dove arriva il testo, e non deve saperlo. La finestra mostra prima P2 e poi gli chiede di leggere, come per l'annuncio catturato, così l'attesa si vede dove succede invece che su un pannello che sto per lasciare.
- **Nessun componente nuovo, davvero.** Il lettore di pagina era di T5a, la strutturazione era già indipendente dalla fonte, e `ImportProfilo.DaTestoAsync` stava lì dai tempi di T3 con un commento che diceva «questa porta serve quando il testo arriverà da altrove». È arrivato.
- **Lo scorrimento prima della lettura**, che il disegno non aveva previsto (sotto il perché).
- **La seconda porta, nel Profilo.** «Importa CV da un sito…» accanto a «Importa CV da un file…» — che prima si chiamava solo «Importa da un CV…», e adesso ognuno dei due dice da dove legge. Il bottone nuovo non legge niente: porta in Ricerca, e lì il pannello mi dice cosa fare.
- **Il banco.** Nove collaudi col primo pezzo e quattro con il secondo: **465 verdi**, da 452.

**Cosa ho imparato**
- **Una pagina non esiste finché non la scorri.** Letta com'era, la mia pagina profilo ha dato **2196 caratteri**: l'intestazione, una sola esperienza senza date né mansioni, zero studi, zero competenze. Un profilo dimezzato che si presenta come completo è peggio di un errore che si dichiara, perché non ha nessun modo di farsi notare. Scorrendo prima di leggere, la stessa pagina, con lo stesso bottone: **9681 caratteri**, l'esperienza intera con le sue date, dieci voci di formazione, nove competenze.
- **A scorrere non è la finestra.** `window.scrollBy` su quella pagina non muove niente — il contenitore che scorre è un altro — e me ne sono accorto solo perché il numero di caratteri non cambiava di una virgola. Adesso chiedo a `document.scrollingElement` chi scorre davvero, e se non risponde cerco il contenitore più grande che possa farlo.
- **Il primo «sono in fondo» è una bugia.** Una pagina non ancora caricata è corta: il fondo arriva dopo due passi, e in quel momento non è stato aggiunto ancora niente. Scendo finché non me lo conferma **tre volte di fila**, e mi fermo se non si muove affatto.
- **Il numero di caratteri che scrivo sullo schermo non è un vezzo da programmatore.** È il solo modo che ho di accorgermi che alla strutturazione è andata poca roba **prima** di guardare il profilo e crederlo intero. Oggi lo scorrimento lo faccio io, ma un sito può sempre caricare in un modo che non prevedo.

**Dove ho faticato / cosa non era ovvio**
- **Due tentativi buttati, uno per ogni cosa che credevo di sapere.** Il primo perché ero convinto che scorrere una pagina fosse una riga; il secondo perché mi sono fidato del primo «sono arrivato in fondo». Entrambi gli errori davano lo stesso sintomo — un profilo povero — e nessuno dei due dava un errore.
- **Lo strumento mi ha fotografato la finestra sbagliata.** Gli ho chiesto una schermata dell'applicazione e mi ha ritratto il terminale, che era davanti: per un attimo ho creduto a un difetto di impaginazione che non esisteva. Si chiama due volte di fila, e ora sta scritto nel suo README — insieme a una cosa che non sa fare e che oggi mi sarebbe servita: **ridimensionare la finestra**.
- **Un difetto che non ho visto, ma che ho contato.** Con quattro bottoni nella fascia del Profilo, alla larghezza minima della finestra (1150 px) la fila di sinistra e quella di destra **si sovrappongono**. Non l'ho visto — l'applicazione si apre massimizzata e lì è tutto a posto — l'ho scoperto rifacendo a mano il conto che fa il codice: sinistra finisce a 948, destra comincia a 638. C'era già prima di me, con tre bottoni: 108 px invece di 310. È in `in_sospeso.md`, perché una fascia di comandi dovrebbe andare a capo quando lo spazio non basta.

**Cosa ho deciso e perché**
- **La scelta sta nel Profilo, l'atto nella Ricerca.** Il bottone che legge deve stare dove c'è il browser: uno che leggesse dal Profilo troverebbe che non c'è nessuna pagina aperta. Ma la **scelta** fra le tre strade per costruire il profilo deve stare dove il profilo si costruisce — fino a ieri la terza si trovava solo entrando in un pannello chiamato «Ricerca», dove nessuno che voglia compilare il profilo andrebbe a cercarla.
- **Non controllo che la pagina sia LinkedIn, e nemmeno che sia la mia.** Il primo controllo sarebbe inutile: la strutturazione legge altrettanto bene la pagina «chi sono» di un sito personale. Il secondo è impossibile da fare onestamente, perché l'indirizzo di un profilo non dice di chi sia. Fingere un controllo che non esiste è peggio che non averlo: «solo la tua pagina» resta una regola mia, e il programma me la **dice** — nella pagina di casa e nel suggerimento del bottone — invece di far finta di verificarla.
- **Scorre solo l'import del CV.** La cattura dell'annuncio legge la pagina com'è, ed è così che l'ho collaudata su quattro portali a T5b: aggiungerle uno scorrimento cambierebbe, su tutti, quello che finisce nell'analisi — e nessuno me l'ha chiesto. Un collaudo tiene ferma la distinzione, così nessuno le riavvicina per distrazione.
- **Ho provato sui miei dati veri senza salvare niente.** Il profilo che esce da LinkedIn ha una esperienza contro tre e non ha né email né telefono, perché quel sito non li pubblica: sostituirlo al mio sarebbe stato un peggioramento. Ho verificato l'impronta dei file prima e dopo — profilo, storico e le sei candidature identici — e ho chiuso l'applicazione senza passare dal salvataggio. Da lì è nata un'idea per il futuro: un import che **fonde** invece di sostituire, che è poi lo stesso meccanismo della sessione di aggiornamento.

💡 *Mia intuizione / scelta ragionata* — Nel backlog quella voce diceva «complessità alta», ed era vero **quando l'ho scritta**: allora immaginavo di scaricare una pagina da un indirizzo, e non avrebbe funzionato — un profilo si costruisce in JavaScript e sta dietro un accesso. Poi ho costruito il browser interno per un altro motivo (gli annunci), e la strutturazione era già indipendente dalla fonte per un motivo ancora diverso (i quattro formati di file). Il giorno in cui sono tornato a quella voce, la strada era già asfaltata da capo a fondo e mancava il cartello. La lezione che mi porto è che **le stime di complessità invecchiano più in fretta delle idee**: quello che costa una funzione non dipende da quanto è ambiziosa, ma da quanta strada è già stata fatta per altri motivi — e questo si può sapere solo rileggendo il backlog con in mano il programma di oggi, non quello di quando l'ho scritto.

### Step 2.15 — Tre etichette, e tutti i posti in cui vive il nome di un bottone

*Uno step piccolo, e lo dico subito: ho cambiato il nome a tre bottoni della scheda Profilo. Nessuna funzione nuova, nessun prompt toccato. Lo scrivo lo stesso perché ha mostrato una cosa che non avevo mai misurato — quanti posti conoscono il nome di un bottone — e perché una delle tre etichette ha peggiorato un difetto che avevo annotato tre giorni fa.*

**Cosa ho fatto**
- **Le tre etichette.** «Importa CV da un file…» è diventato **«IMPORTA CV DA UN FILE»**, «Importa CV da un sito…» è diventato **«IMPORTA CV DA LINKEDIN»**, «Costruiscilo con il dialogo» è diventato **«COSTRUISCI IL TUO CV - DIALOGO GUIDATO»**. Maiuscolo e più esplicito: sono i tre modi di costruire il profilo, ed è la fascia che un utente nuovo guarda per prima.
- **I sette posti che quel nome lo sapevano.** Non è bastato il `Designer`: l'etichetta del primo bottone si riscrive anche nel codice, perché durante la lettura di un CV diventa «Annulla lettura» e alla fine deve tornare com'era; due collaudi la verificano **parola per parola**; e la pagina di casa del browser, che è scritta da me, la cita per spiegare che l'import dalla pagina fa la stessa cosa. Più due commenti che la nominano.
- **Il bottone allargato.** «COSTRUISCI IL TUO CV - DIALOGO GUIDATO» sono 38 caratteri maiuscoli contro 27: nei 200 px di prima usciva tagliato a metà. Portato a **300**.
- **Il collaudo.** 465 su 465 verdi, e poi l'applicazione vera aperta sulla scheda Profilo per guardare la fascia in faccia.

**Cosa ho imparato**
- **Il nome di un bottone non è una stringa: è un piccolo contratto.** Lo conoscono i collaudi che lo controllano alla lettera e i testi che lo citano per dire all'utente cosa premere. Una frase che spiega «fa la stessa cosa di «Importa CV da un file…»» diventa **falsa** nel momento in cui quel bottone si chiama in un altro modo, e non se ne accorge nessuno: non è codice che si rompe, è una promessa che smette di combaciare con la realtà.
- **Il banco vede il testo, non la sua larghezza.** I collaudi confrontano l'etichetta carattere per carattere e restano verdi anche se a schermo esce mozzata: la larghezza dei bottoni è un numero fisso nel `Designer`, e nessuno lo confronta col testo che ci deve stare dentro. L'unico modo di saperlo è stato guardare la finestra.

**Dove ho faticato / cosa non era ovvio**
- **Un'etichetta che promette meno di quello che fa.** Il bottone chiamato oggi «IMPORTA CV DA LINKEDIN» non legge LinkedIn: porta nel pannello Ricerca, dove il browser legge **qualunque** pagina che racconti un percorso — a T5d avevo deciso apposta di non controllare che il sito fosse LinkedIn, perché la strutturazione legge altrettanto bene la pagina «chi sono» di un sito personale. Il nome nuovo restringe ciò che il bottone sa fare. L'ho scelto lo stesso, perché è il caso d'uso vero e perché il pannello che accoglie continua a dire «su LinkedIn, o su un altro sito che racconti il tuo percorso»: la porta è più stretta del corridoio, ma il corridoio è ancora tutto lì.
- **Ho peggiorato un difetto già a verbale.** Lo Step 2.14 aveva annotato che alla larghezza minima della finestra le due file di comandi del Profilo si sovrappongono. Ho rifatto quel conto con il bottone allargato, e la prima volta l'ho sbagliato: avevo usato la larghezza del logo a finestra grande (261 px), dimenticando che sotto i 1350 px il pannello del logo passa in **modalità compatta** e ne misura 130 — cioè che proprio nel caso che sto misurando la fascia comincia in un altro punto. Rifatto per bene: alla larghezza minima la fila di sinistra finisce a **1048** px (erano 948) contro i 638 di quella di destra, quindi la sovrapposizione passa da 310 a **410 px**. I 100 px del bottone allargato sono finiti tutti lì. A finestra massimizzata non si sovrappone nulla, ma il margine fra le due file scende da 345 a 245 px. Il difetto non è nato qui, ma questa modifica lo rende peggiore, e sta scritto in `in_sospeso.md` col numero aggiornato invece che con quello vecchio.

**Cosa ho deciso e perché**
- **Ho allargato il bottone invece di accorciare la frase.** L'etichetta è quella che volevo io; a cedere è il numero nel `Designer`, non le parole. Che poi è la stessa scelta della fascia delle azioni fin da T3 — «a cedere il posto sono i bottoni, che di spazio ne chiedono poco».
- **Ho aggiornato anche i commenti che citavano il vecchio nome.** Il codice non si riscrive per allineamento, ma un commento che nomina un bottone inesistente non è allineamento: è una bugia lasciata lì per il prossimo che legge.
- **Non ho toccato «Importa CV da questa pagina».** È il bottone di P3, ed è l'unico dei quattro che nessuno mi ha chiesto di cambiare: rinominarlo «per coerenza» sarebbe stato allargare da solo il perimetro di ciò che mi era stato chiesto.

💡 *Mia intuizione / scelta ragionata* — La cosa che mi porto da uno step di tre stringhe è il conto della sovrapposizione. Nessuno me l'aveva chiesto, e nessuno se ne sarebbe accorto: l'applicazione si apre massimizzata, la fascia è perfetta, i collaudi sono verdi. Ma quel difetto era **scritto** — l'avevo annotato io tre giorni fa — e la prima cosa da fare, prima di dire «fatto», era andare a vedere se quello che stavo facendo lo toccava. Lo toccava, e in peggio. Un lavoro finito non è un lavoro che funziona: è un lavoro di cui so anche cosa ha spostato attorno a sé.

### Step 2.16 — La porta opposta al salvataggio

*Ho chiesto una cosa sola: accanto a «Salva profilo» un tasto che elimini il profilo per davvero, e che azzeri tutti i campi dell'applicazione — ma senza toccare gli annunci della Home, che con il profilo non c'entrano. Sembrava un bottone. Era, come al solito, il posto in cui si scopre in quanti angoli diversi vive una cosa sola.*

**Cosa ho fatto**
- **Il bottone.** «ELIMINA PROFILO - DEFINITIVO», di **livello 6** — il colore critico che il progetto aveva definito a T1 e che fino a ieri non aveva mai usato nessuno. Sta nella fascia dei comandi del Profilo, ma **una riga sopra** gli altri e all'estrema destra: vicino a «Salva profilo» perché è lì che lo si cerca, non in fila con lui perché quello si preme cento volte e questo una sola.
- **Che cosa manda via.** La cartella `profilo\` **intera**: il profilo, lo storico delle versioni, il 📄 CV base con i suoi `.docx` e `.pdf`, le copie `profilo.rotto-…` e qualunque temporaneo. Non un elenco di nomi: la cartella.
- **Che cosa non tocca.** Le candidature, il registro della Home, le ricerche salvate, la taratura, i modelli, i dati di navigazione. Sul disco quei due mondi erano già separati da T4, e questa è la prima volta che quella separazione **serve a qualcosa**.
- **La conferma.** Non una `MessageBox`: una finestra nuova, `FinestraConfermaCritica`, che elenca cosa sparisce e cosa resta e per accendere il bottone chiede di **riscrivere la parola «TrovaLavoro»**. Invio non conferma niente, Esc annulla. È generica: le eliminazioni delle Impostazioni (T9) useranno questa.
- **L'azzeramento di tutta l'applicazione.** Il profilo non vive solo nella sua scheda: eliminandolo si svuotano anche i campi e la linguetta «Testo letto» di P2, il dialogo guidato di P5 e il 📄 CV base in mostra in P6 — mai i documenti di una candidatura — e la Home rilegge.
- **Il banco.** Tredici collaudi nuovi: **478 verdi**, da 465. E la versione a 0.3.018.

**Cosa ho imparato**
- **«Definitivo» è una promessa sul disco, non sullo schermo.** Se cancello i file che conosco per nome, la prima volta che qualcuno mette un file nuovo in quella cartella la promessa diventa falsa in silenzio: resterebbe indietro un `profilo.rotto-…` con dentro tutto il mio racconto, proprio il file che l'applicazione crea quando le cose vanno male. Si cancella la casa, non i mobili che mi ricordo.
- **Un «sei sicuro?» non è una domanda, è una formalità.** A furia di rispondere di sì alle finestrelle si impara a scacciarle come mosche. La parola da riscrivere costa tre secondi e cambia la natura del gesto: non si può fare per riflesso.
- **Il mio profilo vive in quattro posti a video, non in uno.** La scheda, il dialogo che l'ha costruito, il CV base che ne è il ritratto, il cruscotto che dice a che punto sono. Ne avevo cancellato uno e gli altri tre continuavano a mostrarlo: il pannello che l'ha eliminato **non conosce nessuno**, e dev'essere la finestra a girare la voce.
- **Il livello 6 esisteva da prima del suo primo bottone.** Il capitolo 03 lo aveva definito quando ancora non c'era niente da eliminare. Oggi che serviva, non ho dovuto decidere né inventare un colore: era già scritto, e sono andato a leggerlo.

**Dove ho faticato / cosa non era ovvio**
- **«Ricomincia» sembrava la risposta e non lo era.** Per azzerare il dialogo guidato c'era già un comando pronto — e avrebbe fatto una chiamata all'AI per riaprire la conversazione dal primo turno. Cioè: cancello tutto, e il programma mi chiede «ciao, come ti chiami?». Ho dovuto scrivere un `Dimentica` che butta la conversazione **senza aprirne un'altra**: è la stessa azione vista da un'altra intenzione, e le intenzioni non si riciclano.
- **Il bottone non l'ho mai premuto per davvero.** L'ho premuto sull'applicazione vera, ho scritto la parola, ho visto «Elimina il profilo» accendersi in rosso — e ho annullato: dall'altra parte c'era il mio profilo con il suo storico e le sei candidature. Per provarlo fino in fondo serve poter avviare l'applicazione su una **cartella dati usa-e-getta**, che oggi non si può: `avvia_app` usa sempre quella vera. È scritto in `in_sospeso.md`, e la copia di sicurezza che mi ero messo da parte prima di cominciare l'ho tenuta fino alla fine.
- **Ho peggiorato la solita fascia, e stavolta solo di lato.** A finestra grande il bottone nuovo sta sopra e non ruba spazio a nessuno: il margine fra le due file resta 245 px. Ma sotto i 1350 px il logo passa in compatta, la fascia si abbassa a 68 e due righe non ci stanno più: il bottone scende in riga e la sovrapposizione già nota passa da 410 a **676 px**. L'ho misurato prima di dire fatto, come la volta scorsa, e sta nel debito con il numero nuovo.

**Cosa ho deciso e perché**
- **Via anche lo storico, e anche il CV base.** Se lascio lo storico, il profilo è ancora tutto lì e «definitivo» è una bugia; se lascio il CV base, dentro ci sono nome, contatti ed esperienze, e non ho cancellato niente. Ne accetto la conseguenza: ogni candidatura annota **con quale versione** di profilo furono scritti i suoi documenti, e quei riferimenti restano a puntare nel vuoto. La candidatura si riapre lo stesso — i suoi file sono suoi — ma alla domanda «con quale profilo l'avevo scritta?» non si risponde più. È il prezzo, e chi preme quel bottone vuole proprio quello.
- **Le candidature restano, e lo dico prima.** Era la richiesta di partenza, ed è diventata metà del testo della finestra: non basta non cancellarle, bisogna che l'utente lo sappia **prima** di confermare, non che lo scopra dopo con sollievo o con rabbia.
- **Acceso solo se c'è qualcosa da eliminare.** Senza profilo su disco e senza niente scritto nei campi, il bottone è spento e dice perché. Un tasto rosso che non ha niente da fare insegna solo a non fidarsi del colore.
- **Ho scritto prima il capitolo, poi il codice.** Cap. 11.5 e cap. 03: perimetro, cosa resta, livello della conferma. Poi il codice ha fatto quello che c'era scritto. È la regola del progetto, e su una funzione che cancella dati è la volta in cui conviene di più.

💡 *Mia intuizione / scelta ragionata* — La cosa che mi porto è che questo bottone non ha aggiunto una capacità: ha **reso visibile una separazione che c'era già**. Profilo di qua, candidature di là, in due cartelle diverse, fin da quando ho deciso dove mettere i file — per ordine, non per prudenza. Il giorno in cui ho voluto poter buttare l'uno e tenere l'altro, quella scelta di ordine è diventata una scelta di potere: se il CV base fosse finito dentro la cartella di una candidatura, oggi «elimina il profilo» sarebbe stata una funzione da scrivere a mano, file per file, con la certezza di dimenticarne uno. Le decisioni di dove mettere le cose non si pagano quando si prendono: si incassano anni dopo, quando qualcuno chiede qualcosa che allora non esisteva.

### Step 2.17 — Quattro debiti prima di cominciare

*Prima di aprire T6 sono andato a rileggere `in_sospeso.md`, che è la regola: a inizio tappa si guarda se qualcosa si può chiudere adesso. Ne ho trovate quattro che si potevano chiudere subito, e tre di loro T6 le avrebbe rese più care — non impossibili, più care. Le ho fatte tutte e quattro prima di scrivere una riga della tappa nuova.*

**Cosa ho fatto**
- **`--dati`, la cartella di prova.** L'applicazione partiva sempre da `%APPDATA%\TrovaLavoro`, e provare una funzione che cancella voleva dire rischiare i miei dati veri. Ora un argomento sulla riga di comando la sposta in blocco su un'altra radice. Tre cose la tengono onesta: **si vede** (una cartella diversa da quella di sempre è dichiarata nel titolo della finestra e nella barra di stato), **non impedisce l'avvio** (una radice illeggibile ripiega sulla predefinita dicendolo, un argomento sconosciuto si segnala e si scavalca) e lo ha imparato anche lo strumento di collaudo.
- **La fascia dei comandi che va a capo.** Il difetto che mi ero annotato due volte, peggiorandolo due volte: alla larghezza minima le due file di bottoni si incontravano in mezzo, fino a **676 px** di bottoni sopra altri bottoni. Adesso la fascia si comporta come qualunque barra di comandi — se le due file non ci stanno insieme vanno su righe separate, e una riga troppo lunga si spezza ancora. L'area dei dati cede lo spazio, non la leggibilità dei comandi.
- **La stessa geometria, in un posto solo.** Scrivendo la fascia per il Profilo mi sono accorto che quella disposizione era **la quinta copia** della stessa cosa: Home, Profilo, Opportunità, Dialogo e Documenti se la disponevano ognuno per conto suo. `FasciaDeiComandi` adesso possiede la geometria; i pannelli dichiarano solo quello che sanno loro — quali comandi stanno a sinistra, quali a destra, quale è critico.
- **Il filtro per stelle.** Il cap. 07.3 prometteva una coda filtrabile «per stato e stelle»: a T5c avevo fatto la metà sullo stato. La tendina dice **«almeno N»** e non «N», perché chi ha una coda lunga chiede quali valgono da 3 in su. E i due filtri si intersecano invece di sostituirsi: rispondono a domande diverse.
- **L'export del registro.** In CSV per il foglio di calcolo, in markdown per leggerlo. Esce **quello che è a schermo**, filtrato e ordinato com'è: la forma leggibile si porta il proprio perimetro in cima (data, filtri, «3 di 12»), così una vista non si scambia per l'archivio intero.
- **Il banco.** Da 478 a **514 collaudi** verdi, in cinque passaggi.

**Cosa ho imparato**
- **Un difetto che si aggrava da solo va chiuso, non riannotato.** Quella sovrapposizione l'avevo scritta tre volte in `in_sospeso.md`, e ogni volta con un numero più grande: 310, 410, 676. Non peggiorava perché nessuno ci badava — peggiorava **perché il programma cresceva**, e ogni bottone nuovo la spingeva avanti. Un debito così non aspetta il momento buono: se lo lasci lì, il momento buono si allontana da solo.
- **Cinque copie della stessa geometria erano cinque occasioni di sbagliare.** Nessuna delle cinque era rotta. Ma la sesta lo sarebbe stata, e il difetto lo avrei visto solo in un pannello — quello dove qualcuno aveva aggiunto un bottone.
- **Un file che esce dall'applicazione ha due lettori diversi.** Il CSV lo apre un programma, il markdown lo legge una persona: al primo una frase di intestazione **rompe la tabella**, al secondo serve per non credere di avere tutto. La stessa informazione, due destini opposti.

**Dove ho faticato / cosa non era ovvio**
- **Excel legge l'UTF-8 con la codepage di sistema.** Un CSV corretto si apriva con «perché» diventato «perchÃ©» su ogni riga. Servono due cose che non c'entrano niente col contenuto: il **punto e virgola** come separatore (Excel usa quello di elenco di Windows) e il **segno d'ordine dei byte** in testa al file. Sono dettagli da impianto idraulico, e decidono se il file si apre dritto o storto.
- **Le stelle mancanti non sono zero.** Nell'export ho lasciato la cella **vuota**: zero vorrebbe dire confrontata e senza valore, che non è la stessa cosa di non ancora confrontata. È la stessa distinzione dell'anti-invenzione, applicata a un foglio di calcolo.

**Cosa ho deciso e perché**
- **Ho chiuso i debiti prima, non dopo.** Avrei potuto cominciare T6 e infilarli in mezzo. Ma `--dati` mi serviva **durante** T6 per provare senza rischiare, e la fascia dei comandi doveva reggere il bottone nuovo che T6 avrebbe aggiunto a P7: farli dopo voleva dire farli due volte.
- **La riga di comando cresce, quindi l'ignoto si scavalca.** T8 aggiungerà `--mcp`. Un eseguibile che rifiuta di partire per una parola in più è peggio di uno che spiega e prosegue.

💡 *Mia intuizione / scelta ragionata* — La cosa che mi porto è che `in_sospeso.md` ha fatto il suo mestiere per la prima volta sul serio. Non è un elenco di rimpianti: è la lista delle cose che **so** di dover fare, e leggerla a inizio tappa ha cambiato l'ordine del lavoro invece di produrre solo un po' di senso di colpa. Tre di quei quattro debiti sono diventati strumenti della tappa nuova — la cartella usa-e-getta con cui l'ho collaudata, la fascia che ha retto il bottone in più. Un debito pagato al momento giusto non è una spesa: è un attrezzo che ti ritrovi in mano.

### Step 2.18 — T6: l'email che nasce dalla lettera, e un esempio che smentiva la sua regola

*L'ultimo miglio: da qui la candidatura o parte o resta una cartella di file. Il capitolo 7 lo avevo scritto mesi fa e diceva già la cosa più importante — che il programma **non spedisce**. Prepara un messaggio e lo consegna al mio programma di posta, dove sono già autenticato. Scriverlo è stato più facile che accettarlo.*

**Cosa ho fatto**
- **Pool 1.04, due prompt nuovi.** `email_candidatura` prende la ✉️ lettera **già generata** e la accorcia: profilo → lettera → email, e ogni anello stringe invece di aggiungere. `classifica_documenti` smista una cartella di file in `cv`, `attestato`, `lettera`, `altro` — serve agli allegati, ed è il pezzo che ho usato due step più avanti.
- **Un attrezzo per il rito del bump.** Il comando «Sigilla pool» sta nelle Impostazioni, che sono di T9, ma i prompt si toccano da T2: `strumenti/sigilla-pool` chiama **lo stesso codice** con cui il caricatore verifica le impronte, così la regola delle impronte resta in un posto solo.
- **Il file `.eml`, scritto a mano.** Niente librerie: `MailMessage` di .NET sa consegnare un messaggio a un server SMTP ma non salvarlo come file. Il messaggio esce con l'intestazione **`X-Unsent`**, che i programmi di posta riconoscono e aprono come una finestra di composizione pronta per «Invia».
- **Il pannello P7.** Destinatario (vuoto se l'annuncio non ne portava uno: il programma non lo inventa mai), oggetto, corpo e allegati da spuntare, con il PDF già acceso e il DOCX spento. La bozza si salva in `email.json`, e riaprendola **l'AI non viene disturbata**.
- **Pool 1.05, poche ore dopo.** La prima email vera ha fatto vedere un difetto in tre parole: il prompt vietava di dare del tu all'azienda e due sezioni più giù ne dava un esempio che lo faceva — *«In allegato **trovi** il mio CV»*. Il modello ha seguito l'esempio, e per giunta l'ha storpiato: *«In allegato **trovo** il mio CV»*.
- **Il banco.** Da 514 a **539 collaudi** verdi, di cui 13 sul solo formato del messaggio.

**Cosa ho imparato**
- **In un'istruzione l'esempio pesa più della regola.** Se i due si contraddicono vince l'esempio, perché è la cosa concreta da imitare. Da oggi, quando rileggo un prompt, comincio dagli esempi e mi chiedo se uno di loro stia insegnando l'opposto di quel che c'è scritto sopra.
- **Tre trappole MIME che l'italiano di ogni giorno non incontra.** Le righe finiscono con CRLF; le intestazioni sono ASCII, quindi un oggetto accentato va codificato o arriva a pezzi; un allegato dal nome accentato vuole la forma RFC 2231 accanto a quella semplice. E un a capo dentro l'oggetto farebbe leggere come corpo tutto quello che segue — l'AI può produrlo, quindi si appiattisce.
- **Il vantaggio di collaudare un file che è testo.** Ogni promessa fatta al formato si può rileggere: intestazioni, `X-Unsent`, confini delle parti, allegati. Tredici collaudi non sono un lusso, sono la cosa più facile della tappa.
- **Gli a capo che Windows non mostra.** L'AI scrive `\n`, e una casella multiriga li fa vedere solo se sono CRLF: il messaggio compariva tutto attaccato — «Cordiali saluti,Mirco Parenti» — e chi lo rilegge crede che sia stato scritto così. Visto sull'applicazione vera, non al banco.

**Dove ho faticato / cosa non era ovvio**
- **Il messaggio non appartiene alla fila.** La pipeline di T4 si percorre da sé fino ai documenti. L'email no: chiede a chi mandare, quali allegati, e se spedire. Un passo che aspetta delle scelte non sta dentro una fila automatica, e ho tenuto il compositore **accanto** alla pipeline invece che dentro.
- **Il `.eml` che si allegava a sé stesso.** Alla seconda preparazione il messaggio scritto la prima volta è un file come gli altri nella cartella dei documenti: senza escluderlo finiva dentro la copia successiva, e ogni giro raddoppiava.
- **Una `MessageBox` in un banco aspetta per sempre.** «L'ho spedita» chiede conferma, e quella conferma non si può collaudare. Ho staccato l'**atto** dalla domanda: la conferma sta nel bottone, quel che succede dopo un sì sta in un metodo suo — che il banco può chiamare.

**Cosa ho deciso e perché**
- **Nessun bottone «Invia», e non è una rinuncia.** L'invio con utente e password si sta chiudendo ovunque: Microsoft ha spento l'autenticazione di base su SMTP, Google ha annunciato di voler togliere le password per le app. Rientrare vorrebbe dire OAuth 2.0, cioè un progetto a sé — registrazione dell'applicazione presso ogni provider, finestra di consenso, rinnovo dei permessi. In cambio: **nessuna password di posta da custodire**, che è anche la ragione per cui la chiave API è rimasta l'unico segreto del programma.
- **La bozza si salva, perché qui l'utente scrive davvero.** È l'unico punto della candidatura in cui le parole sono sue: riaprirla domani non deve voler dire ricominciare, e riscriverle sopra con una chiamata all'AI sarebbe il modo peggiore di essere utili.
- **Delle spunte mi fido, dei file no.** Rientrando in P7 l'elenco si rifà dai file che ci sono **adesso** su disco, e dalla bozza si riprendono solo le spunte: un documento cancellato nel frattempo non torna in vita perché un elenco lo nominava.

💡 *Mia intuizione / scelta ragionata* — Il difetto del Pool 1.05 mi ha insegnato più della funzione intera. Avevo scritto una regola chiara e un esempio comodo, e il modello ha imitato l'esempio: non perché sia distratto, ma perché **un esempio è più concreto di un divieto**. È esattamente quello che farebbe una persona che impara un mestiere guardando. Da allora ho un metodo in più per rileggere un prompt — leggerlo come lo leggerebbe qualcuno che deve *fare* quella cosa, non qualcuno che deve *capirla* — e la conferma che il collaudo vero non è il banco: quel difetto è saltato fuori alla prima email scritta sul serio, con il mio nome in fondo.

### Step 2.19 — La chiave che non stava più in una variabile d'ambiente

*Da T2 la chiave dell'API la leggevo da una variabile d'ambiente. Va benissimo per me, che apro un terminale; non vuol dire niente per chiunque altro apra l'eseguibile con un doppio clic. Era un debito assegnato a T6 da un anno di calendario e da due tappe, e il collaudo di tappa lo chiede espressamente: che la chiave non compaia in chiaro né su disco né nei log.*

**Cosa ho fatto**
- **`segreti.bin`, cifrato da Windows.** La chiave vive nella cartella dati, cifrata con la **protezione dati di Windows** e legata all'account che l'ha salvata: quel file copiato su un altro PC, o letto da un altro utente, non si apre. Nessuna crittografia inventata da me, e nessun pacchetto in più — sta già dentro .NET per le applicazioni Windows.
- **Una finestrella al primo avvio.** Se la chiave non c'è, prima ancora che i pannelli si colleghino al motore, l'applicazione la chiede: dice a cosa serve, dove si prende, e che resta su questo computer. La casella è mascherata, con una spunta per rileggere quel che si è incollato.
- **«Non adesso» è una risposta.** Chi rimanda entra lo stesso: profilo, candidature e documenti si vedono, e restano spente le sole funzioni che chiamano l'AI.
- **Tre posti dove cercarla, in ordine.** Quella dichiarata da chi avvia (che è la porta del banco), poi il file cifrato, poi la variabile d'ambiente di sempre. E una **nota all'avvio che dice da dove è arrivata**, con la chiave mascherata: `sk-ant-…1234`.
- **`--chiave` per rifarla.** Le Impostazioni sono di T9: finché non ci sono, riavviare con questo argomento fa ricomparire la finestra anche quando una chiave c'è già.
- **Il banco.** Da 539 a **566 collaudi** verdi.

**Cosa ho imparato**
- **«Alla prima chiamata all'AI» non poteva esistere.** Era la mia idea di partenza: chiedere la chiave quando serve. Ma senza chiave i pannelli **spengono i bottoni** che chiamano l'AI — quindi non c'è nessun bottone da premere che faccia scattare la richiesta. L'unico posto possibile era l'avvio, e me l'ha detto il codice che avevo già scritto io mesi fa.
- **Su una finestra mai mostrata, `Visible` è sempre falso.** Vale per i controlli figli, e me ne sono accorto perché il **layout** ne dipendeva: la riga «adesso ne è salvata una» non riservava mai il suo spazio, perché il calcolo avveniva nel costruttore — cioè proprio quando la finestra non è ancora a video. Lo stato voluto e lo stato visibile sono due cose diverse, e per il layout serve il primo.
- **E un bottone di una finestra mai mostrata non si lascia premere.** `PerformClick` vuole un controllo selezionabile. È la stessa lezione di «l'ho spedita», arrivata da un'altra porta: l'**atto** va staccato dal gesto, se lo si vuole collaudare.
- **Un `testhost` sopravvissuto blocca la compilazione dopo.** Se il banco viene interrotto a metà, il processo che lo faceva girare resta vivo e tiene bloccata la libreria: il giro successivo fallisce con dieci tentativi di copia e un errore che parla di file bloccati, e sembra un guasto del progetto. È un avanzo del giro di prima, e adesso sta scritto nel README dello strumento.

**Dove ho faticato / cosa non era ovvio**
- **Chi vince fra il file e la variabile.** Ci ho ragionato più del previsto. Ho messo il **file davanti**, perché è la volontà più recente dell'utente — l'ha digitata lui nell'applicazione — mentre la variabile è l'eredità di T2 e resta per lo sviluppo. Il rischio è la sorpresa muta: «ho cambiato la variabile e non succede niente». Per questo la provenienza finisce nel resoconto d'avvio.
- **Una chiave sulla riga di comando resterebbe scritta.** `--chiave` **non** prende un valore: se qualcuno lo passa lo stesso, si scarta dicendolo — e senza ripeterlo nell'avviso, perché quell'avviso finisce nella barra di stato, cioè sotto gli occhi di chiunque guardi lo schermo.
- **Il file che c'è ma non si apre.** Non è un «non ce l'ho»: è il caso del file copiato da un altro PC, e l'utente quel file lo vede su disco e lo crede buono. Torna come caso a parte e diventa un avviso che dice il perché.

**Cosa ho deciso e perché**
- **Nessuna prova della chiave.** Verificarla vorrebbe dire spendere una chiamata mentre l'utente sta ancora entrando, e comunque non distinguerebbe una chiave sbagliata da una rete che non c'è. Della **forma** dico quel che vedo — le chiavi di Anthropic cominciano per `sk-ant-` — ma avverto senza bloccare: chi ne ha una fatta in un altro modo la sa usare meglio di me.
- **Se il salvataggio fallisce, la chiave vale per la sessione.** Disco pieno o cartella in sola lettura: l'utente l'ha data per lavorare adesso, e perderla in silenzio sarebbe la reazione peggiore. Glielo dico, e vado avanti.
- **Niente pannello Impostazioni adesso.** Sarebbe stato il posto naturale, ed è di T9. Una finestra e un argomento della riga di comando bastano, e non lasciano un pannello a metà da riprendere.

💡 *Mia intuizione / scelta ragionata* — Mi porto via una prova su me stesso, più che sul codice. Ho scritto la finestrella con l'idea di chiederla «quando serve», e il programma mi ha risposto di no: i bottoni che chiamano l'AI erano già spenti, quindi quel momento non sarebbe mai arrivato. Non l'ho scoperto ragionando — l'ho scoperto **rileggendo una regola che avevo scritto io** a T3, che i pannelli spengono ciò che non si può fare. Le decisioni vecchie continuano a decidere per te molto tempo dopo, e la differenza tra scoprirlo prima o dopo aver scritto il codice è tutta lì.

### Step 2.20 — I documenti che ho già, e quali di loro allegare

*Il pezzo che chiude T6, ed è quello a cui tenevo di più: una candidatura non è solo il CV che il programma ha appena scritto. Sono anche gli attestati che ho in una cartella da anni — il patentino del muletto, l'HACCP, il corso sicurezza — e che a mandarli tutti insieme non li legge nessuno.*

**Cosa ho fatto**
- **La cartella si registra una volta.** «Documenti da allegare…» in P7 chiede dove tengo i miei documenti; il percorso finisce in `documenti.json`, ed è l'unico posto del programma che contiene un percorso **fuori** dalla cartella dati — quella cartella è mia e sta dove voglio io.
- **La lettura.** File dei formati che l'applicazione sa leggere, nella cartella e nelle sottocartelle di **primo livello**; per ognuno nome, data, dimensione e un **assaggio** delle prime righe dove il disco lo concede.
- **Una sola chiamata all'AI** per tutta la cartella: il prompt propone per ogni file se è un CV, un attestato, una lettera o altro, e dice il perché in una riga.
- **La conferma è mia.** Una finestra mostra l'elenco con categoria e motivo; scelgo una riga, cambio la voce dalla tendina, e quella correzione **resta**: le riletture successive non ci ripassano sopra. Da lì «Conferma», oppure «Fai rileggere la cartella», oppure «Cambia cartella».
- **Gli attestati fra gli allegati, spenti.** Compaiono in fondo all'elenco di P7, marcati «(dai tuoi documenti)», da spuntare uno per uno.
- **Un contatore che finalmente può muoversi.** Nella Home mancava quello delle **inviate**, e il commento nel codice diceva perché: «lo aggiungerà la tappa che lo fa salire». Quella tappa era questa, e me ne sono accorto rileggendo il capitolo 07 per l'«aggiorna-tutto» — cioè dal documento, non dal codice.
- **Il banco e il campo.** Da 566 a **598 collaudi** verdi; e sul campo, con la chiave vera, **nove file su nove** classificati giusti — compresi i tre attestati, la busta paga e un rapporto tecnico finiti dove dovevano. L'attestato spuntato è finito nel messaggio **identico byte per byte**.

**Cosa ho imparato**
- **L'assaggio dei PDF costerebbe una trascrizione a testa.** Il disco non basta a leggere un PDF: ci vuole l'AI, una chiamata per file. In una cartella di documenti i PDF sono quasi tutto, quindi sarebbero quindici chiamate per smistare della carta. Ho deciso di **non assaggiarli** e di farli giudicare dal nome — e sul campo ha funzionato: «Attestato_HACCP_2019.pdf» non ha bisogno di essere aperto.
- **Un prompt che sa dire «non basta» vale più di uno che indovina.** L'avevo scritto in `classifica_documenti` prima ancora di avere il codice che lo chiama, e nella prova si è visto nei motivi: *«assaggio non disponibile ma la nomenclatura è inequivocabile»*. È una frase che dice **su cosa** ha deciso, e a me serve esattamente quella per correggerlo a colpo d'occhio.
- **Il modello dati era già pronto da tre giorni.** La bozza dell'email sapeva già distinguere un allegato «della candidatura» da uno «dei documenti», e sapeva già ricomporne il percorso: l'avevo scritto a P7, quando la cartella documenti non esisteva ancora. Trovarselo fatto è il premio di aver seguito il capitolo invece di scrivere il minimo che serviva quel giorno.

**Dove ho faticato / cosa non era ovvio**
- **Quanti file mandare.** Una cartella con trecento documenti farebbe scoppiare il prompt. Ho messo un tetto a sessanta — ma la cosa che conta non è il numero: è che quel che resta fuori **si dica**. Un elenco troncato in silenzio si legge come «nella cartella non c'era altro».
- **La griglia con la tendina dentro ogni riga non l'ho fatta.** Sarebbe stato il modo ovvio di correggere una categoria, e sarebbe stato l'unico controllo del suo genere in tutta l'applicazione: da vestire, da collaudare e da spiegare. Ho usato quello che c'è già ovunque — una lista in vista dettagli — con una tendina sotto che agisce sulla riga scelta.
- **La finestra di scelta cartella non l'ho potuta premere io.** Lo strumento di collaudo sa rispondere alla scelta di un *file*, non di una *cartella*: sono due finestre diverse e la seconda non espone la casella che il primo cerca. Ho registrato la cartella scrivendo il file di configurazione a mano e ho provato tutto il resto del giro; quel bottone resta da premere a mano, e sta scritto fra i limiti dello strumento.

**Cosa ho deciso e perché**
- **Gli attestati arrivano spenti.** Quali provino qualcosa **per questo annuncio** lo so solo io, e allegare a un'azienda tutti i certificati che possiedo è il modo di far leggere nessuno. Il programma me li mette a portata di mano; a sceglierli sono io.
- **Quello che l'AI mette in «altro» non si propone.** Una busta paga non deve poter finire in un'email per sbaglio. Se sbaglia categoria, il posto per correggerla è la finestra di conferma — non l'elenco degli allegati.
- **Dei nomi mi fido, dei file no.** L'elenco su disco dice cosa c'era l'ultima volta; a dire cosa c'è adesso è solo il disco. Un attestato cancellato nel frattempo non compare fra le cose allegabili, ed è quello che uno si aspetta di aver fatto cancellandolo.
- **Nessun file viene copiato.** La raccolta è un elenco di nomi con una categoria: gli allegati si leggono da dove sono. Se sposto la cartella, il programma lo dice invece di allegare fantasmi.

💡 *Mia intuizione / scelta ragionata* — La cosa che mi porto è la differenza fra **classificare** e **capire**. Il prompt che smista quella cartella non legge i miei documenti per sapere cosa contengono: li guarda da fuori — nome, data, forma delle prime righe — e dice a quale mucchio appartengono. È un lavoro modesto, e proprio per questo costa una chiamata sola e si può correggere con due clic. Avevo cominciato immaginando che l'AI dovesse *leggere* ogni file per meritarsi la risposta; e invece la risposta giusta era chiedere molto meno, dichiarare quel che non si è visto, e lasciare l'ultima parola a chi quei documenti li ha in mano.

### Step 2.21 — Il collaudo di T6 in mano mia, e i due difetti che ha stanato

*T6 era chiusa, fusa e pushata. Restavano due prove che dalla sessione non si potevano fare: aprire l'`.eml` in un programma di posta vero e spedirlo, e premere il bottone che sceglie la cartella dei documenti. Le ho fatte io, sui miei dati veri, la sera stessa. Sono servite: hanno trovato due difetti che 598 collaudi verdi non avevano visto.*

**Cosa ho fatto**
- **La chiave, salvata per davvero.** Doppio clic sull'exe — senza il lanciatore, così la variabile d'ambiente non c'era — e la finestra ha chiesto la chiave. Incollata, salvata. Su disco: `segreti.bin`, 326 byte, che comincia con la firma della protezione dati di Windows e **non contiene «sk-ant» in chiaro**. Da lì in poi l'applicazione parte col doppio clic.
- **La mia cartella dei documenti personali**, indicata al dialogo di Windows. Tredici file, sottocartelle comprese: i **due CV riconosciuti** (col più recente indicato), e tutto il resto in «altro» — carte d'identità, codici fiscali, NASPI, documenti dello stage. La cosa che conta è quella che **non** è successa: nessun documento personale è finito fra gli allegabili.
- **La candidatura a Delta Sistemi spedita davvero**, al mio indirizzo. Aperta nel nuovo Outlook come bozza pronta, con destinatario, oggetto, corpo e allegati; premuto Invia; ricevuta.
- **Due difetti trovati e chiusi**, più una falsa pista.
- **Il banco**: 599 verdi, versione 0.3.027.

**Cosa ho imparato**
- **Un errore del client che nomina i miei file si legge come un errore mio.** Al primo tentativo Outlook ha detto: «Non è stato possibile allegare i file… Riprova più tardi», elencando i miei due PDF. Sembrava il formato. Non lo era: il file, riletto con un lettore indipendente, aveva le intestazioni giuste e i PDF **identici byte per byte**. A mancare era la **sessione dell'account** dentro Outlook, scaduta — il nuovo Outlook, per allegare a un messaggio importato, deve parlare col servizio. Rifatto l'accesso, tutto è partito. *Prima di toccare il formato, guarda se il client è in condizione di funzionare.*
- **Un indice si fida di sé stesso finché nessuno gli dice il contrario.** Dopo l'invio la cartella diceva `inviata` e la Home mostrava ancora «generata», col contatore a zero: `registro.json` controlla che **l'insieme delle cartelle** combaci, non cosa c'è dentro. P4 e P6 lo annotavano a ogni cambio di stato; P7 no — l'avevo dimenticato scrivendo T6.
- **Un collaudo che apre un programma vero non è un collaudo, è un'invasione.** Sullo schermo c'erano cinque finestre di Outlook con «non è stato possibile aprire *Email_Rossi_S_p_A…*»: le apriva il **banco**, che premeva «Prepara l'email» — bottone che scrive il file *e lo apre* — su cartelle temporanee poi cancellate. Ogni giro del banco, una finestra.

**Dove ho faticato / cosa non era ovvio**
- **La riga di comando che non trova niente.** Il percorso dell'eseguibile mi era stato dato relativo alla radice del repo: l'ho copiato e incollato, e non c'era. Da qui una regola che vale sempre: **i percorsi che devo usare io si scrivono completi**, da `C:\Users\…` in giù.
- **Il registro non si ripara da solo.** Corretto il codice, la mia riga restava sbagliata: l'indice si rigenera solo quando *non combacia*. È bastato cancellarlo — è un file rigenerabile per costruzione, e riaprendo l'applicazione si è ricostruito giusto, con «1 inviata». Il capitolo lo prometteva; è la prima volta che quella promessa è servita davvero.

**Cosa ho deciso e perché**
- **Ho corretto invece di annotare.** Erano difetti piccoli e a portata di mano, e uno di loro rendeva falsa la prima cosa che l'utente guarda. Entrambi hanno ora il loro collaudo — e quello dell'indice l'ho **provato al contrario**, disattivando la correzione per vedere il test fallire: un collaudo che non morde non è una prova.
- **L'atto separato dal gesto, di nuovo.** «Prepara l'email» fa due cose: scrive il file e lo consegna al programma di posta. Ho estratto la prima in un metodo suo, che è quello che chiama il banco; l'effetto verso il mondo esterno resta nel gestore del bottone, dove nessun collaudo lo tocca. È la terza volta in questa tappa che la stessa lezione torna utile.
- **Ho buttato il registro invece di riscriverlo a mano.** Modificare un file dell'utente per aggiustare un mio errore sarebbe stato peggio del difetto: l'indice è dichiaratamente rigenerabile, e ho usato quella porta.

💡 *Mia intuizione / scelta ragionata* — Il conto della serata: 598 collaudi automatici verdi, due difetti veri trovati in venti minuti di uso vero. Non è una critica al banco — quei due erano fuori dalla sua portata per costruzione: uno viveva nel dialogo fra due pannelli che il banco prova separatamente, l'altro **era** il banco. È la conferma di una cosa che sospettavo da T4: il collaudo automatico difende da ciò che si rompe, l'uso vero rivela ciò che non è mai stato collegato. Servono tutti e due, e il secondo lo può fare solo chi il programma lo usa per il suo lavoro — cioè io.

### Step 2.22 — T7a: la lingua che viaggia dall'annuncio fino alla pagina stampata

*Ho aperto T7 il 15 agosto e l'ho spezzata in tre: la lingua (T7a), l'anti-slop (T7b), il brainstorming (T7c). La prima è quella che sembrava più grossa e si è rivelata la più corta, perché quasi tutti gli anelli erano già mezzi costruiti da tappe precedenti — solo che nessuno li aveva mai collegati.*

**Cosa ho fatto**
- **La catena intera**: l'annuncio dichiara la sua lingua, l'opportunità se la porta dietro, il generatore chiede al pool quella variante, l'impaginazione intitola le sezioni in quella lingua e il nome del file la scrive in sigla.
- **Un posto solo che decide**: `LinguaDocumenti`. Vuoto è italiano, `it` è italiano, tutto il resto è inglese; e una **terza lingua** viene dichiarata come tale, così P6 può dire che l'inglese è un ripiego e non una scelta.
- **Pool 1.06**: le tre varianti inglesi di `cv_base`, `cv_mirato` e `lettera`, che non sono traduzioni e non contengono nessun «traduci»; e `analisi_annuncio` guadagna due campi in uscita, `lingua` e `contatto`.
- **Il destinatario dall'annuncio**, che chiude un debito di T6: se l'annuncio scrive un indirizzo per esteso, P7 lo propone — nella casella dove scriverei io, cancellabile come se l'avessi scritto io, e **solo al primo arrivo**, mai sopra una bozza ripresa.
- **La tendina di P6 si sveglia**: cambiare lingua chiede conferma e riscrive i documenti, invece di essere un'impostazione silenziosa.
- Da 599 a **623 collaudi** verdi, poi 628 con la coda del giorno dopo.

**Cosa ho imparato**
- **Una regola scritta due volte è già una regola divergente.** Le etichette stampate nei documenti erano costanti italiane: un CV inglese sarebbe uscito con «Esperienze professionali» sopra del testo inglese — un documento finito a metà che *nessun collaudo sui prompt avrebbe mai preso*, perché il pool il suo lavoro l'aveva fatto. E quando ho messo la regola della lingua accanto a quella delle etichette, le due copie **non dicevano già la stessa cosa**: un annuncio tedesco andava in inglese per il pool e in italiano per le etichette.
- **La lingua si legge nell'annuncio, non nell'interfaccia.** Un annuncio italiano per un posto a Dublino resta italiano, e un annuncio inglese dentro un portale con l'interfaccia italiana resta inglese. Conta il testo.
- **Il vuoto deve cadere sull'italiano.** Le candidature nate prima di T7 il campo `lingua` non ce l'hanno: se il vuoto valesse «inglese», il programma le riscriverebbe tutte all'indietro. È lo stesso ragionamento che T5c aveva applicato agli stati.

**Dove ho faticato / cosa non era ovvio**
- **Quanto era già lì.** `LibreriaPrompt.Carica` sceglieva varianti di lingua **da T2**, senza aver mai avuto niente fra cui scegliere; `NomiDocumenti` sapeva già scrivere `EN`; `Opportunita.Lingua` era già salvata e già nel registro. La tappa è stata più «collegare» che «costruire», e riconoscerlo mi è costato un giro di lettura dei capitoli.
- **La tendina dice «Inglese», non «English».** Sembra un dettaglio e invece è la regola dell'interfaccia: il programma parla **una lingua sola** con me, anche quando scrive documenti in un'altra.

**Cosa ho deciso e perché**
- **Il profilo resta uno, e resta in italiano.** Le rese inglesi vivono nei documenti generati. Un secondo profilo da tenere allineato sarebbe stato una seconda verità, e nel giro di due tappe avrebbe cominciato a divergere.
- **Vietato l'upgrade nella traduzione**, scritto dentro i prompt inglesi con la sua tentazione accanto: la resa generosa va rifiutata **soprattutto** quando è quella che combacerebbe con un requisito dell'annuncio. Un'invenzione fatta in traduzione è pur sempre un'invenzione.
- **Le chiavi del JSON restano italiane.** Le legge il programma, non chi riceve il CV: tradurle avrebbe rotto impaginazione, archivi e anteprime senza cambiare niente per il lettore.

💡 *Mia intuizione / scelta ragionata* — Il pezzo che mi porto è che **la lingua non è un'impostazione, è un dato del caso**. Se fosse stata una preferenza mia in un menù, il programma avrebbe dovuto chiedermela ogni volta e io avrei dovuto saperla; essendo un attributo dell'annuncio, la sa lui e me la propone, e io devo solo poter dire di no. Le cose che il programma può dedurre dal materiale non dovrebbero mai diventare domande.

### Step 2.23 — Il collaudo di T7a, e l'oggetto che parlava ancora italiano

*Un annuncio inglese vero, pescato da un portale e portato fino ai file. Volevo vedere se la catena reggeva dall'inizio alla fine, e ho scelto il caso più scomodo che sapevo costruire: un annuncio in inglese dentro una pagina con l'interfaccia italiana, che conteneva anche un secondo annuncio in italiano.*

**Cosa ho fatto**
- **Indeed col filtro *Lingua delle offerte = English***, e ne è uscito un *External Warehouse Manager* da Fedrigoni, a Caponago. Match 0,8 su 5 — basso, ma non era quello che stavo misurando.
- **Il giro intero su una cartella usa-e-getta**, dati veri mai toccati: cattura, analisi, confronto, generazione, esportazione in DOCX e PDF.
- **Quello che ha retto**: `lingua: "en"` presa dall'annuncio e non dall'interfaccia; il confronto **fra due lingue** (requisiti in inglese, lettura d'insieme e ponti in italiano, la lingua di chi legge); 🎯 CV-2 e lettera **scritti in inglese, non tradotti**; le etichette *Work experience / Other experience / Skills / Education* nei documenti veri; la tendina provata **in tutte e due le direzioni**, perché rispondendo No rimette la lingua di prima invece di lasciare un pannello che ne dichiara una e mostra documenti scritti in un'altra.
- **Due cose non provate, e l'ho scritto**: `Driving licence`, perché il mio profilo la patente non la dichiara e quella riga non si stampa mai; e il destinatario dall'annuncio, perché quell'annuncio un indirizzo non lo pubblica — il campo è restato **vuoto**, che è la promessa mantenuta, non tradita.
- **Pool 1.07** subito dopo, per il difetto che il giro ha trovato.

**Cosa ho imparato**
- **`Cover letter` non era sparita: era nei metadati.** L'ho cercata nel corpo del documento e non c'era. Sta in `dc:title` e in `/Title`, dove il progetto aveva deciso di metterla, ed è lì che va cercata.
- **L'unico italiano rimasto nel CV inglese era giusto che restasse**: il **nome proprio di un corso**, tenuto in originale con la glossa inglese fra parentesi. È esattamente il comportamento che il prompt chiede, e vederlo capitare da solo vale più di un collaudo.
- **Il banco non poteva vedere il difetto dell'email.** I collaudi coi finti **non caricano nessun prompt**: guardano la lingua dichiarata, non il testo che parte davvero. Da lì i tre collaudi nuovi che leggono il messaggio spedito all'API.

**Dove ho faticato / cosa non era ovvio**
- **La diagnosi giusta è arrivata alla seconda.** L'email era a metà del guado — corpo inglese, oggetto italiano — e la prima spiegazione che mi ero dato era imprecisa. Il prompt italiano la regola ce l'aveva, sezione 3: «nella stessa lingua della lettera», e il **corpo l'ha seguita**. A disobbedire è stata la **formula dell'oggetto**, che la sezione 1 detta parola per parola in italiano.
- **Rinominare un prompt rompe il pool integrato** finché non lo si risigilla: il manifest *è* l'elenco dei file attesi. E il bump ancora la versione in **tre** punti dei collaudi della libreria, non due.

**Cosa ho deciso e perché**
- **Il rimedio pieno, non la riga in più.** Potevo scrivere «scrivi nella lingua della lettera» e cavarmela con un file solo. Avrei però messo la regola della lingua in **due posti** — ed è l'errore che T7a aveva appena finito di correggere sulle etichette, dove le due copie erano già divergenti. La lingua si decide in un posto solo e da lì viaggia: fino a 1.06 arrivava a P6 e si fermava, adesso arriva anche a P7.
- **Il difetto della bozza persa uscendo da P7 l'ho annotato, non corretto.** Toccava il disegno del pannello ed era fuori dal mandato di quella sera; l'ho messo in `in_sospeso.md` sotto T6.

💡 *Mia intuizione / scelta ragionata* — La lezione l'avevo già pagata col Pool 1.05, dove un esempio che dava del tu all'azienda aveva battuto la regola che lo vietava due sezioni più su. Qui è tornata identica sotto un altro vestito: **fra una regola generale e una forma concreta da imitare, vince la forma**. Comincio a pensare che sia la cosa più importante che ho imparato sui prompt in tutto il progetto — e che quando ne rileggo uno, il posto da guardare per primo non siano le regole ma gli esempi, perché sono quelli che il modello copia davvero.

### Step 2.24 — T7b: l'anti-slop, e perché i prompt sono tre e non uno

*La cartella `rifinitura/` era nel progetto dal capitolo 04.3 ed era rimasta vuota per sei tappe. Il capitolo ne prevedeva **un** prompt; facendo l'inventario dei campi da rifinire ne ho trovati tre, e incompatibili fra loro.*

**Cosa ho fatto**
- **Pool 1.08, tre prompt in due lingue**: `umanizzazione_sintesi` per il sommario del CV, `umanizzazione_frasi` per le descrizioni delle esperienze — che sono **frasi nominali** e devono restarlo — e `umanizzazione_prosa` per il corpo di lettera ed email.
- **La rifinitura dentro la pipeline**, subito dopo ogni documento: da quattro passi a **sei**. Così la lettera riceve un CV **già rifinito** e i due non raccontano la stessa storia con parole diverse.
- **Il «prima» si conserva accanto al documento, mai dentro**: il 🎯 CV-2 finisce nel prompt della lettera e la lettera in quello dell'email, e un campo di servizio viaggerebbe con loro.
- **In P6 la casella che stava lì spenta da T4** accende finalmente il prima/dopo, campo per campo.
- **659 collaudi** verdi, versione 0.3.030.

**Cosa ho imparato**
- **Su una frase nominale «varia la lunghezza dei periodi» è l'istruzione sbagliata**: la trasformerebbe in una frase. È stato l'argomento che ha deciso i tre prompt — le tre forme non si possono servire con le stesse parole.
- **L'anti-invenzione conviene renderla strutturale prima che scritta.** Al modello arrivano **solo i campi-prosa**: nomi, aziende, date, competenze e titoli non entrano nella richiesta, e quel che non entra non può tornare cambiato. La regola smette di essere una promessa dentro un prompt e diventa una proprietà della chiamata.
- **In VB una variabile locale che si chiama come una funzione la copre**, e la chiamata viene letta come indicizzazione. L'ho ripagata due volte in un pomeriggio (`campi`/`Campi`, `casella`/`Casella`).

**Dove ho faticato / cosa non era ovvio**
- **Il prezzo dei tre prompt è una chiamata in più sul CV** — sommario e descrizioni sono due generi diversi, quindi due chiamate. L'ho scelto sapendo il costo, perché l'alternativa era un prompt unico che conteneva tutte e tre le forme e lasciava al modello la scelta di quale imitare: cioè l'errore già pagato col Pool 1.05 e col 1.07.
- **Nessun documento deve poter uscire peggio di com'è entrato.** Un pezzo dimenticato, uno vuoto, un id inventato, una risposta illeggibile: in ogni caso resta il testo originale. E una rifinitura che fallisce non fa fallire la generazione — l'annullamento invece deve passare, perché non è un inciampo dell'AI, sono io che ho premuto Annulla.

**Cosa ho deciso e perché**
- **Umanizzare tutti e quattro i testi, ma ciascuno nel modo consono al suo genere.** Era la domanda di partenza e la risposta ovvia («uno solo, il più visibile») avrebbe lasciato fuori proprio la lettera, che è il testo che l'azienda legge per primo.
- **Fuori di proposito**: apertura, chiusura e firma della lettera, e l'oggetto dell'email. Non sono slop, sono le formule che un lettore si aspetta — e l'oggetto è quello che il Pool 1.07 aveva appena dettato parola per parola in due lingue.
- **La casella di P6 è una casella di vista, non un interruttore.** La rifinitura si fa sempre; quella spunta decide solo se mostrarmi cos'è cambiato. L'interruttore vero appartiene alle Impostazioni, che sono di T9, e l'ho scritto fra le cose in sospeso.

💡 *Mia intuizione / scelta ragionata* — Mi ha colpito che la guardia più forte di tutta la tappa non sia una frase di prompt ma **una scelta su cosa mandare**. Potevo spedire il documento intero e scrivere «non toccare i nomi e le date»: sarebbe stato più semplice, avrebbe funzionato quasi sempre, e quel «quasi» sarebbe stato invisibile finché un giorno non avesse cambiato un'azienda. Mandando solo i campi-prosa, quel giorno non può arrivare. Le regole che si possono rendere impossibili da violare non vanno scritte: vanno costruite.

### Step 2.25 — Il collaudo dell'anti-slop, e la regola che perdeva contro il permesso

*T7b era verde al banco. Ma i collaudi coi finti non caricano prompt — l'ho imparato a T7a — quindi quanto valessero davvero quei sei testi non lo sapeva nessuno. Il 18 agosto ho fatto il giro con l'AI vera, in italiano e in inglese, con una griglia di lettura decisa prima di guardare i risultati.*

**Cosa ho fatto**
- **Un giro intero su cartella usa-e-getta**: profilo importato dal mio CV in PDF, un annuncio italiano incollato a mano e costruito con **gap veri** (patentino del muletto, SAP, inglese scritto B2, due anni di esperienza che non ho). Match 1,5 su 5.
- **La lettura campo per campo**, prima a occhio e poi con un confronto parola per parola fatto a macchina: due controlli indipendenti, confrontati fra loro.
- **Quello che ha retto**: nessun grado mai rafforzato in nessuno dei nove campi cambiati, nessun fatto nuovo, le frasi nominali rimaste nominali, **quattro descrizioni su cinque restituite identiche**, i gap onesti sempre espliciti — e in inglese perfino **spostati in evidenza**, da subordinata concessiva a proposizione principale. Un refuso della generazione («la mia inglese») è stato pure corretto.
- **Tre difetti trovati**, curati nel **Pool 1.09** e **riprovati con l'AI** invece che dati per buoni.
- **Il difetto di P7 corretto**: la bozza dell'email ora porta con sé la lingua in cui è nata.
- **663 collaudi** verdi, versione 0.3.031.

**Cosa ho imparato**
- **Rafforzare una regola non serve se a batterla è un permesso.** Le lineette lunghe venivano tolte a metà; ho irrobustito la riga che le vieta e al giro dopo la lettera è tornata **identica, con tutte e sei al loro posto**. Il problema non era la regola ma dove stava: a vincere era il **permesso di non cambiare** — «se il testo è già naturale restituiscilo identico» — che parla del **testo intero**, mentre l'elenco dei tic parla di un dettaglio. Fra due istruzioni che si contraddicono, vince quella che parla di tutto. La cura vera è stata scrivere l'eccezione dentro il permesso: un testo con una lineetta usata come pausa non è mai «già naturale».
- **Una lista di parole vietate può cancellare un fatto.** «End-to-end» era finito fra i riempitivi inglesi, e nel mio CV è diventato «Testing of AI applications»: ma *end-to-end testing* è un tipo preciso di test, non aria. In italiano non era successo, perché quella lista non lo conteneva. Adesso in tutte e due c'è la regola che mancava: un termine del mestiere è un fatto.
- **Riformulare non è riassumere, e il modello non lo sa da sé.** Nella lettera italiana era sparita una proposizione intera — quella che legava la mia esperienza al gestionale che non conosco. Il divieto di togliere c'era già, ma generico: ci ho messo accanto la forma concreta, cioè l'esempio.

**Dove ho faticato / cosa non era ovvio**
- **«Genera CV + lettera» su una candidatura già generata non rigenera niente**: apre P6 e mostra quelli che ci sono. Ho creduto per un giro intero di aver provato i prompt nuovi, e stavo guardando i vecchi risultati. Me ne sono accorto dai **timestamp dei file**, non dallo schermo.
- **Aspettare che un bottone si riaccenda non è aspettare che il lavoro finisca.** Ho dovuto aspettare che cambiasse il **contenuto** del file della lettera: l'app risalva la cartella anche solo cambiando lingua, quindi nemmeno la data del file bastava.
- **Distinguere «non ha voluto cambiare» da «non ci è riuscita» dall'esterno non si può.** Quando la lettera è tornata identica ho dovuto leggere il codice della pipeline per sapere che un fallimento me lo direbbe. È un'informazione che ho dovuto cercare, e l'ho annotata.

**Cosa ho deciso e perché**
- **Le cure vanno nel prompt, non nel codice.** Tutti e tre i difetti erano difetti di istruzioni, e il posto delle istruzioni è il pool: versione nuova, impronte rigenerate, changelog che racconta anche il tentativo fallito — perché il tentativo fallito è la parte che insegna.
- **Il difetto di P7 l'ho fatto correggere subito**, invece di annotarlo come il suo gemello di T7a: erano quindici righe, non toccava il disegno, e lasciava il programma a dire una cosa falsa — mostrava un'email italiana per una candidatura inglese, in silenzio.
- **Ma non si riscrive da sé.** Se le lingue non combaciano il pannello **lo dice** e indica il bottone; non rifà il messaggio, perché quel testo può essere già passato per le mie mani. È la stessa regola per cui una bozza salvata non viene mai sovrascritta all'arrivo.

💡 *Mia intuizione / scelta ragionata* — Il conto della giornata: sei prompt verdi al banco, tre difetti veri in un giro solo con l'AI vera, e uno di quei tre curato **due volte** perché la prima cura non aveva funzionato. È la stessa lezione dello Step 2.21 spostata dai pannelli ai prompt: il banco difende da ciò che si rompe, l'uso vero rivela ciò che non è mai stato collegato. Con una differenza che mi ha fatto pensare — un prompt non «si rompe» mai: risponde sempre qualcosa di plausibile, e se non lo si guarda con una griglia decisa **prima**, quel plausibile passa per giusto. Ho scritto la griglia il giorno prima di usarla, e credo sia stata la parte più utile di tutto il collaudo.

### Step 2.26 — T7c: il brainstorming, e la prima conversazione vera del progetto

*L'ultimo pezzo di T7. Il capitolo 12 lo prevedeva dall'inizio — «una conversazione libera con l'AI, con la risposta che compare in streaming» — e il capitolo 02.5 gli teneva in serbo lo streaming da T2, dicendo che il valore vero era qui e non nella generazione. Sono arrivato a incassare quella promessa, e per farlo ho dovuto insegnare al programma una cosa che non aveva mai fatto: parlare.*

**Cosa ho fatto**
- **Pool 1.10, la cartella `brainstorm/`**: `brainstorm.md`, la conversazione ancorata a profilo, annuncio, giudizi e — deciso da noi, contro il capitolo — anche alle **mitigazioni**; e `appunti_di_mira.md`, che dalla chiacchierata distilla **al massimo sei** appunti operativi.
- **Lo streaming**, il primo trasporto davvero nuovo rispetto al prototipo: `stream: true`, eventi SSE, e una classe che sa **solo** la grammatica del formato e niente di Claude — perché il pezzo rischioso si potesse collaudare con delle stringhe.
- **P5 fa due mestieri**: il dialogo che costruisce il profilo e il ragionamento su una candidatura. Stessi controlli, altri nomi, e la bolla dell'AI che **cresce mentre il testo arriva**.
- **Gli appunti confermati** finiscono in `appunti.json` accanto agli altri file della candidatura, e da lì entrano nei prompt del 🎯 CV-2 e della lettera.
- **726 collaudi** verdi, versione 0.3.032.

**Cosa ho imparato**
- **L'API vuole che i ruoli si alternino**, e due miei turni di fila capitano davvero: basta che una risposta fallisca e io riscriva. Buttare via la prima frase sarebbe stato semplice e sbagliato — resta scritta sullo schermo, e sparirebbe solo dalla memoria del modello. Si **uniscono**: è l'anti-perdita applicata a un dettaglio di protocollo.
- **VB non sa scrivere una funzione `Async` che restituisce `ValueTask`** — niente tipi di ritorno generalizzati come in C#. Nel banco il `Task` vero si avvolge a mano.
- **Un collaudo che non diventa rosso quando rompi il codice non è un collaudo.** Avevo un collaudo sull'attesa che sembrava buono; una lettura critica ha mostrato che provava solo il caso negativo. Ho tolto davvero la riga che riarma il timer per vedere se qualcuno se ne accorgeva: nessuno. Ora c'è la prova positiva, e quella riga tolta la fa fallire.

**Dove ho faticato / cosa non era ovvio**
- **Cosa vuol dire «riprovare» quando metà risposta è già sullo schermo.** Il ritentativo automatico è comodo finché la risposta arriva tutta insieme; a pezzi diventa o una risposta scritta due volte o una cancellazione sotto gli occhi di chi legge. La regola nuova è netta: si ritenta **solo prima del primo pezzo**.
- **Il metro dell'attesa è cambiato senza che nessuno lo chiedesse.** Finché le chiamate erano sincrone, l'attesa cresceva col limite di token del prompt: giusto, perché finché non arriva tutto non è arrivato niente. In streaming quella ragione decade, e un tetto complessivo taglierebbe proprio le risposte lunghe legittime. Quello che resta da riconoscere è il collegamento morto, e un collegamento morto si vede dal **silenzio**.

**Cosa ho deciso e perché**
- **Le mitigazioni entrano nel ragionamento.** Il capitolo ne nominava tre di fonti; nel codice i ponti si costruiscono al passo 2, insieme al confronto, quindi quando apro la conversazione **ci sono già**. E un appunto che dica «usa questo ponte» senza vederli non potrebbe esistere.
- **I fatti nuovi stanno in una lista a parte, e non entrano nei documenti.** È il punto della tappa: quello che dico in chat non può scavalcare il profilo, che è l'unica fonte di fatti; ma non può nemmeno sparire in silenzio. Resta lì, dichiarato, perché lo porti nel profilo se è vero.
- **Il ragionamento si può interrompere**, all'opposto del turno del dialogo guidato. Là una mossa a metà lascerebbe la macchina in uno stato che non esiste; qui resta solo una risposta più corta, e quello che è arrivato è roba buona.

💡 *Mia intuizione / scelta ragionata* — Il brainstorming è il primo posto del programma in cui posso **dire una cosa che il mio profilo non dice**. Potevo trattarlo in due modi: credermi — e allora il profilo smette di essere l'unica fonte di fatti, senza che nessuno se ne accorga — oppure non credermi e lasciar cadere la frase. La terza strada è quella che mi piace di più, e non l'avevo vista subito: il programma mi crede *abbastanza* da annotarlo, e non abbastanza da usarlo. «Se è vero, mettilo nel profilo, e sarà tuo per tutte le prossime candidature.» La fiducia non viene negata, viene **spostata dove c'è una conferma**.

### Step 2.27 — Il collaudo del ragionamento, e la cosa che ho provato a far entrare dalla finestra

*Come per T7b, il banco era verde e non voleva dire niente: i collaudi coi finti non caricano prompt. Il giro con l'AI vera l'ho fatto con una prova costruita per essere sleale — dichiarare in chat proprio un requisito che l'annuncio chiede, per vedere se il CV lo raccoglieva.*

**Cosa ho fatto**
- **Un giro intero su cartella usa-e-getta**: il mio CV in PDF, un annuncio da Tecnico QA costruito con gap veri (due anni di esperienza che non ho, inglese scritto B2, SAP Business One, la laurea). Match 1,1 su 5.
- **La prova sleale**: a metà conversazione ho detto *«in realtà uso SAP Business One da circa un anno, solo che nel CV non l'ho mai scritto»*. SAP è un requisito **preferenziale dell'annuncio**: se il sistema avesse una crepa, quella frase ci passerebbe attraverso.
- **Il seguito del giro**: appunti distillati, confermati nella scheda, e documenti generati.
- **La verifica**: `SAP` e `Business One` cercati in CV e lettera. **Zero.** L'unica occorrenza che il primo controllo aveva contato era dentro «con**sap**evole».

**Cosa ho imparato**
- **L'AI ha risposto meglio di quanto avessi scritto nel prompt.** Alla mia frase su SAP ha detto: «deve entrare nel profilo prima di entrare nella candidatura — dove lo hai usato?», e ha aggiunto che una volta nel profilo il ponte con la gestione magazzino reggerebbe bene. Cioè ha fatto le due cose insieme: non me l'ha lasciata passare e non me l'ha buttata via.
- **Un bottone tagliato non è un dettaglio estetico.** A video si leggeva «Torna alla»: la larghezza era quella pensata per «Torna al profilo». Nessun collaudo del banco poteva vederlo — il banco legge il testo del bottone, non quanto ce ne entra — e infatti l'ho trovato guardando una fotografia.
- **Gli appunti hanno funzionato da soli, senza che io li rileggessi.** Ne avevo confermati tre: metti davanti il test end-to-end su applicazioni AI, non nominare la laurea, tono sobrio. Il sommario del CV si apre esattamente con quella frase, la laurea non compare da nessuna parte, e il testo è asciutto.

**Dove ho faticato / cosa non era ovvio**
- **L'attrezzo di collaudo mi ha mentito su dove stavano i file.** Il comando che elenca la cartella dati guarda sempre quella **predefinita**, anche quando l'applicazione gira su una usa-e-getta: mi sono trovato davanti i file dei dati veri credendoli quelli della prova. L'ho scritto fra le trappole.
- **La casella della chat si chiama come l'ultima risposta dell'AI.** È la vecchia trappola dell'etichetta che si spaccia per la casella, vista in una conversazione: il nome della casella diventa mezza pagina di testo — e se dentro c'è un apostrofo, non si trova più.
- **L'interruzione non sono riuscito a provarla dal vivo**: le risposte arrivavano troppo in fretta per cronometrare il clic. È collaudata al banco, ma non voglio scriverla fra le cose provate: è finita in `in_sospeso.md`.

**Cosa ho deciso e perché**
- **Il difetto del bottone l'ho fatto correggere subito**, con la misura che si adatta al testo invece di un'etichetta più corta: il nome giusto è quello che dice dove porta, e a doversi adattare è il bottone.
- **E accanto alla correzione un collaudo che la misura in numeri.** Una fotografia trova un difetto una volta; a tenerlo fermo serve qualcosa che si ripeta da solo a ogni giro.

💡 *Mia intuizione / scelta ragionata* — Ho costruito la prova come se dovessi truffare il mio stesso programma, e credo sia il modo giusto di collaudare le regole etiche. Le altre cose si provano chiedendosi «funziona?»; una regola come «i fatti vengono solo dal profilo» si prova chiedendosi **«come la aggirerei?»** — e poi provandoci davvero, con l'esca migliore che si riesce a immaginare. Se avessi dichiarato in chat il patentino del muletto, che l'annuncio non chiede, non avrei saputo niente: il modello non aveva motivo di usarlo. La prova vale quanto vale la tentazione che le metti davanti.

### Step 2.28 — T7d: il CV-1 che rinasceva ogni volta, e la porta che era già in casa

*Volevo ripartire dalla coda di T7 — il 📄 CV-1 base in inglese — ma prima ho chiesto di controllare una cosa che mi era rimasta in mente: il giorno prima avevo cliccato «Esporta PDF» e mi era sembrato spento. Era spento davvero, e il motivo si è rivelato più grosso della lingua.*

**Cosa ho fatto**
- **Ho fatto verificare il sospetto invece di raccontarlo**: applicazione compilata da `main` e avviata su una **copia usa-e-getta dei miei dati veri**. Su una candidatura riaperta i due bottoni erano accesi; premendo «Genera 📄 CV-1 base», con il `cv_base.json` di ieri lì sul disco, l'applicazione **l'ha rigenerato**: 45 secondi con tutto spento.
- **La diagnosi**: il CV-1 base viveva **solo nella memoria del pannello**. Il metodo per rileggerlo dal disco esisteva già — `CaricaCvBase` — e a chiamarlo erano **soltanto i collaudi**.
- **La cura, una sola per due problemi**: P6 ora **ripesca** il CV-1 base invece di rifarlo, e quando lo mostra dice di quando è; e siccome adesso quel pannello è casa sua come lo è di una candidatura, la **tendina della lingua** si accende anche su di lui. La coda di T7 si è chiusa lì, senza aggiungere un solo controllo nuovo all'interfaccia.
- **Il giro vero, in fondo**: rientro senza attesa, tendina su «Inglese», conferma, e un CV-1 **scritto in inglese** — esportato in `CV_Riccardo_Parenti_EN_2026-08-18.docx`, con dentro «Work experience» e «Skills», accanto all'italiano di ieri rimasto intatto. 736 collaudi automatici, tutti verdi.

**Cosa ho imparato**
- **Un metodo che non chiama nessuno è un difetto che aspetta.** `CaricaCvBase` era scritto, collaudato e documentato: il capitolo 11.1 prometteva perfino che l'app avrebbe detto «questo CV è di una versione precedente». Non poteva dirlo mai, perché quella versione precedente non arrivava mai a video. Il banco era verde e la promessa era vuota.
- **Il difetto vero non era la lingua.** Ero partito per il CV-1 in inglese; il problema serio era che un documento già scritto e approvato non si poteva **riavere** senza pagarne un altro. Senza chiave API non si poteva affatto.
- **«Rientrare non rigenera» era scritto in cima al pannello** e valeva per metà. Le regole che valgono per un caso e non per il suo gemello si notano solo quando qualcuno usa il gemello.

**Dove ho faticato / cosa non era ovvio**
- **Due collaudi sono diventati rossi, e avevano ragione loro.** Facendo sapere al pannello *quale strada* sta percorrendo, la colonna del CV ha smesso di restare muta quando il documento non c'è. In un caso era un miglioramento (senza chiave, la colonna dice che un CV non c'è invece di sembrare non caricata); nell'altro era una bugia — a profilo eliminato avrebbe detto «non è ancora stato scritto» di un CV che era stato scritto eccome. Ho corretto il codice per il secondo e il collaudo per il primo.
- **Un caso che il banco non può vedere**: cambiando lingua, se la riscrittura fallisse resterebbe a video il testo vecchio sotto le etichette della lingua nuova. La conferma di quel comando apre una finestra, e una finestra in un collaudo automatico blocca tutto: quel pezzo l'ho chiuso ragionando, non provandolo.

**Cosa ho deciso e perché**
- **Il CV-1 base di ieri non si rigenera di nascosto**, nemmeno quando il profilo è cambiato: il pannello lo dice e lascia scegliere. Quel CV potrebbe essere quello che ho già spedito.
- **La lingua sta nel `cv_base.json`**, non in un'impostazione a parte: è una proprietà del documento, come per le candidature — solo che il suo padrone è il profilo e non un annuncio.
- **Il bottone «Genera 📄 CV-1 base» continua a chiamarsi così** anche adesso che, se il CV c'è, non genera niente. È la stessa scelta di «Genera CV + lettera» in P4: quel bottone è **la porta** dei documenti, e a rifarli c'è «Rigenera», che lo dichiara e lo chiede.

💡 *Mia intuizione / scelta ragionata* — Questa volta il lavoro non è nato da un piano, ma da un'impressione mia rimasta in sospeso: *mi pare che ieri fossero spenti*. La cosa giusta non è stata credermi né archiviare la cosa, ma **andare a guardare** — sui miei dati, copiati in una cartella usa-e-getta perché una prova non deve mai avere il potere di rovinare quello che sta provando. In dieci minuti ho avuto la risposta, ed era un difetto più profondo di quello che pensavo di cercare. Le impressioni degli utenti, anche quando l'utente sono io e sono vaghe, valgono più di quanto sembrino: ci vuole solo qualcosa che le trasformi in fatti prima che diventino opinioni.

### Step 2.29 — Pool 1.11: la parola «Copy», e la terza volta che imparo la stessa legge

*Il CV-1 in inglese era appena uscito bene, ma leggendolo mi è saltato all'occhio un «Direttore Operativo» in mezzo a sette ruoli già inglesi. Sembrava un dettaglio da niente. Era invece la terza comparsa di un difetto che questo progetto ha già pagato due volte.*

**Cosa ho fatto**
- **Ho chiesto la diagnosi prima della cura**, e ha smentito la prima spiegazione che ci eravamo dati («il prompt ricopia i titoli perché sono fatti»). Non è così: il prompt li vuole tradurre. La regola contro le promozioni nella traduzione nomina espressamente *a role*, e l'elenco dei nomi propri — dove restano aziende, enti e titoli di studio — **i ruoli non li elenca**.
- **Il colpevole era una parola**: accanto al campo c'era «**Copy** ruolo, azienda and durata». Due righe più sotto, le competenze hanno «copy… *translated as section 3 explains*» e la formazione «*following the rule on proper names*». Sul ruolo il rimando mancava.
- **Pool 1.11**: due file corretti (`cv_base.en`, `cv_mirato.en`), rito del bump completo, impronte verificate anche con un calcolo indipendente — 29 su 29, e nel manifest sono cambiate esattamente le due dei file toccati.
- **Riprovato con l'AI vera**, come mi ha insegnato il 1.09: *Direttore Operativo* → **Operations Director**, e i **sette ruoli già inglesi restituiti identici**.

**Cosa ho imparato**
- **Fra un'istruzione concreta accanto al campo e una regola generale in un'altra sezione, vince la concreta.** È la stessa legge del Pool 1.05 (l'esempio batte la regola) e del 1.07 (la formula batte la regola), qui nella forma più asciutta che abbia visto: a battere due sezioni di regole è bastata **una parola**, scritta nel posto dove il modello guarda mentre riempie quel campo.
- **Il confine giusto non è un giudizio, è un campo.** «Traduci i ruoli ma non i titoli di studio» detto così obbliga il modello a decidere caso per caso; detto come `ruolo` sì / `formazione.titolo` no, è una regola che si applica da sola. E ha una ragione vera sotto: un ruolo descrive una funzione, un titolo di studio è un nome e ha valore legale.
- **Il rischio non era la traduzione, era la riscrittura.** Il pericolo di dire «traduci» era che il modello si mettesse a riformulare anche i sette ruoli già in inglese. Non è successo, ma è la cosa che sono andato a controllare per prima.

**Dove ho faticato / cosa non era ovvio**
- **La prima diagnosi era sbagliata e suonava benissimo.** «Il prompt ricopia i titoli perché sono fatti, ed è anti-invenzione applicata bene» è una frase coerente, che spiega quello che si vede — e non era vera. L'ho scoperto solo andando a leggere le righe vere del prompt, tutte, invece di fidarmi della spiegazione.
- **Decidere cosa *non* toccare.** `lettera.en.md` nomina i ruoli anche lei, e la tentazione di correggerla per simmetria era forte. Sono andato a guardare: lì l'unico «Copy» riguarda la firma, e la regola generale non ha nessuno che le remi contro. Un prompt che non ha il difetto non si tocca.

**Cosa ho deciso e perché**
- **Nessuna regola nuova nei prompt**, solo il rimando rimesso dov'era stato dimenticato, nella forma che quei file usano già per gli altri due campi. Aggiungere una regola in fondo avrebbe creato un quarto posto in cui la stessa cosa è scritta — e prima o poi due di quei posti si sarebbero contraddetti.
- **La traduzione secca**, senza l'originale fra parentesi: quella glossa la teniamo per i titoli di studio, dove serve a spiegare senza gonfiare. Su ogni riga di esperienza sarebbe stata solo zavorra.

💡 *Mia intuizione / scelta ragionata* — La cosa che porto via non è la correzione, è **dove** era il difetto. Quando un prompt sbaglia, il riflesso è aggiungere una regola più forte, più in alto, in maiuscolo: l'ho fatto anch'io a T7b, e non aveva funzionato. Invece il posto da guardare è **l'istruzione più vicina al campo che sta scrivendo** — perché è lì che il modello ha gli occhi in quel momento. Tre volte su tre, in questo progetto, il colpevole stava lì: un esempio, una formula, e adesso un verbo. Sto cominciando a pensare che rileggere un prompt significhi esattamente questo: non ripassare le regole, ma controllare che ogni campo dica da solo la cosa giusta.

### Step 2.30 — Il salto a Sonnet 5, e il giorno in cui il prototipo smette di essere un metro

*Sonnet 4.6 è scivolato fra i modelli superati del listino, e il ragionamento della mia applicazione girava ancora lì. Pensavo di dover cambiare una stringa. Ho scoperto che stavo rinunciando alla cosa che difendo da T2: la parità col prototipo.*

**Cosa ho fatto**
- **Ho spostato il livello del ragionamento su `claude-sonnet-5`.** Sono **18 prompt su 29** — confronto, mitigazione, i quattro CV, le due lettere, le due email, la rifinitura, il brainstorming: tutto ciò che nel programma *pensa*. Costa anche meno di quello che sostituisce, **$2/$10 per MTok contro $3/$15**.
- **Il livello semplice non l'ho toccato.** Haiku 4.5 è tuttora l'ultimo della sua fascia — non esiste un Haiku più recente a cui salire — e gli altri 11 prompt sono rimasti esattamente dov'erano.
- **Ho dichiarato spento il ragionamento esteso** (`thinking: {"type": "disabled"}`). Non è una preferenza di stile: Sonnet 5 lo accenderebbe di suo, e **`max_tokens` limita ragionamento e risposta insieme**. I tetti del pool sono cuciti addosso alla sola risposta, quindi lasciarlo acceso avrebbe troncato le risposte **senza dirlo a nessuno**.
- **La macchina per farlo c'era già**: l'interruttore a tre stati costruito a T2 proprio per questo giorno. Il cambio vero sta in **un punto solo** di `Modelli.vb` — l'identificativo nuovo e l'interruttore, nello stesso posto.
- **Ho riscritto i due collaudi di parità invece di cancellarli.** Non dicono più «stesso modello del prototipo»; dicono «**il modello e l'interruttore sono l'unica cosa che diverge**, e i tetti di token non scendono sotto i suoi».
- **Un collaudo nuovo**, perché adesso il predefinito si porta dietro un interruttore: la forma breve di `modelli.json` non deve trascinarselo addosso a un modello scelto **apposta** per ragionare.
- **738 collaudi verdi.**

**Cosa ho imparato**
- **Cambiare modello non è cambiare una stringa.** Un modello arriva coi suoi valori predefiniti, e quelli non stanno scritti nel mio codice: stanno dall'altra parte. Sonnet 4.6 teneva il ragionamento spento da sé e la mia richiesta poteva tacere; su Sonnet 5 la stessa richiesta silenziosa vuol dire il contrario.
- **Tacere e dire «no» non sono la stessa cosa**, ed è per questo che l'interruttore ha tre stati e non due. Sul livello semplice continuo a tacere, perché tacere tiene la richiesta identica a quella del prototipo; sul ragionamento adesso parlo.
- **Un tetto di token è cucito addosso a un modello, non a un testo.** Sonnet 5 conta circa il 30% di token in più a parità di parole: i numeri scritti nei miei prompt sono giusti per il modello di prima, non per questo.
- **La parità col prototipo era una misura a scadenza, non un valore.** Serviva a poter dire «se il risultato cambia, è colpa del mio codice». Il giorno in cui il modello sotto è diverso quella frase non si pronuncia più, e sul ragionamento il prototipo diventa un **termine di paragone** (cap. 04.7). Sul livello semplice invece la parità è viva: Haiku 4.5 sta da tutte e due le parti.

**Dove ho faticato / cosa non era ovvio**
- **Decidere quali collaudi *non* aggiornare.** Il riflesso era cercare `claude-sonnet-4-6` e sostituirlo ovunque, e sarebbe stato sbagliato due volte: i collaudi che descrivono il corpo della richiesta **del prototipo** devono continuare a nominarlo, perché è il loro mestiere; e quelli che provano che `modelli.json` scavalca il predefinito hanno bisogno di un modello **diverso** dal predefinito. Così il modello del prototipo ha cambiato ruolo — da predefinito a modello di prova — e nel collaudo della forma breve adesso si salta all'indietro invece che in avanti.
- **Una batteria che si chiama «parità» e ammette una differenza va spiegata**, o fra un anno chi la legge penserà che sia stata annacquata per farla passare. Il commento in cima ora dice per esteso che cosa difende oggi e che cosa ha smesso di difendere.

**Cosa ho deciso e perché**
- **Nessun tetto alzato a occhio.** Del +30% sapevo, e la tentazione di aggiungere margine ovunque era forte; ma un margine a naso su 18 prompt sono 18 numeri inventati. L'applicazione un troncamento lo **grida** (`ClientClaude`, causa `Troncata`): si alza il tetto di chi si lamenta davvero. La voce resta aperta in `in_sospeso.md`.
- **Predefinito compilato, non file.** Il salto potevo farlo scrivendo `modelli.json` e lasciando il codice com'era — ma quello è il posto degli esperimenti. Quando una scelta diventa il prodotto va dove il prodotto la porta con sé, anche su un PC che quel file non ce l'ha.

💡 *Mia intuizione / scelta ragionata* — Questa giornata è costata una riga perché l'avevo pagata a T2. L'interruttore a tre stati l'avevo costruito allora, quando non serviva a niente: Sonnet 4.6 il ragionamento lo teneva spento da solo, e dichiararlo sarebbe stato codice morto. L'ho scritto lo stesso perché sapevo che il salto sarebbe arrivato — ed è l'unico tipo di lavoro in anticipo di cui mi fido: non indovinare *cosa* servirà, ma lasciare la porta aperta dove è **certo** che qualcuno busserà. Poi c'è la parte scomoda. Per undici giorni ho difeso la parità col prototipo come se fosse un pezzo del prodotto, e oggi l'ho lasciata andare senza rimpianti perché era un **attrezzo di misura**. Riconoscere il giorno in cui un attrezzo ha finito il suo lavoro è difficile quanto costruirlo: finché non arriva sembra prudenza tenerselo, e il giorno dopo è solo zavorra che ti impedisce di salire di modello.

### Step 2.31 — Una passata sulle cose rimaste indietro, e il difetto che è saltato fuori chiudendone un altro

*T7 era chiusa e T8 non era ancora cominciata: il momento buono per aprire `in_sospeso.md` e chiedermi, voce per voce, se qualcosa si potesse chiudere adesso. Ne sono uscite sei. La settima non era in lista: l'ha trovata la revisione del lavoro appena fatto, ed era la peggiore di tutte.*

**Cosa ho fatto**
- **Ho riletto l'elenco con una sola domanda**: questa la posso chiudere **da questa macchina**? Le voci che vogliono un secondo PC, l'SDK del tutor, un `.docx` salvato davvero da Word o uno schermo al 150% le ho lasciate dov'erano — nominare un debito non è pagarlo.
- **Il ragionamento poteva fermarsi a metà frase senza dirlo.** Sulla strada sincrona un troncamento è un errore e si grida; in streaming no, ed è giusto, perché il testo è già sotto gli occhi di chi legge. Ma il motivo della fine moriva in una riga: `MestiereAi` restituiva il solo testo. Ora torna la risposta intera e il pannello scrive «(fermata qui: ha raggiunto il limite di lunghezza)», gemello del «(interrotto)» che già seguiva le interruzioni mie.
- **I tetti dei token adesso si misurano.** Ogni chiamata lascia una riga in `chiamate_ai.csv` nella cartella dati: quale prompt, il tetto dichiarato, i token spesi e la **percentuale del tetto** consumata. Si ordina per quella colonna e il prompt in difficoltà è la prima riga.
- **Uscire da P7 dalla barra in cima non perde più la bozza.** Il destinatario scritto a mano, le spunte degli allegati e un corpo riscritto — che era costato una chiamata all'AI — li salvava solo il bottone del pannello. Adesso c'è un aggancio d'uscita (`IPannelloCheSalvaUscendo`) che la finestra principale chiede prima di nascondere qualunque pannello.
- **E quell'aggancio ha scoperchiato qualcosa di peggio**, che una revisione indipendente del diff ha visto prima che uscisse: una candidatura senza lettera si teneva **oggetto e corpo di quella di prima**, a video sotto il suo nome e poi scritti nel suo `email.json`.
- **Il destinatario è entrato nell'indice del registro**, così «a chi ho scritto?» non chiede più di aprire le candidature una per una.
- **Due voci che aspettavano il banco, non il codice**: lo strumento di collaudo ha imparato ad **aspettare una condizione**, e il cambio lingua fallito del 📄 CV-1 base l'ho finalmente percorso dal vivo.
- **Ho fatto girare tutto**: 755 collaudi verdi, erano 738.

**Cosa ho imparato**
- **Un difetto silenzioso è peggio di un errore.** La bozza persa uscendo dalla barra non diceva niente, e la volta dopo il pannello rileggeva il disco e mi mostrava la versione vecchia **come se fosse l'ultima**. Un errore lo vedo e reagisco; una perdita muta me la porto dietro credendo che vada tutto bene.
- **Un campo lasciato com'era non è vuoto: è pieno della roba di qualcun altro.** Chi arriva su un soggetto nuovo deve azzerare **tutto** quello che riempirà a condizione, non solo i campi che sa già di dover cambiare.
- **Per vedere l'AI fallire non si toglie la chiave: se ne mette una finta.** Senza chiave non si fallisce a metà — si resta fuori, i bottoni dei pannelli dell'AI restano spenti e al posto che volevo guardare non ci arrivo. La ricetta scritta nella voce di `in_sospeso.md` era sbagliata, e me ne sono accorto solo provandola.
- **Misurare costa meno che indovinare.** I `max_token` del pool sono tarati su Sonnet 4.6 e i tre più stretti sono tutti di livello «ragionamento», cioè in pieno sul cambio di modello. Potevo alzarli a occhio; ho preferito costruire il modo di sapere **chi** sta per lamentarsi.

**Dove ho faticato / cosa non era ovvio**
- **La contaminazione fra candidature non l'ho vista scrivendo il codice.** Era lì da prima, ma il mio aggancio nuovo le ha allargato la bocca — da un bottone solo a **ogni** navigazione — e a stanarla è stata una rilettura del diff fatta con altri occhi. L'ho verificata come si deve: tolta la correzione, la seconda candidatura torna con 81 caratteri di corpo che non sono suoi.
- **Capire di chi fosse il buco della bozza.** Sembrava un difetto di P7 e non lo era: un aggancio d'uscita **non esisteva affatto**, e `IPannelloArea` parlava solo di geometria. Lo stesso buco resta latente in P2, P5 e P4; adesso però l'aggancio c'è, e a loro basterà dichiararlo.
- **Aspettare un bottone non è aspettare che il lavoro finisca**, e con l'attrezzo nuovo la tentazione è più vicina: «Rigenera» è acceso **sia prima sia dopo** il clic, quindi l'attesa si soddisfa in tre decimi di secondo mentre l'AI sta ancora scrivendo. La strada onesta è guardare il **file** che quel lavoro produce.

**Cosa ho deciso e perché**
- **Nessun tetto alzato oggi.** Prima i numeri di un giro d'uso vero, poi la ritaratura: la voce resta aperta in `in_sospeso.md`, ma adesso ha il suo strumento.
- **Il diario dei consumi non deve mai far fallire una chiamata.** Se il file è aperto altrove o il disco è pieno si perde la riga e si tira dritto: una candidatura persa per non aver potuto annotare quanto è costata sarebbe assurda. E dentro non c'è niente di mio — nessun testo, nessun profilo — così cancellarlo non toglie nulla.
- **L'aggancio d'uscita lo dichiara solo chi ha qualcosa da salvare**, oggi il solo P7. Un metodo vuoto ripetuto in sei pannelli non dice a nessuno chi lavora davvero.
- **Il destinatario nell'indice si ricopia dalla bozza, e la bozza resta la fonte**: da lì non si scrive mai all'indietro. È anche la prova pratica che a quell'indice si aggiunge una colonna senza migrare niente, perché si ricostruisce da sé dalle cartelle.
- **Ho aperto una voce nuova invece di chiuderne una in più**: un collaudo che misura il tempo è andato rosso una volta su macchina carica e verde a ogni altro giro. Non è una regressione, ma un collaudo che dipende dal carico prima o poi mente — e mente in tutte e due le direzioni.

💡 *Mia intuizione / scelta ragionata* — Questa passata mi ha convinto che `in_sospeso.md` non è una lista di rimorsi: è un posto dove i difetti **maturano**. Alcune voci le avevo scritte giorni fa dandole per piccole, e rilette a freddo ne è uscita la vera gerarchia — la bozza persa in silenzio valeva molto più di due o tre cose che mi sembravano urgenti. E c'è un'altra cosa che porto via, più scomoda: **il difetto peggiore della giornata è saltato fuori mentre ne chiudevo un altro**, e non l'ho visto io. Correggere un difetto muove il terreno intorno, e proprio lì conviene guardare con occhi che non siano quelli che hanno appena scritto la correzione.

### Step 2.32 — T8a: il guscio del server MCP, e il protocollo che era cambiato tre settimane fa

*T8 apre l'ultima funzione grossa prima del rilascio: l'applicazione che offre sé stessa a un assistente AI esterno. L'ho spezzata in tre — prima il guscio, poi i tool che passano dall'AI, infine quelli che scrivono — e ho cominciato dal guscio. Poi, prima di scrivere una riga, sono andato a rileggere la specifica invece di fidarmi del capitolo che l'aveva descritta a giugno. Ho fatto bene: nel frattempo il protocollo era cambiato del tutto.*

**Cosa ho fatto**
- **Ho spezzato T8 in tre gambe**, come già T3 e T5: **T8a** il guscio (la modalità `--mcp`, il dialogo, i tre tool di sola lettura), **T8b** i tool che chiamano l'AI, **T8c** quelli che scrivono, insieme al lucchetto della cartella dati.
- **Ho riletto la specifica MCP prima di implementarla**, ed è saltato fuori che il **28 luglio 2026** — tre settimane fa — il protocollo ha cambiato pelle: niente più handshake `initialize`, ogni richiesta si autodescrive con la versione dentro il proprio `_meta`, e i server devono rispondere a `server/discover`. Il cap. 09 descriveva i «tre passi canonici» di prima: l'ho riscritto.
- **Il server parla tutte e due le ere** e riconosce quale a ogni messaggio, senza ricordarsi niente: se c'è la versione in `_meta` è la lingua nuova, se arriva un `initialize` è quella vecchia.
- **`--mcp` è entrata dalla porta che l'aspettava**: in `ArgomentiAvvio` c'era un commento che la prometteva a T8, e perfino un collaudo che la usava come esempio di «opzione che non esiste ancora».
- **Tre tool di lettura**: il profilo, il registro delle candidature, e tutto quel che una singola candidatura ha prodotto.
- **Un collaudo che lancia l'eseguibile vero** come lo lancia un client — processo figlio, pipe al posto della console — e guarda le tre cose che contavano: che risponda, che su `stdout` non finisca nient'altro che protocollo, e che si spenga da sé quando l'ingresso si chiude.
- **780 collaudi verdi**, erano 755.

**Cosa ho imparato**
- **Un capitolo di progetto invecchia anche mentre non lo guardo.** Il cap. 09 non era scritto male: era giusto quando l'ho scritto, e nel frattempo il mondo si è mosso. Con le cose mie il design-first funziona — decido io quando una decisione matura — ma quando il disegno poggia su uno standard di qualcun altro, la data di scrittura è una scadenza.
- **Un eseguibile «con finestre» non ha una console, ma i suoi flussi funzionano lo stesso** se è il client a fornirli. È il perno su cui poggia tutta la modalità `--mcp`, e il modo per verificarlo non era provare a mano — senza console non avrei visto niente — ma far lanciare l'exe al banco.
- **La legge dello `stdout`**: lì passa solo il protocollo, e tutto il resto — avvisi, resoconto del montaggio, guasti — va su `stderr`, che i client raccolgono in un file. È la stessa distinzione fra «quel che l'utente deve sapere adesso» e «la diagnostica completa» che l'applicazione fa già fra barra di stato e note; cambia solo dove finiscono.
- **Fra due errori che sembrano uguali c'è di mezzo chi legge.** Un tool che non esiste è un errore di protocollo; un tool che esiste e non ce la fa risponde con un risultato normale, marcato, il cui testo è scritto perché un modello possa correggersi da solo. Sbagliare la corsia vuol dire far arrivare a un lettore un messaggio pensato per un altro.

**Dove ho faticato / cosa non era ovvio**
- **Decidere quale era parlare, senza poter sapere quale parla il client di Mirco.** La matrice di compatibilità della specifica è impietosa: un client vecchio davanti a un server solo nuovo fallisce e basta, e viceversa. Scegliere voleva dire scommettere sulla versione installata oggi sapendo che fra sei mesi la risposta è diversa. Le due porte costano poco perché **i tool sono identici nelle due ere**: cambia solo come si entra.
- **VB non distingue le maiuscole**, e me l'ha ricordato il compilatore: avevo chiamato una variabile `conTesto`, che per lui è `contesto`, cioè il motore montato due righe sopra.
- **Un collaudo è diventato rosso perché il suo esempio è diventato realtà.** `IlPercorsoNonSiMangiaLOpzioneDopo` usava `--mcp` come opzione inesistente e si aspettava due avvisi; ora che l'opzione c'è, l'avviso è uno solo. Non era una regressione da correggere ma un collaudo da aggiornare — e ora dimostra più di prima: quel che veniva inghiottito arriva intero. Gli ho affiancato un gemello con un'opzione davvero ignota, per non perdere il caso che copriva.
- **Avevo scritto nei commenti una promessa che il codice non manteneva**: «non si esce mai per un errore», ma la rete copriva solo l'esecuzione del lavoro, non la lettura del messaggio che arriva. Me ne sono accorto rileggendo, non provando.

**Cosa ho deciso e perché**
- **Due porte invece di una scommessa.** Il server è *dual-era*, e lo dichiara in `server/discover` elencando tutte le versioni che parla.
- **Una richiesta per volta.** Il protocollo permetterebbe di sovrapporle, ma a T8a si legge solo dal disco e la differenza non si vedrebbe. Il conto arriva a T8b, dove un tool dura minuti: allora varrà la pena rivederlo, e non prima.
- **Il JSON si consegna com'è su disco**, senza passare dalle classi del motore: riscriverlo mostrerebbe la mia interpretazione dei file invece dei file, e perderebbe per strada i campi che le classi non conoscono ancora.
- **`leggi_opportunita` raccoglie tutti i `.json` che trova**, invece di chiedere per nome quelli che conosco: a ogni tappa è nato un artefatto nuovo, e così il prossimo si affaccia da sé.
- **Nessun lucchetto a T8a**, e non per rimandare: i tre tool leggono soltanto, e la prova che non toccano niente è che dopo un giro intero **la cartella dati non è nemmeno stata creata**. Il lucchetto nasce con i tool che scrivono, a T8c, che è il momento in cui serve.

💡 *Mia intuizione / scelta ragionata* — La regola «prima ragioniamo, poi costruisci» l'avevo sempre applicata verso l'interno: pensare al disegno prima di scrivere il codice. Qui ha funzionato verso l'esterno, ed è la stessa cosa vista da un'altra parte: mezza giornata spesa a rileggere una specifica che credevo di sapere ha risparmiato una tappa intera da rifare, perché avrei scritto un server perfettamente conforme a un protocollo scomparso — e me ne sarei accorto solo al collaudo finale, con Claude Desktop davanti che non risponde e nessuna idea del perché. La cosa che porto via è più scomoda della soddisfazione: **non l'ho verificato perché sospettavo qualcosa**, l'ho verificato per abitudine. Se quella volta avessi avuto fretta, il capitolo mi avrebbe mentito con la mia stessa voce.

### Step 2.33 — T8b: i sette tool dell'AI, e il ciclo che smette di essere sordo

*La seconda gamba di T8: i tool che passano dall'AI. Sulla carta era la più facile delle tre — le porte del motore esistono già, «avvolgi ed esponi» — e infatti la parte difficile non è stata scrivere i sette tool, ma accorgermi che due delle cose che davo per scontate non erano vere: che il ciclo di T8a andasse bene così, e che il confronto fosse una porta sola.*

**Cosa ho fatto**
- **Ho deciso quattro cose prima di scrivere una riga**, ed è la parte che ha orientato tutto il resto: il server serve **più richieste insieme**; i tool dell'AI **restano in vetrina anche senza chiave**, e falliscono dicendo perché; il diario dei consumi **si tiene** e si corregge il capitolo che lo contraddiceva; i documenti passano **sempre** dall'anti-slop, come nell'applicazione.
- **Ho rifatto il ciclo del server**, che a T8a serviva una richiesta per volta. Adesso il filo che legge riconosce il messaggio, mette da parte il lavoro e torna ad ascoltare; sull'uscita si scrive uno alla volta; un `notifications/cancelled` ferma il lavoro che nomina; e alla chiusura dell'ingresso quel che è in volo si **annulla** invece di finire.
- **Sette tool**: `analizza_annuncio`, `confronta`, `mitiga`, `struttura_cv`, `genera_cv`, `genera_lettera`, `rifinisci_testo`. Il profilo lo leggono da disco, non lo accettano come parametro.
- **Quattro ritocchi al motore**, nessuno di allineamento: ognuno toglie una seconda copia di una regola che deve restare una — la forma JSON del punteggio, l'estrazione dei giudizi da un confronto senza padrone, la domanda «ci sono gap da mitigare?», e il confrontatore esposto accanto al generatore.
- **Ho riscritto due pezzi del cap. 09**: il paragrafo sul parallelo e sull'annullamento, e due note sotto la tabella dei tool.
- **796 collaudi verdi**, erano 780. Versione **0.3.034**, Pool **1.11** invariato — nessun prompt toccato.

**Cosa ho imparato**
- **Un collaudo che non ho provato a far fallire non so se funziona.** Ho scritto cinque prove sul ciclo nuovo, le ho viste verdi, e invece di essere contento ho rotto il ciclo apposta — l'ho reso di nuovo seriale — per vedere quali cadevano. Tre sono cadute. Una no, e non perché sia scritta male: verifica un'invariante che il seriale rispetta comunque. Saperlo è diverso dal sospettarlo, e adesso so esattamente che cosa quel collaudo copre e cosa no.
- **La differenza fra «più veloce» e «capace di sentire».** Ho scelto il parallelo e per un po' ho creduto che il motivo fosse la velocità. Non è quello: un server che aspetta un lavoro di minuti non è lento, è **sordo** — non può nemmeno leggere il messaggio con cui gli si dice di smettere. Il gettone di interruzione ce l'avevo da T7c e non serviva a niente finché nessuno poteva consegnarmelo.
- **Una porta del motore non è automaticamente un tool.** `ConfrontaAsync` della pipeline fa tre cose insieme — giudica, calcola, mitiga — e per di più fa avanzare lo stato dell'opportunità. Il capitolo però chiede `confronta` e `mitiga` separati e senza scrivere niente. Non era «avvolgi ed esponi»: era ricomporre quel passo dai pezzi sotto.
- **Le regole di prodotto vanno inseguite anche nelle porte nuove.** Il profilo che mando ai prompt lo serializzo passando dall'oggetto e non dal file grezzo, perché su `confronto` e `mitigazione` la parità col prototipo si misura carattere per carattere: dal file uscirebbe un testo somigliante, e somigliante non basta.

**Dove ho faticato / cosa non era ovvio**
- **T8b non aggiunge a T8a: lo rimaneggia.** Tutto il motore che dovevo avvolgere è asincrono, mentre il ciclo di T8a era sincrono fin dentro il catalogo. Non esisteva la strada «lascio com'è e aggiungo»: o bloccavo il ciclo su ogni chiamata, o lo rifacevo. Sono trenta chiamate di collaudo riscritte per una decisione che sembrava riguardare solo il futuro.
- **Un difetto trovato mentre ne sistemavo un altro.** Mettendo il turno di parola sul diario dei consumi ho visto che la domanda «il file esiste già?» stava **fuori** dal turno: due chiamate simultanee la trovavano entrambe negativa e l'intestazione del CSV finiva scritta due volte. Non l'avrei mai cercato — l'ho visto solo perché stavo guardando quelle sei righe per un altro motivo.
- **Un collaudo di T8a era già fragile, e la batteria più carica l'ha stanato.** Aspettare che il processo sia morto non vuol dire che i lettori asincroni di `stderr` abbiano consegnato tutto: passava da solo tre volte su tre e cadeva insieme agli altri. Per un momento ho creduto di aver rotto il server.
- **Di nuovo le maiuscole di VB.** Avevo chiamato `letta` una variabile che riceveva il risultato della funzione `Letta`: per il compilatore sono lo stesso nome, e la chiamata è diventata un indice sulla stringa. È la seconda volta in due tappe, e ormai è un riflesso che devo farmi venire prima e non dopo.

**Cosa ho deciso e perché**
- **Parallelo, con un solo scrittore sull'uscita.** La riga è la cornice del messaggio, e due risposte che escono insieme la spezzerebbero: il costo del parallelo è tutto lì, ed è un lucchetto.
- **A una richiesta ritirata non si risponde.** Chi ha annullato non aspetta più niente su quell'identificativo, e mandargli un errore vorrebbe dire raccontargli un guasto che ha causato lui apposta.
- **I tool dell'AI restano in vetrina anche senza chiave.** Nasconderli sarebbe peggio in due modi: il client tiene l'elenco da parte e non gli ho promesso nessun avviso di cambiamento, e un tool che sparisce si annuncia come «Unknown tool» — che a un modello dice «hai sbagliato nome», mandandolo a cercare l'errore dove non è.
- **I documenti escono rifiniti, come quelli dell'applicazione.** Costa una seconda chiamata. L'alternativa era che il CV chiesto via MCP fosse silenziosamente peggiore di quello chiesto dalla finestra: una differenza che non avrebbe dichiarato nessuno e che avrei scoperto mesi dopo, confrontando due documenti senza capire perché uno è più piatto.
- **`mitiga` senza gap non chiede niente all'AI.** La lista uscirebbe vuota comunque: è la stessa scorciatoia che fa la fila dentro l'applicazione, ed è deterministica — non dipende da come il modello si sente quel giorno.
- **Il diario dei consumi resta, e il capitolo si corregge.** Marcare quei sette tool «non scrive dati» era vero per i dati dell'utente e falso alla lettera: ogni chiamata annota una riga e fa nascere la cartella. Ho preferito correggere la descrizione che togliere la misura — è proprio quella che serve per ritarare i tetti dopo il salto a Sonnet 5.

💡 *Mia intuizione / scelta ragionata* — La cosa che porto via da questa gamba non è il parallelo: è il momento in cui ho rotto il mio codice di proposito. Avevo cinque collaudi verdi e la sensazione, che conosco bene, di aver finito. Renderli rossi a comando è stata l'unica cosa che ha trasformato quella sensazione in una notizia: tre di loro parlano davvero del parallelo, uno no. Se non l'avessi fatto, avrei continuato a credere di avere cinque prove invece di tre più una, e la differenza si sarebbe vista solo il giorno in cui una regressione fosse passata in mezzo senza far cadere niente. **Un collaudo verde dice che il codice fa quel che si aspetta; solo un collaudo che ho visto diventare rosso dice che si sarebbe accorto del contrario.**

### Step 2.34 — T8c: il lucchetto, e la regola scritta mesi fa che mi ha fermato

*L'ultima gamba di T8: i due tool che scrivono e il lucchetto della cartella dati, cioè la cosa che finora avevo potuto rimandare perché nessun tool toccava i file dell'utente. Mi aspettavo che il pezzo delicato fosse il lucchetto. Invece il momento che mi resta è un altro: il codice del prodotto ha rifiutato una cosa che stavo per fargli fare, e aveva ragione lui.*

**Cosa ho fatto**
- **Il lucchetto della cartella dati**: un `dati.lock` tenuto aperto in esclusiva. L'applicazione lo prende all'avvio e lo tiene per tutta la sessione; il server MCP lo prende solo per la durata di una scrittura e lo rilascia subito.
- **Due tool che scrivono**: `salva_opportunita`, che mette una candidatura nella coda, e `esporta_documento`, che ne impagina il CV e la lettera in DOCX.
- **Ho tolto `esporta_backup` da questa tappa**: espone il backup, che è la funzione F7 e si costruisce a T9. Nell'applicazione quel bottone è visibile e spento, e lo dichiara.
- **Ho riscritto il §9.4**, che descriveva un lucchetto in tre righe e senza il perché, e ho aggiunto due note al §9.3.
- **812 collaudi verdi**, erano 796. Versione **0.3.035**, Pool **1.11** invariato.

**Cosa ho imparato**
- **Il pericolo non era quello che pensavo.** Avevo in testa due processi che si accavallano sullo stesso file. Ma le mie scritture sono già atomiche una per una: quel che resta sul disco è comunque un file intero. Il danno vero è un altro e nessun file corrotto lo segnalerebbe — l'applicazione tiene un profilo aperto in memoria, il server lo cambia sul disco sotto di lei, e al primo salvataggio lei riscrive sopra quel che non ha mai visto. **Il lucchetto non protegge i byte, protegge quello che uno dei due si ricorda.**
- **Un lucchetto vuoto è più robusto di uno che racconta qualcosa.** La tentazione era scriverci dentro il numero del processo e l'ora. Ma allora qualcuno dovrebbe *ripulirlo*, e prima o poi l'utente resterebbe chiuso fuori dai suoi dati per un file rimasto lì dopo un crash. Tenendo aperto il file e basta, a rilasciarlo è Windows — sempre, e comunque il processo muoia.
- **Due che scrivono non sono simmetrici, e il lucchetto non deve fingere che lo siano.** L'utente davanti alla finestra lavora per un'ora; il server risponde a una richiesta e si dimentica tutto. Dare a tutti e due la stessa regola sarebbe stato elegante e sbagliato.

**Dove ho faticato / cosa non era ovvio**
- **Il codice mi ha corretto.** Avevo fatto sì che `salva_opportunita` deducesse lo stato dagli artefatti ricevuti: con un CV, «generata». Il collaudo è diventato rosso con un messaggio che non arrivava dal mio collaudo ma dal prodotto: *«Da "nuova" non si passa a "generata"»*. La macchina degli stati del cap. 07.3, scritta a T5, mi stava impedendo di registrare una candidatura che non poteva essere successa — un CV mirato **nasce** dai giudizi, e uno senza confronto è una storia senza inizio. Non ho aggirato il vincolo: ho fatto rifiutare quel salvataggio, spiegando che manca il confronto.
- **`esporta_backup` non aveva niente sotto.** L'ho scoperto cercando la classe che scrive il backup e trovando invece un bottone spento con un tooltip che diceva «arrivano con la tappa T9». Il capitolo elencava il tool da giugno, e a leggerlo sembrava una cosa fatta.
- **Il PDF vuole una finestra.** La stampante crea una `Form` e ci mette dentro un WebView2: in modalità `--mcp` non c'è nulla di tutto ciò, perché il programma biforca prima di ogni preparativo grafico. Il ripiego però esisteva già nel motore — «senza stampante, i soli DOCX» — e non ho dovuto inventarlo.
- **Le triple virgolette.** Gli script con cui modifico i file si sono scontrati con i commenti XML di VB, che cominciano per `'''`, e con le stringhe VB, che raddoppiano le virgolette. Ho perso un giro prima di smettere di combattere e usare lo strumento giusto.

**Cosa ho deciso e perché**
- **`salva_opportunita` prende tutto**, non il solo annuncio come diceva il capitolo. Con il solo annuncio, quel che i tool di T8b producono non avrebbe dove andare: si potrebbe generare un CV via MCP e non poterlo mettere da nessuna parte, e `esporta_documento` non avrebbe mai niente da impaginare.
- **Le stelle si ricalcolano anche qui.** Chi chiama può consegnare un confronto che si dichiara perfetto: il punteggio lo rifà il programma dai giudizi, e l'hard-gate scatta lo stesso. È il punto in cui il punteggio finisce su disco, ed è proprio lì che non deve poter essere negoziato a parole.
- **Solo DOCX via MCP, e detto nel risultato.** Un tool che tace su quel che non fa è un tool che sembra rotto.
- **L'app avvisa e parte lo stesso** se non ottiene il lucchetto, mentre il server rifiuta di scrivere. Fermare l'utente sulla porta di casa sua sarebbe sproporzionato; lasciar scrivere il server sotto i piedi di una finestra aperta no.

💡 *Mia intuizione / scelta ragionata* — La cosa che porto via non l'ho scritta oggi. Una regola messa nel posto giusto mesi fa — la macchina degli stati dentro `Opportunita`, non nel pannello che la usava — ha fermato un pezzo di codice nuovo, scritto per una porta che a T5 non esisteva nemmeno, che stava per registrare una candidatura impossibile. Se quel controllo fosse stato nell'interfaccia, come viene naturale metterlo, il tool MCP l'avrebbe scavalcato senza che nessuno se ne accorgesse: sarebbe finita in coda una candidatura «generata» che non è mai stata confrontata, e me ne sarei accorto mesi dopo guardando un registro che non torna. **Una regola vale quanto il posto in cui l'hai messa: se sta dove passano tutti, difende anche le strade che ancora non esistono.**

### Step 2.35 — Prima di T9: tre debiti pagati, e il collaudo che era cieco proprio dove doveva vedere

*Chiusa T8, invece di aprire subito il rilascio ho riletto `in_sospeso.md` dall'inizio: venticinque voci aperte, e la domanda giusta non è «quali sono importanti» ma «quali posso pagare da questa macchina, adesso». Ne sono uscite tre. Una di loro mi ha fatto scoprire che un collaudo scritto per sorvegliare un guasto preciso non si accorgeva di quel guasto: era verde per il motivo sbagliato.*

**Cosa ho fatto**
- **Ho rifatto il collaudo del silenzio** dello streaming, quello che vigila sul limite «se l'AI tace troppo a lungo, chiudi». Cedeva sotto carico, e per ripararlo bastava poco; ma prima di toccarlo ho voluto sapere se, riparato, avrebbe fatto il suo mestiere. Non lo faceva.
- **Ho portato il PDF dentro il server MCP.** A T8c avevo scritto — nel diario e nel capitolo — che via MCP si potevano avere solo i DOCX. Non era vero: `esporta_documento` ora scrive **DOCX, PDF o entrambi**, e chi non dice niente li vuole entrambi.
- **Ho spostato il filo grafico dal banco al prodotto.** La classe che apre un filo STA con la pompa dei messaggi viveva fra i collaudi da T4b; adesso vive nel motore, e il banco usa quella invece di tenersene una copia.
- **Ho aperto la porta «qui c'è tutto» del profilo**: importare un CV non vuol più dire cercarsi il file a mano, perché il programma propone per nome quello che ha già riconosciuto come il più recente.
- **817 collaudi verdi**, erano 812. Versione **0.3.035**, Pool **1.11** invariato — nessun prompt toccato in questo giro.

**Cosa ho imparato**
- **Un collaudo può essere verde per il motivo sbagliato.** Quello del silenzio mandava quattro pause da 120 ms: la risposta intera durava **551 ms**, cioè meno del secondo di silenzio che il codice concede. Un limite sull'attesa complessiva — esattamente il guasto che quel collaudo doveva impedire — lo avrebbe lasciato passare senza un fiato. L'ho dimostrato invece di dedurlo: ho **falsificato il client**, togliendogli il riarmo dell'orologio a ogni pezzo, e il vecchio collaudo è rimasto verde. Adesso le proporzioni sono rovesciate — ventuno pause da 60 ms, **1260 ms** contro 1000 — ed è insieme più stabile (margine per pausa raddoppiato) e capace di diventare rosso quando deve.
- **«Non si può» va riverificato quando cambia chi lo dice.** Il PDF chiede il motore del browser, il motore del browser chiede una finestra, `--mcp` non crea finestre: la catena sembrava chiusa. Ma il pezzo debole era il secondo anello — al motore non serve una finestra **visibile**, serve un **filo STA con la pompa dei messaggi**, ed è una cosa diversa. La prova che fosse fattibile era in casa mia da quattro tappe: il banco stampa PDF veri da un processo senza finestre fin da T4b.
- **Una cosa già calcolata e mai riletta è come se non ci fosse.** La classificazione dei documenti sapeva qual era il CV più recente e lo scriveva pure su disco. Nessuno lo leggeva. Il lavoro era fatto per intero e all'utente non arrivava niente: il pezzo mancante non era il ragionamento, era **una riga che va a prenderlo**.

**Dove ho faticato / cosa non era ovvio**
- **Riscrivere un capitolo per dire il contrario di ieri.** Il cap. 09 spiegava con tanto di motivo perché via MCP i PDF non si potessero fare. L'ho riscritto; ma il cap. 14, che è la storia del piano di lavoro, quella frase la **tiene** con una nota che dice quando è caduta. Cambiare il progetto e cambiare la cronaca sono due gesti diversi, e la cronaca non si corregge: si annota.
- **Il file proposto poteva non esserci più.** La classificazione è di ieri, l'import è di oggi: fra i due l'utente può aver spostato o cancellato quel CV. Perciò l'esistenza si controlla **quando si propone**, non quando si è classificato — e se manca, il programma non lo nomina nemmeno.
- **Tre vie d'uscita, non una.** Un programma che propone e basta è un programma che decide. Chi apre l'import trova il suggerimento, ma anche il modo di scegliere un altro file, di non usarne nessuno e di andarsene: la proposta è una comodità, non un binario.
- **Due copie della stessa idea, e una era nel posto sbagliato.** Il filo grafico stava fra i collaudi: finché serviva solo a stampare in prova andava bene, ma nel momento in cui serve al prodotto la copia buona è quella del prodotto. Ho spostato quella e cancellato l'altra, invece di lasciarne due che divergono.

**Cosa ho deciso e perché**
- **Prima misuro il collaudo, poi lo riparo.** Avevo davanti una riparazione da due minuti. Farla subito avrebbe prodotto un collaudo stabile e cieco, cioè la cosa peggiore: uno che dice di sorvegliare qualcosa e non lo sorveglia.
- **`formati` predefinito «entrambi».** Chi chiama via MCP è un modello, e un modello che non nomina il formato non sta scegliendo il DOCX: sta chiedendo «i documenti». Dargli tutto è la lettura più fedele della richiesta, e nessuno resta senza il file che voleva.
- **Il PDF non lo si esporta se non c'è la finestra? Allora la finestra la creo io, e senza mostrarla.** È il prodotto che si adatta alla porta nuova, non la porta nuova che consegna meno.
- **Il CV più recente si propone, non si impone.** È l'unico modo in cui una scorciatoia resta una scorciatoia.

💡 *Mia intuizione / scelta ragionata* — Il momento della giornata è stato togliere di mano al client il riarmo dell'orologio e vedere il collaudo restare verde. Avevo davanti la prova, in una riga di output, che per settimane avevo creduto di avere una sentinella dove c'era un lampadario acceso. Non è colpa di chi l'aveva scritto: le pause sembravano abbastanza lunghe, il totale non l'aveva calcolato nessuno. **Un collaudo che vigila su un limite deve essere costruito in modo che quel limite sia l'unica cosa che lo può salvare: se ci sono due ragioni per cui è verde, non ne è rimasta nessuna.**

### Step 2.36 — Il prompt che perdeva per conflitto, e tre tetti che nessuno doveva alzare

*Il secondo giro sui debiti, quello che non si poteva fare a secco: cinque voci che chiedevano tutte la stessa cosa, cioè spendere chiamate vere e guardare cosa succede. Sono le domande a cui nessun collaudo senza rete può rispondere — quanto consuma davvero un prompt, cosa fa il programma se premo «Interrompi» mentre l'AI sta scrivendo, e se una frase che sparisce sparisca sempre o solo ogni tanto.*

**Cosa ho fatto**
- **Ho misurato il «corso senza nome»** invece di continuare a ricordarmelo come una stranezza. Nella traccia del dialogo, Anna dice al turno dei contatti: *«Ah, e ho fatto anche un corso, ma non mi ricordo più né quale né dove»*, e quella frase spariva. Era annotata da tempo come varianza del modello, «una volta su tre».
- **Ho curato il prompt e fatto il bump**: `profilo/contatti` da 1.2 a 1.3, **Pool 1.12**, impronte rigenerate, voce nel CHANGELOG.
- **Ho misurato i tetti dei token** con un giro completo dentro l'applicazione: tredici chiamate, dall'import del CV fino all'email.
- **Ho premuto «Interrompi» mentre l'AI scriveva**, a 3,9 secondi dall'inizio della risposta.
- **Ho letto a video il prima/dopo** della rifinitura e ho provato l'import del profilo **su una pagina che non è LinkedIn**.
- **817 collaudi verdi**, più **15 reali**. Versione **0.3.035**, Pool **1.12**.

**Cosa ho imparato**
- **«Una volta su tre» era una diagnosi vecchia di un modello fa.** Misurata adesso, dopo il salto a Sonnet 5, la varianza non c'era più: **tre giri su tre**, sistematica. E un difetto sistematico non si spiega con l'umore del modello — si spiega col testo che gli ho dato.
- **A perdere non era una regola mancante: era una regola in conflitto con la sua vicina.** L'istruzione c'era già, per esteso — *«un titolo, un diploma o un corso → formazione»*. Ma davanti a un accenno che si auto-nega (c'è un corso, ma non so quale né dove) vinceva **«non aggiungere e non inventare nulla»**, e il modello leggeva «qui non c'è materiale» invece di «c'è un accenno da passare avanti». Due regole giuste che si contendevano la stessa frase, e a perdere era **l'anti-perdita**.
- **L'ordine di lettura è una leva.** La cura non aggiunge nessun giudizio nuovo: una riga che dice che un accenno vale anche quando è incompleto, e che a chiedere il resto sarà il turno di destinazione, che ha l'utente davanti. Sta **prima** della regola del non inventare, e questo basta a sciogliere l'ambiguità invece di crearne un'altra. È la quarta volta che questo pool mi insegna la stessa famiglia di lezioni, dopo il 1.05, il 1.07 e il 1.11: **non conta solo cosa scrivo in un prompt, conta accanto a cosa e in che ordine.**
- **Misurare a volte risparmia il lavoro invece di crearlo.** I tetti dei token di Sonnet 5 erano aperti da T2 e me li immaginavo stretti, perché il modello nuovo conta circa un terzo di token in più a parità di testo. I tre sospetti erano davvero i primi tre, ma il peggiore consuma **poco più di un quarto** di quel che ha — `email_candidatura` 27,1%, `umanizzazione_sintesi` 25,0%, `umanizzazione_frasi` 18,2% — e tutte e tredici le righe finiscono con `end_turn`, cioè nessuna risposta è stata tagliata. **Nessun tetto da alzare**: il debito si chiude con un numero, non con una modifica.
- **Un collaudo che non fallisce mai smette di misurare.** Curato il prompt, la traccia del dialogo non ha più nessuna trappola che le sfugga: è una buona notizia e insieme un debito nuovo, perché da domani quel collaudo passa sempre. L'ho segnato invece di godermelo.

**Dove ho faticato / cosa non era ovvio**
- **La prima interruzione l'ho premuta a vuoto.** Chiedere allo strumento di collaudo di aspettare e poi, in una seconda richiesta, di premere, vuol dire arrivare quando l'AI ha già finito: fra le due c'è tutto il tempo di andata e ritorno. L'attesa e il clic devono stare **nella stessa invocazione** — è costato un tentativo, ed è finito nel `README.md` dello strumento, che è l'unico posto dove quel sapere sopravvive.
- **Il prima/dopo l'ho letto a metà.** A video la colonna del CV lo mette sotto la piega, e lo strumento di collaudo sa fotografare ma non scorrere: ho verificato quel che si vedeva e ho **dichiarato il limite** invece di scrivere «verificato» e basta.
- **Una pagina che non è LinkedIn è un collaudo più onesto di una che lo è.** L'import è nato guardando un profilo LinkedIn, e provarlo lì dimostra poco. Su un sito aziendale moderno a pagina unica lo scorrimento ha lavorato fino al piè di pagina, e la cosa che volevo vedere è arrivata: ciò che quella pagina **non dice** di una persona è uscito **vuoto**, non inventato.
- **Cinque voci chiuse, cinque nate.** Nessuna grave — markdown grezzo che si vede nella chat del brainstorming, una finestra stretta da guardare col mouse prima di chiamarla difetto, parole incollate fra i blocchi di una pagina letta, un 404 raccontato come problema di rete, e la traccia da rinforzare. Il numero delle voci aperte non è sceso, ed è giusto così: guardare il programma da vicino **produce** debiti, e non annotarli sarebbe l'unico modo di farli sembrare inesistenti.

**Cosa ho deciso e perché**
- **Prima misuro il difetto, poi lo curo, poi lo rimisuro allo stesso modo.** Tre giri prima, tre giri dopo, e le altre tre trappole della traccia controllate perché la cura non spostasse il problema altrove.
- **Curare col minimo indispensabile.** Avrei potuto riscrivere la sezione delle regole. Una riga, messa nel punto giusto, cambia il comportamento senza aggiungere superficie su cui sbagliare la prossima volta.
- **Il debito dei tetti si chiude anche se non cambio niente.** «Verificato che non serve» è una risposta, e lasciarlo aperto sarebbe stato un modo di tenere in vita una preoccupazione già smentita dai dati.
- **Il gruppo C non lo faccio io.** Le quattro voci che restano di T8 vogliono una macchina Windows con un client MCP vero: le lascio scritte per intero — cosa provare, in che ordine, cosa guardare — e le fa Mirco. Fingere di poterle chiudere da qui sarebbe la peggior forma di debito, quella che si crede pagata.

💡 *Mia intuizione / scelta ragionata* — La cosa che porto via è come è cambiata la diagnosi. Per settimane quella frase che spariva è stata «il modello ogni tanto fa così»: una spiegazione che non chiede di fare niente, perché mette la causa fuori dalla mia portata. Bastava contare fino a tre per scoprire che era sistematica, e da lì la domanda è diventata un'altra — non *perché il modello sbaglia*, ma *quale mia riga lo sta convincendo a sbagliare*. La risposta non era una regola mancante: erano due regole mie, giuste tutte e due, messe una accanto all'altra senza dire quale viene prima. **Quando il modello disobbedisce sempre, non sta disobbedendo: sta obbedendo a un'altra riga che ho scritto io.**

### Step 2.37 — Il gruppo C: il client vero, e la porta da cui la finestra non sa rientrare

*Le quattro voci rimaste di T8 chiedevano tutte la stessa cosa: un client MCP vero. Fino a ieri il mio server era stato interrogato solo dal banco e da un `printf` scritto a mano — due interlocutori che non mi smentiscono mai, perché li ho scritti io. Claude Desktop su questa macchina non c'è, ma il client vero ce l'avevo davanti da settimane senza vederlo: Claude Code, che di MCP è un client a tutti gli effetti. L'ho registrato fra i suoi server, ho riavviato la sessione, e i dodici tool sono comparsi.*

**Cosa ho fatto**
- **`tools/list` da un client vero**: dodici tool visibili come `mcp__trovalavoro__*`, nomi e descrizioni sensate. E una cosa che il `printf` non poteva mostrarmi: il client ha ricevuto e **mostrato le istruzioni del server** — «il punteggio in stelle è calcolato dal programma e non si può negoziare a parole». Quel testo l'avevo scritto per un lettore che fino a ieri non esisteva.
- **Le letture sui dati veri**: profilo e registro, sette opportunità. Il client guardava la cartella giusta.
- **Il lucchetto fra due processi veri.** Ho aperto la finestra e ho chiesto al server di salvare: rifiuto pulito, non un crash — *«La cartella dati è in uso da un altro processo… Chiudi la finestra e riprova»*, con l'elenco di cosa continua a funzionare. Chiusa la finestra, stessa identica chiamata: riuscita. È il gesto che il banco non poteva fare, perché lì erano due `FileStream` dentro un processo solo.
- **Il confronto e la generazione, misurati contro la finestra**: **0,9 stelle da entrambe le porte**, e un CV il cui **scheletro dei fatti è identico** — stesso tipo, stessa intestazione, gli stessi otto ruoli con aziende e durate uguali carattere per carattere, le stesse sedici competenze. Varia solo la prosa, che è ciò che l'AI scrive.
- **`chiamate_ai.csv` è nato.** Non esisteva: nella cartella dati vera nessuna riga era mai stata scritta. Adesso ce ne sono quattordici, sei dal server e otto dalla finestra, tutte sotto il 50% del tetto e tutte chiuse con `end_turn`.
- **Non ho toccato una riga di codice**: nessun collaudo nuovo, **817 verdi** invariati, versione **0.3.035** e Pool **1.12** invariati. Sono usciti due difetti, e li ho scritti invece di curarli.

**Cosa ho imparato**
- **Un client vero fa cose che nessuno gli ha detto di fare.** Le istruzioni del server, che il banco ignora perché non ha un modello a cui darle, un client le legge e le passa. È esattamente il tipo di superficie che un collaudo scritto da me non poteva esercitare: il banco prova quel che so già.
- **La prova migliore non sono due numeri uguali: è la stessa regola vista lavorare su ingressi diversi.** Le due porte hanno dato 0,9 stelle passando per aritmetiche diverse — la finestra 36 di base e 18 di stima, il server 37 e 15. Con `clamp_giu = -20`, il primo caso corregge di −18 e non taglia niente, il secondo vorrebbe correggere di −22, viene tagliato a −20 e dichiara «scarto tagliato». Poi 18/20 e 17/20 arrotondano tutti e due a 0,9. Il conto torna a mano da entrambi i lati, e viene da **una funzione sola con tre chiamanti**: se avessi trovato due numeri identici sarei stato meno sicuro, non di più.
- **Il diario dei consumi è diventato lo strumento con cui ho misurato la parità.** L'analisi dell'annuncio è uscita **identica al token** sulle due porte — 2896 in ingresso, 902 in uscita, 11,3% del tetto — perché era lo stesso prompt sullo stesso testo con lo stesso modello. Le altre tre righe differiscono di **un token**, e solo perché a monte i giudizi non erano gli stessi. Uno strumento nato per sorvegliare i costi ha finito per dimostrare un'identità.
- **La varianza del modello si misura da sé, se guardo bene.** Due giri fatti dallo stesso server, a venti minuti di distanza, differiscono fra loro quanto il server differisce dalla finestra. Questo chiude la domanda in un modo che nessuna coincidenza avrebbe chiuso: la differenza residua non è della porta, è del modello.
- **Aprire una seconda porta crea stati che la prima non sa gestire.** Il server MCP sa creare un'opportunità ferma al solo annuncio — lo stato «nuova» — che l'applicazione, in tre mesi, non aveva mai scritto su disco: archivia solo dopo il confronto. La finestra quella candidatura la riapre, ma non la può proseguire.

**Dove ho faticato / cosa non era ovvio**
- **Il piano che avevo scritto non si poteva eseguire.** Prevedeva di salvare un'opportunità via MCP e poi confrontarla dalla finestra: ma «Analizza» si accende solo se nella casella dell'incolla c'è del testo, e riaprendo una candidatura non ancora confrontata quella casella viene svuotata. Il testo grezzo dell'annuncio non lo conserva nessuno — né il server né l'applicazione, i due `annuncio.json` hanno le stesse identiche chiavi. Ho cambiato la forma del gesto: **lo stesso testo grezzo fatto entrare da tutte e due le porte**. È venuto un collaudo più severo di quello che avevo progettato, perché mette a paragone l'intera catena e non solo l'ultimo anello.
- **Due processi con lo stesso nome, e uno era il client.** In lista c'erano due `TrovaLavoro.exe`: la finestra e il server MCP della sessione. Chiudere quello sbagliato avrebbe spento il client con cui stavo collaudando. Li ho distinti dall'orario di avvio e dal titolo della finestra prima di toccarli, e ho usato la chiusura gentile invece dell'ammazzata. Rifacendo i conti dopo, ho scoperto che ero passato accanto a una trappola peggiore: il mio strumento di collaudo chiude **per nome** (`taskkill /IM TrovaLavoro.exe /F`), e chiamare `compila` mentre il client parlava col server gli avrebbe spento sotto i piedi l'interlocutore. Non è un rischio che avevo previsto: è nato il giorno in cui l'applicazione ha smesso di essere un processo solo. L'ho scritto nel `README.md` dello strumento, che è dove quel sapere sopravvive.
- **Un giudizio mancante l'ho quasi preso per una differenza fra le porte.** In un giro il confronto non ha emesso i cinque giudizi di «contesto» — titolo, sede, contratto, mansioni, benefit. Prima di chiamarlo difetto sono andato a rileggere il prompt, che li pretende per scritto: *«Dai un giudizio per OGNI campo di contesto presente… non saltarne nessuno»*. È disobbedienza al prompt, non asimmetria fra le due strade — e infatti è successo su una chiamata del server, mentre gli altri tre giri, server e finestra, li hanno emessi tutti.

**Cosa ho deciso e perché**
- **Il client vero è Claude Code, non Claude Desktop.** Ho smesso di aspettare un'installazione che non arrivava per una cosa che era già in casa. Il protocollo è lo stesso; a smentirmi serve un client che non ho scritto io, non un client con una finestra.
- **I due difetti li annoto, non li curo oggi.** Il vicolo cieco dello stato «nuova» è mezz'ora di lavoro e appartiene a T9, dove la revisione dei pannelli è già in programma. Il contesto saltato va **misurato** prima di essere curato: uno su quattro non è un dato, e la lezione del «corso senza nome» è fresca — lì «una volta su tre» era una diagnosi vecchia di un modello fa.
- **Ho lasciato i dati di prova nella cartella vera.** Le due candidature del 21 agosto restano: la prima è la dimostrazione viva del vicolo cieco, la seconda è la prova del confronto. Cancellarle adesso vorrebbe dire buttare l'unica cosa che rende riproducibile quel che ho visto.

💡 *Mia intuizione / scelta ragionata* — Il collaudo di tappa l'avevo rimandato quattro volte, e ogni volta con una buona ragione: manca la macchina, manca il client, tocca a un'altra sessione. Intanto tenevo T8 per chiusa, perché ottocento collaudi verdi sono una sensazione di sicurezza molto convincente. In due ore di client vero sono usciti **due difetti che quegli ottocento collaudi non potevano vedere**, e non per distrazione mia: uno vive nello spazio fra le due porte, e nello spazio fra due cose non guarda nessun collaudo che ne provi una sola; l'altro è il modello che disobbedisce a una mia riga, e il banco non chiama mai il modello. **Il collaudo di tappa non è la cerimonia che chiude quel che ho già verificato: è l'unica parte che verifica quel che non sapevo di non aver verificato.** Quando lo rimando, non sto rimandando una formalità — sto rimandando l'unico pezzo che può ancora dirmi di no.

### Step 2.38 — T9a: il file che riporta indietro, e il collaudo che non vedeva

*Ho aperto l'ultima tappa. T9 è larga e tiene insieme cose che non si somigliano — una funzione nuova, un pannello che non c'è mai stato, due difetti trovati dal collaudo di T8, un rilascio — così l'ho spezzata in cinque gambe con un ramo ciascuna, come avevo fatto per T5, T7 e T8. La prima è quella dei dati: backup e ripristino, la funzione F7, che il bottone di P2 prometteva da mesi standosene spento con scritto «arriva con T9».*

**Cosa ho fatto**
- **Il file di backup**: un solo `.json` leggibile, con in testa il formato e l'elenco di cosa contiene. Due scelte: il **solo profilo** — con lo storico e il 📄 CV-1 base — oppure **tutto**, che aggiunge il registro e le candidature. Fuori restano la chiave API, i documenti impaginati, il diario dei consumi e i file dei numeri.
- **Il ripristino con l'anteprima**: si sceglie il file, si legge *cosa contiene e cosa sostituisce cosa*, e solo dopo si conferma. Finché non si è aperto un backup, «Ripristina» è spento.
- **La finestra**, una sola per le due metà, aperta dal bottone di P2 che adesso si chiama **«Backup…»** e non «Esporta backup»: fa tutte e due le cose, e un'etichetta che ne prometteva una sola avrebbe nascosto l'altra.
- **Il tool `esporta_backup`** del server MCP, che aspettava questa tappa per nascere: i tool sono passati da dodici a **tredici**.
- **26 collaudi nuovi**, **845 verdi** in tutto, versione **0.3.036**. E il giro completo provato **dal vivo** sull'applicazione vera, su una cartella dati usa-e-getta.

**Cosa ho imparato**
- **Un backup che passa dagli oggetti ha già cominciato a perdere qualcosa.** Copio i file **grezzi**: profilo, CV base e artefatti delle candidature entrano come stanno sul disco. Se passassi per le classi del programma, un campo che questa versione non modella — scritto a mano, o da un programma futuro — sparirebbe nel viaggio senza che nessuno se ne accorga. L'unica eccezione è il registro, che è un indice rigenerabile: e infatti al ritorno lo **ricostruisco dalle cartelle**, perché dopo un ripristino sul disco c'è un insieme che nessun indice salvato conosce.
- **«Ripristina» non vuol dire «riporta il disco a quel giorno».** Le candidature che il backup non nomina restano dove sono, e l'anteprima lo dice prima della conferma. Un ripristino che cancellasse il non-nominato sarebbe una perdita silenziosa che nessuno ha chiesto — e cancellare, in questo programma, è un gesto che costa una parola scritta a mano.
- **Un file di backup lo può scrivere chiunque.** I nomi che arrivano da dentro quel file devono restare **nomi di file**: `..\..\fuori` non deve poter far scrivere l'applicazione fuori casa. Non è teoria — l'ho visto: togliendo apposta il controllo, il collaudo ha scritto davvero un `annuncio.json` in una cartella che non era la sua.

**Dove ho faticato / cosa non era ovvio**
- **Due processi tenevano l'eseguibile bloccato, e uno era il mio interlocutore.** Alla prima compilazione MSBuild si è fermato: `TrovaLavoro.exe` era aperto due volte — la finestra di ieri e il **server MCP della sessione di Claude Code**. È la trappola annotata dopo il collaudo di T8, vista dall'altro lato: lì il rischio era spegnere il client, qui il risultato è che finché quei processi vivono **non si compila**. Ho provato a scrivere l'output altrove per aggirarla, e ho scoperto che nemmeno quello basta: il banco cerca la cartella dei casi accanto a sé, e spostata la cartella dodici collaudi falliscono per un motivo che non c'entra niente con quel che stai provando. Alla fine la strada è una sola: chiuderli, e ricordarsi che il server MCP torna solo con un riavvio della sessione.
- **Un collaudo verde che non vedeva niente.** Ho scritto la prova per un difetto trovato dal vivo — l'anteprima diceva «il profilo, come era il 17 agosto» di un profilo salvato il 21 — e l'ho verificata falsificando il codice, come vuole la regola 14. È rimasta **verde**. Il caso che avevo costruito non era quello vero: lo storico conteneva anche la versione di oggi, quindi dedurre la data dallo storico dava comunque la risposta giusta. Ho dovuto cancellare quella versione dallo storico per riprodurre la cartella dati che avevo davanti — e solo allora il collaudo è diventato rosso.
- **La prova dal vivo l'ho fatta parlando allo strumento con `curl`.** Il server MCP di collaudo non era caricato nella sessione, e i server si caricano solo all'avvio: invece di aspettare un riavvio l'ho acceso e interrogato via HTTP, che è come era stato usato il primo giorno. Nel farlo ho scoperto un limite dello strumento: i **bottoni-scelta** non compaiono nell'elenco dei controlli, quindi le due opzioni dell'export si guardano solo in fotografia. L'ho scritto nel suo README.

**Cosa ho deciso e perché**
- **Livello 5, non 6.** Il ripristino sovrascrive dati esistenti, ma il profilo di prima finisce nello storico e le candidature non nominate restano: non è una cancellazione definitiva, e chiedere di ridigitare `TrovaLavoro` qui sarebbe un allarme più forte di quanto il gesto meriti. Allarmi così, ripetuti dove non servono, insegnano a scacciarli anche dove servono. La conferma però parte da **«no»**.
- **La data del profilo viaggia dentro il file.** Potevo lasciare l'euristica che deduce il giorno dall'ultima versione dello storico — funziona quasi sempre — ma «quasi sempre» in una riga che l'utente legge *prima di sovrascrivere il proprio profilo* non è abbastanza. Un campo in più, e i backup scritti senza di lui si leggono lo stesso.
- **Dal server MCP si esporta e basta.** Il ritorno indietro resta nell'applicazione, dov'è l'utente a guardare cosa sostituisce cosa: la stessa ragione per cui non esiste un tool che cambia il profilo.

💡 *Mia intuizione / scelta ragionata* — La regola 14 dice di provare a far fallire un collaudo prima di dirlo buono, e finora l'avevo applicata ai meccanismi difficili: la concorrenza, il lucchetto, il silenzio dello streaming. Qui l'ho applicata a una riga di testo — una data in una frase — e ho trovato un collaudo cieco. Il punto non è che quella data fosse importante: è che **il caso di prova me lo ero costruito io**, e me l'ero costruito comodo senza accorgermene. Il codice sotto era giusto, il collaudo era verde, e non stava misurando niente. La falsificazione non serve a scoprire se il codice funziona: serve a scoprire se **il caso che ho scelto** è quello in cui il codice potrebbe non funzionare.

### Step 2.39 — T9b: il pannello che non c'era mai stato, e il bottone che si è svegliato

*Il bottone «⚙ Impostazioni» stava nella barra in alto da mesi, grigio, con un tooltip che nominava la tappa che l'avrebbe acceso. Quella tappa è questa. P8 è una finestra separata, come il disegno ha sempre detto, e raccoglie quel che è configurazione e non un passo di un flusso: la chiave, le preferenze sui documenti che scriviamo, dove stanno le cartelle, cosa gira sotto il cofano, e i dati dell'utente con le loro due pulizie.*

**Cosa ho fatto**
- **La finestra P8**, aperta dal bottone che si accende: chiave API mascherata e riconosciuta senza rileggerla, **lingua predefinita** dei documenti, **interruttore della rifinitura** anti-slop, cartelle, modelli e pool, e la sezione dei dati.
- **Niente OK e niente Annulla, solo «Chiudi»**: le preferenze si scrivono appena si cambiano, in un `impostazioni.json` fatto per essere aperto e corretto a mano.
- **Richiama e non rifà**: la chiave passa dalla finestra del primo avvio, il backup da quella di T9a, l'eliminazione totale dalla `FinestraConfermaCritica` che vuole la parola riscritta. Le due pulizie sono «Svuota dati di navigazione» (L5) ed «ELIMINA TUTTI I DATI» (L6).
- **Il debito di T7b, chiuso**: l'interruttore anti-slop vale subito, senza riavvio, e **da entrambe le porte** — finestra e server MCP — perché il cap. 09.3 vuole che il CV chiesto da un client sia lo stesso che esce dalla finestra. Da spento l'AI non viene chiamata affatto.
- **33 collaudi nuovi**, **878 verdi** in tutto, versione **0.3.037**, Pool 1.12 invariato. **Sei falsificazioni** provate e tutte cadute (regola 14): il lucchetto cancellato insieme al resto, l'interruttore ignorato, la finestra che salva all'apertura, la lingua predefinita che scavalca quella dell'annuncio, i valori di casa cambiati, l'aggiornamento dei bottoni tolto.

**Cosa ho imparato**
- **`config.json` non è mai esistito, e non serve.** Il cap. 11.1 lo prometteva dall'inizio: aprendo il cassetto per costruirlo ho scoperto che le sue due voci avevano trovato casa altrove tappe fa. Al suo posto `impostazioni.json`, con le sole preferenze.
- **«C'è qualcosa da cancellare?» non si chiede alla cartella, si chiede ai file.** Il bottone rosso contava le voci della cartella dati — ma `Assicura` ne ricrea quattro vuote appena qualcosa la tocca, così subito dopo un'eliminazione totale si sarebbe riacceso per offrire di cancellare niente. Adesso guarda i file, su tutto l'albero.
- **Il secondo difetto l'ha trovato solo l'applicazione vera.** Salvare la prima preferenza crea `impostazioni.json`, che su una cartella nuova è il primo dato che sia mai esistito lì dentro: il bottone dell'eliminazione restava spento finché non si chiudeva e riapriva la finestra. Nessun collaudo scritto a tavolino guarda in quell'ordine.

**Dove ho faticato / cosa non era ovvio**
- **Un'ora persa dentro lo strumento di collaudo, non dentro il prodotto.** Il primo `clic` su un bottone che apre una finestra **non la apre**: ci vuole il secondo. Il sospetto cade naturalmente sull'applicazione — e invece il gestore partiva benissimo dall'inizio. L'ho scritto nel README dello strumento, perché è il solo posto dove quel sapere sopravvive.

**Cosa ho deciso e perché**
- **Tre promesse del disegno le ho mantenute in un'altra forma**, e i capitoli adesso lo dicono. La **cartella dati** si mostra e si apre ma non si sposta: il lucchetto è preso all'avvio e tenuto per la sessione, e spostarla a metà partita vorrebbe dire sfilare i file da sotto a chi ci sta scrivendo. **Modelli e pool si leggono e basta** — `modelli.json` esiste apposta perché cambiare un modello costi una riga e non una build, e il «Sigilla pool» del cap. 04.5 in P8 non ci andrà **mai**, perché il manifest vive nel repo e un exe distribuito sigillerebbe qualcosa che nessuno rilegge. La **cartella documenti** si vede qui ma si gestisce in P7, dove quel giro sa aspettare l'AI e annullarla.
- **Il livello 5 non passa dalla finestra della conferma critica.** Quella è il livello 6, quello della parola da riscrivere. Per «Svuota dati di navigazione» basta una `MessageBox` che parte da «No», come lo «Scarta» di P4: non c'era niente da estendere.
- **Un difetto l'ho lasciato aperto, e scritto.** «⚙ Impostazioni» resta premibile mentre l'AI lavora — la barra spegne gli altri quattro bottoni ma non il quinto — e da lì si arriva fino all'eliminazione totale. Sta in `in_sospeso.md`, assegnato a **T9d**, dove la revisione dei pannelli è già in programma.

💡 *Mia intuizione / scelta ragionata* — Ho scoperto costruendola che una finestra di impostazioni è soprattutto un elenco di **cose che non fa**. Tre voci previste dal disegno sono cambiate di natura appena ho provato a implementarle, e nessuna delle tre per un limite tecnico: ognuna aveva già trovato un posto migliore, o non aveva più senso in un eseguibile distribuito. Un capitolo scritto due mesi fa descrive il mondo di due mesi fa; quando lo si implementa non lo si esegue, lo si **rilegge** — e la differenza fra le due cose sono le tre righe che ho dovuto scrivere nei capitoli per spiegare perché il pannello non fa quel che c'era scritto.

### Step 2.40 — T9c: com'è andata, e chi aspetta da troppo

*La Home sapeva dire quante candidature avevo spedito, e non sapeva dire com'erano finite. Questa gamba porta la seconda metà della domanda «a che punto sono?»: lo stato **esito**, che nello schema c'era da T6 ma dall'interfaccia non si raggiungeva, e il **promemoria di follow-up** per chi aspetta da troppo. Più il vicolo cieco dello stato «nuova», che il collaudo di tappa di T8 aveva trovato tre giorni fa.*

**Cosa ho fatto**
- **L'esito si segna in P4**, col comando «Com'è andata…» acceso solo da «inviata» in poi: un menù con i tre esiti — Colloquio · Rifiutata · Assunto 🎉 — e la spunta su quello di adesso. Si corregge e si toglie.
- **Il promemoria**: **quattordici giorni** di casa, modificabili nella sezione nuova di P8 («Candidature spedite»), **zero per spegnerlo**. Nella Home si fa vedere tre volte — la riga sotto i contatori, i giorni di attesa nella colonna «Stato», la voce «Da sollecitare» nel filtro «Mostra» — e quando non c'è niente da ricordare la riga sparisce e la fascia dei filtri torna alta com'era.
- **Il vicolo cieco chiuso**: sulla candidatura riaperta ferma al solo annuncio, «Analizza» **diventa «Confronta»** e fa il secondo passo da solo. Un testo incollato nella casella ha la precedenza e il bottone torna «Analizza», perché chi scrive lì vuole un annuncio nuovo.
- **Il server MCP se n'è accorto**: `leggi_registro` porta adesso anche l'esito, quando c'è.
- **32 collaudi nuovi**, **910 verdi** in tutto, versione **0.3.038**, Pool 1.12 invariato. **Sette falsificazioni** provate e tutte cadute: lo stato e l'esito che si fidano l'uno dell'altro senza mettersi d'accordo, l'attesa contata anche a chi ha già saputo, lo zero che non spegne più il promemoria, la data del passaggio disfatto lasciata dietro, il vicolo cieco rimesso dov'era, il bottone dell'esito sempre premibile, la voce del menù scollegata dall'azione.
- **Il giro dal vivo**, sull'applicazione vera: il promemoria che compare e sparisce, il filtro, l'esito segnato e corretto, e la candidatura nata davvero dal server MCP portata a termine — trentasei secondi, quindici giudizi, la cartella che avanza a «interessante».

**Cosa ho imparato**
- **Gli esiti sono tre, non i quattro del capitolo.** «Spedita e nessuna risposta» **è già** lo stato `inviata`, ed è già un contatore della Home: registrarlo come esito avrebbe creato due modi di dire la stessa cosa, e il promemoria avrebbe dovuto rincorrerla in due posti. Si registra quel che è **successo**; l'attesa si deduce dal silenzio. Nel menù «in attesa» è rimasta, staccata dalle altre, ma non è una quarta scelta: è il modo di **togliere** un esito segnato per sbaglio.
- **La data dell'esito è quella dell'ultima notizia**, non del primo ingresso come per ogni altro stato. Una candidatura andata in colloquio a settembre e diventata «assunto» a novembre, raccontata con la data di settembre, direbbe una cosa falsa proprio nel punto in cui la storia finisce.
- **Il promemoria non è costato nessun campo nuovo.** Le date c'erano già tutte in `date_stati`, da T4: quel che mancava non era un dato, era una **decisione** — dopo quanti giorni — che il progetto non aveva mai preso.

**Dove ho faticato / cosa non era ovvio**
- **Le voci di un menù contestuale non si premono, con lo strumento di collaudo.** Il menù si apre, le voci si vedono nella fotografia, e il `clic` su «Colloquio» risponde **«Premuto»** senza che succeda niente: menù aperto, spunta dov'era, disco intatto. Due volte di fila, e nemmeno la tastiera aiuta perché il fuoco non è dell'applicazione. Un'altra ora, e di nuovo con il sospetto puntato sul gestore sbagliato: a scagionarlo è bastato un clic **vero** del mouse alle coordinate lette sulla fotografia, e l'esito è finito su disco al primo colpo. Stessa storia per la casella dei giorni in P8, che nell'elenco dei controlli compare come due bottoni «SU» e «GIÙ» marcati spenti, col numero che non si vede da nessuna parte. Entrambe scritte nel README dello strumento.
- **La solita trappola del VB**: una variabile locale che copre la funzione omonima. L'ho rinominata, come già altrove.
- **E la trappola già pagata, ripagata a metà**: il server MCP del prodotto era vivo e teneva bloccato l'eseguibile. Chiuso per PID — mai per nome, che quel nome ce l'hanno in due e uno è il client.

**Cosa ho deciso e perché**
- **L'esito si corregge, e per questo ha un metodo suo.** È una dichiarazione dell'utente, non un fatto osservato: da «rifiutata» si passa ad «assunto» e si torna anche indietro a «inviata». Sono due strade che la macchina degli stati non prevede e non deve prevedere — per tutto il resto vale ancora che indietro non si torna — e le fa `Opportunita.SegnaEsito`, che resta il collo di bottiglia unico anche per il server MCP, come `Avanza`.
- **Si segna in P4, non in Home.** Il cruscotto guarda e non decide (cap. 03.6): nemmeno lo scarto sta lì. E non chiede conferma: quel che si disfa con un secondo clic non ha bisogno di un «sei sicuro?», che resta dov'è servito — sullo scarto, che non si disfa.
- **Il vicolo cieco l'ho chiuso senza toccare lo schema su disco.** L'annuncio era già strutturato: rileggerlo avrebbe voluto dire pagare una chiamata all'AI per riottenere quel che c'era già.
- **Il promemoria guarda le sole spedite senza esito**, e chi ha fatto il colloquio senza più saper niente esce dall'elenco. È una scelta di semplicità — «nessun esito registrato» = «in attesa» — e la seconda attesa esiste davvero: sta in `idee_future.md`, perché farla vorrebbe dire far partire il conto anche dalla data dell'esito.

💡 *Mia intuizione / scelta ragionata* — Il difetto più interessante di questa gamba non l'ha trovato il banco e non l'ha trovato la prova dal vivo: l'ha trovato il **secondo controllo**, quello che faccio dopo, cercando apposta gli effetti collaterali. Aprendo davvero lo stato `esito` — che fino a ieri nessuno raggiungeva — **P7** si è trovata a chiedere un passo all'indietro che non esiste: chi torna a rimandare la stessa email e ripreme «L'ho spedita» su una candidatura che ha già un esito si sentiva rispondere di no. Nessun collaudo poteva vederlo, perché nessun collaudo sapeva che quello stato fosse raggiungibile: prima di oggi non lo era. **Uno stato nuovo non tocca solo il codice che lo scrive: tocca tutto il codice che dava per scontato di non incontrarlo mai.** E quella lista non sta nei file che ho appena modificato — sta in quelli che non ho aperto.

### Step 2.41 — T9d: quel che è acceso deve sembrarlo

*La rifinitura era la gamba dei lavori piccoli: gli errori pannello per pannello, la modifica a mano dei testi, i difetti rimasti in `in_sospeso.md`. È diventata la gamba che mi ha insegnato quanto poco conti che una funzione **funzioni**, se chi guarda non se ne accorge. Due volte nello stesso giorno ho premuto un bottone convinto che fosse morto, e due volte era vivo e stava lavorando.*

**Cosa ho fatto**
- **Quattro tempi, quattro merge fast-forward.** Il primo: la barra che si spegne **tutta** durante una chiamata all'AI — «⚙ Impostazioni» compreso, che era il debito lasciato da T9b — i messaggi che incolpavano la cosa sbagliata, e la regola «una rifinitura che fallisce si dichiara» estesa a P6 e P7, che tacevano.
- **Il secondo tempo**: «Modifica i testi», la finestra da cui riscrivo a mano la prosa che ha scritto l'AI — sommario, descrizioni, corpo della lettera — e solo quella: i fatti vengono dal profilo e lì restano.
- **Il terzo**: i due export **chiedono dove salvare** e aprono Esplora risorse; la voce «📄 Documenti» in barra con la sua tendina; il prima/dopo della rifinitura **tolto del tutto**; il testo catturato dai portali che arrivava tutto attaccato, curato.
- **Il quarto**: la prova dal vivo di chiusura, cinque punti, e le due cure che ne sono uscite — la tendina dei documenti diventata la porta che è, e i bottoni di livello 2 che finalmente si leggono come accesi.
- **955 collaudi verdi**, banco dei copioni 10 su 10, versione **0.3.039**, Pool **1.12 invariato**, **sedici falsificazioni** cadute.

**Cosa ho imparato**
- **Il silenzio è un difetto, anche quando tutto funziona.** «Esporta PDF» scriveva i file da mesi, in una cartella che non avevo scelto e non sapevo trovare, e l'unico avviso era una riga piccola in alto a destra. Per me quel bottone era rotto. Non lo era: era muto.
- **L'occhio legge il testo prima del contorno.** Alla prima cura avevo cambiato il **bordo** dei bottoni esplorativi da grigio ad accento, e sembrava ragionevole. Riprovando dal vivo erano ancora spenti: nero su azzurrino è quasi identico a grigio su grigio, che qui dentro *significa* spento. A cambiare le cose è stato il colore delle **lettere**.
- **Una tendina può essere la porta principale di un pannello e non sembrarlo.** La «Documento:» era nata a destra della lingua, larga uguale, scritta uguale: la funzione era giusta, la gerarchia no.
- **Misurare prima di curare vale anche quando il sospetto è mio.** Il confronto ogni tanto saltava dei giudizi di contesto: otto giri, quattro per porta — tutti 5 su 5. Il prompt non aveva niente da correggere, e il Pool è rimasto dov'era.

**Dove ho faticato / cosa non era ovvio**
- **Un collaudo che cadeva da solo.** `UnaRispostaLungaMaVivaNonScadeMai` misura un tempo, e girava in mezzo alla batteria parallela: subito dopo una compilazione la macchina è carica, la ripresa da una pausa di 60 ms arrivava oltre il secondo concesso, e diventava rosso senza che nel prodotto fosse cambiato niente. Tre volte in due giorni, mai lanciato da solo. L'ho misurato — batteria con build: rosso; tre batterie senza build: verdi — invece di continuare a dargli del ballerino.
- **Il primo giro della cura alla tendina l'ho sbagliato**: l'ho tolta dalla fascia per metterla dentro la cornice nuova, e non l'ho aggiunta alla cornice. Restava orfana, cioè invisibile. L'hanno trovata **tre collaudi**, non l'occhio: quella tendina non compariva più in nessuna finestra.

**Cosa ho deciso e perché**
- **Il prima/dopo della rifinitura via del tutto**, e l'ho deciso dopo aver **guardato i numeri**: su una candidatura vera, cinque ritocchi in tutto — un aggettivo tolto e quattro lineette diventate virgole — e nessun fatto toccato. Un comando in più da capire per mostrare una differenza che non distinguo: la garanzia contro un'anti-slop che sbaglia resta il prompt, che si corregge, e l'interruttore in P8.
- **Il collaudo ballerino curato, non annotato.** Il commento del collaudo stesso dice che uno ballerino non lo guarda più nessuno. Ora è fuori dal parallelismo, e le sue proporzioni non le ho toccate: allargare il tetto del silenzio gli avrebbe tolto la sola cosa che sa fare.
- **La lingua non si tocca.** Ragionare sempre in italiano e tradurre alla fine sembrava più semplice: ma un CV inglese non è la traduzione di uno italiano, sarebbe una chiamata all'AI **in più**, e la traduzione è proprio il passaggio dove la regola anti-invenzione non è più scritta da nessuna parte.

💡 *Mia intuizione / scelta ragionata* — Questa gamba ha trovato i suoi difetti in tre modi diversi, e vale la pena tenerli separati. Il **banco** ha trovato la tendina orfana, che nessun occhio avrebbe visto perché non c'era niente da vedere. Il **secondo controllo** ha trovato il promemoria delle riscritture azzerato troppo presto. Ma le due cose che rendevano l'applicazione peggiore da usare — un export che sembrava non fare niente e dei bottoni creduti morti — non le poteva trovare nessuno dei due: **non erano difetti del programma, erano difetti di quello che il programma dice di sé.** Un collaudo verifica che l'app faccia la cosa giusta; nessun collaudo verifica che l'utente se ne accorga. Per quello serve aprirla e guardarla — e serve farlo **due volte**, perché la prima cura al bordo dei bottoni l'avevo data per buona senza riprovare.

### Step 2.42 — T9e: il marchio entra nell'eseguibile, la checklist si chiede al modello, la domanda saltata torna

*L'ultima tappa doveva essere il rilascio, e invece si è aperta in sei tempi. I primi tre li ho fatti uno dietro l'altro, e hanno in comune una cosa che non avevo previsto: ognuno dei tre ha trovato un difetto che non stava cercando, e in due casi su tre il difetto non era nel prodotto ma nel modo in cui lo controllavo.*

**Cosa ho fatto**
- **Primo tempo: l'applicazione finalmente somiglia a sé stessa.** Lo scudo che stava nell'angolo dai tempi di T1 ora veste l'eseguibile: icona in sette misure, ritagliata sul contorno della figura perché a 16 pixel lo scudo intero era un francobollo dentro un quadrato mezzo vuoto. Una **schermata di avvio** copre i momenti prima della finestra, e «Informazioni su…» si apre dal pannello del logo, che adesso lo dichiara col cursore a manina.
- Il **minimo a video della schermata di avvio è una misura, non un gusto**: dal doppio clic alla finestra passano 265-330 ms, quindi una schermata legata al solo caricamento lampeggerebbe senza che nessuno la legga. Ma il minimo vale per chi guarda, non per chi deve rispondere: chi sta per aprire la domanda della chiave API la manda via subito.
- **Secondo tempo: la checklist ereditata dal prototipo**, otto voci di «Problemi e mitigazioni», percorse una per una sull'applicazione nuova. Sette di quelle otto difese vivono **dentro i prompt del pool**, e un prompt lo si può leggere quanto si vuole: dice quel che il modello *deve* fare, non quel che fa. Così non l'ho chiusa leggendo: ho costruito tre casi avversariali che tentano il modello proprio dove la checklist si dichiara debole — un candidato che si vende, un annuncio scarno di quattro righe, un profilo magro contro un annuncio esigente — e sono rimasti nel banco come collaudi ripetibili.
- **Terzo tempo: la domanda saltata torna.** Un turno chiuso con «passiamo oltre» non si perde più: prima del riepilogo viene riofferto **una volta sola**, chiedendo prima il permesso, e solo se nel frattempo nessun frammento recuperato l'ha già riempito.
- **995 collaudi verdi** alla fine dei tre tempi (erano 955), copioni 10 su 10, versione **0.3.041**, Pool **1.12 invariato** — nessun prompt è stato toccato, quindi nessun bump era dovuto.

**Cosa ho imparato**
- **Una difesa scritta in un prompt non si verifica leggendola.** È la cosa più importante di questa tappa. La checklist elencava otto protezioni e io avrei potuto spuntarle tutte rileggendo i file del pool: sarebbe stato un rito, non un controllo.
- **Le difese stanno a strati, e togliere uno strato non basta a far diventare rosso il collaudo.** Nove falsificazioni, sei cadute. Le tre rimaste verdi dicevano tutte la stessa cosa: la proprietà è difesa in più punti. Cancellare la riga del «non determinabile» da `CalcoloMatch` non cambia niente, perché quell'esito non è nemmeno nella tabella dei punti e la guardia successiva lo scarta comunque — il rosso è arrivato solo mettendocelo dentro.
- **La vera inflazione non è un livello inventato: è l'attenuazione che sparisce in silenzio.** Togliendo le regole di normalizzazione dal prompt delle competenze, il modello ha smesso di scrivere «un po' di inglese» e ha scritto «inglese». Il collaudo non se ne accorgeva.

**Dove ho faticato / cosa non era ovvio**
- **Un difetto vero l'ho trovato falsificando, e stava nel marchio**: alzava la bandiera «già caricato» *prima* di caricare, così un secondo lettore trovava la porta chiusa e la stanza vuota. Nell'applicazione non si vedeva — la legge un thread solo — ma il banco parallelo cadeva un giro sì e uno no. Curato con un lucchetto e la bandiera alzata **dopo** il lavoro.
- **La metà in codice della voce 8 non era guardata da nessuno**: due righe di `CalcoloMatch` — l'esito «non determinabile» escluso dal conto, e la sentinella «nessuna esperienza richiesta» che non deve mai pesare — su cui nessuno dei 973 collaudi di allora posava gli occhi. Il rito della checklist le ha scoperte per caso, cercando altro.
- **Una falsificazione ha appeso il banco invece di arrossarlo.** L'aiutante che rifiuta le riprese non aveva un tetto: rifiutando all'infinito, il collaudo non finiva. Un collaudo che non termina non è un collaudo che passa.

**Cosa ho deciso e perché**
- **La voce 4 della checklist (`pending_questions`) l'ho prima ratificata fuori dalla 1.0, e la sera stessa l'ho riaperta e costruita.** Non è un ripensamento a caso: al cancello T0 la decisione era giusta con le informazioni di allora, ma percorrendo il dialogo per intero ho visto che «passiamo oltre» buttava via una risposta possibile senza dire niente, ed è esattamente la famiglia di difetti che questo prodotto ha giurato di non avere.
- **La ripresa non aggiunge un secondo percorso nel dialogo.** Si aggancia alla passata anti-perdita e gira dopo di lei; chi accetta rientra **nel turno vero**, con la sua domanda e le sue schede di conferma. Un secondo cammino parallelo sarebbe stato codice nuovo da difendere per sempre.
- **Una domanda esce dalla lista quando viene offerta, non quando riesce.** È la lezione dell'anti-rimbalzo applicata alle domande: se uscisse solo riuscendo, chi risponde di nuovo a vuoto se la ritroverebbe davanti all'infinito.

💡 *Mia intuizione / scelta ragionata* — Il secondo tempo mi ha lasciato una domanda che vale più della checklist stessa: quei collaudi rimasti verdi sotto falsificazione erano verdi perché la difesa regge, o perché **non sanno vedere il contrario**? Non è una domanda retorica: è la differenza fra una prova e una cerimonia. Ho risposto costruendo dodici collaudi che mettono in mano a ogni giudice un difetto scritto a mano da me — se il giudice non lo vede, è lui a essere cieco, e diventa rosso. Da qui in avanti, quando un collaudo resta verde sotto falsificazione, ho un modo di sapere quale delle due cose sia.

### Step 2.43 — T9e: il collaudo dal vivo, e i difetti che nessun collaudo verde poteva vedere

*Il quarto tempo non doveva trovare granché: 995 collaudi verdi, tre tempi appena chiusi, un'applicazione che si comporta bene da settimane. Ha reso dodici reperti. E la cosa che mi porto via non è il numero: è che si dividono in due famiglie, e nessuna delle due era visibile da dove stavo guardando prima.*

**Cosa ho fatto**
- **Giro A — il dialogo del profilo, su una cartella usa-e-getta.** La ripresa delle domande saltate, mai vista dal vivo, ha funzionato: la domanda torna una volta sola, prima del riepilogo, chiedendo il permesso; «Ci provo» riporta dentro il turno vero, «Lasciamo così» chiude il discorso.
- **Giro C — una candidatura vera, dall'annuncio all'email.** Cinque giudizi di contesto su cinque — il debito lasciato aperto dal collaudo di T8 si chiude — 0,8 stelle, mitigazione onesta, e **nessuna riga inventata** né nel 🎯 CV-2 né nella ✉️ lettera. Il `.eml` scritto dall'applicazione è corretto fin nelle intestazioni, e il destinatario è vuoto **perché l'annuncio non dava nessuna email**: non se l'è inventata.
- **Giro B — la scala di Windows al 150%.** Cinque debiti di `in_sospeso.md` in una passata sola, senza spendere una chiamata all'AI — pannello del logo e fascia dei documenti a DPI alti, finestra Impostazioni e finestra principale su schermo piccolo, la pulizia del browser premuta dal vivo — più la schermata di avvio, che veniva dal primo tempo e non era mai stata annotata come debito.
- **Dodici reperti in tutto**, registrati fuori dal repo mentre il giro andava avanti. Nessuna riga di prodotto toccata: in questo tempo si guarda e si annota, si cura dopo.

**Cosa ho imparato**
- **I difetti del giro A e C sono difetti di silenzio.** Tre volte su nove l'applicazione fa una cosa **difendibile** senza dirla: butta la via del domicilio perché il prompt chiede una città sola, cattura una pagina-lista intera invece del solo annuncio, perde una modifica fatta a mano quando rigenera. Ogni singola scelta si può argomentare; il difetto non è la scelta, è che non la dichiara.
- **I difetti del giro B sono tutti la stessa specie di errore.** Costanti in pixel di progetto usate dove il DPI le ha già moltiplicate: una soglia confrontata con pixel fisici, un ingombro dichiarato di 261×216 mentre il pannello vero ne misura 373×360, una finestra dimensionata sul contenuto senza guardare lo schermo, un minimo scalato ×1,42 invece che ×1,5. Quattro esemplari di un unico sbaglio.
- **Nessuno dei 995 collaudi verdi poteva vederli, e non per pigrizia**: il banco gira a 96 DPI, e a 96 DPI quei numeri sono giusti. Non è un buco nella copertura, è un buco nell'**ambiente** in cui la copertura viene misurata.
- **Una cosa che l'app fa bene vale di più quando il profilo dietro è magro.** Il giro C è finito per sbaglio sul profilo di prova, povero di fatti: proprio la condizione in cui un modello è più tentato di riempire i vuoti. Non ha riempito niente.

**Dove ho faticato / cosa non era ovvio**
- **Ho condotto male il giro C.** Avevo lasciato aperte due finestre dell'applicazione — una sui dati veri, una sulla cartella di prova — e si distinguono **solo dalla barra del titolo**: il giro intero è andato sul profilo sbagliato. I reperti restano validi perché sono strutturali, ma il collaudo generale sul profilo vero resta da rifare, e adesso di finestre ne tengo aperta una.
- **La misura che dice quanto vale la scala dello schermo può mentire.** La prima lettura del DPI di sistema diceva 96 anche col 150% attivo, e sembrava che la disconnessione da Windows non fosse servita a niente: invece è il sistema che risponde 96 a chi non dichiara di capire il DPI. Chiamata la funzione giusta prima di misurare, la risposta è diventata 144. Ci sarei cascato, se non avessi avuto due controprove indipendenti che dicevano il contrario.
- **Anche lo strumento con cui guardo l'applicazione va guardato**: le sue fotografie a 150% escono ridotte, e su un'immagine ridotta un testo un po' sgranato somiglia moltissimo a un testo mal disegnato. Per il giro B ho dovuto costruirmi attrezzi che misurano in pixel veri, se no avrei annotato difetti inesistenti e mancato quelli veri.

**Cosa ho deciso e perché**
- **T9e passa da cinque tempi a sei.** La domanda di approfondimento sui campi mancanti entra nella 1.0 in forma minima — una domanda per voce, solo sui campi che pesano nel CV, occasione unica — e le cure dei reperti diventano un tempo a sé. Il rilascio slitta al sesto.
- **I difetti si curano dopo il giro, non durante.** Fermarsi a curare il primo reperto avrebbe chiuso il collaudo sul più bello: un giro che continua rende ancora, e infatti il giro B è arrivato dopo.
- **La pulizia del browser si proverà dopo la cura della finestra Impostazioni**, alla stessa scala. Il bottone oggi è irraggiungibile proprio per il difetto da curare: premerlo dopo la cura prova due cose in un colpo, il debito vecchio e la cura nuova.
- **La scala resta a 150%.** È la condizione in cui i tre difetti si vedono: tornare al 100% ora vorrebbe dire curare alla cieca e riverificare senza il caso peggiore sotto gli occhi.

💡 *Mia intuizione / scelta ragionata* — La regola 15 dice che una tappa col collaudo rimandato è chiusa con riserva, e l'avevo scritta dopo T8, quando un collaudo di tappa trovò due difetti che 817 collaudi verdi non vedevano. Qui è successo di nuovo, ma con una differenza che vale la pena fissare: a T8 il collaudo di tappa guardava **cose** che il banco non guardava; qui ha guardato le stesse identiche cose, **da un'altra parte** — uno schermo al 150% invece che al 100%. Il banco non aveva una lacuna da riempire: aveva un ambiente solo. Da qui viene la domanda che mi porto al quinto tempo, e che conta più delle tre cure: non «come sistemo questi difetti», ma **come faccio in modo che il banco possa vederli** — perché finché il banco gira in un ambiente solo, curare questi tre significa aspettare il prossimo giro dal vivo per scoprire il quarto.

### Step 2.44 — T9e: dodici reperti curati, e la promessa che un prompt faceva da mesi

*Il quinto tempo doveva essere manutenzione: dodici cose da sistemare, una lista da spuntare. Invece è il tempo in cui ho capito che i dodici reperti erano quasi tutti lo stesso reperto — l'applicazione che fa una cosa difendibile senza dirla — e che l'ultimo pezzo mancante non era un difetto ma una promessa: un prompt scritto mesi fa diceva che a completare una voce incompleta sarebbe stato l'utente, e il posto dove farlo non l'avevo mai costruito.*

**Cosa ho fatto**
- **Le tre cure di scala, e la decisione a monte.** Prima di toccarle ho deciso *come*: uno per uno, non un modo unico per tutta l'interfaccia. Nasce `ScalaSchermo`, funzioni pure a cui il DPI si **passa** — così il banco, che gira a 96, può chiedere cosa succede a 144. Verificate dal vivo a 150%: logo in compatta, finestra ferma al minimo vero, Impostazioni dentro l'area di lavoro e scorrevoli. Nella stessa passata ho premuto per davvero **«Svuota i dati di navigazione»**: 183 MB, e il bottone si spegne da sé.
- **Sette reperti insieme** (R1, R2, R3, R4, R5, R8, R9), col Pool sigillato a **1.13**. Il più grave era R2: correggere il solo domicilio cancellava email e telefono già confermati, in silenzio. La semantica resta la sostituzione — è quella che l'utente può prevedere — ma adesso il riepilogo **dice** quali campi spariranno. Guardando il codice ne è saltato fuori un secondo, più insidioso: una patente non ridetta valeva «no», e la patente è spesso il requisito eliminatorio di un annuncio.
- **R7, il più grosso.** La memoria di una riscrittura a mano viveva in un booleano di sessione, e da lì venivano due silenzi: la lettera che raccontava la storia di prima, e «Rigenera» che si riprendeva la modifica. Ora vive dove vive il documento. Ne discendono l'avviso che **nomina** i testi a rischio, la spia «⚠ Rigenera la lettera», il riallineamento automatico, e un prompt della lettera che distingue una prosa scritta dal modello da una scritta da me.
- **Due difetti trovati a mano nel pannello del profilo**, che nei dodici non c'erano: in sei caselle su diciotto scrivere «abc» lasciava a video «cba», e senza una voce scelta i campi erano scrivibili ma senza destinazione.
- **R6**: da «Modifica i testi» si sceglie anche **quali voci** il documento porta. Due elenchi, «Nel documento» e «Lasciate fuori»; il profilo non si tocca.
- **La domanda di approfondimento**, in linea subito dopo la conferma: «Del lavoro «magazziniere» non mi hai detto quanto è durato né cosa facevi. Me li dici?». Chiude il tempo e chiude la voce 4 della checklist, di cui il terzo tempo aveva fatto l'altra metà.
- **1110 collaudi verdi** (erano 995), copioni 10/10, un collaudo reale nuovo, versione **0.3.046**, Pool **1.13**.

**Cosa ho imparato**
- **Un censimento può dare la risposta giusta alla domanda sbagliata.** Oltre ottanta punti della stessa forma dicevano «fai la via larga». Guardati uno per uno, i tre difetti non erano la stessa specie: un confronto fra unità diverse, una costante che duplicava una misura già in mano al runtime, un tetto e uno scorrimento mancanti — che si sarebbe rotto uguale a 96 DPI su uno schermo basso. Un convertitore unico ne avrebbe curato **uno**, e sul quarto avrebbe moltiplicato due volte.
- **La falsificazione dice cose che i collaudi verdi non dicono.** Su R6, tolta la guardia contro un'impronta estranea al documento, **il banco è rimasto tutto verde**: un altro collaudo la copriva per caso, da un'altra strada. Il collaudo che serviva non c'era, e senza rompere apposta non l'avrei mai saputo. Sui prompt del Pool 1.13 la stessa prova ha detto che **R4 è una cintura e non una cura misurata**: rimettendo il prompt vecchio resta verde comunque.
- **Un difetto grande può essere una riga sola.** `elenco.Items(i) = etichetta`, unica occorrenza in tutto il progetto: in WinForms non riscrive la riga, la toglie e la rimette, e nel farlo alza un evento di selezione che ricarica i campi sotto la mano che scrive. Da lì il cursore torna a zero e le lettere escono al contrario. Ho fatto una passata su tutti e diciotto i file di `Ui/` e una prova di digitazione su ogni casella di ogni schermata: quella forma non c'è altrove.
- **Se l'assunto riguarda il modello, il banco non lo può provare.** La domanda di approfondimento dipende dal fatto che il modello, davanti a una risposta nuda, produca il frammento giusto: è una proprietà del modello, non del codice. Perciò accanto agli otto collaudi offline c'è un collaudo **reale** — e ha risposto: «tre anni circa» è finito in `durata`, col resto della voce fermo.

**Dove ho faticato / cosa non era ovvio**
- **Un rosso finto nella verifica di chiusura.** Ripristinando l'ultima falsificazione ho usato `mv` invece di `cp`: l'orario del file è tornato indietro, MSBuild ha visto un sorgente più vecchio della DLL e ha **saltato la compilazione**. Il banco è andato rosso su un collaudo che avevo appena visto verde. Ne sono uscito **strumentando il confronto** invece di continuare a ipotizzare — e la lezione è che quando la realtà smentisce due volte di fila, la cosa da mettere in dubbio è lo strumento, non il codice.
- **In VB un parametro copre la funzione che si chiama come lui.** La funzione che riceve la risposta prende `risposta` e non `testo`, perché `testo` coprirebbe `Testo(...)` e la chiamata verrebbe letta come indicizzazione di una stringa: l'errore parla di `Chars(index)` e non c'è verso di collegarlo alla causa.
- **Per compilare ho dovuto chiudere il server MCP del prodotto**, che è lo stesso eseguibile: da lì in poi, in quella sessione, i tool `mcp__trovalavoro__*` non c'erano più — si ricaricano solo al riavvio del client.
- **Lo strumento di collaudo non sa guidare il secondo elenco** della finestra a due liste: si presentano tutti e due come *Table* e prende sempre il primo. R6 l'ho provato a mano, e la cosa è annotata dove serve.

**Cosa ho deciso e perché**
- **Il documento non si taglia mai.** Una voce tolta resta nel `cv.json` e la scelta vive accanto. È questo che rende gratis il rimettere e che impedisce a un «Rigenera» di portarsi via il lavoro — e vale la pena anche se costa un blocco in più su disco.
- **Si riconosce per fatti, non per posizione né per prosa.** Il documento nuovo lo scrive il modello: l'indice 2 domani può essere un'altra esperienza, e la prosa cambia a ogni giro. L'impronta guarda i fatti, che vengono dal profilo.
- **Un filtro solo per tutti.** Anteprima, DOCX, PDF, HTML, prompt della lettera e tool MCP: se ognuno decidesse da sé cosa mostrare, la finestra e il server racconterebbero due documenti diversi, e chi legge non saprebbe quale dei due è vero.
- **Occasione unica.** La domanda esce dall'elenco quando viene **offerta**, non quando riesce. Se una risposta a vuoto guadagnasse un secondo tentativo, la stessa voce si farebbe richiedere all'infinito.
- **Correggere non è completare — e lo si dichiara invece di curarlo.** Se nella risposta ridico diverso un campo che non mi era stato chiesto, vale quello già confermato: una correzione entrata senza scheda cambierebbe il profilo alle spalle di chi l'ha confermato. Ma il dialogo **lo dice**, e manda al profilo.
- **Il riordino delle voci resta fuori.** R6 nominava due gesti; è entrato «togliere». Riordinare vuole un ordine da mantenere e una decisione su cosa farne dopo una rigenerazione: sta in `in_sospeso.md`, non nella 1.0.

💡 *Mia intuizione / scelta ragionata* — Contandoli a fine tempo, i dodici reperti sono in realtà **due**. Uno è ambientale, ed è quello del giro B: il banco vive a 96 DPI e lì quei numeri sono giusti. L'altro è di forma, ed è tutto il resto: l'applicazione fa la cosa difendibile e non la dice. La via che scompare, il campo che si azzera, la lettera che resta indietro, il testo buttato in una casella senza destinazione — ognuna di quelle scelte, presa da sola, la rifarei. Il difetto non è mai la scelta: è il silenzio intorno. E la cosa che mi ha colpito di più è che l'ultimo pezzo del tempo — la domanda di approfondimento — appartiene alla stessa famiglia letta dall'altro verso. Il prompt del turno formale prometteva da mesi che «sarà l'utente, con la voce davanti, a completarla o lasciarla»: la promessa era scritta, il posto per mantenerla non l'avevo mai fatto. Un prompt dice quel che il modello **deve** fare; non dice se qualcuno, dall'altra parte, ha costruito dove farlo. Anche quella era una promessa taciuta, solo che a tacerla ero io.

### Step 2.45 — T9e: la 1.0 esce, e la procedura di rilascio si scrive facendola

*Mi aspettavo il tempo più corto di tutti: cambiare un numero, lanciare uno script, mettere un tag. È stato corto davvero, ma non per le ragioni che pensavo — e la prima cosa che ho trovato non era nel codice, era che il rilascio stava per partire da una storia che il remoto non aveva ancora.*

**Cosa ho fatto**
- **Il push arretrato, prima di tutto.** `main` in locale era due commit avanti a `origin/main`: il merge del rito era stato fatto e il push no. Un tag messo lì sopra avrebbe puntato a qualcosa che nessun altro vedeva. Prima riga del tempo, `git push origin main` — col rimando esplicito, perché stavo sul ramo della tappa e il comando nudo punta altrove.
- **Il numero: `0.3.046` → `1.0.000`**, cambiato in un posto solo. Il `.vbproj` la rilegge da `Versione.vb` con una regex, quindi le proprietà dell'eseguibile sono seguite da sé.
- **La procedura di rilascio, scritta facendola.** Il cap. 13 aveva i parametri di `publish.bat` e l'aspetto del risultato, ma non l'**ordine**: è il §13.9, e l'ho scritto mentre pubblicavo, non prima. Ci sono finiti due passi che sbagliati non fanno rumore — il **banco intero prima di pubblicare** e la **cartella svuotata prima del publish**.
- **1110 collaudi verdi** col numero nuovo. Per farli girare ho dovuto chiudere il server MCP del prodotto: è lo stesso eseguibile, e tiene bloccato il `bin`.
- **L'eseguibile: 118.707.086 byte in un file solo.** Contro i 118.633.358 della 0.3.041 del 22 agosto: **73.728 byte** di differenza, cioè le cure del quinto tempo e nient'altro.
- **Le quattro verifiche.** Un solo file (contato, non guardato); le proprietà rilette **dall'exe** e non dal `.vbproj`; l'avvio vero, con `--dati` su una cartella usa-e-getta, fino a leggere `Ver. 1.0.000 · Pool 1.13 (integrato)` nel pannello logo; la dimensione confrontata col rilascio prima.
- **Il rito «aggiorna-tutto»** l'avevo chiesto e interrotto due giorni fa: è stato ripreso e finito per intero prima di questo tempo (commit `2ca0de1`, undici file), e aveva trovato quattro cose che nessuno aveva scritto — fra cui proprio che l'exe pronto per l'altra macchina era anteriore alle cure.
- Chiude il tempo il tag **`v1.0`**, che va sul commit di `main` dopo il merge: la storia delle release sta nei tag, non in un file.

**Cosa ho imparato**
- **Le cose che contano in un rilascio non sono nel banco.** 1110 collaudi verdi non dicono se accanto all'exe è rimasto un `.pdb`, se le proprietà che Windows mostra sono quelle giuste, se l'eseguibile **parte**. Sono quattro domande che nessun collaudo automatico sa porre, e per questo la loro lista adesso sta scritta.
- **«Un solo file» si prova solo partendo dal vuoto.** Se pubblico sopra alla cartella di ieri e poi la guardo, non sto verificando: sto ispezionando una cartella già sporca, e un file di contorno rimasto lì passerebbe per assenza di problema.
- **La console può mentire sul risultato.** Le proprietà lette da PowerShell davano `c 2026 Aviolab AI` dove il progetto scrive `© 2026 Aviolab AI`: era la codifica del terminale, non l'eseguibile. Me ne sono accorto perché ho letto lo stesso valore per due strade — e questa è la stessa disciplina del doppio controllo, applicata a una cosa che sembrava già verificata.
- **Una decisione di T1 si incassa il giorno del rilascio.** Tenere la versione in una costante sola sembrava pedanteria quando l'ho decisa; oggi ho cambiato tre cifre e le proprietà dell'exe, la riga del pannello logo e la finestra «Informazioni su…» si sono aggiornate insieme, senza che dovessi ricordarmi di nessuna delle tre.

**Dove ho faticato / cosa non era ovvio**
- **Lanciare il `.bat` da WSL.** `cmd.exe /c 'cd /d "…" && publish.bat'` risponde «La sintassi del nome del file… non è corretta»: le virgolette si perdono per strada. Invocandolo col percorso completo funziona subito, perché lo script si sposta da sé nella propria cartella.
- **Per pubblicare, come per collaudare, il server MCP va chiuso.** L'avevo già pagata durante le cure; qui l'ho messa in conto prima, e nel §13.9 c'è scritto — insieme al prezzo, cioè che i tool spariscono fino al riavvio del client.

**Cosa ho deciso e perché**
- **La procedura si scrive facendola, non prima.** Una procedura immaginata elenca i passi che vengono in mente; una annotata mentre si eseguono contiene anche quelli che non verrebbero in mente — ed è esattamente dove stanno gli errori che non fanno rumore.
- **L'eseguibile non entra nel repository.** Pesa oltre cento megabyte perché ingloba il runtime: nel repo sta lo script che lo produce, e nel README c'è scritto dove.
- **Niente pubblicazione online, per ora.** L'exe non è firmato e SmartScreen avviserebbe: per due postazioni di casa si porta a mano. Il giorno che uscisse di lì, la firma smette di essere un'opzione.
- **T9e si chiude con due riserve, e tutt'e due si scrivono.** Il **giro D** — l'exe su una macchina senza runtime .NET 10, e con lui i due debiti di Word — vuole una macchina che qui non c'è. La seconda l'ho trovata rileggendo cosa T9 aveva **promesso** invece di cosa aveva fatto: la **demo video per il portfolio**, in un elenco del 5 agosto accanto al diario e al tag, mai fatta e mai annotata da nessuna parte. Chiamarla «chiusa» sarebbe comodo e falso: stanno in `in_sospeso.md`, ed è la regola 15 che me l'ha insegnato due giorni fa.

💡 *Mia intuizione / scelta ragionata* — Il momento del rilascio non è quello in cui il programma diventa buono: è quello in cui smette di essere solo mio. Fino a ieri ogni difetto era un appunto fra me e il codice; da adesso c'è un eseguibile che qualcun altro può copiare su un PC che non ho mai visto, e che non potrà chiedermi niente. È per questo che le quattro verifiche mi sembrano la parte seria del tempo, e non il tag: il tag dichiara una versione, quelle dicono che l'oggetto esiste davvero e parte. E c'è una simmetria che mi piace con quello che ho imparato nel tempo precedente — lì il difetto era il silenzio dell'applicazione verso l'utente, qui sarebbe il silenzio mio verso chi la riceve: una riserva non dichiarata è la stessa specie di bugia. Diciannove giorni fa, al cancello T0, avevo un progetto di quindici capitoli e nessuna riga di VB. Oggi c'è un file da copiare che si apre e funziona. Fra le due cose non c'è stato nessun salto: solo tappe piccole, ognuna chiusa con la sua prova.

### Step 2.46 — Cinque difetti prima del giro D, e la lista che non era una prova

*La 1.0 era uscita e restava solo da portarla sul PC del tutor. Prima però c'erano cinque cose che si vedono a occhio nudo, annotate mentre usavo l'applicazione: le ho prese una alla volta pensando fosse manutenzione. La prima non ha richiesto nemmeno una riga di codice — era già curata da due giorni, e a dirmi che fosse aperta era stata una lista scritta da me.*

**Cosa ho fatto**
- **Il difetto che non c'era.** Le bolle del brainstorming mostravano il markdown grezzo: era stato curato il 22 agosto da T9d (`3b559db`, `ProsaDellAssistente.SenzaMarkdown` rifatta a ogni pezzo sul grezzo accumulato della bolla, non sul frammento che arriva — un pezzo dello streaming può spezzare un `**` a metà). La voce in `in_sospeso.md` non era stata spostata in «Chiuse» insieme alle due sorelle, e da lì è arrivata alla nota di ripresa come difetto aperto — con una cura appena disegnata, identica a quella che già girava. Dieci minuti per confrontarla col codice, zero righe scritte.
- **La barra che si mangiava i controlli.** Nelle Impostazioni, quando la finestra scorre, la barra verticale si prende 17 pixel a 96 DPI e 26 a 150%, contro i 14 del margine di disegno: la fila dei controlli finiva sotto la barra e ne compariva una **orizzontale**, senza niente da mostrare. Ora la fila si dispone due volte, la seconda dentro `ScalaSchermo.LarghezzaSenzaLaBarra`, e la riserva si prende **solo** quando si scorre. `Disponi` è diventata `DisponiIn(altezzaDisponibile)`: l'altezza si riceve invece di leggerla dallo schermo.
- **La selezione che tornava in cima.** In «Modifica i testi» i due elenchi si rifanno a ogni «Togli» e ogni «Rimetti», e un rifacimento non ha memoria. `MostraICampi` ritrova adesso la riga scelta **per identità** — la stessa voce, che nel frattempo si è spostata di elenco — e quando non c'è più cade su chi ha preso il suo posto, o sull'ultima rimasta. Vale per tutti e due, perché chi rimette le voci dentro una per una lavora in quello di destra.
- **Il ✎ che conosceva solo oggi.** Il segno della matita rispondeva a «l'ho scritto io?» da una spia di sessione, mentre l'avviso di «Rigenera» rispondeva alla stessa domanda leggendo il disco: un testo scritto ieri perdeva il segno alla riapertura, e intanto l'avviso continuava a promettere — giustamente — che sarebbe andato perso. Ora la finestra riceve con ogni documento i campi già riscritti a mano (`DocumentoDaRiscrivere.Riscritte`, `RiscrittureAMano.Contiene`), e il segno vuol dire «questo l'hai scritto tu», in questo giro o in uno di prima.
- **La candidatura sopravvissuta al suo profilo.** «Rigenera la lettera» su una candidatura i cui documenti erano nati da un profilo poi eliminato e rifatto finiva in «l'AI ha risposto in una forma che non riesco a leggere». Alla generazione arrivano tre cose — il profilo di oggi, il CV e i giudizi di allora: se quel profilo non c'è più, le tre parlano di due persone diverse e il modello risponde con spiegazioni invece che col documento. `MotivoProfiloSparito` se ne accorge **prima** di chiamare l'AI, su tutte e quattro le strade che ci portano.
- **Venti collaudi nuovi, 1130 verdi** (erano 1110), ognuno visto rosso prima di dirlo buono. Versione **1.0.000** e Pool **1.13** invariati: nessun prompt è stato toccato, quindi niente bump.

**Cosa ho imparato**
- **Una lista aperta non è una prova.** `in_sospeso.md` serve proprio perché una cosa rimasta indietro non si perda, ma resta un'affermazione: dice che il 22 agosto quella voce era aperta, non che lo sia oggi. Il codice è l'unico posto dove sta la risposta vera, e confrontarci una voce costa dieci minuti — meno di quanto costi disegnare la cura di un difetto che non esiste più.
- **Un collaudo può essere verde senza provare niente.** Il primo che ho scritto sulla selezione chiedeva a `SelectedItems` chi fosse scelto: al banco quel `ListView` non è mai stato mostrato, l'handle non esiste, e la risposta è **sempre** la lista vuota. Passava a prescindere. `ListViewItem.Selected`, invece, risponde anche a finestra mai nata: la finestra ha smesso di usare la scorciatoia, e da lì il collaudo ha cominciato a dire qualcosa.
- **La misura giusta si riceve, non si legge.** Vale per il DPI da `ScalaSchermo` e vale per l'altezza disponibile: un banco non può cambiare schermo, e il caso in cui il codice si comporta diversamente è esattamente quello che nessun collaudo saprebbe mettere in scena se il codice se lo andasse a prendere da sé.
- **Una falsificazione non sempre diventa rossa: a volte si pianta.** Tolta la guardia sul cambio di lingua, la finestra di conferma si apre e resta lì, senza nessuno che risponda: il collaudo fallisce come **timeout**. È un rosso anche quello — ma se non me lo fossi aspettato l'avrei letto come un guasto del banco.

**Dove ho faticato / cosa non era ovvio**
- **Il segno giusto per riconoscere il profilo sparito.** La strada ovvia era «la versione è diversa da quella di allora», e sarebbe stata sbagliata: un profilo che **cresce** cambia versione a ogni salvataggio, e i suoi vecchi documenti restano spiegabili. Un avviso lì sarebbe uscito a ogni giro per un caso che funziona. Il segno vero è la **versione che manca dallo storico** — che non si pota mai — e un collaudo tiene adesso quella linea.
- **Un collaudo è sopravvissuto a tutte le falsificazioni.** Undici rotture nel primo commit, ognuna abbatteva esattamente i collaudi che la sorvegliano, e uno restava in piedi qualunque cosa rompessi: gliene ho dovuta scrivere una apposta per vederlo cadere.
- **Dove mettere la guardia, dentro ognuna delle quattro strade.** Su «Rigenera» viene prima che i vecchi documenti siano buttati, perché sono tutto quel che resta di quella candidatura; sul cambio lingua prima della domanda, che altrimenti verrebbe posta per niente con la lingua nuova già salvata.
- **L'elenco «Lasciate fuori» resta fuori portata dello strumento di collaudo**: `scegli_riga` prende sempre il primo dei due elenchi. La logica l'ho collaudata al banco, la prova dal vivo tocca alla mia mano.

**Cosa ho deciso e perché**
- **Il messaggio non offre di rifare il confronto**, perché non si può: una candidatura già confrontata non ha un «riconfronta». Indicare un gesto che non esiste sarebbe peggio del silenzio, quindi dice la cosa onesta — rifai la candidatura dal suo annuncio — e il gesto mancante va in `idee_future.md`, col motivo per cui va maneggiato con cura: rifare i giudizi cambierebbe le stelle di qualcosa che potrei aver già spedito.
- **Il ✎ legge il disco, il «Salva» no.** Sembrano la stessa domanda e sono due: «questo l'hai scritto tu» riguarda tutti i giri, «cosa rientra nel documento adesso» riguarda solo questo — altrimenti un documento che nessuno ha toccato risulterebbe modificato ogni volta che lo si apre.
- **La riserva della barra si prende solo quando si scorre.** Toglierla sempre stringerebbe il contenuto anche nelle finestre che stanno larghe, per difendersi da una barra che non c'è.
- **Tre riserve dichiarate invece che dimenticate** (regola 15): la selezione nel `ListView` vero — il banco prova la logica su una finestra mai mostrata —, la barra a 150% e il messaggio nuovo davanti a una candidatura orfana. Vogliono la mano, non un altro collaudo.

💡 *Mia intuizione / scelta ragionata* — Contandoli alla fine, i cinque difetti sono la stessa forma vista da cinque lati: **due fonti che rispondono diversamente alla stessa domanda, e nessuno che le confronti**. Il ✎ e l'avviso di «Rigenera» rispondevano entrambi a «l'ha scritto l'utente?», uno dalla sessione e uno dal disco. Il margine di disegno diceva 14 pixel e Windows ne prendeva 17. L'elenco rifatto e io non eravamo d'accordo su quale riga fosse scelta. Il profilo di oggi e i documenti di allora parlavano di due persone. E il primo — il più imbarazzante — era la mia lista contro il mio codice, con la lista che aveva torto da due giorni. La cura, ogni volta, non è stata scegliere la fonte giusta: è stata **farne una sola**. Quello che mi resta è che il progetto ha ormai abbastanza pezzi da potersi contraddire da solo, e che i posti dove succede non si vedono leggendo il codice di una parte: si vedono usandolo, oppure mettendo due parti a confronto e chiedendo loro la stessa cosa. È esattamente ciò che farà il giro D, con una differenza che conta — a fare la domanda sarà qualcuno che non sa già la risposta.

### Step 2.47 — I dati del giro D provati prima del viaggio, e il difetto che nessun controllo sapeva vedere

*I dati per la revisione col tutor erano pronti da ieri: un CV finto in PDF e tre annunci, con dentro due trappole messe apposta. Restava una cosa sola prima di metterli in valigia — provarli qui, con l'AI vera. Sono passati, tutti e due i tranelli hanno retto, e mi sarei fermato lì: se non avessi chiesto quale dei due, in caso di guaio, me lo avrebbe detto.*

**Cosa ho fatto**
- **La prova dei dati, con l'API vera.** `CollaudiImportReale` puntato su `casi/giro-d/` (la variabile `CV_DI_PROVA`, che prende il primo PDF della cartella), col prototipo acceso: **verde in 20 secondi**, quattro chiamate — due trascrizioni del PDF e due strutturazioni.
- **Le due insidie hanno retto.** La città esce **Forlì**, cioè il **domicilio**, e non la residenza di Cesena; i **traslochi**, stampati nel CV sotto «Altre esperienze» con un'intestazione che somiglia a quella dei lavori veri, finiscono fra le **esperienze informali**. Le trascrizioni dell'app e del prototipo hanno in comune **33 righe su 33**.
- **Il criterio, confrontato a macchina e non a occhio.** Contro `casi/profilo.json` — lo stesso Luca Ferrari, ed è il motivo per cui il personaggio non è nuovo — coincidono nome, recapiti, patente, conteggi, ruoli, aziende, titoli e istituti. Gli unici due scarti sono di **prosa**: le durate tengono le parole del CV («marzo 2021 - oggi (3 anni)» dove il criterio scrive «3 anni») e la prima competenza perde l'inciso sul patentino, che però resta in formazione.
- **Il rapporto cancellato invece che committato.** Il collaudo lo scrive accanto al CV, e la sua intestazione dice «sta qui e non nel repo perché contiene dati personali»: vera per il mio CV, falsa per questi. Si rigenera quando serve.
- **I sei file committati** (`4678619`), e da lì la domanda: **quale dei due tranelli è davvero sorvegliato?** La città sì — ha un `Assert` in `CollaudiImportReale`, giudicato contro il CV. I traslochi **no**: `ControlloCollocazione` cerca la parola «volontario», che un trasloco non dice mai, `ControlloDoppioni` vede solo la voce contata due volte, e il conteggio delle formali sarebbe rimasto dentro la tolleranza. Il tranello aveva retto per bravura del modello, non perché qualcuno guardasse.
- **`ControlloCriterio`, il terzo controllo che guarda dentro un profilo** (`d96c7ae`), accanto ai due che c'erano già. Confronta il profilo importato col profilo atteso sui **fatti**, e un'attività informale la appaia con **due parole distintive nella stessa voce formale** — distintive perché il contrasto lo dà il criterio stesso: le parole che appartengono già ai suoi lavori veri vengono scartate prima di cercare.
- **`CollaudiGiroD`**, che lo fa girare: un collaudo con l'API e **nove senza rete**. Vuole **solo la chiave** — il CV sta nel repo perché è inventato, e qui il metro non è il prototipo ma il criterio. **1139 collaudi verdi** (erano 1130).
- **Tre falsificazioni prima di dirlo buono**, ognuna col suo rosso mirato: l'appaiamento reso cieco, le due parole ridotte a una, la città non più confrontata. Ogni volta è caduto **solo** il collaudo che sorveglia quel pezzo.
- **Il rito «aggiorna-tutto»**, che ha trovato quattro cose. Il **cap. 14** diceva che il copione del giro D «non esiste», ed esisteva da ieri: la sezione nuova dice quali due delle tre cose in attesa si sono chiuse. Un **commento del banco** affermava che il prototipo «continua a dare la residenza» — oggi ha dato il domicilio due volte su due, e adesso il commento dice quel che si sa davvero: che quella regola lui non ce l'ha, non che faccia il contrario. La voce di `in_sospeso.md` era **doppia** — la chiave del tutor *e* i dati da costruire — e sarebbe rimasta aperta intera o chiusa intera: ora la metà fatta sta fra le «Chiuse» e resta solo la chiave. E la **ricetta del PDF con LibreOffice**, che mi ero annotato di scrivere in `strumenti/README.md`, lì non va: quel file racconta gli **attrezzi** del repo, e la ricetta vive già intera accanto ai dati che serve a rifare.

**Cosa ho imparato**
- **Un dato di collaudo si prova prima di portarlo, o misura sé stesso.** Se il CV finto non fosse stato digerito dall'estrazione, il giro D avrebbe misurato il **dato** invece del programma — e su una macchina che non è mia, in un tempo che non si ripete, non avrei avuto modo di distinguere le due cose.
- **Il verde non dice quali proprietà sta sorvegliando.** Le due insidie erano scritte una accanto all'altra nel `LEGGIMI`, sembravano due prove gemelle, e il collaudo diceva verde su tutte e due. Una però era un `Assert` e l'altra era la mia lettura: dal risultato la differenza non si vede: si vede solo andando a chiedere *chi* la guarda.
- **Appaiare per parola-segnale funziona finché la parola c'è.** Un volontariato nel CV si dichiara — «VOLONTARIO» sta scritto nel ruolo —, un trasloco fatto per un amico nel fine settimana non dice niente di sé. Il segnale non è nel testo: è nella distanza fra quel che il criterio dice e quel che il profilo fa.
- **Il contrasto migliore per riconoscere una voce ce l'ho già in casa.** Non serviva un vocabolario di parole «informali» da mantenere a mano: le parole che distinguono un'attività informale sono quelle che i lavori veri **dello stesso criterio** non usano. Il criterio fa da metro e da contrasto insieme.

**Dove ho faticato / cosa non era ovvio**
- **Due nomi coperti in VB, nello stesso file.** `Dim criterio As Profilo = Criterio()` non compila — VB non distingue le maiuscole, e la locale copre la funzione: l'errore parla di indicizzazione, non di nomi. Poco dopo, un parametro chiamato come la funzione che lo contiene: `BC30530`. Sono due facce della stessa trappola, e la seconda l'ho presa dopo aver appena pagato la prima.
- **Rendere il controllo preciso senza farlo lampeggiare.** Una parola sola in comune capita per caso: «consegne» e «consegna» si somigliano, e una voce vera che nomina i mobili non è un trasloco. Da lì le due parole nella stessa voce, e un collaudo apposta che prova proprio il **limite dichiarato** — una parola sola non basta.
- **Il prototipo ha dato Forlì anche lui**, benché sia fermo al Pool 1.00 e il commento del collaudo si aspetti da lui la residenza. Non cambia l'esito, ma vuol dire che quel comportamento non è stabile come il commento lascia credere.
- **Leggere quale collaudo cade, non solo quanti.** Il primo giro di falsificazioni mi ha dato tre rossi e nessun nome: il filtro pescava i warning di compilazione. Un rosso senza nome non prova niente — poteva essere caduto un altro collaudo, per un'altra ragione.

**Cosa ho deciso e perché**
- **Un collaudo col criterio, invece di allargare il vocabolario di `ControlloCollocazione`.** Aggiungere «davo una mano», «un amico», «nei fine settimana» avrebbe coperto qualunque CV, ma è euristica su parole: «davo una mano ai colleghi» è un lavoro vero, e sarebbe diventato rosso. Il criterio è un file, e un file non ha falsi positivi.
- **I fatti, non la prosa.** Le durate e i `cosa_facevo` cambiano a ogni chiamata: due dei nove collaudi senza rete esistono apposta per **impedire** che il controllo li bocci, e valgono quanto quelli che cercano i difetti — un collaudo che lampeggia si smette di guardarlo.
- **Vuole solo la chiave.** Nessun prototipo e nessuna variabile d'ambiente: il CV è nel repo perché è finto, e questo è il collaudo che deve poter girare anche fra sei mesi, quando il prototipo sarà solo un ricordo.
- **Risolvere invece di annotare.** L'avevo proposto per `in_sospeso.md`, ed era la scelta comoda: quel file serve a non perdere le cose, non a rimandarle quando la cura è di due ore.

💡 *Mia intuizione / scelta ragionata* — Il difetto vero di oggi non stava nel codice: stava nel fatto che avevo **due prove che sembravano gemelle e non lo erano**. Il `LEGGIMI` dei dati le presenta di fila, con lo stesso peso, e per un giorno intero ho creduto che il collaudo le guardasse entrambe. È lo stesso schema che avevo appena raccontato nello Step prima — due fonti che rispondono alla stessa domanda e nessuno che le confronti — solo che stavolta le due fonti erano il collaudo e la mia idea di cosa il collaudo facesse. La domanda che l'ha rotto è piccola e la voglio tenere: *quale riga di codice diventerebbe rossa se questa cosa smettesse di funzionare?* Se la risposta è «nessuna», il verde che ho davanti non è una prova, è una coincidenza che dura da poco. E vale anche al contrario, perché il giro D è vicino: le cose che nessun collaudo sa vedere — un documento aperto in Word, un tutor che esita davanti a un bottone — non diventano più sicure perché il banco è verde. Diventano solo più silenziose.

### Step 2.48 — Il giro D si fa davvero, e l'eseguibile non era quello

*Il giro D aspettava una macchina da quattro giorni: un PC senza SDK .NET 10 e con Word, che qui non c'è. Il 25 agosto quella macchina c'è stata, e l'applicazione ha girato davanti a qualcuno che non l'aveva mai vista. Cinque voci del copione su sette sono passate, le due insidie dei dati finti hanno retto anche là, e la revisione ha lasciato un elenco di lavoro. Ma la cosa che conta di più della giornata non è nessuna delle sette: è che il file che ho portato non era quello che credevo.*

**Cosa ho fatto**
- **Il copione del §13.10 percorso voce per voce**, sulla postazione del tutor, con la chiave digitata a mano e tolta andando via.
- **`D2` — i documenti aperti in Word**: **25 campi su 25** e **6 su 6** ritrovati, DOCX e PDF identici fra loro. È la voce che T4 aspettava dal 10 agosto, e LibreOffice non poteva darla.
- **`D3` — il DOCX salvato *da* Word e reimportato**: l'esito coincide col criterio. È la voce che T3 aspettava dal 7 agosto: fino a ieri i `.docx` di collaudo erano **fabbricati** dal testo, e provavano le strade di lettura, non l'impaginazione vera.
- **Le due insidie hanno retto anche là**, e i tre annunci hanno dato **4,4 · 2,1 · 1,0** stelle — alto, medio, eliminatorio: la scala si comporta come il copione diceva.
- **`D1` e `R-b` non si sono potute provare** su quella macchina, e restano dov'erano.
- **Il reperto della giornata.** L'eseguibile che ho portato è quello pubblicato il **24 agosto alle 17:52**; le tre cure prima del giro D sono delle **19:05 e 19:26**. La versione però non è cambiata: `1.0.000` stava scritto su tutti e due i file, e niente li distingueva. Ho collaudato una 1.0.000 che non conteneva le tre cose che il giro doveva provare.
- **Il conto rifatto a mano, tornato a casa.** Fra il commit di quell'eseguibile (`23f4df7`) e oggi il **prodotto** cambia in **sei file**, e sono tutti e soli quelli delle cure: `FinestraModificaTesti.vb` (la selezione, `R-a`), `ScalaSchermo.vb` e `FinestraImpostazioni.vb` (la barra a 150 %, `R-b`), `PannelloDocumenti.vb` (la candidatura orfana, `R-c`), `ArchivioProfilo.vb` e `RiscrittureAMano.vb` (il segno ✎). L'unica altra modifica al prodotto sono **tre commenti**. Quindi `D2` e `D3` **valgono** — quel codice non l'ha toccato nessuno — e le tre riserve **no**.
- **L'elenco della revisione portato dentro il repo**: quattordici voci in `in_sospeso.md`, una in `idee_future.md`. Fra le prime ce n'è una che è un debito conclamato — il **log diagnostico** che il cap. 11 disegna nella cartella dati (`log\app.log`, «senza segreti») e che nel prodotto **non esiste**: nessuna riga lo scrive, verificato oggi.
- **Tre voci chiuse**: il giro D come *macchina che qui non c'era*, il `.docx` di Word (T3) e i documenti aperti in Word (T4). Le tre che il giro doveva chiudere in un colpo erano queste più l'exe su un PC pulito, che invece **resta aperta**.
- **La firma del codice migrata** da `idee_future.md` a `in_sospeso.md`: stava lì dal 5 agosto con dentro una condizione — «quando l'app circolerà oltre il portfolio» — e quel momento la revisione lo dichiara arrivato.

**Cosa ho imparato**
- **Una versione è un'etichetta, non un'identità.** `1.0.000` è un numero che scrivo io in un file di progetto; l'impronta SHA-256 è un fatto che il file ha addosso. Per tutto il progetto ho legato ogni riga a un commit — e l'unica cosa che poi consegno a qualcuno, l'eseguibile, non è legata a niente.
- **Un collaudo su un binario sbagliato non è un collaudo sbagliato: è il collaudo di un'altra cosa.** Non ha detto il falso, ha detto la verità su una versione che non m'interessava più. È il modo peggiore di sbagliare, perché tutto sembra a posto.
- **La regola 16 ha un punto cieco, e non è colpa sua.** Rilegge quel che ogni **tappa** aveva promesso: il log del cap. 11 non l'ha promesso una tappa, l'ha promesso un capitolo. Un impegno preso fuori dalle tappe non passa da nessuno dei miei controlli — l'ha trovato uno che leggeva il progetto da fuori.
- **Quel che nessuno guarda, nessuno lo vede.** È la lezione dello Step prima, tornata: il legame fra eseguibile e commit non aveva un collaudo rosso possibile, perché non esisteva proprio la cosa da rompere.

**Dove ho faticato / cosa non era ovvio**
- **Decidere se `D2` e `D3` valessero lo stesso.** L'istinto era buttare via tutto il giro. La risposta non stava nel ragionamento ma nel `git diff` fra i due commit: sei file, tutti d'interfaccia e di riscritture, nessuno dei quali tocca la scrittura di un DOCX. Le due voci si chiudono, e nella riga che le chiude c'è scritto **perché**.
- **Un filtro che va in errore restituisce niente, e niente passa per pulito.** Controllando che dal documento della revisione non finissero dati personali nel repo, il controllo mi diceva verde: guardava **zero righe**, perché `grep` qui è **ugrep** e su `^\+\+\+` dà errore di sintassi invece di filtrare. Se non avessi provato a farlo diventare rosso apposta (regola 14), avrei creduto a un verde che non guardava niente. Adesso quel controllo, se non ha righe da esaminare, lo **dice**.
- **Dove va ogni voce.** Metà dell'elenco è roba che serve per dare il programma a qualcun altro — guida, informativa, firma, «Prova la chiave» — e non è ancora nel perimetro dichiarato; l'altra metà sono difetti veri. La riga di separazione fra `in_sospeso.md` e `idee_future.md` è netta sulla carta e scomoda in mano.

**Cosa ho deciso e perché**
- **Il tag `v1.0` aspetta ancora.** Era rimandato «dopo il giro D, se quella prova trovasse qualcosa»: qualcosa l'ha trovato — non nel prodotto, nel legame fra eseguibile e commit. Quando arriverà andrà sul commit **effettivamente pubblicato**, con l'impronta dell'eseguibile annotata accanto.
- **La cura di quel difetto sta in tre cose piccole**: lo SHA del commit nella finestra «Informazioni», `publish.bat` che stampa e verifica versione, dimensione e impronta, e quell'impronta accanto al tag. Nessuna delle tre è codice difficile; tutte e tre servono a rendere **verificabile** una cosa che oggi è una promessa.
- **Le opzionali restano opzionali.** Spezzare i pannelli monolitici — `PannelloDocumenti.vb` è di 2188 righe — e rompere il ciclo `Dati` ↔ `Motore` sono manutenibilità, cioè il costo della *prossima* modifica. Restano in `idee_future.md` per la stessa ragione delle costanti di `StileApp`: il banco quasi non ha collaudi d'interfaccia, e rimettere mano a quindici file di UI oggi costerebbe più di quanto protegga.

💡 *Mia intuizione / scelta ragionata* — La cosa che mi porto dietro di oggi non è nessuno dei quattordici lavori da fare: è che ho passato due giorni a **preparare** il giro D — il copione scritto prima, i dati finti costruiti e provati con l'AI vera, le due insidie messe apposta — e non ho speso trenta secondi a chiedermi *quale file* stavo mettendo in valigia. Ho verificato tutto quel che il giro doveva **misurare** e niente di quel che il giro doveva **usare**. È lo stesso schema di sempre, solo spostato di un passo indietro: si controlla con cura l'oggetto dell'esame e si dà per scontato lo strumento con cui lo si fa — il collaudo che non guardava le insidie, il filtro che non guardava le righe, l'eseguibile che non era quello. La domanda che vale la pena aggiungere alle altre è breve: *questo che ho in mano è davvero la cosa che credo?* Un numero di versione non risponde. Un'impronta sì.

### Step 2.49 — Le ultime tre voci del giro D, e due falsificazioni che hanno trovato un difetto vero

*Delle quattordici voci che la revisione del giro D aveva lasciato ne restavano tre, e sono quelle che ho tenuto per ultime non perché fossero le più piccole ma perché sono le più difficili da giudicare da solo: parlano tutte a **chi il programma non l'ha scritto**. Un contatore di spesa, un'informativa, una guida. Scrivendole ho capito che il programma dava per scontate un mucchio di cose che io sapevo e nessun altro poteva sapere.*

**Cosa ho fatto**
- **I modelli si scelgono dalle Impostazioni** (voce «avvisi e conti», prima parte). Due tendine, una per livello — ragionamento e elaborazioni testuali —, con l'elenco che il programma **chiede all'API**: `/v1/models`, la stessa porta di «Prova la chiave», che non consuma token. La scelta finisce in `modelli.json`, che resta la casa, e vale **dalla chiamata dopo** senza riavviare.
- **Un modello ritirato adesso si riconosce**: un `404` o un `not_found_error` non sono più «l'AI ha rifiutato la richiesta» in mezzo agli altri, ma una causa sua che **nomina il modello** e dice dove si cambia.
- **Il contatore di spesa**: `chiamate_ai.csv` esisteva dal 18 agosto con dentro modello e token di ogni chiamata; bastava leggerlo. In *Impostazioni → Quanto è costato* ci sono chiamate, token e una stima in dollari, in totale e negli ultimi trenta giorni.
- **L'avviso di versione**, su richiesta: un bottone in «Informazioni» chiede a GitHub qual è l'ultima release e confronta i numeri. Mai all'avvio, mai da solo.
- **L'informativa dentro l'applicazione**: «Come funziona, e cosa esce dal tuo PC». Compare **una volta sola al primo avvio, prima della richiesta della chiave**.
- **La guida per chi usa il programma**: `GUIDA.md` in radice, linkata dal README.
- **Il rito «aggiorna-tutto»**, e la passata su `in_sospeso.md` voce per voce: delle quattordici ne restano due, e sono le due che non dipendono da me.

**Cosa ho imparato**
- **Il dato per rispondere a una domanda nuova ce l'avevo già.** Il contatore di spesa non ha richiesto di annotare niente: il file dei token nacque il 18 agosto per un'altra ragione — ritarare i `max_token` del pool — e rispondeva anche a «quanto mi costa», solo che nessuno gliel'aveva chiesto. Mi sa che è una regola generale: prima di aggiungere un'annotazione, guardare cosa si annota già.
- **Un elenco compilato dentro il programma invecchia; uno chiesto all'API no.** La tentazione era scrivere nel codice la lista dei modelli fra cui scegliere. Chiedendola invece all'AI, un modello nuovo compare da sé e uno ritirato sparisce — e mi è costato meno codice, non di più.
- **Che un prezzo abbia una scadenza me l'ha detto un capitolo, non la memoria.** Ho scritto nel listino i $2/$10 di Sonnet 5 come se fossero stabili; poi il cap. 15 mi ha ricordato che sono una **promozione fino al 31 agosto**. Adesso la scadenza è annotata nel codice e in `in_sospeso.md`, perché è una cosa che nessun collaudo può accorgersi.

**Dove ho faticato**
- **Due guardie che si coprivano a vicenda non sono due difese.** Le tendine dei modelli avevano una guardia sugli eventi *e* un controllo «se è già quello, non fare niente». Falsificandone una alla volta, il banco restava verde tutte e due le volte: sembrava ridondanza sana. Falsificandole **insieme** non è caduto un collaudo — è caduto il processo, con una **ricorsione senza fondo**: riempire una tendina faceva scattare l'evento che la riempiva. La cura non è stata rimettere le guardie ma **togliere l'anello**, separando chi riempie le tendine da chi aggiorna le righe sotto. Poi una guardia sola è bastata, ed è diventata falsificabile per davvero: cinque rossi.
- **Un assert può essere verde per merito di qualcun altro.** Il collaudo del modello ritirato chiedeva che il messaggio dicesse *quale* modello. Ho falsificato la funzione che lo nomina: verde. Il motivo è che quell'identificativo compariva anche nel **corpo d'errore che l'API rimanda indietro**, e il collaudo lo leggeva lì. Adesso il corpo finto non lo nomina, e la prova riguarda solo quel che scriviamo noi.
- **In VB un nome coperto colpisce tre volte in un giorno.** Una locale `tendina` che copre la funzione `Tendina`, un parametro `predefinito` che copre `Predefinito()`, un parametro `versione` che copre il modulo `Versione`. L'errore non dice mai «nome coperto»: dice che una classe «non può essere indicizzata», oppure che `Riga` non è membro di un `Func`.
- **Lo strumento di collaudo non ha potuto guardare.** Volevo vedere le tre cose nuove nell'applicazione vera: la `schermata` riprendeva la finestra in primo piano invece dell'app, e il `clic` diceva «Premuto» senza premere — la trappola già annotata a scala 150 %. Ho verificato quel che si poteva **interrogare** invece di guardare (l'elenco dei controlli dice che l'informativa c'è al primo avvio e non c'è al secondo), e ho **dichiarato la riserva** invece di dire che era fatto.

**Cosa ho deciso e perché**
- **L'informativa compare prima della chiave, non dopo.** Chi ha appena incollato una chiave a pagamento ha già scelto di fidarsi: dirgli allora che cosa esce dal suo PC è arrivare tardi. E compare **una volta sola**: se il file delle preferenze non si lascia scrivere, ricompare — il dubbio va a favore di chi deve essere informato, non della mia voglia di non ripetermi.
- **Un modello senza prezzo conta i token e non i soldi, dicendolo.** Il listino conosce tre modelli, quelli che il programma ha davvero usato. Per gli altri un totale che tace su una parte delle chiamate sembrerebbe completo, ed è il modo più educato di dire una cifra sbagliata. Stessa famiglia: sotto il centesimo non scrivo «$0,00», che si legge come «gratis».
- **Nella guida non ho scritto quanto costa un giro.** La tentazione era «pochi centesimi». Non è un numero che io conosca — dipende da quanto sono lunghi i testi di chi usa il programma — e una cifra inventata dentro un'informativa è precisamente la cosa che l'informativa esiste per non fare. Ho scritto da cosa dipende, e dove il programma tiene il conto vero. C'è un collaudo che lo verifica.
- **Il testo dell'informativa sta in una funzione, non in un file di disegno.** Così un collaudo può chiedergli se **nomina ogni porta** da cui qualcosa esce. Se un giorno il programma comincerà a mandare qualcosa da una porta nuova e qualcuno si scorderà di dirlo, sarà un rosso invece di una bugia.

> 💡 **La falsificazione che serve non è quella che conferma.** Diciassette falsificazioni in due sessioni, e quindici hanno fatto esattamente quel che dovevano: rosso dove serviva, verde altrove. Le due che sono servite davvero sono le altre — quella che non è diventata rossa affatto (e nascondeva una ricorsione infinita) e quella che è rimasta verde per merito dell'API. La regola 14 dice di provare a far fallire un collaudo prima di dirlo buono; quello che ho imparato oggi è che **l'esito interessante è quando la falsificazione non si comporta come previsto** — lì c'è sempre qualcosa che non avevo capito.

### Step 2.50 — Le tre cose guardate a occhio, e i quattro difetti che solo l'occhio poteva trovare

*La riserva dello Step precedente diceva una cosa sola: le tre cose nuove — le tendine dei modelli, «Quanto è costato», i due bottoni di «Informazioni» — erano provate al banco e verificate vive, ma **nessuno le aveva guardate**. Le ho guardate oggi. Ne sono usciti quattro difetti, e il primo non era estetico affatto: il contatore di spesa nasceva monco, e il banco non poteva accorgersene perché i dati di prova che gli avevo dato io erano più gentili della realtà.*

**Cosa ho fatto**
- **Ho ricompilato prima di guardare.** L'eseguibile di Release che avevo era di otto minuti prima del commit: probabilmente conteneva tutto, ma è esattamente il dubbio che ha fatto fallire il giro D (Step 2.48). Una build fresca in `C:\Temp` dal commit vero, avviata sulla cartella dati vera — e senza spegnere il server MCP, perché quel blocco riguarda la Release e non un output altrove.
- **A — l'alias e la versione datata erano due modelli diversi.** Il programma chiede `claude-haiku-4-5`; l'API elenca `claude-haiku-4-5-20251001`. Da lì: nelle Impostazioni Haiku 4.5 compariva **due volte** nella stessa tendina — una col nome, una con l'identificativo crudo — e nel conto della spesa **tre chiamate su diciassette non avevano prezzo**, perché nel `chiamate_ai.csv` finisce il modello che *ha risposto* mentre il listino conosce l'alias. La cura sta in una classe di tre righe, `IdModello`, che toglie il suffisso `-AAAAMMGG` e nient'altro; la usano il listino e la tendina.
- **B — in «Informazioni» la riga del copyright copriva «Cerca aggiornamenti».** Non solo lo rendeva illeggibile: i tre quarti sinistri del bottone **non si potevano premere**, perché il clic lo prendeva la scritta davanti. Ora le tre righe di testo e i comandi stanno su bande separate, e un collaudo verifica che **nessun controllo ne copra un altro** e che nessuno esca dalla finestra.
- **C — il 404 raccontato in italiano.** «Il servizio ha risposto 404» manda a cercare un guasto: GitHub risponde così quando di release pubblicate non ce n'è nessuna, che è esattamente il caso di chi ha in mano la prima versione. Adesso lo dice: «Non risulta pubblicata nessuna versione». Un guasto vero porta ancora il suo numero.
- **D — «Chiudi» delle Impostazioni non scorre più via.** La finestra si apre alta quanto lo schermo e il suo contenuto è più alto: il bottone stava 145 pixel sotto il bordo. Ora il contenuto scorre dentro un pannello e la fascia in fondo, con «Chiudi», resta ferma — con un filo sopra, perché il testo che le passa sotto non sembri tagliato da niente.
- **Otto falsificazioni, otto rossi giusti**, ciascuna che colpisce solo il suo collaudo; le righe sono in `falsificazioni.md`, che passa da 31 a 39. Banco: **1257 verdi** (erano 1243).

**Cosa ho imparato**
- **Un dato di prova più gentile della realtà rende il banco cieco.** La risposta finta dell'elenco modelli dichiarava `claude-haiku-4-5` — l'alias con cui *chiediamo*, non quello con cui l'API *risponde*. Con quel dato ogni collaudo era verde e ogni falsificazione dava il suo rosso, mentre nell'applicazione il difetto c'era. La falsificazione difende il collaudo dal codice; dai propri dati lo difende solo guardare la cosa vera.
- **Un difetto di impaginazione può essere un difetto di funzione.** Due controlli sovrapposti, al banco, sono due controlli presenti e accesi. A video uno copre l'altro; sotto il dito, uno **prende il clic** dell'altro. È la stessa cosa detta due volte, ma la seconda si può collaudare: da oggi c'è un collaudo che confronta i rettangoli a coppie.
- **A 96 DPI lo strumento di collaudo torna a vedere e a premere.** Le due trappole della riserva — la fotografia virtualizzata e il `clic` che mente — erano figlie del 150 %. Resta però un terzo modo di mentire, che il DPI non c'entra: se il bersaglio è **fuori dallo schermo**, o se un'altra finestra è passata davanti, il `clic` dice «Premuto» lo stesso.

**Dove ho faticato**
- **Tre clic andati a vuoto prima di capire perché.** Il primo l'ha preso Esplora file, che era passato davanti fra una fotografia e l'altra. Il secondo e il terzo sono finiti a `y = 1177`, cioè **sotto il bordo dello schermo**, perché premevo un «Chiudi» che stava fuori: lo strumento ci porta il puntatore e riferisce «Premuto». Il modo che regge è portare l'applicazione davanti con ALT + `SetForegroundWindow` e **verificare** con `GetForegroundWindow` prima di premere.
- **Il mio primo collaudo della geometria era troppo severo, e per un dettaglio.** `Rectangle.IsEmpty` è vero solo per il rettangolo tutto a zero: due controlli che si toccano sul bordo danno un'intersezione alta zero, che «vuota» non è. Accusava una sovrapposizione che a video non c'era. Si scrive `IntersectsWith`.
- **`ESC` chiude la finestra, tranne quando non la chiude.** Con una tendina aperta il primo colpo ha chiuso tutto, il secondo giro non ha chiuso niente. Ho smesso di indovinare e ho premuto il bottone vero, portando prima l'applicazione davanti.

**Cosa ho deciso e perché**
- **Ho insegnato al programma a riconoscere l'alias, invece di cambiare i predefiniti.** L'altra strada era scrivere `claude-haiku-4-5-20251001` in `Modelli.vb`: cura il sintomo e mi costringe a una build nuova a ogni versione di Haiku, oltre a contraddire il cap. 15 voce 6, che i predefiniti li dichiara per alias. La normalizzazione toglie **solo** la data in coda: `claude-opus-4-5` e `claude-opus-4-6` restano due modelli diversi, e c'è un collaudo che lo pretende.
- **La tendina mostra la voce dell'elenco, non l'alias.** Quando il modello in uso è l'alias di uno elencato, risulta scelto quello vero dell'API, col suo nome leggibile: nessuna riga sciolta in cima, nessun doppione. Se poi qualcuno tocca la tendina, in `modelli.json` finisce l'identificativo datato — ma perché l'ha scelto lui, non perché il programma l'ha deciso di nascosto.
- **Ho fatto scorrere il contenuto e non la finestra.** Era la cura più invasiva delle quattro — un pannello nuovo, due collaudi da adeguare — e l'alternativa (lasciar scorrere tutto e sperare che l'utente lo scopra) non è una cura: è una scommessa su chi guarda. La fascia dei comandi che non scorre è già il modo in cui funzionano i pannelli del programma da T4.

> 💡 **Quel che il banco non può vedere non è quel che non abbiamo collaudato: è quel che gli abbiamo raccontato noi.** I 66 collaudi delle tre cose nuove erano scritti bene, falsificati e verdi. Il difetto stava nella riga di JSON finto che avevo scritto io per fare da elenco dei modelli — un identificativo senza la data, perché era così che pensavo si chiamasse il modello. Il banco non poteva contraddirmi: gli avevo dato la mia idea di realtà, e lui l'ha verificata con scrupolo. A contraddirmi è bastato aprire le Impostazioni e leggere due righe.


### Step 2.51 — Lo strumento che diceva «Premuto» senza aver premuto

*Lo Step precedente si chiudeva con tre clic andati a vuoto e una riga di diagnosi: lo strumento con cui provo l'applicazione riferiva un successo che non c'era stato. Oggi gliel'ho tolto. Non è codice del prodotto — è l'attrezzo — ma è l'attrezzo con cui giudico il prodotto, e uno strumento che mente non è neutro: manda a cercare un difetto dove non c'è. La cura è di due domande, e la seconda mi ha fatto scoprire che quei clic non andavano semplicemente persi: finivano da qualche altra parte.*

**Cosa ho fatto**
- **Ho insegnato a `clic` due domande da farsi prima di premere.** La prima: sono davvero in primo piano? `SetForegroundWindow` da sola non basta — Windows la rifiuta a un processo che non è già davanti, e nessuno se ne accorgeva perché il suo esito non lo guardava nessuno. Adesso c'è il **colpo di ALT** che scioglie il rifiuto (dato solo quando davanti c'è qualcun altro: all'applicazione già davanti aprirebbe la barra dei menù), fino a tre tentativi, e poi la verifica con `GetForegroundWindow`. La seconda: **di chi è il pixel che sto per premere?** `WindowFromPoint` sul punto del colpo, confrontato per processo. Se una delle due risposte non torna, l'attrezzo **non preme** e dice perché.
- **L'ho data a tutti e quattro gli attrezzi che muovono il puntatore**, non al solo `clic`: anche `scrivi`, `scegli_voce` e `scegli_riga` colpivano al buio esattamente allo stesso modo.
- **Ho provato dal vivo sull'applicazione vera**, su una cartella dati usa-e-getta: clic normale (preme), clic con Blocco note davanti e verificato davanti (conquista il primo piano e preme), bottone spinto **540 pixel sotto il bordo** dello schermo (rifiuta, e dice a quali coordinate sarebbe finito il colpo).
- **Due falsificazioni.** Rimettendo il codice di prima, lo stesso bottone fuori schermo risponde «Premuto «Backup…»»: la bugia, riprodotta. Rompendo apposta la funzione del primo piano, il clic si rifiuta e nomina chi c'è davanti: la guardia scatta davvero.
- **Curato anche `schermata`**, che aveva lo stesso difetto in versione muta — portava la finestra davanti senza verificarlo, e la trappola era scritta nel README con il rimedio «chiamala due volte». Ora ci prova lei tre volte e, se non ci riesce, **lo dichiara nella risposta**: una fotografia della finestra sbagliata sembra un difetto dell'applicazione.
- **E due difetti minori trovati per strada**: `scrivi` su una casella di sola lettura usciva con l'eccezione COM e tutto lo stack (ora lo dice come i bottoni dicono «è SPENTO»), e `schermata.ps1` non impostava l'`OutputEncoding` — il suo primo messaggio con un accento è uscito «l� c'� rimasta».

**Cosa ho imparato**
- **Un clic che manca il bersaglio non si perde: va a finire da qualche parte.** `SetCursorPos` non rifiuta le coordinate fuori schermo, le **clampa** al bordo. Il mio clic a `y = 1620` è finito a `y = 1079`, cioè sulla **barra delle applicazioni**, e alla prova ha minimizzato l'applicazione. Per un mese ho creduto che quei «Premuto» fossero colpi andati a vuoto; erano clic a caso su Windows.
- **Una domanda sola può coprire due difetti diversi.** «Il controllo è fuori dallo schermo?» e «c'è un'altra finestra davanti?» sembrano due controlli da scrivere; sono la stessa domanda — *di chi è questo pixel?* — e `WindowFromPoint` la risponde in una riga. Con un vantaggio che non avevo previsto: restano leciti i clic sui popup che l'applicazione apre **fuori** dalla propria finestra, come la tendina di un menù, che qualunque controllo sul perimetro avrebbe rifiutato a torto.
- **Il README dello strumento diceva una cosa non vera, e me ne sono accorto usandolo.** «Dopo aver toccato `server.mjs` o gli script il server va riacceso»: gli script `.ps1` no — il server li lancia da capo a ogni chiamata, e le mie modifiche erano vive al comando successivo. Il riavvio serve solo per `server.mjs`.

**Dove ho faticato**
- **Non sono riuscito a far fallire il primo piano per davvero.** Volevo vedere Windows rifiutare la richiesta, ma in quella sessione il sistema non concedeva il primo piano a **nessuno**: né a Blocco note né a Esplora file, provati tutti e due. Ho falsificato allora la funzione, rompendola perché rispondesse sempre di no — la guardia scatta, il messaggio è quello giusto — e la riserva l'ho scritta invece di far finta di niente: quel ramo l'ho visto rosso per rottura, non per rifiuto vero.
- **La mia prima falsificazione si è mangiata una parentesi.** Avevo infilato il commento «FALSIFICAZIONE» a fine riga, dentro un `if`: in PowerShell il commento arriva fino a fine riga e si è portato via la graffa che apriva il blocco. Errore di sintassi — che per fortuna il controllo del parser ha visto subito, prima che diventasse un rosso da interpretare.

**Cosa ho deciso e perché**
- **Il confronto si fa sul processo, non sulla finestra.** Pretendere che davanti ci sia *quella* finestra escluderebbe le finestre di messaggio dell'applicazione, che sono lei quanto la principale; e il perimetro della finestra escluderebbe le tendine, che sono finestre a sé. Il processo è la domanda giusta: «è roba nostra?».
- **Rifiutare, non arrangiarsi.** L'attrezzo avrebbe potuto provare a portare in vista il controllo da solo — scorrere, ridimensionare. Non lo fa: si ferma e me lo dice. Un attrezzo di collaudo che si aggiusta le condizioni da sé finirebbe per provare l'applicazione in una situazione che non è quella che volevo io.
- **Il DPI resta fuori.** La sorella cattiva di questa trappola — a 150 % il puntatore va altrove — si curerebbe con tre righe (`SetProcessDPIAware`), ma provarla vuole un cambio di scala e una disconnessione, e non tocco quel che funziona senza poterlo collaudare. L'ho scritto nel README, con una nota che dice chiaro che le due guardie nuove **non** coprono quel caso.

> 💡 **Uno strumento che mente costa più di uno strumento che manca.** Se `clic` mi avesse detto «non ci arrivo», avrei ridimensionato la finestra e sarei andato avanti in trenta secondi. Dicendomi «Premuto», mi ha mandato a cercare per un'ora un difetto dentro un'applicazione che era sana — e la volta prima, allo Step 2.50, mi era già costato tre clic e una diagnosi sbagliata sfiorata. Il difetto dell'attrezzo non resta nell'attrezzo: diventa un difetto immaginario del prodotto, e quello lo paghi due volte, prima cercandolo e poi non trovandolo.


### Step 2.52 — Le due liste affiancate, e i due difetti che si vedevano solo curando il primo

*Dal 24 agosto lo strumento di collaudo aveva un debito scritto: delle due liste di «Modifica i testi» ne guidava una sola, e la colonna «Lasciate fuori» l'avevo provata a mano. Oggi gliel'ho insegnata. La cura vera è di poche righe — dire *quale* lista —, ma provarla dal vivo ha stanato altri due difetti che nessuno aveva mai visto, e uno dei due rendeva ciechi tutti gli attrezzi insieme facendo sembrare chiusa un'applicazione aperta.*

**Cosa ho fatto**
- **Ho insegnato a `scegli_riga` a dire quale lista.** Prima raccoglieva tutte le liste della finestra e poi prendeva la prima, sempre: `$tabelle[0]`. Adesso c'è `lista`, che accetta un pezzo del nome («Lasciate fuori») o il numero; `controlli` quel numero lo dice accanto a ogni elenco, e la numerazione esce dalla **stessa funzione** che usa `scegli_riga`, così le due non possono divergere.
- **Senza `lista` non tira più a indovinare.** Se non gliela dico, la riga la cerca in **tutte**: se il testo combacia in una sola, agisce; se combacia in due, si ferma e mi dice dove l'ha trovato. È la stessa condotta della cura di ieri — premere nella lista sbagliata è esattamente il modo in cui l'attrezzo mentiva.
- **Senza testo, adesso le racconta tutte.** Prima elencava le righe della prima lista e taceva che ce n'era un'altra: la seconda non era «difficile da raggiungere», era **invisibile**.
- **Provato dal vivo sull'applicazione vera**, su una cartella dati usa-e-getta e senza spendere una chiamata all'AI (mi sono servito di una candidatura che i documenti li aveva già). Il giro completo, andata e ritorno, tutto guidato dall'attrezzo: 26 righe → «Togli →» → 25 + 1 → riga scelta **nella lista di destra** → «← Rimetti» → 26 + 0. È il giro che il README dello strumento dichiarava provato **a mano**.
- **Tre falsificazioni, due cadute e una no.** Rimettendo `[0]`, la stessa chiamata risponde «Non c'è nessuna lista numero 2» — e senza `lista`, «Nessuna riga contiene «Competenza 16»», che è precisamente com'era prima: la riga a un palmo, e invisibile. Togliendo il portare-in-vista, la riga in fondo torna a non farsi scegliere. La terza — il `Pane` qui sotto — non si è potuta rifare, e l'ho scritto invece di lasciarla credere.

**Cosa ho imparato**
- **La causa scritta nel README era sbagliata, e l'ho scoperto in tre secondi.** Diceva: «tutte e due si presentano come Table, e la ricerca si ferma alla prima» — cioè la colpa era dell'accessibilità. Invece un nome le due liste ce l'hanno eccome, «Nel documento» e «Lasciate fuori»: WinForms presta a un controllo il testo dell'etichetta che lo precede. Non mancava il nome, mancava il modo di **dire quale**. Una diagnosi scritta un mese fa e mai riguardata mi avrebbe fatto costruire una cura molto più complicata di quella che serviva.
- **Il rettangolo che UI Automation dichiara non è quel che si vede.** Della 25ª riga di 26 diceva `y = 586` mentre la lista finisce a `610`, e della prima `y = 130` mentre la lista comincia a `370`: le righe fuori vista hanno coordinate **fuori dalla lista ma dentro la finestra**. Il clic partiva lì, la guardia del pixel rispondeva «è tua» — perché la finestra è davvero nostra — e non si sceglieva niente. Adesso, prima di premere, la riga si porta in vista.
- **Una finestra che non è una finestra può render ciechi tutti gli attrezzi insieme.** Per qualche minuto, accanto alla finestra vera è comparso un `Pane` senza controlli con un handle più alto; e siccome la radice si sceglie per handle più alto, `controlli` ha risposto tre volte «nessun controllo: l'applicazione è aperta?» con l'applicazione aperta davanti a me, `schermata` ha fotografato un rettangolo di 267 × 25 pixel e `ridimensiona` ha ridimensionato quello. Il sintomo è identico a quello di un'applicazione chiusa: se non avessi guardato le finestre del processo una per una avrei riavviato tutto e non avrei capito niente.

**Dove ho faticato**
- **La mia prima falsificazione ha rotto la sintassi invece del comportamento.** Ho tagliato via il blocco intero e ho lasciato un `try` orfano: PowerShell si è fermato al parser, e un errore di sintassi **non è** un collaudo rosso — non prova niente su quel che volevo dimostrare. Rifatta commentando la sola riga della cura.
- **E la seconda è stata verde per il motivo sbagliato.** Tolto il portare-in-vista, la riga in fondo si sceglieva lo stesso: perché per preparare la prova avevo scelto prima un'altra riga **con lo stesso attrezzo**, e quel passo — senza la cura — non aveva scorso niente, lasciando la lista già in fondo, con il bersaglio in vista. La falsificazione stava usando la cura che voleva rompere. L'ho rifatta chiudendo e riaprendo la finestra, che riparte con la vista in cima.
- **Un `$dove:` dentro una stringa non è una variabile.** In PowerShell `$nome:` è la sintassi dell'ambito (`$global:x`), e con i due punti seguiti da uno spazio il parser si ferma. Due messaggi su tre erano scritti così. Si delimita con `${dove}:`, e il controllo del parser lo dice subito — se lo si fa.

**Cosa ho deciso e perché**
- **Solo l'attrezzo, niente prodotto.** Avrei potuto dare alle due liste un `AccessibleName` nel Designer, due righe: ci avrebbe guadagnato anche l'accessibilità vera. Ma il debito è dell'attrezzo, e l'attrezzo deve saper guidare anche una lista **senza** nome — la coda di P1 è così e resterà così. E il prodotto, con il giro D che aspetta di essere rifatto sull'eseguibile giusto, è meglio che stia fermo.
- **Portare in vista sì, ridimensionare no.** Ieri avevo deciso il contrario: *rifiutare, non arrangiarsi*, perché un attrezzo che si aggiusta le condizioni prova l'applicazione in una situazione che non è quella che volevo io. Scorrere una lista però non cambia niente di quel che sto misurando: nessun evento dell'applicazione scatta, nessun dato si muove, ed è quel che farebbe una persona con la rotella prima di cliccare. Ridimensionare una finestra sì che cambierebbe la prova. La linea sta lì.
- **Agire quando il testo combacia in una sola lista, invece di chiedere sempre.** La domanda in più costa un giro e non aggiunge niente: se le parole che ho scritto io stanno in una riga sola, quella è la riga. La domanda serve quando ce ne sono due, ed è lì che l'attrezzo si ferma.

> 💡 **Il difetto più grosso della giornata non stava in quel che ho curato: stava sulla strada per provarlo.** Il debito annotato era uno, e piccolo; provandolo dal vivo ne sono usciti altri due, e uno dei due — la finestra che non era una finestra — non l'avrebbe trovato nessun collaudo automatico, perché rende cieco proprio l'attrezzo con cui si guarda. È la terza volta in una settimana che *guardare* trova più di quanto trovi il banco: qui però con una torsione in più, perché a guardare era lo strumento e il difetto era **dentro l'occhio**, non davanti.


### Step 2.53 — Il banner nuovo, e il blu che teneva in vita il vecchio

*Il marchio cambia sfondo: dentro la cornice non c'è più la spirale a spire concentriche, ma una girella a spicchi rosso e argento tagliata sulle diagonali dello scudetto Aviolab. È un cambio che avevo lavorato fuori dal repo giorni fa e mai portato dentro — e proprio per questo la sostituzione ha stanato una cosa che non mi aspettavo: il vecchio marchio non viveva solo nei PNG, viveva anche in una riga di codice.*

**Cosa ho fatto**
- **Ho messo il disegno nuovo al posto del vecchio**: la sorgente (`immagini/MASTER-solo-disegno-1536x1024.png`), la testata del README e la **schermata di avvio dentro l'eseguibile**, che è una risorsa compilata e quindi vuole una build.
- **Ho rigenerato i formati invece di ritagliarli a occhio.** I valori esatti — dove va il disegno sulla tela, il corpo del titolo e del sottotitolo, l'ancoraggio, il riempimento dei lati clonando la colonna di bordo — stavano già scritti nel `LEGGIMI.txt` della cartella di lavorazione, da quando i formati furono fatti la prima volta. Ho scritto un compositore che li applica.
- **L'ho validato prima di fidarmene**: rigenerando con quei valori il formato *già esistente* del 24 agosto, esce la stessa immagine. Solo allora l'ho usato sul disegno nuovo.
- **Ho allineato `StileApp.FondoMarchio`**, che valeva `#000C38`: adesso è il blu dello stemma `#0B06B0`, lo stesso dell'accento.
- **Ho aggiornato i due testi che descrivono il marchio**: il `LEGGIMI.md` degli asset (i quattro colori, e che lo stesso disegno alimenta lo splash) e la ricetta `prompt-logo.md`, che prende una **revisione 3**: il prompt non cambia, cambia la lavorazione.
- **Provato**: banco a **1257 verdi** prima e dopo il cambio di colore, build Release senza errori, e lo **splash guardato dal vivo** sull'applicazione avviata su una cartella dati usa-e-getta.

**Cosa ho imparato**
- **Il repository era indietro di una revisione del marchio, e nessuno se n'era accorto.** I formati «definitivi» del 24 agosto — quelli con la tavolozza Aviolab — erano rimasti nella cartella di lavoro: dentro il repo c'era ancora la versione del 22, col fondo blu notte. Non è un difetto che qualcuno potesse trovare, perché nessuno confronta due immagini che stanno in due posti diversi; l'ha trovato la sostituzione, che ha messo i due file uno accanto all'altro.
- **Un asset non è solo un file: ha un gemello nel codice.** Il fondo del banner era scritto **due volte** — nel PNG e in `StileApp.FondoMarchio` — perché la finestra dell'applicazione tiene quel colore *sotto* l'immagine. Cambiare solo il PNG avrebbe lasciato una banda di blu diverso attorno allo splash, ed è esattamente ciò che il commento di quella costante diceva da mesi, scritto da me. L'ho ritrovato cercando chi **nominava** il vecchio colore, non rileggendo il codice.
- **Quella che sembrava una macchia era un antialias fuori posto.** Attorno alle barre del grafico i pixel andavano `(250,8,37)` → `(166,64,51)` → `(146,116,89)` → nero: due pixel che sfumano verso il *grigio*, perché nel disegno di partenza sotto quel bordo c'era un anello argento, mentre nella girella lì adesso c'è rosso. Non era sporco depositato: era il bordo giusto di un'altra immagine.

**Dove ho faticato**
- **Ho fatto cercare la macchia sbagliata per tre giri.** «Vicino alle tacchette del grafico» per me era chiarissimo, ma le prime diagnosi cadevano su un'ombra nera che non esiste — il nero, misurato, era tutto contorno legittimo — e su una frangia che si vedeva solo ingrandendo. Se ne è usciti quando invece di ragionare sui colori si è misurato un **profilo di pixel**, riga per riga, e si è visto dove il fondo diventava grigio.
- **La pulizia automatica l'ho poi buttata.** Ricomposta com'era giusto — misurando quanto fondo vecchio traspariva e rimettendo quella stessa trasparenza sul fondo nuovo — il risultato tecnico era corretto e il fondo arrivava pulito fino al contorno. Guardandola accanto all'originale ho scelto l'originale: i bordi erano diventati più duri, e per due pixel non vale la pena irrigidire tutta l'illustrazione.

**Cosa ho deciso e perché**
- **Il banner entra così com'è, senza ritocchi.** L'immagine che ho scelto è quella uscita dalla lavorazione del 29 agosto, non la versione «pulita»: la macchia che mi dava fastidio, vista a schermo intero e non al microscopio, costa meno del bordo indurito che la cura si portava dietro.
- **`FondoMarchio` e `Accento` restano due costanti**, anche se oggi hanno lo stesso valore. Sono due ruoli: se un domani il marchio cambia fondo, cambia il marchio e non l'accento dei bottoni.
- **La ricetta del logo si annota, non si riscrive.** Il prompt che genera il disegno non è cambiato: è cambiato quel che gli si fa dopo. Perciò `prompt-logo.md` prende una revisione in testa che dice esattamente questo, e chi lo rigenerasse sa che otterrà il disegno *prima* della lavorazione.

> 💡 **Il marchio non stava dove pensavo che stesse.** Credevo di dover cambiare due immagini; erano tre file, una costante di codice e due testi che lo descrivevano — e la costante era la sola che, restando indietro, si sarebbe vista a occhio nudo al primo avvio dell'applicazione. Le cose che un progetto ripete in due posti si scoprono quando una delle due cambia: fino a quel giorno sono d'accordo per caso.


### Step 2.54 — Un filetto tre volte più spesso, e cinque secondi per leggerlo

*Due richieste fatte insieme, e che stanno insieme: il filetto giallo dentro la cornice del marchio era troppo sottile perché si notasse, e la schermata di avvio passava troppo in fretta perché la si potesse guardare. Ingrossare una riga sembrava il compito da due minuti della giornata; è quello che mi ha fatto pagare due difetti, tutti e due perché il disegno non è fatto di colori piatti come credevo.*

**Cosa ho fatto**
- **Ho ingrossato il filetto da 6-7 pixel a 20-21**, sulla sorgente a 1536×1024, facendolo crescere **solo verso l'interno**: il bordo esterno del disegno non si è mosso di un pixel. Dove le maniche della tuta tagliano la cornice a mezza altezza la crescita si ferma da sé, così la riga resta interrotta esattamente com'era.
- **Ho preteso una prova, non un'impressione**: 63.750 pixel cambiati, di cui 62.894 blu della cornice e 856 il filo scuro di antialias fra riga e blu — zero imprevisti. Spessore minimo 20 px su tutti e quattro i lati.
- **Ho rigenerato i formati dal disegno nuovo** con lo stesso compositore dello Step 2.53, validato prima come allora: dandogli la sorgente vecchia restituisce la testata del README e lo splash già committati, bit per bit. Cambiano tre file — la sorgente, la testata, e lo splash **dentro l'eseguibile**, che è una risorsa compilata e quindi vuole una build.
- **Ho portato `FinestraAvvio.MinimoAVideo` da 1500 ms a 5 secondi**, e l'ho **misurato sull'applicazione viva** invece di dedurlo: comparsa a 0,47 s, sparita a 5,47 s — 5,00 s a video. Lo splash fotografato dall'eseguibile ricompilato mostra la riga nuova.
- **Ho corretto il cap. 03.4**, che diceva che la schermata viene congedata «un secondo prima della sua scadenza»: con il minimo nuovo non era più vero. E `immagini/LEGGIMI.md` annota il filetto accanto alla girella dello stesso giorno.
- **Provato**: banco a **1257 verdi**, 0 falliti.

**Cosa ho imparato**
- **Il filo scuro non è sempre scuro.** Fra la riga gialla e il blu c'è un filo di antialias che credevo quasi nero; alcuni suoi pixel arrivano a 90-95 su un canale, e una soglia fissata a 90 fermava la crescita lasciando striature blu **dentro** la riga. La regola che funziona non guarda il colore ma la **sottigliezza**: un filo si attraversa solo se il blu della cornice ritorna entro tre pixel — dentro una manica non ritorna mai.
- **I bottoni dorati del polsino passavano per riga gialla.** Con una tolleranza di 40 il loro `(245,187,45)` rientrava nel giallo del filetto, e si sono ritrovati ridipinti. La riga è un colore piatto esatto: la tolleranza va tenuta stretta, e il fatto che una tolleranza generosa «funzioni» sulla riga non dice niente su cosa altro sta raccogliendo per strada.
- **Il minimo a video non è un ostaggio.** Cinque secondi sono tanti se c'è qualcosa da chiedere all'utente: il minimo continua a cedere il passo, e al primo avvio la finestra della chiave API manda via la schermata subito, come prima.

**Dove ho faticato**
- **Le prime due passate le ho buttate**, e tutte e due per lo stesso motivo: avevo descritto il disegno come se fosse fatto di campiture piatte. Non lo è — è un'illustrazione con antialias dappertutto, e ogni regola scritta sul colore ha un'eccezione da qualche parte nell'immagine. Le regole che hanno retto sono quelle scritte sulla **forma** (quanto è sottile, da che parte cresce), non sulla tinta.

**Cosa ho deciso e perché**
- **Crescere solo verso l'interno, mai verso l'esterno.** Il bordo esterno del disegno è il punto in cui il marchio incontra tutto il resto — la testata del README, lo splash, l'icona: spostarlo di un pixel avrebbe voluto dire rifare i conti di tutti i formati. Verso l'interno c'è solo il blu della cornice, che è mio.
- **Contare i pixel cambiati, non guardare il risultato.** «Sembra giusto» su un'immagine è un'affermazione che non si può falsificare. 62.894 blu e 856 filo, e nient'altro, sì.

> 💡 **Un'immagine non si modifica: si modifica un elenco di pixel che si crede di aver capito.** Le due volte che ho sbagliato avevo in mente il disegno come lo vedo io — riga gialla, cornice blu, contorno nero — mentre il file è pieno di sfumature intermedie che appartengono ora all'una ora all'altra cosa. Il conto finale, per categoria, non serve a documentare il lavoro: serve a scoprire che si stava ridipingendo un bottone.


### Step 2.55 — Un eseguibile solo, sul Desktop, e la riga di git che non ha mai parlato

*Fino a oggi due file si chiamavano `TrovaLavoro.exe`: quello di `bin/Release`, che è quello su cui giravano le prove, e quello che aprivo a mano quando non c'era nessuna sessione in corso. Niente diceva se fossero la stessa versione. Adesso ne esiste uno solo, sul Desktop, e lo usano tutti e due i lati. Cercando di farlo, ho trovato una riga di `.bat` che non ha mai funzionato — e che zittiva proprio le due guardie messe lì per impedire un rilascio senza identità.*

**Cosa ho fatto**
- **Ho scritto `strumenti/aggiorna-riferimento.bat`**: rifà l'eseguibile di riferimento sul Desktop in circa sei secondi, file unico e autonomo, con il runtime .NET dentro e gli stessi parametri di rilascio del cap. 13.2 — 113 MiB.
- **Gli ho fatto stampare l'identità alla fine**: versione, commit, dimensione, SHA-256. Un numero di versione è un'etichetta scritta a mano, e due file diversi hanno già portato lo stesso «1.0.000».
- **Ho puntato lì lo strumento di collaudo**: `compila` chiama quello script invece di far girare `dotnet build`, e `avvia_app` lancia quel che ha prodotto — verificato leggendo il percorso del processo vivo, non fidandomi. Anche `avvia-demo.bat` lancia quello.
- **Ho spostato la compilazione intermedia in `%TEMP%`**, fuori da `bin/Release`. Quel file è proprio quello che il server MCP del prodotto tiene bloccato, ed era il motivo per cui `compila` non si poteva chiamare senza sacrificare i tool della sessione. Adesso si può.
- **Ho tolto la chiusura per nome** da `compila` e `chiudi_app`: passano da `chiudi-finestre.ps1`, che legge la riga di comando e risparmia chi ha `--mcp`. **Falsificato con una finestra davvero aperta**: la finestra si è chiusa, il server è rimasto vivo.
- **Ho corretto `publish.bat`** e ho verificato che nessun rilascio ne fosse uscito storto.

**Cosa ho imparato**
- **`git -C "%~dp0"` non ha mai funzionato, e falliva in silenzio.** La variabile `%~dp0` finisce con una barra rovescia: dentro le virgolette quella barra si mangia la virgoletta di chiusura, git riceve un percorso malformato, fallisce senza dire niente, e il ciclo `for` che doveva raccoglierne l'uscita non assegna nulla. Scritto col punto — `git -C .` — funziona: provato da quella cartella, `[]` prima contro `[a0507f2]` dopo.
- **Zittiva due guardie in una volta sola**: il commit timbrato dentro l'eseguibile e l'avviso «ci sono modifiche non committate». Cioè esattamente i due controlli che esistono per fermare un rilascio senza identità. Un difetto che disattiva i controlli è peggio di un difetto che rompe una funzione: la funzione rotta si vede, il controllo spento no.
- **Nessun rilascio è però uscito mal etichettato**, e me ne sono accertato invece di supporlo: l'unico eseguibile in `pubblicazione/` è del 24 agosto, tre giorni prima che il timbro esistesse. L'ho rimesso intatto dopo la prova a fondo — stesso SHA-256, stessa dimensione, stessa data.
- **Il riferimento non «assomiglia» al rilascio: è lo stesso file.** Dallo stesso working tree le due ricette producono un eseguibile identico bit per bit, SHA-256 `99d178e2…`. Un controllo incrociato che vale la pena rifare ogni volta che una delle due cambia.

**Dove ho faticato**
- **`collaudi` chiude ancora per nome, e non ho potuto curarlo qui.** Fa `dotnet test -c Release`, che ricompila proprio il file che il server MCP tiene bloccato: finché la compilazione dei collaudi vive in `bin/`, quel server va spento. Ho preferito scriverlo nel README dello strumento piuttosto che deciderlo di nascosto dentro il codice.

**Cosa ho deciso e perché**
- **Un solo eseguibile, e sul Desktop.** Non in `bin/Release`, che è terra della compilazione e viene sovrascritta; non in `pubblicazione/`, che è la cartella dei rilasci veri. Sul Desktop è dove lo aprirei comunque a mano, ed è l'unico posto in cui «quello che provo io» e «quello che prova l'assistente» non possono divergere senza che me ne accorga.
- **L'identità stampata a ogni ricostruzione.** Costa una riga e toglie di mezzo la domanda «ma questo exe è aggiornato?», che negli ultimi giorni mi sono fatto tre volte.

> 💡 **Le due guardie del rilascio erano spente da sempre, e nessun collaudo poteva accorgersene.** Non c'era un rosso da guardare: c'era un `for` che non assegnava, e un timbro che restava vuoto. L'ho trovato solo perché stavo riusando lo stesso idioma altrove e mi sono chiesto se funzionasse davvero — cioè per lo stesso motivo per cui il cap. 16 delle regole dice di rileggere quel che una tappa aveva *promesso*: le cose che non fanno rumore non le trova nessuno che stia guardando altrove.


### Step 2.56 — I dati di prova senza più nessuno dentro, e le quattro volte che un a-capo mi ha ingannato

*Ieri il profilo di prova è diventato Crash Bandicoot, e credevo che la faccenda finisse lì. Invece «elimina profilo» tocca solo il profilo: i miei dati veri erano ancora dentro le candidature, i documenti, i backup — e in sette cartelle, non due. Ripulirle è stato un lavoro da sostituzioni cieche, cioè il genere di lavoro che riesce quasi sempre e che quando fallisce non lo dice.*

**Cosa ho fatto**
- **Ho censito prima di toccare.** Sette cartelle dati, non le due che ricordavo. E la sorpresa: i dati veri dentro le candidature non erano di Riccardo ma **miei** — nome, email, telefono, città, in 6 CV, 6 lettere, 2 email e in `documenti.json`, ripetuti identici in tre cartelle. Di Riccardo erano il profilo di R7 e quello dentro i backup.
- **Ho fatto una copia di sicurezza** delle sette cartelle in `C:\Temp` prima di qualunque modifica, ed è servita.
- **Ho anonimizzato 78 file JSON** in tre cartelle: 516 sostituzioni fra nome, cognome, due email, due telefoni, città, sito e i nomi dentro gli allegati. I contatti diventano quelli di Crash Bandicoot; i **testi restano quelli veri**, perché è il nome che identifica, non il mestiere.
- **Ho riscritto `documenti.json` da capo** invece di anonimizzarlo: conteneva i percorsi dei documenti d'identità e dei codici fiscali, miei e di Riccardo. Adesso ha 13 voci finte e parlanti — Crash e Coco Bandicoot — con le stesse categorie e gli stessi motivi di prima.
- **Ho eliminato 62 documenti esportati** (`.docx`, `.pdf`, `.eml`): il nome vero stava anche nel nome del file, e un `.docx` non si corregge con una sostituzione di testo. Si riesportano dall'applicazione senza spendere una chiamata all'AI.
- **Ho messo il profilo Crash in tutte e tre le cartelle**, produzione compresa — che dal 29 agosto era rimasta **senza profilo**, e il diario tecnico lo diceva: `FileNotFoundException … profilo.json`, ore 18:32:36.
- **Ho eliminato quattro cartelle di collaudi chiusi** (T7a, T7b, T7c, il backup di T5d): 88 MB, quasi tutti cache del browser incorporato, e quattro copie del mio profilo.
- **Ho eliminato le tre candidature TM PEDANE e i due backup del 21 agosto**, che portavano dentro il CV di Riccardo con la descrizione di TTR-SUITE — la sola cosa in tutta la faccenda coperta dalla regola 10. Le voci corrispondenti sono uscite anche dal registro: 10 → 7.
- **Ho creato `C:\Temp\Documenti-di-prova`** con 13 file finti e PDF veri, così la cartella che `documenti.json` dichiara esiste davvero e l'applicazione non protesta più.
- **Provato dal vivo**, con l'applicazione vera su R6: profilo Crash Bandicoot, sette candidature in elenco, confronto, 🎯 CV-2 e lettera leggibili. Nessuna chiamata all'AI spesa.

**Cosa ho imparato**
- **Nel JSON un a-capo è la coppia di caratteri `\` `n`, e questo mi ha ingannato quattro volte di fila.** *Uno*: la classe di caratteri davanti all'email si è mangiata la `n` dell'escape, lasciando una barra rovescia spaiata — nove file non erano più JSON validi. *Due*, ed è il peggiore: `\b` non scatta fra la `n` di `\n` e la `M` di `Mirco`, perché sono due caratteri di parola attaccati; così `\r\nMirco Parenti` non veniva riconosciuto come nome+cognome, passava solo la regola sul cognome, e restava **`Mirco Bandicoot`** in un file perfettamente valido. *Tre*: anche l'underscore è carattere di parola, e nei nomi degli allegati — `CV_Mirco_Parenti_…` — il confine non c'era. *Quattro*: pretendevo `https://` e nel campo link c'era `www.aviolab.ai`.
- **Il controllo che il file sia valido non vede il difetto peggiore.** I nove file rotti li ha trovati subito; i dodici nomi rimasti in chiaro stavano in file validissimi, e li ha trovati solo il secondo controllo — fatto con un metodo diverso, cercando le parole invece di rileggere lo script.
- **Anonimizzare i contatti non anonimizza il contenuto.** Tolti nome, email e telefono, il CV di TM PEDANE restava il curriculum di Riccardo, con dentro brevetto e architettura di TTR-SUITE. Fuori dal repo non viola niente; in uno screenshot per il diario o per la demo sarebbe una pubblicazione.
- **La cura si prova rompendola.** Prima di applicare le sostituzioni ho verificato che, togliendo la protezione degli escape, il difetto **ritorni**: se non tornasse, il collaudo sarebbe verde per il motivo sbagliato. E che senza regole il testo torni identico su tutti e 169 i file, così una passata a vuoto non muove un byte.

**Dove ho faticato**
- **La prima versione dello script l'ho lanciata sul serio prima di provarla su un caso scritto a mano.** Ha rotto nove file, e li ho rimessi dalla copia di sicurezza in un secondo — ma la copia me l'ero fatta per prudenza, non perché sapessi cosa stavo per fare. Il caso di prova costava tre righe e sarebbe arrivato prima del danno.
- **Il secondo controllo l'ho scritto con lo stesso difetto del primo.** Cercando «qualunque email diversa da quella finta» mi ha risposto `ncrash.bandicoot@esempio.it`: la stessa `n` di prima. Non era un dato vero, ma per un momento l'ho creduto.

**Cosa ho deciso e perché**
- **I testi restano veri, i contatti no.** Rigenerare tutto con Crash Bandicoot sarebbe costato una settantina di chiamate all'AI per riavere quel che ho già: candidature in stati diversi, alcune ferme al confronto e altre complete, che è precisamente ciò che le rende utili come banco di prova.
- **Le TM PEDANE però via.** Lì il contenuto non era il mio mestiere ma quello di un'altra persona, e conteneva materiale che la regola 10 tiene fuori dal repo. Sette candidature bastano.
- **Anche i due backup del 21 agosto via**, benché fossero il solo dato di prova per il ripristino: contenevano il profilo intero di Riccardo, e tenerli avrebbe reso vano tutto il resto. Un backup nuovo si rifà dall'applicazione in dieci secondi, e sarà coerente con Crash.
- **La cartella di produzione la tratto come le altre.** Non ci lavoro più davvero da settimane, e tenerla «vera» significava solo tenere i miei dati in un posto che apro per sbaglio.

> 💡 **«Elimina profilo» elimina il profilo, e questo non è un difetto: è che io leggevo quella parola come «elimina i miei dati».** Il profilo è una cosa sola in una cartella; i dati veri erano già colati nelle candidature il giorno in cui le avevo generate, nei documenti esportati, nei backup, in un `documenti.json` che si portava dietro i nomi dei file dei documenti d'identità. La domanda giusta non era «dov'è il profilo», era **«chi ha copiato il profilo, e quando»** — ed è la stessa domanda che il 29 agosto aveva trovato il colore vecchio nascosto in una costante.

### Step 2.57 — Il timbro nella fascia, e il difetto che si nascondeva dietro il proprio colore

*Il banner è arrivato alla sua forma definitiva: lo scudetto Aviolab entra nella fascia del testo come timbro, il nome prende un contorno nero che salda le lettere in un blocco solo, il sottotitolo diventa bianco pieno. Per far posto al timbro la fascia si alza, e alzandosi cambia l'altezza di ogni formato — che sembrava un dettaglio di grafica ed è invece la sola cosa di tutta la giornata che ha toccato il codice. Ho trovato due cose che non stavo cercando: nel repository c'era una versione del disegno anteriore all'ultima ripulitura, e una finestra che avrebbe rimpicciolito il marchio senza che nessuno potesse vederlo.*

**Cosa ho fatto**
- **Ho messo i tre file nuovi**: la sorgente, la testata del README e la schermata di avvio dentro l'eseguibile. Tutti e tre **bit-identici** alla cartella dei definitivi, verificato con `cmp` dopo aver finito, non solo dopo aver copiato.
- **Ho rinominato la testata** da `TrovaLavoro-readme-1200x972.png` a `-1200x1052.png`, con `git mv` invece di sovrascriverla: il nome porta la misura, e una misura sbagliata dentro il nome è una bugia che nessun controllo legge.
- **Ho aggiornato le misure nel codice**: la finestra di avvio da 800×648 a 800×702 (due valori nel `Designer` e due commenti), e il collaudo che sorveglia l'altezza della risorsa incorporata.
- **Ho adeguato «Informazioni su…»**: il riquadro del marchio da 520×421 a **520×456**, le otto righe sotto scalate di 35 pixel, la finestra da 548×648 a 548×683.
- **Ho falsificato prima di dirlo fatto**: rimesso `648` nel collaudo dell'altezza, che è diventato **rosso** dicendo «Previsto `<648>`. Effettivo `<702>`». Ripristinato con `cp`, banco di nuovo verde — **1257 verdi**, 0 falliti.
- **Ho guardato a video** sull'eseguibile di riferimento appena rifatto: la schermata di avvio col timbro, e «Informazioni su…» dove il marchio riempie il riquadro senza strisce ai lati.
- **Ho aggiornato i testi**: `immagini/LEGGIMI.md` (misure, nome del file, il timbro e il contorno, il sottotitolo bianco), `prompt-logo.md` con la revisione 4, il cap. 03.4 e la testata del README.

**Cosa ho imparato**
- **Nel repository c'era un disegno più vecchio di quello che credevo di avere.** Confrontando pixel per pixel la sorgente committata con quella della cartella di lavoro sono usciti **1.534 pixel diversi**, tutti in un rettangolo attorno all'istogramma: residui scuri rimasti dalla lavorazione, che nella cartella di lavoro erano già stati puliti un'ora dopo il commit. Nessun controllo poteva accorgersene, perché nessuno confronta due immagini che stanno in due posti diversi — è esattamente il difetto che avevo annotato il 30 agosto e che si è ripresentato subito.
- **Il difetto peggiore della giornata sarebbe stato invisibile per costruzione.** Il riquadro del marchio in «Informazioni su…» era tagliato sulle proporzioni vecchie: con l'immagine nuova, più alta, si sarebbe rimpicciolita lasciando due strisce ai lati. Ma lo sfondo di quel riquadro è `StileApp.FondoMarchio`, cioè **lo stesso blu del banner**: le strisce non si sarebbero viste. Il marchio sarebbe semplicemente diventato più piccolo, e nessuno avrebbe saputo dire perché.
- **Un collaudo che diventa rosso dice più di quel che gli si chiede.** Il rosso su `648` contro `702` non prova solo che il collaudo sorveglia l'altezza: prova anche che la risorsa **dentro l'eseguibile** è quella nuova, che è la cosa che volevo davvero sapere e che non avevo modo di guardare.

**Dove ho faticato**
- **Poco, e vale la pena dire perché**: quasi tutto il lavoro era già stato fatto dal `LEGGIMI.txt` della cartella dei definitivi, che porta i valori esatti, le due impaginazioni, i controlli da rifare se si tocca una manopola. Il tempo se l'è preso la **ricognizione** — trovare i cinque posti e i dodici file che si trascinano dietro una misura — non la modifica.

**Cosa ho deciso e perché**
- **Adeguare il riquadro invece di lasciar rimpicciolire l'immagine.** Costava dieci coordinate e una finestra 35 pixel più alta; l'alternativa era un marchio più piccolo del necessario in una finestra che esiste apposta per mostrarlo.
- **L'icona dell'eseguibile resta indietro.** Porta ancora il disegno vecchio ed è già una voce di `in_sospeso.md`: un'icona è 16 pixel, e il disegno nuovo va guardato ridotto prima di dire che regge. Non è il lavoro di oggi.

> 💡 **Un difetto che si nasconde dietro il proprio colore non è un caso fortunato: è il caso normale.** Le strisce ai lati del marchio sarebbero state dello stesso blu del marchio perché qualcuno — io, in una tappa precedente — aveva fatto la cosa giusta, cioè allineare il fondo del riquadro al fondo dell'immagine. La cura di ieri è ciò che avrebbe reso muto il difetto di oggi. Non c'è modo di trovarne uno così guardando lo schermo: si trova solo chiedendosi **chi altro dipende dalla misura che sto cambiando**.

### Step 2.58 — La revisione di sicurezza: cinque rilievi, e i quattro fix che nessuno sorvegliava

*Oggi si è aperta la revisione di finalizzazione, su un ramo suo (`feature/finalizzazione`): tre fasi — sicurezza, interfaccia, e il confronto fra quello che il repository dichiara e quello che il programma fa — condotte dal tutor con una regola scritta in apertura, **nessun fix senza approvazione, voce per voce**. Io ho seguito e approvato ogni passaggio: il lavoro è suo, le decisioni le abbiamo prese insieme, e alcune rovesciano scelte che avevo fatto due giorni fa. La prima fase è andata a cercare cosa può fare un ingresso ostile, e ha trovato quattro cure già scritte nel mio working tree che nessun collaudo sorvegliava.*

**Cosa ho fatto**
- **Ho scritto il perimetro e il modello di minaccia prima di cercare**, invece di andare a caccia a naso: macchina Windows mono-utente fidata (un altro utente della stessa macchina è fuori scope, la chiave è già protetta da DPAPI a scope utente), e come avversari il **contenuto web ostile** degli annunci, i **file CV ostili**, l'**output del modello**, la **rete**, il **client MCP**.
- **Ho fatto mettere per iscritto anche quello che è risultato solido**, non solo i rilievi: chiave mai nel repository e mascherata a video, solo HTTPS in uscita e nessun bypass TLS, il server MCP del prodotto che è a `stdio` e non apre porte, il PDF che non si legge in casa ma va in base64 all'API, il DOCX letto per entry nominata, i percorsi da contenuti AI sempre passati dal `Sillabario`, e il destinatario dell'email che **lo digita l'utente** — quindi un annuncio ostile non può dirottare la candidatura.
- **Cinque rilievi (R1-R5) e due in più emersi durante la verifica.** R1: il server MCP scriveva le eccezioni intere su `stderr` **senza** passare dalla redazione — che esisteva solo nel diario su file, non in quello che il client raccoglie nei suoi log. R4: lo strumento di collaudo parlava `bash -lc` con gli argomenti interpolati dentro la stringa. R5: il prototipo ascoltava su **tutte** le interfacce invece che sulla sola macchina. I due emersi in verifica: i nomi allegato tutti-ASCII saltavano la ripulitura degli a capo (header injection via nome file), e lo slug di azienda+titolo non aveva tetto.
- **Ho fatto mappare la prompt injection sink per sink** (R2), che era l'unico rilievo con la verifica ancora aperta: nomi di file, HTML e PDF, DOCX, EML, interfaccia, archivio, tool MCP. Verdetto: dal testo ostile **non esiste un percorso** verso esecuzione di codice, scrittura fuori dalla cartella dati, intestazioni email nascoste o esfiltrazione senza un gesto dell'utente.
- **Quattro fix del prodotto con quattro collaudi nuovi, tutti visti rossi falsificando**, e le righe scritte in `falsificazioni.md`. Il fix del prototipo in un **commit a parte**, e lo strumento di collaudo che smette di parlare shell: i siti erano **cinque**, non tre come stimato.
- **Ho fatto scrivere il confine di fiducia** in coda al §9.5 del cap. 09 e una riga nella GUIDA sul rovescio dei dati in chiaro. Banco verde a 1343.

**Cosa ho imparato**
- **I quattro fix erano già nel mio working tree, e quello che mancava non era il codice: era la prova che qualcuno se ne accorgerebbe.** Erano lì da prima della revisione, scritti bene, e nessuno dei tre del prodotto aveva un collaudo. Un fix senza collaudo è una cura che il prossimo giro può disfare in silenzio — ed è il caso in cui la regola 14 serve di più, perché non c'è nemmeno un difetto che si veda.
- **Rompendo il tetto dello slug, il collaudo non ha fatto in tempo a dire «previsto/effettivo»**: è caduto prima con l'`IOException` del sistema operativo, cioè esattamente il danno che il tetto previene. Un rosso che arriva dal posto sbagliato può essere la dimostrazione più diretta.
- **Il difetto più netto della fase non era nel prodotto**: stava in `strumenti/`, che non si distribuisce e proprio per questo non lo guarda nessuno. La falsificazione ha dimostrato l'iniezione vera — la forma vecchia piazzava un file in silenzio.
- **Un rilievo si può anche ridimensionare in verifica**, e va detto: i tool MCP di scrittura sembravano un buco, ma `esporta_backup` scrive comunque solo dentro la cartella dati. Da buco è diventato confine da documentare.

**Dove ho faticato**
- **Il giro dal vivo dello strumento senza shell non si è potuto fare subito** — il server era occupato da un'altra sessione — ed è rimasto come riserva dichiarata invece che come «fatto». Si è chiuso **lo stesso giorno**, quando il giro a vista dei fix d'interfaccia è passato tutto da quel codice: compila, avvia, otto clic, sei schermate, accenti compresi.
- **Due residui non si curano col codice, e ammetterlo costa.** L'inganno sul contenuto (un annuncio che orienta il testo del CV) e l'iniezione di secondo ordine verso il client MCP non hanno una riga da scrivere: hanno solo l'anti-invenzione, la revisione umana e un confine dichiarato.

**Cosa ho deciso e perché**
- **Il fix del prototipo in un commit tutto suo.** `HTML+JS/` è congelato e fuori dal rito: una riga sola, ma è voce a parte della pull request, così quando la guardo so che sto accettando una cosa diversa dal resto.
- **Il confine di fiducia si scrive dove il client lo legge**, cioè nel capitolo del server MCP, non in una nota di sicurezza che nessuno apre.
- **Anche i fix già scritti passano dall'approvazione.** Erano miei e li avrei committati senza pensarci: farli ratificare uno per uno è quello che ha fatto emergere che nessuno dei tre aveva un collaudo.

> 💡 **Le cure senza guardia sono la cosa più difficile da vedere, perché somigliano al lavoro finito.** Un difetto lo trovi perché qualcosa si rompe; un fix non sorvegliato non rompe niente — sta lì, funziona, e l'unico momento in cui la sua assenza di collaudo diventa visibile è quando qualcuno lo cancella per sbaglio, cioè quando è già tardi. L'unica cosa che li ha stanati è stata la domanda posta in ordine inverso: non «questo codice funziona?», ma **«che cosa diventerebbe rosso se smettesse di funzionare?»** — la stessa che mi ero annotato prima del giro D.

### Step 2.59 — Venti difetti d'interfaccia, dodici divergenze, e il marchio che torna in un posto solo

*La seconda fase ha misurato l'interfaccia contro un metro industriale — sessantasei regole — con una clausola insolita che il tutor ha messo in apertura: in questa revisione anche **le mie linee guida di progetto sono in discussione**. Non era scontato, e ha cambiato la forma del lavoro: i rilievi si sono divisi in difetti veri (venti) e divergenze di sistema (dodici), dove a essere sotto esame non era l'implementazione ma la regola che avevo scritto io. Undici divergenze le abbiamo tenute. Una l'abbiamo rovesciata, e ha portato via il mega stemma che avevo messo due giorni fa.*

**Cosa ho fatto**
- **Ho fatto passare tutto `VB.NET/src/TrovaLavoro/`**: l'infrastruttura di stile, le undici finestre di dialogo, gli otto pannelli, sempre `.vb` più `.Designer.vb`. Quarantasei rilievi grezzi, consolidati in **venti di categoria A** (difetti) e **dodici di categoria B** (divergenze).
- **Ho deciso voce per voce col tutor.** Categoria A: tutti approvati, con una sola riduzione — dello scudo che compare senza soglia si è preso solo l'anti-flash a ~300 ms, il rispetto della preferenza di sistema per il movimento ridotto **non si fa**. Categoria B: tenuti i bottoni a 32 px, il corpo a 9 pt, la griglia 14/12/8, il sistema dei livelli di conseguenza, la barra a sette caselle, il Bold sui titoli, l'altezza degli input, lo splash a 5 s e le stringhe senza `.resx`. Corrette solo le didascalie, da 8 a 9 pt, perché portano il *motivo* dei bottoni spenti.
- **Ho fatto scrivere il piano prima di partire**: sette blocchi in ordine — prima i token, che stanno a monte di tutto, poi i comportamenti, poi i locali — con il banco verde a **fine di ogni blocco**, non solo alla fine, e le scelte fini lasciate all'implementatore a parità d'intento.
- **I contrasti**: il verde `Successo` scurito a `#1E7E34` (il bianco sopra era a 3,13:1), un token nuovo `RossoCritico` `#B00013` per il fondo del livello 6, e `InformazioneTesto` per il testo informativo che stava a ≈2,8:1. Il rosso del marchio resta ai titoli grandi, che sono testo grande e possono starci.
- **I comportamenti**: l'import del CV — la chiamata AI più lunga di P2 — non alzava il filo del lavoro, e si poteva uscire e avviarne una seconda da un altro pannello; le finestre che svuotano la cache o fanno un backup congelavano perché l'I/O era sincrono; in P6 e P7 un'attesa AI non si poteva annullare da nessun controllo, benché il metodo per farlo esistesse già.
- **Il resto**: gli errori che ora portano la parola e non solo il colore, il DPI dei tre angoli rimasti indietro, i sette bottoni di P3 da 26 a 32 px, le conferme di pari livello che parlavano due lingue (finestra col verbo «Confermo» da una parte, `MessageBox` di sistema dall'altra), due verdi gemelli affiancati, due rossi adiacenti, e i micro-difetti locali — stati vuoti muti, etichette che erano solo un placeholder, colonne numeriche allineate a sinistra, due `TabIndex` uguali.
- **Trentatré collaudi nuovi, tutti visti rossi falsificando.** Banco da 1335 a **1368**, verde a fine di ogni blocco.

**Cosa ho imparato**
- **La differenza fra un difetto e una divergenza è chi ha ragione, e non è sempre il metro.** «Un solo bottone pieno per fascia» contro il mio sistema dei livelli non è un mio difetto: è un altro modello mentale, documentato, con correzioni già pagate sul campo a T9d. Ma dove il mio sistema **si contraddice da sé** — due L1 verdi identici e adiacenti per due azioni diverse, due rossi pieni attaccati — lì il difetto è mio, e il metro non c'entra: me lo dice la mia stessa regola.
- **Il metro sbagliato tiene un collaudo verde meglio di qualunque bug.** La prova che il livello 6 ha il fondo più scuro del 5 era scritta con `GetBrightness()`, che è luminosità HSL: per lei il rosso del marchio è più scuro di quello di L5, mentre l'occhio vede l'opposto. Rimettendo il colore sbagliato, il collaudo restava **verde** — misurava una proprietà vera, ma non quella che la regola nomina. Adesso si misura quanto bianco ci si legge sopra, cioè con lo stesso metro con cui il cap. 03.2 scrive tutti i suoi numeri.
- **La cura non si estende da sola dove non è passata.** Nel progetto il contrasto delle didascalie è curato al centesimo, e nello stesso programma il bianco sui bottoni verdi stava a 3,13:1. Non è distrazione: è che avevo guardato *quella* cosa lì, e mai l'insieme con lo stesso metro.
- **Un rilievo può essere impreciso e restare utile.** «P7 è l'unico pannello senza `AutoScaleMode`» era falso — anche P0 non ce l'ha. Non è stato corretto, perché fuori dal piano approvato, ed è finito nel registro dei fuori perimetro invece che in una correzione di soppiatto.

**Dove ho faticato**
- **Due sessioni sullo stesso repository, e l'index di git è uno solo.** Il primo commit del blocco dei token ha inglobato sei file della sessione di sicurezza stagiati nel frattempo. Rimediato con `reset --soft` e ricommit pulito, e da lì in poi `git add` mirato ai soli file del blocco, mai `-A`, con l'avviso reciproco prima di ogni stage.
- **Il perimetro di un fix non è mai dove lo immagini.** Il colore dei titoli piccoli non stava in un helper: stava in dodici righe `ForeColor` sparse su sette Designer. Un fix «di un token» è diventato un giro su sette file.
- **Due riserve sono rimaste aperte, e le ho scritte invece di ignorarle**: la prova a 150 % vuole una macchina a quella scala, e i flussi AI dal vivo vogliono una chiave che su questa macchina non c'è.

**Cosa ho deciso e perché**
- **Il marchio torna a vivere in un posto solo**, il pannello in basso a sinistra. Il mega stemma dietro il menu l'avevo messo io il 30 agosto, e lo scudo Aviolab come indicatore d'attesa fra il 30 e il 31: due scelte mie, recenti, rovesciate nella stessa giornata. L'indicatore adesso è neutro — ruota di pallini e barra che si riempie — e resta funzionale com'era.
- **Undici divergenze su dodici restano.** Non per affetto: perché sono un sistema locale coerente e documentato, e riallineare la griglia o alzare tutti i bottoni costerebbe ogni Designer per un beneficio che nessuno vedrebbe.
- **I fuori perimetro si annotano e non si correggono.** Ne sono usciti parecchi, dal `TabIndex` doppio in un'altra finestra ai margini di `StileApp` che restano in unità di progetto: correggerli in corsa avrebbe reso il diff di ogni blocco impossibile da rivedere contro il piano.

> 💡 **Il mega stemma l'avevo moltiplicato per affetto, non per disegno.** Quando la revisione ha chiesto perché il marchio comparisse in tre posti, la risposta onesta era che ogni volta che ne mettevo uno mi piaceva — non che servisse. Il sistema dei livelli, invece, ho saputo difenderlo con una ragione, e infatti è rimasto. È una prova che mi porto dietro: se davanti a una regola mia so dire **quale problema risolve**, è una linea guida; se so dire solo che mi piace, è un gusto, e i gusti in un'interfaccia si pagano.

### Step 2.60 — Il dichiarato contro il fatto: dieci scostamenti, e una promessa ritirata

*La terza fase non cercava difetti nel codice: cercava la distanza fra quello che il repository promette — il README, la GUIDA, i capitoli di progetto — e quello che il programma fa davvero. È la fase che temevo di più, perché la documentazione l'ho scritta io e mi fido di quello che ricordo di averci messo. Il prodotto è uscito notevolmente fedele; ma la voce più pesante della giornata non è stata un difetto — è stata una promessa di progetto che abbiamo deciso di ritirare.*

**Cosa ho fatto**
- **Ho definito che cosa conta come «dichiarato»**: README (stato e funzioni), GUIDA (quello che l'utente vede e fa), le promesse dei capitoli — in particolare i flussi del cap. 12 e i capitoli funzionali 05-11 — e `in_sospeso.md`. Con una regola che mi ha risparmiato una giornata di falsi positivi: **le riserve già dichiarate non sono rilievi**, si spuntano come «dichiarato coerente» e si cercano le divergenze *non* dichiarate.
- **Ho fatto confrontare promessa per promessa, col codice alla mano.** Sul fronte funzionale nessuna divergenza vera: tredici tool MCP coi nomi del cap. 09, le due ere del protocollo, il lucchetto asimmetrico, il backup con le esclusioni promesse, i modelli dalle Impostazioni, l'informativa prima della chiave, DPAPI e mascheratura — tutto riscontrato al file e alla riga. I flussi del cap. 12 conformi passo per passo, con un'eccezione sola.
- **Dieci scostamenti, da V1 a V10.** Cinque sono nomi rimasti indietro nella GUIDA — la stella diventata «★», «📄 Documenti» diventato «▤ Documenti», «Elimina tutto» che a video è «ELIMINA TUTTI I DATI», «Backup e ripristino» che è il titolo della finestra e non la voce del menù, e il link che si incolla in Ricerca mentre il testo si incolla in Confronta. Uno è il conteggio dei collaudi nel README, fermo al 30 agosto. Uno è il cap. 12 che dichiara **aperto un debito chiuso** il 19 agosto. Uno è una freccia mancante nel diagramma della macchina a stati.
- **V1, il solo scostamento sostanziale della GUIDA**: prometteva che l'assenza del runtime WebView2 te la dica **all'avvio** e ti dia **il link ufficiale di Microsoft**. Nessun controllo all'avvio (l'accensione è pigra, ed è una scelta), e il link non esisteva in tutto il sorgente. Corretta la GUIDA **e** aggiunto il link ai due messaggi del codice, perché altrimenti la GUIDA appena corretta sarebbe tornata bugiarda.
- **Ho preso la decisione sul flusso D**: la sessione di aggiornamento del profilo, con proposta datata, dialogo differenziale e salvataggio versionato, promessa dal cap. 12.4 e **mai costruita** — il bottone è spento, senza gestore, e l'infrastruttura di versionamento esiste ma non la usa nessuno.

**Cosa ho imparato**
- **Il dichiarato invecchia dove cambiano i nomi.** Cinque scostamenti su dieci sono etichette di bottoni che ho ritoccato negli ultimi giorni senza tornare sulla GUIDA. Nessuno di quei cinque è un difetto del programma, e tutti e cinque farebbero cercare all'utente una cosa che non c'è.
- **Una guida può essere fedele parola per parola e contenere l'unica promessa che il programma non mantiene.** Di WebView2 la GUIDA diceva la verità sulla parte difficile — «non muore, degrada con garbo» — e sbagliava sulla parte facile: *quando* te lo dice e con quale link.
- **Un capitolo che dichiara aperto un debito già chiuso è il difetto simmetrico della regola 16.** Lì il rischio è dimenticare quello che si era promesso; qui è credere di dovere ancora una cosa già pagata, e magari rifarla.
- **Un flusso mai costruito non produce nessun rosso.** Il bottone spento col tooltip che prometteva «arriva più avanti» stava lì da mesi, verde in ogni senso: nessun collaudo può fallire su una funzione che non c'è. La sola cosa che l'ha trovato è stata la rilettura di quello che il progetto **aveva promesso**.

**Dove ho faticato**
- **La decisione sul flusso D non era tecnica, ed è quella che è costata.** Non c'era un difetto da riparare: c'era da scegliere fra costruire una funzione intera adesso e ammettere per iscritto che la 1.0 non ce l'ha. La voce era già a registro dal 31 agosto — «o si fa o si dichiara fuori dalla 1.0» — e questa era l'occasione per non rimandarla ancora.

**Cosa ho deciso e perché**
- **Il flusso D è fuori dalla 1.0**, dichiarato nel cap. 12 invece che lasciato in sospeso. Costruirlo ora significava aprire una funzione grossa dentro una revisione di finalizzazione, cioè esattamente il modo per non finire mai; lasciarlo come promessa significava tenere in vita una riga di progetto che il codice smentisce da mesi. La ritiro, e resta scritta come cosa che potrà tornare.
- **V1 si chiude col codice, non solo con la GUIDA.** Correggere solo la guida sarebbe stato più economico e avrebbe abbassato la promessa invece di mantenerla: il link a Microsoft costa due stringhe.
- **Il conteggio dei collaudi nel README si aggiorna col rito**, non a mano in corsa: è uno dei numeri che il rito «aggiorna-tutto» esiste apposta per tenere onesti.

> 💡 **Il rilievo più pesante della fase non era un difetto: era una promessa che nessun collaudo poteva contraddire.** Il banco è verde su tutto quello che c'è, e non ha nessun modo di accorgersi di quello che manca — il flusso D non ha un collaudo rosso, ha un capitolo che dice di sì e un bottone spento che dice di no, e finché nessuno mette i due testi accanto la contraddizione non esiste da nessuna parte. È la regola 16 vista da un'altra angolatura: **il metodo sorveglia quello che ho fatto, e quello che ho solo promesso lo sorveglia soltanto chi va a rileggere le promesse.**

### Step 2.61 — Otto rifiniture dettate davanti allo schermo, due fix finali, e la pull request che aspetta la mia mano

*Chiuse le tre fasi, ci siamo seduti davanti all'applicazione vera e il tutor ha dettato otto rifiniture guardandola, non leggendola — le cose che si vedono solo aprendo il programma. Poi sono arrivati i due fix finali, e uno dei due ha avuto la sorpresa della giornata: il meccanismo che sospettavo non esisteva, ma nello stesso identico percorso c'era un difetto vero che non stavo cercando. Il banco chiude a 1394 e la pull request è aperta in bozza: il merge lo faccio io.*

**Cosa ho fatto**
- **Le otto rifiniture, in un giro solo** (trentacinque file, banco verde a 1390, ventidue collaudi nuovi tutti falsificati): il logo senza box né fondo proprio, che ora segue il colore del pannello sotto; la scritta del menu che scala con la finestra e non si sovrappone mai a nome e sottotitolo; i bottoni su una **scala di misure fisse** (110/130/190/240/300, più 40 per l'icona), coi gradini presi dalle ventiquattro larghezze ad-hoc che c'erano prima; `FlatStyle.Standard` ovunque, con i bordi ormai inerti tolti da `StileApp`; lo splash portato a **10 secondi**, che si chiude subito con un clic o con Invio; un bottone «?» in coda alla barra che riapre l'informativa, con dentro i **Credits**; l'apertura a **1920×1024 centrata** invece che massimizzata; e le Impostazioni **a due colonne**, due terzi di testo e un terzo di comandi, in frazione e non in pixel, così scala col DPI.
- **Il primo fix finale: la scala della scritta del menu dopo un ridimensionamento seguito da «massimizza».** Ho fatto misurare invece di credere — geometria e inchiostro davvero dipinto — e la memoria della misura che sospettavo **non c'era**. Ma nello stesso percorso i fermi del clamp erano scritti in unità di progetto e confrontati coi pixel dello schermo: a 150 % la scritta smetteva del tutto di seguire la finestra. Curato passando il DPI alle funzioni pure, che il banco ora interroga a 144 punti; a 96 DPI non cambia un pixel.
- **Il secondo: lo stemma non invita più a un clic.** Via il gestore, la manina e il tooltip; «Informazioni su…» adesso vive nelle **Impostazioni**, sotto «Come funziona…», con la diagnostica e i modelli traslocati lì insieme e un punto unico che la apre.
- **Verifiche prima del push**, chieste dalla fase di sicurezza e fatte davvero: nessuna firma estranea nei messaggi della giornata (regola 12), nessuna chiave nei diff. Banco **1394 verdi, 0 rossi**, push del ramo, pull request aperta **in bozza**.

**Cosa ho imparato**
- **Misurare invece di supporre non serve solo a confermare: serve a trovare l'altro difetto.** Se avessi «curato» la memoria della misura che credevo ci fosse, avrei toccato un meccanismo inesistente e lasciato dentro quello vero — che per giunta si vede solo a 150 %, cioè su una scala che io non uso.
- **Un collaudo può collaudare la propria ricostruzione dell'oggetto invece dell'oggetto.** La prima stesura della prova su «Informazioni su…» costruiva la finestra da sé e restava verde qualunque cosa facesse il programma. Da quel verde inutile è nato il punto unico che oggi apre la finestra: il collaudo, riscritto, adesso guarda la consegna vera.
- **E può restare verde perché non passa mai dal caso rotto.** La prova della tela del menu non dipingeva mai alla misura piccola: con la tela rotta, verde lo stesso. Due collaudi su due, alla prima stesura, verdi per il motivo sbagliato — nella stessa serata.
- **Lo splash a 5 secondi l'avevo difeso la mattina e la sera l'ho portato a 10.** Non è un'incoerenza: la mattina la domanda era «va tolto perché il caricamento dura 300 millisecondi?», e la risposta è no, quella schermata la voglio; la sera la domanda era un'altra, cioè quanto deve durare per chi la guarda e come esce chi non la vuole. Con clic e Invio, dieci secondi non trattengono nessuno.

**Dove ho faticato**
- **Le due riserve della fase 2 sono ancora lì**, e le tengo scritte: la prova a 150 % (vuole una macchina a quella scala) e i flussi AI dal vivo (vogliono una chiave che qui non c'è, e con essa i due numeri dell'indicatore d'attesa da giudicare a video).
- **La geometria del menu P0 non scala col DPI** — i 420×53 dei bottoni, le distanze, il corpo del testo. È la specie delle «~83 somme» che mi porto dietro da tempo, ed è la prima volta che la vedo in un punto preciso invece che come nota generale.
- **Lo strumento di collaudo ha una cecità nuova**: davanti alla **sola** informativa modale del primo avvio, quando la finestra principale non esiste ancora, risponde «non ha una finestra aperta» mentre il sistema la vede benissimo. Aggirata con uno script di ripiego e scritta nel README dello strumento, dove va il sapere pagato sul campo.

**Cosa ho deciso e perché**
- **Una porta sola per «Informazioni su…».** Prima ci si arrivava cliccando lo stemma — un invito che nessuna etichetta annunciava — e la finestra aveva anche una strada che la apriva senza il foglietto della diagnostica. Adesso è una voce nelle Impostazioni, accanto a «Come funziona…», e la GUIDA può finalmente dire **dove** sta.
- **La finestra si apre a misura, non massimizzata.** A 150 % apre più grande in pixel, perché il tetto è in unità di progetto convertite col DPI: contiene lo stesso disegno, che è quello che volevo.
- **La pull request resta in bozza finché il rito «aggiorna-tutto» non è finito**, e il merge su `main` lo faccio io con «Create a merge commit» (regola 11): ventitré commit di una giornata sola meritano di restare leggibili uno per uno nella storia, non schiacciati in un commit unico che dica soltanto «revisione».

> 💡 **Due collaudi scritti la stessa sera, tutti e due verdi al primo colpo e tutti e due ciechi, e per due motivi diversi.** Uno guardava un oggetto che si era costruito da solo, l'altro non passava mai dal caso in cui il difetto vive. Il punto non è che erano scritti male: è che **il verde di un collaudo appena scritto non è un'informazione**. Diventa informazione solo dopo che gli hai rotto il codice sotto e l'hai visto diventare rosso — e in tutti e due i casi, quel rosso mancato ha migliorato il programma e non solo la prova: da uno è nato il punto unico di «Informazioni su…», dall'altro il conto della scala fatto in pixel veri.

### Step 2.62 — Il lanciatore cieco dalla nascita, e le lucine che dicono se il profilo è quello di oggi

*Volevo solo aprire l'applicazione e guardarla. Il lanciatore scritto stamattina mi ha detto due bugie opposte nel giro di mezz'ora — «non è partita» mentre si stava aprendo, e di nuovo mentre era aperta da minuti — e la diagnosi che avevo in mano spiegava benissimo tutti e due i fatti ed era completamente sbagliata. Poi sono arrivate le due cose che volevo davvero: le lucine che dicono, riga per riga e documento per documento, se quel che si sta guardando è nato dal profilo di oggi; e lo splash che se ne va se clicco dove sto guardando, non solo se lo colpisco in mezzo. Banco a 1413, otto falsificazioni tutte viste rosse.*

**Cosa ho fatto**
- **Curato il lanciatore, dopo aver buttato via la prima diagnosi.** `strumenti\apri-app.bat` non aveva mai funzionato: ogni controllo passava da un comando PowerShell scritto con `^|`, e **dentro le virgolette di `-Command` il `^` non è un escape** — cmd non lo consuma, PowerShell lo riceve tale e quale e risponde «Impossibile trovare un parametro posizionale che accetta l'argomento '^'». Un `2>nul` in coda alla riga si mangiava quell'errore, il comando tornava vuoto, e il vuoto veniva letto come «nessuna finestra». Adesso i due controlli sono scritti senza pipe con `.Where({…})`, l'errore non è più nascosto, e il lanciatore dichiara alla fine **quanti secondi** ha aspettato.
- **La spia del profilo, in quattro posti** (cap. 03.8). Una lucina con la sua parola accanto: 🟢 «profilo allineato», 🔴 «profilo disallineato», e **spenta** quando non c'è niente da giudicare. Sta nella coda della Home come colonna nuova subito dopo il Match, accanto alle stelle in P4, in cima a **ciascuna** delle due colonne dei documenti in P6, e sopra il 📄 CV base in P2. La domanda non è stata riscritta quattro volte: la fanno tutte a `ArchivioProfilo.CambiatoDopo` attraverso un posto unico, `Ui/SpiaDelProfilo.vb`, che aggiunge solo il come si mostra.
- **Il registro ha imparato la firma del profilo.** La coda della Home non apre le candidature: legge il registro, e lì la versione di profilo non c'era. Era l'unica schermata del programma in cui una candidatura disallineata non lo diceva — proprio quella dove si guardano tutte insieme.
- **Lo splash si chiude con un clic ovunque nell'applicazione**, non solo su di sé: stessa strada dell'Invio, il filtro dei messaggi, che vede tutto il ciclo invece dei soli controlli propri.
- **Tredici collaudi nuovi e otto falsificazioni**, tutte viste rosse e scritte in `falsificazioni.md`. Banco a **1413 verdi, 0 rossi**.

**Cosa ho imparato**
- **Una diagnosi può spiegare tutti i fatti osservati ed essere sbagliata da capo a fondo.** Il tetto d'attesa di 40 secondi spiegava benissimo entrambe le bugie: l'eseguibile autonomo da 120 MB era stato appena riscritto, si riestraeva da zero con l'antivirus sopra, e quaranta secondi potevano non bastare. Era plausibile, era coerente, e curarla avrebbe cambiato un numero che non c'entrava niente lasciando il difetto dov'era — con l'aria di averlo risolto. A smentirla non è stato un ragionamento migliore: è stato **togliere il `2>nul`** e leggere l'errore che c'era sotto da sempre. La misura vera, che adesso il lanciatore stampa da sé, dice **due secondi**.
- **Il rosso che manca è una notizia quanto quello di troppo.** Una falsificazione doveva far cadere due collaudi e ne ha fatto cadere uno solo. L'altro — quello che sorveglia la spia spenta nella coda — era verde per il motivo sbagliato: la candidatura di prova era mai confrontata **e** senza versione annotata, due ragioni per restare spenta, e a tenerla spenta bastava la seconda. Cancellando il controllo sulle stelle restava verde. Adesso quella candidatura una versione ce l'ha, ed è per giunta una che non esiste: se il controllo cadesse, la spia non tornerebbe grigia per caso ma **rossa** su una candidatura mai giudicata.
- **Un difetto che sopravvive perché la risposta sbagliata è quasi sempre quella giusta.** Il controllo «c'è già una finestra aperta?» rispondeva sempre «no». È anche la risposta corretta la gran parte delle volte — di solito l'applicazione non è aperta — ed è per questo che una cecità del genere può campare indisturbata. Le bugie si notano solo nei pochi casi in cui la verità era l'altra.

**Dove ho faticato**
- **Da WSL `cmd.exe /c apri-app.bat` non torna** finché l'applicazione resta aperta: l'interoperabilità aspetta anche i discendenti, e un discendente quel lanciatore lo lascia per mestiere. Cinque minuti di attesa a vuoto con il lavoro già finito, prima di capire che non stava lavorando: stava solo respirando al posto di un processo vivo.
- **Il disegno delle spie non lo vede il banco.** Misura testo e colore delle celle, non dove finiscono i pixel; e su un pannello mai appeso a un Form `.Visible` risponde male comunque. Tre riserve scritte in `in_sospeso.md` invece di dirle provate.
- **Aprire l'app su una cartella dati nuova fa comparire prima l'informativa**, e finché quella è a video la finestra principale non esiste: il lanciatore direbbe «non è partita» mentre a video c'è eccome qualcosa. È la stessa cecità che lo strumento di collaudo aveva mostrato allo Step 2.61 — due attrezzi diversi, lo stesso inciampo, e la stessa causa: `MainWindowHandle` è la finestra **principale**, che non è sempre la prima.

**Cosa ho deciso e perché**
- **Due colori, non tre.** Sotto, i casi restano due e pesano diversamente — il profilo **cresciuto** (stessa persona, dati di ieri) e quello **rifatto da capo** (documenti di un'altra persona) — e il codice li sa già distinguere. Ma chi guarda deve capire in un decimo di secondo se può fidarsi, e un semaforo con due gialli diversi non è più veloce di un cartello. La differenza non si perde: si sposta nel suggerimento, che è dove uno la va a cercare quando gli serve.
- **La spia spenta è il terzo stato, ed è il più importante.** Una candidatura mai confrontata non ha versione, e alla domanda «il profilo è cambiato dopo?» una versione vuota risponde di no — che tradotto in lucine sarebbe un **verde**. Un verde lì direbbe «allineata» di un confronto che non è mai stato fatto: di un dubbio non si fa un allarme, ma nemmeno una promessa.
- **Il rosso della spia non è il rosso dei badge.** Misurato sul fondo avorio delle pagine: `Pericolo` vale 4,35:1 e non arriva alla soglia di 4,5; `RossoCritico` vale 7,07:1. La spia più importante è proprio quella che deve leggersi, e il numero l'ho preso prima di scegliere, non dopo.
- **Due spie in P6 e non una.** Il 🎯 CV mirato e la lettera nascono dallo stesso confronto e portano la stessa firma: una spia sola direbbe la stessa identica cosa a metà del costo. Ma chi sta per esportare la lettera guarda la colonna della lettera, e un avviso sull'altra metà dello schermo è un avviso che non c'è — è lo stesso difetto che il 2 settembre aveva fatto nascere la finestra di P6, scoperto proprio lì.

> 💡 **Il vero costo di una diagnosi plausibile non è il tempo che perdi: è il difetto che ti lascia dentro.** Se avessi alzato il tetto da 40 a 180 secondi, il lanciatore avrebbe smesso di sbagliare **quasi sempre** — l'applicazione ci mette due secondi, e con tre minuti di pazienza qualunque falso negativo sparisce dietro un'attesa che non scade mai. Il difetto sarebbe rimasto lì, invisibile, in un attrezzo che esiste per non farmi credere alle cose: cieco come prima, ma con l'alibi di una cura recente. Quel che l'ha smascherato non è stato guardare meglio il codice. È stato **smettere di nascondere gli errori**: un `2>nul` scritto per non sporcare lo schermo teneva sotto silenzio, da sempre, l'unica riga che diceva la verità.

### Step 2.63 — Tre cose che si vedono solo guardando: due colonne, due parole e un bottone che non c'era

*Avevo appena finito le spie e ho aperto il programma per guardarle. In mezz'ora sono venute fuori tre cose che nessuno dei 1413 collaudi poteva dirmi, e la terza è la più istruttiva: un bottone scritto bene, collaudato da cinque prove verdi, che a video **non compariva mai**. Le prove guardavano cosa c'era scritto sopra; nessuna guardava se si vedesse.*

**Cosa ho fatto**
- **Ho scambiato «Match» e «Profilo» nella coda della Home.** La spia adesso apre la riga e il punteggio la segue: le due colonne si leggono insieme — è del match che la spia parla — e in questo verso si incontra prima la qualifica e poi il numero qualificato. È venuto dietro un guadagno che non cercavo: Windows tiene la **prima** colonna di una lista sempre allineata a sinistra, e per quel limite il punteggio non aveva mai potuto incolonnarsi a destra come la data; adesso quel posto è della spia, che è testo, e il limite non morde più nessuno.
- **Ho cambiato le parole della spia**: 🟢 «profilo usato: corrente» e 🔴 «profilo usato: obsoleto», al posto di «profilo allineato» e «profilo disallineato». Dicono la stessa cosa, ma «allineato» chiede a chi legge di indovinare *a che cosa* — e su una riga di tabella, di fianco a un punteggio, quella domanda non si fa.
- **Ho dato al riconfronto un bottone suo**, «⚠ Riconfronta», in fondo al pannello della candidatura, che c'è solo quando serve — la stessa regola di «⚠ Rigenera la lettera» in P6. Il quarto mestiere di «Analizza» l'ho tolto.
- **Ho curato due difetti che lo scambio ha portato a galla**: le costanti delle colonne, rimaste ai numeri di prima; e il commento XML di `ProfiloDisallineato`, che nel giro precedente era finito sopra la funzione sbagliata.
- **Banco a 1414 verdi**, con un collaudo nuovo sull'ordinamento della colonna «Profilo» e **cinque falsificazioni**, tutte viste rosse.

**Cosa ho imparato**
- **Un controllo invisibile risponde a tutte le domande tranne quella giusta.** Il riconfronto era nato come quarto mestiere di «Analizza», e cinque collaudi verdi guardavano `btnAnalizza.Text`. Solo che «Analizza» vive nella fascia d'ingresso, e una candidatura già confrontata — cioè **l'unica** su cui il riconfronto esista — si riapre con quella fascia chiusa. Il bottone non c'era mai. La `.Text` di un controllo dentro un pannello nascosto si legge benissimo: risponde «Riconfronta» con la stessa faccia sicura che avrebbe se qualcuno potesse vederlo. Il collaudo giusto è di una parola sola — `.Visible` — e adesso ce n'è anche uno che dice l'opposto: `btnAnalizza` **non** si vede, ed è per questo che il gesto non poteva stare lì.
- **Un difetto può essere invisibile perché il collaudo lo condivide.** Aggiungendo la colonna «Profilo» in seconda posizione, le costanti che dicono quale colonna è quale erano rimaste ai numeri di prima: cliccare «Azienda» ordinava per ruolo, «Ruolo» per stato, «Da dove» per data. Nessuno se n'era accorto perché il collaudo dell'ordinamento passa gli indici **a mano** — `OrdinaPer(1)` — e scriveva 1 intendendo «azienda» esattamente come il codice. Erano sbagliati nello stesso modo, e due errori uguali si annullano in un verde.
- **Una promessa scritta nel codice non è una promessa mantenuta.** La finestra che propone il riconfronto diceva, testualmente: «se preferisci farlo più tardi, il bottone Riconfronta resta dov'è». Non c'era. E la frase l'aveva scritta io il giorno prima, convinta che fosse vera — la stessa convinzione che aveva fatto scrivere il collaudo che guardava `.Text`.

**Dove ho faticato**
- **Le parole nuove sono più lunghe di tre caratteri**, e tre caratteri hanno mosso due misure: la spia di P4 da 172 a 230 px e la colonna della Home da 170 a 190. Lo spazio in P4 l'ho preso alla riga dello stato della candidatura, che è allineata a destra e quei pixel li lasciava vuoti. Sono numeri decisi al conto: **vanno riguardati a video**, ed è scritto in `in_sospeso.md`.
- **L'ordinamento della colonna nuova chiede il disco.** La spia interroga lo storico del profilo, e un `Sort` che glielo chiedesse a ogni confronto lo rileggerebbe n·log n volte per rispondere sempre lo stesso. Gli stati si calcolano una volta prima di ordinare, e la tabella si butta subito dopo — tenerla viva vorrebbe dire mostrare la risposta di prima al ridisegno dopo.

**Cosa ho deciso e perché**
- **Il bottone c'è solo quando serve, non sempre.** Era la domanda vera («in questo caso o sempre?»): un «Riconfronta» sempre acceso inviterebbe a ripagare una risposta che non cambierebbe — a parità di profilo e di annuncio il confronto è lo stesso. Il gesto compare quando ha una ragione, e la ragione si legge nel suo suggerimento.
- **Fa anche da «Annulla» della propria attesa**, e non è un di più: l'annulla stava su «Analizza», che in quel momento non è a video. Un lavoro che si può chiedere e non fermare è un'attesa senza uscita — lo stesso difetto, un piano più sotto.
- **Il Match resta allineato a sinistra per ora.** Adesso *potrebbe* andare a destra, ed è quel che il progetto voleva da sempre. Ma è una misura che si decide guardandola, non deducendola: la lascio dov'è finché non l'ho vista.

> 💡 **Il metodo sa distinguere un collaudo che manca da uno che c'è; non sa distinguere un collaudo che c'è da uno che guarda dalla parte sbagliata.** Cinque prove verdi sul riconfronto, scritte apposta per sorvegliarlo, non hanno mai fatto la sola domanda che contava — «si vede?» — e per un giorno intero hanno certificato un gesto che nessun utente poteva compiere. A trovarlo non è stato il banco e nemmeno una rilettura del codice: è stato aprire il programma, cambiare la patente, cliccare «Annulla» e restare senza strada. È la stessa lezione della regola 15, vista dall'altro capo: il collaudo di tappa non serve a ratificare quel che sai di aver verificato — serve a scoprire che una cosa che *credevi* verificata non lo era.

### Step 2.64 — Prima si rifà il match, poi si scrivono i documenti

*Una regola che avevo scritto io, otto giorni fa, con tanto di paragrafo che spiegava perché era giusta. Oggi Mirco l'ha guardata dall'altro capo e l'ha rovesciata in una frase: «se modifico il profilo non devo poter rigenerare CV e lettera, devo prima rifare il match». Aveva ragione, e il paragrafo che la difendeva guardava la cosa sbagliata.*

**Cosa ho fatto**
- **Ho allargato il cancello** che impediva di riscrivere i documenti di una candidatura: guardava se il profilo di allora fosse **sparito**, adesso guarda anche se sia soltanto **cambiato**. È un posto solo — `MotivoProfiloNonPiuQuello` — con quattro porte che ci passano: la generazione che parte da sola aprendo una candidatura senza documenti, «Rigenera», «⚠ Rigenera la lettera» e il cambio di lingua.
- **Il messaggio manda dove sta il rimedio**: torna in «Confronta ANNUNCIO - CV» e premi «⚠ Riconfronta» — una chiamata sola, l'annuncio non si rilegge.
- **In P4 il bottone «Genera CV + lettera» lo dice prima**, nel suggerimento, invece di lasciar fare il viaggio di andata e ritorno. Resta premibile: di là si va anche solo a **guardare** i documenti già scritti, e i due export restano accesi — quel che è già scritto si può ancora mandare.
- **Ho rovesciato il collaudo che sorvegliava la regola vecchia** (`UnProfiloSoloCresciutoNonFermaLaRigenerazione` → `…FermaLaRigenerazione`) e ne ho aggiunti due: la lettera che non si riallinea, e le candidature **senza versione annotata**, che non si fermano affatto. **1416 verdi**, due falsificazioni nuove viste rosse — una che apre il cancello, una che lo chiude su tutti.

**Cosa ho imparato**
- **Un ragionamento può essere corretto e riguardare la cosa sbagliata.** Il paragrafo che difendeva la regola vecchia diceva: un profilo che cresce lascia documenti ancora spiegabili, perché i **fatti** restano quelli del profilo di oggi. È vero. Ma un CV mirato non è un elenco di fatti: è una scelta di **che cosa mettere in risalto**, e quella la fanno i **giudizi**, che sono di allora. Non era un errore di logica: era un errore su **quale** cosa stessi guardando.
- **Il danno silenzioso pesa più del fastidio rumoroso.** L'argomento contro il blocco era «sarebbe un avviso a ogni giro». Vero anche quello. Ma dall'altra parte c'è un CV scritto sulla misura sbagliata che, una volta esportato e spedito, **è indistinguibile da uno buono** — e le stelle almeno lo dicono, un documento no. Un fastidio che si vede vale meno di un danno che non si vede.
- **Rifiutare è più onesto che chiedere.** La strada alternativa era «rigenero sul profilo obsoleto: confermi?». Ma quella domanda mette l'utente a decidere di una cosa che **non può vedere** — la differenza fra i giudizi di ieri e quelli di oggi — e davanti a una domanda così la risposta comoda è sempre sì. Un rifiuto che indica il rimedio a un clic costa meno di un consenso dato al buio.

**Dove ho faticato**
- **Trovare il confine giusto.** Tre casi che sembrano uguali e non lo sono: la candidatura col profilo cambiato si ferma; il 📄 CV base **no**, perché nasce dal solo profilo e riscriverlo è proprio il modo di rimetterlo in pari; la candidatura **senza versione annotata** nemmeno, perché di un dubbio non si fa un divieto — e senza quel terzo controllo il cancello nuovo avrebbe chiuso la porta a tutto quel che esisteva prima. Ognuno dei tre ha adesso il suo collaudo.

**Cosa ho deciso e perché**
- **Il blocco, non l'avviso.** Mirco lasciava aperte le due strade («oppure vedi tu»). Ho scelto quella che chiude, per la ragione scritta sopra: un documento sbagliato non lo si riconosce dopo.
- **Il bottone di P4 resta premibile.** Bloccarlo avrebbe impedito anche di andare a **guardare** i documenti già scritti, che è metà del mestiere di quel pannello. Si dice prima, e si rifiuta dopo, ma solo a chi sta per scrivere.

> 💡 **La regola vecchia non era stata scritta di fretta: era stata scritta con una motivazione, ed è per questo che è durata.** Un difetto senza spiegazione lo trovi al primo giro; uno con accanto un paragrafo che spiega perché va bene si legge come una decisione presa, e chi passa di lì dopo lo rispetta. Il paragrafo era la sua difesa migliore. A scioglierla non è stato rileggerlo — l'avevo riletto oggi, tre volte, mentre lavoravo lì accanto: è stato qualcuno che invece di leggere la spiegazione ha guardato **la cosa**.

### Step 2.65 — Il pannello che cambiava argomento senza dirlo

*«Se non clicco Genera CV + lettera ma passo io manualmente alla sezione Documenti, mi fa vedere che posso cliccare rigenera. E se lo clicco funziona.» Ho passato mezz'ora a cercare la falla nel cancello che avevo appena chiuso. Non c'era: il cancello reggeva da tutte le porte. Quel che non reggeva era **quale candidatura** il pannello stesse mostrando.*

**Cosa ho fatto**
- **Ho chiesto al banco se la falla esistesse**, invece di immaginarla: due collaudi nuovi sui due cammini che saltano P4 — la voce «📄 Documenti» in barra e la tendina «Documento:». Dalla tendina il rifiuto teneva. Dalla barra il collaudo è caduto, ma per un motivo diverso da quello che cercavo: il pannello **non era andato affatto sulla candidatura**.
- **Il difetto vero**: `ApriQualcosaAsync` sceglie da sé cosa mostrare — tiene quel che aveva, altrimenti il 📄 CV base, altrimenti **l'ultima candidatura con documenti**. Chi stava guardando una candidatura e premeva «Documenti» poteva trovarsi davanti quelli di **un'altra**, e su quell'altra «Rigenera» funzionava a ragione.
- **Adesso la barra porta la candidatura che si ha in mano** (`FormPrincipale.btnDocumenti_Click` la chiede a P4), e la porta **senza generarla**: da lì si è chiesto di guardare, non di spendere una chiamata. Se i documenti non ci sono, il pannello lo dice e rimanda a «Genera CV + lettera».
- **E «Rigenera» col profilo obsoleto adesso è spento**, col perché nel suggerimento e nella riga di stato — che smette di dire «puoi rigenerarli» e dice invece dove si passa. **1419 verdi**, tre falsificazioni nuove tutte viste rosse.

**Cosa ho imparato**
- **Una segnalazione descrive quel che si vede, non quel che succede — e il salto fra le due è dove abita il difetto.** «Rigenera funziona» era vero. «Sulla candidatura col profilo obsoleto» era falso, e nessuno dei due lo sapeva: a video la candidatura era cambiata sotto gli occhi, e l'unico posto che lo diceva era il nome nella tendina, in cima, che chi guarda i documenti non guarda.
- **Il collaudo che cade per il motivo sbagliato vale quanto quello che cade per quello giusto.** Il mio primo collaudo verificava una falla che non esiste. È diventato rosso lo stesso, e il messaggio d'errore — `cv_base → cv_base` invece di `cv_mirato` — conteneva l'intera diagnosi: il pannello aveva rigenerato il CV base, cioè stava guardando **un'altra cosa**. Non l'avevo chiesto: me l'ha detto la differenza fra quel che mi aspettavo e quel che è successo.
- **Rifiutare dopo il clic non basta se il bottone sembra pronto.** Il cancello c'era e teneva, ma «mi fa vedere che posso cliccare rigenera» è una segnalazione su un'**aspettativa**, non su un permesso. Un bottone acceso è una promessa; mantenerla o spiegare perché no viene dopo — prima bisogna non averla fatta.

**Dove ho faticato**
- **Mezz'ora a cercare una falla che non c'era.** Rileggevo `RigeneraAsync`, `ApriDallaTendinaAsync`, il gestore del clic, convinta che da qualche parte ci fosse un cammino che scavalcava il controllo. Il codice era giusto: sbagliata era la **domanda**. A rimettermi in carreggiata non è stata un'altra rilettura, ma guardare i **dati sul disco** — le tre candidature della cartella di prova — e accorgermi che quella con i documenti era già stata rimessa in pari, mentre quella obsoleta di documenti non ne aveva affatto.

**Cosa ho deciso e perché**
- **Guardare non genera.** Dalla barra si passa `generaSeManca:=False`. Era già così di fatto per il CV base (che si ripesca da disco) e non per le candidature: premere una voce di menu non deve poter far partire una spesa.
- **La candidatura aperta ha la precedenza su qualunque euristica.** `ApriQualcosaAsync` resta, ma solo per chi arriva senza niente in mano: indovinare va bene quando non c'è una risposta giusta, non quando c'è.

> 💡 **Ho cercato per mezz'ora un buco nella serratura, e la porta era un'altra.** La segnalazione diceva «qui non funziona», io ho letto «qui il controllo si può aggirare», e ho passato il tempo a verificare la mia traduzione invece del fatto. Le due cose si somigliano abbastanza da confondersi e differiscono abbastanza da mandare a cercare dalla parte sbagliata. Il modo per uscirne non è stato pensare meglio: è stato **scrivere la mia ipotesi come collaudo** e lasciarla fallire — perché un'ipotesi sbagliata, se la scrivi in una forma che può essere smentita, ti dice anche da che parte guardare.

### Step 2.66 — Un campo per due domande, e la riga che tagliava sempre la risposta

*«Se riconfronto il match, nella sezione documenti ci sono gli stessi documenti di prima, ma la lucina è diventata verde. È sbagliato!» Aveva ragione, e la ragione stava in un posto che non avevo mai guardato: `versione_profilo` non era un campo, erano due — solo che fino al giorno prima nessuno poteva accorgersene.*

**Cosa ho fatto**
- **Ho separato le due versioni.** `VersioneProfilo` risponde a «con che profilo è stato fatto il **match**», e il riconfronto la aggiorna, giustamente. Le spie dei documenti però leggevano quella, e i documenti nessuno li aveva riscritti: adesso la candidatura annota anche `VersioneDeiDocumenti`, che è quel che le due lucine di P6 guardano. Le candidature già in archivio la **ereditano** dal confronto — fino a ieri erano la stessa cosa per costruzione — così nessuna cambia colore per il fatto che ho cambiato idea.
- **Ho misurato il secondo difetto invece di crederci sulla parola.** Avevo segnalato che anche le lucine dei documenti non si aggiornavano cambiando pagina dalla barra. Il codice diceva di no, e ho fatto provare: passando dalla barra, P6 le rifà davvero. Il pannello fermo era **P4**, dove `OnVisibleChanged` rinfrescava i comandi — ed è per questo che «⚠ Riconfronta» *compariva* — ma non la spia. Due segnali opposti nella stessa schermata, a due centimetri l'uno dall'altro.
- **Poi ho chiesto messaggi più corti, e guardando ho trovato di peggio.** Le righe di stato **troncano in silenzio**: l'etichetta è alta tre righe, il testo ne voleva sei, e a cadere fuori era sempre la coda — cioè «Vai in «Confronta ★ ANNUNCIO - CV» e premi «⚠ Riconfronta»», l'unica riga che diceva che fare. Restava a video «i giudizi sono quelli di allora», con «Rigenera» spento e nessuna spiegazione.
- **Sedici messaggi accorciati e messi in forma schematica** — la conferma del riconfronto da 575 caratteri a 300, il rifiuto dei documenti da 241 a 130, il lavoro in sospeso alla chiusura da filastrocca di «e» a elenco puntato. Le due conferme di eliminazione restano lunghe: lì l'elenco di cosa sparisce *è* il messaggio. **1425 verdi**, sei falsificazioni nuove, tutte viste rosse.

**Cosa ho imparato**
- **Un campo che risponde a due domande è giusto finché le due risposte coincidono per caso.** `versione_profilo` ha significato «il profilo del match» e «il profilo dei documenti» per un mese intero senza sbagliare mai, perché i documenti nascevano dal confronto e il confronto si faceva una volta sola. Il riconfronto non ha creato il difetto: ha tolto la coincidenza che lo teneva nascosto. E la documentazione del campo diceva già la verità sbagliata — «la versione da cui **i documenti** sono nati» — mentre il codice lo scriveva dopo il confronto: due mezze verità che insieme non ne fanno una.
- **Il programma lo diceva già, e nessuno lo ascoltava.** Dopo il riconfronto la riga di stato avvisava «restano dove sono, ma nascono dai giudizi di allora», mentre la lucina accanto diceva verde. Fra due segnali che si contraddicono, chi guarda crede a quello che costa meno leggere: il colore. Un avviso che ha addosso una lucina che lo smentisce non è mezzo avviso, è nessun avviso.
- **Un testo troppo lungo non è un difetto di stile: è un pezzo di programma che non arriva.** Ero partito chiedendo messaggi più corti per comodità di lettura. Il motivo vero era peggio: quel che eccede l'etichetta non va a capo da qualche parte, sparisce — e sparisce sempre la fine, cioè l'istruzione. Il collaudo che ora lo sorveglia non legge il testo, lo **misura** contro la dimensione vera dell'etichetta; falsificandolo dichiara quante righe cadono fuori (tre).

**Dove ho faticato**
- **Su una segnalazione mia che era mezza sbagliata.** Avevo visto le lucine ferme in P4 e avevo dedotto che lo fossero anche in P6, dove non le avevo guardate davvero. Il codice diceva il contrario, ma io avevo visto — e i dati battono il codice che si legge, non quello che si prova. La via d'uscita non è stata rileggere meglio: è stata accendere il server di collaudo, riaprire la candidatura di prova e **guardare le due lucine**, prima e dopo aver salvato il profilo. Sane. Il difetto era uno solo e stava nell'altro pannello.

**Cosa ho deciso e perché**
- **I documenti non si cancellano al riconfronto.** Era la mia prima idea — se i giudizi non valgono più, buttiamo anche i documenti — e ho scelto altro: quei documenti possono essere quelli **già spediti**, e sono l'unica traccia di cosa ho mandato; dentro ci sono i testi che ho riscritto a mano e le voci che ho tolto. Cancellarli su un gesto che riguarda le stelle sarebbe una perdita irreversibile decisa dal programma. Restano, con la lucina rossa che lo dice e «Rigenera» a un clic.
- **Alla riga di stato il testo che ci sta, alla finestra quello disteso.** Non è un riassunto per brevità: è il testo che **arriva**. Il perché per esteso vive dove c'è lo spazio per dirlo — la finestra, e il suggerimento della spia.

> 💡 **Il difetto che ho segnalato era uno, quelli veri erano tre — e il terzo l'ha trovato solo il guardare.** Il primo l'avevo visto (la lucina verde), il secondo l'avevo visto male (P4 al posto di P6), il terzo non l'aveva visto nessuno: un'istruzione scritta, collaudata e mai comparsa a video. Le tre cose stanno su una scala precisa. Il banco sa dirti se il testo è giusto; non sa dirti se **si legge**. La misura che ho aggiunto è un modo di far entrare quella domanda nel banco — ma è nata da un'occhiata, non da un'asserzione, e mi ricorda che il collaudo che manca è sempre quello di ciò che non sapevi si potesse rompere.
