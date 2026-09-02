Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Una candidatura come la si vede da fuori: quel tanto che basta a riconoscerla in
    ''' un elenco e a decidere se aprirla (cap. 07.3).
    ''' </summary>
    ''' <remarks>
    ''' <para>La <see cref="Cartella"/> è il <b>nome</b> della cartella, non il percorso
    ''' intero: la cartella dati si può spostare (cap. 11.1) e si esporta nei backup
    ''' (cap. 11.4), e un indice pieno di percorsi assoluti smetterebbe di valere il
    ''' giorno in cui quei percorsi cambiano. Il percorso lo ricompone chi legge, che la
    ''' radice ce l'ha.</para>
    ''' <para>Cosa <b>non</b> c'è ancora: l'esito della candidatura, che il cap. 07.3
    ''' elenca insieme a questi ed è uno stato che dall'interfaccia non si raggiunge (v.
    ''' <c>in_sospeso.md</c>). Resta vero il motivo per cui non è un debito grave — questo
    ''' file si rigenera dalle cartelle, quindi chi scriverà quel dato aggiungerà qui una
    ''' colonna e l'indice si ricostruirà da sé, senza migrare niente. È esattamente com'è
    ''' andata per il <see cref="Destinatario"/>, che era nella stessa condizione.</para>
    ''' </remarks>
    Public Class VoceRegistro

        ''' <summary>Il nome della cartella-opportunità: è l'identità della voce.</summary>
        Public Property Cartella As String

        ''' <summary>A che punto è (cap. 07.3).</summary>
        Public Property Stato As StatoOpportunita

        ''' <summary>
        ''' Com'è finita, se lo si sa (cap. 07.3); <c>Nothing</c> quando nessuno l'ha
        ''' registrato — che per una candidatura già spedita vuol dire «sta ancora
        ''' aspettando», ed è ciò che il promemoria di follow-up va a cercare.
        ''' </summary>
        Public Property Esito As EsitoCandidatura?

        ''' <summary>Quando è entrata in ciascuno stato.</summary>
        Public ReadOnly Property DateStati As New Dictionary(Of StatoOpportunita, Date)

        ''' <summary>Chi offre il posto e per quale ruolo; vuoti se l'annuncio è anonimo.</summary>
        Public Property Azienda As String
        Public Property Titolo As String

        ''' <summary>Da dove veniva l'annuncio (cap. 06.4); vuoti se incollato a mano.</summary>
        Public Property Fonte As String
        Public Property Link As String

        ''' <summary>La lingua dei documenti.</summary>
        Public Property Lingua As String

        ''' <summary>
        ''' A chi è stata mandata la candidatura; vuoto finché l'email non è stata
        ''' preparata (cap. 07.3).
        ''' </summary>
        ''' <remarks>
        ''' Lo si ricopia qui dalla bozza <c>email.json</c> della cartella. Ce ne fosse una
        ''' copia sola, per leggere «a chi ho scritto?» bisognerebbe aprire una candidatura
        ''' alla volta — e l'indice esiste proprio per rispondere senza aprire niente. La
        ''' bozza resta la fonte: il valore lo digita l'utente, e da qui non si scrive mai
        ''' all'indietro.
        ''' </remarks>
        Public Property Destinatario As String

        ''' <summary>
        ''' Le stelle del match; <c>Nothing</c> se non è stato calcolato — un'opportunità
        ''' può essere ancora <see cref="StatoOpportunita.Nuova"/>.
        ''' </summary>
        Public Property Stelle As Double?

        ''' <summary>Se un requisito eliminatorio è rimasto scoperto: è il ⛔ del cap. 07.3.</summary>
        Public Property GateEliminatorio As Boolean

        ''' <summary>
        ''' Con quale versione di profilo fu fatto il confronto: è quel che accende la spia
        ''' nell'elenco (<see cref="Ui.SpiaDelProfilo"/>).
        ''' </summary>
        ''' <remarks>
        ''' <para>Vive già sull'opportunità dal 2026-09-02, ma la coda della Home le
        ''' opportunità non le apre: legge il registro, ed era l'unico posto del programma in
        ''' cui una candidatura disallineata non lo diceva. Arriva qui perché la Home possa
        ''' dirlo <b>su tutte insieme</b>, che è la domanda vera di quella schermata — non
        ''' «questa com'è messa», ma «quali devo rifare».</para>
        ''' <para>Un registro scritto prima di oggi non ha il campo, e chi lo rilegge trova
        ''' una versione vuota: la spia resta <b>spenta</b>, che è la risposta giusta per
        ''' «non lo so» e non costringe a buttare via il file. Al primo giro di ricognizione
        ''' delle cartelle la voce si riscrive da <see cref="Da"/> e il campo torna.</para>
        ''' </remarks>
        Public Property VersioneProfilo As String

        ''' <summary>Quando la cartella è nata e quando è stata toccata l'ultima volta.</summary>
        Public Property Creata As Date
        Public Property Aggiornata As Date

        ''' <summary>
        ''' Da quanti giorni questa candidatura aspetta una risposta; <c>Nothing</c> se la
        ''' domanda non si pone (cap. 07.3).
        ''' </summary>
        ''' <param name="adesso">Il momento rispetto a cui si contano i giorni.</param>
        ''' <remarks>
        ''' <para>Si pone per le sole <see cref="StatoOpportunita.Inviata"/>: prima non è
        ''' partita niente, e da <see cref="StatoOpportunita.Esito"/> in poi una risposta
        ''' è arrivata — l'attesa è finita anche quando la notizia è brutta. Una candidatura
        ''' spedita ma senza la data dell'invio non si conta: nata da un file corretto a
        ''' mano, direbbe «ferma da duemila anni».</para>
        ''' <para>I giorni si troncano, non si arrotondano: spedita stamattina vuol dire
        ''' zero giorni, e la soglia si tocca il giorno in cui la si tocca davvero.</para>
        ''' </remarks>
        Public Function GiorniDiAttesa(adesso As Date) As Integer?

            If Stato <> StatoOpportunita.Inviata Then Return Nothing

            Dim invio As Date
            If Not DateStati.TryGetValue(StatoOpportunita.Inviata, invio) Then Return Nothing
            If invio = Nothing Then Return Nothing

            Return CInt(Math.Floor((adesso - invio).TotalDays))

        End Function

        ''' <summary>La voce che descrive un'opportunità già salvata su disco.</summary>
        Public Shared Function Da(opportunita As Opportunita) As VoceRegistro

            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))

            If String.IsNullOrEmpty(opportunita.Cartella) Then
                Throw New InvalidOperationException(
                    "Un'opportunità non ancora salvata non ha una cartella con cui stare nel registro.")
            End If

            Dim voce As New VoceRegistro With {
                .Cartella = Path.GetFileName(opportunita.Cartella),
                .Stato = opportunita.Stato,
                .Esito = opportunita.Esito,
                .Azienda = opportunita.Azienda,
                .Titolo = opportunita.Titolo,
                .Fonte = opportunita.Fonte,
                .Link = opportunita.Link,
                .Lingua = opportunita.Lingua,
                .Destinatario = DestinatarioDellaBozza(opportunita.Email),
                .Stelle = opportunita.Match?.Stelle,
                .GateEliminatorio = opportunita.Match IsNot Nothing AndAlso opportunita.Match.GateEliminatorio,
                .VersioneProfilo = opportunita.VersioneProfilo,
                .Creata = opportunita.Creata,
                .Aggiornata = opportunita.Aggiornata}

            For Each passaggio As KeyValuePair(Of StatoOpportunita, Date) In opportunita.DateStati
                voce.DateStati(passaggio.Key) = passaggio.Value
            Next

            Return voce

        End Function

        ''' <summary>
        ''' Il destinatario scritto nella bozza dell'email, se c'è una bozza e se l'utente
        ''' l'ha già compilato.
        ''' </summary>
        ''' <remarks>
        ''' La bozza arriva qui come JSON grezzo — è così che l'opportunità la tiene, senza
        ''' interpretarla — quindi il campo si legge alla maniera dei campi: quello che non
        ''' c'è o non è testo vale come «non ancora scritto», non come un guasto.
        ''' </remarks>
        Private Shared Function DestinatarioDellaBozza(bozza As JsonNode) As String

            Return CampiJson.Testo(TryCast(bozza, JsonObject), "destinatario")

        End Function

        ''' <summary>La voce come si scrive nel file.</summary>
        Public Function ComeJson() As JsonObject

            Return New JsonObject From {
                {"cartella", Cartella},
                {"stato", StatiOpportunita.Nome(Stato)},
                {"esito", If(Esito.HasValue, EsitiCandidatura.Nome(Esito.Value), Nothing)},
                {"date_stati", StatiOpportunita.DateComeJson(DateStati)},
                {"azienda", Azienda},
                {"titolo", Titolo},
                {"fonte", Fonte},
                {"link", Link},
                {"lingua", Lingua},
                {"destinatario", Destinatario},
                {"stelle", Stelle},
                {"gate_eliminatorio", GateEliminatorio},
                {"versione_profilo", VersioneProfilo},
                {"creata", CampiJson.Quando(Creata)},
                {"aggiornata", CampiJson.Quando(Aggiornata)}}

        End Function

        ''' <summary>
        ''' La voce riletta dal file; <c>Nothing</c> se non dice nemmeno a quale cartella
        ''' si riferisce, che è l'unica cosa senza la quale non serve a niente.
        ''' </summary>
        Public Shared Function DaJson(scritta As JsonObject) As VoceRegistro

            Dim cartella As String = CampiJson.Testo(scritta, "cartella")
            If String.IsNullOrWhiteSpace(cartella) Then Return Nothing

            ' Uno stato che non riconosciamo non si indovina: la voce nasce «nuova» e sarà
            ' il confronto con le cartelle a rimetterla a posto (v. ArchivioRegistro).
            Dim stato As StatoOpportunita? = StatiOpportunita.DaNome(CampiJson.Testo(scritta, "stato"))

            ' Da T9c. Stato ed esito si leggono insieme, con la stessa regola che vale per
            ' lo stato.json di ogni cartella: da soli possono contraddirsi (cap. 07.3).
            Dim letto As StatoOpportunita = If(stato.HasValue, stato.Value, StatoOpportunita.Nuova)
            Dim esito As EsitoCandidatura? = EsitiCandidatura.DaNome(CampiJson.Testo(scritta, "esito"))
            EsitiCandidatura.Concorda(letto, esito)

            Dim voce As New VoceRegistro With {
                .Cartella = cartella.Trim(),
                .Stato = letto,
                .Esito = esito,
                .Azienda = CampiJson.Testo(scritta, "azienda"),
                .Titolo = CampiJson.Testo(scritta, "titolo"),
                .Fonte = CampiJson.Testo(scritta, "fonte"),
                .Link = CampiJson.Testo(scritta, "link"),
                .Lingua = CampiJson.Testo(scritta, "lingua"),
                .Destinatario = CampiJson.Testo(scritta, "destinatario"),
                .Stelle = CampiJson.Numero(scritta, "stelle"),
                .GateEliminatorio = CampiJson.Vero(scritta, "gate_eliminatorio"),
                .VersioneProfilo = CampiJson.Testo(scritta, "versione_profilo"),
                .Creata = CampiJson.Istante(scritta, "creata"),
                .Aggiornata = CampiJson.Istante(scritta, "aggiornata")}

            StatiOpportunita.RiempiDate(voce.DateStati, TryCast(CampiJson.Nodo(scritta, "date_stati"), JsonObject))

            Return voce

        End Function

    End Class

    ''' <summary>
    ''' La vista d'insieme delle candidature: «a che punto sono?» (cap. 07.3). È il
    ''' contenuto di <c>registro.json</c>, ed è l'elenco che il pannello Home mostra.
    ''' </summary>
    ''' <remarks>
    ''' <b>Non è la fonte di verità</b> (deciso con Mirco il 2026-08-12): la verità sono le
    ''' cartelle-opportunità, ognuna con il suo <c>stato.json</c>. Questo è un indice, e un
    ''' indice si rigenera — è per questo che perderlo, o trovarlo che non torna coi fatti,
    ''' non è mai un guaio: si riscandisce e si riscrive (v. <see cref="ArchivioRegistro"/>).
    ''' </remarks>
    Public Class Registro

        ''' <summary>
        ''' Le candidature, nell'ordine delle cartelle: dalla più vecchia alla più recente,
        ''' perché il nome comincia con la data. Chi mostra l'elenco lo gira come gli serve.
        ''' </summary>
        Public ReadOnly Property Voci As New List(Of VoceRegistro)

        ''' <summary>
        ''' Se questa vista è stata ricostruita adesso invece di essere letta dal file: chi
        ''' l'ha chiesta sa così che il file su disco non è (ancora) aggiornato.
        ''' </summary>
        Public Property Rigenerato As Boolean

        ''' <summary>Cosa c'è da sapere; <c>Nothing</c> se non c'è niente da dire.</summary>
        Public Property Avviso As String

        ''' <summary>La voce di quella cartella, o <c>Nothing</c>.</summary>
        Public Function Trova(cartella As String) As VoceRegistro

            If String.IsNullOrWhiteSpace(cartella) Then Return Nothing

            Dim cercata As String = Path.GetFileName(cartella.Trim().TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))

            Return Voci.FirstOrDefault(
                Function(v) String.Equals(v.Cartella, cercata, StringComparison.OrdinalIgnoreCase))

        End Function

        ''' <summary>
        ''' Mette la voce al suo posto: se quella cartella c'è già la <b>sostituisce</b>
        ''' dov'era, altrimenti la aggiunge in fondo. L'ordine dell'elenco non cambia mai
        ''' sotto i piedi di chi lo sta guardando.
        ''' </summary>
        Public Sub Annota(voce As VoceRegistro)

            If voce Is Nothing Then Throw New ArgumentNullException(NameOf(voce))
            If String.IsNullOrWhiteSpace(voce.Cartella) Then
                Throw New ArgumentException("Una voce del registro vuole la sua cartella.", NameOf(voce))
            End If

            Dim gia As Integer = Voci.FindIndex(
                Function(v) String.Equals(v.Cartella, voce.Cartella, StringComparison.OrdinalIgnoreCase))

            If gia >= 0 Then
                Voci(gia) = voce
            Else
                Voci.Add(voce)
            End If

        End Sub

        ''' <summary>Quante candidature sono a quel punto: sono i contatori di P1 (cap. 07.3).</summary>
        ''' <remarks>
        ''' Si passa da <c>Where</c> e non si scrive <c>Voci.Count(…)</c>: in VB quella
        ''' forma non è il conteggio con la condizione, è la proprietà <c>Count</c> della
        ''' lista che qualcuno sta provando a indicizzare.
        ''' </remarks>
        Public Function Quante(stato As StatoOpportunita) As Integer
            Return Voci.Where(Function(v) v.Stato = stato).Count()
        End Function

        ''' <summary>
        ''' Le candidature spedite che aspettano da troppo: è il promemoria di follow-up
        ''' del cap. 07.3, in ordine di attesa — la più vecchia per prima.
        ''' </summary>
        ''' <param name="dopoQuantiGiorni">
        ''' Dopo quanti giorni di silenzio una candidatura entra nell'elenco. Il valore lo
        ''' sceglie l'utente nelle Impostazioni; <b>zero o meno spegne il promemoria</b>,
        ''' che è il modo di dire «non ricordarmelo» senza aggiungere un interruttore.
        ''' </param>
        ''' <param name="adesso">Il momento rispetto a cui si contano i giorni.</param>
        ''' <remarks>
        ''' Il promemoria è <b>passivo</b> (cap. 15.3): dice quali candidature sono ferme,
        ''' e il sollecito lo scrive l'utente. Qui non si manda niente e non si cambia
        ''' nessuno stato — questa funzione risponde a una domanda e basta.
        ''' </remarks>
        Public Function DaSollecitare(dopoQuantiGiorni As Integer, adesso As Date) As List(Of VoceRegistro)

            If dopoQuantiGiorni <= 0 Then Return New List(Of VoceRegistro)

            Return Voci.
                Where(Function(v) If(v.GiorniDiAttesa(adesso), -1) >= dopoQuantiGiorni).
                OrderByDescending(Function(v) v.GiorniDiAttesa(adesso).Value).
                ToList()

        End Function

        ''' <summary>
        ''' Se l'indice descrive <b>esattamente</b> le cartelle che ci sono: né una in più
        ''' (cancellata da Esplora file) né una in meno (arrivata da un backup, o scritta
        ''' mentre il registro non c'era).
        ''' </summary>
        Public Function Combacia(cartelle As IEnumerable(Of String)) As Boolean

            If cartelle Is Nothing Then Return False

            Dim suDisco As New HashSet(Of String)(
                cartelle.Select(Function(c) Path.GetFileName(c)), StringComparer.OrdinalIgnoreCase)

            Dim nelRegistro As New HashSet(Of String)(
                Voci.Select(Function(v) v.Cartella), StringComparer.OrdinalIgnoreCase)

            Return suDisco.SetEquals(nelRegistro)

        End Function

        ''' <summary>Il file come va scritto.</summary>
        Public Function ComeJson() As JsonObject

            Return New JsonObject From {
                {"voci", New JsonArray(Voci.Select(Function(v) CType(v.ComeJson(), JsonNode)).ToArray())}}

        End Function

        ''' <summary>
        ''' Il registro riletto da un testo JSON. Le voci che non dicono a quale cartella
        ''' appartengono si scartano in silenzio: tanto quel che conta è se l'insieme
        ''' combacia con le cartelle vere, e una voce mancante lo fa già fallire.
        ''' </summary>
        Public Shared Function DaJson(testo As String) As Registro

            Dim radice As JsonObject = TryCast(JsonNode.Parse(testo), JsonObject)
            If radice Is Nothing Then
                Throw New JsonException("Il registro dev'essere un oggetto JSON.")
            End If

            Dim letto As New Registro
            Dim elenco As JsonArray = TryCast(CampiJson.Nodo(radice, "voci"), JsonArray)
            If elenco Is Nothing Then Return letto

            For Each scritta As JsonNode In elenco
                Dim voce As VoceRegistro = VoceRegistro.DaJson(TryCast(scritta, JsonObject))
                If voce IsNot Nothing Then letto.Annota(voce)
            Next

            Return letto

        End Function

    End Class

    ''' <summary>
    ''' Il registro su disco: lo legge, lo riscrive e — quando non c'è o non torna — lo
    ''' <b>ricostruisce</b> scandendo le cartelle-opportunità (cap. 07.3, cap. 11.1).
    ''' </summary>
    ''' <remarks>
    ''' <para>Come <see cref="ArchivioRicerche"/>, in lettura non solleva mai: al peggio
    ''' rigenera. Ma la ragione è più forte, e non è la clemenza — è che qui non c'è
    ''' proprio niente da perdere: ogni riga di <c>registro.json</c> si ricava dalle
    ''' cartelle, che restano la fonte di verità. Un indice illeggibile è un indice da
    ''' rifare, non un dato perso.</para>
    ''' <para>La rigenerazione <b>non scrive</b>: restituisce la vista e la marca
    ''' <see cref="Registro.Rigenerato"/>, lasciando a chi l'ha chiesta il momento di
    ''' salvarla. È la stessa regola del montaggio del motore — chi apre l'applicazione e
    ''' la richiude senza fare niente non deve trovarsi dei file nuovi.</para>
    ''' </remarks>
    Public Class ArchivioRegistro

        ''' <summary>UTF-8 senza BOM, come gli altri JSON dell'applicazione.</summary>
        Private Shared ReadOnly Codifica As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        ''' <summary>Rientri e accenti in chiaro: il file si legge anche senza l'app (cap. 11.1).</summary>
        Private Shared ReadOnly FormatoLeggibile As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        Private ReadOnly _cartella As CartellaDati
        Private ReadOnly _opportunita As ArchivioOpportunita

        ''' <param name="cartella">La mappa della cartella dati.</param>
        ''' <param name="opportunita">
        ''' Le candidature su disco: sono loro la fonte di verità da cui l'indice si
        ''' ricostruisce.
        ''' </param>
        Public Sub New(cartella As CartellaDati, opportunita As ArchivioOpportunita)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))

            _cartella = cartella
            _opportunita = opportunita

        End Sub

        ''' <summary>
        ''' Il registro in vigore: quello del file se combacia con le cartelle, altrimenti
        ''' uno ricostruito da zero. Non solleva mai.
        ''' </summary>
        ''' <remarks>
        ''' Che il file manchi o non torni <b>non è un'anomalia da raccontare</b>, a
        ''' differenza di ciò che vale per la taratura o le ricerche (cap. 11.6): quelli
        ''' sono file dell'utente e un ripiego silenzioso gli nasconderebbe che il suo
        ''' ritocco non è in vigore. Qui il file non è di nessuno — è un riflesso delle
        ''' cartelle — e ricostruirlo restituisce esattamente la stessa verità. L'unica
        ''' cosa che vale la pena dire è se una cartella <b>non si è lasciata leggere</b>,
        ''' perché quella è una candidatura che sparisce dall'elenco.
        ''' </remarks>
        Public Function Carica() As Registro

            Dim cartelle As IReadOnlyList(Of String) = _opportunita.Elenco()

            If Not File.Exists(_cartella.FileRegistro) Then Return Rigenera(cartelle)

            Try
                Dim letto As Registro = Registro.DaJson(
                    File.ReadAllText(_cartella.FileRegistro, Encoding.UTF8))

                If letto.Combacia(cartelle.Select(Function(c) Path.GetFileName(c))) Then Return letto

            Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                       OrElse TypeOf ex Is UnauthorizedAccessException
                ' Un indice illeggibile non è un dato perso: si rifà.
            End Try

            Return Rigenera(cartelle)

        End Function

        ''' <summary>
        ''' Ricostruisce l'indice leggendo le cartelle-opportunità, una per una.
        ''' </summary>
        Public Function Rigenera() As Registro
            Return Rigenera(_opportunita.Elenco())
        End Function

        ''' <summary>
        ''' Lo stesso, sulle cartelle già elencate da chi chiama: scandire la cartella dati
        ''' due volte per la stessa domanda sarebbe uno spreco.
        ''' </summary>
        Private Function Rigenera(cartelle As IEnumerable(Of String)) As Registro

            Dim ricostruito As New Registro With {.Rigenerato = True}
            Dim saltate As New List(Of String)

            For Each cartella As String In cartelle

                Try
                    ricostruito.Annota(VoceRegistro.Da(_opportunita.Carica(cartella)))

                Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                           OrElse TypeOf ex Is UnauthorizedAccessException
                    ' Una cartella rovinata non porta via con sé tutte le altre — ma non
                    ' sparisce nemmeno in silenzio: è una candidatura che non si vedrebbe
                    ' più nell'elenco, e chi la cerca deve sapere perché non c'è.
                    saltate.Add(Path.GetFileName(cartella))
                End Try

            Next

            If saltate.Count > 0 Then
                ricostruito.Avviso =
                    $"Registro: {saltate.Count} " &
                    If(saltate.Count = 1, "candidatura non si è lasciata leggere",
                                          "candidature non si sono lasciate leggere") &
                    $" ({String.Join(", ", saltate)})."
            End If

            Return ricostruito

        End Function

        ''' <summary>Scrive il registro, creando la cartella dati se è la prima volta.</summary>
        Public Sub Salva(registro As Registro)

            If registro Is Nothing Then Throw New ArgumentNullException(NameOf(registro))

            _cartella.Assicura()

            Dim temporaneo As String = _cartella.FileRegistro & ".tmp"

            File.WriteAllText(temporaneo, registro.ComeJson().ToJsonString(FormatoLeggibile), Codifica)
            File.Move(temporaneo, _cartella.FileRegistro, overwrite:=True)

        End Sub

        ''' <summary>
        ''' Mette su disco un indice appena ricostruito — ma solo se c'è motivo. Dice se
        ''' l'ha scritto.
        ''' </summary>
        ''' <remarks>
        ''' I due casi in cui <b>non</b> si scrive: un indice che viene dal file è già
        ''' quello che sta su disco, e un indice vuoto non si crea dal nulla — chi apre
        ''' l'applicazione al primo avvio e la richiude senza fare niente non deve trovarsi
        ''' un file nuovo. Un <c>registro.json</c> che invece c'è già si tiene in riga
        ''' comunque, anche quando resta senza voci: un file rimasto indietro racconterebbe
        ''' candidature che non ci sono più.
        ''' </remarks>
        Public Function SalvaSeServe(registro As Registro) As Boolean

            If registro Is Nothing Then Throw New ArgumentNullException(NameOf(registro))

            If Not registro.Rigenerato Then Return False
            If registro.Voci.Count = 0 AndAlso Not File.Exists(_cartella.FileRegistro) Then Return False

            Salva(registro)
            registro.Rigenerato = False

            Return True

        End Function

        ''' <summary>
        ''' Rimette in riga l'indice dopo che un'opportunità è stata scritta su disco, e lo
        ''' salva. Restituisce il registro aggiornato, che è quello che l'elenco mostra.
        ''' </summary>
        ''' <remarks>
        ''' Va chiamata <b>dopo</b> <see cref="ArchivioOpportunita.Salva"/>, mai al posto
        ''' suo: prima si scrive la verità nella cartella, poi si aggiorna il riflesso. Se
        ''' questa dovesse fallire, la candidatura è comunque al sicuro e il registro si
        ''' rigenererà da sé alla prossima apertura.
        ''' </remarks>
        Public Function Annota(opportunita As Opportunita) As Registro

            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))

            Dim registro As Registro = Carica()
            registro.Annota(VoceRegistro.Da(opportunita))
            Salva(registro)

            registro.Rigenerato = False

            Return registro

        End Function

    End Class

End Namespace
