Imports System.IO
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi di chi scrive l'email di candidatura (cap. 07.1). Girano <b>senza rete</b>,
    ''' con lo stesso gestore HTTP finto degli altri mestieri: la meccanica comune è già
    ''' collaudata sotto (<see cref="CollaudiMestiereAi"/>), qui si verifica ciò che è suo —
    ''' che la <b>lingua</b> chiesta scelga davvero la variante di prompt, e che a partire
    ''' sia il testo di quella variante e non dell'altra.
    ''' </summary>
    ''' <remarks>
    ''' Sono nati dal collaudo reale di T7a, che ha trovato l'email a metà del guado:
    ''' oggetto italiano sopra un corpo inglese. La regola c'era — il prompt italiano dice
    ''' «nella stessa lingua della lettera» — e il corpo l'ha seguita; a disobbedire è stata
    ''' la <b>formula</b> dell'oggetto, che quel prompt detta parola per parola in italiano.
    ''' Fra una regola generale e una forma concreta da imitare vince la forma, ed è la
    ''' ragione per cui la lingua dev'essere il prompt, non un'istruzione dentro il prompt.
    ''' Un collaudo coi finti non poteva vederlo: il finto non carica nessun prompt, e
    ''' quello che parte davvero non lo guardava nessuno.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiCompositoreEmail

        Private Shared Function RispostaCon(testo As String) As String
            Return "{""model"":""claude-sonnet-4-6"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":" &
                   JsonValue.Create(testo).ToJsonString() & "}]," &
                   """usage"":{""input_tokens"":10,""output_tokens"":5}}"
        End Function

        ''' <summary>Il compositore vero, col pool integrato e un'API finta dietro.</summary>
        Private Shared Function CompositoreDiProva(finta As ApiFinta) As CompositoreEmail

            ' Pool integrato nell'eseguibile: si indica una cartella che non esiste.
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero

            Return New CompositoreEmail(libreria, client)

        End Function

        Private Shared Function Lettera() As JsonNode
            Return JsonNode.Parse(
                "{""tipo"": ""lettera_mirata"", ""apertura"": ""Dear Sir or Madam,""," &
                """corpo"": ""I have four years of warehouse experience.""," &
                """chiusura"": ""Yours faithfully,""," &
                """firma"": {""nome"": ""Luca Ferrari"", ""email"": ""luca@example.it""}}")
        End Function

        Private Shared Function Annuncio() As JsonNode
            Return JsonNode.Parse(
                "{""titolo"": ""Warehouse Manager"", ""azienda"": ""Rossi S.p.A.""," &
                """lingua"": ""en""}")
        End Function

        ''' <summary>Il testo del messaggio utente che è partito davvero.</summary>
        Private Shared Function Mandato(finta As ApiFinta) As String
            Return JsonNode.Parse(finta.UltimoCorpo)("messages")(0)("content").ToString()
        End Function

        <TestMethod>
        Public Async Function ChiedendoLIngleseParteIlPromptInglese() As Task

            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("{""tipo"":""email_candidatura"",""oggetto"":"""",""corpo"":""""}")})

            Await CompositoreDiProva(finta).ComponiAsync(
                Lettera(), Annuncio(), {"CV.pdf"}, Nothing, "en")

            Dim testo As String = Mandato(finta)

            Assert.Contains("Application for", testo, "la forma inglese dell'oggetto")
            Assert.Contains("turning an already written covering letter into an email", testo,
                            "e il prompt inglese dell'email")
            Assert.DoesNotContain("Candidatura per", testo,
                                  "nessuna traccia della formula italiana: era l'oggetto ibrido di T7a")
            Assert.DoesNotContain("{{", testo, "nessun segnaposto rimasto")

        End Function

        <TestMethod>
        Public Async Function SenzaDireNienteLEmailRestaInItaliano() As Task

            ' Il predefinito è la lingua di casa: un mestiere che non sa la lingua non deve
            ' inventarsene una, e chi non gliela dice ha una candidatura italiana.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("{""tipo"":""email_candidatura"",""oggetto"":"""",""corpo"":""""}")})

            Await CompositoreDiProva(finta).ComponiAsync(Lettera(), Annuncio(), {"CV.pdf"})

            Dim testo As String = Mandato(finta)

            Assert.Contains("Candidatura per", testo, "la forma italiana dell'oggetto")
            Assert.DoesNotContain("Application for", testo, "e non quella inglese")

        End Function

        <TestMethod>
        Public Async Function LaVarianteIngleseRiceveGliStessiTreIngressi() As Task

            ' La variante non è un prompt diverso: è lo stesso compito in un'altra lingua.
            ' Se per strada le si perdesse un segnaposto, l'email inglese nascerebbe più
            ' povera di quella italiana senza che niente fallisca.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("{""tipo"":""email_candidatura"",""oggetto"":"""",""corpo"":""""}")})

            Await CompositoreDiProva(finta).ComponiAsync(
                Lettera(), Annuncio(), {"CV_EN.pdf", "Lettera_EN.pdf"}, Nothing, "en")

            Dim testo As String = Mandato(finta)

            Assert.Contains("four years of warehouse experience", testo, "1/3 la lettera")
            Assert.Contains("Warehouse Manager", testo, "2/3 l'annuncio, per nominare il ruolo")
            Assert.Contains("CV_EN.pdf", testo, "3/3 gli allegati che partiranno")
            Assert.Contains("Lettera_EN.pdf", testo, "tutti e due")

        End Function

    End Class

End Namespace
