Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi della memoria delle voci tolte da un documento (R6, cap. 08.4). È la
    ''' gemella di <see cref="CollaudiRiscrittureAMano"/>, e per le stesse ragioni: quel
    ''' che l'utente decide sul suo documento deve sopravvivere alla chiusura del
    ''' programma, e un file scritto prima non deve cambiare di senso.
    ''' </summary>
    <TestClass>
    Public Class CollaudiVociTolte

        <TestMethod>
        Public Sub ToglierDueVolteLaStessaVoceNonLaDuplica()

            Dim tolte As New VociTolte()
            tolte.Togli("competenze¦uso del muletto", New Date(2026, 8, 24, 10, 0, 0))
            tolte.Togli("competenze¦uso del muletto", New Date(2026, 8, 24, 11, 30, 0))

            Assert.HasCount(1, tolte.Impronte, "una voce tolta due volte resta una voce")
            Assert.AreEqual(New Date(2026, 8, 24, 11, 30, 0), tolte.Quando,
                            "ma la data è quella dell'ultima volta")

        End Sub

        <TestMethod>
        Public Sub RimettereUnaVoceLaTogliDallElenco()

            Dim tolte As New VociTolte()
            tolte.Togli("competenze¦uso del muletto", New Date(2026, 8, 24, 10, 0, 0))
            tolte.Togli("formazione¦licenza media", New Date(2026, 8, 24, 10, 5, 0))

            tolte.Rimetti("competenze¦uso del muletto", New Date(2026, 8, 24, 12, 0, 0))

            Assert.HasCount(1, tolte.Impronte, "ne resta tolta una sola")
            Assert.IsFalse(tolte.Contiene("competenze¦uso del muletto"), "quella rimessa è tornata")
            Assert.IsTrue(tolte.Contiene("formazione¦licenza media"), "l'altra è ancora fuori")
            Assert.AreEqual(New Date(2026, 8, 24, 12, 0, 0), tolte.Quando,
                            "anche rimettere cambia quel che il CV racconta")

        End Sub

        <TestMethod>
        Public Sub RimettereQualcosaCheNonEraToltoNonCambiaLaData()

            Dim tolte As New VociTolte()
            tolte.Togli("competenze¦uso del muletto", New Date(2026, 8, 24, 10, 0, 0))

            tolte.Rimetti("competenze¦mai tolta", New Date(2026, 8, 24, 18, 0, 0))

            Assert.AreEqual(New Date(2026, 8, 24, 10, 0, 0), tolte.Quando,
                            "non è successo niente, e la spia della lettera non deve crederlo")

        End Sub

        <TestMethod>
        Public Sub QuandoNonCENienteIlBloccoNonSiScriveAffatto()

            ' Un documento da cui non si è tolto niente deve restare, nel file, identico a
            ' com'era prima che R6 esistesse.
            Assert.IsNull(New VociTolte().ComeJson(), "niente da dire, niente da scrivere")

        End Sub

        <TestMethod>
        Public Sub QuelCheSiScriveSiRilegge()

            Dim tolte As New VociTolte()
            tolte.Togli("esperienze_professionali¦cameriere¦trattoria da gino¦2019-2023",
                        New Date(2026, 8, 24, 9, 15, 0))
            tolte.Togli("competenze¦uso del registratore di cassa", New Date(2026, 8, 24, 9, 20, 0))

            Dim riletto As New VociTolte()
            riletto.Rileggi(tolte.ComeJson())

            Assert.HasCount(2, riletto.Impronte, "tutte e due")
            Assert.IsTrue(riletto.Contiene("competenze¦uso del registratore di cassa"), "con la loro impronta")
            Assert.AreEqual(New Date(2026, 8, 24, 9, 20, 0), riletto.Quando, "e la data")

        End Sub

        <TestMethod>
        Public Sub UnFileSenzaIlBloccoValeComeDocumentoIntero()

            Dim tolte As New VociTolte()
            tolte.Togli("competenze¦uso del muletto", New Date(2026, 8, 24))

            tolte.Rileggi(Nothing)

            Assert.IsFalse(tolte.CEQualcosa,
                           "i documenti scritti prima di R6 si riaprono interi, non a metà")

        End Sub

        <TestMethod>
        Public Sub UnBloccoStortoNonFaDanno()

            Dim tolte As New VociTolte()
            tolte.Rileggi(CType(JsonNode.Parse("{ ""voci"": [null, 12, """", ""competenze¦buona"",
                                                            ""competenze¦buona""] }"), JsonObject))

            Assert.HasCount(1, tolte.Impronte, "si tiene solo quel che è una voce, e una volta sola")
            Assert.IsTrue(tolte.Contiene("competenze¦buona"), "ed è quella")

        End Sub

        <TestMethod>
        Public Sub PrendereCopiaQuelCheCEAltrove()

            Dim sul As New VociTolte()
            sul.Togli("competenze¦uso del muletto", New Date(2026, 8, 24, 8, 0, 0))

            Dim inMano As New VociTolte()
            inMano.Togli("formazione¦roba vecchia", New Date(2026, 8, 1))
            inMano.Prendi(sul)

            Assert.HasCount(1, inMano.Impronte, "quel che aveva prima non conta più")
            Assert.IsTrue(inMano.Contiene("competenze¦uso del muletto"), "conta quel che c'è nel file")
            Assert.AreEqual(New Date(2026, 8, 24, 8, 0, 0), inMano.Quando, "con la sua data")

        End Sub

        <TestMethod>
        Public Sub DimenticareCancellaVociEData()

            Dim tolte As New VociTolte()
            tolte.Togli("competenze¦uso del muletto", New Date(2026, 8, 24))

            tolte.Dimentica()

            Assert.IsFalse(tolte.CEQualcosa, "niente più tolto")
            Assert.AreEqual(Nothing, tolte.Quando, "e nessuna data da confrontare")

        End Sub

    End Class

End Namespace
