# TrovaLavoro — guida per chi lo usa

*Questa è la guida di **chi usa** il programma. Non serve saper programmare per leggerla, e
non racconta com'è fatto dentro: quello sta nel `README.md` e nei capitoli di
`VB.NET/progetto/`, che sono scritti per chi costruisce.*

---

## Che cosa fa

TrovaLavoro prepara candidature. Gli racconti **una volta** chi sei; poi, per ogni annuncio
che gli dai, lo confronta col tuo profilo, ti dà un punteggio in stelle da 0 a 5 e ti spiega
dove sei forte e dove no. Se decidi di candidarti, scrive **un CV e una lettera su misura per
quell'annuncio**, li salva in DOCX e PDF, e prepara l'email con gli allegati già attaccati.

Una cosa che non fa mai, ed è la regola da cui nasce tutto il resto: **non inventa**. Se per
quell'annuncio ti manca qualcosa, te lo dice invece di riempire il vuoto con una frase che
suona bene.

---

## Che cosa serve

| Cosa | Perché |
|---|---|
| **Windows 11 a 64 bit** | il programma è compilato per questo, e non gira su altro |
| **Un file solo: `TrovaLavoro.exe`** | non c'è niente da installare, nessuna DLL da mettere accanto, nessun diritto di amministratore |
| **Una connessione a Internet** | serve per l'AI e per cercare annunci; senza, il programma si apre lo stesso e ti dice cosa non può fare |
| **Una chiave API di Anthropic** | è la sola cosa da procurarsi, e si paga a consumo (v. sotto) |

**Non** servono: Microsoft Word (i documenti li produce da sé), Adobe Acrobat, un account su
questo programma, o una registrazione da qualche parte.

Il PDF viene stampato dal motore **WebView2**, che in Windows 11 c'è già ed è aggiornato dal
sistema. Se su un PC particolare mancasse, il programma non muore: te lo dice all'avvio, ti dà
il link ufficiale di Microsoft per installarlo, e tutto quello che non lo usa continua a
funzionare.

---

## Come si comincia

1. **Scarica `TrovaLavoro.exe`** e mettilo dove vuoi: sul desktop, in una cartella tua,
   anche su una chiavetta. Non sparpaglia file: i tuoi dati vanno altrove (v. *Dove
   finiscono i tuoi dati*).
2. **Al primo doppio clic Windows può avvisarti** che «il PC è protetto» e che
   l'applicazione non è riconosciuta. Succede a ogni programma non firmato con un
   certificato commerciale. Per procedere: *Ulteriori informazioni* → *Esegui comunque*.
3. **Compare la schermata di avvio** — il marchio del programma — e resta qualche secondo,
   il tempo di leggerla, mentre TrovaLavoro si prepara. Non c'è niente da fare: se c'è
   qualcosa da chiederti se ne va da sola, senza farti aspettare.
4. **Si apre l'informativa**: che cosa esce dal tuo PC, che cosa no, dove restano i dati,
   che spesa comporta. Compare **una volta sola**, ed è il momento giusto per leggerla:
   subito dopo il programma ti chiede la chiave. La ritrovi quando vuoi da *Informazioni* o
   dalle *Impostazioni*.
5. **Incolli la chiave API** e premi «Prova la chiave»: in due secondi sai se funziona,
   senza consumare niente. Se non ce l'hai ancora, il paragrafo dopo dice come si prende.
6. **Racconti il tuo profilo.** Due strade, e la seconda è più veloce: rispondere alle
   domande del programma, oppure dargli un **CV che hai già** (anche in PDF) e lasciare che
   lo legga. Poi controlli quel che ha capito e correggi.

Da lì in avanti la barra in alto è tutto il programma:

| | |
|---|---|
| 🎮 **Menu** | la pagina d'ingresso: le sei cose che il programma sa fare, una per bottone |
| 🏠 **Le mie candidature** | a che punto sei, e cosa conviene fare adesso |
| 👤 **Profilo** | chi sei: si aggiorna quando vuoi, e ogni versione resta nello storico |
| 🔍 **Ricerca** | trova annunci sui portali, o incolla il testo di uno che hai già |
| **Confronta ⭐ ANNUNCIO - CV** | il confronto, le stelle, e la decisione se candidarsi |
| 📄 **Documenti** | CV, lettera ed email: si rileggono, si correggono a mano, si salvano |
| ⚙ **Impostazioni** | la chiave, le preferenze, il backup, quanto hai speso |

