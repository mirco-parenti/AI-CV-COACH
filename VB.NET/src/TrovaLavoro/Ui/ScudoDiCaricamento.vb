Imports System.Drawing
Imports System.Drawing.Drawing2D

''' <summary>
''' Il segno grande che l'AI sta lavorando: lo <b>stemma Aviolab</b> al centro dello
''' schermo, con sopra una ruota di pallini che gira (cap. 03.8).
''' </summary>
''' <remarks>
''' <para><b>Perché non bastava quel che c'era.</b> Fino al 2026-08-30 un'attesa di
''' decine di secondi si annunciava con due segnali piccoli: la rotellina del puntatore —
''' che si vede solo se il mouse è sopra la finestra e lo si sta guardando — e la riga
''' che si muove in fondo alla fascia di stato (<see cref="SegnaleDiAttesa"/>), che è alta
''' due righe in un angolo. Chi aspetta guarda altrove, e da lontano la finestra sembra
''' ferma. Questo si vede dall'altra parte della stanza.</para>
''' <para><b>Qui c'è solo il conto e il disegno, non la finestra.</b> È la stessa
''' divisione di <see cref="SegnaleDiAttesa"/>, e per la stessa ragione: quel che può
''' rompersi — la misura sullo schermo, le proporzioni dello scudo, il fatto che la ruota
''' giri davvero e torni al punto di partenza — si collauda su un <c>Bitmap</c>, senza
''' aprire niente. La finestra vera, che è codice di Windows e non di logica, sta in
''' <see cref="FinestraDiCaricamento"/> e si guarda con gli occhi.</para>
''' </remarks>
Public NotInheritable Class ScudoDiCaricamento

    ''' <summary>
    ''' Quanto dello schermo occupa lo scudo: due decimi in orizzontale, due sesti in
    ''' verticale.
    ''' </summary>
    ''' <remarks>
    ''' <para>Sono le misure chieste da Mirco, e sono <b>due limiti</b>, non due misure da
    ''' imporre: lo scudo ha le sue proporzioni — 200 × 242, più alto che largo — e
    ''' stirarlo per riempire esattamente un rettangolo che non è il suo vorrebbe dire
    ''' deformare il marchio, che è la sola cosa che il marchio non tollera. Perciò si
    ''' prende il più grande scudo <b>non deformato</b> che ci sta dentro tutti e due:
    ''' su uno schermo 1920 × 1080 fanno 297 × 360 invece di 384 × 360.</para>
    ''' <para>Su uno schermo molto largo e basso comanda l'altezza, su uno stretto e alto
    ''' comanda la larghezza; in ogni caso nessuno dei due limiti si sfora.</para>
    ''' </remarks>
    Public Const QuotaOrizzontale As Double = 2.0 / 10.0
    Public Const QuotaVerticale As Double = 2.0 / 6.0

    ''' <summary>Quanti pallini formano la ruota.</summary>
    Public Const Pallini As Integer = 12

    ''' <summary>Ogni quanto la ruota avanza di un pallino, in millisecondi.</summary>
    ''' <remarks>
    ''' Dodici pallini a 80 ms fanno un giro poco meno di un secondo: abbastanza lento da
    ''' non sembrare nervoso, abbastanza vivo da non sembrare fermo.
    ''' </remarks>
    Public Const IntervalloInMillisecondi As Integer = 80

    ''' <summary>Il raggio della ruota, in frazione della larghezza dello scudo.</summary>
    Private Const QuotaDelRaggio As Double = 0.3

    ''' <summary>Il raggio del pallino più grande, in frazione della larghezza.</summary>
    Private Const QuotaDelPallino As Double = 0.055

    ''' <summary>Quanto resta visibile il pallino più spento, da 0 a 255.</summary>
    Private Const AlfaMinima As Integer = 35

    ''' <summary>
    ''' Il più grande scudo non deformato che sta dentro i due limiti dello schermo.
    ''' </summary>
    Public Shared Function MisuraSulloSchermo(schermo As Size) As Size

        Dim scudo As Rectangle = LogoAviolab.ScudoDentroLaTela
        If schermo.Width <= 0 OrElse schermo.Height <= 0 OrElse
           scudo.Width <= 0 OrElse scudo.Height <= 0 Then Return Size.Empty

        Dim largoAlMassimo As Double = schermo.Width * QuotaOrizzontale
        Dim altoAlMassimo As Double = schermo.Height * QuotaVerticale

        ' Si entra in tutti e due i limiti: comanda quello che si tocca per primo.
        Dim larghezza As Double = Math.Min(
            largoAlMassimo, altoAlMassimo * scudo.Width / scudo.Height)

        Return New Size(
            Math.Max(1, CInt(Math.Round(larghezza))),
            Math.Max(1, CInt(Math.Round(larghezza * scudo.Height / scudo.Width))))

    End Function

    ''' <summary>Dove va lo scudo: in mezzo allo schermo che gli si dà.</summary>
    Public Shared Function RiquadroSulloSchermo(schermo As Rectangle) As Rectangle

        Dim misura As Size = MisuraSulloSchermo(schermo.Size)
        If misura.IsEmpty Then Return Rectangle.Empty

        Return New Rectangle(
            schermo.Left + (schermo.Width - misura.Width) \ 2,
            schermo.Top + (schermo.Height - misura.Height) \ 2,
            misura.Width, misura.Height)

    End Function

    ''' <summary>Disegna lo scudo e la sua ruota, al passo che gli si dice.</summary>
    ''' <param name="area">La tela, che è grande quanto lo scudo e nient'altro.</param>
    ''' <param name="passo">Quanti scatti ha già fatto la ruota; cresce e basta.</param>
    Public Shared Sub Disegna(disegno As Graphics, area As Size, passo As Integer)

        If disegno Is Nothing OrElse area.Width <= 0 OrElse area.Height <= 0 Then Return

        DisegnaLoScudo(disegno, area)
        DisegnaLaRuota(disegno, area, passo)

    End Sub

    ''' <summary>
    ''' Lo stemma, grande quanto la tela e centrato su di essa.
    ''' </summary>
    ''' <remarks>
    ''' Il conto è quello del mega stemma del menu (cap. 03.6), e per lo stesso motivo:
    ''' il PNG ha dell'aria trasparente attorno allo scudo, quindi una tela alta quanto lo
    ''' scudo darebbe uno scudo più basso del 6%. La tela si chiede <b>in proporzione</b>,
    ''' e poi si sposta perché sia lo <b>scudo</b> a stare in mezzo.
    ''' </remarks>
    Private Shared Sub DisegnaLoScudo(disegno As Graphics, area As Size)

        Dim scudo As Rectangle = LogoAviolab.ScudoDentroLaTela
        If scudo.Height <= 0 Then Return

        Dim lato As Integer = Math.Max(1, CInt(Math.Round(
            area.Height * LogoAviolab.LatoDellaTela / CDbl(scudo.Height))))

        Dim centroX As Double = (scudo.Left + scudo.Right) / 2.0 / LogoAviolab.LatoDellaTela
        Dim centroY As Double = (scudo.Top + scudo.Bottom) / 2.0 / LogoAviolab.LatoDellaTela

        disegno.InterpolationMode = InterpolationMode.HighQualityBicubic
        disegno.PixelOffsetMode = PixelOffsetMode.HighQuality

        Using stemma As Bitmap = LogoAviolab.Genera(lato)
            disegno.DrawImage(stemma, New Rectangle(
                CInt(Math.Round(area.Width / 2.0 - lato * centroX)),
                CInt(Math.Round(area.Height / 2.0 - lato * centroY)),
                lato, lato))
        End Using

    End Sub

    ''' <summary>
    ''' La ruota dei pallini, sopra lo scudo.
    ''' </summary>
    ''' <remarks>
    ''' <para>È il simbolo di sempre, disegnato invece che animato: i pallini stanno
    ''' fermi sul loro cerchio, e a girare è <b>quale</b> di loro è acceso. Quello di
    ''' testa è pieno e grande, gli altri sbiadiscono all'indietro fino a
    ''' <see cref="AlfaMinima"/> — che non è zero apposta: una ruota con un buco dentro si
    ''' legge come un difetto, non come un movimento.</para>
    ''' <para>Ogni pallino ha un contorno d'ombra perché lo scudo, sotto, non è di un
    ''' colore solo: passa dal blu al bianco al giallo delle stelle, e l'argento da solo
    ''' sparirebbe proprio sulle parti chiare.</para>
    ''' </remarks>
    Private Shared Sub DisegnaLaRuota(disegno As Graphics, area As Size, passo As Integer)

        Dim raggio As Double = area.Width * QuotaDelRaggio
        Dim grande As Double = area.Width * QuotaDelPallino
        Dim centro As New PointF(area.Width / 2.0F, area.Height / 2.0F)

        disegno.SmoothingMode = SmoothingMode.AntiAlias

        For pallino As Integer = 0 To Pallini - 1

            ' Quanti scatti fa che questo pallino era in testa: 0 è quello di adesso.
            Dim quanti As Integer = (((pallino - passo) Mod Pallini) + Pallini) Mod Pallini
            Dim vivacita As Double = 1.0 - quanti / CDbl(Pallini)

            Dim alfa As Integer = CInt(Math.Round(AlfaMinima + (255 - AlfaMinima) * vivacita))
            Dim misura As Double = grande * (0.55 + 0.45 * vivacita)

            ' Si parte dalle dodici e si gira in senso orario, come un orologio.
            Dim angolo As Double = -Math.PI / 2.0 + 2.0 * Math.PI * pallino / Pallini
            Dim x As Double = centro.X + raggio * Math.Cos(angolo)
            Dim y As Double = centro.Y + raggio * Math.Sin(angolo)

            Dim dove As New RectangleF(
                CSng(x - misura), CSng(y - misura), CSng(misura * 2), CSng(misura * 2))

            Using pennello As New SolidBrush(Color.FromArgb(alfa, StileApp.ArgentoDiAttesa))
                disegno.FillEllipse(pennello, dove)
            End Using

            Using penna As New Pen(Color.FromArgb(alfa \ 2, StileApp.OmbraDiAttesa),
                                   Math.Max(1.0F, CSng(misura * 0.18)))
                disegno.DrawEllipse(penna, dove)
            End Using

        Next

    End Sub

End Class
