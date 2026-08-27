Imports System.Globalization
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Ai
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi del conto dell'uso dell'AI (2026-08-27, dalla revisione del giro D): il
    ''' listino dei prezzi e la lettura di <c>chiamate_ai.csv</c>.
    ''' </summary>
    ''' <remarks>
    ''' Le promesse da difendere sono tre. Un modello di cui non si conosce il prezzo
    ''' <b>non vale zero</b>: i suoi token si contano e i suoi soldi no, dichiarandolo. Una
    ''' riga storta non azzera il conto delle altre. E «gli ultimi trenta giorni» si contano
    ''' da un adesso che arriva da fuori, o un collaudo dovrebbe cambiare la data del
    ''' computer.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiContoDelleChiamate

        Private Const Intestazione As String =
            "quando;prompt;modello;tetto;token_ingresso;token_uscita;percentuale_del_tetto;motivo_fine"

        Private Shared ReadOnly Adesso As New Date(2026, 8, 27, 12, 0, 0)

        Private Shared Function Riga(quando As String, modello As String,
                                     ingresso As Integer, uscita As Integer) As String
            Return $"{quando};confronto;{modello};4000;{ingresso};{uscita};12,5;end_turn"
        End Function

        ' ==================================================================
        ' Il listino
        ' ==================================================================

        <TestMethod>
        Public Sub IlListinoConosceIModelliCheIlProgrammaUsa()

            Dim listino As Listino = Listino.Predefinito()

            Assert.IsNotNull(listino.PerModello("claude-haiku-4-5"), "il livello semplice")
            Assert.IsNotNull(listino.PerModello("claude-sonnet-5"), "il ragionamento")
            Assert.IsNotNull(listino.PerModello("claude-sonnet-4-6"), "e quello del prototipo")

        End Sub

        <TestMethod>
        Public Sub IlPrezzoSiTrovaAncheConLIdentificativoDatato()

            ' Nel chiamate_ai.csv finisce il modello che ha risposto, e l'API risponde
            ' con l'identificativo pieno: prima che il listino lo riconoscesse, le
            ' chiamate al livello semplice risultavano senza prezzo a ogni installazione
            ' — il buco del cap. 13.11 aperto sul modello di casa (2026-08-27).
            Dim listino As Listino = Listino.Predefinito()

            Assert.IsNotNull(listino.PerModello("claude-haiku-4-5-20251001"),
                             "l'alias del listino e la versione datata sono lo stesso modello")
            Assert.AreEqual(listino.PerModello("claude-haiku-4-5").Uscita,
                            listino.PerModello("claude-haiku-4-5-20251001").Uscita,
                            "e costano uguale")

        End Sub

        <TestMethod>
        Public Sub LIdentificativoEsattoVinceSulSuoAlias()

            ' Chi in modelli.json dichiara il prezzo di una versione precisa vuole quello.
            Dim m As Modelli = Modelli.DaJson(
                "{ ""prezzi"": { ""claude-haiku-4-5-20251001"": { ""ingresso"": 9, ""uscita"": 99 } } }")

            Assert.AreEqual(99D, m.Prezzi.PerModello("claude-haiku-4-5-20251001").Uscita,
                            "la versione dichiarata")
            Assert.AreEqual(5D, m.Prezzi.PerModello("claude-haiku-4-5").Uscita,
                            "e l'alias resta al prezzo di casa, che è dichiarato anche lui")

        End Sub

        <TestMethod>
        Public Sub UnModelloSconosciutoNonHaPrezzo()

            ' Non vale zero: vale «non lo so», ed è tutta un'altra cosa quando si somma.
            Assert.IsNull(Listino.Predefinito().PerModello("claude-inventato-9"))
            Assert.IsNull(Listino.Predefinito().PerModello(""))

        End Sub

        <TestMethod>
        Public Sub IlCostoSiFaSulMilioneDiToken()

            ' Un milione in ingresso a 2 dollari e un milione in uscita a 10 fanno 12.
            Dim prezzo As New PrezzoModello(2D, 10D)

            Assert.AreEqual(12D, prezzo.Costo(1000000, 1000000), "un milione per parte")
            Assert.AreEqual(0.002D, prezzo.Costo(1000, 0), "mille token in ingresso")

        End Sub

        <TestMethod>
        Public Sub IlFilePuoScavalcareIlListino()

            ' Stessa regola dei modelli: cambiare un numero costa una riga, non una build.
            Dim m As Modelli = Modelli.DaJson(
                "{ ""prezzi"": { ""claude-haiku-4-5"": { ""ingresso"": 9, ""uscita"": 99 } } }")

            Assert.AreEqual(99D, m.Prezzi.PerModello("claude-haiku-4-5").Uscita, "il prezzo dichiarato vince")
            Assert.IsNotNull(m.Prezzi.PerModello("claude-sonnet-5"), "gli altri restano quelli di casa")

        End Sub

        <TestMethod>
        Public Sub UnPrezzoAMetaNonEUnPrezzo()

            ' Completarlo con uno zero darebbe un conto più basso del vero, che è
            ' esattamente il tipo di bugia che un contatore non deve dire.
            Dim m As Modelli = Modelli.DaJson(
                "{ ""prezzi"": { ""claude-haiku-4-5"": { ""ingresso"": 9 } } }")

            Assert.AreEqual(1D, m.Prezzi.PerModello("claude-haiku-4-5").Ingresso,
                            "resta il prezzo di casa, non quello monco")

        End Sub

        ' ==================================================================
        ' Leggere il file
        ' ==================================================================

        <TestMethod>
        Public Sub UnFileCheNonCEsisteNonEUnErrore()

            Dim conto As ContoDoppio = ContoDelleChiamate.Leggi(
                Path.Combine(Path.GetTempPath(), "chiamate-che-non-esistono.csv"),
                Listino.Predefinito(), Adesso)

            Assert.IsFalse(conto.Tutte.CEQualcosa, "nessuna chiamata")
            Assert.AreEqual(0D, conto.Tutte.Spesa, "nessuna spesa")

        End Sub

        <TestMethod>
        Public Sub LIntestazioneNonEUnaChiamata()

            Dim conto As ContoDoppio = ContoDelleChiamate.DalleRighe(
                {Intestazione}, Listino.Predefinito(), Adesso)

            Assert.AreEqual(0, conto.Tutte.Chiamate)

        End Sub

        <TestMethod>
        Public Sub SommaITokenEIlCosto()

            Dim conto As ContoDoppio = ContoDelleChiamate.DalleRighe({
                Intestazione,
                Riga("2026-08-26 10:00:00", "claude-sonnet-5", 1000000, 100000),
                Riga("2026-08-26 11:00:00", "claude-haiku-4-5", 1000000, 0)},
                Listino.Predefinito(), Adesso)

            Assert.AreEqual(2, conto.Tutte.Chiamate, "due chiamate")
            Assert.AreEqual(2100000L, conto.Tutte.TokenIngresso + conto.Tutte.TokenUscita, "i token")

            ' Sonnet 5: 1 M in ingresso a $2 e 100 mila in uscita a $10 = 2 + 1 = 3.
            ' Haiku 4.5: 1 M in ingresso a $1 = 1. In tutto 4.
            Assert.AreEqual(4D, conto.Tutte.Spesa, "la spesa")
            Assert.AreEqual(0, conto.Tutte.SenzaPrezzo, "tutte valutate")

        End Sub

        <TestMethod>
        Public Sub UnModelloSenzaPrezzoContaITokenENonISoldi()

            Dim conto As ContoDoppio = ContoDelleChiamate.DalleRighe({
                Riga("2026-08-26 10:00:00", "claude-haiku-4-5", 1000000, 0),
                Riga("2026-08-26 10:05:00", "claude-domani-1", 5000000, 5000000)},
                Listino.Predefinito(), Adesso)

            Assert.AreEqual(2, conto.Tutte.Chiamate, "sono due chiamate")
            Assert.AreEqual(11000000L, conto.Tutte.TokenIngresso + conto.Tutte.TokenUscita,
                            "e i token si contano tutti")
            Assert.AreEqual(1D, conto.Tutte.Spesa, "ma i soldi solo di quella che so valutare")
            Assert.AreEqual(1, conto.Tutte.SenzaPrezzo, "e il buco si dichiara")

        End Sub

        <TestMethod>
        Public Sub UnaRigaStortaNonAzzeraLeAltre()

            ' Il file si apre in un foglio di calcolo: chiunque può averci messo dentro una
            ' riga a mano, e non deve costare il conto di tutte le altre.
            Dim conto As ContoDoppio = ContoDelleChiamate.DalleRighe({
                Intestazione,
                "questa riga non c'entra niente",
                ";;;;;",
                Riga("2026-08-26 10:00:00", "claude-haiku-4-5", 1000000, 0),
                "2026-08-26 10:01:00;confronto;claude-haiku-4-5;4000;tanti;pochi;12,5;end_turn"},
                Listino.Predefinito(), Adesso)

            Assert.AreEqual(1, conto.Tutte.Chiamate, "una sola riga era buona")
            Assert.AreEqual(1D, conto.Tutte.Spesa)

        End Sub

        <TestMethod>
        Public Sub GliUltimiTrentaGiorniSiContanoDaUnAdessoCheArrivaDaFuori()

            Dim conto As ContoDoppio = ContoDelleChiamate.DalleRighe({
                Riga("2026-08-26 10:00:00", "claude-haiku-4-5", 1000000, 0),
                Riga("2026-01-02 10:00:00", "claude-haiku-4-5", 1000000, 0)},
                Listino.Predefinito(), Adesso)

            Assert.AreEqual(2, conto.Tutte.Chiamate, "nel totale ci sono tutte")
            Assert.AreEqual(1, conto.Recenti.Chiamate, "nei trenta giorni solo quella di ieri")
            Assert.AreEqual(New Date(2026, 1, 2), conto.Tutte.DalGiorno.Value.Date, "la prima volta")

        End Sub

        <TestMethod>
        Public Sub UnaRigaSenzaDataStaNelTotaleENonNeiGiorniRecenti()

            ' Dire di sì la conterebbe di nuovo a ogni mese che passa; dire di no la lascia
            ' nel totale, dove è certamente giusta.
            Dim conto As ContoDoppio = ContoDelleChiamate.DalleRighe({
                Riga("non è una data", "claude-haiku-4-5", 1000000, 0)},
                Listino.Predefinito(), Adesso)

            Assert.AreEqual(1, conto.Tutte.Chiamate, "nel totale")
            Assert.AreEqual(0, conto.Recenti.Chiamate, "e non nei giorni recenti")
            Assert.IsFalse(conto.Tutte.DalGiorno.HasValue, "e non si inventa una prima volta")

        End Sub

    End Class

End Namespace
