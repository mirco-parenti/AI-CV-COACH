Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Motore

Namespace Ui

    ''' <summary>
    ''' Collaudi dell'informativa dentro l'applicazione (2026-08-27, dalla revisione del
    ''' giro D).
    ''' </summary>
    ''' <remarks>
    ''' <para>Un'informativa non si collauda come un calcolo: non c'è un risultato giusto.
    ''' Quel che si può difendere è che <b>nomini tutte le porte da cui qualcosa esce</b> —
    ''' l'AI, i portali, GitHub — e che non prometta il contrario di quel che il codice fa.
    ''' Se un giorno il programma comincerà a mandare qualcosa da una porta nuova, questo
    ''' elenco dovrà crescere, e chi lo dimentica trova qui un rosso.</para>
    ''' <para>L'altra promessa è di <b>tempo</b>: compare una volta sola, e il fatto di
    ''' averla vista sopravvive alla chiusura del programma.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiInformativa

        <TestMethod>
        Public Sub NominaTutteLePorteDaCuiQualcosaEsce()

            Dim tutto As String = String.Join(" ", FinestraInformativa.Voci().Select(
                Function(voce) voce.Titolo & " " & voce.Testo))

            StringAssert.Contains(tutto, "Anthropic", "l'AI a cui vanno i testi")
            StringAssert.Contains(tutto, "portali", "i siti che il browser incorporato visita")
            StringAssert.Contains(tutto, "GitHub", "il controllo della versione, che è l'altra uscita")

        End Sub

        <TestMethod>
        Public Sub DiceQuelCheNonEsceEQuelCheNonFa()

            Dim tutto As String = String.Join(" ", FinestraInformativa.Voci().Select(Function(voce) voce.Testo))

            StringAssert.Contains(tutto, "telemetria", "niente statistiche d'uso")
            StringAssert.Contains(tutto, "non si aggiorna da solo", "e nessun aggiornamento silenzioso")
            StringAssert.Contains(tutto, "non parte da qui", "l'email la spedisce l'utente")

        End Sub

        <TestMethod>
        Public Sub DiceCheSiPagaEDoveSiVedeIlConto()

            Dim tutto As String = String.Join(" ", FinestraInformativa.Voci().Select(Function(voce) voce.Testo))

            StringAssert.Contains(tutto, "a consumo", "la chiave si paga")
            StringAssert.Contains(tutto, "Quanto è costato", "e il conto sta in un posto preciso")

        End Sub

        <TestMethod>
        Public Sub NonPromettePrezziCheNonPossoSapere()

            ' La tentazione era scrivere «un giro costa pochi centesimi»: non è un numero
            ' che io conosca — dipende da quanto sono lunghi i testi di chi usa il
            ' programma — e una cifra inventata in un'informativa è precisamente la cosa
            ' che l'informativa esiste per non fare.
            Dim tutto As String = String.Join(" ", FinestraInformativa.Voci().Select(Function(voce) voce.Testo))

            Assert.IsFalse(tutto.Contains("centesim"), "nessuna cifra promessa a naso")
            StringAssert.Contains(tutto, "dipende da quanto sono lunghi", "si dice da cosa dipende")

        End Sub

        <TestMethod>
        Public Sub OgniVoceHaUnTitoloEUnTesto()

            Dim voci As IReadOnlyList(Of VoceInformativa) = FinestraInformativa.Voci()

            Assert.IsTrue(voci.Count >= 5, "l'informativa non è una riga sola")

            For Each voce As VoceInformativa In voci
                Assert.IsFalse(String.IsNullOrWhiteSpace(voce.Titolo), "titolo")
                Assert.IsFalse(String.IsNullOrWhiteSpace(voce.Testo), $"testo di «{voce.Titolo}»")
            Next

        End Sub

        <TestMethod>
        Public Sub LaFinestraMostraTuttiICapitoli()

            Using finestra As New FinestraInformativa()

                Assert.AreEqual(FinestraInformativa.Voci().Count * 2, finestra.Righe.Count,
                                "un'etichetta per il titolo e una per il testo, capitolo per capitolo")

                For Each voce As VoceInformativa In FinestraInformativa.Voci()
                    StringAssert.Contains(finestra.TestoIntero, voce.Titolo, $"il capitolo «{voce.Titolo}»")
                Next

            End Using

        End Sub

        <TestMethod>
        Public Sub QuandoNonCiStaSiScorreInveceDiTagliare()

            Using finestra As New FinestraInformativa()

                finestra.DisponiIn(200)

                Assert.IsTrue(finestra.AutoScroll, "con questo spazio si scorre")
                Assert.IsTrue(finestra.ClientSize.Height <= 200, "e la finestra sta nello spazio che c'è")

            End Using

        End Sub

        ' ==================================================================
        ' «Una volta sola» è una promessa che deve sopravvivere alla chiusura
        ' ==================================================================

        <TestMethod>
        Public Sub LAverlaVistaSiRicordaSuDisco()

            Dim prima As Impostazioni = Impostazioni.Predefinite()
            Assert.IsFalse(prima.InformativaVista, "al primo avvio non l'ha vista nessuno")

            prima.InformativaVista = True

            Dim dopo As Impostazioni = Impostazioni.DaJson(prima.VersoJson().ToJsonString())
            Assert.IsTrue(dopo.InformativaVista, "e riletta dal disco resta vista")

        End Sub

        <TestMethod>
        Public Sub UnValoreStortoNonFaRicomparireLeAltrePreferenze()

            ' La stessa rete degli altri campi: una voce storta si scarta e le altre
            ' valgono (cap. 11.6).
            Dim letto As Impostazioni = Impostazioni.DaJson(
                "{ ""informativa_vista"": ""forse"", ""giorni_follow_up"": 7 }")

            Assert.IsFalse(letto.InformativaVista, "quella storta vale il predefinito")
            Assert.AreEqual(7, letto.GiorniFollowUp, "e l'altra resta buona")
            Assert.IsNotNull(letto.Avviso, "con l'avviso di quel che si è scartato")

        End Sub

    End Class

End Namespace
