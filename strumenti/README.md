# Strumenti

*Gli attrezzi di sviluppo di AI-CV-COACH. Stanno **fuori dal prodotto** — non entrano
nell'eseguibile e non si distribuiscono — ma senza di loro certe cose non si vedrebbero
mai: com'è fatta l'applicazione mentre gira, e come la si mette in mano a qualcuno.*

| Attrezzo | A cosa serve |
|---|---|
| [`mcp-collaudi/`](mcp-collaudi/README.md) | Il server MCP con cui l'assistente prova l'applicazione vera: la compila, fa girare il banco, la avvia, la fotografa, le preme i bottoni e — dal 2026-08-18 — **aspetta** che una condizione si avveri invece di guardare a intervalli. **Il suo README si legge prima di usarlo**: sono ore risparmiate. |
| `aggiorna-riferimento.bat` | Rifà l'**eseguibile di riferimento sul Desktop**: un file solo, autonomo, dall'ultimo codice di questo albero di lavoro. È quello che si apre col doppio clic *ed* è quello su cui l'assistente prova. |
| `avvia-demo.bat` | Apre TrovaLavoro con un doppio clic **passandole la chiave API**, per mostrarla a qualcuno senza passare da Claude Code. |
| `sigilla-pool/` | Il **rito del bump** da riga di comando (cap. 04.5): rigenera le impronte del pool dei prompt e riscrive il manifest. |
| [`collauda-copioni/`](collauda-copioni/README.md) | Il banco dei **copioni JavaScript** di `LettorePagina` — l'unico codice del prodotto che il banco VB non raggiunge, perché gira dentro la WebView. Li estrae dal sorgente, li compila e li prova su pagine finte. |

## Perché esiste `sigilla-pool/`

Ogni modifica a un prompt si chiude col bump (cap. 04.5): versione nuova, impronte
rigenerate, changelog annotato. Il comando che rigenera le impronte era dichiarato dentro
l'applicazione — Impostazioni → «Sigilla pool» — ma le **Impostazioni erano di T9**, e i
prompt si toccano da T2. Fino ad allora il rito non aveva un attrezzo, e ogni bump era
una cosa da rifare a mano.

*E le Impostazioni, arrivate a **T9b** (2026-08-21), quel comando non l'hanno preso — non
per fretta, ma perché lì non ci sta: il manifest vive nel repo accanto ai prompt, e un
eseguibile distribuito sigillerebbe qualcosa che nessuno rileggerà mai (cap. 04.5). In P8
è andata la sola **diagnosi** — la versione con l'asterisco e l'elenco dei file fuori
impronta. Questo attrezzo non è più un anticipo: è la casa definitiva del sigillo.*

L'attrezzo **non ricalcola niente per conto suo**: referenzia il progetto del prodotto e
chiama `LibreriaPrompt.Sigilla`, cioè lo stesso codice con cui il caricatore verifica le
impronte. È il punto: se il sigillo e il caricatore le calcolassero ognuno a modo suo,
prima o poi divergerebbero, e il pool risulterebbe modificato senza esserlo — un allarme
falso che si insegue per ore. Dopo aver scritto il manifest lo **rilegge** aprendo il
pool: un sigillo mai riletto è una promessa non verificata.

```bash
cd strumenti/sigilla-pool
"/mnt/c/Program Files/dotnet/dotnet.exe" build SigillaPool.vbproj -c Release
bin/Release/net10.0-windows/SigillaPool.exe "$(wslpath -w ../../VB.NET/src/TrovaLavoro/prompt-pool)" 1.04
```

Il percorso del pool va passato **alla maniera di Windows** (`wslpath -w`): è un exe
Windows, e un `/mnt/c/…` lo prenderebbe per un percorso relativo inesistente. La
compilazione emette un `MSB3277` su `WindowsBase` — due versioni, una dal runtime e una
da WebView2 — che riguarda solo questo attrezzo: l'eseguibile del prodotto compila senza
avvisi. *(Nato il 2026-08-14, col bump a Pool 1.04 di T6.)*

## Perché esiste `aggiorna-riferimento.bat`

*Nato il **2026-08-30**, perché Mirco l'ha chiesto con una frase che è anche la sua
specifica: «un eseguibile dell'app sul desktop che sarà sempre la versione più aggiornata,
e userai sempre quella quando fai i test, così che quando siamo offline so che è quella di
riferimento».*

