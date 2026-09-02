<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraDocumenti
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
        Me.lblCartella = New System.Windows.Forms.Label()
        Me.lvwDocumenti = New System.Windows.Forms.ListView()
        Me.colDocumento = New System.Windows.Forms.ColumnHeader()
        Me.colCategoria = New System.Windows.Forms.ColumnHeader()
        Me.colMotivo = New System.Windows.Forms.ColumnHeader()
        Me.lblScelta = New System.Windows.Forms.Label()
        Me.cmbCategoria = New System.Windows.Forms.ComboBox()
        Me.btnRileggi = New System.Windows.Forms.Button()
        Me.btnCambiaCartella = New System.Windows.Forms.Button()
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
        Me.lblTitolo.Text = "I documenti della tua cartella"
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
        'lblCartella
        '
        Me.lblCartella.AutoSize = True
        Me.lblCartella.Location = New System.Drawing.Point(14, 109)
        Me.lblCartella.MaximumSize = New System.Drawing.Size(772, 0)
        Me.lblCartella.Name = "lblCartella"
        Me.lblCartella.Size = New System.Drawing.Size(772, 15)
        Me.lblCartella.TabIndex = 2
        Me.lblCartella.Text = "Cartella"
        '
        'lvwDocumenti
        '
        Me.lvwDocumenti.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colDocumento, Me.colCategoria, Me.colMotivo})
        Me.lvwDocumenti.FullRowSelect = True
        Me.lvwDocumenti.HideSelection = False
        Me.lvwDocumenti.Location = New System.Drawing.Point(14, 132)
        Me.lvwDocumenti.MultiSelect = False
        Me.lvwDocumenti.Name = "lvwDocumenti"
        Me.lvwDocumenti.Size = New System.Drawing.Size(772, 300)
        Me.lvwDocumenti.TabIndex = 3
        Me.lvwDocumenti.UseCompatibleStateImageBehavior = False
        Me.lvwDocumenti.View = System.Windows.Forms.View.Details
        '
        'colDocumento
        '
        Me.colDocumento.Text = "Documento"
        Me.colDocumento.Width = 300
        '
        'colCategoria
        '
        Me.colCategoria.Text = "È"
        Me.colCategoria.Width = 110
        '
        'colMotivo
        '
        Me.colMotivo.Text = "Perché"
        Me.colMotivo.Width = 340
        '
        'lblScelta
        '
        Me.lblScelta.AutoSize = True
        Me.lblScelta.Location = New System.Drawing.Point(14, 444)
        Me.lblScelta.Name = "lblScelta"
        Me.lblScelta.Size = New System.Drawing.Size(150, 15)
        Me.lblScelta.TabIndex = 4
        Me.lblScelta.Text = "Il documento scelto è:"
        '
        'cmbCategoria
        '
        Me.cmbCategoria.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbCategoria.Enabled = False
        Me.cmbCategoria.Location = New System.Drawing.Point(174, 440)
        Me.cmbCategoria.Name = "cmbCategoria"
        Me.cmbCategoria.Size = New System.Drawing.Size(180, 23)
        Me.cmbCategoria.TabIndex = 5
        '
        'btnRileggi
        '
        Me.btnRileggi.Location = New System.Drawing.Point(14, 477)
        Me.btnRileggi.Name = "btnRileggi"
        Me.btnRileggi.Size = StileApp.BottoneMoltoLargo
        Me.btnRileggi.TabIndex = 6
        Me.btnRileggi.Text = "Fai rileggere la cartella"
        '
        'btnCambiaCartella
        '
        Me.btnCambiaCartella.Location = New System.Drawing.Point(266, 477)
        Me.btnCambiaCartella.Name = "btnCambiaCartella"
        Me.btnCambiaCartella.Size = StileApp.BottoneLargo
        Me.btnCambiaCartella.TabIndex = 7
        Me.btnCambiaCartella.Text = "Cambia cartella…"
        '
        'btnConferma
        '
        Me.btnConferma.Location = New System.Drawing.Point(546, 477)
        Me.btnConferma.Name = "btnConferma"
        Me.btnConferma.Size = StileApp.BottoneMedio
        Me.btnConferma.TabIndex = 8
        Me.btnConferma.Text = "Conferma"
        '
        'btnAnnulla
        '
        Me.btnAnnulla.Location = New System.Drawing.Point(688, 477)
        Me.btnAnnulla.Name = "btnAnnulla"
        Me.btnAnnulla.Size = StileApp.BottoneStandard
        Me.btnAnnulla.TabIndex = 9
        Me.btnAnnulla.Text = "Annulla"
        '
        'FinestraDocumenti
        '
        Me.ClientSize = New System.Drawing.Size(800, 523)
        Me.Controls.Add(Me.lblTitolo)
        Me.Controls.Add(Me.lblSpiegazione)
        Me.Controls.Add(Me.lblCartella)
        Me.Controls.Add(Me.lvwDocumenti)
        Me.Controls.Add(Me.lblScelta)
        Me.Controls.Add(Me.cmbCategoria)
        Me.Controls.Add(Me.btnRileggi)
        Me.Controls.Add(Me.btnCambiaCartella)
        Me.Controls.Add(Me.btnConferma)
        Me.Controls.Add(Me.btnAnnulla)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraDocumenti"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "TrovaLavoro"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSpiegazione As System.Windows.Forms.Label
    Friend WithEvents lblCartella As System.Windows.Forms.Label
    Friend WithEvents lvwDocumenti As System.Windows.Forms.ListView
    Friend WithEvents colDocumento As System.Windows.Forms.ColumnHeader
    Friend WithEvents colCategoria As System.Windows.Forms.ColumnHeader
    Friend WithEvents colMotivo As System.Windows.Forms.ColumnHeader
    Friend WithEvents lblScelta As System.Windows.Forms.Label
    Friend WithEvents cmbCategoria As System.Windows.Forms.ComboBox
    Friend WithEvents btnRileggi As System.Windows.Forms.Button
    Friend WithEvents btnCambiaCartella As System.Windows.Forms.Button
    Friend WithEvents btnConferma As System.Windows.Forms.Button
    Friend WithEvents btnAnnulla As System.Windows.Forms.Button

End Class
