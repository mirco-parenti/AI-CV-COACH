Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace Ai

    ''' <summary>
    ''' Collaudi del diario delle chiamate all'AI (cap. 02.5): il conto di quanto costa
    ''' ogni chiamata e di quanto sfiora il tetto di token del suo prompt.
    ''' </summary>
    ''' <remarks>
    ''' Nasce dalla coda del passaggio a Sonnet 5 *(2026-08-18)*: i <c>max_token</c> del
    ''' pool erano tarati su un modello che a parità di testo contava circa un terzo di
    ''' token in meno, e non c'era modo di sapere quali fossero diventati stretti se non
    ''' aspettando che si troncassero in faccia all'utente. Quello che si collauda qui è
    ''' proprio la possibilità di saperlo <b>prima</b>.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiDiarioChiamate

        Private Shared Function Uscita(tokenUscita As Integer,
                                       Optional motivo As String = "end_turn") As RispostaAi

            Return New RispostaAi With {
                .Testo = "quel che ha risposto",
                .MotivoFine = motivo,
                .Modello = "claude-sonnet-5",
                .TokenIngresso = 1200,
                .TokenUscita = tokenUscita}

        End Function

        Private Shared Sub ConCartellaTemporanea(prova As Action(Of CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "diario-" & Guid.NewGuid().ToString("N"))
            Try
                prova(New CartellaDati(radice))
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

        <TestMethod>
        Public Sub LaPrimaRigaSiPortaDietroLIntestazione()
            ConCartellaTemporanea(
                Sub(cartella)
                    Dim diario As New DiarioChiamateSuFile(cartella)

                    diario.Annota("email_candidatura", 1500, Uscita(1350))

                    Dim righe As String() = File.ReadAllLines(cartella.FileChiamateAi)

                    Assert.HasCount(2, righe, "l'intestazione e la riga")
                    Assert.Contains("percentuale_del_tetto", righe(0), "le colonne sono dichiarate")
                    Assert.Contains("email_candidatura", righe(1), "quale prompt")
                    Assert.Contains("1500", righe(1), "il tetto dichiarato dal prompt")
                    Assert.Contains("1350", righe(1), "e quanto ne ha usato")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LaPercentualeDiceQuantoMancaAlTetto()
            ' È il numero per cui il file esiste: 1350 token su un tetto di 1500 non
            ' dicono niente da soli, «90,0%» dice che quel prompt è in bilico.
            ConCartellaTemporanea(
                Sub(cartella)
                    Dim diario As New DiarioChiamateSuFile(cartella)

                    diario.Annota("email_candidatura", 1500, Uscita(1350))

                    Dim riga As String = File.ReadAllLines(cartella.FileChiamateAi)(1)

                    Assert.Contains("90.0", riga, "novanta per cento del tetto")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LeChiamateSiAccodanoSenzaRiscrivereLIntestazione()
            ConCartellaTemporanea(
                Sub(cartella)
                    Dim diario As New DiarioChiamateSuFile(cartella)

                    diario.Annota("email_candidatura", 1500, Uscita(1350))
                    diario.Annota("umanizzazione_frasi", 2500, Uscita(800))
                    diario.Annota("cv_base", 16000, Uscita(9000))

                    Dim righe As String() = File.ReadAllLines(cartella.FileChiamateAi)

                    Assert.HasCount(4, righe, "un'intestazione e tre chiamate")
                    Assert.Contains("umanizzazione_frasi", righe(2))
                    Assert.Contains("cv_base", righe(3))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnTettoSconosciutoNonInventaUnaPercentuale()
            ' Una percentuale calcolata su zero sarebbe un numero senza significato, e
            ' letta in un foglio di calcolo sembrerebbe un dato vero.
            ConCartellaTemporanea(
                Sub(cartella)
                    Dim diario As New DiarioChiamateSuFile(cartella)

                    diario.Annota("chissà", 0, Uscita(700))

                    Dim campi As String() = File.ReadAllLines(cartella.FileChiamateAi)(1).Split(";"c)

                    Assert.IsEmpty(campi(6), "la colonna della percentuale resta vuota")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlMotivoDellaFineSiPortaDietroIlTroncamento()
            ConCartellaTemporanea(
                Sub(cartella)
                    Dim diario As New DiarioChiamateSuFile(cartella)

                    diario.Annota("brainstorm", 2000, Uscita(2000, RispostaAi.MotivoTroncata))

                    Assert.Contains("max_tokens", File.ReadAllLines(cartella.FileChiamateAi)(1),
                                    "chi legge il file deve vedere chi si è troncato")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnaRispostaCheNonCEnonSiAnnota()
            ConCartellaTemporanea(
                Sub(cartella)
                    Dim diario As New DiarioChiamateSuFile(cartella)

                    diario.Annota("email_candidatura", 1500, Nothing)

                    Assert.IsFalse(File.Exists(cartella.FileChiamateAi),
                                   "niente da contare, nessun file")
                End Sub)
        End Sub

        <TestMethod>
        Public Async Function UnMestiereVeroAnnotaIlSuoPromptEIlSuoTetto() As Task

            ' La prova che l'aggancio regge da un capo all'altro: il diario sta sul client,
            ' ma a scriverci è MestiereAi — l'unico punto dove il prompt (col suo nome e il
            ' suo tetto) e la risposta (coi suoi token) si trovano insieme.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = "{""model"":""claude-sonnet-5"",""stop_reason"":""end_turn""," &
                         """content"":[{""type"":""text"",""text"":" &
                         """{\""tipo\"":\""email_candidatura\"",\""oggetto\"":\""\"",\""corpo\"":\""\""}""}]," &
                         """usage"":{""input_tokens"":1200,""output_tokens"":1350}}"})

            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim spia As New DiarioSpia()

            Using client As New ClientClaude("chiave-di-prova", Nothing, finta)

                client.Pausa = TimeSpan.Zero
                client.Diario = spia

                Await New CompositoreEmail(libreria, client).ComponiAsync(
                    JsonNode.Parse("{""tipo"":""lettera_mirata"",""corpo"":""Quattro anni di magazzino.""}"),
                    JsonNode.Parse("{""titolo"":""Magazziniere"",""azienda"":""Rossi S.p.A.""}"),
                    {"CV.pdf"}, Nothing, "it")

            End Using

            Assert.HasCount(1, spia.Voci, "una chiamata, una riga di diario")
            Assert.AreEqual("email_candidatura", spia.Voci(0).IdPrompt, "il prompt che è partito")
            Assert.AreEqual(1500, spia.Voci(0).Tetto, "il tetto che quel prompt dichiara nel pool")
            Assert.AreEqual(1350, spia.Voci(0).Uscita.TokenUscita, "e i token contati dall'API")

        End Function

        ''' <summary>Un diario che tiene tutto in memoria, per guardarci dentro.</summary>
        Private Class DiarioSpia
            Implements IDiarioChiamate

            Friend Class Voce
                Public Property IdPrompt As String
                Public Property Tetto As Integer
                Public Property Uscita As RispostaAi
            End Class

            Public ReadOnly Property Voci As New List(Of Voce)

            Public Sub Annota(idPrompt As String, tetto As Integer, uscita As RispostaAi) _
                Implements IDiarioChiamate.Annota

                Voci.Add(New Voce With {.IdPrompt = idPrompt, .Tetto = tetto, .Uscita = uscita})

            End Sub

        End Class

    End Class

End Namespace
