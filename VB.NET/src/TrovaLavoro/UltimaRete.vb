''' <summary>
''' I testi dell'<b>ultima rete</b>: quel che il programma dice quando è successo qualcosa
''' che nessuno aveva previsto. Chi le reti le arma è <see cref="Programma"/>; qui ci sono
''' solo le parole.
''' </summary>
''' <remarks>
''' <para>Stanno in un modulo a parte, e pubblico, per una ragione pratica: il modulo di
''' avvio è <c>Friend</c> — è il punto d'ingresso, non una porta — e il banco non lo vede.
''' Un testo che nessuno può leggere da fuori è un testo che nessuno collauda, e questo è
''' l'ultimo che l'utente legge prima di perdere il lavoro di mezz'ora.</para>
''' <para>La finestra che li mostra <b>non</b> si collauda: una finestra di messaggio, in
''' un collaudo, resta lì ad aspettare un clic che non arriva mai.</para>
''' <para><i>(2026-08-27, dalla revisione del giro D: fino a quel giorno la rete era una
''' sola — il ciclo dei messaggi — e il testo viveva dentro chi lo mostrava.)</i></para>
''' </remarks>
Public Module UltimaRete

    ''' <summary>
    ''' Il testo per l'utente: in italiano, senza stack trace, e con la sola cosa che in
    ''' quel momento gli interessa davvero — che il suo profilo su disco è al sicuro.
    ''' </summary>
    Public Function MessaggioImprevisto(eccezione As Exception) As String

        Dim che As String = If(eccezione?.Message, "")
        If String.IsNullOrWhiteSpace(che) Then che = "Un errore che non ha saputo dire di sé."

        Return "È successo qualcosa che il programma non aveva previsto:" & vbLf &
               che & vbLf & vbLf &
               "Il tuo profilo salvato su disco non è stato toccato. " &
               "Se il problema si ripete, chiudi e riapri il programma."

    End Function

    ''' <summary>
    ''' Lo stesso guasto come lo si scrive nel diario di un server: senza finestre e senza
    ''' consolazioni, ma col <b>tipo</b> dell'eccezione — che a chi legge un diario serve
    ''' più di qualunque frase gentile.
    ''' </summary>
    Public Function MessaggioPerIlDiario(eccezione As Exception) As String

        If eccezione Is Nothing Then Return "Un errore che non ha saputo dire di sé."

        Dim che As String = If(eccezione.Message, "")
        If String.IsNullOrWhiteSpace(che) Then che = "(senza descrizione)"

        Return eccezione.GetType().Name & ": " & che

    End Function

End Module
