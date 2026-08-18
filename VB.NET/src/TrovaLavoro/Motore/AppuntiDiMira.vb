Imports System.Linq
Imports System.Text.Json.Nodes

Namespace Motore

    ''' <summary>
    ''' Un appunto di mira: un'istruzione breve per chi scriverà 🎯 CV mirato e ✉️ lettera.
    ''' </summary>
    ''' <remarks>
    ''' Un appunto <b>orienta</b>, non aggiunge: dice cosa mettere davanti, quale gap
    ''' nominare, con che tono. I fatti restano quelli del profilo (cap. 02, vista-dati).
    ''' </remarks>
    Public Class AppuntoDiMira

        ''' <summary>Che genere di indicazione è: v. <see cref="TipiDiAppunto"/>.</summary>
        Public Property Tipo As String

        ''' <summary>L'istruzione vera e propria, in una frase.</summary>
        Public Property Testo As String

        ''' <summary>A cosa si appoggia: la frase dell'utente o l'elemento del profilo.</summary>
        Public Property Da As String

    End Class

    ''' <summary>I quattro generi di appunto che il prompt può produrre (cap. 04.3).</summary>
    Public NotInheritable Class TipiDiAppunto

        Private Sub New()
        End Sub

        Public Const Enfasi As String = "enfasi"
        Public Const Mitigazione As String = "mitigazione"
        Public Const Tono As String = "tono"
        Public Const Evitare As String = "evitare"

        ''' <summary>Come si chiama un genere di appunto per chi legge la scheda.</summary>
        Public Shared Function Etichetta(tipo As String) As String

            Select Case tipo
                Case Enfasi : Return "Metti in risalto"
                Case Mitigazione : Return "Il gap da nominare"
                Case Tono : Return "Il tono"
                Case Evitare : Return "Da evitare"
            End Select

            ' Un genere che non conosciamo non si butta via e non si travisa: l'appunto
            ' resta leggibile con il nome che il modello gli ha dato. Perdere una riga
            ' che l'utente ha appena confermato sarebbe peggio di mostrargliela storta.
            Return If(tipo, "Appunto")

        End Function

    End Class

    ''' <summary>
    ''' L'esito confermato del brainstorming: pochi appunti operativi e, a parte, i fatti
    ''' che l'utente ha dichiarato parlando e nel profilo non risultano.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché i fatti nuovi stanno fuori.</b> Sono le due bussole del prodotto
    ''' messe insieme. Anti-invenzione: quello che l'utente racconta in chat non può
    ''' entrare nei documenti passando dagli appunti, perché scavalcherebbe il profilo,
    ''' che è l'unica fonte di fatti. Anti-perdita: e nemmeno può sparire in silenzio.
    ''' Resta lì, dichiarato, perché l'utente lo porti nel profilo se è vero — lo stesso
    ''' mestiere del campo <c>altrove</c> nei turni del dialogo.</para>
    ''' <para>La conversazione da cui nascono, invece, <b>non si conserva</b>
    ''' (cap. 15.4): quello che resta è questo.</para>
    ''' </remarks>
    Public Class AppuntiDiMira

        ''' <summary>Gli appunti confermabili, nell'ordine in cui li ha proposti l'AI.</summary>
        Public ReadOnly Property Appunti As New List(Of AppuntoDiMira)

        ''' <summary>Quello che è stato detto e nel profilo non c'è.</summary>
        Public ReadOnly Property FattiNuovi As New List(Of String)

        ''' <summary>Se non c'è niente da confermare né da segnalare.</summary>
        Public ReadOnly Property Vuoti As Boolean
            Get
                Return Appunti.Count = 0 AndAlso FattiNuovi.Count = 0
            End Get
        End Property

        ''' <summary>
        ''' Legge quello che ha risposto il prompt. Quel che non ha la forma attesa si
        ''' scarta in silenzio: è una scheda da confermare, non un file di configurazione —
        ''' fermare tutto per una riga storta toglierebbe all'utente anche le altre.
        ''' </summary>
        Public Shared Function DaJson(nodo As JsonNode) As AppuntiDiMira

            Dim esito As New AppuntiDiMira()

            Dim radice As JsonObject = TryCast(nodo, JsonObject)
            If radice Is Nothing Then Return esito

            Dim voci As JsonArray = TryCast(radice("appunti"), JsonArray)
            If voci IsNot Nothing Then
                For Each voce As JsonNode In voci

                    Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                    If oggetto Is Nothing Then Continue For

                    Dim testo As String = Stringa(oggetto, "testo")
                    If String.IsNullOrWhiteSpace(testo) Then Continue For

                    esito.Appunti.Add(New AppuntoDiMira With {
                        .Tipo = Stringa(oggetto, "tipo"),
                        .Testo = testo,
                        .Da = Stringa(oggetto, "da")})

                Next
            End If

            Dim fatti As JsonArray = TryCast(radice("fatti_nuovi"), JsonArray)
            If fatti IsNot Nothing Then
                For Each fatto As JsonNode In fatti
                    Dim testo As String = TryCast(fatto, JsonValue)?.ToString()
                    If Not String.IsNullOrWhiteSpace(testo) Then esito.FattiNuovi.Add(testo.Trim())
                Next
            End If

            Return esito

        End Function

        ''' <summary>Come si salvano nella cartella dell'opportunità (cap. 11.1).</summary>
        Public Function VersoJson() As JsonNode

            Dim voci As New JsonArray()
            For Each appunto As AppuntoDiMira In Appunti
                voci.Add(New JsonObject From {
                    {"tipo", appunto.Tipo},
                    {"testo", appunto.Testo},
                    {"da", appunto.Da}})
            Next

            Dim fatti As New JsonArray()
            For Each fatto As String In FattiNuovi
                fatti.Add(JsonValue.Create(fatto))
            Next

            Return New JsonObject From {
                {"appunti", voci},
                {"fatti_nuovi", fatti}}

        End Function

        ''' <summary>
        ''' Solo gli appunti, come arrivano ai prompt che scrivono i documenti.
        ''' </summary>
        ''' <remarks>
        ''' <para>Due cose restano fuori, ed è tutto il punto di questo metodo. I
        ''' <b>fatti nuovi</b>, perché entrare nei documenti da questa porta scavalcherebbe
        ''' il profilo. E il campo <c>da</c>, che dice a quale frase della chat l'appunto si
        ''' appoggia: serve all'utente per riconoscerlo nella scheda, e a chi scrive il CV
        ''' non direbbe niente — anzi, gli metterebbe davanti pezzi di conversazione da cui
        ''' potrebbe pescare.</para>
        ''' <para>È una <b>lista</b> e non un oggetto, come le mitigazioni: i prompt di
        ''' generazione ricevono elenchi.</para>
        ''' </remarks>
        Public Function SoloAppunti() As JsonNode

            Dim voci As New JsonArray()
            For Each appunto As AppuntoDiMira In Appunti
                voci.Add(New JsonObject From {
                    {"tipo", appunto.Tipo},
                    {"testo", appunto.Testo}})
            Next

            Return voci

        End Function

        ''' <summary>
        ''' Gli appunti salvati con una candidatura, pronti per il prompt. Una lista vuota
        ''' quando non ce ne sono: <b>l'assenza di appunti non è un errore</b>, è il caso
        ''' normale di chi genera senza aver ragionato.
        ''' </summary>
        Public Shared Function PerIlPrompt(salvati As JsonNode) As JsonNode

            If salvati Is Nothing Then Return New JsonArray()

            Return DaJson(salvati).SoloAppunti()

        End Function

        ''' <summary>Toglie gli appunti che l'utente ha spuntato via nella scheda.</summary>
        Public Function Solo(tenere As IEnumerable(Of AppuntoDiMira)) As AppuntiDiMira

            Dim scelti As New AppuntiDiMira()
            scelti.Appunti.AddRange(Appunti.Where(Function(a) tenere.Contains(a)))
            scelti.FattiNuovi.AddRange(FattiNuovi)

            Return scelti

        End Function

        Private Shared Function Stringa(oggetto As JsonObject, campo As String) As String

            Dim valore As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, valore) Then Return String.Empty

            Return If(TryCast(valore, JsonValue)?.ToString(), String.Empty).Trim()

        End Function

    End Class

End Namespace
