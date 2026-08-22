Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace NonRegressione

    ''' <summary>
    ''' Il collaudo di tappa di T9 (cap. 14): la checklist <b>«Problemi e mitigazioni»</b>
    ''' ereditata dal prototipo, ripercorsa sulla nuova app con l'AI vera.
    ''' </summary>
    ''' <remarks>
    ''' <para>Le otto voci della checklist (<c>HTML+JS/prompt_design.md</c>, in fondo) sono
    ''' i rischi incontrati costruendo il prototipo e le difese che ne sono nate. Sette
    ''' delle otto difese oggi vivono <b>dentro i prompt del pool</b>, e un prompt lo si
    ''' può leggere quanto si vuole: dice cosa il modello è tenuto a fare, non cosa fa. Le
    ''' tre prove di questa classe glielo chiedono, ognuna costruita per <b>tentarlo</b>
    ''' dove la checklist dice che è debole:</para>
    ''' <list type="number">
    ''' <item><b>Il candidato che si vende</b> — voci 1, 2, 3, 5: un dialogo intero
    ''' condotto da chi si gonfia, racconta un lavoro in nero al turno dei lavori veri, è
    ''' reticente sui recapiti e infila tre impieghi in una battuta sola.</item>
    ''' <item><b>L'annuncio scarno</b> — voce 7: quattro righe che dicono mansione, sede e
    ''' contratto, e nient'altro. Il gemello del gonfiamento, dal lato dell'offerta.</item>
    ''' <item><b>Il confronto con le lacune</b> — voce 8: un profilo magro contro un
    ''' annuncio esigente, dove ogni requisito non dichiarato deve diventare una
    ''' <i>lacuna</i> e non un <i>non si sa</i>.</item>
    ''' </list>
    ''' <para>Restano fuori due voci, e per due ragioni opposte. La <b>6</b> (scoring della
    ''' famiglia A) è codice, non prompt: la difesa è l'architettura ibrida — l'AI giudica,
    ''' <see cref="CalcoloMatch"/> calcola — ed è già sorvegliata senza rete da
    ''' <see cref="CollaudiCalcoloMatch"/> e dalla parità deterministica di
    ''' <see cref="CollaudiConfrontoReale"/>. La <b>4</b> (<c>pending_questions</c>) non è
    ''' mai stata costruita: il cancello T0 l'ha lasciata fuori
    ''' (<c>VB.NET/progetto/15_decisioni_aperte.md</c>, §15.5) e nella 1.0 il suo posto lo
    ''' tengono il default sicuro sui vuoti, la conferma in blocco e il campo
    ''' <c>altrove</c> dell'anti-perdita — che questa classe prova.</para>
    ''' <para><b>Dove sta il pass/fail.</b> Non su ciò che il modello decide — quello cambia
    ''' da un giro all'altro, ed è la lezione di T2 — ma su ciò che la difesa promette
    ''' comunque il modello risponda: che nel profilo non finiscano parole che l'utente non
    ''' ha detto né livelli che non ha dichiarato, che un'esperienza informale non si
    ''' travesta da lavoro, che un campo taciuto resti vuoto, che tre lavori raccontati
    ''' insieme restino tre, che un annuncio muto non si riempia di requisiti «tipici», e
    ''' che una lacuna si chiami lacuna. Quanto abbia normalizzato una competenza, o come
    ''' abbia formulato una spiegazione, si legge nei rapporti.</para>
    ''' <para><b>Le persone e le aziende sono inventate</b>, come in
    ''' <see cref="CollaudiDialogoReale"/> e per la stessa ragione: il repo è pubblico. Ed
    ''' è meglio così anche nel merito — una traccia inventata si costruisce apposta perché
    ''' le difese abbiano occasione di cedere, cosa che un racconto vero farebbe solo per
    ''' caso. Perciò i rapporti stanno <b>nel repo</b>, in <c>casi/reale/</c>.</para>
    ''' <para>Categoria <b>Reale</b>: vuole la chiave e nient'altro — né il CV vero né il
    ''' prototipo. Si lancia da <c>VB.NET/src</c> con
    ''' <c>dotnet test --settings TrovaLavoro.Collaudi/collaudi-reali.runsettings</c>.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiChecklistReale

        ' ==================================================================
        ' Prova A — il candidato che si vende (voci 1, 2, 3, 5)
        ' ==================================================================

        ''' <summary>Il rapporto della prova A.</summary>
        Private Const NomeRapportoDialogo As String = "checklist_candidato_che_si_vende.md"

        ''' <summary>Chi conduce la traccia: non esiste.</summary>
        Private Const ChiParla As String = "Marco"

        ''' <summary>
        ''' Le sette risposte di Marco Gentili, che non esiste. Ogni turno porta la sua
        ''' tentazione:
        ''' <list type="bullet">
        ''' <item><b>contatti</b> — dice la città e si rifiuta di dare il resto: email e
        ''' telefono devono restare <b>vuoti</b> (voce 3);</item>
        ''' <item><b>esperienze_formali</b> — tre impieghi in una battuta sola (voce 5) e,
        ''' in coda, il banco del cognato «in nero», che un lavoro vero non è (voce 2);</item>
        ''' <item><b>competenze</b> — si vende: «leader nato», «una marcia in più», «un po'
        ''' di inglese», «me la cavo col computer». Nessuna di queste può uscirne con un
        ''' livello o un'etichetta che l'utente non ha dichiarato (voce 1);</item>
        ''' <item><b>formazione</b> — «la licenza media e basta»: il titolo c'è, la scuola e
        ''' l'anno no, e non si indovinano (voce 3). In coda, le superiori lasciate a metà,
        ''' che un titolo di studio non sono.</item>
        ''' </list>
        ''' Il resto è una storia normale: senza una storia intera non si vedrebbe se il
        ''' profilo finale sta in piedi.
        ''' </summary>
        Private Shared ReadOnly Traccia As New List(Of ConduttoreDiDialogo.Gruppo) From {
            New ConduttoreDiDialogo.Gruppo With {
                .Turno = "nome",
                .Battute = New List(Of String) From {
                    "Marco Gentili."}},
            New ConduttoreDiDialogo.Gruppo With {
                .Turno = "contatti",
                .Battute = New List(Of String) From {
                    "Mi trovi a Cesena. Il resto per adesso lascia stare, non mi va di " &
                    "scriverlo qui."}},
            New ConduttoreDiDialogo.Gruppo With {
                .Turno = "patente",
                .Battute = New List(Of String) From {
                    "Sì, ho la patente B."}},
            New ConduttoreDiDialogo.Gruppo With {
                .Turno = "esperienze_formali",
                .Battute = New List(Of String) From {
                    "Ho lavorato tre anni in magazzino alla Rossi Imballaggi di Cesena, dal " &
                    "2018 al 2021, facevo carico e scarico; prima ancora sei mesi come aiuto " &
                    "giardiniere alla Verde Vivo; e nel 2022 ho fatto il fattorino per la " &
                    "pizzeria Vesuvio. Ah, poi d'estate do una mano a mio cognato al banco " &
                    "del mercato, ma in nero, così per fargli un favore."}},
            New ConduttoreDiDialogo.Gruppo With {
                .Turno = "esperienze_informali",
                .Battute = New List(Of String) From {
                    "Da due anni sono volontario alla protezione civile del mio comune."}},
            New ConduttoreDiDialogo.Gruppo With {
                .Turno = "competenze",
                .Battute = New List(Of String) From {
                    "Guarda, sono un leader nato e ho una marcia in più rispetto agli altri. " &
                    "Me la cavo col computer e con la posta elettronica, un po' di inglese lo " &
                    "so, e sotto pressione non mi faccio prendere dal panico."}},
            New ConduttoreDiDialogo.Gruppo With {
                .Turno = "formazione",
                .Battute = New List(Of String) From {
                    "Ho la licenza media e basta. Le superiori le ho lasciate a metà."}}}

        ''' <summary>
        ''' Le parole del frammento che il turno dei lavori veri non deve accogliere: è la
        ''' voce 2 della checklist, l'esperienza informale promossa a formale.
        ''' </summary>
        Private Shared ReadOnly ParoleDelLavoroInNero As String() = {"cognato", "mercato"}

        ''' <summary>
        ''' I livelli di padronanza che l'utente non ha dichiarato: se ne esce uno, la
        ''' competenza è stata <b>gonfiata</b> (voce 1). Si confrontano come parole intere —
        ''' «b1» dentro un'altra parola non è un livello.
        ''' </summary>
        Private Shared ReadOnly LivelliDiLingua As String() =
            {"a1", "a2", "b1", "b2", "c1", "c2"}

        ''' <summary>
        ''' Le etichette che promuovono un modo di dire in gergo professionale: stesso
        ''' difetto della voce 1, per prefisso invece che per parola intera («avanzato»,
        ''' «avanzata»…). Il prompt <c>competenze</c> le vieta per nome: «me la cavo alla
        ''' cassa» diventa «Uso della cassa», MAI «gestione transazioni e contante».
        ''' </summary>
        Private Shared ReadOnly GonfiamentiTipici As String() =
            {"avanzat", "espert", "certificat", "fluent", "madrelingua", "senior"}

        ''' <summary>
        ''' Le competenze che l'utente ha dichiarato <b>con un'attenuante</b>: nel profilo
        ''' non possono comparire nude.
        ''' </summary>
        ''' <remarks>
        ''' <para>È la seconda spia della voce 1, e serve perché la prima non basta. Il
        ''' gonfiamento che ci si aspetta — «Inglese B2» da «un po' di inglese» — è quello
        ''' appariscente; quello che capita davvero è più quieto: l'attenuante si perde e
        ''' resta «Inglese», che promette una lingua che il candidato non ha promesso. Il
        ''' prompt lo vieta per nome («inglese scolastico» resta «Inglese scolastico»).</para>
        ''' <para>Si pretende solo che la competenza non resti <b>una parola sola</b>: «Un po'
        ''' di inglese», «Inglese scolastico», «Inglese di base» passano tutte, perché
        ''' l'attenuante c'è; passa solo «Inglese» nudo. Pretendere le parole esatte
        ''' boccerebbe la normalizzazione leggera, che è ciò che il prompt ordina.</para>
        ''' <para><b>Falsificata</b> il 2026-08-22 togliendo dal prompt le due regole della
        ''' normalizzazione: il modello ha smesso di scrivere «un po' di inglese» e ha
        ''' scritto «inglese», e questa spia è diventata rossa mentre quella dei livelli
        ''' restava verde.</para>
        ''' </remarks>
        Private Shared ReadOnly AttenuantiDaConservare As String() = {"inglese"}

        <TestMethod, TestCategory("Reale")>
        Public Async Function IlCandidatoCheSiVendeNonEntraGonfiatoNelProfilo() As Task

            Dim chiave As String = CollaudoReale.ChiaveOppureRinuncia()

            Dim libreria As LibreriaPrompt = CollaudoReale.PoolIntegrato()

            Using client As New ClientClaude(chiave)

                ' La spia serve anche qui: il banco del cognato deve poter essere seguito
                ' fin dove il modello l'ha mandato (voce 2).
                Dim spia As New StrutturatoreSpia(New StrutturatoreTurni(libreria, client))
                Dim dialogo As New DialogoProfilo(spia)

                Dim conduttore As New ConduttoreDiDialogo(Traccia)
                Dim battiti As List(Of ConduttoreDiDialogo.Battito) = Await conduttore.ConduciAsync(dialogo)

                Dim profilo As Profilo = dialogo.Profilo
                Dim detto As String = ConduttoreDiDialogo.TestoDetto(battiti)

                Dim gonfiate As List(Of String) = CompetenzeGonfiate(profilo, detto)
                Dim promosse As List(Of String) = InformaliPromosseAFormali(profilo)
                Dim inventate As CollaudoReale.Invenzioni =
                    CollaudoReale.ValoriFuoriDalTesto(profilo, detto)
                Dim allineamento As List(Of String) = conduttore.AllineamentoRotto(spia)

                ' Il rapporto si scrive PRIMA di giudicare: se un Assert ferma tutto, la
                ' prova di com'era andata resta su disco comunque.
                Dim dove As String = ScriviRapportoDialogo(battiti, spia, profilo, gonfiate,
                                                           promosse, inventate, allineamento,
                                                           conduttore.Stranezze)
                Console.WriteLine($"Checklist, prova A — voci 1, 2, 3, 5. Mosse {battiti.Count}, " &
                                  $"competenze gonfiate {gonfiate.Count}, informali promosse " &
                                  $"{promosse.Count}, valori inventati {inventate.Gravi.Count}. " &
                                  $"Rapporto: {dove}")

                ' --- Il giro è valido. -----------------------------------------------
                Assert.IsTrue(dialogo.Finito,
                    "il dialogo non è arrivato in fondo: vedi il rapporto per l'ultima mossa")
                Assert.IsEmpty(allineamento,
                    "il conduttore è andato fuori passo, e questo giro non misura le difese " &
                    "ma sé stesso: " & String.Join(" · ", allineamento))

                ' --- Voce 5: più voci raccontate in una sola risposta. ----------------
                Assert.IsGreaterThanOrEqualTo(3, profilo.EsperienzeFormali.Count,
                    "i tre impieghi erano in una battuta sola e devono restare tre: " &
                    $"ne sono usciti {profilo.EsperienzeFormali.Count}")

                ' --- Voce 2: l'informale non si traveste da lavoro. -------------------
                Assert.IsEmpty(promosse,
                    "il banco del cognato «in nero» è finito fra le esperienze formali: " &
                    String.Join(" · ", promosse))

                ' --- Voce 3: i campi non detti restano vuoti. -------------------------
                Assert.AreEqual("", profilo.Contatti.Email.Trim(),
                    "l'email non è stata detta e non si indovina")
                Assert.AreEqual("", profilo.Contatti.Telefono.Trim(),
                    "il telefono non è stato detto e non si indovina")

                ' La stessa difesa su una voce che invece è entrata: il titolo l'utente
                ' l'ha detto, la scuola e l'anno no. Se il turno non ha colto niente non
                ' c'è nulla da giudicare — e allora è il rapporto a dirlo, perché un
                ' collaudo che non trova la voce non deve passare per riuscito.
                For Each voce As VoceFormazione In profilo.Formazione
                    Assert.AreEqual("", voce.Istituto.Trim(),
                        $"«{voce.Titolo}»: la scuola non è stata detta e non si indovina")
                    Assert.AreEqual("", voce.Anno.Trim(),
                        $"«{voce.Titolo}»: l'anno non è stato detto e non si indovina")
                Next

                ' --- Voce 1: nessuna competenza gonfiata. -----------------------------
                Assert.IsEmpty(gonfiate,
                    "competenze uscite con un livello o un'etichetta che l'utente non ha " &
                    "dichiarato: " & String.Join(" · ", gonfiate))

                ' --- La rete di sempre: nel profilo solo parole dell'utente. ----------
                Assert.IsEmpty(inventate.Gravi,
                    "nel profilo ci sono valori che nella traccia non compaiono: " &
                    String.Join(" · ", inventate.Gravi))

            End Using

        End Function

        ''' <summary>
        ''' Le competenze uscite con un livello o un'etichetta di gergo che l'utente non
        ''' aveva dichiarato (voce 1).
        ''' </summary>
        ''' <remarks>
        ''' Non si pretende che la competenza sia <b>ancorata</b> parola per parola al
        ''' detto: il prompt ordina di normalizzare («me la cavo alla cassa» → «Uso della
        ''' cassa»), e pretenderlo boccerebbe il comportamento giusto. Si pretende che non
        ''' compaia ciò che il prompt vieta per nome — un livello di lingua mai dichiarato,
        ''' un'etichetta che promuove il modo di dire in gergo. E la spia è doppia: la
        ''' parola sospetta fa fallire solo se <b>l'utente non l'ha usata</b>.
        ''' </remarks>
        Friend Shared Function CompetenzeGonfiate(profilo As Profilo, detto As String) As List(Of String)

            Dim pagliaio As String = CollaudoReale.PerCercare(detto)
            Dim paroleDette As New HashSet(Of String)(
                pagliaio.Split(" "c, StringSplitOptions.RemoveEmptyEntries))

            Dim trovate As New List(Of String)

            For Each competenza As String In profilo.Competenze

                Dim pulita As String = CollaudoReale.PerCercare(competenza)
                Dim parole As String() = pulita.Split(" "c, StringSplitOptions.RemoveEmptyEntries)

                For Each livello As String In LivelliDiLingua
                    If Not parole.Contains(livello) Then Continue For
                    If paroleDette.Contains(livello) Then Continue For
                    trovate.Add($"«{CollaudoReale.Ripulito(competenza)}» porta il livello " &
                                $"«{livello}», che l'utente non ha dichiarato")
                Next

                For Each gonfiamento As String In GonfiamentiTipici
                    If Not pulita.Contains(gonfiamento) Then Continue For
                    If pagliaio.Contains(gonfiamento) Then Continue For
                    trovate.Add($"«{CollaudoReale.Ripulito(competenza)}» usa «{gonfiamento}…», " &
                                "che nella risposta dell'utente non c'è")
                Next

                For Each attenuata As String In AttenuantiDaConservare
                    If Not parole.Contains(attenuata) Then Continue For
                    If parole.Length > 1 Then Continue For
                    trovate.Add($"«{CollaudoReale.Ripulito(competenza)}» è rimasta nuda: " &
                                "l'utente l'aveva attenuata, e senza l'attenuante promette " &
                                "più di quanto abbia detto")
                Next

            Next

            Return trovate

        End Function

        ''' <summary>
        ''' Le esperienze formali che parlano del lavoro in nero raccontato nella stessa
        ''' battuta (voce 2). Si guardano i tre campi che il prompt riempie con le parole
        ''' dell'utente; <c>cosa_facevo</c> compreso, perché è lì che un frammento estraneo
        ''' finirebbe per essere assorbito senza farsi notare.
        ''' </summary>
        Friend Shared Function InformaliPromosseAFormali(profilo As Profilo) As List(Of String)

            Dim trovate As New List(Of String)

            For Each esperienza As EsperienzaFormale In profilo.EsperienzeFormali

                Dim tutto As String = CollaudoReale.PerCercare(
                    $"{esperienza.Ruolo} {esperienza.Azienda} {esperienza.CosaFacevo}")

                For Each parola As String In ParoleDelLavoroInNero
                    If Not tutto.Contains(parola) Then Continue For
                    trovate.Add($"«{CollaudoReale.Ripulito(esperienza.Ruolo)} / " &
                                $"{CollaudoReale.Ripulito(esperienza.Azienda)}» contiene " &
                                $"«{parola}»")
                    Exit For
                Next

            Next

            Return trovate

        End Function

        ' ==================================================================
        ' Prova B — l'annuncio scarno (voce 7)
        ' ==================================================================

        ''' <summary>Il rapporto della prova B.</summary>
        Private Const NomeRapportoAnnuncio As String = "checklist_annuncio_scarno.md"

        ''' <summary>
        ''' Quattro righe e nient'altro: la mansione, la sede, il contratto e un indirizzo.
        ''' Tutto ciò che un magazziniere «di solito» deve avere — patente, muletto,
        ''' esperienza, diploma, disponibilità ai turni — qui <b>non c'è scritto</b>, ed è
        ''' esattamente ciò che non deve comparire nell'analisi (voce 7).
        ''' </summary>
        Private Const AnnuncioScarno As String =
            "Cercasi magazziniere per la nostra sede di Forlì." & vbLf &
            "Contratto a tempo determinato, full time." & vbLf &
            "Inviare la candidatura a lavoro@example.it"

        ''' <summary>Le quattro liste di requisiti, più le due di contesto che il prompt protegge.</summary>
        Private Shared ReadOnly ListeDaAncorare As String() =
            {"competenze_richieste", "esperienza_richiesta", "formazione_richiesta",
             "altri_requisiti", "mansioni", "benefit"}

        <TestMethod, TestCategory("Reale")>
        Public Async Function LAnnuncioScarnoNonSiRiempieDiRequisitiTipici() As Task

            Dim chiave As String = CollaudoReale.ChiaveOppureRinuncia()

            Dim libreria As LibreriaPrompt = CollaudoReale.PoolIntegrato()

            Using client As New ClientClaude(chiave)

                Dim annuncio As JsonObject = TryCast(
                    Await New AnalizzatoreAnnuncio(libreria, client).AnalizzaAsync(AnnuncioScarno),
                    JsonObject)

                Assert.IsNotNull(annuncio, "l'analisi non ha restituito un oggetto JSON")

                Dim aggiunte As List(Of String) = VociFuoriDallAnnuncio(annuncio)

                Dim dove As String = ScriviRapportoAnnuncio(annuncio, aggiunte)
                Console.WriteLine($"Checklist, prova B — voce 7. Voci non ancorate al testo: " &
                                  $"{aggiunte.Count}. Rapporto: {dove}")

                ' --- Voce 7: niente requisiti «tipici» non scritti. -------------------
                Assert.IsEmpty(aggiunte,
                    "l'analisi ha aggiunto all'annuncio roba che l'annuncio non scrive: " &
                    String.Join(" · ", aggiunte))

                ' --- L'azienda non c'è, e non si deduce. ------------------------------
                Assert.AreEqual("", Valore(annuncio("azienda")).Trim(),
                    "l'annuncio non nomina nessuna azienda: il campo resta vuoto")

                ' --- Ciò che invece c'è dev'esserci: un divieto non basta. ------------
                Assert.AreEqual("lavoro@example.it",
                    Valore(TryCast(annuncio("contatto"), JsonObject)?("email")).Trim(),
                    "l'indirizzo è scritto nell'annuncio e va ricopiato alla lettera")
                Assert.Contains("magazzinier", CollaudoReale.PerCercare(Valore(annuncio("titolo"))),
                    "il ruolo dell'annuncio")

            End Using

        End Function

        ''' <summary>
        ''' Le voci dell'analisi che nel testo dell'annuncio non si ritrovano: sono i
        ''' requisiti, le mansioni e i benefit «tipici» che la voce 7 vieta di aggiungere.
        ''' </summary>
        ''' <remarks>
        ''' L'ancoraggio è quello dell'anti-invenzione di sempre
        ''' (<see cref="CollaudoReale.NelTesto"/>): ogni parola dalle quattro lettere in su
        ''' deve comparire nell'annuncio. Regge la normalizzazione leggera che il prompt
        ''' ordina — riordinare e ripulire non aggiunge parole — e non regge l'invenzione,
        ''' che ne aggiunge sempre.
        ''' </remarks>
        Friend Shared Function VociFuoriDallAnnuncio(annuncio As JsonObject) As List(Of String)

            Dim pagliaio As String = CollaudoReale.PerCercare(AnnuncioScarno)
            Dim senzaSpazi As String = pagliaio.Replace(" ", "")

            Dim fuori As New List(Of String)

            For Each nomeLista As String In ListeDaAncorare

                Dim lista As JsonArray = TryCast(annuncio(nomeLista), JsonArray)
                If lista Is Nothing Then Continue For

                For Each voce As JsonNode In lista

                    ' I requisiti sono oggetti { testo, priorita }, mansioni e benefit
                    ' stringhe nude: si guarda il testo, comunque sia confezionato.
                    Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                    Dim testo As String = If(oggetto Is Nothing, Valore(voce), Valore(oggetto("testo")))

                    If String.IsNullOrWhiteSpace(testo) Then Continue For
                    If CollaudoReale.NelTesto(testo, pagliaio, senzaSpazi) Then Continue For

                    fuori.Add($"{nomeLista}: «{CollaudoReale.Ripulito(testo)}»")

                Next

            Next

            Return fuori

        End Function

        ' ==================================================================
        ' Prova C — il confronto con le lacune (voce 8)
        ' ==================================================================

        ''' <summary>Il rapporto della prova C.</summary>
        Private Const NomeRapportoConfronto As String = "checklist_confronto_lacune.md"

        ''' <summary>
        ''' Un profilo <b>magro</b>: un solo lavoro, nessuna formazione, due competenze
        ''' pratiche. Non è un candidato scarso — è un candidato che di certe cose non ha
        ''' parlato, ed è la condizione in cui la voce 8 era stata scoperta.
        ''' </summary>
        Private Shared Function ProfiloMagro() As JsonNode

            Return New JsonObject From {
                {"nome", "Giulia Neri"},
                {"contatti", New JsonObject From {
                    {"email", "giulia.neri@example.it"},
                    {"telefono", ""},
                    {"citta", "Ravenna"},
                    {"link", ""}}},
                {"patente", New JsonObject From {
                    {"ha", "sì"},
                    {"categorie", New JsonArray From {"B"}}}},
                {"esperienze_formali", New JsonArray From {
                    New JsonObject From {
                        {"ruolo", "Banconiera"},
                        {"azienda", "Bar Centrale di Ravenna"},
                        {"durata", "1 anno"},
                        {"cosa_facevo", "Servizio al banco, caffetteria e cassa."},
                        {"tipo", ""}}}},
                {"esperienze_informali", New JsonArray()},
                {"competenze", New JsonArray From {
                    "Uso della macchina del caffè", "Uso della cassa"}},
                {"formazione", New JsonArray()}}

        End Function

        ''' <summary>
        ''' Un annuncio <b>esigente</b>: chiede cose che il profilo magro non dichiara
        ''' affatto (una cucina, un titolo, l'inglese) e cose che il profilo non raccoglie
        ''' nemmeno (la disponibilità nei weekend). Le prime sono <b>lacune</b>, la seconda
        ''' è l'unico «non si sa» legittimo: è su questo confine che si gioca la voce 8.
        ''' </summary>
        Private Shared Function AnnuncioEsigente() As JsonNode

            Return New JsonObject From {
                {"competenze_richieste", New JsonArray From {
                    New JsonObject From {
                        {"testo", "Preparazione di primi e secondi piatti"}, {"priorita", "richiesto"}},
                    New JsonObject From {
                        {"testo", "Inglese per il servizio ai clienti stranieri"}, {"priorita", "richiesto"}}}},
                {"esperienza_richiesta", New JsonArray From {
                    New JsonObject From {
                        {"testo", "Almeno 3 anni in cucina di ristorante"},
                        {"priorita", "richiesto"}, {"anni", 3}}}},
                {"formazione_richiesta", New JsonArray From {
                    New JsonObject From {
                        {"testo", "Diploma di istituto alberghiero"}, {"priorita", "richiesto"}},
                    New JsonObject From {
                        {"testo", "Attestato HACCP in corso di validità"}, {"priorita", "richiesto"}}}},
                {"altri_requisiti", New JsonArray From {
                    New JsonObject From {
                        {"testo", "Disponibilità a lavorare nei fine settimana"}, {"priorita", "richiesto"}}}},
                {"titolo", "Cuoco di partita per ristorante di pesce"},
                {"azienda", "Ristorante La Salina"},
                {"sede", New JsonArray From {"Cervia"}},
                {"contratto", New JsonObject From {
                    {"tipo", "Tempo determinato"}, {"durata", "Stagionale, sei mesi"},
                    {"orario", "Full time su turni"}, {"retribuzione", ""}}},
                {"mansioni", New JsonArray From {
                    "Preparazione delle materie prime", "Servizio ai fornelli durante il servizio"}},
                {"benefit", New JsonArray From {"Vitto e alloggio per i fuori sede"}},
                {"lingua", "it"},
                {"contatto", New JsonObject From {{"email", ""}, {"riferimento", ""}}}}

        End Function

        ''' <summary>Le categorie in cui un requisito non dichiarato è una lacuna, mai un dubbio.</summary>
        ''' <remarks>
        ''' Sono le tre che il dialogo raccoglie apposta dal candidato: se dopo aver cercato
        ''' su tutto il profilo non se ne trova traccia, l'assenza è <b>reale</b>. È la
        ''' «distinzione chiave» del prompt <c>confronto</c>, ed è la difesa della voce 8.
        ''' <c>altri_requisiti</c> resta fuori: il profilo non raccoglie ancora domicilio e
        ''' disponibilità, e lì «non determinabile» è la risposta onesta.
        ''' </remarks>
        Private Shared ReadOnly CategorieSenzaDubbio As String() =
            {"competenze", "esperienza", "formazione"}

        ''' <summary>Quante stelle al massimo può prendere un profilo che non copre quasi nulla.</summary>
        ''' <remarks>
        ''' Non è una taratura fine: è la soglia sotto cui il punteggio sta <b>discriminando</b>.
        ''' Prima del confine della voce 8 lo <c>score_base</c> saturava — i requisiti non
        ''' dichiarati uscivano dal conteggio come «non determinabile» e restava dentro solo
        ''' ciò che il profilo copriva — e un candidato così avrebbe preso quasi il massimo.
        ''' </remarks>
        Private Const StelleMassimeAmmesse As Double = 2.0

        <TestMethod, TestCategory("Reale")>
        Public Async Function IlConfrontoChiamaLacunaCioCheIlProfiloNonDichiara() As Task

            Dim chiave As String = CollaudoReale.ChiaveOppureRinuncia()

            Dim libreria As LibreriaPrompt = CollaudoReale.PoolIntegrato()

            Using client As New ClientClaude(chiave)

                Dim risposta As JsonNode = Await New Confrontatore(libreria, client).
                    ConfrontaAsync(ProfiloMagro(), AnnuncioEsigente())

                Dim uscita As JsonObject = TryCast(risposta, JsonObject)
                Assert.IsNotNull(uscita, "il confronto non ha restituito un oggetto JSON")

                Dim giudizi As JsonArray = TryCast(uscita("giudizi"), JsonArray)
                Assert.IsNotNull(giudizi, "il confronto non ha restituito i giudizi")

                Dim punteggio As RisultatoMatch =
                    CalcoloMatch.Calcola(giudizi, uscita("numero_complessivo"))

                Dim dubbiFuoriPosto As List(Of String) = DubbiDoveDovrebberoEsserciLacune(giudizi)
                Dim lacune As Integer = Quanti(giudizi, "non soddisfatto")

                Dim dove As String = ScriviRapportoConfronto(uscita, giudizi, punteggio,
                                                             dubbiFuoriPosto, lacune)
                Console.WriteLine($"Checklist, prova C — voce 8. Giudizi {giudizi.Count}, " &
                                  $"lacune {lacune}, «non determinabile» fuori posto " &
                                  $"{dubbiFuoriPosto.Count}, stelle {punteggio.Stelle}. " &
                                  $"Rapporto: {dove}")

                ' --- Voce 8: una lacuna si chiama lacuna. -----------------------------
                Assert.IsEmpty(dubbiFuoriPosto,
                    "requisiti che il profilo non dichiara affatto sono stati archiviati come " &
                    "«non determinabile» invece che come lacune, ed escono dal conteggio: " &
                    String.Join(" · ", dubbiFuoriPosto))

                ' Se non ci fosse nemmeno una lacuna, la prova non avrebbe provato niente:
                ' vorrebbe dire che il profilo magro copriva l'annuncio esigente.
                Assert.IsGreaterThan(0, lacune,
                    "nessun requisito è stato giudicato «non soddisfatto»: il caso non mette " &
                    "più alla prova il confine della voce 8 e va rivisto")

                ' --- Voce 8, la conseguenza: il punteggio discrimina. -----------------
                Assert.IsNotNull(punteggio.Stelle, "il calcolo non ha prodotto le stelle")
                Assert.IsLessThanOrEqualTo(StelleMassimeAmmesse, punteggio.Stelle.Value,
                    $"un profilo che non copre quasi nulla ha preso {punteggio.Stelle} stelle: " &
                    "il punteggio sta saturando invece di distinguere")

            End Using

        End Function

        ''' <summary>
        ''' I giudizi che archiviano come «non determinabile» un requisito di competenze,
        ''' esperienza o formazione: lì il profilo <b>è</b> la fonte, e ciò che non contiene
        ''' è una lacuna. Sono i casi che facevano saturare il punteggio.
        ''' </summary>
        Friend Shared Function DubbiDoveDovrebberoEsserciLacune(giudizi As JsonArray) As List(Of String)

            Dim fuoriPosto As New List(Of String)

            For Each giudizio As JsonNode In giudizi

                Dim voce As JsonObject = TryCast(giudizio, JsonObject)
                If voce Is Nothing Then Continue For

                Dim categoria As String = Valore(voce("categoria")).Trim().ToLowerInvariant()
                If Not CategorieSenzaDubbio.Contains(categoria) Then Continue For

                If Valore(voce("esito")).Trim().ToLowerInvariant() <> "non determinabile" Then Continue For

                fuoriPosto.Add($"[{categoria}] «{CollaudoReale.Ripulito(Valore(voce("requisito")))}»")

            Next

            Return fuoriPosto

        End Function

        ''' <summary>Quanti giudizi portano quell'esito.</summary>
        Private Shared Function Quanti(giudizi As JsonArray, esito As String) As Integer

            Return giudizi.Cast(Of JsonNode)().Count(Function(g) Valore(TryCast(g, JsonObject)?("esito")).
                                             Trim().ToLowerInvariant() = esito)

        End Function

        ' ==================================================================
        ' I rapporti
        ' ==================================================================

        ''' <summary>L'intestazione comune: cosa si stava provando, e quando.</summary>
        Private Shared Function Apertura(titolo As String, voci As String, cosa As String) As StringBuilder

            Dim testo As New StringBuilder()

            testo.Append($"# Checklist «Problemi e mitigazioni» — {titolo}").Append(vbLf).Append(vbLf)
            testo.Append($"*Collaudo di tappa di T9 (cap. 14), voci **{voci}** della checklist ").
                  Append("ereditata dal prototipo (`HTML+JS/prompt_design.md`). ").Append(cosa).
                  Append(" Persone e aziende sono inventate: per questo il rapporto sta nel repo.*").
                  Append(vbLf).Append(vbLf)
            testo.Append($"- **Quando**: {DateTime.Now:yyyy-MM-dd HH:mm}").Append(vbLf)

            Return testo

        End Function

        ''' <summary>Un elenco di esiti, o la riga che dice che non ce ne sono.</summary>
        Private Shared Sub Esiti(testo As StringBuilder, titolo As String, voce As String,
                                 righe As List(Of String), seNessuno As String)

            testo.Append($"## {titolo}").Append(vbLf).Append(vbLf)
            testo.Append($"*Voce {voce}.*").Append(vbLf).Append(vbLf)

            If righe.Count = 0 Then
                testo.Append("✅ ").Append(seNessuno).Append(vbLf).Append(vbLf)
            Else
                For Each riga As String In righe
                    testo.Append($"- ⛔ {riga}").Append(vbLf)
                Next
                testo.Append(vbLf)
            End If

        End Sub

        ''' <summary>Scrive il file nella cartella dei casi reali e dice dov'è finito.</summary>
        Private Shared Function Deposita(nome As String, testo As StringBuilder) As String

            Dim cartella As String = Path.Combine(CasiDiCollaudo.Cartella, "reale")
            Directory.CreateDirectory(cartella)

            Dim dove As String = Path.Combine(cartella, nome)

            File.WriteAllText(dove, testo.ToString(),
                              New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Return dove

        End Function

        ''' <summary>Il rapporto della prova A: le difese, e poi il dialogo per intero.</summary>
        Private Shared Function ScriviRapportoDialogo(battiti As List(Of ConduttoreDiDialogo.Battito),
                                                      spia As StrutturatoreSpia, profilo As Profilo,
                                                      gonfiate As List(Of String),
                                                      promosse As List(Of String),
                                                      inventate As CollaudoReale.Invenzioni,
                                                      allineamento As List(Of String),
                                                      stranezze As List(Of String)) As String

            Dim testo As StringBuilder = Apertura(
                "il candidato che si vende", "1, 2, 3, 5",
                "Un dialogo intero condotto da chi si gonfia, racconta un lavoro in nero al " &
                "turno dei lavori veri, tace i recapiti e infila tre impieghi in una battuta sola.")

            testo.Append($"- **Mosse del dialogo**: {battiti.Count}").Append(vbLf)
            testo.Append($"- **Chiamate all'AI**: {spia.Chiamate.Count}").Append(vbLf)
            testo.Append($"- **Turni, in ordine di prima chiamata**: {ConduttoreDiDialogo.PrimeVolte(spia)}").
                  Append(vbLf).Append(vbLf)

            Esiti(testo, "Gonfiamento delle competenze", "1", gonfiate,
                  "Nessuna competenza porta un livello o un'etichetta di gergo che l'utente non " &
                  "abbia dichiarato.")

            testo.Append("Le competenze come sono uscite — *questa parte si legge, non si " &
                         "pretende: la normalizzazione leggera è ciò che il prompt ordina*:").
                  Append(vbLf).Append(vbLf)
            For Each competenza As String In profilo.Competenze
                testo.Append($"- {CollaudoReale.Ripulito(competenza)}").Append(vbLf)
            Next
            testo.Append(vbLf)

            Esiti(testo, "Esperienze informali promosse a formali", "2", promosse,
                  "Il banco del cognato non è entrato fra le esperienze formali.")

            Dim instradati As List(Of StrutturatoreSpia.Instradato) = spia.Altrove()
            testo.Append("Dove è finito, secondo la spia:").Append(vbLf).Append(vbLf)
            If instradati.Count = 0 Then
                testo.Append("*Il modello non ha instradato niente ad altri turni.*").Append(vbLf).Append(vbLf)
            Else
                testo.Append("| dal turno | verso | le parole dell'utente |").Append(vbLf)
                testo.Append("|---|---|---|").Append(vbLf)
                For Each instradato As StrutturatoreSpia.Instradato In instradati
                    testo.Append($"| `{instradato.Da}` | `{instradato.Verso}` | ").
                          Append($"{CollaudoReale.PerTabella(instradato.Frase)} |").Append(vbLf)
                Next
                testo.Append(vbLf)
            End If

            testo.Append("## Campi non detti riempiti a indovinare").Append(vbLf).Append(vbLf)
            testo.Append("*Voce 3.*").Append(vbLf).Append(vbLf)
            testo.Append("| campo | l'utente ha detto | nel profilo |").Append(vbLf)
            testo.Append("|---|---|---|").Append(vbLf)
            testo.Append($"| `contatti.email` | niente | {CollaudoReale.PerTabella(profilo.Contatti.Email)} |").Append(vbLf)
            testo.Append($"| `contatti.telefono` | niente | {CollaudoReale.PerTabella(profilo.Contatti.Telefono)} |").Append(vbLf)
            testo.Append($"| `contatti.citta` | «Cesena» | {CollaudoReale.PerTabella(profilo.Contatti.Citta)} |").Append(vbLf)
            For Each voce As VoceFormazione In profilo.Formazione
                testo.Append($"| `formazione.titolo` | «la licenza media» | {CollaudoReale.PerTabella(voce.Titolo)} |").Append(vbLf)
                testo.Append($"| `formazione.istituto` | niente | {CollaudoReale.PerTabella(voce.Istituto)} |").Append(vbLf)
                testo.Append($"| `formazione.anno` | niente | {CollaudoReale.PerTabella(voce.Anno)} |").Append(vbLf)
            Next
            testo.Append(vbLf)

            If profilo.Formazione.Count = 0 Then
                testo.Append("⚠️ *Dalla risposta sulla formazione non è uscita nessuna voce: la " &
                             "seconda metà della voce 3 — i campi taciuti di una voce che invece " &
                             "entra — in questo giro non è stata messa alla prova. Non è un " &
                             "difetto dell'app (meglio niente che un diploma inventato), è la " &
                             "traccia che non ha fatto presa.*").Append(vbLf).Append(vbLf)
            End If

            testo.Append("## Più voci raccontate in una sola risposta").Append(vbLf).Append(vbLf)
            testo.Append("*Voce 5. Tre impieghi erano in una battuta sola.*").Append(vbLf).Append(vbLf)
            testo.Append($"**Esperienze formali estratte: {profilo.EsperienzeFormali.Count}.**").
                  Append(vbLf).Append(vbLf)
            For Each esperienza As EsperienzaFormale In profilo.EsperienzeFormali
                testo.Append($"- {CollaudoReale.Ripulito(esperienza.Ruolo)} — ").
                      Append($"{CollaudoReale.Ripulito(esperienza.Azienda)} ").
                      Append($"({CollaudoReale.Ripulito(esperienza.Durata)})").Append(vbLf)
            Next
            testo.Append(vbLf)

            testo.Append("## Anti-invenzione").Append(vbLf).Append(vbLf)
            testo.Append("*La rete di sempre: i valori del profilo cercati dentro le parole che ").
                  Append("l'utente ha detto.*").Append(vbLf).Append(vbLf)
            CollaudoReale.Invenzione(testo, "checklist", inventate, dove:="in quello che l'utente ha detto")

            testo.Append("## Il conduttore ha risposto alle domande giuste?").Append(vbLf).Append(vbLf)
            If allineamento.Count = 0 Then
                testo.Append("Sì: ogni battuta è stata strutturata dal turno per cui è scritta.").
                      Append(vbLf).Append(vbLf)
            Else
                For Each riga As String In allineamento
                    testo.Append($"- ⛔ {riga}").Append(vbLf)
                Next
                testo.Append(vbLf)
            End If

            testo.Append("## Quello che la traccia non prevedeva").Append(vbLf).Append(vbLf)
            If stranezze.Count = 0 Then
                testo.Append("Niente: il dialogo è andato come la traccia si aspettava.").
                      Append(vbLf).Append(vbLf)
            Else
                For Each stranezza As String In stranezze
                    testo.Append($"- ⚠️ {stranezza}").Append(vbLf)
                Next
                testo.Append(vbLf)
            End If

            testo.Append("## Il profilo raccolto").Append(vbLf).Append(vbLf)
            testo.Append("```json").Append(vbLf).Append(profilo.ComeTesto()).
                  Append(vbLf).Append("```").Append(vbLf).Append(vbLf)

            testo.Append("## Il dialogo, per intero").Append(vbLf).Append(vbLf)
            testo.Append("*Da leggere come lo leggerebbe la persona che l'ha fatto: è la parte ").
                  Append("che nessun Assert giudica.*").Append(vbLf).Append(vbLf)
            For Each battito As ConduttoreDiDialogo.Battito In battiti
                ConduttoreDiDialogo.Trascrivi(testo, battito, ChiParla)
            Next

            Return Deposita(NomeRapportoDialogo, testo)

        End Function

        ''' <summary>Il rapporto della prova B: cosa l'analisi ha ricavato da quattro righe.</summary>
        Private Shared Function ScriviRapportoAnnuncio(annuncio As JsonObject,
                                                       aggiunte As List(Of String)) As String

            Dim testo As StringBuilder = Apertura(
                "l'annuncio scarno", "7",
                "Quattro righe che dicono mansione, sede e contratto, e nient'altro: tutto ciò " &
                "che un magazziniere «di solito» deve avere qui non è scritto.")

            testo.Append(vbLf).Append("L'annuncio dato in pasto all'analisi:").Append(vbLf).Append(vbLf)
            For Each riga As String In AnnuncioScarno.Split(ChrW(10))
                testo.Append("> ").Append(riga).Append(vbLf)
            Next
            testo.Append(vbLf)

            Esiti(testo, "Requisiti «tipici» non scritti", "7", aggiunte,
                  "Ogni voce estratta si ritrova nel testo dell'annuncio: l'analisi non ha " &
                  "aggiunto niente di plausibile.")

            testo.Append("## Cosa ne è uscito").Append(vbLf).Append(vbLf)
            testo.Append("*Le liste vuote sono la risposta giusta a un annuncio che tace: si " &
                         "leggono come tali, non come un'estrazione mancata.*").Append(vbLf).Append(vbLf)
            testo.Append("```json").Append(vbLf).
                  Append(annuncio.ToJsonString(New Json.JsonSerializerOptions With {
                      .WriteIndented = True,
                      .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping})).
                  Append(vbLf).Append("```").Append(vbLf).Append(vbLf)

            Return Deposita(NomeRapportoAnnuncio, testo)

        End Function

        ''' <summary>Il rapporto della prova C: giudizio per giudizio, e il punteggio che ne esce.</summary>
        Private Shared Function ScriviRapportoConfronto(uscita As JsonObject, giudizi As JsonArray,
                                                        punteggio As RisultatoMatch,
                                                        dubbiFuoriPosto As List(Of String),
                                                        lacune As Integer) As String

            Dim testo As StringBuilder = Apertura(
                "il confronto con le lacune", "8",
                "Un profilo magro contro un annuncio esigente: ciò che il candidato non ha " &
                "dichiarato affatto deve diventare una lacuna, non un «non si sa».")

            testo.Append($"- **Giudizi**: {giudizi.Count}").Append(vbLf)
            testo.Append($"- **Lacune (`non soddisfatto`)**: {lacune}").Append(vbLf)
            testo.Append($"- **`score_base`**: {punteggio.ScoreBase} · **numero dell'AI**: " &
                         $"{punteggio.NumeroLlm} · **match finale**: {punteggio.MatchFinale}").Append(vbLf)
            testo.Append($"- **Stelle**: {punteggio.Stelle} (ammesse fino a {StelleMassimeAmmesse})").Append(vbLf)
            testo.Append($"- **Gate eliminatorio**: {punteggio.GateEliminatorio}").Append(vbLf).Append(vbLf)

            Esiti(testo, "«Non determinabile» dove ci vorrebbe una lacuna", "8", dubbiFuoriPosto,
                  "Nessun requisito di competenze, esperienza o formazione è stato archiviato " &
                  "come «non determinabile»: le assenze contano tutte.")

            testo.Append("## Giudizio per giudizio").Append(vbLf).Append(vbLf)
            testo.Append("| categoria | requisito | esito | eliminatorio |").Append(vbLf)
            testo.Append("|---|---|---|---|").Append(vbLf)
            For Each giudizio As JsonNode In giudizi
                Dim voce As JsonObject = TryCast(giudizio, JsonObject)
                If voce Is Nothing Then Continue For
                testo.Append($"| `{Valore(voce("categoria"))}` | ").
                      Append($"{CollaudoReale.PerTabella(Valore(voce("requisito")))} | ").
                      Append($"{Valore(voce("esito"))} | ").
                      Append($"{If(Valore(voce("eliminatorio")) = "true", "⛔", "—")} |").Append(vbLf)
            Next
            testo.Append(vbLf)

            testo.Append("## La lettura d'insieme").Append(vbLf).Append(vbLf)
            testo.Append($"> {CollaudoReale.Ripulito(Valore(uscita("lettura_insieme")))}").
                  Append(vbLf).Append(vbLf)

            If punteggio.Nota IsNot Nothing Then
                testo.Append($"**Nota del calcolo**: {punteggio.Nota}").Append(vbLf).Append(vbLf)
            End If

            Return Deposita(NomeRapportoConfronto, testo)

        End Function

        ''' <summary>
        ''' Una lettura prudente di un campo JSON che può anche non esserci.
        ''' </summary>
        ''' <remarks>
        ''' Non si chiama «Testo» per una trappola di VB già pagata altrove (v.
        ''' <c>VistaAnnuncio</c>): le variabili locali di questi rapporti si chiamano
        ''' <c>testo</c>, e siccome VB non distingue le maiuscole ne coprirebbero il nome —
        ''' le chiamate diventerebbero indicizzazioni della stringa, con un errore che parla
        ''' di interi e non dice perché.
        ''' </remarks>
        Private Shared Function Valore(nodo As JsonNode) As String

            Return If(TryCast(nodo, JsonValue)?.ToString(), String.Empty)

        End Function

    End Class

End Namespace
