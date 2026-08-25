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
*Aggiornamento del 2026-08-25 (passata della regola 16): delle tre, l'**icona è stata
fatta** — è arrivata col primo tempo di T9e (`Risorse/TrovaLavoro.ico`, dichiarata nel
`.vbproj` e incorporata nell'eseguibile), e questa riga ha continuato a darla per mancante
per un giorno. Le altre due restano, e stanno in `in_sospeso.md`: le chiude il giro D.*

### T2 — Il motore e il pool — ✔ **CHIUSO il 2026-08-07**
`Ai/LibreriaPrompt` (pool esterno + integrato, manifest, segnaposto);
`Ai/ClientClaude` (**chiamate sincrone**, retry, timeout) verso **Haiku 4.5**
(estrazione) e **Sonnet 4.6** (ragionamento), cioè gli **stessi modelli del prototipo**:
il confronto si fa a parità di modello, così una differenza nei risultati è una
differenza di codice e non del modello sotto. Il salto a **Sonnet 5** (cap. 15, voce 6)
è il **secondo esperimento**: si fa da `modelli.json` senza ricompilare e porta con sé
l'interruttore del ragionamento esteso, che lì va acceso (cap. 02.5).
*Aggiornamento del 2026-08-25 (passata della regola 16): **è andata al contrario**, e il
cap. 02.5 — citato qui sopra come fonte — dice l'opposto di questa riga. Su Sonnet 5
l'interruttore va **spento** (`thinking: {"type": "disabled"}`), perché `max_tokens`
limita ragionamento e risposta **insieme**: lasciarlo acceso tronca le risposte senza che
l'API dia errore, e un confronto troncato produce JSON invalido. Così è stato fatto il
2026-08-18 (`Ai/Modelli.vb`, `RagionamentoEsteso = False`). La frase resta com'era scritta,
perché è quel che si credeva a T2; il rinvio, però, mandava a un capitolo che la smentisce.*
Lo **streaming non è di questa tappa**: arriva con T4/T7, quando ci sarà un pannello
che lo mostra.
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

*Fra T3 e T4 (2026-08-09) è passata una **revisione adversariale** fuori piano, su
mandato di Mirco: una ventina di difetti chiusi nel motore e nei pannelli (crash,
perdite di dati alle cuciture, buchi dell'anti-perdita), il **Pool 1.02** sugli otto
prompt del profilo (lingue, domicilio, patentini, guardia anti-injection), e la
validazione sul modello vero. La batteria è salita a **205 collaudi verdi**. Non è una
tappa: è manutenzione straordinaria fatta prima di aprire T4, narrata nel diario
(Step 2.8) e nel `CHANGELOG.md` del pool.*

### T4 — La pipeline di candidatura (F3 + F4 + F5 in italiano) — ✔ **CHIUSA il 2026-08-11**
Analisi annuncio da testo incollato; confronto con stelle, note e ⛔; pannello P4;
generazione CV-1/CV-2/lettera in italiano; mitigazione; export **DOCX e PDF**
(scrittore OOXML + stampa via WebView2); pannello P6.
**Collaudo:** end-to-end su un annuncio vero: dal testo incollato ai file DOCX/PDF
aperti in Word/LibreOffice, con verifica campo-per-campo del contenuto.

**Nessun prompt nuovo**: tutti e sei quelli che servono — `analisi_annuncio`,
`confronto`, `mitigazione`, `cv_base.it`, `cv_mirato.it`, `lettera.it` — sono nel pool
dal Pool 1.00 e non sono mai stati toccati. I `✚` del cap. 04.3 appartengono a T6 e T7.
È la prima tappa che *consuma* il pool senza doverlo scrivere.

**Il cancello d'ingresso, passato il 2026-08-10 prima di ogni riga di codice.** T4
introduce **WebView2**, l'unica libreria del progetto che porta codice nativo dentro un
exe che deve restare uno solo: se avesse rotto il single-file sarebbe cambiato il
disegno della stampa PDF (cap. 05.5), e andava saputo il primo giorno, non l'ultimo —
la stessa logica con cui T1 provò subito la pubblicazione. Provato a vuoto, fuori dal
repo: **117,2 MB in un file solo** (+1,2 MB sullo scheletro), l'exe funziona anche con
la cartella dei dati di navigazione cancellata, e la WebView **fuori schermo** stampa un
PDF con testo selezionabile e accenti in chiaro. Un difetto trovato e chiuso: il
pacchetto depositava **tre `.xml`** accanto all'exe (cap. 13.2).

La tappa si spezza in tre, **motore prima dell'interfaccia** come T3, perché lì ha
funzionato e T4 è più grossa:
**T4a — la pipeline nel motore**: i tre mestieri AI (`AnalizzatoreAnnuncio`,
`Confrontatore`, `Generatore`), `PipelineCandidatura` che li mette in fila,
`Motore/Opportunita` e `Dati/ArchivioOpportunita` che la scrive su disco. Tutto
collaudabile senza rete con i sostituti finti, come il dialogo a T3a.
**T4b — le stampanti**: `ScrittoreDocx` (ZIP OOXML) e `StampantePdf` (WebView2), col
modello di impaginazione condiviso. Qui il giudice non è più il prototipo, che i
documenti non li sa scrivere: sono Word e LibreOffice.
*Chiusa il 2026-08-10.* In mezzo alle due stampanti è nata la **pagina di blocchi**
(cap. 05.3), che è ciò che rende identico il contenuto dei due formati per costruzione —
e un collaudo mette DOCX e HTML uno accanto all'altro a verificarlo. Poi i **nomi
parlanti** dei file e `ArchivioDocumenti`, che sa dove vanno (cap. 05.6). Banco:
**252 → 301 collaudi** verdi, più uno nuovo fra i «Reale» per il PDF vero. I giudici
esterni hanno parlato: **LibreOffice** apre e riconverte DOCX e PDF con accenti e simboli
intatti, il **PDF** ha testo selezionabile (`/ToUnicode`) e font incorporati
(`FontFile2`), e il publish resta **un file solo** (117,5 MB) con WebView2 dentro.
**Word manca su questa postazione**: la sua metà della gamba C resta in `in_sospeso.md`,
dov'era già la voce gemella di T3.
**T4c — i pannelli**: P4 con la sua fascia d'ingresso (cap. 03.6) e P6, più il filo che
li lega a P2 e al 📄 CV-1 base.
*Chiusa il 2026-08-10.* «Analizza» fa **due passi in fila** — analisi e confronto — perché
in mezzo l'utente non decide niente (cap. 12, A5→A7), e a confronto fatto la fascia si
richiude. Sono nate le due **viste di sola lettura** promesse a T4a (`VistaConfronto`,
`VistaAnnuncio`) e la terza stampante, `ScrittoreTesto`, per l'anteprima a video
(cap. 05.3). In barra è comparso il quinto bottone, **📋 Candidatura**: senza, il pannello
di questa tappa non era raggiungibile (cap. 03.4). Banco: **301 → 350 collaudi** verdi.

Qui è anche nato lo **strumento di collaudo** (`strumenti/mcp-collaudi/`, cap. 09.1): un
server MCP locale che compila, fa girare il banco, avvia l'applicazione vera, la fotografa
e le preme i bottoni. Non è parte del prodotto. È nato il giorno in cui «i bottoni non
fanno nulla» non si poteva diagnosticare dal banco — il banco vede lo *stato* dei
controlli, non come si *vedono* — ed è lo strumento con cui il collaudo di tappa qui sotto
è stato percorso dall'interfaccia, senza mani.

Il **collaudo di tappa** è in tre gambe, come a T3, perché le domande sono di nuovo
diverse fra loro. **Condotto il 2026-08-10** con l'AI vera e il CV vero di Mirco; il
rapporto sta accanto al CV, fuori dal repo, perché contiene dati personali.

- **A — il prototipo come giudice, sull'intera pipeline.** Ha tutti gli endpoint che
  servono (`/struttura`, `/confronta`, `/mitiga`, `/genera-cv`, `/genera-lettera`), e su
  `confronto` e `mitigazione` è ancora il **metro carattere-per-carattere** (cap. 04.7).
  *Fatto:* 9 collaudi di parità verdi sul metro; batteria «Reale» **10 su 10**; e sulla
  **generazione**, dove nessun collaudo automatico arriva, gli stessi input dati alle due
  parti — forma dello schema identica, **gli stessi quattro gap** mitigati, stessi
  conteggi nel CV-2, **nessuna invenzione** da nessuna delle due parti, e le due lettere
  che nominano gli stessi tre gap. Le differenze sono di lunghezza, non di sostanza.
- **B — la pipeline reale end-to-end**, dal testo di un annuncio vero fino ai tre
  documenti, col profilo vero. *Fatto*, e percorso **dall'interfaccia**: import del CV in
  PDF → profilo salvato → annuncio incollato in P4 → «Analizza» → **1,4 su 5** su 14 voci
  → mitigazioni → 🎯 CV-2 e lettera in P6 → export; più il 📄 CV-1 base generato da P2.
  *Limite dichiarato*: l'annuncio era verosimile ma scritto per il collaudo, non pescato
  da un portale (`in_sospeso.md`).
- **C — i file**: DOCX e PDF aperti in Word e LibreOffice, testo estratto e confrontato
  **campo per campo** col JSON di partenza (cap. 05.7). *Fatto:* **114 campi su 114**
  ritrovati nei sei file, e i due formati identici carattere per carattere una volta tolti
  spazi e segni. Il confronto è stato **messo alla prova**: cambiando una lettera al nome
  di un'azienda diventa subito rosso. **Word manca su questa postazione** e la sua metà
  resta in `in_sospeso.md`, dov'era già la gemella di T3 e T4b.

**Voci di backlog prese dentro questa tappa** *(decise con Mirco il 2026-08-10)*, perché
T4 le tocca comunque e chiuderle altrove costerebbe di più:
- la **parità del prompt estesa** dagli attuali uno a tutti i sei prompt che T4 usa —
  oggi il banco verifica carattere per carattere solo `confronto`, e proprio a T4 gli
  altri cinque entrano in produzione (`idee_future.md`).
  ✔ *Fatta, ma su **due** prompt invece di sei, e la revisione è la parte che conta*: la
  parità carattere-per-carattere ha senso solo dove il prototipo è ancora il **metro**, e
  sono `confronto` e `mitigazione` (cap. 04.7). Su `analisi_annuncio` il distacco c'è già
  (Pool 1.03) e sui tre della generazione arriverà: inchiodare quei prompt a un testo che
  vogliamo poter cambiare avrebbe trasformato un collaudo in una gabbia.
- la **validazione di range della taratura**: T4 è la prima tappa che usa `CalcoloMatch`
  sul serio dentro l'app, e oggi un `"clamp_su": -50` entrerebbe zitto. ✔ *Fatta*
  (cap. 11.6), con tre collaudi che la provano.
- la **città pass/fail in `CollaudiFormatiReale`**, allineamento meccanico rimasto
  indietro dal Pool 1.02. ✔ *Chiusa il 2026-08-11, e non come previsto*: il collaudo di
  tappa ha mostrato che la stessa riga «Carasco (GE)», identica in tutti e quattro i file,
  torna a volte con la provincia e a volte senza — due letture **entrambe fedeli**, perché
  il prompt dice quale indirizzo prendere, non come scriverlo. Fra le strade si chiede
  quindi che sia la **stessa città**, ammettendo la sola sigla di provincia; il pass/fail
  vero resta quello contro il CV. È nato lì anche il primo banco degli **attrezzi di
  misura** (`CollaudiMetroReale`), che prima non ne avevano — ed è servito subito, perché
  ha bocciato la prima versione della regola.
- la **trappola latente di `Sigilla`** sul `CHANGELOG` con una riga `---`. ✔ *Chiusa*, col
  suo collaudo.

**Fatto:** **355 collaudi verdi** senza rete e **10 reali** su aviolab03. **T5 può
iniziare.**

### T5 — La ricerca annunci (F2) e il registro (F6) — ✔ **CHIUSA il 2026-08-13**
Pannello P3 con WebView2, ricerche salvate, cattura dell'annuncio, coda delle
opportunità; cartelle-opportunità su disco; pannello P1 Home con registro e stati.
Verifica sul campo degli schemi di indirizzo dei portali del primo rilascio.
**Collaudo:** su **Indeed / Jooble / Subito.it** reali *(era InfoJobs: chiuso, v. T5a)*:
login manuale dove serve,
cattura di annunci veri, rifiuto garbato delle pagine-elenco; riapertura dell'app con
stato intatto.
*Fatto per le prime tre voci il 2026-08-12, con T5b: i quattro portali percorsi dal menù
uno per uno, un annuncio vero di Indeed analizzato e salvato con la sua provenienza, il
rifiuto garbato visto scattare sulla griglia di Subito.it e su una pagina senza offerte.
La **riapertura con stato intatto** è arrivata con T5c: l'applicazione chiusa e riaperta
mostra le sei candidature reali di Mirco al loro posto, con gli stati giusti — e la
tappa che costruisce la vista è la stessa che permette di verificarla.*

**Il pedaggio è già pagato.** T5 introdurrebbe WebView2, l'unica libreria con codice
nativo dentro un exe che deve restare uno solo — ma il cancello l'ha attraversato **T4**
il 2026-08-10 per la stampa PDF: il pacchetto è già nel progetto, il publish resta un
file solo, e la WebView gira già in produzione. Qui non c'è un rischio nuovo da provare
il primo giorno; c'è un componente noto usato in modo nuovo — visibile e navigabile
invece che fuori schermo.

La tappa si spezza in tre *(deciso con Mirco il 2026-08-12)*. Il criterio **non** è
quello di T3 e T4 — motore prima dell'interfaccia — perché qui il motore *è*
l'interfaccia: la cattura non esiste senza una pagina vera aperta in un browser vero, e
una WebView2 non si sostituisce con un finto come si è fatto per l'AI. Il taglio segue
allora l'ordine in cui le cose si accendono, ciascuna appoggiata alla precedente:
**T5a — il browser e le ricerche salvate** — ✔ **CHIUSA il 2026-08-12**: pannello P3 con
la WebView2 a tutta area e il suo profilo di navigazione nella cartella dati (cap. 6.6);
`ricerche.json` con la tabella dei portali e le ricerche salvate (cap. 6.3); il campo del
link diretto (cap. 6.5). Qui si verificano **sul campo gli schemi di indirizzo** dei tre
portali del primo rilascio (cap. 15, voce 7). Alla fine il bottone **🔍 Ricerca** della
barra si accende: oggi è lì spento, col tooltip «arriva con la tappa T5» (regola 3.8).
*Com'è andata: la verifica sul campo ha trovato **InfoJobs chiuso** e l'ha sostituito con
Jooble (cap. 6.3), e un cancello passato prima di scrivere il pannello ha stabilito che
l'ambiente WebView2 dev'essere **uno solo** per tutta l'applicazione (cap. 05.5, cap. 02.3).
La tappa si è chiusa con un buco dichiarato — il giro che parte dal **menù dei portali**
non era mai stato percorso sull'applicazione vera, perché lo strumento di collaudo non
sapeva scegliere una voce da una tendina — chiuso poi il 2026-08-12 insegnandoglielo, e
percorso su tutti e quattro i portali.*
**T5b — la cattura** — ✔ **CHIUSA il 2026-08-12**: titolo, URL e testo visibile letti dal
DOM della pagina che l'utente sta guardando, passati ad `AnalizzatoreAnnuncio` — che
esiste da T4a e non va toccato — e da lì alla coda delle opportunità; più il rifiuto
garbato delle pagine che un annuncio non lo contengono (cap. 6.4). La fascia incolla-testo
di P4 non sparisce: le si affianca, e resta di prima classe (cap. 03.6, cap. 12.3).
*Com'è andata: il testo catturato **entra proprio in quella fascia**, così si vede; ogni
opportunità porta con sé fonte e link (cap. 11.1); la stessa pagina catturata due volte
non genera due candidature gemelle. Il collaudo sul campo ha analizzato un annuncio vero
di Indeed e ha visto il rifiuto scattare sulla griglia di Subito.it — e ha corretto il
punto 3 del cap. 6.4, che prometteva un comportamento uniforme che i portali non hanno.*
**T5c — la Home e il registro (F6)** — ✔ **CHIUSA il 2026-08-13**: pannello P1 con lo
stato del profilo, la coda delle opportunità con stelle e stati, le scorciatoie ai flussi
(cap. 03.6); `registro.json` e la macchina degli stati — nuova → interessante → generata →
inviata → esito (cap. 07.3) — che legge le cartelle-opportunità già scritte da T4
(cap. 11.1). Accende il bottone **🏠 Home** — ed è lì che l'applicazione ora si apre — e
chiude la voce in sospeso «la coda dell'opportunità non si riapre»: la promessa del
cap. 12.7 smette di essere mantenuta solo sul disco.
*Com'è andata: la decisione che regge tutto è che le **cartelle-opportunità sono la fonte
di verità** e `registro.json` è solo un **indice rigenerabile** — si ricostruisce quando
manca o non torna, e chi lo guarda è anche chi lo tiene in riga. Da lì discende il
trattamento delle candidature nate prima: quelle di T4 il campo `stato` non ce l'hanno, e
lo **deducono dai file che hanno** invece di farsi riscrivere all'indietro — riscrivere i
file dell'utente per aggiungerci un campo nostro sarebbe stata un'invasione. Gli stati
`inviata` ed `esito` esistono nello schema ma dall'interfaccia non si raggiungono: sono di
T6, e stanno lì perché quella tappa aggiunga dei passaggi e non una migrazione. Lo scarto
è **terminale e con conferma** (la cartella però resta: si scarta, non si cancella), e il
suo comando vive nella scheda che si sta guardando, non nella Home — che, come dice il
cap. 07.3, è il posto in cui si guarda, non quello in cui si decide.*

Il **collaudo di tappa**, condotto la notte fra il 12 e il 13 agosto, ha una gamba sola —
qui la domanda è una: *quello che è stato scritto ieri si ritrova domani?* Banco senza rete
**verde prima e dopo**, poi l'applicazione vera sulla cartella dati reale di Mirco: si apre
sulla Home con le **sei candidature** e gli stati dedotti giusti; una si riapre in P4 dal
bottone e dal doppio clic, coi suoi giudizi e le sue stelle — **anche quella di T4**, che
non ha né fonte né campo `stato`. Lo **scarto è stato provato davvero**: «No» su una
candidatura reale (e il disco è rimasto intatto, verificato a mano), «Sì» su una cartella
di prova fabbricata apposta — comparsa da sola nella coda, e sparita senza lasciare voci
fantasma una volta cancellata, che è la prova che l'indice si rigenera. Infine
l'applicazione **chiusa e riaperta**, con tutto al suo posto, scarto compreso. Nessuno
`stato.json` reale è stato riscritto.
*Due difetti trovati guardando, non collaudando* — ed è il motivo per cui il collaudo si fa
sull'applicazione vera: lo **strumento** accettava una chiamata a cui mancava un argomento
obbligatorio e premeva il primo bottone della finestra riferendo un successo (chiuso, con
la trappola annotata nel suo README); e i contatori della Home dicevano «1 scartate»,
perché la parola era fissa e il numero no. Il banco chiude a **452 collaudi** verdi.

Con T5b si chiude anche l'altro debito lasciato aperto da T4: **un annuncio davvero
pescato da un portale**, non verosimile ma scritto per il collaudo.

**Fatto:** **452 collaudi verdi** senza rete. I collaudi della categoria *Reale* restano
i **14** di T5b e non sono stati rilanciati qui: la Home e il registro non chiamano l'AI —
leggono il disco e mostrano — e un collaudo con la chiave non avrebbe verificato niente
che il banco non veda già. **T5d può iniziare.**

### T5d — Il profilo da LinkedIn (voce 2.1.3) — ✔ **CHIUSA il 2026-08-14**
*Rinominata il 2026-08-12: si chiamava «T5b», nome che le tre gambe di T5 hanno preso.
La tappa è la stessa, e resta una coda a sé dopo T5.*
Piccola coda di T5: cattura della **propria** pagina profilo LinkedIn dal browser
integrato e invio alla strutturazione `importa_cv` già esistente (cap. 06.7). Nessun
componente nuovo — riusa cattura e prompt di T5 e T3.
**Collaudo:** dalla pagina profilo reale di Mirco esce un profilo JSON coerente con
quello ottenuto dal suo CV in PDF.

*Com'è andata, in due pezzi.* Il primo è la **lettura della pagina**: in P3 «Importa CV
da questa pagina» legge quel che l'utente sta guardando e lo consegna a P2, che struttura
col turno `importa_cv` di sempre — la finestra mostra prima il pannello e poi gli chiede
di leggere, come per l'annuncio catturato, così l'attesa si vede dove succede. La
promessa «nessun componente nuovo» ha retto alla lettera, e **il pool non è stato
toccato**. Il secondo pezzo è la **porta in P2**: «Importa CV da un sito…» accanto a
«Importa CV da un file…» — perché fino a lì la terza strada per costruire il profilo
esisteva solo dentro un pannello chiamato «Ricerca», dove nessuno l'avrebbe cercata. Il
bottone non legge niente: porta in P3, dove vive il browser, e il pannello arrivando dice
cosa fare (cap. 06.7).

*Quel che solo la pagina vera poteva insegnare:* un profilo su un sito moderno **non
esiste finché non lo si scorre**. Letta com'era dava **2196 caratteri** — la sola
intestazione — contro **9681** dopo lo scorrimento. Due tentativi buttati hanno pagato
due misure: `window.scrollBy` lì non muove niente, e il primo «sono in fondo» è una
bugia. Lo scorrimento lo chiede **solo** l'import del CV; la cattura dell'annuncio legge
la pagina com'è, come era stata collaudata su quattro portali, e un collaudo tiene ferma
la distinzione.

Il **collaudo di tappa** è quello promesso qui sopra, e l'ha superato: sulla pagina vera,
con la chiave vera, ne esce un profilo che non contraddice quello ricavato dal CV in PDF —
dove differisce è perché differiscono le fonti, e i campi che LinkedIn non pubblica (email,
telefono) escono **vuoti invece che inventati**. Le altre cinque persone che la pagina
mostrava nei riquadri dei suggerimenti non sono entrate nel profilo. I dati reali sono
stati verificati **intatti prima e dopo** ogni prova, per impronta: profilo, storico e le
sei candidature identici, e l'applicazione chiusa senza passare dal salvataggio.

**Fatto:** **465 collaudi verdi** senza rete (erano 452), versione **0.3.016**. I collaudi
della categoria *Reale* restano i **14** di T5b: la lettura della pagina si prova col
lettore finto, e l'unica cosa che l'AI fa qui è il turno `importa_cv`, che ha già il suo.
**T6 può iniziare.**

*Dopo T5d, lo stesso 2026-08-14, è passata una modifica **fuori tappa** chiesta da Mirco:
le tre porte del profilo in P2 cambiano nome — «IMPORTA CV DA UN FILE», «IMPORTA CV DA
LINKEDIN», «COSTRUISCI IL TUO CV - DIALOGO GUIDATO» (cap. 03.6). I due nomi citati qui
sopra nella cronaca di T5d sono quelli con cui i bottoni erano nati. Non è una tappa e non
cambia nessuna funzione: banco fermo a **465 verdi**, versione **0.3.017**. Lascia però un
segno che vale la pena aver scritto — il bottone del dialogo è dovuto crescere di 100 px
per contenere l'etichetta, e la fascia più lunga peggiora la sovrapposizione a finestra
stretta già annotata in `in_sospeso.md` (diario Step 2.15).*

### T6 — Le email (F5 completo) — ✔ **CHIUSA il 2026-08-14**
Composizione con allegati suggeriti (inclusa la scansione della cartella documenti,
cap. 05.2); scrittura del file `.eml` con intestazione `X-Unsent`; conferma dell'avvenuto
invio da parte dell'utente e aggiornamento del registro.
*Tappa alleggerita dalle decisioni del 2026-08-05:* niente `.msg`, niente invio SMTP,
quindi niente pannello di configurazione del server né password di posta da custodire
(cap. 15, voci 8 e 9).
**Collaudo:** un'`.eml` generata, aperta in un programma di posta vero e spedita da lì,
con allegati integri; verifica che la chiave API non compaia né su disco in chiaro né
nei log.

*Com'è andata (2026-08-14).* La tappa è cominciata **saldando quattro debiti** di
`in_sospeso.md`, e tre servivano proprio qui: la cartella dati usa-e-getta (`--dati`), con
cui tutto il resto è stato collaudato senza mettere in gioco i dati veri; la **fascia dei
comandi che va a capo**, che ha retto il bottone in più di P7 e ha chiuso una
sovrapposizione arrivata a 676 px; il **filtro per stelle** e l'**esportazione del
registro**, promessi dal cap. 07.3 fin dal primo rilascio.

Poi i tre pezzi della tappa. **Il messaggio**: `email_candidatura` (Pool 1.04) accorcia la
✉️ lettera già scritta invece di rifarne una, `Documenti/ScrittoreEml` scrive a mano il
file MIME con `X-Unsent` — niente librerie, e tre trappole pagate: CRLF, intestazioni
ASCII con l'oggetto accentato da codificare, nome dell'allegato in forma RFC 2231 — e
**P7** mette insieme destinatario, oggetto, corpo e allegati, salvando la bozza in
`email.json`. Il **Pool 1.05**, poche ore dopo, corregge un esempio che contraddiceva la
sua regola: *in un'istruzione l'esempio pesa più della regola*. **La chiave API**: cifrata
in `segreti.bin` con la protezione dati di Windows, chiesta in una finestra al primo
avvio, rifacibile con `--chiave` (cap. 11.3) — ed è il debito che T3 aveva assegnato qui.
**La cartella documenti**: letta, classificata con una sola chiamata, confermata
dall'utente in una finestra, e da lì gli attestati compaiono fra gli allegati, spenti
(cap. 05.2).

Il **collaudo di tappa è passato per intero**, in due riprese. Dalla sessione: il file
`.eml` prodotto dall'applicazione vera validato da un **lettore di posta indipendente** —
intestazioni, `X-Unsent`, corpo integro, e l'attestato preso dalla cartella dell'utente
**identico byte per byte**; la classificazione che riconosce **nove file su nove** (PDF
compresi, dal solo nome); e la chiave che su disco non compare in chiaro — il file comincia
con la firma di DPAPI — né nella diagnostica, che la mostra come `sk-ant-…1234`.

La sera stessa, **da Mirco sui dati veri**, la parte che dalla sessione non si poteva fare:
la chiave digitata e salvata nella finestra del primo avvio; la **sua** cartella di
documenti personali indicata al dialogo di Windows — tredici file, i due CV riconosciuti e
tutto il resto in «altro», carte d'identità comprese, quindi **nessun documento personale
fra gli allegabili**; e la candidatura a Delta Sistemi **aperta nel nuovo Outlook e spedita
da lì**, arrivata con i suoi allegati. Un falso allarme lungo la strada — «non è stato
possibile allegare i file» — si è rivelato una **sessione dell'account scaduta** e non un
difetto del formato (cap. 07.2).

*E ha stanato due difetti veri, chiusi con il loro collaudo (diario Step 2.21):* la Home
restava a «generata» dopo un invio, perché **chi cambia uno stato deve annotarlo
nell'indice** e P7 non lo faceva (cap. 07.3); e il **banco apriva il programma di posta**
dell'utente, perché premeva il bottone che scrive il messaggio *e lo consegna* — ora le due
cose sono separate. Sono difetti che 598 collaudi verdi non potevano vedere: uno viveva fra
due pannelli, l'altro era il banco stesso.

**Fatto:** **599 collaudi verdi** senza rete (erano 478), versione **0.3.027**, Pool
**1.05**. Restano indietro, dichiarate: il destinatario proposto dall'annuncio, la porta
«qui c'è tutto» del profilo, lo stato `esito` col follow-up. **T7 può iniziare.**

### T7 — Multilingua e qualità (F4 completo) — ✔ **CHIUSA il 2026-08-18**
Varianti `en` dei prompt di generazione; campo `lingua` nell'analisi; prompt di
rifinitura anti-slop con prima/dopo in P6; brainstorming (P5) con streaming e appunti
di mira.
**Collaudo:** candidatura completa su un annuncio in inglese; lettura critica dei
testi prodotti (nessun tic da AI, nessun fatto inventato, nessun errore introdotto).

*Spezzata in tre, come già T3.* **T7a — la lingua** (2026-08-15): varianti `en` dei tre
prompt di generazione, campo `lingua` nell'analisi, tendina in P6, etichette dei documenti
tradotte; chiusa col collaudo reale su un annuncio Indeed in inglese, che ha lasciato
dietro il **Pool 1.07** (l'email impara l'inglese, oggetto compreso). **T7b — l'anti-slop**
(2026-08-18): la cartella `rifinitura/` del pool, **tre prompt in due lingue** invece
dell'unico previsto (cap. 08.6), la passata dentro la pipeline — che passa da quattro a sei
passi — e la casella del prima/dopo in P6, che era lì spenta da T4. **T7c — il
brainstorming** (2026-08-18): i due prompt nuovi `brainstorm` e `appunti_di_mira`
(**Pool 1.10**), lo **streaming SSE** che il cap. 02.5 teneva in serbo da T2, P5 che
guadagna una seconda modalità, e gli appunti confermati che entrano nei prompt di
`cv_mirato` e `lettera` in due lingue.

*Il collaudo di T7b (2026-08-18).* Il capitolo chiedeva «lettura critica dei testi
prodotti»: è stata fatta con una **griglia decisa prima** di guardare i risultati — grado
rafforzato, fatti comparsi, cose sparite, forma nominale, gap attenuati, formule rotte,
refusi, cambiamenti dove non servivano — su un giro intero **nelle due lingue**, con un
annuncio costruito apposta con requisiti scoperti. Ha retto la sostanza: nessun grado
rafforzato, nessun fatto inventato, gap onesti al loro posto (in inglese perfino resi più
espliciti), frasi nominali intatte, e quattro descrizioni su cinque **restituite identiche**.
Sono usciti **tre difetti dei prompt**, curati nel **Pool 1.09** e — regola nuova, imparata
qui — **riprovati con l'AI**, non dati per chiusi: la cura sulle lineette lunghe alla prima
riprova non aveva funzionato affatto (cap. 08.7). Lo stesso giro ha stanato un difetto d'uso
fuori dall'anti-slop, corretto subito: la bozza dell'email non sapeva in che lingua era
nata, e dopo un cambio di lingua P7 la riproponeva in silenzio (cap. 07.1).

*Il collaudo di T7c (2026-08-18).* Costruito per essere **sleale**: a metà conversazione si
è dichiarato di usare SAP Business One, che è un requisito *preferenziale dell'annuncio* —
l'esca migliore possibile per una regola che dice «i fatti vengono solo dal profilo». L'AI
ha risposto che quella cosa «deve entrare nel profilo prima di entrare nella candidatura»,
l'ha messa fra i **fatti nuovi**, e nei documenti generati non compare: zero occorrenze in
CV e lettera. I tre appunti confermati hanno funzionato tutti — il sommario si apre con
l'esperienza che si era chiesto di mettere davanti, la laurea non è nominata da nessuna
parte, il tono è asciutto. Un difetto trovato **guardando** e non misurando: l'etichetta
nuova di un bottone non ci stava nella sua misura e a video si leggeva a metà; corretta, e
accompagnata da un collaudo che la misura in numeri. Non provata dal vivo l'interruzione di
un turno (è al banco): in `in_sospeso.md`.

*La coda: **T7d — il 📄 CV-1 base** (2026-08-18).* Non era in programma: è nata da
un'impressione di Mirco — «ieri ho premuto Esporta e mi pare fosse spento» — verificata
sull'applicazione vera prima di discuterne. Era spento davvero, e il motivo stava sotto la
lingua: il CV-1 base viveva nella **sola memoria del pannello**, così ogni visita a P6 ne
generava uno nuovo e l'unico modo di riesportare quello di ieri era rifarlo — senza AI,
niente affatto. `ArchivioProfilo.CaricaCvBase` esisteva dal primo giorno, completo e
collaudato: **a chiamarlo erano soltanto i collaudi**, e la promessa del cap. 11.1 («l'app
dice che è di una versione precedente») non poteva essere mantenuta perché quella versione
non arrivava mai a video. Fatto: P6 ripesca il CV-1 base e racconta di dove viene; la
tendina della lingua si accende **anche su di lui** — cadeva la premessa che la teneva
spenta, non la regola — e con essa si chiude la coda di T7a, senza aggiungere nessun
controllo nuovo all'interfaccia; `cv_base.json` annota la lingua (i file di prima valgono
italiano) e quella lingua arriva fino al prompt `.en`, alla rifinitura anti-slop (che era
rimasta inchiodata all'italiano), alle etichette e alla sigla del nome file. Provato dal
vivo su una copia dei dati veri: rientro senza attesa, poi il CV-1 riscritto in inglese ed
esportato in `CV_..._EN_2026-08-18.docx` con «Work experience» dentro.

*La coda della coda: **Pool 1.11** (2026-08-18).* Il primo CV-1 inglese vero ha mostrato
un titolo di ruolo rimasto in italiano in mezzo a sette già inglesi. Il prompt voleva
tradurlo — la regola anti-upgrade nomina il ruolo, l'elenco dei nomi propri non lo elenca —
ma accanto al campo c'era «**Copy** ruolo, azienda and durata» senza il rimando alla
traduzione che competenze e formazione avevano. È la **terza** volta che il pool impara la
stessa legge (1.05, 1.07, 1.11), ed è la formulazione più asciutta: *fra un'istruzione
concreta accanto al campo e una regola generale in un'altra sezione, vince la concreta.*
Corretti `cv_base.en` e `cv_mirato.en`, **non** `lettera.en` (lì non c'è nessun «Copy» che
rema contro: un prompt che non ha il difetto non si tocca per simmetria). Riprovato con
l'AI vera, come insegna il 1.09: *Direttore Operativo* → **Operations Director**, nessun
salto a *COO*, e i **sette ruoli già inglesi restituiti identici** — che era il rischio
vero, cioè che «traduci» diventasse «riscrivi».

**Fatto:** **737 collaudi verdi** senza rete (erano 599 a fine T6, 726 a fine T7c),
versione **0.3.032**, Pool **1.11**. Restano indietro, dichiarate in
`in_sospeso.md`: la modifica a mano dei testi in P6, l'interruttore della rifinitura
(P8, T9), il prima/dopo dell'email, la lettura a video del prima/dopo fino in fondo e
l'interruzione del turno provata dal vivo.
**T7 è chiusa.**

*Fra T7 e T8 — **una passata su `in_sospeso.md`** (2026-08-18).* Non è una tappa e non ne
prende il nome: è la manutenzione prevista dalla regola 13 del `CLAUDE.md`, cioè rileggere
l'elenco di quel che è rimasto indietro e chiudere ciò che si può chiudere **da questa
macchina**. Ne sono uscite sei voci: il turno del ragionamento che poteva fermarsi a metà
frase senza dichiararlo (cap. 03.6, P5), il **diario dei consumi di token** che permette di
ritarare i tetti sui numeri veri invece che a naso (`chiamate_ai.csv`, cap. 02.5 e 04.4),
la bozza dell'email che si perdeva uscendo dalla **barra di navigazione** (cap. 03.8), il
**destinatario** entrato nell'indice del registro (cap. 07.3), l'attesa di una condizione
nello strumento di collaudo, e il cambio lingua fallito del 📄 CV-1 base percorso finalmente
dal vivo. Una settima l'ha trovata la revisione del diff prima che uscisse, ed era la
peggiore: una candidatura senza lettera **ereditava oggetto e corpo della precedente**, a
video e su disco (cap. 03.8). Quel che chiede una **seconda macchina o una mano** — l'exe su
un PC pulito, l'SDK sulla postazione del tutor, un `.docx` salvato davvero da Word, uno
schermo al 150% — è rimasto aperto: nominare un debito non è pagarlo.
**755 collaudi verdi** (erano 738), versione **0.3.032**, Pool **1.11** invariato.

### T8 — Il server MCP (F8)
Modalità `--mcp` su stdio; i tool del cap. 09; lucchetto di scrittura.
**Collaudo:** da Claude Desktop/Code: `tools/list` corretto, un confronto e una
generazione via MCP con risultati identici a quelli dell'interfaccia.

*Spezzata in tre gambe* (2026-08-19), come già T3 e T5: **T8a** il guscio — la modalità
`--mcp`, il dialogo, i tool di sola lettura; **T8b** i tool che passano dall'AI; **T8c**
quelli che scrivono, insieme al lucchetto della cartella dati.

**T8a — il guscio (2026-08-19).** Prima di scrivere si è riletta la **specifica**, e ne è
uscito il fatto che ha riscritto il capitolo: il **28 luglio 2026** MCP è diventato
**senza stato** — niente più handshake `initialize`, la versione del protocollo dentro il
`_meta` di ogni richiesta, `server/discover` obbligatorio — mentre il cap. 09 descriveva i
tre passi canonici di prima. Il server è perciò **dual-era**: parla la revisione
`2026-07-28` e quelle dell'handshake fino a `2025-11-25`, e riconosce quale a ogni
messaggio senza ricordarsi niente (cap. 09.2). Fatto: `--mcp` in `ArgomentiAvvio` e la
biforcazione in `Programma.Main` prima di ogni preparativo grafico; il ciclo su stdio in
`Mcp/`, con `Rispondi` staccato dal ciclo così che il banco possa interrogarlo senza
avviare un processo; i tre tool di lettura `leggi_profilo`, `leggi_registro`,
`leggi_opportunita`. **Il nodo sciolto**: un `WinExe` non ha una console, e che i suoi
flussi standard funzionino quando è il client a fornirli è ciò su cui poggia tutta la
modalità — provato da `CollaudiServerMcpDalVivo`, che avvia l'**eseguibile vero** con le
pipe e verifica che risponda, che su `stdout` non finisca nient'altro che protocollo e che
si spenga da sé alla chiusura dell'ingresso. Nessun lucchetto qui, e non per rimandare: i
tre tool leggono soltanto, e dopo un giro intero la cartella dati non risulta nemmeno
creata. **780 collaudi verdi** (erano 755), versione **0.3.033**, Pool **1.11** invariato.
Il collaudo di tappa dichiarato qui sopra — quello da un client MCP vero — resta da fare
ed è in `in_sospeso.md`.

**T8b — i tool che passano dall'AI (2026-08-19).** Sette tool — `analizza_annuncio`,
`confronta`, `mitiga`, `struttura_cv`, `genera_cv`, `genera_lettera`, `rifinisci_testo` —
sugli stessi mestieri e sugli stessi prompt del pool che usa l'applicazione. Il **profilo
si legge da disco** e non si passa come parametro, serializzato come lo serializza l'app:
su `confronto` e `mitigazione` la parità col prototipo si misura carattere per carattere
(cap. 04.7). **Quattro decisioni** hanno orientato la gamba (cap. 09.2 e 09.3): il ciclo
serve **più richieste insieme**, perché uno che aspetta un `genera_cv` di minuti non è
lento ma **sordo** — non leggerebbe nemmeno il `notifications/cancelled` che arriva
proprio allora; i tool dell'AI **restano in vetrina anche senza chiave** e falliscono
dicendo dove si mette; il **diario dei consumi** si tiene anche via MCP e si corregge il
capitolo che marcava quei tool «non scrive dati»; i documenti passano **sempre**
dall'anti-slop, perché il CV chiesto da un client dev'essere lo stesso che esce dalla
finestra. Il ciclo di T8a è stato perciò **rifatto**, non esteso: lucchetto sull'uscita,
mappa degli annullamenti, e alla chiusura dell'ingresso i lavori in volo si fermano invece
di essere portati a termine. `confronta` non avvolge la fila di T4 — quel passo giudica,
calcola e mitiga insieme, e fa avanzare lo stato dell'opportunità — ma la **ricompone** dai
pezzi sotto, col punteggio che resta del programma e non del modello (cap. 09.5).
**796 collaudi verdi** (erano 780), versione **0.3.034**, Pool **1.11** invariato. I tre
collaudi del ciclo sono stati **falsificati apposta**, rendendolo di nuovo seriale, per
verificare che parlassero davvero del parallelo. Resta da fare la parte del collaudo di
tappa che chiede «un confronto e una generazione via MCP con risultati identici a quelli
dell'interfaccia»: è in `in_sospeso.md`, perché nessun collaudo automatico chiama l'AI
vera.

**T8c — i tool che scrivono, e il lucchetto (2026-08-19).** Due tool — `salva_opportunita`
e `esporta_documento` — più il lucchetto della cartella dati (cap. 09.4), che fino a qui
aveva potuto aspettare perché nessun tool toccava i file dell'utente. Il lucchetto è un
`dati.lock` **tenuto aperto in esclusiva e vuoto**: chi lo tiene lo dichiara al sistema
operativo, così un processo morto comunque non lascia dietro di sé un lucchetto da
ripulire a mano. Lo prendono in modo **asimmetrico**, perché non lo sono i due che
scrivono: l'applicazione all'avvio e per tutta la sessione, il server MCP per la sola
durata di una scrittura. Chi resta fuori reagisce diverso: il server rifiuta di scrivere e
lo spiega, l'applicazione avvisa e parte lo stesso. **Due cose sono cambiate rispetto al
capitolo**: `esporta_backup` non è di questa tappa — espone F7, che si costruisce a T9, e
oggi nell'applicazione quel bottone è visibile e spento — e `esporta_documento` scrive i
**soli DOCX**, perché il PDF si stampa dal browser incorporato, che vuole una finestra che
in `--mcp` non esiste. *(La seconda è durata poco: il 2026-08-19, chiudendo le cose rimaste
indietro prima di T9, si è visto che quella finestra non serve — basta un filo STA con la
sua pompa di messaggi, e il PDF esce anche da qui. Vedi cap. 09.3.)* `salva_opportunita` accetta **tutti** gli artefatti e non il solo
annuncio, altrimenti quel che i tool di T8b producono non avrebbe dove andare; le stelle
però le **ricalcola il programma** dai giudizi, e dei documenti senza il confronto da cui
nascono si rifiutano — la macchina degli stati (cap. 07.3) non ammette il salto da «nuova»
a «generata». **812 collaudi verdi** (erano 796), versione **0.3.035**, Pool **1.11**
invariato; il lucchetto è stato **falsificato apposta**, rendendolo permissivo, e tre
collaudi sono caduti. Con questa gamba T8 è completa: resta il **collaudo di tappa**, che
va fatto a mano da un client MCP vero ed è in `in_sospeso.md`.

*Fra T8 e T9 — **una seconda passata su `in_sospeso.md`** (2026-08-19).* Stessa
manutenzione della precedente (regola 13 del `CLAUDE.md`), in due giri, per **otto voci**
chiuse. Il primo giro, offline: il **collaudo del silenzio** dello streaming, che oltre a
cedere sotto carico era **cieco al guasto per cui era nato** — quattro pause da 120 ms per
551 ms totali, meno del secondo di silenzio concesso, cosicché falsificando il client
passava lo stesso; ora ventuno pause da 60 ms per 1260 ms contro 1000, ed è insieme più
stabile e capace di diventare rosso. Poi il **PDF via MCP**, che questo capitolo dava per
impossibile due paragrafi più su (cap. 09.3), e la **porta «qui c'è tutto»** del profilo,
che propone per nome il CV più recente già riconosciuto dalla classificazione (cap. 05.2,
cap. 03.6). Il secondo giro è costato **chiamate vere**, perché sono le domande a cui
nessun collaudo senza rete risponde: il **«corso senza nome»**, archiviato come varianza
«una volta su tre» e misurato invece **3 su 3** — a farlo perdere era il conflitto fra due
regole del prompt, curato con una riga e l'ordine di lettura (**Pool 1.12**, cap. 04.7); i
**tetti dei token** di Sonnet 5, aperti da T2 e chiusi da un giro completo di tredici
chiamate — il più sollecitato è al **27,1%**, nessuno da alzare (cap. 02.5, cap. 04.4,
cap. 15 voce 6); l'**interruzione** premuta finalmente mentre l'AI scriveva, a 3,9 s; il
**prima/dopo** della rifinitura letto a video, con il suo limite dichiarato; e l'import del
profilo provato su una **pagina che non è LinkedIn** (cap. 06.7). Cinque voci nuove, tutte
minori, hanno preso il posto di quelle chiuse. Resta fuori quel che chiede una **macchina
Windows con un client MCP vero** — il collaudo di tappa di T8 — insieme ai debiti di sempre
che vogliono una seconda macchina. **817 collaudi verdi** (erano 812) più 15 reali,
versione **0.3.035**, Pool **1.12**.

**Il collaudo di tappa (2026-08-21).** Quello dichiarato qui sopra — «da Claude
Desktop/Code: `tools/list` corretto, un confronto e una generazione via MCP con risultati
identici a quelli dell'interfaccia» — è stato fatto, e senza aspettare Claude Desktop: il
client vero è **Claude Code**, registrato fra i suoi server MCP. Dodici tool visti da un
client che non abbiamo scritto noi, insieme alle **istruzioni del server**, che il banco
non poteva esercitare perché non ha un modello a cui darle. Il lucchetto provato fra **due
processi veri** — app aperta, scrittura rifiutata e spiegata; app chiusa, stessa chiamata
riuscita. Il confronto: **0,9 stelle da entrambe le porte**, per aritmetiche diverse (36 e
18 dalla finestra, 37 e 15 dal server, dove `clamp_giu = -20` taglia lo scarto), il conto
verificato a mano da tutti e due i lati e prodotto da una funzione sola con tre chiamanti.
La generazione: 🎯 CV-2 con lo **scheletro dei fatti identico**, varia solo la prosa. E
`chiamate_ai.csv`, che non esisteva, è **nato**: quattordici righe, sei dal server e otto
dalla finestra, con l'analisi dell'annuncio uscita **identica al token** dalle due porte.
Nessuna riga di codice toccata: **817 collaudi verdi**, versione **0.3.035**, Pool **1.12**
invariati. Il collaudo ha però prodotto **due difetti**, entrambi in `in_sospeso.md`: lo
stato «nuova» che la finestra riapre ma non sa proseguire — una porta aperta dal server da
cui l'interfaccia non sa rientrare, ed è roba della revisione dei pannelli di T9 — e i
giudizi di «contesto» saltati dal confronto in un giro su quattro, da misurare prima di
curare. Con questo T8 è chiusa per intero.

### T9 — Rifinitura e rilascio 1.0 — ✔ **CHIUSA con riserva il 2026-08-24**
Backup/ripristino (F7); Impostazioni complete; pulizia dati; gestione errori rivista
pannello per pannello; **collaudo generale condotto da Mirco su candidature reali**;
aggiornamento del diario e del README; demo (video) per il portfolio; tag `v1.0`.
**Collaudo finale:** la checklist «Problemi e mitigazioni» ereditata dal prototipo
(`HTML+JS/prompt_design.md`) ripercorsa punto per punto sulla nuova app.

**T9 si fa in cinque gambe** *(2026-08-21)*. È la tappa più larga del percorso e tiene
insieme cose che non si somigliano — una funzione nuova, un pannello che non c'è mai stato,
due difetti trovati dal collaudo di T8, e un rilascio. Come a T5, T7 e T8, ogni gamba è un
ramo suo, chiuso con merge fast-forward: `main` resta pubblicabile a ogni passo, e una
gamba che si areni non tiene ferme le altre.

- **T9a — I dati.** Backup e ripristino (F7, cap. 11.4) e il tool `esporta_backup` che li
  aspettava (cap. 09.3).
- **T9b — P8 Impostazioni.** Il pannello che non c'è mai stato (cap. 03, tabella dei
  pannelli) e la pulizia dati del cap. 11.5, che vive lì.
- **T9c — «A che punto sono».** Lo stato `esito` e il promemoria di follow-up, debito di T6
  (cap. 07.3), più il **vicolo cieco dello stato «nuova»** trovato dal collaudo di T8.
- **T9d — Rifinitura.** La gestione errori rivista pannello per pannello, la **misura** dei
  giudizi di contesto che il confronto ogni tanto salta, la modifica a mano dei testi in P6
  (cap. 08.4) e i difetti minori raccolti in `in_sospeso.md`.
- **T9e — Rilascio.** La checklist «Problemi e mitigazioni», la **ripresa delle domande
  saltate** (la voce 4 della checklist, riaperta e costruita), il collaudo generale di Mirco
  su candidature reali, l'icona dell'eseguibile, diario e README, la demo e il tag `v1.0`.

Tre cose lasciate in bianco dal progetto sono state decise prima di cominciare: il
**follow-up scatta a 14 giorni**, con il valore modificabile da P8; la **modifica a mano dei
testi in P6** entra nella 1.0 nella sua forma minima (T9d); le gambe hanno **un ramo
ciascuna**.

**T9a è chiusa** *(2026-08-21)*. Backup e ripristino esistono: un solo file `.json` con
l'intestazione di formato, due contenuti a scelta (il profilo con storico e CV base, oppure
tutto), l'anteprima che dice *cosa contiene e cosa sovrascrive* prima di ogni conferma, e il
profilo di adesso messo in salvo nello storico prima che quello del backup ne prenda il
posto. Il ripristino **non cancella** quello che il backup non nomina, e i nomi che arrivano
da un file scritto a mano restano nomi di file: `..\..\fuori` viene rifiutato e detto in
chiaro. Il tool `esporta_backup` porta i tool del server a **tredici**. **845 collaudi
verdi** (erano 817), versione **0.3.036**, Pool **1.12** invariato. Cinque falsificazioni
provate e cadute tutte (regola 14) — fra cui quella che, tolto il controllo dei nomi, ha
scritto davvero un file fuori dalla cartella dati. Il giro completo — esporta,
perdi tutto, ripristina — è stato percorso **dal vivo** sull'applicazione vera, su una
cartella dati usa-e-getta, e ha stanato un difetto che nessun collaudo aveva visto: la data
del profilo nel backup, dedotta dall'ultima versione dello storico, annunciava «come era il
17 agosto» un profilo salvato il 21. Curato facendo viaggiare la data **dentro il file**.
*La riserva dichiarata alla chiusura — il tredicesimo tool mai visto da un **client MCP
vero** — è caduta nella sessione successiva, quella che riavviandosi ha ricaricato il
server: `esporta_backup` chiamato da Claude Code sui dati veri, con i due contenuti, e nel
file l'intestazione di formato e la data del profilo che ora viaggia dentro; fuori la chiave
API e la cartella `out` dei documenti impaginati. Un `contenuto` inventato è stato rifiutato
spiegando i valori validi senza scrivere niente, e il lucchetto ha negato la scrittura a
finestra aperta — dicendo cosa continuava a funzionare, e diceva il vero — per concederla
appena chiusa. Nessun difetto, nessuna riga toccata: **T9a è chiusa senza riserve.***

