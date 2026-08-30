# Le immagini del marchio

Qui stanno **solo** gli asset che il repository usa davvero. La lavorazione — le versioni
scartate, i formati per i social, l'editor con cui si trascina il lettering — vive **fuori
dal repo**, nella cartella di lavoro `LOGO PROJECT`: sono decine di megabyte di file che
cambiano interi a ogni salvataggio, e un PNG in git non si aggiorna, si riscrive da capo.
La **ricetta** per rigenerare tutto — i due prompt, le righe di ricambio, le parole da non
usare mai — è in [`../VB.NET/progetto/prompt-logo.md`](../VB.NET/progetto/prompt-logo.md).

| File | Misura | A cosa serve |
|---|---|---|
| `MASTER-solo-disegno-1536x1024.png` | 1536×1024 | Il disegno nudo, senza nessuna scritta. È la **sorgente**: ogni altro formato nasce da qui, ritagliando e aggiungendo il lettering. Non si sovrascrive con un formato derivato — cambia **solo** quando cambia il marchio, e allora cambiano con lei tutti i formati (l'ultima volta il 2026-08-30, la girella a spicchi). |
| `TrovaLavoro-readme-1200x972.png` | 1200×972 | La testata di questo `README.md`. |

**Come sono fatti.** Fondo, cornice e fasce laterali sono un unico **blu Aviolab `#0B06B0`**
— lo stesso di `StileApp.Accento` — col filetto giallo `#E2E44E`; lo sfondo interno del
disegno è a spicchi **rosso `#FA0825`** e **argento `#C0CFCB`**, i quattro colori dello
stemma e nessun altro. Nome in Segoe UI Black bianco, sottotitolo in Segoe UI semi-grassetto
`#DDE5F7`. Nei formati panoramici i lati si riempiono prolungando le colonne di bordo del
disegno, così il braccio si allunga invece di troncarsi. *(Dal 2026-08-30 lo sfondo interno
non è più a spire concentriche ma a **spicchi**, con le diagonali dello scudetto Aviolab: il
fondo del banner era `#000C38`, un blu che alla tavolozza non apparteneva.)*

**Lo stesso disegno vive anche dentro l'applicazione**: la schermata di avvio
(`../VB.NET/src/TrovaLavoro/Risorse/schermata-avvio.png`, 800×648) nasce da questa sorgente
con gli stessi valori di composizione, e cambia insieme a lei — è una risorsa compilata
nell'eseguibile, quindi vuole una build.

**Il nome sull'immagine è «TrovaLavoro»**, che è il nome del **prodotto**; `AI-CV-COACH`
resta il nome del progetto e del repository (cap. 13.5, cap. 15 voce 3).
