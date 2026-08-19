Imports System.IO
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Channels
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Mcp
Imports TrovaLavoro.Motore

Namespace Mcp

    ''' <summary>
    ''' Collaudi del <b>ciclo</b> del server MCP (cap. 09.2): non cosa risponde — quello
    ''' è <c>CollaudiServerMcp</c> — ma <b>come</b> serve. Le cose da tenere ferme sono
    ''' tre: che più lavori procedano davvero insieme, che un annullamento raggiunga un
    ''' lavoro <i>mentre</i> è in corso, e che chiudere l'ingresso non lasci il processo
    ''' appeso a finire qualcosa che nessuno leggerà.
    ''' </summary>
    ''' <remarks>
    ''' <para>Qui non si misura il tempo e non si dorme mai: un collaudo che aspetta
    ''' «tanto quanto basta» passa sulla macchina di chi lo scrive e fallisce a caso su
    ''' quella di qualcun altro. Il tool di prova si ferma su un segnale ed è il collaudo
    ''' a scegliere quando lasciarlo andare, quindi ogni prova qui sotto è
    ''' <b>deterministica</b>: se una fallisce è perché il ciclo è rotto, non perché il
    ''' portatile stava indicizzando il disco.</para>
    ''' <para>I tool veri non sanno fermarsi su comando, ed è la ragione per cui il
    ''' server accetta una vetrina dal di fuori.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiCicloServerMcp

        Private Const ChiaveFinta As String = "chiave-di-collaudo"

        ''' <summary>Quanto si aspetta prima di dire che il ciclo è piantato.</summary>
        Private Const AttesaMassima As Integer = 15000

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

        Private Shared Function CartellaTemporanea() As String
            Return Path.Combine(Path.GetTempPath(), "ciclo-mcp-" & Guid.NewGuid().ToString("N"))
        End Function

        Private Shared Function Monta(radice As String) As ContestoApp
            Return ContestoApp.Monta(radice, ChiaveFinta, PoolInesistente())
        End Function

#Region "Le prove"

        <TestMethod>
        Public Async Function UnLavoroLungoNonRendeIlServerSordo() As Task
            ' Il cuore della scelta di T8b: mentre un tool macina, il ciclo continua a
            ' servire. Se fosse seriale, la risposta al ping non potrebbe uscire prima di
            ' quella del lavoro fermo — e invece esce, ed è la prova.
            Using banco As Banco = Banco.Apri()

                banco.Manda(ChiamaIlTool(CatalogoConAttesa.Dormi, id:=1))
                banco.Manda(Richiesta("ping", id:=2))

                Assert.AreEqual(2, Await banco.ProssimoId(),
                                "il ping deve rispondere mentre il primo lavoro è ancora fermo")

                banco.Sblocca()
                Assert.AreEqual(1, Await banco.ProssimoId(), "e poi arriva il lavoro lungo")

                Await banco.Chiudi()

            End Using
        End Function

        <TestMethod>
        Public Async Function UnLavoroInCorsoSiAnnullaERestaMuto() As Task
            ' La notifica che a T8a non poteva servire a niente: adesso arriva mentre il
            ' lavoro c'è ancora. E a una richiesta ritirata non si risponde — il client
            ' non aspetta più niente su quell'identificativo.
            Using banco As Banco = Banco.Apri()

                banco.Manda(ChiamaIlTool(CatalogoConAttesa.Dormi, id:=1))
                banco.Manda(Annullamento(1))
                banco.Manda(Richiesta("ping", id:=2))

                Assert.AreEqual(2, Await banco.ProssimoId(),
                                "la sola risposta è quella del ping: il lavoro ritirato tace")

                Await banco.Chiudi()
                Assert.AreEqual(0, banco.RigheRimaste(),
                                "dopo la chiusura non deve essersi affacciata nessuna risposta per il lavoro annullato")

            End Using
        End Function

        <TestMethod>
        Public Async Function UnAnnullamentoInRitardoNonFaDanni() As Task
            ' Il caso normale, non il guasto: il lavoro era già finito quando la notifica
            ' è partita, e le due cose si sono incrociate sulla pipe.
            Using banco As Banco = Banco.Apri()

                banco.Manda(Richiesta("ping", id:=7))
                Assert.AreEqual(7, Await banco.ProssimoId(), "il ping ha già risposto")

                banco.Manda(Annullamento(7))
                banco.Manda(Richiesta("ping", id:=8))

                Assert.AreEqual(8, Await banco.ProssimoId(),
                                "il ciclo prosegue: annullare un lavoro finito non è un guasto")

                Await banco.Chiudi()

            End Using
        End Function

        <TestMethod>
        Public Async Function ChiudereLIngressoNonLasciaIlProcessoAppeso() As Task
            ' Il congedo del client: da lì in poi nessuna risposta ha dove andare, e
            ' restare a macinare sarebbe solo un modo più lento di morire. Il lavoro non
            ' viene mai sbloccato: se il ciclo lo aspettasse, questa prova non finirebbe.
            Using banco As Banco = Banco.Apri()

                banco.Manda(ChiamaIlTool(CatalogoConAttesa.Dormi, id:=1))
                Await banco.LavoroPartito()

                Await banco.Chiudi()

                Assert.AreEqual(0, banco.RigheRimaste(),
                                "il lavoro in volo è stato annullato, non finito")

            End Using
        End Function

        <TestMethod>
        Public Async Function MolteRisposteInsiemeNonSiIntrecciano() As Task
            ' Il lucchetto sull'uscita: la riga è la cornice del messaggio, e due
            ' risposte che escono insieme la spezzerebbero. Ogni riga dev'essere un JSON
            ' intero, e devono essere tante quante le richieste.
            Const quante As Integer = 40

            Using banco As Banco = Banco.Apri()

                For numero As Integer = 1 To quante
                    banco.Manda(Richiesta("tools/list", id:=numero))
                Next

                Dim visti As New HashSet(Of Integer)()

                For numero As Integer = 1 To quante
                    Dim riga As String = Await banco.ProssimaRiga()

                    ' Se il lucchetto non ci fosse, è qui che si vedrebbe: una riga
                    ' spezzata a metà non si lascia nemmeno leggere come JSON.
                    Dim letta As JsonObject = TryCast(JsonNode.Parse(riga), JsonObject)
                    Assert.IsNotNull(letta, $"la riga {numero} dev'essere un JSON intero: «{riga}»")
                    Assert.IsNotNull(letta("result"), "e portare un risultato")

                    Assert.IsTrue(visti.Add(letta("id").GetValue(Of Integer)()),
                                  "ogni identificativo torna una volta sola")
                Next

                Await banco.Chiudi()
                Assert.HasCount(quante, visti, "tante risposte quante richieste")

            End Using
        End Function

#End Region

#Region "I messaggi"

        ''' <summary>
        ''' Una richiesta dell'era moderna, col <c>_meta</c> completo che il protocollo
        ''' pretende su ogni messaggio.
        ''' </summary>
        Private Shared Function Richiesta(metodo As String, id As Integer,
                                          Optional parametri As JsonObject = Nothing) As String

            Dim corpo As JsonObject = If(parametri, New JsonObject())

            corpo("_meta") = New JsonObject From {
                {ProtocolloMcp.ChiaveVersione, ProtocolloMcp.VersioneModerna},
                {ProtocolloMcp.ChiaveCapacitaClient, New JsonObject()}}

            Return New JsonObject From {
                {"jsonrpc", "2.0"},
                {"id", id},
                {"method", metodo},
                {"params", corpo}}.ToJsonString()

        End Function

        Private Shared Function ChiamaIlTool(nome As String, id As Integer) As String

            Return Richiesta("tools/call", id,
                             New JsonObject From {{"name", nome}, {"arguments", New JsonObject()}})

        End Function

        ''' <summary>La notifica con cui il client ritira una richiesta: niente id, quindi nessuna risposta.</summary>
        Private Shared Function Annullamento(idRitirato As Integer) As String

            Return New JsonObject From {
                {"jsonrpc", "2.0"},
                {"method", "notifications/cancelled"},
                {"params", New JsonObject From {{"requestId", idRitirato}}}}.ToJsonString()

        End Function

