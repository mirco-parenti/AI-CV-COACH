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

*Chiusa il 2026-08-21 con T9c: lo stato «esito» e il promemoria di follow-up sono in
«Chiuse».*

## Da T7 — multilingua e qualità (2026-08-15, alla chiusura di T7a)

*La voce sulla **modifica a mano dei testi in P6** è stata chiusa il 2026-08-24, in ritardo
sui fatti: la costruiva T9d il 22 agosto, e il quinto tempo di T9e l'ha estesa due volte.
Sta in «Chiuse».*

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
erano in nessun piano, e nessuna è grave. La voce sulla finestra a misura ridotta è stata
verificata il 2026-08-23 dal collaudo dal vivo di T9e ed è in «Chiuse». Altre tre — le
**parole incollate** fra un blocco e l'altro della pagina letta, il **404 raccontato come un
guasto di rete** e il **markdown grezzo** nelle bolle del brainstorming — sono state chiuse il
2026-08-24 rileggendo il codice: le aveva curate T9d il 22 agosto, e nessuno le aveva
spostate.*

- **La traccia del dialogo reale non ha più nessuna trappola che sfugga.** Curato il «corso
  senza nome» (Pool 1.12), tutte e quattro le trappole della traccia vengono instradate ogni
  volta — ed è il rapporto stesso a dire che in quel caso «la traccia va rifatta»: un
  collaudo che passa sempre smette di misurare. Serve una trappola nuova, più difficile di
  quelle di adesso, scritta apposta per il modello di oggi. *(cap. 14, T3;
  `CollaudiDialogoReale`; `casi/reale/dialogo_guidato.md`.)*

## Dal collaudo di tappa di T8 — il gruppo C (2026-08-21)

*La voce sui giudizi di «contesto» saltati è stata chiusa il 2026-08-23 dal collaudo dal vivo
di T9e ed è in «Chiuse»: misurata, come chiedeva, invece che curata a naso.*

## Da T9a — backup e ripristino (2026-08-21, alla chiusura)

*Chiusa lo stesso giorno: il tredicesimo tool è stato chiamato da un client MCP vero ed è
in «Chiuse».*

## Da T9b — le Impostazioni (2026-08-21, alla chiusura)

*La voce sul bottone «⚙ Impostazioni» premibile durante una chiamata all'AI è stata chiusa il
2026-08-22 col primo tempo di T9d, ed è in «Chiuse»; le due qui sotto restano aperte.*

*Le due voci sui DPI — la finestra mai vista su uno schermo piccolo e «Svuota i dati di
navigazione» mai premuto — sono state **chiuse il 2026-08-23** dal quinto tempo di T9e e
stanno in «Chiuse»: la prima curata, la seconda provata proprio grazie alla cura della prima.*

## Da T9d — la rifinitura (2026-08-22, alla chiusura)

*La voce sulla fascia di P6 a DPI alti è stata verificata il 2026-08-23 dal collaudo dal vivo
di T9e ed è in «Chiuse»: regge.*

*Il collaudo fragile che doveva finire qui — `UnaRispostaLungaMaVivaNonScadeMai`, che cadeva
in batteria completa e mai da solo — è stato **curato** invece che annotato: sta fuori dal
parallelismo e la sua storia è nella chiusura della gamba (cap. 14).*

## Dal collaudo dal vivo di T9e — il giro D, rimasto fuori (2026-08-23)

*Il quarto tempo di T9e prevedeva un **giro D** apposta per chiudere in un colpo tre debiti
vecchi: tutti e tre vogliono una macchina che questa postazione non è. Non si è potuto fare,
e la riserva si dichiara qui invece di lasciarla implicita (regola 15).*

