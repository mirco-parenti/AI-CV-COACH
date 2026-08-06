// Rigenera i prompt attesi di atteso/ facendoli costruire al PROTOTIPO: estrae
// promptConfronto() da HTML+JS/server.js e lo esegue sugli artefatti dei casi.
//
// Il prototipo è il giudice della non-regressione (cap. 14, T2): il testo che esce
// di qui è il termine di paragone di CollaudiParitaPrompt. Si rilancia quando
// cambiano gli artefatti dei casi o quando il prompt del confronto cambia nel pool
// (e allora, prima, si fa il rito del bump — cap. 04.5).
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

// Ritaglio della funzione: dalla dichiarazione alla chiusura del template literal che
// restituisce. Non si può cercare una graffa a inizio riga, perché il prompt ne
// contiene (lo schema JSON della risposta).
const inizio = sorgente.indexOf("function promptConfronto(");
if (inizio < 0) throw new Error("promptConfronto non trovata in server.js");
const chiusura = sorgente.indexOf("`;\n}", inizio);
if (chiusura < 0) throw new Error("fine di promptConfronto non trovata in server.js");

const promptConfronto = new Function(
  `${sorgente.slice(inizio, chiusura + 4)}; return promptConfronto;`)();

const casi = [
  ["compatibile", "annuncio_compatibile.json"],
  ["eliminatorio", "annuncio_eliminatorio.json"],
];

const profilo = JSON.parse(readFileSync(join(cartellaCasi, "profilo.json"), "utf8"));

for (const [caso, fileAnnuncio] of casi) {
  const annuncio = JSON.parse(readFileSync(join(cartellaCasi, fileAnnuncio), "utf8"));
  const prompt = promptConfronto(profilo, annuncio);
  const destinazione = join(cartellaCasi, "atteso", `prompt_confronto_${caso}.txt`);
  writeFileSync(destinazione, prompt, "utf8");
  console.log(`${caso}: ${prompt.length} caratteri scritti in ${destinazione}`);
}
