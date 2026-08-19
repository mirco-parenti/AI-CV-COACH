Imports System.IO
Imports System.Text
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Mcp

    ''' <summary>
    ''' Il server MCP dell'applicazione (cap. 09): le funzioni di TrovaLavoro offerte a
    ''' un client AI esterno, sullo stesso eseguibile e sugli stessi dati.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Una richiesta per volta, nell'ordine in cui arrivano.</b> Il protocollo
    ''' permetterebbe al client di mandarne più d'una senza aspettare, ma qui si legge
    ''' soltanto dal disco e la differenza non si vedrebbe. Il conto arriverà a T8b, dove
    ''' un tool può durare minuti: allora varrà la pena rivederlo, e non prima —
    ''' complicare adesso il ciclo per un problema che non abbiamo è il modo classico di
    ''' sbagliarlo.</para>
    ''' <para><b>Niente stato fra una richiesta e l'altra.</b> Non è pigrizia ma la
    ''' regola del protocollo moderno: il processo non è una conversazione, e sulla stessa
    ''' pipe possono passare richieste che non c'entrano nulla fra loro.</para>
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

        Private ReadOnly _catalogo As CatalogoTool
        Private ReadOnly _ingresso As TextReader
        Private ReadOnly _uscita As TextWriter
        Private ReadOnly _diario As TextWriter

        ''' <param name="ingresso">Da dove arrivano i messaggi; nel programma vero, stdin.</param>
        ''' <param name="uscita">Dove vanno le risposte, e <b>nient'altro</b>; nel programma vero, stdout.</param>
        ''' <param name="diario">Dove va tutto il resto; nel programma vero, stderr.</param>
        Public Sub New(contesto As ContestoApp, ingresso As TextReader, uscita As TextWriter, diario As TextWriter)

            If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))

            _catalogo = New CatalogoTool(contesto)
            _ingresso = ingresso
            _uscita = uscita
            _diario = diario

        End Sub

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
        Public Sub Servi()

            Do

                Dim riga As String = _ingresso.ReadLine()
                If riga Is Nothing Then Exit Do

                Dim risposta As String

                Try
                    risposta = Rispondi(riga)

                Catch ex As Exception
                    ' Rispondi ha già la sua rete attorno al lavoro vero: questa è quella
                    ' attorno alla rete, e copre ciò che sta prima — leggere il messaggio,
                    ' capire in che era è scritto. Senza, il ciclo si porterebbe dietro una
                    ' promessa che non mantiene, perché basterebbe una riga malformata in
                    ' un modo che non avevamo previsto per far morire il processo.
                    Annota($"Errore imprevisto prima di poter rispondere: {ex}")
                    risposta = ProtocolloMcp.Compatto(
                        ProtocolloMcp.Errore(Nothing, ProtocolloMcp.ErroreInterno, ex.Message))
                End Try

                If risposta Is Nothing Then Continue Do

                ' A capo scritto a mano, e non con WriteLine: su Windows quello
                ' metterebbe due caratteri, e la riga è la cornice del messaggio.
                _uscita.Write(risposta)
                _uscita.Write(vbLf)
                _uscita.Flush()

            Loop

        End Sub

        ''' <summary>
        ''' Una riga in arrivo, la riga da rispondere: <c>Nothing</c> quando non se ne
        ''' deve nessuna, cioè per le notifiche. È il cuore del server, e sta staccato dal
        ''' ciclo apposta — così il banco può interrogarlo senza avviare un processo.
        ''' </summary>
        Public Function Rispondi(riga As String) As String

            Dim richiesta As RichiestaMcp = RichiestaMcp.Leggi(riga)

            ' Una riga che non è JSON non ha nemmeno un identificativo da ricopiare: la
            ' risposta esce senza, ed è il solo caso in cui la spec lo ammette.
            If richiesta Is Nothing Then
                Return ProtocolloMcp.Compatto(
                    ProtocolloMcp.Errore(Nothing, ProtocolloMcp.ErroreParse, "Parse error"))
            End If

            If richiesta.ENotifica Then
                ' A una notifica non si risponde, nemmeno per dire che non l'abbiamo
                ' capita. Le due che ci aspettiamo — «sono pronto» e «lascia perdere
                ' quella richiesta» — non chiedono niente: la prima chiude un handshake
                ' che per noi non apre nessuna sessione, e la seconda arriverebbe quando
                ' il lavoro è già finito, perché qui si legge solo dal disco.
                Return Nothing
            End If

            If String.IsNullOrWhiteSpace(richiesta.Metodo) Then
                Return Scrivi(ProtocolloMcp.Errore(richiesta.Id, ProtocolloMcp.ErroreRichiestaNonValida,
                                                   "Invalid Request: manca il metodo."))
            End If

            Dim rifiuto As JsonObject = ControllaIlProtocollo(richiesta)
            If rifiuto IsNot Nothing Then Return Scrivi(rifiuto)

            Try
                Return Scrivi(Smista(richiesta))

            Catch ex As Exception
                ' L'ultima rete, gemella di quella della finestra principale: un guasto
                ' che nessuno aveva previsto diventa una risposta, non la morte del
                ' processo. Il dettaglio va nel diario, che il client raccoglie in un file.
                Annota($"Errore imprevisto su «{richiesta.Metodo}»: {ex}")
                Return Scrivi(ProtocolloMcp.Errore(richiesta.Id, ProtocolloMcp.ErroreInterno, ex.Message))
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
        Private Function Smista(richiesta As RichiestaMcp) As JsonObject

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
                    Return Chiama(richiesta)

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
        Private Function Chiama(richiesta As RichiestaMcp) As JsonObject

            Dim nome As String = CampiJson.Testo(richiesta.Parametri, "name")

            ' Un tool che non esiste è un errore di protocollo e non un tool che
            ' fallisce: chi ha chiamato ha sbagliato la richiesta, non i parametri, e
            ' rispondergli «riprova» non lo aiuterebbe (cap. 09.2).
            If Not _catalogo.Conosce(nome) Then
                Return ProtocolloMcp.Errore(richiesta.Id, ProtocolloMcp.ErroreParametriNonValidi,
                                            $"Unknown tool: {If(nome, "(nessun nome)")}")
            End If

            Dim argomenti As JsonObject = TryCast(CampiJson.Nodo(richiesta.Parametri, "arguments"), JsonObject)
            Dim esito As EsitoTool = _catalogo.Esegui(nome, argomenti)

            Return ProtocolloMcp.Risposta(richiesta.Id, esito.ComeRisultato(), richiesta.Era)

        End Function

        ''' <summary>
        ''' Una riga per il diario: <b>mai</b> sull'uscita, dove passa solo il protocollo.
        ''' Se anche il diario non si lascia scrivere si tira dritto — un server che muore
        ''' per non aver potuto annotare qualcosa sarebbe assurdo.
        ''' </summary>
        Public Sub Annota(riga As String)

            If _diario Is Nothing Then Return

            Try
                _diario.WriteLine(riga)
            Catch ex As IOException
            Catch ex As ObjectDisposedException
            End Try

        End Sub

        Private Shared Function Scrivi(messaggio As JsonObject) As String
            Return ProtocolloMcp.Compatto(messaggio)
        End Function

    End Class

End Namespace
