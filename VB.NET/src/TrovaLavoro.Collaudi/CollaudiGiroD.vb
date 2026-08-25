Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace NonRegressione

    ''' <summary>
    ''' I dati che si portano al <b>giro D</b> (cap. 13.10) reggono l'import? Il CV finto
    ''' di <c>casi/giro-d/</c> percorre la catena vera — trascrizione del PDF e
    ''' strutturazione — e il profilo che ne esce si confronta con
    ''' <c>casi/profilo.json</c>, che di quel CV è il <b>criterio</b>.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché esiste.</b> Il giro D si fa su una macchina che non è mia, con un
    ''' tempo che non si ripete: un profilo che l'estrazione non digerisce brucerebbe il
    ''' giro e misurerebbe il dato invece del programma. Provato qui, quel rischio è
    ''' chiuso — e resta chiuso: se domani un prompt del pool cambia e quel CV smette di
    ''' essere letto come deve, questo collaudo lo dice prima del viaggio, non durante.
    ''' </para>
    ''' <para><b>Il candidato è lo stesso di <c>profilo.json</c></b>, e non per pigrizia:
    ''' è ciò che rende il criterio un file invece di un giudizio a occhio. Nel CV ci sono
    ''' due insidie messe apposta — la <b>residenza a Cesena e il domicilio a Forlì</b>
    ''' (dal pool 1.02 il prompt tiene il domicilio), e i <b>traslochi sotto «Altre
    ''' esperienze»</b>, che devono finire fra le informali e mai fra i lavori.</para>
    ''' <para><b>Il buco che questo collaudo chiude.</b> La prima insidia aveva già un
    ''' guardiano: la città è un pass/fail di <see cref="CollaudiImportReale"/>. La
    ''' seconda no, e non per distrazione — <see cref="ControlloCollocazione"/> cerca la
    ''' parola «volontario», che un trasloco non dice mai, e
    ''' <see cref="ControlloDoppioni"/> vede solo la voce contata <i>due</i> volte. Un
    ''' trasloco promosso a impiego e sparito dalle informali passava fra i due, con i
    ''' conteggi dentro la tolleranza. Lo vede <see cref="ControlloCriterio"/>, che è nato
    ''' per questo.</para>
    ''' <para><b>Dove sta il pass/fail</b>: sui fatti, non sulla prosa. Le durate tengono
    ''' le parole del CV, <c>cosa_facevo</c> e le competenze il modello le riformula ogni
    ''' volta — pretenderle uguali sarebbe un collaudo che lampeggia (cap. 14, la lezione
    ''' di T2). Sotto il collaudo reale stanno i collaudi <b>senza rete</b> del controllo:
    ''' è lì che lo si vede diventare rosso, su profili storti apposta, senza spendere una
    ''' chiamata.</para>
    ''' <para>Categoria <b>Reale</b>: vuole <b>solo la chiave</b> — il CV è nel repo
    ''' perché è inventato, e il prototipo qui non serve, perché il metro non è lui ma il
    ''' criterio. Si lancia da <c>VB.NET/src</c> con
    ''' <c>dotnet test --settings TrovaLavoro.Collaudi/collaudi-reali.runsettings</c>.
    ''' </para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiGiroD

        ''' <summary>Sotto quanti caratteri la trascrizione non è il testo di un CV.</summary>
        Private Const CaratteriMinimi As Integer = 1000

        <TestMethod, TestCategory("Reale")>
        Public Async Function IDatiDelGiroDReggonoIlLoroCriterio() As Task

            Dim chiave As String = CollaudoReale.ChiaveOppureRinuncia()
            Dim pdf As String = CasiDiCollaudo.CvDelGiroD()
            ' Non «criterio»: VB non distingue le maiuscole, e la locale coprirebbe
            ' la funzione omonima qui sotto.
            Dim atteso As Profilo = Criterio()

            Dim libreria As LibreriaPrompt = CollaudoReale.PoolIntegrato()

            Using client As New ClientClaude(chiave)

                Dim esito As EsitoImport = Await New ImportProfilo(
                    New StrutturatoreTurni(libreria, client),
                    New TrascrittorePdf(libreria, client)).DaFileAsync(pdf)

                Dim profilo As Profilo = esito.Profilo

                ' Quello che esce si mostra comunque vada: se il collaudo diventa rosso,
                ' la prima domanda è «e allora che cosa ha capito?».
                Console.WriteLine(Riassunto(pdf, esito))

                Assert.IsGreaterThan(CaratteriMinimi, esito.TestoLetto.Length,
                    "dal PDF del giro D deve uscire il testo di un CV, non poche righe")

                ' --- Il criterio: i fatti del profilo atteso. -------------------------
                Dim scostamenti As List(Of String) = ControlloCriterio.Scostamenti(atteso, profilo)
                Assert.IsEmpty(scostamenti,
                    "il profilo importato non dice quello che dice casi/profilo.json: " &
                    String.Join(" · ", scostamenti))

                ' --- E i tre controlli che il banco ha già, sullo stesso profilo. -----
                ' Costano zero e guardano cose diverse: valori che nel CV non ci sono, la
                ' stessa attività contata due volte, un volontariato promosso a impiego.
                Dim inventate As CollaudoReale.Invenzioni =
                    CollaudoReale.ValoriFuoriDalTesto(profilo, esito.TestoLetto)
                Assert.IsEmpty(inventate.Gravi,
                    "nel profilo ci sono valori che nel CV non ci sono: " &
                    String.Join(" · ", inventate.Gravi))

                Dim doppioni As List(Of String) = ControlloDoppioni.Trova(profilo)
                Assert.IsEmpty(doppioni,
                    "la stessa attività è contata due volte: " & String.Join(" · ", doppioni))

                Dim malCollocate As List(Of String) =
                    ControlloCollocazione.VolontariatoFraLeFormali(profilo)
                Assert.IsEmpty(malCollocate,
                    "un'attività che si dichiara volontaria sta fra i lavori: " &
                    String.Join(" · ", malCollocate))

            End Using

        End Function

        ''' <summary>Il criterio, riletto ogni volta: chi lo storce non storce gli altri.</summary>
        Private Shared Function Criterio() As Profilo

            Return TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo())

        End Function

        ''' <summary>Quello che è uscito dall'import, in poche righe da leggere a video.</summary>
        Private Shared Function Riassunto(pdf As String, esito As EsitoImport) As String

            Dim profilo As Profilo = esito.Profilo
            Dim righe As New StringBuilder()

            righe.AppendLine($"CV del giro D: {Path.GetFileName(pdf)}")
            righe.AppendLine($"Testo trascritto: {esito.TestoLetto.Length} caratteri")
            righe.AppendLine($"Nome: {profilo.Nome} — città: {profilo.Contatti.Citta}")
            righe.AppendLine($"Patente: {profilo.Patente.Ha} {String.Join(", ", profilo.Patente.Categorie)}")

            For Each formale As EsperienzaFormale In profilo.EsperienzeFormali
                righe.AppendLine($"  formale: {formale.Ruolo} — {formale.Azienda} ({formale.Durata})")
            Next
            For Each informale As EsperienzaInformale In profilo.EsperienzeInformali
                righe.AppendLine($"  informale: {informale.CosaFacevo}")
            Next
            For Each voce As VoceFormazione In profilo.Formazione
                righe.AppendLine($"  formazione: {voce.Titolo} — {voce.Istituto} ({voce.Anno})")
            Next

            righe.AppendLine($"Competenze: {profilo.Competenze.Count}")

            Return righe.ToString()

        End Function

        ' --- Il controllo del criterio, provato senza rete ---------------------------
        ' Il collaudo qui sopra vuole la chiave e due chiamate; il controllo che gli serve
        ' si prova invece su profili storti a mano, e sono questi a dire che diventa rosso
        ' quando deve (cap. 14, la regola del collaudo falsificato). Ogni caso parte dal
        ' criterio vero e ne storce una cosa sola.

        <TestMethod>
        Public Sub IlCriterioReggeSeStesso()

            Assert.IsEmpty(ControlloCriterio.Scostamenti(Criterio(), Criterio()),
                           "il criterio confrontato con sé stesso non ha scostamenti")

        End Sub

        <TestMethod>
        Public Sub LaResidenzaAlPostoDelDomicilioSiVede()

            Dim storto As Profilo = Criterio()
            storto.Contatti.Citta = "Cesena"

            Assert.IsTrue(ControlloCriterio.Scostamenti(Criterio(), storto).
                              Any(Function(s) s.Contains("contatti.citta")),
                          "la città che torna alla residenza è la prima insidia del CV")

        End Sub

        <TestMethod>
        Public Sub UnTraslocoPromossoAImpiegoSiVede()

            Dim storto As Profilo = Criterio()
            storto.EsperienzeInformali.Clear()
            storto.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Traslochi e consegne di mobili",
                .Azienda = "",
                .Durata = "nei fine settimana, per un paio d'anni",
                .CosaFacevo = "Davo una mano a un amico che ha una ditta di traslochi, " &
                              "anche guidando il furgone della sua azienda"})

            Assert.IsNotEmpty(ControlloCriterio.InformaliPromosse(Criterio(), storto),
                              "un'attività informale finita fra i lavori è la seconda insidia, " &
                              "e nessun altro controllo del banco la vede")

        End Sub

        <TestMethod>
        Public Sub UnaSolaParolaInComuneNonBastaAPromuovere()

            ' Il limite dichiarato, provato: servono due parole distintive nella stessa
            ' voce. Un lavoro vero che nomina i mobili resta un lavoro vero.
            Dim storto As Profilo = Criterio()
            storto.EsperienzeFormali.First().CosaFacevo &= " Montaggio di mobili in reparto."

            Assert.IsEmpty(ControlloCriterio.InformaliPromosse(Criterio(), storto),
                           "una parola sola in comune capita per caso e non è un difetto")

        End Sub

        <TestMethod>
        Public Sub UnaInformaleSpezzataInDueNonEUnDifetto()

            Dim storto As Profilo = Criterio()
            storto.EsperienzeInformali.Add(New EsperienzaInformale With {
                .CosaFacevo = "Consegne di mobili col furgone",
                .Quando = "nei fine settimana",
                .ConChi = "un amico che ha una ditta di traslochi"})

            Assert.IsEmpty(ControlloCriterio.Scostamenti(Criterio(), storto),
                           "distillare un racconto in due voci invece che in una è un giudizio")

        End Sub

        <TestMethod>
        Public Sub UnaInformalePersaSiVede()

            Dim storto As Profilo = Criterio()
            storto.EsperienzeInformali.Clear()

            Assert.IsTrue(ControlloCriterio.Scostamenti(Criterio(), storto).
                              Any(Function(s) s.Contains("esperienze_informali")),
                          "un'attività che sparisce del tutto non è un giudizio, è una perdita")

        End Sub

        <TestMethod>
        Public Sub UnDatoreDiLavoroCheSparisceSiVede()

            Dim storto As Profilo = Criterio()
            storto.EsperienzeFormali.First().Azienda = "Logistica Bianchi S.p.A."

            Assert.IsTrue(ControlloCriterio.Scostamenti(Criterio(), storto).
                              Any(Function(s) s.Contains("Romagna Logistica")),
                          "il nome del datore di lavoro il prompt lo copia: non può cambiare")

        End Sub

        <TestMethod>
        Public Sub LeCompetenzeVuoteSiVedono()

            Dim storto As Profilo = Criterio()
            storto.Competenze.Clear()

            Assert.IsTrue(ControlloCriterio.Scostamenti(Criterio(), storto).
                              Any(Function(s) s.Contains("competenze")),
                          "da un CV intero qualche competenza deve uscire")

        End Sub

        <TestMethod>
        Public Sub LaProsaDiversaNonEUnoScostamento()

            ' La prova che il collaudo non lampeggia: le durate tengono le parole del CV e
            ' cosa_facevo il modello lo riformula ogni volta. Nessuna delle due è un
            ' difetto, e un collaudo che le bocciasse si smetterebbe di guardarlo.
            Dim storto As Profilo = Criterio()
            storto.EsperienzeFormali.First().Durata = "marzo 2021 - oggi (3 anni)"
            storto.EsperienzeFormali.First().CosaFacevo =
                "Carico e scarico delle merci col muletto; controllo delle bolle; " &
                "preparazione degli ordini"
            storto.Competenze.Add("Uso del carrello elevatore")

            Assert.IsEmpty(ControlloCriterio.Scostamenti(Criterio(), storto),
                           "la forma delle parole cambia a ogni chiamata: si giudicano i fatti")

        End Sub

    End Class

End Namespace
