# Strumenti

*Gli attrezzi di sviluppo di AI-CV-COACH. Stanno **fuori dal prodotto** — non entrano
nell'eseguibile e non si distribuiscono — ma senza di loro certe cose non si vedrebbero
mai: com'è fatta l'applicazione mentre gira, e come la si mette in mano a qualcuno.*

| Attrezzo | A cosa serve |
|---|---|
| [`mcp-collaudi/`](mcp-collaudi/README.md) | Il server MCP con cui l'assistente prova l'applicazione vera: la compila, fa girare il banco, la avvia, la fotografa e le preme i bottoni. **Il suo README si legge prima di usarlo**: sono ore risparmiate. |
| `avvia-demo.bat` | Apre TrovaLavoro con un doppio clic, per mostrarla a qualcuno senza passare da Claude Code. |

## Perché esiste `avvia-demo.bat`

L'applicazione prende la chiave API dalla variabile d'ambiente `ANTHROPIC_API_KEY` (la
cifratura su disco è di T6). Con un doppio clic sull'eseguibile quella variabile non c'è,
e **tutto ciò che passa dall'AI si ferma**: l'analisi di un annuncio, il confronto, la
generazione. L'applicazione si apre e sembra viva — il guasto si scopre solo al primo
comando che chiama l'AI, cioè nel momento peggiore, davanti a chi guarda.

Il lanciatore legge la chiave dal `.env` del prototipo e la tiene in vita **solo per quell'avvio**:
non la copia da nessuna parte e non la stampa mai a schermo. Avvia la build di
`bin/Release` — quella di `dotnet build -c Release` — e se non la trova lo dice, invece
di aprire il vuoto.

*(Nato il 2026-08-12, il giorno in cui l'applicazione è stata mostrata per la prima volta
a qualcuno di fuori dal progetto.)*
