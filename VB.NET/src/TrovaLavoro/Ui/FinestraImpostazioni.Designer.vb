<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FinestraImpostazioni
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
        Me.lblSezioneChiave = New System.Windows.Forms.Label()
        Me.lblStatoChiave = New System.Windows.Forms.Label()
        Me.btnCambiaChiave = New System.Windows.Forms.Button()
        Me.lblSezioneDocumenti = New System.Windows.Forms.Label()
        Me.lblLingua = New System.Windows.Forms.Label()
        Me.cmbLingua = New System.Windows.Forms.ComboBox()
        Me.chkRifinitura = New System.Windows.Forms.CheckBox()
        Me.lblRifinituraNota = New System.Windows.Forms.Label()
        Me.lblSezioneCartelle = New System.Windows.Forms.Label()
        Me.lblCartellaDati = New System.Windows.Forms.Label()
        Me.btnApriCartellaDati = New System.Windows.Forms.Button()
        Me.lblCartellaDocumenti = New System.Windows.Forms.Label()
        Me.btnGestisciDocumenti = New System.Windows.Forms.Button()
        Me.lblSezioneMotore = New System.Windows.Forms.Label()
        Me.lblModelli = New System.Windows.Forms.Label()
        Me.lblPool = New System.Windows.Forms.Label()
        Me.btnApriModelli = New System.Windows.Forms.Button()
        Me.lblSezioneDati = New System.Windows.Forms.Label()
        Me.btnBackup = New System.Windows.Forms.Button()
        Me.btnSvuotaNavigazione = New System.Windows.Forms.Button()
        Me.btnEliminaTutto = New System.Windows.Forms.Button()
        Me.lblStato = New System.Windows.Forms.Label()
        Me.btnChiudi = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'lblTitolo
        '
        Me.lblTitolo.AutoSize = True
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(200, 30)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "Impostazioni"
        '
        'lblSpiegazione
        '
        Me.lblSpiegazione.AutoSize = True
        Me.lblSpiegazione.Name = "lblSpiegazione"
        Me.lblSpiegazione.TabIndex = 1
        Me.lblSpiegazione.Text = "Spiegazione"
        '
        'lblSezioneChiave
        '
        Me.lblSezioneChiave.AutoSize = True
        Me.lblSezioneChiave.Name = "lblSezioneChiave"
        Me.lblSezioneChiave.TabIndex = 2
        Me.lblSezioneChiave.Text = "Chiave API"
        '
        'lblStatoChiave
        '
        Me.lblStatoChiave.AutoSize = True
        Me.lblStatoChiave.Name = "lblStatoChiave"
        Me.lblStatoChiave.TabIndex = 3
        Me.lblStatoChiave.Text = ""
        '
        'btnCambiaChiave
        '
        Me.btnCambiaChiave.Name = "btnCambiaChiave"
        Me.btnCambiaChiave.Size = New System.Drawing.Size(190, 32)
        Me.btnCambiaChiave.TabIndex = 4
        Me.btnCambiaChiave.Text = "Cambia la chiave…"
        '
        'lblSezioneDocumenti
        '
        Me.lblSezioneDocumenti.AutoSize = True
        Me.lblSezioneDocumenti.Name = "lblSezioneDocumenti"
        Me.lblSezioneDocumenti.TabIndex = 5
        Me.lblSezioneDocumenti.Text = "Documenti che l'applicazione scrive"
        '
        'lblLingua
        '
        Me.lblLingua.AutoSize = True
        Me.lblLingua.Name = "lblLingua"
        Me.lblLingua.TabIndex = 6
        Me.lblLingua.Text = "Lingua predefinita:"
        '
        'cmbLingua
        '
        Me.cmbLingua.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbLingua.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmbLingua.Name = "cmbLingua"
        Me.cmbLingua.Size = New System.Drawing.Size(180, 24)
        Me.cmbLingua.TabIndex = 7
        '
        'chkRifinitura
        '
        Me.chkRifinitura.AutoSize = True
        Me.chkRifinitura.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.chkRifinitura.Name = "chkRifinitura"
        Me.chkRifinitura.TabIndex = 8
        Me.chkRifinitura.Text = "Rifinisci i testi generati (anti-slop)"
        Me.chkRifinitura.UseVisualStyleBackColor = False
        '
        'lblRifinituraNota
        '
        Me.lblRifinituraNota.AutoSize = True
        Me.lblRifinituraNota.Name = "lblRifinituraNota"
        Me.lblRifinituraNota.TabIndex = 9
        Me.lblRifinituraNota.Text = ""
        '
        'lblSezioneCartelle
        '
        Me.lblSezioneCartelle.AutoSize = True
        Me.lblSezioneCartelle.Name = "lblSezioneCartelle"
        Me.lblSezioneCartelle.TabIndex = 10
        Me.lblSezioneCartelle.Text = "Cartelle"
        '
        'lblCartellaDati
        '
        Me.lblCartellaDati.AutoSize = True
        Me.lblCartellaDati.Name = "lblCartellaDati"
        Me.lblCartellaDati.TabIndex = 11
        Me.lblCartellaDati.Text = ""
        '
        'btnApriCartellaDati
        '
        Me.btnApriCartellaDati.Name = "btnApriCartellaDati"
        Me.btnApriCartellaDati.Size = New System.Drawing.Size(190, 32)
        Me.btnApriCartellaDati.TabIndex = 12
        Me.btnApriCartellaDati.Text = "Apri la cartella dati"
        '
        'lblCartellaDocumenti
        '
        Me.lblCartellaDocumenti.AutoSize = True
        Me.lblCartellaDocumenti.Name = "lblCartellaDocumenti"
        Me.lblCartellaDocumenti.TabIndex = 13
        Me.lblCartellaDocumenti.Text = ""
        '
        'btnGestisciDocumenti
        '
        Me.btnGestisciDocumenti.Name = "btnGestisciDocumenti"
        Me.btnGestisciDocumenti.Size = New System.Drawing.Size(230, 32)
        Me.btnGestisciDocumenti.TabIndex = 14
        Me.btnGestisciDocumenti.Text = "Gestisci i documenti…"
        '
        'lblSezioneMotore
        '
        Me.lblSezioneMotore.AutoSize = True
        Me.lblSezioneMotore.Name = "lblSezioneMotore"
        Me.lblSezioneMotore.TabIndex = 15
        Me.lblSezioneMotore.Text = "Sotto il cofano"
        '
        'lblModelli
        '
        Me.lblModelli.AutoSize = True
        Me.lblModelli.Name = "lblModelli"
        Me.lblModelli.TabIndex = 16
        Me.lblModelli.Text = ""
        '
        'lblPool
        '
        Me.lblPool.AutoSize = True
        Me.lblPool.Name = "lblPool"
        Me.lblPool.TabIndex = 17
        Me.lblPool.Text = ""
        '
        'btnApriModelli
        '
        Me.btnApriModelli.Name = "btnApriModelli"
        Me.btnApriModelli.Size = New System.Drawing.Size(230, 32)
        Me.btnApriModelli.TabIndex = 18
        Me.btnApriModelli.Text = "Apri modelli.json"
        '
        'lblSezioneDati
        '
        Me.lblSezioneDati.AutoSize = True
        Me.lblSezioneDati.Name = "lblSezioneDati"
        Me.lblSezioneDati.TabIndex = 19
        Me.lblSezioneDati.Text = "I tuoi dati"
        '
        'btnBackup
        '
        Me.btnBackup.Name = "btnBackup"
        Me.btnBackup.Size = New System.Drawing.Size(190, 32)
        Me.btnBackup.TabIndex = 20
        Me.btnBackup.Text = "Backup…"
        '
        'btnSvuotaNavigazione
        '
        Me.btnSvuotaNavigazione.Name = "btnSvuotaNavigazione"
        Me.btnSvuotaNavigazione.Size = New System.Drawing.Size(280, 32)
        Me.btnSvuotaNavigazione.TabIndex = 21
        Me.btnSvuotaNavigazione.Text = "Svuota i dati di navigazione"
        '
        'btnEliminaTutto
        '
        Me.btnEliminaTutto.Name = "btnEliminaTutto"
        Me.btnEliminaTutto.Size = New System.Drawing.Size(280, 32)
        Me.btnEliminaTutto.TabIndex = 22
        Me.btnEliminaTutto.Text = "ELIMINA TUTTI I DATI"
        '
        'lblStato
        '
        Me.lblStato.AutoSize = True
        Me.lblStato.Name = "lblStato"
        Me.lblStato.TabIndex = 23
        Me.lblStato.Text = ""
        '
        'btnChiudi
        '
        Me.btnChiudi.Name = "btnChiudi"
        Me.btnChiudi.Size = New System.Drawing.Size(110, 32)
        Me.btnChiudi.TabIndex = 24
        Me.btnChiudi.Text = "Chiudi"
        '
        'FinestraImpostazioni
        '
        Me.ClientSize = New System.Drawing.Size(660, 760)
        Me.Controls.Add(Me.lblTitolo)
        Me.Controls.Add(Me.lblSpiegazione)
        Me.Controls.Add(Me.lblSezioneChiave)
        Me.Controls.Add(Me.lblStatoChiave)
        Me.Controls.Add(Me.btnCambiaChiave)
        Me.Controls.Add(Me.lblSezioneDocumenti)
        Me.Controls.Add(Me.lblLingua)
        Me.Controls.Add(Me.cmbLingua)
        Me.Controls.Add(Me.chkRifinitura)
        Me.Controls.Add(Me.lblRifinituraNota)
        Me.Controls.Add(Me.lblSezioneCartelle)
        Me.Controls.Add(Me.lblCartellaDati)
        Me.Controls.Add(Me.btnApriCartellaDati)
        Me.Controls.Add(Me.lblCartellaDocumenti)
        Me.Controls.Add(Me.btnGestisciDocumenti)
        Me.Controls.Add(Me.lblSezioneMotore)
        Me.Controls.Add(Me.lblModelli)
        Me.Controls.Add(Me.lblPool)
        Me.Controls.Add(Me.btnApriModelli)
        Me.Controls.Add(Me.lblSezioneDati)
        Me.Controls.Add(Me.btnBackup)
        Me.Controls.Add(Me.btnSvuotaNavigazione)
        Me.Controls.Add(Me.btnEliminaTutto)
        Me.Controls.Add(Me.lblStato)
        Me.Controls.Add(Me.btnChiudi)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraImpostazioni"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "TrovaLavoro"
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSpiegazione As System.Windows.Forms.Label
    Friend WithEvents lblSezioneChiave As System.Windows.Forms.Label
    Friend WithEvents lblStatoChiave As System.Windows.Forms.Label
    Friend WithEvents btnCambiaChiave As System.Windows.Forms.Button
    Friend WithEvents lblSezioneDocumenti As System.Windows.Forms.Label
    Friend WithEvents lblLingua As System.Windows.Forms.Label
    Friend WithEvents cmbLingua As System.Windows.Forms.ComboBox
    Friend WithEvents chkRifinitura As System.Windows.Forms.CheckBox
    Friend WithEvents lblRifinituraNota As System.Windows.Forms.Label
    Friend WithEvents lblSezioneCartelle As System.Windows.Forms.Label
    Friend WithEvents lblCartellaDati As System.Windows.Forms.Label
    Friend WithEvents btnApriCartellaDati As System.Windows.Forms.Button
    Friend WithEvents lblCartellaDocumenti As System.Windows.Forms.Label
    Friend WithEvents btnGestisciDocumenti As System.Windows.Forms.Button
    Friend WithEvents lblSezioneMotore As System.Windows.Forms.Label
    Friend WithEvents lblModelli As System.Windows.Forms.Label
    Friend WithEvents lblPool As System.Windows.Forms.Label
    Friend WithEvents btnApriModelli As System.Windows.Forms.Button
    Friend WithEvents lblSezioneDati As System.Windows.Forms.Label
    Friend WithEvents btnBackup As System.Windows.Forms.Button
    Friend WithEvents btnSvuotaNavigazione As System.Windows.Forms.Button
    Friend WithEvents btnEliminaTutto As System.Windows.Forms.Button
    Friend WithEvents lblStato As System.Windows.Forms.Label
    Friend WithEvents btnChiudi As System.Windows.Forms.Button

End Class
