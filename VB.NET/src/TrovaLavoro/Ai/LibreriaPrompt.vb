Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Text.RegularExpressions

Namespace Ai

    ''' <summary>Da dove arriva il pool in uso (cap. 4.2).</summary>
    Public Enum OriginePool
        ''' <summary>La copia incorporata nell'eseguibile.</summary>
        Integrato
        ''' <summary>La cartella prompt-pool accanto all'eseguibile, che ha la precedenza.</summary>
        Esterno
    End Enum

    ''' <summary>
    ''' Un prompt del pool: i suoi metadati e il suo testo (cap. 4.4).
    ''' </summary>
    Public Class Prompt
        Public Property Id As String
        Public Property Versione As String
        Public Property Lingua As String
        ''' <summary>Il <i>livello</i> di modello: "semplice" o "ragionamento", mai un nome concreto.</summary>
        Public Property Modello As String
        Public Property MaxToken As Integer
        ''' <summary>"json" oppure "testo".</summary>
        Public Property Uscita As String
        Public Property Segnaposto As IReadOnlyList(Of String)
        Public Property Descrizione As String
        ''' <summary>Il testo del prompt, coi segnaposto ancora da riempire.</summary>
        Public Property Corpo As String
    End Class

    ''' <summary>
    ''' Il caricatore della libreria dei prompt (cap. 4.6). Sceglie la sorgente,
    ''' valida il manifest, carica i prompt su richiesta e ne riempie i segnaposto.
    ''' </summary>
    ''' <remarks>
    ''' Il pool è aperto di proposito, e il caricatore è <b>trasparente, non
    ''' poliziesco</b>: un file che non corrisponde alla sua impronta non blocca
    ''' nulla, ma la versione si mostra con un asterisco. Chi sperimenta lo fa alla
    ''' luce del sole; chi distribuisce fa il bump (cap. 4.5).
    ''' </remarks>
    Public Class LibreriaPrompt

        Private Const CartellaPool As String = "prompt-pool"
        Private Const NomeManifest As String = "pool_manifest.json"
        Private Const PrefissoRisorsa As String = "pool/"

        ' Dove leggere i file: dalla cartella esterna o dalle risorse dell'eseguibile.
        Private ReadOnly _leggi As Func(Of String, String)
        Private ReadOnly _percorsi As List(Of String)
        Private ReadOnly _cache As New Dictionary(Of String, Prompt)(StringComparer.OrdinalIgnoreCase)

        ''' <summary>La versione del pool; con l'asterisco se qualche file è stato modificato.</summary>
        Public ReadOnly Property Versione As String

        ''' <summary>Da dove è stato caricato il pool in uso.</summary>
        Public ReadOnly Property Origine As OriginePool

        ''' <summary>
        ''' Perché si è ripiegato sul pool integrato, oppure quali file non
        ''' corrispondono alla loro impronta; <c>Nothing</c> se è tutto in ordine.
        ''' </summary>
        Public ReadOnly Property Avviso As String

        ''' <summary>
        ''' L'etichetta da mostrare accanto al logo: <c>Pool 1.00</c> se esterno,
        ''' <c>Pool 1.00 (integrato)</c> altrimenti (cap. 4.2).
        ''' </summary>
        Public ReadOnly Property Etichetta As String
            Get
                Return If(Origine = OriginePool.Esterno, $"Pool {Versione}", $"Pool {Versione} (integrato)")
            End Get
        End Property

        Private Sub New(leggi As Func(Of String, String), percorsi As List(Of String),
                        versione As String, origine As OriginePool, avviso As String)
            _leggi = leggi
            _percorsi = percorsi
            Me.Versione = versione
            Me.Origine = origine
            Me.Avviso = avviso
        End Sub

        ''' <summary>
        ''' Apre la libreria. Se accanto all'eseguibile c'è una cartella
        ''' <c>prompt-pool</c> valida, quella sostituisce il pool integrato; se è
        ''' invalida si ripiega sull'integrato dicendolo, mai un avvio muto con
        ''' prompt sbagliati (cap. 4.2).
        ''' </summary>
        ''' <param name="cartellaEsterna">
        ''' La cartella del pool esterno; se omessa, quella accanto all'eseguibile.
        ''' </param>
        Public Shared Function Apri(Optional cartellaEsterna As String = Nothing) As LibreriaPrompt

            Dim cartella As String = If(cartellaEsterna, PercorsoPoolEsterno())
            Dim motivo As String = Nothing

            If Directory.Exists(cartella) Then
                Dim esterno As LibreriaPrompt = ProvaEsterno(cartella, motivo)
                If esterno IsNot Nothing Then Return esterno
            End If

            Dim integrato As LibreriaPrompt = ApriIntegrato()
            If motivo Is Nothing Then Return integrato

            Return New LibreriaPrompt(integrato._leggi, integrato._percorsi, integrato.Versione,
                                      OriginePool.Integrato,
                                      $"Pool esterno ignorato ({motivo}): uso quello integrato.")
        End Function

        ''' <summary>La cartella del pool esterno: accanto all'eseguibile in esecuzione.</summary>
        Public Shared Function PercorsoPoolEsterno() As String
            Return Path.Combine(AppContext.BaseDirectory, CartellaPool)
        End Function

        Private Shared Function ProvaEsterno(cartella As String, ByRef motivo As String) As LibreriaPrompt

            Dim leggi As Func(Of String, String) =
                Function(percorso) File.ReadAllText(Path.Combine(cartella, percorso.Replace("/"c, Path.DirectorySeparatorChar)), Encoding.UTF8)

            Dim testoManifest As String
            Try
                testoManifest = leggi(NomeManifest)
            Catch ex As Exception When TypeOf ex Is IOException OrElse TypeOf ex Is UnauthorizedAccessException
                motivo = "manifest non leggibile"
                Return Nothing
            End Try

            Return DaManifest(testoManifest, leggi, OriginePool.Esterno, motivo)

        End Function

        Private Shared Function ApriIntegrato() As LibreriaPrompt

            Dim assembly As Assembly = GetType(LibreriaPrompt).Assembly

            ' I nomi delle risorse conservano il separatore di cartella di Windows:
            ' si normalizza per parlare sempre di percorsi con la barra in avanti.
            Dim risorse As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each nome As String In assembly.GetManifestResourceNames()
                If nome.StartsWith(PrefissoRisorsa, StringComparison.Ordinal) Then
                    risorse(nome.Substring(PrefissoRisorsa.Length).Replace("\"c, "/"c)) = nome
                End If
            Next

            Dim leggi As Func(Of String, String) =
                Function(percorso)
                    Dim nome As String = Nothing
                    If Not risorse.TryGetValue(percorso, nome) Then
                        Throw New FileNotFoundException($"Il pool integrato non contiene «{percorso}».")
                    End If
                    Using flusso As Stream = assembly.GetManifestResourceStream(nome)
                        Using lettore As New StreamReader(flusso, Encoding.UTF8)
                            Return lettore.ReadToEnd()
                        End Using
                    End Using
                End Function

            Dim motivo As String = Nothing
            Dim libreria As LibreriaPrompt = DaManifest(leggi(NomeManifest), leggi, OriginePool.Integrato, motivo)

            If libreria Is Nothing Then
                ' Il pool integrato è parte dell'eseguibile: se non è valido, il
                ' programma è stato costruito male e non c'è ripiego possibile.
                Throw New InvalidOperationException($"Pool integrato non valido: {motivo}.")
            End If

            Return libreria

        End Function

        ''' <summary>
        ''' Valida il manifest e costruisce la libreria; restituisce <c>Nothing</c>
        ''' con il motivo se il pool non è utilizzabile.
        ''' </summary>
        Private Shared Function DaManifest(testoManifest As String, leggi As Func(Of String, String),
                                           origine As OriginePool, ByRef motivo As String) As LibreriaPrompt

            Dim radice As JsonObject
            Try
                radice = TryCast(JsonNode.Parse(testoManifest), JsonObject)
            Catch ex As JsonException
                motivo = "manifest non è JSON valido"
                Return Nothing
            End Try

            If radice Is Nothing Then
                motivo = "manifest non è un oggetto JSON"
                Return Nothing
            End If

            Dim versione As String = TryCast(radice("versione_pool"), JsonValue)?.ToString()
            Dim elenco As JsonArray = TryCast(radice("file"), JsonArray)

            If String.IsNullOrWhiteSpace(versione) OrElse elenco Is Nothing OrElse elenco.Count = 0 Then
                motivo = "manifest incompleto"
                Return Nothing
            End If

            Dim percorsi As New List(Of String)
            Dim modificati As New List(Of String)

            For Each voce As JsonNode In elenco
                Dim oggetto As JsonObject = TryCast(voce, JsonObject)
                If oggetto Is Nothing Then Continue For

                Dim percorso As String = TryCast(oggetto("percorso"), JsonValue)?.ToString()
                Dim improntaAttesa As String = TryCast(oggetto("sha256"), JsonValue)?.ToString()
                If String.IsNullOrWhiteSpace(percorso) Then Continue For

                Dim contenuto As String
                Try
                    contenuto = leggi(percorso)
                Catch ex As Exception When TypeOf ex Is IOException OrElse
                                           TypeOf ex Is UnauthorizedAccessException
                    ' Un file dichiarato che non c'è — o non si lascia leggere — rende
                    ' il pool inutilizzabile: un prompt mancante si scoprirebbe a metà
                    ' di un flusso. Con questo ritorno scatta il ripiego sull'integrato.
                    motivo = $"manca o non si legge il file «{percorso}»"
                    Return Nothing
                End Try

                percorsi.Add(percorso)

                If Not String.IsNullOrWhiteSpace(improntaAttesa) AndAlso
                   Not String.Equals(Impronta(contenuto), improntaAttesa, StringComparison.OrdinalIgnoreCase) Then
                    modificati.Add(percorso)
                End If
            Next

            If percorsi.Count = 0 Then
                motivo = "manifest senza file utilizzabili"
                Return Nothing
            End If

            ' Trasparente, non poliziesco: si segnala, non si blocca.
            Dim avviso As String = Nothing
            If modificati.Count > 0 Then
                versione &= "*"
                avviso = $"File modificati rispetto al manifest: {String.Join(", ", modificati)}."
            End If

            Return New LibreriaPrompt(leggi, percorsi, versione, origine, avviso)

        End Function

        ''' <summary>
        ''' L'impronta di un file del pool. Si calcola sul testo normalizzato a LF,
        ''' così non dipende dall'editor usato né da come i file sono finiti su disco
        ''' (cap. 4.4).
        ''' </summary>
        Public Shared Function Impronta(contenuto As String) As String
            Dim normalizzato As String = contenuto.Replace(vbCrLf, vbLf)
            Return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizzato))).ToLowerInvariant()
        End Function

        ''' <summary>
        ''' Sigilla un pool: ricalcola le impronte di tutti i suoi <c>.md</c> e
        ''' riscrive <c>pool_manifest.json</c>. È il rito del bump descritto nel
        ''' cap. 4.5 — si modificano i file, si aggiorna la versione, si rigenerano
        ''' le impronte, si annota il changelog.
        ''' </summary>
        ''' <remarks>
        ''' Vive qui, e non in uno strumento a parte, perché la regola con cui si
        ''' calcola un'impronta deve stare in un posto solo: se il sigillo e il
        ''' caricatore la calcolassero ognuno a modo suo, prima o poi divergerebbero
        ''' e il pool risulterebbe modificato senza esserlo. Il comando «Sigilla pool»
        ''' delle Impostazioni non farà altro che chiamare questo.
        ''' </remarks>
        ''' <param name="cartella">La cartella del pool da sigillare.</param>
        ''' <param name="versionePool">La nuova versione, es. "1.01".</param>
        ''' <param name="data">La data del sigillo; se omessa, oggi.</param>
        Public Shared Sub Sigilla(cartella As String, versionePool As String,
                                  Optional data As String = Nothing)

            If Not Directory.Exists(cartella) Then
                Throw New DirectoryNotFoundException($"Non trovo la cartella del pool «{cartella}».")
            End If

            Dim elenco As New JsonArray()

            Dim file As IEnumerable(Of String) =
                Directory.EnumerateFiles(cartella, "*.md", SearchOption.AllDirectories).
                          Select(Function(percorso) Path.GetRelativePath(cartella, percorso).
                                                         Replace(Path.DirectorySeparatorChar, "/"c)).
                          OrderBy(Function(percorso) percorso, StringComparer.Ordinal)

            For Each relativo As String In file

                Dim contenuto As String = IO.File.ReadAllText(
                    Path.Combine(cartella, relativo.Replace("/"c, Path.DirectorySeparatorChar)), Encoding.UTF8)

                ' Nel pool vive anche il CHANGELOG, che è un documento e non un prompt.
                ' Sigillarlo renderebbe il rito del bump contraddittorio: si sigilla, si
                ' annota il changelog, e il pool risulta subito modificato.
                If Not SembraUnPrompt(contenuto) Then Continue For

                elenco.Add(New JsonObject From {
                    {"percorso", relativo},
                    {"sha256", Impronta(contenuto)}})

            Next

            Dim manifest As New JsonObject From {
                {"formato", 1},
                {"versione_pool", versionePool},
                {"data", If(data, DateTime.Today.ToString("yyyy-MM-dd"))},
                {"file", elenco}}

            IO.File.WriteAllText(Path.Combine(cartella, NomeManifest),
                                 manifest.ToJsonString(New JsonSerializerOptions With {.WriteIndented = True}) & vbLf,
                                 New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))

        End Sub

        ''' <summary>
        ''' Se un file del pool è un <b>prompt</b> e non un documento. Si riconosce dal
        ''' frontmatter (cap. 4.4): una riga <c>---</c> che chiude un'intestazione fatta
        ''' <i>tutta</i> di righe «chiave: valore».
        ''' </summary>
        ''' <remarks>
        ''' Cercare la sola riga <c>---</c> non basta, ed era una trappola pronta a
        ''' scattare: un documento come il <c>CHANGELOG</c> può contenerne una in mezzo
        ''' alla prosa — una linea orizzontale in Markdown si scrive proprio così — e
        ''' finirebbe nel manifest. Da lì il rito del bump si morderebbe la coda: si
        ''' sigilla, si annota il changelog, e il pool appena sigillato risulta modificato.
        ''' </remarks>
        Private Shared Function SembraUnPrompt(contenuto As String) As Boolean

            Dim testo As String = contenuto.Replace(vbCrLf, vbLf)

            Dim separatore As Integer = testo.IndexOf(vbLf & "---" & vbLf, StringComparison.Ordinal)
            If separatore <= 0 Then Return False

            For Each riga As String In testo.Substring(0, separatore).Split(vbLf)

                ' Nell'intestazione ogni riga porta un metadato: niente righe vuote,
                ' niente prosa. La chiave sta prima dei due punti ed è una parola sola,
                ' così una frase che i due punti li ha in mezzo non passa per metadato.
                Dim duePunti As Integer = riga.IndexOf(":"c)
                If duePunti <= 0 Then Return False
                If riga.Substring(0, duePunti).Contains(" "c) Then Return False

            Next

            Return True

        End Function

        ''' <summary>
        ''' Carica un prompt per identificativo, scegliendo la variante di lingua
        ''' quando esiste. Il risultato resta in cache: il file si legge una volta sola.
        ''' </summary>
        ''' <param name="id">L'identificativo del prompt, es. "confronto".</param>
        ''' <param name="lingua">La lingua desiderata; si ripiega sulla versione senza lingua.</param>
        Public Function Carica(id As String, Optional lingua As String = "it") As Prompt

            ' La cache va protetta: le continuazioni con ConfigureAwait(False) possono
            ' arrivare qui dal thread pool mentre il thread dell'interfaccia carica un
            ' altro prompt, e un Dictionary conteso si corrompe senza dare segni.
            Dim chiave As String = $"{id}|{lingua}"
            SyncLock _cache
                Dim inCache As Prompt = Nothing
                If _cache.TryGetValue(chiave, inCache) Then Return inCache
            End SyncLock

            Dim percorso As String = TrovaPercorso(id, lingua)
            If percorso Is Nothing Then
                Throw New FileNotFoundException($"Nel pool non c'è un prompt «{id}» (lingua «{lingua}»).")
            End If

            Dim p As Prompt = Interpreta(_leggi(percorso), percorso)
            SyncLock _cache
                _cache(chiave) = p
            End SyncLock
            Return p

        End Function

        ''' <summary>Cerca prima la variante di lingua, poi il file senza lingua.</summary>
        Private Function TrovaPercorso(id As String, lingua As String) As String

            Dim conLingua As String = $"/{id}.{lingua}.md"
            Dim senzaLingua As String = $"/{id}.md"

            For Each percorso As String In _percorsi
                If ("/" & percorso).EndsWith(conLingua, StringComparison.OrdinalIgnoreCase) Then Return percorso
            Next
            For Each percorso As String In _percorsi
                If ("/" & percorso).EndsWith(senzaLingua, StringComparison.OrdinalIgnoreCase) Then Return percorso
            Next

            Return Nothing

        End Function

        ''' <summary>
        ''' Separa l'intestazione dal corpo e verifica che i segnaposto dichiarati e
        ''' quelli presenti nel testo coincidano: un errore qui blocca subito, con
        ''' messaggio chiaro, invece di produrre una chiamata monca (cap. 4.4).
        ''' </summary>
        Private Shared Function Interpreta(contenuto As String, percorso As String) As Prompt

            Dim testo As String = contenuto.Replace(vbCrLf, vbLf)
            Dim separatore As Integer = testo.IndexOf(vbLf & "---" & vbLf, StringComparison.Ordinal)
            If separatore < 0 Then
                Throw New InvalidDataException($"«{percorso}»: manca la riga --- che separa i metadati dal testo.")
            End If

            Dim intestazione As String = testo.Substring(0, separatore)

            ' L'a capo finale è di chi ha salvato il file, non di chi ha scritto il
            ' prompt: qualunque editor lo aggiunge o lo toglie da sé. Toglierlo qui fa
            ' sì che il testo consegnato all'AI dipenda solo dal contenuto — ed è anche
            ' ciò che lo rende identico a quello del prototipo, dove i prompt erano
            ' stringhe dentro il codice e finivano senza a capo (cap. 14, T2).
            Dim corpo As String = testo.Substring(separatore + 5).TrimEnd(ChrW(10), ChrW(13))

            Dim metadati As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            For Each riga As String In intestazione.Split(vbLf)
                Dim duePunti As Integer = riga.IndexOf(":"c)
                If duePunti > 0 Then
                    metadati(riga.Substring(0, duePunti).Trim()) = riga.Substring(duePunti + 1).Trim()
                End If
            Next

            Dim dichiarati As List(Of String) =
                If(metadati.ContainsKey("segnaposto"), metadati("segnaposto"), String.Empty).
                Split(","c).Select(Function(s) s.Trim()).Where(Function(s) s.Length > 0).ToList()

            ' Il controllo va in entrambe le direzioni: un segnaposto dichiarato ma
            ' assente resterebbe senza dato, uno presente ma non dichiarato non
            ' verrebbe mai riempito e finirebbe nel prompt come «{{NOME}}».
            For Each nome As String In dichiarati
                If Not corpo.Contains("{{" & nome & "}}") Then
                    Throw New InvalidDataException($"«{percorso}»: il segnaposto {{{{{nome}}}}} è dichiarato ma non compare nel testo.")
                End If
            Next
            For Each trovato As Match In Regex.Matches(corpo, "\{\{([A-Z_][A-Z0-9_]*)\}\}")
                Dim nome As String = trovato.Groups(1).Value
                If Not dichiarati.Contains(nome) Then
                    Throw New InvalidDataException($"«{percorso}»: il segnaposto {{{{{nome}}}}} compare nel testo ma non è dichiarato.")
                End If
            Next

            Dim maxToken As Integer
            Integer.TryParse(If(metadati.ContainsKey("max_token"), metadati("max_token"), "0"), maxToken)

            Return New Prompt With {
                .Id = If(metadati.ContainsKey("id"), metadati("id"), Nothing),
                .Versione = If(metadati.ContainsKey("versione"), metadati("versione"), Nothing),
                .Lingua = If(metadati.ContainsKey("lingua"), metadati("lingua"), Nothing),
                .Modello = If(metadati.ContainsKey("modello"), metadati("modello"), Nothing),
                .MaxToken = maxToken,
                .Uscita = If(metadati.ContainsKey("uscita"), metadati("uscita"), Nothing),
                .Segnaposto = dichiarati,
                .Descrizione = If(metadati.ContainsKey("descrizione"), metadati("descrizione"), Nothing),
                .Corpo = corpo
            }

        End Function

        ''' <summary>
        ''' Riempie i segnaposto di un prompt. Ogni segnaposto dichiarato deve avere
        ''' il suo valore: se ne manca uno l'errore arriva subito, non sotto forma di
        ''' una chiamata all'AI con un buco dentro (cap. 4.6).
        ''' </summary>
        Public Shared Function Riempi(prompt As Prompt, valori As IDictionary(Of String, String)) As String

            If prompt Is Nothing Then Throw New ArgumentNullException(NameOf(prompt))
            If valori Is Nothing Then valori = New Dictionary(Of String, String)

            ' Prima si controlla che ogni segnaposto abbia il suo valore, poi si
            ' sostituisce in un passaggio solo: sostituire in sequenza riaprirebbe il
            ' testo già riempito — se un dato dell'utente contenesse la stringa
            ' «{{NOME}}» di un segnaposto successivo, verrebbe rimpiazzato pure lui.
            For Each nome As String In prompt.Segnaposto
                Dim valore As String = Nothing
                If valori Is Nothing OrElse Not valori.TryGetValue(nome, valore) OrElse valore Is Nothing Then
                    Throw New ArgumentException($"Manca il valore per il segnaposto {{{{{nome}}}}} del prompt «{prompt.Id}».")
                End If
            Next

            Return Regex.Replace(prompt.Corpo, "\{\{([A-Z_][A-Z0-9_]*)\}\}",
                Function(trovato)
                    Dim valore As String = Nothing
                    Return If(valori.TryGetValue(trovato.Groups(1).Value, valore),
                              valore, trovato.Value)
                End Function)

        End Function

        ''' <summary>
        ''' Serializza un artefatto JSON (profilo, annuncio, giudizi…) come va scritto
        ''' <i>dentro</i> un prompt: indentato di due spazi, con gli accenti in chiaro.
        ''' </summary>
        ''' <remarks>
        ''' Sta qui, e non nel punto in cui serve, perché deve produrre esattamente ciò
        ''' che produce <c>JSON.stringify(x, null, 2)</c> del prototipo: è la condizione
        ''' perché la richiesta che parte sia identica alla sua (cap. 14, T2). L'encoder
        ''' predefinito di .NET, invece, sostituirebbe ogni lettera accentata con la sua
        ''' sequenza di escape — «Forlì» scritto a codici — e con lei anche apostrofi e
        ''' segni come &amp; o &lt;: per una macchina è lo stesso JSON, per il modello è
        ''' un prompt diverso da quello con cui il prototipo è stato validato.
        ''' </remarks>
        Public Shared Function ComeNelPrompt(artefatto As JsonNode) As String

            If artefatto Is Nothing Then Throw New ArgumentNullException(NameOf(artefatto))

            Return artefatto.ToJsonString(New JsonSerializerOptions With {
                .WriteIndented = True,
                .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping})

        End Function

    End Class

End Namespace
