# 08 — Qualità del testo (anti-slop)

*Un CV o una lettera che «sanno di intelligenza artificiale» danneggiano il candidato.
Questo capitolo definisce come i testi in prosa vengono resi naturali — senza toccare i
fatti e senza trucchi.*

## 8.1 Il problema

I testi generati dai modelli hanno tic riconoscibili: lineette lunghe (—) ovunque,
elenchi puntati non richiesti, frasi tutte della stessa lunghezza e tutte bilanciate
(«tuttavia», «è importante notare»), formalismo da manuale, paragrafi simmetrici. Un
selezionatore li riconosce a colpo d'occhio; nel migliore dei casi suonano freddi, nel
peggiore squalificano la candidatura.

## 8.2 Dove si interviene (e dove no)

| Testo | Rifinitura? |
|---|---|
| **Campi-prosa**: sommario del CV, descrizioni delle esperienze, lettera, corpo email | **Sì** |
| **Campi-fatto**: nomi, aziende, date, titoli, elenco competenze, recapiti | **No, mai** — sono ricopiati dal profilo e non si toccano |

La rifinitura è un **passo separato** dalla generazione (principio del compito
ristretto: un prompt = una cosa). La generazione pensa al *cosa dire* rispettando il
profilo; la rifinitura pensa solo al *come suona*.

## 8.3 I prompt di rifinitura (`umanizzazione_*`)

Derivano da un prompt di casa già collaudato per l'umanizzazione dei testi, adattato a
questo prodotto. *A T7b (2026-08-18) il prompt unico è diventato **tre**, uno per genere
di prosa — v. 8.6.* Le regole valgono per tutti e tre:

1. **Rimuovere i marcatori tipici dell'AI**: lineette lunghe (sostituite da virgole,
   punti o riformulazioni), elenchi puntati non necessari, formule fatte («in sintesi»,
   «è importante notare che», «vale la pena sottolineare»).
2. **Ritmo naturale**: alternare frasi lunghe e corte; connettivi semplici dove servono;
   non ogni frase deve essere perfettamente bilanciata.
3. **Togliere il formalismo eccessivo**: niente disclaimer continui, niente «tuttavia»
   a ogni affermazione.
4. **Rompere la simmetria**: paragrafi di lunghezza diversa, come scrive una persona.
5. **⛔ NESSUN errore di battitura.** Alcune tecniche di umanizzazione prevedono
   micro-refusi per «credibilità»; qui sono **esplicitamente esclusi** (decisione del
   mandato): un CV o una lettera di candidatura devono essere impeccabili. La
   naturalezza si ottiene con ritmo e lessico, non con gli errori.

**Vincolo di sostanza** (cablato nel prompt): riformulare **senza aggiungere né
togliere informazioni** — niente fatti nuovi, niente enfasi che il profilo non
sostiene, niente omissioni. La rifinitura è un cambio di forma; l'anti-invenzione vale
anche qui.

Il prompt riceve la **lingua di destinazione** come parametro (i tic da correggere in
inglese non sono identici a quelli italiani). *A T7b la lingua non è un parametro dentro
il prompt ma la **variante del prompt**, `.it` / `.en`, come per gli altri documenti dal
Pool 1.06: è la stessa lezione — una regola scritta dentro un testo che detta una forma
perde contro la forma.*

## 8.4 Il controllo dell'utente

- Il pannello Documenti (P6) mostra **prima / dopo** per ogni campo rifinito;
  l'utente può accettare, modificare a mano o tornare alla versione non rifinita.
  *A T7b il prima/dopo c'è (casella «Mostra il prima/dopo della rifinitura»); la
  **modifica a mano** campo per campo no — è rimandata, v. 8.6 e `in_sospeso.md`.*
  *A **T9d** (2026-08-22) ci sono anche le altre due, e vivono in una **finestra** aperta
  dal bottone «Modifica i testi» di P6: l'elenco dei campi di prosa, la casella per
  riscrivere quello scelto e il «Ripristina il testo non rifinito», acceso sui soli campi
  che l'anti-slop aveva cambiato. Tre scelte che il capitolo prometteva insieme e che
  insieme adesso ci sono — accettare è non toccare niente.*
