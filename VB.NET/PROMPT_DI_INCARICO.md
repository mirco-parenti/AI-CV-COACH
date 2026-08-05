# Prompt di incarico — fase VB.NET

Questo file conserva, **verbatim**, il prompt con cui è stata aperta la fase VB.NET di
AI-CV-COACH (5 agosto 2026), seguito dalle integrazioni arrivate nella stessa sessione.
È il mandato di riferimento del progetto dettagliato in `progetto/`.

---

## Prompt originale (5 agosto 2026)

> Trovi nel repo un progetto di gestore di CV che abbiamo sviluppato. A partire da questo vogliam,o sviluppare un applicativo in vb.net che implmenti tutte le funzioni e che possa essere distribuito come .exe singolo, senza dll. I prompt che troverai vogliamo che siano collezionati ed usati come una libreria di file .md da caricare al bisogno quando il flusso del programma lo richiede. L'interfaccia grafica dovrà essere family feeling con "C:\GitHub\TTR-SUITE-ROOT" e, in particolare, vogliamo mantenere la struttura del logo in basso a sinistra con versione e pool di prompt o una cosa del genere. Tutti i pannelli dovranno essere realizzati staicamente ed editabili nel designer. Usa tutte le regole di design codificate per il progetto "C:\GitHub\TTR-SUITE-ROOT". I CV già disponibili a mano dell'utente o i documenti da allegare potranno essere in PDF o TXT o MD o DOCX oppure può venire indicata una cartella in cui ci sono tutti i documenti e il programma dovrà usare quello che serve trovandolo da solo. In output abbiamo bisogno di poter estrarre il CV tailored in docx o pdf, di generare mail da mandare con allegato quanto necessario e poi definiremo assieme il dettaglio. Importantissimo evitare AI slope e potrai se utile usare prompt tipo "C:\Users\rpsno\Desktop\AVIOLAB sviluppo\prompt-umanizzazione-testo.md" magari senza introdurre errori di battitura. Inoltre molto importante una capacità del programma di trovare e scaricare annunci di lavoro adatti alle eseigenze anche da grandi provider come linkedin, indeed etc che, spesso, usano metodi per impedire il download diretto. L'uso atteso tipico o normale della applicazione è fornire un proprio CV, indicare delle preferenze o delle tipologie di lavoro di interesse, cercare gli annunci connessi, proporre alternative o scelte relative a quanto trovato, per ogni alternativa segnalata come di interesse da parte dell'utente, fare brain storming, poi discutere con l'utente le alternative o i fix necessari, generare un CV+lettera di presentazione adatto all'annuncio, per ogni annuncio, generare delle mail per l'invio in formato .msg o direttamente pilotando un server mail per l'invio, fino a finire le opportunità. Una ulteriore funzionalità è quella di aiutare l'utente con un dialogo guidato a formare il CV alla luce delle sue esperienze. Inoltre, se l'utente ha un link di un annuncio di lavoro che giudica interessante, il propgramma dovrà permettere di utilizzarlo ed esplorarlo con flussi come indicato sopra. Altra funzionalità l'aggiornamento periodico assistito del CV di base dell'utente a mano a mano che sviluppa nuove competenze o esperienze. Ti chiedo di fare un progetto dettagliato e scriverlo in una subdir del repo che useremo come area di sviluppo. Usiamo l'italiano, il livello tecnico di riferimento è quello del perito elettronico e non dell'ingegnere, metteremo a punto il sistema a questo livello (progetto dettagliato) fino ad essere convinti di passare alla fase implementativa. Organizza il repo in due areee: HTML+JS dove mettiamop tutto il progetto attuale e VB.NET in cui mettiamo tutto lo sviluppo da adesso in avanti su queste tematiche. Puoi fare tutte le domande che vuoi, l'obiettivo deve essere un progetto sviluppato completamente, perfettamente funzionante su PC WIN11 e dovrà servire anche come carta di presentazione di Mirco, che è l'autore di AI-CV-COACH e che, per primo userà il sistema per trovare lavoro. Questo progetto viene sviluppato nell'ambito di uno tirocinio CPI che Mirco sta seguendo in Aviolab AI come da documentazione in "C:\Users\rpsno\Desktop\aviolab\Stage Mirco Parenti 2026". Appena riorganizzato il repo, salva questo prompt in un file isolato nell'area VB.NET del repo.

## Integrazioni della stessa sessione

1. **Server MCP integrato**
   > Per favore aggiungi anche un server MCP integrato che faccia tutte le funziioni che servono

2. **Multilingua**
   > Aggiungi anche la gestione multilingua italiano/inglese per CV e lettere

3. **Backup del profilo**
   > Aggiungi anche l'export del profilo in formato JSON per backup

4. **Riservatezza TTR-SUITE**
   > i dettagli implementativi di ttr-suite sono segreti e non disponibili per la pubblicazione

   > importante: ttr-suite è proprietaria, non possiamo mettere pubblici dettagli della stessa, forse in qualche caso possiamo mettere a disposizione dei programmi esterni, come pdfprocessor.exe o simili ma solo se veramente utile. In ogni caso questi dovranno essere inclusi solo come eseguibile nel progetto

---

*Nota: il testo è conservato tal quale (refusi compresi) perché è il mandato originale;
le scelte di progetto che ne derivano sono motivate nei documenti di `progetto/`.*
