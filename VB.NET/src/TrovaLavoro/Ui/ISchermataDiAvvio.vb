''' <summary>
''' La schermata di avvio vista da chi deve toglierla di mezzo (cap. 03.4).
''' </summary>
''' <remarks>
''' <para>Esiste come interfaccia e non come riferimento diretto alla finestra per una
''' ragione sola, ed è collaudabile: la regola che conta non è «si vede all'avvio» ma
''' <b>quando</b> sparisce — subito, se sta per aprirsi una finestra che chiede
''' qualcosa; con calma, quando l'applicazione è a video. Con l'interfaccia il banco può
''' passare alla finestra principale una schermata finta e verificare l'ordine dei due
''' momenti, che è esattamente il difetto da prevenire: una schermata di avvio davanti
''' alla domanda della chiave API.</para>
''' <para>Chi la riceve non la deve chiudere due volte, ma può farlo senza danno: le
''' due chiamate sono idempotenti.</para>
''' </remarks>
Public Interface ISchermataDiAvvio

    ''' <summary>
    ''' Toglie la schermata adesso, senza aspettare il tempo minimo: la usa chi sta per
    ''' mostrare una finestra che aspetta una risposta.
    ''' </summary>
    Sub ChiudiSubito()

    ''' <summary>
    ''' Toglie la schermata appena il tempo minimo è passato — subito, se è già passato.
    ''' È la chiusura normale, quando la finestra principale è a video.
    ''' </summary>
    Sub ChiudiQuandoPuoi()

End Interface
