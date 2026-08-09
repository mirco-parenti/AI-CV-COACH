Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace NonRegressione

    ''' <summary>
    ''' La terza gamba del collaudo di tappa di T3 (cap. 14): il <b>dialogo guidato da
    ''' zero</b>, condotto con l'AI vera dal primo turno al profilo salvato.
    ''' </summary>
    ''' <remarks>
    ''' <para>Le altre due gambe guardano l'import di un CV che esiste già
    ''' (<see cref="CollaudiImportReale"/>, <see cref="CollaudiFormatiReale"/>). Questa
    ''' guarda l'altra porta del profilo, quella di chi un CV non ce l'ha: i sette turni,
    ''' la scheda di conferma prima di ogni cosa che entra, e i due meccanismi che il
    ''' banco senza rete prova su frammenti preparati e che qui devono reggere davanti a
    ''' un modello vero — l'<b>anti-perdita</b> (ciò che l'utente dice nel turno sbagliato
    ''' viene ripescato al turno giusto) e il <b>«lasciato fuori»</b> (ciò che nessuna
    ''' sezione sa accogliere si dichiara, non si perde in silenzio).</para>
    ''' <para><b>La persona è inventata</b>, come il candidato di T2 e per la stessa
    ''' ragione: il repo è pubblico. Ed è meglio così anche nel merito — una traccia
    ''' inventata si costruisce apposta perché i due meccanismi scattino, cosa che un
    ''' racconto vero farebbe solo per caso. Siccome nel rapporto non ci sono dati di
    ''' nessuno, questo rapporto <b>sta nel repo</b> (<c>casi/reale/</c>), a differenza di
    ''' quelli delle altre due gambe, che stanno accanto al CV.</para>
    ''' <para><b>Dove sta il pass/fail.</b> Non su ciò che il modello decide — quello
    ''' cambia da un giro all'altro ed è la lezione di T2 — ma su ciò che il <i>dialogo</i>
    ''' deve fare comunque il modello risponda: percorrere i sette turni nel loro ordine,
    ''' non chiedere mai una conferma al buio, rendere conto di <b>ogni</b> frammento che
    ''' l'AI ha instradato altrove, non mettere nel profilo parole che l'utente non ha
    ''' detto, arrivare in fondo, e consegnare un profilo che si salva e si rilegge
    ''' identico. Quale sezione abbia accolto che cosa, e quante voci ne siano uscite, si
    ''' legge nel rapporto.</para>
    ''' <para>Il <b>«lasciato fuori»</b> resta una segnalazione anche quando la traccia lo
    ''' cerca apposta: se scatti dipende da come il modello legge una frase, e pretenderlo
    ''' darebbe un collaudo che lampeggia. Il suo pass/fail sta nel banco senza rete
    ''' (<c>CollaudiDialogoProfilo</c>), dove il frammento che nessuno sa collocare è
    ''' preparato a mano; qui si guarda se il caso si presenta davvero e come viene
    ''' detto.</para>
    ''' <para>Categoria <b>Reale</b>: vuole la chiave, e per il solo turno di parità anche
    ''' il prototipo acceso. Si lancia da <c>VB.NET/src</c> con
    ''' <c>dotnet test --settings TrovaLavoro.Collaudi/collaudi-reali.runsettings</c>.
    ''' Il CV di prova qui <b>non serve</b>: la traccia è nel codice.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiDialogoReale

        ''' <summary>Il rapporto: nel repo, perché la persona è inventata.</summary>
        Private Const NomeRapporto As String = "dialogo_guidato.md"

        ''' <summary>L'esito del turno chiesto anche al prototipo.</summary>
        Private Const NomeParita As String = "dialogo_turno_formali.json"

        ''' <summary>
        ''' Quante mosse al massimo può durare il dialogo prima che si debba concludere
        ''' che non converge. La traccia ne prevede una trentina: il doppio è largo, e
        ''' serve solo a non lasciare un collaudo a girare all'infinito se un giorno la
        ''' guardia anti-rimbalzo si rompesse.
        ''' </summary>
        Private Const MosseMassime As Integer = 80

        ''' <summary>La risposta di ripiego se il dialogo richiede la categoria della patente.</summary>
        ''' <remarks>
        ''' La traccia la dichiara già («la patente B»), quindi la ri-domanda non dovrebbe
        ''' arrivare. Se arriva è il modello che non ha colto la categoria: si risponde e
        ''' si annota fra le stranezze del rapporto, invece di mandare al posto suo la
        ''' battuta del turno dopo e sballare tutto il resto.
        ''' </remarks>
        Private Const RipiegoCategoria As String = "La B."

        ' ==================================================================
        ' La traccia: una persona inventata, e quattro trappole di proposito
        ' ==================================================================

        ''' <summary>
        ''' Un turno della traccia: le risposte che l'utente darebbe, nell'ordine. Più di
        ''' una vuol dire che al giro «ne hai un'altra?» si risponde di sì.
        ''' </summary>
        Private Class Gruppo
            Public Property Turno As String
            Public Property Battute As New List(Of String)
        End Class

        ''' <summary>
        ''' Le sette risposte di Anna Ricci, che non esiste. Quattro pezzi sono messi lì
        ''' apposta per finire nel turno sbagliato — è l'unico modo di far scattare
        ''' l'anti-perdita con un modello vero invece che con frammenti preparati:
        ''' <list type="bullet">
        ''' <item>il <b>corso senza nome</b>, detto ai contatti: appartiene alla formazione,
        ''' che verrà cinque turni dopo (instradamento in avanti);</item>
        ''' <item>il <b>patentino del muletto</b>, detto alla patente: è una qualifica, e
        ''' anche lui aspetta il turno della formazione;</item>
        ''' <item>il <b>posto fisso rifiutato</b>, detto alle competenze: parla di lavoro,
        ''' ma il turno dei lavori è già passato (instradamento all'indietro) — e un'offerta
        ''' che non si è accettata non è un'esperienza, quindi è il candidato naturale al
        ''' «lasciato fuori»;</item>
        ''' <item>il <b>chiosco della sorella</b>, detto alla formazione: un'esperienza
        ''' informale nell'ultimo turno, da recuperare nella passata finale.</item>
        ''' </list>
        ''' Il resto è una storia normale, con due lavori, un volontariato, quattro
        ''' competenze e un titolo di studio: senza una storia intera non si vedrebbe se il
        ''' profilo finale sta in piedi.
        ''' </summary>
        Private Shared ReadOnly Traccia As New List(Of Gruppo) From {
            New Gruppo With {
                .Turno = "nome",
                .Battute = New List(Of String) From {
                    "Buongiorno, mi chiamo Anna Ricci."}},
            New Gruppo With {
                .Turno = "contatti",
                .Battute = New List(Of String) From {
                    "anna.ricci@example.it, il numero è 340 1122334, abito a Forlì in via del " &
                    "Mulino 12. Ah, e ho fatto anche un corso, ma non mi ricordo più né quale né dove."}},
            New Gruppo With {
                .Turno = "patente",
                .Battute = New List(Of String) From {
                    "Sì, ho la patente B. Ho anche il patentino per il muletto, se può servire."}},
            New Gruppo With {
                .Turno = "esperienze_formali",
                .Battute = New List(Of String) From {
                    "Per cinque anni ho fatto le pulizie al supermercato Il Gabbiano di Forlì, " &
                    "dal 2016 al 2021: pulivo i reparti e gli uffici la mattina presto, prima " &
                    "dell'apertura.",
                    "Poi dal 2021 lavoro come aiuto cuoca alla trattoria Da Vittorio, sempre a " &
                    "Forlì: preparo le verdure, gli antipasti e do una mano quando escono i piatti."}},
            New Gruppo With {
                .Turno = "esperienze_informali",
                .Battute = New List(Of String) From {
                    "Da tre anni do una mano alla sagra del mio paese: cucino per duecento " &
                    "persone insieme alle altre volontarie."}},
            New Gruppo With {
                .Turno = "competenze",
                .Battute = New List(Of String) From {
                    "So cucinare per tanta gente, sono veloce e precisa, e non mi pesa alzarmi " &
                    "presto. So usare le lavapavimenti industriali.",
                    "Ah, poi mi hanno anche offerto un posto fisso in trattoria, ma ho dovuto " &
                    "dire di no per via degli orari."}},
            New Gruppo With {
                .Turno = "formazione",
                .Battute = New List(Of String) From {
                    "Ho la licenza media e basta. Ah, e per due estati ho aiutato mia sorella " &
                    "nel suo chiosco di piadine in spiaggia."}}}

        ''' <summary>Un pezzo della traccia messo lì per finire nel turno sbagliato.</summary>
        Private Class Trappola

            ''' <summary>Una parola che si ritrova nella frase, per riconoscerla nel rapporto.</summary>
            Public Property Parola As String

            ''' <summary>Come si chiama nel rapporto.</summary>
            Public Property Come As String

            ''' <summary>In che turno l'utente l'ha detta.</summary>
            Public Property Detta As String

            ''' <summary>Dove dovrebbe finire.</summary>
            Public Property Attesa As String

        End Class

        ''' <summary>Le quattro trappole, per leggere nel rapporto dove sono finite.</summary>
        ''' <remarks>
        ''' Servono a <b>guardare</b>, non a pretendere: se il modello instrada il corso
        ''' alle competenze invece che alla formazione non è il dialogo a sbagliare, ed è
        ''' una cosa da leggere. Ciò che si pretende è che di ognuna, dovunque sia stata
        ''' mandata, il dialogo renda conto.
        ''' </remarks>
        Private Shared ReadOnly Trappole As New List(Of Trappola) From {
            New Trappola With {.Parola = "corso", .Come = "il corso senza nome",
                               .Detta = "contatti", .Attesa = "formazione"},
            New Trappola With {.Parola = "patentino", .Come = "il patentino del muletto",
                               .Detta = "patente", .Attesa = "formazione"},
            New Trappola With {.Parola = "posto fisso", .Come = "il posto fisso rifiutato",
                               .Detta = "competenze", .Attesa = "esperienze_formali"},
            New Trappola With {.Parola = "chiosco", .Come = "il chiosco della sorella",
                               .Detta = "formazione", .Attesa = "esperienze_informali"}}

        ''' <summary>I sette turni nell'ordine del prototipo: l'ordine è parte del disegno.</summary>
        Private Shared ReadOnly OrdineDeiTurni As String() =
            {"nome", "contatti", "patente", "esperienze_formali",
             "esperienze_informali", "competenze", "formazione"}

        ' ==================================================================
        ' Il collaudo
        ' ==================================================================

        ''' <summary>Una mossa del dialogo e ciò che il conduttore le ha risposto.</summary>
        Private Class Battito
            Public Property Mossa As Mossa
            Public Property Risposta As String
            Public Property Scelta As String
        End Class

        <TestMethod, TestCategory("Reale")>
        Public Async Function IlDialogoGuidatoCostruisceIlProfiloSenzaPerdereNiente() As Task

            Dim chiave As String = CollaudoReale.ChiaveOppureRinuncia()

            Dim libreria As LibreriaPrompt = CollaudoReale.PoolIntegrato()

            Using client As New ClientClaude(chiave)

                ' Lo strutturatore è quello vero, con la spia sopra: senza vedere gli
                ' «altrove» che il modello produce non si potrebbe giudicare l'anti-perdita.
                Dim spia As New StrutturatoreSpia(New StrutturatoreTurni(libreria, client))
                Dim dialogo As New DialogoProfilo(spia)

                Dim stranezze As New List(Of String)
                Dim battiti As List(Of Battito) = Await ConduciAsync(dialogo, stranezze)

                Dim profilo As Profilo = dialogo.Profilo
                Dim instradati As List(Of StrutturatoreSpia.Instradato) = spia.Altrove()
                Dim fuoriDalDialogo As List(Of StrutturatoreSpia.Instradato) =
                    spia.Altrove(soloCategorieDelDialogo:=False).
                         Where(Function(i) Not StrutturatoreSpia.Categorie.Contains(i.Verso)).ToList()

                ' Di ogni frammento instradato altrove il dialogo deve rendere conto: o lo
                ' ripesca e lo rimette in bocca all'utente, o dichiara di lasciarlo fuori.
                Dim nonOnorati As List(Of StrutturatoreSpia.Instradato) =
                    instradati.Where(Function(i) Not Onorato(i, battiti)).ToList()

                Dim inventate As CollaudoReale.Invenzioni =
                    CollaudoReale.ValoriFuoriDalTesto(profilo, TestoDellaTraccia(battiti))

                ' Il profilo salvato e riletto: è dove finisce il flusso B del cap. 12, e
                ' un dialogo che arriva in fondo ma non si conserva non serve a niente.
                Dim riletto As String = SalvaERileggi(profilo)

                ' Il rapporto si scrive PRIMA di giudicare: se un Assert ferma tutto, la
                ' prova di com'era andata resta su disco comunque.
                Dim dove As String = Scrivi(battiti, spia, profilo, instradati, fuoriDalDialogo,
                                            nonOnorati, inventate, stranezze)
                Console.WriteLine(Riassunto(dove, battiti, spia, profilo, instradati,
                                            nonOnorati, inventate, stranezze))

                ' --- Il dialogo arriva in fondo. -------------------------------------
                Assert.IsTrue(dialogo.Finito,
                    "il dialogo non è arrivato al riepilogo: vedi il rapporto per l'ultima mossa")
                Assert.AreEqual(TipoMossa.Fine, battiti.Last().Mossa.Tipo, "l'ultima mossa chiude")

                ' --- I sette turni, nel loro ordine. ---------------------------------
                ' Si guardano le prime volte che ciascun turno è stato chiesto: dopo, lo
                ' stesso prompt torna per smaltire i frammenti parcheggiati, ed è giusto
                ' che torni fuori ordine — è l'anti-perdita che lavora.
                Assert.AreEqual(String.Join(" → ", OrdineDeiTurni), PrimeVolte(spia),
                    "l'ordine dei sette turni del prototipo")

                ' --- Il conduttore ha risposto alle domande giuste. -------------------
                ' Prima di giudicare l'app bisogna sapere che il collaudo ha misurato ciò
                ' che dice di misurare: ogni battuta è scritta per un turno preciso.
                Dim allineamento As List(Of String) = AllineamentoRotto(spia)
                Assert.IsEmpty(allineamento,
                    "il conduttore è andato fuori passo, e questo giro non misura il dialogo " &
                    "ma sé stesso: " & String.Join(" · ", allineamento))

                ' --- Nessuna conferma al buio. ---------------------------------------
                For Each battito As Battito In battiti
                    If Not ChiedeConferma(battito.Mossa) Then Continue For
                    Assert.IsTrue(battito.Mossa.Detto.Count > 0 OrElse battito.Mossa.Schede.Count > 0,
                        "il dialogo ha chiesto una conferma senza mostrare niente da confermare")
                Next

                ' --- Anti-perdita: di ogni frammento instradato si rende conto. -------
                Assert.IsEmpty(nonOnorati.Select(Function(i) $"{i.Verso}: «{i.Frase}»").ToList(),
                    "materiale instradato a un altro turno e mai più ricomparso — né ripescato " &
                    "né dichiarato lasciato fuori: " &
                    String.Join(" · ", nonOnorati.Select(Function(i) $"{i.Verso}: «{i.Frase}»")))

                ' Se nessuna delle quattro trappole è scattata il collaudo non ha provato
                ' l'anti-perdita: non è un difetto dell'app, è la traccia da rifare.
                Assert.IsGreaterThan(0, instradati.Count,
                    "il modello non ha instradato niente ad altri turni: la traccia non fa più " &
                    "scattare l'anti-perdita e va rivista (le trappole sono in «Trappole»)")

                ' --- Anti-invenzione: nel profilo solo parole dell'utente. ------------
                Assert.IsEmpty(inventate.Gravi,
                    "nel profilo ci sono valori che nella traccia non compaiono: " &
                    String.Join(" · ", inventate.Gravi))

                ' --- Il profilo raccolto sta in piedi. --------------------------------
                CollaudiImportReale.VerificaProfiloPieno("dialogo", profilo)
                Assert.IsGreaterThan(0, profilo.EsperienzeInformali.Count,
                    "[dialogo] la sagra del paese è un'esperienza informale, e il turno la chiede")
                Assert.AreEqual("sì", profilo.Patente.Ha, "[dialogo] la patente dichiarata")
                Assert.Contains("B", CollaudoReale.Categorie(profilo), "[dialogo] la categoria")

                ' --- E si salva. ------------------------------------------------------
                Assert.AreEqual(profilo.ComeTesto(), riletto,
                    "il profilo riletto da disco non è identico a quello raccolto")

            End Using

        End Function

        ''' <summary>
        ''' Il turno delle esperienze formali chiesto anche al prototipo, con la
        ''' <b>stessa identica risposta</b>: è la prova di parità della gamba C.
        ''' </summary>
        ''' <remarks>
        ''' <para>Il dialogo intero non si rifà nel prototipo, e per due ragioni: la
        ''' macchina a stati è la sua, portata nel motore riga per riga e già collaudata
        ''' senza rete, e rifarla vorrebbe dire pilotare una pagina web da un collaudo. Ciò
        ''' che vale la pena confrontare è l'altro pezzo, quello che parla con l'AI: a
        ''' parità di prompt e di risposta, dal turno deve uscire la stessa esperienza.</para>
        ''' <para>Sta in un metodo suo e non dentro il dialogo perché il prototipo può
        ''' essere spento: così la sua rinuncia non porta via anche il collaudo del
        ''' dialogo, che il prototipo non lo usa affatto.</para>
        ''' </remarks>
        <TestMethod, TestCategory("Reale")>
        Public Async Function IlTurnoDelleEsperienzeFormaliDiceQuelloCheDiceIlPrototipo() As Task

            Dim chiave As String = CollaudoReale.ChiaveOppureRinuncia()

            Dim battuta As String = Traccia.First(Function(g) g.Turno = "esperienze_formali").Battute(0)

            Dim libreria As LibreriaPrompt = CollaudoReale.PoolIntegrato()

            Using client As New ClientClaude(chiave)

                Dim daApp As JsonObject = TryCast(
                    Await New StrutturatoreTurni(libreria, client).
                        StrutturaAsync("esperienze_formali", battuta), JsonObject)

                Dim daPrototipo As JsonObject =
                    Await CollaudoReale.StrutturaDalPrototipoAsync("esperienze_formali", battuta)

                Dim app As Profilo = TrovaLavoro.Dati.Profilo.DaJson(daApp)
                Dim prototipo As Profilo = TrovaLavoro.Dati.Profilo.DaJson(daPrototipo)

                Dim dove As String = ScriviParita(battuta, daApp, daPrototipo)
                Console.WriteLine(
                    $"Turno «esperienze_formali» a risposta identica — app: {app.EsperienzeFormali.Count} " &
                    $"voci, prototipo: {prototipo.EsperienzeFormali.Count}. Esito completo: {dove}")

                Assert.HasCount(1, app.EsperienzeFormali, "l'app ricava un'esperienza dalla risposta")
                Assert.HasCount(1, prototipo.EsperienzeFormali, "e il prototipo pure")

                ' L'azienda è il campo che il prompt ordina di copiare: lì una differenza
                ' non è varianza del modello. Ruolo e durata li può riformulare, e si
                ' leggono nel file dell'esito.
                Assert.AreEqual(CollaudoReale.Ripulito(prototipo.EsperienzeFormali(0).Azienda),
                                CollaudoReale.Ripulito(app.EsperienzeFormali(0).Azienda),
                                "il posto di lavoro dev'essere lo stesso dalle due parti")

            End Using

        End Function

        ' ==================================================================
        ' Il conduttore: chi risponde al posto dell'utente
        ' ==================================================================

        ''' <summary>
        ''' Porta il dialogo dall'inizio alla fine rispondendo con la traccia. Non segue
        ''' una sequenza di gesti preparata: <b>guarda la mossa</b> e decide, perché con
        ''' l'AI vera non si sa in anticipo se un turno coglierà qualcosa o dirà «non ho
        ''' colto», e un copione rigido si romperebbe al primo scarto.
        ''' </summary>
        ''' <param name="stranezze">Dove annotare ciò che la traccia non prevedeva.</param>
        Private Shared Async Function ConduciAsync(dialogo As DialogoProfilo,
                                                   stranezze As List(Of String)) As Task(Of List(Of Battito))

            Dim battiti As New List(Of Battito)

            Dim gruppo As Integer = 0
            Dim battuta As Integer = 0
            Dim ultima As String = Nothing
            Dim daRipetere As Boolean = False
            Dim riprovato As Boolean = False

            Dim mossa As Mossa = Await dialogo.AvviaAsync()

            Do
                Dim battito As New Battito With {.Mossa = mossa}
                battiti.Add(battito)

                If mossa.Tipo = TipoMossa.Fine Then Exit Do

                If battiti.Count > MosseMassime Then
                    stranezze.Add($"Il dialogo ha superato {MosseMassime} mosse senza chiudere: " &
                                  "interrotto qui.")
                    Exit Do
                End If

                If mossa.Tipo = TipoMossa.ChiediRisposta Then

                    Dim testo As String

                    If daRipetere Then
                        ' Si è scelto «riprovo»: si ridice la stessa cosa, non la prossima.
                        testo = ultima
                        daRipetere = False

                    ElseIf ChiedeLaCategoriaDellaPatente(mossa) Then
                        ' La traccia la dichiara già: se la si richiede, il modello non
                        ' l'aveva colta. Si risponde senza consumare la traccia.
                        stranezze.Add("Il modello non ha colto la categoria della patente dalla " &
                                      "risposta, e il dialogo l'ha richiesta.")
                        testo = RipiegoCategoria

                    Else
                        ' La prossima battuta: se il gruppo è finito, si passa al turno dopo.
                        While gruppo < Traccia.Count AndAlso battuta >= Traccia(gruppo).Battute.Count
                            gruppo += 1
                            battuta = 0
                        End While

                        If gruppo >= Traccia.Count Then
                            stranezze.Add("Il dialogo ha chiesto una risposta in più di quelle che " &
                                          "la traccia prevede: interrotto qui.")
                            Exit Do
                        End If

                        testo = Traccia(gruppo).Battute(battuta)
                        battuta += 1
                        riprovato = False
                    End If

                    ultima = testo
                    battito.Risposta = testo
                    mossa = Await dialogo.RispondiAsync(testo)

                Else

                    Dim offerte As List(Of String) = mossa.Scelte.Select(Function(s) s.Id).ToList()
                    Dim scelta As String

                    If offerte.Contains(Scelte.Riprova) Then
                        ' «Non ho colto niente»: si riprova una volta con le stesse parole,
                        ' poi si passa oltre — insistere non porterebbe da nessuna parte.
                        If riprovato Then
                            scelta = Scelte.Oltre
                            stranezze.Add($"Il turno non ha colto nulla nemmeno al secondo tentativo: " &
                                          $"«{Accorcia(ultima)}». Si è passato oltre.")
                        Else
                            scelta = Scelte.Riprova
                            riprovato = True
                            daRipetere = True
                            stranezze.Add($"Il turno non ha colto nulla da: «{Accorcia(ultima)}». " &
                                          "Si è riprovato con le stesse parole.")
                        End If

                    ElseIf offerte.Contains(Scelte.Aggiungi) AndAlso AncoraInQuestoGruppo(gruppo, battuta) Then
                        ' Le competenze: la traccia ne ha un secondo giro da aggiungere.
                        scelta = Scelte.Aggiungi

                    ElseIf offerte.Contains(Scelte.Altra) Then
                        ' Un turno ripetibile: un'altra voce se la traccia ce l'ha, se no avanti.
                        scelta = If(AncoraInQuestoGruppo(gruppo, battuta), Scelte.Altra, Scelte.Procedi)

                    ElseIf offerte.Contains(Scelte.Conferma) Then
                        ' Vale per le schede dei turni e per i frammenti ripescati: la
                        ' persona che si è raccontata bene conferma ciò che ha detto.
                        scelta = Scelte.Conferma

                    Else
                        stranezze.Add("Il dialogo ha offerto scelte che il conduttore non conosce: " &
                                      String.Join(", ", offerte) & ". Interrotto qui.")
                        Exit Do
                    End If

                    battito.Scelta = scelta
                    mossa = Await dialogo.ScegliAsync(scelta)

                End If

            Loop

            Return battiti

        End Function

        ''' <summary>Se nel turno in corso la traccia ha ancora qualcosa da dire.</summary>
        Private Shared Function AncoraInQuestoGruppo(gruppo As Integer, battuta As Integer) As Boolean

            Return gruppo < Traccia.Count AndAlso battuta < Traccia(gruppo).Battute.Count

        End Function

        ''' <summary>Se questa mossa è la ri-domanda della categoria della patente.</summary>
        ''' <remarks>
        ''' Si cerca <c>«Una cosa sola:»</c> e non <c>«di che categoria»</c>, che sembrerebbe
        ''' più naturale ma è la trappola in cui questo conduttore è caduto al primo giro:
        ''' quelle parole stanno <b>anche</b> nell'apertura del turno della patente, così il
        ''' conduttore ha risposto «La B.» alla domanda vera, si è tenuto in tasca la battuta
        ''' della traccia e ha fatto slittare di un turno tutte quelle dopo — con il collaudo
        ''' che restava verde perché il dialogo, dal canto suo, si era comportato bene.
        ''' Da lì nasce anche <see cref="AllineamentoRotto"/>: un conduttore fuori passo non
        ''' deve poter passare per un collaudo riuscito.
        ''' </remarks>
        Private Shared Function ChiedeLaCategoriaDellaPatente(mossa As Mossa) As Boolean

            Return mossa.Detto.Any(Function(d) d.Contains("Una cosa sola:"))

        End Function

        ''' <summary>
        ''' Le battute della traccia finite nel turno sbagliato: ognuna è scritta per un
        ''' turno preciso, e se il conduttore la manda a un altro il collaudo non sta più
        ''' misurando quello che dice di misurare.
        ''' </summary>
        ''' <remarks>
        ''' Non guarda il conduttore, guarda la <b>spia</b>: che turno abbia strutturato
        ''' quella risposta lo dice il dialogo, chiamando il prompt di quel turno. È il
        ''' secondo controllo, indipendente dal primo.
        ''' </remarks>
        Private Shared Function AllineamentoRotto(spia As StrutturatoreSpia) As List(Of String)

            Dim rotte As New List(Of String)

            For Each chiamata As StrutturatoreSpia.Chiamata In spia.Chiamate

                Dim suo As Gruppo = Traccia.FirstOrDefault(
                    Function(g) g.Battute.Any(Function(b) b = chiamata.Risposta))

                ' Non è una battuta della traccia: è un frammento ripescato, o una
                ' risposta di ripiego. Quelle vanno dove il dialogo decide.
                If suo Is Nothing Then Continue For

                If suo.Turno <> chiamata.Turno Then
                    rotte.Add($"«{Accorcia(chiamata.Risposta)}» è scritta per il turno " &
                              $"«{suo.Turno}» ed è finita nel turno «{chiamata.Turno}»")
                End If

            Next

            Return rotte

        End Function

        ''' <summary>Se questa mossa sta chiedendo di confermare qualcosa.</summary>
        Private Shared Function ChiedeConferma(mossa As Mossa) As Boolean

            Return mossa.Tipo = TipoMossa.ChiediScelta AndAlso
                   mossa.Scelte.Any(Function(s) s.Id = Scelte.Conferma)

        End Function

        ' ==================================================================
        ' I giudizi che non guardano il contenuto
        ' ==================================================================

        ''' <summary>
        ''' Se di un frammento instradato altrove il dialogo ha reso conto: o l'ha
        ''' ripescato rimettendolo in bocca all'utente (<c>EcoUtente</c>), o ha detto di
        ''' lasciarlo fuori. Il confronto è per sole lettere e cifre — il modello ricopia
        ''' le parole dell'utente ma può cambiare una virgola, e una virgola non è una
        ''' perdita.
        ''' </summary>
        Private Shared Function Onorato(instradato As StrutturatoreSpia.Instradato,
                                        battiti As List(Of Battito)) As Boolean

            Dim cercato As String = CollaudoReale.PerCercare(instradato.Frase)
            If cercato = "" Then Return True

            For Each battito As Battito In battiti

                If CollaudoReale.PerCercare(battito.Mossa.EcoUtente).Contains(cercato) Then Return True

                For Each detto As String In battito.Mossa.Detto
                    If Not detto.Contains("lascio fuori") Then Continue For
                    If CollaudoReale.PerCercare(detto).Contains(cercato) Then Return True
                Next

            Next

            Return False

        End Function

        ''' <summary>
        ''' I sette turni nell'ordine in cui sono stati chiesti la <b>prima</b> volta.
        ''' Le volte successive sono gli smaltimenti dell'anti-perdita, che per mestiere
        ''' tornano su un turno passato: contarle direbbe che l'ordine è rotto quando
        ''' invece sta funzionando.
        ''' </summary>
        Private Shared Function PrimeVolte(spia As StrutturatoreSpia) As String

            Dim visti As New List(Of String)

            For Each chiamata As StrutturatoreSpia.Chiamata In spia.Chiamate
                If Not visti.Contains(chiamata.Turno) Then visti.Add(chiamata.Turno)
            Next

            Return String.Join(" → ", visti)

        End Function

        ''' <summary>
        ''' Tutto ciò che l'utente ha detto, in un testo solo: è il pagliaio in cui
        ''' l'anti-invenzione cerca i valori del profilo. Il posto del CV, qui, lo prende
        ''' la traccia.
        ''' </summary>
        Private Shared Function TestoDellaTraccia(battiti As List(Of Battito)) As String

            Return String.Join(vbLf, battiti.Where(Function(b) b.Risposta IsNot Nothing).
                                             Select(Function(b) b.Risposta))

        End Function

        ''' <summary>
        ''' Salva il profilo in una cartella temporanea e lo rilegge: la fine del flusso B
        ''' del cap. 12. La cartella dati vera non si tocca, e quella di prova sparisce
        ''' comunque vada.
        ''' </summary>
        ''' <returns>Il profilo riletto da disco, in forma di testo.</returns>
        Private Shared Function SalvaERileggi(profilo As Profilo) As String

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "dialogo-reale-" & Guid.NewGuid().ToString("N"))
            Try
                Dim archivio As New ArchivioProfilo(New CartellaDati(radice))
                archivio.Salva(profilo)
                Return archivio.Carica().ComeTesto()
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Function

        ''' <summary>Una frase lunga ridotta a un promemoria leggibile.</summary>
        Private Shared Function Accorcia(testo As String) As String

            Dim pulito As String = CollaudoReale.Ripulito(testo)
            Return If(pulito.Length <= 60, pulito, pulito.Substring(0, 57) & "…")

        End Function

        ' ==================================================================
        ' Il rapporto
        ' ==================================================================

        ''' <summary>
        ''' Il rapporto da leggere a mano. Sta in <c>casi/reale/</c>, cioè <b>nel repo</b>:
        ''' la persona è inventata e non c'è niente da proteggere — al contrario, il
        ''' dialogo per intero è la parte che nessun Assert può giudicare.
        ''' </summary>
        ''' <returns>Dove è stato scritto.</returns>
        Private Shared Function Scrivi(battiti As List(Of Battito), spia As StrutturatoreSpia,
                                       profilo As Profilo,
                                       instradati As List(Of StrutturatoreSpia.Instradato),
                                       fuoriDalDialogo As List(Of StrutturatoreSpia.Instradato),
                                       nonOnorati As List(Of StrutturatoreSpia.Instradato),
                                       inventate As CollaudoReale.Invenzioni,
                                       stranezze As List(Of String)) As String

            Dim testo As New StringBuilder()

            testo.Append("# Collaudo di tappa T3 — il dialogo guidato da zero").Append(vbLf).Append(vbLf)
            testo.Append("*Rapporto generato da `CollaudiDialogoReale`. Sta nel repo perché la ").
                  Append("persona è inventata: Anna Ricci non esiste, e la sua storia è scritta ").
                  Append("apposta perché l'anti-perdita e il «lasciato fuori» abbiano occasione di ").
                  Append("scattare.*").Append(vbLf).Append(vbLf)
            testo.Append($"- **Quando**: {DateTime.Now:yyyy-MM-dd HH:mm}").Append(vbLf)
            testo.Append($"- **Mosse del dialogo**: {battiti.Count}").Append(vbLf)
            testo.Append($"- **Chiamate all'AI**: {spia.Chiamate.Count}").Append(vbLf)
            testo.Append($"- **Turni, in ordine di prima chiamata**: {PrimeVolte(spia)}").Append(vbLf).Append(vbLf)

            ' --- Le trappole ---------------------------------------------------------
            testo.Append("## Le quattro trappole, e dove sono finite").Append(vbLf).Append(vbLf)
            testo.Append("*Quattro pezzi della traccia sono detti nel turno sbagliato di proposito. ").
                  Append("Dove il modello li mandi è cosa da **leggere**, non da pretendere; ciò che ").
                  Append("si pretende è che di ognuno il dialogo renda conto — sotto, «reso conto».*").
                  Append(vbLf).Append(vbLf)

            testo.Append("| trappola | detta nel turno | attesa in | instradata a | reso conto |").Append(vbLf)
            testo.Append("|---|---|---|---|---|").Append(vbLf)

            For Each trappola As Trappola In Trappole

                Dim trovata As StrutturatoreSpia.Instradato = instradati.FirstOrDefault(
                    Function(i) CollaudoReale.PerCercare(i.Frase).
                                Contains(CollaudoReale.PerCercare(trappola.Parola)))

                Dim dove As String = If(trovata Is Nothing, "*(non instradata)*", $"`{trovata.Verso}`")
                Dim segno As String = If(trovata Is Nothing, "—",
                                         If(nonOnorati.Contains(trovata), "**≠**", "="))

                testo.Append($"| {trappola.Come} | `{trappola.Detta}` | `{trappola.Attesa}` | ").
                      Append($"{dove} | {segno} |").Append(vbLf)

            Next

            testo.Append(vbLf)
            testo.Append("*Una trappola «non instradata» non è un difetto dell'app: vuol dire che il ").
                  Append("modello ha letto quella frase come roba del turno in corso, o l'ha lasciata ").
                  Append("cadere. Se non ne resta più nessuna, la traccia va rifatta — il collaudo lo ").
                  Append("dice da sé.*").Append(vbLf).Append(vbLf)

            ' --- L'anti-perdita, frammento per frammento -----------------------------
            testo.Append("## Anti-perdita — ogni frammento instradato").Append(vbLf).Append(vbLf)

            If instradati.Count = 0 Then
                testo.Append("*Il modello non ha instradato niente ad altri turni.*").Append(vbLf).Append(vbLf)
            Else
                testo.Append("| dal turno | verso | le parole dell'utente | reso conto |").Append(vbLf)
                testo.Append("|---|---|---|---|").Append(vbLf)
                For Each instradato As StrutturatoreSpia.Instradato In instradati
                    testo.Append($"| `{instradato.Da}` | `{instradato.Verso}` | ").
                          Append($"{CollaudoReale.PerTabella(instradato.Frase)} | ").
                          Append(If(nonOnorati.Contains(instradato), "⛔", "=")).Append(" |").Append(vbLf)
                Next
                testo.Append(vbLf)
            End If

            If fuoriDalDialogo.Count > 0 Then
                testo.Append("**Instradato a categorie che nessun turno ripesca** — il dialogo ").
                      Append("smaltisce le quattro categorie del profilo; un frammento mandato ").
                      Append("altrove (per esempio a `patente`) non ha nessuno che lo vada a ").
                      Append("riprendere. Qui non è una perdita — quel turno la sua domanda la fa ").
                      Append("comunque — ma è una cosa da sapere:").Append(vbLf).Append(vbLf)
                For Each fuori As StrutturatoreSpia.Instradato In fuoriDalDialogo
                    testo.Append($"- ⚠️ da `{fuori.Da}` verso `{fuori.Verso}`: «{fuori.Frase}»").Append(vbLf)
                Next
                testo.Append(vbLf)
            End If

            ' --- Il «lasciato fuori» -------------------------------------------------
            Dim lasciatiFuori As List(Of String) = battiti.
                SelectMany(Function(b) b.Mossa.Detto).
                Where(Function(d) d.Contains("lascio fuori")).ToList()

            testo.Append("## Il «lasciato fuori»").Append(vbLf).Append(vbLf)
            testo.Append("*Il gemello dell'anti-perdita: ciò che nessuna sezione sa accogliere si ").
                  Append("dichiara. Qui è una **segnalazione** e non un pass/fail — che scatti ").
                  Append("dipende da come il modello legge una frase, e pretenderlo darebbe un ").
                  Append("collaudo che lampeggia. Il suo pass/fail sta nel banco senza rete, dove il ").
                  Append("frammento che nessuno sa collocare è preparato a mano.*").Append(vbLf).Append(vbLf)

            If lasciatiFuori.Count = 0 Then
                testo.Append("Non è scattato in questo giro: tutto ciò che era stato messo da parte ").
                      Append("ha trovato una sezione.").Append(vbLf).Append(vbLf)
            Else
                For Each riga As String In lasciatiFuori
                    testo.Append($"- {CollaudoReale.Ripulito(riga)}").Append(vbLf)
                Next
                testo.Append(vbLf)
            End If

            ' --- Il conduttore in passo ----------------------------------------------
            Dim allineamento As List(Of String) = AllineamentoRotto(spia)

            testo.Append("## Il conduttore ha risposto alle domande giuste?").Append(vbLf).Append(vbLf)
            testo.Append("*Ogni battuta della traccia è scritta per un turno preciso. Se una finisce ").
                  Append("in un altro turno, il giro non misura il dialogo: misura il conduttore che ").
                  Append("è andato fuori passo. A dirlo non è lui, è il prompt che il dialogo ha ").
                  Append("chiamato per strutturarla.*").Append(vbLf).Append(vbLf)

            If allineamento.Count = 0 Then
                testo.Append("Sì: ogni battuta è stata strutturata dal turno per cui è scritta.").
                      Append(vbLf).Append(vbLf)
            Else
                For Each riga As String In allineamento
                    testo.Append($"- ⛔ {riga}").Append(vbLf)
                Next
                testo.Append(vbLf)
            End If

            ' --- Le stranezze --------------------------------------------------------
            testo.Append("## Quello che la traccia non prevedeva").Append(vbLf).Append(vbLf)

            If stranezze.Count = 0 Then
                testo.Append("Niente: il dialogo è andato come la traccia si aspettava.").
                      Append(vbLf).Append(vbLf)
            Else
                For Each stranezza As String In stranezze
                    testo.Append($"- ⚠️ {stranezza}").Append(vbLf)
                Next
                testo.Append(vbLf)
            End If

            ' --- Anti-invenzione -----------------------------------------------------
            testo.Append("## Anti-invenzione").Append(vbLf).Append(vbLf)
            testo.Append("*I valori del profilo cercati dentro le parole che l'utente ha detto. ").
                  Append("`cosa_facevo` e le competenze restano fuori: il modello li riformula per ").
                  Append("mestiere, e a giudicarli è chi legge.*").Append(vbLf).Append(vbLf)
            CollaudoReale.Invenzione(testo, "dialogo", inventate, dove:="in quello che l'utente ha detto")

            ' --- Il profilo ----------------------------------------------------------
            testo.Append("## Il profilo raccolto").Append(vbLf).Append(vbLf)
            testo.Append("| sezione | voci |").Append(vbLf)
            testo.Append("|---|---|").Append(vbLf)
            testo.Append($"| esperienze formali | {profilo.EsperienzeFormali.Count} |").Append(vbLf)
            testo.Append($"| esperienze informali | {profilo.EsperienzeInformali.Count} |").Append(vbLf)
            testo.Append($"| competenze | {profilo.Competenze.Count} |").Append(vbLf)
            testo.Append($"| formazione | {profilo.Formazione.Count} |").Append(vbLf)
            testo.Append(vbLf)
            testo.Append("```json").Append(vbLf).Append(profilo.ComeTesto()).
                  Append(vbLf).Append("```").Append(vbLf).Append(vbLf)

            ' --- Il dialogo per intero -----------------------------------------------
            testo.Append("## Il dialogo, per intero").Append(vbLf).Append(vbLf)
            testo.Append("*Da leggere come lo leggerebbe la persona che l'ha fatto: è la parte che ").
                  Append("nessun Assert giudica.*").Append(vbLf).Append(vbLf)

            For Each battito As Battito In battiti
                Trascrivi(testo, battito)
            Next

            Dim cartella As String = Path.Combine(CasiDiCollaudo.Cartella, "reale")
            Directory.CreateDirectory(cartella)

            Dim doveScritto As String = Path.Combine(cartella, NomeRapporto)

            File.WriteAllText(doveScritto, testo.ToString(),
                              New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Return doveScritto

        End Function

        ''' <summary>Una mossa del dialogo, come si leggerebbe a schermo.</summary>
        Private Shared Sub Trascrivi(testo As StringBuilder, battito As Battito)

            For Each detto As String In battito.Mossa.Detto
                For Each riga As String In detto.Split(ChrW(10))
                    testo.Append("> ").Append(riga).Append(vbLf)
                Next
                testo.Append(">").Append(vbLf)
            Next

            If Not String.IsNullOrEmpty(battito.Mossa.EcoUtente) Then
                testo.Append($"> *(le tue parole)* «{battito.Mossa.EcoUtente}»").Append(vbLf).Append(">").Append(vbLf)
            End If

            For Each scheda As Scheda In battito.Mossa.Schede
                If Not String.IsNullOrEmpty(scheda.Titolo) Then
                    testo.Append($"> **{scheda.Titolo}**").Append(vbLf)
                End If
                For Each riga As RigaScheda In scheda.Righe
                    testo.Append("> - ").
                          Append(If(riga.Etichetta = "", "", riga.Etichetta & ": ")).
                          Append(riga.Valore).Append(vbLf)
                Next
                testo.Append(">").Append(vbLf)
            Next

            If battito.Risposta IsNot Nothing Then
                testo.Append(vbLf).Append($"**Anna:** {battito.Risposta}").Append(vbLf).Append(vbLf)
            ElseIf battito.Scelta IsNot Nothing Then
                Dim etichetta As String = battito.Mossa.Scelte.
                    Where(Function(s) s.Id = battito.Scelta).
                    Select(Function(s) s.Etichetta).FirstOrDefault()
                testo.Append(vbLf).Append($"**Anna:** *[{etichetta}]*").Append(vbLf).Append(vbLf)
            Else
                testo.Append(vbLf)
            End If

        End Sub

        ''' <summary>
        ''' L'esito del turno chiesto anche al prototipo, nella stessa forma dei rapporti
        ''' reali di T2: le due risposte grezze, per poterle rileggere.
        ''' </summary>
        Private Shared Function ScriviParita(battuta As String, daApp As JsonObject,
                                             daPrototipo As JsonObject) As String

            Dim rapporto As New JsonObject From {
                {"turno", "esperienze_formali"},
                {"quando", DateTime.Now.ToString("yyyy-MM-dd HH:mm")},
                {"che_cosa", "La stessa identica risposta strutturata dall'app e dal prototipo: " &
                             "è la prova di parità della gamba C del collaudo di tappa T3."},
                {"risposta", battuta},
                {"app", daApp?.DeepClone()},
                {"prototipo", daPrototipo?.DeepClone()}}

            Dim cartella As String = Path.Combine(CasiDiCollaudo.Cartella, "reale")
            Directory.CreateDirectory(cartella)

            Dim dove As String = Path.Combine(cartella, NomeParita)

            File.WriteAllText(dove,
                rapporto.ToJsonString(New JsonSerializerOptions With {
                    .WriteIndented = True,
                    .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}) & vbLf,
                New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Return dove

        End Function

        ''' <summary>I numeri che si possono dire a console, e dove sta il rapporto.</summary>
        Private Shared Function Riassunto(dove As String, battiti As List(Of Battito),
                                          spia As StrutturatoreSpia, profilo As Profilo,
                                          instradati As List(Of StrutturatoreSpia.Instradato),
                                          nonOnorati As List(Of StrutturatoreSpia.Instradato),
                                          inventate As CollaudoReale.Invenzioni,
                                          stranezze As List(Of String)) As String

            Dim lasciatiFuori As Integer = battiti.
                SelectMany(Function(b) b.Mossa.Detto).
                Count(Function(d) d.Contains("lascio fuori"))

            Dim testo As New StringBuilder()

            testo.AppendLine("Collaudo di tappa T3 — il dialogo guidato da zero, con l'AI vera.")
            testo.AppendLine($"Mosse {battiti.Count}, chiamate all'AI {spia.Chiamate.Count}.")
            testo.AppendLine($"Turni in ordine di prima chiamata: {PrimeVolte(spia)}")
            testo.AppendLine($"Profilo: formali {profilo.EsperienzeFormali.Count}, " &
                             $"informali {profilo.EsperienzeInformali.Count}, " &
                             $"competenze {profilo.Competenze.Count}, " &
                             $"formazione {profilo.Formazione.Count}.")
            testo.AppendLine($"Anti-perdita: {instradati.Count} frammenti instradati altrove, " &
                             $"{nonOnorati.Count} senza risposta (bocciano).")
            testo.AppendLine($"«Lasciato fuori» dichiarato {lasciatiFuori} volte (segnalazione).")
            testo.AppendLine($"Valori non ritrovati nella traccia: {inventate.Gravi.Count} gravi, " &
                             $"{inventate.DaGuardare.Count} da guardare.")
            testo.AppendLine($"Cose non previste dalla traccia: {stranezze.Count}.")
            testo.AppendLine($"Rapporto completo (nel repo, persona inventata): {dove}")

            Return testo.ToString()

        End Function

    End Class

End Namespace
