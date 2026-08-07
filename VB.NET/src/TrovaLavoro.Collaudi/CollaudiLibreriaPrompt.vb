Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi della libreria dei prompt (cap. 04). Verificano che il pool
    ''' integrato ci sia tutto, che i metadati siano quelli del prototipo, che il
    ''' pool esterno abbia la precedenza quando è valido e che si ripieghi
    ''' sull'integrato — dicendolo — quando non lo è.
    ''' </summary>
    <TestClass>
    Public Class CollaudiLibreriaPrompt

        ' I 15 prompt migrati dal prototipo (Pool 1.00).
        Private Shared ReadOnly Quindici As String() = {
            "nome", "contatti", "patente", "esperienze_formali", "esperienze_informali",
            "competenze", "formazione", "importa_cv", "trascrizione_pdf",
            "analisi_annuncio", "confronto", "mitigazione",
            "cv_base", "cv_mirato", "lettera"}

        <TestMethod>
        Public Sub IlPoolIntegratoSiApre()
            ' Nessuna cartella esterna: deve valere la copia dentro l'eseguibile.
            Dim libreria = LibreriaPrompt.Apri(Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Assert.AreEqual(OriginePool.Integrato, libreria.Origine, "origine")
            Assert.AreEqual("1.00", libreria.Versione, "versione del pool")
            Assert.AreEqual("Pool 1.00 (integrato)", libreria.Etichetta, "etichetta accanto al logo")
        End Sub

        <TestMethod>
        Public Sub TuttiEQuindiciSiCaricano()
            Dim libreria = LibreriaPrompt.Apri(Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            For Each id As String In Quindici
                Dim p = libreria.Carica(id)
                Assert.AreEqual(id, p.Id, $"«{id}»: l'id dichiarato non corrisponde")
                Assert.IsFalse(String.IsNullOrWhiteSpace(p.Corpo), $"«{id}»: corpo vuoto")
                Assert.IsGreaterThan(0, p.MaxToken, $"«{id}»: max_token non dichiarato")
            Next

            Assert.HasCount(15, Quindici, "il pool 1.00 ha quindici prompt")
        End Sub

        <TestMethod>
        Public Sub IMetadatiSonoQuelliDelPrototipo()
            Dim libreria = LibreriaPrompt.Apri(Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            ' Il confronto: il più esigente, gira sul modello di ragionamento.
            Dim confronto = libreria.Carica("confronto")
            Assert.AreEqual("ragionamento", confronto.Modello, "confronto: modello")
            Assert.AreEqual(4000, confronto.MaxToken, "confronto: max_token")
            Assert.AreEqual("json", confronto.Uscita, "confronto: uscita")
            CollectionAssert.AreEqual({"PROFILO", "ANNUNCIO"}, confronto.Segnaposto.ToArray(),
                                      "confronto: segnaposto")

            ' Un turno del profilo: estrazione semplice.
            Dim nome = libreria.Carica("nome")
            Assert.AreEqual("semplice", nome.Modello, "nome: modello")
            Assert.AreEqual(1500, nome.MaxToken, "nome: max_token")

            ' La trascrizione del PDF è l'unica che risponde in testo, non in JSON,
            ' e l'unica senza segnaposto.
            Dim pdf = libreria.Carica("trascrizione_pdf")
            Assert.AreEqual("testo", pdf.Uscita, "trascrizione_pdf: uscita")
            Assert.IsEmpty(pdf.Segnaposto, "trascrizione_pdf: nessun segnaposto")

            ' L'import da CV chiede più spazio: produce il profilo intero.
            Assert.AreEqual(3000, libreria.Carica("importa_cv").MaxToken, "importa_cv: max_token")

            ' La lettera è quella con più dati iniettati.
            Assert.HasCount(5, libreria.Carica("lettera").Segnaposto, "lettera: segnaposto")
        End Sub

        <TestMethod>
        Public Sub LaVarianteDiLinguaVieneScelta()
            ' cv_base esiste solo come cv_base.it.md: va trovato chiedendo "cv_base".
            Dim libreria = LibreriaPrompt.Apri(Path.Combine(Path.GetTempPath(), "pool-inesistente"))
            Dim p = libreria.Carica("cv_base", "it")

            Assert.AreEqual("cv_base", p.Id, "id")
            Assert.AreEqual("it", p.Lingua, "lingua")
        End Sub

        <TestMethod>
        Public Sub RiempiMetteIDatiAlPostoDeiSegnaposto()
            Dim libreria = LibreriaPrompt.Apri(Path.Combine(Path.GetTempPath(), "pool-inesistente"))
            Dim confronto = libreria.Carica("confronto")

            Dim testo = LibreriaPrompt.Riempi(confronto, New Dictionary(Of String, String) From {
                {"PROFILO", "{""nome"": ""Mario Rossi""}"},
                {"ANNUNCIO", "{""titolo"": ""Magazziniere""}"}})

            Assert.Contains("Mario Rossi", testo, "il profilo deve essere finito nel prompt")
            Assert.Contains("Magazziniere", testo, "l'annuncio deve essere finito nel prompt")
            Assert.DoesNotContain("{{", testo, "non devono restare segnaposto da riempire")
        End Sub

        <TestMethod>
        Public Sub RiempiSenzaUnValoreSiFermaSubito()
            ' Meglio un errore chiaro adesso che una chiamata all'AI con un buco dentro.
            Dim libreria = LibreriaPrompt.Apri(Path.Combine(Path.GetTempPath(), "pool-inesistente"))
            Dim confronto = libreria.Carica("confronto")

            Assert.Throws(Of ArgumentException)(
                Sub() LibreriaPrompt.Riempi(confronto, New Dictionary(Of String, String) From {
                    {"PROFILO", "{}"}}),
                "manca ANNUNCIO: doveva sollevare")
        End Sub

        <TestMethod>
        Public Sub IlPoolEsternoHaLaPrecedenza()
            Dim cartella = CreaPoolFinto("1.99", corpoDiverso:=False)
            Try
                Dim libreria = LibreriaPrompt.Apri(cartella)

                Assert.AreEqual(OriginePool.Esterno, libreria.Origine, "origine")
                Assert.AreEqual("1.99", libreria.Versione, "versione")
                Assert.AreEqual("Pool 1.99", libreria.Etichetta, "etichetta senza «(integrato)»")
                Assert.Contains("finto", libreria.Carica("nome").Corpo, "deve leggere il file esterno")
            Finally
                Directory.Delete(cartella, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub UnFileModificatoMetteLAsterisco()
            ' Trasparente, non poliziesco: il pool funziona, ma lo dichiara.
            Dim cartella = CreaPoolFinto("1.99", corpoDiverso:=True)
            Try
                Dim libreria = LibreriaPrompt.Apri(cartella)

                Assert.AreEqual("1.99*", libreria.Versione, "la versione deve portare l'asterisco")
                Assert.IsNotNull(libreria.Avviso, "l'avviso deve dire quali file")
                Assert.Contains("nome.md", libreria.Avviso, "l'avviso deve nominare il file")
                Assert.IsNotNull(libreria.Carica("nome"), "ma il prompt si deve caricare lo stesso")
            Finally
                Directory.Delete(cartella, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub UnPoolEsternoRottoRipiegaSullIntegrato()
            ' Il manifest dichiara un file che non c'è: mai un avvio muto con prompt
            ' sbagliati, si torna all'integrato dicendolo.
            Dim cartella = Path.Combine(Path.GetTempPath(), "pool-rotto-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(cartella)
            File.WriteAllText(Path.Combine(cartella, "pool_manifest.json"),
                "{""formato"":1,""versione_pool"":""9.99"",""data"":""2026-08-06""," &
                """file"":[{""percorso"":""profilo/manca.md"",""sha256"":""00""}]}")
            Try
                Dim libreria = LibreriaPrompt.Apri(cartella)

                Assert.AreEqual(OriginePool.Integrato, libreria.Origine, "deve ripiegare sull'integrato")
                Assert.AreEqual("1.00", libreria.Versione, "con la versione dell'integrato")
                Assert.IsNotNull(libreria.Avviso, "e deve dire perché")
                Assert.Contains("manca.md", libreria.Avviso, "l'avviso deve nominare il file mancante")
            Finally
                Directory.Delete(cartella, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub IlManifestDelPoolIntegratoECoerente()
            ' La rete di sicurezza da qui in avanti: se qualcuno modifica un prompt
            ' senza risigillare il pool, l'apertura lo dichiara e questo diventa rosso.
            Dim libreria = LibreriaPrompt.Apri(Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Assert.IsNull(libreria.Avviso,
                          "nessun file deve divergere dal manifest; se diverge, va risigillato il pool")
            Assert.DoesNotContain("*", libreria.Versione,
                                  "la versione non deve portare l'asterisco delle modifiche")
        End Sub

        <TestMethod>
        Public Sub SigillaProduceUnPoolCheSiRiapreValido()
            ' Il rito del bump (cap. 4.5): si tocca un file, si risigilla, e il pool
            ' torna coerente senza asterisco.
            Dim cartella = CreaPoolFinto("1.99", corpoDiverso:=True)
            Try
                ' Prima del sigillo il file non corrisponde al manifest.
                Assert.AreEqual("1.99*", LibreriaPrompt.Apri(cartella).Versione,
                                "di partenza il pool risulta modificato")

                LibreriaPrompt.Sigilla(cartella, "2.00", "2026-08-06")

                Dim risigillato = LibreriaPrompt.Apri(cartella)
                Assert.AreEqual("2.00", risigillato.Versione, "versione nuova, senza asterisco")
                Assert.IsNull(risigillato.Avviso, "e nessun avviso")
                Assert.AreEqual(OriginePool.Esterno, risigillato.Origine, "resta il pool esterno")
            Finally
                Directory.Delete(cartella, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub SigillaElencaTuttiIFileTrovati()
            ' Un prompt aggiunto a mano deve entrare nel manifest al primo sigillo.
            Dim cartella = CreaPoolFinto("1.99", corpoDiverso:=False)
            Try
                Directory.CreateDirectory(Path.Combine(cartella, "annuncio"))
                File.WriteAllText(Path.Combine(cartella, "annuncio", "aggiunto.md"),
                    "id: aggiunto" & vbLf & "max_token: 100" & vbLf & "segnaposto: " & vbLf &
                    "---" & vbLf & "Testo." & vbLf)

                LibreriaPrompt.Sigilla(cartella, "2.00", "2026-08-06")

                Dim manifest = File.ReadAllText(Path.Combine(cartella, "pool_manifest.json"))
                Assert.Contains("annuncio/aggiunto.md", manifest, "il file nuovo deve essere nel manifest")
                Assert.Contains("profilo/nome.md", manifest, "e quello vecchio deve restarci")

                Dim risigillato = LibreriaPrompt.Apri(cartella)
                Assert.IsNull(risigillato.Avviso, "il pool risigillato deve essere coerente")
                Assert.IsNotNull(risigillato.Carica("aggiunto"), "e il prompt nuovo deve caricarsi")
            Finally
                Directory.Delete(cartella, recursive:=True)
            End Try
        End Sub

        <TestMethod>
        Public Sub SigillaLasciaFuoriIDocumentiCheNonSonoPrompt()
            ' Nel pool vive anche il CHANGELOG (cap. 4.3), che è un documento. Se
            ' finisse nel manifest, il rito del bump si morderebbe la coda: si sigilla,
            ' si annota il changelog, e il pool appena sigillato risulta modificato.
            Dim cartella = CreaPoolFinto("1.99", corpoDiverso:=False)
            Try
                File.WriteAllText(Path.Combine(cartella, "CHANGELOG.md"),
                    "# Storia del pool" & vbLf & vbLf & "## Pool 1.99 — 2026-08-07" & vbLf)

                LibreriaPrompt.Sigilla(cartella, "2.00", "2026-08-07")

                Dim manifest = File.ReadAllText(Path.Combine(cartella, "pool_manifest.json"))
                Assert.DoesNotContain("CHANGELOG.md", manifest, "il changelog non è un prompt")
                Assert.Contains("profilo/nome.md", manifest, "i prompt veri devono restare nel manifest")

                ' La prova del nove: annotare il changelog non deve sporcare il pool.
                File.AppendAllText(Path.Combine(cartella, "CHANGELOG.md"), vbLf & "Annotazione." & vbLf)

                Dim dopo = LibreriaPrompt.Apri(cartella)
                Assert.IsNull(dopo.Avviso, "annotare il changelog non è modificare un prompt")
                Assert.AreEqual("2.00", dopo.Versione, "e la versione resta senza asterisco")
            Finally
                Directory.Delete(cartella, recursive:=True)
            End Try
        End Sub

        ''' <summary>Un pool esterno minimo, con un solo prompt e il suo manifest.</summary>
        Private Shared Function CreaPoolFinto(versione As String, corpoDiverso As Boolean) As String

            Dim cartella = Path.Combine(Path.GetTempPath(), "pool-finto-" & Guid.NewGuid().ToString("N"))
            Directory.CreateDirectory(Path.Combine(cartella, "profilo"))

            Dim corpo = "id: nome" & vbLf & "versione: 1.0" & vbLf & "lingua: it" & vbLf &
                        "modello: semplice" & vbLf & "max_token: 1500" & vbLf & "uscita: json" & vbLf &
                        "segnaposto: RISPOSTA_UTENTE" & vbLf & "descrizione: prompt finto di collaudo" & vbLf &
                        "---" & vbLf & "Prompt finto per il collaudo." & vbLf & "{{RISPOSTA_UTENTE}}" & vbLf
            File.WriteAllText(Path.Combine(cartella, "profilo", "nome.md"), corpo)

            ' L'impronta si calcola sul contenuto dichiarato: se poi il file cambia
            ' senza rifare il manifest, il caricatore deve accorgersene.
            Dim impronta = LibreriaPrompt.Impronta(If(corpoDiverso, corpo & "una riga in più" & vbLf, corpo))

            File.WriteAllText(Path.Combine(cartella, "pool_manifest.json"),
                $"{{""formato"":1,""versione_pool"":""{versione}"",""data"":""2026-08-06""," &
                $"""file"":[{{""percorso"":""profilo/nome.md"",""sha256"":""{impronta}""}}]}}")

            Return cartella

        End Function

    End Class

End Namespace
