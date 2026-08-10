# Collaudo di tappa T3 — il dialogo guidato da zero

*Rapporto generato da `CollaudiDialogoReale`. Sta nel repo perché la persona è inventata: Anna Ricci non esiste, e la sua storia è scritta apposta perché l'anti-perdita e il «lasciato fuori» abbiano occasione di scattare.*

- **Quando**: 2026-08-10 20:09
- **Mosse del dialogo**: 25
- **Chiamate all'AI**: 12
- **Turni, in ordine di prima chiamata**: nome → contatti → patente → esperienze_formali → esperienze_informali → competenze → formazione

## Le quattro trappole, e dove sono finite

*Quattro pezzi della traccia sono detti nel turno sbagliato di proposito. Dove il modello li mandi è cosa da **leggere**, non da pretendere; ciò che si pretende è che di ognuno il dialogo renda conto — sotto, «reso conto».*

| trappola | detta nel turno | attesa in | instradata a | reso conto |
|---|---|---|---|---|
| il corso senza nome | `contatti` | `formazione` | *(non instradata)* | — |
| il patentino del muletto | `patente` | `formazione` | `formazione` | = |
| il posto fisso rifiutato | `competenze` | `esperienze_formali` | `esperienze_formali` | = |
| il chiosco della sorella | `formazione` | `esperienze_informali` | `esperienze_informali` | = |

*Una trappola «non instradata» non è un difetto dell'app: vuol dire che il modello ha letto quella frase come roba del turno in corso, o l'ha lasciata cadere. Se non ne resta più nessuna, la traccia va rifatta — il collaudo lo dice da sé.*

## Anti-perdita — ogni frammento instradato

| dal turno | verso | le parole dell'utente | reso conto |
|---|---|---|---|
| `patente` | `formazione` | Ho anche il patentino per il muletto | = |
| `competenze` | `esperienze_formali` | mi hanno anche offerto un posto fisso in trattoria | = |
| `formazione` | `esperienze_informali` | per due estati ho aiutato mia sorella nel suo chiosco di piadine in spiaggia | = |

## Il «lasciato fuori»

*Il gemello dell'anti-perdita: ciò che nessuna sezione sa accogliere si dichiara. Qui è una **segnalazione** e non un pass/fail — che scatti dipende da come il modello legge una frase, e pretenderlo darebbe un collaudo che lampeggia. Il suo pass/fail sta nel banco senza rete, dove il frammento che nessuno sa collocare è preparato a mano.*

- Questo non sono riuscita a collocarlo in nessuna sezione, quindi lo lascio fuori: «mi hanno anche offerto un posto fisso in trattoria».

## Il conduttore ha risposto alle domande giuste?

*Ogni battuta della traccia è scritta per un turno preciso. Se una finisce in un altro turno, il giro non misura il dialogo: misura il conduttore che è andato fuori passo. A dirlo non è lui, è il prompt che il dialogo ha chiamato per strutturarla.*

Sì: ogni battuta è stata strutturata dal turno per cui è scritta.

## Quello che la traccia non prevedeva

Niente: il dialogo è andato come la traccia si aspettava.

## Anti-invenzione

*I valori del profilo cercati dentro le parole che l'utente ha detto. `cosa_facevo` e le competenze restano fuori: il modello li riformula per mestiere, e a giudicarli è chi legge.*

**dialogo** — ogni valore copiato si ritrova in quello che l'utente ha detto.

## Il profilo raccolto

| sezione | voci |
|---|---|
| esperienze formali | 2 |
| esperienze informali | 2 |
| competenze | 5 |
| formazione | 2 |

