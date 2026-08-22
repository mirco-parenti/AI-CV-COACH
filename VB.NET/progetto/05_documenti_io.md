# 05 — Documenti: ingresso e uscita

*Come entrano i documenti dell'utente (CV esistenti, attestati) e come escono quelli
generati (CV in DOCX e PDF). Regola guida: un solo modello di contenuto, più
«stampanti».*

## 5.1 Formati in ingresso

L'utente può fornire un **file singolo** o una **cartella**. Formati accettati:

| Formato | Come viene letto |
|---|---|
| **PDF** | come nel prototipo: il file va all'API in blocco `document` e il modello di estrazione lo **trascrive fedelmente** (metodo validato anche su CV a due colonne). Il testo trascritto passa poi alla strutturazione (`importa_cv`). |
| **TXT / MD** | lettura diretta dal disco (UTF-8, con riconoscimento del BOM; UTF-16 anche **senza** BOM; ripiego sulla codifica ANSI se serve). Il Markdown si usa così com'è: le intestazioni aiutano il modello. |
| **DOCX** | un `.docx` è un archivio ZIP: il programma estrae `word/document.xml` e ne ricava il testo (paragrafi e tabelle, nell'ordine del documento) con gli strumenti standard di .NET — nessuna libreria esterna. |

*Da T5d (2026-08-14) esiste una sorgente che **non è un file**: la pagina aperta nel
browser integrato, di norma la propria pagina profilo (cap. 06.7). Non entra in questa
tabella perché non c'è niente da leggere dal disco — il testo lo porta fuori il lettore
di pagina — ma da lì in poi la strada è **la stessa**: `importa_cv`, gli stessi campi,
lo stesso «Testo letto» a fronte. È il motivo per cui aggiungerla non ha richiesto un
formato in più.*

Casi limite dichiarati:

- **PDF scannerizzati** (immagine): la trascrizione può uscire vuota o povera; l'app lo
  rileva (testo troppo corto) e propone il ripiego onesto: incollare il testo a mano.
  Un OCR locale non è in perimetro; se in futuro servisse un elaboratore PDF esterno
  della casa, verrebbe integrato **solo come eseguibile opzionale** affiancato all'app
  (l'app funziona comunque senza).
- **PDF oltre i limiti dell'API** (~32 MB / 100 pagine) o protetti da password:
  messaggio chiaro e ripiego incolla-testo.
- **File enormi, rotti o maligni** *(2026-08-09, revisione adversariale)*: ogni lettura
  ha un **tetto dichiarato** — dimensione massima del PDF prima di mandarlo all'API,
  dei documenti letti dal disco e del testo estratto, con una guardia **anti zip-bomb**
  sul DOCX. E ogni inciampo di lettura — file sparito, chiavetta smontata, permesso
  negato — esce come `ErroreImport` in italiano, mai come finestra di crash; la lettura
  gira **fuori dal thread dell'interfaccia**, così la finestra non si congela.

## 5.2 La cartella documenti

Se l'utente indica una cartella («qui c'è tutto»), il programma **trova da solo quello
che serve**:

1. **Scansione**: elenca i file leggibili (PDF/TXT/MD/DOCX, anche in sottocartelle di
   primo livello), con nome, data e dimensione.
2. **Assaggio**: per ogni file estrae un campione di testo (prima pagina / primi
   ~600 caratteri).
3. **Classificazione AI** (prompt `classifica_documenti`, modello di estrazione): per
   ogni file propone una categoria — `cv` (con stima di quale sia il **più recente**),
   `attestato/certificato`, `lettera`, `altro` — e una riga di motivazione.
4. **Conferma umana**: l'utente vede la proposta («questo è il CV che uso come fonte;
   questi sono attestati allegabili») e può correggerla. Niente entra nel profilo o
   nelle email senza conferma.

La cartella resta **registrata** in configurazione: i suoi attestati compaiono poi tra
gli allegati suggeriti quando si prepara un'email (cap. 07), e la scansione si può
ripetere quando il contenuto cambia.

### Com'è stata costruita (T6, 2026-08-14)

- **Costruita per gli allegati, poi anche per il profilo.** Delle due cose che questa
  cartella serve a fare, T6 aveva fatto la seconda: gli attestati da allegare a un'email.
  La porta «qui c'è tutto» del profilo — prendere da sé il CV più recente e importarlo —
  è arrivata il **2026-08-19**, chiudendo le cose rimaste indietro prima di T9. Fino a lì
  la classificazione quel CV lo indicava e il dato si salvava in `documenti.json`, ma non
  lo leggeva nessuno: chi importava un CV doveva ritrovarselo a mano fra i propri file.
