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

## 5.6 Nomi e collocazione dei file generati

- Ogni opportunità ha la sua sottocartella nella cartella dati (cap. 11):
  i file generati finiscono lì e restano legati alla candidatura. *Con un'eccezione che
  il capitolo 11 spiega per esteso (2026-08-10): il 📄 CV-1 base non nasce da un
  annuncio e non appartiene a nessuna candidatura, quindi i suoi file stanno accanto al
  profilo da cui discende.*
- Nomi parlanti e stabili: `CV_Mirco_Parenti_<Azienda>_<AAAA-MM-GG>.docx` (e `.pdf`),
  `Lettera_<Azienda>_<AAAA-MM-GG>.docx`. In inglese se l'output è in inglese
  (`CV_Mirco_Parenti_EN_…`). Il CV base, che un'azienda non ce l'ha, la lascia fuori dal
  nome: `CV_Mirco_Parenti_<AAAA-MM-GG>.docx`.
- Un bottone «Apri cartella» accanto a ogni documento generato: l'utente deve sempre
  poter mettere le mani sui suoi file.
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
