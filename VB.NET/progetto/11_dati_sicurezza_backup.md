# 11 — Dati, sicurezza e backup

*Dove vivono i dati dell'utente, come sono protetti i segreti, come si salva e si
ripristina tutto. File JSON leggibili, niente database: la trasparenza è parte del
prodotto.*

## 11.1 La cartella dati

Default: `%APPDATA%\TrovaLavoro` (modificabile al primo avvio o nelle Impostazioni).

```
TrovaLavoro\
├── config.json            impostazioni (modelli AI, cartelle)
├── ricerche.json          preferenze di ricerca, ricerche salvate e tabella dei portali
├── taratura.json          soglia, pesi e limiti del match (v. 11.6)
├── modelli.json           mappa livello → modello AI (v. 11.6, cap. 02.5)
├── segreti.bin            chiave API Anthropic, cifrata (v. 11.3)
├── profilo\
│   ├── profilo.json       il profilo corrente (fonte di verità unica)
│   ├── cv_base.json       il 📄 CV-1 base: è del profilo, non di un annuncio
│   ├── out\               i file del CV base (.docx .pdf)
│   └── storico\           una copia datata per ogni versione confermata
├── opportunita\
│   └── 2026-08-05_rossi-spa_tecnico-manutenzione\
│       ├── annuncio.json · giudizi.json · mitigazioni.json
│       ├── appunti.json   (esito confermato del brainstorming)
│       ├── cv.json · lettera.json
│       ├── stato.json     (stato, date, lingua, versione profilo usata)
│       └── out\           (i file prodotti: .docx .pdf .eml)
├── registro.json          la vista d'insieme delle candidature
├── webview2\              profilo di navigazione del browser integrato
├── log\app.log            log tecnico (senza segreti, v. 11.3)
└── backup\                gli export JSON (v. 11.4)
```

- **Un'opportunità = una cartella**: tutto ciò che riguarda una candidatura sta
  insieme, apribile anche a mano con Esplora file. Il nome della cartella è parlante
  (data + azienda + ruolo).
- **La cartella nasce a T4, la vista d'insieme a T5** *(deciso il 2026-08-10, aprendo
  T4)*. La coda delle opportunità, il `registro.json` e la macchina a stati sono di T5,
  ma T4 produce già annuncio, giudizi, mitigazioni, CV e lettera: quella roba deve
  atterrare da qualche parte. La scelta è di **scriverla subito nella sua casa
  definitiva** — la cartella qui sopra, con i nomi qui sopra — e non in un ripiego
  temporaneo. A T4 la cartella nasce dunque completa di tutto tranne ciò che la tappa
  non ha ancora: nessun `registro.json`, e uno `stato.json` che porta le date, la lingua
  e la versione di profilo usata ma non ancora lo stato del ciclo di vita. Così T5
  aggiunge una vista sopra dati che ci sono già, invece di dover migrare quelli
  prodotti nel frattempo. È lo stesso principio dell'app che «riapre dov'era»
  (cap. 12.7): un documento generato e non ritrovabile domani sarebbe un documento
  perso, e T4 genera i primi documenti veri del progetto.
- **Il profilo è versionato**: ogni modifica confermata (editing, sessione di
  aggiornamento) salva una copia datata nello storico; `stato.json` di ogni
  opportunità annota **con quale versione** del profilo furono generati i documenti.
  Così un CV inviato resta sempre spiegabile, anche a profilo evoluto.
- **Il 📄 CV-1 base sta col profilo, non con le opportunità** *(deciso il 2026-08-10:
  il disegno non gli aveva mai dato una casa, e T4 è la tappa che lo genera)*. Nasce
  senza alcun annuncio, da P2, ed è il ritratto del profilo in forma di CV: metterlo in
  una cartella-opportunità vorrebbe dire legarlo a una candidatura che non ha. Vive
  quindi accanto al profilo da cui discende — che è poi la vista-dati «un profilo, molti
  CV» del cap. 02.2 presa alla lettera: il CV base è del profilo, i CV mirati sono delle
  opportunità. Come `stato.json`, anche `cv_base.json` annota **da quale versione di
  profilo** è nato: se il profilo poi cambia, il CV base non viene cancellato di
  nascosto né aggiornato di soppiatto — resta lì e l'app dice che è di una versione
  precedente, lasciando a chi legge la scelta se rigenerarlo.
