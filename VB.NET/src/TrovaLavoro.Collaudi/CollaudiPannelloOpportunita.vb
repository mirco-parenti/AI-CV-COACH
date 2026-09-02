Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi del pannello P4 (cap. 03.6; cap. 12, A5). Girano <b>senza rete</b>: al
    ''' posto dell'AI c'è una <see cref="PipelineCandidatura"/> coi mestieri finti, che il
    ''' pannello accetta apposta perché il giro completo si possa fare da qui.
    ''' </summary>
    ''' <remarks>
    ''' <para>Le regole del confronto sono già collaudate altrove — la pipeline, il calcolo
    ''' del match, la vista dei giudizi. Qui si verifica soltanto quello che il pannello
    ''' aggiunge: che i due passi partano insieme, che quello che arriva finisca a video
    ''' <i>tutto</i>, che l'opportunità venga scritta nella sua cartella, e che quando
    ''' qualcosa non si può fare il motivo sia detto dov'è successo.</para>
    ''' <para>I clic si simulano chiamando i metodi che i gestori chiamerebbero: di un
    ''' <c>Async Sub</c> non si può aspettare la fine.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiPannelloOpportunita

        Private Const TestoIncollato As String = "Cercasi autista con patente C a Forlì."

        Private Const AnnuncioLetto As String =
            "{""titolo"": ""Autista consegne"", ""azienda"": ""Rossi S.p.A."", ""sede"": [""Forlì""]," &
            """competenze_richieste"": [{""testo"": ""Uso del muletto"", ""priorita"": ""richiesto""}]}"

        ''' <summary>Un confronto con un paletto scoperto: gap da mitigare e tetto al punteggio.</summary>
        Private Const ConfrontoConPaletto As String =
            "{""giudizi"": [" &
            "{""requisito"": ""Patente C"", ""categoria"": ""altri_requisiti"", ""priorita"": ""richiesto""," &
            """esito"": ""non soddisfatto"", ""eliminatorio"": true," &
            """spiegazione"": ""Il profilo dichiara la sola patente B.""}," &
            "{""requisito"": ""Uso del muletto"", ""categoria"": ""competenze"", ""priorita"": ""richiesto""," &
            """esito"": ""soddisfatto"", ""eliminatorio"": false, ""spiegazione"": ""Dichiarato nel profilo.""}]," &
            """lettura_insieme"": ""Bene il magazzino, manca la patente C."", ""numero_complessivo"": 45}"

        ''' <summary>
        ''' Lo schema con tutti i campi vuoti: è quel che il prompt restituisce quando il
        ''' testo non è un annuncio di lavoro — una pagina di elenco, una home, un login.
        ''' </summary>
        Private Const SchemaVuoto As String =
            "{""competenze_richieste"": [], ""esperienza_richiesta"": [], ""formazione_richiesta"": []," &
            """altri_requisiti"": [], ""titolo"": """", ""azienda"": """", ""sede"": []," &
            """contratto"": {}, ""mansioni"": [], ""benefit"": []}"

        ''' <summary>Un confronto pieno: nessun gap, nessuna mitigazione da chiedere.</summary>
        Private Const ConfrontoPieno As String =
            "{""giudizi"": [" &
            "{""requisito"": ""Uso del muletto"", ""categoria"": ""competenze"", ""priorita"": ""richiesto""," &
            """esito"": ""soddisfatto"", ""eliminatorio"": false, ""spiegazione"": ""Dichiarato nel profilo.""}]," &
            """lettura_insieme"": ""In linea su tutto."", ""numero_complessivo"": 95}"

        ' ==================================================================
        ' Il giro dei due passi
        ' ==================================================================

        <TestMethod>
        Public Async Function AnalizzaFaIDuePassiEMostraTutto() As Task

            ' Fra l'annuncio e il confronto l'utente non decide niente: un bottone solo,
            ' due passi (cap. 12, A5→A7).
            Dim confrontatore As New ConfrontatoreFinto

            Await ConPannelloAsync(
                PipelineFinta(confrontatore:=confrontatore),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Assert.AreEqual("confronto → mitigazione", confrontatore.LavoriChiesti(),
                                    "il confronto è partito, e il gap ha chiesto anche le mitigazioni")

                    Assert.Contains("Titolo: Autista consegne", Casella(pannello, "txtAnnuncioLetto").Text,
                                    "l'annuncio come l'AI l'ha capito")
                    Assert.HasCount(2, Giudizi(pannello), "i due giudizi, uno per riga")
                    Assert.Contains("su 5", Etichetta(pannello, "lblStelle").Text, "le stelle in cima")
                    Assert.Contains("Bene il magazzino", Casella(pannello, "txtSpiegazione").Text,
                                    "e la lettura d'insieme in fondo")
                End Function)

        End Function

        <TestMethod>
        Public Async Function AConfrontoFattoLaFasciaDIngressoSiRichiude() As Task

            ' Cap. 03.6: a cattura avvenuta la fascia si richiude, per non rubare spazio
            ' ai giudizi — e «Nuovo annuncio» la riapre quando serve.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Dim ingresso As Panel = Riquadro(pannello, "pnlIngresso")
                    Assert.IsTrue(ingresso.Visible, "all'inizio è aperta: è l'unica porta")

                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)
                    Assert.IsFalse(ingresso.Visible, "a confronto fatto si richiude")

                    Bottone(pannello, "btnNuovoAnnuncio").PerformClick()
                    Assert.IsTrue(ingresso.Visible, "e si riapre per il prossimo")
                    Assert.IsEmpty(Casella(pannello, "txtAnnuncio").Text, "con la casella pulita")
                End Function)

        End Function

        <TestMethod>
        Public Async Function LOpportunitaFinisceNellaSuaCartella() As Task

            ' Un documento generato e non ritrovabile domani sarebbe perso: la cartella
            ' nasce subito, col nome parlante (cap. 11.1).
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Dim cartella As String = pannello.Candidatura.Cartella
                    Assert.IsTrue(Directory.Exists(cartella), "la cartella c'è")
                    Assert.Contains("rossi-s-p-a", Path.GetFileName(cartella), "e ha il nome parlante")

                    Assert.IsTrue(File.Exists(Path.Combine(cartella, ArchivioOpportunita.FileGiudizi)),
                                  "coi giudizi dentro")
                    Assert.Contains("Salvata in", Etichetta(pannello, "lblStatoOpportunita").Text,
                                    "e la barra dice dove")
                End Function)

        End Function

        <TestMethod>
        Public Async Function LOpportunitaAnnotaLaVersioneDiProfilo() As Task

            ' È ciò che tiene spiegabile un CV già inviato anche a profilo evoluto
            ' (cap. 11.1): il confronto dice con quale profilo è stato fatto.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Assert.AreEqual(contesto.Archivio.Versioni().Last(), pannello.Candidatura.VersioneProfilo,
                                    "la versione corrente del profilo")
                End Function)

        End Function

        ' ==================================================================
        ' Quel che si vede: paletti, note, spiegazioni
        ' ==================================================================

        <TestMethod>
        Public Async Function IlPalettoScopertoSiVedeDaLontano() As Task

            ' Cap. 12.7, onestà visibile: il ⛔ e il colore dicono qual è la voce che ha
            ' craterato il match, prima ancora che si legga la nota.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Dim paletto As ListViewItem = Giudizi(pannello)(0)
                    Assert.AreEqual("✗", paletto.SubItems(0).Text, "il segno dell'esito")
                    Assert.Contains("⛔", paletto.SubItems(1).Text, "il paletto in coda al requisito")
                    Assert.AreEqual(StileApp.Pericolo, paletto.ForeColor, "e la riga si tinge")

                    Assert.Contains("Requisito eliminatorio", Etichetta(pannello, "lblNota").Text,
                                    "la nota spiega il tetto")
                    Assert.AreEqual(StileApp.Pericolo, Etichetta(pannello, "lblNota").ForeColor,
                                    "col colore di ciò che pesa")
                End Function)

        End Function

        <TestMethod>
        Public Async Function ScegliendoUnaVoceSiLeggeIlPerche() As Task

            ' È il punto in cui l'utente verifica che il giudizio sia ancorato al profilo
            ' e non inventato.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Dim riquadro As TextBox = Casella(pannello, "txtSpiegazione")
                    Assert.Contains("Bene il magazzino", riquadro.Text, "prima c'è la lettura d'insieme")

                    Giudizi(pannello)(0).Selected = True
                    Assert.AreEqual("Perché", Etichetta(pannello, "lblEtichettaSpiegazione").Text,
                                    "il riquadro cambia mestiere")
                    Assert.Contains("la sola patente B", riquadro.Text, "e racconta quella voce")

                    Giudizi(pannello)(0).Selected = False
                    Assert.Contains("Bene il magazzino", riquadro.Text, "lasciandola, torna l'insieme")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SottoSogliaLaCandidaturaSiSconsigliaSenzaImpedirla() As Task

            ' Cap. 12, A5.3: si sconsiglia, mai si impedisce — la scelta resta dell'utente.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Assert.IsLessThan(1.5, VistaConfronto.Da(pannello.Candidatura).Stelle.Value,
                                      "il paletto ha craterato il match")
                    Assert.Contains("poco spendibili", Etichetta(pannello, "lblNota").Text,
                                    "e il pannello lo dice con parole, non con un divieto")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaPalettiNonSiSconsigliaNiente() As Task

            Await ConPannelloAsync(
                PipelineFinta(confronto:=ConfrontoPieno),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Assert.IsEmpty(Etichetta(pannello, "lblNota").Text, "niente da avvisare")
                    Assert.HasCount(1, Giudizi(pannello), "e il solo giudizio che c'era")
                End Function)

        End Function

        ' ==================================================================
        ' Quando qualcosa non si può fare
        ' ==================================================================

        <TestMethod>
        Public Async Function SenzaChiaveNonSiAnalizzaELoDice() As Task

            ' L'annuncio e il confronto passano entrambi dall'AI: senza chiave non si
            ' comincia, e la ragione va detta dov'è successo (cap. 03.8).
            Await ConPannelloAsync(
                Nothing,
                Async Function(pannello, contesto)
                    Dim stato As Label = Etichetta(pannello, "lblStatoOpportunita")
                    Assert.Contains("chiave API", stato.Text, "lo dice appena si apre")
                    Assert.AreEqual(StileApp.Pericolo, stato.ForeColor, "col colore di chi non può funzionare")

                    Casella(pannello, "txtAnnuncio").Text = TestoIncollato
                    Assert.IsFalse(Bottone(pannello, "btnAnalizza").Enabled, "e il bottone resta spento")

                    Await pannello.AnalizzaLAnnuncioAsync()
                    Assert.IsNull(pannello.Candidatura, "niente è stato analizzato")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaProfiloNonCEnienteDaConfrontare() As Task

            ' Il confronto è fra due cose: senza la prima non si parte, e si dice da dove
            ' si comincia (cap. 12, A2).
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Assert.Contains("scheda «Profilo»", Etichetta(pannello, "lblStatoOpportunita").Text,
                                    "manda dove si comincia")

                    Casella(pannello, "txtAnnuncio").Text = TestoIncollato
                    Assert.IsFalse(Bottone(pannello, "btnAnalizza").Enabled, "e intanto non si analizza")
                End Function,
                conProfilo:=False)

        End Function

        <TestMethod>
        Public Async Function IlProfiloNatoDopoSiVedeAppenaSiTornaInVista() As Task

            ' È il primo giro vero, quello del primo avvio: si importa il CV in P2, lo si
            ' salva, e si viene qui. Il pannello si era collegato all'avvio, quando un
            ' profilo non c'era: senza tornare a chiedere direbbe «prima serve il profilo»
            ' per un profilo appena salvato, e terrebbe «Analizza» spento (cap. 12, A2→A5).
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Dim stato As Label = Etichetta(pannello, "lblStatoOpportunita")
                    Assert.Contains("scheda «Profilo»", stato.Text, "all'avvio un profilo non c'era")

                    Casella(pannello, "txtAnnuncio").Text = TestoIncollato
                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))

                    ' Il gesto della finestra, che mostra un pannello solo per volta.
                    pannello.Visible = False
                    pannello.Visible = True

                    Assert.Contains("Incolla il testo di un annuncio", stato.Text, "adesso il profilo c'è")
                    Assert.IsTrue(Bottone(pannello, "btnAnalizza").Enabled, "e si può analizzare")

                    Await Task.CompletedTask
                End Function,
                conProfilo:=False)

        End Function

        <TestMethod>
        Public Async Function TornandoInVistaAConfrontoFattoIlRaccontoResta() As Task

            ' Risvegliarsi non vuol dire ricominciare: se un confronto c'è già, quella riga
            ' dice a che punto siamo, e riscriverla come all'inizio sarebbe una bugia.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Dim stato As Label = Etichetta(pannello, "lblStatoOpportunita")
                    Dim raccontoDopoIlConfronto As String = stato.Text
                    Assert.Contains("Confronto fatto", raccontoDopoIlConfronto, "il match, in una riga")

                    pannello.Visible = False
                    pannello.Visible = True

                    Assert.AreEqual(raccontoDopoIlConfronto, stato.Text, "e torna in vista dicendo la stessa cosa")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaTestoIlBottoneRestaSpento() As Task

            ' Un bottone acceso che poi risponde «incolla qualcosa» è una promessa non
            ' mantenuta: meglio spento finché non c'è niente da leggere.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Assert.IsFalse(Bottone(pannello, "btnAnalizza").Enabled, "casella vuota, bottone spento")

                    Casella(pannello, "txtAnnuncio").Text = "   "
                    Assert.IsFalse(Bottone(pannello, "btnAnalizza").Enabled, "e i soli spazi non contano")

                    Casella(pannello, "txtAnnuncio").Text = TestoIncollato
                    Assert.IsTrue(Bottone(pannello, "btnAnalizza").Enabled, "col testo si accende")

                    Await Task.CompletedTask
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnErroreDellAiSiRaccontaEIlTestoRestaDovE() As Task

            ' Il messaggio è già scritto per l'utente; e ciò che ha incollato non si
            ' butta, così riprovare non costa di ritrovarlo.
            Dim analizzatore As New AnalizzatoreFinto
            analizzatore.Fallira(New ErroreAi(CausaErroreAi.Rete,
                                              "Non sono riuscita a leggere l'annuncio: riprova."))

            Await ConPannelloAsync(
                New PipelineCandidatura(analizzatore, New ConfrontatoreFinto, New GeneratoreFinto),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Dim stato As Label = Etichetta(pannello, "lblStatoOpportunita")
                    Assert.Contains("Non sono riuscita a leggere l'annuncio", stato.Text, "lo dice con le sue parole")
                    Assert.AreEqual(StileApp.Pericolo, stato.ForeColor, "col colore dell'errore")

                    Assert.AreEqual(TestoIncollato, Casella(pannello, "txtAnnuncio").Text,
                                    "e il testo incollato è ancora lì")
                    Assert.IsTrue(Riquadro(pannello, "pnlIngresso").Visible, "con la fascia ancora aperta")
                    Assert.IsNull(pannello.Candidatura, "niente opportunità a metà")
                End Function)

        End Function

        ' ==================================================================
        ' Riaprire e scartare (T5c)
        ' ==================================================================

        <TestMethod>
        Public Async Function UnaCandidaturaSiRiapreComEra() As Task

            ' È la strada che arriva dalla coda della Home, e chiude il debito di T4: si
            ' rientra in una candidatura senza pagare una riga all'AI (cap. 12.7).
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)
                    Dim dove As String = pannello.Candidatura.Cartella

                    ' Un pannello nuovo, come dopo aver chiuso e riaperto l'applicazione.
                    Using riaperto As New PannelloOpportunita()
                        riaperto.CreateControl()
                        riaperto.Collega(contesto, PipelineFinta())

                        riaperto.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                        Assert.HasCount(2, Giudizi(riaperto), "i giudizi di allora, tutti")
                        Assert.Contains("Autista consegne", Casella(riaperto, "txtAnnuncioLetto").Text)
                        Assert.Contains("su 5", Etichetta(riaperto, "lblStelle").Text, "e le stelle")
                        Assert.AreEqual("Interessante", Etichetta(riaperto, "lblStatoCandidatura").Text.
                                        Split(" "c)(0), "con lo stato a cui era arrivata")
                        Assert.IsFalse(Riquadro(riaperto, "pnlIngresso").Visible,
                                       "la fascia d'ingresso resta chiusa: non c'è niente da incollare")
                    End Using

                End Function)

        End Function

        <TestMethod>
        Public Async Function SuUnaCandidaturaScartataNonSiLavoraPiu() As Task

            ' Lo scarto è un capolinea (cap. 07.3): niente documenti, e niente da scartare
            ' una seconda volta. Il bottone non si preme qui — chiede conferma con una
            ' finestra — ma la sua condizione sì.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Assert.IsTrue(Bottone(pannello, "btnScarta").Enabled, "prima si può scartare")

                    Dim candidatura As Opportunita = pannello.Candidatura
                    candidatura.Avanza(StatoOpportunita.Scartata)
                    pannello.RiapriLaCandidatura(candidatura)

                    Assert.IsFalse(Bottone(pannello, "btnScarta").Enabled, "poi non più")
                    Assert.IsFalse(Bottone(pannello, "btnGeneraDocumenti").Enabled,
                                   "e non le si scrive un CV")
                    Assert.Contains("Scartata", Etichetta(pannello, "lblStatoCandidatura").Text)

                End Function)

        End Function

        <TestMethod>
        Public Sub LoScartoDiceCheCosaNonSuccede()

            ' Il testo della conferma di livello 5, che il banco legge da qui perché di
            ' una finestra modale non può aspettare la chiusura (come in P1). «Scarta» è
            ' una parola che fa temere una cancellazione: la prima cosa da dire è che qui
            ' non sparisce niente, la seconda che dallo scarto non si torna indietro.
            Dim domanda As String = PannelloOpportunita.SpiegazioneDelloScarto()

            Assert.Contains("Non cancello niente", domanda, "quel che non succede, per primo")
            Assert.Contains("Home", domanda, "e dove la si ritrova")
            Assert.Contains("non si torna indietro", domanda, "ma anche che è un capolinea")

        End Sub

        <TestMethod>
        Public Async Function UnaCandidaturaAnalizzataEntraSubitoNellIndice() As Task

            ' Il registro è un indice rigenerabile, ma tenerlo in riga strada facendo è
            ' quello che fa comparire la candidatura nella Home senza rileggere tutto.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Dim indice As Registro = contesto.Registro.Carica()

                    Assert.IsFalse(indice.Rigenerato, "l'indice su disco è già quello giusto")
                    Assert.AreEqual("Rossi S.p.A.", indice.Voci.Single().Azienda)
                    Assert.AreEqual(StatoOpportunita.Interessante, indice.Voci.Single().Stato)

                End Function)

        End Function

        ' ==================================================================
        ' Il bottone del ragionamento (T7c)
        ' ==================================================================

        <TestMethod>
        Public Async Function SenzaUnConfrontoIlRagionamentoRestaSpento() As Task

            ' Prima di analizzare non c'è ancora niente su cui ragionare: il prompt del
            ' brainstorming vuole i giudizi (cap. 12, A6.2).
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Assert.IsFalse(Bottone(pannello, "btnBrainstorm").Enabled,
                                   "niente ancora da confrontare")

                    Await Task.CompletedTask
                End Function)

        End Function

        <TestMethod>
        Public Async Function DopoUnConfrontoRiuscitoSiPuoRagionare() As Task

            ' A giudizi in mano il ragionamento ha di che partire (cap. 12, A5→A6).
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Assert.IsTrue(Bottone(pannello, "btnBrainstorm").Enabled,
                                  "il confronto c'è, si può ragionare")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaAiIlRagionamentoRestaSpentoAncheAConfrontoFatto() As Task

            ' «Genera documenti» si riapre anche senza rete: i documenti stanno già nel
            ' confronto scritto su disco. Ragionare invece è un giro nuovo dall'AI — qui
            ' serve davvero, a differenza della riapertura dei documenti — e senza
            ' pipeline quel giro non si può fare, anche col confronto già completo.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)
                    Dim dove As String = pannello.Candidatura.Cartella

                    Using senzaAi As New PannelloOpportunita()
                        senzaAi.CreateControl()
                        senzaAi.Collega(contesto, pipeline:=Nothing)

                        senzaAi.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                        Assert.IsTrue(Bottone(senzaAi, "btnGeneraDocumenti").Enabled,
                                      "i documenti si riaprono anche senza rete")
                        Assert.IsFalse(Bottone(senzaAi, "btnBrainstorm").Enabled,
                                       "ma ragionare chiede una chiamata vera all'AI")
                    End Using

                End Function)

        End Function

        <TestMethod>
        Public Async Function SuUnaCandidaturaScartataNonSiRagionaPiu() As Task

            ' Lo scarto è un capolinea (cap. 07.3): sulla stessa candidatura per cui non
            ' si scrive più un CV non ha senso nemmeno ragionare.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)
                    Assert.IsTrue(Bottone(pannello, "btnBrainstorm").Enabled, "prima si può ragionare")

                    Dim candidatura As Opportunita = pannello.Candidatura
                    candidatura.Avanza(StatoOpportunita.Scartata)
                    pannello.RiapriLaCandidatura(candidatura)

                    Assert.IsFalse(Bottone(pannello, "btnBrainstorm").Enabled, "poi non più")

                End Function)

        End Function

        <TestMethod>
        Public Async Function IlBottoneBrainstormChiedeAllaFinestraDiRagionare() As Task

            ' Il pannello non conosce P5: dice solo che vuole ragionare, ed è la finestra
            ' a portarci in vista.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Dim chiesto As Integer = 0
                    AddHandler pannello.BrainstormRichiesto, Sub(mittente, argomenti) chiesto += 1

                    Bottone(pannello, "btnBrainstorm").PerformClick()

                    Assert.AreEqual(1, chiesto, "il pannello ha chiesto di ragionare")
                End Function)

        End Function

        ' ==================================================================
        ' Lo spazio del logo
        ' ==================================================================

        <TestMethod>
        Public Sub LaFasciaDelleAzioniLasciaIlPostoAlLogo()

            ' Lo stesso vincolo geometrico di P2 e P5 (cap. 03.5): sotto il logo non va
            ' niente di vivo, e a cedere il posto è la fascia dei bottoni.
            Using pannello As New PannelloOpportunita()
                pannello.ImpostaIngombroLogo(New Size(261, 188))

                Dim azioni As Panel = Riquadro(pannello, "pnlAzioni")

                Assert.AreEqual(188, azioni.Height, "alta quanto il logo sfonda nell'area centrale")
                Assert.AreEqual(273, azioni.Padding.Left, "e i bottoni cominciano dopo la sua larghezza")
                Assert.IsGreaterThanOrEqualTo(273, Bottone(pannello, "btnNuovoAnnuncio").Left,
                                              "nessun bottone sotto il logo")
            End Using

        End Sub


        ''' <summary>
        ''' Ogni comando del pannello dice per intero il proprio nome.
        ''' </summary>
        ''' <remarks>
        ''' Nato il 2026-08-30, quando «Nuovo annuncio» è diventato «Incolla annuncio
        ''' manualmente»: settantasei pixel di testo in più su un bottone largo 130. Un
        ''' Button non manda a capo e non mette i puntini — taglia, e l'unico segno è mezza
        ''' parola mancante a video. La fascia dei comandi manda a capo la fila se i bottoni
        ''' non ci stanno in larghezza, quindi allargarne uno non rovina la disposizione: è
        ''' il testo dentro il bottone a doverci stare, e qui si misura.
        ''' </remarks>
        <TestMethod>
        Public Sub OgniComandoDiceIlProprioNomePerIntero()

            Using pannello As New PannelloOpportunita()

                Dim comandi As Button() = Riquadro(pannello, "pnlAzioni").
                    Controls.OfType(Of Button)().ToArray()

                Assert.IsGreaterThan(0, comandi.Length, "la fascia dei comandi non è vuota")

                For Each comando As Button In comandi

                    Assert.IsLessThanOrEqualTo(
                        comando.Width,
                        TextRenderer.MeasureText(comando.Text, comando.Font).Width,
                        $"«{comando.Text}» non ci sta nel suo bottone")

                Next

            End Using

        End Sub

        ' ==================================================================
        ' Il banco
        ' ==================================================================

        ' ==================================================================
        ' L'annuncio catturato dal browser (cap. 06.4; cap. 12, A4)
        ' ==================================================================

        <TestMethod>
        Public Async Function IlCatturatoSiVedeEPortaConSeLaProvenienza() As Task

            Await ConPannelloAsync(PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Await pannello.AnalizzaIlCatturatoAsync(
                        TestoIncollato, "Indeed", "https://it.indeed.com/viewjob?jk=9f3c1a")

                    ' Il testo catturato entra dalla porta di sempre, e si vede: chi guarda
                    ' deve poter leggere quello che è stato mandato all'AI.
                    Assert.AreEqual(TestoIncollato, Casella(pannello, "txtAnnuncio").Text)

                    Assert.AreEqual("Indeed", pannello.Candidatura.Fonte)
                    Assert.AreEqual("https://it.indeed.com/viewjob?jk=9f3c1a", pannello.Candidatura.Link)

                    ' E la provenienza arriva su disco insieme al resto.
                    Dim stato As String = File.ReadAllText(Path.Combine(
                        pannello.Candidatura.Cartella, ArchivioOpportunita.FileStato))
                    Assert.Contains("Indeed", stato)
                    Assert.Contains("viewjob", stato)

                End Function)

        End Function

        <TestMethod>
        Public Async Function IlCatturatoSiLeggeConGliACapoAlLoroPosto() As Task

            ' T9d (2026-08-22), trovato nella prova dal vivo: il lettore di pagine cuce i
            ' pezzi con \n (cap. 06.4) e una casella multiriga di Windows i \n non li
            ' mostra — una pagina intera arrivava in un blocco unico e illeggibile, proprio
            ' dove si promette all'utente che potrà rileggerla e correggerla. Il difetto
            ' viveva solo a video: il testo mandato all'AI era già giusto.
            Await ConPannelloAsync(PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Await pannello.AnalizzaIlCatturatoAsync(
                        "Addetto spedizioni" & vbCrLf & "Atena Service" & vbLf & "Rapallo, Liguria",
                        "Indeed", "https://it.indeed.com/viewjob?jk=9f3c1a")

                    Dim aVideo As String = Casella(pannello, "txtAnnuncio").Text

                    Assert.Contains("Addetto spedizioni" & vbCrLf & "Atena Service", aVideo,
                                    "le righe restano righe")
                    Assert.HasCount(3, aVideo.Split(New String() {vbCrLf}, StringSplitOptions.None),
                                    "tre righe, non un blocco solo")
                    ' Il testo arriva misto di proposito: è cucito da più pezzi di pagina,
                    ' e chi converte i \n senza passare prima da lì riempie di righe vuote
                    ' quello che i CRLF ce li aveva già.
                    Assert.DoesNotContain(vbCrLf & vbCrLf, aVideo,
                                          "e nessuna riga vuota inventata da una doppia conversione")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaPaginaDiElencoNonDiventaUnaCandidatura() As Task

            ' Lo schema tutto vuoto è il modo in cui il prompt dice «questo non è un
            ' annuncio» (cap. 06.4).
            Dim chiConfronta As New ConfrontatoreFinto

            Await ConPannelloAsync(PipelineFinta(confrontatore:=chiConfronta, annuncio:=SchemaVuoto),
                Async Function(pannello, contesto) As Task

                    Await pannello.AnalizzaIlCatturatoAsync(
                        TestoIncollato, "Indeed", "https://it.indeed.com/jobs?q=magazziniere")

                    Assert.Contains("ricattura", Etichetta(pannello, "lblStatoOpportunita").Text,
                                    "si rimanda al singolo annuncio")

                    ' Ci si ferma prima del confronto: non si paga una seconda chiamata
                    ' per confrontare il profilo con niente.
                    Assert.IsEmpty(chiConfronta.Chiamate, "il confronto non è stato chiesto")

                    ' E non si scrive su disco una candidatura che non esiste.
                    Assert.IsEmpty(contesto.Opportunita.Elenco(), "niente cartelle-opportunità")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnTestoIncollatoCheNonEUnAnnuncioSiDiceInModoDiverso() As Task

            Await ConPannelloAsync(PipelineFinta(annuncio:=SchemaVuoto),
                Async Function(pannello, contesto) As Task

                    Await IncollaEAnalizzaAsync(pannello, "Ricetta della torta di mele.")

                    ' Chi non ha aperto nessuna pagina non può «ricatturare»: il consiglio
                    ' deve essere uno che si può seguire.
                    Dim detto As String = Etichetta(pannello, "lblStatoOpportunita").Text
                    Assert.Contains("incollato", detto)
                    Assert.DoesNotContain("ricattura", detto)

                End Function)

        End Function

        ' ==================================================================
        ' La riaperta che aspetta solo il confronto (T9c)
        ' ==================================================================

        ''' <summary>
        ''' Una candidatura ferma al solo annuncio, come la scrive il server MCP: niente
        ''' giudizi, niente stelle, stato «nuova» (cap. 09.3).
        ''' </summary>
        Private Shared Function SoloLAnnuncio() As Opportunita

            Return New Opportunita With {
                .Creata = New Date(2026, 8, 20, 9, 0, 0),
                .Annuncio = System.Text.Json.Nodes.JsonNode.Parse(AnnuncioLetto)}

        End Function

        <TestMethod>
        Public Async Function LaRiapertaSenzaGiudiziSiConfrontaSenzaRileggereLAnnuncio() As Task

            ' Il vicolo cieco trovato dal collaudo di tappa di T8: si riapriva e non si
            ' poteva proseguire. Adesso «Analizza» cambia mestiere.
            Dim confrontatore As New ConfrontatoreFinto

            Await ConPannelloAsync(
                PipelineFinta(confrontatore:=confrontatore),
                Async Function(pannello, contesto) As Task

                    Dim dove As String = contesto.Opportunita.Salva(SoloLAnnuncio())
                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    Dim analizza As Button = Bottone(pannello, "btnAnalizza")
                    Assert.AreEqual("Confronta", analizza.Text, "il bottone dice l'unico passo che manca")
                    Assert.IsTrue(analizza.Enabled, "e si può premere con la casella vuota")

                    Await pannello.ConfrontaLaRiapertaAsync()

                    Assert.AreEqual("confronto → mitigazione", confrontatore.LavoriChiesti(),
                                    "l'annuncio non si rilegge: era già strutturato")
                    Assert.HasCount(2, Giudizi(pannello), "i giudizi arrivano a video")

                    Dim riletta As Opportunita = contesto.Opportunita.Carica(dove)
                    Assert.AreEqual(StatoOpportunita.Interessante, riletta.Stato,
                                    "e la candidatura avanza nella sua cartella")
                    Assert.IsTrue(riletta.Confrontata)

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnTestoIncollatoHaLaPrecedenzaSulConfronto() As Task

            ' Chi scrive nella casella vuole leggere un annuncio nuovo, non ripescare
            ' quello di prima: il bottone torna a essere «Analizza».
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Dim dove As String = contesto.Opportunita.Salva(SoloLAnnuncio())
                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    Casella(pannello, "txtAnnuncio").Text = TestoIncollato
                    Assert.AreEqual("Analizza", Bottone(pannello, "btnAnalizza").Text)

                    Casella(pannello, "txtAnnuncio").Text = ""
                    Assert.AreEqual("Confronta", Bottone(pannello, "btnAnalizza").Text,
                                    "svuotata la casella, torna quello che mancava")

                    Await Task.CompletedTask

                End Function)

        End Function

        <TestMethod>
        Public Async Function SuUnaScartataSenzaGiudiziNonSiConfrontaPiu() As Task

            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Dim persa As Opportunita = SoloLAnnuncio()
                    persa.Avanza(StatoOpportunita.Scartata)
                    Dim dove As String = contesto.Opportunita.Salva(persa)

                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    Assert.AreEqual("Analizza", Bottone(pannello, "btnAnalizza").Text,
                                    "su una candidatura chiusa non si offre di proseguirla")
                    Assert.IsFalse(Bottone(pannello, "btnAnalizza").Enabled)

                    Await Task.CompletedTask

                End Function)

        End Function

        ' ==================================================================
        ' Il riconfronto: quando il profilo non è più quello (2026-09-02)
        ' ==================================================================

        ''' <summary>Una candidatura confrontata con una certa versione di profilo.</summary>
        Private Shared Function ConfrontataCon(versione As String) As Opportunita

            Dim o As Opportunita = SoloLAnnuncio()
            o.Confronto = System.Text.Json.Nodes.JsonNode.Parse(ConfrontoPieno)
            o.VersioneProfilo = versione
            o.Avanza(StatoOpportunita.Interessante, o.Creata.AddMinutes(2))

            Return o

        End Function

        ''' <summary>Salva un'altra versione del profilo: da qui in poi quella di prima è vecchia.</summary>
        Private Shared Sub IlProfiloCambia(contesto As ContestoApp)
            contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))
        End Sub

        <TestMethod>
        Public Async Function IlProfiloCresciutoAccendeIlRiconfronto() As Task

            ' Il difetto trovato usando il programma il 2026-09-02: cambiata la patente, le
            ' candidature di prima mostravano le stelle di allora e non c'era modo di
            ' rifarle. Adesso «Analizza» prende il suo quarto mestiere.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Dim diAllora As String = contesto.Archivio.Versioni().Last()
                    Dim dove As String = contesto.Opportunita.Salva(ConfrontataCon(diAllora))

                    IlProfiloCambia(contesto)

                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    Dim analizza As Button = Bottone(pannello, "btnAnalizza")
                    Assert.AreEqual("Riconfronta", analizza.Text,
                                    "il bottone dice il mestiere che serve adesso")
                    Assert.IsTrue(analizza.Enabled,
                                  "e si preme con la casella vuota: l'annuncio è già nella cartella")

                End Function)

        End Function

        <TestMethod>
        Public Async Function AProfiloFermoIlRiconfrontoNonSiPropone() As Task

            ' Un «Riconfronta» sempre acceso inviterebbe a ripagare una risposta che non
            ' cambierebbe: a parità di profilo e di annuncio il confronto è lo stesso.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Dim dove As String = contesto.Opportunita.Salva(
                        ConfrontataCon(contesto.Archivio.Versioni().Last()))

                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    Dim analizza As Button = Bottone(pannello, "btnAnalizza")
                    Assert.AreNotEqual("Riconfronta", analizza.Text,
                                       "niente da rifare: la candidatura è in pari col profilo")
                    Assert.IsFalse(analizza.Enabled, "e il bottone resta spento com'era")

                End Function)

        End Function

        <TestMethod>
        Public Async Function IlRiconfrontoRifaIGiudiziSenzaRileggereLAnnuncio() As Task

            ' La ragione per cui il gesto vale la pena: l'annuncio è già letto e
            ' strutturato: rifarlo da capo costerebbe una chiamata in più e chiederebbe
            ' all'utente di ricopiare una cosa che il programma ha già.
            Dim confrontatore As New ConfrontatoreFinto

            Await ConPannelloAsync(
                PipelineFinta(confrontatore:=confrontatore),
                Async Function(pannello, contesto) As Task

                    Dim dove As String = contesto.Opportunita.Salva(
                        ConfrontataCon(contesto.Archivio.Versioni().Last()))

                    IlProfiloCambia(contesto)

                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))
                    Await pannello.ConfrontaLaRiapertaAsync()

                    Assert.AreEqual("confronto → mitigazione", confrontatore.LavoriChiesti(),
                                    "l'annuncio non si rilegge: era già strutturato")

                    Dim riletta As Opportunita = contesto.Opportunita.Carica(dove)
                    Assert.AreEqual(contesto.Archivio.Versioni().Last(), riletta.VersioneProfilo,
                                    "e la candidatura torna in pari col profilo di oggi")

                End Function)

        End Function

        <TestMethod>
        Public Async Function LaGiaSpeditaTieneIlGestoMaNonLoPropone() As Task

            ' La cautela annotata in idee_future.md il 2026-08-24: rifare i giudizi cambia le
            ' stelle di una candidatura che l'utente potrebbe aver già mandato, e il registro
            ' non ne tiene uno storico — le rilegge dalle cartelle. Il gesto resta per chi lo
            ' vuole (la conferma dice cosa si perde), ma la finestra non si apre da sola: quella
            ' decisione è già stata presa, e lì l'utente sta solo riguardando.
            '
            ' RISERVA: il banco vede il bottone, non la finestra — che senza un Form attorno al
            ' pannello non si apre affatto. Che non compaia sulla già spedita, e che la conferma
            ' avverta della perdita, restano da provare a mano (v. in_sospeso.md).
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Dim spedita As Opportunita = ConfrontataCon(contesto.Archivio.Versioni().Last())
                    spedita.Avanza(StatoOpportunita.Generata, spedita.Creata.AddMinutes(9))
                    spedita.Avanza(StatoOpportunita.Inviata, spedita.Creata.AddMinutes(20))

                    Dim dove As String = contesto.Opportunita.Salva(spedita)

                    IlProfiloCambia(contesto)

                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    Assert.AreEqual("Riconfronta", Bottone(pannello, "btnAnalizza").Text,
                                    "il gesto resta a disposizione anche a candidatura partita")

                End Function)

        End Function

        <TestMethod>
        Public Async Function LaScartataNonSiRiconfronta() As Task

            ' Quella è chiusa (cap. 07.3): rifarle i conti sarebbe lavorare per niente, la
            ' stessa ragione per cui non le si scrive un CV.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Dim scartata As Opportunita = ConfrontataCon(contesto.Archivio.Versioni().Last())
                    scartata.Avanza(StatoOpportunita.Scartata, scartata.Creata.AddMinutes(5))

                    Dim dove As String = contesto.Opportunita.Salva(scartata)

                    IlProfiloCambia(contesto)

                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    Assert.AreNotEqual("Riconfronta", Bottone(pannello, "btnAnalizza").Text,
                                       "sulla scartata il gesto non si propone")

                End Function)

        End Function

        ' ==================================================================
        ' Com'è andata (T9c, cap. 07.3)
        ' ==================================================================

        ''' <summary>Una candidatura arrivata fino all'invio, come dopo P7.</summary>
        Private Shared Function GiaSpedita() As Opportunita

            Dim o As Opportunita = SoloLAnnuncio()
            o.Confronto = System.Text.Json.Nodes.JsonNode.Parse(ConfrontoPieno)
            o.Cv = System.Text.Json.Nodes.JsonNode.Parse("{""intestazione"": {}}")

            o.Avanza(StatoOpportunita.Interessante, o.Creata.AddMinutes(2))
            o.Avanza(StatoOpportunita.Generata, o.Creata.AddMinutes(9))
            o.Avanza(StatoOpportunita.Inviata, o.Creata.AddMinutes(20))

            Return o

        End Function

        <TestMethod>
        Public Async Function ComEAndataSiSegnaSoloDopoAverSpedito() As Task

            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)
                    Assert.IsFalse(Bottone(pannello, "btnEsito").Enabled,
                                   "prima dell'invio non c'è niente che possa essere andato in un modo")

                    Dim dove As String = contesto.Opportunita.Salva(GiaSpedita())
                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    Assert.IsTrue(Bottone(pannello, "btnEsito").Enabled, "dopo sì")

                End Function)

        End Function

        <TestMethod>
        Public Async Function LEsitoSegnatoFinisceSullaSchedaSulDiscoENellIndice() As Task

            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Dim dove As String = contesto.Opportunita.Salva(GiaSpedita())
                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    pannello.SegnaLEsito(EsitoCandidatura.Colloquio)

                    Assert.Contains("Colloquio", Etichetta(pannello, "lblStatoCandidatura").Text,
                                    "la scheda dice com'è andata, non «con esito»")

                    Assert.AreEqual(EsitoCandidatura.Colloquio,
                                    contesto.Opportunita.Carica(dove).Esito,
                                    "e la cartella se lo tiene")

                    Assert.AreEqual(EsitoCandidatura.Colloquio,
                                    contesto.Registro.Carica().Voci.Single().Esito,
                                    "chi cambia uno stato lo annota anche nell'indice (cap. 07.3)")

                    Await Task.CompletedTask

                End Function)

        End Function

        <TestMethod>
        Public Async Function LaVoceSceltaNelMenuArrivaFinoAlDisco() As Task

            ' Il filo fra la voce del menù e l'azione: è l'unico pezzo di questa strada che
            ' nessun altro collaudo tocca, ed è anche l'unico che dal vivo non si può
            ' provare — lo strumento di collaudo risponde «Premuto» e non succede niente
            ' (2026-08-21). Qui la voce si preme davvero.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Dim dove As String = contesto.Opportunita.Salva(GiaSpedita())
                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    Dim menu As ContextMenuStrip = pannello.MenuDegliEsiti()

                    Dim voci As List(Of ToolStripMenuItem) =
                        menu.Items.OfType(Of ToolStripMenuItem)().ToList()

                    Assert.HasCount(4, voci, "l'attesa più i tre esiti")
                    Assert.IsTrue(voci(0).Checked, "adesso è in attesa, e il menù lo dice")

                    voci.Single(Function(v) v.Text = "Assunto 🎉").PerformClick()

                    Assert.AreEqual(EsitoCandidatura.Assunto, contesto.Opportunita.Carica(dove).Esito)

                    ' Riaperto, il menù sposta la spunta su quello che vale adesso.
                    Assert.IsTrue(pannello.MenuDegliEsiti().Items.OfType(Of ToolStripMenuItem)().
                                  Single(Function(v) v.Text = "Assunto 🎉").Checked)

                    Await Task.CompletedTask

                End Function)

        End Function

        <TestMethod>
        Public Async Function LEsitoSiCorreggeESiTogliedallaStessaScheda() As Task

            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto) As Task

                    Dim dove As String = contesto.Opportunita.Salva(GiaSpedita())
                    pannello.RiapriLaCandidatura(contesto.Opportunita.Carica(dove))

                    pannello.SegnaLEsito(EsitoCandidatura.Rifiutata)
                    pannello.SegnaLEsito(EsitoCandidatura.Assunto)
                    Assert.AreEqual(EsitoCandidatura.Assunto, contesto.Opportunita.Carica(dove).Esito,
                                    "una dichiarazione si corregge")

                    pannello.SegnaLEsito(Nothing)
                    Dim riletta As Opportunita = contesto.Opportunita.Carica(dove)
                    Assert.AreEqual(StatoOpportunita.Inviata, riletta.Stato,
                                    "e si toglie del tutto: torna in attesa")
                    Assert.IsFalse(riletta.Esito.HasValue)
                    Assert.Contains("Inviata", Etichetta(pannello, "lblStatoCandidatura").Text)

                    Await Task.CompletedTask

                End Function)

        End Function

        ' ==================================================================
        ' La candidatura eliminata dalla Home (cap. 11.5)
        ' ==================================================================

        <TestMethod>
        Public Async Function LaSchedaLasciaAndareLaCandidaturaEliminata() As Task

            ' Non è pulizia della vista: finché quell'oggetto resta in mano al pannello, il
            ' primo comando che archivia lo riscrive su disco — cioè ricrea la cartella
            ' appena cancellata, e l'eliminazione si disfa da sé.
            Await ConPannelloAsync(
                PipelineFinta(),
                Async Function(pannello, contesto)
                    Await IncollaEAnalizzaAsync(pannello, TestoIncollato)

                    Dim dove As String = pannello.Candidatura.Cartella
                    Assert.IsNotNull(dove, "l'analisi l'ha già scritta nella sua cartella")

                    Assert.IsFalse(pannello.Dimentica(dove & "-di-un-altra"),
                                   "una candidatura che non è la sua non lo riguarda")
                    Assert.IsNotNull(pannello.Candidatura, "e infatti la sua ce l'ha ancora")

                    contesto.Opportunita.Elimina(dove)

                    Assert.IsTrue(pannello.Dimentica(dove), "questa invece era proprio la sua")
                    Assert.IsNull(pannello.Candidatura, "e non la tiene più in mano")
                    Assert.AreEqual(String.Empty, Casella(pannello, "txtAnnuncioLetto").Text,
                                    "la scheda torna com'era prima di aprirne una")
                End Function)

        End Function

        ''' <summary>
        ''' Un pannello collegato a un motore vero — cartella dati temporanea, nessuna
        ''' chiave — con la pipeline finta che gli si vuol dare.
        ''' </summary>
        ''' <param name="pipeline">Chi conduce i passi; <c>Nothing</c> per provare l'app senza AI.</param>
        ''' <param name="conProfilo">Se su disco c'è già un profilo da confrontare.</param>
        Private Shared Async Function ConPannelloAsync(pipeline As PipelineCandidatura,
                                                       prova As Func(Of PannelloOpportunita, ContestoApp, Task),
                                                       Optional conProfilo As Boolean = True) As Task

            Dim radice As String = Path.Combine(
                Path.GetTempPath(), "pannello-opportunita-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      pannello As New PannelloOpportunita()

                    ' Il nome va qualificato: in questo banco «Profilo» è anche la
                    ' funzione del modulo dei casi, che coprirebbe il tipo.
                    If conProfilo Then
                        contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))
                    End If

                    ' Senza handle i controlli non sono «realizzati»: qui il pannello non
                    ' è appeso a nessuna finestra, e va creato a mano.
                    pannello.CreateControl()
                    pannello.Collega(contesto, pipeline)

                    Await prova(pannello, contesto)
                End Using

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Function

        ''' <summary>La pipeline coi mestieri finti, già pronti a rispondere.</summary>
        Private Shared Function PipelineFinta(Optional confronto As String = ConfrontoConPaletto,
                                              Optional confrontatore As ConfrontatoreFinto = Nothing,
                                              Optional annuncio As String = AnnuncioLetto) As PipelineCandidatura

            Dim analizzatore As New AnalizzatoreFinto
            analizzatore.Dara(annuncio)

            Dim chiConfronta As ConfrontatoreFinto = If(confrontatore, New ConfrontatoreFinto)
            chiConfronta.Dara(confronto).Dara("{""mitigazioni"": [{""requisito_gap"": ""Patente C""}]}")

            Return New PipelineCandidatura(analizzatore, chiConfronta, New GeneratoreFinto)

        End Function

        ''' <summary>Incolla il testo e preme «Analizza», come farebbe l'utente.</summary>
        Private Shared Function IncollaEAnalizzaAsync(pannello As PannelloOpportunita, testo As String) As Task

            Casella(pannello, "txtAnnuncio").Text = testo
            Return pannello.AnalizzaLAnnuncioAsync()

        End Function

        Private Shared Function Giudizi(pannello As Control) As List(Of ListViewItem)

            Dim elenco As ListView = DirectCast(
                pannello.Controls.Find("lvwGiudizi", searchAllChildren:=True).Single(), ListView)

            Return elenco.Items.Cast(Of ListViewItem)().ToList()

        End Function

        Private Shared Function Casella(pannello As Control, nome As String) As TextBox
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), TextBox)
        End Function

        Private Shared Function Bottone(pannello As Control, nome As String) As Button
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Etichetta(pannello As Control, nome As String) As Label
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

        Private Shared Function Riquadro(pannello As Control, nome As String) As Panel
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Panel)
        End Function

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

    End Class

End Namespace
