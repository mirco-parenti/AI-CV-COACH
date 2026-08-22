<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraInformazioni
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
        Me.picMarchio = New System.Windows.Forms.PictureBox()
        Me.lblVersione = New System.Windows.Forms.Label()
        Me.lblCopyright = New System.Windows.Forms.Label()
        Me.btnChiudi = New System.Windows.Forms.Button()
        CType(Me.picMarchio, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'picMarchio
        '
        Me.picMarchio.BackColor = StileApp.FondoMarchio
        Me.picMarchio.Location = New System.Drawing.Point(14, 14)
        Me.picMarchio.Name = "picMarchio"
        Me.picMarchio.Size = New System.Drawing.Size(520, 421)
        Me.picMarchio.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.picMarchio.TabIndex = 0
        Me.picMarchio.TabStop = False
        '
        'lblVersione
        '
        Me.lblVersione.AutoSize = True
        Me.lblVersione.Location = New System.Drawing.Point(14, 449)
        Me.lblVersione.Name = "lblVersione"
        Me.lblVersione.Size = New System.Drawing.Size(300, 15)
        Me.lblVersione.TabIndex = 1
        Me.lblVersione.Text = "Ver."
        '
        'lblCopyright
        '
        Me.lblCopyright.AutoSize = True
        Me.lblCopyright.Location = New System.Drawing.Point(14, 473)
        Me.lblCopyright.Name = "lblCopyright"
        Me.lblCopyright.Size = New System.Drawing.Size(300, 15)
        Me.lblCopyright.TabIndex = 2
        Me.lblCopyright.Text = "Copyright"
        '
        'btnChiudi
        '
        Me.btnChiudi.Location = New System.Drawing.Point(424, 455)
        Me.btnChiudi.Name = "btnChiudi"
        Me.btnChiudi.Size = New System.Drawing.Size(110, 32)
        Me.btnChiudi.TabIndex = 3
        Me.btnChiudi.Text = "Chiudi"
        '
        'FinestraInformazioni
        '
        Me.ClientSize = New System.Drawing.Size(548, 501)
        Me.Controls.Add(Me.picMarchio)
        Me.Controls.Add(Me.lblVersione)
        Me.Controls.Add(Me.lblCopyright)
        Me.Controls.Add(Me.btnChiudi)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraInformazioni"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Informazioni su TrovaLavoro"
        CType(Me.picMarchio, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents picMarchio As System.Windows.Forms.PictureBox
    Friend WithEvents lblVersione As System.Windows.Forms.Label
    Friend WithEvents lblCopyright As System.Windows.Forms.Label
    Friend WithEvents btnChiudi As System.Windows.Forms.Button

End Class
