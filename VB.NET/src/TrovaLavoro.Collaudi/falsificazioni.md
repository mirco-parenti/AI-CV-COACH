# Le falsificazioni del banco

*Elenco versionato delle falsificazioni: che cosa rompere, e quale collaudo deve diventare
rosso. Nato il 2026-08-27 dalla revisione del giro D, che l'ha chiesto con una frase sola —
«elenco versionato delle falsificazioni» — e aveva ragione: fino a quel giorno le
falsificazioni esistevano solo nel racconto del `diario_di_bordo.md`, cioè in un posto dove
si leggono ma non si **rifanno**.*

## A che serve

La **regola 14** del `CLAUDE.md` dice che un collaudo che sorveglia un meccanismo si prova a
farlo fallire prima di dirlo buono. Un collaudo verde dice che il codice fa quel che ti
aspetti; solo un collaudo che hai *visto* diventare rosso dice che si accorgerebbe del
contrario.

Questo file è la memoria operativa di quelle prove. Non racconta — per il racconto c'è il
diario — ma dice, meccanismo per meccanismo, **come si rompe** e **che cosa deve cadere**.
Serve tre volte: quando si tocca quel codice, quando un collaudo si comporta in modo strano,
e quando si vuole sapere se una parte del banco è sorvegliata davvero o è verde per caso.

## Come si usa

1. Si applica la modifica della colonna «Come si rompe» al file indicato.
2. `dotnet test TrovaLavoro.sln --filter "FullyQualifiedName~<classe di collaudo>"` da `VB.NET/src`.
3. Devono cadere **quelli elencati, e solo quelli**. Un rosso in più o in meno è una notizia.
4. Si ripristina con **`cp file.bak file`**, mai con `mv`: `mv` non cambia l'ora del file e
   la compilazione successiva può non accorgersi che il sorgente è tornato quello di prima.

## L'elenco

