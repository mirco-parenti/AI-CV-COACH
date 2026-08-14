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
Friend Class GeneratoreFinto
    Inherits MestiereFinto
    Implements IGeneratore

    Public Function GeneraCvBaseAsync(profilo As JsonNode,
                                      Optional annulla As CancellationToken = Nothing) _
                                      As Task(Of JsonNode) Implements IGeneratore.GeneraCvBaseAsync

        Return Prossima("cv_base", profilo)

    End Function

    Public Function GeneraCvMiratoAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                        Optional annulla As CancellationToken = Nothing) _
                                        As Task(Of JsonNode) Implements IGeneratore.GeneraCvMiratoAsync

        Return Prossima("cv_mirato", profilo, annuncio, giudizi)

    End Function

    Public Function GeneraLetteraAsync(profilo As JsonNode, annuncio As JsonNode, giudizi As JsonNode,
                                       cv As JsonNode, mitigazioni As JsonNode,
                                       Optional annulla As CancellationToken = Nothing) _
                                       As Task(Of JsonNode) Implements IGeneratore.GeneraLetteraAsync

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
Friend Class CompositoreFinto
    Inherits MestiereFinto
    Implements ICompositoreEmail

    ''' <summary>Gli elenchi di allegati ricevuti, uno per chiamata.</summary>
    Public ReadOnly Property AllegatiNominati As New List(Of List(Of String))

    Public Function ComponiAsync(lettera As JsonNode, annuncio As JsonNode,
                                 allegati As IEnumerable(Of String),
                                 Optional annulla As CancellationToken = Nothing) _
                                 As Task(Of JsonNode) Implements ICompositoreEmail.ComponiAsync

        AllegatiNominati.Add(If(allegati Is Nothing, New List(Of String), allegati.ToList()))

        Return Prossima("email", lettera, annuncio)

    End Function

End Class
