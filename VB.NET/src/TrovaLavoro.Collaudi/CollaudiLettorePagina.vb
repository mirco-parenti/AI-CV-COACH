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

        ' --- L'avviso sui cookie (2026-08-30) -------------------------------------------

        ''' <summary>
        ''' Il banner del consenso non entra nel testo dell'annuncio, e che ci fosse si
        ''' dichiara.
        ''' </summary>
        ''' <remarks>
        ''' Il difetto vero, visto al primo avvio su una macchina nuova: premendo «Cattura
        ''' annuncio» prima di rispondere al banner, nella casella dell'annuncio finiva il
        ''' testo del banner — e nessuna guardia scattava, perché è corto e l'indirizzo è
        ''' quello della ricerca.
        ''' </remarks>
        <TestMethod, TestCategory("Reale")>
        Public Sub IlBannerDeiCookieRestaFuoriDalTestoESiDichiara()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista, PaginaCol(BannerDeiCookie("onetrust-consent-sdk") & AnnuncioInPagina()))
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.DoesNotContain("Rispettiamo la tua privacy", letta.Testo,
                                  "il banner non è l'annuncio")
            Assert.DoesNotContain("Accetta tutti", letta.Testo, "e nemmeno i suoi bottoni")
            Assert.Contains("Cerchiamo un magazziniere", letta.Testo,
                            "mentre l'annuncio sotto si legge tutto")
            Assert.IsTrue(letta.ConsensoAperto, "e che il banner ci fosse si dichiara")

        End Sub

        ''' <summary>
        ''' Una pagina ancora tutta coperta dal banner non ha testo da leggere — che è la
        ''' verità, e prima non lo sembrava affatto.
        ''' </summary>
        <TestMethod, TestCategory("Reale")>
        Public Sub UnaPaginaCopertaSoloDalBannerNonHaTestoDaLeggere()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista, PaginaCol(BannerDeiCookie("onetrust-banner-sdk")))
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.IsEmpty(letta.Testo.Trim(),
                           "sotto il banner non c'era niente, e niente si porta via")
            Assert.IsTrue(letta.ConsensoAperto,
                          "così a chi ha premuto si può dire che deve rispondere al banner")

        End Sub

        ''' <summary>
        ''' Un banner a cui si è già risposto non si dichiara aperto, e il bottoncino delle
        ''' preferenze che resta a video non è un banner.
        ''' </summary>
        ''' <remarks>
        ''' <para>I due modi in cui questo criterio poteva mentire dopo il consenso: il
        ''' gestore lascia il banner nel documento ma spento, e lascia a video un bottone
        ''' «Impostazioni cookie» che di un avviso ha il nome e non la sostanza — per quello
        ''' c'è <see cref="LettorePagina.MinimoDellAvvisoSuiCookie"/>.</para>
        ''' <para><b>Lo spegnimento qui è quello scomodo, ed è voluto</b>: il banner non è
        ''' spento da sé, è dentro un contenitore spento che di nome non dice niente. Chi
        ''' cerca l'avviso ci arriva dritto — il banner porta il nome giusto — e a quel punto
        ''' <c>display</c> risponde «block», perché è il valore suo e non quello che gli
        ''' antenati gli impongono. L'unica cosa che si accorge della differenza è che a
        ''' video non occupa spazio. Con il banner spento da sé (la prima versione di questo
        ''' collaudo) restava verde anche togliendo quel controllo: verde per il motivo
        ''' sbagliato.</para>
        ''' </remarks>
        <TestMethod, TestCategory("Reale")>
        Public Sub UnBannerGiaChiusoNonSiDichiaraAperto()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista,
                        PaginaCol("<div class='sovrapposizioni' style='display: none'>" &
                                  BannerDeiCookie("onetrust-consent-sdk") & "</div>" &
                                  "<div class='ot-floating-cookie-button'>Impostazioni cookie</div>" &
                                  AnnuncioInPagina()))
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.IsFalse(letta.ConsensoAperto,
                           "a un banner spento si è già risposto: non c'è niente da fare")
            Assert.Contains("Cerchiamo un magazziniere", letta.Testo, "e l'annuncio si legge")
            Assert.Contains("Impostazioni cookie", letta.Testo,
                            "il bottoncino si vede a video, e quel che si vede si legge")

        End Sub

        ''' <summary>
        ''' Un testo che <i>parla</i> di cookie non è un banner, e una pagina intera nemmeno.
        ''' </summary>
        ''' <remarks>
        ''' Qui il danno di sbagliarsi non è lasciar passare del rumore: è cancellare
        ''' l'annuncio. Perciò il nome non basta da solo — serve anche che il pezzo sia
        ''' corto come un avviso — e i due casi che tengono aperta questa porta sono un
        ''' annuncio che i cookie li nomina e un contenitore che di nome fa «cookie» e
        ''' dentro ha mezzo sito.
        ''' </remarks>
        <TestMethod, TestCategory("Reale")>
        Public Sub UnTestoCheParlaDiCookieNonSiScambiaPerUnBanner()

            Dim letta As PaginaLetta = Nothing

            Dim lungo As String = String.Concat(
                Enumerable.Repeat("Informativa estesa sull'uso dei cookie in questo sito. ", 120))

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista,
                        PaginaCol("<div class='descrizione'><p>Cerchiamo un magazziniere che sappia " &
                                  "usare il gestionale, i cookie del portale interno e il muletto.</p></div>" &
                                  "<div id='cookie-policy'><p>" & lungo & "</p></div>"))
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.Contains("Cerchiamo un magazziniere", letta.Testo,
                            "l'annuncio resta, anche se nomina i cookie")
            Assert.Contains("Informativa estesa", letta.Testo,
                            "e una pagina intera non si cancella perché si chiama «cookie»")
            Assert.IsFalse(letta.ConsensoAperto, "né si dichiara un consenso che nessuno chiede")

        End Sub

        ''' <summary>
        ''' Con una selezione il banner si dichiara lo stesso, e questa è l'altra strada per
        ''' cui lo si va a cercare.
        ''' </summary>
        ''' <remarks>
        ''' Quando l'utente ha selezionato il testo col mouse, la pagina non si percorre
        ''' affatto: si legge la selezione e basta (R5). Il banner non passerebbe mai per le
        ''' mani di chi lo toglie, e senza una seconda strada resterebbe invisibile.
        ''' </remarks>
        <TestMethod, TestCategory("Reale")>
        Public Sub ConUnaSelezioneIlBannerSiDichiaraLoStesso()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista,
                        PaginaCol(BannerDeiCookie("onetrust-consent-sdk") &
                                  "<div id='descrizione'><p>" & UnParagrafoLungo() & "</p></div>" &
                                  "<script>" &
                                  "var r = document.createRange();" &
                                  "r.selectNodeContents(document.getElementById('descrizione'));" &
                                  "var s = window.getSelection(); s.removeAllRanges(); s.addRange(r);" &
                                  "</script>"))
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.IsTrue(letta.DaSelezione, "la selezione ha la precedenza, com'era")
            Assert.IsTrue(letta.ConsensoAperto,
                          "e il banner si trova anche senza percorrere la pagina")

        End Sub

        ' --- Il contorno del portale (2026-08-30) ---------------------------------------

        ''' <summary>
        ''' Il menù del sito e il piè di pagina non entrano nel testo dell'annuncio.
        ''' </summary>
        ''' <remarks>
        ''' Il difetto era piccolo e vero: il testo catturato cominciava con «Passa a
        ''' contenuto principale, Homepage, Recensioni aziendali…» e finiva col piè di
        ''' pagina. L'AI lo ignora — ma la casella di P4 è dove l'utente rilegge e corregge
        ''' quel che ha preso, e la prima cosa che ci leggeva era il menù di Indeed.
        ''' </remarks>
        <TestMethod, TestCategory("Reale")>
        Public Sub IlContornoDelPortaleNonEntraNelTesto()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista,
                        PaginaCol("<header id='gnav'>" &
                                  "<a href='#contenuto'>Passa a contenuto principale</a>" &
                                  "<nav>Homepage · Recensioni aziendali · Esplora stipendi</nav>" &
                                  "</header>" &
                                  AnnuncioInPagina() &
                                  "<footer>Guida alla carriera · Cerca annunci · Chi siamo</footer>"))
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.DoesNotContain("Passa a contenuto principale", letta.Testo, "la testata resta fuori")
            Assert.DoesNotContain("Recensioni aziendali", letta.Testo, "il menù pure")
            Assert.DoesNotContain("Guida alla carriera", letta.Testo, "e il piè di pagina")
            Assert.IsTrue(letta.Testo.StartsWith("Magazziniere", StringComparison.Ordinal),
                          "così la prima riga che si legge è l'annuncio")

        End Sub

        ''' <summary>
        ''' La testata <b>dentro l'articolo</b> non è la testata del sito, e lì c'è quel che
        ''' l'annuncio dice di sé.
        ''' </summary>
        ''' <remarks>
        ''' È il criterio dello standard, ed è la difesa che tiene: <c>header</c> e
        ''' <c>footer</c> valgono per la pagina solo <b>fuori</b> da <c>article</c>,
        ''' <c>aside</c>, <c>main</c>, <c>nav</c> e <c>section</c>. Qui dentro non c'è
        ''' nessun <c>h1</c> apposta: se ci fosse, il collaudo resterebbe verde per l'altra
        ''' ragione, e di questa non direbbe niente.
        ''' </remarks>
        <TestMethod, TestCategory("Reale")>
        Public Sub LaTestataDentroLArticoloNonEQuellaDelSito()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista,
                        PaginaCol("<header id='gnav'><nav>Homepage · Esplora stipendi</nav></header>" &
                                  "<main><article>" &
                                  "<header><p>Rossi S.p.A. — Genova — pubblicato 2 giorni fa</p></header>" &
                                  "<p>" & UnParagrafoLungo() & "</p>" &
                                  "<footer><p>Codice offerta 12345</p></footer>" &
                                  "</article></main>"))
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.Contains("Rossi S.p.A.", letta.Testo, "chi offre il posto è dentro l'articolo")
            Assert.Contains("Codice offerta 12345", letta.Testo, "e il suo piè di pagina è suo")
            Assert.DoesNotContain("Esplora stipendi", letta.Testo, "mentre quella del sito se ne va")

        End Sub

        ''' <summary>
        ''' Una testata che porta il <b>titolo</b> non si toglie, nemmeno se è di pagina.
        ''' </summary>
        ''' <remarks>
        ''' L'asimmetria, scritta come collaudo: sbagliarsi in un verso costa il titolo
        ''' dell'annuncio, nell'altro costa una riga di logo. Nel dubbio si tiene — e il
        ''' menù dentro la testata risparmiata cade lo stesso, perché un <c>nav</c> resta un
        ''' <c>nav</c> anche là dentro.
        ''' </remarks>
        <TestMethod, TestCategory("Reale")>
        Public Sub UnaTestataColTitoloDellAnnuncioNonSiToglie()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista,
                        PaginaCol("<header><h1>Magazziniere</h1>" &
                                  "<p>Rossi S.p.A. — Genova</p>" &
                                  "<nav>Homepage · Esplora stipendi</nav></header>" &
                                  "<div><p>" & UnParagrafoLungo() & "</p></div>"))
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.Contains("Magazziniere", letta.Testo, "il titolo non si perde per un dubbio")
            Assert.Contains("Rossi S.p.A.", letta.Testo, "e nemmeno chi offre il posto")
            Assert.DoesNotContain("Esplora stipendi", letta.Testo,
                                  "ma il menù lì dentro cade quando ci si scende")

        End Sub

        ''' <summary>
        ''' Un contorno che dentro ha il contenuto principale non è un contorno.
        ''' </summary>
        <TestMethod, TestCategory("Reale")>
        Public Sub UnContornoCheContieneIlContenutoNonSiToglie()

            Dim letta As PaginaLetta = Nothing

            ConVista(
                Async Function(vista) As Task
                    Await CaricaAsync(vista,
                        PaginaCol("<footer role='contentinfo'>" &
                                  "<nav>Chi siamo · Supporto</nav>" &
                                  "<main><p>" & UnParagrafoLungo() & "</p></main>" &
                                  "</footer>"))
                    letta = Await New LettorePagina(vista.CoreWebView2).LeggiAsync()
                End Function)

            Assert.Contains("Cerchiamo un magazziniere", letta.Testo,
                            "un sito fatto male non è una buona ragione per cancellargli l'annuncio")
            Assert.DoesNotContain("Chi siamo", letta.Testo,
                                  "e il menù lì dentro se ne va lo stesso")

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

        ''' <summary>Una pagina qualunque, con dentro quel che le si mette.</summary>
        Private Shared Function PaginaCol(corpo As String) As String

            Return "<!doctype html><html lang='it'><head><meta charset='utf-8'>" &
                   "<title>Magazziniere — Rossi S.p.A.</title></head><body>" &
                   corpo & "</body></html>"

        End Function

        ''' <summary>
        ''' Il banner del consenso ai cookie, com'è fatto sui portali veri: un contenitore
        ''' il cui nome dice cos'è, un testo che nomina i cookie e i due bottoni.
        ''' </summary>
        Private Shared Function BannerDeiCookie(nome As String) As String

            Return $"<div id='{nome}'>" &
                   "<h2>Rispettiamo la tua privacy</h2>" &
                   "<p>Noi e i nostri partner usiamo i cookie per personalizzare i contenuti " &
                   "e misurare gli annunci. Puoi accettare tutto o gestire le preferenze.</p>" &
                   "<button>Accetta tutti</button><button>Rifiuta tutti</button>" &
                   "</div>"

        End Function

        ''' <summary>L'annuncio, dove un portale lo mette: dentro il contenuto principale.</summary>
        Private Shared Function AnnuncioInPagina() As String

            Return "<main><h1>Magazziniere</h1><p>" & UnParagrafoLungo() & "</p></main>"

        End Function

        Private Shared Function UnParagrafoLungo() As String

            Return "Cerchiamo un magazziniere con esperienza nella preparazione degli ordini " &
                   "e nella gestione delle scorte, disponibile a lavorare su turni anche nel " &
                   "fine settimana, con buona capacità di organizzazione e attenzione alla " &
                   "sicurezza sul lavoro."

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

        ''' <summary>
        ''' L'indirizzo di un annuncio si distingue da quello di una ricerca, sui portali
        ''' che il programma conosce.
        ''' </summary>
        ''' <remarks>
        ''' Nato il 2026-08-30 dal vicolo cieco della cattura: su Indeed un annuncio aperto
        ''' da solo ha la forma di una lista, e il giudizio sul testo lo rifiutava
        ''' consigliando di fare quel che l'utente aveva appena fatto. Questi sono i segni
        ''' che gli danno ragione prima che quel giudizio parli.
        ''' </remarks>
        <TestMethod>
        Public Sub UnIndirizzoDiAnnuncioSiDistingueDaUnoDiRicerca()

            Dim annunci As String() = {
                "https://it.indeed.com/viewjob?jk=9f3a1c",
                "https://it.indeed.com/m/viewjob?jk=9f3a1c&from=serp",
                "https://it.jooble.org/desc/1234567890",
                "https://www.subito.it/offerte-lavoro/magazziniere-genova-123456.htm",
                "https://www.linkedin.com/jobs/view/4012345678/"}

            For Each indirizzo As String In annunci
                Assert.IsTrue(LettorePagina.SembraLaPaginaDiUnAnnuncio(indirizzo),
                              $"«{indirizzo}» è la pagina di un annuncio solo")
            Next

            Dim ricerche As String() = {
                "https://it.indeed.com/jobs?q=magazziniere&l=Genova",
                "https://it.jooble.org/SearchResult?ukw=magazziniere",
                "https://www.subito.it/annunci-italia/vendita/offerte-lavoro/?q=magazziniere",
                "https://www.linkedin.com/jobs/search/?keywords=magazziniere"}

            For Each indirizzo As String In ricerche
                Assert.IsFalse(LettorePagina.SembraLaPaginaDiUnAnnuncio(indirizzo),
                               $"«{indirizzo}» è una pagina di risultati")
            Next

            ' Il caso preso dal vivo il 2026-08-30, e il motivo per cui i segni sono
            ' delimitati: cliccando un risultato, la pagina di RICERCA di Indeed si porta
            ' dietro «vjk=» — l'annuncio mostrato nel riquadro di destra — e «vjk=»
            ' contiene «jk=». Con il segno nudo la lista passava per un annuncio, e la
            ' cattura si riprendeva dentro tutte le offerte della pagina.
            Assert.IsFalse(
                LettorePagina.SembraLaPaginaDiUnAnnuncio(
                    "https://it.indeed.com/jobs?q=magazziniere&l=Genova&vjk=02e9c34d08f1f31d"),
                "la ricerca con un annuncio in anteprima resta una ricerca")

            ' Fuori dai portali conosciuti non si indovina: decide il testo, come prima.
            Assert.IsFalse(LettorePagina.SembraLaPaginaDiUnAnnuncio("https://www.azienda.it/lavora-con-noi/magazziniere"),
                           "un sito qualunque non si giudica dall'indirizzo")
            Assert.IsFalse(LettorePagina.SembraLaPaginaDiUnAnnuncio("non è un indirizzo"),
                           "e quel che non è un indirizzo non è un annuncio")
            Assert.IsFalse(LettorePagina.SembraLaPaginaDiUnAnnuncio(Nothing),
                           "nemmeno il nulla")

        End Sub

    End Class

End Namespace
