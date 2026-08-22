Imports System.Linq
Imports System.Threading.Tasks
Imports TrovaLavoro.Motore

''' <summary>
''' Chi risponde al posto dell'utente quando un collaudo conduce il dialogo del profilo
''' con l'AI vera: riceve una <b>traccia</b> — cosa direbbe quella persona, turno per
''' turno — e la porta dall'inizio alla fine.
''' </summary>
''' <remarks>
''' <para>Non segue una sequenza di gesti preparata: <b>guarda la mossa</b> e decide,
''' perché con l'AI vera non si sa in anticipo se un turno coglierà qualcosa o dirà «non
''' ho colto», e un copione rigido si romperebbe al primo scarto.</para>
''' <para>Sta qui e non dentro un collaudo perché le tracce sono più d'una e fanno
''' domande diverse sulla stessa materia: <see cref="NonRegressione.CollaudiDialogoReale"/>
''' guarda se il dialogo costruisce un profilo senza perdere niente,
''' <see cref="NonRegressione.CollaudiChecklistReale"/> se regge davanti a chi si gonfia,
''' racconta le cose nel turno sbagliato e salta le domande. Il modo di condurre deve
''' essere <b>uno solo</b>: due copie che divergono darebbero due verdetti diversi sullo
''' stesso difetto — ed è lo stesso principio per cui l'anti-invenzione vive in un posto
''' solo (<see cref="NonRegressione.CollaudoReale"/>).</para>
''' <para>Il conduttore non giudica: raccoglie i <b>battiti</b> (ogni mossa e ciò che le
''' ha risposto) e le <b>stranezze</b> (ciò che la traccia non prevedeva). A giudicare è
''' il collaudo che lo usa.</para>
''' </remarks>
Friend Class ConduttoreDiDialogo

    ''' <summary>
    ''' Un turno della traccia: le risposte che l'utente darebbe, nell'ordine. Più di una
    ''' vuol dire che al giro «ne hai un'altra?» si risponde di sì.
    ''' </summary>
    Friend Class Gruppo
        Public Property Turno As String
        Public Property Battute As New List(Of String)
    End Class

    ''' <summary>Una mossa del dialogo e ciò che il conduttore le ha risposto.</summary>
    Friend Class Battito
        Public Property Mossa As Mossa
        Public Property Risposta As String
        Public Property Scelta As String
    End Class

    ''' <summary>
    ''' Quante mosse al massimo può durare il dialogo prima che si debba concludere che
    ''' non converge. Una traccia ne prevede una trentina: il doppio è largo, e serve solo
    ''' a non lasciare un collaudo a girare all'infinito se un giorno la guardia
    ''' anti-rimbalzo si rompesse.
    ''' </summary>
    Private Const MosseMassime As Integer = 80

    ''' <summary>La risposta di ripiego se il dialogo richiede la categoria della patente.</summary>
    ''' <remarks>
    ''' Le tracce la dichiarano già («la patente B»), quindi la ri-domanda non dovrebbe
    ''' arrivare. Se arriva è il modello che non ha colto la categoria: si risponde e si
    ''' annota fra le stranezze, invece di mandare al posto suo la battuta del turno dopo
    ''' e sballare tutto il resto.
    ''' </remarks>
    Private Const RipiegoCategoria As String = "La B."

    Private ReadOnly _traccia As List(Of Gruppo)

    ''' <param name="traccia">Cosa direbbe l'utente, turno per turno.</param>
    Public Sub New(traccia As List(Of Gruppo))

        If traccia Is Nothing Then Throw New ArgumentNullException(NameOf(traccia))
        _traccia = traccia

    End Sub

    ''' <summary>Ogni mossa del dialogo e ciò che le è stato risposto, nell'ordine.</summary>
    Public ReadOnly Property Battiti As New List(Of Battito)

    ''' <summary>Ciò che la traccia non prevedeva: da leggere nel rapporto.</summary>
    Public ReadOnly Property Stranezze As New List(Of String)

    ''' <summary>Porta il dialogo dall'inizio alla fine rispondendo con la traccia.</summary>
    Public Async Function ConduciAsync(dialogo As DialogoProfilo) As Task(Of List(Of Battito))

        Dim gruppo As Integer = 0
        Dim battuta As Integer = 0
        Dim ultima As String = Nothing
        Dim daRipetere As Boolean = False
        Dim riprovato As Boolean = False

        Dim mossa As Mossa = Await dialogo.AvviaAsync()

        Do
            Dim battito As New Battito With {.Mossa = mossa}
            Battiti.Add(battito)

            If mossa.Tipo = TipoMossa.Fine Then Exit Do

            If Battiti.Count > MosseMassime Then
                Stranezze.Add($"Il dialogo ha superato {MosseMassime} mosse senza chiudere: " &
                              "interrotto qui.")
                Exit Do
            End If

            If mossa.Tipo = TipoMossa.ChiediRisposta Then

                Dim testo As String

                If daRipetere Then
                    ' Si è scelto «riprovo»: si ridice la stessa cosa, non la prossima.
                    testo = ultima
                    daRipetere = False

                ElseIf ChiedeLaCategoriaDellaPatente(mossa) Then
                    ' La traccia la dichiara già: se la si richiede, il modello non
                    ' l'aveva colta. Si risponde senza consumare la traccia.
                    Stranezze.Add("Il modello non ha colto la categoria della patente dalla " &
                                  "risposta, e il dialogo l'ha richiesta.")
                    testo = RipiegoCategoria

                Else
                    ' La prossima battuta: se il gruppo è finito, si passa al turno dopo.
                    While gruppo < _traccia.Count AndAlso battuta >= _traccia(gruppo).Battute.Count
                        gruppo += 1
                        battuta = 0
                    End While

                    If gruppo >= _traccia.Count Then
                        Stranezze.Add("Il dialogo ha chiesto una risposta in più di quelle che " &
                                      "la traccia prevede: interrotto qui.")
                        Exit Do
                    End If

                    testo = _traccia(gruppo).Battute(battuta)
                    battuta += 1
                    riprovato = False
                End If

                ultima = testo
                battito.Risposta = testo
                mossa = Await dialogo.RispondiAsync(testo)

            Else

                Dim offerte As List(Of String) = mossa.Scelte.Select(Function(s) s.Id).ToList()
                Dim scelta As String

                If offerte.Contains(Scelte.Riprova) Then
                    ' «Non ho colto niente»: si riprova una volta con le stesse parole,
                    ' poi si passa oltre — insistere non porterebbe da nessuna parte.
                    If riprovato Then
                        scelta = Scelte.Oltre
                        Stranezze.Add($"Il turno non ha colto nulla nemmeno al secondo tentativo: " &
                                      $"«{Accorcia(ultima)}». Si è passato oltre.")
                    Else
                        scelta = Scelte.Riprova
                        riprovato = True
                        daRipetere = True
                        Stranezze.Add($"Il turno non ha colto nulla da: «{Accorcia(ultima)}». " &
                                      "Si è riprovato con le stesse parole.")
                    End If

                ElseIf offerte.Contains(Scelte.Aggiungi) AndAlso AncoraInQuestoGruppo(gruppo, battuta) Then
                    ' Le competenze: la traccia ne ha un secondo giro da aggiungere.
                    scelta = Scelte.Aggiungi

                ElseIf offerte.Contains(Scelte.Altra) Then
                    ' Un turno ripetibile: un'altra voce se la traccia ce l'ha, se no avanti.
                    scelta = If(AncoraInQuestoGruppo(gruppo, battuta), Scelte.Altra, Scelte.Procedi)

                ElseIf offerte.Contains(Scelte.Riprendi) Then
                    ' Prima di chiudere il dialogo riofre una domanda rimasta a vuoto.
                    ' La traccia però è scritta per un giro solo, e le sue battute sono
                    ' già state dette tutte: si declina e lo si annota, perché il
                    ' rapporto dica che quel turno è rimasto senza risposta.
                    scelta = Scelte.Lascia
                    Stranezze.Add("Il dialogo ha riproposto una domanda rimasta senza risposta: " &
                                  $"«{Accorcia(String.Join(" ", mossa.Detto))}». Si è lasciata così.")

                ElseIf offerte.Contains(Scelte.Conferma) Then
                    ' Vale per le schede dei turni e per i frammenti ripescati: la persona
                    ' che si è raccontata bene conferma ciò che ha detto.
                    scelta = Scelte.Conferma

                Else
                    Stranezze.Add("Il dialogo ha offerto scelte che il conduttore non conosce: " &
                                  String.Join(", ", offerte) & ". Interrotto qui.")
                    Exit Do
                End If

                battito.Scelta = scelta
                mossa = Await dialogo.ScegliAsync(scelta)

            End If

        Loop

        Return Battiti

    End Function

    ''' <summary>Se nel turno in corso la traccia ha ancora qualcosa da dire.</summary>
    Private Function AncoraInQuestoGruppo(gruppo As Integer, battuta As Integer) As Boolean

        Return gruppo < _traccia.Count AndAlso battuta < _traccia(gruppo).Battute.Count

    End Function

    ''' <summary>
    ''' Le battute della traccia finite nel turno sbagliato: ognuna è scritta per un turno
    ''' preciso, e se il conduttore la manda a un altro il collaudo non sta più misurando
    ''' quello che dice di misurare.
    ''' </summary>
    ''' <remarks>
    ''' Non guarda il conduttore, guarda la <b>spia</b>: che turno abbia strutturato quella
    ''' risposta lo dice il dialogo, chiamando il prompt di quel turno. È il secondo
    ''' controllo, indipendente dal primo.
    ''' </remarks>
    Public Function AllineamentoRotto(spia As StrutturatoreSpia) As List(Of String)

        Dim rotte As New List(Of String)

        For Each chiamata As StrutturatoreSpia.Chiamata In spia.Chiamate

            Dim suo As Gruppo = _traccia.FirstOrDefault(
                Function(g) g.Battute.Any(Function(b) b = chiamata.Risposta))

            ' Non è una battuta della traccia: è un frammento ripescato, o una risposta
            ' di ripiego. Quelle vanno dove il dialogo decide.
            If suo Is Nothing Then Continue For

            If suo.Turno <> chiamata.Turno Then
                rotte.Add($"«{Accorcia(chiamata.Risposta)}» è scritta per il turno " &
                          $"«{suo.Turno}» ed è finita nel turno «{chiamata.Turno}»")
            End If

        Next

        Return rotte

    End Function

    ''' <summary>Una battuta della traccia, per turno e posizione.</summary>
    Public Function Battuta(turno As String, Optional quale As Integer = 0) As String

        Return _traccia.First(Function(g) g.Turno = turno).Battute(quale)

    End Function

    ''' <summary>Se questa mossa è la ri-domanda della categoria della patente.</summary>
    ''' <remarks>
    ''' Si cerca <c>«Una cosa sola:»</c> e non <c>«di che categoria»</c>, che sembrerebbe
    ''' più naturale ma è la trappola in cui questo conduttore è caduto al primo giro:
    ''' quelle parole stanno <b>anche</b> nell'apertura del turno della patente, così il
    ''' conduttore ha risposto «La B.» alla domanda vera, si è tenuto in tasca la battuta
    ''' della traccia e ha fatto slittare di un turno tutte quelle dopo — con il collaudo
    ''' che restava verde perché il dialogo, dal canto suo, si era comportato bene. Da lì
    ''' nasce anche <see cref="AllineamentoRotto"/>: un conduttore fuori passo non deve
    ''' poter passare per un collaudo riuscito.
    ''' </remarks>
    Private Shared Function ChiedeLaCategoriaDellaPatente(mossa As Mossa) As Boolean

        Return mossa.Detto.Any(Function(d) d.Contains("Una cosa sola:"))

    End Function

    ''' <summary>Se questa mossa sta chiedendo di confermare qualcosa.</summary>
    Public Shared Function ChiedeConferma(mossa As Mossa) As Boolean

        Return mossa.Tipo = TipoMossa.ChiediScelta AndAlso
               mossa.Scelte.Any(Function(s) s.Id = Scelte.Conferma)

    End Function

    ''' <summary>
    ''' Se di un frammento instradato altrove il dialogo ha reso conto: o l'ha ripescato
    ''' rimettendolo in bocca all'utente (<c>EcoUtente</c>), o ha detto di lasciarlo fuori.
    ''' Il confronto è per sole lettere e cifre — il modello ricopia le parole dell'utente
    ''' ma può cambiare una virgola, e una virgola non è una perdita.
    ''' </summary>
    Public Shared Function Onorato(instradato As StrutturatoreSpia.Instradato,
                                   battiti As List(Of Battito)) As Boolean

        Dim cercato As String = NonRegressione.CollaudoReale.PerCercare(instradato.Frase)
        If cercato = "" Then Return True

        For Each battito As Battito In battiti

            If NonRegressione.CollaudoReale.PerCercare(battito.Mossa.EcoUtente).Contains(cercato) Then Return True

            For Each detto As String In battito.Mossa.Detto
                If Not detto.Contains("lascio fuori") Then Continue For
                If NonRegressione.CollaudoReale.PerCercare(detto).Contains(cercato) Then Return True
            Next

        Next

        Return False

    End Function

    ''' <summary>
    ''' I turni nell'ordine in cui sono stati chiesti la <b>prima</b> volta. Le volte
    ''' successive sono gli smaltimenti dell'anti-perdita, che per mestiere tornano su un
    ''' turno passato: contarle direbbe che l'ordine è rotto quando invece sta funzionando.
    ''' </summary>
    Public Shared Function PrimeVolte(spia As StrutturatoreSpia) As String

        Dim visti As New List(Of String)

        For Each chiamata As StrutturatoreSpia.Chiamata In spia.Chiamate
            If Not visti.Contains(chiamata.Turno) Then visti.Add(chiamata.Turno)
        Next

        Return String.Join(" → ", visti)

    End Function

    ''' <summary>
    ''' Tutto ciò che l'utente ha detto, in un testo solo: è il pagliaio in cui
    ''' l'anti-invenzione cerca i valori del profilo. Il posto del CV, qui, lo prende la
    ''' traccia.
    ''' </summary>
    Public Shared Function TestoDetto(battiti As List(Of Battito)) As String

        Return String.Join(vbLf, battiti.Where(Function(b) b.Risposta IsNot Nothing).
                                         Select(Function(b) b.Risposta))

    End Function

    ''' <summary>Una mossa del dialogo, come si leggerebbe a schermo.</summary>
    ''' <param name="chiParla">Il nome della persona della traccia: è lei che risponde.</param>
    Public Shared Sub Trascrivi(testo As Text.StringBuilder, battito As Battito, chiParla As String)

        For Each detto As String In battito.Mossa.Detto
            For Each riga As String In detto.Split(ChrW(10))
                testo.Append("> ").Append(riga).Append(vbLf)
            Next
            testo.Append(">").Append(vbLf)
        Next

        If Not String.IsNullOrEmpty(battito.Mossa.EcoUtente) Then
            testo.Append($"> *(le tue parole)* «{battito.Mossa.EcoUtente}»").Append(vbLf).Append(">").Append(vbLf)
        End If

        For Each scheda As Scheda In battito.Mossa.Schede
            If Not String.IsNullOrEmpty(scheda.Titolo) Then
                testo.Append($"> **{scheda.Titolo}**").Append(vbLf)
            End If
            For Each riga As RigaScheda In scheda.Righe
                testo.Append("> - ").
                      Append(If(riga.Etichetta = "", "", riga.Etichetta & ": ")).
                      Append(riga.Valore).Append(vbLf)
            Next
            testo.Append(">").Append(vbLf)
        Next

        If battito.Risposta IsNot Nothing Then
            testo.Append(vbLf).Append($"**{chiParla}:** {battito.Risposta}").Append(vbLf).Append(vbLf)
        ElseIf battito.Scelta IsNot Nothing Then
            Dim etichetta As String = battito.Mossa.Scelte.
                Where(Function(s) s.Id = battito.Scelta).
                Select(Function(s) s.Etichetta).FirstOrDefault()
            testo.Append(vbLf).Append($"**{chiParla}:** *[{etichetta}]*").Append(vbLf).Append(vbLf)
        Else
            testo.Append(vbLf)
        End If

    End Sub

    ''' <summary>Una frase lunga ridotta a un promemoria leggibile.</summary>
    Public Shared Function Accorcia(testo As String) As String

        Dim pulito As String = NonRegressione.CollaudoReale.Ripulito(testo)
        Return If(pulito.Length <= 60, pulito, pulito.Substring(0, 57) & "…")

    End Function

End Class
