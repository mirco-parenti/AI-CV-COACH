# 03 — Interfaccia grafica

*Il design dell'applicazione: principi, colori, font, struttura delle finestre e dei
pannelli. È il sistema di design **proprio** di AI-CV-COACH, pensato per dare lo stesso
«family feeling» delle applicazioni di casa Aviolab. Nota di riservatezza: qui non si
descrive nessun software aziendale; si specifica, in autonomo, l'aspetto che questa
applicazione deve avere.*

## 3.1 Principi

1. **L'interfaccia guida, non decora.** L'utente di riferimento non è un tecnico: davanti
   a ogni bottone si chiede «cosa mi succede se lo premo?». L'interfaccia deve
   rispondere da sola, con il colore e la posizione, prima ancora del testo.
2. **Il colore di un bottone dice la conseguenza dell'azione**, non il marchio: un'azione
   sicura è verde, una esplorativa è tenue, una distruttiva è rossa. Il colore del brand
   vive nei **titoli** e negli accenti, mai nei bottoni ordinari.
3. **Flat totale**: niente gradienti, ombre, bordi 3D. Bordi sottili da 1 px, superfici
   piatte, `FlatStyle.Flat`.
4. **Un solo font**: Segoe UI ovunque (Consolas solo per dati tecnici). Mai font di
   sistema vecchio stile.
5. **Coerenza assoluta**: due bottoni con la stessa funzione, in due pannelli diversi,
   sono identici. Nel dubbio sul «peso» di un'azione, si sale di livello (più cautela).
6. **Pannelli statici**: ogni schermata è disegnata nel designer di Visual Studio,
   con controlli dichiarati staticamente. A runtime cambiano solo i contenuti
   (testi, liste, anteprime, visibilità), mai la struttura.
   **Unica eccezione, dichiarata: le bolle della conversazione** (P5). Una chat non ha un
   numero di righe noto in anticipo, e nessuna lista di sistema sa mostrare bolle di
   larghezza variabile allineate a destra e a sinistra: lì i controlli nascono a runtime
   dentro il flusso. Tutto il resto del pannello — casella, «Invia», i **tre bottoni di
   scelta**, la fascia delle azioni — è dichiarato nel designer, e le scelte del turno si
   limitano a cambiare etichetta, colore e visibilità di quei tre. *(Deciso in T3c,
   2026-08-07.)*

## 3.2 Token di design

Tutti i colori e i font dell'applicazione vengono **da questa tabella e solo da questa**
(nel codice: un modulo `StileApp` con le costanti; vietato `Color.FromArgb` sparso nei form).

### Colori

| Token | Hex | Uso |
|---|---|---|
| `TestoPrimario` | `#212529` | testo normale, valori, titoli di sezione |
| `TestoSecondario` | `#6C757D` | didascalie, suggerimenti, stati |
| `SfondoBase` | `#F8F9FA` | sfondo delle finestre |
| `SfondoContenuto` | `#FFFFFF` | aree di lavoro (testi, anteprime, input) |
| `BordoLeggero` | `#DEE2E6` | separatori e bordi da 1 px |
| `BordoForte` | `#CED4DA` | bordo dei controlli interattivi |
| `Accento` | `#0B06B0` | focus, link, selezione (blu profondo) |
| `AccentoTenue` | `#E4E7FB` | riga selezionata, hover |
| `FondoAzione` | `#C0E8FF` | fondo del bottone d'azione principale del pannello |
| `RossoTitoli` | `#FA0825` | titoli delle finestre e dei GroupBox, marker |
| `Successo` | `#28A745` | azioni sicure/positive, badge OK |
| `Avviso` | `#FFC107` | azioni che modificano, badge attenzione |
| `Pericolo` | `#DC3545` | azioni distruttive, badge errore |
| `Informazione` | `#17A2B8` | badge informativi |

### Font

| Ruolo | Font |
|---|---|
| Titolo finestra/pannello | Segoe UI 14–16 **Bold**, colore `RossoTitoli` |
| Titolo GroupBox | Segoe UI 9 **Bold**, colore `RossoTitoli` |
| Bottone d'azione principale | Segoe UI 9.75 **Bold** |
| Testo di lavoro e bottoni neutri | Segoe UI 9 |
| Didascalie / hint | Segoe UI 8, colore `TestoSecondario` |
| Dati tecnici (punteggi, log) | Consolas 8.5 |

### Spaziature e dimensioni (regola 14 / 12 / 8)

- **14 px** di margine interno nei GroupBox e nei riquadri;
- **12 px** di distanza tra controlli affiancati;
- **8 px** minimo tra le righe (14–16 dove serve respiro).
- Bottoni standard **110×32** (testo breve) o **130×32** (testo medio); bottoni della
  barra superiore **110×34**.

## 3.3 I livelli di conseguenza dei bottoni

Ogni bottone dell'applicazione appartiene a **uno** di questi livelli. La saturazione
del colore cresce con il peso della conseguenza:

| Livello | Quando | Aspetto |
|---|---|---|
| **0 — Neutro** | navigazione, annulla, chiudi | bianco, bordo `BordoLeggero`, Segoe UI 9 |
| **1 — Sicuro positivo** | conferme senza rischio («Salva profilo», «Cattura annuncio») | fondo `Successo`, testo bianco, bold |
| **2 — Esplorativo leggero** | aprire, sfogliare, vedere anteprime | fondi pastello tenui, testo scuro |
| **3 — Azione principale del pannello** | il bottone «avanti» del flusso («Genera CV», «Confronta») | fondo `FondoAzione`, bordo `Accento`, Segoe UI 9.75 Bold |
| **4 — Attenzione** | modifica dati esistenti («Sovrascrivi profilo», «Rigenera») | fondo `Avviso`, testo scuro, bold |
| **5 — Distruttivo** | eliminare un'opportunità, scartare | fondo `Pericolo`, testo bianco, bold |
| **6 — Critico** | inviare un'email, cancellazioni definitive | fondo `RossoTitoli`, testo bianco, bold — sempre preceduto da conferma |

