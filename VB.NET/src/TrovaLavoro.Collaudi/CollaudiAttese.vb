Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi del tetto delle attese (<see cref="Attese"/>). Il meccanismo esiste per un
    ''' caso che nessuno ha mai visto — un evento del browser che non arriva mai — e proprio
    ''' per questo va provato con un compito che <b>davvero</b> non finisce, non con uno
    ''' lento. <i>(2026-08-27, dalla revisione del giro D.)</i>
    ''' </summary>
    <TestClass>
    Public Class CollaudiAttese

        ''' <summary>Un compito che non finisce mai, e non finirà.</summary>
        Private Shared Function MaiPiu() As Task(Of Boolean)
            Return New TaskCompletionSource(Of Boolean)().Task
        End Function

        ' Il limite di tempo qui non protegge il codice: protegge il collaudo. Rompendo
        ' il tetto, questo collaudo aspetterebbe per sempre — e un collaudo appeso non è
        ' rosso, è niente. (Regola 14: l'ho provato a rompere, e serviva.)
        <TestMethod>
        <Timeout(5000)>
        Public Async Function UnAttesaCheNonFinisceScadeInveceDiRestareAppesa() As Task

            Dim scaduta As TimeoutException = Await Assert.ThrowsExactlyAsync(Of TimeoutException)(
                Function() Attese.EntroIlTetto(MaiPiu(), TimeSpan.FromMilliseconds(80), "La pagina da stampare"))

            StringAssert.Contains(scaduta.Message, "La pagina da stampare",
                                  "il messaggio dice che cosa stava aspettando")

        End Function

        <TestMethod>
        Public Async Function UnCompitoCheFiniscePassaComeSeIlTettoNonCiFosse() As Task

            Dim valore As Boolean = Await Attese.EntroIlTetto(
                Task.FromResult(True), TimeSpan.FromSeconds(30), "Qualcosa")

            Assert.IsTrue(valore, "il tetto non tocca il caso normale")

        End Function

        <TestMethod>
        Public Async Function UnCompitoFallitoPortaIlSuoErrore() As Task

            ' Il tetto non deve nascondere il guasto vero sotto un guasto di tempo.
            Dim rotto As Task(Of Boolean) = Task.FromException(Of Boolean)(New InvalidOperationException("rotto"))

            Await Assert.ThrowsExactlyAsync(Of InvalidOperationException)(
                Function() Attese.EntroIlTetto(rotto, TimeSpan.FromSeconds(30), "Qualcosa"))

        End Function

        <TestMethod>
        Public Sub IlMessaggioDiceQuantoHaAspettato()

            Assert.AreEqual("La stampa del PDF non è finita entro 60 secondi.",
                            Attese.Scaduta("La stampa del PDF", TimeSpan.FromSeconds(60)))
            Assert.AreEqual("L'attesa non è finita entro 5 secondi.",
                            Attese.Scaduta(Nothing, TimeSpan.FromSeconds(5)),
                            "senza un nome, si dice comunque quanto")

        End Sub

        <TestMethod>
        Public Async Function UnTettoCheNonEUnTempoNonSiAccetta() As Task

            Await Assert.ThrowsExactlyAsync(Of ArgumentOutOfRangeException)(
                Function() Attese.EntroIlTetto(MaiPiu(), TimeSpan.Zero, "Qualcosa"))

        End Function

    End Class

End Namespace
