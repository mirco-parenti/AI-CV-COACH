Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace Motore

    ''' <summary>
    ''' L'unico posto che sa <b>quali</b> campi di un documento sono prosa, e di che genere
    ''' (cap. 08.2): estrae quelli, li manda a rifinire, e rimette i testi al loro posto.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché l'estrazione sta qui e non nel prompt.</b> Al modello arrivano solo
    ''' i campi-prosa: nomi, aziende, date, competenze e titoli non entrano nella richiesta,
    ''' quindi non c'è modo che ne esca uno cambiato. La regola «i campi-fatto non si
    ''' toccano» smette di essere una promessa scritta in un prompt e diventa una proprietà
    ''' di come è fatta la chiamata.</para>
    ''' <para><b>Che cosa torna.</b> Non il documento — quello viene modificato sul posto —
    ''' ma i testi <b>di prima</b> dei soli campi davvero cambiati: è quello che P6 mostra
    ''' nel prima/dopo (cap. 08.4) e che permette di tornare indietro. Se non è cambiato
    ''' niente non torna un oggetto vuoto ma <c>Nothing</c>, così nei file non compare un
    ''' campo che non racconta nulla.</para>
    ''' <para><b>Il CV costa due chiamate</b>, e non è una svista: il sommario e le
    ''' descrizioni delle esperienze sono due forme diverse — frasi in prima persona le
    ''' une, frasi nominali le altre — e hanno due prompt distinti (v.
    ''' <see cref="GenereProsa"/>).</para>
    ''' </remarks>
    Public Class Rifinitura

        ''' <summary>Gli identificativi con cui un campo va e torna dalla rifinitura.</summary>
        ''' <remarks>
        ''' Non li legge il modello per capirci qualcosa: gli si chiede solo di ricopiarli.
        ''' Servono a noi per rimettere ogni testo dov'era, senza fidarci dell'ordine.
        ''' </remarks>
        Public Const IdSommario As String = "sommario"
        Public Const IdCorpo As String = "corpo"
        Private Const PrefissoEsperienza As String = "esperienza."
        Private Const PrefissoAltra As String = "altra."

        Private ReadOnly _rifinitore As IRifinitore

        ''' <param name="rifinitore">Il mestiere che parla con l'AI.</param>
        Public Sub New(rifinitore As IRifinitore)

            If rifinitore Is Nothing Then Throw New ArgumentNullException(NameOf(rifinitore))
            _rifinitore = rifinitore

        End Sub

        ''' <summary>
        ''' Rifinisce un CV — il 📄 base o il 🎯 mirato, che hanno la stessa forma: prima il
        ''' sommario, poi le descrizioni delle esperienze.
        ''' </summary>
        ''' <returns>I testi di prima dei campi cambiati, o <c>Nothing</c> se nessuno lo è.</returns>
        Public Async Function DelCvAsync(cv As JsonNode,
                                         Optional lingua As String = "it",
                                         Optional annulla As CancellationToken = Nothing) As Task(Of JsonObject)

            Dim documento As JsonObject = TryCast(cv, JsonObject)
            If documento Is Nothing Then Return Nothing

            Dim prima As New JsonObject()

            Await ApplicaAsync(Sommario(documento), GenereProsa.Sintesi, prima, lingua, annulla).
                ConfigureAwait(False)

            Await ApplicaAsync(Descrizioni(documento), GenereProsa.Frasi, prima, lingua, annulla).
                ConfigureAwait(False)

            Return SeQualcosaECambiato(prima)

        End Function

        ''' <summary>
        ''' Rifinisce la lettera: solo il corpo.
        ''' </summary>
        ''' <remarks>
        ''' Apertura, chiusura e firma restano fuori di proposito (cap. 08.2). Non sono
        ''' slop: sono le formule che un lettore si aspetta — «Spettabile Azienda,»,
        ''' «Cordiali saluti,» — e in inglese sono perfino una coppia obbligata. Umanizzarle
        ''' vorrebbe dire romperle.
        ''' </remarks>
        ''' <returns>I testi di prima dei campi cambiati, o <c>Nothing</c> se nessuno lo è.</returns>
        Public Async Function DellaLetteraAsync(lettera As JsonNode,
                                                Optional lingua As String = "it",
                                                Optional annulla As CancellationToken = Nothing) As Task(Of JsonObject)

            Dim documento As JsonObject = TryCast(lettera, JsonObject)
            If documento Is Nothing Then Return Nothing

            Dim prima As New JsonObject()

            Await ApplicaAsync(Corpo(documento), GenereProsa.Prosa, prima, lingua, annulla).
                ConfigureAwait(False)

            Return SeQualcosaECambiato(prima)

        End Function

        ''' <summary>
        ''' Rifinisce un testo sciolto: è il corpo dell'email, che non è un documento JSON
        ''' ma una stringa dentro la bozza (cap. 07.1).
        ''' </summary>
        ''' <returns>Il testo rifinito, o quello di partenza se non c'era niente da fare.</returns>
        Public Async Function DelTestoAsync(testo As String,
                                            Optional lingua As String = "it",
                                            Optional annulla As CancellationToken = Nothing) As Task(Of String)

            If String.IsNullOrWhiteSpace(testo) Then Return testo

            Dim esito As IReadOnlyDictionary(Of String, String) = Await _rifinitore.RifinisciAsync(
                {New PezzoDiProsa With {.Id = IdCorpo, .Testo = testo}},
                GenereProsa.Prosa, annulla, lingua).ConfigureAwait(False)

            Dim rifinito As String = Nothing
            If Not esito.TryGetValue(IdCorpo, rifinito) OrElse String.IsNullOrWhiteSpace(rifinito) Then
                Return testo
            End If

            Return rifinito

        End Function

        ''' <summary>Un campo che la rifinitura ha cambiato, come si mostra in P6.</summary>
        Public Class CampoRifinito

            ''' <summary>Il nome del campo in italiano: «Sommario», «Esperienza 2».</summary>
            Public Property Etichetta As String

            ''' <summary>Il testo com'era.</summary>
            Public Property Prima As String

            ''' <summary>Il testo com'è adesso.</summary>
            Public Property Dopo As String

        End Class

        ''' <summary>
        ''' Mette in fila i campi cambiati di un documento: è il prima/dopo che P6 mostra
        ''' quando la casella è spuntata (cap. 08.4).
        ''' </summary>
        ''' <param name="documento">Il documento com'è adesso, cioè il «dopo».</param>
        ''' <param name="prima">I testi conservati alla rifinitura.</param>
        ''' <remarks>
        ''' Si parte dai campi del documento e non dalle chiavi del «prima»: così l'ordine
        ''' è quello in cui l'utente li legge nel CV, e un id che nel documento non esiste
        ''' più — una voce cancellata dopo la generazione — sparisce da sé invece di
        ''' comparire come un confronto con il nulla.
        ''' </remarks>
        Public Shared Function Confronta(documento As JsonNode, prima As JsonNode) As List(Of CampoRifinito)

            Dim cambiati As New List(Of CampoRifinito)

            Dim comEra As JsonObject = TryCast(prima, JsonObject)
            Dim comE As JsonObject = TryCast(documento, JsonObject)
            If comEra Is Nothing OrElse comE Is Nothing Then Return cambiati

            Dim campi As New List(Of Campo)
            campi.AddRange(Sommario(comE))
            campi.AddRange(Descrizioni(comE))
            campi.AddRange(Corpo(comE))

            For Each campo As Campo In campi

                Dim testo As String = CampiJson.Testo(comEra, campo.Id)
                If String.IsNullOrWhiteSpace(testo) Then Continue For

                cambiati.Add(New CampoRifinito With {
                    .Etichetta = ComeSiLegge(campo.Id),
                    .Prima = testo,
                    .Dopo = campo.Testo})

            Next

            Return cambiati

        End Function

        ''' <summary>Il nome di un campo come si legge a video (cap. 03.2: l'interfaccia è in italiano).</summary>
        Public Shared Function ComeSiLegge(id As String) As String

            If String.IsNullOrEmpty(id) Then Return ""

            If id = IdSommario Then Return "Sommario"
            If id = IdCorpo Then Return "Corpo della lettera"

            If id.StartsWith(PrefissoEsperienza, StringComparison.Ordinal) Then
                Return "Esperienza " & Numero(id, PrefissoEsperienza)
            End If

            If id.StartsWith(PrefissoAltra, StringComparison.Ordinal) Then
                Return "Altra esperienza " & Numero(id, PrefissoAltra)
            End If

            Return id

        End Function

        ''' <summary>La posizione dentro la lista, contata da 1 come la conta una persona.</summary>
        Private Shared Function Numero(id As String, prefisso As String) As String

            Dim indice As Integer
            If Not Integer.TryParse(id.Substring(prefisso.Length),
                                    Globalization.NumberStyles.Integer,
                                    Globalization.CultureInfo.InvariantCulture, indice) Then
                Return id.Substring(prefisso.Length)
            End If

            Return (indice + 1).ToString(Globalization.CultureInfo.InvariantCulture)

        End Function

        ''' <summary>Un campo-prosa: dove sta, come si chiama e con quale id viaggia.</summary>
        Private Class Campo

            Public Property Id As String
            Public Property Dove As JsonObject
            Public Property Nome As String

            Public ReadOnly Property Testo As String
                Get
                    Return CampiJson.Testo(Dove, Nome)
                End Get
            End Property

        End Class

        ''' <summary>
        ''' Manda a rifinire un gruppo di campi dello stesso genere e riscrive quelli
        ''' tornati diversi, annotando com'erano.
        ''' </summary>
        Private Async Function ApplicaAsync(campi As List(Of Campo), genere As GenereProsa,
                                            prima As JsonObject, lingua As String,
                                            annulla As CancellationToken) As Task

            If campi.Count = 0 Then Return

            Dim esito As IReadOnlyDictionary(Of String, String) = Await _rifinitore.RifinisciAsync(
                campi.Select(Function(c) New PezzoDiProsa With {.Id = c.Id, .Testo = c.Testo}),
                genere, annulla, lingua).ConfigureAwait(False)

            For Each campo As Campo In campi

                Dim rifinito As String = Nothing
                If Not esito.TryGetValue(campo.Id, rifinito) Then Continue For

                ' Un testo tornato identico non è un cambiamento: nel prima/dopo comparirebbe
                ' come una riga in cui non è successo niente, e il prompt il permesso di non
                ' cambiare ce l'ha scritto dentro.
                Dim comEra As String = campo.Testo
                If String.Equals(rifinito, comEra, StringComparison.Ordinal) Then Continue For

                prima(campo.Id) = comEra
                campo.Dove(campo.Nome) = rifinito

            Next

        End Function

        ''' <summary>Il sommario del CV, se c'è.</summary>
        Private Shared Function Sommario(documento As JsonObject) As List(Of Campo)

            Return UnCampo(documento, IdSommario, IdSommario)

        End Function

        ''' <summary>Il corpo della lettera, se c'è.</summary>
        Private Shared Function Corpo(documento As JsonObject) As List(Of Campo)

            Return UnCampo(documento, IdCorpo, IdCorpo)

        End Function

        ''' <summary>
        ''' Le descrizioni delle esperienze, formali e informali, in un gruppo solo: sono
        ''' la stessa forma, e chiederle in una volta è una chiamata invece di due.
        ''' </summary>
        Private Shared Function Descrizioni(documento As JsonObject) As List(Of Campo)

            Dim campi As New List(Of Campo)

            campi.AddRange(DentroLaLista(documento, "esperienze_professionali", PrefissoEsperienza))
            campi.AddRange(DentroLaLista(documento, "altre_esperienze", PrefissoAltra))

            Return campi

        End Function

        ''' <summary>Il campo <c>descrizione</c> di ogni voce di una lista, se valorizzato.</summary>
        ''' <remarks>
        ''' L'indice fa parte dell'id perché è l'unica cosa che distingue due voci: due
        ''' esperienze possono avere la stessa descrizione parola per parola, e una mappa
        ''' che le confondesse rimetterebbe il testo rifinito nella voce sbagliata.
        ''' </remarks>
        Private Shared Function DentroLaLista(documento As JsonObject, lista As String,
                                              prefisso As String) As List(Of Campo)

            Dim trovati As New List(Of Campo)

            Dim voci As JsonArray = TryCast(CampiJson.Nodo(documento, lista), JsonArray)
            If voci Is Nothing Then Return trovati

            For indice As Integer = 0 To voci.Count - 1

                Dim voce As JsonObject = TryCast(voci(indice), JsonObject)
                If voce Is Nothing Then Continue For

                trovati.AddRange(UnCampo(voce, "descrizione", prefisso & indice.ToString(
                    Globalization.CultureInfo.InvariantCulture)))

            Next

            Return trovati

        End Function

        ''' <summary>
        ''' Un campo solo, se esiste e ha del testo dentro; una lista vuota altrimenti.
        ''' </summary>
        ''' <remarks>
        ''' Restituisce una lista invece di un <see cref="Campo"/> o <c>Nothing</c> perché
        ''' chi chiama la concatena alle altre: un campo assente diventa così «niente da
        ''' aggiungere» e non un caso da controllare a ogni chiamata.
        ''' </remarks>
        Private Shared Function UnCampo(dove As JsonObject, nome As String, id As String) As List(Of Campo)

            Dim trovati As New List(Of Campo)

            If String.IsNullOrWhiteSpace(CampiJson.Testo(dove, nome)) Then Return trovati

            trovati.Add(New Campo With {.Id = id, .Dove = dove, .Nome = nome})

            Return trovati

        End Function

        ''' <summary>Il «prima» solo se ha qualcosa da raccontare.</summary>
        Private Shared Function SeQualcosaECambiato(prima As JsonObject) As JsonObject

            Return If(prima.Count = 0, Nothing, prima)

        End Function

    End Class

End Namespace