**T9b è chiusa** *(2026-08-21)*. Il pannello che non c'era mai stato adesso c'è, ed è una
finestra: «⚙ Impostazioni» stava nella barra da mesi, spento, con scritto per quale tappa.
Dentro, cinque sezioni — la chiave API che si riconosce senza rileggerla, le preferenze sui
documenti, dove stanno le cartelle, cosa gira sotto il cofano, e i dati con le loro due
pulizie. **Non ha OK né Annulla**: le preferenze si scrivono appena si cambiano, in un
`impostazioni.json` fatto per essere aperto e corretto a mano, e quel che invece non si
disfa ha la sua conferma prima di partire. Le tre finestre che servivano c'erano già e
vengono **richiamate**, non rifatte. **878 collaudi verdi** (erano 845), versione
**0.3.037**, Pool **1.12** invariato.

Due promesse del progetto sono state mantenute in una forma diversa da come erano scritte,
e le ragioni stanno nei capitoli: la **cartella dati** si mostra ma non si sposta (cap.
11.1), i **modelli** e il **pool** si leggono e basta (cap. 11.6, cap. 04.5), la **cartella
documenti** si gestisce in P7 dove quel giro sa aspettare l'AI (cap. 05.2). In compenso
`config.json`, che il cap. 11.1 prometteva dal principio e nessuno aveva mai scritto, si
scopre non servire più: le sue due voci avevano trovato case migliori da sole.

