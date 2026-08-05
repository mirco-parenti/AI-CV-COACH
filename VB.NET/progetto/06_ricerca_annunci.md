# 06 — La ricerca degli annunci

*La funzione nuova più importante della fase desktop: trovare annunci adatti, anche sui
grandi portali, e portarli dentro la pipeline. La strada è quella decisa nello
Step 1.34 del diario: un browser vero dentro l'app, dove naviga l'utente.*

## 6.1 Perché un browser integrato (WebView2)

Il prototipo ha già dimostrato (prova sul campo, poi rimossa) che il prelievo diretto
delle pagine **non funziona**: sui portali moderni l'HTML scaricato è quasi vuoto,
perché la pagina si costruisce con JavaScript, spesso dietro login e protezioni
anti-robot. La soluzione non è aggirare le protezioni, è **cambiare punto di lettura**:

- dentro l'app c'è un **browser Edge/Chromium vero** (WebView2, componente di serie su
  Windows 11);
- **l'utente** naviga, cerca e, dove serve, **accede con il proprio account**, come
  farebbe in un browser qualsiasi;
- quando ha davanti un annuncio, il programma **legge la pagina che l'utente sta
  guardando** (il DOM già costruito, a JavaScript risolto) e la passa alla pipeline.

Questa impostazione è anche la bussola etica della funzione (cap. 01.4): niente
scraping massivo, niente automazione dell'accesso; si assiste la lettura di ciò che
l'utente sta legittimamente vedendo, per uso personale.

Rispetto alla lettera del mandato («trovare e scaricare» gli annunci), questa è una
**riduzione di perimetro dichiarata**: nessuna raccolta automatica in blocco — il
programma prepara le ricerche, l'utente sceglie, la cattura è un clic per annuncio.
È il compromesso che tiene insieme efficacia, rispetto delle regole dei portali e
qualità dei dati: si analizza solo ciò che un umano ha giudicato interessante.

## 6.2 Il pannello Ricerca (P3)

```
┌──────────────────────────────────────────────────────────────────┐
│ Ricerca salvata: [Indeed — perito elettronico Genova ▼] [Apri]   │
│ Oppure link diretto: [___________________________] [Vai]         │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│                  WebView2 (il portale, navigabile)               │
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│ [✔ Cattura annuncio]   Ultima cattura: «Tecnico manutenzione —   │
│                        Rossi SpA» → in coda (⭐ da calcolare)     │
└──────────────────────────────────────────────────────────────────┘
```

## 6.3 Le ricerche salvate

Le **preferenze** dell'utente (tipologie di ruolo, zona, contratto, parole chiave —
flusso A3 del cap. 12) diventano ricerche pronte per i portali supportati. Una ricerca
salvata è solo un **indirizzo parametrizzato**: il programma compone l'URL di ricerca
del portale con le parole chiave e la zona, e lo apre nel browser integrato.

- Portali previsti al primo rilascio: **Indeed, LinkedIn Jobs, InfoJobs** più una
  ricerca generica via motore di ricerca (per le pagine career aziendali).
- L'elenco è una **tabella dati** (nome portale + schema di URL), non codice: aggiungere
  un portale non richiede una nuova build. La tabella vive in `ricerche.json` nella
  cartella dati (cap. 11.1), insieme alle preferenze e alle ricerche salvate.
- Nessun risultato viene prelevato in automatico: il programma apre la pagina dei
  risultati, **l'utente** sceglie cosa aprire e cosa catturare.

## 6.4 La cattura

Alla pressione di **«Cattura annuncio»**:

1. il programma legge dalla pagina corrente: **titolo**, **URL** e **testo visibile**
   (l'equivalente di «seleziona tutto → copia», eseguito sul DOM);
2. il testo va al prompt `analisi_annuncio` (invariato dal prototipo): ne esce
   l'**Annuncio JSON** con requisiti, contesto e — novità — la **lingua** rilevata
   dell'annuncio (per il cap. 10);
3. se la pagina **non contiene un annuncio** (è una lista di risultati, una home, una
   pagina di login), lo schema esce vuoto e l'app risponde con garbo: «questa sembra
   una pagina di elenco: apri il singolo annuncio e ricattura»;
4. l'annuncio entra nella **coda delle opportunità** con fonte e link; da lì in poi la
   pipeline è quella di sempre (confronto → stelle → generazione).

Il ripiego del prototipo resta sempre disponibile: **incollare il testo** dell'annuncio
a mano (utile per annunci ricevuti via email o messaggio).

## 6.5 L'annuncio da link (flusso C)

Un link incollato nel campo dedicato apre la pagina nel browser integrato; l'utente
completa l'eventuale accesso e preme «Cattura annuncio». Stessa strada, stesso esito.
Non c'è alcun tentativo di scaricare il link «alla cieca»: la lezione dello Step 1.34
è definitiva.

## 6.6 Sessioni e riservatezza

- WebView2 conserva il proprio **profilo di navigazione** (cookie, sessioni) in una
  sottocartella della cartella dati dell'app: i login ai portali sopravvivono tra un
  avvio e l'altro, e un bottone nelle Impostazioni li cancella («Svuota dati di
  navigazione»).
- Le credenziali dei portali **non passano mai** dal programma: l'utente le digita nel
  browser, come farebbe in Edge. L'app non le vede e non le salva.
- Ciò che viene catturato (testo dell'annuncio) resta **sul PC dell'utente**, nella
  cartella dati; l'unico invio all'esterno è quello verso l'API AI per l'analisi.

## 6.7 Estensione futura già prevista dal disegno

Lo stesso meccanismo di cattura abilita la voce **2.1.3** del disegno funzionale
(profilo da LinkedIn): l'utente apre la **propria** pagina profilo nel browser
integrato, la cattura e la manda alla strutturazione (`importa_cv`), che è già
indipendente dalla fonte. Non è nel primo rilascio, ma non richiederà nessun
componente nuovo (il capitolo 14 la colloca).
