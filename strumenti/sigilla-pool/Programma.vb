Imports System.IO
Imports TrovaLavoro.Ai

''' <summary>
''' Il rito del bump da riga di comando (cap. 04.5): rigenera le impronte del pool e
''' riscrive <c>pool_manifest.json</c> con la versione nuova.
''' </summary>
''' <remarks>
''' Non calcola niente da sé: chiama <see cref="LibreriaPrompt.Sigilla"/>, lo stesso
''' codice con cui il caricatore verifica le impronte. Un attrezzo che le ricalcolasse
''' a modo suo sarebbe la premessa del giorno in cui il pool risulta modificato senza
''' esserlo.
''' </remarks>
Friend Module Programma

    Friend Function Main(argomenti As String()) As Integer

        ' La console di Windows scriverebbe con la tabella caratteri di sistema, e i
        ' messaggi di qui sono in italiano con le virgolette basse: senza questo, «finché»
        ' arriva storto a chi legge — la stessa trappola già pagata negli script del banco.
        Console.OutputEncoding = Text.Encoding.UTF8

        If argomenti Is Nothing OrElse argomenti.Length < 2 Then
            Console.WriteLine("Uso: SigillaPool <cartella-del-pool> <versione> [data]")
            Console.WriteLine("Esempio: SigillaPool ..\..\VB.NET\src\TrovaLavoro\prompt-pool 1.04")
            Return 1
        End If

        Dim cartella As String = argomenti(0)
        Dim versione As String = argomenti(1)
        Dim data As String = If(argomenti.Length > 2, argomenti(2), Nothing)

        Try
            LibreriaPrompt.Sigilla(cartella, versione, data)

            ' La prova che il sigillo è valido è riaprire il pool e sentirselo dire da
            ' lui: un manifest scritto e mai riletto è una promessa non verificata.
            Dim riletto As LibreriaPrompt = LibreriaPrompt.Apri(cartella)

            Console.WriteLine($"Pool sigillato: {riletto.Etichetta}")
            If riletto.Avviso IsNot Nothing Then
                Console.WriteLine($"Attenzione: {riletto.Avviso}")
                Return 2
            End If

            Console.WriteLine($"Manifest riscritto in «{Path.GetFullPath(Path.Combine(cartella, "pool_manifest.json"))}».")
            Console.WriteLine("Ora resta da annotare il CHANGELOG.md: il rito non è finito finché non è raccontato.")
            Return 0

        Catch ex As Exception
            Console.WriteLine($"Non sono riuscita a sigillare il pool: {ex.Message}")
            Return 1
        End Try

    End Function

End Module
