Imports System.Drawing
Imports System.Windows.Forms
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

    ''' <summary>Il motore montato all'avvio: da qui in avanti lo usano i pannelli.</summary>
    Private _contesto As ContestoApp

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

    ''' <summary>Da quale pannello si è arrivati ai documenti, e con quale bottone della barra.</summary>
    Private _ritornoDaiDocumenti As Control
    Private _bottoneDelRitorno As Button

    Private Sub FormPrincipale_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        _contesto = ContestoApp.Monta()
        MostraLoStatoDellAvvio()

        ' La barra di navigazione è fatta di bottoni neutri (livello 0, cap. 03.3): li
        ' veste StileApp, che sa anche come si spegne un bottone.
        For Each navigazione As Button In BottoniDiNavigazione()
            StileApp.VestiBottone(navigazione, LivelloBottone.Neutro)
        Next

        DichiaraLeTappeCheMancano()

        ' Un motore del browser per tutta l'applicazione, e da lui la stampante: due
        ' ambienti sulla stessa cartella di navigazione stanno buoni solo finché nessuno
        ' cambia loro un'opzione (v. MotoreBrowser, cancello di T5a).
        _motoreBrowser = New MotoreBrowser(_contesto.Cartella.CartellaWebView2)
        _stampante = New StampantePdf(_motoreBrowser)

        pnlProfilo.Collega(_contesto)
        pnlDialogo.Collega(_contesto)
        pnlOpportunita.Collega(_contesto)
        pnlDocumenti.Collega(_contesto, New ArchivioDocumenti(_contesto.Cartella, _stampante))
        pnlRicerca.Collega(_contesto, _motoreBrowser)

        pnlLogo.BringToFront()
        AggiornaPannelloLogo()

        ' Finché il cruscotto (P1) non esiste, la casa è il profilo: è comunque il primo
        ' passo del flusso A, e senza di lui non si fa nient'altro.
        MostraPannello(pnlProfilo, btnProfilo)
    End Sub

    ''' <summary>
    ''' I pannelli che arrivano con le tappe successive restano visibili e spenti, con
    ''' scritto quando arrivano: un bottone che sparisce non insegna niente, uno spento
    ''' che si spiega dice come è fatto il programma.
    ''' </summary>
    Private Sub DichiaraLeTappeCheMancano()

        btnHome.Enabled = False
        ttSuggerimenti.SetToolTip(btnHome, "Il cruscotto con il registro delle candidature arriva con la tappa T5.")

        btnImpostazioni.Enabled = False
        ttSuggerimenti.SetToolTip(btnImpostazioni, "La finestra delle impostazioni arriva con la tappa T9.")

    End Sub

    ''' <summary>
    ''' Mostra uno dei pannelli dell'area centrale, uno solo per volta (cap. 03.4), e
    ''' segna quale bottone della barra lo sta mostrando.
    ''' </summary>
    Private Sub MostraPannello(pannello As Control, bottone As Button)

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

    Private Function BottoniDiNavigazione() As Button()
        Return {btnHome, btnProfilo, btnRicerca, btnCandidatura, btnImpostazioni}
    End Function

    Private Sub btnProfilo_Click(sender As Object, e As EventArgs) Handles btnProfilo.Click
        MostraPannello(pnlProfilo, btnProfilo)
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

    Private Sub pnlDocumenti_TornaIndietro(sender As Object, e As EventArgs) _
        Handles pnlDocumenti.TornaIndietro

        ' Si torna da dove si è venuti: dai documenti si esce verso l'opportunità o verso
        ' la scheda del profilo, a seconda di quale delle due ci ha mandati qui.
        MostraPannello(If(_ritornoDaiDocumenti, pnlOpportunita),
                       If(_bottoneDelRitorno, btnCandidatura))

    End Sub

    ''' <summary>Porta ai documenti, ricordandosi la strada da cui si è arrivati.</summary>
    Private Sub VaiAiDocumenti(daDove As Control, conQualeBottone As Button)

        _ritornoDaiDocumenti = daDove
        _bottoneDelRitorno = conQualeBottone

        MostraPannello(pnlDocumenti, conQualeBottone)

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
    ''' Apre o chiude le destinazioni raggiungibili. Le altre restano spente comunque: le
    ''' spegne la tappa che ancora non è arrivata, e non tocca a questa guardia riaprirle.
    ''' </summary>
    Private Sub BarraDiNavigazione(libera As Boolean)

        btnProfilo.Enabled = libera
        btnCandidatura.Enabled = libera
        btnRicerca.Enabled = libera

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

        ' Il motore del browser si spegne con la finestra: è l'unica cosa che questa
        ' finestra possiede oltre al contesto.
        _stampante?.Dispose()
        _contesto?.Dispose()
    End Sub

    ''' <summary>
    ''' Racconta com'è andato il montaggio: la riga «Ver. 0.3.003 · Pool 1.00
    ''' (integrato)» del pannello logo (cap. 03.5) e, nella barra di stato, l'unica cosa
    ''' che l'utente deve sapere subito — se c'è. L'etichetta del pool dichiara da sé
    ''' sorgente e stato (esterna, integrata, o con l'asterisco dei file modificati); il
    ''' «Pool —» resta per l'anomalia totale, quando la libreria non si è aperta affatto.
    ''' </summary>
    Private Sub MostraLoStatoDellAvvio()
        Dim etichettaPool As String = If(_contesto.Libreria IsNot Nothing,
                                         _contesto.Libreria.Etichetta, "Pool —")
        lblVersione.Text = $"Ver. {Versione.Numero} · {etichettaPool}"
        lblStato.Text = If(_contesto.Avviso, "Pronto")
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
        Dim compatta As Boolean = (Me.ClientSize.Width < LarghezzaModalitaCompatta)

        If Not compattaAttiva.HasValue OrElse compattaAttiva.Value <> compatta Then
            compattaAttiva = compatta
            DisponiPannelloLogo(compatta)
        End If

        pnlLogo.Location = New Point(0, Me.ClientSize.Height - pnlLogo.Height)
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

        DichiaraLIngombroDelLogo(larghezza, altezza)
    End Sub

    ''' <summary>
    ''' Dice ai pannelli dell'area centrale quanto spazio si prende il logo. Il pannello
    ''' logo è flottante ed è molto più alto della fascia di stato, quindi copre l'angolo
    ''' in basso a sinistra dell'area centrale: quel rettangolo non è disponibile, e ogni
    ''' pannello deve saperlo per non metterci dentro dei dati (v. <see cref="IPannelloArea"/>).
    ''' </summary>
    Private Sub DichiaraLIngombroDelLogo(larghezza As Integer, altezza As Integer)

        Dim sfondamento As Integer = Math.Max(0, altezza - pnlFasciaInferiore.Height)
        Dim ingombro As New Size(larghezza, sfondamento)

        For Each figlio As Control In pnlAreaCentrale.Controls
            Dim pannello As IPannelloArea = TryCast(figlio, IPannelloArea)
            pannello?.ImpostaIngombroLogo(ingombro)
        Next

    End Sub

End Class
