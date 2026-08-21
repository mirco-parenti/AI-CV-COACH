# 12 — Flussi utente

*Come si usa l'applicazione, passo per passo. I pannelli citati (P1, P2, …) sono
definiti nel capitolo 03; le funzioni (F1–F8) nel capitolo 01. Questo capitolo è la
«sceneggiatura» dell'applicazione: ogni flusso qui descritto deve poter essere
percorso senza uscire dal programma.*

## 12.1 Flusso A — Il percorso tipico (dalla persona alle candidature inviate)

È l'uso «normale» dichiarato nel mandato: si parte da un CV e si finisce quando le
opportunità sono esaurite.

**A1. Primo avvio e setup**
1. L'utente avvia l'exe. Al primo avvio l'app chiede due sole cose: cartella dati
   (proposta: `%APPDATA%\TrovaLavoro`) e chiave API Anthropic (salvata cifrata,
   cap. 11). *Nessun account di posta da configurare:* l'app non spedisce (cap. 07).
   *A T6 (2026-08-14) di quelle due domande ne esiste **una**, la chiave: compare in una
   finestra prima di ogni altra cosa, e «Non adesso» è una risposta — si entra lo stesso,
   con le sole funzioni dell'AI spente (cap. 11.3). La **cartella dati** non si chiede:
   vale quella predefinita, e per lavorare altrove c'è `--dati` alla riga di comando
   (cap. 11.1). Chiederla al primo avvio a chi non sa ancora cosa sia il programma
   sarebbe stato un ostacolo prima del primo vantaggio; quando arriveranno le
   Impostazioni (T9) sarà lì che si cambia.* *A **T9b** (2026-08-21) le Impostazioni sono
   arrivate e la cartella dati **non si cambia** nemmeno lì: la si vede e la si apre, ma
   spostarla resta un mestiere dell'avvio, perché il lucchetto è preso all'apertura e per
   tutta la sessione (cap. 09.4). `--dati` non è più un ripiego provvisorio: è la strada.*
2. L'app carica la libreria prompt — quella integrata nell'exe o, se presente, la
   cartella `prompt-pool/` accanto all'exe (cap. 04.2) — e ne mostra versione e
   sorgente nel riquadro del logo, in basso a sinistra.

**A2. Il profilo (F1)**
1. Bivio: **«Ho già un CV»** oppure **«Costruiamolo insieme»** (Flusso B).
2. Con «Ho già un CV»: l'utente indica **un file** (PDF, TXT, MD, DOCX) **o una
   cartella**. Se è una cartella, l'app la scandisce, riconosce da sola i documenti
   utili (cap. 05) e propone: «ho trovato questi: questo sembra il CV più recente,
   questi sembrano attestati». L'utente conferma.
   *Dal 2026-08-07 la scelta della cartella è rimandata a **T6***, dove la scansione
   della cartella documenti serve comunque agli allegati dell'email (cap. 05.2): T3 ha
   chiuso la strada del file singolo, che è quella che porta al profilo.
   *E a T6 (2026-08-14) è arrivata **metà**: la cartella si sceglie, si legge e si
   conferma, ma da lì escono gli **attestati per l'email**, non il profilo. Il CV più
   recente la classificazione lo indica già — è scritto in `documenti.json` — e a non
   leggerlo è l'import, che continua a chiedere un file singolo. Questo punto 2 resta
   quindi una promessa a metà, annotata in `in_sospeso.md`.*
3. *Da T5d (2026-08-14) c'è una **terza porta**, e il bivio del punto 1 diventa un
   trivio: il CV può non essere un file.* Se il proprio percorso è già scritto in una
   pagina — di norma la **propria** pagina LinkedIn — la si apre nel browser integrato e
   si preme «Importa CV da questa pagina» (cap. 06.7). Ci si arriva anche partendo dal
   profilo: **«IMPORTA CV DA LINKEDIN»** in P2 porta al browser e il pannello, arrivando,
   dice cosa fare. Da qui in avanti i passi sono gli stessi degli altri due.
   *(Quel bottone si chiamava «Importa CV da un sito…» quando è nato; ribattezzato il
   2026-08-14, cap. 03.6. Il nome nomina il caso d'uso tipico, ma la porta non è ristretta
   a LinkedIn: di là si legge qualunque pagina che racconti un percorso.)*
4. Il CV scelto viene trascritto — se è un PDF; una pagina è già testo — e strutturato
   nel **profilo JSON** (stessi due passi del prototipo: trascrizione fedele +
   strutturazione anti-invenzione).
