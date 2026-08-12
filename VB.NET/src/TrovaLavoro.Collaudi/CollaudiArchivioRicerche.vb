Imports System.IO
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi dell'archivio delle ricerche (cap. 06.3, cap. 11.1): il file
    ''' <c>ricerche.json</c> che nasce al primo salvataggio, si rilegge uguale e, quando
    ''' non c'è, non impedisce di cercare.
    ''' </summary>
    <TestClass>
    Public Class CollaudiArchivioRicerche

        <TestMethod>
        Public Sub SenzaFileSiParteDaiPortaliPredefiniti()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)

                    Assert.IsFalse(archivio.Esiste, "al primo avvio il file non c'è")

                    Dim ricerche As Ricerche = archivio.Carica()

                    Assert.AreEqual(OrigineRicerche.Predefinita, ricerche.Origine)
                    Assert.IsNotEmpty(ricerche.Portali, "senza portali non si cercherebbe niente")

                    ' Il ripiego è normale, ma detto: è la stessa regola di taratura e
                    ' modelli (cap. 11.6).
                    Assert.IsNotNull(ricerche.Avviso)
                    Assert.Contains(cartella.FileRicerche, ricerche.Avviso,
                                    "l'avviso dice anche dove il file sarebbe andato cercato")

                End Sub)

        End Sub

        <TestMethod>
        Public Sub SalvaERilegge()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)

                    Dim ricerche As Ricerche = Ricerche.Predefinita()
                    ricerche.MettiDaParte(New RicercaSalvata With {
                        .Nome = "Perito a Genova", .Portale = "Indeed",
                        .Cosa = "perito elettronico", .Dove = "Genova"})

                    archivio.Salva(ricerche)

                    Assert.IsTrue(archivio.Esiste, "il file deve esserci")

                    Dim rilette As Ricerche = archivio.Carica()

                    Assert.AreEqual(OrigineRicerche.File, rilette.Origine)
                    Assert.IsNull(rilette.Avviso, "ciò che abbiamo scritto noi si rilegge senza rimostranze")
                    Assert.HasCount(1, rilette.Salvate)
                    Assert.AreEqual("perito elettronico", rilette.Salvate(0).Cosa)

                    ' I portali finiscono nel file anche se nessuno li ha toccati: è ciò
                    ' che li rende modificabili senza una nuova build (cap. 06.3).
                    CollectionAssert.AreEqual(
                        ricerche.Portali.Select(Function(p) p.Nome).ToArray(),
                        rilette.Portali.Select(Function(p) p.Nome).ToArray())

                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlFileELeggibileAMano()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)

                    Dim ricerche As Ricerche = Ricerche.Predefinita()
                    ricerche.MettiDaParte(New RicercaSalvata With {
                        .Nome = "Perito a Forlì", .Portale = "Indeed", .Dove = "Forlì"})

                    archivio.Salva(ricerche)

                    Dim testo As String = File.ReadAllText(cartella.FileRicerche, Text.Encoding.UTF8)

                    Assert.StartsWith("{", testo, "niente BOM davanti: dà noia agli altri strumenti")
                    Assert.Contains(vbLf, testo, "scritto con i rientri, per essere corretto a mano")
                    Assert.Contains("Forlì", testo, "gli accenti restano in chiaro, non in ì")

                    ' Della scrittura atomica non deve restare traccia.
                    Assert.IsFalse(File.Exists(cartella.FileRicerche & ".tmp"))

                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnFileRottoNonImpedisceDiCercare()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)

                    cartella.Assicura()
                    File.WriteAllText(cartella.FileRicerche, "{ questo non è JSON")

                    Dim ricerche As Ricerche = archivio.Carica()

                    Assert.AreEqual(OrigineRicerche.Predefinita, ricerche.Origine,
                                    "si ripiega sui portali predefiniti")
                    Assert.IsNotNull(ricerche.Avviso, "e lo si dice")

                    ' Il file rotto resta dov'è: a differenza del profilo non c'è niente
                    ' di irripetibile da mettere in salvo, ma nemmeno si cancella la roba
                    ' di qualcun altro.
                    Assert.IsTrue(File.Exists(cartella.FileRicerche))

                End Sub)

        End Sub

        ''' <summary>Un archivio su una cartella temporanea, che si porta via tutto alla fine.</summary>
        Private Shared Sub ConArchivioTemporaneo(prova As Action(Of ArchivioRicerche, CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "ricerche-" & Guid.NewGuid().ToString("N"))

            Dim cartella As New CartellaDati(radice)

            Try
                prova(New ArchivioRicerche(cartella), cartella)
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

    End Class

End Namespace
