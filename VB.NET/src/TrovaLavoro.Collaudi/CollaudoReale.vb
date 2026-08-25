Imports System.IO
Imports System.Linq
Imports System.Net.Http
Imports System.Text
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace NonRegressione

    ''' <summary>
    ''' Gli attrezzi che i collaudi reali si dividono: i prerequisiti (il CV vero, la
    ''' chiave), la porta verso il prototipo, il modo di mettere a confronto due elenchi,
    ''' la caccia ai valori che nel CV non c'erano, e i pezzi di rapporto che si scrivono
    ''' uguali.
    ''' </summary>
    ''' <remarks>
    ''' <para>Sta qui e non dentro un collaudo perché i collaudi reali sono più d'uno e
    ''' fanno domande diverse sulla stessa materia:
    ''' <see cref="CollaudiImportReale"/> chiede se la nuova app legge il CV come lo
    ''' leggeva il prototipo, <see cref="CollaudiFormatiReale"/> se le quattro strade di
    ''' lettura portano allo stesso profilo, <see cref="CollaudiDialogoReale"/> se il
    ''' dialogo guidato costruisce un profilo senza perdere niente per strada.
    ''' L'anti-invenzione, in particolare, deve essere <b>una sola</b>: due copie che
    ''' divergono darebbero due verdetti diversi sullo stesso difetto. E la porta verso il
    ''' prototipo pure: chi lo interroga deve rinunciare sempre allo stesso modo quando
    ''' non risponde.</para>
    ''' <para>A parte quella porta, nessuno di questi attrezzi tocca la rete: si
    ''' confrontano stringhe.</para>
    ''' </remarks>
    Friend Class CollaudoReale

        ''' <summary>Quante voci di differenza mostrare in un elenco del rapporto.</summary>
        Friend Const RigheDaMostrare As Integer = 12

        ''' <summary>
        ''' Perché <c>contatti.citta</c>, pur essendo tornata un <b>pass/fail</b>, in
        ''' tabella porta un ⚠️ e non un ≠.
        ''' </summary>
        ''' <remarks>
        ''' <para>Un CV può portare due indirizzi — residenza e domicilio — e fino al pool
        ''' 1.01 nessun prompt diceva quale dei due fosse «la città». Misurato il
        ''' 2026-08-08 sul CV vero, che ne ha due: a <b>trascrizione identica bit per
        ''' bit</b>, tre strutturazioni di fila hanno risposto «Pieve Ligure (GE)», «Pieve
        ''' Ligure (GE) / Carasco (GE)» e «Pieve Ligure (GE), Carasco (GE)». Tenerci un
        ''' <c>Assert</c> sarebbe stato un collaudo che lampeggia a caso: è esattamente la
        ''' lezione di T2 sul pass/fail (cap. 14).</para>
        ''' <para>Dal <b>pool 1.02</b> il prompt decide: la città è il <b>domicilio</b>,
        ''' una sola — il posto dove si è raggiungibili per lavorare. Il caso di confine
        ''' non è più tale, e il pass/fail è tornato: lo dà
        ''' <see cref="CittaFuoriDalDomicilio"/>. Ma il metro è il <b>CV</b>, non l'altra
        ''' colonna: il prototipo è fermo al pool 1.00, quella regola non ce l'ha, e
        ''' pretendere che i due valori coincidano boccerebbe l'app proprio dove ha
        ''' imparato a fare meglio. In tabella la differenza resta perciò un ⚠️ da
        ''' leggere.</para>
        ''' <para><b>Senza quella regola non vuol dire con la regola opposta</b>: il
        ''' 2026-08-25, sul CV del giro D, il prototipo ha dato il <b>domicilio</b> come
        ''' l'app. Non è un difetto e non cambia il metro — dice solo che il suo esito lì
        ''' non è garantito, e che una differenza va letta, non prevista.</para>
        ''' </remarks>
        Friend Const PerchePerLaCitta As String =
            "*La **città** è segnalata e non bocciata **in questa tabella**, ma un " &
            "pass/fail adesso ce l'ha: dal **Pool 1.02** il prompt decide — la città è il " &
            "**domicilio**, una sola — e il metro è il **CV**, non l'altra colonna. Chi " &
            "legge lo stesso CV con un prompt precedente può ancora tenere la residenza: " &
            "due valori diversi qui non sono di per sé un difetto della lettura.*"

        ''' <summary>
        ''' Perché il <b>numero</b> delle esperienze informali è segnalato e non bocciato,
        ''' mentre le formali e la formazione restano sotto tolleranza.
        ''' </summary>
        ''' <remarks>
        ''' <para>Misurato il 2026-08-08 sul CV vero: un blocco solo — un volontariato con
        ''' un ruolo, un'organizzazione e una descrizione che dentro cita un'abilitazione,
        ''' un corso e un concorso vinto — è stato letto in tre modi da un giro all'altro.
        ''' Una voce sola con tutto dentro <c>cosa_facevo</c>; tre voci, con il corso e il
        ''' concorso promossi a esperienze; una voce fra le formali <i>e</i> una fra le
        ''' informali.</para>
        ''' <para>Il pool 1.01 ha chiuso le due letture sbagliate — un'attività in una
        ''' sezione sola, i dettagli di una descrizione non diventano esperienze — ma
        ''' quanto finemente distillare un racconto in voci resta un giudizio, non un
        ''' numero giusto: è la stessa ragione per cui le competenze non si contano
        ''' (<see cref="CollaudiImportReale.TolleranzaVoci"/>). Il numero quindi si legge
        ''' nel rapporto; ciò che è <b>sempre</b> sbagliato — la stessa attività contata
        ''' due volte — ha il suo Assert in <see cref="ControlloDoppioni"/>.</para>
        ''' </remarks>
        Friend Const PerchePerLeInformali As String =
            "*Il **numero** delle esperienze informali è segnalato e non bocciato: quanto " &
            "finemente distillare un racconto in voci è un giudizio, come per le " &
            "competenze. Ciò che è sempre sbagliato — la stessa attività contata due " &
            "volte — si boccia più sotto.*"

        ' --- I prerequisiti ---------------------------------------------------------

        ''' <summary>
        ''' La cartella del CV vero, indicata dall'ambiente. Quel file non sta nel repo e
        ''' non deve starci: senza la variabile il collaudo rinuncia invece di fallire,
        ''' perché sull'altra postazione non c'è.
        ''' </summary>
        Friend Const VariabileCartella As String = "CV_DI_PROVA"

        ''' <summary>La cartella del CV di prova, o la rinuncia dichiarata.</summary>
        Friend Shared Function CartellaOppureRinuncia() As String

            Dim cartella As String = Environment.GetEnvironmentVariable(VariabileCartella)

            If String.IsNullOrWhiteSpace(cartella) OrElse Not Directory.Exists(cartella) Then
                Assert.Inconclusive(
                    $"Collaudo reale saltato: definisci {VariabileCartella} con la cartella che " &
                    "contiene il CV da importare.")
                Return Nothing
            End If

            Return cartella

        End Function

        ''' <summary>Il CV in PDF dentro quella cartella, o la rinuncia dichiarata.</summary>
        Friend Shared Function CvInPdfOppureRinuncia() As String

            Dim cartella As String = CartellaOppureRinuncia()

            Dim pdf As String = Directory.EnumerateFiles(cartella, "*.pdf").
                                          OrderBy(Function(f) f).FirstOrDefault()

            If pdf Is Nothing Then
                Assert.Inconclusive($"Collaudo reale saltato: in «{cartella}» non c'è nessun PDF.")
                Return Nothing
            End If

            Return pdf

        End Function

        ''' <summary>La chiave dall'ambiente; senza, il collaudo non fallisce: rinuncia.</summary>
        Friend Shared Function ChiaveOppureRinuncia() As String

            Try
                Return ClientClaude.ChiaveDaAmbiente()
            Catch ex As ErroreAi
                Assert.Inconclusive(
                    "Collaudo reale saltato: manca la chiave. Definisci " &
                    ClientClaude.NomeVariabileChiave & " nell'ambiente prima di lanciare il banco.")
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Il pool <b>integrato</b>, per forza: se accanto all'eseguibile ce ne fosse uno
        ''' esterno, il collaudo misurerebbe di nascosto un altro prompt.
        ''' </summary>
        Friend Shared Function PoolIntegrato() As LibreriaPrompt

            Return LibreriaPrompt.Apri(Path.Combine(Path.GetTempPath(), "pool-inesistente"))

        End Function

        ' --- La porta del prototipo --------------------------------------------------

        ''' <summary>Il passo 2 del prototipo: un turno del profilo, torna il frammento JSON.</summary>
        Friend Const PrototipoStruttura As String = "http://localhost:3000/struttura"

        ''' <summary>
        ''' Chiede al prototipo di strutturare una risposta per un turno, come fa la sua
        ''' pagina: <c>POST /struttura</c>. È la stessa domanda che l'app gira al pool e
        ''' all'API, ed è per questo che serve a due collaudi — l'import di un CV
        ''' (<see cref="CollaudiImportReale"/>) e un turno del dialogo
        ''' (<see cref="CollaudiDialogoReale"/>).
        ''' </summary>
        Friend Shared Function StrutturaDalPrototipoAsync(turno As String,
                                                          risposta As String) As Task(Of JsonObject)

            Dim richiesta As New JsonObject From {
                {"turno", turno},
                {"risposta", risposta}}

            Return ChiediAlPrototipoAsync(
                PrototipoStruttura, richiesta, $"la strutturazione del turno «{turno}»")

        End Function

        ''' <summary>
        ''' Una domanda al prototipo. Se non risponde non è un difetto della nuova app:
        ''' il collaudo rinuncia dicendo come si accende.
        ''' </summary>
        Friend Shared Async Function ChiediAlPrototipoAsync(indirizzo As String, richiesta As JsonObject,
                                                            cosa As String) As Task(Of JsonObject)

            Using http As New HttpClient()

                ' La trascrizione di un PDF è la chiamata più lenta di tutto il progetto:
                ' il PDF viaggia intero e il modello lo legge pagina per pagina.
                http.Timeout = TimeSpan.FromMinutes(3)

                Dim risposta As HttpResponseMessage
                Try
                    risposta = Await http.PostAsync(indirizzo,
                        New StringContent(richiesta.ToJsonString(), New UTF8Encoding(False), "application/json"))
                Catch ex As HttpRequestException
                    Assert.Inconclusive(
                        $"Il prototipo non risponde su {indirizzo}: avvialo con «npm start» dentro " &
                        $"HTML+JS/ (la chiave sta nel suo .env). {ex.Message}")
                    Return Nothing
                End Try

                Using risposta

                    Dim corpo As String = Await risposta.Content.ReadAsStringAsync()

                    Assert.IsTrue(risposta.IsSuccessStatusCode,
                        $"il prototipo ha risposto HTTP {CInt(risposta.StatusCode)} per {cosa}: {corpo}")

                    Dim oggetto As JsonObject = TryCast(JsonNode.Parse(corpo), JsonObject)
                    Assert.IsNotNull(oggetto, $"il prototipo non ha restituito un oggetto JSON per {cosa}")

                    Return oggetto

                End Using
            End Using

        End Function

        ' --- Due elenchi messi a confronto ------------------------------------------

        ''' <summary>
        ''' Quanto si somigliano due elenchi di voci, e in che cosa differiscono. Serve
        ''' sulle righe di due trascrizioni e sulle competenze di due profili, che sono la
        ''' stessa domanda su cose diverse.
        ''' </summary>
        Friend Class ConfrontoElenchi

            ''' <summary>Frazione di voci in comune sul lato che ne ha più.</summary>
            Public Property Frazione As Double

            Public Property VociPrimo As Integer
            Public Property VociSecondo As Integer
            Public Property InComune As Integer

            ''' <summary>Voci che ha solo il primo elenco, come le ha scritte lui.</summary>
            Public Property SoloPrimo As New List(Of String)

            ''' <summary>Voci che ha solo il secondo.</summary>
            Public Property SoloSecondo As New List(Of String)

        End Class

        ''' <summary>
        ''' Confronta due elenchi. Le voci si appaiano normalizzate negli spazi e nelle
        ''' maiuscole — si vuole sapere se una manca, non se è scritta in maiuscolo da una
        ''' parte sola — ma si mostrano come sono state scritte.
        ''' </summary>
        Friend Shared Function ConfrontaElenchi(primo As Dictionary(Of String, String),
                                                secondo As Dictionary(Of String, String)) As ConfrontoElenchi

            ' Il conteggio passa da Where: su un dizionario «Count» è la sua proprietà, e
            ' scritto con la condizione dentro non compilerebbe nemmeno.
            Dim inComune As Integer = primo.Keys.Where(Function(v) secondo.ContainsKey(v)).Count()
            Dim quante As Integer = Math.Max(primo.Count, secondo.Count)

            Return New ConfrontoElenchi With {
                .Frazione = If(quante = 0, 0.0, inComune / quante),
                .VociPrimo = primo.Count,
                .VociSecondo = secondo.Count,
                .InComune = inComune,
                .SoloPrimo = primo.Where(Function(v) Not secondo.ContainsKey(v.Key)).
                                   Select(Function(v) v.Value).Take(RigheDaMostrare).ToList(),
                .SoloSecondo = secondo.Where(Function(v) Not primo.ContainsKey(v.Key)).
                                       Select(Function(v) v.Value).Take(RigheDaMostrare).ToList()}

        End Function

        ''' <summary>Due testi messi a confronto riga per riga.</summary>
        ''' <param name="perContenuto">
        ''' Se appaiare le righe <b>per sole lettere e cifre</b>, buttando via
        ''' punteggiatura e marcatura. Serve quando i due testi arrivano da formati
        ''' diversi: fra un CV in markdown e lo stesso in testo semplice, «- Lavoro in
        ''' team» e «• Lavoro in team» sono la stessa riga, e contarle come diverse
        ''' misurerebbe la marcatura invece del contenuto. Fra due trascrizioni dello
        ''' stesso PDF, invece, la punteggiatura è contenuto e va guardata: lì questo
        ''' resta spento.
        ''' </param>
        Friend Shared Function ConfrontaTesti(primo As String, secondo As String,
                                              Optional perContenuto As Boolean = False) As ConfrontoElenchi

            Return ConfrontaElenchi(RigheSignificative(primo, perContenuto),
                                    RigheSignificative(secondo, perContenuto))

        End Function

        ''' <summary>Le competenze di due profili, voce per voce.</summary>
        Friend Shared Function ConfrontaCompetenze(primo As Profilo, secondo As Profilo) As ConfrontoElenchi

            Return ConfrontaElenchi(Appaiabili(primo.Competenze), Appaiabili(secondo.Competenze))

        End Function

        ''' <summary>
        ''' Le righe di un testo che vale la pena confrontare. Le righe di una lettera o
        ''' due — separatori, iniziali — non dicono niente. La tabulazione conta come un a
        ''' capo: in un CV impaginato a tabella separa due celle, cioè due informazioni
        ''' diverse messe sulla stessa riga.
        ''' </summary>
        Friend Shared Function RigheSignificative(testo As String,
                                                  Optional perContenuto As Boolean = False) As Dictionary(Of String, String)

            Return Appaiabili(If(testo, String.Empty).
                Split({vbCr, vbLf, vbTab}, StringSplitOptions.RemoveEmptyEntries),
                lunghezzaMinima:=3, perContenuto:=perContenuto)

        End Function

        ''' <summary>
        ''' Un elenco pronto per l'appaiamento: dalla voce normalizzata (la chiave) a
        ''' quella da mostrare. La prima occorrenza è quella che si mostra — se una voce si
        ''' ripete, nel rapporto non serve vederla due volte.
        ''' </summary>
        ''' <param name="perContenuto">
        ''' Appaia per sole lettere e cifre, con lo stesso metro dell'anti-invenzione
        ''' (<see cref="PerCercare"/>): serve quando i due elenchi vengono da formati
        ''' diversi. Vedi <see cref="ConfrontaTesti"/>.
        ''' </param>
        Friend Shared Function Appaiabili(voci As IEnumerable(Of String),
                                          Optional lunghezzaMinima As Integer = 1,
                                          Optional perContenuto As Boolean = False) As Dictionary(Of String, String)

            Dim elenco As New Dictionary(Of String, String)

            For Each voce As String In voci

                Dim chiave As String = If(perContenuto,
                                          PerCercare(voce),
                                          Ripulito(voce).ToLowerInvariant())
                If chiave.Length < lunghezzaMinima Then Continue For

                If Not elenco.ContainsKey(chiave) Then elenco.Add(chiave, Ripulito(voce))

            Next

            Return elenco

        End Function

        ' --- L'anti-invenzione -------------------------------------------------------

        ''' <summary>I valori del profilo che nel CV non si trovano, divisi per gravità.</summary>
        Friend Class Invenzioni

            ''' <summary>
            ''' Nomi propri e recapiti: il prompt ordina di copiarli, e se non stanno nel
            ''' testo di partenza sono inventati. Qui il collaudo si ferma.
            ''' </summary>
            Public Property Gravi As New List(Of String)

            ''' <summary>
            ''' Campi che il modello può legittimamente riformulare — una data resa
            ''' «2015-2018» invece di «set. 2015 – giu. 2018», un ruolo tradotto. Si
            ''' guardano nel rapporto, non fanno cadere il collaudo.
            ''' </summary>
            Public Property DaGuardare As New List(Of String)

        End Class

        ''' <summary>
        ''' Cerca nel testo del CV ogni valore che il profilo dovrebbe aver copiato da
        ''' lì. Restano fuori <c>cosa_facevo</c> e le competenze: quelli il modello li
        ''' riformula per mestiere, e giudicarli è compito di chi legge.
        ''' </summary>
        Friend Shared Function ValoriFuoriDalTesto(profilo As Profilo, testo As String) As Invenzioni

            Dim pagliaio As String = PerCercare(testo)
            Dim senzaSpazi As String = pagliaio.Replace(" ", "")
            Dim fuori As New Invenzioni

            Cerca(fuori.Gravi, "nome", profilo.Nome, pagliaio, senzaSpazi)
            Cerca(fuori.Gravi, "contatti.email", profilo.Contatti.Email, pagliaio, senzaSpazi)

            Cerca(fuori.DaGuardare, "contatti.telefono", profilo.Contatti.Telefono, pagliaio, senzaSpazi)
            Cerca(fuori.DaGuardare, "contatti.citta", profilo.Contatti.Citta, pagliaio, senzaSpazi)
            Cerca(fuori.DaGuardare, "contatti.link", profilo.Contatti.Link, pagliaio, senzaSpazi)

            For Each categoria As String In profilo.Patente.Categorie
                Cerca(fuori.DaGuardare, "patente.categoria", categoria, pagliaio, senzaSpazi)
            Next

            For Each esperienza As EsperienzaFormale In profilo.EsperienzeFormali
                Cerca(fuori.Gravi, "esperienza.azienda", esperienza.Azienda, pagliaio, senzaSpazi)
                Cerca(fuori.DaGuardare, "esperienza.ruolo", esperienza.Ruolo, pagliaio, senzaSpazi)
                Cerca(fuori.DaGuardare, "esperienza.durata", esperienza.Durata, pagliaio, senzaSpazi)
            Next

            For Each voce As VoceFormazione In profilo.Formazione
                Cerca(fuori.Gravi, "formazione.istituto", voce.Istituto, pagliaio, senzaSpazi)
                Cerca(fuori.DaGuardare, "formazione.titolo", voce.Titolo, pagliaio, senzaSpazi)
                Cerca(fuori.DaGuardare, "formazione.anno", voce.Anno, pagliaio, senzaSpazi)
            Next

            Return fuori

        End Function

        ''' <summary>Aggiunge il valore all'elenco se nel CV non si trova.</summary>
        Private Shared Sub Cerca(dove As List(Of String), campo As String, valore As String,
                                 pagliaio As String, senzaSpazi As String)

            If String.IsNullOrWhiteSpace(valore) Then Return
            If NelTesto(valore, pagliaio, senzaSpazi) Then Return

            dove.Add($"{campo}: «{Ripulito(valore)}»")

        End Sub

        ''' <summary>
        ''' Se un valore si ritrova nel testo del CV. La prova è per parole e non per
        ''' stringa intera, altrimenti un «S.r.l.» diventato «Srl» sembrerebbe
        ''' un'invenzione: ogni parola dalle quattro lettere in su deve comparire nel
        ''' testo. Per i valori che di parole lunghe non ne hanno — una sigla, un anno —
        ''' si cerca il valore intero, ignorando spazi e punteggiatura: è così che un
        ''' telefono scritto «333 123 4567» si ritrova in «3331234567».
        ''' </summary>
        Friend Shared Function NelTesto(valore As String, pagliaio As String, senzaSpazi As String) As Boolean

            Dim cercato As String = PerCercare(valore)
            If cercato.Length = 0 Then Return True

            Dim lunghe As String() = cercato.Split(" "c, StringSplitOptions.RemoveEmptyEntries).
                                            Where(Function(p) p.Length >= 4).ToArray()

            If lunghe.Length > 0 Then Return lunghe.All(Function(p) pagliaio.Contains(p))

            Return senzaSpazi.Contains(cercato.Replace(" ", ""))

        End Function

        ''' <summary>
        ''' Il testo pronto per cercarci dentro: tutto minuscolo, e tutto ciò che non è
        ''' lettera o cifra ridotto a un solo spazio. Gli accenti restano — sono lettere,
        ''' e in un CV italiano ci sono.
        ''' </summary>
        Friend Shared Function PerCercare(testo As String) As String

            Dim costruito As New StringBuilder(If(testo, String.Empty).Length)

            For Each carattere As Char In If(testo, String.Empty).ToLowerInvariant()
                If Char.IsLetterOrDigit(carattere) Then
                    costruito.Append(carattere)
                ElseIf costruito.Length > 0 AndAlso costruito(costruito.Length - 1) <> " "c Then
                    costruito.Append(" "c)
                End If
            Next

            Return costruito.ToString().TrimEnd()

        End Function

        ' --- La città: il domicilio, una sola ----------------------------------------

        ''' <summary>La parola con cui un CV nomina il domicilio.</summary>
        Private Const ParolaDomicilio As String = "domicilio"

        ''' <summary>
        ''' La parola con cui nomina la residenza, troncata al pezzo che le due forme —
        ''' «residenza», «residente» — hanno in comune.
        ''' </summary>
        Private Const ParolaResidenza As String = "residen"

        ''' <summary>
        ''' Che cosa non torna nella <c>citta</c> di un profilo letto da un CV, o la
        ''' stringa vuota se torna. È il pass/fail che il <b>pool 1.02</b> ha reso
        ''' possibile (<see cref="PerchePerLaCitta"/>): quel prompt dice che la città è il
        ''' <b>domicilio</b> e che dev'essere una sola, quindi ora c'è una risposta giusta
        ''' da pretendere.
        ''' </summary>
        ''' <remarks>
        ''' <para>Il metro è il <b>CV</b> e non l'altra lettura: si guarda dove il CV
        ''' dichiara il domicilio e ci si cerca dentro la città del profilo, con lo stesso
        ''' appaiamento per parole dell'anti-invenzione (<see cref="NelTesto"/>). Chi
        ''' scrive la residenza non ritrova lì le sue parole; chi le scrive tutt'e due —
        ''' «A / B», «A, B» — nemmeno, ed è il modo in cui questo controllo vede anche la
        ''' seconda metà della regola, «una sola».</para>
        ''' <para>Il limite dichiarato, perché non si scopra un giorno per caso: se il CV
        ''' non nomina il domicilio, o lo nomina e manda l'indirizzo a capo, non c'è niente
        ''' da confrontare e il controllo <b>rinuncia</b> invece di bocciare — un CV con un
        ''' indirizzo solo non deve far cadere il collaudo. Preferisce cioè tacere che
        ''' sbagliare: la sezione «da leggere a mano» del rapporto resta il posto dove un
        ''' caso storto si vede comunque.</para>
        ''' </remarks>
        Friend Shared Function CittaFuoriDalDomicilio(citta As String, testo As String) As String

            Dim dichiarato As String = DomicilioDichiarato(testo)
            If dichiarato.Length = 0 Then Return String.Empty

            Dim scritta As String = Ripulito(citta)
            If scritta.Length = 0 Then Return "il CV dichiara un domicilio e la città è vuota"

            If NelTesto(scritta, dichiarato, dichiarato.Replace(" ", "")) Then Return String.Empty

            Return $"«{scritta}» non è il domicilio che il CV dichiara"

        End Function

        ''' <summary>
        ''' Se due letture nominano la <b>stessa</b> città, anche quando una si porta
        ''' dietro la provincia e l'altra no.
        ''' </summary>
        ''' <remarks>
        ''' <para>È l'altra metà della questione città, e va letta insieme a
        ''' <see cref="CittaFuoriDalDomicilio"/>: quello è il <b>pass/fail</b> e ha per
        ''' metro il CV; questo serve solo a confrontare due letture fra loro, e per farlo
        ''' bene deve sapere che cosa il prompt ordina davvero. Il pool 1.02 dice
        ''' <i>quale</i> indirizzo prendere — il domicilio, uno solo — non <i>come</i>
        ''' scriverlo.</para>
        ''' <para>Misurato il <b>2026-08-10</b> sul CV vero, durante il collaudo di tappa
        ''' di T4: la stessa riga di domicilio, che sta identica in tutti e quattro i file,
        ''' è tornata tre volte **senza** la sigla della provincia e una volta con. Nessuna
        ''' delle due letture ha perso l'indirizzo e nessuna ha inventato niente: è il
        ''' modello che a volte lascia cadere la provincia. Pretendere l'uguaglianza esatta
        ''' faceva cadere il collaudo per una differenza che non è un difetto.</para>
        ''' <para>Si confronta perciò <b>per parole</b>, e l'unica differenza ammessa è la
        ''' <b>sigla della provincia</b> — due lettere, in Italia sempre: «Forlì» dentro
        ''' «Forlì (FC)» passa. Non basta invece il semplice contenimento, e lo ha detto
        ''' il collaudo di questo attrezzo appena scritto: con quello, «Forlì Rimini»
        ''' sarebbe passato per la stessa città di «Forlì», mentre due città insieme
        ''' violano la seconda metà della regola del pool 1.02, «una sola».</para>
        ''' </remarks>
        Friend Shared Function StessaCitta(una As String, altra As String) As Boolean

            Dim parole As Func(Of String, HashSet(Of String)) =
                Function(v) New HashSet(Of String)(
                    PerCercare(Ripulito(v)).Split({" "c}, StringSplitOptions.RemoveEmptyEntries),
                    StringComparer.Ordinal)

            Dim prima As HashSet(Of String) = parole(una)
            Dim seconda As HashSet(Of String) = parole(altra)

            ' Una città vuota non è «la stessa» di una scritta: è una lettura che non ha
            ' trovato l'indirizzo, e va vista.
            If prima.Count = 0 OrElse seconda.Count = 0 Then Return prima.Count = seconda.Count

            Dim piuRicca As HashSet(Of String) = If(prima.Count >= seconda.Count, prima, seconda)
            Dim piuScarna As HashSet(Of String) = If(prima.Count >= seconda.Count, seconda, prima)

            If Not piuScarna.IsSubsetOf(piuRicca) Then Return False

            Return piuRicca.Except(piuScarna).All(Function(inPiu) inPiu.Length <= SigleDiProvincia)

        End Function

        ''' <summary>Quanto è lunga la sigla di una provincia italiana: «GE», «RE», «TO».</summary>
        Private Const SigleDiProvincia As Integer = 2

        ''' <summary>
        ''' Il pezzo di CV che dichiara il domicilio, già pronto per cercarci dentro: la
        ''' riga che lo nomina, da quella parola in poi. Se dopo il domicilio la stessa
        ''' riga torna a parlare di residenza, lì il pezzo si chiude — altrimenti un CV che
        ''' scrive i due indirizzi in quest'ordine farebbe passare per domicilio anche la
        ''' residenza.
        ''' </summary>
        Private Shared Function DomicilioDichiarato(testo As String) As String

            For Each riga As String In If(testo, String.Empty).
                Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)

                Dim normalizzata As String = PerCercare(riga)

                Dim dove As Integer = normalizzata.IndexOf(ParolaDomicilio, StringComparison.Ordinal)
                If dove < 0 Then Continue For

                Dim dopo As String = normalizzata.Substring(dove + ParolaDomicilio.Length)

                Dim residenza As Integer = dopo.IndexOf(ParolaResidenza, StringComparison.Ordinal)
                If residenza >= 0 Then dopo = dopo.Substring(0, residenza)

                Return dopo.Trim()

            Next

            Return String.Empty

        End Function

        ' --- Valori e pezzi di rapporto ----------------------------------------------

        ''' <summary>Un valore senza gli spazi di troppo: quelli sono impaginazione.</summary>
        Friend Shared Function Ripulito(valore As String) As String

            Return String.Join(" ", If(valore, String.Empty).
                Split({" "c, vbTab, vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries))

        End Function

        ''' <summary>Le categorie della patente, in una riga confrontabile.</summary>
        Friend Shared Function Categorie(profilo As Profilo) As String

            Return String.Join(", ", profilo.Patente.Categorie.Select(
                Function(c) Ripulito(c).ToUpperInvariant()))

        End Function

        ''' <summary>
        ''' Il segno con cui una riga di rapporto si chiude: <c>=</c> se i valori
        ''' coincidono, <c>≠</c> se no, e <c>⚠️</c> quando la differenza è di quelle che si
        ''' guardano invece di bocciare (la città, <see cref="PerchePerLaCitta"/>, e il
        ''' numero delle esperienze informali, <see cref="PerchePerLeInformali"/>). Sta
        ''' qui, e non dentro i due rapporti, perché i due rapporti devono usare lo stesso
        ''' alfabeto — e perché così il banco lo prova senza rete: il caso ⚠️ dipende da
        ''' come le due parti leggono un CV con due indirizzi, e su un CV che ne ha uno
        ''' solo può non presentarsi mai.
        ''' </summary>
        Friend Shared Function Marcatore(uguali As Boolean, segnalato As Boolean) As String

            If uguali Then Return "="

            Return If(segnalato, "⚠️", "**≠**")

        End Function

        ''' <summary>Un valore dentro una cella di tabella, senza rompere la tabella.</summary>
        Friend Shared Function PerTabella(valore As String) As String

            Dim pulito As String = Ripulito(valore).Replace("|", "\|")
            Return If(pulito.Length = 0, "*(vuoto)*", pulito)

        End Function

        ''' <summary>Un elenco di righe, o il silenzio se non ce ne sono.</summary>
        Friend Shared Sub Elenco(testo As StringBuilder, titolo As String, righe As List(Of String))

            If righe.Count = 0 Then Return

            testo.Append($"**{titolo}** (fino a {RigheDaMostrare}):").Append(vbLf).Append(vbLf)
            For Each riga As String In righe
                testo.Append($"- `{riga}`").Append(vbLf)
            Next
            testo.Append(vbLf)

        End Sub

        ''' <summary>Cosa non si è ritrovato nel testo di partenza, per un lato.</summary>
        ''' <param name="dove">
        ''' Come si chiama il testo in cui si è cercato. Per l'import è il CV; per il
        ''' dialogo guidato non c'è nessun CV, c'è quello che l'utente ha detto.
        ''' </param>
        Friend Shared Sub Invenzione(testo As StringBuilder, lato As String, inventate As Invenzioni,
                                     Optional dove As String = "nel CV")

            testo.Append($"**{lato}** — ")

            If inventate.Gravi.Count = 0 AndAlso inventate.DaGuardare.Count = 0 Then
                testo.Append($"ogni valore copiato si ritrova {dove}.").Append(vbLf).Append(vbLf)
                Return
            End If

            testo.Append(vbLf).Append(vbLf)

            For Each voce As String In inventate.Gravi
                testo.Append($"- ⛔ {voce}").Append(vbLf)
            Next
            For Each voce As String In inventate.DaGuardare
                testo.Append($"- ⚠️ {voce} *(riformulazione ammessa: da guardare)*").Append(vbLf)
            Next

            testo.Append(vbLf)

        End Sub

        ''' <summary>Le voci contate due volte, per un lato.</summary>
        ''' <param name="bocciato">
        ''' Se per questo lato il doppione fa cadere il collaudo (⛔) invece di essere solo
        ''' segnalato (⚠️). Vale per l'<b>app</b>, a cui il pool 1.01 ha detto che
        ''' un'attività sta in una sezione sola; non per il prototipo, che quella regola
        ''' non ce l'ha e che qui è il termine di paragone, non il metro.
        ''' </param>
        Friend Shared Sub Doppioni(testo As StringBuilder, lato As String, doppioni As List(Of String),
                                   Optional bocciato As Boolean = False)

            PerLato(testo, lato, doppioni, "nessuna voce contata due volte", bocciato)

        End Sub

        ''' <summary>Le attività finite nella sezione sbagliata, per un lato.</summary>
        ''' <param name="bocciato">Come in <see cref="Doppioni"/>: ⛔ per l'app, ⚠️ per il prototipo.</param>
        Friend Shared Sub Collocazione(testo As StringBuilder, lato As String, malCollocate As List(Of String),
                                       Optional bocciato As Boolean = False)

            PerLato(testo, lato, malCollocate, "ogni attività sta nella sua sezione", bocciato)

        End Sub

        ''' <summary>Un verdetto per lato: il silenzio se non c'è niente da dire.</summary>
        Private Shared Sub PerLato(testo As StringBuilder, lato As String, voci As List(Of String),
                                   quandoVuoto As String, bocciato As Boolean)

            If voci.Count = 0 Then
                testo.Append($"**{lato}** — {quandoVuoto}.").Append(vbLf).Append(vbLf)
                Return
            End If

            testo.Append($"**{lato}**").Append(vbLf).Append(vbLf)
            For Each voce As String In voci
                testo.Append($"- {If(bocciato, "⛔", "⚠️")} {voce}").Append(vbLf)
            Next
            testo.Append(vbLf)

        End Sub

    End Class

End Namespace
