Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi della fascia dei comandi (cap. 03.4). L'invariante è uno solo e vale a
    ''' qualunque larghezza: <b>due comandi non si sovrappongono mai</b>. Fino alla 0.3.018
    ''' ogni pannello si disponeva la sua fascia per conto suo, con una fila da sinistra e
    ''' una da destra che si ignoravano, e alla larghezza minima della finestra i bottoni
    ''' finivano uno sopra l'altro — in P2 fino a 676 px. Non si vedeva perché
    ''' l'applicazione si apre massimizzata.
    ''' </summary>
    <TestClass>
    Public Class CollaudiFasciaDeiComandi

        ''' <summary>Le larghezze che contano: la minima della finestra, il salto del logo compatto, e larga.</summary>
        Private Shared ReadOnly Larghezze As Integer() = {1106, 1150, 1250, 1350, 1600, 1920}

        <TestMethod>
        Public Sub ConSpazioDaVendereIComandiStannoSuUnaRigaSola()
            ' Il caso di sempre — l'applicazione massimizzata — non deve cambiare: i
            ' comandi del pannello a sinistra, quelli che portano altrove a destra, tutti
            ' sulla stessa riga in fondo.
            Using banco As New BancoDiProva(larghezza:=1200, altezza:=100)

                banco.Comandi.ASinistra(banco.Bottone("A", 150), banco.Bottone("B", 150))
                banco.Comandi.ADestra(banco.Bottone("C", 150))
                banco.Comandi.Disponi(altezzaMinima:=100)

                Dim righe As Integer() = banco.Righe()

                Assert.HasCount(1, righe, "una riga sola")
                Assert.AreEqual(100, banco.Fascia.Height, "e la fascia resta alta quanto le è stato chiesto")
            End Using
        End Sub

        <TestMethod>
        Public Sub QuandoNonCiStannoInsiemeVannoSuRigheDiverse()
            ' Prima si sovrapponevano e basta: nessuno dei due gruppi sapeva dell'altro.
            Using banco As New BancoDiProva(larghezza:=500, altezza:=60)

                banco.Comandi.ASinistra(banco.Bottone("A", 200), banco.Bottone("B", 200))
                banco.Comandi.ADestra(banco.Bottone("C", 200))
                banco.Comandi.Disponi(altezzaMinima:=60)

                Assert.HasCount(2, banco.Righe(), "due righe")
                banco.NessunaSovrapposizione()
            End Using
        End Sub

        <TestMethod>
        Public Sub UnaFilaTroppoLungaSiSpezzaASuaVolta()
            ' Quattro bottoni che in una riga non ci stanno: si riempie finché ci stanno,
            ' poi si va a capo.
            Using banco As New BancoDiProva(larghezza:=500, altezza:=60)

                banco.Comandi.ASinistra(banco.Bottone("A", 200), banco.Bottone("B", 200),
                                        banco.Bottone("C", 200), banco.Bottone("D", 200))
                banco.Comandi.Disponi(altezzaMinima:=60)

                Assert.IsGreaterThan(1, banco.Righe().Length, "più di una riga")
                banco.NessunaSovrapposizione()
            End Using
        End Sub

        <TestMethod>
        Public Sub LaFasciaCresceQuantoServeEMaiMenoDelMinimo()
            ' Le due promesse insieme: mai più bassa di quel che il pannello chiede (è
            ' l'ingombro del logo), ma più alta se le righe lo richiedono.
            Using banco As New BancoDiProva(larghezza:=500, altezza:=60)

                banco.Comandi.ASinistra(banco.Bottone("A", 200), banco.Bottone("B", 200))
                banco.Comandi.ADestra(banco.Bottone("C", 200))
                banco.Comandi.Disponi(altezzaMinima:=60)

                Assert.IsGreaterThan(60, banco.Fascia.Height, "cresciuta per far posto alle righe")

                Using larga As New BancoDiProva(larghezza:=1200, altezza:=60)
                    larga.Comandi.ASinistra(larga.Bottone("A", 150))
                    larga.Comandi.Disponi(altezzaMinima:=188)

                    Assert.AreEqual(188, larga.Fascia.Height, "e mai sotto il minimo chiesto dal pannello")
                End Using
            End Using
        End Sub

        <TestMethod>
        Public Sub UnAzioneCriticaHaSempreLaSuaRiga()
            ' Il vuoto intorno è la sua prima difesa (cap. 11.5): non deve mai finire sotto
            ' il dito di chi sta premendo il comando accanto, nemmeno quando lo spazio è
            ' tanto e ci starebbe in fila.
            Using banco As New BancoDiProva(larghezza:=1600, altezza:=60)

                Dim critico As Button = banco.Bottone("ELIMINA", 230)

                banco.Comandi.ASinistra(banco.Bottone("A", 150))
                banco.Comandi.ADestra(banco.Bottone("B", 150))
                banco.Comandi.Critici(critico)
                banco.Comandi.Disponi(altezzaMinima:=60)

                For Each altro As Button In banco.Comandi_Bottoni().Where(Function(b) b IsNot critico)
                    Assert.AreNotEqual(critico.Top, altro.Top, $"«{altro.Text}» finisce sulla riga del critico")
                Next

                Assert.IsLessThan(banco.Comandi_Bottoni().Where(Function(b) b IsNot critico).Min(Function(b) b.Top),
                                  critico.Top, "e sta sopra, non sotto")
            End Using
        End Sub

        <TestMethod>
        Public Sub UnComandoNascostoNonOccupaPosto()

            ' R7: il primo comando che c'è solo quando serve è «Rigenera la lettera» di P6.
            ' Da nascosto non deve né lasciare un buco in fascia né — peggio — mandare a
            ' capo gli altri per fare spazio a sé stesso.
            Using banco As New BancoDiProva(larghezza:=700, altezza:=60)

                Dim primo As Button = banco.Bottone("A", 200)
                Dim nascosto As Button = banco.Bottone("NASCOSTO", 300)
                Dim ultimo As Button = banco.Bottone("B", 200)
                nascosto.Visible = False

                banco.Comandi.ASinistra(primo, nascosto, ultimo)
                banco.Comandi.Disponi(altezzaMinima:=60)

                Assert.AreEqual(primo.Top, ultimo.Top, "i due che si vedono stanno sulla stessa riga")
                Assert.AreEqual(primo.Right + StileApp.DistanzaControlli, ultimo.Left,
                                "e uno accanto all'altro, senza il buco di quello nascosto")

            End Using

        End Sub

        <TestMethod>
        Public Sub QuandoIlComandoNascostoCompareTuttiGliAltriGliFannoPosto()

            Using banco As New BancoDiProva(larghezza:=700, altezza:=60)

                Dim primo As Button = banco.Bottone("A", 200)
                Dim aRichiesta As Button = banco.Bottone("A RICHIESTA", 300)
                Dim ultimo As Button = banco.Bottone("B", 200)
                aRichiesta.Visible = False

                banco.Comandi.ASinistra(primo, aRichiesta, ultimo)
                banco.Comandi.Disponi(altezzaMinima:=60)

                aRichiesta.Visible = True
                banco.Comandi.Disponi(altezzaMinima:=60)

                banco.NessunaSovrapposizione()
                Assert.AreEqual(primo.Right + StileApp.DistanzaControlli, aRichiesta.Left,
                                "adesso il posto ce l'ha")

            End Using

        End Sub

        <TestMethod>
        Public Sub NessunPannelloSovrapponeIProprioComandiANessunaLarghezza()
            ' Il collaudo di sistema: la geometria sta in un posto solo, ma i comandi li
            ' dichiara ogni pannello, e un pannello che ne aggiunge uno troppo largo
            ' tornerebbe a sovrapporli. P3 è entrato il 2026-09-08: fino a quel giorno la
            ' sua fascia era d'altra natura — due bottoni posati a mano e un'etichetta
            ' elastica di fianco — e adesso è quella di tutti.
            Dim pannelli As New List(Of Control) From {
                New PannelloHome(), New PannelloProfilo(), New PannelloOpportunita(),
                New PannelloDialogo(), New PannelloDocumenti(), New PannelloRicerca()}

            Try
                For Each pannello As Control In pannelli
                    For Each larghezza As Integer In Larghezze

                        ' Sotto i 1350 px di finestra il logo passa in compatta (cap. 03.5):
                        ' il caso stretto non si ricava da quello largo.
                        Dim ingombro As Size = If(larghezza < 1350, New Size(130, 68), New Size(261, 188))

                        Dim fascia As Panel = DirectCast(
                            pannello.Controls.Find("pnlAzioni", searchAllChildren:=True).Single(), Panel)
                        Dim comandi As Button() = fascia.Controls.OfType(Of Button)().ToArray()

                        ' Si accende tutto quel che la fascia può contenere, compresi i
                        ' comandi che di norma non ci sono — «Rigenera la lettera» di P6
                        ' compare solo quando la lettera è rimasta indietro (R7). È il caso
                        ' peggiore, ed è un caso vero: quando quel bottone c'è, ci sono anche
                        ' tutti gli altri. Verificarlo solo da spento vorrebbe dire non
                        ' verificarlo mai — e la fascia, da spento, non gli tiene il posto.
                        For Each comando As Button In comandi
                            comando.Visible = True
                        Next

                        pannello.Width = larghezza
                        DirectCast(pannello, IPannelloArea).ImpostaIngombroLogo(ingombro)

                        For primo As Integer = 0 To comandi.Length - 2
                            For secondo As Integer = primo + 1 To comandi.Length - 1
                                Assert.IsFalse(
                                    comandi(primo).Bounds.IntersectsWith(comandi(secondo).Bounds),
                                    $"{pannello.GetType().Name} a {larghezza} px: " &
                                    $"«{comandi(primo).Text}» copre «{comandi(secondo).Text}»")
                            Next
                        Next

                        For Each comando As Button In comandi
                            Assert.IsGreaterThanOrEqualTo(fascia.Padding.Left, comando.Left,
                                $"{pannello.GetType().Name} a {larghezza} px: «{comando.Text}» finisce sotto il logo")
                            Assert.IsGreaterThanOrEqualTo(0, comando.Top,
                                $"{pannello.GetType().Name} a {larghezza} px: «{comando.Text}» esce dalla fascia in alto")
                        Next

                    Next
                Next
            Finally
                For Each pannello As Control In pannelli
                    pannello.Dispose()
                Next
            End Try
        End Sub

        ' ==================================================================
        ' La riga che racconta (cap. 03.8)
        ' ==================================================================

        <TestMethod>
        Public Sub IlRaccontoStaSopraIComandiENonLiTocca()

            Using banco As New BancoDiProva(larghezza:=1106, altezza:=188)

                banco.ConLogo(273)
                Dim riga As Label = banco.Racconto()

                banco.Comandi.ASinistra(banco.Bottone("A", 190), banco.Bottone("B", 130))
                banco.Comandi.ADestra(banco.Bottone("C", 190))
                banco.Comandi.Disponi(altezzaMinima:=188)

                For Each comando As Button In banco.Comandi_Bottoni()
                    Assert.IsFalse(riga.Bounds.IntersectsWith(comando.Bounds),
                                   $"il racconto copre «{comando.Text}»")
                    Assert.IsLessThanOrEqualTo(comando.Top, riga.Bottom,
                                               $"il racconto sta sopra «{comando.Text}», non in mezzo")
                Next

                Assert.IsGreaterThanOrEqualTo(0, riga.Top, "e resta dentro la fascia")
                Assert.IsGreaterThanOrEqualTo(banco.Fascia.Padding.Left, riga.Left,
                                              "senza finire sotto il logo")

            End Using

        End Sub

        ''' <summary>
        ''' Il centro della riga è il centro della <b>schermata</b>, non quello dello spazio
        ''' che avanza a destra del logo: sopra c'è la casella di testo, centrata sul
        ''' pannello, e una riga centrata sull'avanzo si leggerebbe come spostata a destra.
        ''' </summary>
        <TestMethod>
        Public Sub IlRaccontoECentratoSullaSchermataNonSulloSpazioCheAvanza()

            Using banco As New BancoDiProva(larghezza:=1106, altezza:=188)

                banco.ConLogo(273)
                Dim riga As Label = banco.Racconto()

                banco.Comandi.ASinistra(banco.Bottone("A", 190))
                banco.Comandi.Disponi(altezzaMinima:=188)

                Assert.AreEqual(riga.Left, banco.Fascia.ClientSize.Width - riga.Right,
                                "lo stesso margine a destra e a sinistra")

            End Using

        End Sub

        ''' <summary>
        ''' Il caso stretto: sotto i 1350 px il logo passa in compatta e la fascia non ha
        ''' più i 188 px che il logo grande le imponeva. Senza un posto tenuto da parte, al
        ''' racconto resterebbero sei pixel — cioè niente, e per giunta in silenzio.
        ''' </summary>
        <TestMethod>
        Public Sub LaFasciaTieneIlPostoAlRaccontoAncheQuandoIlLogoNonGlieloChiede()

            Using banco As New BancoDiProva(larghezza:=1060, altezza:=68)

                banco.ConLogo(142)
                Dim riga As Label = banco.Racconto()

                banco.Comandi.ASinistra(banco.Bottone("A", 190))
                banco.Comandi.Disponi(altezzaMinima:=68)

                Assert.IsGreaterThan(68, banco.Fascia.Height,
                                     "la fascia è cresciuta per far posto al racconto")

                ' Tre righe, che è quel che chiede il più lungo dei messaggi a testo fisso:
                ' quello di P7 con il destinatario preso dall'annuncio e la rifinitura non
                ' riuscita. Il numero non si prende da FasciaDeiComandi apposta — un metro
                ' preso dalla cosa da misurare non la può bocciare.
                Assert.IsGreaterThanOrEqualTo(3 * riga.Font.Height, riga.Height,
                                              "e il posto è di tre righe di testo")

            End Using

        End Sub

        ''' <summary>
        ''' Una fascia senza racconto non paga niente: chi non lo dichiara ha la fascia di
        ''' prima, alta uguale.
        ''' </summary>
        <TestMethod>
        Public Sub UnaFasciaSenzaRaccontoNonTienePostoANessuno()

            Using banco As New BancoDiProva(larghezza:=1060, altezza:=60)

                banco.Comandi.ASinistra(banco.Bottone("A", 190))
                banco.Comandi.Disponi(altezzaMinima:=60)

                Assert.AreEqual(60, banco.Fascia.Height, "alta quanto le è stato chiesto")

            End Using

        End Sub

        ''' <summary>
        ''' Il collaudo di sistema del racconto, su tutte e sette le schermate. Difende due
        ''' cose insieme: che la riga sia davvero <b>nella fascia</b> — fino al 2026-09-08
        ''' stava in alto a destra, dove chi lavora non guarda — e che lì non pesti i piedi
        ''' a nessun comando, a nessuna larghezza.
        ''' </summary>
        <TestMethod>
        Public Sub InTuttiIPannelliIlRaccontoStaSopraIComandiENonLiCopre()

            Dim pannelli As New List(Of Control) From {
                New PannelloHome(), New PannelloProfilo(), New PannelloOpportunita(),
                New PannelloDialogo(), New PannelloDocumenti(), New PannelloEmail(),
                New PannelloRicerca()}

            Try
                For Each pannello As Control In pannelli

                    Dim nome As String = pannello.GetType().Name
                    Dim fascia As Panel = DirectCast(
                        pannello.Controls.Find("pnlAzioni", searchAllChildren:=True).Single(), Panel)

                    ' Se qualcuno la rimettesse nell'intestazione, qui non ci sarebbe più.
                    Dim riga As Label = fascia.Controls.OfType(Of Label)().Single()

                    Assert.AreEqual(ContentAlignment.MiddleCenter, riga.TextAlign,
                                    $"{nome}: il racconto è centrato")

                    For Each larghezza As Integer In Larghezze

                        ' Sotto i 1350 px di finestra il logo passa in compatta (cap. 03.5).
                        Dim ingombro As Size = If(larghezza < 1350, New Size(130, 68), New Size(261, 188))

                        ' Il caso peggiore è quello in cui la fascia è piena: si accende
                        ' tutto, compresi i comandi che di norma non ci sono (R7).
                        For Each comando As Button In fascia.Controls.OfType(Of Button)()
                            comando.Visible = True
                        Next

                        pannello.Width = larghezza
                        DirectCast(pannello, IPannelloArea).ImpostaIngombroLogo(ingombro)

                        For Each comando As Button In fascia.Controls.OfType(Of Button)()
                            Assert.IsFalse(riga.Bounds.IntersectsWith(comando.Bounds),
                                $"{nome} a {larghezza} px: il racconto copre «{comando.Text}»")
                            Assert.IsLessThanOrEqualTo(comando.Top, riga.Bottom,
                                $"{nome} a {larghezza} px: il racconto non sta sopra «{comando.Text}»")
                        Next

                        Assert.IsGreaterThanOrEqualTo(fascia.Padding.Left, riga.Left,
                            $"{nome} a {larghezza} px: il racconto finisce sotto il logo")
                        Assert.IsGreaterThanOrEqualTo(3 * riga.Font.Height, riga.Height,
                            $"{nome} a {larghezza} px: al racconto non restano tre righe di spazio")

                    Next
                Next
            Finally
                For Each pannello As Control In pannelli
                    pannello.Dispose()
                Next
            End Try

        End Sub

        ''' <summary>
        ''' Un pannello finto con la sua fascia: serve a collaudare la geometria senza
        ''' passare da un pannello vero, dove le larghezze dei bottoni sono quelle che sono
        ''' e i casi limite non si possono costruire.
        ''' </summary>
        Private NotInheritable Class BancoDiProva
            Implements IDisposable

            Private ReadOnly _bottoni As New List(Of Button)

            Public Sub New(larghezza As Integer, altezza As Integer)

                Fascia = New Panel With {.Width = larghezza, .Height = altezza}
                Fascia.Padding = New Padding(0, 0, 0, 0)
                Comandi = New FasciaDeiComandi(Fascia)

            End Sub

            Public ReadOnly Property Fascia As Panel
            Public ReadOnly Property Comandi As FasciaDeiComandi

            ''' <summary>Un bottone della misura voluta, già dentro la fascia.</summary>
            Public Function Bottone(nome As String, larghezza As Integer) As Button

                ' Il nome della variabile non può essere «bottone»: in VB le maiuscole non
                ' distinguono, e si chiamerebbe come la funzione che la contiene.
                Dim nuovo As New Button With {
                    .Text = nome, .Width = larghezza, .Height = StileApp.BottoneStandard.Height}

                Fascia.Controls.Add(nuovo)
                _bottoni.Add(nuovo)

                Return nuovo

            End Function

            ''' <summary>
            ''' Il posto che il pannello del logo si prende in fondo a sinistra: nella
            ''' fascia vera è un <c>Padding</c>, e da lì in poi comincia lo spazio buono.
            ''' </summary>
            Public Sub ConLogo(larghezza As Integer)
                Fascia.Padding = New Padding(larghezza, 0, 0, 0)
            End Sub

            ''' <summary>La riga che racconta, già dentro la fascia e già dichiarata.</summary>
            Public Function Racconto() As Label

                ' Il nome della variabile non può essere «racconto»: coprirebbe la
                ' funzione che la contiene (BC30530).
                Dim nata As New Label With {
                    .Font = StileApp.FontDidascalia,
                    .TextAlign = ContentAlignment.MiddleCenter}

                Fascia.Controls.Add(nata)
                Comandi.Racconta(nata)

                Return nata

            End Function

            ''' <summary>I bottoni dichiarati, nell'ordine in cui sono nati.</summary>
            Public Function Comandi_Bottoni() As Button()
                Return _bottoni.ToArray()
            End Function

            ''' <summary>Le righe occupate, per ordinata: una riga è un valore di Top.</summary>
            Public Function Righe() As Integer()
                Return _bottoni.Select(Function(b) b.Top).Distinct().OrderBy(Function(y) y).ToArray()
            End Function

            Public Sub NessunaSovrapposizione()

                For primo As Integer = 0 To _bottoni.Count - 2
                    For secondo As Integer = primo + 1 To _bottoni.Count - 1
                        Assert.IsFalse(_bottoni(primo).Bounds.IntersectsWith(_bottoni(secondo).Bounds),
                                       $"«{_bottoni(primo).Text}» copre «{_bottoni(secondo).Text}»")
                    Next
                Next

            End Sub

            Public Sub Dispose() Implements IDisposable.Dispose
                Fascia.Dispose()
            End Sub

        End Class

    End Class

End Namespace
