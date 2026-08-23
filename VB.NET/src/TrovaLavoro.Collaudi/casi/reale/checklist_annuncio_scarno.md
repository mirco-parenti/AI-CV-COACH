# Checklist «Problemi e mitigazioni» — l'annuncio scarno

*Collaudo di tappa di T9 (cap. 14), voci **7** della checklist ereditata dal prototipo (`HTML+JS/prompt_design.md`). Quattro righe che dicono mansione, sede e contratto, e nient'altro: tutto ciò che un magazziniere «di solito» deve avere qui non è scritto. Persone e aziende sono inventate: per questo il rapporto sta nel repo.*

- **Quando**: 2026-08-23 21:36

L'annuncio dato in pasto all'analisi:

> Cercasi magazziniere per la nostra sede di Forlì.
> Contratto a tempo determinato, full time.
> Inviare la candidatura a lavoro@example.it

## Requisiti «tipici» non scritti

*Voce 7.*

✅ Ogni voce estratta si ritrova nel testo dell'annuncio: l'analisi non ha aggiunto niente di plausibile.

## Cosa ne è uscito

*Le liste vuote sono la risposta giusta a un annuncio che tace: si leggono come tali, non come un'estrazione mancata.*

```json
{
  "competenze_richieste": [],
  "esperienza_richiesta": [],
  "formazione_richiesta": [],
  "altri_requisiti": [],
  "titolo": "magazziniere",
  "azienda": "",
  "sede": [
    "Forlì"
  ],
  "contratto": {
    "tipo": "tempo determinato",
    "durata": "",
    "orario": "full time",
    "retribuzione": ""
  },
  "mansioni": [],
  "benefit": [],
  "lingua": "it",
  "contatto": {
    "email": "lavoro@example.it",
    "riferimento": ""
  }
}
```

