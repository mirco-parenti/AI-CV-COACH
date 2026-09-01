Imports System.Collections.Concurrent
Imports System.IO
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Mcp

    ''' <summary>
    ''' Il server MCP dell'applicazione (cap. 09): le funzioni di TrovaLavoro offerte a
    ''' un client AI esterno, sullo stesso eseguibile e sugli stessi dati.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Più richieste insieme, e non per fare in fretta.</b> Fino a T8a si
    ''' serviva una richiesta per volta, e andava bene: si leggeva soltanto dal disco.
    ''' Con i tool che passano dall'AI un <c>genera_cv</c> dura minuti, e un ciclo che lo
    ''' aspetta è un ciclo <b>sordo</b> — non sentirebbe nemmeno il «lascia perdere» che
    ''' il client manda proprio mentre quel lavoro è in corso. Il filo che legge non si
    ''' ferma mai: mette da parte il lavoro e torna ad ascoltare.</para>
    ''' <para><b>Un solo scrittore sull'uscita.</b> È la conseguenza immediata: due
    ''' risposte che escono insieme si intreccerebbero a metà riga, e la riga è la
    ''' cornice del messaggio (cap. 09.2). Sull'uscita si va uno alla volta, e si passa
    ''' tutti da <see cref="Consegna"/>.</para>
    ''' <para><b>Niente stato fra una richiesta e l'altra.</b> Non è pigrizia ma la
    ''' regola del protocollo moderno: il processo non è una conversazione, e sulla stessa
    ''' pipe possono passare richieste che non c'entrano nulla fra loro. L'unica cosa che
    ''' il server ricorda è quali lavori ha in corso, e solo per poterli annullare.</para>
    ''' <para><b>Non si esce mai per un errore.</b> Una riga illeggibile, un metodo che
    ''' non conosciamo, un tool che scoppia: tutto diventa una risposta e il ciclo
    ''' continua. Un server che muore alla prima stranezza costringe il client a
    ''' riavviarlo, e chi guarda non capisce perché.</para>
    ''' </remarks>
    Public Class ServerMcp

        ''' <summary>Cosa raccontare di sé a chi arriva: è il posto in cui dire i limiti veri.</summary>
        Private Const Presentazione As String =
            "TrovaLavoro prepara candidature: profilo professionale, analisi di un annuncio, " &
            "confronto con punteggio, CV e lettera su misura. I tool leggono la cartella dati " &
            "dell'applicazione. Il punteggio in stelle è calcolato dal programma e non si può " &
            "negoziare a parole, e nessun testo prodotto inventa esperienze che il profilo non " &
            "dichiara."

        ''' <summary>La notifica con cui il client ritira una richiesta ancora in corso.</summary>
        Private Const NotificaAnnullamento As String = "notifications/cancelled"

        Private ReadOnly _catalogo As CatalogoTool
        Private ReadOnly _ingresso As TextReader
        Private ReadOnly _uscita As TextWriter
        Private ReadOnly _diario As TextWriter

        ''' <summary>Il turno di parola sull'uscita: ci si scrive uno alla volta.</summary>
        Private ReadOnly _lucchettoUscita As New Object

        ''' <summary>
        ''' I lavori in corso, per identificativo: è quel che permette di annullarne uno
        ''' quando il client cambia idea. Ci si entra all'avvio del lavoro e se ne esce
        ''' quando è finito, comunque sia finito.
        ''' </summary>
        Private ReadOnly _inCorso As New ConcurrentDictionary(Of String, CancellationTokenSource)

        ''' <summary>
        ''' I lavori messi da parte, per poterli aspettare quando l'ingresso si chiude.
        ''' Un lavoro esce di qui appena finisce: durante una sessione lunga l'elenco
        ''' resta corto, non cresce quanto le richieste servite.
        ''' </summary>
        Private ReadOnly _lavori As New ConcurrentDictionary(Of Task, Boolean)

        ''' <param name="ingresso">Da dove arrivano i messaggi; nel programma vero, stdin.</param>
        ''' <param name="uscita">Dove vanno le risposte, e <b>nient'altro</b>; nel programma vero, stdout.</param>
        ''' <param name="diario">Dove va tutto il resto; nel programma vero, stderr.</param>
        Public Sub New(contesto As ContestoApp, ingresso As TextReader, uscita As TextWriter, diario As TextWriter)
            Me.New(CatalogoDi(contesto), ingresso, uscita, diario)
        End Sub

        ''' <summary>
        ''' Il server su una vetrina già pronta. È la porta da cui il banco entra: per
        ''' collaudare il <b>ciclo</b> — che più lavori procedano davvero insieme, che un
        ''' annullamento li raggiunga — serve un tool che duri quanto vuole chi collauda,
        ''' e nessuno dei tool veri sa farlo su comando.
        ''' </summary>
        Public Sub New(catalogo As CatalogoTool, ingresso As TextReader, uscita As TextWriter, diario As TextWriter)

            If catalogo Is Nothing Then Throw New ArgumentNullException(NameOf(catalogo))

            _catalogo = catalogo
            _ingresso = ingresso
            _uscita = uscita
            _diario = diario

        End Sub

        ''' <summary>La vetrina di un contesto, col suo controllo: serve prima di <c>Me.New</c>.</summary>
        Private Shared Function CatalogoDi(contesto As ContestoApp) As CatalogoTool

            If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
            Return New CatalogoTool(contesto)

        End Function

        ''' <summary>
        ''' Il server sui flussi veri del processo. Non si passa da <c>Console.In</c> e
        ''' <c>Console.Out</c>: quelli portano la codifica di sistema, e un programma che
        ''' parla italiano si vedrebbe uscire gli accenti rotti dalla prima riga. Qui la
        ''' codifica è UTF-8 <b>senza BOM</b> in tutte e tre le direzioni — tre byte di
        ''' firma in testa alla prima riga basterebbero a rendere illeggibile il primo
        ''' messaggio.
        ''' </summary>
        Public Shared Function SuStdio(contesto As ContestoApp) As ServerMcp

            Dim senzaFirma As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

            Return New ServerMcp(
                contesto,
                New StreamReader(Console.OpenStandardInput(), senzaFirma),
                New StreamWriter(Console.OpenStandardOutput(), senzaFirma),
                New StreamWriter(Console.OpenStandardError(), senzaFirma) With {.AutoFlush = True})

        End Function

        ''' <summary>
        ''' Serve il client finché c'è da servire. Si esce quando l'ingresso si chiude:
        ''' è il modo con cui il client dice «ho finito», ed è l'unico segnale portabile
        ''' che esista (cap. 09.2). Un server che lo ignora si fa terminare a forza.
        ''' </summary>
        ''' <remarks>
        ''' Il filo che legge fa tre cose sole — riconoscere il messaggio, servire subito
        ''' le notifiche, mettere da parte il resto — e nessuna delle tre può durare.
        ''' È la condizione perché un annullamento arrivi mentre c'è ancora qualcosa da
        ''' annullare.
        ''' </remarks>
        Public Async Function ServiAsync() As Task

            Do

                Dim riga As String = Await _ingresso.ReadLineAsync().ConfigureAwait(False)
                If riga Is Nothing Then Exit Do

                Dim richiesta As RichiestaMcp = RichiestaMcp.Leggi(riga)

                ' Una riga che non è JSON non ha nemmeno un identificativo da ricopiare:
                ' la risposta esce senza, ed è il solo caso in cui la spec lo ammette.
                If richiesta Is Nothing Then
                    Consegna(ProtocolloMcp.Compatto(
                        ProtocolloMcp.Errore(Nothing, ProtocolloMcp.ErroreParse, "Parse error")))
                    Continue Do
                End If

                ' Le notifiche si servono qui, nel filo che legge, e non messe da parte
                ' come le richieste: non producono risposta, non fanno lavoro lungo, e
                ' quella che conta — «lascia perdere» — vale solo se arriva *mentre* il
                ' lavoro è in corso. Metterla in fila dietro a ciò che deve fermare
                ' sarebbe il modo più elegante di non fermare niente.
                If richiesta.ENotifica Then
                    ServiLaNotifica(richiesta)
                    Continue Do
                End If

                MettiDaParte(richiesta)

            Loop

            ' L'ingresso chiuso è il congedo del client, e da lì in poi nessuna risposta
            ' ha più dove andare: quel che è in volo si annulla invece di lasciarlo
            ' finire. Il client aspetta un poco e poi termina il processo a forza —
            ' macinare un CV che nessuno leggerà sarebbe solo un modo più lento di
            ' morire, e intanto la chiave dell'utente continua a pagarlo.
            AnnullaTutto()
            Await AspettaChiFinisce().ConfigureAwait(False)

        End Function

        ''' <summary>
        ''' Prende in carico una richiesta senza aspettarla: le dà un gettone di
        ''' annullamento suo, la mette fra i lavori e torna subito al chiamante.
        ''' </summary>
        Private Sub MettiDaParte(richiesta As RichiestaMcp)

            Dim chiave As String = ChiaveDi(richiesta.Id)
            Dim gettone As New CancellationTokenSource()

            ' JSON-RPC vuole identificativi mai ripetuti finché una richiesta è viva. Se
            ' il client ne riusa uno si serve lo stesso — rifiutare un lavoro per un
            ' cavillo sarebbe peggio del difetto — ma solo il primo resta annullabile, e
            ' la stranezza va detta invece di sparire.
            If Not _inCorso.TryAdd(chiave, gettone) Then
                Annota($"Identificativo «{chiave}» già in corso: la richiesta si serve, " &
                       "ma un annullamento raggiungerebbe solo la prima.")
            End If

            Dim lavoro As Task = ServiLaRichiestaAsync(richiesta, chiave, gettone)

            _lavori.TryAdd(lavoro, True)

            ' La rimozione si registra *dopo* l'inserimento: un lavoro già finito non
            ' resta appeso all'elenco, perché la continuazione parte comunque.
            lavoro.ContinueWith(Sub(finito)
                                    Dim ignorato As Boolean
                                    _lavori.TryRemove(finito, ignorato)
                                End Sub, TaskScheduler.Default)

        End Sub

        ''' <summary>
        ''' Il lavoro di una richiesta, dall'inizio alla riga consegnata. Non solleva
        ''' mai: è un lavoro messo da parte, e un'eccezione qui non avrebbe nessuno a
        ''' raccoglierla.
        ''' </summary>
        Private Async Function ServiLaRichiestaAsync(richiesta As RichiestaMcp, chiave As String,
                                                     gettone As CancellationTokenSource) As Task

            Try
                Dim risposta As String = Await RispondiAsync(richiesta, gettone.Token).ConfigureAwait(False)
                If risposta IsNot Nothing Then Consegna(risposta)

            Catch ex As Exception
                Annota($"Errore imprevisto servendo «{richiesta.Metodo}»: {ex}")

                ' Anche il messaggio dell'eccezione esce di qui, e va ripulito come il
                ' diario (v. Annota): un guasto della rete può ristampare l'intestazione
                ' che porta la chiave dell'utente.
                Consegna(ProtocolloMcp.Compatto(
                    ProtocolloMcp.Errore(richiesta.Id, ProtocolloMcp.ErroreInterno,
                                         DiarioTecnico.SenzaSegreti(ex.Message))))

            Finally
                Dim tolto As CancellationTokenSource = Nothing
                _inCorso.TryRemove(chiave, tolto)
                gettone.Dispose()
            End Try

        End Function

        ''' <summary>
        ''' Cosa fare di una notifica. L'unica che cambia qualcosa è l'annullamento; le
        ''' altre si prendono e si tacciono, perché a una notifica non si risponde
        ''' nemmeno per dire che non la si è capita.
        ''' </summary>
        Private Sub ServiLaNotifica(richiesta As RichiestaMcp)

            If Not NotificaAnnullamento.Equals(richiesta.Metodo, StringComparison.Ordinal) Then Return

            Dim id As JsonNode = CampiJson.Nodo(richiesta.Parametri, "requestId")
            If id Is Nothing Then Return

            Dim chiave As String = ChiaveDi(id)
            Dim gettone As CancellationTokenSource = Nothing

            ' Un annullamento che non trova più il suo lavoro è normale, non un guasto:
            ' vuol dire che il lavoro era già finito quando la notifica è partita, e le
            ' due cose si sono incrociate sulla pipe.
            If Not _inCorso.TryGetValue(chiave, gettone) Then
                Annota($"Annullamento per «{chiave}»: quel lavoro è già finito.")
                Return
            End If

            Annulla(gettone)
            Annota($"Annullato il lavoro «{chiave}» su richiesta del client.")

        End Sub

        ''' <summary>Ferma tutti i lavori ancora in corso.</summary>
        Private Sub AnnullaTutto()

            For Each gettone As CancellationTokenSource In _inCorso.Values
                Annulla(gettone)
            Next

        End Sub

        ''' <summary>
        ''' Ferma un lavoro. Il gettone può essere già stato smaltito, se il lavoro è
        ''' finito nell'istante fra il ritrovarlo e il fermarlo: è una gara che si può
        ''' solo perdere con eleganza, non evitare.
        ''' </summary>
        Private Shared Sub Annulla(gettone As CancellationTokenSource)

            Try
                gettone.Cancel()
            Catch ex As ObjectDisposedException
            End Try

        End Sub

        ''' <summary>
        ''' Aspetta che i lavori in volo finiscano. Sono già stati annullati, quindi è
        ''' un'attesa breve; e se uno di loro finisce male non è più il momento di
        ''' occuparsene — si stava chiudendo.
        ''' </summary>
        Private Async Function AspettaChiFinisce() As Task

            Try
                Await Task.WhenAll(_lavori.Keys).ConfigureAwait(False)
            Catch ex As Exception
                Annota($"Chiudendo, un lavoro è finito male: {ex.Message}")
            End Try

        End Function

        ''' <summary>
        ''' L'identificativo come chiave: la sua forma JSON compatta, che tiene distinto
        ''' il numero <c>1</c> dal testo <c>"1"</c> — per JSON-RPC sono due richieste
        ''' diverse, e confonderle vorrebbe dire annullare quella sbagliata.
        ''' </summary>
        Private Shared Function ChiaveDi(id As JsonNode) As String
            Return ProtocolloMcp.Compatto(id)
        End Function

        ''' <summary>
        ''' Una riga sull'uscita, tutta intera. È l'unico punto da cui si scrive, ed è
        ''' quel che rende innocuo il servire più richieste insieme.
        ''' </summary>
        Private Sub Consegna(riga As String)

            SyncLock _lucchettoUscita

                ' A capo scritto a mano, e non con WriteLine: su Windows quello
                ' metterebbe due caratteri, e la riga è la cornice del messaggio.
                _uscita.Write(riga)
                _uscita.Write(vbLf)
                _uscita.Flush()

            End SyncLock

        End Sub

        ''' <summary>
        ''' Una riga in arrivo, la riga da rispondere: <c>Nothing</c> quando non se ne
        ''' deve nessuna, cioè per le notifiche. È il cuore del server, e sta staccato dal
        ''' ciclo apposta — così il banco può interrogarlo senza avviare un processo.
        ''' </summary>
        Public Function RispondiAsync(riga As String) As Task(Of String)

            Dim richiesta As RichiestaMcp = RichiestaMcp.Leggi(riga)

            ' Una riga che non è JSON non ha nemmeno un identificativo da ricopiare: la
            ' risposta esce senza, ed è il solo caso in cui la spec lo ammette.
            If richiesta Is Nothing Then
                Return Task.FromResult(ProtocolloMcp.Compatto(
                    ProtocolloMcp.Errore(Nothing, ProtocolloMcp.ErroreParse, "Parse error")))
            End If

            Return RispondiAsync(richiesta, CancellationToken.None)

        End Function

        ''' <summary>
        ''' La risposta a una richiesta già riconosciuta, col gettone che permette di
        ''' fermarla per strada.
        ''' </summary>
        Public Async Function RispondiAsync(richiesta As RichiestaMcp, annulla As CancellationToken) As Task(Of String)

            If richiesta Is Nothing Then Throw New ArgumentNullException(NameOf(richiesta))

            ' A una notifica non si risponde, nemmeno per dire che non l'abbiamo capita.
            If richiesta.ENotifica Then Return Nothing

            If String.IsNullOrWhiteSpace(richiesta.Metodo) Then
                Return Scrivi(ProtocolloMcp.Errore(richiesta.Id, ProtocolloMcp.ErroreRichiestaNonValida,
                                                   "Invalid Request: manca il metodo."))
            End If

            Dim rifiuto As JsonObject = ControllaIlProtocollo(richiesta)
            If rifiuto IsNot Nothing Then Return Scrivi(rifiuto)

            Try
                Return Scrivi(Await SmistaAsync(richiesta, annulla).ConfigureAwait(False))

            Catch ex As OperationCanceledException When annulla.IsCancellationRequested
                ' Chi ha annullato non aspetta più niente su quell'identificativo, e la
                ' spec è netta: a una richiesta ritirata non si risponde. Il silenzio qui
                ' è la risposta giusta — mandare un errore vorrebbe dire far arrivare al
                ' client la notizia di un guasto che ha causato lui apposta.
                Annota($"«{richiesta.Metodo}» interrotto: nessuna risposta, era stato ritirato.")
                Return Nothing

            Catch ex As Exception
                ' L'ultima rete, gemella di quella della finestra principale: un guasto
                ' che nessuno aveva previsto diventa una risposta, non la morte del
                ' processo. Il dettaglio va nel diario, che il client raccoglie in un file,
                ' e il messaggio che torna esce ripulito come lui.
                Annota($"Errore imprevisto su «{richiesta.Metodo}»: {ex}")
                Return Scrivi(ProtocolloMcp.Errore(richiesta.Id, ProtocolloMcp.ErroreInterno,
                                                   DiarioTecnico.SenzaSegreti(ex.Message)))
            End Try

        End Function

        ''' <summary>
        ''' Quel che va verificato prima di guardare il metodo, e vale solo per l'era
        ''' moderna: lì la versione e le capacità del client viaggiano su <b>ogni</b>
        ''' richiesta, e mancarle è una richiesta malformata. Nell'era legacy queste due
        ''' cose si sono dette una volta nell'handshake, e chiederle di nuovo sarebbe
        ''' pretendere qualcosa che il protocollo di allora non prevede.
        ''' </summary>
        ''' <returns>L'errore da mandare, o <c>Nothing</c> se si può proseguire.</returns>
        Private Shared Function ControllaIlProtocollo(richiesta As RichiestaMcp) As JsonObject

            If richiesta.Era <> EraMcp.Moderna Then Return Nothing

            If Not ProtocolloMcp.VersioneModerna.Equals(richiesta.VersioneDichiarata, StringComparison.Ordinal) Then
                Return ProtocolloMcp.ErroreDiVersione(richiesta.Id, richiesta.VersioneDichiarata)
            End If

            If Not richiesta.CapacitaDichiarate Then
                Return ProtocolloMcp.Errore(
                    richiesta.Id, ProtocolloMcp.ErroreParametriNonValidi,
                    $"Invalid params: manca «{ProtocolloMcp.ChiaveCapacitaClient}» in _meta.")
            End If

            Return Nothing

        End Function

        ''' <summary>Dal metodo alla risposta.</summary>
        Private Async Function SmistaAsync(richiesta As RichiestaMcp, annulla As CancellationToken) As Task(Of JsonObject)

            Select Case richiesta.Metodo

                Case "initialize"
                    ' Chi apre così parla la lingua vecchia per definizione, qualunque
                    ' cosa dica il resto del messaggio.
                    Return ProtocolloMcp.Risposta(richiesta.Id, Handshake(richiesta), EraMcp.Legacy)

                Case "server/discover"
                    Return ProtocolloMcp.Risposta(richiesta.Id, Scoperta(), richiesta.Era)

                Case "ping"
                    Return ProtocolloMcp.Risposta(richiesta.Id, New JsonObject(), richiesta.Era)

                Case "tools/list"
                    Return ProtocolloMcp.Risposta(
                        richiesta.Id, New JsonObject From {{"tools", _catalogo.Elenco()}}, richiesta.Era)

                Case "tools/call"
                    Return Await ChiamaAsync(richiesta, annulla).ConfigureAwait(False)

                Case Else
                    Return ProtocolloMcp.Errore(richiesta.Id, ProtocolloMcp.ErroreMetodoIgnoto,
                                                $"Method not found: {richiesta.Metodo}")

            End Select

        End Function

        ''' <summary>
        ''' La risposta all'<c>initialize</c> dell'era legacy. Sulla versione si segue la
        ''' regola di allora: se quella chiesta la sappiamo parlare si risponde con la
        ''' stessa, altrimenti con la più recente che conosciamo — spostare un client su
        ''' un'altra revisione senza motivo lo costringerebbe a decidere se restare.
        ''' </summary>
        Private Shared Function Handshake(richiesta As RichiestaMcp) As JsonObject

            Dim chiesta As String = CampiJson.Testo(richiesta.Parametri, "protocolVersion")

            Return New JsonObject From {
                {"protocolVersion", If(ProtocolloMcp.LegacyConosciuta(chiesta), chiesta.Trim(),
                                       ProtocolloMcp.VersioneLegacy)},
                {"capabilities", Capacita()},
                {"serverInfo", ProtocolloMcp.InfoServer()},
                {"instructions", Presentazione}}

        End Function

        ''' <summary>
        ''' La risposta a <c>server/discover</c>: chi siamo, che versioni parliamo, cosa
        ''' sappiamo fare. È il primo messaggio che un client dual-era manda per capire
        ''' con che era ha a che fare, quindi è anche il posto in cui si dichiara di
        ''' parlarle tutte e due.
        ''' </summary>
        Private Shared Function Scoperta() As JsonObject

            Return New JsonObject From {
                {"supportedVersions", ProtocolloMcp.VersioniSupportate()},
                {"capabilities", Capacita()},
                {"instructions", Presentazione},
                {"_meta", New JsonObject From {{ProtocolloMcp.ChiaveInfoServer, ProtocolloMcp.InfoServer()}}}}

        End Function

        ''' <summary>
        ''' Cosa sa fare il server. Solo tool: niente risorse, niente prompt, e
        ''' <c>listChanged</c> non si dichiara perché l'elenco dei tool non cambia mai
        ''' mentre il processo è vivo — prometterlo vorrebbe dire un avviso che non
        ''' arriverà.
        ''' </summary>
        Private Shared Function Capacita() As JsonObject
            Return New JsonObject From {{"tools", New JsonObject()}}
        End Function

        ''' <summary>La chiamata di un tool.</summary>
        Private Async Function ChiamaAsync(richiesta As RichiestaMcp, annulla As CancellationToken) As Task(Of JsonObject)

            Dim nome As String = CampiJson.Testo(richiesta.Parametri, "name")

            ' Un tool che non esiste è un errore di protocollo e non un tool che
            ' fallisce: chi ha chiamato ha sbagliato la richiesta, non i parametri, e
            ' rispondergli «riprova» non lo aiuterebbe (cap. 09.2).
            If Not _catalogo.Conosce(nome) Then
                Return ProtocolloMcp.Errore(richiesta.Id, ProtocolloMcp.ErroreParametriNonValidi,
                                            $"Unknown tool: {If(nome, "(nessun nome)")}")
            End If

            Dim argomenti As JsonObject = TryCast(CampiJson.Nodo(richiesta.Parametri, "arguments"), JsonObject)
            Dim esito As EsitoTool = Await _catalogo.EseguiAsync(nome, argomenti, annulla).ConfigureAwait(False)

            Return ProtocolloMcp.Risposta(richiesta.Id, esito.ComeRisultato(), richiesta.Era)

        End Function

        ''' <summary>
        ''' Una riga per il diario: <b>mai</b> sull'uscita, dove passa solo il protocollo.
        ''' Se anche il diario non si lascia scrivere si tira dritto — un server che muore
        ''' per non aver potuto annotare qualcosa sarebbe assurdo.
        ''' </summary>
        ''' <remarks>
        ''' Il turno di parola vale anche qui: adesso che più lavori procedono insieme, due
        ''' annotazioni possono capitare nello stesso istante, e un diario intrecciato a
        ''' metà riga è precisamente ciò che non si riesce più a leggere quando serve.
        ''' </remarks>
        Public Sub Annota(riga As String)

            If _diario Is Nothing Then Return

            Try
                SyncLock _diario
                    ' La stessa rete del diario su file (cap. 11.3): questo diario esce di
                    ' qui — il client MCP lo raccoglie nei suoi log — e un'eccezione può
                    ' portarsi dietro una chiave (un'intestazione HTTP ristampata, per
                    ' dire). Ripulirla prima di scriverla è la condizione perché possa uscire.
                    _diario.WriteLine(DiarioTecnico.SenzaSegreti(riga))
                End SyncLock
            Catch ex As IOException
            Catch ex As ObjectDisposedException
            End Try

        End Sub

        Private Shared Function Scrivi(messaggio As JsonObject) As String
            Return ProtocolloMcp.Compatto(messaggio)
        End Function

    End Class

End Namespace
