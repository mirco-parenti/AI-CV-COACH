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
un blocco unico, e sotto il sottotitolo in Segoe UI semi-grassetto **bianco pieno**:
**centrati tutt'e due sull'asse dell'immagine**, e nient'altro accanto. *(Dal 2026-09-02 lo
**stemma Aviolab non c'è più** nella fascia: stava alla destra del nome come timbro, e il
nome gli faceva posto stando a sinistra. Tolto lui, il nome si è messo in mezzo. Le misure
di tutti i formati sono rimaste **le stesse** — l'altezza è imposta e la fascia è quel che
avanza sopra il disegno, quindi cambia solo come la si riempie — e il disegno sotto la
fascia è **identico bit a bit** a prima.)* *(Dal 2026-08-30 lo sfondo
interno non è più a spire concentriche ma a **spicchi**, con le diagonali dello scudetto
Aviolab: il fondo del banner era `#000C38`, un blu che alla tavolozza non apparteneva. Dello
stesso giorno è il filetto **tre volte più spesso** — una ventina di pixel sulla sorgente
invece di sette —, cresciuto solo **verso l'interno**: il bordo esterno del disegno non si è
mosso, e dove le maniche delle giacche tagliano la cornice il filetto resta interrotto
com'era. Della stessa giornata, in coda, sono il timbro e il contorno del nome: la fascia del
testo si è alzata per far posto al timbro, e con lei sono cresciute le misure di tutti i
formati — la testata passa da 1200×972 a 1200×1052, la schermata di avvio da 800×648 a
800×702. Il sottotitolo, che era `#DDE5F7`, è diventato bianco pieno.)*

*(Dal 2026-09-08 il disegno è **ripulito**: se n'erano andati via i residui del vecchio
sfondo rosso, che restavano attaccati ai contorni delle figure — undici righe larghe 4 px
dentro le sagome, un migliaio di schegge del colore di fondo finite nello spicchio
sbagliato e una macchia sotto la mano che regge la chiave inglese. In tutto poco più di
2.400 pixel sul master, lo 0,16%. Il disegno **non è stato ridisegnato**: i pixel sbagliati
hanno preso il colore di quel che li circondava, le misure e il testo non sono cambiati di
un pixel. La ricetta sta in `LOGO PROJECT`, negli strumenti `ripulisci-residui.py` e
`ripulisci-macchia.py`.)*

**Il copyright viaggia dentro i file.** Tutti i PNG del marchio — i due di qui e le due
risorse dell'applicazione — si portano nei **dati nascosti** la riga
`Copyright 2026 by Mirco Parenti - Aviolab AI`, scritta in due forme perché i programmi non
leggono tutti la stessa cosa: il chunk PNG `tEXt` «Copyright», che è quello che mostra
Windows in *Proprietà → Dettagli*, e un pacchetto XMP `dc:rights` per Adobe e i programmi di
fotoritocco. Non cambia **un pixel**: i chunk si infilano nel file senza ricomprimere
l'immagine, e infatti questi quattro restano identici agli originali in `LOGO PROJECT`. La
firma nasce di là — la scrivono da soli lo script di composizione e `timbra-copyright.py` —
e qui arriva già dentro i file che si copiano. L'icona `TrovaLavoro.ico` è l'unica **senza**:
il formato ICO non ha un posto dove metterla.

**Lo stesso disegno vive anche dentro l'applicazione**, in due risorse compilate
nell'eseguibile — quindi cambiarle vuole una build. La schermata di avvio
(`../VB.NET/src/TrovaLavoro/Risorse/schermata-avvio.png`, 800×702) nasce da questa sorgente
con gli stessi valori di composizione e cambia insieme a lei; accanto c'è il banner intero
(`../VB.NET/src/TrovaLavoro/Risorse/sfondo-menu.png`, 1536×1348), che dal 2026-08-30 (sera)
**non ha più nessun lettore nel prodotto** — il menu il suo fondo lo dipinge — e resta
incorporato perché un giorno lo si possa riusare: si tiene allineato al marchio come gli
altri, e un collaudo dice almeno che è integro.

**Il nome sull'immagine è «TrovaLavoro»**, che è il nome del **prodotto**; `AI-CV-COACH`
resta il nome del progetto e del repository (cap. 13.5, cap. 15 voce 3).
