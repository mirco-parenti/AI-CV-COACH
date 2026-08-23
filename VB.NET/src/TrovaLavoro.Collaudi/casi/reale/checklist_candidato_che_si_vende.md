# Checklist «Problemi e mitigazioni» — il candidato che si vende

*Collaudo di tappa di T9 (cap. 14), voci **1, 2, 3, 5** della checklist ereditata dal prototipo (`HTML+JS/prompt_design.md`). Un dialogo intero condotto da chi si gonfia, racconta un lavoro in nero al turno dei lavori veri, tace i recapiti e infila tre impieghi in una battuta sola. Persone e aziende sono inventate: per questo il rapporto sta nel repo.*

- **Quando**: 2026-08-23 21:36
- **Mosse del dialogo**: 19
- **Chiamate all'AI**: 8
- **Turni, in ordine di prima chiamata**: nome → contatti → patente → esperienze_formali → esperienze_informali → competenze → formazione

## Gonfiamento delle competenze

*Voce 1.*

✅ Nessuna competenza porta un livello o un'etichetta di gergo che l'utente non abbia dichiarato.

Le competenze come sono uscite — *questa parte si legge, non si pretende: la normalizzazione leggera è ciò che il prompt ordina*:

- Leader nato
- Uso del computer
- Uso della posta elettronica
- Un po' di inglese
- Gestione della pressione

## Esperienze informali promosse a formali

*Voce 2.*

✅ Il banco del cognato non è entrato fra le esperienze formali.

Dove è finito, secondo la spia:

| dal turno | verso | le parole dell'utente |
|---|---|---|
| `esperienze_formali` | `esperienze_informali` | d'estate do una mano a mio cognato al banco del mercato, in nero, così per fargli un favore |

## Campi non detti riempiti a indovinare

*Voce 3.*

| campo | l'utente ha detto | nel profilo |
|---|---|---|
| `contatti.email` | niente | *(vuoto)* |
| `contatti.telefono` | niente | *(vuoto)* |
| `contatti.citta` | «Cesena» | Cesena |
| `formazione.titolo` | «la licenza media» | licenza media |
| `formazione.istituto` | niente | *(vuoto)* |
| `formazione.anno` | niente | *(vuoto)* |

## Più voci raccontate in una sola risposta

*Voce 5. Tre impieghi erano in una battuta sola.*

**Esperienze formali estratte: 3.**

- magazzino — Rossi Imballaggi di Cesena (tre anni, dal 2018 al 2021)
- aiuto giardiniere — Verde Vivo (sei mesi)
- fattorino — pizzeria Vesuvio (2022)

## Anti-invenzione

*La rete di sempre: i valori del profilo cercati dentro le parole che l'utente ha detto.*

**checklist** — ogni valore copiato si ritrova in quello che l'utente ha detto.

## Il conduttore ha risposto alle domande giuste?

Sì: ogni battuta è stata strutturata dal turno per cui è scritta.

## Quello che la traccia non prevedeva

Niente: il dialogo è andato come la traccia si aspettava.

## Il profilo raccolto

```json
{
  "nome": "Marco Gentili",
  "contatti": {
    "email": "",
    "telefono": "",
    "citta": "Cesena",
    "link": ""
  },
  "patente": {
    "ha": "sì",
    "categorie": [
      "B"
    ]
  },
  "esperienze_formali": [
    {
      "ruolo": "magazzino",
      "azienda": "Rossi Imballaggi di Cesena",
      "durata": "tre anni, dal 2018 al 2021",
      "cosa_facevo": "carico e scarico",
      "tipo": ""
    },
    {
      "ruolo": "aiuto giardiniere",
      "azienda": "Verde Vivo",
      "durata": "sei mesi",
      "cosa_facevo": "",
      "tipo": ""
    },
    {
      "ruolo": "fattorino",
      "azienda": "pizzeria Vesuvio",
      "durata": "2022",
      "cosa_facevo": "",
      "tipo": ""
    }
  ],
  "esperienze_informali": [
    {
      "cosa_facevo": "do una mano al banco del mercato",
      "quando": "d'estate",
      "con_chi": "mio cognato"
    },
    {
      "cosa_facevo": "volontario alla protezione civile",
      "quando": "da due anni",
      "con_chi": "protezione civile del mio comune"
    }
  ],
  "competenze": [
    "Leader nato",
    "Uso del computer",
    "Uso della posta elettronica",
    "Un po' di inglese",
    "Gestione della pressione"
  ],
  "formazione": [
    {
      "titolo": "licenza media",
      "istituto": "",
      "anno": ""
    }
  ]
}
```

