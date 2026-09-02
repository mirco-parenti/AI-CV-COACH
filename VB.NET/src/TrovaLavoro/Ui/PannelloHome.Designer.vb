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
        Me.lblPromemoria = New System.Windows.Forms.Label()
        Me.lblStelle = New System.Windows.Forms.Label()
        Me.cboStelle = New System.Windows.Forms.ComboBox()
        Me.lblMostra = New System.Windows.Forms.Label()
        Me.cboMostra = New System.Windows.Forms.ComboBox()
        Me.pnlCorpo = New System.Windows.Forms.Panel()
        Me.lvwCoda = New System.Windows.Forms.ListView()
        Me.colProfilo = New System.Windows.Forms.ColumnHeader()
        Me.colMatch = New System.Windows.Forms.ColumnHeader()
        Me.colAzienda = New System.Windows.Forms.ColumnHeader()
        Me.colRuolo = New System.Windows.Forms.ColumnHeader()
        Me.colStato = New System.Windows.Forms.ColumnHeader()
        Me.colEsito = New System.Windows.Forms.ColumnHeader()
        Me.colFonte = New System.Windows.Forms.ColumnHeader()
        Me.colQuando = New System.Windows.Forms.ColumnHeader()
        Me.pnlAzioni = New System.Windows.Forms.Panel()
        Me.btnApriCandidatura = New System.Windows.Forms.Button()
        Me.btnNuovaRicerca = New System.Windows.Forms.Button()
        Me.btnEliminaCandidatura = New System.Windows.Forms.Button()
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
        Me.lblEtichettaProfilo.ForeColor = StileApp.RossoCritico
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
        Me.btnApriProfilo.Location = New System.Drawing.Point(916, 16)
        Me.btnApriProfilo.Name = "btnApriProfilo"
        Me.btnApriProfilo.Size = StileApp.BottoneLargo
        Me.btnApriProfilo.TabIndex = 2
        Me.btnApriProfilo.Text = "Apri il profilo"
        '
        'pnlFiltro
        '
        Me.pnlFiltro.Controls.Add(Me.lblContatori)
        Me.pnlFiltro.Controls.Add(Me.lblPromemoria)
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
        Me.lblContatori.Size = New System.Drawing.Size(660, 20)
        Me.lblContatori.TabIndex = 0
        '
        'lblPromemoria
        '
        Me.lblPromemoria.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblPromemoria.Font = StileApp.FontDidascalia
        Me.lblPromemoria.ForeColor = StileApp.InformazioneTesto
        Me.lblPromemoria.Location = New System.Drawing.Point(0, 30)
        Me.lblPromemoria.Name = "lblPromemoria"
        Me.lblPromemoria.Size = New System.Drawing.Size(1100, 20)
        Me.lblPromemoria.TabIndex = 5
        Me.lblPromemoria.Visible = False
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
        Me.lvwCoda.BackColor = StileApp.FondoCasella
        Me.lvwCoda.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lvwCoda.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colAzienda, Me.colRuolo, Me.colProfilo, Me.colMatch, Me.colStato, Me.colEsito, Me.colFonte, Me.colQuando})
        Me.lvwCoda.Dock = System.Windows.Forms.DockStyle.Fill
        Me.lvwCoda.Font = StileApp.FontTesto
        Me.lvwCoda.FullRowSelect = True
        Me.lvwCoda.HideSelection = False
        Me.lvwCoda.Location = New System.Drawing.Point(0, 0)
        Me.lvwCoda.MultiSelect = False
        ' Il suggerimento della riga: è dove la spia del profilo spiega il perché, che nella
        ' colonna non ci starebbe (v. SpiaDelProfilo).
        Me.lvwCoda.ShowItemToolTips = True
        Me.lvwCoda.Name = "lvwCoda"
        Me.lvwCoda.Size = New System.Drawing.Size(1106, 516)
        Me.lvwCoda.TabIndex = 0
        Me.lvwCoda.UseCompatibleStateImageBehavior = False
        Me.lvwCoda.View = System.Windows.Forms.View.Details
        '
        'colAzienda
        '
        ' Apre la coda dal 2026-09-03: è il nome con cui una candidatura si chiama quando
        ' se ne parla («quella della Rossi»), ed è da lì che l'occhio la cerca in un elenco.
        ' Prima aprivano la spia e il match — cioè un giudizio su una riga di cui non si era
        ' ancora detto il soggetto.
        ' Windows tiene la prima colonna di una lista sempre allineata a sinistra (WinForms
        ' rimette Left da sé appena si prova ad assegnare altro): qui non costa niente,
        ' perché è testo, e il testo a sinistra ci sta di suo.
        ' 200 e non più 250: i 50 px vanno alle due colonne nuove, e sull'azienda si
        ' perde solo aria — v. il commento in cima ad AdattaLeColonne.
        Me.colAzienda.Text = "Azienda"
        Me.colAzienda.Width = 200
        '
        'colRuolo
        '
        ' Segue l'azienda dal 2026-09-03: sono le due metà della stessa domanda — chi
        ' offre il posto, e quale posto — e si leggono di fila come si dicono a voce.
        ' È anche la colonna elastica (v. AdattaLeColonne): quel che avanza va a lei,
        ' perché è la sola voce che si allunga davvero.
        Me.colRuolo.Text = "Ruolo"
        Me.colRuolo.Width = 300
        '
        'colProfilo
        '
        ' Apre la parte del giudizio e precede il match, che è di lui che parla: prima si
        ' incontra se quel numero vale ancora, poi il numero. Fino al 2026-09-03 stavano
        ' nell'altro verso, e a scambiarli non è stata una teoria — è che azienda e ruolo
        ' vogliono stare vicini, e il punteggio con la sua qualifica dopo di loro.
        ' Si chiama «Profilo del match» e non «Profilo» dal 2026-09-03: due colonne che
        ' parlano della stessa cosa sembravano scollegate, e l'idea di fonderle in una si
        ' fermava sul limite di sempre — un ListView dà un inchiostro per cella, e lucina
        ' rossa e stelle nere nella stessa cella vorrebbero dire ridisegnare a mano tutta
        ' la coda. Il legame lo dichiara l'intestazione: costa una parola, e lascia intatti
        ' i due colori e i due ordinamenti (le due domande restano diverse — «quanto vale»
        ' e «quali sono da rifare»).
        ' 150 basta: la scritta più lunga — «● profilo usato: obsoleto» — misura 136 px col
        ' carattere della coda, e l'intestazione 98; misurati, non stimati (v. i collaudi).
        Me.colProfilo.Text = "Profilo del match"
        Me.colProfilo.Width = 150
        '
        'colMatch
        '
        ' Il punteggio è un numero, e i numeri si leggono incolonnati a destra come la data
        ' dell'ultima colonna. Fino al 2026-09-02 non ci arrivava, perché il match apriva la
        ' coda e la prima colonna a destra non ci va; da allora quel limite non lo tocca più
        ' — quel posto è passato alla spia, poi all'azienda — ma l'allineamento resta a
        ' sinistra finché non lo si guarda a video: è una misura da decidere guardando, non
        ' deducendo.
        Me.colMatch.Text = "Match"
        Me.colMatch.Width = 110

        '
        'colStato
        '
        ' Dal 2026-09-03 dice a che punto è la procedura — «CV mirato ✓ · lettera ✓ · email
        ' ✓» — e non più il nome dello stato. 250 px perché la riga più lunga, quella con in
        ' coda l'avviso «⚠ obsoleti», ne misura 239: è la lezione del giorno prima, quando
        ' due righe di stato tagliavano in silenzio proprio la parte che diceva cosa fare.
        Me.colStato.Text = "Stato"
        Me.colStato.Width = 250
        '
        'colEsito
        '
        ' Nata il 2026-09-03 dalla colonna «Stato», che ha cambiato mestiere. Tiene le due
        ' cose che i file non sanno e le dice l'utente — com'è finita e se ha lasciato
        ' perdere — più i giorni d'attesa di una spedita senza risposta. «Assunto 🎉» è la
        ' voce più larga con 65 px.
        Me.colEsito.Text = "Esito"
        Me.colEsito.Width = 100
        '
        'colFonte
        '
        Me.colFonte.Text = "Da dove"
        Me.colFonte.Width = 110
        '
        'colQuando
        '
        ' Una data è un numero incolonnato: allineata a destra, giorni e ore si leggono
        ' uno sotto l'altro invece di ballare con la lunghezza del testo accanto.
        Me.colQuando.Text = "Aggiornata"
        Me.colQuando.TextAlign = System.Windows.Forms.HorizontalAlignment.Right
        Me.colQuando.Width = 110

        '
        'pnlAzioni
        '
        ' Solo bottoni: la riga di stato sta in alto a destra nell'intestazione, come in
        ' P4. Qui sotto il pannello logo si prende l'angolo sinistro (v. IPannelloArea),
        ' e un'etichetta in mezzo ai bottoni finirebbe sotto di lui o sopra di loro.
        Me.pnlAzioni.Controls.Add(Me.btnApriCandidatura)
        Me.pnlAzioni.Controls.Add(Me.btnNuovaRicerca)
        Me.pnlAzioni.Controls.Add(Me.btnEsportaRegistro)
        Me.pnlAzioni.Controls.Add(Me.btnEliminaCandidatura)
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
        Me.btnApriCandidatura.Size = StileApp.BottoneLargo
        Me.btnApriCandidatura.TabIndex = 0
        Me.btnApriCandidatura.Text = "Apri la candidatura"
        '
        'btnNuovaRicerca
        '
        Me.btnNuovaRicerca.Location = New System.Drawing.Point(944, 12)
        Me.btnNuovaRicerca.Name = "btnNuovaRicerca"
        Me.btnNuovaRicerca.Size = StileApp.BottoneLargo
        Me.btnNuovaRicerca.TabIndex = 3
        Me.btnNuovaRicerca.Text = "Nuova ricerca"
        '
        'btnEsportaRegistro
        '
        Me.btnEsportaRegistro.Location = New System.Drawing.Point(202, 12)
        Me.btnEsportaRegistro.Name = "btnEsportaRegistro"
        Me.btnEsportaRegistro.Size = StileApp.BottoneLargo
        Me.btnEsportaRegistro.TabIndex = 1
        Me.btnEsportaRegistro.Text = "Esporta l'elenco…"
        '
        'btnEliminaCandidatura
        '
        ' Ultimo della fila e ultimo col tasto Tab: si accende con la stessa riga scelta
        ' che accende «Apri la candidatura», e i due non devono stare sotto lo stesso dito.
        Me.btnEliminaCandidatura.Location = New System.Drawing.Point(384, 12)
        Me.btnEliminaCandidatura.Name = "btnEliminaCandidatura"
        Me.btnEliminaCandidatura.Size = StileApp.BottoneLargo
        Me.btnEliminaCandidatura.TabIndex = 2
        Me.btnEliminaCandidatura.Text = "Elimina candidatura"
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
        Me.BackColor = StileApp.FondoPagina
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
    Friend WithEvents lblPromemoria As System.Windows.Forms.Label
    Friend WithEvents lblStelle As System.Windows.Forms.Label
    Friend WithEvents cboStelle As System.Windows.Forms.ComboBox
    Friend WithEvents lblMostra As System.Windows.Forms.Label
    Friend WithEvents cboMostra As System.Windows.Forms.ComboBox
    Friend WithEvents pnlCorpo As System.Windows.Forms.Panel
    Friend WithEvents lvwCoda As System.Windows.Forms.ListView
    Friend WithEvents colProfilo As System.Windows.Forms.ColumnHeader
    Friend WithEvents colMatch As System.Windows.Forms.ColumnHeader
    Friend WithEvents colAzienda As System.Windows.Forms.ColumnHeader
    Friend WithEvents colRuolo As System.Windows.Forms.ColumnHeader
    Friend WithEvents colStato As System.Windows.Forms.ColumnHeader
    Friend WithEvents colEsito As System.Windows.Forms.ColumnHeader
    Friend WithEvents colFonte As System.Windows.Forms.ColumnHeader
    Friend WithEvents colQuando As System.Windows.Forms.ColumnHeader
    Friend WithEvents pnlAzioni As System.Windows.Forms.Panel
    Friend WithEvents btnApriCandidatura As System.Windows.Forms.Button
    Friend WithEvents btnNuovaRicerca As System.Windows.Forms.Button
    Friend WithEvents btnEliminaCandidatura As System.Windows.Forms.Button
    Friend WithEvents btnEsportaRegistro As System.Windows.Forms.Button
    Friend WithEvents lblStatoHome As System.Windows.Forms.Label

End Class
