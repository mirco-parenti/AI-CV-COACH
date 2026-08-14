# Strumenti

*Gli attrezzi di sviluppo di AI-CV-COACH. Stanno **fuori dal prodotto** — non entrano
nell'eseguibile e non si distribuiscono — ma senza di loro certe cose non si vedrebbero
mai: com'è fatta l'applicazione mentre gira, e come la si mette in mano a qualcuno.*

| Attrezzo | A cosa serve |
|---|---|
| [`mcp-collaudi/`](mcp-collaudi/README.md) | Il server MCP con cui l'assistente prova l'applicazione vera: la compila, fa girare il banco, la avvia, la fotografa e le preme i bottoni. **Il suo README si legge prima di usarlo**: sono ore risparmiate. |
| `avvia-demo.bat` | Apre TrovaLavoro con un doppio clic, per mostrarla a qualcuno senza passare da Claude Code. |
| `sigilla-pool/` | Il **rito del bump** da riga di comando (cap. 04.5): rigenera le impronte del pool dei prompt e riscrive il manifest. |

## Perché esiste `sigilla-pool/`

Ogni modifica a un prompt si chiude col bump (cap. 04.5): versione nuova, impronte
rigenerate, changelog annotato. Il comando che rigenera le impronte è dichiarato dentro
l'applicazione — Impostazioni → «Sigilla pool» — ma le **Impostazioni sono di T9**, e i
prompt si toccano da T2. Fino ad allora il rito non aveva un attrezzo, e ogni bump era
una cosa da rifare a mano.

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

L'applicazione prende la chiave API dalla variabile d'ambiente `ANTHROPIC_API_KEY` (la
chiave cifrata nella cartella dati arriva con la 1.0 — capp. 02.5 e 11.3). Con un doppio
clic sull'eseguibile quella variabile non c'è,
e **tutto ciò che passa dall'AI si ferma**: l'analisi di un annuncio, il confronto, la
generazione. L'applicazione si apre e sembra viva — il guasto si scopre solo al primo
comando che chiama l'AI, cioè nel momento peggiore, davanti a chi guarda.

Il lanciatore legge la chiave dal `.env` del prototipo e la tiene in vita **solo per quell'avvio**:
non la copia da nessuna parte e non la stampa mai a schermo. Avvia la build di
`bin/Release` — quella di `dotnet build -c Release` — e se non la trova lo dice, invece
di aprire il vuoto.

*(Nato il 2026-08-12, il giorno in cui l'applicazione è stata mostrata per la prima volta
a qualcuno di fuori dal progetto.)*