Con questa gamba si chiude anche un **debito di T7b**: l'interruttore dell'anti-slop
(cap. 08.4). Vale subito e da entrambe le porte — anche via MCP, perché il cap. 09.3 vuole
che il CV chiesto da un client sia lo stesso che esce dalla finestra. Spenta, la rifinitura
non chiama l'AI affatto: interrogare il modello per buttarne via la risposta costerebbe a
chi l'ha spenta apposta.

**Sei falsificazioni** provate e tutte cadute (regola 14): il lucchetto cancellato con
tutto il resto, l'interruttore ignorato, la finestra che salva già all'apertura, la
preferenza che scavalca la lingua dell'annuncio, i predefiniti cambiati, il ricalcolo dei
bottoni tolto. E **due difetti trovati**, nessuno dei quali un collaudo scritto a tavolino
avrebbe visto. Il primo: «c'è qualcosa da eliminare?» contava le *voci* della cartella, ma
`Assicura` ne ricrea quattro vuote appena qualcuno la tocca — il bottone rosso si sarebbe
riacceso subito dopo un'eliminazione totale, promettendo di mandare via il nulla. Il
secondo l'ha trovato solo la **prova dal vivo**: salvando la prima preferenza nasce
`impostazioni.json`, cioè il primo dato di quella cartella, e il bottone che elimina tutto
restava spento fino alla riapertura della finestra. Il giro è stato poi percorso fino in
fondo su una cartella usa-e-getta — preferenze cambiate e ritrovate al riavvio,
eliminazione totale confermata con la parola scritta a mano, tre voci via, applicazione
chiusa da sé e nella cartella il solo `dati.lock`.

