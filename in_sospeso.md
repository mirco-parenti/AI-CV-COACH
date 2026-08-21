# In sospeso — AI-CV-COACH

Raccolta **unica** delle cose **già decise e dentro il perimetro** che sono rimaste
indietro: una tappa si è chiusa lo stesso, ma qualcosa aspetta il momento giusto, una
macchina che qui non c'è, o una mano che non è quella dell'assistente.

*Cos'è e cosa non è.* Qui non ci sono idee da valutare: quelle stanno in
`idee_future.md`, che è il backlog dei **raffinamenti futuri**. La differenza è netta —
una voce di `idee_future.md` potrebbe non farsi mai, una voce di questo file **va
fatta**, e se non è ancora fatta è un debito aperto. Lo **stato** del progetto non si
copia qui: sta nel `README.md` e nell'ultimo `### Step` del `diario_di_bordo.md`.

*Come si compila (modalità):* una riga per voce, raggruppata per **tappa d'origine**;
ogni voce dice **cosa manca · perché è rimasta indietro · dove ne parla il progetto**,
con la data in cui è emersa. Quando una voce è chiusa si sposta in fondo, in
**«Chiuse»**, con la data e come si è chiusa: così l'elenco attivo qui sopra resta solo
di cose da fare. Aggiornato con "aggiorna-tutto".

## Da T1 — lo scheletro (2026-08-06)

- **La prova dell'exe su un PC davvero pulito.** Il publish single-file è stato
  verificato sulla macchina di sviluppo, che ha l'SDK installato: non è la stessa cosa
  che un PC senza niente. È il collaudo dichiarato di T1 (cap. 14) e serve una seconda
  macchina, quindi tocca a Mirco. *Fino ad allora il vincolo più rigido del progetto è
  dimostrato al 90%, non al 100%.*
- **L'icona dell'eseguibile.** Oggi l'exe ha l'icona predefinita di Windows. Va prodotta,
  idealmente dallo stesso scudo Aviolab già incorporato nel sorgente. È una scelta di
  prodotto, non di codice. *(cap. 13.5; cap. 15, voce 4.)*
- **L'SDK .NET 10 sulla postazione del tutor.** Lì c'è ancora l'SDK 9: quella macchina
  oggi non compila il progetto. *(cap. 13.1; regola 11 — le due postazioni.)*
- **La cartella `VB.NET/src/pubblicazione/`** (111 MB, ignorata da git) è rimasta su
  disco dopo il publish di prova. Non dà fastidio a nessuno e non entra nel repo: si
  cancella quando lo spazio serve.

## Da T2 — il motore e il pool (2026-08-07)

*Chiusa il 2026-08-19: i tetti dei `max_token` sono stati misurati su un giro vero e sono
in «Chiuse».*

## Da T3 — il profilo (2026-08-07 · integrata alla chiusura, 2026-08-09)

- **Un `.docx` salvato davvero da Word.** La gamba A del collaudo di tappa ha provato le
  quattro porte d'ingresso, ma i file DOCX, TXT e MD sono stati **fabbricati** dal testo
  trascritto dal PDF: provano le *strade di lettura*, non l'impaginazione di Word, che su
  questa postazione non c'è. Serve una macchina che ce l'abbia. *(cap. 05.1; cap. 14, T3,
  gamba A — il limite è dichiarato anche nel collaudo.)*

## Da T4 — la pipeline di candidatura (2026-08-10 · integrata alla chiusura, 2026-08-11)

- **I documenti prodotti aperti in Word.** La gamba C del collaudo di tappa (cap. 14)
  chiede DOCX e PDF aperti **in Word e in LibreOffice**: LibreOffice c'è su questa
  postazione e ha detto la sua — apre entrambi, li riconverte, accenti e simboli intatti —
  ma Word qui non è installato. È il gemello in **scrittura** della voce di T3 qui sopra,
  che riguarda la **lettura** di un `.docx` salvato da Word: serve la stessa macchina, e
  conviene chiuderle insieme. *(cap. 05.7; cap. 14, T4 gamba C.)* **Riconfermata alla
  chiusura di T4** *(2026-08-11)*: il collaudo di tappa ha ripercorso la gamba per intero —
  114 campi su 114 ritrovati, DOCX e PDF identici — ma la metà di Word è di nuovo rimasta
  fuori, per la stessa ragione.

## Da T5 — la ricerca annunci e il registro (2026-08-13, alla chiusura di T5c)

*Tutte chiuse a T6: l'export del registro e il filtro per stelle sono in «Chiuse».*

## Da T5d — il profilo dalla propria pagina (2026-08-14)

*Chiusa il 2026-08-19: la pagina che non è LinkedIn è stata provata ed è in «Chiuse».*

## Da «Elimina profilo» (2026-08-14)

*Chiusa a T6: la cartella dati usa-e-getta è in «Chiuse».*

## Da T6 — le email di candidatura (2026-08-14, alla chiusura)

