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

        ''' <summary>I bottoni della barra superiore, quelli che la guardia deve spegnere.</summary>
        Private Shared ReadOnly Barra As String() =
            {"btnHome", "btnProfilo", "btnRicerca", "btnCandidatura", "btnImpostazioni"}

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

                    Assert.HasCount(Barra.Length, Accesi(form),
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

                    CollectionAssert.AreEquivalent(Barra, Accesi(form),
                                                   "finito il giro si riapre tutto, Impostazioni compreso")

                End Function)

        End Function

        ' ==================================================================
        ' L'impalcatura
        ' ==================================================================

        ''' <summary>I nomi dei bottoni della barra che in questo momento sono premibili.</summary>
        Private Shared Function Accesi(form As Control) As String()

            Return Barra.Where(Function(nome) Bottone(form, nome).Enabled).ToArray()

        End Function

        Private Shared Function Bottone(form As Control, nome As String) As Button
            Return DirectCast(form.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
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
