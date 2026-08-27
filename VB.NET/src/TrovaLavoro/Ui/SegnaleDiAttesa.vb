''' <summary>
''' Il segno visibile che l'AI sta lavorando: la riga che compare nella barra di stato
''' mentre si aspetta, e che si muove — perché è il muoversi, non il testo, a dire che il
''' programma è vivo.
''' </summary>
''' <remarks>
''' <para><b>Perché serve.</b> Fino al 2026-08-27 l'unico segnale di un'attesa che dura
''' decine di secondi era il <b>puntatore del mouse</b> (<c>Cursors.AppStarting</c>), e in
''' un pannello — quello dell'email — non c'era nemmeno quello. Il puntatore lo si vede
''' solo se il mouse è sopra la finestra e lo si sta guardando: chi aspetta guarda altrove,
''' e dopo venti secondi non sa se il programma stia pensando o sia morto.
''' <i>(Reperto D-R2 del giro D.)</i></para>
''' <para><b>Perché è una classe e non tre righe dentro la finestra.</b> Quel che qui può
''' rompersi è la logica, non il disegno: che il testo si muova davvero, che dopo un po'
''' dica anche <i>da quanto</i> si aspetta, e soprattutto che finita l'attesa la barra
''' torni a dire quel che diceva prima invece di restare con l'ultima animazione addosso.
''' Tutte cose che si collaudano senza aprire una finestra — se il tempo lo si passa da
''' fuori, invece di leggerlo dall'orologio.</para>
''' </remarks>
Public NotInheritable Class SegnaleDiAttesa

    ''' <summary>Ogni quanto si muove il segnale, in millisecondi.</summary>
    Public Const IntervalloInMillisecondi As Integer = 500

    ''' <summary>
    ''' Dopo quanti secondi la riga dice anche da quanto si aspetta. Prima è rumore: le
    ''' attese brevi si vedono passare.
    ''' </summary>
    Public Const SecondiPrimaDelConto As Integer = 10

    Private ReadOnly _testoDiRiposo As String
    Private _iniziata As Date
    Private _passo As Integer

    ''' <param name="testoDiRiposo">Quel che la barra diceva prima, e tornerà a dire dopo.</param>
    Public Sub New(testoDiRiposo As String)
        _testoDiRiposo = If(testoDiRiposo, "")
    End Sub

    ''' <summary>Se un'attesa è in corso adesso.</summary>
    Public ReadOnly Property InCorso As Boolean

    ''' <summary>Comincia l'attesa e restituisce la prima riga da mostrare.</summary>
    Public Function Avvia(quando As Date) As String

        _iniziata = quando
        _passo = 0
        _InCorso = True

        Return Riga(_passo, TimeSpan.Zero)

    End Function

    ''' <summary>Un battito: la riga successiva. Se non c'è nessuna attesa, il riposo.</summary>
    Public Function Battito(adesso As Date) As String

        If Not InCorso Then Return _testoDiRiposo

        _passo += 1
        Return Riga(_passo, adesso - _iniziata)

    End Function

    ''' <summary>
    ''' Finisce l'attesa e restituisce quel che la barra deve tornare a dire. Non è un
    ''' dettaglio: una barra che resta con «sta lavorando…» addosso a lavoro finito mente
    ''' peggio di una barra muta.
    ''' </summary>
    Public Function Ferma() As String
        _InCorso = False
        Return _testoDiRiposo
    End Function

    ''' <summary>La riga a un certo passo, dopo un certo tempo.</summary>
    Public Shared Function Riga(passo As Integer, trascorsi As TimeSpan) As String

        Dim punti As String = New String("."c, 1 + (Math.Abs(passo) Mod 3))

        If trascorsi.TotalSeconds < SecondiPrimaDelConto Then Return "L'AI sta lavorando" & punti

        Return $"L'AI sta lavorando{punti} ({CInt(Math.Floor(trascorsi.TotalSeconds))} s)"

    End Function

End Class
