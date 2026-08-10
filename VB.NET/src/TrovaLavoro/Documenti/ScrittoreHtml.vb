Imports System.IO
Imports System.Net
Imports System.Reflection
Imports System.Text

Namespace Documenti

    ''' <summary>
    ''' Il gemello di <see cref="ScrittoreDocx"/> per l'altro formato: da una
    ''' <see cref="PaginaDocumento"/> a una pagina HTML autonoma, che è ciò che
    ''' <see cref="StampantePdf"/> dà in pasto alla WebView per ricavarne il PDF
    ''' (cap. 05.5).
    ''' </summary>
    ''' <remarks>
    ''' <para>Scrive una pagina <b>che sta tutta in sé stessa</b> — stile compreso —
    ''' perché non ha una cartella dove vivere: nasce in memoria, viene mostrata e muore
    ''' appena il PDF è scritto. Nessun riferimento a file esterni, nessuna immagine,
    ''' nessuno script: non c'è niente che possa mancare al momento della stampa.</para>
    ''' <para>Le due stampanti non si consultano fra loro (cap. 05.3): partono dalla
    ''' stessa pagina e ognuna la disegna nella sua lingua. Il foglio di stile qui
    ''' accanto ricalca <c>modello-docx/styles.xml</c> misura per misura, ed è lì che si
    ''' va a guardare se un giorno i due documenti non si somigliassero più.</para>
    ''' </remarks>
    Public Class ScrittoreHtml

        ''' <summary>Le classi CSS dichiarate in <c>modello-html/stile.css</c>.</summary>
        Public Const ClasseNome As String = "nome"
        Public Const ClasseRecapiti As String = "recapiti"
        Public Const ClasseSezione As String = "sezione"
        Public Const ClasseVoceTitolo As String = "voce-titolo"
        Public Const ClasseVoceDettaglio As String = "voce-dettaglio"
        Public Const ClasseTesto As String = "testo"
        Public Const ClasseFirma As String = "firma"

        ''' <summary>Il prefisso con cui il modello entra nell'eseguibile.</summary>
        Private Const PrefissoRisorsa As String = "modello-html/"

        Private Const SegnapostoTitolo As String = "{{TITOLO}}"
        Private Const SegnapostoStile As String = "{{STILE}}"
        Private Const SegnapostoCorpo As String = "{{CORPO}}"

        ''' <summary>
        ''' La pagina HTML completa, pronta da mostrare a una WebView. Si chiama
        ''' <c>Componi</c> come la gemella di <see cref="ScrittoreDocx"/>: stesso lavoro,
        ''' altro formato.
        ''' </summary>
        ''' <param name="pagina">La pagina da stampare.</param>
        Public Shared Function Componi(pagina As PaginaDocumento) As String

            If pagina Is Nothing Then Throw New ArgumentNullException(NameOf(pagina))

            ' Lo stile per primo e il corpo per ultimo: così un testo dell'utente che per
            ' caso contenesse un segnaposto resta testo, invece di diventare un buco.
            Return Risorsa("pagina.html").
                Replace(SegnapostoStile, Risorsa("stile.css")).
                Replace(SegnapostoTitolo, WebUtility.HtmlEncode(pagina.Titolo)).
                Replace(SegnapostoCorpo, Corpo(pagina))

        End Function

        ''' <summary>
        ''' Il corpo della pagina: ogni blocco diventa un elemento con la sua classe. È
        ''' la stessa scelta che <see cref="ScrittoreDocx"/> fa con gli stili, detta in
        ''' HTML.
        ''' </summary>
        Private Shared Function Corpo(pagina As PaginaDocumento) As String

            Dim scritto As New StringBuilder()

            For Each blocco As Blocco In pagina.Blocchi

                Select Case blocco.Genere

                    Case GenereBlocco.Nome
                        Riga(scritto, ClasseNome, blocco.Testo)

                    Case GenereBlocco.Recapiti
                        Riga(scritto, ClasseRecapiti,
                             String.Join(Impaginazione.Separatore, blocco.Voci))

                    Case GenereBlocco.Sezione
                        Riga(scritto, ClasseSezione, blocco.Testo)

                    Case GenereBlocco.Paragrafo
                        Riga(scritto, ClasseTesto, blocco.Testo)

                    Case GenereBlocco.Voce
                        If blocco.Testo.Length > 0 Then Riga(scritto, ClasseVoceTitolo, blocco.Testo)
                        If blocco.Dettaglio.Length > 0 Then Riga(scritto, ClasseVoceDettaglio, blocco.Dettaglio)
                        If blocco.Descrizione.Length > 0 Then Riga(scritto, ClasseTesto, blocco.Descrizione)

                    Case GenereBlocco.Elenco
                        scritto.AppendLine("<ul>")
                        For Each voce As String In blocco.Voci
                            scritto.AppendLine($"<li>{WebUtility.HtmlEncode(voce)}</li>")
                        Next
                        scritto.AppendLine("</ul>")

                    Case GenereBlocco.Firma
                        Riga(scritto, ClasseFirma, blocco.Testo)

                End Select

            Next

            Return scritto.ToString().TrimEnd()

        End Function

        ''' <summary>Un paragrafo con la sua classe, col testo messo al sicuro.</summary>
        Private Shared Sub Riga(scritto As StringBuilder, classe As String, testo As String)
            scritto.AppendLine($"<p class=""{classe}"">{WebUtility.HtmlEncode(testo)}</p>")
        End Sub

        ''' <summary>Una parte del modello, dall'eseguibile.</summary>
        Private Shared Function Risorsa(nome As String) As String

            Dim assembly As Assembly = GetType(ScrittoreHtml).Assembly

            Using flusso As Stream = assembly.GetManifestResourceStream(PrefissoRisorsa & nome)

                If flusso Is Nothing Then
                    Throw New InvalidOperationException(
                        $"Il modello della pagina è incompleto: manca «{nome}».")
                End If

                Using lettore As New StreamReader(flusso, Encoding.UTF8)
                    Return lettore.ReadToEnd()
                End Using

            End Using

        End Function

    End Class

End Namespace
