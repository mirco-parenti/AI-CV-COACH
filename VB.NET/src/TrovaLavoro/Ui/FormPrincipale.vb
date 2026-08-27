Imports System.Drawing
Imports System.Windows.Forms
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Documenti
Imports TrovaLavoro.Motore
Imports TrovaLavoro.Web

''' <summary>
''' Finestra principale dell'applicazione (cap. 03.4): barra superiore di navigazione,
''' area centrale, fascia di stato e pannello logo. È anche il punto in cui il motore
''' viene montato all'avvio e smaltito alla chiusura; i pannelli P1–P7 si appoggiano al
''' <see cref="ContestoApp"/> che nasce qui.
''' </summary>
Public Class FormPrincipale

    ''' <summary>Sotto questa larghezza il pannello logo passa in compatta (cap. 03.5).</summary>
    Private Const LarghezzaModalitaCompatta As Integer = 1350

    ''' <summary>
    ''' Il minimo della finestra in unità di progetto (cap. 03.4). Il Designer dichiara gli
    ''' stessi due numeri — è lì che li vuole la finestra di progettazione — ma li lascia
    ''' scalare a WinForms in modo asimmetrico: il valore buono lo rimette
    ''' <see cref="RiapplicaIlMinimoDellaFinestra"/>.
    ''' </summary>
    Private Const LarghezzaMinimaDiProgetto As Integer = 1150
    Private Const AltezzaMinimaDiProgetto As Integer = 600

    ' Geometria del pannello logo nelle due modalità.
    Private Const LogoLarghezza As Integer = 261
    Private Const LogoAltezza As Integer = 216
    Private Const LogoLatoImmagine As Integer = 101
    Private Const LogoLarghezzaCompatta As Integer = 130
    Private Const LogoAltezzaCompatta As Integer = 96
    Private Const LogoLatoImmagineCompatta As Integer = 56
    Private Const AltezzaRigaNome As Integer = 30
    Private Const AltezzaRigaDidascalia As Integer = 15

    ' Nothing finché la modalità non è stata decisa la prima volta.
    Private compattaAttiva As Boolean?

    ' L'ultimo ingombro comunicato ai pannelli: si ridichiara solo quando cambia davvero.
    Private ingombroDichiarato As Size = Size.Empty

    ''' <summary>Il motore montato all'avvio: da qui in avanti lo usano i pannelli.</summary>
    Private _contesto As ContestoApp

    ''' <summary>Quel che è stato chiesto dalla riga di comando (cap. 11.1).</summary>
    Private ReadOnly _argomenti As ArgomentiAvvio

    ''' <summary>
    ''' Il costruttore senza argomenti serve alla finestra di progettazione, che non ha
    ''' nessuna riga di comando da passare: là dentro vale la cartella dati di sempre.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <param name="schermataDiAvvio">
    ''' La schermata che copre il montaggio (cap. 03.4), se ce n'è una: è
    ''' <c>Nothing</c> nel banco e in ogni avvio che non l'ha aperta. La finestra non sa
    ''' che aspetto abbia — le chiede soltanto di togliersi, e sa dire <b>quando</b>.
    ''' </param>
    Public Sub New(argomenti As ArgomentiAvvio, Optional schermataDiAvvio As ISchermataDiAvvio = Nothing)

        InitializeComponent()
        Marchio.Vesti(Me)
        _argomenti = If(argomenti, ArgomentiAvvio.Leggi(Nothing))
        _schermataDiAvvio = schermataDiAvvio
        DichiaraLaPortaDelleInformazioni()

    End Sub

    ''' <summary>
    ''' Il pannello del logo è cliccabile e deve sembrarlo: mano sul puntatore e
    ''' suggerimento, su tutte le sue parti. Senza, sarebbe una porta senza maniglia —
    ''' la lezione di T9d, «quel che è acceso deve sembrarlo», applicata a un pannello
    ''' che per undici tappe è stato solo un'insegna.
    ''' </summary>
    Private Sub DichiaraLaPortaDelleInformazioni()

        For Each parte As Control In PartiDelPannelloLogo()
            parte.Cursor = Cursors.Hand
            ttSuggerimenti.SetToolTip(parte, "Informazioni su TrovaLavoro")
        Next

    End Sub

    ''' <summary>
    ''' Le parti del pannello del logo, in un posto solo: le vestono il puntatore, il
    ''' suggerimento e il clic che apre «Informazioni su…», e tre elenchi separati
    ''' finirebbero per non dire più la stessa cosa.
    ''' </summary>
    Private Function PartiDelPannelloLogo() As Control()
        Return New Control() {pnlLogo, picLogo, lblMarchio, lblVersione, lblCopyright}
    End Function

    ''' <summary>
    ''' Apre «Informazioni su…» (cap. 03.4). La porta è il pannello del logo, che è dove
    ''' versione e pool si vanno già a leggere (cap. 03.5): nessun bottone nuovo in
    ''' barra, e il gesto è quello che si prova per primo.
    ''' </summary>
    Private Sub ApriLeInformazioni(mittente As Object, e As EventArgs) _
        Handles pnlLogo.Click, picLogo.Click, lblMarchio.Click, lblVersione.Click, lblCopyright.Click

        FinestraInformazioni.Mostra(Me, EtichettaDelPool(), AddressOf ComponiLaDiagnostica)

    End Sub

    ''' <summary>
    ''' Il foglietto da mettere negli appunti quando si chiede «Copia diagnostica»
    ''' (cap. 11.1). Si compone al momento del clic e non all'apertura della finestra:
    ''' fra le due cose può esserci passato di mezzo il guasto che si vuole raccontare.
    ''' </summary>
    Private Function ComponiLaDiagnostica() As String

        Return Diagnostica.Componi(
            Date.Now,
            Versione.Riga(EtichettaDelPool()),
            Versione.RigaDelSorgente(),
            _contesto?.Cartella?.Radice,
            ModelliInVigore(),
            _contesto?.Diario?.UltimeRighe(Diagnostica.RigheDiDiario))

    End Function

    ''' <summary>I due modelli in vigore, scritti come si leggono; <c>Nothing</c> se non ci sono.</summary>
    Private Function ModelliInVigore() As String

        Dim modelli As Ai.Modelli = _contesto?.Modelli
        If modelli Is Nothing Then Return Nothing

        Return $"{modelli.ModelloSemplice?.Id} (estrazione) · {modelli.ModelloRagionamento?.Id} (ragionamento)"

    End Function

    ''' <summary>La schermata di avvio da togliere, se l'avvio ne ha aperta una.</summary>
    Private ReadOnly _schermataDiAvvio As ISchermataDiAvvio

    ''' <summary>
    ''' L'unico motore del browser dell'applicazione (v. <see cref="MotoreBrowser"/>).
    ''' Nasce qui, come la stampante, perché è di qui che scendono i due usi che lo
    ''' vogliono: la stampa dei PDF e il browser integrato del pannello Ricerca.
    ''' </summary>
    Private _motoreBrowser As MotoreBrowser

    ''' <summary>
    ''' La stampante PDF nasce <b>qui</b> e non nel motore: WebView2 è un controllo
    ''' WinForms e vuole il thread dell'interfaccia con la sua pompa di messaggi
    ''' (v. <see cref="StampantePdf"/>). Accenderla costa, e resta accesa per tutte le
    ''' stampe della sessione; si spegne alla chiusura.
    ''' </summary>
    Private _stampante As StampantePdf

    ''' <summary>
    ''' Il lucchetto di scrittura della cartella dati (cap. 09.4), preso all'avvio e
    ''' tenuto per tutta la sessione: finché questa finestra è aperta, i tool del server
    ''' MCP che scrivono rispondono «la cartella è in uso» invece di cambiare sotto i
    ''' piedi dei dati che qui stanno in memoria. È <c>Nothing</c> se non si è riusciti a
    ''' prenderlo — e allora è l'utente a essere avvisato, non il programma a fermarsi.
    ''' </summary>
    Private _lucchetto As LucchettoDati

    ''' <summary>Da quale pannello si è arrivati ai documenti, e con quale bottone della barra.</summary>
    Private _ritornoDaiDocumenti As Control
    Private _bottoneDelRitorno As Button

    Private Sub FormPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _contesto = ContestoApp.Monta(_argomenti.RadiceDati)

        ' Il lucchetto prima di tutto, perché da qui in poi qualunque cosa può scrivere.
        ' Non si rilascia alla rimonta del motore per la chiave: la cartella dati è la
        ' stessa, e quel che stiamo dichiarando è la sessione, non il contesto.
        _lucchetto = LucchettoDati.Prendi(_contesto.Cartella)

        ' Prima ancora della chiave: si informa chi sta per decidere se fidarsi, non chi
        ' ha già deciso (cap. 11.2).
        MostraLInformativaLaPrimaVolta()

        ' Prima dei pannelli, perché una chiave data adesso rimonta il motore: collegarli
        ' a un contesto che sta per essere sostituito vorrebbe dire riaccenderli a mano
        ' uno per uno (cap. 11.3).
        ChiediLaChiaveApiSeServe()

        MostraLoStatoDellAvvio()
        DichiaraLaCartellaDati()

        ' La barra di navigazione è fatta di bottoni neutri (livello 0, cap. 03.3): li
        ' veste StileApp, che sa anche come si spegne un bottone.
        For Each navigazione As Button In BottoniDiNavigazione()
            StileApp.VestiBottone(navigazione, LivelloBottone.Neutro)
        Next


        ' Un motore del browser per tutta l'applicazione, e da lui la stampante: due
        ' ambienti sulla stessa cartella di navigazione stanno buoni solo finché nessuno
        ' cambia loro un'opzione (v. MotoreBrowser, cancello di T5a).
        _motoreBrowser = New MotoreBrowser(_contesto.Cartella.CartellaWebView2)
        _stampante = New StampantePdf(_motoreBrowser)

        CollegaIPannelli()

        pnlLogo.BringToFront()
        AggiornaPannelloLogo()

        ' La casa è il cruscotto (T5c): dice a che punto si è e da dove riprendere. Al
        ' primo avvio, quando non c'è ancora niente, è lui a mandare al profilo — che
        ' resta il primo passo del flusso A.
        MostraPannello(pnlHome, btnHome)
    End Sub

    ''' <summary>
    ''' La finestra è a video: la schermata di avvio ha finito il suo mestiere e se ne
    ''' va — appena scaduto il minimo, se non è ancora passato. Si aspetta <c>Shown</c>
    ''' e non la fine del <c>Load</c>, che arriva prima del primo disegno: toglierla là
    ''' lascerebbe scoperto per un istante il desktop.
    ''' </summary>
    Private Sub FormPrincipale_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        _schermataDiAvvio?.ChiudiQuandoPuoi()
        RiapplicaIlMinimoDellaFinestra()
    End Sub

    ''' <summary>
    ''' Rimette il minimo di progetto (cap. 03.4) in pixel veri di questo schermo.
    ''' <c>AutoScaleMode.Font</c> lo scala già per conto suo, ma con rapporti <b>diversi</b>
    ''' per larghezza e altezza — a 144 DPI ×1,42 contro ×1,605 — e la larghezza finiva 61
    ''' unità di progetto <i>sotto</i> il minimo dichiarato, cioè dentro le misure in cui la
    ''' fascia dei comandi non ha più spazio. Si fa nello <c>Shown</c> e non nel <c>Load</c>
    ''' perché la scalatura automatica dev'essere già avvenuta: altrimenti l'ultima parola
    ''' resta la sua (decisione 15.7).
    ''' </summary>
    Private Sub RiapplicaIlMinimoDellaFinestra()

        Me.MinimumSize = New Size(
            ScalaSchermo.InPixelDelloSchermo(LarghezzaMinimaDiProgetto, Me.DeviceDpi),
            ScalaSchermo.InPixelDelloSchermo(AltezzaMinimaDiProgetto, Me.DeviceDpi))

    End Sub

    ''' <summary>
    ''' Dà a ogni pannello il contesto in vigore. Sta in un metodo perché si fa <b>due
    ''' volte</b>: all'avvio, e quando una chiave data dalle Impostazioni rimonta il
    ''' motore a pannelli già in piedi (cap. 11.3). All'avvio la chiave arriva prima dei
    ''' pannelli e basterebbe una volta sola; dopo, no.
    ''' </summary>
    Private Sub CollegaIPannelli()

        pnlHome.Collega(_contesto)
        pnlProfilo.Collega(_contesto)
        pnlDialogo.Collega(_contesto)
        pnlOpportunita.Collega(_contesto)
        pnlDocumenti.Collega(_contesto, New ArchivioDocumenti(_contesto.Cartella, _stampante))
        pnlRicerca.Collega(_contesto, _motoreBrowser)
        pnlEmail.Collega(_contesto)

    End Sub

    ''' <summary>
    ''' Mostra uno dei pannelli dell'area centrale, uno solo per volta (cap. 03.4), e
    ''' segna quale bottone della barra lo sta mostrando.
    ''' </summary>
    Private Sub MostraPannello(pannello As Control, bottone As Button)

        SalvaChiEsce(pannello)

        For Each figlio As Control In pnlAreaCentrale.Controls
            figlio.Visible = (figlio Is pannello)
        Next

        For Each navigazione As Button In BottoniDiNavigazione()
            ' Un bottone spento resta spento: l'evidenza del pannello attivo non deve
            ' riaccendere il colore di una destinazione che ancora non esiste.
            If Not navigazione.Enabled Then Continue For
            navigazione.BackColor = If(navigazione Is bottone, StileApp.AccentoTenue, StileApp.SfondoContenuto)
        Next

        pnlLogo.BringToFront()

    End Sub

    ''' <summary>
    ''' Dà al pannello che si sta lasciando l'occasione di mettere al sicuro quello che
    ''' l'utente gli ha scritto dentro (<see cref="IPannelloCheSalvaUscendo"/>).
    ''' </summary>
    ''' <remarks>
    ''' <para>Passa di qui <b>ogni</b> cambio di pannello, compresi quelli che partono
    ''' dalla barra in cima: è il buco che faceva perdere la bozza dell'email uscendo da P7
    ''' senza toccare il suo «◀ Torna ai documenti» *(2026-08-18)*. I bottoni propri dei
    ''' pannelli continuano a salvare per conto loro — chiamarlo due volte non fa danno,
    ''' ed è meglio di affidarsi a un solo dei due passaggi.</para>
    ''' <para>Non tocca il pannello che sta per essere mostrato: entrando non c'è ancora
    ''' niente da salvare, e chiamarlo lì scriverebbe sopra il disco un contenuto appena
    ''' letto dal disco.</para>
    ''' </remarks>
    Private Sub SalvaChiEsce(entrante As Control)

        For Each figlio As Control In pnlAreaCentrale.Controls

            If figlio Is entrante OrElse Not figlio.Visible Then Continue For

            TryCast(figlio, IPannelloCheSalvaUscendo)?.SalvaUscendo()

        Next

    End Sub

    Private Function BottoniDiNavigazione() As Button()
        Return {btnHome, btnProfilo, btnRicerca, btnCandidatura, btnDocumenti, btnImpostazioni}
    End Function

    Private Sub btnHome_Click(sender As Object, e As EventArgs) Handles btnHome.Click
        MostraPannello(pnlHome, btnHome)
    End Sub

    Private Sub btnProfilo_Click(sender As Object, e As EventArgs) Handles btnProfilo.Click
        MostraPannello(pnlProfilo, btnProfilo)
    End Sub

    ''' <summary>
    ''' Apre le Impostazioni (P8, cap. 03). È una finestra e non un pannello dell'area:
    ''' non è un passo di nessun flusso, e si raggiunge da dovunque si sia.
    ''' </summary>
    ''' <remarks>
    ''' Tre cose possono uscirne, e nessuna la sa fare la finestra da sola. Una chiave
    ''' nuova vuole il motore <b>rimontato</b> e i pannelli ricollegati, com'è già al
    ''' primo avvio (cap. 11.3). La cartella documenti vuole P7, dove quel giro sa
    ''' aspettare l'AI e annullarla (cap. 05.2): le Impostazioni ci mandano, invece di
    ''' rifarlo. E i dati eliminati vogliono la chiusura, perché da lì in poi ogni
    ''' pannello lavorerebbe su file che non ci sono più (cap. 11.5).
    ''' </remarks>
    Private Async Sub btnImpostazioni_Click(sender As Object, e As EventArgs) Handles btnImpostazioni.Click

        Dim chiaveCambiata As Boolean
        Dim documenti As Boolean
        Dim eliminati As Boolean

        Using finestra As New FinestraImpostazioni(_contesto)
            finestra.ShowDialog(Me)
            chiaveCambiata = finestra.ChiaveCambiata
            documenti = finestra.VuoleGestireIDocumenti
            eliminati = finestra.DatiEliminati
        End Using

        If eliminati Then
            Close()
            Return
        End If

        If chiaveCambiata Then
            _contesto.Dispose()
            _contesto = ContestoApp.Monta(_argomenti.RadiceDati)
            CollegaIPannelli()
            AggiornaPannelloLogo()
        End If

        If documenti Then
            MostraPannello(pnlEmail, btnCandidatura)
            Await pnlEmail.GestisciIDocumentiAsync()
        End If

    End Sub

    ''' <summary>Dalle scorciatoie del cruscotto ai due flussi che ci portano (cap. 03.6).</summary>
    Private Sub pnlHome_ProfiloRichiesto(sender As Object, e As EventArgs) Handles pnlHome.ProfiloRichiesto
        MostraPannello(pnlProfilo, btnProfilo)
    End Sub

    Private Async Sub pnlHome_RicercaRichiesta(sender As Object, e As EventArgs) Handles pnlHome.RicercaRichiesta

        MostraPannello(pnlRicerca, btnRicerca)
        Await pnlRicerca.ApriAsync()

    End Sub

    ''' <summary>
    ''' Una candidatura scelta nella coda torna nella sua scheda, com'era: è la promessa
    ''' «tutto riapribile» del cap. 12.7, che fino a T5c era mantenuta solo sul disco.
    ''' </summary>
    Private Sub pnlHome_CandidaturaScelta(sender As Object, e As CandidaturaSceltaEventArgs) _
        Handles pnlHome.CandidaturaScelta

        MostraPannello(pnlOpportunita, btnCandidatura)
        pnlOpportunita.RiapriLaCandidatura(e.Candidatura)

    End Sub

    ''' <summary>
    ''' La voce «📄 Documenti» della barra (T9d): porta a P6 da fermo, senza passare da un
    ''' flusso. Si torna alla Home, che è la casa di chi si sposta con la barra.
    ''' </summary>
    Private Async Sub btnDocumenti_Click(sender As Object, e As EventArgs) Handles btnDocumenti.Click

        VaiAiDocumenti(pnlHome, btnHome)
        Await pnlDocumenti.ApriQualcosaAsync()

    End Sub

    Private Sub btnCandidatura_Click(sender As Object, e As EventArgs) Handles btnCandidatura.Click
        MostraPannello(pnlOpportunita, btnCandidatura)
    End Sub

    ''' <summary>
    ''' Alla ricerca: il pannello si mostra <b>subito</b> e il browser si accende dopo, che
    ''' è l'ordine giusto — accendere il motore prima di mostrare il pannello lascerebbe la
    ''' finestra ferma sul pannello di prima per il tempo dell'accensione, e sembrerebbe
    ''' che il bottone non abbia funzionato.
    ''' </summary>
    Private Async Sub btnRicerca_Click(sender As Object, e As EventArgs) Handles btnRicerca.Click

        MostraPannello(pnlRicerca, btnRicerca)
        Await pnlRicerca.ApriAsync()

    End Sub

    ''' <summary>
    ''' L'annuncio catturato in P3 va alla scheda della candidatura, che è dove si
    ''' analizza (cap. 12, A4 → A5). Si mostra il pannello <b>prima</b> di far partire
    ''' l'analisi, per la stessa ragione per cui la ricerca si mostra prima di accendere il
    ''' browser: altrimenti la finestra resterebbe sul pannello di prima per tutta
    ''' l'attesa, e sembrerebbe che il bottone non abbia funzionato.
    ''' </summary>
    Private Async Sub pnlRicerca_AnnuncioCatturato(sender As Object, e As AnnuncioCatturatoEventArgs) _
        Handles pnlRicerca.AnnuncioCatturato

        MostraPannello(pnlOpportunita, btnCandidatura)
        Await pnlOpportunita.AnalizzaIlCatturatoAsync(e.Testo, e.Fonte, e.Link)

    End Sub

    ''' <summary>
    ''' Il CV letto in P3 va alla scheda del profilo (cap. 06.7, T5d), per la stessa
    ''' ragione per cui l'annuncio va alla candidatura: è lì che quel testo diventa
    ''' qualcosa, ed è lì che l'utente lo controlla prima di salvarlo. Anche qui il
    ''' pannello si mostra <b>prima</b> della lettura, così l'attesa si vede dove
    ''' succede invece che su un pannello che si sta per lasciare.
    ''' </summary>
    Private Async Sub pnlRicerca_CvCatturato(sender As Object, e As CvCatturatoEventArgs) _
        Handles pnlRicerca.CvCatturato

        MostraPannello(pnlProfilo, btnProfilo)
        Await pnlProfilo.ImportaDaTestoAsync(e.Testo, OrigineDi(e.Fonte))

    End Sub

    ''' <summary>
    ''' Il viaggio contrario: dalla scheda del profilo al browser, perché è lì che si legge
    ''' una pagina (cap. 06.7). La scelta della strada sta in P2, dove chi vuole un profilo
    ''' la cerca; l'atto sta in P3, dove c'è il browser — e il pannello, arrivando, dice
    ''' cosa fare, che è la ragione per cui questa apertura non è quella della barra.
    ''' </summary>
    Private Async Sub pnlProfilo_ImportDaSitoRichiesto(sender As Object, e As EventArgs) _
        Handles pnlProfilo.ImportDaSitoRichiesto

        MostraPannello(pnlRicerca, btnRicerca)
        Await pnlRicerca.ApriPerIlCvAsync()

    End Sub

    ''' <summary>
    ''' Da dove viene il CV, detto all'utente. Il sito è già quello che il pannello della
    ''' ricerca sa riconoscere; quando non c'è — una pagina locale, un indirizzo strano —
    ''' si dice comunque qualcosa di vero.
    ''' </summary>
    Private Shared Function OrigineDi(fonte As String) As String

        Return If(String.IsNullOrWhiteSpace(fonte),
                  "dalla pagina aperta nel browser",
                  $"da {fonte}")

    End Function

    ''' <summary>
    ''' Dalla scheda dell'opportunità ai documenti. Il bottone della barra resta quello
    ''' della candidatura: P6 non è un'altra destinazione, è il passo successivo dello
    ''' stesso flusso (cap. 12, A7).
    ''' </summary>
    Private Async Sub pnlOpportunita_DocumentiRichiesti(sender As Object, e As EventArgs) _
        Handles pnlOpportunita.DocumentiRichiesti

        Dim candidatura As Opportunita = pnlOpportunita.Candidatura
        If candidatura Is Nothing Then Return

        VaiAiDocumenti(pnlOpportunita, btnCandidatura)
        Await pnlDocumenti.MostraLaCandidaturaAsync(candidatura)

    End Sub

    ''' <summary>
    ''' Dalla scheda dell'opportunità al ragionamento (cap. 12, A6). Vale la stessa regola
    ''' dei documenti — il bottone della barra resta quello della candidatura — con una
    ''' differenza: P5 è un pannello che ha già un altro mestiere, e ci si entra dicendogli
    ''' quale dei due sta facendo adesso.
    ''' </summary>
    Private Async Sub pnlOpportunita_BrainstormRichiesto(sender As Object, e As EventArgs) _
        Handles pnlOpportunita.BrainstormRichiesto

        Dim candidatura As Opportunita = pnlOpportunita.Candidatura
        If candidatura Is Nothing Then Return

        MostraPannello(pnlDialogo, btnCandidatura)
        Await pnlDialogo.ApriIlBrainstormingAsync(candidatura)

    End Sub

    ''' <summary>Dal ragionamento si torna alla candidatura, che è da dove si è venuti.</summary>
    Private Sub pnlDialogo_TornaAllaCandidatura(sender As Object, e As EventArgs) _
        Handles pnlDialogo.TornaAllaCandidatura

        MostraPannello(pnlOpportunita, btnCandidatura)

    End Sub

    ''' <summary>
    ''' Gli appunti sono stati confermati e sono su disco: si torna alla candidatura, dove
    ''' adesso c'è qualcosa che prima non c'era.
    ''' </summary>
    Private Sub pnlDialogo_AppuntiConfermati(sender As Object, e As EventArgs) _
        Handles pnlDialogo.AppuntiConfermati

        MostraPannello(pnlOpportunita, btnCandidatura)

    End Sub

    ''' <summary>
    ''' Dai documenti all'email (cap. 12, A8). Come per i documenti, il bottone della barra
    ''' resta quello della candidatura: P7 non è un'altra destinazione, è l'ultimo passo
    ''' dello stesso flusso — quello in cui il programma si ferma e passa la parola al
    ''' programma di posta.
    ''' </summary>
    Private Async Sub pnlDocumenti_EmailRichiesta(sender As Object, e As EventArgs) _
        Handles pnlDocumenti.EmailRichiesta

        Dim candidatura As Opportunita = pnlDocumenti.Candidatura
        If candidatura Is Nothing Then Return

        MostraPannello(pnlEmail, btnCandidatura)
        Await pnlEmail.MostraLaCandidaturaAsync(candidatura)

    End Sub

    ''' <summary>Dall'email si torna ai documenti, che è da dove si è venuti.</summary>
    Private Sub pnlEmail_TornaAiDocumenti(sender As Object, e As EventArgs) _
        Handles pnlEmail.TornaAiDocumenti

        MostraPannello(pnlDocumenti, btnCandidatura)

    End Sub

    ''' <summary>
    ''' La candidatura è stata dichiarata inviata: il cruscotto ha un numero in più da
    ''' mostrare, e la coda uno stato nuovo (cap. 07.3).
    ''' </summary>
    Private Sub pnlEmail_CandidaturaInviata(sender As Object, e As EventArgs) _
        Handles pnlEmail.CandidaturaInviata

        pnlHome.Aggiorna()

    End Sub

    ''' <summary>Come per gli altri pannelli: mentre l'AI scrive, la barra non porta via.</summary>
    Private Sub pnlEmail_LavoroAiCambiato(sender As Object, e As EventArgs) _
        Handles pnlEmail.LavoroAiCambiato

        BarraDiNavigazione(libera:=Not pnlEmail.AiAlLavoro)

    End Sub

    Private Sub pnlDocumenti_TornaIndietro(sender As Object, e As EventArgs) _
        Handles pnlDocumenti.TornaIndietro

        ' Si torna da dove si è venuti: dai documenti si esce verso l'opportunità o verso
        ' la scheda del profilo, a seconda di quale delle due ci ha mandati qui.
        MostraPannello(If(_ritornoDaiDocumenti, pnlOpportunita),
                       If(_bottoneDelRitorno, btnCandidatura))

    End Sub

    ''' <summary>Porta ai documenti, ricordandosi la strada da cui si è arrivati.</summary>
    ''' <remarks>
    ''' <b>In barra si accende «📄 Documenti», qualunque sia la strada</b> (T9d). Prima si
    ''' teneva acceso il bottone di provenienza, perché P6 non era una destinazione ma il
    ''' passo successivo di un flusso; da quando ha una voce sua, la barra direbbe di
    ''' trovarsi in un posto diverso da quello che si sta guardando. Dove si torna, invece,
    ''' resta la strada percorsa: alla candidatura, al profilo o alla Home.
    ''' </remarks>
    Private Sub VaiAiDocumenti(daDove As Control, conQualeBottone As Button)

        _ritornoDaiDocumenti = daDove
        _bottoneDelRitorno = conQualeBottone

        MostraPannello(pnlDocumenti, btnDocumenti)

    End Sub

    ''' <summary>
    ''' Il 📄 CV base si chiede dalla scheda del profilo — è il ritratto del profilo, non
    ''' di una candidatura — e si guarda dove si guardano tutti i documenti.
    ''' </summary>
    Private Async Sub pnlProfilo_CvBaseRichiesto(sender As Object, e As EventArgs) _
        Handles pnlProfilo.CvBaseRichiesto

        VaiAiDocumenti(pnlProfilo, btnProfilo)
        Await pnlDocumenti.MostraIlCvBaseAsync()

    End Sub

    ''' <summary>
    ''' Il profilo è stato eliminato (cap. 11.5). La scheda si è già svuotata da sé; qui
    ''' si spegne tutto il resto che ancora lo mostrava — il dialogo guidato, che
    ''' altrimenti riproporrebbe un profilo cancellato, e il 📄 CV base in mostra fra i
    ''' documenti. Le candidature <b>non</b> si toccano: sono l'altra metà dei dati
    ''' dell'utente e restano nella Home, che le rilegge e basta.
    ''' </summary>
    Private Sub pnlProfilo_ProfiloEliminato(sender As Object, e As EventArgs) _
        Handles pnlProfilo.ProfiloEliminato

        pnlDialogo.Dimentica()
        pnlDocumenti.DimenticaIlCvBase()
        pnlHome.Aggiorna()

    End Sub

    Private Sub pnlDocumenti_LavoroAiCambiato(sender As Object, e As EventArgs) _
        Handles pnlDocumenti.LavoroAiCambiato

        BarraDiNavigazione(libera:=Not pnlDocumenti.AiAlLavoro)

    End Sub

    ''' <summary>
    ''' Come per il dialogo: mentre l'AI legge un annuncio la barra si blocca, altrimenti
    ''' da qui si aggirerebbe la guardia del pannello (cap. 02.6).
    ''' </summary>
    Private Sub pnlOpportunita_LavoroAiCambiato(sender As Object, e As EventArgs) _
        Handles pnlOpportunita.LavoroAiCambiato

        BarraDiNavigazione(libera:=Not pnlOpportunita.AiAlLavoro)

    End Sub

    ''' <summary>
    ''' Dalla scheda del profilo al dialogo guidato. Il bottone della barra resta quello
    ''' del profilo: P5 non è un'altra destinazione, è un altro modo di riempire P2.
    ''' </summary>
    Private Async Sub pnlProfilo_DialogoRichiesto(sender As Object, e As EventArgs) _
        Handles pnlProfilo.DialogoRichiesto

        MostraPannello(pnlDialogo, btnProfilo)
        Await pnlDialogo.ApriIlDialogoAsync()

    End Sub

    Private Sub pnlDialogo_TornaAlProfilo(sender As Object, e As EventArgs) Handles pnlDialogo.TornaAlProfilo
        MostraPannello(pnlProfilo, btnProfilo)
    End Sub

    ''' <summary>
    ''' Il profilo costruito parlando arriva nella scheda, dove l'utente lo controlla e
    ''' lo salva. Se preferisce tenersi le correzioni che aveva in sospeso, la scheda
    ''' rifiuta la proposta e si resta nel dialogo, che è ancora tutto lì.
    ''' </summary>
    Private Sub pnlDialogo_ProfiloPronto(sender As Object, e As EventArgs) Handles pnlDialogo.ProfiloPronto

        If Not pnlProfilo.ProponiProfilo(pnlDialogo.ProfiloCostruito, "dal dialogo guidato") Then Return

        ' Da qui il racconto è al sicuro nella scheda: il dialogo lo sa, così la
        ' chiusura non avvisa più per un profilo già consegnato.
        pnlDialogo.SegnaConsegnato()

        MostraPannello(pnlProfilo, btnProfilo)

    End Sub

    ''' <summary>
    ''' «Mentre l'AI lavora non si esce» vale anche per la barra: senza questo blocco,
    ''' da qui si aggirava la guardia del pannello e si poteva lanciare un import
    ''' concorrente mentre un turno era in volo.
    ''' </summary>
    Private Sub pnlDialogo_LavoroAiCambiato(sender As Object, e As EventArgs) _
        Handles pnlDialogo.LavoroAiCambiato

        BarraDiNavigazione(libera:=Not pnlDialogo.AiAlLavoro)

    End Sub

    ''' <summary>
    ''' Apre o chiude le destinazioni della barra mentre l'AI lavora (cap. 02.6): si
    ''' spegne <b>tutta</b>, e chi la compone è scritto in un posto solo —
    ''' <see cref="BottoniDiNavigazione"/>.
    ''' </summary>
    ''' <remarks>
    ''' Fino a T9d i bottoni erano elencati qui a mano, ed erano quattro su cinque: il
    ''' quinto restava fuori apposta, perché «⚙ Impostazioni» era spento sempre — P8 non
    ''' esisteva ancora, e riaccenderlo qui avrebbe aperto una destinazione che non c'era.
    ''' A T9b il pannello è nato e il bottone si è acceso, ma questo elenco è rimasto
    ''' quello di prima: non era un bottone dimenticato, erano <b>due elenchi</b> della
    ''' stessa cosa, e il secondo è invecchiato in silenzio. Da lì si esce in tre
    ''' modi che una chiamata in volo non regge: i dati eliminati (che chiudono
    ''' l'applicazione mentre una generazione ci scrive dentro, cap. 11.5), una chiave
    ''' nuova (che smonta e rimonta il contesto sotto i piedi di chi lo sta usando,
    ''' cap. 11.3) e la cartella dei documenti (che manda in P7 e avvia un secondo giro
    ''' di AI, cap. 05.2).
    ''' </remarks>
    Private Sub BarraDiNavigazione(libera As Boolean)

        For Each navigazione As Button In BottoniDiNavigazione()
            navigazione.Enabled = libera
        Next

        ' La barra di stato è l'unico posto della finestra che parla per tutti i pannelli,
        ' e fino al 2026-08-27 diceva «Pronto» dall'avvio alla chiusura. Adesso, mentre
        ' l'AI lavora, lo dice — e si muove, perché è il muoversi a dire che il programma
        ' è vivo (reperto D-R2 del giro D).
        SegnalaCheLAiLavora(Not libera)

    End Sub

    ''' <summary>Il segnale d'attesa in corso, quando ce n'è uno.</summary>
    Private _segnaleDiAttesa As SegnaleDiAttesa

    ''' <summary>Il battito che lo fa muovere.</summary>
    Private _battitoDellAttesa As System.Windows.Forms.Timer

    ''' <summary>
    ''' Accende o spegne il segnale d'attesa nella barra di stato. La logica di che cosa
    ''' scrivere sta in <see cref="SegnaleDiAttesa"/>, che si collauda senza finestre: qui
    ''' resta solo il battito e l'etichetta da riempire.
    ''' </summary>
    Private Sub SegnalaCheLAiLavora(inCorso As Boolean)

        If inCorso Then

            If _segnaleDiAttesa IsNot Nothing AndAlso _segnaleDiAttesa.InCorso Then Return

            _segnaleDiAttesa = New SegnaleDiAttesa(lblStato.Text)

            If _battitoDellAttesa Is Nothing Then
                _battitoDellAttesa = New System.Windows.Forms.Timer() With {
                    .Interval = SegnaleDiAttesa.IntervalloInMillisecondi}
                AddHandler _battitoDellAttesa.Tick, AddressOf UnBattitoDellAttesa
            End If

            lblStato.ForeColor = StileApp.Accento
            lblStato.Text = _segnaleDiAttesa.Avvia(Date.Now)
            _battitoDellAttesa.Start()

        Else

            If _segnaleDiAttesa Is Nothing OrElse Not _segnaleDiAttesa.InCorso Then Return

            _battitoDellAttesa?.Stop()
            lblStato.Text = _segnaleDiAttesa.Ferma()
            lblStato.ForeColor = StileApp.TestoSecondario

        End If

    End Sub

    Private Sub UnBattitoDellAttesa(mittente As Object, e As EventArgs)
        lblStato.Text = _segnaleDiAttesa.Battito(Date.Now)
    End Sub

    ''' <summary>
    ''' Prima di chiudere, l'unica domanda che vale la pena fare: il profilo è la sola
    ''' cosa che l'utente non può rigenerare (cap. 11.1), e chiuderlo con delle
    ''' correzioni ancora in memoria — o con un racconto lasciato a metà — le butterebbe
    ''' via in silenzio.
    ''' </summary>
    Private Sub FormPrincipale_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing

        Dim inSospeso As New List(Of String)

        If pnlProfilo.HaModificheNonSalvate Then
            inSospeso.Add("correzioni al profilo che non hai ancora salvato")
        End If

        If pnlDialogo.HaUnDialogoInCorso Then
            inSospeso.Add("un dialogo cominciato e non finito")
        End If

        ' Il caso peggiore: il dialogo è arrivato in fondo ma il profilo non è mai
        ' stato portato nella scheda — non risulta «in corso», eppure vive solo in
        ' memoria e chiudere adesso lo perderebbe tutto.
        If pnlDialogo.HaUnRaccontoNonConsegnato Then
            inSospeso.Add("un profilo costruito col dialogo e non ancora portato nella scheda")
        End If

        If pnlProfilo.HaUnaLetturaInCorso Then
            inSospeso.Add("una lettura del CV ancora in corso")
        End If

        If pnlOpportunita.AiAlLavoro Then
            inSospeso.Add("un annuncio che sto ancora leggendo e confrontando")
        End If

        If pnlDocumenti.AiAlLavoro Then
            inSospeso.Add("dei documenti che sto ancora scrivendo")
        End If

        If pnlEmail.AiAlLavoro Then
            inSospeso.Add("un'email che sto ancora scrivendo")
        End If

        If inSospeso.Count = 0 Then Return

        Dim risposta As DialogResult = MessageBox.Show(
            $"Hai {String.Join(" e ", inSospeso)}." & vbLf &
            "Se chiudi adesso, quel lavoro va perso. Vuoi chiudere lo stesso?",
            Me.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)

        e.Cancel = (risposta <> DialogResult.Yes)

    End Sub

    Private Sub FormPrincipale_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ' Una chiamata ancora in volo si annulla prima di smaltire il client: muore per
        ' la via pulita dell'annullo, non su un HttpClient disposto.
        pnlProfilo.AnnullaLaLettura()
        pnlOpportunita.AnnullaIlLavoro()
        pnlDocumenti.AnnullaIlLavoro()
        pnlEmail.AnnullaIlLavoro()

        ' Il motore del browser si spegne con la finestra: è l'unica cosa che questa
        ' finestra possiede oltre al contesto.
        _stampante?.Dispose()
        _contesto?.Dispose()

        ' Il lucchetto per ultimo: finché c'è qualcosa da smaltire, questa sessione ha
        ' ancora dei dati in mano.
        _lucchetto?.Dispose()
    End Sub

    ''' <summary>
    ''' Chiede la chiave API quando non ce n'è una, o quando l'avvio l'ha chiesto con
    ''' <c>--chiave</c> (cap. 11.3). Se l'utente ne dà una, il motore si <b>rimonta</b>:
    ''' è il modo di accendere tutti i servizi che dalla chiave dipendono senza
    ''' inseguirli uno per uno.
    ''' </summary>
    ''' <remarks>
    ''' Finché le Impostazioni non ci sono (T9) questa finestra è l'unico posto in cui la
    ''' chiave si digita, e <c>--chiave</c> l'unico modo di richiamarla: per questo la
    ''' richiesta esplicita vale anche a chiave presente.
    ''' </remarks>
    ''' <summary>
    ''' Mostra l'informativa la prima volta che il programma parte su questa cartella dati
    ''' (cap. 11.2), e se la annota per non rifarlo più.
    ''' </summary>
    ''' <remarks>
    ''' <para>Sta prima della chiave perché quello è il momento della decisione: chi ha
    ''' appena incollato una chiave a pagamento ha già scelto di fidarsi, e dirgli allora
    ''' che cosa esce dal suo PC è arrivare tardi.</para>
    ''' <para>Se il file delle preferenze non si lascia scrivere l'informativa <b>ricompare
    ''' al prossimo avvio</b>: un fastidio, ma dalla parte giusta — il dubbio va a favore di
    ''' chi deve essere informato, non della nostra voglia di non ripeterci.</para>
    ''' </remarks>
    Private Sub MostraLInformativaLaPrimaVolta()

        If _contesto.Impostazioni.InformativaVista Then Return

        ' Come per la chiave: la schermata di avvio non può restare davanti a una finestra
        ' che aspetta una risposta.
        _schermataDiAvvio?.ChiudiSubito()

        FinestraInformativa.Mostra(Me)

        _contesto.Impostazioni.InformativaVista = True

        Try
            _contesto.ArchivioImpostazioni.Salva(_contesto.Impostazioni)
        Catch ex As Exception When TypeOf ex Is System.IO.IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException
            ' Non è un guasto che meriti una finestra: l'unica conseguenza è che
            ' l'informativa si rivedrà, e non è un danno.
            Dati.DiarioTecnico.Corrente?.AnnotaGuasto("il salvataggio dell'informativa vista", ex)
        End Try

    End Sub

    Private Sub ChiediLaChiaveApiSeServe()

        If _contesto.Client IsNot Nothing AndAlso Not _argomenti.ChiediLaChiave Then Return

        ' Da qui in poi si apre una finestra che aspetta una risposta, e la schermata di
        ' avvio non può restarle davanti: il suo tempo minimo vale per chi guarda, non
        ' per chi deve rispondere. Sta dentro questo metodo, dopo la sua condizione, e
        ' non nel Load con una condizione gemella: due condizioni che dicono la stessa
        ' cosa in due posti sono il difetto che T9d ha già pagato una volta.
        _schermataDiAvvio?.ChiudiSubito()

        Dim illeggibile As Boolean
        Dim digitata As String = FinestraChiaveApi.Chiedi(Me, _contesto.Segreti.LeggiChiaveApi(illeggibile),
                                                     Function(daProvare As String) Ai.ProvaChiave.ProvaAsync(daProvare))
        If digitata Is Nothing Then Return

        ' Se il salvataggio non riesce — disco pieno, cartella di sola lettura — la
        ' chiave vale lo stesso per questa sessione: l'utente l'ha data per lavorare
        ' adesso, e perderla in silenzio sarebbe il modo peggiore di reagire.
        Dim salvata As Boolean = SalvaLaChiave(digitata)

        _contesto.Dispose()
        _contesto = ContestoApp.Monta(_argomenti.RadiceDati, If(salvata, Nothing, digitata))

    End Sub

    ''' <summary>Scrive la chiave cifrata e dice se c'è riuscita.</summary>
    Private Function SalvaLaChiave(chiave As String) As Boolean

        Try
            _contesto.Segreti.SalvaChiaveApi(chiave)
            Return True

        Catch ex As Exception When TypeOf ex Is System.IO.IOException OrElse
                                   TypeOf ex Is UnauthorizedAccessException OrElse
                                   TypeOf ex Is System.Security.Cryptography.CryptographicException

            MessageBox.Show(
                "Non sono riuscita a salvare la chiave sul disco:" & vbLf & ex.Message & vbLf & vbLf &
                "La uso lo stesso per questa sessione, ma alla prossima apertura te la richiederò.",
                "TrovaLavoro", MessageBoxButtons.OK, MessageBoxIcon.Warning)

            Return False

        End Try

    End Function

    ''' <summary>
    ''' Racconta com'è andato il montaggio: la riga «Ver. 0.3.003 · Pool 1.00
    ''' (integrato)» del pannello logo (cap. 03.5) e, nella barra di stato, l'unica cosa
    ''' che l'utente deve sapere subito — se c'è. L'etichetta del pool dichiara da sé
    ''' sorgente e stato (esterna, integrata, o con l'asterisco dei file modificati); il
    ''' «Pool —» resta per l'anomalia totale, quando la libreria non si è aperta affatto.
    ''' </summary>
    Private Sub MostraLoStatoDellAvvio()
        lblVersione.Text = Versione.Riga(EtichettaDelPool())
        lblStato.Text = If(AvvisoDellAvvio(), "Pronto")
    End Sub

    ''' <summary>
    ''' Come si chiama la libreria in vigore: la dichiara lei stessa — sorgente e stato,
    ''' asterisco compreso (cap. 04.5) — e il «Pool —» resta per l'anomalia totale,
    ''' quando non si è aperta affatto. Sta in un metodo perché la leggono in due: il
    ''' pannello del logo e «Informazioni su…».
    ''' </summary>
    Private Function EtichettaDelPool() As String
        Return If(_contesto?.Libreria IsNot Nothing, _contesto.Libreria.Etichetta, "Pool —")
    End Function

    ''' <summary>
    ''' Tutto ciò che l'utente deve sapere appena aperta la finestra, in un ordine che
    ''' non è casuale: prima <b>dove</b> si sta lavorando — se non è il posto di sempre,
    ''' cambia il senso di tutto il resto — poi quel che la riga di comando non ha potuto
    ''' rispettare, e infine quel che il motore ha trovato montandosi.
    ''' </summary>
    Private Function AvvisoDellAvvio() As String

        Dim voci As New List(Of String)

        If Not _contesto.Cartella.SullaRadicePredefinita Then
            voci.Add($"Cartella dati: {_contesto.Cartella.Radice}")
        End If

        If _argomenti.Avviso IsNot Nothing Then voci.Add(_argomenti.Avviso)
        If _contesto.Avviso IsNot Nothing Then voci.Add(_contesto.Avviso)

        ' Il lucchetto in mano a qualcun altro vuol dire quasi sempre un server MCP che
        ' sta scrivendo (cap. 09.4), o una seconda copia dell'applicazione. Si dice e si
        ' tira dritto: fermare l'utente sarebbe sproporzionato, ma se poi due salvataggi
        ' si accavallano deve sapere che gliel'avevamo detto.
        If _lucchetto Is Nothing Then
            voci.Add("La cartella dati è già in uso da un altro programma: attento a non " &
                     "lavorare sugli stessi dati da due parti insieme.")
        End If

        If voci.Count = 0 Then Return Nothing
        Return String.Join(" · ", voci)

    End Function

    ''' <summary>
    ''' Una cartella dati diversa da quella di sempre si dichiara <b>nel titolo</b>
    ''' (cap. 11.1). La barra di stato lo dice già, ma la barra di stato è una riga che il
    ''' primo messaggio successivo si porta via: il titolo resta lì per tutta la sessione,
    ''' ed è quello che si legge tornando alla finestra un'ora dopo, quando ci si è
    ''' dimenticati con quale comando la si era aperta.
    ''' </summary>
    Private Sub DichiaraLaCartellaDati()

        If _contesto.Cartella.SullaRadicePredefinita Then Return

        Me.Text = $"{Me.Text} — dati in «{_contesto.Cartella.Radice}»"

    End Sub

    Private Sub FormPrincipale_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
        AggiornaPannelloLogo()
    End Sub

    ''' <summary>
    ''' Tiene il pannello logo incollato all'angolo in basso a sinistra e sceglie fra
    ''' modalità piena e compatta. Il pannello è flottante sopra la struttura, quindi
    ''' la sua posizione si ricalcola a ogni ridimensionamento.
    ''' </summary>
    Private Sub AggiornaPannelloLogo()
        Dim compatta As Boolean = ScalaSchermo.ModalitaCompatta(
            Me.ClientSize.Width, Me.DeviceDpi, LarghezzaModalitaCompatta)

        If Not compattaAttiva.HasValue OrElse compattaAttiva.Value <> compatta Then
            compattaAttiva = compatta
            DisponiPannelloLogo(compatta)
        End If

        pnlLogo.Location = New Point(0, Me.ClientSize.Height - pnlLogo.Height)

        ' L'ingombro si dichiara qui, e non dentro DisponiPannelloLogo, perché il pannello
        ' vero lo dimensiona WinForms e non alla stessa svolta in cui cambia la modalità:
        ' quel che serve ai pannelli è la misura che ha adesso (decisione 15.7).
        DichiaraLIngombroDelLogo()
    End Sub

    ''' <summary>Dispone i contenuti del pannello logo nella modalità richiesta.</summary>
    Private Sub DisponiPannelloLogo(compatta As Boolean)
        Dim larghezza As Integer = If(compatta, LogoLarghezzaCompatta, LogoLarghezza)
        Dim altezza As Integer = If(compatta, LogoAltezzaCompatta, LogoAltezza)
        Dim lato As Integer = If(compatta, LogoLatoImmagineCompatta, LogoLatoImmagine)
        Dim margine As Integer = If(compatta, StileApp.InterlineaMinima, StileApp.MargineRiquadro)

        pnlLogo.SuspendLayout()

        pnlLogo.Size = New Size(larghezza, altezza)

        Dim immaginePrecedente As Image = picLogo.Image
        picLogo.SetBounds((larghezza - lato) \ 2, margine, lato, lato)
        picLogo.Image = LogoAviolab.Genera(lato)
        immaginePrecedente?.Dispose()

        ' In compatta restano solo l'immagine ridotta e la versione.
        lblMarchio.Visible = Not compatta
        lblCopyright.Visible = Not compatta

        Dim riga As Integer = margine + lato + StileApp.InterlineaMinima
        If Not compatta Then
            lblMarchio.SetBounds(0, riga, larghezza, AltezzaRigaNome)
            riga += AltezzaRigaNome + 2
        End If
        lblVersione.SetBounds(0, riga, larghezza, AltezzaRigaDidascalia)
        If Not compatta Then
            lblCopyright.SetBounds(0, riga + AltezzaRigaDidascalia + 2, larghezza, AltezzaRigaDidascalia)
        End If

        ' La barra di stato scrive a destra del pannello logo, che le sta sopra.
        lblStato.Padding = New Padding(larghezza + StileApp.DistanzaControlli, 0,
                                       StileApp.DistanzaControlli, 0)

        pnlLogo.ResumeLayout()

    End Sub

    ''' <summary>
    ''' Dice ai pannelli dell'area centrale quanto spazio si prende il logo. Il pannello
    ''' logo è flottante ed è molto più alto della fascia di stato, quindi copre l'angolo
    ''' in basso a sinistra dell'area centrale: quel rettangolo non è disponibile, e ogni
    ''' pannello deve saperlo per non metterci dentro dei dati (v. <see cref="IPannelloArea"/>).
    ''' </summary>
    Private Sub DichiaraLIngombroDelLogo()

        ' Si misura il pannello invece di ripetere le costanti di geometria: quelle sono in
        ' unità di progetto, il pannello vero lo scala WinForms, e a 150% dichiarare 261×216
        ' dove il pannello ne occupa 373×360 sfondava nell'area viva (cap. 03.5).
        Dim sfondamento As Integer = Math.Max(0, pnlLogo.Height - pnlFasciaInferiore.Height)
        Dim ingombro As New Size(pnlLogo.Width, sfondamento)

        If ingombro = ingombroDichiarato Then Return
        ingombroDichiarato = ingombro

        For Each figlio As Control In pnlAreaCentrale.Controls
            Dim pannello As IPannelloArea = TryCast(figlio, IPannelloArea)
            pannello?.ImpostaIngombroLogo(ingombro)
        Next

    End Sub

End Class
