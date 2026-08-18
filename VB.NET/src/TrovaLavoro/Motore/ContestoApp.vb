Imports System.IO
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace Motore

    ''' <summary>
    ''' Il motore montato all'avvio: la cartella dati, i numeri, il pool, il client
    ''' dell'AI e i servizi che ne discendono, tutti in un posto solo. È ciò che finora
    ''' mancava — i pezzi esistevano dalla tappa T2, ma nessuno li metteva insieme
    ''' quando l'applicazione partiva, e <c>taratura.json</c> e <c>modelli.json</c>
    ''' restavano file che solo i collaudi aprivano.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Il montaggio non solleva mai.</b> Ogni pezzo che manca o non si lascia
    ''' leggere diventa un avviso e un servizio spento, mai un'eccezione: la finestra
    ''' deve comparire comunque e dire cosa non va (cap. 03.8). Senza chiave API, per
    ''' dire, l'applicazione si apre lo stesso e mostra il profilo che ha su disco —
    ''' solo le funzioni che chiamano l'AI restano fuori uso.</para>
    ''' <para>Qui non si scrive niente su disco e non si crea nessuna cartella: chi
    ''' scrive chiama <see cref="CartellaDati.Assicura"/> al momento suo. Un'applicazione
    ''' aperta e richiusa senza fare nulla non deve lasciare cartelle vuote in giro.</para>
    ''' <para>Cosa <b>non</b> sta qui: il <see cref="DialogoProfilo"/>, che non è un
    ''' servizio ma una conversazione — ha uno stato suo e ne nasce uno per ogni dialogo,
    ''' costruito sullo <see cref="Strutturatore"/> di qui.</para>
    ''' </remarks>
    Public Class ContestoApp
        Implements IDisposable

        Private ReadOnly _note As New List(Of String)
        Private ReadOnly _avvisi As New List(Of String)
        Private _smaltito As Boolean

        Private Sub New(cartella As CartellaDati)
            _Cartella = cartella
            _Segreti = New ArchivioSegreti(cartella)
        End Sub

        ''' <summary>Dove vivono i file dell'utente (cap. 11.1).</summary>
        Public ReadOnly Property Cartella As CartellaDati

        ''' <summary>
        ''' La chiave API cifrata su disco (cap. 11.3). C'è sempre — è una porta, non un
        ''' servizio: dice se una chiave c'è, e la salva quando l'utente ne digita una.
        ''' Nasce con la cartella perché <see cref="MontaAi"/> la interroga prima di
        ''' tutto il resto.
        ''' </summary>
        Public ReadOnly Property Segreti As ArchivioSegreti

        ''' <summary>Il pool dei prompt; <c>Nothing</c> se non si è lasciato aprire.</summary>
        Public ReadOnly Property Libreria As LibreriaPrompt

        ''' <summary>I numeri del calcolo delle stelle (cap. 11.6).</summary>
        Public ReadOnly Property Taratura As Motore.Taratura

        ''' <summary>La mappa livello → modello in uso (cap. 02.5).</summary>
        Public ReadOnly Property Modelli As Ai.Modelli

        ''' <summary>Il client dell'AI; <c>Nothing</c> se la chiave non c'è.</summary>
        Public ReadOnly Property Client As ClientClaude

        ''' <summary>Chi struttura le risposte dei turni; <c>Nothing</c> senza AI o senza pool.</summary>
        Public ReadOnly Property Strutturatore As IStrutturatoreTurni

        ''' <summary>Chi ricava il testo da un PDF; <c>Nothing</c> senza AI o senza pool.</summary>
        Public ReadOnly Property Trascrittore As ITrascrittorePdf

        ''' <summary>Il profilo su disco e il suo storico: c'è sempre, anche senza AI.</summary>
        Public ReadOnly Property Archivio As ArchivioProfilo

        ''' <summary>L'import di un CV esistente; <c>Nothing</c> senza AI.</summary>
        Public ReadOnly Property ImportCv As ImportProfilo

        ''' <summary>
        ''' La fila dei passi di una candidatura (T4); <c>Nothing</c> senza AI o senza
        ''' pool, come gli altri servizi che passano dai prompt.
        ''' </summary>
        Public ReadOnly Property Pipeline As PipelineCandidatura

        ''' <summary>
        ''' Chi scrive i documenti; <c>Nothing</c> senza AI o senza pool. È lo stesso che
        ''' lavora dentro la <see cref="Pipeline"/>, ed è qui perché il 📄 CV base non
        ''' passa da quella fila: nasce dal solo profilo, senza alcun annuncio.
        ''' </summary>
        Public ReadOnly Property Generatore As IGeneratore

        ''' <summary>
        ''' Chi scrive l'email di candidatura dalla lettera (T6, cap. 07.1);
        ''' <c>Nothing</c> senza AI o senza pool, come gli altri mestieri.
        ''' </summary>
        Public ReadOnly Property Email As ICompositoreEmail

        ''' <summary>
        ''' La passata anti-slop (T7b, cap. 08); <c>Nothing</c> senza AI o senza pool.
        ''' </summary>
        ''' <remarks>
        ''' Lavora dentro la <see cref="Pipeline"/> per i documenti di una candidatura, ed
        ''' è esposta qui per gli altri due testi che da quella fila non passano: il 📄 CV
        ''' base, che nasce dal solo profilo, e il corpo dell'email, che nasce in P7 dopo
        ''' che l'utente ha scelto gli allegati.
        ''' </remarks>
        Public ReadOnly Property Rifinitura As Motore.Rifinitura

        ''' <summary>
        ''' Chi ragiona sull'opportunità e ne distilla gli appunti di mira (T7c,
        ''' cap. 12 A6); <c>Nothing</c> senza AI o senza pool.
        ''' </summary>
        ''' <remarks>
        ''' Sta qui e non nella <see cref="Pipeline"/> per la stessa ragione dell'email: la
        ''' fila di T4 si percorre da sé, e una conversazione non appartiene a una fila —
        ''' comincia quando l'utente vuole, dura quanto vuole lui e può anche non produrre
        ''' niente.
        ''' </remarks>
        Public ReadOnly Property Brainstorm As IBrainstormatore

        ''' <summary>
        ''' Le candidature su disco: c'è sempre, anche senza AI — le opportunità già
        ''' generate si riaprono comunque (cap. 12.7).
        ''' </summary>
        Public ReadOnly Property Opportunita As ArchivioOpportunita

        ''' <summary>
        ''' La vista d'insieme delle candidature (cap. 07.3). C'è sempre, come l'archivio
        ''' da cui si ricostruisce. Qui sta l'<b>archivio</b> e non il registro già letto:
        ''' l'elenco cambia sotto i piedi — una candidatura in più, uno scarto — e chi lo
        ''' mostra se lo rilegge quando torna in vista (cap. 03.8).
        ''' </summary>
        Public ReadOnly Property Registro As ArchivioRegistro

        ''' <summary>
        ''' I portali e le ricerche salvate in vigore (cap. 06.3). C'è sempre: senza file
        ''' sono i portali predefiniti, e cercare lavoro non dipende dall'AI.
        ''' </summary>
        Public ReadOnly Property Ricerche As Dati.Ricerche

        ''' <summary>Da dove le <see cref="Ricerche"/> si rileggono e dove si riscrivono.</summary>
        Public ReadOnly Property ArchivioRicerche As ArchivioRicerche

        ''' <summary>
        ''' La cartella documenti dell'utente e quel che ci si è riconosciuto dentro
        ''' (cap. 05.2). C'è sempre — vuota finché una cartella non è stata scelta — perché
        ''' leggere un elenco di nomi non dipende dall'AI.
        ''' </summary>
        Public ReadOnly Property Raccolta As RaccoltaDocumenti

        ''' <summary>Da dove la <see cref="Raccolta"/> si rilegge e dove si riscrive.</summary>
        Public ReadOnly Property ArchivioRaccolta As ArchivioRaccoltaDocumenti

        ''' <summary>
        ''' Chi riconosce i documenti di una cartella (cap. 05.2); <c>Nothing</c> senza AI
        ''' o senza pool. Senza di lui la cartella si sceglie lo stesso: i file si vedono,
        ''' e a dire cos'è ciascuno resta l'utente.
        ''' </summary>
        Public ReadOnly Property Classificatore As IClassificatoreDocumenti

        ''' <summary>
        ''' Se c'è tutto ciò che serve per una chiamata all'AI: la chiave <b>e</b> i
        ''' prompt. È la domanda che i pannelli fanno prima di accendere un bottone.
        ''' </summary>
        Public ReadOnly Property AiDisponibile As Boolean
            Get
                Return Strutturatore IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Il resoconto del montaggio, in ordine: cosa si è caricato e da dove. È la
        ''' diagnostica completa — quella che finirà nel log (cap. 11.2) e nelle
        ''' Impostazioni — e comprende anche gli avvisi.
        ''' </summary>
        Public ReadOnly Property Note As IReadOnlyList(Of String)
            Get
                Return _note
            End Get
        End Property

        ''' <summary>
        ''' Ciò che l'utente deve sapere <i>adesso</i>, pronto per la barra di stato;
        ''' <c>Nothing</c> se il montaggio è filato liscio. È un sottoinsieme delle
        ''' <see cref="Note"/>: la maggior parte di quel resoconto — «uso i valori
        ''' predefiniti» al primo avvio — è normalità, e allarmare per la normalità
        ''' insegna solo a non leggere più la barra.
        ''' </summary>
        Public ReadOnly Property Avviso As String
            Get
                If _avvisi.Count = 0 Then Return Nothing
                Return String.Join(" · ", _avvisi)
            End Get
        End Property

        ''' <summary>
        ''' Monta il motore. Non solleva: quel che non si può montare resta spento e
        ''' detto.
        ''' </summary>
        ''' <param name="radiceDati">
        ''' La cartella dati; se omessa, quella predefinita (cap. 11.1). Serve ai
        ''' collaudi per lavorare in una cartella temporanea.
        ''' </param>
        ''' <param name="chiave">
        ''' La chiave API. <c>Nothing</c> significa «cercala dove sta» — prima
        ''' <c>segreti.bin</c>, poi l'ambiente (v. <see cref="LeggiChiave"/>); una
        ''' <b>stringa vuota</b> significa invece «non c'è, e non cercarla da nessuna
        ''' parte»: è ciò che permette al banco di collaudare l'applicazione senza chiave
        ''' anche su una postazione che la chiave ce l'ha davvero.
        ''' </param>
        ''' <param name="cartellaPool">Un pool esterno diverso da quello predefinito (cap. 04.2).</param>
        Public Shared Function Monta(Optional radiceDati As String = Nothing,
                                     Optional chiave As String = Nothing,
                                     Optional cartellaPool As String = Nothing) As ContestoApp

            Dim rifiutata As String = Nothing
            Dim contesto As New ContestoApp(MappaDati(radiceDati, rifiutata))

            If rifiutata IsNot Nothing Then
                contesto.Avvisa($"La cartella dati «{rifiutata}» non è un percorso valido: " &
                                $"uso «{contesto.Cartella.Radice}».")
            End If

            contesto.MontaPool(cartellaPool)
            contesto.MontaNumeri()
            contesto.MontaAi(chiave)
            contesto.MontaDati()

            Return contesto

        End Function

        ''' <summary>
        ''' Rilegge la <see cref="Raccolta"/> da disco, dimenticando quel che è cambiato in
        ''' memoria.
        ''' </summary>
        ''' <remarks>
        ''' Serve a chi <b>annulla</b>: le correzioni alle categorie si scrivono
        ''' nell'oggetto vivo mentre l'utente le fa — è quello che rende la finestra
        ''' immediata — e annullare deve poterle dimenticare tutte insieme, senza tenere
        ''' da parte una copia di riserva che poi qualcuno dovrebbe ricordarsi di
        ''' riallineare.
        ''' </remarks>
        Public Sub RileggiLaRaccolta()
            _Raccolta = ArchivioRaccolta.Carica()
        End Sub

        ''' <summary>
        ''' La mappa della cartella dati, con ripiego sulla predefinita se la radice
        ''' indicata non è un percorso valido: una configurazione sbagliata non deve
        ''' impedire l'avvio. Il ripiego però non è muto — la radice rifiutata torna al
        ''' chiamante, che la mette fra gli avvisi: altrimenti l'utente cercherebbe i
        ''' suoi file in una cartella dove nessuno sta scrivendo.
        ''' </summary>
        ''' <param name="rifiutata">La radice scartata, o <c>Nothing</c> se è andata bene.</param>
        Private Shared Function MappaDati(radice As String, ByRef rifiutata As String) As CartellaDati

            rifiutata = Nothing
            If String.IsNullOrWhiteSpace(radice) Then Return CartellaDati.Predefinita()

            Try
                Return New CartellaDati(radice)
            Catch ex As Exception When TypeOf ex Is ArgumentException OrElse TypeOf ex Is IOException _
                                       OrElse TypeOf ex Is NotSupportedException
                rifiutata = radice
                Return CartellaDati.Predefinita()
            End Try

        End Function

        Private Sub MontaPool(cartellaPool As String)

            Try
                _Libreria = LibreriaPrompt.Apri(cartellaPool)
                Nota($"Libreria prompt: {Libreria.Etichetta}.")

                ' Il pool dichiara da sé quando c'è qualcosa da sapere: un ripiego
                ' sull'integrato, dei file fuori impronta (cap. 04.5).
                If Libreria.Avviso IsNot Nothing Then Avvisa(Libreria.Avviso)

            Catch ex As Exception
                ' Qualunque cosa sia andata storta, l'applicazione deve comparire lo
                ' stesso: senza pool non si chiama l'AI, ma il profilo su disco si legge
                ' e si corregge ugualmente.
                Avvisa($"Libreria dei prompt non disponibile: {ex.Message}")
            End Try

        End Sub

        Private Sub MontaNumeri()

            _Taratura = Motore.Taratura.Carica(Cartella.FileTaratura)
            RiportaRipiego(Taratura.Avviso,
                           Taratura.Origine = OrigineTaratura.Predefinita,
                           Cartella.FileTaratura)

            _Modelli = Ai.Modelli.Carica(Cartella.FileModelli)
            RiportaRipiego(Modelli.Avviso,
                           Modelli.Origine = OrigineModelli.Predefinita,
                           Cartella.FileModelli)

        End Sub

        ''' <summary>
        ''' Come si racconta un ripiego sui valori predefiniti. Un file che <b>non c'è</b>
        ''' è la normalità del primo avvio e resta una nota; un file che <b>c'è ma non si
        ''' legge</b> è un'anomalia e va detta: senza dirlo, chi ha ritoccato un numero
        ''' crederebbe che il suo ritocco sia in vigore mentre non lo è.
        ''' </summary>
        Private Sub RiportaRipiego(avviso As String, ripiegato As Boolean, percorso As String)

            If avviso Is Nothing Then Return

            If ripiegato AndAlso File.Exists(percorso) Then
                Avvisa(avviso)
            Else
                Nota(avviso)
            End If

        End Sub

        Private Sub MontaAi(chiave As String)

            ' Attenzione al nome: in VB le maiuscole non distinguono, e una variabile
            ' che si chiamasse come la funzione qui sotto verrebbe letta come un
            ' indice sulla stringa invece che come una chiamata.
            Dim chiaveInUso As String = LeggiChiave(chiave)

            If chiaveInUso Is Nothing Then
                Avvisa("Manca la chiave API: le funzioni che usano l'AI restano spente.")
                Return
            End If

            _Client = New ClientClaude(chiaveInUso, Modelli)
            Nota($"Modelli in uso: {Modelli.ModelloSemplice.Id} per le estrazioni, " &
                 $"{Modelli.ModelloRagionamento.Id} per il ragionamento.")

            ' Senza pool non c'è nessun prompt da riempire: il client resta montato (la
            ' chiave c'è) ma i servizi che passano dai prompt no. Il perché è già stato
            ' detto da MontaPool, e non si ripete.
            If Libreria Is Nothing Then Return

            _Strutturatore = New StrutturatoreTurni(Libreria, Client)
            _Trascrittore = New TrascrittorePdf(Libreria, Client)

            ' I tre mestieri di T4 e la fila che li ordina. La taratura le serve per il
            ' punteggio, ed è già montata a questo punto (MontaNumeri viene prima).
            ' Il tipo si scrive qualificato: qui «Generatore» è anche il nome della
            ' proprietà, e in VB una cosa che si chiama come un'altra la copre.
            _Generatore = New Ai.Generatore(Libreria, Client)

            ' La rifinitura anti-slop (cap. 08): per la stessa ragione del generatore il
            ' tipo si scrive qualificato, perché la proprietà che la tiene si chiama come
            ' lui. Nasce prima della pipeline perché è la pipeline a riceverla.
            _Rifinitura = New Motore.Rifinitura(New Rifinitore(Libreria, Client))

            _Pipeline = New PipelineCandidatura(
                New AnalizzatoreAnnuncio(Libreria, Client),
                New Confrontatore(Libreria, Client),
                _Generatore,
                Taratura,
                _Rifinitura)

            ' L'email sta fuori dalla pipeline e non è una dimenticanza: la fila di T4
            ' arriva ai documenti, e da lì in poi decide l'utente — quali allegati, a chi,
            ' e se spedire. Un passo che aspetta delle scelte non appartiene a una fila
            ' che si percorre da sé (cap. 07.1).
            _Email = New CompositoreEmail(Libreria, Client)

            ' Chi smista la cartella documenti (cap. 05.2). Sta qui accanto all'email
            ' perché è lì che serve — gli attestati da allegare — ma non appartiene a
            ' quella fila più di quanto ci appartenga il compositore.
            _Classificatore = New ClassificatoreDocumenti(Libreria, Client)

            ' Chi ragiona sull'opportunità (T7c): fuori dalla pipeline, come l'email, e
            ' per lo stesso motivo — una conversazione la comincia l'utente.
            _Brainstorm = New Brainstormatore(Libreria, Client)

        End Sub

        ''' <summary>
        ''' La chiave da usare, o <c>Nothing</c> se non ce n'è una. Si cerca in tre
        ''' posti, in quest'ordine: quella <b>dichiarata dal chiamante</b> (il banco), il
        ''' <b>file cifrato</b> della cartella dati (cap. 11.3), la <b>variabile
        ''' d'ambiente</b>.
        ''' </summary>
        ''' <remarks>
        ''' <para>Il file viene prima dell'ambiente perché è la volontà più recente
        ''' dell'utente: l'ha digitata lui nell'applicazione, mentre la variabile è
        ''' l'eredità di T2 e resta per lo sviluppo e per i collaudi reali. Perché la
        ''' precedenza non diventi una sorpresa — «ho cambiato la variabile e non
        ''' succede niente» — la provenienza finisce fra le <see cref="Note"/>, con la
        ''' chiave <b>mascherata</b>: nella diagnostica non compare mai per intero
        ''' (cap. 11.3).</para>
        ''' </remarks>
        Private Function LeggiChiave(chiave As String) As String

            ' Dichiarata dal chiamante: una stringa vuota è un «no» esplicito.
            If chiave IsNot Nothing Then
                If String.IsNullOrWhiteSpace(chiave) Then Return Nothing
                Nota($"Chiave API: quella indicata all'avvio ({ArchivioSegreti.Maschera(chiave)}).")
                Return chiave
            End If

            Dim illeggibile As Boolean
            Dim salvata As String = Segreti.LeggiChiaveApi(illeggibile)

            If salvata IsNot Nothing Then
                Nota($"Chiave API: dal file cifrato della cartella dati ({ArchivioSegreti.Maschera(salvata)}).")
                Return salvata
            End If

            ' Un file che c'è e non si apre non è un «non ce l'ho»: l'utente lo vede su
            ' disco e lo crede buono. Di solito è lo stesso file su un altro PC o sotto
            ' un altro account di Windows, che è precisamente ciò che DPAPI promette.
            If illeggibile Then
                Avvisa("Il file della chiave API c'è ma non si decifra: è stato salvato " &
                       "da un altro account di Windows o su un altro PC. Va reinserita.")
            End If

            Try
                Dim dallAmbiente As String = ClientClaude.ChiaveDaAmbiente()
                Nota($"Chiave API: dalla variabile {ClientClaude.NomeVariabileChiave} " &
                     $"({ArchivioSegreti.Maschera(dallAmbiente)}).")
                Return dallAmbiente
            Catch ex As ErroreAi
                Return Nothing
            End Try

        End Function

        Private Sub MontaDati()

            _Archivio = New ArchivioProfilo(Cartella)
            _Opportunita = New ArchivioOpportunita(Cartella)
            _Registro = New ArchivioRegistro(Cartella, _Opportunita)

            _ArchivioRicerche = New ArchivioRicerche(Cartella)
            _Ricerche = ArchivioRicerche.Carica()
            RiportaRicerche()

            ' La cartella documenti dell'utente (cap. 05.2). Che non ci sia è la
            ' normalità — è una comodità, non un ingranaggio — ma una cartella scelta e
            ' poi sparita va detta: gli attestati che l'utente si aspetta fra gli
            ' allegati non ci sarebbero, e nessuno gli avrebbe spiegato perché.
            _ArchivioRaccolta = New ArchivioRaccoltaDocumenti(Cartella)
            _Raccolta = ArchivioRaccolta.Carica()

            If Not String.IsNullOrWhiteSpace(Raccolta.Cartella) AndAlso Not Raccolta.CartellaUtilizzabile Then
                Avvisa($"La cartella dei documenti «{Raccolta.Cartella}» non c'è più: " &
                       "gli attestati da allegare non si possono proporre finché non ne scegli un'altra.")
            End If

            Nota(If(Archivio.Esiste,
                    $"Profilo presente in «{Cartella.FileProfilo}».",
                    "Nessun profilo salvato: si parte dal bivio «ho già un CV» / «costruiamolo insieme»."))

            If Strutturatore IsNot Nothing Then
                _ImportCv = New ImportProfilo(Strutturatore, Trascrittore)
            End If

        End Sub

        ''' <summary>
        ''' Come si racconta ciò che le ricerche hanno da dire. Il file assente è la
        ''' normalità del primo avvio e resta una nota; un file che <b>c'è</b> e ha qualcosa
        ''' che non è entrato — uno schema che non è un indirizzo del web, una ricerca che
        ''' nomina un portale sconosciuto — è un avviso, perché quella riga l'ha scritta
        ''' l'utente e sta per non trovarsela nel menù.
        ''' </summary>
        Private Sub RiportaRicerche()

            If Ricerche.Avviso Is Nothing Then Return

            If Ricerche.Origine = OrigineRicerche.File Then
                Avvisa(Ricerche.Avviso)
            Else
                RiportaRipiego(Ricerche.Avviso, ripiegato:=True, percorso:=Cartella.FileRicerche)
            End If

        End Sub

        Private Sub Nota(testo As String)
            _note.Add(testo)
        End Sub

        ''' <summary>Un avviso è anche una nota: il resoconto resta completo e in ordine.</summary>
        Private Sub Avvisa(testo As String)
            _avvisi.Add(testo)
            _note.Add(testo)
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose

            If _smaltito Then Return
            _smaltito = True

            Client?.Dispose()
            GC.SuppressFinalize(Me)

        End Sub

    End Class

End Namespace
