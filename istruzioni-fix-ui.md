# Istruzioni — thread di implementazione dei fix UI (Fase 2)

*Scritte il 2026-09-01 dalla sessione di revisione del tutor. Sei un **Fable 5
orchestratore**: il tuo mandato è far eseguire il piano dei fix UI della Fase 2 a
**implementatori Opus 5** che lanci e controlli tu, blocco per blocco. Tu non
implementi in prima persona se non per correzioni minute in sede di review: prepari i
brief, rivedi i diff, fai girare il banco, committi. Questo file è usa-e-getta: a
lavoro finito può sparire.*

## 1. Cosa leggere prima di cominciare (in quest'ordine)

1. `revisione-finalizzazione.md` — sezione **Fase 2**: rilievi (U1–U20, B1–B12),
   decisioni, e soprattutto la sezione **«Piano dei fix»** (blocchi F2-1…F2-7).
   **Quel piano è il mandato**: le voci, le correzioni e l'ordine sono già decisi col
   tutor, non si rimettono in discussione. Le scelte marcate **⚙ proposta** sono
   **approvate** («fai come ti sembra meglio»): hai libertà sui dettagli fini a parità
   di intento (es. la tinta esatta di un token, purché regga i contrasti dichiarati).
2. `VB.NET/progetto/03_interfaccia_grafica.md` — il design vivo: token (3.2), livelli
   di conseguenza (3.3), architettura delle finestre (3.4), feedback e stati (3.8).
