Imports System.Linq
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace NonRegressione

    ''' <summary>
    ''' I <b>giudici</b> della checklist «Problemi e mitigazioni» messi alla prova senza
    ''' rete: a ciascuno si dà il difetto che deve riconoscere, costruito a mano, e poi il
    ''' caso pulito.
    ''' </summary>
    ''' <remarks>
    ''' <para>Nasce da una falsificazione che <b>non</b> è diventata rossa (regola 14, e il
    ''' motivo per cui la regola esiste). Le prove di <see cref="CollaudiChecklistReale"/>
    ''' si falsificano rompendo la difesa nel prompt che sorvegliano: per le voci 1 e 8 il
    ''' modello ha ceduto e le prove se ne sono accorte, ma per le voci 2 e 7 ha continuato
    ''' a comportarsi bene anche senza le regole tolte — perché quei prompt difendono la
    ''' stessa cosa <b>su più strati</b>, e toglierne uno non basta. Restava una domanda
    ''' aperta: quelle prove sono verdi perché la difesa regge, o perché non saprebbero
    ''' vedere il contrario?</para>
    ''' <para>Qui la domanda ha risposta. Il difetto non lo produce il modello — lo si
    ''' scrive — e il giudice deve trovarlo: un profilo col lavoro in nero messo fra i
    ''' lavori veri, un annuncio riempito di requisiti che non erano scritti, una
    ''' competenza gonfiata, una lacuna archiviata come dubbio. Se un giorno uno di questi
    ''' giudici smettesse di funzionare, le prove reali resterebbero verdi senza guardare
    ''' più niente: è esattamente il caso che la regola 14 teme, e questi collaudi sono la
    ''' sua sentinella.</para>
    ''' <para>Nessuna rete e nessuna chiave: gira con la batteria di tutti i giorni.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiGiudiziChecklist

        ' --- Voce 1: il gonfiamento delle competenze ---------------------------------

        ''' <summary>Quel che l'utente aveva detto: due competenze, entrambe attenuate.</summary>
        Private Const DettoDallUtente As String =
            "Me la cavo col computer e un po' di inglese lo so."

        <TestMethod>
        Public Sub IlGiudiceVedeIlLivelloCheLUtenteNonHaDichiarato()

            Dim profilo As New Profilo With {
                .Competenze = New List(Of String) From {"Uso del computer", "Inglese B2"}}

            Dim trovate As List(Of String) =
                CollaudiChecklistReale.CompetenzeGonfiate(profilo, DettoDallUtente)

            Assert.HasCount(1, trovate, "«Inglese B2» è un livello che l'utente non ha dichiarato")
            Assert.Contains("b2", trovate(0), "il giudice dice quale livello ha trovato")

        End Sub

        <TestMethod>
        Public Sub IlGiudiceVedeLAttenuanteCheSiPerdeStrada()

            ' Il gonfiamento quieto: «un po' di inglese» che diventa «Inglese» e promette
            ' una lingua che il candidato non ha promesso.
            Dim profilo As New Profilo With {
                .Competenze = New List(Of String) From {"Uso del computer", "Inglese"}}

            Dim trovate As List(Of String) =
                CollaudiChecklistReale.CompetenzeGonfiate(profilo, DettoDallUtente)

            Assert.HasCount(1, trovate, "«Inglese» nudo ha perso l'attenuante dell'utente")
            Assert.Contains("nuda", trovate(0), "il giudice dice perché")

        End Sub

        <TestMethod>
        Public Sub IlGiudiceLasciaPassareLaNormalizzazioneLeggera()

            ' Ciò che il prompt ORDINA di fare non deve far cadere niente: le parole sono
            ' ripulite, l'attenuante è conservata, nessun livello è comparso.
            Dim profilo As New Profilo With {
                .Competenze = New List(Of String) From {"Uso del computer", "Un po' di inglese"}}

            Assert.IsEmpty(CollaudiChecklistReale.CompetenzeGonfiate(profilo, DettoDallUtente),
                           "la normalizzazione leggera non è un gonfiamento")

        End Sub

        <TestMethod>
        Public Sub UnaParolaSospettaDettaDallUtenteNonEUnGonfiamento()

            ' Se è l'utente a dirsi esperto, riportarlo è aderenza, non invenzione: la spia
            ' scatta solo su ciò che nella risposta non c'era.
            Dim profilo As New Profilo With {
                .Competenze = New List(Of String) From {"Esperto di computer"}}

            Assert.IsEmpty(CollaudiChecklistReale.CompetenzeGonfiate(
                               profilo, "Sono esperto di computer."),
                           "la parola veniva dall'utente")

        End Sub

        ' --- Voce 2: l'informale promossa a formale ----------------------------------

        <TestMethod>
        Public Sub IlGiudiceVedeIlLavoroInNeroMessoFraILavoriVeri()

            Dim profilo As New Profilo With {
                .EsperienzeFormali = New List(Of EsperienzaFormale) From {
                    New EsperienzaFormale With {
                        .Ruolo = "Magazziniere", .Azienda = "Rossi Imballaggi",
                        .Durata = "3 anni", .CosaFacevo = "Carico e scarico."},
                    New EsperienzaFormale With {
                        .Ruolo = "Aiuto al banco", .Azienda = "Banco del mercato del cognato",
                        .Durata = "D'estate", .CosaFacevo = "Davo una mano."}}}

            Dim trovate As List(Of String) = CollaudiChecklistReale.InformaliPromosseAFormali(profilo)

            Assert.HasCount(1, trovate, "il banco del cognato non è un lavoro formale")

        End Sub

        <TestMethod>
        Public Sub IlGiudiceVedeIlFrammentoAnchePerLaViaPiuQuieta()

            ' Il caso peggiore non è la voce a sé: è il frammento assorbito dentro
            ' «cosa_facevo» di un'esperienza vera, dove non si nota leggendo l'elenco.
            Dim profilo As New Profilo With {
                .EsperienzeFormali = New List(Of EsperienzaFormale) From {
                    New EsperienzaFormale With {
                        .Ruolo = "Magazziniere", .Azienda = "Rossi Imballaggi", .Durata = "3 anni",
                        .CosaFacevo = "Carico e scarico, e d'estate il banco del mercato."}}}

            Assert.HasCount(1, CollaudiChecklistReale.InformaliPromosseAFormali(profilo),
                            "il frammento era dentro cosa_facevo, e conta lo stesso")

        End Sub

        <TestMethod>
        Public Sub TreLavoriVeriNonFannoScattareNiente()

            Dim profilo As New Profilo With {
                .EsperienzeFormali = New List(Of EsperienzaFormale) From {
                    New EsperienzaFormale With {
                        .Ruolo = "Magazziniere", .Azienda = "Rossi Imballaggi", .Durata = "3 anni"},
                    New EsperienzaFormale With {
                        .Ruolo = "Aiuto giardiniere", .Azienda = "Verde Vivo", .Durata = "6 mesi"},
                    New EsperienzaFormale With {
                        .Ruolo = "Fattorino", .Azienda = "Pizzeria Vesuvio", .Durata = "2022"}}}

            Assert.IsEmpty(CollaudiChecklistReale.InformaliPromosseAFormali(profilo),
                           "sono tutti lavori veri")

        End Sub

        ' --- Voce 7: i requisiti «tipici» aggiunti all'annuncio ----------------------

        ''' <summary>Un'analisi come l'app la riceve, con dentro ciò che il caso vuole provare.</summary>
        ''' <remarks>
        ''' Non si chiama «Analisi» perché le variabili locali dei casi sì, e VB non distingue
        ''' le maiuscole: il nome coperto trasforma la chiamata in un'indicizzazione (la stessa
        ''' trappola già pagata in <c>VistaAnnuncio</c> e in <see cref="CollaudiChecklistReale"/>).
        ''' </remarks>
        Private Shared Function AnalisiFinta(requisiti As JsonArray, mansioni As JsonArray) As JsonObject

            Return New JsonObject From {
                {"competenze_richieste", New JsonArray()},
                {"esperienza_richiesta", New JsonArray()},
                {"formazione_richiesta", New JsonArray()},
                {"altri_requisiti", requisiti},
                {"mansioni", mansioni},
                {"benefit", New JsonArray()},
                {"titolo", "Magazziniere"},
                {"azienda", ""}}

        End Function

        <TestMethod>
        Public Sub IlGiudiceVedeIlRequisitoCheLAnnuncioNonScriveva()

            ' «Patente B richiesta» è il requisito tipico del magazziniere, ed è proprio
            ' ciò che l'annuncio scarno non dice.
            Dim analisi As JsonObject = AnalisiFinta(
                New JsonArray From {
                    New JsonObject From {{"testo", "Patente B richiesta"}, {"priorita", "richiesto"}}},
                New JsonArray())

            Dim trovate As List(Of String) = CollaudiChecklistReale.VociFuoriDallAnnuncio(analisi)

            Assert.HasCount(1, trovate, "la patente non era scritta da nessuna parte")
            Assert.Contains("altri_requisiti", trovate(0), "il giudice dice in quale lista")

        End Sub

        <TestMethod>
        Public Sub IlGiudiceGuardaAncheLeMansioniEIBenefit()

            ' «Carico e scarico merci» è ciò che un magazziniere fa, e un'analisi
            ' compiacente lo scrive anche quando l'annuncio non l'ha scritto.
            Dim analisi As JsonObject = AnalisiFinta(
                New JsonArray(),
                New JsonArray From {"Carico e scarico delle merci"})

            Assert.HasCount(1, CollaudiChecklistReale.VociFuoriDallAnnuncio(analisi),
                            "la mansione non era nell'annuncio")

        End Sub

        <TestMethod>
        Public Sub CioCheLAnnuncioScriveDavveroPassa()

            ' Le parole ci sono tutte, anche riordinate e ripulite: è la normalizzazione
            ' leggera che il prompt ordina, e non deve far cadere niente.
            '
            ' Il giudice è severo di proposito — ogni parola dalle quattro lettere in su
            ' deve stare nell'annuncio — e la prima stesura di questo caso è caduta per
            ' un «presso» che l'annuncio non scriveva. La severità resta: una parafrasi
            ' che aggiunge parole è il primo passo dell'invenzione, e nel dubbio è meglio
            ' guardarla. Chi legge il rapporto distingue in un attimo un «presso» da una
            ' patente comparsa dal nulla.
            Dim analisi As JsonObject = AnalisiFinta(
                New JsonArray From {
                    New JsonObject From {{"testo", "Contratto a tempo determinato"}, {"priorita", "richiesto"}}},
                New JsonArray From {"Magazziniere per la sede di Forlì"})

            Assert.IsEmpty(CollaudiChecklistReale.VociFuoriDallAnnuncio(analisi),
                           "ogni parola si ritrova nell'annuncio")

        End Sub

        ' --- Voce 8: la lacuna archiviata come dubbio --------------------------------

        ''' <summary>Un giudizio del confronto, come arriva dall'AI.</summary>
        Private Shared Function Giudizio(categoria As String, requisito As String,
                                         esito As String) As JsonObject

            Return New JsonObject From {
                {"requisito", requisito}, {"categoria", categoria}, {"esito", esito}}

        End Function

        <TestMethod>
        Public Sub IlGiudiceVedeLaLacunaArchiviataComeDubbio()

            Dim giudizi As New JsonArray From {
                Giudizio("formazione", "Diploma alberghiero", "non soddisfatto"),
                Giudizio("competenze", "Inglese per i clienti stranieri", "non determinabile"),
                Giudizio("esperienza", "3 anni in cucina", "non soddisfatto")}

            Dim trovati As List(Of String) =
                CollaudiChecklistReale.DubbiDoveDovrebberoEsserciLacune(giudizi)

            Assert.HasCount(1, trovati, "una competenza non dichiarata è una lacuna, non un dubbio")
            Assert.Contains("Inglese", trovati(0), "il giudice dice quale requisito")

        End Sub

        <TestMethod>
        Public Sub IlDubbioLegittimoNonFaScattareNiente()

            ' Le due sole categorie in cui «non si sa» è la risposta onesta: ciò che il
            ' profilo non raccoglie affatto, e il contesto lato-offerta.
            Dim giudizi As New JsonArray From {
                Giudizio("altri_requisiti", "Disponibilità nei fine settimana", "non determinabile"),
                Giudizio("contesto", "Benefit: vitto e alloggio", "non determinabile"),
                Giudizio("competenze", "Uso della cassa", "soddisfatto")}

            Assert.IsEmpty(CollaudiChecklistReale.DubbiDoveDovrebberoEsserciLacune(giudizi),
                           "lì il dubbio è la risposta giusta")

        End Sub

    End Class

End Namespace
