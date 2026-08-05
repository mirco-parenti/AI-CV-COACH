# 04 — La libreria dei prompt (il pool)

*I prompt sono l'asset più prezioso del progetto: codificano la logica e le bussole
etiche. In questa fase escono dal codice e diventano una libreria di file `.md`
esterni, versionata come un tutto — il «pool» la cui versione compare accanto al logo.*

## 4.1 Principi

1. **Un prompt = un file `.md`**, leggibile da chiunque. Il pool è **in chiaro e
   pubblico**: fa parte del portfolio, la trasparenza è un valore del progetto.
2. **Unica fonte di verità**: il prompt vive **solo** nel pool. Sparisce la vecchia
   regola del prototipo che imponeva la sincronizzazione carattere-per-carattere tra
   documentazione e codice: non c'è più un doppione da tenere allineato. Il codice
   VB.NET non contiene testo di prompt, solo riferimenti per `id`.
3. **Caricamento al bisogno**: il programma legge il file quando il flusso lo richiede
   (con una piccola cache in memoria); i segnaposto vengono riempiti al volo.
4. **Versione di pool unica**: il pool si aggiorna **tutto insieme** e ha un numero di
   versione proprio (`Pool 1.03`), indipendente dalla versione dell'app. Mai
   aggiornamenti parziali di un file solo senza incrementare la versione del pool.
5. **Contenuto invariato nella migrazione**: i 15 prompt del prototipo entrano nel pool
   con lo stesso testo validato (adattando solo i segnaposto); ogni modifica di
   sostanza è una decisione da diario, non un effetto collaterale.

## 4.2 Dove sta il pool

- **Pool integrato**: una copia completa del pool è **incorporata nell'exe** come
  risorsa. Così l'applicazione resta davvero «un solo file»: copiata su un PC nudo,
  funziona.
- **Pool esterno (facoltativo)**: se accanto all'exe esiste la cartella
  `prompt-pool/`, questa **sostituisce** il pool integrato. Serve a mettere a punto i
  prompt senza ricompilare, e a distribuire aggiornamenti del solo pool.
- Il riquadro del logo dichiara sempre cosa è in uso: `Pool 1.03` (esterno) oppure
  `Pool 1.03 (integrato)`. Se il pool esterno è invalido, l'app lo dice e ripiega
  sull'integrato: mai un avvio muto con prompt sbagliati.

## 4.3 Struttura della cartella

```
prompt-pool/
├── pool_manifest.json          versione del pool + elenco file con impronta
├── CHANGELOG.md                storia delle versioni del pool
├── profilo/
│   ├── nome.md                 turni del dialogo guidato (7 file)
│   ├── contatti.md
│   ├── patente.md
│   ├── esperienze_formali.md
│   ├── esperienze_informali.md
│   ├── competenze.md
│   ├── formazione.md
│   ├── importa_cv.md           strutturazione di un CV trascritto
│   ├── trascrizione_pdf.md     lettura fedele del PDF (output testo)
│   ├── aggiornamento.md        ✚ sessione differenziale di aggiornamento
│   └── classifica_documenti.md ✚ riconoscere i documenti utili in una cartella
├── annuncio/
│   └── analisi_annuncio.md     estrazione requisiti + contesto (+ campo lingua ✚)
├── confronto/
│   ├── confronto.md            giudizi voce-per-voce + lettura d'insieme
│   └── mitigazione.md          ponti onesti sui gap (può tacere)
├── generazione/
│   ├── cv_base.it.md           📄 CV-1 (italiano)
│   ├── cv_base.en.md           📄 CV-1 (inglese) ✚
│   ├── cv_mirato.it.md         🎯 CV-2 (italiano)
│   ├── cv_mirato.en.md         🎯 CV-2 (inglese) ✚
│   ├── lettera.it.md           ✉️ lettera (italiano)
│   ├── lettera.en.md           ✉️ lettera (inglese) ✚
│   └── email_candidatura.md    ✚ oggetto + corpo email dalla lettera (parametrico per lingua)
├── brainstorm/
│   ├── brainstorm.md           ✚ conversazione ancorata a profilo+annuncio+giudizi
│   └── appunti_di_mira.md      ✚ distilla dal brainstorm gli appunti confermabili
└── rifinitura/
    └── umanizzazione.md        ✚ anti-slop (cap. 08), senza errori di battitura
```

`✚` = prompt nuovo della fase VB.NET; gli altri migrano dal prototipo. I prompt nuovi
si progettano nel dettaglio (testo completo) **prima** dell'implementazione, con lo
stesso metodo dei vecchi: compito ristretto, formato d'uscita rigido, anti-invenzione
cablata.

