Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi della memoria delle riscritture a mano (R7, 2026-08-23): quali campi di
    ''' prosa ha riscritto l'utente, quando, e come quel poco sopravvive a un giro su
    ''' disco.
    ''' </summary>
    ''' <remarks>
    ''' Il difetto da cui nasce non era in questi metodi — non esistevano — ma nel fatto
    ''' che l'informazione viveva in un booleano di sessione: bastava cambiare pannello
    ''' perché «Rigenera» smettesse di avvisare che stava per buttare via il lavoro
    ''' dell'utente. Qui si verifica il minimo che rende quell'avviso una promessa
    ''' mantenibile: che l'annotazione non si duplichi, non ingombri quando è vuota, e
    ''' torni identica da un file.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiRiscrittureAMano

        Private Shared ReadOnly Ieri As New Date(2026, 8, 22, 10, 0, 0)
        Private Shared ReadOnly Oggi As New Date(2026, 8, 23, 18, 40, 0)

        <TestMethod>
        Public Sub RiscrivereDueVolteLoStessoCampoNonLoDuplica()

            Dim riscritture As New RiscrittureAMano

            riscritture.Annota("sommario", Ieri)
            riscritture.Annota("sommario", Oggi)

            Assert.HasCount(1, riscritture.Campi, "è pur sempre un campo riscritto")
            Assert.AreEqual(Oggi, riscritture.Quando, "ma la data è quella dell'ultima volta")

        End Sub

        <TestMethod>
        Public Sub UnCampoSenzaNomeNonSiAnnota()

            Dim riscritture As New RiscrittureAMano

            riscritture.Annota(Nothing, Oggi)
            riscritture.Annota("   ", Oggi)

            Assert.IsFalse(riscritture.CEQualcosa, "un id vuoto non è un campo")

        End Sub

        <TestMethod>
        Public Sub QuandoNonCENienteIlBloccoNonSiScriveAffatto()

            ' È la promessa fatta ai file già scritti: una candidatura che nessuno ha
            ' toccato a mano resta sul disco esattamente com'era (cap. 11.1).
            Assert.IsNull(New RiscrittureAMano().ComeJson(), "niente da scrivere, niente scritto")

        End Sub

        <TestMethod>
        Public Sub QuelCheSiScriveSiRilegge()

            Dim scritte As New RiscrittureAMano
            scritte.Annota("sommario", Oggi)
            scritte.Annota("esperienza.1", Oggi)

            Dim rilette As New RiscrittureAMano
            rilette.Rileggi(scritte.ComeJson())

            Assert.AreEqual("sommario, esperienza.1", String.Join(", ", rilette.Campi),
                            "gli stessi campi, nello stesso ordine")
            Assert.AreEqual(Oggi, rilette.Quando, "e lo stesso istante")

        End Sub

        <TestMethod>
        Public Sub UnFileSenzaIlBloccoValeComeMaiToccatoAMano()

            ' Tutte le candidature scritte prima di R7 stanno così, ed è la ragione per cui
            ' la rilettura non deve inventarsi niente: non si deduce all'indietro una
            ' storia che nessuno ha registrato.
            Dim rilette As New RiscrittureAMano
            rilette.Annota("sommario", Oggi)

            rilette.Rileggi(Nothing)

            Assert.IsFalse(rilette.CEQualcosa, "il documento non ha riscritture")
            Assert.AreEqual(Date.MinValue, rilette.Quando, "e nemmeno una data")

        End Sub

        <TestMethod>
        Public Sub UnBloccoStortoNonFaDanno()

            ' Il pool è aperto e i file della cartella dati si possono correggere a mano:
            ' un blocco che non ha la forma attesa vale come assente, non come un guasto
            ' (è la regola di CampiJson).
            Dim rilette As New RiscrittureAMano
            rilette.Rileggi(TryCast(JsonNode.Parse("{""campi"": ""sommario"", ""quando"": 12}"), JsonObject))

            Assert.IsFalse(rilette.CEQualcosa, "un elenco che non è un elenco non porta campi")

        End Sub

        <TestMethod>
        Public Sub NellElencoRilettoLeVociVuoteELeRipetizioniNonEntrano()

            Dim rilette As New RiscrittureAMano
            rilette.Rileggi(TryCast(JsonNode.Parse(
                "{""campi"": [""sommario"", """", ""sommario"", null, 7, ""corpo""]}"), JsonObject))

            Assert.AreEqual("sommario, corpo", String.Join(", ", rilette.Campi),
                            "restano i campi veri, una volta sola")

        End Sub

        <TestMethod>
        Public Sub DimenticareCancellaCampiEData()

            ' È quel che succede a un documento riscritto da capo dall'AI: le parole
            ' dell'utente non ci sono più, e prometterle in un avviso sarebbe falso.
            Dim riscritture As New RiscrittureAMano
            riscritture.Annota("corpo", Oggi)

            riscritture.Dimentica()

            Assert.IsFalse(riscritture.CEQualcosa, "niente campi")
            Assert.AreEqual(Date.MinValue, riscritture.Quando, "e niente data")

        End Sub

        <TestMethod>
        Public Sub PrendereCopiaQuelCheCEAltrove()

            Dim altre As New RiscrittureAMano
            altre.Annota("sommario", Oggi)

            Dim mie As New RiscrittureAMano
            mie.Annota("corpo", Ieri)
            mie.Prendi(altre)

            Assert.AreEqual("sommario", String.Join(", ", mie.Campi), "quel che c'era prima è andato")
            Assert.AreEqual(Oggi, mie.Quando, "con la data dell'altra")

            altre.Annota("esperienza.1", Oggi)
            Assert.HasCount(1, mie.Campi, "e le due liste restano separate")

        End Sub

    End Class

End Namespace
