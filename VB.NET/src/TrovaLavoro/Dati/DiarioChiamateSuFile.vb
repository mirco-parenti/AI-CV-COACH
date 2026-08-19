Imports System.Globalization
Imports System.IO
Imports System.Text
Imports TrovaLavoro.Ai

Namespace Dati

    ''' <summary>
    ''' Il diario delle chiamate all'AI scritto in fondo a un CSV nella cartella dati
    ''' (<see cref="CartellaDati.FileChiamateAi"/>).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Si aggiunge in fondo, non si riscrive.</b> Una riga per chiamata, e
    ''' l'intestazione solo la prima volta: così il file si apre in un foglio di calcolo, si
    ''' ordina per <c>percentuale_del_tetto</c> e in un colpo d'occhio si vede quale prompt
    ''' sta per non starci più.</para>
    ''' <para><b>Il punto vero è la percentuale.</b> I token consumati da soli non dicono
    ''' niente — 1400 sono tanti o pochi? — mentre il rapporto col tetto dichiarato dal
    ''' prompt sì: sopra il 90% quel prompt è già in bilico, e basterà una risposta un po'
    ''' più lunga della media per troncarsi. Il conto lo si fa qui una volta invece che a
    ''' mano ogni volta.</para>
    ''' <para><b>Non solleva mai.</b> È un'annotazione di servizio: se il disco è pieno o il
    ''' file è aperto altrove, si perde la riga e la chiamata all'AI prosegue. Il contrario —
    ''' una candidatura persa perché non si è potuto scrivere quanto è costata — sarebbe
    ''' assurdo (v. <see cref="IDiarioChiamate"/>).</para>
    ''' </remarks>
    Public Class DiarioChiamateSuFile
        Implements IDiarioChiamate

        ''' <summary>Lo stesso separatore dell'esportazione del registro.</summary>
        Private Const Separatore As String = ";"

        ''' <summary>
        ''' Il turno di parola per scrivere la riga. Serve da quando il server MCP
        ''' (cap. 09) chiama l'AI più volte insieme: due annotazioni nello stesso istante
        ''' si contenderebbero il file, una delle due prenderebbe un
        ''' <see cref="IOException"/> e verrebbe inghiottita dalla rete qui sotto — che è
        ''' giusta per un disco pieno e assurda per una gara con noi stessi. Perdere
        ''' proprio le righe delle chiamate simultanee falserebbe la misura nel punto in
        ''' cui serve di più, cioè quando il programma lavora al massimo.
        ''' </summary>
        ''' <remarks>
        ''' È condiviso e non d'istanza: costa un'attesa impercettibile — si scrive una
        ''' riga — e copre anche il caso di due contesti montati sulla stessa cartella
        ''' dati, dove due istanze diverse guardano lo stesso file.
        ''' </remarks>
        Private Shared ReadOnly Lucchetto As New Object

        Private Shared ReadOnly Intestazioni As String() = {
            "quando", "prompt", "modello", "tetto", "token_ingresso",
            "token_uscita", "percentuale_del_tetto", "motivo_fine"}

        Private ReadOnly _percorso As String

        ''' <param name="cartella">La cartella dati in cui vive il file.</param>
        Public Sub New(cartella As CartellaDati)

            If cartella Is Nothing Then Throw New ArgumentNullException(NameOf(cartella))

            _percorso = cartella.FileChiamateAi

        End Sub

        ''' <inheritdoc/>
        Public Sub Annota(idPrompt As String, tetto As Integer, uscita As RispostaAi) _
            Implements IDiarioChiamate.Annota

            If uscita Is Nothing Then Return

            Try
                Dim riga As String = String.Join(Separatore, Campi(idPrompt, tetto, uscita))

                SyncLock Lucchetto

                    ' L'intestazione la scrive chi trova il file ancora inesistente: così
                    ' non serve nessun passo di preparazione, e un file cancellato a mano
                    ' rinasce completo invece che monco. La domanda sta dentro il turno di
                    ' parola insieme alla scrittura: fuori, due chiamate simultanee
                    ' troverebbero il file assente tutte e due e scriverebbero
                    ' l'intestazione due volte.
                    Dim testa As String = If(File.Exists(_percorso),
                                             "",
                                             String.Join(Separatore, Intestazioni) & Environment.NewLine)

                    Directory.CreateDirectory(Path.GetDirectoryName(_percorso))
                    File.AppendAllText(_percorso, testa & riga & Environment.NewLine, Encoding.UTF8)

                End SyncLock

            Catch ex As Exception When TypeOf ex Is IOException OrElse
                                       TypeOf ex Is UnauthorizedAccessException
            End Try

        End Sub

        ''' <summary>I campi di una riga, nell'ordine delle intestazioni.</summary>
        Private Shared Function Campi(idPrompt As String, tetto As Integer,
                                      uscita As RispostaAi) As String()

            Return {
                Date.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                If(idPrompt, ""),
                If(uscita.Modello, ""),
                tetto.ToString(CultureInfo.InvariantCulture),
                uscita.TokenIngresso.ToString(CultureInfo.InvariantCulture),
                uscita.TokenUscita.ToString(CultureInfo.InvariantCulture),
                Percentuale(tetto, uscita.TokenUscita),
                If(uscita.MotivoFine, "")}

        End Function

        ''' <summary>
        ''' Quanto della risposta concessa è stato davvero usato. Vuota se il tetto non si
        ''' sa: una percentuale calcolata su zero direbbe un numero senza significato.
        ''' </summary>
        Private Shared Function Percentuale(tetto As Integer, tokenUscita As Integer) As String

            If tetto <= 0 Then Return ""

            Return (100.0 * tokenUscita / tetto).ToString("0.0", CultureInfo.InvariantCulture)

        End Function

    End Class

End Namespace
