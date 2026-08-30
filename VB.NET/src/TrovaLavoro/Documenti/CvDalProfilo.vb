Imports System.Linq
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Dati

Namespace Documenti

    ''' <summary>
    ''' Il 📄 CV base ricavato dal profilo <b>senza chiamare l'AI</b>: quel che il CV
    ''' contiene di già scritto nel profilo, messo nella forma del CV.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché si può fare.</b> Lo schema del CV base (prompt <c>cv_base</c>,
    ''' sezione 1) divide i suoi campi in due: i <b>campi-fatto</b>, che il modello
    ''' <i>ricopia</i> dal profilo — nome, recapiti, patente, ruolo, azienda, durata,
    ''' competenze, formazione — e i due soli <b>campi-prosa</b> che scrive davvero, il
    ''' <c>sommario</c> e le <c>descrizione</c> delle esperienze. I campi-fatto non hanno
    ''' bisogno di nessuno per essere ricopiati, e qui si ricopiano.</para>
    ''' <para><b>Perché serve.</b> Il CV base dell'AI è una fotografia: nasce da una certa
    ''' versione del profilo e da quel momento invecchia, e finora l'unico modo di sapere
    ''' quanto fosse invecchiato era leggere la riga che lo dichiara. Questa composizione
    ''' invece non può invecchiare, perché non è salvata da nessuna parte: si rifà a ogni
    ''' sguardo dal profilo di adesso. Aggiungi un'esperienza e compare, la togli e sparisce.
    ''' Le due cose non si sostituiscono — stanno una accanto all'altra in P2, ed è la
    ''' <b>differenza</b> fra loro a dire cosa, esattamente, ci ha messo l'AI.</para>
    ''' <para><b>Che cosa non fa, e lo dice.</b> Il <c>sommario</c> resta vuoto: è prosa,
    ''' e riempirlo qui vorrebbe dire scriverla senza il modello — cioè inventare un tono
    ''' che poi il CV vero non avrebbe. Le <c>descrizione</c> non sono riformulate:
    ''' riportano <c>cosa_facevo</c> <b>com'è scritto nel profilo</b>. Questa è la sola
    ''' differenza sostanziale col documento generato, ed è a favore della
    ''' verità: qui non c'è una riga che l'utente non abbia scritto di suo pugno.</para>
    ''' <para><b>Perché è un modulo puro.</b> Non tocca il disco, non ha stato, non
    ''' conosce l'interfaccia: prende un profilo e torna un artefatto. Così il banco lo
    ''' interroga senza montare niente, e chi lo guarda a video —
    ''' <see cref="Impaginazione.PaginaCv"/> — è lo stesso che impagina il CV vero,
    ''' e non una seconda impaginazione destinata a divergere.</para>
    ''' </remarks>
    Public Module CvDalProfilo

        ''' <summary>Il valore con cui il profilo dichiara di avere la patente.</summary>
        Private Const PatenteDichiarata As String = "sì"

        ''' <summary>
        ''' Compone il CV base dal profilo dato, coi soli campi che il profilo già contiene.
        ''' </summary>
        ''' <param name="profilo">
        ''' Il profilo di adesso — anche quello che si sta correggendo a video e non è
        ''' ancora stato salvato: è precisamente il caso per cui questa composizione esiste.
        ''' </param>
        ''' <returns>
        ''' Un artefatto nella forma del prompt <c>cv_base</c>. Un profilo nullo dà un CV
        ''' vuoto ma valido, non un'eccezione: a video quel caso è «non c'è ancora niente»,
        ''' e non è un errore.
        ''' </returns>
        Public Function Componi(profilo As Profilo) As JsonObject

            If profilo Is Nothing Then profilo = New Profilo()

            Return New JsonObject From {
                {"tipo", "cv_base"},
                {"intestazione", Intestazione(profilo)},
                {"sommario", ""},
                {"esperienze_professionali", EsperienzeProfessionali(profilo)},
                {"altre_esperienze", AltreEsperienze(profilo)},
                {"competenze", Elenco(profilo.Competenze)},
                {"formazione", Formazione(profilo)}}

        End Function

        ''' <summary>Nome, recapiti e patente: tutti campi-fatto, ricopiati.</summary>
        Private Function Intestazione(profilo As Profilo) As JsonObject

            Return New JsonObject From {
                {"nome", Pulito(profilo.Nome)},
                {"email", Pulito(profilo.Contatti.Email)},
                {"telefono", Pulito(profilo.Contatti.Telefono)},
                {"citta", Pulito(profilo.Contatti.Citta)},
                {"link", Pulito(profilo.Contatti.Link)},
                {"patente", Patente(profilo.Patente)}}

        End Function

        ''' <summary>
        ''' Le categorie di patente in una riga, o stringa vuota.
        ''' </summary>
        ''' <remarks>
        ''' Solo se il profilo dichiara di averla: un elenco di categorie rimasto lì da
        ''' una risposta cambiata poi in «no» non deve ricomparire nel CV. È la stessa
        ''' condizione che il prompt impone al modello.
        ''' </remarks>
        Private Function Patente(dichiarazione As PatenteProfilo) As String

            If dichiarazione Is Nothing OrElse dichiarazione.Ha <> PatenteDichiarata Then Return ""

            Dim categorie As String() = dichiarazione.Categorie _
                .Select(Function(c) Pulito(c)) _
                .Where(Function(c) c.Length > 0) _
                .ToArray()

            Return String.Join(", ", categorie)

        End Function

        ''' <summary>
        ''' Le esperienze formali: ruolo, azienda e durata ricopiati, la descrizione presa
        ''' da <c>cosa_facevo</c> senza riformularla.
        ''' </summary>
        Private Function EsperienzeProfessionali(profilo As Profilo) As JsonArray

            Dim voci As New JsonArray()

            For Each e As EsperienzaFormale In profilo.EsperienzeFormali
                voci.Add(New JsonObject From {
                    {"ruolo", RuoloConIlTipo(e)},
                    {"azienda", Pulito(e.Azienda)},
                    {"durata", Pulito(e.Durata)},
                    {"descrizione", Pulito(e.CosaFacevo)}})
            Next

            Return voci

        End Function

        ''' <summary>
        ''' Il ruolo, con davanti «Tirocinio» o «Stage» se il profilo lo dichiara.
        ''' </summary>
        ''' <remarks>
        ''' Non è un abbellimento ma un fatto, e ometterlo sarebbe la sola invenzione
        ''' possibile in questa composizione: un tirocinio presentato come ruolo nudo si
        ''' legge come un impiego. Il prompt chiede al modello esattamente la stessa cosa.
        ''' Se il tipo è già scritto nel ruolo non si ripete.
        ''' </remarks>
        Private Function RuoloConIlTipo(esperienza As EsperienzaFormale) As String

            Dim ruolo As String = Pulito(esperienza.Ruolo)
            Dim tipo As String = Pulito(esperienza.Tipo)

            If tipo.Length = 0 Then Return ruolo
            If ruolo.Length = 0 Then Return Maiuscola(tipo)
            If ruolo.StartsWith(tipo, StringComparison.CurrentCultureIgnoreCase) Then Return ruolo

            Return Maiuscola(tipo) & " — " & ruolo

        End Function

        ''' <summary>
        ''' Le esperienze informali: quel che si faceva e quando, senza promuoverle a
        ''' impieghi.
        ''' </summary>
        ''' <remarks>
        ''' Niente ruolo e niente azienda, come impone il prompt: sono le esperienze che
        ''' un CV normale perderebbe, e presentarle come lavori sarebbe il primo passo
        ''' verso un curriculum che dice più di quel che è successo. Il «con chi» si
        ''' accosta fra parentesi invece di essere cucito in una frase: accostare due
        ''' fatti è ricopiare, cucirli sarebbe scrivere.
        ''' </remarks>
        Private Function AltreEsperienze(profilo As Profilo) As JsonArray

            Dim voci As New JsonArray()

            For Each e As EsperienzaInformale In profilo.EsperienzeInformali

                Dim descrizione As String = Pulito(e.CosaFacevo)
                Dim conChi As String = Pulito(e.ConChi)
                If conChi.Length > 0 Then
                    descrizione = If(descrizione.Length > 0, descrizione & " (" & conChi & ")", conChi)
                End If

                voci.Add(New JsonObject From {
                    {"descrizione", descrizione},
                    {"quando", Pulito(e.Quando)}})

            Next

            Return voci

        End Function

        ''' <summary>I titoli di studio, ricopiati campo per campo.</summary>
        Private Function Formazione(profilo As Profilo) As JsonArray

            Dim voci As New JsonArray()

            For Each v As VoceFormazione In profilo.Formazione
                voci.Add(New JsonObject From {
                    {"titolo", Pulito(v.Titolo)},
                    {"istituto", Pulito(v.Istituto)},
                    {"anno", Pulito(v.Anno)}})
            Next

            Return voci

        End Function

        ''' <summary>Una lista di stringhe come array JSON, saltando le vuote.</summary>
        Private Function Elenco(voci As List(Of String)) As JsonArray

            Dim uscita As New JsonArray()
            If voci Is Nothing Then Return uscita

            For Each voce As String In voci
                Dim pulita As String = Pulito(voce)
                If pulita.Length > 0 Then uscita.Add(pulita)
            Next

            Return uscita

        End Function

        ''' <summary>
        ''' Il testo senza spazi ai bordi, e mai <c>Nothing</c>.
        ''' </summary>
        ''' <remarks>
        ''' È la «normalizzazione leggera» che il prompt concede sui campi-fatto: si
        ''' ripulisce la forma, non il contenuto. Nient'altro viene toccato — né le
        ''' maiuscole, né la punteggiatura, né le abbreviazioni.
        ''' </remarks>
        Private Function Pulito(testo As String) As String
            Return If(testo, "").Trim()
        End Function

        ''' <summary>La prima lettera maiuscola, per il tipo che apre il ruolo.</summary>
        Private Function Maiuscola(testo As String) As String

            If testo.Length = 0 Then Return testo
            Return Char.ToUpper(testo(0), Globalization.CultureInfo.CurrentCulture) & testo.Substring(1)

        End Function

    End Module

End Namespace