Il problema che risolve non è la comodità del doppio clic: è l'**ambiguità**. Prima c'erano
due file che si chiamavano TrovaLavoro.exe — quello di `bin/Release`, che provava
l'assistente, e qualunque cosa Mirco aprisse per conto suo — e niente diceva se fossero la
stessa versione. Adesso ce n'è **uno**, sul Desktop, e lo usano tutti e due: lo strumento di
collaudo ci punta, `avvia-demo.bat` pure.

```
strumenti\aggiorna-riferimento.bat        (doppio clic, oppure l'attrezzo «compila»)
```

Sono gli stessi parametri del rilascio (cap. 13.2) — un file solo, il runtime .NET dentro,
niente DLL a fianco: **113 MiB in circa sei secondi**. Alla fine stampa il riquadro
dell'identità (versione, commit, dimensione, SHA-256), che è il modo di sapere *quale* file
si ha davanti quando il numero di versione non basta.

Tre accorgimenti, ciascuno pagato:

- **La compilazione intermedia va in `%TEMP%`, non in `bin\Release`.** Quel file lo tiene
  bloccato il server MCP del prodotto, e senza questo il comando fallirebbe con `MSB3027`
  per un motivo che col codice non c'entra niente.
- **Si chiude solo l'applicazione che gira dal Desktop**, riconosciuta dal percorso: mai un
  `taskkill /IM TrovaLavoro.exe`, che quel nome ce l'ha anche il server MCP.
- **`git -C "%~dp0"` non funziona**, e non lo dice: `%~dp0` finisce con una barra rovescia,
  e a git arriva un percorso con la virgoletta dentro. Il commit resta vuoto e l'eseguibile
  esce dichiarandosi «compilazione di sviluppo» — cioè con l'identità sbagliata, in silenzio.
  Si scrive **`git -C "%~dp0."`**, col punto. Lo stesso difetto stava in
  `VB.NET/src/publish.bat` fin dal giorno in cui il timbro del commit è nato (2026-08-27):
  **corretto il 2026-08-30**, e con lui si è rimessa in funzione anche la seconda guardia,
  l'avviso «ci sono modifiche non committate», che era muta per la stessa ragione. Nessun
  rilascio ne è uscito storpiato — l'unico eseguibile in `pubblicazione/` è del 24 agosto,
  cioè di tre giorni prima che il timbro esistesse.

*Una prova che vale la pena conoscere*: le due ricette producono un eseguibile **identico
bit per bit**. Il 2026-08-30, dallo stesso albero di lavoro, `aggiorna-riferimento.bat` e
`publish.bat` hanno dato lo stesso SHA-256 (`99d178e2…`). È il modo di sapere che il
riferimento su cui si prova non è «quasi» il rilascio: è lo stesso file, in un'altra
cartella.

## Perché esiste `avvia-demo.bat`

Fino a T6 l'applicazione prendeva la chiave API **solo** dalla variabile d'ambiente
`ANTHROPIC_API_KEY`. Con un doppio clic sull'eseguibile quella variabile non c'è, e
**tutto ciò che passa dall'AI si ferma**: l'analisi di un annuncio, il confronto, la
generazione. L'applicazione si apre e sembra viva — il guasto si scopre solo al primo
comando che chiama l'AI, cioè nel momento peggiore, davanti a chi guarda.

*Dal 2026-08-14 (T6) la chiave ha una casa vera*: cifrata in `segreti.bin` dentro la
cartella dati, chiesta in una finestra al primo avvio (cap. 11.3). Su un PC dove è già
stata salvata, questo lanciatore non serve più — l'applicazione la trova da sé, e anzi il
file **viene prima** della variabile. Resta comodo qui: la postazione di sviluppo lavora
di continuo su cartelle dati usa-e-getta (`--dati`), dove nessuna chiave è mai stata
salvata.

Il lanciatore legge la chiave dal `.env` del prototipo e la tiene in vita **solo per quell'avvio**:
non la copia da nessuna parte e non la stampa mai a schermo. Avvia l'**eseguibile di
riferimento sul Desktop** *(dal 2026-08-30; prima era la build di `bin/Release`)*, e se non
lo trova lo dice — rimandando ad `aggiorna-riferimento.bat` — invece di aprire il vuoto.

*(Nato il 2026-08-12, il giorno in cui l'applicazione è stata mostrata per la prima volta
a qualcuno di fuori dal progetto.)*

