# 06 — La ricerca degli annunci

*La funzione nuova più importante della fase desktop: trovare annunci adatti, anche sui
grandi portali, e portarli dentro la pipeline. La strada è quella decisa nello
Step 1.34 del diario: un browser vero dentro l'app, dove naviga l'utente.*

## 6.1 Perché un browser integrato (WebView2)

Il prototipo ha già dimostrato (prova sul campo, poi rimossa) che il prelievo diretto
delle pagine **non funziona**: sui portali moderni l'HTML scaricato è quasi vuoto,
perché la pagina si costruisce con JavaScript, spesso dietro login e protezioni
anti-robot. La soluzione non è aggirare le protezioni, è **cambiare punto di lettura**:

- dentro l'app c'è un **browser Edge/Chromium vero** (WebView2, componente di serie su
  Windows 11);
- **l'utente** naviga, cerca e, dove serve, **accede con il proprio account**, come
  farebbe in un browser qualsiasi;
- quando ha davanti un annuncio, il programma **legge la pagina che l'utente sta
  guardando** (il DOM già costruito, a JavaScript risolto) e la passa alla pipeline.

Questa impostazione è anche la bussola etica della funzione (cap. 01.4): niente
scraping massivo, niente automazione dell'accesso; si assiste la lettura di ciò che
l'utente sta legittimamente vedendo, per uso personale.

Rispetto alla lettera del mandato («trovare e scaricare» gli annunci), questa è una
**riduzione di perimetro dichiarata**: nessuna raccolta automatica in blocco — il
programma prepara le ricerche, l'utente sceglie, la cattura è un clic per annuncio.
È il compromesso che tiene insieme efficacia, rispetto delle regole dei portali e
qualità dei dati: si analizza solo ciò che un umano ha giudicato interessante.

## 6.2 Il pannello Ricerca (P3)

*Com'è venuto costruendolo (T5a, 2026-08-12): la barra di sopra è di tre righe, non di
una. Il disegno di dettaglio sta nel cap. 03.6, che è la sua casa; qui il ritratto.*

```
┌──────────────────────────────────────────────────────────────────┐
│ Ricerche salvate: [Indeed — magazziniere, Genova ▼] [Apri] [Dim.]│
│ Cerca su: [Indeed ▼]  cosa [________] dove [______] [Cerca][Salva]│
│ [◀] [⟳] [https://it.indeed.com/jobs?q=…________________]   [Vai] │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│                  WebView2 (il portale, navigabile)               │
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│ [✔ Cattura annuncio] [✔ Importa CV da questa pagina]             │
│                        Catturato: «Addetto/a spedizioni per      │
│                        servizi logistici».                       │
└──────────────────────────────────────────────────────────────────┘
```

*Il secondo bottone in basso è di T5d (2026-08-14) e legge la stessa pagina per portarla
altrove: al profilo invece che all'analisi (v. 6.7).*

## 6.3 Le ricerche salvate

Le **preferenze** dell'utente (tipologie di ruolo, zona, contratto, parole chiave —
flusso A3 del cap. 12) diventano ricerche pronte per i portali supportati. Una ricerca
salvata è solo un **indirizzo parametrizzato**: il programma compone l'URL di ricerca
del portale con le parole chiave e la zona, e lo apre nel browser integrato.

- Portali previsti al primo rilascio: **Indeed, Jooble, Subito.it** più una
  ricerca generica via motore di ricerca (per le pagine «lavora con noi» dei siti
  aziendali). *Scelta rivista il 2026-08-05 (cap. 15, voce 7):* LinkedIn Jobs esce dalla
  terna iniziale perché è il portale meno adatto ai ruoli operativi che il nostro utente
  cerca — magazziniere, addetto alle vendite, manutentore — mentre **Subito.it** è quello
  che quel pubblico riconosce di più. Resta comunque aggiungibile in qualunque momento,
  come tutti gli altri: è una riga di `ricerche.json`.
