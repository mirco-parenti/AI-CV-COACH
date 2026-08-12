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
- **La chiave API cifrata (DPAPI) è assegnata a T6.** Oggi arriva dalla variabile
  d'ambiente `ANTHROPIC_API_KEY`, come a T2, e a T3 resta così: il passaggio a
  `segreti.bin` cifrato vuole un posto dove l'utente la digita — primo avvio o
  Impostazioni — che a T3 non esiste ancora. Il cap. 14 non lo assegnava a nessuna
  tappa; deciso il 2026-08-07 di metterlo in **T6**, il cui collaudo già verifica che la
  chiave non compaia in chiaro su disco né nei log. *(cap. 02.5; cap. 11.3; cap. 14, T6.)*

## Da T4 — la pipeline di candidatura (2026-08-10 · integrata alla chiusura, 2026-08-11)

- **I documenti prodotti aperti in Word.** La gamba C del collaudo di tappa (cap. 14)
  chiede DOCX e PDF aperti **in Word e in LibreOffice**: LibreOffice c'è su questa
  postazione e ha detto la sua — apre entrambi, li riconverte, accenti e simboli intatti —
  ma Word qui non è installato. È il gemello in **scrittura** della voce di T3 qui sopra,
  che riguarda la **lettura** di un `.docx` salvato da Word: serve la stessa macchina, e
  conviene chiuderle insieme. *(cap. 05.7; cap. 14, T4 gamba C.)* **Riconfermata alla
  chiusura di T4** *(2026-08-11)*: il collaudo di tappa ha ripercorso la gamba per intero —
  114 campi su 114 ritrovati, DOCX e PDF identici — ma la metà di Word è di nuovo rimasta
  fuori, per la stessa ragione.
## Da T5 — la ricerca annunci e il registro (2026-08-13, alla chiusura di T5c)

- **L'export del registro in CSV/markdown.** Il cap. 07.3 lo promette «fin dal primo
  rilascio» — serve a Mirco per documentare l'uso reale del prodotto nella propria ricerca
  di lavoro — e a T5c **non è stato fatto**: deciso con Mirco il 2026-08-12, perché
  aggiungere una strada d'uscita a dei dati che stavano ancora prendendo forma sarebbe
  costato due volte. Ora la forma ce l'hanno. Va fatto entro la 1.0 (T9 al più tardi).
  *(cap. 07.3; cap. 14, T9.)*
- **Il filtro per stelle nella coda.** Il cap. 07.3 chiede un elenco «ordinabile e
  filtrabile **per stato e stelle**»: a T5c si ordina per qualunque colonna (stelle
  comprese) e si filtra **per stato** («Mostra»: tutte, da completare, generate, scartate),
  ma un filtro sulle stelle — «fammi vedere solo quelle sopra le 3» — non c'è. Con sei
  candidature l'ordinamento basta; con sessanta no, ed è quello lo scenario per cui il
  registro esiste. *(cap. 07.3; cap. 03.6, P1.)*

## Da revisione adversariale (2026-08-09)

- **Il pannello del logo a DPI alti.** Le costanti di geometria sono in pixel non scalati:
  a 125/150% di scala — l'impostazione di fabbrica di quasi tutti i portatili recenti — il
  disegno è fuori misura. Difetto vero, ma correggerlo alla cieca rischiava di rompere il
  layout **validato a video** in T3: serve uno schermo su cui verificare a 150%.
  *(cap. 03.5; segnalato dalla revisione, rimandato con motivo.)*
- **Il «corso senza nome» che una volta su tre sparisce.** Al turno contatti della traccia
  reale, un corso citato di sfuggita non viene sempre ripescato: varianza del modello, non
  regressione — l'istruzione nel prompt c'è già, e insistere con altre regole rischia di
  peggiorare altro. Ma l'anti-perdita promette che nulla si perde in silenzio, quindi non è
  archiviabile: va ripreso quando si rimetterà mano ai prompt del dialogo (o col salto di
  modello, v. voce T2). *(casi/reale/dialogo_guidato.md; ricomparso una volta nel giro di
  validazione del Pool 1.02.)*

## Chiuse

- ✅ **La coda dell'opportunità non si riapre** *(aperta il 2026-08-10 da T4, chiusa il
  2026-08-13 da T5c)*. La vista che mancava è la **Home**: la coda mostra tutte le
  candidature con stelle, stato e provenienza, e da lì una si riapre col doppio clic o col
  suo bottone, giudizi e documenti compresi. Provata sulle sei candidature vere di Mirco —
  comprese quelle di T4, che il campo `stato` non ce l'hanno e se lo fanno **dedurre dai
  file presenti** invece di farsi riscrivere — e con l'applicazione **chiusa e riaperta**,
  che è la domanda vera: quello che è stato scritto ieri si ritrova domani. La promessa
  «tutto riapribile» del cap. 12.7 non è più mantenuta solo sul disco.
  *(cap. 11.1; cap. 12.7; cap. 14, T5c; diario Step 2.13.)*
