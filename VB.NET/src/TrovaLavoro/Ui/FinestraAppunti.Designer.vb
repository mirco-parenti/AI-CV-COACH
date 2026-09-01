<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraAppunti
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
        Me.lvwAppunti = New System.Windows.Forms.ListView()
        Me.colTipo = New System.Windows.Forms.ColumnHeader()
        Me.colAppunto = New System.Windows.Forms.ColumnHeader()
        Me.colDa = New System.Windows.Forms.ColumnHeader()
        Me.lblModifica = New System.Windows.Forms.Label()
        Me.txtAppunto = New System.Windows.Forms.TextBox()
        Me.lblFattiTitolo = New System.Windows.Forms.Label()
        Me.lvwFatti = New System.Windows.Forms.ListView()
        Me.colFatto = New System.Windows.Forms.ColumnHeader()
        Me.btnConferma = New System.Windows.Forms.Button()
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
        Me.lblTitolo.Text = "Gli appunti di questo ragionamento"
        '
        'lblSpiegazione
        '
        Me.lblSpiegazione.AutoSize = True
        Me.lblSpiegazione.Location = New System.Drawing.Point(14, 56)
        Me.lblSpiegazione.MaximumSize = New System.Drawing.Size(772, 0)
        Me.lblSpiegazione.Name = "lblSpiegazione"
        Me.lblSpiegazione.Size = New System.Drawing.Size(772, 45)
        Me.lblSpiegazione.TabIndex = 1
        Me.lblSpiegazione.Text = "Spiegazione"
        '
        'lvwAppunti
        '
        Me.lvwAppunti.CheckBoxes = True
        Me.lvwAppunti.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colTipo, Me.colAppunto, Me.colDa})
        Me.lvwAppunti.FullRowSelect = True
        Me.lvwAppunti.HideSelection = False
        Me.lvwAppunti.Location = New System.Drawing.Point(14, 120)
        Me.lvwAppunti.MultiSelect = False
        Me.lvwAppunti.Name = "lvwAppunti"
        Me.lvwAppunti.Size = New System.Drawing.Size(772, 232)
        Me.lvwAppunti.TabIndex = 2
        Me.lvwAppunti.UseCompatibleStateImageBehavior = False
        Me.lvwAppunti.View = System.Windows.Forms.View.Details
        '
        'colTipo
        '
        Me.colTipo.Text = "Che indicazione è"
        Me.colTipo.Width = 150
        '
        'colAppunto
        '
        Me.colAppunto.Text = "L'appunto"
        Me.colAppunto.Width = 400
        '
        'colDa
        '
        Me.colDa.Text = "Da dove nasce"
        Me.colDa.Width = 218
        '
        'lblModifica
        '
        Me.lblModifica.AutoSize = True
        Me.lblModifica.Location = New System.Drawing.Point(14, 364)
        Me.lblModifica.Name = "lblModifica"
        Me.lblModifica.Size = New System.Drawing.Size(300, 15)
        Me.lblModifica.TabIndex = 3
        Me.lblModifica.Text = "L'appunto scelto (puoi riscriverlo):"
        '
        'txtAppunto
        '
        Me.txtAppunto.Enabled = False
        Me.txtAppunto.Location = New System.Drawing.Point(14, 387)
        Me.txtAppunto.Multiline = True
        Me.txtAppunto.Name = "txtAppunto"
        Me.txtAppunto.Size = New System.Drawing.Size(772, 52)
        Me.txtAppunto.TabIndex = 4
        '
        'lblFattiTitolo
        '
        Me.lblFattiTitolo.AutoSize = True
        Me.lblFattiTitolo.Location = New System.Drawing.Point(14, 451)
        Me.lblFattiTitolo.MaximumSize = New System.Drawing.Size(772, 0)
        Me.lblFattiTitolo.Name = "lblFattiTitolo"
        Me.lblFattiTitolo.Size = New System.Drawing.Size(772, 30)
        Me.lblFattiTitolo.TabIndex = 5
        Me.lblFattiTitolo.Text = "Fatti nuovi"
        '
        'lvwFatti
        '
        Me.lvwFatti.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colFatto})
        Me.lvwFatti.FullRowSelect = True
        Me.lvwFatti.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None
        Me.lvwFatti.Location = New System.Drawing.Point(14, 493)
        Me.lvwFatti.MultiSelect = False
        Me.lvwFatti.Name = "lvwFatti"
        Me.lvwFatti.Size = New System.Drawing.Size(772, 76)
        Me.lvwFatti.TabIndex = 6
        Me.lvwFatti.UseCompatibleStateImageBehavior = False
        Me.lvwFatti.View = System.Windows.Forms.View.Details
        '
        'colFatto
        '
        Me.colFatto.Text = "Detto in chat"
        Me.colFatto.Width = 748
        '
        'btnConferma
        '
        Me.btnConferma.Location = New System.Drawing.Point(536, 583)
        Me.btnConferma.Name = "btnConferma"
        Me.btnConferma.Size = New System.Drawing.Size(140, 32)
        Me.btnConferma.TabIndex = 7
        Me.btnConferma.Text = "Conferma"
        '
        'btnAnnulla
        '
        Me.btnAnnulla.Location = New System.Drawing.Point(688, 583)
        Me.btnAnnulla.Name = "btnAnnulla"
        Me.btnAnnulla.Size = New System.Drawing.Size(110, 32)
        Me.btnAnnulla.TabIndex = 8
        Me.btnAnnulla.Text = "Annulla"
        '
        'FinestraAppunti
        '
        Me.ClientSize = New System.Drawing.Size(800, 629)
        Me.Controls.Add(Me.lblTitolo)
        Me.Controls.Add(Me.lblSpiegazione)
        Me.Controls.Add(Me.lvwAppunti)
        Me.Controls.Add(Me.lblModifica)
        Me.Controls.Add(Me.txtAppunto)
        Me.Controls.Add(Me.lblFattiTitolo)
        Me.Controls.Add(Me.lvwFatti)
        Me.Controls.Add(Me.btnConferma)
        Me.Controls.Add(Me.btnAnnulla)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraAppunti"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "TrovaLavoro"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSpiegazione As System.Windows.Forms.Label
    Friend WithEvents lvwAppunti As System.Windows.Forms.ListView
    Friend WithEvents colTipo As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAppunto As System.Windows.Forms.ColumnHeader
    Friend WithEvents colDa As System.Windows.Forms.ColumnHeader
    Friend WithEvents lblModifica As System.Windows.Forms.Label
    Friend WithEvents txtAppunto As System.Windows.Forms.TextBox
    Friend WithEvents lblFattiTitolo As System.Windows.Forms.Label
    Friend WithEvents lvwFatti As System.Windows.Forms.ListView
    Friend WithEvents colFatto As System.Windows.Forms.ColumnHeader
    Friend WithEvents btnConferma As System.Windows.Forms.Button
    Friend WithEvents btnAnnulla As System.Windows.Forms.Button

End Class
