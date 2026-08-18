Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai

''' <summary>
''' I tre mestieri che i collaudi mettono al posto dell'AI nella pipeline di candidatura:
''' restituiscono gli artefatti che il collaudo ha preparato, nell'ordine in cui li ha
''' messi, e annotano cosa è stato chiesto loro. Sono ciò che permette di far girare la
''' pipeline intera — analisi, confronto, punteggio, mitigazione, documenti — senza rete
''' e senza spendere un token, come <see cref="StrutturatoreFinto"/> fa col dialogo.
''' </summary>
''' <remarks>
''' Stanno in un file solo perché si usano sempre insieme: la pipeline li vuole tutti e
''' tre, e leggerli uno accanto all'altro fa vedere che rispondono nello stesso modo.
''' </remarks>
Friend MustInherit Class MestiereFinto

    ''' <summary>Una chiamata ricevuta: quale lavoro, e con quali artefatti in ingresso.</summary>
    Friend Class Chiamata
        Public Property Lavoro As String
        Public Property Ingressi As IReadOnlyList(Of JsonNode)
    End Class

    ''' <summary>Le risposte preparate: testo JSON, oppure un'eccezione da sollevare.</summary>
    Private ReadOnly _preparate As New Queue(Of Object)

    ''' <summary>Tutto ciò che è stato chiesto, nell'ordine.</summary>
    Public ReadOnly Property Chiamate As New List(Of Chiamata)

    ''' <summary>I lavori chiesti finora, in ordine, come una sola stringa leggibile.</summary>
    Public Function LavoriChiesti() As String
        Return String.Join(" → ", Chiamate.Select(Function(c) c.Lavoro))
    End Function

    ''' <summary>Prepara il prossimo artefatto (forma fluente: si concatenano).</summary>
    Public Function Dara(artefattoJson As String) As MestiereFinto
        _preparate.Enqueue(artefattoJson)
        Return Me
    End Function

    ''' <summary>Prepara un errore al posto del prossimo artefatto.</summary>
    Public Function Fallira(errore As Exception) As MestiereFinto
        _preparate.Enqueue(errore)
        Return Me
    End Function

    ''' <summary>Annota la chiamata e restituisce ciò che era preparato.</summary>
    Protected Function Prossima(lavoro As String, ParamArray ingressi As JsonNode()) As Task(Of JsonNode)

        Chiamate.Add(New Chiamata With {.Lavoro = lavoro, .Ingressi = ingressi})

        If _preparate.Count = 0 Then
            ' Meglio un errore chiaro che un collaudo che prosegue a caso: vuol dire che
            ' la pipeline ha chiesto una cosa in più rispetto a quelle previste.
            Throw New InvalidOperationException(
                $"Il collaudo non ha preparato una risposta per «{lavoro}» " &
                $"(chiamate finora: {LavoriChiesti()}).")
        End If

        Dim preparata As Object = _preparate.Dequeue()

        Dim errore As Exception = TryCast(preparata, Exception)
        If errore IsNot Nothing Then Throw errore

        Return Task.FromResult(JsonNode.Parse(CStr(preparata)))

    End Function

End Class

''' <summary>L'analizzatore dell'annuncio, finto.</summary>
Friend Class AnalizzatoreFinto
    Inherits MestiereFinto
    Implements IAnalizzatoreAnnuncio

    ''' <summary>I testi di annuncio ricevuti, nell'ordine.</summary>
    Public ReadOnly Property Testi As New List(Of String)

    Public Function AnalizzaAsync(testoAnnuncio As String,
                                  Optional annulla As CancellationToken = Nothing) _
                                  As Task(Of JsonNode) Implements IAnalizzatoreAnnuncio.AnalizzaAsync

        Testi.Add(testoAnnuncio)
        Return Prossima("analisi")

    End Function

End Class

