Namespace Dati

    ''' <summary>
    ''' Com'è finita una candidatura (cap. 07.3): è ciò che vive <b>dentro</b> lo stato
    ''' <see cref="StatoOpportunita.Esito"/>, l'unico del ciclo di vita che da solo non
    ''' direbbe niente — «con esito» senza dire quale è una risposta a metà.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>«In attesa» non è qui, ed è una decisione</b> (presa con Mirco il
    ''' 2026-08-21). Il cap. 07.3 elencava quattro valori, ma «spedita e nessuna risposta»
    ''' è già lo stato <see cref="StatoOpportunita.Inviata"/>, ed è già un contatore della
    ''' Home: registrarla come esito avrebbe creato due modi di dire la stessa cosa, e il
    ''' promemoria di follow-up avrebbe dovuto rincorrere le candidature in due stati
    ''' invece che in uno. Qui si registra solo ciò che <b>è successo</b>; l'attesa si
    ''' deduce dal silenzio.</para>
    ''' <para><b>È una dichiarazione dell'utente, non un fatto osservato</b>, come lo è
    ''' «l'ho spedita» (cap. 07.3): il programma non legge la posta di nessuno. Per questo
    ''' si corregge — chi segna «rifiutata» per sbaglio deve poter tornare indietro senza
    ''' rifare la candidatura (v. <see cref="Motore.Opportunita.SegnaEsito"/>).</para>
    ''' </remarks>
    Public Enum EsitoCandidatura

        ''' <summary>Hanno richiamato: c'è stato un colloquio, o è fissato.</summary>
        Colloquio

        ''' <summary>Hanno detto di no.</summary>
        Rifiutata

        ''' <summary>Il posto è suo.</summary>
        Assunto

    End Enum

    ''' <summary>
    ''' Come un <see cref="EsitoCandidatura"/> si scrive su disco, come si mostra e come
    ''' si accorda con lo stato che lo contiene.
    ''' </summary>
    Public NotInheritable Class EsitiCandidatura

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Come l'esito si scrive nei file: minuscolo e senza accenti, come ogni altro
        ''' valore di schema del progetto.
        ''' </summary>
        Public Shared Function Nome(esito As EsitoCandidatura) As String

            Select Case esito
                Case EsitoCandidatura.Colloquio : Return "colloquio"
                Case EsitoCandidatura.Rifiutata : Return "rifiutata"
                Case EsitoCandidatura.Assunto : Return "assunto"
                Case Else : Throw New ArgumentOutOfRangeException(NameOf(esito))
            End Select

        End Function

        ''' <summary>
        ''' L'esito scritto su disco; <c>Nothing</c> se non c'è o se quel nome non lo
        ''' conosciamo — un valore che non riconosciamo vale come «non registrato», che è
        ''' il modo in cui il resto del programma tratta i file dell'utente: non si
        ''' indovina.
        ''' </summary>
        ''' <param name="scritto">
        ''' Il nome letto dal file. Non può chiamarsi «esito»: in VB le maiuscole non
        ''' distinguono, e coprirebbe il tipo <see cref="EsitoCandidatura"/> — è la stessa
        ''' trappola che <see cref="StatiOpportunita.DaNome"/> documenta.
        ''' </param>
        Public Shared Function DaNome(scritto As String) As EsitoCandidatura?

            If String.IsNullOrWhiteSpace(scritto) Then Return Nothing

            Dim cercato As String = scritto.Trim().ToLowerInvariant()

            For Each valore As EsitoCandidatura In [Enum].GetValues(Of EsitoCandidatura)()
                If Nome(valore) = cercato Then Return valore
            Next

            Return Nothing

        End Function

        ''' <summary>Come l'esito si mostra all'utente: in italiano, con l'iniziale grande.</summary>
        ''' <remarks>
        ''' L'assunzione porta il suo 🎉, che il cap. 07.3 le mette accanto fin dal
        ''' disegno: è l'unica riga di questo programma che vale la pena festeggiare.
        ''' </remarks>
        Public Shared Function Etichetta(esito As EsitoCandidatura) As String

            Select Case esito
                Case EsitoCandidatura.Colloquio : Return "Colloquio"
                Case EsitoCandidatura.Rifiutata : Return "Rifiutata"
                Case EsitoCandidatura.Assunto : Return "Assunto 🎉"
                Case Else : Throw New ArgumentOutOfRangeException(NameOf(esito))
            End Select

        End Function

        ''' <summary>
        ''' A che punto è una candidatura, in una parola sola: l'esito quando c'è, lo
        ''' stato quando non c'è.
        ''' </summary>
        ''' <remarks>
        ''' Chi guarda non pensa per stati: pensa «rifiutata», non «con esito». È il testo
        ''' che va nella colonna «Stato» della Home e nella scheda di P4, e sta qui invece
        ''' che nei due pannelli perché una candidatura assunta non può chiamarsi in un
        ''' modo nell'elenco e in un altro nella sua scheda.
        ''' </remarks>
        Public Shared Function EtichettaDi(stato As StatoOpportunita, esito As EsitoCandidatura?) As String

            If stato = StatoOpportunita.Esito AndAlso esito.HasValue Then Return Etichetta(esito.Value)

            Return StatiOpportunita.Etichetta(stato)

        End Function

        ''' <summary>
        ''' Lo stato e l'esito letti da un file, messi d'accordo fra loro.
        ''' </summary>
        ''' <remarks>
        ''' <para>I due campi si scrivono insieme e vanno letti insieme, perché da soli
        ''' possono contraddirsi: un file corretto a mano — cosa che il cap. 11.1 invita
        ''' a fare — può dichiarare uno stato «esito» senza dire quale, o lasciare un
        ''' esito appeso a una candidatura che nel frattempo è stata scartata.</para>
        ''' <para>Si sceglie in un modo solo, e conservativo. <b>Stato «esito» senza
        ''' esito</b>: si torna a <see cref="StatoOpportunita.Inviata"/>, che è l'unico
        ''' punto da cui a quello stato si arriva — meglio una candidatura che aspetta
        ''' ancora che una finita in un modo che nessuno ha scritto. <b>Esito su un altro
        ''' stato</b>: l'esito si lascia cadere, perché lo stato è il campo che tutto il
        ''' programma guarda, ed è quello che l'utente vede nella Home.</para>
        ''' <para>Sta qui, e non nei due archivi, per la stessa ragione per cui il blocco
        ''' delle date sta in <see cref="StatiOpportunita.DateComeJson"/>: quei due campi
        ''' li leggono in due — lo <c>stato.json</c> di ogni cartella e il
        ''' <c>registro.json</c> — e la regola dev'essere una sola.</para>
        ''' </remarks>
        Public Shared Sub Concorda(ByRef stato As StatoOpportunita, ByRef esito As EsitoCandidatura?)

            If stato = StatoOpportunita.Esito Then
                If Not esito.HasValue Then stato = StatoOpportunita.Inviata
                Return
            End If

            esito = Nothing

        End Sub

    End Class

End Namespace
