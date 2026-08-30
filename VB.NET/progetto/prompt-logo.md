# TrovaLavoro — prompt per la generazione del logo (ChatGPT / GPT Image)

Schema di Mirco, 2026-08-22. Revisione 2: stile ribaltato su **illustrazione vettoriale
piatta e stilizzata** (la revisione 1 diceva "semi-realistic" e usciva realistica).
Decisioni: slash = prima/dopo della stessa persona · due prompt (illustrazione + icona) ·
flat vector con glow neon solo sulle frecce · palette dei codici del progetto
(#0B06B0 blu, #FA0825 rosso, #28A745 verde).

**Revisione 3, 2026-08-30: il prompt non cambia, cambia la lavorazione.** L'immagine che
questo prompt genera ha, dentro la cornice, un fondo blu piatto; da oggi quel fondo interno
è sostituito in post-produzione con una **girella a spicchi** rosso `#FA0825` e argento
`#C0CFCB`, tagliata sulle diagonali dello scudetto Aviolab e centrata sulla moneta. Il blu
resta dov'era — cornice, fasce laterali, fondo del lettering — ed è quello dello stemma
(`#0B06B0`), non più il blu notte `#000C38` che alla tavolozza non apparteneva.
Nella stessa giornata la lavorazione ha fatto un secondo passo: il **filetto giallo**
`#E2E44E` che corre dentro la cornice è stato portato a **tre volte** il suo spessore,
cresciuto solo verso l'interno — il bordo esterno del disegno non si sposta di un pixel, e
dove le maniche tagliano la cornice il filetto resta interrotto com'era. I numeri esatti e
il metodo stanno in [`../../immagini/LEGGIMI.md`](../../immagini/LEGGIMI.md), che è la
casa della lavorazione. Chi rigenera il disegno da questo prompt ottiene perciò il disegno
**prima** della lavorazione — di **tutta** la lavorazione, girella e filetto; la sorgente
già lavorata, da cui nascono tutti i formati, è
[`../../immagini/MASTER-solo-disegno-1536x1024.png`](../../immagini/MASTER-solo-disegno-1536x1024.png).

---

## PROMPT A — illustrazione orizzontale (key visual: splash, primo avvio, README, banner)

Flat vector illustration, bold graphic poster style, fully stylized. Aspect ratio 16:9.

STYLE RULES (these override everything else):
- Pure flat vector art: solid color fills, thick clean uniform outlines, simplified geometric shapes.
- No gradients, no shading, no drop shadows, no textures, no lighting effects — except the neon glow of the green arrows, which is the only luminous element in the whole image.
- People are stylized flat characters: simple geometric bodies, minimal facial features (dots and lines), no rendered skin, no anatomical detail.
- NOT a photograph, NOT a 3D render, NOT photorealistic, no realistic materials, no depth of field, no lens effects.

BACKGROUND: one solid flat deep blue rectangle, color #0B06B0. Nothing else.

COMPOSITION: three zones read left to right — left block, center block, right block — with balanced visual weight on the left and on the right.

LEFT BLOCK (the candidate):
- FOREGROUND: one large stylized hand, flat vector, side view, open and extended toward the right, fingers together and thumb up, in the position of offering a handshake. Exactly five fingers, drawn as simple rounded shapes. Solid fill, thick outline. Big and dominant, occupying the lower-left third.
- BEHIND IT, smaller: the same stylized man shown twice, before and after, separated by a bold red diagonal slash (flat #FA0825) cutting between them. Left of the slash, the BEFORE: slumped shoulders, plain shapeless clothes, downturned mouth, both palms open in a gesture of asking for help. Right of the slash, the AFTER: the same simplified face, now upright and confident, wearing a brown fedora hat, a flat brown suit, holding a brown briefcase. Both figures are flat vector characters in solid colors.

RIGHT BLOCK (the employer):
- FOREGROUND: a second large stylized hand, mirrored, extended toward the left, open for the same handshake. Exactly five fingers, same flat vector style and size as the left hand. The two hands face each other but DO NOT touch: a clear gap remains between them.
- BEHIND IT: a simple geometric factory with a tall chimney and flat curling smoke shapes, beside a simple rectangular office tower with a grid of windows; in front of them a flat contract document with a pen signing it and a round red stamp (an abstract circular mark, no readable letters).

CENTER BLOCK (the connector):
- Two green neon arrows. One arcs over the top, the other arcs under the bottom; together they trace an open oval ring around the central space. Their tips almost meet but stay separated by a small visible gap at each end: the loop is never closed.
- Each arrow grows out of the wrist of the hand nearest to it. Arrow and hand overlap and merge, as if the hand emitted the arrow to reach the other hand.
- The arrows carry a luminous neon green trail (#28A745 at neon brightness) with a few scattered glowing dots like spores. Soft even glow, gentle, never harsh. This is the ONLY glowing element in the image.

CENTER BACKGROUND OVERLAY (flat on the blue, behind everything, inside the empty oval only):
- A simple stopwatch outline, a euro currency symbol, and a rising bar chart with an upward arrow line, all drawn as thin flat line icons.
- These three icons are semi-transparent, tone-on-tone in a lighter flat blue, sitting on the blue background like a watermark. They stay strictly inside the central empty area framed by the arrows and never overlap the hands, the figures, the factory or the arrows.

COLORS: flat deep blue #0B06B0, flat red #FA0825, neon green #28A745, flat brown for hat/suit/briefcase, one flat skin tone.

STRICTLY NO TEXT: no letters, no words, no title, no signature, no watermark. Clean uncluttered composition.

---

## PROMPT B — icona quadrata (icona dell'exe, pannello 101x101)

Flat vector app icon, bold minimal graphic style, fully stylized, designed to stay readable when scaled down to 32x32 pixels. Aspect ratio 1:1.

STYLE RULES (these override everything else):
- Pure flat vector art: solid color fills, thick uniform outlines, simple geometric shapes.
- No gradients, no shading, no shadows, no textures — except the neon glow of the green arrows.
- NOT a photograph, NOT a 3D render, NOT photorealistic, no realistic hands, no anatomical detail.

BACKGROUND: one solid flat deep blue square, color #0B06B0. No scenery, no clutter.

SUBJECT — only three things, nothing else:
1. One large stylized hand entering from the left, flat vector, side view, open and extended toward the right, offering a handshake. Exactly five fingers as simple rounded shapes.
2. A mirrored stylized hand entering from the right, extended toward the left, identical style. The two hands face each other and DO NOT touch: a clear gap separates them at the center.
3. Two neon green arrows (#28A745 at neon brightness): one arcs above, the other arcs below, together tracing an open oval ring that frames both hands. Their tips almost meet but leave a small visible gap at each end. Each arrow grows out of the wrist of the hand nearest to it, overlapping and merging with it. Soft even neon trail with a few glowing dots.

The overall silhouette must read as a single circular emblem. Bold simple shapes, thick outlines, strong contrast against the blue, no small details, no scenery, no people, no factory, no charts, no stopwatch.

NO TEXT of any kind. Centered composition with even margins on all four sides.

---

## Righe di ricambio

Candidato "prima" in versione caricaturale (stile Sims) — sostituisce la frase del
BEFORE nel prompt A:

    Left of the slash, the BEFORE: slumped shoulders, plain shapeless clothes, a cartoonish frustrated face, rubbing thumb and fingertips together in the universal "give me money" gesture, with a few flat coin shapes floating away.

Se esce ancora troppo morbida o realistica, aggiungi in testa al prompt:

    Style reference: flat corporate vector illustration, Adobe Illustrator artwork, sticker art, screen-print poster. Absolutely no realism.

Se invece esce troppo povera e piatta, ammorbidisci di un gradino:

    Flat vector illustration with subtle two-tone shading on the figures (two flat tones per shape, no gradients).

Formato verticale per lo splash — sostituisce la riga dell'aspect ratio nel prompt A:

    Aspect ratio 3:2.

---

## Come usarlo su ChatGPT

1. Incolla il prompt cosi' com'e', preceduto da: "Genera l'immagine usando esattamente
   questo prompt, senza riscriverlo ne' riassumerlo."
2. Una immagine per volta: le varianti multiple abbassano la resa dei dettagli.
3. In vettoriale le mani sbagliano molto meno che in realistico. Se capita comunque,
   rigenera aggiungendo in coda: "Critical: each hand must have exactly four fingers
   and one thumb, clearly separated."
4. Nessuna scritta nell'immagine: il lettering "TrovaLavoro" si aggiunge dopo, con un
   font vero (Segoe UI Bold, coerente con l'app). Chiedere le lettere al generatore
   produce quasi sempre refusi.
5. Verifica finale dell'icona: riducila a 32x32 px e guardala. Se non si capisce che
   sono due mani e un anello verde, l'icona va semplificata ancora.

## Parole da non rimettere mai nel prompt

realistic · semi-realistic · photorealistic · cinematic · rich in detail ·
natural skin tones · depth · render · 3D · lighting · texture.
Sono quelle che nella revisione 1 hanno prodotto l'immagine realistica.
