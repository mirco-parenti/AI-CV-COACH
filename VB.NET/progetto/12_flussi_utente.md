# 12 — Flussi utente

*Come si usa l'applicazione, passo per passo. I pannelli citati (P1, P2, …) sono
definiti nel capitolo 03; le funzioni (F1–F8) nel capitolo 01. Questo capitolo è la
«sceneggiatura» dell'applicazione: ogni flusso qui descritto deve poter essere
percorso senza uscire dal programma.*

## 12.1 Flusso A — Il percorso tipico (dalla persona alle candidature inviate)

È l'uso «normale» dichiarato nel mandato: si parte da un CV e si finisce quando le
opportunità sono esaurite.

**A1. Primo avvio e setup**
1. L'utente avvia l'exe. Al primo avvio l'app chiede due sole cose: cartella dati
   (proposta: `%APPDATA%\TrovaLavoro`) e chiave API Anthropic (salvata cifrata,
   cap. 11). *Nessun account di posta da configurare:* l'app non spedisce (cap. 07).
2. L'app carica la libreria prompt — quella integrata nell'exe o, se presente, la
   cartella `prompt-pool/` accanto all'exe (cap. 04.2) — e ne mostra versione e
   sorgente nel riquadro del logo, in basso a sinistra.

**A2. Il profilo (F1)**
1. Bivio: **«Ho già un CV»** oppure **«Costruiamolo insieme»** (Flusso B).
2. Con «Ho già un CV»: l'utente indica **un file** (PDF, TXT, MD, DOCX) **o una
   cartella**. Se è una cartella, l'app la scandisce, riconosce da sola i documenti
   utili (cap. 05) e propone: «ho trovato questi: questo sembra il CV più recente,
   questi sembrano attestati». L'utente conferma.
3. Il CV scelto viene trascritto e strutturato nel **profilo JSON** (stessi due passi
   del prototipo: trascrizione fedele + strutturazione anti-invenzione).
4. Il profilo viene mostrato **campo per campo, modificabile** (novità rispetto al
   prototipo: si corregge la singola voce, non si ricomincia). Alla conferma, il
   profilo diventa la **fonte di verità unica**.

**A3. Le preferenze di ricerca (F2)**
1. L'utente descrive che lavoro cerca: tipologie di ruolo, zona geografica,
   contratto, parole chiave, portali preferiti.
2. Le preferenze diventano **ricerche salvate** (una per portale), riutilizzabili.

**A4. La raccolta degli annunci (F2)**
1. L'utente apre il pannello di ricerca: il browser integrato (WebView2) carica il
   portale con la ricerca salvata; l'utente naviga e, se serve, fa login come sé
   stesso.
2. Quando un annuncio interessa, preme **«Cattura annuncio»**: l'app legge il testo
   della pagina visualizzata, lo passa all'analisi (F3) e mette l'annuncio nella
   **coda delle opportunità** con titolo, azienda, fonte e link.
3. Si ripete finché si vuole: la coda può contenere quante opportunità si desidera.

**A5. Valutazione e scelta (F3)**
1. Per ogni opportunità in coda l'app mostra la scheda: estratto dell'annuncio,
   **match in stelle 0–5**, requisiti con esito (✓ soddisfatto, ~ in parte,
   ✗ non soddisfatto, ? non determinabile), eventuale **⛔ eliminatorio** che ha
   messo il tetto al punteggio (≤ 1 stella), lettura d'insieme.
2. L'utente ordina/filtra la coda per stelle e decide, opportunità per opportunità:
   **«Mi interessa»**, **«Scarto»**, **«Più tardi»**.
3. Sotto 1,5 stelle l'app **sconsiglia** la candidatura (senza impedirla), come nel
   prototipo.

**A6. Brainstorming e messa a punto (F3)**
1. Sulle opportunità marcate «Mi interessa» si apre il pannello di ragionamento (P5):
   una conversazione libera con l'AI, in italiano, con la risposta che compare in
   streaming.
2. La conversazione è **ancorata**: a ogni turno il programma invia, insieme alla
   chiacchierata, profilo + annuncio + giudizi (prompt `brainstorm` del pool). Si
   discute di punti di forza da mettere davanti, gap e come mitigarli onestamente,
   dubbi dell'utente («ho senso io per questo ruolo?»).
3. Quando l'utente è soddisfatto preme **«Trasforma in appunti»**: il prompt
   `appunti_di_mira` distilla dalla conversazione pochi punti operativi (cosa
   enfatizzare, quale mitigazione usare, tono della lettera), mostrati in una scheda
   di conferma modificabile.
4. Gli **appunti di mira** confermati si salvano con l'opportunità e verranno passati
   alla generazione; la conversazione integrale invece non si conserva (v. cap. 15.4).
   I fatti restano quelli del profilo: gli appunti orientano, non aggiungono.

