<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraConfermaCritica
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
        Me.lblRichiesta = New System.Windows.Forms.Label()
        Me.txtParola = New System.Windows.Forms.TextBox()
        Me.btnAzione = New System.Windows.Forms.Button()
        Me.btnAnnulla = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblTitolo
        '
        Me.lblTitolo.AutoSize = True
        Me.lblTitolo.Location = New System.Drawing.Point(14, 14)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(200, 30)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "Titolo"
        '
        'lblSpiegazione
        '
        Me.lblSpiegazione.AutoSize = True
        Me.lblSpiegazione.Location = New System.Drawing.Point(14, 56)
        Me.lblSpiegazione.MaximumSize = New System.Drawing.Size(592, 0)
        Me.lblSpiegazione.Name = "lblSpiegazione"
        Me.lblSpiegazione.Size = New System.Drawing.Size(592, 60)
        Me.lblSpiegazione.TabIndex = 1
        Me.lblSpiegazione.Text = "Spiegazione"
        '
        'lblRichiesta
        '
        Me.lblRichiesta.AutoSize = True
        Me.lblRichiesta.Location = New System.Drawing.Point(14, 130)
        Me.lblRichiesta.MaximumSize = New System.Drawing.Size(592, 0)
        Me.lblRichiesta.Name = "lblRichiesta"
        Me.lblRichiesta.Size = New System.Drawing.Size(592, 15)
        Me.lblRichiesta.TabIndex = 2
        Me.lblRichiesta.Text = "Richiesta"
        '
        'txtParola
        '
        Me.txtParola.Location = New System.Drawing.Point(14, 153)
        Me.txtParola.Name = "txtParola"
        Me.txtParola.Size = New System.Drawing.Size(260, 23)
        Me.txtParola.TabIndex = 3
        '
        'btnAzione
        '
        Me.btnAzione.Enabled = False
        Me.btnAzione.Location = New System.Drawing.Point(320, 190)
        Me.btnAzione.Name = "btnAzione"
        Me.btnAzione.Size = New System.Drawing.Size(160, 32)
        Me.btnAzione.TabIndex = 4
        Me.btnAzione.Text = "Elimina"
        '
        'btnAnnulla
        '
        Me.btnAnnulla.Location = New System.Drawing.Point(492, 190)
        Me.btnAnnulla.Name = "btnAnnulla"
        Me.btnAnnulla.Size = New System.Drawing.Size(110, 32)
        Me.btnAnnulla.TabIndex = 5
        Me.btnAnnulla.Text = "Annulla"
        '
        'FinestraConfermaCritica
        '
        Me.ClientSize = New System.Drawing.Size(620, 236)
        Me.Controls.Add(Me.lblTitolo)
        Me.Controls.Add(Me.lblSpiegazione)
        Me.Controls.Add(Me.lblRichiesta)
        Me.Controls.Add(Me.txtParola)
        Me.Controls.Add(Me.btnAzione)
        Me.Controls.Add(Me.btnAnnulla)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraConfermaCritica"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "TrovaLavoro"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSpiegazione As System.Windows.Forms.Label
    Friend WithEvents lblRichiesta As System.Windows.Forms.Label
    Friend WithEvents txtParola As System.Windows.Forms.TextBox
    Friend WithEvents btnAzione As System.Windows.Forms.Button
    Friend WithEvents btnAnnulla As System.Windows.Forms.Button

End Class
