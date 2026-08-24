Imports System.Linq
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports TrovaLavoro.Motore

''' <summary>
''' Collaudi di <see cref="ConduttoreDiDialogo"/>, cioè di chi risponde al posto
''' dell'utente quando un collaudo conduce il dialogo con l'AI vera.
''' </summary>
''' <remarks>
''' Il conduttore è codice di collaudo, ma è quello su cui i collaudi reali fondano il
''' loro verdetto: se sbaglia turno, il rapporto resta verde e dice una cosa falsa — è già
''' successo con «Una cosa sola:» della patente, e le prove reali costano una chiamata
''' all'AI ciascuna, quindi non sono la rete adatta ad accorgersene. Questi girano senza
''' rete, con <see cref="StrutturatoreFinto"/> al posto del modello.
''' </remarks>
<TestClass>
Public Class CollaudiConduttoreDiDialogo

    Private Const FrNome As String = "{""nome"": ""Luca Ferrari"", ""altrove"": {}}"

    Private Const FrContatti As String =
        "{""contatti"": {""email"": ""luca@example.it"", ""telefono"": ""333 0000000""," &
        " ""citta"": ""Forlì"", ""link"": """"}, ""altrove"": {}}"

    Private Const FrPatente As String = "{""patente"": {""ha"": ""no"", ""categorie"": []}, ""altrove"": {}}"

    ''' <summary>Un lavoro senza la durata: è la voce che fa scattare la domanda.</summary>
    Private Const FrLavoroSenzaDurata As String =
        "{""esperienze_formali"": [{""ruolo"": ""Magazziniere"", ""azienda"": ""Romagna Logistica""," &
        " ""durata"": """", ""cosa_facevo"": ""Carico e scarico"", ""tipo"": """"}], ""altrove"": {}}"

    Private Const FrInformale As String =
        "{""esperienze_informali"": [{""cosa_facevo"": ""Traslochi con un amico"", ""quando"": """"," &
        " ""con_chi"": """"}], ""altrove"": {}}"

    Private Const FrCompetenze As String = "{""competenze"": [""Uso del muletto""], ""altrove"": {}}"

    Private Const FrFormazione As String =
        "{""formazione"": [{""titolo"": ""Diploma di perito"", ""istituto"": ""ITIS""," &
        " ""anno"": ""2018""}], ""altrove"": {}}"

    <TestMethod>
    Public Async Function AllaDomandaSuUnCampoVuotoRispondeSenzaConsumareLaTraccia() As Task

        ' Se il conduttore non riconoscesse la domanda, le manderebbe la battuta del turno
        ' dopo: da lì in avanti ogni risposta finirebbe nel turno sbagliato, e il collaudo
        ' reale resterebbe verde perché il dialogo, dal canto suo, si è comportato bene.
        Dim finto As New StrutturatoreFinto
        finto.Dara(FrNome).Dara(FrContatti).Dara(FrPatente).
              Dara(FrLavoroSenzaDurata).Dara(FrLavoroSenzaDurata).
              Dara(FrInformale).Dara(FrCompetenze).Dara(FrFormazione)

        Dim conduttore As New ConduttoreDiDialogo(Traccia())
        Dim dialogo As New DialogoProfilo(finto)

        Dim battiti As List(Of ConduttoreDiDialogo.Battito) = Await conduttore.ConduciAsync(dialogo)

        Dim domanda As ConduttoreDiDialogo.Battito = battiti.FirstOrDefault(
            Function(b) b.Mossa.Detto.Any(Function(d) d.Contains(DialogoProfilo.NonMiHaiDetto)))

        Assert.IsNotNull(domanda, "il dialogo ha chiesto il campo mancante")
        Assert.AreEqual("Non me lo ricordo.", domanda.Risposta, "e il conduttore ha risposto di ripiego")

        Assert.IsTrue(conduttore.Stranezze.Any(Function(s) s.Contains("campo rimasto vuoto")),
                      "annotandolo, perché il rapporto lo dica")

        Dim doveEFinita As String = finto.Chiamate.First(
            Function(c) c.Risposta.Contains("Ho preso il diploma")).Turno
        Assert.AreEqual("formazione", doveEFinita, "e la traccia non è slittata di un turno")

        Assert.IsTrue(dialogo.Finito, "il dialogo è arrivato in fondo")

    End Function

    ''' <summary>Una traccia di una battuta per turno: la persona si racconta e basta.</summary>
    Private Shared Function Traccia() As List(Of ConduttoreDiDialogo.Gruppo)

        Return New List(Of ConduttoreDiDialogo.Gruppo) From {
            Gruppo("nome", "Mi chiamo Luca Ferrari"),
            Gruppo("contatti", "luca@example.it, 333 0000000, Forlì"),
            Gruppo("patente", "No, non ce l'ho"),
            Gruppo("esperienze_formali", "Ho fatto il magazziniere alla Romagna Logistica"),
            Gruppo("esperienze_informali", "Ho fatto traslochi con un amico"),
            Gruppo("competenze", "So usare il muletto"),
            Gruppo("formazione", "Ho preso il diploma di perito all'ITIS nel 2018")}

    End Function

    Private Shared Function Gruppo(turno As String, ParamArray battute As String()) As ConduttoreDiDialogo.Gruppo

        Return New ConduttoreDiDialogo.Gruppo With {
            .Turno = turno, .Battute = battute.ToList()}

    End Function

End Class
