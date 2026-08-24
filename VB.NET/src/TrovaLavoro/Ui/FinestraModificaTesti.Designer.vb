<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraModificaTesti
    Inherits System.Windows.Forms.Form

    'Form esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Richiesto da Progettazione Windows Form
    Private components As System.ComponentModel.IContainer

    'NOTA: la procedura che segue è richiesta da Progettazione Windows Form
    'Può essere modificata mediante Progettazione Windows Form.
    'Non modificarla mediante l'editor del codice.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblTitolo = New System.Windows.Forms.Label()
        Me.lblSpiegazione = New System.Windows.Forms.Label()
        Me.lvwCampi = New System.Windows.Forms.ListView()
        Me.colCampo = New System.Windows.Forms.ColumnHeader()
        Me.colTesto = New System.Windows.Forms.ColumnHeader()
        Me.colSegno = New System.Windows.Forms.ColumnHeader()
        Me.lblNelDocumento = New System.Windows.Forms.Label()
        Me.lblFuori = New System.Windows.Forms.Label()
        Me.lvwFuori = New System.Windows.Forms.ListView()
        Me.colFuoriVoce = New System.Windows.Forms.ColumnHeader()
        Me.colFuoriCosa = New System.Windows.Forms.ColumnHeader()
        Me.btnTogli = New System.Windows.Forms.Button()
        Me.btnRimetti = New System.Windows.Forms.Button()
        Me.lblModifica = New System.Windows.Forms.Label()
        Me.txtTesto = New System.Windows.Forms.TextBox()
        Me.btnSalva = New System.Windows.Forms.Button()
        Me.btnAnnulla = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblTitolo
        '
        Me.lblTitolo.AutoSize = True
        Me.lblTitolo.Location = New System.Drawing.Point(14, 14)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(320, 30)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "Modifica i testi"
        '
        'lblSpiegazione
        '
        Me.lblSpiegazione.AutoSize = True
        Me.lblSpiegazione.Location = New System.Drawing.Point(14, 56)
        Me.lblSpiegazione.MaximumSize = New System.Drawing.Size(972, 0)
        Me.lblSpiegazione.Name = "lblSpiegazione"
        Me.lblSpiegazione.Size = New System.Drawing.Size(972, 45)
        Me.lblSpiegazione.TabIndex = 1
        Me.lblSpiegazione.Text = "Spiegazione"
        '
        'lvwCampi
        '
        Me.lvwCampi.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colCampo, Me.colTesto, Me.colSegno})
        Me.lvwCampi.FullRowSelect = True
        Me.lvwCampi.HideSelection = False
        Me.lvwCampi.Location = New System.Drawing.Point(14, 140)
        Me.lvwCampi.MultiSelect = False
        Me.lvwCampi.Name = "lvwCampi"
        Me.lvwCampi.Size = New System.Drawing.Size(540, 240)
        Me.lvwCampi.TabIndex = 2
        Me.lvwCampi.UseCompatibleStateImageBehavior = False
        Me.lvwCampi.View = System.Windows.Forms.View.Details
        '
        'colCampo
        '
        Me.colCampo.Text = "Campo"
        Me.colCampo.Width = 150
        '
        'colTesto
        '
        Me.colTesto.Text = "Il testo"
        Me.colTesto.Width = 315
        '
        'colSegno
        '
        Me.colSegno.Text = "✎"
        Me.colSegno.Width = 40
        '
        'lblNelDocumento
        '
        Me.lblNelDocumento.AutoSize = True
        Me.lblNelDocumento.Location = New System.Drawing.Point(14, 118)
        Me.lblNelDocumento.Name = "lblNelDocumento"
        Me.lblNelDocumento.Size = New System.Drawing.Size(200, 15)
        Me.lblNelDocumento.TabIndex = 2
        Me.lblNelDocumento.Text = "Nel documento"
        '
        'lblFuori
        '
        Me.lblFuori.AutoSize = True
        Me.lblFuori.Location = New System.Drawing.Point(690, 118)
        Me.lblFuori.Name = "lblFuori"
        Me.lblFuori.Size = New System.Drawing.Size(200, 15)
        Me.lblFuori.TabIndex = 5
        Me.lblFuori.Text = "Lasciate fuori"
        '
        'lvwFuori
        '
        Me.lvwFuori.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colFuoriVoce, Me.colFuoriCosa})
        Me.lvwFuori.FullRowSelect = True
        Me.lvwFuori.HideSelection = False
        Me.lvwFuori.Location = New System.Drawing.Point(690, 140)
        Me.lvwFuori.MultiSelect = False
        Me.lvwFuori.Name = "lvwFuori"
        Me.lvwFuori.Size = New System.Drawing.Size(296, 240)
        Me.lvwFuori.TabIndex = 6
        Me.lvwFuori.UseCompatibleStateImageBehavior = False
        Me.lvwFuori.View = System.Windows.Forms.View.Details
        '
        'colFuoriVoce
        '
        Me.colFuoriVoce.Text = "Voce"
        Me.colFuoriVoce.Width = 120
        '
        'colFuoriCosa
        '
        Me.colFuoriCosa.Text = "Cosa dice"
        Me.colFuoriCosa.Width = 172
        '
        'btnTogli
        '
        Me.btnTogli.Enabled = False
        Me.btnTogli.Location = New System.Drawing.Point(566, 200)
        Me.btnTogli.Name = "btnTogli"
        Me.btnTogli.Size = New System.Drawing.Size(110, 32)
        Me.btnTogli.TabIndex = 3
        Me.btnTogli.Text = "Togli →"
        '
        'btnRimetti
        '
        Me.btnRimetti.Enabled = False
        Me.btnRimetti.Location = New System.Drawing.Point(566, 244)
        Me.btnRimetti.Name = "btnRimetti"
        Me.btnRimetti.Size = New System.Drawing.Size(110, 32)
        Me.btnRimetti.TabIndex = 4
        Me.btnRimetti.Text = "← Rimetti"
        '
        'lblModifica
        '
        Me.lblModifica.AutoSize = True
        Me.lblModifica.Location = New System.Drawing.Point(14, 398)
        Me.lblModifica.Name = "lblModifica"
        Me.lblModifica.Size = New System.Drawing.Size(400, 15)
        Me.lblModifica.TabIndex = 7
        Me.lblModifica.Text = "Il testo scelto (puoi riscriverlo):"
        '
        'txtTesto
        '
        Me.txtTesto.Enabled = False
        Me.txtTesto.Location = New System.Drawing.Point(14, 420)
        Me.txtTesto.Multiline = True
        Me.txtTesto.Name = "txtTesto"
        Me.txtTesto.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtTesto.Size = New System.Drawing.Size(972, 110)
        Me.txtTesto.TabIndex = 8
        '
        'btnSalva
        '
        Me.btnSalva.Location = New System.Drawing.Point(724, 546)
        Me.btnSalva.Name = "btnSalva"
        Me.btnSalva.Size = New System.Drawing.Size(140, 32)
        Me.btnSalva.TabIndex = 9
        Me.btnSalva.Text = "Salva"
        '
        'btnAnnulla
        '
        Me.btnAnnulla.Location = New System.Drawing.Point(876, 546)
        Me.btnAnnulla.Name = "btnAnnulla"
        Me.btnAnnulla.Size = New System.Drawing.Size(110, 32)
        Me.btnAnnulla.TabIndex = 10
        Me.btnAnnulla.Text = "Annulla"
        '
        'FinestraModificaTesti
        '
        Me.ClientSize = New System.Drawing.Size(1000, 594)
        Me.Controls.Add(Me.lblTitolo)
        Me.Controls.Add(Me.lblSpiegazione)
        Me.Controls.Add(Me.lblNelDocumento)
        Me.Controls.Add(Me.lvwCampi)
        Me.Controls.Add(Me.btnTogli)
        Me.Controls.Add(Me.btnRimetti)
        Me.Controls.Add(Me.lblFuori)
        Me.Controls.Add(Me.lvwFuori)
        Me.Controls.Add(Me.lblModifica)
        Me.Controls.Add(Me.txtTesto)
        Me.Controls.Add(Me.btnSalva)
        Me.Controls.Add(Me.btnAnnulla)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraModificaTesti"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "TrovaLavoro"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSpiegazione As System.Windows.Forms.Label
    Friend WithEvents lvwCampi As System.Windows.Forms.ListView
    Friend WithEvents colCampo As System.Windows.Forms.ColumnHeader
    Friend WithEvents colTesto As System.Windows.Forms.ColumnHeader
    Friend WithEvents colSegno As System.Windows.Forms.ColumnHeader
    Friend WithEvents lblNelDocumento As System.Windows.Forms.Label
    Friend WithEvents lblFuori As System.Windows.Forms.Label
    Friend WithEvents lvwFuori As System.Windows.Forms.ListView
    Friend WithEvents colFuoriVoce As System.Windows.Forms.ColumnHeader
    Friend WithEvents colFuoriCosa As System.Windows.Forms.ColumnHeader
    Friend WithEvents btnTogli As System.Windows.Forms.Button
    Friend WithEvents btnRimetti As System.Windows.Forms.Button
    Friend WithEvents lblModifica As System.Windows.Forms.Label
    Friend WithEvents txtTesto As System.Windows.Forms.TextBox
    Friend WithEvents btnSalva As System.Windows.Forms.Button
    Friend WithEvents btnAnnulla As System.Windows.Forms.Button

End Class
