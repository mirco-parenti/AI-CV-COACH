''' <summary>
''' Le due parole con cui una riga di stato dichiara che qualcosa non va: il prefisso di
''' un <b>errore</b> e quello di un <b>avviso</b> (cap. 03.8).
''' </summary>
''' <remarks>
''' <para><b>Perché una parola e non il solo colore.</b> Fino al 2026-09-01 un guasto si
''' distingueva da uno stato per il colore e basta — e in un pannello, P3, nemmeno per
''' quello: là tutto finiva nel grigio delle didascalie, e «Non sono riuscita a salvare le
''' ricerche» aveva lo stesso peso di «Ricerca salvata». Un colore che porta da solo
''' un'informazione è la cosa che WCAG 1.4.1 chiede di non fare, e la ragione non è
''' formale: c'è chi i due rossi non li distingue, chi legge questa riga con un lettore
''' di schermo, e chi guarda lo schermo di traverso al sole. Il colore resta dov'era; la
''' parola gli sta davanti, e le due cose dicono la stessa cosa per due strade.</para>
''' <para><b>Perché due parole e non una.</b> Le righe rosse di questo programma non
''' raccontano tutte lo stesso fatto. Alcune dicono che qualcosa <i>è andato storto</i> —
''' un file che non si scrive, l'AI che risponde male, una pagina che non si legge — e
''' quelle sono errori. Altre dicono che qualcosa <i>manca o non torna</i> prima ancora di
''' provare: la chiave API che non c'è, un backup rimesso a posto solo in parte, una bozza
''' scritta in un'altra lingua. Chiamare «Errore» la mancanza della chiave sarebbe una
''' bugia — il cap. 11.3 dice che non darla è una risposta legittima — e dare la stessa
''' parola a due fatti diversi la svuota. Il colore però è lo stesso: la gravità per
''' l'occhio non cambia, cambia di che cosa si parla.</para>
''' <para><b>Perché una parola e non un'icona.</b> La lezione è già pagata da
''' <see cref="NomiUi.Confronto"/>: i controlli di Windows Forms li disegna GDI, che le
''' emoji a colori non le sa fare, e ogni simbolo fuori dal font finisce a un ripiego dove
''' non si sa che aspetto avrà. Una parola non ha ripieghi, e un lettore di schermo la
''' legge invece di annunciare «segno di attenzione».</para>
''' <para>I due prefissi si scrivono <b>qui e in nessun altro posto</b>: a portarli nelle
''' righe sono i metodi <c>RaccontaUnErrore</c> e <c>RaccontaUnAvviso</c> dei pannelli e
''' delle finestre, che mettono insieme la parola e il colore in un gesto solo — così non
''' esiste il caso di una riga rossa senza parola, che è esattamente il difetto da cui
''' questo file nasce.</para>
''' </remarks>
Public Module Segnalazioni

    ''' <summary>Davanti a quel che non è riuscito.</summary>
    Public Const PrefissoErrore As String = "Errore — "

    ''' <summary>Davanti a quel che manca, o è riuscito solo a metà.</summary>
    Public Const PrefissoAvviso As String = "Attenzione — "

End Module