- **Serve una macchina senza l'SDK .NET 10 e con Word.** Non può essere AVIOLAB03, che l'SDK
  ce l'ha, e qui c'è solo LibreOffice. Con quella macchina si chiudono in un giro solo la
  **prova dell'exe su un PC pulito** (voce di T1), il **DOCX e il PDF aperti in Word** (voce
  di T4) e il **`.docx` salvato da Word e reimportato nel profilo** (voce di T3), che sono
  già aperte qui sopra e restano tali. Il giro D si può fare prima del sesto tempo o subito
  dopo; quel che non si può è darlo per fatto. *(cap. 13.5; cap. 14, T9e.)*
  **Aggiornamento del 2026-08-24**: l'eseguibile che stava lì pronto —
  `VB.NET\src\pubblicazione\TrovaLavoro.exe`, un file solo — porta **ProductVersion
  0.3.041** ed è del 22 agosto, cioè **precedente alle cure del quinto tempo**: non ha le
  tre cure di scala, né R6, né R7, né la domanda di approfondimento. Provarlo adesso
  misurerebbe una versione superata, e proprio sul difetto — il comportamento a DPI alti —
  che quelle cure hanno tolto. Il giro D si fa quindi con l'eseguibile del **sesto tempo**
  (1.0.000), che va ripubblicato prima di portarlo sull'altra macchina.
  **Aggiornamento del 2026-08-24, la sera**: *quella metà è pagata.* Il sesto tempo ha
  pubblicato la **1.0.000** — `VB.NET\src\pubblicazione\TrovaLavoro.exe`, un file solo da
  118.707.086 byte, proprietà verificate e avvio provato qui su una cartella dati
  usa-e-getta — e l'exe pronto da portare è adesso quello giusto. Quel che manca è **solo la
  macchina**: la voce resta aperta per intero, e con lei le tre che il giro D chiuderebbe.
  Con questa riserva **T9e e T9 si chiudono** (cap. 13.9; cap. 14, il sesto tempo).

## Dal sesto tempo di T9e — il rilascio (2026-08-24, alla chiusura)

*Il rilascio non ha lasciato indietro niente di suo: il numero, il banco, la pubblicazione e
le quattro verifiche sull'eseguibile sono state fatte e sono scritte nel §13.9. La **riserva
della tappa** è il **giro D** qui sopra — non una voce nuova, la stessa di due giorni fa, che
adesso però ha l'eseguibile giusto ad aspettarla. Restano aperti anche i quattro reperti del
quinto tempo e le due voci di scala/interfaccia elencati più sotto: nessuno di essi è del
rilascio, e nessuno impediva di rilasciare.*

- **La demo (video) per il portfolio.** T9 la dichiarava fin dal 5 agosto, in una riga
  accanto al diario, al README e al tag: nessun tempo l'ha fatta e nessuno l'aveva annotata.
  L'ha trovata la rilettura di quel che la tappa prometteva, alla chiusura — che è il motivo
  per cui la regola 15 chiede di rileggere il **dichiarato**, non solo il fatto. A differenza
  del giro D non aspetta una macchina: aspetta un pomeriggio, un giro registrato
  sull'applicazione vera e un posto dove metterlo. *(cap. 14, T9; visto mancante il
  2026-08-24 chiudendo il sesto tempo.)*

## Da revisione adversariale (2026-08-09)

*L'unica voce rimasta — il pannello del logo a DPI alti — è stata **chiusa il 2026-08-23** dal
quinto tempo di T9e ed è in «Chiuse». Era aperta da quattordici giorni e aspettava uno schermo
su cui verificarla.*

## Dal quinto tempo di T9e — le cure dei reperti DPI (2026-08-23)

*Le tre cure sono fatte e verificate dal vivo a 150% (decisione 15.7). Quel che segue è ciò che
si è deciso di **non** fare adesso, e va scritto per non spacciarlo per finito.*

*La voce sulla **barra orizzontale** delle Impostazioni è stata chiusa il 2026-08-24, primo
lavoro del ramo delle rifiniture prima del giro D, ed è in «Chiuse»: resta la specie da cui
veniva, che è la voce qui sotto.*

- **Le costanti di `StileApp` sommate a misure già scalate.** Curati i tre difetti, resta la
  specie da cui venivano: `MargineRiquadro` (14), `DistanzaControlli` (12), `InterlineaMinima`
  (8) e le misure dei bottoni sono in unità di progetto, e il codice che dispone a mano le
  finestre le somma a `.Bottom`, `.Right`, `.ClientSize` — che a runtime sono in pixel dello
  schermo. Quante siano dipende da come si conta: due censimenti indipendenti dello stesso
  giorno danno **83 occorrenze su 16 file** e **107 su 14**, quindi l'ordine di grandezza è
  «oltre ottanta, su una quindicina di file» e il numero esatto non va citato come se fosse
  una misura. A queste si aggiungono i **10 file** di `Ui/` che non dichiarano
  `AutoScaleMode` contro i 7 che lo fanno — e la divisione non è fra finestre e pannelli,
  come verrebbe da dire: `PannelloEmail` sta fra i dieci senza e `FormPrincipale` fra i
  sette con. Il danno è cosmetico
  (spaziature compresse a DPI alti, non comandi irraggiungibili); il rischio di toccarle non
  lo è, perché sarebbe rimettere mano al posizionamento di sedici file e il banco **quasi non
  ha collaudi di misure d'interfaccia**: dal 2026-08-24 ce ne sono due, e guardano una finestra
  sola — le Impostazioni quando scorrono. Su tutto il resto una regressione non la vedrebbe
  nessuno fino al prossimo collaudo dal vivo. Rimandate di proposito, non dimenticate: con `ScalaSchermo` ora
  esiste il posto dove passerebbero, e le funzioni pure che le renderebbero collaudabili.
  *(cap. 03.4 e 15.7.)*

