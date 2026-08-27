Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi del controllo della versione (2026-08-27, dalla revisione del giro D).
    ''' </summary>
    ''' <remarks>
    ''' Le promesse sono due. La prima è di <b>metodo</b>: il confronto è fra numeri, non
    ''' fra stringhe — «1.0.000» nella costante del programma e «v1.0» nel tag di un
    ''' rilascio sono la stessa versione. La seconda è di <b>disciplina</b>: la chiamata
    ''' parte solo quando la si chiede, e se non riesce non è un guasto ma un esito.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiControlloVersione

        Private NotInheritable Class GitHubFinto
            Inherits HttpMessageHandler

            Private ReadOnly _stato As HttpStatusCode?
            Private ReadOnly _corpo As String
            Private ReadOnly _guasto As Exception

            Public Sub New(stato As HttpStatusCode, Optional corpo As String = "{}")
                _stato = stato
                _corpo = corpo
            End Sub

            Public Sub New(guasto As Exception)
                _guasto = guasto
            End Sub

            Public Property SiEPresentato As String
            Public Property Chiamate As Integer

            Protected Overrides Function SendAsync(richiesta As HttpRequestMessage,
                                                   annulla As CancellationToken) As Task(Of HttpResponseMessage)

                Chiamate += 1

                Dim valori As IEnumerable(Of String) = Nothing
                If richiesta.Headers.TryGetValues("User-Agent", valori) Then
                    SiEPresentato = String.Join(" ", valori)
                End If

                If _guasto IsNot Nothing Then Return Task.FromException(Of HttpResponseMessage)(_guasto)

                Return Task.FromResult(New HttpResponseMessage(_stato.Value) With {
                    .Content = New StringContent(_corpo)})

            End Function

        End Class

        Private Shared Function Release(tag As String) As String
            Return $"{{""tag_name"":""{tag}"",""name"":""TrovaLavoro {tag}""}}"
        End Function

        ' ==================================================================
        ' Il confronto, che è la parte che può sbagliare in silenzio
        ' ==================================================================

        <TestMethod>
        Public Sub LaStessaVersioneScrittaInDueModiELaStessa()

            ' «1.0.000» è la costante di Versione.vb, «1.0» il tag del rilascio: a
            ' caratteri sarebbero diverse, e il programma direbbe di essere indietro
            ' rispetto a se stesso.
            Dim esito As EsitoVersione = ControlloVersione.Confronta("1.0.000", "1.0")

            Assert.AreEqual(StatoVersione.Aggiornata, esito.Stato)

        End Sub

        <TestMethod>
        Public Sub UnaVersionePiuRecenteSiRiconosce()

            Assert.AreEqual(StatoVersione.CeNEUnaNuova, ControlloVersione.Confronta("1.0.000", "1.1").Stato,
                            "la minore cresce")
            Assert.AreEqual(StatoVersione.CeNEUnaNuova, ControlloVersione.Confronta("1.0.000", "1.0.001").Stato,
                            "e anche la build")
            Assert.AreEqual(StatoVersione.CeNEUnaNuova, ControlloVersione.Confronta("1.9.000", "2.0").Stato,
                            "e nove non è più di dieci: si confrontano numeri")

        End Sub

        <TestMethod>
        Public Sub ChiCostruisceIlProgrammaNonESentitoDireCheEIndietro()

            ' Non è un caso di scuola: è la macchina di chi sviluppa, che ha in mano una
            ' versione non ancora rilasciata.
            Dim esito As EsitoVersione = ControlloVersione.Confronta("1.1.003", "1.0")

            Assert.AreEqual(StatoVersione.PiuAvantiDelPubblicato, esito.Stato)

        End Sub

        <TestMethod>
        Public Sub UnaVersioneCheNonSiCapisceNonDiventaUnVerdetto()

            For Each storta As String In {"", "  ", "la prima", "1.0-beta", Nothing}
                Assert.AreEqual(StatoVersione.NonSiSa, ControlloVersione.Confronta("1.0.000", storta).Stato,
                                $"«{storta}» non è un numero di versione")
            Next

        End Sub

        <TestMethod>
        Public Sub IlTagPerdeLaSuaVi()

            Assert.AreEqual("1.0", ControlloVersione.NumeroPubblicato(Release("v1.0")), "la v minuscola")
            Assert.AreEqual("2.3.4", ControlloVersione.NumeroPubblicato(Release("2.3.4")), "e senza v va bene uguale")

        End Sub

        <TestMethod>
        Public Sub UnaRispostaSenzaTagNonDiceNiente()

            Assert.IsNull(ControlloVersione.NumeroPubblicato("{}"), "nessun tag")
            Assert.IsNull(ControlloVersione.NumeroPubblicato("non è json"), "nemmeno json")
            Assert.IsNull(ControlloVersione.NumeroPubblicato(""), "né vuoto")

        End Sub

        ' ==================================================================
        ' La chiamata
        ' ==================================================================

        <TestMethod>
        Public Async Function ChiedeAGitHubEDiceCheCEUnaNuova() As Task

            Dim finto As New GitHubFinto(HttpStatusCode.OK, Release("v1.2"))

            Dim esito As EsitoVersione = Await ControlloVersione.ChiediAsync("1.0.000", finto)

            Assert.AreEqual(StatoVersione.CeNEUnaNuova, esito.Stato, "ce n'è una nuova")
            Assert.AreEqual("1.2", esito.Pubblicata, "e si dice quale")
            StringAssert.Contains(esito.Messaggio, "i tuoi dati restano dove sono",
                                  "e che sostituire l'exe non porta via niente")

        End Function

        <TestMethod>
        Public Async Function SiPresentaConIlProprioNome() As Task

            ' GitHub rifiuta le richieste che non dicono chi sono: senza User-Agent
            ' l'esito sarebbe un 403 buffo da capire.
            Dim finto As New GitHubFinto(HttpStatusCode.OK, Release("v1.0"))

            Await ControlloVersione.ChiediAsync("1.0.000", finto)

            StringAssert.Contains(finto.SiEPresentato, "TrovaLavoro", "dice chi è")

        End Function

        <TestMethod>
        Public Async Function SenzaReteNonEUnGuastoMaUnEsito() As Task

            For Each finto As GitHubFinto In {New GitHubFinto(New HttpRequestException("giù")),
                                              New GitHubFinto(HttpStatusCode.ServiceUnavailable),
                                              New GitHubFinto(HttpStatusCode.OK, "non è json")}

                Dim esito As EsitoVersione = Await ControlloVersione.ChiediAsync("1.0.000", finto)

                Assert.AreEqual(StatoVersione.NonSiSa, esito.Stato, "non si sa")
                Assert.IsFalse(String.IsNullOrWhiteSpace(esito.Messaggio), "ma si dice qualcosa")

            Next

        End Function

    End Class

End Namespace
