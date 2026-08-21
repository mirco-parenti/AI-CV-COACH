Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Dati

    ''' <summary>Che cosa mettere dentro un backup (cap. 11.4).</summary>
    Public Enum ContenutoBackup

        ''' <summary>Il profilo con il suo storico e il 📄 CV-1 base: chi sono, senza le candidature.</summary>
        SoloProfilo

        ''' <summary>Tutto: il profilo, il registro e le candidature con i loro artefatti.</summary>
        Tutto

    End Enum

    ''' <summary>Una versione dello storico dentro un backup: il suo nome e il profilo di quel giorno.</summary>
    Public Class VersioneInBackup

        Public Property Versione As String
        Public Property Profilo As JsonNode

    End Class

    ''' <summary>
    ''' Una candidatura dentro un backup: il nome della sua cartella e i suoi artefatti
    ''' JSON, presi <b>così come sono sul disco</b>.
    ''' </summary>
    ''' <remarks>
    ''' I file si copiano grezzi e non si ricostruiscono da <see cref="Opportunita"/>:
    ''' passare per le classi vorrebbe dire che un campo che il programma di oggi non
    ''' modella — scritto a mano, o da una versione futura — sparirebbe nel viaggio. Un
    ''' backup che perde qualcosa non è un backup.
    ''' </remarks>
    Public Class OpportunitaInBackup

        ''' <summary>Il nome della cartella, non il suo percorso: un backup si ripristina anche altrove.</summary>
        Public Property Cartella As String

        ''' <summary>Gli artefatti, per nome di file (<c>annuncio.json</c>, <c>stato.json</c>…).</summary>
        Public ReadOnly Property File As New Dictionary(Of String, JsonNode)(StringComparer.OrdinalIgnoreCase)

    End Class

    ''' <summary>
    ''' Il contenuto di un file di backup (cap. 11.4): un solo <c>.json</c> leggibile, con
    ''' l'intestazione che dice di che formato è e cosa c'è dentro.
    ''' </summary>
    ''' <remarks>
    ''' <para>Il campo <c>formato_backup</c> esiste perché i programmi di domani sappiano
    ''' leggere i backup di oggi: cresce solo quando lo schema cambia davvero, e un file
    ''' che dichiara un formato <b>più nuovo</b> di quello conosciuto si rifiuta invece di
    ''' essere letto a metà.</para>
    ''' <para>Cosa <b>non</b> c'è dentro, di proposito: la chiave API (cap. 11.3 — un
    ''' backup gira via email o chiavetta e non deve portarsi dietro una credenziale), i
    ''' documenti impaginati delle cartelle <c>out\</c> (sono file normali e si copiano da
    ''' sé), i dati di navigazione, il diario dei consumi e i file dei numeri
    ''' (<c>taratura.json</c>, <c>modelli.json</c>), che sono taratura del prodotto e non
    ''' roba dell'utente.</para>
    ''' </remarks>
    Public Class Backup

        ''' <summary>Il formato che questo programma scrive.</summary>
        Public Const FormatoCorrente As Integer = 1

        ''' <summary>Il nome con cui il file dichiara chi l'ha scritto.</summary>
        Public Const NomeApp As String = "TrovaLavoro"

        Public Property Formato As Integer = FormatoCorrente
        Public Property App As String = NomeApp

        ''' <summary>Quando è stato esportato.</summary>
        Public Property Data As Date

        ''' <summary>Il profilo corrente, com'era sul disco.</summary>
        Public Property Profilo As JsonNode

        ''' <summary>
        ''' Quando quel profilo era stato salvato l'ultima volta. Serve all'anteprima del
        ''' ripristino, che deve poter dire «il backup ha un profilo del 3 agosto, quello
        ''' attuale è del 5» (cap. 11.4): senza questo campo la data si può solo dedurre
        ''' dall'ultima versione dello storico, che è la stessa cosa <b>quasi</b> sempre —
        ''' e quel «quasi» l'ha mostrato la prova dal vivo, con un profilo del 21 agosto
        ''' annunciato come «del 17».
        ''' </summary>
        Public Property ProfiloSalvato As Date?

        ''' <summary>Le versioni conservate, dalla più vecchia alla più recente.</summary>
        Public ReadOnly Property Storico As New List(Of VersioneInBackup)

        ''' <summary>Il 📄 CV-1 base, che appartiene al profilo (cap. 11.1).</summary>
        Public Property CvBase As JsonNode

        ''' <summary>L'indice delle candidature, se il backup è completo.</summary>
        Public Property Registro As JsonNode

        ''' <summary>Le candidature, se il backup è completo.</summary>
        Public ReadOnly Property Opportunita As New List(Of OpportunitaInBackup)

        ''' <summary>
        ''' Che cosa c'è davvero dentro, nell'ordine in cui si legge. È l'elenco che
        ''' finisce nel campo <c>contenuto</c>: non si dichiara, si <b>constata</b> — così
        ''' l'intestazione non può mentire sul corpo.
        ''' </summary>
        Public Function Contenuto() As IReadOnlyList(Of String)

            Dim dentro As New List(Of String)

            If Profilo IsNot Nothing Then dentro.Add("profilo")
            If Storico.Count > 0 Then dentro.Add("storico")
            If CvBase IsNot Nothing Then dentro.Add("cv_base")
            If Registro IsNot Nothing Then dentro.Add("registro")
            If Opportunita.Count > 0 Then dentro.Add("opportunita")

            Return dentro

        End Function

        ''' <summary>Il file di backup, pronto da scrivere.</summary>
        Public Function ComeJson() As JsonObject

            Dim scritto As New JsonObject From {
                {"formato_backup", Formato},
                {"app", App},
                {"data", CampiJson.Quando(Data)},
                {"contenuto", New JsonArray(Contenuto().Select(Function(v) JsonValue.Create(v)).ToArray())}}

            If ProfiloSalvato.HasValue Then scritto("profilo_salvato") = CampiJson.Quando(ProfiloSalvato.Value)
            If Profilo IsNot Nothing Then scritto("profilo") = Profilo.DeepClone()

            If Storico.Count > 0 Then

                Dim versioni As New JsonArray()

                For Each v As VersioneInBackup In Storico
                    versioni.Add(New JsonObject From {
                        {"versione", v.Versione},
                        {"profilo", v.Profilo?.DeepClone()}})
                Next

                scritto("storico") = versioni

            End If

            If CvBase IsNot Nothing Then scritto("cv_base") = CvBase.DeepClone()
            If Registro IsNot Nothing Then scritto("registro") = Registro.DeepClone()

            If Opportunita.Count > 0 Then

                Dim candidature As New JsonArray()

                For Each o As OpportunitaInBackup In Opportunita

                    Dim file As New JsonObject()
                    For Each coppia As KeyValuePair(Of String, JsonNode) In o.File
                        file(coppia.Key) = coppia.Value?.DeepClone()
                    Next

                    candidature.Add(New JsonObject From {
                        {"cartella", o.Cartella},
                        {"file", file}})

                Next

                scritto("opportunita") = candidature

            End If

            Return scritto

        End Function

        ''' <summary>
        ''' Rilegge un file di backup. Se non è JSON solleva <see cref="JsonException"/>;
        ''' se è un JSON che non è un backup di questo programma solleva
        ''' <see cref="InvalidDataException"/> con scritto perché.
        ''' </summary>
        ''' <remarks>
        ''' L'intestazione si controlla <b>prima</b> del corpo, ed è il punto in cui un
        ''' errore costa poco: un file scelto per sbaglio — il CV, un annuncio, il
        ''' riepilogo delle candidature — si ferma qui, con il suo nome ancora in mano
        ''' all'utente, invece di diventare un ripristino a metà.
        ''' </remarks>
        Public Shared Function DaTesto(testo As String) As Backup

            Dim radice As JsonObject = TryCast(JsonNode.Parse(testo), JsonObject)

            If radice Is Nothing Then
                Throw New InvalidDataException(
                    "Questo file non è un backup di TrovaLavoro: dentro non c'è un oggetto JSON.")
            End If

            Dim formato As Integer? = CampiJson.Intero(radice, "formato_backup")

            If formato Is Nothing Then
                Throw New InvalidDataException(
                    "Questo file non è un backup di TrovaLavoro: manca il campo «formato_backup».")
            End If

            If formato.Value > FormatoCorrente Then
                Throw New InvalidDataException(
                    $"Questo backup è di formato {formato.Value} e io conosco fino al {FormatoCorrente}: " &
                    "l'ha scritto una versione più recente dell'applicazione. Aggiornala e riprova.")
            End If

            Dim letto As New Backup With {
                .Formato = formato.Value,
                .App = If(CampiJson.Testo(radice, "app"), NomeApp),
                .Data = CampiJson.Istante(radice, "data"),
                .ProfiloSalvato = QuandoSeCE(radice, "profilo_salvato"),
                .Profilo = CampiJson.Nodo(radice, "profilo"),
                .CvBase = CampiJson.Nodo(radice, "cv_base"),
                .Registro = CampiJson.Nodo(radice, "registro")}

            For Each scritta As JsonObject In Oggetti(radice, "storico")

                Dim versione As String = CampiJson.Testo(scritta, "versione")
                If String.IsNullOrWhiteSpace(versione) Then Continue For

                letto.Storico.Add(New VersioneInBackup With {
                    .Versione = versione,
                    .Profilo = CampiJson.Nodo(scritta, "profilo")})

            Next

            For Each scritta As JsonObject In Oggetti(radice, "opportunita")

                Dim cartella As String = CampiJson.Testo(scritta, "cartella")
                If String.IsNullOrWhiteSpace(cartella) Then Continue For

                Dim candidatura As New OpportunitaInBackup With {.Cartella = cartella}

                Dim file As JsonObject = TryCast(CampiJson.Nodo(scritta, "file"), JsonObject)

                If file IsNot Nothing Then
                    For Each coppia As KeyValuePair(Of String, JsonNode) In file
                        candidatura.File(coppia.Key) = coppia.Value
                    Next
                End If

                letto.Opportunita.Add(candidatura)

            Next

            Return letto

        End Function

        ''' <summary>
        ''' Una data che può non esserci. Un backup scritto prima che questo campo
        ''' esistesse resta leggibile: dice solo una cosa in meno.
        ''' </summary>
        Private Shared Function QuandoSeCE(radice As JsonObject, campo As String) As Date?

            If CampiJson.Nodo(radice, campo) Is Nothing Then Return Nothing

            Dim quando As Date = CampiJson.Istante(radice, campo)
            Return If(quando = Nothing, CType(Nothing, Date?), quando)

        End Function

        ''' <summary>Gli oggetti dentro un campo che dovrebbe essere una lista; niente, se non lo è.</summary>
        Private Shared Iterator Function Oggetti(radice As JsonObject, campo As String) As IEnumerable(Of JsonObject)

            Dim lista As JsonArray = TryCast(CampiJson.Nodo(radice, campo), JsonArray)
            If lista Is Nothing Then Return

            For Each voce As JsonNode In lista
                Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                If oggetto IsNot Nothing Then Yield oggetto
            Next

        End Function

    End Class

End Namespace
