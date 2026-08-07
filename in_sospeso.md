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

## Da T3 — il profilo (2026-08-07)

- **`taratura.json` e `modelli.json` non li legge ancora nessuno all'avvio.** I due file
  della cartella dati esistono nel codice — `Taratura.Carica` e `Modelli.Carica` sanno
  leggerli e ripiegare sui predefiniti — ma l'applicazione all'avvio apre solo la
  libreria prompt: `CalcoloMatch` e `ClientClaude` usano i valori interni, e i `Carica`
  oggi li chiamano solo i collaudi. Non è una regressione (è così da T2) e non è un
  errore: manca il punto in cui il motore si monta all'avvio, che nascerà con i pannelli
  (T3c) o con la pipeline (T4). Finché non c'è, ritoccare un numero nella cartella dati
  non ha effetto sull'app. *(cap. 11.6; cap. 02.5.)*
- **La chiave API cifrata (DPAPI) è assegnata a T6.** Oggi arriva dalla variabile
  d'ambiente `ANTHROPIC_API_KEY`, come a T2, e a T3 resta così: il passaggio a
  `segreti.bin` cifrato vuole un posto dove l'utente la digita — primo avvio o
  Impostazioni — che a T3 non esiste ancora. Il cap. 14 non lo assegnava a nessuna
  tappa; deciso il 2026-08-07 di metterlo in **T6**, il cui collaudo già verifica che la
  chiave non compaia in chiaro su disco né nei log. *(cap. 02.5; cap. 11.3; cap. 14, T6.)*

## Chiuse

*(Ancora nessuna: questo file nasce il 2026-08-07, alla chiusura di T2.)*
