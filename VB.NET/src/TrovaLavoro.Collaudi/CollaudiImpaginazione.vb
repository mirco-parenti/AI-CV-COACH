Imports System.Linq
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Documenti

Namespace Documenti

    ''' <summary>
    ''' Collaudi del modello di impaginazione (cap. 05.3): il punto in cui un CV o una
    ''' lettera in JSON diventano una pagina di blocchi, prima e indipendentemente da
    ''' qualunque formato di file.
    ''' </summary>
    ''' <remarks>
    ''' È il pezzo di T4b che decide <b>che cosa</b> finisce nel documento, ed è anche
    ''' l'unico collaudabile per intero senza aprire uno ZIP e senza accendere una
    ''' WebView: se sbaglia qui, sbagliano tutti e due i file allo stesso modo — che è poi
    ''' il motivo per cui questo passaggio esiste.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiImpaginazione

        ''' <summary>Un CV pieno, con tutte le sezioni: è la forma che il pool dichiara.</summary>
        Private Shared Function CvDiProva() As JsonNode

            Return JsonNode.Parse("
                {
                  ""tipo"": ""cv_base"",
                  ""intestazione"": {
                    ""nome"": ""Luca Ferrari"",
                    ""email"": ""luca.ferrari@example.it"",
                    ""telefono"": ""333 1234567"",
                    ""citta"": ""Modena"",
                    ""link"": ""linkedin.com/in/lucaferrari"",
                    ""patente"": ""B""
                  },
                  ""sommario"": ""Ho esperienza nel servizio di sala."",
                  ""esperienze_professionali"": [
                    { ""ruolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino"",
                      ""durata"": ""2019-2023"", ""descrizione"": ""Servizio ai tavoli e cassa."" }
                  ],
                  ""altre_esperienze"": [
                    { ""descrizione"": ""Volontariato alla sagra del paese"", ""quando"": ""estate 2021"" }
                  ],
                  ""competenze"": [""Servizio ai tavoli"", ""Uso del registratore di cassa""],
                  ""formazione"": [
                    { ""titolo"": ""Diploma alberghiero"", ""istituto"": ""IPSSAR Modena"", ""anno"": ""2018"" }
                  ]
                }")

        End Function

        ''' <summary>Una lettera piena, nei quattro blocchi che il pool dichiara.</summary>
        Private Shared Function LetteraDiProva() As JsonNode

            Return JsonNode.Parse("
                {
                  ""tipo"": ""lettera_mirata"",
                  ""apertura"": ""Spettabile Azienda, mi candido per la posizione di Cameriere."",
                  ""corpo"": ""Ho quattro anni di servizio di sala."",
                  ""chiusura"": ""Resto a disposizione. Cordiali saluti,"",
                  ""firma"": { ""nome"": ""Luca Ferrari"", ""email"": ""luca.ferrari@example.it"",
                               ""telefono"": ""333 1234567"" }
                }")

        End Function

        Private Shared Function Generi(pagina As PaginaDocumento) As GenereBlocco()
            Return pagina.Blocchi.Select(Function(b) b.Genere).ToArray()
        End Function

        Private Shared Function Primo(pagina As PaginaDocumento, genere As GenereBlocco) As Blocco
            Return pagina.Blocchi.FirstOrDefault(Function(b) b.Genere = genere)
        End Function

        Private Shared Function Sezioni(pagina As PaginaDocumento) As String()
            Return pagina.Blocchi.Where(Function(b) b.Genere = GenereBlocco.Sezione).
                Select(Function(b) b.Testo).ToArray()
        End Function

        <TestMethod>
        Public Sub IlCvPienoDiventaLaPaginaAttesa()

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(CvDiProva())

            ' L'ordine è quello del cap. 05.4: intestazione, sommario, esperienze,
            ' altre esperienze, competenze, formazione.
            CollectionAssert.AreEqual(
                {GenereBlocco.Nome, GenereBlocco.Recapiti, GenereBlocco.Recapiti,
                 GenereBlocco.Paragrafo,
                 GenereBlocco.Sezione, GenereBlocco.Voce,
                 GenereBlocco.Sezione, GenereBlocco.Voce,
                 GenereBlocco.Sezione, GenereBlocco.Elenco,
                 GenereBlocco.Sezione, GenereBlocco.Voce},
                Generi(pagina))

            CollectionAssert.AreEqual(
                {Impaginazione.SezioneEsperienze, Impaginazione.SezioneAltreEsperienze,
                 Impaginazione.SezioneCompetenze, Impaginazione.SezioneFormazione},
                Sezioni(pagina))

        End Sub

        <TestMethod>
        Public Sub LEsperienzaPortaRuoloAziendaDurataEDescrizione()

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(CvDiProva())
            Dim voce As Blocco = Primo(pagina, GenereBlocco.Voce)

            Assert.AreEqual("Cameriere", voce.Testo)
            Assert.AreEqual("Trattoria Da Gino · 2019-2023", voce.Dettaglio)
            Assert.AreEqual("Servizio ai tavoli e cassa.", voce.Descrizione)

        End Sub

        <TestMethod>
        Public Sub LAltraEsperienzaEIntitolataDalQuandoENonHaAzienda()

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(CvDiProva())

            ' La seconda voce è quella delle esperienze informali: il prompt vieta di
            ' dar loro un ruolo o un'azienda, e l'impaginazione non gliene inventa uno.
            Dim voce As Blocco = pagina.Blocchi.
                Where(Function(b) b.Genere = GenereBlocco.Voce).ToList()(1)

            Assert.AreEqual("estate 2021", voce.Testo)
            Assert.AreEqual("", voce.Dettaglio)
            Assert.AreEqual("Volontariato alla sagra del paese", voce.Descrizione)

        End Sub

        <TestMethod>
        Public Sub IRecapitiStannoInUnaRigaSolaELaPatenteNellaSua()

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(CvDiProva())
            Dim righe As List(Of Blocco) =
                pagina.Blocchi.Where(Function(b) b.Genere = GenereBlocco.Recapiti).ToList()

            Assert.HasCount(2, righe)
            CollectionAssert.AreEqual(
                {"luca.ferrari@example.it", "333 1234567", "Modena", "linkedin.com/in/lucaferrari"},
                righe(0).Voci.ToArray())
            CollectionAssert.AreEqual({"Patente: B"}, righe(1).Voci.ToArray())

        End Sub

        <TestMethod>
        Public Sub SenzaPatenteLaRigaDellaPatenteNonCE()

            Dim cv As JsonObject = CType(CvDiProva(), JsonObject)
            CType(cv("intestazione"), JsonObject)("patente") = JsonValue.Create("")

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(cv)

            Assert.AreEqual(1, pagina.Blocchi.Where(Function(b) b.Genere = GenereBlocco.Recapiti).Count())

        End Sub

        <TestMethod>
        Public Sub UnaSezioneVuotaSeNePortaViaIlTitolo()

            Dim cv As JsonObject = CType(CvDiProva(), JsonObject)
            cv("competenze") = New JsonArray()
            cv.Remove("formazione")

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(cv)

            ' Un «Competenze» seguito dal nulla è peggio che non averlo: spariscono
            ' insieme, e la lista mancante del tutto si comporta come quella vuota.
            CollectionAssert.AreEqual(
                {Impaginazione.SezioneEsperienze, Impaginazione.SezioneAltreEsperienze},
                Sezioni(pagina))

        End Sub

        <TestMethod>
        Public Sub UnaVoceSenzaNienteDentroNonDiventaUnBlocco()

            Dim cv As JsonObject = CType(CvDiProva(), JsonObject)
            cv("formazione") = JsonNode.Parse("[{ ""titolo"": """", ""istituto"": """", ""anno"": """" }]")

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(cv)

            Assert.IsFalse(Sezioni(pagina).Contains(Impaginazione.SezioneFormazione))

        End Sub

        <TestMethod>
        Public Sub UnAnnoScrittoComeNumeroResta()

            Dim cv As JsonObject = CType(CvDiProva(), JsonObject)
            cv("formazione") = JsonNode.Parse(
                "[{ ""titolo"": ""Diploma alberghiero"", ""istituto"": ""IPSSAR Modena"", ""anno"": 2018 }]")

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(cv)
            Dim voce As Blocco = pagina.Blocchi.Last(Function(b) b.Genere = GenereBlocco.Voce)

            ' Lo schema chiede una stringa, ma un anno che arriva come numero è un dato
            ' buono scritto in un altro modo: nel CV ci va lo stesso.
            Assert.AreEqual("IPSSAR Modena · 2018", voce.Dettaglio)

        End Sub

        <TestMethod>
        Public Sub IlSommarioSiSpezzaInCapoversi()

            Dim cv As JsonObject = CType(CvDiProva(), JsonObject)
            cv("sommario") = JsonValue.Create("Primo capoverso." & vbLf & vbLf & "Secondo capoverso.")

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(cv)
            Dim paragrafi As String() = pagina.Blocchi.
                Where(Function(b) b.Genere = GenereBlocco.Paragrafo).
                Select(Function(b) b.Testo).ToArray()

            CollectionAssert.AreEqual({"Primo capoverso.", "Secondo capoverso."}, paragrafi)

        End Sub

        <TestMethod>
        Public Sub NessunBloccoContieneUnACapo()

            Dim cv As JsonObject = CType(CvDiProva(), JsonObject)
            cv("sommario") = JsonValue.Create("Prima riga." & vbCrLf & "Seconda riga.")
            CType(CType(cv("esperienze_professionali"), JsonArray)(0), JsonObject)("descrizione") =
                JsonValue.Create("Servizio ai tavoli," & vbLf & vbTab & "cassa e magazzino.")

            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(cv)

            ' È l'invariante su cui le due stampanti fanno affidamento (v. PaginaDocumento):
            ' nessuna delle due deve decidere per conto suo che cosa fare di un a capo.
            For Each blocco As Blocco In pagina.Blocchi
                For Each testo As String In {blocco.Testo, blocco.Dettaglio, blocco.Descrizione}
                    Assert.IsFalse(testo.Contains(vbLf) OrElse testo.Contains(vbCr) OrElse testo.Contains(vbTab),
                                   $"Blocco {blocco.Genere} con un a capo dentro: «{testo}»")
                Next
                For Each voce As String In blocco.Voci
                    Assert.IsFalse(voce.Contains(vbLf) OrElse voce.Contains(vbCr) OrElse voce.Contains(vbTab))
                Next
            Next

            Assert.AreEqual("Servizio ai tavoli, cassa e magazzino.",
                            Primo(pagina, GenereBlocco.Voce).Descrizione)

        End Sub

        <TestMethod>
        Public Sub IlTitoloDelDocumentoPortaIlNome()

            Assert.AreEqual("CV — Luca Ferrari", Impaginazione.PaginaCv(CvDiProva()).Titolo)
            Assert.AreEqual("Lettera di presentazione — Luca Ferrari",
                            Impaginazione.PaginaLettera(LetteraDiProva()).Titolo)

        End Sub

        <TestMethod>
        Public Sub UnCvVuotoDaUnaPaginaVuotaSenzaLamentarsi()

            ' Non è un caso di scuola: l'utente ha appena aspettato la generazione, e un
            ' documento povero è meglio di un'eccezione.
            Dim pagina As PaginaDocumento = Impaginazione.PaginaCv(JsonNode.Parse("{}"))

            Assert.IsEmpty(pagina.Blocchi)
            Assert.AreEqual(Impaginazione.TitoloCv, pagina.Titolo)

            Dim storto As PaginaDocumento = Impaginazione.PaginaCv(JsonNode.Parse("[1, 2]"))
            Assert.IsEmpty(storto.Blocchi)

        End Sub

        <TestMethod>
        Public Sub SenzaCvNonSiImpaginaNiente()

            Assert.Throws(Of ArgumentNullException)(Sub() Impaginazione.PaginaCv(Nothing))
            Assert.Throws(Of ArgumentNullException)(Sub() Impaginazione.PaginaLettera(Nothing))

        End Sub

        <TestMethod>
        Public Sub LaLetteraHaCartaIntestataQuattroBlocchiEFirma()

            Dim pagina As PaginaDocumento = Impaginazione.PaginaLettera(LetteraDiProva())

            CollectionAssert.AreEqual(
                {GenereBlocco.Nome, GenereBlocco.Recapiti,
                 GenereBlocco.Paragrafo, GenereBlocco.Paragrafo, GenereBlocco.Paragrafo,
                 GenereBlocco.Firma},
                Generi(pagina))

            CollectionAssert.AreEqual({"luca.ferrari@example.it", "333 1234567"},
                                      Primo(pagina, GenereBlocco.Recapiti).Voci.ToArray())
            Assert.AreEqual("Luca Ferrari", Primo(pagina, GenereBlocco.Firma).Testo)

        End Sub

        <TestMethod>
        Public Sub IlCorpoDellaLetteraSiSpezzaInCapoversi()

            Dim lettera As JsonObject = CType(LetteraDiProva(), JsonObject)
            lettera("corpo") = JsonValue.Create("Ho quattro anni di sala." & vbLf & vbLf & "So usare la cassa.")

            Dim pagina As PaginaDocumento = Impaginazione.PaginaLettera(lettera)

            Assert.AreEqual(4, pagina.Blocchi.Where(Function(b) b.Genere = GenereBlocco.Paragrafo).Count())

        End Sub

        <TestMethod>
        Public Sub LaFirmaScrittaComeStringaValeLoStesso()

            Dim lettera As JsonObject = CType(LetteraDiProva(), JsonObject)
            lettera("firma") = JsonValue.Create("Luca Ferrari")

            Dim pagina As PaginaDocumento = Impaginazione.PaginaLettera(lettera)

            ' Il prototipo accetta anche questa forma, e una lettera senza nome in calce
            ' per un dettaglio di schema sarebbe una perdita gratuita.
            Assert.AreEqual("Luca Ferrari", Primo(pagina, GenereBlocco.Nome).Testo)
            Assert.AreEqual("Luca Ferrari", Primo(pagina, GenereBlocco.Firma).Testo)
            Assert.IsNull(Primo(pagina, GenereBlocco.Recapiti))

        End Sub

        <TestMethod>
        Public Sub UnaLetteraSenzaFirmaNonPerdeIlTesto()

            Dim lettera As JsonObject = CType(LetteraDiProva(), JsonObject)
            lettera.Remove("firma")

            Dim pagina As PaginaDocumento = Impaginazione.PaginaLettera(lettera)

            CollectionAssert.AreEqual(
                {GenereBlocco.Paragrafo, GenereBlocco.Paragrafo, GenereBlocco.Paragrafo},
                Generi(pagina))
            Assert.AreEqual(Impaginazione.TitoloLettera, pagina.Titolo)

        End Sub

    End Class

End Namespace