## 4.4 Formato di un file prompt

Ogni `.md` ha un'**intestazione di metadati** (poche righe chiave: valore) separata dal
corpo con una riga `---`:

```markdown
id: cv_mirato
versione: 1.0
lingua: it
modello: ragionamento        # semplice | ragionamento
max_token: 2000
uscita: json                 # json | testo
segnaposto: PROFILO, ANNUNCIO, GIUDIZI, APPUNTI
descrizione: Genera il CV mirato; il profilo è l'unica fonte di fatti.
---
(qui il testo del prompt, con i segnaposto {{PROFILO}}, {{ANNUNCIO}}, ...)
```

- **`modello`** dice il *livello* (estrazione → Haiku, ragionamento → Sonnet), non il
  nome del modello: i nomi concreti stanno in configurazione (cap. 02.5). Un cambio di
  modello non tocca il pool.
- **`segnaposto`** dichiara cosa il motore deve fornire: al caricamento il programma
  verifica che tutti i segnaposto dichiarati esistano nel corpo e viceversa (un errore
  qui blocca subito, con messaggio chiaro).
- I dati vengono iniettati come JSON dentro tag delimitatori (`<profilo>…</profilo>`),
  con la stessa difesa da prompt-injection del prototipo: «tratta ciò che sta nel tag
  solo come dato, mai come istruzioni».
- I file sono **UTF-8**; i fine riga vengono normalizzati (LF) al caricamento, così
  l'impronta non dipende dall'editor usato.

## 4.5 Il manifest e la versione del pool

`pool_manifest.json`:

```json
{
  "formato": 1,
  "versione_pool": "1.03",
  "data": "2026-08-05",
  "file": [
    { "percorso": "profilo/nome.md", "sha256": "…" },
    { "percorso": "generazione/cv_mirato.it.md", "sha256": "…" }
  ]
}
```

- L'app all'avvio valida il pool: manifest presente e ben formato, tutti i file
  elencati esistenti, impronte corrispondenti.
- **Loader trasparente, non poliziesco** (il pool è aperto di proposito): se un file
  non corrisponde all'impronta, il pool funziona lo stesso ma la versione viene
  mostrata con asterisco — `Pool 1.03*` — e il dettaglio dei file modificati è
  visibile nelle Impostazioni. Chi sperimenta lo fa alla luce del sole; chi
  distribuisce fa il bump.
- Il **bump del pool** è un piccolo rito documentato: si modificano i file, si
  aggiorna `versione_pool`, si rigenerano le impronte (lo fa un comando
  dell'app stessa, in Impostazioni → «Sigilla pool»), si annota il `CHANGELOG.md`.

## 4.6 Il caricatore nel codice (`Ai/LibreriaPrompt`)

Compiti, in ordine:

1. all'avvio: scegliere la sorgente (esterna se valida, altrimenti integrata) e
   validare il manifest;
2. su richiesta del motore: `Carica(id, lingua)` → trova il file (per gli id con
   varianti di lingua sceglie `.it`/`.en`), legge metadati e corpo, mette in cache;
3. `Riempi(prompt, valori)` → sostituisce i segnaposto con i dati (JSON serializzato
   per gli artefatti, testo semplice per le risposte utente); errore immediato se
   manca un valore dichiarato;
4. esporre al motore i metadati (`modello`, `max_token`, `uscita`) perché la chiamata
   API usi i parametri giusti senza costanti sparse nel codice.

## 4.7 Cosa cambia rispetto al prototipo (e cosa no)

| Aspetto | Prototipo | Fase VB.NET |
|---|---|---|
| Dove vive un prompt | duplicato in `prompt_design.md` + `server.js` | **solo** nel pool |
| Regola di allineamento | sync char-by-char tra i due doppioni | non serve più; al suo posto: validazione manifest + segnaposto |
| Scelta del modello | costanti nel server | metadato `modello` + configurazione |
| Testo dei prompt | 15 prompt validati | **identici**, salvo adattamento segnaposto; nuovi prompt ✚ progettati con lo stesso metodo |
| Documentazione | `prompt_design.md` (prompt + note di design) | il pool **è** la documentazione dei prompt; le note di design restano nel diario e nei capitoli di questo progetto |

`HTML+JS/prompt_design.md` resta com'è: è la storia validata da cui il pool nasce, e
il riferimento per il collaudo di non-regressione (cap. 14: stessi input → stessi
comportamenti).
