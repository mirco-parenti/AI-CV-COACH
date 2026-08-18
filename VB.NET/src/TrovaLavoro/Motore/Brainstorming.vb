Imports System.Linq
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai

Namespace Motore

    ''' <summary>
    ''' Il ragionamento su una singola opportunità: tiene la conversazione, la manda
    ''' all'AI un turno alla volta e alla fine ne fa distillare gli appunti di mira
    ''' (cap. 12, A6).
    ''' </summary>
    ''' <remarks>
    ''' <para>Sta qui e non nel pannello per la stessa ragione di
    ''' <see cref="DialogoProfilo"/>: una conversazione ha delle regole, e le regole si
    ''' collaudano senza interfaccia. Il pannello disegna le bolle e riporta indietro
    ''' quello che l'utente scrive.</para>
    ''' <para><b>La memoria è qui.</b> L'AI non ricorda niente da un turno all'altro
    ''' (cap. 02.5): a ogni giro riparte il contesto — profilo, annuncio, giudizi,
    ''' mitigazioni — nel primo messaggio, e sopra ci si appoggiano i turni scambiati.</para>
    ''' <para><b>La conversazione non si conserva</b> (cap. 15.4): vive finché il pannello
    ''' la tiene, e quello che resta su disco sono gli appunti confermati.</para>
    ''' </remarks>
    Public Class Brainstorming

        ''' <summary>Come si presenta ciascuno nella trascrizione mandata a distillare.</summary>
        Private Const ChiParlaUtente As String = "Utente:"
        Private Const ChiParlaAssistente As String = "Assistente:"

        Private ReadOnly _mestiere As IBrainstormatore
        Private ReadOnly _profilo As JsonNode
        Private ReadOnly _battute As New List(Of TurnoChat)

        ''' <param name="mestiere">Chi parla con l'AI.</param>
        ''' <param name="candidatura">L'opportunità di cui si ragiona, già confrontata.</param>
        ''' <param name="profilo">Il profilo da cui la candidatura è nata.</param>
        Public Sub New(mestiere As IBrainstormatore, candidatura As Opportunita, profilo As JsonNode)

            If mestiere Is Nothing Then Throw New ArgumentNullException(NameOf(mestiere))
            If candidatura Is Nothing Then Throw New ArgumentNullException(NameOf(candidatura))

            _mestiere = mestiere
            _profilo = profilo
            _Candidatura = candidatura

        End Sub

        ''' <summary>L'opportunità di cui si sta ragionando.</summary>
        Public ReadOnly Property Candidatura As Opportunita

        ''' <summary>I turni scambiati, dal più vecchio al più recente.</summary>
        Public ReadOnly Property Battute As IReadOnlyList(Of TurnoChat)
            Get
                Return _battute
            End Get
        End Property

        ''' <summary>Se l'AI ha già aperto il ragionamento.</summary>
        Public ReadOnly Property Cominciato As Boolean
            Get
                Return _battute.Count > 0
            End Get
        End Property

        ''' <summary>
        ''' Se c'è qualcosa da distillare: serve che l'utente abbia detto la sua. Un'AI
        ''' che ha solo aperto non ha prodotto nessuna decisione, e distillare il nulla
        ''' costerebbe una chiamata per farsi rispondere una lista vuota.
        ''' </summary>
        Public ReadOnly Property SiPuoDistillare As Boolean
            Get
                Return _battute.Any(Function(b) b.Ruolo = TurnoChat.Utente)
            End Get
        End Property

        ''' <summary>
        ''' Apre il ragionamento: qui parla l'AI per prima, con quello che vede nel
        ''' confronto. Una chat che si apre vuota non dice a nessuno da dove cominciare.
        ''' </summary>
        Public Async Function ApriAsync(pezzo As Action(Of String),
                                        Optional annulla As CancellationToken = Nothing) As Task

            If Cominciato Then Return

            Await UnTurnoAsync(pezzo, annulla).ConfigureAwait(False)

        End Function

        ''' <summary>Manda quello che ha scritto l'utente e riceve la risposta.</summary>
        Public Async Function RispondiAsync(testo As String, pezzo As Action(Of String),
                                            Optional annulla As CancellationToken = Nothing) As Task

            If String.IsNullOrWhiteSpace(testo) Then Return

            AggiungiDettoDallUtente(testo.Trim())

            Await UnTurnoAsync(pezzo, annulla).ConfigureAwait(False)

        End Function

        ''' <summary>
        ''' Chiede all'AI il turno successivo e lo aggiunge alla conversazione.
        ''' </summary>
        ''' <remarks>
        ''' Se la chiamata fallisce, quello che l'utente aveva scritto <b>resta</b>: l'ha
        ''' detto, ed è a video nella sua bolla. Toglierlo dal contesto per far tornare i
        ''' conti lascerebbe una frase che si vede e che il modello non sa (v.
        ''' <see cref="AggiungiDettoDallUtente"/>, che è dove quel conto torna davvero).
        ''' </remarks>
        Private Async Function UnTurnoAsync(pezzo As Action(Of String),
                                            annulla As CancellationToken) As Task

            Dim risposta As String = Await _mestiere.ConversaAsync(
                _profilo, Candidatura.Annuncio, Candidatura.Confronto, Candidatura.Mitigazioni,
                _battute, pezzo, annulla).ConfigureAwait(False)

            If String.IsNullOrWhiteSpace(risposta) Then Return

            _battute.Add(TurnoChat.DallAssistente(risposta.Trim()))

        End Function

        ''' <summary>
        ''' Mette in conversazione quello che ha detto l'utente, <b>unendolo</b> al turno
        ''' precedente se anche quello era suo.
        ''' </summary>
        ''' <remarks>
        ''' L'API vuole che i ruoli si alternino, e due turni dell'utente di fila
        ''' capitano davvero: basta che una risposta sia fallita e lui riscriva. Unire è
        ''' l'unica via che non perde niente — l'alternativa sarebbe buttare via la prima
        ''' frase, che l'utente ha detto e continua a vedere sullo schermo.
        ''' </remarks>
        Private Sub AggiungiDettoDallUtente(testo As String)

            Dim ultima As TurnoChat = _battute.LastOrDefault()

            If ultima IsNot Nothing AndAlso ultima.Ruolo = TurnoChat.Utente Then
                _battute(_battute.Count - 1) = TurnoChat.DallUtente(ultima.Testo & vbLf & testo)
                Return
            End If

            _battute.Add(TurnoChat.DallUtente(testo))

        End Sub

        ''' <summary>
        ''' Distilla dalla conversazione gli appunti di mira, da mostrare all'utente
        ''' perché li confermi o li corregga (cap. 12, A6.3).
        ''' </summary>
        Public Async Function AppuntiAsync(Optional annulla As CancellationToken = Nothing) _
                                           As Task(Of AppuntiDiMira)

            Dim esito As JsonNode = Await _mestiere.AppuntiAsync(Trascrizione(), annulla).
                ConfigureAwait(False)

            Return AppuntiDiMira.DaJson(esito)

        End Function

        ''' <summary>
        ''' La conversazione come la legge il prompt degli appunti: i turni in fila, ognuno
        ''' preceduto da chi l'ha detto. Il messaggio di contesto non c'è, perché non è
        ''' conversazione — è il materiale su cui si conversa.
        ''' </summary>
        Public Function Trascrizione() As String

            Dim testo As New StringBuilder()

            For Each battuta As TurnoChat In _battute
                If testo.Length > 0 Then testo.Append(vbLf).Append(vbLf)
                testo.Append(If(battuta.Ruolo = TurnoChat.Utente, ChiParlaUtente, ChiParlaAssistente))
                testo.Append(" ").Append(battuta.Testo)
            Next

            Return testo.ToString()

        End Function

    End Class

End Namespace