## Perché esiste `collauda-copioni/`

Due pezzi del prodotto non girano nell'applicazione ma **dentro la WebView**: il copione
che legge la pagina e quello che la scorre di un passo. Il banco VB non li vede — e
nemmeno un compilatore, perché sono stringhe concatenate: un errore di sintassi si
scopriva a runtime, con la cattura che torna vuota e il sospetto che cade sulla pagina
sbagliata.

Il difetto che ha fatto nascere l'attrezzo era di quelli che si notano per caso: la fine
di un blocco attaccata all'inizio del successivo — «Pubblica AmministrazioneDue suite
specializzate» — vista su un sito vero mentre si guardava altro. Su quel sito il modello
aveva capito lo stesso, ed è proprio il motivo per cui una cosa così può restare in giro
per mesi.

```bash
node strumenti/collauda-copioni/collauda-copioni.mjs
```

Non è un browser e non pretende di esserlo: i limiti di quel che può dire stanno scritti
nel [suo README](collauda-copioni/README.md), insieme alla ragione per cui, se il sorgente
cambia forma, l'attrezzo **si ferma** invece di continuare a dire «tutto bene».
*(Nato a T9d, 2026-08-22.)*

## Perché esiste `apri-app.bat`

Aprire il riferimento del Desktop sembra la cosa più semplice del mondo, e da WSL non lo
è: il 2026-09-02 l'assistente ha creduto per cinque minuti di aver aperto l'applicazione
mentre non era partito niente, e più tardi ha annunciato che non era partita mentre stava
aperta lì davanti. Due bugie opposte, e nessuna delle due si vede da un codice d'uscita.

```bash
cmd.exe /c "C:\Users\Mirco Parenti\Desktop\FIRST PROJECT\GITHub repository\AI-CV-COACH\strumenti\apri-app.bat"
```

Il lanciatore non compila niente — per rifare l'eseguibile c'è `aggiorna-riferimento.bat`
qui accanto — e non apre una seconda finestra se ce n'è già una: due finestre della stessa
applicazione si distinguono solo dalla barra del titolo, e un giro di collaudo intero è già
finito una volta su quella sbagliata. Quel che fa in più di uno `start` è **andare a
guardare** se la finestra c'è davvero, dichiarare il pid, il titolo e quanti secondi ci ha
messo.

Le tre trappole che ha già pagato, perché non le paghi di nuovo qualcun altro:

1. **`start` non dice se ha aperto qualcosa.** Invocato da WSL come
   `cmd.exe /c start "" "…\TrovaLavoro.exe"`, il percorso con gli spazi si perde fra le due
   shell: a volte risponde «Accesso negato», a volte — ed è il caso peggiore — **esce a zero
   senza aprire niente**. Il rimedio non è un quoting più furbo, è non credere all'esito.
2. **Di `TrovaLavoro.exe` ce n'è più d'uno.** Il server MCP del prodotto gira come
   `TrovaLavoro.exe --mcp` e non ha nessuna finestra: chi cerca «il primo processo che si
   chiama così» trova quasi sempre lui e conclude che l'applicazione non è partita. Il
   filtro giusto è `MainWindowHandle`, non il nome.
3. **Dentro le virgolette di `-Command`, il `^` non è un escape.** Un `^|` scritto lì non
   viene consumato da cmd: arriva a PowerShell tale e quale e gli rompe il comando in faccia
   (*«Impossibile trovare un parametro posizionale che accetta l'argomento '^'»*). Con un
   `2>nul` in coda a nascondere l'errore, il controllo rispondeva sempre «nessuna finestra»
   — che è anche la risposta giusta quasi sempre, ed è per questo che una cecità del genere
   campa. Fuori dalle virgolette il `^` invece serve davvero: il `2^>nul` degli altri
   attrezzi sta bene dov'è. La stessa cosa si dice senza pipe con `.Where({…})`.

Una nota per chi lo chiama **da WSL**: `cmd.exe /c apri-app.bat` **non torna** finché
l'applicazione resta aperta, perché l'interoperabilità aspetta anche i discendenti — e un
discendente questo lanciatore lo lascia per mestiere. Va lanciato in background, leggendone
l'uscita da un file; l'attesa vera, misurata, è di **due secondi**.

*(Nato il 2026-09-02, curato lo stesso giorno da tre difetti suoi.)*