- **La porta si apre proponendo, non prendendo** *(2026-08-19)*. Premuto «IMPORTA CV DA UN
  FILE», se la cartella è registrata e il CV riconosciuto è **ancora al suo posto**, il
  programma lo propone per nome e lascia tre uscite: usalo, scelgo io un altro file,
  lascia stare. È il passo 4 di questo capitolo — la conferma umana — applicato anche
  qui: la macchina indovina il più recente da nome e data, e qualche volta sbaglia. Che il
  file esista si guarda **al momento di proporlo** e non quando fu classificato, perché qui
  nessun file viene copiato e nel frattempo quel CV può essere stato spostato o buttato:
  in quel caso non si propone niente e si va alla scelta del file, come prima.
- **Dei PDF non si assaggia il testo.** Il passo 2 vale per TXT, MD e DOCX, che si leggono
  dal disco; per un PDF servirebbe una **trascrizione dell'AI a file** (cap. 05.1), e in
  una cartella di documenti i PDF sono quasi tutto: quindici chiamate per smistare della
  carta. Si mandano nome, data e dimensione, e l'elenco dichiara che l'assaggio non c'è —
  il prompt lo sa e giudica su quel che vede, dicendolo nel motivo. Sul campo ha retto:
  nove file su nove riconosciuti, i tre attestati compresi.
- **Un tetto dichiarato invece di un elenco troncato.** Si mandano a classificare al
  massimo **60 file**, perché l'elenco entra in un prompt; quel che resta fuori **si
  dice**, altrimenti si leggerebbe come «nella cartella non c'era altro».
- **Solo il primo livello di sottocartelle**, come dice il passo 1: una cartella
  «documenti» qualunque ha dentro di tutto, e scendere all'infinito vorrebbe dire proporre
  a un'azienda un file pescato chissà dove. Il nome che si conserva è **relativo** alla
  cartella (`attestati\HACCP.pdf`), così spostare la cartella non invalida l'elenco.
- **Una correzione dell'utente non si rimette in discussione.** Il documento che è stato
  corretto a mano resta com'è anche quando la cartella si rilegge: la rilettura serve a
  riconoscere i file **nuovi**, non a ridiscutere una decisione già presa (passo 4).
- **Nessun file viene copiato**, e i nomi non bastano: quel che si conserva è un elenco di
  nomi con una categoria, e gli allegati si leggono da dove sono. Un attestato cancellato
  dalla cartella sparisce anche dagli allegati proposti — che è quel che l'utente si
  aspetta di aver fatto cancellandolo.

## 5.3 Il modello di contenuto in uscita

CV e lettera nascono come **JSON strutturato** (stessi schemi del prototipo: sezioni
del CV con campi-fatto e campi-prosa; lettera in quattro blocchi + firma). Da quel JSON
il programma «stampa» nei formati richiesti. Il testo è identico su tutti i formati:
cambiare stampante non cambia il contenuto.

```
                     ┌───────────► DOCX  (scrittore OOXML interno)
CV JSON / Lettera ──►│  pagina     ├───────────► PDF   (modello HTML → WebView2 → stampa PDF)
   (impaginazione)   └───────────► testo (anteprima a video in P6)
```

*Le stampanti sono tre, e la terza non scrive file (2026-08-10, T4c).* L'anteprima che
P6 mostra accanto all'annuncio nasce **dagli stessi blocchi** degli altri due formati,
non dal JSON: se leggesse il JSON per conto suo, il giorno in cui l'impaginazione cambia
mostrerebbe un documento che i file non contengono — cioè mentirebbe proprio nel punto in
cui l'utente controlla prima di esportare.

*In mezzo c'è una pagina, e non è un dettaglio di implementazione (2026-08-10, T4b).*
Il JSON non arriva alle stampanti: prima diventa una **pagina di blocchi** — un nome,
una riga di recapiti, un titolo di sezione, un paragrafo, una voce, un elenco — e ogni
stampante sa soltanto come si disegna un blocco nel suo formato. Senza quel passaggio le
stampanti sarebbero due, e ognuna dovrebbe sapere per conto suo che cosa entra nel
documento, in che ordine e con quali etichette: due letture da tenere allineate a mano, e
il primo giorno in cui divergono il DOCX e il PDF della stessa candidatura direbbero cose
diverse. Così invece il contenuto è identico **per costruzione**, e un collaudo mette le
due uscite una accanto all'altra a verificarlo (cap. 14).

## 5.4 Uscita DOCX