**A7. Generazione dei documenti (F4)**
1. L'app propone la **lingua** dell'output (rilevata dall'annuncio, modificabile).
2. Genera 🎯 CV-2 mirato e ✉️ lettera (con mitigazioni, se esistono), li mostra
   affiancati all'annuncio per il controllo.
3. La prosa (sommario, lettera) passa la **rifinitura anti-slop** (cap. 08); l'utente
   vede il prima/dopo e può intervenire a mano su ogni campo.
4. Alla conferma, l'app produce i file: **DOCX e/o PDF** del CV, testo della lettera
   (nel corpo email e/o come allegato).

**A8. La candidatura (F5 + F6)**
1. L'app compone l'email: destinatario (se noto dall'annuncio), oggetto, corpo
   (lettera o suo adattamento breve), allegati scelti (CV generato + eventuali
   attestati dalla cartella documenti).
2. L'utente rilegge l'anteprima e preme **«Prepara l'email»**: l'app scrive il file
   `.eml` nella cartella dell'opportunità e lo apre nel programma di posta predefinito,
   che lo mostra come bozza già compilata, allegati compresi. L'ultima parola — il
   pulsante «Invia» — è nel suo programma di posta.
3. Al ritorno nell'app, una domanda sola: «l'hai spedita?». Alla conferma l'opportunità
   passa allo stato **«inviata»** con data e ora, e il registro si aggiorna.

**A9. Fino a esaurimento**
1. Si torna ad A5 per la prossima opportunità in coda; quando la coda è vuota, si
   torna ad A4 (nuova raccolta) o si chiude.
2. Il registro (F6) mostra in ogni momento: inviate, in attesa, da fare, scartate.

## 12.2 Flusso B — Costruire il CV con il dialogo guidato (F1)

Per chi un CV non ce l'ha, o lo vuole rifare da zero. È l'anello 1 del prototipo,
portato su desktop:

1. La conversazione procede **un argomento per turno**: nome → contatti → patente →
   esperienze lavorative → esperienze informali → competenze → formazione.
2. Ogni risposta viene strutturata dall'AI e mostrata in una **scheda di conferma**
   («ho capito questo: giusto?») prima di entrare nel profilo.
3. Ciò che l'utente dice «nel turno sbagliato» non si perde: viene parcheggiato e
   riproposto al turno giusto (convenzione anti-perdita, campo `altrove`); ciò che non
   trova posto viene dichiarato «lasciato fuori», mai perso in silenzio.
4. Alla fine: riepilogo leggibile, conferma, e da lì si può già generare il
   📄 CV-1 base (senza alcun annuncio) o proseguire con il Flusso A dal punto A3.

## 12.3 Flusso C — Annuncio da link (F2)

Quando l'utente ha già in mano un link interessante (da un'email, da un amico, da un
altro dispositivo):

1. Incolla il link nel pannello ricerca; il browser integrato apre la pagina;
   l'utente fa l'eventuale login e arriva a vedere l'annuncio.
2. **«Cattura annuncio»** e da lì il flusso è identico ad A5 → A8.
3. In alternativa resta sempre il ripiego del prototipo: incollare direttamente il
   **testo** dell'annuncio.

## 12.4 Flusso D — Aggiornamento periodico del profilo (F1)

Il profilo invecchia: nuove esperienze, corsi, competenze. Ogni tanto (o quando
l'utente lo chiede) l'app propone una **sessione di aggiornamento**:

1. «Dall'ultima volta (data): è cambiato qualcosa? Nuovo lavoro, corso, attività?»
2. La conversazione usa gli stessi turni del dialogo guidato, ma **in modalità
   differenziale**: si aggiunge o si corregge, non si ricomincia.
3. Ogni modifica passa dalla solita scheda di conferma; il profilo aggiornato viene
   salvato e **versionato** (si conserva lo storico: cap. 11), così i CV già inviati
   restano riconducibili al profilo con cui furono generati.

## 12.5 Flusso E — Backup e ripristino (F7)

1. **Esporta**: l'utente sceglie file e contenuto — solo profilo (con storico),
   oppure profilo + registro + opportunità (cap. 11.4). L'app scrive un **JSON
   leggibile** con versione dello schema.
2. **Importa**: da un file di backup l'app ricostruisce profilo (e storico, se
   presente) mostrando **prima** cosa sta per sovrascrivere e chiedendo conferma.

## 12.6 Flusso F — L'app come server MCP (F8)

Per l'utente evoluto che lavora con Claude Desktop / Claude Code:

1. L'app (stesso exe, avviato in modalità server: cap. 09) espone come **tool MCP** le
   funzioni del motore: leggere il profilo, analizzare un annuncio incollato,
   confrontare, generare CV e lettera, esportare i file, leggere il registro.
2. Il client AI esterno può così orchestrare i flussi («confronta il mio profilo con
   questi 3 annunci e dimmi dove candidarmi prima») usando **la stessa logica e gli
   stessi prompt** dell'app, con i dati che restano sul PC dell'utente.
3. Le azioni che modificano il profilo **non sono esposte** nella prima versione del
   server MCP: restano nell'app, dove c'è la conferma visiva dell'utente (cap. 09.3).

## 12.7 Regole trasversali dei flussi

- **Mai un vicolo cieco**: da ogni pannello si torna indietro senza perdere ciò che è
  stato fatto; le operazioni lunghe (chiamate AI) mostrano attesa e sono annullabili.
- **Conferma prima di scrivere**: nessun dato entra nel profilo, nessun file viene
  scritto, nessuna email parte senza un passaggio esplicito di conferma.
- **Tutto riapribile**: ogni opportunità conserva annuncio, giudizi, documenti
  generati e stato; si può riprendere domani da dove si era rimasti.
- **Onestà visibile**: stelle, ⛔ eliminatori e avvisi di soglia non si nascondono mai;
  sono il valore del prodotto, non un fastidio da minimizzare.
