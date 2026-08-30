<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraAvvio
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
        Me.picSchermata = New System.Windows.Forms.PictureBox()
        CType(Me.picSchermata, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'picSchermata
        '
        Me.picSchermata.Dock = System.Windows.Forms.DockStyle.Fill
        Me.picSchermata.Name = "picSchermata"
        Me.picSchermata.Size = New System.Drawing.Size(800, 702)
        Me.picSchermata.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.picSchermata.TabIndex = 0
        Me.picSchermata.TabStop = False
        '
        'FinestraAvvio
        '
        Me.BackColor = StileApp.FondoMarchio
        Me.ClientSize = New System.Drawing.Size(800, 702)
        Me.Controls.Add(Me.picSchermata)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraAvvio"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "TrovaLavoro"
        Me.TopMost = True
        CType(Me.picSchermata, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents picSchermata As System.Windows.Forms.PictureBox

End Class
