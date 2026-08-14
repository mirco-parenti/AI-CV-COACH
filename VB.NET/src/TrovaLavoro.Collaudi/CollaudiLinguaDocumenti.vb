Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi della regola che porta qualunque annuncio dentro una delle due lingue in
    ''' cui l'applicazione sa scrivere (cap. 10.1, cap. 10.5).
    ''' </summary>
    ''' <remarks>
    ''' Sono pochi e stanno tutti qui perché la regola sta in un posto solo: quattro punti
    ''' del programma le chiedono la stessa cosa — quale prompt, quali etichette, quale
    ''' sigla nel nome del file, cosa proporre in P6 — e se rispondesse in modo diverso a
    ''' uno di loro se ne accorgerebbe solo chi apre il documento finito.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiLinguaDocumenti

        <TestMethod>
        Public Sub LeDueLingueRestanoSeStesse()
            Assert.AreEqual("it", LinguaDocumenti.PerDocumenti("it"), "italiano")
            Assert.AreEqual("en", LinguaDocumenti.PerDocumenti("en"), "inglese")
        End Sub

        <TestMethod>
        Public Sub UnaLinguaNonDichiarataEItaliano()
            ' Le candidature nate prima del Pool 1.06 il campo «lingua» non ce l'hanno, e
            ' nessuna di loro era in inglese: far ricadere il vuoto sull'inglese le
            ' riscriverebbe tutte all'indietro (cap. 11.1, la lezione di T5c sugli stati).
            Assert.AreEqual("it", LinguaDocumenti.PerDocumenti(Nothing), "campo assente")
            Assert.AreEqual("it", LinguaDocumenti.PerDocumenti(""), "campo vuoto")
            Assert.AreEqual("it", LinguaDocumenti.PerDocumenti("   "), "campo di soli spazi")

            Assert.IsFalse(LinguaDocumenti.EStraniera(""), "un campo vuoto non è una terza lingua")
        End Sub

        <TestMethod>
        Public Sub UnaTerzaLinguaDiventaIngleseELoDichiara()
            ' Cap. 10.2: un annuncio in tedesco non ferma niente — si propone l'inglese e
            ' lo si dice. Le due metà vanno insieme: proporre l'inglese senza dichiararlo
            ' farebbe passare per scelta quello che è un adattamento.
            For Each terza As String In {"fr", "de", "es", "pt-BR"}
                Assert.AreEqual("en", LinguaDocumenti.PerDocumenti(terza), $"«{terza}» ripiega sull'inglese")
                Assert.IsTrue(LinguaDocumenti.EStraniera(terza), $"«{terza}» va dichiarata come terza lingua")
            Next
        End Sub

        <TestMethod>
        Public Sub NessunaDelleDueEStraniera()
            Assert.IsFalse(LinguaDocumenti.EStraniera("it"), "l'italiano")
            Assert.IsFalse(LinguaDocumenti.EStraniera("EN"), "l'inglese, comunque scritto")
        End Sub

        <TestMethod>
        Public Sub LaFormaDelCampoNonConta()
            ' Il campo arriva da un modello, non da una tendina: maiuscole e spazi attorno
            ' sono la norma, e trattarli come una terza lingua manderebbe in inglese un
            ' annuncio italiano.
            For Each scritta As String In {"IT", " it ", "It"}
                Assert.AreEqual("it", LinguaDocumenti.PerDocumenti(scritta), $"«{scritta}»")
            Next

            Assert.AreEqual("en", LinguaDocumenti.PerDocumenti(" EN "), "« EN »")
        End Sub

        <TestMethod>
        Public Sub IlNomeSiLeggeInItaliano()
            ' L'interfaccia resta in una lingua sola (cap. 10.1): la tendina di P6 dice
            ' «Inglese», non «English».
            Assert.AreEqual("Italiano", LinguaDocumenti.Nome("it"))
            Assert.AreEqual("Inglese", LinguaDocumenti.Nome("en"))
            Assert.AreEqual("Italiano", LinguaDocumenti.Nome(""), "il vuoto è la lingua di casa")
            Assert.AreEqual("Inglese", LinguaDocumenti.Nome("de"), "e una terza lingua si mostra come ciò che diventa")
        End Sub

    End Class

End Namespace
