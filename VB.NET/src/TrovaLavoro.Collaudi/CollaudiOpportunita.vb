Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
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

        ' ==================================================================
        ' La lettera rimasta indietro (R7, 2026-08-23)
        ' ==================================================================

        <TestMethod>
        Public Sub SenzaRiscrittureLaLetteraNonEMaiIndietro()

            ' Il caso normale, ed è la maggioranza: CV e lettera li ha scritti l'AI di
            ' seguito, sullo stesso profilo. Non c'è niente da riallineare, e una spia che
            ' si accendesse qui insegnerebbe solo a non guardarla.
            Assert.IsFalse(ConDocumenti().LetteraDaRiallineare)

        End Sub

        <TestMethod>
        Public Sub IlCvRiscrittoDopoLaLetteraLaLasciaIndietro()

            ' È il difetto di R7 visto da dentro: l'utente cambia «trasloco» in «elefante»
            ' nel CV, e la lettera continua a raccontare i traslochi.
            Dim candidatura As Opportunita = ConDocumenti()
            candidatura.SegnaLetteraGenerata(Ieri)
            candidatura.SegnaRiscritture(RuoloDocumento.Cv, {"sommario"}, Oggi)

            Assert.IsTrue(candidatura.LetteraDaRiallineare)

        End Sub

        <TestMethod>
        Public Sub UnaLetteraScrittaDopoLaRiscritturaLHaGiaVista()

            ' Ed è ciò che deve succedere subito dopo il riallineo: la spia si spegne senza
            ' che nessuno debba cancellare l'annotazione del CV, che resta vera.
            Dim candidatura As Opportunita = ConDocumenti()
            candidatura.SegnaRiscritture(RuoloDocumento.Cv, {"sommario"}, Ieri)
            candidatura.SegnaLetteraGenerata(Oggi)

            Assert.IsFalse(candidatura.LetteraDaRiallineare, "la lettera è più recente della riscrittura")
            Assert.IsTrue(candidatura.RiscrittureDelCv.CEQualcosa, "ma il CV resta un documento riscritto a mano")

        End Sub

        <TestMethod>
        Public Sub RiscrivereLaLetteraNonDisallineaNiente()

            ' Il verso conta: il CV racconta, la lettera ripete. Chi corregge una frase
            ' della lettera non ha reso il CV vecchio di un minuto.
            Dim candidatura As Opportunita = ConDocumenti()
            candidatura.SegnaLetteraGenerata(Ieri)
            candidatura.SegnaRiscritture(RuoloDocumento.Lettera, {"corpo"}, Oggi)

            Assert.IsFalse(candidatura.LetteraDaRiallineare)

        End Sub

        <TestMethod>
        Public Sub SenzaLetteraNonCENienteDaRiallineare()

            Dim candidatura As Opportunita = ConDocumenti()
            candidatura.Lettera = Nothing
            candidatura.SegnaRiscritture(RuoloDocumento.Cv, {"sommario"}, Oggi)

            Assert.IsFalse(candidatura.LetteraDaRiallineare, "una lettera che non c'è non è indietro")

        End Sub

        <TestMethod>
        Public Sub UnaLetteraSenzaDataConUnCvRiscrittoSiDaPerIndietro()

            ' Le candidature scritte prima di R7 non hanno la data della lettera, e nemmeno
            ' quelle arrivate da «salva_opportunita» (cap. 09.3). Il dubbio si scioglie dal
            ' lato prudente: costa un avviso a chi non ne aveva bisogno, mentre l'altro
            ' verso costerebbe il silenzio da cui R7 nasce.
            Dim candidatura As Opportunita = ConDocumenti()
            candidatura.SegnaRiscritture(RuoloDocumento.Cv, {"sommario"}, Oggi)

            Assert.IsTrue(candidatura.LetteraDaRiallineare)

        End Sub

        <TestMethod>
        Public Sub QuandoLaLetteraSiRifaLeSueRiscrittureSeNeVanno()

            ' Quel che l'utente aveva scritto *nella lettera* non c'è più: l'ha sostituito
            ' il testo nuovo, e continuare ad annotarlo prometterebbe in un avviso un
            ' lavoro che nel file non esiste. Le riscritture del CV invece restano: il CV
            ' non l'ha toccato nessuno.
            Dim candidatura As Opportunita = ConDocumenti()
            candidatura.SegnaRiscritture(RuoloDocumento.Cv, {"sommario"}, Ieri)
            candidatura.SegnaRiscritture(RuoloDocumento.Lettera, {"corpo"}, Ieri)

            candidatura.SegnaLetteraGenerata(Oggi)

            Assert.IsFalse(candidatura.RiscrittureDellaLettera.CEQualcosa, "la lettera è tutta dell'AI, adesso")
            Assert.AreEqual("sommario", String.Join(", ", candidatura.RiscrittureDelCv.Campi),
                            "e il CV è ancora quello che ha riscritto l'utente")

        End Sub

        Private Shared ReadOnly Ieri As New Date(2026, 8, 22, 10, 0, 0)
        Private Shared ReadOnly Oggi As New Date(2026, 8, 23, 18, 40, 0)

        ''' <summary>Una candidatura con i suoi due documenti già scritti.</summary>
        Private Shared Function ConDocumenti() As Opportunita

            Return New Opportunita With {
                .Cv = JsonNode.Parse("{""tipo"": ""cv_mirato"", ""sommario"": ""Traslochi.""}"),
                .Lettera = JsonNode.Parse("{""tipo"": ""lettera_mirata"", ""corpo"": ""Ho fatto traslochi.""}")}

        End Function

        Private Shared Function Con(annuncioJson As String) As Opportunita
            Return New Opportunita With {.Annuncio = JsonNode.Parse(annuncioJson)}
        End Function

    End Class

End Namespace
