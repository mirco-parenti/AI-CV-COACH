<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraBackup
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
        Me.lblSezioneEsporta = New System.Windows.Forms.Label()
        Me.lblCosaCE = New System.Windows.Forms.Label()
        Me.rdoSoloProfilo = New System.Windows.Forms.RadioButton()
        Me.rdoTutto = New System.Windows.Forms.RadioButton()
        Me.btnEsporta = New System.Windows.Forms.Button()
        Me.lblSezioneRipristina = New System.Windows.Forms.Label()
        Me.lblComeSiRipristina = New System.Windows.Forms.Label()
        Me.btnScegli = New System.Windows.Forms.Button()
        Me.txtAnteprima = New System.Windows.Forms.TextBox()
        Me.btnRipristina = New System.Windows.Forms.Button()
        Me.lblStato = New System.Windows.Forms.Label()
        Me.btnChiudi = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblTitolo
        '
        Me.lblTitolo.AutoSize = True
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(240, 30)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "Backup e ripristino"
        '
        'lblSpiegazione
        '
        Me.lblSpiegazione.AutoSize = True
        Me.lblSpiegazione.Name = "lblSpiegazione"
        Me.lblSpiegazione.TabIndex = 1
        Me.lblSpiegazione.Text = "Spiegazione"
        '
        'lblSezioneEsporta
        '
        Me.lblSezioneEsporta.AutoSize = True
        Me.lblSezioneEsporta.Name = "lblSezioneEsporta"
        Me.lblSezioneEsporta.TabIndex = 2
        Me.lblSezioneEsporta.Text = "Esporta"
        '
        'lblCosaCE
        '
        Me.lblCosaCE.AutoSize = True
        Me.lblCosaCE.Name = "lblCosaCE"
        Me.lblCosaCE.TabIndex = 3
        Me.lblCosaCE.Text = "Cosa c'è adesso"
        '
        'rdoSoloProfilo
        '
        Me.rdoSoloProfilo.AutoSize = True
        Me.rdoSoloProfilo.Checked = True
        Me.rdoSoloProfilo.Name = "rdoSoloProfilo"
        Me.rdoSoloProfilo.TabIndex = 4
        Me.rdoSoloProfilo.TabStop = True
        Me.rdoSoloProfilo.Text = "Solo il profilo, con il suo storico e il CV base"
        Me.rdoSoloProfilo.UseVisualStyleBackColor = True
        '
        'rdoTutto
        '
        Me.rdoTutto.AutoSize = True
        Me.rdoTutto.Name = "rdoTutto"
        Me.rdoTutto.TabIndex = 5
        Me.rdoTutto.Text = "Tutto: il profilo, il registro e le candidature"
        Me.rdoTutto.UseVisualStyleBackColor = True
        '
        'btnEsporta
        '
        Me.btnEsporta.Name = "btnEsporta"
        Me.btnEsporta.Size = New System.Drawing.Size(150, 32)
        Me.btnEsporta.TabIndex = 6
        Me.btnEsporta.Text = "Esporta…"
        '
        'lblSezioneRipristina
        '
        Me.lblSezioneRipristina.AutoSize = True
        Me.lblSezioneRipristina.Name = "lblSezioneRipristina"
        Me.lblSezioneRipristina.TabIndex = 7
        Me.lblSezioneRipristina.Text = "Ripristina"
        '
        'lblComeSiRipristina
        '
        Me.lblComeSiRipristina.AutoSize = True
        Me.lblComeSiRipristina.Name = "lblComeSiRipristina"
        Me.lblComeSiRipristina.TabIndex = 8
        Me.lblComeSiRipristina.Text = "Come si ripristina"
        '
        'btnScegli
        '
        Me.btnScegli.Name = "btnScegli"
        Me.btnScegli.Size = New System.Drawing.Size(210, 32)
        Me.btnScegli.TabIndex = 9
        Me.btnScegli.Text = "Scegli un file di backup…"
        '
        'txtAnteprima
        '
        Me.txtAnteprima.Multiline = True
        Me.txtAnteprima.Name = "txtAnteprima"
        Me.txtAnteprima.ReadOnly = True
        Me.txtAnteprima.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtAnteprima.Size = New System.Drawing.Size(632, 150)
        Me.txtAnteprima.TabIndex = 10
        '
        'btnRipristina
        '
        Me.btnRipristina.Enabled = False
        Me.btnRipristina.Name = "btnRipristina"
        Me.btnRipristina.Size = New System.Drawing.Size(150, 32)
        Me.btnRipristina.TabIndex = 11
        Me.btnRipristina.Text = "Ripristina"
        '
        'lblStato
        '
        Me.lblStato.AutoSize = True
        Me.lblStato.Name = "lblStato"
        Me.lblStato.TabIndex = 12
        Me.lblStato.Text = ""
        '
        'btnChiudi
        '
        Me.btnChiudi.Name = "btnChiudi"
        Me.btnChiudi.Size = New System.Drawing.Size(110, 32)
        Me.btnChiudi.TabIndex = 13
        Me.btnChiudi.Text = "Chiudi"
        '
        'FinestraBackup
        '
        Me.ClientSize = New System.Drawing.Size(660, 620)
        Me.Controls.Add(Me.lblTitolo)
        Me.Controls.Add(Me.lblSpiegazione)
        Me.Controls.Add(Me.lblSezioneEsporta)
        Me.Controls.Add(Me.lblCosaCE)
        Me.Controls.Add(Me.rdoSoloProfilo)
        Me.Controls.Add(Me.rdoTutto)
        Me.Controls.Add(Me.btnEsporta)
        Me.Controls.Add(Me.lblSezioneRipristina)
        Me.Controls.Add(Me.lblComeSiRipristina)
        Me.Controls.Add(Me.btnScegli)
        Me.Controls.Add(Me.txtAnteprima)
        Me.Controls.Add(Me.btnRipristina)
        Me.Controls.Add(Me.lblStato)
        Me.Controls.Add(Me.btnChiudi)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraBackup"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "TrovaLavoro"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSpiegazione As System.Windows.Forms.Label
    Friend WithEvents lblSezioneEsporta As System.Windows.Forms.Label
    Friend WithEvents lblCosaCE As System.Windows.Forms.Label
    Friend WithEvents rdoSoloProfilo As System.Windows.Forms.RadioButton
    Friend WithEvents rdoTutto As System.Windows.Forms.RadioButton
    Friend WithEvents btnEsporta As System.Windows.Forms.Button
    Friend WithEvents lblSezioneRipristina As System.Windows.Forms.Label
    Friend WithEvents lblComeSiRipristina As System.Windows.Forms.Label
    Friend WithEvents btnScegli As System.Windows.Forms.Button
    Friend WithEvents txtAnteprima As System.Windows.Forms.TextBox
    Friend WithEvents btnRipristina As System.Windows.Forms.Button
    Friend WithEvents lblStato As System.Windows.Forms.Label
    Friend WithEvents btnChiudi As System.Windows.Forms.Button

End Class
