Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore
Imports TrovaLavoro.Web

Namespace Mcp

    ''' <summary>
    ''' I tool che cambiano qualcosa nella cartella dati (cap. 09.3): mettere una
    ''' candidatura nella coda, e impaginarne i documenti.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Passano tutti dal lucchetto</b> (cap. 09.4, <see cref="LucchettoDati"/>),
    ''' che è ciò che li distingue da tutti gli altri. Se l'applicazione con le finestre è
    ''' aperta il lucchetto ce l'ha lei, e questi tool rispondono con un messaggio onesto
    ''' invece di scrivere sotto i piedi di qualcuno che ha gli stessi dati aperti in
    ''' memoria.</para>
    ''' <para><b>Il lucchetto si prende il più tardi possibile</b>: prima si controllano i
    ''' parametri, che non costano niente, e solo quando c'è davvero qualcosa da scrivere
    ''' si va a bussare. Tenerlo mentre si guarda un JSON malformato vorrebbe dire fermare
    ''' l'applicazione dell'utente per un errore di chi ha chiamato.</para>
    ''' <para><b>Anche i PDF, dal 2026-08-19.</b> Fino a lì di qui uscivano i soli DOCX,
    ''' perché il PDF si stampa nel browser incorporato «che vuole una finestra», e in
    ''' modalità <c>--mcp</c> il programma biforca prima di ogni preparativo grafico
    ''' (cap. 09.2). La frase era imprecisa: quel che il motore pretende non è una finestra
    ''' visibile ma un filo STA con la sua pompa di messaggi, e quello si accende anche qui
    ''' (v. <see cref="FiloGrafico"/>). A smentirla è stato il banco di collaudo, che stampa
    ''' PDF veri da un processo di test dove finestre non ce n'è nessuna.</para>
    ''' </remarks>
    Public Class ToolDiScrittura

        Private ReadOnly _contesto As ContestoApp

        Public Sub New(contesto As ContestoApp)

            If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
            _contesto = contesto

        End Sub

        ''' <summary>
        ''' Mette una candidatura nella coda: l'annuncio, e quel che di quella candidatura
        ''' è già stato prodotto.
        ''' </summary>
        ''' <remarks>
        ''' <para>Prende <b>tutti</b> gli artefatti e non il solo annuncio, perché
        ''' altrimenti quel che i tool di T8b producono non avrebbe dove andare: si
        ''' potrebbe generare un CV via MCP e non poterlo mettere da nessuna parte, e
        ''' <c>esporta_documento</c> non avrebbe mai niente da impaginare.</para>
        ''' <para><b>Le stelle si ricalcolano qui</b> e non si accettano da fuori. Se
        ''' arriva un confronto, il punteggio lo rifà <see cref="CalcoloMatch"/> dai
        ''' giudizi: è la stessa regola del tool <c>confronta</c> (cap. 09.5) — il
        ''' punteggio è del programma, e prenderlo per buono da chi lo consegna vorrebbe
        ''' dire lasciarlo negoziare a parole proprio nel punto in cui finisce su disco.</para>
        ''' </remarks>
        Public Function SalvaOpportunita(argomenti As JsonObject,
                                         annulla As CancellationToken) As Task(Of EsitoTool)

            Dim annuncio As JsonNode = CampiJson.Nodo(argomenti, "annuncio")
            If annuncio Is Nothing Then
                Return Task.FromResult(EsitoTool.NonRiuscito(
                    "Manca il parametro «annuncio»: ci vuole l'annuncio strutturato, come lo " &
                    "restituisce analizza_annuncio."))
            End If

            Dim confronto As JsonNode = CampiJson.Nodo(argomenti, "confronto")
            Dim cv As JsonNode = CampiJson.Nodo(argomenti, "cv")
            Dim lettera As JsonNode = CampiJson.Nodo(argomenti, "lettera")

            Dim nata As New Opportunita With {
                .Annuncio = annuncio,
                .Confronto = confronto,
                .Mitigazioni = CampiJson.Nodo(argomenti, "mitigazioni"),
                .Appunti = CampiJson.Nodo(argomenti, "appunti"),
                .Cv = cv,
                .Lettera = lettera,
                .Fonte = "MCP",
                .Lingua = LinguaDocumenti.PerDocumenti(
                    If(CampiJson.Testo(argomenti, "lingua"),
                       CampiJson.Testo(TryCast(annuncio, JsonObject), "lingua")))}

            If confronto IsNot Nothing Then
                nata.Match = CalcoloMatch.Calcola(
                    Opportunita.GiudiziDi(confronto),
                    PipelineCandidatura.NumeroComplessivo(confronto),
                    _contesto.Taratura)
            End If

            ' Dei documenti senza un confronto sono una storia che non può essere successa:
            ' un CV mirato *nasce* dai giudizi, e genera_cv senza di quelli scrive il CV
            ' base, che appartiene al profilo e non a una candidatura. Meglio dirlo che
            ' mettere in coda una candidatura che nessuno saprebbe più spiegare.
            If (cv IsNot Nothing OrElse lettera IsNot Nothing) AndAlso confronto Is Nothing Then
                Return Task.FromResult(EsitoTool.NonRiuscito(
                    "Ci sono dei documenti ma non il confronto da cui nascono: passa anche " &
                    "«confronto», quello che restituisce confronta. Una candidatura non può " &
                    "avere un CV mirato senza essere mai stata confrontata."))
            End If

            ' Lo stato lo dice quel che c'è, non chi chiama — e si avanza per gradi, perché
            ' la macchina degli stati non si scavalca (cap. 07.3): una candidatura passa da
            ' «interessante» prima di essere «generata», anche quando arriva tutta insieme.
            If confronto IsNot Nothing Then nata.Avanza(StatoOpportunita.Interessante)
            If cv IsNot Nothing OrElse lettera IsNot Nothing Then nata.Avanza(StatoOpportunita.Generata)

            Using preso As LucchettoDati = LucchettoDati.Prendi(_contesto.Cartella)

                If preso Is Nothing Then Return Task.FromResult(LAppETuttaSua())

                Try
                    Dim dove As String = _contesto.Opportunita.Salva(nata)

                    Dim detto As New JsonObject From {
                        {"cartella", Path.GetFileName(dove)},
                        {"stato", nata.Stato.ToString().ToLowerInvariant()}}

                    If nata.Match IsNot Nothing Then detto("match") = nata.Match.ComeJson()

                    Return Task.FromResult(EsitoTool.Riuscito(detto))

                Catch ex As Exception When TypeOf ex Is IOException OrElse
                                           TypeOf ex Is UnauthorizedAccessException
                    Return Task.FromResult(EsitoTool.NonRiuscito(
                        $"Non sono riuscito a salvare la candidatura: {ex.Message}"))
                End Try

            End Using

        End Function

        ''' <summary>
        ''' Impagina il CV e la lettera di una candidatura già salvata: DOCX, PDF o tutti
        ''' e due.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>Chi non dice il formato li vuole tutti e due</b>, ed è la stessa regola
        ''' dei testi rifiniti (cap. 09.3): quel che esce da qui dev'essere quel che si
        ''' otterrebbe dalla finestra, dove i due bottoni ci sono entrambi. Il prezzo lo paga
        ''' chi chiede il PDF — accendere il motore del browser costa secondi — e per questo
        ''' «docx» resta a portata di chi ha fretta.</para>
        ''' <para><b>Il ripiego è onesto e non silenzioso</b>: se il motore non c'è o non si
        ''' accende, i DOCX sono già sul disco e si consegnano dicendo perché il PDF manca.
        ''' Chi aveva chiesto il solo PDF invece non riceve niente, ed è giusto che lo senta
        ''' come un fallimento.</para>
        ''' </remarks>
        Public Async Function EsportaDocumento(argomenti As JsonObject,
                                               annulla As CancellationToken) As Task(Of EsitoTool)

            Dim nome As String = Nothing
            Dim rifiuto As EsitoTool = ToolDiLettura.NomeDiCartellaAmmesso(
                CampiJson.Testo(argomenti, "cartella"), nome)
            If rifiuto IsNot Nothing Then Return rifiuto

            Dim voluti As FormatiDocumento
            rifiuto = FormatiAmmessi(CampiJson.Testo(argomenti, "formati"), voluti)
            If rifiuto IsNot Nothing Then Return rifiuto

            Dim dove As String = Path.Combine(_contesto.Cartella.CartellaOpportunita, nome)

            If Not Directory.Exists(dove) Then
                Return EsitoTool.NonRiuscito(
                    $"L'opportunità «{nome}» non c'è. L'elenco di quelle che ci sono lo dà " &
                    "leggi_registro; una nuova la mette in coda salva_opportunita.")
            End If

            Using preso As LucchettoDati = LucchettoDati.Prendi(_contesto.Cartella)

                If preso Is Nothing Then Return LAppETuttaSua()

                Try
                    Dim candidatura As Opportunita = _contesto.Opportunita.Carica(dove)

                    If candidatura.Cv Is Nothing AndAlso candidatura.Lettera Is Nothing Then
                        Return EsitoTool.NonRiuscito(
                            $"L'opportunità «{nome}» non ha ancora né CV né lettera da impaginare: " &
                            "si scrivono con genera_cv e genera_lettera, e si mettono nella " &
                            "candidatura con salva_opportunita.")
                    End If

                    Dim scritti As IReadOnlyList(Of String)
                    Dim ripiego As String = Nothing

                    If voluti = FormatiDocumento.Docx Then

                        ' Senza stampante si scrivono i soli DOCX: è l'archivio stesso a
                        ' dirlo, e qui è quel che ci hanno chiesto.
                        scritti = Await New ArchivioDocumenti(_contesto.Cartella).
                            ScriviCandidaturaAsync(candidatura, voluti).ConfigureAwait(False)

                    Else

                        Dim stampati As IReadOnlyList(Of String) = Nothing
                        Dim guasto As ErroreStampa = Nothing

                        ' Il motore del browser e la stampante nascono e muoiono **dentro**
                        ' il filo: fuori di lì non sarebbero di nessuno (v. FiloGrafico, e
                        ' MotoreBrowser sul perché di un thread solo).
                        Await FiloGrafico.EseguiAsync(
                            Async Function() As Task
                                Try
                                    Using stampante As New StampantePdf(
                                        New MotoreBrowser(_contesto.Cartella.CartellaWebView2))

                                        stampati = Await New ArchivioDocumenti(_contesto.Cartella, stampante).
                                            ScriviCandidaturaAsync(candidatura, voluti).ConfigureAwait(True)

                                    End Using
                                Catch ex As ErroreStampa
                                    guasto = ex
                                End Try
                            End Function).ConfigureAwait(False)

                        If guasto Is Nothing Then
                            scritti = stampati

                        ElseIf voluti = FormatiDocumento.Pdf Then
                            ' Chiedeva il solo PDF: non è rimasto niente da consegnare, e
                            ' chiamarlo «riuscito» sarebbe una bugia gentile.
                            Return EsitoTool.NonRiuscito(guasto.Message)

                        Else
                            ' I DOCX erano già scritti quando la stampa è caduta — l'archivio
                            ' fa prima quelli — ma l'elenco se n'è andato con l'eccezione: si
                            ' riscrivono per sapere **quali** sono. Costa un attimo, e vale un
                            ' elenco esatto invece di uno indovinato.
                            scritti = Await New ArchivioDocumenti(_contesto.Cartella).
                                ScriviCandidaturaAsync(candidatura, FormatiDocumento.Docx).ConfigureAwait(False)
                            ripiego = guasto.Message
                        End If

                    End If

                    Dim nomi As New JsonArray()
                    For Each prodotto As String In scritti
                        nomi.Add(Path.GetFileName(prodotto))
                    Next

                    Dim risposta As New JsonObject From {
                        {"cartella", nome},
                        {"documenti", nomi},
                        {"formato", NomeFormato(If(ripiego Is Nothing, voluti, FormatiDocumento.Docx))}}

                    If ripiego IsNot Nothing Then risposta.Add("nota", ripiego)

                    Return EsitoTool.Riuscito(risposta)

                Catch ex As Exception When TypeOf ex Is IOException OrElse
                                           TypeOf ex Is UnauthorizedAccessException OrElse
                                           TypeOf ex Is DirectoryNotFoundException
                    Return EsitoTool.NonRiuscito(
                        $"Non sono riuscito a impaginare i documenti di «{nome}»: {ex.Message}")
                End Try

            End Using

        End Function

        ''' <summary>
        ''' Quali formati vuole chi ha chiamato. <b>Chi non lo dice li vuole tutti e due</b>:
        ''' il tool si chiama «impagina i documenti», e chi lo chiama senza aggiungere altro
        ''' vuole i file finiti, gli stessi che gli darebbe la finestra.
        ''' </summary>
        ''' <returns>
        ''' <c>Nothing</c> se la richiesta è buona — e allora <paramref name="formati"/> dice
        ''' quali —, il rifiuto da consegnare se il nome del formato non esiste.
        ''' </returns>
        Private Shared Function FormatiAmmessi(chiesto As String,
                                               ByRef formati As FormatiDocumento) As EsitoTool

            formati = FormatiDocumento.Entrambi
            If String.IsNullOrWhiteSpace(chiesto) Then Return Nothing

            Select Case chiesto.Trim().ToLowerInvariant()

                Case "docx"
                    formati = FormatiDocumento.Docx

                Case "pdf"
                    formati = FormatiDocumento.Pdf

                Case "entrambi"
                    formati = FormatiDocumento.Entrambi

                Case Else
                    Return EsitoTool.NonRiuscito(
                        $"Il formato «{chiesto}» non esiste. Si può chiedere «docx», «pdf» o " &
                        "«entrambi» — e se non lo chiedi, entrambi.")

            End Select

            Return Nothing

        End Function

        ''' <summary>Come si chiama, nella risposta, quel che è stato scritto davvero.</summary>
        Private Shared Function NomeFormato(formati As FormatiDocumento) As String

            Select Case formati
                Case FormatiDocumento.Docx
                    Return "docx"
                Case FormatiDocumento.Pdf
                    Return "pdf"
                Case Else
                    Return "entrambi"
            End Select

        End Function

        ''' <summary>
        ''' Quando il lucchetto ce l'ha qualcun altro. La frase dice <b>che cosa fare</b>,
        ''' non solo che è andata male: chi legge è un modello, e da «chiudi la finestra
        ''' oppure usa i tool di lettura» sa scegliere da sé la strada che gli resta.
        ''' </summary>
        Private Shared Function LAppETuttaSua() As EsitoTool

            Return EsitoTool.NonRiuscito(
                "La cartella dati è in uso da un altro processo — quasi sempre " &
                "l'applicazione TrovaLavoro aperta sullo schermo, che tiene i suoi dati in " &
                "memoria e li riscriverebbe sopra i miei. Chiudi la finestra e riprova. " &
                "Tutti i tool che leggono (leggi_profilo, leggi_registro, leggi_opportunita) " &
                "e quelli che passano dall'AI funzionano lo stesso: è solo la scrittura a " &
                "essere di uno alla volta.")

        End Function

    End Class

End Namespace
