Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi di «Informazioni su…» (cap. 03.4) e della riga di versione che mostra
    ''' (cap. 03.5). Quel che qui può rompersi davvero è la riga: dice all'utente con
    ''' quale libreria di prompt sta lavorando, ed è la prima cosa che si guarda quando
    ''' un esito non torna.
    ''' </summary>
    ''' <remarks>
    ''' La finestra si costruisce e si interroga <b>senza mostrarla</b>, come le altre
    ''' modali del progetto.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraInformazioni

        <TestMethod>
        Public Sub LaRigaDiVersioneHaUnaFonteSola()

            Assert.AreEqual($"Ver. {Versione.Numero} · Pool 1.12 (integrato)",
                            Versione.Riga("Pool 1.12 (integrato)"),
                            "la riga la compone Versione, non chi la mostra")

        End Sub

        <TestMethod>
        Public Sub LaFinestraMostraVersioneEPool()

            Using finestra As New FinestraInformazioni("Pool 1.12 (integrato)")

                Assert.AreEqual(Versione.Riga("Pool 1.12 (integrato)"), finestra.RigaDiVersione,
                                "la stessa riga del pannello del logo")
                StringAssert.Contains(finestra.RigaDiCopyright, "Aviolab AI",
                                      "e il marchio di chi l'ha fatto")

            End Using

        End Sub

        <TestMethod>
        Public Sub SenzaLibreriaLoDiceInveceDiTacere()

            ' «Pool —» è l'anomalia totale del cap. 03.5: la libreria non si è aperta
            ' affatto. Meglio dirlo che mostrare una riga monca.
            For Each niente As String In New String() {Nothing, "", "   "}

                Using finestra As New FinestraInformazioni(niente)
                    StringAssert.Contains(finestra.RigaDiVersione, "Pool —",
                                          "senza libreria la riga lo dichiara")
                End Using

            Next

        End Sub

        <TestMethod>
        Public Sub LaFinestraPortaIlMarchioInAlto()

            Using finestra As New FinestraInformazioni("Pool 1.12")

                Assert.IsNotNull(finestra.ImmagineDelMarchio, "l'immagine del marchio c'è, e si vede")
                Assert.AreSame(Marchio.SchermataDiAvvio, finestra.ImmagineDelMarchio,
                               "ed è la stessa dell'avvio, non una seconda copia")

            End Using

        End Sub

    End Class

End Namespace
