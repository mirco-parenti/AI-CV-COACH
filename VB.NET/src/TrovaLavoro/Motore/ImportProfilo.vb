Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace Motore

    ''' <summary>Perché un import non è andato a buon fine.</summary>
    Public Enum CausaImport
        ''' <summary>Il file indicato non c'è.</summary>
        FileMancante
        ''' <summary>L'estensione non è fra quelle accettate (cap. 05.1).</summary>
        FormatoNonSupportato
        ''' <summary>Il PDF supera il limite dell'API.</summary>
        FileTroppoGrande
        ''' <summary>Il file c'è ma non si lascia leggere: non è un archivio, è corrotto.</summary>
        DocumentoIllegibile
        ''' <summary>
        ''' Dal documento è uscito troppo poco testo per essere un CV: quasi sempre un
        ''' PDF scannerizzato, cioè un'immagine senza testo dentro.
        ''' </summary>
        TestoTroppoCorto
        ''' <summary>L'AI ha risposto qualcosa che non è un profilo.</summary>
        ProfiloIllegibile
    End Enum

    ''' <summary>
    ''' Un import che non si può portare a termine. Il messaggio è già quello da mostrare
    ''' all'utente e, dove esiste, porta con sé il <b>ripiego onesto</b>: dire cosa non si
    ''' riesce a fare e come farlo lo stesso, invece di lasciare un errore muto.
    ''' </summary>
    Public Class ErroreImport
        Inherits Exception

        Public ReadOnly Property Causa As CausaImport

        Public Sub New(causa As CausaImport, messaggio As String, Optional dentro As Exception = Nothing)
            MyBase.New(messaggio, dentro)
            Me.Causa = causa
        End Sub

    End Class

    ''' <summary>
    ''' Cos'è uscito da un import: il profilo proposto e il testo da cui è stato ricavato.
    ''' </summary>
    ''' <remarks>
    ''' Il testo letto viaggia insieme al profilo di proposito: è la fonte con cui
    ''' l'utente può controllare, campo per campo, che non sia stato aggiunto nulla —
    ''' e a T3c il pannello P2 potrà mostrarglielo accanto.
    ''' </remarks>
    Public Class EsitoImport

        ''' <summary>Il profilo ricavato dal documento. Ancora una proposta, non un profilo salvato.</summary>
        Public Property Profilo As Profilo

        ''' <summary>Il testo del CV da cui il profilo è stato ricavato.</summary>
        Public Property TestoLetto As String = ""

        ''' <summary>Da dove viene: il percorso del file, o com'è arrivato il testo.</summary>
        Public Property Origine As String = ""

        ''' <summary>Il formato riconosciuto (<c>NonSupportato</c> per il testo incollato).</summary>
        Public Property Formato As FormatoDocumento = FormatoDocumento.NonSupportato

    End Class

    ''' <summary>
    ''' L'import di un CV già esistente nel profilo (voce 2.1.2): da un file al profilo
    ''' proposto, in due passi — prima il <b>testo</b>, poi la <b>struttura</b>.
    ''' </summary>
    ''' <remarks>
    ''' <para>I due passi sono quelli del prototipo. Il testo si ricava in modo diverso
    ''' a seconda del formato (dal disco, o dall'AI per il PDF); la strutturazione è
    ''' invece sempre la stessa e passa dal turno <c>importa_cv</c> del pool, cioè dallo
    ''' stesso strutturatore che serve il dialogo. Un solo posto dove il testo di un CV
    ''' diventa profilo: da qui passeranno anche il ripiego «incollo il testo a mano» e,
    ''' più avanti, il profilo LinkedIn (T5b, cap. 06.7).</para>
    ''' <para>Qui non si scrive niente su disco e non si tocca il profilo esistente:
    ''' quello che esce è una <i>proposta</i>. A salvarla — dopo che l'utente l'ha vista e
    ''' corretta — ci pensa il pannello con <see cref="ArchivioProfilo"/>. È la stessa
    ''' regola della conferma prima di ogni scrittura (cap. 05.2): niente entra nel
    ''' profilo senza che l'utente l'abbia visto.</para>
    ''' </remarks>
    Public Class ImportProfilo

        ''' <summary>Il turno del pool che struttura il testo di un CV nel profilo.</summary>
        Public Const TurnoImport As String = "importa_cv"

        ''' <summary>
        ''' Sotto quanti caratteri il testo ricavato non è un CV. Non sta in
        ''' <c>taratura.json</c> di proposito: quella è la taratura del <i>match</i>, e
        ''' mescolarci una soglia di lettura confonderebbe due piani diversi. Il numero è
        ''' volutamente basso — deve far scattare il ripiego sul PDF-immagine, che di
        ''' testo ne restituisce pochissimo o niente, non sindacare su un CV breve.
        ''' </summary>
        Public Const CaratteriMinimi As Integer = 200

        Private ReadOnly _strutturatore As IStrutturatoreTurni
        Private ReadOnly _trascrittore As ITrascrittorePdf

        ''' <param name="strutturatore">Chi trasforma il testo del CV nel frammento di profilo.</param>
        ''' <param name="trascrittore">
        ''' Chi ricava il testo da un PDF. Può mancare: senza, tutto il resto funziona
        ''' lo stesso e solo il PDF si ferma dicendo perché.
        ''' </param>
        Public Sub New(strutturatore As IStrutturatoreTurni, Optional trascrittore As ITrascrittorePdf = Nothing)

            If strutturatore Is Nothing Then Throw New ArgumentNullException(NameOf(strutturatore))

            _strutturatore = strutturatore
            _trascrittore = trascrittore

        End Sub

        ''' <summary>
        ''' Importa un CV da un file: PDF, DOCX, TXT o MD. Passo 1 il testo, passo 2 la
        ''' struttura.
        ''' </summary>
        Public Async Function DaFileAsync(percorso As String,
                                          Optional annulla As CancellationToken = Nothing) As Task(Of EsitoImport)

            If String.IsNullOrWhiteSpace(percorso) Then
                Throw New ErroreImport(CausaImport.FileMancante, "Non mi hai detto quale file importare.")
            End If

            Dim nome As String = Path.GetFileName(percorso)
            Dim formato As FormatoDocumento = LettoreDocumenti.Formato(percorso)

            If formato = FormatoDocumento.NonSupportato Then
                Throw New ErroreImport(CausaImport.FormatoNonSupportato,
                    $"«{nome}» non è un formato che so leggere. " &
                    "Posso importare un CV in PDF, DOCX, TXT o MD.")
            End If

            If Not File.Exists(percorso) Then
                Throw New ErroreImport(CausaImport.FileMancante,
                    $"Il file «{percorso}» non esiste.")
            End If

            Dim testo As String = Await TestoDelDocumentoAsync(percorso, nome, formato, annulla).ConfigureAwait(False)

            ' Il ripiego onesto del cap. 05.1: se dal documento non è uscito abbastanza
            ' testo, ci si ferma qui invece di far strutturare il vuoto — che darebbe un
            ' profilo tutto vuoto senza dire perché.
            If testo.Trim().Length < CaratteriMinimi Then
                Throw New ErroreImport(CausaImport.TestoTroppoCorto, MotivoTestoCorto(nome, formato))
            End If

            Dim esito As EsitoImport = Await DaTestoAsync(testo, percorso, annulla).ConfigureAwait(False)
            esito.Formato = formato

            Return esito

        End Function

        ''' <summary>
        ''' Importa un CV da un testo già pronto: è il ripiego quando il documento non si
        ''' lascia leggere («incollalo a mano»), e la porta da cui passerà il profilo
        ''' LinkedIn (T5b).
        ''' </summary>
        ''' <param name="testo">Il testo del CV.</param>
        ''' <param name="origine">Da dove viene, per chi legge l'esito.</param>
        Public Async Function DaTestoAsync(testo As String, Optional origine As String = "testo incollato",
                                           Optional annulla As CancellationToken = Nothing) As Task(Of EsitoImport)

            If String.IsNullOrWhiteSpace(testo) Then
                Throw New ErroreImport(CausaImport.TestoTroppoCorto,
                    "Non c'è nessun testo da importare.")
            End If

            Dim frammento As JsonNode = Await _strutturatore.StrutturaAsync(
                TurnoImport, testo, annulla).ConfigureAwait(False)

            Dim profilo As Profilo
            Try
                profilo = Profilo.DaJson(frammento)
            Catch ex As JsonException
                Throw New ErroreImport(CausaImport.ProfiloIllegibile,
                    "Ho letto il CV, ma quello che l'AI ha risposto non ha la forma di un profilo. " &
                    "Riprova, oppure inserisci i dati con il dialogo guidato.", ex)
            End Try

            Return New EsitoImport With {
                .Profilo = profilo,
                .TestoLetto = testo,
                .Origine = origine}

        End Function

        ''' <summary>Passo 1: il testo del documento, per la via che il formato richiede.</summary>
        Private Async Function TestoDelDocumentoAsync(percorso As String, nome As String,
                                                      formato As FormatoDocumento,
                                                      annulla As CancellationToken) As Task(Of String)

            If formato = FormatoDocumento.Pdf Then

                If _trascrittore Is Nothing Then
                    Throw New ErroreImport(CausaImport.FormatoNonSupportato,
                        "Per leggere un PDF serve l'AI, e qui non è disponibile. " &
                        "Puoi importare il CV in DOCX, TXT o MD, oppure incollarne il testo.")
                End If

                Dim quantoPesa As Long = New FileInfo(percorso).Length
                If quantoPesa > TrascrittorePdf.DimensioneMassima Then
                    Throw New ErroreImport(CausaImport.FileTroppoGrande,
                        $"«{nome}» pesa {quantoPesa \ (1024L * 1024L)} MB e supera il limite di " &
                        $"{TrascrittorePdf.DimensioneMassima \ (1024L * 1024L)} MB per un PDF. " &
                        "Esporta un PDF più leggero, oppure incolla il testo del CV a mano.")
                End If

                Return Await _trascrittore.TrascriviAsync(percorso, annulla).ConfigureAwait(False)

            End If

            Try
                Return LettoreDocumenti.LeggiTesto(percorso)
            Catch ex As InvalidDataException
                Throw New ErroreImport(CausaImport.DocumentoIllegibile, ex.Message, ex)
            Catch ex As IOException
                Throw New ErroreImport(CausaImport.DocumentoIllegibile,
                    $"Non riesco a leggere «{nome}»: {ex.Message}", ex)
            End Try

        End Function

        ''' <summary>Perché il testo è troppo corto, detto in modo utile a seconda del formato.</summary>
        Private Shared Function MotivoTestoCorto(nome As String, formato As FormatoDocumento) As String

            If formato = FormatoDocumento.Pdf Then
                Return $"Da «{nome}» è uscito pochissimo testo: probabilmente è un PDF scannerizzato, " &
                       "cioè un'immagine senza testo dentro. Puoi incollare qui il testo del CV, " &
                       "oppure raccontarmi il tuo percorso con il dialogo guidato."
            End If

            Return $"«{nome}» contiene troppo poco testo per essere un CV. " &
                   "Controlla di aver indicato il file giusto, oppure incolla qui il testo del CV."

        End Function

    End Class

End Namespace
