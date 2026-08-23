<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PannelloDocumenti
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
        Me.lblStatoDocumenti = New System.Windows.Forms.Label()
        Me.pnlOpzioni = New System.Windows.Forms.Panel()
        Me.lblLingua = New System.Windows.Forms.Label()
        Me.cmbLingua = New System.Windows.Forms.ComboBox()
        Me.lblEsporta = New System.Windows.Forms.Label()
        Me.cmbEsporta = New System.Windows.Forms.ComboBox()
        Me.lblDocumento = New System.Windows.Forms.Label()
        Me.pnlCorniceDocumento = New System.Windows.Forms.Panel()
        Me.cmbDocumento = New System.Windows.Forms.ComboBox()
        Me.pnlCorpo = New System.Windows.Forms.Panel()
        Me.pnlCv = New System.Windows.Forms.Panel()
        Me.lblCv = New System.Windows.Forms.Label()
        Me.txtCv = New System.Windows.Forms.TextBox()
        Me.pnlAnnuncio = New System.Windows.Forms.Panel()
        Me.lblAnnuncio = New System.Windows.Forms.Label()
        Me.txtAnnuncio = New System.Windows.Forms.TextBox()
        Me.pnlLettera = New System.Windows.Forms.Panel()
        Me.lblLettera = New System.Windows.Forms.Label()
        Me.txtLettera = New System.Windows.Forms.TextBox()
        Me.pnlAzioni = New System.Windows.Forms.Panel()
        Me.btnTornaIndietro = New System.Windows.Forms.Button()
        Me.btnRigenera = New System.Windows.Forms.Button()
        Me.btnModificaTesti = New System.Windows.Forms.Button()
        Me.btnRigeneraLettera = New System.Windows.Forms.Button()
        Me.btnEsportaDocx = New System.Windows.Forms.Button()
        Me.btnEsportaPdf = New System.Windows.Forms.Button()
        Me.btnPreparaEmail = New System.Windows.Forms.Button()
        Me.pnlIntestazione.SuspendLayout()
        Me.pnlOpzioni.SuspendLayout()
        Me.pnlCorpo.SuspendLayout()
        Me.pnlCv.SuspendLayout()
        Me.pnlAnnuncio.SuspendLayout()
        Me.pnlLettera.SuspendLayout()
        Me.pnlAzioni.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlIntestazione
        '
        Me.pnlIntestazione.Controls.Add(Me.lblTitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblSottotitolo)
        Me.pnlIntestazione.Controls.Add(Me.lblStatoDocumenti)
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
        Me.lblTitolo.Text = "Documenti"
        '
        'lblSottotitolo
        '
        Me.lblSottotitolo.Font = StileApp.FontDidascalia
        Me.lblSottotitolo.ForeColor = StileApp.TestoSecondario
        Me.lblSottotitolo.Location = New System.Drawing.Point(2, 32)
        Me.lblSottotitolo.Name = "lblSottotitolo"
        Me.lblSottotitolo.Size = New System.Drawing.Size(700, 18)
        Me.lblSottotitolo.TabIndex = 1
        Me.lblSottotitolo.Text = "Passo 2 di 2 — rileggi quello che ho scritto, poi esportalo nel formato che ti serve."
        '
        'lblStatoDocumenti
        '
        Me.lblStatoDocumenti.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatoDocumenti.Font = StileApp.FontDidascalia
        Me.lblStatoDocumenti.ForeColor = StileApp.TestoSecondario
        Me.lblStatoDocumenti.Location = New System.Drawing.Point(706, 4)
        Me.lblStatoDocumenti.Name = "lblStatoDocumenti"
        Me.lblStatoDocumenti.Size = New System.Drawing.Size(400, 46)
        Me.lblStatoDocumenti.TabIndex = 2
        Me.lblStatoDocumenti.TextAlign = System.Drawing.ContentAlignment.TopRight
        '
        'pnlOpzioni
        '
        ' La rifinitura anti-slop è di T7b: si vede, spenta, col suo suggerimento
        ' (cap. 03.8), perché è qui che arriverà. La lingua accanto a lei era nella stessa
        ' condizione fino a T7a, che l'ha accesa.
        ' L'ordine è quello della lettura, e dice cosa viene prima: **quale** documento
        ' si guarda, e solo dopo in che lingua. Fino a T9d era il contrario, con la
        ' tendina dei documenti relegata a destra della lingua e larga uguale: la prova
        ' dal vivo l'ha trovata «poco visibile» proprio mentre è la porta da cui questo
        ' pannello si usa.
        Me.pnlOpzioni.Controls.Add(Me.lblDocumento)
        Me.pnlOpzioni.Controls.Add(Me.pnlCorniceDocumento)
        Me.pnlOpzioni.Controls.Add(Me.lblLingua)
        Me.pnlOpzioni.Controls.Add(Me.cmbLingua)
        Me.pnlOpzioni.Controls.Add(Me.lblEsporta)
        Me.pnlOpzioni.Controls.Add(Me.cmbEsporta)
        Me.pnlOpzioni.Dock = System.Windows.Forms.DockStyle.Top
        Me.pnlOpzioni.Location = New System.Drawing.Point(14, 74)
        Me.pnlOpzioni.Name = "pnlOpzioni"
        Me.pnlOpzioni.Size = New System.Drawing.Size(1106, 48)
        Me.pnlOpzioni.TabIndex = 1
        '
        'lblLingua
        '
        Me.lblLingua.Font = StileApp.FontTesto
        Me.lblLingua.ForeColor = StileApp.TestoPrimario
        Me.lblLingua.Location = New System.Drawing.Point(548, 14)
        Me.lblLingua.Name = "lblLingua"
        Me.lblLingua.Size = New System.Drawing.Size(60, 20)
        Me.lblLingua.TabIndex = 0
        Me.lblLingua.Text = "Lingua:"
        '
        'cmbLingua
        '
        Me.cmbLingua.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbLingua.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        ' I due nomi sono quelli di «LinguaDocumenti.Nome», e sono in italiano perché
        ' l'interfaccia è in una lingua sola (cap. 10.1): la tendina dice «Inglese», non
        ' «English». L'ordine conta — è quello che «LinguaDaTendina» legge.
        Me.cmbLingua.Items.AddRange(New Object() {"Italiano", "Inglese"})
        Me.cmbLingua.Location = New System.Drawing.Point(612, 12)
        Me.cmbLingua.Name = "cmbLingua"
        Me.cmbLingua.Size = New System.Drawing.Size(140, 23)
        Me.cmbLingua.TabIndex = 1
        '
        'lblEsporta
        '
        Me.lblEsporta.Font = StileApp.FontTesto
        Me.lblEsporta.ForeColor = StileApp.TestoPrimario
        Me.lblEsporta.Location = New System.Drawing.Point(768, 14)
        Me.lblEsporta.Name = "lblEsporta"
        Me.lblEsporta.Size = New System.Drawing.Size(64, 20)
        Me.lblEsporta.TabIndex = 2
        Me.lblEsporta.Text = "Esporta:"
        '
        'cmbEsporta
        '
        ' Quali documenti finiscono nei file (R8, 2026-08-23): prima l'esportazione era in
        ' blocco e chi voleva il solo CV si ritrovava accanto anche la lettera. L'ordine
        ' delle voci conta — è quello che «DocumentiDaTendina» legge — e la prima è quella
        ' di sempre, così chi non tocca niente ottiene quel che otteneva prima.
        Me.cmbEsporta.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbEsporta.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        Me.cmbEsporta.Items.AddRange(New Object() {"CV e lettera", "Solo il CV", "Solo la lettera"})
        Me.cmbEsporta.Location = New System.Drawing.Point(836, 12)
        Me.cmbEsporta.Name = "cmbEsporta"
        Me.cmbEsporta.Size = New System.Drawing.Size(160, 23)
        Me.cmbEsporta.TabIndex = 3
        '
        'lblDocumento
        '
        Me.lblDocumento.Font = StileApp.FontTitoloGruppo
        Me.lblDocumento.ForeColor = StileApp.Accento
        Me.lblDocumento.Location = New System.Drawing.Point(0, 14)
        Me.lblDocumento.Name = "lblDocumento"
        Me.lblDocumento.Size = New System.Drawing.Size(112, 20)
        Me.lblDocumento.TabIndex = 0
        Me.lblDocumento.Text = "📄 Documento:"
        '
        'cmbDocumento
        '
        ' Il selettore dei documenti (T9d): da qui si salta al 📄 CV-1 base o a quelli di
        ' un'altra candidatura senza tornare alla Home. Le voci le mette il pannello, che
        ' sa quali candidature hanno davvero un documento da mostrare.
        Me.cmbDocumento.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbDocumento.FlatStyle = System.Windows.Forms.FlatStyle.Flat
        ' Il carattere è quello dell'azione principale: una tendina non è un bottone, ma
        ' qui fa lo stesso mestiere — è da lei che si sceglie cosa si sta guardando.
        Me.cmbDocumento.Font = StileApp.FontAzionePrincipale
        Me.cmbDocumento.Location = New System.Drawing.Point(2, 2)
        Me.cmbDocumento.Name = "cmbDocumento"
        Me.cmbDocumento.Size = New System.Drawing.Size(412, 26)
        Me.cmbDocumento.TabIndex = 1
        '
        'pnlCorniceDocumento
        '
        ' Non è decorazione: una ComboBox piatta non sa disegnarsi un bordo colorato, e
        ' il contorno d'accento è ciò che la distingue da una casella qualunque. Il
        ' pannello fa da cornice di 2 px, e la sua misura la detta la tendina (vedi il
        ' costruttore): l'altezza di una ComboBox la decide il carattere, non chi disegna.
        Me.pnlCorniceDocumento.Controls.Add(Me.cmbDocumento)
        Me.pnlCorniceDocumento.BackColor = StileApp.Accento
        Me.pnlCorniceDocumento.Location = New System.Drawing.Point(116, 9)
        Me.pnlCorniceDocumento.Name = "pnlCorniceDocumento"
        Me.pnlCorniceDocumento.Size = New System.Drawing.Size(416, 30)
        Me.pnlCorniceDocumento.TabIndex = 1
        '
        'pnlCorpo
        '
        Me.pnlCorpo.Controls.Add(Me.pnlCv)
        Me.pnlCorpo.Controls.Add(Me.pnlAnnuncio)
        Me.pnlCorpo.Controls.Add(Me.pnlLettera)
        Me.pnlCorpo.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCorpo.Location = New System.Drawing.Point(14, 114)
        Me.pnlCorpo.Name = "pnlCorpo"
        Me.pnlCorpo.Size = New System.Drawing.Size(1106, 568)
        Me.pnlCorpo.TabIndex = 2
        '
        'pnlCv
        '
        Me.pnlCv.Controls.Add(Me.txtCv)
        Me.pnlCv.Controls.Add(Me.lblCv)
        Me.pnlCv.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pnlCv.Location = New System.Drawing.Point(310, 0)
        Me.pnlCv.Name = "pnlCv"
        Me.pnlCv.Padding = New System.Windows.Forms.Padding(0, 0, 12, 0)
        Me.pnlCv.Size = New System.Drawing.Size(416, 568)
        Me.pnlCv.TabIndex = 1
        '
        'lblCv
        '
        Me.lblCv.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblCv.Font = StileApp.FontTitoloGruppo
        Me.lblCv.ForeColor = StileApp.RossoTitoli
        Me.lblCv.Location = New System.Drawing.Point(0, 0)
        Me.lblCv.Name = "lblCv"
        Me.lblCv.Size = New System.Drawing.Size(404, 18)
        Me.lblCv.TabIndex = 0
        Me.lblCv.Text = "CV"
        '
        'txtCv
        '
        Me.txtCv.BackColor = StileApp.SfondoContenuto
        Me.txtCv.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtCv.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtCv.Multiline = True
        Me.txtCv.Name = "txtCv"
        Me.txtCv.ReadOnly = True
        Me.txtCv.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtCv.Size = New System.Drawing.Size(404, 550)
        Me.txtCv.TabIndex = 1
        '
        'pnlAnnuncio
        '
        Me.pnlAnnuncio.Controls.Add(Me.txtAnnuncio)
        Me.pnlAnnuncio.Controls.Add(Me.lblAnnuncio)
        Me.pnlAnnuncio.Dock = System.Windows.Forms.DockStyle.Left
        Me.pnlAnnuncio.Location = New System.Drawing.Point(0, 0)
        Me.pnlAnnuncio.Name = "pnlAnnuncio"
        Me.pnlAnnuncio.Padding = New System.Windows.Forms.Padding(0, 0, 12, 0)
        Me.pnlAnnuncio.Size = New System.Drawing.Size(310, 568)
        Me.pnlAnnuncio.TabIndex = 0
        '
        'lblAnnuncio
        '
        Me.lblAnnuncio.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblAnnuncio.Font = StileApp.FontTitoloGruppo
        Me.lblAnnuncio.ForeColor = StileApp.RossoTitoli
        Me.lblAnnuncio.Location = New System.Drawing.Point(0, 0)
        Me.lblAnnuncio.Name = "lblAnnuncio"
        Me.lblAnnuncio.Size = New System.Drawing.Size(298, 18)
        Me.lblAnnuncio.TabIndex = 0
        Me.lblAnnuncio.Text = "Annuncio"
        '
        'txtAnnuncio
        '
        Me.txtAnnuncio.BackColor = StileApp.SfondoContenuto
        Me.txtAnnuncio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtAnnuncio.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtAnnuncio.Multiline = True
        Me.txtAnnuncio.Name = "txtAnnuncio"
        Me.txtAnnuncio.ReadOnly = True
        Me.txtAnnuncio.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtAnnuncio.Size = New System.Drawing.Size(298, 550)
        Me.txtAnnuncio.TabIndex = 1
        '
        'pnlLettera
        '
        Me.pnlLettera.Controls.Add(Me.txtLettera)
        Me.pnlLettera.Controls.Add(Me.lblLettera)
        Me.pnlLettera.Dock = System.Windows.Forms.DockStyle.Right
        Me.pnlLettera.Location = New System.Drawing.Point(726, 0)
        Me.pnlLettera.Name = "pnlLettera"
        Me.pnlLettera.Size = New System.Drawing.Size(380, 568)
        Me.pnlLettera.TabIndex = 2
        '
        'lblLettera
        '
        Me.lblLettera.Dock = System.Windows.Forms.DockStyle.Top
        Me.lblLettera.Font = StileApp.FontTitoloGruppo
        Me.lblLettera.ForeColor = StileApp.RossoTitoli
        Me.lblLettera.Location = New System.Drawing.Point(0, 0)
        Me.lblLettera.Name = "lblLettera"
        Me.lblLettera.Size = New System.Drawing.Size(380, 18)
        Me.lblLettera.TabIndex = 0
        Me.lblLettera.Text = "Lettera di presentazione"
        '
        'txtLettera
        '
        Me.txtLettera.BackColor = StileApp.SfondoContenuto
        Me.txtLettera.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.txtLettera.Dock = System.Windows.Forms.DockStyle.Fill
        Me.txtLettera.Multiline = True
        Me.txtLettera.Name = "txtLettera"
        Me.txtLettera.ReadOnly = True
        Me.txtLettera.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.txtLettera.Size = New System.Drawing.Size(380, 550)
        Me.txtLettera.TabIndex = 1
        '
        'pnlAzioni
        '
        Me.pnlAzioni.Controls.Add(Me.btnTornaIndietro)
        Me.pnlAzioni.Controls.Add(Me.btnRigenera)
        Me.pnlAzioni.Controls.Add(Me.btnModificaTesti)
        Me.pnlAzioni.Controls.Add(Me.btnRigeneraLettera)
        Me.pnlAzioni.Controls.Add(Me.btnEsportaDocx)
        Me.pnlAzioni.Controls.Add(Me.btnEsportaPdf)
        Me.pnlAzioni.Controls.Add(Me.btnPreparaEmail)
        Me.pnlAzioni.Dock = System.Windows.Forms.DockStyle.Bottom
        Me.pnlAzioni.Location = New System.Drawing.Point(14, 682)
        Me.pnlAzioni.Name = "pnlAzioni"
        Me.pnlAzioni.Size = New System.Drawing.Size(1106, 64)
        Me.pnlAzioni.TabIndex = 3
        '
        'btnTornaIndietro
        '
        Me.btnTornaIndietro.Location = New System.Drawing.Point(0, 18)
        Me.btnTornaIndietro.Name = "btnTornaIndietro"
        Me.btnTornaIndietro.Size = New System.Drawing.Size(170, 32)
        Me.btnTornaIndietro.TabIndex = 0
        Me.btnTornaIndietro.Text = "Torna all'opportunità"
        '
        'btnRigenera
        '
        Me.btnRigenera.Location = New System.Drawing.Point(182, 18)
        Me.btnRigenera.Name = "btnRigenera"
        Me.btnRigenera.Size = New System.Drawing.Size(130, 32)
        Me.btnRigenera.TabIndex = 1
        Me.btnRigenera.Text = "Rigenera"
        '
        'btnModificaTesti
        '
        Me.btnModificaTesti.Location = New System.Drawing.Point(324, 18)
        Me.btnModificaTesti.Name = "btnModificaTesti"
        Me.btnModificaTesti.Size = New System.Drawing.Size(170, 32)
        Me.btnModificaTesti.TabIndex = 2
        Me.btnModificaTesti.Text = "Modifica i testi"
        '
        'btnRigeneraLettera
        '
        ' Nasce invisibile: c'è solo quando la lettera è rimasta indietro rispetto al CV
        ' riscritto a mano (R7). La posizione la decide la fascia, come per tutti gli altri.
        Me.btnRigeneraLettera.Location = New System.Drawing.Point(506, 18)
        Me.btnRigeneraLettera.Name = "btnRigeneraLettera"
        Me.btnRigeneraLettera.Size = New System.Drawing.Size(210, 32)
        Me.btnRigeneraLettera.TabIndex = 3
        Me.btnRigeneraLettera.Text = "⚠ Rigenera la lettera"
        Me.btnRigeneraLettera.Visible = False
        '
        'btnEsportaDocx
        '
        Me.btnEsportaDocx.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnEsportaDocx.Location = New System.Drawing.Point(646, 18)
        Me.btnEsportaDocx.Name = "btnEsportaDocx"
        Me.btnEsportaDocx.Size = New System.Drawing.Size(130, 32)
        Me.btnEsportaDocx.TabIndex = 4
        Me.btnEsportaDocx.Text = "Esporta DOCX"
        '
        'btnEsportaPdf
        '
        Me.btnEsportaPdf.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnEsportaPdf.Location = New System.Drawing.Point(788, 18)
        Me.btnEsportaPdf.Name = "btnEsportaPdf"
        Me.btnEsportaPdf.Size = New System.Drawing.Size(130, 32)
        Me.btnEsportaPdf.TabIndex = 5
        Me.btnEsportaPdf.Text = "Esporta PDF"
        '
        'btnPreparaEmail
        '
        Me.btnPreparaEmail.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnPreparaEmail.Location = New System.Drawing.Point(930, 18)
        Me.btnPreparaEmail.Name = "btnPreparaEmail"
        Me.btnPreparaEmail.Size = New System.Drawing.Size(176, 32)
        Me.btnPreparaEmail.TabIndex = 6
        Me.btnPreparaEmail.Text = "Prepara email"
        '
        'PannelloDocumenti
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 15.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = StileApp.SfondoBase
        Me.Controls.Add(Me.pnlCorpo)
        Me.Controls.Add(Me.pnlOpzioni)
        Me.Controls.Add(Me.pnlIntestazione)
        Me.Controls.Add(Me.pnlAzioni)
        Me.Font = StileApp.FontTesto
        Me.ForeColor = StileApp.TestoPrimario
        Me.Name = "PannelloDocumenti"
        Me.Padding = New System.Windows.Forms.Padding(14)
        Me.Size = New System.Drawing.Size(1134, 760)
        Me.pnlIntestazione.ResumeLayout(False)
        Me.pnlOpzioni.ResumeLayout(False)
        Me.pnlCorpo.ResumeLayout(False)
        Me.pnlCv.ResumeLayout(False)
        Me.pnlCv.PerformLayout()
        Me.pnlAnnuncio.ResumeLayout(False)
        Me.pnlAnnuncio.PerformLayout()
        Me.pnlLettera.ResumeLayout(False)
        Me.pnlLettera.PerformLayout()
        Me.pnlAzioni.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents pnlIntestazione As System.Windows.Forms.Panel
    Friend WithEvents lblTitolo As System.Windows.Forms.Label
    Friend WithEvents lblSottotitolo As System.Windows.Forms.Label
    Friend WithEvents lblStatoDocumenti As System.Windows.Forms.Label
    Friend WithEvents pnlOpzioni As System.Windows.Forms.Panel
    Friend WithEvents lblLingua As System.Windows.Forms.Label
    Friend WithEvents cmbLingua As System.Windows.Forms.ComboBox
    Friend WithEvents lblEsporta As System.Windows.Forms.Label
    Friend WithEvents cmbEsporta As System.Windows.Forms.ComboBox
    Friend WithEvents lblDocumento As System.Windows.Forms.Label
    Friend WithEvents pnlCorniceDocumento As System.Windows.Forms.Panel
    Friend WithEvents cmbDocumento As System.Windows.Forms.ComboBox
    Friend WithEvents pnlCorpo As System.Windows.Forms.Panel
    Friend WithEvents pnlAnnuncio As System.Windows.Forms.Panel
    Friend WithEvents lblAnnuncio As System.Windows.Forms.Label
    Friend WithEvents txtAnnuncio As System.Windows.Forms.TextBox
    Friend WithEvents pnlCv As System.Windows.Forms.Panel
    Friend WithEvents lblCv As System.Windows.Forms.Label
    Friend WithEvents txtCv As System.Windows.Forms.TextBox
    Friend WithEvents pnlLettera As System.Windows.Forms.Panel
    Friend WithEvents lblLettera As System.Windows.Forms.Label
    Friend WithEvents txtLettera As System.Windows.Forms.TextBox
    Friend WithEvents pnlAzioni As System.Windows.Forms.Panel
    Friend WithEvents btnTornaIndietro As System.Windows.Forms.Button
    Friend WithEvents btnRigenera As System.Windows.Forms.Button
    Friend WithEvents btnModificaTesti As System.Windows.Forms.Button
    Friend WithEvents btnRigeneraLettera As System.Windows.Forms.Button
    Friend WithEvents btnEsportaDocx As System.Windows.Forms.Button
    Friend WithEvents btnEsportaPdf As System.Windows.Forms.Button
    Friend WithEvents btnPreparaEmail As System.Windows.Forms.Button

End Class
