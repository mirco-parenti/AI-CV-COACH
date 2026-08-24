Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace Motore

    ''' <summary>Come si comporta un turno del dialogo.</summary>
    Friend Enum TipoTurno
        ''' <summary>Una risposta, un campo: nome, contatti, patente.</summary>
        Singolo
        ''' <summary>Una lista di voci, con il giro «un'altra o procediamo?».</summary>
        Ripetibile
        ''' <summary>Una lista di stringhe raccolta in blocco, con un solo giro di aggiunta.</summary>
        Blocco
    End Enum

    ''' <summary>
    ''' Un campo che pesa nel CV: se una voce lo lascia vuoto, vale una domanda in più.
    ''' </summary>
    ''' <remarks>
    ''' Non tutti i campi la meritano. Ci sono quelli che il CV non sa surrogare — il ruolo,
    ''' che è il titolo della riga; il «cosa facevo», che diventa la descrizione e che il
    ''' confronto legge per le mansioni; la durata, che risponde al requisito «X anni di
    ''' esperienza» — e ci sono quelli il cui vuoto non toglie niente a nessuno (l'azienda,
    ''' l'anno, l'istituto). Chiedere anche quelli allungherebbe il dialogo senza rendere.
    ''' </remarks>
    Friend Class CampoCheConta

        ''' <summary>La chiave del campo nel frammento JSON del turno.</summary>
        Public Property Chiave As String

        ''' <summary>Come si dice dentro la frase: «non mi hai detto <b>quanto è durato</b>».</summary>
        Public Property ComeChiederlo As String

    End Class

    ''' <summary>Un turno: cosa chiede, come si comporta, cosa dice quando le cose vanno storte.</summary>
    Friend Class Turno
        Public Property Chiave As String
        Public Property Tipo As TipoTurno
        Public Property Apertura As String
        Public Property Riapertura As String
        Public Property Ponte As String
        Public Property TestoVuoto As String
        Public Property TestoRiprova As String
        Public Property TestoCorrezione As String
        Public Property Campi As New List(Of RigaScheda)

        ''' <summary>I campi che, restando vuoti, valgono la domanda di approfondimento.</summary>
        Public Property CampiCheContano As New List(Of CampoCheConta)

        ''' <summary>
        ''' I campi da cui prendere il nome con cui chiamare la voce, in ordine di
        ''' preferenza: si usa il primo compilato.
        ''' </summary>
        Public Property CampiDelNome As String() = {}

        ''' <summary>Come si nomina la voce quando un nome c'è: «Del lavoro «{0}»».</summary>
        Public Property VoceConNome As String

        ''' <summary>Come si nomina quando nessun campo del nome è compilato.</summary>
        Public Property VoceSenzaNome As String

    End Class

    ''' <summary>
    ''' Il dialogo guidato che costruisce il profilo (cap. 12, flusso B): sette turni,
    ''' una scheda di conferma prima di ogni cosa che entra, e la convenzione
    ''' <b>anti-perdita</b> — ciò che l'utente dice nel turno sbagliato non si perde, si
    ''' parcheggia e si ripropone al turno giusto.
    ''' </summary>
    ''' <remarks>
    ''' <para>È la macchina a stati del prototipo (<c>HTML+JS/index.html</c>) portata nel
    ''' motore: stessa logica, stessi testi, nessuna riga di interfaccia. Là il flusso
    ''' era fatto di funzioni che disegnavano da sé la pagina; qui ogni passo produce una
    ''' <see cref="Mossa"/> e aspetta che chi la mostra torni con una risposta o una
    ''' scelta. È ciò che permette di collaudare il dialogo intero senza interfaccia e
    ''' senza rete.</para>
    ''' <para>Le tre regole che tengono in piedi l'anti-perdita, prese dal prototipo:</para>
    ''' <list type="number">
    ''' <item><b>Instradamento in avanti</b>: ciò che appartiene a un turno non ancora
    ''' aperto si parcheggia e viene ripescato all'ingresso di quel turno.</item>
    ''' <item><b>Instradamento all'indietro</b>: ciò che appartiene a un turno già
    ''' passato si recupera nella passata finale, prima del riepilogo.</item>
    ''' <item><b>Guardia anti-rimbalzo</b>: un frammento parcheggiato viene tentato
    ''' <b>una volta sola</b>. Ciò che il turno di destinazione non sa strutturare non
    ''' torna nel magazzino — si dichiara «lasciato fuori». Senza questa regola due
    ''' categorie potrebbero rimpallarselo all'infinito e la passata finale non
    ''' convergerebbe mai.</item>
    ''' </list>
    ''' <para>Alla stessa passata finale è appesa la <b>ripresa delle domande
    ''' saltate</b>: un turno chiuso con «passiamo oltre» non è perduto per sempre, e
    ''' prima del riepilogo viene riofferto — una volta sola, chiedendo il permesso, e
    ''' soltanto se nel frattempo nessun frammento recuperato l'ha già riempito. Dove
    ''' l'anti-perdita recupera <i>contenuto</i> detto nel turno sbagliato, questa
    ''' recupera una <i>domanda</i> a cui non si era risposto: sono cugine, non la stessa
    ''' cosa. Anche qui vale il tentativo unico, per lo stesso motivo — se una ripresa
    ''' potesse rimettere in elenco la propria domanda, il dialogo non finirebbe più.</para>
    ''' </remarks>
    Public Class DialogoProfilo

        ''' <summary>Cosa sta aspettando la macchina.</summary>
        Private Enum Attesa
            ''' <summary>Niente: il dialogo non è ancora stato avviato.</summary>
            NonCominciato
            RispostaTurno
            ConfermaSingolo
            CategoriaPatente
            NienteColto
            ConfermaVoci
            AltraVoce
            Approfondimento
            Competenze
            AggiuntaCompetenze
            ConfermaPending
            CorrezionePending
            RipresaDomanda
            Concluso
        End Enum

        ''' <summary>Dove si torna dopo aver smaltito i frammenti parcheggiati.</summary>
        Private Enum DopoPending
            ChiediIlTurno
            PassataFinale
        End Enum

        ''' <summary>
        ''' Il pezzo fisso della domanda di approfondimento, in un posto solo perché serve
        ''' due volte: al dialogo per comporre la frase, e a chi conduce un dialogo di
        ''' collaudo per riconoscerla e non consumarci una battuta della sua traccia. Due
        ''' copie divergerebbero, e un conduttore fuori passo non si vede — è la trappola
        ''' già pagata con «Una cosa sola:» della patente.
        ''' </summary>
        Public Const NonMiHaiDetto As String = "non mi hai detto"

        ' --- Le categorie del profilo che possono ricevere materiale «da altrove». ---
        ' L'ordine conta: è quello in cui la passata finale li ripesca.
        Private Const EsperienzeFormali As String = "esperienze_formali"
        Private Const EsperienzeInformali As String = "esperienze_informali"
        Private Const Competenze As String = "competenze"
        Private Const Formazione As String = "formazione"
        Private Const Nome As String = "nome"
        Private Const Contatti As String = "contatti"
        Private Const Patente As String = "patente"

        Private Shared ReadOnly Categorie As String() =
            {EsperienzeFormali, EsperienzeInformali, Competenze, Formazione}

        ''' <summary>
        ''' Le destinazioni che il magazzino accetta: le quattro categorie più la
        ''' patente, che il prompt dei contatti instrada esplicitamente («ho la patente
        ''' B» detta col domicilio). Prima il magazzino conosceva solo le categorie, e
        ''' quella promessa del prompt cadeva nel vuoto: perdita silenziosa.
        ''' </summary>
        Private Shared ReadOnly DestinazioniPending As String() =
            {EsperienzeFormali, EsperienzeInformali, Competenze, Formazione, Patente}

        ''' <summary>L'etichetta leggibile di una categoria, per i messaggi.</summary>
        Private Shared ReadOnly EtichetteCategoria As New Dictionary(Of String, String) From {
            {EsperienzeFormali, "esperienze di lavoro"},
            {EsperienzeInformali, "esperienze informali"},
            {Competenze, "competenze"},
            {Formazione, "studi e formazione"},
            {Patente, "patente"}}

        ''' <summary>
        ''' I sette turni, nell'ordine del prototipo. I testi sono i suoi, parola per
        ''' parola: sono stati provati su persone vere e non si riscrivono per gusto.
        ''' </summary>
        Private Shared ReadOnly Turni As New List(Of Turno) From {
            New Turno With {
                .Chiave = Nome,
                .Tipo = TipoTurno.Singolo,
                .Apertura =
                    "Ciao!" & vbLf &
                    "Ti chiederò sia le esperienze formali (lavori, studi, corsi), sia quelle informali " &
                    "(cose fatte per amici, famiglia, passioni): contano entrambe." & vbLf &
                    "Useremo le tue risposte — solo quelle — per preparare un CV su misura." & vbLf & vbLf &
                    "Per iniziare: come ti chiami?",
                .TestoCorrezione = "Va bene, riscrivimela come viene."},
            New Turno With {
                .Chiave = Contatti,
                .Tipo = TipoTurno.Singolo,
                .Apertura =
                    "Bene! Ora qualche dato pratico, che useremo così com'è per l'intestazione del CV." & vbLf &
                    "Scrivimeli pure anche tutti insieme: email, telefono, domicilio, e un eventuale link (LinkedIn o un tuo sito).",
                .TestoCorrezione = "Va bene, riscrivimela come viene."},
            New Turno With {
                .Chiave = Patente,
                .Tipo = TipoTurno.Singolo,
                .Apertura =
                    "Un'ultima cosa importante per il confronto con gli annunci: hai la patente di guida? " &
                    "Se sì, di che categoria (es. B)? Se ne hai più di una, indicale tutte.",
                .TestoCorrezione = "Va bene, riscrivimela come viene."},
            New Turno With {
                .Chiave = EsperienzeFormali,
                .Tipo = TipoTurno.Ripetibile,
                .Apertura =
                    "Partiamo dalle esperienze di lavoro vere e proprie: raccontamele con parole tue, come vengono. " &
                    "Gli studi e i corsi li vediamo dopo." & vbLf &
                    "Procediamo un lavoro alla volta." & vbLf & vbLf &
                    "* Qual è il primo che ti viene in mente?",
                .Riapertura = "Raccontami la prossima.",
                .Ponte = "Hai un'altra esperienza di lavoro da raccontarmi, o procediamo?",
                .TestoVuoto = "Non ho colto un'esperienza di lavoro in quello che hai scritto.",
                .CampiCheContano = New List(Of CampoCheConta) From {
                    New CampoCheConta With {.Chiave = "ruolo", .ComeChiederlo = "che ruolo avevi"},
                    New CampoCheConta With {.Chiave = "durata", .ComeChiederlo = "quanto è durato"},
                    New CampoCheConta With {.Chiave = "cosa_facevo", .ComeChiederlo = "cosa facevi"}},
                .CampiDelNome = {"ruolo", "azienda"},
                .VoceConNome = "Del lavoro «{0}»",
                .VoceSenzaNome = "Di quel lavoro",
                .TestoRiprova = "Va bene, raccontamela come viene.",
                .TestoCorrezione = "Va bene, raccontamela di nuovo.",
                .Campi = New List(Of RigaScheda) From {
                    New RigaScheda With {.Etichetta = "Ruolo", .Valore = "ruolo"},
                    New RigaScheda With {.Etichetta = "Azienda", .Valore = "azienda"},
                    New RigaScheda With {.Etichetta = "Durata", .Valore = "durata"},
                    New RigaScheda With {.Etichetta = "Cosa facevo", .Valore = "cosa_facevo"}}},
            New Turno With {
                .Chiave = EsperienzeInformali,
                .Tipo = TipoTurno.Ripetibile,
                .Apertura =
                    "Raccontami ora le esperienze informali: cose fatte senza che fossero un lavoro vero e proprio — " &
                    "aiutare un familiare con la sua attività, dare una mano in associazioni o eventi, " &
                    "una passione che ti ha insegnato qualcosa. Non c'è bisogno che sia ""importante""." & vbLf & vbLf &
                    "* Ti viene in mente qualcosa del genere?",
                .Riapertura = "Raccontami la prossima.",
                .Ponte = "Hai un'altra esperienza informale, o procediamo?",
                .TestoVuoto = "Non ho colto un'esperienza informale in quello che hai scritto.",
                .CampiCheContano = New List(Of CampoCheConta) From {
                    New CampoCheConta With {.Chiave = "cosa_facevo", .ComeChiederlo = "cosa facevi"}},
                .CampiDelNome = {"con_chi", "quando"},
                .VoceConNome = "Di quell'esperienza «{0}»",
                .VoceSenzaNome = "Di quell'esperienza",
                .TestoRiprova = "Va bene, raccontamela come viene.",
                .TestoCorrezione = "Va bene, raccontamela di nuovo.",
                .Campi = New List(Of RigaScheda) From {
                    New RigaScheda With {.Etichetta = "Cosa facevo", .Valore = "cosa_facevo"},
                    New RigaScheda With {.Etichetta = "Quando", .Valore = "quando"},
                    New RigaScheda With {.Etichetta = "Con chi", .Valore = "con_chi"}}},
            New Turno With {
                .Chiave = Competenze,
                .Tipo = TipoTurno.Blocco,
                .Apertura =
                    "Passiamo alle competenze, cioè le cose che sai fare." & vbLf &
                    "Pensa a quello che mi hai raccontato finora — i lavori, le esperienze. " &
                    "Da tutto questo, cosa ti senti di saper fare? Anche cose pratiche e concrete." & vbLf & vbLf &
                    "* Cosa ti riesce bene?",
                .TestoVuoto = "Non ho colto competenze in quello che hai scritto.",
                .TestoRiprova = "Va bene, cosa ti riesce bene?"},
            New Turno With {
                .Chiave = Formazione,
                .Tipo = TipoTurno.Ripetibile,
                .Apertura =
                    "Siamo all'ultimo campo: gli studi e i corsi. Diplomi, qualifiche, corsi di formazione — " &
                    "tutto quello che hai studiato o imparato in modo strutturato." & vbLf &
                    "Procediamo come prima, uno alla volta." & vbLf & vbLf &
                    "* Qual è il primo che ti viene in mente?",
                .Riapertura = "Raccontami la prossima.",
                .Ponte = "Hai un'altra esperienza di studio o formazione, o abbiamo finito?",
                .TestoVuoto = "Non ho colto un titolo di studio o un corso in quello che hai scritto.",
                .CampiCheContano = New List(Of CampoCheConta) From {
                    New CampoCheConta With {.Chiave = "titolo", .ComeChiederlo = "che titolo o corso era"}},
                .CampiDelNome = {"istituto", "anno"},
                .VoceConNome = "Di quello che hai studiato «{0}»",
                .VoceSenzaNome = "Di quello che hai studiato",
                .TestoRiprova = "Va bene, raccontamela come viene.",
                .TestoCorrezione = "Va bene, raccontamela di nuovo.",
                .Campi = New List(Of RigaScheda) From {
                    New RigaScheda With {.Etichetta = "Titolo", .Valore = "titolo"},
                    New RigaScheda With {.Etichetta = "Istituto", .Valore = "istituto"},
                    New RigaScheda With {.Etichetta = "Anno", .Valore = "anno"}}}}

        Private ReadOnly _strutturatore As IStrutturatoreTurni

        ''' <summary>Il magazzino dei frammenti instradati ad altri turni.</summary>
        Private ReadOnly _pending As New Dictionary(Of String, List(Of String))

        ''' <summary>
        ''' Le domande chiuse senza aver raccolto niente, in attesa della seconda
        ''' occasione che la ripresa dà loro prima del riepilogo.
        ''' </summary>
        Private ReadOnly _saltate As New List(Of String)

        ''' <summary>Se il turno in corso è stato riaperto dalla ripresa.</summary>
        Private _inRipresa As Boolean

        ''' <summary>
        ''' Le voci appena confermate che aspettano la loro domanda di approfondimento:
        ''' sono indici dentro <see cref="_voci"/>, perché è lì che la voce si completa
        ''' prima di entrare nel profilo.
        ''' </summary>
        Private _daApprofondire As New List(Of Integer)

        ''' <summary>A quale voce si riferisce la domanda in corso.</summary>
        Private _voceApprofondita As Integer

        ''' <summary>Quali campi ha chiesto la domanda in corso.</summary>
        Private _campiChiesti As New List(Of CampoCheConta)

        Private _indice As Integer
        Private _attesa As Attesa
        Private _frammento As JsonObject
        Private _voci As JsonArray
        Private _destPending As String
        Private _vociPending As JsonArray
        Private _fuoriPending As New List(Of String)
        Private _dopoPending As DopoPending

        ''' <param name="strutturatore">Chi trasforma le risposte in frammenti JSON.</param>
        Public Sub New(strutturatore As IStrutturatoreTurni)

            If strutturatore Is Nothing Then Throw New ArgumentNullException(NameOf(strutturatore))
            _strutturatore = strutturatore

            For Each destinazione As String In DestinazioniPending
                _pending(destinazione) = New List(Of String)
            Next

        End Sub

        ''' <summary>Il profilo raccolto finora: si riempie turno dopo turno.</summary>
        Public ReadOnly Property Profilo As New Profilo

        ''' <summary>Se il dialogo è arrivato in fondo.</summary>
        Public ReadOnly Property Finito As Boolean
            Get
                Return _attesa = Attesa.Concluso
            End Get
        End Property

        ''' <summary>
        ''' Apre il dialogo dal primo turno. Si avvia una volta sola: un dialogo è una
        ''' conversazione con la sua storia, e riavviarlo sulla stessa istanza
        ''' accumulerebbe nel profilo i dati del giro precedente. Per ricominciare se ne
        ''' crea uno nuovo (è ciò che fa il pannello).
        ''' </summary>
        Public Function AvviaAsync(Optional annulla As CancellationToken = Nothing) As Task(Of Mossa)

            If _attesa <> Attesa.NonCominciato Then
                Throw New InvalidOperationException(
                    "Questo dialogo è già stato avviato: per ricominciare creane uno nuovo.")
            End If

            _indice = 0
            Return ApriTurnoAsync(New Mossa, annulla)

        End Function

        ''' <summary>Consegna alla macchina la risposta scritta dell'utente.</summary>
        Public Function RispondiAsync(testo As String,
                                      Optional annulla As CancellationToken = Nothing) As Task(Of Mossa)

            Select Case _attesa
                Case Attesa.RispostaTurno
                    Return RispostaDelTurnoAsync(testo, annulla)
                Case Attesa.CategoriaPatente
                    Return RispostaCategoriaPatenteAsync(testo, annulla)
                Case Attesa.AggiuntaCompetenze
                    Return RispostaAggiuntaCompetenzeAsync(testo, annulla)
                Case Attesa.CorrezionePending
                    Return RispostaCorrezionePendingAsync(testo, annulla)
                Case Attesa.Approfondimento
                    Return RispostaApprofondimentoAsync(testo, annulla)
                Case Else
                    Throw New InvalidOperationException(
                        "Il dialogo non sta aspettando una risposta scritta in questo momento.")
            End Select

        End Function

        ''' <summary>Consegna alla macchina la scelta dell'utente (v. <see cref="Scelte"/>).</summary>
        Public Function ScegliAsync(scelta As String,
                                    Optional annulla As CancellationToken = Nothing) As Task(Of Mossa)

            Select Case _attesa
                Case Attesa.ConfermaSingolo
                    Return ScegliSuSingoloAsync(scelta, annulla)
                Case Attesa.NienteColto
                    Return ScegliSuNienteColtoAsync(scelta, annulla)
                Case Attesa.ConfermaVoci
                    Return ScegliSuVociAsync(scelta, annulla)
                Case Attesa.AltraVoce
                    Return ScegliSuAltraVoceAsync(scelta, annulla)
                Case Attesa.Competenze
                    Return ScegliSuCompetenzeAsync(scelta, annulla)
                Case Attesa.ConfermaPending
                    Return ScegliSuPendingAsync(scelta, annulla)
                Case Attesa.RipresaDomanda
                    Return ScegliSuRipresaAsync(scelta, annulla)
                Case Else
                    Throw New InvalidOperationException(
                        "Il dialogo non sta aspettando una scelta in questo momento.")
            End Select

        End Function

        ' ==================================================================
        ' Il filo del dialogo: apertura dei turni, avanzamento, chiusura
        ' ==================================================================

        ''' <summary>Apre il turno corrente, o chiude il dialogo se sono finiti.</summary>
        Private Async Function ApriTurnoAsync(mossa As Mossa, annulla As CancellationToken) As Task(Of Mossa)

            If _indice >= Turni.Count Then Return Await PassataFinaleAsync(mossa, annulla).ConfigureAwait(False)

            Dim turno As Turno = Turni(_indice)
            mossa.Detto.Add(turno.Apertura)

            ' All'ingresso di un turno si smaltisce prima ciò che era stato messo da
            ' parte per lui: è l'instradamento in avanti. La patente ha la sua strada:
            ' è un turno singolo, e il parcheggiato passa dalla conferma del singolo.
            If _pending.ContainsKey(turno.Chiave) AndAlso _pending(turno.Chiave).Count > 0 Then
                If turno.Chiave = Patente Then
                    Return Await ApriPatenteDaPendingAsync(turno, mossa, annulla).ConfigureAwait(False)
                End If
                _dopoPending = DopoPending.ChiediIlTurno
                Return Await SmaltisciPendingAsync(turno.Chiave, mossa, annulla).ConfigureAwait(False)
            End If

            Return ChiediRisposta(mossa, Attesa.RispostaTurno)

        End Function

        ''' <summary>Passa al turno successivo, o chiude la ripresa in corso.</summary>
        Private Function AvanzaAsync(mossa As Mossa, annulla As CancellationToken) As Task(Of Mossa)

            ' Dentro una ripresa non c'è un turno dopo: il dialogo era già arrivato in
            ' fondo. Si torna alla passata finale, che smaltirà l'eventuale materiale
            ' nuovo finito nel magazzino e passerà poi alla domanda saltata successiva.
            ' È l'unico punto da deviare, perché tutte le uscite di un turno passano
            ' di qui: la conferma, il ponte «un'altra o procediamo», il passare oltre.
            If _inRipresa Then
                _inRipresa = False
                Return PassataFinaleAsync(mossa, annulla)
            End If

            _indice += 1
            Return ApriTurnoAsync(mossa, annulla)

        End Function

        ''' <summary>
        ''' Prima del riepilogo recupera, uno alla volta, i frammenti ancora parcheggiati
        ''' la cui destinazione è un turno già passato: è l'instradamento all'indietro.
        ''' Si scandiscono <b>tutte</b> le destinazioni del magazzino, patente compresa:
        ''' un residuo lì (un modello che disobbedisce alle categorie del prompt) esce
        ''' come «lasciato fuori» dichiarato, mai come perdita muta.
        ''' </summary>
        Private Async Function PassataFinaleAsync(mossa As Mossa, annulla As CancellationToken) As Task(Of Mossa)

            Dim dest As String = DestinazioniPending.FirstOrDefault(Function(c) _pending(c).Count > 0)
            If dest Is Nothing Then Return Await RiprendiSaltateAsync(mossa, annulla).ConfigureAwait(False)

            mossa.Detto.Add(
                "Prima di chiudere, recuperiamo una cosa che avevi accennato e non avevamo ancora registrato.")

            _dopoPending = DopoPending.PassataFinale
            Return Await SmaltisciPendingAsync(dest, mossa, annulla).ConfigureAwait(False)

        End Function

        ''' <summary>Il riepilogo leggibile del profilo raccolto, e fine.</summary>
        Private Function Riepilogo(mossa As Mossa) As Mossa

            mossa.Detto.Add(
                "Perfetto, abbiamo finito di costruire il tuo profilo." & vbLf &
                "Ecco un riepilogo di quello che ho raccolto:")

            mossa.Schede.Add(SchedaElenco("Nome", {If(String.IsNullOrEmpty(Profilo.Nome), "(non indicato)", Profilo.Nome)}))

            RiepilogaSezione(mossa, "Esperienze formali", EsperienzeFormali)
            RiepilogaSezione(mossa, "Esperienze informali", EsperienzeInformali)

            mossa.Schede.Add(SchedaElenco("Competenze",
                If(Profilo.Competenze.Count > 0, Profilo.Competenze, New List(Of String) From {"(nessuna indicata)"})))

            RiepilogaSezione(mossa, "Formazione", Formazione)

            mossa.Detto.Add(
                "Userò soltanto queste informazioni — niente di inventato — per aiutarti a " &
                "preparare CV e lettera su misura.")

            _attesa = Attesa.Concluso
            mossa.Tipo = TipoMossa.Fine
            Return mossa

        End Function

        ''' <summary>Una sezione a voci-oggetto del riepilogo: una scheda per voce.</summary>
        Private Sub RiepilogaSezione(mossa As Mossa, titolo As String, chiave As String)

            Dim voci As JsonArray = ProfiloComeVoci(chiave)

            If voci.Count = 0 Then
                mossa.Schede.Add(SchedaElenco(titolo, {"(nessuna indicata)"}))
                Return
            End If

            Dim turno As Turno = TrovaTurno(chiave)
            For i As Integer = 0 To voci.Count - 1
                mossa.Schede.Add(SchedaVoce(turno, voci(i), $"{titolo} {i + 1}"))
            Next

        End Sub

        ' ==================================================================
        ' I turni
        ' ==================================================================

        ''' <summary>La risposta dell'utente al turno corrente.</summary>
        Private Async Function RispostaDelTurnoAsync(testo As String, annulla As CancellationToken) As Task(Of Mossa)

            Dim turno As Turno = Turni(_indice)
            Dim mossa As New Mossa

            Dim frammento As JsonObject = Await StrutturaAsync(turno.Chiave, testo, mossa, annulla).ConfigureAwait(False)
            If frammento Is Nothing Then Return mossa   ' l'AI non ha risposto: si richiede la stessa cosa

            _frammento = frammento

            Select Case turno.Tipo

                Case TipoTurno.Singolo
                    DiciCosaHoCapito(mossa, turno, frammento, Profilo)
                    _attesa = Attesa.ConfermaSingolo
                    Return DueScelte(mossa, "Sì, è giusto", Scelte.Conferma, "Correggi", Scelte.Correggi)

                Case TipoTurno.Ripetibile
                    Dim voci As JsonArray = Elenco(frammento, turno.Chiave)
                    If voci.Count = 0 Then Return NienteColto(mossa, turno)

                    _voci = voci
                    mossa.Detto.Add("Ecco cosa ho capito:")
                    For Each voce As JsonNode In voci
                        mossa.Schede.Add(SchedaVoce(turno, voce, Nothing))
                    Next
                    _attesa = Attesa.ConfermaVoci
                    Return DueScelte(mossa, "Sì, è giusto", Scelte.Conferma, "Correggi", Scelte.Correggi)

                Case Else ' Blocco: le competenze
                    Dim voci As JsonArray = Elenco(frammento, turno.Chiave)
                    If voci.Count = 0 AndAlso Profilo.Competenze.Count = 0 Then Return NienteColto(mossa, turno)

                    AggiungiAlProfilo(turno.Chiave, voci)
                    RaccogliAltrove(frammento, Nothing)
                    MostraCompetenze(mossa)

                    _attesa = Attesa.Competenze
                    Return DueScelte(mossa, "Ne aggiungo altre", Scelte.Aggiungi, "Vanno bene", Scelte.Conferma,
                                     principaleELaSeconda:=True)

            End Select

        End Function

        ''' <summary>Niente da strutturare: si riprova o si passa oltre.</summary>
        Private Function NienteColto(mossa As Mossa, turno As Turno) As Mossa

            mossa.Detto.Add(turno.TestoVuoto)
            _attesa = Attesa.NienteColto
            Return DueScelte(mossa, "Riprovo", Scelte.Riprova, "Passiamo oltre", Scelte.Oltre)

        End Function

        Private Async Function ScegliSuNienteColtoAsync(scelta As String, annulla As CancellationToken) As Task(Of Mossa)

            Dim turno As Turno = Turni(_indice)
            Dim mossa As New Mossa

            Select Case scelta
                Case Scelte.Riprova
                    mossa.Detto.Add(turno.TestoRiprova)
                    Return ChiediRisposta(mossa, Attesa.RispostaTurno)
                Case Scelte.Oltre
                    ' Anche rinunciando, ciò che era per altri turni non si butta.
                    RaccogliAltrove(_frammento, Nothing)
                    ' E non si butta nemmeno la domanda: prima di chiudere si riofre.
                    SegnaSaltata(turno.Chiave)
                    Return Await AvanzaAsync(mossa, annulla).ConfigureAwait(False)
                Case Else
                    Throw SceltaSconosciuta(scelta)
            End Select

        End Function

        ' --- Turni singoli: nome, contatti, patente ---------------------------------

        ''' <summary>
        ''' Il «ho capito questo» dei turni singoli, con le parole del prototipo — più, dal
        ''' 2026-08-23, quel che <b>sparisce</b> confermando. I turni singoli sostituiscono il
        ''' blocco intero (v. <see cref="Unisci"/>), quindi un campo già confermato e non
        ''' ripetuto se ne va: prima lo faceva in silenzio, e chi correggeva la sola via
        ''' perdeva email e telefono senza che nessuno glielo dicesse. Si continua a
        ''' sostituire — è la regola che l'utente può prevedere — ma non più di nascosto.
        ''' </summary>
        Private Shared Sub DiciCosaHoCapito(mossa As Mossa, turno As Turno, frammento As JsonObject,
                                            adesso As Profilo)

            Select Case turno.Chiave

                Case Nome
                    ' Attenzione al nome della variabile: in VB le maiuscole non
                    ' distinguono, e un locale «nome» oscurerebbe la costante «Nome».
                    Dim letto As String = Testo(frammento, Nome)
                    mossa.Detto.Add(If(letto <> "",
                                       $"Ho capito che ti chiami: {letto}.",
                                       "Non ho colto un nome nella tua risposta."))

                Case Contatti
                    Dim righe As List(Of String) = RigheDeiContatti(Dati.Profilo.DaJson(frammento).Contatti)
                    mossa.Detto.Add(If(righe.Count > 0,
                                       "Ecco cosa ho capito:" & vbLf & String.Join(vbLf, righe),
                                       "Non ho colto nessun recapito nella tua risposta."))

                    Dim perduti As List(Of String) =
                        CampiCheSpariscono(RigheDeiContatti(adesso?.Contatti), righe)
                    If perduti.Count > 0 Then
                        mossa.Detto.Add(
                            "Attenzione: questi me li avevi già dati, e confermando spariscono, " &
                            "perché in questa risposta non ci sono:" & vbLf &
                            String.Join(vbLf, perduti) & vbLf &
                            "Se li vuoi tenere, scegli «Correggi» e ridimmeli insieme al resto.")
                    End If

                    ' Il secondo «lasciato fuori» del dialogo, e va tenuto distinto dal primo.
                    ' Quello di AltroveLasciatoFuori è materiale di ALTRI turni che nessuna
                    ' sezione ha saputo accogliere: lì si lascia fuori per resa. Questo è
                    ' materiale di QUESTO turno che il turno sceglie di non tenere — la via e il
                    ' civico di un indirizzo — e si lascia fuori di proposito. Le due ragioni
                    ' sono opposte, il vocabolario verso l'utente è lo stesso, e la frase dice
                    ' quale delle due è: né l'una né l'altra sparisce in silenzio.
                    Dim nonTenuto As String = Testo(frammento, "lasciato_fuori")
                    If nonTenuto <> "" Then
                        mossa.Detto.Add(
                            $"Questo l'ho lasciato fuori di proposito: «{nonTenuto}». Su un CV si " &
                            "scrive la città e non l'indirizzo di casa, perché finisce in mano a " &
                            "sconosciuti. Se per te conta, scegli «Correggi» e dimmelo.")
                    End If

                Case Else ' patente
                    Dim p As Profilo = Dati.Profilo.DaJson(frammento)
                    Dim detto As String
                    If p.Patente.Ha = "sì" Then
                        detto = "sì" & If(p.Patente.Categorie.Count > 0,
                                          $" ({String.Join(", ", p.Patente.Categorie)})", "")
                    ElseIf p.Patente.Ha = "no" Then
                        detto = "no"
                    Else
                        detto = "non indicata"
                    End If
                    mossa.Detto.Add("Ecco cosa ho capito:" & vbLf & "Patente: " & detto)

                    ' Confermando, una patente non colta vale «no» (v. Unisci): è la scelta del
                    ' prototipo e resta, ma chi l'aveva già dichiarata deve vederselo dire —
                    ' la patente è spesso il requisito eliminatorio di un annuncio.
                    If adesso IsNot Nothing AndAlso adesso.Patente.Ha = "sì" AndAlso p.Patente.Ha <> "sì" Then
                        mossa.Detto.Add(
                            "Attenzione: mi avevi detto di avere la patente" &
                            If(adesso.Patente.Categorie.Count > 0,
                               $" ({String.Join(", ", adesso.Patente.Categorie)})", "") &
                            ", e confermando questa risposta risulterai senza. " &
                            "Se non è quello che vuoi, scegli «Correggi».")
                    End If

            End Select

        End Sub

        ''' <summary>
        ''' Le righe con cui si mostrano i recapiti, saltando i vuoti. Sta in un posto solo
        ''' perché servono due volte: per dire cosa si è capito e per dire cosa sparisce, e
        ''' due elenchi costruiti separatamente divergerebbero al primo campo aggiunto.
        ''' </summary>
        Public Shared Function RigheDeiContatti(c As ContattiProfilo) As List(Of String)

            Dim righe As New List(Of String)
            If c Is Nothing Then Return righe

            If c.Email <> "" Then righe.Add("Email: " & c.Email)
            If c.Telefono <> "" Then righe.Add("Telefono: " & c.Telefono)
            If c.Citta <> "" Then righe.Add("Città: " & c.Citta)
            If c.Link <> "" Then righe.Add("Link: " & c.Link)

            Return righe

        End Function

        ''' <summary>
        ''' Quali righe già confermate se ne andrebbero, se al posto delle vecchie si
        ''' mettessero le nuove. Il confronto è sull'<b>etichetta</b> e non sul valore: un
        ''' campo ridetto diverso è una correzione voluta, un campo non ridetto è una perdita.
        ''' </summary>
        Public Shared Function CampiCheSpariscono(prima As List(Of String),
                                                  dopo As List(Of String)) As List(Of String)

            If prima Is Nothing OrElse prima.Count = 0 Then Return New List(Of String)

            Dim restano As New HashSet(Of String)(
                If(dopo, New List(Of String)).Select(Function(r) EtichettaDi(r)))

            Return prima.Where(Function(r) Not restano.Contains(EtichettaDi(r))).ToList()

        End Function

        ''' <summary>La parte di riga prima dei due punti: «Email: a@b» -> «Email».</summary>
        Private Shared Function EtichettaDi(riga As String) As String

            Dim taglio As Integer = If(riga, "").IndexOf(":"c)
            Return If(taglio < 0, If(riga, ""), riga.Substring(0, taglio))

        End Function

        Private Async Function ScegliSuSingoloAsync(scelta As String, annulla As CancellationToken) As Task(Of Mossa)

            Dim turno As Turno = Turni(_indice)
            Dim mossa As New Mossa

            Select Case scelta

                Case Scelte.Correggi
                    mossa.Detto.Add(turno.TestoCorrezione)
                    Return ChiediRisposta(mossa, Attesa.RispostaTurno)

                Case Scelte.Conferma
                    Unisci(turno, _frammento)

                    ' La patente dichiarata senza categoria vale una ri-domanda sola: può
                    ' averne più d'una, e la categoria è spesso il requisito eliminatorio.
                    If turno.Chiave = Patente AndAlso Profilo.Patente.Ha = "sì" AndAlso
                       Profilo.Patente.Categorie.Count = 0 Then
                        mossa.Detto.Add(
                            "Una cosa sola: di che categoria è la tua patente? (es. B) " &
                            "Se ne hai più di una, indicale tutte.")
                        Return ChiediRisposta(mossa, Attesa.CategoriaPatente)
                    End If

                    Return Await AvanzaAsync(mossa, annulla).ConfigureAwait(False)

                Case Else
                    Throw SceltaSconosciuta(scelta)

            End Select

        End Function

        ''' <summary>Porta nel profilo ciò che il turno singolo ha raccolto.</summary>
        Private Sub Unisci(turno As Turno, frammento As JsonObject)

            Dim letto As Profilo = Dati.Profilo.DaJson(frammento)

            Select Case turno.Chiave

                Case Nome
                    Profilo.Nome = letto.Nome
                    ' Alla prima domanda capita di rispondere raccontando tutto insieme
                    ' («mi chiamo Anna e facevo la commessa»): dal Pool 1.02 anche il
                    ' prompt del nome instrada quel materiale, e qui lo si parcheggia.
                    RaccogliAltrove(frammento, Nome)

                Case Contatti
                    Profilo.Contatti = letto.Contatti
                    RaccogliAltrove(frammento, Contatti)

                Case Else ' patente
                    ' Se l'utente conferma senza essersi pronunciato, la patente vale
                    ' come non posseduta: è la scelta del prototipo, e serve al confronto
                    ' con gli annunci, dove «non so» non è una risposta utile.
                    Profilo.Patente = New PatenteProfilo With {
                        .Ha = If(letto.Patente.Ha = "", "no", letto.Patente.Ha),
                        .Categorie = letto.Patente.Categorie}
                    RaccogliAltrove(frammento, Patente)

            End Select

        End Sub

        ''' <summary>
        ''' Il turno della patente quando c'è materiale parcheggiato per lui — il prompt
        ''' dei contatti instrada qui «ho la patente B» detta insieme al domicilio. Si
        ''' struttura subito ciò che l'utente aveva detto, glielo si rimette in bocca, e
        ''' si passa dalla stessa conferma del turno singolo: nulla entra senza il suo
        ''' sì. Se l'AI non ce la fa, la domanda del turno è appena stata posta — si
        ''' ascolta la risposta come in un turno normale, e nulla è andato perso.
        ''' </summary>
        Private Async Function ApriPatenteDaPendingAsync(turno As Turno, mossa As Mossa,
                                                         annulla As CancellationToken) As Task(Of Mossa)

            Dim frammenti As List(Of String) = _pending(Patente).ToList()
            _pending(Patente).Clear()

            mossa.Detto.Add("Me ne avevi già accennato, e l'avevo tenuto da parte:")
            mossa.AggiungiEco(String.Join(" / ", frammenti))

            Dim frammento As JsonObject = Nothing
            Try
                frammento = TryCast(Await _strutturatore.StrutturaAsync(
                    Patente, String.Join(vbLf, frammenti), annulla).ConfigureAwait(False), JsonObject)
            Catch ex As ErroreAi
                mossa.Detto.Add("Non sono riuscita a rileggerlo (problema con l'AI): dimmelo tu direttamente.")
                Return ChiediRisposta(mossa, Attesa.RispostaTurno)
            End Try

            _frammento = If(frammento, New JsonObject())
            DiciCosaHoCapito(mossa, turno, _frammento, Profilo)
            _attesa = Attesa.ConfermaSingolo
            Return DueScelte(mossa, "Sì, è giusto", Scelte.Conferma, "Correggi", Scelte.Correggi)

        End Function

        ''' <summary>La ri-domanda della categoria: una sola, poi si prosegue comunque.</summary>
        Private Async Function RispostaCategoriaPatenteAsync(testo As String,
                                                             annulla As CancellationToken) As Task(Of Mossa)

            Dim mossa As New Mossa

            Dim frammento As JsonObject = Await StrutturaAsync(Patente, testo, mossa, annulla).ConfigureAwait(False)
            If frammento Is Nothing Then Return mossa

            Dim categorie As List(Of String) = Dati.Profilo.DaJson(frammento).Patente.Categorie

            If categorie.Count > 0 Then
                Profilo.Patente.Categorie = categorie
                mossa.Detto.Add($"Perfetto: patente {String.Join(", ", categorie)}.")
            Else
                mossa.Detto.Add("Va bene, proseguiamo senza specificarla.")
            End If

            RaccogliAltrove(frammento, Patente)
            Return Await AvanzaAsync(mossa, annulla).ConfigureAwait(False)

        End Function

        ' --- Turni ripetibili: esperienze e formazione -------------------------------

        ' Non è Async e non deve esserlo: dopo aver confermato delle voci non si chiama
        ' l'AI: si segna nel profilo e si offre il giro successivo.
        Private Function ScegliSuVociAsync(scelta As String, annulla As CancellationToken) As Task(Of Mossa)

            Dim turno As Turno = Turni(_indice)
            Dim mossa As New Mossa

            Select Case scelta

                Case Scelte.Correggi
                    mossa.Detto.Add(turno.TestoCorrezione)
                    Return Task.FromResult(ChiediRisposta(mossa, Attesa.RispostaTurno))

                Case Scelte.Conferma
                    RaccogliAltrove(_frammento, Nothing)

                    _daApprofondire = VociDaApprofondire(turno, _voci)
                    If _daApprofondire.Count = 0 Then
                        Return Task.FromResult(ChiudiLeVoci(mossa, turno, conSegnata:=True))
                    End If

                    ' Le voci entrano nel profilo solo dopo le domande: a completarsi è il
                    ' frammento, e il profilo continua ad avere un ingresso solo — quello
                    ' che passa dal lettore tollerante e dal suo filtro anti-invenzione.
                    mossa.Detto.Add("Perfetto, segnata.")
                    Return Task.FromResult(ProssimoApprofondimento(mossa, turno))

                Case Else
                    Throw SceltaSconosciuta(scelta)

            End Select

        End Function

        Private Async Function ScegliSuAltraVoceAsync(scelta As String, annulla As CancellationToken) As Task(Of Mossa)

            Dim turno As Turno = Turni(_indice)
            Dim mossa As New Mossa

            Select Case scelta
                Case Scelte.Altra
                    mossa.Detto.Add(turno.Riapertura)
                    Return ChiediRisposta(mossa, Attesa.RispostaTurno)
                Case Scelte.Procedi
                    Return Await AvanzaAsync(mossa, annulla).ConfigureAwait(False)
                Case Else
                    Throw SceltaSconosciuta(scelta)
            End Select

        End Function

        ' --- La domanda di approfondimento: la voce mezza vuota ----------------------

        ''' <summary>Le voci entrano nel profilo, e il turno offre il suo ponte.</summary>
        ''' <param name="conSegnata">
        ''' Se dire anche «Perfetto, segnata»: quando ci sono state domande di
        ''' approfondimento l'ha già detto la conferma, e ridirlo qui sembrerebbe una
        ''' seconda voce entrata.
        ''' </param>
        Private Function ChiudiLeVoci(mossa As Mossa, turno As Turno, conSegnata As Boolean) As Mossa

            AggiungiAlProfilo(turno.Chiave, _voci)

            mossa.Detto.Add(If(conSegnata, "Perfetto, segnata." & vbLf & vbLf, "") & turno.Ponte)
            _attesa = Attesa.AltraVoce
            Return DueScelte(mossa, "Ne ho un'altra", Scelte.Altra, "Procediamo", Scelte.Procedi)

        End Function

        ''' <summary>
        ''' Quali delle voci appena confermate hanno lasciato vuoto un campo che pesa nel
        ''' CV, nell'ordine in cui l'utente le ha raccontate.
        ''' </summary>
        Private Shared Function VociDaApprofondire(turno As Turno, voci As JsonArray) As List(Of Integer)

            Dim quali As New List(Of Integer)
            If voci Is Nothing Then Return quali

            For i As Integer = 0 To voci.Count - 1
                If CampiVuoti(turno, TryCast(voci(i), JsonObject)).Count > 0 Then quali.Add(i)
            Next

            Return quali

        End Function

        ''' <summary>I campi che pesano e che questa voce ha lasciato vuoti.</summary>
        Private Shared Function CampiVuoti(turno As Turno, voce As JsonObject) As List(Of CampoCheConta)

            Dim oggetto As JsonObject = If(voce, New JsonObject())
            Return turno.CampiCheContano.Where(Function(c) Testo(oggetto, c.Chiave) = "").ToList()

        End Function

        ''' <summary>
        ''' Offre la domanda alla prossima voce incompleta, o chiude il turno se non ne
        ''' restano.
        ''' </summary>
        ''' <remarks>
        ''' Una voce esce dall'elenco <b>quando la domanda le viene offerta</b>, non quando
        ''' la risposta riesce: è la disciplina della ripresa (v.
        ''' <see cref="RiprendiSaltateAsync"/>) e della guardia anti-rimbalzo del magazzino,
        ''' applicata qui. L'occasione è una — se una risposta andata di nuovo a vuoto ne
        ''' guadagnasse un'altra, la stessa voce si farebbe richiedere all'infinito.
        ''' </remarks>
        Private Function ProssimoApprofondimento(mossa As Mossa, turno As Turno) As Mossa

            Do While _daApprofondire.Count > 0

                _voceApprofondita = _daApprofondire(0)
                _daApprofondire.RemoveAt(0)

                Dim voce As JsonObject = TryCast(_voci(_voceApprofondita), JsonObject)
                _campiChiesti = CampiVuoti(turno, voce)

                ' Una voce completata nel frattempo non si chiede: senza questa uscita la
                ' frase resterebbe monca, «non mi hai detto .».
                If _campiChiesti.Count = 0 Then Continue Do

                mossa.Detto.Add(
                    $"{ComeSiChiama(turno, voce)} {NonMiHaiDetto} {ElencoDeiCampi(_campiChiesti)}. " &
                    If(_campiChiesti.Count = 1, "Te lo ricordi?", "Me li dici?"))

                Return ChiediRisposta(mossa, Attesa.Approfondimento)

            Loop

            Return ChiudiLeVoci(mossa, turno, conSegnata:=False)

        End Function

        ''' <summary>
        ''' Come si chiama la voce dentro la domanda: «Del lavoro «Magazziniere»». Il nome
        ''' si prende dal primo campo compilato fra quelli che il turno indica — mai da un
        ''' campo che la domanda sta chiedendo, che per definizione è vuoto.
        ''' </summary>
        Private Shared Function ComeSiChiama(turno As Turno, voce As JsonObject) As String

            Dim oggetto As JsonObject = If(voce, New JsonObject())

            For Each campo As String In turno.CampiDelNome
                Dim valore As String = Testo(oggetto, campo)
                If valore <> "" Then Return String.Format(turno.VoceConNome, valore)
            Next

            Return turno.VoceSenzaNome

        End Function

        ''' <summary>«quanto è durato»; «cosa facevi né quanto è durato»; e così via.</summary>
        Private Shared Function ElencoDeiCampi(campi As List(Of CampoCheConta)) As String

            Dim voci As List(Of String) = campi.Select(Function(c) c.ComeChiederlo).ToList()
            If voci.Count = 1 Then Return voci(0)

            Return String.Join(", ", voci.Take(voci.Count - 1)) & " né " & voci.Last()

        End Function

        ''' <summary>
        ''' La risposta a una domanda di approfondimento: si struttura col prompt del turno
        ''' e del frammento che torna si prende <b>soltanto</b> il campo che mancava.
        ''' </summary>
        ''' <remarks>
        ''' Mai una voce nuova, ed è la seconda guardia di terminazione: se una risposta
        ''' potesse generare voci, ognuna vorrebbe la sua domanda e il giro non finirebbe.
        ''' Quel che l'utente ha detto in più non si perde comunque — ciò che appartiene ad
        ''' altre categorie va nel magazzino come sempre, e se dalla risposta è uscita più
        ''' di un'esperienza glielo si dice, perché il ponte del turno sta per chiedergliela.
        ''' </remarks>
        ''' <param name="risposta">
        ''' Le parole dell'utente. Si chiama così e non «testo» come nelle sorelle perché
        ''' qui serve la funzione <see cref="Testo"/>, e in VB un parametro omonimo la
        ''' coprirebbe — senza errori di nome, con la chiamata letta come indicizzazione
        ''' della stringa.
        ''' </param>
        Private Async Function RispostaApprofondimentoAsync(risposta As String,
                                                            annulla As CancellationToken) As Task(Of Mossa)

            Dim turno As Turno = Turni(_indice)
            Dim mossa As New Mossa
            Dim voce As JsonObject = TryCast(_voci(_voceApprofondita), JsonObject)

            Dim frammento As JsonObject = Await StrutturaAsync(
                turno.Chiave, ConLaVoceDavanti(turno, voce, risposta), mossa, annulla).ConfigureAwait(False)
            If frammento Is Nothing Then Return mossa   ' l'AI non ha risposto: si richiede la stessa cosa

            Dim voci As JsonArray = Elenco(frammento, turno.Chiave)
            Dim letta As JsonObject = If(voci.Count > 0, TryCast(voci(0), JsonObject), New JsonObject())

            Dim valori As New List(Of String)
            Dim righe As New List(Of String)

            For Each campo As CampoCheConta In _campiChiesti
                Dim valore As String = Testo(If(letta, New JsonObject()), campo.Chiave)
                If valore = "" Then Continue For
                voce(campo.Chiave) = valore
                valori.Add(valore)
                righe.Add($"{EtichettaDelCampo(turno, campo.Chiave)}: {valore}")
            Next

            If righe.Count = 0 Then
                ' Le parole della ri-domanda della patente, che è questa stessa cosa fatta
                ' per un turno solo: non sapere è una risposta, e non blocca niente.
                mossa.Detto.Add("Va bene, proseguiamo così.")
            ElseIf righe.Count = 1 Then
                mossa.Detto.Add($"Perfetto: {valori(0)}.")
            Else
                mossa.Detto.Add("Perfetto, ho segnato:" & vbLf & String.Join(vbLf, righe))
            End If

            ' Nella risposta l'utente può aver ridetto diverso un campo che non gli era
            ' stato chiesto — «tre anni, ma non ero magazziniere, ero mulettista». Qui si
            ' prende solo ciò che mancava, ed è la regola giusta: correggere non è
            ' completare, e una correzione che entrasse senza scheda di conferma
            ' cambierebbe il profilo alle spalle di chi l'ha confermato. Ma non si tace:
            ' i difetti di silenzio sono la famiglia curata in tutto questo tempo.
            Dim ridetti As List(Of String) = CampiRidettiDiverso(turno, voce, letta)
            If ridetti.Count > 0 Then
                mossa.Detto.Add(
                    "Questi invece li tengo come me li avevi già detti: " &
                    String.Join("; ", ridetti) & ". Se volevi correggerli, si fa dal profilo.")
            End If

            RaccogliAltrove(frammento, Nothing)

            If voci.Count > 1 Then
                mossa.Detto.Add("Se mi hai detto anche altro, tienilo lì: te lo chiedo fra un momento.")
            End If

            Return ProssimoApprofondimento(mossa, turno)

        End Function

        ''' <summary>
        ''' I campi che la risposta ha ridetto <b>diversi</b> da come erano stati
        ''' confermati, in forma leggibile. Non guarda i campi che la domanda ha chiesto:
        ''' quelli sono appena stati riempiti con ciò che è arrivato, quindi coincidono.
        ''' Il confronto perdona spazi e maiuscole, che non sono una correzione.
        ''' </summary>
        Private Shared Function CampiRidettiDiverso(turno As Turno, voce As JsonObject,
                                                    letta As JsonObject) As List(Of String)

            Dim diversi As New List(Of String)
            Dim prima As JsonObject = If(voce, New JsonObject())
            Dim dopo As JsonObject = If(letta, New JsonObject())

            For Each campo As RigaScheda In turno.Campi
                Dim confermato As String = Testo(prima, campo.Valore)
                Dim ridetto As String = Testo(dopo, campo.Valore)
                If confermato = "" OrElse ridetto = "" Then Continue For
                If String.Equals(confermato.Trim(), ridetto.Trim(),
                                 StringComparison.OrdinalIgnoreCase) Then Continue For
                diversi.Add($"{campo.Etichetta}: {confermato}")
            Next

            Return diversi

        End Function

        ''' <summary>
        ''' Il testo che va al prompt del turno: la voce già confermata, campo per campo, e
        ''' in fondo le parole nuove. Niente di inventato — sono i campi che l'utente ha
        ''' appena confermato — e serve a far collocare la risposta nel posto giusto:
        ''' «tre anni circa», da solo, non è un'esperienza di lavoro, e il prompt potrebbe
        ''' non ricavarne niente.
        ''' </summary>
        Private Shared Function ConLaVoceDavanti(turno As Turno, voce As JsonObject,
                                                 risposta As String) As String

            Dim oggetto As JsonObject = If(voce, New JsonObject())
            Dim righe As New List(Of String)

            For Each campo As RigaScheda In turno.Campi
                Dim valore As String = Testo(oggetto, campo.Valore)
                If valore <> "" Then righe.Add($"{campo.Etichetta}: {valore}")
            Next

            If righe.Count = 0 Then Return risposta
            Return String.Join(vbLf, righe) & vbLf & vbLf & "In più: " & risposta

        End Function

        ''' <summary>L'etichetta con cui il turno chiama un suo campo: «durata» → «Durata».</summary>
        Private Shared Function EtichettaDelCampo(turno As Turno, chiave As String) As String

            Dim campo As RigaScheda = turno.Campi.FirstOrDefault(Function(c) c.Valore = chiave)
            Return If(campo?.Etichetta, chiave)

        End Function

        ' --- Turno a blocco: le competenze -------------------------------------------

        Private Async Function ScegliSuCompetenzeAsync(scelta As String, annulla As CancellationToken) As Task(Of Mossa)

            Dim mossa As New Mossa

            Select Case scelta
                Case Scelte.Aggiungi
                    mossa.Detto.Add("Dimmi pure cosa aggiungere.")
                    Return ChiediRisposta(mossa, Attesa.AggiuntaCompetenze, "Cosa vuoi aggiungere…")
                Case Scelte.Conferma
                    Return Await AvanzaAsync(mossa, annulla).ConfigureAwait(False)
                Case Else
                    Throw SceltaSconosciuta(scelta)
            End Select

        End Function

        ''' <summary>Un solo giro di aggiunta, poi si chiude: niente secondo giro.</summary>
        Private Async Function RispostaAggiuntaCompetenzeAsync(testo As String,
                                                               annulla As CancellationToken) As Task(Of Mossa)

            Dim mossa As New Mossa

            Dim frammento As JsonObject = Await StrutturaAsync(Competenze, testo, mossa, annulla).ConfigureAwait(False)
            If frammento Is Nothing Then Return mossa

            AggiungiAlProfilo(Competenze, Elenco(frammento, Competenze))
            RaccogliAltrove(frammento, Nothing)
            MostraCompetenze(mossa)

            _attesa = Attesa.Competenze
            mossa.Tipo = TipoMossa.ChiediScelta
            mossa.Scelte.Add(New Scelta With {.Id = Scelte.Conferma, .Etichetta = "Confermiamo", .Principale = True})
            Return mossa

        End Function

        Private Sub MostraCompetenze(mossa As Mossa)

            mossa.Detto.Add("Ecco cosa ho capito — le cose che sai fare:")
            mossa.Schede.Add(SchedaElenco(Nothing, Profilo.Competenze))

        End Sub

        ' ==================================================================
        ' Le domande saltate: la ripresa prima del riepilogo
        ' ==================================================================

        ''' <summary>
        ''' Mette in conto una domanda chiusa a mani vuote, perché prima del riepilogo le
        ''' si possa dare una seconda occasione.
        ''' </summary>
        ''' <remarks>
        ''' <b>Dentro una ripresa non si segna niente</b>, ed è la guardia di
        ''' terminazione: senza, un turno che resta vuoto anche la seconda volta
        ''' rientrerebbe nell'elenco e tornerebbe all'infinito. È la stessa disciplina
        ''' della guardia anti-rimbalzo del magazzino — si tenta una volta sola.
        ''' </remarks>
        Private Sub SegnaSaltata(chiave As String)

            If _inRipresa Then Return
            If Not _saltate.Contains(chiave) Then _saltate.Add(chiave)

        End Sub

        ''' <summary>
        ''' La prossima domanda da riproporre, o <c>Nothing</c> se non ne restano. Quelle
        ''' che nel frattempo si sono riempite da sé — l'anti-perdita ha recuperato per
        ''' loro un frammento detto nel turno sbagliato — si tolgono senza chiedere
        ''' niente: la risposta c'è già, e richiederla sembrerebbe non aver ascoltato.
        ''' </summary>
        Private Function ProssimaSaltata() As String

            _saltate.RemoveAll(Function(c) Not AncoraVuoto(c))
            Return _saltate.FirstOrDefault()

        End Function

        ''' <summary>Se di quella categoria il profilo non ha ancora niente.</summary>
        Private Function AncoraVuoto(chiave As String) As Boolean

            If chiave = Competenze Then Return Profilo.Competenze.Count = 0
            Return ProfiloComeVoci(chiave).Count = 0

        End Function

        ''' <summary>
        ''' L'ultimo giro prima del riepilogo: le domande rimaste senza risposta si
        ''' riofrono una per una, e prima si chiede il permesso — chi aveva scelto di
        ''' saltare non se la ritrova addosso. Chi accetta rientra nel turno vero, con la
        ''' sua domanda e le sue conferme: qui non si duplica niente.
        ''' </summary>
        Private Function RiprendiSaltateAsync(mossa As Mossa, annulla As CancellationToken) As Task(Of Mossa)

            Dim chiave As String = ProssimaSaltata()
            If chiave Is Nothing Then Return Task.FromResult(Riepilogo(mossa))

            ' Si toglie dall'elenco adesso, non quando la risposta arriva: l'occasione è
            ' una, e resta consumata anche se va di nuovo a vuoto.
            _saltate.Remove(chiave)
            _indice = Turni.FindIndex(Function(t) t.Chiave = chiave)

            mossa.Detto.Add(
                $"Prima di chiudere: su «{EtichetteCategoria(chiave)}» non avevamo raccolto niente. " &
                "Vuoi provarci ora?")

            _attesa = Attesa.RipresaDomanda
            Return Task.FromResult(DueScelte(mossa, "Ci provo", Scelte.Riprendi, "Lasciamo così", Scelte.Lascia))

        End Function

        Private Async Function ScegliSuRipresaAsync(scelta As String,
                                                    annulla As CancellationToken) As Task(Of Mossa)

            Dim mossa As New Mossa

            Select Case scelta

                Case Scelte.Riprendi
                    ' Da qui il turno si comporta come sempre: è AvanzaAsync che, invece
                    ' di proseguire col turno dopo, riporterà alla passata finale.
                    _inRipresa = True
                    Return Await ApriTurnoAsync(mossa, annulla).ConfigureAwait(False)

                Case Scelte.Lascia
                    mossa.Detto.Add("Va bene, lasciamo così.")
                    Return Await RiprendiSaltateAsync(mossa, annulla).ConfigureAwait(False)

                Case Else
                    Throw SceltaSconosciuta(scelta)

            End Select

        End Function

        ' ==================================================================
        ' Anti-perdita: il magazzino, l'iniezione, il «lasciato fuori»
        ' ==================================================================

        ''' <summary>
        ''' Travasa nel magazzino il materiale che il frammento ha instradato ad altre
        ''' categorie, con le parole esatte dell'utente.
        ''' </summary>
        ''' <param name="escludi">
        ''' La categoria che si sta già smaltendo, per non rimetterci dentro roba sua.
        ''' </param>
        Private Sub RaccogliAltrove(frammento As JsonObject, escludi As String)

            Dim altrove As JsonObject = TryCast(frammento?("altrove"), JsonObject)
            If altrove Is Nothing Then Return

            For Each dest As String In DestinazioniPending
                If dest = escludi Then Continue For
                For Each frase As String In Frasi(altrove, dest)
                    _pending(dest).Add(frase)
                Next
            Next

        End Sub

        ''' <summary>
        ''' Come sopra, ma <b>senza parcheggiare</b>: restituisce soltanto l'elenco. È la
        ''' guardia anti-rimbalzo — ciò che il turno di destinazione non ha saputo
        ''' collocare non torna nel magazzino, si dichiara lasciato fuori.
        ''' </summary>
        Private Shared Function AltroveLasciatoFuori(frammento As JsonObject, escludi As String) As List(Of String)

            Dim fuori As New List(Of String)
            Dim altrove As JsonObject = TryCast(frammento?("altrove"), JsonObject)
            If altrove Is Nothing Then Return fuori

            For Each dest As String In DestinazioniPending
                If dest = escludi Then Continue For
                fuori.AddRange(Frasi(altrove, dest))
            Next

            Return fuori

        End Function

        ''' <summary>
        ''' Ripesca i frammenti messi da parte per una categoria: li struttura col prompt
        ''' di quel turno, li mostra e lascia scegliere. Nulla entra nel profilo senza la
        ''' conferma dell'utente.
        ''' </summary>
        Private Async Function SmaltisciPendingAsync(dest As String, mossa As Mossa,
                                                     annulla As CancellationToken) As Task(Of Mossa)

            Dim frammenti As List(Of String) = _pending(dest).ToList()
            _pending(dest).Clear()

            mossa.Detto.Add(
                $"Prima avevi accennato a qualcosa che riguarda «{EtichetteCategoria(dest)}», " &
                "e l'avevo tenuto da parte. Vediamolo ora:")

            ' L'eco si ancora qui, subito dopo l'annuncio: così anche quando il
            ' ripescaggio fallisce l'utente rivede le proprie parole prima del
            ' verdetto, non dopo.
            mossa.AggiungiEco(String.Join(" / ", frammenti))

            ' L'esito della chiamata si raccoglie in una variabile invece di proseguire
            ' dentro il Catch: in VB dentro un Catch non si può aspettare.
            Dim frammento As JsonObject = Nothing
            Dim illeggibile As Boolean = False
            Try
                frammento = TryCast(Await _strutturatore.StrutturaAsync(
                    dest, String.Join(vbLf, frammenti), annulla).ConfigureAwait(False), JsonObject)
            Catch ex As ErroreAi
                mossa.Detto.Add(
                    "Non sono riuscita a leggerlo (problema con l'AI), quindi lo lascio fuori: " &
                    $"«{String.Join(" / ", frammenti)}».")
                illeggibile = True
            End Try

            If illeggibile Then Return Await ProseguiDopoPendingAsync(mossa, annulla).ConfigureAwait(False)

            Dim voci As JsonArray = Elenco(frammento, dest)
            Dim fuori As List(Of String) = AltroveLasciatoFuori(frammento, dest)

            If voci.Count = 0 Then
                Dim rimasto As String = If(fuori.Count > 0, String.Join(" / ", fuori), String.Join(" / ", frammenti))
                mossa.Detto.Add(
                    $"Questo non sono riuscita a collocarlo in nessuna sezione, quindi lo lascio fuori: «{rimasto}».")
                Return Await ProseguiDopoPendingAsync(mossa, annulla).ConfigureAwait(False)
            End If

            _destPending = dest
            _vociPending = voci
            _fuoriPending = fuori

            Anteprima(mossa, dest, voci)

            _attesa = Attesa.ConfermaPending
            mossa.Tipo = TipoMossa.ChiediScelta
            mossa.Scelte.Add(New Scelta With {.Id = Scelte.Conferma, .Etichetta = "Sì, aggiungilo", .Principale = True})
            mossa.Scelte.Add(New Scelta With {.Id = Scelte.Correggi, .Etichetta = "Correggi"})
            mossa.Scelte.Add(New Scelta With {.Id = Scelte.Scarta, .Etichetta = "Scartalo"})
            Return mossa

        End Function

        Private Async Function ScegliSuPendingAsync(scelta As String, annulla As CancellationToken) As Task(Of Mossa)

            Dim mossa As New Mossa

            Select Case scelta

                Case Scelte.Conferma
                    AggiungiAlProfilo(_destPending, _vociPending)
                    mossa.Detto.Add("Fatto, l'ho aggiunto.")
                    If _fuoriPending.Count > 0 Then
                        mossa.Detto.Add(
                            "Una parte non l'ho saputa collocare, la lascio fuori: " &
                            $"«{String.Join(" / ", _fuoriPending)}».")
                    End If
                    Return Await ProseguiDopoPendingAsync(mossa, annulla).ConfigureAwait(False)

                Case Scelte.Correggi
                    mossa.Detto.Add("Va bene, riscrivimelo come va detto.")
                    Return ChiediRisposta(mossa, Attesa.CorrezionePending, "La tua correzione…")

                Case Scelte.Scarta
                    mossa.Detto.Add("Va bene, lo lascio perdere.")
                    Return Await ProseguiDopoPendingAsync(mossa, annulla).ConfigureAwait(False)

                Case Else
                    Throw SceltaSconosciuta(scelta)

            End Select

        End Function

        Private Async Function RispostaCorrezionePendingAsync(testo As String,
                                                              annulla As CancellationToken) As Task(Of Mossa)

            Dim mossa As New Mossa
            Dim dest As String = _destPending

            Dim frammento As JsonObject = Nothing
            Dim fallita As Boolean = False
            Try
                frammento = TryCast(Await _strutturatore.StrutturaAsync(dest, testo, annulla).
                                    ConfigureAwait(False), JsonObject)
            Catch ex As ErroreAi
                ' Nel prototipo un errore qui fa proseguire senza aggiungere nulla.
                mossa.Detto.Add(ex.Message)
                fallita = True
            End Try

            If fallita Then Return Await ProseguiDopoPendingAsync(mossa, annulla).ConfigureAwait(False)

            Dim voci As JsonArray = Elenco(frammento, dest)

            If voci.Count > 0 Then
                AggiungiAlProfilo(dest, voci)
                Anteprima(mossa, dest, voci)
                mossa.Detto.Add("Aggiornato e aggiunto.")
            Else
                mossa.Detto.Add("Non ho colto una voce da aggiungere. Proseguiamo.")
            End If

            ' Anche la correzione può contenere materiale d'altre categorie: per la
            ' guardia anti-rimbalzo non torna nel magazzino, ma non sparisce in
            ' silenzio — si dichiara, come in ogni altro punto del ripescaggio.
            Dim fuori As List(Of String) = AltroveLasciatoFuori(frammento, dest)
            If fuori.Count > 0 Then
                mossa.Detto.Add(
                    "Una parte non l'ho saputa collocare, la lascio fuori: " &
                    $"«{String.Join(" / ", fuori)}».")
            End If

            Return Await ProseguiDopoPendingAsync(mossa, annulla).ConfigureAwait(False)

        End Function

        ''' <summary>Dopo un frammento smaltito: la domanda del turno, o la passata finale.</summary>
        Private Function ProseguiDopoPendingAsync(mossa As Mossa, annulla As CancellationToken) As Task(Of Mossa)

            If _dopoPending = DopoPending.PassataFinale Then Return PassataFinaleAsync(mossa, annulla)
            Return Task.FromResult(ChiediRisposta(mossa, Attesa.RispostaTurno))

        End Function

        ''' <summary>L'anteprima di ciò che si è ricavato da un frammento parcheggiato.</summary>
        Private Shared Sub Anteprima(mossa As Mossa, dest As String, voci As JsonArray)

            If dest = Competenze Then
                mossa.Schede.Add(SchedaElenco(Nothing, voci.Select(Function(v) If(v?.ToString(), "")).ToList()))
                Return
            End If

            Dim turno As Turno = TrovaTurno(dest)
            For Each voce As JsonNode In voci
                mossa.Schede.Add(SchedaVoce(turno, voce, Nothing))
            Next

        End Sub

        ' ==================================================================
        ' Servizi
        ' ==================================================================

        ''' <summary>
        ''' Chiede all'AI di strutturare una risposta. Se l'AI non ce la fa, invece di
        ''' cadere si dice cosa è successo e si richiede la stessa risposta: la
        ''' <paramref name="mossa"/> torna già pronta e il chiamante restituisce quella.
        ''' </summary>
        ''' <returns>Il frammento, oppure <c>Nothing</c> se la chiamata è fallita.</returns>
        Private Async Function StrutturaAsync(turno As String, testo As String, mossa As Mossa,
                                              annulla As CancellationToken) As Task(Of JsonObject)

            Try
                Dim uscita As JsonNode = Await _strutturatore.StrutturaAsync(turno, testo, annulla).ConfigureAwait(False)

                ' Una risposta che non è un oggetto vale come risposta vuota: ogni campo
                ' risulterà mancante e il turno dirà «non ho colto», invece di cadere.
                Return If(TryCast(uscita, JsonObject), New JsonObject())

            Catch ex As ErroreAi
                mossa.Detto.Add(ex.Message)
                mossa.Detto.Add("Riprova pure: riscrivimi la risposta.")
                ChiediRisposta(mossa, _attesa)
                Return Nothing
            End Try

        End Function

        ''' <summary>Aggiunge al profilo le voci di una categoria, passando dal lettore tollerante.</summary>
        Private Sub AggiungiAlProfilo(chiave As String, voci As JsonArray)

            If voci Is Nothing OrElse voci.Count = 0 Then Return

            ' Si riusa il lettore del profilo: così ciò che entra passa dallo stesso
            ' filtro anti-invenzione del resto (i campi fuori schema non entrano).
            Dim letto As Profilo = Dati.Profilo.DaJson(New JsonObject From {{chiave, voci.DeepClone()}})

            Select Case chiave
                Case EsperienzeFormali
                    Profilo.EsperienzeFormali.AddRange(letto.EsperienzeFormali)
                Case EsperienzeInformali
                    Profilo.EsperienzeInformali.AddRange(letto.EsperienzeInformali)
                Case Competenze
                    Profilo.Competenze.AddRange(letto.Competenze)
                Case Formazione
                    Profilo.Formazione.AddRange(letto.Formazione)
            End Select

        End Sub

        ''' <summary>Le voci già nel profilo per una categoria, in forma JSON.</summary>
        Private Function ProfiloComeVoci(chiave As String) As JsonArray
            Return If(TryCast(Profilo.VersoJson()(chiave), JsonArray), New JsonArray())
        End Function

        ''' <summary>La scheda di una voce-oggetto: il tipo in testa se c'è, poi i campi del turno.</summary>
        Private Shared Function SchedaVoce(turno As Turno, voce As JsonNode, titolo As String) As Scheda

            Dim scheda As New Scheda With {.Titolo = titolo}
            Dim oggetto As JsonObject = If(TryCast(voce, JsonObject), New JsonObject())

            Dim tipo As String = Testo(oggetto, "tipo")
            If tipo <> "" Then
                scheda.Righe.Add(New RigaScheda With {.Etichetta = "Tipo", .Valore = tipo})
            End If

            For Each campo As RigaScheda In turno.Campi
                Dim valore As String = Testo(oggetto, campo.Valore)
                scheda.Righe.Add(New RigaScheda With {
                    .Etichetta = campo.Etichetta,
                    .Valore = If(valore <> "", valore, Mossa.NonSpecificato)})
            Next

            Return scheda

        End Function

        ''' <summary>Una scheda a elenco nudo (le competenze, le righe del riepilogo).</summary>
        Private Shared Function SchedaElenco(titolo As String, voci As IEnumerable(Of String)) As Scheda

            Dim scheda As New Scheda With {.Titolo = titolo}
            For Each voce As String In voci
                scheda.Righe.Add(New RigaScheda With {.Valore = voce})
            Next
            Return scheda

        End Function

        ''' <summary>Prepara la mossa che aspetta una risposta scritta.</summary>
        Private Function ChiediRisposta(mossa As Mossa, attesa As Attesa,
                                        Optional suggerimento As String = "La tua risposta…") As Mossa

            _attesa = attesa
            mossa.Tipo = TipoMossa.ChiediRisposta
            mossa.SuggerimentoCasella = suggerimento
            Return mossa

        End Function

        ''' <summary>Prepara la mossa che aspetta una scelta fra due bottoni.</summary>
        Private Shared Function DueScelte(mossa As Mossa,
                                          primaEtichetta As String, primoId As String,
                                          secondaEtichetta As String, secondoId As String,
                                          Optional principaleELaSeconda As Boolean = False) As Mossa

            mossa.Tipo = TipoMossa.ChiediScelta
            mossa.Scelte.Add(New Scelta With {
                .Id = primoId, .Etichetta = primaEtichetta, .Principale = Not principaleELaSeconda})
            mossa.Scelte.Add(New Scelta With {
                .Id = secondoId, .Etichetta = secondaEtichetta, .Principale = principaleELaSeconda})
            Return mossa

        End Function

        Private Shared Function TrovaTurno(chiave As String) As Turno
            Return Turni.First(Function(t) t.Chiave = chiave)
        End Function

        ''' <summary>L'elenco di una chiave del frammento; vuoto se assente o malformato.</summary>
        Private Shared Function Elenco(frammento As JsonObject, chiave As String) As JsonArray
            If frammento Is Nothing Then Return New JsonArray()
            Return If(TryCast(frammento(chiave), JsonArray), New JsonArray())
        End Function

        ''' <summary>Le frasi di una categoria dentro «altrove», ripulite dagli spazi.</summary>
        Private Shared Function Frasi(altrove As JsonObject, dest As String) As List(Of String)

            Dim raccolte As New List(Of String)
            Dim elenco As JsonArray = TryCast(altrove(dest), JsonArray)
            If elenco Is Nothing Then Return raccolte

            For Each voce As JsonNode In elenco
                Dim valore As JsonValue = TryCast(voce, JsonValue)
                If valore Is Nothing Then Continue For
                Dim frase As String = valore.ToString().Trim()
                If frase <> "" Then raccolte.Add(frase)
            Next

            Return raccolte

        End Function

        ''' <summary>Un campo di testo del frammento; vuoto se assente.</summary>
        Private Shared Function Testo(oggetto As JsonObject, chiave As String) As String
            Return If(TryCast(oggetto(chiave), JsonValue)?.ToString(), "")
        End Function

        Private Shared Function SceltaSconosciuta(scelta As String) As ArgumentException
            Return New ArgumentException(
                $"Il dialogo non offre la scelta «{scelta}» in questo momento.", NameOf(scelta))
        End Function

    End Class

End Namespace
