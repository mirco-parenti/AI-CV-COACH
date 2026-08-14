Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace Ui

    ''' <summary>
    ''' Collaudi della finestra che chiede la chiave API (cap. 11.3). Quello che qui può
    ''' rompersi davvero è di due tipi: che si salvi una chiave che non c'è, e che la
    ''' chiave già in uso compaia per intero sotto gli occhi di chi passa.
    ''' </summary>
    ''' <remarks>
    ''' <para>Come per <see cref="TrovaLavoro.FinestraConfermaCritica"/>, la finestra si
    ''' costruisce e si interroga <b>senza mostrarla</b>: di una modale il banco non può
    ''' aspettare la chiusura.</para>
    ''' <para>Ed è per questo che qui non si guarda <c>Visible</c> di un'etichetta né il
    ''' <c>DialogResult</c> della finestra: su una finestra mai mostrata il primo è
    ''' sempre falso e il secondo resta <c>None</c> anche dopo una chiusura. Si guarda lo
    ''' stato che la finestra dichiara — <c>MostraLaChiaveInUso</c>,
    ''' <c>AvvertimentoDiForma</c>, <c>ChiaveDigitata</c> — che è anche quello su cui il
    ''' codice vero prende le sue decisioni.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraChiaveApi

        Private Const ChiaveFinta As String = "sk-ant-collaudo-non-vera-0000-9876"

        <TestMethod>
        Public Sub SenzaNienteScrittoNonCEDaSalvare()
            Using finestra As New TrovaLavoro.FinestraChiaveApi()

                Dim salva As Button = Bottone(finestra, "btnSalva")
                Dim campoChiave As TextBox = Casella(finestra, "txtChiave")

                Assert.IsFalse(salva.Enabled, "appena aperta non c'è niente da salvare")

                campoChiave.Text = "   "
                Assert.IsFalse(salva.Enabled, "e nemmeno con dei soli spazi")

                campoChiave.Text = ChiaveFinta
                Assert.IsTrue(salva.Enabled, "con una chiave scritta sì")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaCasellaNasceMascherataESiPuoScoprire()
            ' La mascheratura è contro gli occhi di passaggio, non contro l'utente: chi
            ' incolla deve poter controllare di aver incollato la cosa giusta.
            Using finestra As New TrovaLavoro.FinestraChiaveApi()

                Dim campoChiave As TextBox = Casella(finestra, "txtChiave")
                Dim mostra As CheckBox = Spunta(finestra, "chkMostra")

                Assert.IsTrue(campoChiave.UseSystemPasswordChar, "di suo non si legge")

                mostra.Checked = True
                Assert.IsFalse(campoChiave.UseSystemPasswordChar, "chiedendolo sì")

                mostra.Checked = False
                Assert.IsTrue(campoChiave.UseSystemPasswordChar, "e si richiude")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaFormaSiSegnalaMaNonImpedisce()
            ' Chi ha una chiave fatta in un altro modo la sa usare meglio di noi: si
            ' avverte e si lascia passare.
            Using finestra As New TrovaLavoro.FinestraChiaveApi()

                Dim campoChiave As TextBox = Casella(finestra, "txtChiave")

                Assert.IsFalse(finestra.AvvertimentoDiForma, "a casella vuota non si rimprovera nessuno")

                campoChiave.Text = "una-chiave-di-un-altro-tipo"
                Assert.IsTrue(finestra.AvvertimentoDiForma, "l'avvertimento compare")
                Assert.IsTrue(Bottone(finestra, "btnSalva").Enabled, "ma il bottone resta acceso")

                campoChiave.Text = ChiaveFinta
                Assert.IsFalse(finestra.AvvertimentoDiForma, "e sparisce quando la forma è quella attesa")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaChiaveGiaSalvataSiVedeSoloMascherata()
            ' È la riga che dice «quella che scrivi prende il posto di questa»: serve a
            ' capire cosa sta per succedere, non a rileggersi la chiave.
            Using finestra As New TrovaLavoro.FinestraChiaveApi(ChiaveFinta)

                Dim salvata As Label = Etichetta(finestra, "lblSalvata")

                Assert.IsTrue(finestra.MostraLaChiaveInUso, "la riga c'è")
                Assert.DoesNotContain(ChiaveFinta, salvata.Text, "ma non per intero")
                Assert.Contains("sk-ant-…9876", salvata.Text, "solo i bordi")
            End Using
        End Sub

        <TestMethod>
        Public Sub AlPrimoAvvioLaRigaDellaChiaveInUsoNonCE()
            Using finestra As New TrovaLavoro.FinestraChiaveApi()
                Assert.IsFalse(finestra.MostraLaChiaveInUso, "non c'è nessuna chiave da sostituire")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaFinestraSiAdattaAllaRigaInPiu()
            ' Con la riga della chiave già in uso la finestra cresce; senza, quella riga
            ' non lascia il suo buco.
            Using primoAvvio As New TrovaLavoro.FinestraChiaveApi(),
                  conChiave As New TrovaLavoro.FinestraChiaveApi(ChiaveFinta)

                Assert.IsLessThan(conChiave.ClientSize.Height, primoAvvio.ClientSize.Height,
                                  "con una riga in più la finestra è più alta")
                Assert.AreEqual(primoAvvio.ClientSize.Width, conChiave.ClientSize.Width,
                                "ma larga uguale")
            End Using
        End Sub

        <TestMethod>
        Public Sub ACasellaVuotaNonSiSalvaNiente()
            ' Invio è legato al bottone «Salva», e un Invio preso per abitudine non deve
            ' far salvare una chiave che non è stata scritta.
            Using finestra As New TrovaLavoro.FinestraChiaveApi()

                Assert.IsFalse(finestra.PrendiLaChiaveScritta(), "non c'è niente da prendere")
                Assert.IsNull(finestra.ChiaveDigitata, "e infatti non c'è nessuna chiave")

                Casella(finestra, "txtChiave").Text = "   "
                Assert.IsFalse(finestra.PrendiLaChiaveScritta(), "e degli spazi non sono una chiave")
                Assert.IsNull(finestra.ChiaveDigitata, "niente nemmeno adesso")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaChiaveDigitataEsceNettaDagliSpazi()
            ' Una chiave incollata si porta dietro spazi e a capo: entrerebbero
            ' nell'intestazione HTTP della chiamata.
            Using finestra As New TrovaLavoro.FinestraChiaveApi()

                Casella(finestra, "txtChiave").Text = "  " & ChiaveFinta & "  "

                Assert.IsTrue(finestra.PrendiLaChiaveScritta(), "c'è una chiave da prendere")
                Assert.AreEqual(ChiaveFinta, finestra.ChiaveDigitata, "e esce netta")
            End Using
        End Sub

        <TestMethod>
        Public Sub RimandareELegittimo()
            ' Senza chiave l'applicazione si apre lo stesso: «Non adesso» è una risposta,
            ' non una fuga. Chi chiama guarda la chiave, non l'esito della finestra: così
            ' anche la X in alto a destra vale come un «non adesso».
            Using finestra As New TrovaLavoro.FinestraChiaveApi()

                ' Scritta nella casella, ma mai presa: chi non preme «Salva» — perché ha
                ' scelto «Non adesso», Esc o la X — non lascia nessuna chiave dietro di sé.
                Casella(finestra, "txtChiave").Text = ChiaveFinta

                Assert.IsNull(finestra.ChiaveDigitata, "quel che è solo scritto non è salvato")
                Assert.AreSame(Bottone(finestra, "btnNonAdesso"), finestra.CancelButton, "ed Esc fa lo stesso")
                Assert.AreSame(Bottone(finestra, "btnSalva"), finestra.AcceptButton, "mentre Invio salva")
            End Using
        End Sub

        Private Shared Function Bottone(finestra As Control, nome As String) As Button
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Casella(finestra As Control, nome As String) As TextBox
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), TextBox)
        End Function

        Private Shared Function Spunta(finestra As Control, nome As String) As CheckBox
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), CheckBox)
        End Function

        Private Shared Function Etichetta(finestra As Control, nome As String) As Label
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

    End Class

End Namespace
