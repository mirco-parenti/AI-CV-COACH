# Revisione di finalizzazione — traccia di lavoro

*Aperta il 2026-09-01 sul ramo `feature/finalizzazione`. Documento di lavoro della
revisione condotta con il tutor (Riccardo) sul prodotto completato da Mirco: raccoglie
perimetro, decisioni e rilievi delle tre fasi, e sostiene la pull request finale che
Mirco valuterà e integrerà. Non è documentazione del prodotto: a revisione chiusa e PR
integrata, questo file può sparire o diventare uno `situazione-*.txt`.*

## Processo concordato

1. **Brainstorming** per fase → decisioni esplicite → **verifica** dei rilievi →
   **nessun fix senza approvazione**, voce per voce.
2. Al termine delle tre fasi: pull request verso `main`; **il merge lo fa Mirco**
   (regola 11 di progetto).
3. Commit firmati solo `(c) 2026 Aviolab AI` (regola 12).

---

## Fase 1 — Revisione di sicurezza

### Perimetro e threat model (DECISI)

- **Perimetro completo**: prodotto `VB.NET/src/`, strumenti di sviluppo (`strumenti/`),
  prototipo `HTML+JS/` (congelato: eventuali fix sono voce a parte della PR, li accetta
  Mirco).
- **Threat model**: macchina Windows mono-utente fidata. Un altro utente della stessa
  macchina è fuori scope (DPAPI `CurrentUser` copre già la chiave). Avversari
  considerati: contenuto web ostile (annunci), file CV ostili, output del modello,
  rete (MITM), client MCP.

### Cosa è risultato solido (verificato, nessun intervento)

- Chiave API: DPAPI scope utente, mascheratura, redazione regex nel `DiarioTecnico`,
  nessuna chiave nel repo.
- Rete: solo HTTPS in uscita, zero bypass TLS; il server MCP del prodotto è **stdio**,
  non apre porte.
