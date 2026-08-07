# Storia del pool dei prompt

Il pool si aggiorna **tutto insieme** e ha una versione propria, indipendente da quella
dell'applicazione (cap. 04.1). Ogni modifica a un prompt si chiude con il rito del bump
(cap. 04.5): si aggiorna `versione_pool`, si rigenerano le impronte con «Sigilla pool»,
si annota qui che cosa è cambiato e perché.

Questo file è un documento, non un prompt: il sigillo lo riconosce dall'assenza
dell'intestazione di metadati e lo lascia fuori dal manifest — altrimenti annotare il
changelog dopo il sigillo farebbe risultare modificato il pool appena sigillato.

## Pool 1.00 — 2026-08-06

Nascita del pool. I **15 prompt del prototipo** entrano nella libreria con il **testo
validato invariato**: cambiano solo i segnaposto, che passano dall'interpolazione di
JavaScript alla forma `{{NOME}}` (cap. 04.4).

- `profilo/` — i sette turni del dialogo guidato (`nome`, `contatti`, `patente`,
  `esperienze_formali`, `esperienze_informali`, `competenze`, `formazione`), più
  `importa_cv` e `trascrizione_pdf`.
- `annuncio/analisi_annuncio.md` — requisiti con priorità e contesto.
- `confronto/` — `confronto` (giudizi voce per voce, campo `eliminatorio` incluso) e
  `mitigazione`.
- `generazione/` — `cv_base.it`, `cv_mirato.it`, `lettera.it`.

La non-regressione contro il prototipo (cap. 14, T2) è passata su questa versione: il
prompt del confronto costruito dal pool è identico carattere per carattere a quello che
il prototipo costruisce nel codice.
