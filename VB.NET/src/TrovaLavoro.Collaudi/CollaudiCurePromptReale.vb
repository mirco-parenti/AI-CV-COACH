Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace NonRegressione

    ''' <summary>
    ''' Le tre cure ai prompt del quinto tempo di T9e (Pool 1.13), provate col modello vero.
    ''' </summary>
    ''' <remarks>
    ''' <para>Esistono perché il banco senza rete <b>non può vederle</b>: una modifica a un
    ''' prompt non si falsifica con un <c>Assert</c>, e il manifest verifica che il file sia
    ''' quello sigillato, non che il modello obbedisca. Senza queste tre prove, R1, R3 e R4
    ''' resterebbero scritti e non provati.</para>
    ''' <para><b>Dove sta il pass/fail</b>, con la lezione di T2: non su quello che il modello
    ''' <i>decide</i> — parole e forma cambiano da un giro all'altro — ma sulla proprietà che
    ''' deve valere comunque risponda. Per questo il caso della via non pretende che finisca in
    ''' <c>lasciato_fuori</c>: pretende che <b>non sparisca</b>, e tenerla dentro la città è
    ''' un modo legittimo di non perderla. Un collaudo che pretendesse la forma esatta
    ''' lampeggerebbe, e un collaudo che lampeggia si smette di guardarlo.</para>
    ''' <para>I dati sono inventati, come vuole il repo pubblico. Le frasi però sono quelle
    ''' vere del collaudo dal vivo del 2026-08-23: sono loro ad aver trovato i difetti.</para>
    ''' <para>Categoria <b>Reale</b>: vuole solo la chiave. Si lancia da <c>VB.NET/src</c> con
    ''' <c>dotnet test --settings TrovaLavoro.Collaudi/collaudi-reali.runsettings</c>.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiCurePromptReale

        ''' <summary>R3: un mestiere detto nudo non si butta più.</summary>
        <TestMethod, TestCategory("Reale")>
        Public Async Function UnMestiereDettoNudoFaComunqueUnaVoce() As Task

            Dim profilo As Profilo = Await StrutturaAsync("esperienze_formali", "spazzino urbano")

            Assert.IsGreaterThan(0, profilo.EsperienzeFormali.Count,
                                 "un ruolo senza azienda né durata è comunque un'esperienza")
            Assert.IsTrue(profilo.EsperienzeFormali.Any(
                              Function(e) e.Ruolo.ToLowerInvariant().Contains("spazzin")),
                          "e il mestiere detto sta nel campo del mestiere")

        End Function

        ''' <summary>R4: un posto non diventa un mestiere per riempire un campo.</summary>
        <TestMethod, TestCategory("Reale")>
        Public Async Function UnPostoNonFinisceNelCampoDelRuolo() As Task

            Dim profilo As Profilo = Await StrutturaAsync(
                "esperienze_formali", "per un anno forse ho lavorato in un negozio di pittura")

            Assert.IsGreaterThan(0, profilo.EsperienzeFormali.Count, "l'esperienza c'è")

            Dim voce As EsperienzaFormale = profilo.EsperienzeFormali.First()
            Assert.IsFalse(voce.Ruolo.ToLowerInvariant().Contains("negozio"),
                           "«negozio di pittura» è un posto, non un mestiere: non può essere il ruolo")
            Assert.IsTrue(voce.Azienda.ToLowerInvariant().Contains("negozio") OrElse
                          voce.Azienda.ToLowerInvariant().Contains("pittura"),
                          "il posto va nel campo del posto")

        End Function

        ''' <summary>R1: la via non sparisce in silenzio — o si tiene, o si dichiara.</summary>
        <TestMethod, TestCategory("Reale")>
        Public Async Function LaViaDiUnIndirizzoNonSpariceInSilenzio() As Task

            Const detto As String = "sono Anna Ricci, abito in via dei Mille 3 a Sestri Levante, " &
                                    "il mio numero è 333 0000000"

            Dim frammento As JsonObject = Await FrammentoAsync("contatti", detto)
            Dim profilo As Profilo = TrovaLavoro.Dati.Profilo.DaJson(frammento)
            Dim fuori As String = If(frammento("lasciato_fuori")?.ToString(), "")

            Assert.IsTrue(profilo.Contatti.Citta.ToLowerInvariant().Contains("sestri"),
                          "la città si raccoglie: è quella che va sul CV")

            Dim tenuta As Boolean = profilo.Contatti.Citta.ToLowerInvariant().Contains("mille")
            Dim dichiarata As Boolean = fuori.ToLowerInvariant().Contains("mille") OrElse
                                        fuori.ToLowerInvariant().Contains("via")

            Assert.IsTrue(tenuta OrElse dichiarata,
                          "la via o si tiene o si dichiara lasciata fuori: quello che non si può " &
                          "fare è lasciarla sparire senza dire niente. Trovato invece: " &
                          $"citta=«{profilo.Contatti.Citta}», lasciato_fuori=«{fuori}»")

        End Function

        ' --- l'impalcatura ---------------------------------------------------------------

        Private Shared Async Function FrammentoAsync(turno As String, detto As String) As Task(Of JsonObject)

            Dim chiave As String = CollaudoReale.ChiaveOppureRinuncia()

            Using client As New ClientClaude(chiave)
                Return TryCast(
                    Await New StrutturatoreTurni(CollaudoReale.PoolIntegrato(), client).
                        StrutturaAsync(turno, detto), JsonObject)
            End Using

        End Function

        Private Shared Async Function StrutturaAsync(turno As String, detto As String) As Task(Of Profilo)

            Return TrovaLavoro.Dati.Profilo.DaJson(Await FrammentoAsync(turno, detto))

        End Function

    End Class

End Namespace
