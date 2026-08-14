Imports System.IO
Imports System.Security.Cryptography
Imports System.Text

Namespace Dati

    ''' <summary>
    ''' La chiave API su disco, cifrata con la protezione dati di Windows (cap. 11.3):
    ''' <c>segreti.bin</c> nella cartella dati, legato all'<b>utente</b> che l'ha
    ''' salvata. Copiato su un altro PC o letto da un altro account non si decifra.
    ''' </summary>
    ''' <remarks>
    ''' <para>Niente crittografia inventata in proprio: la cifratura è quella di Windows
    ''' (<see cref="ProtectedData"/>, ambito <see cref="DataProtectionScope.CurrentUser"/>),
    ''' che è esattamente il compromesso che il cap. 11.3 chiede per un'applicazione
    ''' personale. La chiave con cui si cifra non la vede nemmeno questo codice: la
    ''' custodisce Windows, per quell'account.</para>
    ''' <para><b>In lettura non solleva mai.</b> File assente, troncato, o scritto da un
    ''' altro utente: sono tutti «non ce l'ho», perché nessuno di quei casi impedisce di
    ''' aprire l'applicazione — impedisce solo di chiamare l'AI, e quello lo si dice
    ''' (v. <see cref="ContestoApp"/>). Il caso «c'è ma non si decifra» torna a parte,
    ''' perché merita un avviso diverso da «non c'è»: è un file che l'utente vede su
    ''' disco e che potrebbe credere buono.</para>
    ''' <para>Dentro ci sta la <b>sola chiave</b>, in UTF-8, cifrata: nessuna struttura,
    ''' nessun JSON. Il nome è al plurale perché lo dice il cap. 11.1, ma la chiave API è
    ''' rimasta l'unica credenziale del programma — senza invio SMTP non c'è più nessuna
    ''' password di posta da custodire (cap. 07.2). Se un domani ne arriverà un'altra,
    ''' quel giorno si cifrerà un JSON: inventargli una struttura adesso vorrebbe dire
    ''' tenerla allineata per un solo campo.</para>
    ''' </remarks>
    Public Class ArchivioSegreti

        ''' <summary>
        ''' L'entropia aggiuntiva passata a DPAPI. <b>Non è un segreto</b> — sta scritta
        ''' nell'eseguibile, come starebbe in qualunque programma — e non aggiunge
        ''' robustezza: serve a far fallire subito la decifratura di un blob che non è
        ''' nostro, invece di restituire byte che sembrano una chiave e non lo sono.
        ''' </summary>
        Private Shared ReadOnly Firma As Byte() = Encoding.UTF8.GetBytes("TrovaLavoro/segreti/1")

        ''' <summary>Il testo cifrato è UTF-8, come tutto il resto dell'applicazione.</summary>
        Private Shared ReadOnly Codifica As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        Private ReadOnly _cartella As CartellaDati

        Public Sub New(cartella As CartellaDati)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            _cartella = cartella

        End Sub

        ''' <summary>Se un file dei segreti c'è già; non dice se si lascia decifrare.</summary>
        Public ReadOnly Property Esiste As Boolean
            Get
                Return File.Exists(_cartella.FileSegreti)
            End Get
        End Property

        ''' <summary>
        ''' La chiave API salvata, o <c>Nothing</c> se non ce n'è una utilizzabile.
        ''' </summary>
        ''' <param name="illeggibile">
        ''' Vero quando il file c'è ma non si è lasciato decifrare: è la differenza fra
        ''' «non l'ho mai salvata» e «l'ho salvata su un altro PC o con un altro account»,
        ''' e chi chiama la deve poter dire.
        ''' </param>
        Public Function LeggiChiaveApi(ByRef illeggibile As Boolean) As String

            illeggibile = False
            If Not Esiste Then Return Nothing

            Try
                Dim cifrato As Byte() = File.ReadAllBytes(_cartella.FileSegreti)
                Dim chiaro As Byte() = ProtectedData.Unprotect(cifrato, Firma, DataProtectionScope.CurrentUser)
                Dim chiave As String = Codifica.GetString(chiaro).Trim()

                ' Un file decifrato ma vuoto è illeggibile quanto uno cifrato male: non
                ' c'è nessuna chiave da usare, e non è per una svista di chi ha scritto.
                If chiave.Length = 0 Then
                    illeggibile = True
                    Return Nothing
                End If

                Return chiave

            Catch ex As Exception When TypeOf ex Is CryptographicException OrElse
                                       TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
                illeggibile = True
                Return Nothing
            End Try

        End Function

        ''' <summary>
        ''' Scrive la chiave cifrata, creando la cartella dati se è la prima volta. Come
        ''' gli altri archivi passa da un temporaneo: un'interruzione a metà lascia il
        ''' file di prima, non uno troncato che al prossimo avvio sembrerebbe corrotto.
        ''' </summary>
        Public Sub SalvaChiaveApi(chiave As String)

            If String.IsNullOrWhiteSpace(chiave) Then
                Throw New ArgumentException("La chiave API non può essere vuota.", NameOf(chiave))
            End If

            _cartella.Assicura()

            Dim cifrato As Byte() = ProtectedData.Protect(
                Codifica.GetBytes(chiave.Trim()), Firma, DataProtectionScope.CurrentUser)

            Dim temporaneo As String = _cartella.FileSegreti & ".tmp"

            File.WriteAllBytes(temporaneo, cifrato)
            File.Move(temporaneo, _cartella.FileSegreti, overwrite:=True)

        End Sub

        ''' <summary>
        ''' Toglie la chiave salvata. Non protesta se non c'era: chi cancella vuole che
        ''' dopo non ci sia, e quello è già vero.
        ''' </summary>
        Public Sub Cancella()

            If Not Esiste Then Return
            File.Delete(_cartella.FileSegreti)

        End Sub

        ''' <summary>
        ''' La chiave come si mostra a video e nelle note: <c>sk-ant-…1234</c>
        ''' (cap. 11.3). Non è un abbellimento — è la regola per cui una chiave non
        ''' compare mai per intero fuori dal file cifrato, nemmeno nella diagnostica.
        ''' </summary>
        Public Shared Function Maschera(chiave As String) As String

            If String.IsNullOrWhiteSpace(chiave) Then Return "—"

            Dim netta As String = chiave.Trim()

            ' Sotto una certa lunghezza le ultime quattro cifre sarebbero mezza chiave:
            ' di una stringa così corta non si mostra niente.
            If netta.Length <= 12 Then Return New String("•"c, netta.Length)

            Return netta.Substring(0, 7) & "…" & netta.Substring(netta.Length - 4)

        End Function

    End Class

End Namespace
