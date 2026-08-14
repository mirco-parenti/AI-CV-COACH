Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
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

        Private Const EmailScritta As String =
            "{""tipo"": ""email_candidatura"", ""oggetto"": ""Candidatura per Magazziniere — Luca Ferrari""," &
            """corpo"": ""Spettabile Azienda,\nmi candido per la posizione.\nCordiali saluti,\nLuca Ferrari""}"

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
                    Assert.IsEmpty(Casella(pannello, "txtDestinatario").Text)
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
                    Bottone(pannello, "btnPreparaEmail").PerformClick()

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
                    Bottone(pannello, "btnPreparaEmail").PerformClick()

                    Dim riletta As Opportunita = contesto.Opportunita.Carica(candidatura.Cartella)
                    Dim bozza As BozzaEmail = BozzaEmail.DaJson(riletta.Email)

                    Assert.IsNotNull(bozza, "la bozza è su disco")
                    Assert.AreEqual("lavoro@rossi.it", bozza.Destinatario)
                    Assert.Contains("mi candido per la posizione", bozza.Corpo)
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
        Public Async Function IlMessaggioGiaScrittoNonSiAllegaASeStesso() As Task

            Dim compositore As New CompositoreFinto
            compositore.Dara(EmailScritta).Dara(EmailScritta)

            Await ConPannelloAsync(compositore,
                Async Function(pannello, contesto, candidatura)
                    ScriviDocumenti(candidatura, "CV_Luca_Rossi.pdf")

                    Await pannello.MostraLaCandidaturaAsync(candidatura)
                    Bottone(pannello, "btnPreparaEmail").PerformClick()

                    ' Si rientra: adesso nella cartella c'è anche il .eml appena scritto.
                    Await pannello.MostraLaCandidaturaAsync(candidatura)

                    For Each voce As String In Allegati(pannello).Items.Cast(Of String)()
                        Assert.DoesNotEndWith(".eml", voce, "un messaggio dentro il messaggio no")
                    Next
                End Function)

        End Function

        ' ==================================================================
        ' Il banco
        ' ==================================================================

        Private Shared Async Function ConPannelloAsync(
                compositore As CompositoreFinto,
                prova As Func(Of PannelloEmail, ContestoApp, Opportunita, Task)) As Task

            Dim radice As String = Path.Combine(
                Path.GetTempPath(), "pannello-email-" & Guid.NewGuid().ToString("N"))

            Try
                Using contesto As ContestoApp = ContestoApp.Monta(radice, "", PoolInesistente()),
                      pannello As New PannelloEmail()

                    contesto.Archivio.Salva(TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()))

                    pannello.CreateControl()
                    pannello.Collega(contesto, compositore)

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
