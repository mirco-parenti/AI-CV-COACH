# 15 — Decisioni aperte

*Le scelte che spettano all'utente prima dell'implementazione. Ogni voce ha la
proposta di chi scrive, così la discussione parte da qualcosa. Quando una voce si
chiude, si sposta nel capitolo giusto e qui si spunta. Il piano (cap. 14) parte solo a
capitolo svuotato — o con le voci restanti dichiarate «rimandate» esplicitamente.*

## 15.1 Da decidere prima di T1 (fondamenta)

*Revisione definitiva del 2026-08-05: le voci sono state ripassate una per una alla
postazione del tutor; dove l'esito si discosta dalla proposta, la colonna dice perché.*

| # | Decisione | Proposta | Esito definitivo (2026-08-05) |
|---|---|---|---|
| 1 | **Versione .NET** | **.NET 8 LTS**, già collaudata dalla toolchain di casa; l'eventuale passaggio alla LTS successiva è un cambio di una riga, rivalutabile a T1. | ⚠️ **Cambiata: .NET 10 LTS**. Verificato sul sito Microsoft: **.NET 8 esce di supporto il 10/11/2026** (ed è già in sola manutenzione), mentre .NET 10 è LTS fino a **novembre 2028**. Comporta installare l'SDK 10 su entrambe le postazioni (oggi assente su quella del tutor, che ha l'SDK 9). |
| 2 | **Formato di rilascio** | **Autonomo** (self-contained, ≈150–180 MB): copi un file e funziona, senza «prima installa il runtime». La variante leggera resta per lo sviluppo. | ✔ **Confermata**: autonomo **non compresso** (avvio più rapido). Nota: il *trimming* non è supportato su WinForms, quindi la stima regge; e WebView2 (cap. 06) resta una dipendenza di sistema esterna, da dichiarare. *Misurato a T1 (2026-08-06): **116 MB** in un file solo, avvio in **~0,26 s** a freddo — sotto la stima.* |
| 3 | **Nome dell'eseguibile e dell'app** | `AiCvCoach.exe`, nome visuale «AI-CV-COACH». | ⚠️ **Cambiata: «TrovaLavoro»**, eseguibile `TrovaLavoro.exe`, sottotitolo «e candidati con il CV giusto, senza fatica». Criterio: il nome deve essere ricordabile e comprensibile per un utente non tecnico (modello *TrovaPrezzi*). **Il nome del progetto, del repo e dei documenti resta AI-CV-COACH**: cambia solo ciò che l'utente legge. Proprietà dell'eseguibile: azienda **Aviolab AI**, prodotto **TrovaLavoro**, copyright **© 2026 Aviolab AI**. *Il **sottotitolo** cambia il **2026-08-22**, insieme al banner del logo: «**Crea il tuo miglior CV e rispondi subito all'annuncio di lavoro perfetto per te!**» al posto di «e candidati con il CV giusto, senza fatica». Dice l'azione invece del beneficio ed è più lungo (76 caratteri contro 41): dove lo spazio è poco — l'anteprima di un link, una finestra stretta — va verificato che resti leggibile. Il **nome** non si tocca.* |
| 4 | **Logo del progetto** | serve un'immagine propria (quadrata, leggibile a 101×101 px). Da produrre (anche con Canva); finché non c'è, un segnaposto tipografico. | ✔ **Superata in T1** *(2026-08-06)*: il logo è lo **scudo di Aviolab AI**, incorporato **in forma binaria nel sorgente** (`LogoAviolab.vb`), con sotto sempre e solo la scritta «AVIOLAB AI»; il segnaposto «TL» non è mai arrivato al commit. Resta da produrre l'icona dell'exe. |
| 5 | **Schema di versione** | `1.0.012` (maggiore.minore.build) in `Versione.vb`; pool separato (`Pool 1.03`). | ✔ **Confermata** senza modifiche. |

## 15.2 Da decidere entro la tappa interessata

| # | Decisione | Tappa | Proposta | Esito definitivo (2026-08-05) |
|---|---|---|---|---|
| 6 | **Modelli AI concreti** | T2 | oggi: Haiku 4.5 (estrazione) e Sonnet 4.6 (ragionamento); si riverifica il listino al momento, i nomi stanno in configurazione. | ✔ **Decisa** (listino riverificato): **Haiku 4.5** (`claude-haiku-4-5`, $1/$5 per MTok) per l'estrazione, **Sonnet 5** (`claude-sonnet-5`, $3/$15 — promo $2/$10 fino al 31/08/2026) per il ragionamento. **Due avvertenze per T2**: Sonnet 5 attiva il ragionamento esteso *di default* se non lo si disabilita, e il suo nuovo conteggio token dà ~+30% a parità di testo → dimensionare il limite di risposta di conseguenza. I nomi restano in configurazione. *Attuata **in due tempi**. Primo tempo (2026-08-07): predefinito **Sonnet 4.6**, lo stesso del prototipo, perché la non-regressione si misura a parità di modello. Secondo tempo (**2026-08-18**): Sonnet 4.6 è passato fra i modelli superati del listino e **Sonnet 5 è il predefinito compilato**, con l'interruttore del ragionamento dichiarato **spento** — la prima delle due avvertenze, attuata. Il prezzo intanto è sceso: **$2/$10 per MTok**. Sul livello semplice non si è mosso nulla: **Haiku 4.5 è tuttora l'ultimo della sua fascia**, non esiste un Haiku successivo. La **seconda avvertenza**, il ~+30% di token, è stata a lungo l'ultima aperta: i `max_token` del pool erano cuciti su Sonnet 4.6 e nessuno li aveva rimisurati — il codice però un troncamento lo grida invece di subirlo (`ClientClaude`, causa `Troncata`). L'interruttore in `modelli.json` conserva i suoi tre stati (cap. 02.5). *Dal 2026-08-18 quell'avvertenza ha avuto il suo strumento (`chiamate_ai.csv`, cap. 02.5 e 04.4), e il **2026-08-19** la sua risposta: un giro d'uso completo, tredici chiamate dall'import del CV all'email, ha mostrato che il tetto più sollecitato è `email_candidatura` al **27,1%** del proprio, con ogni riga chiusa da `end_turn`. **Nessun tetto è stato alzato**, nessuna risposta troncata: l'avvertenza è chiusa non perché rimossa, ma perché misurata.* |
| 7 | **Portali del primo rilascio** | T5 | Indeed, LinkedIn Jobs, InfoJobs + ricerca generica; schema URL da verificare sul campo in T5. | ⚠️ **Cambiata: Indeed, InfoJobs, Subito.it** + ricerca generica. LinkedIn esce dal primo rilascio: è il portale meno adatto ai ruoli operativi che cerca il nostro utente, mentre Subito.it è il più riconosciuto da chi non è pratico. Resta aggiungibile in ogni momento come riga di `ricerche.json` (nessuna nuova build). Schemi URL da verificare sul campo a T5. *Verificati il 2026-08-12 (T5a), e la verifica è servita: **InfoJobs è chiuso** — la sua pagina dichiara la piattaforma «ufficialmente chiusa e non più disponibile» — e al suo posto entra **Jooble**, aggregatore che raccoglie anche le agenzie per il lavoro, provato sul campo con 407 offerte su «magazziniere, Genova». Indeed, Subito.it e la ricerca generica hanno risposto tutti. La terna del primo rilascio è quindi **Indeed, Jooble, Subito.it** + ricerca generica.* |
| 8 | **`.msg` sì/no** | T6 | tenerlo **solo se** su almeno un PC di riferimento c'è Outlook classico; altrimenti si rimanda (l'`.eml` copre il bisogno). | ⚠️ **Cambiata: fuori dalla 1.0** (→ 15.3). Motivo: l'`.eml` con intestazione `X-Unsent` apre in Outlook *già* la finestra di composizione pronta all'invio — identico risultato del `.msg` — che in cambio costerebbe l'automazione COM di Outlook, fragile fra versioni di Office e architetture. Outlook classico **è** presente sulla postazione del tutor (verificato), ma la condizione non basta a giustificare la funzione. |
| 9 | **Account SMTP di riferimento** | T6 | quale casella userà Mirco per le candidature (Gmail con password per le app? altro provider?). | ⚠️ **Cambiata: invio diretto SMTP fuori dalla 1.0** (→ 15.3). Fatto nuovo verificato: Microsoft ha chiuso l'autenticazione con password su SMTP — rifiuto al **100% dal 30/04/2026** — quindi ogni indirizzo `@outlook.it`/`@hotmail.it`/Microsoft 365 è già inutilizzabile; Gmail regge con le password per le app ma Google le sta eliminando. L'unica uscita della 1.0 è il file `.eml`, che non dipende da alcuna autenticazione. |
| 10 | **Soglia e pesi del match** | T2 | **restano fissi** (scelte di prodotto validate nel prototipo: soglia 1,5 stelle, pesi 5/1, clamp −20/+10, tetto 20). Non configurabili dall'utente. | ✔ **Confermata con una precisazione**: restano **fissi e invisibili all'utente** (nessun pannello: le stelle devono restare confrontabili fra annunci), ma vivono in un **file di taratura** nella cartella dati, non nel codice — così ritoccarli durante le prove non costa una nuova build. Include la regola del requisito eliminatorio (⛔ → massimo 1 stella). |

