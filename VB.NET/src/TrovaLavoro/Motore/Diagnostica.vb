Imports System.Text

Namespace Motore

    ''' <summary>
    ''' Il foglietto che si copia e si manda quando qualcosa non va: che programma è, da
    ''' quale codice nasce, dove tiene i dati, con quali modelli lavora, e le ultime righe
    ''' del diario tecnico.
    ''' </summary>
    ''' <remarks>
    ''' <para>Nasce il 2026-08-27, dalla revisione del giro D, insieme al diario tecnico
    ''' (<see cref="Dati.DiarioTecnico"/>). L'idea è vecchia quanto l'assistenza: chi ha un
    ''' guaio non sa quali cose servono per capirlo, e se glielo si chiede a voce si
    ''' perdono tre messaggi a testa. Un bottone che mette tutto negli appunti costa poco e
    ''' toglie quel giro.</para>
    ''' <para>Il testo passa <b>tutto</b> da <see cref="Dati.DiarioTecnico.SenzaSegreti"/>:
    ''' è fatto per uscire dalla macchina di chi lo copia, quindi vale la regola del diario,
    ''' non quella degli altri file della cartella dati.</para>
    ''' </remarks>
    Public NotInheritable Class Diagnostica

        ''' <summary>Quante righe di diario si portano dietro: abbastanza per il guasto e la sua rincorsa.</summary>
        Public Const RigheDiDiario As Integer = 40

        Private Sub New()
        End Sub

        ''' <summary>Compone il foglietto.</summary>
        ''' <param name="quando">L'istante in cui lo si copia.</param>
        ''' <param name="rigaDiVersione">La riga «Ver. … · Pool …», come la mostra l'interfaccia.</param>
        ''' <param name="rigaDelSorgente">La riga del commit, come la mostra «Informazioni».</param>
        ''' <param name="cartellaDati">Dove il programma tiene i dati di chi lo usa.</param>
        ''' <param name="modelli">I due modelli in vigore, già scritti come si leggono.</param>
        ''' <param name="ultimeRighe">Le ultime righe del diario tecnico, dalla più vecchia.</param>
        Public Shared Function Componi(quando As Date, rigaDiVersione As String, rigaDelSorgente As String,
                                       cartellaDati As String, modelli As String,
                                       ultimeRighe As IEnumerable(Of String)) As String

            Dim foglio As New StringBuilder()

            foglio.AppendLine("TrovaLavoro — diagnostica del " &
                              quando.ToString("yyyy-MM-dd HH:mm", Globalization.CultureInfo.InvariantCulture))
            foglio.AppendLine(Vera(rigaDiVersione, "Ver. —"))
            foglio.AppendLine(Vera(rigaDelSorgente, "Codice sorgente: non dichiarato"))
            foglio.AppendLine("Sistema: " & Environment.OSVersion.VersionString &
                              " · .NET " & Environment.Version.ToString())
            foglio.AppendLine("Cartella dati: " & Vera(cartellaDati, "(non ancora montata)"))
            foglio.AppendLine("Modelli: " & Vera(modelli, "(non ancora letti)"))
            foglio.AppendLine()

            Dim righe As String() = If(ultimeRighe, Array.Empty(Of String)()).
                Where(Function(v) Not String.IsNullOrWhiteSpace(v)).ToArray()

            If righe.Length = 0 Then
                foglio.AppendLine("Diario tecnico: nessuna riga (nessun guasto annotato).")
            Else
                foglio.AppendLine($"Diario tecnico, ultime {righe.Length} righe:")
                For Each riga As String In righe
                    foglio.AppendLine("  " & riga)
                Next
            End If

            Return Dati.DiarioTecnico.SenzaSegreti(foglio.ToString())

        End Function

        ''' <summary>Il valore, o il ripiego che dice esplicitamente che non c'è.</summary>
        Private Shared Function Vera(valore As String, ripiego As String) As String
            Return If(String.IsNullOrWhiteSpace(valore), ripiego, valore.Trim())
        End Function

    End Class

End Namespace
