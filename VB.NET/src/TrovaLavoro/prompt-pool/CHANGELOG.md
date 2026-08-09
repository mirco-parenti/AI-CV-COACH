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

## Pool 1.01 — 2026-08-09

**Un'attività, una sezione sola.** Due regole nuove in `profilo/importa_cv.md` (§2),
nate da ciò che il collaudo di tappa di T3 ha misurato sul CV vero: un blocco solo —
un volontariato con un ruolo, un'organizzazione e una descrizione ricca — veniva letto
in tre modi diversi da un giro all'altro.

- **Ogni attività va in UNA sola sezione.** Capitava che lo stesso volontariato
  comparisse fra le esperienze formali *e* fra le informali: la stessa cosa contata due
  volte, che nel confronto con un annuncio pesa doppio. Il prompt non lo vietava da
  nessuna parte. Ora sì.
- **Decide la natura, non la sezione del CV.** La regola qui sopra, da sola, ha spostato
  il difetto invece di toglierlo: il doppione è sparito, ma una lettura su cinque ha
  risolto l'ambiguità scegliendo la sezione sbagliata — il volontariato promosso a
  impiego, perché quel CV lo stampa sotto «esperienza lavorativa» e il modello seguiva
  l'impaginazione. Ora il prompt dice che a decidere è l'attività: se il ruolo o la
  descrizione la dicono volontaria, è informale anche quando ha ruolo, organizzazione e
  periodo, e anche quando il CV la mette fra i lavori.
- **I dettagli di un'esperienza non sono esperienze.** Abilitazioni, corsi e
  riconoscimenti citati dentro la descrizione di un'attività venivano a volte promossi a
  esperienze a sé: un corso diventava un'esperienza informale invece di restare nella
  descrizione o di andare in `formazione`, dove le regole del prompt lo mandavano già.

Nessun cambiamento di schema: gli stessi campi, la stessa forma JSON. È il primo punto
in cui l'app **si stacca di proposito** dal prototipo, che quelle due regole non le ha:
da qui in avanti, su `importa_cv`, il prototipo non è più il metro: è il termine di
paragone di ciò che l'app fa meglio.