| Meccanismo | Dove | Come si rompe | Che cosa deve diventare rosso |
|---|---|---|---|
| L'eseguibile dichiara il commit da cui nasce (D-R1) | `TrovaLavoro/Versione.vb` | `RigaDelSorgente(codice)` restituisce sempre «non dichiarato» | `LaRigaDelSorgenteDiceIlCommit`, `UnAlberoSporcoSeLoPortaDietro` |
| «Informazioni» mostra quella riga | `TrovaLavoro/Ui/FinestraInformazioni.vb` | la proprietà legge `lblVersione.Text` invece di `lblSorgente.Text` | `LaFinestraMostraDaQualeSorgenteNasce` |
| L'indirizzo dell'`.eml` non apre una riga nuova (M1) | `TrovaLavoro/Documenti/ScrittoreEml.vb` | `Indirizzo` torna a fare solo `Trim()` | `UnACapoNelDestinatarioNonApreUnaRigaNuova`, `UnACapoNelMittenteNonApreUnaRigaNuova` |
| Una cella del CSV non è una formula (M2) | `TrovaLavoro/Dati/EsportazioneRegistro.vb` | `NonUnaFormula` restituisce il valore intatto | `UnaCellaCheSembraUnaFormulaNonLoDiventa` |
| `impostazioni.json`: anche la lingua ha la sua rete | `TrovaLavoro/Motore/Impostazioni.vb` | si toglie il `Try/Catch` attorno a `lingua_predefinita` | `UnaLinguaCheNonEUnaStringaSiScartaSenzaEsplodere` |
| Il `.docx` dichiara la versione del formato (D-R3) | `TrovaLavoro/Documenti/ScrittoreDocx.vb` | non si scrive più `word/settings.xml` nel pacchetto | `IlPacchettoDichiaraLaVersioneDelFormato`, `IlPacchettoHaLeOttoParti` |
| Le date su disco portano il fuso | `TrovaLavoro/Dati/CampiJson.vb` | `FormatoIstante` torna a `"yyyy-MM-dd HH:mm:ss"` | `LaDataScrittaSuDiscoPortaIlSuoFuso` |
| Le date del formato vecchio si leggono ancora | `TrovaLavoro/Dati/CampiJson.vb` | si toglie il secondo `TryParseExact` (quello senza fuso) | `LeDateScritteDallaVersionePrecedenteSiLeggonoAncora` |
| L'attesa della stampa ha un tetto | `TrovaLavoro/Motore/Attese.vb` | `EntroIlTetto` fa `Return Await compito` e basta | `UnAttesaCheNonFinisceScadeInveceDiRestareAppesa` |
| L'ultima rete non mostra stack trace | `TrovaLavoro/UltimaRete.vb` | si usa `eccezione.ToString()` invece di `.Message` | `LErroreSiRaccontaInItalianoESenzaStackTrace` |
| Il diario tecnico non porta fuori la chiave | `TrovaLavoro/Dati/DiarioTecnico.vb` | `SenzaSegreti` restituisce il testo intatto | `UnaChiaveApiNonEntraNelDiario`, `AncheLIntestazioneCheLaPortaSiRipulisce`, `LaDiagnosticaNonPortaFuoriLaChiave` |
| Il diario non cresce senza fine | `TrovaLavoro/Dati/DiarioTecnico.vb` | `FaiPostoSeServe` esce subito senza guardare la dimensione | `IlDiarioNonCresceSenzaFine` |
| Il diario non solleva mai | `TrovaLavoro/Dati/DiarioTecnico.vb` | il `Catch` di `Annota` rilancia (`Throw`) | `UnDiarioCheNonSiLasciaScrivereNonFaCadereIlProgramma` |
| La barra torna a dire quel che diceva (D-R2) | `TrovaLavoro/Ui/SegnaleDiAttesa.vb` | `Ferma()` restituisce stringa vuota | `FinitaLAttesaLaBarraTornaADireQuelloCheDiceva` |
| Chiave rifiutata ≠ rete assente | `TrovaLavoro/Ai/ProvaChiave.vb` | 401/403 mappati su `CausaErroreAi.Rete` | `UnaChiaveRifiutataNonSiConfondeConLaReteAssente`, `OgniStatoDellApiDiceLaSuaCosa` |
| `modelli.json`: si cambia un campo, non il file | `TrovaLavoro/Ai/Modelli.vb` | `ConLivello` parte sempre da un oggetto vuoto (`If True Then radice = New JsonObject()`) | `CambiareUnLivelloNonToccaIlResto`, `UnFileCheNonSiCapisceNonSiSostituisce` |
| Un livello assente porta con sé l'interruttore in vigore | `TrovaLavoro/Ai/Modelli.vb` | il ramo `interruttoreInVigore.HasValue` sparisce: si scrive sempre la forma breve | `UnLivelloAssenteSiScriveConLInterruttoreInVigore` |
| Prima si compone, poi si scrive | `TrovaLavoro/Ai/Modelli.vb` | in `CambiaModello` si assegna il modello nuovo **prima** di scrivere il file | `UnDiscoCheRifiutaNonCambiaNienteInVigore` |
| Nella tendina c'è sempre il modello in uso | `TrovaLavoro/Ai/ElencoModelli.vb` | `ConQuelloInUso` non fa più l'`Insert(0, …)` | `NellElencoCEsempreQuelloCheSiStaUsando`, `UnModelloRitiratoRestaNellaTendina` |
| Un elenco vuoto non è un elenco | `TrovaLavoro/Ai/ElencoModelli.vb` | `DalCorpo` restituisce `Riuscito` anche con zero modelli | `UnElencoVuotoArrivatoBeneNonServeANiente` |
| L'informativa nomina ogni porta da cui qualcosa esce | `TrovaLavoro/Ui/FinestraInformativa.vb` | si toglie da `Voci()` la riga su GitHub | `NominaTutteLePorteDaCuiQualcosaEsce` |
| «Vista una volta» sopravvive alla chiusura | `TrovaLavoro/Motore/Impostazioni.vb` | `VersoJson` scrive `informativa_vista: False` fisso | `LAverlaVistaSiRicordaSuDisco` |
| Le versioni si confrontano per numeri | `TrovaLavoro/Motore/ControlloVersione.vb` | `Confronta` usa `String.CompareOrdinal` invece di `Paragone` | `LaStessaVersioneScrittaInDueModiELaStessa` |
| Una versione che non si capisce non è un verdetto | `TrovaLavoro/Motore/ControlloVersione.vb` | in `Pezzi` il `TryParse` fallito non torna più `Nothing`: il pezzo vale zero | `UnaVersioneCheNonSiCapisceNonDiventaUnVerdetto` |
| Il controllo parte solo se lo si chiede | `TrovaLavoro/Ui/FinestraInformazioni.vb` | si chiama `ControllaLaVersione()` dentro il costruttore | `AprirlaNonChiedeNienteANessuno` |
| Un modello senza prezzo non vale zero | `TrovaLavoro/Ai/Listino.vb` | `PerModello` restituisce `New PrezzoModello(0, 0)` invece di `Nothing` | `UnModelloSconosciutoNonHaPrezzo`, `UnModelloSenzaPrezzoContaITokenENonISoldi`, `IlBucoSiDichiara` |
| Una riga senza data non è di oggi | `TrovaLavoro/Dati/ContoDelleChiamate.vb` | la condizione dei giorni recenti diventa `Not quando.HasValue OrElse …` | `UnaRigaSenzaDataStaNelTotaleENonNeiGiorniRecenti` |
| Sotto il centesimo non si scrive zero | `TrovaLavoro/Ui/FinestraImpostazioni.vb` | si toglie la riga «meno di $0,01» da `Spesa` | `SottoIlCentesimoNonSiScriveZero` |
| Un modello ritirato non è una richiesta sbagliata | `TrovaLavoro/Ai/ClientClaude.vb` | `ParlaDiUnModelloCheNonCE` restituisce sempre `False` | `UnModelloRitiratoNonEUnaRichiestaSbagliata`, `IlModelloRitiratoSiRiconosceDalTipoAncheSenza404`, `AncheInStreamingIlModelloRitiratoSiRiconosce` |
| E il messaggio dice **quale** | `TrovaLavoro/Ai/ClientClaude.vb` | `SpiegaIlModello` scrive sempre «richiesto» invece del nome | `UnModelloRitiratoNonEUnaRichiestaSbagliata` |
| Riempire le tendine non è scegliere | `TrovaLavoro/Ui/FinestraImpostazioni.vb` | in `RiempiLeTendine` la guardia diventa `_sto = False` | `AprirlaNonScriveModelliJson`, `LElencoArrivatoAllungaLeTendineSenzaCambiareLaScelta`, `DiceCosaGiraSottoIlCofano`, `SalvareUnaPreferenzaAccendeIlBottoneCheEliminaTutto`, `IBottoniDistruttiviSonoSpentiQuandoNonCENiente` |

