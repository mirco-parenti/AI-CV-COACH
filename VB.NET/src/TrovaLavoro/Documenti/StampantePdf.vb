Imports System.Drawing
Imports System.IO
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.Web.WebView2.Core
Imports Microsoft.Web.WebView2.WinForms
Imports TrovaLavoro.Web

Namespace Documenti

    ''' <summary>Perché una stampa non si è potuta fare.</summary>
    Public Enum CausaStampa
        ''' <summary>Il motore del browser di Windows non c'è (cap. 13.3).</summary>
        MotoreAssente
        ''' <summary>La pagina non si è caricata, o il PDF non è stato scritto.</summary>
        StampaFallita
    End Enum

    ''' <summary>
    ''' Una stampa che non si può portare a termine. Come <c>ErroreImport</c>, il
    ''' messaggio è già quello da mostrare all'utente, e dove esiste porta con sé il
    ''' ripiego onesto — qui: il documento in DOCX c'è comunque.
    ''' </summary>
    Public Class ErroreStampa
        Inherits Exception

        Public ReadOnly Property Causa As CausaStampa

        Public Sub New(causa As CausaStampa, messaggio As String, Optional dentro As Exception = Nothing)
            MyBase.New(messaggio, dentro)
            Me.Causa = causa
        End Sub

    End Class

    ''' <summary>
    ''' La stampante PDF: la pagina HTML di <see cref="ScrittoreHtml"/> viene mostrata a
    ''' una WebView2 <b>fuori schermo</b> e stampata con la funzione nativa di Chromium
    ''' (cap. 05.5). Ne esce un PDF con i font incorporati e il testo selezionabile,
    ''' senza librerie PDF e senza bisogno di Word.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>«Fuori schermo» ha un significato preciso</b>, verificato prima di
    ''' scrivere questo codice (cap. 05.5): il controllo pretende una finestra vera con
    ''' il suo handle, quindi la finestra <b>esiste</b> — ma nasce a coordinate fuori
    ''' dall'area visibile e non compare nella barra delle applicazioni. Non è un
    ''' espediente: è il modo in cui il motore accetta di lavorare senza mostrarsi.</para>
    ''' <para><b>Va usata dal thread dell'interfaccia.</b> WebView2 è un controllo
    ''' WinForms e vuole un thread STA con la sua pompa di messaggi: chiamarla da un
    ''' thread qualunque non funziona. Nell'applicazione è il thread in cui vive la
    ''' finestra; nel collaudo, un thread STA aperto apposta.</para>
    ''' <para><b>Una stampante, molte stampe.</b> Accendere il motore costa: la finestra
    ''' si prepara alla prima stampa e resta pronta per le successive — un CV e la sua
    ''' lettera sono due stampe di fila. Si chiude con <see cref="Dispose"/>.</para>
    ''' <para><b>L'ambiente non è suo</b> <i>(da T5a, 2026-08-12)</i>: lo chiede al
    ''' <see cref="MotoreBrowser"/>, che è l'unico dell'applicazione. Prima se lo creava
    ''' da sé, e finché il browser di P3 non esisteva era lo stesso; da qui in poi due
    ''' ambienti sulla stessa cartella di navigazione sono un guasto che aspetta soltanto
    ''' che le loro opzioni divergano.</para>
    ''' </remarks>
    Public Class StampantePdf
        Implements IDisposable

        ''' <summary>
        ''' Il margine del foglio, in pollici: 0,79" sono i 2 cm del DOCX. I margini li
        ''' mette la stampa e non il foglio di stile — dichiararli in tutti e due i posti
        ''' li sommerebbe.
        ''' </summary>
        Private Const Margine As Double = 0.79

        ''' <summary>
        ''' Quanto è grande la finestra che non si vede. Non cambia il PDF, che è
        ''' impaginato sul foglio: serve solo a dare alla WebView un'area di lavoro
        ''' plausibile.
        ''' </summary>
        Private Shared ReadOnly Area As New Size(1024, 1400)

        ''' <summary>
        ''' Dove nasce la finestra: fuori da qualunque schermo, anche in una postazione a
        ''' più monitor.
        ''' </summary>
        Private Shared ReadOnly FuoriSchermo As New Point(-32000, -32000)

        Private ReadOnly _motore As MotoreBrowser

        Private _finestra As Form
        Private _vista As WebView2

        ''' <param name="motore">
        ''' L'unico motore del browser dell'applicazione, che porta con sé la cartella di
        ''' navigazione (cap. 11.1, <c>webview2\</c>).
        ''' </param>
        Public Sub New(motore As MotoreBrowser)

            If motore Is Nothing Then Throw New ArgumentNullException(NameOf(motore))

            _motore = motore

        End Sub

        ''' <summary>
        ''' Stampa la pagina in un file PDF. Come per il DOCX la scrittura è atomica: il
        ''' motore scrive un file temporaneo, che prende il nome definitivo solo a stampa
        ''' riuscita.
        ''' </summary>
        ''' <param name="pagina">La pagina da stampare.</param>
        ''' <param name="percorso">Il file <c>.pdf</c> da scrivere.</param>
        Public Async Function StampaAsync(pagina As PaginaDocumento, percorso As String) As Task

            If pagina Is Nothing Then Throw New ArgumentNullException(NameOf(pagina))
            If String.IsNullOrWhiteSpace(percorso) Then
                Throw New ArgumentException("Manca il percorso del documento.", NameOf(percorso))
            End If

            Await AccendiAsync().ConfigureAwait(True)

            Await MostraAsync(ScrittoreHtml.Componi(pagina)).ConfigureAwait(True)

            Dim temporaneo As String = percorso & ".tmp"
            Dim fatto As Boolean = Await _vista.CoreWebView2.
                PrintToPdfAsync(temporaneo, Impostazioni()).ConfigureAwait(True)

            If Not fatto Then
                If File.Exists(temporaneo) Then File.Delete(temporaneo)
                Throw New ErroreStampa(CausaStampa.StampaFallita,
                                       "Non sono riuscita a scrivere il PDF del documento.")
            End If

            File.Move(temporaneo, percorso, overwrite:=True)

        End Function

        ''' <summary>
        ''' Accende il motore, se non è già acceso: l'ambiente, la finestra invisibile e
        ''' la WebView dentro di lei.
        ''' </summary>
        Private Async Function AccendiAsync() As Task

            If _vista IsNot Nothing Then Return

            Dim ambiente As CoreWebView2Environment

            Try
                ambiente = Await _motore.AmbienteAsync().ConfigureAwait(True)

            Catch ex As ErroreBrowser When ex.MotoreAssente
                ' Il runtime è di serie in Windows 11, ma su una macchina bloccata può
                ' mancare: l'app non muore e lo dice, perché il DOCX resta disponibile.
                Throw New ErroreStampa(CausaStampa.MotoreAssente,
                    "Per creare il PDF serve il componente WebView2 di Windows, che su questo " &
                    "computer non c'è. Il documento in formato Word è stato scritto lo stesso.", ex)

            Catch ex As ErroreBrowser
                ' Il motore c'è ma non si è acceso: il ripiego è lo stesso, e vale la pena
                ' dirlo con le stesse parole invece di lasciar passare un guasto tecnico.
                Throw New ErroreStampa(CausaStampa.StampaFallita,
                    "Non sono riuscita ad accendere il motore che stampa i PDF. " &
                    "Il documento in formato Word è stato scritto lo stesso.", ex)
            End Try

            _finestra = New Form With {
                .FormBorderStyle = FormBorderStyle.None,
                .ShowInTaskbar = False,
                .StartPosition = FormStartPosition.Manual,
                .Location = FuoriSchermo,
                .Size = Area}

            _vista = New WebView2 With {.Dock = DockStyle.Fill}
            _finestra.Controls.Add(_vista)

            ' La finestra si mostra davvero: è l'unico modo perché il controllo abbia un
            ' handle e il motore accetti di lavorare. Nessuno la vede, dov'è.
            _finestra.Show()

            Await _vista.EnsureCoreWebView2Async(ambiente).ConfigureAwait(True)

        End Function

        ''' <summary>Mostra la pagina alla WebView e aspetta che sia caricata davvero.</summary>
        Private Async Function MostraAsync(html As String) As Task

            Dim caricata As New TaskCompletionSource(Of Boolean)

            Dim aCaricamentoFinito As EventHandler(Of CoreWebView2NavigationCompletedEventArgs) = Nothing
            aCaricamentoFinito =
                Sub(mittente As Object, argomenti As CoreWebView2NavigationCompletedEventArgs)
                    RemoveHandler _vista.NavigationCompleted, aCaricamentoFinito
                    caricata.TrySetResult(argomenti.IsSuccess)
                End Sub

            AddHandler _vista.NavigationCompleted, aCaricamentoFinito
            _vista.NavigateToString(html)

            If Not Await caricata.Task.ConfigureAwait(True) Then
                Throw New ErroreStampa(CausaStampa.StampaFallita,
                                       "Non sono riuscita a preparare la pagina da stampare.")
            End If

        End Function

        ''' <summary>Il foglio della stampa: A4 in verticale con i margini del DOCX.</summary>
        Private Function Impostazioni() As CoreWebView2PrintSettings

            Dim foglio As CoreWebView2PrintSettings =
                _vista.CoreWebView2.Environment.CreatePrintSettings()

            foglio.MarginTop = Margine
            foglio.MarginBottom = Margine
            foglio.MarginLeft = Margine
            foglio.MarginRight = Margine

            ' Niente intestazioni del browser (titolo della pagina, data, indirizzo): un
            ' CV non è la stampa di una pagina web.
            foglio.ShouldPrintHeaderAndFooter = False
            foglio.ShouldPrintBackgrounds = False

            Return foglio

        End Function

        ''' <summary>Spegne il motore e chiude la finestra che non si vedeva.</summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            _vista?.Dispose()
            _vista = Nothing

            _finestra?.Dispose()
            _finestra = Nothing

            GC.SuppressFinalize(Me)

        End Sub

    End Class

End Namespace
