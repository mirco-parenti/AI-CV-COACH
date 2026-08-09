Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text

Namespace Dati

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