## Dal quinto tempo di T9e — le cure dei reperti (2026-08-24, alla chiusura)

*Dodici reperti su dodici curati, più due difetti trovati a mano nel pannello del profilo.
Quel che resta sono **quattro reperti nuovi**, visti provando le cure dal vivo: nati qui, e
annotati qui prima che in ogni altro posto. Il **riordino** delle voci di un documento, che
R6 nominava accanto alla rimozione, non sta qui ma in `idee_future.md`: la forma di R6 è stata
decisa — si toglie e si rimette — e riordinare è un raffinamento in più, non un debito.*

*Due dei quattro — il **segno ✎** che non sapeva del disco e la **selezione** che tornava in
cima — sono stati curati il 2026-08-24, primo lavoro del ramo delle rifiniture prima del giro
D, e sono in «Chiuse».*

- **Rigenerare la lettera di una candidatura costruita con un altro profilo dà un errore
  criptico.** «⚠ Rigenera la lettera» su una candidatura i cui documenti nacquero da una
  versione di profilo diversa da quella di adesso finisce in «L'AI ha risposto in una forma
  che non riesco a leggere» — che manda a cercare il guasto dalla parte sbagliata, e per un
  utente non vuol dire niente. Il dato per dirlo bene c'è già ed è a portata: la
  `versione_profilo` annotata nello `stato.json`. *(cap. 08.4; cap. 11.1; visto il 2026-08-24
  provando R7 dal vivo.)*
- **`scegli_riga` dello strumento di collaudo tocca solo il primo elenco della finestra.** I
  due elenchi di «Modifica i testi» sono esposti all'accessibilità come **Table** e non come
  List, e l'attrezzo prende sempre la prima che trova: la colonna «Lasciate fuori» non si può
  guidare da lì, e R6 è stato provato a mano. È un debito dell'attrezzo, non del prodotto, ma
  pesa sul prodotto — quella parte di finestra oggi nessun collaudo automatico la tocca.
  *(`strumenti/mcp-collaudi/README.md`.)*

## Chiuse

- ✅ **La modifica a mano dei testi in P6** *(aperta il 2026-08-18 alla chiusura di T7b,
  **fatta** il 2026-08-22 da T9d, spostata qui il 2026-08-24)*. **Costruita**, e in una forma
  diversa da come la voce la immaginava. Delle tre cose che il cap. 8.4 prometteva davanti al
  prima/dopo — accettare, riscrivere a mano, tornare al testo non rifinito — la seconda è
  arrivata con la finestra «Modifica i testi» di T9d, e la terza è stata **ritirata** poche
  ore dopo insieme al prima/dopo stesso: su dati veri quel confronto cambiava cinque parole e
  nessun fatto, e un confronto che l'utente non distingue è un comando in più da capire, non
  una garanzia. Le tre promesse restano due, ed è scritto nel capitolo. Il quinto tempo di T9e
  ha poi esteso la finestra due volte: la riscrittura **si ricorda** e la lettera lo viene a
  sapere (R7), e da lì si sceglie anche **quali voci** il documento porta (R6). La voce era
  rimasta aperta per svista, non per una riserva: si chiude in ritardo sui fatti.
  *(cap. 08.4; cap. 03.6.)*
