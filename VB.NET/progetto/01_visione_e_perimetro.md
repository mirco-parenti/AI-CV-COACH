# 01 — Visione e perimetro

*Cos'è l'applicazione, per chi è, cosa deve fare e dentro quali paletti. Questo è il
capitolo da cui dipendono tutti gli altri: se una scelta nei capitoli successivi
contraddice questo, vince questo.*

## 1.1 Cos'è

**AI-CV-COACH per Windows** — nome mostrato all'utente: **TrovaLavoro** (cap. 15,
voce 3) — è un'applicazione desktop per Windows 11, distribuita come **un solo file
`.exe`**, che accompagna una persona lungo **tutto** il percorso di candidatura:

1. costruire (o importare) il proprio **profilo professionale**;
2. **trovare** annunci di lavoro adatti, anche sui grandi portali (Indeed, Jooble,
   Subito.it…) o partendo da un link;
3. **valutare** ogni annuncio con un punteggio di match onesto (stelle 0–5, con
   requisiti eliminatori che fanno da cancello);
4. **ragionare** sull'annuncio insieme all'utente (brainstorming, punti deboli, come
   presentarsi al meglio senza mentire);
5. **generare** un CV mirato e una lettera di presentazione, in italiano o in inglese;
6. **produrre le uscite**: CV in DOCX o PDF ed email di candidatura pronta da inviare
   (file `.eml`), con gli allegati giusti — a spedirla è il programma di posta
   dell'utente;
7. **tenere il registro** delle candidature, fino a esaurire le opportunità;
8. **mantenere aggiornato** il profilo base nel tempo, man mano che l'utente matura
   nuove esperienze e competenze.

L'intelligenza del sistema sta nei **prompt**, conservati come **libreria di file `.md`**
esterni al codice e caricati al bisogno; il motore applicativo (VB.NET) orchestra i
prompt, chiama l'API di Anthropic e rende deterministico ciò che deve esserlo (punteggi,
soglie, formati dei file).

## 1.2 Da dove viene

L'applicazione è la **Fase 3** del progetto AI-CV-COACH. La logica è già stata
progettata, costruita e collaudata in un prototipo web (area `HTML+JS/` del repo):
4 anelli — profilo, analisi annuncio, confronto, generazione — più mitigazione dei gap,
import di CV in PDF, soglia di prudenza e hard-gate sui requisiti eliminatori.

Dal prototipo **migra l'asset durevole**:

- i **15 prompt** validati e i loro **schemi JSON** (`HTML+JS/prompt_design.md`);
- la **logica del punteggio** (pesi, clamp, tetto eliminatorio — oggi in
  `calcolaMatch` di `HTML+JS/server.js`);
