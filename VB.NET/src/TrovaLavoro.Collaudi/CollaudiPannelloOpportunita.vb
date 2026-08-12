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
