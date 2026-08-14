Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading
Imports System.Threading.Tasks
Imports TrovaLavoro.Motore

Namespace Ai

    ''' <summary>
    ''' La meccanica che tutti i mestieri AI hanno in comune: prendere il prompt dal
    ''' pool, riempirne i segnaposti, chiedere all'AI ed estrarre il JSON da ciò che
    ''' torna. Nessuno di questi passi è nuovo — sono i tre pezzi collaudati a T2
    ''' (<see cref="LibreriaPrompt"/>, <see cref="ClientClaude"/>,
    ''' <see cref="EstrattoreJson"/>) messi in fila come già faceva
    ''' <see cref="StrutturatoreTurni"/>; qui stanno in un posto solo perché a T4 i
    ''' mestieri diventano sei e la fila non va ricopiata sei volte.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>I messaggi sono interfaccia.</b> Quello che questi <see cref="ErroreAi"/>
    ''' dicono non finisce in un log: il dialogo lo infila dritto in una bolla di chat
    ''' (cap. 12) e i pannelli lo mostrano com'è. Per questo l'<c>etichetta</c> è un
    ''' parametro e non l'identificativo del prompt: l'utente deve leggere «il turno
    ''' «nome»» o «l'analisi dell'annuncio», non un nome di file.</para>
    ''' <para><b>Il Catch resta stretto.</b> Si catturano solo gli inciampi del pool
    ''' (<see cref="InvalidDataException"/>, <see cref="IOException"/>,
    ''' <see cref="ArgumentException"/>): un <see cref="ErroreAi"/> che sale dal client —
    ''' rete caduta, chiave sbagliata, risposta troncata — deve arrivare a chi legge con
    ''' le <b>sue</b> parole, già scritte in italiano per l'utente (cap. 02.5). Un
    ''' <c>Catch ex As Exception</c> qui riscriverebbe la voce dell'applicazione.</para>
    ''' <para>Livello di modello e limite di token non si passano mai: li dichiara il
    ''' prompt nei suoi metadati (cap. 04) ed è <c>ChiediAsync</c> a leggerli da lì. È
    ''' anche il motivo per cui un mestiere non deve sapere nulla dei modelli.</para>
    ''' </remarks>
    Public MustInherit Class MestiereAi

        ''' <summary>Il pool da cui vengono i prompt.</summary>
        Protected ReadOnly Property Libreria As LibreriaPrompt

        ''' <summary>Il client dell'AI, già con la sua chiave.</summary>
        Protected ReadOnly Property Client As ClientClaude

        ''' <param name="libreria">Il pool da cui vengono i prompt.</param>
        ''' <param name="client">Il client dell'AI, già con la sua chiave.</param>
        Protected Sub New(libreria As LibreriaPrompt, client As ClientClaude)

            If libreria Is Nothing Then Throw New ArgumentNullException(NameOf(libreria))
            If client Is Nothing Then Throw New ArgumentNullException(NameOf(client))

            Me.Libreria = libreria
            Me.Client = client

        End Sub

        ''' <summary>
        ''' La fila intera per un mestiere che chiede JSON: prompt dal pool, segnaposti
        ''' riempiti, richiesta all'AI, frammento estratto.
        ''' </summary>
        ''' <param name="idPrompt">L'identificativo del prompt nel pool.</param>
        ''' <param name="etichetta">
        ''' Come si chiama questo lavoro nelle parole dell'utente — «il turno «nome»»,
        ''' «l'analisi dell'annuncio» — perché è così che comparirà nei messaggi d'errore.
        ''' </param>
        ''' <param name="valori">Un valore per ogni segnaposto che il prompt dichiara.</param>
        ''' <param name="annulla">Il gettone del pulsante Annulla (cap. 02.6).</param>
        ''' <param name="lingua">
        ''' Quale variante di lingua del prompt caricare (cap. 04.6). La chiedono solo i
        ''' mestieri che scrivono documenti per l'azienda: il profilo e il confronto
        ''' restano in italiano perché parlano con l'utente (cap. 10.1).
        ''' </param>
        ''' <returns>Il frammento JSON prodotto dall'AI, già estratto.</returns>
        Protected Async Function EseguiAsync(idPrompt As String, etichetta As String,
                                             valori As IDictionary(Of String, String),
                                             Optional annulla As CancellationToken = Nothing,
                                             Optional lingua As String = "it") _
                                             As Task(Of JsonNode)

            Dim prompt As Prompt = CaricaPrompt(idPrompt, lingua)
            Dim testo As String = Riempi(prompt, etichetta, valori)

            Dim uscita As RispostaAi = Await ChiediAsync(
                prompt, JsonValue.Create(testo), annulla).ConfigureAwait(False)

            Return Estrai(uscita, etichetta)

        End Function

        ''' <summary>
        ''' Prende il prompt dal pool traducendo gli inciampi in <see cref="ErroreAi"/>:
        ''' il pool esterno è modificabile per design (cap. 04.2), e chi lo ritocca deve
        ''' ricevere il messaggio in italiano dei pannelli, non un crash a metà lavoro.
        ''' </summary>
        Protected Function CaricaPrompt(idPrompt As String,
                                        Optional lingua As String = "it") As Prompt

            Try
                Return Libreria.Carica(idPrompt, lingua)
            Catch ex As Exception When TypeOf ex Is InvalidDataException OrElse
                                       TypeOf ex Is IOException OrElse
                                       TypeOf ex Is ArgumentException
                Throw ErroreDiPool(idPrompt, ex)
            End Try

        End Function

        ''' <summary>
        ''' Riempie i segnaposti del prompt con i valori del mestiere.
        ''' </summary>
        ''' <remarks>
        ''' Questo inciampo ha due origini che dall'esterno non si distinguono: un valore
        ''' che il mestiere ha dimenticato di passare (un difetto nostro) e un segnaposto
        ''' aggiunto a mano nel pool, per cui nessun valore esisterà mai. Per questo il
        ''' messaggio non accusa il pool — dice che la richiesta non si è potuta preparare
        ''' e riporta quale dato manca, che serve a capire in entrambi i casi.
        ''' </remarks>
        Protected Function Riempi(prompt As Prompt, etichetta As String,
                                  valori As IDictionary(Of String, String)) As String

            Try
                Return LibreriaPrompt.Riempi(prompt, valori)
            Catch ex As ArgumentException
                Throw New ErroreAi(CausaErroreAi.Richiesta,
                    $"Non sono riuscita a preparare la richiesta per {etichetta}: {ex.Message}", ex)
            End Try

        End Function

        ''' <summary>
        ''' Manda la richiesta all'AI col livello e il limite dichiarati dal prompt.
        ''' </summary>
        ''' <remarks>
        ''' Il contenuto è un <see cref="JsonNode"/> e non una stringa perché non tutti i
        ''' mestieri mandano solo testo: la trascrizione di un PDF manda un blocco
        ''' <c>document</c> e poi l'istruzione (cap. 05.1). L'unico inciampo tradotto qui
        ''' è il livello di modello scritto male nel frontmatter, che è ancora un pool
        ''' ritoccato e non deve diventare un crash.
        ''' </remarks>
        Protected Async Function ChiediAsync(prompt As Prompt, contenuto As JsonNode,
                                             Optional annulla As CancellationToken = Nothing) _
                                             As Task(Of RispostaAi)

            Try
                Return Await Client.ChiediAsync(prompt.Modello, contenuto, prompt.MaxToken, annulla).
                    ConfigureAwait(False)
            Catch ex As ArgumentException
                Throw ErroreDiPool(prompt.Id, ex)
            End Try

        End Function

        ''' <summary>
        ''' Sbuccia il JSON dalla risposta.
        ''' </summary>
        ''' <remarks>
        ''' Cap. 02.5: mai un crash davanti a una risposta illeggibile. Il testo grezzo
        ''' viaggia dentro il messaggio perché l'utente possa vedere «cosa ha risposto il
        ''' modello» invece di un errore muto.
        ''' </remarks>
        Protected Shared Function Estrai(uscita As RispostaAi, etichetta As String) As JsonNode

            Try
                Return EstrattoreJson.Estrai(uscita.Testo)
            Catch ex As JsonException
                Throw New ErroreAi(CausaErroreAi.RispostaInattesa,
                    $"L'AI ha risposto in una forma che non riesco a leggere per {etichetta}. " &
                    $"Ecco cosa ha risposto: {uscita.Testo}", ex)
            End Try

        End Function

        ''' <summary>
        ''' Si ferma prima di chiamare l'AI se manca un artefatto d'ingresso.
        ''' </summary>
        ''' <remarks>
        ''' Senza questo, un ingresso mancante diventerebbe un <c>null</c> scritto nel
        ''' prompt e l'AI risponderebbe qualcosa a vuoto: una risposta costa un'attesa e
        ''' dei token, e sarebbe pure plausibile abbastanza da non insospettire nessuno.
        ''' </remarks>
        ''' <param name="quale">Come si chiama l'artefatto: «il profilo», «l'annuncio».</param>
        ''' <param name="etichetta">Il lavoro che non si è potuto preparare.</param>
        Protected Shared Sub Esigi(artefatto As JsonNode, quale As String, etichetta As String)

            If artefatto Is Nothing Then
                Throw New ErroreAi(CausaErroreAi.Richiesta,
                    $"Non sono riuscita a preparare {etichetta}: manca {quale}.")
            End If

        End Sub

        ''' <summary>Il prompt del pool non è utilizzabile: sempre con le stesse parole.</summary>
        Private Shared Function ErroreDiPool(idPrompt As String, ex As Exception) As ErroreAi

            Return New ErroreAi(CausaErroreAi.Richiesta,
                $"Il prompt «{idPrompt}» del pool non è utilizzabile: {ex.Message}", ex)

        End Function

    End Class

End Namespace
