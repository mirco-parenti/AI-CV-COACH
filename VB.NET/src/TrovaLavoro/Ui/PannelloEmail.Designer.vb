<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PannelloEmail
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
        Me.lblStatoEmail = New System.Windows.Forms.Label()
        Me.pnlCampi = New System.Windows.Forms.Panel()
        Me.lblDestinatario = New System.Windows.Forms.Label()
        Me.txtDestinatario = New System.Windows.Forms.TextBox()
        Me.lblOggetto = New System.Windows.Forms.Label()
        Me.txtOggetto = New System.Windows.Forms.TextBox()
        Me.pnlCorpo = New System.Windows.Forms.Panel()
        Me.pnlTesto = New System.Windows.Forms.Panel()
        Me.lblCorpo = New System.Windows.Forms.Label()
        Me.txtCorpo = New System.Windows.Forms.TextBox()
        Me.pnlAllegati = New System.Windows.Forms.Panel()
        Me.lblAllegati = New System.Windows.Forms.Label()
        Me.lstAllegati = New System.Windows.Forms.CheckedListBox()
        Me.lblNotaAllegati = New System.Windows.Forms.Label()
        Me.pnlAzioni = New System.Windows.Forms.Panel()
        Me.btnTornaAiDocumenti = New System.Windows.Forms.Button()
        Me.btnRiscrivi = New System.Windows.Forms.Button()
        Me.btnHoSpedito = New System.Windows.Forms.Button()
        Me.btnPreparaEmail = New System.Windows.Forms.Button()
        Me.pnlIntestazione.SuspendLayout()
        Me.pnlCampi.SuspendLayout()
        Me.pnlCorpo.SuspendLayout()
        Me.pnlTesto.SuspendLayout()
        Me.pnlAllegati.SuspendLayout()
        Me.pnlAzioni.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlIntestazione
        '
        Me.pnlIntestazione.Controls.Add(Me.lblTitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblSottotitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblStatoEmail)
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
        Me.lblTitolo.Text = "L'email di candidatura"
        '
        'lblSottotitolo
        '
        Me.lblSottotitolo.Font = StileApp.FontDidascalia
        Me.lblSottotitolo.ForeColor = StileApp.TestoSecondario
        Me.lblSottotitolo.Location = New System.Drawing.Point(2, 32)
        Me.lblSottotitolo.Name = "lblSottotitolo"
        Me.lblSottotitolo.Size = New System.Drawing.Size(760, 18)
        Me.lblSottotitolo.TabIndex = 1
        Me.lblSottotitolo.Text = "Rileggila e correggila: a spedire sarà il tuo programma di posta, non questo."
        '
        'lblStatoEmail
        '
        Me.lblStatoEmail.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatoEmail.Font = StileApp.FontDidascalia
        Me.lblStatoEmail.ForeColor = StileApp.TestoSecondario
        Me.lblStatoEmail.Location = New System.Drawing.Point(600, 0)
        Me.lblStatoEmail.Name = "lblStatoEmail"
        Me.lblStatoEmail.Size = New System.Drawing.Size(506, 32)
        Me.lblStatoEmail.TextAlign = System.Drawing.ContentAlignment.TopRight
        Me.lblStatoEmail.TabIndex = 2
        '
        'pnlCampi
        '
        Me.pnlCampi.Controls.Add(Me.lblDestinatario)
        Me.pnlCampi.Controls.Add(Me.txtDestinatario)
        Me.pnlCampi.Controls.Add(Me.lblOggetto)
        Me.pnlCampi.Controls.Add(Me.txtOggetto)
        Me.pnlCampi.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlCampi.Location = New System.Drawing.Point(14, 74)
        Me.pnlCampi.Name = "pnlCampi"
        Me.pnlCampi.Size = New System.Drawing.Size(1106, 76)
        Me.pnlCampi.TabIndex = 1
        '
        'lblDestinatario
        '
        Me.lblDestinatario.Location = New System.Drawing.Point(0, 8)
        Me.lblDestinatario.Name = "lblDestinatario"
        Me.lblDestinatario.Size = New System.Drawing.Size(90, 20)
        Me.lblDestinatario.TabIndex = 0
        Me.lblDestinatario.Text = "A"
        '
        'txtDestinatario
        '
        Me.txtDestinatario.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtDestinatario.Location = New System.Drawing.Point(96, 4)
        Me.txtDestinatario.Name = "txtDestinatario"
        Me.txtDestinatario.Size = New System.Drawing.Size(1010, 23)
        Me.txtDestinatario.TabIndex = 1
        '
        'lblOggetto
        '
        Me.lblOggetto.Location = New System.Drawing.Point(0, 44)
        Me.lblOggetto.Name = "lblOggetto"
        Me.lblOggetto.Size = New System.Drawing.Size(90, 20)
        Me.lblOggetto.TabIndex = 2
        Me.lblOggetto.Text = "Oggetto"
        '
        'txtOggetto
        '
        Me.txtOggetto.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtOggetto.Location = New System.Drawing.Point(96, 40)
        Me.txtOggetto.Name = "txtOggetto"
        Me.txtOggetto.Size = New System.Drawing.Size(1010, 23)
        Me.txtOggetto.TabIndex = 3
        '
        'pnlCorpo
        '
        Me.pnlCorpo.Controls.Add(Me.pnlTesto)
        Me.pnlCorpo.Controls.Add(Me.pnlAllegati)
        Me.pnlCorpo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCorpo.Location = New System.Drawing.Point(14, 150)
        Me.pnlCorpo.Name = "pnlCorpo"
        Me.pnlCorpo.Size = New System.Drawing.Size(1106, 532)
        Me.pnlCorpo.TabIndex = 2
        '
        'pnlTesto
        '
        Me.pnlTesto.Controls.Add(Me.lblCorpo)
        Me.pnlTesto.Controls.Add(Me.txtCorpo)
        Me.pnlTesto.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlTesto.Location = New System.Drawing.Point(0, 0)
        Me.pnlTesto.Name = "pnlTesto"
        Me.pnlTesto.Size = New System.Drawing.Size(746, 532)
        Me.pnlTesto.TabIndex = 0
        '
        'lblCorpo
        '
        Me.lblCorpo.Font = StileApp.FontTitoloGruppo
        Me.lblCorpo.ForeColor = StileApp.RossoTitoli
        Me.lblCorpo.Location = New System.Drawing.Point(0, 0)
        Me.lblCorpo.Name = "lblCorpo"
        Me.lblCorpo.Size = New System.Drawing.Size(300, 18)
        Me.lblCorpo.TabIndex = 0
        Me.lblCorpo.Text = "Il messaggio"
        '
        'txtCorpo
        '
        Me.txtCorpo.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCorpo.BackColor = StileApp.SfondoContenuto
        Me.txtCorpo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtCorpo.Location = New System.Drawing.Point(0, 22)
        Me.txtCorpo.Multiline = True
        Me.txtCorpo.Name = "txtCorpo"
        Me.txtCorpo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtCorpo.Size = New System.Drawing.Size(734, 500)
        Me.txtCorpo.TabIndex = 1
        '
        'pnlAllegati
        '
        Me.pnlAllegati.Controls.Add(Me.lblAllegati)
        Me.pnlAllegati.Controls.Add(Me.lstAllegati)
        Me.pnlAllegati.Controls.Add(Me.lblNotaAllegati)
        Me.pnlAllegati.Dock = System.Windows.Forms.DockStyle.Right
        Me.pnlAllegati.Location = New System.Drawing.Point(746, 0)
        Me.pnlAllegati.Name = "pnlAllegati"
        Me.pnlAllegati.Size = New System.Drawing.Size(360, 532)
        Me.pnlAllegati.TabIndex = 1
        '
        'lblAllegati
        '
        Me.lblAllegati.Font = StileApp.FontTitoloGruppo
        Me.lblAllegati.ForeColor = StileApp.RossoTitoli
        Me.lblAllegati.Location = New System.Drawing.Point(0, 0)
        Me.lblAllegati.Name = "lblAllegati"
        Me.lblAllegati.Size = New System.Drawing.Size(300, 18)
        Me.lblAllegati.TabIndex = 0
        Me.lblAllegati.Text = "Cosa allego"
        '
        'lstAllegati
        '
        Me.lstAllegati.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstAllegati.BackColor = StileApp.SfondoContenuto
        Me.lstAllegati.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lstAllegati.CheckOnClick = True
        Me.lstAllegati.IntegralHeight = False
        Me.lstAllegati.Location = New System.Drawing.Point(0, 22)
        Me.lstAllegati.Name = "lstAllegati"
        Me.lstAllegati.Size = New System.Drawing.Size(360, 460)
        Me.lstAllegati.TabIndex = 1
        '
        'lblNotaAllegati
        '
        Me.lblNotaAllegati.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblNotaAllegati.Font = StileApp.FontDidascalia
        Me.lblNotaAllegati.ForeColor = StileApp.TestoSecondario
        Me.lblNotaAllegati.Location = New System.Drawing.Point(0, 488)
        Me.lblNotaAllegati.Name = "lblNotaAllegati"
        Me.lblNotaAllegati.Size = New System.Drawing.Size(360, 40)
        Me.lblNotaAllegati.TabIndex = 2
        Me.lblNotaAllegati.Text = "Spunta quelli da mandare. Il messaggio li nomina: se li cambi, fallo riscrivere."
        '
        'pnlAzioni
        '
        Me.pnlAzioni.Controls.Add(Me.btnTornaAiDocumenti)
        Me.pnlAzioni.Controls.Add(Me.btnRiscrivi)
        Me.pnlAzioni.Controls.Add(Me.btnHoSpedito)
        Me.pnlAzioni.Controls.Add(Me.btnPreparaEmail)
        Me.pnlAzioni.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlAzioni.Location = New System.Drawing.Point(14, 682)
        Me.pnlAzioni.Name = "pnlAzioni"
        Me.pnlAzioni.Size = New System.Drawing.Size(1106, 64)
        Me.pnlAzioni.TabIndex = 3
        '
        'btnTornaAiDocumenti
        '
        Me.btnTornaAiDocumenti.Location = New System.Drawing.Point(0, 12)
        Me.btnTornaAiDocumenti.Name = "btnTornaAiDocumenti"
        Me.btnTornaAiDocumenti.Size = New System.Drawing.Size(190, 32)
        Me.btnTornaAiDocumenti.TabIndex = 0
        Me.btnTornaAiDocumenti.Text = "◀ Torna ai documenti"
        '
        'btnRiscrivi
        '
        Me.btnRiscrivi.Location = New System.Drawing.Point(202, 12)
        Me.btnRiscrivi.Name = "btnRiscrivi"
        Me.btnRiscrivi.Size = New System.Drawing.Size(190, 32)
        Me.btnRiscrivi.TabIndex = 1
        Me.btnRiscrivi.Text = "Fallo riscrivere"
        '
        'btnHoSpedito
        '
        Me.btnHoSpedito.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnHoSpedito.Location = New System.Drawing.Point(724, 12)
        Me.btnHoSpedito.Name = "btnHoSpedito"
        Me.btnHoSpedito.Size = New System.Drawing.Size(190, 32)
        Me.btnHoSpedito.TabIndex = 2
        Me.btnHoSpedito.Text = "L'ho spedita"
        '
        'btnPreparaEmail
        '
        Me.btnPreparaEmail.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPreparaEmail.Location = New System.Drawing.Point(926, 12)
        Me.btnPreparaEmail.Name = "btnPreparaEmail"
        Me.btnPreparaEmail.Size = New System.Drawing.Size(180, 32)
        Me.btnPreparaEmail.TabIndex = 3
        Me.btnPreparaEmail.Text = "Prepara l'email"
        '
        'PannelloEmail
        '
        Me.BackColor = StileApp.SfondoBase
        Me.Controls.Add(Me.pnlCorpo)
        Me.Controls.Add(Me.pnlCampi)
        Me.Controls.Add(Me.pnlIntestazione)
        Me.Controls.Add(Me.pnlAzioni)
        Me.Font = StileApp.FontTesto
        Me.ForeColor = StileApp.TestoPrimario
        Me.Name = "PannelloEmail"
        Me.Padding = New System.Windows.Forms.Padding(14)
        Me.Size = New System.Drawing.Size(1134, 760)
        Me.pnlIntestazione.ResumeLayout(False)
        Me.pnlCampi.ResumeLayout(False)
        Me.pnlCampi.PerformLayout()
        Me.pnlCorpo.ResumeLayout(False)
        Me.pnlTesto.ResumeLayout(False)
        Me.pnlTesto.PerformLayout()
        Me.pnlAllegati.ResumeLayout(False)
        Me.pnlAzioni.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents pnlIntestazione As System.Windows.Forms.Panel
    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSottotitolo As System.Windows.Forms.Label
    Friend WithEvents lblStatoEmail As System.Windows.Forms.Label
    Friend WithEvents pnlCampi As System.Windows.Forms.Panel
    Friend WithEvents lblDestinatario As System.Windows.Forms.Label
    Friend WithEvents txtDestinatario As System.Windows.Forms.TextBox
    Friend WithEvents lblOggetto As System.Windows.Forms.Label
    Friend WithEvents txtOggetto As System.Windows.Forms.TextBox
    Friend WithEvents pnlCorpo As System.Windows.Forms.Panel
    Friend WithEvents pnlTesto As System.Windows.Forms.Panel
    Friend WithEvents lblCorpo As System.Windows.Forms.Label
    Friend WithEvents txtCorpo As System.Windows.Forms.TextBox
    Friend WithEvents pnlAllegati As System.Windows.Forms.Panel
    Friend WithEvents lblAllegati As System.Windows.Forms.Label
    Friend WithEvents lstAllegati As System.Windows.Forms.CheckedListBox
    Friend WithEvents lblNotaAllegati As System.Windows.Forms.Label
    Friend WithEvents pnlAzioni As System.Windows.Forms.Panel
    Friend WithEvents btnTornaAiDocumenti As System.Windows.Forms.Button
    Friend WithEvents btnRiscrivi As System.Windows.Forms.Button
    Friend WithEvents btnHoSpedito As System.Windows.Forms.Button
    Friend WithEvents btnPreparaEmail As System.Windows.Forms.Button

End Class
