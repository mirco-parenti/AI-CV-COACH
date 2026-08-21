Imports Microsoft.Web.WebView2.Core

Namespace Web

    ''' <summary>
    ''' Traduce l'esito di una navigazione del browser integrato in una frase per
    ''' l'utente (cap. 06.2): <c>Nothing</c> quando non c'è niente da dire.
    ''' </summary>
    ''' <remarks>
    ''' <para>Sta qui e non nel pannello per la stessa ragione di
    ''' <see cref="LettorePagina"/>: è una <b>decisione</b>, e le decisioni il banco le
    ''' deve poter provare senza pretendere WebView2 e un thread STA. Per questo prende
    ''' tre valori semplici invece dell'oggetto dell'evento, che nei collaudi non si
    ''' costruisce.</para>
    ''' <para>I casi sono <b>tre</b>, e la ragione di fermarsi a tre è che di più non
    ''' cambierebbero quel che l'utente fa dopo: o guarda l'indirizzo, o guarda il
    ''' collegamento, o riprova. Il motivo tecnico viaggia fra parentesi per chi lo sa
    ''' leggere, ma non è lui a dare il titolo alla frase.</para>
    ''' <para>Fino a T9d la frase era una sola — «controlla il collegamento a Internet» —
    ''' e si diceva a ogni fallimento: chi apriva un indirizzo che il server non ha veniva
    ''' mandato a controllare il modem, mentre il collegamento c'era e il server aveva
    ''' risposto benissimo. Il browser sapeva già distinguere i casi; non glielo chiedeva
    ''' nessuno.</para>
    ''' </remarks>
    Public Module EsitoNavigazione

        ''' <summary>Sotto questo numero la risposta del server non è un rifiuto.</summary>
        Private Const PrimoStatoDErrore As Integer = 400

        ''' <summary>
        ''' Perché la pagina non si è aperta, detto all'utente; <c>Nothing</c> se si è
        ''' aperta, o se è stato lui a fermarla.
        ''' </summary>
        ''' <param name="riuscita">L'<c>IsSuccess</c> dell'evento di fine navigazione.</param>
        ''' <param name="errore">Il <c>WebErrorStatus</c> dello stesso evento.</param>
        ''' <param name="statoHttp">
        ''' Lo stato HTTP della risposta, <c>0</c> quando non c'è stata risposta o quando
        ''' la pagina non viene dalla rete — la pagina di casa, per esempio, è scritta da
        ''' noi e non ha nessuno stato da mostrare.
        ''' </param>
        Public Function PercheNonSiEAperta(riuscita As Boolean,
                                           errore As CoreWebView2WebErrorStatus,
                                           statoHttp As Integer) As String

            ' Fermare una pagina è una scelta, non un guaio: chi preme «✕» sa già cos'è
            ' successo, e dirglielo sarebbe rimproverarlo.
            If errore = CoreWebView2WebErrorStatus.OperationCanceled Then Return Nothing

            If riuscita Then

                ' La navigazione è andata a buon fine e il server ha risposto: se la sua
                ' risposta è un rifiuto, la pagina che si vede è quella d'errore del sito,
                ' e la fascia dice la stessa cosa che c'è scritta lì.
                If statoHttp >= PrimoStatoDErrore Then
                    Return $"Il server ha risposto «{statoHttp}»: questa pagina non c'è, " &
                           "o non è accessibile. Controlla l'indirizzo."
                End If

                Return Nothing

            End If

            Select Case errore

                Case CoreWebView2WebErrorStatus.HostNameNotResolved
                    Return "Questo indirizzo non esiste: controlla come l'hai scritto."

                Case CoreWebView2WebErrorStatus.CannotConnect,
                     CoreWebView2WebErrorStatus.ConnectionAborted,
                     CoreWebView2WebErrorStatus.ConnectionReset,
                     CoreWebView2WebErrorStatus.Disconnected,
                     CoreWebView2WebErrorStatus.ServerUnreachable,
                     CoreWebView2WebErrorStatus.Timeout
                    Return "Non sono riuscita a raggiungere il server. Controlla il " &
                           "collegamento a Internet, o riprova con «⟳»."

                Case Else
                    Return $"La pagina non si è caricata ({errore}). Riprova con «⟳»."

            End Select

        End Function

    End Module

End Namespace
