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
  completa regge. **Misura dell'app vera, al publish di T1 (2026-08-06): 116 MB in un
  file solo, avvio in ~0,26 s a freddo** — cioè lo scheletro non costa nulla in più del
  runtime che si porta dietro, e il margine sulla stima è tutto per le funzioni che
  verranno.
- **`DebugType = none` non è un dettaglio**: senza quel parametro accanto all'exe resta
  il file dei simboli `.pdb`, e il vincolo «nessuna DLL, un file solo» decade nella
  forma. Con esso, al collaudo del 2026-08-06 la cartella di pubblicazione conteneva
  **un solo file**.
- **`AllowedReferenceRelatedFileExtensions = none`, la seconda trappola dello stesso
  genere** *(scoperta il 2026-08-10, prova a vuoto di T4)*. Finché le librerie esterne
  non c'erano, il `.pdb` era l'unico file che si intrufolava. Ma un pacchetto NuGet
  porta con sé anche la propria **documentazione IntelliSense**, e quella il publish la
  copia accanto all'eseguibile: con il solo `Microsoft.Web.WebView2` la cartella si è
  ritrovata **tre `.xml`** da 800 KB complessivi a fianco dell'exe. Non sono DLL, ma la
  promessa del capitolo è «un file da copiare», e tre file di contorno la rompono
  ugualmente. Il parametro li esclude tutti in un colpo, per ogni pacchetto presente e
  futuro — vale anche per `FontAwesome.Sharp`, che non è ancora entrato. Verificato:
  con esso la cartella di pubblicazione torna a contenere **un solo file**.
  *Dal 2026-08-10 (T4b) il parametro è in `publish.bat`*, insieme al pacchetto vero:
  finché la prova era a vuoto stava solo scritto qui, e una precauzione che vive in un
  capitolo non protegge nessuna pubblicazione.
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

*Misurato il 2026-08-10, prima di scrivere una riga di T4.* Il capitolo dava WebView2
per «inglobabile» sulla fiducia: era la scommessa più grossa rimasta, perché è la sola
libreria che porta **codice nativo** dentro un exe che deve restare uno solo. La prova
è stata fatta a vuoto, fuori dal repo, con gli stessi parametri di `publish.bat`:

| Domanda | Risposta misurata |
|---|---|
| Il single-file regge il codice nativo? | Sì — **117,2 MB in un file solo**, contro i 116 MB dello scheletro di T1: WebView2 costa **~1,2 MB** |
| L'exe pubblicato funziona davvero? | Sì — provato con la cartella dei dati di navigazione **cancellata**, cioè come su un PC che non l'ha mai avviato |
| Servono file a fianco? | No, una volta esclusa la documentazione dei pacchetti (v. 13.2) |
| La stampa PDF con la WebView **fuori schermo** (cap. 05.5)? | Sì, e il PDF prodotto ha testo **selezionabile e ricercabile**, accenti in chiaro e font incorporati |

Il pacchetto usato è `Microsoft.Web.WebView2` **1.0.4129.50**, contro il runtime
Evergreen `151.0.4129.72` trovato sulla postazione: il numero di build coincide, che è
la condizione che conta — l'API del pacchetto non può chiedere al motore cose che il
motore non sa fare.

*Il pacchetto è entrato davvero nel progetto il 2026-08-10, con T4b, e il publish è stato
rifatto sul codice vero:* **117,5 MB in un file solo**, in linea con la prova a vuoto. Due
cose scoperte mettendolo dentro. La prima è che il pacchetto porta **due involucri**, uno
per Windows Forms e uno per WPF: il secondo non serve a nessuno qui e si tira dietro una
versione di `WindowsBase` diversa da quella del framework, che la compilazione segnala
come conflitto irrisolvibile. Il riferimento si toglie in `VB.NET/src/Directory.Build.targets`,
che vale per l'applicazione e per il banco: un avviso vero su una dipendenza inutile è
peggio di nessun avviso, perché insegna a non leggerli. La seconda è che WebView2 chiude
i suoi processi **dopo** che il controllo è stato smesso, e finché non ha finito tiene il
proprio `lockfile`: chi cancella la sua cartella subito dopo (un collaudo) deve avere la
pazienza di riprovare.

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
icona, eseguibile `TrovaLavoro.exe`, sottotitolo «Crea il tuo miglior CV e rispondi
subito all'annuncio di lavoro perfetto per te!» *(cambiato il 2026-08-22 insieme al
banner del logo: prima era «e candidati con il CV giusto, senza fatica»)*. Il criterio
è la comprensibilità per chi non è tecnico — sul modello di *TrovaPrezzi*: verbo più
nome, si detta al telefono senza compitarlo e dice da sé che cosa fa. **Il nome del
progetto, del repository e di questi documenti resta AI-CV-COACH**: cambia solo ciò
che appare all'utente.

Le **proprietà dell'eseguibile** (scheda «Dettagli» di Windows) si compilano a T1
insieme al resto:

| Proprietà | Valore |
|---|---|
| Nome prodotto | TrovaLavoro |
| Società | Aviolab AI |
| Copyright | © 2026 Aviolab AI |
| Descrizione | Trova annunci, prepara CV e lettera su misura |

