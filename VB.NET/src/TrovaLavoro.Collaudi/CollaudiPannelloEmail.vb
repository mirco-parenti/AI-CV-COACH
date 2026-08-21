Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi del pannello P7 (cap. 03.6; cap. 12, A8). Girano <b>senza rete</b>: il
    ''' compositore è finto, come i mestieri di P6.
    ''' </summary>
    ''' <remarks>
    ''' Le domande che contano sono quattro: che il messaggio nasca dalla lettera e non dal
    ''' nulla; che una bozza già scritta si riapra <b>com'era</b>, invece di essere
    ''' riscritta sopra; che gli allegati proposti siano i file che ci sono davvero; e che
    ''' «l'ho spedita» faccia una cosa sola — segnare, con la data. La conferma che lo
    ''' precede non si collauda qui: una <c>MessageBox</c> in un banco resta lì ad
    ''' aspettare per sempre, ed è per questo che l'atto sta in un metodo suo.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiPannelloEmail

        Private Const Lettera As String =
            "{""tipo"": ""lettera_mirata"", ""apertura"": ""Spettabile Azienda,""," &
            """corpo"": ""Ho quattro anni di magazzino."", ""chiusura"": ""Cordiali saluti,""," &
            """firma"": {""nome"": ""Luca Ferrari"", ""email"": ""luca@example.it""}}"

        Private Const AnnuncioLetto As String =
            "{""titolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A."", ""sede"": [""Forlì""]}"

        ''' <summary>Lo stesso annuncio, ma che dichiara a chi mandare (Pool 1.06).</summary>
        Private Const AnnuncioColContatto As String =
            "{""titolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A."", ""sede"": [""Forlì""]," &
            """contatto"": {""email"": ""selezione@rossi.it"", ""riferimento"": ""Ufficio Selezione""}}"

        Private Const EmailScritta As String =
            "{""tipo"": ""email_candidatura"", ""oggetto"": ""Candidatura per Magazziniere — Luca Ferrari""," &
            """corpo"": ""Spettabile Azienda,\nmi candido per la posizione.\nCordiali saluti,\nLuca Ferrari""}"

        <TestMethod>
        Public Async Function IlCorpoPassaDallAntiSlopMaLOggettoNo() As Task

            ' T7b, cap. 07.1. L'email nasce da una lettera già rifinita, ma il corpo lo
            ' riscrive l'AI da capo e i tic rientrano dalla finestra. L'oggetto invece è una
            ' formula dettata parola per parola dal prompt (Pool 1.07): rifinirla vorrebbe
            ' dire disfarla.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Dim rifinitore As RifinitoreFinto = New RifinitoreFinto().
                Dara("corpo", "Spettabile Azienda," & vbLf & "mi candido, e le scrivo perché…")

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.HasCount(1, rifinitore.Passate, "una passata sola")
                    Assert.AreEqual("corpo", rifinitore.Passate(0).Id(), "e sul corpo, non sull'oggetto")
                    Assert.AreEqual(GenereProsa.Prosa, rifinitore.Passate(0).Genere,
                                    "un'email è prosa distesa, come la lettera")

                    Assert.Contains("le scrivo perché", Casella(pannello, "txtCorpo").Text,
                                    "nella casella c'è il corpo rifinito")
                    Assert.AreEqual("Candidatura per Magazziniere — Luca Ferrari",
                                    Casella(pannello, "txtOggetto").Text, "e l'oggetto è quello di prima")
                End Function,
                rifinitore)

        End Function

        ''' <summary>
        ''' Il gemello del caso di P6: rifinitura inciampata, messaggio grezzo, e la
        ''' fascia che lo dice invece di lasciarlo credere rifinito (T9d).
        ''' </summary>
        <TestMethod>
        Public Async Function SeLaRifinituraInciampaIlMessaggioRestaGrezzoELoDice() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Dim rifinitore As New RifinitoreFinto With {
                .Fallira = New ErroreAi(CausaErroreAi.Servizio, "L'AI non risponde.")}

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsNotEmpty(Casella(pannello, "txtCorpo").Text,
                                      "il messaggio c'è lo stesso, col testo del compositore")

                    Assert.Contains("La rifinitura non è riuscita",
                                    Etichetta(pannello, "lblStatoEmail").Text,
                                    "detto con le stesse parole della pipeline")
                End Function,
                rifinitore)

        End Function

        <TestMethod>
        Public Async Function LaRifinituraDellEmailSegueLaLinguaDellaCandidatura() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Dim rifinitore As New RifinitoreFinto()

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Lingua = "en"
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("en", rifinitore.Passate.Single().Lingua,
                                    "un corpo inglese ripulito dai tic italiani sarebbe il guado di T7a")
                End Function,
                rifinitore)

        End Function

        <TestMethod>
        Public Async Function IlMessaggioNasceDallaLettera() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("Candidatura per Magazziniere — Luca Ferrari",
                                    Casella(pannello, "txtOggetto").Text, "l'oggetto")
                    Assert.Contains("mi candido per la posizione", Casella(pannello, "txtCorpo").Text, "il corpo")

                    ' La lettera è quel che gli è stato dato in pasto: è la fonte di fatti
                    ' dichiarata dal prompt (cap. 07.1).
                    Assert.HasCount(1, compositore.Chiamate, "una chiamata sola")
                    Assert.IsNotNull(compositore.Chiamate(0).Ingressi(0), "la lettera è arrivata")
                End Function)

        End Function

        <TestMethod>
        Public Async Function GliACapoDelMessaggioSiVedonoNellaCasella() As Task

            ' Visto sull'applicazione vera il 2026-08-14: l'AI scrive «\n», e una casella
            ' multiriga di Windows i ritorni a capo li mostra solo se sono CRLF. Il
            ' messaggio compariva tutto attaccato — «Cordiali saluti,Mirco Parenti» — e chi
            ' lo rilegge crede che sia stato scritto così.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim corpo As String = Casella(pannello, "txtCorpo").Text

                    Assert.Contains(vbCrLf, corpo, "gli a capo ci sono")
                    Assert.DoesNotContain("posizione." & vbLf, corpo, "e non sono quelli che Windows non mostra")
                    Assert.Contains("Cordiali saluti," & vbCrLf & "Luca Ferrari", corpo, "la firma va a capo")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlDestinatarioNonLoScriveMaiIlProgramma() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    ' Il cap. 07.1 è netto: se l'annuncio non porta un indirizzo, il campo
                    ' resta vuoto. Un indirizzo inventato è peggio di un campo da riempire.
                    ' Da T7a l'annuncio un indirizzo può portarlo (Pool 1.06), e questo
                    ' collaudo conta il doppio: «Rossi S.p.A.» non ne dichiara nessuno, e
                    ' né il prompt né il pannello devono ricavarne uno dall'azienda.
                    Assert.IsEmpty(Casella(pannello, "txtDestinatario").Text)
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlDestinatarioDellAnnuncioVienePropostoo() As Task

            ' L'altra metà della promessa del cap. 07.1, che fino a T6 non era mantenuta:
            ' se l'annuncio l'indirizzo lo scrive, si propone.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Annuncio = JsonNode.Parse(AnnuncioColContatto)

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("selezione@rossi.it", Casella(pannello, "txtDestinatario").Text,
                                    "l'indirizzo dell'annuncio arriva in casella")
                    Assert.Contains("preso dall'annuncio", Etichetta(pannello, "lblStatoEmail").Text,
                                    "e il pannello dice da dove viene, invece di farlo comparire dal nulla")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlRiferimentoDellAnnuncioNonFiniscePerDestinatario() As Task

            ' Il «riferimento» — l'ufficio, la persona, il codice della posizione — è un
            ' dato dell'annuncio, ma non è un indirizzo a cui si spedisce: nella casella
            ' del destinatario darebbe un'email che non parte.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Annuncio = JsonNode.Parse(
                        "{""titolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A.""," &
                        """contatto"": {""email"": """", ""riferimento"": ""Ufficio Selezione, rif. 4471/AB""}}")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsEmpty(Casella(pannello, "txtDestinatario").Text)
                End Function)

        End Function

        <TestMethod>
        Public Async Function LEmailSiScriveNellaLinguaDellaCandidatura() As Task

            ' L'ultimo anello della catena della lingua (cap. 10.1): la candidatura è in
            ' inglese, la lettera da cui l'email nasce pure, e l'email deve seguirle. Il
            ' collaudo reale di T7a l'ha trovata ferma qui — oggetto italiano sopra un
            ' corpo inglese — perché la lingua a P7 non arrivava proprio.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Lingua = "en"

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("en", compositore.LingueChieste.Single(),
                                    "la lingua della candidatura è arrivata al compositore")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaCandidaturaItalianaChiedeLEmailInItaliano() As Task

            ' Il gemello del collaudo di sopra: senza di lui «arriva la lingua» sarebbe
            ' dimostrato da un solo caso, e una lingua incollata a "en" lo passerebbe.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("it", compositore.LingueChieste.Single(),
                                    "la lingua di casa, che è quella della candidatura")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaBozzaRipresaTieneIlSuoDestinatario() As Task

            ' Se l'utente aveva già scritto (o corretto) il destinatario, riaprire la
            ' candidatura non deve rimetterci sopra quello dell'annuncio: lì c'è una sua
            ' decisione, e P7 è il pannello in cui l'utente scrive davvero.
            Dim compositore As New CompositoreFinto

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Annuncio = JsonNode.Parse(AnnuncioColContatto)
                    candidatura.Email = JsonNode.Parse(
                        "{""destinatario"": ""lavoro@rossi.it"", ""oggetto"": ""Candidatura""," &
                        """corpo"": ""Buongiorno."", ""allegati"": []}")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("lavoro@rossi.it", Casella(pannello, "txtDestinatario").Text,
                                    "vale quello che l'utente aveva lasciato")
                    Assert.IsEmpty(compositore.Chiamate, "e l'AI non viene disturbata")
                End Function)

        End Function

        <TestMethod>
        Public Async Function GliAllegatiPropostiSonoIDocumentiCheEsistono() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf", "CV_Luca_Rossi.docx", "Lettera_Rossi.pdf")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim elenco As CheckedListBox = Allegati(pannello)
                    Assert.HasCount(3, elenco.Items, "i tre file scritti")

                    ' Il PDF si spunta da sé: è il formato che si apre uguale dappertutto.
                    ' Il DOCX resta lì, spento, per chi lo vuole (cap. 07.1).
                    Assert.IsTrue(SpuntatoQuello(elenco, "CV_Luca_Rossi.pdf"), "il CV in PDF")
                    Assert.IsTrue(SpuntatoQuello(elenco, "Lettera_Rossi.pdf"), "la lettera in PDF")
                    Assert.IsFalse(SpuntatoQuello(elenco, "CV_Luca_Rossi.docx"), "il DOCX no")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlMessaggioSaQualiAllegatiNomina() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf", "CV_Luca_Rossi.docx")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    ' Al prompt arrivano solo quelli spuntati: nominare un allegato che non
                    ' parte è un'email che si smentisce da sola.
                    Assert.HasCount(1, compositore.AllegatiNominati, "una chiamata")
                    CollectionAssert.AreEqual({"CV_Luca_Rossi.pdf"}, compositore.AllegatiNominati(0).ToArray())
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaBozzaSalvataSiRiapreComEraSenzaDisturbareLAi() As Task

            Dim compositore As New CompositoreFinto

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")

                    candidatura.Email = JsonNode.Parse(
                        "{""destinatario"": ""lavoro@rossi.it"", ""oggetto"": ""Il mio oggetto""," &
                        """corpo"": ""Il testo che ho corretto a mano.""," &
                        """allegati"": [{""nome"": ""CV_Luca_Rossi.pdf"", ""da"": ""candidatura"", ""scelto"": false}]}")
                    contesto.Opportunita.Salva(candidatura)

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.AreEqual("lavoro@rossi.it", Casella(pannello, "txtDestinatario").Text)
                    Assert.AreEqual("Il mio oggetto", Casella(pannello, "txtOggetto").Text)
                    Assert.AreEqual("Il testo che ho corretto a mano.", Casella(pannello, "txtCorpo").Text)
                    Assert.IsFalse(SpuntatoQuello(Allegati(pannello), "CV_Luca_Rossi.pdf"),
                                   "anche la spunta tolta è lavoro dell'utente")

                    Assert.IsEmpty(compositore.Chiamate,
                                   "riscrivere sopra il lavoro di ieri sarebbe il modo peggiore di essere utili")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaBozzaInUnAltraLinguaLoDiceInveceDiTacere() As Task

            ' Chi cambia la tendina di P6 dopo aver già preparato l'email si ritrova
            ' documenti inglesi e la bozza italiana di prima. Riprenderla in silenzio la
            ' farebbe passare per quella giusta: si riprende lo stesso, perché è lavoro
            ' dell'utente, ma dicendo com'è e dov'è il bottone che la rifà.
            Dim compositore As New CompositoreFinto

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Lingua = "en"
                    candidatura.Email = JsonNode.Parse(
                        "{""destinatario"": ""lavoro@rossi.it"", ""oggetto"": ""Candidatura""," &
                        """corpo"": ""Buongiorno."", ""lingua"": ""it"", ""allegati"": []}")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.Contains("Fallo riscrivere", Etichetta(pannello, "lblStatoEmail").Text,
                                    "dice dov'è il bottone che la rifà")
                    Assert.AreEqual("Buongiorno.", Casella(pannello, "txtCorpo").Text,
                                    "il messaggio di ieri resta lì: rifarlo lo decide l'utente")
                    Assert.IsEmpty(compositore.Chiamate, "e l'AI non parte da sé")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaBozzaNellaStessaLinguaSiRiprendeSenzaAllarmi() As Task

            ' Il gemello del collaudo qui sopra: quando le lingue combaciano non c'è niente
            ' da dire, e dirlo sarebbe un falso allarme.
            Dim compositore As New CompositoreFinto

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Lingua = "en"
                    candidatura.Email = JsonNode.Parse(
                        "{""destinatario"": ""lavoro@rossi.it"", ""oggetto"": ""Application""," &
                        """corpo"": ""Dear Sir or Madam,"", ""lingua"": ""en"", ""allegati"": []}")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.Contains("da dove l'avevi lasciata", Etichetta(pannello, "lblStatoEmail").Text)
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaBozzaVecchiaSenzaLinguaNonSiFaPassarePerSbagliata() As Task

            ' Le bozze salvate prima che la lingua si annotasse non ce l'hanno: allora non
            ' si sa, e non sapere non è un motivo per mandare l'utente a rifare un lavoro
            ' che magari andava bene (è la regola del vuoto, cap. 10.1).
            Dim compositore As New CompositoreFinto

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Lingua = "en"
                    candidatura.Email = JsonNode.Parse(
                        "{""destinatario"": ""lavoro@rossi.it"", ""oggetto"": ""Candidatura""," &
                        """corpo"": ""Buongiorno."", ""allegati"": []}")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.Contains("da dove l'avevi lasciata", Etichetta(pannello, "lblStatoEmail").Text)
                End Function)

        End Function

        <TestMethod>
        Public Async Function LaBozzaSiRicordaInCheLinguaEStataScritta() As Task

            ' Senza questo campo su disco, domani non c'è modo di accorgersi che la lingua
            ' è cambiata: un testo non dichiara da sé in che lingua è.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Lingua = "en"

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Casella(pannello, "txtDestinatario").Text = "lavoro@rossi.it"
                    pannello.PreparaIlMessaggio()

                    Dim riletta As Opportunita = contesto.Opportunita.Carica(candidatura.Cartella)
                    Dim bozza As BozzaEmail = BozzaEmail.DaJson(riletta.Email)

                    Assert.AreEqual("en", bozza.Lingua, "la lingua con cui l'AI l'ha scritta")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaMessaggioNonSiPuoPreparareNienteEMenoCheMaiSpedire() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara("{""tipo"": ""email_candidatura"", ""oggetto"": """", ""corpo"": """"}")

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsFalse(Bottone(pannello, "btnPreparaEmail").Enabled, "niente da preparare")
                    Assert.IsFalse(Bottone(pannello, "btnHoSpedito").Enabled,
                                   "e men che meno da dichiarare spedito")
                End Function)

        End Function

        <TestMethod>
        Public Async Function PreparareScriveIlMessaggioAccantoAiDocumenti() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Casella(pannello, "txtDestinatario").Text = "lavoro@rossi.it"

                    Assert.IsTrue(Bottone(pannello, "btnPreparaEmail").Enabled, "c'è un messaggio da preparare")
                    pannello.PreparaIlMessaggio()

                    Dim scritti As String() = Directory.GetFiles(
                        Path.Combine(candidatura.Cartella, ArchivioOpportunita.NomeCartellaOut), "*.eml")

                    Assert.HasCount(1, scritti, "il messaggio è stato scritto")

                    Dim eml As String = File.ReadAllText(scritti(0), Encoding.ASCII)
                    Assert.Contains("To: lavoro@rossi.it", eml, "col destinatario scritto a mano")
                    Assert.Contains("X-Unsent: 1", eml, "e dichiarato bozza da inviare")
                    Assert.Contains("filename=""CV_Luca_Rossi.pdf""", eml, "con l'allegato spuntato")
                End Function)

        End Function

        <TestMethod>
        Public Async Function PreparareSalvaLaBozzaPerchéDomaniSiRitrovi() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Casella(pannello, "txtDestinatario").Text = "lavoro@rossi.it"
                    pannello.PreparaIlMessaggio()

                    Dim riletta As Opportunita = contesto.Opportunita.Carica(candidatura.Cartella)
                    Dim bozza As BozzaEmail = BozzaEmail.DaJson(riletta.Email)

                    Assert.IsNotNull(bozza, "la bozza è su disco")
                    Assert.AreEqual("lavoro@rossi.it", bozza.Destinatario)
                    Assert.Contains("mi candido per la posizione", bozza.Corpo)
                End Function)

        End Function

        <TestMethod>
        Public Async Function UscireDallaBarraInCimaSalvaLaBozzaComeIlBottone() As Task

            ' Difetto visto sull'applicazione vera il 2026-08-18: la bozza si salvava
            ' uscendo da «◀ Torna ai documenti», ma dalla barra di navigazione in cima si
            ' lasciava P7 senza passare di lì — e destinatario, spunte e messaggio
            ' riscritto (costato una chiamata all'AI) sparivano in silenzio. Peggio:
            ' rientrando, P7 rileggeva email.json e mostrava la bozza vecchia come se fosse
            ' l'ultima. Qui si chiama l'aggancio che la finestra usa a ogni cambio pannello.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Casella(pannello, "txtDestinatario").Text = "lavoro@rossi.it"
                    Casella(pannello, "txtCorpo").Text = "Riscritto a mano da me."

                    DirectCast(pannello, IPannelloCheSalvaUscendo).SalvaUscendo()

                    Dim riletta As Opportunita = contesto.Opportunita.Carica(candidatura.Cartella)
                    Dim bozza As BozzaEmail = BozzaEmail.DaJson(riletta.Email)

                    Assert.IsNotNull(bozza, "la bozza è su disco anche senza premere il bottone")
                    Assert.AreEqual("lavoro@rossi.it", bozza.Destinatario, "il destinatario scritto a mano")
                    Assert.Contains("Riscritto a mano da me.", bozza.Corpo, "e il messaggio riscritto")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UscireDueVolteDiFilaNonFaDanno() As Task

            ' L'aggancio si chiama a ogni uscita, e i bottoni propri del pannello salvano
            ' comunque per conto loro: le due strade si sovrappongono per costruzione, e
            ' devono poterlo fare senza che la seconda rovini quel che ha fatto la prima.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Casella(pannello, "txtDestinatario").Text = "lavoro@rossi.it"

                    Dim uscita As IPannelloCheSalvaUscendo = DirectCast(pannello, IPannelloCheSalvaUscendo)
                    uscita.SalvaUscendo()
                    uscita.SalvaUscendo()

                    Dim bozza As BozzaEmail = BozzaEmail.DaJson(
                        contesto.Opportunita.Carica(candidatura.Cartella).Email)

                    Assert.AreEqual("lavoro@rossi.it", bozza.Destinatario, "la bozza è quella")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaCandidaturaNonEreditaIlMessaggioDiQuellaDiPrima() As Task

            ' Trovato dalla revisione del 2026-08-18, guardando l'aggancio d'uscita qui
            ' sopra. Il pannello si riusa da una candidatura all'altra, e la bozza in
            ' memoria si riempie in due modi soli: ripresa dal disco, o scritta dall'AI.
            ' Ma la scrittura ha due uscite anticipate legittime — manca la chiave, manca
            ' la lettera — e in quei casi la bozza restava quella di prima: bastava
            ' cambiare pannello perché finisse nell'email.json della candidatura sbagliata.
            ' Qui la seconda candidatura non ha lettera, che è il caso più facile da avere.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")

                    ' La prima: le si scrive il messaggio e lo si salva.
                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Casella(pannello, "txtDestinatario").Text = "prima@rossi.it"
                    DirectCast(pannello, IPannelloCheSalvaUscendo).SalvaUscendo()

                    Assert.Contains("mi candido per la posizione",
                                    BozzaEmail.DaJson(
                                        contesto.Opportunita.Carica(candidatura.Cartella).Email).Corpo,
                                    "la prima ha il suo messaggio")

                    ' La seconda: senza lettera, l'email non si può scrivere.
                    Dim senzaLettera As New Opportunita With {
                        .Annuncio = JsonNode.Parse(AnnuncioLetto),
                        .Creata = New Date(2026, 8, 11)}
                    senzaLettera.Avanza(StatoOpportunita.Interessante, senzaLettera.Creata)
                    senzaLettera.Avanza(StatoOpportunita.Generata, senzaLettera.Creata)
                    contesto.Opportunita.Salva(senzaLettera)

                    Await pannello.MostraLaCandidaturaAsync(senzaLettera)

                    Assert.IsEmpty(Casella(pannello, "txtCorpo").Text,
                                   "a video non deve restare il messaggio dell'altra")

                    ' Ed è qui che prima si faceva il danno: uscendo dalla barra.
                    DirectCast(pannello, IPannelloCheSalvaUscendo).SalvaUscendo()

                    Dim sua As BozzaEmail = BozzaEmail.DaJson(
                        contesto.Opportunita.Carica(senzaLettera.Cartella).Email)

                    Assert.IsEmpty(If(sua?.Corpo, ""), "e nemmeno nel suo email.json")
                    Assert.IsEmpty(If(sua?.Destinatario, ""), "né il destinatario dell'altra")
                End Function)

        End Function

        <TestMethod>
        Public Async Function SenzaCandidaturaUscireNonScriveNiente() As Task

            ' Si entra in P7 e si esce subito, senza che ci sia una candidatura aperta:
            ' non c'è niente da salvare, e non deve succedere niente.
            Dim compositore As New CompositoreFinto

            Await ConPannelloAsync(compositore,
                Function(pannello, contesto, candidatura)
                    DirectCast(pannello, IPannelloCheSalvaUscendo).SalvaUscendo()

                    Assert.IsNull(contesto.Opportunita.Carica(candidatura.Cartella).Email,
                                  "nessuna bozza inventata dal nulla")
                    Assert.IsEmpty(compositore.Chiamate, "e nessuna chiamata all'AI")
                    Return Task.CompletedTask
                End Function)

        End Function

        <TestMethod>
        Public Async Function DichiararlaSpeditaLaPortaAInviataConLaData() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    pannello.SegnaComeInviata()

                    Dim riletta As Opportunita = contesto.Opportunita.Carica(candidatura.Cartella)

                    Assert.AreEqual(StatoOpportunita.Inviata, riletta.Stato, "lo stato del cap. 07.3")
                    Assert.IsTrue(riletta.DateStati.ContainsKey(StatoOpportunita.Inviata), "con la sua data")
                    Assert.AreEqual(Date.Today, riletta.DateStati(StatoOpportunita.Inviata).Date)
                End Function)

        End Function

        <TestMethod>
        Public Async Function RidichiararlaSpeditaConUnEsitoGiaSegnatoNonRompeNiente() As Task

            ' Da T9c una candidatura può essere andata **oltre** l'invio: ha un esito. Chi
            ' torna qui a rimandare la stessa email preme di nuovo «L'ho spedita», e prima
            ' quel gesto chiedeva alla macchina degli stati un passo indietro che non
            ' esiste — cioè sollevava, in faccia a chi non aveva sbagliato niente.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    pannello.SegnaComeInviata()

                    candidatura.SegnaEsito(EsitoCandidatura.Colloquio)
                    contesto.Opportunita.Salva(candidatura)

                    pannello.SegnaComeInviata()

                    ' Il punto è qui: prima l'eccezione veniva raccolta e raccontata, e chi
                    ' rimandava la sua email si vedeva dire «non sono riuscita» per un gesto
                    ' che andava benissimo. L'esito, intatto, non bastava a rivelarlo.
                    Assert.DoesNotContain("Non sono riuscita",
                                          Etichetta(pannello, "lblStatoEmail").Text)
                    Assert.Contains("Segnata come inviata", Etichetta(pannello, "lblStatoEmail").Text)

                    Dim riletta As Opportunita = contesto.Opportunita.Carica(candidatura.Cartella)

                    Assert.AreEqual(StatoOpportunita.Esito, riletta.Stato,
                                    "l'esito segnato non si perde per una seconda dichiarazione")
                    Assert.AreEqual(EsitoCandidatura.Colloquio, riletta.Esito)
                End Function)

        End Function

        <TestMethod>
        Public Async Function DichiararlaSpeditaLoDiceAncheAllIndice() As Task

            ' Difetto visto sull'applicazione vera il 2026-08-15, al collaudo di tappa: la
            ' cartella diceva «inviata» e la Home continuava a mostrare «generata». L'indice
            ' si fida di sé stesso finché l'insieme delle cartelle combacia (cap. 07.3), e un
            ' cambio di stato dentro una cartella non lo fa scattare: ad annotarlo dev'essere
            ' chi lo cambia, come già fanno P4 quando scarta e P6 quando genera.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    contesto.Registro.Salva(contesto.Registro.Carica())

                    pannello.SegnaComeInviata()

                    Dim voce As VoceRegistro = contesto.Registro.Carica().Trova(candidatura.Cartella)

                    Assert.IsNotNull(voce, "la candidatura è nell'indice")
                    Assert.AreEqual(StatoOpportunita.Inviata, voce.Stato,
                                    "e l'indice sa che è partita, senza aspettare una rigenerazione")
                End Function)

        End Function

        <TestMethod>
        Public Async Function IlMessaggioGiaScrittoNonSiAllegaASeStesso() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta).Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    pannello.PreparaIlMessaggio()

                    ' Si rientra: adesso nella cartella c'è anche il .eml appena scritto.
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    For Each voce As String In Allegati(pannello).Items.Cast(Of String)()
                        Assert.DoesNotEndWith(".eml", voce, "un messaggio dentro il messaggio no")
                    Next
                End Function)

        End Function

        <TestMethod>
        Public Async Function GliAttestatiDeiTuoiDocumentiSiPropongonoSpenti() As Task

            ' Cap. 07.1: gli attestati pertinenti della cartella documenti compaiono fra
            ' gli allegati, da spuntare. Spenti, perché quali provino qualcosa per questo
            ' annuncio lo sa l'utente — e mandarli tutti è il modo di non farne leggere
            ' nessuno.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")
                    CartellaDocumentiCon(contesto,
                                         attestati:={"HACCP.pdf"},
                                         altri:={"busta_paga.pdf"})

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim elenco As CheckedListBox = Allegati(pannello)
                    Dim voci As String() = elenco.Items.Cast(Of String)().ToArray()

                    Assert.HasCount(2, voci, "il documento generato e l'attestato")
                    Assert.AreEqual("CV_Luca_Rossi.pdf", voci(0), "prima quel che è nato per questa candidatura")
                    Assert.Contains("HACCP.pdf", voci(1), "poi l'attestato")
                    Assert.Contains("dai tuoi documenti", voci(1), "e si vede da dove viene")

                    Assert.IsTrue(elenco.GetItemChecked(0), "il CV in PDF parte")
                    Assert.IsFalse(elenco.GetItemChecked(1), "l'attestato aspetta la spunta")

                    ' Quel che l'AI mette in «altro» non si propone: una busta paga non si
                    ' manda a un'azienda per sbaglio.
                    Assert.IsFalse(voci.Any(Function(v) v.Contains("busta_paga")), "gli «altro» restano fuori")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnAttestatoCancellatoDalDiscoNonSiPropone() As Task

            ' L'elenco su disco dice cosa c'era l'ultima volta; a dire cosa c'è adesso è
            ' solo il disco (stessa regola degli allegati, cap. 07.1).
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Dim cartella As String = CartellaDocumentiCon(contesto, attestati:={"HACCP.pdf"})
                    File.Delete(Path.Combine(cartella, "HACCP.pdf"))

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsEmpty(Allegati(pannello).Items, "niente da allegare: quel file non c'è più")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnAttestatoSpuntatoParteDavvero() As Task

            ' La prova che la strada regge fino in fondo: l'attestato vive fuori dalla
            ' cartella della candidatura, e chi scrive il messaggio deve saperlo ritrovare.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")
                    CartellaDocumentiCon(contesto, attestati:={"HACCP.pdf"})

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim elenco As CheckedListBox = Allegati(pannello)
                    elenco.SetItemChecked(1, True)

                    Casella(pannello, "txtDestinatario").Text = "lavoro@rossi.it"
                    pannello.PreparaIlMessaggio()

                    Dim scritti As String() = Directory.GetFiles(
                        Path.Combine(candidatura.Cartella, ArchivioOpportunita.NomeCartellaOut), "*.eml")

                    Dim eml As String = File.ReadAllText(scritti(0), Encoding.ASCII)

                    Assert.Contains("filename=""CV_Luca_Rossi.pdf""", eml, "il CV generato")
                    Assert.Contains("filename=""HACCP.pdf""", eml, "e l'attestato preso dai documenti dell'utente")
                End Function)

        End Function

        ' ==================================================================
        ' Il banco
        ' ==================================================================

        ''' <summary>
        ''' Prepara la cartella documenti dell'utente: dei file veri su disco e le
        ''' categorie già riconosciute, come se la classificazione fosse già stata
        ''' confermata (cap. 05.2).
        ''' </summary>
        ''' <returns>La cartella, per chi vuole poi toccarne i file.</returns>
        Private Shared Function CartellaDocumentiCon(contesto As ContestoApp,
                                                     attestati As String(),
                                                     Optional altri As String() = Nothing) As String

            ' Sta sotto la radice del collaudo per essere buttata con lei, ma per il
            ' programma è una cartella qualunque: quel che conta è che sia fuori dalle
            ' cartelle delle candidature.
            Dim cartella As String = Path.Combine(contesto.Cartella.Radice, "documenti-di-luca")
            Directory.CreateDirectory(cartella)

            For Each nome As String In attestati
                File.WriteAllText(Path.Combine(cartella, nome), $"finto: {nome}")
                contesto.Raccolta.Documenti.Add(New DocumentoClassificato With {
                    .Nome = nome, .Categoria = CategoriaDocumento.Attestato})
            Next

            For Each nome As String In If(altri, Array.Empty(Of String)())
                File.WriteAllText(Path.Combine(cartella, nome), $"finto: {nome}")
                contesto.Raccolta.Documenti.Add(New DocumentoClassificato With {
                    .Nome = nome, .Categoria = CategoriaDocumento.Altro})
            Next

            contesto.Raccolta.Cartella = cartella
            Return cartella

        End Function

        Private Shared Async Function ConPannelloAsync(
                compositore As CompositoreFinto,
                prova As Func(Of PannelloEmail, ContestoApp, Opportunita, Task),
                Optional rifinitore As RifinitoreFinto = Nothing) As Task

            Dim radice As String = Path.Combine(
                Path.GetTempPath(), "pannello-email-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      pannello As New PannelloEmail()

                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))

                    pannello.CreateControl()
                    pannello.Collega(contesto, compositore, Nothing,
                                     If(rifinitore Is Nothing, Nothing, New Rifinitura(rifinitore)))

                    Await prova(pannello, contesto, Generata(contesto))
                End Using

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Function

        ''' <summary>Una candidatura arrivata fino ai documenti: è da lì che nasce l'email.</summary>
        Private Shared Function Generata(contesto As ContestoApp) As Opportunita

            Dim candidatura As New Opportunita With {
                .Annuncio = JsonNode.Parse(AnnuncioLetto),
                .Confronto = JsonNode.Parse("{""giudizi"": [], ""lettura_insieme"": ""In linea.""}"),
                .Cv = JsonNode.Parse("{""tipo"": ""cv_mirato""}"),
                .Lettera = JsonNode.Parse(Lettera),
                .Creata = New Date(2026, 8, 10)}

            candidatura.Avanza(StatoOpportunita.Interessante, candidatura.Creata)
            candidatura.Avanza(StatoOpportunita.Generata, candidatura.Creata)

            contesto.Opportunita.Salva(candidatura)
            Return candidatura

        End Function

        ''' <summary>Mette nella <c>out\</c> della candidatura dei file veri da allegare.</summary>
        Private Shared Sub ScriviDocumenti(candidatura As Opportunita, ParamArray nomi As String())

            Dim cartella As String = Path.Combine(candidatura.Cartella, ArchivioOpportunita.NomeCartellaOut)
            Directory.CreateDirectory(cartella)

            For Each nome As String In nomi
                File.WriteAllText(Path.Combine(cartella, nome), $"finto: {nome}")
            Next

        End Sub

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

        Private Shared Function Casella(pannello As Control, nome As String) As TextBox
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), TextBox)
        End Function

        Private Shared Function Etichetta(pannello As Control, nome As String) As Label
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

        Private Shared Function Bottone(pannello As Control, nome As String) As Button
            Return DirectCast(pannello.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Allegati(pannello As Control) As CheckedListBox
            Return DirectCast(pannello.Controls.Find("lstAllegati", searchAllChildren:=True).Single(), CheckedListBox)
        End Function

        Private Shared Function SpuntatoQuello(elenco As CheckedListBox, nome As String) As Boolean

            For indice As Integer = 0 To elenco.Items.Count - 1
                If CStr(elenco.Items(indice)) = nome Then Return elenco.GetItemChecked(indice)
            Next

            Return False

        End Function

    End Class

End Namespace
