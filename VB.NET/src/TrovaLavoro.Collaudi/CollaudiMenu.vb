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

        ''' <summary>
        ''' Sopra la colonna resta scoperta una fascia per il nome, su qualunque misura.
        ''' </summary>
        ''' <remarks>
        ''' <para>Nato dal difetto visto a video due volte in un pomeriggio: la colonna dei
        ''' bottoni saliva fin sopra il sottotitolo del marchio. Fino al 2026-09-01 la
        ''' misura era «più di un quarto dell'altezza», perché la fascia <b>era</b> una
        ''' frazione dell'altezza e nient'altro.</para>
        ''' <para>Dal 2026-09-01, su indicazione del tutor, la fascia segue la
        ''' <b>scritta</b>, e la scritta segue la larghezza (v.
        ''' <c>PannelloMenu.FasciaDelTesto</c>): su una finestra alta e stretta la fascia
        ''' non si prende più un terzo dell'altezza, e un quarto non è più il metro giusto.
        ''' Quel che va sorvegliato resta lo stesso — che una fascia ci sia, e che sia larga
        ''' abbastanza da tenerci un nome leggibile — e si misura adesso in pixel: sotto i
        ''' cento un nome sarebbe un francobollo, e i bottoni gli sarebbero addosso.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub SopraLaZonaRestaPostoPerNomeESottotitolo()

            Const MinimoLeggibile As Integer = 100

            For Each area As Size In New Size() {New Size(1936, 940), New Size(1134, 485),
                                                 New Size(700, 1000), New Size(3840, 2000)}

                Dim zona As Rectangle = PannelloMenu.ZonaSottoIlNome(area)

                Assert.IsGreaterThanOrEqualTo(MinimoLeggibile, zona.Top,
                    $"su {area.Width}x{area.Height} sopra i bottoni non resta scoperto abbastanza")

                Assert.IsLessThanOrEqualTo(CInt(area.Height / 3.0), zona.Top,
                    $"su {area.Width}x{area.Height} la fascia si prende più di un terzo dell'area")

            Next

        End Sub

        ''' <summary>
        ''' La scritta smette di crescere su uno schermo grande e di rimpicciolire su uno
        ''' piccolo.
        ''' </summary>
        ''' <remarks>
        ''' È la rifinitura del 2026-09-01: «proporzione sulla larghezza, con un minimo e un
        ''' massimo sensati perché non degeneri né a francobollo né a manifesto». In mezzo
        ''' ai due fermi la proporzione resta quella di sempre, e questo il collaudo lo
        ''' guarda: un fermo che valesse ovunque sarebbe una misura fissa, non una scala.
        ''' </remarks>
        <TestMethod>
        Public Sub LaScrittaSmetteDiCrescereEDiRimpicciolire()

            Dim aSchermoPieno As Integer = PannelloMenu.LarghezzaDiRiferimento(New Size(1936, 940))
            Dim suUnMuro As Integer = PannelloMenu.LarghezzaDiRiferimento(New Size(3840, 2000))

            Assert.AreEqual(aSchermoPieno, suUnMuro,
                            "oltre il tetto la scritta non cresce più: il doppio dello schermo non la raddoppia")

            Dim stretta As Integer = PannelloMenu.LarghezzaDiRiferimento(New Size(700, 1000))
            Dim strettissima As Integer = PannelloMenu.LarghezzaDiRiferimento(New Size(300, 1000))

            Assert.AreEqual(stretta, strettissima,
                            "e sotto il minimo non rimpicciolisce più")

            Dim inMezzo As Integer = PannelloMenu.LarghezzaDiRiferimento(New Size(1134, 485))

            Assert.IsGreaterThan(stretta, inMezzo, "fra i due fermi la scritta segue la larghezza")
            Assert.IsLessThan(aSchermoPieno, inMezzo, "e la segue davvero, non a scatti")

        End Sub

        ''' <summary>
        ''' La fascia del nome segue la scritta, non l'altezza della finestra.
        ''' </summary>
        ''' <remarks>
        ''' Il difetto che il tutor ha indicato il 2026-09-01: legata alla sola altezza, la
        ''' fascia si allargava a ogni pixel di finestra in più e il nome ci cresceva dentro
        ''' fino a riempirla — un manifesto sopra sei bottoni che di crescere si erano già
        ''' fermati. Qui la stessa larghezza si prova a due altezze molto diverse: se la
        ''' fascia tornasse a dipendere dall'altezza, i due numeri divergerebbero.
        ''' </remarks>
        <TestMethod>
        Public Sub LaFasciaDelNomeSegueLaScrittaENonLAltezza()

            Dim bassa As Integer = PannelloMenu.FasciaDelTesto(New Size(1400, 1600))
            Dim alta As Integer = PannelloMenu.FasciaDelTesto(New Size(1400, 2400))

            Assert.AreEqual(bassa, alta,
                            "a parità di larghezza la fascia è la stessa, per alta che sia la finestra")

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
        ''' A nessuna misura la colonna dei bottoni pesta il nome del prodotto.
        ''' </summary>
        ''' <remarks>
        ''' <para>Difetto vero, visto dal tutor su una finestra di 1136×593 il 2026-09-01:
        ''' «TrovaLavoro» spuntava da dietro le prime due voci e il sottotitolo tagliava in
        ''' mezzo ai bottoni. Il centraggio era giusto; era sbagliata la <b>guardia</b> che
        ''' tratteneva la colonna, che guardava il bordo del pannello invece della fascia
        ''' del nome, e il rialzo dell'occhio la faceva salire fin lì.</para>
        ''' <para>Si misura la proprietà e non il conto: <b>la cima del primo bottone non
        ''' sale sopra la fine della fascia</b>, a qualunque misura, compresa una in cui la
        ''' colonna non ci sta nemmeno stringendosi ai minimi. Il difetto è di quelli che si
        ''' vedono solo guardando, e solo su una misura di finestra che nessuno prova per
        ''' caso: l'applicazione si apriva massimizzata.</para>
        ''' <para>L'ingombro del logo si dichiara come lo dichiarerebbe la finestra vera,
        ''' perché è uno dei due termini da cui dipende quanto spazio ha la colonna.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub ANessunaMisuraLaColonnaPestaIlNome()

            For Each area As Size In New Size() {New Size(1936, 940), New Size(1134, 700),
                                                 New Size(1134, 513), New Size(1120, 506),
                                                 New Size(1000, 300)}

                Using menu As New PannelloMenu()

                    menu.Size = area
                    menu.ImpostaIngombroLogo(New Size(130, 96))

                    Dim bottoni As BottoneMenu() =
                        menu.Controls.OfType(Of BottoneMenu)().OrderBy(Function(b) b.Top).ToArray()

                    Dim fascia As Integer = PannelloMenu.FasciaDelTesto(menu.ClientSize)

                    Assert.IsGreaterThanOrEqualTo(fascia, bottoni.First().Top,
                        $"su {area.Width}x{area.Height} il primo bottone entra nella fascia del nome")

                End Using

            Next

        End Sub

        ''' <summary>
        ''' Dalla misura minima della finestra in su, l'ultima voce non esce dal bordo.
        ''' </summary>
        ''' <remarks>
        ''' L'altra metà della stessa fotografia del tutor: in fondo alla finestra restava
        ''' una striscia di pixel tagliati. Sotto la misura minima — che la finestra vera non
        ''' raggiunge, perché il suo <c>MinimumSize</c> è 1150×600 — la colonna esce
        ''' comunque in basso, ed è una scelta: fra uscire dove non c'è niente e salire sul
        ''' nome, esce in basso.
        ''' </remarks>
        <TestMethod>
        Public Sub DallaMisuraMinimaInSuLUltimaVoceRestaDentro()

            For Each area As Size In New Size() {New Size(1936, 940), New Size(1134, 700),
                                                 New Size(1134, 513), New Size(1120, 506)}

                Using menu As New PannelloMenu()

                    menu.Size = area
                    menu.ImpostaIngombroLogo(New Size(130, 96))

                    Dim bottoni As BottoneMenu() =
                        menu.Controls.OfType(Of BottoneMenu)().OrderBy(Function(b) b.Top).ToArray()

                    Assert.IsLessThanOrEqualTo(menu.ClientSize.Height, bottoni.Last().Bottom,
                        $"su {area.Width}x{area.Height} l'ultima voce esce dal bordo di sotto")

                End Using

            Next

        End Sub

        ''' <summary>
        ''' La colonna sta sopra il centro della zona, di quanto il progetto ha deciso.
        ''' </summary>
        ''' <remarks>
        ''' <para>È una correzione dell'occhio, e per questo va sorvegliata: un numero deciso
        ''' guardando non ha nessuno che lo difenda: se domani qualcuno rimette la colonna
        ''' al centro geometrico — che è la cosa che verrebbe naturale scrivere — non
        ''' sbaglia nessun conto, e a vederlo è appena un po' peggio di prima. Le due volte
        ''' in cui questa geometria è stata sbagliata, nel pomeriggio in cui è nata, se n'è
        ''' accorto solo l'occhio guardando una fotografia.</para>
        ''' <para>Si misura a schermo pieno e non più su 1134×700, e la ragione è la
        ''' rifinitura del 2026-09-01: il rialzo <b>cede il passo</b> alla regola «la colonna
        ''' non entra nella fascia del nome», quindi dove lo spazio è poco non si vede più.
        ''' Sorvegliarlo su una finestra stretta vorrebbe dire pretendere indietro proprio
        ''' quello che si è tolto.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub LaColonnaStaSopraIlCentroDellaZona()

            Using menu As New PannelloMenu()

                menu.Size = New Size(1936, 940)

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