*Un'ora è stata pagata a una trappola dello **strumento di collaudo**, non del prodotto: il
primo `clic` su un bottone che apre una finestra non la apre, e ci vuole il secondo. È
annotata nel `README.md` di `strumenti/mcp-collaudi/`, perché il sospetto cade
naturalmente sull'applicazione — e lì il gestore partiva benissimo.*

**T9c è chiusa** *(2026-08-21)*. «A che punto sono» ha adesso la sua seconda metà: lo stato
**`esito`** e il **promemoria di follow-up**, il debito che T6 si era lasciata dietro, più il
**vicolo cieco dello stato «nuova»** trovato dal collaudo di tappa di T8. **910 collaudi
verdi** (erano 878), versione **0.3.038**, Pool **1.12** invariato.

Tre decisioni hanno orientato la gamba, e le ragioni stanno nel cap. 07.3. Gli esiti
registrabili sono **tre e non quattro**: «in attesa» è già lo stato `inviata`, e registrarla
avrebbe creato due modi di dire la stessa cosa da rincorrere in due posti. L'esito **si
corregge e si toglie** — è una dichiarazione dell'utente, non un fatto osservato — e per
questo ha un metodo suo, l'unico punto da cui il ciclo di vita torna indietro. Il promemoria
guarda le **sole spedite senza esito**, con la soglia scelta in P8 (quattordici giorni, zero
per spegnerlo): le date c'erano già tutte in `date_stati`, e non è servito nessun campo
nuovo. Il vicolo cieco si è chiuso senza toccare lo schema su disco: «Analizza», sulla
candidatura riaperta al solo annuncio, **diventa «Confronta»** e fa il secondo passo da solo.