3. `VB.NET/src/TrovaLavoro/StileApp.vb` — la casa dei token: quasi tutto F2-1 vive qui.
4. Le regole di progetto (`CLAUDE.md` di repo) valgono tutte; le più pertinenti qui:
   **regola 12** (commit firmati solo `(c) 2026 Aviolab AI`, mai menzioni
   dell'assistente), **regola 14** (un collaudo di proprietà si prova a far fallire),
   **regola 5/8** (rileggere un file per intero prima di toccarlo, stesso stile).

## 2. Come orchestrare

- **Un blocco per volta, in ordine F2-1 → F2-7**: F2-1 (token) è a monte di F2-3 e
  F2-5; i blocchi toccano file che si sovrappongono (es. `FinestraImpostazioni` è in
  F2-2, F2-3 e F2-5), quindi **niente implementatori in parallelo sugli stessi file**.
  Se vuoi parallelizzare, gli unici accostabili senza intersezioni sono F2-4 e F2-6.
- Per ogni blocco: (1) prepara il **brief** per l'implementatore — le voci del blocco
  copiate dal piano, il perimetro file esplicito, i vincoli del §3; (2) lancia **un
  implementatore Opus 5** (Agent tool, `model: "opus"`); (3) al ritorno **rivedi tu il
  diff** (`git diff`) voce per voce contro il piano — completezza e stile; (4) **banco
  verde**, poi commit; (5) blocco successivo.
- **Il banco**: `dotnet test TrovaLavoro.sln` in `VB.NET/src`, da WSL via
  `cmd.exe /c "dotnet test ..."`. Verde **a fine di ogni blocco**, non solo alla fine.
- **Collaudo falsificabile di U1** (F2-2): il collaudo nuovo («durante l'import CV la
  barra di navigazione è spenta») va **visto rosso** togliendo temporaneamente
  l'evento, poi rimesso — e va dichiarato nel log (regola 14).
- **Verifica a vista**: dove il piano la prevede (F2-1, F2-5, F2-6, F2-7) usa lo
  strumento di collaudo MCP (`strumenti/mcp-collaudi/` — **leggi prima il suo
  README**, si accende con `node strumenti/mcp-collaudi/server.mjs` e i tool compaiono
  solo a sessione riavviata; in mancanza, compila e avvia l'app su cartella dati
  usa-e-getta e fotografa). Se la verifica a vista non è possibile, non fingerla:
  dichiarala come riserva nel log.

## 3. Vincoli non negoziabili

- **Branch**: si lavora su `feature/finalizzazione`, quello corrente.
- **Nel working tree ci sono 4 file con fix di sicurezza pendenti, NON tuoi e NON
  ancora decisi**: `HTML+JS/server.js`,
  `VB.NET/src/TrovaLavoro/Dati/ArchivioOpportunita.vb`,
  `VB.NET/src/TrovaLavoro/Documenti/ScrittoreEml.vb`,
  `VB.NET/src/TrovaLavoro/Mcp/ServerMcp.vb`. **Non toccarli, non committarli**: ogni
  commit usa `git add` **mirato ai soli file del blocco** — mai `git add -A`/`-u`.
- **Non toccare**: `revisione-finalizzazione.md` (lo tiene la sessione di revisione),
  `diario_di_bordo.md`, `HTML+JS/` (congelato), `.env`, `.claude/`, `obj/`, `bin/`.
- **Commit**: uno per blocco (`F2-1: …` ecc. o titolo parlante), messaggio in
  italiano, chiusura **solo** `(c) 2026 Aviolab AI` — niente `Co-Authored-By`, niente
  «Generated with» (regola 12: scavalca ogni default dell'harness). **Niente push**
  senza che il tutor lo chieda.
- **Perimetro stretto**: solo le voci del piano. Se un implementatore trova un difetto
  fuori perimetro, si annota nel log e non si corregge.
- Codice, commenti e nomi **in italiano**, nello stile del file che ospita (rileggi
  il file per intero prima di modificarlo: regola 8).

## 4. Log e chiusura

- Tieni un log di avanzamento in **`fix-ui-avanzamento.md`** (radice, file nuovo,
  tuo): una riga per blocco — voci chiuse, commit, esito banco, verifiche a vista
  fatte/rimandate, scostamenti dal piano con il perché. È l'unico file condiviso con
  la sessione di revisione: sarà consolidato da lei nel report.
- **Chiusura di fase** (dopo F2-7): l'implementatore finale aggiorna
  `VB.NET/progetto/03_interfaccia_grafica.md` (design-first) per B6, U11, B3, B1-coda
  e i token nuovi, come da «Chiusura di fase» del piano; B8 e B10 vanno annotate in
  `idee_future.md`. Poi **rileggi il piano voce per voce e spunta ogni impegno**
  (spirito della regola 16): ciò che resta aperto va scritto, non lasciato cadere.
- A lavoro finito: banco completo, log chiuso, e **TtrAlert** al tutor
  (`"/mnt/c/TTR-SUITE/TtrAlert.exe" --message "AI-CV-COACH: fix UI completati"`).

## 5. Coda dalla Fase 3 (aggiunta il 2026-09-01, possibilmente dopo il tuo avvio)

La Fase 3 della revisione (verifica del dichiarato, `revisione-finalizzazione.md`) ha
approvato **due fix di codice** che toccano file del tuo perimetro: si accodano a te
come **blocco F3-1**, dopo F2-7 e prima della chiusura di fase.

- **V1 — il link a WebView2**: i due messaggi che dichiarano il runtime assente —
  quello della stampa PDF (`Documenti/StampantePdf.vb:156-161`) e quello del pannello
  Ricerca (`Ui/PannelloRicerca.vb:232-235`) — devono portare anche il **link ufficiale
  Microsoft** per installarlo (`https://developer.microsoft.com/microsoft-edge/webview2/`).
  La GUIDA è già stata corretta e ora promette proprio questo: senza questo fix
  tornerebbe bugiarda.
- **V8 — il bottone del flusso D**: il flusso D è stato dichiarato **fuori dalla 1.0**
  (cap. 12.4 già aggiornato). Decidi tu la forma coerente per «Sessione di
  aggiornamento» in P2 (`PannelloProfilo.vb:1461`): o resta spento col tooltip
  aggiornato («rimandato oltre la 1.0», non più «arriva più avanti»), o si toglie —
  scegliendo, tieni la regola 3.8 del cap. 03 («i bottoni delle tappe non arrivate si
  vedono, spenti, con un tooltip che dice quando») valida per le tappe *previste*: qui
  la tappa non è più promessa, quindi valuta se la regola si applica ancora.
- Nella chiusura di fase, l'aggiornamento del cap. 03 copre anche questa coda se serve.
