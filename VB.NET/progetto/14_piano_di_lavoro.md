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

### T2 — Il motore e il pool — ✔ **CHIUSO il 2026-08-07**
`Ai/LibreriaPrompt` (pool esterno + integrato, manifest, segnaposto);
`Ai/ClientClaude` (**chiamate sincrone**, retry, timeout) verso **Haiku 4.5**
(estrazione) e **Sonnet 4.6** (ragionamento), cioè gli **stessi modelli del prototipo**:
il confronto si fa a parità di modello, così una differenza nei risultati è una
differenza di codice e non del modello sotto. Il salto a **Sonnet 5** (cap. 15, voce 6)
è il **secondo esperimento**: si fa da `modelli.json` senza ricompilare e porta con sé
l'interruttore del ragionamento esteso, che lì va acceso (cap. 02.5). Lo **streaming non
è di questa tappa**: arriva con T4/T7, quando ci sarà un pannello che lo mostra.
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

### T7 — Multilingua e qualità (F4 completo)
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
brainstorming** (`brainstorm` + `appunti_di_mira`, P5, streaming SSE): da fare.

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

**Fatto:** **663 collaudi verdi** senza rete (erano 599 a fine T6), versione **0.3.031**,
Pool **1.09**. Restano indietro, dichiarate in `in_sospeso.md`: la modifica a mano dei testi
in P6, l'interruttore della rifinitura (P8, T9), il prima/dopo dell'email e la lettura a
video del prima/dopo fino in fondo. **Resta T7c.**

### T8 — Il server MCP (F8)
Modalità `--mcp` su stdio; i tool del cap. 09; lucchetto di scrittura.
**Collaudo:** da Claude Desktop/Code: `tools/list` corretto, un confronto e una
generazione via MCP con risultati identici a quelli dell'interfaccia.

### T9 — Rifinitura e rilascio 1.0
Backup/ripristino (F7); Impostazioni complete; pulizia dati; gestione errori rivista
pannello per pannello; **collaudo generale condotto da Mirco su candidature reali**;
aggiornamento del diario e del README; demo (video) per il portfolio; tag `v1.0`.
**Collaudo finale:** la checklist «Problemi e mitigazioni» ereditata dal prototipo
(`HTML+JS/prompt_design.md`) ripercorsa punto per punto sulla nuova app.

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