```json
{
  "nome": "Anna Ricci",
  "contatti": {
    "email": "anna.ricci@example.it",
    "telefono": "340 1122334",
    "citta": "Forlì",
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
      "ruolo": "pulizie",
      "azienda": "supermercato Il Gabbiano di Forlì",
      "durata": "2016-2021",
      "cosa_facevo": "pulivo i reparti e gli uffici la mattina presto, prima dell'apertura",
      "tipo": ""
    },
    {
      "ruolo": "aiuto cuoca",
      "azienda": "trattoria Da Vittorio",
      "durata": "dal 2021",
      "cosa_facevo": "preparo le verdure, gli antipasti e do una mano quando escono i piatti",
      "tipo": ""
    }
  ],
  "esperienze_informali": [
    {
      "cosa_facevo": "cucino per duecento persone",
      "quando": "da tre anni",
      "con_chi": "altre volontarie alla sagra del mio paese"
    },
    {
      "cosa_facevo": "aiutato mia sorella nel suo chiosco di piadine in spiaggia",
      "quando": "due estati",
      "con_chi": "mia sorella"
    }
  ],
  "competenze": [
    "Cucina per grandi numeri",
    "Velocità",
    "Precisione",
    "Disponibilità a svegliarsi presto",
    "Uso di lavapavimenti industriali"
  ],
  "formazione": [
    {
      "titolo": "patentino per il muletto",
      "istituto": "",
      "anno": ""
    },
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

**Anna:** Buongiorno, mi chiamo Anna Ricci.

> Ho capito che ti chiami: Anna Ricci.
>

**Anna:** *[Sì, è giusto]*

> Bene! Ora qualche dato pratico, che useremo così com'è per l'intestazione del CV.
> Scrivimeli pure anche tutti insieme: email, telefono, domicilio, e un eventuale link (LinkedIn o un tuo sito).
>

**Anna:** anna.ricci@example.it, il numero è 340 1122334, abito a Forlì in via del Mulino 12. Ah, e ho fatto anche un corso, ma non mi ricordo più né quale né dove.

> Ecco cosa ho capito:
> Email: anna.ricci@example.it
> Telefono: 340 1122334
> Domicilio: Forlì
>

**Anna:** *[Sì, è giusto]*

> Un'ultima cosa importante per il confronto con gli annunci: hai la patente di guida? Se sì, di che categoria (es. B)? Se ne hai più di una, indicale tutte.
>

**Anna:** Sì, ho la patente B. Ho anche il patentino per il muletto, se può servire.

> Ecco cosa ho capito:
> Patente: sì (B)
>

**Anna:** *[Sì, è giusto]*

> Partiamo dalle esperienze di lavoro vere e proprie: raccontamele con parole tue, come vengono. Gli studi e i corsi li vediamo dopo.
> Procediamo un lavoro alla volta.
> 
> * Qual è il primo che ti viene in mente?
>

**Anna:** Per cinque anni ho fatto le pulizie al supermercato Il Gabbiano di Forlì, dal 2016 al 2021: pulivo i reparti e gli uffici la mattina presto, prima dell'apertura.

> Ecco cosa ho capito:
>
> - Ruolo: pulizie
> - Azienda: supermercato Il Gabbiano di Forlì
> - Durata: 2016-2021
> - Cosa facevo: pulivo i reparti e gli uffici la mattina presto, prima dell'apertura
>

**Anna:** *[Sì, è giusto]*

> Perfetto, segnata.
> 
> Hai un'altra esperienza di lavoro da raccontarmi, o procediamo?
>

**Anna:** *[Ne ho un'altra]*

> Raccontami la prossima.
>

**Anna:** Poi dal 2021 lavoro come aiuto cuoca alla trattoria Da Vittorio, sempre a Forlì: preparo le verdure, gli antipasti e do una mano quando escono i piatti.

> Ecco cosa ho capito:
>
> - Ruolo: aiuto cuoca
> - Azienda: trattoria Da Vittorio
> - Durata: dal 2021
> - Cosa facevo: preparo le verdure, gli antipasti e do una mano quando escono i piatti
>

**Anna:** *[Sì, è giusto]*

> Perfetto, segnata.
> 
> Hai un'altra esperienza di lavoro da raccontarmi, o procediamo?
>

**Anna:** *[Procediamo]*

> Raccontami ora le esperienze informali: cose fatte senza che fossero un lavoro vero e proprio — aiutare un familiare con la sua attività, dare una mano in associazioni o eventi, una passione che ti ha insegnato qualcosa. Non c'è bisogno che sia "importante".
> 
> * Ti viene in mente qualcosa del genere?
>

**Anna:** Da tre anni do una mano alla sagra del mio paese: cucino per duecento persone insieme alle altre volontarie.

> Ecco cosa ho capito:
>
> - Cosa facevo: cucino per duecento persone
> - Quando: da tre anni
> - Con chi: altre volontarie alla sagra del mio paese
>

**Anna:** *[Sì, è giusto]*

> Perfetto, segnata.
> 
> Hai un'altra esperienza informale, o procediamo?
>

**Anna:** *[Procediamo]*

> Passiamo alle competenze, cioè le cose che sai fare.
> Pensa a quello che mi hai raccontato finora — i lavori, le esperienze. Da tutto questo, cosa ti senti di saper fare? Anche cose pratiche e concrete.
> 
> * Cosa ti riesce bene?
>

**Anna:** So cucinare per tanta gente, sono veloce e precisa, e non mi pesa alzarmi presto. So usare le lavapavimenti industriali.

> Ecco cosa ho capito — le cose che sai fare:
>
> - Cucina per grandi numeri
> - Velocità
> - Precisione
> - Disponibilità a svegliarsi presto
> - Uso di lavapavimenti industriali
>

**Anna:** *[Ne aggiungo altre]*

> Dimmi pure cosa aggiungere.
>

**Anna:** Ah, poi mi hanno anche offerto un posto fisso in trattoria, ma ho dovuto dire di no per via degli orari.

> Ecco cosa ho capito — le cose che sai fare:
>
> - Cucina per grandi numeri
> - Velocità
> - Precisione
> - Disponibilità a svegliarsi presto
> - Uso di lavapavimenti industriali
>

**Anna:** *[Confermiamo]*

> Siamo all'ultimo campo: gli studi e i corsi. Diplomi, qualifiche, corsi di formazione — tutto quello che hai studiato o imparato in modo strutturato.
> Procediamo come prima, uno alla volta.
> 
> * Qual è il primo che ti viene in mente?
>
> Prima avevi accennato a qualcosa che riguarda «studi e formazione», e l'avevo tenuto da parte. Vediamolo ora:
>
> *(le tue parole)* «Ho anche il patentino per il muletto»
>
> - Titolo: patentino per il muletto
> - Istituto: (non specificata)
> - Anno: (non specificata)
>

**Anna:** *[Sì, aggiungilo]*

> Fatto, l'ho aggiunto.
>

**Anna:** Ho la licenza media e basta. Ah, e per due estati ho aiutato mia sorella nel suo chiosco di piadine in spiaggia.

> Ecco cosa ho capito:
>
> - Titolo: licenza media
> - Istituto: (non specificata)
> - Anno: (non specificata)
>

**Anna:** *[Sì, è giusto]*

> Perfetto, segnata.
> 
> Hai un'altra esperienza di studio o formazione, o abbiamo finito?
>

**Anna:** *[Procediamo]*

> Prima di chiudere, recuperiamo una cosa che avevi accennato e non avevamo ancora registrato.
>
> Prima avevi accennato a qualcosa che riguarda «esperienze di lavoro», e l'avevo tenuto da parte. Vediamolo ora:
>
> Questo non sono riuscita a collocarlo in nessuna sezione, quindi lo lascio fuori: «mi hanno anche offerto un posto fisso in trattoria».
>
> Prima di chiudere, recuperiamo una cosa che avevi accennato e non avevamo ancora registrato.
>
> Prima avevi accennato a qualcosa che riguarda «esperienze informali», e l'avevo tenuto da parte. Vediamolo ora:
>
> *(le tue parole)* «mi hanno anche offerto un posto fisso in trattoria / per due estati ho aiutato mia sorella nel suo chiosco di piadine in spiaggia»
>
> - Cosa facevo: aiutato mia sorella nel suo chiosco di piadine in spiaggia
> - Quando: due estati
> - Con chi: mia sorella
>

**Anna:** *[Sì, aggiungilo]*

> Fatto, l'ho aggiunto.
>
> Perfetto, abbiamo finito di costruire il tuo profilo.
> Ecco un riepilogo di quello che ho raccolto:
>
> Userò soltanto queste informazioni — niente di inventato — per aiutarti a preparare CV e lettera su misura.
>
> **Nome**
> - Anna Ricci
>
> **Esperienze formali 1**
> - Ruolo: pulizie
> - Azienda: supermercato Il Gabbiano di Forlì
> - Durata: 2016-2021
> - Cosa facevo: pulivo i reparti e gli uffici la mattina presto, prima dell'apertura
>
> **Esperienze formali 2**
> - Ruolo: aiuto cuoca
> - Azienda: trattoria Da Vittorio
> - Durata: dal 2021
> - Cosa facevo: preparo le verdure, gli antipasti e do una mano quando escono i piatti
>
> **Esperienze informali 1**
> - Cosa facevo: cucino per duecento persone
> - Quando: da tre anni
> - Con chi: altre volontarie alla sagra del mio paese
>
> **Esperienze informali 2**
> - Cosa facevo: aiutato mia sorella nel suo chiosco di piadine in spiaggia
> - Quando: due estati
> - Con chi: mia sorella
>
> **Competenze**
> - Cucina per grandi numeri
> - Velocità
> - Precisione
> - Disponibilità a svegliarsi presto
> - Uso di lavapavimenti industriali
>
> **Formazione 1**
> - Titolo: patentino per il muletto
> - Istituto: (non specificata)
> - Anno: (non specificata)
>
> **Formazione 2**
> - Titolo: licenza media
> - Istituto: (non specificata)
> - Anno: (non specificata)
>

