Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi della pipeline di candidatura (cap. 02, cap. 12). Girano <b>senza rete</b>
    ''' coi tre mestieri finti: quello che si verifica non è cosa risponde l'AI, ma
    ''' l'<b>ordine dei passi</b>, cosa riceve ciascuno, e le due cose che la pipeline
    ''' decide da sé — quando saltare la mitigazione e quanto vale il match.
    ''' </summary>
    <TestClass>
    Public Class CollaudiPipelineCandidatura

        Private Const TestoAnnuncio As String = "Cercasi tecnico manutenzione a Forlì."

        Private Const AnnuncioJson As String =
            "{""titolo"": ""Tecnico manutenzione"", ""azienda"": ""Rossi S.p.A.""}"

        ''' <summary>Un confronto senza gap: tutto soddisfatto.</summary>
        Private Const ConfrontoPieno As String =
            "{""giudizi"": [" &
            "{""requisito"": ""Patente B"", ""categoria"": ""altri_requisiti""," &
            """priorita"": ""richiesto"", ""esito"": ""soddisfatto"", ""eliminatorio"": false}]," &
            """lettura_insieme"": ""In linea"", ""numero_complessivo"": 90}"

        ''' <summary>Un confronto con un gap: c'è qualcosa da mitigare.</summary>
        Private Const ConfrontoConGap As String =
            "{""giudizi"": [" &
            "{""requisito"": ""Patente B"", ""categoria"": ""altri_requisiti""," &
            """priorita"": ""richiesto"", ""esito"": ""soddisfatto"", ""eliminatorio"": false}," &
            "{""requisito"": ""Tre anni di esperienza"", ""categoria"": ""esperienza""," &
            """priorita"": ""richiesto"", ""esito"": ""in parte"", ""eliminatorio"": false}]," &
            """lettura_insieme"": ""Quasi"", ""numero_complessivo"": 70}"

        Private Shared Function ProfiloDiProva() As TrovaLavoro.Dati.Profilo
            Return TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo())
        End Function

        ''' <summary>La pipeline coi tre finti, già pronti a rispondere.</summary>
        Private Shared Function PipelineDiProva(analizzatore As AnalizzatoreFinto,
                                                confrontatore As ConfrontatoreFinto,
                                                generatore As GeneratoreFinto) As PipelineCandidatura
            Return New PipelineCandidatura(analizzatore, confrontatore, generatore)
        End Function

        <TestMethod>
        Public Async Function LaPipelineMetteIPassiInFila() As Task

            Dim analizzatore As New AnalizzatoreFinto
            Dim confrontatore As New ConfrontatoreFinto
            Dim generatore As New GeneratoreFinto

            analizzatore.Dara(AnnuncioJson)
            confrontatore.Dara(ConfrontoConGap).Dara("{""mitigazioni"": [{""requisito_gap"": ""Tre anni""}]}")
            generatore.Dara("{""intestazione"": {}}").Dara("{""corpo"": ""Gentile azienda…""}")

            Dim opportunita As Opportunita = Await PipelineDiProva(analizzatore, confrontatore, generatore).
                EseguiTuttoAsync(TestoAnnuncio, ProfiloDiProva())

            Assert.AreEqual("analisi", analizzatore.LavoriChiesti(), "prima l'annuncio")
            Assert.AreEqual("confronto → mitigazione", confrontatore.LavoriChiesti(),
                            "poi il giudizio e i ponti sui gap")
            Assert.AreEqual("cv_mirato → lettera", generatore.LavoriChiesti(),
                            "e infine i documenti, il CV prima della lettera")

            Assert.AreEqual(TestoAnnuncio, analizzatore.Testi.Single(), "il testo arriva com'è")
            Assert.AreEqual("Rossi S.p.A.", opportunita.Azienda, "l'opportunità porta l'annuncio")
            Assert.IsNotNull(opportunita.Cv, "il CV mirato")
            Assert.IsNotNull(opportunita.Lettera, "la lettera")
            Assert.IsNotNull(opportunita.Match, "e il punteggio")

        End Function

        ''' <summary>La stessa pipeline, con la passata anti-slop montata (T7b).</summary>
        Private Shared Function PipelineConRifinitura(generatore As GeneratoreFinto,
                                                      rifinitore As RifinitoreFinto) As PipelineCandidatura

            Dim confrontatore As New ConfrontatoreFinto
            confrontatore.Dara(ConfrontoPieno)

            Return New PipelineCandidatura(New AnalizzatoreFinto, confrontatore, generatore,
                                           Nothing, New Rifinitura(rifinitore))

        End Function

        ''' <summary>Un'opportunità già confrontata, pronta per la generazione.</summary>
        Private Shared Async Function GiaConfrontataAsync(pipeline As PipelineCandidatura) As Task(Of Opportunita)

            Dim opportunita As New Opportunita With {.Annuncio = JsonNode.Parse(AnnuncioJson)}
            Await pipeline.ConfrontaAsync(opportunita, ProfiloDiProva())
            Return opportunita

        End Function

        <TestMethod>
        Public Async Function OgniDocumentoPassaDallaRifinituraAppenaScritto() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara("{""sommario"": ""Ho un — sommario""}").
                       Dara("{""corpo"": ""Un corpo — con lineetta""}")

            Dim rifinitore As New RifinitoreFinto()
            Dim pipeline As PipelineCandidatura = PipelineConRifinitura(generatore, rifinitore)

            Await pipeline.GeneraAsync(Await GiaConfrontataAsync(pipeline), ProfiloDiProva())

            Assert.AreEqual("Sintesi → Prosa", rifinitore.GeneriChiesti(),
                            "il sommario del CV e poi il corpo della lettera")

            ' Le descrizioni non compaiono perché questo CV non ne ha: niente da rifinire,
            ' nessuna chiamata (v. CollaudiRifinitore).
            Assert.AreEqual("sommario", rifinitore.Passate(0).Id(), "prima il CV")
            Assert.AreEqual("corpo", rifinitore.Passate(1).Id(), "poi la lettera")

        End Function

        <TestMethod>
        Public Async Function LaLetteraRiceveIlCvGiaRifinito() As Task

            ' La lettera usa il CV come riferimento di coerenza: dargli quello grezzo
            ' vorrebbe dire farle raccontare la stessa storia con parole che nel CV non ci
            ' sono più.
            Dim generatore As New GeneratoreFinto
            generatore.Dara("{""sommario"": ""Ho un — sommario""}").Dara("{""corpo"": ""…""}")

            Dim rifinitore As RifinitoreFinto = New RifinitoreFinto().
                Dara("sommario", "Ho un sommario, senza lineetta")

            Dim pipeline As PipelineCandidatura = PipelineConRifinitura(generatore, rifinitore)

            Await pipeline.GeneraAsync(Await GiaConfrontataAsync(pipeline), ProfiloDiProva())

            Dim cvVistoDallaLettera As JsonNode = generatore.Chiamate.Last().Ingressi(3)

            Assert.Contains("senza lineetta", cvVistoDallaLettera.ToJsonString(),
                            "alla lettera arriva il CV rifinito")

        End Function

        <TestMethod>
        Public Async Function IlPrimaSiAnnotaDocumentoPerDocumento() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara("{""sommario"": ""com'era il sommario""}").
                       Dara("{""corpo"": ""com'era il corpo""}")

            Dim rifinitore As RifinitoreFinto = New RifinitoreFinto().
                Dara("sommario", "adesso il sommario").
                Dara("corpo", "adesso il corpo")

            Dim pipeline As PipelineCandidatura = PipelineConRifinitura(generatore, rifinitore)
            Dim opportunita As Opportunita = Await GiaConfrontataAsync(pipeline)

            Await pipeline.GeneraAsync(opportunita, ProfiloDiProva())

            Dim prima As JsonNode = opportunita.PrimaDellaRifinitura

            Assert.AreEqual("com'era il sommario", prima("cv")("sommario").GetValue(Of String)(),
                            "il prima del CV")
            Assert.AreEqual("com'era il corpo", prima("lettera")("corpo").GetValue(Of String)(),
                            "e quello della lettera, ciascuno sotto il suo nome")

        End Function

        <TestMethod>
        Public Async Function UnaRifinituraCheInciampaNonButtaViaIDocumenti() As Task

            ' La rifinitura è un miglioramento facoltativo (cap. 08): far cadere l'intera
            ' candidatura per un suo inciampo vorrebbe dire buttare via un CV già pronto e
            ' chiedere all'utente di rifare tutto, altre attese comprese.
            Dim generatore As New GeneratoreFinto
            generatore.Dara("{""sommario"": ""un sommario""}").Dara("{""corpo"": ""un corpo""}")

            Dim rifinitore As New RifinitoreFinto With {
                .Fallira = New TrovaLavoro.Ai.ErroreAi(
                    TrovaLavoro.Ai.CausaErroreAi.Rete, "la rete è caduta")}

            Dim pipeline As PipelineCandidatura = PipelineConRifinitura(generatore, rifinitore)
            Dim opportunita As Opportunita = Await GiaConfrontataAsync(pipeline)

            Await pipeline.GeneraAsync(opportunita, ProfiloDiProva())

            Assert.IsNotNull(opportunita.Cv, "il CV c'è comunque")
            Assert.IsNotNull(opportunita.Lettera, "e la lettera pure: la fila non si è fermata")
            Assert.IsNull(opportunita.PrimaDellaRifinitura, "nessun prima, perché non è cambiato niente")

        End Function

        <TestMethod>
        Public Async Function IlContoDeiPassiDipendeDaComEMontataLaFila() As Task

            ' Con la rifinitura i passi sono sei — scrivere e rifinire, per due documenti —
            ' e senza sono quattro. Promettere sei passi a chi ne farà quattro lascerebbe
            ' l'attesa a metà per sempre.
            Dim generatore As New GeneratoreFinto
            generatore.Dara("{""sommario"": ""un sommario""}").Dara("{""corpo"": ""un corpo""}")

            Dim conRifinitura As New SpiaAvanzamento
            Dim pipeline As PipelineCandidatura = PipelineConRifinitura(generatore, New RifinitoreFinto())
            Await pipeline.GeneraAsync(Await GiaConfrontataAsync(pipeline), ProfiloDiProva(), conRifinitura)

            Assert.IsTrue(conRifinitura.Visti.All(Function(v) v.EndsWith(" di 6)")),
                          $"sei passi in tutto: {String.Join(" · ", conRifinitura.Visti)}")
            Assert.Contains("Rifinisco il CV (4 di 6)", conRifinitura.Visti,
                            "e la rifinitura del CV è il quarto")

            Dim altroGeneratore As New GeneratoreFinto
            altroGeneratore.Dara("{""sommario"": ""un sommario""}").Dara("{""corpo"": ""un corpo""}")

            Dim confrontatore As New ConfrontatoreFinto
            confrontatore.Dara(ConfrontoPieno)

            Dim senza As New SpiaAvanzamento
            Dim spoglia As PipelineCandidatura = PipelineDiProva(New AnalizzatoreFinto, confrontatore, altroGeneratore)
            Await spoglia.GeneraAsync(Await GiaConfrontataAsync(spoglia), ProfiloDiProva(), senza)

            Assert.IsTrue(senza.Visti.All(Function(v) v.EndsWith(" di 4)")),
                          $"e quattro senza: {String.Join(" · ", senza.Visti)}")

        End Function

        <TestMethod>
        Public Async Function SenzaGapLaMitigazioneNonSiChiede() As Task

            ' Senza requisiti scoperti la lista uscirebbe vuota per forza: chiederlo
            ' all'AI costerebbe un'attesa e dei token per sapere una cosa che si sa già.
            ' È una differenza voluta dal prototipo, che la chiamava sempre.
            Dim confrontatore As New ConfrontatoreFinto
            Dim generatore As New GeneratoreFinto

            confrontatore.Dara(ConfrontoPieno)
            generatore.Dara("{""intestazione"": {}}").Dara("{""corpo"": ""…""}")

            Dim opportunita As New Opportunita With {.Annuncio = JsonNode.Parse(AnnuncioJson)}
            Dim pipeline As PipelineCandidatura = PipelineDiProva(New AnalizzatoreFinto, confrontatore, generatore)

            Await pipeline.ConfrontaAsync(opportunita, ProfiloDiProva())

            Assert.AreEqual("confronto", confrontatore.LavoriChiesti(), "un lavoro solo")
            Assert.IsEmpty(CType(opportunita.Mitigazioni("mitigazioni"), JsonArray),
                           "e la lista vuota c'è lo stesso, senza averla chiesta")

            ' E la lettera riceve comunque il suo blocco: il generatore non deve
            ' accorgersi che la mitigazione è stata saltata.
            Await pipeline.GeneraAsync(opportunita, ProfiloDiProva())
            Dim allaLettera As JsonNode = generatore.Chiamate.Last().Ingressi(4)
            Assert.IsEmpty(CType(allaLettera, JsonArray), "una lista vuota, non un campo assente")

        End Function

        <TestMethod>
        Public Async Function ConUnGapLaMitigazioneSiChiede() As Task

            Dim confrontatore As New ConfrontatoreFinto
            confrontatore.Dara(ConfrontoConGap).Dara("{""mitigazioni"": []}")

            Dim opportunita As New Opportunita With {.Annuncio = JsonNode.Parse(AnnuncioJson)}

            Await PipelineDiProva(New AnalizzatoreFinto, confrontatore, New GeneratoreFinto).
                ConfrontaAsync(opportunita, ProfiloDiProva())

            Assert.AreEqual("confronto → mitigazione", confrontatore.LavoriChiesti(),
                            "un «in parte» basta a farla chiedere")

            ' E alla mitigazione arriva la LISTA dei giudizi, non l'oggetto intero del
            ' confronto: è ciò che il prompt dichiara di ricevere.
            Dim aiGiudizi As JsonNode = confrontatore.Chiamate.Last().Ingressi(1)
            Assert.IsInstanceOfType(Of JsonArray)(aiGiudizi, "i giudizi come lista")

        End Function

        <TestMethod>
        Public Async Function OgniPassoPortaAvantiLoStato() As Task

            ' La macchina degli stati (cap. 07.3) la muove chi fa il lavoro, non chi
            ' guarda: l'annuncio letto fa una candidatura «nuova», il confronto la fa
            ' «interessante», i documenti la fanno «generata».
            Dim analizzatore As New AnalizzatoreFinto
            Dim confrontatore As New ConfrontatoreFinto
            Dim generatore As New GeneratoreFinto

            analizzatore.Dara(AnnuncioJson)
            confrontatore.Dara(ConfrontoPieno)
            generatore.Dara("{""intestazione"": {}}").Dara("{""corpo"": ""Gentile azienda…""}")

            Dim pipeline As PipelineCandidatura = PipelineDiProva(analizzatore, confrontatore, generatore)
            Dim profilo As TrovaLavoro.Dati.Profilo = ProfiloDiProva()

            Dim opportunita As Opportunita = Await pipeline.AnalizzaAsync(TestoAnnuncio)
            Assert.AreEqual(StatoOpportunita.Nuova, opportunita.Stato, "letto l'annuncio, è nuova")
            Assert.AreEqual(opportunita.Creata, opportunita.DateStati(StatoOpportunita.Nuova),
                            "la data di nascita e quella del primo stato sono la stessa cosa")

            Await pipeline.ConfrontaAsync(opportunita, profilo)
            Assert.AreEqual(StatoOpportunita.Interessante, opportunita.Stato,
                            "ci sono le stelle: adesso si può decidere")

            Await pipeline.GeneraAsync(opportunita, profilo)
            Assert.AreEqual(StatoOpportunita.Generata, opportunita.Stato, "e i documenti ci sono")

        End Function

        <TestMethod>
        Public Async Function RiconfrontareUnaCandidaturaNonLaFaTornareIndietro() As Task

            ' Succede riaprendo una candidatura già completa: i giudizi si rifanno, ma i
            ' documenti restano scritti e lo stato non retrocede.
            Dim confrontatore As New ConfrontatoreFinto
            confrontatore.Dara(ConfrontoPieno)

            Dim opportunita As New Opportunita With {.Annuncio = JsonNode.Parse(AnnuncioJson)}
            opportunita.Avanza(StatoOpportunita.Interessante)
            opportunita.Avanza(StatoOpportunita.Generata)

            Await PipelineDiProva(New AnalizzatoreFinto, confrontatore, New GeneratoreFinto).
                ConfrontaAsync(opportunita, ProfiloDiProva())

            Assert.AreEqual(StatoOpportunita.Generata, opportunita.Stato)

        End Function

        <TestMethod>
        Public Async Function IlPunteggioLoCalcolaIlProgrammaNonLAi() As Task

            ' L'AI dice 90, ma un requisito eliminatorio non soddisfatto impone il tetto:
            ' il numero finale è del programma, ed è deterministico (cap. 02).
            Dim confrontatore As New ConfrontatoreFinto
            confrontatore.Dara(
                "{""giudizi"": [{""requisito"": ""Patente C"", ""categoria"": ""altri_requisiti""," &
                """priorita"": ""richiesto"", ""esito"": ""non soddisfatto"", ""eliminatorio"": true}]," &
                """numero_complessivo"": 90}").Dara("{""mitigazioni"": []}")

            Dim opportunita As New Opportunita With {.Annuncio = JsonNode.Parse(AnnuncioJson)}

            Await PipelineDiProva(New AnalizzatoreFinto, confrontatore, New GeneratoreFinto).
                ConfrontaAsync(opportunita, ProfiloDiProva())

            Assert.IsTrue(opportunita.Match.GateEliminatorio, "il requisito tassativo è scoperto")
            Assert.IsLessThanOrEqualTo(20, opportunita.Match.MatchFinale.Value,
                                       "e il match cratera, checché ne dica l'AI")
            Assert.IsNotNull(opportunita.Match.Nota, "con la nota che lo spiega")

        End Function

        <TestMethod>
        Public Async Function LaLetteraRiceveIlCvAppenaGenerato() As Task

            ' Lettera e CV devono raccontare la stessa storia: il CV che arriva alla
            ' lettera è quello appena scritto, non uno di prima.
            Dim confrontatore As New ConfrontatoreFinto
            Dim generatore As New GeneratoreFinto

            confrontatore.Dara(ConfrontoPieno)
            generatore.Dara("{""intestazione"": {""nome"": ""Luca Ferrari""}, ""marchio"": ""questo-cv""}").
                       Dara("{""corpo"": ""…""}")

            Dim opportunita As New Opportunita With {.Annuncio = JsonNode.Parse(AnnuncioJson)}
            Dim pipeline As PipelineCandidatura = PipelineDiProva(New AnalizzatoreFinto, confrontatore, generatore)

            Await pipeline.ConfrontaAsync(opportunita, ProfiloDiProva())
            Await pipeline.GeneraAsync(opportunita, ProfiloDiProva())

            Dim cvArrivato As JsonNode = generatore.Chiamate.Last().Ingressi(3)
            Assert.AreEqual("questo-cv", cvArrivato("marchio").ToString(), "proprio quel CV")

        End Function

        <TestMethod>
        Public Async Function IDocumentiNonSiGeneranoPrimaDelConfronto() As Task

            ' Un errore di programma, non dell'utente: va detto subito e chiaramente,
            ' invece di mandare all'AI una lettera senza giudizi.
            Dim opportunita As New Opportunita With {.Annuncio = JsonNode.Parse(AnnuncioJson)}

            Dim errore As InvalidOperationException = Await Assert.ThrowsAsync(Of InvalidOperationException)(
                Function() PipelineDiProva(New AnalizzatoreFinto, New ConfrontatoreFinto, New GeneratoreFinto).
                    GeneraAsync(opportunita, ProfiloDiProva()))

            Assert.Contains("ConfrontaAsync", errore.Message, "e dice cosa fare prima")

        End Function

        <TestMethod>
        Public Async Function LAvanzamentoDiceAChePuntoSiamo() As Task

            Dim analizzatore As New AnalizzatoreFinto
            Dim confrontatore As New ConfrontatoreFinto
            Dim generatore As New GeneratoreFinto

            analizzatore.Dara(AnnuncioJson)
            confrontatore.Dara(ConfrontoPieno)
            generatore.Dara("{""intestazione"": {}}").Dara("{""corpo"": ""…""}")

            Dim spia As New SpiaAvanzamento

            Await PipelineDiProva(analizzatore, confrontatore, generatore).
                EseguiTuttoAsync(TestoAnnuncio, ProfiloDiProva(), spia)

            Assert.HasCount(4, spia.Visti, "quattro passi annunciati")
            Assert.AreEqual("Leggo l'annuncio (1 di 4)", spia.Visti(0), "il primo")
            Assert.AreEqual("Scrivo la lettera (4 di 4)", spia.Visti(3), "e l'ultimo")

        End Function

        <TestMethod>
        Public Async Function LaLinguaDellAnnuncioDiventaQuellaDellaCandidatura() As Task

            ' Cap. 10.1: la lingua è una proprietà della candidatura, e la propone
            ' l'analisi. Qui si verifica il primo anello del filo — dal campo del JSON al
            ' campo dell'opportunità, che è quello che finisce su disco e nel registro.
            Dim analizzatore As New AnalizzatoreFinto
            analizzatore.Dara("{""titolo"": ""Maintenance technician"", ""lingua"": ""en""}")

            Dim opportunita As Opportunita = Await PipelineDiProva(
                analizzatore, New ConfrontatoreFinto, New GeneratoreFinto).AnalizzaAsync(TestoAnnuncio)

            Assert.AreEqual("en", opportunita.Lingua, "l'annuncio è in inglese, la candidatura pure")

        End Function

        <TestMethod>
        Public Async Function UnAnnuncioSenzaLinguaRestaInItaliano() As Task

            ' Il caso di tutti gli annunci analizzati prima del Pool 1.06 — e di quelli
            ' che il modello dovesse restituire senza quel campo. La lingua di casa è
            ' l'unica risposta che non riscrive niente all'indietro.
            Dim analizzatore As New AnalizzatoreFinto
            analizzatore.Dara(AnnuncioJson)

            Dim opportunita As Opportunita = Await PipelineDiProva(
                analizzatore, New ConfrontatoreFinto, New GeneratoreFinto).AnalizzaAsync(TestoAnnuncio)

            Assert.AreEqual("it", opportunita.Lingua, "senza il campo, italiano")

        End Function

        <TestMethod>
        Public Async Function UnAnnuncioInUnaTerzaLinguaSiScriveInInglese() As Task

            ' Cap. 10.2: il pool ha due varianti, non tutte le lingue. Un annuncio in
            ' tedesco non deve fermare la candidatura né chiedere un prompt che non esiste
            ' — «de» arriverebbe al caricatore e non troverebbe nessun «cv_mirato.de.md».
            Dim analizzatore As New AnalizzatoreFinto
            analizzatore.Dara("{""titolo"": ""Instandhaltungstechniker"", ""lingua"": ""de""}")

            Dim opportunita As Opportunita = Await PipelineDiProva(
                analizzatore, New ConfrontatoreFinto, New GeneratoreFinto).AnalizzaAsync(TestoAnnuncio)

            Assert.AreEqual("en", opportunita.Lingua, "una terza lingua si scrive in inglese")

        End Function

        <TestMethod>
        Public Async Function LaLinguaDellaCandidaturaArrivaAlGeneratore() As Task

            ' Il secondo anello del filo, e quello che senza un finto che se ne accorga
            ' nessuno vedrebbe: la lingua deve arrivare fino alla scelta della variante di
            ' prompt (cap. 04.6). Se si fermasse per strada, i documenti uscirebbero in
            ' italiano con un nome di file che dice EN.
            Dim analizzatore As New AnalizzatoreFinto
            Dim confrontatore As New ConfrontatoreFinto
            Dim generatore As New GeneratoreFinto

            analizzatore.Dara("{""titolo"": ""Maintenance technician"", ""lingua"": ""en""}")
            confrontatore.Dara(ConfrontoPieno)
            generatore.Dara("{""intestazione"": {}}").Dara("{""corpo"": ""Dear Sir or Madam…""}")

            Await PipelineDiProva(analizzatore, confrontatore, generatore).
                EseguiTuttoAsync(TestoAnnuncio, ProfiloDiProva())

            CollectionAssert.AreEqual({"en", "en"}, generatore.LingueChieste.ToArray(),
                                      "il CV e la lettera, tutti e due in inglese")

        End Function

        <TestMethod>
        Public Async Function GliAppuntiDiMiraArrivanoAiDueDocumenti() As Task

            ' Il filo di T7c, gemello di quello della lingua: gli appunti confermati nel
            ' brainstorming vivono nella cartella della candidatura, e senza un finto che
            ' se ne accorga nessuno vedrebbe se si fermassero prima della richiesta.
            Dim generatore As New GeneratoreFinto
            generatore.Dara("{""intestazione"": {}}").Dara("{""corpo"": ""…""}")

            ' La forma fluente di «Dara» restituisce il tipo base: qui il confrontatore
            ' serve col suo tipo, e le due cose si scrivono su righe separate.
            Dim confrontatore As New ConfrontatoreFinto
            confrontatore.Dara(ConfrontoPieno)

            Dim pipeline As PipelineCandidatura = PipelineDiProva(
                New AnalizzatoreFinto, confrontatore, generatore)

            Dim opportunita As Opportunita = Await GiaConfrontataAsync(pipeline)
            opportunita.Appunti = JsonNode.Parse(
                "{""appunti"":[{""tipo"":""enfasi"",""testo"":""Metti davanti il magazzino"",""da"":""la chat""}]," &
                """fatti_nuovi"":[""ho il patentino del muletto""]}")

            Await pipeline.GeneraAsync(opportunita, ProfiloDiProva())

            Assert.HasCount(2, generatore.AppuntiVisti, "il CV mirato e la lettera")

            For Each visti As JsonNode In generatore.AppuntiVisti

                Dim comeArrivano As String = visti.ToJsonString()

                Assert.Contains("magazzino", comeArrivano, "l'appunto arriva")

                ' La regola che tiene in piedi tutto: quello che l'utente ha detto in chat
                ' e che nel profilo non c'è NON entra nei documenti da questa porta.
                Assert.DoesNotContain("patentino", comeArrivano, "il fatto nuovo no")

                ' E nemmeno la frase da cui l'appunto nasce: serve all'utente per
                ' riconoscerlo nella scheda, a chi scrive il CV metterebbe solo davanti
                ' pezzi di conversazione da cui pescare.
                Assert.DoesNotContain("la chat", comeArrivano, "né la sua provenienza")

            Next

        End Function

        <TestMethod>
        Public Async Function SenzaBrainstormingSiGeneraComeSempre() As Task

            ' Il caso normale: chi non ha ragionato non ha appunti, e non è un errore —
            ' al prompt arriva una lista vuota, non un buco.
            Dim generatore As New GeneratoreFinto
            generatore.Dara("{""intestazione"": {}}").Dara("{""corpo"": ""…""}")

            ' La forma fluente di «Dara» restituisce il tipo base: qui il confrontatore
            ' serve col suo tipo, e le due cose si scrivono su righe separate.
            Dim confrontatore As New ConfrontatoreFinto
            confrontatore.Dara(ConfrontoPieno)

            Dim pipeline As PipelineCandidatura = PipelineDiProva(
                New AnalizzatoreFinto, confrontatore, generatore)

            Await pipeline.GeneraAsync(Await GiaConfrontataAsync(pipeline), ProfiloDiProva())

            For Each visti As JsonNode In generatore.AppuntiVisti
                Assert.IsNotNull(visti, "gli appunti non arrivano mai come niente")
                Assert.AreEqual("[]", visti.ToJsonString(), "ma come lista vuota")
            Next

        End Function

        <TestMethod>
        Public Async Function CambiareLinguaERigenerareScriveNellaLinguaNuova() As Task

            ' P6 lascia cambiare la lingua proposta (cap. 10.1), e da lì si rigenera. La
            ' pipeline legge la lingua dall'opportunità a ogni generazione, non se la
            ' ricorda dall'analisi: è ciò che rende il ripensamento un'operazione sola.
            Dim analizzatore As New AnalizzatoreFinto
            Dim confrontatore As New ConfrontatoreFinto
            Dim generatore As New GeneratoreFinto

            analizzatore.Dara(AnnuncioJson)
            confrontatore.Dara(ConfrontoPieno)
            generatore.Dara("{""intestazione"": {}}").Dara("{""corpo"": ""Dear Sir or Madam…""}")

            Dim pipeline As PipelineCandidatura = PipelineDiProva(analizzatore, confrontatore, generatore)
            Dim opportunita As Opportunita = Await pipeline.AnalizzaAsync(TestoAnnuncio)
            Assert.AreEqual("it", opportunita.Lingua, "l'annuncio era italiano")

            Await pipeline.ConfrontaAsync(opportunita, ProfiloDiProva())

            ' L'utente ci ripensa: l'azienda è italiana ma vuole candidarsi in inglese.
            opportunita.Lingua = "en"
            Await pipeline.GeneraAsync(opportunita, ProfiloDiProva())

            CollectionAssert.AreEqual({"en", "en"}, generatore.LingueChieste.ToArray(),
                                      "vale la lingua di adesso, non quella rilevata")

        End Function

        ''' <summary>
        ''' Chi guarda l'avanzamento, e lo guarda <b>subito</b>. Non si usa
        ''' <see cref="Progress(Of T)"/> di proposito: quello consegna sul contesto di
        ''' sincronizzazione catturato alla nascita, e nel banco quel contesto non pompa
        ''' finché il collaudo non è finito — il collaudo passerebbe da solo e fallirebbe
        ''' in batteria, che è il modo peggiore di fallire.
        ''' </summary>
        Private Class SpiaAvanzamento
            Implements IProgress(Of AvanzamentoPipeline)

            Public ReadOnly Property Visti As New List(Of String)

            Public Sub Report(valore As AvanzamentoPipeline) Implements IProgress(Of AvanzamentoPipeline).Report
                Visti.Add(valore.ToString())
            End Sub

        End Class

    End Class

End Namespace
