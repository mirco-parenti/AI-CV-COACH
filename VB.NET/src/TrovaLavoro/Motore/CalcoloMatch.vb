Imports System.Globalization
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Motore

    ''' <summary>
    ''' L'esito del calcolo del match: gli stessi campi restituiti dal prototipo.
    ''' I valori che il prototipo può lasciare a <c>null</c> sono qui annullabili.
    ''' </summary>
    Public Class RisultatoMatch
        ''' <summary>Il punteggio del solo conteggio dei requisiti, prima dell'AI.</summary>
        Public Property ScoreBase As Integer?
        ''' <summary>La stima d'insieme dell'AI, se utilizzabile.</summary>
        Public Property NumeroLlm As Double?
        ''' <summary>Il match finale 0-100, dopo fusione, clamp ed eventuale tetto.</summary>
        Public Property MatchFinale As Integer?
        ''' <summary>Il match convertito in stelle 0-5, con un decimale.</summary>
        Public Property Stelle As Double?
        ''' <summary>Vero se il clamp ha tagliato lo scarto fra conteggio e stima dell'AI.</summary>
        Public Property ScartoTagliato As Boolean
        ''' <summary>Vero se un requisito eliminatorio non soddisfatto ha imposto il tetto.</summary>
        Public Property GateEliminatorio As Boolean
        ''' <summary>La spiegazione da mostrare all'utente; <c>Nothing</c> se non serve.</summary>
        Public Property Nota As String
    End Class

    ''' <summary>
    ''' Dai giudizi dell'AI (Giro 1) e dal suo numero complessivo calcola il match
    ''' finale (Giro 2). È la trascrizione fedele di <c>calcolaMatch</c> del prototipo
    ''' (HTML+JS/server.js): stessi pesi, stesso clamp asimmetrico, stesso tetto
    ''' eliminatorio, stesse note. Codice deterministico: dato lo stesso input, stesso
    ''' punteggio del prototipo — che è anche il suo collaudo (cap. 14, T2).
    ''' </summary>
    Public Module CalcoloMatch

        ''' <summary>
        ''' Calcola il match.
        ''' </summary>
        ''' <param name="giudizi">I giudizi voce-per-voce prodotti dal confronto.</param>
        ''' <param name="numeroComplessivo">La stima d'insieme dell'AI (0-100).</param>
        ''' <param name="taratura">I numeri del calcolo; se omessa, i valori di prodotto.</param>
        Public Function Calcola(giudizi As JsonArray,
                                numeroComplessivo As JsonNode,
                                Optional taratura As Taratura = Nothing) As RisultatoMatch

            Dim t As Taratura = If(taratura, Taratura.Predefinita())

            Dim numeratore As Double = 0
            Dim denominatore As Double = 0
            Dim gateFalliti As New List(Of String)   ' requisiti ELIMINATORI non soddisfatti

            If giudizi IsNot Nothing Then
                For Each g As JsonNode In giudizi

                    ' Le voci che dichiarano l'ASSENZA di un requisito (es. "Nessuna
                    ' esperienza richiesta", sentinel della pipeline 2) non sono
                    ' requisiti da soddisfare: restano fuori dal conteggio in modo
                    ' deterministico, senza dipendere dall'AI.
                    If Norm(Campo(g, "requisito")).Contains("nessuna esperienza richiesta") Then Continue For

                    Dim esito As String = Norm(Campo(g, "esito"))
                    If esito = "non determinabile" Then Continue For   ' escluso dal conteggio

                    Dim punti As Double
                    If Not t.Punti.TryGetValue(esito, punti) Then Continue For  ' esito non riconosciuto

                    Dim peso As Double = PesoVoce(g, t)
                    numeratore += punti * peso
                    denominatore += peso

                    ' Hard-gate: un requisito eliminatorio non soddisfatto craterà il
                    ' match (sotto). Il flag è accettato sia booleano sia come stringa,
                    ' esattamente come nel prototipo.
                    If esito = "non soddisfatto" AndAlso EliminatorioVero(Campo(g, "eliminatorio")) Then
                        gateFalliti.Add(Testo(Campo(g, "requisito")))
                    End If

                Next
            End If

            Dim numLlm As Double = NumeroJs(numeroComplessivo)
            Dim llmValido As Boolean = Not (Double.IsNaN(numLlm) OrElse Double.IsInfinity(numLlm))

            Dim scoreBase As Integer? = Nothing
            Dim finale As Integer? = Nothing
            Dim tagliato As Boolean = False
            Dim nota As String = Nothing

            If denominatore = 0 Then
                ' Nessuna voce determinabile: il conteggio non esiste, ci si affida
                ' al numero dell'AI.
                If llmValido Then finale = CInt(Clamp(Arrotonda(numLlm), 0, 100))
            Else
                scoreBase = CInt(Arrotonda(100 * numeratore / denominatore))
                If Not llmValido Then
                    finale = CInt(Clamp(scoreBase.Value, 0, 100))
                Else
                    Dim delta As Double = numLlm - scoreBase.Value
                    Dim corr As Double = Clamp(delta, t.ClampGiu, t.ClampSu)  ' asimmetrico: anti-invenzione
                    finale = CInt(Clamp(Arrotonda(scoreBase.Value + corr), 0, 100))
                    tagliato = delta < t.ClampGiu OrElse delta > t.ClampSu
                End If
            End If

            ' Hard-gate: se c'è almeno un requisito ELIMINATORIO non soddisfatto, il
            ' match non può superare il tetto (≤ 1 stella), a prescindere dal resto del
            ' profilo. Applicato dopo il clamp; le note si compongono coi valori
            ' definitivi per restare coerenti.
            Dim finaleAnteGate As Integer? = finale
            Dim gateEliminatorio As Boolean = False
            If finale.HasValue AndAlso gateFalliti.Count > 0 Then
                finale = CInt(Math.Min(finale.Value, t.TettoEliminatorio))
                gateEliminatorio = True
            End If

            If tagliato Then
                nota = $"Il conteggio dei requisiti darebbe {Numero(scoreBase.Value)}, ma la valutazione " &
                       $"d'insieme dell'AI lo porta verso {Numero(numLlm)}: match finale " &
                       $"{Numero(finaleAnteGate.Value)}."
            End If

            If gateEliminatorio Then
                Dim notaGate As String =
                    $"Requisito eliminatorio non soddisfatto ({String.Join("; ", gateFalliti)}): " &
                    $"il match non può superare {Numero(t.TettoEliminatorio)}/100, cioè ≤ 1 stella, " &
                    "a prescindere dal resto del profilo."
                nota = If(nota Is Nothing, notaGate, $"{nota} {notaGate}")
            End If

            ' Passo finale: il match vero, convertito in stelle 0-5 con un decimale.
            Dim stelle As Double? = Nothing
            If finale.HasValue Then stelle = Arrotonda(finale.Value / 20.0 * 10) / 10

            Return New RisultatoMatch With {
                .ScoreBase = scoreBase,
                .NumeroLlm = If(llmValido, CType(numLlm, Double?), Nothing),
                .MatchFinale = finale,
                .Stelle = stelle,
                .ScartoTagliato = tagliato,
                .GateEliminatorio = gateEliminatorio,
                .Nota = nota
            }

        End Function

        ''' <summary>
        ''' Peso di una singola voce giudicata: il contesto vale un quinto; nel nucleo
        ''' conta la priorità, e le "non specificata" usano l'importanza stimata dall'AI.
        ''' </summary>
        Private Function PesoVoce(g As JsonNode, t As Taratura) As Double

            If Norm(Campo(g, "categoria")) = "contesto" Then Return t.PesoContesto

            Dim priorita As String = Norm(Campo(g, "priorita"))
            Dim peso As Double

            If priorita = "non specificata" Then
                If t.PesoImportanza.TryGetValue(Norm(Campo(g, "importanza")), peso) Then Return peso
                Return t.PesoFallback
            End If

            If t.PesoPriorita.TryGetValue(priorita, peso) Then Return peso
            Return t.PesoFallback

        End Function

        ''' <summary>Il campo di un giudizio, oppure <c>Nothing</c> se assente.</summary>
        Private Function Campo(g As JsonNode, nome As String) As JsonNode
            Dim oggetto As JsonObject = TryCast(g, JsonObject)
            If oggetto Is Nothing Then Return Nothing
            Dim valore As JsonNode = Nothing
            oggetto.TryGetPropertyValue(nome, valore)
            Return valore
        End Function

        ''' <summary>
        ''' L'equivalente di <c>norm</c> del prototipo: minuscolo e senza spazi ai bordi
        ''' se il valore è una stringa, altrimenti stringa vuota. Il controllo sul tipo
        ''' non è pignoleria: replica <c>typeof x === "string"</c>, ed è ciò che rende
        ''' <c>Norm(true)</c> uguale a "" e non a "true".
        ''' </summary>
        Private Function Norm(nodo As JsonNode) As String
            If nodo Is Nothing OrElse nodo.GetValueKind() <> JsonValueKind.String Then Return String.Empty
            Return nodo.GetValue(Of String)().Trim().ToLowerInvariant()
        End Function

        ''' <summary>Il valore come testo, per comporre le note (requisito originale, non normalizzato).</summary>
        Private Function Testo(nodo As JsonNode) As String
            If nodo Is Nothing Then Return String.Empty
            If nodo.GetValueKind() = JsonValueKind.String Then Return nodo.GetValue(Of String)()
            Return nodo.ToJsonString()
        End Function

        ''' <summary>
        ''' Il flag eliminatorio come lo intende il prototipo:
        ''' <c>g.eliminatorio === true || norm(g.eliminatorio) === "true"</c>,
        ''' cioè il booleano vero oppure la stringa "true" scritta in qualunque modo.
        ''' </summary>
        Private Function EliminatorioVero(nodo As JsonNode) As Boolean
            If nodo Is Nothing Then Return False
            If nodo.GetValueKind() = JsonValueKind.True Then Return True
            Return Norm(nodo) = "true"
        End Function

        ''' <summary>
        ''' L'equivalente di <c>Number(x)</c> di JavaScript, che non coincide con una
        ''' conversione .NET: <c>null</c> vale 0 (non "non valido"), la stringa vuota
        ''' vale 0, un campo assente è NaN. Il punto decimale è sempre quello inglese,
        ''' perché così lo scrive il JSON.
        ''' </summary>
        Private Function NumeroJs(nodo As JsonNode) As Double

            If nodo Is Nothing Then Return Double.NaN            ' undefined

            Select Case nodo.GetValueKind()

                Case JsonValueKind.Null : Return 0               ' Number(null) === 0
                Case JsonValueKind.True : Return 1
                Case JsonValueKind.False : Return 0

                Case JsonValueKind.Number
                    ' Non si può usare GetValue(Of Double): un nodo costruito in memoria
                    ' conserva il tipo con cui è nato (Integer, Decimal…) e rifiuta la
                    ' conversione, mentre uno letto da testo la accetta. Passare dalla
                    ' scrittura JSON del numero funziona in entrambi i casi, e quella
                    ' scrittura ha sempre il punto decimale.
                    Dim numero As Double
                    If Double.TryParse(nodo.ToJsonString(), NumberStyles.Float,
                                       CultureInfo.InvariantCulture, numero) Then Return numero
                    Return Double.NaN

                Case JsonValueKind.String
                    Dim s As String = nodo.GetValue(Of String)().Trim()
                    If s.Length = 0 Then Return 0                ' Number("") === 0
                    Dim d As Double
                    If Double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, d) Then Return d
                    Return Double.NaN

                Case Else
                    ' Oggetti e array non arrivano mai qui dallo schema del confronto.
                    Return Double.NaN

            End Select

        End Function

        ''' <summary>
        ''' L'equivalente di <c>Math.round</c> di JavaScript, che arrotonda i mezzi
        ''' <b>verso l'alto</b>. Non si può usare <c>Math.Round</c> di .NET, che invece
        ''' porta i mezzi al pari (66,5 diventerebbe 66 invece di 67): sui numeri
        ''' frazionari dell'AI darebbe punteggi diversi da quelli del prototipo.
        ''' </summary>
        Private Function Arrotonda(x As Double) As Double
            Return Math.Floor(x + 0.5)
        End Function

        ''' <summary>Limita un valore fra due estremi, come il <c>clamp</c> del prototipo.</summary>
        Private Function Clamp(x As Double, minimo As Double, massimo As Double) As Double
            Return Math.Max(minimo, Math.Min(massimo, x))
        End Function

        ''' <summary>
        ''' Un numero scritto come lo scriverebbe JavaScript dentro una stringa: punto
        ''' decimale e nessuno zero superfluo. Con le impostazioni italiane .NET userebbe
        ''' la virgola, e le note non coinciderebbero più con quelle del prototipo.
        ''' </summary>
        Private Function Numero(x As Double) As String
            Return x.ToString("R", CultureInfo.InvariantCulture)
        End Function

    End Module

End Namespace
