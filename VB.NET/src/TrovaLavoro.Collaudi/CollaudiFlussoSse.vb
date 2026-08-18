Imports System.IO
Imports System.Text
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi del lettore di eventi SSE (cap. 02.5, T7c). Girano su <b>stringhe</b>,
    ''' senza rete e senza HTTP: è il motivo per cui la grammatica del formato vive in
    ''' una classe che non sa niente di Claude. Qui si verifica solo che gli eventi
    ''' vengano spezzati dove vanno spezzati — cosa significhino è affare del client.
    ''' </summary>
    <TestClass>
    Public Class CollaudiFlussoSse

        ''' <summary>Un flusso di byte da un testo, come lo consegnerebbe la rete.</summary>
        Private Shared Function Flusso(testo As String) As MemoryStream
            Return New MemoryStream(Encoding.UTF8.GetBytes(testo))
        End Function

        <TestMethod>
        Public Async Function UnEventoSiLeggeConIlSuoNomeEISuoiDati() As Task
            Using sorgente As MemoryStream = Flusso("event: ping" & vbLf & "data: {}" & vbLf & vbLf)
                Using lettore As New LettoreSse(sorgente)

                    Dim evento As EventoSse = Await lettore.ProssimoAsync()

                    Assert.AreEqual("ping", evento.Nome, "il nome")
                    Assert.AreEqual("{}", evento.Dati, "i dati")

                End Using
            End Using
        End Function

        <TestMethod>
        Public Async Function GliEventiEscanoUnoAllaVoltaENellOrdine() As Task
            Dim corpo As String =
                "event: uno" & vbLf & "data: primo" & vbLf & vbLf &
                "event: due" & vbLf & "data: secondo" & vbLf & vbLf &
                "event: tre" & vbLf & "data: terzo" & vbLf & vbLf

            Using sorgente As MemoryStream = Flusso(corpo)
                Using lettore As New LettoreSse(sorgente)

                    Assert.AreEqual("primo", (Await lettore.ProssimoAsync()).Dati, "primo")
                    Assert.AreEqual("secondo", (Await lettore.ProssimoAsync()).Dati, "secondo")
                    Assert.AreEqual("terzo", (Await lettore.ProssimoAsync()).Dati, "terzo")
                    Assert.IsNull(Await lettore.ProssimoAsync(), "e poi il flusso è finito")

                End Using
            End Using
        End Function

        <TestMethod>
        Public Async Function PiuRigheDiDatiSonoUnTestoSolo() As Task
            ' La specifica le rimette insieme con gli a capo che le separavano: un JSON
            ' spezzato su due righe deve tornare valido, non arrivare a pezzi.
            Using sorgente As MemoryStream = Flusso(
                "event: message" & vbLf & "data: {""a"":1," & vbLf & "data: ""b"":2}" & vbLf & vbLf)

                Using lettore As New LettoreSse(sorgente)
                    Assert.AreEqual("{""a"":1," & vbLf & """b"":2}", (Await lettore.ProssimoAsync()).Dati)
                End Using
            End Using
        End Function

        <TestMethod>
        Public Async Function ICommentiTengonoVivaLaConnessioneENonSonoEventi() As Task
            ' Una riga che comincia con i due punti è un commento: serve a non far
            ' addormentare la connessione. Se passasse per un campo, il flusso si
            ' riempirebbe di eventi che nessuno ha mandato.
            Using sorgente As MemoryStream = Flusso(
                ": mi sto scaldando" & vbLf & vbLf &
                "event: vero" & vbLf & "data: eccomi" & vbLf & vbLf)

                Using lettore As New LettoreSse(sorgente)
                    Dim evento As EventoSse = Await lettore.ProssimoAsync()
                    Assert.AreEqual("vero", evento.Nome, "il primo evento vero")
                    Assert.AreEqual("eccomi", evento.Dati, "i suoi dati")
                End Using
            End Using
        End Function

        <TestMethod>
        Public Async Function UnEventoSenzaLaRigaVuotaNonSiConsegna() As Task
            ' Il flusso si è interrotto prima che l'evento fosse chiuso: consegnarlo
            ' monco vorrebbe dire dare per buono un JSON tagliato a metà.
            Using sorgente As MemoryStream = Flusso("event: monco" & vbLf & "data: {""a"":")
                Using lettore As New LettoreSse(sorgente)
                    Assert.IsNull(Await lettore.ProssimoAsync(), "un evento a metà non esiste")
                End Using
            End Using
        End Function

        <TestMethod>
        Public Async Function UnNomeSenzaDatiNonFaUnEvento() As Task
            Using sorgente As MemoryStream = Flusso(
                "event: solo-nome" & vbLf & vbLf &
                "event: completo" & vbLf & "data: ci sono" & vbLf & vbLf)

                Using lettore As New LettoreSse(sorgente)
                    Dim evento As EventoSse = Await lettore.ProssimoAsync()
                    Assert.AreEqual("completo", evento.Nome, "il nome solitario si scarta")
                End Using
            End Using
        End Function

        <TestMethod>
        Public Async Function UnSoloSpazioDopoIDuePuntiEFormaNonDato() As Task
            ' «data:  due spazi» ne perde uno solo: il secondo è testo dell'utente.
            Using sorgente As MemoryStream = Flusso("data:  spazio" & vbLf & vbLf)
                Using lettore As New LettoreSse(sorgente)
                    Assert.AreEqual(" spazio", (Await lettore.ProssimoAsync()).Dati)
                End Using
            End Using
        End Function

        <TestMethod>
        Public Async Function UnEventoSenzaNomeArrivaLoStesso() As Task
            ' Il campo «event:» è facoltativo: senza, resta il nome vuoto.
            Using sorgente As MemoryStream = Flusso("data: nudo" & vbLf & vbLf)
                Using lettore As New LettoreSse(sorgente)
                    Dim evento As EventoSse = Await lettore.ProssimoAsync()
                    Assert.AreEqual("", evento.Nome, "nessun nome")
                    Assert.AreEqual("nudo", evento.Dati, "ma i dati ci sono")
                End Using
            End Using
        End Function

        <TestMethod>
        Public Async Function LeRigheFinisconoComeVuoleLaRete() As Task
            ' Sul filo gli a capo sono CRLF: se il ritorno a carrello restasse attaccato
            ' ai dati, il JSON non si aprirebbe più.
            Using sorgente As MemoryStream = Flusso(
                "event: message" & vbCrLf & "data: {""a"":1}" & vbCrLf & vbCrLf)

                Using lettore As New LettoreSse(sorgente)
                    Assert.AreEqual("{""a"":1}", (Await lettore.ProssimoAsync()).Dati)
                End Using
            End Using
        End Function

        <TestMethod>
        Public Async Function IlLettoreNonChiudeIlFlussoDiChiGliELoHaDato() As Task
            ' Il flusso appartiene alla risposta HTTP che lo contiene: chiuderlo qui
            ' vorrebbe dire smaltire due volte la stessa cosa.
            Dim sorgente As MemoryStream = Flusso("data: uno" & vbLf & vbLf)
            Try
                Using lettore As New LettoreSse(sorgente)
                    Await lettore.ProssimoAsync()
                End Using

                Assert.IsTrue(sorgente.CanRead, "il flusso deve essere ancora aperto")
            Finally
                sorgente.Dispose()
            End Try
        End Function

    End Class

End Namespace