| L'alias e la versione datata sono lo stesso modello | `TrovaLavoro/Ai/ElencoModelli.vb` | in `ConQuelloInUso` il confronto torna `v.Id = cercato` | `LAliasNonSiRaddoppiaConLaSuaVersioneDatata` |
| Il listino riconosce l'alias di quel che ha risposto | `TrovaLavoro/Ai/Listino.vb` | si toglie da `PerModello` il ciclo su `IdModello.StessoModello` | `IlPrezzoSiTrovaAncheConLIdentificativoDatato` |
| Si toglie la data, e **solo** la data | `TrovaLavoro/Ai/IdModello.vb` | la regex diventa `-\d+$` invece di `-\d{8}$` | `QuelCheNonEUnaDataResta`, `DueModelliDiversiRestanoDiversi`, `LAliasNonSiRaddoppiaConLaSuaVersioneDatata`, `UnRitiratoEntraLoStessoAncheOraCheSiRiconosconoGliAlias` |
| In «Informazioni» nessun controllo ne copre un altro | `TrovaLavoro/Ui/FinestraInformazioni.Designer.vb` | `btnControllaVersione` torna in `Point(120, 464)`, sopra la riga del copyright | `NessunControlloNeCopreUnAltro` |
| E nessuno esce dalla finestra | `TrovaLavoro/Ui/FinestraInformazioni.Designer.vb` | `btnComeFunziona` va in `Point(14, 700)`, sotto il bordo | `TuttoStaDentroLaFinestra` |
| Un 404 non è un guasto: è «non c'è nessuna versione» | `TrovaLavoro/Motore/ControlloVersione.vb` | si toglie il ramo `If risposta.StatusCode = HttpStatusCode.NotFound` | `SenzaNessunaVersionePubblicataNonSiParlaDiGuasti` |
| «Chiudi» delle Impostazioni non scorre via | `TrovaLavoro/Ui/FinestraImpostazioni.Designer.vb` | `btnChiudi` torna dentro `pnlContenuto` invece che nella fascia | `ChiudiRestaInVistaAncheQuandoIlContenutoNonCiSta`, `QuandoSiScorreNienteFinisceSottoLaBarra` |
| E la fascia si ancora davvero al fondo | `TrovaLavoro/Ui/FinestraImpostazioni.Designer.vb` | `pnlFascia.Dock` diventa `DockStyle.None` | `ChiudiRestaInVistaAncheQuandoIlContenutoNonCiSta` |