**Sette falsificazioni** provate e tutte cadute (regola 14): lo stato e l'esito che si
fidano l'uno dell'altro senza mettersi d'accordo, l'attesa contata anche a chi ha già
saputo, lo zero che non spegne più il promemoria, la data del passaggio disfatto lasciata
dietro, il vicolo cieco rimesso dov'era, il bottone dell'esito sempre premibile, e la voce
del menù scollegata dall'azione.

**Uno stato nuovo tocca chi non se lo aspetta**, e a dirlo è stato il secondo controllo, non
il banco: aprendo davvero lo stato `esito` — che fino a ieri nessuno raggiungeva — **P7** si è
trovata a chiedere un passo all'indietro che non esiste. Chi torna a rimandare la stessa
email e ripreme «L'ho spedita» su una candidatura che ha già un esito si sentiva rispondere
«non sono riuscita a segnarla come inviata»: l'eccezione della macchina degli stati,
raccolta e raccontata a chi non aveva sbagliato niente. Adesso si avanza **solo se la
transizione è lecita**, e ripremere non fa nulla. Il primo collaudo scritto per difendere
quel punto era **verde anche col codice rotto** — guardava l'esito, che restava intatto —
ed è stato rifatto perché guardasse la riga che l'utente legge (regola 14).

*La prova dal vivo ha trovato quel che il banco non poteva vedere*, su una cartella dati
usa-e-getta: il menù degli esiti **scendeva fuori dalla finestra**, perché il suo bottone
sta nella fascia in fondo — ora si apre verso l'alto. E ha stanato un limite dello
**strumento di collaudo**, non del prodotto: le voci di un menù contestuale non si premono
(risponde «Premuto» e non succede niente), così come non si legge il valore di un
`NumericUpDown`. È annotato nel `README.md` di `strumenti/mcp-collaudi/`; il filo fra la
voce e l'azione, che restava scoperto, si è coperto nel banco con `PerformClick`. Il giro
vero è stato percorso lo stesso — con un clic del mouse alle coordinate della voce — e con
esso il **confronto di una candidatura nata dal server MCP**: 36 secondi, quindici giudizi,
0,8 stelle e la cartella che avanza a «interessante».

**T9d è chiusa** *(2026-08-22)*. La gamba più disomogenea di T9 — la gestione errori rivista
pannello per pannello, la misura dei giudizi di contesto, la modifica a mano dei testi e i
difetti minori di `in_sospeso.md` — si è fatta in **quattro tempi**, tutti con merge
fast-forward. **955 collaudi verdi** (erano 910), più il banco dei copioni 10 su 10, versione
**0.3.039**, Pool **1.12 invariato**. **Sedici falsificazioni** provate e tutte cadute
(regola 14).

Il **primo tempo** ha chiuso il debito che T9b si era lasciata dietro: durante una chiamata
all'AI si spegne **tutta** la barra, «⚙ Impostazioni» compreso — il difetto non era un
bottone dimenticato ma **due elenchi** che dicevano cose diverse, e ora ce n'è uno solo. Con
lui, i messaggi che incolpavano la cosa sbagliata, l'annullamento che su `PannelloRicerca`
avrebbe fatto cadere l'applicazione se lasciato propagare, e la regola «una rifinitura che
fallisce si dichiara» estesa a P6 e P7, che tacevano. `FormPrincipale` ha avuto il suo primo
banco — senza reflection e senza `InternalsVisibleTo` — e i due copioni JavaScript di
`LettorePagina`, l'unico codice del prodotto che nessun compilatore guardava, hanno avuto il
loro in `strumenti/collauda-copioni/`.

Il **secondo tempo** ha portato «Modifica i testi» (cap. 03, P6; cap. 08.4), e la domanda da
cui era partito aveva una premessa falsa: le tre caselle di P6 non mostrano il documento ma
la pagina di blocchi che finisce nel DOCX, e renderle scrivibili avrebbe voluto dire
ricostruire il JSON da un testo impaginato. Di lì la finestra a parte, con l'elenco dei campi
di prosa che **`Rifinitura` è l'unico posto a conoscere**: una seconda lista nell'interfaccia
divergerebbe al primo campo nuovo.

Il **terzo tempo** è nato da quattro dubbi guardando l'applicazione vera. I due export
**chiedono dove salvare** e aprono Esplora risorse sul file: il difetto non era il colore dei
bottoni ma il **silenzio** — i file c'erano, in una cartella che l'utente non aveva scelto.
La voce **«📄 Documenti»** entra in barra con la tendina che le fa da seconda metà, e il
**prima/dopo della rifinitura è stato tolto del tutto**, misurando prima cosa mostrava
davvero: su una candidatura vera, cinque ritocchi in tutto e **nessun fatto toccato**. La
lingua invece non si tocca, e la ragione è che un CV inglese non è la traduzione di uno
italiano. Due difetti stanati dalla prova dal vivo sullo stesso punto di codice: il testo
catturato da un portale arrivava **tutto attaccato** (`\n` in una casella che rende solo
`\r\n`) e portava le righe `&nbsp;` dei separatori grafici — curati, il secondo con un
criterio buono per ogni portale: un pezzo che, ripulito, non contiene nemmeno una lettera o
una cifra non entra nel testo.

