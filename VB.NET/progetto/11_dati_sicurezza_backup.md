# 11 — Dati, sicurezza e backup

*Dove vivono i dati dell'utente, come sono protetti i segreti, come si salva e si
ripristina tutto. File JSON leggibili, niente database: la trasparenza è parte del
prodotto.*

## 11.1 La cartella dati

Default: `%APPDATA%\AI-CV-COACH` (modificabile al primo avvio o nelle Impostazioni).

```
AI-CV-COACH\
├── config.json            impostazioni (modelli AI, cartelle, account SMTP senza password)
├── ricerche.json          preferenze di ricerca, ricerche salvate e tabella dei portali
├── segreti.bin            chiave API + password SMTP, cifrate (v. 11.3)
├── profilo\
│   ├── profilo.json       il profilo corrente (fonte di verità unica)
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
- **Il profilo è versionato**: ogni modifica confermata (editing, sessione di
  aggiornamento) salva una copia datata nello storico; `stato.json` di ogni
  opportunità annota **con quale versione** del profilo furono generati i documenti.
  Così un CV inviato resta sempre spiegabile, anche a profilo evoluto.
- Formati **JSON con rientri**, leggibili in qualsiasi editor: l'utente è padrone dei
  suoi dati anche senza l'app.

## 11.2 Cosa esce dal PC (e cosa no)

| Dato | Esce? | Verso dove |
|---|---|---|
| Testi per l'elaborazione (profilo, annuncio, PDF da trascrivere) | sì | solo API Anthropic, via HTTPS |
| Email di candidatura | solo su comando esplicito | il server SMTP dell'utente |
| Tutto il resto (registro, documenti, configurazione, log) | **no** | — |

Niente telemetria, niente servizi del produttore, niente aggiornamenti automatici
silenziosi.

## 11.3 I segreti

- **Chiave API Anthropic** e **password SMTP** sono cifrate con la protezione dati di
  Windows (DPAPI) **legata all'utente che le ha salvate**: il file `segreti.bin` copiato
  su un altro PC o letto da un altro account non si decifra. È il compromesso giusto
  per un'app personale: robusto, senza inventare crittografia in proprio.
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
  "app": "AI-CV-COACH",
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