- *E rivista di nuovo il 2026-08-12, a T5a, dai fatti:* la **verifica sul campo** chiesta
  dal cap. 14 ha aperto i quattro indirizzi uno per uno nel browser integrato, e
  **InfoJobs non esiste più** — al suo posto risponde un avviso che dichiara la
  piattaforma «ufficialmente chiusa e non più disponibile». Al suo posto entra **Jooble**,
  un aggregatore che raccoglie anche gli annunci delle agenzie per il lavoro, dove sta
  gran parte delle offerte per quei ruoli. Gli altri tre hanno risposto: Indeed e Subito.it
  con le loro pagine di risultati, la ricerca generica con i suoi. La sostituzione è
  costata **una riga**, che è esattamente ciò che la tabella-dati prometteva.
- **Un portale può chiudere, e il programma deve poterlo scoprire tardi.** Gli schemi
  invecchiano — cambiano forma o spariscono — e il rimedio non è ricompilare: si corregge
  `ricerche.json` nella cartella dati. Uno schema che non è un indirizzo `http`/`https`
  viene rifiutato e dichiarato: quel file finisce dritto nella barra di un browser vero, e
  un `file://` o un `javascript:` lì dentro non sarebbe configurazione ma una sorpresa.
- L'elenco è una **tabella dati** (nome portale + schema di URL), non codice: aggiungere
  un portale non richiede una nuova build. La tabella vive in `ricerche.json` nella
  cartella dati (cap. 11.1), insieme alle preferenze e alle ricerche salvate.
- Nessun risultato viene prelevato in automatico: il programma apre la pagina dei
  risultati, **l'utente** sceglie cosa aprire e cosa catturare.

## 6.4 La cattura

Alla pressione di **«Cattura annuncio»**:

