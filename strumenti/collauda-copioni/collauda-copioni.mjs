// Banco dei copioni JavaScript di LettorePagina: li estrae dal sorgente VB, li compila
// e li fa girare su pagine finte, confrontando l'esito con quello atteso.
//
// Esiste perché quel JavaScript è l'unico codice del prodotto che il banco VB non può
// raggiungere: gira dentro la WebView, e da lì fino a T9d nessuno l'ha mai provato se
// non aprendo un sito vero.
//
//   node strumenti/collauda-copioni/collauda-copioni.mjs

import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join } from "node:path";

const QUI = dirname(fileURLToPath(import.meta.url));
const SORGENTE = join(QUI, "..", "..", "VB.NET", "src", "TrovaLavoro", "Web", "LettorePagina.vb");

// ==================================================================
// L'estrazione dal sorgente VB
// ==================================================================

/**
 * Il JavaScript di una delle funzioni che lo compongono a pezzi di stringa.
 * Se non lo trova **si ferma**: uno strumento che tace quando il sorgente cambia forma
 * è peggio di non averlo, perché continua a dire «tutto bene» senza guardare niente.
 */
function estrai(nomeFunzione) {
  const vb = readFileSync(SORGENTE, "utf8");

  const dopo = vb.split(`Private Shared Function ${nomeFunzione}(`)[1];
  if (dopo === undefined) {
    throw new Error(`Nel sorgente non c'è più «${nomeFunzione}»: il banco guarda un codice che non esiste.`);
  }

  const apertura = 'Return "(function () {" &';
  const corpo = dopo.split(apertura)[1];
  if (corpo === undefined) {
    throw new Error(`«${nomeFunzione}» non comincia più con «${apertura}»: l'estrazione va rifatta.`);
  }

  const pezzi = ["(function () {"];
  for (const riga of corpo.split("\n")) {
    const pulita = riga.trim();
    if (pulita === '"})()"') break;
    const dentro = pulita.match(/^\$?"(.*)" &$/);
    if (dentro) pezzi.push(dentro[1].replaceAll('""', '"'));
  }

  if (pezzi.length === 1) throw new Error(`Da «${nomeFunzione}» non è uscita nessuna riga di JavaScript.`);

  // Il limite di caratteri arriva interpolato da VB: qui si rimette quello **vero**,
  // letto dalla costante del prodotto, o si collauderebbe un taglio che nessuno fa.
  return (pezzi.join("") + "})()").replaceAll("{massimo}", String(massimoCaratteri(vb)));
}

/** Il valore di `MassimoCaratteri`, dal sorgente: è il taglio che il prodotto applica davvero. */
function massimoCaratteri(vb) {
  const trovato = vb.match(/MassimoCaratteri\s+As\s+Integer\s*=\s*(\d+)/);
  if (!trovato) throw new Error("Nel sorgente non si trova più «MassimoCaratteri».");
  return Number(trovato[1]);
}

/** Compila ed esegue il copione su una pagina finta, come fa ExecuteScriptAsync. */
function esegui(js, pagina) {
  const compilato = new Function("document", "window", "location", `return ${js};`);
  return compilato(pagina.document, pagina.window, pagina.location);
}

// ==================================================================
// Le pagine finte
// ==================================================================

const testo = (valore) => ({ nodeType: 3, nodeValue: valore });

/**
 * Un elemento. `innerText` finge **il difetto vero**: concatena i figli senza separarli,
 * che è quello che il browser faceva sul sito da cui è nata la voce di `in_sospeso.md`
 * («Pubblica AmministrazioneDue suite specializzate»). Salta però ciò che è nascosto,
 * perché quello il browser lo fa bene ed è la ragione per cui il prodotto usa innerText
 * invece di textContent.
 */
function elemento(tag, display, figli = [], scritto = undefined, visibilita = "visible") {
  return {
    nodeType: 1, tagName: tag, display, visibilita, childNodes: figli,
    get innerText() {
      if (scritto !== undefined) return scritto;
      return this.childNodes
        .filter((n) => n.nodeType !== 1 || (n.display !== "none" && n.visibilita !== "hidden"))
        .map((n) => (n.nodeType === 3 ? n.nodeValue : n.innerText))
        .join("");
    },
  };
}

function pagina(corpo, { titolo = "Prova", indirizzo = "https://esempio.it/annuncio" } = {}) {
  return {
    document: { body: corpo, title: titolo, querySelectorAll: () => [] },
    window: { getComputedStyle: (e) => ({ display: e.display, visibility: e.visibilita }) },
    location: { href: indirizzo },
  };
}

// ==================================================================
// I casi
// ==================================================================

const casi = [];
const caso = (nome, prova) => casi.push({ nome, prova });

const lettura = () => estrai("Copione");

caso("due blocchi che il layout separa non finiscono attaccati", () => {
  // Il caso osservato: due <span> dentro un contenitore flex. Il browser li «blockifica»,
  // quindi il loro display calcolato è block.
  const corpo = elemento("BODY", "block", [
    elemento("DIV", "flex", [
      elemento("SPAN", "block", [testo("Pubblica Amministrazione")]),
      elemento("SPAN", "block", [testo("Due suite specializzate")]),
    ]),
  ]);

  const letto = JSON.parse(esegui(lettura(), pagina(corpo)));

  if (letto.testo.includes("AmministrazioneDue")) {
    return "le parole sono ancora incollate: " + JSON.stringify(letto.testo);
  }
  if (!letto.testo.includes("Pubblica Amministrazione\nDue suite specializzate")) {
    return "manca l'a capo fra i due blocchi: " + JSON.stringify(letto.testo);
  }
});

