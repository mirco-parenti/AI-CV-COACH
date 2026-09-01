Imports System.Collections.Generic
Imports System.Drawing
Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro

Namespace Ui

    ''' <summary>
    ''' Collaudi dei token di design (cap. 03.2 e 03.3). Non si verificano i colori uno
    ''' per uno — quelli sono la tabella — ma la regola che il capitolo esprime e che il
    ''' codice potrebbe tradire: il colore dice la <b>conseguenza</b>, e un bottone che
    ''' non si può premere non deve sembrare premibile.
    ''' </summary>
    <TestClass>
    Public Class CollaudiStileApp

        <TestMethod>
        Public Sub UnBottoneSpentoNonSembraPremibile()
            ' Un bottone piatto con un colore suo resta acceso all'occhio anche da
            ' disabilitato: è il difetto che si è visto alla prima prova a video.
            Using bottone As New Button()
                StileApp.VestiBottone(bottone, LivelloBottone.SicuroPositivo)
                Assert.AreEqual(StileApp.Successo, bottone.BackColor, "acceso: il verde del livello 1")

                bottone.Enabled = False

                Assert.AreEqual(StileApp.SfondoBase, bottone.BackColor, "spento: fondo smorzato")
                Assert.AreEqual(StileApp.TestoSecondario, bottone.ForeColor, "e testo smorzato")
            End Using
        End Sub

        <TestMethod>
        Public Sub RiaccendendoloRitrovaIlSuoLivello()
            ' Il livello resta scritto nel Tag proprio per questo: quando la tappa che lo
            ' abilita arriva, il bottone non deve essere rivestito a mano.
            Using bottone As New Button()
                StileApp.VestiBottone(bottone, LivelloBottone.Distruttivo)
                bottone.Enabled = False

                bottone.Enabled = True

                Assert.AreEqual(StileApp.Pericolo, bottone.BackColor, "torna il rosso del livello 5")
                Assert.AreEqual(StileApp.SfondoContenuto, bottone.ForeColor, "con il testo bianco")
            End Using
        End Sub

        <TestMethod>
        Public Sub VestirloDueVolteNonAccumulaGestori()
            ' La vestizione deve poter essere ripetuta (succede quando un pannello si
            ' ricostruisce) senza lasciare dietro di sé gestori sovrapposti.
            Using bottone As New Button()
                StileApp.VestiBottone(bottone, LivelloBottone.Esplorativo)
                StileApp.VestiBottone(bottone, LivelloBottone.AzionePrincipale)

                bottone.Enabled = False
                bottone.Enabled = True

                Assert.AreEqual(StileApp.FondoAzione, bottone.BackColor, "vale l'ultimo livello dichiarato")
            End Using
        End Sub

        <TestMethod>
        Public Sub OgniLivelloHaIlSuoFondo()
            ' Nel dubbio fra due livelli si sceglie il più alto: perché la scelta abbia
            ' senso, due livelli diversi devono guardarsi diversi.
            Dim fondi As New List(Of Drawing.Color)

            For Each livello As LivelloBottone In [Enum].GetValues(GetType(LivelloBottone))
                Using bottone As New Button()
                    StileApp.VestiBottone(bottone, livello)
                    fondi.Add(bottone.BackColor)
                End Using
            Next

            Assert.HasCount(fondi.Count, fondi.Distinct().ToList(), "nessun livello copia il fondo di un altro")
        End Sub

        ''' <summary>
        ''' Un bottone si scurisce sotto il puntatore, e di più mentre lo si preme.
        ''' </summary>
        ''' <remarks>
        ''' Fino al 2026-09-01 nessun livello dichiarava <c>MouseOverBackColor</c>, e un
        ''' bottone piatto con un colore suo resta <b>identico</b> sotto il puntatore: il
        ''' colore diceva la conseguenza, ma niente diceva «questo si preme». È il difetto
        ''' che non rompe niente — l'applicazione funziona, semplicemente non risponde — e
        ''' per questo lo deve dire il banco. Si guarda la <b>luce</b> e non il colore
        ''' esatto: quale sia il fondo di ciascun livello lo dice la tabella, quel che qui si
        ''' sorveglia è che i tre momenti siano tre e nell'ordine giusto.
        ''' </remarks>
        <TestMethod>
        Public Sub OgniBottoneSiScurisceSottoIlPuntatore()

            For Each livello As LivelloBottone In [Enum].GetValues(GetType(LivelloBottone))
                Using bottone As New Button()

                    StileApp.VestiBottone(bottone, livello)

                    Dim riposo As Single = bottone.BackColor.GetBrightness()
                    Dim sopra As Single = bottone.FlatAppearance.MouseOverBackColor.GetBrightness()
                    Dim premuto As Single = bottone.FlatAppearance.MouseDownBackColor.GetBrightness()

                    Assert.IsLessThan(riposo, sopra, $"il livello {livello} si scurisce al passaggio")
                    Assert.IsLessThan(sopra, premuto, $"e il livello {livello} di più da premuto")

                End Using
            Next

        End Sub

        ''' <summary>E lo fanno anche le caselle della barra, che sono la fila che si attraversa.</summary>
        <TestMethod>
        Public Sub AncheLeCaselleDellaBarraSiScurisconoSottoIlPuntatore()

            For Each ruolo As RuoloBarra In [Enum].GetValues(GetType(RuoloBarra))
                Using casella As New Button()

                    StileApp.VestiBottoneBarra(casella, ruolo, attiva:=False)

                    Dim riposo As Single = casella.BackColor.GetBrightness()
                    Dim sopra As Single = casella.FlatAppearance.MouseOverBackColor.GetBrightness()
                    Dim premuto As Single = casella.FlatAppearance.MouseDownBackColor.GetBrightness()

                    Assert.IsLessThan(riposo, sopra, $"la casella {ruolo} si scurisce al passaggio")
                    Assert.IsLessThan(sopra, premuto, $"e la casella {ruolo} di più da premuta")

                End Using
            Next

        End Sub

        <TestMethod>
        Public Sub LaCasellaApertaSiDistingueSenzaCambiareFondo()
            ' Sulla barra il fondo dice il ruolo (azzurro le destinazioni, verde il
            ' ritorno) e non può dire anche quale pannello è aperto: a dirlo restano la
            ' cornice, le lettere e il carattere — tre segnali insieme, come al livello 2.
            Using riposo As New Button(), aperta As New Button()

                StileApp.VestiBottoneBarra(riposo, RuoloBarra.Destinazione, attiva:=False)
                StileApp.VestiBottoneBarra(aperta, RuoloBarra.Destinazione, attiva:=True)

                Assert.AreEqual(riposo.BackColor, aperta.BackColor, "il fondo è lo stesso")
                Assert.AreNotEqual(riposo.FlatAppearance.BorderColor, aperta.FlatAppearance.BorderColor,
                                   "la cornice cambia colore")
                Assert.AreNotEqual(riposo.FlatAppearance.BorderSize, aperta.FlatAppearance.BorderSize,
                                   "e spessore")
                Assert.AreNotEqual(riposo.ForeColor, aperta.ForeColor, "le lettere cambiano colore")

            End Using
        End Sub

        <TestMethod>
        Public Sub UnaCasellaSpentaSiSmorzaEPoiRitrovaIlSuoRuolo()
            ' Mentre l'AI lavora la barra si spegne tutta (cap. 02.6): sette caselle
            ' colorate che restano colorate direbbero che si può ancora andare via.
            Using casella As New Button()

                StileApp.VestiBottoneBarra(casella, RuoloBarra.RitornoAlMenu, attiva:=True)

                casella.Enabled = False

                Assert.AreEqual(StileApp.SfondoBase, casella.BackColor, "spenta: fondo smorzato")
                Assert.AreEqual(StileApp.TestoSecondario, casella.ForeColor, "e testo smorzato")

                casella.Enabled = True

                Assert.AreEqual(StileApp.Successo, casella.BackColor, "riaccesa: torna il verde del ritorno")
                Assert.AreEqual(2, casella.FlatAppearance.BorderSize, "e la cornice doppia di quella aperta")

            End Using
        End Sub

        ''' <summary>
        ''' Le pagine che si aprono dal menu portano il fondo caldo, e dentro non resta
        ''' niente di bianco.
        ''' </summary>
        ''' <remarks>
        ''' <para>Dal 2026-08-31 l'avorio della soglia entra anche nelle pagine
        ''' (cap. 03.6): il fondo è <c>FondoPagina</c>, i rettangoli che tengono testo
        ''' <c>FondoCasella</c>. Un colore che manca non rompe niente — la pagina si apre
        ''' lo stesso — e per questo lo deve dire il banco: una casella dimenticata resta
        ''' bianca in mezzo all'avorio e la si scopre guardando, se qualcuno guarda proprio
        ''' quella scheda.</para>
        ''' <para>Non si chiede <i>quale</i> colore abbia ogni rettangolo, ma che non sia
        ''' rimasto <b>bianco</b>: il bianco è il fondo che Windows dà da sé a una casella
        ''' di cui nessuno ha detto niente, ed è l'unico modo in cui il difetto si presenta.
        ''' Chiedere il colore esatto significherebbe riscrivere qui il designer, e
        ''' bocciare domani una casella spenta solo perché si smorza come deve.</para>
        ''' </remarks>
        <TestMethod>
        Public Sub OgniPaginaPortaIlFondoCaldoEDentroNonRestaBianco()

            Using form As New FormPrincipale()

                Dim centrale As Control =
                    form.Controls.Find("pnlAreaCentrale", searchAllChildren:=True).Single()

                ' Il menu resta fuori: è la soglia, e l'avorio ce l'ha già per conto suo.
                Dim pagine As Control() =
                    centrale.Controls.OfType(Of Control)().
                             Where(Function(c) Not (TypeOf c Is PannelloMenu)).ToArray()

                Assert.AreEqual(7, pagine.Length, "le pagine ospitate dall'area centrale")

                For Each pagina As Control In pagine

                    Assert.AreEqual(StileApp.FondoPagina, pagina.BackColor,
                                    $"«{pagina.Name}» non ha il fondo delle pagine")

                    For Each rettangolo As Control In RettangoliDi(pagina)
                        Assert.AreNotEqual(Color.White.ToArgb(), rettangolo.BackColor.ToArgb(),
                                           $"«{rettangolo.Name}» è rimasto bianco")
                    Next

                Next

            End Using

        End Sub

        ''' <summary>
        ''' I rettangoli che tengono testo dentro una pagina, a qualunque profondità.
        ''' </summary>
        ''' <remarks>
        ''' Le tendine non ci sono: sono comandi, e come i bottoni di questo giro non erano.
        ''' </remarks>
        Private Shared Iterator Function RettangoliDi(radice As Control) As IEnumerable(Of Control)

            For Each figlio As Control In radice.Controls

                If TypeOf figlio Is TextBoxBase OrElse TypeOf figlio Is ListBox OrElse
                   TypeOf figlio Is ListView OrElse TypeOf figlio Is TabPage Then
                    Yield figlio
                End If

                For Each dentro As Control In RettangoliDi(figlio)
                    Yield dentro
                Next

            Next

        End Function

    End Class

End Namespace