- **Lo stato «esito» e il follow-up** *(ridotta il 2026-08-18: delle tre cose, il
  destinatario è in «Chiuse»)*. Il cap. 07.3 assegnava a T6 tre cose oltre all'invio, e T6
  ne ha portata una sola. Restano: lo stato **`esito`** (in attesa · colloquio · rifiutata ·
  assunto 🎉), che nello schema c'è ma dall'interfaccia non si raggiunge; e il **promemoria
  di follow-up** per le candidature ferme da giorni. Sono due passi del racconto «a che
  punto sono», e vanno fatti entro la 1.0. Quel che si è imparato guardandoli da vicino:
  i quattro valori dell'esito **non esistono in codice**, sono una riga di commento in
  `StatoOpportunita.vb` — servono un tipo nuovo, un punto d'ingresso nell'interfaccia che
  non c'è mai stato, e una sotto-macchina a stati dentro uno stato oggi terminale. Il
  follow-up invece è **il più economico dei due e non dipende dall'altro**: le date stanno
  già in `DateStati`, non serve nessun campo nuovo, e finché l'esito non c'è «nessun esito
  registrato» coincide con «stato = inviata». Manca però una decisione che il progetto non
  ha mai preso: **dopo quanti giorni**. *(cap. 07.3; cap. 14, T9; cap. 15.3 — il promemoria
  è passivo, il testo del sollecito lo scrive l'utente.)*

## Da T7 — multilingua e qualità (2026-08-15, alla chiusura di T7a)

- **La modifica a mano dei testi in P6** (2026-08-18, alla chiusura di T7b). Il cap. 8.4
  promette che davanti al prima/dopo l'utente possa «accettare, modificare a mano o
  tornare alla versione non rifinita»: il prima/dopo c'è, la modifica a mano no — le tre
  caselle di P6 sono `ReadOnly`, e renderle scrivibili vuol dire decidere che cosa
  succede al documento salvato, agli export e alla rigenerazione. È **disegno di P6**, non
  anti-slop, e apparteneva a T7b solo per vicinanza. *(cap. 08.4; cap. 08.6; cap. 03.6.)*
- **L'interruttore della rifinitura** (2026-08-18). Il cap. 8.4 la dà per disattivabile
  dalle Impostazioni: quelle sono il pannello **P8, di T9**, e fino ad allora la
  rifinitura è sempre attiva. Non è un buco pericoloso — la via d'uscita è il prima/dopo,
  e una rifinitura che fallisce lascia comunque il testo grezzo — ma è una promessa del
  capitolo non ancora mantenuta. *(cap. 08.4; cap. 03, tabella dei pannelli, P8.)*
- **Il prima/dopo dell'email in P7** (2026-08-18). Il corpo del messaggio passa
  dall'anti-slop come gli altri testi, ma com'era prima non si conserva e non si mostra:
  la casella del confronto vive in P6, e in P7 l'utente ha davanti una casella che può già
  riscrivere. Se un giorno servirà, il posto è la bozza (`email.json`). *(cap. 07.1;
  cap. 08.6.)*

## Da T7d — il 📄 CV-1 base riletto e in inglese (2026-08-18, alla chiusura)

*Chiusa lo stesso giorno: il cambio di lingua che fallisce a metà è stato percorso dal vivo
ed è in «Chiuse», insieme alla correzione della ricetta con cui si prova.*

## Da questa passata sulle voci in sospeso (2026-08-18)

*Chiusa il 2026-08-19: il collaudo che cedeva sotto carico è in «Chiuse», e il difetto
vero era un altro.*

## Da T8a — il guscio del server MCP (2026-08-19, alla chiusura)

- **La versione moderna del protocollo accettata è una sola.** Il server parla
  `2026-07-28` e, per l'era dell'handshake, le quattro revisioni note fino a
  `2025-11-25`. Quando ne uscirà una moderna nuova, un client che la chieda si sentirà
  rispondere «non la parlo» con l'elenco di quelle buone — che è il comportamento giusto e
  previsto dalla spec, ma è anche una data di scadenza da guardare, non un problema
  risolto. *(cap. 09.2; `ProtocolloMcp.VersioniSupportate`.)*

## Da T8b — i tool che passano dall'AI (2026-08-19, alla chiusura)

*Chiuse il 2026-08-21 dal collaudo di tappa: il confronto e la generazione messi
accanto a quelli della finestra, e il diario dei consumi, sono in «Chiuse».*

## Da T8c — i tool che scrivono e il lucchetto (2026-08-19, alla chiusura)

*Chiusa il 2026-08-21 con T9a: il tool `esporta_backup` è nato insieme alla funzione che
espone, ed è in «Chiuse».*

## Da questa passata sui debiti prima di T9 (2026-08-19)

*Le voci qui sotto sono nate guardando l'applicazione mentre si chiudevano altre cose: non
erano in nessun piano, e nessuna è grave.*

- **La traccia del dialogo reale non ha più nessuna trappola che sfugga.** Curato il «corso
  senza nome» (Pool 1.12), tutte e quattro le trappole della traccia vengono instradate ogni
  volta — ed è il rapporto stesso a dire che in quel caso «la traccia va rifatta»: un
  collaudo che passa sempre smette di misurare. Serve una trappola nuova, più difficile di
  quelle di adesso, scritta apposta per il modello di oggi. *(cap. 14, T3;
  `CollaudiDialogoReale`; `casi/reale/dialogo_guidato.md`.)*
- **Nel brainstorming il markdown si vede grezzo.** L'AI risponde con `**grassetto**` e
  `## titoli`, e la chat di P5 mostra gli asterischi e i cancelletti così come sono. Non
  rompe niente e il testo si capisce, ma è l'unico posto del programma in cui si vede la
  scrittura di una macchina invece di un testo: gli appunti di mira che ne escono, invece,
  sono già puliti. O si rende la bolla capace di leggerlo, o si dice al prompt di non
  scriverlo. *(cap. 03.5; `PannelloDialogo`.)*
- **La finestra a misura ridotta è da guardare col mouse.** Forzandola via API a 1366×768 —
  sopra la `MinimumSize`, che è 1150×600 — il contenuto risulta tagliato e la barra di
  navigazione sparisce. Ma il layout è tutto `Dock.Fill`, e ridimensionare via API una
  finestra **massimizzata** è manovra che inganna (il README dello strumento ne elenca già
  di simili): finché non si trascina il bordo con il mouse, non è un difetto, è
  un'osservazione. Se si conferma, è roba della rifinitura di T9. *(cap. 03.4;
  `FormPrincipale.Designer`.)*
- **Le parole si incollano fra un blocco e l'altro della pagina letta.** Nel testo che l'app
  ricava da una pagina web, la fine di un blocco e l'inizio del successivo finiscono
  attaccate: «Pubblica AmministrazioneDue suite specializzate». Sul sito provato il modello
  ha capito lo stesso, ma è fortuna, non progetto: basta un separatore fra i nodi di testo.
  *(cap. 06.7; il lettore di pagina di T5a.)*
- **Un 404 viene raccontato come un problema di rete.** Aprendo un indirizzo che il server
  non ha, la fascia dice «La pagina non si è caricata. Controlla il collegamento a Internet»
  — mentre il collegamento c'era e il server ha risposto benissimo, dicendo che quella
  pagina non esiste. È il tipo di messaggio che manda l'utente a controllare il modem invece
  che l'indirizzo. *(cap. 06.6; rientra nella revisione della gestione errori, T9.)*

## Dal collaudo di tappa di T8 — il gruppo C (2026-08-21)

