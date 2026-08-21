Imports System.IO
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Collaudi dell'esito di una candidatura (cap. 07.3): i tre valori, come si
    ''' registrano e si correggono, e come si accordano con lo stato che li contiene.
    ''' </summary>
    ''' <remarks>
    ''' L'esito è la sola parte del ciclo di vita che <b>torna indietro</b>, perché è una
    ''' dichiarazione dell'utente e non un fatto osservato. Metà di questi collaudi prova
    ''' proprio quello: che si possa cambiare idea, e che cambiando idea non resti dietro
    ''' niente — né un esito appeso a uno stato che non lo prevede, né una data di un
    ''' passaggio che non è più avvenuto.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiEsitoCandidatura

        Private Shared ReadOnly Spedita As New Date(2026, 8, 5, 9, 30, 0)
        Private Shared ReadOnly Risposta As New Date(2026, 8, 21, 17, 0, 0)

        ''' <summary>Una candidatura arrivata fino allo stato «inviata», come dopo P7.</summary>
        Private Shared Function Inviata() As Opportunita

            Dim o As New Opportunita With {
                .Creata = Spedita.AddDays(-2),
                .Annuncio = JsonNode.Parse("{""titolo"": ""Tecnico"", ""azienda"": ""Acme""}"),
                .Confronto = JsonNode.Parse("{""giudizi"": [{""requisito"": ""Patente B""}]}"),
                .Cv = JsonNode.Parse("{""sezioni"": []}")}

            o.Avanza(StatoOpportunita.Interessante, o.Creata)
            o.Avanza(StatoOpportunita.Generata, o.Creata.AddHours(1))
            o.Avanza(StatoOpportunita.Inviata, Spedita)

            Return o

        End Function

        <TestMethod>
        Public Sub ITreEsitiSiScrivonoESiRileggono()

            For Each esito As EsitoCandidatura In [Enum].GetValues(Of EsitoCandidatura)()
                Dim scritto As String = EsitiCandidatura.Nome(esito)

                Assert.AreEqual(scritto, scritto.ToLowerInvariant(),
                                "sul disco i valori di schema si scrivono minuscoli")
                Assert.AreEqual(esito, EsitiCandidatura.DaNome(scritto))
            Next

            ' «In attesa» non è un esito: è il silenzio di una candidatura già spedita
            ' (deciso con Mirco il 2026-08-21).
            Assert.AreEqual(3, [Enum].GetValues(Of EsitoCandidatura)().Length)
            Assert.IsNull(EsitiCandidatura.DaNome("in attesa"))

        End Sub

        <TestMethod>
        Public Sub UnEsitoCheNonConosciamoValeComeNonRegistrato()

            Assert.IsNull(EsitiCandidatura.DaNome(Nothing))
            Assert.IsNull(EsitiCandidatura.DaNome("   "))
            Assert.IsNull(EsitiCandidatura.DaNome("promosso"))

            ' Spazi e maiuscole di un file corretto a mano non fanno perdere il valore.
            Assert.AreEqual(EsitoCandidatura.Assunto, EsitiCandidatura.DaNome("  Assunto "))

        End Sub

        <TestMethod>
        Public Sub LEtichettaDiceComeEFinitaEnonComeSiChiamaLoStato()

            ' Chi guarda pensa «rifiutata», non «con esito» (cap. 07.3).
            Assert.AreEqual("Rifiutata",
                            EsitiCandidatura.EtichettaDi(StatoOpportunita.Esito, EsitoCandidatura.Rifiutata))
            Assert.AreEqual("Assunto 🎉",
                            EsitiCandidatura.EtichettaDi(StatoOpportunita.Esito, EsitoCandidatura.Assunto))

            ' Senza esito resta la parola dello stato, per tutti gli altri stati.
            Assert.AreEqual("Inviata", EsitiCandidatura.EtichettaDi(StatoOpportunita.Inviata, Nothing))
            Assert.AreEqual("Scartata", EsitiCandidatura.EtichettaDi(StatoOpportunita.Scartata, Nothing))

        End Sub

        <TestMethod>
        Public Sub StatoEdEsitoLettiDaUnFileSiMettonoDAccordo()

            ' Stato «esito» senza dire quale: si torna a «inviata», che è l'unico punto da
            ' cui a quello stato si arriva.
            Dim stato As StatoOpportunita = StatoOpportunita.Esito
            Dim esito As EsitoCandidatura? = Nothing
            EsitiCandidatura.Concorda(stato, esito)
            Assert.AreEqual(StatoOpportunita.Inviata, stato)
            Assert.IsFalse(esito.HasValue)

            ' Un esito appeso a uno stato che non lo prevede cade: lo stato è il campo che
            ' tutto il programma guarda.
            stato = StatoOpportunita.Scartata
            esito = EsitoCandidatura.Colloquio
            EsitiCandidatura.Concorda(stato, esito)
            Assert.AreEqual(StatoOpportunita.Scartata, stato)
            Assert.IsFalse(esito.HasValue)

            ' La coppia buona non si tocca.
            stato = StatoOpportunita.Esito
            esito = EsitoCandidatura.Assunto
            EsitiCandidatura.Concorda(stato, esito)
            Assert.AreEqual(StatoOpportunita.Esito, stato)
            Assert.AreEqual(EsitoCandidatura.Assunto, esito.Value)

        End Sub

        <TestMethod>
        Public Sub SegnareUnEsitoPortaAlloStatoEsitoConLaSuaData()

            Dim o As Opportunita = Inviata()

            o.SegnaEsito(EsitoCandidatura.Colloquio, Risposta)

            Assert.AreEqual(StatoOpportunita.Esito, o.Stato)
            Assert.AreEqual(EsitoCandidatura.Colloquio, o.Esito.Value)
            Assert.AreEqual(Risposta, o.DateStati(StatoOpportunita.Esito))

            ' La data dell'invio resta quella: il promemoria di follow-up la legge da lì.
            Assert.AreEqual(Spedita, o.DateStati(StatoOpportunita.Inviata))

        End Sub

        <TestMethod>
        Public Sub LEsitoSiCorreggeELaDataSegueLUltimaNotizia()

            Dim o As Opportunita = Inviata()
            o.SegnaEsito(EsitoCandidatura.Colloquio, Risposta)

            Dim dopo As Date = Risposta.AddDays(30)
            o.SegnaEsito(EsitoCandidatura.Assunto, dopo)

            Assert.AreEqual(EsitoCandidatura.Assunto, o.Esito.Value)

            ' È l'unica eccezione alla regola del «primo ingresso»: una storia finita a
            ' novembre non si racconta con la data di settembre.
            Assert.AreEqual(dopo, o.DateStati(StatoOpportunita.Esito))

        End Sub

        <TestMethod>
        Public Sub TogliereLEsitoRimetteLaCandidaturaInAttesa()

            Dim o As Opportunita = Inviata()
            o.SegnaEsito(EsitoCandidatura.Rifiutata, Risposta)

            o.SegnaEsito(Nothing)

            Assert.AreEqual(StatoOpportunita.Inviata, o.Stato)
            Assert.IsFalse(o.Esito.HasValue)
            Assert.IsFalse(o.DateStati.ContainsKey(StatoOpportunita.Esito),
                           "la data di un passaggio che non è più avvenuto non resta dietro")

        End Sub

        <TestMethod>
        Public Sub TogliereUnEsitoCheNonCEraNonFaNiente()

            Dim o As Opportunita = Inviata()

            o.SegnaEsito(Nothing)

            Assert.AreEqual(StatoOpportunita.Inviata, o.Stato)
            Assert.IsFalse(o.Esito.HasValue)

        End Sub

        <TestMethod>
        Public Sub PrimaDellInvioNonCEnienteCheSiaAndatoInUnModoONellAltro()

            Dim o As New Opportunita With {
                .Annuncio = JsonNode.Parse("{""titolo"": ""Tecnico""}"),
                .Confronto = JsonNode.Parse("{""giudizi"": []}")}
            o.Avanza(StatoOpportunita.Interessante)

            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() o.SegnaEsito(EsitoCandidatura.Colloquio))

            ' E su una candidatura scartata nemmeno: quella è chiusa in un altro modo.
            Dim scartata As Opportunita = Inviata()
            Dim persa As New Opportunita With {.Annuncio = scartata.Annuncio}
            persa.Avanza(StatoOpportunita.Scartata)

            Assert.ThrowsExactly(Of InvalidOperationException)(
                Sub() persa.SegnaEsito(EsitoCandidatura.Rifiutata))

        End Sub

        <TestMethod>
        Public Sub LEsitoSopravviveAlGiroSuDisco()

            Dim radice As String = Path.Combine(Path.GetTempPath(), "esito-" & Guid.NewGuid().ToString("N"))
            Dim archivio As New ArchivioOpportunita(New CartellaDati(radice))

            Try
                Dim o As Opportunita = Inviata()
                o.SegnaEsito(EsitoCandidatura.Colloquio, Risposta)

                Dim dove As String = archivio.Salva(o)
                Dim riletta As Opportunita = archivio.Carica(dove)

                Assert.AreEqual(StatoOpportunita.Esito, riletta.Stato)
                Assert.AreEqual(EsitoCandidatura.Colloquio, riletta.Esito.Value)
                Assert.AreEqual(Risposta, riletta.DateStati(StatoOpportunita.Esito))

                ' Tolto l'esito, il file non lo racconta più: nella cartella non resta una
                ' riga che dice come è andata una cosa che non si sa ancora.
                riletta.SegnaEsito(Nothing)
                archivio.Salva(riletta)

                Dim ancora As Opportunita = archivio.Carica(dove)
                Assert.AreEqual(StatoOpportunita.Inviata, ancora.Stato)
                Assert.IsFalse(ancora.Esito.HasValue)

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

        <TestMethod>
        Public Sub UnFileConLEsitoSuUnoStatoSbagliatoNonInventaNiente()

            Dim radice As String = Path.Combine(Path.GetTempPath(), "esito-" & Guid.NewGuid().ToString("N"))
            Dim cartella As New CartellaDati(radice)
            Dim archivio As New ArchivioOpportunita(cartella)

            Try
                Dim o As Opportunita = Inviata()
                Dim dove As String = archivio.Salva(o)

                ' La cartella dati si può correggere a mano (cap. 11.1): qui qualcuno ha
                ' scritto un esito lasciando lo stato «inviata».
                Dim file As String = Path.Combine(dove, ArchivioOpportunita.FileStato)
                Dim stato As JsonObject = TryCast(JsonNode.Parse(IO.File.ReadAllText(file)), JsonObject)
                stato("esito") = "assunto"
                IO.File.WriteAllText(file, stato.ToJsonString())

                Dim riletta As Opportunita = archivio.Carica(dove)

                Assert.AreEqual(StatoOpportunita.Inviata, riletta.Stato)
                Assert.IsFalse(riletta.Esito.HasValue,
                               "un esito senza il suo stato non promuove nessuno")

            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

    End Class

End Namespace
