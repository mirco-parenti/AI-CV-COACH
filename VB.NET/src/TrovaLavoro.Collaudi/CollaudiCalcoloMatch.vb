Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Non-regressione di <c>CalcoloMatch</c> contro il prototipo (cap. 14, T2).
    ''' Sono i sei casi dell'hard-gate dello Step 1.37 — gate attivo, gate ma requisito
    ''' soddisfatto, flag come stringa, match già basso che non va alzato, gate più
    ''' clamp con note coerenti — con in più il caso di andamento normale, e i tre del
    ''' <b>confine</b> aggiunti a T9e ripercorrendo la voce 8 della checklist «Problemi e
    ''' mitigazioni»: ciò che esce dal conteggio, e cosa resta quando non ne esce niente.
    ''' </summary>
    ''' <remarks>
    ''' I valori attesi non sono stati scritti a mano: sono l'uscita di
    ''' <c>calcolaMatch</c> estratto da HTML+JS/server.js ed eseguito su questi stessi
    ''' input. Il prototipo resta il giudice finché la non-regressione non passa.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiCalcoloMatch

        ''' <summary>Costruisce i giudizi da un frammento JSON, come arrivano dall'AI.</summary>
        Private Shared Function Giudizi(json As String) As JsonArray
            Return CType(JsonNode.Parse(json), JsonArray)
        End Function

        ''' <summary>Confronta tutti i campi del risultato con quelli del prototipo.</summary>
        Private Shared Sub Verifica(caso As String, esito As RisultatoMatch,
                                    scoreBase As Integer?, numeroLlm As Double?,
                                    matchFinale As Integer?, stelle As Double?,
                                    scartoTagliato As Boolean, gate As Boolean,
                                    nota As String)

            Assert.AreEqual(scoreBase, esito.ScoreBase, $"[{caso}] score_base")
            Assert.AreEqual(numeroLlm, esito.NumeroLlm, $"[{caso}] numero_llm")
            Assert.AreEqual(matchFinale, esito.MatchFinale, $"[{caso}] match_finale")
            Assert.AreEqual(stelle, esito.Stelle, $"[{caso}] stelle")
            Assert.AreEqual(scartoTagliato, esito.ScartoTagliato, $"[{caso}] scarto_tagliato")
            Assert.AreEqual(gate, esito.GateEliminatorio, $"[{caso}] gate_eliminatorio")
            Assert.AreEqual(nota, esito.Nota, $"[{caso}] nota")

        End Sub

        <TestMethod>
        Public Sub Caso1_SenzaGate()
            ' Andamento normale: nessun eliminatorio, l'AI concorda col conteggio.
            Dim g = Giudizi("[
                {""requisito"":""Patente B"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Inglese fluente"",""esito"":""in parte"",""priorita"":""preferenziale"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Diploma"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""}]")

            Verifica("1 senza gate", CalcoloMatch.Calcola(g, JsonValue.Create(85)),
                     scoreBase:=95, numeroLlm:=85, matchFinale:=85, stelle:=4.3,
                     scartoTagliato:=False, gate:=False, nota:=Nothing)
        End Sub

        <TestMethod>
        Public Sub Caso2_GateAttivo()
            ' La patente C è tassativa e manca: il match cratera a 1 stella nonostante
            ' tutto il resto sia soddisfatto.
            Dim g = Giudizi("[
                {""requisito"":""Patente C indispensabile"",""esito"":""non soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito"",""eliminatorio"":true},
                {""requisito"":""Esperienza di guida"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Diploma"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Sede a Forlì"",""esito"":""soddisfatto"",""priorita"":""non specificata"",""importanza"":"""",""categoria"":""contesto""}]")

            Verifica("2 gate attivo", CalcoloMatch.Calcola(g, JsonValue.Create(70)),
                     scoreBase:=67, numeroLlm:=70, matchFinale:=20, stelle:=1,
                     scartoTagliato:=False, gate:=True,
                     nota:="Requisito eliminatorio non soddisfatto (Patente C indispensabile): " &
                           "il match non può superare 20/100, cioè ≤ 1 stella, a prescindere dal resto del profilo.")
        End Sub

        <TestMethod>
        Public Sub Caso3_GateMaRequisitoSoddisfatto()
            ' Eliminatorio sì, ma il requisito c'è: il cancello non deve scattare.
            Dim g = Giudizi("[
                {""requisito"":""Patente B indispensabile"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito"",""eliminatorio"":true},
                {""requisito"":""Esperienza di guida"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Diploma"",""esito"":""in parte"",""priorita"":""preferenziale"",""importanza"":"""",""categoria"":""requisito""}]")

            Verifica("3 gate ma soddisfatto", CalcoloMatch.Calcola(g, JsonValue.Create(90)),
                     scoreBase:=95, numeroLlm:=90, matchFinale:=90, stelle:=4.5,
                     scartoTagliato:=False, gate:=False, nota:=Nothing)
        End Sub

        <TestMethod>
        Public Sub Caso4_FlagComeStringa()
            ' Il flag arriva come stringa "true" invece che come booleano: il prototipo
            ' lo accetta, e la trascrizione deve accettarlo allo stesso modo.
            Dim g = Giudizi("[
                {""requisito"":""Iscrizione all'albo obbligatoria"",""esito"":""non soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito"",""eliminatorio"":""true""},
                {""requisito"":""Laurea"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Inglese"",""esito"":""soddisfatto"",""priorita"":""preferenziale"",""importanza"":"""",""categoria"":""requisito""}]")

            Verifica("4 flag stringa", CalcoloMatch.Calcola(g, JsonValue.Create(60)),
                     scoreBase:=55, numeroLlm:=60, matchFinale:=20, stelle:=1,
                     scartoTagliato:=False, gate:=True,
                     nota:="Requisito eliminatorio non soddisfatto (Iscrizione all'albo obbligatoria): " &
                           "il match non può superare 20/100, cioè ≤ 1 stella, a prescindere dal resto del profilo.")
        End Sub

        <TestMethod>
        Public Sub Caso5_MatchGiaBassoNonVieneAlzato()
            ' Il tetto è un massimo, non un valore da imporre: qui il match vale già 5
            ' e deve restare 5. Il flag del gate però resta acceso.
            Dim g = Giudizi("[
                {""requisito"":""Abilitazione richiesta per legge"",""esito"":""non soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito"",""eliminatorio"":true},
                {""requisito"":""Esperienza decennale"",""esito"":""non soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Laurea magistrale"",""esito"":""non soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""}]")

            Verifica("5 già basso", CalcoloMatch.Calcola(g, JsonValue.Create(5)),
                     scoreBase:=0, numeroLlm:=5, matchFinale:=5, stelle:=0.3,
                     scartoTagliato:=False, gate:=True,
                     nota:="Requisito eliminatorio non soddisfatto (Abilitazione richiesta per legge): " &
                           "il match non può superare 20/100, cioè ≤ 1 stella, a prescindere dal resto del profilo.")
        End Sub

        <TestMethod>
        Public Sub Caso6_GatePiuClampNoteCoerenti()
            ' Le due note convivono e devono restare coerenti: quella del clamp cita 85,
            ' il valore PRIMA del tetto, perché è ciò che darebbe la fusione col numero
            ' dell'AI; poi la nota del gate spiega perché il finale è invece 20.
            Dim g = Giudizi("[
                {""requisito"":""Patente C tassativa"",""esito"":""non soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito"",""eliminatorio"":true},
                {""requisito"":""Esperienza logistica"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Muletto"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Diploma"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""}]")

            Verifica("6 gate più clamp", CalcoloMatch.Calcola(g, JsonValue.Create(95)),
                     scoreBase:=75, numeroLlm:=95, matchFinale:=20, stelle:=1,
                     scartoTagliato:=True, gate:=True,
                     nota:="Il conteggio dei requisiti darebbe 75, ma la valutazione d'insieme dell'AI " &
                           "lo porta verso 95: match finale 85. " &
                           "Requisito eliminatorio non soddisfatto (Patente C tassativa): " &
                           "il match non può superare 20/100, cioè ≤ 1 stella, a prescindere dal resto del profilo.")
        End Sub

        ' ==================================================================
        ' Il confine «lacuna» / «non si sa» (checklist, voce 8) — T9e
        ' ==================================================================

        ''' <summary>
        ''' Il conteggio non deve contare ciò che nessuno poteva sapere: <c>non
        ''' determinabile</c> esce dal numeratore <b>e</b> dal denominatore.
        ''' </summary>
        ''' <remarks>
        ''' È la metà-codice della voce 8 della checklist, ed è una riga sola di
        ''' <see cref="CalcoloMatch"/> che fino a T9e nessun collaudo guardava. Il caso è
        ''' costruito perché la differenza si veda: una lacuna, un dubbio e una voce
        ''' coperta, tutte di peso 5. Escludendo il dubbio il conteggio è 5/10 = 50; se
        ''' invece finisse nel denominatore come una lacuna sarebbe 5/15 = 33, e il
        ''' candidato pagherebbe per una domanda che non gli è mai stata fatta.
        ''' </remarks>
        <TestMethod>
        Public Sub Caso7_NonDeterminabileEsceDalConteggio()
            Dim g = Giudizi("[
                {""requisito"":""Diploma di istituto alberghiero"",""esito"":""non soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""formazione""},
                {""requisito"":""Disponibilità nei fine settimana"",""esito"":""non determinabile"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""altri_requisiti""},
                {""requisito"":""Uso della cassa"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""competenze""}]")

            Verifica("7 non determinabile escluso", CalcoloMatch.Calcola(g, JsonValue.Create(50)),
                     scoreBase:=50, numeroLlm:=50, matchFinale:=50, stelle:=2.5,
                     scartoTagliato:=False, gate:=False, nota:=Nothing)
        End Sub

        ''' <summary>
        ''' «Nessuna esperienza richiesta» non è un requisito da soddisfare: esce dal
        ''' conteggio <b>qualunque</b> esito le abbia dato l'AI.
        ''' </summary>
        ''' <remarks>
        ''' Il sentinel lo scrive il prompt dell'annuncio quando l'offerta dichiara che
        ''' esperienza non ne serve; il prompt del confronto ordina di giudicarlo «non
        ''' determinabile», ma è il codice a garantirlo, e apposta: qui il caso gli dà
        ''' l'esito peggiore — <c>non soddisfatto</c> — e il conteggio deve ignorarlo lo
        ''' stesso. Contarlo darebbe 5/10 = 50 invece di 5/5 = 100: un candidato bocciato
        ''' a metà per non avere l'esperienza che l'annuncio non chiedeva.
        ''' </remarks>
        <TestMethod>
        Public Sub Caso8_NessunaEsperienzaRichiestaNonPesaMaiSulConteggio()
            Dim g = Giudizi("[
                {""requisito"":""Nessuna esperienza richiesta"",""esito"":""non soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""esperienza""},
                {""requisito"":""Uso del muletto"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""competenze""}]")

            Verifica("8 sentinel nessuna esperienza", CalcoloMatch.Calcola(g, JsonValue.Create(95)),
                     scoreBase:=100, numeroLlm:=95, matchFinale:=95, stelle:=4.8,
                     scartoTagliato:=False, gate:=False, nota:=Nothing)
        End Sub

        ''' <summary>
        ''' Se <b>tutte</b> le voci sono indeterminabili non c'è nessun conteggio da fare:
        ''' lo <c>score_base</c> resta vuoto e il match è quello che dice l'AI.
        ''' </summary>
        ''' <remarks>
        ''' È il ramo del denominatore a zero, l'unico posto in cui il numero dell'AI non
        ''' viene corretto da niente. Non è un caso di scuola: capita a un annuncio che
        ''' chiede solo disponibilità e offre solo benefit, dove il profilo non ha nulla da
        ''' opporre. Il collaudo lo fissa perché è anche il ramo più facile da rompere
        ''' cambiando il conteggio.
        ''' </remarks>
        <TestMethod>
        Public Sub Caso9_TutteIndeterminabiliLasciaDecidereAllAi()
            Dim g = Giudizi("[
                {""requisito"":""Disponibilità a trasferte"",""esito"":""non determinabile"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""altri_requisiti""},
                {""requisito"":""benefit: buoni pasto"",""esito"":""non determinabile"",""priorita"":""non specificata"",""importanza"":"""",""categoria"":""contesto""}]")

            Verifica("9 tutte non determinabili", CalcoloMatch.Calcola(g, JsonValue.Create(62)),
                     scoreBase:=Nothing, numeroLlm:=62, matchFinale:=62, stelle:=3.1,
                     scartoTagliato:=False, gate:=False, nota:=Nothing)
        End Sub

    End Class

End Namespace
