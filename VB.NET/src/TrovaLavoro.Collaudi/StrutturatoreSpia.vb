Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai

''' <summary>
''' Lo strutturatore <b>vero</b> con un taccuino accanto: passa ogni domanda a chi la sa
''' fare davvero e annota che cosa gli è stato chiesto e che cosa è tornato.
''' </summary>
''' <remarks>
''' <para>È il gemello di <see cref="StrutturatoreFinto"/> per il collaudo reale del
''' dialogo (cap. 14, T3, gamba C). Là l'AI non c'è e i frammenti li prepara il collaudo;
''' qui l'AI c'è e i frammenti non si conoscono in anticipo — ma per giudicare
''' l'<b>anti-perdita</b> bisogna sapere che cosa il modello ha instradato «altrove»,
''' e quello lo si vede solo passando di qui.</para>
''' <para>Non cambia niente di ciò che il dialogo riceve: restituisce il frammento tale e
''' quale, e un errore lo rilancia dopo averlo annotato. Se lo alterasse, misurerebbe
''' sé stesso.</para>
''' </remarks>
Friend Class StrutturatoreSpia
    Implements IStrutturatoreTurni

    ''' <summary>Le quattro categorie che possono ricevere materiale «da altrove».</summary>
    ''' <remarks>
    ''' Sono quelle che il dialogo sa smaltire (<c>DialogoProfilo</c>): un frammento
    ''' instradato altrove — verso <c>patente</c>, per dire — non ha un turno che lo
    ''' ripeschi, e va guardato a parte invece di essere preteso.
    ''' </remarks>
    Friend Shared ReadOnly Categorie As String() =
        {"esperienze_formali", "esperienze_informali", "competenze", "formazione"}

    ''' <summary>Una chiamata annotata: cosa è stato chiesto, e cosa ne è tornato.</summary>
    Friend Class Chiamata

        Public Property Turno As String
        Public Property Risposta As String

        ''' <summary>Il frammento restituito dall'AI; <c>Nothing</c> se è fallita.</summary>
        Public Property Frammento As JsonNode

        ''' <summary>Il messaggio dell'errore, quando l'AI non ha risposto.</summary>
        Public Property Errore As String

    End Class

    ''' <summary>Un frammento che l'AI ha instradato a un altro turno.</summary>
    Friend Class Instradato

        ''' <summary>Il turno in cui l'utente l'aveva detto.</summary>
        Public Property Da As String

        ''' <summary>La categoria a cui l'AI l'ha destinato.</summary>
        Public Property Verso As String

        ''' <summary>Le parole dell'utente, così come l'AI le ha ricopiate.</summary>
        Public Property Frase As String

    End Class

    Private ReadOnly _vero As IStrutturatoreTurni

    ''' <param name="vero">Chi struttura davvero: la spia non fa altro che guardarlo.</param>
    Public Sub New(vero As IStrutturatoreTurni)

        If vero Is Nothing Then Throw New ArgumentNullException(NameOf(vero))
        _vero = vero

    End Sub

    ''' <summary>Tutto ciò che è stato chiesto, nell'ordine.</summary>
    Public ReadOnly Property Chiamate As New List(Of Chiamata)

    Public Async Function StrutturaAsync(turno As String, risposta As String,
                                         Optional annulla As CancellationToken = Nothing) _
                                         As Task(Of JsonNode) Implements IStrutturatoreTurni.StrutturaAsync

        Dim annotata As New Chiamata With {.Turno = turno, .Risposta = risposta}
        Chiamate.Add(annotata)

        Try
            Dim uscita As JsonNode = Await _vero.StrutturaAsync(turno, risposta, annulla).ConfigureAwait(False)
            annotata.Frammento = uscita
            Return uscita
        Catch ex As ErroreAi
            ' Annotato e rilanciato tale e quale: come il dialogo si comporta davanti a
            ' un'AI che non risponde è parte di ciò che il collaudo sta guardando.
            annotata.Errore = ex.Message
            Throw
        End Try

    End Function

    ''' <summary>I turni chiesti finora, in ordine, come una sola stringa leggibile.</summary>
    Public Function TurniChiesti() As String
        Return String.Join(" → ", Chiamate.Select(Function(c) c.Turno))
    End Function

    ''' <summary>
    ''' Ogni frammento che l'AI ha messo in «altrove», con il turno da cui viene e la
    ''' categoria a cui l'ha destinato. È la materia prima del giudizio sull'anti-perdita:
    ''' di ognuno di questi il dialogo deve poi rendere conto.
    ''' </summary>
    ''' <param name="soloCategorieDelDialogo">
    ''' Se tenere solo le quattro categorie che il dialogo sa ripescare
    ''' (<see cref="Categorie"/>). Con <c>False</c> tornano anche le altre — quelle che
    ''' nessun turno ripesca, e che si guardano invece di pretenderle.
    ''' </param>
    Public Function Altrove(Optional soloCategorieDelDialogo As Boolean = True) As List(Of Instradato)

        Dim raccolti As New List(Of Instradato)

        For Each chiamata As Chiamata In Chiamate

            Dim oggetto As JsonObject = TryCast(chiamata.Frammento, JsonObject)
            Dim altroveDelFrammento As JsonObject = TryCast(oggetto?("altrove"), JsonObject)
            If altroveDelFrammento Is Nothing Then Continue For

            For Each voce As KeyValuePair(Of String, JsonNode) In altroveDelFrammento

                If soloCategorieDelDialogo AndAlso Not Categorie.Contains(voce.Key) Then Continue For

                Dim elenco As JsonArray = TryCast(voce.Value, JsonArray)
                If elenco Is Nothing Then Continue For

                For Each frase As JsonNode In elenco
                    Dim valore As JsonValue = TryCast(frase, JsonValue)
                    If valore Is Nothing Then Continue For
                    If valore.ToString().Trim() = "" Then Continue For

                    raccolti.Add(New Instradato With {
                        .Da = chiamata.Turno,
                        .Verso = voce.Key,
                        .Frase = valore.ToString().Trim()})
                Next

            Next

        Next

        Return raccolti

    End Function

End Class
