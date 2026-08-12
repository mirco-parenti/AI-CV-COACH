Imports System.Text.Json.Nodes
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' A che punto è una candidatura (cap. 07.3). È lo stato che <c>stato.json</c>
    ''' conserva e che il registro mostra.
    ''' </summary>
    ''' <remarks>
    ''' <para>Il ciclo di vita del cap. 07.3 per intero: <c>nuova → interessante →
    ''' generata → inviata → esito</c>, con <c>scartata</c> che chiude la strada da
    ''' qualunque punto prima dell'invio.</para>
    ''' <para><b>Due valori esistono ma nessuno li scrive ancora</b> (deciso con Mirco il
    ''' 2026-08-12): <see cref="Inviata"/> ed <see cref="Esito"/> sono di T6, che è la
    ''' tappa dell'email, e nell'interfaccia di oggi i comandi che ci portano sono spenti
    ''' col loro tooltip (cap. 03.8). Stanno qui lo stesso perché lo schema su disco sia
    ''' già quello definitivo: così T6 aggiunge dei passaggi, non una migrazione dei file
    ''' scritti fino a lì.</para>
    ''' </remarks>
    Public Enum StatoOpportunita

        ''' <summary>L'annuncio c'è, ma non è ancora stato confrontato col profilo.</summary>
        Nuova

        ''' <summary>Confrontata: ci sono i giudizi e le stelle, e vale la pena guardarla.</summary>
        Interessante

        ''' <summary>Il 🎯 CV mirato e la lettera sono stati scritti.</summary>
        Generata

        ''' <summary>La candidatura è partita — lo conferma l'utente, non il programma (T6).</summary>
        Inviata

        ''' <summary>Si sa com'è finita: in attesa, colloquio, rifiutata, assunto 🎉 (T6).</summary>
        Esito

        ''' <summary>Lasciata perdere. La cartella resta su disco: si scarta, non si cancella.</summary>
        Scartata

    End Enum

    ''' <summary>
    ''' Come si legge, come si scrive e come si cambia uno <see cref="StatoOpportunita"/>:
    ''' il nome su disco, l'etichetta da mostrare, le transizioni lecite e la deduzione
    ''' per le opportunità nate prima che questo campo esistesse.
    ''' </summary>
    ''' <remarks>
    ''' È una classe di soli metodi condivisi e non un <c>Module</c>: i membri di un modulo
    ''' si chiamano senza prefisso da tutto ciò che importa il namespace, e un nome corto e
    ''' comune come <c>Nome</c> lì dentro diventa una trappola — in VB una variabile locale
    ''' che si chiama come una funzione la copre, e la chiamata si legge come un indice
    ''' sulla stringa (è già successo, v. <c>ContestoApp.LeggiChiave</c>).
    ''' </remarks>
    Public NotInheritable Class StatiOpportunita

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Dove può andare uno stato. Le liste vuote sono i capolinea: da
        ''' <see cref="StatoOpportunita.Esito"/> non si va più avanti, e lo scarto è una
        ''' decisione che non si disfa dall'interfaccia — la cartella però resta su disco,
        ''' e chi ci ripensa davvero ha ancora tutto (deciso con Mirco il 2026-08-12).
        ''' </summary>
        ''' <remarks>
        ''' <c>generata → scartata</c> non sta nel disegno del cap. 07.3, dove lo scarto
        ''' parte da «nuova» e da «interessante», ma è una strada vera: i documenti si
        ''' possono guardare e decidere lo stesso di lasciar perdere, e il bottone «Scarta»
        ''' di P4 è lì anche in quel momento (cap. 03.6).
        ''' </remarks>
        Private Shared ReadOnly Strade As New Dictionary(Of StatoOpportunita, StatoOpportunita()) From {
            {StatoOpportunita.Nuova, {StatoOpportunita.Interessante, StatoOpportunita.Scartata}},
            {StatoOpportunita.Interessante, {StatoOpportunita.Generata, StatoOpportunita.Scartata}},
            {StatoOpportunita.Generata, {StatoOpportunita.Inviata, StatoOpportunita.Scartata}},
            {StatoOpportunita.Inviata, {StatoOpportunita.Esito}},
            {StatoOpportunita.Esito, {}},
            {StatoOpportunita.Scartata, {}}}

        ''' <summary>
        ''' Come lo stato si scrive in <c>stato.json</c>: minuscolo e senza accenti, come
        ''' ogni altro valore di schema del progetto.
        ''' </summary>
        Public Shared Function Nome(stato As StatoOpportunita) As String

            Select Case stato
                Case StatoOpportunita.Nuova : Return "nuova"
                Case StatoOpportunita.Interessante : Return "interessante"
                Case StatoOpportunita.Generata : Return "generata"
                Case StatoOpportunita.Inviata : Return "inviata"
                Case StatoOpportunita.Esito : Return "esito"
                Case StatoOpportunita.Scartata : Return "scartata"
                Case Else : Throw New ArgumentOutOfRangeException(NameOf(stato))
            End Select

        End Function

        ''' <summary>
        ''' Lo stato scritto su disco; <c>Nothing</c> se quel nome non lo conosciamo — che
        ''' è come si comporta il resto del programma coi file dell'utente: non si
        ''' indovina, si torna a dedurlo dai fatti.
        ''' </summary>
        ''' <param name="scritto">
        ''' Il nome letto dal file. Non può chiamarsi «nome»: coprirebbe la funzione
        ''' <see cref="Nome"/> qui sopra, e la chiamata dentro il ciclo verrebbe letta
        ''' come un indice su questa stringa (è la trappola di VB che il progetto ha già
        ''' pagato una volta, v. <c>ContestoApp.LeggiChiave</c>).
        ''' </param>
        Public Shared Function DaNome(scritto As String) As StatoOpportunita?

            If String.IsNullOrWhiteSpace(scritto) Then Return Nothing

            Dim cercato As String = scritto.Trim().ToLowerInvariant()

            For Each stato As StatoOpportunita In [Enum].GetValues(Of StatoOpportunita)()
                If Nome(stato) = cercato Then Return stato
            Next

            Return Nothing

        End Function

        ''' <summary>Come lo stato si mostra all'utente: in italiano, con l'iniziale grande.</summary>
        Public Shared Function Etichetta(stato As StatoOpportunita) As String

            Select Case stato
                Case StatoOpportunita.Nuova : Return "Nuova"
                Case StatoOpportunita.Interessante : Return "Interessante"
                Case StatoOpportunita.Generata : Return "Generata"
                Case StatoOpportunita.Inviata : Return "Inviata"
                Case StatoOpportunita.Esito : Return "Con esito"
                Case StatoOpportunita.Scartata : Return "Scartata"
                Case Else : Throw New ArgumentOutOfRangeException(NameOf(stato))
            End Select

        End Function

        ''' <summary>
        ''' Se da uno stato si può passare all'altro. Restare dov'è non è una transizione e
        ''' qui risulta <b>falso</b>: chi avanza lo tratta a parte
        ''' (<see cref="Opportunita.Avanza"/>), perché rigenerare i documenti di
        ''' un'opportunità già generata non è un errore ma nemmeno un passaggio nuovo.
        ''' </summary>
        Public Shared Function Consentita(da As StatoOpportunita, a As StatoOpportunita) As Boolean

            ' Il nome non può essere «strade»: in VB le maiuscole non distinguono, e una
            ' locale così coprirebbe la tabella «Strade» qui sopra — la chiamata a
            ' TryGetValue finirebbe sull'array vuoto appena dichiarato.
            Dim dove As StatoOpportunita() = Nothing
            If Not Strade.TryGetValue(da, dove) Then Return False

            Return Array.IndexOf(dove, a) >= 0

        End Function

        ''' <summary>
        ''' Lo stato di un'opportunità che non lo dichiara, dedotto <b>dai file che ha</b>
        ''' (deciso con Mirco il 2026-08-12).
        ''' </summary>
        ''' <remarks>
        ''' Le cartelle scritte da T4 hanno uno <c>stato.json</c> senza il campo
        ''' <c>stato</c>: quel campo nasce qui, a T5c, e riscrivere all'indietro i file
        ''' dell'utente per aggiungercelo sarebbe un'invasione. Si guarda invece cosa c'è
        ''' dentro la cartella, che è il fatto: se i documenti ci sono, quell'opportunità
        ''' è stata generata; se ci sono i giudizi, è stata confrontata; altrimenti c'è
        ''' solo l'annuncio ed è nuova. Lo scarto non si deduce — è una decisione, e una
        ''' decisione che nessuno ha scritto non c'è.
        ''' </remarks>
        Public Shared Function Dedotto(opportunita As Opportunita) As StatoOpportunita

            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))

            If opportunita.Cv IsNot Nothing Then Return StatoOpportunita.Generata
            If opportunita.Confrontata Then Return StatoOpportunita.Interessante

            Return StatoOpportunita.Nuova

        End Function

        ''' <summary>
        ''' Le date dei passaggi come si scrivono, <b>nell'ordine del ciclo di vita</b> e
        ''' non in quello in cui capita di annotarle: un file che si legge a mano deve
        ''' raccontare la storia dall'inizio. Gli stati mai raggiunti non compaiono.
        ''' </summary>
        ''' <remarks>
        ''' Sta qui, insieme a <see cref="RiempiDate"/>, perché il blocco <c>date_stati</c>
        ''' lo scrivono in due — lo <c>stato.json</c> di ogni cartella e il
        ''' <c>registro.json</c> — e la forma di quel blocco dev'essere una sola.
        ''' </remarks>
        Public Shared Function DateComeJson(quando As IReadOnlyDictionary(Of StatoOpportunita, Date)) As JsonObject

            Dim scritte As New JsonObject
            If quando Is Nothing Then Return scritte

            For Each stato As StatoOpportunita In [Enum].GetValues(Of StatoOpportunita)()
                Dim istante As Date
                If quando.TryGetValue(stato, istante) Then
                    scritte(Nome(stato)) = CampiJson.Quando(istante)
                End If
            Next

            Return scritte

        End Function

        ''' <summary>
        ''' Rimette le date dei passaggi dentro il dizionario che le tiene, buttando quelle
        ''' che c'erano. Una data che non si lascia leggere sparisce e basta: non sapere
        ''' <i>quando</i> è successo non toglie che sia successo.
        ''' </summary>
        Public Shared Sub RiempiDate(quando As IDictionary(Of StatoOpportunita, Date),
                                     scritte As JsonObject)

            If quando Is Nothing Then Throw New ArgumentNullException(NameOf(quando))

            quando.Clear()
            If scritte Is Nothing Then Return

            For Each stato As StatoOpportunita In [Enum].GetValues(Of StatoOpportunita)()
                Dim istante As Date = CampiJson.Istante(scritte, Nome(stato))
                If istante <> Nothing Then quando(stato) = istante
            Next

        End Sub

    End Class

End Namespace
