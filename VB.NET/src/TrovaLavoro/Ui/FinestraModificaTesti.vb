Imports System.Linq
Imports System.Text.Json.Nodes
Imports System.Windows.Forms
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore

''' <summary>Cosa l'utente ha deciso davanti ai testi che stava riscrivendo.</summary>
Public Enum EsitoModifica

    ''' <summary>Ha chiuso senza confermare: i documenti restano come li aveva trovati.</summary>
    Annullato

    ''' <summary>Va bene così: le riscritture entrano nei documenti.</summary>
    Confermato

End Enum

''' <summary>
''' Un documento aperto alla modifica a mano, con i testi da cui la rifinitura era
''' partita.
''' </summary>
Public Class DocumentoDaRiscrivere

    ''' <summary>Il documento: un CV o una lettera.</summary>
    Public Property Documento As JsonNode

    ''' <summary>
    ''' Quale dei due è (R7): il CV — mirato o base — oppure la lettera. Serve a chi
    ''' salva, non a questa finestra: un campo riscritto nel CV può disallineare la
    ''' lettera che ne discende, e per accorgersene bisogna sapere dove è stato scritto.
    ''' </summary>
    Public Property Ruolo As RuoloDocumento

    ''' <summary>
    ''' Le voci già lasciate fuori da questo documento (R6): la finestra le mostra
    ''' nell'elenco di destra, così chi riapre ritrova il taglio che aveva scelto invece
    ''' di doverlo rifare.
    ''' </summary>
    Public Property Tolte As VociTolte

    ''' <summary>
    ''' I campi di prosa che in questo documento risultano <b>già</b> riscritti a mano
    ''' (R7): quelli annotati nei file, che sopravvivono alla sessione.
    ''' </summary>
    ''' <remarks>
    ''' Servono al segno ✎. Senza, la finestra conoscerebbe solo le riscritture di questo
    ''' giro e di un testo scritto dall'utente ieri direbbe che non l'ha mai toccato —
    ''' mentre l'avviso di «Rigenera», che i file li legge, dice il contrario.
    ''' </remarks>
    Public Property Riscritte As RiscrittureAMano

End Class

''' <summary>Un campo che l'utente ha riscritto davvero, e in quale documento (R7).</summary>
''' <remarks>
''' È quello che la finestra consegna a chi l'ha aperta: fino a T9d bastava <b>quanti</b>
''' campi fossero cambiati, perché serviva solo a scrivere «ho salvato i 2 testi che hai
''' riscritto». Da R7 il pannello deve anche annotare <b>quali</b> e <b>dove</b>, perché
''' quell'annotazione sopravvive alla sessione e finisce nei file (v.
''' <see cref="RiscrittureAMano"/>).
''' </remarks>
Public Class RiscritturaFatta

    ''' <summary>In quale documento.</summary>
    Public Property Ruolo As RuoloDocumento

    ''' <summary>Quale campo: <c>sommario</c>, <c>esperienza.1</c>, <c>corpo</c>.</summary>
    Public Property Id As String

End Class

''' <summary>
''' La <b>modifica a mano</b> dei testi di P6 (T9d, cap. 08.4): l'ultima delle tre cose
''' che il capitolo prometteva davanti al prima/dopo — accettare, riscrivere, tornare
''' alla versione non rifinita.
''' </summary>
''' <remarks>
''' <para><b>Perché una finestra e non le caselle di P6.</b> Le tre caselle del pannello
''' non mostrano il documento: mostrano la <b>pagina di blocchi</b> con cui finirà nel
''' DOCX e nel PDF (cap. 05.3), etichette di sezione comprese, e in coda il prima/dopo,
''' che documento non è. Renderle scrivibili vorrebbe dire ricostruire il JSON da quel
''' testo — un mestiere che nessuna delle tre stampanti fa, e che sbaglierebbe proprio
''' dove l'utente si fida di più.</para>
''' <para><b>Solo la prosa scritta dall'AI</b>: sommario, descrizioni delle esperienze,
''' corpo della lettera. Non i fatti — nomi, aziende, date, competenze, titoli — che
''' vengono dal profilo: cambiarli qui li farebbe divergere in silenzio dal profilo che
''' li custodisce, e tornerebbero com'erano alla prima rigenerazione. Quali campi siano
''' prosa non lo decide questa finestra: lo chiede a <see cref="Rifinitura"/>, che è
''' l'unico posto che lo sa.</para>
''' <para><b>Qui non si chiama l'AI e non si scrive su disco</b>, come nella finestra
''' degli appunti: si raccoglie una decisione. I documenti si toccano solo con
''' <see cref="Applica"/>, cioè dopo il «Salva» — annullando, quello che si era scritto
''' muore con la finestra. A salvare è il pannello.</para>
''' <para><b>L'anti-invenzione non c'entra.</b> Quel vincolo tiene la <b>macchina</b>
''' dentro i fatti dichiarati; qui a scrivere è l'utente, che dei suoi fatti è il
''' proprietario. Il testo che esce di qui non passa da nessun prompt: entra nel
''' documento com'è stato scritto.</para>
''' </remarks>
Public Class FinestraModificaTesti

    ''' <summary>Il segno che marca, nell'elenco, i campi scritti a mano dall'utente.</summary>
    Private Const SegnoRiscritto As String = "✎"

    ''' <summary>Un campo di prosa aperto alla riscrittura, con tutto ciò che lo riguarda.</summary>
    Private Class Voce

        ''' <summary>Il documento a cui appartiene: è lì che il testo va rimesso.</summary>
        Public Property Documento As JsonNode

        ''' <summary>Quale documento è, per chi dovrà annotare la riscrittura (R7).</summary>
        Public Property Ruolo As RuoloDocumento

        Public Property Id As String
        Public Property Etichetta As String

        ''' <summary>Il testo com'era all'apertura della finestra.</summary>
        Public Property Originale As String

        ''' <summary>Il testo com'è adesso nella casella.</summary>
        Public Property Testo As String

        ''' <summary>
        ''' Se risultava riscritto a mano <b>prima</b> che questa finestra si aprisse
        ''' (R7): è quel che sta nei file, non quel che si è fatto adesso.
        ''' </summary>
        Public Property GiaRiscritto As Boolean

        ''' <summary>
        ''' Se in questo giro l'utente l'ha cambiato. È questo — e non
        ''' <see cref="RiscrittoAMano"/> — a dire cosa va rimesso nel documento: un testo
        ''' riscritto in un giro precedente e non toccato oggi nel file c'è già.
        ''' </summary>
        Public ReadOnly Property CambiatoInQuestoGiro As Boolean
            Get
                Return Not String.Equals(Testo, Originale, StringComparison.Ordinal)
            End Get
        End Property

        ''' <summary>
        ''' Se questo testo l'ha scritto l'utente: adesso, oppure in un giro precedente.
        ''' </summary>
        ''' <remarks>
        ''' È la domanda del segno ✎, ed è la stessa a cui risponde l'avviso di
        ''' «Rigenera» leggendo i file: due risposte diverse alla stessa domanda facevano
        ''' sparire il segno alla riapertura della finestra, mentre l'avviso continuava —
        ''' giustamente — a promettere che quel testo si sarebbe perso.
        ''' </remarks>
        Public ReadOnly Property RiscrittoAMano As Boolean
            Get
                Return CambiatoInQuestoGiro OrElse GiaRiscritto
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Una riga dell'elenco. Le due cose che si fanno qui dentro — riscrivere un testo e
    ''' togliere una voce dal documento — non stanno sulle stesse righe: il sommario si
    ''' riscrive ma non si toglie (un CV senza sommario non è un CV con una voce in meno,
    ''' è un CV rotto); una competenza si toglie ma non si riscrive, perché è un fatto e i
    ''' fatti stanno nel profilo; un'esperienza fa tutt'e due. Perciò una riga porta quel
    ''' che ha: la prosa, l'impronta, o entrambe.
    ''' </summary>
    Private Class RigaElenco

        ''' <summary>Il campo di prosa, e dov'è in <c>_voci</c>; -1 se questa riga non ne ha.</summary>
        Public Property IndiceProsa As Integer = -1

        ''' <summary>Chi è la voce, se si può togliere; <c>Nothing</c> se non si può.</summary>
        Public Property Impronta As String

        Public Property Etichetta As String

        ''' <summary>Cosa dice: il testo di prosa, o il riepilogo dei suoi fatti.</summary>
        Public Property Riepilogo As String

    End Class

    Private ReadOnly _voci As New List(Of Voce)

    ''' <summary>Le righe dei due elenchi, nell'ordine in cui si leggono nel documento.</summary>
    Private ReadOnly _righe As New List(Of RigaElenco)

    ''' <summary>Le impronte delle voci che in questo momento sono lasciate fuori.</summary>
    Private ReadOnly _fuori As New HashSet(Of String)(StringComparer.Ordinal)

    Private ReadOnly _suggerimenti As New ToolTip()

    ''' <summary>Quante volte si sta riempiendo un controllo: gli eventi di riflesso non contano.</summary>
    Private _riempimenti As Integer

    ''' <summary>Cosa ha deciso l'utente; <see cref="EsitoModifica.Annullato"/> finché non decide.</summary>
    Public ReadOnly Property Esito As EsitoModifica = EsitoModifica.Annullato

    ''' <summary>
    ''' Prepara la finestra sui documenti da riscrivere. È pubblica perché il banco la
    ''' costruisce e la interroga senza mostrarla.
    ''' </summary>
    ''' <param name="documenti">
    ''' I documenti in mostra nel pannello: per una candidatura sono due — il 🎯 CV mirato
    ''' e la ✉️ lettera — per il 📄 CV base uno solo. Quelli senza campi di prosa non
    ''' portano righe, e non è un errore: è un documento che non ha niente da riscrivere.
    ''' </param>
    Public Sub New(documenti As IEnumerable(Of DocumentoDaRiscrivere))

        InitializeComponent()

        AddHandler Me.Disposed, Sub() _suggerimenti.Dispose()

        If documenti Is Nothing Then Throw New ArgumentNullException(NameOf(documenti))

        RaccogliLaProsa(documenti)
        ComponiLeRighe(documenti)

        lblSpiegazione.Text =
            "Qui riscrivi i testi che ho scritto io — il sommario, le descrizioni delle " &
            "esperienze, il corpo della lettera — e scegli quali voci mettere in questo documento." & vbLf &
            "I fatti vengono dal tuo profilo e si cambiano di là: quel che lasci fuori esce da " &
            "questo documento soltanto, e resta nel profilo per le altre candidature."

        Vesti()
        MostraICampi()
        MostraIlCampoScelto()

        ' Esc annulla; nessun bottone appeso a Invio, perché si scrive nella casella del
        ' testo e un Invio di passaggio chiuderebbe la finestra a metà lavoro.
        CancelButton = btnAnnulla
        AcceptButton = Nothing

    End Sub

    ''' <summary>
    ''' Mostra la finestra e mette nei documenti quello che l'utente ha riscritto.
    ''' </summary>
    ''' <returns>
    ''' I campi riscritti davvero, con il documento in cui stanno: la lista è <b>vuota</b>
    ''' se ha annullato, e vuota anche se ha confermato senza cambiare niente — che per chi
    ''' salva è la stessa cosa.
    ''' </returns>
    Public Shared Function Chiedi(proprietario As IWin32Window,
                                  documenti As IEnumerable(Of DocumentoDaRiscrivere),
                                  <Runtime.InteropServices.Out> ByRef fuori As List(Of String)) _
                                  As List(Of RiscritturaFatta)

        fuori = Nothing

        Using finestra As New FinestraModificaTesti(documenti)

            finestra.ShowDialog(proprietario)
            If finestra.Esito <> EsitoModifica.Confermato Then Return New List(Of RiscritturaFatta)

            ' Annullando non torna niente, né i testi né il taglio: è il senso di
            ' «Annulla», e vale per tutti e due i lavori che si fanno lì dentro (R6).
            fuori = finestra.VociFuori()

            Return finestra.Applica()

        End Using

    End Function

    ''' <summary>Quanti campi di prosa ci sono da riscrivere.</summary>
    Public ReadOnly Property Quanti As Integer
        Get
            Return _voci.Count
        End Get
    End Property

    ''' <summary>Il nome del campo in quella riga, o vuoto se quella riga non c'è.</summary>
    Public Function Etichetta(indice As Integer) As String

        If Not CE(indice) Then Return String.Empty

        Return _voci(indice).Etichetta

    End Function

    ''' <summary>Il testo del campo in quella riga com'è adesso nella finestra.</summary>
    Public Function Testo(indice As Integer) As String

        If Not CE(indice) Then Return String.Empty

        Return _voci(indice).Testo

    End Function

    ''' <summary>
    ''' Riscrive il testo di un campo.
    ''' </summary>
    ''' <remarks>
    ''' Sta in un metodo pubblico, staccato dalla casella, perché è l'unico modo di
    ''' collaudarlo: su una finestra mai mostrata i controlli non si lasciano pilotare
    ''' (v. <see cref="FinestraAppunti.RiscriviLAppunto"/>).
    ''' </remarks>
    ''' <returns>Falso se quella riga non c'è o il testo è vuoto.</returns>
    Public Function Riscrivi(indice As Integer, testo As String) As Boolean

        If Not CE(indice) Then Return False
        If String.IsNullOrWhiteSpace(testo) Then Return False

        _voci(indice).Testo = testo
        AggiornaLaRiga(indice)

        Return True

    End Function

    ''' <summary>
    ''' Mette nei documenti i campi riscritti.
    ''' </summary>
    ''' <remarks>
    ''' I campi lasciati stare non si riscrivono affatto: rimettere a posto un testo
    ''' identico non cambierebbe niente nel file, ma farebbe contare come «modificato» un
    ''' documento che nessuno ha toccato.
    ''' </remarks>
    ''' <returns>I campi finiti davvero nei documenti, con il documento di ciascuno.</returns>
    Public Function Applica() As List(Of RiscritturaFatta)

        Dim scritti As New List(Of RiscritturaFatta)

        For Each voce As Voce In _voci

            If Not voce.CambiatoInQuestoGiro Then Continue For
            If Rifinitura.Riscrivi(voce.Documento, voce.Id, voce.Testo) Then
                scritti.Add(New RiscritturaFatta With {.Ruolo = voce.Ruolo, .Id = voce.Id})
            End If

        Next

        Return scritti

    End Function

    ''' <summary>
    ''' Mette in fila le righe dei due elenchi: per ogni documento, prima la sua prosa —
    ''' nell'ordine in cui la dà <c>Rifinitura</c> — e poi le voci che quella prosa non
    ''' copre, cioè competenze e titoli di studio, che di descrizione non ne hanno.
    ''' </summary>
    ''' <remarks>
    ''' La riga di un'esperienza è <b>una sola</b>: la stessa si riscrive e si toglie.
    ''' Farne due — «Esperienza 2» fra i testi e «Esperienza 2» fra le voci — lascerebbe a
    ''' chi legge il compito di capire che parlano della stessa cosa.
    ''' </remarks>
    Private Sub ComponiLeRighe(documenti As IEnumerable(Of DocumentoDaRiscrivere))

        For Each documento As DocumentoDaRiscrivere In documenti

            If documento Is Nothing OrElse documento.Documento Is Nothing Then Continue For

            Dim voci As List(Of VoceDelCv) = VociDelCv.Elenca(documento.Documento)

            ' Quel che era già fuori resta fuori: chi riapre la finestra ritrova il taglio
            ' che aveva scelto, invece di doverlo rifare da capo.
            If documento.Tolte IsNot Nothing Then
                For Each impronta As String In documento.Tolte.Impronte
                    _fuori.Add(impronta)
                Next
            End If

            Dim gia As New HashSet(Of String)(StringComparer.Ordinal)

            For indice As Integer = 0 To _voci.Count - 1

                If Not ReferenceEquals(_voci(indice).Documento, documento.Documento) Then Continue For

                Dim accoppiata As VoceDelCv = VoceDellaProsa(voci, _voci(indice).Id)
                If accoppiata IsNot Nothing Then gia.Add(accoppiata.Impronta)

                _righe.Add(New RigaElenco With {
                    .IndiceProsa = indice,
                    .Impronta = accoppiata?.Impronta,
                    .Etichetta = _voci(indice).Etichetta,
                    .Riepilogo = _voci(indice).Testo})

            Next

            For Each voce As VoceDelCv In voci

                If gia.Contains(voce.Impronta) Then Continue For

                _righe.Add(New RigaElenco With {
                    .Impronta = voce.Impronta,
                    .Etichetta = voce.Etichetta,
                    .Riepilogo = voce.Riepilogo})

            Next

        Next

    End Sub

    ''' <summary>
    ''' La voce a cui appartiene un campo di prosa. I due mondi si nominano diversamente —
    ''' <c>esperienza.1</c> il testo, l'impronta la voce — ma dentro un documento la
    ''' seconda esperienza è una sola, e il numero basta a ritrovarla.
    ''' </summary>
    Private Shared Function VoceDellaProsa(voci As List(Of VoceDelCv), idProsa As String) As VoceDelCv

        If String.IsNullOrEmpty(idProsa) Then Return Nothing

        Dim sezione As String

        If idProsa.StartsWith("esperienza.", StringComparison.Ordinal) Then
            sezione = "esperienze_professionali"
        ElseIf idProsa.StartsWith("altra.", StringComparison.Ordinal) Then
            sezione = "altre_esperienze"
        Else
            ' Il sommario e il corpo della lettera non sono voci: si riscrivono e basta.
            Return Nothing
        End If

        Dim numero As Integer
        If Not Integer.TryParse(idProsa.Substring(idProsa.IndexOf("."c) + 1), numero) Then Return Nothing

        Return voci.FirstOrDefault(Function(v) v.Sezione = sezione AndAlso v.Indice = numero)

    End Function

    ''' <summary>Le voci che l'utente ha lasciato fuori dal documento, come impronte.</summary>
    ''' <remarks>
    ''' È quel che la finestra consegna a chi l'ha aperta, insieme alle riscritture: chi
    ''' salva le mette in <see cref="VociTolte"/>, che vive dove vive il documento.
    ''' </remarks>
    Public Function VociFuori() As List(Of String)

        Return _fuori.ToList()

    End Function

    ''' <summary>Mette in fila i campi di prosa di tutti i documenti.</summary>
    Private Sub RaccogliLaProsa(documenti As IEnumerable(Of DocumentoDaRiscrivere))

        For Each documento As DocumentoDaRiscrivere In documenti

            If documento Is Nothing OrElse documento.Documento Is Nothing Then Continue For

            For Each campo As Rifinitura.CampoDiProsa In Rifinitura.CampiDiProsa(documento.Documento)

                _voci.Add(New Voce With {
                    .Documento = documento.Documento,
                    .Ruolo = documento.Ruolo,
                    .Id = campo.Id,
                    .Etichetta = campo.Etichetta,
                    .Originale = campo.Testo,
                    .Testo = campo.Testo,
                    .GiaRiscritto = documento.Riscritte IsNot Nothing AndAlso
                                    documento.Riscritte.Contiene(campo.Id)})

            Next

        Next

    End Sub

    ''' <summary>I colori e i font, tutti da <see cref="StileApp"/> (cap. 03.2).</summary>
    Private Sub Vesti()

        BackColor = StileApp.SfondoContenuto
        Font = StileApp.FontTesto

        lblTitolo.Font = StileApp.FontTitoloPannello
        lblTitolo.ForeColor = StileApp.RossoTitoli

        lblSpiegazione.ForeColor = StileApp.TestoPrimario
        lblModifica.ForeColor = StileApp.TestoPrimario

        lvwCampi.BackColor = StileApp.SfondoContenuto
        lvwFuori.BackColor = StileApp.SfondoContenuto
        txtTesto.BackColor = StileApp.SfondoContenuto

        lblNelDocumento.ForeColor = StileApp.TestoPrimario
        lblFuori.ForeColor = StileApp.TestoPrimario

        ' Togliere e rimettere sono due gesti reversibili, e nessuno dei due distrugge
        ' niente: il documento resta intero, cambia solo quel che se ne mostra.
        StileApp.VestiBottone(btnTogli, LivelloBottone.Neutro)
        StileApp.VestiBottone(btnRimetti, LivelloBottone.Neutro)

        StileApp.VestiBottone(btnSalva, LivelloBottone.SicuroPositivo)
        StileApp.VestiBottone(btnAnnulla, LivelloBottone.Neutro)

    End Sub

    ''' <summary>
    ''' Riempie i due elenchi tenendo ferma la scelta di chi ci sta lavorando.
    ''' </summary>
    ''' <remarks>
    ''' Gli elenchi si rifanno da capo a ogni «Togli» e a ogni «Rimetti», e una
    ''' ricostruzione non ha memoria: la scelta ripartiva sempre dalla prima riga, così
    ''' togliere la sesta voce di dieci riportava in cima, e chi ne toglieva tre di fila
    ''' doveva ritrovare il punto ogni volta. La riga si ritrova per <b>identità</b> e non
    ''' per posizione, perché intanto le altre si sono spostate.
    ''' </remarks>
    Private Sub MostraICampi()

        _riempimenti += 1

        Try
            ' Chi era scelto, e dove stava: il primo serve a ritrovarlo se c'è ancora, il
            ' secondo a scegliere chi ha preso il suo posto quando non c'è più.
            Dim eraASinistra As RigaElenco = RigaSceltaASinistra()
            Dim postoASinistra As Integer = PostoScelto(lvwCampi)
            Dim eraADestra As RigaElenco = RigaSceltaADestra()
            Dim postoADestra As Integer = PostoScelto(lvwFuori)

            lvwCampi.Items.Clear()
            lvwFuori.Items.Clear()

            For Each riga As RigaElenco In _righe

                If riga.Impronta IsNot Nothing AndAlso _fuori.Contains(riga.Impronta) Then

                    Dim fuori As New ListViewItem({riga.Etichetta, UnaRiga(riga.Riepilogo)}) With {.Tag = riga}
                    lvwFuori.Items.Add(fuori)

                Else

                    Dim dentro As New ListViewItem({riga.Etichetta, UnaRiga(TestoDellaRiga(riga)),
                                                    SegnoDellaRiga(riga)}) With {.Tag = riga}
                    lvwCampi.Items.Add(dentro)

                End If

            Next

            RimettiLaScelta(lvwCampi, eraASinistra, postoASinistra)
            RimettiLaScelta(lvwFuori, eraADestra, postoADestra)

        Finally
            _riempimenti -= 1
        End Try

        AggiornaIComandi()

    End Sub

    ''' <summary>
    ''' La riga scelta in un elenco, o <c>Nothing</c>.
    ''' </summary>
    ''' <remarks>
    ''' Si guardano le righe una a una invece di chiedere <c>SelectedItems</c>, che è la
    ''' strada breve ma risponde soltanto quando l'elenco è <b>nato</b>: su una finestra
    ''' mai mostrata — cioè al banco — direbbe sempre «non è scelto niente», e un collaudo
    ''' sulla scelta sarebbe verde per il motivo sbagliato. Il <c>Selected</c> di una riga
    ''' invece risponde in tutti e due i casi: dal controllo di Windows quando c'è, dallo
    ''' stato tenuto da parte quando non c'è ancora.
    ''' </remarks>
    Private Shared Function SceltaIn(elenco As ListView) As ListViewItem

        For Each riga As ListViewItem In elenco.Items
            If riga.Selected Then Return riga
        Next

        Return Nothing

    End Function

    ''' <summary>Dov'era la riga scelta in un elenco, o -1 se non era scelto niente.</summary>
    Private Shared Function PostoScelto(elenco As ListView) As Integer

        Dim scelta As ListViewItem = SceltaIn(elenco)
        If scelta Is Nothing Then Return -1

        Return elenco.Items.IndexOf(scelta)

    End Function

    ''' <summary>
    ''' Rimette la scelta dov'era: sulla stessa riga se c'è ancora, altrimenti su quella
    ''' che ne ha preso il posto — e sull'ultima, quando quella sparita era in coda.
    ''' </summary>
    ''' <remarks>
    ''' Un elenco appena riempito, su cui prima non era scelto niente, parte dalla prima
    ''' riga: è il caso dell'apertura, ed è giusto che cominci da lì.
    ''' </remarks>
    Private Shared Sub RimettiLaScelta(elenco As ListView, era As RigaElenco, posto As Integer)

        If elenco.Items.Count = 0 Then Return

        If era IsNot Nothing Then
            For Each riga As ListViewItem In elenco.Items
                If riga.Tag Is era Then
                    Scegli(riga)
                    Return
                End If
            Next
        End If

        Scegli(elenco.Items(Math.Min(Math.Max(posto, 0), elenco.Items.Count - 1)))

    End Sub

    ''' <summary>Sceglie una riga e la porta in vista: una scelta che non si vede non serve.</summary>
    Private Shared Sub Scegli(riga As ListViewItem)

        riga.Selected = True
        riga.EnsureVisible()

    End Sub

    ''' <summary>
    ''' Il testo come si legge nella colonna dell'elenco: una riga sola, perché una
    ''' descrizione va a capo e la riga di un elenco no.
    ''' </summary>
    Private Shared Function UnaRiga(testo As String) As String

        If String.IsNullOrEmpty(testo) Then Return String.Empty

        Return testo.Replace(vbCrLf, " ").Replace(vbLf, " ").Replace(vbCr, " ")

    End Function

    ''' <summary>Cosa si legge nella colonna del testo: la prosa se c'è, altrimenti i fatti.</summary>
    Private Function TestoDellaRiga(riga As RigaElenco) As String

        If riga.IndiceProsa >= 0 Then Return _voci(riga.IndiceProsa).Testo
        Return riga.Riepilogo

    End Function

    ''' <summary>Il segno ✎, che vale per la prosa scritta a mano dall'utente.</summary>
    Private Function SegnoDellaRiga(riga As RigaElenco) As String

        If riga.IndiceProsa < 0 Then Return String.Empty
        Return If(_voci(riga.IndiceProsa).RiscrittoAMano, SegnoRiscritto, String.Empty)

    End Function

    ''' <summary>
    ''' Accende i due comandi su quel che si può fare adesso. «Togli» vuole una riga
    ''' scelta a sinistra che sia una voce — il sommario e il corpo della lettera non lo
    ''' sono — e «Rimetti» vuole una riga scelta a destra.
    ''' </summary>
    Private Sub AggiornaIComandi()

        Dim scelta As RigaElenco = RigaSceltaASinistra()

        btnTogli.Enabled = scelta IsNot Nothing AndAlso scelta.Impronta IsNot Nothing
        btnRimetti.Enabled = RigaSceltaADestra() IsNot Nothing

        If scelta IsNot Nothing AndAlso scelta.Impronta Is Nothing Then
            _suggerimenti.SetToolTip(btnTogli,
                $"«{scelta.Etichetta}» fa parte del documento: si riscrive, non si toglie.")
        Else
            _suggerimenti.SetToolTip(btnTogli, "Lascia questa voce fuori da questo documento.")
        End If

    End Sub

    ''' <summary>La riga scelta nell'elenco di sinistra, o <c>Nothing</c>.</summary>
    Private Function RigaSceltaASinistra() As RigaElenco

        Return TryCast(SceltaIn(lvwCampi)?.Tag, RigaElenco)

    End Function

    ''' <summary>La riga scelta nell'elenco di destra, o <c>Nothing</c>.</summary>
    Private Function RigaSceltaADestra() As RigaElenco

        Return TryCast(SceltaIn(lvwFuori)?.Tag, RigaElenco)

    End Function

    ''' <summary>
    ''' Toglie dal documento la voce con questa impronta. Il documento non si tocca: quel
    ''' che cambia è l'elenco di ciò che non va mostrato, e si rimette con un clic (R6).
    ''' </summary>
    ''' <remarks>
    ''' Prende l'impronta invece di leggere la riga scelta perché è l'unico modo di
    ''' collaudarlo: su una finestra mai mostrata i controlli non si lasciano pilotare
    ''' (v. <see cref="FinestraAppunti.RiscriviLAppunto"/>).
    ''' </remarks>
    ''' <returns>Falso se quell'impronta non è di una voce che si può togliere.</returns>
    Public Function Togli(impronta As String) As Boolean

        If String.IsNullOrEmpty(impronta) Then Return False
        If Not _righe.Any(Function(r) r.Impronta = impronta) Then Return False
        If Not _fuori.Add(impronta) Then Return False

        MostraICampi()
        MostraIlCampoScelto()

        Return True

    End Function

    ''' <summary>Rimette nel documento una voce lasciata fuori.</summary>
    ''' <returns>Falso se quella voce non era fuori.</returns>
    Public Function Rimetti(impronta As String) As Boolean

        If String.IsNullOrEmpty(impronta) Then Return False
        If Not _fuori.Remove(impronta) Then Return False

        MostraICampi()
        MostraIlCampoScelto()

        Return True

    End Function

    ''' <summary>
    ''' L'impronta della riga che si chiama così, o <c>Nothing</c> se quella riga non è
    ''' una voce che si può togliere — il sommario e il corpo della lettera non lo sono.
    ''' </summary>
    Public Function ImprontaDi(etichetta As String) As String

        Return _righe.FirstOrDefault(Function(r) r.Etichetta = etichetta)?.Impronta

    End Function

    ''' <summary>Rimette in riga quello che di un campo si vede nell'elenco.</summary>
    ''' <remarks>
    ''' La riga non sta più dov'era: da R6 l'elenco di sinistra mostra anche le voci senza
    ''' prosa e non mostra quelle lasciate fuori, quindi l'indice di <c>_voci</c> e quello
    ''' della riga a video non coincidono. La riga si ritrova per <c>Tag</c>, che è il solo
    ''' legame che resta vero mentre le righe vanno e vengono.
    ''' </remarks>
    Private Sub AggiornaLaRiga(indice As Integer)

        If Not CE(indice) Then Return

        _riempimenti += 1

        Try

            For Each riga As ListViewItem In lvwCampi.Items

                Dim quale As RigaElenco = TryCast(riga.Tag, RigaElenco)
                If quale Is Nothing OrElse quale.IndiceProsa <> indice Then Continue For

                riga.SubItems(1).Text = UnaRiga(_voci(indice).Testo)
                riga.SubItems(2).Text = If(_voci(indice).RiscrittoAMano, SegnoRiscritto, String.Empty)
                Exit For

            Next

        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ''' <summary>Porta nella casella il campo scelto, e dice cosa se ne può fare.</summary>
    Private Sub MostraIlCampoScelto()

        Dim scelta As RigaElenco = RigaSceltaASinistra()
        Dim indice As Integer = If(scelta Is Nothing, -1, scelta.IndiceProsa)

        _riempimenti += 1

        Try
            txtTesto.Enabled = CE(indice)
            txtTesto.Text = If(CE(indice), _voci(indice).Testo, String.Empty)

            If CE(indice) Then
                lblModifica.Text = $"Il testo di «{_voci(indice).Etichetta}» (puoi riscriverlo):"
            ElseIf scelta IsNot Nothing Then
                ' Una competenza o un titolo di studio sono fatti, e i fatti stanno nel
                ' profilo: di qui si può solo lasciarli fuori da questo documento.
                lblModifica.Text = $"«{scelta.Etichetta}» viene dal profilo: qui puoi solo toglierla " &
                                   "da questo documento."
            Else
                lblModifica.Text = "Scegli una riga dall'elenco."
            End If

        Finally
            _riempimenti -= 1
        End Try

    End Sub

    ''' <summary>La riga di prosa scelta, o -1 se quella scelta non ha prosa.</summary>
    Private ReadOnly Property IndiceScelto As Integer
        Get
            Dim scelta As RigaElenco = RigaSceltaASinistra()
            If scelta Is Nothing Then Return -1
            Return scelta.IndiceProsa
        End Get
    End Property

    ''' <summary>Se quell'indice è un campo di prosa che esiste.</summary>
    Private Function CE(indice As Integer) As Boolean

        Return indice >= 0 AndAlso indice < _voci.Count

    End Function

    Private Sub lvwCampi_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lvwCampi.SelectedIndexChanged

        If _riempimenti > 0 Then Return

        MostraIlCampoScelto()
        AggiornaIComandi()

    End Sub

    Private Sub lvwFuori_SelectedIndexChanged(sender As Object, e As EventArgs) _
        Handles lvwFuori.SelectedIndexChanged

        If _riempimenti > 0 Then Return

        AggiornaIComandi()

    End Sub

    Private Sub btnTogli_Click(sender As Object, e As EventArgs) Handles btnTogli.Click
        Togli(RigaSceltaASinistra()?.Impronta)
    End Sub

    Private Sub btnRimetti_Click(sender As Object, e As EventArgs) Handles btnRimetti.Click
        Rimetti(RigaSceltaADestra()?.Impronta)
    End Sub

    Private Sub txtTesto_TextChanged(sender As Object, e As EventArgs) Handles txtTesto.TextChanged

        If _riempimenti > 0 Then Return

        Riscrivi(IndiceScelto, txtTesto.Text)

    End Sub

    Private Sub btnSalva_Click(sender As Object, e As EventArgs) Handles btnSalva.Click
        Chiudi(EsitoModifica.Confermato, DialogResult.OK)
    End Sub

    Private Sub btnAnnulla_Click(sender As Object, e As EventArgs) Handles btnAnnulla.Click
        Chiudi(EsitoModifica.Annullato, DialogResult.Cancel)
    End Sub

    Private Sub Chiudi(esito As EsitoModifica, risultato As DialogResult)

        _Esito = esito
        DialogResult = risultato
        Close()

    End Sub

End Class
