<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PannelloOpportunita
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
        Me.lblStatoOpportunita = New System.Windows.Forms.Label()
        Me.pnlIngresso = New System.Windows.Forms.Panel()
        Me.lblIncolla = New System.Windows.Forms.Label()
        Me.txtAnnuncio = New System.Windows.Forms.TextBox()
        Me.btnAnalizza = New System.Windows.Forms.Button()
        Me.lblPerchePento = New System.Windows.Forms.Label()
        Me.pnlCorpo = New System.Windows.Forms.Panel()
        Me.pnlValutazione = New System.Windows.Forms.Panel()
        Me.lvwGiudizi = New System.Windows.Forms.ListView()
        Me.colSegno = New System.Windows.Forms.ColumnHeader()
        Me.colRequisito = New System.Windows.Forms.ColumnHeader()
        Me.colPeso = New System.Windows.Forms.ColumnHeader()
        Me.colEsito = New System.Windows.Forms.ColumnHeader()
        Me.pnlPunteggio = New System.Windows.Forms.Panel()
        Me.lblStelle = New System.Windows.Forms.Label()
        Me.lblNota = New System.Windows.Forms.Label()
        Me.pnlSpiegazione = New System.Windows.Forms.Panel()
        Me.lblEtichettaSpiegazione = New System.Windows.Forms.Label()
        Me.txtSpiegazione = New System.Windows.Forms.TextBox()
        Me.pnlAnnuncioLetto = New System.Windows.Forms.Panel()
        Me.lblAnnuncioLetto = New System.Windows.Forms.Label()
        Me.txtAnnuncioLetto = New System.Windows.Forms.TextBox()
        Me.pnlAzioni = New System.Windows.Forms.Panel()
        Me.btnNuovoAnnuncio = New System.Windows.Forms.Button()
        Me.btnBrainstorm = New System.Windows.Forms.Button()
        Me.btnScarta = New System.Windows.Forms.Button()
        Me.btnGeneraDocumenti = New System.Windows.Forms.Button()
        Me.pnlIntestazione.SuspendLayout()
        Me.pnlIngresso.SuspendLayout()
        Me.pnlCorpo.SuspendLayout()
        Me.pnlValutazione.SuspendLayout()
        Me.pnlPunteggio.SuspendLayout()
        Me.pnlSpiegazione.SuspendLayout()
        Me.pnlAnnuncioLetto.SuspendLayout()
        Me.pnlAzioni.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlIntestazione
        '
        Me.pnlIntestazione.Controls.Add(Me.lblTitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblSottotitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblStatoOpportunita)
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
        Me.lblTitolo.Text = "Opportunità"
        '
        'lblSottotitolo
        '
        Me.lblSottotitolo.Font = StileApp.FontDidascalia
        Me.lblSottotitolo.ForeColor = StileApp.TestoSecondario
        Me.lblSottotitolo.Location = New System.Drawing.Point(2, 32)
        Me.lblSottotitolo.Name = "lblSottotitolo"
        Me.lblSottotitolo.Size = New System.Drawing.Size(700, 18)
        Me.lblSottotitolo.TabIndex = 1
        Me.lblSottotitolo.Text = "Passo 1 di 2 — incolla un annuncio e lo confronto con il tuo profilo."
        '
        'lblStatoOpportunita
        '
        Me.lblStatoOpportunita.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatoOpportunita.Font = StileApp.FontDidascalia
        Me.lblStatoOpportunita.ForeColor = StileApp.TestoSecondario
        Me.lblStatoOpportunita.Location = New System.Drawing.Point(706, 4)
        Me.lblStatoOpportunita.Name = "lblStatoOpportunita"
        Me.lblStatoOpportunita.Size = New System.Drawing.Size(400, 46)
        Me.lblStatoOpportunita.TabIndex = 2
        Me.lblStatoOpportunita.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'pnlIngresso
        '
        ' La fascia d'ingresso: fino a T5 è l'unica porta da cui un annuncio entra, e
        ' anche dopo resta la strada di chi ha in mano un testo e basta (cap. 03.6).
        ' A confronto avvenuto si richiude, per non rubare spazio ai giudizi.
        Me.pnlIngresso.Controls.Add(Me.lblIncolla)
        Me.pnlIngresso.Controls.Add(Me.txtAnnuncio)
        Me.pnlIngresso.Controls.Add(Me.btnAnalizza)
        Me.pnlIngresso.Controls.Add(Me.lblPerchePento)
        Me.pnlIngresso.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlIngresso.Location = New System.Drawing.Point(14, 74)
        Me.pnlIngresso.Name = "pnlIngresso"
        Me.pnlIngresso.Size = New System.Drawing.Size(1106, 148)
        Me.pnlIngresso.TabIndex = 1
        '
        'lblIncolla
        '
        Me.lblIncolla.Font = StileApp.FontTitoloGruppo
        Me.lblIncolla.ForeColor = StileApp.RossoTitoli
        Me.lblIncolla.Location = New System.Drawing.Point(0, 0)
        Me.lblIncolla.Name = "lblIncolla"
        Me.lblIncolla.Size = New System.Drawing.Size(500, 18)
        Me.lblIncolla.TabIndex = 0
        Me.lblIncolla.Text = "Incolla qui il testo dell'annuncio"
        '
        'txtAnnuncio
        '
        Me.txtAnnuncio.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtAnnuncio.Location = New System.Drawing.Point(0, 22)
        Me.txtAnnuncio.Multiline = True
        Me.txtAnnuncio.Name = "txtAnnuncio"
        Me.txtAnnuncio.PlaceholderText = "Copia il testo dell'annuncio dal portale o dall'email e incollalo qui…"
        Me.txtAnnuncio.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtAnnuncio.Size = New System.Drawing.Size(984, 112)
        Me.txtAnnuncio.TabIndex = 1
        '
        'btnAnalizza
        '
        Me.btnAnalizza.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnAnalizza.Location = New System.Drawing.Point(996, 22)
        Me.btnAnalizza.Name = "btnAnalizza"
        Me.btnAnalizza.Size = New System.Drawing.Size(110, 32)
        Me.btnAnalizza.TabIndex = 2
        Me.btnAnalizza.Text = "Analizza"
        '
        'lblPerchePento
        '
        ' Il motivo per cui «Analizza» non si può premere sta *sotto «Analizza»*: la
        ' barra di stato del pannello è all'angolo opposto, e un avviso lontano dal
        ' bottone che riguarda non viene letto (cap. 03.8).
        Me.lblPerchePento.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblPerchePento.Font = StileApp.FontDidascalia
        Me.lblPerchePento.ForeColor = StileApp.TestoSecondario
        Me.lblPerchePento.Location = New System.Drawing.Point(996, 58)
        Me.lblPerchePento.Name = "lblPerchePento"
        Me.lblPerchePento.Size = New System.Drawing.Size(110, 76)
        Me.lblPerchePento.TabIndex = 3
        '
        'pnlCorpo
        '
        Me.pnlCorpo.Controls.Add(Me.pnlValutazione)
        Me.pnlCorpo.Controls.Add(Me.pnlAnnuncioLetto)
        Me.pnlCorpo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCorpo.Location = New System.Drawing.Point(14, 222)
        Me.pnlCorpo.Name = "pnlCorpo"
        Me.pnlCorpo.Size = New System.Drawing.Size(1106, 460)
        Me.pnlCorpo.TabIndex = 2
        '
        'pnlValutazione
        '
        Me.pnlValutazione.Controls.Add(Me.lvwGiudizi)
        Me.pnlValutazione.Controls.Add(Me.pnlPunteggio)
        Me.pnlValutazione.Controls.Add(Me.pnlSpiegazione)
        Me.pnlValutazione.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlValutazione.Location = New System.Drawing.Point(380, 0)
        Me.pnlValutazione.Name = "pnlValutazione"
        Me.pnlValutazione.Size = New System.Drawing.Size(726, 460)
        Me.pnlValutazione.TabIndex = 1
        '
        'lvwGiudizi
        '
        ' L'elenco dei giudizi è una lista di sistema, non dei controlli creati a runtime
        ' (cap. 03.1, punto 6): a cambiare sono le righe, mai la struttura.
        Me.lvwGiudizi.BackColor = StileApp.SfondoContenuto
        Me.lvwGiudizi.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvwGiudizi.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colSegno, Me.colRequisito, Me.colPeso, Me.colEsito})
        Me.lvwGiudizi.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvwGiudizi.Font = StileApp.FontTesto
        Me.lvwGiudizi.FullRowSelect = True
        Me.lvwGiudizi.HideSelection = False
        Me.lvwGiudizi.Location = New System.Drawing.Point(0, 76)
        Me.lvwGiudizi.MultiSelect = False
        Me.lvwGiudizi.Name = "lvwGiudizi"
        Me.lvwGiudizi.Size = New System.Drawing.Size(726, 252)
        Me.lvwGiudizi.TabIndex = 1
        Me.lvwGiudizi.UseCompatibleStateImageBehavior = False
        Me.lvwGiudizi.View = System.Windows.Forms.View.Details
        '
        'colSegno
        '
        Me.colSegno.Text = ""
        Me.colSegno.Width = 34
        '
        'colRequisito
        '
        Me.colRequisito.Text = "Requisito"
        Me.colRequisito.Width = 380
        '
        'colPeso
        '
        Me.colPeso.Text = "Peso"
        Me.colPeso.Width = 130
        '
        'colEsito
        '
        Me.colEsito.Text = "Esito"
        Me.colEsito.Width = 160
        '
        'pnlPunteggio
        '
        Me.pnlPunteggio.Controls.Add(Me.lblStelle)
        Me.pnlPunteggio.Controls.Add(Me.lblNota)
        Me.pnlPunteggio.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlPunteggio.Location = New System.Drawing.Point(0, 0)
        Me.pnlPunteggio.Name = "pnlPunteggio"
        Me.pnlPunteggio.Size = New System.Drawing.Size(726, 76)
        Me.pnlPunteggio.TabIndex = 0
        '
        'lblStelle
        '
        ' Le stelle sono la prima cosa che si guarda: grandi, in cima (cap. 03.6).
        Me.lblStelle.Font = StileApp.FontTitoloFinestra
        Me.lblStelle.ForeColor = StileApp.TestoPrimario
        Me.lblStelle.Location = New System.Drawing.Point(0, 0)
        Me.lblStelle.Name = "lblStelle"
        Me.lblStelle.Size = New System.Drawing.Size(500, 32)
        Me.lblStelle.TabIndex = 0
        '
        'lblNota
        '
        Me.lblNota.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right)), System.Windows.Forms.AnchorStyles)
        Me.lblNota.Font = StileApp.FontDidascalia
        Me.lblNota.ForeColor = StileApp.TestoSecondario
        Me.lblNota.Location = New System.Drawing.Point(2, 34)
        Me.lblNota.Name = "lblNota"
        Me.lblNota.Size = New System.Drawing.Size(724, 38)
        Me.lblNota.TabIndex = 1
        '
        'pnlSpiegazione
        '
        Me.pnlSpiegazione.Controls.Add(Me.txtSpiegazione)
        Me.pnlSpiegazione.Controls.Add(Me.lblEtichettaSpiegazione)
        Me.pnlSpiegazione.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlSpiegazione.Location = New System.Drawing.Point(0, 328)
        Me.pnlSpiegazione.Name = "pnlSpiegazione"
        Me.pnlSpiegazione.Padding = New System.Windows.Forms.Padding(0, 8, 0, 0)
        Me.pnlSpiegazione.Size = New System.Drawing.Size(726, 132)
        Me.pnlSpiegazione.TabIndex = 2
        '
        'lblEtichettaSpiegazione
        '
        Me.lblEtichettaSpiegazione.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblEtichettaSpiegazione.Font = StileApp.FontTitoloGruppo
        Me.lblEtichettaSpiegazione.ForeColor = StileApp.RossoTitoli
        Me.lblEtichettaSpiegazione.Location = New System.Drawing.Point(0, 8)
        Me.lblEtichettaSpiegazione.Name = "lblEtichettaSpiegazione"
        Me.lblEtichettaSpiegazione.Size = New System.Drawing.Size(726, 18)
        Me.lblEtichettaSpiegazione.TabIndex = 0
        Me.lblEtichettaSpiegazione.Text = "Lettura d'insieme"
        '
        'txtSpiegazione
        '
        Me.txtSpiegazione.BackColor = StileApp.SfondoContenuto
        Me.txtSpiegazione.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtSpiegazione.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtSpiegazione.Multiline = True
        Me.txtSpiegazione.Name = "txtSpiegazione"
        Me.txtSpiegazione.ReadOnly = True
        Me.txtSpiegazione.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtSpiegazione.Size = New System.Drawing.Size(726, 106)
        Me.txtSpiegazione.TabIndex = 1
        '
        'pnlAnnuncioLetto
        '
        Me.pnlAnnuncioLetto.Controls.Add(Me.txtAnnuncioLetto)
        Me.pnlAnnuncioLetto.Controls.Add(Me.lblAnnuncioLetto)
        Me.pnlAnnuncioLetto.Dock = System.Windows.Forms.DockStyle.Left
        Me.pnlAnnuncioLetto.Location = New System.Drawing.Point(0, 0)
        Me.pnlAnnuncioLetto.Name = "pnlAnnuncioLetto"
        Me.pnlAnnuncioLetto.Padding = New System.Windows.Forms.Padding(0, 0, 12, 0)
        Me.pnlAnnuncioLetto.Size = New System.Drawing.Size(380, 460)
        Me.pnlAnnuncioLetto.TabIndex = 0
        '
        'lblAnnuncioLetto
        '
        Me.lblAnnuncioLetto.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblAnnuncioLetto.Font = StileApp.FontTitoloGruppo
        Me.lblAnnuncioLetto.ForeColor = StileApp.RossoTitoli
        Me.lblAnnuncioLetto.Location = New System.Drawing.Point(0, 0)
        Me.lblAnnuncioLetto.Name = "lblAnnuncioLetto"
        Me.lblAnnuncioLetto.Size = New System.Drawing.Size(368, 18)
        Me.lblAnnuncioLetto.TabIndex = 0
        Me.lblAnnuncioLetto.Text = "L'annuncio come l'ho capito"
        '
        'txtAnnuncioLetto
        '
        Me.txtAnnuncioLetto.BackColor = StileApp.SfondoContenuto
        Me.txtAnnuncioLetto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtAnnuncioLetto.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtAnnuncioLetto.Multiline = True
        Me.txtAnnuncioLetto.Name = "txtAnnuncioLetto"
        Me.txtAnnuncioLetto.ReadOnly = True
        Me.txtAnnuncioLetto.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtAnnuncioLetto.Size = New System.Drawing.Size(368, 442)
        Me.txtAnnuncioLetto.TabIndex = 1
        '
        'pnlAzioni
        '
        Me.pnlAzioni.Controls.Add(Me.btnNuovoAnnuncio)
        Me.pnlAzioni.Controls.Add(Me.btnBrainstorm)
        Me.pnlAzioni.Controls.Add(Me.btnScarta)
        Me.pnlAzioni.Controls.Add(Me.btnGeneraDocumenti)
        Me.pnlAzioni.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlAzioni.Location = New System.Drawing.Point(14, 682)
        Me.pnlAzioni.Name = "pnlAzioni"
        Me.pnlAzioni.Size = New System.Drawing.Size(1106, 64)
        Me.pnlAzioni.TabIndex = 3
        '
        'btnNuovoAnnuncio
        '
        Me.btnNuovoAnnuncio.Location = New System.Drawing.Point(0, 18)
        Me.btnNuovoAnnuncio.Name = "btnNuovoAnnuncio"
        Me.btnNuovoAnnuncio.Size = New System.Drawing.Size(130, 32)
        Me.btnNuovoAnnuncio.TabIndex = 0
        Me.btnNuovoAnnuncio.Text = "Nuovo annuncio"
        '
        'btnBrainstorm
        '
        Me.btnBrainstorm.Location = New System.Drawing.Point(142, 18)
        Me.btnBrainstorm.Name = "btnBrainstorm"
        Me.btnBrainstorm.Size = New System.Drawing.Size(130, 32)
        Me.btnBrainstorm.TabIndex = 1
        Me.btnBrainstorm.Text = "Brainstorm"
        '
        'btnScarta
        '
        Me.btnScarta.Location = New System.Drawing.Point(284, 18)
        Me.btnScarta.Name = "btnScarta"
        Me.btnScarta.Size = New System.Drawing.Size(110, 32)
        Me.btnScarta.TabIndex = 2
        Me.btnScarta.Text = "Scarta"
        '
        'btnGeneraDocumenti
        '
        Me.btnGeneraDocumenti.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnGeneraDocumenti.Location = New System.Drawing.Point(916, 18)
        Me.btnGeneraDocumenti.Name = "btnGeneraDocumenti"
        Me.btnGeneraDocumenti.Size = New System.Drawing.Size(190, 32)
        Me.btnGeneraDocumenti.TabIndex = 3
        Me.btnGeneraDocumenti.Text = "Genera CV + lettera"
        '
        'PannelloOpportunita
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = StileApp.SfondoBase
        Me.Controls.Add(Me.pnlCorpo)
        Me.Controls.Add(Me.pnlIngresso)
        Me.Controls.Add(Me.pnlIntestazione)
        Me.Controls.Add(Me.pnlAzioni)
        Me.Font = StileApp.FontTesto
        Me.ForeColor = StileApp.TestoPrimario
        Me.Name = "PannelloOpportunita"
        Me.Padding = New System.Windows.Forms.Padding(14)
        Me.Size = New System.Drawing.Size(1134, 760)
        Me.pnlIntestazione.ResumeLayout(False)
        Me.pnlIngresso.ResumeLayout(False)
        Me.pnlIngresso.PerformLayout()
        Me.pnlCorpo.ResumeLayout(False)
        Me.pnlValutazione.ResumeLayout(False)
        Me.pnlPunteggio.ResumeLayout(False)
        Me.pnlSpiegazione.ResumeLayout(False)
        Me.pnlSpiegazione.PerformLayout()
        Me.pnlAnnuncioLetto.ResumeLayout(False)
        Me.pnlAnnuncioLetto.PerformLayout()
        Me.pnlAzioni.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents pnlIntestazione As System.Windows.Forms.Panel
    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSottotitolo As System.Windows.Forms.Label
    Friend WithEvents lblStatoOpportunita As System.Windows.Forms.Label
    Friend WithEvents pnlIngresso As System.Windows.Forms.Panel
    Friend WithEvents lblIncolla As System.Windows.Forms.Label
    Friend WithEvents txtAnnuncio As System.Windows.Forms.TextBox
    Friend WithEvents btnAnalizza As System.Windows.Forms.Button
    Friend WithEvents lblPerchePento As System.Windows.Forms.Label
    Friend WithEvents pnlCorpo As System.Windows.Forms.Panel
    Friend WithEvents pnlAnnuncioLetto As System.Windows.Forms.Panel
    Friend WithEvents lblAnnuncioLetto As System.Windows.Forms.Label
    Friend WithEvents txtAnnuncioLetto As System.Windows.Forms.TextBox
    Friend WithEvents pnlValutazione As System.Windows.Forms.Panel
    Friend WithEvents pnlPunteggio As System.Windows.Forms.Panel
    Friend WithEvents lblStelle As System.Windows.Forms.Label
    Friend WithEvents lblNota As System.Windows.Forms.Label
    Friend WithEvents lvwGiudizi As System.Windows.Forms.ListView
    Friend WithEvents colSegno As System.Windows.Forms.ColumnHeader
    Friend WithEvents colRequisito As System.Windows.Forms.ColumnHeader
    Friend WithEvents colPeso As System.Windows.Forms.ColumnHeader
    Friend WithEvents colEsito As System.Windows.Forms.ColumnHeader
    Friend WithEvents pnlSpiegazione As System.Windows.Forms.Panel
    Friend WithEvents lblEtichettaSpiegazione As System.Windows.Forms.Label
    Friend WithEvents txtSpiegazione As System.Windows.Forms.TextBox
    Friend WithEvents pnlAzioni As System.Windows.Forms.Panel
    Friend WithEvents btnNuovoAnnuncio As System.Windows.Forms.Button
    Friend WithEvents btnBrainstorm As System.Windows.Forms.Button
    Friend WithEvents btnScarta As System.Windows.Forms.Button
    Friend WithEvents btnGeneraDocumenti As System.Windows.Forms.Button

End Class
