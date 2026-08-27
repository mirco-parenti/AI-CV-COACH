Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Dati

    ''' <summary>
    ''' Collaudi del diario tecnico (cap. 11.1) e del foglietto di diagnostica che se lo
    ''' porta dietro. Il cap. 11 disegnava <c>log\app.log</c> «senza segreti» dal primo
    ''' giorno e per tutta la 1.0 non l'ha scritto nessuno: qui si guarda che adesso esista
    ''' — e soprattutto che quel «senza segreti» sia vero, perché è l'unico file della
    ''' cartella dati fatto per <b>uscire</b> di lì. <i>(2026-08-27, revisione del giro D.)</i>
    ''' </summary>
    <TestClass>
    Public Class CollaudiDiarioTecnico

        Private Shared Sub ConDiarioTemporaneo(prova As Action(Of DiarioTecnico, CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(), "diario-" & Guid.NewGuid().ToString("N"))
            Dim cartella As New CartellaDati(radice)
            Try
                prova(New DiarioTecnico(cartella), cartella)
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

        <TestMethod>
        Public Sub UnaChiaveApiNonEntraNelDiario()

            ' Non è una precauzione fra le altre: è la condizione perché questo file possa
            ' esistere. Un diario che si manda a qualcuno e porta dentro la chiave è un
            ' modo elaborato di regalarla.
            Dim ripulito As String = DiarioTecnico.SenzaSegreti(
                "chiamata rifiutata con sk-ant-api03-FINTA-abcdefghij1234")

            Assert.DoesNotContain("abcdefghij", ripulito, "il grosso della chiave sparisce")
            Assert.Contains("sk-…1234", ripulito, "restano le ultime quattro, come nell'interfaccia (cap. 11.3)")

        End Sub

        <TestMethod>
        Public Sub AncheLIntestazioneCheLaPortaSiRipulisce()

            For Each riga As String In New String() {
                "x-api-key: sk-ant-FINTA-9999", "Authorization: Bearer qualcosa-di-lungo-e-segreto"}

                Dim ripulito As String = DiarioTecnico.SenzaSegreti(riga)

                Assert.Contains("«tolta»", ripulito, "il valore dell'intestazione se ne va")
                Assert.DoesNotContain("Bearer qualcosa", ripulito)
                Assert.DoesNotContain("9999", ripulito.Replace("sk-…", ""))

            Next

        End Sub

        <TestMethod>
        Public Sub IlDiarioScriveUnaRigaPerVoltaConLIstanteEIlFuso()
            ConDiarioTemporaneo(
                Sub(diario, cartella)
                    diario.Annota("prima cosa")
                    diario.Annota("seconda cosa" & vbCrLf & "che era andata a capo")

                    Dim righe As String() = File.ReadAllLines(cartella.FileLog)

                    Assert.HasCount(2, righe, "un messaggio a capo resta una riga sola: un diario si legge a righe")
                    Assert.EndsWith("prima cosa", righe(0))
                    Assert.EndsWith("seconda cosa che era andata a capo", righe(1))
                    Assert.IsTrue(Text.RegularExpressions.Regex.IsMatch(
                                      righe(0), "^\d{4}-\d\d-\d\d \d\d:\d\d:\d\d[+-]\d\d:\d\d  "),
                                  "l'istante col fuso, come le date su disco")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnDiarioCheNonSiLasciaScrivereNonFaCadereIlProgramma()

            ' La prima promessa del diario: non solleva mai. Chi lo chiama non ha niente
            ' da gestire, e non deve nemmeno pensarci.
            Dim impossibile As New CartellaDati(Path.Combine(Path.GetTempPath(), "diario-" & Guid.NewGuid().ToString("N")))
            Dim diario As New DiarioTecnico(impossibile)

            File.WriteAllText(impossibile.Radice, "questo non è una cartella, è un file")
            Try
                diario.Annota("qualcosa")
                Assert.HasCount(0, diario.UltimeRighe(10), "non ha scritto, e non ha protestato")
            Finally
                If File.Exists(impossibile.Radice) Then File.Delete(impossibile.Radice)
            End Try

        End Sub

        <TestMethod>
        Public Sub IlDiarioNonCresceSenzaFine()
            ConDiarioTemporaneo(
                Sub(diario, cartella)
                    Directory.CreateDirectory(cartella.CartellaLog)
                    File.WriteAllText(cartella.FileLog, New String("x"c, CInt(DiarioTecnico.TettoInByte) + 10))

                    diario.Annota("la riga che fa traboccare")

                    Assert.IsTrue(File.Exists(cartella.FileLogPrecedente), "il diario grosso diventa quello di prima")
                    Assert.IsTrue(New FileInfo(cartella.FileLog).Length < DiarioTecnico.TettoInByte,
                                  "e quello nuovo riparte piccolo")
                    Assert.HasCount(1, diario.UltimeRighe(10), "con dentro la riga che l'ha aperto")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnGuastoSiAnnotaColTipoEIlMessaggio()
            ConDiarioTemporaneo(
                Sub(diario, cartella)
                    diario.AnnotaGuasto("la stampa del PDF", New TimeoutException("non è finita entro 60 secondi"))

                    Dim riga As String = diario.UltimeRighe(1)(0)

                    Assert.Contains("la stampa del PDF", riga, "dove è successo")
                    Assert.Contains("TimeoutException", riga, "che cos'era")
                    Assert.Contains("non è finita entro 60 secondi", riga, "che cosa diceva")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LaDiagnosticaDiceChiEQuestoProgrammaESiPortaDietroIlDiario()

            Dim foglio As String = Diagnostica.Componi(
                New Date(2026, 8, 27, 12, 30, 0),
                "Ver. 1.0.000 · Pool 1.13 (integrato)",
                "Codice sorgente: 23f4df7",
                "C:\dati\TrovaLavoro",
                "claude-haiku-4-5 (estrazione) · claude-sonnet-5 (ragionamento)",
                {"2026-08-27 12:29:00+02:00  GUASTO in la stampa del PDF"})

            Assert.Contains("Ver. 1.0.000", foglio, "che versione è")
            Assert.Contains("23f4df7", foglio, "e da quale codice nasce: senza, non si sa cosa si sta guardando")
            Assert.Contains("C:\dati\TrovaLavoro", foglio, "dove tiene i dati")
            Assert.Contains("claude-sonnet-5", foglio, "con quali modelli lavora")
            Assert.Contains("GUASTO in la stampa del PDF", foglio, "e che cosa è andato storto")

        End Sub

        <TestMethod>
        Public Sub LaDiagnosticaNonPortaFuoriLaChiave()

            ' Il foglietto è fatto apposta per uscire dalla macchina: qui la regola del
            ' diario vale due volte.
            Dim foglio As String = Diagnostica.Componi(
                New Date(2026, 8, 27, 12, 30, 0), "Ver. 1.0.000", "Codice sorgente: 23f4df7",
                "C:\dati", "un modello", {"chiave rifiutata: sk-ant-api03-FINTA-abcdefghij1234"})

            Assert.DoesNotContain("abcdefghij", foglio)
            Assert.Contains("sk-…1234", foglio)

        End Sub

        <TestMethod>
        Public Sub SenzaDiarioLaDiagnosticaLoDiceInveceDiSembrareMonca()

            Dim foglio As String = Diagnostica.Componi(
                New Date(2026, 8, 27, 12, 30, 0), Nothing, Nothing, Nothing, Nothing, Nothing)

            Assert.Contains("nessun guasto annotato", foglio, "il silenzio si dichiara")
            Assert.Contains("non ancora montata", foglio, "e anche quel che manca")

        End Sub

    End Class

End Namespace
