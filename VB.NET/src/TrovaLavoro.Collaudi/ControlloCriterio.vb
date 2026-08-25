Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports TrovaLavoro.Dati

''' <summary>
''' Confronta un profilo <b>uscito dall'import</b> con il profilo <b>atteso</b> — il
''' criterio — e dice in che cosa i due non dicono la stessa cosa. Lista vuota quando il
''' criterio regge.
''' </summary>
''' <remarks>
''' <para>È il terzo dei controlli del banco che guardano dentro un profilo, dopo
''' <see cref="ControlloDoppioni"/> e <see cref="ControlloCollocazione"/>, e nasce da un
''' buco che quei due lasciavano aperto: sono entrambi <b>ciechi al caso in cui il modello
''' promuove a impiego un'attività che impiego non è</b>, se quell'attività non usa la
''' parola «volontario» e non compare anche fra le informali. Un trasloco fatto per un
''' amico nel fine settimana non dice né l'una né l'altra cosa: finirebbe fra i lavori
''' veri lasciando i conteggi dentro la tolleranza, e nessun Assert lo vedrebbe.</para>
''' <para><b>Si giudicano i fatti, non la prosa.</b> Le durate tengono le parole del CV
''' («marzo 2021 - oggi (3 anni)» dove il criterio scrive «3 anni»), <c>cosa_facevo</c> e
''' le competenze il modello le riformula ogni volta: pretenderle uguali sarebbe un
''' collaudo che lampeggia, e un collaudo che lampeggia si smette di guardarlo (cap. 14,
''' la lezione di T2). Si pretendono quindi i valori che il CV scrive nero su bianco e che
''' il prompt ordina di copiare, i conteggi delle sezioni che il CV nomina una per una, e
''' la <b>collocazione</b> di ogni attività.</para>
''' <para><b>Come si appaia un'attività informale</b>, che è la parte nuova: dal criterio
''' si prendono le parole con cui quell'attività si riconosce — lunghe, e assenti dalle
''' esperienze formali dello stesso criterio, che fanno da contrasto — e se ne cercano
''' <b>almeno due nella stessa voce</b> fra le formali del profilo. Due, non una: una
''' parola sola in comune capita per caso, due nella stessa voce vogliono dire che è
''' quell'attività. Il limite dichiarato: un'attività informale riscritta dal modello con
''' parole tutte diverse da quelle del criterio, qui non si vede.</para>
''' <para>Sta nel banco e non nel prodotto per la stessa ragione degli altri due: la cura
''' è nel prompt, l'app non potrebbe rimediare da sé.</para>
''' </remarks>
Friend Module ControlloCriterio

    ''' <summary>Quanto testo di una voce mostrare in uno scostamento.</summary>
    Private Const Quanto As Integer = 60

    ''' <summary>
    ''' Quante parole distintive della stessa attività devono ricomparire nella stessa
    ''' esperienza formale perché sia lei e non una somiglianza casuale.
    ''' </summary>
    Private Const IncontriPerAppaiare As Integer = 2

    ''' <summary>
    ''' Quanto dev'essere lunga una parola per distinguere un'attività da un'altra. Sotto
    ''' questa misura ci sono gli articoli, le preposizioni e i verbi di servizio, che due
    ''' testi qualunque hanno in comune.
    ''' </summary>
    Private Const LettereDistintive As Integer = 6

    ''' <summary>
    ''' Le parole lunghe che però non distinguono niente: le si incontra in qualunque
    ''' racconto di lavoro, e lasciarle dentro darebbe appaiamenti per caso.
    ''' </summary>
    Private ReadOnly DiServizio As New HashSet(Of String) From {
        "azienda", "aziende", "lavoro", "lavori", "lavorato", "attività", "esperienza",
        "esperienze", "società", "settimana", "settimane", "periodo", "sempre", "quando"}

    ''' <summary>Le parole di un testo che possono distinguere un'attività.</summary>
    Private ReadOnly Parola As New Regex("\p{L}{" & LettereDistintive & ",}",
                                         RegexOptions.Compiled)

    ''' <summary>
    ''' Gli scostamenti del profilo dal criterio, già scritti come vanno mostrati. Lista
    ''' vuota quando il criterio regge.
    ''' </summary>
    Friend Function Scostamenti(criterio As Profilo, profilo As Profilo) As List(Of String)

        Dim visti As New List(Of String)

        If criterio Is Nothing Then Throw New ArgumentNullException(NameOf(criterio))

        If profilo Is Nothing Then
            visti.Add("dall'import non è uscito nessun profilo")
            Return visti
        End If

        ' --- I valori che il CV scrive nero su bianco e il prompt ordina di copiare. ---
        Confronta(visti, "nome", criterio.Nome, profilo.Nome)
        Confronta(visti, "contatti.email", criterio.Contatti.Email, profilo.Contatti.Email)
        Confronta(visti, "contatti.telefono", criterio.Contatti.Telefono, profilo.Contatti.Telefono)
        Confronta(visti, "contatti.link", criterio.Contatti.Link, profilo.Contatti.Link)
        Confronta(visti, "patente.ha", criterio.Patente.Ha, profilo.Patente.Ha)
        ' Ordinate prima di confrontarle: che la B venga prima o dopo la C è impaginazione
        ' della lista, non un fatto diverso.
        Confronta(visti, "patente.categorie",
                  Categorie(criterio.Patente.Categorie), Categorie(profilo.Patente.Categorie))

        ' La città ha una riga sua perché ha una regola sua: dal pool 1.02 è il domicilio,
        ' ed è una sola. Un CV che dichiara due indirizzi è il posto in cui quella regola
        ' si vede o non si vede.
        Confronta(visti, "contatti.citta (dev'essere il domicilio)",
                  criterio.Contatti.Citta, profilo.Contatti.Citta)

        ' --- Le sezioni che il CV nomina una per una: quante voci. --------------------
        Conta(visti, "esperienze_formali", criterio.EsperienzeFormali.Count,
              profilo.EsperienzeFormali.Count)
        Conta(visti, "formazione", criterio.Formazione.Count, profilo.Formazione.Count)

        ' Le informali no: quel racconto lo si può distillare in una voce o in due, ed è
        ' un giudizio legittimo. Si pretende che non ne manchi nessuna, non che siano
        ' contate come le ha contate il criterio.
        If profilo.EsperienzeInformali.Count < criterio.EsperienzeInformali.Count Then
            visti.Add(
                $"esperienze_informali: il criterio ne ha {criterio.EsperienzeInformali.Count}, " &
                $"il profilo {profilo.EsperienzeInformali.Count} — qualcuna è andata persa " &
                "(spezzarne una in due sarebbe invece un giudizio legittimo)")
        End If

        ' Sulle competenze nessun conteggio, per la lezione di T2: a testo identico il
        ' modello ne distilla ogni volta un po' di più o un po' di meno. Una lista vuota è
        ' però un'altra cosa — vorrebbe dire che il turno non ha distillato nulla.
        If profilo.Competenze.Count = 0 Then
            visti.Add("competenze: nessuna, e da un CV intero qualcuna deve uscire")
        End If

        ' --- I nomi propri che devono esserci: datori di lavoro e istituti. -----------
        For Each formale As EsperienzaFormale In criterio.EsperienzeFormali
            If Appaiabile(formale.Azienda).Length = 0 Then Continue For
            If profilo.EsperienzeFormali.Any(
                   Function(v) Appaiabile(v.Azienda) = Appaiabile(formale.Azienda)) Then Continue For
            visti.Add(
                $"esperienze_formali: «{Corto(formale.Azienda)}» è nel criterio ma non nel profilo")
        Next

        For Each voce As VoceFormazione In criterio.Formazione
            If Appaiabile(voce.Istituto).Length = 0 Then Continue For
            If profilo.Formazione.Any(
                   Function(v) Appaiabile(v.Istituto) = Appaiabile(voce.Istituto)) Then Continue For
            visti.Add($"formazione: «{Corto(voce.Istituto)}» è nel criterio ma non nel profilo")
        Next

        ' --- E la parte per cui questo controllo esiste: la collocazione. -------------
        visti.AddRange(InformaliPromosse(criterio, profilo))

        Return visti

    End Function

    ''' <summary>
    ''' Le attività che il criterio colloca fra le <b>informali</b> e che il profilo ha
    ''' messo fra i <b>lavori</b>. È il difetto che né i doppioni né la parola
    ''' «volontario» sanno vedere.
    ''' </summary>
    Friend Function InformaliPromosse(criterio As Profilo, profilo As Profilo) As List(Of String)

        Dim promosse As New List(Of String)
        If criterio Is Nothing OrElse profilo Is Nothing Then Return promosse

        ' Il contrasto: le parole che nel criterio appartengono già ai lavori veri non
        ' distinguono un'attività informale da loro, e vanno tolte prima di cercare.
        Dim deiLavori As HashSet(Of String) = ParoleDi(
            criterio.EsperienzeFormali.Select(
                Function(f) f.Ruolo & " " & f.Azienda & " " & f.CosaFacevo))

        For Each informale As EsperienzaInformale In criterio.EsperienzeInformali

            Dim distintive As HashSet(Of String) = ParoleDi(
                {informale.CosaFacevo & " " & informale.ConChi})
            distintive.ExceptWith(deiLavori)

            If distintive.Count < IncontriPerAppaiare Then Continue For

            For Each formale As EsperienzaFormale In profilo.EsperienzeFormali

                Dim incontrate As List(Of String) = ParoleDi(
                    {formale.Ruolo & " " & formale.Azienda & " " & formale.CosaFacevo}).
                    Where(AddressOf distintive.Contains).OrderBy(Function(p) p).ToList()

                If incontrate.Count < IncontriPerAppaiare Then Continue For

                promosse.Add(
                    $"«{Corto(informale.CosaFacevo)}» sta fra le esperienze formali " &
                    $"come «{Corto(formale.Ruolo)}», ma il criterio la colloca fra le " &
                    $"informali (parole in comune: {String.Join(", ", incontrate)})")

            Next

        Next

        Return promosse

    End Function

    ' --- Gli attrezzi -------------------------------------------------------------

    ''' <summary>Aggiunge lo scostamento se i due valori non dicono la stessa cosa.</summary>
    Private Sub Confronta(visti As List(Of String), campo As String,
                          atteso As String, ottenuto As String)

        If Appaiabile(atteso) = Appaiabile(ottenuto) Then Exit Sub

        visti.Add($"{campo}: il criterio dice «{Corto(atteso)}», il profilo «{Corto(ottenuto)}»")

    End Sub

    ''' <summary>Aggiunge lo scostamento se la sezione non ha le voci che deve.</summary>
    Private Sub Conta(visti As List(Of String), sezione As String,
                      atteso As Integer, ottenuto As Integer)

        If atteso = ottenuto Then Exit Sub

        visti.Add($"{sezione}: il criterio ne ha {atteso}, il profilo {ottenuto}")

    End Sub

    ''' <summary>Le categorie della patente in una riga confrontabile, ordine indifferente.</summary>
    Private Function Categorie(voci As IEnumerable(Of String)) As String

        ' Non «categorie»: in VB il parametro non può chiamarsi come la funzione.
        Return String.Join(", ", voci.Select(AddressOf Appaiabile).
                                 Where(Function(c) c.Length > 0).OrderBy(Function(c) c))

    End Function

    ''' <summary>Le parole di uno o più testi che possono distinguere un'attività.</summary>
    Private Function ParoleDi(testi As IEnumerable(Of String)) As HashSet(Of String)

        Dim parole As New HashSet(Of String)

        For Each testo As String In testi
            For Each trovata As Match In Parola.Matches(Normalizzato(testo))
                Dim parola As String = trovata.Value
                If DiServizio.Contains(parola) Then Continue For
                parole.Add(parola)
            Next
        Next

        Return parole

    End Function

    ''' <summary>
    ''' Un valore ridotto a come si confronta: spazi di troppo via (quelli sono
    ''' impaginazione), maiuscole indifferenti, accenti in una forma sola — «Forlì»
    ''' scritto in due modi diversi è la stessa città.
    ''' </summary>
    Private Function Appaiabile(valore As String) As String

        Return String.Join(" ", Normalizzato(valore).
            Split({" "c, vbTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries))

    End Function

    ''' <summary>Il testo in minuscolo e con gli accenti in forma composta.</summary>
    Private Function Normalizzato(testo As String) As String

        Return If(testo, String.Empty).Normalize(NormalizationForm.FormC).ToLowerInvariant()

    End Function

    ''' <summary>Un testo abbastanza corto per stare su una riga.</summary>
    Private Function Corto(valore As String) As String

        Dim pulito As String = String.Join(" ", If(valore, String.Empty).
            Split({" "c, vbTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries))

        If pulito.Length = 0 Then Return "(vuoto)"
        If pulito.Length <= Quanto Then Return pulito

        Return pulito.Substring(0, Quanto).TrimEnd() & "…"

    End Function

End Module
