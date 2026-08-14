Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi del montaggio del motore all'avvio. Le cose da tenere ferme sono tre:
    ''' l'applicazione si apre <b>sempre</b>, anche senza chiave e senza file di
    ''' configurazione; i file della cartella dati arrivano davvero fin dentro i servizi
    ''' che li usano (è la ragione per cui questa classe esiste); e ciò che l'utente
    ''' legge nella barra di stato è solo ciò che merita di essere letto.
    ''' </summary>
    ''' <remarks>
    ''' Due cautele di banco. La <b>cartella dati</b> è sempre una cartella temporanea
    ''' con nome irripetibile: la <c>%APPDATA%</c> vera non si tocca. Il <b>pool</b> si
    ''' apre sempre indicando una cartella esterna inesistente, così si usa quello
    ''' integrato nell'assembly e l'esito non dipende da cosa c'è accanto al binario dei
    ''' collaudi.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiContestoApp

        ''' <summary>Il «no» esplicito alla chiave: vale anche dove la variabile d'ambiente c'è.</summary>
        Private Const SenzaChiave As String = ""

        Private Const ChiaveFinta As String = "chiave-di-collaudo"

        Private Shared Function PoolInesistente() As String
            Return Path.Combine(Path.GetTempPath(), "pool-inesistente")
        End Function

        Private Shared Function CartellaTemporanea() As String
            Return Path.Combine(Path.GetTempPath(), "contesto-" & Guid.NewGuid().ToString("N"))
        End Function

        ''' <summary>Monta il contesto su una cartella dati tutta sua.</summary>
        Private Shared Function Monta(radice As String, Optional chiave As String = ChiaveFinta) As ContestoApp
            Return ContestoApp.Monta(radice, chiave, PoolInesistente())
        End Function

        <TestMethod>
        Public Sub SenzaChiaveLApplicazioneSiApreLoStesso()
            ' Cap. 03.8: la finestra deve comparire e dire cosa non va. Senza chiave
            ' restano fuori uso i soli servizi che chiamano l'AI; il profilo su disco si
            ' legge e si corregge ugualmente, quindi l'archivio ci deve essere.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice, SenzaChiave)

                Assert.IsNull(contesto.Client, "niente client")
                Assert.IsNull(contesto.Strutturatore, "niente strutturatore")
                Assert.IsNull(contesto.Trascrittore, "niente trascrittore")
                Assert.IsNull(contesto.ImportCv, "niente import")
                Assert.IsFalse(contesto.AiDisponibile, "AI non disponibile")

                Assert.IsNotNull(contesto.Archivio, "l'archivio del profilo c'è comunque")
                Assert.IsNotNull(contesto.Libreria, "il pool integrato c'è comunque")
                Assert.IsNotNull(contesto.Taratura, "i numeri ci sono comunque")
                Assert.IsNotNull(contesto.Modelli, "i modelli ci sono comunque")

                Assert.IsNotNull(contesto.Avviso, "e l'utente deve saperlo")
                Assert.Contains("chiave API", contesto.Avviso,
                                "l'avviso dice cosa manca, con le parole di chi la digita — " &
                                "non con il nome di una variabile d'ambiente")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaChiaveCifrataVienePrimaDellAmbiente()
            ' Cap. 11.3: quella salvata nell'applicazione è la volontà più recente
            ' dell'utente. Il collaudo regge anche dove la variabile d'ambiente c'è
            ' davvero — è il caso della postazione di sviluppo — perché il file vince.
            Dim radice As String = CartellaTemporanea()
            Try
                Dim segreti As New ArchivioSegreti(New CartellaDati(radice))
                segreti.SalvaChiaveApi("sk-ant-dal-file-cifrato-1111")

                Using contesto As ContestoApp = ContestoApp.Monta(radice, Nothing, PoolInesistente())

                    Assert.IsNotNull(contesto.Client, "la chiave c'è, quindi l'AI si monta")
                    Assert.Contains("file cifrato", String.Join(vbLf, contesto.Note),
                                    "e il resoconto dice da dove è arrivata")
                End Using
            Finally
                Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub LaChiaveIndicataAllAvvioVinceSuTutto()
            ' È la porta del banco: senza questa precedenza un collaudo che monta il
            ' contesto dipenderebbe da cosa c'è nella cartella dati della macchina.
            Dim radice As String = CartellaTemporanea()
            Try
                Dim segreti As New ArchivioSegreti(New CartellaDati(radice))
                segreti.SalvaChiaveApi("sk-ant-dal-file-cifrato-1111")

                Using contesto As ContestoApp = Monta(radice)
                    Assert.Contains("indicata all'avvio", String.Join(vbLf, contesto.Note),
                                    "vale quella dichiarata dal chiamante")
                End Using
            Finally
                Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub UnFileDellaChiaveCheNonSiApreSiFaSentire()
            ' Il file copiato da un altro PC, o salvato da un altro account di Windows:
            ' DPAPI lo rifiuta ed è giusto così, ma l'utente quel file lo vede su disco e
            ' lo crede buono. Tacere sarebbe la cosa peggiore.
            Dim radice As String = CartellaTemporanea()
            Try
                Dim cartella As New CartellaDati(radice)
                cartella.Assicura()
                File.WriteAllBytes(cartella.FileSegreti, New Byte() {9, 9, 9, 9})

                Using contesto As ContestoApp = ContestoApp.Monta(radice, Nothing, PoolInesistente())
                    Assert.IsNotNull(contesto.Avviso, "l'utente deve saperlo")
                    Assert.Contains("non si decifra", contesto.Avviso, "e sapere perché")
                End Using
            Finally
                Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub LaChiaveNonCompareMaiPerInteroNelResoconto()
            ' Cap. 11.3: la diagnostica non contiene mai segreti. Il resoconto del
            ' montaggio è la prima cosa che finirà nel log, e dice da dove viene la
            ' chiave: deve dirlo mostrandone i soli bordi.
            Dim radice As String = CartellaTemporanea()
            Const chiave As String = "sk-ant-non-deve-comparire-mai-5555"

            Using contesto As ContestoApp = Monta(radice, chiave)

                Dim resoconto As String = String.Join(vbLf, contesto.Note)
                Assert.DoesNotContain(chiave, resoconto, "per intero non c'è")
                Assert.DoesNotContain("non-deve-comparire", resoconto, "nemmeno il suo mezzo")
                Assert.Contains("sk-ant-…5555", resoconto, "ma la si riconosce")
            End Using
        End Sub

        <TestMethod>
        Public Sub ConLaChiaveIServiziDellAiSiMontano()
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Assert.IsNotNull(contesto.Client, "client")
                Assert.IsNotNull(contesto.Strutturatore, "strutturatore")
                Assert.IsNotNull(contesto.Trascrittore, "trascrittore")
                Assert.IsNotNull(contesto.ImportCv, "import")
                Assert.IsTrue(contesto.AiDisponibile, "AI disponibile")
            End Using
        End Sub

        <TestMethod>
        Public Sub IlPrimoAvvioNonAllarma()
            ' Cartella dati vuota: taratura e modelli ripiegano sui predefiniti, ed è la
            ' normalità. Se questo comparisse nella barra di stato, l'utente imparerebbe
            ' a non leggerla più — ma nel resoconto va detto lo stesso.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)

                Assert.IsNull(contesto.Avviso, "niente da segnalare all'utente")

                Dim resoconto As String = String.Join(vbLf, contesto.Note)
                Assert.Contains("taratura.json", resoconto, "il ripiego della taratura resta scritto")
                Assert.Contains("modelli.json", resoconto, "e quello dei modelli")
            End Using
        End Sub

        <TestMethod>
        Public Sub IModelliDelFileArrivanoFinoAlClient()
            ' È la ragione per cui questa classe esiste: prima di lei modelli.json era un
            ' file che solo i collaudi aprivano, e ritoccarlo non aveva alcun effetto
            ' sull'applicazione. Il secondo esperimento (cap. 02.5) si fa così.
            Dim radice As String = CartellaTemporanea()
            Directory.CreateDirectory(radice)
            Try
                File.WriteAllText(Path.Combine(radice, "modelli.json"),
                                  "{ ""ragionamento"": ""claude-sonnet-5"" }")

                Using contesto As ContestoApp = Monta(radice)
                    Assert.AreEqual(OrigineModelli.File, contesto.Modelli.Origine, "origine")
                    Assert.AreEqual("claude-sonnet-5", contesto.Modelli.ModelloRagionamento.Id, "nel contesto")
                    Assert.AreEqual("claude-sonnet-5", contesto.Client.ModelliInUso.ModelloRagionamento.Id,
                                    "e dentro il client che farà le chiamate")
                    Assert.IsNull(contesto.Avviso, "un file valido non è un avviso")
                End Using
            Finally
                Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub LaTaraturaDelFileEntraNelContesto()
            Dim radice As String = CartellaTemporanea()
            Directory.CreateDirectory(radice)
            Try
                File.WriteAllText(Path.Combine(radice, "taratura.json"),
                                  "{ ""soglia_stelle_generazione"": 2.5 }")

                Using contesto As ContestoApp = Monta(radice)
                    Assert.AreEqual(OrigineTaratura.File, contesto.Taratura.Origine, "origine")
                    Assert.AreEqual(2.5, contesto.Taratura.SogliaStelleGenerazione, "soglia dal file")
                    Assert.IsNull(contesto.Avviso, "un file valido non è un avviso")
                End Using
            Finally
                Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub LeRicercheDelFileArrivanoNelContesto()
            ' Stessa ragione dei due file dei numeri: un portale aggiunto a mano deve
            ' comparire nel programma, altrimenti «aggiungere un portale è una riga di
            ' ricerche.json» (cap. 06.3) sarebbe una promessa non mantenuta.
            Dim radice As String = CartellaTemporanea()
            Directory.CreateDirectory(radice)
            Try
                File.WriteAllText(Path.Combine(radice, "ricerche.json"),
                                  "{ ""portali"": [ { ""nome"": ""Il mio portale"", " &
                                  """schema"": ""https://esempio.it/jobs?q={cosa}"" } ] }")

                Using contesto As ContestoApp = Monta(radice)
                    Assert.AreEqual(OrigineRicerche.File, contesto.Ricerche.Origine, "origine")
                    Assert.HasCount(1, contesto.Ricerche.Portali, "solo il suo")
                    Assert.AreEqual("Il mio portale", contesto.Ricerche.Portali(0).Nome)
                    Assert.IsNotNull(contesto.ArchivioRicerche, "e da qualche parte si risalva")
                    Assert.IsNull(contesto.Avviso, "un file valido non è un avviso")
                End Using
            Finally
                Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub UnaRicercaScartataSiFaSentireAllAvvio()
            ' Qui il file c'è ed è leggibile: a non entrare è una riga che l'utente ha
            ' scritto. È un avviso e non una nota, perché quella ricerca sta per non
            ' trovarsi nel menù e nessuno gliel'avrebbe detto.
            Dim radice As String = CartellaTemporanea()
            Directory.CreateDirectory(radice)
            Try
                File.WriteAllText(Path.Combine(radice, "ricerche.json"),
                                  "{ ""salvate"": [ { ""nome"": ""Va a vuoto"", " &
                                  """portale"": ""PortaleCheNonCe"" } ] }")

                Using contesto As ContestoApp = Monta(radice)
                    Assert.IsNotNull(contesto.Avviso, "l'utente deve saperlo")
                    Assert.Contains("Va a vuoto", contesto.Avviso, "e sapere quale ricerca")
                    Assert.IsEmpty(contesto.Ricerche.Salvate, "intanto nel menù non c'è")
                End Using
            Finally
                Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub UnFileDiNumeriRottoSiFaSentire()
            ' Un file che c'è ma non si legge è un'anomalia: senza dirlo, chi ha
            ' ritoccato un numero crederebbe che il suo ritocco sia in vigore.
            Dim radice As String = CartellaTemporanea()
            Directory.CreateDirectory(radice)
            Try
                File.WriteAllText(Path.Combine(radice, "taratura.json"), "{ questo non è JSON")

                Using contesto As ContestoApp = Monta(radice)
                    Assert.IsNotNull(contesto.Avviso, "l'utente deve saperlo")
                    Assert.Contains("taratura.json", contesto.Avviso, "e sapere quale file")
                    Assert.AreEqual(1.5, contesto.Taratura.SogliaStelleGenerazione,
                                    "intanto valgono i predefiniti")
                End Using
            Finally
                Directory.Delete(radice, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub IlMontaggioNonCreaNiente()
            ' Aprire e richiudere l'applicazione senza fare nulla non deve lasciare
            ' cartelle vuote in giro: a crearle è chi scrive, quando scrive.
            Dim radice As String = CartellaTemporanea()
            Using contesto As ContestoApp = Monta(radice)
                Assert.IsFalse(Directory.Exists(radice), "la cartella dati non nasce da sola")
                Assert.IsFalse(contesto.Archivio.Esiste, "e nessun profilo c'è ancora")
            End Using
        End Sub

        <TestMethod>
        Public Sub UnaRadiceImpossibileRipiegaSullaPredefinita()
            ' Una configurazione sbagliata non deve impedire l'avvio: si ripiega e si
            ' prosegue. Il carattere nullo è un percorso invalido su ogni sistema.
            Using contesto As ContestoApp = ContestoApp.Monta(
                "C:\cartella" & ChrW(0) & "impossibile", SenzaChiave, PoolInesistente())

                Assert.AreEqual(CartellaDati.RadicePredefinita, contesto.Cartella.Radice,
                                "ripiega sulla cartella dati predefinita")
                Assert.IsNotNull(contesto.Avviso, "e non in silenzio")
                Assert.Contains("cartella dati", contesto.Avviso,
                                "l'avviso dice che la cartella indicata è stata scartata")
            End Using
        End Sub

        <TestMethod>
        Public Sub SmaltirloDueVolteNonFaDanno()
            Dim contesto As ContestoApp = Monta(CartellaTemporanea())

            contesto.Dispose()
            contesto.Dispose()
        End Sub

    End Class

End Namespace