1. il programma legge dalla pagina corrente: **titolo**, **URL** e **testo visibile**
   (l'equivalente di «seleziona tutto → copia», eseguito sul DOM);
2. il testo va al prompt `analisi_annuncio` (invariato dal prototipo): ne esce
   l'**Annuncio JSON** con requisiti, contesto e — novità — la **lingua** rilevata
   dell'annuncio (per il cap. 10);
3. se la pagina **non contiene il testo di un annuncio** (una griglia di risultati, una
   home, una schermata di accesso), lo schema esce vuoto e l'app risponde con garbo:
   «questa sembra una pagina di elenco: apri il singolo annuncio e ricattura»;
4. l'annuncio entra nella **coda delle opportunità** con fonte e link; da lì in poi la
   pipeline è quella di sempre (confronto → stelle → generazione). *Fino a T5b «coda»
   voleva dire una cartella su disco; da **T5c** è anche una cosa che si vede — la Home la
   mostra con stelle, stato e provenienza (cap. 03.6), e la colonna «Da dove» è proprio la
   fonte che la cattura ha scritto qui.*

*Il punto 3 riscritto sui fatti (T5b, 2026-08-12).* Diceva «se la pagina non contiene un
annuncio», e sul campo si è scoperto che **dipende da com'è fatto il portale**. Il rifiuto
scatta dove doveva: sulla griglia di risultati di Subito.it e su una pagina senza offerte
(provata anche sulla nostra pagina di casa). Su **Indeed no**, e non è un difetto: quel
portale non ha una pagina di solo elenco — nel riquadro di destra tiene **sempre** un
annuncio aperto, il suo testo fa parte della pagina, e la cattura prende quello che
l'utente sta guardando, che è la cosa giusta. Il disegno prometteva un comportamento
uniforme che i portali non hanno: qui si corregge la carta, non il territorio.

*E il verdetto va corretto a sua volta (collaudo dal vivo di T9e, 2026-08-23).* Su Indeed la
cattura **non** prende «quello che l'utente sta guardando»: `LettorePagina` parte da
`document.body`, quindi insieme al riquadro di destra porta via anche la **lista** di
sinistra — e su una lista a scorrimento infinito i venticinque passi di scorrimento nel
frattempo **caricano altri annunci**, così il di più cresce invece di restare fisso. Il
danno non è il testo in eccesso: è che l'applicazione analizza l'accozzaglia, dà un
punteggio in stelle e **sembra funzionare**. Chi conosce il programma se ne accorge e apre
l'annuncio in una finestra a parte; chi lo usa e basta, no. Deciso di riconoscere la
pagina-lista e fermarsi, e di catturare la **selezione** quando c'è; la scelta del
sottoalbero più «articolo» per densità di testo resta fuori dalla 1.0, perché cambierebbe
la cattura su tutti i portali e andrebbe misurata su ciascuno.

*E il rifiuto, a sua volta, era diventato un **vicolo cieco*** *(2026-08-30, segnalato da
Mirco sull'uso vero)*. Il giudizio di R5 guarda la **forma** del testo — righe corte, nessun
paragrafo lungo, parole di servizio ripetute — e su Indeed **anche un annuncio aperto da solo
ha quella forma**: è fatto di elenchi puntati, e in coda porta «Candidati» e i lavori simili
coi loro «giorni fa». Così chi seguiva il consiglio dell'avviso — clic destro, «Apri
collegamento in una nuova finestra», ripremi «Cattura annuncio» — si vedeva tornare lo stesso
avviso, parola per parola, all'infinito: il consiglio era giusto e il verdetto non cambiava.
L'unica uscita rimasta era selezionare il testo col mouse, che l'avviso dice per ultima e che
nessuno legge quando ha appena fatto quel che gli era stato chiesto.

La cura è che **l'indirizzo parla prima del testo**. Se l'URL è quello della pagina di un
singolo annuncio — `it.indeed.com/viewjob?jk=…`, `jooble.org/desc/…`, un `.htm` di Subito,
`linkedin.com/jobs/view/…` — il giudizio sulla forma non si applica affatto: non perché sia
sbagliato, ma perché sta rispondendo a una domanda a cui l'indirizzo ha già risposto meglio.
Quel sapere non era nuovo: che `jobs?q=` cerchi e `viewjob?jk=` sia l'annuncio stava scritto
in `Ricerche.FonteDi` da T5 — semplicemente la cattura non lo guardava.

Due limiti dichiarati. È **conoscenza che invecchia**: un portale può cambiare i suoi
indirizzi domani, e allora il segno smette di riconoscersi — ma non rompe niente, si torna
esattamente al giudizio sul testo di prima. E vale per **quattro portali**, non per il mondo:
fuori di lì decide il testo, con il rifiuto di R5 così com'è. *Deciso con Mirco lo stesso
giorno, scartando l'alternativa — trasformare l'avviso in una domanda a due uscite («leggila
lo stesso» / «annulla»), che avrebbe tolto il vicolo cieco ovunque ma riaperto la porta al
difetto che R5 era nato per chiudere: stelle su un'accozzaglia di offerte, stavolta con il
permesso dell'utente.*

*E la cura ha avuto il suo difetto, trovato provandola dal vivo la sera stessa.* Il primo
segno scritto per Indeed era `jk=` nudo. Ma la **pagina di ricerca** di Indeed, appena si
clicca un risultato, diventa `jobs?q=…&`**`vjk`**`=…` — *viewed job key*, l'annuncio mostrato
nel riquadro di destra — e `vjk=` contiene `jk=`: la lista passava per un annuncio, e la
cattura ci prendeva dentro tutte e venticinque le offerte. Cioè **esattamente il difetto che
R5 era nato per chiudere, riaperto dalla sua cura**, e su un portale solo — quello da cui
tutta questa storia era partita. Il banco non poteva vederlo: i suoi indirizzi di prova erano
`viewjob?jk=…` puliti, scritti da chi già sapeva come devono venire. L'ha visto la prova a
video, in cinque minuti, guardando la casella dell'indirizzo. Adesso il segno buono è il
**percorso** `/viewjob`, e il parametro vale solo dove un parametro comincia davvero — `?jk=`
o `&jk=` — con l'indirizzo vero della prova finito nei collaudi.

Due cose che il programma **non** fa, e sono decisioni:
- **non indovina** se la pagina sia un elenco. A dirlo è l'analisi, con lo schema vuoto;
  l'unica cosa che si controlla prima è che una pagina da mandare ci sia — sotto un
  minimo di testo non si spende una chiamata per sentirsi dire quel che si sa già;
- **non cattura due volte la stessa pagina.** L'identità è l'indirizzo, che è l'unica
  cosa esatta che si ha in mano: se quell'annuncio è già fra le opportunità lo dice e si
  ferma, invece di pagare una seconda analisi e lasciare nella coda due voci gemelle.
  Due candidature allo **stesso posto** restano invece legittime (cap. 11.1).

Il ripiego del prototipo resta sempre disponibile: **incollare il testo** dell'annuncio
a mano (utile per annunci ricevuti via email o messaggio). *E le due strade non sono
due: il testo catturato entra nella stessa casella in cui lo si incollerebbe, si vede, e
da lì parte l'analisi di sempre (cap. 03.6, P4).*

*«Si vede» ha smesso di essere vero per un po', e nessun collaudo poteva accorgersene*
*(T9d, 2026-08-22)*: il lettore cuce i pezzi della pagina con `\n`, e una casella
multiriga di Windows i `\n` non li mostra — la pagina compariva in un blocco unico,
illeggibile. Il testo era giusto ovunque: su disco, e in quello che partiva per l'AI.
Sbagliato era solo ciò che leggeva l'utente. Ora **gli a capo si convertono alla porta
della casella** (`Motore.TestoDaMostrare`), lo stesso rimedio che il corpo dell'email
usava già dalla sua parte: dentro il programma gli a capo restano `\n`, come vogliono
JSON, prompt e pagine web.

## 6.5 L'annuncio da link (flusso C)

Un link incollato nel campo dedicato apre la pagina nel browser integrato; l'utente
completa l'eventuale accesso e preme «Cattura annuncio». Stessa strada, stesso esito.
Non c'è alcun tentativo di scaricare il link «alla cieca»: la lezione dello Step 1.34
è definitiva.

## 6.6 Sessioni e riservatezza

- WebView2 conserva il proprio **profilo di navigazione** (cookie, sessioni) in una
  sottocartella della cartella dati dell'app: i login ai portali sopravvivono tra un
  avvio e l'altro, e un bottone nelle Impostazioni li cancella («Svuota dati di
  navigazione»). *Il bottone c'è da **T9b** (2026-08-21) ed è di livello 5: si conferma
  come uno scarto, partendo da «No», e la domanda dice che cosa sparisce davvero — le
  sessioni sui portali, non le candidature (cap. 11.5).*
- Le credenziali dei portali **non passano mai** dal programma: l'utente le digita nel
  browser, come farebbe in Edge. L'app non le vede e non le salva.
- Ciò che viene catturato (testo dell'annuncio) resta **sul PC dell'utente**, nella
  cartella dati; l'unico invio all'esterno è quello verso l'API AI per l'analisi.

## 6.7 Il profilo da LinkedIn (voce 2.1.3) — **dentro la 1.0**, fatta a T5d

Lo stesso meccanismo di cattura abilita la voce **2.1.3** del disegno funzionale:
l'utente apre la **propria** pagina profilo nel browser integrato, la cattura e la manda
alla strutturazione (`importa_cv`), che è già indipendente dalla fonte.

*Promossa nel primo rilascio il 2026-08-05 (cap. 15.5):* non richiede nessun componente
nuovo — il browser incorporato esiste già a T5 e il prompt di strutturazione è lo stesso
che legge un CV in PDF — quindi è poco più di un pulsante in più, a fronte di un
risparmio grosso per l'utente, che si trova il profilo quasi compilato invece di
riscriverlo. Si colloca **subito dopo T5**, quando la cattura è collaudata.

Due avvertenze da rispettare: l'utente deve **accedere a LinkedIn dentro il browser
integrato** come farebbe altrove (l'app non vede le credenziali, cap. 6.6), e si cattura
**solo la propria** pagina profilo — non quelle altrui. Vale anche qui la bussola del
capitolo 01.4: si assiste la lettura di ciò che l'utente sta legittimamente guardando.

### Com'è andata (T5d, 2026-08-14)

La promessa era «poco più di un pulsante in più», e alla lettera è stata mantenuta:
**nessun componente nuovo**. Il lettore di pagina era di T5a, la strutturazione non ha
mai saputo da dove venisse il testo, e il **pool non è stato toccato** — il turno che
legge la pagina è lo stesso `importa_cv` che legge un CV in PDF.

**I pulsanti però sono due, e stanno in due pannelli diversi.** In P3 c'è l'atto —
«Importa CV da questa pagina» — perché è lì che vive il browser ed è lì che c'è una
pagina aperta da leggere. In P2 c'è la scelta — «Importa CV da un sito…» — perché il
profilo si costruisce lì, e una terza strada raggiungibile solo entrando in un pannello
che si chiama «Ricerca» nessuno l'avrebbe trovata; quel bottone non legge niente, porta
in P3 e il pannello, arrivando, dice cosa fare. *(I due nomi qui sopra sono quelli con cui
i bottoni sono nati. Il 2026-08-14 quello di P2 è stato ribattezzato **«IMPORTA CV DA
LINKEDIN»** — cap. 03.6: un nome che sceglie il caso d'uso vero e promette meno di quanto
il bottone faccia, visto che di là si legge qualunque pagina; a dire il resto restano la
pagina di casa e il suggerimento. Quello di P3, «Importa CV da questa pagina», non è
cambiato.)* Il testo letto va **a P2**, che ha già
l'attesa annullabile, la guardia sulle correzioni non salvate e la scheda «Testo letto»
dove si controlla campo per campo: la finestra mostra prima il pannello e poi gli chiede
di leggere, com'era per l'annuncio catturato, così l'attesa si vede dove succede.

**Il fatto che il disegno non aveva previsto: una pagina va scorsa prima di leggerla.**
Su un sito moderno le sezioni del profilo entrano nel documento solo mentre si scende, e
un profilo tagliato a metà che si presenta come completo è peggio di un errore che si
dichiara. Letta com'era, la pagina vera ha dato **2196 caratteri** — la sola intestazione,
un'esperienza senza date né mansioni, nessuno studio, nessuna competenza; scorsa prima di
leggere, **9681**, con l'esperienza intera, dieci voci di formazione e nove competenze.
Due cose sono state misurate invece che supposte, e ognuna era costata un tentativo:
`window.scrollBy` su quella pagina **non muove niente** (a scorrere non è la finestra: si
chiede a `document.scrollingElement` chi lo faccia davvero, e in mancanza si cerca il
contenitore più grande che possa farlo), e il primo «sono in fondo» **è una bugia**,
perché una pagina non ancora caricata è corta — la discesa finisce solo dopo tre conferme
di fila che la pagina è finita e non cresce più, e si ferma se non si muove affatto.

**Lo scorrimento lo chiede solo l'import del CV.** La cattura dell'annuncio legge la
pagina com'è, ed è così che è stata collaudata su quattro portali a T5b: aggiungerle uno
scorrimento cambierebbe, su tutti, ciò che finisce nell'analisi — senza che nessuno
l'abbia chiesto. Un collaudo tiene ferma la distinzione.

**Non si controlla che la pagina sia LinkedIn, e nemmeno che sia la propria.** Il primo
controllo sarebbe inutile: la strutturazione è indipendente dalla fonte e legge
altrettanto bene la pagina «chi sono» di un sito personale. Il secondo è impossibile da
fare onestamente, perché l'indirizzo di un profilo non dice di chi sia. Il «solo la tua
pagina» resta una regola per l'utente, e gliela si **dice** — nella pagina di casa e nel
suggerimento del bottone — invece di fingere un controllo che non esiste.

*Provata sulla pagina vera con la chiave vera.* Il profilo che ne esce non contraddice
quello costruito dal CV in PDF: dove differisce, differisce perché differiscono le fonti
— LinkedIn non pubblica né email né telefono, e quei campi escono **vuoti invece che
inventati**. La pagina portava con sé altre cinque persone con nome, ruolo e azienda, nei
riquadri dei suggerimenti: nessuna è entrata nel profilo. E nemmeno «lingua del profilo:
italiano», che è un'impostazione dell'interfaccia e non una lingua che si parla.

*E provata anche dove il capitolo si limitava a prevederlo (2026-08-19)*: su una pagina
che **non è LinkedIn**, un sito aziendale moderno a pagina unica. Lo scorrimento — che su
LinkedIn aveva dovuto cercarsi il contenitore giusto — ha percorso la pagina fino al piè
di pagina, e la strutturazione ha ricavato quel poco che una pagina del genere dice di una
persona (nome, email, città, un ruolo) lasciando **vuoti** durata, tipo e telefono invece
di inventarli. La previsione «legge altrettanto bene un sito personale» regge quindi anche
sui fatti, non solo sul disegno. Ne è uscito un difetto minore, annotato fra le cose in
sospeso: fra un blocco di testo e il successivo le parole si **incollano**
(«Pubblica AmministrazioneDue suite specializzate»), perché il lettore concatena i nodi
senza separatore — il modello se l'è cavata lo stesso, ma per fortuna, non per progetto.