- ✅ **Un annuncio davvero pescato da un portale** *(aperta il 2026-08-10 da T4, chiusa il
  2026-08-12 da T5b)*. La gamba B di T4 era stata percorsa con un annuncio **verosimile ma
  scritto per il collaudo**; adesso la cattura dal browser esiste, e l'annuncio è arrivato
  da **Indeed** con tutto quello che nessuno si inventa — il banner dei cookie, i filtri, i
  menù e i titoli degli altri annunci nella stessa pagina. L'analisi ne ha ricavato il
  ruolo giusto con la sua azienda e il suo contratto, il confronto ha dato 1,4 stelle su 10
  voci, e l'opportunità è finita nella sua cartella con fonte e link.
  *(cap. 06.4; cap. 14, T4 gamba B e T5b; diario Step 2.12.)*
- ✅ **Il giro dal menù dei portali, mai provato sull'applicazione vera** *(aperta il
  2026-08-12 alla chiusura di T5a, chiusa lo stesso giorno)*. Non era un difetto del
  pannello ma un **limite dello strumento** di collaudo, che sapeva premere bottoni e
  scrivere nelle caselle ma non scegliere una voce da una tendina: Indeed risultava provato
  solo perché è il primo ed è già selezionato, gli altri tre erano stati aperti dalla
  casella dell'indirizzo. Insegnato allo strumento a scegliere (`scegli_voce`), il giro è
  stato percorso su tutti e quattro i portali. Lungo la strada lo strumento ha scoperto due
  difetti **propri**, che facevano premere il controllo sbagliato e riferire il contrario
  del vero. *(strumenti/mcp-collaudi/README.md; diario Step 2.11 e 2.12.)*
- ✅ **`taratura.json` e `modelli.json` non li legge nessuno all'avvio** *(aperta il
  2026-08-07 da T3, chiusa il 2026-08-07 da T3c)*. Mancava il punto in cui il motore si
  monta all'avvio: l'ha portato **`Motore/ContestoApp`**, che carica entrambi i file e
  avvisa quando ripiega sui predefiniti. Da allora un numero ritoccato nella cartella dati
  ha effetto sull'applicazione, che era esattamente ciò che non succedeva.
  *(cap. 11.6; cap. 02.3.)*
- ✅ **La città quando il CV ne porta due** *(aperta il 2026-08-08 da T3, chiusa il
  2026-08-09 dalla revisione adversariale)*. Il **Pool 1.02** dichiara nei prompt che la
  città è quella del **domicilio** — dove uno è raggiungibile per lavorare — e una sola;
  il campo è **tornato un pass/fail** nel collaudo reale, e morde: la residenza oggi
  boccerebbe. *(cap. 04; pool CHANGELOG 1.02; `CollaudoReale`.)*
- ✅ **Le lingue non hanno un posto** *(aperta il 2026-08-08 da T3, chiusa il 2026-08-09
  dalla revisione adversariale)*. Il rimedio concordato è nel **Pool 1.02**, esteso a tutti
  i prompt che toccano le competenze: le lingue **sono** competenze, riportate come dette,
  **mai con un livello non dichiarato**. Validato sul CV vero: 3 lingue su 3 ricopiate alla
  lettera, contro le 0 del prototipo. Il campo `lingue` vero e proprio resta un'idea futura.
  *(pool CHANGELOG 1.02; diario Step 2.8.)*
- ✅ **Il patentino del muletto si perde, dichiarandolo** *(aperta il 2026-08-09 da T3,
  chiusa il 2026-08-09 dalla revisione adversariale)*. Il **Pool 1.02** dà a quel genere di
  qualifica la sua casa: i **patentini professionali stanno in formazione**, e lo dicono
  tutti i prompt coinvolti (i blocchi `altrove`, `competenze` che separa l'abilità dal
  certificato, `patente` che chiarisce che non sono patenti di guida). Nel dialogo reale il
  muletto atterra in formazione **al primo colpo**. *(pool CHANGELOG 1.02; diario Step 2.8.)*
- ✅ **P5: a dialogo finito resta un buco** *(aperta il 2026-08-09 da T3, chiusa il
  2026-08-09 dalla revisione adversariale)*. La fascia della risposta ora **si ritira**
  quando il dialogo chiude: niente più zona morta fra l'ultima bolla e i bottoni.
  *(cap. 03.6; commit `a813253`.)*
- ✅ **P5: l'eco arriva dopo il verdetto** *(aperta il 2026-08-09 da T3, chiusa il
  2026-08-09 dalla revisione adversariale)*. Era davvero una decisione sul disegno di
  `Mossa`, ed è stata presa: la mossa ora porta **eco ancorate** al punto giusto della
  sequenza, così l'utente rivede le proprie parole *prima* del «lo lascio fuori» — e nella
  passata finale ogni recupero ha la sua eco, dove prima un campo singolo le sovrascriveva.
  *(cap. 02.4; commit `a813253`.)*
