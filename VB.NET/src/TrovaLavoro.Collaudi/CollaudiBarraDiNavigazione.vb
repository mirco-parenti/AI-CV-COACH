Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi della barra di navigazione della finestra principale (cap. 03.4): mentre
    ''' una chiamata all'AI è in volo, da lì non si va da nessuna parte (cap. 02.6).
    ''' </summary>
    ''' <remarks>
    ''' <para>La finestra si costruisce e non si mostra, come le altre: il suo
    ''' <c>Load</c> — che monta il motore, prende il lucchetto e accende il browser — qui
    ''' non gira. Basta il costruttore, perché quel che si guarda è il filo che va
    ''' dall'evento del pannello ai bottoni della barra, e quel filo è agganciato da
    ''' <c>WithEvents</c> fin dalla costruzione.</para>
    ''' <para>Il pannello, quindi, lo collega il collaudo: è quello vero dentro la
    ''' finestra vera — non una copia — perché è la finestra a dover reagire.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiBarraDiNavigazione

        Private Const CvBase As String =
            "{""tipo"": ""cv_base"", ""intestazione"": {""nome"": ""Luca Ferrari"", ""citta"": ""Modena""}," &
            """sommario"": ""Il ritratto del profilo."", ""competenze"": [""Uso del muletto""]}"

        ''' <summary>
        ''' I bottoni della barra superiore, quelli che la guardia deve spegnere: si
        ''' <b>leggono dalla barra</b>, non si elencano.
        ''' </summary>
        ''' <remarks>
        ''' Fino al 2026-08-30 erano cinque nomi scritti qui a mano, ed e' la terza volta
        ''' che questa storia si ripete nello stesso punto: il codice della finestra
        ''' racconta di un elenco gemello invecchiato in silenzio fra T9b e T9d, e
        ''' l'elenco fu unificato la'. Questo, che e' il collaudo incaricato di
        ''' accorgersene, aveva la sua copia e non conteneva «btnDocumenti», nato a T9d.
        ''' Un collaudo con un elenco proprio non sorveglia la barra: sorveglia la sua
        ''' idea della barra, e resta verde mentre il bottone nuovo resta acceso. Leggendo
        ''' i figli del pannello si guarda la barra vera, e un bottone che nasce entra nel
        ''' collaudo il giorno stesso.
        ''' </remarks>
        Private Shared Function Barra(form As Control) As Button()

            Dim pannello As Control = form.Controls.Find("pnlBarraSuperiore", searchAllChildren:=True).Single()

            Return pannello.Controls.OfType(Of Button)().
                OrderBy(Function(b) b.Name, StringComparer.Ordinal).
                ToArray()

        End Function

        ''' <summary>I nomi dei bottoni della barra.</summary>
        Private Shared Function NomiDellaBarra(form As Control) As String()
            Return Barra(form).Select(Function(b) b.Name).ToArray()
        End Function

        <TestMethod>
        Public Async Function MentreLAiLavoraLaBarraSiSpegneTutta() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConLaFinestraAsync(
                generatore,
                Async Function(form, pannello)

                    Dim accesiDurante As String() = Nothing

                    ' L'handler della finestra è agganciato per primo (WithEvents, alla
                    ' costruzione): quando questo gira, la barra ha già deciso.
                    AddHandler pannello.LavoroAiCambiato,
                        Sub()
                            If pannello.AiAlLavoro Then accesiDurante = Accesi(form)
                        End Sub

                    Assert.HasCount(Barra(form).Length, Accesi(form),
                                    "prima di cominciare la barra è tutta accesa")

                    Await pannello.MostraIlCvBaseAsync()

                    Assert.IsNotNull(accesiDurante, "l'AI è stata chiamata davvero")
                    Assert.IsEmpty(accesiDurante,
                                   "mentre l'AI scrive non resta acceso nessun bottone della barra")

                End Function)

        End Function

        <TestMethod>
        Public Async Function FinitoIlLavoroLaBarraTornaTuttaAccesa() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConLaFinestraAsync(
                generatore,
                Async Function(form, pannello)

                    Await pannello.MostraIlCvBaseAsync()

                    CollectionAssert.AreEquivalent(NomiDellaBarra(form), Accesi(form),
                                                   "finito il giro si riapre tutto, Impostazioni compreso")

                End Function)

        End Function

        ''' <summary>
        ''' Ogni nome sta dentro il suo bottone, e nessun bottone finisce sotto il vicino.
        ''' </summary>
        ''' <remarks>
        ''' <para>Nato il 2026-08-30, quando «🏠 Home» è diventato «🏠 Le mie candidature»: un
        ''' bottone della barra ha una larghezza scritta a mano e non manda a capo né mette
        ''' i puntini — taglia, e basta. Allungare un nome senza allargarne il bottone non
        ''' rompe niente che si possa vedere da un collaudo di comportamento: la finestra
        ''' funziona, i cammini reggono, e l'unico segno è mezza parola mancante a video.
        ''' E allargarlo senza spostare i cinque che vengono dopo li fa sovrapporre, che è
        ''' lo stesso genere di guasto: silenzioso finché non lo si guarda. Qui si misura.</para>
        ''' <para>Dalla sera dello stesso giorno la misura è più stretta senza che questo
        ''' collaudo sia cambiato: la barra è passata tutta al <b>grassetto</b>, che è più
        ''' largo del tondo, e il carattere che si misura è quello che la casella porta
        ''' davvero. Un collaudo gemello che misurava apposta col grassetto è stato tolto
        ''' quando è diventato la stessa domanda fatta due volte.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub OgniNomeDellaBarraStaDentroIlSuoBottone()

            Using form As New FormPrincipale()

                Dim bottoni As Button() = Barra(form).OrderBy(Function(b) b.Left).ToArray()

                For Each bottone As Button In bottoni
                    Assert.IsLessThanOrEqualTo(
                        bottone.Width,
                        TextRenderer.MeasureText(bottone.Text, bottone.Font).Width,
                        $"«{bottone.Text}» non ci sta nel suo bottone")
                Next

                For i As Integer = 1 To bottoni.Length - 1
                    Assert.IsLessThanOrEqualTo(
                        bottoni(i).Left, bottoni(i - 1).Right,
                        $"«{bottoni(i - 1).Text}» finisce sotto «{bottoni(i).Text}»")
                Next

            End Using

        End Sub

        ''' <summary>
        ''' Il bottone del confronto porta il nome che i messaggi promettono all'utente.
        ''' </summary>
        ''' <remarks>
        ''' <para>È un capo dell'anello che <c>NomiUi.Confronto</c> chiude: di qui si
        ''' guarda che il <b>bottone vero</b> porti quel nome, dall'altro capo
        ''' (<c>CollaudiPannelloRicerca</c>) che i messaggi continuino a pescarlo di là
        ''' invece di riscriverselo. Fino al 2026-08-30 il nome stava a mano in sei posti
        ''' e a tenerli insieme c'era soltanto il banco; adesso il posto è uno, e resta un
        ''' solo modo di romperlo — riscrivere un letterale nel designer, che è
        ''' esattamente com'era arrivato fin lì.</para>
        ''' <para>Non è una tautologia: il testo si legge dal controllo <b>costruito</b>,
        ''' non dalla costante. Se il designer tornasse a un letterale e i due
        ''' divergessero, la differenza si vedrebbe qui.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub IlBottoneDelConfrontoPortaIlNomeCheIMessaggiPromettono()

            Using form As New FormPrincipale()

                Dim bottone As Button = Barra(form).Single(Function(b) b.Name = "btnCandidatura")

                Assert.AreEqual(NomiUi.Confronto, bottone.Text,
                                "il bottone della barra e i messaggi devono dire lo stesso nome")

            End Using

        End Sub

        ''' <summary>
        ''' La fascia di stato non c'è finché non ha qualcosa da dire, e quando ce l'ha
        ''' occupa spazio davvero.
        ''' </summary>
        ''' <remarks>
        ''' <para>Fino al 2026-08-30 la fascia stava lì sempre, e a riposo diceva
        ''' «Pronto». Una striscia chiara che non cambia mai si smette di guardarla, e il
        ''' giorno che ci compare «L'AI sta lavorando» non la si vede più: la fascia adesso
        ''' compare e sparisce, così quando c'è vuol dire qualcosa.</para>
        ''' <para><b>Si misura l'altezza della riga, non solo <c>Visible</c>.</b> La fascia
        ''' vive nella terza riga di un <c>TableLayoutPanel</c>, e quella riga ha altezza
        ''' <i>assoluta</i>: nascondere il pannello e fermarsi lì lascia il buco, alto
        ''' uguale e dello stesso colore chiaro — cioè esattamente la striscia che si
        ''' voleva togliere, senza più nemmeno la scritta. È il modo in cui questo lavoro
        ''' poteva riuscire a metà senza che nessuno se ne accorgesse, ed è per questo che
        ''' il collaudo guarda i pixel della riga e non la proprietà del pannello.</para>
        ''' </remarks>
        <TestMethod>
        Public Async Function LaFasciaDiStatoCEsoloQuandoParla() As Task

            Dim generatore As New GeneratoreFinto
            generatore.Dara(CvBase)

            Await ConLaFinestraAsync(
                generatore,
                Async Function(form, pannello)

                    Dim durante As String = Nothing
                    Dim altezzaDurante As Single = -1

                    AddHandler pannello.LavoroAiCambiato,
                        Sub()
                            If pannello.AiAlLavoro Then
                                durante = Etichetta(form, "lblStato").Text
                                altezzaDurante = AltezzaDellaFascia(form)
                            End If
                        End Sub

                    Assert.AreEqual(0.0F, AltezzaDellaFascia(form),
                                    "a riposo la fascia non si prende nemmeno un pixel")
                    Assert.IsEmpty(Etichetta(form, "lblStato").Text,
                                   "e non dice «Pronto», che non è una notizia")

                    Await pannello.MostraIlCvBaseAsync()

                    Assert.IsNotNull(durante, "l'AI è stata chiamata davvero")
                    Assert.Contains("sta lavorando", durante,
                                    "mentre l'AI lavora la fascia lo dice")
                    Assert.IsGreaterThan(0.0F, altezzaDurante,
                                         "e per dirlo si prende lo spazio che le serve")

                    Assert.AreEqual(0.0F, AltezzaDellaFascia(form),
                                    "finito il lavoro se ne va di nuovo")
                    Assert.IsEmpty(Etichetta(form, "lblStato").Text,
                                   "e non resta un «sta lavorando» su un lavoro finito")

                End Function)

        End Function


        ''' <summary>
        ''' La barra porta i colori e il carattere del menu d'ingresso: verde la porta di
        ''' casa, azzurre le sei destinazioni, tutte e sette in grassetto.
        ''' </summary>
        ''' <remarks>
        ''' Nato il 2026-08-30, quando la barra ha smesso di essere una fila di sette
        ''' bottoni bianchi. Il legame con il menu d'ingresso passa dai <b>token</b> — è
        ''' <c>FondoAzione</c> il colore delle voci di P0 (v. <c>BottoneMenu</c>) e
        ''' <c>Successo</c> il verde delle azioni sicure — e qui si guarda che la barra
        ''' peschi di lì e non da un colore suo: sette caselle vestite a mano nel designer
        ''' sono sette occasioni di divergere. Dalla sera dello stesso giorno la fila è
        ''' anche tutta in <b>grassetto</b>, per la ragione per cui lo sono le voci del
        ''' menu: sono nomi che si leggono di sfuggita, mentre si sta facendo altro.
        ''' </remarks>
        <TestMethod>
        Public Sub LaBarraPortaIColoriDelMenuDIngresso()

            Using form As New FormPrincipale()

                Dim caselle As Button() = Barra(form)
                Dim menu As Button = caselle.Single(Function(b) b.Name = "btnMenu")

                Assert.AreEqual(StileApp.Successo, menu.BackColor,
                                "la casella che torna al menu è verde")
                Assert.AreEqual(StileApp.SfondoContenuto, menu.ForeColor,
                                "col testo bianco, che è l'unico leggibile su quel verde")
                Assert.AreEqual(StileApp.BordoSuccesso, menu.FlatAppearance.BorderColor,
                                "e il contorno scuro che la tiene una forma sul bianco della barra")

                For Each casella As Button In caselle.Where(Function(b) b IsNot menu)

                    Assert.AreEqual(StileApp.FondoAzione, casella.BackColor,
                                    $"«{casella.Text}» ha l'azzurro delle voci del menu d'ingresso")
                    Assert.AreEqual(StileApp.TestoPrimario, casella.ForeColor,
                                    $"«{casella.Text}» scrive in scuro su quell'azzurro")

                Next

                For Each casella As Button In caselle

                    Assert.IsTrue(casella.Font.Bold,
                                  $"«{casella.Text}» è in grassetto, come le voci del menu d'ingresso")

                Next

            End Using

        End Sub

        ''' <summary>
        ''' Quale pannello è aperto si vede dalla sua casella, e da una sola.
        ''' </summary>
        ''' <remarks>
        ''' <para>È la metà che il colore da solo non dà. Finché la barra era bianca, il
        ''' pannello aperto si riconosceva dal fondo lilla; adesso che il riposo è azzurro
        ''' quel lilla non si distinguerebbe più, e l'evidenza è passata alla cornice —
        ''' doppia e d'accento, con le lettere dello stesso blu. Se qualcuno togliesse la
        ''' vestizione da <c>MostraPannello</c>, la barra resterebbe bella e muta: sette
        ''' caselle uguali, e nessun modo di sapere dove si è.</para>
        ''' <para>Si apre il pannello dalla strada della finestra e non rivestendo la
        ''' casella a mano: quel che si sorveglia è che <c>MostraPannello</c> continui a
        ''' rifare la veste della barra, non che la veste sappia farsi.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub IlPannelloApertoSiVedeDallaSuaCasella()

            Using form As New FormPrincipale()

                Dim profilo As Button = Barra(form).Single(Function(b) b.Name = "btnProfilo")

                ApriIlPannello(form, "pnlProfilo", profilo)

                Assert.AreEqual(StileApp.Accento, profilo.FlatAppearance.BorderColor,
                                "la casella aperta prende la cornice d'accento")
                Assert.AreEqual(2, profilo.FlatAppearance.BorderSize,
                                "e la prende doppia")
                Assert.AreEqual(StileApp.Accento, profilo.ForeColor,
                                "con le lettere dello stesso blu, che è il segnale che si legge per primo")
                Assert.AreEqual(StileApp.FondoAzione, profilo.BackColor,
                                "il fondo però non cambia: è la cornice a dire dove si è")

                For Each altra As Button In Barra(form).Where(Function(b) b IsNot profilo)
                    Assert.AreNotEqual(StileApp.Accento, altra.FlatAppearance.BorderColor,
                                       $"«{altra.Text}» non è il pannello aperto e non deve sembrarlo")
                Next

            End Using

        End Sub

        ' ==================================================================
        ' L'impalcatura
        ' ==================================================================

        ''' <summary>
        ''' Apre un pannello per la stessa strada che percorre la finestra quando si preme
        ''' una casella della barra, ma senza mostrarla.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>Perché non <c>PerformClick</c>.</b> Su una finestra mai mostrata quel
        ''' metodo non fa niente e non lo dice: <c>CanSelect</c> risale la catena dei
        ''' genitori fino al Form, che invisibile è, e il click non parte affatto. Provato
        ''' il 2026-08-30, con un collaudo che restava rosso su un codice giusto — dentro i
        ''' pannelli, che nei collaudi vivono senza finestra attorno, lo stesso
        ''' <c>PerformClick</c> funziona benissimo, ed è per questo che l'inganno regge.</para>
        ''' <para><b>E perché non mostrarla.</b> Il <c>Load</c> della finestra principale
        ''' monta il motore, prende il lucchetto della cartella dati e può aprire finestre
        ''' modali (l'informativa, la chiave): un banco che le apre non finisce più. Resta
        ''' la chiamata al metodo che il click chiamerebbe — privato, quindi per riflesso:
        ''' se un giorno cambia nome, questo collaudo lo dice subito invece di restare
        ''' verde su una barra che non si aggiorna più.</para>
        ''' </remarks>
        Private Shared Sub ApriIlPannello(form As FormPrincipale, pannello As String, casella As Button)

            Dim mostra As MethodInfo = GetType(FormPrincipale).GetMethod(
                "MostraPannello", BindingFlags.Instance Or BindingFlags.NonPublic)

            Assert.IsNotNull(mostra, "«MostraPannello» è la porta da cui si apre un pannello")

            mostra.Invoke(form, {form.Controls.Find(pannello, searchAllChildren:=True).Single(), casella})

        End Sub

        ''' <summary>I nomi dei bottoni della barra che in questo momento sono premibili.</summary>
        ''' <remarks>
        ''' Si chiede lo stato ai bottoni <b>della barra</b>, invece di ricercarli per nome
        ''' in tutta la finestra: cercando per nome si inciampa in
        ''' <c>Sequence contains more than one element</c>, perché «btnDocumenti» esiste
        ''' due volte — in barra da T9d, e da prima ancora dentro P7, dove è il bottone che
        ''' torna ai documenti. Sono due controlli diversi in due contenitori diversi, e
        ''' per Windows Forms va benissimo; è il <i>cercare per nome</i> a non poterli
        ''' distinguere. Il difetto non si vedeva perché l'elenco a mano di questo
        ''' collaudo «btnDocumenti» non lo conteneva: la cecità nascondeva l'ambiguità.
        ''' </remarks>
        Private Shared Function Accesi(form As Control) As String()

            Return Barra(form).Where(Function(b) b.Enabled).Select(Function(b) b.Name).ToArray()

        End Function

        ''' <summary>
        ''' La finestra principale costruita (non mostrata) su una cartella dati
        ''' usa-e-getta, col pannello P6 collegato a un generatore finto: nessuna rete,
        ''' nessuna chiave, nessuna stampante.
        ''' </summary>
        Private Shared Async Function ConLaFinestraAsync(
                generatore As GeneratoreFinto,
                prova As Func(Of FormPrincipale, PannelloDocumenti, Task)) As Task

            Dim radice As String = Path.Combine(
                Path.GetTempPath(), "barra-navigazione-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      form As New FormPrincipale()

                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))

                    Dim pannello As PannelloDocumenti =
                        DirectCast(form.Controls.Find("pnlDocumenti", searchAllChildren:=True).Single(),
                                   PannelloDocumenti)

                    pannello.CreateControl()
                    pannello.Collega(contesto, New ArchivioDocumenti(contesto.Cartella),
                                     generatore:=generatore)

                    Await prova(form, pannello)
                End Using

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Function

        ''' <summary>Quanto è alta, adesso, la riga che ospita la fascia di stato.</summary>
        ''' <remarks>
        ''' Si guarda la riga e non <c>pnlFasciaInferiore.Visible</c>: quella proprietà, su
        ''' una finestra mai mostrata, risponde False per tutti e non distinguerebbe una
        ''' fascia sparita da una fascia che c'è. L'altezza della riga invece è vera in
        ''' entrambi i casi, ed è anche il pixel che l'utente vede o non vede.
        ''' </remarks>
        Private Shared Function AltezzaDellaFascia(form As Control) As Single

            Dim tabella As TableLayoutPanel =
                DirectCast(form.Controls.Find("tlpStruttura", searchAllChildren:=True).Single(),
                           TableLayoutPanel)

            Return tabella.RowStyles(tabella.RowCount - 1).Height

        End Function

        ''' <summary>Un'etichetta della finestra, per nome.</summary>
        Private Shared Function Etichetta(form As Control, nome As String) As Label
            Return DirectCast(form.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

    End Class

End Namespace
