Imports System.IO
Imports System.Text
Imports System.Text.Json

Namespace Dati

    ''' <summary>
    ''' Le ricerche su disco: le legge da <c>ricerche.json</c> e le riscrive quando
    ''' l'utente ne mette una da parte (cap. 06.3, cap. 11.1).
    ''' </summary>
    ''' <remarks>
    ''' <para>Scrive <b>tutto il file</b>, portali compresi, e non solo la sezione delle
    ''' ricerche salvate: al primo salvataggio il file nasce completo, e l'utente si trova
    ''' sotto gli occhi la tabella dei portali che il cap. 06.3 gli promette modificabile
    ''' senza una nuova build. Un file che dichiarasse solo metà di ciò che il programma
    ''' usa sarebbe una mezza verità.</para>
    ''' <para>Come <see cref="ArchivioProfilo"/>, la scrittura passa da un temporaneo:
    ''' un'interruzione a metà lascia il file di prima, non uno troncato. La differenza è
    ''' cosa si rischia — qui delle ricerche da rifare, là il racconto di una vita — e
    ''' infatti in lettura questo archivio <b>ripiega</b> sui predefiniti invece di
    ''' sollevare (cap. 11.6): senza le sue ricerche si cerca lo stesso, e non c'è niente
    ''' da recuperare a mano.</para>
    ''' </remarks>
    Public Class ArchivioRicerche

        ''' <summary>UTF-8 senza BOM, come gli altri JSON dell'applicazione.</summary>
        Private Shared ReadOnly Codifica As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        ''' <summary>Rientri e accenti in chiaro: il file è fatto per essere letto e corretto a mano.</summary>
        Private Shared ReadOnly FormatoLeggibile As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        Private ReadOnly _cartella As CartellaDati

        Public Sub New(cartella As CartellaDati)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            _cartella = cartella

        End Sub

        ''' <summary>Se un file delle ricerche c'è già.</summary>
        Public ReadOnly Property Esiste As Boolean
            Get
                Return File.Exists(_cartella.FileRicerche)
            End Get
        End Property

        ''' <summary>
        ''' Le ricerche in vigore: quelle del file, o i portali predefiniti con l'avviso
        ''' del ripiego. Non solleva mai.
        ''' </summary>
        Public Function Carica() As Ricerche
            Return Ricerche.Carica(_cartella.FileRicerche)
        End Function

        ''' <summary>Scrive le ricerche, creando la cartella dati se è la prima volta.</summary>
        Public Sub Salva(ricerche As Ricerche)

            If ricerche Is Nothing Then Throw New ArgumentNullException(NameOf(ricerche))

            _cartella.Assicura()

            Dim temporaneo As String = _cartella.FileRicerche & ".tmp"

            File.WriteAllText(temporaneo, ricerche.ComeJson().ToJsonString(FormatoLeggibile), Codifica)
            File.Move(temporaneo, _cartella.FileRicerche, overwrite:=True)

        End Sub

    End Class

End Namespace