*La **misura** dei giudizi di contesto che il confronto ogni tanto salta è stata fatta e non
ha chiesto nessuna cura: **otto giri, 4 per porta**, stesso profilo e stesso annuncio, tutti
**5 giudizi su 5** (titolo, sede, contratto, mansioni, benefit). `prompt-pool/confronto` non
si tocca, e il Pool resta 1.12.*

Il **quarto tempo** è la prova dal vivo di chiusura, condotta da Mirco su cinque punti, e ha
detto due volte la stessa cosa: **quel che è acceso deve sembrarlo**. La tendina dei documenti
era «poco visibile» proprio mentre è la porta da cui P6 si usa, e adesso apre la fascia, in
grande e dentro una cornice d'accento (cap. 03, P6); i bottoni di livello 2 erano **ancora**
letti come spenti dopo la cura del bordo del terzo tempo, e adesso hanno lettere d'accento,
contorno doppio e grassetto — tre segnali invece di uno, su tutta l'applicazione (cap. 03,
livelli). Nello stesso tempo il **sottotitolo** dell'app è cambiato insieme al banner del
logo prodotto fuori dal repo (cap. 13.5; cap. 15, voce 3).

*Un collaudo ballerino è stato curato invece di essere annotato, ed è il caso che la regola 14
guarda con più sospetto: `UnaRispostaLungaMaVivaNonScadeMai` misura un tempo e girava in mezzo
al traffico della batteria parallela — subito dopo una compilazione la ripresa da una pausa da
60 ms arrivava oltre il secondo concesso, e cadeva senza che nel prodotto fosse cambiato
niente. Tre volte in due giorni, mai da solo. Ora è `DoNotParallelize`, e le sue proporzioni
non si sono toccate: allargare il tetto del silenzio gli avrebbe tolto la sola cosa che sa
fare, accorgersi del ritorno a un'attesa complessiva. Falsificato togliendo il riarmo del
silenzio: **rosso**.*


**Il collaudo di tappa: la checklist «Problemi e mitigazioni»** *(2026-08-22, secondo tempo
di T9e)*. Le otto voci ereditate dal prototipo (`HTML+JS/prompt_design.md`) sono state
ripercorse una per una sulla nuova app. Sette delle otto difese oggi vivono **dentro i
prompt del pool**, e un prompt lo si può leggere quanto si vuole: dice cosa il modello è
tenuto a fare, non cosa fa. Perciò la checklist non si è chiusa leggendo, ma **chiedendolo
al modello** con tre prove costruite per tentarlo, che restano nel banco come collaudi
rieseguibili (`CollaudiChecklistReale`, categoria `Reale`) con il loro rapporto in
`casi/reale/`.

| # | Voce | Come è stata verificata | Esito |
|---|---|---|---|
| 1 | Gonfiamento delle competenze | prova A, con l'AI vera | ✅ «leader nato» resta «leader nato», «un po' di inglese» resta attenuato |
| 2 | Informali promosse a formali | prova A | ✅ il banco del cognato «in nero» è stato instradato alle informali |
| 3 | Campi indovinati | prova A | ✅ email, telefono, scuola e anno taciuti restano vuoti; città e titolo detti ci sono |
| 4 | `pending_questions` | decisione, poi **costruita** | ✅ **entrata nella 1.0**, in due metà: la ripresa delle domande saltate (terzo tempo) e la domanda di approfondimento sulla voce mezza vuota (quinto tempo) — v. sotto |
| 5 | Più voci in una risposta | prova A | ✅ tre impieghi in una battuta sola restano tre |
| 6 | Scoring della famiglia A | rilettura del codice + banco | ✅ architettura ibrida intatta; sei casi dell'hard-gate più la parità deterministica |
| 7 | Requisiti «tipici» non scritti | prova B, con l'AI vera | ✅ da quattro righe escono quattro liste vuote, azienda vuota, e l'email ricopiata |
| 8 | Confine lacuna / «non si sa» | prova C, con l'AI vera + tre casi nuovi senza rete | ✅ nessuna lacuna archiviata come dubbio; 0,2 stelle invece di saturare |

*La voce 4 non chiedeva una decisione nuova: il **cancello T0 l'aveva già presa**
(cap. 15.5, «`pending_questions`: **fuori**, resta nel backlog»). È stata **ratificata**:
nella 1.0 il suo posto lo tengono tre difese che esistono davvero — il default sicuro sui
vuoti, la conferma in blocco prima che qualcosa entri, e il campo `altrove`
dell'anti-perdita, che di una domanda saltata recupera almeno il contenuto detto nel turno
sbagliato. La voce resta in `idee_future.md`, dov'era.* **Quella ratifica è durata poche
ore**: rileggendola, Mirco ha chiesto di far entrare la voce nella 1.0, ed è il terzo tempo
qui sotto. Il paragrafo resta com'era scritto perché la ratifica c'è stata davvero, e la
sua motivazione — tre difese che esistono al posto di una che non c'era — è ancora il
motivo per cui la 1.0 sarebbe uscita bene anche senza.

**Quel che il rito ha stanato, e che nessuno cercava.** La metà-codice della voce 8 —
`non determinabile` escluso dal conteggio, e il sentinel «Nessuna esperienza richiesta» che
non pesa mai — era **scoperta**: due righe di `CalcoloMatch` che nessuno dei 973 collaudi
guardava. Adesso hanno i loro tre casi (`Caso7`, `Caso8`, `Caso9`), con gli attesi prodotti
dal prototipo come tutti gli altri della classe. **988 collaudi verdi** (erano 955 alla
chiusura di T9d, 973 dopo il primo tempo). Versione e Pool **non si toccano**: nessun
prompt è stato modificato, perché nessuna prova ha trovato un difetto da curare.

**Nove falsificazioni, sei rosse e tre verdi — e le tre verdi dicono qualcosa** (regola
14). Sono diventate rosse: il conteggio che accoglie `non determinabile` fra i punti; il
sentinel che smette di essere escluso; il prompt `confronto` privato della «distinzione
chiave», dove il modello ha subito archiviato l'inglese come «non determinabile» e la prova
C l'ha visto; la prova A dopo la cura raccontata qui sotto; e i due giudici accecati apposta.

Le tre rimaste verdi hanno tutte lo stesso motivo, e vale la pena saperlo: **la stessa cosa
è difesa su più strati, e toglierne uno non basta.** Nel codice, cancellare da
`CalcoloMatch` la riga che esclude `non determinabile` non cambia niente — quell'esito non
sta nemmeno nella tabella dei punti, e la guardia successiva lo scarta lo stesso; per
vedere il rosso è servito **metterlo nella tabella**. Nei prompt, le regole tolte a
`esperienze_formali` e ad `analisi_annuncio` non hanno fatto cedere il modello, perché
attorno restavano la definizione delle quattro categorie e il compito dichiarato in
apertura.

C'è poi la falsificazione che è verde e rossa insieme — quella contata fra le sei, perché
rossa lo è diventata dopo — e ha stanato un difetto della **prova**, non del prodotto:
tolte a `competenze` le due regole della normalizzazione, il modello ha smesso di scrivere
«un po' di inglese» e ha scritto «inglese» — il gonfiamento vero non è il livello inventato,
è l'attenuante che sparisce — e la prova A non lo vedeva. Adesso ha una seconda spia, e
rifatta la falsificazione è diventata rossa. Restava però la domanda aperta sulle altre:
verdi perché la difesa regge, o perché non saprebbero vedere il contrario? Da lì è nato
`CollaudiGiudiziChecklist`: dodici collaudi senza rete che danno a ciascun giudice il
difetto scritto a mano — il lavoro in nero messo fra i lavori veri, anche solo nascosto
dentro `cosa_facevo`; l'annuncio riempito di requisiti mai scritti; la competenza gonfiata;
la lacuna archiviata come dubbio — e verificano che lo trovi.

*Il conduttore del dialogo — chi risponde al posto dell'utente — è stato **estratto** da
`CollaudiDialogoReale` in `ConduttoreDiDialogo`, perché adesso le tracce sono due: quella di
Anna Ricci, che guarda l'anti-perdita, e quella di Marco Gentili, che tenta le difese. Il
modo di condurre dev'essere uno solo, per la stessa ragione per cui l'anti-invenzione vive
in un posto solo. Il dialogo di Anna è stato rifatto con l'AI vera dopo l'estrazione:
verde.*


**La ripresa delle domande saltate** *(2026-08-22, terzo tempo di T9e)*. La voce 4 della
checklist era l'unica senza una difesa costruita, e Mirco ha scelto di darle la sua invece di
lasciarla al backlog. Il buco era piccolo e preciso: un turno che non raccoglie niente offre
«Riprovo / Passiamo oltre», e con «passiamo oltre» quella domanda non tornava mai più. Ora
torna — **una volta sola, prima del riepilogo, e chiedendo il permesso**: «su *«esperienze
informali»* non avevamo raccolto niente. Vuoi provarci ora?» → *Ci provo* / *Lasciamo così*.

Il perimetro è **i quattro turni-contenuto** (esperienze formali e informali, competenze,
formazione), gli unici che passano dallo stato «niente colto». I tre turni singoli — nome,
contatti, patente — restano fuori: lì il vuoto lo si conferma, si rilegge nel riepilogo e si
corregge dall'editing del profilo (cap. 12, A2); riproporlo sarebbe insistere su una risposta
data.

Il disegno **non aggiunge una seconda strada al dialogo**, che è la parte che conta. La
ripresa si aggancia alla passata finale — quella dell'anti-perdita — e **dopo** di lei, così
un turno che nel frattempo si è riempito da sé (un frammento recuperato dal magazzino) non
viene richiesto: la risposta c'è già. Chi accetta **rientra nel turno vero**, con la sua
domanda e le sue schede di conferma; l'unico punto deviato è `AvanzaAsync`, che dentro una
ripresa torna alla passata finale invece di proseguire col turno dopo. È un `If` solo, e
copre tutte le uscite di un turno perché passano tutte di lì — compreso il caso in cui la
ripresa accenni a un'altra categoria, che finisce nel magazzino e viene smaltito prima del
riepilogo.

**La terminazione è la stessa lezione dell'anti-rimbalzo, applicata alle domande**: dentro
una ripresa non si segna niente, e la domanda esce dall'elenco quando la si offre, non quando
riesce. Senza l'una o l'altra il dialogo non finirebbe più. **Cinque falsificazioni, cinque
rosse** (regola 14): la guardia di terminazione, l'occasione unica, la guardia di rilevanza,
il deviatore di `AvanzaAsync` e la segnalazione del turno saltato — ognuna rompe esattamente
il collaudo che la difende, nessuna è rimasta verde per merito di un altro strato. La prima
volta però una di esse **appese** il banco invece di arrossarlo: l'aiutante che declina le
riprese ciclava senza fine, e adesso ha un tetto — un collaudo che non termina non è un
collaudo che passa.

**995 collaudi verdi** (erano 988), più i copioni 10 su 10; versione **0.3.041**, **Pool 1.12
invariato**: nessun prompt è stato toccato, la ripresa è tutta codice e testi. Due collaudi
dell'anti-perdita sono stati adeguati — saltavano dei turni per arrivare in fondo, e ora in
fondo trovano la domanda — e il conduttore dei collaudi reali ha imparato a declinarla,
annotandolo fra le stranezze del rapporto. Una cosa va detta e messa in conto: **questa
funzione nel prototipo non esiste**, quindi il collaudo di non-regressione non la copre e non
la può coprire (cap. 04.7). È il primo pezzo di dialogo che nasce senza termine di paragone,
e la sua unica rete è il banco headless — per questo qui le falsificazioni contano doppio.