- Input: PDF non parsato localmente (base64 all'API); DOCX letto per entry nominata
  (no zip-slip), `XDocument` con DTD proibite; HTML generato passa da `HtmlEncode`;
  percorsi da contenuti AI sempre sanitizzati (`Sillabario`, `NomiDocumenti.Pezzo`,
  guardie anti-traversal in `Mcp/ToolDiLettura.vb:181` e `ArchivioOpportunita`).
- Processi: nessuna shell, nessun argomento da input non fidato nel prodotto.

### Rilievi

| # | Dove | Rilievo | Gravità proposta | Verifica | Decisione fix |
|---|------|---------|------------------|----------|---------------|
| R1 | `Mcp/ServerMcp.vb:219`, `:378`, `Annota` a `:516` | Eccezioni complete (`{ex}`) scritte su stderr **senza** passare da `DiarioTecnico.SenzaSegreti`: la redazione esiste solo nel diario su file, non nel diario stderr che il client MCP raccoglie nei suoi log. Possibili percorsi e frammenti di contenuto in log altrui. | Media | ✅ Confermato alla fonte | da decidere |
| R2 | `Web/LettorePagina.vb` → prompt → sink dell'output AI | Prompt injection dal web: testo di annunci ostili entra nei prompt senza sanitizzazione semantica. Raggio d'azione mappato sink per sink il 2026-09-01 (v. `revisione-sicurezza.md`): percorsi, HTML/PDF, DOCX, EML, UI e archivio tutti chiusi; i prompt del pool delimitano già il testo non fidato. Residui non codificabili: inganno sul contenuto (mitigato da anti-invenzione + revisione umana) e iniezione di secondo ordine verso il client MCP (da documentare, si fonde con R3). | Bassa (documentare) | ✅ Verifica completata | da decidere |
| R3 | `Mcp/CatalogoTool.vb`, `ToolDiScrittura.vb` | Tool MCP di scrittura senza conferma umana (`esporta_backup`, `genera_*` che spendono token). **Ridimensionato in verifica**: `esporta_backup` scrive solo dentro la cartella dati (`FileLibero`), non in destinazioni scelte dal client. Resta il confine di fiducia da documentare. | Bassa (documentare) | ✅ Verificato | da decidere |
| R4 | `strumenti/mcp-collaudi/server.mjs` (`esegui()` a :80; interpolazioni a :278, :557, :725) | `spawn("bash", ["-lc", …])` con argomenti dei tool interpolati nella stringa → command injection; server HTTP su 127.0.0.1:3300 senza autenticazione. Strumento di sviluppo locale, rischio reale basso, fix semplice (argomenti senza shell). | Bassa | ✅ Confermato alla fonte | da decidere |
| R5 | `HTML+JS/server.js:1248` | `server.listen(PORT)` senza host → bind su **tutte le interfacce**, senza autenticazione: chiunque raggiunga la porta 3000 consuma la chiave API. Prototipo congelato: fix di una riga (`127.0.0.1`), voce a parte della PR per Mirco. | Media (quando il prototipo gira) | ✅ Confermato alla fonte | da decidere |

**Note minori** (nessun intervento previsto): niente `packages.lock.json` (una sola
dipendenza NuGet); dati personali in chiaro in `%APPDATA%` è scelta dichiarata del
prodotto («l'utente padrone dei suoi dati»), al più da riflettere nella GUIDA.

### Fase 1 — stato (2026-09-01)

**Analisi completa**: la verifica di R2 è chiusa e il report finale della fase — con
due rilievi in più emersi in verifica (CR/LF nei nomi allegato EML, tetto allo slug),
lo stato dei fix già presenti nel working tree (R1, R5, EML, slug; banco verde
1335/1335) e il piano della fase di fix in quattro blocchi S1–S4 — è in
**`revisione-sicurezza.md`**.

**Fase di fix eseguita** *(2026-09-01, su mandato del tutor, tre agenti sotto
coordinamento)*: quattro commit su `feature/finalizzazione` — `4d3b4a9` (prodotto +
quattro collaudi falsificati), `dcc879e` (prototipo, voce a parte per Mirco),
`227fde8` (strumento di collaudo senza shell), `a1ff30f` (confine di fiducia nel
cap. 09 + GUIDA). Banco 1343/1343. La riserva sul giro dal vivo dello strumento di
collaudo è stata **chiusa il giorno stesso** dal giro a vista dei fix UI, passato tutto
dal codice senza shell (in `in_sospeso.md`, «Chiuse»): la Fase 1 è **chiusa senza
riserve**. Dettaglio in `revisione-sicurezza.md`, «Esito della fase di fix».

---

## Fase 2 — Revisione UI secondo criteri industriali

### Criteri e precedenze (DECISI)

- Metro: documento della skill `ui-guidelines` (66 regole + checklist).
- Le style guide di progetto (cap. `VB.NET/progetto/03_interfaccia_grafica.md`)
  prevalgono di norma su palette e branding, **ma in questa revisione sono
  potenzialmente correggibili**: dove divergono in modo evidente dallo standard
  industriale si decide **caso per caso** (direttiva del tutor, 2026-09-01).
- Conseguenza operativa: i rilievi si dividono in **due categorie**. La **A** sono
  difetti di implementazione — violano i criteri industriali *e spesso le stesse
  regole del progetto* — e si propongono per la correzione. La **B** sono divergenze
  *di sistema* fra le linee guida di progetto e lo standard: lì la linea guida è essa
  stessa in discussione, e ogni voce porta una raccomandazione da confermare.

### Metodo (2026-09-01)

Revisione condotta con tre agenti paralleli su tutto `VB.NET/src/TrovaLavoro/`:
infrastruttura di stile (`StileApp`, `BottoneMenu`, `FasciaDeiComandi`,
`FinestraDiCaricamento`, `FormPrincipale`, `Marchio`, `FiloGrafico`, `Programma`),
le 11 finestre di dialogo, gli 8 pannelli P0–P7 (sempre `.vb` + `.Designer.vb`).
46 rilievi grezzi, consolidati qui sotto (i doppioni fra agenti sono fusi).

### Punti solidi (nessun intervento)

- **Feedback e asincronia** (regole 5-8, 64): async/await ovunque nei flussi AI,
  scudo + barra a stima + fascia di stato su un filo unico, finestra layered che non
  ruba clic né fuoco. È il punto più forte dell'applicazione.
- **Token centralizzati**: tutta la palette in `StileApp.vb`, ogni colore con
  motivazione e misure di contrasto in commento; nei form nessun colore o font
  letterale. Due sole famiglie di font (regola 25 rispettata).
- Contrasto **curato al centesimo** dove il progetto ci ha messo mano
  (`TestoSecondario` corretto due volte); nomi accessibili dichiarati in P3.

### Rilievi — Categoria A (difetti di implementazione)

| # | Dove | Rilievo | Gravità | Decisione fix |
|---|------|---------|---------|---------------|
| U1 | `Ui/PannelloProfilo.vb:1206-1244` | L'import del CV (la chiamata AI più lunga di P2) non alza `LavoroAiCambiato`: niente scudo né fascia di stato, e la barra di navigazione **resta attiva** — si può uscire e avviare una seconda chiamata AI da un altro pannello. Contraddice il «filo unico» del cap. 03.8. | Alta | deciso — v. «Decisioni» |
| U2 | `Ui/FinestraImpostazioni.vb:676, 711`; `Ui/FinestraBackup.vb:93-94, 202` | Svuota cache WebView2, elimina tutto, esporta/ripristina backup: I/O potenzialmente di secondi **sincrono sul thread UI**, senza spinner né stato — la finestra congela (regole 6-7, 64). Il pattern async giusto esiste già lì accanto (prova chiave, elenco modelli). | Alta | deciso — v. «Decisioni» |
| U3 | `StileApp.vb:370-372, 410-413, 506-509`; `Ui/PannelloHome.vb:391, 607` | Contrasti sotto il 4,5:1 WCAG per testo piccolo: bianco su `Successo` **3,13:1** (bottoni L1 e casella «🎮 Menu»), bianco su `RossoTitoli` **4,10:1** (L6 Critico), `Informazione` usato come colore di *testo* **≈2,8:1** (promemoria e righe «da sollecitare» in Home). Il progetto stesso cura il WCAG al centesimo sulle didascalie: qui è sotto di decimi. | Alta | deciso — v. «Decisioni» |
| U4 | `Ui/FinestraImpostazioni.vb` (9 occorrenze), `Ui/FinestraChiaveApi.vb:146`, `Ui/PannelloRicerca.vb:910-912` + trasversale | Errori incoerenti e affidati al solo colore (regole 8, 18): Impostazioni/ChiaveApi li scrivono in `RossoTitoli` (il colore dei *titoli*, 4,1:1), P3 li scrive nel **grigio** delle didascalie (indistinguibili da «Ricerca salvata»), Backup usa correttamente `Pericolo`. Ovunque manca la parola «Errore»/icona. → Uniformare: `Pericolo` + prefisso testuale. | Media | deciso — v. «Decisioni» |
| U5 | `Ui/PannelloDocumenti.vb:2111-2141`, `Ui/PannelloEmail.vb:1104-1133` | In P6 e P7 le attese AI non sono annullabili da nessun controllo (`AnnullaIlLavoro()` esiste, nessuno lo chiama), mentre P2, P4 e P5 offrono «Annulla»/«Interrompi»; il commento di `FinestraDiCaricamento.vb:22` presuppone che «Annulla» sia premibile lì sotto. (P5-dialogo resta non annullabile **per progetto**: stato a mosse.) | Media | deciso — v. «Decisioni» |
| U6 | `Ui/PannelloEmail.Designer.vb:288-299`; `FinestraAppunti`, `FinestraDocumenti`, `FinestraModificaTesti` | DPI: P7 è l'**unico** pannello senza `AutoScaleMode = Font`; le tre finestre non applicano la correzione `ScalaSchermo` della decisione 15.7 — il difetto «testi crescono, finestra no» già curato altrove vive ancora lì (a 150% troncano). | Media | deciso — v. «Decisioni» |
| U7 | `Ui/PannelloRicerca.Designer.vb:131-256` (7 bottoni) | I bottoni della barra di ricerca di P3 sono alti **26 px**: fuori dal criterio industriale, fuori dal token locale (32) e fuori griglia. Unici fuori sistema negli otto pannelli. | Media | deciso — v. «Decisioni» |
| U8 | `Ui/PannelloProfilo.vb:882`, `Ui/PannelloRicerca.vb:409`, `Ui/PannelloOpportunita.vb:811` vs `Ui/PannelloHome.vb:698` | Conferme di pari livello 5 con due meccanismi: `FinestraConferma` (col verbo «Confermo») per «Elimina candidatura», `MessageBox` Sì/No di sistema per eliminazione voce profilo, «Dimentica» ricerca e «Scarta» — proprio ciò che la finestra di progetto è nata per evitare. | Media | deciso — v. «Decisioni» |
| U9 | `Ui/PannelloEmail.vb:1093` | «Fallo riscrivere» (L2, Esplorativo) riscrive oggetto e corpo **anche corretti a mano, senza conferma**; in P6 la stessa classe di azione è L4 con conferma che elenca i testi riscritti. | Media | deciso — v. «Decisioni» |
| U10 | `Ui/FinestraBackup.vb:407` | `btnRipristina` vestito `Distruttivo` (L5) mentre il suo stesso commento dice che non è una cancellazione: per la tabella 03.3 è `Attenzione` (L4, «modifica dati esistenti»). | Bassa | deciso — v. «Decisioni» |
| U11 | `Ui/PannelloRicerca.vb:993-997` | «Cattura annuncio» e «Importa CV da questa pagina»: **due verdi pieni identici adiacenti** per due azioni diverse — anche dentro il sistema dei livelli, due L1 gemelli affiancati annullano il principio «il colore dice la conseguenza». | Media | deciso — v. «Decisioni» |
| U12 | `Ui/FinestraImpostazioni.vb:813-814` | Due bottoni rossi pieni **adiacenti** (Distruttivo + Critico): contraddice il principio interno del progetto — «il vuoto intorno è la prima difesa» delle azioni di livello 6, che in fascia stanno su una riga tutta loro. | Media | deciso — v. «Decisioni» |
| U13 | `StileApp.vb:338-417, 485-516` | Hover/pressed mai definiti (`MouseOverBackColor`/`MouseDownBackColor`): sui fondi saturi ci si affida al default WinForms non calibrato; `BottoneMenu` fa già la cosa giusta (scurimento 18/36) ma da solo. | Bassa | deciso — v. «Decisioni» |
| U14 | `Ui/BottoneMenu.vb:76` | Unico raggio dell'app (6 px): valore fuori scala e non tokenizzato in `StileApp`. → 4 o 8 px, come token. | Bassa | deciso — v. «Decisioni» |
| U15 | `Ui/FinestraDocumenti.vb:130-148`, `Ui/FinestraModificaTesti.vb:764`, `Ui/PannelloHome.vb:426` | Stati vuoti non guidati (regola 60): lista documenti vuota senza messaggio; «Scegli una riga dall'elenco.» anche a elenco vuoto; coda Home vuota senza dire il gesto per cominciare. | Bassa | deciso — v. «Decisioni» |
| U16 | `Ui/PannelloDialogo.Designer.vb:155`, `Ui/PannelloRicerca.Designer.vb:247` | Placeholder come unica etichetta (regola 17) nella casella risposta di P5 e nella casella indirizzo di P3. | Bassa | deciso — v. «Decisioni» |
| U17 | `Ui/PannelloHome.Designer.vb:232-260` | Colonne «Match» e «Aggiornata» allineate a sinistra, senza cifre tabulari (regola 44). | Bassa | deciso — v. «Decisioni» |
| U18 | `Ui/PannelloEmail.Designer.vb:267, 276` | TabIndex duplicato (2) su `btnDocumenti` e `btnHoSpedito`: ordine di tabulazione imprevedibile. | Bassa | deciso — v. «Decisioni» |
| U19 | vari | Minori di coerenza: «◀» solo su un bottone-indietro dei tre; gap locali fuori griglia (7/9/20/21 px; 8 px fra due bottoni in Informazioni); coordinate fittizie nel Designer di P2 (`btnDialogo` copre `btnImportaDaSito`). | Bassa | deciso — v. «Decisioni» |
| U20 | `Ui/FinestraDiCaricamento.vb:41-51`, `Ui/FormPrincipale.vb:908-910` | Lo scudo non rispetta la preferenza di sistema per il movimento ridotto e compare senza soglia anti-flash (~300 ms; mitigato: le chiamate durano secondi). | Bassa | deciso — v. «Decisioni» |

### Divergenze — Categoria B (linea guida di progetto vs standard industriale)

Qui la linea guida di progetto è **essa stessa in discussione**: ogni voce porta la
raccomandazione del revisore, la decisione spetta al caso per caso con Mirco.

| # | Linea guida di progetto | Criterio industriale | Raccomandazione | Decisione |
|---|------------------------|----------------------|-----------------|-----------|
| B1 | `RossoTitoli #FA0825` su **tutti** i titoli di finestre e GroupBox (family feeling) | Regola 21: il rosso è riservato a errore/pericolo; 22: i GroupBox 9 pt bold fanno 4,10:1 | **Compromesso**: tenere il rosso brand sui titoli grandi (16/14 pt bold = testo grande WCAG, 4,10:1 ≥ 3:1 lecito), toglierlo agli **errori** (→U4) così la collisione semantica sparisce; valutare i soli GroupBox 9 pt (sotto soglia). | deciso — v. «Decisioni» |
| B2 | Bottoni 110×32 / 130×32 / barra 110×34 | Regola 13: altezza min 40-48, larghezza min 120 | **Tenere 32 px**: è lo standard desktop Windows 11 (WinUI ≈32); la regola è calibrata sul touch. Correggere solo i 26 px di P3 (→U7) e il 34 della barra se si vuole la griglia. | deciso — v. «Decisioni» |
| B3 | Corpo Segoe UI 9 pt (12 px), didascalie 8 pt (10,7 px) | Regola 26: corpo ≥14 px, mai sotto 13 px | 9 pt è il default di Windows: **tenere il corpo**. Ma le didascalie a 8 pt portano informazioni operative (il *motivo* dei bottoni spenti, cap. 03.8): **alzarle a 9 pt**. | deciso — v. «Decisioni» |
| B4 | Griglia di spaziatura «14/12/8» | Regola 3: griglia a multipli di 4 px | **Tenere**: sistema locale coerente e documentato; riallineare tutto costerebbe ogni Designer per un beneficio invisibile. | deciso — v. «Decisioni» |
| B5 | Sistema dei **livelli di conseguenza** (03.3): il colore dice il peso, quindi più bottoni pieni per fascia (P1, P2, P3, P4, P7; FinestraDocumenti, Backup) | Regole 12/53: un solo pulsante pieno per fascia/modale | **Tenere il sistema**: è un modello mentale proprio, documentato, con correzioni pagate sul campo (bottoni «creduti morti» a T9d). Curare solo i casi in cui il sistema si contraddice da sé: U11 (due L1 gemelli adiacenti), U12 (due rossi adiacenti), e valutare `btnRileggi` L3→L2 in FinestraDocumenti. | deciso — v. «Decisioni» |
| B6 | Tre presenze del marchio: pannello logo permanente 261×216, mega stemma dietro il menu P0, scudo Aviolab come indicatore d'attesa | Regola 1: max un logo piccolo per schermata, mai negli sfondi | **Tenere**: identità dichiarata del prodotto (cap. 03.5 «irrinunciabile»; il mega stemma è la soglia, lo scudo d'attesa è funzionale e porta la barra). Annotare la deroga qui: è la precedenza brand-di-progetto, confermata consapevolmente. | deciso — v. «Decisioni» |
| B7 | Splash con minimo garantito di **5 s** (chiesto da Mirco il 2026-08-30) a fronte di 265-330 ms di caricamento | Regola 1: lo splash scompare a caricamento finito | **Tenere** (scelta del proprietario, un clic la chiude e chi deve rispondere a una finestra la salta); eventuale riduzione a ~3 s solo se Mirco la vuole. | deciso — v. «Decisioni» |
| B8 | Tutte le stringhe utente nel codice, nessun `.resx` | Regola 65: stringhe in risorse | **Deroga dichiarata**: app deliberatamente mono-lingua italiana. Annotare in `idee_future.md` per un'eventuale localizzazione. | deciso — v. «Decisioni» |
| B9 | Barra superiore: 7 caselle piene (1 verde + 6 azzurre), l'attiva distinta dalla cornice | Regola 12: mai tre o più pieni affiancati | **Tenere**: la barra è l'indice-eco del menu d'ingresso (03.4), scelta recente e motivata; il criterio nasce per gruppi di *azioni*, la barra è navigazione. | deciso — v. «Decisioni» |
| B10 | Impostazioni: finestra unica a 7 sezioni con scroll | Regola 58: evitare schermate contenitore | **Tenere** (finestra che «fa», non wizard); eventuale riorganizzazione a tab in `idee_future.md`. | deciso — v. «Decisioni» |
| B11 | Titoli in `FontStyle.Bold` (≈700) | Regola 26: titoli peso 500-600 | **Tenere**: Segoe UI Semibold è un'altra famiglia; il guadagno non vale la deviazione dal font unico. | deciso — v. «Decisioni» |
| B12 | Campi input alti 23-24 px (conseguenza del font 9 pt) | Regola 16: input 40-48 px | Legata a B2/B3: se corpo e bottoni restano, **tenere**; eventuale padding esplicito (~30-32 px) solo sulle caselle singole più usate (chiave API, parola di conferma). | deciso — v. «Decisioni» |

### Decisioni (2026-09-01, voce per voce col tutor)

**Categoria A — tutti approvati per la correzione**, con una sola riduzione:

- **U1–U19: approvati.**
- **U20: approvato il solo anti-flash (~300 ms)**; il rispetto della preferenza di
  sistema per il movimento ridotto **non si fa**.

**Categoria B — esiti:**

| # | Esito | Conseguenza operativa |
|---|-------|----------------------|
| B1 | ✅ Compromesso confermato | Rosso brand resta sui titoli grandi; via dagli errori (dentro U4); GroupBox 9 pt da valutare al momento del fix. |
| B2 | ✅ Tenere 32 px | Nessun cambio ai token dei bottoni. |
| B3 | ✅ Confermata | **Fix**: didascalie da 8 a 9 pt (`FontDidascalia`); il corpo resta 9 pt. |
| B4 | ✅ Tenere 14/12/8 | Deroga consapevole alla griglia 4 px. |
| B5 | ✅ Tenere il sistema dei livelli | **Fix**: solo `btnRileggi` L3→L2 in FinestraDocumenti (oltre a U11/U12 già approvati). |
| B6 | ❌ **Raccomandazione rovesciata** | **Il marchio vive solo nel pannello in basso a sinistra, com'era già.** **Fix**: via il mega stemma dietro il menu P0; l'indicatore d'attesa diventa neutro (ruota + barra, senza scudo). ⚠ Rovescia scelte recenti di Mirco (mega stemma del 30/08, scudo d'attesa del 30-31/08): **da dichiarare esplicitamente nella PR**. |
| B7 | ✅ Tenere 5 s | Lo splash resta com'è (scelta di Mirco confermata); nessun fix. |
| B8 | ✅ Deroga confermata | Niente `.resx`; nota in `idee_future.md` per un'eventuale localizzazione. |
| B9 | ✅ Tenere la barra | Le 7 caselle piene restano. |
| B10 | ✅ Tenere Impostazioni | Eventuale riorganizzazione a tab in `idee_future.md`. |
| B11 | ✅ Tenere il Bold | Nessun cambio ai font dei titoli. |
| B12 | ✅ Tenere gli input | Nessun cambio alle altezze. |

