Namespace Motore

    ''' <summary>
    ''' Il testo come lo vuole una casella di Windows.
    ''' </summary>
    ''' <remarks>
    ''' <para>Sta in un posto solo perché lo stesso inciampo si presenta a ogni porta da cui
    ''' un testo scritto altrove entra in una <c>TextBox</c> multiriga: il corpo dell'email
    ''' che arriva dall'AI (cap. 07.2) e il testo di un annuncio catturato da una pagina web
    ''' (cap. 06.4). Fuori di qui gli a capo sono <c>\n</c> — è la forma con cui viaggiano
    ''' il JSON, i prompt e le pagine — ma una casella multiriga li mostra <b>solo</b> se
    ''' sono CRLF, e senza questa conversione il testo compare tutto attaccato.</para>
    ''' <para><b>È un difetto che si vede solo dal vivo</b>: il testo è giusto, il file su
    ''' disco è giusto, quel che parte per l'AI è giusto. Sbagliato è soltanto ciò che legge
    ''' l'utente — e l'utente crede a quel che legge. È successo due volte: col corpo
    ''' dell'email («Cordiali saluti,Mirco Parenti», T6) e con l'annuncio catturato, dove
    ''' pagine intere arrivavano in un blocco unico e illeggibile (T9d, 2026-08-22).</para>
    ''' </remarks>
    Public Class TestoDaMostrare

        ''' <summary>
        ''' Gli a capo come li vuole una casella di Windows, da qualunque forma arrivino.
        ''' </summary>
        ''' <remarks>
        ''' Si passa da <c>\n</c> prima di andare a CRLF: un testo che i CRLF li avesse già
        ''' diventerebbe altrimenti pieno di righe vuote, e un testo misto — che capita, se
        ''' viene da più pezzi cuciti insieme — resterebbe sbagliato a metà.
        ''' </remarks>
        Public Shared Function ConGliACapoDiWindows(testo As String) As String
            Return If(testo, "").Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Replace(vbLf, vbCrLf)
        End Function

    End Class

End Namespace
