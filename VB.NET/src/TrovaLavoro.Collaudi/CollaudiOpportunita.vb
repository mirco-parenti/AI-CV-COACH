Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi dell'opportunità: le poche domande che si possono fare all'annuncio senza
    ''' entrare nel merito di quel che dice.
    ''' </summary>
    ''' <remarks>
    ''' Quella che conta è <c>AnnuncioVuoto</c>, che è il <b>rifiuto garbato</b> di
    ''' cap. 06.4 visto da dentro: il prompt restituisce lo schema con tutti i campi vuoti
    ''' quando il testo non è un annuncio di lavoro, e da quella risposta il programma deve
    ''' capire di avere in mano una pagina di elenco invece di un'offerta. La domanda è
    ''' <b>conservativa</b> di proposito — un annuncio scarno vale più del rischio di
    ''' buttare via una cattura buona — e questi collaudi sono il posto in cui quella scelta
    ''' è scritta nero su bianco.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiOpportunita

        <TestMethod>
        Public Sub SenzaAnnuncioLOpportunitaEVuota()

            Assert.IsTrue(New Opportunita().AnnuncioVuoto,
                          "un'opportunità che non ha ancora un annuncio non ha niente da confrontare")

        End Sub

        <TestMethod>
        Public Sub LoSchemaTuttoVuotoELaPaginaCheNonEUnAnnuncio()

            ' È esattamente quel che il prompt restituisce per una pagina di elenco.
            Assert.IsTrue(Con("
                {
                  ""competenze_richieste"": [], ""esperienza_richiesta"": [],
                  ""formazione_richiesta"": [], ""altri_requisiti"": [],
                  ""titolo"": """", ""azienda"": """", ""sede"": [],
                  ""contratto"": { ""tipo"": """" }, ""mansioni"": [], ""benefit"": []
                }").AnnuncioVuoto)

        End Sub

        <TestMethod>
        Public Sub BastaUnaSolaCosaPercheLAnnuncioValga()

            ' Cinque campi, uno per volta: se c'è quello, l'annuncio si analizza. Un
            ' annuncio scarno esiste, e rifiutarlo sarebbe peggio che leggerlo.
            Dim uno As String() = {
                """titolo"": ""Magazziniere""",
                """competenze_richieste"": [{ ""testo"": ""Uso del muletto"" }]",
                """esperienza_richiesta"": [{ ""testo"": ""1 anno in magazzino"" }]",
                """formazione_richiesta"": [{ ""testo"": ""Licenza media"" }]",
                """altri_requisiti"": [{ ""testo"": ""Patente B"" }]",
                """mansioni"": [""Carico e scarico""]"}

            For Each solo As String In uno
                Assert.IsFalse(Con("{" & solo & "}").AnnuncioVuoto,
                               $"con {solo} l'annuncio non è vuoto")
            Next

        End Sub

        <TestMethod>
        Public Sub IlContornoDaSoloNonFaUnAnnuncio()

            ' Azienda, sede e benefit una pagina di elenco può averli — il nome del
            ' portale, la città della ricerca — proprio mentre di offerte non ne descrive
            ' nessuna. Da soli non bastano.
            Assert.IsTrue(Con("
                {
                  ""titolo"": """", ""azienda"": ""Indeed"", ""sede"": [""Genova""],
                  ""benefit"": [""Buoni pasto""], ""mansioni"": []
                }").AnnuncioVuoto)

        End Sub

        <TestMethod>
        Public Sub UnaRispostaFuoriSchemaValeComeVuota()

            ' Se al posto delle liste arriva altro, non si può dire che ci sia qualcosa:
            ' meglio il rifiuto garbato di un confronto su una forma che non conosciamo.
            Assert.IsTrue(Con("{ ""titolo"": 12, ""competenze_richieste"": ""nessuna"" }").AnnuncioVuoto)

        End Sub

        Private Shared Function Con(annuncioJson As String) As Opportunita
            Return New Opportunita With {.Annuncio = JsonNode.Parse(annuncioJson)}
        End Function

    End Class

End Namespace
