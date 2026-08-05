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

## 7.2 L'uscita: il file `.eml`

*Il capitolo prevedeva tre uscite. La revisione del 2026-08-05 (cap. 15, voci 8 e 9) ne
ha lasciata **una sola**; le ragioni sono qui sotto, perché sono istruttive.*

| Uscita | Come funziona |
|---|---|
| **File `.eml`** | il programma scrive un file email standard (formato MIME, con allegati incorporati) nella cartella dell'opportunità, marcato come **bozza da inviare** (intestazione `X-Unsent`): i client che la riconoscono — Outlook in testa — lo aprono pronto per «Invia», non come messaggio ricevuto. L'utente rilegge nel proprio programma di posta, dove è già autenticato, e spedisce da lì. |

Il formato `.eml` è **aperto e verificabile**: niente dipendenze, funziona con qualsiasi
client, e — questo è il punto che ha deciso il resto — **non chiede una password a
nessuno**.

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
dal primo rilascio.

## 7.4 Sicurezza e buon senso

- Il programma **non spedisce**: scrive un file e lo consegna al programma di posta
  dell'utente. Non esiste alcun «invia a tutti», e nemmeno un «invia».
- Di conseguenza il programma **non custodisce credenziali di posta**: l'unica
  credenziale che tocca resta la chiave dell'API (cap. 11.3).
- L'ultima parola è sempre di chi si candida: l'email parte quando lui preme «Invia»
  nel proprio programma di posta, dopo averla riletta.
- Nel registro finiscono data e destinatario di ogni candidatura confermata come
  spedita; il log tecnico non contiene mai il testo integrale dei documenti.