- La versione dell'app vive in **un solo file sorgente** (`Versione.vb`): formato
  `maggiore.minore.build`, mostrata nel pannello logo insieme alla versione del pool —
  oggi `Ver. 0.3.046 · Pool 1.13` — schema confermato in cap. 15, voce 5. Le due versioni
  corrono separate di proposito: il pool ha una storia sua (cap. 04.1), e l'etichetta
  dichiara da sé sorgente e stato («integrato», l'asterisco di chi sperimenta).
- Ogni modifica al codice incrementa il numero di build; la storia delle release sta
  nei tag Git del repo.
- Il logo è lo **scudo di Aviolab AI**, incorporato **in forma binaria nel sorgente**
  (PNG codificato Base64 in `LogoAviolab.vb`): nessun file immagine nel repo né accanto
  all'exe *(deciso 2026-08-06 in T1 — cap. 15, voce 4)*. Nel pannello logo, sotto lo
  scudo, compare sempre e solo «AVIOLAB AI» (cap. 03.5).
- **L'icona dell'eseguibile c'è, e viene da quello stesso scudo** *(T9e, 2026-08-22)*.
  Ricavata dal PNG di `LogoAviolab.vb` — ritagliata al contorno della figura e ricentrata,
  altrimenti a 16 px lo scudo sarebbe un francobollo dentro un riquadro mezzo vuoto — in
  sette misure (16, 24, 32, 48, 64, 128, 256), le prime cinque come bitmap e le due grandi
  compresse in PNG, che è il formato che Windows si aspetta. Vive in
  `VB.NET/src/TrovaLavoro/Risorse/TrovaLavoro.ico` ed è **l'unico file immagine del
  prodotto nel repository**: non è una deroga di comodo, è che `<ApplicationIcon>` di
  MSBuild vuole un file su disco e un Base64 nel sorgente non lo sa leggere. Il vincolo
  vero — *niente file accanto all'exe* — resta intatto: quel file entra **dentro**
  l'eseguibile, due volte, come icona di Windows e come risorsa incorporata per la finestra
  principale. La stessa cartella ospita `schermata-avvio.png`, il marchio della schermata
  di avvio (cap. 03.4), anch'esso incorporato.
- **Le finestre secondarie non prendono l'icona**, e non è una dimenticanza: sono tutte
  `FixedDialog`, e Windows in quella cornice l'icona non la disegna affatto.

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
└── src/                       la soluzione Visual Studio
    ├── TrovaLavoro.sln
    ├── publish.bat            l'exe di rilascio, sempre con gli stessi parametri
    ├── TrovaLavoro/           l'applicazione (WinForms)
    │   └── prompt-pool/       il pool dei prompt: file veri nel repo, risorse nell'exe
    └── TrovaLavoro.Collaudi/  il banco di collaudo, fuori dal rilascio
        └── casi/              i casi della non-regressione contro il prototipo (cap. 14)
            └── reale/         gli esiti dei collaudi con l'API vera, quando i dati sono inventati
```

- Il **pool vive dentro il progetto dell'applicazione** perché di lì entra nell'exe come
  risorsa incorporata (cap. 04.2): la stessa cartella serve anche da pool esterno di
  prova, senza copie da tenere allineate.
- Fuori da `VB.NET/`, in radice, c'è **`strumenti/`** *(da T4c, 2026-08-10)*: attrezzi di
  sviluppo che **non fanno parte del prodotto** e non entrano in nessuna pubblicazione —
  oggi il server MCP di collaudo (cap. 09.1), che compila, fa girare il banco e prova
  l'applicazione vera. Stanno nel repo perché sono ripetibili e servono su tutt'e due le
  postazioni, non perché si distribuiscano.
  *Da T6 (2026-08-14) ce n'è un secondo, **`strumenti/sigilla-pool`**: rigenera le impronte
  del manifest chiamando lo stesso codice del caricatore (cap. 04.5). Esiste perché quel
  comando, nel disegno, doveva vivere nelle Impostazioni — cioè a T9 — mentre i prompt si
  toccano da T2. Come l'altro, è un progetto a sé che non entra in nessuna pubblicazione.*
  *E a **T9b** (2026-08-21) si è deciso che nelle Impostazioni non ci andrà mai: il
  manifest sta nel repo, non nella cartella dati, e un exe distribuito non avrebbe nulla di
  utile da sigillare (cap. 04.5). Questo strumento non è più un anticipo: è il posto.*
- Build di sviluppo da Visual Studio (o `dotnet build` da riga di comando Windows);
  collaudi con `dotnet test` da `VB.NET/src` (cap. 14).
- Lo script `publish.bat` in `src/` produce l'exe di rilascio con i parametri del
  punto 13.2, sempre uguali: la pubblicazione non deve dipendere dalla memoria di
  nessuno.

## 13.8 Aggiornamenti

Prima versione: aggiornamento **manuale** — si scarica il nuovo exe e si sostituisce
il vecchio; i dati in `%APPDATA%` non si toccano e il programma riparte da dove era.
Niente auto-update in perimetro: per un'app personale è complessità senza guadagno
(annotato comunque tra le idee future).
