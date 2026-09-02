Imports System.Drawing
Imports System.Drawing.Drawing2D

''' <summary>
''' Il segno grande che l'AI sta lavorando: una <b>ruota di pallini</b> che gira al centro
''' dello schermo, con sotto una barra che si riempie (cap. 03.8).
''' </summary>
''' <remarks>
''' <para><b>Perché non bastava quel che c'era.</b> Fino al 2026-08-30 un'attesa di
''' decine di secondi si annunciava con due segnali piccoli: la rotellina del puntatore —
''' che si vede solo se il mouse è sopra la finestra e lo si sta guardando — e la riga
''' che si muove in fondo alla fascia di stato (<see cref="SegnaleDiAttesa"/>), che è alta
''' due righe in un angolo. Chi aspetta guarda altrove, e da lontano la finestra sembra
''' ferma. Questo si vede dall'altra parte della stanza.</para>
''' <para><b>Perché il marchio non c'è.</b> Fra il 2026-08-30 e il 2026-09-01 sotto la
''' ruota c'è stato lo <b>stemma Aviolab</b>, grande un terzo dello schermo: è stato
''' tolto, e il marchio è tornato a vivere in un posto solo, il pannello del logo in basso
''' a sinistra (cap. 03.5). Un'attesa non è il posto in cui firmarsi — chi guarda sta
''' aspettando, non ammirando — e lo stemma qui faceva due danni: prendeva il centro dello
''' schermo con qualcosa che non dice nulla sull'attesa, e obbligava la ruota a stargli
''' addosso, dove i pallini argento dovevano difendersi dal blu, dal bianco e dal giallo
''' delle stelle. Adesso la ruota sta sul suo sfondo e basta.</para>
''' <para><b>Qui c'è solo il conto e il disegno, non la finestra.</b> È la stessa
''' divisione di <see cref="SegnaleDiAttesa"/>, e per la stessa ragione: quel che può
''' rompersi — la misura sullo schermo, il fatto che la ruota giri davvero e torni al
''' punto di partenza, che la barra cresca e non torni mai indietro — si collauda su un
''' <c>Bitmap</c>, senza aprire niente. La finestra vera, che è codice di Windows e non di
''' logica, sta in <see cref="FinestraDiCaricamento"/> e si guarda con gli occhi.</para>
''' <para><i>Il nome della classe è rimasto quello dei giorni dello stemma: rinominarla
''' toccherebbe la finestra, il banco e la finestra principale, ed è un giro a parte.</i></para>
''' </remarks>
Public NotInheritable Class ScudoDiCaricamento

    ''' <summary>
    ''' Quanto dello schermo occupa l'indicatore: due decimi in orizzontale, un quarto in
    ''' verticale.
    ''' </summary>
    ''' <remarks>
    ''' <para>Sono <b>due limiti</b>, non due misure da imporre: la figura ha le sue
    ''' proporzioni — la ruota è tonda, e la barra sta sotto, larga quanto tutto — e si
    ''' prende la più grande che ci sta dentro tutti e due. Su uno schermo molto largo e
    ''' basso comanda l'altezza, su uno stretto e alto comanda la larghezza.</para>
    ''' <para>Il limite verticale era <b>due sesti</b> quando misurava il solo stemma, e il
    ''' complesso lo sforava perché la barra si aggiungeva dopo: su 1920 × 1080 erano 297 di
    ''' larghezza e 397 di altezza, contro i 360 dichiarati. Adesso vale per <b>tutto quel
    ''' che si vede</b>, ed è sceso a un quarto per una ragione precisa: togliendo lo stemma
    ''' la figura si è abbassata di molto, e con il vecchio limite sarebbe stata la sola
    ''' larghezza a comandare — l'indicatore sarebbe cresciuto di un terzo, e con lui la
    ''' barra che Mirco aveva tarato a video. A un quarto le misure restano quelle di prima:
    ''' su 1920 × 1080 fanno 316 × 269, con la barra spessa 23 px contro i 22 di ieri.</para>
    ''' </remarks>
    Public Const QuotaOrizzontale As Double = 2.0 / 10.0
    Public Const QuotaVerticale As Double = 1.0 / 4.0

    ''' <summary>
    ''' Di quanti pixel il complesso sta <b>più in alto</b> del centro dello schermo.
    ''' </summary>
    ''' <remarks>
    ''' <para>Non è la correzione di un conto sbagliato — il centro geometrico è giusto —
    ''' ma di come lo legge l'occhio: una figura appesa esattamente a metà di un rettangolo
    ''' sembra cadere verso il basso, e si guarda meglio un filo più su.</para>
    ''' <para>Il numero è stato <b>rifatto</b> quando lo stemma se n'è andato, non lasciato
    ''' dov'era. I trenta pixel chiesti da Mirco guardando a video il 2026-08-31 valevano
    ''' per un complesso alto 397 px su uno schermo comune — erano il 7,6% della sua
    ''' altezza. Quello di oggi, ruota e barra soltanto, è alto 269, e la stessa frazione fa
    ''' venti. Che venti sia anche il numero che Mirco aveva scelto la prima volta, quando
    ''' sotto lo stemma non c'era ancora la barra, è una conferma e non la ragione.</para>
    ''' <para>Resta in pixel e non in frazione perché è un numero deciso <b>guardando</b>, e
    ''' va riguardato: il conto qui sopra dice da dove ripartire, non che sia giusto. Vale
    ''' per il <b>complesso</b> — si sposta la finestra, non il disegno dentro di lei, o la
    ''' barra scivolerebbe addosso alla ruota.</para>
    ''' </remarks>
    Public Const AlzataInPixel As Integer = 20

    ''' <summary>Quanti pallini formano la ruota.</summary>
    Public Const Pallini As Integer = 12

    ''' <summary>Ogni quanto la ruota avanza di un pallino, in millisecondi.</summary>
    ''' <remarks>
    ''' Dodici pallini a 80 ms fanno un giro poco meno di un secondo: abbastanza lento da
    ''' non sembrare nervoso, abbastanza vivo da non sembrare fermo.
    ''' </remarks>
    Public Const IntervalloInMillisecondi As Integer = 80

    ''' <summary>Il raggio del cerchio su cui stanno i pallini, in frazione della larghezza.</summary>
    Private Const QuotaDelRaggio As Double = 0.3

    ''' <summary>Il raggio del pallino più grande, in frazione della larghezza.</summary>
    Private Const QuotaDelPallino As Double = 0.055

    ''' <summary>Quanto è larga la ruota <b>tutta</b>, pallini compresi.</summary>
    ''' <remarks>
    ''' Poco più di due terzi della tela, e la differenza non è aria sprecata: la barra
    ''' sotto è larga quanto la tela, la ruota no, e una barra un filo più larga del cerchio
    ''' che le sta sopra è ciò che le dà un principio e una fine. Questa quota però serve
    ''' soprattutto in <b>altezza</b> — la ruota è tonda, quindi è anche quanto è alta — e
    ''' di lì viene la fetta di tela che le si dà: v. <see cref="AltezzaDellaRuota"/>.
    ''' </remarks>
    Private Const QuotaDellaRuota As Double = (QuotaDelRaggio + QuotaDelPallino) * 2.0

    ''' <summary>Quanto resta visibile il pallino più spento, da 0 a 255.</summary>
    Private Const AlfaMinima As Integer = 35

    ''' <summary>Lo spessore della barra, in frazione della larghezza dell'indicatore.</summary>
    ''' <remarks>
    ''' Le misure della barra si dicono <b>in frazione della larghezza</b> e non in pixel,
    ''' per la ragione per cui la larghezza stessa si misura in frazione dello schermo: su
    ''' un monitor 4K, o a 150 punti per pollice, venti pixel sono un filo di capello. Così
    ''' invece la barra cresce col resto, e il gruppo resta quello che si è guardato a video.
    ''' <para>Il 7,4% — <b>ventitré pixel</b> su uno schermo comune — è la terza misura,
    ''' e le tre raccontano com'è andata: 5,5% scritto a tavolino, poi 6,5% e infine
    ''' questa, tutte e due chieste da Mirco guardando la barra a video il 2026-08-31.
    ''' Nessun conto ci sarebbe arrivato: quanto dev'essere spessa una barra perché si
    ''' veda bene da lontano è una cosa che si sa solo vedendola.</para>
    ''' <para>È di un pixel più alta dello stacco che la separa dalla ruota, e va bene
    ''' così: quel che conta è che ci resti dell'aria vera in mezzo, non che l'aria vinca
    ''' il confronto. Il banco sorveglia la proprietà, non il pareggio.</para>
    ''' </remarks>
    Public Const QuotaDelloSpessore As Double = 0.074

    ''' <summary>Quanto la barra sta staccata dalla ruota, in frazione della larghezza.</summary>
    ''' <remarks>
    ''' Lo stacco non è aria sprecata: appoggiata al piede della ruota la barra sembrerebbe
    ''' il pavimento su cui i pallini girano, e non la seconda cosa che dice l'attesa.
    ''' Quando sotto la ruota c'era lo stemma serviva a un'altra cosa ancora — tenere la
    ''' striscia verde fuori dal marchio — e la misura era la stessa.
    ''' </remarks>
    Public Const QuotaDelDistacco As Double = 0.07

    ''' <summary>
    ''' Quanto viene alto tutto l'indicatore, in frazione della sua larghezza.
    ''' </summary>
    ''' <remarks>
    ''' La ruota, l'aria e la barra, sommate. Serve a fare il conto <b>all'indietro</b>: il
    ''' limite dello schermo si dà sull'altezza, e la misura da cui discende tutto è invece
    ''' la larghezza. Si tiene qui, ricavata dalle tre quote vere, perché scriverla a mano
    ''' vorrebbe dire due numeri da tenere d'accordo — e il giorno che uno si muove da solo,
    ''' l'indicatore sfora il limite senza che nessuno se ne accorga.
    ''' </remarks>
    Private Const QuotaDellAltezza As Double =
        QuotaDellaRuota + QuotaDelDistacco + QuotaDelloSpessore

    ''' <summary>Quanto è lunga la sfumatura chiara sulla testa, in frazione della barra.</summary>
    Private Const QuotaDellaTesta As Double = 0.13

    ''' <summary>
    ''' Fin dove arriva la barra da sola: <b>mai</b> in fondo.
    ''' </summary>
    ''' <remarks>
    ''' <para>Il 95% non è timidezza, è l'unica cosa onesta da fare. Quanto durerà una
    ''' chiamata all'AI <b>non lo sa nessuno</b> — i tempi veri vanno dai venticinque
    ''' secondi di un'analisi ai cinquantasette di un CV con la lettera, e su una rete
    ''' lenta il doppio. Una barra che arrivasse in fondo da sola direbbe «finito» mentre
    ''' si sta ancora aspettando, e una barra che ha mentito una volta non la si guarda
    ''' più. Qui l'ultimo pezzo lo riempie il <b>fatto</b>: l'AI risponde, la barra scatta
    ''' a uno, e solo allora sparisce.</para>
    ''' <para>Non arrivare mai in fondo non vuol dire fermarsi: la curva rallenta e basta,
    ''' e anche dopo due minuti il riempimento cresce ancora di qualche centesimo. È il
    ''' filo di movimento che dice che il programma è vivo, che è poi tutto il mestiere di
    ''' questa barra.</para>
    ''' </remarks>
    Public Const RiempimentoMassimo As Double = 0.95

    ''' <summary>Dopo quanti secondi la barra è a poco più di metà corsa.</summary>
    Private Const SecondiDiScala As Double = 14.0

    ''' <summary>Quanto la curva è schiacciata: sotto 1 parte forte e si appiattisce.</summary>
    Private Const FormaDellaCurva As Double = 0.86

    ''' <summary>
    ''' La larghezza dell'indicatore: la misura da cui discende tutto il resto.
    ''' </summary>
    ''' <remarks>
    ''' Il limite orizzontale si applica com'è; quello verticale riguarda l'altezza, e qui
    ''' si comanda la larghezza — perciò si divide per <see cref="QuotaDellAltezza"/>, che è
    ''' il conto all'indietro. Si arrotonda per difetto e non al più vicino: i due limiti
    ''' sono limiti, e passarli di un pixel per un arrotondamento sarebbe passarli.
    ''' </remarks>
    Public Shared Function LarghezzaSulloSchermo(schermo As Size) As Integer

        If schermo.Width <= 0 OrElse schermo.Height <= 0 Then Return 0

        ' Si entra in tutti e due i limiti: comanda quello che si tocca per primo.
        Dim larghezza As Double = Math.Min(
            schermo.Width * QuotaOrizzontale,
            schermo.Height * QuotaVerticale / QuotaDellAltezza)

        Return Math.Max(1, CInt(Math.Floor(larghezza)))

    End Function

    ''' <summary>Lo spessore della barra sotto un indicatore largo così.</summary>
    Public Shared Function SpessoreDellaBarra(larghezza As Integer) As Integer
        Return Math.Max(3, CInt(Math.Round(larghezza * QuotaDelloSpessore)))
    End Function

    ''' <summary>Quanta aria fra il piede della ruota e la barra.</summary>
    Public Shared Function DistaccoDellaBarra(larghezza As Integer) As Integer
        Return Math.Max(2, CInt(Math.Round(larghezza * QuotaDelDistacco)))
    End Function

    ''' <summary>Quanto è alta la ruota: quanto è larga, perché è tonda.</summary>
    ''' <remarks>
    ''' È la fetta di tela che si dà alla ruota, e non è tutta la tela: la barra vuole la
    ''' sua in fondo. Finché sotto c'era lo stemma questa fetta era alta quanto <b>lui</b>,
    ''' e la ruota ci nuotava dentro perché è larga poco più di due terzi; adesso che lo
    ''' stemma non c'è, dargliela ancora così lascerebbe due dita di vuoto sopra e sotto i
    ''' pallini, e la barra sembrerebbe scivolata via da sola.
    ''' </remarks>
    Public Shared Function AltezzaDellaRuota(larghezza As Integer) As Integer
        Return Math.Max(1, CInt(Math.Round(larghezza * QuotaDellaRuota)))
    End Function

    ''' <summary>
    ''' Quanto ingombra tutto insieme: la ruota, l'aria e la barra.
    ''' </summary>
    ''' <remarks>
    ''' È l'altezza da cui <see cref="LarghezzaSulloSchermo"/> è partita all'indietro, e qui
    ''' si ricompone con le misure vere — quelle arrotondate, coi loro pavimenti — invece di
    ''' rifare la moltiplicazione. Chi disegna userà queste, e due conti che dicono quasi la
    ''' stessa cosa lascerebbero un pixel di tela senza padrone.
    ''' </remarks>
    Public Shared Function MisuraDelComplesso(schermo As Size) As Size

        Dim larghezza As Integer = LarghezzaSulloSchermo(schermo)
        If larghezza <= 0 Then Return Size.Empty

        Return New Size(
            larghezza,
            AltezzaDellaRuota(larghezza) + DistaccoDellaBarra(larghezza) +
            SpessoreDellaBarra(larghezza))

    End Function

    ''' <summary>
    ''' Quanta barra è piena dopo un certo tempo d'attesa, da 0 a
    ''' <see cref="RiempimentoMassimo"/>.
    ''' </summary>
    ''' <remarks>
    ''' <para>La forma è quella che Mirco ha scelto guardandola scritta in numeri: dopo
    ''' cinque secondi un terzo, dopo quindici due terzi, dopo trentacinque l'84%, dopo un
    ''' minuto il 92%, e da lì in poi centesimi. Cresce in fretta quando l'attesa può
    ''' ancora essere breve e rallenta quando diventa lunga — che è il contrario di quel
    ''' che la barra sa, e proprio per questo l'unica cosa che può dire senza sbagliare:
    ''' <b>più tempo passa, meno ne resta in proporzione</b>.</para>
    ''' <para>Il tempo si prende da fuori invece di leggerlo dall'orologio, come in
    ''' <see cref="SegnaleDiAttesa"/>: è quel che permette di collaudare la curva senza
    ''' aspettare un minuto per ogni asserzione.</para>
    ''' </remarks>
    Public Shared Function Riempimento(trascorsi As TimeSpan) As Double

        Dim secondi As Double = trascorsi.TotalSeconds
        If secondi <= 0.0 Then Return 0.0

        Return RiempimentoMassimo *
               (1.0 - Math.Exp(-Math.Pow(secondi / SecondiDiScala, FormaDellaCurva)))

    End Function

    ''' <summary>Dove va il complesso: in mezzo allo schermo che gli si dà.</summary>
    Public Shared Function RiquadroSulloSchermo(schermo As Rectangle) As Rectangle

        Dim misura As Size = MisuraDelComplesso(schermo.Size)
        If misura.IsEmpty Then Return Rectangle.Empty

        ' L'aria che avanza si divide in due, e il pixel dispari va sopra e a sinistra.
        ' Non è pignoleria: il complesso è alto 269 pixel su uno schermo comune, e con
        ' un'altezza dispari «la metà» sono due pixel diversi. Scegliendo di arrotondare
        ' per eccesso, il centro del complesso torna a cadere esattamente sull'alzata
        ' chiesta invece che un pixel più in basso — che è la misura decisa a video, e
        ' quella che il banco sorveglia.
        Return New Rectangle(
            schermo.Left + (schermo.Width - misura.Width + 1) \ 2,
            schermo.Top + (schermo.Height - misura.Height + 1) \ 2 - AlzataInPixel,
            misura.Width, misura.Height)

    End Function

    ''' <summary>Disegna il complesso: la ruota e la barra sotto.</summary>
    ''' <param name="area">La tela, grande quanto il <see cref="MisuraDelComplesso">complesso</see>.</param>
    ''' <param name="passo">Quanti scatti ha già fatto la ruota; cresce e basta.</param>
    ''' <param name="quotaPiena">Quanta barra è piena, da 0 a 1.</param>
    Public Shared Sub Disegna(disegno As Graphics, area As Size, passo As Integer,
                              quotaPiena As Double)

        If disegno Is Nothing OrElse area.Width <= 0 OrElse area.Height <= 0 Then Return

        ' La ruota non sa che sotto di lei c'è dell'altro: si dà la sua fetta di tela, e
        ' disegna al centro di quella. Se il centro lo prendesse dalla tela intera,
        ' scivolerebbe in giù di mezza barra.
        Dim soloRuota As New Size(area.Width, AltezzaDellaRuota(area.Width))

        If soloRuota.Height > 0 Then DisegnaLaRuota(disegno, soloRuota, passo)

        DisegnaLaBarra(disegno, area, quotaPiena)

    End Sub

    ''' <summary>
    ''' La ruota dei pallini.
    ''' </summary>
    ''' <remarks>
    ''' <para>È il simbolo di sempre, disegnato invece che animato: i pallini stanno
    ''' fermi sul loro cerchio, e a girare è <b>quale</b> di loro è acceso. Quello di
    ''' testa è pieno e grande, gli altri sbiadiscono all'indietro fino a
    ''' <see cref="AlfaMinima"/> — che non è zero apposta: una ruota con un buco dentro si
    ''' legge come un difetto, non come un movimento.</para>
    ''' <para>Ogni pallino ha un contorno d'ombra. Serviva contro lo stemma, che sotto
    ''' passava dal blu al bianco al giallo delle stelle e faceva sparire l'argento sulle
    ''' parti chiare; adesso che lo stemma non c'è serve <b>di più</b>, non di meno — la
    ''' finestra è trasparente, e sotto ci sono i pannelli dell'applicazione, che l'argento
    ''' se lo mangiano uguale.</para>
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

    ''' <summary>
    ''' La barra sotto la ruota: il fondo grigio, il verde che avanza, la testa chiara.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché è dipinta e non è un <c>ProgressBar</c>.</b> Un controllo qui non
    ''' si vedrebbe affatto: questa finestra non ha controlli: è un'unica immagine con il
    ''' suo alfa consegnata a Windows in un colpo solo
    ''' (<see cref="FinestraDiCaricamento"/>), e un figlio dentro non finirebbe mai in
    ''' quell'immagine. Perciò i colori si sono campionati sulla barra di sistema — uno per
    ''' uno, dall'immagine che Mirco ha portato — e stanno fra i token di
    ''' <see cref="StileApp"/> come tutti gli altri.</para>
    ''' <para>Le sfumature sono due e fanno due mestieri. Quella <b>verticale</b>, appena
    ''' percettibile, toglie alla striscia l'aria del rettangolo dipinto. Quella
    ''' <b>orizzontale</b> sulla punta è invece la cosa che si vede muoversi: schiarisce
    ''' l'ultimo ottavo del pieno fino al verde acceso, ed è ciò che nella barra vera
    ''' sembra un effetto fluorescente.</para>
    ''' </remarks>
    Private Shared Sub DisegnaLaBarra(disegno As Graphics, area As Size, quotaPiena As Double)

        Dim spessore As Integer = SpessoreDellaBarra(area.Width)
        Dim tutta As New Rectangle(0, area.Height - spessore, area.Width, spessore)
        If tutta.Width < 4 OrElse tutta.Height < 3 Then Return

        ' Niente antialias e nessuno spostamento di mezzo pixel: la barra è fatta di
        ' rettangoli, e i bordi sfumati la farebbero sembrare fuori fuoco. Si dichiarano
        ' tutte e due, e non si lasciano come stanno: chi ha disegnato la ruota un attimo
        ' fa le ha messe a modo suo, che è il contrario di quel che serve qui.
        disegno.SmoothingMode = SmoothingMode.None
        disegno.PixelOffsetMode = PixelOffsetMode.None

        ' Il filetto non è decorazione: la parte ancora da riempire è grigio chiaro, e
        ' dietro ci può essere un desktop altrettanto chiaro. Senza contorno la barra
        ' comincerebbe dove finisce il verde, e sembrerebbe corta invece che vuota.
        ' Si dipinge come fondo e non con una penna, perché una penna larga un pixel
        ' cade a destra o a sinistra della riga secondo come è messo il disegno, e qui
        ' quella riga è il confine fra la barra e quel che c'è fuori.
        Using pennello As New SolidBrush(StileApp.BordoDiAttesa)
            disegno.FillRectangle(pennello, tutta)
        End Using

        Dim dentro As Rectangle = Rectangle.Inflate(tutta, -1, -1)

        Using pennello As New SolidBrush(StileApp.FondoDiAttesa)
            disegno.FillRectangle(pennello, dentro)
        End Using
        Dim quanto As Integer = CInt(Math.Round(
            dentro.Width * Math.Max(0.0, Math.Min(1.0, quotaPiena))))
        If quanto <= 0 Then Return

        Dim pieno As New Rectangle(dentro.X, dentro.Y, quanto, dentro.Height)

        DipingiIlPieno(disegno, pieno)
        DipingiLaTesta(disegno, pieno, dentro.Width)

    End Sub

    ''' <summary>Il verde che avanza, con la sua sfumatura verticale.</summary>
    ''' <remarks>
    ''' Il rettangolo dato al pennello è più alto di due pixel di quello che si riempie: è
    ''' il modo consueto di tenere fuori dal disegno l'ultima riga di un
    ''' <c>LinearGradientBrush</c>, che altrimenti riprende il primo colore invece
    ''' dell'ultimo e lascia un filo scuro proprio sul bordo.
    ''' </remarks>
    Private Shared Sub DipingiIlPieno(disegno As Graphics, pieno As Rectangle)

        Dim disteso As New Rectangle(pieno.X, pieno.Y - 1, pieno.Width, pieno.Height + 2)

        Using pennello As New LinearGradientBrush(
            disteso, StileApp.VerdeDiAttesa, StileApp.VerdeSulFondo,
            LinearGradientMode.Vertical)

            ' Scuro per più di metà e poi schiarisce: è la misura presa sulla barra vera,
            ' dove la sfumatura non è distribuita ma sta tutta nell'ultimo terzo.
            ' Attenzione: questi tre colori sostituiscono in pieno i due dati al
            ' costruttore qui sopra, che dopo questa riga non dipingono più niente. Chi
            ' volesse cambiare il verde della barra e lo cambiasse là non vedrebbe
            ' cambiare nulla — provato apposta.
            pennello.InterpolationColors = New ColorBlend(3) With {
                .Colors = New Color() {StileApp.VerdeDiAttesa,
                                       StileApp.VerdeDiAttesa,
                                       StileApp.VerdeSulFondo},
                .Positions = New Single() {0.0F, 0.55F, 1.0F}}

            disegno.FillRectangle(pennello, pieno)

        End Using

    End Sub

    ''' <summary>
    ''' La punta accesa: un velo di verde chiaro che da trasparente diventa pieno.
    ''' </summary>
    ''' <remarks>
    ''' La sua lunghezza si misura sulla <b>barra intera</b>, non sul pieno: così la punta
    ''' è sempre quella, e non un'unghia all'inizio che si allarga mentre la barra avanza.
    ''' Nei primi istanti, quando il pieno è più corto della punta, si accorcia lei.
    ''' </remarks>
    Private Shared Sub DipingiLaTesta(disegno As Graphics, pieno As Rectangle,
                                      larghezzaDellaBarra As Integer)

        Dim lunga As Integer = Math.Max(1, CInt(Math.Round(larghezzaDellaBarra * QuotaDellaTesta)))
        If lunga > pieno.Width Then lunga = pieno.Width

        Dim testa As New Rectangle(pieno.Right - lunga, pieno.Y, lunga, pieno.Height)
        Dim disteso As New Rectangle(testa.X - 1, testa.Y, testa.Width + 2, testa.Height)

        Using pennello As New LinearGradientBrush(
            disteso, Color.FromArgb(0, StileApp.VerdeInTesta), StileApp.VerdeInTesta,
            LinearGradientMode.Horizontal)

            disegno.FillRectangle(pennello, testa)

        End Using

    End Sub

End Class
