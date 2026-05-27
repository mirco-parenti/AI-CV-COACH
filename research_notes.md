# Research Notes

Questo documento raccoglie appunti sull'analisi di progetti simili ad AI-CV-COACH.

## Obiettivo dell'analisi

Studiare strumenti open source o applicazioni simili per capire:

- quali problemi risolvono;
- come strutturano l'interfaccia;
- come analizzano annunci di lavoro;
- come analizzano profili professionali o CV;
- come generano risultati personalizzati;
- se e come limitano il rischio di inventare informazioni;
- quali idee possono essere utili per il nostro progetto.

## Griglia di analisi

Per ogni progetto osservato, annotare:

### Nome progetto

### Link

### Problema risolto

### Funzionalità principali

### Tecnologie usate

### Flusso utente

### Uso dell'AI

### Gestione del rischio di informazioni inventate

### Idee utili per AI-CV-COACH

### Limiti osservati

---

## Progetto analizzato 1

### Nome progetto
Resume-Matcher (srbhr/Resume-Matcher)

### Link
https://github.com/srbhr/Resume-Matcher

### Problema risolto
Aiuta i candidati a non essere scartati automaticamente dai sistemi ATS,
analizzando la compatibilità del CV con l'annuncio e individuando le parole
chiave e le lacune di contenuto.

### Funzionalità principali
- Punteggio di match istantaneo tra CV e annuncio.
- Ottimizzazione delle parole chiave rispetto all'annuncio.
- Suggerimenti guidati per migliorare il CV.
- Analisi di compatibilità con i sistemi ATS.

### Tecnologie usate
- Backend: FastAPI (Python).
- Frontend: Next.js.
- LiteLLM per supportare più provider AI.
- Ollama per esecuzione locale dei modelli.
- SQLite per il database, Tailwind CSS per lo stile.

### Flusso utente
1. L'utente carica un "master resume" completo (tutte le esperienze reali).
2. Incolla un annuncio di lavoro.
3. Il sistema estrae competenze e requisiti da entrambi i documenti.
4. Esegue un confronto per identificare corrispondenze e lacune.
5. Genera suggerimenti mirati su cosa enfatizzare e cosa manca.

### Uso dell'AI
L'AI viene usata per estrarre informazioni dai documenti, confrontarli e
generare suggerimenti. I compiti sono ristretti e separati in fasi.

### Gestione del rischio di informazioni inventate
Il sistema parte da un profilo completo e reale (master resume) e si limita
a selezionare, evidenziare e riordinare ciò che è già presente. Non chiede
all'AI di "scrivere un CV", ma di lavorare solo sul materiale fornito.
Il rischio di invenzione è ridotto progettando il sistema: fonte chiusa di
dati, compiti ristretti e fasi separate.

### Idee utili per AI-CV-COACH
- Partire da un profilo master completo da cui pescare per ogni annuncio.
- Separare estrazione, confronto e generazione di suggerimenti.
- Far selezionare ed enfatizzare invece di riscrivere liberamente.

### Limiti osservati
- Tecnicamente molto più complesso del nostro progetto (Docker, modelli locali).
- Non prevede un dialogo guidato per costruire il profilo da zero,
  che è invece una caratteristica distintiva di AI-CV-COACH.