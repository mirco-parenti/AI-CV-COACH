<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PannelloDialogo
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
        Me.lblStatoDialogo = New System.Windows.Forms.Label()
        Me.pnlConversazione = New System.Windows.Forms.Panel()
        Me.pnlScorrimento = New System.Windows.Forms.Panel()
        Me.flpConversazione = New System.Windows.Forms.FlowLayoutPanel()
        Me.pnlRisposta = New System.Windows.Forms.Panel()
        Me.txtRisposta = New System.Windows.Forms.TextBox()
        Me.btnInvia = New System.Windows.Forms.Button()
        Me.btnScelta1 = New System.Windows.Forms.Button()
        Me.btnScelta2 = New System.Windows.Forms.Button()
        Me.btnScelta3 = New System.Windows.Forms.Button()
        Me.pnlAzioni = New System.Windows.Forms.Panel()
        Me.btnTornaAlProfilo = New System.Windows.Forms.Button()
        Me.btnRicomincia = New System.Windows.Forms.Button()
        Me.btnPortaNelProfilo = New System.Windows.Forms.Button()
        Me.pnlIntestazione.SuspendLayout()
        Me.pnlConversazione.SuspendLayout()
        Me.pnlScorrimento.SuspendLayout()
        Me.pnlRisposta.SuspendLayout()
        Me.pnlAzioni.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlIntestazione
        '
        Me.pnlIntestazione.Controls.Add(Me.lblTitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblSottotitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblStatoDialogo)
        Me.pnlIntestazione.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlIntestazione.Location = New System.Drawing.Point(14, 14)
        Me.pnlIntestazione.Name = "pnlIntestazione"
        Me.pnlIntestazione.Size = New System.Drawing.Size(1106, 60)
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
        Me.lblTitolo.Text = "Costruiamo il tuo profilo"
        '
        'lblSottotitolo
        '
        Me.lblSottotitolo.Font = StileApp.FontDidascalia
        Me.lblSottotitolo.ForeColor = StileApp.TestoSecondario
        Me.lblSottotitolo.Location = New System.Drawing.Point(2, 32)
        Me.lblSottotitolo.Name = "lblSottotitolo"
        Me.lblSottotitolo.Size = New System.Drawing.Size(700, 18)
        Me.lblSottotitolo.TabIndex = 1
        Me.lblSottotitolo.Text = "Un argomento alla volta: rispondi con parole tue, e prima di registrare qualcosa ti mostro cosa ho capito."
        '
        'lblStatoDialogo
        '
        Me.lblStatoDialogo.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatoDialogo.Font = StileApp.FontDidascalia
        Me.lblStatoDialogo.ForeColor = StileApp.TestoSecondario
        Me.lblStatoDialogo.Location = New System.Drawing.Point(706, 4)
        Me.lblStatoDialogo.Name = "lblStatoDialogo"
        Me.lblStatoDialogo.Size = New System.Drawing.Size(400, 46)
        Me.lblStatoDialogo.TabIndex = 2
        Me.lblStatoDialogo.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'pnlConversazione
        '
        ' Il bordo di 1 px è il pannello stesso: sfondo del colore del bordo e un
        ' padding di uno, con dentro la conversazione a tutta area.
        Me.pnlConversazione.BackColor = StileApp.BordoLeggero
        Me.pnlConversazione.Controls.Add(Me.pnlScorrimento)
        Me.pnlConversazione.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlConversazione.Location = New System.Drawing.Point(14, 74)
        Me.pnlConversazione.Name = "pnlConversazione"
        Me.pnlConversazione.Padding = New System.Windows.Forms.Padding(1)
        Me.pnlConversazione.Size = New System.Drawing.Size(1106, 512)
        Me.pnlConversazione.TabIndex = 1
        '
        'pnlScorrimento
        '
        ' A scorrere è questo pannello, non l'elenco delle bolle: un FlowLayoutPanel che
        ' scorre da sé tiene un conto sbagliato di quanto è alto il proprio contenuto —
        ' 24 px in meno, misurati — e l'ultima bolla resta tagliata proprio quando è
        ' quella da leggere.
        ' Il fondo è quello neutro delle finestre, non il bianco delle aree di lavoro:
        ' le bolle sono bianche, e su bianco non si vedrebbero.
        Me.pnlScorrimento.AutoScroll = True
        Me.pnlScorrimento.BackColor = StileApp.FondoPagina
        Me.pnlScorrimento.Controls.Add(Me.flpConversazione)
        Me.pnlScorrimento.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlScorrimento.Location = New System.Drawing.Point(1, 1)
        Me.pnlScorrimento.Name = "pnlScorrimento"
        Me.pnlScorrimento.Padding = New System.Windows.Forms.Padding(12)
        Me.pnlScorrimento.Size = New System.Drawing.Size(1104, 510)
        Me.pnlScorrimento.TabIndex = 0
        '
        'flpConversazione
        '
        ' Ancorato in alto e alto quanto le sue bolle: così il pannello che lo contiene
        ' sa esattamente quanto c'è da scorrere, e la larghezza si adatta da sola quando
        ' compare la barra.
        Me.flpConversazione.AutoSize = True
        Me.flpConversazione.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.flpConversazione.BackColor = StileApp.FondoPagina
        Me.flpConversazione.Dock = System.Windows.Forms.DockStyle.Top
        Me.flpConversazione.FlowDirection = System.Windows.Forms.FlowDirection.TopDown
        Me.flpConversazione.Location = New System.Drawing.Point(12, 12)
        Me.flpConversazione.Name = "flpConversazione"
        Me.flpConversazione.Size = New System.Drawing.Size(1080, 0)
        Me.flpConversazione.TabIndex = 0
        Me.flpConversazione.WrapContents = False
        '
        'pnlRisposta
        '
        Me.pnlRisposta.Controls.Add(Me.txtRisposta)
        Me.pnlRisposta.Controls.Add(Me.btnInvia)
        Me.pnlRisposta.Controls.Add(Me.btnScelta1)
        Me.pnlRisposta.Controls.Add(Me.btnScelta2)
        Me.pnlRisposta.Controls.Add(Me.btnScelta3)
        Me.pnlRisposta.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlRisposta.Location = New System.Drawing.Point(14, 586)
        Me.pnlRisposta.Name = "pnlRisposta"
        Me.pnlRisposta.Size = New System.Drawing.Size(1106, 96)
        Me.pnlRisposta.TabIndex = 2
        '
        'txtRisposta
        '
        Me.txtRisposta.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtRisposta.BackColor = StileApp.FondoCasella
        Me.txtRisposta.Location = New System.Drawing.Point(0, 14)
        Me.txtRisposta.Multiline = True
        Me.txtRisposta.Name = "txtRisposta"
        Me.txtRisposta.PlaceholderText = "La tua risposta…"
        Me.txtRisposta.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtRisposta.Size = New System.Drawing.Size(984, 68)
        Me.txtRisposta.TabIndex = 0
        '
        'btnInvia
        '
        Me.btnInvia.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnInvia.Location = New System.Drawing.Point(996, 14)
        Me.btnInvia.Name = "btnInvia"
        Me.btnInvia.Size = New System.Drawing.Size(110, 32)
        Me.btnInvia.TabIndex = 1
        Me.btnInvia.Text = "Invia"
        '
        'btnScelta1
        '
        ' I bottoni delle scelte sono tre e stanno qui, fissi: le mosse del dialogo non
        ' ne offrono mai di più (cap. 03.1, punto 6 — la struttura non si crea a runtime).
        ' Etichetta, livello e posizione cambiano a ogni mossa; l'ingombro no.
        Me.btnScelta1.AutoSize = True
        Me.btnScelta1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.btnScelta1.Location = New System.Drawing.Point(0, 14)
        Me.btnScelta1.MinimumSize = New System.Drawing.Size(130, 32)
        Me.btnScelta1.Name = "btnScelta1"
        Me.btnScelta1.Size = New System.Drawing.Size(130, 32)
        Me.btnScelta1.TabIndex = 2
        Me.btnScelta1.Visible = False
        '
        'btnScelta2
        '
        Me.btnScelta2.AutoSize = True
        Me.btnScelta2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.btnScelta2.Location = New System.Drawing.Point(142, 14)
        Me.btnScelta2.MinimumSize = New System.Drawing.Size(130, 32)
        Me.btnScelta2.Name = "btnScelta2"
        Me.btnScelta2.Size = New System.Drawing.Size(130, 32)
        Me.btnScelta2.TabIndex = 3
        Me.btnScelta2.Visible = False
        '
        'btnScelta3
        '
        Me.btnScelta3.AutoSize = True
        Me.btnScelta3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
        Me.btnScelta3.Location = New System.Drawing.Point(284, 14)
        Me.btnScelta3.MinimumSize = New System.Drawing.Size(130, 32)
        Me.btnScelta3.Name = "btnScelta3"
        Me.btnScelta3.Size = New System.Drawing.Size(130, 32)
        Me.btnScelta3.TabIndex = 4
        Me.btnScelta3.Visible = False
        '
        'pnlAzioni
        '
        Me.pnlAzioni.Controls.Add(Me.btnTornaAlProfilo)
        Me.pnlAzioni.Controls.Add(Me.btnRicomincia)
        Me.pnlAzioni.Controls.Add(Me.btnPortaNelProfilo)
        Me.pnlAzioni.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlAzioni.Location = New System.Drawing.Point(14, 682)
        Me.pnlAzioni.Name = "pnlAzioni"
        Me.pnlAzioni.Size = New System.Drawing.Size(1106, 64)
        Me.pnlAzioni.TabIndex = 3
        '
        'btnTornaAlProfilo
        '
        Me.btnTornaAlProfilo.Location = New System.Drawing.Point(0, 18)
        Me.btnTornaAlProfilo.Name = "btnTornaAlProfilo"
        Me.btnTornaAlProfilo.Size = New System.Drawing.Size(130, 32)
        Me.btnTornaAlProfilo.TabIndex = 0
        Me.btnTornaAlProfilo.Text = "Torna al profilo"
        '
        'btnRicomincia
        '
        Me.btnRicomincia.Location = New System.Drawing.Point(142, 18)
        Me.btnRicomincia.Name = "btnRicomincia"
        Me.btnRicomincia.Size = New System.Drawing.Size(130, 32)
        Me.btnRicomincia.TabIndex = 1
        Me.btnRicomincia.Text = "Ricomincia"
        '
        'btnPortaNelProfilo
        '
        Me.btnPortaNelProfilo.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPortaNelProfilo.Location = New System.Drawing.Point(946, 18)
        Me.btnPortaNelProfilo.Name = "btnPortaNelProfilo"
        Me.btnPortaNelProfilo.Size = New System.Drawing.Size(160, 32)
        Me.btnPortaNelProfilo.TabIndex = 2
        Me.btnPortaNelProfilo.Text = "Porta nel profilo"
        '
        'PannelloDialogo
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = StileApp.FondoPagina
        Me.Controls.Add(Me.pnlConversazione)
        Me.Controls.Add(Me.pnlIntestazione)
        Me.Controls.Add(Me.pnlRisposta)
        Me.Controls.Add(Me.pnlAzioni)
        Me.Font = StileApp.FontTesto
        Me.ForeColor = StileApp.TestoPrimario
        Me.Name = "PannelloDialogo"
        Me.Padding = New System.Windows.Forms.Padding(14)
        Me.Size = New System.Drawing.Size(1134, 760)
        Me.pnlIntestazione.ResumeLayout(False)
        Me.pnlConversazione.ResumeLayout(False)
        Me.pnlScorrimento.ResumeLayout(False)
        Me.pnlScorrimento.PerformLayout()
        Me.pnlRisposta.ResumeLayout(False)
        Me.pnlRisposta.PerformLayout()
        Me.pnlAzioni.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents pnlIntestazione As System.Windows.Forms.Panel
    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSottotitolo As System.Windows.Forms.Label
    Friend WithEvents lblStatoDialogo As System.Windows.Forms.Label
    Friend WithEvents pnlConversazione As System.Windows.Forms.Panel
    Friend WithEvents pnlScorrimento As System.Windows.Forms.Panel
    Friend WithEvents flpConversazione As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents pnlRisposta As System.Windows.Forms.Panel
    Friend WithEvents txtRisposta As System.Windows.Forms.TextBox
    Friend WithEvents btnInvia As System.Windows.Forms.Button
    Friend WithEvents btnScelta1 As System.Windows.Forms.Button
    Friend WithEvents btnScelta2 As System.Windows.Forms.Button
    Friend WithEvents btnScelta3 As System.Windows.Forms.Button
    Friend WithEvents pnlAzioni As System.Windows.Forms.Panel
    Friend WithEvents btnTornaAlProfilo As System.Windows.Forms.Button
    Friend WithEvents btnRicomincia As System.Windows.Forms.Button
    Friend WithEvents btnPortaNelProfilo As System.Windows.Forms.Button

End Class
