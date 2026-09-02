Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.Json
Imports System.Text.RegularExpressions
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

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
        Public Sub IlCvBaseStaColProfiloEDiceDaQualeVersioneViene()
            ' Il 📄 CV-1 nasce senza alcun annuncio: è il ritratto del profilo in forma di
            ' CV, e vive accanto a lui (cap. 11.1). L'etichetta della versione serve a
            ' poter dire «è di una versione precedente» invece di rigenerarlo di soppiatto.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim versione As String = archivio.Salva(ProfiloDiProva())

                    archivio.SalvaCvBase(
                        Text.Json.Nodes.JsonNode.Parse("{""intestazione"": {""nome"": ""Luca Ferrari""}}"),
                        versione)

                    Assert.IsTrue(File.Exists(cartella.FileCvBase), "il file sta nella cartella del profilo")

                    Dim riletto As CvBase = archivio.CaricaCvBase()
                    Assert.AreEqual("Luca Ferrari",
                                    riletto.Cv("intestazione")("nome").ToString(), "il CV riletto")
                    Assert.AreEqual(versione, riletto.VersioneProfilo, "e la versione da cui è nato")
                    Assert.IsGreaterThan(New Date(2026, 1, 1), riletto.Generato,
                                         "con la data di generazione, non quella vuota")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlCvBaseSiPortaDietroLaSuaLingua()
            ' T7d: rileggendolo bisogna poter impaginare e riesportare il CV con le
            ' etichette della lingua in cui è scritto, e indovinarla dal testo non è un
            ' mestiere di questo strato (cap. 10.3).
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim versione As String = archivio.Salva(ProfiloDiProva())

                    archivio.SalvaCvBase(
                        Text.Json.Nodes.JsonNode.Parse("{""intestazione"": {""nome"": ""Luca Ferrari""}}"),
                        versione, "en")

                    Assert.AreEqual("en", archivio.CaricaCvBase().Lingua, "la lingua è annotata nel file")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub UnCvBaseNatoPrimaDiT7dNonDiceLaLingua()
            ' I file scritti prima il campo non ce l'hanno, e nessuno di essi era in
            ' inglese: la lingua torna vuota e il ripiego lo fa chi legge, una volta sola,
            ' in LinguaDocumenti — come per le candidature di prima di T7a.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Directory.CreateDirectory(Path.GetDirectoryName(cartella.FileCvBase))
                    File.WriteAllText(cartella.FileCvBase,
                        "{""versione_profilo"": ""2026-08-01_120000"", ""generato"": ""2026-08-01 12:00:00""," &
                        """cv"": {""intestazione"": {""nome"": ""Luca Ferrari""}}}")

                    Dim riletto As CvBase = archivio.CaricaCvBase()

                    Assert.IsNull(riletto.Lingua, "il campo non c'era")
                    Assert.AreEqual("it", LinguaDocumenti.PerDocumenti(riletto.Lingua),
                                    "e per chi legge quel vuoto è italiano, non inglese")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub IlCvBaseRicordaCosaHaRiscrittoLUtente()

            ' R7: il 📄 CV-1 base una lettera non ce l'ha, quindi qui non c'è nessuna spia
            ' da accendere — l'annotazione serve all'avviso di «Rigenera», che senza di lei
            ' scadeva al primo rientro in P6.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim versione As String = archivio.Salva(ProfiloDiProva())

                    Dim riscritture As New RiscrittureAMano
                    riscritture.Annota("sommario", New Date(2026, 8, 23, 18, 40, 0))

                    archivio.SalvaCvBase(
                        Text.Json.Nodes.JsonNode.Parse("{""intestazione"": {""nome"": ""Luca Ferrari""}}"),
                        versione, "it", Nothing, riscritture)

                    Assert.AreEqual("sommario",
                                    String.Join(", ", archivio.CaricaCvBase().Riscritture.Campi),
                                    "il campo riscritto a mano torna dal file")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnCvBaseMaiToccatoAManoNonPortaIlBloccoNuovo()

            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim versione As String = archivio.Salva(ProfiloDiProva())

                    archivio.SalvaCvBase(
                        Text.Json.Nodes.JsonNode.Parse("{""intestazione"": {""nome"": ""Luca Ferrari""}}"),
                        versione)

                    Assert.DoesNotContain("riscritture", File.ReadAllText(cartella.FileCvBase),
                                          "un CV che nessuno ha toccato resta scritto com'era")
                    Assert.IsFalse(archivio.CaricaCvBase().Riscritture.CEQualcosa,
                                   "e rileggendolo non se ne inventa nessuna")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub SenzaGenerazioneNonCECvBase()
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Assert.IsNull(archivio.CaricaCvBase(), "mai generato, niente da mostrare")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub CambiatoDopoRiconosceSoloLeVersioniSuperate()
            ' Il gemello di CELaVersione, dal 2026-09-02. Là si chiede se il profilo di
            ' allora esiste ancora — eliminato e rifatto, cioè un'altra persona; qui se ne è
            ' arrivato uno più nuovo — la stessa persona, ma di ieri. È la domanda che
            ' accende «Riconfronta» su una candidatura, e che toglie alla riga del 📄 CV
            ' base il grigio delle didascalie.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Assert.IsFalse(archivio.CambiatoDopo("2026-01-01_000000"),
                                   "storico vuoto: non c'è nessun «dopo» da dichiarare")

                    Dim p As TrovaLavoro.Dati.Profilo = ProfiloDiProva()
                    Dim prima As String = archivio.Salva(p)

                    Assert.IsFalse(archivio.CambiatoDopo(prima),
                                   "l'unica versione che c'è è in pari con sé stessa")

                    Dim dopo As String = archivio.Salva(p)

                    Assert.AreNotEqual(prima, dopo, "due salvataggi lasciano due copie")
                    Assert.IsTrue(archivio.CambiatoDopo(prima),
                                  "e adesso la prima è superata: quel che nacque da lì è di ieri")
                    Assert.IsFalse(archivio.CambiatoDopo(dopo),
                                   "la seconda no: è il profilo di oggi")

                    Assert.IsFalse(archivio.CambiatoDopo(""),
                                   "di una versione non annotata non si fa un allarme")
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
        Public Sub LArchivioSaQuandoEStatoSalvato()
            ' «Aggiornato quando?» è la domanda che il cruscotto e la scheda del profilo
            ' fanno all'archivio: il pannello non deve andare a guardare il disco da sé.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Assert.IsFalse(archivio.UltimoSalvataggio.HasValue, "senza profilo non c'è data")

                    Dim prima As Date = Date.Now.AddSeconds(-1)
                    archivio.Salva(ProfiloDiProva())
                    Dim dopo As Date = Date.Now.AddSeconds(1)

                    Assert.IsTrue(archivio.UltimoSalvataggio.HasValue, "dopo il salvataggio sì")
                    Assert.IsTrue(archivio.UltimoSalvataggio.Value >= prima AndAlso
                                  archivio.UltimoSalvataggio.Value <= dopo,
                                  $"ed è l'istante del salvataggio, ottenuto «{archivio.UltimoSalvataggio}»")
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

        <TestMethod>
        Public Sub EliminareIlProfiloNonLasciaNiente()
            ' Cap. 11.5: «definitivo» vuol dire che dopo non si rimette insieme niente —
            ' né dallo storico, né dal CV base, né da una copia messa in salvo. Il
            ' collaudo mette apposta nella cartella i due residui che un elenco di nomi
            ' da tenere allineato si dimenticherebbe.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim versione As String = archivio.Salva(ProfiloDiProva())
                    archivio.Salva(ProfiloDiProva())
                    archivio.SalvaCvBase(
                        Text.Json.Nodes.JsonNode.Parse("{""intestazione"": {""nome"": ""Luca Ferrari""}}"),
                        versione)

                    File.WriteAllText(Path.Combine(cartella.CartellaOutProfilo, "cv_base.docx"), "documento")
                    File.WriteAllText(Path.Combine(cartella.CartellaProfilo, "profilo.rotto-2026.json"), "rotto")

                    Assert.IsTrue(archivio.EliminaTutto(), "c'era qualcosa da eliminare")

                    Assert.IsFalse(archivio.Esiste, "il profilo non c'è più")
                    Assert.IsEmpty(archivio.Versioni(), "e nemmeno una versione dello storico")
                    Assert.IsNull(archivio.CaricaCvBase(), "né il 📄 CV base")
                    Assert.IsFalse(Directory.Exists(cartella.CartellaProfilo),
                                   "la cartella se n'è andata con tutto quello che aveva dentro")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub EliminareIlProfiloNonToccaLeCandidature()
            ' La separazione del cap. 11.5, ed è il motivo per cui questo gesto esiste:
            ' chi elimina il profilo si toglie di dosso il proprio racconto, non il lavoro
            ' di ricerca già fatto.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    archivio.Salva(ProfiloDiProva())

                    Dim candidatura As String = Path.Combine(cartella.CartellaOpportunita,
                                                             "2026-08-14_rossi-spa_magazziniere")
                    Directory.CreateDirectory(candidatura)
                    File.WriteAllText(Path.Combine(candidatura, "cv.json"), "{}")
                    File.WriteAllText(cartella.FileRegistro, "{}")
                    File.WriteAllText(cartella.FileRicerche, "{}")

                    archivio.EliminaTutto()

                    Assert.IsTrue(File.Exists(Path.Combine(candidatura, "cv.json")),
                                  "la candidatura resta con i suoi documenti")
                    Assert.IsTrue(File.Exists(cartella.FileRegistro), "il registro della Home resta")
                    Assert.IsTrue(File.Exists(cartella.FileRicerche), "e le ricerche salvate restano")
                End Sub)
        End Sub

        <TestMethod>
        Public Sub EliminareQuandoNonCEnienteNonEUnErrore()
            ' Il primo avvio: la cartella del profilo non esiste ancora, e chiedere di
            ' eliminarla non è un guasto — semplicemente non c'era niente.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Assert.IsFalse(archivio.EliminaTutto(), "non c'era niente da eliminare")
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

        <TestMethod>
        Public Sub UnaVersioneEliminataNonCEPiu()

            ' È la domanda che una candidatura fa prima di farsi riscrivere: il profilo da
            ' cui nasco c'è ancora, o è stato eliminato e rifatto da capo? Lo storico non si
            ' pota mai, quindi una versione che manca ha una causa sola (cap. 11.1, 11.5).
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    Dim versione As String = archivio.Salva(ProfiloDiProva())

                    Assert.IsTrue(archivio.CELaVersione(versione), "appena salvata, c'è")
                    Assert.IsFalse(archivio.CELaVersione("2026-07-01_090000"),
                                   "una versione mai esistita, no")

                    archivio.EliminaTutto()

                    Assert.IsFalse(archivio.CELaVersione(versione),
                                   "e dopo «Elimina profilo» non c'è più nemmeno la sua")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaVersioneNonAnnotataNonFermaNiente()

            ' Le candidature nate prima che la versione si annotasse non hanno niente da
            ' cercare, e un dubbio non deve fermare un lavoro.
            ConArchivioTemporaneo(
                Sub(archivio, cartella)
                    archivio.Salva(ProfiloDiProva())

                    Assert.IsTrue(archivio.CELaVersione(Nothing), "senza versione si passa")
                    Assert.IsTrue(archivio.CELaVersione(""), "e con la stringa vuota pure")
                End Sub)

        End Sub

    End Class

End Namespace
