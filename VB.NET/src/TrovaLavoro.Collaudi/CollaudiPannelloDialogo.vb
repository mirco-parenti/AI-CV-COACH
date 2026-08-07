Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi del pannello P5 (cap. 03.6; cap. 12, flusso B). Girano <b>senza rete</b>:
    ''' al posto dell'AI c'è <see cref="StrutturatoreFinto"/>, che il pannello accetta
    ''' apposta perché il dialogo si possa condurre fino in fondo da qui.
    ''' </summary>
    ''' <remarks>
    ''' <para>Le regole del dialogo sono già collaudate altrove
    ''' (<c>CollaudiDialogoProfilo</c>): qui si verifica soltanto quello che il pannello
    ''' aggiunge — che ogni pezzo di una mossa finisca a video, che la zona in basso
    ''' chieda la cosa giusta al momento giusto, e che il profilo raccolto non possa
    ''' sfuggire alla conferma dell'utente.</para>
    ''' <para>I clic si simulano chiamando i metodi che i gestori chiamerebbero: di un
    ''' <c>Async Sub</c> non si può aspettare la fine, e un collaudo che non aspetta
    ''' verificherebbe lo schermo di un istante prima.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiPannelloDialogo

        ' ------------------------------------------------------------------
        ' I frammenti che l'AI restituirebbe
        ' ------------------------------------------------------------------

        Private Const FrNome As String = "{""nome"": ""Luca Ferrari""}"

        Private Shared Function FrContatti(Optional altrove As String = "{}") As String
            Return "{""contatti"": {""email"": ""luca@example.it"", ""telefono"": ""333 0000000""," &
                   " ""citta"": ""Forlì"", ""link"": """"}, ""altrove"": " & altrove & "}"
        End Function

        Private Const FrPatente As String =
            "{""patente"": {""ha"": ""sì"", ""categorie"": [""B""]}, ""altrove"": {}}"

        Private Const FrLavoro As String =
            "{""esperienze_formali"": [{""ruolo"": ""Magazziniere"", ""azienda"": ""Romagna Logistica""," &
            " ""durata"": ""3 anni"", ""cosa_facevo"": ""Carico e scarico"", ""tipo"": """"}], ""altrove"": {}}"

        Private Const FrInformale As String =
            "{""esperienze_informali"": [{""cosa_facevo"": ""Traslochi con un amico"", ""quando"": """"," &
            " ""con_chi"": """"}], ""altrove"": {}}"

        Private Const FrCompetenze As String = "{""competenze"": [""Uso del muletto""], ""altrove"": {}}"

        Private Const FrFormazione As String =
            "{""formazione"": [{""titolo"": ""Diploma"", ""istituto"": ""ITIS"", ""anno"": ""2018""}]," &
            " ""altrove"": {}}"

        ' ==================================================================
        ' L'apertura e il giro di un turno
        ' ==================================================================

        <TestMethod>
        Public Async Function AllAperturaChiedeIlNomeEAspettaCheSiScriva() As Task

            Await ConDialogoApertoAsync(
                New StrutturatoreFinto,
                Async Function(pannello)
                    Assert.HasCount(1, Bolle(pannello), "una sola bolla: l'apertura")
                    Assert.Contains("come ti chiami?", Conversazione(pannello), "la prima domanda del prototipo")

                    Assert.IsTrue(Casella(pannello).Visible, "la casella c'è")
                    Assert.IsTrue(Bottone(pannello, "btnInvia").Visible, "e il bottone per mandarla")
                    Assert.IsFalse(Bottone(pannello, "btnScelta1").Visible, "nessuna scelta da fare")

                    Await Task.CompletedTask
                End Function)

        End Function

        <TestMethod>
        Public Async Function QuelloCheScrivoCompareNellaConversazione() As Task

            Await ConDialogoApertoAsync(
                New StrutturatoreFinto().Dara(FrNome),
                Async Function(pannello)
                    Await RispondiAsync(pannello, "Mi chiamo Luca Ferrari")

                    Assert.HasCount(1, Bolle(pannello, "bollaUtente"), "una bolla mia")
                    Assert.AreEqual("Mi chiamo Luca Ferrari", Bolle(pannello, "bollaUtente")(0),
                                    "con le parole che ho scritto")
                    Assert.IsEmpty(Casella(pannello).Text, "e la casella si è svuotata")
                End Function)

        End Function

        <TestMethod>
        Public Async Function DopoLaRispostaChiedeConfermaEMetteViaLaCasella() As Task

            Await ConDialogoApertoAsync(
                New StrutturatoreFinto().Dara(FrNome),
                Async Function(pannello)
                    Await RispondiAsync(pannello, "Mi chiamo Luca Ferrari")

                    Assert.Contains("Ho capito che ti chiami: Luca Ferrari.", Conversazione(pannello),
                                    "dice cosa ha capito")

                    Assert.IsFalse(Casella(pannello).Visible,
                                   "la casella sparisce: adesso non si scrive, si sceglie")
                    Assert.AreEqual("Sì, è giusto", Bottone(pannello, "btnScelta1").Text, "la conferma")
                    Assert.AreEqual("Correggi", Bottone(pannello, "btnScelta2").Text, "e la correzione")
                    Assert.IsFalse(Bottone(pannello, "btnScelta3").Visible, "non ce n'è una terza")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IBottoniPortanoScrittoQuantoPesano() As Task

            ' Cap. 03.3: il colore dice la conseguenza prima che si legga l'etichetta.
            Await ConDialogoApertoAsync(
                New StrutturatoreFinto().Dara(FrNome),
                Async Function(pannello)
                    Await RispondiAsync(pannello, "Mi chiamo Luca Ferrari")

                    Assert.AreEqual(LivelloBottone.AzionePrincipale, Livello(pannello, "btnScelta1"),
                                    "confermare è l'avanti del flusso")
                    Assert.AreEqual(LivelloBottone.Esplorativo, Livello(pannello, "btnScelta2"),
                                    "correggere apre una strada")
                End Function)

        End Function

        <TestMethod>
        Public Async Function LaSchedaDiConfermaMostraICampiDellaVoce() As Task

            ' È il punto in cui l'utente vede cosa sta per entrare nel profilo: i campi
            ' devono esserci tutti, anche quelli che non ha specificato.
            Await ConDialogoApertoAsync(
                New StrutturatoreFinto().Dara(FrNome).Dara(FrContatti()).Dara(FrPatente).Dara(FrLavoro),
                Async Function(pannello)
                    Await FinoAlleEsperienzeAsync(pannello)
                    Await RispondiAsync(pannello, "Ho fatto il magazziniere")

                    Dim scheda As String = Bolle(pannello, "scheda").Last()
                    Assert.Contains("Ruolo: Magazziniere", scheda, "il ruolo")
                    Assert.Contains("Azienda: Romagna Logistica", scheda, "l'azienda")
                    Assert.Contains("Durata: 3 anni", scheda, "la durata")
                    Assert.Contains("Cosa facevo: Carico e scarico", scheda, "cosa faceva")
                End Function)

        End Function

        ' ==================================================================
        ' Anti-perdita
        ' ==================================================================

        <TestMethod>
        Public Async Function IlPezzoTenutoDaParteTornaInBoccaAMe() As Task

            ' La convenzione anti-perdita a video: quello che ho detto nel turno
            ' sbagliato lo rivedo con le mie parole, al turno giusto, e decido io.
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).
                  Dara(FrContatti("{""esperienze_formali"": [""Ho fatto il magazziniere per tre anni""]}")).
                  Dara(FrPatente).Dara(FrLavoro)

            Await ConDialogoApertoAsync(
                finto,
                Async Function(pannello)
                    Await FinoAlleEsperienzeAsync(pannello)

                    Assert.Contains("l'avevo tenuto da parte", Conversazione(pannello), "lo annuncia")
                    Assert.Contains("Ho fatto il magazziniere per tre anni", Bolle(pannello, "bollaUtente"),
                                    "e rimette in bolla le parole mie, non le sue")

                    Assert.AreEqual("Sì, aggiungilo", Bottone(pannello, "btnScelta1").Text, "si può aggiungere")
                    Assert.AreEqual("Scartalo", Bottone(pannello, "btnScelta3").Text, "oppure lasciar perdere")
                    Assert.AreEqual(LivelloBottone.Distruttivo, Livello(pannello, "btnScelta3"),
                                    "e scartare quello che ho detto pesa come un'eliminazione")
                End Function)

        End Function

        ' ==================================================================
        ' La fine, e il profilo che passa alla scheda
        ' ==================================================================

        <TestMethod>
        Public Async Function AMetaDialogoNonCEAncoraNienteDaPortareNelProfilo() As Task

            Await ConDialogoApertoAsync(
                New StrutturatoreFinto().Dara(FrNome),
                Async Function(pannello)
                    Await RispondiAsync(pannello, "Mi chiamo Luca Ferrari")

                    Assert.IsFalse(Bottone(pannello, "btnPortaNelProfilo").Enabled,
                                   "il profilo si consegna quando è finito, non prima")
                    Assert.IsTrue(pannello.HaUnDialogoInCorso, "e intanto c'è del lavoro in ballo")
                End Function)

        End Function

        <TestMethod>
        Public Async Function AllaFineIlProfiloERaccoltoESiPuoPortareNellaScheda() As Task

            Await ConDialogoApertoAsync(
                TuttiISetteTurni(),
                Async Function(pannello)
                    Await FinoInFondoAsync(pannello)

                    Assert.IsFalse(Casella(pannello).Visible, "non c'è più niente da scrivere")
                    Assert.IsFalse(Bottone(pannello, "btnScelta1").Visible, "né da scegliere")
                    Assert.IsTrue(Bottone(pannello, "btnPortaNelProfilo").Enabled, "adesso sì che si consegna")
                    Assert.IsFalse(pannello.HaUnDialogoInCorso, "il dialogo è arrivato in fondo")

                    Dim raccolto As Profilo = pannello.ProfiloCostruito
                    Assert.AreEqual("Luca Ferrari", raccolto.Nome, "il nome")
                    Assert.AreEqual("luca@example.it", raccolto.Contatti.Email, "il recapito")
                    Assert.HasCount(1, raccolto.EsperienzeFormali, "l'esperienza di lavoro")
                    Assert.HasCount(1, raccolto.EsperienzeInformali, "quella informale")
                    Assert.HasCount(1, raccolto.Competenze, "la competenza")
                    Assert.HasCount(1, raccolto.Formazione, "e il titolo di studio")

                    Assert.Contains("niente di inventato", Conversazione(pannello),
                                    "la chiusura del prototipo, parola per parola")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlProfiloConsegnatoEUnaCopia() As Task

            ' Consegnato alla scheda, il profilo è suo: quello che l'utente corregge lì
            ' non deve rientrare nella conversazione, che è ancora viva.
            Await ConDialogoApertoAsync(
                TuttiISetteTurni(),
                Async Function(pannello)
                    Await FinoInFondoAsync(pannello)

                    Dim consegnato As Profilo = pannello.ProfiloCostruito
                    consegnato.Nome = "Corretto nella scheda"

                    Assert.AreEqual("Luca Ferrari", pannello.ProfiloCostruito.Nome,
                                    "la conversazione ha ancora il suo")
                End Function)

        End Function

        ' ==================================================================
        ' Uscire, rientrare, ricominciare
        ' ==================================================================

        <TestMethod>
        Public Async Function RientrareRiprendeLaConversazioneDaDoveEra() As Task

            ' Cap. 12.7, mai un vicolo cieco: si va a vedere la scheda e si torna senza
            ' perdere quello che si è già raccontato.
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome)

            Await ConDialogoApertoAsync(
                finto,
                Async Function(pannello)
                    Await RispondiAsync(pannello, "Mi chiamo Luca Ferrari")
                    Dim quante As Integer = Bolle(pannello).Count

                    ' Come se si fosse usciti e rientrati dal bottone della scheda.
                    Await pannello.ApriIlDialogoAsync(finto)

                    Assert.HasCount(quante, Bolle(pannello), "la conversazione è ancora tutta lì")
                    Assert.HasCount(1, finto.Chiamate, "e non si è ricominciato niente")
                End Function)

        End Function

        <TestMethod>
        Public Async Function RicominciareButtaViaLaConversazioneEReapre() As Task

            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome)

            Await ConDialogoApertoAsync(
                finto,
                Async Function(pannello)
                    Await RispondiAsync(pannello, "Mi chiamo Luca Ferrari")

                    Await pannello.RicominciaAsync()

                    Assert.HasCount(1, Bolle(pannello), "si riparte dalla sola apertura")
                    Assert.Contains("come ti chiami?", Conversazione(pannello), "e dalla prima domanda")
                    Assert.IsEmpty(pannello.ProfiloCostruito.Nome, "il nome di prima non c'è più")
                End Function)

        End Function

        ' ==================================================================
        ' Quando l'AI non c'è, e lo spazio del logo
        ' ==================================================================

        <TestMethod>
        Public Async Function SenzaChiaveIlDialogoNonSiApreELoDice() As Task

            ' Ogni turno passa dall'AI: senza chiave non si comincia nemmeno, e la
            ' ragione va detta dov'è successo (cap. 03.8).
            Dim radice As String = Path.Combine(Path.GetTempPath(), "pannello-dialogo-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      pannello As New TrovaLavoro.PannelloDialogo()

                    pannello.Collega(contesto)
                    Await pannello.ApriIlDialogoAsync()

                    Assert.IsEmpty(Bolle(pannello), "nessuna conversazione cominciata")
                    Assert.IsFalse(pannello.HaUnDialogoInCorso, "e niente da perdere")

                    Dim stato As Label = Etichetta(pannello, "lblStatoDialogo")
                    Assert.Contains("chiave API", stato.Text, "lo dice")
                    Assert.AreEqual(StileApp.Pericolo, stato.ForeColor, "con il colore di chi non può funzionare")
                End Using

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Function

        <TestMethod>
        Public Sub LaFasciaDelleAzioniLasciaIlPostoAlLogo()

            ' Lo stesso vincolo geometrico di P2 (cap. 03.5): sotto il logo non va
            ' niente di vivo, e a cedere il posto è la fascia dei bottoni.
            Using pannello As New TrovaLavoro.PannelloDialogo()
                pannello.ImpostaIngombroLogo(New Size(261, 188))

                Dim azioni As Panel = DirectCast(
                    pannello.Controls.Find("pnlAzioni", searchAllChildren:=True).Single(), Panel)

                Assert.AreEqual(188, azioni.Height, "alta quanto il logo sfonda nell'area centrale")
                Assert.AreEqual(273, azioni.Padding.Left, "e i bottoni cominciano dopo la sua larghezza")
                Assert.IsGreaterThanOrEqualTo(273, Bottone(pannello, "btnTornaAlProfilo").Left,
                                              "nessun bottone sotto il logo")
            End Using

        End Sub

        ' ==================================================================
        ' Il banco
        ' ==================================================================

        ''' <summary>Un pannello con la conversazione già aperta sul finto strutturatore.</summary>
        Private Shared Async Function ConDialogoApertoAsync(finto As StrutturatoreFinto,
                                                            prova As Func(Of TrovaLavoro.PannelloDialogo, Task)) As Task

            Using pannello As New TrovaLavoro.PannelloDialogo()

                ' Senza handle i controlli non sono «realizzati»: nel banco va creato a
                ' mano, perché qui il pannello non è appeso a nessuna finestra.
                pannello.CreateControl()

                Await pannello.ApriIlDialogoAsync(finto)
                Await prova(pannello)
            End Using

        End Function

        ''' <summary>Scrive nella casella e manda, come farebbe il bottone «Invia».</summary>
        Private Shared Function RispondiAsync(pannello As TrovaLavoro.PannelloDialogo, testo As String) As Task

            Casella(pannello).Text = testo
            Return pannello.InviaLaRispostaAsync()

        End Function

        Private Shared Async Function RispondiEConfermaAsync(pannello As TrovaLavoro.PannelloDialogo,
                                                             testo As String) As Task

            Await RispondiAsync(pannello, testo)
            Await pannello.ScegliAsync(Scelte.Conferma)

        End Function

        ''' <summary>Porta il dialogo fino all'apertura del turno delle esperienze di lavoro.</summary>
        Private Shared Async Function FinoAlleEsperienzeAsync(pannello As TrovaLavoro.PannelloDialogo) As Task

            Await RispondiEConfermaAsync(pannello, "Mi chiamo Luca Ferrari")
            Await RispondiEConfermaAsync(pannello, "luca@example.it, 333 0000000, Forlì")
            Await RispondiEConfermaAsync(pannello, "Sì, la B")

        End Function

        ''' <summary>Il giro completo dei sette turni, fino al riepilogo.</summary>
        Private Shared Async Function FinoInFondoAsync(pannello As TrovaLavoro.PannelloDialogo) As Task

            Await FinoAlleEsperienzeAsync(pannello)

            Await RispondiEConfermaAsync(pannello, "Ho fatto il magazziniere")
            Await pannello.ScegliAsync(Scelte.Procedi)

            Await RispondiEConfermaAsync(pannello, "Davo una mano nei traslochi")
            Await pannello.ScegliAsync(Scelte.Procedi)

            ' Le competenze si raccolgono in blocco: niente scheda da confermare, solo
            ' «ne aggiungo altre?».
            Await RispondiAsync(pannello, "So usare il muletto")
            Await pannello.ScegliAsync(Scelte.Conferma)

            Await RispondiEConfermaAsync(pannello, "Ho il diploma")
            Await pannello.ScegliAsync(Scelte.Procedi)

        End Function

        Private Shared Function TuttiISetteTurni() As StrutturatoreFinto

            Return New StrutturatoreFinto().
                Dara(FrNome).Dara(FrContatti()).Dara(FrPatente).Dara(FrLavoro).
                Dara(FrInformale).Dara(FrCompetenze).Dara(FrFormazione)

        End Function

        ''' <summary>
        ''' I testi delle bolle, nell'ordine in cui si leggono. Ogni bolla è un
        ''' contenitore con dentro la cornice — che porta il nome del tipo — e il fondo
        ''' colorato con le righe di testo.
        ''' </summary>
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

        ''' <summary>Tutta la conversazione in una stringa sola, per cercarci dentro.</summary>
        Private Shared Function Conversazione(pannello As Control) As String
            Return String.Join(vbLf, Bolle(pannello))
        End Function

        Private Shared Function Livello(pannello As Control, nome As String) As LivelloBottone
            Return DirectCast(Bottone(pannello, nome).Tag, LivelloBottone)
        End Function

        Private Shared Function Casella(pannello As Control) As TextBox
            Return DirectCast(pannello.Controls.Find("txtRisposta", searchAllChildren:=True).Single(), TextBox)
        End Function

        Private Shared Function Bottone(pannello As Control, nome As String) As Button
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Etichetta(pannello As Control, nome As String) As Label
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

    End Class

End Namespace
