Imports System.Drawing
Imports System.Windows.Forms
Imports TrovaLavoro.Ai

''' <summary>
''' Finestra principale dell'applicazione (cap. 03.4). A T1 c'è il solo scheletro:
''' barra superiore di navigazione, area centrale vuota, fascia di stato e pannello
''' logo. I pannelli P1–P7 e la navigazione vera arrivano nelle tappe successive.
''' </summary>
Public Class FormPrincipale

    ''' <summary>Sotto questa larghezza il pannello logo passa in compatta (cap. 03.5).</summary>
    Private Const LarghezzaModalitaCompatta As Integer = 1350

    ' Geometria del pannello logo nelle due modalità.
    Private Const LogoLarghezza As Integer = 261
    Private Const LogoAltezza As Integer = 216
    Private Const LogoLatoImmagine As Integer = 101
    Private Const LogoLarghezzaCompatta As Integer = 130
    Private Const LogoAltezzaCompatta As Integer = 96
    Private Const LogoLatoImmagineCompatta As Integer = 56
    Private Const AltezzaRigaNome As Integer = 30
    Private Const AltezzaRigaDidascalia As Integer = 15

    ' Nothing finché la modalità non è stata decisa la prima volta.
    Private compattaAttiva As Boolean?

    Private Sub FormPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MostraLeDueVersioni()
        pnlLogo.BringToFront()
        AggiornaPannelloLogo()
    End Sub

    ''' <summary>
    ''' Riempie la riga «Ver. 0.3.001 · Pool 1.00 (integrato)» del pannello logo
    ''' (cap. 03.5). Il pool si apre proprio qui perché la sua etichetta dichiara da
    ''' sé sorgente e stato — esterno, integrato, o con l'asterisco dei file
    ''' modificati — e quel che ha da dire in più (un ripiego, dei file fuori
    ''' impronta) finisce nella barra di stato, in attesa delle Impostazioni.
    ''' </summary>
    Private Sub MostraLeDueVersioni()
        Try
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri()
            lblVersione.Text = $"Ver. {Versione.Numero} · {libreria.Etichetta}"
            If libreria.Avviso IsNot Nothing Then lblStato.Text = libreria.Avviso
        Catch ex As Exception
            ' L'anomalia totale del cap. 03.5: la finestra deve comparire lo stesso e
            ' dire cosa non va, invece di morire prima di mostrarsi.
            lblVersione.Text = $"Ver. {Versione.Numero} · Pool —"
            lblStato.Text = $"Libreria dei prompt non disponibile: {ex.Message}"
        End Try
    End Sub

    Private Sub FormPrincipale_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        AggiornaPannelloLogo()
    End Sub

    ''' <summary>
    ''' Tiene il pannello logo incollato all'angolo in basso a sinistra e sceglie fra
    ''' modalità piena e compatta. Il pannello è flottante sopra la struttura, quindi
    ''' la sua posizione si ricalcola a ogni ridimensionamento.
    ''' </summary>
    Private Sub AggiornaPannelloLogo()
        Dim compatta As Boolean = (Me.ClientSize.Width < LarghezzaModalitaCompatta)

        If Not compattaAttiva.HasValue OrElse compattaAttiva.Value <> compatta Then
            compattaAttiva = compatta
            DisponiPannelloLogo(compatta)
        End If

        pnlLogo.Location = New Point(0, Me.ClientSize.Height - pnlLogo.Height)
    End Sub

    ''' <summary>Dispone i contenuti del pannello logo nella modalità richiesta.</summary>
    Private Sub DisponiPannelloLogo(compatta As Boolean)
        Dim larghezza As Integer = If(compatta, LogoLarghezzaCompatta, LogoLarghezza)
        Dim altezza As Integer = If(compatta, LogoAltezzaCompatta, LogoAltezza)
        Dim lato As Integer = If(compatta, LogoLatoImmagineCompatta, LogoLatoImmagine)
        Dim margine As Integer = If(compatta, StileApp.InterlineaMinima, StileApp.MargineRiquadro)

        pnlLogo.SuspendLayout()

        pnlLogo.Size = New Size(larghezza, altezza)

        Dim immaginePrecedente As Image = picLogo.Image
        picLogo.SetBounds((larghezza - lato) \ 2, margine, lato, lato)
        picLogo.Image = LogoAviolab.Genera(lato)
        immaginePrecedente?.Dispose()

        ' In compatta restano solo l'immagine ridotta e la versione.
        lblMarchio.Visible = Not compatta
        lblCopyright.Visible = Not compatta

        Dim riga As Integer = margine + lato + StileApp.InterlineaMinima
        If Not compatta Then
            lblMarchio.SetBounds(0, riga, larghezza, AltezzaRigaNome)
            riga += AltezzaRigaNome + 2
        End If
        lblVersione.SetBounds(0, riga, larghezza, AltezzaRigaDidascalia)
        If Not compatta Then
            lblCopyright.SetBounds(0, riga + AltezzaRigaDidascalia + 2, larghezza, AltezzaRigaDidascalia)
        End If

        ' La barra di stato scrive a destra del pannello logo, che le sta sopra.
        lblStato.Padding = New Padding(larghezza + StileApp.DistanzaControlli, 0,
                                       StileApp.DistanzaControlli, 0)

        pnlLogo.ResumeLayout()
    End Sub

End Class
