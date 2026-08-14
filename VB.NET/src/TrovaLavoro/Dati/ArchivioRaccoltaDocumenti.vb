Imports System.IO
Imports System.Text

Namespace Dati

    ''' <summary>
    ''' La raccolta dei documenti su disco: la legge da <c>documenti.json</c> e la
    ''' riscrive quando l'utente sceglie una cartella o corregge una categoria
    ''' (cap. 05.2, cap. 11.1).
    ''' </summary>
    ''' <remarks>
    ''' Come <see cref="ArchivioRicerche"/> la scrittura passa da un temporaneo, e la
    ''' lettura <b>ripiega</b> invece di sollevare: quel che si perde è una cartella da
    ''' riscegliere, non il racconto di una vita.
    ''' </remarks>
    Public Class ArchivioRaccoltaDocumenti

        ''' <summary>UTF-8 senza BOM, come gli altri JSON dell'applicazione.</summary>
        Private Shared ReadOnly Codifica As New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False)

        Private ReadOnly _cartella As CartellaDati

        Public Sub New(cartella As CartellaDati)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            _cartella = cartella

        End Sub

        ''' <summary>Se una raccolta è già stata scritta.</summary>
        Public ReadOnly Property Esiste As Boolean
            Get
                Return File.Exists(_cartella.FileDocumenti)
            End Get
        End Property

        ''' <summary>La raccolta in vigore, o una vuota. Non solleva mai.</summary>
        Public Function Carica() As RaccoltaDocumenti
            Return RaccoltaDocumenti.Carica(_cartella.FileDocumenti)
        End Function

        ''' <summary>Scrive la raccolta, creando la cartella dati se è la prima volta.</summary>
        Public Sub Salva(raccolta As RaccoltaDocumenti)

            If raccolta Is Nothing Then Throw New ArgumentNullException(NameOf(raccolta))

            _cartella.Assicura()

            Dim temporaneo As String = _cartella.FileDocumenti & ".tmp"

            File.WriteAllText(temporaneo, raccolta.ComeTesto(), Codifica)
            File.Move(temporaneo, _cartella.FileDocumenti, overwrite:=True)

        End Sub

    End Class

End Namespace
