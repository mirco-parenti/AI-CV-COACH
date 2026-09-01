Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi del menu d'ingresso (P0): che la fascia del nome lasci scoperta la cima,
    ''' che la colonna dei bottoni stia dove il progetto l'ha messa, che ognuna delle sei
    ''' voci abbia il suo bottone, e che il menu si chiuda insieme alla barra quando l'AI
    ''' lavora.
    ''' </summary>
    ''' <remarks>
    ''' Il pannello si costruisce e non si mostra, come gli altri collaudi di interfaccia:
    ''' quel che si guarda è la geometria e il filo fra bottone ed evento, e nessuno dei
    ''' due ha bisogno di uno schermo. La geometria dello sfondo è addirittura
    ''' <c>Shared</c> apposta: si interroga senza costruire niente.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiMenu

        ' ==================================================================
        ' La fascia del nome, e lo spazio che resta sotto
        ' ==================================================================

        <TestMethod>
        Public Sub SopraLaZonaRestaPostoPerNomeESottotitolo()

            ' Il difetto visto a video, due volte in un pomeriggio: la colonna dei bottoni
            ' saliva fin sopra il sottotitolo del marchio. Sul banner nome e sottotitolo
            ' finivano a 356 px su 1348, poco oltre un quarto dell'altezza; qui la fascia
            ' deve tenerne almeno altrettanto, perché ci sta dentro anche il respiro che
            ' stacca il sottotitolo dal primo bottone.
            For Each area As Size In New Size() {New Size(1936, 940), New Size(1134, 485), New Size(700, 1000)}

                Dim zona As Rectangle = PannelloMenu.ZonaSottoIlNome(area)
                Dim scoperto As Double = zona.Top / CDbl(area.Height)

                Assert.IsGreaterThanOrEqualTo(0.26, scoperto,
                                              $"su {area.Width}x{area.Height} sopra i bottoni resta scoperto più di un quarto")

            Next

        End Sub

        <TestMethod>
        Public Sub LaZonaPrendeTuttoQuelCheLaFasciaLasciaSenzaBuchi()

            Dim area As New Size(1134, 485)

            Dim fascia As Integer = PannelloMenu.FasciaDelTesto(area)
            Dim zona As Rectangle = PannelloMenu.ZonaSottoIlNome(area)

            Assert.AreEqual(fascia, zona.Top, "la zona comincia dove la fascia finisce")
            Assert.AreEqual(area.Height, zona.Bottom, "e arriva in fondo: fra le due non resta un buco")
            Assert.AreEqual(area.Width, zona.Width, "in larghezza prende tutto")
            Assert.AreEqual(0, zona.Left, "da bordo a bordo")

        End Sub

        <TestMethod>
        Public Sub LaZonaSegueLaFinestraQuandoCambia()

            For Each area As Size In New Size() {New Size(1936, 940), New Size(1134, 485), New Size(700, 1000)}

                Dim zona As Rectangle = PannelloMenu.ZonaSottoIlNome(area)

                Assert.IsGreaterThan(0, zona.Width, $"su {area.Width}x{area.Height} la zona non si annulla")
                Assert.IsGreaterThan(0, zona.Height, "né in altezza")
                Assert.IsLessThanOrEqualTo(area.Height, zona.Bottom, "e non sborda in fondo")

            Next

        End Sub

        ' ==================================================================
        ' Il marchio, che qui dietro non c'è più
        ' ==================================================================

        ''' <summary>
        ''' Lo scudo sta davvero dove <see cref="LogoAviolab.ScudoDentroLaTela"/> dice.
        ''' </summary>
        ''' <remarks>
        ''' Quei quattro numeri dicono dove sta lo scudo dentro la tela del PNG, e sono
        ''' misurati una volta sola: questa è la guardia che li tiene onesti, perché se un
        ''' domani il marchio cambiasse disegno e lo scudo si spostasse, nessun conto fatto
        ''' su di loro tornerebbe più. Si rilegge quindi il PNG e si guarda dove stanno i
        ''' pixel che non sono trasparenti.
        ''' <para>Dal 2026-09-01 li legge <b>solo questo collaudo</b>: il mega stemma di
        ''' sfondo è stato tolto e con lui l'unico posto che se ne serviva. La guardia resta
        ''' com'è rimasto il banner (v. <see cref="IlBannerEAncoraIncorporato"/>): finché
        ''' quei numeri stanno in <c>LogoAviolab</c>, tanto vale che siano veri.</para>
        ''' <para>Con quattro dita di tolleranza, e non per pigrizia: attorno al disegno
        ''' il PNG si porta un alone di alfa quasi nulla, e <c>Genera</c> lo ridisegna con
        ''' l'interpolazione accesa. Contando solo i pixel davvero opachi il bordo cade a
        ''' 30 invece che a 28 — due pixel che a video non esistono. Con una tolleranza di
        ''' due il collaudo passerebbe esattamente al limite, cioè sarebbe rosso al primo
        ''' pixel che GDI+ tratta un filo diversamente; con quattro resta capace di vedere
        ''' uno spostamento vero (venti pixel lo fanno cadere) senza essere ballerino.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub LoScudoStaDoveLaCostanteDice()

            Const Tolleranza As Integer = 4

            Using stemma As Bitmap = LogoAviolab.Genera(LogoAviolab.LatoDellaTela)

                Dim sinistra As Integer = Integer.MaxValue
                Dim destra As Integer = -1
                Dim cima As Integer = Integer.MaxValue
                Dim fondo As Integer = -1

                For y As Integer = 0 To stemma.Height - 1
                    For x As Integer = 0 To stemma.Width - 1
                        If stemma.GetPixel(x, y).A > 8 Then
                            If x < sinistra Then sinistra = x
                            If x > destra Then destra = x
                            If y < cima Then cima = y
                            If y > fondo Then fondo = y
                        End If
                    Next
                Next

                Dim dichiarato As Rectangle = LogoAviolab.ScudoDentroLaTela

                Assert.AreEqual(dichiarato.Left, sinistra, Tolleranza, "lo scudo comincia dove è scritto")
                Assert.AreEqual(dichiarato.Right - 1, destra, Tolleranza, "e finisce dove è scritto")
                Assert.AreEqual(dichiarato.Top, cima, Tolleranza, "in cima pure")
                Assert.AreEqual(dichiarato.Bottom - 1, fondo, Tolleranza, "e in fondo")

            End Using

        End Sub

        <TestMethod>
        Public Sub UnaMisuraImpossibileNonFaCadereNiente()

            Assert.AreEqual(Rectangle.Empty, PannelloMenu.ZonaSottoIlNome(Size.Empty),
                            "un'area di misura zero non si riempie: non è un errore, non c'è spazio")

            Assert.AreEqual(0, PannelloMenu.FasciaDelTesto(Size.Empty),
                            "né una fascia del nome")

            Assert.AreEqual(0, PannelloMenu.FineDelNome(Size.Empty),
                            "e dentro una fascia che non c'è, il nome non finisce da nessuna parte")

        End Sub

        ''' <summary>
        ''' Il banner è ancora dentro l'eseguibile, anche se il menu non lo usa più.
        ''' </summary>
        ''' <remarks>
        ''' Dal 2026-08-30 (sera) lo sfondo del menu si dipinge, e questa risorsa non ha
        ''' più nessun lettore nel prodotto: resta incorporata finché non si decide se
        ''' toglierla del tutto (sono 825 KB dentro l'exe). Fino ad allora il collaudo
        ''' dice almeno che è integra — un giorno che si decida di riusarla, non la si
        ''' troverà rotta.
        ''' </remarks>
        <TestMethod>
        Public Sub IlBannerEAncoraIncorporato()

            Dim sfondo As Image = Marchio.SfondoDelMenu

            Assert.IsNotNull(sfondo, "il banner è incorporato")
            Assert.AreEqual(New Size(1536, 1348), sfondo.Size, "e misura quel che ha sempre misurato")

        End Sub

        ''' <summary>
        ''' La colonna sta sopra il centro della zona, di quanto il progetto ha deciso.
        ''' </summary>
        ''' <remarks>
        ''' È una correzione dell'occhio, e per questo va sorvegliata: un numero deciso
        ''' guardando non ha nessuno che lo difenda: se domani qualcuno rimette la colonna
        ''' al centro geometrico — che è la cosa che verrebbe naturale scrivere — non
        ''' sbaglia nessun conto, e a vederlo è appena un po' peggio di prima. Le due volte
        ''' in cui questa geometria è stata sbagliata, nel pomeriggio in cui è nata, se n'è
        ''' accorto solo l'occhio guardando una fotografia.
        ''' </remarks>
        <TestMethod>
        Public Sub LaColonnaStaSopraIlCentroDellaZona()

            Using menu As New PannelloMenu()

                menu.Size = New Size(1134, 700)

                Dim bottoni As BottoneMenu() =
                    menu.Controls.OfType(Of BottoneMenu)().OrderBy(Function(b) b.Top).ToArray()

                Dim primo As BottoneMenu = bottoni.First()
                Dim ultimo As BottoneMenu = bottoni.Last()

                Dim zona As Rectangle = PannelloMenu.ZonaSottoIlNome(menu.ClientSize)

                Dim centroColonna As Double = (primo.Top + ultimo.Bottom) / 2.0
                Dim centroZona As Double = zona.Top + zona.Height / 2.0

                ' Dichiarato qui e non preso dalla costante del pannello: un metro
                ' copiato dalla cosa da misurare non può dire che è sbagliata.
                Const RialzoAtteso As Double = 1.75

                Assert.AreEqual(primo.Height * RialzoAtteso, centroZona - centroColonna, 2.0,
                                "la colonna sta un bottone e tre quarti più in alto del centro")

            End Using

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
