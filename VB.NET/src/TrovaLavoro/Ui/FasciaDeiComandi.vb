Imports System.Drawing
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

        PosaLeRighe(righe)

    End Sub

    ''' <summary>
    ''' Come si dividono i comandi fra le righe. Il caso di sempre — l'applicazione
    ''' massimizzata — resta quello di prima: una riga sola, i comandi del pannello a
    ''' sinistra e le uscite a destra. Le righe sono in ordine dall'alto verso il basso.
    ''' </summary>
    Private Function ComponiLeRighe() As List(Of RigaDiComandi)

        Dim disponibile As Integer = _fascia.ClientSize.Width -
                                     _fascia.Padding.Left - StileApp.MargineRiquadro

        Dim righe As New List(Of RigaDiComandi)

        For Each critico As Button In _critici
            Dim sua As New RigaDiComandi(StaccoDelCritico)
            sua.Aggiungi(critico, versoDestra:=True)
            righe.Add(sua)
        Next

        If Larghezza(_aSinistra) + StileApp.DistanzaControlli + Larghezza(_aDestra) <= disponibile Then
            Dim unica As New RigaDiComandi(StileApp.InterlineaMinima)
            unica.ASinistra.AddRange(_aSinistra)
            unica.ADestra.AddRange(_aDestra)
            righe.Add(unica)
            Return righe
        End If

        ' Insieme non ci stanno: ogni fila prende le righe che le servono, e quelle che
        ' portano altrove restano in fondo — il comando principale di un pannello si cerca
        ' in basso a destra, e deve restare dov'era.
        righe.AddRange(Spezzata(_aSinistra, disponibile, versoDestra:=False))
        righe.AddRange(Spezzata(_aDestra, disponibile, versoDestra:=True))

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

    ''' <summary>Posa le righe dal fondo della fascia verso l'alto.</summary>
    Private Sub PosaLeRighe(righe As List(Of RigaDiComandi))

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

    End Sub

    ''' <summary>
    ''' L'altezza che la fascia deve avere perché tutte le righe ci stiano: il margine del
    ''' riquadro sotto, l'interlinea minima sopra, e in mezzo le righe con i loro stacchi.
    ''' </summary>
    Private Shared Function AltezzaNecessaria(righe As List(Of RigaDiComandi)) As Integer

        Dim altezza As Integer = StileApp.MargineRiquadro + StileApp.InterlineaMinima

        For indice As Integer = 0 To righe.Count - 1
            altezza += StileApp.BottoneStandard.Height
            If indice < righe.Count - 1 Then altezza += righe(indice).StaccoSotto
        Next

        Return altezza

    End Function

    ''' <summary>Quanto spazio vuole una fila di bottoni messi in riga.</summary>
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
