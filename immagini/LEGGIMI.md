# Le immagini del marchio

Qui stanno **solo** gli asset che il repository usa davvero. La lavorazione — le versioni
scartate, i formati per i social, l'editor con cui si trascina il lettering — vive **fuori
dal repo**, nella cartella di lavoro `LOGO PROJECT`: sono decine di megabyte di file che
cambiano interi a ogni salvataggio, e un PNG in git non si aggiorna, si riscrive da capo.
La **ricetta** per rigenerare tutto — i due prompt, le righe di ricambio, le parole da non
usare mai — è in [`../VB.NET/progetto/prompt-logo.md`](../VB.NET/progetto/prompt-logo.md).

| File | Misura | A cosa serve |
|---|---|---|
| `MASTER-solo-disegno-1536x1024.png` | 1536×1024 | Il disegno nudo, senza nessuna scritta. È la **sorgente**: ogni altro formato nasce da qui, ritagliando e aggiungendo il lettering. Non si sovrascrive con un formato derivato — cambia **solo** quando cambia il marchio, e allora cambiano con lei tutti i formati (l'ultima volta il 2026-08-30: la girella a spicchi, il filetto giallo tre volte più spesso e la ripulitura dei residui scuri rimasti attorno all'istogramma). |
| `TrovaLavoro-readme-1200x1052.png` | 1200×1052 | La testata di questo `README.md`. |

**Come sono fatti.** Fondo, cornice e fasce laterali sono un unico **blu Aviolab `#0B06B0`**
— lo stesso di `StileApp.Accento` — col filetto giallo `#E2E44E`; lo sfondo interno del
disegno è a spicchi **rosso `#FA0825`** e **argento `#C0CFCB`**, i quattro colori dello
stemma e nessun altro. Nella fascia del testo il nome è in Segoe UI Black bianco con un
**contorno nero** spesso, che a quello spessore salda fra loro le lettere e fa della scritta
un blocco unico; alla sua destra sta lo **stemma Aviolab come timbro**, col bordino argento;
sotto, il sottotitolo in Segoe UI semi-grassetto **bianco pieno**. *(Dal 2026-08-30 lo sfondo
interno non è più a spire concentriche ma a **spicchi**, con le diagonali dello scudetto
Aviolab: il fondo del banner era `#000C38`, un blu che alla tavolozza non apparteneva. Dello
stesso giorno è il filetto **tre volte più spesso** — una ventina di pixel sulla sorgente
invece di sette —, cresciuto solo **verso l'interno**: il bordo esterno del disegno non si è
mosso, e dove le maniche delle giacche tagliano la cornice il filetto resta interrotto
com'era. Della stessa giornata, in coda, sono il timbro e il contorno del nome: la fascia del
testo si è alzata per far posto al timbro, e con lei sono cresciute le misure di tutti i
formati — la testata passa da 1200×972 a 1200×1052, la schermata di avvio da 800×648 a
800×702. Il sottotitolo, che era `#DDE5F7`, è diventato bianco pieno.)*

**Lo stesso disegno vive anche dentro l'applicazione**: la schermata di avvio
(`../VB.NET/src/TrovaLavoro/Risorse/schermata-avvio.png`, 800×702) nasce da questa sorgente
con gli stessi valori di composizione, e cambia insieme a lei — è una risorsa compilata
nell'eseguibile, quindi vuole una build.

**Il nome sull'immagine è «TrovaLavoro»**, che è il nome del **prodotto**; `AI-CV-COACH`
resta il nome del progetto e del repository (cap. 13.5, cap. 15 voce 3).
