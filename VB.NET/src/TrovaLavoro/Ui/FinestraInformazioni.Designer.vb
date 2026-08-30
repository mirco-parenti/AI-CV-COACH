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
        Me.lblSorgente = New System.Windows.Forms.Label()
        Me.btnCopiaDiagnostica = New System.Windows.Forms.Button()
        Me.lblEsitoVersione = New System.Windows.Forms.Label()
        Me.btnControllaVersione = New System.Windows.Forms.Button()
        Me.btnComeFunziona = New System.Windows.Forms.Button()
        Me.btnChiudi = New System.Windows.Forms.Button()
        CType(Me.picMarchio, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'picMarchio
        '
        Me.picMarchio.BackColor = StileApp.FondoMarchio
        Me.picMarchio.Location = New System.Drawing.Point(14, 14)
        Me.picMarchio.Name = "picMarchio"
        Me.picMarchio.Size = New System.Drawing.Size(520, 456)
        Me.picMarchio.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.picMarchio.TabIndex = 0
        Me.picMarchio.TabStop = False
        '
        'lblVersione
        '
        Me.lblVersione.AutoSize = True
        Me.lblVersione.Location = New System.Drawing.Point(14, 482)
        Me.lblVersione.Name = "lblVersione"
        Me.lblVersione.Size = New System.Drawing.Size(300, 15)
        Me.lblVersione.TabIndex = 1
        Me.lblVersione.Text = "Ver."
        '
        'lblCopyright
        '
        Me.lblCopyright.AutoSize = True
        Me.lblCopyright.Location = New System.Drawing.Point(14, 503)
        Me.lblCopyright.Name = "lblCopyright"
        Me.lblCopyright.Size = New System.Drawing.Size(300, 15)
        Me.lblCopyright.TabIndex = 2
        Me.lblCopyright.Text = "Copyright"
        '
        'lblSorgente
        '
        Me.lblSorgente.AutoSize = True
        Me.lblSorgente.Location = New System.Drawing.Point(14, 524)
        Me.lblSorgente.Name = "lblSorgente"
        Me.lblSorgente.Size = New System.Drawing.Size(300, 15)
        Me.lblSorgente.TabIndex = 3
        Me.lblSorgente.Text = "Codice sorgente"
        '
        'btnCopiaDiagnostica
        '
        Me.btnCopiaDiagnostica.Location = New System.Drawing.Point(172, 551)
        Me.btnCopiaDiagnostica.Name = "btnCopiaDiagnostica"
        Me.btnCopiaDiagnostica.Size = New System.Drawing.Size(134, 32)
        Me.btnCopiaDiagnostica.TabIndex = 4
        Me.btnCopiaDiagnostica.Text = "Copia diagnostica"
        '
        'lblEsitoVersione
        '
        Me.lblEsitoVersione.AutoSize = False
        Me.lblEsitoVersione.Location = New System.Drawing.Point(14, 593)
        Me.lblEsitoVersione.Name = "lblEsitoVersione"
        Me.lblEsitoVersione.Size = New System.Drawing.Size(520, 36)
        Me.lblEsitoVersione.TabIndex = 5
        Me.lblEsitoVersione.Text = ""
        '
        'btnControllaVersione
        '
        Me.btnControllaVersione.Location = New System.Drawing.Point(14, 551)
        Me.btnControllaVersione.Name = "btnControllaVersione"
        Me.btnControllaVersione.Size = New System.Drawing.Size(150, 32)
        Me.btnControllaVersione.TabIndex = 6
        Me.btnControllaVersione.Text = "Cerca aggiornamenti"
        '
        'btnComeFunziona
        '
        Me.btnComeFunziona.Location = New System.Drawing.Point(14, 637)
        Me.btnComeFunziona.Name = "btnComeFunziona"
        Me.btnComeFunziona.Size = New System.Drawing.Size(260, 32)
        Me.btnComeFunziona.TabIndex = 7
        Me.btnComeFunziona.Text = "Come funziona, e cosa esce dal tuo PC"
        '
        'btnChiudi
        '
        Me.btnChiudi.Location = New System.Drawing.Point(424, 637)
        Me.btnChiudi.Name = "btnChiudi"
        Me.btnChiudi.Size = New System.Drawing.Size(110, 32)
        Me.btnChiudi.TabIndex = 5
        Me.btnChiudi.Text = "Chiudi"
        '
        'FinestraInformazioni
        '
        Me.ClientSize = New System.Drawing.Size(548, 683)
        Me.Controls.Add(Me.picMarchio)
        Me.Controls.Add(Me.lblVersione)
        Me.Controls.Add(Me.lblCopyright)
        Me.Controls.Add(Me.lblSorgente)
        Me.Controls.Add(Me.btnCopiaDiagnostica)
        Me.Controls.Add(Me.lblEsitoVersione)
        Me.Controls.Add(Me.btnControllaVersione)
        Me.Controls.Add(Me.btnComeFunziona)
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
    Friend WithEvents lblSorgente As System.Windows.Forms.Label
    Friend WithEvents lblEsitoVersione As System.Windows.Forms.Label
    Friend WithEvents btnControllaVersione As System.Windows.Forms.Button
    Friend WithEvents btnComeFunziona As System.Windows.Forms.Button
    Friend WithEvents btnCopiaDiagnostica As System.Windows.Forms.Button
    Friend WithEvents btnChiudi As System.Windows.Forms.Button

End Class
