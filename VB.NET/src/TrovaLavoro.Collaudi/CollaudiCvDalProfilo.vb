Imports System.Linq
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti

Namespace Documenti

    ''' <summary>
    ''' Collaudi della composizione del 📄 CV base <b>senza AI</b>
    ''' (<see cref="CvDalProfilo"/>): che ricopi tutti i campi-fatto, che non scriva
    ''' niente di suo, e che non prometta quel che non fa.
    ''' </summary>
    ''' <remarks>
    ''' <para>È un modulo puro, quindi qui non si monta niente: né cartella dati, né
    ''' contesto, né finestra. Un profilo in ingresso, un artefatto in uscita.</para>
    ''' <para>Il collaudo che conta più di tutti è l'ultimo:
    ''' <see cref="NonCompareUnaParolaCheIlProfiloNonAbbia"/>. Gli altri guardano se un
    ''' campo è arrivato; quello guarda se è arrivato <b>qualcosa in più</b>, che è la
    ''' promessa del prodotto (cap. anti-invenzione) e la sola cosa che una composizione
    ''' come questa potrebbe tradire senza che si veda.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiCvDalProfilo

        ''' <summary>Un profilo con dentro un po' di tutto, per non collaudare a metà.</summary>
        Private Shared Function ProfiloDiProva() As Profilo

            Dim p As New Profilo With {.Nome = "Crash Bandicoot"}

            p.Contatti.Email = "crash.bandicoot@esempio.it"
            p.Contatti.Telefono = "333 0000000"
            p.Contatti.Citta = "Wumpa Island"
            p.Contatti.Link = "www.esempio.it/crash"

            p.Patente.Ha = "sì"
            p.Patente.Categorie = New List(Of String) From {"B", "C"}

            p.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Magazziniere", .Azienda = "Depositi Wumpa",
                .Durata = "2023-2025", .CosaFacevo = "Carico e scarico merci"})

            p.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Assistente di laboratorio", .Azienda = "Istituto Cortex",
                .Durata = "2022", .CosaFacevo = "Prove sui materiali", .Tipo = "tirocinio"})

            p.EsperienzeInformali.Add(New EsperienzaInformale With {
                .CosaFacevo = "Raccolta frutta", .Quando = "estate 2021", .ConChi = "una cooperativa"})

            p.Competenze.Add("Uso del muletto")
            p.Competenze.Add("Inglese scolastico")

            p.Formazione.Add(New VoceFormazione With {
                .Titolo = "Diploma di perito", .Istituto = "ITIS Esempio", .Anno = "2021"})

            Return p

        End Function

        Private Shared Function Testo(cv As JsonObject, chiave As String) As String
            Return If(cv(chiave)?.GetValue(Of String)(), "")
        End Function

        Private Shared Function Voci(cv As JsonObject, chiave As String) As JsonArray
            Return DirectCast(cv(chiave), JsonArray)
        End Function

        <TestMethod>
        Public Sub RicopiaINtestazioneEIRecapiti()

            Dim cv As JsonObject = CvDalProfilo.Componi(ProfiloDiProva())
            Dim intestazione As JsonObject = DirectCast(cv("intestazione"), JsonObject)

            Assert.AreEqual("cv_base", Testo(cv, "tipo"), "è un CV base")
            Assert.AreEqual("Crash Bandicoot", Testo(intestazione, "nome"), "il nome")
            Assert.AreEqual("crash.bandicoot@esempio.it", Testo(intestazione, "email"), "l'email")
            Assert.AreEqual("333 0000000", Testo(intestazione, "telefono"), "il telefono")
            Assert.AreEqual("Wumpa Island", Testo(intestazione, "citta"), "la città")
            Assert.AreEqual("www.esempio.it/crash", Testo(intestazione, "link"), "il link")

        End Sub

        <TestMethod>
        Public Sub LaPatenteCompareSoloSeIlProfiloLaDichiara()

            Dim con As JsonObject = CvDalProfilo.Componi(ProfiloDiProva())
            Assert.AreEqual("B, C", Testo(DirectCast(con("intestazione"), JsonObject), "patente"),
                            "dichiarata: le categorie in fila")

            ' Le categorie restano scritte nel profilo — il pannello non le cancella —
            ' ma con la patente negata non devono ricomparire nel CV.
            Dim profilo As Profilo = ProfiloDiProva()
            profilo.Patente.Ha = "no"

            Dim senza As JsonObject = CvDalProfilo.Componi(profilo)
            Assert.AreEqual("", Testo(DirectCast(senza("intestazione"), JsonObject), "patente"),
                            "negata: niente patente nel CV, anche se le categorie sono rimaste lì")

        End Sub

        <TestMethod>
        Public Sub IlSommarioRestaVuotoPercheLoScriveLAi()

            Dim cv As JsonObject = CvDalProfilo.Componi(ProfiloDiProva())

            Assert.AreEqual("", Testo(cv, "sommario"),
                            "il sommario è prosa: senza modello non si scrive, e non si finge")

        End Sub

        <TestMethod>
        Public Sub LaDescrizioneEQuellaDelProfiloSenzaRiformularla()

            Dim cv As JsonObject = CvDalProfilo.Componi(ProfiloDiProva())
            Dim prima As JsonObject = DirectCast(Voci(cv, "esperienze_professionali")(0), JsonObject)

            Assert.AreEqual("Magazziniere", Testo(prima, "ruolo"), "il ruolo ricopiato")
            Assert.AreEqual("Depositi Wumpa", Testo(prima, "azienda"), "l'azienda ricopiata")
            Assert.AreEqual("2023-2025", Testo(prima, "durata"), "la durata ricopiata")
            Assert.AreEqual("Carico e scarico merci", Testo(prima, "descrizione"),
                            "la descrizione è «cosa facevo» com'è scritto, parola per parola")

        End Sub

        <TestMethod>
        Public Sub UnTirocinioNonSiTravesteDaImpiego()

            Dim cv As JsonObject = CvDalProfilo.Componi(ProfiloDiProva())
            Dim seconda As JsonObject = DirectCast(Voci(cv, "esperienze_professionali")(1), JsonObject)

            Assert.AreEqual("Tirocinio — Assistente di laboratorio", Testo(seconda, "ruolo"),
                            "il tipo dichiarato apre il ruolo, come chiede il prompt")

        End Sub

        <TestMethod>
        Public Sub UnTipoGiaScrittoNelRuoloNonSiRipete()

            Dim profilo As New Profilo
            profilo.EsperienzeFormali.Add(New EsperienzaFormale With {
                .Ruolo = "Tirocinio in officina", .Tipo = "tirocinio"})

            Dim cv As JsonObject = CvDalProfilo.Componi(profilo)
            Dim voce As JsonObject = DirectCast(Voci(cv, "esperienze_professionali")(0), JsonObject)

            Assert.AreEqual("Tirocinio in officina", Testo(voce, "ruolo"),
                            "«Tirocinio — Tirocinio in officina» sarebbe una balbuzie")

        End Sub

        <TestMethod>
        Public Sub LeEsperienzeInformaliNonDiventanoImpieghi()

            Dim cv As JsonObject = CvDalProfilo.Componi(ProfiloDiProva())
            Dim voce As JsonObject = DirectCast(Voci(cv, "altre_esperienze")(0), JsonObject)

            Assert.IsNull(voce("ruolo"), "niente ruolo: non era un impiego")
            Assert.IsNull(voce("azienda"), "niente azienda: non era un datore")
            Assert.AreEqual("estate 2021", Testo(voce, "quando"), "il quando ricopiato")
            Assert.Contains("Raccolta frutta", Testo(voce, "descrizione"), "quel che faceva")
            Assert.Contains("una cooperativa", Testo(voce, "descrizione"), "e con chi, accostato")

        End Sub

        <TestMethod>
        Public Sub CompetenzeEFormazioneArrivanoIntere()

            Dim cv As JsonObject = CvDalProfilo.Componi(ProfiloDiProva())

            Assert.HasCount(2, Voci(cv, "competenze"), "tutte le competenze")
            Assert.AreEqual("Uso del muletto", Voci(cv, "competenze")(0).GetValue(Of String)(),
                            "nell'ordine del profilo")

            Dim studio As JsonObject = DirectCast(Voci(cv, "formazione")(0), JsonObject)
            Assert.AreEqual("Diploma di perito", Testo(studio, "titolo"), "il titolo")
            Assert.AreEqual("ITIS Esempio", Testo(studio, "istituto"), "l'istituto")
            Assert.AreEqual("2021", Testo(studio, "anno"), "l'anno")

        End Sub

        <TestMethod>
        Public Sub UnProfiloVuotoDaUnCvVuotoENonUnErrore()

            For Each profilo As Profilo In New Profilo() {Nothing, New Profilo()}

                Dim cv As JsonObject = CvDalProfilo.Componi(profilo)

                Assert.AreEqual("cv_base", Testo(cv, "tipo"), "resta un CV base valido")
                Assert.IsEmpty(Voci(cv, "esperienze_professionali"), "nessuna esperienza")
                Assert.IsEmpty(Voci(cv, "competenze"), "nessuna competenza")
                Assert.IsEmpty(Voci(cv, "formazione"), "nessun titolo")

            Next

        End Sub

        <TestMethod>
        Public Sub NonCompareUnaParolaCheIlProfiloNonAbbia()

            ' La prova anti-invenzione, presa dal verso che conta: non «i campi sono
            ' arrivati» ma «non è arrivato nient'altro». Ogni testo del CV — tolte le
            ' due parole di servizio che la composizione aggiunge apposta, il tipo e
            ' l'etichetta del tirocinio — deve trovarsi scritto nel profilo.
            Dim profilo As Profilo = ProfiloDiProva()
            Dim scritto As String = profilo.VersoJson().ToJsonString()

            For Each valore As String In TuttiITesti(CvDalProfilo.Componi(profilo))

                If valore.Length = 0 Then Continue For
                If valore = "cv_base" Then Continue For

                ' Il tipo dichiarato apre il ruolo con l'iniziale maiuscola: nel profilo
                ' c'è la stessa parola, minuscola. Si confronta senza distinguere.
                ' Si spezza sui punti in cui la composizione accosta due fatti del
                ' profilo: il trattino del tipo davanti al ruolo, la parentesi del «con
                ' chi», la virgola fra le categorie di patente. Ognuno dei pezzi deve
                ' trovarsi scritto nel profilo per conto suo.
                For Each pezzo As String In valore.Split({" — ", " (", ")", ", "}, StringSplitOptions.RemoveEmptyEntries)
                    Assert.Contains(pezzo.Trim(), scritto, StringComparison.CurrentCultureIgnoreCase,
                                    $"«{pezzo.Trim()}» non è scritto da nessuna parte nel profilo")
                Next

            Next

        End Sub

        ''' <summary>Tutti i valori testuali dell'artefatto, a qualunque profondità.</summary>
        Private Shared Iterator Function TuttiITesti(nodo As JsonNode) As IEnumerable(Of String)

            If TypeOf nodo Is JsonValue Then
                Dim testo As String = Nothing
                If DirectCast(nodo, JsonValue).TryGetValue(Of String)(testo) Then Yield testo
                Return
            End If

            If TypeOf nodo Is JsonObject Then
                For Each coppia As KeyValuePair(Of String, JsonNode) In DirectCast(nodo, JsonObject)
                    For Each testo As String In TuttiITesti(coppia.Value)
                        Yield testo
                    Next
                Next
                Return
            End If

            If TypeOf nodo Is JsonArray Then
                For Each figlio As JsonNode In DirectCast(nodo, JsonArray)
                    For Each testo As String In TuttiITesti(figlio)
                        Yield testo
                    Next
                Next
            End If

        End Function

    End Class

End Namespace