- **Scrittore interno**: un `.docx` viene costruito componendo lo ZIP OOXML con gli
  strumenti standard di .NET (`System.IO.Compression` + XML) partendo da un
  **modello incorporato** nell'exe: stili (titoli, corpo, elenchi), margini,
  intestazione con i recapiti. Nessuna dipendenza da Word: il file si apre con Word,
  LibreOffice, Google Docs.
  *Che forma abbia quel modello, deciso implementando (2026-08-10, T4b):* sono le
  **cinque parti fisse** del pacchetto — l'elenco dei contenuti, le due relazioni, gli
  stili, la numerazione degli elenchi — incorporate come file `.xml` veri, allo stesso
  modo del pool dei prompt; cambiare font, corpi o margini si fa lì, senza toccare il
  codice. Le due parti che dipendono dai dati — il corpo e il titolo — si costruiscono
  invece con `XDocument`, che è anche il modo di non doversi ricordare a mano che una
  `&` nel nome di un'azienda va scritta `&amp;`. Due dettagli voluti: l'elenco puntato è
  un **elenco vero** e non un « • » scritto nel testo, così chi estrae il testo (un ATS,
  o il collaudo che rilegge quel che ha scritto) ritrova la competenza pulita; e le date
  interne dell'archivio sono fissate a un istante costante, così **lo stesso contenuto dà
  sempre lo stesso file** e a cambiare un documento è il contenuto, non l'ora in cui l'hai
  salvato.
- **Impaginazione del CV** (modello unico, sobrio, una colonna — pensato per superare
  anche i lettori automatici degli ATS): intestazione con nome, recapiti e patente;
  sommario; esperienze (ruolo, azienda, periodo, descrizione); competenze; formazione.
  L'ordine delle sezioni è quello del profilo, come nel prototipo.
- La **lettera** in DOCX usa lo stesso modello tipografico (carta intestata minimale
  con nome e recapiti).

## 5.5 Uscita PDF

- Il CV viene impaginato in un **modello HTML+CSS incorporato** (stesso disegno del
  DOCX) e reso in una WebView2 **fuori schermo**; la stampa in PDF usa la funzione
  nativa del motore Chromium (`PrintToPdfAsync`). Risultato: PDF fedele, con font
  incorporati, senza librerie PDF esterne e senza bisogno di Word.
- WebView2 è già nel programma per la ricerca annunci (cap. 06): questa scelta non
  aggiunge alcun componente nuovo. *Con una precisazione di calendario, emersa aprendo
  T4 il 2026-08-10:* la ricerca annunci è **T5**, la stampa PDF è **T4**, quindi a
  introdurre davvero WebView2 nel programma è questa tappa, non quella. Chi legge il
  capitolo 06 come «il posto da cui WebView2 arriva» ha ragione sul disegno e torto
  sull'ordine. *E quando T5a è arrivata davvero (2026-08-12) le due WebView non sono
  diventate due mondi: l'ambiente dell'applicazione è **uno solo**, lo tiene
  `Web/MotoreBrowser` e la stampante se lo fa dare invece di accendersene uno suo. Il
  perché sta nel cap. 06.1 — due ambienti sulla stessa cartella di navigazione stanno
  buoni finché nessuno cambia loro un'opzione, e quando si rompono lo fanno con un errore
  che non spiega niente.*
- **I margini del foglio li mette la stampa, non il foglio di stile** *(2026-08-10, T4b)*:
  sono gli stessi 2 cm del DOCX, dichiarati una volta sola nelle impostazioni di stampa.
  Scriverli anche nel CSS della pagina rischierebbe di sommarli, e un CV con margini
  doppi si accorge solo chi lo stampa.
- «Fuori schermo» ha un significato preciso, verificato prima di implementare
  *(2026-08-10)*: il controllo vuole una finestra vera con il suo handle, quindi la
  finestra **esiste** ma nasce a coordinate fuori dall'area visibile e non compare nella
  barra delle applicazioni. Il PDF che ne esce ha `/ToUnicode` e font TrueType
  incorporati, cioè **testo selezionabile e ricercabile** con gli accenti al posto
  giusto — che è esattamente ciò che il collaudo di 5.7 chiede. Il dettaglio della prova
  (dimensioni, single-file, versioni) sta nel cap. 13.3, dov'è il vincolo che rischiava
  di più.
- **Quella finestra però non è il requisito vero** *(2026-08-19)*. La frase qui sopra
  descrive come la stampa nasce dentro l'applicazione, e per tre tappe è stata letta come
  una condizione: niente finestra, niente PDF — è il motivo per cui T8c aveva dichiarato
  il PDF impossibile da MCP (cap. 09.3). Rimisurata, la condizione è un'altra: al motore
  serve un **filo STA con la pompa dei messaggi**, cioè un thread che sappia rispondere ai
  messaggi di Windows, e la finestra è solo il modo più comune di averne uno. La prova era
  in casa da T4b — il banco stampa PDF veri da un processo di collaudo, che finestre non ne
  apre. Quel filo vive ora nel motore (`Motore/FiloGrafico`) e lo usano tutti e tre: la
  finestra, il banco e il server MCP.

