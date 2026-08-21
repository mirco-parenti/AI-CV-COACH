Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi delle due pulizie delle Impostazioni (cap. 11.5): svuotare i dati di
    ''' navigazione, e mandare via tutto.
    ''' </summary>
    ''' <remarks>
    ''' Stanno fuori dalla finestra apposta: una cancellazione va provata, e un banco non
    ''' sa premere un bottone. La proprietà che questa batteria custodisce è che «tutto»
    ''' voglia dire davvero tutto — <b>tranne il lucchetto</b>, che è nostro e che
    ''' l'applicazione viva non lascerebbe comunque cancellare (cap. 09.4).
    ''' </remarks>
    <TestClass>
    Public Class CollaudiPuliziaDati

        <TestMethod>
        Public Sub SvuotaSoloLaNavigazione()

            ConCartellaDiProva(
                Sub(pulizia, cartella)

                    Riempi(cartella)

                    Assert.IsTrue(pulizia.SvuotaNavigazione(), "c'era da svuotare")

                    Assert.IsFalse(Directory.Exists(cartella.CartellaWebView2), "la navigazione se n'è andata")
                    Assert.IsTrue(File.Exists(cartella.FileRegistro), "il registro no")
                    Assert.IsTrue(File.Exists(cartella.FileProfilo), "e nemmeno il profilo")
                    Assert.IsTrue(Directory.Exists(cartella.CartellaOpportunita), "né le candidature")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SvuotareDueVolteNonEUnErrore()

            ConCartellaDiProva(
                Sub(pulizia, cartella)

                    Riempi(cartella)

                    Assert.IsTrue(pulizia.SvuotaNavigazione())
                    Assert.IsFalse(pulizia.SvuotaNavigazione(), "la seconda volta non c'era più niente")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub EliminaTuttoTranneIlLucchetto()

            ConCartellaDiProva(
                Sub(pulizia, cartella)

                    Riempi(cartella)
                    Dim lucchetto As String = Path.Combine(cartella.Radice, "dati.lock")
                    File.WriteAllText(lucchetto, "")

                    Dim andate As Integer = pulizia.EliminaTutto()

                    Assert.IsGreaterThan(0, andate, "qualcosa è stato eliminato")

                    ' Il lucchetto è l'unica cosa che resta: non è un dato dell'utente, e
                    ' con l'applicazione viva non si lascerebbe cancellare comunque.
                    Assert.IsTrue(File.Exists(lucchetto), "il lucchetto resta")
                    Assert.HasCount(1, Directory.GetFileSystemEntries(cartella.Radice),
                                    "e non resta nient'altro")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub EliminaTuttoNonSiSpaventaSeIlLucchettoNonCE()

            ' Il lucchetto può non esserci: il server MCP lo prende solo per la durata di
            ' una scrittura, e una cartella appena nata non ne ha ancora uno.
            ConCartellaDiProva(
                Sub(pulizia, cartella)

                    Riempi(cartella)

                    pulizia.EliminaTutto()

                    Assert.IsEmpty(Directory.GetFileSystemEntries(cartella.Radice),
                                   "senza lucchetto non resta proprio niente")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SuUnaCartellaCheNonCESiComportaBene()

            Dim radice As String = Path.Combine(Path.GetTempPath(), "mai-nata-" & Guid.NewGuid().ToString("N"))
            Dim pulizia As New PuliziaDati(New CartellaDati(radice))

            Assert.IsFalse(pulizia.SvuotaNavigazione())
            Assert.AreEqual(0, pulizia.EliminaTutto())
            Assert.IsFalse(pulizia.CEQualcosa)

        End Sub

        <TestMethod>
        Public Sub IlSoloLucchettoNonEQualcosaDaEliminare()

            ConCartellaDiProva(
                Sub(pulizia, cartella)

                    cartella.Assicura()

                    ' Assicura() lascia dietro di sé profilo\, storico\, out\ e
                    ' opportunita\ vuote: sono impalcatura, non dati.
                    Assert.IsFalse(pulizia.CEQualcosa,
                                   "le cartelle di servizio vuote non sono roba da eliminare")

                    File.WriteAllText(Path.Combine(cartella.Radice, "dati.lock"), "")

                    ' È quel che accende o spegne il bottone rosso: uno che non ha niente
                    ' da fare insegna solo a non fidarsi del colore.
                    Assert.IsFalse(pulizia.CEQualcosa, "il lucchetto da solo non conta")

                    File.WriteAllText(cartella.FileRegistro, "{}")

                    Assert.IsTrue(pulizia.CEQualcosa, "un dato vero sì")

                End Sub)

        End Sub

        ''' <summary>Una cartella dati temporanea con dentro un po' di tutto.</summary>
        Private Shared Sub Riempi(cartella As CartellaDati)

            cartella.Assicura()

            Directory.CreateDirectory(cartella.CartellaWebView2)
            File.WriteAllText(Path.Combine(cartella.CartellaWebView2, "cookies.dat"), "briciole")

            Directory.CreateDirectory(cartella.CartellaProfilo)
            File.WriteAllText(cartella.FileProfilo, "{}")

            Directory.CreateDirectory(cartella.CartellaOpportunita)
            File.WriteAllText(cartella.FileRegistro, "{""voci"": []}")
            File.WriteAllText(cartella.FileSegreti, "cifrato")
            File.WriteAllText(cartella.FileImpostazioni, "{}")

        End Sub

        Private Shared Sub ConCartellaDiProva(prova As Action(Of PuliziaDati, CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "pulizia-" & Guid.NewGuid().ToString("N"))

            Dim cartella As New CartellaDati(radice)

            Try
                prova(New PuliziaDati(cartella), cartella)
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

    End Class

End Namespace
