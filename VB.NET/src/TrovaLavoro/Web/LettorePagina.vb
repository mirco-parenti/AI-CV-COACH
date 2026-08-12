Imports System.Globalization
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.Web.WebView2.Core

Namespace Web

    ''' <summary>
    ''' Quel che si è letto dalla pagina che l'utente sta guardando: il <b>titolo</b>,
    ''' l'<b>indirizzo</b> e il <b>testo visibile</b> (cap. 06.4).
    ''' </summary>
    ''' <remarks>
    ''' Nessuno dei tre è mai <c>Nothing</c>: una pagina che non si è lasciata leggere
    ''' torna con i campi vuoti, e a chi ha premuto «Cattura annuncio» si risponde con
    ''' garbo invece che con un guasto.
    ''' </remarks>
    Public Class PaginaLetta

        Public Property Titolo As String = String.Empty
        Public Property Indirizzo As String = String.Empty
        Public Property Testo As String = String.Empty

        ''' <summary>
        ''' Se la pagina aveva più testo del massimo e si è dovuto tagliare. Va detto: un
        ''' annuncio letto a metà è ancora un annuncio, ma chi legge deve sapere che manca
        ''' un pezzo — nel progetto niente si perde in silenzio.
        ''' </summary>
        Public Property Troncato As Boolean

    End Class

    ''' <summary>
    ''' Chi sa leggere la pagina aperta nel browser integrato.
    ''' </summary>
    ''' <remarks>
    ''' C'è un'interfaccia per la stessa ragione per cui l'AI ne ha una: la cattura è fatta
    ''' di decisioni — il testo basta? è un annuncio? da che portale viene? — e quelle
    ''' decisioni il banco le deve poter provare senza pretendere WebView2 e un thread STA
    ''' (v. <see cref="MotoreBrowser"/>). La lettura vera resta una riga di JavaScript, e
    ''' ha il suo collaudo nella categoria «Reale».
    ''' </remarks>
    Public Interface ILettorePagina

        ''' <summary>Legge la pagina di adesso.</summary>
        Function LeggiAsync() As Task(Of PaginaLetta)

    End Interface

    ''' <summary>
    ''' Legge la pagina dal DOM della <c>WebView2</c>: l'equivalente di «seleziona tutto →
    ''' copia», fatto dal programma (cap. 06.4).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché <c>innerText</c> e non <c>textContent</c>.</b> Il primo è il testo
    ''' come lo si vede — rispetta ciò che il foglio di stile nasconde, e manda a capo dove
    ''' la pagina va a capo; il secondo restituisce anche i menù nascosti, i banner mai
    ''' mostrati e le porzioni spente, che nell'annuncio non ci sono e che l'analisi
    ''' dovrebbe poi ignorare.</para>
    ''' <para><b>Il limite noto</b>: il testo dentro un <c>iframe</c> non ci arriva, perché
    ''' quella è un'altra pagina. Sui portali del primo rilascio la pagina di un singolo
    ''' annuncio non ne usa, ma un portale che cambiasse idea si riconoscerebbe subito —
    ''' il testo tornerebbe troppo corto, e la cattura lo dice invece di far analizzare
    ''' un'accozzaglia di menù.</para>
    ''' <para>Va usato dal thread dell'interfaccia, come tutto ciò che tocca la vista.</para>
    ''' </remarks>
    Public Class LettorePagina
        Implements ILettorePagina

        ''' <summary>
        ''' Quanto testo si porta via al massimo. Un annuncio sta in poche migliaia di
        ''' caratteri: il tetto è largo apposta, e serve solo a non trascinare dentro
        ''' l'intero contenuto di una pagina che di annunci ne mostra cento.
        ''' </summary>
        Public Const MassimoCaratteri As Integer = 50000

        Private ReadOnly _vista As CoreWebView2

        Public Sub New(vista As CoreWebView2)

            If vista Is Nothing Then Throw New ArgumentNullException(NameOf(vista))
            _vista = vista

        End Sub

        Public Async Function LeggiAsync() As Task(Of PaginaLetta) Implements ILettorePagina.LeggiAsync

            Return Interpreta(Await _vista.ExecuteScriptAsync(Copione()).ConfigureAwait(True))

        End Function

        ''' <summary>
        ''' Il JavaScript che legge i tre pezzi in un colpo solo: tre viaggi separati
        ''' potrebbero cadere su tre pagine diverse, se l'utente naviga nel frattempo.
        ''' </summary>
        Private Shared Function Copione() As String

            Dim massimo As String = MassimoCaratteri.ToString(CultureInfo.InvariantCulture)

            Return "(function () {" &
                   "  var t = document.body ? document.body.innerText : '';" &
                   "  return JSON.stringify({" &
                   "    titolo: document.title || ''," &
                   "    indirizzo: location.href || ''," &
                   $"    testo: t.slice(0, {massimo})," &
                   $"    troncato: t.length > {massimo}" &
                   "  });" &
                   "})()"

        End Function

        ''' <summary>
        ''' Da quel che la vista risponde alla pagina letta. La risposta è <b>JSON dentro
        ''' JSON</b>: il copione restituisce una stringa, e <c>ExecuteScriptAsync</c>
        ''' consegna la rappresentazione JSON di quel risultato — cioè quella stringa fra
        ''' virgolette e con le fughe. Vanno perciò tolti due strati, e non uno.
        ''' </summary>
        Private Shared Function Interpreta(risposta As String) As PaginaLetta

            Dim letta As New PaginaLetta()

            ' «null» è quel che torna quando il copione non ha potuto girare: una pagina
            ' interna del browser, una scheda ancora vuota, un errore di rete.
            If String.IsNullOrWhiteSpace(risposta) OrElse risposta = "null" Then Return letta

            Try
                Dim primoStrato As JsonNode = JsonNode.Parse(risposta)
                If primoStrato Is Nothing OrElse
                   primoStrato.GetValueKind() <> JsonValueKind.String Then Return letta

                Dim contenuto As JsonObject = TryCast(
                    JsonNode.Parse(primoStrato.GetValue(Of String)()), JsonObject)
                If contenuto Is Nothing Then Return letta

                letta.Titolo = Testo(contenuto, "titolo")
                letta.Indirizzo = Testo(contenuto, "indirizzo")
                letta.Testo = Testo(contenuto, "testo")
                letta.Troncato = Vero(contenuto, "troncato")

            Catch ex As JsonException
                ' Una risposta che non è JSON non è un guasto da propagare: è una pagina
                ' che non si è lasciata leggere, ed è già un esito previsto.
                Return New PaginaLetta()

            End Try

            Return letta

        End Function

        Private Shared Function Testo(oggetto As JsonObject, campo As String) As String

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, valore) OrElse valore Is Nothing Then
                Return String.Empty
            End If

            Return If(valore.GetValueKind() = JsonValueKind.String,
                      valore.GetValue(Of String)(), String.Empty)

        End Function

        Private Shared Function Vero(oggetto As JsonObject, campo As String) As Boolean

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, valore) OrElse valore Is Nothing Then Return False

            Return valore.GetValueKind() = JsonValueKind.True

        End Function

    End Class

End Namespace
