Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Microsoft.Web.WebView2.Core
Imports TrovaLavoro.Web

Namespace Web

    ''' <summary>
    ''' Collaudi della frase che la fascia di P3 mostra quando una pagina non si apre
    ''' (T9d): che dica la cosa giusta, e che non ne dica nessuna quando non serve.
    ''' </summary>
    ''' <remarks>
    ''' Girano senza browser: <see cref="EsitoNavigazione"/> prende tre valori semplici
    ''' apposta, perché l'oggetto dell'evento di WebView2 nel banco non si costruisce.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiEsitoNavigazione

        ''' <summary>
        ''' Il difetto da cui nasce il modulo: un indirizzo che il server non ha veniva
        ''' raccontato come un problema di collegamento.
        ''' </summary>
        <TestMethod>
        Public Sub UnQuattrocentoQuattroNonParlaDiInternet()

            ' Il server ha risposto, quindi per il browser la navigazione è riuscita.
            Dim detto As String = EsitoNavigazione.PercheNonSiEAperta(
                riuscita:=True, errore:=CoreWebView2WebErrorStatus.Unknown, statoHttp:=404)

            Assert.IsNotNull(detto, "una pagina che non c'è si dice")
            StringAssert.Contains(detto, "404", "col numero che il server ha risposto")
            StringAssert.Contains(detto, "indirizzo", "e mandando a guardare l'indirizzo")
            Assert.DoesNotContain("Internet", detto,
                                  "il collegamento c'era e il server ha risposto: non è lui da controllare")

        End Sub

        <TestMethod>
        Public Sub UnIndirizzoCheNonEsisteMandaAGuardareLIndirizzo()

            Dim detto As String = EsitoNavigazione.PercheNonSiEAperta(
                riuscita:=False, errore:=CoreWebView2WebErrorStatus.HostNameNotResolved, statoHttp:=0)

            StringAssert.Contains(detto, "indirizzo")
            Assert.DoesNotContain("Internet", detto)

        End Sub

        <TestMethod>
        Public Sub UnCollegamentoCadutoRestaUnProblemaDiCollegamento()

            For Each caduta As CoreWebView2WebErrorStatus In {
                CoreWebView2WebErrorStatus.CannotConnect,
                CoreWebView2WebErrorStatus.ConnectionAborted,
                CoreWebView2WebErrorStatus.ConnectionReset,
                CoreWebView2WebErrorStatus.Disconnected,
                CoreWebView2WebErrorStatus.ServerUnreachable,
                CoreWebView2WebErrorStatus.Timeout}

                Dim detto As String = EsitoNavigazione.PercheNonSiEAperta(
                    riuscita:=False, errore:=caduta, statoHttp:=0)

                StringAssert.Contains(detto, "Internet", $"({caduta}) qui il modem c'entra davvero")

            Next

        End Sub

        <TestMethod>
        Public Sub UnaPaginaCheSiApreNonDiceNiente()

            Assert.IsNull(EsitoNavigazione.PercheNonSiEAperta(
                riuscita:=True, errore:=CoreWebView2WebErrorStatus.Unknown, statoHttp:=200))

            ' La pagina di casa è scritta da noi e non ha nessuno stato HTTP da mostrare.
            Assert.IsNull(EsitoNavigazione.PercheNonSiEAperta(
                riuscita:=True, errore:=CoreWebView2WebErrorStatus.Unknown, statoHttp:=0))

        End Sub

        <TestMethod>
        Public Sub FermareUnaPaginaNonEUnGuaio()

            Assert.IsNull(EsitoNavigazione.PercheNonSiEAperta(
                riuscita:=False, errore:=CoreWebView2WebErrorStatus.OperationCanceled, statoHttp:=0),
                "chi preme «✕» sa già cos'è successo")

        End Sub

        <TestMethod>
        Public Sub IlCasoIgnotoPortaIlMotivoTecnico()

            Dim detto As String = EsitoNavigazione.PercheNonSiEAperta(
                riuscita:=False, errore:=CoreWebView2WebErrorStatus.CertificateExpired, statoHttp:=0)

            StringAssert.Contains(detto, "CertificateExpired",
                                  "chi lo sa leggere trova il motivo, fra parentesi")

        End Sub

    End Class

End Namespace
