Imports System.Globalization
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Dati

    ''' <summary>
    ''' Come si legge un campo dai JSON che l'applicazione scrive nella cartella dati: un
    ''' campo che manca, che è nullo o che ha un tipo diverso da quello atteso <b>non è un
    ''' errore</b>, è un campo che non c'è.
    ''' </summary>
    ''' <remarks>
    ''' <para>Nasce a T5c per la stessa ragione di <see cref="CartellaDati"/>: dallo stesso
    ''' <c>stato.json</c> adesso leggono in due — <see cref="ArchivioOpportunita"/>, che
    ''' riapre una candidatura, e <see cref="ArchivioRegistro"/>, che ricostruisce la vista
    ''' d'insieme — e la stessa mezza dozzina di righe difensive sarebbe finita in due
    ''' posti.</para>
    ''' <para>La coppia <see cref="Quando"/> / <see cref="Istante"/> sta qui insieme
    ''' apposta: sono lo stesso formato letto da un verso e scritto dall'altro, e separarli
    ''' vorrebbe dire poterli far divergere.</para>
    ''' </remarks>
    Friend NotInheritable Class CampiJson

        ''' <summary>Il formato delle date su disco: ordinabile e leggibile a occhio.</summary>
        Private Const FormatoIstante As String = "yyyy-MM-dd HH:mm:ss"

        Private Sub New()
        End Sub

        ''' <summary>Il nodo di quel campo, o <c>Nothing</c>.</summary>
        Friend Shared Function Nodo(oggetto As JsonObject, campo As String) As JsonNode

            Dim valore As JsonNode = Nothing
            If oggetto Is Nothing OrElse Not oggetto.TryGetPropertyValue(campo, valore) Then Return Nothing

            Return valore

        End Function

        ''' <summary>Un campo di testo; <c>Nothing</c> se manca o non è un testo.</summary>
        Friend Shared Function Testo(oggetto As JsonObject, campo As String) As String

            Dim valore As JsonNode = Nodo(oggetto, campo)
            If valore Is Nothing OrElse valore.GetValueKind() <> JsonValueKind.String Then Return Nothing

            Return valore.GetValue(Of String)()

        End Function

        ''' <summary>Un numero intero; <c>Nothing</c> se manca o non è un numero.</summary>
        Friend Shared Function Intero(oggetto As JsonObject, campo As String) As Integer?

            Dim valore As JsonNode = Nodo(oggetto, campo)
            If valore Is Nothing OrElse valore.GetValueKind() <> JsonValueKind.Number Then Return Nothing

            Return valore.GetValue(Of Integer)()

        End Function

        ''' <summary>Un numero con la virgola; <c>Nothing</c> se manca o non è un numero.</summary>
        Friend Shared Function Numero(oggetto As JsonObject, campo As String) As Double?

            Dim valore As JsonNode = Nodo(oggetto, campo)
            If valore Is Nothing OrElse valore.GetValueKind() <> JsonValueKind.Number Then Return Nothing

            Return valore.GetValue(Of Double)()

        End Function

        ''' <summary>
        ''' Un sì o un no. Tutto ciò che non è un <c>true</c> vale no: un campo assente non
        ''' è un'affermazione.
        ''' </summary>
        Friend Shared Function Vero(oggetto As JsonObject, campo As String) As Boolean

            Dim valore As JsonNode = Nodo(oggetto, campo)
            Return valore IsNot Nothing AndAlso valore.GetValueKind() = JsonValueKind.True

        End Function

        ''' <summary>
        ''' Un sì o un no, dicendo cosa vale l'<b>assenza</b> del campo. Non sempre «non
        ''' detto» vuol dire no: un allegato scritto senza la spunta è un allegato che
        ''' parte, perché è finito in quell'elenco per essere spedito (cap. 07.1).
        ''' </summary>
        Friend Shared Function Vero(oggetto As JsonObject, campo As String, quandoManca As Boolean) As Boolean

            Dim valore As JsonNode = Nodo(oggetto, campo)
            If valore Is Nothing Then Return quandoManca

            Return valore.GetValueKind() = JsonValueKind.True

        End Function

        ''' <summary>
        ''' Un istante; <c>Nothing</c> (cioè la data vuota) se manca o non è scritto nel
        ''' formato che <see cref="Quando"/> produce.
        ''' </summary>
        Friend Shared Function Istante(oggetto As JsonObject, campo As String) As Date

            Dim scritto As String = Testo(oggetto, campo)
            Dim letto As Date

            If scritto IsNot Nothing AndAlso Date.TryParseExact(
                scritto, FormatoIstante, CultureInfo.InvariantCulture,
                DateTimeStyles.None, letto) Then Return letto

            Return Nothing

        End Function

        ''' <summary>
        ''' Un istante come si scrive: ordinabile e leggibile. La data vuota non si scrive
        ''' affatto — diventa un <c>null</c>, che è ciò che è.
        ''' </summary>
        Friend Shared Function Quando(istante As Date) As String

            If istante = Nothing Then Return Nothing

            Return istante.ToString(FormatoIstante, CultureInfo.InvariantCulture)

        End Function

    End Class

End Namespace
