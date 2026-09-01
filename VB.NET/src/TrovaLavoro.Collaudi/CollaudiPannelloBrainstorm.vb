Imports System.IO
Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi del pannello P5 quando fa l'<b>altro</b> mestiere: il ragionamento su una
    ''' candidatura (T7c, cap. 12 A6). Girano senza rete, con
    ''' <see cref="BrainstormatoreFinto"/> al posto dell'AI.
    ''' </summary>
    ''' <remarks>
    ''' Le regole della conversazione sono collaudate altrove
    ''' (<c>CollaudiBrainstorming</c>): qui si verifica soltanto quello che il pannello
    ''' aggiunge — che la risposta compaia in <b>una</b> bolla che cresce invece che in
    ''' tante, che i comandi cambino nome e mestiere, e che gli appunti confermati
    ''' finiscano davvero su disco.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiPannelloBrainstorm

        Private Const Appunti As String =
            "{""appunti"":[{""tipo"":""enfasi"",""testo"":""Metti davanti il magazzino"",""da"":""la chat""}]," &
            """fatti_nuovi"":[]}"

        <TestMethod>
        Public Async Function AllAperturaMostraLaCandidaturaEParlaLAi() As Task

            Dim finto As New BrainstormatoreFinto()
            finto.Dira("Su questo annuncio reggi bene sul magazzino. Da dove partiamo?")

            Await ConRagionamentoApertoAsync(finto,
                Function(pannello, contesto)
                    ' Il cartello dice di quale candidatura si sta parlando: due
                    ' ragionamenti su due annunci diversi si somiglierebbero troppo.
                    Assert.Contains("Magazziniere", Conversazione(pannello), "il cartello della candidatura")
                    Assert.Contains("Da dove partiamo?", Conversazione(pannello), "e l'apertura dell'AI")

                    Return Task.CompletedTask
                End Function)

        End Function

        <TestMethod>
        Public Async Function LaRispostaCresceInUnaBollaSola() As Task
            ' Il finto consegna parola per parola, come il flusso vero. Se il pannello
            ' aprisse una bolla per pezzo, qui ne troveremmo una dozzina.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("una risposta lunga fatta di parecchie parole staccate")

            Await ConRagionamentoApertoAsync(finto,
                Function(pannello, contesto)
                    Dim dellAssistente As List(Of String) = Bolle(pannello, "bollaAssistente")

                    Assert.HasCount(1, dellAssistente, "una bolla sola, cresciuta")
                    Assert.AreEqual("una risposta lunga fatta di parecchie parole staccate",
                                    dellAssistente(0), "col testo intero dentro")

                    Return Task.CompletedTask
                End Function)

        End Function

        ''' <summary>
        ''' Nella bolla non si vede la scrittura di una macchina (T9d): il finto consegna
        ''' <b>parola per parola</b>, quindi i segni arrivano spezzati come dal vero, ed è
        ''' il testo intero a doversi spianare.
        ''' </summary>
        <TestMethod>
        Public Async Function NellaBollaNonSiVedonoGliAsterischi() As Task

            Dim finto As New BrainstormatoreFinto()
            finto.Dira("Il **CV** conta, e la *lettera* pure")

            Await ConRagionamentoApertoAsync(finto,
                Function(pannello, contesto)
                    Dim dellAssistente As List(Of String) = Bolle(pannello, "bollaAssistente")

                    Assert.AreEqual("Il CV conta, e la lettera pure", dellAssistente(0),
                                    "il testo c'è tutto, i segni no")

                    Return Task.CompletedTask
                End Function)

        End Function

        <TestMethod>
        Public Async Function IComandiCambianoMestiereConIlPannello() As Task

            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io")

            Await ConRagionamentoApertoAsync(finto,
                Function(pannello, contesto)
                    Assert.AreEqual("◀ Torna alla candidatura", Bottone(pannello, "btnTornaAlProfilo").Text,
                                    "l'uscita porta dove si è venuti, e la freccia lo dice come negli altri pannelli")
                    Assert.AreEqual("Trasforma in appunti", Bottone(pannello, "btnPortaNelProfilo").Text,
                                    "e la conclusione è un'altra")

                    Return Task.CompletedTask
                End Function)

        End Function

        <TestMethod>
        Public Async Function INomiNuoviCiStannoDentroILoroBottoni() As Task
            ' Difetto vero del collaudo di T7c: a video si leggeva «Torna alla», perché il
            ' bottone era largo quanto serviva a «Torna al profilo». Un comando tagliato a
            ' metà non dice più dove porta. La fotografia l'ha trovato, questo lo tiene
            ' fermo: il banco vede il testo, e qui misura anche quanto ce ne entra.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io")

            Await ConRagionamentoApertoAsync(finto,
                Function(pannello, contesto)

                    ' La variabile non può chiamarsi «bottone»: in VB coprirebbe la
                    ' funzione «Bottone» qui sotto, e la chiamata verrebbe letta come
                    ' un'indicizzazione. È la trappola di casa, pagata già tre volte.
                    For Each nome As String In {"btnTornaAlProfilo", "btnPortaNelProfilo"}
                        Dim comando As Button = Bottone(pannello, nome)
                        Assert.IsGreaterThanOrEqualTo(comando.PreferredSize.Width, comando.Width,
                                                      $"«{comando.Text}» non ci sta nel suo bottone")
                    Next

                    Return Task.CompletedTask
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaUnaBattutaDellUtenteNonSiTrasformaInAppunti() As Task
            ' Distillare il nulla costerebbe un'attesa per farsi rispondere una lista vuota.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io").Dira("ti rispondo")

            Await ConRagionamentoApertoAsync(finto,
                Async Function(pannello, contesto)
                    Assert.IsFalse(Bottone(pannello, "btnPortaNelProfilo").Enabled,
                                   "dopo la sola apertura non c'è niente da distillare")

                    Casella(pannello).Text = "secondo me il magazzino conta"
                    Await pannello.InviaAlRagionamentoAsync()

                    Assert.IsTrue(Bottone(pannello, "btnPortaNelProfilo").Enabled,
                                  "detta la sua, adesso sì")
                End Function)

        End Function

        <TestMethod>
        Public Async Function GliAppuntiConfermatiFinisconoNellaCandidatura() As Task

            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io").Dira("ti rispondo")
            finto.Dara(Appunti)

            Await ConRagionamentoApertoAsync(finto,
                Async Function(pannello, contesto)

                    Casella(pannello).Text = "il magazzino è la cosa che conta"
                    Await pannello.InviaAlRagionamentoAsync()

                    Dim proposti As AppuntiDiMira = Await pannello.DistillaGliAppuntiAsync()
                    Assert.IsNotNull(proposti, "l'AI ha proposto qualcosa")
                    Assert.HasCount(1, proposti.Appunti, "un appunto")

                    pannello.ConfermaGliAppunti(proposti)

                    ' Su disco, nella cartella della candidatura: è lì che vivono i suoi
                    ' file (cap. 11.1), e da lì li rileggerà la generazione.
                    Dim candidatura As Opportunita = pannello.CandidaturaInEsame
                    Assert.IsNotNull(candidatura.Appunti, "la candidatura se li porta")

                    Dim riletta As Opportunita = contesto.Opportunita.Carica(candidatura.Cartella)
                    Assert.Contains("magazzino", riletta.Appunti.ToJsonString(),
                                    "e li ritrova chi riapre la cartella")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaConversazioneCheNonProduceNienteLoDice() As Task
            ' Una lista vuota è un esito legittimo del prompt: non è un errore, e non deve
            ' sembrarlo — ma nemmeno restare muta, o l'utente aspetterebbe qualcosa.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io").Dira("ti rispondo")
            finto.Dara("{""appunti"":[],""fatti_nuovi"":[]}")

            Await ConRagionamentoApertoAsync(finto,
                Async Function(pannello, contesto)

                    Casella(pannello).Text = "boh"
                    Await pannello.InviaAlRagionamentoAsync()

                    Dim proposti As AppuntiDiMira = Await pannello.DistillaGliAppuntiAsync()

                    Assert.IsNull(proposti, "niente da confermare")
                    Assert.Contains("niente di operativo", Conversazione(pannello),
                                    "ma l'utente lo legge")
                    Assert.IsNull(pannello.CandidaturaInEsame.Appunti, "e non si salva niente")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnErroreNonButtaViaLaConversazione() As Task

            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io")
            finto.FalliraParlando(New ErroreAi(CausaErroreAi.Rete, "Non riesco a raggiungere l'AI"))

            Await ConRagionamentoApertoAsync(finto,
                Async Function(pannello, contesto)

                    Casella(pannello).Text = "una domanda"
                    Await pannello.InviaAlRagionamentoAsync()

                    Assert.Contains("Non riesco a raggiungere l'AI", Conversazione(pannello),
                                    "l'errore si legge in italiano")
                    Assert.Contains("Riprova pure", Conversazione(pannello), "e si può riprovare")
                    Assert.Contains("una domanda", Conversazione(pannello),
                                    "quello che l'utente aveva scritto resta a video")

                End Function)

        End Function

        <TestMethod>
        Public Async Function InterrompereNonEUnErrore() As Task
            ' Qui interrompere si può (cap. 02.6): non c'è nessuna mossa a metà, solo una
            ' risposta più corta. E non deve travestirsi da guasto.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io")
            finto.FalliraParlando(New OperationCanceledException())

            Await ConRagionamentoApertoAsync(finto,
                Async Function(pannello, contesto)

                    Casella(pannello).Text = "una domanda"
                    Await pannello.InviaAlRagionamentoAsync()

                    Assert.Contains("(interrotto)", Conversazione(pannello), "si dice che è rimasta a metà")
                    Assert.DoesNotContain("Riprova pure", Conversazione(pannello),
                                          "ma non è un errore da riprovare")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaRispostaTroncataLoDiceAVideo() As Task
            ' Il gemello di «(interrotto)»: là a fermare la frase è stato l'utente, qui il
            ' tetto dei token del prompt. Il testo arrivato è buono e resta — ma tacere che
            ' manca il resto lo farebbe sembrare una risposta finita, e chi legge crederebbe
            ' che l'AI non avesse altro da dire.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io")
            finto.DiraTroncando("Il muletto lo puoi girare così, però")

            Await ConRagionamentoApertoAsync(finto,
                Async Function(pannello, contesto)

                    Casella(pannello).Text = "una domanda"
                    Await pannello.InviaAlRagionamentoAsync()

                    Assert.Contains("Il muletto lo puoi girare così, però", Conversazione(pannello),
                                    "quel che è arrivato resta a video")
                    Assert.Contains("limite di lunghezza", Conversazione(pannello),
                                    "e si dice perché si è fermata lì")
                    Assert.DoesNotContain("Riprova pure", Conversazione(pannello),
                                          "ma non è un errore da riprovare")

                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaRispostaInteraNonDiceNiente() As Task
            ' L'altra metà della prova: l'avviso non deve comparire quando non serve, o
            ' diventerebbe rumore che nessuno legge più.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io")
            finto.Dira("Il muletto lo puoi girare così, e basta.")

            Await ConRagionamentoApertoAsync(finto,
                Async Function(pannello, contesto)

                    Casella(pannello).Text = "una domanda"
                    Await pannello.InviaAlRagionamentoAsync()

                    Assert.DoesNotContain("limite di lunghezza", Conversazione(pannello),
                                          "la frase è finita da sé: niente da dichiarare")

                End Function)

        End Function

        <TestMethod>
        Public Async Function TornandoAlProfiloIlPannelloRidiventaQuelloDiPrima() As Task
            ' Un pannello solo per due mestieri: passando dall'uno all'altro non devono
            ' restare in giro le etichette sbagliate.
            Dim finto As New BrainstormatoreFinto()
            finto.Dira("apro io")

            Await ConRagionamentoApertoAsync(finto,
                Async Function(pannello, contesto)

                    Await pannello.ApriIlDialogoAsync(New StrutturatoreFinto)

                    Assert.AreEqual("◀ Torna al profilo", Bottone(pannello, "btnTornaAlProfilo").Text,
                                    "i nomi tornano quelli del dialogo guidato")
                    Assert.AreEqual("Porta nel profilo", Bottone(pannello, "btnPortaNelProfilo").Text,
                                    "tutti e due")

                End Function)

        End Function

        ' ==================================================================
        ' Il banco
        ' ==================================================================

        ''' <summary>
        ''' Un pannello collegato a un motore vero — cartella dati temporanea, nessuna
        ''' chiave — con il ragionamento già aperto sul finto.
        ''' </summary>
        Private Shared Async Function ConRagionamentoApertoAsync(
            finto As BrainstormatoreFinto,
            prova As Func(Of TrovaLavoro.PannelloDialogo, ContestoApp, Task)) As Task

            Dim radice As String = Path.Combine(
                Path.GetTempPath(), "pannello-brainstorm-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      pannello As New TrovaLavoro.PannelloDialogo()

                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))

                    ' Senza handle i controlli non sono «realizzati»: qui il pannello non
                    ' è appeso a nessuna finestra, e va creato a mano.
                    pannello.CreateControl()
                    pannello.Collega(contesto)

                    Await pannello.ApriIlBrainstormingAsync(Candidatura(contesto), finto)

                    Await prova(pannello, contesto)

                End Using

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Function

        ''' <summary>Una candidatura già confrontata, salvata nella cartella di prova.</summary>
        Private Shared Function Candidatura(contesto As ContestoApp) As Opportunita

            ' Attenzione al nome: in VB una variabile locale non può chiamarsi come la
            ' funzione che la contiene — la coprirebbe, e la chiamata verrebbe letta come
            ' un'indicizzazione. È la stessa trappola già pagata in ContestoApp.MontaAi.
            Dim daRagionare As New Opportunita With {
                .Annuncio = JsonNode.Parse("{""titolo"":""Magazziniere"",""azienda"":""Acme""}"),
                .Confronto = JsonNode.Parse("{""giudizi"":[{""voce"":""muletto"",""esito"":""non soddisfatto""}]}"),
                .Mitigazioni = JsonNode.Parse("{""mitigazioni"":[]}")}

            contesto.Opportunita.Salva(daRagionare)

            Return daRagionare

        End Function

        ''' <summary>Una cartella di pool che non esiste: qui i prompt non servono.</summary>
        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

        Private Shared Function Bolle(pannello As Control, Optional tipo As String = Nothing) As List(Of String)

            Dim conversazione As FlowLayoutPanel = DirectCast(
                pannello.Controls.Find("flpConversazione", searchAllChildren:=True).Single(), FlowLayoutPanel)

            Dim testi As New List(Of String)

            For Each contenitore As Control In conversazione.Controls
                Dim cornice As Control = contenitore.Controls(0)
                If tipo IsNot Nothing AndAlso cornice.Name <> tipo Then Continue For

                Dim fondo As Control = cornice.Controls(0)
                testi.Add(String.Join(vbLf, fondo.Controls.OfType(Of Label)().Select(Function(riga) riga.Text)))
            Next

            Return testi

        End Function

        Private Shared Function Conversazione(pannello As Control) As String
            Return String.Join(vbLf, Bolle(pannello))
        End Function

        Private Shared Function Bottone(pannello As Control, nome As String) As Button
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Casella(pannello As Control) As TextBox
            Return DirectCast(pannello.Controls.Find("txtRisposta", searchAllChildren:=True).Single(), TextBox)
        End Function

    End Class

    ''' <summary>
    ''' Collaudi della scheda di conferma degli appunti (cap. 12, A6.3). La finestra non
    ''' si mostra mai: si costruisce e si interroga, come per
    ''' <c>CollaudiFinestraDocumenti</c> — una finestra modale il banco non la sa chiudere.
    ''' </summary>
    <TestClass>
    Public Class CollaudiFinestraAppunti

        Private Shared Function Proposti() As AppuntiDiMira

            Return AppuntiDiMira.DaJson(JsonNode.Parse(
                "{""appunti"":[" &
                "{""tipo"":""enfasi"",""testo"":""Metti davanti il magazzino"",""da"":""la chat""}," &
                "{""tipo"":""tono"",""testo"":""Sobrio"",""da"":""la chat""}]," &
                """fatti_nuovi"":[""ho il patentino del muletto""]}"))

        End Function

        <TestMethod>
        Public Sub DiPartenzaSonoTuttiDaTenere()
            ' Sono la proposta dell'AI su una conversazione appena fatta: togliere è più
            ' veloce che rimettere, e chi è d'accordo conferma e basta.
            Using finestra As New FinestraAppunti(Proposti())
                Assert.HasCount(2, finestra.Scelti().Appunti, "tutti e due")
            End Using

        End Sub

        <TestMethod>
        Public Sub QuelloTolteDallaSpuntaNonPassa()

            Using finestra As New FinestraAppunti(Proposti())

                Assert.IsTrue(finestra.Tieni(0, tenere:=False), "la riga esiste")

                Dim scelti As AppuntiDiMira = finestra.Scelti()

                Assert.HasCount(1, scelti.Appunti, "uno solo")
                Assert.AreEqual(TipiDiAppunto.Tono, scelti.Appunti(0).Tipo, "quello rimasto")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnAppuntoRiscrittoDiceCheLHaScrittoLUtente()
            ' Se il testo è dell'utente, la colonna «da dove nasce» non può continuare a
            ' citare l'AI: sarebbe una citazione sbagliata, e le altre righe non
            ' varrebbero più niente.
            Using finestra As New FinestraAppunti(Proposti())

                Assert.IsTrue(finestra.RiscriviLAppunto(0, "  Metti davanti la logistica  "), "riscritto")

                Dim scelti As AppuntiDiMira = finestra.Scelti()

                Assert.AreEqual("Metti davanti la logistica", scelti.Appunti(0).Testo, "col testo nuovo")
                Assert.Contains("scritto tu", scelti.Appunti(0).Da, "e la provenienza aggiornata")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnAppuntoNonSiPuoSvuotare()
            ' Una riga vuota che resta spuntata arriverebbe al prompt come un'istruzione
            ' senza istruzione: si tiene quella di prima, e a toglierla c'è la spunta.
            Using finestra As New FinestraAppunti(Proposti())

                Assert.IsFalse(finestra.RiscriviLAppunto(0, "   "), "non si accetta")
                Assert.AreEqual("Metti davanti il magazzino", finestra.Scelti().Appunti(0).Testo,
                                "resta quello di prima")

            End Using

        End Sub

        <TestMethod>
        Public Sub IFattiNuoviAccompagnanoSempre()
            ' Non sono una scelta dell'utente: sono il promemoria di quello che ha detto e
            ' che nel profilo non c'è. Togliere tutti gli appunti non li fa sparire.
            Using finestra As New FinestraAppunti(Proposti())

                finestra.Tieni(0, tenere:=False)
                finestra.Tieni(1, tenere:=False)

                Dim scelti As AppuntiDiMira = finestra.Scelti()

                Assert.IsEmpty(scelti.Appunti, "nessun appunto")
                Assert.HasCount(1, scelti.FattiNuovi, "ma il fatto nuovo resta")

            End Using

        End Sub

        <TestMethod>
        Public Sub SenzaConfermaNonTornaNiente()

            Using finestra As New FinestraAppunti(Proposti())
                Assert.AreEqual(EsitoAppunti.Annullato, finestra.Esito,
                                "finché non decide, la risposta è no")
            End Using

        End Sub

        <TestMethod>
        Public Sub QuandoNonCiStaSiScorreInveceDiTagliare()
            ' A 150% i testi crescono e la finestra cresce con loro, ma non oltre lo
            ' spazio che c'è: il tetto e lo scorrimento vanno insieme, o quel che resta
            ' fuori cade fuori dalla finestra e nessuno spostamento lo recupera
            ' (decisione 15.7).
            Using finestra As New FinestraAppunti(Proposti())

                finestra.DisponiIn(200)

                Assert.IsTrue(finestra.AutoScroll, "con questo spazio si scorre")
                Assert.IsLessThanOrEqualTo(200, finestra.ClientSize.Height,
                                           "e la finestra sta nello spazio che c'è")

            End Using

        End Sub

    End Class

End Namespace
