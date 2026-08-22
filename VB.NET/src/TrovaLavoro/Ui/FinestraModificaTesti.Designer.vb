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
        Me.lblModifica = New System.Windows.Forms.Label()
        Me.txtTesto = New System.Windows.Forms.TextBox()
        Me.btnRipristina = New System.Windows.Forms.Button()
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
        Me.lblSpiegazione.MaximumSize = New System.Drawing.Size(772, 0)
        Me.lblSpiegazione.Name = "lblSpiegazione"
        Me.lblSpiegazione.Size = New System.Drawing.Size(772, 45)
        Me.lblSpiegazione.TabIndex = 1
        Me.lblSpiegazione.Text = "Spiegazione"
        '
        'lvwCampi
        '
        Me.lvwCampi.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colCampo, Me.colTesto, Me.colSegno})
        Me.lvwCampi.FullRowSelect = True
        Me.lvwCampi.HideSelection = False
        Me.lvwCampi.Location = New System.Drawing.Point(14, 120)
        Me.lvwCampi.MultiSelect = False
        Me.lvwCampi.Name = "lvwCampi"
        Me.lvwCampi.Size = New System.Drawing.Size(772, 240)
        Me.lvwCampi.TabIndex = 2
        Me.lvwCampi.UseCompatibleStateImageBehavior = False
        Me.lvwCampi.View = System.Windows.Forms.View.Details
        '
        'colCampo
        '
        Me.colCampo.Text = "Campo"
        Me.colCampo.Width = 180
        '
        'colTesto
        '
        Me.colTesto.Text = "Il testo"
        Me.colTesto.Width = 528
        '
        'colSegno
        '
        Me.colSegno.Text = "Riscritto"
        Me.colSegno.Width = 60
        '
        'lblModifica
        '
        Me.lblModifica.AutoSize = True
        Me.lblModifica.Location = New System.Drawing.Point(14, 372)
        Me.lblModifica.Name = "lblModifica"
        Me.lblModifica.Size = New System.Drawing.Size(400, 15)
        Me.lblModifica.TabIndex = 3
        Me.lblModifica.Text = "Il testo scelto (puoi riscriverlo):"
        '
        'txtTesto
        '
        Me.txtTesto.Enabled = False
        Me.txtTesto.Location = New System.Drawing.Point(14, 394)
        Me.txtTesto.Multiline = True
        Me.txtTesto.Name = "txtTesto"
        Me.txtTesto.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtTesto.Size = New System.Drawing.Size(772, 110)
        Me.txtTesto.TabIndex = 4
        '
        'btnRipristina
        '
        Me.btnRipristina.Location = New System.Drawing.Point(14, 518)
        Me.btnRipristina.Name = "btnRipristina"
        Me.btnRipristina.Size = New System.Drawing.Size(280, 32)
        Me.btnRipristina.TabIndex = 5
        Me.btnRipristina.Text = "Ripristina il testo non rifinito"
        '
        'btnSalva
        '
        Me.btnSalva.Location = New System.Drawing.Point(536, 518)
        Me.btnSalva.Name = "btnSalva"
        Me.btnSalva.Size = New System.Drawing.Size(140, 32)
        Me.btnSalva.TabIndex = 6
        Me.btnSalva.Text = "Salva"
        '
        'btnAnnulla
        '
        Me.btnAnnulla.Location = New System.Drawing.Point(688, 518)
        Me.btnAnnulla.Name = "btnAnnulla"
        Me.btnAnnulla.Size = New System.Drawing.Size(110, 32)
        Me.btnAnnulla.TabIndex = 7
        Me.btnAnnulla.Text = "Annulla"
        '
        'FinestraModificaTesti
        '
        Me.ClientSize = New System.Drawing.Size(800, 566)
        Me.Controls.Add(Me.lblTitolo)
        Me.Controls.Add(Me.lblSpiegazione)
        Me.Controls.Add(Me.lvwCampi)
        Me.Controls.Add(Me.lblModifica)
        Me.Controls.Add(Me.txtTesto)
        Me.Controls.Add(Me.btnRipristina)
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
    Friend WithEvents lblModifica As System.Windows.Forms.Label
    Friend WithEvents txtTesto As System.Windows.Forms.TextBox
    Friend WithEvents btnRipristina As System.Windows.Forms.Button
    Friend WithEvents btnSalva As System.Windows.Forms.Button
    Friend WithEvents btnAnnulla As System.Windows.Forms.Button

End Class
