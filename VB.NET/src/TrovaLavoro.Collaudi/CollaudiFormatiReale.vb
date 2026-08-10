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
    ''' L'altra metà del collaudo di tappa di T3 (cap. 14): lo <b>stesso CV</b> entra
    ''' nell'app dalle quattro porte che l'app dichiara di avere — PDF, DOCX, TXT e MD —
    ''' e ne deve uscire lo stesso profilo.
    ''' </summary>
    ''' <remarks>
    ''' <para>La domanda è diversa da quella di <see cref="CollaudiImportReale"/>. Lì si
    ''' chiede se la nuova app legge il CV come lo leggeva il prototipo; qui il prototipo
    ''' non c'entra, e si chiede se le <b>tre strade di lettura</b> — la trascrizione
    ''' dell'AI per il PDF, l'archivio ZIP per il DOCX, il disco per il testo semplice —
    ''' consegnano al passo 2 la stessa sostanza. Il PDF fa da riferimento perché è la
    ''' via principale, quella per cui il prompt è stato scritto.</para>
    ''' <para>Il pass/fail sta dove sta sempre: sui campi che il CV scrive nero su bianco
    ''' e che il prompt ordina di <i>copiare</i> — nome, email, telefono, <b>città</b>,
    ''' link, patente. Lì una differenza fra due formati non è varianza del modello: vuol
    ''' dire che una delle strade ha consegnato un testo mutilo. Sui conteggi vale la
    ''' tolleranza già dichiarata in <see cref="CollaudiImportReale.TolleranzaVoci"/>,
    ''' sulle competenze e sul numero delle esperienze informali non si pretende un numero
    ''' (<see cref="CollaudoReale.PerchePerLeInformali"/>), e il resto si legge nel
    ''' rapporto.</para>
    ''' <para><b>La città è tornata fra i pass/fail</b> — qui per ultima, ed è un
    ''' allineamento rimasto indietro dal <b>Pool 1.02</b>. La ragione per cui era solo
    ''' segnalata vale fra l'app e il prototipo, che leggono lo stesso CV con prompt
    ''' diversi (<see cref="CollaudoReale.PerchePerLaCitta"/>); qui le quattro strade
    ''' usano tutte lo stesso prompt, quello che decide — la città è il <b>domicilio</b>,
    ''' una sola. Due città diverse fra le strade vogliono dire che una lettura ha perso
    ''' l'indirizzo, e in più ogni strada risponde del proprio: la città che consegna
    ''' dev'essere quella del domicilio dichiarato nel testo che ha letto.</para>
    ''' <para>Un pass/fail vale su <b>ogni</b> strada, PDF compreso, perché non dipende da
    ''' come il file è fatto: la stessa attività non può stare sia fra le esperienze
    ''' formali sia fra le informali. Dal <b>pool 1.01</b> il prompt lo dice, e
    ''' <see cref="ControlloDoppioni"/> lo verifica.</para>
    ''' <para><b>Quello che questo collaudo non prova.</b> I file DOCX, TXT e MD non sono
    ''' quattro stesure indipendenti dello stesso curriculum: nascono dal testo che l'AI
    ''' ha trascritto dal PDF vero, rivestito come lo vestirebbe il suo programma — il
    ''' DOCX con tabella d'intestazione, controllo contenuto, un campo calcolato e una
    ''' revisione cancellata; il TXT con CRLF e la codifica ANSI di Windows; il MD in
    ''' UTF-8 con la sua marcatura. Prova quindi che le strade di lettura non perdono e
    ''' non aggiungono nulla, <b>non</b> che Word impagini come impagina il PDF: per
    ''' quello servirebbe un DOCX salvato da Word, che su questa postazione non c'è
    ''' (<c>in_sospeso.md</c>).</para>
    ''' <para>Come l'altro: categoria <b>Reale</b>, il CV fuori dal repo indicato da
    ''' <c>CV_DI_PROVA</c>, il rapporto scritto lì accanto. Il prototipo qui <b>non
    ''' serve</b>: si parla solo con l'API.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFormatiReale

        ''' <summary>Il rapporto, accanto al CV perché contiene dati personali.</summary>
        Private Const NomeRapporto As String = "rapporto-collaudo-T3-formati.md"

        ''' <summary>
        ''' Le estensioni dei compagni del PDF, nell'ordine in cui si provano. Si cercano
        ''' per <b>nome di file uguale</b> a quello del PDF: nella cartella del CV ci sono
        ''' anche i rapporti, che sono <c>.md</c> e non c'entrano nulla.
        ''' </summary>
        Private Shared ReadOnly Compagni As String() = {".docx", ".txt", ".md"}

        ''' <summary>Sotto quanti caratteri il testo letto non è un CV.</summary>
        Private Const CaratteriMinimi As Integer = 1000

        ''' <summary>
        ''' Quanto può differire dal PDF il testo che esce da un'altra strada, contato per
        ''' righe in comune — righe appaiate <b>per contenuto</b>, cioè per sole lettere e
        ''' cifre, perché fra formati diversi la punteggiatura di riga è marcatura.
        ''' </summary>
        ''' <remarks>
        ''' La soglia resta più bassa di quella fra due trascrizioni (95%) perché una
        ''' differenza vera ci sta ed è voluta: una tabella d'intestazione mette su una
        ''' riga sola quello che il PDF scrive su tre, e una tabulazione ne spezza una in
        ''' due. Sotto questa soglia, però, non è più impaginazione: è testo che si è
        ''' perso per strada. La prima misura, col metro sbagliato — riga per riga alla
        ''' lettera — dava 43% sul markdown e 84% sul DOCX: stava contando i trattini
        ''' degli elenchi.
        ''' </remarks>
        Private Const SomiglianzaMinima As Double = 0.6

        ''' <summary>Cosa è uscito da una delle strade di lettura.</summary>
        Private Class Lettura

            ''' <summary>Il formato, come si chiama nel rapporto: PDF, DOCX, TXT, MD.</summary>
            Public Property Formato As String

            ''' <summary>Il nome del file letto.</summary>
            Public Property Nome As String

            ''' <summary>Il testo che la strada ha consegnato al passo 2.</summary>
            Public Property Testo As String

            ''' <summary>Il profilo che ne è uscito.</summary>
            Public Property Profilo As Profilo

            ''' <summary>I valori del profilo che nel testo letto non si ritrovano.</summary>
            Public Property Inventate As CollaudoReale.Invenzioni

            ''' <summary>Le voci contate due volte.</summary>
            Public Property Doppioni As List(Of String)

            ''' <summary>Le attività dichiarate volontarie finite fra i lavori.</summary>
            Public Property MalCollocate As List(Of String)

            ''' <summary>Quanto il testo letto somiglia a quello del PDF.</summary>
            Public Property ControIlPdf As CollaudoReale.ConfrontoElenchi

        End Class

        <TestMethod, TestCategory("Reale")>
        Public Async Function LeStradeDiLetturaPortanoAlloStessoProfilo() As Task

            Dim pdf As String = CollaudoReale.CvInPdfOppureRinuncia()
            Dim chiave As String = CollaudoReale.ChiaveOppureRinuncia()

            Dim daProvare As List(Of String) = FileDaProvare(pdf)

            If daProvare.Count = 1 Then
                Assert.Inconclusive(
                    $"Collaudo dei formati saltato: accanto a «{Path.GetFileName(pdf)}» non c'è " &
                    $"nessuno degli altri formati ({String.Join(", ", Compagni)}) con lo stesso nome.")
            End If

            Dim libreria As LibreriaPrompt = CollaudoReale.PoolIntegrato()

            Using client As New ClientClaude(chiave)

                Dim importatore As New ImportProfilo(
                    New StrutturatoreTurni(libreria, client),
                    New TrascrittorePdf(libreria, client))

                ' Una strada alla volta, non tutte insieme: sono chiamate all'AI, e
                ' lanciarle in parallelo vorrebbe dire misurare anche il traffico.
                Dim letture As New List(Of Lettura)
                For Each percorso As String In daProvare
                    letture.Add(Await LeggiAsync(importatore, percorso, letture.FirstOrDefault()))
                Next

                ' Il PDF è il riferimento: è la via principale, quella per cui il prompt
                ' è stato scritto, ed è l'unica che non nasce da un file fabbricato.
                Dim riferimento As Lettura = letture(0)
                Dim altre As List(Of Lettura) = letture.Skip(1).ToList()

                ' Il rapporto si scrive PRIMA di giudicare: se un Assert ferma tutto, la
                ' prova di com'era andata resta su disco comunque.
                Dim dove As String = Scrivi(pdf, letture)
                Console.WriteLine(Riassunto(dove, letture))

                For Each lettura As Lettura In letture

                    Assert.IsGreaterThan(CaratteriMinimi, lettura.Testo.Length,
                        $"[{lettura.Formato}] da «{lettura.Nome}» è uscito troppo poco testo " &
                        "per essere un CV")

                    CollaudiImportReale.VerificaProfiloPieno(lettura.Formato, lettura.Profilo)

                    Assert.IsEmpty(lettura.Inventate.Gravi,
                        $"[{lettura.Formato}] nel profilo ci sono valori che nel testo letto non " &
                        $"compaiono: {String.Join(" · ", lettura.Inventate.Gravi)}")

                    ' Valgono per ogni strada, PDF compreso: il pool 1.01 dice dove va
                    ' un'attività, e questo non dipende da come il file è fatto.
                    Assert.IsEmpty(lettura.Doppioni,
                        $"[{lettura.Formato}] la stessa attività è contata due volte, fra le " &
                        $"esperienze formali e fra le informali: {String.Join(" · ", lettura.Doppioni)}")

                    Assert.IsEmpty(lettura.MalCollocate,
                        $"[{lettura.Formato}] un'attività che si dichiara volontaria è finita fra " &
                        $"i lavori: {String.Join(" · ", lettura.MalCollocate)}")

                    ' La città, giudicata sul testo di quella strada. Dal pool 1.02 il
                    ' prompt decide — è il domicilio, una sola — e qui tutte e quattro le
                    ' strade passano da quel prompt: nessuna ha la scusa di leggere con
                    ' un prompt precedente.
                    Dim cittaFuoriPosto As String = CollaudoReale.CittaFuoriDalDomicilio(
                        lettura.Profilo.Contatti.Citta, lettura.Testo)

                    Assert.IsEmpty(cittaFuoriPosto, $"[{lettura.Formato}] contatti.citta: {cittaFuoriPosto}")

                Next

                For Each lettura As Lettura In altre

                    ' Il testo può essere impaginato diversamente, ma non può mancare.
                    Assert.IsGreaterThanOrEqualTo(SomiglianzaMinima, lettura.ControIlPdf.Frazione,
                        $"[{lettura.Formato}] il testo letto ha in comune col PDF solo il " &
                        $"{lettura.ControIlPdf.Frazione * 100:F1}% delle righe " &
                        $"(PDF {lettura.ControIlPdf.VociPrimo}, {lettura.Formato} " &
                        $"{lettura.ControIlPdf.VociSecondo}): non è impaginazione diversa, " &
                        "è testo perso per strada. I dettagli nel rapporto.")

                    ' --- I campi che si copiano: qui non c'è tolleranza. --------------
                    StessoValore(lettura, "nome", riferimento.Profilo.Nome, lettura.Profilo.Nome)
                    StessoValore(lettura, "contatti.email",
                                 riferimento.Profilo.Contatti.Email, lettura.Profilo.Contatti.Email)
                    StessoValore(lettura, "contatti.telefono",
                                 riferimento.Profilo.Contatti.Telefono, lettura.Profilo.Contatti.Telefono)

                    ' La città sta qui insieme agli altri campi copiati, e non più fra le
                    ' cose solo segnalate: la ragione per cui era segnalata vale <b>fra
                    ' l'app e il prototipo</b>, che leggono con prompt diversi
                    ' (CollaudoReale.PerchePerLaCitta), non fra quattro strade dell'app che
                    ' usano tutte lo stesso. Qui due città diverse vogliono dire che una
                    ' delle letture ha perso l'indirizzo, ed è un difetto.
                    StessoValore(lettura, "contatti.citta",
                                 riferimento.Profilo.Contatti.Citta, lettura.Profilo.Contatti.Citta)
                    StessoValore(lettura, "contatti.link",
                                 riferimento.Profilo.Contatti.Link, lettura.Profilo.Contatti.Link)
                    StessoValore(lettura, "patente.ha",
                                 riferimento.Profilo.Patente.Ha, lettura.Profilo.Patente.Ha)
                    StessoValore(lettura, "patente.categorie",
                                 CollaudoReale.Categorie(riferimento.Profilo),
                                 CollaudoReale.Categorie(lettura.Profilo))

                    ' --- Quante voci per sezione, entro la tolleranza di sempre. ------
                    ' Le informali restano fuori dal conteggio come nell'altro collaudo:
                    ' vedi CollaudoReale.PerchePerLeInformali. Di loro, qui sopra, si
                    ' boccia il doppione.
                    Conteggio(lettura, "esperienze_formali",
                              riferimento.Profilo.EsperienzeFormali.Count,
                              lettura.Profilo.EsperienzeFormali.Count)
                    Conteggio(lettura, "formazione",
                              riferimento.Profilo.Formazione.Count,
                              lettura.Profilo.Formazione.Count)

                Next

            End Using

        End Function

        ' --- Le strade da provare ---------------------------------------------------

        ''' <summary>
        ''' Il PDF e i suoi compagni con lo stesso nome, nell'ordine di prova. Un formato
        ''' che manca non è un errore: sull'altra postazione la cartella può avere il solo
        ''' PDF, e il rapporto dirà quali strade sono state percorse.
        ''' </summary>
        Private Shared Function FileDaProvare(pdf As String) As List(Of String)

            Dim cartella As String = Path.GetDirectoryName(pdf)
            Dim base As String = Path.GetFileNameWithoutExtension(pdf)

            Dim elenco As New List(Of String) From {pdf}

            For Each estensione As String In Compagni
                Dim candidato As String = Path.Combine(cartella, base & estensione)
                If File.Exists(candidato) Then elenco.Add(candidato)
            Next

            Return elenco

        End Function

        ''' <summary>
        ''' Percorre una strada: dal file al profilo, passando per il testo. È
        ''' <see cref="ImportProfilo.DaFileAsync"/> — cioè esattamente quello che fa il
        ''' pannello quando l'utente sceglie un file, non una scorciatoia scritta qui.
        ''' </summary>
        ''' <param name="riferimento">
        ''' La lettura del PDF, per misurarci contro il testo. È <c>Nothing</c> quando la
        ''' strada in corso è proprio quella del PDF.
        ''' </param>
        Private Shared Async Function LeggiAsync(importatore As ImportProfilo, percorso As String,
                                                 riferimento As Lettura) As Task(Of Lettura)

            Dim esito As EsitoImport = Await importatore.DaFileAsync(percorso)

            Dim lettura As New Lettura With {
                .Formato = Path.GetExtension(percorso).TrimStart("."c).ToUpperInvariant(),
                .Nome = Path.GetFileName(percorso),
                .Testo = esito.TestoLetto,
                .Profilo = esito.Profilo,
                .Inventate = CollaudoReale.ValoriFuoriDalTesto(esito.Profilo, esito.TestoLetto),
                .Doppioni = ControlloDoppioni.Trova(esito.Profilo),
                .MalCollocate = ControlloCollocazione.VolontariatoFraLeFormali(esito.Profilo)}

            If riferimento IsNot Nothing Then
                ' Per contenuto, non alla lettera: fra formati diversi la punteggiatura
                ' di riga è marcatura — «- Lavoro in team» del markdown e «• Lavoro in
                ' team» del PDF sono la stessa riga. Misurarle come diverse direbbe
                ' quanto markdown c'è, non quanto CV è arrivato.
                lettura.ControIlPdf = CollaudoReale.ConfrontaTesti(
                    riferimento.Testo, lettura.Testo, perContenuto:=True)
            End If

            Return lettura

        End Function

        ' --- I giudizi --------------------------------------------------------------

        ''' <summary>Un campo che il prompt ordina di copiare: fra due strade è lo stesso.</summary>
        Private Shared Sub StessoValore(lettura As Lettura, campo As String,
                                        dalPdf As String, dalFormato As String)

            Assert.AreEqual(CollaudoReale.Ripulito(dalPdf), CollaudoReale.Ripulito(dalFormato),
                $"[{lettura.Formato}] {campo}: dal PDF esce «{CollaudoReale.Ripulito(dalPdf)}», " &
                $"da «{lettura.Nome}» «{CollaudoReale.Ripulito(dalFormato)}»")

        End Sub

        ''' <summary>Quante voci ha una sezione dalle due strade, entro la tolleranza.</summary>
        Private Shared Sub Conteggio(lettura As Lettura, sezione As String,
                                     dalPdf As Integer, dalFormato As Integer)

            Assert.IsLessThanOrEqualTo(CollaudiImportReale.TolleranzaVoci,
                Math.Abs(dalPdf - dalFormato),
                $"[{lettura.Formato}] {sezione}: {dalPdf} voci dal PDF, {dalFormato} da " &
                $"«{lettura.Nome}» — più di {CollaudiImportReale.TolleranzaVoci} di scarto non è " &
                "un giudizio diverso, è un pezzo di CV letto diversamente")

        End Sub

        ' --- Il rapporto ------------------------------------------------------------

        ''' <summary>
        ''' Il rapporto da leggere a mano, <b>accanto al CV</b> e non nel repo: dentro ci
        ''' sono i dati personali di chi ha scritto quel curriculum.
        ''' </summary>
        ''' <returns>Dove è stato scritto, che è l'unica cosa che si può dire a console.</returns>
        Private Shared Function Scrivi(pdf As String, letture As List(Of Lettura)) As String

            Dim testo As New StringBuilder()

            testo.Append("# Collaudo di tappa T3 — lo stesso CV dalle quattro porte").Append(vbLf).Append(vbLf)
            testo.Append("*Rapporto generato da `CollaudiFormatiReale`. Sta qui e non nel repo perché ").
                  Append("contiene dati personali.*").Append(vbLf).Append(vbLf)
            testo.Append($"- **CV**: `{Path.GetFileNameWithoutExtension(pdf)}`").Append(vbLf)
            testo.Append($"- **Strade percorse**: {String.Join(", ", letture.Select(Function(l) l.Formato))}").Append(vbLf)
            testo.Append($"- **Quando**: {DateTime.Now:yyyy-MM-dd HH:mm}").Append(vbLf).Append(vbLf)
            testo.Append("*Il PDF fa da riferimento: è la via principale, ed è l'unica che non nasce ").
                  Append("da un file fabbricato. Gli altri tre portano lo stesso testo vestito come lo ").
                  Append("vestirebbe il suo programma — quindi qui si misura la **strada di lettura**, ").
                  Append("non l'impaginazione di Word.*").Append(vbLf).Append(vbLf)

            testo.Append("## Passo 1 — il testo che ogni strada consegna").Append(vbLf).Append(vbLf)
            testo.Append("| formato | file | caratteri | righe | in comune col PDF |").Append(vbLf)
            testo.Append("|---|---|---|---|---|").Append(vbLf)

            For Each lettura As Lettura In letture

                Dim comune As String =
                    If(lettura.ControIlPdf Is Nothing,
                       "*(riferimento)*",
                       $"{lettura.ControIlPdf.InComune} / {lettura.ControIlPdf.VociPrimo} " &
                       $"(**{lettura.ControIlPdf.Frazione * 100:F1}%**)")

                Dim righe As Integer =
                    If(lettura.ControIlPdf Is Nothing,
                       CollaudoReale.RigheSignificative(lettura.Testo, perContenuto:=True).Count,
                       lettura.ControIlPdf.VociSecondo)

                testo.Append($"| {lettura.Formato} | `{lettura.Nome}` | {lettura.Testo.Length} | ").
                      Append($"{righe} | {comune} |").Append(vbLf)

            Next

            testo.Append(vbLf)
            testo.Append($"*Soglia: {SomiglianzaMinima * 100:F0}%. Sotto, non è impaginazione diversa: ").
                  Append("è testo perso.*").Append(vbLf).Append(vbLf)

            For Each lettura As Lettura In letture.Where(Function(l) l.ControIlPdf IsNot Nothing)
                testo.Append($"### {lettura.Formato} — cosa cambia rispetto al PDF").Append(vbLf).Append(vbLf)
                CollaudoReale.Elenco(testo, "Righe che ha solo il PDF", lettura.ControIlPdf.SoloPrimo)
                CollaudoReale.Elenco(testo, $"Righe che ha solo il {lettura.Formato}", lettura.ControIlPdf.SoloSecondo)
            Next

            testo.Append("## Passo 2 — i profili, uno per strada").Append(vbLf).Append(vbLf)
            testo.Append("*I campi copiati (nome, recapiti, patente) devono coincidere: lì una ").
                  Append("differenza non è varianza del modello. Sui conteggi vale la tolleranza di ").
                  Append($"{CollaudiImportReale.TolleranzaVoci}; sulle competenze non si pretende un ").
                  Append("numero.*").Append(vbLf).Append(vbLf)

            Intestazione(testo, letture)
            RigaCampo(testo, letture, "nome", Function(p) p.Nome)
            RigaCampo(testo, letture, "email", Function(p) p.Contatti.Email)
            RigaCampo(testo, letture, "telefono", Function(p) p.Contatti.Telefono)
            RigaCampo(testo, letture, "città", Function(p) p.Contatti.Citta)
            RigaCampo(testo, letture, "link", Function(p) p.Contatti.Link)
            RigaCampo(testo, letture, "patente", Function(p) p.Patente.Ha)
            RigaCampo(testo, letture, "categorie", AddressOf CollaudoReale.Categorie)
            RigaCampo(testo, letture, "esperienze formali",
                      Function(p) p.EsperienzeFormali.Count.ToString())
            RigaCampo(testo, letture, "esperienze informali",
                      Function(p) p.EsperienzeInformali.Count.ToString(), segnalato:=True)
            RigaCampo(testo, letture, "competenze", Function(p) p.Competenze.Count.ToString())
            RigaCampo(testo, letture, "formazione", Function(p) p.Formazione.Count.ToString())
            testo.Append(vbLf)
            testo.Append(CollaudoReale.PerchePerLeInformali).Append(vbLf).Append(vbLf)

            testo.Append("### Le competenze, rispetto al PDF").Append(vbLf).Append(vbLf)
            testo.Append("*Sul numero non c'è pass/fail: a testo identico il modello ne distilla ").
                  Append("ogni volta un po' di più o un po' di meno (misurato a T3: 18, 20, 20, 20, 21).*").
                  Append(vbLf).Append(vbLf)

            For Each lettura As Lettura In letture.Skip(1)

                Dim confronto As CollaudoReale.ConfrontoElenchi =
                    CollaudoReale.ConfrontaCompetenze(letture(0).Profilo, lettura.Profilo)

                testo.Append($"**{lettura.Formato}** — in comune col PDF: **{confronto.InComune}** ").
                      Append($"su {confronto.VociPrimo} del PDF e {confronto.VociSecondo} sue ").
                      Append($"(**{confronto.Frazione * 100:F1}%**).").Append(vbLf).Append(vbLf)

                CollaudoReale.Elenco(testo, "Solo nel PDF", confronto.SoloPrimo)
                CollaudoReale.Elenco(testo, $"Solo nel {lettura.Formato}", confronto.SoloSecondo)

            Next

            testo.Append("## Anti-invenzione").Append(vbLf).Append(vbLf)
            testo.Append("*Ogni profilo è cercato dentro il testo che la **sua** strada ha letto: ").
                  Append("è lì che si vedrebbe una strada che perde un pezzo e un modello che lo ").
                  Append("rimette a memoria.*").Append(vbLf).Append(vbLf)

            For Each lettura As Lettura In letture
                CollaudoReale.Invenzione(testo, lettura.Formato, lettura.Inventate)
            Next

            testo.Append("## La stessa cosa contata due volte").Append(vbLf).Append(vbLf)
            testo.Append("*Un ⛔ su ogni strada, PDF compreso: dal pool 1.01 `importa_cv` dice che ").
                  Append("un'attività sta in una sezione sola, e questo non dipende da come il file ").
                  Append("è fatto.*").Append(vbLf).Append(vbLf)

            For Each lettura As Lettura In letture
                CollaudoReale.Doppioni(testo, lettura.Formato, lettura.Doppioni, bocciato:=True)
            Next

            testo.Append("## Ogni attività nella sua sezione").Append(vbLf).Append(vbLf)
            testo.Append("*Il gemello del doppione: la voce sta in una sezione sola, ma è quella ").
                  Append("sbagliata — un volontariato promosso a impiego. Anche questo un ⛔ su ogni ").
                  Append("strada: a decidere è la natura dell'attività, non come il file è fatto né ").
                  Append("la sezione del CV in cui è stampata.*").Append(vbLf).Append(vbLf)

            For Each lettura As Lettura In letture
                CollaudoReale.Collocazione(testo, lettura.Formato, lettura.MalCollocate, bocciato:=True)
            Next

            testo.Append("## Da leggere a mano").Append(vbLf).Append(vbLf)
            testo.Append("I profili per intero e il testo che ogni strada ha consegnato.").Append(vbLf).Append(vbLf)

            For Each lettura As Lettura In letture
                testo.Append($"### Profilo dal {lettura.Formato}").Append(vbLf).Append(vbLf)
                testo.Append("```json").Append(vbLf).Append(lettura.Profilo.ComeTesto()).
                      Append(vbLf).Append("```").Append(vbLf).Append(vbLf)
            Next

            For Each lettura As Lettura In letture
                testo.Append($"### Testo letto dal {lettura.Formato}").Append(vbLf).Append(vbLf)
                testo.Append("```").Append(vbLf).Append(lettura.Testo).
                      Append(vbLf).Append("```").Append(vbLf).Append(vbLf)
            Next

            Dim dove As String = Path.Combine(Path.GetDirectoryName(pdf), NomeRapporto)

            File.WriteAllText(dove, testo.ToString(),
                              New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

            Return dove

        End Function

        ''' <summary>L'intestazione della tabella dei profili: una colonna per strada.</summary>
        Private Shared Sub Intestazione(testo As StringBuilder, letture As List(Of Lettura))

            testo.Append("| campo | ").Append(String.Join(" | ", letture.Select(Function(l) l.Formato))).
                  Append(" | |").Append(vbLf)
            testo.Append("|---|").Append(String.Concat(Enumerable.Repeat("---|", letture.Count + 1))).
                  Append(vbLf)

        End Sub

        ''' <summary>
        ''' Una riga della tabella: lo stesso campo letto da ogni strada, e il segno di
        ''' ciò che non torna col PDF.
        ''' </summary>
        ''' <param name="segnalato">
        ''' Il campo si guarda e non si boccia: la differenza prende il ⚠️ invece del ≠.
        ''' Resta il caso del numero delle esperienze informali
        ''' (<see cref="CollaudoReale.PerchePerLeInformali"/>); la città lo era fino al
        ''' Pool 1.02 e ora è un pass/fail come gli altri campi copiati.
        ''' </param>
        Private Shared Sub RigaCampo(testo As StringBuilder, letture As List(Of Lettura),
                                     campo As String, valore As Func(Of Profilo, String),
                                     Optional segnalato As Boolean = False)

            Dim valori As List(Of String) =
                letture.Select(Function(l) CollaudoReale.Ripulito(valore(l.Profilo))).ToList()

            Dim tutti As Boolean = valori.All(Function(v) v = valori(0))

            testo.Append($"| {campo} | ").
                  Append(String.Join(" | ", valori.Select(AddressOf CollaudoReale.PerTabella))).
                  Append(" | ").Append(CollaudoReale.Marcatore(tutti, segnalato)).
                  Append(" |").Append(vbLf)

        End Sub

        ''' <summary>
        ''' Cosa si può dire a voce alta: i numeri e dove sta il rapporto. Il contenuto
        ''' del CV resta nel file, fuori dal repo.
        ''' </summary>
        Private Shared Function Riassunto(dove As String, letture As List(Of Lettura)) As String

            Dim testo As New StringBuilder()

            testo.AppendLine("Collaudo di tappa T3 — lo stesso CV dalle quattro porte.")

            For Each lettura As Lettura In letture

                Dim comune As String =
                    If(lettura.ControIlPdf Is Nothing,
                       "riferimento",
                       $"{lettura.ControIlPdf.Frazione * 100:F1}% di righe in comune col PDF")

                testo.AppendLine(
                    $"{lettura.Formato}: {lettura.Testo.Length} caratteri, {comune}; " &
                    $"formali {lettura.Profilo.EsperienzeFormali.Count}, " &
                    $"informali {lettura.Profilo.EsperienzeInformali.Count}, " &
                    $"competenze {lettura.Profilo.Competenze.Count}, " &
                    $"formazione {lettura.Profilo.Formazione.Count}; " &
                    $"invenzioni {lettura.Inventate.Gravi.Count} gravi / " &
                    $"{lettura.Inventate.DaGuardare.Count} da guardare; " &
                    $"doppioni {lettura.Doppioni.Count}; " &
                    $"mal collocate {lettura.MalCollocate.Count}.")

            Next

            ' La città ora è un pass/fail e la boccia l'Assert; qui resta il riepilogo a
            ' console, che serve a leggere subito *quali* valori sono usciti quando salta.
            Dim citta As List(Of String) = letture.
                Select(Function(l) $"{l.Formato} «{CollaudoReale.Ripulito(l.Profilo.Contatti.Citta)}»").
                ToList()

            If letture.Select(Function(l) CollaudoReale.Ripulito(l.Profilo.Contatti.Citta)).
                       Distinct().Count() > 1 Then
                testo.AppendLine($"Città diversa fra le strade: {String.Join(", ", citta)}.")
            End If

            testo.AppendLine($"Rapporto completo (contiene dati personali, fuori dal repo): {dove}")

            Return testo.ToString()

        End Function

    End Class

End Namespace
