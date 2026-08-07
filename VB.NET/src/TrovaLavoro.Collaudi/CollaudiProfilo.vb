Imports System.Linq
Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi del profilo tipizzato (cap. 02.2, cap. 11.1). Due proprietà da tenere
    ''' ferme, e una terza che è il motivo stesso della tipizzazione:
    ''' <list type="number">
    ''' <item>andata e ritorno non perdono nulla e non cambiano l'ordine delle chiavi —
    ''' il profilo entra nei prompt attraverso <c>ComeNelPrompt</c>, e la parità
    ''' carattere per carattere con il prototipo è ciò che misura la non-regressione
    ''' (cap. 14, T2);</item>
    ''' <item>un campo assente vale «vuoto» e non fa cadere il programma;</item>
    ''' <item>ciò che non appartiene allo schema <b>non entra</b>: è la guardia
    ''' anti-invenzione al confine con l'AI.</item>
    ''' </list>
    ''' </summary>
    <TestClass>
    Public Class CollaudiProfilo

        <TestMethod>
        Public Sub IlProfiloDelBancoFaAndataERitornoSenzaPerdere()
            ' Il caso vero della batteria di T2, che è anche il più ricco: accenti,
            ' apostrofi, liste, campi vuoti. Il confronto si fa sulla forma con cui il
            ' profilo entra davvero in un prompt.
            Dim originale As JsonNode = CasiDiCollaudo.Profilo()
            Dim rifatto As JsonNode = TrovaLavoro.Dati.Profilo.DaJson(originale).VersoJson()

            Dim differenza As String = CasiDiCollaudo.PrimaDifferenza(
                LibreriaPrompt.ComeNelPrompt(originale), LibreriaPrompt.ComeNelPrompt(rifatto))

            Assert.IsNull(differenza, If(differenza, "identici"))
        End Sub

        <TestMethod>
        Public Sub LOrdineDelleChiaviEQuelloDelPrototipo()
            ' Controllo indipendente dal file dei casi: l'ordine è dichiarato qui, così
            ' se qualcuno riordina i campi della classe il collaudo lo dice subito.
            Dim json As JsonObject = TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo()).VersoJson()

            Assert.AreEqual(
                "nome, contatti, patente, esperienze_formali, esperienze_informali, competenze, formazione",
                Chiavi(json), "chiavi del profilo")
            Assert.AreEqual("email, telefono, citta, link",
                            Chiavi(CType(json("contatti"), JsonObject)), "chiavi dei contatti")
            Assert.AreEqual("ha, categorie",
                            Chiavi(CType(json("patente"), JsonObject)), "chiavi della patente")
            Assert.AreEqual("ruolo, azienda, durata, cosa_facevo, tipo",
                            Chiavi(CType(CType(json("esperienze_formali"), JsonArray)(0), JsonObject)),
                            "chiavi di un'esperienza formale")
            Assert.AreEqual("cosa_facevo, quando, con_chi",
                            Chiavi(CType(CType(json("esperienze_informali"), JsonArray)(0), JsonObject)),
                            "chiavi di un'esperienza informale")
            Assert.AreEqual("titolo, istituto, anno",
                            Chiavi(CType(CType(json("formazione"), JsonArray)(0), JsonObject)),
                            "chiavi di una voce di formazione")
        End Sub

        <TestMethod>
        Public Sub UnProfiloVuotoHaComunqueTuttiICampi()
            ' L'artefatto non deve mai avere buchi: chi legge il profilo trova sempre le
            ' sette chiavi, anche quando l'utente non ha ancora detto nulla.
            Dim json As JsonObject = New TrovaLavoro.Dati.Profilo().VersoJson()

            Assert.AreEqual(
                "nome, contatti, patente, esperienze_formali, esperienze_informali, competenze, formazione",
                Chiavi(json), "chiavi del profilo vuoto")
            Assert.IsEmpty(json("nome").ToString(), "nome vuoto")
            Assert.IsEmpty(CType(json("esperienze_formali"), JsonArray), "nessuna esperienza")
            Assert.IsEmpty(CType(json("competenze"), JsonArray), "nessuna competenza")
        End Sub

        <TestMethod>
        Public Sub ICampiAssentiValgonoVuoto()
            ' Un profilo salvato da una versione precedente, o un frammento monco, si
            ' devono riaprire lo stesso.
            Dim p As TrovaLavoro.Dati.Profilo = TrovaLavoro.Dati.Profilo.DaTesto("{}")

            Assert.IsEmpty(p.Nome, "nome")
            Assert.IsEmpty(p.Contatti.Email, "email")
            Assert.IsEmpty(p.Patente.Ha, "patente")
            Assert.IsEmpty(p.Patente.Categorie, "categorie")
            Assert.IsEmpty(p.EsperienzeFormali, "esperienze formali")
            Assert.IsEmpty(p.EsperienzeInformali, "esperienze informali")
            Assert.IsEmpty(p.Competenze, "competenze")
            Assert.IsEmpty(p.Formazione, "formazione")
        End Sub

        <TestMethod>
        Public Sub CioCheNonEDelloSchemaNonEntraNelProfilo()
            ' La guardia anti-invenzione al confine: se il modello aggiunge un campo che
            ' lo schema non prevede, il profilo non se lo porta dietro.
            Dim inventato As String = "{""nome"": ""Luca"", ""stipendio_desiderato"": ""3000 euro""," &
                                      """esperienze_formali"": [{""ruolo"": ""Magazziniere"", ""voto"": 10}]}"

            Dim json As String = TrovaLavoro.Dati.Profilo.DaTesto(inventato).VersoJson().ToJsonString()

            Assert.Contains("Magazziniere", json, "ciò che è dello schema resta")
            Assert.DoesNotContain("stipendio_desiderato", json, "il campo inventato non entra")
            Assert.DoesNotContain("voto", json, "nemmeno dentro una voce")
        End Sub

        <TestMethod>
        Public Sub LeVociMalformateVengonoSaltate()
            ' Una lista che contiene una stringa dove ci vorrebbe un oggetto: si salta
            ' quella voce, non si perde il resto e non si cade.
            Dim storto As String = "{""esperienze_formali"": [""non sono un oggetto""," &
                                   " {""ruolo"": ""Commesso""}], ""competenze"": [""Cassa"", 7, null]}"

            Dim p As TrovaLavoro.Dati.Profilo = TrovaLavoro.Dati.Profilo.DaTesto(storto)

            Assert.HasCount(1, p.EsperienzeFormali, "resta la voce buona")
            Assert.AreEqual("Commesso", p.EsperienzeFormali(0).Ruolo, "ruolo")
            Assert.HasCount(2, p.Competenze, "il null si salta, il numero si legge come testo")
        End Sub

        <TestMethod>
        Public Sub UnProfiloCheNonEUnOggettoVieneRifiutato()
            ' Meglio l'errore in faccia che un profilo vuoto e silenzioso al posto di
            ' quello dell'utente.
            Assert.Throws(Of JsonException)(
                Sub() TrovaLavoro.Dati.Profilo.DaJson(JsonNode.Parse("[1, 2, 3]")))
        End Sub

        <TestMethod>
        Public Sub IlTestoSuDiscoTieneGliAccentiInChiaro()
            ' Cap. 11.1: file «leggibili in qualsiasi editor». Con l'encoder predefinito
            ' di .NET «Forlì» finirebbe scritto a codici.
            Dim p As TrovaLavoro.Dati.Profilo = TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo())
            Dim testo As String = p.ComeTesto()

            Assert.Contains("Forlì", testo, "l'accento resta una lettera")
            ' La sequenza di escape va scritta spezzata, se no la scriviamo per davvero.
            Assert.DoesNotContain(ChrW(92) & "u00EC", testo, "e non la sua sequenza di escape")
            Assert.Contains(vbLf, testo, "il file è indentato, non una riga sola")

            ' E ciò che si scrive si rilegge uguale.
            Dim differenza As String = CasiDiCollaudo.PrimaDifferenza(
                testo, TrovaLavoro.Dati.Profilo.DaTesto(testo).ComeTesto())
            Assert.IsNull(differenza, If(differenza, "identici"))
        End Sub

        ''' <summary>I nomi delle chiavi di un oggetto JSON, nell'ordine in cui stanno.</summary>
        Private Shared Function Chiavi(oggetto As JsonObject) As String
            Return String.Join(", ", oggetto.Select(Function(voce) voce.Key))
        End Function

    End Class

End Namespace