## 5.6 Nomi e collocazione dei file generati

- Ogni opportunità ha la sua sottocartella nella cartella dati (cap. 11):
  i file generati finiscono lì e restano legati alla candidatura. *Con un'eccezione che
  il capitolo 11 spiega per esteso (2026-08-10): il 📄 CV-1 base non nasce da un
  annuncio e non appartiene a nessuna candidatura, quindi i suoi file stanno accanto al
  profilo da cui discende.*
- Nomi parlanti e stabili: `CV_Mirco_Parenti_<Azienda>_<AAAA-MM-GG>.docx` (e `.pdf`),
  `Lettera_<Azienda>_<AAAA-MM-GG>.docx`. In inglese se l'output è in inglese
  (`CV_Mirco_Parenti_EN_…`). Il CV base, che un'azienda non ce l'ha, la lascia fuori dal
  nome: `CV_Mirco_Parenti_<AAAA-MM-GG>.docx`. *La sigla della lingua vale **anche per lui**
  da T7d (2026-08-18), da quando la lingua si può scegliere: `CV_Mirco_Parenti_EN_…`. Non è
  un dettaglio di ordine — è ciò che impedisce alla versione inglese di sovrascrivere
  l'italiana dello stesso giorno, e in una cartella dove i due CV convivono dice a colpo
  d'occhio quale si sta per allegare.*
- L'utente deve sempre poter mettere le mani sui suoi file. *Il disegno prevedeva un
  bottone «Apri cartella» accanto a ogni documento generato; non è mai stato fatto, e la
  prova dal vivo di T9d ha mostrato il prezzo: premere «Esporta» scriveva davvero i file,
  ma nella cartella dati — che l'utente non ha scelto e non sa trovare — e l'unico avviso
  era una riga di testo in fondo al pannello. Sembrava un bottone rotto. **Dal 2026-08-22
  la promessa si mantiene meglio di così**: l'esportazione **chiede dove salvare** (si parte
  dal Desktop, poi dall'ultima cartella usata) e alla fine **apre Esplora risorse** sul file
  appena scritto. Il bottone che apre una cartella diventa superfluo quando è
  l'esportazione stessa ad aprirla.*
- **La cartella scelta riceve una copia, non gli originali** *(2026-08-22)*: i file
  nascono comunque dove dice questo capitolo — la cartella dell'opportunità, o quella del
  profilo per il 📄 CV-1 base — perché di lì li prende P7 per allegarli all'email
  (cap. 07.2). Spostarli invece di copiarli lascerebbe l'email senza niente da allegare.
  Se nella cartella scelta ci sono già file con quei nomi si chiede prima di sostituirli, e
  chi dice di no non perde niente: i documenti aggiornati restano dove sono nati, e il
  pannello lo dice.
- **Si scrive solo il formato che si chiede** *(2026-08-10, T4c)*: P6 ha due bottoni
  d'esportazione (cap. 03.6), e chi preme «Esporta PDF» non deve ritrovarsi accanto anche
  un DOCX che non ha chiesto. Resta invece l'ordine di T4b quando si chiedono entrambi —
  prima i DOCX, poi i PDF — perché il PDF è l'unico che dipende da un pezzo di Windows
  che potrebbe mancare.

## 5.7 Collaudo previsto (rimando al cap. 14)

- DOCX: apertura senza avvisi in Word e LibreOffice; testo integro (confronto
  automatico campo-per-campo tra JSON e testo estratto dal file prodotto).
- PDF: testo selezionabile e ricercabile; resa identica al DOCX a vista.
- Ingresso: batteria di CV veri nei quattro formati + i due CV «trappola» a due
  colonne già usati nel prototipo.

*Che cosa vuol dire «identica», misurato nel collaudo di tappa di T4 (2026-08-10):* il
**contenuto** è identico e si pretende tale — 114 campi su 114 ritrovati nei sei file, e
i due formati coincidono carattere per carattere una volta tolti spazi e segni.
L'**impaginazione** no, e non può esserlo: il DOCX lo impagina il programma che lo apre,
il PDF l'ha già impaginato Chromium, e la stessa pagina esce un po' più compatta di là.
È la stessa differenza che si vede aprendo un DOCX in Word e in LibreOffice. Si guarda
perciò che siano identiche la **struttura** e la gerarchia tipografica, non l'interlinea.