**Il quarto tempo è il collaudo dal vivo** (2026-08-23), condotto da Mirco su quattro giri
pensati per domande diverse fra loro, come già a T3, T4, T5 e T8. **Nessuna riga di prodotto
è stata toccata**: qui si guarda, si misura e si annota, e il registro dei reperti vive fuori
dal repo perché tocca dati reali.

- **Giro A — il dialogo del profilo**, su una cartella dati usa-e-getta. La ripresa delle
  domande saltate costruita nel terzo tempo funziona dal vivo: la domanda torna una volta
  sola, prima del riepilogo, chiedendo il permesso. Quattro reperti, tutti da curare: la via
  del domicilio che sparisce in silenzio, un «Correggi» che azzera i campi non ripetuti —
  perdita di dati già confermati, il più grave del giro — un ruolo nudo scartato in
  contraddizione con la regola dell'`altrove`, e un luogo finito nel campo «Ruolo».
- **Giro C — una candidatura vera, dall'annuncio all'email.** Chiude il debito lasciato dal
  collaudo di T8 sui giudizi di contesto: cinque su cinque. Con 0,8 stelle, mitigazione
  onesta e **nessuna riga inventata** in CV mirato e lettera; il `.eml` è corretto fin nelle
  intestazioni, e il destinatario resta vuoto perché l'annuncio non dava nessuna email — non
  se l'è inventata. Cinque reperti: due con la cura già decisa nella stessa sessione — la
  cattura che sulle pagine-risultati prende tutto in silenzio, e il conflitto fra modifica a
  mano e rigenerazione, che si porta dietro un **bump del Pool** su `lettera.it/en.md` — e tre
  lasciati al tempo delle cure: la finestra «Modifica i testi» copre la sola prosa, e va
  deciso dove si tolgono e si riordinano le voci di un singolo documento senza duplicare i
  fatti che vivono nel profilo; l'**export è in blocco** e scrive tutti i documenti della
  candidatura insieme, mentre CV e lettera vanno esportabili uno per uno; e il primo
  «Prepara email» con il programma di posta chiuso mostra una **finestra grigia** finché
  Outlook non si è scaldato — non è un difetto nostro, ma l'esperienza è rotta e basta una
  riga di avviso.
