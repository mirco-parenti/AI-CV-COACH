Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Mcp

Namespace Mcp

    ''' <summary>
    ''' Il server MCP provato <b>sull'eseguibile vero</b>, avviato come lo avvia un
    ''' client: processo figlio, pipe al posto della console, e niente altro.
    ''' </summary>
    ''' <remarks>
    ''' <para>Perché non basta interrogare <see cref="ServerMcp.Rispondi"/>: questo è un
    ''' programma <c>WinExe</c>, cioè nato per aprire finestre, e un eseguibile di quel
    ''' tipo su Windows <b>non ha una console</b>. Che i suoi flussi standard funzionino
    ''' lo stesso quando è il client a fornirli è la sola cosa su cui poggia tutta la
    ''' modalità <c>--mcp</c>, e finché non la si vede accadere resta una scommessa.
    ''' Provarlo a mano non varrebbe: senza console non si vedrebbe niente comunque.</para>
    ''' <para>Le tre cose che si guardano: che risponda, che su <c>stdout</c> non finisca
    ''' <b>nient'altro</b> che protocollo, e che chiuda da sé quando l'ingresso si
    ''' chiude — che è il modo in cui un client lo spegne (cap. 09.2).</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiServerMcpDalVivo

        ''' <summary>Quanto si concede al processo per rispondere o per morire.</summary>
        Private Const Pazienza As Integer = 30000

        Private Shared Function Eseguibile() As String
            Return Path.Combine(AppContext.BaseDirectory, "TrovaLavoro.exe")
        End Function

        Private Shared Function CartellaTemporanea() As String
            Return Path.Combine(Path.GetTempPath(), "mcp-vivo-" & Guid.NewGuid().ToString("N"))
        End Function

        <TestMethod>
        <Timeout(120000)>
        Public Sub LEseguibileSenzaFinestreParlaSulleSuePipe()

            Dim exe As String = Eseguibile()
            Assert.IsTrue(File.Exists(exe), $"l'eseguibile deve stare accanto al banco: {exe}")

            Dim dati As String = CartellaTemporanea()

            ' Niente BOM in nessuna direzione: tre byte di firma in testa alla prima riga
            ' basterebbero a rendere illeggibile il primo messaggio.
            Dim senzaFirma As New UTF8Encoding(False)

            Dim avvio As New ProcessStartInfo(exe) With {
                .UseShellExecute = False,
                .CreateNoWindow = True,
                .RedirectStandardInput = True,
                .RedirectStandardOutput = True,
                .RedirectStandardError = True,
                .StandardOutputEncoding = senzaFirma,
                .StandardErrorEncoding = senzaFirma}

            avvio.ArgumentList.Add("--mcp")
            avvio.ArgumentList.Add("--dati")
            avvio.ArgumentList.Add(dati)

            Dim diario As New StringBuilder()
            Dim processo As Process = Nothing

            Try

                processo = Process.Start(avvio)

                ' Il diario si legge a parte e senza aspettarlo: se si riempisse il suo
                ' buffer mentre noi leggiamo le risposte, i due flussi si bloccherebbero
                ' a vicenda e il collaudo si pianterebbe invece di fallire.
                AddHandler processo.ErrorDataReceived,
                    Sub(mittente As Object, riga As DataReceivedEventArgs)
                        If riga.Data IsNot Nothing Then SyncLock diario : diario.AppendLine(riga.Data) : End SyncLock
                    End Sub
                processo.BeginErrorReadLine()

                ' L'handshake dell'era vecchia, e subito dopo la vetrina.
                Manda(processo, "{""jsonrpc"":""2.0"",""id"":1,""method"":""initialize""," &
                                """params"":{""protocolVersion"":""2025-11-25"",""capabilities"":{}," &
                                """clientInfo"":{""name"":""banco"",""version"":""1.0""}}}")

                Dim apertura As JsonObject = Aspetta(processo, "l'handshake")
                Assert.AreEqual(1, apertura("id").GetValue(Of Integer)(), "risponde alla richiesta giusta")
                Assert.IsNotNull(TryCast(apertura("result"), JsonObject)("serverInfo"), "e dice chi è")

                Manda(processo, "{""jsonrpc"":""2.0"",""method"":""notifications/initialized""}")

                Manda(processo, "{""jsonrpc"":""2.0"",""id"":2,""method"":""tools/list"",""params"":{}}")

                Dim vetrina As JsonObject = Aspetta(processo, "l'elenco dei tool")
                Assert.AreEqual(2, vetrina("id").GetValue(Of Integer)(), "la notifica in mezzo non ha prodotto righe")

                Dim tool As JsonArray = TryCast(TryCast(vetrina("result"), JsonObject)("tools"), JsonArray)
                Assert.AreEqual(12, tool.Count,
                                "i tre di lettura, i sette che passano dall'AI e i due che scrivono")

                ' E adesso lo spegnimento, che è la parte che nessun collaudo di
                ' scrivania può provare: si chiude l'ingresso e si aspetta che esca da sé.
                processo.StandardInput.Close()

                Assert.IsTrue(processo.WaitForExit(Pazienza),
                              "chiuso l'ingresso, il server deve uscire da solo invece di farsi ammazzare")
                Assert.AreEqual(0, processo.ExitCode, "e uscire bene")

                ' Un'attesa in più, e non è una ripetizione: l'attesa con la scadenza qui
                ' sopra dice soltanto che il processo è morto, mentre il diario lo stiamo
                ' leggendo a eventi, e quelli possono essere ancora per strada. Solo
                ' l'attesa *senza* scadenza promette che i lettori asincroni abbiano
                ' consegnato tutto. Senza, il diario si guarda mentre si sta ancora
                ' riempiendo — e il collaudo passa o cade a seconda di quanto è carica la
                ' macchina, che è il modo peggiore di fallire.
                processo.WaitForExit()

                ' Il resoconto del montaggio è finito nel diario, che è l'altra metà del
                ' patto: su stdout solo protocollo, tutto il resto su stderr.
                Assert.AreNotEqual(0, diario.Length, "il diario su stderr non deve restare muto")

            Finally

                If processo IsNot Nothing Then
                    Try
                        If Not processo.HasExited Then processo.Kill(entireProcessTree:=True)
                    Catch ex As InvalidOperationException
                    End Try
                    processo.Dispose()
                End If

                Try
                    If Directory.Exists(dati) Then Directory.Delete(dati, recursive:=True)
                Catch ex As IOException
                Catch ex As UnauthorizedAccessException
                End Try

            End Try

        End Sub

        ''' <summary>Una riga al processo, con l'a capo che fa da cornice al messaggio.</summary>
        Private Shared Sub Manda(processo As Process, messaggio As String)

            processo.StandardInput.Write(messaggio)
            processo.StandardInput.Write(vbLf)
            processo.StandardInput.Flush()

        End Sub

        ''' <summary>
        ''' La prossima riga di <c>stdout</c>, che deve essere un messaggio JSON e
        ''' nient'altro: se ci finisse un saluto, un avviso o una firma UTF-8, il client
        ''' vero si troverebbe davanti una riga che non sa leggere.
        ''' </summary>
        Private Shared Function Aspetta(processo As Process, cosa As String) As JsonObject

            Dim riga As String = processo.StandardOutput.ReadLine()
            Assert.IsNotNull(riga, $"nessuna risposta per {cosa}")

            Dim messaggio As JsonObject = Nothing
            Try
                messaggio = TryCast(JsonNode.Parse(riga), JsonObject)
            Catch ex As Text.Json.JsonException
                Assert.Fail($"su stdout è finita una riga che non è un messaggio MCP: «{riga}»")
            End Try

            Assert.IsNotNull(messaggio, $"su stdout è finita una riga che non è un messaggio MCP: «{riga}»")
            Assert.AreEqual("2.0", messaggio("jsonrpc").GetValue(Of String)(), "e deve essere JSON-RPC 2.0")

            Return messaggio

        End Function

    End Class

End Namespace