Il **Menu** e la barra portano nelle stesse stanze: la barra è sempre lì mentre lavori, il menu
è la pagina da cui si parte quando non sai ancora dove andare.

Dal **Profilo** esce anche un **CV base senza AI**: il tuo profilo messo nella forma di un
curriculum, subito e **senza spendere niente**. Non prende il posto del CV che scrive l'AI, gli
sta accanto — lì il sommario e le descrizioni delle esperienze sono scritti, qui restano le tue
parole com'erano — e la differenza fra i due è esattamente quello che ci ha messo l'AI.

---

## La chiave API: dove si prende e quanto costa

Il programma **non ha un abbonamento suo**. Usa la tua chiave di **Anthropic**, e quello che
consumi lo paghi tu a loro, a consumo, in dollari.

**Come si ottiene:**

1. crea un account su `console.anthropic.com`;
2. carica un credito (si può cominciare con pochi dollari);
3. crea una chiave API e copiala — è una stringa che comincia con `sk-ant-`;
4. incollala in TrovaLavoro, alla prima apertura o in *Impostazioni → Cambia la chiave*.

La chiave resta **sul tuo computer**, cifrata con la protezione dati di Windows: è legata al
tuo account, e chi aprisse quel file da un altro utente o da un altro PC non ne caverebbe
niente. Il programma non te la rimostra mai per intero — solo le ultime lettere, quanto basta
a riconoscerla.

**Quanto costa usarlo.** Dipende da quanto sono lunghi i tuoi testi: un profilo ricco e un
annuncio di tre pagine costano più di un profilo asciutto e di un annuncio breve. Invece di
darti una cifra a naso, il programma **tiene il conto**: in *Impostazioni → Quanto è costato*
trovi quante chiamate sono partite, quanti token sono passati e una stima in dollari, in
totale e negli ultimi trenta giorni. È una stima ai prezzi di listino: la verità resta la
fattura di Anthropic.

Due dettagli che spiegano perché costa poco: il programma usa **due modelli diversi** — uno
economico per i lavori meccanici (leggere un annuncio, mettere in ordine quel che dici) e uno
più capace solo dove serve ragionare (il confronto, la scrittura dei documenti) — e li puoi
cambiare tu da *Impostazioni → Sotto il cofano*, scegliendoli da un elenco che il programma
chiede all'AI.

---

## Dove finiscono i tuoi dati

Tutto in **una cartella tua**, che le Impostazioni ti aprono con un clic
(*Impostazioni → Apri la cartella dati*). Sta in `%APPDATA%\TrovaLavoro`, cioè dentro il
profilo del tuo utente di Windows.

Dentro trovi, in file di testo leggibili con qualunque editor:

- il tuo **profilo**, con lo storico di tutte le versioni confermate;
- una **cartella per ogni candidatura**, col nome parlante (data, azienda, ruolo): dentro
  l'annuncio, il confronto, il CV, la lettera, l'email e i documenti prodotti;
- il **registro** delle candidature e le tue **preferenze**;
- la **chiave API**, e quello è l'unico file non leggibile — perché è una credenziale.

Una candidatura che non ti serve più — una prova, un doppione, un annuncio che si è
rivelato un'altra cosa — la togli dalla **Home**: scegli la sua riga nell'elenco e premi
**«Elimina candidatura»**. Il programma ti dice che cosa sparisce e aspetta un «Confermo»:
se ne va la sua cartella intera, con l'annuncio, il confronto, il CV, la lettera e l'email.
Non c'è cestino e non si torna indietro — i file che avevi esportato altrove, sul Desktop o
in una cartella tua, restano dove sono.

Il principio è che tu resti padrone dei tuoi dati **anche senza questo programma**: se un
giorno lo cancelli, i tuoi CV e le tue lettere restano lì, in DOCX e PDF, apribili da
chiunque.

