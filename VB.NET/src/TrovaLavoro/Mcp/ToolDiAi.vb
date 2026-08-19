Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Mcp

    ''' <summary>
    ''' I tool che passano dall'AI (cap. 09.3): l'analisi di un annuncio, il confronto
    ''' col profilo, le mitigazioni, la lettura di un CV, i documenti, la passata
    ''' anti-slop. Sono gli stessi mestieri che lavorano dentro l'applicazione, con gli
    ''' stessi prompt del pool e gli stessi limiti etici.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Non salvano niente.</b> Prendono JSON, restituiscono JSON: chi chiama
    ''' decide che farne. L'unica cosa che leggono da disco è il <b>profilo</b>, che è
    ''' dell'utente e non si passa come parametro — un CV si scrive dai fatti che stanno
    ''' in quel file, non da quelli che un modello riferisce di ricordare.</para>
    ''' <para><b>Il profilo si serializza come lo serializza l'applicazione</b>, passando
    ''' dall'oggetto e non dal file grezzo. Non è un dettaglio: <c>confronto</c> e
    ''' <c>mitigazione</c> sono i due prompt su cui vale ancora la parità carattere per
    ''' carattere col prototipo (cap. 04.7), e basterebbe una virgola in più nel testo che
    ''' parte per perderla senza accorgersene.</para>
    ''' <para><b>I documenti escono rifiniti</b>, come quelli dell'applicazione: la
    ''' passata anti-slop (cap. 08) è dentro la generazione anche qui. Un CV chiesto via
    ''' MCP dev'essere lo stesso CV che si otterrebbe dalla finestra — una differenza di
    ''' qualità fra le due porte non la dichiarerebbe nessuno, e si scoprirebbe mesi dopo
    ''' confrontando due documenti senza capire perché uno è più piatto.</para>
    ''' <para><b>Un lavoro ritirato non si tampona.</b> L'annullamento risale al ciclo
    ''' (cap. 09.2), che sa che a una richiesta ritirata non si risponde: qui non lo si
    ''' cattura, o diventerebbe un guasto da raccontare a un client che non sta più
    ''' ascoltando.</para>
    ''' </remarks>
    Public Class ToolDiAi

        Private ReadOnly _contesto As ContestoApp

        Public Sub New(contesto As ContestoApp)

            If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
            _contesto = contesto

        End Sub

#Region "I sette tool"

        ''' <summary>Dal testo di un annuncio all'annuncio strutturato (anello 2).</summary>
        Public Async Function AnalizzaAnnuncio(argomenti As JsonObject,
                                               annulla As CancellationToken) As Task(Of EsitoTool)

            Dim guasto As EsitoTool = SeLAiNonCE()
            If guasto IsNot Nothing Then Return guasto

            Dim testo As String = CampiJson.Testo(argomenti, "testo")
            If String.IsNullOrWhiteSpace(testo) Then
                Return Manca("testo", "il testo dell'annuncio, copiato dal portale")
            End If

            Try
                ' Si passa dalla fila vera, non dal solo mestiere: così l'annuncio esce
                ' identico a quello che l'applicazione mette in una candidatura, lingua
                ' proposta compresa. L'opportunità che ne nasce si butta — qui non si
                ' salva niente.
                Dim nata As Opportunita = Await _contesto.Pipeline.AnalizzaAsync(testo, annulla).
                    ConfigureAwait(False)

                Return EsitoTool.Riuscito(nata.Annuncio)

            Catch ex As OperationCanceledException
                Throw
            Catch ex As ErroreAi
                Return NonCeLHaFatta("analizzare l'annuncio", ex)
            End Try

        End Function

        ''' <summary>
        ''' Il profilo davanti a un annuncio: i giudizi voce per voce e il punteggio.
        ''' </summary>
        ''' <remarks>
        ''' Le <b>stelle non passano dall'AI</b>: le calcola il programma dai giudizi, col
        ''' clamp e col tetto del requisito eliminatorio (cap. 09.5). È il motivo per cui
        ''' questo tool non si limita a girare la risposta del modello — e per cui il
        ''' punteggio non si può convincere a parole.
        ''' </remarks>
        Public Async Function Confronta(argomenti As JsonObject,
                                        annulla As CancellationToken) As Task(Of EsitoTool)

            Dim guasto As EsitoTool = SeLAiNonCE()
            If guasto IsNot Nothing Then Return guasto

            Dim annuncio As JsonNode = CampiJson.Nodo(argomenti, "annuncio")
            If annuncio Is Nothing Then
                Return Manca("annuncio", "l'annuncio strutturato, come lo restituisce analizza_annuncio")
            End If

            Dim profilo As JsonNode = Nothing
            guasto = ProfiloPerIPrompt(profilo)
            If guasto IsNot Nothing Then Return guasto

            Try
                Dim confronto As JsonNode = Await _contesto.Confrontatore.ConfrontaAsync(
                    profilo, annuncio, annulla).ConfigureAwait(False)

                Dim punteggio As RisultatoMatch = CalcoloMatch.Calcola(
                    Opportunita.GiudiziDi(confronto),
                    PipelineCandidatura.NumeroComplessivo(confronto),
                    _contesto.Taratura)

                ' Il confronto com'è uscito dall'AI, e accanto il punteggio che è nostro:
                ' due cose diverse, e chi legge deve poterle distinguere.
                Return EsitoTool.Riuscito(New JsonObject From {
                    {"confronto", confronto},
                    {"match", punteggio.ComeJson()}})

            Catch ex As OperationCanceledException
                Throw
            Catch ex As ErroreAi
                Return NonCeLHaFatta("confrontare il profilo con l'annuncio", ex)
            End Try

        End Function

        ''' <summary>
        ''' I ponti onesti sui gap che i giudizi hanno trovato.
        ''' </summary>
        ''' <remarks>
        ''' Se nessun giudizio è «in parte» o «non soddisfatto» la lista esce vuota
        ''' <b>senza chiedere niente all'AI</b>: uscirebbe vuota comunque, e costerebbe
        ''' un'attesa e dei token per sapere una cosa che si sa già. È la stessa scelta
        ''' che fa la fila dentro l'applicazione, ed è deterministica.
        ''' </remarks>
        Public Async Function Mitiga(argomenti As JsonObject,
                                     annulla As CancellationToken) As Task(Of EsitoTool)

            Dim guasto As EsitoTool = SeLAiNonCE()
            If guasto IsNot Nothing Then Return guasto

            Dim giudizi As JsonArray = TryCast(CampiJson.Nodo(argomenti, "giudizi"), JsonArray)
            If giudizi Is Nothing Then
                Return Manca("giudizi",
                             "la lista dei giudizi, che sta nel campo «giudizi» della risposta di confronta")
            End If

            If Not PipelineCandidatura.CiSonoGap(giudizi) Then
                Return EsitoTool.Riuscito(New JsonObject From {{"mitigazioni", New JsonArray()}})
            End If

            Dim profilo As JsonNode = Nothing
            guasto = ProfiloPerIPrompt(profilo)
            If guasto IsNot Nothing Then Return guasto

            Try
                Return EsitoTool.Riuscito(Await _contesto.Confrontatore.MitigaAsync(
                    profilo, giudizi, annulla).ConfigureAwait(False))

            Catch ex As OperationCanceledException
                Throw
            Catch ex As ErroreAi
                Return NonCeLHaFatta("cercare le mitigazioni", ex)
            End Try

        End Function

        ''' <summary>
        ''' Il testo di un CV letto e messo in forma di profilo. <b>Non lo salva</b>: è
        ''' una proposta da guardare, e sostituire il profilo è un'azione irreversibile
        ''' che resta nell'applicazione, dove c'è una conferma davanti agli occhi
        ''' (cap. 09.3).
        ''' </summary>
        Public Async Function StrutturaCv(argomenti As JsonObject,
                                          annulla As CancellationToken) As Task(Of EsitoTool)

            Dim guasto As EsitoTool = SeLAiNonCE()
            If guasto IsNot Nothing Then Return guasto

            Dim testo As String = CampiJson.Testo(argomenti, "testo")
            If String.IsNullOrWhiteSpace(testo) Then
                Return Manca("testo", "il testo del CV, trascritto o incollato")
            End If

            Try
                Dim letto As EsitoImport = Await _contesto.ImportCv.DaTestoAsync(
                    testo, "testo passato via MCP", annulla).ConfigureAwait(False)

                Return EsitoTool.Riuscito(New JsonObject From {
                    {"profilo_proposto", JsonNode.Parse(letto.Profilo.ComeTesto())},
                    {"salvato", False}})

            Catch ex As OperationCanceledException
                Throw
            Catch ex As ErroreImport
                ' Il messaggio è già scritto per essere letto da una persona, e va bene
                ' anche per un modello: dice cosa non è andato e cosa si può riprovare.
                Return EsitoTool.NonRiuscito(ex.Message)
            Catch ex As ErroreAi
                Return NonCeLHaFatta("leggere il CV", ex)
            End Try

        End Function

        ''' <summary>
        ''' Il CV: 🎯 <b>mirato</b> se arrivano annuncio e giudizi, 📄 <b>base</b> se non
        ''' arriva niente. Esce già rifinito.
        ''' </summary>
        Public Async Function GeneraCv(argomenti As JsonObject,
                                       annulla As CancellationToken) As Task(Of EsitoTool)

            Dim guasto As EsitoTool = SeLAiNonCE()
            If guasto IsNot Nothing Then Return guasto

            Dim annuncio As JsonNode = CampiJson.Nodo(argomenti, "annuncio")
            Dim giudizi As JsonNode = CampiJson.Nodo(argomenti, "giudizi")
            Dim lingua As String = LinguaChiesta(argomenti)

            ' Uno solo dei due non basta e non è un dettaglio da indovinare: mirare senza
            ' sapere dove il profilo regge e dove no vorrebbe dire mettere in risalto a
            ' caso. Meglio dirlo che produrre un documento che sembra mirato e non lo è.
            ' Il controllo viene prima di leggere il profilo, come negli altri tool: si
            ' risponde sul pezzo più vicino a quel che chi ha chiamato ha scritto.
            If (annuncio Is Nothing) <> (giudizi Is Nothing) Then
                Return EsitoTool.NonRiuscito(
                    "Per il CV mirato servono tutti e due: «annuncio» e «giudizi» (li dà confronta, " &
                    "nel campo «giudizi» del confronto). Senza nessuno dei due esce il CV base, " &
                    "che nasce dal solo profilo.")
            End If

            Dim profilo As JsonNode = Nothing
            guasto = ProfiloPerIPrompt(profilo)
            If guasto IsNot Nothing Then Return guasto

            Try
                Dim cv As JsonNode

                If annuncio Is Nothing Then
                    cv = Await _contesto.Generatore.GeneraCvBaseAsync(profilo, annulla, lingua).
                        ConfigureAwait(False)
                Else
                    cv = Await _contesto.Generatore.GeneraCvMiratoAsync(
                        profilo, annuncio, giudizi, annulla, lingua,
                        CampiJson.Nodo(argomenti, "appunti")).ConfigureAwait(False)
                End If

                Await RifinisciIlCv(cv, lingua, annulla).ConfigureAwait(False)

                Return EsitoTool.Riuscito(New JsonObject From {
                    {"cv", cv},
                    {"mirato", annuncio IsNot Nothing},
                    {"lingua", lingua}})

            Catch ex As OperationCanceledException
                Throw
            Catch ex As ErroreAi
                Return NonCeLHaFatta("scrivere il CV", ex)
            End Try

        End Function

        ''' <summary>La lettera di presentazione, coerente col CV mirato. Esce già rifinita.</summary>
        Public Async Function GeneraLettera(argomenti As JsonObject,
                                            annulla As CancellationToken) As Task(Of EsitoTool)

            Dim guasto As EsitoTool = SeLAiNonCE()
            If guasto IsNot Nothing Then Return guasto

            Dim annuncio As JsonNode = CampiJson.Nodo(argomenti, "annuncio")
            If annuncio Is Nothing Then Return Manca("annuncio", "l'annuncio strutturato")

            Dim giudizi As JsonNode = CampiJson.Nodo(argomenti, "giudizi")
            If giudizi Is Nothing Then Return Manca("giudizi", "la lista dei giudizi che dà confronta")

            Dim cv As JsonNode = CampiJson.Nodo(argomenti, "cv")
            If cv Is Nothing Then
                Return Manca("cv", "il 🎯 CV mirato già generato: la lettera deve raccontare la stessa storia")
            End If

            Dim profilo As JsonNode = Nothing
            guasto = ProfiloPerIPrompt(profilo)
            If guasto IsNot Nothing Then Return guasto

            Dim lingua As String = LinguaChiesta(argomenti)

            Try
                ' Le mitigazioni sono facoltative: senza, la lettera si scrive lo stesso e
                ' tace sui gap. Bloccarla per la loro assenza sarebbe un capriccio, e il
                ' prototipo non lo faceva.
                Dim lettera As JsonNode = Await _contesto.Generatore.GeneraLetteraAsync(
                    profilo, annuncio, giudizi, cv, CampiJson.Nodo(argomenti, "mitigazioni"),
                    annulla, lingua, CampiJson.Nodo(argomenti, "appunti")).ConfigureAwait(False)

                If _contesto.Rifinitura IsNot Nothing Then
                    Await _contesto.Rifinitura.DellaLetteraAsync(lettera, lingua, annulla).
                        ConfigureAwait(False)
                End If

                Return EsitoTool.Riuscito(New JsonObject From {
                    {"lettera", lettera},
                    {"lingua", lingua}})

            Catch ex As OperationCanceledException
                Throw
            Catch ex As ErroreAi
                Return NonCeLHaFatta("scrivere la lettera", ex)
            End Try

        End Function

        ''' <summary>La passata anti-slop su un testo sciolto (cap. 08).</summary>
        Public Async Function RifinisciTesto(argomenti As JsonObject,
                                             annulla As CancellationToken) As Task(Of EsitoTool)

            Dim guasto As EsitoTool = SeLAiNonCE()
            If guasto IsNot Nothing Then Return guasto

            Dim testo As String = CampiJson.Testo(argomenti, "testo")
            If String.IsNullOrWhiteSpace(testo) Then
                Return Manca("testo", "il testo di prosa da ripulire")
            End If

            Dim lingua As String = LinguaChiesta(argomenti)

            Try
                Dim rifinito As String = Await _contesto.Rifinitura.DelTestoAsync(
                    testo, lingua, annulla).ConfigureAwait(False)

                ' Che non sia cambiato niente è un esito legittimo, non un fallimento: un
                ' testo già pulito resta com'è, e dirlo esplicitamente evita a chi ha
                ' chiamato di doverlo dedurre confrontando due stringhe.
                Return EsitoTool.Riuscito(New JsonObject From {
                    {"testo", rifinito},
                    {"cambiato", Not String.Equals(testo, rifinito, StringComparison.Ordinal)},
                    {"lingua", lingua}})

            Catch ex As OperationCanceledException
                Throw
            Catch ex As ErroreAi
                Return NonCeLHaFatta("rifinire il testo", ex)
            End Try

        End Function

