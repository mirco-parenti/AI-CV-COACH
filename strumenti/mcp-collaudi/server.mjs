#!/usr/bin/env node
// Server MCP di collaudo per AI-CV-COACH.
//
// A che serve: dare all'assistente una manciata di comandi *codificati* per compilare,
// far girare i collaudi, avviare l'applicazione vera e guardarla in faccia con una
// schermata. Non è un'esecuzione di comandi arbitrari: ogni attrezzo è scritto qui, e
// se ne serve uno nuovo lo si aggiunge — è il patto di questo strumento.
//
// Come parla: MCP su HTTP (JSON-RPC 2.0) sulla porta 3300, solo da 127.0.0.1.
// Nessuna dipendenza npm, come tutto il resto del repo: Node ≥ 20 e basta.
//
// Avvio:  node strumenti/mcp-collaudi/server.mjs
// Prova:  curl -s -X POST http://127.0.0.1:3300/mcp -H 'content-type: application/json' \
//           -d '{"jsonrpc":"2.0","id":1,"method":"tools/list"}'

import { createServer } from "node:http";
import { spawn } from "node:child_process";
import { readFile, writeFile, unlink, mkdir } from "node:fs/promises";
import { existsSync } from "node:fs";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { tmpdir } from "node:os";

const QUI = dirname(fileURLToPath(import.meta.url));
const REPO = resolve(QUI, "..", "..");
const SRC = join(REPO, "VB.NET", "src");

const PORTA = 3300;
const VERSIONE_PROTOCOLLO = "2025-06-18";

const DOTNET = "/mnt/c/Program Files/dotnet/dotnet.exe";
const ESEGUIBILE = join(SRC, "TrovaLavoro", "bin", "Release", "net10.0-windows", "TrovaLavoro.exe");

// I progetti si nominano **relativi** a VB.NET/src, e i comandi si eseguono da lì:
// `dotnet.exe` è un eseguibile Windows, e un percorso alla maniera di WSL
// (`/mnt/c/…`) MSBuild lo scambia per un'opzione — errore MSB1001, visto sul serio.
const PROGETTO = "TrovaLavoro/TrovaLavoro.vbproj";
const COLLAUDI = "TrovaLavoro.Collaudi/TrovaLavoro.Collaudi.vbproj";
const CHIAVE_DA = join(REPO, "HTML+JS", ".env");

// ============================================================================
// Mestieri di base: eseguire un comando, e non aspettarlo per sempre
// ============================================================================

/** Esegue un comando in bash e restituisce uscita, errori e codice. */
function esegui(comando, { timeoutMs = 600_000, ambiente = {} } = {}) {
  return new Promise((risolvi) => {
    const figlio = spawn("bash", ["-lc", comando], {
      cwd: REPO,
      env: { ...process.env, ...ambiente },
    });

    let uscita = "";
    let errori = "";
    let scaduto = false;

    const allarme = setTimeout(() => {
      scaduto = true;
      figlio.kill("SIGKILL");
    }, timeoutMs);

    figlio.stdout.on("data", (pezzo) => (uscita += pezzo));
    figlio.stderr.on("data", (pezzo) => (errori += pezzo));

    figlio.on("close", (codice) => {
      clearTimeout(allarme);
      risolvi({ codice, uscita, errori, scaduto });
    });
  });
}

/** Le ultime righe di un testo: i log lunghi non servono interi. */
function coda(testo, righe = 40) {
  const tutte = testo.trimEnd().split("\n");
  return tutte.slice(-righe).join("\n");
}

/**
 * La riga della chiave API, presa dal .env del prototipo senza mai stamparla.
 * Serve all'applicazione avviata da qui: senza, i servizi che passano dall'AI
 * restano spenti e non si potrebbe provare nulla.
 */
