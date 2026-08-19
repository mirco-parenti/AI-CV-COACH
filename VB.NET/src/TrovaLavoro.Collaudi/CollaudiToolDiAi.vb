Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Mcp
Imports TrovaLavoro.Motore

Namespace Mcp

    ''' <summary>
    ''' Collaudi dei sette tool che passano dall'AI (cap. 09.3, T8b).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Qui non si chiama nessuna AI</b>, e non è una rinuncia: si collauda tutto
    ''' quel che succede <i>prima</i> della chiamata, che è dove stanno le decisioni del
    ''' programma — la chiave che manca, il parametro che non è arrivato, il profilo che
    ''' non c'è, la mitigazione che non ha senso chiedere. Quel che l'AI risponde è
    ''' materia dei collaudi dei mestieri, che hanno i loro finti; qui si guarda la porta,
    ''' non la stanza.</para>
    ''' <para>Una chiave <b>vuota</b> non vuol dire «cercala dove sta» ma «non c'è, e non
    ''' cercarla»: è ciò che permette di provare il comportamento senza chiave anche su
    ''' una postazione che la chiave ce l'ha davvero.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiToolDiAi

        Private Const ChiaveFinta As String = "chiave-di-collaudo"

        ''' <summary>Tutti e sette, per le prove che valgono per ciascuno.</summary>
        Private Shared ReadOnly ToolDellAi As String() = {
            CatalogoTool.AnalizzaAnnuncio, CatalogoTool.Confronta, CatalogoTool.Mitiga,
            CatalogoTool.StrutturaCv, CatalogoTool.GeneraCv, CatalogoTool.GeneraLettera,
            CatalogoTool.RifinisciTesto}

        Private Shared Function CartellaTemporanea() As String
            Return Path.Combine(Path.GetTempPath(), "tool-ai-" & Guid.NewGuid().ToString("N"))
        End Function

        ''' <summary>Il motore con la chiave e i prompt: basta per tutto ciò che non chiama l'AI.</summary>
        Private Shared Function ConChiave(radice As String) As ContestoApp
            Return ContestoApp.Monta(radice, ChiaveFinta)
        End Function

        ''' <summary>Il motore senza chiave: la stringa vuota è un «no» esplicito.</summary>
        Private Shared Function SenzaChiave(radice As String) As ContestoApp
            Return ContestoApp.Monta(radice, "")
        End Function

        Private Shared Async Function Chiama(contesto As ContestoApp, nome As String,
                                             Optional argomenti As JsonObject = Nothing) As Task(Of EsitoTool)

            Dim catalogo As New CatalogoTool(contesto)
            Return Await catalogo.EseguiAsync(nome, If(argomenti, New JsonObject()))

        End Function

#Region "Senza chiave: si vedono, e dicono perché non possono"

        <TestMethod>
        Public Sub SenzaChiaveIToolDellAiRestanoInVetrina()
            ' La decisione di T8b: non si nascondono. Il client tiene l'elenco da parte e
            ' non gli abbiamo promesso nessun avviso di cambiamento, quindi una vetrina
            ' che dipende dalla chiave all'avvio sarebbe una vetrina che mente dopo.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = SenzaChiave(radice)

                Dim nomi As New List(Of String)
                For Each definito As DefinizioneTool In New CatalogoTool(contesto).Definizioni
                    nomi.Add(definito.Nome)
                Next

                For Each atteso As String In ToolDellAi
                    Assert.Contains(atteso, nomi, $"«{atteso}» deve restare in vetrina anche senza chiave")
                Next

            End Using
        End Sub

        <TestMethod>
        Public Async Function SenzaChiaveOgnunoFallisceDicendoDoveSiMetteLaChiave() As Task
            ' Un fallimento del tool, non un errore di protocollo: la frase è scritta per
            ' essere letta da un modello, che da lì può capire cosa fare invece di
            ' riprovare uguale.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = SenzaChiave(radice)

                For Each nome As String In ToolDellAi

                    Dim esito As EsitoTool = Await Chiama(contesto, nome)

                    Assert.IsTrue(esito.Fallito, $"«{nome}» non può lavorare senza chiave")
                    StringAssert.Contains(esito.Spiegazione, "chiave API",
                                          $"«{nome}» deve dire che manca la chiave")
                    StringAssert.Contains(esito.Spiegazione, "Impostazioni",
                                          $"«{nome}» deve dire dove si mette")

                Next

            End Using
        End Function

        <TestMethod>
        Public Async Function SenzaChiaveILettoriFunzionanoLoStesso() As Task
            ' L'altra metà della stessa frase: quel che non dipende dall'AI continua a
            ' funzionare, ed è quel che i messaggi promettono a chi legge.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = SenzaChiave(radice)

                Dim esito As EsitoTool = Await Chiama(contesto, CatalogoTool.LeggiRegistro)
                Assert.IsFalse(esito.Fallito, "il registro si legge anche senza chiave")

            End Using
        End Function

#End Region

#Region "I parametri che mancano"

        <TestMethod>
        Public Async Function OgniToolDiceQualeParametroGliManca() As Task
            ' Il nome del parametro va detto: un modello che riceve «manca qualcosa» non
            ' ha modo di correggersi, e riprova identico.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = ConChiave(radice)

                Dim attesi As New Dictionary(Of String, String) From {
                    {CatalogoTool.AnalizzaAnnuncio, "testo"},
                    {CatalogoTool.Confronta, "annuncio"},
                    {CatalogoTool.Mitiga, "giudizi"},
                    {CatalogoTool.StrutturaCv, "testo"},
                    {CatalogoTool.GeneraLettera, "annuncio"},
                    {CatalogoTool.RifinisciTesto, "testo"}}

                For Each atteso As KeyValuePair(Of String, String) In attesi

                    Dim esito As EsitoTool = Await Chiama(contesto, atteso.Key)

                    Assert.IsTrue(esito.Fallito, $"«{atteso.Key}» senza parametri non può fare niente")
                    StringAssert.Contains(esito.Spiegazione, atteso.Value,
                                          $"«{atteso.Key}» deve nominare il parametro che manca")

                Next

            End Using
        End Function

        <TestMethod>
        Public Async Function IlCvMiratoVuoleAnnuncioEGiudiziInsieme() As Task
            ' Mezzo segnale di mira è peggio di nessuno: uscirebbe un documento che sembra
            ' mirato e non lo è, e nessuno se ne accorgerebbe leggendolo.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = ConChiave(radice)

                Dim soloAnnuncio As EsitoTool = Await Chiama(
                    contesto, CatalogoTool.GeneraCv,
                    New JsonObject From {{"annuncio", New JsonObject()}})

                Assert.IsTrue(soloAnnuncio.Fallito, "con il solo annuncio non si genera")
                StringAssert.Contains(soloAnnuncio.Spiegazione, "giudizi", "e si dice cosa manca")

                Dim soloGiudizi As EsitoTool = Await Chiama(
                    contesto, CatalogoTool.GeneraCv,
                    New JsonObject From {{"giudizi", New JsonArray()}})

                Assert.IsTrue(soloGiudizi.Fallito, "né con i soli giudizi")
                StringAssert.Contains(soloGiudizi.Spiegazione, "annuncio", "e si dice cosa manca")

            End Using
        End Function

#End Region

#Region "Il profilo, che sta su disco e non nei parametri"

        <TestMethod>
        Public Async Function SenzaProfiloNonSiGeneraNienteESiDiceComeFarlo() As Task
            ' Il profilo non si passa come parametro apposta: un CV nasce dai fatti scritti
            ' nel file dell'utente, non da quelli che un modello riferisce di ricordare.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = ConChiave(radice)

                Dim esito As EsitoTool = Await Chiama(contesto, CatalogoTool.GeneraCv)

                Assert.IsTrue(esito.Fallito, "senza profilo non c'è niente da cui scrivere")
                StringAssert.Contains(esito.Spiegazione, "profilo", "va detto cos'è che manca")
                StringAssert.Contains(esito.Spiegazione, "applicazione", "e dove si costruisce")

            End Using
        End Function

#End Region

#Region "La scorciatoia della mitigazione"

        <TestMethod>
        Public Async Function SenzaGapLaMitigazioneEVuotaESiRisparmiaLaChiamata() As Task
            ' Deterministico e gratis: se nessun giudizio è «in parte» o «non soddisfatto»
            ' la lista uscirebbe vuota comunque. La prova che l'AI non è stata chiamata è
            ' che qui non c'è nessun profilo su disco — e senza profilo, una chiamata vera
            ' si sarebbe fermata prima con un altro messaggio.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = ConChiave(radice)

                Dim giudizi As New JsonArray From {
                    New JsonObject From {{"requisito", "VB.NET"}, {"esito", "soddisfatto"}},
                    New JsonObject From {{"requisito", "Inglese"}, {"esito", "non determinabile"}}}

                Dim esito As EsitoTool = Await Chiama(
                    contesto, CatalogoTool.Mitiga, New JsonObject From {{"giudizi", giudizi}})

                Assert.IsFalse(esito.Fallito, "senza gap non è un fallimento: è una lista vuota")

                Dim lista As JsonArray = TryCast(TryCast(esito.Dati, JsonObject)("mitigazioni"), JsonArray)
                Assert.IsNotNull(lista, "la forma resta quella di sempre")
                Assert.AreEqual(0, lista.Count, "e la lista è vuota")

            End Using
        End Function

        <TestMethod>
        Public Async Function ConUnGapLaMitigazioneSiChiedeDavvero() As Task
            ' Il rovescio della prova qui sopra: con un gap la scorciatoia non scatta, si
            ' va a cercare il profilo — e non trovandolo si fallisce per *quello*. È il
            ' modo di distinguere «non l'ho chiesto» da «l'ho chiesto e non si poteva».
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = ConChiave(radice)

                Dim giudizi As New JsonArray From {
                    New JsonObject From {{"requisito", "Kubernetes"}, {"esito", "non soddisfatto"}}}

                Dim esito As EsitoTool = Await Chiama(
                    contesto, CatalogoTool.Mitiga, New JsonObject From {{"giudizi", giudizi}})

                Assert.IsTrue(esito.Fallito, "con un gap si prosegue, e senza profilo non si può")
                StringAssert.Contains(esito.Spiegazione, "profilo", "ed è il profilo a mancare")

            End Using
        End Function

#End Region

#Region "La vetrina"

        <TestMethod>
        Public Sub GliSchemiDichiaranoIParametriObbligatori()
            ' Uno schema che non dichiara gli obbligatori lascia il modello a indovinare,
            ' e la prima chiamata sbagliata è un giro di attesa buttato.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = ConChiave(radice)

                Dim schemi As New Dictionary(Of String, String())

                For Each definito As DefinizioneTool In New CatalogoTool(contesto).Definizioni
                    schemi(definito.Nome) = Obbligatori(definito.Schema)
                Next

                CollectionAssert.AreEqual({"testo"}, schemi(CatalogoTool.AnalizzaAnnuncio), "analizza_annuncio")
                CollectionAssert.AreEqual({"annuncio"}, schemi(CatalogoTool.Confronta), "confronta")
                CollectionAssert.AreEqual({"giudizi"}, schemi(CatalogoTool.Mitiga), "mitiga")
                CollectionAssert.AreEqual({"testo"}, schemi(CatalogoTool.StrutturaCv), "struttura_cv")
                CollectionAssert.AreEqual({"annuncio", "giudizi", "cv"},
                                          schemi(CatalogoTool.GeneraLettera), "genera_lettera")
                CollectionAssert.AreEqual({"testo"}, schemi(CatalogoTool.RifinisciTesto), "rifinisci_testo")

                Assert.IsEmpty(schemi(CatalogoTool.GeneraCv),
                               "genera_cv non ha obbligatori: senza niente fa il CV base")

            End Using
        End Sub

        <TestMethod>
        Public Sub LaLinguaSiDichiaraConLeDueCheSappiamoScrivere()
            ' Un client che chiedesse il francese si vedrebbe rispondere in inglese: meglio
            ' che lo sappia dallo schema che dal risultato.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = ConChiave(radice)

                For Each nome As String In {CatalogoTool.GeneraCv, CatalogoTool.GeneraLettera,
                                            CatalogoTool.RifinisciTesto}

                    Dim schema As JsonObject = Nothing
                    For Each definito As DefinizioneTool In New CatalogoTool(contesto).Definizioni
                        If definito.Nome = nome Then schema = definito.Schema
                    Next

                    Dim lingua As JsonObject = TryCast(
                        TryCast(schema("properties"), JsonObject)("lingua"), JsonObject)

                    Assert.IsNotNull(lingua, $"«{nome}» deve accettare la lingua")

                    Dim ammesse As JsonArray = TryCast(lingua("enum"), JsonArray)
                    Assert.AreEqual(2, ammesse.Count, $"«{nome}»: le due lingue del pool")

                Next

            End Using
        End Sub

#End Region

        ''' <summary>I nomi dichiarati obbligatori da uno schema, in ordine.</summary>
        Private Shared Function Obbligatori(schema As JsonObject) As String()

            Dim elencati As JsonArray = TryCast(schema("required"), JsonArray)
            If elencati Is Nothing Then Return Array.Empty(Of String)()

            Dim nomi As New List(Of String)
            For Each nome As JsonNode In elencati
                nomi.Add(nome.GetValue(Of String)())
            Next

            Return nomi.ToArray()

        End Function

    End Class

End Namespace
