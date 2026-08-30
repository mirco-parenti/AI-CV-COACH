Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi del menu d'ingresso (P0): che lo sfondo copra l'area senza deformarsi,
    ''' che ognuna delle sei voci abbia il suo bottone, e che il menu si chiuda insieme
    ''' alla barra quando l'AI lavora.
    ''' </summary>
    ''' <remarks>
    ''' Il pannello si costruisce e non si mostra, come gli altri collaudi di interfaccia:
    ''' quel che si guarda è la geometria e il filo fra bottone ed evento, e nessuno dei
    ''' due ha bisogno di uno schermo. Il riempimento dello sfondo è addirittura
    ''' <c>Shared</c> apposta: si interroga senza costruire niente.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiMenu

        ' ==================================================================
        ' Lo sfondo che riempie
        ' ==================================================================

        <TestMethod>
        Public Sub LoSfondoSiVedeIntero()

            ' Il caso vero: il banner è quasi quadrato, l'area è panoramica.
            Dim riquadro As Rectangle =
                PannelloMenu.RiquadroDelloSfondo(New Size(1536, 1348), New Size(1134, 485))

            Assert.IsGreaterThanOrEqualTo(0, riquadro.X, "non sborda a sinistra")
            Assert.IsGreaterThanOrEqualTo(0, riquadro.Y, "né in cima")
            Assert.IsLessThanOrEqualTo(1134, riquadro.Right, "né a destra")
            Assert.IsLessThanOrEqualTo(485, riquadro.Bottom, "né in fondo: si vede tutto il marchio")

        End Sub

        <TestMethod>
        Public Sub LoSfondoNonSiSchiaccia()

            Dim immagine As New Size(1536, 1348)
            Dim riquadro As Rectangle = PannelloMenu.RiquadroDelloSfondo(immagine, New Size(1134, 485))

            Dim originale As Double = immagine.Width / CDbl(immagine.Height)
            Dim disegnata As Double = riquadro.Width / CDbl(riquadro.Height)

            Assert.AreEqual(originale, disegnata, 0.01,
                            "le proporzioni non si toccano: il disegno non si allunga")

        End Sub

        <TestMethod>
        Public Sub LoSfondoRestaCentrato()

            Dim riquadro As Rectangle =
                PannelloMenu.RiquadroDelloSfondo(New Size(1536, 1348), New Size(1134, 485))

            Assert.AreEqual(riquadro.X, 1134 - riquadro.Right, 1,
                            "in orizzontale l'aria che avanza si divide in parti uguali")
            Assert.AreEqual(riquadro.Y, 485 - riquadro.Bottom, 1,
                            "e in verticale pure")

        End Sub

        <TestMethod>
        Public Sub LoSfondoCrescePerQuantoPuo()

            ' Starci dentro non basta: un'immagine grande la metà del necessario ci sta
            ' anche lei, e sarebbe un francobollo in mezzo al blu. Il lato che le sta più
            ' stretto va toccato.
            Dim riquadro As Rectangle =
                PannelloMenu.RiquadroDelloSfondo(New Size(1536, 1348), New Size(1134, 485))

            Assert.IsGreaterThanOrEqualTo(484, riquadro.Height,
                                          "l'altezza è il lato stretto: si arriva a filo")

            ' E su un'area alta e magra tocca invece la larghezza.
            Dim magra As Rectangle =
                PannelloMenu.RiquadroDelloSfondo(New Size(1536, 1348), New Size(400, 900))

            Assert.IsGreaterThanOrEqualTo(399, magra.Width, "qui il lato stretto è la larghezza")
            Assert.IsLessThanOrEqualTo(900, magra.Bottom, "e in altezza ne avanza")

        End Sub

        <TestMethod>
        Public Sub UnaMisuraImpossibileNonFaCadereNiente()

            Assert.AreEqual(Rectangle.Empty,
                            PannelloMenu.RiquadroDelloSfondo(New Size(1536, 1348), Size.Empty),
                            "un'area di misura zero non si riempie: non è un errore, non c'è spazio")

            Dim senzaImmagine As Rectangle =
                PannelloMenu.RiquadroDelloSfondo(Size.Empty, New Size(800, 600))
            Assert.AreEqual(New Size(800, 600), senzaImmagine.Size,
                            "senza immagine non si divide per zero")

        End Sub

        <TestMethod>
        Public Sub LoSfondoEDentroLEseguibileEMisuraQuelCheIlCodiceCrede()

            Dim sfondo As Image = Marchio.SfondoDelMenu

            Assert.IsNotNull(sfondo, "lo sfondo del menu è incorporato")

            ' La geometria del menu non chiede la misura all'immagine — leggerla è una
            ' chiamata a GDI+ su un oggetto condiviso, e in parallelo esplode — ma la
            ' tiene in una costante. Questa è la guardia che tiene onesta la costante: se
            ' il banner cambiasse misura senza che nessuno aggiorni MisuraDelMaster, i
            ' bottoni si centrerebbero su un riquadro che non esiste più.
            Assert.AreEqual(PannelloMenu.MisuraDelMaster, sfondo.Size,
                            "e misura quel che PannelloMenu.MisuraDelMaster dichiara")

        End Sub

        ' ==================================================================
        ' Lo spazio dentro la cornice, dove vanno i bottoni
        ' ==================================================================

        <TestMethod>
        Public Sub LaZonaDeiBottoniStaDentroLoSfondo()

            Dim immagine As New Size(1536, 1348)
            Dim area As New Size(1936, 940)

            Dim sfondo As Rectangle = PannelloMenu.RiquadroDelloSfondo(immagine, area)
            Dim zona As Rectangle = PannelloMenu.ZonaDentroLaCornice(immagine, area)

            Assert.IsTrue(sfondo.Contains(zona),
                          "lo spazio dei bottoni è dentro l'immagine, non fuori")

        End Sub

        <TestMethod>
        Public Sub SopraLaZonaRestaPostoPerNomeESottotitolo()

            ' Il difetto visto a video, due volte in un pomeriggio: la colonna dei bottoni
            ' saliva fin sopra il sottotitolo del marchio. Sopra il filetto giallo il
            ' banner tiene nome e sottotitolo, e quella parte deve restare scoperta: nel
            ' master finisce a 356 px su 1348, cioè poco oltre un quarto dell'altezza.
            Dim immagine As New Size(1536, 1348)
            Dim area As New Size(1936, 940)

            Dim sfondo As Rectangle = PannelloMenu.RiquadroDelloSfondo(immagine, area)
            Dim zona As Rectangle = PannelloMenu.ZonaDentroLaCornice(immagine, area)

            Dim scopertoSopra As Double = (zona.Top - sfondo.Top) / CDbl(sfondo.Height)

            Assert.IsGreaterThanOrEqualTo(0.26, scopertoSopra,
                                          "sopra i bottoni resta scoperto più di un quarto del banner")

        End Sub

        <TestMethod>
        Public Sub LaZonaSegueLoSfondoQuandoLaFinestraCambia()

            Dim immagine As New Size(1536, 1348)

            For Each area As Size In New Size() {New Size(1936, 940), New Size(1134, 485), New Size(700, 1000)}

                Dim sfondo As Rectangle = PannelloMenu.RiquadroDelloSfondo(immagine, area)
                Dim zona As Rectangle = PannelloMenu.ZonaDentroLaCornice(immagine, area)

                Assert.IsTrue(sfondo.Contains(zona),
                              $"su {area.Width}x{area.Height} la zona resta dentro l'immagine")
                Assert.IsGreaterThan(0, zona.Width, "e non si annulla")
                Assert.IsGreaterThan(0, zona.Height, "né in altezza")

            Next

        End Sub

        ' ==================================================================
        ' Le sei voci
        ' ==================================================================

        ''' <summary>
        ''' A misura piena ogni etichetta ci sta: nessuna voce finisce coi puntini.
        ''' </summary>
        ''' <remarks>
        ''' <c>BottoneMenu</c> scrive il testo su una riga sola e lo taglia con
        ''' <c>StringTrimming.EllipsisCharacter</c> quando non ci sta: è la scelta giusta
        ''' per una finestra stretta — meglio «Confronto ANNU…» che una parola a capo in
        ''' mezzo a un bottone a pillola — ma vuol dire che un nome troppo lungo **non
        ''' rompe niente**, si accorcia da solo e nessuno se ne accorge fino a quando non
        ''' lo si guarda. Qui si misura alla larghezza piena, che è quella con cui il menu
        ''' si apre: se un'etichetta non ci sta nemmeno lì, è troppo lunga e basta.
        ''' Si misura con <c>Graphics.MeasureString</c> e non con <c>TextRenderer</c>
        ''' perché a scrivere è <c>Graphics.DrawString</c>, e i due non danno lo stesso
        ''' numero: misurare con l'attrezzo dell'altro sarebbe un collaudo verde per
        ''' il motivo sbagliato.
        ''' </remarks>
        <TestMethod>
        Public Sub OgniVoceCiStaNelSuoBottoneAMisuraPiena()

            Using menu As New PannelloMenu()

                menu.Size = New Size(1134, 513)

                Using tela As New Bitmap(1, 1),
                      g As Graphics = Graphics.FromImage(tela),
                      formato As New StringFormat(StringFormatFlags.NoWrap)

                    For Each bottone As BottoneMenu In menu.Controls.OfType(Of BottoneMenu)()

                        Dim serve As Single = g.MeasureString(
                            bottone.Text, bottone.Font, Integer.MaxValue, formato).Width

                        Assert.IsLessThanOrEqualTo(
                            CSng(bottone.Width), serve,
                            $"«{bottone.Text}» non ci sta: servono {serve:F0} px su {bottone.Width}")

                    Next

                End Using

            End Using

        End Sub

        <TestMethod>
        Public Sub OgniVoceHaIlSuoBottoneEOgniBottoneLaSuaVoce()

            Using menu As New PannelloMenu()

                Dim voci As VoceDelMenu() =
                    [Enum].GetValues(GetType(VoceDelMenu)).Cast(Of VoceDelMenu)().ToArray()

                Dim bottoni As Integer =
                    menu.Controls.OfType(Of BottoneMenu)().Count()

                Assert.HasCount(bottoni, voci,
                                "sei voci e sei bottoni: una voce senza bottone è una porta murata")

                ' Spegnere una voce deve spegnere un bottone, e uno solo: è il modo di
                ' verificare che il legame voce → bottone copra tutte e sei senza
                ' sovrapporsi. Un ramo dimenticato nel Select Case tornerebbe Nothing e
                ' lascerebbe acceso il suo bottone.
                For Each voce As VoceDelMenu In voci

                    For Each bottone As BottoneMenu In menu.Controls.OfType(Of BottoneMenu)()
                        bottone.Enabled = True
                    Next

                    menu.ImpostaStato(voce, False)

                    Assert.HasCount(1, menu.Controls.OfType(Of BottoneMenu)().Where(Function(b) Not b.Enabled).ToArray(),
                                    $"la voce {voce} spegne un bottone, e uno solo")

                Next

            End Using

        End Sub

        <TestMethod>
        Public Sub PremereUnBottoneDiceQualeVoce()

            Using menu As New PannelloMenu()

                Dim scelte As New List(Of VoceDelMenu)
                AddHandler menu.VoceScelta, Sub(mittente, e) scelte.Add(e.Voce)

                For Each bottone As BottoneMenu In menu.Controls.OfType(Of BottoneMenu)()
                    bottone.PerformClick()
                Next

                Assert.HasCount(6, scelte, "sei bottoni, sei annunci")
                CollectionAssert.AreEquivalent(
                    [Enum].GetValues(GetType(VoceDelMenu)).Cast(Of VoceDelMenu)().ToArray(),
                    scelte,
                    "e ogni voce viene annunciata una volta sola: nessun bottone parla per un altro")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnaVoceSenzaStatoNonCambiaNiente()

            Using menu As New PannelloMenu()

                Dim primo As BottoneMenu = menu.Controls.OfType(Of BottoneMenu)().First()
                primo.Enabled = False

                menu.ImpostaStato(VoceDelMenu.Candidature, Nothing)

                Assert.IsFalse(primo.Enabled,
                               "chi non ha uno stato da dare non ne cambia nessuno")

            End Using

        End Sub

    End Class

End Namespace
