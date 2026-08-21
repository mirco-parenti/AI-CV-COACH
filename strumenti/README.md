# Strumenti

*Gli attrezzi di sviluppo di AI-CV-COACH. Stanno **fuori dal prodotto** — non entrano
nell'eseguibile e non si distribuiscono — ma senza di loro certe cose non si vedrebbero
mai: com'è fatta l'applicazione mentre gira, e come la si mette in mano a qualcuno.*

| Attrezzo | A cosa serve |
|---|---|
| [`mcp-collaudi/`](mcp-collaudi/README.md) | Il server MCP con cui l'assistente prova l'applicazione vera: la compila, fa girare il banco, la avvia, la fotografa, le preme i bottoni e — dal 2026-08-18 — **aspetta** che una condizione si avveri invece di guardare a intervalli. **Il suo README si legge prima di usarlo**: sono ore risparmiate. |
| `avvia-demo.bat` | Apre TrovaLavoro con un doppio clic, per mostrarla a qualcuno senza passare da Claude Code. |
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
non la copia da nessuna parte e non la stampa mai a schermo. Avvia la build di
`bin/Release` — quella di `dotnet build -c Release` — e se non la trova lo dice, invece
di aprire il vuoto.

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
