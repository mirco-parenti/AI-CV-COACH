Namespace Motore

    ''' <summary>
    ''' Le due lingue in cui l'applicazione sa scrivere i documenti, e la regola che porta
    ''' dentro una di esse qualunque annuncio (cap. 10.1, cap. 10.5).
    ''' </summary>
    ''' <remarks>
    ''' <para>Sta in un posto solo perché la stessa risposta serve in quattro punti che non
    ''' si parlano fra loro: quale variante di prompt caricare (cap. 04.6), con quali
    ''' etichette impaginare (<c>Documenti.Impaginazione</c>), che sigla scrivere nel nome
    ''' del file (cap. 05.6) e cosa proporre nella tendina di P6. Se la regola vivesse in
    ''' quattro copie, il giorno del francese ne resterebbe indietro una.</para>
    ''' <para><b>Una lingua non dichiarata è italiano, non inglese.</b> Le candidature nate
    ''' prima di T7 il campo <c>lingua</c> non ce l'hanno, e nessuna di esse era in inglese:
    ''' far ricadere il vuoto sull'inglese le riscriverebbe tutte all'indietro, che è
    ''' esattamente ciò che T5c ha deciso di non fare con lo stato (cap. 11.1).</para>
    ''' </remarks>
    Public Class LinguaDocumenti

        ''' <summary>La lingua di casa: quella del profilo, dell'interfaccia e del vuoto.</summary>
        Public Const Italiano As String = "it"

        ''' <summary>L'altra lingua che il pool sa scrivere (cap. 04.3).</summary>
        Public Const Inglese As String = "en"

        ''' <summary>
        ''' La lingua in cui si scriveranno i documenti, a partire da quella che l'analisi
        ''' ha riconosciuto nell'annuncio.
        ''' </summary>
        ''' <remarks>
        ''' Un annuncio in una terza lingua non ferma niente: si propone l'inglese, che è
        ''' la lingua franca delle candidature, e lo si dichiara (cap. 10.2). Proporre
        ''' l'italiano a chi ha davanti un annuncio in tedesco sarebbe stato peggio, e
        ''' rifiutarsi di generare peggio ancora.
        ''' </remarks>
        ''' <param name="rilevata">Il campo <c>lingua</c> dell'annuncio; può essere vuoto.</param>
        Public Shared Function PerDocumenti(rilevata As String) As String

            If String.IsNullOrWhiteSpace(rilevata) Then Return Italiano

            Dim pulita As String = rilevata.Trim()

            If pulita.Equals(Italiano, StringComparison.OrdinalIgnoreCase) Then Return Italiano

            Return Inglese

        End Function

        ''' <summary>
        ''' Vero quando l'annuncio non è in nessuna delle due lingue che sappiamo scrivere:
        ''' è il caso in cui l'inglese è un ripiego e P6 deve dirlo, invece di far sembrare
        ''' una scelta quello che è un adattamento.
        ''' </summary>
        Public Shared Function EStraniera(rilevata As String) As Boolean

            If String.IsNullOrWhiteSpace(rilevata) Then Return False

            Dim pulita As String = rilevata.Trim()

            Return Not pulita.Equals(Italiano, StringComparison.OrdinalIgnoreCase) AndAlso
                   Not pulita.Equals(Inglese, StringComparison.OrdinalIgnoreCase)

        End Function

        ''' <summary>
        ''' Come si chiama una lingua quando la si mostra all'utente, che legge in
        ''' italiano (cap. 10.1: l'interfaccia resta in una lingua sola).
        ''' </summary>
        Public Shared Function Nome(lingua As String) As String

            If PerDocumenti(lingua) = Italiano Then Return "Italiano"

            Return "Inglese"

        End Function

    End Class

End Namespace
