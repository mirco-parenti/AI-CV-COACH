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
  oggi `Ver. 1.0.000 · Pool 1.13` — schema confermato in cap. 15, voce 5. Le due versioni
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

**Dal 2026-08-27 l'aggiornamento manuale ha un avviso** *(revisione del giro D)*. Restava
vero il pezzo difficile — chi non guarda il repository non sa mai che è uscita una versione
nuova — e la cura è piccola: in «Informazioni» c'è **«Cerca aggiornamenti»**, che chiede a
GitHub qual è l'ultima release pubblicata e confronta il numero con quello di questa copia.

Quattro scelte dentro quelle poche righe — l'ultima aggiunta dopo averlo premuto davvero.

- **Parte solo premendolo.** Non all'avvio, non una volta al giorno, mai da sola: il cap. 11.2
  promette «niente aggiornamenti automatici silenziosi», e una domanda che l'utente pone è il
  contrario del silenzio. Chi non lo preme non manda niente a nessuno — e un collaudo lo
  sorveglia, perché una promessa senza collaudo dura fino alla prossima distrazione.
- **Confronta numeri, non stringhe.** «1.0.000» è la costante di `Versione.vb`, «v1.0» è il
  tag del rilascio: sono la stessa versione scritta in due posti, e a caratteri direbbero di
  no. I pezzi che mancano valgono zero.
- **Ha un terzo esito oltre a «aggiornata» e «ce n'è una nuova»**: *più avanti del pubblicato*,
  cioè la macchina di chi costruisce il programma. Dirle «sei aggiornata» sarebbe falso e
  dirle «scarica la nuova» assurdo.
- **Un 404 non è un guasto** *(2026-08-27, premendo il bottone per la prima volta)*. Finché di
  release pubblicate non ce n'è nessuna — cioè finché il tag `v1.0` aspetta il giro D rifatto
  — GitHub risponde `404` a `releases/latest`, ed è la risposta che tocca **a chi ha in mano
  la prima versione**. Raccontargliela col numero («il servizio ha risposto 404») lo manda a
  cercare un difetto che non c'è: adesso quel caso ha una riga sua, «non risulta pubblicata
  nessuna versione», mentre un guasto vero porta ancora il proprio numero, che lì è la sola
  cosa da riferire.

Se c'è davvero una versione nuova, il programma apre la pagina delle release: a scaricare e a
sostituire l'eseguibile ci pensa la persona. Resta fuori perimetro l'auto-update, e resta
fuori per la stessa ragione di sempre — un programma che si riscrive da solo chiede più
fiducia di quanta ne serva qui.

## 13.9 Come si fa un rilascio

*Fin qui il capitolo ha detto **con che parametri** si pubblica e **che aspetto** ha il
risultato; questo paragrafo dice **in che ordine** si fanno le cose, perché un rilascio non
è un gesto — è una sequenza, e la parte che si dimentica è sempre la stessa: la verifica
dell'eseguibile vero, che nessun collaudo automatico copre.*

**I passi, nell'ordine.**

1. **Si parte dal pulito.** Working tree senza modifiche pendenti, sul ramo della tappa. Un
   rilascio fatto sopra a un lavoro a metà non si sa più che cosa contenga.
2. **Il numero si cambia in un posto solo** — la costante di `Versione.vb` (13.5). Il
   `.vbproj` la rilegge da lì con una regex, quindi le proprietà dell'eseguibile seguono da
   sé e non c'è un secondo posto da tenere allineato.
3. **Il pool si chiude prima.** Se un prompt è stato toccato, il rito del bump (cap. 04.5)
   va finito adesso: l'etichetta che l'utente leggerà dev'essere `Pool x.yy (integrato)`,
   senza l'asterisco di chi sperimenta.
4. **Il banco intero, prima di pubblicare.** `dotnet test` da `VB.NET/src`: il
   `collaudi.runsettings` dichiarato nel progetto tiene già fuori i collaudi che vogliono
   l'API vera, quindi il comando nudo è quello giusto. Non si pubblica una versione che non
   è stata provata. *Se il **server MCP del prodotto** è vivo (modalità `--mcp`, cap. 09)
   tiene bloccato l'eseguibile e la compilazione si ferma: va chiuso per PID prima di
   cominciare, sapendo che i suoi tool spariranno fino al riavvio del client.*
