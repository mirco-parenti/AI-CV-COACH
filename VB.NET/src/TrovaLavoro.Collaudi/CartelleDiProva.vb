Imports System.IO
Imports System.Threading

''' <summary>
''' Le cartelle usa-e-getta del banco: come si cancellano quando qualcuno le tiene
''' ancora per un attimo.
''' </summary>
''' <remarks>
''' Nato dentro <c>CollaudiStampaPdf</c>, portato qui il 2026-08-19: da quando anche
''' <c>esporta_documento</c> può accendere il motore del browser (v. <c>FiloGrafico</c>),
''' i collaudi che lasciano dietro di sé un <c>lockfile</c> vivo sono due — e una pulizia
''' copiata in due file è una pulizia che prima o poi diverge, come fu per la pompa di
''' messaggi.
''' </remarks>
Friend Module CartelleDiProva

    ''' <summary>
    ''' Cancella la cartella, con pazienza. Il motore del browser chiude i suoi processi
    ''' <b>dopo</b> che il controllo è stato smesso, e finché non ha finito tiene il
    ''' proprio <c>lockfile</c>: cancellare al primo colpo fallisce, e l'errore coprirebbe
    ''' l'esito vero del collaudo. Se dopo qualche tentativo la cartella resiste ancora si
    ''' lascia dov'è — è la cartella temporanea di Windows, e un collaudo non deve fallire
    ''' per le pulizie.
    ''' </summary>
    Friend Sub PortaVia(cartella As String)

        For tentativo As Integer = 1 To 10
            Try
                Directory.Delete(cartella, recursive:=True)
                Return
            Catch ex As IOException
                Thread.Sleep(300)
            Catch ex As UnauthorizedAccessException
                Thread.Sleep(300)
            End Try
        Next

    End Sub

End Module