- ✅ **Le parole si incollano fra un blocco e l'altro della pagina letta** *(aperta il
  2026-08-19, **curata** il 2026-08-22 da T9d, spostata qui il 2026-08-24)*. Il lettore
  chiedeva `innerText` al solo `body`, e la fine di un blocco usciva attaccata all'inizio del
  successivo. Adesso scende fino ai **blocchi foglia** — quelli che dentro non contengono
  altri blocchi — e ne unisce i testi con un a capo: la fonte resta `innerText`, coi suoi
  pregi, ma fra un pezzo e l'altro un confine c'è sempre. Resta il limite dichiarato: dentro
  un `iframe` non si entra, perché quella è un'altra pagina. *(cap. 06.7; `LettorePagina`.)*
- ✅ **Un 404 viene raccontato come un problema di rete** *(aperta il 2026-08-19, **curata**
  il 2026-08-22 da T9d, spostata qui il 2026-08-24)*. La frase adesso la sceglie
  `EsitoNavigazione` guardando cos'è successo davvero — esito, errore di rete e **stato
  HTTP** — e con una risposta d'errore del server dice «Il server ha risposto «404»: questa
  pagina non c'è», invece di mandare a controllare il modem. È la stessa passata che ha
  smesso di incolpare la cosa sbagliata pannello per pannello. *(cap. 06.6;
  `EsitoNavigazione`.)*
- ✅ **Nel brainstorming il markdown si vede grezzo** *(aperta il 2026-08-19, **curata** il
  2026-08-22 da T9d, spostata qui il 2026-08-24)*. Le bolle di P5 non mostrano più gli
  asterischi e i cancelletti: `ProsaDellAssistente.SenzaMarkdown` è una funzione pura che
  toglie i segni — grassetto, corsivo, titoli, elenchi, recinzioni di codice — e di un
  collegamento tiene **il testo e l'indirizzo**, perché buttarne via uno dei due
  nasconderebbe qualcosa a chi legge. Delle due strade che la voce immaginava non se n'è
  presa nessuna: la bolla non impara a leggere il markdown — le bolle sono `Label` — e al
  prompt non si chiede niente, che sarebbe stato un bump del pool e una fiducia. La cautela
  che conta è un'altra: la ripulitura gira sul testo **intero accumulato**, mai sul
  frammento, perché la risposta arriva a pezzi e un `**` spezzato fra due di essi non si
  riconoscerebbe mai — e il collaudo lo prova con un finto che consegna **parola per
  parola**. *(cap. 03.5; `ProsaDellAssistente`; `PannelloDialogo.CresciLaBolla`.)*
- ✅ **Il segno ✎ e l'avviso di «Rigenera» non conoscono le stesse riscritture** *(aperta il
  2026-08-24 dal quinto tempo di T9e, chiusa lo stesso giorno aprendo le rifiniture pre-giro
  D)*. Delle due strade che la voce lasciava aperte si è presa la prima: **il segno legge la
  stessa fonte del disco**. Insieme al documento la finestra riceve ora i campi che vi
  risultano **già** riscritti a mano (R7), e il ✎ vale per «questo testo l'hai scritto tu» —
  adesso, o in un giro precedente: esattamente la domanda dell'avviso di «Rigenera». Quel che
  **non** è cambiato è cosa rientra nel documento al «Salva»: lì conta solo ciò che è stato
  toccato in questo giro, o un documento che nessuno ha modificato si farebbe risalvare a ogni
  visita. Erano due risposte a una domanda sola per sbaglio; adesso sono due domande distinte
  per disegno, e il collaudo tiene ferma anche la seconda. *(cap. 03.6; cap. 08.4;
  `FinestraModificaTesti`; `RiscrittureAMano.Contiene`.)*
- ✅ **Dopo aver tolto una voce, la selezione dell'elenco torna alla prima riga** *(aperta il
  2026-08-24 dal quinto tempo di T9e, chiusa lo stesso giorno aprendo le rifiniture pre-giro
  D)*. Gli elenchi si rifanno da capo a ogni «Togli» e a ogni «Rimetti», e una ricostruzione
  non ha memoria. Adesso la riga scelta si ritrova **per identità** — è la stessa voce, che
  intanto si è spostata — e quando quella riga non c'è più la scelta cade su chi ha preso il
  suo posto, o sull'ultima rimasta se era in coda. Vale per **tutti e due** gli elenchi, perché
  chi rimette dentro le voci una a una lavora in quello di destra, dove la fila si accorcia
  allo stesso modo. Per poterlo collaudare la finestra ha smesso di chiedere a
  `SelectedItems` chi è scelto — quella strada risponde solo a elenco già nato, cioè mai al
  banco — e guarda le righe una a una: stesso esito nell'applicazione, ma verificabile.
  *(cap. 03.6; `FinestraModificaTesti.MostraICampi`.)*