Regole: `FlatStyle.Flat`, `UseVisualStyleBackColor = False` ovunque; mai il rosso del
brand su un bottone che non sia di livello 6; nel dubbio tra due livelli si sceglie il
più alto.

## 3.4 Architettura delle finestre

Una **finestra principale** (`FormPrincipale`) più finestre secondarie di servizio.
Niente barra dei menu classica: la navigazione sta in una **barra superiore di bottoni
con icona** (FontAwesome.Sharp); i menu contestuali (tasto destro) usano voci con emoji
(`✏️ Rinomina`, `🗑️ Elimina`, `📤 Esporta…`).

```
┌────────────────────────────────────────────────────────────────────┐
│ BARRA SUPERIORE   [🏠 Home] [👤 Profilo] [🔍 Ricerca]              │
│      [📋 Candidatura] [⚙ Impostazioni]  stato AI · modello · …     │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│                    AREA CENTRALE                                   │
│   (un pannello per funzione, uno solo visibile per volta:          │
│    P1 Home · P2 Profilo · P3 Ricerca · P4 Opportunità ·            │
│    P5 Dialogo/Brainstorm · P6 Documenti · P7 Email)                │
│                                                                    │
├──────────────┬─────────────────────────────────────────────────────┤
│  ┌────────┐  │  BARRA DI STATO   «Pronto» · avanzamento chiamate   │
│  │ LOGO   │  │                                                     │
│  │ AI-CV- │  │                                                     │
│  │ COACH  │  │                                                     │
│  └────────┘  │                                                     │
└──────────────┴─────────────────────────────────────────────────────┘
```

- La macro-struttura è un `TableLayoutPanel` (righe: barra superiore fissa, area
  centrale elastica, fascia inferiore fissa); dentro ogni banda, `Panel` a coordinate
  fisse. I pannelli P1–P7 sono **UserControl disegnati nel designer**, impilati
  nell'area centrale e mostrati uno alla volta: struttura statica, nessun controllo
  creato a runtime.
- **La barra ha cinque bottoni, non quattro** *(deciso in T4c, 2026-08-10)*: fra Ricerca
  e Impostazioni c'è **📋 Candidatura**, che porta a P4. Nel disegno originale a P4 si
  arrivava dalla coda delle opportunità in Home, ma la Home è di T5c e l'incolla-testo è
  già di T4: senza una porta propria, il pannello che questa tappa costruisce non
  sarebbe raggiungibile. Resta anche dopo T5 — un annuncio arriva spesso da un'email,
  senza passare dal registro (cap. 12.3).
- **Non ogni pannello ha un bottone in barra.** P5 (dialogo), P6 (documenti) e P7 (email)
  sono **passi di un flusso**, non destinazioni: ci si arriva dal pannello che li
  precede, e il bottone della barra resta acceso su quello — P5 e il 📄 CV-1 base sotto
  «Profilo», P6 di una candidatura sotto «Candidatura». Il bottone «torna indietro» di
  quei pannelli riporta **da dove si è venuti**, e lo dice nell'etichetta: a P6 si arriva
  da due strade, e mandare l'utente in un pannello dove non è mai stato sarebbe il vicolo
  cieco che il cap. 12.7 vieta. *(T4c, 2026-08-10.)*
- Finestra principale: avvio massimizzata, `MinimumSize` 1150×600, DPI `SystemAware`,
  sfondo `SfondoBase`.
- **La fascia dei comandi va a capo** *(2026-08-14)*. In fondo a ogni pannello sta una
  fascia con due file di bottoni: i comandi di quel pannello a sinistra, quelli che portano
  altrove a destra. Fino alla 0.3.018 ognuna si disponeva per conto suo — una da sinistra,
  una da destra — e quando lo spazio finiva **si incontravano a metà strada**: alla
  `MinimumSize` fino a 676 px di bottoni sopra altri bottoni. Non si vedeva perché
  l'applicazione si apre massimizzata, ed è rimasto lì per tre tappe, peggiorando a ogni
  bottone aggiunto. Ora la fascia si comporta come qualunque barra di comandi: se le due
  file non ci stanno insieme si dispongono su **righe diverse** — quelle che portano
  altrove restano in fondo, dove «Salva profilo» si cerca — e una fila troppo lunga si
  spezza a sua volta. La **fascia cresce in altezza** quanto le righe richiedono: a
  rimetterci è l'area dei dati, che di spazio ne ha, e non la leggibilità dei comandi. Le
  azioni di **livello 6** restano su una riga tutta loro (v. cap. 11.5): il vuoto intorno è
  la loro prima difesa, e non si baratta per due righe di spazio.
- Finestre secondarie (Impostazioni, Primo avvio, Anteprima file, Informazioni su…):
  dialoghi a bordo fisso, sfondo bianco, titolo Segoe UI 14 Bold in `RossoTitoli`.
- **La prima finestra secondaria è la conferma critica** *(2026-08-14)*: `FinestraConfermaCritica`,
  il dialogo delle azioni di livello 6. Elenca cosa sparisce e cosa resta, e per accendere
  il bottone chiede di **ridigitare una parola** (il nome dell'app, come vuole il
  cap. 11.5). Non è una `MessageBox` perché un Sì/No si preme di riflesso: qui la mano
  deve scrivere. Nasce per «ELIMINA PROFILO - DEFINITIVO» in P2, ma è generica —
  «Elimina tutti i dati» e «Elimina un'opportunità» delle Impostazioni (T9) useranno
  questa. Invio non conferma, Esc annulla.
