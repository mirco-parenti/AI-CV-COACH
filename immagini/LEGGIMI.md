# Le immagini del marchio

Qui stanno **solo** gli asset che il repository usa davvero. La lavorazione — le versioni
scartate, i formati per i social, l'editor con cui si trascina il lettering — vive **fuori
dal repo**, nella cartella di lavoro `LOGO PROJECT`: sono decine di megabyte di file che
cambiano interi a ogni salvataggio, e un PNG in git non si aggiorna, si riscrive da capo.
La **ricetta** per rigenerare tutto — i due prompt, le righe di ricambio, le parole da non
usare mai — è in [`../VB.NET/progetto/prompt-logo.md`](../VB.NET/progetto/prompt-logo.md).

| File | Misura | A cosa serve |
|---|---|---|
| `MASTER-solo-disegno-1536x1024.png` | 1536×1024 | Il disegno nudo, senza nessuna scritta. È la **sorgente**: ogni altro formato nasce da qui, ritagliando e aggiungendo il lettering. Non si sovrascrive. |
| `TrovaLavoro-readme-1200x972.png` | 1200×972 | La testata di questo `README.md`. |

**Come sono fatti.** Fondo `#000C38`; nome in Segoe UI Black bianco, sottotitolo in Segoe UI
semi-grassetto `#DDE5F7`. Nei formati panoramici i lati si riempiono prolungando le colonne
di bordo del disegno, così il braccio si allunga invece di troncarsi.

**Il nome sull'immagine è «TrovaLavoro»**, che è il nome del **prodotto**; `AI-CV-COACH`
resta il nome del progetto e del repository (cap. 13.5, cap. 15 voce 3).
