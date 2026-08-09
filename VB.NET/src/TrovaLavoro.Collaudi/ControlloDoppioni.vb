Imports TrovaLavoro.Dati

''' <summary>
''' Cerca dentro un profilo <b>la stessa cosa contata due volte</b>: un'attività che sta
''' sia fra le esperienze formali sia fra le informali.
''' </summary>
''' <remarks>
''' <para>Non è un controllo teorico: è nato da un esito reale del collaudo di T3. Sullo
''' stesso CV, con lo stesso prompt e lo stesso modello, un volontariato è finito una
''' volta fra le esperienze informali (dove lo schema lo vuole) e un'altra volta in
''' <b>entrambe</b> le sezioni — cioè scritto due volte nel profilo, e una delle due con
''' l'associazione nel campo «azienda», come se fosse un datore di lavoro. Un doppione
''' non è un giudizio diverso del modello: è sempre sbagliato, e la tolleranza sul
''' conteggio delle voci se lo mangia in silenzio (una voce di scarto è ammessa).</para>
''' <para>L'appaiamento è sull'<b>organizzazione</b> — <c>azienda</c> per una formale,
''' <c>con_chi</c> per una informale — perché è l'unico campo che le due sezioni
''' condividono e che il modello <i>copia</i> dal CV invece di riformularlo. I testi
''' (<c>ruolo</c>, <c>cosa_facevo</c>) vengono riscritti ogni volta con parole diverse e
''' appaiarli darebbe falsi negativi a ripetizione. Il limite dichiarato: un doppione in
''' cui nessuna delle due voci nomina l'organizzazione, qui non si vede.</para>
''' <para>Sta nel progetto dei collaudi e non nel prodotto perché la cura è nel prompt:
''' l'app non potrebbe rimediare da sé, il doppione lo produce il modello. Nato come
''' osservatore, dal <b>pool 1.01</b> è un Assert: <c>importa_cv</c> adesso dice che
''' un'attività va in una sezione sola, quindi un doppione dell'app è un difetto e non
''' più un caso di confine. Resta un'osservazione per il <b>prototipo</b>, che quella
''' regola non ce l'ha: lì il doppione si segnala e basta.</para>
''' </remarks>
Friend Module ControlloDoppioni

    ''' <summary>Quanto testo di una voce mostrare nel rapporto.</summary>
    Private Const Quanto As Integer = 60

    ''' <summary>
    ''' I doppioni del profilo, già scritti come vanno mostrati. Lista vuota quando è
    ''' tutto in ordine.
    ''' </summary>
    Friend Function Trova(profilo As Profilo) As List(Of String)

        Dim doppioni As New List(Of String)
        If profilo Is Nothing Then Return doppioni

        For Each formale As EsperienzaFormale In profilo.EsperienzeFormali

            Dim organizzazione As String = Appaiabile(formale.Azienda)
            If organizzazione.Length = 0 Then Continue For

            For Each informale As EsperienzaInformale In profilo.EsperienzeInformali

                If Appaiabile(informale.ConChi) <> organizzazione Then Continue For

                doppioni.Add(
                    $"«{formale.Azienda.Trim()}» sta in due sezioni: " &
                    $"fra le formali come «{Corto(formale.Ruolo)}», " &
                    $"fra le informali come «{Corto(informale.CosaFacevo)}»")

            Next

        Next

        Return doppioni

    End Function

    ''' <summary>
    ''' Il nome di un'organizzazione ridotto a come si appaia: spazi di troppo via
    ''' (quelli sono impaginazione) e maiuscole indifferenti.
    ''' </summary>
    Private Function Appaiabile(valore As String) As String

        Return String.Join(" ", If(valore, String.Empty).
            Split({" "c, vbTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant()

    End Function

    ''' <summary>Un testo abbastanza corto per stare su una riga del rapporto.</summary>
    Private Function Corto(valore As String) As String

        Dim pulito As String = String.Join(" ", If(valore, String.Empty).
            Split({" "c, vbTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries))

        If pulito.Length <= Quanto Then Return pulito

        Return pulito.Substring(0, Quanto).TrimEnd() & "…"

    End Function

End Module
