Imports System.Globalization
Imports System.IO
Imports TrovaLavoro.Ai

Namespace Dati

    ''' <summary>
    ''' Quanto è costato finora: chiamate, token e una stima in dollari.
    ''' </summary>
    Public NotInheritable Class Conto

        Friend Sub New(chiamate As Integer, tokenIngresso As Long, tokenUscita As Long,
                       spesa As Decimal, senzaPrezzo As Integer, dalGiorno As Date?)
            Me.Chiamate = chiamate
            Me.TokenIngresso = tokenIngresso
            Me.TokenUscita = tokenUscita
            Me.Spesa = spesa
            Me.SenzaPrezzo = senzaPrezzo
            Me.DalGiorno = dalGiorno
        End Sub

        ''' <summary>Un conto senza niente dentro: nessuna chiamata, nessuna spesa.</summary>
        Public Shared ReadOnly Property Vuoto As Conto
            Get
                Return New Conto(0, 0, 0, 0D, 0, Nothing)
            End Get
        End Property

        Public ReadOnly Property Chiamate As Integer
        Public ReadOnly Property TokenIngresso As Long
        Public ReadOnly Property TokenUscita As Long

        ''' <summary>La stima in dollari delle sole chiamate di cui si conosce il prezzo.</summary>
        Public ReadOnly Property Spesa As Decimal

        ''' <summary>Quante chiamate il listino non ha saputo valutare.</summary>
        Public ReadOnly Property SenzaPrezzo As Integer

        ''' <summary>Il giorno della chiamata più vecchia; <c>Nothing</c> se non ce ne sono.</summary>
        Public ReadOnly Property DalGiorno As Date?

        ''' <summary>Se c'è qualcosa da raccontare.</summary>
        Public ReadOnly Property CEQualcosa As Boolean
            Get
                Return Chiamate > 0
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Legge <c>chiamate_ai.csv</c> (cap. 11.1) e ne ricava quanto è costato l'uso dell'AI.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Il file c'era già, il conto no.</b> Ogni chiamata lascia lì la sua riga dal
    ''' 2026-08-18, con modello e token andati e venuti: serviva a ritarare i
    ''' <c>max_token</c> del pool sui numeri veri. Da qui esce l'altra domanda che quegli
    ''' stessi numeri sanno già rispondere — «quanto mi è costato» — e non serve annotare
    ''' niente di nuovo: bastava leggerlo.</para>
    ''' <para><b>Non solleva mai, e un file storto non azzera il conto.</b> Una riga che non
    ''' si capisce si salta e le altre valgono: il file si apre in un foglio di calcolo, e
    ''' chiunque può averci messo dentro una riga a mano. Il conto è una curiosità legittima,
    ''' non un registro contabile — e comunque la verità è la fattura, non questo.</para>
    ''' <para><b>Il tempo si riceve.</b> «Gli ultimi trenta giorni» dipendono da che giorno
    ''' è oggi, e un collaudo non può cambiare la data del computer: chi chiede porta con sé
    ''' il proprio adesso.</para>
    ''' </remarks>
    Public NotInheritable Class ContoDelleChiamate

        ''' <summary>Lo stesso separatore con cui il diario scrive.</summary>
        Private Const Separatore As Char = ";"c

        ''' <summary>Le colonne che servono, nell'ordine in cui il diario le scrive.</summary>
        Private Const ColonnaQuando As Integer = 0
        Private Const ColonnaModello As Integer = 2
        Private Const ColonnaTokenIngresso As Integer = 4
        Private Const ColonnaTokenUscita As Integer = 5

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Il conto di tutte le chiamate annotate nel file, e quello dei giorni recenti.
        ''' </summary>
        ''' <param name="percorso">Il <c>chiamate_ai.csv</c> da leggere.</param>
        ''' <param name="prezzi">Il listino con cui valutare i token.</param>
        ''' <param name="adesso">Che giorno è oggi, per il conto dei giorni recenti.</param>
        ''' <param name="giorniRecenti">Quanti giorni indietro guarda il secondo conto.</param>
        Public Shared Function Leggi(percorso As String, prezzi As Listino, adesso As Date,
                                     Optional giorniRecenti As Integer = 30) As ContoDoppio

            Dim righe As String()

            Try
                If Not File.Exists(percorso) Then Return ContoDoppio.Vuoto
                righe = File.ReadAllLines(percorso, Text.Encoding.UTF8)
            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                Return ContoDoppio.Vuoto
            End Try

            Return DalleRighe(righe, prezzi, adesso, giorniRecenti)

        End Function

        ''' <summary>
        ''' Il conto ricavato da queste righe: è la parte che non tocca il disco, e quella
        ''' che si collauda.
        ''' </summary>
        Public Shared Function DalleRighe(righe As IEnumerable(Of String), prezzi As Listino,
                                          adesso As Date,
                                          Optional giorniRecenti As Integer = 30) As ContoDoppio

            Dim listino As Listino = If(prezzi, Listino.Predefinito())
            Dim daQuando As Date = adesso.Date.AddDays(-giorniRecenti)

            Dim tutte As New Somma()
            Dim recenti As New Somma()

            For Each riga As String In If(righe, Array.Empty(Of String)())

                Dim campi As String() = If(riga, String.Empty).Split(Separatore)
                If campi.Length <= ColonnaTokenUscita Then Continue For

                ' L'intestazione, o una riga che qualcuno ha aggiunto a mano: la si
                ' riconosce dal fatto che i token non sono numeri.
                Dim ingresso, uscita As Long
                If Not Long.TryParse(campi(ColonnaTokenIngresso), NumberStyles.Integer,
                                     CultureInfo.InvariantCulture, ingresso) Then Continue For
                If Not Long.TryParse(campi(ColonnaTokenUscita), NumberStyles.Integer,
                                     CultureInfo.InvariantCulture, uscita) Then Continue For

                Dim modello As String = campi(ColonnaModello)
                Dim quando As Date? = Giorno(campi(ColonnaQuando))

                tutte.Aggiungi(listino.PerModello(modello), ingresso, uscita, quando)

                ' Una riga senza data non può stare nel conto dei giorni recenti: dire di
                ' sì la conterebbe due volte a ogni mese che passa, dire di no la lascia
                ' nel totale, dove è certamente giusta.
                If quando.HasValue AndAlso quando.Value >= daQuando Then
                    recenti.Aggiungi(listino.PerModello(modello), ingresso, uscita, quando)
                End If

            Next

            Return New ContoDoppio(tutte.Chiudi(), recenti.Chiudi(), giorniRecenti)

        End Function

        ''' <summary>La data della prima colonna, se si lascia leggere.</summary>
        Private Shared Function Giorno(quando As String) As Date?

            Dim letta As Date
            If Date.TryParseExact(If(quando, String.Empty).Trim(), "yyyy-MM-dd HH:mm:ss",
                                  CultureInfo.InvariantCulture, DateTimeStyles.None, letta) Then
                Return letta
            End If

            Return Nothing

        End Function

        ''' <summary>Il conto mentre si accumula.</summary>
        Private NotInheritable Class Somma

            Private _chiamate As Integer
            Private _ingresso As Long
            Private _uscita As Long
            Private _spesa As Decimal
            Private _senzaPrezzo As Integer
            Private _dalGiorno As Date?

            Public Sub Aggiungi(prezzo As PrezzoModello, ingresso As Long, uscita As Long, quando As Date?)

                _chiamate += 1
                _ingresso += ingresso
                _uscita += uscita

                If prezzo Is Nothing Then
                    _senzaPrezzo += 1
                Else
                    _spesa += prezzo.Costo(ingresso, uscita)
                End If

                If quando.HasValue AndAlso (Not _dalGiorno.HasValue OrElse quando.Value < _dalGiorno.Value) Then
                    _dalGiorno = quando.Value
                End If

            End Sub

            Public Function Chiudi() As Conto
                Return New Conto(_chiamate, _ingresso, _uscita, _spesa, _senzaPrezzo, _dalGiorno)
            End Function

        End Class

    End Class

    ''' <summary>Il conto di sempre e quello dei giorni recenti, insieme.</summary>
    Public NotInheritable Class ContoDoppio

        Friend Sub New(tutte As Conto, recenti As Conto, giorniRecenti As Integer)
            Me.Tutte = tutte
            Me.Recenti = recenti
            Me.GiorniRecenti = giorniRecenti
        End Sub

        Public Shared ReadOnly Property Vuoto As ContoDoppio
            Get
                Return New ContoDoppio(Conto.Vuoto, Conto.Vuoto, 30)
            End Get
        End Property

        ''' <summary>Tutto quel che c'è nel file.</summary>
        Public ReadOnly Property Tutte As Conto

        ''' <summary>Solo gli ultimi <see cref="GiorniRecenti"/> giorni.</summary>
        Public ReadOnly Property Recenti As Conto

        Public ReadOnly Property GiorniRecenti As Integer

    End Class

End Namespace
