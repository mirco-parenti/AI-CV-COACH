Imports System.IO
Imports System.Text.Json.Nodes
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

Namespace Motore

    ''' <summary>
    ''' Collaudi del file di taratura (cap. 11.6): i valori predefiniti devono
    ''' coincidere con le costanti del prototipo, un file assente o corrotto non deve
    ''' impedire l'avvio, e i valori letti devono davvero governare il calcolo.
    ''' </summary>
    <TestClass>
    Public Class CollaudiTaratura

        <TestMethod>
        Public Sub PredefinitiUgualiAlPrototipo()
            ' Le costanti di HTML+JS/server.js, una per una.
            Dim t As Taratura = Taratura.Predefinita()

            Assert.AreEqual(1.0, t.Punti("soddisfatto"), "PUNTI.soddisfatto")
            Assert.AreEqual(0.5, t.Punti("in parte"), "PUNTI['in parte']")
            Assert.AreEqual(0.0, t.Punti("non soddisfatto"), "PUNTI['non soddisfatto']")
            Assert.AreEqual(5.0, t.PesoPriorita("richiesto"), "PESO_PRIORITA.richiesto")
            Assert.AreEqual(1.0, t.PesoPriorita("preferenziale"), "PESO_PRIORITA.preferenziale")
            Assert.AreEqual(5.0, t.PesoImportanza("alta"), "PESO_IMPORTANZA.alta")
            Assert.AreEqual(3.0, t.PesoImportanza("media"), "PESO_IMPORTANZA.media")
            Assert.AreEqual(1.0, t.PesoImportanza("bassa"), "PESO_IMPORTANZA.bassa")
            Assert.AreEqual(0.2, t.PesoContesto, "PESO_CONTESTO")
            Assert.AreEqual(3.0, t.PesoFallback, "PESO_FALLBACK")
            Assert.AreEqual(-20.0, t.ClampGiu, "CLAMP_GIU")
            Assert.AreEqual(10.0, t.ClampSu, "CLAMP_SU")
            Assert.AreEqual(20.0, t.TettoEliminatorio, "TETTO_ELIMINATORIO")
            Assert.AreEqual(1.5, t.SogliaStelleGenerazione, "SOGLIA_STELLE_GENERAZIONE")
        End Sub

        <TestMethod>
        Public Sub FileAssenteRipiegaSuiPredefiniti()
            ' "Una taratura corrotta non deve impedire l'avvio" (cap. 11.6): niente
            ' eccezione, valori di prodotto e un avviso da annotare nel log.
            Dim t As Taratura = Taratura.Carica(Path.Combine(Path.GetTempPath(), "taratura-che-non-esiste.json"))

            Assert.AreEqual(OrigineTaratura.Predefinita, t.Origine, "origine")
            Assert.IsNotNull(t.Avviso, "l'avviso per il log deve esserci")
            Assert.AreEqual(20.0, t.TettoEliminatorio, "deve valere il predefinito")
        End Sub

        <TestMethod>
        Public Sub FileCorrottoRipiegaSuiPredefiniti()
            Dim percorso As String = Path.Combine(Path.GetTempPath(), "taratura-corrotta.json")
            File.WriteAllText(percorso, "{ questo non è JSON")
            Try
                Dim t As Taratura = Taratura.Carica(percorso)
                Assert.AreEqual(OrigineTaratura.Predefinita, t.Origine, "origine")
                Assert.IsNotNull(t.Avviso, "l'avviso per il log deve esserci")
                Assert.AreEqual(20.0, t.TettoEliminatorio, "deve valere il predefinito")
            Finally
                File.Delete(percorso)
            End Try
        End Sub

        <TestMethod>
        Public Sub FileParzialeSostituisceSoloCioCheDichiara()
            ' Ritoccare un valore deve costare una riga: il resto resta al suo posto.
            Dim t As Taratura = Taratura.DaJson("{ ""tetto_eliminatorio"": 35 }")

            Assert.AreEqual(OrigineTaratura.File, t.Origine, "origine")
            Assert.AreEqual(35.0, t.TettoEliminatorio, "il valore dichiarato")
            Assert.AreEqual(-20.0, t.ClampGiu, "un valore non dichiarato resta predefinito")
            Assert.AreEqual(5.0, t.PesoPriorita("richiesto"), "anche le mappe restano")
        End Sub

        <TestMethod>
        Public Sub UnValoreNonNumericoInUnaMappaNonImpedisceLAvvio()
            ' Il refuso più facile da fare a mano: il numero fra virgolette. Prima
            ' faceva cadere l'applicazione all'avvio («il montaggio non solleva mai»);
            ' ora la mappa storta si scarta per intero e resta la predefinita — tenere
            ' solo le voci buone escluderebbe in silenzio gli esiti delle altre.
            Dim t As Taratura = Taratura.DaJson(
                "{ ""punti"": { ""soddisfatto"": ""1"", ""in parte"": 0.4 }, ""clamp_su"": 12 }")

            Assert.AreEqual(1.0, t.Punti("soddisfatto"), "la mappa storta resta la predefinita")
            Assert.AreEqual(0.5, t.Punti("in parte"), "tutta intera")
            Assert.AreEqual(12.0, t.ClampSu, "i valori buoni fuori dalla mappa entrano lo stesso")
        End Sub

        <TestMethod>
        Public Sub LaTaraturaGovernaDavveroIlCalcolo()
            ' Il collaudo che conta: cambiando il tetto cambia il risultato. Stesso
            ' caso 2 della batteria dell'hard-gate, ma con tetto portato a 35.
            Dim g As JsonArray = CType(JsonNode.Parse("[
                {""requisito"":""Patente C indispensabile"",""esito"":""non soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito"",""eliminatorio"":true},
                {""requisito"":""Esperienza di guida"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Diploma"",""esito"":""soddisfatto"",""priorita"":""richiesto"",""importanza"":"""",""categoria"":""requisito""},
                {""requisito"":""Sede a Forlì"",""esito"":""soddisfatto"",""priorita"":""non specificata"",""importanza"":"""",""categoria"":""contesto""}]"), JsonArray)

            Dim conPredefiniti = CalcoloMatch.Calcola(g, JsonValue.Create(70))
            Assert.AreEqual(20, conPredefiniti.MatchFinale, "col tetto di prodotto")

            Dim conTetto35 = CalcoloMatch.Calcola(g, JsonValue.Create(70),
                                                  Taratura.DaJson("{ ""tetto_eliminatorio"": 35 }"))
            Assert.AreEqual(35, conTetto35.MatchFinale, "col tetto ritoccato")
            Assert.Contains("35/100", conTetto35.Nota, "anche la nota deve citare il nuovo tetto")
        End Sub

    End Class

End Namespace
