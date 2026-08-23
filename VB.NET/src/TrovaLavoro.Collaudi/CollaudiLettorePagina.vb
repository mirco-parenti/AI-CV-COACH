Imports System.Drawing
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.Web.WebView2.WinForms
Imports TrovaLavoro.Motore
Imports TrovaLavoro.Web

Namespace Web

    ''' <summary>
    ''' Collaudi del lettore della pagina (cap. 06.4): la riga di JavaScript che porta
    ''' fuori dalla WebView titolo, indirizzo e testo visibile.
    ''' </summary>
    ''' <remarks>
    ''' <para>Sono nella categoria <b>«Reale»</b> come quelli del motore: non chiamano l'AI
    ''' e non spendono un token, ma vogliono la <b>macchina</b> — WebView2 e un thread STA.
    ''' Non vogliono però la <b>rete</b>: la pagina è scritta qui e caricata con
    ''' <c>NavigateToString</c>, come fa la stampa dei PDF (cap. 05.5). Un collaudo che
    ''' aprisse un portale vero misurerebbe il portale, non il lettore, e fallirebbe il
    ''' giorno che quello cambia una classe CSS.</para>
    ''' <para>Le decisioni che stanno <i>attorno</i> alla lettura — il testo basta? da che
    ''' portale viene? cosa si racconta? — non sono qui: si provano in
    ''' <c>CollaudiPannelloRicerca</c> col lettore finto, e girano sempre.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiLettorePagina

        Private Shared ReadOnly Area As New Size(800, 600)
        Private Shared ReadOnly FuoriSchermo As New Point(-32000, -32000)

        <TestMethod, TestCategory("Reale")>
        Public Sub LeggeTitoloIndirizzoETestoVisibile()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista, PaginaDiProva())
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.AreEqual("Magazziniere — Rossi S.p.A.", letta.Titolo, "il titolo della pagina")

            ' NavigateToString non lascia un indirizzo vero, ma uno ce n'è sempre: quel
            ' che conta è che il lettore lo riporti invece di inventarselo.
            Assert.IsNotEmpty(letta.Indirizzo, "un indirizzo c'è")

            Assert.Contains("Cerchiamo un magazziniere", letta.Testo, "il testo dell'annuncio")
            Assert.Contains("Città: Genova", letta.Testo,
                            "accenti compresi: passano da JSON, non dalla tabella dei caratteri")

            ' È «seleziona tutto → copia», non «leggi il sorgente»: quel che il foglio di
            ' stile nasconde non si vede, e non deve arrivare all'analisi.
            Assert.DoesNotContain("banner dei cookie", letta.Testo,
                                  "il testo nascosto resta fuori")
            Assert.DoesNotContain("<p>", letta.Testo, "e i tag nemmeno")

            Assert.IsFalse(letta.Troncato, "una pagina corta non si tronca")

        End Sub

        <TestMethod, TestCategory("Reale")>
        Public Sub UnaPaginaPiuLungaDelMassimoSiTroncaELoDichiara()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista, PaginaLunghissima())
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.IsLessThanOrEqualTo(LettorePagina.MassimoCaratteri, letta.Testo.Length,
                                       "il testo non supera il tetto")
            Assert.IsTrue(letta.Troncato,
                          "e il taglio si dichiara: niente si perde in silenzio")

        End Sub

        ' ==================================================================
        ' Attrezzi
        ' ==================================================================

        Private Shared Function PaginaDiProva() As String

            Return "<!doctype html><html lang=""it""><head><meta charset=""utf-8"">" &
                   "<title>Magazziniere — Rossi S.p.A.</title>" &
                   "<style>.nascosto { display: none }</style></head><body>" &
                   "<div class=""nascosto"">Questo è il banner dei cookie, mai mostrato.</div>" &
                   "<h1>Magazziniere</h1>" &
                   "<p>Cerchiamo un magazziniere con esperienza.</p>" &
                   "<p>Città: Genova. Perù escluso.</p>" &
                   "</body></html>"

        End Function

        ''' <summary>Una pagina con più testo del massimo che il lettore porta via.</summary>
        Private Shared Function PaginaLunghissima() As String

            Dim riga As String = "Requisito ripetuto per fare volume. "
            Dim quante As Integer = (LettorePagina.MassimoCaratteri \ riga.Length) + 50

            Return "<!doctype html><html><head><meta charset=""utf-8""><title>Lunga</title></head>" &
                   "<body><p>" & String.Concat(Enumerable.Repeat(riga, quante)) & "</p></body></html>"

        End Function

        ''' <summary>
        ''' Una vista accesa su un motore tutto suo, in una finestra fuori da ogni schermo:
        ''' il banco non deve far comparire finestre a chi lo lancia.
        ''' </summary>
        Private Shared Sub ConVista(prova As Func(Of WebView2, Task))

            Dim cartella As String = Path.Combine(Path.GetTempPath(),
                                                  "lettore-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(cartella)

            Try
                FiloGrafico.Esegui(
                    Async Function() As Task

                        Dim motore As New MotoreBrowser(Path.Combine(cartella, "webview2"))

                        Using finestra As New Form With {
                                  .FormBorderStyle = FormBorderStyle.None,
                                  .ShowInTaskbar = False,
                                  .StartPosition = FormStartPosition.Manual,
                                  .Location = FuoriSchermo,
                                  .Size = Area},
                              vista As New WebView2 With {.Dock = DockStyle.Fill}

                            finestra.Controls.Add(vista)
                            finestra.Show()

                            Await vista.EnsureCoreWebView2Async(Await motore.AmbienteAsync())
                            Await prova(vista)

                        End Using

                    End Function)

            Finally
                PortaVia(cartella)
            End Try

        End Sub

        Private Shared Async Function CaricaAsync(vista As WebView2, html As String) As Task

            Dim caricata As New TaskCompletionSource(Of Boolean)

            Dim aFineCaricamento As EventHandler(Of CoreWebView2NavigationCompletedEventArgs) = Nothing
            aFineCaricamento =
                Sub(mittente As Object, argomenti As CoreWebView2NavigationCompletedEventArgs)
                    RemoveHandler vista.NavigationCompleted, aFineCaricamento
                    caricata.TrySetResult(argomenti.IsSuccess)
                End Sub

            AddHandler vista.NavigationCompleted, aFineCaricamento

            vista.NavigateToString(html)

            Assert.IsTrue(Await caricata.Task, "la pagina di prova non si è caricata")

        End Function

        ''' <summary>
        ''' Come in <c>CollaudiMotoreBrowser</c>: il motore tiene il suo <c>lockfile</c>
        ''' finché non ha chiuso i propri processi, e un collaudo non deve fallire per le
        ''' pulizie.
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

        ' --- Riconoscere una pagina-risultati (R5, 2026-08-23) ---------------------------

        ''' <summary>Una lista di annunci: tante righe corte, tutte con le stesse parole.</summary>
        Private Shared Function ListaDiAnnunci(quanti As Integer) As String

            Dim righe As New List(Of String)
            For i As Integer = 1 To quanti
                righe.Add($"Magazziniere addetto al carico {i}")
                righe.Add("Logistica Bianchi s.r.l. - Sestri Levante")
                righe.Add($"{i} giorni fa")
                righe.Add("Candidati")
            Next
            Return String.Join(vbLf, righe)

        End Function

        ''' <summary>Un annuncio solo: poche righe, ma con dentro dei paragrafi veri.</summary>
        Private Shared Function UnAnnuncioSolo() As String

            Dim paragrafo As String =
                "Cerchiamo un magazziniere da inserire nel nostro reparto logistico con " &
                "esperienza nella preparazione degli ordini e nella gestione delle scorte, " &
                "disponibile a lavorare su turni anche nel fine settimana, con buona " &
                "capacità di organizzazione e attenzione alla sicurezza sul lavoro."

            Return String.Join(vbLf, {
                "Magazziniere addetto al carico",
                "Logistica Bianchi s.r.l. - Sestri Levante",
                "2 giorni fa",
                "Descrizione del posto",
                paragrafo,
                "Che cosa chiediamo",
                paragrafo,
                "Che cosa offriamo",
                paragrafo,
                "Candidati"})

        End Function

        <TestMethod>
        Public Sub UnaListaDiAnnunciSiRiconosce()

            Assert.IsTrue(LettorePagina.SembraUnaPaginaDiRisultati(ListaDiAnnunci(30)),
                          "trenta voci con «Candidati» e «giorni fa» a ogni riga sono una lista")

        End Sub

        <TestMethod>
        Public Sub UnAnnuncioSoloNonSiScambiaPerUnaLista()

            Assert.IsFalse(LettorePagina.SembraUnaPaginaDiRisultati(UnAnnuncioSolo()),
                           "qui il falso allarme è il danno peggiore: fermerebbe chi ha ragione")

        End Sub

        <TestMethod>
        Public Sub UnAnnuncioLungoEArticolatoNonSiScambiaPerUnaLista()

            ' Il caso che fa più paura: un annuncio ricco, con molte righe di elenco
            ' («richiediamo: …») e le stesse parole di servizio di un portale.
            Dim righe As New List(Of String) From {"Candidati", "3 giorni fa", "Offerte di lavoro"}
            For i As Integer = 1 To 45
                righe.Add($"requisito numero {i} richiesto per la posizione")
            Next
            righe.Add("Cerchiamo una persona motivata da inserire nel nostro organico con " &
                      "esperienza pregressa nel settore della logistica e della gestione " &
                      "del magazzino, disponibile fin da subito e con voglia di crescere " &
                      "insieme a noi in un ambiente giovane e dinamico e collaborativo.")

            Assert.IsFalse(LettorePagina.SembraUnaPaginaDiRisultati(String.Join(vbLf, righe)),
                           "le spie da sole non bastano: senza la ripetizione non è una lista")

        End Sub

        <TestMethod>
        Public Sub UnTestoCortoNonSiGiudicaAffatto()

            Assert.IsFalse(LettorePagina.SembraUnaPaginaDiRisultati(ListaDiAnnunci(3)),
                           "su poche righe non c'è abbastanza per dire niente, e nel dubbio si tace")
            Assert.IsFalse(LettorePagina.SembraUnaPaginaDiRisultati(""),
                           "e il vuoto non è una lista")
            Assert.IsFalse(LettorePagina.SembraUnaPaginaDiRisultati(Nothing),
                           "né lo è il niente")

        End Sub

    End Class

End Namespace
