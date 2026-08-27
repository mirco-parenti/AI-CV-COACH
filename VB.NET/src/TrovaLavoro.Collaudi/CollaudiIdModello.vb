Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi del riconoscimento fra l'alias di un modello e la sua versione datata
    ''' (2026-08-27, guardando l'applicazione a occhio). La promessa da difendere è
    ''' stretta: si toglie <b>la data in coda</b> e nient'altro — due modelli diversi
    ''' restano diversi, altrimenti la cura costerebbe più del male.
    ''' </summary>
    <TestClass>
    Public Class CollaudiIdModello

        <TestMethod>
        Public Sub LaDataInCodaSiToglie()

            Assert.AreEqual("claude-haiku-4-5", IdModello.SenzaLaData("claude-haiku-4-5-20251001"))
            Assert.AreEqual("claude-sonnet-4-5", IdModello.SenzaLaData("claude-sonnet-4-5-20250929"))

        End Sub

        <TestMethod>
        Public Sub QuelCheNonEUnaDataResta()

            ' Otto cifre, e in fondo: tutto il resto è parte del nome del modello.
            Assert.AreEqual("claude-haiku-4-5", IdModello.SenzaLaData("claude-haiku-4-5"))
            Assert.AreEqual("claude-haiku-4-5-2025100", IdModello.SenzaLaData("claude-haiku-4-5-2025100"))
            Assert.AreEqual("claude-20251001-haiku", IdModello.SenzaLaData("claude-20251001-haiku"))
            Assert.AreEqual("", IdModello.SenzaLaData(Nothing))

        End Sub

        <TestMethod>
        Public Sub LAliasELaVersioneDatataSonoLoStessoModello()

            Assert.IsTrue(IdModello.StessoModello("claude-haiku-4-5", "claude-haiku-4-5-20251001"))
            Assert.IsTrue(IdModello.StessoModello("claude-haiku-4-5-20251001", "claude-haiku-4-5"),
                          "e vale nei due versi")
            Assert.IsTrue(IdModello.StessoModello("CLAUDE-HAIKU-4-5", "claude-haiku-4-5-20251001"),
                          "le maiuscole non fanno un altro modello")

        End Sub

        <TestMethod>
        Public Sub DueModelliDiversiRestanoDiversi()

            Assert.IsFalse(IdModello.StessoModello("claude-opus-4-5", "claude-opus-4-6"),
                           "una versione in più è un altro modello")
            Assert.IsFalse(IdModello.StessoModello("claude-haiku-4-5", "claude-sonnet-5"))
            Assert.IsFalse(IdModello.StessoModello("claude-opus-4-5-20251101", "claude-opus-4-6-20251101"),
                           "nemmeno con la stessa data in coda")

        End Sub

        <TestMethod>
        Public Sub DueVuotiNonSonoLoStessoModello()

            ' Sono due cose che non si sanno, e da due cose che non si sanno non esce
            ' un'uguaglianza: altrimenti un identificativo mancante prenderebbe il prezzo
            ' del primo modello di passaggio.
            Assert.IsFalse(IdModello.StessoModello("", ""))
            Assert.IsFalse(IdModello.StessoModello(Nothing, "claude-haiku-4-5"))
            Assert.IsFalse(IdModello.StessoModello("   ", "claude-haiku-4-5"))

        End Sub

    End Class

End Namespace
