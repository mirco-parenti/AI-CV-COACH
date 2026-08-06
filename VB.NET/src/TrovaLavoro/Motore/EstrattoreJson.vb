Imports System.Text.Json
Imports System.Text.Json.Nodes
Imports System.Text.RegularExpressions

Namespace Motore

    ''' <summary>
    ''' Estrae il JSON dalla risposta di un modello. È la trascrizione fedele di
    ''' <c>estraiJson</c> del prototipo (HTML+JS/server.js): stessa strategia in due
    ''' tempi — prima il percorso felice, e solo se fallisce il ripiego che ritaglia
    ''' il JSON dalla prosa che lo circonda (Step 1.35 del diario).
    ''' </summary>
    ''' <remarks>
    ''' Il percorso felice non si tocca: gli input ben formati fanno esattamente ciò
    ''' che facevano nel prototipo, e il recupero scatta solo dentro il Catch.
    ''' </remarks>
    Public Module EstrattoreJson

        ' Recinto markdown di apertura (``` oppure ```json) e di chiusura.
        ' Attenzione agli ancoraggi: in JavaScript "^" e "$" delimitano la stringa,
        ' mentre in .NET "$" accetta anche un "\n" finale. Gli equivalenti esatti
        ' delle due regex del prototipo sono quindi "\A" e "\z", non "^" e "$".
        Private ReadOnly RecintoApertura As New Regex("\A```(?:json)?\s*",
                                                      RegexOptions.IgnoreCase Or RegexOptions.Compiled)

        Private ReadOnly RecintoChiusura As New Regex("\s*```\z",
                                                      RegexOptions.IgnoreCase Or RegexOptions.Compiled)

        ''' <summary>
        ''' Toglie l'eventuale recinto markdown e restituisce il JSON interpretato.
        ''' </summary>
        ''' <param name="testo">La risposta grezza del modello.</param>
        ''' <returns>
        ''' L'albero JSON; <c>Nothing</c> se il testo è il letterale <c>null</c>,
        ''' esattamente come <c>JSON.parse("null")</c> nel prototipo.
        ''' </returns>
        ''' <exception cref="JsonException">
        ''' Se il testo non contiene JSON valido nemmeno dopo il ripiego. Viene
        ''' rilanciato l'errore del percorso felice, come fa il prototipo.
        ''' </exception>
        Public Function Estrai(testo As String) As JsonNode

            If testo Is Nothing Then Throw New ArgumentNullException(NameOf(testo))

            ' Le tre ripuliture del prototipo, nello stesso ordine. Il conteggio a 1
            ' replica il comportamento di String.replace senza il flag "g".
            Dim pulito As String = testo.Trim()
            pulito = RecintoApertura.Replace(pulito, String.Empty, 1)
            pulito = RecintoChiusura.Replace(pulito, String.Empty, 1)

            Try
                Return JsonNode.Parse(pulito)
            Catch primoErrore As JsonException
                ' Ripiego: il modello può aver premesso (o aggiunto) prosa attorno al
                ' JSON ("si mette a spiegare"). Ritaglio dal primo '{' all'ultimo '}'
                ' e riprovo; se neanche così è valido, rilancio l'errore originale.
                Dim inizio As Integer = pulito.IndexOf("{"c)
                Dim fine As Integer = pulito.LastIndexOf("}"c)

                If inizio <> -1 AndAlso fine > inizio Then
                    Return JsonNode.Parse(pulito.Substring(inizio, fine - inizio + 1))
                End If

                Throw
            End Try

        End Function

    End Module

End Namespace
