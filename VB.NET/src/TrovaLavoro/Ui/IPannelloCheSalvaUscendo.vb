''' <summary>
''' Lo implementa il pannello che, quando lo si lascia, ha per le mani del lavoro
''' dell'utente non ancora sul disco.
''' </summary>
''' <remarks>
''' <para><b>Perché esiste.</b> Da un pannello si esce in due modi: dai suoi bottoni
''' («◀ Torna ai documenti»), che il pannello controlla, e dalla <b>barra di navigazione
''' in cima</b>, che non passa da lui. Chi salvava solo sul primo perdeva tutto sul
''' secondo, in silenzio — e la volta dopo ritrovava la versione vecchia credendola
''' l'ultima. Il caso che l'ha fatta nascere è P7, dove ad andarsene erano il
''' destinatario scritto a mano, le spunte degli allegati e un messaggio riscritto che
''' era costato una chiamata all'AI *(2026-08-18)*.</para>
''' <para><b>La implementa solo chi ha qualcosa da salvare.</b> Sta a parte da
''' <see cref="IPannelloArea"/>, che invece riguarda tutti: farne un metodo di quella
''' avrebbe obbligato ogni pannello a dichiarare un corpo vuoto, e un corpo vuoto ripetuto
''' sei volte non dice a nessuno chi davvero ha lavoro in sospeso.</para>
''' <para><b>Non è la conferma d'uscita.</b> Qui non si chiede niente e non si può dire
''' di no: quello che l'utente ha scritto è suo e si salva, punto. Il presidio che
''' <i>domanda</i> prima di buttare via qualcosa vive altrove — nelle guardie dei
''' pannelli e nella chiusura della finestra — e questa interfaccia non lo sostituisce.</para>
''' </remarks>
Public Interface IPannelloCheSalvaUscendo

    ''' <summary>
    ''' Mette al sicuro il lavoro in corso, perché il pannello sta per essere lasciato.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Non deve sollevare.</b> Chi lo chiama sta cambiando pannello per conto
    ''' dell'utente: un'eccezione che salisse da qui bloccherebbe la navigazione per un
    ''' guasto del disco. L'implementazione se ne occupa e lo racconta a modo suo.</para>
    ''' <para><b>Dev'essere innocuo a vuoto.</b> Lo si chiama a ogni uscita, anche quando
    ''' l'utente è solo entrato e uscito senza toccare niente: chiamarlo due volte di fila,
    ''' o quando non c'è niente da salvare, non deve cambiare nulla.</para>
    ''' </remarks>
    Sub SalvaUscendo()

End Interface