#End Region

#Region "Il banco"

        ''' <summary>
        ''' Il server acceso su due tubi che il collaudo governa: si manda una riga
        ''' quando si vuole, si legge una risposta quando arriva, si chiude l'ingresso
        ''' quando è ora.
        ''' </summary>
        Private NotInheritable Class Banco
            Implements IDisposable

            Private ReadOnly _contesto As ContestoApp
            Private ReadOnly _catalogo As CatalogoConAttesa
            Private ReadOnly _ingresso As IngressoAPassi
            Private ReadOnly _uscita As UscitaARighe
            Private ReadOnly _ciclo As Task

            Private Sub New(contesto As ContestoApp)

                _contesto = contesto
                _catalogo = New CatalogoConAttesa(contesto)
                _ingresso = New IngressoAPassi()
                _uscita = New UscitaARighe()

                ' Il diario si butta via: qui si guarda l'uscita, e su stderr passa la
                ' diagnostica, che è un'altra domanda.
                _ciclo = New ServerMcp(_catalogo, _ingresso, _uscita, TextWriter.Null).ServiAsync()

            End Sub

            Public Shared Function Apri() As Banco
                Return New Banco(Monta(CartellaTemporanea()))
            End Function

            Public Sub Manda(riga As String)
                _ingresso.Manda(riga)
            End Sub

            ''' <summary>Lascia andare il tool che sta aspettando.</summary>
            Public Sub Sblocca()
                _catalogo.Sblocca()
            End Sub

            ''' <summary>Aspetta che il tool di prova sia davvero entrato in attesa.</summary>
            Public Function LavoroPartito() As Task
                Return ConScadenza(_catalogo.Partito, "il lavoro non è mai partito")
            End Function

            Public Function ProssimaRiga() As Task(Of String)
                Return _uscita.Prossima(AttesaMassima)
            End Function

            ''' <summary>L'identificativo della prossima risposta che esce.</summary>
            Public Async Function ProssimoId() As Task(Of Integer)

                Dim riga As String = Await ProssimaRiga()
                Return JsonNode.Parse(riga)("id").GetValue(Of Integer)()

            End Function

            ''' <summary>
            ''' Chiude l'ingresso e aspetta che il ciclo finisca. Se non finisce, il
            ''' collaudo fallisce qui invece di restare appeso fino al timeout del banco:
            ''' un piantone si legge molto meglio come asserzione che come prova che non
            ''' termina mai.
            ''' </summary>
            Public Async Function Chiudi() As Task

                _ingresso.Chiudi()
                Await ConScadenza(_ciclo, "il ciclo non si è chiuso quando l'ingresso è finito")

            End Function

            ''' <summary>Quante risposte sono uscite e nessuno ha ancora raccolto.</summary>
            Public Function RigheRimaste() As Integer
                Return _uscita.Quante()
            End Function

            Private Shared Async Function ConScadenza(atteso As Task, lamentela As String) As Task

                Dim scaduto As Task = Task.Delay(AttesaMassima)
                If Await Task.WhenAny(atteso, scaduto) Is scaduto Then Assert.Fail(lamentela)

                Await atteso

            End Function

            Public Sub Dispose() Implements IDisposable.Dispose

                _ingresso.Chiudi()
                _catalogo.Sblocca()
                _contesto.Dispose()

            End Sub

        End Class

        ''' <summary>
        ''' Un ingresso che non finisce quando finiscono le righe già scritte, ma quando
        ''' lo dice il collaudo: è la differenza fra provare un ciclo e provare la
        ''' lettura di una stringa.
        ''' </summary>
        Private NotInheritable Class IngressoAPassi
            Inherits TextReader

            Private ReadOnly _canale As Channel(Of String) = Channel.CreateUnbounded(Of String)()

            Public Sub Manda(riga As String)
                _canale.Writer.TryWrite(riga)
            End Sub

            Public Sub Chiudi()
                _canale.Writer.TryComplete()
            End Sub

            Public Overrides Async Function ReadLineAsync() As Task(Of String)

                Try
                    Return Await _canale.Reader.ReadAsync().AsTask().ConfigureAwait(False)
                Catch ex As ChannelClosedException
                    ' Le righe sono finite e non ne arriveranno altre: è lo stdin che si
                    ' chiude, cioè il congedo del client.
                    Return Nothing
                End Try

            End Function

        End Class

        ''' <summary>
        ''' L'uscita raccolta una riga alla volta. Il server scrive il messaggio e poi
        ''' l'a-capo con due chiamate distinte, quindi qui si taglia sui caratteri e non
        ''' si dà per scontato che una scrittura sia una riga.
        ''' </summary>
        Private NotInheritable Class UscitaARighe
            Inherits TextWriter

            Private ReadOnly _righe As Channel(Of String) = Channel.CreateUnbounded(Of String)()
            Private ReadOnly _lucchetto As New Object
            Private ReadOnly _corrente As New StringBuilder()

            Public Overrides ReadOnly Property Encoding As Encoding
                Get
                    Return Encoding.UTF8
                End Get
            End Property

            Public Overrides Sub Write(valore As Char)

                SyncLock _lucchetto

                    If valore = ChrW(10) Then
                        _righe.Writer.TryWrite(_corrente.ToString())
                        _corrente.Clear()
                    Else
                        _corrente.Append(valore)
                    End If

                End SyncLock

            End Sub

            Public Overrides Sub Write(valore As String)

                If valore Is Nothing Then Return

                For Each carattere As Char In valore
                    Write(carattere)
                Next

            End Sub

            ''' <summary>La prossima riga uscita, o un fallimento se non ne esce nessuna.</summary>
            Public Async Function Prossima(attesaMassima As Integer) As Task(Of String)

                Using scadenza As New CancellationTokenSource(attesaMassima)
                    Try
                        Return Await _righe.Reader.ReadAsync(scadenza.Token).AsTask().ConfigureAwait(False)
                    Catch ex As OperationCanceledException
                        Assert.Fail("nessuna risposta è uscita entro l'attesa")
                        Return Nothing
                    End Try
                End Using

            End Function

            ''' <summary>Quante righe sono uscite e nessuno ha raccolto.</summary>
            Public Function Quante() As Integer
                Return _righe.Reader.Count
            End Function

        End Class

        ''' <summary>
        ''' La vetrina vera con in più un tool che si ferma finché non lo si lascia
        ''' andare — e che rispetta il gettone, perché è proprio quel che si vuole
        ''' provare.
        ''' </summary>
        Private NotInheritable Class CatalogoConAttesa
            Inherits CatalogoTool

            Public Const Dormi As String = "dormi"

            Private ReadOnly _sblocco As New TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously)

            Private ReadOnly _partito As New TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously)

            Public Sub New(contesto As ContestoApp)
                MyBase.New(contesto)
            End Sub

            ''' <summary>Si avvera quando il tool è entrato in attesa.</summary>
            Public ReadOnly Property Partito As Task
                Get
                    Return _partito.Task
                End Get
            End Property

            Public Sub Sblocca()
                _sblocco.TrySetResult()
            End Sub

            Public Overrides Function Conosce(nome As String) As Boolean
                Return Dormi.Equals(nome, StringComparison.Ordinal) OrElse MyBase.Conosce(nome)
            End Function

            Public Overrides Async Function EseguiAsync(nome As String, argomenti As JsonObject,
                                                        Optional annulla As CancellationToken = Nothing) _
                                                        As Task(Of EsitoTool)

                If Not Dormi.Equals(nome, StringComparison.Ordinal) Then
                    Return Await MyBase.EseguiAsync(nome, argomenti, annulla).ConfigureAwait(False)
                End If

                _partito.TrySetResult()

                ' Ci si sveglia in due modi: perché il collaudo lascia andare, o perché
                ' il gettone è stato tirato. Il secondo è il caso che interessa.
                Using iscrizione As CancellationTokenRegistration =
                    annulla.Register(Sub() _sblocco.TrySetCanceled(annulla))

                    Await _sblocco.Task.ConfigureAwait(False)
                End Using

                Return EsitoTool.Riuscito(New JsonObject From {{"svegliato", True}})

            End Function

        End Class

#End Region

    End Class

End Namespace
