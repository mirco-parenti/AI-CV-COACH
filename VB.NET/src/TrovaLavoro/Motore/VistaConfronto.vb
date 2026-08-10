Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Motore

    ''' <summary>Come è andata una singola voce del confronto (prompt <c>confronto</c>).</summary>
    Public Enum EsitoGiudizio
        ''' <summary>Il profilo copre chiaramente la voce.</summary>
        Soddisfatto
        ''' <summary>La copre solo in parte, o in modo affine.</summary>
        InParte
        ''' <summary>Il profilo non la copre: è una lacuna vera.</summary>
        NonSoddisfatto
        ''' <summary>Non c'era modo di valutarla; resta fuori dal conteggio.</summary>
        NonDeterminabile
        ''' <summary>Un esito che il prompt non prevede: si mostra com'è, senza fingere di capirlo.</summary>
        Sconosciuto
    End Enum

    ''' <summary>
    ''' Un giudizio pronto da mostrare: i campi che l'AI ha dato, più il simbolo con cui
    ''' l'utente lo riconosce a colpo d'occhio (cap. 03.6).
    ''' </summary>
    Public Class GiudizioMostrato

        ''' <summary>La voce dell'annuncio, o il campo di contesto.</summary>
        Public Property Requisito As String = ""

        ''' <summary>Da quale delle cinque liste viene: competenze, esperienza, … , contesto.</summary>
        Public Property Categoria As String = ""

        ''' <summary>
        ''' Quanto pesa, con le parole dell'annuncio: la <c>priorita</c>, o l'importanza
        ''' stimata dall'AI quando l'annuncio non l'ha dichiarata. È lo stesso ripiego del
        ''' prototipo, che scrive <c>priorita || importanza</c>.
        ''' </summary>
        Public Property Peso As String = ""

        ''' <summary>L'esito, riconosciuto.</summary>
        Public Property Esito As EsitoGiudizio

        ''' <summary>L'esito con le parole del prompt: è ciò che si legge nell'elenco.</summary>
        Public Property NomeEsito As String = ""

        ''' <summary>Se la voce è tassativa: senza, la candidatura non è proponibile.</summary>
        Public Property Eliminatorio As Boolean

        ''' <summary>Perché quell'esito, ancorato al profilo.</summary>
        Public Property Spiegazione As String = ""

        ''' <summary>
        ''' Il segno che precede la voce nell'elenco: ✓ soddisfatto, ~ in parte,
        ''' ✗ non soddisfatto, ? non determinabile (cap. 12, A5).
        ''' </summary>
        Public ReadOnly Property Simbolo As String
            Get
                Select Case Esito
                    Case EsitoGiudizio.Soddisfatto : Return "✓"
                    Case EsitoGiudizio.InParte : Return "~"
                    Case EsitoGiudizio.NonSoddisfatto : Return "✗"
                    Case EsitoGiudizio.NonDeterminabile : Return "?"
                    Case Else : Return "·"
                End Select
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Il confronto come lo legge un pannello: i giudizi tipizzati, la lettura d'insieme,
    ''' le stelle e le note del calcolo. È la <b>vista di sola lettura</b> promessa a T4a —
    ''' gli artefatti restano JSON grezzo (v. <see cref="Opportunita"/>), e a tipizzare è
    ''' solo chi li deve disegnare.
    ''' </summary>
    ''' <remarks>
    ''' <para>Non calcola e non giudica: legge. Il punteggio è già stato fatto da
    ''' <see cref="CalcoloMatch"/> quando l'opportunità è stata confrontata, e qui si
    ''' ripete soltanto — perché quello è il giudizio <i>di quel giorno</i>.</para>
    ''' <para><b>Sopporta il JSON storto.</b> Quello che arriva è la risposta di un
    ''' modello: un campo che manca, un <c>eliminatorio</c> scritto come stringa o un
    ''' esito che il prompt non prevede non devono far crollare il pannello. Ogni lettura
    ''' ha il suo ripiego, e l'esito che non si riconosce si mostra com'è.</para>
    ''' </remarks>
    Public Class VistaConfronto

        ''' <summary>
        ''' Sotto questa soglia la candidatura si <b>sconsiglia</b>, senza impedirla
        ''' (cap. 12, A5.3). È la «soglia B» del prototipo, decisa sui dati di un collaudo
        ''' reale a 0,1 stelle (Step 1.25): i documenti verrebbero comunque onesti, ma
        ''' poco spendibili su quell'annuncio, e a decidere resta l'utente.
        ''' </summary>
        Public Const SogliaSconsiglio As Double = 1.5

        ''' <summary>
        ''' La voce-sentinella che dichiara l'<b>assenza</b> di un requisito. Non entra nel
        ''' conteggio (v. <see cref="CalcoloMatch"/>) e non si mostra: per chi legge
        ''' sarebbe solo rumore, come nel prototipo.
        ''' </summary>
        Private Const Sentinella As String = "nessuna esperienza richiesta"

        Private ReadOnly _giudizi As New List(Of GiudizioMostrato)

        Private Sub New()
        End Sub

        ''' <summary>I giudizi da mostrare, nell'ordine in cui l'AI li ha dati.</summary>
        Public ReadOnly Property Giudizi As IReadOnlyList(Of GiudizioMostrato)
            Get
                Return _giudizi
            End Get
        End Property

        ''' <summary>La sintesi onesta del match, in poche frasi.</summary>
        Public ReadOnly Property LetturaInsieme As String = ""

        ''' <summary>Le stelle 0–5 con un decimale; <c>Nothing</c> se il calcolo non c'è.</summary>
        Public ReadOnly Property Stelle As Double?

        ''' <summary>
        ''' La nota del calcolo — lo scarto tagliato dal clamp, il tetto del requisito
        ''' eliminatorio — o <c>Nothing</c> se non c'è niente da spiegare.
        ''' </summary>
        Public ReadOnly Property Nota As String

        ''' <summary>Se un requisito eliminatorio non soddisfatto ha messo il tetto al punteggio.</summary>
        Public ReadOnly Property GateEliminatorio As Boolean

        ''' <summary>
        ''' Se il match è così basso che candidarsi è sconsigliato. Falso anche quando le
        ''' stelle mancano: senza punteggio non si sconsiglia niente, si tace.
        ''' </summary>
        Public ReadOnly Property Sconsigliata As Boolean
            Get
                Return Stelle.HasValue AndAlso Stelle.Value < SogliaSconsiglio
            End Get
        End Property

        ''' <summary>Le voci tassative rimaste scoperte: sono quelle che hanno craterato il match.</summary>
        Public Function Eliminatori() As IReadOnlyList(Of GiudizioMostrato)

            Return _giudizi.FindAll(
                Function(g) g.Eliminatorio AndAlso g.Esito = EsitoGiudizio.NonSoddisfatto)

        End Function

        ''' <summary>
        ''' La vista di un'opportunità già confrontata; <c>Nothing</c> se il confronto non
        ''' c'è ancora — che non è un guasto, è il flusso (cap. 12, A5→A7).
        ''' </summary>
        Public Shared Function Da(opportunita As Opportunita) As VistaConfronto

            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))
            If opportunita.Confronto Is Nothing Then Return Nothing

            Dim vista As New VistaConfronto With {
                ._LetturaInsieme = If(Testo(TryCast(opportunita.Confronto, JsonObject), "lettura_insieme"), ""),
                ._Stelle = opportunita.Match?.Stelle,
                ._Nota = opportunita.Match?.Nota,
                ._GateEliminatorio = (opportunita.Match IsNot Nothing AndAlso opportunita.Match.GateEliminatorio)}

            Dim voci As JsonArray = opportunita.Giudizi()
            If voci Is Nothing Then Return vista

            For Each voce As JsonNode In voci

                Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                If oggetto Is Nothing Then Continue For

                Dim requisito As String = If(Testo(oggetto, "requisito"), "")
                If requisito.Trim().ToLowerInvariant() = Sentinella Then Continue For

                vista._giudizi.Add(Leggi(oggetto, requisito))

            Next

            Return vista

        End Function

        ''' <summary>Un giudizio dell'AI, letto campo per campo con i suoi ripieghi.</summary>
        Private Shared Function Leggi(oggetto As JsonObject, requisito As String) As GiudizioMostrato

            Dim scritto As String = If(Testo(oggetto, "esito"), "")

            Return New GiudizioMostrato With {
                .Requisito = requisito,
                .Categoria = If(Testo(oggetto, "categoria"), ""),
                .Peso = PesoMostrato(oggetto),
                .Esito = Riconosci(scritto),
                .NomeEsito = scritto,
                .Eliminatorio = EliminatorioVero(oggetto),
                .Spiegazione = If(Testo(oggetto, "spiegazione"), "")}

        End Function

        ''' <summary>
        ''' L'esito con le parole del prompt, riconosciuto senza pignoleria su maiuscole e
        ''' spazi. Quello che non si riconosce resta <see cref="EsitoGiudizio.Sconosciuto"/>:
        ''' l'utente lo vedrà scritto com'è, che è meglio di un simbolo inventato.
        ''' </summary>
        Private Shared Function Riconosci(esito As String) As EsitoGiudizio

            Select Case esito.Trim().ToLowerInvariant()
                Case "soddisfatto" : Return EsitoGiudizio.Soddisfatto
                Case "in parte" : Return EsitoGiudizio.InParte
                Case "non soddisfatto" : Return EsitoGiudizio.NonSoddisfatto
                Case "non determinabile" : Return EsitoGiudizio.NonDeterminabile
                Case Else : Return EsitoGiudizio.Sconosciuto
            End Select

        End Function

        ''' <summary>
        ''' Quanto pesa la voce: la priorità dell'annuncio, e in sua assenza l'importanza
        ''' che l'AI ha stimato. Lo stesso ripiego del prototipo, e per la stessa ragione:
        ''' delle due ne è compilata sempre una sola.
        ''' </summary>
        Private Shared Function PesoMostrato(oggetto As JsonObject) As String

            Dim priorita As String = If(Testo(oggetto, "priorita"), "").Trim()
            If priorita <> "" AndAlso priorita.ToLowerInvariant() <> "non specificata" Then Return priorita

            Dim importanza As String = If(Testo(oggetto, "importanza"), "").Trim()
            If importanza <> "" Then Return importanza

            Return priorita

        End Function

        ''' <summary>
        ''' Il flag eliminatorio come lo intende il prototipo: il booleano vero, oppure la
        ''' stringa «true» scritta in qualunque modo. Stessa lettura di
        ''' <see cref="CalcoloMatch"/> — quel che ha craterato il punteggio e quel che si
        ''' mostra con il ⛔ devono essere la stessa cosa.
        ''' </summary>
        Private Shared Function EliminatorioVero(oggetto As JsonObject) As Boolean

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue("eliminatorio", valore) OrElse valore Is Nothing Then Return False

            If valore.GetValueKind() = JsonValueKind.True Then Return True
            If valore.GetValueKind() <> JsonValueKind.String Then Return False

            Return valore.GetValue(Of String)().Trim().ToLowerInvariant() = "true"

        End Function

        ''' <summary>Un campo di testo, o <c>Nothing</c> se manca o non è un testo.</summary>
        Private Shared Function Testo(oggetto As JsonObject, campo As String) As String

            If oggetto Is Nothing Then Return Nothing

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, valore) OrElse valore Is Nothing Then Return Nothing
            If valore.GetValueKind() <> JsonValueKind.String Then Return Nothing

            Return valore.GetValue(Of String)()

        End Function

    End Class

End Namespace
