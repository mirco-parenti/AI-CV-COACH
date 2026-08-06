# I casi della non-regressione contro il prototipo

Questa cartella contiene i dati della batteria di tappa T2 (cap. 14): gli stessi
input dati al prototipo e alla nuova app, ciò che il prototipo ne ha fatto, e ciò
che ne è uscito dal confronto fra i due.

**I dati sono inventati.** Il repo è pubblico e un collaudo non vale di più perché
contiene dati veri: il candidato «Luca Ferrari» non esiste, e le aziende nemmeno.
Sono però scritti come li scriverebbe l'app — stessi campi degli schemi del pool — e
portano accenti e apostrofi di proposito, perché è lì che una differenza di codifica
si vede.

| File | Cos'è |
|---|---|
| `profilo.json` | Il candidato: magazziniere, patente B, patentino per il muletto. |
| `annuncio_compatibile.json` | Un annuncio in linea col profilo: nessun requisito eliminatorio. |
| `annuncio_eliminatorio.json` | «Patente C indispensabile»: il candidato ha solo la B, e il match deve craterare (⛔). |
| `atteso/prompt_confronto_*.txt` | Il prompt che **il prototipo** costruisce per quel caso: è il termine di paragone della parità. |
| `genera_attesi.mjs` | Rigenera i due file qui sopra facendoli produrre al prototipo. |
| `reale/confronto_*.json` | L'esito del collaudo reale: risposta del prototipo, risposta dell'app, e il ricalcolo. |

## Le due batterie

**Senza rete, sempre** — `CollaudiParitaPrompt` verifica che il prompt costruito dal
pool sia carattere per carattere quello del prototipo. Gira con la batteria normale
(`dotnet test` da `VB.NET/src`) e resterà verde anche quando il prototipo non sarà
più avviabile.

**Con l'API vera** — `CollaudiConfrontoReale` manda gli stessi casi al prototipo
(`POST /confronta`) e alla pipeline dell'app, poi confronta. Vuole la chiave e il
prototipo acceso, quindi gira **solo su aviolab03** e resta fuori dalla batteria di
tutti i giorni: si lancia da `VB.NET/src` passando a `dotnet test` l'opzione
`settings` con il file `TrovaLavoro.Collaudi/collaudi-reali.runsettings`, dopo aver
avviato il prototipo (`npm start` dentro `HTML+JS/`) e definito `ANTHROPIC_API_KEY`
nell'ambiente.

## Se un prompt del pool cambia

Il prompt atteso va rigenerato, altrimenti la parità fallisce su una differenza che
non è una regressione: prima il rito del bump del pool (cap. 04.5), poi
`node VB.NET/src/TrovaLavoro.Collaudi/casi/genera_attesi.mjs` dalla radice del repo.
Vale finché il prototipo resta il giudice: dal momento in cui i prompt del pool
prenderanno una strada loro, questi attesi diventeranno la fotografia del punto di
partenza, non più un vincolo.
