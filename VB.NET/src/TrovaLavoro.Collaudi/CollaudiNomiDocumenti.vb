Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Documenti

Namespace Documenti

    ''' <summary>
    ''' Collaudi dei nomi dei file prodotti (cap. 05.6). Sono nomi che escono di casa —
    ''' allegati a un'email, scaricati da chi li riceve — e devono restare leggibili e
    ''' innocui su qualunque sistema.
    ''' </summary>
    <TestClass>
    Public Class CollaudiNomiDocumenti

        Private Shared ReadOnly Giorno As New Date(2026, 8, 10)

        <TestMethod>
        Public Sub IlCvMiratoPortaPersonaAziendaEData()

            Assert.AreEqual("CV_Luca_Ferrari_Trattoria_Da_Gino_2026-08-10",
                            NomiDocumenti.Cv("Luca Ferrari", "Trattoria Da Gino", Giorno))

        End Sub

        <TestMethod>
        Public Sub IlCvBaseLasciaFuoriLAzienda()

            ' Il CV base non nasce da un annuncio: un'azienda non ce l'ha, e il nome non
            ' se la inventa (cap. 05.6).
            Assert.AreEqual("CV_Luca_Ferrari_2026-08-10",
                            NomiDocumenti.Cv("Luca Ferrari", "", Giorno))

        End Sub

        <TestMethod>
        Public Sub LaLetteraSiRiconosceDallAzienda()

            Assert.AreEqual("Lettera_Rossi_Figli_S_p_A_2026-08-10",
                            NomiDocumenti.Lettera("Rossi & Figli S.p.A.", Giorno))

        End Sub

        <TestMethod>
        Public Sub GliAccentiSiSciolgonoNellaLoroLettera()

            Assert.AreEqual("CV_Luca_Ferrari_2026-08-10",
                            NomiDocumenti.Cv("Luca Ferrarì", "", Giorno))

        End Sub

        <TestMethod>
        Public Sub LaLinguaDiversaDallItalianoSiVedeNelNome()

            ' L'italiano non si scrive perché è la lingua di casa; l'inglese sì, e arriva
            ' con T7 (cap. 05.6, cap. 10).
            Assert.AreEqual("CV_Luca_Ferrari_EN_Acme_2026-08-10",
                            NomiDocumenti.Cv("Luca Ferrari", "Acme", Giorno, "en"))
            Assert.AreEqual("CV_Luca_Ferrari_Acme_2026-08-10",
                            NomiDocumenti.Cv("Luca Ferrari", "Acme", Giorno, "it"))

        End Sub

        <TestMethod>
        Public Sub UnNomeLunghissimoSiTaglia()

            Dim nome As String = NomiDocumenti.Cv("Luca Ferrari", New String("a"c, 200), Giorno)

            Assert.IsLessThan(100, nome.Length, $"Nome troppo lungo: {nome.Length} caratteri.")
            StringAssert.EndsWith(nome, "_2026-08-10")

        End Sub

        <TestMethod>
        Public Sub NelNomeNonFinisceMaiUnCarattereChePuoRompereUnPercorso()

            Dim nome As String = NomiDocumenti.Cv("Luca / Ferrari", "C:\\Rossi *2* <casa>", Giorno)

            For Each vietato As Char In IO.Path.GetInvalidFileNameChars()
                Assert.DoesNotContain(vietato.ToString(), nome,
                                      $"Nel nome è finito un carattere vietato: «{vietato}».")
            Next

            Assert.AreEqual("CV_Luca_Ferrari_C_Rossi_2_casa_2026-08-10", nome)

        End Sub

        <TestMethod>
        Public Sub SenzaNomeESenzaAziendaRestaIlDocumentoDatato()

            Assert.AreEqual("CV_2026-08-10", NomiDocumenti.Cv("", "", Giorno))
            Assert.AreEqual("Lettera_2026-08-10", NomiDocumenti.Lettera(Nothing, Giorno))

        End Sub

    End Class

End Namespace
