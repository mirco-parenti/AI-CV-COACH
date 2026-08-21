Imports System.Text.RegularExpressions

''' <summary>
''' Toglie i segni del Markdown dal testo che l'AI scrive nella chat di P5, prima che
''' finisca in una bolla (cap. 03.5).
''' </summary>
''' <remarks>
''' <para><b>Perché qui e non nel prompt.</b> Il modello risponde con
''' <c>**grassetto**</c> e <c>## titoli</c> perché è come scrive di suo, ed è l'unico
''' posto del programma in cui si vedeva la scrittura di una macchina invece di un testo.
''' Chiederglielo nel prompt sarebbe stato un bump del pool e una fiducia: la riga c'è, e
''' il modello a volte non la segue — la regola 15 nasce anche da lì. Qui invece la cosa
''' non dipende da come si comporta l'AI di oggi né da quella di domani.</para>
''' <para><b>Che cosa non fa.</b> Non rende il grassetto: lo <b>toglie</b>. In una bolla
''' di chat l'enfasi non serviva, e disegnarla vorrebbe dire rifare le bolle, che sono
''' <c>Label</c>. Quel che invece non si perde è l'informazione: di un collegamento resta
''' il testo <i>e</i> l'indirizzo, perché buttarne via uno dei due sarebbe nascondere
''' qualcosa a chi legge.</para>
''' <para><b>Il testo arriva a pezzi</b> (v. <c>CresciLaBolla</c>): la ripulitura si fa
''' sempre sul testo <b>intero</b> accumulato, mai sul frammento, o un <c>**</c> spezzato
''' fra due pezzi non verrebbe mai riconosciuto. Un segno ancora aperto — <c>**parola</c>
''' senza la chiusura — resta a video finché non arriva la sua metà: è giusto così, prima
''' di allora nessuno può sapere se è enfasi o due asterischi.</para>
''' </remarks>
Public Module ProsaDellAssistente

    ''' <summary>La recinzione di un blocco di codice: si toglie la riga intera.</summary>
    Private ReadOnly Recinzione As New Regex("^[ \t]*```.*$", RegexOptions.Multiline)

    ''' <summary>Una riga orizzontale: <c>---</c>, <c>***</c>, <c>___</c>.</summary>
    Private ReadOnly RigaOrizzontale As New Regex("^[ \t]{0,3}([-*_])[ \t]*(\1[ \t]*){2,}$",
                                                  RegexOptions.Multiline)

    ''' <summary>I cancelletti di un titolo, fino a sei.</summary>
    Private ReadOnly Titolo As New Regex("^[ \t]{0,3}#{1,6}[ \t]+", RegexOptions.Multiline)

    ''' <summary>Il segno di citazione a inizio riga.</summary>
    Private ReadOnly Citazione As New Regex("^[ \t]{0,3}>[ \t]?", RegexOptions.Multiline)

    ''' <summary>Il trattino (o l'asterisco) di un elenco puntato.</summary>
    Private ReadOnly Puntato As New Regex("^([ \t]*)[-*+][ \t]+", RegexOptions.Multiline)

    ''' <summary>Un collegamento: se ne tiene il testo <b>e</b> l'indirizzo.</summary>
    Private ReadOnly Collegamento As New Regex("\[([^\]\n]+)\]\(([^)\s]+)\)")

    ''' <summary>Le due coppie del grassetto, che vanno guardate prima del corsivo.</summary>
    Private ReadOnly Grassetto As New Regex("\*\*([^*\n]+)\*\*")
    Private ReadOnly GrassettoBasso As New Regex("__([^_\n]+)__")

    ''' <summary>Il corsivo con gli asterischi.</summary>
    Private ReadOnly Corsivo As New Regex("\*([^*\n]+)\*")

    ''' <summary>
    ''' Il corsivo con i trattini bassi, ma solo quando sono davvero due delimitatori:
    ''' dentro una parola — <c>nome_campo</c> — non sono corsivo, sono il nome.
    ''' </summary>
    Private ReadOnly CorsivoBasso As New Regex("(?<![\w])_([^_\n]+)_(?![\w])")

    ''' <summary>Il codice fra apici inversi.</summary>
    Private ReadOnly CodiceInLinea As New Regex("`([^`\n]+)`")

    ''' <summary>
    ''' Lo stesso testo, senza i segni del Markdown. Un testo vuoto o fatto di soli spazi
    ''' torna com'era: non c'è niente da spianare.
    ''' </summary>
    Public Function SenzaMarkdown(testo As String) As String

        If String.IsNullOrWhiteSpace(testo) Then Return testo

        Dim spianato As String = testo

        ' L'ordine conta due volte. Le recinzioni e le righe orizzontali se ne vanno per
        ' prime, o l'elenco puntato scambierebbe un «---» per una voce; e il grassetto va
        ' guardato prima del corsivo, o di «**parola**» resterebbe «*parola*».
        spianato = Recinzione.Replace(spianato, "")
        spianato = RigaOrizzontale.Replace(spianato, "")

        spianato = Titolo.Replace(spianato, "")
        spianato = Citazione.Replace(spianato, "")
        spianato = Puntato.Replace(spianato, "$1• ")

        spianato = Collegamento.Replace(spianato, "$1 ($2)")

        spianato = Grassetto.Replace(spianato, "$1")
        spianato = GrassettoBasso.Replace(spianato, "$1")
        spianato = Corsivo.Replace(spianato, "$1")
        spianato = CorsivoBasso.Replace(spianato, "$1")

        spianato = CodiceInLinea.Replace(spianato, "$1")

        Return spianato

    End Function

End Module
