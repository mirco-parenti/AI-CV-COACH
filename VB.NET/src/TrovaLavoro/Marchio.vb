Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Windows.Forms

''' <summary>
''' Il marchio dentro l'eseguibile (cap. 13.5): l'icona che veste l'exe e ogni finestra,
''' e la schermata di avvio dei primi istanti (cap. 03.4). Vivono come risorse
''' incorporate, come il pool (cap. 04.2) e i modelli dei documenti (cap. 05.4): accanto
''' all'exe non resta nessun file.
''' </summary>
''' <remarks>
''' <para><b>Perché l'icona sta anche qui, e non solo in <c>&lt;ApplicationIcon&gt;</c>.</b>
''' Quella dichiarazione veste l'eseguibile — l'icona che si vede in Esplora risorse e
''' sulla barra delle applicazioni — ma le finestre di WinForms non la ereditano: senza
''' un <c>Icon</c> esplicito mostrano quella di sistema, e l'applicazione si troverebbe
''' con due facce diverse. Lo stesso file entra perciò due volte, come icona dell'exe e
''' come risorsa, e resta un file solo nel repository.</para>
''' <para><b>Se una risorsa manca, non si muore.</b> Un marchio che non si carica è un
''' guaio estetico, non un guaio di dati: l'applicazione parte lo stesso, con l'icona di
''' sistema e senza schermata di avvio. A pretendere che ci siano è il banco, che è il
''' posto giusto per accorgersi di una risorsa dimenticata nel progetto — non l'avvio
''' sulla macchina di chi cerca lavoro.</para>
''' <para>Le due immagini si caricano una volta sola e restano in piedi per tutta la
''' sessione: l'icona la chiede ogni finestra che si apre.</para>
''' </remarks>
Public Module Marchio

    ''' <summary>Il percorso con cui le risorse sono state incorporate (v. il .vbproj).</summary>
    Private Const PrefissoRisorsa As String = "marchio/"

    ''' <summary>
    ''' Il lucchetto delle due immagini. Non è prudenza a caso: la bandiera «già
    ''' caricata» e l'immagine sono due cose distinte, e chi arrivasse fra l'una e
    ''' l'altra troverebbe la porta chiusa e la stanza vuota — cioè <c>Nothing</c> al
    ''' posto del marchio. Nell'applicazione le legge il solo filo dell'interfaccia,
    ''' ma il banco no: là il difetto è uscito subito, come un collaudo che cade una
    ''' volta ogni tanto senza che nel prodotto sia cambiato niente.
    ''' </summary>
    Private ReadOnly _lucchetto As New Object()

    Private _icona As Icon
    Private _iconaCaricata As Boolean
    Private _schermata As Image
    Private _schermataCaricata As Boolean
    Private _sfondoMenu As Image
    Private _sfondoMenuCaricato As Boolean

    ''' <summary>
    ''' L'icona dell'applicazione, o <c>Nothing</c> se la risorsa non c'è.
    ''' </summary>
    Public ReadOnly Property Icona As Icon
        Get
            SyncLock _lucchetto

                If Not _iconaCaricata Then
                    Using flusso As Stream = Risorsa("TrovaLavoro.ico")
                        If flusso IsNot Nothing Then _icona = New Icon(flusso)
                    End Using
                    ' La bandiera si alza a lavoro finito, mai prima.
                    _iconaCaricata = True
                End If

                Return _icona

            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' La schermata di avvio (800x702), o <c>Nothing</c> se la risorsa non c'è.
    ''' </summary>
    ''' <remarks>
    ''' Torna sempre la stessa immagine, che non va liberata da chi la riceve: la
    ''' mostrano la schermata di avvio e la finestra «Informazioni su…», e sono due usi
    ''' che possono capitare nella stessa sessione.
    ''' </remarks>
    Public ReadOnly Property SchermataDiAvvio As Image
        Get
            SyncLock _lucchetto

                If Not _schermataCaricata Then
                    Using flusso As Stream = Risorsa("schermata-avvio.png")
                        ' Bitmap tiene aperto il flusso da cui nasce: se ne fa una copia,
                        ' altrimenti disegnarla dopo la chiusura del using darebbe errore.
                        If flusso IsNot Nothing Then
                            Using originale As New Bitmap(flusso)
                                _schermata = New Bitmap(originale)
                            End Using
                        End If
                    End Using
                    ' La bandiera si alza a lavoro finito, mai prima.
                    _schermataCaricata = True
                End If

                Return _schermata

            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Lo sfondo del menu (1536x1348), o <c>Nothing</c> se la risorsa non c'è.
    ''' </summary>
    ''' <remarks>
    ''' È il banner intero, <b>identico</b> al file dei definitivi: il velo che lo
    ''' schiarisce dietro ai bottoni lo dipinge <see cref="PannelloMenu"/> a video, e
    ''' non è cotto dentro l'immagine. Così il marchio nel repository resta uno solo, e
    ''' cambiare quanto è chiaro il fondo non vuol dire rigenerare un PNG.
    ''' Come le altre, torna sempre la stessa immagine e non va liberata da chi la riceve.
    ''' </remarks>
    Public ReadOnly Property SfondoDelMenu As Image
        Get
            SyncLock _lucchetto

                If Not _sfondoMenuCaricato Then
                    Using flusso As Stream = Risorsa("sfondo-menu.png")
                        ' Come sopra: Bitmap tiene aperto il flusso da cui nasce.
                        If flusso IsNot Nothing Then
                            Using originale As New Bitmap(flusso)
                                _sfondoMenu = New Bitmap(originale)
                            End Using
                        End If
                    End Using
                    ' La bandiera si alza a lavoro finito, mai prima.
                    _sfondoMenuCaricato = True
                End If

                Return _sfondoMenu

            End SyncLock
        End Get
    End Property

    ''' <summary>
    ''' Mette l'icona dell'applicazione a una finestra. Non fa niente se l'icona non
    ''' c'è: <see cref="Form.Icon"/> a <c>Nothing</c> toglierebbe anche quella di
    ''' sistema, lasciando la barra del titolo spoglia.
    ''' </summary>
    ''' <remarks>
    ''' La chiama la sola <c>FormPrincipale</c>, e non è una dimenticanza: tutte le
    ''' finestre secondarie sono <see cref="FormBorderStyle.FixedDialog"/>, e Windows
    ''' l'icona in quella cornice non la disegna affatto. Vestirle sarebbe codice che
    ''' non si vede — se un giorno una di loro diventerà ridimensionabile, allora sì.
    ''' </remarks>
    Public Sub Vesti(finestra As Form)
        Dim nostra As Icon = Icona
        If nostra IsNot Nothing Then finestra.Icon = nostra
    End Sub

    ''' <summary>Il flusso di una risorsa incorporata, o <c>Nothing</c> se non c'è.</summary>
    Public Function Risorsa(nome As String) As Stream
        Dim assembly As Assembly = GetType(Versione).Assembly
        Return assembly.GetManifestResourceStream(PrefissoRisorsa & nome)
    End Function

End Module