### Piano dei fix (documentato prima di partire, 2026-09-01)

Sette blocchi in ordine di esecuzione: prima i token (a monte di tutto), poi i
comportamenti, poi i locali. **Ogni blocco chiude con `dotnet test` verde** prima del
successivo; i collaudi nuovi di proprietà si provano a far fallire (regola 14 di
progetto). Le scelte puntuali che le decisioni lasciavano aperte sono qui fissate come
**⚙ proposta**: si correggono a vista, non richiedono un nuovo giro di decisioni.

**F2-1 — Token e stile (`StileApp.vb`, `BottoneMenu.vb`)** — copre U3, U13, U14, B3,
e la coda di B1.
- U3a ⚙ proposta: `Successo` scurisce a **`#1E7E34`** per tutti i suoi usi (un solo
  verde di famiglia; bianco sopra ≥ 4,5:1). Controllo a vista su badge OK e casella
  «🎮 Menu».
- U3b ⚙ proposta: il fondo del livello **Critico** lascia `RossoTitoli` e prende un
  token nuovo **`RossoCritico` ≈ `#B00013`** (bianco sopra ≈ 6:1): più scuro e grave
  del `Pericolo` di L5 — la saturazione continua a crescere col peso — e il rosso
  brand resta solo ai titoli, che è la sostanza di B1.
