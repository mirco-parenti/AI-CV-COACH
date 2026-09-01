<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraChiaveApi
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
        Me.lblSalvata = New System.Windows.Forms.Label()
        Me.lblRichiesta = New System.Windows.Forms.Label()
        Me.txtChiave = New System.Windows.Forms.TextBox()
        Me.chkMostra = New System.Windows.Forms.CheckBox()
        Me.lblForma = New System.Windows.Forms.Label()
        Me.lblEsitoProva = New System.Windows.Forms.Label()
        Me.btnProva = New System.Windows.Forms.Button()
        Me.btnSalva = New System.Windows.Forms.Button()
        Me.btnNonAdesso = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblTitolo
        '
        Me.lblTitolo.AutoSize = True
        Me.lblTitolo.Location = New System.Drawing.Point(14, 14)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(280, 30)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "La chiave API di Anthropic"
        '
        'lblSpiegazione
        '
        Me.lblSpiegazione.AutoSize = True
        Me.lblSpiegazione.Location = New System.Drawing.Point(14, 56)
        Me.lblSpiegazione.MaximumSize = New System.Drawing.Size(592, 0)
        Me.lblSpiegazione.Name = "lblSpiegazione"
        Me.lblSpiegazione.Size = New System.Drawing.Size(592, 120)
        Me.lblSpiegazione.TabIndex = 1
        Me.lblSpiegazione.Text = "Spiegazione"
        '
        'lblSalvata
        '
        Me.lblSalvata.AutoSize = True
        Me.lblSalvata.Location = New System.Drawing.Point(14, 186)
        Me.lblSalvata.MaximumSize = New System.Drawing.Size(592, 0)
        Me.lblSalvata.Name = "lblSalvata"
        Me.lblSalvata.Size = New System.Drawing.Size(592, 15)
        Me.lblSalvata.TabIndex = 2
        Me.lblSalvata.Text = "Chiave salvata"
        Me.lblSalvata.Visible = False
        '
        'lblRichiesta
        '
        Me.lblRichiesta.AutoSize = True
        Me.lblRichiesta.Location = New System.Drawing.Point(14, 209)
        Me.lblRichiesta.MaximumSize = New System.Drawing.Size(592, 0)
        Me.lblRichiesta.Name = "lblRichiesta"
        Me.lblRichiesta.Size = New System.Drawing.Size(592, 15)
        Me.lblRichiesta.TabIndex = 3
        Me.lblRichiesta.Text = "Richiesta"
        '
        'txtChiave
        '
        Me.txtChiave.Location = New System.Drawing.Point(14, 232)
        Me.txtChiave.Name = "txtChiave"
        Me.txtChiave.Size = New System.Drawing.Size(420, 23)
        Me.txtChiave.TabIndex = 4
        Me.txtChiave.UseSystemPasswordChar = True
        '
        'chkMostra
        '
        Me.chkMostra.AutoSize = True
        Me.chkMostra.Location = New System.Drawing.Point(446, 234)
        Me.chkMostra.Name = "chkMostra"
        Me.chkMostra.Size = New System.Drawing.Size(140, 19)
        Me.chkMostra.TabIndex = 5
        Me.chkMostra.Text = "Mostra la chiave"
        '
        'lblForma
        '
        Me.lblForma.AutoSize = True
        Me.lblForma.Location = New System.Drawing.Point(14, 263)
        Me.lblForma.MaximumSize = New System.Drawing.Size(592, 0)
        Me.lblForma.Name = "lblForma"
        Me.lblForma.Size = New System.Drawing.Size(592, 15)
        Me.lblForma.TabIndex = 6
        Me.lblForma.Text = "Forma"
        Me.lblForma.Visible = False
        '
        'lblEsitoProva
        '
        Me.lblEsitoProva.AutoSize = True
        Me.lblEsitoProva.Location = New System.Drawing.Point(14, 286)
        Me.lblEsitoProva.Name = "lblEsitoProva"
        Me.lblEsitoProva.Size = New System.Drawing.Size(592, 15)
        Me.lblEsitoProva.TabIndex = 7
        Me.lblEsitoProva.Text = ""
        Me.lblEsitoProva.Visible = False
        '
        'btnProva
        '
        Me.btnProva.Location = New System.Drawing.Point(150, 324)
        Me.btnProva.Name = "btnProva"
        Me.btnProva.Size = StileApp.BottoneLargo
        Me.btnProva.TabIndex = 8
        Me.btnProva.Text = "Prova la chiave"
        '
        'btnSalva
        '
        Me.btnSalva.Enabled = False
        Me.btnSalva.Location = New System.Drawing.Point(290, 300)
        Me.btnSalva.Name = "btnSalva"
        Me.btnSalva.Size = StileApp.BottoneLargo
        Me.btnSalva.TabIndex = 9
        Me.btnSalva.Text = "Salva la chiave"
        '
        'btnNonAdesso
        '
        Me.btnNonAdesso.Location = New System.Drawing.Point(492, 300)
        Me.btnNonAdesso.Name = "btnNonAdesso"
        Me.btnNonAdesso.Size = StileApp.BottoneStandard
        Me.btnNonAdesso.TabIndex = 10
        Me.btnNonAdesso.Text = "Non adesso"
        '
        'FinestraChiaveApi
        '
        Me.ClientSize = New System.Drawing.Size(620, 346)
        Me.Controls.Add(Me.lblTitolo)
        Me.Controls.Add(Me.lblSpiegazione)
        Me.Controls.Add(Me.lblSalvata)
        Me.Controls.Add(Me.lblRichiesta)
        Me.Controls.Add(Me.txtChiave)
        Me.Controls.Add(Me.chkMostra)
        Me.Controls.Add(Me.lblForma)
        Me.Controls.Add(Me.lblEsitoProva)
        Me.Controls.Add(Me.btnProva)
        Me.Controls.Add(Me.btnSalva)
        Me.Controls.Add(Me.btnNonAdesso)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraChiaveApi"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "TrovaLavoro"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSpiegazione As System.Windows.Forms.Label
    Friend WithEvents lblSalvata As System.Windows.Forms.Label
    Friend WithEvents lblRichiesta As System.Windows.Forms.Label
    Friend WithEvents txtChiave As System.Windows.Forms.TextBox
    Friend WithEvents chkMostra As System.Windows.Forms.CheckBox
    Friend WithEvents lblForma As System.Windows.Forms.Label
    Friend WithEvents lblEsitoProva As System.Windows.Forms.Label
    Friend WithEvents btnProva As System.Windows.Forms.Button
    Friend WithEvents btnSalva As System.Windows.Forms.Button
    Friend WithEvents btnNonAdesso As System.Windows.Forms.Button

End Class