- **Si riscrive la prosa, non i fatti** *(T9d)*. I campi aperti alla modifica sono quelli
  che l'anti-slop tocca — sommario, descrizioni delle esperienze, corpo della lettera — e
  a dire quali siano è la stessa classe che li rifinisce (8.2): una seconda lista
  nell'interfaccia divergerebbe al primo campo nuovo. Nomi, aziende, date, competenze e
  titoli restano fuori: vengono dal profilo, e riscriverli qui li farebbe divergere in
  silenzio da chi li custodisce — per poi tornare com'erano alla prima rigenerazione.
  L'**anti-invenzione non c'entra**: quel vincolo tiene la macchina dentro i fatti
  dichiarati, e qui a scrivere è l'utente, che dei suoi fatti è il proprietario.
- **Le tre caselle di P6 restano in sola lettura, ed è il motivo per cui la finestra
  esiste** *(T9d)*. Non mostrano il documento: mostrano la **pagina di blocchi** che
  finirà nel DOCX e nel PDF (cap. 05.3), etichette di sezione comprese, e in coda il
  prima/dopo, che documento non è. Renderle scrivibili vorrebbe dire ricostruire il JSON
  da quel testo — un mestiere che nessuna delle tre stampanti fa, e che sbaglierebbe
  proprio dove l'utente si fida di più.
- **Dove finisce quel che si riscrive** *(T9d)*. Nel documento, al «Salva»: annullando,
  la finestra muore con dentro tutto quello che si era scritto. Da lì il testo si salva
  **subito** su disco — fra la modifica e la prossima azione si può chiudere
  l'applicazione, e un lavoro perso in silenzio è peggio di un lavoro non offerto — e non
  serve avvisare nessun altro: export, email di P7 e tool del server leggono lo stesso
  JSON (cap. 05.6, cap. 09.3). Il **📄 CV-1 base risalvato non rinasce**: la versione di
  profilo da cui viene e la data in cui fu scritto restano quelle, o il pannello direbbe
  «l'ho scritto oggi» di un documento di ieri, e smetterebbe di avvertire che il profilo
  è cambiato da allora (cap. 11.1). E **«Rigenera»** — come il cambio di lingua, che passa
  di lì — avverte che a sparire saranno anche le modifiche fatte a mano: di un testo
  scritto dall'AI si sa che si rifà premendo di nuovo, di uno scritto a mano no.
- **Il confronto dice «prima» e «adesso»** *(T9d)*, non più «prima» e «dopo». Il secondo
  termine è il documento **com'è in questo momento**: se in mezzo è passata anche la mano
  dell'utente, «adesso» resta vero e «dopo la rifinitura» no. Il «prima» invece non si
  tocca mai — resta il testo da cui l'AI era partita, ed è quello che il «Ripristina»
  rimette nella casella.
- La rifinitura si può disattivare (Impostazioni), ma è attiva di default: è parte
  dell'identità del prodotto. *A T7b è sempre attiva: l'interruttore vive in P8, che è
  di T9.* *E a **T9b** (2026-08-21) l'interruttore c'è, in P8: «Rifinisci i testi generati
  (anti-slop)». Vale **subito**, senza riavviare — la rifinitura non riceve un valore
  copiato all'avvio ma una domanda da rifare a ogni giro — e vale da **tutte e due le
  porte**, finestra e server MCP, perché il cap. 09.3 vuole che il CV chiesto da un client
  sia lo stesso che esce dalla finestra: un interruttore valido solo di qua li farebbe
  divergere proprio sul testo. Spenta, i documenti escono col testo grezzo, che è quel che
  già succede quando una rifinitura fallisce; e non si chiama l'AI affatto, perché
  interrogare il modello per poi buttarne via la risposta costerebbe soldi e tempo a chi
  l'ha spenta apposta.*
