Imports System.IO
Imports System.Text.Json.Nodes
Imports TrovaLavoro.Ai

''' <summary>
''' I casi della batteria di non-regressione contro il prototipo (cap. 14, T2) e i
''' pochi servizi che servono a entrambe le batterie che li usano: quella del prompt
''' (senza rete) e quella del confronto reale.
''' </summary>
''' <remarks>
''' Profilo e annunci sono <b>inventati</b>: il repo è pubblico, e un collaudo non
''' vale di più perché contiene dati veri. Sono però scritti come li scriverebbe
''' l'app — stessi campi degli schemi del pool — e portano accenti e apostrofi di
''' proposito, perché è lì che una differenza di codifica si vede.
''' </remarks>
Friend Module CasiDiCollaudo

    ''' <summary>Il caso «annuncio compatibile»: nessun requisito eliminatorio.</summary>
    Public Const Compatibile As String = "compatibile"

    ''' <summary>Il caso «patente C indispensabile»: il candidato ha solo la B.</summary>
    Public Const Eliminatorio As String = "eliminatorio"

    ''' <summary>I due casi, col file di annuncio di ciascuno.</summary>
    Public ReadOnly Casi As New Dictionary(Of String, String) From {
        {Compatibile, "annuncio_compatibile.json"},
        {Eliminatorio, "annuncio_eliminatorio.json"}}

    ''' <summary>
    ''' La cartella <c>casi/</c> nei sorgenti — non quella copiata accanto alla dll:
    ''' il collaudo reale ci <i>scrive</i> dentro i suoi esiti, e devono finire nel
    ''' repo, non in <c>bin/</c>. Si risale dalla cartella di esecuzione fino a
    ''' trovare il progetto del banco.
    ''' </summary>
    Public ReadOnly Property Cartella As String
        Get
            Dim qui As New DirectoryInfo(AppContext.BaseDirectory)
            While qui IsNot Nothing
                If File.Exists(Path.Combine(qui.FullName, "TrovaLavoro.Collaudi.vbproj")) Then
                    Return Path.Combine(qui.FullName, "casi")
                End If
                qui = qui.Parent
            End While
            Throw New DirectoryNotFoundException(
                "Non trovo la cartella dei casi: il banco va eseguito dalla sua soluzione.")
        End Get
    End Property

    ''' <summary>Legge un artefatto JSON dei casi (il profilo, un annuncio).</summary>
    Public Function Artefatto(nomeFile As String) As JsonNode
        Return JsonNode.Parse(File.ReadAllText(Path.Combine(Cartella, nomeFile), Text.Encoding.UTF8))
    End Function

    ''' <summary>Il profilo del candidato di collaudo.</summary>
    Public Function Profilo() As JsonNode
        Return Artefatto("profilo.json")
    End Function

    ''' <summary>L'annuncio di uno dei due casi.</summary>
    Public Function Annuncio(caso As String) As JsonNode
        Return Artefatto(Casi(caso))
    End Function

    ''' <summary>
    ''' I giudizi dell'anello 3 per uno dei due casi: la <b>lista</b> che il confronto
    ''' produce e che la mitigazione riceve.
    ''' </summary>
    ''' <remarks>
    ''' Sono un artefatto a sé, e non l'uscita di una chiamata: così la parità del
    ''' prompt di mitigazione resta verificabile senza rete, come quella del confronto.
    ''' Vengono dal collaudo reale di T2 — li ha prodotti il prototipo sul profilo e
    ''' sull'annuncio inventati di questa cartella — quindi hanno la forma vera.
    ''' </remarks>
    Public Function Giudizi(caso As String) As JsonNode
        Return Artefatto($"giudizi_{caso}.json")
    End Function

    ''' <summary>
    ''' Il prompt del confronto già riempito, esattamente come lo manderebbe l'app:
    ''' prompt del pool, artefatti serializzati come nel prototipo.
    ''' </summary>
    Public Function PromptConfronto(libreria As LibreriaPrompt, caso As String) As String

        Dim confronto As Prompt = libreria.Carica("confronto")

        Return LibreriaPrompt.Riempi(confronto, New Dictionary(Of String, String) From {
            {"PROFILO", LibreriaPrompt.ComeNelPrompt(Profilo())},
            {"ANNUNCIO", LibreriaPrompt.ComeNelPrompt(Annuncio(caso))}})

    End Function

    ''' <summary>
    ''' Il prompt della mitigazione già riempito, come sopra: profilo e giudizi dello
    ''' stesso caso.
    ''' </summary>
    Public Function PromptMitigazione(libreria As LibreriaPrompt, caso As String) As String

        Dim mitigazione As Prompt = libreria.Carica("mitigazione")

        Return LibreriaPrompt.Riempi(mitigazione, New Dictionary(Of String, String) From {
            {"PROFILO", LibreriaPrompt.ComeNelPrompt(Profilo())},
            {"GIUDIZI", LibreriaPrompt.ComeNelPrompt(Giudizi(caso))}})

    End Function

    ''' <summary>
    ''' Il prompt che il prototipo costruisce per lo stesso caso, generato eseguendo la
    ''' funzione corrispondente di <c>HTML+JS/server.js</c> su questi stessi artefatti
    ''' (v. <c>genera_attesi.mjs</c>).
    ''' </summary>
    ''' <param name="idPrompt">"confronto" o "mitigazione": i due su cui la parità vale
    ''' ancora carattere per carattere (cap. 04.7).</param>
    Public Function PromptAtteso(idPrompt As String, caso As String) As String
        Return File.ReadAllText(
            Path.Combine(Cartella, "atteso", $"prompt_{idPrompt}_{caso}.txt"), Text.Encoding.UTF8)
    End Function

    ''' <summary>
    ''' Riporta la prima differenza fra due testi, con il contorno per riconoscerla;
    ''' <c>Nothing</c> se sono identici. Le fini riga si normalizzano a LF prima del
    ''' confronto, come per le impronte del pool (cap. 4.4): il sorgente del prototipo
    ''' su disco è CRLF, il pool è LF, e questa è una differenza di file, non di testo.
    ''' </summary>
    Public Function PrimaDifferenza(atteso As String, ottenuto As String) As String

        Dim a As String = atteso.Replace(vbCrLf, vbLf)
        Dim b As String = ottenuto.Replace(vbCrLf, vbLf)

        If String.Equals(a, b, StringComparison.Ordinal) Then Return Nothing

        Dim comuni As Integer = 0
        While comuni < a.Length AndAlso comuni < b.Length AndAlso a(comuni) = b(comuni)
            comuni += 1
        End While

        Dim riga As Integer = a.Substring(0, comuni).Split(ChrW(10)).Length

        Return $"differiscono al carattere {comuni} (riga {riga}):" & vbLf &
               $"  prototipo: …{Estratto(a, comuni)}" & vbLf &
               $"  app:       …{Estratto(b, comuni)}"

    End Function

    ''' <summary>Un pezzetto di testo attorno al punto in cui due testi divergono.</summary>
    Private Function Estratto(testo As String, da As Integer) As String

        If da >= testo.Length Then Return "(qui finisce)"

        Dim quanti As Integer = Math.Min(60, testo.Length - da)
        Return testo.Substring(da, quanti).Replace(vbLf, "⏎")

    End Function

End Module
