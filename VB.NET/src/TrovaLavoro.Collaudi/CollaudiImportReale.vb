Imports System.IO
Imports System.Linq
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
    ''' prompt ordina di copiare — nome, recapiti, patente: lì una differenza fra i due
    ''' non è varianza dell'AI, è un difetto da guardare. Sui conteggi delle sezioni vale
    ''' una tolleranza dichiarata (<see cref="TolleranzaVoci"/>), perché dividere o unire
    ''' una voce è un giudizio; sul resto — <c>cosa_facevo</c>, le competenze — non si
    ''' pretende nulla, e a leggere è Mirco nel rapporto.</para>
    ''' <para>La parte che il piano non chiedeva: l'<b>anti-invenzione diventa un
    ''' Assert</b>. I valori che il profilo copia dal CV devono comparire nel testo di
    ''' partenza; se un nome d'azienda o d'istituto non c'è, il collaudo si ferma. Vale
    ''' per entrambi i lati.</para>
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

        ''' <summary>Dove sta il CV vero. Fuori dal repo, per forza.</summary>
        Private Const VariabileCartella As String = "CV_DI_PROVA"

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
        ''' — esperienze e formazione. Una: unire due righe in un'esperienza sola, o
        ''' dividerle, è un giudizio legittimo del modello; due voci di scarto sono invece
        ''' un pezzo di CV letto diversamente.
        ''' </summary>
        ''' <remarks>
        ''' Sulle <b>competenze</b> questa tolleranza non si applica, e non per comodità:
        ''' è misurata. Lo stesso testo, con lo stesso prompt (identico carattere per
        ''' carattere fra pool e prototipo) e lo stesso modello, in cinque strutturazioni
        ''' ha dato 18, 20, 20, 20 e 21 competenze — perché quella è l'unica lista che il
        ''' modello <i>distilla</i> invece di copiare, e quante competenze stiano in un CV
        ''' non ha una risposta giusta. Pretendere un numero lì sarebbe un collaudo che
        ''' lampeggia a caso (cap. 14, la lezione di T2). Si verifica quindi che nessuna
        ''' delle due liste sia vuota, e il resto si legge nel rapporto: quante competenze
        ''' hanno in comune e quali ha una parte sola.
        ''' </remarks>
        Private Const TolleranzaVoci As Integer = 1

        ''' <summary>Sotto quanti caratteri la trascrizione non è il testo di un CV.</summary>
        Private Const CaratteriMinimi As Integer = 1000

        ''' <summary>Quante righe di differenza mostrare nel rapporto, per lato.</summary>
        Private Const RigheDaMostrare As Integer = 12

        <TestMethod, TestCategory("Reale")>
        Public Async Function IlProfiloDalCvVeroReggeIlConfrontoConIlPrototipo() As Task

            Dim pdf As String = CvDiProvaOppureRinuncia()
            Dim chiave As String = ChiaveOppureRinuncia()

            ' Il pool integrato, per forza: se accanto all'eseguibile ce ne fosse uno
            ' esterno, il confronto misurerebbe di nascosto un altro prompt. Qui si vuole
            ' il Pool 1.00, quello migrato dal prototipo.
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Using client As New ClientClaude(chiave)

                Dim strutturatore As New StrutturatoreTurni(libreria, client)
                Dim trascrittore As New TrascrittorePdf(libreria, client)

                ' --- Passo 1: la trascrizione, dalle due parti. -----------------------
                Dim testoApp As String = Await trascrittore.TrascriviAsync(pdf)
                Dim testoPrototipo As String = Await TrascriviDalPrototipoAsync(pdf)

                Dim trascrizioni As ConfrontoElenchi = ConfrontaTesti(testoApp, testoPrototipo)

                ' --- Passo 2: lo stesso testo alle due strutturazioni. ----------------
                ' L'ingresso è quello dell'app da entrambe le parti: è ciò che rende il
                ' confronto sul profilo un confronto sul solo passo 2.
                Dim daApp As TrovaLavoro.Dati.Profilo =
                    (Await New ImportProfilo(strutturatore).DaTestoAsync(
                        testoApp, Path.GetFileName(pdf))).Profilo

                Dim daPrototipo As TrovaLavoro.Dati.Profilo = TrovaLavoro.Dati.Profilo.DaJson(
                    Await StrutturaDalPrototipoAsync(ImportProfilo.TurnoImport, testoApp))

                ' Le competenze non si contano, si guardano: il rapporto dice quante ne
                ' hanno in comune e quali ha una parte sola (vedi TolleranzaVoci).
                Dim competenze As ConfrontoElenchi = ConfrontaCompetenze(daApp, daPrototipo)

                ' --- L'anti-invenzione, sul testo da cui entrambi sono partiti. -------
                Dim inventateApp As Invenzioni = ValoriFuoriDalTesto(daApp, testoApp)
                Dim inventatePrototipo As Invenzioni = ValoriFuoriDalTesto(daPrototipo, testoApp)

                ' --- La stessa cosa contata due volte (vedi ControlloDoppioni). --------
                ' Segnalata, non asserita: il doppione nasce da un caso di confine che il
                ' prompt — lo stesso da entrambe le parti — non decide, quindi non è una
                ' regressione dell'app. Diventa un Assert quando il prompt avrà deciso.
                Dim doppioniApp As List(Of String) = ControlloDoppioni.Trova(daApp)
                Dim doppioniPrototipo As List(Of String) = ControlloDoppioni.Trova(daPrototipo)

                ' Il rapporto si scrive PRIMA di giudicare: se un Assert ferma tutto, la
                ' prova di com'era andata resta su disco comunque.
                Dim dove As String = Scrivi(pdf, testoApp, testoPrototipo, trascrizioni, competenze,
                                            daApp, daPrototipo, inventateApp, inventatePrototipo,
                                            doppioniApp, doppioniPrototipo)

                ' A console vanno solo i numeri, non il contenuto: l'esecutore dei
                ' collaudi lascia i suoi file dentro il repo (TestResults), e il CV di una
                ' persona vera non deve poter arrivare fin lì nemmeno per errore.
                Console.WriteLine(Riassunto(dove, trascrizioni, competenze, daApp, daPrototipo,
                                            inventateApp, inventatePrototipo,
                                            doppioniApp, doppioniPrototipo))

                ' --- Passo 1: le due trascrizioni dicono la stessa cosa? --------------
                Assert.IsGreaterThan(CaratteriMinimi, testoApp.Length,
                    "dal PDF l'app deve ricavare il testo di un CV, non poche righe")
                Assert.IsGreaterThan(CaratteriMinimi, testoPrototipo.Length,
                    "e il prototipo lo stesso")

                Assert.IsGreaterThanOrEqualTo(SomiglianzaMinima, trascrizioni.Frazione,
                    $"le due trascrizioni hanno in comune solo il {trascrizioni.Frazione * 100:F1}% " &
                    $"delle righe (app {trascrizioni.VociApp}, prototipo {trascrizioni.VociPrototipo}): " &
                    $"una delle due sta perdendo pezzi di CV. I dettagli nel rapporto.")

                ' --- Passo 2: i campi che si copiano devono coincidere. ---------------
                VerificaStessoValore("nome", daPrototipo.Nome, daApp.Nome)
                VerificaStessoValore("contatti.email", daPrototipo.Contatti.Email, daApp.Contatti.Email)
                VerificaStessoValore("contatti.telefono", daPrototipo.Contatti.Telefono, daApp.Contatti.Telefono)
                VerificaStessoValore("contatti.citta", daPrototipo.Contatti.Citta, daApp.Contatti.Citta)
                VerificaStessoValore("contatti.link", daPrototipo.Contatti.Link, daApp.Contatti.Link)
                VerificaStessoValore("patente.ha", daPrototipo.Patente.Ha, daApp.Patente.Ha)
                VerificaStessoValore("patente.categorie",
                                     Categorie(daPrototipo), Categorie(daApp))

                ' --- Passo 2: quante voci per sezione, entro la tolleranza. -----------
                ' Le competenze restano fuori di proposito: vedi TolleranzaVoci.
                VerificaConteggio("esperienze_formali",
                                  daPrototipo.EsperienzeFormali.Count, daApp.EsperienzeFormali.Count)
                VerificaConteggio("esperienze_informali",
                                  daPrototipo.EsperienzeInformali.Count, daApp.EsperienzeInformali.Count)
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
            Dim profilo As New TrovaLavoro.Dati.Profilo
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
            Dim profilo As New TrovaLavoro.Dati.Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Volontario", .Azienda = "PUBBLICA  ASSISTENZA   vallemare"})
            profilo.EsperienzeInformali.Add(New EsperienzaInformale With {
                .CosaFacevo = "Soccorso", .ConChi = Associazione})

            Assert.HasCount(1, ControlloDoppioni.Trova(profilo), "il doppione")

        End Sub

        <TestMethod>
        Public Sub DueOrganizzazioniDiverseNonSonoUnDoppione()

            Dim profilo As New TrovaLavoro.Dati.Profilo
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
            Dim profilo As New TrovaLavoro.Dati.Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Soccorritore volontario", .Azienda = ""})
            profilo.EsperienzeInformali.Add(New EsperienzaInformale With {
                .CosaFacevo = "Facevo il soccorritore volontario", .ConChi = ""})

            Assert.IsEmpty(ControlloDoppioni.Trova(profilo),
                           "senza organizzazione non c'è niente da appaiare")

        End Sub

        ' --- I giudizi, uno per genere ---------------------------------------------

        ''' <summary>
        ''' Un campo che il prompt ordina di copiare dal CV: fra i due lati deve essere
        ''' lo stesso. Si confronta ripulito dagli spazi di troppo — quelli sono
        ''' impaginazione, non contenuto — ma non dalle maiuscole, che in un nome
        ''' contano.
        ''' </summary>
        Private Shared Sub VerificaStessoValore(campo As String, prototipo As String, app As String)

            Assert.AreEqual(Ripulito(prototipo), Ripulito(app),
                $"{campo}: il prototipo dice «{Ripulito(prototipo)}», l'app «{Ripulito(app)}»")

        End Sub

        ''' <summary>Quante voci ha una sezione dalle due parti, entro la tolleranza.</summary>
        Private Shared Sub VerificaConteggio(sezione As String, prototipo As Integer, app As Integer)

            Assert.IsLessThanOrEqualTo(TolleranzaVoci, Math.Abs(prototipo - app),
                $"{sezione}: {prototipo} voci nel prototipo, {app} nell'app — " &
                $"più di {TolleranzaVoci} di scarto non è un giudizio diverso, è un pezzo di CV letto diversamente")

        End Sub

        ''' <summary>Le parti che un CV vero ha per certo, e che devono esserci.</summary>
        Private Shared Sub VerificaProfiloPieno(lato As String, profilo As TrovaLavoro.Dati.Profilo)

            Assert.IsNotEmpty(profilo.Nome, $"[{lato}] il nome")
            Assert.IsNotEmpty(profilo.Contatti.Email, $"[{lato}] un recapito")
            Assert.IsGreaterThan(0, profilo.EsperienzeFormali.Count, $"[{lato}] almeno un'esperienza")
            Assert.IsGreaterThan(0, profilo.Formazione.Count, $"[{lato}] almeno un titolo di studio")

            ' Sulle competenze il conteggio non si pretende, ma una lista vuota è un'altra
            ' cosa: vorrebbe dire che il turno non ha distillato nulla da un CV intero.
            Assert.IsGreaterThan(0, profilo.Competenze.Count, $"[{lato}] almeno una competenza")

        End Sub

        ''' <summary>Le categorie della patente, in una riga confrontabile.</summary>
        Private Shared Function Categorie(profilo As TrovaLavoro.Dati.Profilo) As String
            Return String.Join(", ", profilo.Patente.Categorie.Select(
                Function(c) Ripulito(c).ToUpperInvariant()))
        End Function

        ' --- Due elenchi messi a confronto -----------------------------------------

        ''' <summary>
        ''' Quanto si somigliano due elenchi di voci, e in che cosa differiscono. Serve
        ''' due volte: sulle righe delle trascrizioni (passo 1) e sulle competenze dei due
        ''' profili (passo 2), che sono la stessa domanda su cose diverse.
        ''' </summary>
        Private Class ConfrontoElenchi

            ''' <summary>Frazione di voci in comune sul lato che ne ha più.</summary>
            Public Property Frazione As Double

            Public Property VociApp As Integer
            Public Property VociPrototipo As Integer
            Public Property InComune As Integer

            ''' <summary>Voci che ha solo l'app, come le ha scritte lei.</summary>
            Public Property SoloApp As New List(Of String)

            ''' <summary>Voci che ha solo il prototipo.</summary>
            Public Property SoloPrototipo As New List(Of String)

        End Class

        ''' <summary>
        ''' Confronta due elenchi. Le voci si appaiano normalizzate negli spazi e nelle
        ''' maiuscole — si vuole sapere se una manca, non se è scritta in maiuscolo da una
        ''' parte sola — ma si mostrano come sono state scritte.
        ''' </summary>
        Private Shared Function ConfrontaElenchi(app As Dictionary(Of String, String),
                                                 prototipo As Dictionary(Of String, String)) As ConfrontoElenchi

            ' Il conteggio passa da Where: su un dizionario «Count» è la sua proprietà, e
            ' scritto con la condizione dentro non compilerebbe nemmeno.
            Dim inComune As Integer = app.Keys.Where(Function(v) prototipo.ContainsKey(v)).Count()
            Dim quante As Integer = Math.Max(app.Count, prototipo.Count)

            Return New ConfrontoElenchi With {
                .Frazione = If(quante = 0, 0.0, inComune / quante),
                .VociApp = app.Count,
                .VociPrototipo = prototipo.Count,
                .InComune = inComune,
                .SoloApp = app.Where(Function(v) Not prototipo.ContainsKey(v.Key)).
                               Select(Function(v) v.Value).Take(RigheDaMostrare).ToList(),
                .SoloPrototipo = prototipo.Where(Function(v) Not app.ContainsKey(v.Key)).
                                           Select(Function(v) v.Value).Take(RigheDaMostrare).ToList()}

        End Function

        ''' <summary>Le due trascrizioni, riga per riga.</summary>
        Private Shared Function ConfrontaTesti(app As String, prototipo As String) As ConfrontoElenchi
            Return ConfrontaElenchi(RigheSignificative(app), RigheSignificative(prototipo))
        End Function

        ''' <summary>Le competenze dei due profili, voce per voce.</summary>
        Private Shared Function ConfrontaCompetenze(app As TrovaLavoro.Dati.Profilo,
                                                    prototipo As TrovaLavoro.Dati.Profilo) As ConfrontoElenchi
            Return ConfrontaElenchi(Appaiabili(app.Competenze), Appaiabili(prototipo.Competenze))
        End Function

        ''' <summary>
        ''' Le righe di una trascrizione che vale la pena confrontare. Le righe di una
        ''' lettera o due — separatori, iniziali — non dicono niente.
        ''' </summary>
        Private Shared Function RigheSignificative(testo As String) As Dictionary(Of String, String)

            Return Appaiabili(If(testo, String.Empty).
                Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries), lunghezzaMinima:=3)

        End Function

        ''' <summary>
        ''' Un elenco pronto per l'appaiamento: dalla voce normalizzata (la chiave) a
        ''' quella da mostrare. La prima occorrenza è quella che si mostra — se una voce si
        ''' ripete, nel rapporto non serve vederla due volte.
        ''' </summary>
        Private Shared Function Appaiabili(voci As IEnumerable(Of String),
                                           Optional lunghezzaMinima As Integer = 1) As Dictionary(Of String, String)

            Dim elenco As New Dictionary(Of String, String)

            For Each voce As String In voci

                Dim chiave As String = Ripulito(voce).ToLowerInvariant()
                If chiave.Length < lunghezzaMinima Then Continue For

                If Not elenco.ContainsKey(chiave) Then elenco.Add(chiave, Ripulito(voce))

            Next

            Return elenco

        End Function

        ' --- L'anti-invenzione ------------------------------------------------------

        ''' <summary>
        ''' I valori del profilo che nel CV non si trovano, divisi per gravità.
        ''' </summary>
        Private Class Invenzioni

            ''' <summary>
            ''' Nomi propri e recapiti: il prompt ordina di copiarli, e se non stanno nel
            ''' testo di partenza sono inventati. Qui il collaudo si ferma.
            ''' </summary>
            Public Property Gravi As New List(Of String)

            ''' <summary>
            ''' Campi che il modello può legittimamente riformulare — una data resa
            ''' «2015-2018» invece di «set. 2015 – giu. 2018», un ruolo tradotto. Si
            ''' guardano nel rapporto, non fanno cadere il collaudo.
            ''' </summary>
            Public Property DaGuardare As New List(Of String)

        End Class

        ''' <summary>
        ''' Cerca nel testo del CV ogni valore che il profilo dovrebbe aver copiato da
        ''' lì. Restano fuori <c>cosa_facevo</c> e le competenze: quelli il modello li
        ''' riformula per mestiere, e giudicarli è compito di chi legge.
        ''' </summary>
        Private Shared Function ValoriFuoriDalTesto(profilo As TrovaLavoro.Dati.Profilo,
                                                    testo As String) As Invenzioni

            Dim pagliaio As String = PerCercare(testo)
            Dim senzaSpazi As String = pagliaio.Replace(" ", "")
            Dim fuori As New Invenzioni

            Cerca(fuori.Gravi, "nome", profilo.Nome, pagliaio, senzaSpazi)
            Cerca(fuori.Gravi, "contatti.email", profilo.Contatti.Email, pagliaio, senzaSpazi)

            Cerca(fuori.DaGuardare, "contatti.telefono", profilo.Contatti.Telefono, pagliaio, senzaSpazi)
            Cerca(fuori.DaGuardare, "contatti.citta", profilo.Contatti.Citta, pagliaio, senzaSpazi)
            Cerca(fuori.DaGuardare, "contatti.link", profilo.Contatti.Link, pagliaio, senzaSpazi)

            For Each categoria As String In profilo.Patente.Categorie
                Cerca(fuori.DaGuardare, "patente.categoria", categoria, pagliaio, senzaSpazi)
            Next

            For Each esperienza As EsperienzaFormale In profilo.EsperienzeFormali
                Cerca(fuori.Gravi, "esperienza.azienda", esperienza.Azienda, pagliaio, senzaSpazi)
                Cerca(fuori.DaGuardare, "esperienza.ruolo", esperienza.Ruolo, pagliaio, senzaSpazi)
                Cerca(fuori.DaGuardare, "esperienza.durata", esperienza.Durata, pagliaio, senzaSpazi)
            Next

            For Each voce As VoceFormazione In profilo.Formazione
                Cerca(fuori.Gravi, "formazione.istituto", voce.Istituto, pagliaio, senzaSpazi)
                Cerca(fuori.DaGuardare, "formazione.titolo", voce.Titolo, pagliaio, senzaSpazi)
                Cerca(fuori.DaGuardare, "formazione.anno", voce.Anno, pagliaio, senzaSpazi)
            Next

            Return fuori

        End Function

        ''' <summary>Aggiunge il valore all'elenco se nel CV non si trova.</summary>
        Private Shared Sub Cerca(dove As List(Of String), campo As String, valore As String,
                                 pagliaio As String, senzaSpazi As String)

            If String.IsNullOrWhiteSpace(valore) Then Return
            If NelTesto(valore, pagliaio, senzaSpazi) Then Return

            dove.Add($"{campo}: «{Ripulito(valore)}»")

        End Sub

        ''' <summary>
        ''' Se un valore si ritrova nel testo del CV. La prova è per parole e non per
        ''' stringa intera, altrimenti un «S.r.l.» diventato «Srl» sembrerebbe
        ''' un'invenzione: ogni parola dalle quattro lettere in su deve comparire nel
        ''' testo. Per i valori che di parole lunghe non ne hanno — una sigla, un anno —
        ''' si cerca il valore intero, ignorando spazi e punteggiatura: è così che un
        ''' telefono scritto «333 123 4567» si ritrova in «3331234567».
        ''' </summary>
        Private Shared Function NelTesto(valore As String, pagliaio As String, senzaSpazi As String) As Boolean

            Dim cercato As String = PerCercare(valore)
            If cercato.Length = 0 Then Return True

            Dim lunghe As String() = cercato.Split(" "c, StringSplitOptions.RemoveEmptyEntries).
                                            Where(Function(p) p.Length >= 4).ToArray()

            If lunghe.Length > 0 Then Return lunghe.All(Function(p) pagliaio.Contains(p))

            Return senzaSpazi.Contains(cercato.Replace(" ", ""))

        End Function

        ''' <summary>
        ''' Il testo pronto per cercarci dentro: tutto minuscolo, e tutto ciò che non è
        ''' lettera o cifra ridotto a un solo spazio. Gli accenti restano — sono lettere,
        ''' e in un CV italiano ci sono.
        ''' </summary>
        Private Shared Function PerCercare(testo As String) As String

            Dim costruito As New StringBuilder(If(testo, String.Empty).Length)

            For Each carattere As Char In If(testo, String.Empty).ToLowerInvariant()
                If Char.IsLetterOrDigit(carattere) Then
                    costruito.Append(carattere)
                ElseIf costruito.Length > 0 AndAlso costruito(costruito.Length - 1) <> " "c Then
                    costruito.Append(" "c)
                End If
            Next

            Return costruito.ToString().TrimEnd()

        End Function

        ''' <summary>Un valore senza gli spazi di troppo: quelli sono impaginazione.</summary>
        Private Shared Function Ripulito(valore As String) As String

            Return String.Join(" ", If(valore, String.Empty).
                Split({" "c, vbTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries))

        End Function

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

        ' --- Prerequisiti e rapporto ------------------------------------------------

        ''' <summary>
        ''' Il PDF del CV vero, dalla cartella indicata dall'ambiente. Senza, il collaudo
        ''' rinuncia: quel file non sta nel repo e non deve starci.
        ''' </summary>
        Private Shared Function CvDiProvaOppureRinuncia() As String

            Dim cartella As String = Environment.GetEnvironmentVariable(VariabileCartella)

            If String.IsNullOrWhiteSpace(cartella) OrElse Not Directory.Exists(cartella) Then
                Assert.Inconclusive(
                    $"Collaudo reale saltato: definisci {VariabileCartella} con la cartella che " &
                    "contiene il CV in PDF da importare.")
                Return Nothing
            End If

            Dim pdf As String = Directory.EnumerateFiles(cartella, "*.pdf").
                                          OrderBy(Function(f) f).FirstOrDefault()

            If pdf Is Nothing Then
                Assert.Inconclusive($"Collaudo reale saltato: in «{cartella}» non c'è nessun PDF.")
                Return Nothing
            End If

            Return pdf

        End Function

        ''' <summary>La chiave dall'ambiente; senza, il collaudo non fallisce: rinuncia.</summary>
        Private Shared Function ChiaveOppureRinuncia() As String

            Try
                Return ClientClaude.ChiaveDaAmbiente()
            Catch ex As ErroreAi
                Assert.Inconclusive(
                    "Collaudo reale saltato: manca la chiave. Definisci " &
                    ClientClaude.NomeVariabileChiave & " nell'ambiente prima di lanciare il banco.")
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Il rapporto da leggere a mano, scritto <b>accanto al CV</b> e non nel repo:
        ''' dentro ci sono i dati personali di chi ha scritto quel curriculum. Contiene
        ''' ciò che nessun Assert può giudicare — quanto si somigliano le due
        ''' trascrizioni, come ognuna delle due parti ha riformulato le stesse frasi.
        ''' </summary>
        ''' <returns>Dove è stato scritto, che è l'unica cosa che si può dire a console.</returns>
        Private Shared Function Scrivi(pdf As String, testoApp As String, testoPrototipo As String,
                                       trascrizioni As ConfrontoElenchi,
                                       competenze As ConfrontoElenchi,
                                       daApp As TrovaLavoro.Dati.Profilo,
                                       daPrototipo As TrovaLavoro.Dati.Profilo,
                                       inventateApp As Invenzioni,
                                       inventatePrototipo As Invenzioni,
                                       doppioniApp As List(Of String),
                                       doppioniPrototipo As List(Of String)) As String

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
            testo.Append($"| righe significative | {trascrizioni.VociApp} | {trascrizioni.VociPrototipo} |").Append(vbLf)
            testo.Append(vbLf)
            testo.Append($"Righe in comune: **{trascrizioni.InComune}** — ").
                  Append($"**{trascrizioni.Frazione * 100:F1}%** (soglia {SomiglianzaMinima * 100:F0}%).").
                  Append(vbLf).Append(vbLf)

            Elenco(testo, "Righe che ha solo l'app", trascrizioni.SoloApp)
            Elenco(testo, "Righe che ha solo il prototipo", trascrizioni.SoloPrototipo)

            testo.Append("## Passo 2 — i due profili, dallo stesso testo").Append(vbLf).Append(vbLf)
            testo.Append("*Entrambi hanno ricevuto la trascrizione dell'app: ").
                  Append("qui si confronta la sola strutturazione.*").Append(vbLf).Append(vbLf)

            testo.Append("| campo | prototipo | app | |").Append(vbLf)
            testo.Append("|---|---|---|---|").Append(vbLf)
            Riga(testo, "nome", daPrototipo.Nome, daApp.Nome)
            Riga(testo, "email", daPrototipo.Contatti.Email, daApp.Contatti.Email)
            Riga(testo, "telefono", daPrototipo.Contatti.Telefono, daApp.Contatti.Telefono)
            Riga(testo, "città", daPrototipo.Contatti.Citta, daApp.Contatti.Citta)
            Riga(testo, "link", daPrototipo.Contatti.Link, daApp.Contatti.Link)
            Riga(testo, "patente", daPrototipo.Patente.Ha, daApp.Patente.Ha)
            Riga(testo, "categorie", Categorie(daPrototipo), Categorie(daApp))
            Riga(testo, "esperienze formali",
                 daPrototipo.EsperienzeFormali.Count.ToString(), daApp.EsperienzeFormali.Count.ToString())
            Riga(testo, "esperienze informali",
                 daPrototipo.EsperienzeInformali.Count.ToString(), daApp.EsperienzeInformali.Count.ToString())
            Riga(testo, "competenze",
                 daPrototipo.Competenze.Count.ToString(), daApp.Competenze.Count.ToString())
            Riga(testo, "formazione",
                 daPrototipo.Formazione.Count.ToString(), daApp.Formazione.Count.ToString())
            testo.Append(vbLf)

            testo.Append("### Le competenze, voce per voce").Append(vbLf).Append(vbLf)
            testo.Append("*Sul numero non c'è pass/fail: a testo identico il modello ne distilla ").
                  Append("ogni volta un po' di più o un po' di meno. Quello che conta è se le due ").
                  Append("parti si **contraddicono** o se una è solo meno completa.*").Append(vbLf).Append(vbLf)
            testo.Append($"In comune: **{competenze.InComune}** su ").
                  Append($"{competenze.VociPrototipo} del prototipo e {competenze.VociApp} dell'app ").
                  Append($"(**{competenze.Frazione * 100:F1}%**).").Append(vbLf).Append(vbLf)

            Elenco(testo, "Competenze che ha solo l'app", competenze.SoloApp)
            Elenco(testo, "Competenze che ha solo il prototipo", competenze.SoloPrototipo)

            testo.Append("## Anti-invenzione").Append(vbLf).Append(vbLf)
            testo.Append("*Valori del profilo cercati nel testo trascritto. ").
                  Append("`cosa_facevo` e le competenze restano fuori: il modello li riformula ").
                  Append("per mestiere, e a giudicarli sei tu qui sotto.*").Append(vbLf).Append(vbLf)
            Invenzione(testo, "app", inventateApp)
            Invenzione(testo, "prototipo", inventatePrototipo)

            testo.Append("## La stessa cosa contata due volte").Append(vbLf).Append(vbLf)
            testo.Append("*Un'attività che sta sia fra le esperienze formali sia fra le informali. ").
                  Append("Segnalata e non bocciata: nasce da un caso di confine che il prompt — ").
                  Append("lo stesso da entrambe le parti — non decide. Ma un doppione è sempre ").
                  Append("sbagliato, e la tolleranza sul conteggio delle voci se lo mangerebbe.*").
                  Append(vbLf).Append(vbLf)
            Doppioni(testo, "app", doppioniApp)
            Doppioni(testo, "prototipo", doppioniPrototipo)

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
        Private Shared Function Riassunto(dove As String, trascrizioni As ConfrontoElenchi,
                                          competenze As ConfrontoElenchi,
                                          daApp As TrovaLavoro.Dati.Profilo,
                                          daPrototipo As TrovaLavoro.Dati.Profilo,
                                          inventateApp As Invenzioni,
                                          inventatePrototipo As Invenzioni,
                                          doppioniApp As List(Of String),
                                          doppioniPrototipo As List(Of String)) As String

            Dim testo As New StringBuilder()

            testo.AppendLine("Collaudo di tappa T3 — import del CV vero, app contro prototipo.")
            testo.AppendLine($"Trascrizioni: {trascrizioni.VociApp} righe l'app, " &
                             $"{trascrizioni.VociPrototipo} il prototipo, " &
                             $"{trascrizioni.InComune} in comune ({trascrizioni.Frazione * 100:F1}%).")
            testo.AppendLine($"Voci nel profilo (prototipo → app): " &
                             $"formali {daPrototipo.EsperienzeFormali.Count} → {daApp.EsperienzeFormali.Count}, " &
                             $"informali {daPrototipo.EsperienzeInformali.Count} → {daApp.EsperienzeInformali.Count}, " &
                             $"competenze {daPrototipo.Competenze.Count} → {daApp.Competenze.Count} " &
                             $"({competenze.InComune} in comune), " &
                             $"formazione {daPrototipo.Formazione.Count} → {daApp.Formazione.Count}.")
            testo.AppendLine($"Valori non ritrovati nel CV: app {inventateApp.Gravi.Count} gravi " &
                             $"e {inventateApp.DaGuardare.Count} da guardare, " &
                             $"prototipo {inventatePrototipo.Gravi.Count} gravi " &
                             $"e {inventatePrototipo.DaGuardare.Count} da guardare.")
            testo.AppendLine($"Voci contate due volte: app {doppioniApp.Count}, " &
                             $"prototipo {doppioniPrototipo.Count}.")
            testo.AppendLine($"Rapporto completo (contiene dati personali, fuori dal repo): {dove}")

            Return testo.ToString()

        End Function

        ''' <summary>Una riga della tabella di confronto, con il segno di ciò che non torna.</summary>
        Private Shared Sub Riga(testo As StringBuilder, campo As String, prototipo As String, app As String)

            Dim uguali As Boolean = Ripulito(prototipo) = Ripulito(app)

            testo.Append($"| {campo} | {PerTabella(prototipo)} | {PerTabella(app)} | ").
                  Append(If(uguali, "=", "**≠**")).Append(" |").Append(vbLf)

        End Sub

        ''' <summary>Un valore dentro una cella di tabella, senza rompere la tabella.</summary>
        Private Shared Function PerTabella(valore As String) As String

            Dim pulito As String = Ripulito(valore).Replace("|", "\|")
            Return If(pulito.Length = 0, "*(vuoto)*", pulito)

        End Function

        ''' <summary>Un elenco di righe, o il silenzio se non ce ne sono.</summary>
        Private Shared Sub Elenco(testo As StringBuilder, titolo As String, righe As List(Of String))

            If righe.Count = 0 Then Return

            testo.Append($"**{titolo}** (fino a {RigheDaMostrare}):").Append(vbLf).Append(vbLf)
            For Each riga As String In righe
                testo.Append($"- `{riga}`").Append(vbLf)
            Next
            testo.Append(vbLf)

        End Sub

        ''' <summary>Le voci contate due volte, per un lato.</summary>
        Private Shared Sub Doppioni(testo As StringBuilder, lato As String, doppioni As List(Of String))

            If doppioni.Count = 0 Then
                testo.Append($"**{lato}** — nessuna voce contata due volte.").Append(vbLf).Append(vbLf)
                Return
            End If

            testo.Append($"**{lato}**").Append(vbLf).Append(vbLf)
            For Each voce As String In doppioni
                testo.Append($"- ⚠️ {voce}").Append(vbLf)
            Next
            testo.Append(vbLf)

        End Sub

        ''' <summary>Cosa non si è ritrovato nel CV, per un lato.</summary>
        Private Shared Sub Invenzione(testo As StringBuilder, lato As String, inventate As Invenzioni)

            testo.Append($"**{lato}** — ")

            If inventate.Gravi.Count = 0 AndAlso inventate.DaGuardare.Count = 0 Then
                testo.Append("ogni valore copiato si ritrova nel CV.").Append(vbLf).Append(vbLf)
                Return
            End If

            testo.Append(vbLf).Append(vbLf)

            For Each voce As String In inventate.Gravi
                testo.Append($"- ⛔ {voce}").Append(vbLf)
            Next
            For Each voce As String In inventate.DaGuardare
                testo.Append($"- ⚠️ {voce} *(riformulazione ammessa: da guardare)*").Append(vbLf)
            Next

            testo.Append(vbLf)

        End Sub

    End Class

End Namespace
