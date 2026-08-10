Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi del riassunto dell'annuncio (cap. 03.6): quello che P4 mostra nella
    ''' colonna di sinistra, accanto ai giudizi che ne discendono.
    ''' </summary>
    <TestClass>
    Public Class CollaudiVistaAnnuncio

        <TestMethod>
        Public Sub LAnnuncioSiLeggeSezionePerSezione()

            Dim scritto As String = VistaAnnuncio.Riassunto(CasiDiCollaudo.Annuncio(CasiDiCollaudo.Compatibile))

            Assert.Contains("Titolo:", scritto, "chi cerca cosa")
            Assert.Contains("Competenze richieste", scritto, "i requisiti, per lista")
            Assert.Contains("• ", scritto, "una voce per riga")

        End Sub

        <TestMethod>
        Public Sub IlContrattoSiScriveInUnaRigaSola()

            Dim annuncio As String =
                "{""titolo"": ""Autista"", ""azienda"": ""Rossi S.p.A."", ""sede"": [""Forlì"", ""Cesena""]," &
                """contratto"": {""tipo"": ""tempo determinato"", ""durata"": ""6 mesi""," &
                """orario"": ""full time"", ""retribuzione"": """"}}"

            Dim scritto As String = VistaAnnuncio.Riassunto(JsonNode.Parse(annuncio))

            Assert.Contains("Sede: Forlì · Cesena", scritto, "le sedi in fila")
            Assert.Contains("Contratto: tempo determinato · 6 mesi · full time", scritto,
                            "e le condizioni, senza la voce che l'annuncio tace")

        End Sub

        <TestMethod>
        Public Sub OgniRequisitoPortaLaSuaPrioritaEISuoiAnni()

            Dim annuncio As String =
                "{""competenze_richieste"": [{""testo"": ""Uso del muletto"", ""priorita"": ""richiesto""}," &
                "{""testo"": ""Inglese"", ""priorita"": ""non specificata""}]," &
                """esperienza_richiesta"": [{""testo"": ""Nel settore logistico"", ""priorita"": ""preferenziale""," &
                """anni"": ""3 anni""}]}"

            Dim scritto As String = VistaAnnuncio.Riassunto(JsonNode.Parse(annuncio))

            Assert.Contains("• Uso del muletto — richiesto", scritto, "la priorità dichiarata")
            Assert.Contains("• Inglese" & vbCrLf, scritto, "«non specificata» non si scrive: non dice niente")
            Assert.Contains("• Nel settore logistico (3 anni) — preferenziale", scritto, "e gli anni chiesti")

        End Sub

        <TestMethod>
        Public Sub LeSezioniVuoteNonSiScrivono()

            ' Un titoletto seguito dal nulla si legge come un'informazione persa.
            Dim scritto As String = VistaAnnuncio.Riassunto(JsonNode.Parse(
                "{""titolo"": ""Autista"", ""competenze_richieste"": [], ""benefit"": []}"))

            Assert.Contains("Titolo: Autista", scritto, "quel che c'è si mostra")
            Assert.DoesNotContain("Competenze richieste", scritto, "e quel che non c'è non si annuncia")
            Assert.DoesNotContain("Benefit", scritto, "nemmeno il contesto vuoto")

        End Sub

        <TestMethod>
        Public Sub UnAnnuncioCheNonHaLaFormaAttesaNonRompeNiente()

            Assert.IsEmpty(VistaAnnuncio.Riassunto(Nothing), "niente annuncio, niente riassunto")
            Assert.IsEmpty(VistaAnnuncio.Riassunto(JsonNode.Parse("[]")), "una lista non è un annuncio")
            Assert.IsEmpty(VistaAnnuncio.Riassunto(JsonNode.Parse("{}")), "e un oggetto vuoto non dice niente")

            ' Liste di sole stringhe al posto degli oggetti: non è la forma che il prompt
            ' chiede, ma se arriva si mostra lo stesso.
            Dim storto As String = VistaAnnuncio.Riassunto(JsonNode.Parse(
                "{""competenze_richieste"": [""Patente C""], ""mansioni"": [""Consegne"", 7]}"))

            Assert.Contains("• Patente C", storto, "la voce di testo si legge")
            Assert.Contains("• Consegne", storto, "e anche le mansioni")

        End Sub

    End Class

End Namespace