## Tre cose imparate falsificando, che valgono più della tabella

- **Un collaudo può restare verde per il motivo sbagliato.** Il primo collaudo dell'a capo
  nell'`.eml` spezzava le righe solo su `CRLF`: col codice rotto e un a capo isolato (`LF`)
  restava verde. L'ha trovato la falsificazione, non la rilettura.
- **L'eco dell'API può passare per parola nostra.** Il collaudo del modello ritirato
  chiedeva che il messaggio dicesse *quale* modello: falsificando la funzione che lo
  nomina, restava verde — perché quell'identificativo compariva anche nel corpo d'errore
  che l'API rimanda indietro, e il collaudo lo leggeva lì. Adesso il corpo finto **non**
  nomina il modello, e la prova riguarda solo quel che scriviamo noi. *(2026-08-27.)*
- **Un collaudo appeso non è rosso, è niente.** Rompendo il tetto delle attese, il collaudo
  che lo sorveglia aspetterebbe per sempre: senza il suo `<Timeout(5000)>` la falsificazione
  non avrebbe prodotto nessun rosso, e sarebbe sembrata una prova superata.
- **Due guardie che si coprono a vicenda non sono due difese: sono una difesa e un ciclo
  nascosto.** Le tendine dei modelli avevano una guardia sugli eventi *e* un controllo
  «se è già quello, non fare niente». Rompendone una, il banco restava verde; rompendole
  **insieme**, non è caduto un collaudo — è caduto il processo, con una ricorsione senza
  fondo (`CambiaIlModello` → riempi → evento → `CambiaIlModello`). La cura non è stata
  rimettere le guardie: è stata **togliere l'anello**, separando chi riempie le tendine da
  chi aggiorna le righe sotto. Poi una guardia sola è bastata, ed è diventata falsificabile
  per davvero — cinque rossi. *(2026-08-27.)*

- **Un dato di prova più gentile della realtà rende il banco cieco, e nessuna falsificazione
  lo stana.** La risposta finta dell'elenco modelli dichiarava `claude-haiku-4-5`, che è
  l'alias con cui il programma *chiede*; l'API vera dichiara `claude-haiku-4-5-20251001`. Con
  quel dato ogni collaudo era verde e ogni falsificazione produceva il suo rosso, mentre
  nell'applicazione l'alias e la versione datata venivano trattati come **due modelli
  diversi** — la tendina mostrava Haiku 4.5 due volte e il contatore di spesa non prezzava le
  chiamate del modello predefinito. La falsificazione difende il collaudo dal codice, non il
  collaudo dai propri dati: quelli si difendono solo **guardando la cosa vera** — qui, aprendo
  le Impostazioni e leggendo che cosa c'è scritto nelle tendine. *(2026-08-27.)*

## Quel che manca

Le falsificazioni **precedenti** al 2026-08-27 — il ciclo del server reso di nuovo seriale
(T8b), il lucchetto reso permissivo (T8c), il client dello streaming privato del riarmo del
silenzio, le tre del `ControlloCriterio` — sono raccontate nel `diario_di_bordo.md` e non
sono ancora trascritte qui. Vanno riportate in tabella **rifacendole**, una alla volta: una
riga scritta a memoria varrebbe quanto non averla.
