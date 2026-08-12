Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Windows.Forms

''' <summary>
''' Fa girare un lavoro asincrono su un thread STA con la sua pompa di messaggi: è
''' l'ambiente in cui WebView2 vive dentro l'applicazione, ricostruito qui perché il
''' banco di collaudo non ne ha uno. Le eccezioni tornano a chi chiama.
''' </summary>
''' <remarks>
''' Nato dentro <c>CollaudiStampaPdf</c> a T4b, portato qui a T5a: da quando il motore del
''' browser è uno solo e condiviso, i collaudi che ne hanno bisogno sono due, e una pompa
''' di messaggi copiata in due file è una pompa che prima o poi diverge. È un attrezzo del
''' banco, non del prodotto.
''' </remarks>
Friend Module ThreadInterfaccia

    ''' <summary>Esegue il lavoro e aspetta che finisca, come farebbe l'applicazione.</summary>
    Friend Sub Esegui(lavoro As Func(Of Task))

        Dim scoppio As Exception = Nothing

        Dim thread As New Thread(
            Sub()
                SynchronizationContext.SetSynchronizationContext(
                    New WindowsFormsSynchronizationContext())

                Dim compito As Task = lavoro()

                ' Quando il lavoro è finito si ferma la pompa: senza, Application.Run
                ' resterebbe in attesa per sempre.
                compito.ContinueWith(
                    Sub(finito)
                        scoppio = finito.Exception?.InnerExceptions.FirstOrDefault()
                        Application.ExitThread()
                    End Sub,
                    TaskScheduler.FromCurrentSynchronizationContext())

                Application.Run()
            End Sub)

        thread.SetApartmentState(ApartmentState.STA)
        thread.Start()
        thread.Join()

        If scoppio IsNot Nothing Then Throw scoppio

    End Sub

End Module
