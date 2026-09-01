# Fix UI Fase 2 — log di avanzamento

*Log dell'orchestratore dei fix UI (istruzioni in `istruzioni-fix-ui.md`, piano in
`revisione-finalizzazione.md` §«Piano dei fix»). Una riga per blocco: voci chiuse,
commit, esito banco, verifiche a vista fatte/rimandate, scostamenti dal piano.*

| Blocco | Voci | Stato | Commit | Banco | Verifica a vista | Note |
|--------|------|-------|--------|-------|------------------|------|
| F2-1 | U3, U13, U14, B3, B1-coda | ✅ chiuso | `752e652` | verde (1343/0) | fatta: menu, Home, P2, Impostazioni (screenshot); hover coperti dai 2 collaudi nuovi | perimetro esteso ai Designer (B1-coda vive in 12 righe su 7 file, non in un helper); vedi note |
| F2-2 | U1, U2, U5, U20 | ✅ chiuso | `e41a9d7` | verde (1344/0) | svuota navigazione dal vivo (ramo d'errore onesto, finestra viva); flussi AI dal vivo **in riserva** (niente chiave API su questa macchina) | collaudo U1 falsificato in 2 modi; U20 verificato a codice + banco |
| F2-3 | U4 | ✅ chiuso | `f68c5ba` | verde (1345/0) | grep `RossoTitoli`: resta solo su titoli grandi (verificato) | doppio prefisso «Errore — »/«Attenzione — » (scostamento dichiarato, a parità d'intento); P3 con metodi dedicati invece del parametro |
| F2-4 | U6 | ✅ chiuso | `07390ec` | verde (1347/0, albero F2-4+F2-6) | prova a 150% **in riserva** (macchina a quella scala; → in_sospeso) | 5 collaudi nuovi falsificati (tetto, scorrimento, margine); variante lunga del pattern 15.7 per la trappola della barra |
| F2-5 | U7–U12, B5 | ✅ chiuso | `3705267` | verde (1361/0) | P3 a 32 px + Cattura L3 e Impostazioni col Critico su riga propria verificati a schermo; U8/U9/U10/B5 su diff+collaudi | 14 collaudi nuovi falsificati; U9 con marchi del lavoro a mano e bozza ripresa gestita con onestà; BOM spurio su 2 file rimosso in review |
| F2-6 | B6 | ✅ chiuso | `3fd21c7` | verde (1347/0) | menu senza stemma verificato a schermo; attesa AI **in riserva** (chiave assente; → in_sospeso) | 4 falsificazioni viste rosse (lo stemma rimesso accende solo il collaudo dell'assenza); limiti dichiarati: niente collaudo dell'assenza in P0 (solo il compilatore), classe `ScudoDiCaricamento` da rinominare (fuori perimetro) |
| — | falsificazioni + riserve | ✅ | `afff0d2` | — | — | commit di servizio |
| F2-7 | U15–U19 | ✅ chiuso | `c5f4dfc` | verde (1368/0) | Home vuota col messaggio guida, «Indirizzo» in P3, «Aggiornata» a destra verificati a schermo | 7 collaudi nuovi falsificati; «Match» non allineabile (limite Win32 su prima colonna, dichiarato+sorvegliato); «Indirizzo» senza due punti (coerenza con le etichette vicine); mappa gap applicata letterale, con la nota che 5 finestre su 6 si dispongono a runtime |
| — | falsificazioni F2-5/F2-7 + README strumento | ✅ | `1da19ee` | — | — | commit di servizio |
| Chiusura | cap. 03, idee_future | ✅ chiuso | `103447d` | **verde finale: 1368/0** | — | cap. 03 riallineato (token, livelli, marchio in un posto solo, Segnalazioni, deroghe B2/B4/B7/B9/B11/B12 annotate); B8+B10 in idee_future.md |

## Giro di rifiniture dettate dal tutor in sessione (2026-09-01, pomeriggio)

Dopo la chiusura della fase, il tutor — guardando l'app dal vivo — ha dettato otto
rifiniture, implementate in un giro unico (commit `4b9daeb`, 35 file, banco verde
**1390/0**, 22 collaudi nuovi falsificati):

1. logo Aviolab senza box né fondo proprio (segue il colore del pannello sotto);
2. scritta del menu che scala con la finestra, e colonna mai sovrapposta a
   nome/sottotitolo a nessuna misura (guardia su `zona.Top`, non sul bordo);
3. bottoni su **scala di misure fisse** (110/130/190/240/300 + icona 40, gradini presi
   dalle 24 larghezze ad-hoc preesistenti; l'unica scritta accorciata è «Come
   funziona…», anche in Informazioni — stessa porta, stesso nome);
4. **FlatStyle.Standard** ovunque (via da StileApp i bordi FlatAppearance ormai inerti,
   `BordoMarchio` e `BordoSuccesso` rimossi; collaudi hover rimossi/riscritti);
5. splash: minimo **10 s**, Invio (filtro messaggi: il fuoco non è mai suo) e clic lo
   chiudono subito;
6. bottone «?» in coda alla barra (token `BottoneBarraSuperioreIcona`, neutro, acceso
   anche durante il lavoro AI — scelta dichiarata) che riapre l'informativa; dentro,
   «Credits» (© Aviolab AI, Mirco Parenti, tecnologie; collaudo che vieta menzioni di
   strumenti di sviluppo, regola 12);
7. apertura **1920×1024 centrata** invece che massimizzata (funzione pura
   `ScalaSchermo.MisuraDiApertura`, tetto in unità di progetto convertito col DPI:
   a 150% la finestra apre più grande per contenere lo stesso disegno — interpretazione
   dichiarata);
8. Impostazioni **a due colonne**: 2/3 testi, 1/3 comandi a destra all'altezza della
   propria sezione (frazione, non pixel: scala col DPI); valori (lingua, giorni,
   modelli) restano nel flusso del testo. ⚠ Supera la forma di U12 committata al
   mattino («margine opposto»): cap. 03 riallineato dall'implementatore.

Verifica a vista fatta (apertura a misura, menu a 1150×600 senza sovrapposizioni,
Impostazioni a colonne, logo fuso, «?»+Credits); banco rifatto da me. Fuori perimetro
annotato: le etichette AI dei tre bottoni-scelta di P5 possono troncare a 130 px
(preesistente). Trappola nuova dello strumento di collaudo: davanti alla **sola
informativa modale del primo avvio** (nessuna finestra principale ancora)
`controlli`/`clic` rispondono «non ha una finestra aperta» mentre UIA la vede — aggirata
con lo script PowerShell di ripiego; da scrivere nel README dello strumento.

## Rilettura del piano voce per voce (spirito della regola 16, 2026-09-01)

Riletto il «Piano dei fix» di `revisione-finalizzazione.md` impegno per impegno:

- **F2-1**: U3a ✅ · U3b ✅ · U3c ✅ · U13 ✅ · U14 ✅ · B3 ✅ · B1-coda ✅ · verifica banco+vista ✅
- **F2-2**: U1 ✅ (collaudo falsificato in 2 modi) · U2 ✅ · U5 ✅ · U20 ✅ (solo anti-flash,
  come deciso) · prova dal vivo: svuota cache ✅, **flussi AI in riserva** (in_sospeso.md)
- **F2-3**: U4 ✅ · grep finale ✅ (`RossoTitoli` solo su titoli grandi)
- **F2-4**: U6 ✅ · **prova a 150% in riserva** (in_sospeso.md)
- **F2-5**: U7 ✅ · U8 ✅ · U9 ✅ · U10 ✅ · U11 ✅ · U12 ✅ · B5 ✅ · verifica a vista ✅
- **F2-6**: B6 ✅ (menu verificato a schermo; **screenshot dell'attesa in riserva**;
  ⚠ da dichiarare nella PR: rovescia mega stemma 30/08 e scudo 30-31/08 — nel cap. 03
  la nota c'è, la PR la scrive la sessione di revisione)
- **F2-7**: U15 ✅ · U16 ✅ · U17 ✅ (con il limite Win32 dichiarato e sorvegliato) ·
  U18 ✅ · U19 ✅
- **Chiusura**: cap. 03 per B6/U11/B3/B1-coda/token ✅ · deroghe B2/B4/B7–B12 annotate ✅ ·
  B8+B10 in idee_future.md ✅ · banco completo finale ✅ (1368/0) · trappola
  SynchronizationContext nel README dello strumento ✅ (impegno emerso in corsa, regola
  di progetto sulle trappole)
- **Decisioni della revisione rispettate senza fix**: B1 compromesso ✅ · B2 ✅ · B4 ✅ ·
  B5-sistema ✅ · B7 ✅ · B9 ✅ · B11 ✅ · B12 ✅ · U20-movimento-ridotto deliberatamente
  non fatto ✅

**Nessun impegno del piano è rimasto in silenzio**: ciò che non è chiuso è scritto — le
due riserve in `in_sospeso.md` (150% e flussi AI dal vivo, coi due numeri dell'indicatore
da giudicare a video) e i fuori perimetro annotati in questo log, che la sessione di
revisione consoliderà nel report. Domanda aperta per il tutor, emersa da U11: con
«Cattura annuncio» a L3, P3 ha **due L3 in due fasce** («Cerca» lo era già); il cap. 03
ora dichiara il criterio «per fascia» — se «Cerca» vada declassato resta da decidere.

**Bilancio**: 8 commit (`752e652`, `e41a9d7`, `f68c5ba`, `07390ec`, `3fd21c7`, `afff0d2`,
`3705267`, `c5f4dfc` + servizio `1da19ee` e chiusura `103447d` — dieci in tutto), banco
da 1335 a **1368 collaudi, sempre verde a fine blocco**, 33 collaudi nuovi tutti visti
rossi falsificando, 4 file di sicurezza mai toccati né committati.

## Note di percorso

- 2026-09-01 — Avvio orchestrazione. Baseline del banco **verde prima di ogni
  modifica: 1335 passati, 0 falliti**. I 4 file con fix di sicurezza pendenti (`HTML+JS/server.js`,
  `ArchivioOpportunita.vb`, `ScrittoreEml.vb`, `ServerMcp.vb`) restano intoccati e
  fuori da ogni commit (add mirato per blocco).
- 2026-09-01 — **F2-1 chiuso** (commit `752e652`). Scostamenti dal piano, tutti a
  parità di intento: B1-coda non passa da un helper — il colore dei titoli 9 pt è
  assegnato dai Designer — quindi il perimetro si è esteso a 12 righe `ForeColor` in
  7 file (`PannelloDocumenti/Opportunita/Ricerca/Email/Home/Profilo.Designer.vb`,
  `PannelloDialogo.vb:1083`); idem la riga U3c del promemoria, che vive in
  `PannelloHome.Designer.vb:154`. `RossoCritico` è `#B00013` (bianco sopra 7,35:1,
  meglio del ~6 stimato). I 4 collaudi nuovi sono stati **visti rossi** falsificando
  (righe in `falsificazioni.md`), compresa la lezione del metro sbagliato:
  `GetBrightness` (HSL) teneva verde la prova della scala anche col difetto rimesso.
  Verifica a vista: giro con lo strumento di collaudo su menu, Home, P2 e
  Impostazioni (chiave finta, dati usa-e-getta); gli hover non si fotografano, li
  coprono i due collaudi dedicati.
- 2026-09-01 — Annotazioni fuori perimetro dall'implementatore F2-1 (non corrette,
  come da istruzioni): (a) il rosso del marchio come testo d'errore a 9 pt nei dieci
  `Racconta(..., RossoTitoli)` di `FinestraImpostazioni.vb` (righe 184, 269, 308,
  507, 516, 551, 634, 685, 727) e in `FinestraChiaveApi.vb:146` — è materia di
  **F2-3 (U4)**, righe da coprire lì; (b) `VB.NET/progetto/03_interfaccia_grafica.md`
  indietro su cinque punti (token nuovi, `Successo`, didascalia 9 pt, colore titoli
  GroupBox, fondo livello 6) — è la **Chiusura di fase**, già a piano. Nota di banco:
  un rosso transitorio su `LEsitoSopravviveAlGiroSuDisco` durante un'esecuzione
  intermedia era la sessione sicurezza che scriveva `ArchivioOpportunita.vb` in quel
  momento; due esecuzioni consecutive verdi dopo, e verde anche il mio giro (1343/0).
- 2026-09-01 — Annotazioni fuori perimetro dall'implementatore F2-2 (non corrette):
  (a) la **trappola del contesto di sincronizzazione WinForms nel banco** — un `Await`
  che attraversa un thread dentro una finestra collaudata non torna mai, perché la
  continuazione va a una pompa di messaggi che nel banco non esiste; cura: spegnere
  `WindowsFormsSynchronizationContext.AutoInstall` E azzerare il contesto per il tempo
  della prova (v. `ConMotoreAsync` in `CollaudiFinestraBackup.vb`) — **da scrivere nel
  `strumenti/README.md`** per regola di progetto (trappole pagate sul campo); lo farà
  l'orchestratore in una finestra propria, probabilmente alla chiusura di fase;
  (b) `FormPrincipale` non smaltisce i `Timer` creati a mano (`_battitoDellAttesa` e
  ora `_sogliaDelloScudo`): vivono quanto il processo — incoerenza minore rispetto al
  `ToolTip` dei pannelli, non un guasto.
- 2026-09-01 — **F2-3, dettagli dal report**: oltre alle voci del piano, corretto in
  perimetro `PannelloProfilo.vb:1363`, unico uso di `StileApp.Avviso` (giallo da
  fondo, ≈1,9:1) come inchiostro → ora avviso standard; dopo il giro `Avviso` resta
  solo ai badge. Falsificazioni fatte e viste rosse (colore non riscritto →
  `DopoUnErroreLaRigaTornaGrigia` rosso; prefisso tolto →
  `UnaPaginaCheNonSiLasciaLeggere…` rosso): **righe da aggiungere in
  `falsificazioni.md` alla prossima finestra di commit** (commit piccolo dedicato).
  Fuori perimetro annotati e non corretti: (1) fallimenti parziali raccontati in
  grigio da funzioni che ritornano `String` senza gravità (`PannelloDocumenti.vb`
  665, 817, 1283, 1411; `PannelloOpportunita.vb:437`); (2) `lblPerchePento` di P4
  rosso senza parola (scelta motivata: spiega solo bottoni spenti); (3) esiti
  negativi in grigio in `FinestraInformazioni.vb:198/206` e `lblEsitoProva` di
  FinestraChiaveApi (testo nato in `Ai.ProvaChiave.Spiega`, fuori perimetro);
  (4) `FormPrincipale.vb:1227` errore in MessageBox di sistema (altro canale);
  (5) P5: guasti AI nelle bolle, indistinguibili da risposte (U4 non si applica);
  (6) proposta di token `AvvisoTesto` (ambra scurita, gemello di
  `InformazioneTesto`) — non fatta, da valutare col tutor.
- 2026-09-01 — Annotazioni fuori perimetro dall'implementatore F2-4 (non corrette):
  (a) **`PannelloMenu.Designer.vb` è il secondo pannello senza `AutoScaleMode`** — il
  rilievo U6 diceva «P7 è l'unico» ed era impreciso; non corretto perché fuori dal
  piano approvato (P0 è anche il pannello del mega stemma, toccato da F2-6 per altro);
  (b) `FinestraAvvio` e `FinestraInformazioni` non applicano la 15.7 e U6 non le
  elenca; (c) i margini di `StileApp` restano in unità di progetto anche nel codice
  nuovo — è il debito delle «~83 somme» già annotato in `in_sospeso.md`. Limite onesto
  dichiarato dei collaudi F2-4: a 96 DPI non vedono la conversione DPI in sé (quella
  ha i suoi collaudi puri in `CollaudiScalaSchermo`), sorvegliano tetto, scorrimento
  e margine.
- 2026-09-01 — **F2-5, coda del report**: 14 collaudi nuovi tutti falsificati (righe
  per `falsificazioni.md` nel prossimo commit di servizio). Due falsificazioni hanno
  chiesto un secondo giro, dichiarate: sul testo del profilo la prima rottura toccava
  una frase che il collaudo non guarda; su «appena scritto dall'AI non chiede» le due
  difese (guardia `_riempimenti` + reset post-scrittura) si coprono a vicenda e il
  rosso arriva solo togliendole entrambe. Fuori perimetro annotati: (a) con U11 il P3
  ha **due L3 in due fasce** («Cerca» era già L3) — se «Cerca» vada declassato lo
  decide la chiusura di fase; (b) `email.json` non registra le riscritture a mano
  (per questo la conferma su bozza ripresa è prudenziale) — toccherebbe il formato;
  (c) la MessageBox di conferma P7 non è collaudabile, si collauda la riga che elenca
  (come in P6); (d) FinestraImpostazioni non porta le misure bottoni in pixel veri —
  specie delle «~83 somme» già in `in_sospeso.md`.
- 2026-09-01 — **F2-7, coda del report**: collaudo 7 dichiarato con la lezione giusta —
  la prima stesura restava **verde** con la rottura dentro (guardava apertura/fine del
  dialogo, dove sparisce la fascia intera) ed è stata riscritta sul turno a scelta,
  l'unico momento in cui la riga decide. Fuori perimetro annotati e non corretti:
  (a) `FinestraInformazioni.Designer.vb`: `btnChiudi` e `lblEsitoVersione` con lo
  stesso `TabIndex = 5` (specie di U18, altro file); (b) `FinestraChiaveApi.Designer.vb`:
  coordinate del Designer che non corrispondono al runtime (specie di U19-P2, finestra
  non nominata dal piano); (c) `grpDatiPersonali` di P2: stacchi 22/11 px fuori griglia
  e fuori dalla mappa del piano.
- 2026-09-01 — **Incidente di concorrenza, rimediato**: in questo repo lavora in
  parallelo la sessione di revisione sicurezza, e l'**index git è condiviso** — il
  primo commit F2-1 (`e586050`) ha inglobato 6 file suoi stagiati nel frattempo.
  Rimedio: `reset --soft` + ricommit pulito (`752e652`); il lavoro di sicurezza è
  integro nel working tree, non staged. Concordato con l'altra sessione l'avviso
  reciproco prima di ogni stage/commit.
