Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Dati

    ''' <summary>
    ''' Che cosa succederebbe a ripristinare questo backup: cosa contiene, cosa
    ''' sovrascrive e cosa resta dov'è (cap. 11.4, passo 2).
    ''' </summary>
    ''' <remarks>
    ''' Le frasi stanno qui e non nella finestra perché sono la parte che l'utente deve
    ''' <b>leggere prima di confermare</b>: un collaudo le può verificare senza aprire
    ''' niente, e una promessa che si può collaudare è una promessa che regge.
    ''' </remarks>
    Public Class AnteprimaRipristino

        ''' <summary>Quando il backup è stato esportato.</summary>
        Public Property Data As Date

        Public Property ConProfilo As Boolean
        Public Property VersioniNelBackup As Integer
        Public Property ConCvBase As Boolean
        Public Property CandidatureNelBackup As Integer

        ''' <summary>Il giorno del profilo che sta nel backup: la sua versione più recente.</summary>
        Public Property DataDelProfilo As Date?

        ''' <summary>Quando è stato salvato il profilo che c'è adesso; <c>Nothing</c> se non ce n'è.</summary>
        Public Property ProfiloAttuale As Date?

        ''' <summary>Le candidature che il ripristino riscrive, perché hanno lo stesso nome.</summary>
        Public ReadOnly Property CandidatureRiscritte As New List(Of String)

        ''' <summary>Quante candidature già presenti il backup non nomina, e che restano dove sono.</summary>
        Public Property CandidatureCheRestano As Integer

        ''' <summary>Che cosa c'è dentro il file, riga per riga.</summary>
        Public Function CosaContiene() As IReadOnlyList(Of String)

            Dim righe As New List(Of String)

            If ConProfilo Then
                righe.Add(If(DataDelProfilo.HasValue,
                             $"Il profilo, come era il {Giorno(DataDelProfilo.Value)}",
                             "Il profilo"))
            End If

            If VersioniNelBackup > 0 Then
                righe.Add($"{VersioniNelBackup} {Plurale(VersioniNelBackup, "versione", "versioni")} nello storico")
            End If

            If ConCvBase Then righe.Add("Il 📄 CV-1 base")

            If CandidatureNelBackup > 0 Then
                righe.Add($"{CandidatureNelBackup} {Plurale(CandidatureNelBackup, "candidatura", "candidature")}")
            End If

            If righe.Count = 0 Then righe.Add("Niente: questo backup è vuoto.")

            Return righe

        End Function

        ''' <summary>
        ''' Che cosa cambia sul disco, riga per riga. È il testo che il cap. 11.4 vuole
        ''' davanti all'utente <b>prima</b> della conferma: non «sei sicuro?», ma cosa
        ''' esattamente prende il posto di cosa.
        ''' </summary>
        Public Function CosaSovrascrive() As IReadOnlyList(Of String)

            Dim righe As New List(Of String)

            If ConProfilo Then

                If ProfiloAttuale.HasValue Then
                    righe.Add($"Il profilo di adesso (salvato il {Giorno(ProfiloAttuale.Value)}) viene sostituito. " &
                              "Prima però finisce nello storico: da lì si può riprendere.")
                Else
                    righe.Add("Adesso un profilo non c'è: questo lo mette al suo posto, senza sostituire niente.")
                End If

            End If

            If CandidatureRiscritte.Count > 0 Then
                righe.Add($"{CandidatureRiscritte.Count} " &
                          $"{Plurale(CandidatureRiscritte.Count, "candidatura si riscrive", "candidature si riscrivono")}, " &
                          $"perché nel backup c'è {Plurale(CandidatureRiscritte.Count, "la sua cartella", "la loro cartella")} " &
                          $"con lo stesso nome: {String.Join(", ", CandidatureRiscritte)}")
            End If

            If CandidatureCheRestano > 0 Then
                righe.Add($"{CandidatureCheRestano} " &
                          $"{Plurale(CandidatureCheRestano, "candidatura resta dov'è", "candidature restano dove sono")}: " &
                          "un ripristino non cancella quello che il backup non nomina.")
            End If

            If righe.Count = 0 Then righe.Add("Niente: non c'è nulla che questo backup possa sostituire.")

            Return righe

        End Function

        Private Shared Function Giorno(quando As Date) As String
            Return quando.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("it-IT"))
        End Function

        Private Shared Function Plurale(quanti As Integer, uno As String, molti As String) As String
            Return If(quanti = 1, uno, molti)
        End Function

    End Class

    ''' <summary>Che cosa il ripristino ha davvero fatto (cap. 11.4).</summary>
    Public Class EsitoRipristino

        ''' <summary>La versione con cui il profilo di prima è stato messo in salvo, se ce n'era uno.</summary>
        Public Property ProfiloMessoInSalvo As String

        Public Property ProfiloRipristinato As Boolean
        Public Property VersioniAggiunte As Integer
        Public Property CvBaseRipristinato As Boolean
        Public Property CandidatureRipristinate As Integer

        ''' <summary>
        ''' Quello che il backup conteneva e che si è rifiutato di scrivere: nomi che non
        ''' sono nomi di file. Non è un dettaglio da nascondere — se c'è dentro qualcosa
        ''' del genere, chi ripristina deve saperlo.
        ''' </summary>
        Public ReadOnly Property Rifiutati As New List(Of String)

    End Class

    ''' <summary>
    ''' Backup e ripristino, la funzione F7 (cap. 11.4): un solo file <c>.json</c> che
    ''' porta via i dati dell'utente e sa rimetterli al loro posto.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Legge i file grezzi, non gli oggetti.</b> Il profilo, il CV base e gli
    ''' artefatti delle candidature entrano nel backup così come stanno sul disco: un
    ''' campo che il programma di oggi non modella deve poter tornare indietro identico.
    ''' L'unica cosa che non viaggia grezza è il registro, che è un indice rigenerabile
    ''' (cap. 07.3) e infatti al ritorno si <b>ricostruisce dalle cartelle</b>.</para>
    ''' <para><b>Non cancella mai.</b> Il ripristino rimette al loro posto le cose che il
    ''' backup nomina e lascia stare tutte le altre: le candidature che il file non
    ''' contiene restano dove sono. Cancellare è un altro gesto, sta nelle Impostazioni e
    ''' costa una parola scritta a mano (cap. 11.5).</para>
    ''' <para><b>I nomi che arrivano da fuori sono nomi di file, non percorsi.</b> Un file
    ''' di backup si può scrivere a mano, e uno costruito male — <c>..\..\altro</c> al
    ''' posto del nome di una cartella — non deve poter scrivere fuori dalla cartella
    ''' dati. Chi non passa il controllo finisce fra i <see cref="EsitoRipristino.Rifiutati"/>,
    ''' scritto in chiaro.</para>
    ''' </remarks>
    Public Class ArchivioBackup

        ''' <summary>UTF-8 <b>senza BOM</b>, come tutti i JSON che l'app scrive.</summary>
        Private Shared ReadOnly Codifica As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        ''' <summary>Rientri e accenti in chiaro: un backup si deve poter leggere anche senza l'app (cap. 11.1).</summary>
        Private Shared ReadOnly FormatoLeggibile As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        Private ReadOnly _cartella As CartellaDati
        Private ReadOnly _profilo As ArchivioProfilo
        Private ReadOnly _opportunita As ArchivioOpportunita
        Private ReadOnly _registro As ArchivioRegistro

        Public Sub New(cartella As CartellaDati, profilo As ArchivioProfilo,
                       opportunita As ArchivioOpportunita, registro As ArchivioRegistro)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            If profilo Is Nothing Then Throw New ArgumentNullException(NameOf(profilo))
            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))
            If registro Is Nothing Then Throw New ArgumentNullException(NameOf(registro))

            _cartella = cartella
            _profilo = profilo
            _opportunita = opportunita
            _registro = registro

        End Sub

        ''' <summary>
        ''' Raccoglie quel che c'è sul disco. Ciò che manca non c'è: un backup fatto prima
        ''' della prima candidatura è un backup del solo profilo, e va bene così.
        ''' </summary>
        Public Function Componi(contenuto As ContenutoBackup) As Backup

            Dim fatto As New Backup With {.Data = Date.Now}

            fatto.Profilo = LeggiNodo(_cartella.FileProfilo)
            fatto.ProfiloSalvato = _profilo.UltimoSalvataggio
            fatto.CvBase = LeggiNodo(_cartella.FileCvBase)

            For Each versione As String In _profilo.Versioni()

                Dim nodo As JsonNode = LeggiNodo(Path.Combine(_cartella.CartellaStorico, versione & ".json"))
                If nodo Is Nothing Then Continue For

                fatto.Storico.Add(New VersioneInBackup With {.Versione = versione, .Profilo = nodo})

            Next

            If contenuto = ContenutoBackup.Tutto Then

                fatto.Registro = LeggiNodo(_cartella.FileRegistro)

                For Each cartella As String In _opportunita.Elenco()

                    Dim candidatura As New OpportunitaInBackup With {.Cartella = Path.GetFileName(cartella)}

                    ' Solo i JSON del primo livello: la sottocartella «out» tiene i
                    ' documenti impaginati, che sono file normali e si copiano da sé
                    ' (cap. 11.4). Metterli qui dentro, per giunta in base64, gonfierebbe
                    ' il backup di roba che si rigenera con un bottone.
                    For Each file As String In Directory.GetFiles(cartella, "*.json").OrderBy(Function(f) f, StringComparer.Ordinal)

                        Dim nodo As JsonNode = LeggiNodo(file)
                        If nodo IsNot Nothing Then candidatura.File(Path.GetFileName(file)) = nodo

                    Next

                    If candidatura.File.Count > 0 Then fatto.Opportunita.Add(candidatura)

                Next

            End If

            Return fatto

        End Function

        ''' <summary>Scrive il backup dove dice l'utente: anche su una chiavetta, se vuole.</summary>
        Public Sub Scrivi(backup As Backup, percorso As String)

            If backup Is Nothing Then Throw New ArgumentNullException(NameOf(backup))
            If String.IsNullOrWhiteSpace(percorso) Then
                Throw New ArgumentException("Manca il percorso del file di backup.", NameOf(percorso))
            End If

            Dim cartella As String = Path.GetDirectoryName(Path.GetFullPath(percorso))
            If Not String.IsNullOrEmpty(cartella) Then Directory.CreateDirectory(cartella)

            ' Dal temporaneo accanto, come tutti i file che questa applicazione scrive: un
            ' backup troncato a metà è peggio di un backup che non c'è, perché sembra esserci.
            Dim temporaneo As String = percorso & ".tmp"

            File.WriteAllText(temporaneo, backup.ComeJson().ToJsonString(FormatoLeggibile), Codifica)
            File.Move(temporaneo, percorso, overwrite:=True)

        End Sub

        ''' <summary>Rilegge un file di backup scelto dall'utente.</summary>
        Public Shared Function Leggi(percorso As String) As Backup
            Return Backup.DaTesto(File.ReadAllText(percorso, Encoding.UTF8))
        End Function

        ''' <summary>
        ''' Il nome che si propone nel dialogo di salvataggio: dice cosa c'è dentro e di
        ''' che giorno è, perché una cartella piena di backup sia ancora leggibile fra sei mesi.
        ''' </summary>
        Public Shared Function NomeProposto(contenuto As ContenutoBackup, quando As Date) As String

            Dim che As String = If(contenuto = ContenutoBackup.Tutto, "completo", "profilo")

            Return $"trovalavoro-{che}-{quando.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.json"

        End Function

        ''' <summary>
        ''' Che cosa succederebbe a ripristinare questo backup, guardando anche cosa c'è
        ''' adesso sul disco. Non tocca niente: è il passo 2 del cap. 11.4.
        ''' </summary>
        Public Function Anteprima(backup As Backup) As AnteprimaRipristino

            If backup Is Nothing Then Throw New ArgumentNullException(NameOf(backup))

            Dim detto As New AnteprimaRipristino With {
                .Data = backup.Data,
                .ConProfilo = backup.Profilo IsNot Nothing,
                .VersioniNelBackup = backup.Storico.Count,
                .ConCvBase = backup.CvBase IsNot Nothing,
                .CandidatureNelBackup = backup.Opportunita.Count,
                .DataDelProfilo = GiornoDelProfilo(backup),
                .ProfiloAttuale = _profilo.UltimoSalvataggio}

            Dim presenti As HashSet(Of String) = _opportunita.Elenco().
                Select(Function(c) Path.GetFileName(c)).
                ToHashSet(StringComparer.OrdinalIgnoreCase)

            Dim nelBackup As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For Each candidatura As OpportunitaInBackup In backup.Opportunita

                nelBackup.Add(candidatura.Cartella)

                If presenti.Contains(candidatura.Cartella) Then
                    detto.CandidatureRiscritte.Add(candidatura.Cartella)
                End If

            Next

            detto.CandidatureCheRestano = presenti.Where(Function(c) Not nelBackup.Contains(c)).Count()

            Return detto

        End Function

        ''' <summary>
        ''' Rimette al loro posto i dati del backup. Chiamarlo è il passo 3 del cap. 11.4:
        ''' ci si arriva <b>dopo</b> che l'utente ha visto l'anteprima e ha confermato.
        ''' </summary>
        Public Function Ripristina(backup As Backup) As EsitoRipristino

            If backup Is Nothing Then Throw New ArgumentNullException(NameOf(backup))

            Dim esito As New EsitoRipristino
            _cartella.Assicura()

            If backup.Profilo IsNot Nothing Then

                ' Prima si mette in salvo quello di adesso, poi si scrive: il ripristino
                ' non deve mai poter distruggere l'unico profilo buono (cap. 11.4).
                esito.ProfiloMessoInSalvo = _profilo.ArchiviaIlCorrente()

                For Each versione As VersioneInBackup In backup.Storico

                    If versione.Profilo Is Nothing Then Continue For

                    Try
                        If _profilo.RiportaNelloStorico(versione.Versione, Testo(versione.Profilo)) Then
                            esito.VersioniAggiunte += 1
                        End If
                    Catch ex As ArgumentException
                        esito.Rifiutati.Add($"storico/{versione.Versione}")
                    End Try

                Next

                _profilo.RipristinaCorrente(Testo(backup.Profilo))
                esito.ProfiloRipristinato = True

            End If

            If backup.CvBase IsNot Nothing Then
                _profilo.RipristinaCvBase(Testo(backup.CvBase))
                esito.CvBaseRipristinato = True
            End If

            For Each candidatura As OpportunitaInBackup In backup.Opportunita

                If Not NomeDiCartellaAmmesso(candidatura.Cartella) Then
                    esito.Rifiutati.Add(candidatura.Cartella)
                    Continue For
                End If

                Dim dove As String = Path.Combine(_cartella.CartellaOpportunita, candidatura.Cartella)
                Directory.CreateDirectory(dove)

                Dim scritti As Integer = 0

                For Each file As KeyValuePair(Of String, JsonNode) In candidatura.File

                    If Not NomeDiFileAmmesso(file.Key) Then
                        esito.Rifiutati.Add($"{candidatura.Cartella}/{file.Key}")
                        Continue For
                    End If

                    If file.Value Is Nothing Then Continue For

                    ScriviInModoAtomico(Path.Combine(dove, file.Key), Testo(file.Value))
                    scritti += 1

                Next

                If scritti > 0 Then esito.CandidatureRipristinate += 1

            Next

            ' L'indice si ricostruisce dalle cartelle invece di essere ricopiato dal
            ' backup: è la regola del cap. 07.3 — le cartelle sono la fonte di verità — ed
            ' è anche l'unica cosa giusta da fare qui, perché adesso sul disco ci sono
            ' insieme le candidature ripristinate e quelle che c'erano già.
            If backup.Opportunita.Count > 0 Then _registro.Salva(_registro.Rigenera())

            Return esito

        End Function

        ''' <summary>Il giorno del profilo che sta nel backup: la versione più recente, o il giorno dell'export.</summary>
        Private Shared Function GiornoDelProfilo(backup As Backup) As Date?

            If backup.Profilo Is Nothing Then Return Nothing

            ' Il campo scritto insieme al profilo, quando c'è: è la sua data vera.
            If backup.ProfiloSalvato.HasValue Then Return backup.ProfiloSalvato

            ' Per i backup che non ce l'hanno si deduce dall'ultima versione dello storico.
            ' I nomi delle versioni sono date al secondo, e l'ordine alfabetico è già
            ' quello del tempo (v. ArchivioProfilo.Versioni).
            Dim ultima As VersioneInBackup = backup.Storico.LastOrDefault()

            If ultima IsNot Nothing Then

                Dim quando As Date
                If Date.TryParseExact(ultima.Versione.Substring(0, Math.Min(17, ultima.Versione.Length)),
                                      "yyyy-MM-dd_HHmmss", CultureInfo.InvariantCulture,
                                      DateTimeStyles.None, quando) Then
                    Return quando
                End If

            End If

            Return If(backup.Data = Nothing, CType(Nothing, Date?), backup.Data)

        End Function

        ''' <summary>Il JSON in testo, nella stessa forma leggibile in cui l'app scrive i suoi file.</summary>
        Private Shared Function Testo(nodo As JsonNode) As String
            Return nodo.ToJsonString(FormatoLeggibile)
        End Function

        ''' <summary>Il contenuto di un file JSON, o <c>Nothing</c> se non c'è o non si lascia leggere.</summary>
        Private Shared Function LeggiNodo(percorso As String) As JsonNode

            If Not File.Exists(percorso) Then Return Nothing

            Try
                Return JsonNode.Parse(File.ReadAllText(percorso, Encoding.UTF8))
            Catch ex As JsonException
                ' Un file rovinato non deve impedire il backup di tutto il resto: quello
                ' che non si legge non entra, e resta dov'è.
                Return Nothing
            End Try

        End Function

        ''' <summary>Se è un nome di cartella e non un percorso travestito.</summary>
        Private Shared Function NomeDiCartellaAmmesso(nome As String) As Boolean

            If String.IsNullOrWhiteSpace(nome) Then Return False
            If nome = "." OrElse nome = ".." Then Return False
            If Not nome.Equals(Path.GetFileName(nome), StringComparison.Ordinal) Then Return False

            Return nome.IndexOfAny(Path.GetInvalidFileNameChars()) < 0

        End Function

        ''' <summary>Se è il nome di un artefatto JSON e non qualcos'altro.</summary>
        Private Shared Function NomeDiFileAmmesso(nome As String) As Boolean

            If Not NomeDiCartellaAmmesso(nome) Then Return False

            Return nome.EndsWith(".json", StringComparison.OrdinalIgnoreCase)

        End Function

        ''' <summary>Scrive passando da un temporaneo accanto: o c'è il file di prima, o c'è quello nuovo.</summary>
        Private Shared Sub ScriviInModoAtomico(percorso As String, testo As String)

            Dim temporaneo As String = percorso & ".tmp"

            File.WriteAllText(temporaneo, testo, Codifica)
            File.Move(temporaneo, percorso, overwrite:=True)

        End Sub

    End Class

End Namespace
