<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormPrincipale
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
        Me.components = New System.ComponentModel.Container()
        Me.ttSuggerimenti = New System.Windows.Forms.ToolTip(Me.components)
        Me.tlpStruttura = New System.Windows.Forms.TableLayoutPanel()
        Me.pnlBarraSuperiore = New System.Windows.Forms.Panel()
        Me.btnMenu = New System.Windows.Forms.Button()
        Me.btnHome = New System.Windows.Forms.Button()
        Me.btnProfilo = New System.Windows.Forms.Button()
        Me.btnRicerca = New System.Windows.Forms.Button()
        Me.btnCandidatura = New System.Windows.Forms.Button()
        Me.btnDocumenti = New System.Windows.Forms.Button()
        Me.btnImpostazioni = New System.Windows.Forms.Button()
        Me.pnlBordoBarraSuperiore = New System.Windows.Forms.Panel()
        Me.pnlAreaCentrale = New System.Windows.Forms.Panel()
        Me.pnlFasciaInferiore = New System.Windows.Forms.Panel()
        Me.lblStato = New System.Windows.Forms.Label()
        Me.pnlBordoFasciaInferiore = New System.Windows.Forms.Panel()
        Me.pnlMenu = New PannelloMenu()
        Me.pnlHome = New PannelloHome()
        Me.pnlProfilo = New PannelloProfilo()
        Me.pnlDialogo = New PannelloDialogo()
        Me.pnlOpportunita = New PannelloOpportunita()
        Me.pnlDocumenti = New PannelloDocumenti()
        Me.pnlEmail = New PannelloEmail()
        Me.pnlRicerca = New PannelloRicerca()
        Me.pnlLogo = New System.Windows.Forms.Panel()
        Me.picLogo = New System.Windows.Forms.PictureBox()
        Me.lblMarchio = New System.Windows.Forms.Label()
        Me.lblVersione = New System.Windows.Forms.Label()
        Me.lblCopyright = New System.Windows.Forms.Label()
        Me.tlpStruttura.SuspendLayout()
        Me.pnlBarraSuperiore.SuspendLayout()
        Me.pnlAreaCentrale.SuspendLayout()
        Me.pnlFasciaInferiore.SuspendLayout()
        Me.pnlLogo.SuspendLayout()
        CType(Me.picLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'tlpStruttura
        '
        Me.tlpStruttura.ColumnCount = 1
        Me.tlpStruttura.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tlpStruttura.Controls.Add(Me.pnlBarraSuperiore, 0, 0)
        Me.tlpStruttura.Controls.Add(Me.pnlAreaCentrale, 0, 1)
        Me.tlpStruttura.Controls.Add(Me.pnlFasciaInferiore, 0, 2)
        Me.tlpStruttura.Dock = System.Windows.Forms.DockStyle.Fill
        Me.tlpStruttura.Location = New System.Drawing.Point(0, 0)
        Me.tlpStruttura.Margin = New System.Windows.Forms.Padding(0)
        Me.tlpStruttura.Name = "tlpStruttura"
        Me.tlpStruttura.RowCount = 3
        Me.tlpStruttura.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48.0!))
        Me.tlpStruttura.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tlpStruttura.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 0.0!))
        Me.tlpStruttura.Size = New System.Drawing.Size(1134, 561)
        Me.tlpStruttura.TabIndex = 0
        '
        'pnlBarraSuperiore
        '
        Me.pnlBarraSuperiore.BackColor = StileApp.SfondoContenuto
        Me.pnlBarraSuperiore.Controls.Add(Me.btnMenu)
        Me.pnlBarraSuperiore.Controls.Add(Me.btnHome)
        Me.pnlBarraSuperiore.Controls.Add(Me.btnProfilo)
        Me.pnlBarraSuperiore.Controls.Add(Me.btnRicerca)
        Me.pnlBarraSuperiore.Controls.Add(Me.btnCandidatura)
        Me.pnlBarraSuperiore.Controls.Add(Me.btnDocumenti)
        Me.pnlBarraSuperiore.Controls.Add(Me.btnImpostazioni)
        Me.pnlBarraSuperiore.Controls.Add(Me.pnlBordoBarraSuperiore)
        Me.pnlBarraSuperiore.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlBarraSuperiore.Location = New System.Drawing.Point(0, 0)
        Me.pnlBarraSuperiore.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlBarraSuperiore.Name = "pnlBarraSuperiore"
        Me.pnlBarraSuperiore.Size = New System.Drawing.Size(1134, 48)
        Me.pnlBarraSuperiore.TabIndex = 0
        '
        'btnMenu
        '
        ' La via di ritorno al menu d'ingresso (P0). Senza, dal menu si esce e non si
        ' rientra: la barra porta ai sei pannelli ma non alla schermata che li elenca.
        Me.btnMenu.BackColor = StileApp.SfondoContenuto
        Me.btnMenu.FlatAppearance.BorderColor = StileApp.BordoLeggero
        Me.btnMenu.FlatAppearance.BorderSize = 1
        Me.btnMenu.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnMenu.ForeColor = StileApp.TestoPrimario
        Me.btnMenu.Location = New System.Drawing.Point(14, 7)
        Me.btnMenu.Name = "btnMenu"
        Me.btnMenu.Size = StileApp.BottoneBarraSuperiore
        Me.btnMenu.TabIndex = 0
        Me.btnMenu.Text = "🎮 Menu"
        Me.btnMenu.UseVisualStyleBackColor = False
        '
        'btnHome
        '
        Me.btnHome.BackColor = StileApp.SfondoContenuto
        Me.btnHome.FlatAppearance.BorderColor = StileApp.BordoLeggero
        Me.btnHome.FlatAppearance.BorderSize = 1
        Me.btnHome.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnHome.ForeColor = StileApp.TestoPrimario
        Me.btnHome.Location = New System.Drawing.Point(136, 7)
        Me.btnHome.Name = "btnHome"
        Me.btnHome.Size = StileApp.BottoneBarraSuperioreLargo
        Me.btnHome.TabIndex = 1
        Me.btnHome.Text = "🏠 Le mie candidature"
        Me.btnHome.UseVisualStyleBackColor = False
        '
        'btnProfilo
        '
        Me.btnProfilo.BackColor = StileApp.SfondoContenuto
        Me.btnProfilo.FlatAppearance.BorderColor = StileApp.BordoLeggero
        Me.btnProfilo.FlatAppearance.BorderSize = 1
        Me.btnProfilo.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnProfilo.ForeColor = StileApp.TestoPrimario
        Me.btnProfilo.Location = New System.Drawing.Point(358, 7)
        Me.btnProfilo.Name = "btnProfilo"
        Me.btnProfilo.Size = StileApp.BottoneBarraSuperiore
        Me.btnProfilo.TabIndex = 2
        Me.btnProfilo.Text = "👤 Profilo"
        Me.btnProfilo.UseVisualStyleBackColor = False
        '
        'btnRicerca
        '
        Me.btnRicerca.BackColor = StileApp.SfondoContenuto
        Me.btnRicerca.FlatAppearance.BorderColor = StileApp.BordoLeggero
        Me.btnRicerca.FlatAppearance.BorderSize = 1
        Me.btnRicerca.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnRicerca.ForeColor = StileApp.TestoPrimario
        Me.btnRicerca.Location = New System.Drawing.Point(480, 7)
        Me.btnRicerca.Name = "btnRicerca"
        Me.btnRicerca.Size = StileApp.BottoneBarraSuperiore
        Me.btnRicerca.TabIndex = 3
        Me.btnRicerca.Text = "🔍 Ricerca"
        Me.btnRicerca.UseVisualStyleBackColor = False
        '
        'btnCandidatura
        '
        ' La porta di P4. Il cruscotto che raccoglierà le opportunità è di T5: fino ad
        ' allora questa è l'unica strada per arrivare a un annuncio (cap. 12.3).
        Me.btnCandidatura.BackColor = StileApp.SfondoContenuto
        Me.btnCandidatura.FlatAppearance.BorderColor = StileApp.BordoLeggero
        Me.btnCandidatura.FlatAppearance.BorderSize = 1
        Me.btnCandidatura.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnCandidatura.ForeColor = StileApp.TestoPrimario
        Me.btnCandidatura.Location = New System.Drawing.Point(602, 7)
        Me.btnCandidatura.Name = "btnCandidatura"
        Me.btnCandidatura.Size = StileApp.BottoneBarraSuperioreLargo
        Me.btnCandidatura.TabIndex = 4
        Me.btnCandidatura.Text = NomiUi.Confronto
        Me.btnCandidatura.UseVisualStyleBackColor = False
        '
        'btnDocumenti
        '
        ' La porta di P6 (T9d). Fino ad allora i documenti erano solo il passo successivo
        ' di un flusso — dalla candidatura o dal profilo — e per rileggere un CV scritto
        ' ieri bisognava ripassare dalla Home e riaprire la candidatura giusta.
        Me.btnDocumenti.BackColor = StileApp.SfondoContenuto
        Me.btnDocumenti.FlatAppearance.BorderColor = StileApp.BordoLeggero
        Me.btnDocumenti.FlatAppearance.BorderSize = 1
        Me.btnDocumenti.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnDocumenti.ForeColor = StileApp.TestoPrimario
        Me.btnDocumenti.Location = New System.Drawing.Point(824, 7)
        Me.btnDocumenti.Name = "btnDocumenti"
        Me.btnDocumenti.Size = StileApp.BottoneBarraSuperiore
        Me.btnDocumenti.TabIndex = 5
        Me.btnDocumenti.Text = "📄 Documenti"
        Me.btnDocumenti.UseVisualStyleBackColor = False
        '
        'btnImpostazioni
        '
        Me.btnImpostazioni.BackColor = StileApp.SfondoContenuto
        Me.btnImpostazioni.FlatAppearance.BorderColor = StileApp.BordoLeggero
        Me.btnImpostazioni.FlatAppearance.BorderSize = 1
        Me.btnImpostazioni.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.btnImpostazioni.ForeColor = StileApp.TestoPrimario
        Me.btnImpostazioni.Location = New System.Drawing.Point(946, 7)
        Me.btnImpostazioni.Name = "btnImpostazioni"
        Me.btnImpostazioni.Size = StileApp.BottoneBarraSuperiore
        Me.btnImpostazioni.TabIndex = 6
        Me.btnImpostazioni.Text = "⚙ Impostazioni"
        Me.btnImpostazioni.UseVisualStyleBackColor = False
        '
        'pnlBordoBarraSuperiore
        '
        Me.pnlBordoBarraSuperiore.BackColor = StileApp.BordoLeggero
        Me.pnlBordoBarraSuperiore.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlBordoBarraSuperiore.Location = New System.Drawing.Point(0, 47)
        Me.pnlBordoBarraSuperiore.Name = "pnlBordoBarraSuperiore"
        Me.pnlBordoBarraSuperiore.Size = New System.Drawing.Size(1134, 1)
        Me.pnlBordoBarraSuperiore.TabIndex = 5
        '
        'pnlAreaCentrale
        '
        Me.pnlAreaCentrale.BackColor = StileApp.SfondoBase
        Me.pnlAreaCentrale.Controls.Add(Me.pnlMenu)
        Me.pnlAreaCentrale.Controls.Add(Me.pnlHome)
        Me.pnlAreaCentrale.Controls.Add(Me.pnlProfilo)
        Me.pnlAreaCentrale.Controls.Add(Me.pnlDialogo)
        Me.pnlAreaCentrale.Controls.Add(Me.pnlOpportunita)
        Me.pnlAreaCentrale.Controls.Add(Me.pnlDocumenti)
        Me.pnlAreaCentrale.Controls.Add(Me.pnlEmail)
        Me.pnlAreaCentrale.Controls.Add(Me.pnlRicerca)
        Me.pnlAreaCentrale.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlAreaCentrale.Location = New System.Drawing.Point(0, 48)
        Me.pnlAreaCentrale.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlAreaCentrale.Name = "pnlAreaCentrale"
        Me.pnlAreaCentrale.Size = New System.Drawing.Size(1134, 485)
        Me.pnlAreaCentrale.TabIndex = 1
        '
        'pnlMenu
        '
        Me.pnlMenu.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlMenu.Location = New System.Drawing.Point(0, 0)
        Me.pnlMenu.Name = "pnlMenu"
        Me.pnlMenu.Size = New System.Drawing.Size(1134, 485)
        Me.pnlMenu.TabIndex = 7
        Me.pnlMenu.Visible = False
        '
        'pnlHome
        '
        Me.pnlHome.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlHome.Location = New System.Drawing.Point(0, 0)
        Me.pnlHome.Name = "pnlHome"
        Me.pnlHome.Size = New System.Drawing.Size(1134, 485)
        Me.pnlHome.TabIndex = 5
        Me.pnlHome.Visible = False
        '
        'pnlProfilo
        '
        Me.pnlProfilo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlProfilo.Location = New System.Drawing.Point(0, 0)
        Me.pnlProfilo.Name = "pnlProfilo"
        Me.pnlProfilo.Size = New System.Drawing.Size(1134, 485)
        Me.pnlProfilo.TabIndex = 0
        Me.pnlProfilo.Visible = False
        '
        'pnlDialogo
        '
        Me.pnlDialogo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlDialogo.Location = New System.Drawing.Point(0, 0)
        Me.pnlDialogo.Name = "pnlDialogo"
        Me.pnlDialogo.Size = New System.Drawing.Size(1134, 485)
        Me.pnlDialogo.TabIndex = 1
        Me.pnlDialogo.Visible = False
        '
        'pnlOpportunita
        '
        Me.pnlOpportunita.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlOpportunita.Location = New System.Drawing.Point(0, 0)
        Me.pnlOpportunita.Name = "pnlOpportunita"
        Me.pnlOpportunita.Size = New System.Drawing.Size(1134, 485)
        Me.pnlOpportunita.TabIndex = 2
        Me.pnlOpportunita.Visible = False
        '
        'pnlDocumenti
        '
        Me.pnlDocumenti.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlDocumenti.Location = New System.Drawing.Point(0, 0)
        Me.pnlDocumenti.Name = "pnlDocumenti"
        Me.pnlDocumenti.Size = New System.Drawing.Size(1134, 485)
        Me.pnlDocumenti.TabIndex = 3
        Me.pnlDocumenti.Visible = False
        '
        'pnlEmail
        '
        Me.pnlEmail.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlEmail.Location = New System.Drawing.Point(0, 0)
        Me.pnlEmail.Name = "pnlEmail"
        Me.pnlEmail.Size = New System.Drawing.Size(1134, 485)
        Me.pnlEmail.TabIndex = 6
        Me.pnlEmail.Visible = False
        '
        'pnlRicerca
        '
        Me.pnlRicerca.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlRicerca.Location = New System.Drawing.Point(0, 0)
        Me.pnlRicerca.Name = "pnlRicerca"
        Me.pnlRicerca.Size = New System.Drawing.Size(1134, 485)
        Me.pnlRicerca.TabIndex = 4
        Me.pnlRicerca.Visible = False
        '
        'pnlFasciaInferiore
        '
        Me.pnlFasciaInferiore.BackColor = StileApp.SfondoBase
        Me.pnlFasciaInferiore.Controls.Add(Me.lblStato)
        Me.pnlFasciaInferiore.Controls.Add(Me.pnlBordoFasciaInferiore)
        Me.pnlFasciaInferiore.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlFasciaInferiore.Location = New System.Drawing.Point(0, 533)
        Me.pnlFasciaInferiore.Margin = New System.Windows.Forms.Padding(0)
        Me.pnlFasciaInferiore.Name = "pnlFasciaInferiore"
        Me.pnlFasciaInferiore.Size = New System.Drawing.Size(1134, 28)
        Me.pnlFasciaInferiore.Visible = False
        Me.pnlFasciaInferiore.TabIndex = 2
        '
        'lblStato
        '
        Me.lblStato.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lblStato.ForeColor = StileApp.TestoSecondario
        Me.lblStato.Location = New System.Drawing.Point(0, 1)
        Me.lblStato.Name = "lblStato"
        Me.lblStato.Padding = New System.Windows.Forms.Padding(273, 0, 12, 0)
        Me.lblStato.Size = New System.Drawing.Size(1134, 27)
        Me.lblStato.TabIndex = 0
        Me.lblStato.TextAlign = System.Drawing.ContentAlignment.MiddleLeft
        '
        'pnlBordoFasciaInferiore
        '
        Me.pnlBordoFasciaInferiore.BackColor = StileApp.BordoLeggero
        Me.pnlBordoFasciaInferiore.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlBordoFasciaInferiore.Location = New System.Drawing.Point(0, 0)
        Me.pnlBordoFasciaInferiore.Name = "pnlBordoFasciaInferiore"
        Me.pnlBordoFasciaInferiore.Size = New System.Drawing.Size(1134, 1)
        Me.pnlBordoFasciaInferiore.TabIndex = 1
        '
        'pnlLogo
        '
        Me.pnlLogo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.pnlLogo.BackColor = StileApp.SfondoBase
        Me.pnlLogo.Controls.Add(Me.picLogo)
        Me.pnlLogo.Controls.Add(Me.lblMarchio)
        Me.pnlLogo.Controls.Add(Me.lblVersione)
        Me.pnlLogo.Controls.Add(Me.lblCopyright)
        Me.pnlLogo.Location = New System.Drawing.Point(0, 345)
        Me.pnlLogo.Name = "pnlLogo"
        Me.pnlLogo.Size = New System.Drawing.Size(261, 216)
        Me.pnlLogo.TabIndex = 1
        '
        'picLogo
        '
        Me.picLogo.Location = New System.Drawing.Point(80, 14)
        Me.picLogo.Name = "picLogo"
        Me.picLogo.Size = New System.Drawing.Size(101, 101)
        Me.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.picLogo.TabIndex = 0
        Me.picLogo.TabStop = False
        '
        'lblMarchio
        '
        Me.lblMarchio.Font = StileApp.FontTitoloFinestra
        Me.lblMarchio.ForeColor = StileApp.TestoPrimario
        Me.lblMarchio.Location = New System.Drawing.Point(1, 123)
        Me.lblMarchio.Name = "lblMarchio"
        Me.lblMarchio.Size = New System.Drawing.Size(259, 30)
        Me.lblMarchio.TabIndex = 1
        Me.lblMarchio.Text = "AVIOLAB AI"
        Me.lblMarchio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblVersione
        '
        Me.lblVersione.Font = StileApp.FontDidascalia
        Me.lblVersione.ForeColor = StileApp.TestoSecondario
        Me.lblVersione.Location = New System.Drawing.Point(1, 155)
        Me.lblVersione.Name = "lblVersione"
        Me.lblVersione.Size = New System.Drawing.Size(259, 15)
        Me.lblVersione.TabIndex = 2
        Me.lblVersione.Text = "Ver. — · Pool —"
        Me.lblVersione.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblCopyright
        '
        Me.lblCopyright.Font = StileApp.FontDidascalia
        Me.lblCopyright.ForeColor = StileApp.TestoSecondario
        Me.lblCopyright.Location = New System.Drawing.Point(1, 172)
        Me.lblCopyright.Name = "lblCopyright"
        Me.lblCopyright.Size = New System.Drawing.Size(259, 15)
        Me.lblCopyright.TabIndex = 3
        Me.lblCopyright.Text = "©2026 Aviolab AI - Tutti i diritti riservati"
        Me.lblCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'FormPrincipale
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = StileApp.SfondoBase
        Me.ClientSize = New System.Drawing.Size(1134, 561)
        Me.Controls.Add(Me.pnlLogo)
        Me.Controls.Add(Me.tlpStruttura)
        Me.Font = StileApp.FontTesto
        Me.ForeColor = StileApp.TestoPrimario
        Me.MinimumSize = New System.Drawing.Size(1150, 600)
        Me.Name = "FormPrincipale"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "TrovaLavoro"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.tlpStruttura.ResumeLayout(False)
        Me.pnlBarraSuperiore.ResumeLayout(False)
        Me.pnlAreaCentrale.ResumeLayout(False)
        Me.pnlFasciaInferiore.ResumeLayout(False)
        Me.pnlLogo.ResumeLayout(False)
        CType(Me.picLogo, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents tlpStruttura As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents pnlBarraSuperiore As System.Windows.Forms.Panel
    Friend WithEvents btnMenu As System.Windows.Forms.Button
    Friend WithEvents btnHome As System.Windows.Forms.Button
    Friend WithEvents btnProfilo As System.Windows.Forms.Button
    Friend WithEvents btnRicerca As System.Windows.Forms.Button
    Friend WithEvents btnCandidatura As System.Windows.Forms.Button
    Friend WithEvents btnDocumenti As System.Windows.Forms.Button
    Friend WithEvents btnImpostazioni As System.Windows.Forms.Button
    Friend WithEvents pnlBordoBarraSuperiore As System.Windows.Forms.Panel
    Friend WithEvents pnlAreaCentrale As System.Windows.Forms.Panel
    Friend WithEvents pnlMenu As PannelloMenu
    Friend WithEvents pnlHome As PannelloHome
    Friend WithEvents pnlProfilo As PannelloProfilo
    Friend WithEvents pnlDialogo As PannelloDialogo
    Friend WithEvents pnlOpportunita As PannelloOpportunita
    Friend WithEvents pnlDocumenti As PannelloDocumenti
    Friend WithEvents pnlEmail As PannelloEmail
    Friend WithEvents pnlRicerca As PannelloRicerca
    Friend WithEvents ttSuggerimenti As System.Windows.Forms.ToolTip
    Friend WithEvents pnlFasciaInferiore As System.Windows.Forms.Panel
    Friend WithEvents lblStato As System.Windows.Forms.Label
    Friend WithEvents pnlBordoFasciaInferiore As System.Windows.Forms.Panel
    Friend WithEvents pnlLogo As System.Windows.Forms.Panel
    Friend WithEvents picLogo As System.Windows.Forms.PictureBox
    Friend WithEvents lblMarchio As System.Windows.Forms.Label
    Friend WithEvents lblVersione As System.Windows.Forms.Label
    Friend WithEvents lblCopyright As System.Windows.Forms.Label

End Class
