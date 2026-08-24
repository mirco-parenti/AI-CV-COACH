# Progetto dettagliato — AI-CV-COACH per Windows (fase VB.NET)

*Questo è il progetto dell'applicazione desktop che realizza la Fase 3 di AI-CV-COACH:
un solo exe per Windows 11 che accompagna l'utente dal profilo alle candidature
inviate. Il mandato è in `../PROMPT_DI_INCARICO.md`; il prototipo validato da cui
tutto nasce è nell'area `HTML+JS/` del repo.*

**Metodo**: design-first — si mette a punto **questo progetto** finché non siamo
convinti; solo dopo si scrive codice (cancello T0, cap. 14). Il livello tecnico di
riferimento è quello del perito, non dell'ingegnere: se un capitolo non si capisce,
è un difetto del capitolo.

## I capitoli

| Cap. | Titolo | In una riga |
|---|---|---|
| [01](01_visione_e_perimetro.md) | Visione e perimetro | cos'è, per chi, le funzioni F1–F8, i vincoli non negoziabili |
| [02](02_architettura.md) | Architettura | i blocchi interni, la pipeline di artefatti, cosa migra dal prototipo |
| [03](03_interfaccia_grafica.md) | Interfaccia grafica | il design di casa: token, livelli dei bottoni, pannelli, logo in basso a sinistra |
| [04](04_libreria_prompt.md) | Libreria prompt | il pool di file `.md`: formato, manifest, versione, caricatore |
| [05](05_documenti_io.md) | Documenti I/O | ingresso PDF/TXT/MD/DOCX e cartella; uscita DOCX e PDF |
| [06](06_ricerca_annunci.md) | Ricerca annunci | il browser integrato: l'utente naviga, il programma legge |
| [07](07_email_e_candidature.md) | Email e registro | `.eml` pronta da inviare, allegati, stati delle candidature |
| [08](08_qualita_del_testo.md) | Qualità del testo | la rifinitura anti-slop: naturale sì, errori mai |
| [09](09_server_mcp.md) | Server MCP | le funzioni esposte ai client AI esterni, stesso motore |
| [10](10_multilingua.md) | Multilingua | italiano/inglese per CV e lettere, senza tradire il profilo |
| [11](11_dati_sicurezza_backup.md) | Dati, sicurezza, backup | cartella dati, segreti cifrati, export/import JSON |
| [12](12_flussi_utente.md) | Flussi utente | la sceneggiatura: dal primo avvio all'ultima candidatura |
| [13](13_distribuzione.md) | Distribuzione | un solo exe: publish single-file, WebView2, versioni, come si fa un rilascio |
| [14](14_piano_di_lavoro.md) | Piano di lavoro | le tappe T0–T9, ognuna con il suo collaudo |
| [15](15_decisioni_aperte.md) | Decisioni aperte | ciò che spetta all'utente decidere prima di partire |

## Allegati

Non sono capitoli — non descrivono l'applicazione — ma stanno qui perché è qui che si
verrebbe a cercarli.

| File | In una riga |
|---|---|
| [prompt-logo.md](prompt-logo.md) | I due prompt con cui è stata generata l'illustrazione del marchio (banner e icona), le righe di ricambio e le parole che ne rovinano la resa. È la **ricetta**: le immagini che ne sono uscite stanno in `immagini/`, la lavorazione fuori dal repo. |

## Come leggerlo

- **Prima volta**: 01 → 12 → 02 (capire cosa fa e come), poi i capitoli tematici.
- **Per decidere**: 15 (con le proposte già pronte), poi 14 per il percorso.
- **Regola dei rimandi**: se due capitoli sembrano dirsi cose diverse, vince il più
  specifico; se il dubbio resta, vince il 01. Le contraddizioni scoperte si segnalano
  e si correggono.
- **Dopo il cancello T0** *(chiuso il 2026-08-05)* questi capitoli non sono più in
  scrittura libera: restano la verità del disegno e si toccano quando una decisione
  cambia o matura in implementazione — con la data della decisione accanto, così si
  vede sempre quando una cosa è stata scelta e quando invece è stata scoperta sul
  campo.

## Nota di riservatezza

Il «family feeling» richiesto dal mandato è specificato nel cap. 03 come **design
proprio** di AI-CV-COACH. In questo repo, pubblico, non compaiono dettagli
implementativi di software proprietario di terzi; eventuali strumenti esterni della
casa entrerebbero solo come eseguibili binari opzionali (cap. 13.4).
