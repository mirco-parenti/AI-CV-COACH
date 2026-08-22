Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Windows.Forms
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

''' <summary>Cosa l'utente ha deciso davanti ai testi che stava riscrivendo.</summary>
Public Enum EsitoModifica

    ''' <summary>Ha chiuso senza confermare: i documenti restano come li aveva trovati.</summary>
    Annullato

    ''' <summary>Va bene così: le riscritture entrano nei documenti.</summary>
    Confermato

End Enum

''' <summary>
''' Un documento aperto alla modifica a mano, con i testi da cui la rifinitura era
''' partita.
''' </summary>
Public Class DocumentoDaRiscrivere

    ''' <summary>Il documento: un CV o una lettera.</summary>
    Public Property Documento As JsonNode

    ''' <summary>
    ''' Com'erano i suoi campi prima dell'anti-slop, o <c>Nothing</c> se non ne è cambiato
    ''' nessuno: è quello che il «Ripristina» rimette nella casella (cap. 08.4).
    ''' </summary>
    Public Property PrimaDellaRifinitura As JsonNode

End Class

''' <summary>
''' La <b>modifica a mano</b> dei testi di P6 (T9d, cap. 08.4): l'ultima delle tre cose
''' che il capitolo prometteva davanti al prima/dopo — accettare, riscrivere, tornare
''' alla versione non rifinita.
''' </summary>
''' <remarks>
''' <para><b>Perché una finestra e non le caselle di P6.</b> Le tre caselle del pannello
''' non mostrano il documento: mostrano la <b>pagina di blocchi</b> con cui finirà nel
''' DOCX e nel PDF (cap. 05.3), etichette di sezione comprese, e in coda il prima/dopo,
''' che documento non è. Renderle scrivibili vorrebbe dire ricostruire il JSON da quel
''' testo — un mestiere che nessuna delle tre stampanti fa, e che sbaglierebbe proprio
''' dove l'utente si fida di più.</para>
''' <para><b>Solo la prosa scritta dall'AI</b>: sommario, descrizioni delle esperienze,
''' corpo della lettera. Non i fatti — nomi, aziende, date, competenze, titoli — che
''' vengono dal profilo: cambiarli qui li farebbe divergere in silenzio dal profilo che
''' li custodisce, e tornerebbero com'erano alla prima rigenerazione. Quali campi siano
''' prosa non lo decide questa finestra: lo chiede a <see cref="Rifinitura"/>, che è
''' l'unico posto che lo sa.</para>
''' <para><b>Qui non si chiama l'AI e non si scrive su disco</b>, come nella finestra
''' degli appunti: si raccoglie una decisione. I documenti si toccano solo con
''' <see cref="Applica"/>, cioè dopo il «Salva» — annullando, quello che si era scritto
''' muore con la finestra. A salvare è il pannello.</para>
''' <para><b>L'anti-invenzione non c'entra.</b> Quel vincolo tiene la <b>macchina</b>
''' dentro i fatti dichiarati; qui a scrivere è l'utente, che dei suoi fatti è il
''' proprietario. Il testo che esce di qui non passa da nessun prompt: entra nel
''' documento com'è stato scritto.</para>
''' </remarks>
Public Class FinestraModificaTesti

    ''' <summary>Il segno che marca, nell'elenco, i campi riscritti a mano in questo giro.</summary>
    Private Const SegnoRiscritto As String = "✎"

    ''' <summary>Un campo di prosa aperto alla riscrittura, con tutto ciò che lo riguarda.</summary>
    Private Class Voce

        ''' <summary>Il documento a cui appartiene: è lì che il testo va rimesso.</summary>
        Public Property Documento As JsonNode

        Public Property Id As String
        Public Property Etichetta As String

        ''' <summary>Il testo com'era all'apertura della finestra.</summary>
        Public Property Originale As String

        ''' <summary>Il testo com'è adesso nella casella.</summary>
        Public Property Testo As String

        ''' <summary>Il testo prima della rifinitura, o <c>Nothing</c> se non fu cambiato.</summary>
        Public Property NonRifinito As String

        ''' <summary>Se in questo giro l'utente l'ha cambiato.</summary>
        Public ReadOnly Property Riscritto As Boolean
            Get
                Return Not String.Equals(Testo, Originale, StringComparison.Ordinal)
            End Get
        End Property

    End Class

    Private ReadOnly _voci As New List(Of Voce)

    Private ReadOnly _suggerimenti As New ToolTip()

    ''' <summary>Quante volte si sta riempiendo un controllo: gli eventi di riflesso non contano.</summary>
    Private _riempimenti As Integer

    ''' <summary>Cosa ha deciso l'utente; <see cref="EsitoModifica.Annullato"/> finché non decide.</summary>
    Public ReadOnly Property Esito As EsitoModifica = EsitoModifica.Annullato

    ''' <summary>
    ''' Prepara la finestra sui documenti da riscrivere. È pubblica perché il banco la
    ''' costruisce e la interroga senza mostrarla.
    ''' </summary>
    ''' <param name="documenti">
    ''' I documenti in mostra nel pannello: per una candidatura sono due — il 🎯 CV mirato
    ''' e la ✉️ lettera — per il 📄 CV base uno solo. Quelli senza campi di prosa non
    ''' portano righe, e non è un errore: è un documento che non ha niente da riscrivere.
    ''' </param>
    Public Sub New(documenti As IEnumerable(Of DocumentoDaRiscrivere))

        InitializeComponent()

        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        If documenti Is Nothing Then Throw New ArgumentNullException(NameOf(documenti))

        RaccogliLaProsa(documenti)

        lblSpiegazione.Text =
            "Qui riscrivi i testi che ho scritto io: il sommario, le descrizioni delle esperienze, " &
            "il corpo della lettera." & vbLf &
            "I fatti — nomi, aziende, date, competenze, titoli — vengono dal tuo profilo e si " &
            "cambiano di là, o alla prossima rigenerazione tornerebbero com'erano."

        Vesti()
        MostraICampi()
        MostraIlCampoScelto()

        ' Esc annulla; nessun bottone appeso a Invio, perché si scrive nella casella del
        ' testo e un Invio di passaggio chiuderebbe la finestra a metà lavoro.
        CancelButton = btnAnnulla
        AcceptButton = Nothing

    End Sub

    ''' <summary>
    ''' Mostra la finestra e mette nei documenti quello che l'utente ha riscritto.
    ''' </summary>
    ''' <returns>
    ''' Quanti campi sono stati riscritti davvero: zero se ha annullato, e zero anche se
    ''' ha confermato senza cambiare niente — che per chi salva è la stessa cosa.
    ''' </returns>
    Public Shared Function Chiedi(proprietario As IWin32Window,
                                  documenti As IEnumerable(Of DocumentoDaRiscrivere)) As Integer

        Using finestra As New FinestraModificaTesti(documenti)

            finestra.ShowDialog(proprietario)
            If finestra.Esito <> EsitoModifica.Confermato Then Return 0

            Return finestra.Applica()

        End Using

    End Function

    ''' <summary>Quanti campi di prosa ci sono da riscrivere.</summary>
    Public ReadOnly Property Quanti As Integer
        Get
            Return _voci.Count
        End Get
    End Property

    ''' <summary>Il nome del campo in quella riga, o vuoto se quella riga non c'è.</summary>
    Public Function Etichetta(indice As Integer) As String

        If Not CE(indice) Then Return String.Empty

        Return _voci(indice).Etichetta

    End Function

    ''' <summary>Il testo del campo in quella riga com'è adesso nella finestra.</summary>
    Public Function Testo(indice As Integer) As String

        If Not CE(indice) Then Return String.Empty

        Return _voci(indice).Testo

    End Function

    ''' <summary>
    ''' Riscrive il testo di un campo.
    ''' </summary>
    ''' <remarks>
    ''' Sta in un metodo pubblico, staccato dalla casella, perché è l'unico modo di
    ''' collaudarlo: su una finestra mai mostrata i controlli non si lasciano pilotare
    ''' (v. <see cref="FinestraAppunti.RiscriviLAppunto"/>).
    ''' </remarks>
    ''' <returns>Falso se quella riga non c'è o il testo è vuoto.</returns>
    Public Function Riscrivi(indice As Integer, testo As String) As Boolean

        If Not CE(indice) Then Return False
        If String.IsNullOrWhiteSpace(testo) Then Return False

        _voci(indice).Testo = testo
        AggiornaLaRiga(indice)

        Return True

    End Function

    ''' <summary>Se di quel campo si conserva il testo da cui la rifinitura era partita.</summary>
    Public Function SiPuoRipristinare(indice As Integer) As Boolean

        If Not CE(indice) Then Return False

        Return Not String.IsNullOrWhiteSpace(_voci(indice).NonRifinito)

    End Function

    ''' <summary>
    ''' Rimette nel campo il testo che c'era prima della rifinitura anti-slop.
    ''' </summary>
    ''' <remarks>
    ''' È la terza delle tre cose promesse dal cap. 08.4 — «tornare alla versione non
    ''' rifinita» — e vive qui e non nel pannello perché è la stessa mano che riscrive:
    ''' ripristinare è riscrivere con un testo che si conosce già. Come ogni altra
    ''' riscrittura non tocca il documento finché non si conferma.
    ''' </remarks>
    ''' <returns>Falso se quella riga non c'è, o se di quel campo non si conserva il prima.</returns>
    Public Function Ripristina(indice As Integer) As Boolean

        If Not SiPuoRipristinare(indice) Then Return False

        Return Riscrivi(indice, _voci(indice).NonRifinito)

    End Function

    ''' <summary>
    ''' Mette nei documenti i campi riscritti.
    ''' </summary>
    ''' <remarks>
    ''' I campi lasciati stare non si riscrivono affatto: rimettere a posto un testo
    ''' identico non cambierebbe niente nel file, ma farebbe contare come «modificato» un
    ''' documento che nessuno ha toccato.
    ''' </remarks>
    ''' <returns>Quanti campi sono finiti davvero nei documenti.</returns>
    Public Function Applica() As Integer

        Dim scritti As Integer

        For Each voce As Voce In _voci

            If Not voce.Riscritto Then Continue For
            If Rifinitura.Riscrivi(voce.Documento, voce.Id, voce.Testo) Then scritti += 1

        Next

        Return scritti

    End Function

    ''' <summary>
    ''' Mette in fila i campi di prosa di tutti i documenti, ciascuno col suo «prima».
    ''' </summary>
    Private Sub RaccogliLaProsa(documenti As IEnumerable(Of DocumentoDaRiscrivere))

        For Each documento As DocumentoDaRiscrivere In documenti

            If documento Is Nothing OrElse documento.Documento Is Nothing Then Continue For

            Dim prima As JsonObject = TryCast(documento.PrimaDellaRifinitura, JsonObject)

            For Each campo As Rifinitura.CampoDiProsa In Rifinitura.CampiDiProsa(documento.Documento)

                _voci.Add(New Voce With {
                    .Documento = documento.Documento,
                    .Id = campo.Id,
                    .Etichetta = campo.Etichetta,
                    .Originale = campo.Testo,
                    .Testo = campo.Testo,
                    .NonRifinito = If(prima Is Nothing, Nothing, CampiJson.Testo(prima, campo.Id))})

            Next

        Next

    End Sub

    ''' <summary>I colori e i font, tutti da <see cref="StileApp"/> (cap. 03.2).</summary>
    Private Sub Vesti()

        BackColor = StileApp.SfondoContenuto
        Font = StileApp.FontTesto

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli

        lblSpiegazione.ForeColor = StileApp.TestoPrimario
        lblModifica.ForeColor = StileApp.TestoPrimario

        lvwCampi.BackColor = StileApp.SfondoContenuto
        txtTesto.BackColor = StileApp.SfondoContenuto

        StileApp.VestiBottone(btnSalva, LivelloBottone.SicuroPositivo)
        StileApp.VestiBottone(btnRipristina, LivelloBottone.Attenzione)
        StileApp.VestiBottone(btnAnnulla, LivelloBottone.Neutro)

    End Sub

    ''' <summary>Riempie l'elenco dei campi e sceglie il primo.</summary>
    Private Sub MostraICampi()

        _riempimenti += 1

        Try
            lvwCampi.Items.Clear()

            For Each voce As Voce In _voci
                lvwCampi.Items.Add(New ListViewItem({voce.Etichetta, UnaRiga(voce.Testo), String.Empty}))
            Next

            If lvwCampi.Items.Count > 0 Then lvwCampi.Items(0).Selected = True

        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ''' <summary>
    ''' Il testo come si legge nella colonna dell'elenco: una riga sola, perché una
    ''' descrizione va a capo e la riga di un elenco no.
    ''' </summary>
    Private Shared Function UnaRiga(testo As String) As String

        If String.IsNullOrEmpty(testo) Then Return String.Empty

        Return testo.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbCr, " ")

    End Function

    ''' <summary>Rimette in riga quello che di un campo si vede nell'elenco.</summary>
    Private Sub AggiornaLaRiga(indice As Integer)

        If indice < 0 OrElse indice >= lvwCampi.Items.Count Then Return

        _riempimenti += 1

        Try
            lvwCampi.Items(indice).SubItems(1).Text = UnaRiga(_voci(indice).Testo)
            lvwCampi.Items(indice).SubItems(2).Text = If(_voci(indice).Riscritto, SegnoRiscritto, String.Empty)
        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ''' <summary>Porta nella casella il campo scelto, e dice cosa se ne può fare.</summary>
    Private Sub MostraIlCampoScelto()

        Dim indice As Integer = IndiceScelto

        _riempimenti += 1

        Try
            txtTesto.Enabled = CE(indice)
            txtTesto.Text = If(CE(indice), _voci(indice).Testo, String.Empty)

            lblModifica.Text = If(CE(indice),
                                  $"Il testo di «{_voci(indice).Etichetta}» (puoi riscriverlo):",
                                  "Scegli un campo dall'elenco.")

            btnRipristina.Enabled = SiPuoRipristinare(indice)

            _suggerimenti.SetToolTip(btnRipristina,
                If(btnRipristina.Enabled,
                   "Rimette il testo com'era prima della rifinitura anti-slop.",
                   "Su questo campo non c'è niente da ripristinare: la rifinitura non l'ha cambiato."))

        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ''' <summary>La riga scelta, o -1.</summary>
    Private ReadOnly Property IndiceScelto As Integer
        Get
            If lvwCampi.SelectedIndices.Count = 0 Then Return -1
            Return lvwCampi.SelectedIndices(0)
        End Get
    End Property

    ''' <summary>Se quell'indice è una riga che esiste.</summary>
    Private Function CE(indice As Integer) As Boolean

        Return indice >= 0 AndAlso indice < _voci.Count

    End Function

    Private Sub lvwCampi_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lvwCampi.SelectedIndexChanged

        MostraIlCampoScelto()

    End Sub

    Private Sub txtTesto_TextChanged(sender As Object, e As EventArgs) Handles txtTesto.TextChanged

        If _riempimenti > 0 Then Return

        Riscrivi(IndiceScelto, txtTesto.Text)

    End Sub

    Private Sub btnRipristina_Click(sender As Object, e As EventArgs) Handles btnRipristina.Click

        Dim indice As Integer = IndiceScelto
        If Not Ripristina(indice) Then Return

        ' La casella si rilegge dalla voce: scrivendoci dentro si passerebbe di nuovo da
        ' TextChanged, e la riscrittura si farebbe due volte per lo stesso gesto.
        MostraIlCampoScelto()

    End Sub

    Private Sub btnSalva_Click(sender As Object, e As EventArgs) Handles btnSalva.Click
        Chiudi(EsitoModifica.Confermato, DialogResult.OK)
    End Sub

    Private Sub btnAnnulla_Click(sender As Object, e As EventArgs) Handles btnAnnulla.Click
        Chiudi(EsitoModifica.Annullato, DialogResult.Cancel)
    End Sub

    Private Sub Chiudi(esito As EsitoModifica, risultato As DialogResult)

        _Esito = esito
        DialogResult = risultato
        Close()

    End Sub

End Class
