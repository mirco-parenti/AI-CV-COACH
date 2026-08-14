<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PannelloHome
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
        Me.pnlProfiloInSintesi = New System.Windows.Forms.Panel()
        Me.lblEtichettaProfilo = New System.Windows.Forms.Label()
        Me.lblProfilo = New System.Windows.Forms.Label()
        Me.btnApriProfilo = New System.Windows.Forms.Button()
        Me.pnlFiltro = New System.Windows.Forms.Panel()
        Me.lblContatori = New System.Windows.Forms.Label()
        Me.lblStelle = New System.Windows.Forms.Label()
        Me.cboStelle = New System.Windows.Forms.ComboBox()
        Me.lblMostra = New System.Windows.Forms.Label()
        Me.cboMostra = New System.Windows.Forms.ComboBox()
        Me.pnlCorpo = New System.Windows.Forms.Panel()
        Me.lvwCoda = New System.Windows.Forms.ListView()
        Me.colMatch = New System.Windows.Forms.ColumnHeader()
        Me.colAzienda = New System.Windows.Forms.ColumnHeader()
        Me.colRuolo = New System.Windows.Forms.ColumnHeader()
        Me.colStato = New System.Windows.Forms.ColumnHeader()
        Me.colFonte = New System.Windows.Forms.ColumnHeader()
        Me.colQuando = New System.Windows.Forms.ColumnHeader()
        Me.pnlAzioni = New System.Windows.Forms.Panel()
        Me.btnApriCandidatura = New System.Windows.Forms.Button()
        Me.btnNuovaRicerca = New System.Windows.Forms.Button()
        Me.btnAggiornaProfilo = New System.Windows.Forms.Button()
        Me.btnEsportaRegistro = New System.Windows.Forms.Button()
        Me.lblStatoHome = New System.Windows.Forms.Label()
        Me.pnlIntestazione.SuspendLayout()
        Me.pnlProfiloInSintesi.SuspendLayout()
        Me.pnlFiltro.SuspendLayout()
        Me.pnlCorpo.SuspendLayout()
        Me.pnlAzioni.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlIntestazione
        '
        Me.pnlIntestazione.Controls.Add(Me.lblTitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblSottotitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblStatoHome)
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
        Me.lblTitolo.Text = "Home"
        '
        'lblSottotitolo
        '
        Me.lblSottotitolo.Font = StileApp.FontDidascalia
        Me.lblSottotitolo.ForeColor = StileApp.TestoSecondario
        Me.lblSottotitolo.Location = New System.Drawing.Point(2, 32)
        Me.lblSottotitolo.Name = "lblSottotitolo"
        Me.lblSottotitolo.Size = New System.Drawing.Size(900, 18)
        Me.lblSottotitolo.TabIndex = 1
        Me.lblSottotitolo.Text = "A che punto sei, e da dove riprendere."
        '
        'pnlProfiloInSintesi
        '
        ' La prima domanda del cruscotto (cap. 03.6): il profilo esiste, e di quando è?
        ' Senza di lui non si fa nient'altro, quindi sta in cima e non in fondo.
        Me.pnlProfiloInSintesi.Controls.Add(Me.lblEtichettaProfilo)
        Me.pnlProfiloInSintesi.Controls.Add(Me.lblProfilo)
        Me.pnlProfiloInSintesi.Controls.Add(Me.btnApriProfilo)
        Me.pnlProfiloInSintesi.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlProfiloInSintesi.Location = New System.Drawing.Point(14, 70)
        Me.pnlProfiloInSintesi.Name = "pnlProfiloInSintesi"
        Me.pnlProfiloInSintesi.Size = New System.Drawing.Size(1106, 62)
        Me.pnlProfiloInSintesi.TabIndex = 1
        '
        'lblEtichettaProfilo
        '
        Me.lblEtichettaProfilo.Font = StileApp.FontTitoloGruppo
        Me.lblEtichettaProfilo.ForeColor = StileApp.RossoTitoli
        Me.lblEtichettaProfilo.Location = New System.Drawing.Point(0, 4)
        Me.lblEtichettaProfilo.Name = "lblEtichettaProfilo"
        Me.lblEtichettaProfilo.Size = New System.Drawing.Size(200, 18)
        Me.lblEtichettaProfilo.TabIndex = 0
        Me.lblEtichettaProfilo.Text = "Il tuo profilo"
        '
        'lblProfilo
        '
        Me.lblProfilo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblProfilo.Location = New System.Drawing.Point(0, 24)
        Me.lblProfilo.Name = "lblProfilo"
        Me.lblProfilo.Size = New System.Drawing.Size(910, 34)
        Me.lblProfilo.TabIndex = 1
        '
        'btnApriProfilo
        '
        Me.btnApriProfilo.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnApriProfilo.Location = New System.Drawing.Point(936, 16)
        Me.btnApriProfilo.Name = "btnApriProfilo"
        Me.btnApriProfilo.Size = New System.Drawing.Size(170, 32)
        Me.btnApriProfilo.TabIndex = 2
        Me.btnApriProfilo.Text = "Apri il profilo"
        '
        'pnlFiltro
        '
        Me.pnlFiltro.Controls.Add(Me.lblContatori)
        Me.pnlFiltro.Controls.Add(Me.lblStelle)
        Me.pnlFiltro.Controls.Add(Me.cboStelle)
        Me.pnlFiltro.Controls.Add(Me.lblMostra)
        Me.pnlFiltro.Controls.Add(Me.cboMostra)
        Me.pnlFiltro.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlFiltro.Location = New System.Drawing.Point(14, 132)
        Me.pnlFiltro.Name = "pnlFiltro"
        Me.pnlFiltro.Size = New System.Drawing.Size(1106, 34)
        Me.pnlFiltro.TabIndex = 2
        '
        'lblContatori
        '
        Me.lblContatori.Font = StileApp.FontTitoloGruppo
        Me.lblContatori.ForeColor = StileApp.TestoPrimario
        Me.lblContatori.Location = New System.Drawing.Point(0, 6)
        Me.lblContatori.Name = "lblContatori"
        Me.lblContatori.Size = New System.Drawing.Size(620, 20)
        Me.lblContatori.TabIndex = 0
        '
        'lblStelle
        '
        Me.lblStelle.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStelle.Font = StileApp.FontDidascalia
        Me.lblStelle.ForeColor = StileApp.TestoSecondario
        Me.lblStelle.Location = New System.Drawing.Point(686, 8)
        Me.lblStelle.Name = "lblStelle"
        Me.lblStelle.Size = New System.Drawing.Size(42, 18)
        Me.lblStelle.TabIndex = 1
        Me.lblStelle.Text = "Stelle"
        '
        'cboStelle
        '
        Me.cboStelle.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cboStelle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboStelle.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cboStelle.Location = New System.Drawing.Point(728, 4)
        Me.cboStelle.Name = "cboStelle"
        Me.cboStelle.Size = New System.Drawing.Size(140, 23)
        Me.cboStelle.TabIndex = 2
        '
        'lblMostra
        '
        Me.lblMostra.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblMostra.Font = StileApp.FontDidascalia
        Me.lblMostra.ForeColor = StileApp.TestoSecondario
        Me.lblMostra.Location = New System.Drawing.Point(886, 8)
        Me.lblMostra.Name = "lblMostra"
        Me.lblMostra.Size = New System.Drawing.Size(50, 18)
        Me.lblMostra.TabIndex = 3
        Me.lblMostra.Text = "Mostra"
        '
        'cboMostra
        '
        Me.cboMostra.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cboMostra.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cboMostra.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cboMostra.Location = New System.Drawing.Point(936, 4)
        Me.cboMostra.Name = "cboMostra"
        Me.cboMostra.Size = New System.Drawing.Size(170, 23)
        Me.cboMostra.TabIndex = 4
        '
        'pnlCorpo
        '
        Me.pnlCorpo.Controls.Add(Me.lvwCoda)
        Me.pnlCorpo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCorpo.Location = New System.Drawing.Point(14, 166)
        Me.pnlCorpo.Name = "pnlCorpo"
        Me.pnlCorpo.Size = New System.Drawing.Size(1106, 516)
        Me.pnlCorpo.TabIndex = 3
        '
        'lvwCoda
        '
        ' La coda delle opportunità (cap. 03.6): una lista di sistema come quella dei
        ' giudizi in P4 — a cambiare sono le righe, mai la struttura (cap. 03.1).
        ' L'intestazione si può cliccare per ordinare, e il doppio clic riapre.
        Me.lvwCoda.BackColor = StileApp.SfondoContenuto
        Me.lvwCoda.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvwCoda.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colMatch, Me.colAzienda, Me.colRuolo, Me.colStato, Me.colFonte, Me.colQuando})
        Me.lvwCoda.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvwCoda.Font = StileApp.FontTesto
        Me.lvwCoda.FullRowSelect = True
        Me.lvwCoda.HideSelection = False
        Me.lvwCoda.Location = New System.Drawing.Point(0, 0)
        Me.lvwCoda.MultiSelect = False
        Me.lvwCoda.Name = "lvwCoda"
        Me.lvwCoda.Size = New System.Drawing.Size(1106, 516)
        Me.lvwCoda.TabIndex = 0
        Me.lvwCoda.UseCompatibleStateImageBehavior = False
        Me.lvwCoda.View = System.Windows.Forms.View.Details
        '
        'colMatch
        '
        Me.colMatch.Text = "Match"
        Me.colMatch.Width = 130
        '
        'colAzienda
        '
        Me.colAzienda.Text = "Azienda"
        Me.colAzienda.Width = 250
        '
        'colRuolo
        '
        Me.colRuolo.Text = "Ruolo"
        Me.colRuolo.Width = 300
        '
        'colStato
        '
        Me.colStato.Text = "Stato"
        Me.colStato.Width = 130
        '
        'colFonte
        '
        Me.colFonte.Text = "Da dove"
        Me.colFonte.Width = 130
        '
        'colQuando
        '
        Me.colQuando.Text = "Aggiornata"
        Me.colQuando.Width = 130
        '
        'pnlAzioni
        '
        ' Solo bottoni: la riga di stato sta in alto a destra nell'intestazione, come in
        ' P4. Qui sotto il pannello logo si prende l'angolo sinistro (v. IPannelloArea),
        ' e un'etichetta in mezzo ai bottoni finirebbe sotto di lui o sopra di loro.
        Me.pnlAzioni.Controls.Add(Me.btnApriCandidatura)
        Me.pnlAzioni.Controls.Add(Me.btnNuovaRicerca)
        Me.pnlAzioni.Controls.Add(Me.btnAggiornaProfilo)
        Me.pnlAzioni.Controls.Add(Me.btnEsportaRegistro)
        Me.pnlAzioni.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlAzioni.Location = New System.Drawing.Point(14, 682)
        Me.pnlAzioni.Name = "pnlAzioni"
        Me.pnlAzioni.Size = New System.Drawing.Size(1106, 64)
        Me.pnlAzioni.TabIndex = 4
        '
        'btnApriCandidatura
        '
        Me.btnApriCandidatura.Location = New System.Drawing.Point(0, 12)
        Me.btnApriCandidatura.Name = "btnApriCandidatura"
        Me.btnApriCandidatura.Size = New System.Drawing.Size(190, 32)
        Me.btnApriCandidatura.TabIndex = 0
        Me.btnApriCandidatura.Text = "Apri la candidatura"
        '
        'btnNuovaRicerca
        '
        Me.btnNuovaRicerca.Location = New System.Drawing.Point(202, 12)
        Me.btnNuovaRicerca.Name = "btnNuovaRicerca"
        Me.btnNuovaRicerca.Size = New System.Drawing.Size(150, 32)
        Me.btnNuovaRicerca.TabIndex = 1
        Me.btnNuovaRicerca.Text = "Nuova ricerca"
        '
        'btnAggiornaProfilo
        '
        Me.btnAggiornaProfilo.Location = New System.Drawing.Point(364, 12)
        Me.btnAggiornaProfilo.Name = "btnAggiornaProfilo"
        Me.btnAggiornaProfilo.Size = New System.Drawing.Size(160, 32)
        Me.btnAggiornaProfilo.TabIndex = 2
        Me.btnAggiornaProfilo.Text = "Aggiorna profilo"
        '
        'btnEsportaRegistro
        '
        Me.btnEsportaRegistro.Location = New System.Drawing.Point(534, 12)
        Me.btnEsportaRegistro.Name = "btnEsportaRegistro"
        Me.btnEsportaRegistro.Size = New System.Drawing.Size(170, 32)
        Me.btnEsportaRegistro.TabIndex = 3
        Me.btnEsportaRegistro.Text = "Esporta l'elenco…"
        '
        'lblStatoHome
        '
        Me.lblStatoHome.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatoHome.Font = StileApp.FontDidascalia
        Me.lblStatoHome.ForeColor = StileApp.TestoSecondario
        Me.lblStatoHome.Location = New System.Drawing.Point(706, 4)
        Me.lblStatoHome.Name = "lblStatoHome"
        Me.lblStatoHome.Size = New System.Drawing.Size(400, 46)
        Me.lblStatoHome.TabIndex = 2
        Me.lblStatoHome.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'PannelloHome
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = StileApp.SfondoBase
        Me.Controls.Add(Me.pnlCorpo)
        Me.Controls.Add(Me.pnlFiltro)
        Me.Controls.Add(Me.pnlProfiloInSintesi)
        Me.Controls.Add(Me.pnlIntestazione)
        Me.Controls.Add(Me.pnlAzioni)
        Me.Font = StileApp.FontTesto
        Me.ForeColor = StileApp.TestoPrimario
        Me.Name = "PannelloHome"
        Me.Padding = New System.Windows.Forms.Padding(14)
        Me.Size = New System.Drawing.Size(1134, 760)
        Me.pnlIntestazione.ResumeLayout(False)
        Me.pnlProfiloInSintesi.ResumeLayout(False)
        Me.pnlFiltro.ResumeLayout(False)
        Me.pnlCorpo.ResumeLayout(False)
        Me.pnlAzioni.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents pnlIntestazione As System.Windows.Forms.Panel
    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSottotitolo As System.Windows.Forms.Label
    Friend WithEvents pnlProfiloInSintesi As System.Windows.Forms.Panel
    Friend WithEvents lblEtichettaProfilo As System.Windows.Forms.Label
    Friend WithEvents lblProfilo As System.Windows.Forms.Label
    Friend WithEvents btnApriProfilo As System.Windows.Forms.Button
    Friend WithEvents pnlFiltro As System.Windows.Forms.Panel
    Friend WithEvents lblContatori As System.Windows.Forms.Label
    Friend WithEvents lblStelle As System.Windows.Forms.Label
    Friend WithEvents cboStelle As System.Windows.Forms.ComboBox
    Friend WithEvents lblMostra As System.Windows.Forms.Label
    Friend WithEvents cboMostra As System.Windows.Forms.ComboBox
    Friend WithEvents pnlCorpo As System.Windows.Forms.Panel
    Friend WithEvents lvwCoda As System.Windows.Forms.ListView
    Friend WithEvents colMatch As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAzienda As System.Windows.Forms.ColumnHeader
    Friend WithEvents colRuolo As System.Windows.Forms.ColumnHeader
    Friend WithEvents colStato As System.Windows.Forms.ColumnHeader
    Friend WithEvents colFonte As System.Windows.Forms.ColumnHeader
    Friend WithEvents colQuando As System.Windows.Forms.ColumnHeader
    Friend WithEvents pnlAzioni As System.Windows.Forms.Panel
    Friend WithEvents btnApriCandidatura As System.Windows.Forms.Button
    Friend WithEvents btnNuovaRicerca As System.Windows.Forms.Button
    Friend WithEvents btnAggiornaProfilo As System.Windows.Forms.Button
    Friend WithEvents btnEsportaRegistro As System.Windows.Forms.Button
    Friend WithEvents lblStatoHome As System.Windows.Forms.Label

End Class
