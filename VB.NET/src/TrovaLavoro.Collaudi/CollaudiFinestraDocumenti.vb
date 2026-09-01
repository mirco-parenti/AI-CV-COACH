Imports System.Linq
Imports System.Windows.Forms
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro
Imports TrovaLavoro.Dati

Namespace Ui

    ''' <summary>
    ''' Collaudi della finestra che fa confermare i documenti riconosciuti (cap. 05.2,
    ''' passo 4). Quello che qui può rompersi davvero è la correzione: se una categoria
    ''' cambiata a mano non arrivasse alla raccolta — o non restasse — la conferma umana
    ''' sarebbe una formalità.
    ''' </summary>
    ''' <remarks>
    ''' Come le altre finestre modali, si costruisce e si interroga <b>senza mostrarla</b>:
    ''' per questo la correzione ha un metodo suo (<c>CambiaLaCategoria</c>), che è anche
    ''' quello che la tendina chiama.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiFinestraDocumenti

        Private Shared Function ConTreDocumenti() As RaccoltaDocumenti

            Dim raccolta As New RaccoltaDocumenti With {.Cartella = "C:\documenti"}

            raccolta.Documenti.Add(New DocumentoClassificato With {
                .Nome = "CV_2025.pdf", .Categoria = CategoriaDocumento.Cv, .Motivo = "nome e contatti in testa"})
            raccolta.Documenti.Add(New DocumentoClassificato With {
                .Nome = "HACCP.pdf", .Categoria = CategoriaDocumento.Attestato, .Motivo = "rilasciato da un ente"})
            raccolta.Documenti.Add(New DocumentoClassificato With {
                .Nome = "misterioso.pdf", .Categoria = CategoriaDocumento.Altro, .Motivo = "non si capisce"})

            Return raccolta

        End Function

        <TestMethod>
        Public Sub LElencoMostraCosaSiEriconosciuto()
            Dim raccolta As RaccoltaDocumenti = ConTreDocumenti()

            Using finestra As New FinestraDocumenti(raccolta)

                Dim elenco As ListView = Lista(finestra)

                Assert.HasCount(3, elenco.Items, "tutti e tre")
                Assert.AreEqual("CV_2025.pdf", elenco.Items(0).SubItems(0).Text, "il nome")
                Assert.AreEqual("attestato", elenco.Items(1).SubItems(1).Text, "la categoria, in italiano")
                Assert.AreEqual("non si capisce", elenco.Items(2).SubItems(2).Text,
                                "e il perché, che serve a correggere a colpo d'occhio")
            End Using
        End Sub

        <TestMethod>
        Public Sub CorreggereUnaCategoriaLaScriveNellaRaccolta()
            ' È il passo 4 del cap. 05.2: la proposta è dell'AI, la parola è dell'utente.
            Dim raccolta As RaccoltaDocumenti = ConTreDocumenti()

            Using finestra As New FinestraDocumenti(raccolta)

                Assert.IsTrue(finestra.CambiaLaCategoria(2, CategoriaDocumento.Attestato), "corretto")

                Assert.AreEqual(CategoriaDocumento.Attestato, raccolta.Documenti(2).Categoria, "nella raccolta")
                Assert.IsTrue(raccolta.Documenti(2).Corretto, "e marcato come deciso da una persona")
                Assert.HasCount(2, raccolta.Attestati(), "adesso gli attestati da allegare sono due")

                Assert.AreEqual("attestato", Lista(finestra).Items(2).SubItems(1).Text, "e si vede nell'elenco")
                Assert.Contains("tu", Lista(finestra).Items(2).SubItems(2).Text, "col motivo che dice chi l'ha detto")
            End Using
        End Sub

        <TestMethod>
        Public Sub UnaCorrezioneFuoriDallElencoNonFaDanno()
            Dim raccolta As RaccoltaDocumenti = ConTreDocumenti()

            Using finestra As New FinestraDocumenti(raccolta)

                Assert.IsFalse(finestra.CambiaLaCategoria(-1, CategoriaDocumento.Cv), "prima del primo")
                Assert.IsFalse(finestra.CambiaLaCategoria(9, CategoriaDocumento.Cv), "dopo l'ultimo")
                Assert.HasCount(1, raccolta.Attestati(), "e niente è cambiato")
            End Using
        End Sub

        <TestMethod>
        Public Sub ChiudendoSenzaDireNienteNonSiEConfermatoNiente()
            ' La X in alto a destra vale come un annulla: chi chiama, su «annullato»,
            ' rilegge la raccolta da disco e butta le correzioni.
            Using finestra As New FinestraDocumenti(ConTreDocumenti())

                Assert.AreEqual(EsitoDocumenti.Annullato, finestra.Esito, "finché non decide, non ha deciso")
                Assert.AreSame(Bottone(finestra, "btnAnnulla"), finestra.CancelButton, "ed Esc annulla")
                Assert.IsNull(finestra.AcceptButton,
                              "nessun bottone appeso a Invio: l'elenco si percorre con la tastiera")
            End Using
        End Sub

        <TestMethod>
        Public Sub LaTendinaOffreLeQuattroCategorieDelCapitolo()
            Using finestra As New FinestraDocumenti(ConTreDocumenti())

                Dim menuCategorie As ComboBox = Tendina(finestra)
                Dim voci As String() = menuCategorie.Items.Cast(Of String)().ToArray()

                CollectionAssert.AreEqual({"CV", "attestato", "lettera", "altro"}, voci, "le quattro del cap. 05.2")
                Assert.IsFalse(menuCategorie.Enabled, "spenta finché non si sceglie un documento")
            End Using
        End Sub

        <TestMethod>
        Public Sub SenzaCartellaLaFinestraLoDice()
            Using finestra As New FinestraDocumenti(New RaccoltaDocumenti())

                Assert.Contains("Nessuna cartella", Etichetta(finestra, "lblCartella").Text)
                Assert.IsEmpty(Lista(finestra).Items, "e non c'è niente da confermare")
            End Using
        End Sub

        <TestMethod>
        Public Sub SenzaRaccoltaNonSiApre()
            ' Una finestra che chiede di confermare il niente non ha senso, e il difetto
            ' sarebbe di chi l'ha aperta.
            Assert.Throws(Of ArgumentNullException)(
                Sub()
                    Dim inutile As New FinestraDocumenti(Nothing)
                End Sub)
        End Sub

        <TestMethod>
        Public Sub QuandoNonCiStaSiScorreInveceDiTagliare()
            ' A 150% i testi crescono e la finestra cresce con loro, ma non oltre lo
            ' spazio che c'è: il tetto e lo scorrimento vanno insieme, o quel che resta
            ' fuori cade fuori dalla finestra e nessuno spostamento lo recupera
            ' (decisione 15.7).
            Using finestra As New FinestraDocumenti(ConTreDocumenti())

                finestra.DisponiIn(200)

                Assert.IsTrue(finestra.AutoScroll, "con questo spazio si scorre")
                Assert.IsLessThanOrEqualTo(200, finestra.ClientSize.Height,
                                           "e la finestra sta nello spazio che c'è")
            End Using
        End Sub

        <TestMethod>
        Public Sub LElencoArrivaFinoAlMargine()
            ' L'elenco si misura sulla larghezza della finestra e non su una costante:
            ' è quel che a DPI alti gli faceva lasciare una fascia vuota a destra.
            Using finestra As New FinestraDocumenti(ConTreDocumenti())

                finestra.DisponiIn(4000)

                Assert.IsFalse(finestra.AutoScroll, "con tutto questo spazio non si scorre")
                Assert.AreEqual(finestra.ClientSize.Width - StileApp.MargineRiquadro,
                                Lista(finestra).Bounds.Right, "e l'elenco finisce al margine")
            End Using
        End Sub

        <TestMethod>
        Public Sub LAzionePrincipaleDiQuestaFinestraEConfermare()
            ' Il passo 4 del cap. 05.2 è la conferma umana: è per quella che la finestra si
            ' apre. Far rileggere la cartella è il vicino di riga di «cambia cartella» — si
            ' torna indietro di un passo — e dal 2026-09-01 porta il suo livello 2: due
            ' bottoni che dicono «vai avanti» ne lasciano zero che dicano «hai finito».
            Using finestra As New FinestraDocumenti(ConTreDocumenti())

                Assert.AreEqual(LivelloBottone.SicuroPositivo, Bottone(finestra, "btnConferma").Tag)
                Assert.AreEqual(LivelloBottone.Esplorativo, Bottone(finestra, "btnRileggi").Tag)
                Assert.AreEqual(LivelloBottone.Esplorativo, Bottone(finestra, "btnCambiaCartella").Tag)

            End Using
        End Sub

        Private Shared Function Lista(finestra As Control) As ListView
            Return DirectCast(finestra.Controls.Find("lvwDocumenti", searchAllChildren:=True).Single(), ListView)
        End Function

        Private Shared Function Tendina(finestra As Control) As ComboBox
            Return DirectCast(finestra.Controls.Find("cmbCategoria", searchAllChildren:=True).Single(), ComboBox)
        End Function

        Private Shared Function Bottone(finestra As Control, nome As String) As Button
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Button)
        End Function

        Private Shared Function Etichetta(finestra As Control, nome As String) As Label
            Return DirectCast(finestra.Controls.Find(nome, searchAllChildren:=True).Single(), Label)
        End Function

    End Class

End Namespace
