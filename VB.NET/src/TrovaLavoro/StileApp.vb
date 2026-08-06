Imports System.Drawing

''' <summary>
''' Token di design dell'applicazione (cap. 03.2): colori, font, spaziature.
''' Tutta l'interfaccia pesca da qui e solo da qui: nei form non compaiono mai
''' Color.FromArgb né New Font.
''' </summary>
Public Module StileApp

    ' --- Colori (cap. 03.2) ---

    ''' <summary>Testo normale, valori, titoli di sezione.</summary>
    Public ReadOnly TestoPrimario As Color = ColorTranslator.FromHtml("#212529")

    ''' <summary>Didascalie, suggerimenti, stati.</summary>
    Public ReadOnly TestoSecondario As Color = ColorTranslator.FromHtml("#6C757D")

    ''' <summary>Sfondo delle finestre.</summary>
    Public ReadOnly SfondoBase As Color = ColorTranslator.FromHtml("#F8F9FA")

    ''' <summary>Aree di lavoro: testi, anteprime, input.</summary>
    Public ReadOnly SfondoContenuto As Color = ColorTranslator.FromHtml("#FFFFFF")

    ''' <summary>Separatori e bordi da 1 px.</summary>
    Public ReadOnly BordoLeggero As Color = ColorTranslator.FromHtml("#DEE2E6")

    ''' <summary>Bordo dei controlli interattivi.</summary>
    Public ReadOnly BordoForte As Color = ColorTranslator.FromHtml("#CED4DA")

    ''' <summary>Focus, link, selezione (blu profondo).</summary>
    Public ReadOnly Accento As Color = ColorTranslator.FromHtml("#0B06B0")

    ''' <summary>Riga selezionata, hover.</summary>
    Public ReadOnly AccentoTenue As Color = ColorTranslator.FromHtml("#E4E7FB")

    ''' <summary>Fondo del bottone d'azione principale del pannello.</summary>
    Public ReadOnly FondoAzione As Color = ColorTranslator.FromHtml("#C0E8FF")

    ''' <summary>Titoli delle finestre e dei GroupBox, marker.</summary>
    Public ReadOnly RossoTitoli As Color = ColorTranslator.FromHtml("#FA0825")

    ''' <summary>Azioni sicure/positive, badge OK.</summary>
    Public ReadOnly Successo As Color = ColorTranslator.FromHtml("#28A745")

    ''' <summary>Azioni che modificano, badge attenzione.</summary>
    Public ReadOnly Avviso As Color = ColorTranslator.FromHtml("#FFC107")

    ''' <summary>Azioni distruttive, badge errore.</summary>
    Public ReadOnly Pericolo As Color = ColorTranslator.FromHtml("#DC3545")

    ''' <summary>Badge informativi.</summary>
    Public ReadOnly Informazione As Color = ColorTranslator.FromHtml("#17A2B8")

    ' --- Font (cap. 03.2) ---

    ''' <summary>Un solo font in tutta l'applicazione.</summary>
    Public Const NomeFont As String = "Segoe UI"

    ''' <summary>Font dei soli dati tecnici.</summary>
    Public Const NomeFontTecnico As String = "Consolas"

    ''' <summary>Titolo di finestra o di pannello (con RossoTitoli).</summary>
    Public ReadOnly FontTitoloFinestra As New Font(NomeFont, 16.0F, FontStyle.Bold)

    ''' <summary>Titolo di pannello più contenuto (con RossoTitoli).</summary>
    Public ReadOnly FontTitoloPannello As New Font(NomeFont, 14.0F, FontStyle.Bold)

    ''' <summary>Titolo di GroupBox (con RossoTitoli).</summary>
    Public ReadOnly FontTitoloGruppo As New Font(NomeFont, 9.0F, FontStyle.Bold)

    ''' <summary>Bottone d'azione principale del pannello (livello 3).</summary>
    Public ReadOnly FontAzionePrincipale As New Font(NomeFont, 9.75F, FontStyle.Bold)

    ''' <summary>Testo di lavoro e bottoni neutri.</summary>
    Public ReadOnly FontTesto As New Font(NomeFont, 9.0F)

    ''' <summary>Didascalie e suggerimenti (con TestoSecondario).</summary>
    Public ReadOnly FontDidascalia As New Font(NomeFont, 8.0F)

    ''' <summary>Punteggi, log e altri dati tecnici.</summary>
    Public ReadOnly FontDatiTecnici As New Font(NomeFontTecnico, 8.5F)

    ' --- Spaziature e dimensioni: regola 14 / 12 / 8 (cap. 03.2) ---

    ''' <summary>Margine interno di GroupBox e riquadri.</summary>
    Public Const MargineRiquadro As Integer = 14

    ''' <summary>Distanza tra controlli affiancati.</summary>
    Public Const DistanzaControlli As Integer = 12

    ''' <summary>Distanza minima tra le righe.</summary>
    Public Const InterlineaMinima As Integer = 8

    ''' <summary>Bottone standard, testo breve.</summary>
    Public ReadOnly BottoneStandard As New Size(110, 32)

    ''' <summary>Bottone standard, testo medio.</summary>
    Public ReadOnly BottoneMedio As New Size(130, 32)

    ''' <summary>Bottone della barra superiore di navigazione.</summary>
    Public ReadOnly BottoneBarraSuperiore As New Size(110, 34)

End Module