- U3c ⚙ proposta: token **`InformazioneTesto` ≈ `#0F6674`** per il testo informativo
  (promemoria e righe «da sollecitare» in Home); `Informazione` resta ai badge.
- U13: `MouseOverBackColor`/`MouseDownBackColor` derivati dal fondo di ogni livello
  (scurimento come già fa `BottoneMenu`), dentro `Dipingi`/`DipingiLaCasella`.
- U14 ⚙ proposta: raggio del bottone-menu **8 px**, dichiarato in `StileApp` come
  token dei raggi.
- B3: `FontDidascalia` da 8 a **9 pt**. Controllo a vista dei punti stretti (riga
  versione del logo, hint sotto i bottoni).
- B1-coda ⚙ proposta: i titoli dei **GroupBox** (9 pt, testo piccolo, oggi 4,10:1)
  passano al rosso scuro `RossoCritico` usato come colore di testo (≈ 6:1 sui fondi
  chiari, resta rosso brand all'occhio); i titoli grandi 14/16 pt restano `#FA0825`.
- Verifica: banco + giro a vista con lo strumento di collaudo (screenshot dei
  pannelli toccati).

**F2-2 — Filo del lavoro AI e asincronia** — copre U1, U2, U5, U20.
- U1: `PannelloProfilo` dichiara `LavoroAiCambiato`, lo alza in `LetturaInCorso`
  (import CV), `FormPrincipale` lo aggancia come per P4–P7. **Collaudo nuovo
  falsificabile**: durante l'import la barra di navigazione è spenta (si prova rosso
  togliendo l'evento).
- U2: `SvuotaNavigazione`, `EliminaTutto` (FinestraImpostazioni), `Componi`+`Scrivi` e
  `Ripristina` (FinestraBackup) passano a `Task.Run`+`Await` col pattern già in uso
  lì accanto (bottoni spenti + riga di stato «Sto lavorando…»).
- U5: in P6 e P7 il comando che avvia l'attesa diventa «Annulla» durante il lavoro
  (pattern di P4 «Analizza»→«Annulla»), collegato ad `AnnullaIlLavoro()`.
- U20: lo scudo compare dopo ~300 ms di lavoro (timer di soglia in `FormPrincipale`);
  sotto, nessun flash.
- Verifica: banco + prova dal vivo (import CV, generazione P6, email P7, svuota
  cache).

**F2-3 — Errori uniformi (U4)** — trasversale.
- Ovunque: colore `Pericolo` + prefisso **«Errore: »** (o ⚠) nei messaggi d'errore;
  via il `RossoTitoli` dagli errori di FinestraImpostazioni (9 occorrenze) e
  FinestraChiaveApi; `Racconta` di P3 prende il parametro colore/gravità come i
  `RaccontaLoStato` degli altri pannelli.
- Verifica: banco + ricerca a tappeto (`grep RossoTitoli`) che dopo il giro il token
  compaia solo su titoli e livello… (post F2-1: solo titoli grandi).

**F2-4 — DPI residui (U6)**.
- `PannelloEmail.Designer.vb`: `AutoScaleDimensions`/`AutoScaleMode = Font` come gli
  altri sette.
- `FinestraAppunti`, `FinestraDocumenti`, `FinestraModificaTesti`: correzione
  `ScalaSchermo` della decisione 15.7 (larghezze in pixel veri, tetto sull'area di
  lavoro).
- Verifica: banco; prova a 150% rimandata alla macchina con quel DPI → riserva in
  `in_sospeso.md` se non disponibile (regola 15).

**F2-5 — Livelli e conferme** — copre U7, U8, U9, U10, U11, U12, B5.
- U7: i 7 bottoni della barra di ricerca P3 a **32 px** (token `BottoneStandard`).
- U8: `FinestraConferma` (verbo «Confermo») per eliminazione voce profilo, «Dimentica»
  ricerca, «Scarta» — via le tre `MessageBox` Sì/No.
- U9: «Fallo riscrivere» (P7) → livello 4 + conferma quando oggetto/corpo portano
  correzioni manuali (pattern dell'avviso di «Rigenera» in P6).
- U10: `btnRipristina` → `Attenzione` (L4).
- U11 ⚙ proposta: **«Cattura annuncio» → L3** (è l'azione principale del pannello:
  azzurro + bordo accento, più coerente del L1 attuale); «Importa CV da questa
  pagina» resta L1. I due verdi gemelli spariscono senza inventare colori.
- U12: in Impostazioni il bottone Critico prende una riga/fascia propria, separata
  dal Distruttivo (il vuoto intorno, come nella fascia dei comandi).
- B5: `btnRileggi` (FinestraDocumenti) L3→L2.
- Verifica: banco + a vista.

**F2-6 — Marchio (B6)**.
- `PannelloMenu`: via `DisegnaIlMegaStemma` (resta l'avorio, il nome e il
  sottotitolo in cima); il collaudo del margine dei bottoni si aggiorna se guardava lo
  stemma.
- `FinestraDiCaricamento`: l'indicatore d'attesa perde lo scudo — restano **ruota di
  pallini + barra che si riempie**, centrati dove stava il complesso (stessa finestra
  layered, stessi conti di posizione).
- ⚠ Da dichiarare nella PR: rovescia il mega stemma (30/08) e lo scudo d'attesa
  (30-31/08), scelte di Mirco.
- Verifica: banco + screenshot del menu e di un'attesa AI.

**F2-7 — Micro-fix locali** — copre U15, U16, U17, U18, U19.
- U15: «Nessun documento riconosciuto in questa cartella.» (FinestraDocumenti),
  «Questo documento non ha testi da riscrivere.» (FinestraModificaTesti), coda Home
  vuota: «Comincia da "Nuova ricerca", o incolla un annuncio in Confronta.»
- U16: etichetta visibile sopra la casella risposta di P5 e «Indirizzo:» accanto alla
  casella di P3.
- U17: colonne «Match» e «Aggiornata» allineate a destra.
- U18: TabIndex di P7 rinumerati.
- U19: «◀» anche su «Torna all'opportunità» e «Torna al profilo»; gap locali
  riallineati **ai token di progetto 8/12/14** (7→8, 9→8, 20→14, 21→14, e 12 px fra i
  due bottoni di FinestraInformazioni); coordinate del Designer di P2 allineate alla
  disposizione reale (btnDialogo/btnImportaDaSito).
- Verifica: banco + a vista.

**Chiusura di fase**: aggiornamento del cap. 03 (design-first) per B6, U11, B3,
B1-coda e i token nuovi; deroghe B2/B4/B7–B12 annotate; B8 e B10 in
`idee_future.md`; banco completo finale.

### Fase 2 — stato

Decisioni complete e piano dei fix documentato qui sopra. **Implementazione non
ancora partita**: attende il via sul piano (le ⚙ proposte si correggono a vista).

---

## Fase 3 — Verifica della funzionalità rispetto al dichiarato

### Perimetro e metodo (DECISI, 2026-09-01)

- **Il «dichiarato»**: `README.md` (stato e funzioni), `GUIDA.md` (ciò che l'utente
  vede e fa), le promesse dei capitoli di `VB.NET/progetto/` (in particolare i flussi
  del cap. 12 e le promesse comportamentali dei capitoli funzionali 05–11),
  `in_sospeso.md`.
- **Metodo**: confronto promessa per promessa col comportamento reale — codice alla
  mano; le prove dal vivo che richiedono build si coordinano col thread dei fix UI, che
  lavora in parallelo sul working tree. Tre agenti: GUIDA vs codice, flussi cap. 12 vs
  implementazione, README/capitoli funzionali vs codice.
- **Le riserve già dichiarate in `in_sospeso.md` non sono rilievi**: la verifica le
  spunta come «dichiarato coerente» e cerca le divergenze **non** dichiarate.
- I fix UI approvati in Fase 2 non si ricontano qui.

### Esito d'insieme (2026-09-01)

**Il prodotto è notevolmente fedele al dichiarato.** Sul fronte funzionale (README +
capitoli 05/07/09/11 campionati) **nessuna divergenza vera**: 13 tool MCP con i nomi
del cap. 09, le due ere del protocollo, il lucchetto asimmetrico, backup con
esclusioni promesse, modelli dalle Impostazioni, informativa prima della chiave,
DPAPI e mascheratura — tutto riscontrato al file:riga. I flussi del cap. 12 sono
conformi passo per passo (A, B, C, E, F, regole trasversali 12.7, macchina a stati
07.3), con un'eccezione sola: il **flusso D**. La GUIDA è molto fedele (una quarantina
di affermazioni riscontrate, spesso parola per parola); un'unica promessa sostanziale
non mantenuta (WebView2) e cinque nomi rimasti indietro.

### Rilievi

| # | Dichiarato (dove) | Realtà (dove) | Gravità | Correzione proposta | Decisione |
|---|-------------------|---------------|---------|---------------------|-----------|
| V1 | GUIDA (righe 35-37): WebView2 mancante → «te lo dice **all'avvio**, ti dà **il link ufficiale di Microsoft**» | Nessun controllo all'avvio (accensione pigra deliberata, `Web/MotoreBrowser.vb:44-47`): l'assenza si scopre alla prima stampa PDF o aprendo Ricerca; nessun link a Microsoft in tutto il sorgente. La parte «non muore, degrada con garbo» è vera. | Media | **GUIDA** («te lo dice la prima volta che serve»); opzionale una riga di codice per aggiungere il link ai due messaggi | ✅ GUIDA **e** link nei due messaggi (2026-09-01) |
| V2 | GUIDA (70): «Confronta ⭐ ANNUNCIO - CV» | Dal 31/08 è «Confronta ★ ANNUNCIO - CV» (`Ui/NomiUi.vb:42`) | Bassa | GUIDA | ✅ approvata |
| V3 | GUIDA (71): «📄 Documenti» | Il bottone è «▤ Documenti» (`FormPrincipale.Designer.vb:156`) | Bassa | GUIDA | ✅ approvata |
| V4 | GUIDA (169): «Elimina tutto» | Il bottone è «ELIMINA TUTTI I DATI» (`FinestraImpostazioni.Designer.vb:325`); comportamento promesso tutto presente | Bassa | GUIDA | ✅ approvata |
| V5 | GUIDA (156): «Impostazioni → Backup e ripristino» | La voce è «Backup…» nella sezione «I tuoi dati»; «Backup e ripristino» è il titolo della finestra che si apre | Bassa | GUIDA | ✅ approvata |
| V6 | GUIDA (69): in Ricerca «incolla il testo di uno che hai già» | In Ricerca si incolla il **link**; il **testo** si incolla nel pannello Confronta (`PannelloOpportunita.vb:202`) | Bassa | GUIDA | ✅ approvata |
| V7 | README (5): «1257 collaudi verdi» | Fermo al 30/08: conteggio statico attuale 1368 metodi (33 «Reale», ~1335 senza rete — combacia col banco verde 1335 della Fase 1) | Bassa | README, al prossimo «aggiorna-tutto» | ✅ approvata |
| V8 | Cap. 12.4: **flusso D** (sessione di aggiornamento) con proposta datata, dialogo differenziale, salvataggio versionato | **Mai costruito**: `btnAggiornamento` spento incondizionato e senza gestore (`PannelloProfilo.vb:1461`), nessuna modalità differenziale nel dialogo; l'infrastruttura di versionamento esiste ma nulla la usa. La decisione «o si fa o si dichiara fuori dalla 1.0» è **già a registro** (in_sospeso, 2026-08-31): questa revisione è il posto per prenderla. | Media (decisione, non difetto) | O si costruisce (fuori scope di questa revisione) o si dichiara nel cap. 12 fuori dalla 1.0, allineando P2 | ✅ **fuori dalla 1.0** (2026-09-01): si dichiara nel cap. 12; il destino del bottone di P2 (tooltip onesto o rimozione) lo decide il fix. ⚠ Da evidenziare nella PR: chiude una promessa di progetto. |
| V9 | Cap. 12 A2.2 (45-49): la porta «qui c'è tutto» dell'import «resta una promessa a metà, annotata in in_sospeso.md» | Costruita il 19/08 (`PannelloProfilo.CvDaImportare:1124-1149`) e la voce di in_sospeso è in «Chiuse»: il capitolo dichiara aperto un debito chiuso | Bassa | Capitolo 12 | ✅ approvata |
| V10 | Diagramma cap. 07.3: transizioni della macchina a stati | Manca la freccia `generata → scartata`, che il codice consente deliberatamente e documenta (`StatoOpportunita.vb:67-71`) | Bassa | Capitolo 07.3 (una freccia) | ✅ approvata |

**Spuntate come «dichiarato coerente»** (riserve attive di `in_sospeso.md` ritrovate
tali, non rilievi): giro D aperto e tag `v1.0` rimandato; firma del codice; icona col
marchio vecchio; «GUASTO in l'ultima rete»; versione moderna MCP unica; collaudo cieco
della `FinestraConfermaCritica`; conferme di livello 5 in due forme (→ coperta dal fix
U8 della Fase 2); prima/dopo dell'email in P7; demo video; costanti `StileApp` sommate
a misure scalate; profilo LinkedIn testimoniato e non sorvegliato.

### Fase 3 — stato

Verifica completata e **decisioni prese voce per voce col tutor (2026-09-01)**:
V2–V7, V9, V10 approvate in blocco (correzioni documentali); V1 = GUIDA corretta
**più** il link Microsoft aggiunto ai due messaggi del codice; V8 = **flusso D
dichiarato fuori dalla 1.0** nel cap. 12 (da evidenziare nella PR). Fix documentali
eseguibili in questa stessa sessione; V1-codice e il destino del bottone di P2 si
accodano al thread dei fix (o a un blocco finale qui), per non incrociare le mani sul
working tree.