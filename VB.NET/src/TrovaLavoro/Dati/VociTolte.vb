Imports System.Text.Json
Imports System.Text.Json.Nodes

Namespace Dati

    ''' <summary>
    ''' Le voci che l'utente ha tolto da <b>questo</b> documento, e quando (R6, cap. 08.4).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Cosa risolve.</b> Un CV generato porta tutto quello che il profilo
    ''' dichiara, ed è giusto: il profilo è la fonte dei fatti. Ma un CV mandato a
    ''' un'azienda non è l'inventario di una vita — su quell'annuncio, tre esperienze su
    ''' dieci non dicono niente, e chi si candida vuole poterle lasciare fuori. Toglierle
    ''' dal profilo sarebbe sbagliato: sparirebbero da tutte le candidature, anche da
    ''' quelle dove contano.</para>
    ''' <para><b>Perché il documento resta intero.</b> Quel che si toglie non si cancella:
    ''' il <c>cv.json</c> continua a contenere ogni voce, e qui accanto si annota quali
    ''' non vanno mostrate (<see cref="Documenti.VociDelCv.ComeSiVede"/>). Così rimettere
    ''' una voce è togliere una riga da questo elenco, e non costa una rigenerazione — e
    ''' una rigenerazione, dal canto suo, non porta via il lavoro fatto.</para>
    ''' <para><b>Perché l'impronta e non l'indice.</b> Perché una voce tolta resta tolta
    ''' anche dopo un «Rigenera», e il documento nuovo lo scrive il modello: contando le
    ''' voci si finirebbe per togliere quella che nel frattempo ha preso quel posto. Le
    ''' impronte le costruisce <see cref="Documenti.VociDelCv.ImprontaDi"/>, che è l'unico
    ''' posto a sapere quali campi di una voce sono fatti e quali sono prosa.</para>
    ''' <para><b>Un blocco che tace quando non ha niente da dire</b>, come le
    ''' <see cref="RiscrittureAMano"/>: se non è stato tolto niente, nei file non compare
    ''' affatto. I documenti scritti prima di R6 si riaprono senza, e valgono come «niente
    ''' tolto» — che è esattamente com'erano (cap. 11.1).</para>
    ''' </remarks>
    Public Class VociTolte

        ''' <summary>
        ''' Le impronte delle voci tolte, nella forma che costruisce
        ''' <see cref="Documenti.VociDelCv.ImprontaDi"/>.
        ''' </summary>
        Public ReadOnly Property Impronte As New List(Of String)

        ''' <summary>
        ''' Quando è stata tolta l'ultima voce; la data vuota se non ce n'è nessuna. Serve
        ''' alla spia della lettera: togliere una voce dal CV cambia la storia che il CV
        ''' racconta, e la lettera che la ripete resta indietro come dopo una riscrittura
        ''' (<see cref="Motore.Opportunita.LetteraDaRiallineare"/>).
        ''' </summary>
        Public Property Quando As Date

        ''' <summary>Se da questo documento è stata tolta almeno una voce.</summary>
        Public ReadOnly Property CEQualcosa As Boolean
            Get
                Return Impronte.Count > 0
            End Get
        End Property

        ''' <summary>Se questa voce è stata tolta dal documento.</summary>
        Public Function Contiene(impronta As String) As Boolean

            If String.IsNullOrWhiteSpace(impronta) Then Return False
            Return Impronte.Contains(impronta)

        End Function

        ''' <summary>
        ''' Toglie una voce dal documento. Toglierla due volte non la duplica, ma aggiorna
        ''' la data: è quella dell'<b>ultima</b> volta che l'utente ha cambiato il taglio.
        ''' </summary>
        Public Sub Togli(impronta As String, quando As Date)

            If String.IsNullOrWhiteSpace(impronta) Then Return

            If Not Impronte.Contains(impronta) Then Impronte.Add(impronta)

            ' «Me.» non è pignoleria: senza, VB assegnerebbe il parametro a sé stesso e la
            ' proprietà resterebbe vuota, senza un errore di compilazione.
            Me.Quando = quando

        End Sub

        ''' <summary>
        ''' Rimette nel documento una voce tolta. La data resta quella dell'ultimo
        ''' cambiamento, perché anche rimettere cambia quel che il CV racconta.
        ''' </summary>
        Public Sub Rimetti(impronta As String, quando As Date)

            If String.IsNullOrWhiteSpace(impronta) Then Return
            If Not Impronte.Remove(impronta) Then Return

            Me.Quando = quando

        End Sub

        ''' <summary>
        ''' Dimentica tutto. A differenza delle riscritture non lo chiama «Rigenera» — un
        ''' taglio scelto dall'utente vale anche sul documento nuovo — ma serve a chi
        ''' ricostruisce questo elenco da un file.
        ''' </summary>
        Public Sub Dimentica()

            Impronte.Clear()
            Quando = Nothing

        End Sub

        ''' <summary>
        ''' Prende su di sé quel che è annotato altrove: è la copia che il pannello tiene
        ''' in mano di ciò che sta nel file del 📄 CV base, che a differenza di una
        ''' candidatura non ha un oggetto suo in memoria.
        ''' </summary>
        Public Sub Prendi(altre As VociTolte)

            Dimentica()

            If altre Is Nothing Then Return

            Impronte.AddRange(altre.Impronte)
            Quando = altre.Quando

        End Sub

        ''' <summary>
        ''' Come si scrive nel file; <c>Nothing</c> quando non c'è niente da scrivere, così
        ''' chi salva sa che quel blocco va lasciato fuori.
        ''' </summary>
        Public Function ComeJson() As JsonObject

            If Not CEQualcosa Then Return Nothing

            Dim elenco As New JsonArray()
            For Each impronta As String In Impronte
                elenco.Add(JsonValue.Create(impronta))
            Next

            Return New JsonObject From {
                {"voci", elenco},
                {"quando", CampiJson.Quando(Quando)}}

        End Function

        ''' <summary>
        ''' Rimette dentro quel che il file conserva. Un file senza questo blocco — tutti
        ''' quelli scritti prima di R6 — lascia il documento intero, che è com'era.
        ''' </summary>
        Public Sub Rileggi(scritto As JsonObject)

            Dimentica()

            If scritto Is Nothing Then Return

            Dim elenco As JsonArray = TryCast(CampiJson.Nodo(scritto, "voci"), JsonArray)
            If elenco IsNot Nothing Then

                For Each voce As JsonNode In elenco

                    If voce Is Nothing OrElse voce.GetValueKind() <> JsonValueKind.String Then Continue For

                    Dim impronta As String = voce.GetValue(Of String)()
                    If Not String.IsNullOrWhiteSpace(impronta) AndAlso
                       Not Impronte.Contains(impronta) Then Impronte.Add(impronta)

                Next

            End If

            Quando = CampiJson.Istante(scritto, "quando")

        End Sub

    End Class

End Namespace
