Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Dati

Namespace Ui

    ''' <summary>
    ''' Collaudi della spia del profilo (cap. 03.8): la lucina che dice se quel che si sta
    ''' guardando — un punteggio, un CV, una lettera — è nato dal profilo di oggi.
    ''' </summary>
    ''' <remarks>
    ''' <para>Qui si sorveglia la <b>decisione</b>: quale stato per quale versione, con che
    ''' parola e con che inchiostro. Che poi quella decisione arrivi davvero a video in tutti
    ''' e quattro i posti è un'altra cosa, e per i due che vivono dentro una finestra non è
    ''' il banco a poterlo dire (v. <c>in_sospeso.md</c>).</para>
    ''' <para>Il caso più importante di tutti è il terzo: <b>spenta</b>. Una candidatura mai
    ''' confrontata non ha versione, e la domanda «il profilo è cambiato dopo?» di una
    ''' versione vuota risponde di no — che tradotto in lucine sarebbe un verde. Un verde su
    ''' una cosa che non esiste è una bugia detta con la faccia rassicurante, ed è il difetto
    ''' che questi collaudi esistono per non far tornare.</para>
    ''' </remarks>
    <TestClass>
    Public Class CollaudiSpiaDelProfilo

        <TestMethod>
        Public Sub SenzaNienteDaGiudicareNonSiAccendeNiente()

            ConArchivio(
                Sub(archivio)
                    Dim versione As String = archivio.Salva(ProfiloDiProva())

                    Dim spia As LetturaSpia = SpiaDelProfilo.Leggi(archivio, versione, ceQualcosa:=False)

                    Assert.AreEqual(StatoSpia.Spenta, spia.Stato,
                                    "su una candidatura mai confrontata non c'è niente da dichiarare")
                    Assert.IsFalse(spia.Accesa)
                    Assert.AreEqual("", spia.Scritta, "e a spia spenta non si scrive niente")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnaVersioneNonAnnotataLasciaLaSpiaSpenta()

            ' Di un dubbio non si fa un allarme, ma nemmeno una promessa: se non si sa da
            ' quale profilo venga, non si dice né che è in pari né che non lo è.
            ConArchivio(
                Sub(archivio)
                    archivio.Salva(ProfiloDiProva())

                    Assert.AreEqual(StatoSpia.Spenta,
                                    SpiaDelProfilo.Leggi(archivio, "", ceQualcosa:=True).Stato)
                    Assert.AreEqual(StatoSpia.Spenta,
                                    SpiaDelProfilo.Leggi(archivio, Nothing, ceQualcosa:=True).Stato)
                End Sub)

        End Sub

        <TestMethod>
        Public Sub IlProfiloDiOggiAccendeIlVerde()

            ConArchivio(
                Sub(archivio)
                    Dim versione As String = archivio.Salva(ProfiloDiProva())

                    Dim spia As LetturaSpia = SpiaDelProfilo.Leggi(archivio, versione, ceQualcosa:=True)

                    Assert.AreEqual(StatoSpia.Allineato, spia.Stato)
                    Assert.AreEqual("profilo usato: corrente", spia.Parola)
                    Assert.AreEqual(StileApp.Successo, spia.Colore)
                    Assert.IsFalse(String.IsNullOrWhiteSpace(spia.Perche),
                                   "anche il verde deve saper dire perché è verde")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnProfiloCresciutoAccendeIlRosso()

            ' Stessa persona, dati di ieri. Fino al 2026-09-01 questo caso si taceva sulle
            ' candidature: poi si è visto che fra un salvataggio e l'altro può cambiare un
            ' requisito eliminatorio, e allora le stelle di allora non sono vecchie, sono
            ' un'altra risposta.
            ConArchivio(
                Sub(archivio)
                    Dim profilo As TrovaLavoro.Dati.Profilo = ProfiloDiProva()
                    Dim prima As String = archivio.Salva(profilo)
                    archivio.Salva(profilo)

                    Dim spia As LetturaSpia = SpiaDelProfilo.Leggi(archivio, prima, ceQualcosa:=True)

                    Assert.AreEqual(StatoSpia.Disallineato, spia.Stato)
                    Assert.AreEqual("profilo usato: obsoleto", spia.Parola)
                    Assert.AreEqual(StileApp.RossoCritico, spia.Colore)
                    Assert.Contains("Hai cambiato il profilo", spia.Perche,
                                    "il perché dice che è cresciuto, non che è sparito")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub UnProfiloRifattoDaCapoLoDiceConLeSueParole()

            ' Il caso grave: il profilo di allora è stato eliminato, e quei documenti
            ' raccontano un'altra persona. Il colore è lo stesso — chi guarda deve solo
            ' sapere se fidarsi — ma il perché no, e va guardato che non si confonda col
            ' caso lieve: quando una versione non c'è più, anche CambiatoDopo risponde di sì.
            ConArchivio(
                Sub(archivio)
                    Dim spia As LetturaSpia = SpiaDelProfilo.Leggi(archivio, "2026-01-01_000000",
                                                                   ceQualcosa:=True)

                    Assert.AreEqual(StatoSpia.Disallineato, spia.Stato)
                    Assert.Contains("non c'è più", spia.Perche,
                                    "il caso grave non si racconta con le parole di quello lieve")
                End Sub)

        End Sub

        <TestMethod>
        Public Sub LeCorrezioniNonSalvateValgonoGiaIlRosso()

            ' Nessun archivio può saperlo: su disco non è successo niente. Ma chi guarda il
            ' 📄 CV base mentre corregge il profilo sta guardando il ritratto di com'era
            ' prima, e la spia deve dirlo prima del salvataggio, non dopo.
            ConArchivio(
                Sub(archivio)
                    Dim versione As String = archivio.Salva(ProfiloDiProva())

                    Dim spia As LetturaSpia = SpiaDelProfilo.Leggi(archivio, versione,
                                                                   ceQualcosa:=True,
                                                                   giaCambiato:=True)

                    Assert.AreEqual(StatoSpia.Disallineato, spia.Stato,
                                    "il profilo è già cambiato: lo si è appena scritto")
                    Assert.Contains("non l'hai ancora salvato", spia.Perche)
                End Sub)

        End Sub

        <TestMethod>
        Public Sub LaSpiaPortaSempreLaSuaParolaEnonSoloIlPallino()

            ' Cap. 03.8: il colore da solo non basta mai, e verde contro rosso è proprio la
            ' coppia che una persona su dodici non separa. Il pallino da solo lascerebbe
            ' fuori esattamente lei.
            ConArchivio(
                Sub(archivio)
                    Dim profilo As TrovaLavoro.Dati.Profilo = ProfiloDiProva()
                    Dim prima As String = archivio.Salva(profilo)
                    Dim dopo As String = archivio.Salva(profilo)

                    For Each spia As LetturaSpia In {SpiaDelProfilo.Leggi(archivio, dopo, True),
                                                     SpiaDelProfilo.Leggi(archivio, prima, True)}

                        Assert.IsTrue(spia.Accesa)
                        Assert.Contains("profilo", spia.Scritta,
                                        "la parola sta nella scritta, non solo nel suggerimento")
                        Assert.IsTrue(spia.Scritta.Length > SpiaDelProfilo.Pallino.Length + 1,
                                      "un pallino con niente accanto non dice niente a chi non vede i colori")
                    Next
                End Sub)

        End Sub

        <TestMethod>
        Public Sub IDueInchiostriSonoQuelliCheSiLeggonoSullAvorio()

            ' Misurati il 2026-09-02 sul fondo delle pagine (#FFFAF0), non scelti a occhio:
            ' il rosso dei badge — StileApp.Pericolo — lì vale 4,35:1 e non arriva alla
            ' soglia di 4,5. RossoCritico vale 7,07:1. Questo collaudo esiste perché il
            ' rosso «ovvio» non torni dentro con le migliori intenzioni.
            Assert.AreNotEqual(StileApp.Pericolo, StileApp.RossoCritico,
                               "se i due rossi diventassero lo stesso, questo collaudo non guarderebbe più niente")

            ConArchivio(
                Sub(archivio)
                    Dim profilo As TrovaLavoro.Dati.Profilo = ProfiloDiProva()
                    Dim prima As String = archivio.Salva(profilo)
                    Dim dopo As String = archivio.Salva(profilo)

                    Assert.AreEqual(StileApp.Successo,
                                    SpiaDelProfilo.Leggi(archivio, dopo, True).Colore)
                    Assert.AreEqual(StileApp.RossoCritico,
                                    SpiaDelProfilo.Leggi(archivio, prima, True).Colore)
                End Sub)

        End Sub

        <TestMethod>
        Public Sub SenzaArchivioNonSiInventaNiente()

            ' Il pannello che nasce prima di avere un contesto non deve accendere niente:
            ' meglio una spia spenta che una verde detta a caso.
            Assert.AreEqual(StatoSpia.Spenta,
                            SpiaDelProfilo.Leggi(Nothing, "2026-01-01_000000", ceQualcosa:=True).Stato)

        End Sub

        ''' <summary>Il profilo del banco.</summary>
        Private Shared Function ProfiloDiProva() As TrovaLavoro.Dati.Profilo
            Return TrovaLavoro.Dati.Profilo.DaJson(CasiDiCollaudo.Profilo())
        End Function

        ''' <summary>
        ''' Una prova su un archivio in una cartella temporanea, cancellata comunque vada.
        ''' </summary>
        Private Shared Sub ConArchivio(prova As Action(Of ArchivioProfilo))

            Dim radice As String = Path.Combine(Path.GetTempPath(),
                                                "spia-profilo-" & Guid.NewGuid().ToString("N"))
            Try
                prova(New ArchivioProfilo(New CartellaDati(radice)))
            Finally
                If Directory.Exists(radice) Then Directory.Delete(radice, recursive:=True)
            End Try

        End Sub

    End Class

End Namespace
