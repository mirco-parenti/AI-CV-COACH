Imports System.IO
Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi del pannello P1, la Home (cap. 03.6; cap. 07.3). Girano <b>senza rete</b>
    ''' e senza chiave: il cruscotto non chiama mai l'AI — legge il disco e mostra.
    ''' </summary>
    ''' <remarks>
    ''' Le regole del registro sono già collaudate in <c>CollaudiRegistro</c>. Qui si
    ''' verifica solo quello che il pannello aggiunge: che la coda mostri quel che c'è
    ''' nelle cartelle, che il filtro e l'ordinamento facciano quel che dicono, che dalla
    ''' riga scelta esca la candidatura giusta — ed è quella la promessa di T5c — e che
    ''' senza profilo la Home dica da dove si comincia invece di mostrare un vuoto.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiPannelloHome

        ''' <summary>
        ''' Una candidatura come la scrive il flusso vero, allo stato indicato. Il giorno
        ''' entra nel nome della cartella e quindi nell'ordine dell'elenco.
        ''' </summary>
        Private Shared Function Candidatura(azienda As String, giorno As Integer,
                                            stelle As Double?,
                                            arrivataA As StatoOpportunita) As Opportunita

            Dim o As New Opportunita With {
                .Creata = New Date(2026, 8, giorno, 9, 30, 0),
                .Fonte = "Indeed",
                .Annuncio = JsonNode.Parse(
                    $"{{""titolo"": ""Magazziniere"", ""azienda"": ""{azienda}""}}")}

            o.Avanza(StatoOpportunita.Nuova, o.Creata)

            If arrivataA <> StatoOpportunita.Nuova Then
                o.Confronto = JsonNode.Parse("{""giudizi"": [{""requisito"": ""Patente B""}]}")
                o.Match = New RisultatoMatch With {.Stelle = stelle, .GateEliminatorio = False}
                o.Avanza(StatoOpportunita.Interessante, o.Creata.AddMinutes(2))
            End If

            ' Una candidatura inviata è passata per forza dalla generazione: i documenti
            ' sono quello che parte, e la macchina a stati non ammette il salto.
            If arrivataA = StatoOpportunita.Generata OrElse arrivataA = StatoOpportunita.Inviata Then
                o.Cv = JsonNode.Parse("{""intestazione"": {}}")
                o.Avanza(StatoOpportunita.Generata, o.Creata.AddMinutes(9))
            End If

            If arrivataA = StatoOpportunita.Inviata Then
                o.Avanza(StatoOpportunita.Inviata, o.Creata.AddMinutes(20))
            End If

            If arrivataA = StatoOpportunita.Scartata Then
                o.Avanza(StatoOpportunita.Scartata, o.Creata.AddMinutes(5))
            End If

            Return o

        End Function

        ''' <summary>Le tre candidature che quasi tutti questi collaudi vogliono trovare.</summary>
        Private Shared Sub TreCandidature(candidature As ArchivioOpportunita)

            candidature.Salva(Candidatura("Rossi S.p.A.", 10, 4.1, StatoOpportunita.Generata))
            candidature.Salva(Candidatura("Bianchi S.r.l.", 11, 2.3, StatoOpportunita.Interessante))
            candidature.Salva(Candidatura("Verdi & C.", 12, 0.8, StatoOpportunita.Scartata))

        End Sub

        Private Shared Sub ConPannelloHome(prova As Action(Of PannelloHome, ContestoApp),
                                           Optional preparazione As Action(Of ArchivioOpportunita) = Nothing,
                                           Optional conProfilo As Boolean = True)

            Dim radice As String = Path.Combine(
                Path.GetTempPath(), "pannello-home-" & Guid.NewGuid().ToString("N"))

            Try
                ' Chiave vuota: la Home deve funzionare comunque, perché non chiede niente
                ' all'AI — è la stessa ragione per cui l'archivio sta nel motore anche
                ' senza client (cap. 12.7).
                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      pannello As New PannelloHome()

                    If conProfilo Then
                        contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))
                    End If

                    preparazione?.Invoke(contesto.Opportunita)

                    ' Senza handle i controlli non sono «realizzati»: qui il pannello non
                    ' è appeso a nessuna finestra, e va creato a mano.
                    pannello.CreateControl()
                    pannello.Collega(contesto)

                    prova(pannello, contesto)
                End Using

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

        ' ==================================================================
        ' La coda
        ' ==================================================================

        <TestMethod>
        Public Sub LaCodaMostraLeCandidatureCheStannoSuDisco()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim righe As List(Of ListViewItem) = Coda(pannello)
                    Assert.HasCount(3, righe, "una riga per cartella")

                    ' La più recente in cima: è l'ordine predefinito, per data.
                    Assert.AreEqual("Verdi & C.", righe(0).SubItems(1).Text)
                    Assert.AreEqual("Rossi S.p.A.", righe(2).SubItems(1).Text)

                    Dim rossi As ListViewItem = righe(2)
                    Assert.Contains("★★★★☆", rossi.SubItems(0).Text, "le stelle si vedono")
                    Assert.Contains("4,1", rossi.SubItems(0).Text.Replace(".", ","), "col numero")
                    Assert.AreEqual("Generata", rossi.SubItems(3).Text, "e lo stato")
                    Assert.AreEqual("Indeed", rossi.SubItems(4).Text, "e da dove veniva")
                End Sub, AddressOf TreCandidature)
        End Sub

        <TestMethod>
        Public Sub UnaScartataRestaNellElencoMaNonPesa()
            ' Scartare non è cancellare (cap. 07.3): la candidatura resta, in grigio.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim scartata As ListViewItem = Coda(pannello).
                        Single(Function(r) r.SubItems(1).Text = "Verdi & C.")

                    Assert.AreEqual("Scartata", scartata.SubItems(3).Text)
                    Assert.AreEqual(StileApp.TestoSecondario, scartata.ForeColor, "scritta in grigio")
                End Sub, AddressOf TreCandidature)
        End Sub

        <TestMethod>
        Public Sub IContatoriDiconoAChePuntoSiE()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim detto As String = Etichetta(pannello, "lblContatori").Text

                    Assert.Contains("1 da completare", detto)
                    Assert.Contains("1 generata", detto)
                    Assert.Contains("1 scartata", detto)
                End Sub, AddressOf TreCandidature)
        End Sub

        <TestMethod>
        Public Sub LeInviateSiContanoDaT6()
            ' Il contatore è entrato con la tappa che lo fa salire (cap. 07.3): prima di T6
            ' lo stato «inviata» non si raggiungeva, e un numero fermo a zero non conta
            ' niente. È anche il solo contatore che dice quante candidature sono partite
            ' davvero — cioè la domanda per cui il registro esiste.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim detto As String = Etichetta(pannello, "lblContatori").Text

                    Assert.Contains("2 inviate", detto, "quelle partite")
                    Assert.Contains("1 generata", detto, "e quelle ferme un passo prima")
                End Sub,
                Sub(candidature)
                    candidature.Salva(Candidatura("Rossi S.p.A.", 10, 4.1, StatoOpportunita.Inviata))
                    candidature.Salva(Candidatura("Neri S.p.A.", 11, 3.2, StatoOpportunita.Inviata))
                    candidature.Salva(Candidatura("Verdi & C.", 12, 2.0, StatoOpportunita.Generata))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IContatoriConcordanoLaParolaColNumero()
            ' Il gemello del collaudo qui sopra, dall'altra parte: lì l'uno, qui i molti.
            ' «1 generate» è la stonatura vista sull'applicazione vera al collaudo di T5c —
            ' e per accorgersene bastava scartare la prima candidatura.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.Contains("2 generate", Etichetta(pannello, "lblContatori").Text)
                End Sub,
                Sub(candidature)
                    candidature.Salva(Candidatura("Rossi S.p.A.", 10, 4.1, StatoOpportunita.Generata))
                    candidature.Salva(Candidatura("Neri S.p.A.", 11, 3.2, StatoOpportunita.Generata))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaCandidatureLaCodaLoDiceInvecediRestareMuta()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.IsEmpty(Coda(pannello))
                    Assert.Contains("Nessuna candidatura", Etichetta(pannello, "lblContatori").Text)
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlFiltroMostraSoloQuelloCheDice()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim mostra As ComboBox = Tendina(pannello, "cboMostra")

                    mostra.SelectedItem = "Generate"
                    Assert.AreEqual("Rossi S.p.A.", Coda(pannello).Single().SubItems(1).Text)

                    mostra.SelectedItem = "Da completare"
                    Assert.AreEqual("Bianchi S.r.l.", Coda(pannello).Single().SubItems(1).Text)

                    mostra.SelectedItem = "Scartate"
                    Assert.AreEqual("Verdi & C.", Coda(pannello).Single().SubItems(1).Text)

                    mostra.SelectedItem = "Tutte"
                    Assert.HasCount(3, Coda(pannello))

                    ' I contatori contano tutto: dicono a che punto si è, non cosa si sta
                    ' guardando adesso.
                    Assert.Contains("1 generata", Etichetta(pannello, "lblContatori").Text)
                End Sub, AddressOf TreCandidature)
        End Sub

        <TestMethod>
        Public Sub IlFiltroPerStelleTieneQuelleDaLiInSu()
            ' Il cap. 07.3 chiede un elenco filtrabile «per stato e stelle»: lo stato c'era
            ' da T5c, le stelle no. La domanda vera di chi ha una coda lunga non è «quali
            ' valgono 3» ma «quali valgono da 3 in su».
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim stelle As ComboBox = Tendina(pannello, "cboStelle")

                    stelle.SelectedItem = "almeno 3 ★"
                    Assert.AreEqual("Rossi S.p.A.", Coda(pannello).Single().SubItems(1).Text,
                                    "solo quella da 4,1")

                    stelle.SelectedItem = "almeno 2 ★"
                    Assert.HasCount(2, Coda(pannello), "anche quella da 2,3")

                    stelle.SelectedItem = "tutte"
                    Assert.HasCount(3, Coda(pannello), "e senza filtro ci sono tutte")
                End Sub, AddressOf TreCandidature)
        End Sub

        <TestMethod>
        Public Sub IDueFiltriSiSommanoInvecediSostituirsi()
            ' Sono due domande diverse — a che punto sono, e quanto valgono — e chi le fa
            ' entrambe si aspetta l'incrocio, non l'ultima delle due.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Tendina(pannello, "cboMostra").SelectedItem = "Generate"
                    Tendina(pannello, "cboStelle").SelectedItem = "almeno 4 ★"
                    Assert.AreEqual("Rossi S.p.A.", Coda(pannello).Single().SubItems(1).Text)

                    Tendina(pannello, "cboMostra").SelectedItem = "Da completare"
                    Assert.IsEmpty(Coda(pannello), "da completare ce n'è una, ma vale 2,3")
                End Sub, AddressOf TreCandidature)
        End Sub

        <TestMethod>
        Public Sub UnaCandidaturaSenzaStelleNonPassaIlFiltro()
            ' Una candidatura appena catturata non è ancora stata confrontata: non è che
            ' valga poco, è che non lo sappiamo. Con un filtro sulle stelle non passa — e
            ' senza, si vede come tutte le altre.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.HasCount(2, Coda(pannello), "senza filtro ci sono entrambe")

                    Tendina(pannello, "cboStelle").SelectedItem = "almeno 1 ★"
                    Assert.AreEqual("Rossi S.p.A.", Coda(pannello).Single().SubItems(1).Text,
                                    "quella mai confrontata resta fuori")
                End Sub,
                Sub(candidature)
                    candidature.Salva(Candidatura("Rossi S.p.A.", 10, 4.1, StatoOpportunita.Generata))
                    candidature.Salva(Candidatura("Neri S.p.A.", 11, Nothing, StatoOpportunita.Nuova))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub QuandoIFiltriNascondonoQualcosaILContatoreLoDice()
            ' Una coda che si accorcia senza spiegazione fa credere che una candidatura sia
            ' sparita. I contatori restano sul totale, ma dicono quante se ne stanno vedendo.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.DoesNotContain("ne vedi", Etichetta(pannello, "lblContatori").Text,
                                          "senza filtri non c'è niente da spiegare")

                    Tendina(pannello, "cboStelle").SelectedItem = "almeno 3 ★"
                    Assert.Contains("ne vedi 1 su 3", Etichetta(pannello, "lblContatori").Text)
                End Sub, AddressOf TreCandidature)
        End Sub

        <TestMethod>
        Public Sub CliccandoUnIntestazioneSiOrdinaERiCliccandolaSiGira()
            ConPannelloHome(
                Sub(pannello, contesto)
                    ' Colonna 1: l'azienda. Una colonna di nomi parte dalla A.
                    pannello.OrdinaPer(1)
                    Assert.AreEqual("Bianchi S.r.l., Rossi S.p.A., Verdi & C.", Aziende(pannello))

                    pannello.OrdinaPer(1)
                    Assert.AreEqual("Verdi & C., Rossi S.p.A., Bianchi S.r.l.", Aziende(pannello),
                                    "il secondo clic gira il verso")

                    ' Colonna 0: il match. Le stelle partono dalla più alta, e quella senza
                    ' punteggio finisce in fondo.
                    pannello.OrdinaPer(0)
                    Assert.AreEqual("Rossi S.p.A., Bianchi S.r.l., Verdi & C.", Aziende(pannello))
                End Sub, AddressOf TreCandidature)
        End Sub

        ' ==================================================================
        ' Il profilo in sintesi
        ' ==================================================================

        <TestMethod>
        Public Sub ConIlProfiloLaHomeDiceChiSeiEDiQuando()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.Contains("salvato il", Etichetta(pannello, "lblProfilo").Text)
                    Assert.AreEqual("Apri il profilo", Bottone(pannello, "btnApriProfilo").Text)
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaProfiloLaHomeDiceDaDoveSiComincia()
            ' È la schermata del primo avvio: senza profilo non si fa nient'altro, e il
            ' cruscotto deve mandare lì invece di mostrare un cruscotto vuoto.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.Contains("da lì che si comincia", Etichetta(pannello, "lblProfilo").Text)
                    Assert.AreEqual("Costruisci il profilo", Bottone(pannello, "btnApriProfilo").Text,
                                    "il bottone cambia mestiere")
                End Sub, conProfilo:=False)
        End Sub

        ' ==================================================================
        ' Riaprire una candidatura: il debito di T4 che si chiude
        ' ==================================================================

        <TestMethod>
        Public Sub DallaRigaSceltaEsceLaCandidaturaIntera()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim uscita As Opportunita = Nothing
                    AddHandler pannello.CandidaturaScelta,
                        Sub(mittente, e) uscita = e.Candidatura

                    Coda(pannello).Single(Function(r) r.SubItems(1).Text = "Rossi S.p.A.").Selected = True
                    Bottone(pannello, "btnApriCandidatura").PerformClick()

                    Assert.IsNotNull(uscita, "la candidatura è arrivata a chi mostra P4")
                    Assert.AreEqual("Rossi S.p.A.", uscita.Azienda)
                    Assert.AreEqual(StatoOpportunita.Generata, uscita.Stato, "col suo stato")
                    Assert.IsNotNull(uscita.Cv, "e con dentro i documenti già scritti")
                    Assert.IsNotNull(uscita.Giudizi(), "e i giudizi di allora")
                End Sub, AddressOf TreCandidature)
        End Sub

        <TestMethod>
        Public Sub SenzaUnaRigaSceltaNonCENienteDaAprire()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.IsFalse(Bottone(pannello, "btnApriCandidatura").Enabled,
                                   "spento finché non si sceglie")

                    Coda(pannello).First().Selected = True
                    Assert.IsTrue(Bottone(pannello, "btnApriCandidatura").Enabled)
                End Sub, AddressOf TreCandidature)
        End Sub
        ' ==================================================================
        ' Eliminare una candidatura (cap. 11.5)
        ' ==================================================================

        <TestMethod>
        Public Sub SenzaUnaRigaSceltaNonCENienteDaEliminare()

            ' Un bottone rosso che non ha niente da fare insegna solo a non fidarsi del
            ' colore: è la stessa regola dell'«ELIMINA PROFILO» di P2 (cap. 03.6).
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim elimina As Button = Bottone(pannello, "btnEliminaCandidatura")

                    Assert.IsFalse(elimina.Enabled, "spento finché non si sceglie")

                    Coda(pannello).First().Selected = True
                    Assert.IsTrue(elimina.Enabled, "acceso sulla riga scelta, e su quella sola")
                End Sub, AddressOf TreCandidature)

        End Sub

        <TestMethod>
        Public Sub LaConfermaDiceQualeCandidaturaSparisceECosaCEraDentro()

            ' La domanda deve nominare la candidatura come la nomina la riga: chi legge
            ' «la tua cartella» e basta non sa se sta per perdere una candidatura o tutto.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim voce As VoceRegistro = DirectCast(
                        Coda(pannello).Single(Function(r) r.SubItems(1).Text = "Rossi S.p.A.").Tag,
                        VoceRegistro)

                    Dim domanda As String = PannelloHome.SpiegazioneDellEliminazione(voce)

                    Assert.Contains("Rossi S.p.A.", domanda, "quale candidatura sparisce")
                    Assert.Contains("Magazziniere", domanda, "detta com'è scritta nella riga")
                    Assert.Contains("CV mirato", domanda, "e che cosa c'era dentro la cartella")
                    Assert.Contains("Non si torna indietro", domanda, "e che non si disfa")
                End Sub, AddressOf TreCandidature)

        End Sub

        <TestMethod>
        Public Sub IlPercorsoCompostoDallaCodaEQuelloCheIPannelliHannoInMano()

            ' La Home compone il percorso della candidatura mettendo insieme la cartella
            ' delle opportunità e il nome che sta nella voce del registro; i pannelli
            ' tengono invece quello che l'archivio ha scritto in Opportunita.Cartella. È su
            ' quei due che si riconoscono quando una candidatura viene eliminata: se
            ' divergessero, la scheda non capirebbe che quella era la sua — e continuerebbe
            ' a poterla riscrivere su disco, ricreandola (cap. 11.5).
            Dim salvata As Opportunita = Nothing

            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim voce As VoceRegistro = DirectCast(Coda(pannello).Single().Tag, VoceRegistro)

                    Assert.AreEqual(salvata.Cartella,
                                    Path.Combine(contesto.Cartella.CartellaOpportunita, voce.Cartella),
                                    "lo stesso percorso, composto per due strade")
                End Sub,
                Sub(candidature)
                    salvata = Candidatura("Rossi S.p.A.", 10, 4.1, StatoOpportunita.Generata)
                    candidature.Salva(salvata)
                End Sub)

        End Sub

        <TestMethod>
        Public Sub EliminataUnaCandidaturaLaCodaEIlRegistroNonLaNominanoPiu()

            ' Il registro non si aggiorna a mano: è il riflesso delle cartelle e si rifà da
            ' sé, perché non combacia più. Ma deve anche tornare su disco — chi lo
            ' rileggesse (il prossimo avvio, il server MCP) troverebbe una candidatura che
            ' non c'è.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim dove As String = contesto.Opportunita.Elenco().
                        Single(Function(c) Path.GetFileName(c).Contains("rossi"))

                    contesto.Opportunita.Elimina(dove)
                    pannello.Aggiorna()

                    Assert.HasCount(2, Coda(pannello), "una riga in meno")
                    Assert.DoesNotContain("Rossi S.p.A.", Aziende(pannello), "e non è più in elenco")

                    Assert.DoesNotContain("Rossi", File.ReadAllText(contesto.Cartella.FileRegistro),
                                          "nemmeno nell'indice su disco")
                End Sub, AddressOf TreCandidature)

        End Sub


        <TestMethod>
        Public Sub UnaCartellaSpostataDaSottoNonFaCadereNiente()
            ' L'utente è padrone dei suoi file (cap. 11.1): può rinominare o spostare una
            ' cartella da Esplora file mentre l'applicazione è aperta. Chi sceglie quella
            ' riga deve sentirselo dire, non vedere un crash.
            '
            ' Si sposta invece di cancellare, e non è un dettaglio: su Windows una
            ' Directory.Delete resta «in sospeso» finché non si chiude l'ultimo handle, e
            ' per un istante la cartella risulta ancora lì — con dentro più niente. Il
            ' collaudo che cancellava passava quasi sempre e ogni tanto no.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim aperta As Boolean = False
                    AddHandler pannello.CandidaturaScelta, Sub(mittente, e) aperta = True

                    Dim spostata As String = contesto.Opportunita.Elenco().First()
                    Coda(pannello).Single(Function(r) r.SubItems(1).Text = "Rossi S.p.A.").Selected = True

                    Directory.Move(spostata, spostata & "-altrove")
                    Bottone(pannello, "btnApriCandidatura").PerformClick()

                    Assert.IsFalse(aperta, "non c'è niente da aprire")
                    Assert.Contains("non si è lasciata riaprire",
                                    Etichetta(pannello, "lblStatoHome").Text)
                End Sub, AddressOf TreCandidature)
        End Sub

        ' ==================================================================
        ' L'indice
        ' ==================================================================

        <TestMethod>
        Public Sub GuardareLaHomeMetteInRigaLIndice()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.IsTrue(File.Exists(contesto.Cartella.FileRegistro),
                                  "l'indice ricostruito è stato scritto")

                    ' Una candidatura nuova, arrivata mentre si guardava altrove: il
                    ' cruscotto se ne accorge appena torna a guardare (cap. 03.8).
                    contesto.Opportunita.Salva(
                        Candidatura("Neri S.p.A.", 13, 3.0, StatoOpportunita.Interessante))

                    pannello.Aggiorna()

                    Assert.HasCount(4, Coda(pannello))
                    Assert.AreEqual("Neri S.p.A.", Coda(pannello).First().SubItems(1).Text,
                                    "ed è la più recente")
                End Sub, AddressOf TreCandidature)
        End Sub

        <TestMethod>
        Public Sub AlPrimoAvvioNonNasceNessunFile()
            ' Chi apre l'applicazione e la richiude senza fare niente non deve trovarsi
            ' un registro.json che non racconta niente.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.IsFalse(File.Exists(contesto.Cartella.FileRegistro))
                End Sub, conProfilo:=False)
        End Sub

        ' ==================================================================
        ' Attrezzi
        ' ==================================================================

        ' ==================================================================
        ' Il promemoria di follow-up e l'esito (T9c, cap. 07.3)
        ' ==================================================================

        ''' <summary>
        ''' Una candidatura spedita <b>tanti giorni fa quanti se ne chiedono</b>, con
        ''' l'esito che si vuole.
        ''' </summary>
        ''' <remarks>
        ''' Le date sono relative a oggi e non fisse come nelle altre prove di questo
        ''' banco: qui si misura un'attesa, e una data scritta nel calendario diventerebbe
        ''' «ferma da mille giorni» il mese prossimo — un collaudo che passa oggi e mente
        ''' domani.
        ''' </remarks>
        Private Shared Function Spedita(azienda As String, giorniFa As Integer,
                                        Optional esito As EsitoCandidatura? = Nothing) As Opportunita

            Dim invio As Date = Date.Now.AddDays(-giorniFa)

            Dim o As New Opportunita With {
                .Creata = invio.AddHours(-2),
                .Fonte = "Indeed",
                .Annuncio = JsonNode.Parse(
                    $"{{""titolo"": ""Magazziniere"", ""azienda"": ""{azienda}""}}"),
                .Confronto = JsonNode.Parse("{""giudizi"": [{""requisito"": ""Patente B""}]}"),
                .Match = New RisultatoMatch With {.Stelle = 3.5},
                .Cv = JsonNode.Parse("{""intestazione"": {}}")}

            o.Avanza(StatoOpportunita.Interessante, o.Creata)
            o.Avanza(StatoOpportunita.Generata, o.Creata.AddMinutes(30))
            o.Avanza(StatoOpportunita.Inviata, invio)

            If esito.HasValue Then o.SegnaEsito(esito.Value, Date.Now.AddHours(-1))

            Return o

        End Function

        <TestMethod>
        Public Sub IlPromemoriaRicordaLeSpediteFermeDaTroppo()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Dim avviso As Label = Etichetta(pannello, "lblPromemoria")

                    Assert.IsTrue(avviso.Visible, "una aspetta da venti giorni")
                    Assert.Contains("Acme", avviso.Text, "e la riga dice quale")
                    Assert.Contains("20", avviso.Text, "e da quanto")

                    ' La riga stessa lo dice, e non solo col colore: un colore da solo si
                    ' legge «importante» tanto quanto «in ritardo».
                    Dim ferma As ListViewItem = Coda(pannello).
                        Single(Function(r) r.SubItems(1).Text = "Acme")
                    Assert.Contains("20 gg", ferma.SubItems(3).Text)

                    Dim fresca As ListViewItem = Coda(pannello).
                        Single(Function(r) r.SubItems(1).Text = "Bianchi")
                    Assert.AreEqual("Inviata", fresca.SubItems(3).Text, "quella di ieri no")

                    ' E si isolano dal filtro, che è quello che serve su una coda lunga.
                    Tendina(pannello, "cboMostra").SelectedItem = "Da sollecitare"
                    Assert.AreEqual("Acme", Coda(pannello).Single().SubItems(1).Text)
                End Sub,
                Sub(candidature)
                    candidature.Salva(Spedita("Acme", giorniFa:=20))
                    candidature.Salva(Spedita("Bianchi", giorniFa:=1))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaNienteDaRicordareLaRigaSparisce()
            ' Un avviso che occupa il suo spazio anche da spento insegna a non guardarlo.
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.IsFalse(Etichetta(pannello, "lblPromemoria").Visible)
                End Sub,
                Sub(candidature)
                    candidature.Salva(Spedita("Bianchi", giorniFa:=2))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub ChiHaSaputoComEAndataNonAspettaPiu()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.IsFalse(Etichetta(pannello, "lblPromemoria").Visible,
                                   "una risposta è arrivata: l'attesa è finita, anche se è un no")

                    Dim riga As ListViewItem = Coda(pannello).Single()
                    Assert.AreEqual("Rifiutata", riga.SubItems(3).Text,
                                    "e la colonna dice com'è andata, non «con esito»")

                    Assert.Contains("1 con esito", Etichetta(pannello, "lblContatori").Text)
                End Sub,
                Sub(candidature)
                    candidature.Salva(Spedita("Acme", giorniFa:=40, esito:=EsitoCandidatura.Rifiutata))
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LaSogliaVieneDalleImpostazioni()
            ConPannelloHome(
                Sub(pannello, contesto)
                    Assert.IsFalse(Etichetta(pannello, "lblPromemoria").Visible,
                                   "con i quattordici giorni di casa, cinque non bastano")

                    contesto.ArchivioImpostazioni.Salva(New Impostazioni With {
                        .LinguaPredefinita = LinguaDocumenti.Italiano,
                        .RifinituraAttiva = True,
                        .GiorniFollowUp = 3})
                    contesto.RileggiLeImpostazioni()
                    pannello.Aggiorna()

                    Assert.IsTrue(Etichetta(pannello, "lblPromemoria").Visible,
                                  "abbassata la soglia, la stessa candidatura è ferma da troppo")

                    ' E zero la spegne del tutto, che è il modo di dire «non ricordarmelo».
                    contesto.ArchivioImpostazioni.Salva(New Impostazioni With {
                        .LinguaPredefinita = LinguaDocumenti.Italiano,
                        .RifinituraAttiva = True,
                        .GiorniFollowUp = 0})
                    contesto.RileggiLeImpostazioni()
                    pannello.Aggiorna()

                    Assert.IsFalse(Etichetta(pannello, "lblPromemoria").Visible)
                End Sub,
                Sub(candidature)
                    candidature.Salva(Spedita("Acme", giorniFa:=5))
                End Sub)
        End Sub

        Private Shared Function Coda(pannello As Control) As List(Of ListViewItem)

            Dim elenco As ListView = DirectCast(
                pannello.Controls.Find("lvwCoda", searchAllChildren:=True).Single(), ListView)

            Return elenco.Items.Cast(Of ListViewItem)().ToList()

        End Function

        ''' <summary>Le aziende nell'ordine in cui la coda le mostra.</summary>
        Private Shared Function Aziende(pannello As Control) As String
            Return String.Join(", ", Coda(pannello).Select(Function(r) r.SubItems(1).Text))
        End Function

        Private Shared Function Bottone(pannello As Control, nome As String) As Button
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Etichetta(pannello As Control, nome As String) As Label
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

        Private Shared Function Tendina(pannello As Control, nome As String) As ComboBox
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), ComboBox)
        End Function

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

    End Class

End Namespace