''' <summary>Il confrontatore, finto: giudizi e mitigazioni.</summary>
Friend Class ConfrontatoreFinto
    Inherits MestiereFinto
    Implements IConfrontatore

    Public Function ConfrontaAsync(profilo As JsonNode, annuncio As JsonNode,
                                   Optional annulla As CancellationToken = Nothing) _
                                   As Task(Of JsonNode) Implements IConfrontatore.ConfrontaAsync

        Return Prossima("confronto", profilo, annuncio)

    End Function

    Public Function MitigaAsync(profilo As JsonNode, giudizi As JsonNode,
                                Optional annulla As CancellationToken = Nothing) _
                                As Task(Of JsonNode) Implements IConfrontatore.MitigaAsync

        Return Prossima("mitigazione", profilo, giudizi)

    End Function

End Class

''' <summary>Il generatore, finto: i due CV e la lettera.</summary>
''' <remarks>
''' <b>Annota la lingua che gli è stata chiesta</b> (T7): un finto non carica prompt, e
''' quindi non potrebbe accorgersi da sé se la lingua si fermasse per strada — che è
''' esattamente il difetto che il filo dell'annuncio fino al pool può avere (cap. 10.2).
''' Senza questo, un collaudo del multilingua verificherebbe solo che nessuno è esploso.
''' </remarks>
Friend Class GeneratoreFinto
    Inherits MestiereFinto
    Implements IGeneratore

    ''' <summary>Le lingue chieste, una per chiamata e nell'ordine in cui sono arrivate.</summary>
    Friend ReadOnly Property LingueChieste As New List(Of String)

    ''' <summary>
    ''' Gli appunti di mira arrivati a ogni chiamata (T7c), per la stessa ragione delle
    ''' lingue: un finto non carica prompt, e senza annotarli nessuno si accorgerebbe se
    ''' si fermassero per strada fra la cartella della candidatura e la richiesta.
    ''' </summary>
    Friend ReadOnly Property AppuntiVisti As New List(Of JsonNode)

    Public Function GeneraCvBaseAsync(profilo As JsonNode,
                                      Optional annulla As CancellationToken = Nothing,
                                      Optional lingua As String = "it") _
                                      As Task(Of JsonNode) Implements IGeneratore.GeneraCvBaseAsync

        LingueChieste.Add(lingua)
        Return Prossima("cv_base", profilo)

    End Function

    Public Function GeneraCvMiratoAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                        Optional annulla As CancellationToken = Nothing,
                                        Optional lingua As String = "it",
                                        Optional appunti As JsonNode = Nothing) _
                                        As Task(Of JsonNode) Implements IGeneratore.GeneraCvMiratoAsync

        LingueChieste.Add(lingua)
        AppuntiVisti.Add(appunti)
        Return Prossima("cv_mirato", profilo, annuncio, giudizi)

    End Function

    Public Function GeneraLetteraAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                       cv As JsonNode, mitigazioni As JsonNode,
                                       Optional annulla As CancellationToken = Nothing,
                                       Optional lingua As String = "it",
                                       Optional appunti As JsonNode = Nothing) _
                                       As Task(Of JsonNode) Implements IGeneratore.GeneraLetteraAsync

        LingueChieste.Add(lingua)
        AppuntiVisti.Add(appunti)
        Return Prossima("lettera", profilo, annuncio, giudizi, cv, mitigazioni)

    End Function

End Class

''' <summary>
''' Il classificatore dei documenti, finto (T6). Annota <b>quali file</b> gli sono stati
''' dati da smistare: il cap. 05.2 promette che si guardi la cartella dell'utente e non
''' altro, e senza guardarli non si potrebbe verificare.
''' </summary>
Friend Class ClassificatoreFinto
    Inherits MestiereFinto
    Implements IClassificatoreDocumenti

    ''' <summary>Gli elenchi di file ricevuti, uno per chiamata.</summary>
    Public ReadOnly Property FileVisti As New List(Of List(Of TrovaLavoro.Dati.FileTrovato))

    Public Function ClassificaAsync(documenti As IEnumerable(Of TrovaLavoro.Dati.FileTrovato),
                                    Optional annulla As CancellationToken = Nothing) _
                                    As Task(Of JsonNode) Implements IClassificatoreDocumenti.ClassificaAsync

        FileVisti.Add(If(documenti Is Nothing, New List(Of TrovaLavoro.Dati.FileTrovato), documenti.ToList()))

        Return Prossima("classificazione")

    End Function

End Class

''' <summary>
''' Il compositore dell'email, finto (T6). Oltre a rispondere annota <b>quali allegati</b>
''' gli sono stati nominati: è la promessa del cap. 07.1 — il messaggio nomina quello che
''' parte davvero — e senza guardarli non si potrebbe verificare.
''' </summary>
''' <remarks>
''' <b>Annota anche la lingua</b>, per la stessa ragione di <see cref="GeneratoreFinto"/>:
''' l'email è l'ultimo anello della catena della lingua, ed è quello dove il collaudo reale
''' di T7a l'ha trovata ferma per strada — oggetto italiano sopra un corpo inglese.
''' </remarks>
Friend Class CompositoreFinto
    Inherits MestiereFinto
    Implements ICompositoreEmail

    ''' <summary>Gli elenchi di allegati ricevuti, uno per chiamata.</summary>
    Public ReadOnly Property AllegatiNominati As New List(Of List(Of String))

    ''' <summary>Le lingue chieste, una per chiamata e nell'ordine in cui sono arrivate.</summary>
    Friend ReadOnly Property LingueChieste As New List(Of String)

    Public Function ComponiAsync(lettera As JsonNode, annuncio As JsonNode,
                                 allegati As IEnumerable(Of String),
                                 Optional annulla As CancellationToken = Nothing,
                                 Optional lingua As String = "it") _
                                 As Task(Of JsonNode) Implements ICompositoreEmail.ComponiAsync

        AllegatiNominati.Add(If(allegati Is Nothing, New List(Of String), allegati.ToList()))
        LingueChieste.Add(lingua)

        Return Prossima("email", lettera, annuncio)

    End Function

