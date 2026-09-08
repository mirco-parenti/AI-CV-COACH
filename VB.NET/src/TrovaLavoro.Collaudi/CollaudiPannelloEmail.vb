Imports System.Drawing
Imports System.IO
Imports System.IO.Compression
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

        ''' <summary>Un 📄 CV base come lo scrive l'AI: quel che conta è che abbia un nome.</summary>
        Private Const CvBaseScritto As String =
            "{""tipo"": ""cv_base"", ""intestazione"": {""nome"": ""Luca Ferrari"", ""citta"": ""Forlì""}," &
            """sommario"": ""Il ritratto del profilo."", ""competenze"": [""Uso del muletto""]}"

        ''' <summary>
        ''' Lo stesso 📄 CV base, ma con due competenze: una si lascia fuori e l'altra
        ''' resta, che è l'unico modo di vedere se il taglio arriva fino all'allegato (R6).
        ''' </summary>
        Private Const CvBaseConDueVoci As String =
            "{""tipo"": ""cv_base"", ""intestazione"": {""nome"": ""Luca Ferrari"", ""citta"": ""Forlì""}," &
            """sommario"": ""Il ritratto del profilo.""," &
            """competenze"": [""Uso del muletto"", ""Gestione del magazzino""]}"

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
        Public Async Function AppenaScrittoDallAiRiscrivereNonChiedeNiente() As Task

            ' Il costo di un sì, qui, è un'attesa e nient'altro: un testo appena arrivato
            ' dal compositore si rifà premendo di nuovo. Una domanda che si fa comunque è
            ' una domanda a cui si risponde senza leggerla.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.IsEmpty(pannello.AncheQuelloCheHaiScrittoAMano(),
                                   "in casella c'è solo roba dell'AI: niente da difendere")
                End Function)

        End Function

        <TestMethod>
        Public Async Function RiscrivereDiceQualiTestiCorrettiAManoSpariscono() As Task

            ' Cap. 03.3, livello 4: «Fallo riscrivere» sostituisce oggetto e corpo, e da
            ' quando l'utente ci mette mano quello che sparisce non è più un'attesa, è
            ' lavoro suo. La riga li nomina uno per uno, come fa «Rigenera» in P6.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Casella(pannello, "txtCorpo").Text = "Il testo come lo voglio io."

                    Dim detto As String = pannello.AncheQuelloCheHaiScrittoAMano()

                    Assert.Contains("il testo del messaggio", detto, "il corpo è stato riscritto a mano")
                    Assert.DoesNotContain("l'oggetto", detto, "l'oggetto invece no, ed è ancora dell'AI")

                    Casella(pannello, "txtOggetto").Text = "Candidatura magazziniere"

                    Assert.Contains("l'oggetto", pannello.AncheQuelloCheHaiScrittoAMano(),
                                    "adesso sono tutti e due")
                End Function)

        End Function

        <TestMethod>
        Public Async Function DiUnaBozzaRipresaDalDiscoNonSiSaESiDice() As Task

            ' email.json tiene i testi, non la loro storia: un messaggio limato ieri sera
            ' e uno mai toccato tornano identici. Nel dubbio fra due livelli si sceglie il
            ' più alto (cap. 03.3), e si chiede — dicendo però la cosa vera, cioè che a
            ' saperlo è solo chi legge.
            Dim compositore As New CompositoreFinto

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    candidatura.Email = JsonNode.Parse(
                        "{""destinatario"": ""lavoro@rossi.it"", ""oggetto"": ""Il mio oggetto""," &
                        """corpo"": ""Il testo che ho corretto a mano."", ""allegati"": []}")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim detto As String = pannello.AncheQuelloCheHaiScrittoAMano()

                    Assert.IsNotEmpty(detto, "una bozza ripresa non si riscrive senza chiedere")
                    Assert.Contains("se ci avevi messo mano", detto,
                                    "e non si finge di sapere quel che non si sa")
                End Function)

        End Function

        <TestMethod>
        Public Async Function UnaCandidaturaNuovaNonEreditaISospettiDiQuellaDiPrima() As Task

            ' Il pannello si riusa da una candidatura all'altra: il lavoro a mano sul
            ' messaggio di prima non è lavoro a mano su questo, e portarselo dietro
            ' vorrebbe dire una domanda che nessuno sa più a cosa si riferisca.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Casella(pannello, "txtCorpo").Text = "Il testo come lo voglio io."
                    Assert.IsNotEmpty(pannello.AncheQuelloCheHaiScrittoAMano(), "su questa sì")

                    Dim laSeconda As New Opportunita With {
                        .Annuncio = JsonNode.Parse(AnnuncioLetto),
                        .Lettera = JsonNode.Parse(Lettera),
                        .Creata = New Date(2026, 8, 11)}
                    laSeconda.Avanza(StatoOpportunita.Interessante, laSeconda.Creata)
                    laSeconda.Avanza(StatoOpportunita.Generata, laSeconda.Creata)
                    contesto.Opportunita.Salva(laSeconda)

                    Await pannello.MostraLaCandidaturaAsync(laSeconda)

                    Assert.IsEmpty(pannello.AncheQuelloCheHaiScrittoAMano(),
                                   "sull'altra no: il suo messaggio l'ha appena scritto l'AI")
                End Function)

        End Function

        <TestMethod>
        Public Sub RiscrivereEUnAzioneDiLivelloQuattro()

            ' Sostituisce testi già scritti, esattamente come «Rigenera» in P6: da
            ' esplorativo prometteva un'anteprima e invece cancellava (cap. 03.3).
            Using pannello As New PannelloEmail()
                Assert.AreEqual(LivelloBottone.Attenzione, Bottone(pannello, "btnRiscrivi").Tag)
            End Using

        End Sub

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

        ' ==================================================================
        ' La candidatura eliminata dalla Home (cap. 11.5)
        ' ==================================================================

        <TestMethod>
        Public Async Function LEmailLasciaAndareLaCandidaturaEliminata() As Task

            ' Salvare la bozza scrive l'email.json nella cartella della candidatura: una
            ' bozza sopravvissuta alla sua cartella la ricreerebbe.
            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim dove As String = candidatura.Cartella

                    Assert.IsFalse(pannello.Dimentica(dove & "-di-un-altra"),
                                   "una candidatura che non è la sua non la riguarda")
                    Assert.AreNotEqual(String.Empty, Casella(pannello, "txtCorpo").Text,
                                       "e infatti la bozza è ancora lì")

                    contesto.Opportunita.Elimina(dove)

                    Assert.IsTrue(pannello.Dimentica(dove), "questa invece era proprio la sua")
                    Assert.AreEqual(String.Empty, Casella(pannello, "txtCorpo").Text,
                                    "la bozza sparisce con lei")
                    Assert.IsFalse(Bottone(pannello, "btnRiscrivi").Enabled,
                                   "e non c'è più niente da riscrivere")
                End Function)

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
                    ' Senza stampante PDF, come in CollaudiPannelloDocumenti: qui si guarda
                    ' il pannello, e il PDF ha il suo banco a parte.
                    pannello.Collega(contesto, compositore, Nothing,
                                     If(rifinitore Is Nothing, Nothing, New Rifinitura(rifinitore)),
                                     New ArchivioDocumenti(contesto.Cartella))

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

        ''' <summary>
        ''' Mette nella <c>out\</c> del <b>profilo</b> dei finti file di 📄 CV base già
        ''' esportati: è l'altra cartella da cui l'email pesca, e non è di nessuna
        ''' candidatura (cap. 11.1).
        ''' </summary>
        Private Shared Sub ScriviCvBaseEsportato(contesto As ContestoApp, ParamArray nomi As String())

            Dim cartella As String = contesto.Cartella.CartellaOutProfilo
            Directory.CreateDirectory(cartella)

            For Each nome As String In nomi
                File.WriteAllText(Path.Combine(cartella, nome), $"finto: {nome}")
            Next

        End Sub

        ''' <summary>
        ''' Il 📄 CV base mai esportato compare lo stesso, dicendo che il file non c'è
        ''' ancora; spuntandolo lo si scrive davvero (2026-09-08).
        ''' </summary>
        ''' <remarks>
        ''' <para>È la seconda metà di «sempre allegabile», e senza di lei la prima è una
        ''' mezza promessa: un CV base esiste appena l'AI lo scrive, mentre i <b>file</b>
        ''' nascono solo se qualcuno preme «Esporta» in P6 — cioè quasi mai, se uno arriva
        ''' qui dalla Home. La voce c'è comunque e lo dichiara; il file lo scrive la spunta,
        ''' senza chiamare l'AI: è impaginazione, non scrittura.</para>
        ''' <para>Nel banco la stampante PDF non c'è (vuole una WebView e il thread
        ''' dell'interfaccia), quindi nasce il solo DOCX — ed è la ragione per cui la spunta
        ''' finisce su quel che è <b>davvero</b> nato invece che sul nome promesso: una
        ''' spunta sul file mancante allegherebbe il nulla, in silenzio.</para>
        ''' </remarks>
        <TestMethod>
        Public Async Function IlCvBaseMaiEsportatoSiPrometteEPoiSiScrive() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ' Un 📄 CV base c'è, ma nessuno l'ha mai esportato: su disco c'è il solo
                    ' cv_base.json, e la sua cartella out\ non esiste nemmeno.
                    contesto.Archivio.SalvaCvBase(JsonNode.Parse(CvBaseScritto),
                                                  contesto.Archivio.Versioni().Last())

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim elenco As CheckedListBox = Allegati(pannello)
                    Dim promessa As String = RigheDi(elenco).
                        FirstOrDefault(Function(v) v.Contains("CV base"))

                    Assert.IsNotNull(promessa, "la voce c'è anche se il file no")
                    Assert.Contains("lo scrivo quando lo spunti", promessa,
                                    "e dice che il file non c'è ancora, invece di far credere che sia lì")
                    Assert.IsFalse(SpuntatoQuello(elenco, promessa), "spenta, come tutte le sue")

                    Await pannello.AllegaIlCvBaseAsync()

                    Dim nati As String() = Directory.GetFiles(contesto.Cartella.CartellaOutProfilo)
                    Assert.IsNotEmpty(nati, "adesso il file c'è davvero")

                    Dim riga As String = RigheDi(elenco).
                        FirstOrDefault(Function(v) v.StartsWith(Path.GetFileName(nati(0))))

                    Assert.IsNotNull(riga, "l'elenco mostra il file nato, col suo nome vero")
                    Assert.DoesNotContain("lo scrivo quando", riga, "che non è più una promessa")
                    Assert.IsTrue(SpuntatoQuello(elenco, riga),
                                  "ed è spuntato: l'utente l'ha chiesto spuntandolo")
                End Function)

        End Function

        ''' <summary>
        ''' Il 📄 CV base scritto per essere allegato non riporta le voci lasciate fuori
        ''' (R6, 2026-09-08).
        ''' </summary>
        ''' <remarks>
        ''' Qui il documento nasce da <see cref="PannelloEmail.AllegaIlCvBaseAsync"/>, che
        ''' impagina il CV salvato accanto al profilo — e il taglio dell'utente viaggia con
        ''' lui, nel <c>cv_base.json</c>, perché in questo pannello P6 non c'è a ricordarlo.
        ''' È la copia peggiore in cui sbagliarsi: è l'unica che esce di casa.
        ''' </remarks>
        <TestMethod>
        Public Async Function IlCvBaseAllegatoNonRiportaLeVociLasciateFuori() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    Dim fuori As New VociTolte()
                    fuori.Togli("competenze¦gestione del magazzino", Date.Now)

                    contesto.Archivio.SalvaCvBase(JsonNode.Parse(CvBaseConDueVoci),
                                                  contesto.Archivio.Versioni().Last(),
                                                  "it", Nothing, Nothing, fuori)

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Await pannello.AllegaIlCvBaseAsync()

                    Dim nati As String() = Directory.GetFiles(
                        contesto.Cartella.CartellaOutProfilo, "*.docx")
                    Assert.HasCount(1, nati, "il 📄 CV base è nato per essere allegato")

                    Dim dentro As String = TestoDelDocx(nati(0))
                    Assert.Contains("Uso del muletto", dentro, "la voce tenuta c'è")
                    Assert.DoesNotContain("Gestione del magazzino", dentro,
                                          "e quella lasciata fuori non è tornata dentro")
                End Function)

        End Function

        ''' <summary>
        ''' Aprendo un documento in P6, l'email lo segue — <b>senza</b> far scrivere niente
        ''' all'AI (2026-09-08).
        ''' </summary>
        ''' <remarks>
        ''' <para>È la metà delicata della richiesta di Mirco. Che l'email segua il documento
        ''' è comodo; che lo faccia chiamando il compositore sarebbe un disastro: cambiare
        ''' voce in una tendina spenderebbe una chiamata a ogni giro, per un messaggio che
        ''' nessuno ha chiesto. Perciò questo pannello, quando si allinea, riprende la bozza
        ''' salvata se c'è e altrimenti resta vuoto dicendolo — il messaggio si scrive con
        ''' «Fallo riscrivere», che è un gesto.</para>
        ''' <para>Sul 📄 CV base si svuota: quel CV non si manda a nessuno, perché non nasce
        ''' da un annuncio e non ha un'azienda a cui andare.</para>
        ''' </remarks>
        <TestMethod>
        Public Async Function LEmailSegueIlDocumentoSenzaChiamareLAi() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Ferrari_Rossi.pdf")

                    Await pannello.SegueIlDocumentoAsync(candidatura)

                    Assert.AreSame(candidatura, pannello.Candidatura,
                                   "l'email è di quella candidatura")
                    Assert.IsEmpty(compositore.AllegatiNominati,
                                   "e nessuno ha chiesto all'AI di scrivere: allinearsi non è chiedere")
                    Assert.IsEmpty(Casella(pannello, "txtCorpo").Text, "il messaggio non c'è ancora")
                    Assert.Contains("Fallo riscrivere", Etichetta(pannello, "lblStatoEmail").Text,
                                    "e la riga dice qual è il gesto che lo scrive")

                    ' In P6 si passa al 📄 CV base: non c'è nessuna azienda a cui mandarlo.
                    Await pannello.SegueIlDocumentoAsync(Nothing)

                    Assert.IsNull(pannello.Candidatura, "il pannello ha lasciato andare la candidatura")
                    Assert.IsEmpty(Allegati(pannello).Items, "e con lei i suoi allegati")
                    Assert.Contains("non si manda a nessuno", Etichetta(pannello, "lblStatoEmail").Text,
                                    "detto con le parole del perché")
                End Function)

        End Function

        ''' <summary>Le righe dell'elenco degli allegati, come si leggono.</summary>
        Private Shared Function RigheDi(elenco As CheckedListBox) As List(Of String)

            Return elenco.Items.Cast(Of Object)().Select(Function(v) CStr(v)).ToList()

        End Function

        ''' <summary>
        ''' Il 📄 CV base si può allegare all'email, sempre: anche se non appartiene a
        ''' questa candidatura e sta in un'altra cartella (2026-09-08).
        ''' </summary>
        ''' <remarks>
        ''' <para>L'elenco «Cosa allego» guardava due posti — la <c>out\</c> della
        ''' candidatura e la cartella documenti — e il 📄 CV base non sta né nell'una né
        ''' nell'altra: vive accanto al <b>profilo</b>, perché non è di nessuna candidatura
        ''' (cap. 11.1). Il risultato era che un CV base esportato non compariva da nessuna
        ''' parte, e per mandarlo bisognava allegarlo a mano dal programma di posta.</para>
        ''' <para>Arriva <b>spento</b>, come gli attestati: su una candidatura il PDF del
        ''' 🎯 CV mirato è già spuntato, e due CV nella stessa email si annullano a vicenda.
        ''' Il programma lo mette a portata di mano; a sceglierlo è chi si candida.</para>
        ''' </remarks>
        <TestMethod>
        Public Async Function IlCvBaseSiPuoSempreAllegare() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Ferrari_Rossi.pdf")
                    ScriviCvBaseEsportato(contesto, "CV_Ferrari_2026-09-08.pdf")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim elenco As CheckedListBox = Allegati(pannello)
                    Dim righe As List(Of String) =
                        elenco.Items.Cast(Of Object)().Select(Function(v) CStr(v)).ToList()

                    Dim riga As String = righe.FirstOrDefault(
                        Function(v) v.StartsWith("CV_Ferrari_2026-09-08.pdf"))

                    Assert.IsNotNull(riga, "il 📄 CV base è fra le cose che si possono allegare")
                    Assert.Contains("CV base", riga,
                                    "e la riga dice di chi è: non è un documento di questa candidatura")
                    Assert.IsFalse(SpuntatoQuello(elenco, riga),
                                   "arriva spento: il 🎯 CV mirato è già spuntato, e due CV si annullano")
                    Assert.IsTrue(SpuntatoQuello(elenco, "CV_Ferrari_Rossi.pdf"),
                                  "e non ha rubato la spunta al CV di questa candidatura")
                End Function)

        End Function

        <TestMethod>
        Public Sub IlTastoTabPercorreLaFasciaComeLaLeggeLOcchio()

            ' Fino al 2026-09-01 «Documenti da allegare…» e «L'ho spedita» portavano lo
            ' stesso TabIndex: con due indici uguali a decidere è l'ordine in cui i
            ' controlli sono stati aggiunti, che non si vede guardando la fascia — e chi
            ' percorre l'applicazione col Tab si ritrova dall'altra parte dello schermo.
            Using pannello As New PannelloEmail()

                pannello.Width = 1890
                pannello.ImpostaIngombroLogo(New Size(261, 188))

                Dim fascia As Panel = DirectCast(
                    pannello.Controls.Find("pnlAzioni", searchAllChildren:=True).Single(), Panel)

                Dim inFila As List(Of Button) = fascia.Controls.OfType(Of Button)().
                    OrderBy(Function(b) b.Top).ThenBy(Function(b) b.Left).ToList()

                Assert.HasCount(5, inFila, "i cinque comandi della fascia")

                For posto As Integer = 1 To inFila.Count - 1
                    Assert.IsGreaterThan(inFila(posto - 1).TabIndex, inFila(posto).TabIndex,
                                         $"«{inFila(posto).Text}» viene col Tab dopo «{inFila(posto - 1).Text}»")
                Next

            End Using

        End Sub

        ''' <summary>
        ''' La spia sopra il <b>messaggio</b>: il testo nasce dalla ✉️ lettera, e la spia
        ''' dice se quella lettera viene dal profilo di oggi (cap. 03.8).
        ''' </summary>
        ''' <remarks>
        ''' <para>Era l'ultima schermata a non avere nessuna spia. Qui i documenti non si
        ''' guardano, si <b>consegnano</b>: chi arriva dalla Home a riprendere una bozza di
        ''' ieri in P6 non passa affatto, e l'avviso che sta di là per lui non esiste.</para>
        ''' <para>Il collaudo prova i tre stati e la <b>frase</b> del suggerimento, che qui
        ''' non può nominare «Rigenera» — quel bottone in questa schermata non c'è — e deve
        ''' dire i due gesti veri, nel loro ordine: rigenerare la lettera di là, e solo poi
        ''' far riscrivere il messaggio di qua.</para>
        ''' </remarks>
        <TestMethod>
        Public Async Function LEmailDiceSeIlTestoNasceDaUnaLetteraDelProfiloDiOggi() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta).Dara(EmailScritta).Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ' Una candidatura scritta prima del 2026-09-03 non ha nessuna versione
                    ' annotata sui documenti: la spia resta spenta, che non è «in pari».
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim spia As Label = Etichetta(pannello, "lblSpiaCorpo")
                    Assert.IsEmpty(spia.Text, "senza sapere da dove viene, non si promette niente")

                    candidatura.VersioneDeiDocumenti = contesto.Archivio.Versioni().Last()
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.Contains(SpiaDelProfilo.ParolaAllineato, spia.Text,
                                    "la lettera è nata dal profilo di adesso")

                    ' Il profilo cambia sotto la lettera già scritta: da qui in avanti il
                    ' messaggio racconta qualcun altro, e riscriverlo non lo rimette in pari.
                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.Contains(SpiaDelProfilo.ParolaDisallineato, spia.Text,
                                    "e la lucina lo dice prima che il messaggio parta")

                    Dim detto As String = SuggerimentiDelPannello(pannello).GetToolTip(spia)
                    Assert.Contains("Torna ai documenti", detto, "il gesto che in questa schermata c'è")
                    Assert.DoesNotContain("«Rigenera»", detto, "e non uno che qui non esiste")
                    Assert.Contains("Fallo riscrivere", detto,
                                    "e il secondo: rigenerata la lettera, il messaggio va rifatto")
                End Function)

        End Function

        ''' <summary>
        ''' La spia sopra gli <b>allegati</b> parla dei file che stanno nell'elenco, non dei
        ''' documenti che la candidatura ha in pancia.
        ''' </summary>
        ''' <remarks>
        ''' <para>È il difetto trovato guardando il programma la sera del 2026-09-08, con la
        ''' spia nata quel giorno stesso. Si accendeva chiedendo <i>«questa candidatura ha un
        ''' CV o una lettera?»</i> — un fatto che con l'elenco lì sotto non c'entrava — e su
        ''' una candidatura i cui documenti non erano mai stati esportati diventava rossa
        ''' sopra due file scritti quel giorno, dicendo di riesportare roba che lì non
        ''' c'era.</para>
        ''' <para>Le due metà del collaudo hanno lo stesso identico stato del profilo: quel
        ''' che cambia è solo se in <c>out\</c> c'è un file. Contano tutt'e due, perché il
        ''' rosso è facile da far comparire e la cosa difficile è che <b>non</b> compaia
        ''' quando non deve.</para>
        ''' </remarks>
        <TestMethod>
        Public Async Function LaSpiaDegliAllegatiGuardaLElencoNonLaCandidatura() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta).Dara(EmailScritta).Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ' In elenco c'è un 📄 CV base già esportato — che dal profilo di oggi
                    ' viene per definizione — e nient'altro. Il 🎯 CV e la ✉️ lettera della
                    ' candidatura ci sono, ma come JSON: nessuno li ha mai esportati.
                    ScriviCvBaseEsportato(contesto, "CV_base.pdf")
                    candidatura.VersioneDeiDocumenti = contesto.Archivio.Versioni().Last()
                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))

                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Dim spiaAllegati As Label = Etichetta(pannello, "lblSpiaDocumenti")
                    Dim spiaTesto As Label = Etichetta(pannello, "lblSpiaCorpo")

                    ' Senza questa riga l'asserto qui sotto sarebbe verde anche su un
                    ' elenco vuoto, cioè per il motivo sbagliato.
                    Assert.IsNotEmpty(Allegati(pannello).Items, "in elenco c'è il CV base")

                    Assert.IsEmpty(spiaAllegati.Text,
                                   "di quei due file non ne parte nessuno: non c'è niente da giudicare")
                    Assert.Contains(SpiaDelProfilo.ParolaDisallineato, spiaTesto.Text,
                                    "mentre il testo viene dalla lettera vecchia, e quello va detto")

                    ' Adesso il 🎯 CV mirato è stato esportato davvero: da qui in poi fra gli
                    ' allegati c'è un file che non viene dal profilo di oggi.
                    ScriviDocumenti(candidatura, "CV_mirato.pdf")
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    Assert.Contains(SpiaDelProfilo.ParolaDisallineato, spiaAllegati.Text,
                                    "adesso sì: uno di quei file sta per partire")

                    Dim detto As String = SuggerimentiDelPannello(pannello).GetToolTip(spiaAllegati)
                    Assert.Contains("Torna ai documenti", detto, "il gesto che in questa schermata c'è")
                    Assert.DoesNotContain("«Rigenera»", detto, "e non uno che qui non esiste")
                    Assert.Contains("riesporta", detto,
                                    "gli allegati sono i file scritti: vanno rifatti anche quelli")
                End Function)

        End Function

        ''' <summary>
        ''' Anche la spia del messaggio si è presa una riga sua, fra «Il messaggio» e la
        ''' casella, senza rubarla a nessuno.
        ''' </summary>
        ''' <remarks>
        ''' Stessa misura della gemella sugli allegati, e per la stessa ragione: infilare una
        ''' riga in un riquadro a posizioni fisse vuol dire spostare in giù quel che viene
        ''' dopo e accorciarlo dello stesso tanto, o va a finire fuori dal pannello. Qui si
        ''' misura, che costa meno di guardare e non dimentica.
        ''' </remarks>
        <TestMethod>
        Public Sub LaSpiaDelMessaggioHaUnaRigaSuaFraIlTitoloELaCasella()

            Using pannello As New PannelloEmail()

                Dim titolo As Label = Etichetta(pannello, "lblCorpo")
                Dim spia As Label = Etichetta(pannello, "lblSpiaCorpo")
                Dim riquadro As TextBox = Casella(pannello, "txtCorpo")

                Assert.IsGreaterThanOrEqualTo(titolo.Bottom, spia.Top, "la spia sta sotto «Il messaggio»")
                Assert.IsGreaterThanOrEqualTo(spia.Bottom, riquadro.Top, "e sopra la casella, senza accavallarsi")
                Assert.AreEqual(titolo.Left, spia.Left, "incolonnata col titolo di cui continua la riga")

            End Using

        End Sub

        ''' <summary>
        ''' La spia si è presa una riga sua fra «Cosa allego» e l'elenco, e non l'ha rubata
        ''' a nessuno.
        ''' </summary>
        ''' <remarks>
        ''' Il riquadro degli allegati è a posizioni fisse: infilarci una riga vuol dire
        ''' spostare l'elenco in giù e accorciarlo dello stesso tanto, o va a finire sotto
        ''' la nota in fondo — che è il genere di cosa che a video si vede solo se la lista
        ''' è piena. Qui si misura, che costa meno di guardare e non dimentica.
        ''' </remarks>
        <TestMethod>
        Public Sub LaSpiaHaUnaRigaSuaFraIlTitoloELElenco()

            Using pannello As New PannelloEmail()

                Dim titolo As Label = Etichetta(pannello, "lblAllegati")
                Dim spia As Label = Etichetta(pannello, "lblSpiaDocumenti")
                Dim elenco As CheckedListBox = Allegati(pannello)
                Dim nota As Label = Etichetta(pannello, "lblNotaAllegati")

                Assert.IsGreaterThanOrEqualTo(titolo.Bottom, spia.Top, "la spia sta sotto «Cosa allego»")
                Assert.IsGreaterThanOrEqualTo(spia.Bottom, elenco.Top, "e sopra l'elenco, senza accavallarsi")
                Assert.IsGreaterThanOrEqualTo(elenco.Bottom, nota.Top, "e l'elenco resta sopra la nota in fondo")
                Assert.AreEqual(titolo.Left, spia.Left, "incolonnata col titolo di cui continua la riga")

            End Using

        End Sub

        ''' <summary>
        ''' Il fornitore di suggerimenti del pannello: non è un controllo e non si trova con
        ''' <c>Controls.Find</c>, si arriva solo al campo che lo tiene (come in
        ''' <c>CollaudiPannelloDocumenti</c>).
        ''' </summary>
        Private Shared Function SuggerimentiDelPannello(pannello As Control) As ToolTip

            Dim campo As System.Reflection.FieldInfo = pannello.GetType().GetField(
                "_suggerimenti", System.Reflection.BindingFlags.Instance Or
                                 System.Reflection.BindingFlags.NonPublic)

            Assert.IsNotNull(campo, "il pannello ha ancora il suo fornitore di suggerimenti")

            Return DirectCast(campo.GetValue(pannello), ToolTip)

        End Function

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

        ''' <summary>Il testo dentro un <c>.docx</c>: l'XML del documento, tag compresi.</summary>
        ''' <remarks>
        ''' La gemella di quella in <c>CollaudiPannelloDocumenti</c>, e serve alla stessa
        ''' domanda sola: «questa frase c'è dentro?». Sta qui e non in un posto comune per
        ''' la ragione di <see cref="Casella"/> e delle altre — ogni banco di pannello porta
        ''' con sé i suoi attrezzi, e si legge senza saltare altrove.
        ''' </remarks>
        Private Shared Function TestoDelDocx(percorso As String) As String

            Using archivio As ZipArchive = ZipFile.OpenRead(percorso)
                Using lettore As New StreamReader(archivio.GetEntry("word/document.xml").Open())
                    Return lettore.ReadToEnd()
                End Using
            End Using

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
