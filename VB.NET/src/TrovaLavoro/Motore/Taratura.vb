Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Motore

    ''' <summary>Da dove arrivano i numeri della taratura in uso.</summary>
    Public Enum OrigineTaratura
        ''' <summary>I valori che il programma porta dentro di sé (cap. 11.6).</summary>
        Predefinita
        ''' <summary>Il file taratura.json della cartella dati.</summary>
        File
    End Enum

    ''' <summary>
    ''' I numeri del calcolo delle stelle: soglia, pesi e limiti (cap. 11.6).
    ''' Sono valori <b>di prodotto, non preferenze</b>: l'interfaccia non li mostra e
    ''' non li lascia toccare, perché le stelle devono restare confrontabili fra un
    ''' annuncio e l'altro. Stanno fuori dal codice per un motivo pratico opposto:
    ''' durante la messa a punto ritoccare un valore deve costare una riga, non una
    ''' nuova versione da ricompilare e reinstallare su due macchine.
    ''' </summary>
    Public Class Taratura

        ' I valori predefiniti sono la trascrizione delle costanti del prototipo
        ' (HTML+JS/server.js): PUNTI, PESO_PRIORITA, PESO_IMPORTANZA, PESO_CONTESTO,
        ' PESO_FALLBACK, CLAMP_GIU, CLAMP_SU, TETTO_ELIMINATORIO. Le chiavi sono
        ' identiche a quelle del prototipo — anche negli spazi ("in parte") — perché
        ' vengono confrontate con gli esiti normalizzati che arrivano dall'AI.

        ''' <summary>Punti per esito. L'esito "non determinabile" non compare: è escluso dal conteggio.</summary>
        Public Property Punti As Dictionary(Of String, Double)

        ''' <summary>Peso delle voci del nucleo, per priorità dichiarata.</summary>
        Public Property PesoPriorita As Dictionary(Of String, Double)

        ''' <summary>Peso delle voci "non specificata", per importanza stimata dall'AI.</summary>
        Public Property PesoImportanza As Dictionary(Of String, Double)

        ''' <summary>Peso di una voce di contesto: un quinto di un requisito del nucleo.</summary>
        Public Property PesoContesto As Double

        ''' <summary>Peso neutro quando né priorità né importanza sono riconoscibili.</summary>
        Public Property PesoFallback As Double

        ''' <summary>Quanto la valutazione d'insieme dell'AI può abbassare il conteggio.</summary>
        Public Property ClampGiu As Double

        ''' <summary>Quanto può alzarlo: meno margine, per anti-invenzione.</summary>
        Public Property ClampSu As Double

        ''' <summary>Tetto imposto da un requisito eliminatorio non soddisfatto (≤ 1 stella).</summary>
        Public Property TettoEliminatorio As Double

        ''' <summary>Sotto questo numero di stelle la generazione è sconsigliata (Step 1.35).</summary>
        Public Property SogliaStelleGenerazione As Double

        ''' <summary>Da dove vengono i valori in uso.</summary>
        Public Property Origine As OrigineTaratura = OrigineTaratura.Predefinita

        ''' <summary>
        ''' Motivo per cui si è ripiegato sui predefiniti, da annotare nel log
        ''' (cap. 11.6); <c>Nothing</c> se non c'è stato alcun ripiego.
        ''' </summary>
        Public Property Avviso As String

        ''' <summary>I valori di prodotto, identici alle costanti del prototipo.</summary>
        Public Shared Function Predefinita() As Taratura
            Return New Taratura With {
                .Punti = New Dictionary(Of String, Double) From {
                    {"soddisfatto", 1}, {"in parte", 0.5}, {"non soddisfatto", 0}},
                .PesoPriorita = New Dictionary(Of String, Double) From {
                    {"richiesto", 5}, {"preferenziale", 1}},
                .PesoImportanza = New Dictionary(Of String, Double) From {
                    {"alta", 5}, {"media", 3}, {"bassa", 1}},
                .PesoContesto = 0.2,
                .PesoFallback = 3,
                .ClampGiu = -20,
                .ClampSu = 10,
                .TettoEliminatorio = 20,
                .SogliaStelleGenerazione = 1.5,
                .Origine = OrigineTaratura.Predefinita
            }
        End Function

        ''' <summary>
        ''' Il percorso del file di taratura nella cartella dati predefinita. Dov'è la
        ''' cartella lo sa <see cref="Dati.CartellaDati"/>, non questa classe (cap. 11.1).
        ''' </summary>
        Public Shared ReadOnly Property PercorsoPredefinito As String
            Get
                Return Dati.CartellaDati.Predefinita().FileTaratura
            End Get
        End Property

        ''' <summary>
        ''' Carica la taratura dal file indicato. Se il file manca o è illeggibile
        ''' restituisce i valori predefiniti annotando il motivo in <see cref="Avviso"/>:
        ''' una taratura corrotta non deve impedire l'avvio (cap. 11.6).
        ''' </summary>
        ''' <param name="percorso">Il file da leggere; se omesso, quello della cartella dati.</param>
        Public Shared Function Carica(Optional percorso As String = Nothing) As Taratura

            Dim file As String = If(percorso, PercorsoPredefinito)

            If Not IO.File.Exists(file) Then
                Dim t As Taratura = Predefinita()
                t.Avviso = $"Taratura non trovata in «{file}»: uso i valori predefiniti."
                Return t
            End If

            Try
                Return DaJson(IO.File.ReadAllText(file, Text.Encoding.UTF8))
            Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                       OrElse TypeOf ex Is UnauthorizedAccessException
                Dim t As Taratura = Predefinita()
                t.Avviso = $"Taratura illeggibile in «{file}» ({ex.Message}): uso i valori predefiniti."
                Return t
            End Try

        End Function

        ''' <summary>
        ''' Costruisce la taratura da un testo JSON. Ogni valore assente ricade sul
        ''' predefinito corrispondente, così un file parziale resta utilizzabile.
        ''' </summary>
        Public Shared Function DaJson(testo As String) As Taratura

            Dim radice As JsonObject = TryCast(JsonNode.Parse(testo), JsonObject)
            If radice Is Nothing Then
                Throw New JsonException("La taratura deve essere un oggetto JSON.")
            End If

            Dim t As Taratura = Predefinita()
            t.Origine = OrigineTaratura.File

            LeggiMappa(radice, "punti", t.Punti)
            LeggiMappa(radice, "peso_priorita", t.PesoPriorita)
            LeggiMappa(radice, "peso_importanza", t.PesoImportanza)

            t.PesoContesto = LeggiNumero(radice, "peso_contesto", t.PesoContesto)
            t.PesoFallback = LeggiNumero(radice, "peso_fallback", t.PesoFallback)
            t.ClampGiu = LeggiNumero(radice, "clamp_giu", t.ClampGiu)
            t.ClampSu = LeggiNumero(radice, "clamp_su", t.ClampSu)
            t.TettoEliminatorio = LeggiNumero(radice, "tetto_eliminatorio", t.TettoEliminatorio)
            t.SogliaStelleGenerazione = LeggiNumero(radice, "soglia_stelle_generazione",
                                                   t.SogliaStelleGenerazione)
            Return t

        End Function

        ''' <summary>Sostituisce una mappa di pesi solo se il JSON la dichiara.</summary>
        Private Shared Sub LeggiMappa(radice As JsonObject, chiave As String,
                                      ByRef destinazione As Dictionary(Of String, Double))

            Dim nodo As JsonObject = TryCast(radice(chiave), JsonObject)
            If nodo Is Nothing Then Return

            Dim mappa As New Dictionary(Of String, Double)
            For Each voce As KeyValuePair(Of String, JsonNode) In nodo
                ' Un valore non numerico («"1"» fra virgolette, un true) scarta la
                ' mappa intera e lascia la predefinita: prima faceva cadere l'avvio
                ' (contro il cap. 11.6), e tenere solo le voci buone falserebbe il
                ' punteggio in silenzio — una chiave assente esclude quegli esiti.
                Dim valore As JsonValue = TryCast(voce.Value, JsonValue)
                Dim numero As Double
                If valore Is Nothing OrElse Not valore.TryGetValue(Of Double)(numero) Then Return
                mappa(voce.Key.Trim().ToLowerInvariant()) = numero
            Next

            If mappa.Count > 0 Then destinazione = mappa

        End Sub

        ''' <summary>Legge un numero, lasciando il predefinito se assente o non numerico.</summary>
        Private Shared Function LeggiNumero(radice As JsonObject, chiave As String,
                                            predefinito As Double) As Double

            Dim valore As JsonValue = TryCast(radice(chiave), JsonValue)
            If valore Is Nothing Then Return predefinito

            Dim numero As Double
            Return If(valore.TryGetValue(Of Double)(numero), numero, predefinito)

        End Function

    End Class

End Namespace
