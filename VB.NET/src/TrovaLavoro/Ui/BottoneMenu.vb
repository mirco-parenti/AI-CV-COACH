Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Il bottone a pillola del menu d'ingresso (P0): riempimento a sfumatura, bordo scuro
''' spesso, testo bianco con contorno. Si dipinge da sé, perché in Windows Forms un
''' bottone di sistema non sa fare né gli angoli tondi né la sfumatura.
''' </summary>
''' <remarks>
''' <para><b>Perché un controllo e non sei bottoni vestiti a mano.</b> La forma non è un
''' colore che si assegna: è un disegno, e disegnarlo in sei posti vorrebbe dire sei
''' copie della stessa geometria destinate a divergere. Qui il disegno sta in un posto
''' solo e i sei bottoni ne sono istanze.</para>
''' <para><b>La tavolozza è quella del marchio.</b> Il riempimento è il blu Aviolab di
''' <see cref="StileApp.Accento"/>, schiarito in cima e pieno in fondo; il bordo è il
''' giallo del filetto della cornice (<c>#E2E44E</c>), che su quel blu è la coppia del
''' banner. Il menu appartiene al marchio invece di importare i colori di un'altra
''' grafica. Sono costanti dichiarate qui sotto — cambiare l'aspetto di tutti e sei è
''' cambiare una riga.</para>
''' <para><b>Il testo ha un contorno, non un'ombra.</b> Dietro ci passa un'immagine, e
''' un'ombra sola non basta a staccare le lettere da tutti i fondi che possono capitare:
''' il contorno le stacca da qualunque cosa ci sia sotto, com'è nel nome del banner.</para>
''' <para><b>Gli angoli fuori dalla pillola: due strade provate, una sola funziona.</b>
''' Là fuori deve vedersi quel che c'è sotto, e in Windows Forms un <c>BackColor</c>
''' trasparente <i>non</i> lo garantisce: la trasparenza è simulata, e il figlio ricopia
''' lo sfondo del padre solo per la parte che il padre dipinge nel modo consueto. Il
''' pannello del menu disegna invece l'immagine dentro il proprio
''' <c>OnPaintBackground</c>, che un figlio non vede — e il risultato, guardato a video il
''' 2026-08-30, era un <b>rettangolo nero</b> attorno a ogni pillola: esattamente la forma
''' che il menu non doveva avere. Chiedere al padre di ridipingere la propria porzione
''' sotto il bottone (<c>InvokePaintBackground</c> con le coordinate traslate) sembrava la
''' cura elegante, e a video non ha cambiato niente: il rettangolo è rimasto nero. Quel
''' che funziona è <b>ritagliare il controllo</b> con una <see cref="Region"/> a forma di
''' pillola: fuori di lì il bottone non esiste proprio, e si vede il pannello. Il prezzo è
''' che una regione ritaglia a pixel interi, senza sfumare i bordi — lo paga il bordo
''' giallo, che è spesso e disegnato in antialias <i>dentro</i> il ritaglio, e assorbe la
''' scalettatura invece di esibirla.</para>
''' </remarks>
Public Class BottoneMenu
    Inherits Button

    ''' <summary>Il blu chiaro in cima al riempimento: il blu Aviolab, schiarito.</summary>
    Private Shared ReadOnly BluChiaro As Color = Color.FromArgb(58, 47, 208)

    ''' <summary>Il blu in fondo al riempimento: quello del marchio, <c>#0B06B0</c>.</summary>
    Private Shared ReadOnly BluScuro As Color = StileApp.Accento

    ''' <summary>Il bordo: il giallo del filetto della cornice. Sul blu è la coppia del marchio.</summary>
    Private Shared ReadOnly ColoreBordo As Color = ColorTranslator.FromHtml("#E2E44E")

    ''' <summary>Quanto è spesso il bordo, in pixel.</summary>
    Private Const SpessoreBordo As Single = 5.0F

    ''' <summary>Il filo chiaro appena dentro il bordo, che dà il rilievo.</summary>
    Private Shared ReadOnly ColoreLucido As Color = Color.FromArgb(150, 255, 255, 255)

    ''' <summary>Quanto si schiarisce il riempimento quando il mouse ci passa sopra.</summary>
    Private Const SchiarimentoSopra As Integer = 26

    ''' <summary>Quanto si scurisce quando è premuto — e di quanto scende il testo.</summary>
    Private Const ScurimentoPremuto As Integer = 26

    ''' <summary>Di quanti pixel scende il testo mentre il bottone è premuto.</summary>
    Private Const DiscesaDelPremuto As Integer = 2

    ''' <summary>Se il puntatore è sopra.</summary>
    Private _sopra As Boolean

    ''' <summary>Se il tasto è giù su questo bottone.</summary>
    Private _premuto As Boolean

    Public Sub New()

        SetStyle(ControlStyles.UserPaint Or
                 ControlStyles.AllPaintingInWmPaint Or
                 ControlStyles.OptimizedDoubleBuffer Or
                 ControlStyles.ResizeRedraw Or
                 ControlStyles.SupportsTransparentBackColor, True)

        Me.BackColor = Color.Transparent
        Me.FlatStyle = FlatStyle.Flat
        Me.FlatAppearance.BorderSize = 0
        Me.ForeColor = Color.White
        Me.Font = New Font(StileApp.NomeFont, 15.0F, FontStyle.Bold)
        Me.Cursor = Cursors.Hand
        Me.UseVisualStyleBackColor = False

    End Sub

    ''' <summary>
    ''' Il percorso della pillola: gli angoli sono semicerchi di raggio pari a metà
    ''' altezza, che è la forma del bottone del menu.
    ''' </summary>
    ''' <remarks>
    ''' Il rettangolo arriva già rientrato di mezzo spessore di bordo, perché una penna
    ''' larga dipinge metà dentro e metà fuori del percorso: senza il rientro, metà del
    ''' bordo cadrebbe fuori dal controllo e verrebbe tagliata.
    ''' </remarks>
    Private Shared Function Pillola(riquadro As RectangleF) As GraphicsPath

        Dim percorso As New GraphicsPath()

        Dim raggio As Single = Math.Min(riquadro.Height, riquadro.Width) / 2.0F
        If raggio <= 0 Then
            percorso.AddRectangle(riquadro)
            Return percorso
        End If

        Dim lato As Single = raggio * 2.0F
        percorso.AddArc(riquadro.X, riquadro.Y, lato, lato, 90, 180)
        percorso.AddArc(riquadro.Right - lato, riquadro.Y, lato, lato, 270, 180)
        percorso.CloseFigure()

        Return percorso

    End Function

    ''' <summary>Sposta un colore verso il bianco o verso il nero, restando nei limiti.</summary>
    Private Shared Function Sposta(colore As Color, di As Integer) As Color

        Return Color.FromArgb(colore.A,
                              Math.Clamp(colore.R + di, 0, 255),
                              Math.Clamp(colore.G + di, 0, 255),
                              Math.Clamp(colore.B + di, 0, 255))

    End Function

    ''' <summary>
    ''' Ritaglia il controllo a pillola, così fuori di lì si vede il pannello.
    ''' </summary>
    ''' <remarks>
    ''' Si rifà a ogni cambio di misura, ed è l'unica cosa che <c>SetBoundsCore</c> fa in
    ''' più: la forma dipende dall'altezza, e un bottone che cresce con una regione vecchia
    ''' resterebbe tagliato alla misura di prima. Passa di qui e non da <c>OnResize</c>
    ''' perché i limiti sono già quelli nuovi quando la base ha finito.
    ''' </remarks>
    Protected Overrides Sub SetBoundsCore(x As Integer, y As Integer,
                                          larghezza As Integer, altezza As Integer,
                                          specificato As BoundsSpecified)

        MyBase.SetBoundsCore(x, y, larghezza, altezza, specificato)
        RifaiIlRitaglio()

    End Sub

    ''' <summary>La regione a pillola, rifatta sulla misura di adesso.</summary>
    Private Sub RifaiIlRitaglio()

        If Me.Width <= 0 OrElse Me.Height <= 0 Then Return

        Dim vecchia As Region = Me.Region

        Using percorso As GraphicsPath = Pillola(New RectangleF(0, 0, Me.Width, Me.Height))
            Me.Region = New Region(percorso)
        End Using

        ' La regione di prima non la libera nessuno: assegnarne una nuova non dispone la
        ' vecchia, e questo bottone la rifà a ogni ridimensionamento della finestra.
        vecchia?.Dispose()

    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)

        Dim g As Graphics = e.Graphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit

        Dim rientro As Single = SpessoreBordo / 2.0F
        Dim riquadro As New RectangleF(rientro, rientro,
                                       Math.Max(1.0F, Me.Width - SpessoreBordo),
                                       Math.Max(1.0F, Me.Height - SpessoreBordo))

        ' Quanto è chiaro il riempimento dipende da cosa sta facendo il mouse.
        Dim spostamento As Integer = 0
        If _premuto Then
            spostamento = -ScurimentoPremuto
        ElseIf _sopra AndAlso Me.Enabled Then
            spostamento = SchiarimentoSopra
        End If

        Using percorso As GraphicsPath = Pillola(riquadro)

            If Me.Enabled Then
                Using pennello As New LinearGradientBrush(riquadro,
                                                          Sposta(BluChiaro, spostamento),
                                                          Sposta(BluScuro, spostamento),
                                                          LinearGradientMode.Vertical)
                    g.FillPath(pennello, percorso)
                End Using
            Else
                ' Spento: grigio piatto, così si vede a colpo d'occhio che non si preme.
                Using pennello As New SolidBrush(StileApp.BordoForte)
                    g.FillPath(pennello, percorso)
                End Using
            End If

            ' Il filo chiaro appena dentro, che dà il rilievo: solo la metà di sopra,
            ' come fa la luce.
            If Me.Enabled Then
                Using rientrato As GraphicsPath = Pillola(New RectangleF(riquadro.X + SpessoreBordo,
                                                                        riquadro.Y + SpessoreBordo,
                                                                        Math.Max(1.0F, riquadro.Width - SpessoreBordo * 2.0F),
                                                                        Math.Max(1.0F, riquadro.Height - SpessoreBordo * 2.0F)))
                    Using penna As New Pen(ColoreLucido, 2.0F)
                        Dim ritaglio As Region = g.Clip
                        g.SetClip(New RectangleF(0, 0, Me.Width, Me.Height / 2.0F))
                        g.DrawPath(penna, rientrato)
                        g.Clip = ritaglio
                    End Using
                End Using
            End If

            Using penna As New Pen(If(Me.Enabled, ColoreBordo, StileApp.BordoForte), SpessoreBordo)
                penna.Alignment = PenAlignment.Center
                g.DrawPath(penna, percorso)
            End Using

        End Using

        DisegnaIlTesto(g)

        ' Il rettangolo del fuoco da tastiera: senza, chi naviga col tabulatore non sa
        ' dove si trova. Sta dentro il bordo, tratteggiato.
        If Me.Focused AndAlso Me.ShowFocusCues Then
            Dim dentro As Rectangle = Rectangle.Round(riquadro)
            dentro.Inflate(-CInt(SpessoreBordo) - 2, -CInt(SpessoreBordo) - 2)
            ControlPaint.DrawFocusRectangle(g, dentro)
        End If

    End Sub

    ''' <summary>
    ''' Scrive l'etichetta al centro, col contorno che la stacca dallo sfondo.
    ''' </summary>
    Private Sub DisegnaIlTesto(g As Graphics)

        If String.IsNullOrEmpty(Me.Text) Then Return

        Dim discesa As Integer = If(_premuto, DiscesaDelPremuto, 0)
        Dim area As New Rectangle(0, discesa, Me.Width, Me.Height)

        Using formato As New StringFormat(StringFormatFlags.NoWrap)

            formato.Alignment = StringAlignment.Center
            formato.LineAlignment = StringAlignment.Center
            formato.Trimming = StringTrimming.EllipsisCharacter

            ' Il contorno si fa ridisegnando il testo attorno a sé stesso: otto copie
            ' scure spostate di un pixel, poi quella chiara sopra. È il modo che non
            ' dipende da GraphicsPath, che sui font di sistema rende peggio.
            Using scuro As New SolidBrush(Color.FromArgb(190, 0, 0, 0))
                For Each dx As Integer In New Integer() {-1, 0, 1}
                    For Each dy As Integer In New Integer() {-1, 0, 1}
                        If dx = 0 AndAlso dy = 0 Then Continue For
                        Dim spostata As New Rectangle(area.X + dx, area.Y + dy, area.Width, area.Height)
                        g.DrawString(Me.Text, Me.Font, scuro, spostata, formato)
                    Next
                Next
            End Using

            Using chiaro As New SolidBrush(If(Me.Enabled, Me.ForeColor, StileApp.SfondoBase))
                g.DrawString(Me.Text, Me.Font, chiaro, area, formato)
            End Using

        End Using

    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        _sopra = True
        Invalidate()
        MyBase.OnMouseEnter(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        _sopra = False
        _premuto = False
        Invalidate()
        MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        If e.Button = MouseButtons.Left Then
            _premuto = True
            Invalidate()
        End If
        MyBase.OnMouseDown(e)
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        If _premuto Then
            _premuto = False
            Invalidate()
        End If
        MyBase.OnMouseUp(e)
    End Sub

    Protected Overrides Sub OnEnabledChanged(e As EventArgs)
        ' Un bottone che si spegne mentre il mouse gli sta sopra resterebbe schiarito.
        If Not Me.Enabled Then
            _sopra = False
            _premuto = False
        End If
        Invalidate()
        MyBase.OnEnabledChanged(e)
    End Sub

End Class