- Regola pratica ereditata dal metodo del progetto: se il prima/dopo rivela che la
  rifinitura ha cambiato un fatto, è un difetto del prompt da correggere nel pool, non
  un caso da sistemare a mano in silenzio.
- **La rifinitura non è una funzione del pannello** *(T8b, 2026-08-19)*: anche i documenti
  chiesti dal **server MCP** ci passano, anti-slop compreso (cap. 09.3). Chi chiede da
  fuori riceve gli stessi testi di chi preme il bottone — una differenza di qualità fra le
  due porte non l'avrebbe dichiarata nessuno, e si sarebbe scoperta mesi dopo confrontando
  due documenti senza capire perché uno è più piatto.

## 8.5 A monte: generare già sobrio

La miglior difesa contro lo slop è non produrlo: i prompt di generazione già impongono
prosa asciutta e in prima persona («se il profilo è scarno, il sommario è breve»).
La rifinitura è il secondo filtro, non una scusa per generare male. Se in collaudo un
tic ricorrente emerge già in generazione, si corregge **quel** prompt (e si annota nel
CHANGELOG del pool).

## 8.6 Com'è stata costruita (T7b, 2026-08-18)

**Tre prompt e non uno** (Pool 1.08, cartella `rifinitura/`). Il capitolo ne prevedeva
uno; l'inventario dei campi ne ha trovate **tre forme incompatibili**: il sommario del CV
(prima persona, sintetico), le descrizioni delle esperienze (**frasi nominali** — «Servizio
ai tavoli e gestione della cassa»), il corpo di lettera ed email (prosa distesa). Su una
frase nominale «alterna frasi lunghe e corte» è un'istruzione *sbagliata*: la farebbe
diventare una frase. Un prompt unico avrebbe contenuto tutte e tre le forme lasciando al
modello la scelta di quale imitare — l'errore già pagato col Pool 1.05 e col 1.07. Si paga
con una chiamata in più (il CV ne fa due), ed è stata una scelta fatta sapendo il prezzo.

| Prompt | Su cosa lavora |
|---|---|
| `umanizzazione_sintesi` | il `sommario` del 📄 CV-1 e del 🎯 CV-2 |
| `umanizzazione_frasi` | le `descrizione` delle esperienze, formali e informali |
| `umanizzazione_prosa` | il `corpo` della ✉️ lettera e quello dell'📧 email |

**L'anti-invenzione è strutturale, prima ancora che scritta.** Al modello arrivano **solo i
campi-prosa**: nomi, aziende, date, competenze e titoli non entrano nella richiesta, e quel
che non entra non può tornare cambiato (`Motore/Rifinitura`). Dentro i prompt, la regola
della sostanza si dichiara **sovraordinata** a ogni regola di stile — «se per rifinire
dovresti violarla, non rifinisci» — e la guardia più specifica è **non cambiare il grado**:
«ho collaborato a» che diventa «ho gestito» è un fatto inventato. Nella prosa se ne
aggiunge una che nasce da qui: dove il testo **riconosce onestamente un gap**, quel
riconoscimento resta e resta chiaro — le mitigazioni non si annacquano.

**Un passaggio da cui un documento non può uscire peggiore.** Se l'AI dimentica un pezzo,
ne inventa uno, lo restituisce vuoto o risponde in una forma illeggibile, resta il testo di
partenza. E se la rifinitura **fallisce del tutto**, la generazione non fallisce con lei: il
documento grezzo è buono, e buttare via un CV già scritto per un inciampo del secondo filtro
sarebbe sproporzionato. L'annullamento invece passa: è l'utente che ha chiesto di smettere.

**Dove si incastra.** Dentro la pipeline dopo ciascun documento — la fila passa da 4 a **6
passi** — così la lettera riceve il 🎯 CV-2 **già rifinito** e le due voci non divergono.
Il 📄 CV-1 si rifinisce in P6 (non passa dalla pipeline) e il corpo dell'email in P7.

