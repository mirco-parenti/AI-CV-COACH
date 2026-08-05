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

## 7.2 Le tre uscite

| Uscita | Come funziona | Quando usarla |
|---|---|---|
| **File `.eml`** | il programma scrive un file email standard (formato MIME, con allegati incorporati), marcato come **bozza da inviare** (intestazione `X-Unsent`): i client che la riconoscono — Outlook in testa — lo aprono pronto per «Invia», non come messaggio ricevuto | il **percorso di riferimento**: l'utente rilegge nel suo client e invia da lì; se il client tratta il file come sola lettura, restano il copia-incolla o l'invio SMTP |
| **File `.msg`** | se sul PC è installato Outlook classico, il programma glielo fa generare (automazione COM) | per chi lavora in ambienti dove circola il formato Outlook |
| **Invio diretto SMTP** | l'app invia con l'account configurato nelle Impostazioni (server, porta, utente, password cifrata; connessione protetta STARTTLS/SSL) | quando l'utente si fida del giro completo e vuole fare tutto da qui |

- Il formato `.eml` è la scelta di default perché è **aperto e verificabile**: niente
  dipendenze, funziona con qualsiasi client. Il `.msg` è un formato proprietario: si
  ottiene in modo affidabile solo tramite Outlook, per questo è offerto **solo se
  Outlook c'è** (altrimenti il bottone spiega il perché e propone l'`.eml`).
- L'**invio diretto** è un'azione di livello 6 (cap. 03): bottone rosso, riepilogo
  finale («a chi, con che oggetto, con quali allegati») e conferma esplicita. Nessun
  invio parte mai in automatico.

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
generati, il mezzo di invio usato e il destinatario, l'esito.

**Cosa mostra il pannello Registro (in P1 Home):** l'elenco ordinabile e filtrabile
per stato e stelle, i contatori (inviate / in attesa / da completare / scartate) e le
candidature ferme da più giorni («inviata il 12/08, nessun esito registrato — vuoi
segnare un promemoria di follow-up?»).

Il registro è anche parte del racconto del progetto: per Mirco documenta nero su
bianco l'uso reale del prodotto nella sua ricerca di lavoro. Per questo
l'esportazione del registro in un riepilogo leggibile (CSV/markdown) è prevista fin
dal primo rilascio.

## 7.4 Sicurezza e buon senso

- La password SMTP è cifrata su disco con la protezione dati di Windows legata
  all'utente (cap. 11); non compare mai in chiaro né nei log.
- Il programma non manda **mai** email senza il passaggio di conferma; non esiste un
  «invia a tutti».
- Ogni invio riuscito o fallito viene annotato nel registro con data e ora; un errore
  SMTP mostra il messaggio del server tradotto in indicazioni pratiche («il server ha
  rifiutato la password: ricontrolla l'account nelle Impostazioni»).
