Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Dati

    ''' <summary>
    ''' Collaudi del riepilogo che esce dall'applicazione (cap. 07.3). Non è un backup —
    ''' quello è JSON e serve a tornare indietro (cap. 11.4): questo serve a raccontare, e
    ''' il capitolo lo promette fin dal primo rilascio.
    ''' </summary>
    ''' <remarks>
    ''' Le cose che possono rompersi davvero sono due, e sono entrambe di forma: un campo
    ''' che contiene il separatore <b>sposta tutte le colonne</b> di quella riga, e un
    ''' campo che contiene una barra verticale <b>rompe la tabella</b> markdown da lì in
    ''' giù. I titoli degli annunci veri contengono di tutto.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiEsportazioneRegistro

        <TestMethod>
        Public Sub IlCsvHaUnIntestazioneEUnaRigaPerCandidatura()
            Dim testo As String = EsportazioneRegistro.Componi(
                {Voce("Rossi S.p.A.", "Magazziniere", 4.1)}, FormatoEsportazione.Csv)

            Dim righe As String() = RigheDi(testo)

            Assert.HasCount(2, righe, "l'intestazione e una candidatura")
            Assert.StartsWith("stelle;eliminatorio;azienda", righe(0), "le colonne, col punto e virgola")
            Assert.Contains("Rossi S.p.A.", righe(1))
            Assert.Contains("Magazziniere", righe(1))
        End Sub

        <TestMethod>
        Public Sub IlSeparatoreDentroUnCampoNonSpostaLeColonne()
            ' Un titolo come «Magazziniere; turni notturni» esiste, e senza protezione
            ' sposterebbe di una colonna tutto quel che segue su quella riga.
            Dim testo As String = EsportazioneRegistro.Componi(
                {Voce("Rossi S.p.A.", "Magazziniere; turni notturni", 4.1)}, FormatoEsportazione.Csv)

            Assert.Contains("""Magazziniere; turni notturni""", testo, "il campo va fra virgolette")
        End Sub

        <TestMethod>
        Public Sub LeVirgoletteDentroUnCampoSiRaddoppiano()
            ' È la regola del CSV: dentro un campo protetto, una virgoletta si scrive due
            ' volte. Senza, il campo si chiuderebbe a metà.
            Dim testo As String = EsportazioneRegistro.Componi(
                {Voce("La ""Bottega"" S.r.l.", "Commesso", 2.0)}, FormatoEsportazione.Csv)

            ' Attenzione a leggere l'atteso: qui si chiedono le virgolette del campo
            ' protetto e, dentro, quelle raddoppiate — che in VB si scrivono quattro volte.
            Assert.Contains("""La """"Bottega"""" S.r.l.""", testo)
        End Sub

        <TestMethod>
        Public Sub UnaCandidaturaSenzaStelleLasciaIlCampoVuoto()
            ' Mai uno zero: zero stelle vuol dire «confrontata, e non vale niente», che è
            ' un'altra cosa da «non ancora confrontata».
            Dim testo As String = EsportazioneRegistro.Componi(
                {Voce("Neri S.p.A.", "Autista", Nothing)}, FormatoEsportazione.Csv)

            Assert.StartsWith(";", RigheDi(testo)(1), "il primo campo è vuoto, non uno zero")
        End Sub

        <TestMethod>
        Public Sub LeDateEscanoInFormaOrdinabile()
            ' 14/08/2026 lo legge in un modo ogni programma che lo apre. La forma ISO no, e
            ' per giunta si ordina da sola.
            Dim testo As String = EsportazioneRegistro.Componi(
                {Voce("Rossi S.p.A.", "Magazziniere", 4.1)}, FormatoEsportazione.Csv)

            Assert.Contains("2026-08-10 09:30", testo)
        End Sub

        <TestMethod>
        Public Sub IlMarkdownPortaTitoloContestoETabella()
            Dim testo As String = EsportazioneRegistro.Componi(
                {Voce("Rossi S.p.A.", "Magazziniere", 4.1)}, FormatoEsportazione.Markdown,
                contesto:="Esportato il 14 agosto 2026 · 1 su 3")

            Assert.Contains("# Registro delle candidature", testo, "il titolo")
            Assert.Contains("*Esportato il 14 agosto 2026 · 1 su 3*", testo, "e cosa si sta guardando")
            Assert.Contains("| stelle | eliminatorio |", testo, "l'intestazione della tabella")
            Assert.Contains("| Rossi S.p.A. |", testo, "e la candidatura")
        End Sub

        <TestMethod>
        Public Sub IlContestoRestaFuoriDalCsv()
            ' Una frase in cima a un CSV non è più una tabella: il foglio di calcolo se ne
            ' accorgerebbe con una colonna sballata su tutte le righe.
            Dim testo As String = EsportazioneRegistro.Componi(
                {Voce("Rossi S.p.A.", "Magazziniere", 4.1)}, FormatoEsportazione.Csv,
                contesto:="Esportato il 14 agosto 2026")

            Assert.DoesNotContain("Esportato", testo)
            Assert.StartsWith("stelle;", testo)
        End Sub

        <TestMethod>
        Public Sub UnaBarraVerticaleNonRompeLaTabella()
            ' «Magazziniere | turnista» in una cella markdown aprirebbe una colonna in più
            ' su quella riga, e da lì in giù la tabella si disallinea.
            Dim testo As String = EsportazioneRegistro.Componi(
                {Voce("Rossi S.p.A.", "Magazziniere | turnista", 4.1)}, FormatoEsportazione.Markdown)

            Assert.Contains("Magazziniere \| turnista", testo)
        End Sub

        <TestMethod>
        Public Sub SenzaCandidatureIlMarkdownLoDiceAParole()
            ' Una tabella con la sola intestazione sembrerebbe un'esportazione andata male,
            ' e fra sei mesi nessuno saprebbe distinguere i due casi.
            Dim testo As String = EsportazioneRegistro.Componi(
                New VoceRegistro() {}, FormatoEsportazione.Markdown)

            Assert.Contains("Nessuna candidatura da riportare", testo)
            Assert.DoesNotContain("| stelle |", testo)
        End Sub

        <TestMethod>
        Public Sub OgniFormatoHaLaSuaEstensione()
            Assert.AreEqual(".csv", EsportazioneRegistro.Estensione(FormatoEsportazione.Csv))
            Assert.AreEqual(".md", EsportazioneRegistro.Estensione(FormatoEsportazione.Markdown))
        End Sub

        <TestMethod>
        Public Sub NelRiepilogoLoStatoDiceComEAndata()

            ' «Esce quel che si vede» (cap. 07.3): nella Home la colonna dice «Rifiutata»,
            ' e chi apre il foglio deve leggere la stessa parola — «Con esito», da solo,
            ' non racconta niente a chi non ha l'applicazione davanti.
            Dim finita As VoceRegistro = Voce("Acme", "Magazziniere", 3.5)
            finita.Stato = StatoOpportunita.Esito
            finita.Esito = EsitoCandidatura.Rifiutata

            Dim testo As String = EsportazioneRegistro.Componi(
                {finita}, FormatoEsportazione.Csv)

            Assert.Contains("Rifiutata", testo)
            Assert.DoesNotContain("Con esito", testo)

        End Sub

        Private Shared Function Voce(azienda As String, titolo As String, stelle As Double?) As VoceRegistro

            Return New VoceRegistro With {
                .Cartella = "2026-08-10_prova",
                .Stato = StatoOpportunita.Generata,
                .Azienda = azienda,
                .Titolo = titolo,
                .Fonte = "Indeed",
                .Link = "https://esempio.it/annuncio",
                .Lingua = "it",
                .Stelle = stelle,
                .Creata = New Date(2026, 8, 10, 9, 30, 0),
                .Aggiornata = New Date(2026, 8, 10, 9, 45, 0)}

        End Function

        Private Shared Function RigheDi(testo As String) As String()
            Return testo.Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
        End Function

        ''' <summary>
        ''' Titolo e azienda arrivano dall'annuncio, cioè da un testo scritto da qualcun
        ''' altro. Aperto in Excel, un campo che comincia con <c>=</c> non è un titolo: è una
        ''' formula, e viene eseguita. <i>(Reperto M2 della revisione del giro D.)</i>
        ''' </summary>
        <TestMethod>
        Public Sub UnaCellaCheSembraUnaFormulaNonLoDiventa()

            For Each inizio As String In New String() {"=", "+", "-", "@"}

                Dim testo As String = EsportazioneRegistro.Componi(
                    {Voce("Rossi S.p.A.", inizio & "CMD()", 4.1)}, FormatoEsportazione.Csv)

                Assert.Contains("'" & inizio & "CMD()", testo,
                                $"la cella che comincia con «{inizio}» si dichiara testo")

            Next

        End Sub

        <TestMethod>
        Public Sub UnTitoloNormaleNonSiPortaViaUnApostrofo()

            ' La cura costa un carattere visibile: che si paghi solo dove serve.
            Dim testo As String = EsportazioneRegistro.Componi(
                {Voce("Rossi S.p.A.", "Magazziniere", 4.1)}, FormatoEsportazione.Csv)

            Assert.DoesNotContain("'Magazziniere", testo, "l'apostrofo si mette solo dove serve")

        End Sub

    End Class

End Namespace
