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

*Tutte chiuse a T6: l'export del registro e il filtro per stelle sono in «Chiuse».*

## Da T5d — il profilo dalla propria pagina (2026-08-14)

- **Una pagina che non sia LinkedIn.** L'import legge «la pagina aperta», e sia la pagina
  di casa sia il suggerimento del bottone promettono che vada bene anche «un altro sito che
  racconti il tuo percorso»: provato però è stato **solo** su LinkedIn. Il dubbio non è la
  strutturazione — quella non sa da dove venga il testo — ma lo **scorrimento**, che su
  quella pagina ha dovuto cercarsi da solo il contenitore che scorre: un sito fatto in un
  altro modo può scorrere in un altro modo. Serve una prova su una pagina «chi sono» vera.
  *(cap. 06.7.)*

## Da «Elimina profilo» (2026-08-14)

*Chiusa a T6: la cartella dati usa-e-getta è in «Chiuse».*

## Da T6 — le email di candidatura (2026-08-14, alla chiusura)

- **Il destinatario proposto dall'annuncio.** Il cap. 07.1 promette: «se l'annuncio conteneva
  un indirizzo, viene proposto». Oggi il campo è **sempre vuoto**, e non per prudenza: il
  prompt `analisi_annuncio` non estrae nessun recapito, quindi non c'è niente da proporre.
  Metà della promessa è mantenuta (il programma non inventa mai un indirizzo), l'altra metà
  no. Costa un campo nell'analisi e un bump di pool. *(cap. 07.1; pool `analisi_annuncio`.)*
- **La porta «qui c'è tutto» del profilo.** Il cap. 05.2 descrive una cartella che serve a
  **due** cose: proporre gli attestati da allegare (fatto a T6) e trovare da sé il CV da cui
  costruire il profilo. La classificazione dice già quale CV sembra il più recente e il dato
  si salva in `documenti.json`, ma **non lo legge nessuno**: l'import di un CV passa ancora
  dalla scelta di un file singolo. *(cap. 05.2; `RaccoltaDocumenti.CvPiuRecente`.)*
- **Lo stato «esito», il follow-up e il destinatario nel registro.** Il cap. 07.3 assegnava
  a T6 tre cose oltre all'invio, e T6 ne ha portata una sola. Restano: lo stato **`esito`**
  (in attesa · colloquio · rifiutata · assunto 🎉), che nello schema c'è ma dall'interfaccia
  non si raggiunge; il **promemoria di follow-up** per le candidature ferme da giorni; e il
  **destinatario nella voce di registro**, che oggi vive nella bozza `email.json` della
  candidatura e non nell'indice. Sono tre passi del racconto «a che punto sono», e vanno
  fatti entro la 1.0. *(cap. 07.3; cap. 14, T9.)*

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

- ✅ **L'`.eml` aperto in un programma di posta vero e spedito da lì** *(aperta e chiusa il
  2026-08-14, la sera stessa della tappa)*. Mirco ha percorso il giro sui **dati veri**: la
  candidatura a Delta Sistemi preparata dall'applicazione, aperta nel **nuovo Outlook** —
  che ha riconosciuto `X-Unsent` e l'ha mostrata come bozza pronta, con destinatario,
  oggetto e corpo — e **spedita da lì**, al proprio indirizzo. L'email è arrivata, allegati
  compresi. Lungo la strada un falso allarme istruttivo: al primo tentativo Outlook ha detto
  «Non è stato possibile allegare i file… Riprova più tardi». Il file era a posto (validato
  di nuovo con un lettore indipendente: `X-Unsent`, intestazioni, e i due PDF **identici
  byte per byte**); a mancare era **la sessione dell'account**, scaduta dentro Outlook — che
  per allegare un messaggio importato deve parlare col servizio. Rifatto l'accesso, gli
  allegati sono stati caricati e l'invio è andato. *(cap. 07.2; cap. 14, T6.)*
