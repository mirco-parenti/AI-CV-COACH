Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Text
Imports System.Windows.Forms

''' <summary>
''' Il bottone del menu d'ingresso (P0): riempimento azzurro chiaro, cornice grigia
''' sottile con gli angoli appena smussati, un filo bianco fra le due, testo scuro.
''' Si dipinge da sé, perché in Windows Forms un bottone di sistema non sa fare né gli
''' angoli tondi né il filo interno.
''' </summary>
''' <remarks>
''' <para><b>Perché un controllo e non sei bottoni vestiti a mano.</b> La forma non è un
''' colore che si assegna: è un disegno, e disegnarlo in sei posti vorrebbe dire sei
''' copie della stessa geometria destinate a divergere. Qui il disegno sta in un posto
''' solo e i sei bottoni ne sono istanze.</para>
''' <para><b>La tavolozza è quella dell'applicazione, e nient'altro.</b> Il riempimento è
''' <see cref="StileApp.FondoAzione"/>, la cornice <see cref="StileApp.BordoForte"/>, il
''' filo <see cref="StileApp.SfondoContenuto"/>, il testo
''' <see cref="StileApp.TestoPrimario"/>: quattro token già in tabella (cap. 03.2), non
''' un colore nuovo. <i>Fino al 2026-08-30 (sera) era invece una pillola blu col bordo
''' giallo del filetto e il testo bianco contornato: il blu del marchio su un fondo che
''' era il banner. Cambiato il fondo — avorio, col mega stemma dietro — quella pillola
''' era diventata la cosa più scura della schermata, e sei blocchi blu davanti a uno
''' stemma blu si contendevano lo sguardo.</i></para>
''' <para><b>Sul chiaro si scurisce, non si schiarisce.</b> Il bottone di prima si
''' schiariva al passaggio del mouse, che su un blu profondo era il modo naturale di
''' accendersi. Su un azzurro già chiarissimo schiarire non si vedrebbe: la stessa
''' manopola gira ora dall'altra parte.</para>
''' <para><b>Gli angoli fuori dalla cornice: due strade provate, una sola funziona.</b>
''' Là fuori deve vedersi quel che c'è sotto, e in Windows Forms un <c>BackColor</c>
''' trasparente <i>non</i> lo garantisce: la trasparenza è simulata, e il figlio ricopia
''' lo sfondo del padre solo per la parte che il padre dipinge nel modo consueto. Il
''' pannello del menu disegna invece il fondo dentro il proprio <c>OnPaintBackground</c>,
''' che un figlio non vede — e il risultato, guardato a video il 2026-08-30, era un
''' <b>rettangolo nero</b> attorno a ogni bottone: esattamente la forma che il menu non
''' doveva avere. Chiedere al padre di ridipingere la propria porzione sotto il bottone
''' (<c>InvokePaintBackground</c> con le coordinate traslate) sembrava la cura elegante, e
''' a video non ha cambiato niente: il rettangolo è rimasto nero. Quel che funziona è
''' <b>ritagliare il controllo</b> con una <see cref="Region"/> della forma giusta: fuori
''' di lì il bottone non esiste proprio, e si vede il pannello. Il prezzo è che una
''' regione ritaglia a pixel interi, senza sfumare i bordi — con gli angoli quasi vivi di
''' oggi se ne accorge molto meno di quanto se ne accorgesse la pillola.</para>
''' </remarks>
Public Class BottoneMenu
    Inherits Button

    ''' <summary>Il riempimento: l'azzurro dell'azione (cap. 03.2).</summary>
    Private Shared ReadOnly ColoreFondo As Color = StileApp.FondoAzione

    ''' <summary>La cornice esterna: il grigio dei controlli interattivi.</summary>
    Private Shared ReadOnly ColoreBordo As Color = StileApp.BordoForte

    ''' <summary>Il filo chiaro fra la cornice e il riempimento, che dà il rilievo.</summary>
    Private Shared ReadOnly ColoreFilo As Color = StileApp.SfondoContenuto

    ''' <summary>Il riempimento di un bottone spento.</summary>
    ''' <remarks>
    ''' Più chiaro della cornice, non più scuro: così il contorno continua a vedersi e il
    ''' bottone resta una forma, invece di diventare una macchia grigia uniforme.
    ''' </remarks>
    Private Shared ReadOnly ColoreFondoSpento As Color = StileApp.SfondoBase

    ''' <summary>Quanto è spessa la cornice, in pixel.</summary>
    Private Const SpessoreBordo As Single = 2.0F

    ''' <summary>Quanto è spesso il filo chiaro appena dentro la cornice, in pixel.</summary>
    Private Const SpessoreFilo As Single = 3.0F

    ''' <summary>Quanto sono smussati gli angoli, in pixel.</summary>
    ''' <remarks>
    ''' Appena smussati, non tondi: la pillola di prima aveva raggio pari a metà altezza,
    ''' e su sei bottoni in colonna faceva una fila di losanghe. Sei pixel su
    ''' cinquantatré sono la differenza fra un rettangolo duro e un rettangolo gentile.
    ''' </remarks>
    Private Const RaggioAngolo As Single = 6.0F

    ''' <summary>Quanto si scurisce il riempimento quando il mouse ci passa sopra.</summary>
    Private Const ScurimentoSopra As Integer = 18

    ''' <summary>Quanto si scurisce quando è premuto — e di quanto scende il testo.</summary>
    Private Const ScurimentoPremuto As Integer = 36

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
        Me.ForeColor = StileApp.TestoPrimario
        Me.Font = New Font(StileApp.NomeFont, 15.0F, FontStyle.Bold)
        Me.Cursor = Cursors.Hand
        Me.UseVisualStyleBackColor = False

    End Sub

    ''' <summary>
    ''' Il percorso del bottone: un rettangolo con i quattro angoli smussati.
    ''' </summary>
    ''' <remarks>
    ''' Il rettangolo arriva già rientrato di mezzo spessore di cornice, perché una penna
    ''' larga dipinge metà dentro e metà fuori del percorso: senza il rientro, metà della
    ''' cornice cadrebbe fuori dal controllo e verrebbe tagliata.
    ''' </remarks>
    Private Shared Function Sagoma(riquadro As RectangleF, raggio As Single) As GraphicsPath

        Dim percorso As New GraphicsPath()

        If riquadro.Width <= 0 OrElse riquadro.Height <= 0 Then
            percorso.AddRectangle(riquadro)
            Return percorso
        End If

        ' Un raggio più grande della metà del lato corto non è un angolo, è una pillola.
        Dim r As Single = Math.Min(raggio, Math.Min(riquadro.Width, riquadro.Height) / 2.0F)
        If r <= 0.5F Then
            percorso.AddRectangle(riquadro)
            Return percorso
        End If

        Dim lato As Single = r * 2.0F
        percorso.AddArc(riquadro.X, riquadro.Y, lato, lato, 180, 90)
        percorso.AddArc(riquadro.Right - lato, riquadro.Y, lato, lato, 270, 90)
        percorso.AddArc(riquadro.Right - lato, riquadro.Bottom - lato, lato, lato, 0, 90)
        percorso.AddArc(riquadro.X, riquadro.Bottom - lato, lato, lato, 90, 90)
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
    ''' Ritaglia il controllo alla sua sagoma, così fuori di lì si vede il pannello.
    ''' </summary>
    ''' <remarks>
    ''' Si rifà a ogni cambio di misura, ed è l'unica cosa che <c>SetBoundsCore</c> fa in
    ''' più: un bottone che cresce con una regione vecchia resterebbe tagliato alla misura
    ''' di prima. Passa di qui e non da <c>OnResize</c> perché i limiti sono già quelli
    ''' nuovi quando la base ha finito.
    ''' </remarks>
    Protected Overrides Sub SetBoundsCore(x As Integer, y As Integer,
                                          larghezza As Integer, altezza As Integer,
                                          specificato As BoundsSpecified)

        MyBase.SetBoundsCore(x, y, larghezza, altezza, specificato)
        RifaiIlRitaglio()

    End Sub

    ''' <summary>La regione della sagoma, rifatta sulla misura di adesso.</summary>
    Private Sub RifaiIlRitaglio()

        If Me.Width <= 0 OrElse Me.Height <= 0 Then Return

        Dim vecchia As Region = Me.Region

        Using percorso As GraphicsPath = Sagoma(New RectangleF(0, 0, Me.Width, Me.Height), RaggioAngolo)
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

        ' Quanto è scuro il riempimento dipende da cosa sta facendo il mouse: su un fondo
        ' chiaro ci si accende scurendo, non schiarendo.
        Dim spostamento As Integer = 0
        If _premuto Then
            spostamento = -ScurimentoPremuto
        ElseIf _sopra AndAlso Me.Enabled Then
            spostamento = -ScurimentoSopra
        End If

        Using percorso As GraphicsPath = Sagoma(riquadro, RaggioAngolo)

            Using pennello As New SolidBrush(
                If(Me.Enabled, Sposta(ColoreFondo, spostamento), ColoreFondoSpento))
                g.FillPath(pennello, percorso)
            End Using

            ' Il filo chiaro appena dentro la cornice, tutt'attorno: è quello che stacca
            ' il riempimento dal contorno e dà al bottone il suo spessore.
            If Me.Enabled Then
                Using rientrato As GraphicsPath = Sagoma(
                    New RectangleF(riquadro.X + SpessoreFilo / 2.0F,
                                   riquadro.Y + SpessoreFilo / 2.0F,
                                   Math.Max(1.0F, riquadro.Width - SpessoreFilo),
                                   Math.Max(1.0F, riquadro.Height - SpessoreFilo)),
                    Math.Max(1.0F, RaggioAngolo - SpessoreFilo / 2.0F))

                    Using penna As New Pen(ColoreFilo, SpessoreFilo)
                        g.DrawPath(penna, rientrato)
                    End Using

                End Using
            End If

            Using penna As New Pen(ColoreBordo, SpessoreBordo)
                penna.Alignment = PenAlignment.Center
                g.DrawPath(penna, percorso)
            End Using

        End Using

        DisegnaIlTesto(g)

        ' Il rettangolo del fuoco da tastiera: senza, chi naviga col tabulatore non sa
        ' dove si trova. Sta dentro la cornice, tratteggiato.
        If Me.Focused AndAlso Me.ShowFocusCues Then
            Dim dentro As Rectangle = Rectangle.Round(riquadro)
            dentro.Inflate(-CInt(SpessoreBordo) - 2, -CInt(SpessoreBordo) - 2)
            ControlPaint.DrawFocusRectangle(g, dentro)
        End If

    End Sub

    ''' <summary>Scrive l'etichetta al centro.</summary>
    ''' <remarks>
    ''' Senza contorno, che qui sarebbe di troppo: il contorno serviva a staccare un testo
    ''' bianco da un'immagine che poteva essere di qualunque colore, mentre adesso sotto
    ''' c'è un riempimento pieno e chiaro deciso da noi, e sopra ci va un inchiostro scuro.
    ''' Un testo contornato su un fondo così sarebbe soltanto più sporco da leggere.
    ''' </remarks>
    Private Sub DisegnaIlTesto(g As Graphics)

        If String.IsNullOrEmpty(Me.Text) Then Return

        Dim discesa As Integer = If(_premuto, DiscesaDelPremuto, 0)
        Dim area As New Rectangle(0, discesa, Me.Width, Me.Height)

        Using formato As New StringFormat(StringFormatFlags.NoWrap)

            formato.Alignment = StringAlignment.Center
            formato.LineAlignment = StringAlignment.Center
            formato.Trimming = StringTrimming.EllipsisCharacter

            Using inchiostro As New SolidBrush(
                If(Me.Enabled, Me.ForeColor, StileApp.TestoSecondario))
                g.DrawString(Me.Text, Me.Font, inchiostro, area, formato)
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
        ' Un bottone che si spegne mentre il mouse gli sta sopra resterebbe scurito.
        If Not Me.Enabled Then
            _sopra = False
            _premuto = False
        End If
        Invalidate()
        MyBase.OnEnabledChanged(e)
    End Sub

End Class