- **Giro B — la scala di Windows al 150%** (notte del 23 agosto; DPI di sistema 144
  verificato dopo la disconnessione, che è l'unico modo di farla valere davvero). Cinque
  debiti di `in_sospeso.md` esercitati in una passata sola, senza spendere una chiamata
  all'AI, e tre reperti: il pannello del logo che sfonda nell'area viva, metà della finestra
  Impostazioni irraggiungibile — è dimensionata sul contenuto e non ha scorrimento — e la
  finestra principale che scende sotto il minimo di progetto lasciando accavallati due
  bottoni della fascia comandi di P6. Il filo che li lega è uno solo: **costanti in pixel di
  progetto usate dove il DPI le ha già moltiplicate**. Invisibili al 100%, e invisibili al
  banco, che gira a 96 DPI.
- **Giro D — l'exe su un PC senza runtime .NET 10 e i due debiti di Word** resta **fuori
  portata su questa macchina**: qui c'è l'SDK 10 installato e c'è solo LibreOffice. Va fatto
  altrove, ed è la riserva con cui la tappa si chiuderà (regola 15).

Il quarto tempo non chiude niente da solo: le cure dei **dodici reperti** sono il **quinto
tempo**, insieme alla **domanda di approfondimento sui campi mancanti**, che il collaudo ha
promosso dentro la 1.0 in forma minima — una domanda per voce, solo sui campi che pesano nel
CV, occasione unica. Prima delle tre cure del giro B c'era però una decisione a monte, e il
quinto tempo si è aperto prendendola (cap. 15.7). **995 collaudi verdi**, versione
**0.3.041**, Pool **1.12 invariato**: in questo tempo non si è scritta una riga.

Con lei T9e diventa di **sei tempi**: l'identità visiva, la checklist, la ripresa, il
**collaudo dal vivo**, **le cure dei reperti con la domanda di approfondimento**, e il
rilascio.

### Il quinto tempo — le cure — ✔ **CHIUSO il 2026-08-24** (aperto il 23)

**La decisione a monte, e perché non è andata come sembrava.** La domanda era se curare i tre
difetti di scala uno per uno o dare all'interfaccia un modo unico di dichiarare le misure
rispetto al DPI. Un censimento del codice ha contato oltre ottanta punti della stessa forma —
in astratto, l'argomento perfetto per la via larga. Guardandoli da vicino, però, i tre difetti
**non erano la stessa specie**: uno era davvero un confronto fra unità diverse, uno era una
costante che duplicava una misura già posseduta dal runtime (si toglie, non si converte), uno
non era affatto un problema di unità ma di tetto e scorrimento mancanti — si sarebbe rotto
uguale a 96 DPI su uno schermo basso — e l'ultimo era una **doppia** scalatura, dove un
convertitore in più avrebbe moltiplicato due volte. La via larga ne avrebbe curato uno.
Si è scelta la via puntuale, ma **collaudabile**: un modulo di funzioni pure a cui il DPI si
**passa**, così un banco che gira a 96 DPI può chiedere cosa succede a 144. Prima non esisteva
nel progetto una sola riga che leggesse il DPI, né un collaudo che lo esercitasse.

**Le cure, verificate dal vivo a 150%.** Il pannello del logo passa in compatta (186×160
invece di 373×360) e non sfonda più nell'area viva; la finestra si ferma al minimo vero,
**1725×900**, che sono i 1150×600 di progetto esatti; le Impostazioni stanno in **1012×1008**
dentro l'area di lavoro e quel che eccede si raggiunge scorrendo. Il tetto e lo scorrimento
sono stati messi su **tutte e quattro** le finestre che condividevano quella riga, non solo su
quella caduta. Una scoperta non prevista lega due reperti che sembravano indipendenti: la
sovrapposizione dei comandi al minimo non veniva dal minimo, ma dallo stesso ingombro sbagliato
del logo — la fascia credeva di avere 125 px in più e concludeva che i bottoni ci stavano su
una riga. Curato l'ingombro, va a capo da sé, e il minimo non è stato alzato.

**Con le cure si è chiuso un debito vecchio.** «Svuota i dati di navigazione» (T9b) non era mai
stato premuto dal vivo: a 150% era irraggiungibile proprio per il difetto delle Impostazioni.
Curato quello, il bottone è tornato a portata, ha chiesto conferma e ha cancellato davvero i
**183 MB** di `webview2\`, spegnendosi da sé perché non restava più niente da svuotare. La
stessa prova ha verificato la cura e chiuso il debito, che era il motivo per cui si era deciso
di farla qui.

**Poi i reperti dei giri A e C.** Il primo curato è **R2**, il più grave: i turni singoli del
profilo sostituiscono il blocco intero, quindi chi correggeva la sola via perdeva email e
telefono già confermati — in silenzio. La semantica resta la sostituzione, che è la regola che
l'utente può prevedere; a cambiare è che ora il riepilogo **dice** quali campi spariranno,
prima che si confermi. Guardando il codice è saltato fuori un secondo caso della stessa
famiglia, più insidioso: una patente non colta in una correzione vale «no», e la patente è
spesso il requisito eliminatorio di un annuncio. Avvisata anche quella.

**Con R2 escono gli altri sei della prima passata**, e sono tutti della stessa famiglia —
difetti di silenzio. R1: una via di casa che spariva senza dirlo. R3: un ruolo nudo scartato,
mentre la regola dell'`altrove` dice di parcheggiarlo. R4: un luogo finito nel campo «Ruolo».
R5: la cattura che su una pagina di **risultati** prendeva tutto — adesso una pagina-elenco si
riconosce, e una selezione dell'utente batte la pagina intera. R8: i documenti esportabili
**uno per uno** invece che in blocco. R9: l'avviso quando il programma di posta parte da
freddo. Il Pool si sigilla a **1.13**, e la falsificazione dice qui una cosa che altrimenti
non si sarebbe saputa: rimettendo i prompt di prima cadono R1 e R3, mentre **R4 resta verde
comunque** — è una cintura, non una cura misurata; le soglie di R5 sono severe di proposito ma
non tarate su pagine vere, e R9 un collaudo non ce l'ha perché è una stringa. **1019 collaudi
verdi**, versione **0.3.042**.

**R7 — la modifica a mano che si ricorda** *(2026-08-23)*, il più grosso dei dodici. Due strade
cieche che partivano dallo stesso punto: modificato un testo del 🎯 CV-2 la ✉️ lettera
continuava a raccontare la storia di prima, e «Rigenera» si riprendeva la modifica — in
silenzio tutte e due, perché la memoria di una riscrittura viveva in un **booleano di
sessione** che il rientro in P6 azzerava. Ora vive dove vive il documento (cap. 11.1), e da lì
discendono l'avviso che **nomina** i testi a rischio, la spia «⚠ Rigenera la lettera» che
esiste solo quando il CV è più recente della lettera — il verso conta — il riallineamento
automatico alla chiusura della finestra, e un prompt della lettera che sa distinguere una prosa
scritta dal modello da una scritta dalla persona (blocco `<riscritture>`, Pool 1.13). Sette
difetti rimessi sotto il codice, **sei rossi**; il settimo è il prompt, e per farlo cadere è
servito il modello vero. **1058 collaudi**, 16 reali, versione **0.3.043**.

**Due difetti trovati a mano nel pannello del profilo** *(2026-08-24)*, che nei dodici non
c'erano: in sei delle diciotto caselle scrivere «abc» lasciava a video «cba», e senza una voce
scelta i campi della scheda erano scrivibili ma senza destinazione (cap. 03.6). Il primo è
istruttivo più della sua cura: una riga sola in tutto il progetto, `Items(i) = etichetta`, che
in WinForms non riscrive la riga ma la toglie e la rimette — e nel farlo alza un evento di
selezione che ricarica i campi sotto la mano che scrive. Una **passata statica** su tutti e
diciotto i file di `Ui/` più una prova di digitazione su ogni casella di ogni schermata dice
che quella forma non esiste altrove. Quattro collaudi nuovi, ognuno **visto rosso** col difetto
rimesso. **1062 collaudi**, versione **0.3.044**.

**R6 — una voce si lascia fuori da un documento** *(2026-08-24)*, nella forma decisa con Mirco:
non si tocca il profilo, si sceglie cosa **quel** CV racconta (cap. 08.4). Tre decisioni la
reggono, e ognuna esclude una strada più corta: il documento **non si taglia mai** (la scelta
vive accanto al `cv.json`, ed è per questo che rimettere è gratis); la voce si riconosce per
**impronta dei fatti** e non per posizione né per prosa, perché il documento nuovo lo scrive il
modello; il filtro è **uno solo** e serve anche i due che leggono il JSON grezzo — il prompt
della lettera e il tool `leggi_opportunita` — perché due porte della stessa applicazione non
possono descrivere due documenti diversi (cap. 09.3). La falsificazione ha trovato qui un buco
vero: tolta la guardia contro un'impronta estranea al documento, **il banco restava tutto
verde** — un altro collaudo la copriva per caso, da un'altra strada. **1094 collaudi**, versione
**0.3.045**. Del reperto resta fuori il **riordino** delle voci, che era nominato insieme alla
rimozione: si toglie e si rimette, non si sposta. Non è un debito ma un raffinamento — la
forma di R6 è stata decisa, e questa le sta accanto: sta in `idee_future.md`.

**La domanda di approfondimento sui campi mancanti** *(2026-08-24)* chiude il tempo, ed è la
voce 4 della checklist nella sua **forma piena**: il terzo tempo aveva costruito la metà del
turno saltato, questa è la metà della **voce mezza vuota** (cap. 12.2). Il prompt del turno
formale prometteva esattamente questo — «una voce INCOMPLETA è comunque una voce … sarà
l'utente, con la voce davanti, a completarla o lasciarla» — e fino a qui nessuno manteneva la
promessa. La sua rete è tutta nel banco, perché nel prototipo questa cosa non esiste e la
non-regressione non la può coprire (cap. 04.7): **otto falsificazioni, otto rosse**. Un collaudo
`Reale` in più perché l'assunto centrale del disegno riguarda il **modello** e non il codice —
una risposta nuda ha prodotto la domanda da sé, e «tre anni circa» è finito in `durata` col
resto della voce fermo. Nella stessa gamba il conduttore dei collaudi reali impara a
riconoscere la domanda **senza spendere una battuta della traccia** (la frase fissa si legge dal
prodotto, non si ricopia) e ha finalmente un suo banco offline.

**Il bilancio del quinto tempo.** Dodici reperti su dodici curati, più i due trovati a mano nel
pannello del profilo e un debito vecchio di T9b chiuso di conseguenza; **1110 collaudi verdi**
(erano 995 all'apertura del tempo), copioni **10/10**, versione **0.3.046**, Pool **1.13** —
un bump solo, come previsto, e nessun prompt toccato dopo di quello. Restano fuori il
**riordino** delle voci di R6 e il **giro D** (l'exe su una macchina senza runtime .NET 10, e i
due debiti di Word), che questa postazione non può fare: è la **riserva** con cui la tappa si
chiuderà, regola 15. Il sesto tempo è il rilascio.


### Il sesto tempo — il rilascio — ✔ **CHIUSO il 2026-08-24**

**Il primo passo non era nel codice.** `main` era fermo a due giorni prima sul remoto: il
merge del rito era stato fatto e il push no, e un rilascio che parte da un `origin` indietro
mette il tag su una storia che nessun altro ha. Prima riga del tempo, perciò, `git push
origin main` — e col rimando esplicito, perché si lavorava dal ramo della tappa.

**Il numero.** `0.3.046` → **`1.0.000`**, cambiato dove vive: la costante di `Versione.vb`.
Il `.vbproj` la rilegge da lì, quindi le proprietà dell'eseguibile sono seguite da sé — che
è la ragione per cui a T1 si decise di tenerla in un posto solo, e si vede solo il giorno in
cui il numero cambia davvero.

**La procedura di rilascio, che il progetto non aveva.** Il cap. 13 diceva con che parametri
si pubblica e che aspetto ha il risultato, ma non **in che ordine** si fanno le cose: è il
buco che questo tempo doveva chiudere, ed è ora il **13.9**. Non è stato scritto prima e
seguito dopo, ma il contrario — si è fatto il rilascio annotando i passi, così il paragrafo
racconta una sequenza percorsa e non una immaginata. Due passi ci sono finiti proprio perché
sbagliarli non fa rumore: il **banco intero prima di pubblicare** (col server MCP del
prodotto da chiudere, o l'exe resta bloccato) e la **cartella svuotata prima del publish** —
senza quello, «un solo file» non è una verifica ma un'ispezione su una cartella già sporca,
e un `.pdb` rimasto da ieri passerebbe per assenza di problema.

**L'eseguibile, e le quattro cose che nessun collaudo automatico sa dire.** `publish.bat` su
cartella vuota: **118.707.086 byte in un file solo**. Le proprietà rilette **dall'eseguibile**
e non dal `.vbproj` — `1.0.000`, TrovaLavoro, Aviolab AI, `© 2026 Aviolab AI`, la descrizione
— e qui la console ha provato a mentire, mostrando `c 2026` dove l'exe dice `©`: era la
codifica del terminale, e la si è smascherata leggendo il valore due volte per due strade.
Poi l'avvio vero, che è la prova che un exe compilato non è un exe che parte: lanciato con
`--dati` su una cartella usa-e-getta, la Home è comparsa, la coda vuota, il pannello logo con
`Ver. 1.0.000 · Pool 1.13 (integrato)`. Infine la dimensione, confrontata con la 0.3.041 del
22 agosto: **73.728 byte in più**, cioè le cure del quinto tempo e nient'altro — nessuna
dipendenza entrata di nascosto.

**Il bilancio, e la riserva.** **1110 collaudi verdi** con il numero nuovo, versione
**1.0.000**, Pool **1.13** invariato — in questo tempo non si è toccato un prompt. Il tag
`v1.0` si mette sul commit di `main` dopo il merge, che è dove la storia delle release vive
(13.5). Resta fuori il **giro D**: l'eseguibile su una macchina senza
runtime .NET 10, e con lui i due debiti di Word (il DOCX aperto in Word, il `.docx` salvato da
Word e reimportato). Quella macchina qui non c'è, e adesso almeno l'exe da portarle è quello
giusto — il debito annotato il 24 agosto era proprio che la copia pronta fosse anteriore alle
cure. **E c'è una seconda riserva, che il bilancio del quinto tempo non nominava**: la
**demo (video) per il portfolio**, dichiarata in questa stessa tappa fin dal 5 agosto e mai
fatta — non è emersa da sola, l'ha trovata la rilettura di quel che T9 prometteva. Non
dipende da una macchina che manca, e proprio per questo era la più facile da lasciar
scivolare via in silenzio. **T9e si chiude con due riserve** (regola 15), scritte tutt'e due
in `in_sospeso.md`: non sono una formula di cortesia.

**Con T9e si chiude T9, e con T9 il piano.** Le cinque gambe — i dati, le Impostazioni, «a
che punto sono», la rifinitura, il rilascio — sono tutte fuse in `main`, l'ultima col merge
che chiude il tempo. Il percorso T0→T9
disegnato il 5 agosto è percorso per intero: diciannove giorni, **1110 collaudi offline** più
i reali, **29 file di prompt** nel pool, un eseguibile da un file solo che parte su un PC
Windows 11 e un server MCP che espone tredici tool. Quel che resta non è una tappa ma
manutenzione e raffinamento: `in_sospeso.md` per i debiti, `idee_future.md` per il resto.

*Dopo T9 — **le rifiniture prima del giro D** (2026-08-24).* Non è una tappa e non ne prende
il nome: il piano è chiuso, e questo lavoro vive sul ramo `rifiniture-pre-giro-d` per
arrivare pronti al **giro D**, la revisione col tutor sul suo PC. Il punto di partenza non è
un file da rileggere, come nelle due passate qui sopra, ma i **cinque difetti visibili**
annotati usando la 1.0. Il primo si è chiuso senza scrivere una riga: il **markdown grezzo
nelle bolle** del brainstorming era già curato da T9d il 22 agosto, e a darlo per aperto era
`in_sospeso.md`, che non aveva spostato la voce insieme alle due sorelle — un elenco aperto
non è una prova, e confrontarlo col codice costa dieci minuti. Gli altri quattro hanno
chiesto codice. La **barra orizzontale delle Impostazioni**: quando la finestra scorre, la
fila dei controlli si dispone due volte, la seconda dentro la larghezza che resta tolta la
barra verticale — 17 px a 96 DPI, 26 a 150%, contro i 14 del margine di disegno — e `Disponi`
è diventata `DisponiIn(altezzaDisponibile)`, perché un banco non può cambiare schermo ed è
proprio lì che quella disposizione fa qualcosa di diverso (cap. 03.4). La **selezione che
tornava in cima** nella finestra «Modifica i testi»: i due elenchi si rifanno a ogni «Togli» e
ogni «Rimetti», e la riga scelta si ritrova adesso per **identità** invece di chiedere a
`SelectedItems`, che risponde solo a elenco già nato — al banco mai, dove il primo collaudo
scritto era verde senza provare niente. Il **✎ che conosceva solo il giro corrente**, mentre
l'avviso di «Rigenera» rispondeva alla stessa domanda leggendo il disco (cap. 08.4). E la
**candidatura sopravvissuta al suo profilo**: rigenerarne i documenti dopo che quel profilo è
stato eliminato e rifatto mandava al modello due persone diverse, e la risposta tornava come
errore di lettura; il segno è la **versione che manca dallo storico**, non una versione
diversa, e la guardia sta su tutte e quattro le strade che chiamano l'AI (cap. 11.1 e 12, A7).

**Venti collaudi nuovi, 1130 verdi** (erano 1110), ognuno visto rosso prima di dirlo buono:
**sedici falsificazioni**, e una di esse non diventa rossa ma cade come **timeout** — tolta la
guardia sul cambio di lingua, la finestra di conferma si apre senza nessuno che risponda. Non
è più vero che il banco non abbia collaudi di misura dell'interfaccia: ce ne sono **due**, e
guardano una finestra sola. Nessun prompt toccato, quindi **Pool 1.13** e versione **1.0.000**
restano dove erano: questo non è un rilascio. **Tre riserve dichiarate** (regola 15), tutte da
provare dal vivo: la selezione nel `ListView` **vero** — il banco prova la logica su una
finestra mai mostrata —, la **barra a 150%** e il messaggio nuovo davanti a una candidatura
orfana; per il secondo elenco di quella finestra resta il debito già noto dello strumento di
collaudo. E prima del giro D restano da preparare il suo **copione**, che non esiste, la
**chiave API** sulla macchina del tutor con la decisione sui dati di prova, e la **passata
della regola 16 su T1→T8**, mai fatta — con un ordine che non si può sbagliare: l'**SDK .NET
10 sulla postazione del tutor si installa dopo**, perché il giro D vuole proprio una macchina
che non ce l'ha. Fuori dal giro D restano, come dichiarato chiudendo T9e, la **demo video** e
il **tag `v1.0`**.

*Dopo T9 — **i dati del giro D, provati prima del viaggio** (2026-08-25).* Delle tre cose che
il paragrafo qui sopra dava per da fare, due si chiudono qui. Il **copione esiste**: è il
**§13.10** del cap. 13, messo accanto alla procedura di rilascio perché il giro D è il passo
*dopo* — sette voci col criterio per dirle passate (`D1` l'exe su un PC pulito, `D2` i
documenti aperti in Word, `D3` un `.docx` salvato **davvero da Word** e reimportato, `D4` che
D1-D3 siano avvenute lì, più le tre riserve `R-a/b/c`) e due tempi, prima il giro guidato con
al centro la **catena di Word**, poi le mani al tutor su un compito solo. E i **dati non sono
più da decidere**: sono in `casi/giro-d/` — un CV finto in PDF col suo sorgente, tre annunci e
il `LEGGIMI` che dice cosa deve uscirne — e sono stati **provati qui con l'AI vera** prima di
salire in valigia, perché un profilo che l'estrazione non digerisce misurerebbe il dato invece
del programma, su una macchina che non è mia e in un tempo che non si ripete. Verdi, con le
due insidie che hanno retto: la città esce **Forlì**, cioè il domicilio e non la residenza di
Cesena, e i **traslochi** stampati sotto «Altre esperienze» finiscono fra le informali.

Il seguito non era in programma, e viene dalla domanda su **quale** delle due insidie fosse
davvero sorvegliata. La prima sì, ha un `Assert` in `CollaudiImportReale`; la seconda passava
fra le maglie di tutti i controlli — `ControlloCollocazione` cerca la parola «volontario», che
un trasloco non dice mai, `ControlloDoppioni` vede solo la voce contata due volte, e il
conteggio delle formali sarebbe rimasto dentro la tolleranza. Nasce così **`ControlloCriterio`**,
il **terzo** controllo che guarda dentro un profilo: confronta l'importato con
`casi/profilo.json` sui **fatti** e non sulla prosa — le durate tengono le parole del CV, le
descrizioni il modello le riformula — e appaia un'attività informale promossa a lavoro con
**due parole distintive nella stessa voce**, distintive perché il contrasto lo dà il criterio
stesso. Lo fa girare **`CollaudiGiroD`**: una prova con l'API e **nove senza rete**, e vuole
**solo la chiave**, perché il CV sta nel repo — è inventato — e il metro non è il prototipo ma
il criterio. **Nove collaudi nuovi, 1139 verdi** (erano 1130), con **tre falsificazioni** che
hanno fatto cadere ogni volta solo il collaudo che sorveglia quel pezzo; due dei nove esistono
apposta perché il controllo **non** lampeggi. Le classi che chiedono l'API vera passano da nove
a **dieci**. Niente prompt toccati: **Pool 1.13** e versione **1.0.000** restano dove erano.
Del paragrafo precedente resta aperto **quale chiave** portare sulla macchina del tutor, la
**passata della regola 16 su T1→T8** e — fuori dal giro D — la **demo video** e il **tag
`v1.0`**.


## Ordine e dipendenze

```
T0 ─► T1 ─► T2 ─► T3 ─► T4 ─► T5 ─► T5d ─► T6 ─► T9
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
