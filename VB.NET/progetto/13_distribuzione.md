# 13 — Distribuzione: un solo exe

*Come il programma arriva sul PC dell'utente e ci resta: un file da copiare, senza
installazione, senza DLL a fianco.*

## 13.1 Stack e piattaforma

- **Linguaggio**: VB.NET.
- **Interfaccia**: Windows Forms (i pannelli statici del cap. 03, disegnati nel
  designer di Visual Studio).
- **Runtime**: .NET moderno in versione LTS (proposta: **.NET 8**, già collaudato
  dalla toolchain di casa; l'eventuale salto alla LTS successiva è una riga di
  progetto, non un cambio di disegno — v. cap. 15).
- **Architettura**: 64 bit (`win-x64`), solo Windows 11.
- **Librerie esterne**: le minime indispensabili, e tutte «inglobabili» —
  `Microsoft.Web.WebView2` (browser integrato) e `FontAwesome.Sharp` (icone).
  Per JSON si usa quello integrato in .NET (`System.Text.Json`); per DOCX, ZIP, SMTP,
  cifratura si usano i mattoni standard di .NET. Meno dipendenze = meno sorprese.

## 13.2 La pubblicazione single-file

Il vincolo «un exe, niente DLL» si realizza con la pubblicazione a file singolo di
.NET:

```
dotnet publish -c Release -r win-x64
  PublishSingleFile = true
  SelfContained     = true
  IncludeNativeLibrariesForSelfExtract = true
```

- **Autonomo (`SelfContained = true`)** — scelta proposta: l'exe contiene **anche il
  runtime .NET**. Pesa di più (≈150–180 MB) ma funziona su un PC nudo: copi il file,
  parte. Per un'app-biglietto-da-visita è l'esperienza giusta: nessun «prima installa
  il runtime».
- La variante leggera (framework-dependent, pochi MB ma richiede il runtime .NET
  installato) resta il formato comodo per lo sviluppo quotidiano. La scelta finale del
  formato di rilascio è in cap. 15.
- Le librerie NuGet e le risorse (pool di prompt integrato, logo, modelli DOCX/HTML)
  finiscono **dentro** l'exe; le parti native si estraggono da sole al primo avvio in
  una cartella temporanea gestita da .NET. Accanto all'exe non vive **nessuna** DLL.

## 13.3 Il caso WebView2

WebView2 usa il motore Edge/Chromium **già presente in Windows 11** (runtime
«Evergreen», preinstallato e aggiornato dal sistema): non lo distribuiamo noi. Se per
qualche motivo mancasse (edizioni particolari, PC aziendali bloccati), l'app **non
muore**: all'avvio lo rileva e mostra una finestra cortese con il link ufficiale di
Microsoft per installarlo, mentre le funzioni che non lo usano restano disponibili.

## 13.4 Cosa c'è accanto all'exe

| Elemento | Obbligatorio? |
|---|---|
| `AiCvCoach.exe` | sì — è tutto qui |
| `prompt-pool\` | no — solo per chi vuole aggiornare o studiare i prompt senza ricompilare (il pool integrato basta) |
| strumenti esterni opzionali (es. un elaboratore PDF della casa, **solo come exe binario**) | no — l'app funziona senza; se presenti, l'app li rileva |

I **dati** non stanno mai accanto all'exe: vivono in `%APPDATA%\AI-CV-COACH`
(cap. 11). Così l'exe può stare su desktop, in una cartella qualsiasi o su chiavetta
senza sparpagliare file.

## 13.5 Versione e identità

- La versione dell'app vive in **un solo file sorgente** (`Versione.vb`): formato
  `1.0.012` (maggiore.minore.build), mostrata nel pannello logo insieme alla versione
  del pool (`Ver. 1.0.012 · Pool 1.03`).
- Ogni modifica al codice incrementa il numero di build; la storia delle release sta
  nei tag Git del repo.
- L'icona dell'exe e il logo sono risorse incorporate.

## 13.6 Avvisi di Windows (SmartScreen)

Un exe nuovo e non firmato può far comparire l'avviso SmartScreen («PC protetto da
Windows»). Per la fase personale/portfolio è accettabile e si documenta nel README
(«Ulteriori informazioni → Esegui comunque»); la **firma del codice** con un
certificato è un'opzione futura annotata in cap. 15, non un prerequisito.

## 13.7 L'area di sviluppo nel repo

```
VB.NET/
├── PROMPT_DI_INCARICO.md      il mandato (immutabile)
├── progetto/                  questi documenti di progetto
├── prompt-pool/               nascerà a inizio implementazione, migrando i prompt dal prototipo
└── src/                       la soluzione Visual Studio (AiCvCoach.sln, progetto WinForms)
```

- Build di sviluppo da Visual Studio (o `dotnet build` da riga di comando Windows).
- Uno script `publish.bat` in `src/` produrrà l'exe di rilascio con i parametri del
  punto 13.2, sempre uguali: la pubblicazione non deve dipendere dalla memoria di
  nessuno.

## 13.8 Aggiornamenti

Prima versione: aggiornamento **manuale** — si scarica il nuovo exe e si sostituisce
il vecchio; i dati in `%APPDATA%` non si toccano e il programma riparte da dove era.
Niente auto-update in perimetro: per un'app personale è complessità senza guadagno
(annotato comunque tra le idee future).
