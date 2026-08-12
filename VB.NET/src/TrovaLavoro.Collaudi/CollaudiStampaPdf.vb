Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Web

Namespace Documenti

    ''' <summary>
    ''' Il collaudo della stampa PDF vera (cap. 05.5): la pagina passa dalla WebView2
    ''' fuori schermo e ne esce un file.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché sta fra i collaudi «Reale»</b>, che la batteria di tutti i giorni
    ''' esclude. Qui di rete non ce n'è e non si spende un token: quello che serve è la
    ''' <b>macchina</b> — il motore Edge/Chromium di Windows e un thread STA con la sua
    ''' pompa di messaggi. Sono esattamente le condizioni che la batteria non deve
    ''' pretendere per girare ovunque e in tre secondi.</para>
    ''' <para><b>Che cosa dimostra il PDF prodotto.</b> Che il testo è <b>vero testo</b>,
    ''' non un'immagine: la mappa <c>/ToUnicode</c> è ciò che rende un PDF selezionabile e
    ''' ricercabile, e <c>FontFile2</c> è il font incorporato, cioè la garanzia che il
    ''' documento si legga uguale su un computer che quel font non ce l'ha. Sono le due
    ''' prove chieste dal cap. 13.3 al cancello di T4, qui ripetute sul codice vero.</para>
    ''' <para>Si lancia da <c>VB.NET/src</c> con l'altro file di impostazioni:
    ''' <c>dotnet test --settings TrovaLavoro.Collaudi\collaudi-reali.runsettings</c>.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiStampaPdf

        Private Shared Function CvDiProva() As JsonNode

            Return JsonNode.Parse("
                {
                  ""tipo"": ""cv_base"",
                  ""intestazione"": {
                    ""nome"": ""Luca Ferrarì"", ""email"": ""luca.ferrari@example.it"",
                    ""telefono"": ""333 1234567"", ""citta"": ""Modena"",
                    ""link"": """", ""patente"": ""B""
                  },
                  ""sommario"": ""Ho esperienza nel servizio di sala, con attenzione alla puntualità."",
                  ""esperienze_professionali"": [
                    { ""ruolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino"",
                      ""durata"": ""2019-2023"", ""descrizione"": ""Servizio ai tavoli e cassa."" }
                  ],
                  ""altre_esperienze"": [],
                  ""competenze"": [""Servizio ai tavoli"", ""Uso del muletto""],
                  ""formazione"": [
                    { ""titolo"": ""Diploma alberghiero"", ""istituto"": ""IPSSAR Modena"", ""anno"": ""2018"" }
                  ]
                }")

        End Function

        ''' <summary>Una cartella temporanea che si porta via tutto alla fine.</summary>
        Private Shared Sub ConCartellaTemporanea(prova As Action(Of String))

            Dim cartella As String = Path.Combine(Path.GetTempPath(),
                                                  "pdf-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(cartella)

            Try
                prova(cartella)
            Finally
                PortaVia(cartella)
            End Try

        End Sub

        ''' <summary>
        ''' Cancella la cartella, con pazienza. Il motore del browser chiude i suoi
        ''' processi <b>dopo</b> che il controllo è stato smesso, e finché non ha finito
        ''' tiene il proprio <c>lockfile</c>: cancellare al primo colpo fallisce, e
        ''' l'errore coprirebbe l'esito vero del collaudo. Se dopo qualche tentativo la
        ''' cartella resiste ancora si lascia dov'è — è la cartella temporanea di
        ''' Windows, e un collaudo non deve fallire per le pulizie.
        ''' </summary>
        Private Shared Sub PortaVia(cartella As String)

            For tentativo As Integer = 1 To 10
                Try
                    Directory.Delete(cartella, recursive:=True)
                    Return
                Catch ex As IOException
                    Thread.Sleep(300)
                Catch ex As UnauthorizedAccessException
                    Thread.Sleep(300)
                End Try
            Next

        End Sub

        <TestMethod, TestCategory("Reale")>
        Public Sub IlPdfEsceConTestoVeroEFontIncorporati()

            ConCartellaTemporanea(
                Sub(cartella)

                    Dim documento As String = Path.Combine(cartella, "CV.pdf")
                    Dim lettera As String = Path.Combine(cartella, "Lettera.pdf")

                    ThreadInterfaccia.Esegui(
                        Async Function() As Task
                            Using stampante As New StampantePdf(
                                New MotoreBrowser(Path.Combine(cartella, "webview2")))

                                Await stampante.StampaAsync(
                                    Impaginazione.PaginaCv(CvDiProva()), documento)

                                ' La seconda stampa riusa il motore già acceso: è il caso
                                ' vero, dove a un CV segue sempre la sua lettera.
                                Await stampante.StampaAsync(
                                    Impaginazione.PaginaLettera(JsonNode.Parse(
                                        "{ ""apertura"": ""Spettabile Azienda,"", " &
                                        """corpo"": ""Mi candido per la posizione."", " &
                                        """chiusura"": ""Cordiali saluti,"", " &
                                        """firma"": { ""nome"": ""Luca Ferrarì"" } }")),
                                    lettera)

                            End Using
                        End Function)

                    For Each prodotto As String In {documento, lettera}

                        Assert.IsTrue(File.Exists(prodotto), $"Manca il file «{prodotto}».")

                        Dim contenuto As String =
                            Encoding.Latin1.GetString(File.ReadAllBytes(prodotto))

                        StringAssert.StartsWith(contenuto, "%PDF", "Non è un PDF.")
                        StringAssert.Contains(contenuto, "/ToUnicode",
                                              "Il testo del PDF non sarebbe selezionabile.")
                        StringAssert.Contains(contenuto, "FontFile2",
                                              "Il PDF non si porta dietro i suoi font.")

                    Next

                    ' Scrittura atomica: del file di passaggio non resta traccia.
                    CollectionAssert.AreEquivalent(
                        {"CV.pdf", "Lettera.pdf", "webview2"},
                        Directory.GetFileSystemEntries(cartella).
                            Select(AddressOf Path.GetFileName).ToArray())

                End Sub)

        End Sub

    End Class

End Namespace
