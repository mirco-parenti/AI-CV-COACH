Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Motore

    ''' <summary>Da dove arrivano le impostazioni in uso.</summary>
    Public Enum OrigineImpostazioni
        ''' <summary>I valori che il programma porta dentro di sé.</summary>
        Predefinite
        ''' <summary>Il file impostazioni.json della cartella dati.</summary>
        File
    End Enum

    ''' <summary>
    ''' Le preferenze dell'utente: la lingua con cui si scrivono i documenti nuovi, se la
    ''' rifinitura anti-slop è accesa, e dopo quanti giorni di silenzio la Home ricorda una
    ''' candidatura spedita (cap. 03, pannello P8; cap. 07.3; cap. 08.4; cap. 10.1).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Sono l'opposto della <see cref="Taratura"/>, e per questo stanno in un file
    ''' loro.</b> I numeri delle stelle sono di prodotto e l'interfaccia non li mostra,
    ''' perché un punteggio che l'utente può spostare smette di misurare quanto è adatto a
    ''' quel posto (cap. 11.6). Queste invece sono scelte che <i>solo</i> l'utente può fare:
    ''' in che lingua scrive di solito, e se vuole che una macchina gli ritocchi la prosa.
    ''' Tenerle nello stesso file avrebbe mescolato due cose che si toccano con mani
    ''' diverse — una col permesso di chi usa il programma, l'altra col permesso di chi lo
    ''' fa.</para>
    ''' <para><b>Il ripiego è quello di casa</b> (cap. 11.6): file assente, parziale o
    ''' illeggibile non impedisce l'avvio, si torna ai predefiniti e lo si annota. Qui vale
    ''' a maggior ragione — perdere una preferenza costa una spunta da rimettere, e nessuno
    ''' vorrebbe un programma che si rifiuta di partire per quello.</para>
    ''' <para><b>I predefiniti non cambiano il comportamento di ieri</b>: italiano e
    ''' rifinitura accesa sono esattamente ciò che l'applicazione faceva prima che questo
    ''' file esistesse. Chi non apre mai le Impostazioni non deve accorgersi che sono
    ''' arrivate.</para>
    ''' </remarks>
    Public Class Impostazioni

        ''' <summary>
        ''' La lingua dei documenti nuovi quando nessun annuncio ne impone un'altra:
        ''' <see cref="LinguaDocumenti.Italiano"/> o <see cref="LinguaDocumenti.Inglese"/>.
        ''' </summary>
        Public Property LinguaPredefinita As String

        ''' <summary>
        ''' Se la rifinitura anti-slop lavora sui testi generati (cap. 08.4). Spenta, i
        ''' documenti escono col testo grezzo: è la stessa cosa che succede oggi quando
        ''' una rifinitura fallisce, quindi non è una strada nuova per il programma.
        ''' </summary>
        Public Property RifinituraAttiva As Boolean

        ''' <summary>
        ''' Dopo quanti giorni di silenzio la Home ricorda una candidatura spedita
        ''' (cap. 07.3). <b>Zero spegne il promemoria</b>: chi non lo vuole non deve
        ''' cercare un interruttore, gli basta scendere sotto il primo giorno.
        ''' </summary>
        ''' <remarks>
        ''' <para>Quattordici giorni è il valore deciso con Mirco il 2026-08-21, e non è
        ''' un numero neutro: due settimane sono l'intervallo dopo cui un sollecito non
        ''' sembra impaziente, e restano abbastanza dentro la memoria di chi ha letto
        ''' l'email. Ma è una regola sociale, non una legge, e cambia col settore: per
        ''' questo si sposta dalle Impostazioni invece di stare in una costante.</para>
        ''' <para>Il tetto è <see cref="GiorniFollowUpMassimi"/>: oltre l'anno il
        ''' promemoria non ricorderebbe più niente a nessuno, e un numero senza limite in
        ''' una casella è solo un modo di scrivere «spento» per sbaglio.</para>
        ''' </remarks>
        Public Property GiorniFollowUp As Integer

        ''' <summary>Il valore di casa: due settimane di silenzio (cap. 07.3).</summary>
        Public Const GiorniFollowUpPredefiniti As Integer = 14

        ''' <summary>Oltre un anno non è più un promemoria.</summary>
        Public Const GiorniFollowUpMassimi As Integer = 365

        ''' <summary>Da dove vengono i valori in uso.</summary>
        Public Property Origine As OrigineImpostazioni = OrigineImpostazioni.Predefinite

        ''' <summary>
        ''' Motivo per cui si è ripiegato sui predefiniti, da annotare all'avvio;
        ''' <c>Nothing</c> se non c'è stato alcun ripiego.
        ''' </summary>
        Public Property Avviso As String

        ''' <summary>Quel che valeva prima che le Impostazioni esistessero.</summary>
        Public Shared Function Predefinite() As Impostazioni
            Return New Impostazioni With {
                .LinguaPredefinita = LinguaDocumenti.Italiano,
                .RifinituraAttiva = True,
                .GiorniFollowUp = GiorniFollowUpPredefiniti,
                .Origine = OrigineImpostazioni.Predefinite
            }
        End Function

        ''' <summary>
        ''' Il file delle impostazioni nella cartella dati predefinita. Dov'è la cartella
        ''' lo sa <see cref="Dati.CartellaDati"/>, non questa classe (cap. 11.1).
        ''' </summary>
        Public Shared ReadOnly Property PercorsoPredefinito As String
            Get
                Return Dati.CartellaDati.Predefinita().FileImpostazioni
            End Get
        End Property

        ''' <summary>
        ''' Carica le impostazioni dal file indicato. Se il file manca o è illeggibile
        ''' restituisce i predefiniti annotando il motivo in <see cref="Avviso"/>.
        ''' </summary>
        ''' <param name="percorso">Il file da leggere; se omesso, quello della cartella dati.</param>
        Public Shared Function Carica(Optional percorso As String = Nothing) As Impostazioni

            Dim file As String = If(percorso, PercorsoPredefinito)

            ' Il file che non c'è è il caso normale, non un guasto: fino al primo giro nelle
            ' Impostazioni nessuno l'ha mai scritto. Per questo non lascia avviso.
            If Not IO.File.Exists(file) Then Return Predefinite()

            Try
                Return DaJson(IO.File.ReadAllText(file, Text.Encoding.UTF8))
            Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is IOException _
                                       OrElse TypeOf ex Is UnauthorizedAccessException
                Dim i As Impostazioni = Predefinite()
                i.Avviso = $"Impostazioni illeggibili in «{file}» ({ex.Message}): uso i valori predefiniti."
                Return i
            End Try

        End Function

        ''' <summary>
        ''' Legge le impostazioni da un testo JSON. Una voce mancante o fuori dai valori
        ''' ammessi <b>non fa cadere le altre</b>: si scarta quella e si annota.
        ''' </summary>
        ''' <remarks>
        ''' È la differenza con la <see cref="Taratura"/>, dove una mappa storta si scarta
        ''' intera (cap. 11.6): là le voci si compongono in un unico punteggio, e tenerne
        ''' solo alcune lo falserebbe in silenzio. Qui le due preferenze non si parlano —
        ''' una lingua scritta male non dice niente sulla rifinitura — e buttare via anche
        ''' quella buona sarebbe una perdita gratuita.
        ''' </remarks>
        Public Shared Function DaJson(testo As String) As Impostazioni

            Dim letto As Impostazioni = Predefinite()
            Dim radice As JsonNode = JsonNode.Parse(testo)
            If radice Is Nothing Then Return letto

            Dim scartate As New List(Of String)

            Dim lingua As String = TryCast(radice("lingua_predefinita")?.GetValue(Of String)(), String)
            If lingua IsNot Nothing Then
                If EUnaLinguaAmmessa(lingua) Then
                    letto.LinguaPredefinita = lingua.Trim().ToLowerInvariant()
                Else
                    scartate.Add($"lingua_predefinita «{lingua}» non è fra le lingue che so scrivere")
                End If
            End If

            Try
                Dim rifinitura As JsonNode = radice("rifinitura_attiva")
                If rifinitura IsNot Nothing Then letto.RifinituraAttiva = rifinitura.GetValue(Of Boolean)()
            Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidOperationException
                scartate.Add("rifinitura_attiva non è né vero né falso")
            End Try

            Try
                Dim giorni As JsonNode = radice("giorni_follow_up")
                If giorni IsNot Nothing Then
                    Dim quanti As Integer = giorni.GetValue(Of Integer)()
                    If quanti >= 0 AndAlso quanti <= GiorniFollowUpMassimi Then
                        letto.GiorniFollowUp = quanti
                    Else
                        scartate.Add($"giorni_follow_up «{quanti}» è fuori da 0-{GiorniFollowUpMassimi}")
                    End If
                End If
            Catch ex As Exception When TypeOf ex Is FormatException OrElse TypeOf ex Is InvalidOperationException
                scartate.Add("giorni_follow_up non è un numero di giorni")
            End Try

            letto.Origine = OrigineImpostazioni.File
            If scartate.Count > 0 Then
                letto.Avviso = "Nelle impostazioni ho scartato: " & String.Join("; ", scartate) &
                               ". Per quelle vale il valore predefinito."
            End If

            Return letto

        End Function

        ''' <summary>Le impostazioni come JSON leggibile, quello che finisce su disco.</summary>
        Public Function VersoJson() As JsonObject
            Return New JsonObject From {
                {"lingua_predefinita", LinguaPredefinita},
                {"rifinitura_attiva", RifinituraAttiva},
                {"giorni_follow_up", GiorniFollowUp}
            }
        End Function

        ''' <summary>Se una lingua è una delle due che il pool sa scrivere (cap. 04.3).</summary>
        Private Shared Function EUnaLinguaAmmessa(lingua As String) As Boolean

            If String.IsNullOrWhiteSpace(lingua) Then Return False

            Dim pulita As String = lingua.Trim()

            Return pulita.Equals(LinguaDocumenti.Italiano, StringComparison.OrdinalIgnoreCase) OrElse
                   pulita.Equals(LinguaDocumenti.Inglese, StringComparison.OrdinalIgnoreCase)

        End Function

    End Class

End Namespace
