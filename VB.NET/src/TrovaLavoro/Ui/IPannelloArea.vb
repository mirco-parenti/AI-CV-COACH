Imports System.Drawing

''' <summary>
''' Quel che la finestra principale chiede a ogni pannello dell'area centrale
''' (P1–P7, cap. 03.4).
''' </summary>
''' <remarks>
''' Nasce da un vincolo geometrico che il capitolo 03 lascia implicito: il pannello del
''' logo è <b>flottante</b> sopra la struttura (cap. 03.5) e la fascia di stato è alta
''' molto meno di lui, quindi il logo copre l'angolo in basso a sinistra dell'area
''' centrale — 188 px in modalità piena, 68 in compatta. Nessun contenuto vivo può
''' finire là sotto. Invece di ripetere il conto in ogni pannello, è la finestra a
''' dichiarare il proprio ingombro, e ogni pannello si dispone di conseguenza: la fascia
''' delle azioni, in basso, cresce fino a coprire quell'altezza e comincia a destra del
''' logo. È il posto giusto, perché lì stanno i bottoni e non i dati.
''' </remarks>
Public Interface IPannelloArea

    ''' <summary>
    ''' Dichiara quanto spazio si prende il pannello logo sopra l'area centrale.
    ''' </summary>
    ''' <param name="ingombro">
    ''' Larghezza del pannello logo e altezza della sua parte che sfonda nell'area
    ''' centrale. Cambia quando la finestra passa fra modalità piena e compatta.
    ''' </param>
    Sub ImpostaIngombroLogo(ingombro As Size)

End Interface
