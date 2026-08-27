Imports System.Text.RegularExpressions

Namespace Ai

    ''' <summary>
    ''' Riconoscere due nomi dello stesso modello: l'alias e la versione datata
    ''' (cap. 02.5, cap. 13.11).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché esiste.</b> Il programma chiede i modelli per <b>alias</b> —
    ''' <c>claude-haiku-4-5</c>, <c>claude-sonnet-5</c> — perché è la forma che il cap. 15
    ''' (voce 6) dichiara e quella che non invecchia a ogni versione nuova. L'API però
    ''' risponde, e soprattutto <i>elenca</i>, con l'identificativo pieno:
    ''' <c>claude-haiku-4-5-20251001</c>. Sono lo stesso modello scritto in due modi, e
    ''' finché nessuno lo sapeva il programma si comportava come se fossero due.</para>
    ''' <para><b>Che cosa costava non saperlo.</b> Due guasti con una radice sola, trovati
    ''' guardando l'applicazione a occhio il 2026-08-27. Nelle Impostazioni la tendina
    ''' delle elaborazioni testuali mostrava l'identificativo crudo invece del nome, e
    ''' Haiku 4.5 compariva <b>due volte</b> nello stesso elenco senza che si potesse
    ''' capire che era lo stesso modello. E il contatore di spesa non prezzava le chiamate
    ''' al livello semplice: nel <c>chiamate_ai.csv</c> finisce il modello che <b>ha
    ''' risposto</b> (datato), mentre il listino conosce l'alias — così il buco previsto
    ''' dal cap. 13.11 per un modello sconosciuto si apriva sul modello <b>predefinito</b>,
    ''' a ogni installazione.</para>
    ''' <para><b>Solo la data.</b> Si toglie il suffisso <c>-AAAAMMGG</c> e nient'altro:
    ''' non si accorciano i nomi, non si confrontano prefissi. <c>claude-opus-4-5</c> e
    ''' <c>claude-opus-4-6</c> restano due modelli diversi, come devono.</para>
    ''' </remarks>
    Public NotInheritable Class IdModello

        ''' <summary>La data che l'API appiccica in coda: otto cifre, e solo in fondo.</summary>
        Private Shared ReadOnly LaData As New Regex("-\d{8}$", RegexOptions.Compiled)

        Private Sub New()
        End Sub

        ''' <summary>L'identificativo senza il suffisso della data, se ce l'ha.</summary>
        Public Shared Function SenzaLaData(id As String) As String

            Return LaData.Replace(If(id, String.Empty).Trim(), String.Empty)

        End Function

        ''' <summary>
        ''' Vero se i due identificativi nominano lo stesso modello, comunque siano
        ''' scritti. Due vuoti non sono lo stesso modello: sono due cose che non si sanno.
        ''' </summary>
        Public Shared Function StessoModello(uno As String, altro As String) As Boolean

            Dim primo As String = SenzaLaData(uno)
            Dim secondo As String = SenzaLaData(altro)

            If primo.Length = 0 OrElse secondo.Length = 0 Then Return False

            Return String.Equals(primo, secondo, StringComparison.OrdinalIgnoreCase)

        End Function

    End Class

End Namespace