**Quel che resta fuori, di proposito.** L'apertura, la chiusura e la firma della lettera, e
l'**oggetto** dell'email: non sono slop, sono formule che il lettore si aspetta — e
l'oggetto è quello che il Pool 1.07 ha appena dettato parola per parola nelle due lingue.

**Il «prima» sta accanto al documento, non dentro.** Nello `stato.json` dell'opportunità
(campo `rifinitura`) e nell'involucro di `cv_base.json`: solo i campi davvero cambiati, e
niente affatto se non è cambiato nulla. Dentro il documento no, perché il 🎯 CV-2 finisce
nel prompt della lettera e la lettera in quello dell'email — un campo di servizio ci
arriverebbe insieme a loro.

**Quel che il capitolo prometteva e T7b non ha fatto** (→ `in_sospeso.md`): la **modifica a
mano** campo per campo in P6 (le tre caselle sono `ReadOnly`: è disegno di P6, non
anti-slop) e l'**interruttore** per disattivare la rifinitura, che vive nelle Impostazioni
P8 di T9. Fino ad allora la rifinitura è sempre attiva, e la via d'uscita è il prima/dopo.
*Delle due, l'interruttore è arrivato con **T9b** (2026-08-21), insieme al pannello che lo
ospita (v. 8.4); la **modifica a mano** con **T9d** (2026-08-22), in una finestra a parte —
perché le tre caselle del pannello mostrano la pagina impaginata, non il documento (v. 8.4).
Il capitolo non ha più niente in sospeso.*

## 8.7 Cosa ha insegnato l'AI vera (collaudo di T7b, 2026-08-18)

I sei prompt erano verdi al banco, ma **i collaudi coi finti non caricano prompt**: quanto
valessero si è visto solo con un giro intero nelle due lingue (il racconto è nel diario,
Step 2.25; le correzioni nel CHANGELOG del pool, Pool 1.09). Il grosso ha retto — nessun
grado rafforzato, nessun fatto nuovo, frasi nominali intatte, gap onesti al loro posto, e
**quattro descrizioni su cinque restituite identiche**, che è la prova che il «permesso di
non cambiare» funziona. Restano tre regole di progettazione, che valgono oltre questi
prompt:

1. **Accanto a un divieto generale serve la forma concreta di ciò che vieta.** «Non
   togliere nulla» non ha impedito che sparisse la proposizione che legava un'esperienza al
   requisito scoperto: era un divieto astratto contro una frase contorta, e ha perso. Con
   l'esempio accanto — *una proposizione che lega un'esperienza a un requisito è un
   argomento, non un inciso* — ha tenuto. È la lezione del Pool 1.05 e del 1.07 applicata
   ai divieti invece che alle formule.
2. **Una lista di parole vietate può cancellare un fatto.** `end-to-end` era finito fra i
   riempitivi inglesi, e in un CV tecnico è un termine preciso: la rifinitura l'ha tolto e
   il documento ha detto meno di prima. Ogni elenco di «parole da togliere» va ora letto
   con la domanda che manca(va): *questa parola, in un mestiere, potrebbe essere un fatto?*
   La regola sta ora scritta in tutte e due le lingue.
3. **Fra due istruzioni che si contraddicono vince quella che parla del testo intero.** Le
   lineette lunghe venivano tolte a metà; rafforzare la regola che le vieta **non è
   bastato** — a batterla era il *permesso di non cambiare*, che riguarda tutto il testo,
   mentre l'elenco dei tic riguarda un dettaglio. La cura è stata scrivere l'eccezione
   dentro il permesso. Da qui la regola pratica: quando un prompt concede qualcosa in
   generale e vieta qualcosa in particolare, le due cose vanno messe **nello stesso posto**,
   o la concessione vince.

*E una regola di metodo, che vale per il pool intero:* una cura a un prompt **non è fatta
finché non è riprovata con l'AI vera**. La terza delle tre qui sopra sembrava chiusa dopo la
prima correzione, e non lo era: la riprova ha mostrato la lettera tornata identica, con
tutte le lineette al loro posto.