- **Un profilo corrotto si mette in salvo, mai in pericolo** *(2026-08-09, revisione
  adversariale)*: se `profilo.json` non si legge, l'app ne copia **subito** una fotografia
  (`profilo.rotto-<data>.json`) prima che qualunque «Salva» possa sovrascriverlo, e il
  messaggio dice dove sta la copia. La promessa «il file resta lì da recuperare» ora è
  mantenuta anche se l'utente preme un tasto qualsiasi.
- Formati **JSON con rientri**, leggibili in qualsiasi editor: l'utente è padrone dei
  suoi dati anche senza l'app.

## 11.2 Cosa esce dal PC (e cosa no)

| Dato | Esce? | Verso dove |
|---|---|---|
| Testi per l'elaborazione (profilo, annuncio, PDF da trascrivere) | sì | solo API Anthropic, via HTTPS |
| Pagine dei portali visitate nel browser incorporato | sì, come in un browser qualunque | i portali stessi; le credenziali le digita l'utente e l'app non le vede (cap. 06.6) |
| Tutto il resto (registro, documenti, email, configurazione, log) | **no** | — |

L'email di candidatura **non esce dal programma**: viene scritta come file `.eml` nella
cartella dell'opportunità, e a spedirla è il programma di posta dell'utente
(cap. 15, voce 9).

Niente telemetria, niente servizi del produttore, niente aggiornamenti automatici
silenziosi.

## 11.3 I segreti

- La **chiave API Anthropic** è cifrata con la protezione dati di Windows (DPAPI)
  **legata all'utente che l'ha salvata**: il file `segreti.bin` copiato su un altro PC o
  letto da un altro account non si decifra. È il compromesso giusto per un'app
  personale: robusto, senza inventare crittografia in proprio. *(È rimasta l'unica
  credenziale del programma: senza invio SMTP non c'è più alcuna password di posta da
  custodire — cap. 15, voce 9.)*
- Nell'interfaccia la chiave è sempre mascherata (`sk-ant-…ultime 4 cifre`).
- Il **log** non contiene mai segreti né testi integrali dei documenti: registra
  eventi, esiti e codici di errore (una funzione di redazione maschera qualunque campo
  il cui nome contenga «key» o «password» prima della scrittura).
- I backup JSON (11.4) **non contengono i segreti**, di proposito: dopo un ripristino
  la chiave API va reinserita. Un backup che gira via email o chiavetta non deve poter
  bruciare la chiave.

## 11.4 Backup e ripristino (F7)

**Esporta** (dal pannello Profilo o dalle Impostazioni):

