# 05 — Documenti: ingresso e uscita

*Come entrano i documenti dell'utente (CV esistenti, attestati) e come escono quelli
generati (CV in DOCX e PDF). Regola guida: un solo modello di contenuto, più
«stampanti».*

## 5.1 Formati in ingresso

L'utente può fornire un **file singolo** o una **cartella**. Formati accettati:

| Formato | Come viene letto |
|---|---|
| **PDF** | come nel prototipo: il file va all'API in blocco `document` e il modello di estrazione lo **trascrive fedelmente** (metodo validato anche su CV a due colonne). Il testo trascritto passa poi alla strutturazione (`importa_cv`). |
| **TXT / MD** | lettura diretta dal disco (UTF-8, con riconoscimento del BOM e ripiego sulla codifica ANSI se serve). Il Markdown si usa così com'è: le intestazioni aiutano il modello. |
| **DOCX** | un `.docx` è un archivio ZIP: il programma estrae `word/document.xml` e ne ricava il testo (paragrafi e tabelle, nell'ordine del documento) con gli strumenti standard di .NET — nessuna libreria esterna. |

Casi limite dichiarati:

- **PDF scannerizzati** (immagine): la trascrizione può uscire vuota o povera; l'app lo
  rileva (testo troppo corto) e propone il ripiego onesto: incollare il testo a mano.
  Un OCR locale non è in perimetro; se in futuro servisse un elaboratore PDF esterno
  della casa, verrebbe integrato **solo come eseguibile opzionale** affiancato all'app
  (l'app funziona comunque senza).
- **PDF oltre i limiti dell'API** (~32 MB / 100 pagine) o protetti da password:
  messaggio chiaro e ripiego incolla-testo.

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
                    ┌────────────► DOCX  (scrittore OOXML interno)
CV JSON / Lettera ──┤
                    └────────────► PDF   (modello HTML → WebView2 → stampa PDF)
```

## 5.4 Uscita DOCX

- **Scrittore interno**: un `.docx` viene costruito componendo lo ZIP OOXML con gli
  strumenti standard di .NET (`System.IO.Compression` + XML) partendo da un
  **modello incorporato** nell'exe: stili (titoli, corpo, elenchi), margini,
  intestazione con i recapiti. Nessuna dipendenza da Word: il file si apre con Word,
  LibreOffice, Google Docs.
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
  aggiunge alcun componente nuovo.

## 5.6 Nomi e collocazione dei file generati

- Ogni opportunità ha la sua sottocartella nella cartella dati (cap. 11):
  i file generati finiscono lì e restano legati alla candidatura.
- Nomi parlanti e stabili: `CV_Mirco_Parenti_<Azienda>_<AAAA-MM-GG>.docx` (e `.pdf`),
  `Lettera_<Azienda>_<AAAA-MM-GG>.docx`. In inglese se l'output è in inglese
  (`CV_Mirco_Parenti_EN_…`).
- Un bottone «Apri cartella» accanto a ogni documento generato: l'utente deve sempre
  poter mettere le mani sui suoi file.

## 5.7 Collaudo previsto (rimando al cap. 14)

- DOCX: apertura senza avvisi in Word e LibreOffice; testo integro (confronto
  automatico campo-per-campo tra JSON e testo estratto dal file prodotto).
- PDF: testo selezionabile e ricercabile; resa identica al DOCX a vista.
- Ingresso: batteria di CV veri nei quattro formati + i due CV «trappola» a due
  colonne già usati nel prototipo.
