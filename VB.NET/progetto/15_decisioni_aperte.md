# 15 — Decisioni aperte

*Le scelte che spettano all'utente prima dell'implementazione. Ogni voce ha la
proposta di chi scrive, così la discussione parte da qualcosa. Quando una voce si
chiude, si sposta nel capitolo giusto e qui si spunta. Il piano (cap. 14) parte solo a
capitolo svuotato — o con le voci restanti dichiarate «rimandate» esplicitamente.*

## 15.1 Da decidere prima di T1 (fondamenta)

| # | Decisione | Proposta |
|---|---|---|
| 1 | **Versione .NET** | **.NET 8 LTS**, già collaudata dalla toolchain di casa; l'eventuale passaggio alla LTS successiva è un cambio di una riga, rivalutabile a T1. |
| 2 | **Formato di rilascio** | **Autonomo** (self-contained, ≈150–180 MB): copi un file e funziona, senza «prima installa il runtime». La variante leggera resta per lo sviluppo. |
| 3 | **Nome dell'eseguibile e dell'app** | `AiCvCoach.exe`, nome visuale «AI-CV-COACH». |
| 4 | **Logo del progetto** | serve un'immagine propria (quadrata, leggibile a 101×101 px). Da produrre (anche con Canva); finché non c'è, un segnaposto tipografico. |
| 5 | **Schema di versione** | `1.0.012` (maggiore.minore.build) in `Versione.vb`; pool separato (`Pool 1.03`). |

## 15.2 Da decidere entro la tappa interessata

| # | Decisione | Tappa | Proposta |
|---|---|---|---|
| 6 | **Modelli AI concreti** | T2 | oggi: Haiku 4.5 (estrazione) e Sonnet 4.6 (ragionamento); si riverifica il listino al momento, i nomi stanno in configurazione. |
| 7 | **Portali del primo rilascio** | T5 | Indeed, LinkedIn Jobs, InfoJobs + ricerca generica; schema URL da verificare sul campo in T5. |
| 8 | **`.msg` sì/no** | T6 | tenerlo **solo se** su almeno un PC di riferimento c'è Outlook classico; altrimenti si rimanda (l'`.eml` copre il bisogno). |
| 9 | **Account SMTP di riferimento** | T6 | quale casella userà Mirco per le candidature (Gmail con password per le app? altro provider?). |
| 10 | **Soglia e pesi del match** | T2 | **restano fissi** (scelte di prodotto validate nel prototipo: soglia 1,5 stelle, pesi 5/1, clamp −20/+10, tetto 20). Non configurabili dall'utente. |

## 15.3 Dichiarate rimandate (non bloccano T0)

- **Trasporto HTTP locale per MCP** — stdio basta per i client di oggi.
- **Invio email e scrittura profilo via MCP** — richiedono un meccanismo di conferma;
  seconda versione.
- **Firma del codice** (certificato) — quando l'app circolerà oltre il portfolio.
- **Auto-update** — per un'app personale è complessità senza guadagno.
- **OCR locale per PDF scannerizzati** — ripiego incolla-testo; l'eventuale
  elaboratore PDF esterno di casa (solo exe binario) si valuta **solo se** i PDF
  scannerizzati diventano un caso frequente.
- **Profilo da LinkedIn (voce 2.1.3)** — il meccanismo c'è (cap. 06.7); si colloca
  dopo la 1.0.
- **Terze lingue (fr, de…)** — il pool le ammette per costruzione; fuori perimetro.

## 15.4 Domande aperte all'utente (senza proposta secca)

1. **Follow-up delle candidature**: basta il promemoria passivo nel registro
   (cap. 07.3) o serve una generazione assistita dell'email di sollecito?
2. **Più profili nella stessa installazione** (es. un familiare che usa lo stesso PC):
   oggi il disegno è mono-profilo; il multi-profilo cambierebbe la cartella dati.
   Serve?
3. **Il brainstorming va conservato per intero** (tutta la conversazione) o bastano
   gli appunti di mira confermati? Oggi il disegno salva solo gli appunti.
4. **Pubblicazione dei valori concreti del design** (la palette e le dimensioni del
   cap. 03): sono l'aspetto visibile del family feeling, non meccanismi interni — ma
   la conferma che vada bene pubblicarli in questo repo spetta a te.

## 15.5 Sorte del backlog storico (`idee_future.md`)

| Voce del backlog | Sorte in questo progetto |
|---|---|
| Fonte-link / WebView2 | **dentro** (cap. 06) |
| Multi-annuncio | **dentro** (coda opportunità, cap. 06/07) |
| Editing campo-per-campo del profilo | **dentro** (cap. 12, A2) |
| `estraiFrammento` robusto lato client | **assorbita**: nel desktop c'è un solo estrattore, `EstrattoreJson` (cap. 02) |
| Limite dimensione PDF | **dentro** (cap. 05.1, messaggio chiaro) |
| Profilo da LinkedIn (2.1.3) | rimandata dopo la 1.0 (15.3) |
| PDF scannerizzati / OCR | rimandata (15.3) |
| Estensione profilo a specchio di `altri_requisiti`; domicilio confrontabile | **fuori**: resta nel backlog |
| `pending_questions`; collocazione manuale degli esclusi | **fuori**: resta nel backlog |
| Riordino dinamico / omissione mirata nel CV-2 | **fuori**: resta nel backlog (l'anti-invenzione «per sottrazione» merita una riflessione a sé) |
| Taxonomy mapping (ESCO/O*NET) | **fuori**: resta nel backlog |
| Decomposizione del prompt annuncio in sotto-prompt | **fuori** per ora: il pool la renderà naturale quando servirà |