- **Altre due finestre secondarie con T6** *(2026-08-14)*. `FinestraChiaveApi` chiede la
  chiave API quando non c'è (cap. 11.3): compare **prima** che i pannelli si colleghino al
  motore, con la casella mascherata e la spunta per rileggere quel che si è incollato, e
  «Non adesso» è una risposta legittima — senza chiave l'applicazione si apre lo stesso e
  restano spente le sole funzioni che chiamano l'AI. Si centra sullo **schermo** e non sul
  proprietario, perché quando compare la finestra principale non è ancora a video.
  `FinestraDocumenti` è la **conferma umana** del cap. 05.2: mostra i file riconosciuti
  nella cartella documenti con categoria e motivo, e la categoria si corregge scegliendo la
  riga e poi la voce in una tendina — non con una griglia editabile, che sarebbe stato
  l'unico controllo del suo genere in tutta l'applicazione. Non chiama l'AI: «Fai rileggere
  la cartella» e «Cambia cartella» **tornano al pannello**, che è chi sa aspettare e
  annullare (cap. 02.6).
  `FinestraAppunti` *(2026-08-18, T7c)* è la conferma degli **appunti di mira** distillati
  dal ragionamento (cap. 12, A6.3), e somiglia di proposito alla precedente: elenco con le
  spunte, e la riga scelta si corregge nella casella sotto invece che dentro la griglia.
  Due cose sono sue soltanto. La prima: gli appunti arrivano **già spuntati**, perché sono
  la proposta dell'AI su una conversazione appena fatta e togliere è più veloce che
  rimettere. La seconda: sotto l'elenco c'è una **seconda lista che non si spunta** — le
  cose dette in chat che nel profilo non risultano. Non sono appunti e non entrano nei
  documenti; stanno lì con scritto perché, che è l'unico modo di non perderle senza usarle.
  Come le altre, non chiama l'AI: raccoglie una decisione e la restituisce al pannello.
- **E con T9b arriva la più grande, che è anche l'unica che non chiede niente**
  *(2026-08-21)*. `FinestraImpostazioni` è P8 (v. la tabella dei pannelli): non raccoglie
  una decisione per restituirla a chi l'ha aperta, ma **fa** — scrive le preferenze appena
  si cambiano, apre cartelle, cancella. Per questo non ha un OK, e per questo le tre cose
  che non si disfano passano da altrettante finestre già esistenti invece che da sue
  varianti. Restituisce al `FormPrincipale` **tre notizie**, non un esito: se è arrivata
  una chiave nuova (allora il motore si rimonta e i pannelli si ricollegano), se l'utente
  vuole i documenti (allora si va in P7), se i dati sono stati eliminati (allora
  l'applicazione si chiude, perché da lì in poi lavorerebbe su file che non ci sono più).
  Una cosa l'ha insegnata al banco: su una finestra mai mostrata `PerformClick` non
  scatena niente, quindi ogni azione che valga la pena collaudare deve avere un **metodo
  pubblico** — è la stessa regola che la `FinestraBackup` aveva già scoperto a T9a.

## 3.5 Il pannello del logo (in basso a sinistra)

Elemento identitario irrinunciabile, presente in ogni momento nell'angolo in basso a
sinistra della finestra principale:

```
┌──────────────────────────┐
│        [immagine]        │   PictureBox 101×101, scudo Aviolab AI
│                          │   (in forma binaria nel sorgente)
│       AVIOLAB AI         │   Segoe UI 16 Bold, TestoPrimario, centrato
│  Ver. 1.0.012 · Pool 1.03│   Segoe UI 8, TestoSecondario, centrato
│ ©2026 Aviolab AI - Tutti │   Segoe UI 8, TestoSecondario, centrato
│   i diritti riservati    │
└──────────────────────────┘
```

> Il pannello in basso a sinistra è il **marchio aziendale**: sotto lo scudo compare
> **sempre e solo «AVIOLAB AI»** *(precisato 2026-08-06)*. **Il nome mostrato
> all'utente resta «TrovaLavoro»** (cap. 15, voce 3) nella barra del titolo; il
> sottotitolo «e candidati con il CV giusto, senza fatica» compare nella finestra
> «Informazioni su…» e nel primo avvio. *AI-CV-COACH* resta il nome del progetto e
> del repository, non del prodotto.

- `Panel` di circa **261×216 px**, sfondo `SfondoBase`, ancorato **Bottom+Left**,
  aggiunto al form come elemento flottante sopra la struttura (così sopravvive ai
  ridimensionamenti).
- La riga versione mostra **due numeri**: la versione dell'applicazione e la versione
  della **libreria prompt** caricata (cap. 04), separate dal punto mediano « · ».
  Il numero di pool dichiara anche sorgente e stato: `Pool 1.03` (cartella esterna),
  `Pool 1.03 (integrato)` (copia incorporata nell'exe), `Pool 1.03*` (file modificati
  rispetto al manifest — cap. 04.5). «Pool —» può comparire solo in caso di anomalia
  totale, e l'app la spiega.
- **Modalità compatta**: sotto ~1350 px di larghezza restano solo l'immagine (ridotta) e
  la versione, per liberare spazio.
- Il logo è lo **scudo di Aviolab AI** e vive **in forma binaria dentro il sorgente**
  (PNG 256×256 codificato Base64 in `LogoAviolab.vb`): nel repository e accanto all'exe
  non esiste nessun file immagine. *(Deciso 2026-08-06 in T1 — cap. 15, voce 4.)*
- Il numero di versione dell'app vive in **un solo file sorgente** (`Versione.vb`, una
  costante), mai duplicato altrove; ogni modifica al codice lo incrementa.

## 3.6 I pannelli, uno per uno

