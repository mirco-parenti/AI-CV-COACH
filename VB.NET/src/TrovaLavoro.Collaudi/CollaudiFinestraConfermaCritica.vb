Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace Ui

    ''' <summary>
    ''' Collaudi della finestra delle azioni di livello 6 (cap. 03.3, cap. 11.5). Quello
    ''' che qui può rompersi davvero è uno solo, ed è grave: che il bottone che non si
    ''' disfa si accenda senza che l'utente abbia scritto la parola. Il resto della
    ''' finestra è testo.
    ''' </summary>
    ''' <remarks>
    ''' La finestra si costruisce e si interroga <b>senza mostrarla</b>: di una finestra
    ''' modale il banco non può aspettare la chiusura. È la stessa ragione per cui il
    ''' pannello P2 espone <c>EliminaIlProfilo</c> accanto al bottone che la apre.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraConfermaCritica

        <TestMethod>
        Public Sub IlBottoneSiAccendeSoloConLaParolaEsatta()
            Using finestra As New TrovaLavoro.FinestraConfermaCritica(
                "Elimina profilo - definitivo", "Sparisce tutto.", "Elimina il profilo")

                Dim azione As Button = Bottone(finestra, "btnAzione")
                Dim parola As TextBox = Casella(finestra, "txtParola")

                Assert.IsFalse(azione.Enabled, "appena aperta non si elimina niente")

                parola.Text = "TrovaLavor"
                Assert.IsFalse(azione.Enabled, "una parola quasi giusta non basta")

                parola.Text = " trovalavoro "
                Assert.IsTrue(azione.Enabled,
                              "spazi e maiuscole si perdonano: il gesto è scriverla, non indovinarla")

                parola.Text = ""
                Assert.IsFalse(azione.Enabled, "e cancellandola il bottone si richiude")
            End Using
        End Sub

        <TestMethod>
        Public Sub InvioNonConfermaEdEscAnnulla()
            ' La scorciatoia da tastiera esiste solo per la via d'uscita: un Invio preso
            ' per abitudine uscendo dalla casella non deve eliminare niente.
            Using finestra As New TrovaLavoro.FinestraConfermaCritica(
                "Elimina profilo - definitivo", "Sparisce tutto.", "Elimina il profilo")

                Assert.IsNull(finestra.AcceptButton, "nessun bottone appeso a Invio")
                Assert.AreSame(Bottone(finestra, "btnAnnulla"), finestra.CancelButton, "Esc annulla")
            End Using
        End Sub

        <TestMethod>
        Public Sub SenzaLaParolaNemmenoUnClicConferma()
            ' L'ultima porta: il bottone è spento, ma la guardia si ripete nel gestore —
            ' un clic che non passa dall'interfaccia non deve scavalcarla.
            Using finestra As New TrovaLavoro.FinestraConfermaCritica(
                "Elimina profilo - definitivo", "Sparisce tutto.", "Elimina il profilo")

                Bottone(finestra, "btnAzione").PerformClick()

                Assert.AreNotEqual(DialogResult.OK, finestra.DialogResult, "non ha confermato niente")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaFinestraSiAdattaAlTestoCheLeDanno()
            ' La spiegazione è lunga quanto serve a dire cosa sparisce e cosa resta: è la
            ' finestra ad adattarsi al testo, non il testo ad accorciarsi.
            Using corta As New TrovaLavoro.FinestraConfermaCritica("Titolo", "Una riga.", "Elimina"),
                  lunga As New TrovaLavoro.FinestraConfermaCritica(
                      "Titolo", String.Join(vbLf, Enumerable.Repeat("Una riga in più.", 12)), "Elimina")

                Assert.IsLessThan(lunga.ClientSize.Height, corta.ClientSize.Height,
                                  "con più testo la finestra è più alta")
                Assert.AreEqual(corta.ClientSize.Width, lunga.ClientSize.Width,
                                "ma larga uguale: la colonna del testo è una sola")
            End Using
        End Sub

        Private Shared Function Bottone(finestra As Control, nome As String) As Button
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Casella(finestra As Control, nome As String) As TextBox
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), TextBox)
        End Function

    End Class

End Namespace
