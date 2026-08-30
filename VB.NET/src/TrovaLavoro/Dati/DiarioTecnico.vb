Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Dati

    ''' <summary>
    ''' Il diario tecnico dell'applicazione: <c>log\app.log</c> nella cartella dati.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Perché esiste.</b> Il cap. 11.1 lo disegna nell'albero della cartella dati
    ''' dal primo giorno — «log tecnico (senza segreti)» — e per tutta la 1.0 non l'ha
    ''' scritto nessuno: nessuna riga del prodotto apriva quel file. L'ha trovato la
    ''' revisione del giro D, il 2026-08-27, ed è la specie di impegno che la regola 16 non
    ''' poteva stanare, perché non era stato promesso da una tappa ma da un capitolo.</para>
    ''' <para><b>Tre promesse.</b> (1) Non solleva <i>mai</i>: un diario che fa cadere il
    ''' programma è peggio di nessun diario, e chi lo chiama non ha niente da gestire.
    ''' (2) Non contiene segreti: ogni riga passa da <see cref="SenzaSegreti"/> prima di
    ''' toccare il disco. (3) Non cresce senza fine: due file, e il più vecchio si perde.</para>
    ''' <para>Non è il <c>chiamate_ai.csv</c>, che conta i consumi (cap. 02.5): quello è un
    ''' registro di quel che è andato bene, questo è la memoria di quel che è andato male.</para>
    ''' </remarks>
    Public NotInheritable Class DiarioTecnico

        ''' <summary>Quanto può diventare grosso un diario prima di lasciare il posto (mezzo mega).</summary>
        Public Const TettoInByte As Long = 512L * 1024L

        ''' <summary>Il formato dell'istante: lo stesso delle date su disco, col fuso.</summary>
        Private Const FormatoIstante As String = "yyyy-MM-dd HH:mm:sszzz"

        ''' <summary>
        ''' Le chiavi API, in tutte le forme in cui potrebbero passare di qui: il valore
        ''' nudo, e le due intestazioni HTTP che lo portano.
        ''' </summary>
        Private Shared ReadOnly ChiaveNuda As New Regex("sk-[A-Za-z0-9\-_]{6,}", RegexOptions.Compiled)

        Private Shared ReadOnly IntestazioneCheLaPorta As New Regex(
            "(?i)\b(x-api-key|authorization)(\s*[:=]\s*)(\S+)", RegexOptions.Compiled)

        Private ReadOnly _cartella As CartellaDati
        Private ReadOnly _turno As New Object()

        Public Sub New(cartella As CartellaDati)
            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))
            _cartella = cartella
        End Sub

        ''' <summary>
        ''' Il diario dell'applicazione in corso, se ne è stato montato uno. Esiste per
        ''' l'ultima rete delle eccezioni (v. <c>Programma</c>), che scatta in posti dove
        ''' non arriva nessun contesto — ed è l'unico punto del programma che lo usa così.
        ''' </summary>
        Public Shared Property Corrente As DiarioTecnico

        ''' <summary>Scrive una riga nel diario. Non solleva mai.</summary>
        Public Sub Annota(messaggio As String)

            If String.IsNullOrWhiteSpace(messaggio) Then Return

            Try
                SyncLock _turno
                    Directory.CreateDirectory(_cartella.CartellaLog)
                    FaiPostoSeServe()
                    File.AppendAllText(_cartella.FileLog,
                                       Riga(Date.Now, messaggio) & Environment.NewLine,
                                       New UTF8Encoding(encoderShouldEmitUTF8Identifier:=False))
                End SyncLock
            Catch ex As Exception
                ' Un diario che non si lascia scrivere resta muto: è un peccato, non un guasto.
            End Try

        End Sub

        ''' <summary>Scrive un guasto: dove è successo, che cos'era, che cosa diceva.</summary>
        Public Sub AnnotaGuasto(dove As String, eccezione As Exception)
            Annota($"GUASTO {NelPosto(dove)} — {UltimaRete.MessaggioPerIlDiario(eccezione)}")
        End Sub

        ''' <summary>
        ''' Gli articoli che «in» assorbe. L'ordine conta: quelli che portano lo spazio
        ''' vanno provati prima di <c>l'</c>, che lo spazio non ce l'ha.
        ''' </summary>
        Private Shared ReadOnly Fusioni As (Articolo As String, Fuso As String)() = {
            ("gli ", "negli "), ("lo ", "nello "), ("la ", "nella "), ("le ", "nelle "),
            ("il ", "nel "), ("i ", "nei "), ("l'", "nell'"), ("l’", "nell’")
        }

        ''' <summary>
        ''' «in» e l'articolo del posto, fusi come vuole l'italiano: <c>l'ultima rete</c>
        ''' diventa <c>nell'ultima rete</c>, <c>il ciclo del server MCP</c> diventa
        ''' <c>nel ciclo del server MCP</c>, e quel che non comincia per articolo resta
        ''' <c>in …</c>.
        ''' </summary>
        ''' <remarks>
        ''' <para>Sta qui e non nei chiamanti perché chi chiama sa <b>dove</b> è successo —
        ''' «la prova della chiave API» — e non deve sapere in che frase quel posto andrà a
        ''' finire. Fino al 2026-08-30 la riga si componeva come <c>GUASTO in {dove}</c> e
        ''' usciva «GUASTO in l'ultima rete»: i sette posti che il prodotto nomina
        ''' cominciano <i>tutti</i> per articolo, quindi sbagliavano tutti e sette. Non era
        ''' un caso di confine: era ogni riga di guasto che il diario abbia mai scritto.</para>
        ''' <para>È vissuto a lungo perché il diario non si vede — non è interfaccia, è il
        ''' file che esce col foglietto di diagnostica, cioè il solo pezzo di questa
        ''' cartella fatto per essere <b>letto da altri</b>. Ed è proprio per questo che va
        ''' scritto in italiano.</para>
        ''' </remarks>
        Private Shared Function NelPosto(dove As String) As String

            Dim posto As String = If(dove, String.Empty).Trim()
            If posto.Length = 0 Then Return "in un posto senza nome"

            For Each fusione In Fusioni
                If posto.StartsWith(fusione.Articolo, StringComparison.OrdinalIgnoreCase) Then
                    Return fusione.Fuso & posto.Substring(fusione.Articolo.Length)
                End If
            Next

            Return "in " & posto

        End Function

        ''' <summary>
        ''' Le ultime righe del diario, dalla più vecchia alla più recente. Vuoto se il
        ''' diario non c'è o non si lascia leggere.
        ''' </summary>
        Public Function UltimeRighe(quante As Integer) As String()

            If quante <= 0 Then Return Array.Empty(Of String)()

            Try
                SyncLock _turno
                    If Not File.Exists(_cartella.FileLog) Then Return Array.Empty(Of String)()
                    Dim tutte As String() = File.ReadAllLines(_cartella.FileLog)
                    If tutte.Length <= quante Then Return tutte
                    Dim ultime(quante - 1) As String
                    Array.Copy(tutte, tutte.Length - quante, ultime, 0, quante)
                    Return ultime
                End SyncLock
            Catch ex As Exception
                Return Array.Empty(Of String)()
            End Try

        End Function

        ''' <summary>Una riga del diario: l'istante col fuso, due spazi, il messaggio ripulito.</summary>
        Public Shared Function Riga(istante As Date, messaggio As String) As String

            Dim quando As Date = If(istante.Kind = DateTimeKind.Utc, istante.ToLocalTime(),
                                    Date.SpecifyKind(istante, DateTimeKind.Local))

            Return New DateTimeOffset(quando).ToString(FormatoIstante, Globalization.CultureInfo.InvariantCulture) &
                   "  " & SenzaSegreti(SuUnaRigaSola(messaggio))

        End Function

        ''' <summary>
        ''' Lo stesso testo, con le chiavi API mascherate come le mostra l'interfaccia
        ''' (cap. 11.3): le ultime quattro cifre e nient'altro.
        ''' </summary>
        ''' <remarks>
        ''' Il diario nasce per essere <b>mandato a qualcuno</b> quando qualcosa non va: è
        ''' l'unico file della cartella dati che ha una ragione di uscire di lì. Perciò la
        ''' ripulitura non è una precauzione fra le altre, è la condizione perché il file
        ''' possa esistere.
        ''' </remarks>
        Public Shared Function SenzaSegreti(testo As String) As String

            If String.IsNullOrEmpty(testo) Then Return If(testo, "")

            Dim ripulito As String = IntestazioneCheLaPorta.Replace(testo, Function(t) t.Groups(1).Value & t.Groups(2).Value & "«tolta»")
            Return ChiaveNuda.Replace(ripulito, Function(t) Mascherata(t.Value))

        End Function

        ''' <summary>Una chiave come si scrive quando si può scriverla: «sk-…» e le ultime quattro.</summary>
        Private Shared Function Mascherata(chiave As String) As String
            Return "sk-…" & chiave.Substring(chiave.Length - 4)
        End Function

        ''' <summary>Un messaggio a più righe ne occupa una sola: un diario si legge a righe.</summary>
        Private Shared Function SuUnaRigaSola(messaggio As String) As String
            Return messaggio.Replace(vbCrLf, " ").Replace(vbCr, " "c).Replace(vbLf, " "c).Trim()
        End Function

        ''' <summary>
        ''' Se il diario ha passato il tetto, diventa quello «di prima» e se ne apre uno
        ''' nuovo. Il diario di prima ancora più vecchio si perde: due bastano.
        ''' </summary>
        Private Sub FaiPostoSeServe()

            If Not File.Exists(_cartella.FileLog) Then Return
            If New FileInfo(_cartella.FileLog).Length < TettoInByte Then Return

            File.Move(_cartella.FileLog, _cartella.FileLogPrecedente, overwrite:=True)

        End Sub

    End Class

End Namespace
