Imports System.Linq
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi della macchina del dialogo (cap. 12, flusso B; cap. 14, T3). Girano
    ''' <b>senza interfaccia e senza rete</b>: al posto dell'AI c'è
    ''' <see cref="StrutturatoreFinto"/>, che restituisce frammenti preparati.
    ''' </summary>
    ''' <remarks>
    ''' Le cose che devono restare vere sono quelle del prototipo: l'ordine dei turni, la
    ''' scheda di conferma prima di <b>ogni</b> cosa che entra nel profilo, e le tre
    ''' regole dell'anti-perdita — instradamento in avanti, all'indietro, e la guardia
    ''' anti-rimbalzo che fa convergere la passata finale.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiDialogoProfilo

        ' ------------------------------------------------------------------
        ' Frammenti preparati: le risposte che l'AI darebbe
        ' ------------------------------------------------------------------

        Private Const FrNome As String = "{""nome"": ""Luca Ferrari""}"

        Private Shared Function FrContatti(Optional altrove As String = "{}") As String
            Return "{""contatti"": {""email"": ""luca@example.it"", ""telefono"": ""333 0000000""," &
                   " ""citta"": ""Forlì"", ""link"": """"}, ""altrove"": " & altrove & "}"
        End Function

        Private Shared Function FrPatente(ha As String, ParamArray categorie As String()) As String
            Dim elenco As String = String.Join(", ", categorie.Select(Function(c) """" & c & """"))
            Return "{""patente"": {""ha"": """ & ha & """, ""categorie"": [" & elenco & "]}, ""altrove"": {}}"
        End Function

        Private Shared Function FrLavoro(ruolo As String, Optional altrove As String = "{}") As String
            Return "{""esperienze_formali"": [{""ruolo"": """ & ruolo & """, ""azienda"": ""Romagna Logistica""," &
                   " ""durata"": ""3 anni"", ""cosa_facevo"": ""Carico e scarico"", ""tipo"": """"}]," &
                   " ""altrove"": " & altrove & "}"
        End Function

        Private Shared Function FrInformale(cosa As String) As String
            Return "{""esperienze_informali"": [{""cosa_facevo"": """ & cosa & """, ""quando"": """"," &
                   " ""con_chi"": """"}], ""altrove"": {}}"
        End Function

        Private Shared Function FrCompetenze(altrove As String, ParamArray voci As String()) As String
            Dim elenco As String = String.Join(", ", voci.Select(Function(c) """" & c & """"))
            Return "{""competenze"": [" & elenco & "], ""altrove"": " & altrove & "}"
        End Function

        Private Shared Function FrFormazione(titolo As String, Optional altrove As String = "{}") As String
            Return "{""formazione"": [{""titolo"": """ & titolo & """, ""istituto"": ""ITIS""," &
                   " ""anno"": ""2018""}], ""altrove"": " & altrove & "}"
        End Function

        ''' <summary>Un frammento che non ha colto nulla per il suo turno.</summary>
        Private Shared Function FrVuoto(chiave As String, Optional altrove As String = "{}") As String
            Return "{""" & chiave & """: [], ""altrove"": " & altrove & "}"
        End Function

        ' ------------------------------------------------------------------
        ' Il giro completo
        ' ------------------------------------------------------------------

        <TestMethod>
        Public Async Function IlDialogoPercorreISetteTurniEChiude() As Task

            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).Dara(FrContatti()).Dara(FrPatente("sì", "B")).
                  Dara(FrLavoro("Magazziniere")).Dara(FrInformale("Traslochi con un amico")).
                  Dara(FrCompetenze("{}", "Uso del muletto")).Dara(FrFormazione("Diploma"))

            Dim dialogo As New DialogoProfilo(finto)

            Dim mossa As Mossa = Await dialogo.AvviaAsync()
            Assert.AreEqual(TipoMossa.ChiediRisposta, mossa.Tipo, "si parte chiedendo il nome")
            Assert.Contains("come ti chiami?", mossa.Detto(0), "l'apertura del primo turno")

            Await ConfermaAsync(dialogo, "Mi chiamo Luca Ferrari")
            Await ConfermaAsync(dialogo, "luca@example.it, 333 0000000, Forlì")
            Await ConfermaAsync(dialogo, "Sì, la B")
            Await ConfermaAsync(dialogo, "Ho fatto il magazziniere")
            Await dialogo.ScegliAsync(Scelte.Procedi)
            Await ConfermaAsync(dialogo, "Davo una mano nei traslochi")
            Await dialogo.ScegliAsync(Scelte.Procedi)
            Await dialogo.RispondiAsync("So usare il muletto")
            Await dialogo.ScegliAsync(Scelte.Conferma)
            Await ConfermaAsync(dialogo, "Ho il diploma")
            mossa = Await dialogo.ScegliAsync(Scelte.Procedi)

            Assert.AreEqual(
                "nome → contatti → patente → esperienze_formali → esperienze_informali → competenze → formazione",
                finto.TurniChiesti(), "l'ordine dei turni")

            Assert.AreEqual(TipoMossa.Fine, mossa.Tipo, "il dialogo è finito")
            Assert.IsTrue(dialogo.Finito, "e lo dichiara")
            Assert.Contains("niente di inventato", String.Join(" ", mossa.Detto), "la chiusura del prototipo")

            Dim p As TrovaLavoro.Dati.Profilo = dialogo.Profilo
            Assert.AreEqual("Luca Ferrari", p.Nome, "nome")
            Assert.AreEqual("luca@example.it", p.Contatti.Email, "email")
            Assert.AreEqual("sì", p.Patente.Ha, "patente")
            Assert.HasCount(1, p.EsperienzeFormali, "un'esperienza formale")
            Assert.HasCount(1, p.EsperienzeInformali, "un'esperienza informale")
            Assert.HasCount(1, p.Competenze, "una competenza")
            Assert.HasCount(1, p.Formazione, "un titolo")

        End Function

        <TestMethod>
        Public Async Function IlRiepilogoMostraQuelloCheHaRaccolto() As Task

            Dim dialogo As DialogoProfilo = Await FinoAllaFineAsync()
            Dim mossa As Mossa = Await UltimaMossaAsync(dialogo)

            Dim titoli As String = String.Join(", ", mossa.Schede.Select(Function(s) s.Titolo))
            Assert.Contains("Nome", titoli, "il riepilogo elenca le sezioni")
            Assert.Contains("Esperienze formali 1", titoli, "una scheda per voce")
            Assert.Contains("Competenze", titoli, "e le competenze")

        End Function

        ' ------------------------------------------------------------------
        ' La conferma: niente entra nel profilo senza un sì
        ' ------------------------------------------------------------------

        <TestMethod>
        Public Async Function NienteEntraNelProfiloSenzaConferma() As Task

            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome)

            Dim dialogo As New DialogoProfilo(finto)
            Await dialogo.AvviaAsync()

            Dim mossa As Mossa = Await dialogo.RispondiAsync("Mi chiamo Luca Ferrari")

            Assert.AreEqual(TipoMossa.ChiediScelta, mossa.Tipo, "si chiede conferma")
            Assert.Contains("Ho capito che ti chiami: Luca Ferrari.", mossa.Detto(0), "la scheda di conferma")
            Assert.IsEmpty(dialogo.Profilo.Nome, "ma nel profilo non è ancora entrato niente")

            Await dialogo.ScegliAsync(Scelte.Conferma)
            Assert.AreEqual("Luca Ferrari", dialogo.Profilo.Nome, "solo dopo il sì")

        End Function

        <TestMethod>
        Public Async Function CorreggiRifaLaStessaDomanda() As Task

            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).Dara("{""nome"": ""Luca Maria Ferrari""}")

            Dim dialogo As New DialogoProfilo(finto)
            Await dialogo.AvviaAsync()
            Await dialogo.RispondiAsync("Luca Ferrari")

            Dim mossa As Mossa = Await dialogo.ScegliAsync(Scelte.Correggi)
            Assert.AreEqual(TipoMossa.ChiediRisposta, mossa.Tipo, "si torna a chiedere")
            Assert.IsEmpty(dialogo.Profilo.Nome, "e non è entrato nulla")

            Await dialogo.RispondiAsync("Luca Maria Ferrari")
            Await dialogo.ScegliAsync(Scelte.Conferma)
            Assert.AreEqual("Luca Maria Ferrari", dialogo.Profilo.Nome, "vale la seconda")

        End Function

        ' ------------------------------------------------------------------
        ' Anti-perdita
        ' ------------------------------------------------------------------

        <TestMethod>
        Public Async Function CioCheEDiUnTurnoFuturoTornaAQuelTurno() As Task

            ' Nel turno dei contatti l'utente nomina anche un diploma: si parcheggia e
            ' lo si ripesca all'ingresso del turno della formazione (instradamento in
            ' avanti). Nel frattempo non deve entrare nel profilo.
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).
                  Dara(FrContatti("{""formazione"": [""ho il diploma di istituto tecnico""]}")).
                  Dara(FrPatente("no")).
                  Dara(FrVuoto("esperienze_formali")).
                  Dara(FrVuoto("esperienze_informali")).
                  Dara(FrCompetenze("{}")).
                  Dara(FrFormazione("Diploma di istituto tecnico"))

            Dim dialogo As New DialogoProfilo(finto)
            Await dialogo.AvviaAsync()
            Await ConfermaAsync(dialogo, "Luca Ferrari")
            Await ConfermaAsync(dialogo, "luca@example.it, e ho il diploma di istituto tecnico")
            Await ConfermaAsync(dialogo, "No, niente patente")

            Assert.IsEmpty(dialogo.Profilo.Formazione, "il diploma non entra prima del suo turno")

            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Nessuna")
            Dim mossa As Mossa = Await dialogo.ScegliAsync(Scelte.Oltre)

            ' Turno competenze: risposta vuota ma si prosegue, poi si apre la formazione.
            Await dialogo.RispondiAsync("Niente di che")
            mossa = Await dialogo.ScegliAsync(Scelte.Oltre)

            Assert.AreEqual(TipoMossa.ChiediScelta, mossa.Tipo, "all'ingresso del turno si ripesca")
            Assert.Contains("studi e formazione", String.Join(" ", mossa.Detto), "dicendo di che si tratta")
            Assert.Contains("ho il diploma di istituto tecnico", mossa.EcoUtente, "con le parole dell'utente")
            Assert.AreEqual("formazione", finto.Chiamate.Last().Turno, "strutturato col prompt di quel turno")

            Await dialogo.ScegliAsync(Scelte.Conferma)
            Assert.HasCount(1, dialogo.Profilo.Formazione, "e solo alla conferma entra")
            Assert.AreEqual("Diploma di istituto tecnico", dialogo.Profilo.Formazione(0).Titolo, "il titolo")

        End Function

        <TestMethod>
        Public Async Function CioCheEDiUnTurnoPassatoSiRecuperaAllaFine() As Task

            ' Nell'ultimo turno l'utente nomina un lavoro: il turno è già passato, quindi
            ' si recupera nella passata finale, prima del riepilogo.
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).Dara(FrContatti()).Dara(FrPatente("no")).
                  Dara(FrVuoto("esperienze_formali")).
                  Dara(FrVuoto("esperienze_informali")).
                  Dara(FrCompetenze("{}")).
                  Dara(FrFormazione("Diploma", "{""esperienze_formali"": [""ho lavorato in magazzino tre anni""]}")).
                  Dara(FrLavoro("Magazziniere"))

            Dim dialogo As New DialogoProfilo(finto)
            Await PrimiTreTurniAsync(dialogo)

            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Niente")
            Await dialogo.ScegliAsync(Scelte.Oltre)

            Await ConfermaAsync(dialogo, "Il diploma, e ho lavorato in magazzino tre anni")
            Dim mossa As Mossa = Await dialogo.ScegliAsync(Scelte.Procedi)

            Assert.Contains("Prima di chiudere", String.Join(" ", mossa.Detto), "la passata finale")
            Assert.Contains("esperienze di lavoro", String.Join(" ", mossa.Detto), "sulla categoria giusta")
            Assert.AreEqual(TipoMossa.ChiediScelta, mossa.Tipo, "e chiede conferma")

            mossa = Await dialogo.ScegliAsync(Scelte.Conferma)

            Assert.HasCount(1, dialogo.Profilo.EsperienzeFormali, "il lavoro è stato recuperato")
            Assert.AreEqual(TipoMossa.Fine, mossa.Tipo, "e poi si chiude")

        End Function

        <TestMethod>
        Public Async Function IlFrammentoCheNessunoSaCollocareVieneLasciatoFuori() As Task

            ' La guardia anti-rimbalzo: il turno di destinazione non struttura nulla e
            ' rimanda il frammento a un'altra categoria. Non deve tornare nel magazzino,
            ' se no le due categorie se lo rimpallano e la passata finale non finisce
            ' mai: si dichiara «lasciato fuori», e il dialogo converge.
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).
                  Dara(FrContatti("{""competenze"": [""boh, sono uno che si arrangia""]}")).
                  Dara(FrPatente("no")).
                  Dara(FrVuoto("esperienze_formali")).
                  Dara(FrVuoto("esperienze_informali")).
                  Dara(FrVuoto("competenze", "{""esperienze_formali"": [""sono uno che si arrangia""]}")).
                  Dara(FrVuoto("competenze")).
                  Dara(FrVuoto("formazione"))

            Dim dialogo As New DialogoProfilo(finto)
            Await PrimiTreTurniAsync(dialogo)

            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Nessuna")
            Dim mossa As Mossa = Await dialogo.ScegliAsync(Scelte.Oltre)

            ' Ingresso del turno competenze: il frammento parcheggiato non si struttura.
            Assert.Contains("lascio fuori", String.Join(" ", mossa.Detto), "lo dichiara, non lo perde in silenzio")
            Assert.Contains("si arrangia", String.Join(" ", mossa.Detto), "riportando le parole dell'utente")
            Assert.AreEqual(TipoMossa.ChiediRisposta, mossa.Tipo, "e prosegue col turno")

            Await dialogo.RispondiAsync("Niente")
            mossa = Await dialogo.ScegliAsync(Scelte.Oltre)

            ' Turno formazione, poi la chiusura: se il frammento fosse rimbalzato nel
            ' magazzino, qui la passata finale lo ritroverebbe e non convergerebbe.
            Await dialogo.RispondiAsync("Niente")
            mossa = Await dialogo.ScegliAsync(Scelte.Oltre)

            Assert.AreEqual(TipoMossa.Fine, mossa.Tipo, "il dialogo converge")
            Assert.IsEmpty(dialogo.Profilo.EsperienzeFormali, "e niente è entrato di soppiatto")

        End Function

        <TestMethod>
        Public Async Function LaPatenteDettaAiContattiSiRipescaAlSuoTurno() As Task

            ' Il prompt dei contatti promette: la patente nominata lì finisce in
            ' «altrove.patente» e la si conferma al turno giusto. Prima il magazzino
            ' conosceva solo le quattro categorie e quella promessa cadeva nel vuoto.
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).
                  Dara(FrContatti("{""patente"": [""ho la patente B""]}")).
                  Dara(FrPatente("sì", "B"))

            Dim dialogo As New DialogoProfilo(finto)
            Await dialogo.AvviaAsync()
            Await ConfermaAsync(dialogo, "Luca Ferrari")
            Dim mossa As Mossa = Await ConfermaAsync(dialogo, "luca@example.it, e ho la patente B")

            Assert.AreEqual(TipoMossa.ChiediScelta, mossa.Tipo, "il turno patente ripesca e chiede conferma")
            Assert.Contains("ho la patente B", mossa.EcoUtente, "con le parole dell'utente")
            Assert.Contains("Patente: sì (B)", String.Join(" ", mossa.Detto), "e con cosa ha capito")
            Assert.AreEqual("patente", finto.Chiamate.Last().Turno, "strutturato col prompt della patente")
            Assert.IsEmpty(dialogo.Profilo.Patente.Ha, "ma non è ancora entrato niente")

            Await dialogo.ScegliAsync(Scelte.Conferma)
            Assert.AreEqual("sì", dialogo.Profilo.Patente.Ha, "entra solo col sì")
            Assert.AreEqual("B", dialogo.Profilo.Patente.Categorie(0), "con la sua categoria")

        End Function

        <TestMethod>
        Public Async Function LAltroveDellaCorrezioneDiUnPendingSiDichiara() As Task

            ' Anche la correzione di un frammento ripescato può contenere materiale di
            ' altre categorie: per la guardia anti-rimbalzo non torna nel magazzino, ma
            ' prima spariva in silenzio — ora si dichiara «lasciato fuori».
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).
                  Dara(FrContatti("{""formazione"": [""ho il diploma""]}")).
                  Dara(FrPatente("no")).
                  Dara(FrVuoto("esperienze_formali")).
                  Dara(FrVuoto("esperienze_informali")).
                  Dara(FrCompetenze("{}")).
                  Dara(FrFormazione("Diploma")).
                  Dara(FrFormazione("Diploma serale", "{""esperienze_formali"": [""e facevo il magazziniere""]}"))

            Dim dialogo As New DialogoProfilo(finto)
            Await dialogo.AvviaAsync()
            Await ConfermaAsync(dialogo, "Luca Ferrari")
            Await ConfermaAsync(dialogo, "luca@example.it, e ho il diploma")
            Await ConfermaAsync(dialogo, "No")
            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Niente")
            Await dialogo.ScegliAsync(Scelte.Oltre)

            ' Ingresso della formazione: il diploma parcheggiato si ripesca, ma
            ' l'utente preferisce riscriverlo — e nella correzione dice anche altro.
            Await dialogo.ScegliAsync(Scelte.Correggi)
            Dim mossa As Mossa = Await dialogo.RispondiAsync("Il diploma serale, e facevo il magazziniere")

            Assert.Contains("Aggiornato e aggiunto", String.Join(" ", mossa.Detto), "la correzione entra")
            Assert.Contains("lascio fuori", String.Join(" ", mossa.Detto), "e l'altrove si dichiara")
            Assert.Contains("facevo il magazziniere", String.Join(" ", mossa.Detto), "con le parole dell'utente")
            Assert.AreEqual("Diploma serale", dialogo.Profilo.Formazione(0).Titolo, "il titolo corretto")
            Assert.IsEmpty(dialogo.Profilo.EsperienzeFormali, "niente rimbalzi nel magazzino")

        End Function

        <TestMethod>
        Public Async Function LEcoVienePrimaDelVerdetto() As Task

            ' Quando il ripescaggio non colloca nulla, l'utente deve rivedere le
            ' proprie parole PRIMA di leggere «lo lascio fuori», non dopo: l'eco è
            ' ancorata al punto giusto della mossa.
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).
                  Dara(FrContatti("{""competenze"": [""boh, sono uno che si arrangia""]}")).
                  Dara(FrPatente("no")).
                  Dara(FrVuoto("esperienze_formali")).
                  Dara(FrVuoto("esperienze_informali")).
                  Dara(FrVuoto("competenze"))

            Dim dialogo As New DialogoProfilo(finto)
            Await PrimiTreTurniAsync(dialogo)
            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Nessuna")
            Dim mossa As Mossa = Await dialogo.ScegliAsync(Scelte.Oltre)

            Assert.HasCount(1, mossa.Echi, "un'eco sola")
            Assert.Contains("si arrangia", mossa.Echi(0).Testo, "con le parole dell'utente")

            Dim dopo As Integer = mossa.Echi(0).DopoDetti
            Assert.IsLessThan(mossa.Detto.Count, dopo, "ancorata prima dell'ultima bolla")
            Assert.Contains("avevi accennato", mossa.Detto(dopo - 1), "dopo l'annuncio del recupero")
            Assert.Contains("lascio fuori", mossa.Detto(dopo), "e prima del verdetto")

        End Function

        <TestMethod>
        Public Async Function NellaPassataFinaleOgniRecuperoHaLaSuaEco() As Task

            ' Due categorie parcheggiate alla passata finale, e la prima non si
            ' colloca: la mossa prosegue sulla seconda. Prima l'eco era un campo
            ' singolo e la seconda cancellava la prima: le parole dell'utente del
            ' primo gruppo non comparivano mai.
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).Dara(FrContatti()).Dara(FrPatente("no")).
                  Dara(FrVuoto("esperienze_formali")).
                  Dara(FrVuoto("esperienze_informali")).
                  Dara(FrCompetenze("{}")).
                  Dara(FrFormazione("Diploma",
                       "{""esperienze_formali"": [""tre anni in magazzino""], ""competenze"": [""so arrangiarmi""]}")).
                  Dara(FrVuoto("esperienze_formali")).
                  Dara(FrCompetenze("{}", "Sapersi arrangiare"))

            Dim dialogo As New DialogoProfilo(finto)
            Await PrimiTreTurniAsync(dialogo)
            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Niente")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await ConfermaAsync(dialogo, "Il diploma; tre anni in magazzino, so arrangiarmi")
            Dim mossa As Mossa = Await dialogo.ScegliAsync(Scelte.Procedi)

            Assert.HasCount(2, mossa.Echi, "un'eco per ciascun recupero")
            Assert.Contains("tre anni in magazzino", mossa.Echi(0).Testo, "le parole del primo gruppo")
            Assert.Contains("so arrangiarmi", mossa.Echi(1).Testo, "e quelle del secondo")
            Assert.IsLessThan(mossa.Echi(1).DopoDetti, mossa.Echi(0).DopoDetti,
                              "ciascuna ancorata al suo punto")
            Assert.AreEqual(TipoMossa.ChiediScelta, mossa.Tipo, "e il secondo recupero chiede conferma")

        End Function

        <TestMethod>
        Public Async Function IlDialogoSiAvviaUnaVoltaSola() As Task

            ' Riavviare la stessa istanza accumulerebbe i dati del giro precedente:
            ' per ricominciare se ne crea uno nuovo (è ciò che fa il pannello).
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome)

            Dim dialogo As New DialogoProfilo(finto)
            Await dialogo.AvviaAsync()

            Await Assert.ThrowsAsync(Of InvalidOperationException)(
                Function() dialogo.AvviaAsync())

            Dim vergine As New DialogoProfilo(New StrutturatoreFinto())
            Await Assert.ThrowsAsync(Of InvalidOperationException)(
                Function() vergine.RispondiAsync("una risposta prima dell'avvio"))

        End Function

        ' ------------------------------------------------------------------
        ' I comportamenti particolari dei turni
        ' ------------------------------------------------------------------

        <TestMethod>
        Public Async Function LaPatenteSenzaCategoriaSiRichiedeUnaVoltaSola() As Task

            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).Dara(FrContatti()).
                  Dara(FrPatente("sì")).
                  Dara(FrPatente("sì")).
                  Dara(FrVuoto("esperienze_formali"))

            Dim dialogo As New DialogoProfilo(finto)
            Await dialogo.AvviaAsync()
            Await ConfermaAsync(dialogo, "Luca Ferrari")
            Await ConfermaAsync(dialogo, "luca@example.it")

            Await dialogo.RispondiAsync("Sì, ce l'ho")
            Dim mossa As Mossa = Await dialogo.ScegliAsync(Scelte.Conferma)

            Assert.AreEqual(TipoMossa.ChiediRisposta, mossa.Tipo, "si richiede la categoria")
            Assert.Contains("di che categoria", String.Join(" ", mossa.Detto), "la ri-domanda")

            ' Nemmeno stavolta la specifica: si prosegue senza, senza insistere.
            mossa = Await dialogo.RispondiAsync("Non me lo ricordo")

            Assert.Contains("proseguiamo senza specificarla", String.Join(" ", mossa.Detto), "si rinuncia")
            Assert.Contains("esperienze di lavoro vere e proprie", String.Join(" ", mossa.Detto),
                            "e si apre il turno dopo")
            Assert.IsEmpty(dialogo.Profilo.Patente.Categorie, "nessuna categoria")
            Assert.AreEqual("sì", dialogo.Profilo.Patente.Ha, "ma la patente resta dichiarata")

        End Function

        <TestMethod>
        Public Async Function IlTurnoRipetibileRaccoglieMoltePosizioni() As Task

            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).Dara(FrContatti()).Dara(FrPatente("no")).
                  Dara(FrLavoro("Magazziniere")).Dara(FrLavoro("Commesso")).
                  Dara(FrVuoto("esperienze_informali"))

            Dim dialogo As New DialogoProfilo(finto)
            Await PrimiTreTurniAsync(dialogo)

            Await ConfermaAsync(dialogo, "Ho fatto il magazziniere")
            Dim mossa As Mossa = Await dialogo.ScegliAsync(Scelte.Altra)
            Assert.Contains("Raccontami la prossima", String.Join(" ", mossa.Detto), "il giro riapre")

            Await ConfermaAsync(dialogo, "E poi il commesso")
            Await dialogo.ScegliAsync(Scelte.Procedi)

            Assert.HasCount(2, dialogo.Profilo.EsperienzeFormali, "due esperienze")
            Assert.AreEqual("Commesso", dialogo.Profilo.EsperienzeFormali(1).Ruolo, "la seconda")

        End Function

        <TestMethod>
        Public Async Function LeCompetenzeHannoUnSoloGiroDiAggiunta() As Task

            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).Dara(FrContatti()).Dara(FrPatente("no")).
                  Dara(FrVuoto("esperienze_formali")).Dara(FrVuoto("esperienze_informali")).
                  Dara(FrCompetenze("{}", "Uso del muletto")).
                  Dara(FrCompetenze("{}", "Lettura delle bolle"))

            Dim dialogo As New DialogoProfilo(finto)
            Await PrimiTreTurniAsync(dialogo)
            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)
            Await dialogo.RispondiAsync("Nessuna")
            Await dialogo.ScegliAsync(Scelte.Oltre)

            Await dialogo.RispondiAsync("So usare il muletto")
            Dim mossa As Mossa = Await dialogo.ScegliAsync(Scelte.Aggiungi)
            Assert.AreEqual(TipoMossa.ChiediRisposta, mossa.Tipo, "si chiede cosa aggiungere")

            mossa = Await dialogo.RispondiAsync("So leggere le bolle")

            Assert.HasCount(2, dialogo.Profilo.Competenze, "le competenze si sommano")
            Assert.HasCount(1, mossa.Scelte, "e il giro di aggiunta è uno solo")
            Assert.AreEqual(Scelte.Conferma, mossa.Scelte(0).Id, "resta solo da confermare")

        End Function

        <TestMethod>
        Public Async Function QuandoNonSiColgeNullaSiPuoRiprovareOPassare() As Task

            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).Dara(FrContatti()).Dara(FrPatente("no")).
                  Dara(FrVuoto("esperienze_formali")).
                  Dara(FrLavoro("Magazziniere"))

            Dim dialogo As New DialogoProfilo(finto)
            Await PrimiTreTurniAsync(dialogo)

            Dim mossa As Mossa = Await dialogo.RispondiAsync("Mah, non saprei")
            Assert.Contains("Non ho colto un'esperienza di lavoro", String.Join(" ", mossa.Detto), "lo dice")
            Assert.AreEqual("riprova, oltre", String.Join(", ", mossa.Scelte.Select(Function(s) s.Id)),
                            "e offre le due strade")

            mossa = Await dialogo.ScegliAsync(Scelte.Riprova)
            Assert.AreEqual(TipoMossa.ChiediRisposta, mossa.Tipo, "si riprova")

            Await ConfermaAsync(dialogo, "Ho fatto il magazziniere")
            Assert.HasCount(1, dialogo.Profilo.EsperienzeFormali, "e stavolta entra")

        End Function

        <TestMethod>
        Public Async Function SeLAiNonRispondeSiPuoRiprovare() As Task

            ' Cap. 02.5: l'errore arriva all'utente in italiano, con la possibilità di
            ' riprovare. Il dialogo non deve cadere né perdere il punto in cui era.
            Dim finto As New StrutturatoreFinto
            finto.Fallira(New ErroreAi(CausaErroreAi.Rete,
                                       "Non riesco a raggiungere l'AI: controlla la connessione a Internet.")).
                  Dara(FrNome)

            Dim dialogo As New DialogoProfilo(finto)
            Await dialogo.AvviaAsync()

            Dim mossa As Mossa = Await dialogo.RispondiAsync("Luca Ferrari")

            Assert.AreEqual(TipoMossa.ChiediRisposta, mossa.Tipo, "si resta sulla stessa domanda")
            Assert.Contains("controlla la connessione", String.Join(" ", mossa.Detto), "col messaggio in italiano")
            Assert.IsFalse(dialogo.Finito, "il dialogo non è finito")

            Await ConfermaAsync(dialogo, "Luca Ferrari")
            Assert.AreEqual("Luca Ferrari", dialogo.Profilo.Nome, "e si riprende da dov'era")

        End Function

        <TestMethod>
        Public Async Function UnaSceltaFuoriPostoEUnErroreDiChiamante() As Task

            ' Il pannello non può inventarsi scelte: se lo fa è un errore di programma,
            ' e va detto subito invece di far avanzare il dialogo per caso.
            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome)

            Dim dialogo As New DialogoProfilo(finto)
            Await dialogo.AvviaAsync()
            Await dialogo.RispondiAsync("Luca Ferrari")

            Await Assert.ThrowsAsync(Of ArgumentException)(
                Function() dialogo.ScegliAsync(Scelte.Scarta))

            Await Assert.ThrowsAsync(Of InvalidOperationException)(
                Function() dialogo.RispondiAsync("una risposta quando ci vuole una scelta"))

        End Function

        ' ------------------------------------------------------------------
        ' Servizi dei collaudi
        ' ------------------------------------------------------------------

        ''' <summary>Risponde e conferma: il gesto più frequente del dialogo.</summary>
        Private Shared Async Function ConfermaAsync(dialogo As DialogoProfilo, testo As String) As Task(Of Mossa)
            Await dialogo.RispondiAsync(testo)
            Return Await dialogo.ScegliAsync(Scelte.Conferma)
        End Function

        ''' <summary>I tre turni singoli, sbrigati con risposte qualsiasi.</summary>
        Private Shared Async Function PrimiTreTurniAsync(dialogo As DialogoProfilo) As Task
            Await dialogo.AvviaAsync()
            Await ConfermaAsync(dialogo, "Luca Ferrari")
            Await ConfermaAsync(dialogo, "luca@example.it")
            Await ConfermaAsync(dialogo, "No, niente patente")
        End Function

        ''' <summary>Un dialogo portato fino in fondo, per guardare la chiusura.</summary>
        Private Shared Async Function FinoAllaFineAsync() As Task(Of DialogoProfilo)

            Dim finto As New StrutturatoreFinto
            finto.Dara(FrNome).Dara(FrContatti()).Dara(FrPatente("sì", "B")).
                  Dara(FrLavoro("Magazziniere")).Dara(FrInformale("Traslochi")).
                  Dara(FrCompetenze("{}", "Uso del muletto")).Dara(FrFormazione("Diploma"))

            Dim dialogo As New DialogoProfilo(finto)
            Await PrimiTreTurniAsync(dialogo)
            Await ConfermaAsync(dialogo, "Magazziniere")
            Await dialogo.ScegliAsync(Scelte.Procedi)
            Await ConfermaAsync(dialogo, "Traslochi")
            Await dialogo.ScegliAsync(Scelte.Procedi)
            Await dialogo.RispondiAsync("Muletto")
            Await dialogo.ScegliAsync(Scelte.Conferma)
            Await ConfermaAsync(dialogo, "Diploma")

            Return dialogo

        End Function

        ''' <summary>L'ultima mossa: quella che chiude il dialogo col riepilogo.</summary>
        Private Shared Function UltimaMossaAsync(dialogo As DialogoProfilo) As Task(Of Mossa)
            Return dialogo.ScegliAsync(Scelte.Procedi)
        End Function

    End Class

End Namespace
