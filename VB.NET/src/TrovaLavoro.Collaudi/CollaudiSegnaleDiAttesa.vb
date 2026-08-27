Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi del segno visibile mentre l'AI lavora (reperto D-R2 del giro D). Il tempo
    ''' si passa da fuori apposta: un collaudo che aspetta davvero dieci secondi per vedere
    ''' comparire il conto è un collaudo che nessuno rilancia volentieri.
    ''' </summary>
    <TestClass>
    Public Class CollaudiSegnaleDiAttesa

        Private Shared ReadOnly Adesso As New Date(2026, 8, 27, 15, 0, 0)

        <TestMethod>
        Public Sub LaRigaSiMuoveAOgniBattito()

            ' È il muoversi, non il testo, a dire che il programma è vivo: due battiti di
            ' fila che scrivono la stessa cosa non segnalano niente.
            Dim segnale As New SegnaleDiAttesa("Pronto")

            Dim viste As New List(Of String) From {segnale.Avvia(Adesso)}
            For passo As Integer = 1 To 3
                viste.Add(segnale.Battito(Adesso.AddSeconds(passo)))
            Next

            For i As Integer = 1 To viste.Count - 1
                Assert.AreNotEqual(viste(i - 1), viste(i), $"il battito {i} deve cambiare qualcosa")
            Next
            For Each riga As String In viste
                StringAssert.Contains(riga, "L'AI sta lavorando", "e dire sempre di che si tratta")
            Next

        End Sub

        <TestMethod>
        Public Sub DopoUnPoLaRigaDiceAncheDaQuantoSiAspetta()

            Dim segnale As New SegnaleDiAttesa("Pronto")
            segnale.Avvia(Adesso)

            Assert.DoesNotContain("(", segnale.Battito(Adesso.AddSeconds(3)),
                                  "un'attesa breve si vede passare: il conto sarebbe rumore")
            StringAssert.Contains(segnale.Battito(Adesso.AddSeconds(24)), "(24 s)",
                                  "un'attesa lunga invece va misurata, o sembra piantata")

        End Sub

        <TestMethod>
        Public Sub FinitaLAttesaLaBarraTornaADireQuelloCheDiceva()

            ' Una barra che resta con «sta lavorando…» addosso a lavoro finito mente
            ' peggio di una barra muta.
            Dim segnale As New SegnaleDiAttesa("Pool 1.13: due file fuori impronta")

            segnale.Avvia(Adesso)
            Assert.IsTrue(segnale.InCorso)

            Assert.AreEqual("Pool 1.13: due file fuori impronta", segnale.Ferma(),
                            "torna esattamente quel che c'era prima, avviso compreso")
            Assert.IsFalse(segnale.InCorso)

        End Sub

        <TestMethod>
        Public Sub UnBattitoFuoriDallAttesaNonInventaNiente()

            Dim segnale As New SegnaleDiAttesa("Pronto")

            Assert.AreEqual("Pronto", segnale.Battito(Adesso), "senza attesa in corso, il riposo")

        End Sub

    End Class

End Namespace