- ✅ **La scelta della cartella documenti, premuta a mano** *(aperta e chiusa il
  2026-08-14)*. Il `FolderBrowserDialog` che lo strumento di collaudo non sa pilotare l'ha
  premuto Mirco, indicando la **sua** cartella dei documenti personali. Tredici file letti,
  sottocartelle di primo livello comprese: i due CV riconosciuti (col più recente indicato),
  e **tutto il resto in «altro»** — carte d'identità, codici fiscali, NASPI, documenti dello
  stage. La prova ha mostrato la cosa che contava, cioè quella che **non** è successa:
  nessun documento personale è finito fra gli allegabili di un'email. *(cap. 05.2;
  `strumenti/mcp-collaudi/`.)*
- ✅ **La chiave API cifrata (DPAPI)** *(aperta il 2026-08-07 da T3, chiusa il 2026-08-14 da
  T6)*. Vive in `segreti.bin` nella cartella dati, cifrata con la protezione dati di Windows
  e **legata all'account** che l'ha salvata: copiata altrove non si apre. La si digita in una
  finestra al primo avvio — il posto che a T3 mancava — e la si sostituisce riavviando con
  `--chiave`, finché le Impostazioni non arriveranno con T9. Nella diagnostica compare solo
  come `sk-ant-…1234`, che è la metà del collaudo di tappa verificabile qui.
  *(cap. 11.3; diario Step 2.19.)*
- ✅ **Avviare l'applicazione su una cartella dati usa-e-getta** *(aperta il 2026-08-14 da
  «Elimina profilo», chiusa lo stesso giorno aprendo T6)*. `--dati <percorso>` sposta l'intera
  applicazione su un'altra radice, dichiarandolo nel titolo della finestra e nella barra di
  stato. Ha fatto subito il suo mestiere: tutto il collaudo di T6 è stato percorso su copie
  dei dati veri, senza mai toccarli. *(cap. 11.1; diario Step 2.17.)*
- ✅ **La fascia dei comandi a finestra stretta** *(aperta il 2026-08-14 da T5d, peggiorata due
  volte, chiusa lo stesso giorno aprendo T6)*. La fascia ora **va a capo** quando lo spazio non
  basta, e la geometria — che era ricopiata in cinque pannelli — vive in un posto solo
  (`Ui/FasciaDeiComandi`). I 676 px di bottoni sovrapposti alla larghezza minima non ci sono
  più, e il collaudo cammina su cinque pannelli a sei larghezze verificando che due comandi non
  si intersechino mai. *(cap. 03.4; diario Step 2.17.)*
- ✅ **L'export del registro in CSV/markdown** *(aperta il 2026-08-13 da T5c, chiusa il
  2026-08-14 aprendo T6)*. Esce quel che è a schermo, filtrato e ordinato com'è: il markdown si
  porta il proprio perimetro in cima («3 di 12»), il CSV no perché una frase sopra una tabella
  non è più una tabella. *(cap. 07.3; diario Step 2.17.)*
- ✅ **Il filtro per stelle nella coda** *(aperta il 2026-08-13 da T5c, chiusa il 2026-08-14
  aprendo T6)*. La tendina dice «almeno N», i due filtri si intersecano, e quando nascondono
  qualcosa i contatori aggiungono «ne vedi 1 su 3» — perché una candidatura mai confrontata non
  ha stelle e sparirebbe in silenzio. *(cap. 07.3; diario Step 2.17.)*
- ✅ **L'eliminazione vera non è mai stata premuta sull'applicazione** *(aperta e chiusa il
  2026-08-14)*. L'assistente si era fermata un passo prima — bottone premuto, finestra
  aperta, parola scritta, «Elimina il profilo» acceso, e poi **annullato**, perché
  dall'altra parte c'era il profilo vero. L'ha premuto **Mirco**, lanciando l'applicazione
  con `avvia-demo.bat` sui propri dati, con la copia di sicurezza pronta accanto. Il
  risultato è quello che il cap. 11.5 promette e i collaudi verificavano al banco: la
  cartella `profilo\` sparita per intero — profilo, storico, `cv_base.json` e la sua
  `out\` — e **tutto il resto al suo posto**, le sei candidature nelle loro cartelle, il
  `registro.json` non riscritto, i dati di navigazione intatti. La prova che mancava era
  questa; resta aperto il modo di rifarla **senza** rischiare i dati veri (voce qui sopra).
  *(cap. 11.5; diario Step 2.16.)*
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
