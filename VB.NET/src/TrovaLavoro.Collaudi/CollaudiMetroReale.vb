Imports Microsoft.VisualStudio.TestTools.UnitTesting

Namespace NonRegressione

    ''' <summary>
    ''' Collaudi degli <b>attrezzi di misura</b> che i collaudi reali si dividono
    ''' (<see cref="CollaudoReale"/>). Girano <b>senza rete</b> e senza il CV vero: qui non
    ''' si misura l'applicazione, si misura il metro.
    ''' </summary>
    ''' <remarks>
    ''' Nascono a T4 da una lezione di T3, dove un collaudo verde misurava sé stesso: un
    ''' attrezzo che decide cosa passa e cosa no è codice come tutto il resto, e finché
    ''' nessuno lo prova, un suo errore si traveste da difetto del prodotto — o, peggio,
    ''' da assenza di difetti.
    ''' </remarks>
    <TestClass>
    Public Class CollaudiMetroReale

        ' ==================================================================
        ' La città: la stessa, anche se una si porta dietro la provincia
        ' ==================================================================

        ' Le città qui sono **inventate**, come tutti i casi del banco: il fenomeno vero è
        ' arrivato dal CV di Mirco, ma il repo è pubblico e un collaudo non vale di più
        ' perché ci mette dentro il domicilio di qualcuno (v. `casi/LEGGIMI.md`).

        <TestMethod>
        Public Sub LaProvinciaInPiuNonFaDueCittaDiverse()

            ' Il fenomeno visto nel collaudo di tappa di T4 (2026-08-10): la stessa riga di
            ' domicilio, letta quattro volte, è tornata tre volte senza la sua provincia.
            Assert.IsTrue(CollaudoReale.StessaCitta("Forlì", "Forlì (FC)"),
                          "la provincia in più non cambia la città")
            Assert.IsTrue(CollaudoReale.StessaCitta("Forlì (FC)", "Forlì"),
                          "e non conta da che parte sta la lettura più ricca")
            Assert.IsTrue(CollaudoReale.StessaCitta("forlì  (fc)", "Forlì (FC)"),
                          "maiuscole e spazi doppi non sono una differenza")
            Assert.IsTrue(CollaudoReale.StessaCitta("Reggio Emilia", "Reggio Emilia (RE)"),
                          "vale anche per una città che di parole ne ha due sue")

        End Sub

        <TestMethod>
        Public Sub DueCittaDiverseRestanoDueCittaDiverse()

            ' Il motivo per cui questo controllo esiste ancora: se una lettura sbaglia
            ' indirizzo, deve diventare rosso.
            Assert.IsFalse(CollaudoReale.StessaCitta("Bologna", "Forlì"),
                           "due città diverse sono un difetto, non una sfumatura")
            Assert.IsFalse(CollaudoReale.StessaCitta("Forlì", "Forlì Rimini"),
                           "due città insieme non sono «una sola»: il prompt ne vuole una")

        End Sub

        <TestMethod>
        Public Sub UnaCittaVuotaNonEUgualeAUnaScritta()

            ' Vuota contro scritta è una lettura che non ha trovato l'indirizzo, e va
            ' vista; vuote tutt'e due è un CV che non lo dichiara, e non è colpa di nessuno.
            Assert.IsFalse(CollaudoReale.StessaCitta("", "Forlì"), "chi non l'ha trovata lo dice")
            Assert.IsFalse(CollaudoReale.StessaCitta("Forlì", "   "), "e gli spazi non sono una città")
            Assert.IsTrue(CollaudoReale.StessaCitta("", ""), "nessuna delle due l'ha trovata: pari")

        End Sub

    End Class

End Namespace
