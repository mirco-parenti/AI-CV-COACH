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
│         [⚙ Impostazioni]     stato AI · modello · consumo contesto │
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
- Finestra principale: avvio massimizzata, `MinimumSize` 1150×600, DPI `SystemAware`,
  sfondo `SfondoBase`.
- Finestre secondarie (Impostazioni, Primo avvio, Anteprima file, Informazioni su…):
  dialoghi a bordo fisso, sfondo bianco, titolo Segoe UI 14 Bold in `RossoTitoli`.

## 3.5 Il pannello del logo (in basso a sinistra)

Elemento identitario irrinunciabile, presente in ogni momento nell'angolo in basso a
sinistra della finestra principale:

```
┌──────────────────────────┐
│        [immagine]        │   PictureBox 101×101, logo del progetto
│                          │
│       AI-CV-COACH        │   Segoe UI 16 Bold, TestoPrimario, centrato
│  Ver. 1.0.012 · Pool 1.03│   Segoe UI 8, TestoSecondario, centrato
│  © 2026 Mirco Parenti    │   Segoe UI 8, TestoSecondario, centrato
└──────────────────────────┘
```

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
- Il logo è **risorsa incorporata** nell'exe (niente file immagine esterni). Il disegno
  del logo è una decisione aperta (cap. 15): serve un'immagine propria del progetto.
- Il numero di versione dell'app vive in **un solo file sorgente** (`Versione.vb`, una
  costante), mai duplicato altrove; ogni modifica al codice lo incrementa.

## 3.6 I pannelli, uno per uno

| ID | Pannello | Contenuto principale |
|---|---|---|
| **P1 Home** | cruscotto | stato del profilo (esiste? aggiornato quando?), coda opportunità con stelle e stati, scorciatoie ai flussi («Nuova ricerca», «Aggiorna profilo») |
| **P2 Profilo** | scheda del profilo | tutte le sezioni del profilo JSON **campo per campo, modificabili**; bottoni: «Importa da CV/cartella» (L2), «Costruiscilo con il dialogo» (L2), «Sessione di aggiornamento» (L2), «Genera 📄 CV-1 base» (L3), «Esporta backup» (L2) |
| **P3 Ricerca** | browser integrato | WebView2 a tutta area; sopra: barra con ricerche salvate (ComboBox), campo link, bottone **«Cattura annuncio»** (L1); sotto: ultima cattura con esito |
| **P4 Opportunità** | dettaglio candidatura | annuncio estratto, **stelle 0–5 grandi**, elenco giudizi (✓ ~ ✗ ?) con ⛔ sugli eliminatori, note di clamp/gate, lettura d'insieme; bottoni: «Brainstorm» (L2), «Genera CV+lettera» (L3), «Scarta» (L5) |
| **P5 Dialogo** | conversazione | pannello chat riusato per tre scopi: dialogo guidato del profilo, sessione di aggiornamento, brainstorming sull'opportunità; schede di conferma inline per i turni del profilo |
| **P6 Documenti** | anteprima e rifinitura | anteprima del CV e della lettera affiancate all'annuncio (per il 📄 CV-1 base, generato senza annuncio, la colonna annuncio resta vuota); scelta lingua IT/EN; prima/dopo della rifinitura anti-slop; bottoni: «Esporta DOCX» «Esporta PDF» (L2), «Prepara email» (L3) |
| **P7 Email** | composizione | destinatario, oggetto, corpo, elenco allegati (con quelli suggeriti dalla cartella documenti), bottoni: «Salva .eml/.msg» (L2), **«Invia»** (L6, con conferma) |
| **P8 Impostazioni** | finestra separata | chiave API (mascherata), account SMTP, cartella dati, cartella documenti, modelli AI, lingua predefinita output, interruttore della rifinitura anti-slop (cap. 08.4), gestione del pool («Sigilla pool», dettaglio dei file modificati — cap. 04.5), export/import backup (cap. 11.4), pulizia dati («Svuota dati di navigazione», eliminazioni — cap. 11.5) |

Ogni pannello ha in alto il **titolo in `RossoTitoli`** e un sottotitolo grigio che dice
a che punto del flusso siamo («Passo 2 di 4 — Confronto»).

## 3.7 Naming dei controlli (per il designer)

Prefissi standard, nome semantico in PascalCase: `pnl` (Panel), `btn` (Button),
`lbl` (Label), `txt` (TextBox), `cmb` (ComboBox), `chk` (CheckBox), `pic` (PictureBox),
`grp` (GroupBox), `lst`/`lvw` (liste), `wv` (WebView2), `cms` (menu contestuale),
`tmr` (Timer). Esempi: `btnCatturaAnnuncio`, `lblStelleMatch`, `wvRicerca`,
`pnlLogo`, `lblVersione`.

## 3.8 Feedback e stati

- **Badge di stato** (pannellino 115×26 con etichetta bold centrata): verde OK, azzurro
  info, giallo attenzione, rosso errore — usati per lo stato delle opportunità e delle
  chiamate AI.
- Operazioni AI in corso: indicatore nella barra di stato + testo in streaming dove
  previsto (cap. 02); mai una finestra bloccata.
- Errori: messaggio in italiano nel contesto in cui è avvenuto (non solo un popup),
  sempre con un'azione possibile («Riprova», «Salta»).
