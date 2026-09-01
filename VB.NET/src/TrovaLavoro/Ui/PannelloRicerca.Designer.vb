<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PannelloRicerca
    Inherits System.Windows.Forms.UserControl

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

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.pnlIntestazione = New System.Windows.Forms.Panel()
        Me.lblTitolo = New System.Windows.Forms.Label()
        Me.lblSottotitolo = New System.Windows.Forms.Label()
        Me.pnlComandi = New System.Windows.Forms.Panel()
        Me.lblSalvate = New System.Windows.Forms.Label()
        Me.cboSalvate = New System.Windows.Forms.ComboBox()
        Me.btnApri = New System.Windows.Forms.Button()
        Me.btnDimentica = New System.Windows.Forms.Button()
        Me.lblPortale = New System.Windows.Forms.Label()
        Me.cboPortali = New System.Windows.Forms.ComboBox()
        Me.lblCosa = New System.Windows.Forms.Label()
        Me.txtCosa = New System.Windows.Forms.TextBox()
        Me.lblDove = New System.Windows.Forms.Label()
        Me.txtDove = New System.Windows.Forms.TextBox()
        Me.btnCerca = New System.Windows.Forms.Button()
        Me.btnSalvaRicerca = New System.Windows.Forms.Button()
        Me.btnIndietro = New System.Windows.Forms.Button()
        Me.btnRicarica = New System.Windows.Forms.Button()
        Me.txtIndirizzo = New System.Windows.Forms.TextBox()
        Me.btnVai = New System.Windows.Forms.Button()
        Me.pnlCorpo = New System.Windows.Forms.Panel()
        Me.vista = New Microsoft.Web.WebView2.WinForms.WebView2()
        Me.pnlAzioni = New System.Windows.Forms.Panel()
        Me.btnCattura = New System.Windows.Forms.Button()
        Me.btnImportaCv = New System.Windows.Forms.Button()
        Me.lblStatoRicerca = New System.Windows.Forms.Label()
        Me.pnlIntestazione.SuspendLayout()
        Me.pnlComandi.SuspendLayout()
        Me.pnlCorpo.SuspendLayout()
        Me.pnlAzioni.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlIntestazione
        '
        Me.pnlIntestazione.Controls.Add(Me.lblTitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblSottotitolo)
        Me.pnlIntestazione.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlIntestazione.Location = New System.Drawing.Point(14, 14)
        Me.pnlIntestazione.Name = "pnlIntestazione"
        Me.pnlIntestazione.Size = New System.Drawing.Size(1106, 56)
        Me.pnlIntestazione.TabIndex = 0
        '
        'lblTitolo
        '
        Me.lblTitolo.Font = StileApp.FontTitoloPannello
        Me.lblTitolo.ForeColor = StileApp.RossoTitoli
        Me.lblTitolo.Location = New System.Drawing.Point(0, 0)
        Me.lblTitolo.Name = "lblTitolo"
        Me.lblTitolo.Size = New System.Drawing.Size(500, 28)
        Me.lblTitolo.TabIndex = 0
        Me.lblTitolo.Text = "Ricerca"
        '
        'lblSottotitolo
        '
        Me.lblSottotitolo.Font = StileApp.FontDidascalia
        Me.lblSottotitolo.ForeColor = StileApp.TestoSecondario
        Me.lblSottotitolo.Location = New System.Drawing.Point(2, 32)
        Me.lblSottotitolo.Name = "lblSottotitolo"
        Me.lblSottotitolo.Size = New System.Drawing.Size(900, 18)
        Me.lblSottotitolo.TabIndex = 1
        Me.lblSottotitolo.Text = "Cerchi tu, come in un browser qualunque: quando hai davanti un annuncio, lo catturo."
        '
        'pnlComandi
        '
        ' Quattro righe: le ricerche messe da parte, la ricerca da comporre, e i comandi
        ' di navigazione con l'indirizzo — che dice dove sei e serve per andare altrove.
        Me.pnlComandi.Controls.Add(Me.lblSalvate)
        Me.pnlComandi.Controls.Add(Me.cboSalvate)
        Me.pnlComandi.Controls.Add(Me.btnApri)
        Me.pnlComandi.Controls.Add(Me.btnDimentica)
        Me.pnlComandi.Controls.Add(Me.lblPortale)
        Me.pnlComandi.Controls.Add(Me.cboPortali)
        Me.pnlComandi.Controls.Add(Me.lblCosa)
        Me.pnlComandi.Controls.Add(Me.txtCosa)
        Me.pnlComandi.Controls.Add(Me.lblDove)
        Me.pnlComandi.Controls.Add(Me.txtDove)
        Me.pnlComandi.Controls.Add(Me.btnCerca)
        Me.pnlComandi.Controls.Add(Me.btnSalvaRicerca)
        Me.pnlComandi.Controls.Add(Me.btnIndietro)
        Me.pnlComandi.Controls.Add(Me.btnRicarica)
        Me.pnlComandi.Controls.Add(Me.txtIndirizzo)
        Me.pnlComandi.Controls.Add(Me.btnVai)
        Me.pnlComandi.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlComandi.Location = New System.Drawing.Point(14, 70)
        Me.pnlComandi.Name = "pnlComandi"
        Me.pnlComandi.Size = New System.Drawing.Size(1106, 116)
        Me.pnlComandi.TabIndex = 1
        '
        'lblSalvate
        '
        Me.lblSalvate.Font = StileApp.FontTitoloGruppo
        Me.lblSalvate.ForeColor = StileApp.RossoCritico
        Me.lblSalvate.Location = New System.Drawing.Point(0, 6)
        Me.lblSalvate.Name = "lblSalvate"
        Me.lblSalvate.Size = New System.Drawing.Size(120, 18)
        Me.lblSalvate.TabIndex = 0
        Me.lblSalvate.Text = "Ricerche salvate"
        '
        'cboSalvate
        '
        Me.cboSalvate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboSalvate.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cboSalvate.Location = New System.Drawing.Point(126, 2)
        Me.cboSalvate.Name = "cboSalvate"
        Me.cboSalvate.Size = New System.Drawing.Size(420, 23)
        Me.cboSalvate.TabIndex = 1
        '
        'btnApri
        '
        Me.btnApri.Location = New System.Drawing.Point(558, 1)
        Me.btnApri.Name = "btnApri"
        Me.btnApri.Size = New System.Drawing.Size(90, 26)
        Me.btnApri.TabIndex = 2
        Me.btnApri.Text = "Apri"
        '
        'btnDimentica
        '
        Me.btnDimentica.Location = New System.Drawing.Point(656, 1)
        Me.btnDimentica.Name = "btnDimentica"
        Me.btnDimentica.Size = New System.Drawing.Size(110, 26)
        Me.btnDimentica.TabIndex = 3
        Me.btnDimentica.Text = "Dimentica"
        '
        'lblPortale
        '
        Me.lblPortale.Font = StileApp.FontTitoloGruppo
        Me.lblPortale.ForeColor = StileApp.RossoCritico
        Me.lblPortale.Location = New System.Drawing.Point(0, 40)
        Me.lblPortale.Name = "lblPortale"
        Me.lblPortale.Size = New System.Drawing.Size(120, 18)
        Me.lblPortale.TabIndex = 4
        Me.lblPortale.Text = "Cerca su"
        '
        'cboPortali
        '
        Me.cboPortali.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboPortali.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cboPortali.Location = New System.Drawing.Point(126, 36)
        Me.cboPortali.Name = "cboPortali"
        Me.cboPortali.Size = New System.Drawing.Size(190, 23)
        Me.cboPortali.TabIndex = 5
        '
        'lblCosa
        '
        Me.lblCosa.Font = StileApp.FontDidascalia
        Me.lblCosa.ForeColor = StileApp.TestoSecondario
        Me.lblCosa.Location = New System.Drawing.Point(328, 40)
        Me.lblCosa.Name = "lblCosa"
        Me.lblCosa.Size = New System.Drawing.Size(40, 18)
        Me.lblCosa.TabIndex = 6
        Me.lblCosa.Text = "cosa"
        '
        'txtCosa
        '
        Me.txtCosa.BackColor = StileApp.FondoCasella
        Me.txtCosa.Location = New System.Drawing.Point(370, 36)
        Me.txtCosa.Name = "txtCosa"
        Me.txtCosa.PlaceholderText = "magazziniere, perito elettronico…"
        Me.txtCosa.Size = New System.Drawing.Size(230, 23)
        Me.txtCosa.TabIndex = 7
        '
        'lblDove
        '
        Me.lblDove.Font = StileApp.FontDidascalia
        Me.lblDove.ForeColor = StileApp.TestoSecondario
        Me.lblDove.Location = New System.Drawing.Point(612, 40)
        Me.lblDove.Name = "lblDove"
        Me.lblDove.Size = New System.Drawing.Size(40, 18)
        Me.lblDove.TabIndex = 8
        Me.lblDove.Text = "dove"
        '
        'txtDove
        '
        Me.txtDove.BackColor = StileApp.FondoCasella
        Me.txtDove.Location = New System.Drawing.Point(654, 36)
        Me.txtDove.Name = "txtDove"
        Me.txtDove.PlaceholderText = "Genova, Chiavari…"
        Me.txtDove.Size = New System.Drawing.Size(170, 23)
        Me.txtDove.TabIndex = 9
        '
        'btnCerca
        '
        Me.btnCerca.Location = New System.Drawing.Point(836, 35)
        Me.btnCerca.Name = "btnCerca"
        Me.btnCerca.Size = New System.Drawing.Size(100, 26)
        Me.btnCerca.TabIndex = 10
        Me.btnCerca.Text = "Cerca"
        '
        'btnSalvaRicerca
        '
        Me.btnSalvaRicerca.Location = New System.Drawing.Point(944, 35)
        Me.btnSalvaRicerca.Name = "btnSalvaRicerca"
        Me.btnSalvaRicerca.Size = New System.Drawing.Size(162, 26)
        Me.btnSalvaRicerca.TabIndex = 11
        Me.btnSalvaRicerca.Text = "Salva questa ricerca"
        '
        'btnIndietro
        '
        ' I tre comandi della riga di navigazione dicono il proprio nome a parte: due
        ' portano un simbolo al posto del testo e il terzo non ha un'etichetta accanto,
        ' quindi senza AccessibleName resterebbero anonimi per chi non vede lo schermo —
        ' e per gli strumenti che guidano l'applicazione.
        Me.btnIndietro.AccessibleName = "Indietro"
        Me.btnIndietro.Location = New System.Drawing.Point(0, 74)
        Me.btnIndietro.Name = "btnIndietro"
        Me.btnIndietro.Size = New System.Drawing.Size(40, 26)
        Me.btnIndietro.TabIndex = 12
        Me.btnIndietro.Text = "◀"
        '
        'btnRicarica
        '
        Me.btnRicarica.AccessibleName = "Ricarica"
        Me.btnRicarica.Location = New System.Drawing.Point(46, 74)
        Me.btnRicarica.Name = "btnRicarica"
        Me.btnRicarica.Size = New System.Drawing.Size(40, 26)
        Me.btnRicarica.TabIndex = 13
        Me.btnRicarica.Text = "⟳"
        '
        'txtIndirizzo
        '
        Me.txtIndirizzo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtIndirizzo.AccessibleName = "Indirizzo"
        Me.txtIndirizzo.BackColor = StileApp.FondoCasella
        Me.txtIndirizzo.Font = StileApp.FontDatiTecnici
        Me.txtIndirizzo.Location = New System.Drawing.Point(96, 76)
        Me.txtIndirizzo.Name = "txtIndirizzo"
        Me.txtIndirizzo.PlaceholderText = "…oppure incolla qui il link di un annuncio"
        Me.txtIndirizzo.Size = New System.Drawing.Size(920, 23)
        Me.txtIndirizzo.TabIndex = 14
        '
        'btnVai
        '
        Me.btnVai.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnVai.Location = New System.Drawing.Point(1026, 74)
        Me.btnVai.Name = "btnVai"
        Me.btnVai.Size = New System.Drawing.Size(80, 26)
        Me.btnVai.TabIndex = 15
        Me.btnVai.Text = "Vai"
        '
        'pnlCorpo
        '
        ' Il riquadro del browser: un bordo da 1 px che separa la pagina del portale
        ' dall'applicazione, così è sempre chiaro dove finisce l'una e comincia l'altra.
        Me.pnlCorpo.BackColor = StileApp.FondoCasella
        Me.pnlCorpo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.pnlCorpo.Controls.Add(Me.vista)
        Me.pnlCorpo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCorpo.Location = New System.Drawing.Point(14, 186)
        Me.pnlCorpo.Name = "pnlCorpo"
        Me.pnlCorpo.Padding = New System.Windows.Forms.Padding(1)
        Me.pnlCorpo.Size = New System.Drawing.Size(1106, 496)
        Me.pnlCorpo.TabIndex = 2
        '
        'vista
        '
        Me.vista.AllowExternalDrop = False
        Me.vista.CreationProperties = Nothing
        Me.vista.DefaultBackgroundColor = StileApp.FondoCasella
        Me.vista.Dock = System.Windows.Forms.DockStyle.Fill
        Me.vista.Location = New System.Drawing.Point(1, 1)
        Me.vista.Name = "vista"
        Me.vista.Size = New System.Drawing.Size(1102, 492)
        Me.vista.TabIndex = 0
        Me.vista.ZoomFactor = 1.0R
        '
        'pnlAzioni
        '
        Me.pnlAzioni.Controls.Add(Me.btnCattura)
        Me.pnlAzioni.Controls.Add(Me.btnImportaCv)
        Me.pnlAzioni.Controls.Add(Me.lblStatoRicerca)
        Me.pnlAzioni.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlAzioni.Location = New System.Drawing.Point(14, 682)
        Me.pnlAzioni.Name = "pnlAzioni"
        Me.pnlAzioni.Size = New System.Drawing.Size(1106, 64)
        Me.pnlAzioni.TabIndex = 3
        '
        'btnCattura
        '
        Me.btnCattura.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnCattura.Location = New System.Drawing.Point(0, 12)
        Me.btnCattura.Name = "btnCattura"
        Me.btnCattura.Size = New System.Drawing.Size(180, 32)
        Me.btnCattura.TabIndex = 0
        Me.btnCattura.Text = "Cattura annuncio"
        '
        'btnImportaCv
        '
        Me.btnImportaCv.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnImportaCv.Location = New System.Drawing.Point(192, 12)
        Me.btnImportaCv.Name = "btnImportaCv"
        Me.btnImportaCv.Size = New System.Drawing.Size(210, 32)
        Me.btnImportaCv.TabIndex = 1
        Me.btnImportaCv.Text = "Importa CV da questa pagina"
        '
        'lblStatoRicerca
        '
        Me.lblStatoRicerca.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatoRicerca.Font = StileApp.FontDidascalia
        Me.lblStatoRicerca.ForeColor = StileApp.TestoSecondario
        Me.lblStatoRicerca.Location = New System.Drawing.Point(414, 20)
        Me.lblStatoRicerca.Name = "lblStatoRicerca"
        Me.lblStatoRicerca.Size = New System.Drawing.Size(692, 36)
        Me.lblStatoRicerca.TabIndex = 2
        '
        'PannelloRicerca
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = StileApp.FondoPagina
        Me.Controls.Add(Me.pnlCorpo)
        Me.Controls.Add(Me.pnlComandi)
        Me.Controls.Add(Me.pnlIntestazione)
        Me.Controls.Add(Me.pnlAzioni)
        Me.Font = StileApp.FontTesto
        Me.ForeColor = StileApp.TestoPrimario
        Me.Name = "PannelloRicerca"
        Me.Padding = New System.Windows.Forms.Padding(14)
        Me.Size = New System.Drawing.Size(1134, 760)
        Me.pnlIntestazione.ResumeLayout(False)
        Me.pnlComandi.ResumeLayout(False)
        Me.pnlCorpo.ResumeLayout(False)
        Me.pnlAzioni.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents pnlIntestazione As System.Windows.Forms.Panel
    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSottotitolo As System.Windows.Forms.Label
    Friend WithEvents pnlComandi As System.Windows.Forms.Panel
    Friend WithEvents lblSalvate As System.Windows.Forms.Label
    Friend WithEvents cboSalvate As System.Windows.Forms.ComboBox
    Friend WithEvents btnApri As System.Windows.Forms.Button
    Friend WithEvents btnDimentica As System.Windows.Forms.Button
    Friend WithEvents lblPortale As System.Windows.Forms.Label
    Friend WithEvents cboPortali As System.Windows.Forms.ComboBox
    Friend WithEvents lblCosa As System.Windows.Forms.Label
    Friend WithEvents txtCosa As System.Windows.Forms.TextBox
    Friend WithEvents lblDove As System.Windows.Forms.Label
    Friend WithEvents txtDove As System.Windows.Forms.TextBox
    Friend WithEvents btnCerca As System.Windows.Forms.Button
    Friend WithEvents btnSalvaRicerca As System.Windows.Forms.Button
    Friend WithEvents btnIndietro As System.Windows.Forms.Button
    Friend WithEvents btnRicarica As System.Windows.Forms.Button
    Friend WithEvents txtIndirizzo As System.Windows.Forms.TextBox
    Friend WithEvents btnVai As System.Windows.Forms.Button
    Friend WithEvents pnlCorpo As System.Windows.Forms.Panel
    Friend WithEvents vista As Microsoft.Web.WebView2.WinForms.WebView2
    Friend WithEvents pnlAzioni As System.Windows.Forms.Panel
    Friend WithEvents btnCattura As System.Windows.Forms.Button
    Friend WithEvents btnImportaCv As System.Windows.Forms.Button
    Friend WithEvents lblStatoRicerca As System.Windows.Forms.Label

End Class