5. **La cartella di pubblicazione si svuota prima.** Altrimenti «un solo file» non è una
   verifica ma un'ispezione su una cartella già sporca: un `.pdb` o un `.xml` rimasto da un
   publish precedente passerebbe per assenza di problema. Il vincolo del 13.2 si prova solo
   partendo dal vuoto.
6. **Si pubblica con `publish.bat`**, mai a mano: i parametri del 13.2 stanno lì perché
   la pubblicazione non dipenda dalla memoria di nessuno (13.7).
7. **Le quattro verifiche sull'eseguibile.** Sono la ragione d'essere di questo paragrafo,
   perché nessuna di esse è nel banco:

   | Verifica | Come | Perché |
   |---|---|---|
   | **Un solo file** | contare i file della cartella, non guardarli | è il vincolo più rigido del progetto (13.2) |
   | **Le proprietà** | leggerle **dall'exe**, non dal `.vbproj` | `ProductVersion`, `ProductName`, `Company`, `Copyright`, `Descrizione` (13.5): quel che conta è ciò che Windows mostra, non ciò che si è scritto |
   | **L'avvio vero** | lanciarlo con `--dati` su una cartella usa-e-getta e leggere la riga del pannello logo | un exe che si compila non è un exe che parte; e la riga dice insieme versione e pool |
   | **La dimensione** | annotarla in byte | si confronta con quella del rilascio prima: uno scarto grosso è una dipendenza entrata di nascosto |

8. **Poi la documentazione**, non prima: README (lo stato), `diario_di_bordo.md` (lo Step),
   i capitoli che il rilascio smentisce, `in_sospeso.md` (quel che resta).
9. **Commit, merge e tag.** Il tag si mette sul commit di `main` dopo il merge, nella forma
   `v<maggiore>.<minore>`. Il push è un gesto separato e lo decide Mirco: un tag spinto
   per inerzia pubblica una versione che nessuno ha ancora deciso di pubblicare.

**Che cosa un rilascio non fa**, e va detto perché la parola promette più di quel che qui
significa: non **firma** il codice — SmartScreen avviserà (13.6); non pubblica niente
online — l'eseguibile si porta a mano sulla macchina che deve provarlo; e non aggiorna
nessuno — l'aggiornamento è manuale (13.8).

*La prima volta è stata la **1.0.000**, il 2026-08-24 (sesto tempo di T9e), e questa
sequenza è il resoconto di come è andata: **1110 collaudi verdi**, cartella svuotata,
`publish.bat`, **118.707.086 byte in un file solo** (113,2 MiB) contro i 118.633.358 della
0.3.041 di due giorni prima — **73.728 byte** di differenza, cioè le cure del quinto
tempo e nient'altro — proprietà rilette dall'eseguibile, e l'avvio provato su
`C:\Temp\tl-rilascio-10` con il pannello logo che diceva `Ver. 1.0.000 · Pool 1.13
(integrato)`.*

## 13.10 Il copione del giro D

*Il 13.9 finisce dove l'eseguibile è pronto e verificato **qui**. Il giro D è il passo dopo:
portarlo su una macchina che non ha mai visto questo progetto — **senza SDK .NET 10, con
Word** — e guardare se il vincolo più rigido regge davvero. Fin qui è dimostrato al 90%: la
pubblicazione single-file è stata provata sulla macchina che ha l'SDK installato, e non è la
stessa cosa. Questo paragrafo è il **copione**: cosa si prova, in che ordine, con quali dati.
Si scrive **prima** di partire, perché il giro C di T9e ha già mostrato cosa costa
improvvisare — un giro intero condotto sul profilo sbagliato.*

> **Da non confondere con i «copioni» di `strumenti/collauda-copioni/`**, che sono i due
> script JavaScript di `LettorePagina` provati su DOM finti (i «copioni 10 su 10» del
> diario). Stessa parola, due cose senza parentela: quelli sono un banco automatico, questo
> è un giro fatto da persone.

**Che cosa il giro D deve dimostrare.** Quattro debiti aperti da tappe diverse, che si
chiudono tutti con **una** macchina sola — più tre riserve del ramo delle rifiniture, curate
al banco e mai viste dal vivo:

