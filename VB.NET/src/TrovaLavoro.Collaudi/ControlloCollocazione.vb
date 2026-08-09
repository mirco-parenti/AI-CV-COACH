Imports System.Linq
Imports TrovaLavoro.Dati

''' <summary>
''' Cerca dentro un profilo un'attività che <b>si dichiara volontaria</b> ma è finita
''' fra le esperienze <b>formali</b>, cioè fra i lavori veri e propri.
''' </summary>
''' <remarks>
''' <para>È il gemello di <see cref="ControlloDoppioni"/>, e nasce dallo stesso blocco
''' dello stesso CV. Detto al prompt che un'attività sta in una sezione sola, il doppione
''' è sparito — ma una lettura su cinque ha risolto l'ambiguità dalla parte sbagliata,
''' promovendo il volontariato a impiego: la stessa voce, con la stessa organizzazione
''' nel campo «azienda», come se fosse un datore di lavoro. Il difetto si era spostato,
''' non tolto, e nessun Assert lo vedeva: una voce che passa da una sezione all'altra
''' lascia i conteggi dentro la tolleranza.</para>
''' <para>Perché sia un pass/fail e non un'osservazione: qui non c'è nessun giudizio da
''' dare. Il CV scrive «VOLONTARIO» nel ruolo, e il <b>pool 1.01</b> dice che a decidere
''' è la natura dell'attività e non la sezione del CV in cui è stampata. Se il profilo la
''' mette fra i lavori, il prompt non è stato seguito.</para>
''' <para>Il segnale è la parola <b>intera</b> — «volontario», «volontaria»,
''' «volontariato» — nel ruolo o in ciò che la persona faceva. Il limite dichiarato:
''' un impiego vero il cui titolo contenga una di quelle parole (un «coordinatore
''' volontariato» stipendiato) qui risulterebbe mal collocato. Il plurale «volontari»
''' resta fuori apposta, perché «coordinatore volontari» è quasi sempre un lavoro vero.
''' Su un CV che dichiara il volontariato come tale il controllo è netto; su un CV
''' costruito apposta per confonderlo, no — e in quel caso si guarda il rapporto.</para>
''' </remarks>
Friend Module ControlloCollocazione

    ''' <summary>Le parole con cui un'attività si dichiara volontaria.</summary>
    ''' <remarks>
    ''' Al singolare e come sostantivo: vedi il limite dichiarato sopra sul plurale.
    ''' </remarks>
    Private ReadOnly Parole As String() = {"volontario", "volontaria", "volontariato"}

    ''' <summary>Quanto testo di una voce mostrare nel rapporto.</summary>
    Private Const Quanto As Integer = 60

    ''' <summary>
    ''' Le esperienze formali che si dichiarano volontarie, già scritte come vanno
    ''' mostrate. Lista vuota quando è tutto in ordine.
    ''' </summary>
    Friend Function VolontariatoFraLeFormali(profilo As Profilo) As List(Of String)

        Dim malCollocate As New List(Of String)
        If profilo Is Nothing Then Return malCollocate

        For Each formale As EsperienzaFormale In profilo.EsperienzeFormali

            Dim parola As String = SiDichiaraVolontaria(formale)
            If parola Is Nothing Then Continue For

            malCollocate.Add(
                $"«{Corto(formale.Ruolo)}»" &
                If(String.IsNullOrWhiteSpace(formale.Azienda), "", $" ({formale.Azienda.Trim()})") &
                $" sta fra le esperienze formali, ma si dichiara «{parola}»")

        Next

        Return malCollocate

    End Function

    ''' <summary>
    ''' La parola con cui l'esperienza si dichiara volontaria, o <c>Nothing</c>. Si guarda
    ''' il <b>ruolo</b> e ciò che la persona <b>faceva</b>: sono i due campi in cui il CV
    ''' dice la natura dell'attività. Non l'azienda — il nome di un'associazione di
    ''' volontariato non fa di un impiego lì dentro un volontariato.
    ''' </summary>
    Private Function SiDichiaraVolontaria(formale As EsperienzaFormale) As String

        ' Il nome non può essere «parole»: in VB i nomi non distinguono le maiuscole, e
        ' una locale così coprirebbe l'array «Parole» del modulo — il rilevatore
        ' segnalerebbe la prima parola di ogni esperienza.
        Dim scritte As HashSet(Of String) = New HashSet(Of String)(
            Spezzata(formale.Ruolo).Concat(Spezzata(formale.CosaFacevo)))

        Return Parole.FirstOrDefault(Function(p) scritte.Contains(p))

    End Function

    ''' <summary>
    ''' Un testo ridotto alle sue parole intere, minuscole: tutto ciò che non è lettera o
    ''' cifra separa. Così «SOCCORRITORE PARAMEDICO VOLONTARIO | Croce Verde» dà la parola
    ''' «volontario», e «coordinatore volontari» non la dà.
    ''' </summary>
    Private Function Spezzata(valore As String) As IEnumerable(Of String)

        Return New String(If(valore, String.Empty).
            Select(Function(c) If(Char.IsLetterOrDigit(c), Char.ToLowerInvariant(c), " "c)).ToArray()).
            Split(" "c, StringSplitOptions.RemoveEmptyEntries)

    End Function

    ''' <summary>Un testo abbastanza corto per stare su una riga del rapporto.</summary>
    Private Function Corto(valore As String) As String

        Dim pulito As String = String.Join(" ", If(valore, String.Empty).
            Split({" "c, vbTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries))

        If pulito.Length <= Quanto Then Return pulito

        Return pulito.Substring(0, Quanto).TrimEnd() & "…"

    End Function

End Module
