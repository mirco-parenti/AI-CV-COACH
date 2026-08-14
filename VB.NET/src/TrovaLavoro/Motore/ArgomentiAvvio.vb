Namespace Motore

    ''' <summary>
    ''' Quel che l'applicazione accetta dalla riga di comando (cap. 11.1). Oggi una cosa
    ''' sola — la radice della cartella dati — ma il posto dove leggerla è questo, e non
    ''' sparso fra <see cref="Programma"/> e la finestra.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Nessun argomento è un errore fatale.</b> Un percorso storto, un'opzione
    ''' scritta a metà, una parola che il programma non conosce: tutto diventa un avviso e
    ''' un ripiego, mai un rifiuto di partire. Vale la stessa regola del montaggio del
    ''' motore (<see cref="ContestoApp"/>) e per la stessa ragione — chi ha sbagliato a
    ''' scrivere ha bisogno di vedere l'applicazione dirglielo, non di vederla non
    ''' comparire (cap. 03.8).</para>
    ''' <para>La riga di comando è destinata a crescere: T8 aggiungerà <c>--mcp</c>
    ''' (cap. 09). Perciò l'ignoto si segnala e si scavalca, invece di far cadere l'avvio
    ''' di tutto per una parola in più.</para>
    ''' </remarks>
    Public Class ArgomentiAvvio

        ''' <summary>L'opzione con cui si indica la cartella dati: <c>--dati percorso</c>.</summary>
        Public Const OpzioneDati As String = "--dati"

        Private ReadOnly _avvisi As New List(Of String)

        Private Sub New()
        End Sub

        ''' <summary>
        ''' La cartella dati chiesta all'avvio; <c>Nothing</c> significa «quella
        ''' predefinita» (cap. 11.1). Che il percorso esista o si lasci usare non lo
        ''' decide qui: lo scopre chi monta il motore, che sa anche come ripiegare.
        ''' </summary>
        Public ReadOnly Property RadiceDati As String

        ''' <summary>Cosa non si è potuto rispettare, in ordine; vuoto se è filato tutto liscio.</summary>
        Public ReadOnly Property Avvisi As IReadOnlyList(Of String)
            Get
                Return _avvisi
            End Get
        End Property

        ''' <summary>Gli avvisi in una riga sola, pronti per la barra di stato; <c>Nothing</c> se non ce ne sono.</summary>
        Public ReadOnly Property Avviso As String
            Get
                If _avvisi.Count = 0 Then Return Nothing
                Return String.Join(" · ", _avvisi)
            End Get
        End Property

        ''' <summary>
        ''' Legge gli argomenti dell'eseguibile. Sono accettate entrambe le forme
        ''' correnti — <c>--dati percorso</c> e <c>--dati=percorso</c> — perché chi le
        ''' scrive se le aspetta tutt'e due, e sbagliare la forma non deve costare un
        ''' avvio andato a vuoto.
        ''' </summary>
        ''' <param name="argomenti">Gli argomenti <b>senza</b> il nome dell'eseguibile.</param>
        Public Shared Function Leggi(argomenti As IEnumerable(Of String)) As ArgomentiAvvio

            Dim letti As New ArgomentiAvvio()
            If argomenti Is Nothing Then Return letti

            Dim elenco As New List(Of String)(argomenti)
            Dim indice As Integer = 0

            While indice < elenco.Count

                Dim argomento As String = If(elenco(indice), String.Empty).Trim()
                indice += 1
                If argomento.Length = 0 Then Continue While

                Dim nome As String = argomento
                Dim valore As String = Nothing

                ' La forma «--dati=percorso» porta il valore attaccato. L'uguale in prima
                ' posizione non è un separatore ma un argomento che comincia storto: se lo
                ' trattassimo come tale, il nome dell'opzione sarebbe vuoto.
                Dim uguale As Integer = argomento.IndexOf("="c)
                If uguale > 0 Then
                    nome = argomento.Substring(0, uguale)
                    valore = argomento.Substring(uguale + 1)
                End If

                If Not nome.Equals(OpzioneDati, StringComparison.OrdinalIgnoreCase) Then
                    letti.Avvisa($"Non conosco l'argomento «{argomento}»: l'ho ignorato.")
                    Continue While
                End If

                ' La forma «--dati percorso» tiene il valore nell'argomento dopo — ma solo
                ' se quello non è a sua volta un'opzione: «--dati --mcp» non vuol dire che
                ' la cartella dati si chiami «--mcp», vuol dire che il percorso manca.
                If valore Is Nothing AndAlso indice < elenco.Count AndAlso Not PareUnOpzione(elenco(indice)) Then
                    valore = elenco(indice)
                    indice += 1
                End If

                letti.PrendiLaRadice(valore)

            End While

            Return letti

        End Function

        ''' <summary>
        ''' Assegna la radice, se si può. Ripetere l'opzione non è un capriccio da
        ''' assecondare in silenzio: fra due cartelle diverse la scelta la deve fare chi
        ''' scrive il comando, e finché non l'ha fatta vale la prima — l'ultima vincitrice
        ''' silenziosa manderebbe a scrivere in un posto senza che nessuno l'abbia detto.
        ''' </summary>
        Private Sub PrendiLaRadice(valore As String)

            If String.IsNullOrWhiteSpace(valore) Then
                Avvisa($"L'argomento «{OpzioneDati}» vuole un percorso dopo di sé: uso la cartella dati predefinita.")
                Return
            End If

            If RadiceDati IsNot Nothing Then
                Avvisa($"La cartella dati è indicata più di una volta: tengo la prima, «{RadiceDati}».")
                Return
            End If

            _RadiceDati = valore.Trim()

        End Sub

        ''' <summary>Se un argomento ha l'aria di essere un'opzione e non un valore.</summary>
        Private Shared Function PareUnOpzione(argomento As String) As Boolean

            Dim testo As String = If(argomento, String.Empty).Trim()
            Return testo.StartsWith("--", StringComparison.Ordinal)

        End Function

        Private Sub Avvisa(testo As String)
            _avvisi.Add(testo)
        End Sub

    End Class

End Namespace