| # | Che cosa | Aperta da | Si dice passata quando |
|---|---|---|---|
| **D1** | L'exe parte su un PC **davvero pulito** | T1 *(2026-08-06)* | un file solo, copiato e avviato, arriva alla finestra principale su una macchina senza SDK né runtime .NET |
| **D2** | I documenti prodotti si aprono **in Word** | T4 *(2026-08-10)* | DOCX e PDF aperti in Word: **114 campi su 114** ritrovati, i due identici fra loro, accenti e simboli intatti — lo stesso criterio con cui passò LibreOffice |
| **D3** | Un `.docx` **salvato davvero da Word** si reimporta nel profilo | T3 *(2026-08-07)* | il profilo estratto da quel file è quello di partenza: finora i DOCX di prova erano fabbricati da noi, e provavano la strada di lettura, non l'impaginazione di Word |
| **D4** | La voce madre: quella macchina esiste ed è stata usata | T9e *(2026-08-23)* | D1, D2 e D3 sono state fatte **lì**, non raccontate |
| **R-a** | La selezione nel `ListView` **vero** | rifiniture *(2026-08-24)* | in «Modifica i testi», dopo un «Togli» e un «Rimetti», la riga scelta segue la voce e non torna in cima — al banco la finestra non viene mai mostrata |
| **R-b** | La barra delle Impostazioni **a 150%** | rifiniture *(2026-08-24)* | a scala 150% le Impostazioni scorrono **senza** barra orizzontale; a 96 DPI il difetto non compare affatto |
| **R-c** | Il messaggio davanti a una **candidatura orfana** | rifiniture *(2026-08-24)* | eliminato e rifatto il profilo, «Rigenera la lettera» su una candidatura vecchia dice cos'è successo invece di un errore di lettura |

*Le tre `R` si possono provare anche qui, e la `R-b` vuole comunque una disconnessione per
cambiare scala. Stanno in questo elenco perché se il giro D capita prima, tanto vale
chiuderle nello stesso viaggio.*

**Prima di partire, da fare qui.** Nessuno di questi passi si può recuperare là.

