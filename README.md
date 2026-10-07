![TrovaLavoro — crea il tuo miglior CV e rispondi subito all'annuncio di lavoro perfetto per te](immagini/TrovaLavoro-readme-1200x1052.png)

# AI-CV-COACH — TrovaLavoro

**TrovaLavoro** è un'applicazione per Windows 11 che prepara candidature di lavoro con l'AI,
**senza inventare niente**. *AI-CV-COACH* è il nome del progetto e del repository;
*TrovaLavoro* è il nome del programma che ne è nato.

**Stato:** versione **1.0** rilasciata il 2026-08-24; da allora si fanno rifiniture.
Il racconto passo per passo sta nel [`diario_di_bordo.md`](diario_di_bordo.md).

## Che cosa fa

Gli racconti **una volta** chi sei. Poi, per ogni annuncio che gli dai:

- **Profilo**: lo costruisci con un dialogo guidato oppure lo importi da un CV (PDF, DOCX, TXT,
  MD) o dalla tua pagina LinkedIn. È uno solo, e ogni versione confermata viene conservata.
- **Annuncio**: lo incolli, oppure lo catturi dal portale con il browser integrato. Il programma
  ne ricava ruolo, requisiti e contatto.
- **Confronto**: il profilo viene messo a fianco dell'annuncio voce per voce, con un
  **punteggio da 0 a 5 stelle** calcolato dal programma e non dal modello. I requisiti
  eliminatori fanno da sbarramento.
- **Ponti onesti**: per i requisiti scoperti il programma cerca quello che hai davvero e che ci
  si avvicina, e lo dice. Non riempie il vuoto.
- **Documenti**: scrive un **🎯 CV mirato** e una **✉️ lettera** per quell'annuncio, in italiano
  o in inglese (in base alla lingua dell'annuncio), e li salva in **DOCX e PDF**.
- **Email**: prepara il messaggio con gli allegati già attaccati e lo apre nel tuo programma di
  posta. A spedire sei tu.
- **A che punto sono**: la Home tiene la coda delle candidature, cosa è stato fatto, l'esito e
  chi aspetta una risposta da troppo tempo.
- **Seconda porta (MCP)**: lo stesso eseguibile, avviato con `--mcp`, mette tredici strumenti a
  disposizione di un assistente AI esterno.

## Il principio

Due regole del prodotto, scritte dentro i prompt e sorvegliate dai collaudi:

- **Anti-invenzione**: il programma non deve mai inventare esperienze, competenze, titoli di
  studio o risultati che il profilo non contiene. Vale anche quando traduce: un «diploma di
  perito» non diventa un *degree*.
- **Anti-perdita**: niente di quello che l'utente dichiara va perso, nemmeno se lo dice nel
  momento sbagliato. Va spostato dove serve oppure dichiarato «lasciato fuori», mai fatto
  sparire in silenzio.

## Per chi vuole usarlo

La guida d'uso è in **[`GUIDA.md`](GUIDA.md)**: requisiti, primo avvio, la chiave API e quanto
costa, dove finiscono i dati, il backup. È scritta per chi usa il programma; questo README è
per chi vuole capire com'è fatto.

L'eseguibile **non è versionato qui**, perché pesa oltre 100 MB: ingloba il runtime .NET. Si
costruisce con `VB.NET/src/publish.bat`, e la procedura completa è nel §13.9 di
[`VB.NET/progetto/13_distribuzione.md`](VB.NET/progetto/13_distribuzione.md). L'exe non è
firmato, quindi al primo avvio Windows mostra l'avviso SmartScreen: «Ulteriori informazioni →
Esegui comunque». Prima, leggi la [licenza](#licenza).

## Com'è fatto

- **Piattaforma**: VB.NET su **.NET 10 LTS** con Windows Forms, distribuito come **un solo
  `.exe`** autonomo. Niente installazione, niente DLL a fianco; i dati stanno in
  `%APPDATA%\TrovaLavoro`.
- **Modelli**: due livelli, scelti per il tipo di compito. **Claude Haiku 4.5** si occupa
  dell'estrazione, **Claude Sonnet 5** del ragionamento e della scrittura. I nomi dei modelli
  non sono cablati nel codice: si possono cambiare da `modelli.json` senza ricompilare.
- **Prompt**: vivono fuori dal codice, in una libreria di file `.md` (il *pool*) inglobata
  nell'eseguibile, con un manifest e un'impronta SHA-256 per ogni file. Oggi è il **Pool 1.13**.
- **Documenti senza librerie esterne**: il DOCX nasce componendo a mano lo ZIP OOXML; il PDF lo
  stampa il motore WebView2 già presente in Windows. Word non serve.
- **Chiave API** cifrata con la protezione dati di Windows (DPAPI). Il programma non spedisce
  email: scrive un file `.eml` e lo consegna al tuo programma di posta.
- **Chiamate all'AI** in HTTPS dirette, senza SDK. Lo streaming si usa in un solo punto, il
  ragionamento sulla candidatura, perché lì c'è una persona che legge mentre il testo arriva.
- **Server MCP** con JSON-RPC scritto a mano. Parla sia la forma del protocollo con handshake
  sia quella senza stato.
- **Collaudi**: oltre 1400 collaudi automatici senza rete, più un banco che usa l'API vera.

Il disegno completo è in [`VB.NET/progetto/`](VB.NET/progetto/00_INDICE.md); la bussola
dell'architettura è il capitolo [`02_architettura.md`](VB.NET/progetto/02_architettura.md).

## Come è stato sviluppato

Questo progetto è stato costruito **con l'assistenza di un modello linguistico** — Claude, di
Anthropic, usato attraverso Claude Code — e la cosa va detta invece di lasciarla indovinare a chi
guarda la cronologia dei commit.

La divisione del lavoro è stata questa:

- **Mia** (Mirco Parenti): il perimetro e gli obiettivi, il disegno del prodotto, le decisioni
  di progetto e il loro *perché*, le regole di lavoro (`CLAUDE.md`: diciassette regole, nate una
  per una da incidenti veri), i prompt e la loro evoluzione, le prove dal vivo sull'applicazione
  vera e sui miei dati veri, la lettura degli esiti, e la decisione su che cosa fosse finito e
  che cosa no.
- **Dell'assistente**: la scrittura del codice VB.NET riga per riga, i collaudi, e gran parte
  della stesura dei documenti di progetto — sempre su istruzione, e sempre riletti e verificati.

Il metodo è scritto e ripetibile, ed è la parte del lavoro che considero mia più del codice:
ogni tappa si apre con un elenco di impegni e si chiude rileggendoli uno per uno (regola 16);
ogni collaudo che sorveglia un meccanismo viene **fatto fallire apposta** prima di dirlo buono
(regola 14); quel che resta indietro si dichiara in `in_sospeso.md` invece di sparire (regola
15). Il `diario_di_bordo.md` racconta i passi in prima persona, errori compresi.

Detto in breve, per chi legge questo repository per capire che cosa so fare: **dirigere e
verificare un lavoro di software fatto con l'AI** — definire il perimetro, imporre un metodo,
provare dal vivo, riconoscere i propri errori — e il **prompt engineering**, che è la parte che
ho progettato riga per riga. Non la scrittura autonoma di codice .NET, che qui è
dell'assistente.

È un progetto di apprendimento, nato come tirocinio presso Aviolab AI.

## Storia del progetto

| Tappa | Quando | Che cosa ha portato |
|---|---|---|
| Prototipo HTML+JS | mag – ago 2026 | MVP nel browser con un aiutante Node: profilo, annuncio, confronto con stelle, CV e lettera. Oggi è congelato |
| T0 – T1 | 5 – 6 ago | Decisioni di progetto chiuse; ambiente pronto, verificata la pubblicazione a exe singolo |
| T2 | 7 ago | Il motore: pool dei prompt, calcolo del match, client dell'AI; non-regressione contro il prototipo |
| T3 | 9 ago | Il profilo: scheda, dialogo guidato, import di un CV, storico delle versioni |
| T4 | 11 ago | La pipeline di candidatura: confronto, ponti onesti, CV mirato e lettera in DOCX e PDF |
| T5 | 12 – 14 ago | Browser integrato, cattura degli annunci, Home con la coda, profilo da LinkedIn |
| T6 | 14 ago | Email `.eml` con gli allegati, chiave API cifrata, classificazione dei documenti |
| T7 | 15 – 18 ago | Candidature in inglese, passata anti-slop, brainstorming in streaming |
| T8 | 19 – 21 ago | Server MCP con tredici strumenti e lucchetto della cartella dati |
| T9 → 1.0 | 21 – 24 ago | Backup, Impostazioni, esiti e promemoria, rifinitura, rilascio **1.0.000** |
| Revisione | 1 – 2 set | Sicurezza, interfaccia e confronto fra il promesso e il fatto (pull request #1) |
| Rifiniture | da settembre | Nascono usando il programma: il riconfronto, lo stato della procedura nella Home, le spie |

Il dettaglio di ogni passo, compresi gli errori, sta nel [`diario_di_bordo.md`](diario_di_bordo.md).
Quello che è rimasto indietro sta in [`in_sospeso.md`](in_sospeso.md), le idee per il futuro in
[`idee_future.md`](idee_future.md).

## Struttura del repository

| Percorso | Che cosa contiene |
|---|---|
| `VB.NET/src/` | Il codice dell'applicazione, il pool dei prompt e i collaudi |
| `VB.NET/progetto/` | Il progetto dettagliato, capitolo per capitolo |
| `HTML+JS/` | Il prototipo web, congelato; resta il termine di paragone per la non-regressione |
| `strumenti/` | Attrezzi di sviluppo e di collaudo (non fanno parte del prodotto) |
| `guida-rapida/` | La guida rapida stampabile per chi usa il programma |
| `immagini/` | Gli asset del marchio |

## Licenza

Il sorgente è pubblicato per essere **letto**: come portfolio, per studio e per verificare il
lavoro svolto. **Non** è una licenza open source: usarlo, modificarlo o ridistribuirlo richiede
un'autorizzazione scritta. I termini completi sono in [`LICENSE`](LICENSE).

---

© 2026 Aviolab AI
