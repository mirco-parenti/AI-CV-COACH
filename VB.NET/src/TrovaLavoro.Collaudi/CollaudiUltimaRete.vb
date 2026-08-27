Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

''' <summary>
''' Collaudi dell'<b>ultima rete</b>: i testi che il programma mostra quando è successo
''' qualcosa che nessuno aveva previsto (<see cref="UltimaRete"/>). Si collauda il testo e
''' non chi lo mostra, perché una finestra di messaggio in un collaudo resta lì ad
''' aspettare un clic che non arriva mai. <i>(2026-08-27, dalla revisione del giro D.)</i>
''' </summary>
<TestClass>
Public Class CollaudiUltimaRete

    <TestMethod>
    Public Sub LErroreSiRaccontaInItalianoESenzaStackTrace()

        Dim testo As String = UltimaRete.MessaggioImprevisto(
            New InvalidOperationException("Il lucchetto dei dati è di qualcun altro."))

        StringAssert.Contains(testo, "Il lucchetto dei dati è di qualcun altro.", "che cosa è successo")
        StringAssert.Contains(testo, "non è stato toccato", "che il profilo su disco è al sicuro")
        Assert.DoesNotContain("System.", testo, "niente nomi di classi .NET davanti all'utente")
        Assert.DoesNotContain("   at ", testo, "e niente stack trace")

    End Sub

    <TestMethod>
    Public Sub UnErroreSenzaMessaggioNonLasciaUnaFraseMonca()

        For Each muto As Exception In New Exception() {New Exception(""), New Exception("   "), Nothing}

            Dim testo As String = UltimaRete.MessaggioImprevisto(muto)

            StringAssert.Contains(testo, "non aveva previsto", "la frase resta intera")
            Assert.DoesNotContain(vbLf & vbLf & vbLf, testo, "e non resta un buco al posto della causa")

        Next

    End Sub

    <TestMethod>
    Public Sub NelDiarioDelServerCiVaAncheIlTipoDellEccezione()

        ' Qui non c'è nessuno da consolare: chi legge un diario vuole sapere *che cosa*
        ' è scoppiato, non che il suo profilo è al sicuro.
        Assert.AreEqual("TimeoutException: la pagina non si è caricata",
                        UltimaRete.MessaggioPerIlDiario(New TimeoutException("la pagina non si è caricata")))

        StringAssert.Contains(UltimaRete.MessaggioPerIlDiario(New TimeoutException("")), "TimeoutException",
                              "anche senza descrizione, il tipo si dice")
        Assert.IsFalse(String.IsNullOrWhiteSpace(UltimaRete.MessaggioPerIlDiario(Nothing)),
                       "e anche il niente si racconta")

    End Sub

End Class
