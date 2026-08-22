# I casi dei collaudi con l'API vera

Questa cartella è nata con la batteria di tappa T2 (cap. 14) — gli stessi input dati al
prototipo e alla nuova app, ciò che il prototipo ne ha fatto, e ciò che ne è uscito dal
confronto fra i due — e da T3 ospita anche gli **esiti dei collaudi reali** che si
possono pubblicare.

**I dati sono inventati.** Il repo è pubblico e un collaudo non vale di più perché
contiene dati veri: il candidato «Luca Ferrari» non esiste, e le aziende nemmeno.
Sono però scritti come li scriverebbe l'app — stessi campi degli schemi del pool — e
portano accenti e apostrofi di proposito, perché è lì che una differenza di codifica
si vede.

**È anche il criterio per decidere dove va un rapporto**: quello che nasce da dati
inventati sta qui, nel repo; quello che nasce dal **CV vero di Mirco** (le gambe A e B
del collaudo di tappa di T3) si scrive accanto al CV, fuori dal repo, e qui non ne
resta traccia.

| File | Cos'è |
|---|---|
| `profilo.json` | Il candidato: magazziniere, patente B, patentino per il muletto. |
| `annuncio_compatibile.json` | Un annuncio in linea col profilo: nessun requisito eliminatorio. |
| `annuncio_eliminatorio.json` | «Patente C indispensabile»: il candidato ha solo la B, e il match deve craterare (⛔). |
| `giudizi_*.json` | I giudizi dell'anello 3 per quel caso — la lista che il confronto produce e che la **mitigazione** riceve. Li ha prodotti il prototipo nel collaudo reale di T2: sono un artefatto a sé perché la parità della mitigazione resti verificabile senza rete. |
| `atteso/prompt_confronto_*.txt` | Il prompt che **il prototipo** costruisce per quel caso: è il termine di paragone della parità. |
| `atteso/prompt_mitigazione_*.txt` | Lo stesso, per il secondo prompt del confrontatore (aggiunto a T4). |
| `genera_attesi.mjs` | Rigenera i quattro file qui sopra facendoli produrre al prototipo. |
| `reale/confronto_*.json` | L'esito del collaudo reale di T2: risposta del prototipo, risposta dell'app, e il ricalcolo. |
| `reale/dialogo_guidato.md` | Il collaudo di tappa di T3, gamba C: il dialogo guidato condotto per intero con l'AI vera sulla traccia di **Anna Ricci**, che non esiste. Si legge come una conversazione, e dice dove è finito ogni frammento detto nel turno sbagliato. |
| `reale/checklist_*.md` | Il collaudo di tappa di T9: la checklist «Problemi e mitigazioni» del prototipo ripercorsa con l'AI vera. Tre rapporti — il candidato che si vende (voci 1, 2, 3, 5), l'annuncio scarno (voce 7), il confronto con le lacune (voce 8) — e ognuno dice in testa quali voci copre. Le persone e le aziende sono inventate. |
| `reale/dialogo_turno_formali.json` | Lo stesso turno «esperienze formali» chiesto all'app e al prototipo con la **stessa identica risposta**: la prova di parità della gamba C. |

## Le due batterie

**Senza rete, sempre** — `CollaudiParitaPrompt` verifica che il prompt costruito dal
pool sia carattere per carattere quello del prototipo. Gira con la batteria normale
(`dotnet test` da `VB.NET/src`) e resterà verde anche quando il prototipo non sarà
più avviabile.

**Con l'API vera** — la categoria `Reale`, fuori dalla batteria di tutti i giorni: si
lancia da `VB.NET/src` passando a `dotnet test` l'opzione `settings` con il file
`TrovaLavoro.Collaudi/collaudi-reali.runsettings`. Ognuno ha i suoi prerequisiti, e
quando ne manca uno **si dichiara inconcludente invece di fallire**: sull'altra
postazione non c'è né la chiave né il CV.

| Classe | Cosa chiede | Chiave | CV vero | Prototipo |
|---|---|---|---|---|
| `CollaudiConfrontoReale` | gli stessi due casi al prototipo (`POST /confronta`) e alla pipeline dell'app | sì | — | sì |
| `CollaudiImportReale` | lo stesso CV importato dalle due parti | sì | sì | sì |
| `CollaudiFormatiReale` | lo stesso CV dalle quattro porte (PDF, DOCX, TXT, MD) | sì | sì | — |
| `CollaudiDialogoReale` | il dialogo guidato da zero; e un turno solo chiesto anche al prototipo | sì | — | solo per il turno di parità |
| `CollaudiChecklistReale` | le tre prove della checklist «Problemi e mitigazioni» (cap. 14, T9) | sì | — | — |

Il **CV vero** si indica con la variabile `CV_DI_PROVA` (la cartella che lo contiene);
il **prototipo** si accende con `npm start` dentro `HTML+JS/`; la **chiave** sta in
`ANTHROPIC_API_KEY`. Da WSL le variabili arrivano all'eseguibile Windows solo se
elencate in `WSLENV` (`WSLENV=ANTHROPIC_API_KEY:CV_DI_PROVA/p`).

## Se un prompt del pool cambia

Il prompt atteso va rigenerato, altrimenti la parità fallisce su una differenza che
non è una regressione: prima il rito del bump del pool (cap. 04.5), poi
`node VB.NET/src/TrovaLavoro.Collaudi/casi/genera_attesi.mjs` dalla radice del repo.
Vale finché il prototipo resta il giudice: dal momento in cui i prompt del pool
prenderanno una strada loro, questi attesi diventeranno la fotografia del punto di
partenza, non più un vincolo.

Riguarda **`confronto` e `mitigazione`, e solo quelli**: sugli altri quattro prompt che
la pipeline usa il distacco è già avvenuto ed è voluto (cap. 04.7) — `analisi_annuncio`
estrae anche il nome dell'azienda dal Pool 1.03, e i tre della generazione seguiranno la
loro strada. Per quelli il prototipo resta un termine di paragone, non un metro.
