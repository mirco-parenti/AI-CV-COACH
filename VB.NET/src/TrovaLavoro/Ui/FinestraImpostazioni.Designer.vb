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
        Me.btnComeFunziona = New System.Windows.Forms.Button()
        Me.btnInformazioni = New System.Windows.Forms.Button()
        Me.lblSezioneChiave = New System.Windows.Forms.Label()
        Me.lblStatoChiave = New System.Windows.Forms.Label()
        Me.btnCambiaChiave = New System.Windows.Forms.Button()
        Me.lblSezioneDocumenti = New System.Windows.Forms.Label()
        Me.lblLingua = New System.Windows.Forms.Label()
        Me.cmbLingua = New System.Windows.Forms.ComboBox()
        Me.chkRifinitura = New System.Windows.Forms.CheckBox()
        Me.lblRifinituraNota = New System.Windows.Forms.Label()
        Me.lblSezioneCandidature = New System.Windows.Forms.Label()
        Me.lblFollowUp = New System.Windows.Forms.Label()
        Me.numFollowUp = New System.Windows.Forms.NumericUpDown()
        Me.lblGiorni = New System.Windows.Forms.Label()
        Me.lblFollowUpNota = New System.Windows.Forms.Label()
        Me.lblSezioneCartelle = New System.Windows.Forms.Label()
        Me.lblCartellaDati = New System.Windows.Forms.Label()
        Me.btnApriCartellaDati = New System.Windows.Forms.Button()
        Me.lblCartellaDocumenti = New System.Windows.Forms.Label()
        Me.btnGestisciDocumenti = New System.Windows.Forms.Button()
        Me.lblSezioneMotore = New System.Windows.Forms.Label()
        Me.lblModelloRagionamento = New System.Windows.Forms.Label()
        Me.cmbModelloRagionamento = New System.Windows.Forms.ComboBox()
        Me.lblModelloSemplice = New System.Windows.Forms.Label()
        Me.cmbModelloSemplice = New System.Windows.Forms.ComboBox()
        Me.lblModelli = New System.Windows.Forms.Label()
        Me.lblPool = New System.Windows.Forms.Label()
        Me.btnApriModelli = New System.Windows.Forms.Button()
        Me.lblSezioneConsumo = New System.Windows.Forms.Label()
        Me.lblConsumo = New System.Windows.Forms.Label()
        Me.btnApriChiamate = New System.Windows.Forms.Button()
        Me.lblSezioneDati = New System.Windows.Forms.Label()
        Me.btnBackup = New System.Windows.Forms.Button()
        Me.btnSvuotaNavigazione = New System.Windows.Forms.Button()
        Me.btnEliminaTutto = New System.Windows.Forms.Button()
        Me.lblStato = New System.Windows.Forms.Label()
        Me.btnChiudi = New System.Windows.Forms.Button()
        Me.pnlContenuto = New System.Windows.Forms.Panel()
        Me.pnlFascia = New System.Windows.Forms.Panel()
        CType(Me.numFollowUp, System.ComponentModel.ISupportInitialize).BeginInit()
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
        'btnComeFunziona
        '
        Me.btnComeFunziona.Name = "btnComeFunziona"
        Me.btnComeFunziona.Size = StileApp.BottoneMedio
        Me.btnComeFunziona.TabIndex = 2
        Me.btnComeFunziona.Text = "Come funziona…"
        '
        'btnInformazioni
        '
        Me.btnInformazioni.Name = "btnInformazioni"
        Me.btnInformazioni.Size = StileApp.BottoneMedio
        Me.btnInformazioni.TabIndex = 3
        Me.btnInformazioni.Text = "Informazioni su…"
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
        Me.btnCambiaChiave.Size = StileApp.BottoneLargo
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
        'lblSezioneCandidature
        '
        Me.lblSezioneCandidature.AutoSize = True
        Me.lblSezioneCandidature.Name = "lblSezioneCandidature"
        Me.lblSezioneCandidature.TabIndex = 10
        Me.lblSezioneCandidature.Text = "Candidature spedite"
        '
        'lblFollowUp
        '
        Me.lblFollowUp.AutoSize = True
        Me.lblFollowUp.Name = "lblFollowUp"
        Me.lblFollowUp.TabIndex = 11
        Me.lblFollowUp.Text = "Ricordamele se non rispondono entro:"
        '
        'numFollowUp
        '
        Me.numFollowUp.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.numFollowUp.Name = "numFollowUp"
        Me.numFollowUp.Size = New System.Drawing.Size(70, 24)
        Me.numFollowUp.TabIndex = 12
        Me.numFollowUp.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        '
        'lblGiorni
        '
        Me.lblGiorni.AutoSize = True
        Me.lblGiorni.Name = "lblGiorni"
        Me.lblGiorni.TabIndex = 13
        Me.lblGiorni.Text = "giorni"
        '
        'lblFollowUpNota
        '
        Me.lblFollowUpNota.AutoSize = True
        Me.lblFollowUpNota.Name = "lblFollowUpNota"
        Me.lblFollowUpNota.TabIndex = 14
        Me.lblFollowUpNota.Text = ""
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
        Me.btnApriCartellaDati.Size = StileApp.BottoneLargo
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
        Me.btnGestisciDocumenti.Size = StileApp.BottoneMoltoLargo
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
        'lblModelloRagionamento
        '
        Me.lblModelloRagionamento.AutoSize = True
        Me.lblModelloRagionamento.Name = "lblModelloRagionamento"
        Me.lblModelloRagionamento.TabIndex = 16
        Me.lblModelloRagionamento.Text = "Ragionamento (confronto, mitigazione, testi):"
        '
        'cmbModelloRagionamento
        '
        Me.cmbModelloRagionamento.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbModelloRagionamento.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmbModelloRagionamento.Name = "cmbModelloRagionamento"
        Me.cmbModelloRagionamento.Size = New System.Drawing.Size(330, 24)
        Me.cmbModelloRagionamento.TabIndex = 17
        '
        'lblModelloSemplice
        '
        Me.lblModelloSemplice.AutoSize = True
        Me.lblModelloSemplice.Name = "lblModelloSemplice"
        Me.lblModelloSemplice.TabIndex = 18
        Me.lblModelloSemplice.Text = "Elaborazioni testuali (estrazioni, strutturazioni):"
        '
        'cmbModelloSemplice
        '
        Me.cmbModelloSemplice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbModelloSemplice.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmbModelloSemplice.Name = "cmbModelloSemplice"
        Me.cmbModelloSemplice.Size = New System.Drawing.Size(330, 24)
        Me.cmbModelloSemplice.TabIndex = 19
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
        Me.btnApriModelli.Size = StileApp.BottoneMoltoLargo
        Me.btnApriModelli.TabIndex = 18
        Me.btnApriModelli.Text = "Apri modelli.json"
        '
        'lblSezioneConsumo
        '
        Me.lblSezioneConsumo.AutoSize = True
        Me.lblSezioneConsumo.Name = "lblSezioneConsumo"
        Me.lblSezioneConsumo.TabIndex = 20
        Me.lblSezioneConsumo.Text = "Quanto è costato"
        '
        'lblConsumo
        '
        Me.lblConsumo.AutoSize = True
        Me.lblConsumo.Name = "lblConsumo"
        Me.lblConsumo.TabIndex = 21
        Me.lblConsumo.Text = ""
        '
        'btnApriChiamate
        '
        Me.btnApriChiamate.Name = "btnApriChiamate"
        Me.btnApriChiamate.Size = StileApp.BottoneMoltoLargo
        Me.btnApriChiamate.TabIndex = 22
        Me.btnApriChiamate.Text = "Apri il conto delle chiamate"
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
        Me.btnBackup.Size = StileApp.BottoneLargo
        Me.btnBackup.TabIndex = 20
        Me.btnBackup.Text = "Backup…"
        '
        'btnSvuotaNavigazione
        '
        Me.btnSvuotaNavigazione.Name = "btnSvuotaNavigazione"
        Me.btnSvuotaNavigazione.Size = StileApp.BottoneMassimo
        Me.btnSvuotaNavigazione.TabIndex = 21
        Me.btnSvuotaNavigazione.Text = "Svuota i dati di navigazione"
        '
        'btnEliminaTutto
        '
        Me.btnEliminaTutto.Name = "btnEliminaTutto"
        Me.btnEliminaTutto.Size = StileApp.BottoneMassimo
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
        Me.btnChiudi.Size = StileApp.BottoneStandard
        Me.btnChiudi.TabIndex = 24
        Me.btnChiudi.Text = "Chiudi"
        Me.ClientSize = New System.Drawing.Size(660, 760)
        '
        'pnlContenuto
        '
        Me.pnlContenuto.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlContenuto.Name = "pnlContenuto"
        Me.pnlContenuto.TabIndex = 0
        '
        'pnlFascia
        '
        ' La fascia sta fuori da quel che scorre: «Chiudi» è il comando che chiude questa
        ' finestra e non può essere il primo a finire sotto il bordo quando il contenuto
        ' non ci sta (2026-08-27). Il contenuto si aggiunge per primo di proposito: il
        ' docking parte dall'ultimo arrivato, così la fascia si prende il fondo e il
        ' contenuto quel che resta.
        Me.pnlFascia.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlFascia.Height = StileApp.MargineRiquadro * 2 + 32
        Me.pnlFascia.Name = "pnlFascia"
        Me.pnlFascia.TabIndex = 1
        '
        'FinestraImpostazioni
        '
        Me.pnlContenuto.Controls.Add(Me.lblTitolo)
        Me.pnlContenuto.Controls.Add(Me.lblSpiegazione)
        Me.pnlContenuto.Controls.Add(Me.btnComeFunziona)
        Me.pnlContenuto.Controls.Add(Me.btnInformazioni)
        Me.pnlContenuto.Controls.Add(Me.lblSezioneChiave)
        Me.pnlContenuto.Controls.Add(Me.lblStatoChiave)
        Me.pnlContenuto.Controls.Add(Me.btnCambiaChiave)
        Me.pnlContenuto.Controls.Add(Me.lblSezioneDocumenti)
        Me.pnlContenuto.Controls.Add(Me.lblLingua)
        Me.pnlContenuto.Controls.Add(Me.cmbLingua)
        Me.pnlContenuto.Controls.Add(Me.chkRifinitura)
        Me.pnlContenuto.Controls.Add(Me.lblRifinituraNota)
        Me.pnlContenuto.Controls.Add(Me.lblSezioneCandidature)
        Me.pnlContenuto.Controls.Add(Me.lblFollowUp)
        Me.pnlContenuto.Controls.Add(Me.numFollowUp)
        Me.pnlContenuto.Controls.Add(Me.lblGiorni)
        Me.pnlContenuto.Controls.Add(Me.lblFollowUpNota)
        Me.pnlContenuto.Controls.Add(Me.lblSezioneCartelle)
        Me.pnlContenuto.Controls.Add(Me.lblCartellaDati)
        Me.pnlContenuto.Controls.Add(Me.btnApriCartellaDati)
        Me.pnlContenuto.Controls.Add(Me.lblCartellaDocumenti)
        Me.pnlContenuto.Controls.Add(Me.btnGestisciDocumenti)
        Me.pnlContenuto.Controls.Add(Me.lblSezioneMotore)
        Me.pnlContenuto.Controls.Add(Me.lblModelloRagionamento)
        Me.pnlContenuto.Controls.Add(Me.cmbModelloRagionamento)
        Me.pnlContenuto.Controls.Add(Me.lblModelloSemplice)
        Me.pnlContenuto.Controls.Add(Me.cmbModelloSemplice)
        Me.pnlContenuto.Controls.Add(Me.lblModelli)
        Me.pnlContenuto.Controls.Add(Me.lblPool)
        Me.pnlContenuto.Controls.Add(Me.btnApriModelli)
        Me.pnlContenuto.Controls.Add(Me.lblSezioneConsumo)
        Me.pnlContenuto.Controls.Add(Me.lblConsumo)
        Me.pnlContenuto.Controls.Add(Me.btnApriChiamate)
        Me.pnlContenuto.Controls.Add(Me.lblSezioneDati)
        Me.pnlContenuto.Controls.Add(Me.btnBackup)
        Me.pnlContenuto.Controls.Add(Me.btnSvuotaNavigazione)
        Me.pnlContenuto.Controls.Add(Me.btnEliminaTutto)
        Me.pnlContenuto.Controls.Add(Me.lblStato)
        Me.pnlFascia.Controls.Add(Me.btnChiudi)
        Me.Controls.Add(Me.pnlContenuto)
        Me.Controls.Add(Me.pnlFascia)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FinestraImpostazioni"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "TrovaLavoro"
        CType(Me.numFollowUp, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSpiegazione As System.Windows.Forms.Label
    Friend WithEvents btnComeFunziona As System.Windows.Forms.Button
    Friend WithEvents btnInformazioni As System.Windows.Forms.Button
    Friend WithEvents lblSezioneChiave As System.Windows.Forms.Label
    Friend WithEvents lblStatoChiave As System.Windows.Forms.Label
    Friend WithEvents btnCambiaChiave As System.Windows.Forms.Button
    Friend WithEvents lblSezioneDocumenti As System.Windows.Forms.Label
    Friend WithEvents lblLingua As System.Windows.Forms.Label
    Friend WithEvents cmbLingua As System.Windows.Forms.ComboBox
    Friend WithEvents chkRifinitura As System.Windows.Forms.CheckBox
    Friend WithEvents lblRifinituraNota As System.Windows.Forms.Label
    Friend WithEvents lblSezioneCandidature As System.Windows.Forms.Label
    Friend WithEvents lblFollowUp As System.Windows.Forms.Label
    Friend WithEvents numFollowUp As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblGiorni As System.Windows.Forms.Label
    Friend WithEvents lblFollowUpNota As System.Windows.Forms.Label
    Friend WithEvents lblSezioneCartelle As System.Windows.Forms.Label
    Friend WithEvents lblCartellaDati As System.Windows.Forms.Label
    Friend WithEvents btnApriCartellaDati As System.Windows.Forms.Button
    Friend WithEvents lblCartellaDocumenti As System.Windows.Forms.Label
    Friend WithEvents btnGestisciDocumenti As System.Windows.Forms.Button
    Friend WithEvents lblSezioneMotore As System.Windows.Forms.Label
    Friend WithEvents lblModelloRagionamento As System.Windows.Forms.Label
    Friend WithEvents cmbModelloRagionamento As System.Windows.Forms.ComboBox
    Friend WithEvents lblModelloSemplice As System.Windows.Forms.Label
    Friend WithEvents cmbModelloSemplice As System.Windows.Forms.ComboBox
    Friend WithEvents lblModelli As System.Windows.Forms.Label
    Friend WithEvents lblPool As System.Windows.Forms.Label
    Friend WithEvents btnApriModelli As System.Windows.Forms.Button
    Friend WithEvents lblSezioneConsumo As System.Windows.Forms.Label
    Friend WithEvents lblConsumo As System.Windows.Forms.Label
    Friend WithEvents btnApriChiamate As System.Windows.Forms.Button
    Friend WithEvents lblSezioneDati As System.Windows.Forms.Label
    Friend WithEvents btnBackup As System.Windows.Forms.Button
    Friend WithEvents btnSvuotaNavigazione As System.Windows.Forms.Button
    Friend WithEvents btnEliminaTutto As System.Windows.Forms.Button
    Friend WithEvents lblStato As System.Windows.Forms.Label
    Friend WithEvents btnChiudi As System.Windows.Forms.Button
    Friend WithEvents pnlContenuto As System.Windows.Forms.Panel
    Friend WithEvents pnlFascia As System.Windows.Forms.Panel

End Class
