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
            Competenze
            AggiuntaCompetenze
            ConfermaPending
            CorrezionePending
            Concluso
        End Enum

        ''' <summary>Dove si torna dopo aver smaltito i frammenti parcheggiati.</summary>
        Private Enum DopoPending
            ChiediIlTurno
            PassataFinale
        End Enum

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
                .TestoRiprova = "Va bene, raccontamela come viene.",
                .TestoCorrezione = "Va bene, raccontamela di nuovo.",
                .Campi = New List(Of RigaScheda) From {
                    New RigaScheda With {.Etichetta = "Titolo", .Valore = "titolo"},
                    New RigaScheda With {.Etichetta = "Istituto", .Valore = "istituto"},
                    New RigaScheda With {.Etichetta = "Anno", .Valore = "anno"}}}}

        Private ReadOnly _strutturatore As IStrutturatoreTurni

        ''' <summary>Il magazzino dei frammenti instradati ad altri turni.</summary>
        Private ReadOnly _pending As New Dictionary(Of String, List(Of String))

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

        ''' <summary>Passa al turno successivo.</summary>
        Private Function AvanzaAsync(mossa As Mossa, annulla As CancellationToken) As Task(Of Mossa)

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
            If dest Is Nothing Then Return Riepilogo(mossa)

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
                    DiciCosaHoCapito(mossa, turno, frammento)
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
                    Return Await AvanzaAsync(mossa, annulla).ConfigureAwait(False)
                Case Else
                    Throw SceltaSconosciuta(scelta)
            End Select

        End Function

        ' --- Turni singoli: nome, contatti, patente ---------------------------------

        ''' <summary>Il «ho capito questo» dei turni singoli, con le parole del prototipo.</summary>
        Private Shared Sub DiciCosaHoCapito(mossa As Mossa, turno As Turno, frammento As JsonObject)

            Select Case turno.Chiave

                Case Nome
                    ' Attenzione al nome della variabile: in VB le maiuscole non
                    ' distinguono, e un locale «nome» oscurerebbe la costante «Nome».
                    Dim letto As String = Testo(frammento, Nome)
                    mossa.Detto.Add(If(letto <> "",
                                       $"Ho capito che ti chiami: {letto}.",
                                       "Non ho colto un nome nella tua risposta."))

                Case Contatti
                    Dim c As JsonObject = TryCast(frammento(Contatti), JsonObject)
                    Dim righe As New List(Of String)
                    If c IsNot Nothing Then
                        If Testo(c, "email") <> "" Then righe.Add("Email: " & Testo(c, "email"))
                        If Testo(c, "telefono") <> "" Then righe.Add("Telefono: " & Testo(c, "telefono"))
                        If Testo(c, "citta") <> "" Then righe.Add("Domicilio: " & Testo(c, "citta"))
                        If Testo(c, "link") <> "" Then righe.Add("Link: " & Testo(c, "link"))
                    End If
                    mossa.Detto.Add(If(righe.Count > 0,
                                       "Ecco cosa ho capito:" & vbLf & String.Join(vbLf, righe),
                                       "Non ho colto nessun recapito nella tua risposta."))

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

            End Select

        End Sub

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
            DiciCosaHoCapito(mossa, turno, _frammento)
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
                    AggiungiAlProfilo(turno.Chiave, _voci)
                    RaccogliAltrove(_frammento, Nothing)

                    mossa.Detto.Add("Perfetto, segnata." & vbLf & vbLf & turno.Ponte)
                    _attesa = Attesa.AltraVoce
                    Return Task.FromResult(
                        DueScelte(mossa, "Ne ho un'altra", Scelte.Altra, "Procediamo", Scelte.Procedi))

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
