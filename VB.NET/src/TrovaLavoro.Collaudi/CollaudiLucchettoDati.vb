Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi del lucchetto di scrittura della cartella dati (cap. 09.4).
    ''' </summary>
    ''' <remarks>
    ''' La domanda vera è una sola — <b>il secondo che prova resta fuori?</b> — e tutto il
    ''' resto sono le sue conseguenze: che rilasciandolo si torni a poterlo prendere, e che
    ''' non resti mai un lucchetto che nessuno può togliere.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiLucchettoDati

        Private Shared Sub ConCartellaDati(prova As Action(Of CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "lucchetto-" & Guid.NewGuid().ToString("N"))
            Try
                prova(New CartellaDati(radice))
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

        <TestMethod>
        Public Sub IlSecondoCheProvaRestaFuori()
            ' Il cuore di tutto: due processi non scrivono insieme nella stessa cartella.
            ConCartellaDati(
                Sub(cartella)
                    Using primo As LucchettoDati = LucchettoDati.Prendi(cartella)

                        Assert.IsNotNull(primo, "il primo lo prende")
                        Assert.IsTrue(primo.Tenuto, "e lo tiene")

                        Assert.IsNull(LucchettoDati.Prendi(cartella),
                                      "il secondo deve restare fuori finché il primo non molla")

                    End Using
                End Sub)
        End Sub

        <TestMethod>
        Public Sub RilasciatoSiRiprende()
            ' L'applicazione che si chiude deve lasciare la cartella al server MCP, e
            ' viceversa: un lucchetto che non si restituisce sarebbe una porta murata.
            ConCartellaDati(
                Sub(cartella)
                    Dim primo As LucchettoDati = LucchettoDati.Prendi(cartella)
                    Assert.IsNotNull(primo, "preso")

                    primo.Dispose()
                    Assert.IsFalse(primo.Tenuto, "e restituito")

                    Using secondo As LucchettoDati = LucchettoDati.Prendi(cartella)
                        Assert.IsNotNull(secondo, "adesso tocca al secondo")
                    End Using
                End Sub)
        End Sub

        <TestMethod>
        Public Sub RilasciarloDueVolteNonFaNiente()
            ' Capita: la finestra si chiude e qualcuno smaltisce due volte. Non deve
            ' diventare un errore, e soprattutto non deve rilasciare il lucchetto di
            ' qualcun altro preso nel frattempo.
            ConCartellaDati(
                Sub(cartella)
                    Dim preso As LucchettoDati = LucchettoDati.Prendi(cartella)

                    preso.Dispose()
                    preso.Dispose()

                    Assert.IsFalse(preso.Tenuto, "resta rilasciato e non protesta")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub PrenderloCreaLaCartellaDati()
            ' Non è un effetto collaterale nascosto: per mettere un file in una cartella
            ' la cartella deve esserci, e chi prende il lucchetto sta per scrivere.
            ConCartellaDati(
                Sub(cartella)
                    Assert.IsFalse(Directory.Exists(cartella.Radice), "si parte da niente")

                    Using preso As LucchettoDati = LucchettoDati.Prendi(cartella)
                        Assert.IsNotNull(preso, "preso")
                        Assert.IsTrue(File.Exists(cartella.FileLucchetto), "il file c'è")
                    End Using
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlFileRestaVuotoEQuelloEIlPunto()
            ' Dentro non c'è niente — nessun numero di processo, nessuna ora — perché il
            ' lucchetto è la presa esclusiva, non il contenuto. È ciò che garantisce che un
            ' processo ucciso non lasci indietro un lucchetto da ripulire a mano.
            ConCartellaDati(
                Sub(cartella)
                    Using preso As LucchettoDati = LucchettoDati.Prendi(cartella)
                    End Using

                    Assert.AreEqual(0, New FileInfo(cartella.FileLucchetto).Length,
                                    "il file di lucchetto non racconta niente a nessuno")
                End Sub)
        End Sub

    End Class

End Namespace
