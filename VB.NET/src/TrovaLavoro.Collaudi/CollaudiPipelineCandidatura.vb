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
