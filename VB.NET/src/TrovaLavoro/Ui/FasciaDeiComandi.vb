Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms

''' <summary>
''' La fascia dei comandi in fondo a un pannello (cap. 03.4): i comandi di quel pannello a
''' sinistra, quelli che portano altrove a destra, e le azioni critiche su una riga tutta
''' loro. Sa <b>andare a capo</b> quando in una riga non ci stanno più.
''' </summary>
''' <remarks>
''' <para>Nasce il 2026-08-14 dalla stessa disposizione ricopiata in cinque pannelli. Ogni
''' copia metteva una fila da sinistra e una da destra, ognuna ignara dell'altra, e quando
''' lo spazio finiva <b>si incontravano a metà strada</b>: in P2, alla larghezza minima
''' della finestra, fino a 676 px di bottoni sopra altri bottoni. Non si vedeva perché
''' l'applicazione si apre massimizzata — ed è per questo che è rimasto lì per tre tappe,
''' peggiorando a ogni bottone aggiunto.</para>
''' <para>Il rimedio in un posto solo vale per tutti: un pannello che domani aggiunge un
''' comando non ha una geometria sua da rifare, e il difetto non può tornare da una parte
''' sola. Ai pannelli resta da dichiarare <b>cosa</b> va dove, che è l'unica cosa che
''' sanno loro e questa classe no.</para>
''' </remarks>
Public NotInheritable Class FasciaDeiComandi

    ''' <summary>
    ''' Quanto vuoto tiene un'azione critica lontana dagli altri comandi. Non è una
    ''' spaziatura come le altre: è una difesa (cap. 11.5), e per questo vale il doppio
    ''' della distanza normale fra due controlli.
    ''' </summary>
    Public Const StaccoDelCritico As Integer = 2 * StileApp.DistanzaControlli

    Private ReadOnly _fascia As Panel
    Private ReadOnly _aSinistra As New List(Of Button)
    Private ReadOnly _aDestra As New List(Of Button)
    Private ReadOnly _critici As New List(Of Button)
    Private _racconto As Label

    ''' <param name="fascia">Il pannello in fondo, quello che cede il posto al logo.</param>
    Public Sub New(fascia As Panel)

        If fascia Is Nothing Then Throw New ArgumentNullException(NameOf(fascia))
        _fascia = fascia

    End Sub

    ''' <summary>I comandi del pannello: si allineano a sinistra, nell'ordine dato.</summary>
    Public Sub ASinistra(ParamArray bottoni As Button())
        _aSinistra.AddRange(bottoni)
    End Sub

    ''' <summary>Quelli che portano altrove: si allineano a destra, nell'ordine dato.</summary>
    Public Sub ADestra(ParamArray bottoni As Button())
        _aDestra.AddRange(bottoni)
    End Sub

    ''' <summary>
    ''' Le azioni da cui non si torna indietro (livello 6, cap. 03.3): riga tutta loro, in
    ''' cima e a destra, staccate dal resto. Non finiscono mai sotto il dito di chi sta
    ''' premendo il comando accanto.
    ''' </summary>
    Public Sub Critici(ParamArray bottoni As Button())
        _critici.AddRange(bottoni)
    End Sub

    ''' <summary>
    ''' La riga che racconta com'è andata (cap. 03.8): sta nel vuoto fra i dati e i
    ''' comandi, <b>centrata sulla schermata</b>. Dichiararla è facoltativo; un pannello
    ''' che non lo fa ha la fascia di prima, e la fascia non le tiene nessun posto.
    ''' </summary>
    ''' <remarks>
    ''' <para>Fino al 2026-09-08 questa riga stava <b>in alto a destra</b>, nell'intestazione
    ''' di ogni pannello: lontana dal punto in cui si guarda mentre si lavora — la casella
    ''' in cui si scrive e il bottone che si sta per premere — e in un angolo che l'occhio
    ''' visita solo se sa già che c'è qualcosa. Un errore raccontato là è un errore
    ''' raccontato a nessuno. Adesso compare fra la casella e i bottoni, cioè sulla strada
    ''' che l'occhio fa comunque.</para>
    ''' <para><b>Perché la posa la fascia e non il pannello.</b> Lo spazio che resta sopra i
    ''' comandi non lo sa il pannello: dipende da quante righe i bottoni hanno preso, e
    ''' quelle le decide questa classe a ogni ridimensionamento. Sette geometrie ricopiate
    ''' tornerebbero a divergere come divergevano le due file di bottoni prima del
    ''' 2026-08-14.</para>
    ''' </remarks>
    Public Sub Racconta(riga As Label)
        _racconto = riga
    End Sub

    ''' <summary>
    ''' Rifà la disposizione. Va chiamata a ogni cambio di ingombro del logo e a ogni
    ''' ridimensionamento del pannello, perché entrambi cambiano il rettangolo disponibile.
    ''' </summary>
    ''' <param name="altezzaMinima">
    ''' Sotto questa altezza la fascia non scende: è quella che il pannello pretende per
    ''' sé — l'ingombro del logo, di norma. Se le righe ne chiedono di più, vince la loro:
    ''' a rimetterci è l'area dei dati, che di spazio ne ha, non la leggibilità dei comandi.
    ''' </param>
    Public Sub Disponi(altezzaMinima As Integer)

        Dim righe As List(Of RigaDiComandi) = ComponiLeRighe()

        ' L'altezza va decisa prima di posare i bottoni: le righe si contano a partire dal
        ' fondo della fascia, e il fondo si sposta se la fascia deve crescere.
        Dim altezza As Integer = Math.Max(altezzaMinima, AltezzaNecessaria(righe))
        If _fascia.Height <> altezza Then _fascia.Height = altezza

        PosaIlRacconto(PosaLeRighe(righe))

    End Sub

    ''' <summary>
    ''' Come si dividono i comandi fra le righe. Il caso di sempre — l'applicazione
    ''' massimizzata — resta quello di prima: una riga sola, i comandi del pannello a
    ''' sinistra e le uscite a destra. Le righe sono in ordine dall'alto verso il basso.
    ''' </summary>
    Private Function ComponiLeRighe() As List(Of RigaDiComandi)

        Dim disponibile As Integer = _fascia.ClientSize.Width -
                                     _fascia.Padding.Left - StileApp.MargineRiquadro

        ' Un comando nascosto non occupa posto (R7, 2026-08-23). Finora nessun pannello ne
        ' aveva: il primo è «Rigenera la lettera» di P6, che c'è solo quando la lettera è
        ' rimasta indietro rispetto al CV. Senza questo filtro lascerebbe il suo buco in
        ' fascia anche da invisibile, e — peggio — potrebbe mandare a capo gli altri per
        ' fare spazio a sé stesso.
        Dim aSinistra As List(Of Button) = Visibili(_aSinistra)
        Dim aDestra As List(Of Button) = Visibili(_aDestra)

        Dim righe As New List(Of RigaDiComandi)

        For Each critico As Button In Visibili(_critici)
            Dim sua As New RigaDiComandi(StaccoDelCritico)
            sua.Aggiungi(critico, versoDestra:=True)
            righe.Add(sua)
        Next

        If Larghezza(aSinistra) + StileApp.DistanzaControlli + Larghezza(aDestra) <= disponibile Then
            Dim unica As New RigaDiComandi(StileApp.InterlineaMinima)
            unica.ASinistra.AddRange(aSinistra)
            unica.ADestra.AddRange(aDestra)
            righe.Add(unica)
            Return righe
        End If

        ' Insieme non ci stanno: ogni fila prende le righe che le servono, e quelle che
        ' portano altrove restano in fondo — il comando principale di un pannello si cerca
        ' in basso a destra, e deve restare dov'era.
        righe.AddRange(Spezzata(aSinistra, disponibile, versoDestra:=False))
        righe.AddRange(Spezzata(aDestra, disponibile, versoDestra:=True))

        Return righe

    End Function

    ''' <summary>
    ''' Una fila di bottoni divisa nelle righe che le servono, riempiendo ogni riga finché
    ''' ci stanno. Un bottone più largo dello spazio disponibile resta comunque da solo
    ''' sulla sua riga: sarà largo quanto la fascia, ma non finirà sotto un altro.
    ''' </summary>
    Private Shared Function Spezzata(bottoni As List(Of Button), disponibile As Integer,
                                     versoDestra As Boolean) As List(Of RigaDiComandi)

        Dim righe As New List(Of RigaDiComandi)
        Dim corrente As RigaDiComandi = Nothing

        For Each bottone As Button In bottoni

            If corrente IsNot Nothing AndAlso
               corrente.Larghezza + StileApp.DistanzaControlli + bottone.Width > disponibile Then
                corrente = Nothing
            End If

            If corrente Is Nothing Then
                corrente = New RigaDiComandi(StileApp.InterlineaMinima)
                righe.Add(corrente)
            End If

            corrente.Aggiungi(bottone, versoDestra)

        Next

        Return righe

    End Function

    ''' <summary>
    ''' Posa le righe dal fondo della fascia verso l'alto, e dice a che altezza è arrivata:
    ''' la <b>cima</b> dei comandi è il pavimento del racconto.
    ''' </summary>
    Private Function PosaLeRighe(righe As List(Of RigaDiComandi)) As Integer

        Dim riga As Integer = _fascia.Height - StileApp.MargineRiquadro - StileApp.BottoneStandard.Height

        For indice As Integer = righe.Count - 1 To 0 Step -1

            Dim sinistra As Integer = _fascia.Padding.Left
            For Each bottone As Button In righe(indice).ASinistra
                bottone.Location = New Point(sinistra, riga)
                sinistra += bottone.Width + StileApp.DistanzaControlli
            Next

            ' A destra si posa a ritroso, dall'ultimo bottone al primo: è così che l'ordine
            ' in cui si leggono resta quello in cui sono stati dichiarati.
            Dim destra As Integer = _fascia.ClientSize.Width - StileApp.MargineRiquadro
            For indietro As Integer = righe(indice).ADestra.Count - 1 To 0 Step -1
                Dim bottone As Button = righe(indice).ADestra(indietro)
                destra -= bottone.Width
                bottone.Location = New Point(destra, riga)
                destra -= StileApp.DistanzaControlli
            Next

            ' Lo stacco che conta è quello della riga di sopra: è lei a dichiarare quanto
            ' vuoto vuole sotto di sé.
            If indice > 0 Then
                riga -= StileApp.BottoneStandard.Height + righe(indice - 1).StaccoSotto
            End If

        Next

        ' Senza righe da posare il ciclo non gira: la cima resta quella della riga che ci
        ' sarebbe stata, che è esattamente il pavimento giusto per chi sta sopra.
        Return riga

    End Function

    ''' <summary>
    ''' Posa la riga che racconta nel vuoto fra i dati e i comandi.
    ''' </summary>
    ''' <param name="cima">L'ordinata della riga di comandi più alta.</param>
    Private Sub PosaIlRacconto(cima As Integer)

        If _racconto Is Nothing Then Return

        ' Il vuoto utile: dall'interlinea sotto il bordo alto della fascia fino
        ' all'interlinea sopra il primo bottone. Il testo ci sta in mezzo (MiddleCenter),
        ' quindi quando il vuoto è grande la riga galleggia al centro invece di
        ' appiccicarsi a uno dei due.
        Dim altezza As Integer = cima - 2 * StileApp.InterlineaMinima

        If altezza <= 0 Then
            ' Sopra i comandi non è rimasto niente. Una riga che non si vede è un difetto;
            ' una riga scritta sopra i bottoni sono due.
            _racconto.SetBounds(_fascia.Padding.Left, 0, 0, 0)
            Return
        End If

        ' Lo stesso margine da una parte e dall'altra, ed è il logo a dettarlo perché è
        ' l'ostacolo più largo dei due. Così il centro della riga cade sul centro della
        ' schermata — sotto la casella di testo che le sta sopra — e non sul centro dello
        ' spazio che avanza, che l'occhio leggerebbe come «spostata a destra».
        Dim margine As Integer = Math.Max(_fascia.Padding.Left, StileApp.MargineRiquadro)
        Dim larghezza As Integer = _fascia.ClientSize.Width - 2 * margine

        ' Quando la fascia è così stretta che i due margini se la mangiano, di centratura
        ' non se ne parla più: si prende tutto quel che resta a destra del logo. Una riga
        ' leggibile e spostata vale più di una riga centrata e larga niente.
        If larghezza < StileApp.BottoneLargo.Width Then
            margine = _fascia.Padding.Left
            larghezza = _fascia.ClientSize.Width - margine - StileApp.MargineRiquadro
        End If

        _racconto.SetBounds(margine, StileApp.InterlineaMinima, Math.Max(0, larghezza), altezza)

    End Sub

    ''' <summary>
    ''' L'altezza che la fascia deve avere perché tutte le righe ci stiano: il margine del
    ''' riquadro sotto, l'interlinea minima sopra, in mezzo le righe con i loro stacchi, e
    ''' — se c'è una riga che racconta — il posto che le tocca.
    ''' </summary>
    Private Function AltezzaNecessaria(righe As List(Of RigaDiComandi)) As Integer

        Dim altezza As Integer = StileApp.MargineRiquadro + StileApp.InterlineaMinima

        For indice As Integer = 0 To righe.Count - 1
            altezza += StileApp.BottoneStandard.Height
            If indice < righe.Count - 1 Then altezza += righe(indice).StaccoSotto
        Next

        Return altezza + AltezzaDelRacconto()

    End Function

    ''' <summary>
    ''' Quante righe di testo la fascia garantisce al racconto, comunque vada. <b>Tre</b>
    ''' perché tre ne ha il più lungo dei messaggi a testo fisso — quello di P7 quando il
    ''' destinatario viene dall'annuncio <i>e</i> la rifinitura non è riuscita.
    ''' </summary>
    ''' <remarks>
    ''' È un <b>pavimento</b>, non una misura: dove lo spazio c'è — cioè quasi sempre — il
    ''' racconto si prende tutto il vuoto sopra i comandi, che alla larghezza normale sono
    ''' otto righe abbondanti. L'unico messaggio che può sfondare anche tre righe è
    ''' l'elenco dei file esportati di P6, che cresce con quanti sono; in una finestra
    ''' stretta quello finisce nei puntini di <c>AutoEllipsis</c>, e si è scelto così
    ''' invece di rubare all'area dei dati un posto che quasi nessuno userebbe.
    ''' </remarks>
    Private Const RigheDelRacconto As Integer = 3

    ''' <summary>
    ''' Il posto che la fascia tiene da parte per il racconto: le sue righe di testo, più
    ''' l'interlinea che le stacca dai comandi.
    ''' </summary>
    ''' <remarks>
    ''' Di norma non serve a niente, e va bene così: la fascia è già alta 188 px perché
    ''' deve cedere l'angolo al pannello del logo (cap. 03.5), e sopra i bottoni ne avanzano
    ''' più di cento. Serve nel caso stretto — sotto i 1350 px il logo passa in compatta e
    ''' la fascia scende a 68 — dove senza questo conto al racconto resterebbero sei pixel.
    ''' Il posto si tiene <b>sempre</b>, anche a riga vuota: una fascia che cresce quando
    ''' compare un avviso farebbe ballare l'area dei dati proprio nel momento in cui c'è
    ''' qualcosa da leggere.
    ''' Le righe si misurano sul <b>font della riga</b> e non in pixel fissi, perché con
    ''' <c>AutoScaleMode.Font</c> a 150% il carattere cresce e un numero scritto a mano no.
    ''' </remarks>
    Private Function AltezzaDelRacconto() As Integer

        If _racconto Is Nothing Then Return 0

        Return StileApp.InterlineaMinima + RigheDelRacconto * _racconto.Font.Height

    End Function

    ''' <summary>Quanto spazio vuole una fila di bottoni messi in riga.</summary>
    ''' <summary>
    ''' Solo i bottoni che si vedono: gli altri non sono in fascia, e la fascia non deve
    ''' tenere loro il posto (R7).
    ''' </summary>
    Private Shared Function Visibili(bottoni As List(Of Button)) As List(Of Button)

        Return bottoni.Where(Function(b) b IsNot Nothing AndAlso b.Visible).ToList()

    End Function

    Private Shared Function Larghezza(bottoni As List(Of Button)) As Integer

        Dim totale As Integer = 0
        For Each bottone As Button In bottoni
            If totale > 0 Then totale += StileApp.DistanzaControlli
            totale += bottone.Width
        Next

        Return totale

    End Function

    ''' <summary>
    ''' Una riga della fascia: i bottoni che ci stanno, da che parte si allineano, e quanto
    ''' vuoto tiene sotto di sé.
    ''' </summary>
    Private NotInheritable Class RigaDiComandi

        Public Sub New(staccoSotto As Integer)
            _StaccoSotto = staccoSotto
        End Sub

        ''' <summary>I bottoni allineati al bordo sinistro, nell'ordine in cui si leggono.</summary>
        Public ReadOnly Property ASinistra As New List(Of Button)

        ''' <summary>I bottoni allineati al bordo destro, nell'ordine in cui si leggono.</summary>
        Public ReadOnly Property ADestra As New List(Of Button)

        ''' <summary>Il vuoto fra questa riga e quella sotto.</summary>
        Public ReadOnly Property StaccoSotto As Integer

        ''' <summary>Quanto spazio si prende la riga, comprese le distanze fra i bottoni.</summary>
        Public ReadOnly Property Larghezza As Integer
            Get
                Dim totale As Integer = 0
                For Each bottone As Button In ASinistra
                    If totale > 0 Then totale += StileApp.DistanzaControlli
                    totale += bottone.Width
                Next
                For Each bottone As Button In ADestra
                    If totale > 0 Then totale += StileApp.DistanzaControlli
                    totale += bottone.Width
                Next
                Return totale
            End Get
        End Property

        ''' <remarks>
        ''' Attenzione al nome del parametro: in VB le maiuscole non distinguono, e
        ''' chiamarlo <c>aDestra</c> lo farebbe scambiare per la proprietà
        ''' <see cref="ADestra"/> — che qui dentro diventerebbe un <c>Boolean</c>.
        ''' </remarks>
        Public Sub Aggiungi(bottone As Button, versoDestra As Boolean)
            If versoDestra Then ADestra.Add(bottone) Else ASinistra.Add(bottone)
        End Sub

    End Class

End Class
