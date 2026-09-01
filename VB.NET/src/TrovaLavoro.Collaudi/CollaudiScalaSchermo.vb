Imports System.Drawing
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi delle misure dell'interfaccia rispetto al DPI (cap. 03.4, decisione 15.7).
    ''' </summary>
    ''' <remarks>
    ''' <para>Questi collaudi girano a 96 DPI come tutto il banco, ed è il punto: le
    ''' decisioni che a 144 DPI sbagliavano vivono in funzioni pure a cui il DPI si
    ''' <b>passa</b>, così il banco può chiedere loro cosa succederebbe su uno schermo che
    ''' non ha. Prima di T9e non esisteva niente del genere in tutto il progetto, e infatti
    ''' 995 collaudi verdi non vedevano tre difetti che un collaudo dal vivo ha trovato in
    ''' un pomeriggio.</para>
    ''' <para>I numeri non sono inventati: 1920×144 DPI è la finestra massimizzata della
    ''' macchina di collaudo al 150%, 1384 su 1008 è il contenuto delle Impostazioni contro
    ''' l'area di lavoro, 1150×600 è il minimo dichiarato dal progetto (cap. 14, giro B).</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiScalaSchermo

        <TestMethod>
        Public Sub A96DpiNonCeNienteDaConvertire()

            Assert.AreEqual(261, ScalaSchermo.InPixelDelloSchermo(261, 96),
                            "il DPI di progetto è il metro stesso: non si tocca niente")
            Assert.AreEqual(1920, ScalaSchermo.InUnitaDiProgetto(1920, 96),
                            "e vale in tutt'e due i versi")

        End Sub

        <TestMethod>
        Public Sub A144DpiUnaMisuraDiProgettoOccupaMetaSpazioInPiu()

            Assert.AreEqual(1725, ScalaSchermo.InPixelDelloSchermo(1150, 144),
                            "il minimo di progetto in larghezza, in pixel veri")
            Assert.AreEqual(900, ScalaSchermo.InPixelDelloSchermo(600, 144),
                            "e in altezza, con lo stesso rapporto: è il punto di R12")

        End Sub

        <TestMethod>
        Public Sub A144DpiUnaMisuraDelloSchermoTornaAlMetroDelProgetto()

            Assert.AreEqual(1280, ScalaSchermo.InUnitaDiProgetto(1920, 144),
                            "la finestra massimizzata a 150% vale 1280 unità di progetto")

        End Sub

        <TestMethod>
        Public Sub LaCompattaScattaSulloSchermoScalato()

            ' Il difetto R10a in una riga: gli stessi 1920 pixel di schermo, letti col
            ' metro giusto, stanno sotto la soglia; letti crudi stanno sopra.
            Assert.IsTrue(ScalaSchermo.ModalitaCompatta(1920, 144, 1350),
                          "a 144 DPI la finestra massimizzata vale 1280: la compatta deve scattare")
            Assert.IsFalse(ScalaSchermo.ModalitaCompatta(1920, 96, 1350),
                           "a 96 DPI gli stessi pixel sono spazio vero: la compatta non serve")

        End Sub

        <TestMethod>
        Public Sub SottoLaSogliaLaCompattaScattaAQualunqueDpi()

            Assert.IsTrue(ScalaSchermo.ModalitaCompatta(1200, 96, 1350),
                          "una finestra stretta è stretta anche senza scalatura")
            Assert.IsTrue(ScalaSchermo.ModalitaCompatta(1633, 144, 1350),
                          "il minimo vero misurato a 150% vale 1089: ben sotto la soglia")

        End Sub

        <TestMethod>
        Public Sub IlTettoFermaLaFinestraDentroLoSpazioCheCe()

            ' Il difetto R11: contenuto fino a 1384 su un'area di lavoro alta 1008.
            Assert.AreEqual(1008, ScalaSchermo.AltezzaSostenibile(1384, 1008),
                            "la finestra si ferma dove finisce lo schermo")
            Assert.IsTrue(ScalaSchermo.ServeScorrimento(1384, 1008),
                          "e quel che è rimasto fuori si raggiunge scorrendo, non sparisce")

        End Sub

        <TestMethod>
        Public Sub QuandoIlContenutoCiStaNessunoTagliaNienteENonSiScorre()

            Assert.AreEqual(760, ScalaSchermo.AltezzaSostenibile(760, 1008),
                            "l'altezza voluta passa intera")
            Assert.IsFalse(ScalaSchermo.ServeScorrimento(760, 1008),
                           "e una barra di scorrimento inutile non compare")

        End Sub

        <TestMethod>
        Public Sub LaCorniceDiUnaFinestraNonEAreaCliente()

            Assert.AreEqual(969, ScalaSchermo.SpazioClienteDisponibile(1008, 39),
                            "l'area di lavoro meno titolo e bordi: è quel che resta al contenuto")
            Assert.AreEqual(0, ScalaSchermo.SpazioClienteDisponibile(100, 200),
                            "una cornice più alta dello schermo non fa spazio negativo")

        End Sub

        <TestMethod>
        Public Sub SenzaSpazioNotoNonSiInventaUnTetto()

            Assert.AreEqual(760, ScalaSchermo.AltezzaSostenibile(760, 0),
                            "meglio una finestra grande che una finestra alta zero")
            Assert.IsFalse(ScalaSchermo.ServeScorrimento(760, 0),
                           "e senza tetto non c'è niente da scorrere")

        End Sub

        <TestMethod>
        Public Sub UnDpiNonValidoNonFaSaltareIlConto()

            Assert.AreEqual(261, ScalaSchermo.InPixelDelloSchermo(261, 0),
                            "un DPI di zero non divide niente: si lascia la misura com'è")
            Assert.AreEqual(1920, ScalaSchermo.InUnitaDiProgetto(1920, -1),
                            "idem nell'altro verso")

        End Sub

        <TestMethod>
        Public Sub QuandoSiScorreLaBarraSiPrendeLaSuaLarghezza()

            ' 660 è la larghezza di progetto della finestra delle Impostazioni, 17 la barra
            ' a 96 DPI: i 14 pixel di margine del disegno non bastavano a coprirla, e il
            ' contenuto le finiva sotto.
            Assert.AreEqual(643, ScalaSchermo.LarghezzaSenzaLaBarra(660, siScorre:=True, larghezzaBarra:=17))

        End Sub

        <TestMethod>
        Public Sub SenzaScorrimentoLaLarghezzaResta()

            Assert.AreEqual(660, ScalaSchermo.LarghezzaSenzaLaBarra(660, siScorre:=False, larghezzaBarra:=17),
                            "una barra che non c'è non toglie niente")

        End Sub

        ''' <summary>
        ''' Su uno schermo grande la finestra si apre al suo tetto, non a tutto schermo.
        ''' </summary>
        ''' <remarks>
        ''' La regola d'apertura del 2026-09-01, dettata dal tutor: l'applicazione non parte
        ''' più massimizzata. Su un 4K a scala 100% il tetto è meno di metà schermo, e si
        ''' vede a colpo d'occhio che la finestra è una finestra.
        ''' </remarks>
        <TestMethod>
        Public Sub SuUnoSchermoGrandeLaFinestraSiApreAlSuoTetto()

            Dim misura As Size = ScalaSchermo.MisuraDiApertura(
                New Size(3840, 2120), New Size(1150, 600), dpi:=96)

            Assert.AreEqual(ScalaSchermo.TettoDiApertura, misura,
                            "su un 4K si apre al tetto e non a tutto schermo")

        End Sub

        ''' <summary>Su uno schermo che il tetto non lo contiene, prende quel che c'è.</summary>
        <TestMethod>
        Public Sub SuUnoSchermoPiccoloLaFinestraPrendeLAreaDiLavoro()

            Dim misura As Size = ScalaSchermo.MisuraDiApertura(
                New Size(1366, 728), New Size(1150, 600), dpi:=96)

            Assert.AreEqual(New Size(1366, 728), misura,
                            "si adatta invece di sbordare")

        End Sub

        ''' <summary>E sotto il minimo della finestra non scende comunque.</summary>
        ''' <remarks>
        ''' Uno schermo più piccolo del minimo esiste (un netbook, una macchina virtuale
        ''' stretta), e lì la finestra sborda: è già la scelta del <c>MinimumSize</c>, e
        ''' farla decidere due volte vorrebbe dire due risposte diverse alla stessa domanda.
        ''' </remarks>
        <TestMethod>
        Public Sub SottoIlMinimoLaFinestraNonScende()

            Dim misura As Size = ScalaSchermo.MisuraDiApertura(
                New Size(1024, 500), New Size(1150, 600), dpi:=96)

            Assert.AreEqual(New Size(1150, 600), misura, "vince il minimo dichiarato")

        End Sub

        ''' <summary>
        ''' A 150% il tetto vale una volta e mezza, perché è una misura di progetto.
        ''' </summary>
        ''' <remarks>
        ''' È l'interpretazione dichiarata: 1920×1024 sono unità di progetto come il minimo
        ''' 1150×600, non pixel veri. Trattarli da pixel veri aprirebbe, su uno schermo
        ''' grande a scala alta, una finestra che a video mostra 1280 unità di progetto —
        ''' poco più del minimo, e proprio sulle macchine con lo schermo più grande.
        ''' </remarks>
        <TestMethod>
        Public Sub A144DpiIlTettoDiAperturaValeUnaVoltaEMezza()

            Dim misura As Size = ScalaSchermo.MisuraDiApertura(
                New Size(5120, 2800), New Size(1725, 900), dpi:=144)

            Assert.AreEqual(New Size(2880, 1536), misura,
                            "il tetto convertito, non i 1920x1024 di schermo")

        End Sub

        ''' <summary>Senza uno schermo noto passa il tetto, non una misura nulla.</summary>
        <TestMethod>
        Public Sub SenzaUnoSchermoLaFinestraSiApreAlTetto()

            Assert.AreEqual(ScalaSchermo.TettoDiApertura,
                            ScalaSchermo.MisuraDiApertura(Size.Empty, New Size(1150, 600), dpi:=96),
                            "una finestra di misura zero non è un ripiego, è un guasto")

        End Sub

        <TestMethod>
        Public Sub UnaBarraDiLarghezzaIgnotaNonStringeIlContenuto()

            ' Meglio un contenuto largo che un contenuto sparito: è la stessa scelta di
            ' AltezzaSostenibile davanti a uno spazio non noto.
            Assert.AreEqual(660, ScalaSchermo.LarghezzaSenzaLaBarra(660, siScorre:=True, larghezzaBarra:=0))
            Assert.AreEqual(660, ScalaSchermo.LarghezzaSenzaLaBarra(660, siScorre:=True, larghezzaBarra:=-8))

        End Sub

    End Class

End Namespace
