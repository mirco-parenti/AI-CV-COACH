Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace Ui

    ''' <summary>
    ''' Collaudi della finestra delle azioni di livello 5 (cap. 03.3, cap. 11.5). Qui non
    ''' c'è nessuna parola da indovinare — è proprio la differenza con la sorella maggiore
    ''' <c>FinestraConfermaCritica</c> — e quel che va difeso è l'altra metà: che il gesto
    ''' resti <b>un clic voluto</b>, e non un Invio preso per abitudine.
    ''' </summary>
    ''' <remarks>
    ''' La finestra si costruisce e si interroga <b>senza mostrarla</b>: di una finestra
    ''' modale il banco non può aspettare la chiusura.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraConferma

        <TestMethod>
        Public Sub IlBottoneCheEseguePortaIlVerboDellAzione()

            ' È la ragione per cui questa finestra esiste invece di una MessageBox: lì i
            ' due tasti sono Sì e No, e un «Sì» risponde alla domanda; qui il bottone dice
            ' quel che fa, e risponde alla conseguenza.
            Using finestra As New TrovaLavoro.FinestraConferma(
                "Elimina la candidatura", "Sparisce la sua cartella.", "Confermo")

                Assert.AreEqual("Confermo", Bottone(finestra, "btnAzione").Text)
                Assert.AreEqual("Annulla", Bottone(finestra, "btnAnnulla").Text)
                Assert.IsTrue(Bottone(finestra, "btnAzione").Enabled,
                              "acceso da subito: qui non c'è niente da scrivere")
            End Using

        End Sub

        <TestMethod>
        Public Sub InvioNonConfermaEdEscAnnulla()

            ' La scorciatoia da tastiera esiste solo per la via d'uscita.
            Using finestra As New TrovaLavoro.FinestraConferma(
                "Elimina la candidatura", "Sparisce la sua cartella.", "Confermo")

                Assert.IsNull(finestra.AcceptButton, "nessun bottone appeso a Invio")
                Assert.AreSame(Bottone(finestra, "btnAnnulla"), finestra.CancelButton, "Esc annulla")

                Assert.IsLessThan(Bottone(finestra, "btnAzione").TabIndex,
                                  Bottone(finestra, "btnAnnulla").TabIndex,
                                  "e il fuoco parte sull'annulla, che è il tasto giusto da premere per sbaglio")
            End Using

        End Sub

        <TestMethod>
        Public Sub LaFinestraSiAdattaAlTestoCheLeDanno()

            ' Come nella sorella maggiore: è la finestra ad adattarsi al testo, non il
            ' testo ad accorciarsi per starci dentro.
            Using corta As New TrovaLavoro.FinestraConferma("Titolo", "Una riga.", "Confermo"),
                  lunga As New TrovaLavoro.FinestraConferma(
                      "Titolo", String.Join(vbLf, Enumerable.Repeat("Una riga in più.", 12)), "Confermo")

                Assert.IsLessThan(lunga.ClientSize.Height, corta.ClientSize.Height,
                                  "con più testo la finestra è più alta")
                Assert.AreEqual(corta.ClientSize.Width, lunga.ClientSize.Width,
                                "ma larga uguale: la colonna del testo è una sola")
            End Using

        End Sub

        ' Che un clic su «Confermo» chiuda con OK **non** si collauda qui, e non per
        ' pigrizia: <c>PerformClick</c> passa da <c>CanSelect</c>, che su una finestra mai
        ' mostrata è falso — il clic non scatta affatto, e un collaudo scritto così
        ' sarebbe verde qualunque cosa faccia il gestore. È lo stesso motivo per cui il
        ' banco della sorella maggiore, dove un clic senza parola «non conferma», in realtà
        ' non prova nulla. Qui si difende quel che si vede davvero: le etichette, il fuoco,
        ' i tasti e la misura.

        Private Shared Function Bottone(finestra As Control, nome As String) As Button
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

    End Class

End Namespace
