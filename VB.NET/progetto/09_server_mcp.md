# 09 — Il server MCP integrato

*Le funzioni dell'applicazione esposte, in modo controllato, ai client AI esterni
(Claude Desktop, Claude Code e qualunque altro client MCP): stesso motore, stessi
prompt, stessi dati — un'interfaccia in più.*

## 9.1 Cos'è, in due parole

MCP (Model Context Protocol) è un protocollo aperto con cui un'applicazione può
mettere a disposizione di un assistente AI dei **tool**: funzioni con nome, descrizione
e parametri, che l'assistente può chiamare. In pratica: con il server MCP attivo,
l'utente può stare in Claude Desktop e dire «confronta il mio profilo con questo
annuncio e dimmi se vale la pena candidarmi» — e Claude userà **la nostra pipeline**
(prompt del pool, punteggio deterministico, hard-gate) invece di improvvisare.

> **Da non confondere con il server MCP di collaudo** *(nato a T4c, 2026-08-10)*. In
> `strumenti/mcp-collaudi/` vive un secondo server MCP che **non è parte del prodotto**:
> non entra nell'eseguibile, non si distribuisce e non tocca la pipeline. Quello espone
> gli attrezzi per **provare** l'applicazione — compilare, far girare il banco, avviarla,
> fotografarla, premerle i bottoni, sceglierle una voce da una tendina o una riga da un
> elenco, risponderle quando chiede un file o una conferma. Questo capitolo
> parla invece dei tool che l'applicazione **offre** ai client AI dell'utente (tappa T8).
> Uno serve a chi costruisce il programma, l'altro a chi lo usa.

## 9.2 Come si avvia

- **Stesso exe, modalità dedicata**: `TrovaLavoro.exe --mcp`. Nessun secondo programma:
  il vincolo «un solo file» resta intatto.
- In questa modalità l'app non apre finestre: parla con il client via **stdio**
  (standard input/output), il trasporto MCP più semplice e senza rete. È il client
  (es. Claude Desktop) ad avviare il processo, secondo la sua configurazione:

```json
{
  "mcpServers": {
    "trovalavoro": {
      "command": "C:\\TrovaLavoro\\TrovaLavoro.exe",
      "args": ["--mcp"]
    }
  }
}
```

- Il percorso in `command` è semplicemente quello dove sta l'exe (che può essere una
  cartella qualsiasi — cap. 13.4).
- Il protocollo è JSON-RPC 2.0 con i tre passi canonici (`initialize`,
  `tools/list`, `tools/call`): il programma li implementa direttamente, senza
  librerie aggiuntive — è scambio di messaggi JSON su stdio, alla portata del motore
  che già maneggia JSON tutto il giorno.

## 9.3 I tool esposti

Prima versione — tutti i tool leggono/scrivono la **stessa cartella dati** dell'app:

| Tool | Cosa fa | Scrive dati? |
|---|---|---|
| `leggi_profilo` | restituisce il profilo JSON corrente | no |
| `leggi_registro` | elenco opportunità con stati, stelle, date | no |
| `leggi_opportunita` | tutti gli artefatti di una singola opportunità | no |
| `analizza_annuncio` | testo annuncio → Annuncio JSON (stesso prompt del pool) | no |
| `confronta` | profilo + annuncio → giudizi, stelle, note (con clamp e hard-gate) | no |
| `mitiga` | profilo + giudizi → mitigazioni oneste (anche lista vuota) | no |
| `struttura_cv` | testo di un CV (trascritto o incollato) → proposta di profilo JSON, senza salvarla | no |
| `genera_cv` | CV-1 o CV-2 in JSON, lingua a scelta, appunti di mira come parametro facoltativo | no |
| `genera_lettera` | lettera in JSON, lingua a scelta | no |
| `rifinisci_testo` | passata anti-slop su un testo di prosa | no |
| `esporta_documento` | CV/lettera JSON → file DOCX o PDF nella cartella dell'opportunità | sì (nuovi file) |
| `salva_opportunita` | inserisce un annuncio analizzato nella coda | sì |
| `esporta_backup` | scrive il backup JSON del profilo | sì (nuovo file) |

**Fuori dalla prima versione, di proposito**: la modifica del profilo via MCP, azione
irreversibile che resta nell'app dove c'è la conferma visiva dell'utente (livello 6 del
cap. 03). Potrà entrare in seguito, con un meccanismo di conferma esplicita.
Quanto all'**invio di email**, la questione non si pone più nemmeno nell'app: nella 1.0
il programma non spedisce, prepara un file `.eml` (cap. 07 e cap. 15, voce 9).

**Non sono tool, e il perché**: il **brainstorming** non serve come tool — il client
MCP è già un assistente conversazionale; gli bastano gli artefatti (`leggi_*`) e il
parametro «appunti di mira» dei tool di generazione. La **classificazione della
cartella documenti** è un'operazione interattiva locale (scelta file + conferma a
video) e resta nell'app. L'**export leggibile del registro** è coperto da
`leggi_registro`: la formattazione la fa il client come preferisce.

## 9.4 Convivenza con l'app aperta

Due processi che scrivono gli stessi file sono un problema classico. Regola semplice:

- la cartella dati ha un **lucchetto di scrittura** (file di lock): lo prende chi
  scrive per primo;
- se l'app con finestre è aperta, il server MCP funziona lo stesso ma i tool marcati
  «scrive dati» rispondono con un messaggio onesto («l'app è aperta: chiudi la
  finestra o usa i tool di sola lettura»);
- viceversa, l'app all'avvio segnala se un server MCP sta scrivendo.

Niente sincronizzazioni sofisticate: un solo scrittore alla volta, dichiarato.

## 9.5 Sicurezza

- **Solo locale**: il trasporto stdio non apre porte di rete; il server esiste solo
  come processo figlio del client sullo stesso PC. Un eventuale trasporto HTTP locale è
  rimandato (cap. 15) e comunque nascerebbe legato a `localhost`.
- **La chiave API resta dove sta**: cifrata nella configurazione dell'app (cap. 11).
  Il client MCP non la vede mai: chiama i tool, e sono i tool a usare la chiave.
- **Stessi limiti etici**: i tool passano dagli stessi prompt del pool — anti-invenzione
  e hard-gate valgono anche quando a chiamare è un'AI esterna. Il punteggio non si può
  «convincere»: è calcolato dal codice.

## 9.6 Perché ne vale la pena

- **Dimostrabilità**: per il portfolio, un'app che è anche server MCP mostra competenza
  su un protocollo che il mercato sta adottando adesso.
- **Automazioni personali**: l'utente evoluto può orchestrare valutazioni in serie
  («questi 5 annunci: quali passano le 3 stelle?») dal suo assistente preferito.
- **Architettura onesta**: se il motore è davvero separato dall'interfaccia (cap. 02),
  esporlo via MCP è un'aggiunta sottile, ed è anche la **prova** che la separazione
  regge.
