Imports System.Drawing
Imports System.Linq
Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.Web.WebView2.WinForms
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore
Imports TrovaLavoro.Web

Namespace Web

    ''' <summary>
    ''' Collaudi del motore del browser (cap. 06.1): l'unico ambiente WebView2
    ''' dell'applicazione, quello che serve sia alla stampa dei PDF sia al browser
    ''' integrato del pannello Ricerca.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Qui vive il cancello di T5a</b>, che era una prova a mano fuori dal repo
    ''' (2026-08-12) e da qui in poi è un collaudo: <i>browser in vista e stampa fuori
    ''' schermo convivono su un ambiente solo</i>. La prova a mano aveva anche misurato
    ''' dov'è il muro — due ambienti sulla stessa cartella con opzioni diverse danno un
    ''' <c>COMException: Class not registered</c> quando una vista si accende, non quando
    ''' l'ambiente nasce — e quel caso <b>non</b> si riproduce qui: il collaudo verifica la
    ''' misura che lo rende impossibile, cioè che di ambiente ce ne sia uno solo.</para>
    ''' <para>Come <c>CollaudiStampaPdf</c>, i collaudi che accendono il motore stanno
    ''' nella categoria «Reale»: non chiamano l'AI e non spendono un token, ma vogliono la
    ''' <b>macchina</b> — WebView2 e un thread STA — che la batteria di tutti i giorni non
    ''' deve pretendere. Quello che si accontenta di controllare l'ingresso gira sempre.</para>
    ''' <para><b>Cosa qui non si collauda, e perché.</b> Il ramo d'errore
    ''' dell'accensione — <c>ErroreBrowser</c> e il riprovare invece di tenersi il guasto —
    ''' resta senza collaudo: <c>CoreWebView2Environment.CreateAsync</c> non si è fatta
    ''' fallire in nessuno dei tre modi provati il 2026-08-12 (cartella occupata da un file
    ''' con quel nome, percorso con i due punti in mezzo, cartella su un'unità che non
    ''' esiste). Non è una svista: è la scoperta che quel guasto, nella vita vera, non
    ''' arriva accendendo l'ambiente ma accendendo una <b>vista</b>, ed è lì che il pannello
    ''' Ricerca dovrà aspettarlo.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiMotoreBrowser

        ''' <summary>Quanto è grande la finestra che non si vede.</summary>
        Private Shared ReadOnly Area As New Size(800, 600)

        ''' <summary>Dove nasce: fuori da qualunque schermo, come fa la stampante.</summary>
        Private Shared ReadOnly FuoriSchermo As New Point(-32000, -32000)

        ' ==================================================================
        ' L'ingresso: gira senza macchina
        ' ==================================================================

        <TestMethod>
        Public Sub SenzaCartellaDiNavigazioneNonNasce()

            ' Il motore non accende niente alla nascita, ma la cartella la pretende
            ' subito: un ambiente senza casa perderebbe i login a ogni avvio.
            For Each vuota As String In {Nothing, "", "   "}
                Assert.Throws(Of ArgumentException)(
                    Sub()
                        Dim ignorato As New MotoreBrowser(vuota)
                    End Sub,
                    "una cartella di navigazione vuota non è una cartella")
            Next

        End Sub

        ' ==================================================================
        ' Il motore vero (categoria «Reale»: vuole WebView2 e un thread STA)
        ' ==================================================================

        <TestMethod, TestCategory("Reale")>
        Public Sub LAmbienteSiAccendeUnaVoltaSola()

            ConCartellaTemporanea(
                Sub(cartella)

                    Dim primo As CoreWebView2Environment = Nothing
                    Dim secondo As CoreWebView2Environment = Nothing

                    FiloGrafico.Esegui(
                        Async Function() As Task
                            Dim motore As New MotoreBrowser(Path.Combine(cartella, "webview2"))

                            primo = Await motore.AmbienteAsync()
                            secondo = Await motore.AmbienteAsync()
                        End Function)

                    Assert.IsNotNull(primo, "l'ambiente non si è acceso")

                    ' È la misura che rende impossibile la divergenza di opzioni: chi
                    ' chiede l'ambiente una seconda volta riceve quello di prima.
                    Assert.AreSame(primo, secondo,
                                   "il motore ha acceso due ambienti sulla stessa cartella")

                End Sub)

        End Sub

        <TestMethod, TestCategory("Reale")>
        Public Sub IlBrowserInVistaELaStampaConvivonoSulloStessoMotore()

            ConCartellaTemporanea(
                Sub(cartella)

                    Dim documento As String = Path.Combine(cartella, "CV.pdf")
                    Dim navigaAncora As Boolean = False

                    FiloGrafico.Esegui(
                        Async Function() As Task

                            Dim motore As New MotoreBrowser(Path.Combine(cartella, "webview2"))

                            ' Il browser del pannello Ricerca: una WebView dentro una
                            ' finestra vera, accesa sull'ambiente del motore.
                            Using finestra As Form = FinestraDiProva(),
                                  vista As New WebView2 With {.Dock = DockStyle.Fill}

                                finestra.Controls.Add(vista)
                                finestra.Show()

                                Await vista.EnsureCoreWebView2Async(Await motore.AmbienteAsync())
                                Await NavigaAsync(vista, "prima della stampa")

                                ' E nel frattempo la stampante, sullo stesso motore.
                                Using stampante As New StampantePdf(motore)
                                    Await stampante.StampaAsync(
                                        Impaginazione.PaginaCv(CvDiProva()), documento)
                                End Using

                                ' La domanda che il cancello poneva: dopo la stampa il
                                ' browser è ancora vivo? Un motore che muore alla prima
                                ' esportazione renderebbe P3 inservibile.
                                Await NavigaAsync(vista, "dopo la stampa")
                                navigaAncora = True

                            End Using

                        End Function)

                    Assert.IsTrue(File.Exists(documento),
                                  "la stampa non ha prodotto il PDF")
                    Assert.IsTrue(navigaAncora,
                                  "il browser in vista non ha più navigato dopo la stampa")

                End Sub)

        End Sub

        ' ==================================================================
        ' Attrezzi
        ' ==================================================================

        Private Shared Function FinestraDiProva() As Form

            ' Mostrata davvero, ma fuori da ogni schermo: il collaudo non deve far
            ' comparire finestre a chi lancia il banco.
            Return New Form With {
                .FormBorderStyle = FormBorderStyle.None,
                .ShowInTaskbar = False,
                .StartPosition = FormStartPosition.Manual,
                .Location = FuoriSchermo,
                .Size = Area}

        End Function

        Private Shared Async Function NavigaAsync(vista As WebView2, quando As String) As Task

            Dim caricata As New TaskCompletionSource(Of Boolean)

            Dim aFineCaricamento As EventHandler(Of CoreWebView2NavigationCompletedEventArgs) = Nothing
            aFineCaricamento =
                Sub(mittente As Object, argomenti As CoreWebView2NavigationCompletedEventArgs)
                    RemoveHandler vista.NavigationCompleted, aFineCaricamento
                    caricata.TrySetResult(argomenti.IsSuccess)
                End Sub

            AddHandler vista.NavigationCompleted, aFineCaricamento

            ' Nessuna rete: la pagina è una stringa, come per la stampa (cap. 05.5).
            vista.NavigateToString(
                "<!doctype html><html><head><meta charset=""utf-8""></head>" &
                $"<body><p>Città, perù: {quando}</p></body></html>")

            Assert.IsTrue(Await caricata.Task, $"la pagina «{quando}» non si è caricata")

        End Function

        Private Shared Function CvDiProva() As JsonNode

            Return JsonNode.Parse("
                {
                  ""tipo"": ""cv_base"",
                  ""intestazione"": { ""nome"": ""Luca Ferrarì"", ""citta"": ""Modena"" },
                  ""sommario"": ""Ho esperienza nel servizio di sala."",
                  ""esperienze_professionali"": [],
                  ""altre_esperienze"": [],
                  ""competenze"": [""Servizio ai tavoli""],
                  ""formazione"": []
                }")

        End Function

        ''' <summary>Una cartella temporanea che si porta via tutto alla fine.</summary>
        Private Shared Sub ConCartellaTemporanea(prova As Action(Of String))

            Dim cartella As String = Path.Combine(Path.GetTempPath(),
                                                  "browser-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(cartella)

            Try
                prova(cartella)
            Finally
                PortaVia(cartella)
            End Try

        End Sub

        ''' <summary>
        ''' Cancella la cartella, con pazienza: il motore del browser chiude i suoi
        ''' processi <b>dopo</b> che le viste sono state smesse, e finché non ha finito
        ''' tiene il proprio <c>lockfile</c>. Se resiste si lascia dov'è — è la cartella
        ''' temporanea di Windows, e un collaudo non deve fallire per le pulizie.
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

    End Class

End Namespace
