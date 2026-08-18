Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi di chi sa <b>quali</b> campi di un documento sono prosa (T7b, cap. 08.2):
    ''' li estrae, li manda a rifinire per genere e rimette i testi al loro posto.
    ''' </summary>
    ''' <remarks>
    ''' La promessa che questa batteria custodisce è quella del cap. 08.2: i <b>campi-fatto
    ''' non si toccano</b>. Non si verifica leggendo il documento dopo — un modello che non
    ''' li cambia lo farebbe passare comunque — ma guardando che cosa <b>parte</b>: nomi,
    ''' aziende, date, competenze e titoli nella richiesta non entrano affatto, e quel che
    ''' non entra non può tornare cambiato.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiRifinitura

        Private Shared Function Cv() As JsonNode

            Return JsonNode.Parse(
                "{""tipo"": ""cv_mirato""," &
                """intestazione"": {""nome"": ""Luca Ferrari"", ""email"": ""luca@example.it""}," &
                """sommario"": ""Ho esperienza nel servizio di sala.""," &
                """esperienze_professionali"": [" &
                "  {""ruolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino"", ""durata"": ""2 anni""," &
                "   ""descrizione"": ""Servizio ai tavoli — e gestione della cassa""}," &
                "  {""ruolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A."", ""durata"": ""1 anno""," &
                "   ""descrizione"": ""Carico e scarico merci""}]," &
                """altre_esperienze"": [{""descrizione"": ""Aiuto in oratorio"", ""quando"": ""2019""}]," &
                """competenze"": [""HACCP"", ""Muletto""]," &
                """formazione"": [{""titolo"": ""Perito elettronico"", ""istituto"": ""ITIS"", ""anno"": ""2018""}]}")

        End Function

        Private Shared Function Lettera() As JsonNode

            Return JsonNode.Parse(
                "{""tipo"": ""lettera_mirata""," &
                """apertura"": ""Spettabile Azienda,""," &
                """corpo"": ""Mi candido perché ho esperienza di sala.""," &
                """chiusura"": ""Cordiali saluti,""," &
                """firma"": {""nome"": ""Luca Ferrari"", ""email"": ""luca@example.it""}}")

        End Function

        Private Shared Function Testo(documento As JsonNode, campo As String) As String
            Return documento(campo).GetValue(Of String)()
        End Function

        Private Shared Function Descrizione(documento As JsonNode, lista As String, indice As Integer) As String
            Return documento(lista)(indice)("descrizione").GetValue(Of String)()
        End Function

        <TestMethod>
        Public Async Function DalCvPartonoDueGeneriConGliIdGiusti() As Task

            Dim finto As New RifinitoreFinto()

            Await New Rifinitura(finto).DelCvAsync(Cv())

            Assert.AreEqual("Sintesi → Frasi", finto.GeneriChiesti(),
                            "prima il sommario, poi le descrizioni: due forme, due prompt")

            Assert.AreEqual("sommario", finto.Passate(0).Id(), "il sommario da solo")
            Assert.AreEqual("esperienza.0, esperienza.1, altra.0", finto.Passate(1).Id(),
                            "le descrizioni insieme, formali e informali: sono la stessa forma")

        End Function

        <TestMethod>
        Public Async Function IlCampiFattoNonPartonoNeppure() As Task

            ' Cap. 08.2. Il modello non può cambiare un nome che non ha mai visto.
            Dim finto As New RifinitoreFinto()

            Await New Rifinitura(finto).DelCvAsync(Cv())

            Dim partito As String = String.Join(vbLf,
                finto.Passate.SelectMany(Function(p) p.Pezzi).Select(Function(p) p.Testo))

            Assert.DoesNotContain("Luca Ferrari", partito, "il nome resta a casa")
            Assert.DoesNotContain("Trattoria Da Gino", partito, "e l'azienda")
            Assert.DoesNotContain("HACCP", partito, "e le competenze")
            Assert.DoesNotContain("Perito elettronico", partito, "e i titoli di studio")
            Assert.DoesNotContain("2 anni", partito, "e le durate")

        End Function

        <TestMethod>
        Public Async Function IlDocumentoSiRiscriveEIlPrimaTieneSoloICambiati() As Task

            Dim finto As RifinitoreFinto = New RifinitoreFinto().
                Dara("sommario", "Lavoro in sala da due anni.").
                Dara("esperienza.0", "Servizio ai tavoli e gestione della cassa")

            Dim documento As JsonNode = Cv()
            Dim prima As JsonObject = Await New Rifinitura(finto).DelCvAsync(documento)

            Assert.AreEqual("Lavoro in sala da due anni.", Testo(documento, "sommario"),
                            "il sommario rifinito è nel documento")
            Assert.AreEqual("Servizio ai tavoli e gestione della cassa",
                            Descrizione(documento, "esperienze_professionali", 0),
                            "e la descrizione senza lineetta")

            Assert.AreEqual("Ho esperienza nel servizio di sala.", Testo(prima, "sommario"),
                            "il «prima» conserva com'era")
            Assert.AreEqual(2, prima.Count, "e tiene solo i due cambiati")
            Assert.IsFalse(prima.ContainsKey("esperienza.1"),
                           "un testo tornato identico non è un cambiamento")

        End Function

        <TestMethod>
        Public Async Function SeNienteCambiaNonRestaNessunPrima() As Task

            ' Il permesso di non cambiare è scritto nei prompt: qui si verifica che quando
            ' il modello lo esercita non nasca un prima/dopo che non ha niente da mostrare.
            Dim prima As JsonObject = Await New Rifinitura(New RifinitoreFinto()).DelCvAsync(Cv())

            Assert.IsNull(prima, "niente da raccontare, niente campo da scrivere")

        End Function

        <TestMethod>
        Public Async Function DallaLetteraParteSoloIlCorpo() As Task

            ' Apertura, chiusura e firma sono formule che il lettore si aspetta, non slop:
            ' umanizzarle vorrebbe dire romperle (cap. 08.2).
            Dim finto As RifinitoreFinto = New RifinitoreFinto().
                Dara("corpo", "Mi candido: ho lavorato in sala.")

            Dim documento As JsonNode = Lettera()
            Await New Rifinitura(finto).DellaLetteraAsync(documento)

            Assert.HasCount(1, finto.Passate, "una passata sola")
            Assert.AreEqual(GenereProsa.Prosa, finto.Passate(0).Genere, "ed è prosa distesa")
            Assert.AreEqual("corpo", finto.Passate(0).Id(), "il corpo e nient'altro")

            Assert.AreEqual("Spettabile Azienda,", Testo(documento, "apertura"), "l'apertura è intatta")
            Assert.AreEqual("Cordiali saluti,", Testo(documento, "chiusura"), "e la chiusura")
            Assert.AreEqual("Luca Ferrari", documento("firma")("nome").GetValue(Of String)(),
                            "e la firma non è mai stata in ballo")

        End Function

        <TestMethod>
        Public Async Function LaLinguaArrivaFinoAlRifinitore() As Task

            ' Un CV inglese rifinito con le regole italiane è il difetto che il Pool 1.06 e
            ' il 1.07 hanno già chiuso altrove: qui non deve rinascere.
            Dim finto As New RifinitoreFinto()

            Await New Rifinitura(finto).DelCvAsync(Cv(), "en")

            Assert.IsTrue(finto.Passate.All(Function(p) p.Lingua = "en"),
                          $"tutte le passate in inglese, non solo la prima ({finto.GeneriChiesti()})")

        End Function

        <TestMethod>
        Public Async Function UnTestoScioltoTornaRifinitoOComEra() As Task

            ' È la strada del corpo dell'email, che non è un documento JSON (cap. 07.1).
            Dim rifinita As Rifinitura = New Rifinitura(
                New RifinitoreFinto().Dara("corpo", "Buongiorno, allego il mio CV."))

            Assert.AreEqual("Buongiorno, allego il mio CV.",
                            Await rifinita.DelTestoAsync("Buongiorno — allego il mio CV."),
                            "il testo rifinito")

            Assert.AreEqual("", Await New Rifinitura(New RifinitoreFinto()).DelTestoAsync(""),
                            "e un testo vuoto resta vuoto senza disturbare nessuno")

        End Function

        <TestMethod>
        Public Sub IlConfrontoMetteInFilaSoloICampiCheHannoUnPrima()

            Dim documento As JsonNode = Cv()
            Dim prima As JsonNode = JsonNode.Parse(
                "{""sommario"": ""com'era il sommario"", ""esperienza.1"": ""com'era la seconda""}")

            Dim cambiati As List(Of Rifinitura.CampoRifinito) = Rifinitura.Confronta(documento, prima)

            Assert.HasCount(2, cambiati, "due campi cambiati, due righe")

            Assert.AreEqual("Sommario", cambiati(0).Etichetta, "in italiano, e nell'ordine del CV")
            Assert.AreEqual("com'era il sommario", cambiati(0).Prima, "il prima")
            Assert.AreEqual("Ho esperienza nel servizio di sala.", cambiati(0).Dopo, "e il dopo")

            Assert.AreEqual("Esperienza 2", cambiati(1).Etichetta,
                            "contata da 1, come la conta una persona")

        End Sub

        <TestMethod>
        Public Sub SenzaUnPrimaNonCEUnConfronto()

            Assert.IsEmpty(Rifinitura.Confronta(Cv(), Nothing), "niente prima, niente confronto")
            Assert.IsEmpty(Rifinitura.Confronta(Nothing, JsonNode.Parse("{""sommario"":""x""}")),
                           "e niente documento, nemmeno")

        End Sub

        <TestMethod>
        Public Sub GliIdSiLeggonoInItaliano()

            Assert.AreEqual("Sommario", Rifinitura.ComeSiLegge("sommario"))
            Assert.AreEqual("Corpo della lettera", Rifinitura.ComeSiLegge("corpo"))
            Assert.AreEqual("Esperienza 1", Rifinitura.ComeSiLegge("esperienza.0"))
            Assert.AreEqual("Altra esperienza 3", Rifinitura.ComeSiLegge("altra.2"))

        End Sub

    End Class

End Namespace