1. **L'eseguibile giusto, verificato per numero e per byte.** Non «l'exe che sta in
   `pubblicazione\`»: quella cartella ha già ospitato una **0.3.041** del 22 agosto rimasta
   lì a sembrare pronta, e portarla avrebbe misurato una versione superata proprio sul
   difetto che le cure avevano tolto. Si leggono `ProductVersion` **dall'exe** e la dimensione
   in byte, e si confrontano con quelle annotate al rilascio (13.9): la 1.0.000 è
   **118.707.086 byte**.
2. **I dati finti, costruiti e provati qui** *(decisione del 2026-08-24)*. Sulla macchina di
   qualcun altro non vanno né il mio CV né i miei recapiti: il giro si fa con un profilo
   inventato ma verosimile — nome parlante, `333 0000000`, un percorso di lavoro coerente —
   e un annuncio altrettanto finto, salvati come file da copiare insieme all'exe. Vanno
   **provati qui prima**: un profilo finto che l'estrazione non digerisce brucerebbe il giro
   là, e si scoprirebbe di aver misurato il dato, non il programma. Serve anche il CV finto
   in **PDF**, perché è da lì che il profilo entra.
3. **La chiave API.** Non si trasferisce copiando `segreti.bin`: è cifrato con DPAPI e legato
   all'utente Windows che l'ha scritto (cap. 11.3). Là si digita nella finestra del primo
   avvio, e la si decide **prima** di partire — quale chiave, e che si toglie andando via. Nel
   copione non si scrive mai.
4. **⚠ L'SDK .NET 10 su quella macchina si installa *dopo*, mai prima.** La voce di T1 lo
   chiede perché il tutor possa compilare, ed è giusta — ma il giro D vuole esattamente una
   macchina che non ce l'ha, ed è l'unica disponibile. Installarlo prima brucia la prova e
   non si torna indietro.

**Là: l'allestimento** *(dieci minuti, prima che cominci il giro).*

1. Si copia **il solo `TrovaLavoro.exe`** e la cartella dei dati finti. Se accanto all'exe
   finisce qualcos'altro, D1 non è più una prova.
2. Primo avvio: compare lo **SmartScreen** — l'eseguibile non è firmato (13.6) — e si passa
   con «Ulteriori informazioni → Esegui comunque». *Vale la pena farlo vedere al tutor: è la
   prima cosa che vedrebbe chiunque ricevesse questo file, ed è la faccia meno amichevole del
   progetto.*
3. Poi lo splash, e la **finestra della chiave** prima dei pannelli (cap. 03.4). Si inserisce
   la chiave. «Non adesso» è legittimo ma qui non serve: senza AI metà del giro non si fa.
4. Si avvia con **`--dati`** su una cartella dedicata di quella macchina, così tutto quel che
   il giro produce sta in un posto solo e si cancella in un gesto. La radice non predefinita
   l'applicazione la dichiara da sé nel titolo e nella barra di stato.
5. **Una finestra sola, aperta una volta.** È la trappola del giro C: due istanze si
   distinguono **solo dalla barra del titolo**, e un giro intero è finito sul profilo
   sbagliato senza che nessuno se ne accorgesse mentre andava.

**Primo tempo — guidi tu.** Serve a chiudere i debiti tecnici, che vogliono mano ferma e
ordine preciso. Il tutor guarda.

| Passo | Cosa si fa | Chiude |
|---|---|---|
| 1 | L'app parte, si legge nel pannello logo `Ver. 1.0.000 · Pool 1.13 (integrato)` | **D1** |
| 2 | Si importa il CV finto in PDF, si completa il profilo e si salva | *(prepara il resto)* |
| 3 | Si incolla l'annuncio finto, «Analizza», confronto e stelle | *(prepara il resto)* |
| 4 | Si generano 🎯 CV-2 e ✉️ lettera, si esportano **DOCX e PDF** | *(prepara D2)* |
| 5 | **Si aprono tutt'e due in Word** e si contano i campi: 114 su 114, i due identici | **D2** |
| 6 | Da Word si fa **«Salva con nome»** su un `.docx` nuovo, e lo si **reimporta** nel profilo | **D3** |
| 7 | In «Modifica i testi»: un «Togli», un «Rimetti», si guarda dove va la selezione | **R-a** |
| 8 | Si eliminano i dati del profilo, si rifà il profilo, si riapre la candidatura di prima e si preme «Rigenera la lettera» | **R-c** |
| 9 | Se quella macchina è a **150%** (o la si porta lì, con la disconnessione che serve): Impostazioni, si scorre | **R-b** |

*I passi 5 e 6 sono **la catena di Word**, ed è il cuore del giro: l'unica cosa che qui non si
può fare in nessun modo. Se il tempo stringe, si taglia tutto il resto e si tengono quelli.*

**Secondo tempo — le mani al tutor.** Qui non si chiude nessun debito: si scopre cosa capisce
chi non ha costruito l'applicazione. Vale se e solo se si rispetta una regola.

- Si dà **un compito solo**, detto in una frase e senza istruzioni: *«fatti un profilo e
  preparati una candidatura per questo annuncio»*. Niente di più.
- **Tu stai zitto.** Ogni volta che spieghi, cancelli il dato che sei venuto a prendere. Se si
  blocca del tutto, si annota **dove** e solo allora si aiuta.
- Si annota **dove esita**, non cosa sbaglia: un'esitazione di tre secondi davanti a un
  bottone dice più di un errore, perché l'errore lo racconterebbe lui e l'esitazione no.
- Il profilo del tutor è **suo**: se preferisce dati finti anche per sé, si usano quelli
  preparati. Quel che scrive resta sulla sua macchina e si cancella andando via.

**Come si annota un reperto.** Come nei giri A, B e C: sigla progressiva (`D-R1`, `D-R2`…),
una riga su cosa è successo, una sulla gravità, e — quando c'è — la fotografia. Si annota
**mentre il giro va**, su un file **fuori dal repo**: un reperto ricostruito a casa la sera è
già un ricordo, e i dettagli che contano sono i primi a sbiadire. Entrano nel repo dopo,
quando diventano voci di `in_sospeso.md` o cure.

**Prima di andare via.**

1. Si cancella la **cartella dati** del giro e i documenti esportati (Desktop compreso: gli
   export chiedono dove salvare e la prima volta propongono lì).
2. Si toglie la **chiave API**: dalle Impostazioni, o eliminando la cartella dati che contiene
   `segreti.bin`.
3. Si porta a casa: l'elenco dei reperti, le fotografie, e **quali delle sette voci** della
   tabella sono passate. Una voce non provata si dichiara tale — è la regola 15, e vale
   soprattutto per il giro che nasce apposta per chiudere le riserve altrui.
4. **Solo adesso**, se serve, si installa l'SDK .NET 10 e si chiude la voce di T1.

**Le trappole già pagate**, che questo copione esiste per non ripetere:

- **due finestre aperte si distinguono solo dal titolo** — giro C, condotto per intero sul
  profilo sbagliato;
- **l'exe che sembra pronto può essere di ieri** — la 0.3.041 trovata in `pubblicazione\`;
- **il DPI di sistema mente a chi non si è dichiarato DPI-aware**: `GetDpiForSystem` risponde
  96 anche a 150%, e la scala non cambia davvero senza **disconnessione**;
- **lo strumento di collaudo, a 150%, può dire «Premuto» senza aver premuto** — qui non serve
  (il giro è a mano), ma se lo si usa per fotografare, le immagini escono virtualizzate;
- **una voce data per aperta può essere già chiusa nel codice**: prima di portarsi dietro un
  elenco, lo si confronta con il codice. È costato dieci minuti scoprirlo, e due giorni non
  saperlo.

## 13.11 Quanto è costato: il contatore di spesa

*Aggiunto il 2026-08-27, dalla revisione del giro D.* Il programma usa una chiave che
l'utente paga a consumo, e fino alla 1.0 non gli diceva mai quanto stesse spendendo: lo si
poteva sapere solo dalla console di Anthropic, cioè altrove.

Il dato però c'era già. Dal 2026-08-18 ogni chiamata lascia una riga in `chiamate_ai.csv`
(cap. 11.1) con modello, token andati e venuti: nata per ritarare i `max_token` del pool, quel
file risponde anche all'altra domanda. Non si annota niente di nuovo — **bastava leggerlo**.

In *Impostazioni → Quanto è costato* compaiono chiamate, token e una **stima in dollari**, in
totale e negli ultimi trenta giorni, con il bottone che apre il CSV per chi vuole ordinarlo per
colonna. Quattro cautele, tutte della stessa famiglia:

- **è una stima e lo dice** — prezzi di listino, che non sanno di sconti né della cache dei
  prompt; la verità resta la fattura;
- **i dollari, non gli euro**: è la valuta in cui Anthropic fattura, e convertirla vorrebbe
  dire inventare un cambio;
- **un modello senza prezzo non vale zero**: i suoi token si contano, i suoi soldi no, e il
  buco si dichiara (cap. 11.6);
- **sotto il centesimo non si scrive «$0,00»**, che si legge come «gratis»: si dice *meno di
  un centesimo*, che è la cosa vera.

Il conto non è un registro contabile e non pretende di esserlo: una riga storta nel file si
salta e le altre valgono, perché quel CSV si apre in un foglio di calcolo e chiunque può
averci messo dentro una riga a mano.

*Aggiunta del 2026-08-27, guardando il contatore a occhio.* La terza cautela — «un modello
senza prezzo non vale zero» — è giusta e resta, ma quel giorno si stava applicando al posto
sbagliato: il buco si apriva sul **modello predefinito**, cioè su ogni installazione. La
ragione è nel cap. 02.5: nel CSV si scrive il modello che *ha risposto* (`claude-haiku-4-5-20251001`),
il listino conosce quello che *si chiede* (`claude-haiku-4-5`), e i due non si riconoscevano.
Adesso il prezzo si cerca prima per identificativo esatto — così chi in `modelli.json`
dichiara il prezzo di una versione precisa ottiene quello — e poi per modello, ignorando il
suffisso della data. Sui dati veri di questa macchina il conto è passato da $0,38 a $0,40, e
la riga «di 3 chiamate non conosco il prezzo del modello» è sparita perché non aveva più
niente da dichiarare.

*Su come è stato trovato* c'è poco da aggiungere e molto da ricordare: 66 collaudi verdi,
tutti falsificati, non potevano vederlo, perché la risposta finta dell'elenco modelli
dichiarava l'alias — la mia idea di come si chiamasse il modello. È bastato aprire le
Impostazioni e leggere le due tendine.

