# 07 — Email di candidatura e registro

*L'ultimo miglio: trasformare CV e lettera in una candidatura partita davvero, e
tenerne il conto fino a esaurire le opportunità.*

## 7.1 La composizione dell'email

Quando i documenti di un'opportunità sono pronti (cap. 12, passo A8), il pannello
Email (P7) prepara la bozza:

- **Destinatario**: se l'annuncio conteneva un indirizzo, viene proposto; altrimenti il
  campo resta vuoto e l'utente lo inserisce (il programma non lo inventa mai).
- **Oggetto e corpo**: il prompt `email_candidatura` li ricava dalla ✉️ lettera già
  generata — l'email è la versione breve e diretta della lettera, nella stessa lingua,
  con il rimando esplicito agli allegati. Passa anche lei dalla rifinitura anti-slop
  (cap. 08).
- **Allegati**: proposti automaticamente — il CV generato (PDF di regola, DOCX a
  scelta), la lettera come allegato se l'utente la vuole separata dal corpo, più gli
  **attestati** pertinenti dalla cartella documenti (cap. 05.2), da spuntare.
- Tutto è modificabile a mano prima di procedere; l'anteprima mostra esattamente ciò
  che partirà.

### Com'è stata costruita (T6, 2026-08-14)

- **L'email non sta nella pipeline, e non è una dimenticanza.** La fila di T4 si percorre
  da sé fino ai documenti; da lì in poi decide l'utente — a chi, quali allegati, e se
  spedire. Un passo che aspetta delle scelte non appartiene a una fila automatica: il
  compositore vive accanto alla pipeline, non dentro.
