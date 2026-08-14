Imports System.Windows.Forms
Imports TrovaLavoro.Dati

''' <summary>Cosa l'utente ha deciso davanti all'elenco dei suoi documenti.</summary>
Public Enum EsitoDocumenti

    ''' <summary>Ha chiuso senza confermare: quel che ha toccato si butta.</summary>
    Annullato

    ''' <summary>Va bene così: si salva.</summary>
    Confermato

    ''' <summary>Chiede di farla rileggere all'AI.</summary>
    Rileggere

    ''' <summary>Chiede di indicare un'altra cartella.</summary>
    CambiaCartella

End Enum

''' <summary>
''' La <b>conferma umana</b> del cap. 05.2, passo 4: l'utente vede cosa il programma ha
''' riconosciuto nella sua cartella e può correggerlo, prima che un solo file finisca
''' fra gli allegati di un'email.
''' </summary>
''' <remarks>
''' <para><b>Qui non si chiama l'AI.</b> Rileggere la cartella costa un'attesa e va
''' potuta annullare, e chi sa fare quelle due cose è il pannello (P7) con la sua fascia
''' di stato: la finestra si limita a <b>chiedere</b> la rilettura chiudendosi con
''' <see cref="EsitoDocumenti.Rileggere"/>. È la stessa ragione per cui la scelta della
''' cartella torna indietro invece di aprirsi da qui.</para>
''' <para>La categoria si corregge <b>scegliendo la riga e poi la voce</b>, invece che
''' con una tendina dentro ogni riga: tutta l'applicazione mostra gli elenchi con una
''' <see cref="ListView"/> (la coda di P1, i giudizi di P4), e una griglia editabile
''' sarebbe l'unico controllo del suo genere in tutto il programma — da vestire, da
''' collaudare e da spiegare a chi la incontra per la prima volta.</para>
''' <para>Una correzione fatta a mano <b>resta</b>: il documento si marca
''' <c>Corretto</c>, e la rilettura successiva non ci ripassa sopra.</para>
''' </remarks>
Public Class FinestraDocumenti

    ''' <summary>Le categorie come si offrono, nell'ordine in cui si incontrano.</summary>
    Private Shared ReadOnly Categorie As CategoriaDocumento() = {
        CategoriaDocumento.Cv, CategoriaDocumento.Attestato,
        CategoriaDocumento.Lettera, CategoriaDocumento.Altro}

    Private ReadOnly _raccolta As RaccoltaDocumenti

    ''' <summary>Quante volte si sta riempiendo l'elenco: gli eventi di riflesso non contano.</summary>
    Private _riempimenti As Integer

    ''' <summary>Cosa ha deciso l'utente; <see cref="EsitoDocumenti.Annullato"/> finché non decide.</summary>
    Public ReadOnly Property Esito As EsitoDocumenti = EsitoDocumenti.Annullato

    ''' <summary>
    ''' Prepara la finestra sulla raccolta da confermare. È pubblica perché il banco la
    ''' costruisce e la interroga senza mostrarla.
    ''' </summary>
    ''' <param name="raccolta">
    ''' La raccolta viva: le correzioni si scrivono <b>qui dentro</b>. Chi ha chiamato,
    ''' se l'esito è <see cref="EsitoDocumenti.Annullato"/>, la rilegge da disco.
    ''' </param>
    Public Sub New(raccolta As RaccoltaDocumenti)

        InitializeComponent()

        If raccolta Is Nothing Then Throw New ArgumentNullException(NameOf(raccolta))
        _raccolta = raccolta

        lblSpiegazione.Text =
            "Ho guardato i file della tua cartella e ho provato a dire cos'è ciascuno. " &
            "Correggi quello che ho sbagliato: scegli un documento e poi la voce giusta qui sotto." & vbLf &
            "Gli attestati compariranno fra gli allegati proposti quando prepari un'email — sempre da spuntare, " &
            "nessun file parte da solo."

        lblCartella.Text = If(String.IsNullOrWhiteSpace(_raccolta.Cartella),
                              "Nessuna cartella scelta.",
                              $"Cartella: {_raccolta.Cartella}")

        For Each categoria As CategoriaDocumento In Categorie
            cmbCategoria.Items.Add(DocumentoClassificato.ComeSiLegge(categoria))
        Next

        Vesti()
        MostraIDocumenti()

        ' Esc annulla; nessun bottone appeso a Invio, perché l'elenco si percorre con la
        ' tastiera e un Invio di passaggio chiuderebbe la finestra a metà lavoro.
        CancelButton = btnAnnulla
        AcceptButton = Nothing

    End Sub

    ''' <summary>
    ''' Mostra la finestra e dice cosa l'utente ha deciso. Le correzioni sono già dentro
    ''' la raccolta che è stata passata.
    ''' </summary>
    Public Shared Function Chiedi(proprietario As IWin32Window, raccolta As RaccoltaDocumenti) As EsitoDocumenti

        Using finestra As New FinestraDocumenti(raccolta)
            finestra.ShowDialog(proprietario)
            Return finestra.Esito
        End Using

    End Function

    ''' <summary>I colori e i font, tutti da <see cref="StileApp"/> (cap. 03.2).</summary>
    Private Sub Vesti()

        BackColor = StileApp.SfondoContenuto
        Font = StileApp.FontTesto

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli

        lblSpiegazione.ForeColor = StileApp.TestoPrimario
        lblCartella.ForeColor = StileApp.TestoSecondario
        lblScelta.ForeColor = StileApp.TestoPrimario

        lvwDocumenti.BackColor = StileApp.SfondoContenuto

        StileApp.VestiBottone(btnRileggi, LivelloBottone.AzionePrincipale)
        StileApp.VestiBottone(btnCambiaCartella, LivelloBottone.Esplorativo)
        StileApp.VestiBottone(btnConferma, LivelloBottone.SicuroPositivo)
        StileApp.VestiBottone(btnAnnulla, LivelloBottone.Neutro)

    End Sub

    ''' <summary>Riempie l'elenco con quel che si sa dei documenti, in ordine di nome.</summary>
    Private Sub MostraIDocumenti()

        _riempimenti += 1

        Try
            lvwDocumenti.Items.Clear()

            For Each documento As DocumentoClassificato In _raccolta.Documenti
                lvwDocumenti.Items.Add(New ListViewItem({
                    documento.Nome,
                    DocumentoClassificato.ComeSiLegge(documento.Categoria),
                    documento.Motivo}))
            Next

        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ''' <summary>
    ''' Il documento scelto nell'elenco, o <c>Nothing</c> se non ce n'è uno.
    ''' </summary>
    Public ReadOnly Property DocumentoScelto As DocumentoClassificato
        Get
            Dim indice As Integer = IndiceScelto
            If indice < 0 OrElse indice >= _raccolta.Documenti.Count Then Return Nothing

            Return _raccolta.Documenti(indice)
        End Get
    End Property

    ''' <summary>La riga scelta, o -1.</summary>
    Private ReadOnly Property IndiceScelto As Integer
        Get
            If lvwDocumenti.SelectedIndices.Count = 0 Then Return -1
            Return lvwDocumenti.SelectedIndices(0)
        End Get
    End Property

    ''' <summary>
    ''' Cambia la categoria di un documento e la marca come decisa dall'utente.
    ''' </summary>
    ''' <remarks>
    ''' Sta in un metodo pubblico, staccato dalla tendina, perché è l'unico modo di
    ''' collaudarlo: su una finestra mai mostrata i controlli non si lasciano pilotare
    ''' (v. <see cref="FinestraChiaveApi.PrendiLaChiaveScritta"/>).
    ''' </remarks>
    ''' <returns>Falso se quell'indice non esiste.</returns>
    Public Function CambiaLaCategoria(indice As Integer, categoria As CategoriaDocumento) As Boolean

        If indice < 0 OrElse indice >= _raccolta.Documenti.Count Then Return False

        Dim documento As DocumentoClassificato = _raccolta.Documenti(indice)

        documento.Categoria = categoria
        documento.Corretto = True
        documento.Motivo = "L'hai detto tu."

        _riempimenti += 1

        Try
            If indice < lvwDocumenti.Items.Count Then
                lvwDocumenti.Items(indice).SubItems(1).Text = DocumentoClassificato.ComeSiLegge(categoria)
                lvwDocumenti.Items(indice).SubItems(2).Text = documento.Motivo
            End If
        Finally
            _riempimenti -= 1
        End Try

        Return True

    End Function

    Private Sub lvwDocumenti_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lvwDocumenti.SelectedIndexChanged

        Dim documento As DocumentoClassificato = DocumentoScelto

        _riempimenti += 1

        Try
            cmbCategoria.Enabled = documento IsNot Nothing
            cmbCategoria.SelectedIndex = If(documento Is Nothing, -1, Array.IndexOf(Categorie, documento.Categoria))
        Finally
            _riempimenti -= 1
        End Try

    End Sub

    Private Sub cmbCategoria_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles cmbCategoria.SelectedIndexChanged

        If _riempimenti > 0 Then Return
        If cmbCategoria.SelectedIndex < 0 OrElse cmbCategoria.SelectedIndex >= Categorie.Length Then Return

        CambiaLaCategoria(IndiceScelto, Categorie(cmbCategoria.SelectedIndex))

    End Sub

    Private Sub btnConferma_Click(sender As Object, e As EventArgs) Handles btnConferma.Click
        Chiudi(EsitoDocumenti.Confermato, DialogResult.OK)
    End Sub

    Private Sub btnRileggi_Click(sender As Object, e As EventArgs) Handles btnRileggi.Click
        Chiudi(EsitoDocumenti.Rileggere, DialogResult.Retry)
    End Sub

    Private Sub btnCambiaCartella_Click(sender As Object, e As EventArgs) Handles btnCambiaCartella.Click
        Chiudi(EsitoDocumenti.CambiaCartella, DialogResult.Retry)
    End Sub

    Private Sub btnAnnulla_Click(sender As Object, e As EventArgs) Handles btnAnnulla.Click
        Chiudi(EsitoDocumenti.Annullato, DialogResult.Cancel)
    End Sub

    Private Sub Chiudi(esito As EsitoDocumenti, risultato As DialogResult)

        _Esito = esito
        DialogResult = risultato
        Close()

    End Sub

End Class
