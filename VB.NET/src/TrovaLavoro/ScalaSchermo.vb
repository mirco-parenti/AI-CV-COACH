''' <summary>
''' Le misure dell'interfaccia rispetto al DPI dello schermo (cap. 03.4, decisione 15.7).
'''
''' Le costanti di <see cref="StileApp"/>, le misure scritte nei Designer e le soglie
''' decise nel progetto sono in <b>unità di progetto</b>: pixel a 96 DPI, il DPI su cui il
''' disegno è stato validato a video. Quel che il programma legge invece a runtime —
''' <c>ClientSize</c>, <c>Width</c>, <c>Screen.WorkingArea</c> — è in <b>pixel dello
''' schermo</b>, e a 150% di scala vale una volta e mezza. Confrontare le due cose senza
''' convertire è l'errore che il collaudo dal vivo di T9e ha trovato in tre punti
''' (cap. 14, giro B): a 144 DPI la finestra massimizzata è larga 1920 pixel di schermo,
''' che sono 1280 unità di progetto, e chi confronta 1920 con una soglia di 1350 conclude
''' il contrario di chi confronta 1280.
'''
''' Qui il DPI si <b>passa</b>, non si legge: sono funzioni pure, e il banco può chiedere
''' loro cosa succederebbe a 144 DPI pur girando a 96 (regola di progetto 14). È il motivo
''' per cui le tre decisioni che sbagliavano vivono qui e non dentro un gestore di eventi.
''' </summary>
Public Module ScalaSchermo

    ''' <summary>
    ''' Il DPI a cui l'applicazione è disegnata: sotto questo valore le costanti di
    ''' progetto e i pixel dello schermo coincidono, e non c'è niente da convertire.
    ''' </summary>
    Public Const DpiDiProgetto As Integer = 96

    ''' <summary>
    ''' Da unità di progetto a pixel dello schermo: quanto spazio vero prende, qui e ora,
    ''' una misura scritta nel progetto. Un DPI non valido non converte niente, invece di
    ''' far saltare il conto.
    ''' </summary>
    Public Function InPixelDelloSchermo(unitaDiProgetto As Integer, dpi As Integer) As Integer

        If dpi <= 0 Then Return unitaDiProgetto

        Return CInt(Math.Round(unitaDiProgetto * (dpi / DpiDiProgetto)))

    End Function

    ''' <summary>
    ''' Da pixel dello schermo a unità di progetto: quanto vale, nel metro del progetto,
    ''' una misura letta a runtime. È il verso che serve per confrontare con una soglia.
    ''' </summary>
    Public Function InUnitaDiProgetto(pixelDelloSchermo As Integer, dpi As Integer) As Integer

        If dpi <= 0 Then Return pixelDelloSchermo

        Return CInt(Math.Round(pixelDelloSchermo * (DpiDiProgetto / dpi)))

    End Function

    ''' <summary>
    ''' Se il pannello logo deve passare in modalità compatta (cap. 03.5). La soglia è in
    ''' unità di progetto, la larghezza arriva dallo schermo: è il confronto che a DPI alti
    ''' rispondeva sempre «no», rendendo la compatta codice morto proprio dove lo spazio
    ''' manca di più.
    ''' </summary>
    Public Function ModalitaCompatta(larghezzaDelloSchermo As Integer, dpi As Integer,
                                     sogliaDiProgetto As Integer) As Boolean

        Return InUnitaDiProgetto(larghezzaDelloSchermo, dpi) < sogliaDiProgetto

    End Function

    ''' <summary>
    ''' L'altezza che una finestra può davvero prendersi: quella che vorrebbe, ma non oltre
    ''' lo spazio che c'è. Senza questo tetto una finestra dimensionata sul proprio contenuto
    ''' viene troncata dal sistema, e quel che resta fuori non è raggiungibile in nessun modo
    ''' (cap. 03.4). Uno spazio non noto (zero o negativo) lascia passare l'altezza voluta:
    ''' meglio una finestra grande che una finestra alta zero.
    ''' </summary>
    Public Function AltezzaSostenibile(altezzaVoluta As Integer, altezzaDisponibile As Integer) As Integer

        If altezzaDisponibile <= 0 Then Return altezzaVoluta

        Return Math.Min(altezzaVoluta, altezzaDisponibile)

    End Function

    ''' <summary>
    ''' Quanto spazio in altezza resta all'<i>area cliente</i> di una finestra: l'area di
    ''' lavoro dello schermo meno la cornice (titolo e bordi), che area cliente non è. È il
    ''' numero da dare a <see cref="AltezzaSostenibile"/>, perché una finestra confrontata
    ''' con lo schermo intero si dichiara più bassa di quanto poi diventa.
    ''' </summary>
    Public Function SpazioClienteDisponibile(altezzaAreaDiLavoro As Integer,
                                             altezzaCornice As Integer) As Integer

        Return Math.Max(0, altezzaAreaDiLavoro - altezzaCornice)

    End Function

    ''' <summary>
    ''' Se il contenuto va fatto scorrere, cioè se il tetto di <see cref="AltezzaSostenibile"/>
    ''' ha tagliato qualcosa. Le due cose vanno sempre insieme: il tetto da solo taglierebbe,
    ''' lo scorrimento da solo lascerebbe la finestra fuori schermo.
    ''' </summary>
    Public Function ServeScorrimento(altezzaVoluta As Integer, altezzaDisponibile As Integer) As Boolean

        If altezzaDisponibile <= 0 Then Return False

        Return altezzaVoluta > altezzaDisponibile

    End Function

End Module
