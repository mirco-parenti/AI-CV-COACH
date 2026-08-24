Imports System.Linq
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti

Namespace Documenti

    ''' <summary>
    ''' Collaudi delle voci togliibili di un CV (R6, cap. 08.4): chi sono, come si
    ''' riconoscono domani, e cosa resta del documento quando l'utente ne toglie una.
    ''' </summary>
    ''' <remarks>
    ''' Il collaudo che conta davvero è quello sull'<b>impronta</b>. Una voce tolta deve
    ''' restare tolta anche dopo un «Rigenera», e il documento nuovo lo scrive il modello:
    ''' se l'impronta cambiasse insieme alla prosa, l'esclusione non si riaggancerebbe e
    ''' la voce tornerebbe da sola. Se guardasse la posizione, peggio: toglierebbe la voce
    ''' che nel frattempo ha preso quel posto, e in silenzio.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiVociDelCv

        <TestMethod>
        Public Sub LeQuattroListeDiventanoVociConIlLoroNome()

            Dim voci As List(Of VoceDelCv) = VociDelCv.Elenca(CvDiProva())

            Assert.HasCount(5, voci, "una esperienza, una altra, due competenze, un titolo")

            Assert.AreEqual("Esperienza 1", voci(0).Etichetta, "la prima si chiama così")
            Assert.AreEqual("Cameriere — Trattoria Da Gino", voci(0).Riepilogo, "e si riconosce dai fatti")
            Assert.AreEqual("Altra esperienza 1", voci(1).Etichetta, "poi le altre esperienze")
            Assert.AreEqual("Competenza 1", voci(2).Etichetta, "poi le competenze")
            Assert.AreEqual("Servizio ai tavoli", voci(2).Riepilogo, "che sono già il loro testo")
            Assert.AreEqual("Titolo di studio 1", voci(4).Etichetta, "e infine gli studi")

        End Sub

        <TestMethod>
        Public Sub LaProsaRiscrittaNonCambiaLimprontaDellaVoce()

            ' Il collaudo centrale. La descrizione la riscrive il modello a ogni giro — e
            ' l'utente può riscriverla a mano (R7). Se entrasse nell'impronta, dopo la
            ' prima rigenerazione la voce tolta tornerebbe da sé.
            Dim cv As JsonNode = CvDiProva()
            Dim prima As String = ImprontaEsperienza(cv, 0)

            CType(CType(cv("esperienze_professionali"), JsonArray)(0), JsonObject)("descrizione") =
                "Tutt'altre parole, scritte da un altro giro del modello."

            Assert.AreEqual(prima, ImprontaEsperienza(cv, 0),
                            "cambia la prosa, la voce è sempre quella")

        End Sub

        <TestMethod>
        Public Sub CambiareUnFattoCambiaLaVoce()

            ' La controprova: se l'impronta non cambiasse mai, non distinguerebbe niente.
            Dim cv As JsonNode = CvDiProva()
            Dim prima As String = ImprontaEsperienza(cv, 0)

            CType(CType(cv("esperienze_professionali"), JsonArray)(0), JsonObject)("azienda") =
                "Osteria del Ponte"

            Assert.AreNotEqual(prima, ImprontaEsperienza(cv, 0),
                               "un'altra azienda è un'altra esperienza")

        End Sub

        <TestMethod>
        Public Sub SpaziEMaiuscoleNonFannoUnAltraVoce()

            ' Il modello ritocca la tipografia senza avvisare: due scritture della stessa
            ' cosa devono restare la stessa voce, o l'esclusione si perde per un doppio
            ' spazio.
            Dim cv As JsonNode = CvDiProva()
            Dim prima As String = ImprontaEsperienza(cv, 0)

            Dim voce As JsonObject = CType(CType(cv("esperienze_professionali"), JsonArray)(0), JsonObject)
            voce("ruolo") = "  CAMERIERE  "
            voce("azienda") = "Trattoria  Da  Gino"

            Assert.AreEqual(prima, ImprontaEsperienza(cv, 0), "è sempre lo stesso ruolo")

        End Sub

        <TestMethod>
        Public Sub UnaVoceSenzaFattiNonSiPuoNominare()

            ' E quindi non si può nemmeno togliere: senza impronta, «togli questa» non
            ' saprebbe quale, e ne toglierebbe un'altra.
            Assert.IsNull(VociDelCv.ImprontaDi("esperienze_professionali",
                                               JsonNode.Parse("{ ""descrizione"": ""Solo prosa."" }")),
                          "un'esperienza senza ruolo, azienda e durata")

            Assert.IsNull(VociDelCv.ImprontaDi("competenze", JsonNode.Parse("""   """)),
                          "una competenza fatta di spazi")

        End Sub

        <TestMethod>
        Public Sub LaVoceToltaSparisceELeAltreRestano()

            Dim cv As JsonNode = CvDiProva()
            Dim tolte As New VociTolte()
            tolte.Togli(VociDelCv.Elenca(cv).Single(Function(v) v.Riepilogo = "Uso del registratore di cassa").Impronta,
                        New Date(2026, 8, 24))

            Dim visto As JsonNode = VociDelCv.ComeSiVede(cv, tolte)

            Dim competenze As JsonArray = CType(visto("competenze"), JsonArray)
            Assert.HasCount(1, competenze, "delle due ne resta una")
            Assert.AreEqual("Servizio ai tavoli", competenze(0).GetValue(Of String)(), "quella non tolta")
            Assert.HasCount(1, CType(visto("esperienze_professionali"), JsonArray),
                            "le altre sezioni non si toccano")

        End Sub

        <TestMethod>
        Public Sub IlDocumentoOriginaleNonSiTocca()

            ' È il patto di R6: su disco il CV resta intero, e rimettere una voce non
            ' costa una rigenerazione. Se «ComeSiVede» tagliasse l'originale, la voce
            ' tolta sarebbe persa per sempre al primo salvataggio.
            Dim cv As JsonNode = CvDiProva()
            Dim tolte As New VociTolte()
            tolte.Togli(VociDelCv.Elenca(cv).First().Impronta, New Date(2026, 8, 24))

            VociDelCv.ComeSiVede(cv, tolte)

            Assert.HasCount(1, CType(cv("esperienze_professionali"), JsonArray),
                            "l'esperienza è ancora nel documento vero")

        End Sub

        <TestMethod>
        Public Sub SenzaNienteDaTogliereIlCvRestaQuelloCheEra()

            Dim cv As JsonNode = CvDiProva()

            Assert.AreSame(cv, VociDelCv.ComeSiVede(cv, New VociTolte()),
                           "niente tolto, niente copiato")
            Assert.AreSame(cv, VociDelCv.ComeSiVede(cv, Nothing),
                           "e nemmeno quando nessuno ha mai tolto niente")

        End Sub

        <TestMethod>
        Public Sub UnaVoceToltaRestaToltaAncheSeCambiaDiPosto()

            ' Quel che l'indice non sa fare: il documento rigenerato mette la voce altrove
            ' e con altre parole, e lei deve restare fuori lo stesso.
            Dim cv As JsonNode = CvDiProva()
            Dim tolte As New VociTolte()
            tolte.Togli(VociDelCv.Elenca(cv).First(Function(v) v.Riepilogo = "Servizio ai tavoli").Impronta,
                        New Date(2026, 8, 24))

            Dim rifatto As JsonNode = JsonNode.Parse("
                {
                  ""competenze"": [""Uso del registratore di cassa"", ""Gestione delle prenotazioni"",
                                   ""Servizio ai tavoli""]
                }")

            Dim visto As JsonNode = VociDelCv.ComeSiVede(rifatto, tolte)

            Assert.HasCount(2, CType(visto("competenze"), JsonArray), "la sua è fuori anche adesso")
            Assert.IsFalse(CType(visto("competenze"), JsonArray).
                           Any(Function(c) c.GetValue(Of String)() = "Servizio ai tavoli"),
                           "ed è proprio quella che l'utente aveva tolto")

        End Sub

        Private Shared Function ImprontaEsperienza(cv As JsonNode, indice As Integer) As String

            Return VociDelCv.ImprontaDi("esperienze_professionali",
                                        CType(cv("esperienze_professionali"), JsonArray)(indice))

        End Function

        ''' <summary>Un CV pieno, con tutte e quattro le liste. Dati finti e parlanti.</summary>
        Private Shared Function CvDiProva() As JsonNode

            Return JsonNode.Parse("
                {
                  ""tipo"": ""cv_base"",
                  ""intestazione"": { ""nome"": ""Luca Ferrari"", ""citta"": ""Modena"" },
                  ""sommario"": ""Ho esperienza nel servizio di sala."",
                  ""esperienze_professionali"": [
                    { ""ruolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino"",
                      ""durata"": ""2019-2023"", ""descrizione"": ""Servizio ai tavoli e cassa."" }
                  ],
                  ""altre_esperienze"": [
                    { ""descrizione"": ""Volontariato alla sagra del paese"", ""quando"": ""estate 2021"" }
                  ],
                  ""competenze"": [""Servizio ai tavoli"", ""Uso del registratore di cassa""],
                  ""formazione"": [
                    { ""titolo"": ""Diploma alberghiero"", ""istituto"": ""IPSSAR Modena"", ""anno"": ""2018"" }
                  ]
                }")

        End Function

    End Class

End Namespace