- **La bozza si salva** (`email.json` nella cartella dell'opportunità): destinatario,
  oggetto, corpo e le spunte degli allegati. È l'unico punto della candidatura in cui
  l'utente **scrive davvero**, e riaprirla domani non deve voler dire ricominciare. Quando
  una bozza c'è, rientrando in P7 **l'AI non viene disturbata**: riscrivere sopra le
  correzioni fatte a mano sarebbe il modo peggiore di essere utili.
- **Delle spunte si fida, dei file no.** Riaprendo, l'elenco degli allegati si rifà dai
  file che ci sono *adesso* su disco e dalla bozza si riprendono solo le spunte: un
  documento cancellato nel frattempo non torna in vita perché un elenco lo nominava.
- **Il PDF si spunta da solo, il DOCX no.** È il formato che si apre uguale dappertutto ed
  è quello che si manda a un'azienda; il DOCX resta lì, spento, per gli annunci che lo
  chiedono espressamente.
- **Il `.eml` non si allega a sé stesso.** Alla seconda preparazione il messaggio scritto
  la prima volta è un file come gli altri nella `out\`: senza escluderlo finirebbe dentro
  la copia successiva, e ogni giro raddoppierebbe.
- **«L'ho spedita» è staccato dalla domanda che lo precede.** La conferma sta nel bottone,
  l'atto in un metodo suo: è anche l'unico modo di collaudarlo, perché una finestra di
  messaggio in un banco resta lì ad aspettare per sempre.
- **Gli attestati arrivano spenti, e lo dicono.** Gli allegati suggeriti dalla cartella
  documenti (cap. 05.2) compaiono in fondo all'elenco, marcati «(dai tuoi documenti)» e
  **senza spunta**: quali provino qualcosa *per quell'annuncio* lo sa l'utente, e mandare a
  un'azienda tutti i certificati che una persona possiede è il modo di non farne leggere
  nessuno. Si distinguono a vista dai documenti appena generati perché nella stessa lista
  convivono file nati un minuto fa e file che l'utente ha da anni. Quel che la
  classificazione ha messo in «altro» non compare affatto: una busta paga non deve poter
  finire in un'email per sbaglio, e il posto per correggere una categoria è la finestra di
  conferma, non l'elenco degli allegati.
- **Dell'elenco su disco ci si fida per le categorie, non per l'esistenza.** Gli attestati
  proposti sono solo quelli che **ci sono ancora** nella cartella: è la stessa regola con
  cui si rifanno i documenti della candidatura, applicata a file che vivono fuori da qui.

## 7.2 L'uscita: il file `.eml`

*Il capitolo prevedeva tre uscite. La revisione del 2026-08-05 (cap. 15, voci 8 e 9) ne
ha lasciata **una sola**; le ragioni sono qui sotto, perché sono istruttive.*

| Uscita | Come funziona |
|---|---|
| **File `.eml`** | il programma scrive un file email standard (formato MIME, con allegati incorporati) nella cartella dell'opportunità, marcato come **bozza da inviare** (intestazione `X-Unsent`): i client che la riconoscono — Outlook in testa — lo aprono pronto per «Invia», non come messaggio ricevuto. L'utente rilegge nel proprio programma di posta, dove è già autenticato, e spedisce da lì. |

Il formato `.eml` è **aperto e verificabile**: niente dipendenze, funziona con qualsiasi
client, e — questo è il punto che ha deciso il resto — **non chiede una password a
nessuno**.

### La prova sul campo (2026-08-14)

Il collaudo che il cap. 14 chiedeva a questa tappa — «un'`.eml` generata, aperta in un
programma di posta vero e spedita da lì, con allegati integri» — è stato fatto da Mirco
sui **dati veri**, e la promessa ha retto: il **nuovo Outlook** ha riconosciuto `X-Unsent`
e ha mostrato il messaggio come **bozza pronta**, con destinatario, oggetto e corpo; da lì
è partito, ed è arrivato con i suoi allegati.

Una cosa merita di restare scritta, perché la prossima volta farà risparmiare un'ora. Al
primo tentativo Outlook ha risposto: *«Non è stato possibile allegare i file seguenti…
Riprova più tardi»*. Sembrava un difetto del formato, e non lo era: il file, riletto con un
lettore di posta indipendente, aveva le intestazioni giuste e i due PDF **identici byte per
byte** agli originali. A mancare era la **sessione dell'account** dentro Outlook, scaduta —
il nuovo Outlook, per allegare a un messaggio importato, deve parlare col servizio, e senza
autenticazione quel caricamento fallisce con un messaggio che parla d'altro. Rifatto
l'accesso, gli allegati sono saliti e l'invio è andato. *Un errore del client che nomina i
nostri file si legge come un errore nostro: prima di toccare il formato conviene verificare
che il client sia in condizione di funzionare.*

**Perché il `.msg` è uscito.** Avrebbe prodotto esattamente lo stesso risultato:
l'`.eml` con `X-Unsent`, aperto in Outlook classico, mostra già la finestra di
composizione pronta con destinatario e allegati. In cambio sarebbe costato
l'automazione COM di Outlook, cioè la parte di Windows che si rompe più facilmente fra
versioni di Office, licenze e architetture a 32 o 64 bit. Il `.msg` avrebbe senso se il
file dovesse circolare o essere archiviato in un sistema che parla solo Outlook: qui è
una bozza privata che l'utente apre e spedisce in dieci secondi.

**Perché l'invio diretto SMTP è uscito.** Non è una scelta di gusto: l'invio con utente
e password si sta chiudendo ovunque. Microsoft ha portato al **100% dal 30 aprile 2026**
il rifiuto dell'autenticazione di base su SMTP — le «password per le app» hanno smesso
di funzionare e non si possono rigenerare — quindi ogni indirizzo `@outlook.it`,
`@hotmail.it`, `@live.it` o Microsoft 365 è già oggi inutilizzabile per questa strada.
Gmail regge ancora con le password per le app (a patto di avere la verifica in due
passaggi), ma Google ha annunciato di volerle eliminare a sua volta. Rientrare vorrebbe
dire adottare OAuth 2.0, che è un progetto a sé: registrazione dell'applicazione presso
ciascun provider, finestra di consenso nel browser, rinnovo periodico dei permessi.

Il risultato è che **nella 1.0 il programma non spedisce nulla**: prepara e consegna al
programma di posta dell'utente. Cadono con l'invio anche il pannello di configurazione
del server, la password di posta cifrata su disco, la traduzione degli errori SMTP e la
conferma rossa di livello 6 — che nella 1.0 non serve più qui.

## 7.3 Il registro delle candidature (F6)

Ogni opportunità porta con sé il proprio stato; il registro è la vista d'insieme che
risponde alla domanda «a che punto sono?».

**Stati di un'opportunità:**

```
nuova ──► interessante ──► generata ──► inviata ──► esito
  │              │                     (in attesa · colloquio · rifiutata ·
  └──► scartata ◄┘                      assunto 🎉)
```

**Cosa registra** (per ogni opportunità): azienda, titolo, fonte e link, lingua,
stelle del match (con l'eventuale ⛔), date di ogni passaggio di stato, i file
generati, il destinatario e l'esito. Poiché a spedire è il programma di posta
dell'utente, il passaggio allo stato «inviata» è una **conferma dell'utente**, non un
esito tecnico: dopo aver generato l'`.eml` l'app chiede «l'hai spedita?» e registra
data e ora della risposta.

**Cosa mostra il pannello Registro (in P1 Home):** l'elenco ordinabile e filtrabile
per stato e stelle, i contatori (inviate / in attesa / da completare / scartate) e le
candidature ferme da più giorni («inviata il 12/08, nessun esito registrato — vuoi
segnare un promemoria di follow-up?»).

Il registro è anche parte del racconto del progetto: per Mirco documenta nero su
bianco l'uso reale del prodotto nella sua ricerca di lavoro. Per questo
l'esportazione del registro in un riepilogo leggibile (CSV/markdown) è prevista fin
dal primo rilascio — **fatta il 2026-08-14**, v. più sotto.

### Com'è stato costruito (T5c, 2026-08-13)

- **Le cartelle-opportunità sono la fonte di verità; `registro.json` è un indice
  rigenerabile.** Un indice è comodo — apre la Home senza leggere venti cartelle — ma non
  è un secondo posto dove vive il dato: se manca, se non si legge o se non torna coi
  fatti su disco, si **ricostruisce** dalle cartelle e si riscrive. Chi guarda l'elenco è
  anche chi lo tiene in riga. La conseguenza vale la regola: una cartella copiata a mano
  compare nell'elenco da sola, e una cancellata sparisce senza lasciare una voce fantasma.
- **Le candidature nate prima deducono il proprio stato dai file che hanno.** Le cartelle
  scritte da T4 e T5b non hanno il campo `stato` — nasce qui — e riscrivere all'indietro i
  file dell'utente per aggiungercelo sarebbe un'invasione: se ci sono i documenti è
  *generata*, se ci sono i giudizi è *interessante*, altrimenti è *nuova*. Lo **scarto non
  si deduce**: è una decisione, e una decisione che nessuno ha scritto non c'è.
- **`inviata` ed `esito` esistono nello schema ma dall'interfaccia non si raggiungono**:
  sono di T6, con la conferma dell'utente descritta qui sopra. Stanno nello schema fin
  d'ora perché T6 aggiunga dei passaggi e non una migrazione dei file scritti fino a lì.
  Per la stessa ragione **il contatore delle inviate non c'è ancora**: fino a T6 sarebbe
  fermo a zero, e un contatore che non può muoversi non conta niente. Restano a T6 anche
  il promemoria di follow-up e, nella voce di registro, il destinatario e l'esito.
  *(Aggiornamento del 2026-08-14, a T6 chiusa: lo stato **`inviata` si raggiunge** — lo
  dichiara l'utente da P7 — e con lui è arrivato il **suo contatore**, insieme al filtro
  per stelle e all'esportazione dell'elenco, v. qui sotto. Restano fuori, e sono in
  `in_sospeso.md`, le tre cose che T6 non ha portato: lo stato **`esito`**, che nello
  schema c'è ma dall'interfaccia non si raggiunge, il **promemoria di follow-up**, e il
  **destinatario nella voce di registro** — oggi vive nella bozza `email.json` della
  candidatura, non nell'indice.)*
- **Chi cambia uno stato lo annota anche nell'indice** *(regola resa esplicita il
  2026-08-15, dopo un difetto)*. `registro.json` si fida di sé stesso finché **l'insieme
  delle cartelle** combacia: un cambiamento *dentro* una cartella non lo fa scattare, e
  rileggere lo stato di ogni cartella a ogni apertura vorrebbe dire rigenerare sempre —
  cioè buttare via il motivo per cui l'indice esiste. Perciò annota chi cambia: P4 quando
  scarta, P6 quando genera, **P7 quando segna «inviata»**. Quest'ultimo mancava, ed è
  saltato fuori al collaudo di tappa sui dati veri: la cartella diceva `inviata` e la Home
  continuava a mostrare «generata», col contatore fermo a zero. La verità era comunque sul
  disco — l'indice buttato si è ricostruito giusto — ma una promessa mantenuta solo dove
  l'utente non guarda non è mantenuta (cap. 12.7).
- **Lo scarto è terminale, e chiede conferma.** Da uno scarto non si torna indietro
  dall'interfaccia, ma la cartella **resta su disco**: si scarta, non si cancella — la
  conferma lo dice con parole sue, invece di un generico «sei sicuro?». Chi ci ripensa
  davvero ha ancora tutto. *(Ripescare uno scarto dall'interfaccia è in `idee_future.md`.)*
- **L'esportazione in CSV/markdown non è stata fatta qui** e sta in `in_sospeso.md`: è
  promessa per il primo rilascio, non per questa tappa, e a T5c avrebbe aggiunto una
  strada di uscita a dei dati che stavano ancora prendendo forma.

### L'esportazione dell'elenco (2026-08-14)

Il riepilogo promesso qui sopra, dalla Home: «Esporta l'elenco…» chiede dove scrivere e
in quale forma. **Non è il backup** (cap. 11.4): quello è JSON e serve a tornare
indietro, questo serve a raccontare — e va aperto da qualcun altro, che è ciò che
detta ogni scelta qui sotto.

- **Esce quel che si vede**, filtrato e ordinato come sullo schermo: chi ha appena
  ristretto la coda si aspetta quel foglio lì. Perché la vista non si scambi per
  l'archivio intero, il riepilogo leggibile porta in testa **cosa si sta guardando** —
  data, filtri in vigore, «3 su 12». Il CSV no: una frase in cima non è più una tabella.
- **Il CSV separa col punto e virgola** e comincia col segno d'ordine dei byte. Non è
  pignoleria: Excel apre un `.csv` col separatore di elenco della lingua di Windows — in
  italiano il punto e virgola — e senza quel segno legge l'UTF-8 con la tabella di
  sistema, così «perché» diventa «perchÃ©» in ogni riga. Un file che si apre storto non
  è un file esportato.
- **Le date escono in forma ISO** (`2026-08-14 09:30`): `14/08/2026` lo legge in un modo
  diverso ogni programma che lo apre, e per giunta l'ordine alfabetico dell'ISO è anche
  quello cronologico.
- **Il formato lo decide l'estensione del nome scelto**, non la voce del menù a tendina:
  chi scrive `riepilogo.md` a mano vuole un markdown, qualunque cosa dicesse il filtro
  quando ha cominciato a scrivere.
- **Le stelle mancanti restano vuote, mai zero**: zero stelle vuol dire «confrontata, e
  non vale niente», che è un'altra cosa da «non ancora confrontata».

### Il filtro per stelle (2026-08-14)

Il filtro promesso qui sopra è **per stato e stelle**: a T5c era arrivata solo la metà
sullo stato, e la coda si poteva sì ordinare per stelle ma non filtrare. Con sei
candidature l'ordinamento basta; con sessanta no, ed è quello lo scenario per cui il
registro esiste.

- La tendina dice **«almeno N ★»**, non «N ★»: la domanda di chi guarda una coda lunga è
  *quali valgono da 3 in su*, non *quali valgono esattamente 3*.
- I due filtri **si sommano** invece di sostituirsi: sono due domande diverse — a che
  punto sono, quanto valgono — e chi le fa entrambe si aspetta l'incrocio.
- Una candidatura **mai confrontata non ha stelle** e con un filtro attivo non passa: non
  è che valga poco, è che non lo sappiamo ancora. Perché quella sparizione non sembri una
  perdita, quando i filtri nascondono qualcosa i contatori aggiungono **«ne vedi 1 su 3»**
  — i numeri restano sul totale, che è la risposta alla domanda «a che punto sono»; la
  riga in più risponde a «e perché ne vedo meno».

## 7.4 Sicurezza e buon senso

- Il programma **non spedisce**: scrive un file e lo consegna al programma di posta
  dell'utente. Non esiste alcun «invia a tutti», e nemmeno un «invia».
- Di conseguenza il programma **non custodisce credenziali di posta**: l'unica
  credenziale che tocca resta la chiave dell'API (cap. 11.3).
- L'ultima parola è sempre di chi si candida: l'email parte quando lui preme «Invia»
  nel proprio programma di posta, dopo averla riletta.
- Nel registro finiscono data e destinatario di ogni candidatura confermata come
  spedita; il log tecnico non contiene mai il testo integrale dei documenti.
