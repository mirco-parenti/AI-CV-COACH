Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks

Namespace Ai

    ''' <summary>Un evento del flusso: come si chiama e cosa porta.</summary>
    Public Class EventoSse

        ''' <summary>Il nome dichiarato dalla riga <c>event:</c>; vuoto se non c'era.</summary>
        Public Property Nome As String

        ''' <summary>
        ''' Il contenuto delle righe <c>data:</c>, unite da un a capo quando sono più
        ''' d'una. Per l'API di Claude è sempre un JSON.
        ''' </summary>
        Public Property Dati As String

    End Class

    ''' <summary>
    ''' Spezza in eventi un flusso «server-sent events» (SSE), il formato con cui l'API
    ''' consegna una risposta man mano che la scrive (cap. 02.5, T7c).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Non sa niente di Claude.</b> Qui c'è solo la grammatica del formato:
    ''' righe <c>campo: valore</c>, una riga vuota che chiude l'evento, le righe di
    ''' commento che cominciano con i due punti. Cosa significhi un evento lo decide
    ''' <see cref="ClientClaude"/>. La separazione è voluta: è il pezzo più delicato
    ''' della tappa e così si collauda con delle stringhe, senza rete e senza HTTP.</para>
    ''' <para><b>Il tempo non lo guarda.</b> Un flusso che tace per sempre è un problema
    ''' di chi legge, non della grammatica: l'attesa massima fra un evento e l'altro la
    ''' governa il client col suo gettone (v. <c>SilenzioMassimo</c>).</para>
    ''' <para><b>Un evento a metà non si consegna.</b> Se il flusso finisce senza la riga
    ''' vuota che chiude l'ultimo evento, quell'evento non è mai stato completo: si
    ''' scarta invece di consegnarlo monco, che è anche quello che dice la specifica.</para>
    ''' </remarks>
    Public Class LettoreSse
        Implements IDisposable

        ''' <summary>Il prefisso di una riga di commento: si legge e si butta via.</summary>
        Private Const Commento As Char = ":"c

        Private ReadOnly _righe As StreamReader
        Private _smaltito As Boolean

        ''' <param name="flusso">
        ''' Il flusso da leggere. Resta di chi l'ha aperto: smaltire il lettore non lo
        ''' chiude, perché a possederlo è la risposta HTTP che lo contiene.
        ''' </param>
        Public Sub New(flusso As Stream)

            If flusso Is Nothing Then Throw New ArgumentNullException(NameOf(flusso))

            _righe = New StreamReader(flusso, New UTF8Encoding(False),
                                     detectEncodingFromByteOrderMarks:=False,
                                     bufferSize:=1024, leaveOpen:=True)

        End Sub

        ''' <summary>
        ''' L'evento successivo, oppure <c>Nothing</c> quando il flusso è finito.
        ''' </summary>
        ''' <param name="annulla">
        ''' Il gettone di chi legge: serve sia all'attesa massima fra due eventi sia al
        ''' pulsante con cui l'utente interrompe (cap. 02.6).
        ''' </param>
        Public Async Function ProssimoAsync(Optional annulla As CancellationToken = Nothing) As Task(Of EventoSse)

            Dim nome As String = Nothing
            Dim dati As StringBuilder = Nothing

            While True

                Dim riga As String = Await _righe.ReadLineAsync(annulla).ConfigureAwait(False)

                ' Flusso finito. Se restava un evento aperto non è mai stato chiuso da
                ' una riga vuota: non si consegna.
                If riga Is Nothing Then Return Nothing

                ' La riga vuota chiude l'evento. Un evento senza nessun «data:» non
                ' esiste per la specifica: si riparte da capo invece di consegnare il
                ' vuoto (è il caso di un «event:» solitario, o di due righe vuote di fila).
                If riga.Length = 0 Then
                    If dati IsNot Nothing Then
                        Return New EventoSse With {.Nome = If(nome, String.Empty), .Dati = dati.ToString()}
                    End If
                    nome = Nothing
                    Continue While
                End If

                ' I commenti servono a tenere viva la connessione: si buttano via. Non
                ' vanno confusi con un campo, perché una riga «: qualcosa» ha i due punti
                ' in posizione zero e un campo no.
                If riga(0) = Commento Then Continue While

                Dim duePunti As Integer = riga.IndexOf(Commento)

                Dim campo As String
                Dim valore As String

                If duePunti < 0 Then
                    ' Riga senza due punti: è un campo dal valore vuoto.
                    campo = riga
                    valore = String.Empty
                Else
                    campo = riga.Substring(0, duePunti)
                    valore = riga.Substring(duePunti + 1)
                    ' Un solo spazio dopo i due punti è cortesia del formato, non dato.
                    If valore.StartsWith(" "c) Then valore = valore.Substring(1)
                End If

                Select Case campo
                    Case "event"
                        nome = valore
                    Case "data"
                        ' Più righe «data:» nello stesso evento sono un testo solo,
                        ' rimesso insieme con gli a capo che le separavano.
                        If dati Is Nothing Then
                            dati = New StringBuilder(valore)
                        Else
                            dati.Append(vbLf).Append(valore)
                        End If
                    Case Else
                        ' «id» e «retry» servono a chi riprende una connessione caduta,
                        ' cosa che qui non si fa: una risposta a metà non si ricuce, si
                        ' richiede (cap. 02.5).
                End Select

            End While

            Return Nothing

        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _smaltito Then Return
            _smaltito = True
            _righe.Dispose()
            GC.SuppressFinalize(Me)
        End Sub

    End Class

End Namespace