- ✅ **Nelle Impostazioni compare una barra di scorrimento orizzontale che non serve** *(aperta
  il 2026-08-23 verificando la cura di R11, chiusa il 2026-08-24 col ramo delle rifiniture
  prima del giro D)*. Quando si scorre, la fila dei controlli **si fa due volte**: la prima
  dice quanto verrebbe alta la finestra, la seconda la rifà dentro la larghezza che resta
  tolta la barra verticale (`ScalaSchermo.LarghezzaSenzaLaBarra`). Prima il contenuto arrivava
  fino al margine di 14 pixel del disegno, mentre quella barra ne prende **17** a 96 DPI e
  **26** a 150%: di lì la seconda barra, che non aveva niente da mostrare. La domanda non si
  riapre — righe che vanno a capo prima possono solo far crescere l'altezza, e uno scorrimento
  che serviva serve ancora — e la riserva si prende **solo** quando si scorre, o stringerebbe
  la finestra per una barra che non c'è. Per poterlo collaudare la disposizione **riceve** lo
  spazio in altezza invece di leggerlo dallo schermo: un banco non può cambiare schermo, ed è
  proprio quando il contenuto non ci sta che questa disposizione fa qualcosa di diverso.
  *(cap. 03.4; `FinestraImpostazioni.DisponiIn`; `ScalaSchermo`.)*

- ✅ **Il pannello del logo a DPI alti** *(aperta il 2026-08-09 dalla revisione adversariale,
  misurata il 2026-08-23 dal collaudo dal vivo di T9e, **chiusa lo stesso giorno** dal quinto
  tempo)*. **Curato.** Le costanti di geometria sono in pixel non scalati:
  a 125/150% di scala — l'impostazione di fabbrica di quasi tutti i portatili recenti — il
  disegno è fuori misura. Difetto vero, ma correggerlo alla cieca rischiava di rompere il
  layout **validato a video** in T3: serviva uno schermo su cui verificare a 150%.
  **Verificato il 2026-08-23** (collaudo dal vivo di T9e, giro B, a 144 DPI di sistema): il
  sospetto era giusto, e per una giornata la voce è rimasta aperta col difetto **provato ma
  non curato**.
  Il pannello non passa in compatta e resta in modalità piena, sfondando nell'area viva:
  copre due righe della coda in P1 e 71 px di «Apri la candidatura», e in P6 l'angolo
  basso-sinistro della casella «Annuncio» e 71 px di «Torna al profilo». Le cause misurate
  sono due, tutte e due di unità di misura: la soglia della compatta confrontava pixel
  **fisici** con un numero pensato a 96 DPI, e l'ingombro dichiarato ai pannelli (261×216)
  era quello di progetto mentre il pannello vero misurava 373×360. *(I numeri di riga che
  stavano qui sono stati tolti il 2026-08-23: la cura ha spostato quel codice, e una riga
  citata che non è più quella inganna chi la va a controllare.)*
  La cura è nel **quinto tempo di T9e**, insieme alle due sorelle qui sotto — sono la stessa
  specie di errore. *(cap. 03.5 e 15.7; segnalato dalla revisione, misurato dal collaudo dal
  vivo di T9e.)*
  **La cura, e cosa ha insegnato** *(2026-08-23)*: le due cause erano due malattie diverse. La
  soglia della compatta ora si confronta in **unità di progetto**, e a 150% su 1920 la
  compatta scatta — misurata **186×160** invece di 373×360, con «Apri la candidatura» che
  comincia a x=238 mentre il pannello finisce a 197. L'ingombro dichiarato ai pannelli invece
  non si è convertito: si è **tolto**, perché era una costante che copiava una misura già
  posseduta dal runtime, e ora si legge il pannello vero a ogni ridimensionamento. Il seguito
  non era previsto: quello stesso numero sbagliato faceva credere alla fascia dei comandi di
  avere 125 px in più di spazio, ed è **per questo** che i comandi non andavano a capo e si
  sovrapponevano al minimo. Un difetto ne teneva in piedi un altro in un punto lontano.

