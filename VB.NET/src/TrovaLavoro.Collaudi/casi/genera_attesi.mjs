// Rigenera i prompt attesi di atteso/ facendoli costruire al PROTOTIPO: estrae
// promptConfronto() e promptMitigazione() da HTML+JS/server.js e li esegue sugli
// artefatti dei casi.
//
// Il prototipo è il giudice della non-regressione (cap. 14, T2): il testo che esce
// di qui è il termine di paragone di CollaudiParitaPrompt. Si rilancia quando
// cambiano gli artefatti dei casi o quando uno di quei due prompt cambia nel pool
// (e allora, prima, si fa il rito del bump — cap. 04.5).
//
// Sono questi due e non gli altri quattro perché sono i due su cui la parità vale
// ancora carattere per carattere: `analisi_annuncio` e i tre della generazione hanno
// preso una strada loro (cap. 04.7).
//
// Uso, dalla radice del repo:  node VB.NET/src/TrovaLavoro.Collaudi/casi/genera_attesi.mjs
import { readFileSync, writeFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

const cartellaCasi = dirname(fileURLToPath(import.meta.url));
const radice = join(cartellaCasi, "..", "..", "..", "..");

// Il sorgente del prototipo su disco ha fine riga CRLF: si normalizza a LF prima di
// ritagliare, così il prompt esce con le stesse fini riga dei file del pool (cap. 4.4).
const sorgente = readFileSync(join(radice, "HTML+JS", "server.js"), "utf8").replaceAll("\r\n", "\n");

// Ritaglio di una funzione: dalla dichiarazione alla chiusura del template literal che
// restituisce. Non si può cercare una graffa a inizio riga, perché il prompt ne
// contiene (lo schema JSON della risposta).
function estrai(nome) {
  const inizio = sorgente.indexOf(`function ${nome}(`);
  if (inizio < 0) throw new Error(`${nome} non trovata in server.js`);
  const chiusura = sorgente.indexOf("`;\n}", inizio);
  if (chiusura < 0) throw new Error(`fine di ${nome} non trovata in server.js`);
  return new Function(`${sorgente.slice(inizio, chiusura + 4)}; return ${nome};`)();
}

const promptConfronto = estrai("promptConfronto");
const promptMitigazione = estrai("promptMitigazione");

const casi = ["compatibile", "eliminatorio"];

const profilo = JSON.parse(readFileSync(join(cartellaCasi, "profilo.json"), "utf8"));
const leggi = (nome) => JSON.parse(readFileSync(join(cartellaCasi, nome), "utf8"));

function scrivi(idPrompt, caso, prompt) {
  const destinazione = join(cartellaCasi, "atteso", `prompt_${idPrompt}_${caso}.txt`);
  writeFileSync(destinazione, prompt, "utf8");
  console.log(`${idPrompt} / ${caso}: ${prompt.length} caratteri scritti in ${destinazione}`);
}

for (const caso of casi) {
  // Il confronto parte da profilo + annuncio; la mitigazione da profilo + i giudizi
  // che il confronto ha prodotto — e che nei casi sono un artefatto a sé, così questa
  // parità non dipende da una chiamata all'AI.
  scrivi("confronto", caso, promptConfronto(profilo, leggi(`annuncio_${caso}.json`)));
  scrivi("mitigazione", caso, promptMitigazione(profilo, leggi(`giudizi_${caso}.json`)));
}