End Class

''' <summary>
''' Il rifinitore anti-slop, finto (T7b): restituisce i testi che il collaudo ha
''' preparato, e annota <b>che cosa gli è stato chiesto</b> — i pezzi, il genere e la
''' lingua.
''' </summary>
''' <remarks>
''' <para>Non eredita da <see cref="MestiereFinto"/> come gli altri: quelli rispondono con
''' un artefatto JSON, questo con una mappa di testi, e la fila delle risposte preparate
''' non gli servirebbe a niente.</para>
''' <para>Il suo valore sta in ciò che <b>annota</b>. Che la rifinitura non tocchi i
''' campi-fatto non è una promessa scritta in un prompt: è che quei campi nella richiesta
''' non entrano — e l'unico modo di verificarlo è guardare che cosa parte.</para>
''' <para>Di suo <b>non cambia niente</b>: chi non prepara una risposta per un id si
''' ritrova il testo di partenza, esattamente come fa il rifinitore vero davanti a un'AI
''' che ha deciso di non toccare nulla.</para>
''' </remarks>
Friend Class RifinitoreFinto
    Implements IRifinitore

    ''' <summary>Una passata chiesta: con quali pezzi, di che genere, in che lingua.</summary>
    Friend Class Passata
        Public Property Genere As GenereProsa
        Public Property Lingua As String
        Public Property Pezzi As IReadOnlyList(Of PezzoDiProsa)

        ''' <summary>Gli id chiesti, in fila: com'è comodo leggerli in un Assert.</summary>
        Public Function Id() As String
            Return String.Join(", ", Pezzi.Select(Function(p) p.Id))
        End Function
    End Class

    ''' <summary>Tutte le passate chieste, nell'ordine.</summary>
    Public ReadOnly Property Passate As New List(Of Passata)

    ''' <summary>Cosa rispondere, per id. Un id senza risposta torna com'era.</summary>
    Private ReadOnly _risposte As New Dictionary(Of String, String)(StringComparer.Ordinal)

    ''' <summary>Un errore da sollevare invece di rifinire.</summary>
    Public Property Fallira As Exception

    ''' <summary>Prepara il testo rifinito di un id (forma fluente: si concatenano).</summary>
    Public Function Dara(id As String, testo As String) As RifinitoreFinto
        _risposte(id) = testo
        Return Me
    End Function

    ''' <summary>I generi chiesti finora, in ordine, come una sola stringa leggibile.</summary>
    Public Function GeneriChiesti() As String
        Return String.Join(" → ", Passate.Select(Function(p) p.Genere.ToString()))
    End Function

    Public Function RifinisciAsync(pezzi As IEnumerable(Of PezzoDiProsa), genere As GenereProsa,
                                   Optional annulla As CancellationToken = Nothing,
                                   Optional lingua As String = "it") _
                                   As Task(Of IReadOnlyDictionary(Of String, String)) _
                                   Implements IRifinitore.RifinisciAsync

        Dim daFare As List(Of PezzoDiProsa) = If(pezzi, Enumerable.Empty(Of PezzoDiProsa)()).
            Where(Function(p) p IsNot Nothing AndAlso
                              Not String.IsNullOrWhiteSpace(p.Id) AndAlso
                              Not String.IsNullOrWhiteSpace(p.Testo)).
            ToList()

        ' Come il vero: niente da rifinire, nessuna chiamata. Un finto che si annotasse
        ' anche le passate a vuoto farebbe contare al collaudo attese che non esistono.
        Dim esito As New Dictionary(Of String, String)(StringComparer.Ordinal)
        If daFare.Count = 0 Then
            Return Task.FromResult(Of IReadOnlyDictionary(Of String, String))(esito)
        End If

        Passate.Add(New Passata With {.Genere = genere, .Lingua = lingua, .Pezzi = daFare})

        If Fallira IsNot Nothing Then Throw Fallira

        For Each pezzo As PezzoDiProsa In daFare
            Dim rifinito As String = Nothing
            esito(pezzo.Id) = If(_risposte.TryGetValue(pezzo.Id, rifinito), rifinito, pezzo.Testo)
        Next

        Return Task.FromResult(Of IReadOnlyDictionary(Of String, String))(esito)

    End Function

End Class

''' <summary>
''' Il brainstormatore, finto: risponde quello che il collaudo gli ha messo in bocca e
''' annota che cosa gli è stato dato da guardare.
''' </summary>
''' <remarks>
''' <para><b>Consegna la risposta a pezzi</b>, parola per parola, cosa che nessun altro
''' finto fa: è l'unico modo di collaudare senza rete che la bolla di P5 cresca davvero
''' mentre l'AI scrive. Un finto che restituisse tutto in fondo lascerebbe verde anche un
''' pannello che aspetta la fine e poi stampa.</para>
''' <para><b>Annota le battute che riceve</b> per la stessa ragione per cui il generatore
''' annota la lingua: un finto non manda niente all'API, e senza contarle nessuno si
''' accorgerebbe se la conversazione si fermasse per strada invece di crescere.</para>
''' </remarks>
Friend Class BrainstormatoreFinto
    Inherits MestiereFinto
    Implements IBrainstormatore

    Private ReadOnly _dette As New Queue(Of Object)

    ''' <summary>Quante battute aveva la conversazione a ogni chiamata, nell'ordine.</summary>
    Public ReadOnly Property BattuteViste As New List(Of Integer)

    ''' <summary>L'ultima trascrizione mandata a distillare.</summary>
    Public Property UltimaConversazione As String

    ''' <summary>Le mitigazioni che si è visto passare, per controllare che arrivino.</summary>
    Public ReadOnly Property MitigazioniViste As New List(Of JsonNode)

    ''' <summary>Prepara la prossima risposta parlata (forma fluente).</summary>
    Public Function Dira(testo As String) As BrainstormatoreFinto
        _dette.Enqueue(New RispostaAi With {.Testo = testo, .MotivoFine = "end_turn"})
        Return Me
    End Function

    ''' <summary>
    ''' Prepara una risposta che si ferma contro il tetto dei token: il testo arriva, ma
    ''' il modello non ha finito la frase.
    ''' </summary>
    Public Function DiraTroncando(testo As String) As BrainstormatoreFinto
        _dette.Enqueue(New RispostaAi With {
            .Testo = testo, .MotivoFine = RispostaAi.MotivoTroncata})
        Return Me
    End Function

    ''' <summary>Prepara un errore al posto della prossima risposta parlata.</summary>
    Public Function FalliraParlando(errore As Exception) As BrainstormatoreFinto
        _dette.Enqueue(errore)
        Return Me
    End Function

    Public Function ConversaAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                  mitigazioni As JsonNode, battute As IReadOnlyList(Of TurnoChat),
                                  pezzo As Action(Of String),
                                  Optional annulla As CancellationToken = Nothing) _
                                  As Task(Of RispostaAi) Implements IBrainstormatore.ConversaAsync

        Chiamate.Add(New Chiamata With {
            .Lavoro = "conversazione", .Ingressi = {profilo, annuncio, giudizi, mitigazioni}})

        BattuteViste.Add(If(battute?.Count, 0))
        MitigazioniViste.Add(mitigazioni)

        If _dette.Count = 0 Then
            Throw New InvalidOperationException(
                "Il collaudo non ha preparato nessuna risposta per la conversazione.")
        End If

        Dim preparata As Object = _dette.Dequeue()

        Dim errore As Exception = TryCast(preparata, Exception)
        If errore IsNot Nothing Then Throw errore

        Dim esito As RispostaAi = DirectCast(preparata, RispostaAi)

        ' A pezzi come il flusso vero: le parole arrivano una alla volta, con il loro
        ' spazio attaccato, così rimesse in fila danno esattamente il testo di partenza.
        Dim parole As String() = esito.Testo.Split(" "c)
        For i As Integer = 0 To parole.Length - 1
            annulla.ThrowIfCancellationRequested()
            pezzo?.Invoke(If(i < parole.Length - 1, parole(i) & " ", parole(i)))
        Next

        Return Task.FromResult(esito)

    End Function

    Public Function AppuntiAsync(conversazione As String,
                                 Optional annulla As CancellationToken = Nothing) _
                                 As Task(Of JsonNode) Implements IBrainstormatore.AppuntiAsync

        UltimaConversazione = conversazione
        Return Prossima("appunti")

    End Function

End Class
