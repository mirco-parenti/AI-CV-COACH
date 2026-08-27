Imports System.Globalization
Imports System.Text.Json.Nodes

Namespace Ai

    ''' <summary>
    ''' Quanto costa un modello: dollari per milione di token, in entrata e in uscita.
    ''' </summary>
    Public NotInheritable Class PrezzoModello

        Public Sub New(ingresso As Decimal, uscita As Decimal)
            Me.Ingresso = ingresso
            Me.Uscita = uscita
        End Sub

        ''' <summary>Dollari per milione di token mandati.</summary>
        Public ReadOnly Property Ingresso As Decimal

        ''' <summary>Dollari per milione di token ricevuti.</summary>
        Public ReadOnly Property Uscita As Decimal

        ''' <summary>Quanto costano questi token, in dollari.</summary>
        Public Function Costo(tokenIngresso As Long, tokenUscita As Long) As Decimal
            Return (tokenIngresso * Ingresso + tokenUscita * Uscita) / 1000000D
        End Function

    End Class

    ''' <summary>
    ''' Il listino dei modelli: quanto costa un milione di token, modello per modello
    ''' (cap. 02.5, cap. 11.6).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché è nel programma e non nell'API.</b> I prezzi l'API non li dice: si
    ''' leggono su una pagina web e cambiano quando vuole chi li fa. Quindi qui dentro c'è
    ''' quel che si sapeva il giorno in cui si è scritto, e il file <c>modelli.json</c> può
    ''' scavalcarlo — stessa regola dei modelli: cambiare un numero costa una riga, non una
    ''' nuova build.</para>
    ''' <para><b>Un modello senza prezzo non vale zero.</b> Di un modello che il listino non
    ''' conosce i token si contano lo stesso e i soldi no, e chi guarda lo legge scritto:
    ''' un totale che tace su una parte delle chiamate è peggio di nessun totale, perché
    ''' sembra completo. È la stessa scelta dell'anti-invenzione applicata ai numeri —
    ''' meglio un buco dichiarato che una cifra inventata.</para>
    ''' <para><b>È una stima, e si dice.</b> Sono i prezzi di listino: non sanno di sconti,
    ''' di lotti, di crediti promozionali né della cache dei prompt. Servono a dare un
    ''' ordine di grandezza a chi si chiede «quanto mi costa un giro», non a quadrare una
    ''' fattura.</para>
    ''' </remarks>
    Public NotInheritable Class Listino

        Private ReadOnly _prezzi As Dictionary(Of String, PrezzoModello)

        Private Sub New(prezzi As Dictionary(Of String, PrezzoModello))
            _prezzi = prezzi
        End Sub

        ''' <summary>
        ''' I prezzi noti al 2026-08-27, in dollari per milione di token. Ci sono i tre
        ''' modelli che questo programma ha davvero usato: i due di casa e quello del
        ''' prototipo, che resta il termine di paragone dei collaudi (cap. 04.7).
        ''' </summary>
        ''' <remarks>
        ''' <b>Uno di questi numeri ha una data di scadenza.</b> Il cap. 15, voce 6, annota
        ''' che i $2/$10 di Sonnet 5 sono un prezzo promozionale <b>fino al 31/08/2026</b>,
        ''' contro i $3/$15 di listino; il 18 agosto la promozione sembrava diventata il
        ''' prezzo nuovo, ma nessuno l'ha verificato dopo quella data. Se dal primo
        ''' settembre il conto delle Impostazioni sembra basso di un terzo, è questa la
        ''' ragione — e la cura non è una nuova build: basta il blocco <c>prezzi</c> di
        ''' <c>modelli.json</c> (cap. 11.6).
        ''' </remarks>
        Public Shared Function Predefinito() As Listino

            Return New Listino(New Dictionary(Of String, PrezzoModello)(StringComparer.OrdinalIgnoreCase) From {
                {"claude-haiku-4-5", New PrezzoModello(1D, 5D)},
                {"claude-sonnet-5", New PrezzoModello(2D, 10D)},
                {"claude-sonnet-4-6", New PrezzoModello(3D, 15D)}})

        End Function

        ''' <summary>
        ''' Il listino con sopra i prezzi dichiarati nel file, se ce ne sono. La forma è
        ''' <code>
        ''' "prezzi": { "claude-haiku-4-5": { "ingresso": 1.0, "uscita": 5.0 } }
        ''' </code>
        ''' e vale la regola di sempre: quel che il file non dice resta come lo sa il
        ''' programma, quel che dice vince.
        ''' </summary>
        ''' <remarks>
        ''' Il parametro non può chiamarsi «predefinito»: in VB le maiuscole non
        ''' distinguono, e coprirebbe la funzione omonima qui sopra.
        ''' </remarks>
        Public Shared Function Sopra(diPartenza As Listino, radice As JsonObject) As Listino

            Dim prezzi As New Dictionary(Of String, PrezzoModello)(
                If(diPartenza, Predefinito())._prezzi, StringComparer.OrdinalIgnoreCase)

            Dim dichiarati As JsonObject = TryCast(radice?("prezzi"), JsonObject)
            If dichiarati Is Nothing Then Return New Listino(prezzi)

            For Each voce As KeyValuePair(Of String, JsonNode) In dichiarati

                Dim quanto As JsonObject = TryCast(voce.Value, JsonObject)
                If quanto Is Nothing OrElse String.IsNullOrWhiteSpace(voce.Key) Then Continue For

                Dim ingresso As Decimal? = Numero(quanto, "ingresso")
                Dim uscita As Decimal? = Numero(quanto, "uscita")

                ' Mezzo prezzo non è un prezzo: si scarta la voce invece di completarla
                ' con uno zero che darebbe un conto più basso del vero.
                If Not ingresso.HasValue OrElse Not uscita.HasValue Then Continue For

                prezzi(voce.Key.Trim()) = New PrezzoModello(ingresso.Value, uscita.Value)

            Next

            Return New Listino(prezzi)

        End Function

        ''' <summary>Il prezzo di un modello, o <c>Nothing</c> se il listino non lo conosce.</summary>
        Public Function PerModello(id As String) As PrezzoModello

            If String.IsNullOrWhiteSpace(id) Then Return Nothing

            Dim prezzo As PrezzoModello = Nothing
            If _prezzi.TryGetValue(id.Trim(), prezzo) Then Return prezzo

            ' Secondo giro, sull'alias. Nel chiamate_ai.csv finisce il modello che ha
            ' risposto — datato, come lo scrive l'API — mentre il listino conosce l'alias
            ' con cui il programma lo chiede (cap. 15, voce 6). Senza questo giro il
            ' modello predefinito risulterebbe senza prezzo a ogni installazione, e il
            ' buco che il cap. 13.11 riserva ai modelli sconosciuti si aprirebbe sul
            ' modello di casa. L'identificativo esatto viene prima: chi in modelli.json
            ' dichiara il prezzo di una versione precisa vuole quello, non quello del suo
            ' alias.
            For Each voce As KeyValuePair(Of String, PrezzoModello) In _prezzi
                If IdModello.StessoModello(voce.Key, id) Then Return voce.Value
            Next

            Return Nothing

        End Function

        ''' <summary>Quanti modelli il listino conosce.</summary>
        Public ReadOnly Property Quanti As Integer
            Get
                Return _prezzi.Count
            End Get
        End Property

        Private Shared Function Numero(quanto As JsonObject, chiave As String) As Decimal?

            Dim valore As JsonValue = TryCast(quanto(chiave), JsonValue)
            If valore Is Nothing Then Return Nothing

            Dim quanti As Decimal
            If valore.TryGetValue(Of Decimal)(quanti) AndAlso quanti >= 0D Then Return quanti

            ' Un numero scritto come stringa capita a chi compila il file a mano.
            Dim testo As String = valore.ToString()
            If Decimal.TryParse(testo, NumberStyles.Any, CultureInfo.InvariantCulture, quanti) AndAlso
               quanti >= 0D Then Return quanti

            Return Nothing

        End Function

    End Class

End Namespace
