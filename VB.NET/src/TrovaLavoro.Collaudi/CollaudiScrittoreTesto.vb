Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Documenti

Namespace Documenti

    ''' <summary>
    ''' Collaudi della stampante a video (cap. 05.3): la pagina di blocchi scritta in testo
    ''' semplice per l'anteprima di P6.
    ''' </summary>
    ''' <remarks>
    ''' La domanda è una sola, ed è la stessa che vale per le altre due stampanti: nel
    ''' documento finisce <b>tutto</b> quello che la pagina contiene. Un'anteprima che
    ''' perde per strada una sezione mostra un CV diverso da quello che i file avranno,
    ''' proprio nel punto in cui l'utente lo controlla.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiScrittoreTesto

        <TestMethod>
        Public Sub OgniBloccoDellaPaginaFinisceNelTesto()

            Dim pagina As New PaginaDocumento With {.Titolo = "CV"}
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Nome, .Testo = "Luca Ferrari"})
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Recapiti,
                                                .Voci = New List(Of String) From {"luca@example.it", "Modena"}})
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Paragrafo,
                                                .Testo = "Ho esperienza nel servizio di sala."})
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Sezione, .Testo = "Esperienze professionali"})
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Voce,
                                                .Testo = "Cameriere", .Dettaglio = "Da Gino · 2019-2023",
                                                .Descrizione = "Servizio ai tavoli."})
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Sezione, .Testo = "Competenze"})
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Elenco,
                                                .Voci = New List(Of String) From {"Servizio ai tavoli"}})
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Firma, .Testo = "Luca Ferrari"})

            Dim scritto As String = ScrittoreTesto.Componi(pagina)

            Assert.Contains("Luca Ferrari", scritto, "il nome")
            Assert.Contains("luca@example.it · Modena", scritto, "i recapiti, con lo stesso separatore delle stampanti")
            Assert.Contains("Ho esperienza nel servizio di sala.", scritto, "il sommario")
            Assert.Contains("Esperienze professionali", scritto, "il titolo di sezione")
            Assert.Contains("Cameriere", scritto, "la voce")
            Assert.Contains("Da Gino · 2019-2023", scritto, "il suo dettaglio")
            Assert.Contains("Servizio ai tavoli.", scritto, "e il suo racconto")
            Assert.Contains("• Servizio ai tavoli", scritto, "l'elenco, col suo punto")

        End Sub

        <TestMethod>
        Public Sub LeSezioniSiStaccanoDaCioCheLePrecede()

            Dim pagina As New PaginaDocumento
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Nome, .Testo = "Luca Ferrari"})
            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Sezione, .Testo = "Competenze"})

            Assert.Contains("Luca Ferrari" & vbCrLf & vbCrLf & "Competenze", ScrittoreTesto.Componi(pagina),
                            "una riga vuota prima del titolo: è l'unico «disegno» che il testo si permette")

        End Sub

        <TestMethod>
        Public Sub UnCvVeroSiLeggeTuttoDallIntestazioneAllaFormazione()

            ' Il giro completo, dalla stessa impaginazione che scrive DOCX e PDF.
            Dim cv As JsonNode = JsonNode.Parse("
                {
                  ""intestazione"": { ""nome"": ""Luca Ferrari"", ""email"": ""luca@example.it"",
                                      ""citta"": ""Modena"", ""patente"": ""B"" },
                  ""sommario"": ""Servizio di sala."",
                  ""esperienze_professionali"": [
                    { ""ruolo"": ""Cameriere"", ""azienda"": ""Da Gino"", ""durata"": ""2019-2023"",
                      ""descrizione"": ""Sala e cassa."" } ],
                  ""competenze"": [""Uso della cassa""],
                  ""formazione"": [ { ""titolo"": ""Diploma alberghiero"", ""istituto"": ""IPSSAR"", ""anno"": ""2018"" } ]
                }")

            Dim scritto As String = ScrittoreTesto.Componi(Impaginazione.PaginaCv(cv))

            Assert.Contains("Luca Ferrari", scritto, "il nome")
            Assert.Contains("Patente: B", scritto, "la patente, come la stampa l'etichetta")
            Assert.Contains(Impaginazione.SezioneEsperienze, scritto, "le esperienze")
            Assert.Contains("• Uso della cassa", scritto, "le competenze")
            Assert.Contains(Impaginazione.SezioneFormazione, scritto, "e la formazione")

        End Sub

        <TestMethod>
        Public Sub UnaPaginaCheNonCEDaLuogoAUnTestoVuoto()

            Assert.IsEmpty(ScrittoreTesto.Componi(Nothing), "niente pagina, niente testo")
            Assert.IsEmpty(ScrittoreTesto.Componi(New PaginaDocumento()), "e una pagina senza blocchi non scrive nulla")

        End Sub

    End Class

End Namespace
