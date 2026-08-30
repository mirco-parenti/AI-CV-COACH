Imports System.IO
Imports System.Linq
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
        ''' Nato il 2026-08-30, quando «🏠 Home» è diventato «🏠 Le mie candidature»: un
        ''' bottone della barra ha una larghezza scritta a mano e non manda a capo né mette
        ''' i puntini — taglia, e basta. Allungare un nome senza allargarne il bottone non
        ''' rompe niente che si possa vedere da un collaudo di comportamento: la finestra
        ''' funziona, i cammini reggono, e l'unico segno è mezza parola mancante a video.
        ''' E allargarlo senza spostare i cinque che vengono dopo li fa sovrapporre, che è
        ''' lo stesso genere di guasto: silenzioso finché non lo si guarda. Qui si misura.
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

        ' ==================================================================
        ' L'impalcatura
        ' ==================================================================

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

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

    End Class

End Namespace