Se preferisci tenere i dati altrove — per esempio su un altro disco, o in una cartella di
prova — puoi avviare il programma così:

```
TrovaLavoro.exe --dati "D:\le-mie-candidature"
```

Quando la cartella non è quella solita, il programma **te lo dice** nel titolo della finestra:
scambiare una cartella di prova per quella vera è precisamente l'errore che questa comodità
renderebbe facile.

---

## Il backup

Da *Impostazioni → Backup e ripristino*. Puoi salvare **solo il profilo** — con il suo storico
e il CV base — oppure **tutto**: profilo, registro e candidature. Esce un **unico file `.json`**
che puoi mettere dove vuoi — un altro disco, una chiavetta, il cloud.

Per rimettere le cose a posto si riparte da quel file: il programma prima ti **mostra cosa
contiene e cosa sovrascriverebbe**, e scrive solo dopo la tua conferma. Il profilo che c'era
finisce comunque nello storico, così un ripristino non può distruggere l'unico profilo buono.

Due cose che nel backup **non entrano**, di proposito: la **chiave API** (è cifrata per il tuo
utente di Windows: su un altro PC non servirebbe a niente) e il **diario tecnico dei guasti**,
che non è un tuo dato. I documenti già prodotti — i `.docx`, i `.pdf`, i `.eml` — sono file
normali dentro le cartelle delle candidature: si copiano come qualunque altro file.

Se vuoi ricominciare da zero, sempre dalle Impostazioni c'è **«Elimina tutto»**: chiede
conferma scrivendo una parola, perché è un gesto che non si disfa.

---

## Se qualcosa va storto

Il programma cerca sempre di dirti **che cosa** è successo e **che cosa puoi farci**, invece
di mostrarti un codice d'errore. I casi più comuni:

| Ti dice | Vuol dire | Che fare |
|---|---|---|
| «L'AI ha rifiutato la chiave API» | la chiave è sbagliata, scaduta o senza credito | *Impostazioni → Cambia la chiave*, e prova la chiave nuova col bottone |
| «Non riesco a raggiungere l'AI» | la connessione non c'è | riprova quando torna: non hai perso niente |
| «Il modello … non è più disponibile» | quel modello è stato ritirato dal listino | *Impostazioni → Sotto il cofano*, scegline un altro dalla tendina |
| «Troppe richieste in poco tempo» | hai superato il limite di frequenza | aspetta un minuto e riprova |

Se invece capita qualcosa di strano che vuoi segnalare, in *Informazioni* c'è **«Copia
diagnostica»**: mette negli appunti un foglietto tecnico — versione, cartella dati, ultimi
guasti — **senza la tua chiave e senza i tuoi testi**, che puoi incollare in un messaggio.

---

## Tenerlo aggiornato

Non si aggiorna da solo, ed è una scelta: un programma che si riscrive da sé chiede più
fiducia di quanta ne serva qui.

In *Informazioni* c'è **«Cerca aggiornamenti»**: premuto, chiede a GitHub qual è l'ultima
versione pubblicata e te lo dice. Se ce n'è una nuova, si scarica il nuovo `.exe` e si
**sostituisce il vecchio**: i tuoi dati non si toccano e il programma riparte da dov'era. Se
ti risponde che *non risulta pubblicata nessuna versione*, non è un guasto: vuol dire che
quella che hai è l'unica che esiste.

Finché non lo premi, verso GitHub non parte niente.

---

## Che cosa il programma non fa

Vale la pena dirlo in chiaro, perché sono le domande che uno si fa prima di fidarsi.

- **Non inventa** esperienze, titoli o risultati che tu non abbia dichiarato.
- **Non spedisce email al posto tuo**: prepara il file, e a mandarlo sei tu, dopo averlo
  riletto.
- **Non manda statistiche d'uso a nessuno**: niente telemetria, niente «invii anonimi».
- **Non si aggiorna in silenzio** e non scarica niente da solo.
- **Non condivide i tuoi dati**: all'AI vanno solo i testi che le dai da lavorare, e solo nel
  momento in cui glieli dai.

---

© 2026 Aviolab AI — Tutti i diritti riservati