#End Region

#Region "Quel che serve a tutti"

        ''' <summary>
        ''' Se manca qualcosa per poter chiamare l'AI, la frase da rispondere; altrimenti
        ''' <c>Nothing</c>.
        ''' </summary>
        ''' <remarks>
        ''' I tool restano <b>in vetrina anche senza chiave</b> e falliscono dicendo
        ''' perché (cap. 09.3). Toglierli dall'elenco sarebbe peggio in due modi: il
        ''' client tiene la vetrina da parte e non gli abbiamo promesso nessun avviso di
        ''' cambiamento, e un tool assente si annuncia come «Unknown tool», che a un
        ''' modello dice «hai sbagliato nome» — mandandolo a cercare l'errore dove non è.
        ''' </remarks>
        Private Function SeLAiNonCE() As EsitoTool

            If _contesto.Client Is Nothing Then
                Return EsitoTool.NonRiuscito(
                    "Non c'è nessuna chiave API configurata, quindi i tool che passano dall'AI non " &
                    "possono lavorare. La chiave si inserisce nelle Impostazioni dell'applicazione. " &
                    "I tool di lettura (leggi_profilo, leggi_registro, leggi_opportunita) funzionano " &
                    "lo stesso.")
            End If

            If _contesto.Libreria Is Nothing Then
                Return EsitoTool.NonRiuscito(
                    "La libreria dei prompt non si è aperta, quindi non c'è nessun prompt da usare. " &
                    "È un guasto dell'installazione: il diario del server su stderr dice cosa non è " &
                    "andato. I tool di lettura funzionano lo stesso.")
            End If

            Return Nothing

        End Function

        ''' <summary>
        ''' Il profilo dell'utente nella forma in cui i prompt lo ricevono, o la frase da
        ''' rispondere se non c'è.
        ''' </summary>
        ''' <param name="profilo">Il profilo, se è andata bene.</param>
        ''' <returns><c>Nothing</c> se si può proseguire.</returns>
        Private Function ProfiloPerIPrompt(ByRef profilo As JsonNode) As EsitoTool

            profilo = Nothing

            If Not _contesto.Archivio.Esiste Then
                Return EsitoTool.NonRiuscito(
                    "Non c'è ancora nessun profilo in questa cartella dati, e senza non si può " &
                    "né confrontare né scrivere niente: un CV nasce dai fatti del profilo, non da " &
                    "quelli raccontati in chat. Si costruisce dall'applicazione, importando un CV " &
                    "o rispondendo ai turni del dialogo.")
            End If

            Try
                ' Si passa dall'oggetto e non dal file grezzo: è la stessa serializzazione
                ' che l'applicazione manda ai prompt, e su confronto e mitigazione la
                ' parità col prototipo si misura carattere per carattere (cap. 04.7).
                profilo = JsonNode.Parse(_contesto.Archivio.Carica().ComeTesto())
                Return Nothing

            Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                Return EsitoTool.NonRiuscito($"Il profilo c'è ma non si è lasciato leggere: {ex.Message}")
            End Try

        End Function

        ''' <summary>La lingua chiesta, normalizzata come fa l'applicazione: italiano o inglese.</summary>
        Private Shared Function LinguaChiesta(argomenti As JsonObject) As String
            Return LinguaDocumenti.PerDocumenti(CampiJson.Testo(argomenti, "lingua"))
        End Function

        ''' <summary>
        ''' La passata anti-slop sul CV appena scritto. Il documento si modifica dov'è —
        ''' la rifinitura restituisce i testi di <i>prima</i>, che qui non servono a
        ''' nessuno: non c'è nessuna finestra in cui mostrare il confronto.
        ''' </summary>
        Private Async Function RifinisciIlCv(cv As JsonNode, lingua As String,
                                             annulla As CancellationToken) As Task

            If _contesto.Rifinitura Is Nothing Then Return

            Await _contesto.Rifinitura.DelCvAsync(cv, lingua, annulla).ConfigureAwait(False)

        End Function

        ''' <summary>Un parametro che ci vuole e non è arrivato.</summary>
        Private Shared Function Manca(nome As String, chiCosE As String) As EsitoTool

            Return EsitoTool.NonRiuscito($"Manca il parametro «{nome}»: ci vuole {chiCosE}.")

        End Function

        ''' <summary>
        ''' Un guasto dell'AI raccontato a chi ha chiamato. Il messaggio di
        ''' <see cref="ErroreAi"/> è già scritto per essere letto e dice se conviene
        ''' riprovare: qui si aggiunge solo cosa si stava facendo, che da fuori non si
        ''' vede.
        ''' </summary>
        Private Shared Function NonCeLHaFatta(cosaSiStavaFacendo As String, guasto As ErroreAi) As EsitoTool

            Return EsitoTool.NonRiuscito($"Non sono riuscito a {cosaSiStavaFacendo}: {guasto.Message}")

        End Function

#End Region

    End Class

End Namespace
