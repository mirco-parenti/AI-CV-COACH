Imports System.IO
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace NonRegressione

    ''' <summary>
    ''' Il collaudo di tappa di T3 (cap. 14) nella sua parte automatizzabile: un
    ''' <b>CV vero</b> in PDF percorre l'import della nuova app e quello del prototipo,
    ''' e i due profili si mettono a confronto. È il gemello di
    ''' <see cref="CollaudiConfrontoReale"/> sull'anello 1.
    ''' </summary>
    ''' <remarks>
    ''' <para>I due passi dell'import si collaudano in modo diverso, perché diverso è
    ''' quello che si può pretendere. Il <b>passo 1</b> — la trascrizione del PDF — gira
    ''' da entrambe le parti e i due testi si misurano riga per riga: sono due chiamate
    ''' all'AI, non verranno identici, ma devono dire la stessa cosa. Il <b>passo 2</b> —
    ''' la strutturazione — riceve invece da entrambe le parti <b>lo stesso identico
    ''' testo</b>, quello trascritto dall'app: così l'ingresso è pari e la differenza che
    ''' resta è solo del modello, non della catena che ci arriva. È lo stesso principio
    ''' della parità della richiesta di T2, applicato a due passi in fila.</para>
    ''' <para>Il pass/fail netto sta sui campi che il CV scrive nero su bianco e che il
    ''' prompt ordina di copiare — nome, email, telefono, link, patente: lì una differenza
    ''' fra i due non è varianza dell'AI, è un difetto da guardare. Fa eccezione la
    ''' <b>città</b>, che si guarda e non si boccia finché il prompt non deciderà fra
    ''' residenza e domicilio (<see cref="CollaudoReale.PerchePerLaCitta"/>): tenerci un
    ''' Assert dava un collaudo che lampeggiava. Sui conteggi delle sezioni vale
    ''' una tolleranza dichiarata (<see cref="TolleranzaVoci"/>), perché dividere o unire
    ''' una voce è un giudizio; sul resto — <c>cosa_facevo</c>, le competenze, il numero
    ''' delle esperienze informali — non si pretende nulla, e a leggere è Mirco nel
    ''' rapporto.</para>
    ''' <para>Dove invece si è stretto è la <b>collocazione</b>, che è la domanda vera di
    ''' questo collaudo: un lavoro fra le formali, un volontariato fra le informali, e mai
    ''' la stessa attività in tutt'e due. Quest'ultima — la stessa cosa contata due volte,
    ''' che nel confronto con un annuncio peserebbe doppio — dal <b>pool 1.01</b> è un
    ''' Assert <b>per la sola app</b>, a cui <c>importa_cv</c> ora dice che un'attività sta
    ''' in una sezione sola. I doppioni del prototipo restano nel rapporto: lui quella
    ''' regola non ce l'ha, ed è il primo punto in cui l'app non lo imita ma lo supera
    ''' (vedi <see cref="ControlloDoppioni"/> e il CHANGELOG del pool).</para>
    ''' <para>La parte che il piano non chiedeva: l'<b>anti-invenzione diventa un
    ''' Assert</b>. I valori che il profilo copia dal CV devono comparire nel testo di
    ''' partenza; se un nome d'azienda o d'istituto non c'è, il collaudo si ferma. Vale
    ''' per entrambi i lati. Il rilevatore è quello condiviso di
    ''' <see cref="CollaudoReale"/>, uno solo per tutti i collaudi reali.</para>
    ''' <para><b>Il CV è un dato personale e non sta nel repo</b>: si cerca nella cartella
    ''' indicata da <c>CV_DI_PROVA</c>, e il rapporto si scrive lì accanto — non in
    ''' <c>casi/reale/</c> come quello di T2, che invece nasceva da casi inventati.
    ''' Senza la variabile, o senza chiave, o col prototipo spento, il collaudo si
    ''' dichiara <i>inconcludente</i> invece di fallire: sull'altra postazione niente di
    ''' tutto questo c'è.</para>
    ''' <para>Categoria <b>Reale</b>: si lancia a mano, da <c>VB.NET/src</c>, con
    ''' <c>dotnet test --settings TrovaLavoro.Collaudi/collaudi-reali.runsettings</c>, col
    ''' prototipo acceso (<c>npm start</c> dentro <c>HTML+JS/</c>). Da WSL le due
    ''' variabili arrivano all'eseguibile Windows solo se elencate in <c>WSLENV</c>:
    ''' <c>WSLENV=ANTHROPIC_API_KEY:CV_DI_PROVA/p</c>.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiImportReale

        ''' <summary>Passo 1 del prototipo: il PDF in base64, torna il testo trascritto.</summary>
        Private Const PrototipoLeggiPdf As String = "http://localhost:3000/leggi-pdf"

        ''' <summary>Passo 2 del prototipo: un turno del profilo, torna il frammento JSON.</summary>
        Private Const PrototipoStruttura As String = "http://localhost:3000/struttura"

        ''' <summary>Il rapporto, scritto accanto al CV perché contiene dati personali.</summary>
        Private Const NomeRapporto As String = "rapporto-collaudo-T3-import.md"

        ''' <summary>
        ''' Quanto devono somigliarsi le due trascrizioni, contate per righe non vuote in
        ''' comune. Non 100%: due letture dello stesso PDF possono spezzare una riga in
        ''' modo diverso. Ma sotto questa soglia una delle due sta perdendo pezzi di CV.
        ''' </summary>
        Private Const SomiglianzaMinima As Double = 0.95

        ''' <summary>
        ''' Di quante voci possono differire le sezioni <b>che il CV nomina una per una</b>
        ''' — esperienze formali e formazione. Una: unire due righe in un'esperienza sola,
        ''' o dividerle, è un giudizio legittimo del modello; due voci di scarto sono
        ''' invece un pezzo di CV letto diversamente.
        ''' </summary>
        ''' <remarks>
        ''' Sulle <b>esperienze informali</b> non si applica: quella sezione il CV non la
        ''' nomina una per una — nasce da un racconto, e distillarlo in voci è un giudizio
        ''' come per le competenze (<see cref="CollaudoReale.PerchePerLeInformali"/>).
        ''' Sulle <b>competenze</b> nemmeno, e non per comodità:
        ''' è misurata. Lo stesso testo, con lo stesso prompt (identico carattere per
        ''' carattere fra pool e prototipo) e lo stesso modello, in cinque strutturazioni
        ''' ha dato 18, 20, 20, 20 e 21 competenze — perché quella è l'unica lista che il
        ''' modello <i>distilla</i> invece di copiare, e quante competenze stiano in un CV
        ''' non ha una risposta giusta. Pretendere un numero lì sarebbe un collaudo che
        ''' lampeggia a caso (cap. 14, la lezione di T2). Si verifica quindi che nessuna
        ''' delle due liste sia vuota, e il resto si legge nel rapporto: quante competenze
        ''' hanno in comune e quali ha una parte sola.
        ''' </remarks>
        Friend Const TolleranzaVoci As Integer = 1

        ''' <summary>Sotto quanti caratteri la trascrizione non è il testo di un CV.</summary>
        Friend Const CaratteriMinimi As Integer = 1000

        <TestMethod, TestCategory("Reale")>
        Public Async Function IlProfiloDalCvVeroReggeIlConfrontoConIlPrototipo() As Task

            Dim pdf As String = CollaudoReale.CvInPdfOppureRinuncia()
            Dim chiave As String = CollaudoReale.ChiaveOppureRinuncia()

            Dim libreria As LibreriaPrompt = CollaudoReale.PoolIntegrato()

            Using client As New ClientClaude(chiave)

                Dim strutturatore As New StrutturatoreTurni(libreria, client)
                Dim trascrittore As New TrascrittorePdf(libreria, client)

                ' --- Passo 1: la trascrizione, dalle due parti. -----------------------
                Dim testoApp As String = Await trascrittore.TrascriviAsync(pdf)
                Dim testoPrototipo As String = Await TrascriviDalPrototipoAsync(pdf)

                Dim trascrizioni As CollaudoReale.ConfrontoElenchi =
                    CollaudoReale.ConfrontaTesti(testoApp, testoPrototipo)

                ' --- Passo 2: lo stesso testo alle due strutturazioni. ----------------
                ' L'ingresso è quello dell'app da entrambe le parti: è ciò che rende il
                ' confronto sul profilo un confronto sul solo passo 2.
                Dim daApp As Profilo =
                    (Await New ImportProfilo(strutturatore).DaTestoAsync(
                        testoApp, Path.GetFileName(pdf))).Profilo

                Dim frammento As JsonObject = Await StrutturaDalPrototipoAsync(
                    ImportProfilo.TurnoImport, testoApp)

                ' Il nome «Profilo» va qualificato per intero: nel contesto di un metodo
                ' che maneggia anche JsonNode, VB lo risolverebbe altrimenti come il nodo.
                Dim daPrototipo As Profilo = TrovaLavoro.Dati.Profilo.DaJson(frammento)

                ' Le competenze non si contano, si guardano: il rapporto dice quante ne
                ' hanno in comune e quali ha una parte sola (vedi TolleranzaVoci).
                Dim competenze As CollaudoReale.ConfrontoElenchi =
                    CollaudoReale.ConfrontaCompetenze(daApp, daPrototipo)

                ' --- L'anti-invenzione, sul testo da cui entrambi sono partiti. -------
                Dim inventateApp As CollaudoReale.Invenzioni =
                    CollaudoReale.ValoriFuoriDalTesto(daApp, testoApp)
                Dim inventatePrototipo As CollaudoReale.Invenzioni =
                    CollaudoReale.ValoriFuoriDalTesto(daPrototipo, testoApp)

                ' --- La stessa cosa contata due volte (vedi ControlloDoppioni). --------
                ' Segnalata, non asserita: il doppione nasce da un caso di confine che il
                ' prompt — lo stesso da entrambe le parti — non decide, quindi non è una
                ' regressione dell'app. Diventa un Assert quando il prompt avrà deciso.
                Dim doppioniApp As List(Of String) = ControlloDoppioni.Trova(daApp)
                Dim doppioniPrototipo As List(Of String) = ControlloDoppioni.Trova(daPrototipo)

                ' --- E il suo gemello: la sezione sbagliata (vedi ControlloCollocazione).
                Dim malCollocateApp As List(Of String) =
                    ControlloCollocazione.VolontariatoFraLeFormali(daApp)
                Dim malCollocatePrototipo As List(Of String) =
                    ControlloCollocazione.VolontariatoFraLeFormali(daPrototipo)

                ' Il rapporto si scrive PRIMA di giudicare: se un Assert ferma tutto, la
                ' prova di com'era andata resta su disco comunque.
                Dim dove As String = Scrivi(pdf, testoApp, testoPrototipo, trascrizioni, competenze,
                                            daApp, daPrototipo, inventateApp, inventatePrototipo,
                                            doppioniApp, doppioniPrototipo,
                                            malCollocateApp, malCollocatePrototipo)

                ' A console vanno solo i numeri, non il contenuto: l'esecutore dei
                ' collaudi lascia i suoi file dentro il repo (TestResults), e il CV di una
                ' persona vera non deve poter arrivare fin lì nemmeno per errore.
                Console.WriteLine(Riassunto(dove, trascrizioni, competenze, daApp, daPrototipo,
                                            inventateApp, inventatePrototipo,
                                            doppioniApp, doppioniPrototipo,
                                            malCollocateApp, malCollocatePrototipo))

                ' --- Passo 1: le due trascrizioni dicono la stessa cosa? --------------
                Assert.IsGreaterThan(CaratteriMinimi, testoApp.Length,
                    "dal PDF l'app deve ricavare il testo di un CV, non poche righe")
                Assert.IsGreaterThan(CaratteriMinimi, testoPrototipo.Length,
                    "e il prototipo lo stesso")

                Assert.IsGreaterThanOrEqualTo(SomiglianzaMinima, trascrizioni.Frazione,
                    $"le due trascrizioni hanno in comune solo il {trascrizioni.Frazione * 100:F1}% " &
                    $"delle righe (app {trascrizioni.VociPrimo}, prototipo {trascrizioni.VociSecondo}): " &
                    $"una delle due sta perdendo pezzi di CV. I dettagli nel rapporto.")

                ' --- Passo 2: i campi che si copiano devono coincidere. ---------------
                VerificaStessoValore("nome", daPrototipo.Nome, daApp.Nome)
                VerificaStessoValore("contatti.email", daPrototipo.Contatti.Email, daApp.Contatti.Email)
                VerificaStessoValore("contatti.telefono", daPrototipo.Contatti.Telefono, daApp.Contatti.Telefono)
                VerificaStessoValore("contatti.link", daPrototipo.Contatti.Link, daApp.Contatti.Link)

                ' contatti.citta manca da questo elenco di proposito, ed è l'unico dei
                ' campi copiati a mancarne: vedi CollaudoReale.PerchePerLaCitta.
                VerificaStessoValore("patente.ha", daPrototipo.Patente.Ha, daApp.Patente.Ha)
                VerificaStessoValore("patente.categorie",
                                     CollaudoReale.Categorie(daPrototipo), CollaudoReale.Categorie(daApp))

                ' --- Passo 2: quante voci per sezione, entro la tolleranza. -----------
                ' Le competenze restano fuori di proposito: vedi TolleranzaVoci. Le
                ' esperienze informali pure, e per la stessa ragione: vedi
                ' CollaudoReale.PerchePerLeInformali. Di quella sezione qui sotto si
                ' boccia ciò che è sempre sbagliato — la stessa attività contata due volte.
                VerificaConteggio("esperienze_formali",
                                  daPrototipo.EsperienzeFormali.Count, daApp.EsperienzeFormali.Count)
                VerificaConteggio("formazione",
                                  daPrototipo.Formazione.Count, daApp.Formazione.Count)

                ' Un profilo con le sezioni vuote passerebbe tutti i confronti qui sopra:
                ' il CV vero ha un nome, un recapito, un lavoro e uno studio, e vanno
                ' ritrovati da entrambe le parti.
                VerificaProfiloPieno("app", daApp)
                VerificaProfiloPieno("prototipo", daPrototipo)

                ' --- L'anti-invenzione. ----------------------------------------------
                Assert.IsEmpty(inventateApp.Gravi,
                    "l'app ha messo nel profilo valori che nel CV non ci sono: " &
                    String.Join(" · ", inventateApp.Gravi))
                Assert.IsEmpty(inventatePrototipo.Gravi,
                    "il prototipo ha messo nel profilo valori che nel CV non ci sono: " &
                    String.Join(" · ", inventatePrototipo.Gravi))

                ' --- La stessa cosa contata due volte, dalla sola parte dell'app. ------
                ' Il pool 1.01 dice che un'attività sta in una sezione sola: per l'app
                ' questo è un difetto. I doppioni del prototipo restano nel rapporto e non
                ' qui — quella regola lui non ce l'ha, ed è il punto in cui l'app fa meglio.
                Assert.IsEmpty(doppioniApp,
                    "l'app ha contato la stessa attività due volte, fra le esperienze " &
                    "formali e fra le informali: " & String.Join(" · ", doppioniApp))

                ' --- E la sezione giusta: un volontariato non è un impiego. ------------
                ' Senza questo, il collaudo vedrebbe sparire il doppione e direbbe verde
                ' anche quando il modello ha risolto l'ambiguità dalla parte sbagliata.
                Assert.IsEmpty(malCollocateApp,
                    "l'app ha messo fra i lavori un'attività che si dichiara volontaria: " &
                    String.Join(" · ", malCollocateApp))

            End Using

        End Function

        ' --- Il rilevatore di doppioni, provato senza rete --------------------------
        ' Il collaudo qui sopra vuole chiave, prototipo e un CV vero: la logica che gli
        ' serve si prova invece su profili finti, come tutto il resto della batteria.
        ' I dati sono inventati per la stessa ragione di T2 — il repository è pubblico, e
        ' un CV vero non renderebbe il collaudo più solido, solo più esposto — ma la forma
        ' è quella del caso vero che il collaudo di tappa ha pescato.

        ''' <summary>L'associazione di volontariato dei casi finti.</summary>
        Private Const Associazione As String = "Pubblica Assistenza Vallemare"

        <TestMethod>
        Public Sub UnaVoceInDueSezioniSiVede()

            ' La forma del caso vero: un volontariato scritto due volte, e una delle due
            ' con l'associazione nel campo «azienda», come se fosse un datore di lavoro.
            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Soccorritore volontario", .Azienda = Associazione})
            profilo.EsperienzeInformali.Add(New EsperienzaInformale With {
                .CosaFacevo = "Volontariato continuativo nel soccorso sanitario",
                .ConChi = Associazione})

            Dim doppioni As List(Of String) = ControlloDoppioni.Trova(profilo)

            Assert.HasCount(1, doppioni, "il doppione")
            Assert.Contains(Associazione, doppioni(0),
                            "chi legge deve capire di quale voce si parla")

        End Sub

        <TestMethod>
        Public Sub LOrganizzazioneSiAppaiaAMenoDiSpaziEMaiuscole()

            ' Il modello scrive lo stesso nome in due modi nelle due sezioni: se
            ' l'appaiamento fosse letterale, il doppione passerebbe liscio.
            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Volontario", .Azienda = "PUBBLICA  ASSISTENZA   vallemare"})
            profilo.EsperienzeInformali.Add(New EsperienzaInformale With {
                .CosaFacevo = "Soccorso", .ConChi = Associazione})

            Assert.HasCount(1, ControlloDoppioni.Trova(profilo), "il doppione")

        End Sub

        <TestMethod>
        Public Sub DueOrganizzazioniDiverseNonSonoUnDoppione()

            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Magazziniere", .Azienda = "Logistica Rossi"})
            profilo.EsperienzeInformali.Add(New EsperienzaInformale With {
                .CosaFacevo = "Soccorso", .ConChi = Associazione})

            Assert.IsEmpty(ControlloDoppioni.Trova(profilo),
                           "un lavoro e un volontariato distinti non sono la stessa cosa")

        End Sub

        <TestMethod>
        Public Sub SenzaOrganizzazioneIlDoppioneNonSiPuoVedere()

            ' Il limite dichiarato del rilevatore, scritto qui perché non si scopra un
            ' giorno per caso: senza il nome dell'organizzazione le due voci non hanno
            ' nessun campo confrontabile, e i testi il modello li riformula.
            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Soccorritore volontario", .Azienda = ""})
            profilo.EsperienzeInformali.Add(New EsperienzaInformale With {
                .CosaFacevo = "Facevo il soccorritore volontario", .ConChi = ""})

            Assert.IsEmpty(ControlloDoppioni.Trova(profilo),
                           "senza organizzazione non c'è niente da appaiare")

        End Sub

        ' --- Il volontariato promosso a impiego, provato senza rete -----------------
        ' Il difetto gemello, e la ragione per cui non basta il rilevatore qui sopra:
        ' quando il doppione sparisce perché il modello sceglie una sezione sola, può
        ' sceglierla sbagliata — e allora non c'è nessun doppione da vedere.

        <TestMethod>
        Public Sub UnVolontariatoFraLeFormaliSiVede()

            ' La forma del caso vero: la voce sta in una sezione sola, ma è quella dei
            ' lavori, con l'associazione al posto del datore di lavoro.
            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Soccorritore paramedico volontario", .Azienda = Associazione})

            Dim malCollocate As List(Of String) = ControlloCollocazione.VolontariatoFraLeFormali(profilo)

            Assert.HasCount(1, malCollocate, "la voce mal collocata")
            Assert.Contains(Associazione, malCollocate(0),
                            "chi legge deve capire di quale voce si parla")

        End Sub

        <TestMethod>
        Public Sub LaParolaSiRiconosceAncheInCiòCheFaceva()

            ' Il ruolo può non dirlo: «Soccorritore» e basta. Lo dice la descrizione.
            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Soccorritore", .Azienda = Associazione,
                .CosaFacevo = "Servizio di volontariato sulle ambulanze"})

            Assert.HasCount(1, ControlloCollocazione.VolontariatoFraLeFormali(profilo),
                            "la voce mal collocata")

        End Sub

        <TestMethod>
        Public Sub UnVolontariatoFraLeInformaliStaDoveDeve()

            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Magazziniere", .Azienda = "Logistica Rossi"})
            profilo.EsperienzeInformali.Add(New EsperienzaInformale With {
                .CosaFacevo = "Volontariato nel soccorso sanitario", .ConChi = Associazione})

            Assert.IsEmpty(ControlloCollocazione.VolontariatoFraLeFormali(profilo),
                           "lì il volontariato ci sta di diritto")

        End Sub

        <TestMethod>
        Public Sub CoordinareIVolontariEUnLavoroVero()

            ' Il limite dichiarato del rilevatore, dalla parte buona: il plurale resta
            ' fuori dalle parole cercate apposta, perché «coordinatore volontari» è un
            ' impiego e non un volontariato.
            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Coordinatore volontari", .Azienda = Associazione,
                .CosaFacevo = "Turni e formazione dei volontari"})

            Assert.IsEmpty(ControlloCollocazione.VolontariatoFraLeFormali(profilo),
                           "coordinare i volontari è un lavoro, non un volontariato")

        End Sub

        <TestMethod>
        Public Sub IlNomeDellAssociazioneDaSoloNonBasta()

            ' Un impiego retribuito dentro un'associazione di volontariato resta un
            ' impiego: il rilevatore guarda il ruolo e la descrizione, mai l'azienda.
            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Impiegato amministrativo",
                .Azienda = "Associazione Volontariato Vallemare",
                .CosaFacevo = "Contabilità e segreteria"})

            Assert.IsEmpty(ControlloCollocazione.VolontariatoFraLeFormali(profilo),
                           "il nome del datore non dice la natura dell'attività")

        End Sub

        ' --- Il segno delle righe di rapporto, provato senza rete -------------------
        ' Il caso interessante — la differenza che si guarda invece di bocciare — dipende
        ' da come il modello legge un CV con due indirizzi, e nelle tre esecuzioni del
        ' collaudo di tappa non si è presentato nemmeno una volta. Provarlo qui costa
        ' nulla ed è l'unico modo di sapere che quando servirà sarà giusto.

        <TestMethod>
        Public Sub ILValoriUgualiPortanoIlSegnoDiUguaglianza()

            Assert.AreEqual("=", CollaudoReale.Marcatore(uguali:=True, segnalato:=False),
                            "due valori uguali")
            Assert.AreEqual("=", CollaudoReale.Marcatore(uguali:=True, segnalato:=True),
                            "e restano uguali anche se il campo è di quelli che si guardano")

        End Sub

        <TestMethod>
        Public Sub UnaDifferenzaSiVedeEQuellaDaGuardareSiDistingue()

            Assert.AreEqual("**≠**", CollaudoReale.Marcatore(uguali:=False, segnalato:=False),
                            "una differenza che boccia")
            Assert.AreEqual("⚠️", CollaudoReale.Marcatore(uguali:=False, segnalato:=True),
                            "una differenza da guardare — oggi la città")

        End Sub

        ' --- I giudizi, uno per genere ---------------------------------------------

        ''' <summary>
        ''' Un campo che il prompt ordina di copiare dal CV: fra i due lati deve essere
        ''' lo stesso. Si confronta ripulito dagli spazi di troppo — quelli sono
        ''' impaginazione, non contenuto — ma non dalle maiuscole, che in un nome
        ''' contano.
        ''' </summary>
        Private Shared Sub VerificaStessoValore(campo As String, prototipo As String, app As String)

            Assert.AreEqual(CollaudoReale.Ripulito(prototipo), CollaudoReale.Ripulito(app),
                $"{campo}: il prototipo dice «{CollaudoReale.Ripulito(prototipo)}», " &
                $"l'app «{CollaudoReale.Ripulito(app)}»")

        End Sub

        ''' <summary>Quante voci ha una sezione dalle due parti, entro la tolleranza.</summary>
        Private Shared Sub VerificaConteggio(sezione As String, prototipo As Integer, app As Integer)

            Assert.IsLessThanOrEqualTo(TolleranzaVoci, Math.Abs(prototipo - app),
                $"{sezione}: {prototipo} voci nel prototipo, {app} nell'app — " &
                $"più di {TolleranzaVoci} di scarto non è un giudizio diverso, è un pezzo di CV letto diversamente")

        End Sub

        ''' <summary>Le parti che un CV vero ha per certo, e che devono esserci.</summary>
        Friend Shared Sub VerificaProfiloPieno(lato As String, profilo As Profilo)

            Assert.IsNotEmpty(profilo.Nome, $"[{lato}] il nome")
            Assert.IsNotEmpty(profilo.Contatti.Email, $"[{lato}] un recapito")
            Assert.IsGreaterThan(0, profilo.EsperienzeFormali.Count, $"[{lato}] almeno un'esperienza")
            Assert.IsGreaterThan(0, profilo.Formazione.Count, $"[{lato}] almeno un titolo di studio")

            ' Sulle competenze il conteggio non si pretende, ma una lista vuota è un'altra
            ' cosa: vorrebbe dire che il turno non ha distillato nulla da un CV intero.
            Assert.IsGreaterThan(0, profilo.Competenze.Count, $"[{lato}] almeno una competenza")

        End Sub

        ' --- Le due porte del prototipo --------------------------------------------

        ''' <summary>Il passo 1 come lo fa il prototipo: <c>POST /leggi-pdf</c>.</summary>
        Private Shared Async Function TrascriviDalPrototipoAsync(pdf As String) As Task(Of String)

            Dim richiesta As New JsonObject From {
                {"pdf_base64", Convert.ToBase64String(File.ReadAllBytes(pdf))}}

            Dim risposta As JsonObject = Await ChiediAlPrototipoAsync(
                PrototipoLeggiPdf, richiesta, "la trascrizione del PDF")

            Return If(TryCast(risposta("testo"), JsonValue)?.ToString(), String.Empty)

        End Function

        ''' <summary>Il passo 2 come lo fa il prototipo: <c>POST /struttura</c>.</summary>
        Private Shared Async Function StrutturaDalPrototipoAsync(turno As String,
                                                                 testo As String) As Task(Of JsonObject)

            Dim richiesta As New JsonObject From {
                {"turno", turno},
                {"risposta", testo}}

            Return Await ChiediAlPrototipoAsync(
                PrototipoStruttura, richiesta, $"la strutturazione del turno «{turno}»")

        End Function

        ''' <summary>
        ''' Una domanda al prototipo. Se non risponde non è un difetto della nuova app:
        ''' il collaudo rinuncia dicendo come si accende.
        ''' </summary>
        Private Shared Async Function ChiediAlPrototipoAsync(indirizzo As String, richiesta As JsonObject,
                                                             cosa As String) As Task(Of JsonObject)

            Using http As New HttpClient()

                ' La trascrizione di un PDF è la chiamata più lenta di tutto il progetto:
                ' il PDF viaggia intero e il modello lo legge pagina per pagina.
                http.Timeout = TimeSpan.FromMinutes(3)

                Dim risposta As HttpResponseMessage
                Try
                    risposta = Await http.PostAsync(indirizzo,
                        New StringContent(richiesta.ToJsonString(), New UTF8Encoding(False), "application/json"))
                Catch ex As HttpRequestException
                    Assert.Inconclusive(
                        $"Il prototipo non risponde su {indirizzo}: avvialo con «npm start» dentro " &
                        $"HTML+JS/ (la chiave sta nel suo .env). {ex.Message}")
                    Return Nothing
                End Try

                Using risposta

                    Dim corpo As String = Await risposta.Content.ReadAsStringAsync()

                    Assert.IsTrue(risposta.IsSuccessStatusCode,
                        $"il prototipo ha risposto HTTP {CInt(risposta.StatusCode)} per {cosa}: {corpo}")

                    Dim oggetto As JsonObject = TryCast(JsonNode.Parse(corpo), JsonObject)
                    Assert.IsNotNull(oggetto, $"il prototipo non ha restituito un oggetto JSON per {cosa}")

                    Return oggetto

                End Using
            End Using

        End Function

        ' --- Il rapporto ------------------------------------------------------------

        ''' <summary>
        ''' Il rapporto da leggere a mano, scritto <b>accanto al CV</b> e non nel repo:
        ''' dentro ci sono i dati personali di chi ha scritto quel curriculum. Contiene
        ''' ciò che nessun Assert può giudicare — quanto si somigliano le due
        ''' trascrizioni, come ognuna delle due parti ha riformulato le stesse frasi.
        ''' </summary>
        ''' <returns>Dove è stato scritto, che è l'unica cosa che si può dire a console.</returns>
        Private Shared Function Scrivi(pdf As String, testoApp As String, testoPrototipo As String,
                                       trascrizioni As CollaudoReale.ConfrontoElenchi,
                                       competenze As CollaudoReale.ConfrontoElenchi,
                                       daApp As Profilo,
                                       daPrototipo As Profilo,
                                       inventateApp As CollaudoReale.Invenzioni,
                                       inventatePrototipo As CollaudoReale.Invenzioni,
                                       doppioniApp As List(Of String),
                                       doppioniPrototipo As List(Of String),
                                       malCollocateApp As List(Of String),
                                       malCollocatePrototipo As List(Of String)) As String

            Dim testo As New StringBuilder()

            testo.Append("# Collaudo di tappa T3 — l'import del CV vero").Append(vbLf).Append(vbLf)
            testo.Append("*Rapporto generato da `CollaudiImportReale`. Sta qui e non nel repo perché ").
                  Append("contiene dati personali.*").Append(vbLf).Append(vbLf)
            testo.Append($"- **CV**: `{Path.GetFileName(pdf)}`").Append(vbLf)
            testo.Append($"- **Quando**: {DateTime.Now:yyyy-MM-dd HH:mm}").Append(vbLf).Append(vbLf)

            testo.Append("## Passo 1 — le due trascrizioni").Append(vbLf).Append(vbLf)
            testo.Append("| | app | prototipo |").Append(vbLf)
            testo.Append("|---|---|---|").Append(vbLf)
            testo.Append($"| caratteri | {testoApp.Length} | {testoPrototipo.Length} |").Append(vbLf)
            testo.Append($"| righe significative | {trascrizioni.VociPrimo} | {trascrizioni.VociSecondo} |").Append(vbLf)
            testo.Append(vbLf)
            testo.Append($"Righe in comune: **{trascrizioni.InComune}** — ").
                  Append($"**{trascrizioni.Frazione * 100:F1}%** (soglia {SomiglianzaMinima * 100:F0}%).").
                  Append(vbLf).Append(vbLf)

            CollaudoReale.Elenco(testo, "Righe che ha solo l'app", trascrizioni.SoloPrimo)
            CollaudoReale.Elenco(testo, "Righe che ha solo il prototipo", trascrizioni.SoloSecondo)

            testo.Append("## Passo 2 — i due profili, dallo stesso testo").Append(vbLf).Append(vbLf)
            testo.Append("*Entrambi hanno ricevuto la trascrizione dell'app: ").
                  Append("qui si confronta la sola strutturazione.*").Append(vbLf).Append(vbLf)

            testo.Append("| campo | prototipo | app | |").Append(vbLf)
            testo.Append("|---|---|---|---|").Append(vbLf)
            Riga(testo, "nome", daPrototipo.Nome, daApp.Nome)
            Riga(testo, "email", daPrototipo.Contatti.Email, daApp.Contatti.Email)
            Riga(testo, "telefono", daPrototipo.Contatti.Telefono, daApp.Contatti.Telefono)
            Riga(testo, "città", daPrototipo.Contatti.Citta, daApp.Contatti.Citta, segnalato:=True)
            Riga(testo, "link", daPrototipo.Contatti.Link, daApp.Contatti.Link)
            Riga(testo, "patente", daPrototipo.Patente.Ha, daApp.Patente.Ha)
            Riga(testo, "categorie", CollaudoReale.Categorie(daPrototipo), CollaudoReale.Categorie(daApp))
            Riga(testo, "esperienze formali",
                 daPrototipo.EsperienzeFormali.Count.ToString(), daApp.EsperienzeFormali.Count.ToString())
            Riga(testo, "esperienze informali",
                 daPrototipo.EsperienzeInformali.Count.ToString(), daApp.EsperienzeInformali.Count.ToString(),
                 segnalato:=True)
            Riga(testo, "competenze",
                 daPrototipo.Competenze.Count.ToString(), daApp.Competenze.Count.ToString())
            Riga(testo, "formazione",
                 daPrototipo.Formazione.Count.ToString(), daApp.Formazione.Count.ToString())
            testo.Append(vbLf)
            testo.Append(CollaudoReale.PerchePerLaCitta).Append(vbLf).Append(vbLf)
            testo.Append(CollaudoReale.PerchePerLeInformali).Append(vbLf).Append(vbLf)

            testo.Append("### Le competenze, voce per voce").Append(vbLf).Append(vbLf)
            testo.Append("*Sul numero non c'è pass/fail: a testo identico il modello ne distilla ").
                  Append("ogni volta un po' di più o un po' di meno. Quello che conta è se le due ").
                  Append("parti si **contraddicono** o se una è solo meno completa.*").Append(vbLf).Append(vbLf)
            testo.Append($"In comune: **{competenze.InComune}** su ").
                  Append($"{competenze.VociSecondo} del prototipo e {competenze.VociPrimo} dell'app ").
                  Append($"(**{competenze.Frazione * 100:F1}%**).").Append(vbLf).Append(vbLf)

            CollaudoReale.Elenco(testo, "Competenze che ha solo l'app", competenze.SoloPrimo)
            CollaudoReale.Elenco(testo, "Competenze che ha solo il prototipo", competenze.SoloSecondo)

            testo.Append("## Anti-invenzione").Append(vbLf).Append(vbLf)
            testo.Append("*Valori del profilo cercati nel testo trascritto. ").
                  Append("`cosa_facevo` e le competenze restano fuori: il modello li riformula ").
                  Append("per mestiere, e a giudicarli sei tu qui sotto.*").Append(vbLf).Append(vbLf)
            CollaudoReale.Invenzione(testo, "app", inventateApp)
            CollaudoReale.Invenzione(testo, "prototipo", inventatePrototipo)

            testo.Append("## La stessa cosa contata due volte").Append(vbLf).Append(vbLf)
            testo.Append("*Un'attività che sta sia fra le esperienze formali sia fra le informali. ").
                  Append("Un doppione è sempre sbagliato — nel confronto con un annuncio quella ").
                  Append("esperienza peserebbe doppio — e la tolleranza sul conteggio delle voci ").
                  Append("se lo mangerebbe in silenzio. Per l'**app** è un ⛔ che ferma il ").
                  Append("collaudo: il pool 1.01 le dice che un'attività sta in una sezione sola. ").
                  Append("Per il **prototipo** resta un ⚠️ da leggere: quella regola non ce l'ha, ").
                  Append("ed è esattamente il punto in cui l'app fa meglio.*").
                  Append(vbLf).Append(vbLf)
            CollaudoReale.Doppioni(testo, "app", doppioniApp, bocciato:=True)
            CollaudoReale.Doppioni(testo, "prototipo", doppioniPrototipo)

            testo.Append("## Ogni attività nella sua sezione").Append(vbLf).Append(vbLf)
            testo.Append("*Il gemello del doppione, e il difetto che resta quando il doppione ").
                  Append("sparisce: la voce sta in una sezione sola, ma è quella sbagliata — un ").
                  Append("volontariato promosso a impiego. Il pool 1.01 dice che a decidere è la ").
                  Append("natura dell'attività, non la sezione del CV in cui è stampata: per l'**app** ").
                  Append("è un ⛔, per il **prototipo** un ⚠️ da leggere.*").Append(vbLf).Append(vbLf)
            CollaudoReale.Collocazione(testo, "app", malCollocateApp, bocciato:=True)
            CollaudoReale.Collocazione(testo, "prototipo", malCollocatePrototipo)

            testo.Append("## Da leggere a mano").Append(vbLf).Append(vbLf)
            testo.Append("I due profili per intero, così come sono usciti. Le descrizioni ").
                  Append("(`cosa_facevo`, competenze) sono la parte che nessun Assert giudica: ").
                  Append("vanno lette una per una, e ognuna deve poter essere ricondotta al CV.").
                  Append(vbLf).Append(vbLf)
            testo.Append("### Profilo dell'app").Append(vbLf).Append(vbLf)
            testo.Append("```json").Append(vbLf).Append(daApp.ComeTesto()).Append(vbLf).Append("```").Append(vbLf).Append(vbLf)
            testo.Append("### Profilo del prototipo").Append(vbLf).Append(vbLf)
            testo.Append("```json").Append(vbLf).Append(daPrototipo.ComeTesto()).Append(vbLf).Append("```").Append(vbLf).Append(vbLf)
            testo.Append("### Testo trascritto dall'app").Append(vbLf).Append(vbLf)
            testo.Append("```").Append(vbLf).Append(testoApp).Append(vbLf).Append("```").Append(vbLf)

            Dim dove As String = Path.Combine(Path.GetDirectoryName(pdf), NomeRapporto)

            File.WriteAllText(dove, testo.ToString(),
                              New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Return dove

        End Function

        ''' <summary>
        ''' Cosa si può dire a voce alta: i numeri e dove sta il rapporto. Il contenuto
        ''' del CV resta nel file, fuori dal repo.
        ''' </summary>
        Private Shared Function Riassunto(dove As String, trascrizioni As CollaudoReale.ConfrontoElenchi,
                                          competenze As CollaudoReale.ConfrontoElenchi,
                                          daApp As Profilo,
                                          daPrototipo As Profilo,
                                          inventateApp As CollaudoReale.Invenzioni,
                                          inventatePrototipo As CollaudoReale.Invenzioni,
                                          doppioniApp As List(Of String),
                                          doppioniPrototipo As List(Of String),
                                          malCollocateApp As List(Of String),
                                          malCollocatePrototipo As List(Of String)) As String

            Dim testo As New StringBuilder()

            testo.AppendLine("Collaudo di tappa T3 — import del CV vero, app contro prototipo.")
            testo.AppendLine($"Trascrizioni: {trascrizioni.VociPrimo} righe l'app, " &
                             $"{trascrizioni.VociSecondo} il prototipo, " &
                             $"{trascrizioni.InComune} in comune ({trascrizioni.Frazione * 100:F1}%).")
            testo.AppendLine($"Voci nel profilo (prototipo → app): " &
                             $"formali {daPrototipo.EsperienzeFormali.Count} → {daApp.EsperienzeFormali.Count}, " &
                             $"informali {daPrototipo.EsperienzeInformali.Count} → {daApp.EsperienzeInformali.Count}, " &
                             $"competenze {daPrototipo.Competenze.Count} → {daApp.Competenze.Count} " &
                             $"({competenze.InComune} in comune), " &
                             $"formazione {daPrototipo.Formazione.Count} → {daApp.Formazione.Count}.")
            If CollaudoReale.Ripulito(daPrototipo.Contatti.Citta) <>
               CollaudoReale.Ripulito(daApp.Contatti.Citta) Then
                testo.AppendLine(
                    $"Città diversa (segnalata, non bocciata: il prompt non decide fra " &
                    $"residenza e domicilio): prototipo «{CollaudoReale.Ripulito(daPrototipo.Contatti.Citta)}», " &
                    $"app «{CollaudoReale.Ripulito(daApp.Contatti.Citta)}».")
            End If

            testo.AppendLine($"Valori non ritrovati nel CV: app {inventateApp.Gravi.Count} gravi " &
                             $"e {inventateApp.DaGuardare.Count} da guardare, " &
                             $"prototipo {inventatePrototipo.Gravi.Count} gravi " &
                             $"e {inventatePrototipo.DaGuardare.Count} da guardare.")
            testo.AppendLine($"Voci contate due volte: app {doppioniApp.Count} (bocciano), " &
                             $"prototipo {doppioniPrototipo.Count} (da leggere).")
            testo.AppendLine($"Volontariati messi fra i lavori: app {malCollocateApp.Count} (bocciano), " &
                             $"prototipo {malCollocatePrototipo.Count} (da leggere).")
            testo.AppendLine($"Rapporto completo (contiene dati personali, fuori dal repo): {dove}")

            Return testo.ToString()

        End Function

        ''' <summary>Una riga della tabella di confronto, con il segno di ciò che non torna.</summary>
        ''' <param name="segnalato">
        ''' Il campo è di quelli che si guardano e non si bocciano: la differenza prende
        ''' il ⚠️ invece del ≠. È il caso della città
        ''' (<see cref="CollaudoReale.PerchePerLaCitta"/>) e del numero delle esperienze
        ''' informali (<see cref="CollaudoReale.PerchePerLeInformali"/>).
        ''' </param>
        Private Shared Sub Riga(testo As StringBuilder, campo As String, prototipo As String, app As String,
                                Optional segnalato As Boolean = False)

            Dim uguali As Boolean = CollaudoReale.Ripulito(prototipo) = CollaudoReale.Ripulito(app)

            testo.Append($"| {campo} | {CollaudoReale.PerTabella(prototipo)} | " &
                         $"{CollaudoReale.PerTabella(app)} | ").
                  Append(CollaudoReale.Marcatore(uguali, segnalato)).Append(" |").Append(vbLf)

        End Sub

    End Class

End Namespace
