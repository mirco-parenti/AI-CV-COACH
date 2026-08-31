''' <summary>
''' I nomi che l'utente legge sui bottoni e che i messaggi gli ripetono (cap. 03.4):
''' nel designer e nei pannelli non si scrivono più a mano, si pescano da qui.
''' </summary>
''' <remarks>
''' <para>Un nome che l'utente deve <b>ritrovare</b> non vive solo dove è scritto il
''' bottone: vive in ogni messaggio che ce lo manda. «Confronta ⭐ ANNUNCIO - CV» stava
''' in sei posti — il designer della barra e cinque messaggi fra P3 e P6 — e a tenerli
''' insieme c'era soltanto il banco: il 2026-08-30, quando quel bottone passò da
''' «📋 Candidatura» al nome di oggi, due collaudi diventarono rossi proprio per questo.
''' Accorgersene è meglio che non accorgersene, ma resta un modo di scoprirlo
''' <i>dopo</i>; qui il nome è uno solo, e divergere non è più possibile.</para>
''' <para>Il banco non ha smesso di sorvegliare, e non sorveglia una tautologia: da un
''' capo guarda che il <b>bottone vero</b> della barra porti questo nome
''' (<c>CollaudiBarraDiNavigazione</c>), dall'altro che i messaggi continuino a
''' pescarlo di qui invece di riscriverselo (<c>CollaudiPannelloRicerca</c>). Le due
''' maniere di romperlo sono quelle, e sono coperte entrambe.</para>
''' <para>Ci stanno solo i nomi che compaiono <b>più di una volta</b>. Un'etichetta che
''' vive dove è scritta non guadagna niente a diventare una costante: guadagnerebbe
''' soltanto distanza fra il testo e il posto in cui lo si legge.</para>
''' </remarks>
Public Module NomiUi

    ''' <summary>
    ''' Il bottone della barra che porta al confronto fra annuncio e CV (P4).
    ''' </summary>
    ''' <remarks>
    ''' <para>La stella sta <b>dentro</b> il nome e non davanti: è il punteggio che quel
    ''' pannello produce, non un'icona che lo decora — le altre voci della barra l'icona
    ''' ce l'hanno in testa. Il menu (P0) dice lo stesso mestiere più disteso,
    ''' «Confronta ANNUNCIO - CV / Match 1-5 ⭐», e non passa da qui: lì c'è una riga
    ''' intera a disposizione, e nessun messaggio cita quel testo.</para>
    ''' <para>Dal 2026-08-31 è <b>★</b> (U+2605) e non più ⭐ (U+2B50), e la ragione sta
    ''' in come Windows disegna le due. I bottoni della barra sono disegnati da GDI, che
    ''' le emoji a colori non le sa fare: ogni simbolo finisce al font di ripiego, dove
    ''' la casa e il busto hanno un glifo pieno alto quanto le maiuscole e ⭐ ne ha uno
    ''' minuscolo e sottile — a video sembrava un asterisco stinto in mezzo a icone nere.
    ''' ★ sta invece dentro Segoe UI, quindi prende il <b>grassetto vero</b> del bottone
    ''' ed è piena. Ed è la stessa stella con cui la colonna «Match» scrive il punteggio:
    ''' il bottone promette quel che l'elenco poi mostra, ora anche nel segno.</para>
    ''' </remarks>
    Public Const Confronto As String = "Confronta ★ ANNUNCIO - CV"

End Module
