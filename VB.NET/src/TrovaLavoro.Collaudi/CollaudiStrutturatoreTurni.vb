Imports System.IO
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai

Namespace Ai

    ''' <summary>
    ''' Collaudi dello strutturatore dei turni (cap. 12, flusso B). Girano
    ''' <b>senza rete</b>, con lo stesso gestore HTTP finto dei collaudi del client:
    ''' quello che si verifica qui non è come risponde l'AI, ma che i tre pezzi già
    ''' collaudati a T2 siano messi in fila nel modo giusto — il prompt del turno esce
    ''' dal pool riempito con le parole dell'utente, e ciò che torna esce di qui già
    ''' estratto o come errore leggibile.
    ''' </summary>
    <TestClass>
    Public Class CollaudiStrutturatoreTurni

        ''' <summary>Una risposta dell'API che porta il testo indicato.</summary>
        Private Shared Function RispostaCon(testo As String) As String
            Return "{""model"":""claude-haiku-4-5"",""stop_reason"":""end_turn""," &
                   """content"":[{""type"":""text"",""text"":" &
                   JsonValue.Create(testo).ToJsonString() & "}]," &
                   """usage"":{""input_tokens"":10,""output_tokens"":5}}"
        End Function

        ''' <summary>Lo strutturatore vero, col pool integrato e un'API finta dietro.</summary>
        Private Shared Function StrutturatoreDiProva(finta As ApiFinta) As StrutturatoreTurni

            ' Pool integrato nell'eseguibile: si indica una cartella che non esiste.
            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Dim client As New ClientClaude("chiave-di-prova", Nothing, finta)
            client.Pausa = TimeSpan.Zero

            Return New StrutturatoreTurni(libreria, client)

        End Function

        <TestMethod>
        Public Async Function AllAiArrivaIlPromptDelTurnoRiempito() As Task
            ' La risposta dell'utente deve finire dentro il prompt di quel turno, e il
            ' resto della richiesta deve venire dai metadati del prompt (cap. 04): il
            ' turno non sa nulla di modelli né di limiti.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{""nome"":""Luca Ferrari""}")})

            Await StrutturatoreDiProva(finta).StrutturaAsync("nome", "Mi chiamo Luca Ferrari")

            Dim corpo As JsonObject = CType(JsonNode.Parse(finta.UltimoCorpo), JsonObject)
            Dim mandato As String = corpo("messages")(0)("content").ToString()

            Assert.Contains("Mi chiamo Luca Ferrari", mandato, "le parole dell'utente")
            Assert.Contains("ricavare SOLO il nome e cognome", mandato, "il prompt del turno «nome»")
            Assert.DoesNotContain("{{RISPOSTA_UTENTE}}", mandato, "nessun segnaposto rimasto")
            Assert.AreEqual("claude-haiku-4-5", corpo("model").ToString(), "il livello semplice del prompt")
            Assert.AreEqual(4000, CInt(corpo("max_tokens").GetValue(Of Integer)()), "il limite del prompt")
        End Function

        <TestMethod>
        Public Async Function IlFrammentoEsceGiaEstratto() As Task
            ' Il modello incornicia spesso il JSON in un recinto markdown: chi chiama
            ' deve ricevere il frammento, non il testo da sbucciare.
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("Ecco il risultato:" & vbLf &
                                     "```json" & vbLf &
                                     "{""competenze"": [""Uso della cassa""], ""altrove"": {}}" & vbLf &
                                     "```")})

            Dim frammento As JsonNode = Await StrutturatoreDiProva(finta).
                StrutturaAsync("competenze", "Me la cavo alla cassa")

            Assert.AreEqual("Uso della cassa",
                            frammento("competenze")(0).ToString(), "la competenza estratta")

            ' E ogni turno deve partire col prompt suo, non con quello di un altro.
            Dim mandato As String = JsonNode.Parse(finta.UltimoCorpo)("messages")(0)("content").ToString()
            Assert.Contains("ricavare le COMPETENZE", mandato, "il prompt del turno «competenze»")
        End Function

        <TestMethod>
        Public Async Function UnaRispostaIllegibileDiventaUnErroreLeggibile() As Task
            ' Cap. 02.5: mai un crash. L'errore dice il turno e riporta il testo grezzo,
            ' che è ciò che l'utente deve poter vedere («cosa ha risposto il modello»).
            Dim finta As New ApiFinta(New Passo With {
                .Corpo = RispostaCon("Mi dispiace, non ho capito la domanda.")})

            Dim errore As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() StrutturatoreDiProva(finta).StrutturaAsync("nome", "Luca"))

            Assert.AreEqual(CausaErroreAi.RispostaInattesa, errore.Causa, "causa")
            Assert.Contains("nome", errore.Message, "deve dire di quale turno si tratta")
            Assert.Contains("Mi dispiace, non ho capito la domanda.", errore.Message,
                            "e riportare cosa ha risposto il modello")
        End Function

        <TestMethod>
        Public Async Function UnTurnoCheNelPoolNonCESiFermaSubito() As Task
            ' Meglio fermarsi che mandare all'AI il prompt sbagliato. L'errore esce come
            ' ErroreAi e non come eccezione grezza: il pool esterno è modificabile per
            ' design (cap. 04.2), e chi sperimenta deve ricevere il messaggio in
            ' italiano dei pannelli, non un crash a metà dialogo.
            Dim finta As New ApiFinta(New Passo With {.Corpo = RispostaCon("{}")})

            Dim errore As ErroreAi = Await Assert.ThrowsAsync(Of ErroreAi)(
                Function() StrutturatoreDiProva(finta).StrutturaAsync("hobby", "Vado in bicicletta"))

            Assert.AreEqual(CausaErroreAi.Richiesta, errore.Causa, "è un errore nostro, non dell'API")
            Assert.Contains("hobby", errore.Message, "deve nominare il turno che non c'è")
            Assert.AreEqual(0, finta.Chiamate, "e non deve aver chiamato l'AI")
        End Function

        ''' <summary>
        ''' L'unico collaudo di questa classe che chiama l'API vera: è la prova che la
        ''' catena pool → client → estrattore funziona davvero, e soprattutto che
        ''' l'<b>anti-perdita</b> arriva fin qui — a un turno si racconta anche altro, e
        ''' il campo <c>altrove</c> deve tornare pieno, con le parole dell'utente.
        ''' Categoria <b>Reale</b>: fuori dalla batteria di tutti i giorni, si lancia
        ''' dove c'è la chiave, da <c>VB.NET/src</c>, con
        ''' <c>dotnet test --settings TrovaLavoro.Collaudi/collaudi-reali.runsettings</c>
        ''' (a differenza di quelli del confronto, a questo il prototipo non serve).
        ''' </summary>
        <TestMethod, TestCategory("Reale")>
        Public Async Function IlTurnoRealeRestituisceAncheLAltrove() As Task

            Dim chiave As String
            Try
                chiave = ClientClaude.ChiaveDaAmbiente()
            Catch ex As ErroreAi
                Assert.Inconclusive(
                    "Collaudo reale saltato: manca la chiave. Definisci " &
                    ClientClaude.NomeVariabileChiave & " nell'ambiente prima di lanciare il banco.")
                Return
            End Try

            Dim libreria As LibreriaPrompt = LibreriaPrompt.Apri(
                Path.Combine(Path.GetTempPath(), "pool-inesistente"))

            Using client As New ClientClaude(chiave)

                Dim strutturatore As New StrutturatoreTurni(libreria, client)

                ' Nel turno dei contatti l'utente infila anche un titolo di studio: il
                ' recapito va nel campo suo, il diploma deve finire in «altrove».
                Dim frammento As JsonNode = Await strutturatore.StrutturaAsync(
                    "contatti",
                    "La mia email è luca.ferrari@example.it e il telefono 333 0000000. " &
                    "Ah, ho anche il diploma di istituto tecnico preso nel 2018.")

                Assert.AreEqual("luca.ferrari@example.it",
                                frammento("contatti")("email").ToString(), "l'email va nel suo campo")

                Dim altrove As JsonObject = TryCast(frammento("altrove"), JsonObject)
                Assert.IsNotNull(altrove, "il campo altrove deve esserci")

                Dim formazione As JsonArray = TryCast(altrove("formazione"), JsonArray)
                Assert.IsNotNull(formazione, "il diploma va instradato alla formazione")
                Assert.IsGreaterThan(0, formazione.Count, "e deve portarne il testo")
                Assert.Contains("diploma", formazione(0).ToString().ToLowerInvariant(),
                                "con le parole dell'utente")

            End Using

        End Function

    End Class

End Namespace
