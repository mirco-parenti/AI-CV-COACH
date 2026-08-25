# I dati del giro D

Il **giro D** è il collaudo dal vivo sul PC del tutor — una macchina senza SDK .NET 10 e
con Word — e il suo copione è il **§13.10** di `VB.NET/progetto/13_distribuzione.md`.
Questa cartella è il materiale che si porta là: un CV da importare e tre annunci da
incollare.

**I dati sono inventati**, come tutto ciò che sta in `casi/` — e qui la ragione è doppia:
il repo è pubblico, e la macchina è di qualcun altro. Sulla postazione del tutor non
deve finire niente di vero, né mio né di terzi. Il candidato è lo **stesso Luca Ferrari**
di `../profilo.json`, e non per pigrizia: quel file diventa così il **criterio** con cui
si dice se l'import ha funzionato, invece di giudicare a occhio.

| File | Cos'è |
|---|---|
| `cv_luca_ferrari.pdf` | Il CV da importare, **due pagine**. È la fonte del giro: da qui entra il profilo. |
| `cv_luca_ferrari.html` | Il sorgente da cui il PDF è generato. Sta qui perché il PDF si possa **rifare** invece di essere un binario di cui nessuno sa più l'origine. |
| `annuncio_1_alto.txt` | «Addetto al magazzino», Forlì. Il profilo lo copre quasi tutto: match alto. |
| `annuncio_2_medio.txt` | «Magazziniere carrellista, turno notturno», Cesena. Gap veri ma nessun requisito tassativo: è il caso in cui si vedono i giudizi `~` e `✗` e le mitigazioni. |
| `annuncio_3_eliminatorio.txt` | «Autista patente C», Cesena. Fa scattare l'**hard-gate**: qualunque sia il resto, il match è tagliato a ≤ 20/100. |

## Il criterio: cosa deve uscire dall'import

Importato `cv_luca_ferrari.pdf`, il profilo che compare in P2 deve corrispondere a
`../profilo.json` — stessi campi, stessi fatti. Non conta la prosa, contano i fatti:
nome, i tre recapiti, patente B, le **due** esperienze formali con le loro durate, **una**
esperienza informale, le quattro competenze, i due titoli di formazione.

**Nel CV ci sono due insidie messe apposta**, e sono la parte che vale la pena guardare:

1. **Residenza a Cesena, domicilio a Forlì.** Il prompt `importa_cv` dice di tenere il
   **domicilio** quando i due differiscono: l'atteso è `"citta": "Forlì"`. Un CV che desse
   un indirizzo solo non proverebbe niente, e questa è la regola che nessuno guarderebbe.
2. **I traslochi stanno sotto «Altre esperienze»**, con un'intestazione che somiglia a
   quella dei lavori veri. Devono finire in `esperienze_informali`, mai fra le formali:
   è l'anti-invenzione dal lato che si dimentica sempre — non inventare un lavoro che la
   persona non ha dichiarato come tale.

Se una delle due non regge, non è il giro D a essere fallito: è un reperto, e vale il
viaggio da sola.

## Gli esiti attesi dei tre annunci

| Annuncio | Che cosa ci si aspetta | Perché è nel giro |
|---|---|---|
| 1 — alto | Stelle alte, tutti o quasi i requisiti soddisfatti | Mostra il caso che funziona: è quello che il tutor deve vedere per primo |
| 2 — medio | Punteggio di mezzo. Gap veri: **SAP** (lui usa un terminale di magazzino, non SAP), l'**inglese** (che il profilo non dichiara affatto), il **turno notturno** (dichiara turni, non la notte), i **2 anni** in un magazzino strutturato (ne ha 3, quindi questo regge) | È l'unico dei tre in cui mitigazioni e giudizi parziali hanno qualcosa da dire |
| 3 — eliminatorio | **≤ 1 stella**, con ⛔ sui requisiti che craterano: patente C e CQC, che il profilo non ha | Mostra la difesa: l'app non promette un colloquio che non arriverà mai |

*Nell'annuncio 2 le parole sono scelte con cura: nessun requisito che il candidato non ha
è scritto come «indispensabile» o «obbligatorio», perché il prompt del confronto marca
`eliminatorio` solo davanti a un linguaggio tassativo — e un eliminatorio di troppo
schiaccerebbe il caso medio a ≤ 1 stella, facendone un doppione del terzo.*

## Come si rifà il PDF

Il PDF si rigenera dall'HTML con LibreOffice. Da WSL la strada che **funziona** è una
sola, e le due varianti ovvie falliscono tutt'e due:

```bash
# 1. il file deve stare in una cartella Windows vera: da un percorso UNC non si parte
cp cv_luca_ferrari.html /mnt/c/Temp/giro-d-pdf/

# 2. soffice.exe si invoca DIRETTAMENTE, mai passando da cmd.exe, e i percorsi
#    si scrivono in forma Windows
"/mnt/c/Program Files/LibreOffice/program/soffice.exe" --headless --norestore \
  --infilter="HTML (StarWriter)" --convert-to pdf \
  --outdir 'C:\Temp\giro-d-pdf' 'C:\Temp\giro-d-pdf\cv_luca_ferrari.html'
```

- **`cmd.exe` non serve e non funziona**: da una cartella WSL avvisa che «i percorsi UNC
  non sono supportati», riparte da `C:\Windows` e perde le virgolette del percorso.
- **`--infilter="HTML (StarWriter)"` non è un dettaglio**: senza, LibreOffice apre l'HTML
  come documento *Writer/Web*, dove il formato pagina non è quello del foglio.
- **`file` mente sul numero di pagine** di questi PDF (dice cinque): le pagine vere sono
  due, e si contano leggendo il `/Count` dell'oggetto `/Type /Pages`.

## Che cosa questi dati **non** coprono

Il `.docx` **salvato davvero da Word** (la voce D3 del copione) non può nascere qui: nasce
là, esportando dall'applicazione, aprendo il file in Word e risalvandolo. È il motivo per
cui il giro D esiste, e nessun file di questa cartella lo sostituisce.
