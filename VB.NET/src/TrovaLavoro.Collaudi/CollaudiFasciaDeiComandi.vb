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
        Public Sub NessunPannelloSovrapponeIProprioComandiANessunaLarghezza()
            ' Il collaudo di sistema: la geometria sta in un posto solo, ma i comandi li
            ' dichiara ogni pannello, e un pannello che ne aggiunge uno troppo largo
            ' tornerebbe a sovrapporli. P3 non c'è perché la sua fascia è d'altra natura —
            ' due bottoni e un'etichetta elastica, che non può accavallarsi a nessuno.
            Dim pannelli As New List(Of Control) From {
                New PannelloHome(), New PannelloProfilo(), New PannelloOpportunita(),
                New PannelloDialogo(), New PannelloDocumenti()}

            Try
                For Each pannello As Control In pannelli
                    For Each larghezza As Integer In Larghezze

                        ' Sotto i 1350 px di finestra il logo passa in compatta (cap. 03.5):
                        ' il caso stretto non si ricava da quello largo.
                        Dim ingombro As Size = If(larghezza < 1350, New Size(130, 68), New Size(261, 188))

                        pannello.Width = larghezza
                        DirectCast(pannello, IPannelloArea).ImpostaIngombroLogo(ingombro)

                        Dim fascia As Panel = DirectCast(
                            pannello.Controls.Find("pnlAzioni", searchAllChildren:=True).Single(), Panel)
                        Dim comandi As Button() = fascia.Controls.OfType(Of Button)().ToArray()

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
