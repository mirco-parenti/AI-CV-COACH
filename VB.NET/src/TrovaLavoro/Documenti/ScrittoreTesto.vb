Imports System.Text

Namespace Documenti

    ''' <summary>
    ''' La terza stampante: la stessa pagina, scritta in testo semplice per l'anteprima di
    ''' P6 (cap. 03.6). Nessun file — quello che produce si legge a video.
    ''' </summary>
    ''' <remarks>
    ''' <para>Passa dagli stessi <see cref="Blocco"/> del DOCX e del PDF, e non dal JSON:
    ''' è la regola del cap. 05.3 applicata fino in fondo. Se l'anteprima leggesse il JSON
    ''' per conto suo, il giorno in cui l'impaginazione cambia mostrerebbe un documento
    ''' che i file non contengono — cioè mentirebbe proprio dove l'utente controlla.</para>
    ''' <para>Non imita la grafica: i titoli di sezione si staccano con una riga vuota, gli
    ''' elenchi hanno il loro punto. Quel che conta qui è il <b>contenuto</b>, ed è ciò che
    ''' l'utente deve poter confrontare con l'annuncio che ha accanto.</para>
    ''' </remarks>
    Public Class ScrittoreTesto

        ''' <summary>Il segno che apre una voce di elenco.</summary>
        Private Const Punto As String = "• "

        ''' <summary>La pagina come testo leggibile.</summary>
        Public Shared Function Componi(pagina As PaginaDocumento) As String

            If pagina Is Nothing Then Return String.Empty

            Dim scritto As New StringBuilder()

            For Each blocco As Blocco In pagina.Blocchi

                Select Case blocco.Genere

                    Case GenereBlocco.Nome
                        Riga(scritto, blocco.Testo)

                    Case GenereBlocco.Recapiti
                        Riga(scritto, String.Join(Impaginazione.Separatore, blocco.Voci))

                    Case GenereBlocco.Sezione
                        ' Le sezioni si staccano da ciò che le precede: è l'unico
                        ' «disegno» che un'anteprima di testo si può permettere.
                        If scritto.Length > 0 Then scritto.AppendLine()
                        Riga(scritto, blocco.Testo)

                    Case GenereBlocco.Paragrafo
                        Riga(scritto, blocco.Testo)

                    Case GenereBlocco.Voce
                        Riga(scritto, blocco.Testo)
                        Riga(scritto, blocco.Dettaglio)
                        Riga(scritto, blocco.Descrizione)

                    Case GenereBlocco.Elenco
                        For Each voce As String In blocco.Voci
                            Riga(scritto, Punto & voce)
                        Next

                    Case GenereBlocco.Firma
                        scritto.AppendLine()
                        Riga(scritto, blocco.Testo)

                End Select

            Next

            Return scritto.ToString().TrimEnd()

        End Function

        ''' <summary>Scrive una riga, saltando i campi che quel blocco non usa.</summary>
        Private Shared Sub Riga(scritto As StringBuilder, testo As String)

            If String.IsNullOrEmpty(testo) Then Return
            scritto.AppendLine(testo)

        End Sub

    End Class

End Namespace
