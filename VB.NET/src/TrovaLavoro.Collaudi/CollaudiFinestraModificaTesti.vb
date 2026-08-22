Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi della modifica a mano dei testi di P6 (T9d, cap. 08.4). Quello che qui
    ''' può rompersi davvero è dove finisce il testo: un campo riscritto che entrasse nella
    ''' voce sbagliata — o che entrasse nel documento pur avendo l'utente annullato —
    ''' sarebbe una bugia scritta nel file che poi si spedisce.
    ''' </summary>
    ''' <remarks>
    ''' Come le altre finestre modali, si costruisce e si interroga <b>senza mostrarla</b>:
    ''' per questo riscrivere, ripristinare e applicare hanno un metodo ciascuno, che è poi
    ''' quello che i controlli chiamano.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraModificaTesti

        Private Shared Function Cv() As JsonNode

            Return JsonNode.Parse(
                "{""tipo"": ""cv_mirato""," &
                """intestazione"": {""nome"": ""Luca Ferrari""}," &
                """sommario"": ""Ho esperienza nel servizio di sala.""," &
                """esperienze_professionali"": [" &
                "  {""ruolo"": ""Cameriere"", ""azienda"": ""Trattoria Da Gino""," &
                "   ""descrizione"": ""Servizio ai tavoli""}," &
                "  {""ruolo"": ""Magazziniere"", ""azienda"": ""Rossi S.p.A.""," &
                "   ""descrizione"": ""Carico e scarico merci""}]," &
                """competenze"": [""HACCP""]}")

        End Function

        Private Shared Function Lettera() As JsonNode

            Return JsonNode.Parse(
                "{""tipo"": ""lettera_mirata""," &
                """apertura"": ""Spettabile Azienda,""," &
                """corpo"": ""Mi candido perché ho esperienza di sala.""," &
                """chiusura"": ""Cordiali saluti,""}")

        End Function

        ''' <summary>I documenti da riscrivere, col «prima» che si vuole dare a ciascuno.</summary>
        Private Shared Function Aperti(ParamArray documenti As JsonNode()) As List(Of DocumentoDaRiscrivere)

            Return documenti.Select(
                Function(d) New DocumentoDaRiscrivere With {.Documento = d}).ToList()

        End Function

        Private Shared Function Descrizione(documento As JsonNode, indice As Integer) As String
            Return documento("esperienze_professionali")(indice)("descrizione").GetValue(Of String)()
        End Function

        Private Shared Function Testo(documento As JsonNode, campo As String) As String
            Return documento(campo).GetValue(Of String)()
        End Function

        Private Shared Function Elenco(finestra As Control) As ListView
            Return DirectCast(finestra.Controls.Find("lvwCampi", searchAllChildren:=True).Single(), ListView)
        End Function

        <TestMethod>
        Public Sub IDueDocumentiDiUnaCandidaturaStannoInUnElencoSolo()

            ' Il CV e la lettera si riscrivono nello stesso posto: l'etichetta dice già di
            ' quale documento è ogni campo, e due finestre per un gesto solo sarebbero due
            ' conferme da dare.
            Using finestra As New FinestraModificaTesti(Aperti(Cv(), Lettera()))

                Assert.AreEqual(4, finestra.Quanti, "sommario, due esperienze e il corpo")

                Assert.AreEqual("Sommario", finestra.Etichetta(0), "prima il CV")
                Assert.AreEqual("Esperienza 1", finestra.Etichetta(1))
                Assert.AreEqual("Esperienza 2", finestra.Etichetta(2))
                Assert.AreEqual("Corpo della lettera", finestra.Etichetta(3), "poi la lettera")

                Assert.HasCount(4, Elenco(finestra).Items, "e l'elenco a video li mostra tutti")

            End Using

        End Sub

        <TestMethod>
        Public Sub RiscrivereNonToccaIlDocumentoFinoAlSalva()

            ' È la promessa dell'«Annulla»: quello che si è scritto muore con la finestra.
            Dim documento As JsonNode = Cv()

            Using finestra As New FinestraModificaTesti(Aperti(documento))

                Assert.IsTrue(finestra.Riscrivi(0, "L'ho riscritto io."), "riscritto in finestra")
                Assert.AreEqual("L'ho riscritto io.", finestra.Testo(0), "e la finestra lo mostra")

                Assert.AreEqual("Ho esperienza nel servizio di sala.", Testo(documento, "sommario"),
                                "ma il documento è ancora quello di prima")

            End Using

        End Sub

        <TestMethod>
        Public Sub ApplicaMetteNelDocumentoSoloICampiCambiati()

            Dim documento As JsonNode = Cv()

            Using finestra As New FinestraModificaTesti(Aperti(documento))

                finestra.Riscrivi(2, "L'ho riscritta io.")

                Assert.AreEqual(1, finestra.Applica(), "uno solo è stato toccato")

                Assert.AreEqual("L'ho riscritta io.", Descrizione(documento, 1), "ed è finito nella voce giusta")
                Assert.AreEqual("Servizio ai tavoli", Descrizione(documento, 0), "l'altra esperienza è intatta")
                Assert.AreEqual("Ho esperienza nel servizio di sala.", Testo(documento, "sommario"),
                                "e il sommario pure")

            End Using

        End Sub

        <TestMethod>
        Public Sub RiscrivereConLoStessoTestoNonContaComeUnaModifica()

            ' Chi apre, guarda e chiude senza cambiare niente non ha modificato niente: un
            ' documento «modificato» da nessuno si farebbe risalvare a ogni visita.
            Dim documento As JsonNode = Cv()

            Using finestra As New FinestraModificaTesti(Aperti(documento))

                finestra.Riscrivi(0, "Ho esperienza nel servizio di sala.")

                Assert.AreEqual(0, finestra.Applica(), "niente da scrivere")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnaCasellaSvuotataNonCancellaIlTesto()

            Dim documento As JsonNode = Cv()

            Using finestra As New FinestraModificaTesti(Aperti(documento))

                Assert.IsFalse(finestra.Riscrivi(0, "   "), "il vuoto si rifiuta")
                Assert.AreEqual("Ho esperienza nel servizio di sala.", finestra.Testo(0),
                                "e nella finestra resta quello che c'era")

                Assert.AreEqual(0, finestra.Applica(), "niente è cambiato")

            End Using

        End Sub

        <TestMethod>
        Public Sub SiTornaAlTestoNonRifinitoSoloDoveLaRifinituraEPassata()

            ' È la terza cosa promessa dal cap. 08.4 — accettare, riscrivere, tornare
            ' indietro — e vale sui soli campi che la rifinitura ha davvero cambiato: sugli
            ' altri non c'è nessun «prima» da rimettere.
            Dim documento As JsonNode = Cv()

            Dim aperti As New List(Of DocumentoDaRiscrivere) From {
                New DocumentoDaRiscrivere With {
                    .Documento = documento,
                    .PrimaDellaRifinitura = JsonNode.Parse("{""sommario"": ""Ho esperienza di sala.""}")}}

            Using finestra As New FinestraModificaTesti(aperti)

                Assert.IsTrue(finestra.SiPuoRipristinare(0), "il sommario era stato rifinito")
                Assert.IsFalse(finestra.SiPuoRipristinare(1), "la descrizione no")
                Assert.IsFalse(finestra.Ripristina(1), "e infatti non si ripristina")

                Assert.IsTrue(finestra.Ripristina(0), "il sommario sì")
                Assert.AreEqual("Ho esperienza di sala.", finestra.Testo(0), "col testo da cui l'AI era partita")

                Assert.AreEqual(1, finestra.Applica(), "e ripristinare è riscrivere: entra nel documento")
                Assert.AreEqual("Ho esperienza di sala.", Testo(documento, "sommario"))

            End Using

        End Sub

        <TestMethod>
        Public Sub UnDocumentoSenzaProsaNonPortaRighe()

            ' Un CV tutto fatti — nome, competenze, titoli — non ha niente da riscrivere: la
            ' finestra non lo nega, semplicemente non ha campi da mostrare, ed è il pannello
            ' a non aprirla affatto.
            Dim soloFatti As JsonNode = JsonNode.Parse(
                "{""tipo"": ""cv_base"", ""intestazione"": {""nome"": ""Luca Ferrari""}," &
                """competenze"": [""HACCP""]}")

            Using finestra As New FinestraModificaTesti(Aperti(soloFatti))

                Assert.AreEqual(0, finestra.Quanti, "nessun campo di prosa")
                Assert.AreEqual(0, finestra.Applica(), "e niente da applicare")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnaRigaCheNonCEsisteNonFaDanno()

            Using finestra As New FinestraModificaTesti(Aperti(Cv()))

                Assert.IsFalse(finestra.Riscrivi(-1, "Prima della prima."), "prima del primo")
                Assert.IsFalse(finestra.Riscrivi(9, "Dopo l'ultima."), "dopo l'ultimo")
                Assert.IsFalse(finestra.SiPuoRipristinare(9), "e non si ripristina il nulla")
                Assert.AreEqual(String.Empty, finestra.Etichetta(9), "né si legge il nome di una riga che non c'è")

            End Using

        End Sub

    End Class

End Namespace