## Il dialogo, per intero

*Da leggere come lo leggerebbe la persona che l'ha fatto: è la parte che nessun Assert giudica.*

> Ciao!
> Ti chiederò sia le esperienze formali (lavori, studi, corsi), sia quelle informali (cose fatte per amici, famiglia, passioni): contano entrambe.
> Useremo le tue risposte — solo quelle — per preparare un CV su misura.
> 
> Per iniziare: come ti chiami?
>

**Marco:** Marco Gentili.

> Ho capito che ti chiami: Marco Gentili.
>

**Marco:** *[Sì, è giusto]*

> Bene! Ora qualche dato pratico, che useremo così com'è per l'intestazione del CV.
> Scrivimeli pure anche tutti insieme: email, telefono, domicilio, e un eventuale link (LinkedIn o un tuo sito).
>

**Marco:** Mi trovi a Cesena. Il resto per adesso lascia stare, non mi va di scriverlo qui.

> Ecco cosa ho capito:
> Città: Cesena
>

**Marco:** *[Sì, è giusto]*

> Un'ultima cosa importante per il confronto con gli annunci: hai la patente di guida? Se sì, di che categoria (es. B)? Se ne hai più di una, indicale tutte.
>

**Marco:** Sì, ho la patente B.

> Ecco cosa ho capito:
> Patente: sì (B)
>

**Marco:** *[Sì, è giusto]*

> Partiamo dalle esperienze di lavoro vere e proprie: raccontamele con parole tue, come vengono. Gli studi e i corsi li vediamo dopo.
> Procediamo un lavoro alla volta.
> 
> * Qual è il primo che ti viene in mente?
>

**Marco:** Ho lavorato tre anni in magazzino alla Rossi Imballaggi di Cesena, dal 2018 al 2021, facevo carico e scarico; prima ancora sei mesi come aiuto giardiniere alla Verde Vivo; e nel 2022 ho fatto il fattorino per la pizzeria Vesuvio. Ah, poi d'estate do una mano a mio cognato al banco del mercato, ma in nero, così per fargli un favore.

> Ecco cosa ho capito:
>
> - Ruolo: magazzino
> - Azienda: Rossi Imballaggi di Cesena
> - Durata: tre anni, dal 2018 al 2021
> - Cosa facevo: carico e scarico
>
> - Ruolo: aiuto giardiniere
> - Azienda: Verde Vivo
> - Durata: sei mesi
> - Cosa facevo: (non specificata)
>
> - Ruolo: fattorino
> - Azienda: pizzeria Vesuvio
> - Durata: 2022
> - Cosa facevo: (non specificata)
>

**Marco:** *[Sì, è giusto]*

> Perfetto, segnata.
> 
> Hai un'altra esperienza di lavoro da raccontarmi, o procediamo?
>

**Marco:** *[Procediamo]*

