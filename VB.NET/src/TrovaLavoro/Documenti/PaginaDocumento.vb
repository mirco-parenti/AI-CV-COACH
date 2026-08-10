Namespace Documenti

    ''' <summary>Che cosa è un blocco della pagina: il suo ruolo, non il suo aspetto.</summary>
    ''' <remarks>
    ''' I nomi dicono il <b>mestiere</b> del blocco («questo è un titolo di sezione»), mai
    ''' la forma («questo è grassetto 14»): la forma la decide la stampante, ed è l'unico
    ''' modo perché DOCX e PDF possano avere la stessa impaginazione senza essere lo
    ''' stesso codice.
    ''' </remarks>
    Public Enum GenereBlocco

        ''' <summary>Il nome della persona, in testa al documento.</summary>
        Nome

        ''' <summary>Una riga di recapiti: i pezzi stanno in <see cref="Blocco.Voci"/>.</summary>
        Recapiti

        ''' <summary>Il titolo di una sezione («Esperienze professionali»).</summary>
        Sezione

        ''' <summary>Un testo corrente.</summary>
        Paragrafo

        ''' <summary>
        ''' Una voce di elenco strutturato — un'esperienza, un titolo di studio: un
        ''' <see cref="Blocco.Testo"/> che la intitola, un <see cref="Blocco.Dettaglio"/>
        ''' che la colloca e una <see cref="Blocco.Descrizione"/> che la racconta.
        ''' </summary>
        Voce

        ''' <summary>Un elenco puntato: le voci stanno in <see cref="Blocco.Voci"/>.</summary>
        Elenco

        ''' <summary>La firma in calce a una lettera.</summary>
        Firma

    End Enum

    ''' <summary>
    ''' Un pezzo di documento. Quali campi siano pieni dipende dal
    ''' <see cref="GenereBlocco"/>: gli altri restano vuoti, e una stampante che legge un
    ''' campo vuoto semplicemente non stampa niente lì.
    ''' </summary>
    Public Class Blocco

        Public Property Genere As GenereBlocco

        ''' <summary>Il contenuto principale: il testo del paragrafo, il titolo della voce.</summary>
        Public Property Testo As String = ""

        ''' <summary>La riga secondaria di una voce: «Rossi S.p.A. · 2019-2023».</summary>
        Public Property Dettaglio As String = ""

        ''' <summary>Il racconto di una voce, sotto le sue due righe di testa.</summary>
        Public Property Descrizione As String = ""

        ''' <summary>I pezzi di un elenco o di una riga di recapiti.</summary>
        Public Property Voci As New List(Of String)

    End Class

    ''' <summary>
    ''' Un documento pronto da stampare: il titolo e i suoi blocchi, in ordine. È il
    ''' <b>modello di impaginazione</b> del cap. 05.3 — «un solo modello di contenuto, più
    ''' stampanti» — e sta in mezzo fra il JSON che l'AI produce e i file che l'utente
    ''' apre.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché non stampare direttamente dal JSON.</b> Le stampanti sarebbero
    ''' due, e ognuna dovrebbe sapere per conto suo che cosa entra nel documento, in che
    ''' ordine e con quali etichette: due letture da tenere allineate a mano, e il primo
    ''' giorno in cui divergono il DOCX e il PDF della stessa candidatura dicono cose
    ''' diverse. Passando di qui il contenuto è identico <b>per costruzione</b>: quello
    ''' che cambia da un formato all'altro è solo come si disegna un blocco.</para>
    ''' <para><b>Perché nessun blocco contiene mai un a capo.</b> Chi impagina spezza i
    ''' testi lunghi in più <see cref="GenereBlocco.Paragrafo"/> e appiattisce gli a capo
    ''' rimasti dentro un campo breve. Così una stampante non deve inventarsi che cosa
    ''' farne — un <c>&lt;br&gt;</c> in HTML, un <c>w:br</c> in OOXML, due decisioni
    ''' separate che divergono — e l'unica domanda che le resta è come si disegna un
    ''' blocco intero.</para>
    ''' <para>È di sola lettura per chi la riceve: la costruisce
    ''' <see cref="Impaginazione"/> e da lì in poi si guarda soltanto. La vista tipizzata
    ''' promessa a T4a per l'impaginazione del CV è questa: una classe intermedia che
    ''' ricalcasse lo schema del CV, per poi tradurlo comunque in blocchi, sarebbe stata
    ''' un terzo modello da tenere allineato agli altri due.</para>
    ''' </remarks>
    Public Class PaginaDocumento

        ''' <summary>
        ''' Come si chiama il documento: il titolo della finestra del PDF e la proprietà
        ''' «Titolo» del DOCX. Non si stampa dentro la pagina.
        ''' </summary>
        Public Property Titolo As String = ""

        ''' <summary>I blocchi, nell'ordine in cui vanno stampati.</summary>
        Public Property Blocchi As New List(Of Blocco)

    End Class

End Namespace
