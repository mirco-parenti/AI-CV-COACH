Imports System.IO
Imports System.Linq
Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Documenti

Namespace Documenti

    ''' <summary>
    ''' Collaudi del file <c>.eml</c> (cap. 07.2). Qui si collauda un <b>formato</b>, ed è
    ''' una fortuna: un messaggio di posta è testo, e ogni promessa che gli si fa —
    ''' «l'oggetto accentato arriva intero», «l'allegato è quel file lì» — si legge nel
    ''' file. L'unica cosa che questi collaudi non possono dire è se un programma di posta
    ''' vero lo apre come si deve: quello è il collaudo di tappa (cap. 14, T6).
    ''' </summary>
    <TestClass>
    Public Class CollaudiScrittoreEml

        <TestMethod>
        Public Sub IlMessaggioPortaLeIntestazioniCheServono()
            Dim eml As String = ScrittoreEml.Componi(
                "mirco@example.it", "lavoro@azienda.it", "Candidatura", "Buongiorno,", Nothing,
                New Date(2026, 8, 14, 18, 30, 0))

            Assert.Contains("From: mirco@example.it", eml)
            Assert.Contains("To: lavoro@azienda.it", eml)
            Assert.Contains("Subject: Candidatura", eml)
            Assert.Contains("MIME-Version: 1.0", eml)
            Assert.Contains("Date: Fri, 14 Aug 2026 18:30:00 +", eml, "la data in forma standard, col fuso")
        End Sub

        <TestMethod>
        Public Sub IlMessaggioSiDichiaraBozzaDaInviare()
            ' È la riga che fa la differenza fra una finestra di composizione pronta e un
            ' messaggio che sembra arrivato da qualcuno (cap. 07.2).
            Dim eml As String = ScrittoreEml.Componi("io@example.it", "loro@example.it", "Oggetto", "Corpo", Nothing)

            Assert.Contains("X-Unsent: 1", eml)
        End Sub

        <TestMethod>
        Public Sub UnIntestazioneSenzaValoreNonSiScriveAffatto()
            ' «To:» vuota è peggio che niente: certi client la leggono come un destinatario
            ' che si chiama «». Chi non ha ancora l'indirizzo lo scrive nel programma di posta.
            Dim eml As String = ScrittoreEml.Componi("io@example.it", "", "Oggetto", "Corpo", Nothing)

            Assert.DoesNotContain("To:", eml)
            Assert.Contains("From: io@example.it", eml)
        End Sub

        <TestMethod>
        Public Sub UnOggettoAccentatoViaggiaCodificato()
            ' Le intestazioni sono ASCII: «Candidatura per addetto qualità» scritto com'è
            ' arriverebbe a pezzi. La forma è quella dell'RFC 2047.
            Dim eml As String = ScrittoreEml.Componi(
                "io@example.it", "loro@example.it", "Candidatura per addetto qualità", "Corpo", Nothing)

            Assert.Contains("Subject: =?UTF-8?B?", eml, "l'oggetto è codificato")
            Assert.DoesNotContain("qualità", eml, "e il testo accentato non compare in chiaro")

            ' E si decodifica in quello che era: il collaudo non si accontenta della forma.
            Dim codificato As String = RigaChe(eml, "Subject: ").
                Replace("Subject: =?UTF-8?B?", "").Replace("?=", "")
            Assert.AreEqual("Candidatura per addetto qualità",
                            Encoding.UTF8.GetString(Convert.FromBase64String(codificato)))
        End Sub

        <TestMethod>
        Public Sub UnOggettoTuttoInglesRestaLeggibileNelFile()
            ' Se non serve codificare non si codifica: un oggetto leggibile anche aprendo il
            ' file con un editor è un piccolo regalo a chi ci mette il naso.
            Dim eml As String = ScrittoreEml.Componi(
                "io@example.it", "loro@example.it", "Candidatura per magazziniere", "Corpo", Nothing)

            Assert.Contains("Subject: Candidatura per magazziniere", eml)
        End Sub

        <TestMethod>
        Public Sub UnACapoNellOggettoNonSpezzaIlMessaggio()
            ' Un'intestazione sta su una riga: un a capo dentro l'oggetto farebbe leggere
            ' tutto il resto come corpo del messaggio — e l'AI un a capo lo può produrre.
            Dim eml As String = ScrittoreEml.Componi(
                "io@example.it", "loro@example.it", "Candidatura" & vbCrLf & "per magazziniere", "Corpo", Nothing)

            Assert.Contains("Subject: Candidatura per magazziniere", eml)
        End Sub

        <TestMethod>
        Public Sub SenzaAllegatiIlMessaggioNonHaPartiInutili()
            ' Un multipart con dentro una cosa sola è una scatola vuota, e certi client la
            ' mostrano come un allegato.
            Dim eml As String = ScrittoreEml.Componi("io@example.it", "loro@example.it", "Oggetto", "Buongiorno,", Nothing)

            Assert.Contains("Content-Type: text/plain; charset=utf-8", eml)
            Assert.DoesNotContain("multipart/mixed", eml)
        End Sub

        <TestMethod>
        Public Sub IlCorpoViaggiaInteroEAccentato()
            Dim corpo As String = "Buongiorno," & vbLf & vbLf & "mi candido per la posizione di addetto qualità." &
                                  vbLf & "Cordiali saluti," & vbLf & "Mirco"

            Dim eml As String = ScrittoreEml.Componi("io@example.it", "loro@example.it", "Oggetto", corpo, Nothing)

            ' Il corpo è in base64: si riapre e si confronta con quello che era, a capo
            ' compresi — che nel formato sono CRLF, non quelli con cui è stato scritto.
            Dim riletto As String = Encoding.UTF8.GetString(Convert.FromBase64String(DopoLeIntestazioni(eml)))

            Assert.Contains("addetto qualità", riletto, "gli accenti arrivano interi")
            Assert.Contains("Buongiorno," & vbCrLf, riletto, "e gli a capo diventano quelli del formato")
        End Sub

        <TestMethod>
        Public Sub ConUnAllegatoIlMessaggioDiventaAPiuParti()
            ConFileDiProva(
                Sub(percorso)
                    Dim eml As String = ScrittoreEml.Componi(
                        "io@example.it", "loro@example.it", "Oggetto", "Corpo",
                        {New AllegatoEmail(percorso, "CV_Mirco_Parenti.pdf")})

                    Assert.Contains("Content-Type: multipart/mixed; boundary=", eml)
                    Assert.Contains("Content-Type: application/pdf", eml, "il tipo viene dall'estensione")
                    Assert.Contains("Content-Disposition: attachment; filename=""CV_Mirco_Parenti.pdf""", eml)

                    ' Il confine deve chiudersi: senza le due lineette finali il messaggio
                    ' resta aperto e i client si comportano ognuno a modo suo.
                    Dim confine As String = RigaChe(eml, "Content-Type: multipart/mixed; boundary=").
                        Split(""""c)(1)
                    Assert.Contains("--" & confine & "--", eml, "la parte finale chiude il messaggio")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlContenutoDellAllegatoArrivaIntero()
            ' La promessa vera di un allegato: quel che parte è il file che c'era su disco,
            ' byte per byte.
            ConFileDiProva(
                Sub(percorso)
                    Dim atteso As Byte() = File.ReadAllBytes(percorso)

                    Dim eml As String = ScrittoreEml.Componi(
                        "io@example.it", "loro@example.it", "Oggetto", "Corpo",
                        {New AllegatoEmail(percorso, "prova.pdf")})

                    Assert.AreEqual(Convert.ToBase64String(atteso).Length,
                                    Base64DellAllegato(eml).Length, "tanti byte quanti erano")
                    CollectionAssert.AreEqual(atteso, Convert.FromBase64String(Base64DellAllegato(eml)),
                                              "e sono gli stessi byte")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnNomeDiAllegatoAccentatoViaggiaNelleDueForme()
            ' La forma semplice per i programmi vecchi (codificata, RFC 2047) e quella
            ' dell'RFC 2231 per i moderni: chi legge l'una ignora l'altra.
            ConFileDiProva(
                Sub(percorso)
                    Dim eml As String = ScrittoreEml.Componi(
                        "io@example.it", "loro@example.it", "Oggetto", "Corpo",
                        {New AllegatoEmail(percorso, "Attestato qualità.pdf")})

                    Assert.Contains("filename*=UTF-8''", eml, "la forma con gli accenti veri")
                    Assert.Contains("Attestato%20qualit%C3%A0.pdf", eml, "col nome codificato per l'indirizzo")
                    Assert.Contains("filename==?UTF-8?B?", eml.Replace("""", ""), "e la forma semplice, codificata")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LeRigheDelBase64NonSforanoIlLimiteDelFormato()
            ' 76 caratteri: oltre, certi server e certi client tagliano.
            ConFileDiProva(
                Sub(percorso)
                    Dim eml As String = ScrittoreEml.Componi(
                        "io@example.it", "loro@example.it", "Oggetto", "Corpo",
                        {New AllegatoEmail(percorso, "prova.pdf")})

                    For Each riga As String In eml.Split({vbCrLf}, StringSplitOptions.None)
                        Assert.IsGreaterThanOrEqualTo(riga.Length, 998,
                                                      "nessuna riga di un messaggio supera il limite del formato")
                    Next
                End Sub)
        End Sub

        <TestMethod>
        Public Sub ScrivereIlFileProduceQuelCheComponiDice()
            ' Le due strade devono dire la stessa cosa: i collaudi guardano il testo, e
            ' l'applicazione scrive il file.
            ConFileDiProva(
                Sub(percorso)
                    Dim destinazione As String = Path.Combine(Path.GetTempPath(),
                                                              "eml-" & Guid.NewGuid().ToString("N") & ".eml")
                    Try
                        Dim quando As New Date(2026, 8, 14, 18, 30, 0)

                        ScrittoreEml.Scrivi(destinazione, "io@example.it", "loro@example.it",
                                            "Oggetto", "Corpo", {New AllegatoEmail(percorso, "prova.pdf")}, quando)

                        Assert.IsTrue(File.Exists(destinazione), "il file c'è")

                        ' Le due composizioni hanno confini diversi (sono irripetibili per
                        ' costruzione): si confronta quel che non dipende da loro.
                        Dim scritto As String = File.ReadAllText(destinazione, Encoding.ASCII)
                        Assert.Contains("X-Unsent: 1", scritto)
                        Assert.Contains("Date: Fri, 14 Aug 2026 18:30:00 +", scritto)
                        Assert.Contains("filename=""prova.pdf""", scritto)
                    Finally
                        If File.Exists(destinazione) Then File.Delete(destinazione)
                    End Try
                End Sub)
        End Sub

        ''' <summary>Un file vero da allegare: un PDF finto, ma con dei byte suoi.</summary>
        Private Shared Sub ConFileDiProva(prova As Action(Of String))

            Dim percorso As String = Path.Combine(Path.GetTempPath(),
                                                  "allegato-" & Guid.NewGuid().ToString("N") & ".pdf")
            Try
                File.WriteAllBytes(percorso, Encoding.UTF8.GetBytes("%PDF-1.4 finto, con accenti: qualità" & vbLf))
                prova(percorso)
            Finally
                If File.Exists(percorso) Then File.Delete(percorso)
            End Try

        End Sub

        ''' <summary>La prima riga che comincia così.</summary>
        Private Shared Function RigaChe(eml As String, inizio As String) As String

            Return eml.Split({vbCrLf}, StringSplitOptions.None).
                       First(Function(riga) riga.StartsWith(inizio, StringComparison.Ordinal))

        End Function

        ''' <summary>Il base64 che segue le intestazioni di un messaggio senza parti.</summary>
        Private Shared Function DopoLeIntestazioni(eml As String) As String

            Dim righe As String() = eml.Split({vbCrLf}, StringSplitOptions.None)
            Dim vuota As Integer = Array.IndexOf(righe, "")

            Return String.Concat(righe.Skip(vuota + 1))

        End Function

        ''' <summary>Il base64 dell'allegato: l'ultimo blocco prima della chiusura.</summary>
        Private Shared Function Base64DellAllegato(eml As String) As String

            Dim righe As String() = eml.Split({vbCrLf}, StringSplitOptions.None)
            Dim dopoDisposizione As Integer = Array.FindIndex(
                righe, Function(riga) riga.StartsWith("Content-Disposition:", StringComparison.Ordinal))

            Dim raccolte As New StringBuilder()
            For indice As Integer = dopoDisposizione + 2 To righe.Length - 1
                If righe(indice).StartsWith("--", StringComparison.Ordinal) OrElse righe(indice).Length = 0 Then Exit For
                raccolte.Append(righe(indice))
            Next

            Return raccolte.ToString()

        End Function

        ''' <summary>
        ''' In un file di posta le intestazioni sono <b>righe</b>: un a capo dentro un
        ''' indirizzo non è un indirizzo storto, è una riga nuova — cioè un'intestazione
        ''' che nessuno ha chiesto. Chi scrive quei campi è l'utente, o l'AI che glieli
        ''' propone. <i>(Reperto M1 della revisione del giro D, 2026-08-27.)</i>
        ''' </summary>
        <TestMethod>
        Public Sub UnACapoNelDestinatarioNonApreUnaRigaNuova()

            Dim eml As String = ScrittoreEml.Componi(
                "io@example.it", "loro@example.it" & vbCrLf & "Bcc: nascosto@example.it",
                "Oggetto", "Corpo", Nothing)

            For Each riga As String In eml.Split(New String() {vbCrLf, vbCr, vbLf}, StringSplitOptions.None)
                Assert.IsFalse(riga.StartsWith("Bcc:", StringComparison.Ordinal),
                               "nessuna riga nuova: l'a capo è diventato uno spazio")
            Next

            Assert.Contains("To: loro@example.it Bcc: nascosto@example.it", eml,
                            "e il testo non si perde per strada: resta dentro il valore")

        End Sub

        <TestMethod>
        Public Sub UnACapoNelMittenteNonApreUnaRigaNuova()

            ' Le porte sono due, e ne bastava una aperta.
            Dim eml As String = ScrittoreEml.Componi(
                "io@example.it" & vbLf & "Reply-To: altro@example.it", "loro@example.it",
                "Oggetto", "Corpo", Nothing)

            For Each riga As String In eml.Split(New String() {vbCrLf, vbCr, vbLf}, StringSplitOptions.None)
                Assert.IsFalse(riga.StartsWith("Reply-To:", StringComparison.Ordinal),
                               "vale per il mittente come per il destinatario")
            Next

        End Sub

    End Class

End Namespace
