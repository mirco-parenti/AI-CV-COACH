Imports System.Text.Json.Nodes

Namespace Motore

    ''' <summary>
    ''' Una candidatura, tutta insieme: l'annuncio, i giudizi del confronto, il punteggio,
    ''' le mitigazioni, i due documenti generati, e da quale versione di profilo sono
    ''' nati (cap. 02, cap. 11.1). È ciò che <c>Dati/ArchivioOpportunita</c> scrive nella
    ''' sua cartella.
    ''' </summary>
    ''' <remarks>
    ''' <para>Gli artefatti restano <b>JSON grezzo</b> e non diventano classi: a
    ''' differenza del profilo — che P2 edita campo per campo, e per questo è tipizzato —
    ''' questi il programma li riceve dall'AI, li passa al prompt successivo e li scrive
    ''' su disco senza mai entrarci dentro. Tipizzarli vorrebbe dire scrivere e mantenere
    ''' lo stesso schema due volte, in VB e nel prompt, per poi doverli comunque
    ''' riserializzare identici. Le viste tipizzate arrivano dove servono davvero:
    ''' l'impaginazione del CV (T4b) e l'elenco dei giudizi da mostrare (T4c).</para>
    ''' <para>Un'opportunità <b>può essere incompleta</b>, ed è normale: nel flusso reale
    ''' (cap. 12, A5→A7) l'utente guarda il confronto e decide se generare i documenti.
    ''' Fra un passo e l'altro l'opportunità esiste già, con dentro solo ciò che è stato
    ''' fatto finora.</para>
    ''' </remarks>
    Public Class Opportunita

        ''' <summary>L'annuncio strutturato (anello 2).</summary>
        Public Property Annuncio As JsonNode

        ''' <summary>
        ''' L'esito del confronto (anello 3) così come l'AI l'ha dato: la lista
        ''' <c>giudizi</c>, la <c>lettura_insieme</c> e il <c>numero_complessivo</c>.
        ''' </summary>
        Public Property Confronto As JsonNode

        ''' <summary>
        ''' Il punteggio calcolato dai giudizi: è deterministico e si potrebbe
        ''' ricalcolare, ma si conserva perché è il giudizio <b>di quel giorno, con quella
        ''' taratura</b> — quello in base a cui è stata presa la decisione di candidarsi.
        ''' </summary>
        Public Property Match As RisultatoMatch

        ''' <summary>I ponti onesti sui gap. Una lista vuota è un esito legittimo.</summary>
        Public Property Mitigazioni As JsonNode

        ''' <summary>Il 🎯 CV mirato su questo annuncio.</summary>
        Public Property Cv As JsonNode

        ''' <summary>La lettera di presentazione.</summary>
        Public Property Lettera As JsonNode

        ''' <summary>
        ''' La versione di profilo da cui i documenti sono nati (il nome che
        ''' <c>ArchivioProfilo.Salva</c> restituisce): è ciò che tiene spiegabile un CV
        ''' già inviato anche a profilo evoluto (cap. 11.1).
        ''' </summary>
        Public Property VersioneProfilo As String

        ''' <summary>
        ''' La lingua dei documenti. A T4 è sempre l'italiano: il campo <c>lingua</c>
        ''' dell'annuncio e le varianti <c>en</c> dei prompt arrivano con T7 (cap. 10).
        ''' </summary>
        Public Property Lingua As String = "it"

        ''' <summary>
        ''' Da dove viene l'annuncio: il nome del portale se è uno di quelli conosciuti,
        ''' altrimenti il sito (cap. 06.4; cap. 12, A4). Vuoto per un annuncio incollato a
        ''' mano, che una provenienza non ce l'ha.
        ''' </summary>
        Public Property Fonte As String

        ''' <summary>
        ''' L'indirizzo della pagina da cui l'annuncio è stato catturato. È ciò che
        ''' permette di tornare all'originale mesi dopo — quando l'annuncio non c'è più,
        ''' resta almeno la prova di dove stava.
        ''' </summary>
        Public Property Link As String

        ''' <summary>Quando l'opportunità è nata e quando è stata toccata l'ultima volta.</summary>
        Public Property Creata As Date
        Public Property Aggiornata As Date

        ''' <summary>
        ''' Dove è stata scritta su disco; <c>Nothing</c> finché non lo è. Lo riempie
        ''' l'archivio, che è l'unico a sapere come si chiama la cartella.
        ''' </summary>
        Public Property Cartella As String

        ''' <summary>Il nome di chi offre il posto, dall'annuncio; vuoto se anonimo.</summary>
        Public ReadOnly Property Azienda As String
            Get
                Return DallAnnuncio("azienda")
            End Get
        End Property

        ''' <summary>Il ruolo, dall'annuncio.</summary>
        Public ReadOnly Property Titolo As String
            Get
                Return DallAnnuncio("titolo")
            End Get
        End Property

        ''' <summary>
        ''' Se l'annuncio è uscito <b>vuoto</b>: è così che il prompt <c>analisi_annuncio</c>
        ''' dice «questo testo non è un annuncio di lavoro» — una pagina di elenco, una
        ''' home, una schermata di accesso (cap. 06.4).
        ''' </summary>
        ''' <remarks>
        ''' <para>La domanda si fa <b>conservativa</b>: vuoto vuol dire che non c'è né il
        ''' ruolo, né una mansione, né uno solo dei quattro gruppi di requisiti. Basta una
        ''' di quelle cose perché l'annuncio valga la pena — un annuncio scarno esiste, e
        ''' rifiutarlo sarebbe peggio che analizzarlo. Il rischio si sceglie da questa
        ''' parte: meglio un'analisi in più che una cattura buona buttata via.</para>
        ''' <para>Non guarda azienda, sede, contratto e benefit: sono contorno, e una
        ''' pagina di elenco quelli può averli (il nome del portale, la città della
        ''' ricerca) proprio mentre di annuncio non ne ha nessuno.</para>
        ''' </remarks>
        Public ReadOnly Property AnnuncioVuoto As Boolean
            Get
                If Annuncio Is Nothing Then Return True

                If Not String.IsNullOrWhiteSpace(Titolo) Then Return False

                For Each lista As String In {"competenze_richieste", "esperienza_richiesta",
                                             "formazione_richiesta", "altri_requisiti", "mansioni"}
                    If Not ListaVuota(lista) Then Return False
                Next

                Return True
            End Get
        End Property

        ''' <summary>
        ''' Se il confronto è stato fatto: è la domanda che decide se si può passare alla
        ''' generazione (cap. 12, A6).
        ''' </summary>
        Public ReadOnly Property Confrontata As Boolean
            Get
                Return Giudizi() IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' La sola lista dei giudizi, che è ciò che i prompt successivi ricevono;
        ''' <c>Nothing</c> se il confronto non c'è o non ha la forma attesa.
        ''' </summary>
        Public Function Giudizi() As JsonArray

            If Confronto Is Nothing Then Return Nothing
            Dim oggetto As JsonObject = TryCast(Confronto, JsonObject)
            If oggetto Is Nothing Then Return Nothing

            Dim voci As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue("giudizi", voci) Then Return Nothing

            Return TryCast(voci, JsonArray)

        End Function

        ''' <summary>
        ''' Se una lista dell'annuncio è assente o senza voci. Un campo che non è una lista
        ''' conta come vuoto: l'AI ha risposto fuori schema, e su una forma che non
        ''' riconosciamo non si può dire che ci sia qualcosa.
        ''' </summary>
        Private Function ListaVuota(campo As String) As Boolean

            Dim oggetto As JsonObject = TryCast(Annuncio, JsonObject)
            If oggetto Is Nothing Then Return True

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, valore) Then Return True

            Dim voci As JsonArray = TryCast(valore, JsonArray)
            Return voci Is Nothing OrElse voci.Count = 0

        End Function

        ''' <summary>Un campo di testo dell'annuncio, vuoto se manca o non è un testo.</summary>
        Private Function DallAnnuncio(campo As String) As String

            Dim oggetto As JsonObject = TryCast(Annuncio, JsonObject)
            If oggetto Is Nothing Then Return String.Empty

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, valore) OrElse valore Is Nothing Then
                Return String.Empty
            End If

            Return If(valore.GetValueKind() = Text.Json.JsonValueKind.String,
                      valore.GetValue(Of String)(), String.Empty)

        End Function

    End Class

End Namespace
