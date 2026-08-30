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

        ''' <summary>
        ''' Se il testo viene da quello che l'utente aveva <b>selezionato</b> nella pagina
        ''' invece che da tutta la pagina (R5, 2026-08-23). È la risposta più semplice alle
        ''' pagine-risultati, dove catturare tutto significa catturare cento annunci: chi
        ''' seleziona l'annuncio che gli interessa ha già detto, senza doverlo spiegare a
        ''' nessuno, qual è il testo che conta. Va detto a chi guarda, perché cambia cosa è
        ''' stato letto.
        ''' </summary>
        Public Property DaSelezione As Boolean

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

        ''' <summary>
        ''' Scorre la pagina fino in fondo e torna in cima, per far entrare nel documento
        ''' anche quello che il sito carica solo <b>mentre si scende</b>.
        ''' </summary>
        ''' <remarks>
        ''' Non serve a tutte le letture, e infatti non lo chiede nessuno d'ufficio: la
        ''' cattura di un annuncio legge la pagina com'è. Lo chiede l'import del CV, dove
        ''' la differenza è fra un profilo intero e un profilo dimezzato (cap. 06.7).
        ''' </remarks>
        Function ScorriAsync() As Task

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
    ''' <para><b>Perché a pezzi e non tutto in un colpo</b> <i>(T9d)</i>. Chiedere
    ''' <c>innerText</c> al solo <c>body</c> lasciava la fine di un blocco attaccata
    ''' all'inizio del successivo — «Pubblica AmministrazioneDue suite specializzate» — e
    ''' su quel sito il modello aveva capito lo stesso, per fortuna e non per progetto.
    ''' Adesso si scende fino ai <b>blocchi foglia</b> (quelli che dentro non hanno altri
    ''' blocchi) e si chiede <c>innerText</c> a ciascuno, unendo con un a capo: la fonte
    ''' resta la stessa — coi suoi pregi — ma fra un pezzo e l'altro un confine c'è
    ''' sempre. Quel che il foglio di stile spegne si salta prima di scendere, e i nodi di
    ''' testo appesi direttamente a un contenitore di blocchi non si perdono: diventano un
    ''' pezzo anche loro.</para>
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

        ''' <summary>
        ''' Quanto testo deve avere una selezione perché si preferisca alla pagina intera.
        ''' Sotto, è più probabile un clic andato a vuoto che una scelta voluta.
        ''' </summary>
        Public Const MinimoDellaSelezione As Integer = 200

        ' Le soglie con cui si riconosce una pagina-risultati. Sono volutamente SEVERE:
        ' sbagliare dicendo «sembra una lista» su un annuncio buono ferma chi aveva ragione,
        ' mentre non accorgersene lascia le cose come stavano prima. Nel dubbio, si tace.
        Private Const RigheMinimePerGiudicare As Integer = 40
        Private Const RigaLunga As Integer = 160
        Private Const QuotaMassimaDiRigheLunghe As Double = 0.03
        Private Const SpieMinime As Integer = 8

        ''' <summary>Le parole che in una lista di annunci si ripetono a ogni riga.</summary>
        Private Shared ReadOnly Spie As String() = {
            "candidati", "giorni fa", "giorno fa", "ore fa", "ora fa", "minuti fa",
            "annunci", "offerte di lavoro", "filtra", "ordina per"}

        ''' <summary>
        ''' Se il testo catturato somiglia a una <b>pagina di risultati</b> invece che a un
        ''' annuncio solo (R5, 2026-08-23). Una lista è fatta di tante righe corte tutte
        ''' uguali, con le stesse parole di servizio ripetute a ogni voce, e quasi nessun
        ''' paragrafo vero; un annuncio ha meno righe e almeno qualche frase lunga.
        ''' </summary>
        ''' <remarks>
        ''' Il danno che previene non è l'analisi sbagliata: è il <b>silenzio</b>. Senza
        ''' questo, l'applicazione analizza l'accozzaglia, dà un punteggio in stelle e
        ''' sembra funzionare (collaudo dal vivo di T9e, giro C). Non pretende di
        ''' riconoscerle tutte, e nemmeno di sbagliare mai: pretende di non fingere che vada
        ''' bene quando ha in mano cento annunci invece di uno.
        ''' </remarks>
        Public Shared Function SembraUnaPaginaDiRisultati(testo As String) As Boolean

            If String.IsNullOrWhiteSpace(testo) Then Return False

            Dim righe As List(Of String) = testo.
                Split(ControlChars.Lf).
                Select(Function(r) r.Trim()).
                Where(Function(r) r.Length > 0).
                ToList()

            If righe.Count < RigheMinimePerGiudicare Then Return False

            ' In VB «Count» è la property della lista e copre il metodo con predicato:
            ' si passa da Where, o il compilatore protesta che Count non si indicizza.
            Dim lunghe As Integer = righe.Where(Function(r) r.Length >= RigaLunga).Count()
            If lunghe / CDbl(righe.Count) > QuotaMassimaDiRigheLunghe Then Return False

            Dim minuscolo As String = testo.ToLowerInvariant()
            Dim quante As Integer = Spie.Sum(Function(s) Ripetizioni(minuscolo, s))

            Return quante >= SpieMinime

        End Function

        ''' <summary>
        ''' Le forme con cui i portali che conosciamo scrivono l'indirizzo di <b>un</b>
        ''' annuncio, distinte da quelle delle loro pagine di ricerca.
        ''' </summary>
        ''' <remarks>
        ''' <para>È conoscenza che invecchia — un portale può cambiare i suoi indirizzi
        ''' domani — e per questo sta scritta in un posto solo e non decide mai da sola: se
        ''' il segno non si riconosce più, si torna esattamente al comportamento di prima,
        ''' cioè al giudizio sul testo. Un pattern che invecchia qui non rompe niente:
        ''' smette di aiutare.</para>
        ''' <para><b>I segni sono delimitati apposta.</b> Il primo che scrissi per Indeed
        ''' era <c>jk=</c> nudo, e nella prova dal vivo dello stesso giorno si è visto cosa
        ''' vuol dire: la <b>pagina di ricerca</b> di Indeed, quando si clicca un risultato,
        ''' diventa <c>jobs?q=…&amp;<b>vjk</b>=…</c> — «viewed job key», l'annuncio mostrato
        ''' nel riquadro di destra — e <c>vjk=</c> contiene <c>jk=</c>. La lista veniva
        ''' scambiata per un annuncio, e la cattura ci prendeva dentro tutte e venticinque
        ''' le offerte: cioè esattamente il difetto (R5) che l'avviso era nato per fermare,
        ''' riaperto dalla cura. Il segno buono è il <b>percorso</b> <c>/viewjob</c>, che è
        ''' la pagina dell'annuncio da solo; il parametro vale solo se comincia davvero lì
        ''' dove un parametro comincia, <c>?jk=</c> o <c>&amp;jk=</c>.</para>
        ''' </remarks>
        Private Shared ReadOnly SegniDiUnAnnuncio As New Dictionary(Of String, String()) From {
            {"indeed.", {"/viewjob", "?jk=", "&jk="}},
            {"jooble.", {"/desc/", "/jdp/"}},
            {"subito.it", {".htm"}},
            {"linkedin.", {"/jobs/view/"}}
        }

        ''' <summary>
        ''' Se l'<b>indirizzo</b> è quello della pagina di un singolo annuncio, e non di un
        ''' elenco di offerte.
        ''' </summary>
        ''' <remarks>
        ''' <para>Nasce il 2026-08-30 da un guasto vero, e da un vicolo cieco. Il giudizio
        ''' sul testo (<see cref="SembraUnaPaginaDiRisultati"/>) guarda la <i>forma</i> della
        ''' pagina: righe corte, nessun paragrafo lungo, parole di servizio ripetute. Un
        ''' annuncio di Indeed aperto <b>da solo</b> ha esattamente quella forma — è fatto di
        ''' elenchi puntati, e in coda porta «Candidati» e i lavori simili col loro «giorni
        ''' fa» — e finiva scambiato per la lista da cui l'utente era appena uscito. Il
        ''' consiglio che gli si dava («apri il singolo annuncio e premi di nuovo») era
        ''' quello che aveva già fatto: il messaggio tornava identico, e l'unica uscita
        ''' rimasta era selezionare il testo col mouse.</para>
        ''' <para><b>L'indirizzo è un segno più forte del testo</b>, e lo era già: che
        ''' <c>it.indeed.com/jobs?q=…</c> cerchi e <c>it.indeed.com/viewjob?jk=…</c> sia
        ''' l'annuncio stava scritto in <c>Ricerche.FonteDi</c> da T5, ma la cattura non lo
        ''' guardava. Quando l'indirizzo dice «questo è un annuncio», il giudizio sul testo
        ''' non si applica: non perché sia sbagliato, ma perché sta rispondendo a una
        ''' domanda a cui qualcuno ha già risposto meglio.</para>
        ''' <para>Non riconosce tutti i portali del mondo, e non ci prova: fuori da questi
        ''' quattro resta il giudizio sul testo, com'era.</para>
        ''' </remarks>
        Public Shared Function SembraLaPaginaDiUnAnnuncio(indirizzo As String) As Boolean

            Dim letto As Uri = Nothing
            If String.IsNullOrWhiteSpace(indirizzo) OrElse
               Not Uri.TryCreate(indirizzo.Trim(), UriKind.Absolute, letto) Then Return False

            Dim sito As String = letto.Host.ToLowerInvariant()

            ' Percorso e parametri insieme: «/viewjob» sta nel primo, «jk=» nei secondi, e
            ' un portale può spostare un annuncio dall'uno agli altri senza avvisare.
            Dim resto As String = (letto.AbsolutePath & letto.Query).ToLowerInvariant()

            For Each portale As KeyValuePair(Of String, String()) In SegniDiUnAnnuncio

                If Not sito.Contains(portale.Key) Then Continue For

                For Each segno As String In portale.Value
                    If resto.Contains(segno) Then Return True
                Next

                ' Sito conosciuto ma nessun segno: è una sua pagina che annuncio non è.
                Return False

            Next

            Return False

        End Function

        ''' <summary>Quante volte una parola compare in un testo già in minuscolo.</summary>
        Private Shared Function Ripetizioni(testo As String, parola As String) As Integer

            Dim quante As Integer = 0
            Dim da As Integer = testo.IndexOf(parola, StringComparison.Ordinal)

            While da >= 0
                quante += 1
                da = testo.IndexOf(parola, da + parola.Length, StringComparison.Ordinal)
            End While

            Return quante

        End Function

        Private ReadOnly _vista As CoreWebView2

        Public Sub New(vista As CoreWebView2)

            If vista Is Nothing Then Throw New ArgumentNullException(NameOf(vista))
            _vista = vista

        End Sub

        ''' <summary>
        ''' Quante schermate al massimo si scende, e quanto si aspetta a ogni passo perché
        ''' il sito abbia il tempo di caricare quel che manca. In tutto fanno pochi secondi:
        ''' è uno scorrimento, non una raccolta — la pagina è una, quella che l'utente ha
        ''' davanti, e il tetto c'è perché una pagina infinita non tenga occupato il
        ''' programma finché l'utente non si stufa.
        ''' </summary>
        Public Const PassiDiScorrimento As Integer = 25
        Public Const AttesaFraPassiMs As Integer = 400

        ''' <summary>
        ''' Quante volte di fila si deve trovare la pagina «finita» prima di crederci.
        ''' </summary>
        ''' <remarks>
        ''' Misurato sul campo il 2026-08-14, ed è la ragione per cui il primo tentativo
        ''' non serviva a niente: una pagina non ancora caricata è <b>corta</b>, così il
        ''' fondo arriva dopo due passi, e in quel momento il sito non ha ancora aggiunto
        ''' nulla. Chi si ferma alla prima impressione di aver finito legge la sola
        ''' intestazione — 2196 caratteri, esattamente come se non avesse scorso.
        ''' </remarks>
        Public Const ConfermeDiFine As Integer = 3

        Public Async Function LeggiAsync() As Task(Of PaginaLetta) Implements ILettorePagina.LeggiAsync

            Return Interpreta(Await _vista.ExecuteScriptAsync(Copione()).ConfigureAwait(True))

        End Function

        ''' <summary>
        ''' Scende per la pagina una schermata alla volta e poi risale, come farebbe una
        ''' persona che vuole leggersela tutta prima di copiarla.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>Perché a passi dal lato nostro e non con un ciclo in JavaScript.</b>
        ''' Il ciclo dovrebbe aspettare, e una funzione JavaScript che aspetta restituisce
        ''' una promessa: <c>ExecuteScriptAsync</c> non la attende, tornerebbe subito e
        ''' l'attesa non ci sarebbe. Facendo i passi da qui, fra un passo e l'altro
        ''' l'attesa è vera — ed è quella che dà al sito il tempo di aggiungere quel che
        ''' mancava.</para>
        ''' <para><b>Quando ci si ferma</b>: quando si è in fondo <i>e</i> la pagina non è
        ''' più cresciuta rispetto al passo precedente. Il solo «sono in fondo» non basta:
        ''' su una pagina che carica scendendo, il fondo di adesso è la metà di quello di
        ''' fra un istante.</para>
        ''' <para>Alla fine si <b>torna in cima</b>: la pagina resta come l'utente l'aveva
        ''' lasciata, perché è sua e non l'ha spostata lui.</para>
        ''' </remarks>
        Public Async Function ScorriAsync() As Task Implements ILettorePagina.ScorriAsync

            Dim altezzaDiPrima As Double = -1
            Dim conferme As Integer = 0

            For passo As Integer = 1 To PassiDiScorrimento

                Dim dove As JsonObject = TryCast(
                    JsonNode.Parse(Await _vista.ExecuteScriptAsync(UnPasso()).ConfigureAwait(True)),
                    JsonObject)

                ' Una pagina che non si lascia scorrere non è un guasto: si legge quel che
                ' c'è, che è esattamente ciò che si sarebbe letto senza tutto questo.
                If dove Is Nothing Then Exit For

                Await Task.Delay(AttesaFraPassiMs).ConfigureAwait(True)

                Dim altezza As Double = Numero(dove, "altezza")

                ' Una pagina che non si muove non si muoverà nemmeno insistendo: non c'è
                ' niente da scorrere, o lo scorrimento non è nostro da comandare.
                If Not Vero(dove, "mosso") AndAlso Vero(dove, "fondo") Then Exit For

                ' Si conta quante volte di fila la pagina si dichiara finita e non cresce.
                ' Basta che una volta cresca perché il conto riparta: vuol dire che il
                ' sito stava ancora lavorando, e il fondo di prima non era il fondo.
                If Vero(dove, "fondo") AndAlso altezza <= altezzaDiPrima Then
                    conferme += 1
                    If conferme >= ConfermeDiFine Then Exit For
                Else
                    conferme = 0
                End If

                altezzaDiPrima = Math.Max(altezzaDiPrima, altezza)

            Next

            ' Si torna in cima da dove si è scesi — che non è detto sia la finestra.
            Await _vista.ExecuteScriptAsync(
                "(function () {" &
                "  var d = document.scrollingElement || document.documentElement;" &
                "  if (d) d.scrollTop = 0;" &
                "  var tutti = document.querySelectorAll('div, main, section');" &
                "  for (var i = 0; i < tutti.length; i++) {" &
                "    if (tutti[i].scrollTop > 0) tutti[i].scrollTop = 0;" &
                "  }" &
                "})()").ConfigureAwait(True)

        End Function

        ''' <summary>
        ''' Un passo in giù, e il resoconto di dove si è arrivati.
        ''' </summary>
        ''' <remarks>
        ''' Qui l'oggetto torna <b>così com'è</b>, senza passare da <c>JSON.stringify</c>:
        ''' <c>ExecuteScriptAsync</c> consegna già la forma JSON del risultato, e
        ''' impacchettarlo a mano vorrebbe dire toglierne due strati invece di uno
        ''' (v. <see cref="Interpreta"/>, dove la stringa serve perché il testo di una
        ''' pagina non ha una forma prevedibile).
        ''' </remarks>
        Private Shared Function UnPasso() As String

            Return "(function () {" &
                   "  function chiScorre() {" &
                   "    var d = document.scrollingElement || document.documentElement;" &
                   "    if (d && d.scrollHeight > d.clientHeight + 4) return d;" &
                   "    var scelto = null, area = 0;" &
                   "    var tutti = document.querySelectorAll('div, main, section');" &
                   "    for (var i = 0; i < tutti.length; i++) {" &
                   "      var c = tutti[i];" &
                   "      if (c.scrollHeight > c.clientHeight + 4 && c.clientHeight > 200) {" &
                   "        var a = c.clientHeight * c.clientWidth;" &
                   "        if (a > area) { area = a; scelto = c; }" &
                   "      }" &
                   "    }" &
                   "    return scelto || d;" &
                   "  }" &
                   "  var e = chiScorre();" &
                   "  if (!e) return { altezza: 0, mosso: false, fondo: true };" &
                   "  var prima = e.scrollTop;" &
                   "  e.scrollTop = prima + Math.max(200, e.clientHeight);" &
                   "  return {" &
                   "    altezza: e.scrollHeight," &
                   "    mosso: e.scrollTop !== prima," &
                   "    fondo: (e.scrollTop + e.clientHeight) >= e.scrollHeight - 4" &
                   "  };" &
                   "})()"

        End Function

        ''' <summary>Un numero dal resoconto del passo; <c>-1</c> se non c'è.</summary>
        Private Shared Function Numero(oggetto As JsonObject, campo As String) As Double

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, valore) OrElse valore Is Nothing Then Return -1

            Return If(valore.GetValueKind() = JsonValueKind.Number, valore.GetValue(Of Double)(), -1)

        End Function

        ''' <summary>
        ''' Il JavaScript che legge i tre pezzi in un colpo solo: tre viaggi separati
        ''' potrebbero cadere su tre pagine diverse, se l'utente naviga nel frattempo.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>Un pezzo che non porta né lettere né cifre non entra nel testo</b>
        ''' <i>(T9d, 2026-08-22)</i>. Le pagine dei portali sono piene di elementi che
        ''' l'occhio legge come <i>grafica</i> — una linea di separazione, un pallino, uno
        ''' spazio — e che al copione arrivano come testo: su Indeed sono <c>&amp;nbsp;</c>
        ''' scritti per esteso, sei caratteri per ogni riga grigia che si vede a video. Non
        ''' ingannano l'AI, che li ignora, ma sporcano il testo che l'utente deve poter
        ''' rileggere e correggere (cap. 06.4).</para>
        ''' <para><b>Le entità HTML si tolgono prima di giudicare</b>, o il criterio le
        ''' lascerebbe passare tutte: in <c>&amp;nbsp;</c> di lettere ce ne sono quattro.
        ''' Si toglie solo per <i>decidere</i> — quel che resta dentro un pezzo che entra
        ''' non si tocca — e si scarta soltanto ciò che, tolte quelle, non ha più niente da
        ''' dire.</para>
        ''' </remarks>
        Private Shared Function Copione() As String

            Dim massimo As String = MassimoCaratteri.ToString(CultureInfo.InvariantCulture)

            Return "(function () {" &
                   "  function visibile(e) {" &
                   "    var s = window.getComputedStyle(e);" &
                   "    return s && s.display !== 'none' && s.visibility !== 'hidden';" &
                   "  }" &
                   "  function bloccante(e) {" &
                   "    var s = window.getComputedStyle(e);" &
                   "    if (!s || !s.display) return false;" &
                   "    return s.display.indexOf('inline') !== 0 && s.display !== 'contents';" &
                   "  }" &
                   "  function utile(s) {" &
                   "    var t = s.replace(/&[a-zA-Z]+;|&#[0-9]+;/g, ' ');" &
                   "    return /[\p{L}\p{N}]/u.test(t);" &
                   "  }" &
                   "  function metti(pezzi, s) {" &
                   "    var t = s && s.trim();" &
                   "    if (t && utile(t)) pezzi.push(t);" &
                   "  }" &
                   "  function daSaltare(e) {" &
                   "    var g = e.tagName;" &
                   "    return g === 'SCRIPT' || g === 'STYLE' || g === 'NOSCRIPT' || g === 'TEMPLATE';" &
                   "  }" &
                   "  function raccogli(e, pezzi) {" &
                   "    var figli = e.childNodes, dentroCiSonoBlocchi = false;" &
                   "    for (var i = 0; i < figli.length; i++) {" &
                   "      var n = figli[i];" &
                   "      if (n.nodeType !== 1 || daSaltare(n) || !visibile(n)) continue;" &
                   "      if (bloccante(n)) { dentroCiSonoBlocchi = true; break; }" &
                   "    }" &
                   "    if (!dentroCiSonoBlocchi) {" &
                   "      var tutto = e.innerText;" &
                   "      metti(pezzi, tutto);" &
                   "      return;" &
                   "    }" &
                   "    for (var j = 0; j < figli.length; j++) {" &
                   "      var f = figli[j];" &
                   "      if (f.nodeType === 3) {" &
                   "        var suo = f.nodeValue;" &
                   "        metti(pezzi, suo);" &
                   "        continue;" &
                   "      }" &
                   "      if (f.nodeType !== 1 || daSaltare(f) || !visibile(f)) continue;" &
                   "      raccogli(f, pezzi);" &
                   "    }" &
                   "  }" &
                   "  var scelto = '';" &
                   "  try { scelto = (window.getSelection && window.getSelection().toString()) || ''; }" &
                   "  catch (err) { scelto = ''; }" &
                   "  scelto = scelto.replace(/\n{3,}/g, '\n\n').trim();" &
                   "  var daSelezione = scelto.length >= " & MinimoDellaSelezione & ";" &
                   "  var t;" &
                   "  if (daSelezione) { t = scelto; } else {" &
                   "    var pezzi = [];" &
                   "    if (document.body) raccogli(document.body, pezzi);" &
                   "    t = pezzi.join('\n').replace(/\n{3,}/g, '\n\n');" &
                   "  }" &
                   "  return JSON.stringify({" &
                   "    titolo: document.title || ''," &
                   "    indirizzo: location.href || ''," &
                   $"    testo: t.slice(0, {massimo})," &
                   $"    troncato: t.length > {massimo}," &
                   "    selezione: daSelezione" &
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
                letta.DaSelezione = Vero(contenuto, "selezione")

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