| ID | Pannello | Contenuto principale |
|---|---|---|
| **P1 Home** | cruscotto | stato del profilo (esiste? aggiornato quando?), coda opportunità con stelle e stati, scorciatoie ai flussi («Nuova ricerca», «Aggiorna profilo»). *Costruito a T5c (2026-08-13), ed è il pannello su cui l'applicazione ora **si apre**. La coda è una lista in vista dettagli con sei colonne — **Match** (stelle, numero e ⛔), **Azienda**, **Ruolo**, **Stato**, **Da dove**, **Aggiornata** — ordinabile cliccando un'intestazione, con un filtro «Mostra» (Tutte · Da completare · Generate · Scartate) e i contatori del cap. 07.3; quello delle **inviate** è entrato con **T6** *(2026-08-14)*, cioè con la tappa che lo fa salire — prima sarebbe stato fermo a zero, e un contatore che non può muoversi non conta niente. *Con T6 la coda si filtra anche **per stelle** («almeno N»), e i due filtri si intersecano invece di sostituirsi: rispondono a domande diverse. Da qui l'elenco si può anche **esportare** in CSV o markdown (cap. 07.3).* Le **scartate restano** nell'elenco, scritte in grigio: scartare non è cancellare. Una candidatura si riapre dal bottone «Apri la candidatura» o dal doppio clic sulla riga. **Qui si guarda, non si decide**: il pannello non cambia lo stato di nessuna candidatura — nemmeno lo scarto sta qui, ma nella scheda che si sta guardando. Senza profilo il cruscotto lo dice e il bottone accanto cambia mestiere («Costruisci il profilo» invece di «Apri il profilo»), perché da lì si comincia; «Aggiorna profilo» è lì spento col suo tooltip (flusso D), per la regola 3.8* *A **T9c** (2026-08-21) la Home impara a dire chi aspetta da troppo (cap. 07.3): sotto i contatori compare una riga — «⏳ 2 candidature spedite aspettano da più di 14 giorni; la più vecchia da 21 (Acme — Magazziniere)» — le righe interessate portano nella colonna «Stato» **da quanti giorni** aspettano («Inviata · 20 gg») e si isolano con la voce «Da sollecitare» del filtro «Mostra». Quando non c'è niente da ricordare la riga **sparisce** e la fascia dei filtri torna alta com'era, perché un avviso che occupa spazio anche da spento insegna a non guardarlo. La stessa colonna «Stato» dice ora l'**esito** quando c'è — «Rifiutata», non «Con esito» — e ai contatori se n'è aggiunto uno, «con esito», che prima sarebbe stato fermo a zero* |
| **P2 Profilo** | scheda del profilo | tutte le sezioni del profilo JSON **campo per campo, modificabili**; bottoni: «IMPORTA CV DA UN FILE» (L2), «IMPORTA CV DA LINKEDIN» (L2), «COSTRUISCI IL TUO CV - DIALOGO GUIDATO» (L2), «Sessione di aggiornamento» (L2), «Genera 📄 CV-1 base» (L3), «Esporta backup» (L2), **«Salva profilo» (L1)** — è la sola porta da cui il profilo entra nell'archivio, anche quando arriva dal dialogo (cap. 12.2) — e **«ELIMINA PROFILO - DEFINITIVO» (L6)**, che è la porta opposta. *Dalla revisione adversariale (2026-08-09): il salvataggio **pota le voci mai riempite** (un «Aggiungi» lasciato vuoto non diventa un'esperienza fantasma nei prompt), le categorie della patente valgono solo con il «sì», e la scheda del testo letto compare solo finché il profilo mostrato è quello importato.* *A T5d (2026-08-14) l'import diventa **due bottoni** invece di uno, e ognuno dice da dove legge: il vecchio si chiama ora «da un file…», il nuovo «da un sito…». Il secondo non legge niente — **porta in P3**, dove vive il browser, e il pannello che lo accoglie dice cosa fare. La scelta della strada sta qui perché è qui che si costruisce il profilo ed è qui che la si cerca; l'atto sta là perché è là che c'è una pagina aperta. Sono spenti insieme, per la stessa ragione: senza chiave nessuna delle due strade arriva da nessuna parte, e mandare l'utente in fondo a un corridoio per dirglielo lì sarebbe scortese* *Il 2026-08-14 le **tre porte del profilo cambiano nome** (i nomi qui sopra sono i nuovi; la nota di T5d racconta com'erano quando sono nate): maiuscolo e più espliciti, perché sono la prima cosa che un utente nuovo guarda in questo pannello. Due conseguenze da tenere presenti. La prima: «IMPORTA CV DA LINKEDIN» **promette meno di quello che fa** — porta in P3, dove il browser legge qualunque pagina che racconti un percorso, e a T5d si era deciso apposta di non controllare che il sito fosse LinkedIn (cap. 06.7); il nome sceglie il caso d'uso vero, e a dire il resto resta il pannello che accoglie. La seconda: il bottone del dialogo è passato da 200 a **300 px** perché l'etichetta non ci stava, e la fila di sinistra si è allungata di altrettanto — il che peggiora la sovrapposizione a finestra stretta annotata in `in_sospeso.md` (3.4)* *Il 2026-08-14 compare **«ELIMINA PROFILO - DEFINITIVO» (L6)**, la porta opposta al salvataggio: manda via la cartella `profilo\` intera — profilo, storico, 📄 CV-1 base e i suoi file — e **non tocca le candidature**, che restano nella Home con il loro registro (cap. 11.5). Sta nella fascia delle azioni **con «Salva profilo», ma all'estremo opposto della fila**: è il solo bottone del pannello da cui non si torna indietro, e non deve stare sotto il dito di chi sta salvando. Prima di eseguire passa dalla `FinestraConfermaCritica` (3.4), che elenca cosa sparisce e cosa resta e vuole la parola `TrovaLavoro` scritta a mano; è acceso solo quando c'è davvero qualcosa da eliminare — un profilo su disco o delle correzioni nei campi — perché un bottone rosso che non ha niente da fare insegna solo a non fidarsi del colore. Quando l'eliminazione avviene, si svuota **tutta l'applicazione**: i campi e la scheda «Testo letto» di P2, il dialogo guidato di P5 (che altrimenti riproporrebbe il profilo appena cancellato), il 📄 CV-1 base in mostra in P6 — mai i documenti di una candidatura — e la Home rilegge* *A T7d (2026-08-18) «Genera 📄 CV-1 base» **genera solo la prima volta**: se un CV-1 base c'è già, porta in P6 e lo mostra. Il verbo dell'etichetta promette più di quel che fa, ed è di proposito — è la stessa scelta di «Genera CV + lettera» in P4, dove il bottone è la porta di quei documenti e non solo il comando che li crea; a rifarli, in tutti e due i casi, c'è «Rigenera», che lo dichiara e lo chiede* *Il 2026-08-19 «IMPORTA CV DA UN FILE» smette di partire dal nulla: se la cartella documenti è stata classificata (cap. 05.2) e il CV più recente esiste ancora, il pannello **lo propone per nome** in una domanda a tre uscite — «Sì, usa questo», «No, scelgo io un altro file», «Annulla». È la porta «qui c'è tutto» che il capitolo dei documenti prometteva: si **propone e non si prende**, perché la conferma umana resta il passo che decide, e l'esistenza del file si verifica **al momento di proporlo** — fra la classificazione e l'import quel CV può essere stato spostato o buttato. Nessun controllo nuovo nel pannello: è una finestra di sistema, e il layout di P2 resta quello validato a video a T3* *A T9a (2026-08-21) «Esporta backup» si accende e diventa **«Backup…»**: apre la finestra di backup e ripristino (F7, cap. 11.4), che tiene insieme le due metà della stessa funzione. L'etichetta perde il verbo perché il bottone non esporta soltanto, e un bottone che dicesse «Esporta» per poi offrire anche il ritorno indietro nasconderebbe metà di quel che fa. Prima di aprirla, le correzioni non salvate si fanno confermare: nel backup finisce il profilo che sta **su disco**, e un ripristino le sostituirebbe senza preavviso; chiusa la finestra dopo un ripristino, il pannello **rilegge il profilo** invece di continuare a mostrare quello di prima* |
| **P3 Ricerca** | browser integrato | WebView2 a tutta area; sopra: barra con ricerche salvate (ComboBox), campo link, bottone **«Cattura annuncio»** (L1); sotto: ultima cattura con esito. *Costruito a T5a (2026-08-12), la barra di sopra è risultata di tre righe invece di una, e la cattura è scesa in fondo: **ricerche salvate** (menù + «Apri» + «Dimentica»); **la ricerca nuova** (menù «Cerca su» dei portali, «cosa», «dove», «Cerca», «Salva questa ricerca»); **la navigazione** («◀», «⟳», casella dell'indirizzo, «Vai»). «Cattura annuncio» sta nella fascia delle azioni in basso, accanto alla riga che racconta l'esito — dov'è il bottone principale in tutti gli altri pannelli. I tre comandi senza etichetta dichiarano il proprio nome accessibile: senza, per chi non vede lo schermo sarebbero anonimi. La prima pagina che si apre è **scritta da noi e non tocca la rete**: dice in tre righe come si usa il pannello, ed è l'unica navigazione che parte da sola.* *A T5d (2026-08-14) accanto alla cattura compare **«Importa CV da questa pagina» (L1)**, che legge la pagina aperta — di norma la propria pagina profilo — e la porta a P2. Sono due bottoni e non uno con due usi, perché il testo va in due direzioni diverse: uno all'analisi dell'annuncio, l'altro alla scheda del profilo. La pagina di casa guadagna il suo quarto punto, e la riga di stato dice quanti caratteri sono stati letti — è il solo modo che l'utente ha di accorgersi che alla strutturazione è andata poca roba prima di guardare un profilo dimezzato e crederlo intero* |
| **P4 Opportunità** | dettaglio candidatura | annuncio estratto, **stelle 0–5 grandi**, elenco giudizi (✓ ~ ✗ ?) con ⛔ sugli eliminatori, note di clamp/gate, lettura d'insieme; bottoni: «Brainstorm» (L2), «Genera CV+lettera» (L3), «Scarta» (L5). *Deciso aprendo T4 (2026-08-10): il pannello nasce con in cima una **fascia d'ingresso** — casella multiriga «incolla qui il testo dell'annuncio» e bottone «Analizza» (L3) — perché a T4 quella è l'**unica** porta da cui un annuncio entra: la cattura dal browser è T5 e il flusso C (cap. 12.3) la dà già come ripiego permanente, non provvisorio. A T5 la fascia non sparisce: si affianca alla cattura, e resta la strada di chi ha in mano un testo e basta. A cattura avvenuta la fascia si richiude, per non rubare spazio ai giudizi.* *Ed è andata così (T5b, 2026-08-12), con un di più che non era scritto: l'annuncio catturato in P3 **entra proprio in quella casella**, e si vede. Non c'erano due strade da tenere allineate — ce n'è una sola, e chi guarda può leggere il testo che è stato mandato all'AI, correggerlo e rilanciare* *E a T5c (2026-08-13) il pannello ha preso il suo stato: in cima, accanto alle stelle, la scheda dice a che punto è quella candidatura, e da dove è stata riaperta. **«Scarta» si accende**, con la conferma che spiega cosa succede davvero («non cancello niente: resta nella sua cartella e la ritrovi nella Home. Ma la do per chiusa, e da uno scarto non si torna indietro»); dopo, la scheda **resta a video** con i comandi spenti — sparire sarebbe stato togliere all'utente la cosa che ha appena guardato* *A **T7c** (2026-08-18) si accende anche **«Brainstorm»**, che stava lì spento da T4 con scritto per quale tappa. Le condizioni sono tre e ognuna dice qualcosa: serve un **confronto già fatto** (prima non ci sarebbe niente di cui parlare, e il prompt vuole i giudizi), serve l'**AI** — a differenza dei documenti, che si riaprono anche senza — e la candidatura non deve essere **scartata**, per la stessa ragione per cui non le si scrive più un CV. Premendolo si va in P5, che per l'occasione fa l'altro dei suoi due mestieri* *A **T9c** (2026-08-21) arrivano due gesti che mancavano. Il primo è **«Com'è andata…» (L2)**, accanto agli altri comandi: apre un menù con «In attesa — nessuna risposta», poi i tre esiti — Colloquio · Rifiutata · Assunto 🎉 — e la spunta su quello di adesso (cap. 07.3). È acceso **solo da «inviata» in poi**, perché prima non c'è niente che possa essere andato in un modo o nell'altro, e non chiede conferma: un esito si disfa con un secondo clic sullo stesso menù, e la conferma resta dov'è servita — sullo scarto. Il menù si apre **sopra** il bottone: da lì in giù finirebbe fuori dalla finestra. Il secondo gesto non è un bottone nuovo ma un mestiere in più di «Analizza», che sulla candidatura riaperta **ferma al solo annuncio** diventa **«Confronta»** e fa il secondo passo da solo — è il vicolo cieco trovato dal collaudo di T8 (oggi quelle candidature le sa creare solo il server MCP, cap. 09.3): l'annuncio era già strutturato e rileggerlo sarebbe costato una chiamata per riottenere quel che c'era. Un testo incollato nella casella ha la precedenza e il bottone torna «Analizza», perché chi scrive lì vuole un annuncio nuovo* |
| **P5 Dialogo** | conversazione | pannello chat riusato per tre scopi: dialogo guidato del profilo, sessione di aggiornamento, brainstorming sull'opportunità; schede di conferma inline per i turni del profilo. Bolle a destra per l'utente e a sinistra per l'assistente, casella multiriga (**Invio manda, Maiusc+Invio va a capo**) e tre bottoni di scelta; l'attesa dell'AI **non è annullabile** — una mossa a metà lascerebbe il dialogo in uno stato che non esiste. Uscire non azzera il dialogo, che si riprende dov'era: ad azzerarlo è solo «Ricomincia». *(Sei decisioni di T3c, 2026-08-07.)* *Dalla revisione adversariale (2026-08-09): a dialogo concluso la fascia della risposta **si ritira** — niente zona morta sotto l'ultima bolla — e durante una chiamata si blocca anche la barra di navigazione (cap. 02.6)* *A **T7c** (2026-08-18) il pannello fa davvero i due mestieri che questa riga prometteva dall'inizio, e senza controlli nuovi: cambiano titolo, sottotitolo e i nomi dei tre comandi in fondo — «Torna alla candidatura» e «Trasforma in appunti» al posto delle porte del profilo — e in ogni momento **ne è vivo uno solo**. Le differenze del ragionamento stanno tutte in ciò che lo streaming si porta dietro: la bolla dell'assistente **cresce** mentre il testo arriva invece di comparire finita; «Invia» durante l'attesa **diventa «Interrompi»**, perché lì la mano sta già e c'è qualcosa da fermare (02.6); non ci sono schede né bottoni di scelta, perché non c'è nessuna macchina a mosse — si scrive e basta. «Trasforma in appunti» resta **spento finché l'utente non ha detto la sua**: distillare una conversazione in cui ha parlato solo l'AI costerebbe un'attesa per farsi rispondere una lista vuota. Un difetto trovato guardando e non misurando, il giorno stesso: i due comandi rinominati erano larghi quanto le etichette **di prima**, e a video si leggeva «Torna alla» — ora la misura segue il testo, senza scendere sotto quella del disegno* *Dal 2026-08-18 il pannello dice anche l'altra metà di quello che il cap. 02.5 gli affidava: una risposta fermata dal **tetto dei token** lascia a video il testo arrivato e sotto la riga «(fermata qui: ha raggiunto il limite di lunghezza)», gemella del «(interrotto)» che segue l'interruzione dell'utente — una frase che si spezza senza dirlo sembra una frase finita* |
| **P6 Documenti** | anteprima e rifinitura | anteprima del CV e della lettera affiancate all'annuncio (per il 📄 CV-1 base, generato senza annuncio, la colonna annuncio resta vuota); scelta lingua IT/EN; prima/dopo della rifinitura anti-slop; bottoni: «Esporta DOCX» «Esporta PDF» (L2), «Prepara email» (L3). *A T4 (2026-08-10) il pannello nasce **intero nella struttura e parziale nelle funzioni**: anteprime ed esportazioni funzionano, mentre la scelta di lingua e il prima/dopo dell'anti-slop sono lì spenti con il loro tooltip «arriva a T7», e «Prepara email» spento con «arriva a T6». È la regola 3.8 applicata al pannello che più di ogni altro mostra dove il progetto sta andando* *A T6 (2026-08-14) **«Prepara email» si accende** e porta a P7: restano spenti i due comandi di T7* *A T7a (2026-08-15) si accende la **tendina della lingua**, e a T7b (2026-08-18) la casella del **prima/dopo**, che si abilita quando c'è un confronto da mostrare — spuntarla mette in coda a ogni colonna i campi cambiati, prima e dopo, e non tocca ciò che si esporta (cap. 08.4). Il pannello non ha più comandi spenti in attesa di una tappa* *A T7d (2026-08-18) il pannello impara a **rileggere il 📄 CV-1 base da disco** invece di rigenerarlo a ogni visita: «rientrare non rigenera» valeva per le candidature e non per lui, e la conseguenza era che i due bottoni d'esportazione restavano spenti su un CV che esisteva — per riesportarlo bisognava rifarlo, e senza AI non si poteva affatto. Ora il pannello dice **di quando è** il CV che mostra e, se il profilo è cambiato da allora, lo dichiara invece di rifarlo di nascosto (cap. 11.1). Dalla stessa tappa la **tendina della lingua è accesa anche sul CV-1 base**, con la stessa semantica che ha sulla candidatura — cambiarla lo riscrive, previa conferma — perché la lingua è una proprietà del documento e il CV-1 base è un documento (cap. 10.1)* *A T9d (2026-08-22) compare **«Modifica i testi» (L4)**, fra «Rigenera» e i due export: apre una finestra con l'elenco dei campi di **prosa** — sommario, descrizioni delle esperienze, corpo della lettera — la casella per riscrivere quello scelto e il «Ripristina il testo non rifinito», acceso sui soli campi che l'anti-slop aveva cambiato. È l'ultima delle tre cose che il cap. 08.4 prometteva davanti al prima/dopo. **Le tre caselle del pannello restano in sola lettura**, e non è una rinuncia: mostrano la pagina di blocchi che finirà nel DOCX e nel PDF (cap. 05.3), non il documento — renderle scrivibili vorrebbe dire ricostruire il JSON da un testo impaginato che si porta dietro anche il prima/dopo. Si riscrive **solo la prosa scritta dall'AI**: i fatti — nomi, aziende, date, competenze, titoli — vengono dal profilo, e cambiarli qui li farebbe divergere in silenzio da chi li custodisce. Quel che si scrive entra nel documento al «Salva» e finisce **subito** su disco; da lì lo trovano gli export, l'email di P7 e i tool del server, che leggono lo stesso JSON. Il bottone è spento mentre l'AI lavora, come tutta la fascia, e su un documento che di prosa non ne ha* |
| **P7 Email** | composizione | destinatario, oggetto, corpo, elenco allegati (con quelli suggeriti dalla cartella documenti), bottone **«Prepara l'email»** (L3): scrive il file `.eml` e lo apre nel programma di posta predefinito. Al ritorno, la domanda «l'hai spedita?» per aggiornare il registro. *Nella 1.0 non esiste un bottone «Invia»: a spedire è il programma di posta dell'utente (cap. 07).* *Costruito a T6 (2026-08-14). Nella fascia dei comandi, a sinistra: «◀ Torna ai documenti» (L2), «Fallo riscrivere» (L2) e **«Documenti da allegare…»** (L2), che apre il giro della cartella documenti (cap. 05.2); a destra «L'ho spedita» (L1) e «Prepara l'email» (L3). Il destinatario resta **vuoto** finché non lo scrive l'utente. L'elenco «Cosa allego» mette prima i documenti generati per questa candidatura — il PDF già spuntato, il DOCX spento — e poi gli **attestati** della cartella documenti, spenti e marcati «(dai tuoi documenti)», perché in una lista sola convivono file nati un minuto fa e file che l'utente ha da anni. «L'ho spedita» è spento finché il messaggio non è stato preparato: un `.eml` che non esiste non può essere partito* *A T7b (2026-08-18) il pannello impara a dire una cosa in più: se la bozza ripresa è in una **lingua diversa** da quella dei documenti — succede cambiando la tendina di P6 dopo aver già preparato l'email — al posto di «Bozza ripresa da dove l'avevi lasciata» compare l'avviso, che nomina le due lingue e manda a «Fallo riscrivere». Il messaggio **resta quello di prima**: riscriverlo da sé cancellerebbe un testo che può essere già passato per le mani dell'utente, ed è la stessa ragione per cui una bozza salvata non viene mai sovrascritta all'arrivo (cap. 07.1)* *Dal 2026-08-18 la bozza si salva anche uscendo dalla **barra di navigazione**, e non più solo dal «◀ Torna ai documenti»: prima da lì si perdeva in silenzio (3.8). Mentre l'AI scrive non si salva niente — una bozza a metà messa sopra quella buona sarebbe peggio del male* |
| **P8 Impostazioni** | finestra separata | chiave API (mascherata), cartella dati, cartella documenti, modelli AI, lingua predefinita output, interruttore della rifinitura anti-slop (cap. 08.4), gestione del pool («Sigilla pool», dettaglio dei file modificati — cap. 04.5), export/import backup (cap. 11.4), pulizia dati («Svuota dati di navigazione», eliminazioni — cap. 11.5). *Niente sezione «account di posta»: l'app non spedisce.* *Resta di T9, ma due delle sue voci hanno già una casa provvisoria da T6: la **chiave API** si digita nella finestra del primo avvio e si rifà riavviando con `--chiave`, la **cartella documenti** si sceglie da P7. Quando il pannello arriverà, le assorbirà entrambe — e sarà lui il posto naturale, perché sono configurazione e non passi di un flusso* *A T9a (2026-08-21) anche la voce **export/import backup** ha trovato casa prima del suo pannello: la finestra c'è, si apre da P2, e quando P8 arriverà la **richiamerà** invece di rifarne una che le somiglia — come già la `FinestraConfermaCritica` fa con le eliminazioni* **Costruito a T9b** *(2026-08-21)*: la finestra c'è e il bottone della barra, spento da mesi, si accende. **Non ha OK né Annulla, ma un solo «Chiudi»**: le preferenze si scrivono appena si cambiano, perché ogni voce qui dentro si disfa con un secondo clic e uno stato «cambiato ma non salvato» sarebbe solo un tranello in più; le cose che invece non si disfano — le due pulizie — hanno la loro conferma prima di partire, che è dove la difesa serve. **Richiama e non rifà**, come il capitolo prometteva: la chiave passa dalla finestra del primo avvio, il backup da quella di T9a, l'eliminazione totale dalla `FinestraConfermaCritica`. Tre voci previste sono però cambiate di natura, e ognuna per una ragione sua: la **cartella dati** si mostra e si apre ma non si sposta (il lucchetto è preso all'avvio, cap. 09.4 — v. cap. 11.1); i **modelli** e il **pool** si leggono e basta, perché `modelli.json` esiste apposta perché cambiarli costi una riga e non una nuova build (cap. 11.6) e il manifest del pool si sigilla dal repo, non da un eseguibile distribuito (cap. 04.5); la **cartella documenti** si vede qui ma si gestisce in **P7**, dove quel giro sa aspettare l'AI e annullarla (cap. 05.2) — le Impostazioni ci mandano, come P2 manda in P3 per l'import da un sito. Delle eliminazioni del cap. 11.5 P8 tiene le due **generali**; quella di una singola opportunità sta dove l'opportunità si guarda* *A **T9c** (2026-08-21) si aggiunge la sezione **«Candidature spedite»**, con l'unica preferenza che quel giro chiedeva: «Ricordamele se non rispondono entro N giorni», quattordici di casa, **zero per spegnere** il promemoria (cap. 07.3). Sta fra i documenti e le cartelle perché è una preferenza come la lingua — si scrive appena si cambia, e vale subito: la Home la rilegge alla prima occhiata, senza riavvio* |

Ogni pannello ha in alto il **titolo in `RossoTitoli`** e un sottotitolo grigio che dice
a che punto del flusso siamo («Passo 2 di 4 — Confronto»).

## 3.7 Naming dei controlli (per il designer)

Prefissi standard, nome semantico in PascalCase: `pnl` (Panel), `btn` (Button),
`lbl` (Label), `txt` (TextBox), `cmb` (ComboBox), `chk` (CheckBox), `pic` (PictureBox),
`grp` (GroupBox), `lst`/`lvw` (liste), `wv` (WebView2), `cms` (menu contestuale),
`tmr` (Timer), `num` (NumericUpDown, da T9c). Esempi: `btnCatturaAnnuncio`, `lblStelleMatch`, `wvRicerca`,
`pnlLogo`, `lblVersione`.

## 3.8 Feedback e stati

- **Badge di stato** (pannellino 115×26 con etichetta bold centrata): verde OK, azzurro
  info, giallo attenzione, rosso errore — usati per lo stato delle opportunità e delle
  chiamate AI.
- Operazioni AI in corso: indicatore nella barra di stato + testo in streaming dove
  previsto (cap. 02); mai una finestra bloccata.
- Errori: messaggio in italiano nel contesto in cui è avvenuto (non solo un popup),
  sempre con un'azione possibile («Riprova», «Salta»). *Dal 2026-08-09 c'è anche
  l'ultima rete: un gestore globale delle eccezioni in italiano (in `Programma`),
  perché nemmeno l'imprevisto vero deve mostrare la finestra di crash di .NET.*
- **I bottoni delle tappe non ancora arrivate si vedono, spenti, con un tooltip che dice
  in che tappa arriveranno** — non si nascondono. Chi guarda l'applicazione a metà strada
  deve capire dove sta andando, e un bottone che sparisce e ricompare è più confondente di
  uno spento che si spiega. Stessa regola per ciò che manca all'avvio: senza chiave API il
  bottone del dialogo è spento e il motivo è scritto, non lasciato indovinare (cap. 02.5).
  *(Deciso in T3c, 2026-08-07.)*
- **Il motivo per cui un bottone è spento si scrive sotto quel bottone**, non altrove
  nella schermata. *(Imparato in T4c, 2026-08-10.)* Il motivo per cui «Analizza» non si
  poteva premere c'era, ed era corretto — ma stava nell'angolo opposto dello schermo,
  dove nessuno guardava: chi voleva premere il bottone stava guardando il bottone, e
  l'applicazione sembrava rotta. Una spiegazione che non si trova non è una spiegazione.
- **Un contatore concorda la parola col numero.** *(Imparato in T5c, 2026-08-13.)* I
  contatori della Home dicevano «1 scartate», perché il numero era una variabile e la
  parola no. Il caso «uno» sembra il meno importante — è quasi sempre più di uno — ma
  capita proprio la **prima volta** che l'utente fa una cosa, cioè quando guarda con più
  attenzione, e una stonatura lì fa sembrare improvvisato tutto il resto.
- **Un pannello che torna in vista rilegge le proprie condizioni.** *(Imparato in T4c,
  2026-08-10.)* I pannelli dell'area centrale nascono all'avvio e restano vivi, nascosti,
  per tutta la sessione: ciò che hanno chiesto al motore la prima volta può non essere più
  vero. È successo con P4, che continuava a dire «prima serve il tuo profilo» a un
  profilo appena importato e salvato — proprio sul percorso del primo avvio, dove quel
  messaggio è l'unica cosa che l'utente legge.
- **E un pannello che cambia soggetto si svuota, non si limita a riempirsi.**
  *(Imparato il 2026-08-18.)* È il gemello della voce qui sopra, con un difetto peggiore
  dentro: P7 arrivava su una candidatura nuova e scriveva oggetto e corpo **solo quando
  l'AI glieli dava**, ma quella scrittura ha due uscite anticipate legittime — manca la
  chiave, manca la lettera. In quei due casi restava a video il messaggio della
  candidatura **di prima**, sotto il nome di questa, e finiva nel suo `email.json` al
  primo salvataggio. Chi arriva azzera **tutto** ciò che riempirà a condizione, non solo i
  campi che sa già di dover cambiare: un campo lasciato com'era non è «vuoto», è pieno di
  roba di qualcun altro.
- **Un pannello che si lascia mette al sicuro quello che ha in mano.** *(Imparato il
  2026-08-18.)* Da un pannello si esce in due modi: dai **suoi** bottoni, che lui
  controlla, e dalla **barra di navigazione** in cima, che non passa da lui. Chi salvava
  solo sul primo perdeva tutto sul secondo, e **in silenzio**: la volta dopo il pannello
  rilegge il disco e mostra la versione vecchia come se fosse l'ultima. L'ha insegnato la
  bozza dell'email in P7 — destinatario scritto a mano, spunte degli allegati e un
  messaggio riscritto che era costato una chiamata all'AI. Ora la finestra principale,
  prima di nascondere un pannello, chiede a chi ha del lavoro in sospeso di metterlo al
  sicuro (`IPannelloCheSalvaUscendo`), e a dichiararlo è **solo chi ce l'ha** — oggi il
  solo P7 — perché un metodo vuoto ripetuto in sei pannelli non direbbe a nessuno chi
  lavora davvero. Non è la conferma d'uscita del cap. 11.5: lì si domanda e si può
  rispondere di no, qui non si chiede niente — quel che l'utente ha scritto è suo e si
  salva.
