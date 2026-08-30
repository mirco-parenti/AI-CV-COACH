<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PannelloMenu
    Inherits System.Windows.Forms.UserControl

    'UserControl esegue l'override del metodo Dispose per pulire l'elenco dei componenti.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing Then
                ' Gli elementi di sfondo sono una Bitmap tenuta da parte fra un
                ' ridisegno e l'altro: se ne va con il pannello.
                _sfondo?.Dispose()
                If components IsNot Nothing Then components.Dispose()
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
        Me.btnVoceCandidature = New BottoneMenu()
        Me.btnVoceProfiloCv = New BottoneMenu()
        Me.btnVoceRicercaOnline = New BottoneMenu()
        Me.btnVoceIncollaOffline = New BottoneMenu()
        Me.btnVoceDocumentazione = New BottoneMenu()
        Me.btnVoceImpostazioni = New BottoneMenu()
        Me.SuspendLayout()
        '
        ' I sei bottoni. Misura e posizione le decide DisponiIBottoni: qui c'è solo
        ' quel che non dipende da quanto è grande la finestra.
        '
        'btnVoceCandidature
        '
        Me.btnVoceCandidature.Name = "btnVoceCandidature"
        Me.btnVoceCandidature.TabIndex = 0
        Me.btnVoceCandidature.Text = "Le mie candidature"
        '
        'btnVoceProfiloCv
        '
        Me.btnVoceProfiloCv.Name = "btnVoceProfiloCv"
        Me.btnVoceProfiloCv.TabIndex = 1
        Me.btnVoceProfiloCv.Text = "Profilo e CV base"
        '
        'btnVoceRicercaOnline
        '
        Me.btnVoceRicercaOnline.Name = "btnVoceRicercaOnline"
        Me.btnVoceRicercaOnline.TabIndex = 2
        Me.btnVoceRicercaOnline.Text = "Ricerca annuncio — ONLINE"
        '
        'btnVoceIncollaOffline
        '
        Me.btnVoceIncollaOffline.Name = "btnVoceIncollaOffline"
        Me.btnVoceIncollaOffline.TabIndex = 3
        Me.btnVoceIncollaOffline.Text = "Confronta ANNUNCIO - CV / Match 1-5 ⭐"
        '
        'btnVoceDocumentazione
        '
        Me.btnVoceDocumentazione.Name = "btnVoceDocumentazione"
        Me.btnVoceDocumentazione.TabIndex = 4
        Me.btnVoceDocumentazione.Text = "Elabora Documentazione"
        '
        'btnVoceImpostazioni
        '
        Me.btnVoceImpostazioni.Name = "btnVoceImpostazioni"
        Me.btnVoceImpostazioni.TabIndex = 5
        Me.btnVoceImpostazioni.Text = "Impostazioni"
        '
        'PannelloMenu
        '
        Me.Controls.Add(Me.btnVoceCandidature)
        Me.Controls.Add(Me.btnVoceProfiloCv)
        Me.Controls.Add(Me.btnVoceRicercaOnline)
        Me.Controls.Add(Me.btnVoceIncollaOffline)
        Me.Controls.Add(Me.btnVoceDocumentazione)
        Me.Controls.Add(Me.btnVoceImpostazioni)
        Me.Name = "PannelloMenu"
        Me.Size = New System.Drawing.Size(1134, 513)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents btnVoceCandidature As BottoneMenu
    Friend WithEvents btnVoceProfiloCv As BottoneMenu
    Friend WithEvents btnVoceRicercaOnline As BottoneMenu
    Friend WithEvents btnVoceIncollaOffline As BottoneMenu
    Friend WithEvents btnVoceDocumentazione As BottoneMenu
    Friend WithEvents btnVoceImpostazioni As BottoneMenu

End Class