async function chiaveApi() {
  if (!existsSync(CHIAVE_DA)) return null;

  const testo = await readFile(CHIAVE_DA, "utf8");
  const riga = testo.split("\n").find((r) => r.trim().startsWith("ANTHROPIC_API_KEY="));
  if (!riga) return null;

  return riga.slice(riga.indexOf("=") + 1).trim().replace(/^["']|["']$/g, "");
}

// ============================================================================
// Gli attrezzi
// ============================================================================

const ATTREZZI = [
  {
    name: "compila",
    description:
      "Compila l'applicazione TrovaLavoro in Release e restituisce gli errori del compilatore. " +
      "Se l'applicazione è aperta la chiude prima: un exe in esecuzione tiene il file bloccato e la " +
      "compilazione fallirebbe per un motivo che non ha niente a che vedere col codice.",
    inputSchema: {
      type: "object",
      properties: {
        tieni_aperta: {
          type: "boolean",
          description: "Vero per non chiudere l'applicazione (e accettare che la copia dell'exe fallisca).",
        },
      },
      additionalProperties: false,
    },
    async esegui({ tieni_aperta = false }) {
      let premessa = "";

      if (!tieni_aperta) {
        const chiusa = await esegui(`taskkill.exe /IM TrovaLavoro.exe /F 2>&1 | grep -c RIUSCITA || true`);
        if (chiusa.uscita.trim() !== "0") premessa = "Ho chiuso l'applicazione, che teneva l'exe bloccato.\n";
        await attendi(700);
      }

      const esito = await esegui(
        `cd "${SRC}" && "${DOTNET}" build "${PROGETTO}" -c Release --nologo`,
        { timeoutMs: 300_000 }
      );

      // Del log serve poco: le righe di errore, e quante ce n'erano.
      const errori = esito.uscita
        .split("\n")
        .filter((r) => / error [A-Z]+\d+/.test(r))
        .slice(0, 12);

      return testo(
        esito.codice === 0
          ? `${premessa}Compilazione riuscita.`
          : `${premessa}Compilazione FALLITA (codice ${esito.codice}).\n${errori.join("\n") || coda(esito.uscita, 10)}`,
        esito.codice !== 0
      );
    },
  },

  {
    name: "collaudi",
    description:
      "Fa girare il banco di collaudo (dotnet test, Release). Con «filtro» ne esegue solo una parte, " +
      "come farebbe --filter FullyQualifiedName~<filtro>. Restituisce il conteggio e i collaudi rossi.",
    inputSchema: {
      type: "object",
      properties: {
        filtro: {
          type: "string",
          description: "Parte del nome del collaudo o della classe, es. «PannelloOpportunita».",
        },
      },
      additionalProperties: false,
    },
    async esegui({ filtro }) {
      // Anche il banco ricompila l'applicazione: se è aperta, tiene bloccato l'exe.
      await esegui(`taskkill.exe /IM TrovaLavoro.exe /F > /dev/null 2>&1 || true`);
      await attendi(500);

      const parte = filtro ? ` --filter "FullyQualifiedName~${filtro}"` : "";
      const esito = await esegui(
        `cd "${SRC}" && "${DOTNET}" test "${COLLAUDI}" -c Release --nologo${parte}`
      );

      const righe = esito.uscita
        .split("\n")
        .filter((r) => /error BC|Non superato |Non superati|Superato!|Assert\./.test(r))
        .slice(0, 30);

      return testo(`${righe.join("\n") || coda(esito.uscita, 15)}`, esito.codice !== 0);
    },
  },

  {
    name: "avvia_app",
    description:
      "Avvia TrovaLavoro.exe con la chiave API caricata dal .env del prototipo, e restituisce lo stato " +
      "del processo. Senza «dati» lavora sulla cartella vera (%APPDATA%\\TrovaLavoro); con «dati» " +
      "l'applicazione parte su una radice usa-e-getta, che è il modo di provare le funzioni che " +
      "cancellano senza mettere in gioco i dati di Mirco.",
    inputSchema: {
      type: "object",
      properties: {
        dati: {
          type: "string",
          description:
            "La cartella dati su cui far vivere l'applicazione (opzione --dati). Si può dare alla " +
            "maniera di WSL (/mnt/c/…): viene tradotta. Se non esiste, la crea l'applicazione.",
        },
      },
      additionalProperties: false,
    },
    async esegui({ dati }) {
      if (!existsSync(ESEGUIBILE)) {
        return testo("L'eseguibile non c'è: compila prima (attrezzo «compila»).", true);
      }

      const chiave = await chiaveApi();
      if (!chiave) return testo(`Chiave API non trovata in ${CHIAVE_DA}.`, true);

      // L'exe è un programma Windows: una radice alla maniera di WSL non la
      // capirebbe, e finirebbe per creare una cartella dal nome assurdo accanto a sé.
      let radice = null;
      if (dati) {
        radice = dati.startsWith("/") ? (await esegui(`wslpath -w "${dati}"`)).uscita.trim() : dati;
        if (!radice) return testo(`Non ho saputo tradurre il percorso «${dati}».`, true);
      }

      // WSLENV è ciò che fa arrivare una variabile d'ambiente da WSL a un exe Windows.
      const ambiente = {
        ANTHROPIC_API_KEY: chiave,
        WSLENV: "ANTHROPIC_API_KEY",
      };

      // Staccato dal server: l'applicazione resta viva anche fra una chiamata e l'altra.
      const figlio = spawn(ESEGUIBILE, radice ? ["--dati", radice] : [], {
        cwd: dirname(ESEGUIBILE),
        env: { ...process.env, ...ambiente },
        detached: true,
        stdio: "ignore",
      });
      figlio.unref();

      await attendi(1500);
      const viva = await esegui(`tasklist.exe /FI "IMAGENAME eq TrovaLavoro.exe" 2>/dev/null`);

      return testo(
        `Avviata (pid del lanciatore ${figlio.pid}).\n${coda(viva.uscita, 5)}` +
          (chiave ? "\nChiave API: caricata." : "") +
          (radice ? `\nCartella dati: ${radice} (la finestra lo dice nel titolo).` : "")
      );
    },
  },

  {
    name: "stato_app",
    description: "Dice se TrovaLavoro.exe è in esecuzione, e con quanta memoria.",
    inputSchema: { type: "object", properties: {}, additionalProperties: false },
    async esegui() {
      const esito = await esegui(`tasklist.exe /FI "IMAGENAME eq TrovaLavoro.exe" 2>/dev/null`);
      return testo(coda(esito.uscita, 6));
    },
  },

  {
    name: "chiudi_app",
    description: "Chiude TrovaLavoro.exe, se è aperta.",
    inputSchema: { type: "object", properties: {}, additionalProperties: false },
    async esegui() {
      const esito = await esegui(`taskkill.exe /IM TrovaLavoro.exe /F 2>&1 || true`);
      return testo(coda(esito.uscita, 4));
    },
  },

  {
    name: "schermata",
    description:
      "Cattura una schermata e la restituisce come immagine. Senza argomenti riprende la finestra di " +
      "TrovaLavoro; con «schermo_intero» tutto il desktop. È il modo di guardare l'applicazione in faccia.",
    inputSchema: {
      type: "object",
      properties: {
        schermo_intero: {
          type: "boolean",
          description: "Vero per riprendere tutto il desktop invece della sola finestra dell'applicazione.",
        },
        porta_in_primo_piano: {
          type: "boolean",
          description: "Vero per portare la finestra davanti prima di riprenderla (predefinito: vero).",
        },
      },
      additionalProperties: false,
    },
    async esegui({ schermo_intero = false, porta_in_primo_piano = true }) {
      return await catturaSchermata({ schermo_intero, porta_in_primo_piano });
    },
  },

  {
    name: "controlli",
    description:
      "Elenca i controlli visibili dell'applicazione — bottoni, caselle, schede — dicendo per ciascuno " +
      "se è acceso o SPENTO. È la risposta alla domanda «perché questo bottone non fa niente?».",
    inputSchema: { type: "object", properties: {}, additionalProperties: false },
    async esegui() {
      const esito = await interfaccia({ azione: "elenca" });
      return testo(esito || "(nessun controllo: l'applicazione è aperta?)");
    },
  },

  {
    name: "clic",
    description:
      "Preme un bottone o sceglie una scheda dell'applicazione, cercandolo per etichetta. Se il " +
      "controllo è spento lo dice invece di fingere di averlo premuto.",
    inputSchema: {
      type: "object",
      properties: {
        nome: { type: "string", description: "L'etichetta del controllo, anche parziale: «Analizza»." },
      },
      required: ["nome"],
      additionalProperties: false,
    },
    async esegui({ nome }) {
      return testo(await interfaccia({ azione: "clic", nome }));
    },
  },

  {
    name: "scrivi",
    description:
      "Scrive un testo in una casella dell'applicazione (per esempio l'annuncio da incollare in P4). " +
      "Se una casella non si lascia scrivere, il testo ci arriva incollato come lo incollerebbe una " +
      "persona — appunti e Ctrl+V — così scattano gli eventi che accendono i bottoni.",
    inputSchema: {
      type: "object",
      properties: {
        nome: { type: "string", description: "L'etichetta della casella, anche parziale." },
        testo: { type: "string", description: "Il testo da scriverci dentro." },
      },
      required: ["nome", "testo"],
      additionalProperties: false,
    },
    async esegui({ nome, testo: contenuto }) {
      return testo(await interfaccia({ azione: "scrivi", nome, testo: contenuto }));
    },
  },

  {
    name: "scegli_voce",
    description:
      "Sceglie una voce in un menù a tendina dell'applicazione — per esempio il portale in «Cerca su» " +
      "di P3. Lo apre e ci clicca dentro come farebbe una persona, così scattano gli eventi veri, e " +
      "poi verifica che il menù mostri davvero la voce chiesta. Senza «voce» non sceglie niente: " +
      "elenca le voci che ci sono, con una freccia su quella di adesso.",
    inputSchema: {
      type: "object",
      properties: {
        nome: {
          type: "string",
          description: "L'etichetta del menù, anche parziale: «Cerca su», «Ricerche salvate».",
        },
        voce: {
          type: "string",
          description: "La voce da scegliere, anche parziale. Senza, l'attrezzo si limita a elencarle.",
        },
      },
      required: ["nome"],
      additionalProperties: false,
    },
    async esegui({ nome, voce }) {
      return testo(await interfaccia({ azione: "scegli", nome, voce }));
    },
  },

  {
    name: "scegli_riga",
    description:
      "Sceglie una riga in una lista dell'applicazione — la coda delle candidature di P1 — cercandola " +
      "per un pezzo di quello che c'è scritto nelle sue celle, non solo nella prima. Ci clicca sopra " +
      "come farebbe una persona, e poi verifica che risulti davvero scelta. Senza «testo» non sceglie " +
      "niente: elenca le righe che ci sono, con una freccia su quella di adesso. Con «doppio» fa il " +
      "doppio clic, che nella coda apre la candidatura.",
    inputSchema: {
      type: "object",
      properties: {
        testo: {
          type: "string",
          description: "Un pezzo del contenuto della riga: «Rossi S.p.A.», «magazziniere».",
        },
        doppio: {
          type: "boolean",
          description: "Vero per fare il doppio clic invece del clic solo.",
        },
      },
      additionalProperties: false,
    },
    async esegui({ testo: contenuto, doppio = false }) {
      return testo(await interfaccia({ azione: "riga", testo: contenuto, doppio }));
    },
  },

  {
    name: "rispondi_finestra",
    description:
      "Risponde a una finestra di messaggio dell'applicazione — una conferma, un avviso — premendo " +
      "il bottone che si dice, per nome («Sì», «No», «OK», «Annulla»). Senza «bottone» non risponde: " +
      "legge cosa chiede e quali scelte dà, così si sa cosa si sta per confermare. Per la finestra di " +
      "scelta file c'è «scegli_file»: quella ha la casella del nome, questa no.",
    inputSchema: {
      type: "object",
      properties: {
        bottone: {
          type: "string",
          description: "L'etichetta del bottone da premere, anche parziale. Senza, li elenca.",
        },
      },
      additionalProperties: false,
    },
    async esegui({ bottone }) {
      return testo(await interfaccia({ azione: "rispondi", bottone }));
    },
  },

  {
    name: "scegli_file",
    description:
      "Risponde alla finestra di scelta file che l'applicazione ha aperto — «Importa da un CV…», un " +
      "salvataggio — indicando il file da prendere, oppure annullando. Finché quella finestra è " +
      "aperta l'applicazione non ascolta nient'altro, e nemmeno «controlli» dice la verità.",
    inputSchema: {
      type: "object",
      properties: {
        percorso: {
          type: "string",
          description:
            "Il file da scegliere. Si può dare alla maniera di WSL (/mnt/c/…): viene tradotto.",
        },
        annulla: { type: "boolean", description: "Invece di scegliere, chiude la finestra con «Annulla»." },
      },
      additionalProperties: false,
    },
    async esegui({ percorso, annulla = false }) {
      if (annulla) return testo(await interfaccia({ azione: "scegli_file", annulla: true }));

      if (!percorso) return testo("Serve il percorso del file da scegliere, oppure «annulla».", true);

      // Il dialogo è una finestra di Windows: vuole C:\… Se il percorso arriva alla
      // maniera di WSL lo si traduce qui, invece di far sbagliare chi chiama.
      const percorsoWindows = percorso.startsWith("/")
        ? (await esegui(`wslpath -w "${percorso}"`)).uscita.trim()
        : percorso;

      if (!percorsoWindows) return testo(`Non ho saputo tradurre il percorso «${percorso}».`, true);

      return testo(await interfaccia({ azione: "scegli_file", percorso: percorsoWindows }));
    },
  },

  {
    name: "cartella_dati",
    description:
      "Elenca cosa l'applicazione ha scritto nella cartella dati (profilo, storico, opportunità, " +
      "documenti). Con «percorso» guarda una cartella dati diversa dalla predefinita.",
    inputSchema: {
      type: "object",
      properties: {
        percorso: { type: "string", description: "Percorso WSL della cartella dati da ispezionare." },
      },
      additionalProperties: false,
    },
    async esegui({ percorso }) {
      const radice = percorso || `${process.env.HOME ? "" : ""}/mnt/c/Users/${await utenteWindows()}/AppData/Roaming/TrovaLavoro`;
      const esito = await esegui(
        `if [ -d "${radice}" ]; then find "${radice}" -maxdepth 3 | head -60; else echo "La cartella dati non esiste ancora: ${radice}"; fi`
      );
      return testo(esito.uscita.trimEnd() || "(vuota)");
    },
  },
];

/**
 * Guida l'interfaccia dell'applicazione con lo script UI Automation. Ogni chiamata è
 * un'azione sola: elencare, premere, scrivere, rispondere a una finestra di scelta file.
 *
 * Le scelte viaggiano in un file JSON, non sulla riga di comando. Il motivo è stato
 * pagato: qui si compone una riga per bash che poi diventa una riga per PowerShell, e
 * nel doppio passaggio gli apostrofi sparivano — «Incolla qui il testo dell'annuncio»
 * arrivava allo script come un'etichetta vuota, e la casella «non si trovava». Un file
 * regge apostrofi, accenti e testi con gli a capo senza che nessuno debba proteggerli.
 */
async function interfaccia(scelte) {
  const script = join(QUI, "interfaccia.ps1");
  const scriptWindows = (await esegui(`wslpath -w "${script}"`)).uscita.trim();

  const scelteQui = join(tmpdir(), `interfaccia-${Date.now()}.json`);
  await writeFile(scelteQui, JSON.stringify(scelte), "utf8");
  const scelteWindows = (await esegui(`wslpath -w "${scelteQui}"`)).uscita.trim();

  try {
    // Il percorso del file va fra apici **singoli**: il file sta in /tmp, che per Windows
    // è `\\wsl.localhost\…`, e fra doppi apici bash mangia una delle due barre iniziali —
    // PowerShell si ritrova a cercare `C:\wsl.localhost\…`, che non esiste.
    // -STA: gli appunti di Windows si toccano solo da un thread «a stanza singola», e
    // senza questo Set-Clipboard non riempie niente senza nemmeno lamentarsi.
    const esito = await esegui(
      `powershell.exe -NoProfile -STA -ExecutionPolicy Bypass -File "${scriptWindows}" -Argomenti '${scelteWindows}' 2>&1`,
      { timeoutMs: 60_000 }
    );

    return (esito.uscita + esito.errori).trimEnd();

  } finally {
    await unlink(scelteQui).catch(() => {});
  }
}

/** Il nome utente Windows, per trovare %APPDATA% da WSL. */
async function utenteWindows() {
  const esito = await esegui(`cmd.exe /c "echo %USERNAME%" 2>/dev/null | tr -d "\\r"`);
  return esito.uscita.trim();
}

function attendi(ms) {
  return new Promise((r) => setTimeout(r, ms));
}

/** L'esito di un attrezzo in forma di testo. */
function testo(contenuto, errore = false) {
  return { content: [{ type: "text", text: contenuto }], isError: errore };
}

// ============================================================================
// La schermata: PowerShell cattura, Node la consegna
// ============================================================================

async function catturaSchermata({ schermo_intero, porta_in_primo_piano }) {
  const png = join(tmpdir(), `schermata-${Date.now()}.png`);

  const versoWindows = await esegui(`wslpath -w "${png}"`);
  const destinazione = versoWindows.uscita.trim();

  const script = join(QUI, "schermata.ps1");
  const scriptWindows = (await esegui(`wslpath -w "${script}"`)).uscita.trim();

  const argomenti = [
    `-Percorso '${destinazione}'`,
    schermo_intero ? "-SchermoIntero" : "",
    porta_in_primo_piano ? "-InPrimoPiano" : "",
  ]
    .filter(Boolean)
    .join(" ");

  const esito = await esegui(
    `powershell.exe -NoProfile -ExecutionPolicy Bypass -File "${scriptWindows}" ${argomenti} 2>&1`,
    { timeoutMs: 60_000 }
  );

  if (!existsSync(png)) {
    return testo(
      `Non sono riuscita a catturare la schermata.\n${coda(esito.uscita + esito.errori, 12)}`,
      true
    );
  }

  const immagine = await readFile(png);
  await unlink(png).catch(() => {});

  return {
    content: [
      { type: "text", text: coda(esito.uscita, 3) || "Schermata catturata." },
      { type: "image", data: immagine.toString("base64"), mimeType: "image/png" },
    ],
  };
}

// ============================================================================
// Il protocollo: JSON-RPC 2.0 sopra HTTP
// ============================================================================

async function rispondiA(richiesta) {
  const { id, method, params } = richiesta;

  if (method === "initialize") {
    return risultato(id, {
      protocolVersion: VERSIONE_PROTOCOLLO,
      capabilities: { tools: { listChanged: false } },
      serverInfo: { name: "collaudi-ai-cv-coach", version: "1.0.0" },
    });
  }

  if (method === "ping") return risultato(id, {});

  if (method === "tools/list") {
    return risultato(id, {
      tools: ATTREZZI.map(({ name, description, inputSchema }) => ({
        name,
        description,
        inputSchema,
      })),
    });
  }

  if (method === "tools/call") {
    const attrezzo = ATTREZZI.find((a) => a.name === params?.name);
    if (!attrezzo) return errore(id, -32602, `Attrezzo sconosciuto: ${params?.name}`);

    // Un argomento obbligatorio che non arriva va fermato **qui**, non lasciato passare
    // vuoto: chi cerca un controllo con l'etichetta vuota la trova su tutto, prende il
    // primo bottone della finestra e riferisce di averlo premuto. È successo il
    // 2026-08-12 chiamando «clic» col nome di argomento sbagliato, e la risposta è stata
    // «Premuto «🏠 Home»» — un successo per una cosa mai chiesta.
    const argomenti = params.arguments ?? {};
    const mancanti = (attrezzo.inputSchema.required ?? []).filter(
      (quale) => argomenti[quale] === undefined || argomenti[quale] === "",
    );

    if (mancanti.length > 0) {
      return errore(
        id,
        -32602,
        `A «${attrezzo.name}» manca ${mancanti.map((quale) => `«${quale}»`).join(" e ")}. ` +
          `Gli argomenti che vuole sono: ${Object.keys(attrezzo.inputSchema.properties ?? {}).join(", ")}.`,
      );
    }

    try {
      return risultato(id, await attrezzo.esegui(argomenti));
    } catch (guasto) {
      return risultato(id, testo(`L'attrezzo è inciampato: ${guasto.message}`, true));
    }
  }

  return errore(id, -32601, `Metodo non gestito: ${method}`);
}

function risultato(id, result) {
  return { jsonrpc: "2.0", id, result };
}

function errore(id, code, message) {
  return { jsonrpc: "2.0", id, error: { code, message } };
}

const server = createServer(async (richiesta, risposta) => {
  if (richiesta.method !== "POST" || !richiesta.url.startsWith("/mcp")) {
    risposta.writeHead(404).end();
    return;
  }

  let corpo = "";
  for await (const pezzo of richiesta) corpo += pezzo;

  let messaggio;
  try {
    messaggio = JSON.parse(corpo);
  } catch {
    risposta.writeHead(400, { "content-type": "application/json" });
    risposta.end(JSON.stringify(errore(null, -32700, "JSON non valido")));
    return;
  }

  // Le notifiche non hanno id e non vogliono risposta.
  if (messaggio.id === undefined) {
    risposta.writeHead(202).end();
    return;
  }

  const esito = await rispondiA(messaggio);

  risposta.writeHead(200, { "content-type": "application/json" });
  risposta.end(JSON.stringify(esito));
});

server.listen(PORTA, "127.0.0.1", () => {
  console.log(`Server MCP di collaudo su http://127.0.0.1:${PORTA}/mcp`);
  console.log(`Attrezzi: ${ATTREZZI.map((a) => a.name).join(", ")}`);
});
