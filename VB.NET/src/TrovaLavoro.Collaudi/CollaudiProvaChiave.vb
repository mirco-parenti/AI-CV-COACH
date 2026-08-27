Imports System.Net
Imports System.Net.Http
Imports System.Threading
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi della prova della chiave API. Fino alla 1.0 la finestra dichiarava di
    ''' <b>non</b> provarla, con due ragioni: costerebbe una chiamata, e non
    ''' distinguerebbe una chiave sbagliata da una rete assente. La prima è caduta
    ''' (l'elenco dei modelli non consuma token), la seconda non era vera — ed è quel che
    ''' questi collaudi guardano. <i>(2026-08-27, revisione del giro D.)</i>
    ''' </summary>
    <TestClass>
    Public Class CollaudiProvaChiave

        ''' <summary>Una rete finta che risponde sempre lo stesso, o si rompe sempre allo stesso modo.</summary>
        Private NotInheritable Class ReteFinta
            Inherits HttpMessageHandler

            Private ReadOnly _stato As HttpStatusCode?
            Private ReadOnly _guasto As Exception

            Public Sub New(stato As HttpStatusCode)
                _stato = stato
            End Sub

            Public Sub New(guasto As Exception)
                _guasto = guasto
            End Sub

            Public Property UltimaChiave As String
            Public Property UltimoIndirizzo As String

            Protected Overrides Function SendAsync(richiesta As HttpRequestMessage,
                                                   annulla As CancellationToken) As Task(Of HttpResponseMessage)
                UltimoIndirizzo = richiesta.RequestUri.ToString()
                Dim valori As IEnumerable(Of String) = Nothing
                If richiesta.Headers.TryGetValues("x-api-key", valori) Then UltimaChiave = valori.First()

                If _guasto IsNot Nothing Then Return Task.FromException(Of HttpResponseMessage)(_guasto)
                Return Task.FromResult(New HttpResponseMessage(_stato.Value))
            End Function

        End Class

        <TestMethod>
        Public Async Function UnaChiaveBuonaSiRiconosce() As Task

            Dim rete As New ReteFinta(HttpStatusCode.OK)

            Dim esito As EsitoProva = Await ProvaChiave.ProvaAsync("sk-ant-finta-1234", rete)

            Assert.IsTrue(esito.Riuscita)
            Assert.AreEqual("La chiave funziona.", esito.Messaggio)
            Assert.AreEqual("sk-ant-finta-1234", rete.UltimaChiave, "la chiave viaggia nell'intestazione giusta")
            StringAssert.Contains(rete.UltimoIndirizzo, "/models",
                                  "si chiede l'elenco dei modelli, che non consuma token")

        End Function

        <TestMethod>
        Public Async Function UnaChiaveRifiutataNonSiConfondeConLaReteAssente() As Task

            ' È la ragione per cui questa prova esiste: sono due guai diversi, e portano a
            ' due gesti diversi. Dirli allo stesso modo manderebbe l'utente a cercare una
            ' chiave nuova che non serve.
            Dim rifiutata As EsitoProva = Await ProvaChiave.ProvaAsync("sk-ant-finta", New ReteFinta(HttpStatusCode.Unauthorized))
            Dim senzaRete As EsitoProva = Await ProvaChiave.ProvaAsync("sk-ant-finta", New ReteFinta(New HttpRequestException("giù")))

            Assert.AreEqual(CausaErroreAi.Chiave, rifiutata.Causa)
            Assert.AreEqual(CausaErroreAi.Rete, senzaRete.Causa)
            Assert.AreNotEqual(rifiutata.Messaggio, senzaRete.Messaggio, "e si dicono in modo diverso")
            StringAssert.Contains(senzaRete.Messaggio, "non è stata provata",
                                  "senza rete la chiave non è bocciata: non è stata provata")

        End Function

        <TestMethod>
        Public Sub OgniStatoDellApiDiceLaSuaCosa()

            Assert.IsTrue(ProvaChiave.DallaRisposta(HttpStatusCode.OK).Riuscita)
            Assert.AreEqual(CausaErroreAi.Chiave, ProvaChiave.DallaRisposta(HttpStatusCode.Forbidden).Causa)
            Assert.AreEqual(CausaErroreAi.Limite, ProvaChiave.DallaRisposta(HttpStatusCode.TooManyRequests).Causa)
            Assert.AreEqual(CausaErroreAi.Servizio, ProvaChiave.DallaRisposta(HttpStatusCode.BadGateway).Causa)
            Assert.AreEqual(CausaErroreAi.Richiesta, ProvaChiave.DallaRisposta(HttpStatusCode.BadRequest).Causa)

        End Sub

        <TestMethod>
        Public Async Function UnaChiaveVuotaNonFaNemmenoLaChiamata() As Task

            Dim rete As New ReteFinta(HttpStatusCode.OK)

            Dim esito As EsitoProva = Await ProvaChiave.ProvaAsync("   ", rete)

            Assert.IsFalse(esito.Riuscita)
            Assert.IsNull(rete.UltimaChiave, "non si spreca una chiamata per una casella vuota")

        End Function

    End Class

End Namespace
