Imports System.Linq
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
        Public Sub IRiscrittiAManoPortanoIlNomeDelCampoEIlTestoDiAdesso()

            ' R7: è quel che riceve il prompt della lettera, ed è l'unica cosa dentro un CV
            ' che valga come fonte di fatti — le parole non le ha scritte un modello, le ha
            ' scritte la persona.
            Dim documento As JsonNode = Cv()
            Rifinitura.Riscrivi(documento, "esperienza.1", "Ho traslocato elefanti.")

            Dim riscritti As JsonArray = Rifinitura.RiscrittiAMano(documento, {"esperienza.1"})

            Assert.HasCount(1, riscritti, "un campo solo")
            Assert.AreEqual("Esperienza 2", riscritti(0)("campo").GetValue(Of String)(),
                            "col nome come si legge a video, non con l'id")
            Assert.AreEqual("Ho traslocato elefanti.", riscritti(0)("testo").GetValue(Of String)(),
                            "e col testo che c'è adesso nel documento")

        End Sub

        <TestMethod>
        Public Sub UnCampoRiscrittoCheNelDocumentoNonCEPiuNonSiRiferisce()

            ' Può succedere: il documento è stato rigenerato, o il profilo ha perso quella
            ' voce. Si riferisce quel che c'è, non quel che risultava — e senza inventare
            ' un testo vuoto, che al modello direbbe una cosa falsa.
            Assert.IsEmpty(Rifinitura.RiscrittiAMano(Cv(), {"esperienza.9"}),
                           "un campo che non esiste non porta niente")

            Assert.IsEmpty(Rifinitura.RiscrittiAMano(Nothing, {"sommario"}),
                           "e senza documento nemmeno")

        End Sub

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
        Public Async Function SpentaNonChiamaLAiPerIlCv() As Task

            Dim finto As New RifinitoreFinto()

            Dim prima As JsonObject = Await New Rifinitura(finto, Function() False).DelCvAsync(Cv())

            ' Il punto non è che il documento torni uguale: è che <b>non si chiami</b>.
            ' Una rifinitura spenta che interrogasse il modello per poi buttare via la
            ' risposta costerebbe soldi e tempo all'utente che l'ha spenta apposta.
            Assert.IsEmpty(finto.Passate, "spenta, l'AI non va disturbata affatto")
            Assert.IsNull(prima, "e non c'è nessun «com'era» da mostrare")

        End Function

        <TestMethod>
        Public Async Function SpentaNonChiamaLAiPerLaLettera() As Task

            Dim finto As New RifinitoreFinto()

            Assert.IsNull(Await New Rifinitura(finto, Function() False).DellaLetteraAsync(Lettera()))
            Assert.IsEmpty(finto.Passate)

        End Function

        <TestMethod>
        Public Async Function SpentaIlTestoTornaIdenticoSenzaChiamare() As Task

            Dim finto As RifinitoreFinto = New RifinitoreFinto().Dara("corpo", "questo non deve arrivare mai")

            Dim esito As String = Await New Rifinitura(finto, Function() False).
                DelTestoAsync("Buongiorno, mi candido.")

            Assert.AreEqual("Buongiorno, mi candido.", esito, "spenta, il testo esce com'è entrato")
            Assert.IsEmpty(finto.Passate)

        End Function

        <TestMethod>
        Public Async Function AccesaLavoraComeSempre() As Task

            ' Il gemello del collaudo qui sopra: se questo non fosse verde, i tre di prima
            ' sarebbero verdi per il motivo sbagliato — una rifinitura rotta non chiama
            ' l'AI nemmeno lei.
            Dim finto As New RifinitoreFinto()

            Await New Rifinitura(finto, Function() True).DelCvAsync(Cv())

            Assert.IsNotEmpty(finto.Passate, "accesa, il mestiere viene chiamato")

        End Function

        <TestMethod>
        Public Async Function LInterruttoreSiLeggeAOgniGiroNonAllaCostruzione() As Task

            ' È la ragione per cui il costruttore vuole una funzione e non un valore: la
            ' finestra delle Impostazioni salva subito, e la generazione che parte dopo
            ' deve già saperlo, senza aspettare un riavvio.
            Dim accesa As Boolean = True
            Dim finto As New RifinitoreFinto()
            Dim rifinitura As New Rifinitura(finto, Function() accesa)

            Await rifinitura.DelCvAsync(Cv())
            Assert.IsNotEmpty(finto.Passate, "prima era accesa")

            Dim quante As Integer = finto.Passate.Count
            accesa = False

            Await rifinitura.DelCvAsync(Cv())
            Assert.HasCount(quante, finto.Passate, "spenta a caldo, non ha chiamato di nuovo")

        End Function

        <TestMethod>
        Public Sub SenzaInterruttoreERimastaAccesa()

            ' Chi la costruisce senza dire niente — il banco, e chiunque non abbia un
            ' contesto intorno — deve trovarla com'era prima che l'interruttore esistesse.
            Assert.IsTrue(New Rifinitura(New RifinitoreFinto()).Accesa)

        End Sub

        <TestMethod>
        Public Sub GliIdSiLeggonoInItaliano()

            Assert.AreEqual("Sommario", Rifinitura.ComeSiLegge("sommario"))
            Assert.AreEqual("Corpo della lettera", Rifinitura.ComeSiLegge("corpo"))
            Assert.AreEqual("Esperienza 1", Rifinitura.ComeSiLegge("esperienza.0"))
            Assert.AreEqual("Altra esperienza 3", Rifinitura.ComeSiLegge("altra.2"))

        End Sub

        ' ==================================================================
        ' La prosa che si riscrive a mano (T9d, cap. 08.4)
        ' ==================================================================

        <TestMethod>
        Public Sub LaProsaDiUnCvSiElencaNellOrdineInCuiSiLegge()

            ' È l'elenco che la modifica a mano mette davanti all'utente: se l'ordine non
            ' fosse quello del CV, chi cerca «la seconda esperienza» la troverebbe altrove.
            Dim campi As List(Of Rifinitura.CampoDiProsa) = Rifinitura.CampiDiProsa(Cv())

            Assert.HasCount(4, campi, "sommario, due esperienze e un'altra esperienza")

            Assert.AreEqual("sommario", campi(0).Id, "prima il sommario")
            Assert.AreEqual("Sommario", campi(0).Etichetta, "con l'etichetta che si legge")
            Assert.AreEqual("Ho esperienza nel servizio di sala.", campi(0).Testo, "e il testo di adesso")

            Assert.AreEqual("Esperienza 1", campi(1).Etichetta, "poi le esperienze, contate da 1")
            Assert.AreEqual("Esperienza 2", campi(2).Etichetta, "nell'ordine del documento")
            Assert.AreEqual("Altra esperienza 1", campi(3).Etichetta, "e in fondo quelle informali")

        End Sub

        <TestMethod>
        Public Sub DiUnaLetteraSiRiscriveSoloIlCorpo()

            ' Apertura, chiusura e firma restano fuori come dalla rifinitura (cap. 08.2):
            ' non sono slop, sono le formule che il lettore si aspetta.
            Dim campi As List(Of Rifinitura.CampoDiProsa) = Rifinitura.CampiDiProsa(Lettera())

            Assert.HasCount(1, campi, "un campo solo")
            Assert.AreEqual("corpo", campi(0).Id, "il corpo")

        End Sub

        <TestMethod>
        Public Sub INomiELeDateNonSonoProsa()

            ' La promessa del cap. 08.2 vista dalla parte dell'utente: quello che si
            ' riscrive a mano è la prosa, non i fatti — che vengono dal profilo.
            Dim id As String = String.Join(" ", Rifinitura.CampiDiProsa(Cv()).Select(Function(c) c.Id))

            Assert.DoesNotContain("intestazione", id, "il nome no")
            Assert.DoesNotContain("competenze", id, "le competenze no")
            Assert.DoesNotContain("formazione", id, "i titoli di studio no")

        End Sub

        <TestMethod>
        Public Sub RiscrivereUnCampoLoMetteAlPostoGiusto()

            Dim documento As JsonNode = Cv()

            Assert.IsTrue(Rifinitura.Riscrivi(documento, "esperienza.1", "L'ho scritto io."), "riscritto")

            Assert.AreEqual("L'ho scritto io.", Descrizione(documento, "esperienze_professionali", 1),
                            "il testo è nella voce chiesta")
            Assert.AreEqual("Servizio ai tavoli — e gestione della cassa",
                            Descrizione(documento, "esperienze_professionali", 0),
                            "e l'altra esperienza non l'ha toccata nessuno")

        End Sub

        <TestMethod>
        Public Sub UnTestoVuotoNonSvuotaIlCampo()

            ' Un campo senza testo esce dall'elenco della prosa, e da lì non ci si
            ' rientrerebbe più per rimetterlo: togliere una descrizione è un'altra cosa, e
            ' si fa dal profilo.
            Dim documento As JsonNode = Cv()

            Assert.IsFalse(Rifinitura.Riscrivi(documento, "sommario", "   "), "il vuoto si rifiuta")
            Assert.AreEqual("Ho esperienza nel servizio di sala.", Testo(documento, "sommario"), "e il testo è ancora lì")

        End Sub

        <TestMethod>
        Public Sub UnCampoCheQuiNonCeNonSiRiscrive()

            Dim documento As JsonNode = Lettera()

            Assert.IsFalse(Rifinitura.Riscrivi(documento, "sommario", "Un sommario in una documento."),
                           "una documento il sommario non ce l'ha")
            Assert.IsFalse(Rifinitura.Riscrivi(documento, "esperienza.7", "La settima."),
                           "e nemmeno un'esperienza che non esiste")
            Assert.AreEqual("Mi candido perché ho esperienza di sala.", Testo(documento, "corpo"),
                            "il documento è rimasto com'era")

        End Sub

    End Class

End Namespace
