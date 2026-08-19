Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

Namespace Motore

    ''' <summary>
    ''' Un filo di esecuzione STA con la sua pompa di messaggi, acceso per il tempo di un
    ''' lavoro e spento quando quello finisce. È l'ambiente che WebView2 pretende per
    ''' accendersi — e che l'applicazione con le finestre ha per natura, mentre la modalità
    ''' <c>--mcp</c> non ha affatto (cap. 09.2).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché esiste</b> <i>(2026-08-19)</i>. Fino a qui il PDF era «roba
    ''' dell'applicazione»: il capitolo 09 diceva che stamparlo vuole una finestra, e che in
    ''' <c>--mcp</c> di finestre non ne nasce nessuna. La frase era imprecisa, e l'ha
    ''' smentita il banco di collaudo, che stampa PDF veri da un processo di test dove
    ''' finestre non ce ne sono: quel che serve non è una finestra visibile ma un thread
    ''' <b>STA</b> con una pompa di messaggi, perché è lì che il motore del browser consegna
    ''' i suoi eventi. Senza pompa, l'attesa dell'accensione non si sveglierebbe mai.</para>
    ''' <para><b>Uno per lavoro, e non uno per sempre.</b> Nell'applicazione il motore del
    ''' browser resta acceso per tutta la sessione, perché chi guarda apre e chiude i
    ''' pannelli di continuo e riaccenderlo ogni volta si vedrebbe. Qui è l'opposto: un
    ''' server MCP passa il tempo fermo, e tenere in vita un thread grafico per una stampa
    ''' che forse non arriverà mai è peso senza guadagno. Chi paga il prezzo è la seconda
    ''' esportazione di fila, che riaccende tutto — ed è un prezzo in secondi, su
    ''' un'operazione che di suo dura secondi.</para>
    ''' <para><b>Nato nel banco, portato qui</b>: fino al 2026-08-19 questo codice viveva in
    ''' <c>ThreadInterfaccia</c>, fra i collaudi, dichiarato «attrezzo del banco, non del
    ''' prodotto». Da quando anche il prodotto ne ha bisogno, la copia è una sola e il banco
    ''' usa questa — per la stessa ragione per cui a T5a le due pompe dei collaudi erano
    ''' diventate una: una pompa copiata in due file è una pompa che prima o poi diverge.</para>
    ''' </remarks>
    Public Module FiloGrafico

        ''' <summary>
        ''' Accende il filo, gli fa fare il lavoro e aspetta che la pompa si sia davvero
        ''' fermata prima di restituire: quando questo compito finisce, tutto ciò che il
        ''' lavoro teneva aperto è già stato smesso.
        ''' </summary>
        ''' <param name="lavoro">
        ''' Il lavoro da fare sul filo. Tutto ciò che tocca il motore del browser va creato
        ''' <b>qui dentro</b> e lasciato qui: niente di quel mondo è pensato per essere
        ''' usato da più fili (v. <c>MotoreBrowser</c>).
        ''' </param>
        ''' <remarks>
        ''' Un'eccezione del lavoro non si perde: torna a chi ha chiamato, come se il lavoro
        ''' fosse stato fatto qui.
        ''' </remarks>
        Public Function EseguiAsync(lavoro As Func(Of Task)) As Task

            If lavoro Is Nothing Then Throw New ArgumentNullException(NameOf(lavoro))

            Dim esito As New TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)

            Dim filo As New Thread(
                Sub()

                    Dim scoppio As Exception = Nothing

                    Try
                        SynchronizationContext.SetSynchronizationContext(
                            New WindowsFormsSynchronizationContext())

                        Dim compito As Task = lavoro()

                        If compito Is Nothing Then
                            Throw New InvalidOperationException(
                                "Il lavoro da fare sul filo grafico non ha restituito nessun compito.")
                        End If

                        ' Quando il lavoro è finito si ferma la pompa: senza, Application.Run
                        ' resterebbe in attesa per sempre.
                        compito.ContinueWith(
                            Sub(finito)
                                scoppio = finito.Exception?.InnerExceptions.FirstOrDefault()
                                Application.ExitThread()
                            End Sub,
                            TaskScheduler.FromCurrentSynchronizationContext())

                        Application.Run()

                    Catch ex As Exception
                        scoppio = ex
                    End Try

                    ' Si risponde a chi aspetta **dopo** che la pompa si è fermata, non
                    ' dentro la continuazione: altrimenti il chiamante ripartirebbe mentre
                    ' qui si sta ancora smaltendo, e il motore del browser chiude i suoi
                    ' processi con calma (v. i collaudi della stampa, che per questo
                    ' cancellano la cartella con pazienza).
                    If scoppio Is Nothing Then
                        esito.SetResult()
                    Else
                        esito.SetException(scoppio)
                    End If

                End Sub) With {.Name = "FiloGrafico", .IsBackground = True}

            filo.SetApartmentState(ApartmentState.STA)
            filo.Start()

            Return esito.Task

        End Function

        ''' <summary>
        ''' La stessa cosa per chi non ha un contesto asincrono in cui aspettare — il banco
        ''' di collaudo, che di suo gira su fili qualunque.
        ''' </summary>
        Public Sub Esegui(lavoro As Func(Of Task))

            EseguiAsync(lavoro).GetAwaiter().GetResult()

        End Sub

    End Module

End Namespace
