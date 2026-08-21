Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Le candidature su disco: una cartella per opportunità, col nome parlante — data,
    ''' azienda, ruolo — e dentro un file per artefatto (cap. 11.1).
    ''' </summary>
    ''' <remarks>
    ''' <para>La cartella nasce a T4 anche se la vista d'insieme è di T5: i documenti che
    ''' questa tappa genera sono i primi documenti veri del progetto, e vanno scritti
    ''' subito nella loro casa definitiva. Un documento generato e non ritrovabile domani
    ''' sarebbe un documento perso.</para>
    ''' <para>Si scrive <b>solo ciò che c'è</b>: un'opportunità confrontata ma non ancora
    ''' generata scrive l'annuncio, i giudizi e lo stato, e nient'altro. Nel flusso reale
    ''' è l'utente a decidere se proseguire (cap. 12, A5→A7), e un file vuoto scritto per
    ''' simmetria sarebbe una bugia sul disco.</para>
    ''' <para>Le stesse due cautele dell'archivio del profilo: scrittura atomica, e nomi
    ''' di cartella che non si sovrascrivono mai fra loro.</para>
    ''' </remarks>
    Public Class ArchivioOpportunita

        ''' <summary>UTF-8 <b>senza BOM</b>, come tutti i JSON che l'app scrive.</summary>
        Private Shared ReadOnly Codifica As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        ''' <summary>
        ''' JSON con rientri e accenti in chiaro: l'utente è padrone dei suoi dati anche
        ''' senza l'app (cap. 11.1), e un file pieno di <c>à</c> non si legge.
        ''' </summary>
        Private Shared ReadOnly Formato As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        ''' <summary>
        ''' La sottocartella dei file prodotti — <c>.docx</c>, <c>.pdf</c>, un giorno
        ''' l'<c>.eml</c> (cap. 11.1). Gli artefatti JSON restano nella cartella sopra:
        ''' quelli sono i dati della candidatura, questi sono i documenti che si mandano.
        ''' </summary>
        Public Const NomeCartellaOut As String = "out"

        ''' <summary>I nomi dei file dentro una cartella-opportunità (cap. 11.1).</summary>
        Public Const FileAnnuncio As String = "annuncio.json"
        Public Const FileGiudizi As String = "giudizi.json"
        Public Const FileMitigazioni As String = "mitigazioni.json"

        ''' <summary>Gli appunti di mira confermati nel brainstorming (T7c).</summary>
        Public Const FileAppunti As String = "appunti.json"

        Public Const FileCv As String = "cv.json"
        Public Const FileLettera As String = "lettera.json"

        ''' <summary>La bozza dell'email: destinatario, oggetto, corpo, allegati scelti (T6).</summary>
        Public Const FileEmail As String = "email.json"
        Public Const FileStato As String = "stato.json"

        Private ReadOnly _cartella As CartellaDati

        ''' <summary>Apre l'archivio sulla cartella dati indicata.</summary>
        Public Sub New(cartella As CartellaDati)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            _cartella = cartella

        End Sub

        ''' <summary>
        ''' Scrive l'opportunità nella sua cartella e restituisce il percorso.
        ''' </summary>
        ''' <remarks>
        ''' La prima volta la cartella si crea col nome parlante; le volte successive si
        ''' riscrive quella di prima — è l'opportunità che cresce (prima il confronto,
        ''' poi i documenti), non una nuova ogni volta.
        ''' </remarks>
        Public Function Salva(opportunita As Opportunita) As String

            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))

            _cartella.Assicura()

            If opportunita.Creata = Nothing Then opportunita.Creata = Date.Now
            opportunita.Aggiornata = Date.Now

            Dim cartella As String = If(opportunita.Cartella, CartellaLibera(opportunita))
            Directory.CreateDirectory(cartella)

            Scrivi(cartella, FileAnnuncio, opportunita.Annuncio)
            Scrivi(cartella, FileGiudizi, opportunita.Confronto)
            Scrivi(cartella, FileMitigazioni, opportunita.Mitigazioni)
            Scrivi(cartella, FileAppunti, opportunita.Appunti)
            Scrivi(cartella, FileCv, opportunita.Cv)
            Scrivi(cartella, FileLettera, opportunita.Lettera)
            Scrivi(cartella, FileEmail, opportunita.Email)
            Scrivi(cartella, FileStato, Stato(opportunita))

            opportunita.Cartella = cartella
            Return cartella

        End Function

        ''' <summary>
        ''' Dove vanno i documenti prodotti per un'opportunità già salvata.
        ''' </summary>
        ''' <remarks>
        ''' Sta qui e non in <see cref="CartellaDati"/> perché la cartella di
        ''' un'opportunità non è un percorso fisso: nasce col suo nome parlante quando
        ''' l'opportunità si salva, e questo archivio è l'unico che lo sa.
        ''' </remarks>
        Public Shared Function CartellaOut(opportunita As Opportunita) As String

            If opportunita Is Nothing Then Throw New ArgumentNullException(NameOf(opportunita))

            If String.IsNullOrEmpty(opportunita.Cartella) Then
                Throw New InvalidOperationException(
                    "L'opportunità non è ancora stata salvata: non ha una cartella dove mettere i documenti.")
            End If

            Return Path.Combine(opportunita.Cartella, NomeCartellaOut)

        End Function

        ''' <summary>
        ''' Le cartelle-opportunità esistenti, dalla più vecchia alla più recente: il nome
        ''' comincia con la data, quindi l'ordine alfabetico è già l'ordine del tempo.
        ''' </summary>
        Public Function Elenco() As IReadOnlyList(Of String)

            If Not Directory.Exists(_cartella.CartellaOpportunita) Then
                Return Array.Empty(Of String)()
            End If

            Return Directory.GetDirectories(_cartella.CartellaOpportunita).
                OrderBy(Function(c) Path.GetFileName(c), StringComparer.Ordinal).
                ToList()

        End Function

        ''' <summary>
        ''' La cartella dell'opportunità già catturata da <b>quell'indirizzo</b>, o
        ''' <c>Nothing</c> se quell'annuncio non c'è ancora.
        ''' </summary>
        ''' <remarks>
        ''' <para>Serve alla cattura (cap. 06.4): premere «Cattura annuncio» due volte sulla
        ''' stessa pagina non deve produrre due candidature identiche. L'identità è
        ''' l'<b>indirizzo</b>, che è l'unica cosa esatta che si ha in mano — a differenza
        ''' del nome della cartella, dove due offerte diverse della stessa azienda per lo
        ''' stesso ruolo si somigliano al punto da confondersi.</para>
        ''' <para><b>Il confronto è esatto</b>: lo stesso annuncio raggiunto per due strade
        ''' con parametri di tracciamento diversi non viene riconosciuto. È il verso giusto
        ''' in cui sbagliare — meglio un doppione in più che una cattura buona rifiutata.
        ''' Le opportunità nate prima di T5b non hanno link e non fanno mai da doppione.</para>
        ''' <para>Legge il solo <c>stato.json</c> di ogni cartella, non gli artefatti: è una
        ''' domanda che si fa a ogni cattura. Una cartella con lo stato illeggibile si
        ''' scavalca — l'utente è padrone dei suoi file, e uno rovinato a mano non deve
        ''' impedire di catturare.</para>
        ''' </remarks>
        Public Function CercaPerLink(link As String) As String

            If String.IsNullOrWhiteSpace(link) Then Return Nothing

            Dim cercato As String = link.Trim()

            For Each cartella As String In Elenco()

                Dim stato As JsonObject = Nothing

                Try
                    stato = TryCast(Leggi(cartella, FileStato), JsonObject)
                Catch ex As JsonException
                    Continue For
                End Try

                If stato Is Nothing Then Continue For

                If String.Equals(CampiJson.Testo(stato, "link"), cercato, StringComparison.OrdinalIgnoreCase) Then
                    Return cartella
                End If

            Next

            Return Nothing

        End Function

        ''' <summary>
        ''' Rilegge un'opportunità dalla sua cartella. I file che non ci sono restano
        ''' <c>Nothing</c>: è il modo in cui un'opportunità a metà si riapre a metà,
        ''' invece di sembrare rotta.
        ''' </summary>
        Public Function Carica(cartella As String) As Opportunita

            If String.IsNullOrWhiteSpace(cartella) Then
                Throw New ArgumentException("Manca la cartella dell'opportunità.", NameOf(cartella))
            End If

            If Not Directory.Exists(cartella) Then
                Throw New DirectoryNotFoundException($"L'opportunità «{cartella}» non c'è.")
            End If

            Dim o As New Opportunita With {
                .Cartella = cartella,
                .Annuncio = Leggi(cartella, FileAnnuncio),
                .Confronto = Leggi(cartella, FileGiudizi),
                .Mitigazioni = Leggi(cartella, FileMitigazioni),
                .Appunti = Leggi(cartella, FileAppunti),
                .Cv = Leggi(cartella, FileCv),
                .Lettera = Leggi(cartella, FileLettera),
                .Email = Leggi(cartella, FileEmail)}

            RileggiStato(o, TryCast(Leggi(cartella, FileStato), JsonObject))

            Return o

        End Function

        ''' <summary>
        ''' Lo <c>stato.json</c>: lo stato del ciclo di vita con le date dei suoi
        ''' passaggi, le date della cartella, la lingua, la versione di profilo, la
        ''' provenienza e il punteggio di quel giorno (cap. 07.3, cap. 11.1).
        ''' </summary>
        ''' <remarks>
        ''' Lo <b>stato</b> e le sue date arrivano con T5c: è l'unico campo che il disegno
        ''' del cap. 11.1 aveva promesso e T4 non aveva ancora scritto, ed è per questo che
        ''' le cartelle di T4 si riaprono deducendolo invece di essere migrate
        ''' (<see cref="StatiOpportunita.Dedotto"/>).
        ''' </remarks>
        Private Shared Function Stato(o As Opportunita) As JsonObject

            ' Da T9c. L'esito si scrive <b>sempre</b>, anche quando non c'è, e qui la
            ' riga vuota racconta invece di ingombrare: «"stato": "inviata"» con
            ' «"esito": null» accanto è esattamente il caso che fa scattare il promemoria
            ' di follow-up (cap. 07.3). È l'opposto della scelta fatta per «rifinitura»
            ' qui sotto, che è un blocco intero e tace quando non ha niente da dire.
            Dim esito As String = If(o.Esito.HasValue, EsitiCandidatura.Nome(o.Esito.Value), Nothing)

            Dim scritto As New JsonObject From {
                {"creata", CampiJson.Quando(o.Creata)},
                {"aggiornata", CampiJson.Quando(o.Aggiornata)},
                {"stato", StatiOpportunita.Nome(o.Stato)},
                {"esito", esito},
                {"date_stati", StatiOpportunita.DateComeJson(o.DateStati)},
                {"lingua", o.Lingua},
                {"versione_profilo", o.VersioneProfilo},
                {"azienda", o.Azienda},
                {"titolo", o.Titolo},
                {"fonte", o.Fonte},
                {"link", o.Link}}

            ' Da T7b. Il campo c'è solo quando la rifinitura ha davvero cambiato qualcosa:
            ' scriverlo vuoto direbbe «è stata fatta e non ha toccato niente» esattamente
            ' come non scriverlo, e un file si legge meglio senza righe che non raccontano.
            If o.PrimaDellaRifinitura IsNot Nothing Then
                scritto("rifinitura") = o.PrimaDellaRifinitura.DeepClone()
            End If

            If o.Match IsNot Nothing Then scritto("match") = o.Match.ComeJson()

            Return scritto

        End Function

        ''' <summary>Rimette nell'opportunità ciò che lo <c>stato.json</c> conserva.</summary>
        Private Shared Sub RileggiStato(o As Opportunita, stato As JsonObject)

            ' Nemmeno lo stato.json: una cartella rimasta senza — cancellato a mano, o
            ' un salvataggio interrotto prima di scriverlo — si riapre lo stesso, con lo
            ' stato che i suoi file dichiarano.
            If stato Is Nothing Then
                o.Stato = StatiOpportunita.Dedotto(o)
                Return
            End If

            o.Creata = CampiJson.Istante(stato, "creata")
            o.Aggiornata = CampiJson.Istante(stato, "aggiornata")

            ' Da T5c. Nelle cartelle scritte prima il campo «stato» non c'è, e nemmeno un
            ' valore che non riconosciamo vale come dichiarazione: in entrambi i casi lo
            ' stato si deduce dai file presenti invece di riscrivere all'indietro i file
            ' dell'utente (cap. 07.3).
            Dim dichiarato As StatoOpportunita? = StatiOpportunita.DaNome(CampiJson.Testo(stato, "stato"))
            Dim letto As StatoOpportunita = If(dichiarato.HasValue, dichiarato.Value, StatiOpportunita.Dedotto(o))

            ' Da T9c. Stato ed esito si sono scritti insieme e si leggono insieme: da soli
            ' possono contraddirsi, e chi decide come è una regola sola (cap. 07.3).
            Dim esito As EsitoCandidatura? = EsitiCandidatura.DaNome(CampiJson.Testo(stato, "esito"))
            EsitiCandidatura.Concorda(letto, esito)

            o.Stato = letto
            o.Esito = esito

            StatiOpportunita.RiempiDate(o.DateStati, TryCast(CampiJson.Nodo(stato, "date_stati"), JsonObject))

            Dim lingua As String = CampiJson.Testo(stato, "lingua")
            If Not String.IsNullOrEmpty(lingua) Then o.Lingua = lingua

            o.VersioneProfilo = CampiJson.Testo(stato, "versione_profilo")

            ' Da T5b in poi. Nei file scritti prima questi due campi non ci sono, e
            ' <c>CampiJson.Testo</c> restituisce <c>Nothing</c>: un'opportunità vecchia si
            ' riapre senza fonte né link, che è esattamente com'era.
            o.Fonte = CampiJson.Testo(stato, "fonte")
            o.Link = CampiJson.Testo(stato, "link")

            ' Da T7b. Nelle cartelle scritte prima non c'è, e va benissimo: vuol dire una
            ' candidatura i cui documenti non sono passati dalla rifinitura, che è
            ' esattamente com'era.
            o.PrimaDellaRifinitura = CampiJson.Nodo(stato, "rifinitura")

            Dim match As JsonObject = TryCast(CampiJson.Nodo(stato, "match"), JsonObject)
            If match Is Nothing Then Return

            o.Match = New RisultatoMatch With {
                .MatchFinale = CampiJson.Intero(match, "match_finale"),
                .Stelle = CampiJson.Numero(match, "stelle"),
                .ScoreBase = CampiJson.Intero(match, "score_base"),
                .NumeroLlm = CampiJson.Numero(match, "numero_llm"),
                .ScartoTagliato = CampiJson.Vero(match, "scarto_tagliato"),
                .GateEliminatorio = CampiJson.Vero(match, "gate_eliminatorio"),
                .Nota = CampiJson.Testo(match, "nota")}

        End Sub

        ''' <summary>
        ''' Il nome della cartella: data, azienda e ruolo, ridotti a lettere, cifre e
        ''' trattini perché siano nomi che qualunque disco accetta e un umano riconosce.
        ''' Se in quel giorno una cartella con lo stesso nome c'è già — due candidature
        ''' allo stesso posto — si aggiunge un progressivo invece di scriverci sopra.
        ''' </summary>
        Private Function CartellaLibera(o As Opportunita) As String

            Dim pezzi As New List(Of String) From {
                o.Creata.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}

            For Each parte As String In {o.Azienda, o.Titolo}
                Dim ridotta As String = Sillabario(parte)
                If ridotta.Length > 0 Then pezzi.Add(ridotta)
            Next

            ' Un annuncio anonimo e senza titolo esiste: meglio una cartella che si chiama
            ' «opportunita» di una che si chiama solo con la data e si confonde con le altre.
            If pezzi.Count = 1 Then pezzi.Add("opportunita")

            Dim radice As String = String.Join("_", pezzi)
            Dim nome As String = radice
            Dim progressivo As Integer = 1

            While Directory.Exists(Path.Combine(_cartella.CartellaOpportunita, nome))
                progressivo += 1
                nome = $"{radice}_{progressivo}"
            End While

            Return Path.Combine(_cartella.CartellaOpportunita, nome)

        End Function

        ''' <summary>
        ''' Da «Rossi S.p.A.» a «rossi-s-p-a»: minuscole, accenti sciolti nelle lettere
        ''' che li reggono, tutto il resto diventa un trattino. Si taglia a 40 caratteri
        ''' perché il percorso intero resti maneggevole.
        ''' </summary>
        Private Shared Function Sillabario(testo As String) As String

            If String.IsNullOrWhiteSpace(testo) Then Return String.Empty

            Dim sciolto As String = testo.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD)

            Dim costruito As New StringBuilder()
            For Each c As Char In sciolto

                ' I segni diacritici sciolti dalla normalizzazione si buttano: resta la
                ' lettera che li reggeva (à → a), che è ciò che serve in un nome di file.
                If Globalization.CharUnicodeInfo.GetUnicodeCategory(c) = UnicodeCategory.NonSpacingMark Then
                    Continue For
                End If

                If Char.IsLetterOrDigit(c) Then
                    costruito.Append(c)
                ElseIf costruito.Length > 0 AndAlso costruito(costruito.Length - 1) <> "-"c Then
                    costruito.Append("-"c)
                End If

            Next

            Return costruito.ToString().Trim("-"c)

        End Function

        ''' <summary>Scrive un artefatto; se non c'è, non scrive nulla.</summary>
        Private Shared Sub Scrivi(cartella As String, nomeFile As String, artefatto As JsonNode)

            If artefatto Is Nothing Then Return

            Dim percorso As String = Path.Combine(cartella, nomeFile)
            Dim temporaneo As String = percorso & ".tmp"

            File.WriteAllText(temporaneo, artefatto.ToJsonString(Formato), Codifica)
            File.Move(temporaneo, percorso, overwrite:=True)

        End Sub

        ''' <summary>Rilegge un artefatto; <c>Nothing</c> se quel file non c'è.</summary>
        Private Shared Function Leggi(cartella As String, nomeFile As String) As JsonNode

            Dim percorso As String = Path.Combine(cartella, nomeFile)
            If Not File.Exists(percorso) Then Return Nothing

            Return JsonNode.Parse(File.ReadAllText(percorso, Encoding.UTF8))

        End Function

    End Class

End Namespace
