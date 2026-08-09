Imports System.Linq

Namespace Motore

    ''' <summary>Un'eco: le parole dell'utente da rimettere in bocca, e dove.</summary>
    Public Class EcoMossa

        ''' <summary>Le parole dell'utente, così come le aveva dette.</summary>
        Public Property Testo As String = ""

        ''' <summary>Dopo quante bolle di <see cref="Mossa.Detto"/> va mostrata.</summary>
        Public Property DopoDetti As Integer

    End Class

    ''' <summary>Cosa il dialogo si aspetta adesso dall'utente.</summary>
    Public Enum TipoMossa
        ''' <summary>Una risposta scritta: la casella di testo e il bottone «Invia».</summary>
        ChiediRisposta
        ''' <summary>Una scelta fra i bottoni proposti.</summary>
        ChiediScelta
        ''' <summary>Niente: il dialogo è concluso e il profilo è pronto.</summary>
        Fine
    End Enum

    ''' <summary>Una riga di una scheda: «Ruolo: Magazziniere», oppure una voce nuda di elenco.</summary>
    Public Class RigaScheda

        ''' <summary>L'etichetta del campo; vuota nelle schede a elenco (le competenze).</summary>
        Public Property Etichetta As String = ""

        ''' <summary>Il valore da mostrare, già pronto da leggere.</summary>
        Public Property Valore As String = ""

    End Class

    ''' <summary>
    ''' Una scheda di conferma: è il «ho capito questo: giusto?» del cap. 12, il punto in
    ''' cui l'utente vede cosa sta per entrare nel profilo prima che ci entri.
    ''' </summary>
    Public Class Scheda

        ''' <summary>Il titolo, quando serve distinguere più schede; può mancare.</summary>
        Public Property Titolo As String

        Public Property Righe As New List(Of RigaScheda)

    End Class

    ''' <summary>Un bottone offerto all'utente.</summary>
    Public Class Scelta

        ''' <summary>L'identificativo da restituire alla macchina (v. <see cref="Scelte"/>).</summary>
        Public Property Id As String

        ''' <summary>Il testo del bottone.</summary>
        Public Property Etichetta As String

        ''' <summary>Se è l'azione principale, quella che l'utente sceglierà di solito.</summary>
        Public Property Principale As Boolean

    End Class

    ''' <summary>Gli identificativi delle scelte che il dialogo può proporre.</summary>
    Public Module Scelte
        Public Const Conferma As String = "conferma"
        Public Const Correggi As String = "correggi"
        Public Const Riprova As String = "riprova"
        Public Const Oltre As String = "oltre"
        Public Const Altra As String = "altra"
        Public Const Procedi As String = "procedi"
        Public Const Aggiungi As String = "aggiungi"
        Public Const Scarta As String = "scarta"
    End Module

    ''' <summary>
    ''' Una mossa del dialogo: <b>cosa mostrare</b> e <b>cosa aspettarsi</b>. È tutto ciò
    ''' che passa fra la macchina del dialogo (<see cref="DialogoProfilo"/>) e chi la
    ''' mostra.
    ''' </summary>
    ''' <remarks>
    ''' Nel prototipo il flusso era fatto di funzioni che disegnavano da sé bolle e
    ''' bottoni nella pagina: la logica del dialogo e il suo aspetto erano la stessa
    ''' cosa. Qui in mezzo c'è questo oggetto, e la conseguenza è doppia — il pannello P5
    ''' si limita a disegnare ciò che riceve, e la macchina si collauda tutta intera
    ''' senza interfaccia, guidandola da un collaudo (cap. 14, T3).
    ''' </remarks>
    Public Class Mossa

        ''' <summary>Il valore mostrato al posto di un campo che l'utente non ha specificato.</summary>
        Public Const NonSpecificato As String = "(non specificata)"

        Public Property Tipo As TipoMossa

        ''' <summary>Ciò che l'assistente dice: una voce per ogni bolla.</summary>
        Public Property Detto As New List(Of String)

        ''' <summary>
        ''' Le parole rimesse in bocca all'utente (anti-perdita), <b>ancorate al punto
        ''' della conversazione</b> in cui vanno dette: ogni eco sa dopo quale bolla
        ''' dell'assistente comparire. Senza l'ancora, l'eco finiva dopo tutto il detto
        ''' — e quando il ripescaggio falliva l'utente leggeva «lo lascio fuori: X»
        ''' <i>prima</i> di rivedere le proprie parole; e nella passata finale con più
        ''' categorie un campo singolo ne conservava una sola.
        ''' </summary>
        Public Property Echi As New List(Of EcoMossa)

        ''' <summary>
        ''' Aggiunge un'eco ancorata a questo punto: comparirà dopo le bolle di
        ''' <see cref="Detto"/> già presenti adesso.
        ''' </summary>
        Public Sub AggiungiEco(testo As String)
            Echi.Add(New EcoMossa With {.Testo = testo, .DopoDetti = Detto.Count})
        End Sub

        ''' <summary>
        ''' Tutte le parole ripescate della mossa, in una riga: è la vista d'insieme che
        ''' usano i collaudi e i rapporti; per disegnare vale <see cref="Echi"/>.
        ''' </summary>
        Public ReadOnly Property EcoUtente As String
            Get
                If Echi.Count = 0 Then Return Nothing
                Return String.Join(" / ", Echi.Select(Function(e) e.Testo))
            End Get
        End Property

        ''' <summary>Le schede da mostrare sotto il testo.</summary>
        Public Property Schede As New List(Of Scheda)

        ''' <summary>I bottoni, quando il tipo è <see cref="TipoMossa.ChiediScelta"/>.</summary>
        Public Property Scelte As New List(Of Scelta)

        ''' <summary>Il testo grigio della casella, quando si aspetta una risposta scritta.</summary>
        Public Property SuggerimentoCasella As String

    End Class

End Namespace