- **Lo stato «nuova» è un vicolo cieco nella finestra.** Un'opportunità ferma al solo
  annuncio — che oggi sa creare **solo il server MCP**, perché l'applicazione archivia
  soltanto dopo il confronto — la finestra la riapre ma non la può proseguire:
  `RiapriLaCandidatura` svuota la casella dell'incolla, e «Analizza» si accende solo se lì
  dentro c'è del testo. Il **testo grezzo** dell'annuncio non lo conserva nessuno, né il
  server né l'app: i due `annuncio.json` hanno le stesse identiche chiavi. Il motore
  saprebbe farlo — `ConfrontaAsync` vuole l'annuncio **già strutturato**, non il testo —
  quindi manca solo il gesto nell'interfaccia: un'azione «Confronta» sulla candidatura
  riaperta, oppure il testo grezzo salvato in `annuncio.json`. La Home conta già quello
  stato fra le candidature «da completare», e la macchina degli stati prevede
  `nuova → interessante`: il disegno se lo aspetta, l'interfaccia non lo serve.
  *(cap. 03.6; cap. 07.3; `PannelloOpportunita.vb`; rientra nella revisione dei pannelli
  di T9.)*
- **Il confronto ogni tanto salta i giudizi di «contesto».** Il prompt li pretende per
  scritto — «un giudizio per OGNI campo di contesto presente… non saltarne nessuno» — ma in
  **un giro su quattro** i cinque giudizi di contesto (titolo, sede, contratto, mansioni,
  benefit) non sono stati emessi. Non è un'asimmetria fra le due porte: è successo su una
  chiamata del server, mentre gli altri tre giri — server e finestra — li hanno emessi
  tutti. Siccome il contesto pesa un quinto del nucleo, la sua assenza sposta il punteggio
  di base. È la stessa famiglia del «corso senza nome»: prima si **misura** su più giri,
  poi si decide se curare il prompt — uno su quattro non è ancora un dato.
  *(cap. 04.7; `prompt-pool/confronto/confronto.md`.)*

## Da T9a — backup e ripristino (2026-08-21, alla chiusura)

- **Il tredicesimo tool visto da un client MCP vero.** `esporta_backup` è collaudato dal
  banco (cinque prove, lucchetto compreso) e la funzione che espone è stata percorsa dal
  vivo nella finestra, ma da un client vero non l'ha ancora chiamato nessuno: i server MCP
  si caricano **all'avvio** della sessione, e quella in cui il tool è nato aveva già
  caricato il server di prima. Serve un riavvio, non una modifica al codice. È mezz'ora,
  e la strada è quella già percorsa il 2026-08-21 con gli altri dodici. *(cap. 09.3;
  cap. 14, T9a; regola 15.)*

## Da revisione adversariale (2026-08-09)

- **Il pannello del logo a DPI alti.** Le costanti di geometria sono in pixel non scalati:
  a 125/150% di scala — l'impostazione di fabbrica di quasi tutti i portatili recenti — il
  disegno è fuori misura. Difetto vero, ma correggerlo alla cieca rischiava di rompere il
  layout **validato a video** in T3: serve uno schermo su cui verificare a 150%.
  *(cap. 03.5; segnalato dalla revisione, rimandato con motivo.)*

## Chiuse

