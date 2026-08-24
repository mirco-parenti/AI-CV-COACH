Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Dati

    ''' <summary>
    ''' Quale dei due documenti di una candidatura: il CV o la lettera (R7, cap. 08.4).
    ''' </summary>
    ''' <remarks>
    ''' Sono due e non tre: il 📄 CV-1 base e il 🎯 CV-2 mirato non convivono mai — P6
    ''' mostra o l'uno o l'altro — e quale dei due si stia guardando lo sa il pannello, che
    ''' quel documento ce l'ha in mano. Qui serve distinguere il <b>CV</b> dalla
    ''' <b>lettera</b>, perché è dal CV che la lettera prende la storia da raccontare, e
    ''' mai il contrario: riscrivere il CV può disallineare la lettera, riscrivere la
    ''' lettera non disallinea niente.
    ''' </remarks>
    Public Enum RuoloDocumento

        ''' <summary>Il CV: il 🎯 CV-2 mirato di una candidatura, o il 📄 CV-1 base.</summary>
        Cv

        ''' <summary>La ✉️ lettera di presentazione.</summary>
        Lettera

    End Enum

    ''' <summary>
    ''' I campi di prosa che l'utente ha riscritto <b>a mano</b> in un documento, e quando
    ''' (R7, 2026-08-23).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché si scrive su disco.</b> Da T9d la modifica a mano viveva in un
    ''' booleano in memoria, azzerato a ogni rientro in P6 (<c>PannelloDocumenti</c>):
    ''' l'avviso «vengono sostituite anche le modifiche fatte a mano» era vero finché non
    ''' si cambiava pannello, e dal rientro in poi «Rigenera» se le portava via in
    ''' silenzio. Un lavoro dell'utente che il programma dimentica appena guarda altrove
    ''' non si può avvisare a voce: o lo si annota dove il documento vive, o l'avviso è
    ''' una promessa che scade da sola.</para>
    ''' <para><b>Perché gli id dei campi e non un sì/no.</b> Perché servono a tre cose
    ''' diverse, e un sì/no ne coprirebbe una sola: l'avviso di «Rigenera» dice <b>quali</b>
    ''' testi si perdono; la spia della lettera distingue il documento riscritto da quello
    ''' che ne discende; e il prompt della lettera riceve <b>solo</b> le parole che
    ''' l'utente ha scritto davvero — che sono una sua dichiarazione, e quindi fonte di
    ''' fatti — invece di doversi fidare del CV intero (cap. 04, prompt <c>lettera</c>).</para>
    ''' <para><b>Un blocco che tace quando non ha niente da dire</b>: se nessuno ha
    ''' riscritto niente, nei file non compare affatto — come «match» in <c>stato.json</c>.
    ''' Le candidature scritte prima di R7 si riaprono senza, e vale come «mai toccate a
    ''' mano»: non si deduce all'indietro una storia che nessuno ha registrato
    ''' (cap. 11.1).</para>
    ''' </remarks>
    Public Class RiscrittureAMano

        ''' <summary>
        ''' Gli id dei campi riscritti — <c>sommario</c>, <c>esperienza.1</c>, <c>corpo</c>
        ''' — nella forma che usa <see cref="Motore.Rifinitura.CampiDiProsa"/>, che è
        ''' l'unico posto a sapere quali campi di un documento sono prosa.
        ''' </summary>
        Public ReadOnly Property Campi As New List(Of String)

        ''' <summary>
        ''' Quando è stata fatta l'ultima riscrittura; la data vuota se non ce n'è
        ''' nessuna. Serve alla spia della lettera: è la data che si confronta con quella
        ''' dell'ultima lettera scritta (<see cref="Motore.Opportunita.LetteraDaRiallineare"/>).
        ''' </summary>
        Public Property Quando As Date

        ''' <summary>Se in questo documento c'è almeno un campo riscritto a mano.</summary>
        Public ReadOnly Property CEQualcosa As Boolean
            Get
                Return Campi.Count > 0
            End Get
        End Property

        ''' <summary>
        ''' Se <b>questo</b> campo risulta riscritto a mano.
        ''' </summary>
        ''' <remarks>
        ''' Chiederlo qui invece di frugare in <see cref="Campi"/> tiene in un posto solo
        ''' cosa voglia dire «l'ha scritto l'utente»: il segno ✎ della finestra di modifica
        ''' e l'avviso di «Rigenera» fanno la stessa domanda, e devono avere la stessa
        ''' risposta.
        ''' </remarks>
        Public Function Contiene(id As String) As Boolean

            If String.IsNullOrWhiteSpace(id) Then Return False

            Return Campi.Contains(id)

        End Function

        ''' <summary>
        ''' Annota un campo riscritto a mano. Ripassare dallo stesso campo non lo duplica
        ''' — riscriverlo due volte è pur sempre un campo riscritto — ma aggiorna la data,
        ''' che è quella dell'<b>ultima</b> volta che l'utente ci ha messo mano.
        ''' </summary>
        Public Sub Annota(id As String, quando As Date)

            If String.IsNullOrWhiteSpace(id) Then Return

            If Not Campi.Contains(id) Then Campi.Add(id)

            ' «Me.» non è pignoleria: VB non distingue le maiuscole, e senza di lui questa
            ' riga assegnerebbe il parametro «quando» a sé stesso, lasciando la proprietà
            ' vuota — senza un errore di compilazione, e quindi senza che nessuno lo dica.
            Me.Quando = quando

        End Sub

        ''' <summary>
        ''' Dimentica tutto: il documento è stato riscritto da capo dall'AI, e le parole
        ''' dell'utente non ci sono più — annotarle ancora vorrebbe dire promettere in un
        ''' avviso un lavoro che nel file non esiste.
        ''' </summary>
        Public Sub Dimentica()

            Campi.Clear()
            Quando = Nothing

        End Sub

        ''' <summary>
        ''' Prende su di sé quel che è annotato altrove: è la copia che il pannello tiene
        ''' in mano di ciò che sta nel file (il 📄 CV base, che a differenza di una
        ''' candidatura non ha un oggetto suo in memoria).
        ''' </summary>
        Public Sub Prendi(altre As RiscrittureAMano)

            Dimentica()

            If altre Is Nothing Then Return

            Campi.AddRange(altre.Campi)
            Quando = altre.Quando

        End Sub

        ''' <summary>
        ''' Come si scrive nel file; <c>Nothing</c> quando non c'è niente da scrivere, così
        ''' chi salva sa che quel blocco va lasciato fuori.
        ''' </summary>
        Public Function ComeJson() As JsonObject

            If Not CEQualcosa Then Return Nothing

            Dim elenco As New JsonArray()
            For Each id As String In Campi
                elenco.Add(JsonValue.Create(id))
            Next

            Return New JsonObject From {
                {"campi", elenco},
                {"quando", CampiJson.Quando(Quando)}}

        End Function

        ''' <summary>
        ''' Rimette dentro quel che il file conserva. Un file senza questo blocco — tutti
        ''' quelli scritti prima di R7 — lascia il documento senza riscritture, che è
        ''' esattamente com'era.
        ''' </summary>
        Public Sub Rileggi(scritto As JsonObject)

            Dimentica()

            If scritto Is Nothing Then Return

            Dim elenco As JsonArray = TryCast(CampiJson.Nodo(scritto, "campi"), JsonArray)
            If elenco IsNot Nothing Then

                For Each voce As JsonNode In elenco

                    If voce Is Nothing OrElse voce.GetValueKind() <> JsonValueKind.String Then Continue For

                    Dim id As String = voce.GetValue(Of String)()
                    If Not String.IsNullOrWhiteSpace(id) AndAlso Not Campi.Contains(id) Then Campi.Add(id)

                Next

            End If

            Quando = CampiJson.Istante(scritto, "quando")

        End Sub

    End Class

End Namespace
