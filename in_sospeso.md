# In sospeso — AI-CV-COACH

Raccolta **unica** delle cose **già decise e dentro il perimetro** che sono rimaste
indietro: una tappa si è chiusa lo stesso, ma qualcosa aspetta il momento giusto, una
macchina che qui non c'è, o una mano che non è quella dell'assistente.

*Cos'è e cosa non è.* Qui non ci sono idee da valutare: quelle stanno in
`idee_future.md`, che è il backlog dei **raffinamenti futuri**. La differenza è netta —
una voce di `idee_future.md` potrebbe non farsi mai, una voce di questo file **va
fatta**, e se non è ancora fatta è un debito aperto. Lo **stato** del progetto non si
copia qui: sta nel `README.md` e nell'ultimo `### Step` del `diario_di_bordo.md`.

*Come si compila (modalità):* una riga per voce, raggruppata per **tappa d'origine**;
ogni voce dice **cosa manca · perché è rimasta indietro · dove ne parla il progetto**,
con la data in cui è emersa. Quando una voce è chiusa si sposta in fondo, in
**«Chiuse»**, con la data e come si è chiusa: così l'elenco attivo qui sopra resta solo
di cose da fare. Aggiornato con "aggiorna-tutto".

## Da T1 — lo scheletro (2026-08-06)

- **La prova dell'exe su un PC davvero pulito.** Il publish single-file è stato
  verificato sulla macchina di sviluppo, che ha l'SDK installato: non è la stessa cosa
  che un PC senza niente. È il collaudo dichiarato di T1 (cap. 14) e serve una seconda
  macchina, quindi tocca a Mirco. *Fino ad allora il vincolo più rigido del progetto è
  dimostrato al 90%, non al 100%.*
- **L'icona dell'eseguibile.** Oggi l'exe ha l'icona predefinita di Windows. Va prodotta,
  idealmente dallo stesso scudo Aviolab già incorporato nel sorgente. È una scelta di
  prodotto, non di codice. *(cap. 13.5; cap. 15, voce 4.)*
- **L'SDK .NET 10 sulla postazione del tutor.** Lì c'è ancora l'SDK 9: quella macchina
  oggi non compila il progetto. *(cap. 13.1; regola 11 — le due postazioni.)*
- **La cartella `VB.NET/src/pubblicazione/`** (111 MB, ignorata da git) è rimasta su
  disco dopo il publish di prova. Non dà fastidio a nessuno e non entra nel repo: si
  cancella quando lo spazio serve.

## Da T2 — il motore e il pool (2026-08-07)

- **Il salto a Sonnet 5, il «secondo esperimento».** Il modello di prodotto per il
  ragionamento è Sonnet 5, ma il predefinito di oggi è **Sonnet 4.6**, lo stesso del
  prototipo, perché la non-regressione si misura a parità di modello. Il salto si fa da
  `modelli.json` senza ricompilare, e va **misurato da solo**: porta con sé
  l'interruttore del ragionamento esteso, che lì va acceso, e un conteggio dei token
  diverso (~+30% a parità di testo). *(cap. 02.5; cap. 15, voce 6.)*

## Da T3 — il profilo (2026-08-07 · integrata alla chiusura, 2026-08-09)

- **Un `.docx` salvato davvero da Word.** La gamba A del collaudo di tappa ha provato le
  quattro porte d'ingresso, ma i file DOCX, TXT e MD sono stati **fabbricati** dal testo
  trascritto dal PDF: provano le *strade di lettura*, non l'impaginazione di Word, che su
  questa postazione non c'è. Serve una macchina che ce l'abbia. *(cap. 05.1; cap. 14, T3,
  gamba A — il limite è dichiarato anche nel collaudo.)*
