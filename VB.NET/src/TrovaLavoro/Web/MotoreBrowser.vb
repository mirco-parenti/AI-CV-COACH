Imports System.Threading.Tasks
Imports Microsoft.Web.WebView2.Core

Namespace Web

    ''' <summary>
    ''' Il motore del browser non si è potuto accendere. Come <c>ErroreStampa</c> e
    ''' <c>ErroreImport</c>, il messaggio è già leggibile — ma qui <b>non</b> è ancora
    ''' quello da mostrare all'utente: chi chiama sa a che serviva il browser, e il
    ''' ripiego onesto («il DOCX c'è comunque», «la ricerca resta spenta») lo conosce
    ''' solo lui.
    ''' </summary>
    Public Class ErroreBrowser
        Inherits Exception

        ''' <summary>
        ''' Se manca il componente WebView2 di Windows (cap. 13.3): è l'unico caso in cui
        ''' il rimedio riguarda la macchina e non il programma, e va detto all'utente in
        ''' modo diverso da un guasto qualunque.
        ''' </summary>
        Public ReadOnly Property MotoreAssente As Boolean

        Public Sub New(messaggio As String, motoreAssente As Boolean, Optional dentro As Exception = Nothing)
            MyBase.New(messaggio, dentro)
            Me.MotoreAssente = motoreAssente
        End Sub

    End Class

    ''' <summary>
    ''' L'unico ambiente WebView2 dell'applicazione (cap. 06.1, cap. 11.1): lo chiedono
    ''' la stampa PDF, che lavora fuori schermo (cap. 05.5), e il browser integrato del
    ''' pannello Ricerca, che sta in piena vista. Un ambiente solo, la stessa cartella di
    ''' navigazione per tutti.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché uno solo, provato al cancello di T5a (2026-08-12).</b> Due ambienti
    ''' sulla stessa cartella dati <i>convivono</i>, finché hanno le stesse opzioni; se le
    ''' opzioni divergono, la <c>CreateAsync</c> del secondo riesce comunque e il guasto
    ''' arriva dopo, quando una vista prova ad accendersi — con un
    ''' <c>COMException: Class not registered</c>, che a chi legge non dice niente. La
    ''' misura non è quindi «funziona con due»: è che con un ambiente solo quella
    ''' divergenza diventa <b>impossibile</b> invece che possibile e indecifrabile.</para>
    ''' <para><b>Si accende alla prima richiesta, non alla nascita.</b> Serve perché
    ''' l'ambiente vuole essere creato in modo asincrono, e perché la cartella
    ''' <c>webview2\</c> non deve comparire in casa dell'utente per un'applicazione aperta
    ''' e richiusa senza stampare né cercare niente (v. <c>CartellaDati.Assicura</c>).</para>
    ''' <para><b>Va usato dal thread dell'interfaccia</b>, come le viste che lo useranno
    ''' (v. <c>StampantePdf</c>): niente qui è pensato per essere chiamato da più thread.
    ''' Sul thread dell'interfaccia, però, due richieste possono intrecciarsi comunque —
    ''' l'utente che apre la Ricerca mentre una stampa è in volo — e per questo si tiene da
    ''' parte il <b>compito</b> dell'accensione e non il suo esito: chi arriva secondo
    ''' aspetta la stessa accensione invece di avviarne un'altra.</para>
    ''' <para>Non ha nulla da smaltire: l'ambiente non è <c>IDisposable</c>, e i processi
    ''' del browser vivono quanto le viste che lo usano — sono loro a chiudersi.</para>
    ''' <para><b>Il ramo d'errore è di scorta, e va detto</b> <i>(2026-08-12)</i>: la
    ''' creazione dell'ambiente non si è fatta fallire in nessuno dei modi provati — una
    ''' cartella occupata da un file con quel nome, un percorso con i due punti in mezzo,
    ''' una cartella su un'unità che non esiste. <c>CreateAsync</c> se le prende tutte e
    ''' rimanda il conto a quando una <b>vista</b> si accende. Il ramo resta perché costa
    ''' quattro righe e il giorno che scatterà sarà il giorno peggiore, ma nessun collaudo
    ''' lo attraversa: chi lo tocca lo sappia.</para>
    ''' </remarks>
    Public Class MotoreBrowser

        Private ReadOnly _cartellaNavigazione As String

        ''' <summary>L'accensione in corso o già finita bene; <c>Nothing</c> se non è mai partita.</summary>
        Private _accensione As Task(Of CoreWebView2Environment)

        ''' <param name="cartellaNavigazione">
        ''' Dove il motore tiene cookie, sessioni e cache (cap. 11.1, <c>webview2\</c>): è
        ''' ciò che fa sopravvivere i login ai portali fra un avvio e l'altro (cap. 06.6).
        ''' </param>
        Public Sub New(cartellaNavigazione As String)

            If String.IsNullOrWhiteSpace(cartellaNavigazione) Then
                Throw New ArgumentException("Manca la cartella di navigazione.", NameOf(cartellaNavigazione))
            End If

            _cartellaNavigazione = cartellaNavigazione

        End Sub

        ''' <summary>
        ''' L'ambiente da passare a una <c>WebView2</c>, accendendolo se è la prima volta.
        ''' Solleva <see cref="ErroreBrowser"/> se non si è potuto accendere.
        ''' </summary>
        Public Async Function AmbienteAsync() As Task(Of CoreWebView2Environment)

            If _accensione Is Nothing Then _accensione = AccendiAsync()

            Try
                Return Await _accensione.ConfigureAwait(True)

            Catch
                ' Un'accensione fallita non si tiene da parte: resterebbe a rispondere
                ' con lo stesso guasto per tutta la sessione, anche quando la causa è
                ' passata. La prossima richiesta riprova da zero.
                _accensione = Nothing
                Throw

            End Try

        End Function

        Private Async Function AccendiAsync() As Task(Of CoreWebView2Environment)

            Try
                Return Await CoreWebView2Environment.
                    CreateAsync(userDataFolder:=_cartellaNavigazione).ConfigureAwait(True)

            Catch ex As WebView2RuntimeNotFoundException
                ' Di serie in Windows 11, ma su una macchina amministrata può mancare.
                Throw New ErroreBrowser(
                    "Su questo computer manca il componente WebView2 di Windows.",
                    motoreAssente:=True, dentro:=ex)

            Catch ex As Exception
                ' Cartella non scrivibile, motore che non risponde: il chiamante non deve
                ' conoscere i tipi del pacchetto WebView2 per accorgersene.
                Throw New ErroreBrowser(
                    $"Il motore del browser non si è avviato: {ex.Message}",
                    motoreAssente:=False, dentro:=ex)

            End Try

        End Function

    End Class

End Namespace
