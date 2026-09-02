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

| Il bianco si legge sul fondo che ha sotto, e la scala dei livelli sale fino in fondo | `TrovaLavoro/StileApp.vb` | nel `Case Else` di `Dipingi` il fondo torna `RossoTitoli` | `OgniBottoneColoratoPortaUnTestoCheSiLegge` |
| Un inchiostro colorato si legge sul fondo su cui è scritto | `TrovaLavoro/StileApp.vb` | `Successo` torna a `#28A745` (o `InformazioneTesto` a `#17A2B8`, o `RossoCritico` a `#FA0825`) | `OgniInchiostroColoratoSiLeggeSuiFondiDellApplicazione` — e col solo verde anche `OgniBottoneColoratoPortaUnTestoCheSiLegge`, che è il bianco sul fondo del livello 1 |
| Un fondo pieno reagisce al mouse | `TrovaLavoro/StileApp.vb` | si tolgono le due righe `MouseOverBackColor`/`MouseDownBackColor` in fondo a `Dipingi` **e** a `DipingiLaCasella` | `OgniBottoneSiScurisceSottoIlPuntatore`, `AncheLeCaselleDellaBarraSiScurisconoSottoIlPuntatore` |
| Mentre l'AI legge un CV la barra di navigazione è spenta | `TrovaLavoro/Ui/PannelloProfilo.vb`, `Ui/FormPrincipale.vb` | si toglie il `RaiseEvent LavoroAiCambiato` da `LetturaInCorso` (rosso di guardia: «l'AI è stata chiamata davvero»), oppure il gestore della finestra forzato a `BarraDiNavigazione(libera:=True)` (rosso sull'asserzione che conta: 7 bottoni accesi invece di 0) | `MentreLeggeIlCvLaBarraSiSpegneTutta` |
| Un guasto in P3 porta la parola e il colore, e il rosso non resta addosso | `TrovaLavoro/Ui/PannelloRicerca.vb` | si toglie la riscrittura del colore da `Racconta`, oppure il prefisso da `RaccontaUnErrore` | `DopoUnErroreLaRigaTornaGrigia` il primo, `UnaPaginaCheNonSiLasciaLeggereNonFaCadereIlPannello` il secondo |
| Quel che non ci sta nelle tre finestre si scorre invece di tagliarsi | `TrovaLavoro/Ui/FinestraAppunti.vb`, `FinestraDocumenti.vb`, `FinestraModificaTesti.vb` | si toglie il tetto (`AltezzaSostenibile` sostituita dall'altezza voluta), oppure si spegne `AutoScroll` | i tre «quando non ci sta si scorre» delle tre finestre: tre rossi per ciascuna delle due rotture |
| Gli elenchi si misurano sulla larghezza della finestra, non su una costante | `TrovaLavoro/Ui/FinestraModificaTesti.vb`, `FinestraDocumenti.vb` | elenco rimesso su una costante — per FinestraDocumenti serve 752, perché a 96 DPI la 772 coincide con la misura giusta e il collaudo resta verde: limite dichiarato | `LElencoDiDestraArrivaFinoAlMargine`, `LElencoArrivaFinoAlMargine`; la costante che sfonda accende anche i due dello scorrimento |
| Dietro la ruota dell'attesa non c'è più il marchio, e la geometria è la sua | `TrovaLavoro/Ui/ScudoDiCaricamento.vb` | si rimette lo stemma con `LogoAviolab.Genera` (rosso **soltanto** il primo collaudo qui accanto); la fetta della ruota com'era ai tempi dello stemma (4 rossi); `DistaccoDellaBarra` a 0 (3); il limite verticale sulla sola ruota (2) | `DietroLaRuotaNonCePiuIlMarchio`, `LaRuotaRiempieLaSuaFettaDiTela`, `FraLaRuotaELaBarraLAriaSiVede`, `LIndicatoreStaDentroIDueLimitiChiestiDaMirco`, `SuUnoSchermoLargoEBassoComandaLAltezza`, `LaBarraSiAggiungeSottoLaRuotaESenzaRubargliNiente` |
| I comandi della fascia di ricerca hanno la misura di tutti, su tre righe che non si pestano | `TrovaLavoro/Ui/PannelloRicerca.Designer.vb` | `btnApri` rimesso a 26 px, oppure `btnCerca` rimesso a y=35 | `IComandiDellaRicercaSonoAltiComeGliAltriDellApplicazione`, `LeTreRigheDellaFasciaNonSiPestanoIPiedi` |
| I livelli dicono la conseguenza: cattura L3, riscrivi L4, ripristina L4, rileggi L2 | `TrovaLavoro/Ui/PannelloRicerca.vb`, `PannelloEmail.vb`, `FinestraBackup.vb`, `FinestraDocumenti.vb` | ciascun bottone rimesso al livello di prima, uno alla volta | `LaCatturaNonEPiuLaGemellaDellImportDelCv`, `RiscrivereEUnAzioneDiLivelloQuattro`, `RipristinareModificaMaNonDistrugge`, `LAzionePrincipaleDiQuestaFinestraEConfermare` |
| «Fallo riscrivere» difende il lavoro a mano, e di una bozza ripresa dal disco non si sa | `TrovaLavoro/Ui/PannelloEmail.vb` | tolto `_corpoAMano = True`, oppure `_bozzaRipresa = True`; per il caso «appena scritto dall'AI non chiede» le due difese si coprono a vicenda — guardia dei riempimenti e reset dopo la scrittura — e il rosso arriva solo togliendole **entrambe** | `RiscrivereDiceQualiTestiCorrettiAManoSpariscono`, `DiUnaBozzaRipresaDalDiscoNonSiSaESiDice`, `UnaCandidaturaNuovaNonEreditaISospettiDiQuellaDiPrima`, `AppenaScrittoDallAiRiscrivereNonChiedeNiente` |
| Le conferme di livello 5 dicono che cosa succede e che cosa no | `TrovaLavoro/Ui/PannelloProfilo.vb`, `PannelloRicerca.vb`, `PannelloOpportunita.vb` | tolte le frasi che il collaudo pretende («Salva profilo», «Non tocco né gli annunci…», «Non cancello niente») | `TogliereUnaVoceDiceQualeEQuandoArrivaSulDisco`, `DimenticareUnaRicercaDiceCheCosaSparisceEChePuoTornare`, `LoScartoDiceCheCosaNonSuccede` |
| L'eliminazione di tutto ha una fascia tutta sua | `TrovaLavoro/Ui/FinestraImpostazioni.vb` | il critico riallineato a sinistra e il margine sotto rimesso a 14 | `LEliminazioneDiTuttoHaUnaFasciaTuttaSua` |
| Gli stati vuoti guidano invece di tacere | `TrovaLavoro/Ui/FinestraDocumenti.vb`, `FinestraModificaTesti.vb`, `PannelloHome.vb` | il ramo «cartella senza documenti» riportato al testo semplice; «Scegli una riga» rimesso anche a elenco vuoto; il messaggio della coda riportato a «Nessuna candidatura, per ora.» | `UnaCartellaSenzaDocumentiLoDiceInvecediMostrareUnaListaMuta`, `AElencoVuotoNonSiChiedeDiSceglieUnaRiga`, `UnaCodaVuotaDiceAncheDaDoveSiComincia` |
| Etichette, incolonnamento e Tab seguono l'occhio | `TrovaLavoro/Ui/PannelloDialogo.vb`, `PannelloHome.Designer.vb`, `PannelloEmail.Designer.vb` | tolto `lblRisposta.Visible = chiedeUnaRisposta` — e la **prima stesura** del collaudo restava verde con la rottura dentro, perché guardava apertura e fine del dialogo dove sparisce la fascia intera: riscritta sul turno a scelta, l'unico momento in cui la riga decide; tolto `colQuando.TextAlign`; `btnHoSpedito.TabIndex` rimesso a 2 | `LaCasellaDellaRispostaPortaLaSuaEtichetta`, `IlPunteggioELaDataSiLeggonoIncolonnati`, `IlTastoTabPercorreLaFasciaComeLaLeggeLOcchio` |
| Il marchio è posato sull'angolo, senza contorno né fondo suo | `TrovaLavoro/Ui/FormPrincipale.vb` | filo nero rimesso attorno al pannello, oppure fondo non più riallineato al pannello sotto | `IlPannelloDelMarchioNonHaContorno`, `IlPannelloDelMarchioPrendeIlFondoDiChiGliStaSotto` |
| La scritta del menu scala con i fermi, la fascia segue lei, e la colonna non la pesta | `TrovaLavoro/Ui/PannelloMenu.vb` | `Clamp` tolto; fascia rilegata alla sola altezza — e qui `SopraLaZonaRestaPostoPerNomeESottotitolo` restava **verde**, perché difendeva la regola vecchia: limite dichiarato, la regola nuova la difende il secondo collaudo qui accanto; guardia della colonna tornata al bordo del pannello; spazio contato sull'area intera | `LaScrittaSmetteDiCrescereEDiRimpicciolire`, `LaFasciaDelNomeSegueLaScrittaENonLAltezza`, `ANessunaMisuraLaColonnaPestaIlNome`, `DallaMisuraMinimaInSuLUltimaVoceRestaDentro` |
| Ogni bottone ha una misura della scala, dice il nome per intero, e nessuno resta piatto | `TrovaLavoro/StileApp.vb` e Designer vari | una larghezza ad-hoc rimessa (115 px); un gradino stretto sotto la scritta (190→110); `FlatStyle.Flat` rimesso in `Dipingi`; `attiva` ignorato in `DipingiLaCasella` | `OgniBottoneHaUnaMisuraDellaScala`, `OgniBottoneDiceIlProprioNomePerIntero`, `NessunBottoneVestitoDaQuiRestaPiatto`, `LaCasellaApertaSiDistinguePerIlFondo`, `UnaCasellaApertaSiRisvegliaAncoraAperta` |
| Lo splash dura dieci secondi, ma Invio e il clic lo chiudono subito — e da chiuso non mangia più Invio | `TrovaLavoro/Ui/FinestraAvvio.vb` | minimo rimesso a 5; Invio scambiato con F13 nel filtro; guardia `_chiusa` tolta; clic scollegato | `IlMinimoAVideoEQuelloDichiarato`, `InvioMandaViaLaSchermataSenzaAspettareIlMinimo`, `UnaSchermataGiaChiusaNonSiMangiaPiuLInvio`, `IlClicMandaViaLaSchermataSenzaAspettareIlMinimo` |
| La finestra si apre al suo tetto, e il tetto si converte col DPI | `TrovaLavoro/ScalaSchermo.vb` | finestra che riprende tutta l'area di lavoro; tetto non più convertito | `SuUnoSchermoGrandeLaFinestraSiApreAlSuoTetto`, `A144DpiIlTettoDiAperturaValeUnaVoltaEMezza` |
| L'aiuto sta in barra senza essere una casella, e i credits si leggono a richiesta senza nominare gli attrezzi | `TrovaLavoro/Ui/FormPrincipale.Designer.vb`, `FinestraInformativa.vb` | aiuto vestito da casella della barra; aiuto messo fra i bottoni di navigazione; credits che non si aggiungono; «Claude Code» scritto nei credits | `LAiutoStaInBarraMaNonEUnaCasella`, `MentreLAiLavoraLaBarraSiSpegneTutta`, `ICreditiSiLeggonoSoloQuandoSiChiedono`, `ICreditiNonNominanoGliAttrezziDiChiLiHaScritti` |
| Le Impostazioni sono due colonne, e il critico ha la riga sua | `TrovaLavoro/Ui/FinestraImpostazioni.vb` | un comando largo quanto la scritta; un comando rimesso sotto il paragrafo; critico spostato in un'altra colonna; scritta lunga rimessa nella colonna | `IComandiDelleImpostazioniStannoInUnaColonnaSola`, `OgniComandoStaAllAltezzaDellaSuaSezione`, `LEliminazioneDiTuttoHaUnaRigaTuttaSua`, `OgniComandoDelleImpostazioniDiceIlProprioNomePerIntero` |
| La scritta del menu segue la finestra anche sullo schermo ingrandito, senza memoria della misura | `TrovaLavoro/Ui/PannelloMenu.vb` | clamp rimesso senza conversione DPI (a 144 punti minimo e massimizzata davano lo stesso riferimento); misura tolta dalla chiave della tela (rosso sull'inchiostro dipinto); introdotta una memoria della larghezza più stretta (rosso sulla geometria) — e la prima stesura del collaudo restava **verde** con la tela rotta, perché non dipingeva mai alla misura piccola: riscritta | `LaScrittaSegueLaFinestraAncheSulloSchermoIngrandito`, `DopoUnIngrandimentoLaScalaEQuellaDellaMisuraNuova` |
| Lo stemma non invita più a un clic, e «Informazioni su…» si apre dalle Impostazioni col foglietto vero | `TrovaLavoro/Ui/FormPrincipale.vb`, `FinestraImpostazioni.vb` | rimesso l'invito (prima il cursore, poi il solo tooltip: due rossi); tolto il composer dalla consegna (rosso su «può copiare la diagnostica») — e la prima stesura costruiva la finestra da sé e restava **verde**, perché collaudava la propria ricostruzione: da lì è nato `InformazioniSuTrovaLavoro()` come punto unico | `LoStemmaNonInvitaPiuAUnClic`, `DalleImpostazioniSiArrivaAInformazioniSu` |

| Il diario stderr del server MCP non porta fuori la chiave (R1) | `TrovaLavoro/Mcp/ServerMcp.vb` | in `Annota` torna `_diario.WriteLine(riga)` senza `SenzaSegreti` | `UnaChiaveDentroUnaRigaDelDiarioNonEsceDiQui` |
| E nemmeno la risposta d'errore che torna al client (R1-residuo) | `TrovaLavoro/Mcp/ServerMcp.vb` | si toglie `SenzaSegreti` da `ex.Message` nell'ultima rete di `RispondiAsync` | `UnaChiaveDentroUnEccezioneNonTornaAlClient` |
| Un nome allegato tutto ASCII non salta la ripulitura dell'a capo | `TrovaLavoro/Documenti/ScrittoreEml.vb` | `PerIntestazione` torna `Codificata(…Replace(""""""…))` senza `SenzaACapo` | `UnACapoNelNomeDellAllegatoNonApreUnaRigaNuova` |
| Un titolo abnorme non sfonda il nome della cartella | `TrovaLavoro/Dati/ArchivioOpportunita.vb` | si toglie `If costruito.Length >= Massimo Then Exit For` dallo slug | `UnTitoloAbnormeNonSfondaIlNomeDellaCartella` — rotto, cade prima l'`IOException` del sistema operativo dell'asserzione: è esattamente il danno che il tetto previene |

## Tre cose imparate falsificando, che valgono più della tabella

- **Un collaudo può restare verde per il motivo sbagliato.** Il primo collaudo dell'a capo
  nell'`.eml` spezzava le righe solo su `CRLF`: col codice rotto e un a capo isolato (`LF`)
  restava verde. L'ha trovato la falsificazione, non la rilettura.
- **L'eco dell'API può passare per parola nostra.** Il collaudo del modello ritirato
  chiedeva che il messaggio dicesse *quale* modello: falsificando la funzione che lo
  nomina, restava verde — perché quell'identificativo compariva anche nel corpo d'errore
  che l'API rimanda indietro, e il collaudo lo leggeva lì. Adesso il corpo finto **non**
  nomina il modello, e la prova riguarda solo quel che scriviamo noi. *(2026-08-27.)*
- **Il metro sbagliato tiene il collaudo verde meglio di qualunque bug.** La prova che il
  livello 6 ha il fondo più scuro del 5 era scritta con `Color.GetBrightness()`, che è la
  luminosità **HSL**: per lei `#FA0825` (il rosso del marchio) è più scuro di `#DC3545`,
  mentre l'occhio e WCAG vedono l'opposto — quel rosso ha il canale rosso quasi al massimo,
  e la luminanza percepita lo conta per il 21% mentre l'HSL guarda solo il canale più alto e
  il più basso. Rimettendo il rosso del marchio al livello 6 il collaudo restava **verde**:
  misurava una proprietà vera di un colore, ma non quella che la regola nomina. Adesso si
  misura quanto bianco ci si legge sopra, cioè con lo stesso metro con cui il capitolo 03.2
  scrive tutti i suoi numeri. *(2026-09-01.)*
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
