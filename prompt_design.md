# Prompt Design

Questo documento raccoglie i prompt progettati e usati nello sviluppo di AI-CV-COACH.

## Obiettivo

Documentare come l'AI viene usata nelle diverse parti del sistema, mantenendo separati i compiti di analisi, confronto e generazione.

## Principi guida

- Non inventare informazioni non presenti nel profilo utente.
- Separare estrazione dati, valutazione match e generazione CV.
- Richiedere output strutturati quando possibile.
- Rendere verificabile ogni affermazione generata.
- Presentare il punteggio di match come orientativo.

## Struttura dati del profilo utente (schema MVP)

Lo schema dati del profilo e la base condivisa su cui poggia tutta la
pipeline. Tutti i prompt devono rispettare questa struttura come unico
formato di scambio interno.

### Schema JSON (vuoto)

```json
{
  "nome": "",
  "esperienze_formali": [],
  "esperienze_informali": [],
  "competenze": [],
  "formazione": []
}
```

### Schema JSON (con voci di esempio)

```json
{
  "nome": "Mario Rossi",
  "esperienze_formali": [
    {
      "ruolo": "Cameriere",
      "azienda": "Bar Centrale",
      "durata": "1 anno",
      "cosa_facevo": "Servizio ai tavoli e gestione cassa"
    }
  ],
  "esperienze_informali": [
    {
      "cosa_facevo": "Aiutavo mio zio idraulico durante le estati",
      "quando": "2018-2020, periodi estivi",
      "con_chi": "ditta artigiana di famiglia"
    }
  ],
  "competenze": [
    "Lavoro in team",
    "Uso registratore di cassa"
  ],
  "formazione": [
    {
      "titolo": "Diploma alberghiero",
      "istituto": "Istituto Cellini",
      "anno": "2022"
    }
  ]
}
```

### Note sui campi

- `nome` — stringa. Nome e cognome dell'utente.
- `esperienze_formali` — lista. Esperienze lavorative riconosciute con
  ruolo, azienda, durata e descrizione. I sotto-campi sono attesi ma non
  obbligatori: se un dato manca (es. la durata non e ricordata con
  precisione), si lascia vuoto, non si inventa.
- `esperienze_informali` — lista. Attivita non formali (lavoretti, aiuti
  in famiglia, volontariato, esperienze brevi). **Tutti i sotto-campi
  sono opzionali per scelta progettuale.** Per natura, queste esperienze
  non hanno sempre date precise o riferimenti formali.
- `competenze` — lista di stringhe. Abilita pratiche o trasversali
  dichiarate dall'utente.
- `formazione` — lista. Titoli di studio o corsi. Sotto-campi attesi
  ma non obbligatori.

### Regole d'uso (validi per tutti i prompt che leggono o scrivono il profilo)

1. Nessun sotto-campo va riempito con informazioni non fornite
   esplicitamente dall'utente. Se manca, resta vuoto.
2. Le `esperienze_informali` non vanno mai "promosse" a
   `esperienze_formali`, neanche se sembrano arricchibili. Sono di
   natura diversa.
3. L'AI non aggiunge competenze, esperienze o titoli che l'utente non
   ha menzionato, neanche se appaiono "implicite" o "plausibili".
4. L'output strutturato e sempre JSON valido conforme a questo schema.

### Da definire piu avanti

- Eventuali campi aggiuntivi (contatti, lingue, link) dopo aver visto
  l'MVP girare.
- Formulazione del "testo visibile all'utente" per le esperienze
  informali nei prompt di generazione CV (decisione rimandata al
  contesto giusto).

## Prompt previsti

### Raccolta profilo utente

Da definire.

### Analisi annuncio di lavoro

Da definire.

### Confronto profilo-annuncio

Da definire.

### Generazione CV mirato

Da definire.

### Generazione lettera di presentazione

Da definire.

## Problemi e mitigazioni

Da compilare durante lo sviluppo.