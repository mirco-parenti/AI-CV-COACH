Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi della riga di comando (cap. 11.1). Le cose da tenere ferme sono due: la
    ''' cartella dati si può indicare, e <b>niente</b> di quel che si scrive lì dentro può
    ''' impedire all'applicazione di aprirsi. Un errore di battitura non deve costare un
    ''' eseguibile che non parte e non dice perché.
    ''' </summary>
    <TestClass>
    Public Class CollaudiArgomentiAvvio

        ''' <summary>Un percorso qualunque: qui non si scrive niente, si legge un argomento.</summary>
        Private Const Percorso As String = "D:\prova"

        <TestMethod>
        Public Sub SenzaArgomentiValeLaCartellaDiSempre()
            ' Il caso normale — l'exe aperto con un doppio clic — non deve avere né una
            ' radice sua né qualcosa da dire.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi(New String() {})

            Assert.IsNull(letti.RadiceDati, "nessuna radice chiesta")
            Assert.IsNull(letti.Avviso, "e niente da segnalare")
        End Sub

        <TestMethod>
        Public Sub UnaRigaDiComandoInesistenteNonEUnErrore()
            ' Chi costruisce la finestra senza passare da Main non ha argomenti da dare.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi(Nothing)

            Assert.IsNull(letti.RadiceDati, "nessuna radice chiesta")
            Assert.IsNull(letti.Avviso, "e niente da segnalare")
        End Sub

        <TestMethod>
        Public Sub LaCartellaDatiSiIndicaNelleDueForme()
            ' Chi scrive un comando si aspetta che funzionino entrambe, e sbagliare la
            ' forma non deve costare un avvio andato a vuoto.
            Dim staccata As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--dati", Percorso})
            Dim attaccata As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--dati=" & Percorso})

            Assert.AreEqual(Percorso, staccata.RadiceDati, "--dati percorso")
            Assert.AreEqual(Percorso, attaccata.RadiceDati, "--dati=percorso")
            Assert.IsNull(staccata.Avviso, "niente da segnalare")
            Assert.IsNull(attaccata.Avviso, "niente da segnalare")
        End Sub

        <TestMethod>
        Public Sub GliSpaziIntornoAlPercorsoNonContano()
            ' Un percorso che arriva da un copia-incolla si porta dietro degli spazi: la
            ' cartella però è quella, e trattarli come parte del nome darebbe un percorso
            ' che non esiste.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--dati", "  " & Percorso & "  "})

            Assert.AreEqual(Percorso, letti.RadiceDati, "il percorso ripulito")
        End Sub

        <TestMethod>
        Public Sub LOpzioneSiRiconosceAnchePiuMaiuscola()
            ' Windows non distingue le maiuscole nei nomi, e chi scrive il comando neanche.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--DATI", Percorso})

            Assert.AreEqual(Percorso, letti.RadiceDati, "l'opzione è la stessa")
        End Sub

        <TestMethod>
        Public Sub UnArgomentoSconosciutoSiDiceESiScavalca()
            ' La riga di comando cresce con le tappe (T8 aggiungerà --mcp): un eseguibile
            ' che rifiuta di partire per una parola in più è peggio di uno che spiega.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--pippo", "--dati", Percorso})

            Assert.AreEqual(Percorso, letti.RadiceDati, "il resto della riga si legge lo stesso")
            Assert.IsNotNull(letti.Avviso, "l'ignoto si dice")
            Assert.Contains("--pippo", letti.Avviso, "e si dice quale")
        End Sub

        <TestMethod>
        Public Sub SenzaPercorsoLOpzioneAvvisaERipiega()
            ' «--dati» e basta: la cartella resta quella predefinita, ma chi ha scritto il
            ' comando deve sapere che la sua metà di frase non è arrivata a niente.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--dati"})

            Assert.IsNull(letti.RadiceDati, "si ripiega sulla predefinita")
            Assert.IsNotNull(letti.Avviso, "dicendolo")
            Assert.Contains("--dati", letti.Avviso, "e dicendo di quale argomento si tratta")
        End Sub

        <TestMethod>
        Public Sub UnPercorsoVuotoAttaccatoAllUgualeNonPassa()
            ' «--dati=» è la stessa cosa di «--dati» senza niente dopo, e va detta uguale:
            ' una radice vuota, più avanti, sarebbe un'eccezione dentro CartellaDati.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--dati="})

            Assert.IsNull(letti.RadiceDati, "si ripiega sulla predefinita")
            Assert.IsNotNull(letti.Avviso, "dicendolo")
        End Sub

        <TestMethod>
        Public Sub IlPercorsoNonSiMangiaLOpzioneDopo()
            ' «--dati --mcp» non vuol dire che la cartella si chiami «--mcp»: vuol dire che
            ' il percorso manca. Senza questa regola, l'opzione della tappa dopo verrebbe
            ' inghiottita e nessuno se ne accorgerebbe.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--dati", "--mcp"})

            Assert.IsNull(letti.RadiceDati, "nessuna radice presa a sproposito")
            Assert.HasCount(2, letti.Avvisi, "due cose da dire: il percorso che manca e l'opzione ignota")
            Assert.Contains("--mcp", letti.Avviso, "l'opzione dopo resta visibile")
        End Sub

        <TestMethod>
        Public Sub LaCartellaIndicataDueVolteTieneLaPrima()
            ' Fra due cartelle diverse la scelta la deve fare chi scrive il comando. La
            ' seconda che vince in silenzio manderebbe a scrivere in un posto che nessuno
            ' ha dichiarato di volere.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--dati", Percorso, "--dati", "E:\altra"})

            Assert.AreEqual(Percorso, letti.RadiceDati, "vince la prima")
            Assert.IsNotNull(letti.Avviso, "e l'ambiguità si dice")
        End Sub

        <TestMethod>
        Public Sub LaChiaveSiPuoChiedereDaCapo()
            ' Finché le Impostazioni non ci sono (T9), «--chiave» è l'unico modo di
            ' sostituire una chiave salvata storta: è un interruttore, e vale anche
            ' insieme a una cartella dati diversa.
            Dim sola As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--chiave"})
            Dim insieme As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--dati", Percorso, "--CHIAVE"})

            Assert.IsTrue(sola.ChiediLaChiave, "la chiede")
            Assert.IsNull(sola.Avviso, "e non c'è niente da segnalare")
            Assert.IsTrue(insieme.ChiediLaChiave, "anche scritta in maiuscolo")
            Assert.AreEqual(Percorso, insieme.RadiceDati, "senza disturbare il resto della riga")
        End Sub

        <TestMethod>
        Public Sub SenzaLOpzioneLaChiaveNonSiRichiede()
            ' Il caso normale: chi ha già la sua chiave non deve vedersela richiedere a
            ' ogni avvio.
            Assert.IsFalse(ArgomentiAvvio.Leggi({"--dati", Percorso}).ChiediLaChiave, "nessuno l'ha chiesto")
        End Sub

        <TestMethod>
        Public Sub LaChiaveAttaccataAllUgualeSiScarta()
            ' «--chiave=sk-ant-…» resterebbe scritta nella cronologia della shell e
            ' nell'elenco dei processi: si ignora il valore, si dice che lo si è fatto, e
            ' non lo si ripete nell'avviso (cap. 11.3).
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--chiave=sk-ant-vera-0000"})

            Assert.IsTrue(letti.ChiediLaChiave, "la finestra si apre lo stesso")
            Assert.IsNotNull(letti.Avviso, "dicendo che il valore è stato scartato")
            Assert.DoesNotContain("sk-ant-vera-0000", letti.Avviso, "senza ripetere la chiave")
        End Sub

        <TestMethod>
        Public Sub UnaChiaveScrittaSullaRigaDiComandoNonFinisceNellAvviso()
            ' «--chiave sk-ant-…»: la chiave resta un argomento sconosciuto, e l'avviso
            ' degli sconosciuti li nomina. Quello che si vede nella barra di stato lo
            ' vede chiunque guardi lo schermo.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"--chiave", "sk-ant-vera-0000"})

            Assert.IsTrue(letti.ChiediLaChiave, "la finestra si apre")
            Assert.IsNotNull(letti.Avviso, "l'argomento di troppo si dice")
            Assert.DoesNotContain("sk-ant-vera-0000", letti.Avviso, "ma non si scrive")
            Assert.Contains("chiave API", letti.Avviso, "si dice però cos'era")
        End Sub

        <TestMethod>
        Public Sub GliArgomentiVuotiNonDisturbano()
            ' Windows non li produce, ma una catena di script sì: una stringa vuota non è
            ' un argomento sconosciuto, è niente.
            Dim letti As ArgomentiAvvio = ArgomentiAvvio.Leggi({"", "   ", "--dati", Percorso})

            Assert.AreEqual(Percorso, letti.RadiceDati, "il percorso arriva")
            Assert.IsNull(letti.Avviso, "senza avvisi inventati")
        End Sub

    End Class

End Namespace
