Imports System.Linq
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi della vista tipizzata del confronto (cap. 03.6): quello che P4 disegna.
    ''' </summary>
    ''' <remarks>
    ''' <para>Due domande sole, ma insistenti. La prima: <b>quel che si mostra coincide con
    ''' quel che ha fatto il punteggio</b> — la sentinella esclusa dal conteggio non
    ''' compare nemmeno nell'elenco, e il ⛔ si accende sulle stesse voci che hanno
    ''' craterato il match (v. <c>CalcoloMatch</c>). La seconda: <b>il JSON storto non
    ''' rompe niente</b>, perché quello che arriva è la risposta di un modello.</para>
    ''' <para>La vista non calcola: le stelle e le note vengono dal <c>RisultatoMatch</c>
    ''' salvato con l'opportunità, cioè dal giudizio di quel giorno.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiVistaConfronto

        Private Const ConfrontoDiProva As String =
            "{""giudizi"": [" &
            "{""requisito"": ""Patente C"", ""categoria"": ""altri_requisiti""," &
            """priorita"": ""richiesto"", ""importanza"": """", ""esito"": ""non soddisfatto""," &
            """eliminatorio"": true, ""spiegazione"": ""Il profilo dichiara la sola B.""}," &
            "{""requisito"": ""Uso del muletto"", ""categoria"": ""competenze""," &
            """priorita"": ""non specificata"", ""importanza"": ""alta"", ""esito"": ""soddisfatto""," &
            """eliminatorio"": false, ""spiegazione"": ""Dichiarato fra le competenze.""}," &
            "{""requisito"": ""Tre anni di esperienza"", ""categoria"": ""esperienza""," &
            """priorita"": ""preferenziale"", ""esito"": ""in parte"", ""eliminatorio"": false}," &
            "{""requisito"": ""Buoni pasto"", ""categoria"": ""contesto""," &
            """priorita"": ""non specificata"", ""esito"": ""non determinabile"", ""eliminatorio"": false}]," &
            """lettura_insieme"": ""Manca il paletto della patente."", ""numero_complessivo"": 40}"

        ' ==================================================================
        ' Quel che si mostra
        ' ==================================================================

        <TestMethod>
        Public Sub SenzaConfrontoNonCEUnaVista()

            ' Non è un guasto: fra l'analisi e il confronto l'opportunità esiste già, e
            ' non ha ancora niente da mostrare (cap. 12, A5→A7).
            Assert.IsNull(VistaConfronto.Da(New Opportunita), "niente confronto, niente vista")

        End Sub

        <TestMethod>
        Public Sub OgniGiudizioPortaISuoiCampiEIlSuoSegno()

            Dim vista As VistaConfronto = VistaDiProva()

            Assert.HasCount(4, vista.Giudizi, "i quattro giudizi, nell'ordine in cui l'AI li ha dati")

            Dim patente As GiudizioMostrato = vista.Giudizi(0)
            Assert.AreEqual("Patente C", patente.Requisito, "la voce dell'annuncio")
            Assert.AreEqual("altri_requisiti", patente.Categoria, "da quale lista viene")
            Assert.AreEqual(EsitoGiudizio.NonSoddisfatto, patente.Esito, "com'è andata")
            Assert.AreEqual("non soddisfatto", patente.NomeEsito, "con le parole del prompt")
            Assert.AreEqual("✗", patente.Simbolo, "e il segno che si legge a colpo d'occhio")
            Assert.IsTrue(patente.Eliminatorio, "è il paletto")
            Assert.Contains("la sola B", patente.Spiegazione, "col perché ancorato al profilo")

        End Sub

        <TestMethod>
        Public Sub IQuattroEsitiHannoIQuattroSegni()

            ' Cap. 12, A5: ✓ soddisfatto, ~ in parte, ✗ non soddisfatto, ? non determinabile.
            Dim segni As List(Of String) = VistaDiProva().Giudizi.Select(Function(g) g.Simbolo).ToList()

            Assert.AreEqual("✗ ✓ ~ ?", String.Join(" ", segni), "un segno per esito, e sempre lo stesso")

        End Sub

        <TestMethod>
        Public Sub IlPesoEQuelloDellAnnuncioOQuelloStimatoDallAi()

            ' Delle due ne è compilata sempre una sola: la priorità quando l'annuncio l'ha
            ' dichiarata, l'importanza stimata quando ha lasciato la voce lì senza dirlo.
            Dim vista As VistaConfronto = VistaDiProva()

            Assert.AreEqual("richiesto", vista.Giudizi(0).Peso, "la priorità dell'annuncio")
            Assert.AreEqual("alta", vista.Giudizi(1).Peso, "l'importanza, dov'era «non specificata»")
            Assert.AreEqual("preferenziale", vista.Giudizi(2).Peso, "e di nuovo la priorità")

        End Sub

        <TestMethod>
        Public Sub LaVoceSentinellaNonSiMostra()

            ' «Nessuna esperienza richiesta» dichiara l'ASSENZA di un requisito: non entra
            ' nel conteggio (CalcoloMatch) e per chi legge sarebbe solo rumore. Quel che si
            ' mostra e quel che fa punteggio devono essere la stessa cosa.
            Dim confronto As String =
                "{""giudizi"": [" &
                "{""requisito"": ""Nessuna esperienza richiesta"", ""categoria"": ""esperienza""," &
                """esito"": ""non determinabile"", ""eliminatorio"": false}," &
                "{""requisito"": ""Uso del muletto"", ""categoria"": ""competenze""," &
                """esito"": ""soddisfatto"", ""eliminatorio"": false}]}"

            Dim vista As VistaConfronto = VistaConfronto.Da(Confrontata(confronto))

            Assert.HasCount(1, vista.Giudizi, "la sentinella resta fuori")
            Assert.AreEqual("Uso del muletto", vista.Giudizi(0).Requisito, "e resta il requisito vero")

        End Sub

        <TestMethod>
        Public Sub LaLetturaDInsiemeArrivaComEScritta()

            Assert.AreEqual("Manca il paletto della patente.", VistaDiProva().LetturaInsieme,
                            "la sintesi onesta, parola per parola")

        End Sub

        ' ==================================================================
        ' Le stelle, le note, il paletto
        ' ==================================================================

        <TestMethod>
        Public Sub StelleENoteVengonoDalPunteggioDiQuelGiorno()

            ' La vista non ricalcola niente: mostra il RisultatoMatch salvato con
            ' l'opportunità, che è il giudizio in base a cui si è deciso di candidarsi.
            Dim opportunita As Opportunita = Confrontata(ConfrontoDiProva)
            opportunita.Match = New RisultatoMatch With {
                .Stelle = 0.9, .MatchFinale = 18, .GateEliminatorio = True,
                .Nota = "Requisito eliminatorio non soddisfatto (Patente C): il match non può superare 20/100."}

            Dim vista As VistaConfronto = VistaConfronto.Da(opportunita)

            Assert.AreEqual(0.9, vista.Stelle, "le stelle di quel giorno")
            Assert.IsTrue(vista.GateEliminatorio, "il tetto è scattato")
            Assert.Contains("Patente C", vista.Nota, "e la nota dice quale voce l'ha imposto")

        End Sub

        <TestMethod>
        Public Sub SottoLaSogliaLaCandidaturaSiSconsiglia()

            ' Cap. 12, A5.3: sconsigliata, mai impedita — la scelta resta dell'utente.
            Assert.IsTrue(ConStelle(1.4).Sconsigliata, "sotto 1,5 si sconsiglia")
            Assert.IsFalse(ConStelle(1.5).Sconsigliata, "alla soglia no")
            Assert.IsFalse(ConStelle(4.6).Sconsigliata, "e tanto meno sopra")

        End Sub

        <TestMethod>
        Public Sub SenzaStelleNonSiSconsigliaNiente()

            ' Un confronto senza punteggio è possibile (nessuna voce determinabile e
            ' nessun numero dall'AI): lì non si sconsiglia, si tace.
            Dim vista As VistaConfronto = VistaConfronto.Da(Confrontata(ConfrontoDiProva))

            Assert.IsNull(vista.Stelle, "niente stelle")
            Assert.IsFalse(vista.Sconsigliata, "e nessun consiglio da dare")

        End Sub

        <TestMethod>
        Public Sub GliEliminatoriSonoSoloIPalettiRimastiScoperti()

            ' Un requisito tassativo *soddisfatto* non è un problema: il ⛔ segna quelli
            ' che hanno craterato il match, come nel calcolo.
            Dim confronto As String =
                "{""giudizi"": [" &
                "{""requisito"": ""Patente C"", ""esito"": ""non soddisfatto"", ""eliminatorio"": true}," &
                "{""requisito"": ""Iscrizione all'albo"", ""esito"": ""soddisfatto"", ""eliminatorio"": true}," &
                "{""requisito"": ""Inglese"", ""esito"": ""non soddisfatto"", ""eliminatorio"": false}]}"

            Dim eliminatori As IReadOnlyList(Of GiudizioMostrato) =
                VistaConfronto.Da(Confrontata(confronto)).Eliminatori()

            Assert.HasCount(1, eliminatori, "uno solo")
            Assert.AreEqual("Patente C", eliminatori(0).Requisito, "il paletto scoperto")

        End Sub

        <TestMethod>
        Public Sub IlFlagEliminatorioSiLeggeComeNelCalcolo()

            ' Il prototipo accetta il booleano vero e la stringa «true» scritta in
            ' qualunque modo: se il calcolo cratera il match, il pannello deve mostrare il
            ' ⛔ sulla stessa voce — altrimenti l'utente vede un tetto senza la sua causa.
            Dim confronto As String =
                "{""giudizi"": [" &
                "{""requisito"": ""Patente C"", ""esito"": ""non soddisfatto"", ""eliminatorio"": ""TRUE""}," &
                "{""requisito"": ""Inglese"", ""esito"": ""non soddisfatto"", ""eliminatorio"": ""no""}]}"

            Dim vista As VistaConfronto = VistaConfronto.Da(Confrontata(confronto))

            Assert.IsTrue(vista.Giudizi(0).Eliminatorio, "la stringa «TRUE» vale come il booleano")
            Assert.IsFalse(vista.Giudizi(1).Eliminatorio, "qualunque altra cosa no")

        End Sub

        ' ==================================================================
        ' Il JSON storto
        ' ==================================================================

        <TestMethod>
        Public Sub UnEsitoCheIlPromptNonPrevedeSiMostraComE()

            ' Meglio l'esito scritto com'è che un simbolo inventato: chi legge capisce
            ' che quella voce è strana, e non le crede più di quanto merita.
            Dim vista As VistaConfronto = VistaConfronto.Da(Confrontata(
                "{""giudizi"": [{""requisito"": ""Inglese"", ""esito"": ""quasi soddisfatto""}]}"))

            Assert.AreEqual(EsitoGiudizio.Sconosciuto, vista.Giudizi(0).Esito, "non si riconosce")
            Assert.AreEqual("quasi soddisfatto", vista.Giudizi(0).NomeEsito, "ma si legge")
            Assert.AreEqual("·", vista.Giudizi(0).Simbolo, "e il segno non finge di sapere")

        End Sub

        <TestMethod>
        Public Sub ICampiCheMancanoNonFannoCrollareNiente()

            ' Quello che arriva è la risposta di un modello: un campo assente, un numero
            ' al posto di un testo, una voce che non è nemmeno un oggetto.
            Dim vista As VistaConfronto = VistaConfronto.Da(Confrontata(
                "{""giudizi"": [{}, ""una riga di testo"", {""requisito"": 7, ""esito"": ""soddisfatto""}]}"))

            Assert.HasCount(2, vista.Giudizi, "le voci-oggetto restano, l'altra si scarta")
            Assert.IsEmpty(vista.Giudizi(0).Requisito, "il campo che manca diventa vuoto")
            Assert.IsEmpty(vista.Giudizi(0).NomeEsito, "e l'esito pure")
            Assert.AreEqual(EsitoGiudizio.Sconosciuto, vista.Giudizi(0).Esito, "senza esito non si riconosce niente")
            Assert.IsEmpty(vista.Giudizi(1).Requisito, "un requisito che non è un testo si ignora")
            Assert.AreEqual(EsitoGiudizio.Soddisfatto, vista.Giudizi(1).Esito, "il resto della voce si legge lo stesso")

        End Sub

        <TestMethod>
        Public Sub UnConfrontoSenzaLaListaDeiGiudiziResta()

            ' Il confronto c'è ma non ha la forma attesa: la vista esiste, con la lettura
            ' d'insieme che si è potuta leggere e nessun giudizio.
            Dim vista As VistaConfronto = VistaConfronto.Da(Confrontata(
                "{""lettura_insieme"": ""Non me la sento di giudicare.""}"))

            Assert.IsNotNull(vista, "la vista c'è")
            Assert.IsEmpty(vista.Giudizi, "senza giudizi")
            Assert.Contains("Non me la sento", vista.LetturaInsieme, "ma con quel che si è capito")

        End Sub

        ' ==================================================================
        ' Il banco
        ' ==================================================================

        Private Shared Function Confrontata(confronto As String) As Opportunita

            Return New Opportunita With {
                .Annuncio = JsonNode.Parse("{""titolo"": ""Autista"", ""azienda"": ""Rossi S.p.A.""}"),
                .Confronto = JsonNode.Parse(confronto)}

        End Function

        Private Shared Function VistaDiProva() As VistaConfronto
            Return VistaConfronto.Da(Confrontata(ConfrontoDiProva))
        End Function

        Private Shared Function ConStelle(quante As Double) As VistaConfronto

            Dim opportunita As Opportunita = Confrontata(ConfrontoDiProva)
            opportunita.Match = New RisultatoMatch With {.Stelle = quante}

            Return VistaConfronto.Da(opportunita)

        End Function

    End Class

End Namespace
