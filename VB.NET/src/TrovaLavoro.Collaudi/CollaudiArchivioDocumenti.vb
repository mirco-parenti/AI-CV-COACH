Imports System.IO
Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore

Namespace Documenti

    ''' <summary>
    ''' Collaudi del posto in cui i documenti atterrano (cap. 05.6, cap. 11.1): il 📄 CV
    ''' base accanto al profilo, il 🎯 CV mirato e la ✉️ lettera nella cartella della
    ''' candidatura.
    ''' </summary>
    ''' <remarks>
    ''' Qui la stampante PDF non c'è di proposito: vuole il motore di Windows, e la
    ''' domanda di questi collaudi è un'altra — <b>dove</b> finiscono i file e
    ''' <b>come</b> si chiamano. Che il PDF esca davvero lo chiede
    ''' <see cref="CollaudiStampaPdf"/>, fra i collaudi reali.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiArchivioDocumenti

        Private Shared Function CvDiProva(tipo As String) As JsonNode

            Return JsonNode.Parse($"
                {{
                  ""tipo"": ""{tipo}"",
                  ""intestazione"": {{ ""nome"": ""Luca Ferrari"", ""email"": ""luca@example.it"" }},
                  ""sommario"": ""Ho esperienza nel servizio di sala."",
                  ""competenze"": [""Servizio ai tavoli""]
                }}")

        End Function

        Private Shared Function OpportunitaDiProva() As Opportunita

            Return New Opportunita With {
                .Creata = New Date(2026, 8, 10, 9, 30, 0),
                .Annuncio = JsonNode.Parse(
                    "{ ""titolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino"" }"),
                .Confronto = JsonNode.Parse("{ ""giudizi"": [] }"),
                .Cv = CvDiProva("cv_mirato"),
                .Lettera = JsonNode.Parse(
                    "{ ""apertura"": ""Spettabile Azienda,"", ""corpo"": ""Mi candido."", " &
                    """firma"": { ""nome"": ""Luca Ferrari"" } }")}

        End Function

        Private Shared Sub ConCartellaDati(prova As Action(Of CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "documenti-" & Guid.NewGuid().ToString("N"))
            Try
                prova(New CartellaDati(radice))
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

        Private Shared Function NomiIn(cartella As String) As String()
            Return Directory.GetFiles(cartella).Select(AddressOf Path.GetFileName).OrderBy(
                Function(n) n, StringComparer.Ordinal).ToArray()
        End Function

        <TestMethod>
        Public Sub IlCvBaseVaAccantoAlProfilo()

            Dim scritti As IReadOnlyList(Of String) = Nothing

            ConCartellaDati(
                Sub(cartella)
                    Dim documenti As New ArchivioDocumenti(cartella)
                    scritti = documenti.ScriviCvBaseAsync(
                        CvDiProva("cv_base"), New Date(2026, 8, 10)).GetAwaiter().GetResult()

                    CollectionAssert.AreEqual({"CV_Luca_Ferrari_2026-08-10.docx"},
                                              NomiIn(cartella.CartellaOutProfilo))

                    ' Il CV base non appartiene a nessuna candidatura: fra le opportunità
                    ' non deve esserci finito niente (cap. 11.1).
                    Assert.IsFalse(Directory.Exists(cartella.CartellaOpportunita) AndAlso
                                   Directory.GetFileSystemEntries(cartella.CartellaOpportunita).Any())
                End Sub)

            Assert.HasCount(1, scritti)

        End Sub

        <TestMethod>
        Public Sub ICvMiratoELaLetteraVannoNellaCandidatura()

            ConCartellaDati(
                Sub(cartella)
                    Dim opportunita As Opportunita = OpportunitaDiProva()
                    Dim archivio As New ArchivioOpportunita(cartella)
                    archivio.Salva(opportunita)

                    Dim documenti As New ArchivioDocumenti(cartella)
                    Dim scritti As IReadOnlyList(Of String) =
                        documenti.ScriviCandidaturaAsync(opportunita).GetAwaiter().GetResult()

                    Dim dove As String = ArchivioOpportunita.CartellaOut(opportunita)

                    CollectionAssert.AreEqual(
                        {"CV_Luca_Ferrari_Trattoria_Da_Gino_2026-08-10.docx",
                         "Lettera_Trattoria_Da_Gino_2026-08-10.docx"},
                        NomiIn(dove))

                    ' I documenti stanno in «out», gli artefatti JSON restano sopra: sono
                    ' due cose diverse e non si mescolano (cap. 11.1).
                    CollectionAssert.AreEquivalent(
                        {"annuncio.json", "giudizi.json", "cv.json", "lettera.json", "stato.json"},
                        NomiIn(opportunita.Cartella))

                    Assert.HasCount(2, scritti)
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaCandidaturaAMetaScriveQuelCheHa()

            ConCartellaDati(
                Sub(cartella)
                    Dim opportunita As Opportunita = OpportunitaDiProva()
                    opportunita.Lettera = Nothing

                    Dim archivio As New ArchivioOpportunita(cartella)
                    archivio.Salva(opportunita)

                    Dim documenti As New ArchivioDocumenti(cartella)
                    documenti.ScriviCandidaturaAsync(opportunita).GetAwaiter().GetResult()

                    CollectionAssert.AreEqual({"CV_Luca_Ferrari_Trattoria_Da_Gino_2026-08-10.docx"},
                                              NomiIn(ArchivioOpportunita.CartellaOut(opportunita)))
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaCandidaturaSenzaDocumentiNonCreaNemmenoLaCartella()

            ConCartellaDati(
                Sub(cartella)
                    Dim opportunita As Opportunita = OpportunitaDiProva()
                    opportunita.Cv = Nothing
                    opportunita.Lettera = Nothing

                    Dim archivio As New ArchivioOpportunita(cartella)
                    archivio.Salva(opportunita)

                    Dim documenti As New ArchivioDocumenti(cartella)
                    Dim scritti As IReadOnlyList(Of String) =
                        documenti.ScriviCandidaturaAsync(opportunita).GetAwaiter().GetResult()

                    Assert.IsEmpty(scritti)
                    Assert.IsFalse(Directory.Exists(ArchivioOpportunita.CartellaOut(opportunita)),
                                   "È nata una cartella «out» vuota.")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaCandidaturaNonSalvataNonSaDoveMettereIDocumenti()

            ConCartellaDati(
                Sub(cartella)
                    Dim documenti As New ArchivioDocumenti(cartella)

                    ' Prima viene la cartella dell'opportunità, poi i documenti: il
                    ' contrario è un errore di chi chiama, e va detto subito.
                    Assert.Throws(Of InvalidOperationException)(
                        Sub() documenti.ScriviCandidaturaAsync(OpportunitaDiProva()).
                                  GetAwaiter().GetResult())
                End Sub)

        End Sub

        <TestMethod>
        Public Sub SenzaCartellaDatiNonSiCostruisceLArchivio()
            Assert.Throws(Of ArgumentNullException)(
                Sub()
                    Dim documenti As New ArchivioDocumenti(Nothing)
                End Sub)
        End Sub

        ' --- Esportare un documento solo (R8, 2026-08-23) --------------------------------

        <TestMethod>
        Public Sub SiPuoEsportareIlSoloCvMirato()

            ConCartellaDati(
                Sub(cartella)
                    Dim opportunita As Opportunita = OpportunitaDiProva()
                    Dim archivio As New ArchivioOpportunita(cartella)
                    archivio.Salva(opportunita)

                    Dim documenti As New ArchivioDocumenti(cartella)
                    documenti.ScriviCandidaturaAsync(
                        opportunita, FormatiDocumento.Docx, DocumentiDaScrivere.Cv).GetAwaiter().GetResult()

                    CollectionAssert.AreEqual(
                        {"CV_Luca_Ferrari_Trattoria_Da_Gino_2026-08-10.docx"},
                        NomiIn(ArchivioOpportunita.CartellaOut(opportunita)),
                        "la lettera non è stata chiesta e non deve comparire")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub SiPuoEsportareLaSolaLettera()

            ConCartellaDati(
                Sub(cartella)
                    Dim opportunita As Opportunita = OpportunitaDiProva()
                    Dim archivio As New ArchivioOpportunita(cartella)
                    archivio.Salva(opportunita)

                    Dim documenti As New ArchivioDocumenti(cartella)
                    documenti.ScriviCandidaturaAsync(
                        opportunita, FormatiDocumento.Docx, DocumentiDaScrivere.Lettera).GetAwaiter().GetResult()

                    CollectionAssert.AreEqual(
                        {"Lettera_Trattoria_Da_Gino_2026-08-10.docx"},
                        NomiIn(ArchivioOpportunita.CartellaOut(opportunita)),
                        "e viceversa: il CV non è stato chiesto")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub ChiNonSceglieLiOttieneTuttiEDue()

            ConCartellaDati(
                Sub(cartella)
                    Dim opportunita As Opportunita = OpportunitaDiProva()
                    Dim archivio As New ArchivioOpportunita(cartella)
                    archivio.Salva(opportunita)

                    Dim documenti As New ArchivioDocumenti(cartella)
                    documenti.ScriviCandidaturaAsync(opportunita, FormatiDocumento.Docx).GetAwaiter().GetResult()

                    Assert.AreEqual(2, NomiIn(ArchivioOpportunita.CartellaOut(opportunita)).Count,
                                    "il comportamento di sempre non cambia per chi non chiede niente")
                End Sub)

        End Sub

    End Class

End Namespace