## 15.3 Dichiarate rimandate (non bloccano T0)

*Riviste una per una il 2026-08-05. Due voci hanno lasciato l'elenco — l'OCR perché
risolta, il profilo LinkedIn perché promosso nella 1.0 — e due vi sono entrate,
dalla 15.2.*

- **Trasporto HTTP locale per MCP** — stdio basta per i client di oggi.
- **Invio email e scrittura profilo via MCP** — richiedono un meccanismo di conferma;
  seconda versione (e l'invio email non è più nemmeno nella 1.0: vedi sotto).
- **Firma del codice** (certificato) — quando l'app circolerà oltre il portfolio.
  *Nota del 2026-08-05:* finché il programma resta sulle due postazioni di casa non
  serve; nel momento in cui qualcuno lo **scarica**, Windows mostra il blocco
  «Windows ha protetto il PC», che un utente non pratico legge come «è un virus».
  Costo: qualche centinaio di euro l'anno più un dispositivo fisico per la chiave.
- **Auto-update** — per un'app personale è complessità senza guadagno.
- **Terze lingue (fr, de…)** — il pool le ammette per costruzione; fuori perimetro.
- **Generazione assistita dell'email di sollecito (follow-up)** — nella 1.0 resta il
  **promemoria passivo** del registro (07.3): ricorda le candidature ferme, il testo del
  sollecito lo scrive l'utente. L'estensione è già disegnata (un prompt `email_sollecito`
  che riusa il pannello Email P7 e l'anti-slop); si aggiunge dopo la 1.0 se il bisogno
  si rivela frequente. *Il promemoria passivo è stato costruito a **T9c** (2026-08-21),
  con la soglia in giorni scelta dall'utente in P8: la metà rimandata resta quella
  assistita, e questa voce non cambia — cambia solo che adesso ha su cosa attaccarsi.*
- **Uscita `.msg`** *(dalla 15.2, voce 8)* — l'`.eml` con `X-Unsent` dà lo stesso
  risultato in Outlook senza automazione COM. Si riapre solo davanti a un caso d'uso
  che l'`.eml` non copra.
- **Invio diretto SMTP** *(dalla 15.2, voce 9)* — l'autenticazione con password su
  SMTP si sta chiudendo ovunque (Microsoft al 100% dal 30/04/2026; Google sta
  eliminando le password per le app). Rientrerà solo con OAuth 2.0, che è un progetto
  a sé: registrazione dell'applicazione presso il provider, finestra di consenso nel
  browser, rinnovo periodico dei permessi.

**Uscita dall'elenco — voce chiusa perché risolta:** l'**OCR locale per PDF
scannerizzati** non serve. Verificato sulla documentazione Anthropic: nel blocco
`document` **ogni pagina del PDF viene convertita in immagine** e letta con le
capacità visive del modello, che è esattamente il percorso già usato dal prototipo
(`POST /leggi-pdf`). Un curriculum scannerizzato, privo di strato di testo, viene
quindi già letto oggi. Resta il limite della **qualità della scansione**: se il testo
estratto è troppo povero, l'app lo dice e propone l'incolla-testo (cap. 05.1).

## 15.4 Domande aperte all'utente (senza proposta secca)

1. **Più profili nella stessa installazione** (es. un familiare che usa lo stesso PC):
   oggi il disegno è mono-profilo; il multi-profilo cambierebbe la cartella dati.
   Serve? → ✔ **Chiusa: mono-profilo, struttura dati piatta.** La 1.0 resta a profilo
   singolo e i dati vivono direttamente nella cartella dell'applicazione, senza livelli
   intermedi: struttura più leggibile. *Conseguenza accettata:* se un domani arrivasse
   il multi-profilo, i dati esistenti andranno spostati. Il multi-profilo resta
   estensione possibile, non pianificata.
2. **Il brainstorming va conservato per intero** (tutta la conversazione) o bastano
   gli appunti di mira confermati? → ✔ **Chiusa: solo gli appunti confermati.** La
   conversazione si chiude con la finestra. Oltre alla sobrietà dei dati, è la scelta
   più rispettosa: il brainstorming è la parte più personale di tutto il programma
   (dubbi, insicurezze, ragioni per cui si è lasciato un lavoro) e non deve sedimentare
   su disco senza che nessuno l'abbia chiesto.
3. **Pubblicazione dei valori concreti del design** (la palette e le dimensioni del
   cap. 03) → ✔ **Chiusa: restano pubblici.** Constatato che lo **erano già**: il
   repository è pubblico e il cap. 03, con tavolozza esadecimale, corpi dei caratteri e
   spaziature, è su `origin/main` dal commit `4644331`. La pubblicazione è coerente con
   la **regola 10** di `CLAUDE.md`, che colloca proprio in quel capitolo la
   specificazione del family feeling **come design proprio di AI-CV-COACH**.

*(Tutte le voci 15.1–15.4 hanno un esito **definitivo**. Nessuna resta in sospeso.)*

## 15.5 Sorte del backlog storico (`idee_future.md`)

| Voce del backlog | Sorte in questo progetto |
|---|---|
| Fonte-link / WebView2 | **dentro** (cap. 06) |
| Multi-annuncio | **dentro** (coda opportunità, cap. 06/07) |
| Editing campo-per-campo del profilo | **dentro** (cap. 12, A2) |
| `estraiFrammento` robusto lato client | **assorbita**: nel desktop c'è un solo estrattore, `EstrattoreJson` (cap. 02) |
| Limite dimensione PDF | **dentro** (cap. 05.1, messaggio chiaro) |
| Profilo da LinkedIn (2.1.3) | **dentro** *(promossa il 2026-08-05)*: il browser incorporato esiste già a T5 e la strutturazione (`importa_cv`) è indipendente dalla fonte — è quasi solo un pulsante in più. Si colloca **dopo T5**. ✔ **Fatta a T5d il 2026-08-14**, e la previsione ha retto: nessun componente nuovo, pool intatto. I pulsanti sono però **due** — l'atto in P3, la scelta in P2 — e la pagina va **scorsa** prima di leggerla, che era la sola cosa che il disegno non poteva sapere (cap. 06.7). |
| PDF scannerizzati / OCR | **assorbita** *(2026-08-05)*: già risolta dal blocco `document`, che converte ogni pagina in immagine (vedi 15.3). Nessun componente da scrivere. |
| Estensione profilo a specchio di `altri_requisiti`; domicilio confrontabile | **fuori**: resta nel backlog |
| `pending_questions`; collocazione manuale degli esclusi | **fuori**: resta nel backlog |
| Riordino dinamico / omissione mirata nel CV-2 | **fuori**: resta nel backlog (l'anti-invenzione «per sottrazione» merita una riflessione a sé) |
| Taxonomy mapping (ESCO/O*NET) | **fuori**: resta nel backlog |
| Decomposizione del prompt annuncio in sotto-prompt | **fuori** per ora: il pool la renderà naturale quando servirà |

## 15.6 Esito del cancello T0

Il capitolo è **svuotato**: le voci 1–13 hanno un esito definitivo, le restanti sono
dichiarate rimandate una per una in 15.3 con la loro motivazione. Secondo la regola del
capitolo 14, **il cancello T0 è chiuso e T1 può iniziare**.

Le decisioni che cambiano la proposta originale e vanno riportate negli altri capitoli:

| # | Cosa cambia | Capitoli da allineare |
|---|---|---|
| 1 | .NET 8 → **.NET 10 LTS** | 13, 14 |
| 3 | Nome per l'utente: **TrovaLavoro** (`TrovaLavoro.exe`); progetto e repo restano AI-CV-COACH; proprietà dell'eseguibile a **Aviolab AI** | 03, 13 |
| 6 | Sonnet 4.6 → **Sonnet 5** (con le due avvertenze su ragionamento e conteggio token) | 02, 04 |
| 7 | LinkedIn → **Subito.it** fra i portali del primo rilascio | 06, 14 |
| 8 | **`.msg` fuori** dalla 1.0 | 07, 14 |
| 9 | **Invio diretto SMTP fuori** dalla 1.0: cadono pannello server, password di posta cifrata e traduzione degli errori di invio (la protezione dati di Windows resta per la chiave API) | 02, 07, 11, 14 |
| 10 | Soglia e pesi in **file di taratura**, non nel codice | 02, 11 |
| — | **Profilo da LinkedIn dentro la 1.0**, dopo T5 | 06, 14 |

## 15.7 Riaperta dal collaudo dal vivo di T9e (2026-08-23)

*Il capitolo era stato dichiarato svuotato al cancello T0 (15.6), e da allora le decisioni
delle tappe si sono narrate nel cap. 14, dove nascono. Il collaudo dal vivo del quarto tempo
di T9e ne ha però portata alla luce una che non appartiene a un punto solo: riguarda il modo
in cui l'interfaccia dichiara le proprie misure, e va presa **prima** di curare i tre difetti
che l'hanno segnalata. La numerazione riprende da dove si era fermata: le voci **1–13** sono
quelle del cancello — le tabelle di 15.1 e 15.2 più le tre domande di 15.4 — e questa è la
**14**.*

| # | Decisione | Le due strade | Esito |
|---|---|---|---|
| 14 | **Come curare i difetti di scala a DPI alto**: il pannello del logo che a 150% sfonda nell'area viva, la finestra Impostazioni irraggiungibile per metà, il minimo della finestra principale sceso sotto la soglia di progetto (cap. 14, giro B) | **(a) puntuale** — si aggiusta ciascuno dei tre punti dove fa male. **(b) unità coerenti** — un modo unico di dichiarare le misure dell'interfaccia rispetto al DPI, di cui i tre punti diventano clienti: più lavoro, ma toglie la **specie** di errore invece dei tre esemplari. In entrambi i casi va deciso **come il banco potrà vedere questi difetti**: oggi gira a 96 DPI, dove i numeri sbagliati sono giusti, e nessuno dei 995 collaudi verdi li mostrava. | ⏳ **Aperta**: è il prossimo ragionamento con Mirco, non una decisione presa. |
