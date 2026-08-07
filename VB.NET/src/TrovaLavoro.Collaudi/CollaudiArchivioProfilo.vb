Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi dell'archivio del profilo (cap. 11.1). Il profilo è l'unica cosa che
    ''' l'utente non può rifare da capo: qui si verifica che salvarlo lo conservi
    ''' davvero — con la sua storia — e che quando qualcosa non va lo si dica invece di
    ''' restituire un profilo vuoto al posto suo.
    ''' </summary>
    <TestClass>
    Public Class CollaudiArchivioProfilo

        <TestMethod>
        Public Sub SalvaERilegge()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim originale As TrovaLavoro.Dati.Profilo = ProfiloDiProva()

                    archivio.Salva(originale)

                    Assert.IsTrue(archivio.Esiste, "il profilo deve esserci")
                    Assert.AreEqual(originale.ComeTesto(), archivio.Carica().ComeTesto(),
                                    "riletto deve essere identico")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SenzaSalvataggioNonCEProfilo()
            ' La domanda del primo avvio (cap. 12, A2): «ce l'ho già» o «costruiamolo».
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Assert.IsFalse(archivio.Esiste, "cartella nuova, nessun profilo")
                    Assert.IsEmpty(archivio.Versioni(), "e nessuna versione")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub OgniSalvataggioLasciaLaSuaCopiaDatata()
            ' Il cuore della versione: la copia di ieri deve restare com'era ieri, se no
            ' un CV già inviato non è più spiegabile (cap. 11.1).
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim p As TrovaLavoro.Dati.Profilo = ProfiloDiProva()
                    Dim prima As String = archivio.Salva(p)

                    p.Competenze.Add("Uso del transpallet elettrico")
                    Dim dopo As String = archivio.Salva(p)

                    Assert.HasCount(2, archivio.Versioni(), "due versioni archiviate")
                    Assert.AreNotEqual(prima, dopo, "con nomi diversi")
                    Assert.DoesNotContain("transpallet", archivio.CaricaVersione(prima).ComeTesto(),
                                          "la prima copia non deve cambiare")
                    Assert.Contains("transpallet", archivio.CaricaVersione(dopo).ComeTesto(),
                                    "la seconda deve avere la novità")
                    Assert.Contains("transpallet", archivio.Carica().ComeTesto(),
                                    "e il profilo corrente è l'ultimo salvato")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlNomeDellaVersioneEUnaDataOra()
            ' Il nome non è un'etichetta qualunque: è ciò che ogni opportunità annoterà
            ' per dire con quale profilo furono generati i suoi documenti (cap. 11.1).
            ' Deve restare leggibile a occhio e ordinabile come testo.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim prima As Date = Date.Now
                    Dim versione As String = archivio.Salva(ProfiloDiProva())
                    Dim dopo As Date = Date.Now

                    Assert.IsTrue(Regex.IsMatch(versione, "^\d{4}-\d{2}-\d{2}_\d{6}$"),
                                  $"formato data_ora atteso, ottenuto «{versione}»")

                    ' Le due date coprono anche il caso — improbabile ma reale — di un
                    ' salvataggio a cavallo della mezzanotte.
                    Dim oggi As String = prima.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    Dim domani As String = dopo.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                    Assert.IsTrue(versione.StartsWith(oggi, StringComparison.Ordinal) OrElse
                                  versione.StartsWith(domani, StringComparison.Ordinal),
                                  $"deve portare la data di oggi, ottenuto «{versione}»")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub LeVersioniDelloStessoSecondoNonSiSovrascrivono()
            ' Tre conferme di fila stanno dentro lo stesso secondo: nessuna delle tre
            ' deve sparire, e l'ordine alfabetico dei nomi deve essere quello del tempo.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim p As TrovaLavoro.Dati.Profilo = ProfiloDiProva()
                    Dim nomi As New List(Of String) From {
                        archivio.Salva(p), archivio.Salva(p), archivio.Salva(p)}

                    Assert.HasCount(3, nomi.Distinct().ToList(), "tre nomi distinti")
                    CollectionAssert.AreEqual(nomi, archivio.Versioni().ToList(),
                                              "elencate nell'ordine in cui sono nate")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlSalvataggioNonLasciaFileATerra()
            ' La scrittura passa da un temporaneo: quando ha finito non ne deve restare
            ' traccia, se no la cartella dell'utente si riempie di scarti.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    archivio.Salva(ProfiloDiProva())

                    Assert.IsEmpty(Directory.GetFiles(cartella.Radice, "*.tmp", SearchOption.AllDirectories),
                                   "nessun temporaneo rimasto")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnProfiloRottoNonPassaPerVuoto()
            ' Il ripiego silenzioso è giusto per taratura e modelli (cap. 11.6), qui no:
            ' restituire un profilo vuoto al posto di uno rotto sarebbe il modo peggiore
            ' di perdere i dati dell'utente.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    archivio.Salva(ProfiloDiProva())
                    File.WriteAllText(cartella.FileProfilo, "{ questo non è JSON")

                    Assert.IsTrue(archivio.Esiste, "il file c'è")
                    Assert.Throws(Of JsonException)(Sub() archivio.Carica())
                End Sub)
        End Sub

        <TestMethod>
        Public Sub SeLoStoricoNonSiPuoScrivereIlProfiloBuonoResta()
            ' La ragione per cui la copia si scrive prima del profilo corrente: quando
            ' qualcosa va storto ci si ferma con il profilo buono ancora al suo posto.
            ' Qui lo storico viene reso inagibile mettendo un file al posto della
            ' cartella — è il modo più semplice per far fallire la scrittura.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim p As TrovaLavoro.Dati.Profilo = ProfiloDiProva()
                    archivio.Salva(p)
                    Dim buono As String = archivio.Carica().ComeTesto()

                    Directory.Delete(cartella.CartellaStorico, recursive:=True)
                    File.WriteAllText(cartella.CartellaStorico, "non sono una cartella")

                    p.Competenze.Add("Uso del transpallet elettrico")
                    Assert.Throws(Of IOException)(Sub() archivio.Salva(p))

                    Assert.AreEqual(buono, archivio.Carica().ComeTesto(),
                                    "il profilo corrente non deve essere stato toccato")
                End Sub)
        End Sub

        ''' <summary>Il profilo del banco, quello ricco di accenti e campi vuoti.</summary>
        Private Shared Function ProfiloDiProva() As TrovaLavoro.Dati.Profilo
            Return TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo())
        End Function

        ''' <summary>
        ''' Esegue una prova su un archivio in una cartella temporanea, e la cancella
        ''' comunque vada: la cartella dati vera dell'utente non si tocca mai.
        ''' </summary>
        Private Shared Sub ConArchivioTemporaneo(prova As Action(Of ArchivioProfilo, CartellaDati))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "archivio-profilo-" & Guid.NewGuid().ToString("N"))
            Dim cartella As New CartellaDati(radice)
            Try
                prova(New ArchivioProfilo(cartella), cartella)
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

    End Class

End Namespace
