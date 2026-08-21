Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi della ripulitura del Markdown nelle bolle di P5 (T9d): che i segni se ne
    ''' vadano, che il testo resti tutto, e che non si porti via quel che segno non è.
    ''' </summary>
    <TestClass>
    Public Class CollaudiProsaDellAssistente

        <TestMethod>
        Public Sub IlGrassettoEIlCorsivoSeNeVannoEIlTestoResta()

            Assert.AreEqual("Un profilo curato conta.",
                            ProsaDellAssistente.SenzaMarkdown("Un profilo **curato** conta."))

            Assert.AreEqual("Un profilo curato conta.",
                            ProsaDellAssistente.SenzaMarkdown("Un profilo __curato__ conta."))

            Assert.AreEqual("Un profilo curato conta.",
                            ProsaDellAssistente.SenzaMarkdown("Un profilo *curato* conta."))

            Assert.AreEqual("Un profilo curato conta.",
                            ProsaDellAssistente.SenzaMarkdown("Un profilo _curato_ conta."))

        End Sub

        ''' <summary>
        ''' Il caso che una regex scritta di fretta sbaglia: di «**x**» resterebbe «*x*».
        ''' </summary>
        <TestMethod>
        Public Sub DelGrassettoNonRestaMezzoCorsivo()

            Dim spianato As String = ProsaDellAssistente.SenzaMarkdown("Il **CV** è pronto.")

            Assert.AreEqual("Il CV è pronto.", spianato)
            Assert.DoesNotContain("*", spianato)

        End Sub

        <TestMethod>
        Public Sub ITitoliPerdonoICancellettiENonLaLoroRiga()

            Assert.AreEqual("Le tue mire" & vbLf & "Restare in zona.",
                            ProsaDellAssistente.SenzaMarkdown("## Le tue mire" & vbLf & "Restare in zona."))

        End Sub

        <TestMethod>
        Public Sub UnElencoDiventaUnElencoDaLeggere()

            Assert.AreEqual("• primo" & vbLf & "• secondo",
                            ProsaDellAssistente.SenzaMarkdown("- primo" & vbLf & "* secondo"))

        End Sub

        <TestMethod>
        Public Sub DiUnCollegamentoNonSiPerdeNienteDeiDue()

            Dim spianato As String = ProsaDellAssistente.SenzaMarkdown(
                "Guarda [gli annunci](https://esempio.it/lavoro).")

            Assert.Contains("gli annunci", spianato, "il testo")
            Assert.Contains("https://esempio.it/lavoro", spianato, "e l'indirizzo")
            Assert.DoesNotContain("[", spianato)

        End Sub

        ''' <summary>
        ''' Il falso positivo da evitare: in <c>giorni_follow_up</c> i trattini bassi non
        ''' sono corsivo, sono il nome — e nel brainstorming si parla anche di questo.
        ''' </summary>
        <TestMethod>
        Public Sub UnNomeColTrattinoBassoNonEUnCorsivo()

            Assert.AreEqual("Il campo giorni_follow_up vale 14.",
                            ProsaDellAssistente.SenzaMarkdown("Il campo giorni_follow_up vale 14."))

        End Sub

        <TestMethod>
        Public Sub LeRecinzioniDelCodiceSeNeVannoEIlCodiceResta()

            Dim spianato As String = ProsaDellAssistente.SenzaMarkdown(
                "Ecco:" & vbLf & "```json" & vbLf & "{""a"": 1}" & vbLf & "```")

            Assert.Contains("{""a"": 1}", spianato, "quel che c'era dentro resta")
            Assert.DoesNotContain("```", spianato)

        End Sub

        <TestMethod>
        Public Sub UnaRigaOrizzontaleNonDiventaUnaVoceDiElenco()

            Dim spianato As String = ProsaDellAssistente.SenzaMarkdown(
                "Prima" & vbLf & "---" & vbLf & "Dopo")

            Assert.DoesNotContain("•", spianato, "«---» non è un elenco")
            Assert.DoesNotContain("-", spianato)
            Assert.Contains("Prima", spianato)
            Assert.Contains("Dopo", spianato)

        End Sub

        ''' <summary>
        ''' Il testo arriva a pezzi: finché la chiusura non è arrivata, il segno resta —
        ''' e non è un difetto, è l'unica cosa onesta da fare (v. <c>CresciLaBolla</c>).
        ''' </summary>
        <TestMethod>
        Public Sub UnSegnoAncoraApertoRestaAVideo()

            Assert.AreEqual("Un profilo **cura",
                            ProsaDellAssistente.SenzaMarkdown("Un profilo **cura"))

        End Sub

        <TestMethod>
        Public Sub UnTestoSenzaSegniNonSiTocca()

            Const scritto As String = "Che lavoro ti piacerebbe fare, e dove?"

            Assert.AreEqual(scritto, ProsaDellAssistente.SenzaMarkdown(scritto))
            Assert.AreEqual("", ProsaDellAssistente.SenzaMarkdown(""))
            Assert.IsNull(ProsaDellAssistente.SenzaMarkdown(Nothing))

        End Sub

    End Class

End Namespace