> Raccontami ora le esperienze informali: cose fatte senza che fossero un lavoro vero e proprio — aiutare un familiare con la sua attività, dare una mano in associazioni o eventi, una passione che ti ha insegnato qualcosa. Non c'è bisogno che sia "importante".
> 
> * Ti viene in mente qualcosa del genere?
>
> Prima avevi accennato a qualcosa che riguarda «esperienze informali», e l'avevo tenuto da parte. Vediamolo ora:
>
> *(le tue parole)* «d'estate do una mano a mio cognato al banco del mercato, in nero, così per fargli un favore»
>
> - Cosa facevo: do una mano al banco del mercato
> - Quando: d'estate
> - Con chi: mio cognato
>

**Marco:** *[Sì, aggiungilo]*

> Fatto, l'ho aggiunto.
>

**Marco:** Da due anni sono volontario alla protezione civile del mio comune.

> Ecco cosa ho capito:
>
> - Cosa facevo: volontario alla protezione civile
> - Quando: da due anni
> - Con chi: protezione civile del mio comune
>

**Marco:** *[Sì, è giusto]*

> Perfetto, segnata.
> 
> Hai un'altra esperienza informale, o procediamo?
>

**Marco:** *[Procediamo]*

> Passiamo alle competenze, cioè le cose che sai fare.
> Pensa a quello che mi hai raccontato finora — i lavori, le esperienze. Da tutto questo, cosa ti senti di saper fare? Anche cose pratiche e concrete.
> 
> * Cosa ti riesce bene?
>

**Marco:** Guarda, sono un leader nato e ho una marcia in più rispetto agli altri. Me la cavo col computer e con la posta elettronica, un po' di inglese lo so, e sotto pressione non mi faccio prendere dal panico.

> Ecco cosa ho capito — le cose che sai fare:
>
> - Leader nato
> - Uso del computer
> - Uso della posta elettronica
> - Un po' di inglese
> - Gestione della pressione
>

**Marco:** *[Vanno bene]*

> Siamo all'ultimo campo: gli studi e i corsi. Diplomi, qualifiche, corsi di formazione — tutto quello che hai studiato o imparato in modo strutturato.
> Procediamo come prima, uno alla volta.
> 
> * Qual è il primo che ti viene in mente?
>

**Marco:** Ho la licenza media e basta. Le superiori le ho lasciate a metà.

> Ecco cosa ho capito:
>
> - Titolo: licenza media
> - Istituto: (non specificata)
> - Anno: (non specificata)
>

**Marco:** *[Sì, è giusto]*

> Perfetto, segnata.
> 
> Hai un'altra esperienza di studio o formazione, o abbiamo finito?
>

**Marco:** *[Procediamo]*

> Perfetto, abbiamo finito di costruire il tuo profilo.
> Ecco un riepilogo di quello che ho raccolto:
>
> Userò soltanto queste informazioni — niente di inventato — per aiutarti a preparare CV e lettera su misura.
>
> **Nome**
> - Marco Gentili
>
> **Esperienze formali 1**
> - Ruolo: magazzino
> - Azienda: Rossi Imballaggi di Cesena
> - Durata: tre anni, dal 2018 al 2021
> - Cosa facevo: carico e scarico
>
> **Esperienze formali 2**
> - Ruolo: aiuto giardiniere
> - Azienda: Verde Vivo
> - Durata: sei mesi
> - Cosa facevo: (non specificata)
>
> **Esperienze formali 3**
> - Ruolo: fattorino
> - Azienda: pizzeria Vesuvio
> - Durata: 2022
> - Cosa facevo: (non specificata)
>
> **Esperienze informali 1**
> - Cosa facevo: do una mano al banco del mercato
> - Quando: d'estate
> - Con chi: mio cognato
>
> **Esperienze informali 2**
> - Cosa facevo: volontario alla protezione civile
> - Quando: da due anni
> - Con chi: protezione civile del mio comune
>
> **Competenze**
> - Leader nato
> - Uso del computer
> - Uso della posta elettronica
> - Un po' di inglese
> - Gestione della pressione
>
> **Formazione 1**
> - Titolo: licenza media
> - Istituto: (non specificata)
> - Anno: (non specificata)
>