5. Il profilo viene mostrato **campo per campo, modificabile** (novità rispetto al
   prototipo: si corregge la singola voce, non si ricomincia). Alla conferma, il
   profilo diventa la **fonte di verità unica**.

**A3. Le preferenze di ricerca (F2)**
1. L'utente descrive che lavoro cerca: tipologie di ruolo, zona geografica,
   contratto, parole chiave, portali preferiti.
2. Le preferenze diventano **ricerche salvate** (una per portale), riutilizzabili.

**A4. La raccolta degli annunci (F2)**
1. L'utente apre il pannello di ricerca: il browser integrato (WebView2) carica il
   portale con la ricerca salvata; l'utente naviga e, se serve, fa login come sé
   stesso.
2. Quando un annuncio interessa, preme **«Cattura annuncio»**: l'app legge il testo
   della pagina visualizzata, lo passa all'analisi (F3) e mette l'annuncio nella
   **coda delle opportunità** con titolo, azienda, fonte e link.
3. Si ripete finché si vuole: la coda può contenere quante opportunità si desidera.

**A5. Valutazione e scelta (F3)**
1. Per ogni opportunità in coda l'app mostra la scheda: estratto dell'annuncio,
   **match in stelle 0–5**, requisiti con esito (✓ soddisfatto, ~ in parte,
   ✗ non soddisfatto, ? non determinabile), eventuale **⛔ eliminatorio** che ha
   messo il tetto al punteggio (≤ 1 stella), lettura d'insieme.
2. L'utente ordina/filtra la coda per stelle e decide, opportunità per opportunità:
   **«Mi interessa»**, **«Scarto»**, **«Più tardi»**.
   *Costruito a T5c (2026-08-13), delle tre scelte ne è rimasta **una sola esplicita**.
   «Mi interessa» non è un bottone: un'opportunità confrontata **è** già interessante — lo
   stato lo scrive la pipeline quando i giudizi esistono, e chiedere all'utente di
   ribadirlo sarebbe stato un clic per confermare quel che aveva appena fatto. «Più tardi»
   non è un bottone per la ragione opposta: rimandare vuol dire **non fare niente**, e la
   candidatura resta dov'è, in coda. Resta esplicito **«Scarta»**, che è l'unica delle tre
   a dire qualcosa che dai fatti non si deduce (cap. 07.3).*
3. Sotto 1,5 stelle l'app **sconsiglia** la candidatura (senza impedirla), come nel
   prototipo.

**A6. Brainstorming e messa a punto (F3)**
1. Sulle opportunità marcate «Mi interessa» si apre il pannello di ragionamento (P5):
   una conversazione libera con l'AI, in italiano, con la risposta che compare in
   streaming.
2. La conversazione è **ancorata**: a ogni turno il programma invia, insieme alla
   chiacchierata, profilo + annuncio + giudizi (prompt `brainstorm` del pool). Si
   discute di punti di forza da mettere davanti, gap e come mitigarli onestamente,
   dubbi dell'utente («ho senso io per questo ruolo?»).
3. Quando l'utente è soddisfatto preme **«Trasforma in appunti»**: il prompt
   `appunti_di_mira` distilla dalla conversazione pochi punti operativi (cosa
   enfatizzare, quale mitigazione usare, tono della lettera), mostrati in una scheda
   di conferma modificabile.
4. Gli **appunti di mira** confermati si salvano con l'opportunità e verranno passati
   alla generazione; la conversazione integrale invece non si conserva (v. cap. 15.4).
   I fatti restano quelli del profilo: gli appunti orientano, non aggiungono.

*Costruito a **T7c** (2026-08-18), con quattro precisazioni che il flusso qui sopra non
aveva.* **Al punto 1**: non ci sono opportunità «marcate»: il ragionamento si apre su una
candidatura **confrontata** (il bottone di P4 si accende lì), che è la stessa scelta fatta a
T5c quando dei tre bottoni previsti ne è rimasto uno solo. E ad aprire la conversazione è
**l'AI**, con due o tre appigli concreti e il nodo che pesa di più: una chat che si apre
vuota non dice a nessuno da dove cominciare. **Al punto 2**: le fonti sono **quattro**, non
tre — ci sono anche le **mitigazioni**, che a quel punto della pipeline esistono già (cap.
04.3). E il contesto viaggia **una volta sola**, nel primo messaggio: a ripetersi sono i
turni, non gli artefatti. **Al punto 3**: gli appunti sono **al massimo sei**, tipizzati
(`enfasi` · `mitigazione` · `tono` · `evitare`), e ognuno porta la frase da cui nasce, così
nella scheda si riconosce cosa si sta confermando; la scheda li fa spuntare, riscrivere o
togliere uno per uno. **Al punto 4**, la cosa che il flusso non poteva prevedere: parlando,
l'utente dichiara cose che nel profilo **non ci sono**. Non entrano nei documenti — sarebbe
il profilo scavalcato dalla porta di servizio — e non spariscono: finiscono in un elenco
**a parte**, mostrato con la ragione («se è vero, aggiungilo al profilo e sarà tuo per tutte
le prossime candidature»). È l'anti-perdita del campo `altrove`, spostata dalla costruzione
del profilo alla conversazione su una candidatura.