caso("il testo appeso direttamente a un contenitore non si perde", () => {
  const corpo = elemento("BODY", "block", [
    elemento("DIV", "block", [testo("Testo sciolto"), elemento("P", "block", [testo("Un paragrafo.")])]),
  ]);

  const letto = JSON.parse(esegui(lettura(), pagina(corpo)));

  if (!letto.testo.includes("Testo sciolto")) return "sparito il testo sciolto: " + JSON.stringify(letto.testo);
  if (!letto.testo.includes("Un paragrafo.")) return "sparito il paragrafo: " + JSON.stringify(letto.testo);
});

caso("quello che il foglio di stile spegne resta fuori", () => {
  const corpo = elemento("BODY", "block", [
    elemento("P", "block", [testo("Questo si legge.")]),
    elemento("DIV", "none", [testo("Un menù mai mostrato.")]),
    elemento("DIV", "block", [testo("Un banner spento.")], undefined, "hidden"),
    elemento("SCRIPT", "block", [testo("var x = 1;")]),
    elemento("STYLE", "block", [testo(".a { color: red }")]),
  ]);

  const letto = JSON.parse(esegui(lettura(), pagina(corpo)));

  for (const fuori of ["Un menù mai mostrato.", "Un banner spento.", "var x = 1;", "color: red"]) {
    if (letto.testo.includes(fuori)) return `è entrato quel che doveva restare fuori: ${fuori}`;
  }
  if (!letto.testo.includes("Questo si legge.")) return "ed è uscito anche quel che si vedeva";
});

caso("una pagina senza corpo non fa cadere il copione", () => {
  const vuota = pagina(null);
  vuota.document.body = null;

  const letto = JSON.parse(esegui(lettura(), vuota));

  if (letto.testo !== "") return "da una pagina vuota è uscito del testo: " + JSON.stringify(letto.testo);
  if (letto.troncato !== false) return "e per giunta si dichiara troncata";
});

caso("titolo e indirizzo arrivano insieme al testo", () => {
  const corpo = elemento("BODY", "block", [elemento("P", "block", [testo("Un annuncio.")])]);

  const letto = JSON.parse(esegui(lettura(), pagina(corpo, { titolo: "Magazziniere — Rossi" })));

  if (letto.titolo !== "Magazziniere — Rossi") return "titolo sbagliato: " + letto.titolo;
  if (letto.indirizzo !== "https://esempio.it/annuncio") return "indirizzo sbagliato: " + letto.indirizzo;
});

caso("un testo più lungo del massimo si tronca e lo dichiara", () => {
  const lungo = "parola ".repeat(20000);
  const corpo = elemento("BODY", "block", [elemento("P", "block", [testo(lungo)])]);

  const letto = JSON.parse(esegui(lettura(), pagina(corpo)));

  if (!letto.troncato) return "un testo lunghissimo non si è dichiarato troncato";
  if (letto.testo.length >= lungo.length) return "e non è stato tagliato";
});

const passo = () => estrai("UnPasso");

/** Un documento che si può scorrere, come lo vede il copione dello scorrimento. */
function documentoCheScorre({ altezza = 3000, finestra = 800, partenza = 0 } = {}) {
  const scorrevole = { scrollHeight: altezza, clientHeight: finestra, clientWidth: 1200, scrollTop: partenza };
  return {
    document: { scrollingElement: scorrevole, documentElement: scorrevole, querySelectorAll: () => [] },
    window: {},
    location: { href: "https://esempio.it" },
    scorrevole,
  };
}

caso("un passo in giù muove la pagina e non dice di essere in fondo", () => {
  const finta = documentoCheScorre();
  const esito = esegui(passo(), finta);

  if (!esito.mosso) return "la pagina non si è mossa";
  if (esito.fondo) return "e si è dichiarata in fondo al primo passo";
  if (finta.scorrevole.scrollTop <= 0) return "lo scorrimento è rimasto in cima";
});

caso("arrivati in fondo, il passo lo dichiara", () => {
  const finta = documentoCheScorre({ altezza: 1000, finestra: 800, partenza: 900 });
  const esito = esegui(passo(), finta);

  if (!esito.fondo) return "il fondo della pagina non è stato riconosciuto";
});

// ==================================================================
// Il giro
// ==================================================================

let caduti = 0;

for (const { nome, prova } of casi) {
  let guaio;
  try {
    guaio = prova();
  } catch (errore) {
    guaio = `è saltato: ${errore.message}`;
  }

  if (guaio) {
    caduti++;
    console.log(`  ✗ ${nome}\n      ${guaio}`);
  } else {
    console.log(`  ✓ ${nome}`);
  }
}

console.log(
  caduti === 0
    ? `\nTutti e ${casi.length} i casi sono passati.`
    : `\n${caduti} casi su ${casi.length} non sono passati.`
);

process.exit(caduti === 0 ? 0 : 1);
