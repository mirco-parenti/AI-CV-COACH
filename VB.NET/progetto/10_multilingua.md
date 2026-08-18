# 10 — Multilingua: italiano e inglese

*Gli annunci interessanti non sono tutti in italiano. Il sistema genera CV e lettera
nella lingua giusta per ogni candidatura, con le stesse regole di onestà.*

## 10.1 Il principio: la lingua è una proprietà della candidatura

- L'**interfaccia** del programma resta in italiano (una lingua sola, fatta bene).
- Il **profilo** resta nella lingua dell'utente (per Mirco: italiano). Non esistono due
  profili paralleli da tenere allineati: la fonte di verità è una.
- Ogni **opportunità** ha la sua lingua di output: la propone il sistema (rilevata
  dall'annuncio), la conferma o cambia l'utente nel pannello Documenti (P6).

```
annuncio (IT o EN) ──► analisi ──► campo `lingua` nell'Annuncio JSON
                                        │
                          proposta lingua output (modificabile)
                                        │
                     🎯 CV-2, ✉️ lettera, email nella lingua scelta
```

Il **brainstorming** (P5) resta sempre in italiano: è una conversazione con l'utente,
non un documento per l'azienda.

Il 📄 **CV-1 base** è l'eccezione che completa la regola, e a **T7d** (2026-08-18) ha
avuto la sua lingua: un annuncio non ce l'ha — nasce dal solo profilo — quindi la sua
lingua non può venire da lì. La sceglie l'utente nella stessa tendina di P6, e vive
**dentro `cv_base.json`** (cap. 11.1), che è la sua unica casa. Detta meglio: la lingua è
una proprietà **del documento**; per CV-2, lettera ed email quel documento appartiene a
una candidatura e la lingua sta con lei, per il CV-1 base sta con lui. Fino a T7c la
tendina su di lui restava spenta, perché cambiarla voleva dire rigenerare *una
candidatura*: è caduta la premessa, non la regola.

*Realizzato a **T7c** (2026-08-18), e con una conseguenza che valeva la pena scrivere: in
italiano restano anche gli **appunti di mira** che ne escono, compresi quelli che finiranno
nei prompt di un CV inglese. Non è una svista — un appunto è un'**istruzione a chi scrive**,
non testo da ricopiare, e tradurlo vorrebbe dire tradurre due volte la stessa decisione. I
due prompt `.en` lo dichiarano, con le stesse parole che già usavano per il profilo e per i
ponti: sono in italiano e ci restano.*

## 10.2 Come si realizza

- Il prompt `analisi_annuncio` guadagna un campo in uscita: `lingua` (`"it"` / `"en"`;
  se l'annuncio è in un'altra lingua ancora, il campo la dichiara e il programma
  propone comunque l'inglese, avvisando).
- I prompt di **generazione** esistono in due varianti per lingua
  (`cv_mirato.it.md` / `cv_mirato.en.md`, ecc. — cap. 04.3): non un'istruzione
  «traduci», ma un prompt **scritto in inglese che genera in inglese**, con le stesse
  sezioni, le stesse regole anti-invenzione e la stessa distinzione campi-fatto /
  campi-prosa. È la strada che dà la qualità migliore e prompt più semplici da
  mettere a punto.
- Il **confronto** (anello 3) funziona già oggi tra lingue diverse: il modello di
  ragionamento giudica l'equivalenza semantica tra un profilo in italiano e requisiti
  in inglese. Il collaudo (cap. 14) include esplicitamente questo caso.

## 10.3 Tradurre i fatti senza tradire il profilo

Generare in inglese da un profilo italiano comporta scelte delicate. Regole cablate
nei prompt `.en`:

1. **Tradurre è un cambio di forma ammesso** (come la «normalizzazione leggera» del
   prototipo): `«gestione del magazzino» → "warehouse management"` va bene.
2. **Vietato l'upgrade nella traduzione**: un diploma di perito non diventa
   *engineering degree*; «me la cavo con l'inglese» non diventa *fluent*. Nel dubbio,
   la traduzione più modesta.
3. **I nomi propri restano originali**: aziende, enti, titoli di studio italiani si
   riportano tal quali (l'eventuale spiegazione è ammessa solo se descrive il titolo
   senza accrescerlo: *"Diploma di Perito Elettronico" (technical high school
   diploma in electronics)* — una descrizione, non una promozione).
4. **Recapiti e date**: formati adattati alla lingua (date inglesi, prefisso
   internazionale nel telefono se presente nel profilo).
5. **Il titolo di ruolo si traduce, il titolo di studio no** *(Pool 1.11, 2026-08-18)*.
   Un ruolo descrive una **funzione** — «Direttore Operativo» → *Operations Director* — e
   il punto 2 lo tiene a terra: mai *COO*, che sarebbe una promozione. Un titolo di studio
   è un **nome**, spesso con valore legale, e ricade nel punto 3. Il confine è il campo, e
   così il modello lo applica senza doverlo giudicare: `ruolo` si traduce,
   `formazione.titolo` resta. Un ruolo già scritto in inglese nel profilo non si tocca.

L'utente vede sempre il risultato in anteprima (P6) e resta l'ultimo giudice, come per
tutto il resto.

*Il punto 5 è arrivato tardi e per una ragione che vale la pena tenere a mente: la regola
c'era già — il punto 2 nomina espressamente il ruolo, e il punto 3 fra i nomi propri non lo
elenca — ma accanto al campo, nella sezione 1 dei due prompt, c'era scritto «**Copy** ruolo,
azienda and durata», senza il rimando alla sezione della traduzione che le competenze e la
formazione avevano. Ha vinto quella parola. È la terza volta (Pool 1.05, 1.07, 1.11): **fra
un'istruzione concreta accanto al campo e una regola generale in un'altra sezione, vince la
concreta**. Quando si rilegge un prompt, conviene cominciare da lì.*

## 10.4 Rifinitura e email

- La rifinitura anti-slop (cap. 08) riceve la lingua come parametro: i tic da
  correggere in inglese hanno la loro lista dedicata. *Costruita a T7b (2026-08-18), la
  lingua non è finita in un parametro dentro il prompt ma nella **variante del prompt**
  (`.it` / `.en`), come per i documenti dal Pool 1.06: i tic inglesi non sono i nostri
  tradotti — `delve`, `moreover`, l'em dash — e l'inglese si porta due guardie che
  l'italiano non ha, i **power verb** da CV (`spearheaded`, `drove`) e la **coppia
  obbligata** `Dear Sir or Madam` / `Yours faithfully`.*
- L'email di candidatura (cap. 07) segue la lingua dei documenti, oggetto compreso.
  *Costruito a T6 (2026-08-14), quel «parametrico» era risultato ancora più semplice: il
  prompt non riceveva nessun parametro di lingua — scriveva «nella stessa lingua della
  lettera», che è l'unica cosa che ha davanti. La lingua sta già nel contenuto, e
  chiederla a parte avrebbe creato un secondo posto da tenere d'accordo col primo. Da
  provare quando T7 porterà i documenti in inglese: l'email dovrà seguirli da sé.*
  *La prova è arrivata col collaudo reale di T7a (2026-08-15) e ha risposto **a metà**,
  che è il modo più istruttivo di rispondere: il **corpo** ha seguito la lettera ed è
  uscito in inglese, l'**oggetto** no — «Candidatura per External Warehouse Manager». La
  regola generale c'era ed è stata battuta dalla **forma concreta**, perché la sezione 1
  di quel prompt dettava l'oggetto parola per parola in italiano. È la stessa cosa vista
  al Pool 1.05, dove un esempio aveva battuto la regola che lo vietava. Da **Pool 1.07**
  l'email ha quindi le sue due varianti `.it`/`.en` come gli altri documenti, e riceve la
  lingua lungo la catena (`ICompositoreEmail.ComponiAsync` → P7, che la legge dalla
  candidatura come fa P6). Resta vero il timore che aveva ispirato la scelta di T6 — due
  posti divergono — ma il posto è **uno solo lo stesso**: `Motore/LinguaDocumenti`, da cui
  la lingua parte; quello che è cambiato è fin dove arriva.*
  *Un ultimo tratto, dal collaudo di T7b (2026-08-18): la lingua non basta che **arrivi**
  a P7, deve anche **restarci scritta**. Una bozza salvata è un testo, e un testo non
  dichiara in che lingua è: cambiando la tendina di P6 il giorno dopo, P7 riprendeva
  l'email vecchia senza potersi accorgere di niente. Da qui il campo `lingua` in
  `email.json` (cap. 07.1): la lingua di un documento va conservata **insieme al
  documento**, o al primo cambio di idea dell'utente le due cose si separano in silenzio.*
- I nomi dei file generati dichiarano la lingua quando è inglese
  (`CV_Mirco_Parenti_EN_...`, cap. 05.6).

## 10.5 Confini dichiarati

- Due lingue, non «tutte le lingue»: il pool contiene varianti `it` e `en`, punto. Se
  un giorno servisse il francese, il disegno lo consente (si aggiungono varianti
  `.fr.md` al pool), ma non è in perimetro.
- Nessuna traduzione automatica del profilo memorizzata: le rese inglesi vivono nei
  documenti generati (e nelle correzioni che l'utente fa lì), non come secondo
  profilo da mantenere.