- le **convenzioni** anti-invenzione e anti-perdita (campo `altrove`);
- il criterio dei **due modelli** (Haiku per l'estrazione, Sonnet per il ragionamento).

Del prototipo **non migra l'impalcatura**: pagina web, aiutante Node e banchi di prova
si rifanno in VB.NET nella forma adatta a un'applicazione desktop.

Il disegno funzionale di riferimento resta `HTML+JS/architettura.md` (voci 2.x,
vista-dati «un profilo, molti CV»): questo progetto lo **estende**, non lo riscrive.

## 1.3 Per chi è

- **Primo utente: Mirco**, autore del progetto, che userà il sistema per la propria
  ricerca di lavoro. I flussi sono disegnati sul suo caso reale (profilo da
  riposizionare, candidature multiple, CV in italiano e in inglese).
- **Carta di presentazione**: il repo è anche il portfolio pubblico di Mirco. Il codice,
  i prompt e questi documenti devono essere leggibili da un selezionatore **non
  tecnico**: chiarezza prima di virtuosismo.
- **Utente generico**: chiunque cerchi lavoro e voglia un aiuto concreto senza
  «gonfiare» il proprio CV.

## 1.4 Le due bussole etiche (regole del prodotto)

Ereditate dal prototipo, valgono per ogni funzione nuova:

- **Anti-invenzione** — l'applicazione non deve inventare esperienze, competenze,
  titoli o risultati non presenti nel profilo reale dell'utente. L'annuncio e i
  giudizi sono *segnale di mira*, mai *fonte di fatti*.
- **Anti-perdita** — nulla di ciò che l'utente dichiara va perso in silenzio: o viene
  strutturato al posto giusto, o viene dichiarato esplicitamente «lasciato fuori».

A queste si aggiunge una bussola nuova, imposta dalla funzione di ricerca annunci:

- **L'utente naviga, il programma legge** — la raccolta degli annunci avviene nel
  browser integrato (WebView2) dove **l'utente** naviga e si autentica come sé stesso;
  il programma legge la pagina che l'utente sta guardando. Niente scraping massivo,
  niente aggiramento di protezioni: si automatizza la *lettura assistita*, non il
  *prelievo industriale*.

## 1.5 Perimetro funzionale

Le funzioni sono numerate **F1–F8** e mappate sui capitoli di questo progetto:

| # | Funzione | Contenuto | Capitolo |
|---|---|---|---|
| **F1** | Profilo | dialogo guidato, import da CV (PDF/TXT/MD/DOCX o cartella) o dalla **propria pagina web** letta nel browser integrato *(voce 2.1.3, cap. 06.7)*, editing campo-per-campo, aggiornamento periodico assistito *(quest'ultimo — il flusso D — **dichiarato fuori dalla 1.0** il 2026-09-01: v. cap. 12.4. Il profilo si aggiorna dalla scheda, campo per campo; la sessione assistita resta prevista dal progetto, non dalla versione)* | 05, 06.7, 12 |
| **F2** | Ricerca annunci | ricerche salvate sui portali, cattura dell'annuncio dal browser integrato, annuncio da link, coda di opportunità | 06 |
| **F3** | Valutazione e ragionamento | analisi annuncio, confronto, stelle + hard-gate, mitigazione, brainstorming con l'utente | 02, 12 |
| **F4** | Generazione | 📄 CV-1 base, 🎯 CV-2 mirato, ✉️ lettera; multilingua IT/EN; rifinitura anti-slop | 04, 08, 10 |
| **F5** | Uscite | export DOCX e PDF, email `.eml` pronta da inviare con gli allegati | 05, 07 |
| **F6** | Registro candidature | stati per opportunità (nuova → interessante → generata → inviata → esito, più scartata), storico | 07, 11 |
| **F7** | Dati e backup | cartella dati, export/import del profilo in JSON, chiave API protetta | 11 |
| **F8** | Server MCP integrato | le funzioni di lettura, analisi e generazione esposte come tool MCP a client esterni (Claude Desktop, Claude Code…); le azioni a conseguenza esterna restano nell'app | 09 |

## 1.6 Vincoli di progetto (non negoziabili salvo decisione esplicita)

1. **Un solo `.exe`, nessuna DLL a fianco**: l'applicazione si distribuisce copiando un
   file. Le uniche cose ammesse accanto all'exe sono l'eventuale **cartella della
   libreria prompt** (`prompt-pool/`, facoltativa: una copia del pool è integrata
   nell'exe — cap. 04.2) e gli eventuali **strumenti esterni** del punto 6.
2. **Pannelli statici, editabili nel designer**: tutte le schermate sono Form/Panel
   WinForms disegnati staticamente nel designer di Visual Studio; niente UI generata a
   runtime (fuori dai contenuti: liste, testi, anteprime). *Una sola eccezione, decisa
   in T3c e dichiarata nel cap. 03.1: le **bolle della conversazione**, che sono
   contenuto di lunghezza ignota e non hanno un controllo di sistema che le sappia
   mostrare. Il resto di quel pannello è disegnato come tutti gli altri.*
3. **Family feeling** con la suite di casa (logo in basso a sinistra con **versione
   dell'app e versione del pool di prompt**, stessa impostazione visiva). Il design è
   specificato come design **proprio** di AI-CV-COACH nel capitolo 03.
4. **Prompt esterni al codice**: libreria di file `.md` versionata, caricata al bisogno.
5. **Lingua**: interfaccia in italiano; CV e lettere in italiano **e** inglese (cap. 10).
6. **Riservatezza della suite di casa**: la suite aziendale a cui il family feeling si
   ispira è proprietaria; in questo repo **non compare nessun suo dettaglio
   implementativo**. Se servisse una sua utility (es. un elaboratore PDF), verrebbe
   inclusa **solo come eseguibile binario**, solo se davvero utile, e trattata come
   strumento esterno opzionale.
7. **Anti-slop**: i testi in prosa (sommario, lettera, email) passano una rifinitura di
   «umanizzazione» dedicata (cap. 08) — senza introduzione di errori di battitura.
8. **Windows 11**: unico sistema operativo bersaglio. Niente multipiattaforma.

## 1.7 Metodo: design-first

Questa fase produce **solo documenti**: si scrive e si mette a punto il progetto a
questo livello (tecnico da perito, non da ingegnere) finché non siamo convinti; poi si
passa all'implementazione seguendo il piano del capitolo 14. Le domande ancora aperte
sono raccolte nel capitolo 15: finché una voce sta lì, nessun capitolo può darla per
decisa.

**Criterio di «pronto per implementare»**: il capitolo 15 è vuoto (o contiene solo
rimandi a fasi successive dichiarate), e ogni capitolo ha superato una rilettura fatta
insieme all'utente.

*Il criterio è stato soddisfatto il 2026-08-05 e l'implementazione è in corso (cap. 14):
da allora questi capitoli non si riscrivono per raccontare lo stato — restano il disegno,
e si aggiornano quando una decisione cambia o matura sul campo.*