- **La città quando il CV ne porta due.** Il CV di prova ha residenza *e* domicilio, e
  nessun prompt dice quale finisca in `contatti.citta`: a trascrizione identica bit per bit,
  tre esecuzioni hanno dato tre risposte diverse. Il collaudo perciò **segnala e non boccia**
  quel campo (⚠️). Da decidere: dichiarare nei prompt che la città è quella del **domicilio**
  — dove uno è raggiungibile per lavorare — e una sola; poi il campo può tornare un pass/fail.
  *(2026-08-08; cap. 04; `CollaudoReale.PerchePerLaCitta`.)*
- **Le lingue non hanno un posto.** Nessun prompt del pool le nomina: «Inglese B2» finisce
  fra le competenze o svanisce a seconda del giro (misurato: 3, 0, 2, 2, 2 lingue su cinque
  letture dello stesso CV). Rimedio già concordato, da fare col rito del bump: dire in **due**
  prompt — `importa_cv` e `competenze`, altrimenti profilo-da-CV e profilo-da-dialogo
  divergono — che le lingue sono competenze, riportate **come le scrive il CV**, e **mai con
  un livello non dichiarato** (senza quest'ultima clausola il modello promuove «un po' di
  inglese» a «Inglese B1», saltando l'anti-invenzione). *(2026-08-08; il campo `lingue` vero e
  proprio è invece un'idea futura, `idee_future.md`.)*
- **Il patentino del muletto si perde, dichiarandolo.** Nel dialogo il turno della patente lo
  instrada alle competenze, il turno delle competenze lo rimanda alla formazione, e la guardia
  anti-rimbalzo lo scarta con un «lo lascio fuori» — tre giri su tre. Il meccanismo fa
  esattamente il suo mestiere: è **dove** quel genere di qualifica debba andare che nessun
  prompt dice. *(2026-08-09; cap. 12.2; il rimedio alternativo — far scegliere all'utente —
  sta in `idee_future.md`.)*
- **P5: a dialogo finito resta un buco.** Quando la conversazione chiude, casella e «Invia»
  spariscono (giusto) ma lo spazio che occupavano resta vuoto, e fra l'ultima bolla e la
  fascia dei bottoni si apre una fascia morta. Visto solo a video. *(2026-08-09; cap. 03.6.)*
- **P5: l'eco arriva dopo il verdetto.** Quando un frammento ripescato non si riesce a
  collocare, l'utente legge «lo lascio fuori: X» *prima* di rivedere le proprie parole: il
  pannello disegna tutto ciò che l'assistente dice e poi l'eco. Nel caso riuscito l'ordine è
  giusto (annuncio → eco → schede), in questo no. Sistemarlo tocca il disegno di `Mossa`
  — oggi le bolle e l'eco viaggiano separate — quindi è una decisione, non una toppa.
  *(2026-08-09; cap. 02.4; cap. 03.6.)*
- **La chiave API cifrata (DPAPI) è assegnata a T6.** Oggi arriva dalla variabile
  d'ambiente `ANTHROPIC_API_KEY`, come a T2, e a T3 resta così: il passaggio a
  `segreti.bin` cifrato vuole un posto dove l'utente la digita — primo avvio o
  Impostazioni — che a T3 non esiste ancora. Il cap. 14 non lo assegnava a nessuna
  tappa; deciso il 2026-08-07 di metterlo in **T6**, il cui collaudo già verifica che la
  chiave non compaia in chiaro su disco né nei log. *(cap. 02.5; cap. 11.3; cap. 14, T6.)*

## Chiuse

- ✅ **`taratura.json` e `modelli.json` non li legge nessuno all'avvio** *(aperta il
  2026-08-07 da T3, chiusa il 2026-08-07 da T3c)*. Mancava il punto in cui il motore si
  monta all'avvio: l'ha portato **`Motore/ContestoApp`**, che carica entrambi i file e
  avvisa quando ripiega sui predefiniti. Da allora un numero ritoccato nella cartella dati
  ha effetto sull'applicazione, che era esattamente ciò che non succedeva.
  *(cap. 11.6; cap. 02.3.)*
