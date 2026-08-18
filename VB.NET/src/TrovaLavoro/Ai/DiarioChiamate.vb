Namespace Ai

    ''' <summary>
    ''' Chi tiene il conto di quanto è costata ogni chiamata all'AI e di quanto è andata
    ''' vicina al tetto di token del suo prompt.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché esiste.</b> Il tetto di ogni prompt (<c>max_token</c>, cap. 04) è
    ''' un numero scelto a mano, e quando cambia il modello quel numero non cambia con lui:
    ''' col passaggio a Sonnet 5 la stessa risposta conta circa un terzo di token in più.
    ''' Finché nessuno annotava i consumi, l'unico modo di scoprire che un tetto era
    ''' diventato stretto era vederlo <b>rompersi</b> — cioè una risposta troncata in faccia
    ''' all'utente. Questo diario ribalta la cosa: dopo un giro d'uso vero si legge chi sta
    ''' al 90% e lo si alza prima *(2026-08-18)*.</para>
    ''' <para><b>Non è un registro dell'utente.</b> Non contiene niente di suo — nessun
    ''' testo, nessun dato del profilo, nessuna risposta: solo quale prompt, quanto grande
    ''' era il tetto e quanti token sono passati. Cancellarlo non perde nulla.</para>
    ''' <para><b>Non deve mai far fallire una chiamata.</b> Un diario che non si lascia
    ''' scrivere è un fastidio; una candidatura persa perché non si è potuto annotare il suo
    ''' costo sarebbe assurdo. Chi implementa questa interfaccia si tiene i propri inciampi.</para>
    ''' </remarks>
    Public Interface IDiarioChiamate

        ''' <summary>Annota una chiamata appena conclusa.</summary>
        ''' <param name="idPrompt">Quale prompt del pool: è il nome che si andrà a ritoccare.</param>
        ''' <param name="tetto">Il <c>max_token</c> dichiarato da quel prompt.</param>
        ''' <param name="uscita">La risposta, con dentro i token contati dall'API.</param>
        Sub Annota(idPrompt As String, tetto As Integer, uscita As RispostaAi)

    End Interface

End Namespace
