Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Motore

Namespace Documenti

    ''' <summary>
    ''' Da un CV o da una lettera in JSON alla <see cref="PaginaDocumento"/> che le
    ''' stampanti sanno stampare (cap. 05.3, cap. 05.4). È qui — una volta sola — che si
    ''' decide <b>che cosa</b> entra nel documento, in che ordine e con quali etichette.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Legge in modo tollerante, e non per pigrizia.</b> Il JSON arriva da un
    ''' modello: un campo può mancare, una lista può essere vuota, un anno può arrivare
    ''' come numero invece che come stringa. Nessuna di queste cose è un buon motivo per
    ''' non stampare il CV — l'utente ha appena aspettato la generazione — quindi ciò che
    ''' non c'è si salta e ciò che c'è si stampa. Le sezioni vuote spariscono <b>con il
    ''' loro titolo</b>: un «Competenze» seguito dal nulla è peggio che non averlo.</para>
    ''' <para><b>Le etichette sono quelle del prototipo</b> («Esperienze professionali»,
    ''' «Altre esperienze», «Competenze», «Formazione», i recapiti uniti da « · », la
    ''' patente su una riga sua): lì quel CV l'utente l'ha già letto a schermo, e non c'è
    ''' motivo perché su carta si chiami in un altro modo. <b>Da T7</b> quelle parole
    ''' seguono la <b>lingua del documento</b> (v. <see cref="Etichette"/>): le chiavi del
    ''' JSON restano italiane perché le legge l'applicazione, le parole stampate no perché
    ''' le legge chi riceve il CV.</para>
    ''' <para>Il <b>sommario non ha titolo di sezione</b>: sta sotto i recapiti come
    ''' ritratto d'apertura, che è la forma in cui il prototipo lo mostra e quella che il
    ''' cap. 05.4 descrive.</para>
    ''' </remarks>
    Public Class Impaginazione

        ''' <summary>I titoli delle sezioni del CV, come li legge chi riceve il documento.</summary>
        Public Const SezioneEsperienze As String = "Esperienze professionali"
        Public Const SezioneAltreEsperienze As String = "Altre esperienze"
        Public Const SezioneCompetenze As String = "Competenze"
        Public Const SezioneFormazione As String = "Formazione"

        ''' <summary>Come si presenta la patente nell'intestazione.</summary>
        Public Const EtichettaPatente As String = "Patente: "

        ''' <summary>Il separatore fra i pezzi di una riga di recapiti o di dettaglio.</summary>
        Public Const Separatore As String = " · "

        ''' <summary>I titoli dei due documenti (proprietà del file, non testo stampato).</summary>
        Public Const TitoloCv As String = "CV"
        Public Const TitoloLettera As String = "Lettera di presentazione"

        ''' <summary>
        ''' Le parole che il documento stampa, nella lingua del documento (cap. 10.1).
        ''' </summary>
        ''' <remarks>
        ''' <para>Le <b>chiavi del JSON restano italiane</b> — quelle le legge
        ''' l'applicazione, non chi riceve il CV — ma le parole stampate no: un CV generato
        ''' in inglese con «Esperienze professionali» scritto sopra il testo inglese
        ''' sarebbe un documento a metà. La lingua è una proprietà della candidatura, e
        ''' arriva fin qui.</para>
        ''' <para>Le etichette italiane restano <b>quelle del prototipo</b>, per la ragione
        ''' di sempre: a schermo l'utente quel CV l'ha già letto così.</para>
        ''' </remarks>
        Public Class Etichette

            ''' <summary>I titoli delle sezioni del CV.</summary>
            Public Property Esperienze As String
            Public Property AltreEsperienze As String
            Public Property Competenze As String
            Public Property Formazione As String

            ''' <summary>Come si presenta la patente nell'intestazione.</summary>
            Public Property Patente As String

            ''' <summary>I titoli dei due documenti.</summary>
            Public Property TitoloCv As String
            Public Property TitoloLettera As String

            ''' <summary>
            ''' Le etichette della lingua chiesta.
            ''' </summary>
            ''' <remarks>
            ''' A dire in quale delle due si finisce non è questa funzione ma
            ''' <see cref="LinguaDocumenti"/>, che è il posto unico di quella regola. La
            ''' prima versione la ripeteva qui, e le due copie divergevano già: un annuncio
            ''' in tedesco andava in inglese per il pool e in italiano per le etichette,
            ''' cioè un documento inglese coi titoli italiani. Lo ha trovato un collaudo,
            ''' non l'occhio.
            ''' </remarks>
            Public Shared Function PerLingua(lingua As String) As Etichette

                If LinguaDocumenti.PerDocumenti(lingua) = LinguaDocumenti.Inglese Then
                    Return Inglesi()
                End If

                Return Italiane()

            End Function

            ''' <summary>Le etichette di casa, che sono le costanti di sempre.</summary>
            Public Shared Function Italiane() As Etichette

                Return New Etichette With {
                    .Esperienze = SezioneEsperienze,
                    .AltreEsperienze = SezioneAltreEsperienze,
                    .Competenze = SezioneCompetenze,
                    .Formazione = SezioneFormazione,
                    .Patente = EtichettaPatente,
                    .TitoloCv = Impaginazione.TitoloCv,
                    .TitoloLettera = Impaginazione.TitoloLettera}

            End Function

            ''' <summary>
            ''' Le etichette inglesi. Sono quelle di un CV britannico — «CV», non
            ''' «Resume», e «Driving licence» — perché è a un mercato europeo che questi
            ''' documenti si mandano.
            ''' </summary>
            Public Shared Function Inglesi() As Etichette

                Return New Etichette With {
                    .Esperienze = "Work experience",
                    .AltreEsperienze = "Other experience",
                    .Competenze = "Skills",
                    .Formazione = "Education",
                    .Patente = "Driving licence: ",
                    .TitoloCv = "CV",
                    .TitoloLettera = "Cover letter"}

            End Function

        End Class

        ''' <summary>
        ''' Impagina un CV — base o mirato: sono lo stesso schema, e lo stesso documento.
        ''' </summary>
        ''' <param name="cv">Il CV JSON come esce dal generatore (cap. 04.3).</param>
        ''' <param name="lingua">
        ''' La lingua del documento, che decide le etichette stampate (cap. 10.1). Chi non
        ''' la dichiara ottiene l'italiano, che è la lingua di casa.
        ''' </param>
        ''' <param name="tolte">
        ''' Le voci che l'utente ha tolto da questo documento (R6, cap. 08.4). Passa di qui
        ''' perché di qui passano tutti — anteprima, DOCX, PDF e HTML: un taglio deciso in
        ''' un posto solo non può mostrare nel PDF quel che l'anteprima non fa vedere.
        ''' </param>
        Public Shared Function PaginaCv(cv As JsonNode,
                                        Optional lingua As String = "it",
                                        Optional tolte As Dati.VociTolte = Nothing) As PaginaDocumento

            If cv Is Nothing Then Throw New ArgumentNullException(NameOf(cv))

            Dim etichette As Etichette = Etichette.PerLingua(lingua)
            Dim radice As JsonObject = TryCast(VociDelCv.ComeSiVede(cv, tolte), JsonObject)
            Dim pagina As New PaginaDocumento With {.Titolo = etichette.TitoloCv}
            If radice Is Nothing Then Return pagina

            Dim intestazione As JsonObject = Oggetto(radice, "intestazione")
            Dim nome As String = Testo(intestazione, "nome")

            If nome.Length > 0 Then
                pagina.Titolo = $"{etichette.TitoloCv} — {nome}"
                pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Nome, .Testo = nome})
            End If

            Dim recapiti As List(Of String) = Pieni(
                {Testo(intestazione, "email"), Testo(intestazione, "telefono"),
                 Testo(intestazione, "citta"), Testo(intestazione, "link")})

            If recapiti.Count > 0 Then
                pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Recapiti, .Voci = recapiti})
            End If

            ' La patente sta su una riga sua, come nel prototipo: è un dato che chi legge
            ' un CV cerca da solo, e in mezzo agli altri recapiti si perderebbe.
            Dim patente As String = Testo(intestazione, "patente")
            If patente.Length > 0 Then
                pagina.Blocchi.Add(New Blocco With {
                    .Genere = GenereBlocco.Recapiti,
                    .Voci = New List(Of String) From {etichette.Patente & patente}})
            End If

            For Each capoverso As String In Paragrafi(Grezzo(radice, "sommario"))
                pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Paragrafo, .Testo = capoverso})
            Next

            Sezione(pagina, etichette.Esperienze,
                    Voci(radice, "esperienze_professionali", AddressOf VoceEsperienza))

            Sezione(pagina, etichette.AltreEsperienze,
                    Voci(radice, "altre_esperienze", AddressOf VoceAltraEsperienza))

            Dim competenze As List(Of String) = Testi(radice, "competenze")
            If competenze.Count > 0 Then
                pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Sezione, .Testo = etichette.Competenze})
                pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Elenco, .Voci = competenze})
            End If

            Sezione(pagina, etichette.Formazione,
                    Voci(radice, "formazione", AddressOf VoceFormazione))

            Return pagina

        End Function

        ''' <summary>
        ''' Impagina una lettera di presentazione: carta intestata minimale col nome e i
        ''' recapiti, i tre blocchi di testo e la firma (cap. 05.4).
        ''' </summary>
        ''' <param name="lettera">La lettera JSON come esce dal generatore.</param>
        ''' <param name="lingua">La lingua del documento (v. <see cref="PaginaCv"/>).</param>
        Public Shared Function PaginaLettera(lettera As JsonNode,
                                             Optional lingua As String = "it") As PaginaDocumento

            If lettera Is Nothing Then Throw New ArgumentNullException(NameOf(lettera))

            Dim etichette As Etichette = Etichette.PerLingua(lingua)
            Dim radice As JsonObject = TryCast(lettera, JsonObject)
            Dim pagina As New PaginaDocumento With {.Titolo = etichette.TitoloLettera}
            If radice Is Nothing Then Return pagina

            ' Il prototipo accetta una firma scritta come semplice stringa oltre che come
            ' oggetto: capita, e una lettera senza firma per questo sarebbe un peccato.
            Dim firma As JsonObject = Oggetto(radice, "firma")
            Dim nome As String = If(firma Is Nothing, Testo(radice, "firma"), Testo(firma, "nome"))

            If nome.Length > 0 Then
                pagina.Titolo = $"{etichette.TitoloLettera} — {nome}"
                pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Nome, .Testo = nome})
            End If

            Dim recapiti As List(Of String) = Pieni({Testo(firma, "email"), Testo(firma, "telefono")})
            If recapiti.Count > 0 Then
                pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Recapiti, .Voci = recapiti})
            End If

            For Each campo As String In {"apertura", "corpo", "chiusura"}
                For Each capoverso As String In Paragrafi(Grezzo(radice, campo))
                    pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Paragrafo, .Testo = capoverso})
                Next
            Next

            If nome.Length > 0 Then
                pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Firma, .Testo = nome})
            End If

            Return pagina

        End Function

        ''' <summary>
        ''' Aggiunge una sezione alla pagina, ma <b>solo se ha qualcosa dentro</b>: è la
        ''' regola che tiene fuori dal documento i titoli senza contenuto.
        ''' </summary>
        Private Shared Sub Sezione(pagina As PaginaDocumento, titolo As String, voci As List(Of Blocco))

            If voci.Count = 0 Then Return

            pagina.Blocchi.Add(New Blocco With {.Genere = GenereBlocco.Sezione, .Testo = titolo})
            pagina.Blocchi.AddRange(voci)

        End Sub

        ''' <summary>
        ''' Le voci di una lista del CV, tradotte una a una dalla funzione data. Le voci
        ''' che non hanno niente da dire — tutti i campi vuoti — restano fuori.
        ''' </summary>
        Private Shared Function Voci(radice As JsonObject, campo As String,
                                     comeBlocco As Func(Of JsonObject, Blocco)) As List(Of Blocco)

            Dim costruite As New List(Of Blocco)

            For Each nodo As JsonNode In Lista(radice, campo)

                Dim voce As JsonObject = TryCast(nodo, JsonObject)
                If voce Is Nothing Then Continue For

                Dim blocco As Blocco = comeBlocco(voce)
                If blocco.Testo.Length + blocco.Dettaglio.Length + blocco.Descrizione.Length > 0 Then
                    costruite.Add(blocco)
                End If

            Next

            Return costruite

        End Function

        ''' <summary>Un'esperienza formale: il ruolo intitola, azienda e durata collocano.</summary>
        Private Shared Function VoceEsperienza(voce As JsonObject) As Blocco

            Return New Blocco With {
                .Genere = GenereBlocco.Voce,
                .Testo = Testo(voce, "ruolo"),
                .Dettaglio = Unisci({Testo(voce, "azienda"), Testo(voce, "durata")}),
                .Descrizione = Testo(voce, "descrizione")}

        End Function

        ''' <summary>
        ''' Un'esperienza informale. Qui a intitolare è il <b>quando</b>, e non per
        ''' capriccio: il prompt vieta di dare a queste voci un ruolo o un'azienda
        ''' (non sono impieghi), quindi l'unica cosa breve che possa fare da testatina è
        ''' il periodo. La descrizione resta descrizione.
        ''' </summary>
        Private Shared Function VoceAltraEsperienza(voce As JsonObject) As Blocco

            Return New Blocco With {
                .Genere = GenereBlocco.Voce,
                .Testo = Testo(voce, "quando"),
                .Descrizione = Testo(voce, "descrizione")}

        End Function

        ''' <summary>Un titolo di studio: il titolo intitola, istituto e anno collocano.</summary>
        Private Shared Function VoceFormazione(voce As JsonObject) As Blocco

            Return New Blocco With {
                .Genere = GenereBlocco.Voce,
                .Testo = Testo(voce, "titolo"),
                .Dettaglio = Unisci({Testo(voce, "istituto"), Testo(voce, "anno")})}

        End Function

        ''' <summary>I pezzi non vuoti, uniti dal separatore.</summary>
        Private Shared Function Unisci(pezzi As IEnumerable(Of String)) As String
            Return String.Join(Separatore, Pieni(pezzi))
        End Function

        ''' <summary>Solo i pezzi che hanno qualcosa scritto dentro.</summary>
        Private Shared Function Pieni(pezzi As IEnumerable(Of String)) As List(Of String)
            Return pezzi.Where(Function(p) Not String.IsNullOrEmpty(p)).ToList()
        End Function

        ''' <summary>
        ''' Un testo lungo diviso in capoversi: si spezza su ogni a capo, perché nessun
        ''' blocco deve contenerne (v. <see cref="PaginaDocumento"/>). Le righe vuote
        ''' spariscono da sé.
        ''' </summary>
        Private Shared Function Paragrafi(grezzo As String) As List(Of String)

            If String.IsNullOrWhiteSpace(grezzo) Then Return New List(Of String)

            Return grezzo.Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries).
                Select(AddressOf Pulito).
                Where(Function(p) p.Length > 0).
                ToList()

        End Function

        ''' <summary>Le stringhe di una lista del CV (le competenze), già ripulite.</summary>
        Private Shared Function Testi(radice As JsonObject, campo As String) As List(Of String)

            Return Lista(radice, campo).
                Select(AddressOf Valore).
                Select(AddressOf Pulito).
                Where(Function(v) v.Length > 0).
                ToList()

        End Function

        ''' <summary>Il valore di un campo, ripulito e pronto da stampare.</summary>
        Private Shared Function Testo(oggetto As JsonObject, campo As String) As String
            Return Pulito(Grezzo(oggetto, campo))
        End Function

        ''' <summary>Il valore di un campo così com'è, a capo compresi.</summary>
        Private Shared Function Grezzo(oggetto As JsonObject, campo As String) As String

            If oggetto Is Nothing Then Return String.Empty

            ' Il nome non può essere «valore»: in VB i nomi non distinguono le
            ' maiuscole, e una locale così coprirebbe la funzione «Valore» qui sotto.
            Dim trovato As JsonNode = Nothing
            If Not oggetto.TryGetPropertyValue(campo, trovato) Then Return String.Empty

            Return Valore(trovato)

        End Function

        ''' <summary>
        ''' Un valore JSON come testo. I numeri si accettano <b>di proposito</b>: lo
        ''' schema chiede stringhe, ma un anno che arriva come <c>2019</c> invece che come
        ''' <c>"2019"</c> è un dato buono scritto in un altro modo, non un motivo per
        ''' lasciare un buco nel CV. Tutto il resto — oggetti, liste, <c>true</c> — non è
        ''' testo e resta fuori.
        ''' </summary>
        Private Shared Function Valore(nodo As JsonNode) As String

            If nodo Is Nothing Then Return String.Empty

            Select Case nodo.GetValueKind()
                Case JsonValueKind.String : Return nodo.GetValue(Of String)()
                Case JsonValueKind.Number : Return nodo.ToJsonString()
                Case Else : Return String.Empty
            End Select

        End Function

        ''' <summary>
        ''' Il testo come va in un blocco: spazi e a capo ridotti a un singolo spazio,
        ''' niente spazi ai bordi.
        ''' </summary>
        Private Shared Function Pulito(testo As String) As String

            If String.IsNullOrWhiteSpace(testo) Then Return String.Empty

            ' Split senza separatori spezza su tutti gli spazi bianchi, a capo inclusi.
            Return String.Join(" ", testo.Split(CType(Nothing, Char()),
                                                StringSplitOptions.RemoveEmptyEntries))

        End Function

        ''' <summary>Un campo che è un oggetto; <c>Nothing</c> se manca o è altro.</summary>
        Private Shared Function Oggetto(radice As JsonObject, campo As String) As JsonObject

            If radice Is Nothing Then Return Nothing

            Dim trovato As JsonNode = Nothing
            If Not radice.TryGetPropertyValue(campo, trovato) Then Return Nothing

            Return TryCast(trovato, JsonObject)

        End Function

        ''' <summary>Un campo che è una lista; una lista vuota se manca o è altro.</summary>
        Private Shared Function Lista(radice As JsonObject, campo As String) As JsonArray

            If radice Is Nothing Then Return New JsonArray()

            Dim trovato As JsonNode = Nothing
            If Not radice.TryGetPropertyValue(campo, trovato) Then Return New JsonArray()

            Return If(TryCast(trovato, JsonArray), New JsonArray())

        End Function

    End Class

End Namespace
