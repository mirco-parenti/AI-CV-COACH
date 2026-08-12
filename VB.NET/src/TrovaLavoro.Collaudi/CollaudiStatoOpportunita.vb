Imports System.Linq
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Collaudi della macchina degli stati di una candidatura (cap. 07.3): le strade che
    ''' esistono, quelle che non esistono, come lo stato si scrive su disco e come si
    ''' deduce quando su disco non c'è.
    ''' </summary>
    ''' <remarks>
    ''' Due stati — <c>inviata</c> ed <c>esito</c> — non li scrive ancora nessuno: sono di
    ''' T6. Le loro strade si collaudano lo stesso, perché sono già nello schema e nel
    ''' codice, e un pezzo di macchina che nessuno prova è un pezzo che si scopre rotto il
    ''' giorno in cui serve.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiStatoOpportunita

        Private Shared ReadOnly Ieri As New Date(2026, 8, 11, 9, 0, 0)
        Private Shared ReadOnly Oggi As New Date(2026, 8, 12, 15, 30, 0)

        <TestMethod>
        Public Sub LeStradeDelCicloDiVitaSonoQuelleDelCapitolo()

            ' nuova → interessante → generata → inviata → esito
            Assert.IsTrue(StatiOpportunita.Consentita(StatoOpportunita.Nuova, StatoOpportunita.Interessante))
            Assert.IsTrue(StatiOpportunita.Consentita(StatoOpportunita.Interessante, StatoOpportunita.Generata))
            Assert.IsTrue(StatiOpportunita.Consentita(StatoOpportunita.Generata, StatoOpportunita.Inviata))
            Assert.IsTrue(StatiOpportunita.Consentita(StatoOpportunita.Inviata, StatoOpportunita.Esito))

            ' … e lo scarto, che si può decidere fino a un attimo prima di spedire.
            For Each da As StatoOpportunita In {StatoOpportunita.Nuova, StatoOpportunita.Interessante,
                                                StatoOpportunita.Generata}
                Assert.IsTrue(StatiOpportunita.Consentita(da, StatoOpportunita.Scartata),
                              $"da {StatiOpportunita.Nome(da)} si deve poter scartare")
            Next

        End Sub

        <TestMethod>
        Public Sub NessunaScorciatoiaENessunPassoIndietro()

            ' Saltare un passo: i documenti non esistono prima di essere scritti.
            Assert.IsFalse(StatiOpportunita.Consentita(StatoOpportunita.Nuova, StatoOpportunita.Generata))
            Assert.IsFalse(StatiOpportunita.Consentita(StatoOpportunita.Interessante, StatoOpportunita.Inviata))

            ' Tornare indietro: una candidatura spedita non si de-spedisce.
            Assert.IsFalse(StatiOpportunita.Consentita(StatoOpportunita.Generata, StatoOpportunita.Interessante))
            Assert.IsFalse(StatiOpportunita.Consentita(StatoOpportunita.Inviata, StatoOpportunita.Scartata))

            ' I due capolinea (deciso con Mirco il 2026-08-12): dallo scarto non si
            ' ripesca dall'interfaccia — la cartella resta su disco, e chi ci ripensa
            ' davvero ha ancora tutto.
            For Each fine As StatoOpportunita In {StatoOpportunita.Scartata, StatoOpportunita.Esito}
                For Each verso As StatoOpportunita In [Enum].GetValues(Of StatoOpportunita)()
                    Assert.IsFalse(StatiOpportunita.Consentita(fine, verso),
                                   $"da {StatiOpportunita.Nome(fine)} non si va da nessuna parte")
                Next
            Next

        End Sub

        <TestMethod>
        Public Sub IlNomeSuDiscoSiRileggeSempre()

            For Each stato As StatoOpportunita In [Enum].GetValues(Of StatoOpportunita)()

                Dim scritto As String = StatiOpportunita.Nome(stato)
                Assert.AreEqual(scritto, scritto.ToLowerInvariant(), "i valori di schema sono minuscoli")

                Assert.AreEqual(stato, StatiOpportunita.DaNome(scritto),
                                $"«{scritto}» deve tornare indietro com'era")

                ' Un file corretto a mano può avere maiuscole e spazi: sono parole
                ' dell'utente, e leggerle non è indovinare.
                Assert.AreEqual(stato, StatiOpportunita.DaNome("  " & scritto.ToUpperInvariant() & " "))

            Next

        End Sub

        <TestMethod>
        Public Sub UnNomeCheNonConosciamoNonSiIndovina()

            For Each niente As String In {Nothing, "", "   ", "boh", "in attesa"}
                Assert.IsNull(StatiOpportunita.DaNome(niente), $"«{niente}» non è uno stato")
            Next

        End Sub

        <TestMethod>
        Public Sub LoStatoSiDeduceDaiFilePresenti()

            ' La regola delle cartelle scritte prima di T5c (cap. 11.1): non si migra
            ' niente, si guarda cosa c'è dentro.
            Dim solaAnnuncio As New Opportunita With {.Annuncio = JsonNode.Parse("{""titolo"": ""Tecnico""}")}
            Assert.AreEqual(StatoOpportunita.Nuova, StatiOpportunita.Dedotto(solaAnnuncio))

            Dim confrontata As New Opportunita With {
                .Annuncio = solaAnnuncio.Annuncio,
                .Confronto = JsonNode.Parse("{""giudizi"": []}")}
            Assert.AreEqual(StatoOpportunita.Interessante, StatiOpportunita.Dedotto(confrontata))

            Dim generata As New Opportunita With {
                .Annuncio = solaAnnuncio.Annuncio,
                .Confronto = confrontata.Confronto,
                .Cv = JsonNode.Parse("{""intestazione"": {}}")}
            Assert.AreEqual(StatoOpportunita.Generata, StatiOpportunita.Dedotto(generata))

        End Sub

        <TestMethod>
        Public Sub AvanzareAnnotaQuandoEStatoSuccesso()

            Dim o As New Opportunita
            Assert.AreEqual(StatoOpportunita.Nuova, o.Stato, "si nasce nuove")

            o.Avanza(StatoOpportunita.Nuova, Ieri)
            o.Avanza(StatoOpportunita.Interessante, Oggi)

            Assert.AreEqual(StatoOpportunita.Interessante, o.Stato)
            Assert.AreEqual(Ieri, o.DateStati(StatoOpportunita.Nuova), "quando è nata")
            Assert.AreEqual(Oggi, o.DateStati(StatoOpportunita.Interessante), "quando è diventata interessante")

        End Sub

        <TestMethod>
        Public Sub RestareDovESiENonEUnPassaggioNuovo()

            ' Rigenerare i documenti di un'opportunità già generata è una cosa che si fa:
            ' non è un errore, ma la data del passaggio resta quella della prima volta.
            Dim o As New Opportunita
            o.Avanza(StatoOpportunita.Interessante, Ieri)
            o.Avanza(StatoOpportunita.Generata, Ieri)

            o.Avanza(StatoOpportunita.Generata, Oggi)

            Assert.AreEqual(StatoOpportunita.Generata, o.Stato)
            Assert.AreEqual(Ieri, o.DateStati(StatoOpportunita.Generata), "la data non si riscrive")

        End Sub

        <TestMethod>
        Public Sub UnaStradaCheNonEsisteSiFermaSubito()

            Dim o As New Opportunita
            o.Avanza(StatoOpportunita.Scartata, Ieri)

            Dim ripescata As InvalidOperationException =
                Assert.Throws(Of InvalidOperationException)(Sub() o.Avanza(StatoOpportunita.Interessante))

            Assert.Contains("scartata", ripescata.Message, "il messaggio dice da dove non si passa")
            Assert.AreEqual(StatoOpportunita.Scartata, o.Stato, "e lo stato non si è mosso")

        End Sub

        <TestMethod>
        Public Sub LeDateSiScrivonoNellOrdineDelCicloDiVita()

            Dim o As New Opportunita
            o.Avanza(StatoOpportunita.Nuova, Ieri)
            o.Avanza(StatoOpportunita.Interessante, Oggi)
            o.Avanza(StatoOpportunita.Generata, Oggi)

            Dim scritte As JsonObject = StatiOpportunita.DateComeJson(o.DateStati)

            ' Un file che si legge a mano deve raccontare la storia dall'inizio, non
            ' nell'ordine in cui è capitato di annotarla.
            Assert.AreEqual("nuova, interessante, generata",
                            String.Join(", ", scritte.Select(Function(p) p.Key)))

            Dim rilette As New Dictionary(Of StatoOpportunita, Date)
            StatiOpportunita.RiempiDate(rilette, scritte)

            Assert.HasCount(3, rilette)
            Assert.AreEqual(Oggi, rilette(StatoOpportunita.Generata), "e le date tornano indietro com'erano")

        End Sub

        <TestMethod>
        Public Sub UnaDataIlleggibileSparisceSenzaPortarViaLeAltre()

            Dim scritte As JsonObject = JsonNode.Parse(
                "{""nuova"": ""2026-08-11 09:00:00"", ""interessante"": ""l'altro ieri""}").AsObject()

            Dim rilette As New Dictionary(Of StatoOpportunita, Date)
            StatiOpportunita.RiempiDate(rilette, scritte)

            Assert.HasCount(1, rilette, "quella che si legge resta")
            Assert.AreEqual(Ieri, rilette(StatoOpportunita.Nuova))

        End Sub

    End Class

End Namespace
