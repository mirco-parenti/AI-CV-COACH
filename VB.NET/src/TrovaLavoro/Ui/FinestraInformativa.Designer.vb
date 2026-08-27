' Disegno della finestra dell'informativa. Qui ci sono solo il titolo e il bottone: i
' capitoli nascono a runtime da FinestraInformativa.Voci(), perché il testo deve stare in
' un posto solo — quello che il banco legge — e non duplicato in un file di disegno.
<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraInformativa
    Inherits System.Windows.Forms.Form

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblTitolo = New System.Windows.Forms.Label()
        Me.btnChiudi = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblTitolo
        '
        Me.lblTitolo.AutoSize = True
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "Come funziona"
        '
        'btnChiudi
        '
        Me.btnChiudi.Name = "btnChiudi"
        Me.btnChiudi.Size = New System.Drawing.Size(110, 32)
        Me.btnChiudi.TabIndex = 1
        Me.btnChiudi.Text = "Ho capito"
        '
        'FinestraInformativa
        '
        Me.ClientSize = New System.Drawing.Size(660, 520)
        Me.Controls.Add(Me.lblTitolo)
        Me.Controls.Add(Me.btnChiudi)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraInformativa"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Come funziona TrovaLavoro"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents btnChiudi As System.Windows.Forms.Button

End Class
