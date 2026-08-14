Imports System.Globalization
Imports System.IO
Imports System.Text

Namespace Documenti

    ''' <summary>
    ''' Un allegato dell'email: il file su disco e il nome con cui deve arrivare.
    ''' </summary>
    ''' <remarks>
    ''' Il nome che si vede nel messaggio non è per forza quello del file: un allegato che
    ''' arriva chiamato <c>cv_mirato_20260814.docx</c> dice a chi lo riceve come lavora il
    ''' programma di chi l'ha mandato, non cosa c'è dentro.
    ''' </remarks>
    Public Class AllegatoEmail

        ''' <param name="percorso">Il file da allegare, che deve esistere.</param>
        ''' <param name="nomeMostrato">Con che nome arriva; se omesso, quello del file.</param>
        Public Sub New(percorso As String, Optional nomeMostrato As String = Nothing)

            If String.IsNullOrWhiteSpace(percorso) Then
                Throw New ArgumentException("Un allegato senza percorso non è un allegato.", NameOf(percorso))
            End If

            Me.Percorso = percorso
            Me.NomeMostrato = If(String.IsNullOrWhiteSpace(nomeMostrato), Path.GetFileName(percorso), nomeMostrato)

        End Sub

        Public ReadOnly Property Percorso As String
        Public ReadOnly Property NomeMostrato As String

    End Class

    ''' <summary>
    ''' Scrive il file <c>.eml</c> della candidatura (cap. 07.2): un messaggio di posta
    ''' standard, con gli allegati dentro, marcato come <b>bozza da inviare</b>. Il
    ''' programma non spedisce niente — a spedire è il programma di posta dell'utente, dove
    ''' è già autenticato (cap. 11.2).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché a mano e non con una libreria.</b> Il progetto vieta le dipendenze
    ''' esterne (cap. 13.2), e in .NET la classe che comporrebbe questo messaggio
    ''' (<c>MailMessage</c>) sa consegnarlo a un server SMTP ma non salvarlo come file: la
    ''' scorciatoia che gira in rete — la cartella «pickup» — scrive un formato che non è
    ''' quello che i client si aspettano di aprire. Il MIME di un'email con allegati sta in
    ''' un centinaio di righe, ed è meglio scriverle che dipendere da un pacchetto.</para>
    ''' <para><b>Le tre trappole del MIME</b>, tutte fuori dall'italiano di tutti i giorni:
    ''' le righe finiscono con CRLF e non con l'a capo di Windows a caso; le intestazioni
    ''' sono ASCII, quindi un oggetto con gli accenti va codificato (RFC 2047) o arriva
    ''' illeggibile; e il nome di un allegato accentato vuole la forma dell'RFC 2231, che i
    ''' client moderni leggono, accanto a quella semplice per i vecchi.</para>
    ''' </remarks>
    Public Module ScrittoreEml

        ''' <summary>L'estensione dei messaggi di posta salvati su file.</summary>
        Public Const Estensione As String = ".eml"

        ''' <summary>
        ''' L'intestazione che dice ai programmi di posta «questa è una bozza da inviare, non
        ''' un messaggio ricevuto»: chi la riconosce — Outlook in testa — apre la finestra di
        ''' composizione con dentro già tutto, pronta per «Invia» (cap. 07.2).
        ''' </summary>
        Public Const IntestazioneBozza As String = "X-Unsent"

        ''' <summary>La riga con cui finisce ogni riga di un messaggio di posta.</summary>
        Private Const ACapo As String = vbCrLf

        ''' <summary>Quanto è lunga una riga di base64: è il limite che il formato impone.</summary>
        Private Const LarghezzaBase64 As Integer = 76

        ''' <summary>
        ''' Compone il messaggio e lo scrive nel file indicato.
        ''' </summary>
        ''' <param name="percorso">Dove scrivere il <c>.eml</c>.</param>
        Public Sub Scrivi(percorso As String,
                          mittente As String, destinatario As String,
                          oggetto As String, corpo As String,
                          allegati As IEnumerable(Of AllegatoEmail),
                          Optional quando As Date = Nothing)

            If String.IsNullOrWhiteSpace(percorso) Then
                Throw New ArgumentException("Serve dove scrivere il messaggio.", NameOf(percorso))
            End If

            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(percorso)))

            ' Il file si scrive in byte e non in testo: le parti sono già codificate, e
            ' lasciare che un StreamWriter ci rimetta mano cambierebbe gli a capo.
            File.WriteAllBytes(percorso, Encoding.ASCII.GetBytes(
                Componi(mittente, destinatario, oggetto, corpo, allegati, quando)))

        End Sub

        ''' <summary>
        ''' Il messaggio intero, come testo. Esce da qui e non dal file perché è così che
        ''' si può collaudare: un <c>.eml</c> è una cosa che si legge.
        ''' </summary>
        ''' <param name="mittente">L'indirizzo di chi si candida; se manca, l'intestazione non si scrive.</param>
        ''' <param name="destinatario">L'indirizzo dell'azienda; se manca, la scrive il programma di posta.</param>
        ''' <param name="oggetto">L'oggetto, anche accentato.</param>
        ''' <param name="corpo">Il testo del messaggio, con i suoi a capo.</param>
        ''' <param name="allegati">I file da allegare; possono non essercene.</param>
        ''' <param name="quando">La data del messaggio; adesso, se non si dice.</param>
        Public Function Componi(mittente As String, destinatario As String,
                                oggetto As String, corpo As String,
                                allegati As IEnumerable(Of AllegatoEmail),
                                Optional quando As Date = Nothing) As String

            Dim elenco As New List(Of AllegatoEmail)
            If allegati IsNot Nothing Then elenco.AddRange(allegati)

            Dim confine As String = NuovoConfine()
            Dim messaggio As New StringBuilder()

            ' Un'intestazione vuota non si scrive: «To:» senza indirizzo è peggio che non
            ' dirlo — alcuni client la leggono come un destinatario che si chiama «».
            Aggiungi(messaggio, "From", Indirizzo(mittente))
            Aggiungi(messaggio, "To", Indirizzo(destinatario))
            Aggiungi(messaggio, "Subject", Codificata(SenzaACapo(oggetto)))
            Aggiungi(messaggio, "Date", DataDelMessaggio(If(quando = Nothing, Date.Now, quando)))
            Aggiungi(messaggio, "MIME-Version", "1.0")

            ' La bozza: è questa riga a fare la differenza fra un messaggio che si apre in
            ' composizione e uno che sembra arrivato da qualcuno.
            Aggiungi(messaggio, IntestazioneBozza, "1")

            If elenco.Count = 0 Then

                ' Senza allegati non serve un messaggio a più parti: sarebbe una scatola
                ' con dentro una cosa sola, e certi client lo mostrano come un allegato.
                Aggiungi(messaggio, "Content-Type", "text/plain; charset=utf-8")
                Aggiungi(messaggio, "Content-Transfer-Encoding", "base64")
                messaggio.Append(ACapo)
                messaggio.Append(InBase64(Encoding.UTF8.GetBytes(NormalizzaACapo(corpo))))

                Return messaggio.ToString()

            End If

            Aggiungi(messaggio, "Content-Type", $"multipart/mixed; boundary=""{confine}""")
            messaggio.Append(ACapo)

            ' Chi apre il file con un editor invece che con un programma di posta merita una
            ' riga di spiegazione. È in inglese, come la data qui sopra e come la scrivono
            ' tutti i client: sta **fuori** dalle parti, dove nessuna codifica la protegge —
            ' un accento qui finirebbe in un punto interrogativo, perché il messaggio si
            ' scrive in ASCII (tutto il resto o è già ASCII o è codificato).
            messaggio.Append("This is a multi-part message in MIME format.").Append(ACapo)
            messaggio.Append(ACapo)

            messaggio.Append("--").Append(confine).Append(ACapo)
            Aggiungi(messaggio, "Content-Type", "text/plain; charset=utf-8")
            Aggiungi(messaggio, "Content-Transfer-Encoding", "base64")
            messaggio.Append(ACapo)
            messaggio.Append(InBase64(Encoding.UTF8.GetBytes(NormalizzaACapo(corpo))))
            messaggio.Append(ACapo)

            For Each allegato As AllegatoEmail In elenco

                messaggio.Append("--").Append(confine).Append(ACapo)
                Aggiungi(messaggio, "Content-Type", $"{TipoDi(allegato.NomeMostrato)}; name=""{PerIntestazione(allegato.NomeMostrato)}""")
                Aggiungi(messaggio, "Content-Transfer-Encoding", "base64")
                Aggiungi(messaggio, "Content-Disposition", DisposizioneDi(allegato.NomeMostrato))
                messaggio.Append(ACapo)
                messaggio.Append(InBase64(File.ReadAllBytes(allegato.Percorso)))
                messaggio.Append(ACapo)

            Next

            messaggio.Append("--").Append(confine).Append("--").Append(ACapo)

            Return messaggio.ToString()

        End Function

        ''' <summary>Una riga di intestazione, se ha un valore da portare.</summary>
        Private Sub Aggiungi(messaggio As StringBuilder, nome As String, valore As String)

            If String.IsNullOrWhiteSpace(valore) Then Return
            messaggio.Append(nome).Append(": ").Append(valore).Append(ACapo)

        End Sub

        ''' <summary>
        ''' Un indirizzo come lo vuole l'intestazione. Qui non si valida niente: un
        ''' indirizzo storto è un problema dell'utente e del suo programma di posta, che
        ''' glielo dirà — rifiutarlo qui vorrebbe dire buttare via l'email intera per una
        ''' virgola, e la bozza esiste proprio per essere corretta.
        ''' </summary>
        Private Function Indirizzo(valore As String) As String
            Return If(valore, "").Trim()
        End Function

        ''' <summary>
        ''' La data come la vuole il formato dei messaggi di posta: in inglese e con il
        ''' fuso, sempre — anche su un'applicazione che parla italiano. È l'unico punto in
        ''' cui la lingua di casa non conta, perché a leggerla è un programma.
        ''' </summary>
        Private Function DataDelMessaggio(momento As Date) As String

            Dim istante As New DateTimeOffset(momento)
            Dim fuso As TimeSpan = istante.Offset

            ' Il fuso si scrive «+0200», attaccato: il formato «zzz» di .NET ci mette i due
            ' punti in mezzo, che qui non ci vanno.
            Return istante.ToString("ddd, dd MMM yyyy HH:mm:ss ", CultureInfo.InvariantCulture) &
                   If(fuso < TimeSpan.Zero, "-", "+") &
                   Math.Abs(fuso.Hours).ToString("00", CultureInfo.InvariantCulture) &
                   Math.Abs(fuso.Minutes).ToString("00", CultureInfo.InvariantCulture)

        End Function

        ''' <summary>
        ''' Il confine fra le parti (il nome non può essere «Confine»: in VB le maiuscole
        ''' non distinguono, e la variabile locale che lo raccoglie coprirebbe la funzione).
        ''' Il confine fra le parti: una stringa che <b>non deve comparire</b> nel
        ''' contenuto, altrimenti il messaggio si spezza dove non deve. Un identificativo
        ''' irripetibile lo garantisce meglio di qualunque parola scelta a mano.
        ''' </summary>
        Private Function NuovoConfine() As String
            Return "TrovaLavoro-" & Guid.NewGuid().ToString("N")
        End Function

        ''' <summary>
        ''' Un'intestazione che contiene caratteri non inglesi si codifica (RFC 2047),
        ''' altrimenti l'oggetto arriva a pezzi. Se è tutto ASCII resta com'è: un oggetto
        ''' leggibile anche nel file è un piccolo regalo a chi lo apre con un editor.
        ''' </summary>
        Private Function Codificata(valore As String) As String

            Dim testo As String = If(valore, "")
            If testo.Length = 0 Then Return testo
            If SoloAscii(testo) Then Return testo

            Return "=?UTF-8?B?" & Convert.ToBase64String(Encoding.UTF8.GetBytes(testo)) & "?="

        End Function

        ''' <summary>
        ''' Come si dichiara un allegato. Il nome compare due volte di proposito: nella
        ''' forma semplice, che i programmi vecchi capiscono e che per gli accenti ripiega
        ''' sulla codifica dell'RFC 2047, e in quella dell'RFC 2231 (<c>filename*</c>), che
        ''' i programmi moderni preferiscono e che porta gli accenti veri.
        ''' </summary>
        Private Function DisposizioneDi(nome As String) As String

            Dim disposizione As New StringBuilder($"attachment; filename=""{PerIntestazione(nome)}""")

            If Not SoloAscii(nome) Then
                disposizione.Append("; filename*=UTF-8''").Append(Uri.EscapeDataString(nome))
            End If

            Return disposizione.ToString()

        End Function

        ''' <summary>Il nome di un file dentro un'intestazione: codificato se serve, e senza virgolette.</summary>
        Private Function PerIntestazione(nome As String) As String
            Return Codificata(If(nome, "").Replace("""", ""))
        End Function

        ''' <summary>
        ''' Che genere di file è l'allegato. Si guarda l'estensione e non il contenuto: qui
        ''' i file li produce l'applicazione stessa (cap. 05), e per tutto il resto c'è il
        ''' tipo generico, che nessun programma di posta rifiuta.
        ''' </summary>
        Private Function TipoDi(nome As String) As String

            Select Case Path.GetExtension(If(nome, "")).ToLowerInvariant()
                Case ".pdf" : Return "application/pdf"
                Case ".docx" : Return "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                Case ".doc" : Return "application/msword"
                Case ".txt" : Return "text/plain; charset=utf-8"
                Case ".md" : Return "text/markdown; charset=utf-8"
                Case ".png" : Return "image/png"
                Case ".jpg", ".jpeg" : Return "image/jpeg"
                Case Else : Return "application/octet-stream"
            End Select

        End Function

        ''' <summary>Il base64, spezzato nelle righe che il formato vuole.</summary>
        Private Function InBase64(dati As Byte()) As String

            Dim tutto As String = Convert.ToBase64String(dati)
            Dim righe As New StringBuilder()

            For inizio As Integer = 0 To tutto.Length - 1 Step LarghezzaBase64
                righe.Append(tutto.Substring(inizio, Math.Min(LarghezzaBase64, tutto.Length - inizio))).Append(ACapo)
            Next

            Return righe.ToString()

        End Function

        ''' <summary>
        ''' Il corpo con gli a capo del formato. Un testo scritto in una casella di Windows
        ''' porta CRLF, uno che arriva dall'AI di solito porta LF: si normalizza tutto una
        ''' volta sola, invece di sperare che a valle vada bene lo stesso.
        ''' </summary>
        Private Function NormalizzaACapo(testo As String) As String
            Return If(testo, "").Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Replace(vbLf, ACapo)
        End Function

        ''' <summary>
        ''' Un'intestazione sta su una riga: un a capo dentro l'oggetto spezzerebbe il
        ''' messaggio e tutto quel che segue verrebbe letto come corpo.
        ''' </summary>
        Private Function SenzaACapo(valore As String) As String
            Return If(valore, "").Replace(vbCrLf, " ").Replace(vbCr, " "c).Replace(vbLf, " "c).Trim()
        End Function

        Private Function SoloAscii(testo As String) As Boolean

            For Each carattere As Char In testo
                If AscW(carattere) > 127 Then Return False
            Next

            Return True

        End Function

    End Module

End Namespace
