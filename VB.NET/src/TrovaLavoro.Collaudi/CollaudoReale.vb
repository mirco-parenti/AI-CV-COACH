Imports System.IO
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace NonRegressione

    ''' <summary>
    ''' Gli attrezzi che i collaudi reali dell'import si dividono: i prerequisiti (il CV
    ''' vero, la chiave), il modo di mettere a confronto due elenchi, la caccia ai valori
    ''' che nel CV non c'erano, e i pezzi di rapporto che si scrivono uguali.
    ''' </summary>
    ''' <remarks>
    ''' <para>Sta qui e non dentro un collaudo perché i collaudi reali dell'import sono
    ''' due e fanno due domande diverse sulla stessa materia:
    ''' <see cref="CollaudiImportReale"/> chiede se la nuova app legge il CV come lo
    ''' leggeva il prototipo, <see cref="CollaudiFormatiReale"/> se le quattro strade di
    ''' lettura portano allo stesso profilo. L'anti-invenzione, in particolare, deve
    ''' essere <b>una sola</b>: due copie che divergono darebbero due verdetti diversi
    ''' sullo stesso difetto.</para>
    ''' <para>Nessuno di questi attrezzi tocca la rete: qui si confrontano stringhe.</para>
    ''' </remarks>
    Friend Class CollaudoReale

        ''' <summary>Quante voci di differenza mostrare in un elenco del rapporto.</summary>
        Friend Const RigheDaMostrare As Integer = 12

        ''' <summary>
        ''' Perché <c>contatti.citta</c> è <b>segnalata e non bocciata</b>, unica fra i
        ''' campi che il prompt ordina di copiare.
        ''' </summary>
        ''' <remarks>
        ''' <para>Un CV può portare due indirizzi — residenza e domicilio — e nessun
        ''' prompt del pool dice quale dei due sia «la città». Misurato il 2026-08-08 sul
        ''' CV vero, che ne ha due: a <b>trascrizione identica bit per bit</b>, tre
        ''' strutturazioni di fila hanno risposto «Pieve Ligure (GE)», «Pieve Ligure (GE)
        ''' / Carasco (GE)» e «Pieve Ligure (GE), Carasco (GE)». Non è una regressione
        ''' dell'app — è un caso di confine che il prompt non decide, e lo stesso vale per
        ''' il prototipo, che quel prompt lo condivide carattere per carattere.</para>
        ''' <para>Tenerci un <c>Assert</c> darebbe un collaudo che lampeggia a caso: è
        ''' esattamente la lezione di T2 sul pass/fail (cap. 14). La differenza quindi si
        ''' scrive nel rapporto con un ⚠️ e si legge; l'<c>Assert</c> torna quando il
        ''' prompt avrà deciso quale città sia quella buona.</para>
        ''' </remarks>
        Friend Const PerchePerLaCitta As String =
            "*La **città** è segnalata e non bocciata: un CV può portare due indirizzi — " &
            "residenza e domicilio — e nessun prompt dice quale sia «la città». A " &
            "trascrizione identica, tre strutturazioni di fila hanno dato tre risposte " &
            "diverse. Tornerà un pass/fail quando il prompt avrà deciso.*"

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
        ''' come il modello legge un CV con due indirizzi, e può non presentarsi per tre
        ''' esecuzioni di fila.
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

        ''' <summary>Cosa non si è ritrovato nel CV, per un lato.</summary>
        Friend Shared Sub Invenzione(testo As StringBuilder, lato As String, inventate As Invenzioni)

            testo.Append($"**{lato}** — ")

            If inventate.Gravi.Count = 0 AndAlso inventate.DaGuardare.Count = 0 Then
                testo.Append("ogni valore copiato si ritrova nel CV.").Append(vbLf).Append(vbLf)
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