- ✅ **La finestra Impostazioni su uno schermo piccolo** *(aperta il 2026-08-21 alla chiusura
  di T9b, verificata e **chiusa il 2026-08-23** dal quinto tempo di T9e)*. **Curata.** È alta quanto le serve — le
  sezioni si dispongono in fila, e i testi cambiano lunghezza con quel che c'è nella
  cartella dati — e sulla macchina di sviluppo ci sta comoda. Su un portatile a 768 px di
  altezza, o al 150% di scala, no: e la finestra è a misura fissa, senza scorrimento.
  **Verificato il 2026-08-23** (collaudo dal vivo di T9e, giro B): era il timore giusto, ed
  è andata peggio del previsto — **metà finestra è irraggiungibile**, ed è il reperto più
  grave del giro. `FinestraImpostazioni.vb:574` la dimensiona sul **contenuto** senza
  guardare quanto schermo ci sia, e in tutta la finestra non c'è **nessun `AutoScroll`**:
  il sistema la tronca a 682×1106 (il suo massimo) mentre il contenuto arriva a y=1384, su
  un'area di lavoro alta 1008. Restano fuori «Apri modelli.json», **«Backup…»**, **«Svuota
  i dati di navigazione»**, **«ELIMINA TUTTI I DATI»** e il bottone **«Chiudi»** — e cadono
  fuori dalla **finestra**, non solo fuori dallo schermo, quindi nessuno spostamento li
  recupera. Quel che serviva era chiaro fin da subito — un tetto sull'area di lavoro **e**
  uno scorrimento, le due cose insieme — ed è quel che il quinto tempo ha fatto.
  *(cap. 03.4 e 15.7.)*
  **La cura** *(2026-08-23)*: tre mosse che vanno insieme — larghezza dichiarata in pixel veri
  (cruda stringeva la finestra di un terzo mentre i testi crescevano col DPI, ed era lì che
  nasceva metà dell'altezza di troppo), un tetto sull'area di lavoro, e l'`AutoScroll` per quel
  che resta sotto. Misurata **1012×1008** contro i 682×1106 di prima, con ogni comando
  raggiungibile scorrendo. La stessa riga viveva identica in **altre tre finestre** — Backup,
  ChiaveApi, ConfermaCritica — curate tutte: la ChiaveApi compare al primo avvio prima della
  finestra principale, e troncata lì avrebbe impedito di inserire la chiave.

- ✅ **«Svuota i dati di navigazione» premuto dal vivo** *(aperta il 2026-08-21 alla chiusura
  di T9b, ritentata il 2026-08-23 e **chiusa lo stesso giorno**, subito dopo la cura di R11)*.
  **Passa.** Il giro di prova non è
  mai passato dalla ricerca annunci, quindi una cartella `webview2\` non è mai esistita e
  il bottone è rimasto spento — correttamente, ma non provato. Il banco lo copre; a
  mancare è la prova che il browser incorporato **lasci davvero cancellare** i suoi file,
  che è esattamente il caso che il codice prevede e spiega. Serve un giro che apra P3
  prima. **Ritentato il 2026-08-23** (collaudo dal vivo di T9e, giro B) e **non riuscito per
  una ragione nuova**: la cartella `webview2\` finalmente esiste, quindi il bottone sarebbe
  acceso, ma a 150% è **irraggiungibile** per il difetto della voce qui sopra — sta a y=1211
  dentro una finestra alta 1106, cioè fuori *finestra*, non fuori *schermo*. **Deciso con
  Mirco**: si preme nel **quinto tempo, a 150%, subito dopo la cura di quel difetto**, così
  la stessa prova chiude questo debito e verifica la cura appena fatta. *(cap. 11.5.)*
  **La prova** *(2026-08-23, a 150%)*: curata la finestra, il bottone è tornato raggiungibile
  scorrendo, la conferma ha spiegato cosa sarebbe sparito, e alla risposta «Sì» i **183 MB** di
  `webview2\` sono stati cancellati davvero. L'app ha scritto «Dati di navigazione svuotati.» e
  il bottone **si è spento da sé**, perché non c'era più niente da svuotare. Era il caso che il
  codice prevedeva e spiegava: il browser incorporato lascia cancellare i suoi file quando la
  ricerca annunci non è viva. La stessa prova ha chiuso il debito e verificato la cura, che era
  esattamente il motivo per cui si era deciso di farla qui.

- ✅ **Il confronto ogni tanto salta i giudizi di «contesto»** *(aperta il 2026-08-21 dal
  collaudo di tappa di T8, chiusa il 2026-08-23 dal collaudo dal vivo di T9e, giro C)*. La
  voce chiedeva di **misurare prima di curare**, perché «uno su quattro non è ancora un
  dato», ed è esattamente quel che è stato fatto: otto giri a T9d — quattro per porta, tutti
  5 su 5, per cui il Pool non fu toccato — e il **nono** dal vivo su una candidatura vera,
  ancora 5 su 5. Nove giri buoni di fila dopo l'unico caso storto: il prompt non ha niente da
  correggere, e la voce si chiude senza un bump. *(cap. 04.7;
  `prompt-pool/confronto/confronto.md`.)*

- ✅ **La fascia di P6 a DPI alti** *(aperta il 2026-08-22 alla chiusura di T9d, chiusa il
  2026-08-23 dal collaudo dal vivo di T9e, giro B)*. **Regge.** A 150% l'etichetta
  «📄 Documento:» (x 40, larga 160), la tendina (x 206, larga 591), «Lingua:» (x 823) e la
  sua tendina finiscono attorno a x 1100, contro un bordo a 1902: nessun taglio, e la
  cornice d'accento è al suo posto. Il timore era che la lingua uscisse dal bordo, e non
  esce. Sulla stessa pagina si vede un difetto, ma non è suo: è il **pannello del logo**
  che le sconfina addosso. *(cap. 03, tabella dei pannelli, P6.)*

- ✅ **La finestra a misura ridotta, guardata per davvero** *(aperta il 2026-08-19 dalla
  passata sui debiti prima di T9, chiusa il 2026-08-23 dal collaudo dal vivo di T9e, giro
  B)*. Il dubbio era doppio, e tutte e due le metà hanno risposto. Primo: ridimensionare via
  API vale come trascinare col mouse — sì, perché la `MinimumSize` la applica **Windows**,
  non lo strumento, e chiedendo una misura più piccola del minimo la finestra si ferma
  esattamente lì. Secondo: al minimo il contenuto **non** si taglia e la barra di navigazione
  **resta** — l'osservazione del 19 agosto nasceva proprio dalla manovra ingannevole su una
  finestra massimizzata, com'era il sospetto. Ne è però uscito un difetto diverso e più
  preciso, che vive ora nel quinto tempo di T9e: a 150% la `MinimumSize` non si scala di
  1,5 uniforme ma di 1,42 in larghezza e 1,605 in altezza (`AutoScaleMode.Font`), così il
  minimo vero scende a **1088,7 px logici** — 61 sotto quello di progetto — e in quei 61 px
  la fascia comandi di P6 si accavalla per 77×44 px: il clic su «Prepara email» va a
  «Modifica i testi». *(cap. 03.4 e 15.7; `FormPrincipale.Designer.vb:371`.)*
  **Curato anche questo il 2026-08-23**, dal quinto tempo, e con una sorpresa: il minimo si
  rimette in pixel veri dopo la scalatura automatica — misurato **1725×900**, cioè i 1150×600
  esatti — ma la sovrapposizione non veniva da lì. La fascia calcolava lo spazio disponibile
  col **vecchio ingombro del logo**, credendone 125 px in più di quelli veri, e concludeva che
  i comandi ci stavano su una riga sola. Dichiarato l'ingombro misurato, va a capo da sé: i
  due bottoni sono tornati distanti 90 px, su righe diverse. Un difetto ne teneva in piedi un
  altro in un punto lontano, ed è il motivo per cui il minimo **non** è stato alzato.

- ✅ **«⚙ Impostazioni» premibile mentre l'AI lavora** *(aperta il 2026-08-21 alla chiusura
  di T9b, chiusa il 2026-08-22 col primo tempo di T9d)*. Il difetto non era un bottone
  dimenticato: erano **due elenchi** di bottoni che dicevano cose diverse — `BarraDiNavigazione`
  ne contava quattro a mano, `BottoniDiNavigazione()` cinque — e adesso ce n'è uno solo. Si
  spegne l'intero bottone e non «solo le due pulizie», perché da P8 si esce in **tre** modi
  che una chiamata in volo non regge: i dati eliminati, la chiave nuova (che rimonta il
  contesto sotto una chiamata viva) e la cartella documenti (che manda in P7 e avvia un
  secondo giro di AI). *(cap. 03.8; cap. 11.5; cap. 14, T9d.)*

- ✅ **Lo stato «esito» e il follow-up** *(aperta il 2026-08-14 alla chiusura di T6,
  ridotta il 2026-08-18 col destinatario, chiusa il 2026-08-21 con T9c)*. Gli esiti
  registrabili sono **tre** e non i quattro del capitolo — «in attesa» era già lo stato
  `inviata`, e registrarla avrebbe creato due modi di dire la stessa cosa — e si segnano da
  P4 con «Com'è andata…», che li corregge e li toglie: sono una dichiarazione dell'utente,
  non un fatto osservato. Il promemoria è arrivato con la decisione che mancava, **dopo
  quanti giorni**: quattordici, modificabili in P8, zero per spegnerlo. Nella Home si vede
  in tre punti — la riga sotto i contatori, i giorni di attesa nella colonna «Stato», la
  voce «Da sollecitare» nel filtro. *(cap. 07.3, «Com'è andata, e chi aspetta da troppo».)*
- ✅ **Lo stato «nuova» è un vicolo cieco nella finestra** *(aperta il 2026-08-21 dal
  collaudo di tappa di T8, chiusa lo stesso giorno con T9c)*. Si è scelta la strada che non
  tocca lo schema su disco: sulla candidatura riaperta al solo annuncio «Analizza» **diventa
  «Confronta»** e fa il secondo passo da solo, perché l'annuncio è già strutturato e
  rileggerlo sarebbe costata una chiamata per riottenere quel che c'era. Provato dal vivo su
  una candidatura nata davvero dal server MCP: trentasei secondi, quindici giudizi, e la
  cartella che avanza a «interessante». *(cap. 03.6, P4; cap. 07.3.)*

- ✅ **L'interruttore della rifinitura** *(aperta il 2026-08-18 con la chiusura di T7b,
  chiusa il 2026-08-21 con T9b)*. È arrivato col pannello che lo ospita: «Rifinisci i testi
  generati (anti-slop)» in P8. Due cose non erano scontate e sono state decise lì. Vale
  **subito**, senza riavviare, perché la rifinitura non riceve più un valore copiato
  all'avvio ma una domanda da rifare a ogni giro — la finestra salva appena si cambia, e la
  generazione che parte dopo deve già saperlo. E vale da **tutte e due le porte**, finestra
  e server MCP: il cap. 09.3 vuole che il CV chiesto da un client sia lo stesso che esce
  dalla finestra, e un interruttore valido solo di qua li farebbe divergere proprio sul
  testo. Spenta, l'AI **non viene chiamata affatto**: interrogare il modello per poi
  buttarne via la risposta costerebbe soldi e tempo a chi l'ha spenta apposta.
  *(cap. 08.4; cap. 03, tabella dei pannelli, P8; cap. 14, T9b.)*

- ✅ **Il tredicesimo tool visto da un client MCP vero** *(aperta il 2026-08-21 con la
  chiusura di T9a, chiusa lo stesso giorno)*. Bastava il riavvio che la voce chiedeva: i
  tool `mcp__trovalavoro__*` sono comparsi **tredici**, e `esporta_backup` ha lavorato sui
  dati veri. Senza parametri fa quel che promette — «profilo» — e crea da zero la cartella
  `backup\`; con «tutto» porta via il registro e **nove candidature su nove**. Dentro c'è
  l'intestazione di formato (`formato_backup: 1`) e la data del profilo che viaggia **nel
  file** (`profilo_salvato: 2026-08-17 12:50:03`), cioè la cura del difetto trovato poche
  ore prima. Fuori restano la chiave API — nessun frammento di `segreti.bin`, né binario né
  in base64 — e i documenti impaginati: di ogni candidatura il backup porta via tutti i
  `.json` che ha (dai due della più magra ai sette della più completa) e lascia fuori la
  sola cartella `out`. Provato a farlo fallire, come vuole la regola 14: un `contenuto`
  inventato viene **rifiutato spiegando** i due valori validi e il predefinito, senza
  scrivere niente; a finestra aperta il lucchetto nega la scrittura e dichiara cosa
  funziona lo stesso — vero, `leggi_registro` ha risposto — e chiusa la finestra la stessa
  chiamata riesce; un file già esistente **non viene sovrascritto**, gli
  nasce accanto un `_2` identico a parte la data. Nessun difetto trovato, nessuna riga di
  codice toccata. *(cap. 09.3; cap. 11.4; cap. 14, T9a; regola 15.)*

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
