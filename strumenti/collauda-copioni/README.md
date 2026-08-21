# collauda-copioni

*Il banco dei **copioni JavaScript** di `LettorePagina`: li estrae dal sorgente VB, li
compila e li fa girare su pagine finte, confrontando l'esito con quello atteso.*

```bash
node strumenti/collauda-copioni/collauda-copioni.mjs
```

Esce `0` se tutti i casi passano, `1` se qualcuno cade — e in quel caso dice **quale** e
**che cosa** ha visto invece di quel che si aspettava.

## Perché esiste

Il prodotto ha due pezzi di codice che il banco VB non può raggiungere, perché non girano
nell'applicazione ma **dentro la WebView**: il copione che legge la pagina (`Copione`) e
quello che la scorre di un passo (`UnPasso`). Fino a T9d nessuno li aveva mai provati se
non aprendo un sito vero e guardando cosa ne usciva — ed è così che è nata la voce
«le parole si incollano fra un blocco e l'altro»: notata su un sito, per caso, quando il
modello aveva già capito lo stesso.

Un JavaScript che si compone concatenando stringhe VB non ha nemmeno un compilatore che
lo guardi: un errore di sintassi si scopre a runtime, in silenzio, con la cattura che
torna vuota e il sospetto che cade sulla pagina. Qui invece si scopre subito.

## Cosa guarda

**La lettura della pagina** — che due blocchi separati dal layout non finiscano attaccati
(il difetto da cui è nato lo strumento); che il testo appeso direttamente a un contenitore
non sparisca; che ciò che il foglio di stile spegne resti fuori; che una pagina senza
corpo non faccia cadere il copione; che titolo e indirizzo arrivino insieme al testo; che
un testo più lungo del massimo si tronchi **e lo dichiari**.

**Lo scorrimento** — che un passo muova davvero la pagina e non si dichiari in fondo
prima di esserci; e che il fondo, quando arriva, venga riconosciuto.

## I limiti dichiarati

- **Il DOM finto non è un browser.** Riproduce i nodi, il `display` calcolato e la
  visibilità, non il layout: non risponderà mai a domande su come una pagina *si dispone*.
- **Il suo `innerText` finge il difetto** — concatena i figli senza separarli, che è
  quello che si è visto sul sito vero — e non replica tutto ciò che il browser fa bene:
  non esclude `<script>` e `<style>`, non applica `text-transform`. Il copione del
  prodotto quei due tag li salta per conto suo, e qui si vede la differenza: falsificando
  la lettura, cade anche il caso della roba nascosta.
- **Il vero collaudo resta la pagina vera.** Questo banco dice che l'algoritmo fa quel che
  crede di fare; non dice che il sito di domani sia fatto come i finti di oggi.

## La trappola già pagata

L'estrazione dipende dalla **forma** con cui il VB compone le stringhe. Se il sorgente
cambia — la funzione si rinomina, il copione si scrive in un altro modo — lo strumento
**si ferma e lo dice**: *«Nel sorgente non c'è più «Copione»: il banco guarda un codice che
non esiste»*. È voluto: un banco che tace quando ha smesso di guardare è peggio di non
averlo, perché continua a dire «tutto bene».

Vale anche per il limite di caratteri: non è un numero scritto qui, è
`LettorePagina.MassimoCaratteri` letto dal sorgente. Se sparisse, lo strumento si ferma
invece di collaudare un taglio che nessuno fa.

*(Nato a T9d, 2026-08-22, insieme alla cura delle parole incollate.)*