- ✅ **Il tool `esporta_backup`** *(aperta il 2026-08-19 con la chiusura di T8c, chiusa il
  2026-08-21 con T9a)*. Aspettava la funzione che espone, ed è arrivato con lei: scrive un
  backup JSON nella cartella `backup\` e restituisce il percorso, con le **stesse due
  scelte** della finestra — il solo profilo, oppure tutto. Il ripristino resta fuori, ed è
  una scelta: sovrascrive roba dell'utente e passa dall'anteprima che dice cosa sostituisce
  cosa, quindi vive dove c'è l'utente a guardarla. I tool del server sono adesso **tredici**.
  *(cap. 09.3; cap. 11.4; diario, Step 2.38.)*
- ✅ **Il collaudo da un client MCP vero** *(aperta il 2026-08-19 con la chiusura di T8a,
  chiusa il 2026-08-21)*. Non serviva aspettare Claude Desktop: un client MCP vero era già
  in casa — **Claude Code**, registrato fra i suoi server e caricato al riavvio della
  sessione. `tools/list` ha mostrato i **dodici tool** con nomi e descrizioni sensate, e in
  più una superficie che il banco non poteva esercitare: le **istruzioni del server**, lette
  dal client e passate al modello. Da lì le letture sui dati veri — il profilo e le sette
  opportunità del registro. *(cap. 09.2; cap. 14, T8; diario, Step 2.37.)*
- ✅ **Un confronto e una generazione veri via MCP, accanto a quelli della finestra**
  *(aperta il 2026-08-19 con la chiusura di T8b, chiusa il 2026-08-21)*. Lo stesso testo
  grezzo fatto entrare dalle due porte: **0,9 stelle da entrambe**, e per aritmetiche
  diverse — 36 di base con 18 di stima dalla finestra, 37 con 15 dal server, che con
  `clamp_giu = -20` fa scattare il taglio dello scarto. Il conto torna a mano da tutti e due
  i lati, e viene da una funzione sola con tre chiamanti (`CalcoloMatch.Calcola`): la stessa
  regola vista lavorare su ingressi diversi dice più di due numeri identici. Il 🎯 CV-2 ha
  lo **scheletro dei fatti identico** — stesso tipo, stessa intestazione, otto ruoli con
  aziende e durate uguali carattere per carattere, sedici competenze, stessa formazione —
  e varia solo la prosa. Due giri del solo server differiscono fra loro quanto il server
  differisce dalla finestra: la varianza residua è del modello, non della porta.
  *(cap. 14, T8; cap. 09.3; diario, Step 2.37.)*
- ✅ **Il diario dei consumi via MCP, verificato eseguendo** *(aperta il 2026-08-19 con la
  chiusura di T8b, chiusa il 2026-08-21)*. `chiamate_ai.csv` non esisteva nella cartella
  dati vera: alla prima chiamata partita dal server è **nato**, con l'intestazione e una
  riga per chiamata. A fine collaudo quattordici righe — sei dal server, otto dalla
  finestra — tutte sotto il 50% del tetto e tutte chiuse con `end_turn`. In più ha finito
  per fare da metro: l'analisi dell'annuncio è uscita **identica al token** dalle due porte
  (2896 → 902, 11,3% del tetto), e le altre righe differiscono di uno solo.
  *(cap. 09.3; `ContestoApp.MontaAi`; diario, Step 2.37.)*
- ✅ **Il lucchetto visto da due processi veri** *(aperta il 2026-08-19 con la chiusura di
  T8c, chiusa il 2026-08-21)*. Con la finestra aperta, `salva_opportunita` è stato
  **rifiutato** senza andare in errore: «La cartella dati è in uso da un altro processo…
  Chiudi la finestra e riprova», con l'elenco di ciò che continua a funzionare lo stesso.
  Chiusa la finestra, la stessa identica chiamata è riuscita. Stavolta i processi erano
  davvero due — la finestra e il server MCP della sessione — e non due `FileStream` dentro
  lo stesso. Verificato anche il rovescio: i tool che passano dall'AI **non** vogliono il
  lucchetto, e il diario dei consumi si scrive lo stesso ad app aperta.
  *(cap. 09.4; `CollaudiLucchettoDati`; diario, Step 2.37.)*

- ✅ **I `max_token` di Sonnet 5, misurati invece che temuti** *(aperta il 2026-08-07 da T2,
  riscritta più volte, chiusa il 2026-08-19)*. Sonnet 5 conta i token in modo diverso e a
  parità di testo ne usa ~30% in più, mentre i tetti del pool erano tarati su Sonnet 4.6: i
  tre più stretti — `email_candidatura` e `umanizzazione_sintesi` a 1500, `umanizzazione_frasi`
  a 2500, tutti sul livello ragionamento — erano i sorvegliati speciali. Il diario dei consumi
  esisteva apposta per rispondere, e mancava solo il giro d'uso vero che lo riempisse. Fatto:
  un giro completo su cartella usa-e-getta, dall'import del CV all'email, **tredici chiamate**.
  I tre sospettati sono davvero i primi tre della classifica, ma il peggiore usa poco più di
  un quarto del suo tetto — `email_candidatura` **27,1%**, `umanizzazione_sintesi` **25,0%**,
  `umanizzazione_frasi` **18,2%** — e ogni riga finisce con `end_turn`: **nessun tetto va
  alzato**, nessun troncamento. Il limite del dato va detto: è un giro, con un CV e un
  annuncio; un CV molto più ricco allungherebbe sintesi e frasi, ma con un margine di quasi
  4× c'è spazio. *(cap. 02.5; cap. 04.4; `chiamate_ai.csv` nella cartella dati.)*
- ✅ **L'interruzione di un turno provata dal vivo** *(aperta il 2026-08-18 dal collaudo di
  T7c, chiusa il 2026-08-19)*. Il bottone c'era e il banco lo collaudava; quel che non era
  mai riuscito era premerlo **mentre l'AI scrive**, perché le risposte del ragionamento
  arrivano in pochi secondi. Riuscito al secondo tentativo, nel brainstorming: la ricetta è
  una domanda che chieda un elenco lungo (compra i secondi), e soprattutto `aspetta_che` e
  `clic` **nella stessa invocazione** — al primo tentativo, con le due chiamate separate, la
  risposta era già finita nella latenza in mezzo. Premuto a **3,9 secondi** dall'invio, e
  l'esito è quello promesso: la risposta si ferma a metà frase, il testo già arrivato
  **resta a video**, la bolla si marca «(interrotto)», i comandi si riaccendono, nessun
  errore. *(cap. 02.6; la trappola è scritta nel README di `strumenti/mcp-collaudi/`.)*
- ✅ **Il prima/dopo della rifinitura letto a video** *(aperta il 2026-08-18 dal collaudo di
  T7b, chiusa il 2026-08-19 con un limite dichiarato)*. Si legge bene: riga separatrice,
  intestazione «PRIMA DELLA RIFINITURA», il nome del campo col triangolino, «Prima:» e il
  testo. Nella colonna della lettera la sezione è interamente visibile a schermo pieno.
  Resta fuori una domanda: nella colonna del CV la sezione sta più in basso, e lo strumento
  di collaudo **non sa scorrere dentro una casella** — quindi «cosa succede quando i campi
  cambiati sono molti» non è stato visto. Non riapre la voce: il meccanismo e i dati erano
  già verificati sui JSON, e questa era la guardata alla forma. *(cap. 08.4; cap. 03.6.)*
- ✅ **Una pagina che non sia LinkedIn** *(aperta il 2026-08-14 da T5d, chiusa il
  2026-08-19)*. L'import legge «la pagina aperta» e il dubbio non era la strutturazione — che
  non sa da dove venga il testo — ma lo **scorrimento**, che su LinkedIn aveva dovuto
  cercarsi da solo il contenitore che scorre. Provata su un sito costruito in tutt'altro
  modo, una one-page moderna: lo scorrimento ha percorso la pagina **fino in fondo**, e il
  testo letto va dal menù in cima al piè di pagina con la versione del sito. La
  strutturazione ha ricavato quel poco che una pagina aziendale dice di una persona — nome,
  email, città, link, un ruolo — lasciando **vuoti** durata, tipo e telefono invece di
  inventarli. Ne è uscita una voce nuova, minore: le parole si incollano fra un blocco e
  l'altro. *(cap. 06.7.)*
- ✅ **Il «corso senza nome» che una volta su tre spariva** *(aperta il 2026-08-09 dalla
  revisione adversariale, chiusa il 2026-08-19 con il Pool 1.12)*. Era archiviata come
  varianza del modello, con la riserva che l'anti-perdita promette che nulla sparisca in
  silenzio. Misurata dopo il salto a Sonnet 5, la varianza non c'era più: **3 giri su 3**, la
  frase «ho fatto anche un corso, ma non mi ricordo più né quale né dove» non veniva
  instradata **né** dichiarata nel «lasciato fuori». La diagnosi ha cambiato bersaglio:
  l'istruzione «un corso → formazione» nel prompt c'era già, per esteso; a farla perdere era
  il **conflitto con la regola vicina** — davanti a un accenno che si auto-nega vinceva «non
  aggiungere e non inventare nulla». La cura è una riga che dice che un accenno vale anche
  quando è incompleto, messa **prima** di quella del non inventare. Riverificata come era
  stata misurata: **3 su 3 ripescato**, le altre tre trappole intatte, il conduttore in
  passo. *(`prompt-pool/CHANGELOG.md`, Pool 1.12; `profilo/contatti` 1.3.)*
- ✅ **Un collaudo che cede quando la macchina è carica** *(aperta il 2026-08-18, chiusa il
  2026-08-19)*. Guardandolo da vicino, `UnaRispostaLungaMaVivaNonScadeMai` aveva **due**
  difetti e non uno. Il primo era quello segnalato: quattro pause da 120 ms contro un
  secondo di silenzio concesso, un margine che una macchina satura si mangia. Il secondo
  non lo sapeva nessuno: la risposta durava **551 ms in tutto, meno del tetto**, e questo
  collaudo esiste apposta perché la batteria se ne accorga se un giorno l'attesa tornasse
  un tetto complessivo — cosa che con quei numeri non sarebbe successa. Provato invece di
  dedotto: **falsificando il client** (tolto il riarmo del silenzio a ogni pezzo) il vecchio
  collaudo **passava lo stesso**. Ora le proporzioni sono rovesciate — ventun pause da
  60 ms, 1260 ms contro 1000 concessi — ed è migliorato in tutt'e due le direzioni: il
  margine per singola pausa passa da 8× a 16×, e contro lo stesso client falsificato adesso
  diventa **rosso**. *(`CollaudiClientClaude`; cap. 02.5.)*
- ✅ **Il PDF via MCP** *(aperta il 2026-08-19 con la chiusura di T8c, chiusa lo stesso
  giorno)*. La voce diceva «non è detto sia impossibile, va provato»: provato, e regge. Quel
  che il motore del browser pretende **non è una finestra visibile** — come diceva il
  cap. 09 — ma un filo STA con la sua pompa di messaggi, e a smentire la frase è stato il
  **banco di collaudo**, che stampa PDF veri da un processo di test dove finestre non ce n'è
  nessuna. Quel filo è stato portato dai collaudi al prodotto (`FiloGrafico`, che il banco
  adesso usa invece della sua copia), `esporta_documento` ha imparato il parametro
  `formati` — `docx` · `pdf` · `entrambi`, e chi non lo dice li vuole tutti e due, come per i
  testi rifiniti — e se il motore non c'è i DOCX si consegnano dicendo perché il PDF manca.
  La prova che conta non è del banco: l'**eseguibile vero** avviato con `--mcp` ha scritto un
  PDF da 26.865 byte, firma `%PDF`, ed è uscito pulito. *(cap. 09.3; `ToolDiScrittura`,
  `Motore/FiloGrafico.vb`; collaudo «Reale» `IlPdfEsceAncheDaEsportaDocumento`.)*
- ✅ **La porta «qui c'è tutto» del profilo** *(aperta il 2026-08-14 con la chiusura di T6,
  chiusa il 2026-08-19)*. Delle due cose che la cartella documenti serve a fare, mancava la
  prima: trovare da sé il CV da cui costruire il profilo. Il dato c'era già — la
  classificazione indicava il più recente e lo salvava in `documenti.json` — ma non lo
  leggeva nessuno, e chi importava un CV doveva ritrovarselo a mano. Adesso premendo
  «IMPORTA CV DA UN FILE» il programma **lo propone per nome**, e lascia tre uscite: usalo,
  scelgo io un altro file, lascia stare. Si propone e non si prende, perché il passo 4 del
  capitolo è la conferma umana e la macchina che indovina il più recente da nome e data
  qualche volta sbaglia; e che il file esista si guarda **al momento di proporlo**, perché
  qui nessun file viene copiato e nel frattempo quel CV può essere stato spostato o buttato.
  Nessun controllo nuovo nel pannello: il layout di P2 è quello validato a video a T3, e ha
  già una voce aperta sui DPI. Provata dal vivo su una cartella usa-e-getta — la finestra
  compare, «No» apre la scelta file, «Annulla» non fa niente; «Sì» no, perché avvia l'import
  vero, che è una chiamata all'AI a pagamento. *(cap. 05.2; `RaccoltaDocumenti.PercorsoDelCvPiuRecente`,
  `PannelloProfilo.CvDaImportare`.)*
- ✅ **Il cambio di lingua che fallisce a metà, provato dal vivo** *(aperta il 2026-08-18 da
  T7d, chiusa lo stesso giorno)*. Era chiusa «ragionando e per costruzione», non per prova:
  cambiando la lingua del 📄 CV-1 base il pannello butta il testo di prima *prima* di
  rigenerare, così un errore dell'AI non lascia a video un CV italiano impaginato sotto
  «Work experience» — ma quel ramo nessuno l'aveva percorso. Adesso sì, e il comportamento è
  quello promesso: colonna **«Il CV non è ancora stato scritto.»**, il motivo scritto in
  chiaro, **«Rigenera» acceso**, la tendina rimasta su «Inglese». Il caso brutto non si è
  verificato. In più, il `cv_base.json` su disco è risultato **identico byte per byte** a
  prima del tentativo: il fallimento non lo tocca, perché si scrive solo nel ramo riuscito.
  **Attenzione, la ricetta scritta in questa voce era sbagliata**: «togliendo la chiave API»
  non funziona — senza chiave `AiDisponibile` resta falso, «Genera 📄 CV-1 base» non si
  accende e a P6 non ci si arriva nemmeno, quindi non si fallisce a metà, si resta fuori.
  La prova buona vuole una chiave **presente ma non valida**: il motore si monta, la chiamata
  parte davvero e l'API la rifiuta (401). Vale per ogni prova futura dei rami «l'AI
  fallisce». Fatta su dati fabbricati in una cartella usa-e-getta, senza toccare quelli veri.
  *(cap. 03.6; `PannelloDocumenti.CambiaLinguaDelCvBaseAsync`.)*
- ✅ **Uscendo da P7 con la barra in alto, la bozza si perde** *(aperta il 2026-08-15 dentro
  il collaudo di T7a, chiusa il 2026-08-18)*. `SalvaLaBozza` era appesa a tre momenti —
  preparare l'`.eml`, «L'ho spedita» e il bottone «◀ Torna ai documenti» — ma dalla barra di
  navigazione in cima si usciva **senza passare di lì**, e si perdevano il destinatario
  scritto a mano, le spunte degli allegati e il messaggio riscritto, che era costato una
  chiamata all'AI. La perdita era **silenziosa**: riaprendo, P7 rileggeva `email.json` e
  mostrava la bozza vecchia come se fosse l'ultima. Guardando dove metterci mano è venuto
  fuori che il buco non era di P7: **un aggancio d'uscita non esisteva affatto**, e
  `IPannelloArea` parlava solo di geometria. Adesso c'è — `IPannelloCheSalvaUscendo`, che
  `FormPrincipale` interpella a ogni cambio di pannello, prima di nasconderlo — e per ora la
  implementa **solo P7**, che era l'unico caso costoso. Non salva mentre l'AI lavora e non
  solleva mai: qui si sta cambiando pannello, e un disco pieno non deve inchiodare la
  navigazione. *(cap. 07.1; `Ui/IPannelloCheSalvaUscendo.vb`; `FormPrincipale.SalvaChiEsce`.)*
  **Nota per chi verrà**: lo stesso buco resta **latente** in P2 (le correzioni al profilo),
  P5 e P4 (l'annuncio incollato e non ancora analizzato). Non è stato chiuso lì perché
  nessuna voce lo chiedeva e perché quei pannelli hanno già le loro guardie alla chiusura
  della finestra — ma l'aggancio è pronto e basta implementarlo.
- ✅ **Una candidatura ereditava il messaggio di quella di prima** *(trovata e chiusa il
  2026-08-18, dalla revisione dell'aggancio d'uscita qui sopra)*. P7 si riusa da una
  candidatura all'altra, e la bozza in memoria si riempie in due modi soli: ripresa dal
  disco o scritta dall'AI. Ma la scrittura ha **due uscite anticipate legittime** — manca la
  chiave, manca la lettera — e nessuna delle due toccava la bozza: oggetto e corpo restavano
  quelli della candidatura precedente, **visibili a video sotto il nome di questa**. Bastava
  poi un salvataggio perché finissero nel suo `email.json`. Il difetto esisteva già — anche
  «◀ Torna ai documenti» salvava incondizionatamente — ma l'aggancio d'uscita appena aggiunto
  ne allargava la bocca a *qualunque* clic sulla barra, e conveniva chiuderlo subito. Ora il
  pannello ripulisce la bozza e le caselle all'arrivo, accanto alle righe che già ripulivano
  il destinatario per lo stesso motivo. Il collaudo che lo copre è stato **verificato al
  contrario**: tolta la correzione, la seconda candidatura si ritrova 81 caratteri di corpo
  che non sono suoi. *(cap. 07.1; `PannelloEmail.MostraLaCandidaturaAsync`.)*
- ✅ **Un troncamento che nessuno dichiarava, nel brainstorming** *(aperta e chiusa il
  2026-08-18, trovata mentre si guardava la coda del salto a Sonnet 5)*. Sulla strada
  sincrona un troncamento è un errore e si vede; in streaming — per scelta, giusta — non lo
  è, perché il testo arrivato è già sotto gli occhi di chi legge, e il commento del client
  diceva «il motivo della fine si porta a casa in `RispostaAi.MotivoFine` e **a dirlo è il
  pannello**». Solo che il pannello non poteva saperlo: `MestiereAi.EseguiInStreamingAsync`
  restituiva il solo `Testo` e il motivo moriva lì. Risultato: la conversazione poteva
  fermarsi **a metà frase** senza che nulla lo dichiarasse, e chi legge credeva che l'AI non
  avesse altro da dire — proprio mentre il cambio di modello rendeva i tetti più stretti. Il
  principio era già scritto nel pannello, dieci righe più su, per l'interruzione dell'utente:
  «una risposta troncata che non lo dichiara è peggio di nessuna risposta». Ora vale per
  tutt'e due: accanto a «(interrotto)» c'è «(fermata qui: ha raggiunto il limite di
  lunghezza)». *(cap. 02.5; `RispostaAi.Troncata`; `Brainstorming.UltimoTurnoTroncato`.)*
- ✅ **Il destinatario nella voce di registro** *(aperta il 2026-08-14 da T6, chiusa il
  2026-08-18)*. Era una delle tre cose che il cap. 07.3 assegnava a T6 e che T6 non ha
  portato: il destinatario viveva solo nella bozza `email.json` della candidatura, così per
  sapere «a chi ho scritto?» bisognava aprire una candidatura alla volta — mentre l'indice
  esiste proprio per rispondere senza aprire niente. È bastato un campo, perché
  `VoceRegistro.Da` è il **punto unico** da cui passano sia l'annotazione sia la
  rigenerazione dalle cartelle: i tre pannelli che annotano non sono stati toccati. La bozza
  resta la fonte — il valore lo digita l'utente e da qui non si scrive mai all'indietro — e
  le voci vecchie si ricostruiscono da sé, senza migrare niente, esattamente come il commento
  in cima al file aveva previsto. *(cap. 07.3; `Dati/Registro.vb`.)*
- ✅ **Lo strumento di collaudo non sapeva aspettare** *(aperta il 2026-08-18 da T7c, chiusa
  lo stesso giorno)*. Non c'era un `aspetta_che`: si alternavano `clic` e `controlli`, e le
  cose veloci passavano in mezzo senza farsi vedere. Adesso c'è, in due modalità — lo
  **stato di un controllo** (acceso/spento) e il **contenuto di un file** (che compaia, che
  cambi, che contenga una stringa) — perché una sola non bastava: la trappola, pagata prima
  di scriverlo, è che un bottone come «Rigenera» è acceso **sia prima sia dopo** il clic, e
  aspettarne lo stato si soddisfa subito senza che il lavoro sia finito. Per aspettare la
  fine di un lavoro AI la strada onesta è il file che quel lavoro produce. Il ciclo gira
  **dentro** una sola invocazione di PowerShell, perché avviarlo a ogni tentativo costerebbe
  più dell'attesa. *(`strumenti/mcp-collaudi/README.md`.)*
- ✅ **Il 📄 CV-1 base non si può chiedere in inglese** *(aperta il 2026-08-15 da T7a,
  chiusa il 2026-08-18 da T7d)*. La porta era data per persa in P2, «accanto al bottone»,
  perché la tendina di P6 rigenerava *una candidatura* e il CV-1 base non ne ha una. È
  caduta la premessa, non la regola: da quando P6 **rilegge** il CV-1 base dal suo
  `cv_base.json` invece di rigenerarlo a ogni visita, quel pannello è la sua casa come lo
  è di una candidatura, e la tendina ci sta senza aggiungere nessun controllo altrove.
  Cambiarla chiede conferma e lo riscrive; la lingua scelta viaggia fino in fondo —
  prompt `.en`, rifinitura anti-slop nella lingua giusta (era **inchiodata all'italiano**),
  etichette dell'anteprima, etichette del DOCX/PDF e sigla `_EN_` nel nome del file. Provato
  sui dati veri: `CV_..._EN_2026-08-18.docx` con «Work experience» dentro, accanto
  all'italiano di ieri che è rimasto dov'era. *(cap. 10.1; cap. 03, tabella dei pannelli,
  P6; cap. 11.1; pool `generazione/cv_base.en`.)*
- ✅ **I bottoni d'esportazione spenti su un CV-1 base che esisteva** *(aperta e chiusa il
  2026-08-18 da T7d)*. Nata da una prova di Mirco — «ho cliccato e non erano abilitati» — e
  confermata dal vivo: il `cv_base.json` era su disco dal giorno prima e **non lo rileggeva
  nessuno** (`ArchivioProfilo.CaricaCvBase` esisteva, la chiamavano solo i collaudi), così
  l'unica strada per riesportarlo passava da una rigenerazione — 45 secondi, altri token, e
  un testo diverso da quello già approvato. Senza AI non si poteva affatto. Ora «rientrare
  non rigenera» vale per tutti e due i documenti. *(cap. 03.6; cap. 11.1.)*
- ✅ **Il destinatario proposto dall'annuncio** *(aperta il 2026-08-14 da T6, chiusa il
  2026-08-15 da T7a)*. Il campo era sempre vuoto perché non c'era niente da proporre: il
  **Pool 1.06** ha insegnato ad `analisi_annuncio` il campo **`contatto {email,
  riferimento}`**, con l'ordine di ricopiare alla lettera e **mai dedurre** — un indirizzo
  non si ricava dal sito dell'azienda né dal nome di chi firma, e se l'annuncio non lo
  scrive i due campi restano vuoti. In P7 `ProponiIlDestinatario` lo mette nella casella
  **solo al primo arrivo**, mai sopra una bozza ripresa (lì il destinatario è già passato
  per le mani dell'utente, e sostituirglielo sarebbe cancellargli una decisione), e **dice
  da dove viene**, con una barra e un tooltip che si azzerano appena l'utente ci mette
  mano. Si propone **solo l'`email`**: il `riferimento` — la persona, l'ufficio, il codice
  della posizione — si estrae ma non si propone, perché nella casella del destinatario
  darebbe un'email che non parte. Le due metà della promessa del cap. 07.1 sono ora
  mantenute entrambe: quello che l'annuncio dice viene proposto, quello che non dice non si
  inventa. *(cap. 07.1; pool CHANGELOG 1.06; `VistaAnnuncio.IndirizzoPerCandidarsi`.)*
- ✅ **L'`.eml` aperto in un programma di posta vero e spedito da lì** *(aperta e chiusa il
  2026-08-14, la sera stessa della tappa)*. Mirco ha percorso il giro sui **dati veri**: la
  candidatura a Delta Sistemi preparata dall'applicazione, aperta nel **nuovo Outlook** —
  che ha riconosciuto `X-Unsent` e l'ha mostrata come bozza pronta, con destinatario,
  oggetto e corpo — e **spedita da lì**, al proprio indirizzo. L'email è arrivata, allegati
  compresi. Lungo la strada un falso allarme istruttivo: al primo tentativo Outlook ha detto
  «Non è stato possibile allegare i file… Riprova più tardi». Il file era a posto (validato
  di nuovo con un lettore indipendente: `X-Unsent`, intestazioni, e i due PDF **identici
  byte per byte**); a mancare era **la sessione dell'account**, scaduta dentro Outlook — che
  per allegare un messaggio importato deve parlare col servizio. Rifatto l'accesso, gli
  allegati sono stati caricati e l'invio è andato. *(cap. 07.2; cap. 14, T6.)*
- ✅ **La scelta della cartella documenti, premuta a mano** *(aperta e chiusa il
  2026-08-14)*. Il `FolderBrowserDialog` che lo strumento di collaudo non sa pilotare l'ha
  premuto Mirco, indicando la **sua** cartella dei documenti personali. Tredici file letti,
  sottocartelle di primo livello comprese: i due CV riconosciuti (col più recente indicato),
  e **tutto il resto in «altro»** — carte d'identità, codici fiscali, NASPI, documenti dello
  stage. La prova ha mostrato la cosa che contava, cioè quella che **non** è successa:
  nessun documento personale è finito fra gli allegabili di un'email. *(cap. 05.2;
  `strumenti/mcp-collaudi/`.)*
- ✅ **La chiave API cifrata (DPAPI)** *(aperta il 2026-08-07 da T3, chiusa il 2026-08-14 da
  T6)*. Vive in `segreti.bin` nella cartella dati, cifrata con la protezione dati di Windows
  e **legata all'account** che l'ha salvata: copiata altrove non si apre. La si digita in una
  finestra al primo avvio — il posto che a T3 mancava — e la si sostituisce riavviando con
  `--chiave`, finché le Impostazioni non arriveranno con T9. Nella diagnostica compare solo
  come `sk-ant-…1234`, che è la metà del collaudo di tappa verificabile qui.
  *(cap. 11.3; diario Step 2.19.)*
- ✅ **Avviare l'applicazione su una cartella dati usa-e-getta** *(aperta il 2026-08-14 da
  «Elimina profilo», chiusa lo stesso giorno aprendo T6)*. `--dati <percorso>` sposta l'intera
  applicazione su un'altra radice, dichiarandolo nel titolo della finestra e nella barra di
  stato. Ha fatto subito il suo mestiere: tutto il collaudo di T6 è stato percorso su copie
  dei dati veri, senza mai toccarli. *(cap. 11.1; diario Step 2.17.)*
- ✅ **La fascia dei comandi a finestra stretta** *(aperta il 2026-08-14 da T5d, peggiorata due
  volte, chiusa lo stesso giorno aprendo T6)*. La fascia ora **va a capo** quando lo spazio non
  basta, e la geometria — che era ricopiata in cinque pannelli — vive in un posto solo
  (`Ui/FasciaDeiComandi`). I 676 px di bottoni sovrapposti alla larghezza minima non ci sono
  più, e il collaudo cammina su cinque pannelli a sei larghezze verificando che due comandi non
  si intersechino mai. *(cap. 03.4; diario Step 2.17.)*
- ✅ **L'export del registro in CSV/markdown** *(aperta il 2026-08-13 da T5c, chiusa il
  2026-08-14 aprendo T6)*. Esce quel che è a schermo, filtrato e ordinato com'è: il markdown si
  porta il proprio perimetro in cima («3 di 12»), il CSV no perché una frase sopra una tabella
  non è più una tabella. *(cap. 07.3; diario Step 2.17.)*
- ✅ **Il filtro per stelle nella coda** *(aperta il 2026-08-13 da T5c, chiusa il 2026-08-14
  aprendo T6)*. La tendina dice «almeno N», i due filtri si intersecano, e quando nascondono
  qualcosa i contatori aggiungono «ne vedi 1 su 3» — perché una candidatura mai confrontata non
  ha stelle e sparirebbe in silenzio. *(cap. 07.3; diario Step 2.17.)*
- ✅ **L'eliminazione vera non è mai stata premuta sull'applicazione** *(aperta e chiusa il
  2026-08-14)*. L'assistente si era fermata un passo prima — bottone premuto, finestra
  aperta, parola scritta, «Elimina il profilo» acceso, e poi **annullato**, perché
  dall'altra parte c'era il profilo vero. L'ha premuto **Mirco**, lanciando l'applicazione
  con `avvia-demo.bat` sui propri dati, con la copia di sicurezza pronta accanto. Il
  risultato è quello che il cap. 11.5 promette e i collaudi verificavano al banco: la
  cartella `profilo\` sparita per intero — profilo, storico, `cv_base.json` e la sua
  `out\` — e **tutto il resto al suo posto**, le sei candidature nelle loro cartelle, il
  `registro.json` non riscritto, i dati di navigazione intatti. La prova che mancava era
  questa; resta aperto il modo di rifarla **senza** rischiare i dati veri (voce qui sopra).
  *(cap. 11.5; diario Step 2.16.)*
- ✅ **La coda dell'opportunità non si riapre** *(aperta il 2026-08-10 da T4, chiusa il
  2026-08-13 da T5c)*. La vista che mancava è la **Home**: la coda mostra tutte le
  candidature con stelle, stato e provenienza, e da lì una si riapre col doppio clic o col
  suo bottone, giudizi e documenti compresi. Provata sulle sei candidature vere di Mirco —
  comprese quelle di T4, che il campo `stato` non ce l'hanno e se lo fanno **dedurre dai
  file presenti** invece di farsi riscrivere — e con l'applicazione **chiusa e riaperta**,
  che è la domanda vera: quello che è stato scritto ieri si ritrova domani. La promessa
  «tutto riapribile» del cap. 12.7 non è più mantenuta solo sul disco.
  *(cap. 11.1; cap. 12.7; cap. 14, T5c; diario Step 2.13.)*
- ✅ **Un annuncio davvero pescato da un portale** *(aperta il 2026-08-10 da T4, chiusa il
  2026-08-12 da T5b)*. La gamba B di T4 era stata percorsa con un annuncio **verosimile ma
  scritto per il collaudo**; adesso la cattura dal browser esiste, e l'annuncio è arrivato
  da **Indeed** con tutto quello che nessuno si inventa — il banner dei cookie, i filtri, i
  menù e i titoli degli altri annunci nella stessa pagina. L'analisi ne ha ricavato il
  ruolo giusto con la sua azienda e il suo contratto, il confronto ha dato 1,4 stelle su 10
  voci, e l'opportunità è finita nella sua cartella con fonte e link.
  *(cap. 06.4; cap. 14, T4 gamba B e T5b; diario Step 2.12.)*
- ✅ **Il giro dal menù dei portali, mai provato sull'applicazione vera** *(aperta il
  2026-08-12 alla chiusura di T5a, chiusa lo stesso giorno)*. Non era un difetto del
  pannello ma un **limite dello strumento** di collaudo, che sapeva premere bottoni e
  scrivere nelle caselle ma non scegliere una voce da una tendina: Indeed risultava provato
  solo perché è il primo ed è già selezionato, gli altri tre erano stati aperti dalla
  casella dell'indirizzo. Insegnato allo strumento a scegliere (`scegli_voce`), il giro è
  stato percorso su tutti e quattro i portali. Lungo la strada lo strumento ha scoperto due
  difetti **propri**, che facevano premere il controllo sbagliato e riferire il contrario
  del vero. *(strumenti/mcp-collaudi/README.md; diario Step 2.11 e 2.12.)*
- ✅ **`taratura.json` e `modelli.json` non li legge nessuno all'avvio** *(aperta il
  2026-08-07 da T3, chiusa il 2026-08-07 da T3c)*. Mancava il punto in cui il motore si
  monta all'avvio: l'ha portato **`Motore/ContestoApp`**, che carica entrambi i file e
  avvisa quando ripiega sui predefiniti. Da allora un numero ritoccato nella cartella dati
  ha effetto sull'applicazione, che era esattamente ciò che non succedeva.
  *(cap. 11.6; cap. 02.3.)*
- ✅ **La città quando il CV ne porta due** *(aperta il 2026-08-08 da T3, chiusa il
  2026-08-09 dalla revisione adversariale)*. Il **Pool 1.02** dichiara nei prompt che la
  città è quella del **domicilio** — dove uno è raggiungibile per lavorare — e una sola;
  il campo è **tornato un pass/fail** nel collaudo reale, e morde: la residenza oggi
  boccerebbe. *(cap. 04; pool CHANGELOG 1.02; `CollaudoReale`.)*
- ✅ **Le lingue non hanno un posto** *(aperta il 2026-08-08 da T3, chiusa il 2026-08-09
  dalla revisione adversariale)*. Il rimedio concordato è nel **Pool 1.02**, esteso a tutti
  i prompt che toccano le competenze: le lingue **sono** competenze, riportate come dette,
  **mai con un livello non dichiarato**. Validato sul CV vero: 3 lingue su 3 ricopiate alla
  lettera, contro le 0 del prototipo. Il campo `lingue` vero e proprio resta un'idea futura.
  *(pool CHANGELOG 1.02; diario Step 2.8.)*
- ✅ **Il patentino del muletto si perde, dichiarandolo** *(aperta il 2026-08-09 da T3,
  chiusa il 2026-08-09 dalla revisione adversariale)*. Il **Pool 1.02** dà a quel genere di
  qualifica la sua casa: i **patentini professionali stanno in formazione**, e lo dicono
  tutti i prompt coinvolti (i blocchi `altrove`, `competenze` che separa l'abilità dal
  certificato, `patente` che chiarisce che non sono patenti di guida). Nel dialogo reale il
  muletto atterra in formazione **al primo colpo**. *(pool CHANGELOG 1.02; diario Step 2.8.)*
- ✅ **P5: a dialogo finito resta un buco** *(aperta il 2026-08-09 da T3, chiusa il
  2026-08-09 dalla revisione adversariale)*. La fascia della risposta ora **si ritira**
  quando il dialogo chiude: niente più zona morta fra l'ultima bolla e i bottoni.
  *(cap. 03.6; commit `a813253`.)*
- ✅ **P5: l'eco arriva dopo il verdetto** *(aperta il 2026-08-09 da T3, chiusa il
  2026-08-09 dalla revisione adversariale)*. Era davvero una decisione sul disegno di
  `Mossa`, ed è stata presa: la mossa ora porta **eco ancorate** al punto giusto della
  sequenza, così l'utente rivede le proprie parole *prima* del «lo lascio fuori» — e nella
  passata finale ogni recupero ha la sua eco, dove prima un campo singolo le sovrascriveva.
  *(cap. 02.4; commit `a813253`.)*
