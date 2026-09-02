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
        ''' Ogni bottone dell'applicazione porta una delle misure della scala, e nessuna
        ''' misura sua.
        ''' </summary>
        ''' <remarks>
        ''' <para>È la guardia della regola dettata dal tutor il 2026-09-01: un bottone non
        ''' prende più la larghezza che la sua scritta chiede, la prende dalla scala di
        ''' <c>StileApp</c>. Prima di quel giorno le larghezze scritte a mano nei designer
        ''' erano <b>ventiquattro</b>, e ognuna sembrava giusta perché era stata misurata
        ''' sul proprio testo: è un difetto che non si vede da nessun collaudo di
        ''' comportamento e che ricompare al primo bottone aggiunto di fretta.</para>
        ''' <para>Restano fuori i sei bottoni del menu d'ingresso (<c>BottoneMenu</c>), che
        ''' non hanno una misura di disegno affatto: la loro la calcola il pannello sulla
        ''' finestra di adesso (cap. 03.6).</para>
        ''' <para>Le misure si confrontano <b>esatte</b>, com'è già altrove nel banco: la
        ''' macchina dei collaudi gira a 96 DPI e i numeri del designer non vengono scalati.
        ''' </para>
        ''' </remarks>
        <TestMethod>
        Public Sub OgniBottoneHaUnaMisuraDellaScala()

            Dim scala As Size() = {StileApp.BottoneIcona, StileApp.BottoneStandard,
                                   StileApp.BottoneMedio, StileApp.BottoneLargo,
                                   StileApp.BottoneMoltoLargo, StileApp.BottoneMassimo,
                                   StileApp.BottoneBarraSuperiore, StileApp.BottoneBarraSuperioreLargo,
                                   StileApp.BottoneBarraSuperioreIcona}

            Using form As New FormPrincipale()

                Dim bottoni As Button() = TuttiIBottoni(form).
                    Where(Function(b) Not (TypeOf b Is BottoneMenu)).ToArray()

                Assert.IsGreaterThan(40, bottoni.Length, "i bottoni della finestra e dei suoi pannelli")

                For Each bottone As Button In bottoni
                    Assert.Contains(bottone.Size, scala,
                                    $"«{bottone.Name}» porta una misura sua ({bottone.Width}×{bottone.Height})")
                Next

            End Using

        End Sub

        ''' <summary>
        ''' E dentro quella misura la scritta ci sta per intero.
        ''' </summary>
        ''' <remarks>
        ''' È la metà che rende sopportabile la scala: un `Button` non manda a capo e non
        ''' mette i puntini, <b>taglia</b>, e l'unico segno è mezza parola mancante a video.
        ''' Finché ogni bottone si misurava sul proprio testo il rischio non c'era per
        ''' costruzione; adesso che la misura viene da fuori, qualcuno deve dire che il
        ''' gradino scelto è abbastanza alto — e lo deve dire il banco, perché a occhio un
        ''' bottone tagliato in un pannello che non si apre spesso non lo vede nessuno.
        ''' <para>Restano fuori i sei bottoni del menu d'ingresso: quelli il testo lo
        ''' tagliano con i puntini e hanno il loro collaudo (<c>CollaudiMenu</c>).</para>
        ''' </remarks>
        <TestMethod>
        Public Sub OgniBottoneDiceIlProprioNomePerIntero()

            Using form As New FormPrincipale()

                For Each bottone As Button In TuttiIBottoni(form)

                    If TypeOf bottone Is BottoneMenu OrElse String.IsNullOrEmpty(bottone.Text) Then Continue For

                    Assert.IsLessThanOrEqualTo(
                        bottone.Width,
                        TextRenderer.MeasureText(bottone.Text, bottone.Font).Width,
                        $"«{bottone.Text}» non ci sta nel suo bottone ({bottone.Name})")

                Next

            End Using

        End Sub

        ''' <summary>Tutti i bottoni dentro un controllo, a qualunque profondità.</summary>
        Private Shared Iterator Function TuttiIBottoni(radice As Control) As IEnumerable(Of Button)

            For Each figlio As Control In radice.Controls

                Dim bottone As Button = TryCast(figlio, Button)
                If bottone IsNot Nothing Then Yield bottone

                For Each dentro As Button In TuttiIBottoni(figlio)
                    Yield dentro
                Next

            Next

        End Function

        ''' <summary>
        ''' Nessun bottone vestito da qui è più <c>Flat</c>: sono tutti bottoni di Windows.
        ''' </summary>
        ''' <remarks>
        ''' Dal 2026-09-01, su indicazione del tutor. Non è un dettaglio di stile: da
        ''' <c>Standard</c> tutto ciò che passa da <c>FlatAppearance</c> — i contorni
        ''' disegnati, il fondo che si scuriva sotto il puntatore — smette di avere
        ''' effetto <b>senza dare errore</b>. Rimettere una di quelle righe non romperebbe
        ''' niente e non si vedrebbe: si vedrebbe solo il segnale che non arriva più. Per
        ''' questo il banco guarda lo stile, che è la causa, e non i sintomi.
        ''' </remarks>
        <TestMethod>
        Public Sub NessunBottoneVestitoDaQuiRestaPiatto()

            For Each livello As LivelloBottone In [Enum].GetValues(GetType(LivelloBottone))
                Using bottone As New Button()
                    StileApp.VestiBottone(bottone, livello)
                    Assert.AreEqual(FlatStyle.Standard, bottone.FlatStyle,
                                    $"il livello {livello} è un bottone di Windows")
                End Using
            Next

            For Each ruolo As RuoloBarra In [Enum].GetValues(GetType(RuoloBarra))
                For Each attiva As Boolean In {False, True}
                    Using casella As New Button()
                        StileApp.VestiBottoneBarra(casella, ruolo, attiva)
                        Assert.AreEqual(FlatStyle.Standard, casella.FlatStyle,
                                        $"la casella {ruolo} (aperta: {attiva}) pure")
                    End Using
                Next
            Next

        End Sub

        ''' <summary>La casella aperta si distingue dal fondo, che è l'unico mezzo rimasto.</summary>
        ''' <remarks>
        ''' Fino al 2026-09-01 il fondo diceva il <b>ruolo</b> e a dire quale pannello
        ''' fosse aperto c'era la cornice, disegnata da <c>FlatAppearance</c>. Con
        ''' <c>FlatStyle.Standard</c> quella cornice non esiste più e restavano le sole
        ''' lettere d'accento su fondo azzurro: la differenza più debole della barra, e
        ''' l'unica cosa che avrebbe detto «sei qui». Adesso lo dice il fondo — blu pieno,
        ''' lettere bianche — e vale per tutte e sette, verde compreso.
        ''' </remarks>
        <TestMethod>
        Public Sub LaCasellaApertaSiDistinguePerIlFondo()

            For Each ruolo As RuoloBarra In [Enum].GetValues(GetType(RuoloBarra))
                Using riposo As New Button(), aperta As New Button()

                    StileApp.VestiBottoneBarra(riposo, ruolo, attiva:=False)
                    StileApp.VestiBottoneBarra(aperta, ruolo, attiva:=True)

                    Assert.AreNotEqual(riposo.BackColor, aperta.BackColor,
                                       $"la casella {ruolo} aperta cambia fondo")
                    Assert.AreEqual(StileApp.Accento, aperta.BackColor,
                                    $"e il fondo è il blu d'accento, per la {ruolo} come per le altre")
                    Assert.AreEqual(StileApp.SfondoContenuto, aperta.ForeColor,
                                    "con le lettere bianche, le sole leggibili su quel blu")

                End Using
            Next

        End Sub

        <TestMethod>
        Public Sub UnaCasellaSpentaSiSmorzaEPoiRitrovaIlSuoRuolo()
            ' Mentre l'AI lavora la barra si spegne tutta (cap. 02.6): sette caselle
            ' colorate che restano colorate direbbero che si può ancora andare via.
            Using casella As New Button()

                StileApp.VestiBottoneBarra(casella, RuoloBarra.RitornoAlMenu, attiva:=False)

                casella.Enabled = False

                Assert.AreEqual(StileApp.SfondoBase, casella.BackColor, "spenta: fondo smorzato")
                Assert.AreEqual(StileApp.TestoSecondario, casella.ForeColor, "e testo smorzato")

                casella.Enabled = True

                Assert.AreEqual(StileApp.Successo, casella.BackColor, "riaccesa: torna il verde del ritorno")

            End Using
        End Sub

        ''' <summary>E una casella aperta che si spegne si risveglia ancora aperta.</summary>
        ''' <remarks>
        ''' È la metà che il collaudo qui sopra non guarda: lo stato «aperta» viaggia nel
        ''' <c>Tag</c> insieme al ruolo, e da quando a dirlo è il <b>fondo</b> — lo stesso
        ''' posto in cui lo spento scrive il suo grigio — perderlo vorrebbe dire che il
        ''' pannello aperto smette di sembrarlo ogni volta che l'AI lavora.
        ''' </remarks>
        <TestMethod>
        Public Sub UnaCasellaApertaSiRisvegliaAncoraAperta()

            Using casella As New Button()

                StileApp.VestiBottoneBarra(casella, RuoloBarra.Destinazione, attiva:=True)

                casella.Enabled = False
                casella.Enabled = True

                Assert.AreEqual(StileApp.Accento, casella.BackColor, "riaccesa: è ancora quella aperta")

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
