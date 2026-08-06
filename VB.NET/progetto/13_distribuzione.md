# 13 — Distribuzione: un solo exe

*Come il programma arriva sul PC dell'utente e ci resta: un file da copiare, senza
installazione, senza DLL a fianco.*

## 13.1 Stack e piattaforma

- **Linguaggio**: VB.NET.
- **Interfaccia**: Windows Forms (i pannelli statici del cap. 03, disegnati nel
  designer di Visual Studio).
- **Runtime**: **.NET 10 LTS** *(deciso in cap. 15, voce 1)*. La proposta iniziale era
  .NET 8, scartata perché **esce di supporto il 10/11/2026** ed è già in sola
  manutenzione; .NET 10 è in supporto attivo fino a **novembre 2028**. Richiede l'SDK 10
  su entrambe le postazioni: **installato su aviolab03 il 2026-08-06** (SDK 10.0.302,
  runtime 10.0.10, accanto al 10.0.204 arrivato con Visual Studio 2026 Community 18.5);
  sulla postazione del tutor resta da fare.
- **Architettura**: 64 bit (`win-x64`), solo Windows 11.
- **Librerie esterne**: le minime indispensabili, e tutte «inglobabili» —
  `Microsoft.Web.WebView2` (browser integrato) e `FontAwesome.Sharp` (icone).
  Per JSON si usa quello integrato in .NET (`System.Text.Json`); per DOCX, ZIP,
  cifratura e scrittura `.eml` si usano i mattoni standard di .NET. Meno dipendenze =
  meno sorprese. *(Niente SMTP: l'invio diretto è fuori dalla 1.0 — cap. 15, voce 9.)*

## 13.2 La pubblicazione single-file

Il vincolo «un exe, niente DLL» si realizza con la pubblicazione a file singolo di
.NET:

```
dotnet publish -c Release -r win-x64
  PublishSingleFile = true
  SelfContained     = true
  IncludeNativeLibrariesForSelfExtract = true
  DebugType         = none
```

- **Autonomo (`SelfContained = true`)** — **scelta confermata** (cap. 15, voce 2):
  l'exe contiene **anche il runtime .NET**. Pesa di più (≈150–180 MB) ma funziona su un
  PC nudo: copi il file, parte. Per un'app-biglietto-da-visita è l'esperienza giusta:
  nessun «prima installa il runtime».
- **Senza compressione.** `EnableCompressionInSingleFile` dimezzerebbe grosso modo il
  file, al prezzo di un primo avvio più lento (il contenuto va decompresso in una cache
  locale): non conviene per un programma che vive su due PC. Prima misura di
  riferimento (2026-08-06, WinForms VB **vuoto** su .NET 10, autonomo e non compresso):
  **116 MB in un unico file**, avviato e verificato — la stima dei 150–180 MB per l'app
  completa regge. La misura vera — dimensione e tempo di avvio dell'app vera — si prende
  al **publish di prova di T1**.
- **`DebugType = none` non è un dettaglio**: senza quel parametro accanto all'exe resta
  il file dei simboli `.pdb`, e il vincolo «nessuna DLL, un file solo» decade nella
  forma. Con esso, al collaudo del 2026-08-06 la cartella di pubblicazione conteneva
  **un solo file**.
- **Il trimming non è un'opzione**: Microsoft non lo supporta su Windows Forms, quindi
  la stima di dimensione non è riducibile per quella via.
- La variante leggera (framework-dependent, pochi MB ma richiede il runtime .NET
  installato) resta il formato comodo per lo sviluppo quotidiano.
- Le librerie NuGet e le risorse (pool di prompt integrato, logo, modelli DOCX/HTML)
  finiscono **dentro** l'exe; le parti native si estraggono da sole al primo avvio in
  una cartella temporanea gestita da .NET. Accanto all'exe non vive **nessuna** DLL.

## 13.3 Il caso WebView2

WebView2 usa il motore Edge/Chromium **già presente in Windows 11** (runtime
«Evergreen», preinstallato e aggiornato dal sistema): non lo distribuiamo noi. Se per
qualche motivo mancasse (edizioni particolari, PC aziendali bloccati), l'app **non
muore**: all'avvio lo rileva e mostra una finestra cortese con il link ufficiale di
Microsoft per installarlo, mentre le funzioni che non lo usano restano disponibili.

## 13.4 Cosa c'è accanto all'exe

| Elemento | Obbligatorio? |
|---|---|
| `TrovaLavoro.exe` | sì — è tutto qui |
| `prompt-pool\` | no — solo per chi vuole aggiornare o studiare i prompt senza ricompilare (il pool integrato basta) |
| strumenti esterni opzionali (es. un elaboratore PDF della casa, **solo come exe binario**) | no — l'app funziona senza; se presenti, l'app li rileva |

I **dati** non stanno mai accanto all'exe: vivono in `%APPDATA%\TrovaLavoro`
(cap. 11). Così l'exe può stare su desktop, in una cartella qualsiasi o su chiavetta
senza sparpagliare file.

## 13.5 Nome, versione e identità

**Il nome che l'utente legge è «TrovaLavoro»** (cap. 15, voce 3): barra del titolo,
icona, eseguibile `TrovaLavoro.exe`, sottotitolo «e candidati con il CV giusto, senza
fatica». Il criterio è la comprensibilità per chi non è tecnico — sul modello di
*TrovaPrezzi*: verbo più nome, si detta al telefono senza compitarlo e dice da sé che
cosa fa. **Il nome del progetto, del repository e di questi documenti resta
AI-CV-COACH**: cambia solo ciò che appare all'utente.

Le **proprietà dell'eseguibile** (scheda «Dettagli» di Windows) si compilano a T1
insieme al resto:

| Proprietà | Valore |
|---|---|
| Nome prodotto | TrovaLavoro |
| Società | Aviolab AI |
| Copyright | © 2026 Aviolab AI |
| Descrizione | Trova annunci, prepara CV e lettera su misura |

- La versione dell'app vive in **un solo file sorgente** (`Versione.vb`): formato
  `1.0.012` (maggiore.minore.build), mostrata nel pannello logo insieme alla versione
  del pool (`Ver. 1.0.012 · Pool 1.03`) — schema confermato in cap. 15, voce 5.
- Ogni modifica al codice incrementa il numero di build; la storia delle release sta
  nei tag Git del repo.
- Il logo è lo **scudo di Aviolab AI**, incorporato **in forma binaria nel sorgente**
  (PNG codificato Base64 in `LogoAviolab.vb`): nessun file immagine nel repo né accanto
  all'exe *(deciso 2026-08-06 in T1 — cap. 15, voce 4)*. Nel pannello logo, sotto lo
  scudo, compare sempre e solo «AVIOLAB AI» (cap. 03.5). L'icona dell'exe resta da
  produrre, idealmente dallo stesso scudo.

## 13.6 Avvisi di Windows (SmartScreen)

Un exe nuovo e non firmato può far comparire l'avviso SmartScreen («PC protetto da
Windows»). Per la fase personale/portfolio è accettabile e si documenta nel README
(«Ulteriori informazioni → Esegui comunque»); la **firma del codice** con un
certificato è un'opzione futura annotata in cap. 15, non un prerequisito.

*Da tenere presente se il programma uscirà dalle due postazioni di casa:* quell'avviso
è un ostacolo serio proprio per il pubblico a cui il nome si rivolge — chi non è pratico
lo legge come «è un virus» e si ferma. È il momento in cui la firma del codice smette di
essere un'opzione.

## 13.7 L'area di sviluppo nel repo

```
VB.NET/
├── PROMPT_DI_INCARICO.md      il mandato (immutabile)
├── progetto/                  questi documenti di progetto
├── prompt-pool/               nascerà a inizio implementazione, migrando i prompt dal prototipo
└── src/                       la soluzione Visual Studio (TrovaLavoro.sln, progetto WinForms)
```

- Build di sviluppo da Visual Studio (o `dotnet build` da riga di comando Windows).
- Uno script `publish.bat` in `src/` produrrà l'exe di rilascio con i parametri del
  punto 13.2, sempre uguali: la pubblicazione non deve dipendere dalla memoria di
  nessuno.

## 13.8 Aggiornamenti

Prima versione: aggiornamento **manuale** — si scarica il nuovo exe e si sostituisce
il vecchio; i dati in `%APPDATA%` non si toccano e il programma riparte da dove era.
Niente auto-update in perimetro: per un'app personale è complessità senza guadagno
(annotato comunque tra le idee future).
