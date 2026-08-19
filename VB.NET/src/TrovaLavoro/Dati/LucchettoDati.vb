Imports System.IO

Namespace Dati

    ''' <summary>
    ''' Il lucchetto di scrittura della cartella dati (cap. 09.4): un solo scrittore alla
    ''' volta, dichiarato.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Il problema che risolve non è la corsa sui byte.</b> Le scritture di
    ''' questo programma sono già atomiche una per una (cap. 11.1): il pericolo vero è
    ''' un'altra cosa — l'applicazione tiene aperto in memoria un profilo o una
    ''' candidatura, il server MCP li cambia sul disco sotto di lei, e al primo salvataggio
    ''' l'applicazione riscrive sopra quel che non ha mai visto. È una perdita silenziosa,
    ''' che nessun file corrotto segnalerebbe.</para>
    ''' <para><b>Il lucchetto è il file stesso, tenuto aperto in esclusiva</b>, e non il
    ''' suo contenuto. Non c'è dentro nessun identificativo di processo e nessuna ora, ed è
    ''' la scelta che conta: chi lo tiene lo dichiara al sistema operativo, e quando il
    ''' processo muore — chiuso, in crash o ammazzato — è Windows a rilasciarlo. Un
    ''' lucchetto scritto invece dovrebbe essere <i>ripulito</i> da qualcuno, e prima o poi
    ''' lascerebbe l'utente chiuso fuori dai propri dati per un file rimasto lì da un
    ''' crash di tre settimane prima.</para>
    ''' <para><b>Chi lo prende, e per quanto, non è simmetrico</b>, perché non lo sono i
    ''' due che scrivono. L'applicazione con le finestre lo prende <b>all'avvio e lo tiene
    ''' per tutta la sessione</b>: l'utente scrive quando gli pare, e fra un salvataggio e
    ''' l'altro tiene comunque in mano dei dati che nessun altro deve muovere. Il server
    ''' MCP lo prende <b>solo per la durata di una scrittura</b> e lo rilascia subito: non
    ''' ricorda niente fra una richiesta e l'altra, quindi non ha niente da proteggere nel
    ''' frattempo.</para>
    ''' </remarks>
    Public NotInheritable Class LucchettoDati
        Implements IDisposable

        Private _tenuto As FileStream

        Private Sub New(tenuto As FileStream)
            _tenuto = tenuto
        End Sub

        ''' <summary>
        ''' Prova a prendere il lucchetto.
        ''' </summary>
        ''' <param name="cartella">La cartella dati da chiudere a chiave.</param>
        ''' <returns>
        ''' Il lucchetto preso, da rilasciare con <see cref="Dispose"/>; <c>Nothing</c> se
        ''' ce l'ha qualcun altro.
        ''' </returns>
        ''' <remarks>
        ''' Prendere il lucchetto <b>crea la cartella dati</b>, che è il solo modo di
        ''' metterci dentro un file. Non è un effetto collaterale da nascondere: chi lo
        ''' prende sta per scrivere, o si sta dichiarando padrone di casa per una sessione
        ''' intera.
        ''' </remarks>
        Public Shared Function Prendi(cartella As CartellaDati) As LucchettoDati

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))

            Try
                cartella.Assicura()

                ' FileShare.None è tutto il meccanismo: il secondo che prova si prende un
                ' rifiuto dal sistema operativo, senza che nessuno debba mettersi
                ' d'accordo con nessuno.
                Return New LucchettoDati(New FileStream(
                    cartella.FileLucchetto, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))

            Catch ex As IOException
                ' Ce l'ha un altro processo. È la risposta prevista, non un guasto: chi ha
                ' chiamato ha già pronto che cosa dire.
                Return Nothing

            Catch ex As UnauthorizedAccessException
                ' La cartella non si lascia scrivere: da qui non si distingue da un
                ' lucchetto occupato, e per chi ha chiamato la conseguenza è la stessa —
                ' non si scrive.
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Se questo lucchetto è ancora in mano nostra: falso dopo il rilascio.
        ''' </summary>
        Public ReadOnly Property Tenuto As Boolean
            Get
                Return _tenuto IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Rilascia il lucchetto. Chiamarlo due volte non fa niente, e il file resta lì
        ''' vuoto: cancellarlo aprirebbe una gara con chi lo sta riaprendo proprio adesso,
        ''' e un file di zero byte non dà fastidio a nessuno.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            Dim tenuto As FileStream = _tenuto
            If tenuto Is Nothing Then Return

            _tenuto = Nothing

            Try
                tenuto.Dispose()
            Catch ex As IOException
            End Try

        End Sub

    End Class

End Namespace
