Imports System.IO
Imports System.Text
Imports System.Text.Json
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Le preferenze dell'utente su disco: le legge da <c>impostazioni.json</c> e le
    ''' riscrive quando si chiude la finestra delle Impostazioni (cap. 03, pannello P8).
    ''' </summary>
    ''' <remarks>
    ''' <para>Come <see cref="ArchivioRicerche"/>, la scrittura passa da un temporaneo:
    ''' un'interruzione a metà lascia il file di prima, non uno troncato. E come quello,
    ''' in lettura <b>ripiega</b> invece di sollevare — senza le sue preferenze il
    ''' programma lavora lo stesso, con quelle di fabbrica.</para>
    ''' <para>Scrive <b>tutte</b> le preferenze, anche quelle rimaste al valore predefinito:
    ''' il file è fatto per essere aperto e letto, e uno che elencasse solo ciò che è stato
    ''' cambiato non direbbe all'utente quali altre manopole esistono.</para>
    ''' </remarks>
    Public Class ArchivioImpostazioni

        ''' <summary>UTF-8 senza BOM, come gli altri JSON dell'applicazione.</summary>
        Private Shared ReadOnly Codifica As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        ''' <summary>Rientri e accenti in chiaro: il file è fatto per essere letto a mano.</summary>
        Private Shared ReadOnly FormatoLeggibile As New JsonSerializerOptions With {
            .WriteIndented = True,
            .Encoder = Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping}

        Private ReadOnly _cartella As CartellaDati

        Public Sub New(cartella As CartellaDati)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            _cartella = cartella

        End Sub

        ''' <summary>Se un file delle impostazioni c'è già.</summary>
        Public ReadOnly Property Esiste As Boolean
            Get
                Return File.Exists(_cartella.FileImpostazioni)
            End Get
        End Property

        ''' <summary>
        ''' Le impostazioni in vigore: quelle del file, o i predefiniti con l'avviso del
        ''' ripiego. Non solleva mai.
        ''' </summary>
        Public Function Carica() As Impostazioni
            Return Impostazioni.Carica(_cartella.FileImpostazioni)
        End Function

        ''' <summary>Scrive le impostazioni, creando la cartella dati se è la prima volta.</summary>
        Public Sub Salva(impostazioni As Impostazioni)

            If impostazioni Is Nothing Then Throw New ArgumentNullException(NameOf(impostazioni))

            _cartella.Assicura()

            Dim temporaneo As String = _cartella.FileImpostazioni & ".tmp"

            File.WriteAllText(temporaneo, impostazioni.VersoJson().ToJsonString(FormatoLeggibile), Codifica)
            File.Move(temporaneo, _cartella.FileImpostazioni, overwrite:=True)

        End Sub

    End Class

End Namespace