**A7. Generazione dei documenti (F4)**
1. L'app propone la **lingua** dell'output (rilevata dall'annuncio, modificabile).
2. Genera 🎯 CV-2 mirato e ✉️ lettera (con mitigazioni, se esistono), li mostra
   affiancati all'annuncio per il controllo.
3. La prosa (sommario, descrizioni, lettera) passa la **rifinitura anti-slop** (cap. 08);
   l'utente vede il prima/dopo e può intervenire a mano su ogni campo. *A T7b
   (2026-08-18) la rifinitura c'è e avviene dentro la generazione, subito dopo ogni
   documento — i passi diventano sei — e il prima/dopo si guarda spuntando la casella in
   P6; l'intervento **a mano** campo per campo è rimandato (`in_sospeso.md`).*
4. Alla conferma, l'app produce i file: **DOCX e/o PDF** del CV, testo della lettera
   (nel corpo email e/o come allegato).

**A8. La candidatura (F5 + F6)**
1. L'app compone l'email: destinatario (se noto dall'annuncio), oggetto, corpo
   (lettera o suo adattamento breve), allegati scelti (CV generato + eventuali
   attestati dalla cartella documenti).
2. L'utente rilegge l'anteprima e preme **«Prepara l'email»**: l'app scrive il file
   `.eml` nella cartella dell'opportunità e lo apre nel programma di posta predefinito,
   che lo mostra come bozza già compilata, allegati compresi. L'ultima parola — il
   pulsante «Invia» — è nel suo programma di posta.
3. Al ritorno nell'app, una domanda sola: «l'hai spedita?». Alla conferma l'opportunità
   passa allo stato **«inviata»** con data e ora, e il registro si aggiorna.

*Com'è venuto (T6, 2026-08-14).* Il punto 1 è vero a metà, e la metà mancante è
dichiarata: il **destinatario resta vuoto** anche quando l'annuncio ne conterrebbe uno,
perché l'analisi non estrae recapiti (sta in `in_sospeso.md`). *Metà colmata a T7a
(2026-08-15): `analisi_annuncio` estrae il **contatto** e P7 lo propone — nella casella,
modificabile, e solo al primo arrivo — dicendo da dove viene. Resta vero il resto della
promessa: se l'annuncio un indirizzo non lo scrive, il campo resta vuoto, perché non si
deduce da un dominio né dal nome di chi firma (cap. 07.1). Provato dal vivo il 2026-08-18
su un annuncio che l'indirizzo ce l'aveva.* Gli **attestati della
cartella documenti** ci sono, e arrivano spenti (cap. 07.1). Prima del punto 1 c'è un
passo che il flusso non prevedeva: la **cartella documenti va indicata una volta**, dal
bottone «Documenti da allegare…» di P7 — è configurazione, e quando ci saranno le
Impostazioni sarà lì che si sposterà (cap. 03.6). *A **T9b** (2026-08-21) si è spostata a
metà, di proposito: le Impostazioni **dicono** quale cartella è in uso e quanti attestati
ci si sono riconosciuti, ma a gestirla mandano in P7 — quel giro chiama l'AI per
classificare i file e la sa aspettare e annullare, e rifarlo in una finestra di
configurazione avrebbe significato riscriverne il ciclo.* Il punto 2 è come scritto; il punto 3
pure, con una precisazione che vale la pena: «l'hai spedita?» è **spento** finché il
messaggio non è stato preparato, perché un `.eml` che non esiste non può essere partito.

**A9. Fino a esaurimento**
1. Si torna ad A5 per la prossima opportunità in coda; quando la coda è vuota, si
   torna ad A4 (nuova raccolta) o si chiude.
2. Il registro (F6) mostra in ogni momento: inviate, in attesa, da fare, scartate.

## 12.2 Flusso B — Costruire il CV con il dialogo guidato (F1)

Per chi un CV non ce l'ha, o lo vuole rifare da zero. È l'anello 1 del prototipo,
portato su desktop:

1. La conversazione procede **un argomento per turno**: nome → contatti → patente →
   esperienze lavorative → esperienze informali → competenze → formazione.
2. Ogni risposta viene strutturata dall'AI e mostrata in una **scheda di conferma**
   («ho capito questo: giusto?») prima di entrare nel profilo.
3. Ciò che l'utente dice «nel turno sbagliato» non si perde: viene parcheggiato e
   riproposto al turno giusto (convenzione anti-perdita, campo `altrove`); ciò che non
   trova posto viene dichiarato «lasciato fuori», mai perso in silenzio.
4. Alla fine: riepilogo leggibile e conferma. Il dialogo **non scrive su disco**: porta
   il profilo raccolto nella scheda P2, dove l'utente lo rivede campo per campo e lo
   salva lui (decisione di T3c, 2026-08-07 — a salvare è sempre la stessa mano, e la
   scheda resta l'unico posto da cui il profilo entra nell'archivio). *Dal 2026-08-09
   (revisione adversariale) questa porta unica ha anche la sua rete: un dialogo finito
   ma **non ancora consegnato** a P2 viene dichiarato se si prova a chiudere l'app —
   prima si perdeva in silenzio, ed era il paradosso peggiore: a metà racconto l'avviso
   c'era, a racconto completo no.*
5. Da lì si può già generare il 📄 CV-1 base (senza alcun annuncio) o proseguire con il
   Flusso A dal punto A3.
   *(T7d, 2026-08-18: dalla seconda volta in poi quel bottone non genera più — porta in P6
   e **mostra** il CV-1 base che c'è, con la sua lingua e la data in cui è nato. A rifarlo
   c'è «Rigenera», e a scriverlo in un'altra lingua la tendina, che lo chiede prima.)*

## 12.3 Flusso C — Annuncio da link (F2)

Quando l'utente ha già in mano un link interessante (da un'email, da un amico, da un
altro dispositivo):

1. Incolla il link nel pannello ricerca; il browser integrato apre la pagina;
   l'utente fa l'eventuale login e arriva a vedere l'annuncio.
2. **«Cattura annuncio»** e da lì il flusso è identico ad A5 → A8.
3. In alternativa resta sempre il ripiego del prototipo: incollare direttamente il
   **testo** dell'annuncio. *«Ripiego» va letto bene, e T4 lo chiarisce (2026-08-10):
   fino a T5 l'incolla-testo non è un ripiego ma **la** strada — il browser integrato
   arriva dopo — e anche da T5 in poi resta di prima classe, perché un annuncio arriva
   spesso da un'email o da uno screenshot letto altrove, dove non c'è pagina da
   catturare. Il posto in cui si incolla è la fascia in cima a P4 (cap. 03.6).*

## 12.4 Flusso D — Aggiornamento periodico del profilo (F1)

Il profilo invecchia: nuove esperienze, corsi, competenze. Ogni tanto (o quando
l'utente lo chiede) l'app propone una **sessione di aggiornamento**:

1. «Dall'ultima volta (data): è cambiato qualcosa? Nuovo lavoro, corso, attività?»
2. La conversazione usa gli stessi turni del dialogo guidato, ma **in modalità
   differenziale**: si aggiunge o si corregge, non si ricomincia.
3. Ogni modifica passa dalla solita scheda di conferma; il profilo aggiornato viene
   salvato e **versionato** (si conserva lo storico: cap. 11), così i CV già inviati
   restano riconducibili al profilo con cui furono generati.

## 12.5 Flusso E — Backup e ripristino (F7)

1. **Esporta**: l'utente sceglie file e contenuto — solo profilo (con storico),
   oppure profilo + registro + opportunità (cap. 11.4). L'app scrive un **JSON
   leggibile** con versione dello schema.
2. **Importa**: da un file di backup l'app ricostruisce profilo (e storico, se
   presente) mostrando **prima** cosa sta per sovrascrivere e chiedendo conferma.

## 12.6 Flusso F — L'app come server MCP (F8)

Per l'utente evoluto che lavora con Claude Desktop / Claude Code:

1. L'app (stesso exe, avviato in modalità server: cap. 09) espone come **tool MCP** le
   funzioni del motore: leggere il profilo, analizzare un annuncio incollato,
   confrontare, generare CV e lettera, esportare i file, leggere il registro.
2. Il client AI esterno può così orchestrare i flussi («confronta il mio profilo con
   questi 3 annunci e dimmi dove candidarmi prima») usando **la stessa logica e gli
   stessi prompt** dell'app, con i dati che restano sul PC dell'utente.
3. Le azioni che modificano il profilo **non sono esposte** nella prima versione del
   server MCP: restano nell'app, dove c'è la conferma visiva dell'utente (cap. 09.3).

*Costruito a **T8** (2026-08-19), e il flusso qui sopra è rimasto quello che è: dodici tool
— tre che leggono, sette che passano dall'AI, due che scrivono; **tredici da T9a**, quando ai
secondi si è aggiunto `esporta_backup` (cap. 11.4) — sugli stessi prompt e sugli
stessi mestieri dei pannelli. Tre precisazioni che la sceneggiatura non poteva contenere. La
prima: il punto 2 immagina un client che orchestra più annunci di fila, e perché sia
possibile senza diventare sordo il server serve **più richieste insieme** (cap. 09.2). La
seconda: dal momento in cui una porta scrive, i dati hanno due scrittori, ed è nato il
**lucchetto** del cap. 09.4 — se l'applicazione è aperta, i tool che scrivono rifiutano e
dicono cosa fare, mentre lettura e generazione continuano. La terza: quel che esce da qui è
**identico** a quel che esce dalla finestra, rifinitura anti-slop compresa, e i documenti
escono in DOCX, in PDF o in tutti e due. Il pezzo che nessun collaudo automatico poteva
dare — percorrere il flusso con un **client MCP vero** — è stato fatto il **2026-08-21**,
e non da Claude Desktop, che su questa postazione non c'è, ma da **Claude Code**:
dodici tool visti da un client che non abbiamo scritto noi, insieme alle **istruzioni del
server** (cap. 09.2); il lucchetto provato fra **due processi veri**, con l'applicazione
aperta che fa rifiutare la scrittura e la stessa chiamata che riesce a finestra chiusa; e
la generazione uscita con lo **scheletro dei fatti identico** a quello della finestra. Il
racconto sta nel cap. 14 (T8); i due limiti che il collaudo ha scoperto sono in
`in_sospeso.md`.*

## 12.7 Regole trasversali dei flussi

- **Mai un vicolo cieco**: da ogni pannello si torna indietro senza perdere ciò che è
  stato fatto; le operazioni lunghe (chiamate AI) mostrano attesa e sono annullabili
  (con l'eccezione dichiarata del turno di dialogo, cap. 02.6).
  *Dal 2026-08-18 «senza perdere ciò che è stato fatto» vale anche quando a farti uscire
  è la **barra in cima**, che non passa dal pannello: da lì la bozza dell'email spariva in
  silenzio, ed era un vicolo cieco travestito da scorciatoia (cap. 03.8).*
- **La chiusura dichiara ciò che perderebbe** *(2026-08-09, revisione adversariale)*:
  chiudere l'app con un lavoro a metà — un dialogo non consegnato, un import in volo —
  non è vietato, ma non è mai silenzioso: l'app lo dice, e l'import in corso viene
  annullato in modo pulito.
- **Conferma prima di scrivere**: nessun dato entra nel profilo, nessun file viene
  scritto, nessuna email parte senza un passaggio esplicito di conferma.
- **Tutto riapribile**: ogni opportunità conserva annuncio, giudizi, documenti
  generati e stato; si può riprendere domani da dove si era rimasti. *Dal 2026-08-10 (T4)
  questo era vero **sul disco** ma non a video: le cartelle c'erano e l'interfaccia non
  sapeva tornarci. **Da T5c (2026-08-13) la promessa è mantenuta per intero**: la coda
  della Home riapre qualunque candidatura, comprese quelle scritte prima che la Home
  esistesse. Una promessa mantenuta solo dove l'utente non guarda non è mantenuta.*
  *Con T6 (2026-08-14) entra nell'elenco anche la **bozza dell'email** (`email.json`):
  destinatario, oggetto, corpo e spunte degli allegati. È l'unico punto di una candidatura
  in cui l'utente scrive parole sue, quindi è anche quello in cui «riprendere da dove si
  era rimasti» conta di più — e rientrando l'AI non riscrive sopra: propone solo quando
  non c'è ancora niente.* *Con T7b (2026-08-18) si aggiunge il rovescio della medaglia:
  riprendere una cosa che nel frattempo **non vale più** è una forma di vicolo cieco. Se i
  documenti sono stati rigenerati in un'altra lingua, la bozza si riprende lo stesso — è
  lavoro dell'utente — ma il pannello **dice** che è rimasta indietro, invece di mostrarla
  come se fosse quella giusta (cap. 07.1).*
- **Onestà visibile**: stelle, ⛔ eliminatori e avvisi di soglia non si nascondono mai;
  sono il valore del prodotto, non un fastidio da minimizzare.
