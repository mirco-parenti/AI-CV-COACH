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

    End Class

End Namespace
