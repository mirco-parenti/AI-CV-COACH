Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Motore

Namespace Mcp

    ''' <summary>
    ''' I tool che si limitano a raccontare cosa c'è nella cartella dati (cap. 09.3):
    ''' il profilo, il registro delle candidature, una singola opportunità.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Si rilegge da disco ogni volta.</b> Nessuno di questi metodi tiene una
    ''' copia: il processo del server può vivere ore accanto all'applicazione con le
    ''' finestre, che nel frattempo scrive, e servire quel che si era letto all'avvio
    ''' vorrebbe dire raccontare un passato spacciandolo per presente.</para>
    ''' <para><b>Il JSON si consegna com'è su disco</b>, senza passare dalle classi del
    ''' motore. Non è pigrizia: rileggerlo e riscriverlo significherebbe mostrare al
    ''' client la nostra interpretazione dei file invece dei file, e perdere per strada
    ''' qualunque campo che le classi non conoscono ancora.</para>
    ''' </remarks>
    Public Class ToolDiLettura

        ''' <summary>La sottocartella dove finiscono i documenti prodotti.</summary>
        Private Const CartellaProdotti As String = "out"

        Private ReadOnly _contesto As ContestoApp

        Public Sub New(contesto As ContestoApp)

            If contesto Is Nothing Then Throw New ArgumentNullException(NameOf(contesto))
            _contesto = contesto

        End Sub

        ''' <summary>
        ''' Il profilo corrente, così com'è scritto in <c>profilo.json</c>.
        ''' </summary>
        Public Function LeggiProfilo() As EsitoTool

            Dim percorso As String = _contesto.Cartella.FileProfilo

            If Not File.Exists(percorso) Then
                Return EsitoTool.NonRiuscito(
                    "Non c'è ancora nessun profilo in questa cartella dati. Si costruisce " &
                    "dall'applicazione, importando un CV o rispondendo ai turni del dialogo.")
            End If

            Return LeggiFile(percorso, "Il profilo")

        End Function

        ''' <summary>
        ''' Il registro delle candidature: una riga per opportunità, con stato, stelle e
        ''' date. È lo stesso indice che l'applicazione mostra nella pagina di casa, ed è
        ''' ricostruito dalle cartelle se il file non torna — quindi risponde anche
        ''' quando <c>registro.json</c> non c'è.
        ''' </summary>
        Public Function LeggiRegistro() As EsitoTool

            Try
                Return EsitoTool.Riuscito(_contesto.Registro.Carica().ComeJson())
            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                Return EsitoTool.NonRiuscito($"Non sono riuscito a leggere le candidature: {ex.Message}")
            End Try

        End Function

        ''' <summary>
        ''' Tutto quel che una candidatura ha prodotto: gli artefatti JSON della sua
        ''' cartella, uno per chiave, più i nomi dei documenti già impaginati.
        ''' </summary>
        ''' <param name="nome">
        ''' Il nome della cartella, quello che <c>leggi_registro</c> mette nel campo
        ''' <c>cartella</c> di ogni voce.
        ''' </param>
        ''' <remarks>
        ''' Si raccolgono <b>tutti</b> i <c>.json</c> che ci sono, invece di chiedere per
        ''' nome quelli che il programma conosce: così un artefatto nuovo — e ne sono
        ''' arrivati a ogni tappa — si affaccia da sé, senza che qualcuno debba
        ''' ricordarsi di aggiungerlo anche qui.
        ''' </remarks>
        Public Function LeggiOpportunita(nome As String) As EsitoTool

            Dim chiesto As String = Nothing
            Dim rifiuto As EsitoTool = NomeDiCartellaAmmesso(nome, chiesto)
            If rifiuto IsNot Nothing Then Return rifiuto

            Dim cartella As String = Path.Combine(_contesto.Cartella.CartellaOpportunita, chiesto)

            If Not Directory.Exists(cartella) Then
                Return EsitoTool.NonRiuscito(
                    $"L'opportunità «{chiesto}» non c'è. L'elenco di quelle che ci sono lo dà leggi_registro.")
            End If

            Try

                Dim raccolto As New JsonObject From {{"cartella", chiesto}}

                For Each artefatto As String In Directory.GetFiles(cartella, "*.json").
                                                OrderBy(Function(f) Path.GetFileName(f), StringComparer.Ordinal)

                    Dim testo As String = File.ReadAllText(artefatto, Encoding.UTF8)
                    raccolto(Path.GetFileNameWithoutExtension(artefatto)) = JsonNode.Parse(testo)

                Next

                raccolto("documenti") = Prodotti(cartella)

                Return EsitoTool.Riuscito(raccolto)

            Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                Return EsitoTool.NonRiuscito($"L'opportunità «{chiesto}» non si è lasciata leggere: {ex.Message}")
            End Try

        End Function

        ''' <summary>
        ''' I nomi dei documenti impaginati nella sottocartella <c>out</c>: i soli nomi,
        ''' perché un <c>.docx</c> non si consegna dentro una risposta JSON.
        ''' </summary>
        Private Shared Function Prodotti(cartella As String) As JsonArray

            Dim elenco As New JsonArray()
            Dim out As String = Path.Combine(cartella, CartellaProdotti)

            If Not Directory.Exists(out) Then Return elenco

            For Each documento As String In Directory.GetFiles(out).
                                            OrderBy(Function(f) Path.GetFileName(f), StringComparer.Ordinal)
                elenco.Add(Path.GetFileName(documento))
            Next

            Return elenco

        End Function

        ''' <summary>
        ''' Controlla che quello che è arrivato sia il <b>nome</b> di una
        ''' cartella-opportunità e non una strada per uscire dalla cartella dati
        ''' (cap. 09.5).
        ''' </summary>
        ''' <param name="nome">Quello che ha scritto chi ha chiamato.</param>
        ''' <param name="pulito">Il nome ripulito, se è ammesso.</param>
        ''' <returns>La frase da rispondere, o <c>Nothing</c> se si può proseguire.</returns>
        ''' <remarks>
        ''' È visibile fuori di qui perché la stessa domanda se la fanno i tool che
        ''' <b>scrivono</b> (T8c), e questa regola dev'essere una sola: due copie
        ''' finirebbero per lasciar passare cose diverse, e quella più permissiva sarebbe
        ''' proprio quella che scrive. Qui arriva testo composto da un modello, che può
        ''' sbagliare e che qualcuno potrebbe aver istruito male.
        ''' </remarks>
        Friend Shared Function NomeDiCartellaAmmesso(nome As String, ByRef pulito As String) As EsitoTool

            pulito = Nothing

            If String.IsNullOrWhiteSpace(nome) Then
                Return EsitoTool.NonRiuscito(
                    "Manca il nome della cartella dell'opportunità: si trova nel campo " &
                    "«cartella» di ogni voce di leggi_registro.")
            End If

            Dim chiesto As String = nome.Trim()

            If chiesto <> Path.GetFileName(chiesto) OrElse chiesto = "." OrElse chiesto = ".." Then
                Return EsitoTool.NonRiuscito(
                    $"«{chiesto}» non è il nome di una cartella-opportunità: ci vuole il solo nome, " &
                    "senza percorso.")
            End If

            pulito = chiesto
            Return Nothing

        End Function

        ''' <summary>Un file JSON della cartella dati, consegnato tale e quale.</summary>
        Private Shared Function LeggiFile(percorso As String, chiCosE As String) As EsitoTool

            Try
                Return EsitoTool.Riuscito(JsonNode.Parse(File.ReadAllText(percorso, Encoding.UTF8)))

            Catch ex As JsonException
                Return EsitoTool.NonRiuscito(
                    $"{chiCosE} c'è ma non si lascia leggere come JSON: {ex.Message}")

            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                Return EsitoTool.NonRiuscito($"{chiCosE} non si è lasciato aprire: {ex.Message}")
            End Try

        End Function

    End Class

End Namespace