- contenuto a scelta: **solo profilo** (con storico) oppure **profilo + registro +
  opportunità** (gli artefatti JSON; i file in `out\` restano fuori dal JSON e si
  copiano a parte, sono già file normali);
- un solo file `.json` con intestazione di versione:

```json
{
  "formato_backup": 1,
  "app": "TrovaLavoro",
  "data": "2026-08-05T18:30:00",
  "contenuto": ["profilo", "storico", "registro", "opportunita"],
  "profilo": { … },
  "storico": [ … ],
  "registro": { … },
  "opportunita": [ … ]
}
```

**Importa**:

1. l'app legge il file, ne verifica formato e versione;
2. mostra **cosa contiene** e **cosa sovrascriverebbe** («il backup ha un profilo del
   3 agosto; quello attuale è del 5 agosto: vuoi davvero sostituirlo?»);
3. solo alla conferma scrive, e prima di scrivere salva l'attuale nello storico
   (il ripristino non deve mai poter distruggere l'unico profilo buono).

Il campo `formato_backup` permette ai programmi futuri di leggere i backup vecchi:
il numero cresce solo quando lo schema cambia davvero.

## 11.5 Pulizia e diritto all'oblio

Dalle Impostazioni: «Svuota dati di navigazione» (cartella `webview2\`), «Elimina
un'opportunità» (la sua cartella, con conferma di livello 5), «Elimina tutti i dati»
(l'intera cartella dati, conferma di livello 6 con nome dell'app da ridigitare).
L'app non lascia nulla in giro fuori dalla cartella dati, quindi la disinstallazione
è: cancellare l'exe e, se si vuole, la cartella dati.

## 11.6 I due file dei numeri: taratura e modelli

`taratura.json` contiene i numeri del calcolo delle stelle: soglia 1,5, pesi 5 e 1,
correzione della mitigazione limitata fra −20 e +10, tetto a 20 punti, e la regola del
requisito eliminatorio (⛔ → massimo una stella). Sono valori **di prodotto, non
preferenze**: l'interfaccia non li mostra e non li lascia toccare (cap. 15, voce 10).

Il motivo di tenerli fuori dall'interfaccia è che le stelle devono restare confrontabili
fra un annuncio e l'altro: se l'utente potesse spostare la soglia, il punteggio
smetterebbe di misurare quanto è adatto a quel posto e comincerebbe a misurare quanto è
ottimista quel giorno. Il motivo di tenerli fuori dal **codice** è opposto e pratico:
durante la messa a punto ritoccare un valore deve costare una riga, non una nuova
versione da ricompilare e reinstallare su due macchine.

Se il file manca o è illeggibile, il programma usa i valori predefiniti che porta
dentro di sé e lo annota nel log: una taratura corrotta non deve impedire l'avvio.
*Precisato il 2026-08-09 (revisione adversariale), quando un valore non numerico
dentro una mappa dei pesi faceva cadere l'app all'avvio*: una mappa storta **si scarta
intera** e valgono i predefiniti — tenere solo le voci buone avrebbe falsato il
punteggio in silenzio, che è peggio di un ripiego dichiarato.

*E dal 2026-08-10 (T4) c'è anche la **validazione di range**, che fino a lì era un'idea
futura*: ogni numero letto dichiara l'intervallo in cui ha senso — il clamp verso il
basso fra −100 e 0, quello verso l'alto fra 0 e 100 — e un valore fuori scala, per quanto
ben formato, **viene scartato e annotato** invece di entrare zitto. Era il momento
giusto: T4 è la prima tappa che usa `CalcoloMatch` sul serio dentro l'applicazione, e da
qui in avanti un `"clamp_su": -50` non sposterebbe un numero in un collaudo, sposterebbe
le stelle che l'utente legge prima di decidere se candidarsi.

**`modelli.json` è il suo gemello** (cap. 02.5): tiene la mappa livello → modello, e
per ciascun livello anche l'interruttore del ragionamento esteso, che ha **tre** stati
— non dichiarato, spento, acceso. Vale la stessa regola dei predefiniti: un file
assente, parziale o illeggibile non impedisce l'avvio, si ricade sui valori interni e
lo si annota. Anche qui il motivo è pratico: cambiare modello — o fare il secondo
esperimento su Sonnet 5 — deve costare una riga, non una nuova build da reinstallare
su due macchine. *(Realizzato a T2 in `Ai/Modelli.vb`.)*

A leggerli all'avvio è `Motore/ContestoApp` *(da T3c, 2026-08-07)*: entrambi i file, con
l'avviso di ripiego quando si cade sui predefiniti. Prima esistevano i lettori ma non
li chiamava nessuno, e un file di taratura messo nella cartella dati non avrebbe avuto
alcun effetto — senza che niente lo dicesse.
