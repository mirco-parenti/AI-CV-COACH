Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Dati

    ''' <summary>
    ''' Il 📄 CV base ritrovato su disco, con l'etichetta di dove viene: da quale versione
    ''' di profilo è nato e quando. Serve a poter dire «questo CV è di una versione
    ''' precedente» invece di rigenerarlo di soppiatto (cap. 11.1).
    ''' </summary>
    Public Class CvBase

        ''' <summary>Il CV vero e proprio, come l'ha prodotto la generazione.</summary>
        Public Property Cv As JsonNode

        ''' <summary>La versione di profilo da cui è nato.</summary>
        Public Property VersioneProfilo As String

        ''' <summary>Quando è stato generato.</summary>
        Public Property Generato As Date

    End Class

    ''' <summary>
    ''' Il profilo su disco: lo legge, lo salva e a ogni salvataggio ne conserva una
    ''' copia datata nello storico (cap. 11.1). È il custode dell'unica cosa che
    ''' l'utente non può rifare da capo: le altre — annunci, giudizi, documenti — si
    ''' rigenerano, il suo racconto no.
    ''' </summary>
    ''' <remarks>
    ''' Due cautele, entrambe con la stessa ragione dietro:
    ''' <list type="bullet">
    ''' <item>si scrive <b>in modo atomico</b> (file accanto, poi spostamento), così
    ''' un'interruzione a metà lascia il profilo vecchio intatto invece di uno mezzo
    ''' scritto;</item>
    ''' <item>la copia nello storico si scrive <b>prima</b> di sostituire il profilo
    ''' corrente: se il disco è pieno, o qualcosa va storto, ci si ferma con il profilo
    ''' buono ancora al suo posto e senza un buco nella storia.</item>
    ''' </list>
    ''' Un profilo illeggibile <b>non</b> ripiega su un profilo vuoto — al contrario di
    ''' taratura e modelli (cap. 11.6), dove il ripiego è la cosa giusta. Qui un ripiego
    ''' silenzioso sarebbe il modo peggiore di perdere i dati dell'utente: meglio
    ''' l'errore in faccia, con il file ancora lì da recuperare a mano.
    ''' </remarks>
    Public Class ArchivioProfilo

        ''' <summary>UTF-8 <b>senza BOM</b>, come il manifest del pool: un JSON con il BOM davanti dà noia agli altri strumenti.</summary>
        Private Shared ReadOnly Codifica As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        ''' <summary>Rientri e accenti in chiaro: l'utente deve poter leggere i suoi dati senza l'app (cap. 11.1).</summary>
        Private Shared ReadOnly FormatoLeggibile As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        Private ReadOnly _cartella As CartellaDati

        ''' <summary>Apre l'archivio sulla cartella dati indicata.</summary>
        Public Sub New(cartella As CartellaDati)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            _cartella = cartella

        End Sub

        ''' <summary>
        ''' Se un profilo c'è già. È la domanda del primo avvio (cap. 12, A2): senza
        ''' profilo si apre il bivio «Ho già un CV» / «Costruiamolo insieme».
        ''' </summary>
        Public ReadOnly Property Esiste As Boolean
            Get
                Return File.Exists(_cartella.FileProfilo)
            End Get
        End Property

        ''' <summary>
        ''' Quando il profilo corrente è stato scritto l'ultima volta; <c>Nothing</c> se
        ''' un profilo non c'è ancora. È la domanda «aggiornato quando?» che il cruscotto
        ''' e la scheda del profilo fanno all'archivio (cap. 03.6): il pannello non va a
        ''' guardare il disco per conto suo.
        ''' </summary>
        Public ReadOnly Property UltimoSalvataggio As Date?
            Get
                If Not Esiste Then Return Nothing
                Return File.GetLastWriteTime(_cartella.FileProfilo)
            End Get
        End Property

        ''' <summary>
        ''' Il profilo corrente. Se il file non c'è solleva <see cref="FileNotFoundException"/>
        ''' e se è illeggibile una <see cref="Text.Json.JsonException"/>: chi chiama deve
        ''' distinguere «non l'ho ancora fatto» da «c'è ma è rotto», che sono due
        ''' situazioni diversissime per l'utente.
        ''' </summary>
        Public Function Carica() As Profilo
            Return Profilo.DaTesto(File.ReadAllText(_cartella.FileProfilo, Encoding.UTF8))
        End Function

        ''' <summary>
        ''' Salva il profilo come versione confermata: la copia datata nello storico e
        ''' poi il profilo corrente.
        ''' </summary>
        ''' <returns>
        ''' Il nome della versione appena archiviata (es. <c>2026-08-07_153012</c>). È
        ''' ciò che ogni opportunità annota per dire con quale profilo furono generati i
        ''' suoi documenti (cap. 11.1): un CV inviato resta spiegabile anche a profilo
        ''' evoluto.
        ''' </returns>
        Public Function Salva(profilo As Profilo) As String

            If profilo Is Nothing Then Throw New ArgumentNullException(NameOf(profilo))

            _cartella.Assicura()

            Dim testo As String = profilo.ComeTesto()
            Dim versione As String = VersioneLibera(Date.Now)

            ScriviInModoAtomico(PercorsoVersione(versione), testo)
            ScriviInModoAtomico(_cartella.FileProfilo, testo)

            Return versione

        End Function

        ''' <summary>
        ''' Mette da parte una copia del profilo corrente che non si lascia leggere,
        ''' col nome <c>profilo.rotto-…</c> accanto all'originale, e ne restituisce il
        ''' percorso (<c>Nothing</c> se non c'è niente da copiare o la copia fallisce).
        ''' </summary>
        ''' <remarks>
        ''' È il complemento della promessa «il file resta lì da recuperare a mano»:
        ''' senza questa copia, il primo «Salva» dopo l'errore sovrascriverebbe il file
        ''' corrotto — che magari era recuperabile — con il profilo quasi vuoto mostrato
        ''' al suo posto. La copia non si fa mai due volte per lo stesso contenuto: se
        ''' un salvataggio con lo stesso nome c'è già, va bene quello.
        ''' </remarks>
        Public Function MettiInSalvoIlCorrotto() As String

            If Not Esiste Then Return Nothing

            Dim destinazione As String = Path.Combine(
                _cartella.CartellaProfilo,
                "profilo.rotto-" & Date.Now.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture) & ".json")

            Try
                If Not File.Exists(destinazione) Then
                    File.Copy(_cartella.FileProfilo, destinazione)
                End If
                Return destinazione
            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                ' Se nemmeno la copia riesce, il chiamante lo dice: meglio un avviso
                ' in più che una promessa di recupero non mantenuta.
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Manda via il profilo e tutto ciò che ne discende: il file corrente, lo storico,
        ''' il CV base con i suoi documenti, le copie messe in salvo. È l'eliminazione
        ''' definitiva del cap. 11.5, e non ha un annullo.
        ''' </summary>
        ''' <returns><c>False</c> se non c'era niente da eliminare.</returns>
        ''' <remarks>
        ''' <para>Il perimetro è <b>la cartella del profilo</b>, non un elenco di nomi: lì
        ''' dentro non vive nient'altro, e una lista da tenere allineata sarebbe la prima
        ''' cosa a dimenticarsi un file — un <c>.tmp</c> rimasto da una scrittura
        ''' interrotta, una copia <c>profilo.rotto-…</c>. Un'eliminazione che si dichiara
        ''' definitiva non può permettersi un residuo.</para>
        ''' <para>Le candidature stanno altrove (cap. 11.1) e qui non vengono sfiorate:
        ''' sono l'altra metà dei dati dell'utente, e si eliminano con un altro gesto.</para>
        ''' <para>L'ordine conta solo quando qualcosa va storto a metà — un file aperto in
        ''' un altro programma, un permesso negato. Si comincia da ciò che si rigenera e si
        ''' finisce col profilo corrente: se ci si ferma prima della fine, la cosa più
        ''' preziosa è ancora al suo posto.</para>
        ''' </remarks>
        Public Function EliminaTutto() As Boolean

            If Not Directory.Exists(_cartella.CartellaProfilo) Then Return False

            Dim cerano As Boolean = Directory.EnumerateFileSystemEntries(_cartella.CartellaProfilo).Any()

            EliminaCartella(_cartella.CartellaOutProfilo)
            EliminaFile(_cartella.FileCvBase)
            EliminaCartella(_cartella.CartellaStorico)
            EliminaFile(_cartella.FileProfilo)

            ' E poi la casa, con dentro quel che ci fosse rimasto.
            Directory.Delete(_cartella.CartellaProfilo, recursive:=True)

            Return cerano

        End Function

        ''' <summary>Toglie un file se c'è: cancellarne uno che non esiste non è un errore.</summary>
        Private Shared Sub EliminaFile(percorso As String)
            If File.Exists(percorso) Then File.Delete(percorso)
        End Sub

        ''' <summary>Toglie una cartella con tutto il suo contenuto, se c'è.</summary>
        Private Shared Sub EliminaCartella(percorso As String)
            If Directory.Exists(percorso) Then Directory.Delete(percorso, recursive:=True)
        End Sub

        ''' <summary>
        ''' Salva il 📄 CV base accanto al profilo, annotando da quale versione è nato.
        ''' </summary>
        ''' <remarks>
        ''' Sta qui e non in un archivio nuovo perché è <b>del profilo</b>, non di una
        ''' candidatura: nasce senza alcun annuncio ed è il ritratto del profilo in forma
        ''' di CV (cap. 11.1). È la vista-dati «un profilo, molti CV» presa alla lettera —
        ''' il CV base è del profilo, i CV mirati sono delle opportunità.
        ''' </remarks>
        ''' <param name="cv">Il CV generato.</param>
        ''' <param name="versioneProfilo">Il nome della versione da cui è nato.</param>
        ''' <returns>Il percorso del file scritto.</returns>
        Public Function SalvaCvBase(cv As JsonNode, versioneProfilo As String) As String

            If cv Is Nothing Then Throw New ArgumentNullException(NameOf(cv))

            _cartella.Assicura()

            Dim involucro As New JsonObject From {
                {"versione_profilo", versioneProfilo},
                {"generato", Date.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)},
                {"cv", cv.DeepClone()}}

            ScriviInModoAtomico(_cartella.FileCvBase, involucro.ToJsonString(FormatoLeggibile))

            Return _cartella.FileCvBase

        End Function

        ''' <summary>
        ''' Il 📄 CV base salvato, o <c>Nothing</c> se non ne è mai stato generato uno.
        ''' Solleva se il file c'è ma non si lascia leggere: come per il profilo, un
        ''' ripiego silenzioso nasconderebbe una perdita.
        ''' </summary>
        Public Function CaricaCvBase() As CvBase

            If Not File.Exists(_cartella.FileCvBase) Then Return Nothing

            Dim involucro As JsonObject = TryCast(
                JsonNode.Parse(File.ReadAllText(_cartella.FileCvBase, Encoding.UTF8)), JsonObject)

            If involucro Is Nothing Then
                Throw New JsonException("Il CV base non ha la forma attesa.")
            End If

            Dim generato As Date
            Date.TryParseExact(Campo(involucro, "generato"), "yyyy-MM-dd HH:mm:ss",
                               CultureInfo.InvariantCulture, DateTimeStyles.None, generato)

            Dim cv As JsonNode = Nothing
            involucro.TryGetPropertyValue("cv", cv)

            Return New CvBase With {
                .Cv = cv,
                .VersioneProfilo = Campo(involucro, "versione_profilo"),
                .Generato = generato}

        End Function

        ''' <summary>Un campo di testo dell'involucro, <c>Nothing</c> se manca.</summary>
        Private Shared Function Campo(involucro As JsonObject, nome As String) As String

            Dim valore As JsonNode = Nothing
            If Not involucro.TryGetPropertyValue(nome, valore) OrElse valore Is Nothing Then
                Return Nothing
            End If

            Return If(valore.GetValueKind() = JsonValueKind.String, valore.GetValue(Of String)(), Nothing)

        End Function

        ''' <summary>
        ''' Le versioni conservate, dalla più vecchia alla più recente. Il nome è fatto
        ''' apposta perché l'ordine alfabetico sia già l'ordine del tempo.
        ''' </summary>
        Public Function Versioni() As IReadOnlyList(Of String)

            If Not Directory.Exists(_cartella.CartellaStorico) Then
                Return Array.Empty(Of String)()
            End If

            Return Directory.GetFiles(_cartella.CartellaStorico, "*.json").
                Select(Function(f) Path.GetFileNameWithoutExtension(f)).
                OrderBy(Function(n) n, StringComparer.Ordinal).
                ToList()

        End Function

        ''' <summary>Il profilo di una versione dello storico, così com'era quel giorno.</summary>
        Public Function CaricaVersione(versione As String) As Profilo
            Return Profilo.DaTesto(File.ReadAllText(PercorsoVersione(versione), Encoding.UTF8))
        End Function

        ''' <summary>Il file di una versione dello storico.</summary>
        Private Function PercorsoVersione(versione As String) As String
            Return Path.Combine(_cartella.CartellaStorico, versione & ".json")
        End Function

        ''' <summary>
        ''' Il nome della copia da scrivere: data e ora al secondo. Se in quel secondo
        ''' una copia c'è già — due conferme di fila — si aggiunge un progressivo invece
        ''' di sovrascriverla: una versione confermata non si perde per una collisione
        ''' di orologio.
        ''' </summary>
        Private Function VersioneLibera(istante As Date) As String

            Dim radice As String = istante.ToString("yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture)
            Dim nome As String = radice
            Dim progressivo As Integer = 1

            While File.Exists(PercorsoVersione(nome))
                progressivo += 1
                nome = $"{radice}_{progressivo}"
            End While

            Return nome

        End Function

        ''' <summary>
        ''' Scrive un file passando da un temporaneo accanto a lui. Lo spostamento sullo
        ''' stesso volume è una sola operazione per il sistema: o c'è il file di prima, o
        ''' c'è quello nuovo, mai uno troncato a metà.
        ''' </summary>
        Private Shared Sub ScriviInModoAtomico(percorso As String, testo As String)

            Dim temporaneo As String = percorso & ".tmp"

            File.WriteAllText(temporaneo, testo, Codifica)
            File.Move(temporaneo, percorso, overwrite:=True)

        End Sub

    End Class

End Namespace
