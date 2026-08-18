Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi della conduzione del ragionamento (T7c, cap. 12 A6). Girano senza rete:
    ''' al posto dell'AI c'è <see cref="BrainstormatoreFinto"/>, che risponde quel che il
    ''' collaudo gli mette in bocca e consegna a pezzi come farebbe il flusso vero.
    ''' </summary>
    <TestClass>
    Public Class CollaudiBrainstorming

        ''' <summary>Una candidatura già confrontata, come quella su cui si ragiona.</summary>
        Private Shared Function Candidatura() As Opportunita

            Return New Opportunita With {
                .Annuncio = JsonNode.Parse("{""titolo"":""Magazziniere"",""azienda"":""Acme""}"),
                .Confronto = JsonNode.Parse("{""giudizi"":[{""voce"":""muletto"",""esito"":""non soddisfatto""}]}"),
                .Mitigazioni = JsonNode.Parse("{""mitigazioni"":[{""requisito_gap"":""muletto""}]}")}

        End Function

        Private Shared Function Profilo() As JsonNode
            Return JsonNode.Parse("{""nome"":""Anna""}")
        End Function

        <TestMethod>
        Public Async Function ApreLAiConQuelloCheVede() As Task
            ' Una chat che si apre vuota non dice da dove cominciare: il primo turno è
            ' dell'assistente, e all'AI non è ancora arrivata nessuna battuta.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("Reggi bene sui tre anni di magazzino.")

            Dim ragionamento As New Brainstorming(finto, Candidatura(), Profilo())
            Await ragionamento.ApriAsync(Nothing)

            Assert.AreEqual(0, finto.BattuteViste(0), "all'apertura la conversazione è vuota")
            Assert.HasCount(1, ragionamento.Battute, "e dopo c'è la battuta dell'AI")
            Assert.AreEqual(TurnoChat.Assistente, ragionamento.Battute(0).Ruolo, "ha aperto lei")
            Assert.IsTrue(ragionamento.Cominciato, "il ragionamento è cominciato")

        End Function

        <TestMethod>
        Public Async Function LaRispostaArrivaAPezziEPoiIntera() As Task
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("Il nodo è il muletto.")

            Dim visti As New List(Of String)
            Dim ragionamento As New Brainstorming(finto, Candidatura(), Profilo())

            Await ragionamento.ApriAsync(Sub(p) visti.Add(p))

            Assert.IsGreaterThan(1, visti.Count, "i pezzi devono essere più d'uno")
            Assert.AreEqual("Il nodo è il muletto.", String.Concat(visti), "rimessi in fila fanno il testo")
            Assert.AreEqual("Il nodo è il muletto.", ragionamento.Battute(0).Testo, "e la battuta è quella")

        End Function

        <TestMethod>
        Public Async Function IlContestoViaggiaAOgniTurno() As Task
            ' Profilo, annuncio, giudizi e mitigazioni sono le quattro fonti del prompt
            ' (cap. 04.3): se una si perdesse per strada, l'AI ragionerebbe al buio su
            ' quella parte — e nessuno se ne accorgerebbe leggendo la risposta.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro").Dira("rispondo")

            Dim ragionamento As New Brainstorming(finto, Candidatura(), Profilo())
            Await ragionamento.ApriAsync(Nothing)
            Await ragionamento.RispondiAsync("e io dico la mia", Nothing)

            Assert.HasCount(2, finto.Chiamate, "due turni")
            For Each chiamata As MestiereFinto.Chiamata In finto.Chiamate
                Assert.HasCount(4, chiamata.Ingressi, "quattro fonti")
                For Each fonte As JsonNode In chiamata.Ingressi
                    Assert.IsNotNull(fonte, "nessuna delle quattro deve arrivare vuota")
                Next
            Next

            Assert.AreEqual(2, finto.BattuteViste(1), "al secondo turno l'AI vede quel che si è detto")

        End Function

        <TestMethod>
        Public Async Function DueBattuteDellUtenteDiFilaSiUniscono() As Task
            ' Capita davvero: una risposta fallisce e l'utente riscrive. L'API vuole che i
            ' ruoli si alternino, e l'unica via che non perde niente è unire — la prima
            ' frase l'utente l'ha detta e continua a vederla sullo schermo.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro")
            finto.FalliraParlando(New ErroreAi(CausaErroreAi.Rete, "niente rete"))
            finto.Dira("eccomi")

            Dim ragionamento As New Brainstorming(finto, Candidatura(), Profilo())
            Await ragionamento.ApriAsync(Nothing)

            Try
                Await ragionamento.RispondiAsync("prima frase", Nothing)
                Assert.Fail("la rete caduta doveva sollevare")
            Catch ex As ErroreAi
                ' È quel che deve succedere: la battuta però resta.
            End Try

            Await ragionamento.RispondiAsync("seconda frase", Nothing)

            Dim dellUtente As List(Of TurnoChat) =
                ragionamento.Battute.Where(Function(b) b.Ruolo = TurnoChat.Utente).ToList()

            Assert.HasCount(1, dellUtente, "una sola battuta dell'utente, non due di fila")
            Assert.Contains("prima frase", dellUtente(0).Testo, "la prima frase non si perde")
            Assert.Contains("seconda frase", dellUtente(0).Testo, "e nemmeno la seconda")

        End Function

        <TestMethod>
        Public Async Function LaTrascrizioneDiceChiHaParlato() As Task
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io").Dira("ti rispondo")
            finto.Dara("{""appunti"":[],""fatti_nuovi"":[]}")

            Dim ragionamento As New Brainstorming(finto, Candidatura(), Profilo())
            Await ragionamento.ApriAsync(Nothing)
            Await ragionamento.RispondiAsync("dico la mia", Nothing)
            Await ragionamento.AppuntiAsync()

            Assert.Contains("Assistente: apro io", finto.UltimaConversazione, "l'AI")
            Assert.Contains("Utente: dico la mia", finto.UltimaConversazione, "l'utente")

            ' Il messaggio di contesto — profilo, annuncio, giudizi — non è conversazione:
            ' è il materiale su cui si conversa, e nella trascrizione non ci va.
            Assert.DoesNotContain("Magazziniere", finto.UltimaConversazione,
                                  "il contesto non entra nella trascrizione")

        End Function

        <TestMethod>
        Public Async Function SenzaUnaBattutaDellUtenteNonCEnienteDaDistillare() As Task
            ' Distillare il nulla costerebbe un'attesa per farsi rispondere una lista vuota.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io")

            Dim ragionamento As New Brainstorming(finto, Candidatura(), Profilo())

            Assert.IsFalse(ragionamento.SiPuoDistillare, "prima di cominciare, no")

            Await ragionamento.ApriAsync(Nothing)
            Assert.IsFalse(ragionamento.SiPuoDistillare, "e nemmeno dopo la sola apertura")

        End Function

        <TestMethod>
        Public Async Function DopoUnaBattutaDellUtenteSiPuoDistillare() As Task
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io").Dira("ti rispondo")

            Dim ragionamento As New Brainstorming(finto, Candidatura(), Profilo())
            Await ragionamento.ApriAsync(Nothing)
            Await ragionamento.RispondiAsync("ecco cosa penso", Nothing)

            Assert.IsTrue(ragionamento.SiPuoDistillare, "adesso sì")

        End Function

        <TestMethod>
        Public Async Function UnTurnoVuotoNonSiManda() As Task
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io")

            Dim ragionamento As New Brainstorming(finto, Candidatura(), Profilo())
            Await ragionamento.ApriAsync(Nothing)

            Await ragionamento.RispondiAsync("   ", Nothing)

            Assert.HasCount(1, finto.Chiamate, "uno spazio bianco non è un turno")

        End Function

    End Class

    ''' <summary>
    ''' Collaudi degli appunti di mira: come si leggono, come si salvano e — quello che
    ''' conta di più — cosa <b>non</b> arriva ai prompt che scrivono i documenti.
    ''' </summary>
    <TestClass>
    Public Class CollaudiAppuntiDiMira

        Private Const Distillati As String =
            "{""appunti"":[" &
            "{""tipo"":""enfasi"",""testo"":""Metti davanti i tre anni di magazzino"",""da"":""l'hai detto tu""}," &
            "{""tipo"":""tono"",""testo"":""Sobrio, niente entusiasmo"",""da"":""la conversazione""}]," &
            """fatti_nuovi"":[""ho il patentino del muletto""]}"

        <TestMethod>
        Public Sub SiLeggonoAppuntiEFattiNuovi()

            Dim appunti As AppuntiDiMira = AppuntiDiMira.DaJson(JsonNode.Parse(Distillati))

            Assert.HasCount(2, appunti.Appunti, "due appunti")
            Assert.AreEqual(TipiDiAppunto.Enfasi, appunti.Appunti(0).Tipo, "il tipo del primo")
            Assert.AreEqual("Metti davanti i tre anni di magazzino", appunti.Appunti(0).Testo, "il testo")
            Assert.HasCount(1, appunti.FattiNuovi, "e un fatto nuovo")
            Assert.IsFalse(appunti.Vuoti, "non sono vuoti")

        End Sub

        <TestMethod>
        Public Sub AiDocumentiVannoSoloGliAppunti()
            ' È il punto di tenerli separati: quello che l'utente ha detto in chat e nel
            ' profilo non c'è non deve entrare nei documenti da questa porta — sarebbe
            ' l'anti-invenzione scavalcata con le sue stesse parole.
            Dim appunti As AppuntiDiMira = AppuntiDiMira.DaJson(JsonNode.Parse(Distillati))

            Dim perIlPrompt As String = appunti.SoloAppunti().ToJsonString()

            Assert.Contains("magazzino", perIlPrompt, "gli appunti ci sono")
            Assert.DoesNotContain("fatti_nuovi", perIlPrompt, "i fatti nuovi no")
            Assert.DoesNotContain("patentino", perIlPrompt, "e nemmeno il loro contenuto")

        End Sub

        <TestMethod>
        Public Sub NelSalvataggioInveceRestanoTuttiEDue()
            ' Su disco i fatti nuovi si conservano: sono il promemoria di quel che l'utente
            ' ha detto e che nel profilo non c'è (anti-perdita).
            Dim appunti As AppuntiDiMira = AppuntiDiMira.DaJson(JsonNode.Parse(Distillati))

            Dim salvato As String = appunti.VersoJson().ToJsonString()

            Assert.Contains("magazzino", salvato, "gli appunti")
            Assert.Contains("patentino", salvato, "e i fatti nuovi")

        End Sub

        <TestMethod>
        Public Sub UnAppuntoSenzaTestoNonEUnAppunto()

            Dim appunti As AppuntiDiMira = AppuntiDiMira.DaJson(JsonNode.Parse(
                "{""appunti"":[{""tipo"":""enfasi"",""testo"":""  ""},{""tipo"":""tono"",""testo"":""sobrio""}]}"))

            Assert.HasCount(1, appunti.Appunti, "quello vuoto si scarta, l'altro resta")

        End Sub

        <TestMethod>
        Public Sub UnaRispostaStortaNonFaCadereNiente()
            ' È una scheda da confermare, non un file di configurazione: fermare tutto per
            ' una riga storta toglierebbe all'utente anche le altre.
            Assert.IsTrue(AppuntiDiMira.DaJson(Nothing).Vuoti, "niente non è un errore")
            Assert.IsTrue(AppuntiDiMira.DaJson(JsonNode.Parse("[]")).Vuoti, "nemmeno la forma sbagliata")
            Assert.IsTrue(AppuntiDiMira.DaJson(JsonNode.Parse("{""appunti"":""non è una lista""}")).Vuoti,
                          "né un campo del tipo sbagliato")

        End Sub

        <TestMethod>
        Public Sub UnTipoSconosciutoNonSiButtaVia()
            ' Perdere una riga che l'utente ha appena confermato sarebbe peggio che
            ' mostrargliela con un'etichetta storta.
            Dim appunti As AppuntiDiMira = AppuntiDiMira.DaJson(JsonNode.Parse(
                "{""appunti"":[{""tipo"":""fantasia"",""testo"":""qualcosa""}]}"))

            Assert.HasCount(1, appunti.Appunti, "l'appunto resta")
            Assert.AreEqual("fantasia", TipiDiAppunto.Etichetta("fantasia"), "col nome che gli è stato dato")

        End Sub

        <TestMethod>
        Public Sub SiTengonoSoloQuelliScelti()

            Dim appunti As AppuntiDiMira = AppuntiDiMira.DaJson(JsonNode.Parse(Distillati))
            Dim tenuti As AppuntiDiMira = appunti.Solo({appunti.Appunti(1)})

            Assert.HasCount(1, tenuti.Appunti, "uno solo")
            Assert.AreEqual(TipiDiAppunto.Tono, tenuti.Appunti(0).Tipo, "quello scelto")
            Assert.HasCount(1, tenuti.FattiNuovi, "i fatti nuovi restano comunque: non sono una scelta")

        End Sub

    End Class

End Namespace
