# Guida l'interfaccia di TrovaLavoro da fuori, con UI Automation di Windows.
#
# Serve al server MCP di collaudo per rispondere a domande che una schermata non
# risponde: questo bottone è acceso o spento? cosa succede se lo premo? È il modo di
# provare l'applicazione vera senza chiedere a un umano di fare clic al posto mio.
#
# Gli argomenti arrivano in un file JSON, non sulla riga di comando. La ragione è stata
# pagata: il server componeva una riga per bash che poi diventava una riga per PowerShell,
# e nel doppio passaggio gli apostrofi sparivano — l'etichetta «Incolla qui il testo
# dell'annuncio» arrivava vuota. Un file non ha questi problemi, e ci passano anche i
# testi lunghi con gli a capo.
#
#   { "azione": "elenca" }
#   { "azione": "clic",        "nome": "Analizza" }
#   { "azione": "scrivi",      "nome": "Incolla qui", "testo": "..." }
#   { "azione": "scegli",      "nome": "Cerca su", "voce": "Jooble" }
#   { "azione": "scegli",      "nome": "Cerca su" }              # senza «voce»: le elenca
#   { "azione": "scegli_file", "percorso": "C:\\...\\CV.pdf" }
#   { "azione": "scegli_file", "annulla": true }
#   { "azione": "riga",        "testo": "Rossi S.p.A." }        # sceglie una riga di lista
#   { "azione": "riga",        "testo": "Rossi", "doppio": true }
#   { "azione": "riga" }                                        # senza «testo»: le elenca
#   { "azione": "rispondi",    "bottone": "No" }                # finestra di messaggio
#   { "azione": "rispondi" }                                    # senza: legge e elenca

param([Parameter(Mandatory = $true)][string]$Argomenti)

$ErrorActionPreference = "Stop"

# Due precauzioni sulla lingua, che qui è italiana e piena di accenti e virgolette basse.
# In uscita: PowerShell scriverebbe nella tabella dei caratteri di sistema, e chi legge
# dall'altra parte (Node, e poi l'assistente) si troverebbe «perch�» al posto di «perché».
# In entrata: **questo file va salvato con il BOM**, altrimenti PowerShell 5.1 lo legge
# come ANSI e succede di peggio che due accenti storti — `«$etichetta»` diventa il nome di
# variabile `$etichettaÂ`, che non esiste, e il messaggio esce senza l'etichetta.
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)

Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Windows.Forms

Add-Type @"
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class Finestre {

    public delegate bool Richiamo(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")] public static extern bool EnumWindows(Richiamo f, IntPtr l);
    [DllImport("user32.dll")] public static extern bool EnumChildWindows(IntPtr h, Richiamo f, IntPtr l);
    [DllImport("user32.dll")] public static extern int GetClassName(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] public static extern int GetDlgCtrlID(IntPtr h);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint f, uint dx, uint dy, uint d, IntPtr e);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
    [DllImport("user32.dll")] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, IntPtr l);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, string l);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr h, uint m, IntPtr w, StringBuilder l);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);

    // Il ridimensionamento: senza questo, un difetto di impaginazione che si vede solo a
    // finestra stretta non si può guardare in faccia — l'applicazione si apre massimizzata
    // e lì resta.
    [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr dopo, int x, int y, int cx, int cy, uint flag);
    [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int comando);
    [DllImport("user32.dll")] public static extern bool IsZoomed(IntPtr h);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out Rettangolo r);
    [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr h, out Rettangolo r);

    public struct Rettangolo { public int Sinistra, Alto, Destra, Basso; }

    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOZORDER = 0x0004;
    public const int SW_RESTORE = 9;
    public const int SW_MAXIMIZE = 3;

    public const uint GIU = 0x0002;   // MOUSEEVENTF_LEFTDOWN
    public const uint SU  = 0x0004;   // MOUSEEVENTF_LEFTUP
    public const uint WM_SETTEXT = 0x000C;
    public const uint WM_GETTEXT = 0x000D;
    public const uint WM_GETTEXTLENGTH = 0x000E;
    public const uint BM_CLICK = 0x00F5;

    public static string Classe(IntPtr h) { StringBuilder s = new StringBuilder(256); GetClassName(h, s, 256); return s.ToString(); }
    public static string Titolo(IntPtr h) { StringBuilder s = new StringBuilder(512); GetWindowText(h, s, 512); return s.ToString(); }
    public static int Lunghezza(IntPtr h) { return (int)SendMessage(h, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero); }

    /// <summary>
    /// Il testo che un controllo mostra adesso — anche di un altro processo. GetWindowText
    /// non serve (fuori casa restituisce solo i titoli delle finestre): WM_GETTEXT sì, ed è
    /// uno dei pochi messaggi che Windows porta di là insieme al suo buffer.
    /// </summary>
    public static string TestoDi(IntPtr h) {
        int quanti = Lunghezza(h);
        if (quanti <= 0) return "";
        StringBuilder s = new StringBuilder(quanti + 1);
        SendMessage(h, WM_GETTEXT, (IntPtr)(quanti + 1), s);
        return s.ToString();
    }

    /// <summary>La finestra di dialogo del processo, se in questo momento ce n'è una aperta.</summary>
    public static IntPtr Dialogo(uint pid) {
        IntPtr trovata = IntPtr.Zero;
        EnumWindows(delegate(IntPtr h, IntPtr l) {
            uint suo; GetWindowThreadProcessId(h, out suo);
            if (suo == pid && IsWindowVisible(h) && Classe(h) == "#32770") { trovata = h; return false; }
            return true;
        }, IntPtr.Zero);
        return trovata;
    }

    /// <summary>
    /// I pezzi di una finestra, uno per riga: «classe|numero|testo». Serve alle finestre
    /// di messaggio, dove i bottoni non hanno un numero che si possa sapere in anticipo —
    /// dipendono da quali bottoni la finestra ha — e vanno riconosciuti dal loro testo.
    /// </summary>
    public static string Figli(IntPtr padre) {
        StringBuilder tutti = new StringBuilder();
        EnumChildWindows(padre, delegate(IntPtr h, IntPtr l) {
            tutti.Append(Classe(h)).Append("|").Append(GetDlgCtrlID(h)).Append("|")
                 .Append(TestoDi(h).Replace("\r", " ").Replace("\n", " ")).Append("\n");
            return true;
        }, IntPtr.Zero);
        return tutti.ToString();
    }

    /// <summary>Una finestra figlia, cercata per classe e numero: è così che si nominano i pezzi di un dialogo.</summary>
    public static IntPtr Figlia(IntPtr padre, string classe, int id) {
        IntPtr trovata = IntPtr.Zero;
        EnumChildWindows(padre, delegate(IntPtr h, IntPtr l) {
            if (Classe(h) == classe && GetDlgCtrlID(h) == id) { trovata = h; return false; }
            return true;
        }, IntPtr.Zero);
        return trovata;
    }

    public static void Clic(int x, int y) {
        SetCursorPos(x, y);
        mouse_event(GIU, 0, 0, 0, IntPtr.Zero);
        mouse_event(SU, 0, 0, 0, IntPtr.Zero);
    }
}
"@

$scelte = [System.IO.File]::ReadAllText($Argomenti, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$azione = $scelte.azione

$processo = Get-Process -Name "TrovaLavoro" -ErrorAction SilentlyContinue |
            Where-Object { $_.MainWindowHandle -ne 0 } |
            Select-Object -First 1

if (-not $processo) {
    Write-Output "TrovaLavoro non ha una finestra aperta."
    exit 1
}

$tutti = [System.Windows.Automation.TreeScope]::Descendants
$figli = [System.Windows.Automation.TreeScope]::Children
$scrivania = [System.Windows.Automation.AutomationElement]::RootElement

# Le finestre dell'applicazione possono essere più d'una — la principale e ciò che apre.
# Attenzione però: la finestra di scelta del file **non** è fra queste. Windows la
# appende alla principale, che nell'albero se la ritrova fra i discendenti; e per giunta
# ne mostra solo la parte alta (l'elenco dei file), mai «Nome file», «Apri» e «Annulla».
# Per quella c'è l'azione «scegli_file», che passa dalle finestre native.
$condizioneProcesso = New-Object System.Windows.Automation.PropertyCondition(
    [System.Windows.Automation.AutomationElement]::ProcessIdProperty, $processo.Id)

$radici = @($scrivania.FindAll($figli, $condizioneProcesso) |
            Sort-Object { $_.Current.NativeWindowHandle } -Descending)

if ($radici.Count -eq 0) {
    $radici = @([System.Windows.Automation.AutomationElement]::FromHandle($processo.MainWindowHandle))
}

$radice = $radici[0]

<#
.SYNOPSIS
Cerca un controllo per etichetta, preferendo il tipo che si sta cercando.
.DESCRIPTION
Il tipo non è un dettaglio: un'etichetta e la casella che descrive **hanno lo stesso
nome**, perché UI Automation battezza la casella con la sua etichetta. E l'etichetta,
nell'albero, viene prima. Chiedendo «Incolla qui il testo dell'annuncio» senza dire che
si vuole una casella, si ottiene la scritta lì sopra: ci si scrive dentro senza che
niente si veda, e ci si clicca sopra senza che nessuno prenda il fuoco. Ore buttate,
la prima volta.

Per la stessa ragione il ripiego «per contenuto» pretende una **parola intera**: cercare
«Cerca» col pannello Profilo davanti trovava «🔍 Ricerca», dove quelle cinque lettere
stanno dentro un'altra parola, e premeva il bottone di navigazione — spostando
l'applicazione invece di cercare. Un pezzo di parola non è una somiglianza: è un caso.
Meglio un «non trovato», che fa guardare, di un clic sul controllo sbagliato, che fa
diagnosticare il difetto di un pannello che non era nemmeno a video.

E ci sono altri due inganni, sullo stesso schema, che fanno trovare un controllo che
esiste davvero ma non è quello che si voleva.

Il primo: **ogni menù a tendina si porta dentro un bottone**, la sua freccia, che Windows
in italiano chiama «Apri». In P3 i bottoni di nome «Apri» diventano così tre, di cui uno
solo è il bottone vero — e i due finti vengono prima. La freccia si apre con «scegli»,
non con «clic».

Il secondo, e costa caro: **la pagina web dentro P3 racconta a UI Automation tutto quello
che mostra**, e i portali hanno bottoni che si chiamano come i nostri. Con Jooble aperto,
«Cerca» erano due — quello del sito (che nell'albero viene prima) e quello
dell'applicazione — e premere il primo rifà la ricerca sul portale: l'applicazione non si
muove, l'attrezzo dice «premuto», e si finisce per cercare un difetto nel pannello che è
sano. Li si riconosce dal `FrameworkId`, che per la pagina è `Chrome`.

Perciò questi due si prendono **solo se non c'è nient'altro**, e quando succede
l'attrezzo lo dichiara (v. `$Ripiegato`): premere un bottone della pagina è legittimo —
un giorno servirà — ma chi legge deve sapere che non ha premuto un comando nostro.
#>

# Perché il controllo trovato da «Trova» non è un comando dell'applicazione; $null se lo è.
$Ripiegato = $null

function Trova([string]$Etichetta, [string[]]$TipiPreferiti = @()) {

    $esatta = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $Etichetta)

    function DelTipo($elemento) {
        if ($TipiPreferiti.Count -eq 0) { return $true }
        $suo = $elemento.Current.ControlType.ProgrammaticName -replace "ControlType\.", ""
        return $TipiPreferiti -contains $suo
    }

    # Se l'etichetta cercata è una **parola intera** dentro il nome, e non un pezzo di
    # un'altra parola. Si guarda cosa la circonda invece di usare \b, perché qui i nomi
    # cominciano con un'emoji e finiscono con i puntini di sospensione.
    function AParolaIntera([string]$Nome) {
        return $Nome -match ("(?<![\p{L}\p{N}])" + [regex]::Escape($Etichetta) + "(?![\p{L}\p{N}])")
    }

    function DiSecondaScelta($elemento) {

        # Sotto try: in una pagina viva un elemento può sparire fra il momento in cui lo si
        # trova e quello in cui gli si chiede qualcosa, e non deve far cadere la ricerca.
        try {
            if ($elemento.Current.FrameworkId -eq "Chrome") { return "dalla pagina web" }

            $padre = [System.Windows.Automation.TreeWalker]::ControlViewWalker.GetParent($elemento)
            if ($null -ne $padre -and
                $padre.Current.ControlType -eq [System.Windows.Automation.ControlType]::ComboBox) {
                return "da dentro un menù a tendina"
            }
        } catch { }

        return $null
    }

    # Quel che è di seconda scelta si tiene da parte: vale solo se non si trova nient'altro.
    $ripiego = $null
    $motivo = $null

    # Due giri: prima si guarda solo fra i tipi che servono, poi — se non c'è niente —
    # fra tutti, perché un nome giusto su un tipo inatteso è meglio di un «non trovato».
    foreach ($soloIPreferiti in @($true, $false)) {

        if ($soloIPreferiti -and $TipiPreferiti.Count -eq 0) { continue }

        foreach ($dove in $radici) {

            foreach ($candidato in $dove.FindAll($tutti, $esatta)) {
                if (-not $soloIPreferiti -or (DelTipo $candidato)) {
                    $perche = DiSecondaScelta $candidato
                    if ($perche) {
                        if (-not $ripiego) { $ripiego = $candidato; $motivo = $perche }
                    } else {
                        return $candidato
                    }
                }
            }

            # Ripiego: chi cerca «Analizza» deve trovare anche «Analizza…», e chi cerca
            # «Ricerca» l'etichetta con l'emoji davanti. Si guarda fra tutti i controlli
            # per contenuto, ma **a parola intera**.
            foreach ($candidato in $dove.FindAll($tutti,
                     [System.Windows.Automation.Condition]::TrueCondition)) {

                if ($candidato.Current.Name -notlike "*$Etichetta*") { continue }
                if (-not (AParolaIntera $candidato.Current.Name)) { continue }

                if (-not $soloIPreferiti -or (DelTipo $candidato)) {
                    $perche = DiSecondaScelta $candidato
                    if ($perche) {
                        if (-not $ripiego) { $ripiego = $candidato; $motivo = $perche }
                    } else {
                        return $candidato
                    }
                }
            }
        }
    }

    $script:Ripiegato = $motivo
    return $ripiego
}

<#
.SYNOPSIS
Avverte quando il controllo trovato non era un comando dell'applicazione.
.DESCRIPTION
Dirlo è metà del valore della regola di sopra: senza, l'attrezzo lavorerebbe lo stesso —
sul bottone sbagliato — e riferirebbe un successo. Con questa riga, chi legge sa subito
dove guardare.
#>
function DiSeEraUnRipiego([string]$Etichetta) {
    if ($Ripiegato) {
        Write-Output "Attenzione: «$Etichetta» non è un comando dell'applicazione — l'ho preso $Ripiegato."
    }
}

<#
.SYNOPSIS
Riempie gli appunti di Windows.
.DESCRIPTION
`Set-Clipboard` da solo non basta: lanciato da WSL capita che non riempia niente e non
si lamenti, e l'incolla dopo sembra un difetto dell'applicazione. Si verifica sempre, e
se è andata a vuoto si ripiega su clip.exe.
#>
function RiempiGliAppunti([string]$Testo) {

    try { Set-Clipboard -Value $Testo } catch { }

    $riletto = ""
    try { $riletto = Get-Clipboard -Raw -ErrorAction SilentlyContinue } catch { }
    if ($riletto -and $riletto.TrimEnd() -eq $Testo.TrimEnd()) { return $true }

    $Testo | clip.exe
    Start-Sleep -Milliseconds 200

    try { $riletto = Get-Clipboard -Raw -ErrorAction SilentlyContinue } catch { }
    return ($riletto -and $riletto.TrimEnd() -eq $Testo.TrimEnd())
}

<#
.SYNOPSIS
La voce che un menù a tendina mostra in questo momento.
.DESCRIPTION
Non è il suo `Name`: quello è l'etichetta accanto («Cerca su»), e resta identica
qualunque cosa si scelga. E un menù di sola lettura — `DropDownList` — non ha nemmeno il
ValuePattern, che è il primo posto dove verrebbe da guardare. Si prova nell'ordine: la
selezione, poi Windows con WM_GETTEXT, e per ultimo il ValuePattern per i menù scrivibili.
#>
function ValoreDi($elemento) {

    if (-not $elemento) { return $null }

    $schema = $null

    try {
        if ($elemento.TryGetCurrentPattern(
                [System.Windows.Automation.SelectionPattern]::Pattern, [ref]$schema)) {

            $scelte = @($schema.Current.GetSelection())
            if ($scelte.Count -gt 0 -and $scelte[0].Current.Name) { return $scelte[0].Current.Name }
        }
    } catch { }

    $manico = [IntPtr]$elemento.Current.NativeWindowHandle
    if ($manico -ne [IntPtr]::Zero) {
        $testo = [Finestre]::TestoDi($manico)
        if ($testo) { return $testo }
    }

    try {
        if ($elemento.TryGetCurrentPattern(
                [System.Windows.Automation.ValuePattern]::Pattern, [ref]$schema)) {
            return $schema.Current.Value
        }
    } catch { }

    return $null
}

<#
.SYNOPSIS
Le voci di un menù **già aperto**.
.DESCRIPTION
Chiuso, un menù di Windows non ha voci da mostrare: la tendina è una finestra a sé, che
nasce quando si apre e muore quando si sceglie. Nell'albero può comparire sotto il menù
oppure — ed è il caso dei menù veri di Windows — come finestra di primo livello del
processo, sorella della principale. Si guarda in tutte e due, ma **mai** dentro la
finestra principale: lì ci sono le voci degli *altri* elenchi, e si finirebbe per
scegliere nel menù sbagliato senza accorgersene.
#>
function VociDi($menu) {

    $soloVoci = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)

    $sue = @($menu.FindAll($tutti, $soloVoci))
    if ($sue.Count -gt 0) { return $sue }

    $principale = $radice.Current.NativeWindowHandle

    foreach ($finestra in $scrivania.FindAll($figli, $condizioneProcesso)) {

        if ($finestra.Current.NativeWindowHandle -eq $principale) { continue }

        $sue = @($finestra.FindAll($tutti, $soloVoci))
        if ($sue.Count -gt 0) { return $sue }
    }

    return @()
}

switch ($azione) {

    "elenca" {

        $condizione = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::IsOffscreenProperty, $false)

        foreach ($elemento in $radice.FindAll($tutti, $condizione)) {

            $tipo = $elemento.Current.ControlType.ProgrammaticName -replace "ControlType\.", ""
            if ($tipo -notin @("Button", "Edit", "Tab", "TabItem", "List", "Table", "ComboBox", "CheckBox", "DataItem")) { continue }

            $etichetta = $elemento.Current.Name
            if ([string]::IsNullOrWhiteSpace($etichetta)) { $etichetta = "(senza nome)" }

            # Una lista in vista dettagli non ha nome — la coda di P1 si chiama «(senza
            # nome)» — e il suo nome non è quel che serve sapere: serve sapere quante righe
            # ha, e che si sceglie con «scegli_riga».
            if ($tipo -in @("Table", "List")) {
                $quante = @($elemento.FindAll($figli, (New-Object System.Windows.Automation.PropertyCondition(
                    [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                    [System.Windows.Automation.ControlType]::ListItem)))).Count
                $etichetta = "$etichetta — $quante righe (si scelgono con «scegli_riga»)"
            }

            # Di un menù a tendina l'etichetta non basta: quella non cambia mai, e quel che
            # serve sapere è la voce che mostra adesso. Senza, l'unico modo di scoprirla
            # sarebbe guardare una fotografia.
            if ($tipo -eq "ComboBox") {
                $adesso = ValoreDi $elemento
                if ($adesso) { $etichetta = "$etichetta = $adesso" }
            }

            # Quel che sta nella pagina aperta dentro P3 finisce in questo elenco insieme ai
            # comandi dell'applicazione, e senza un segno i due si confondono: ci sono
            # portali con un bottone «Cerca» identico al nostro.
            if ($elemento.Current.FrameworkId -eq "Chrome") { $etichetta = "[pagina] $etichetta" }

            $stato = if ($elemento.Current.IsEnabled) { "acceso" } else { "SPENTO" }
            Write-Output ("{0,-8} {1,-8} {2}" -f $tipo, $stato, $etichetta)
        }

        # Una finestra aperta cambia tutto: finché non le si risponde l'applicazione non
        # ascolta nessuno, e i controlli elencati qui sopra sono di una finestra che in
        # quel momento non si lascia toccare. Le due specie si distinguono da un dettaglio
        # concreto — la scelta file ha la casella del nome (Edit 1148), una finestra di
        # messaggio no — e vogliono due attrezzi diversi: dirlo qui evita di provare
        # quello sbagliato.
        $dialogo = [Finestre]::Dialogo([uint32]$processo.Id)
        if ($dialogo -ne [IntPtr]::Zero) {
            $conCasella = [Finestre]::Figlia($dialogo, "Edit", 1148) -ne [IntPtr]::Zero
            $come = if ($conCasella) { "scegli_file" } else { "rispondi_finestra" }
            Write-Output ""
            Write-Output "C'è una finestra aperta che aspetta una risposta: «$([Finestre]::Titolo($dialogo))». Rispondile con «$come»."
        }
    }

    "clic" {

        $nome = $scelte.nome
        $elemento = Trova $nome @("Button", "TabItem", "CheckBox", "RadioButton", "ListItem", "MenuItem")
        if (-not $elemento) { Write-Output "Non ho trovato «$nome»."; exit 1 }

        DiSeEraUnRipiego $nome

        if (-not $elemento.Current.IsEnabled) {
            Write-Output "«$($elemento.Current.Name)» è SPENTO: premerlo non fa niente (ed è così che deve essere, se una condizione manca)."
            exit 0
        }

        # Si preme col mouse, non con Invoke. La ragione è pratica: i bottoni che aprono
        # una finestra modale — «Importa da un CV…», una conferma — bloccano Invoke
        # finché quella finestra non si chiude, e dopo un minuto muore di timeout COM. Un
        # clic vero non aspetta niente, ed è anche ciò che fa una persona.
        $area = $elemento.Current.BoundingRectangle
        $etichetta = $elemento.Current.Name

        if ($area.Width -gt 0 -and $area.Height -gt 0) {

            [Finestre]::SetForegroundWindow($radice.Current.NativeWindowHandle) | Out-Null
            Start-Sleep -Milliseconds 200

            [Finestre]::Clic([int]($area.X + $area.Width / 2), [int]($area.Y + $area.Height / 2))
            Start-Sleep -Milliseconds 700

            # Il nome si legge prima del clic: «Analizza» diventa «Annulla» appena il
            # lavoro parte, e riportare il nome nuovo farebbe credere di aver premuto
            # un altro bottone.
            $dialogo = [Finestre]::Dialogo([uint32]$processo.Id)
            if ($dialogo -ne [IntPtr]::Zero) {
                # Quale delle due specie di finestra sia lo dice la casella del nome file:
                # ce l'ha la scelta file, non una finestra di messaggio. Dirlo storto manda
                # a provare l'attrezzo sbagliato, e la finestra intanto blocca tutto.
                $conCasella = [Finestre]::Figlia($dialogo, "Edit", 1148) -ne [IntPtr]::Zero
                $come = if ($conCasella) { "scegli_file" } else { "rispondi_finestra" }
                Write-Output "Premuto «$etichetta»: ha aperto «$([Finestre]::Titolo($dialogo))», che ora aspetta una risposta (rispondi con «$come»)."
            } else {
                Write-Output "Premuto «$etichetta»."
            }

            exit 0
        }

        $schema = $null
        if ($elemento.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$schema)) {
            $schema.Invoke()
            Write-Output "Premuto «$etichetta» (senza passare dal mouse: non ha una posizione sullo schermo)."
            exit 0
        }

        if ($elemento.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$schema)) {
            $schema.Select()
            Write-Output "Scelto «$etichetta»."
            exit 0
        }

        Write-Output "«$nome» non si lascia premere (nessuno schema Invoke o Selection)."
        exit 1
    }

    "scrivi" {

        $nome = $scelte.nome
        $contenuto = [string]$scelte.testo

        $casella = Trova $nome @("Edit", "ComboBox", "Document")
        if (-not $casella) { Write-Output "Non ho trovato la casella «$nome»."; exit 1 }

        DiSeEraUnRipiego $nome

        $etichetta = $casella.Current.Name
        $schema = $null

        if ($casella.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$schema)) {
            $schema.SetValue($contenuto)
            Write-Output "Scritti $($contenuto.Length) caratteri in «$etichetta»."
            exit 0
        }

        # Ripiego per le caselle che non si lasciano scrivere così: si fa come una persona
        # — appunti, clic e Ctrl+V — che è anche l'unico modo di far scattare gli eventi
        # veri. (Le caselle di P4 e P2 il ValuePattern ce l'hanno, multiriga comprese:
        # quando sembrava di no, si stava scrivendo nell'etichetta al posto della casella.
        # Vedi «Trova».)
        if (-not (RiempiGliAppunti $contenuto)) {
            Write-Output "Non sono riuscita a mettere il testo negli appunti: senza quelli, in una casella multiriga non si scrive."
            exit 1
        }

        $area = $casella.Current.BoundingRectangle
        if ($area.Width -le 0 -or $area.Height -le 0) {
            Write-Output "«$etichetta» non ha una posizione sullo schermo: non ci posso cliccare dentro."
            exit 1
        }

        # Il clic serve a due cose insieme: dare il fuoco alla casella e portare avanti la
        # finestra. SetFocus di UI Automation qui non basta — Windows non lascia che un
        # processo che sta dietro si prenda lo stato attivo, e fallisce.
        [Finestre]::Clic([int]($area.X + $area.Width / 2), [int]($area.Y + $area.Height / 2))
        Start-Sleep -Milliseconds 400

        [System.Windows.Forms.SendKeys]::SendWait("^a")
        [System.Windows.Forms.SendKeys]::SendWait("^v")
        Start-Sleep -Milliseconds 800

        $dentro = [Finestre]::Lunghezza([IntPtr]$casella.Current.NativeWindowHandle)
        Write-Output "Incollati $($contenuto.Length) caratteri in «$etichetta»; nella casella ce ne sono $dentro (gli a capo contano doppio)."
    }

    "scegli" {

        $nome = $scelte.nome
        $voluta = [string]$scelte.voce

        $menu = Trova $nome @("ComboBox", "List")
        if (-not $menu) { Write-Output "Non ho trovato il menù «$nome»."; exit 1 }

        DiSeEraUnRipiego $nome

        $etichetta = $menu.Current.Name

        if (-not $menu.Current.IsEnabled) {
            Write-Output "«$etichetta» è SPENTO: non c'è niente da scegliere (ed è così che deve essere, se una condizione manca)."
            exit 0
        }

        $prima = ValoreDi $menu

        # Prima davanti, poi si apre: una tendina aperta mentre la finestra sta dietro si
        # richiude da sola nell'istante in cui la si porta avanti, e la scelta cadrebbe nel
        # vuoto senza che nessuno spieghi perché.
        [Finestre]::SetForegroundWindow($radice.Current.NativeWindowHandle) | Out-Null
        Start-Sleep -Milliseconds 250

        $apertura = $null
        $siApre = $menu.TryGetCurrentPattern(
            [System.Windows.Automation.ExpandCollapsePattern]::Pattern, [ref]$apertura)

        try {

            if ($siApre) {
                $apertura.Expand()
            } else {
                # Ripiego: la freccia sta a destra, e un menù di sola lettura si apre
                # cliccandolo in qualunque punto.
                $suo = $menu.Current.BoundingRectangle
                if ($suo.Width -le 0 -or $suo.Height -le 0) {
                    Write-Output "«$etichetta» non si apre e non ha una posizione sullo schermo: non so come guardarci dentro."
                    exit 1
                }
                [Finestre]::Clic([int]($suo.X + $suo.Width - 10), [int]($suo.Y + $suo.Height / 2))
            }

            Start-Sleep -Milliseconds 500

            $voci = @(VociDi $menu)

            if ($voci.Count -eq 0) {
                Write-Output "«$etichetta» si è aperto ma non mostra nessuna voce. Sotto di lui c'è questo:"
                foreach ($pezzo in $menu.FindAll($tutti, [System.Windows.Automation.Condition]::TrueCondition)) {
                    Write-Output ("  {0} «{1}»" -f
                        ($pezzo.Current.ControlType.ProgrammaticName -replace "ControlType\.", ""),
                        $pezzo.Current.Name)
                }
                exit 1
            }

            $nomi = @($voci | ForEach-Object { $_.Current.Name })

            # Senza «voce» l'attrezzo non sceglie niente: racconta cosa c'è dentro, con una
            # freccia sulla voce di adesso. Serve a scoprire un menù senza aprire il codice.
            if ([string]::IsNullOrEmpty($voluta)) {

                Write-Output "«$etichetta» ha $($voci.Count) voci:"
                foreach ($n in $nomi) {
                    $segno = if ($n -eq $prima) { "→" } else { " " }
                    Write-Output "  $segno $n"
                }
                exit 0
            }

            $scelta = $voci | Where-Object { $_.Current.Name -eq $voluta } | Select-Object -First 1
            if (-not $scelta) {
                $scelta = $voci | Where-Object { $_.Current.Name -like "*$voluta*" } | Select-Object -First 1
            }

            if (-not $scelta) {
                Write-Output "In «$etichetta» non c'è nessuna voce «$voluta». Ci sono: $($nomi -join " · ")."
                exit 1
            }

            $etichettaVoce = $scelta.Current.Name

            $scorrimento = $null
            if ($scelta.TryGetCurrentPattern(
                    [System.Windows.Automation.ScrollItemPattern]::Pattern, [ref]$scorrimento)) {
                try { $scorrimento.ScrollIntoView() } catch { }
                Start-Sleep -Milliseconds 200
            }

            # Si sceglie col mouse, per la stessa ragione per cui i bottoni si premono col
            # mouse: è l'unico modo che fa scattare gli eventi veri dell'applicazione. La
            # Select() dello schema è il ripiego — su un menù di Windows può limitarsi a
            # cambiare la voce mostrata senza avvisare nessuno, e allora il collaudo
            # direbbe una bugia: «scelto», con l'applicazione che non se n'è accorta.
            $area = $scelta.Current.BoundingRectangle

            if (($area.Width -gt 0) -and ($area.Height -gt 0)) {
                [Finestre]::Clic([int]($area.X + $area.Width / 2), [int]($area.Y + $area.Height / 2))

            } else {
                $selezione = $null
                if (-not $scelta.TryGetCurrentPattern(
                        [System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$selezione)) {
                    Write-Output "«$etichettaVoce» non ha una posizione sullo schermo e non si lascia scegliere."
                    exit 1
                }
                $selezione.Select()
            }

            Start-Sleep -Milliseconds 600

            # La prova non è aver cliccato: è che il menù adesso mostri quella voce.
            $dopo = ValoreDi $menu
            $da = ""
            if ($prima -and $prima -ne $dopo) { $da = " (prima c'era «$prima»)" }

            if ($dopo -eq $etichettaVoce) {
                Write-Output "Scelto «$etichettaVoce» in «$etichetta»$da."

            } elseif ($dopo) {
                Write-Output "Ho scelto «$etichettaVoce» in «$etichetta», ma il menù mostra «$dopo»: la scelta non ha attecchito."
                exit 1

            } else {
                Write-Output "Ho scelto «$etichettaVoce» in «$etichetta», ma il menù non racconta che voce mostra: da qui non posso confermarlo (guarda una schermata)."
            }

        } finally {

            # Una tendina lasciata aperta blocca tutto quello che viene dopo, e l'errore
            # sembrerebbe dell'applicazione: si richiude sempre, anche quando qui sopra è
            # andato storto qualcosa.
            if ($siApre) {
                try {
                    if ($apertura.Current.ExpandCollapseState -eq
                        [System.Windows.Automation.ExpandCollapseState]::Expanded) {
                        $apertura.Collapse()
                    }
                } catch { }
            }
        }
    }

    "scegli_file" {

        $dialogo = [Finestre]::Dialogo([uint32]$processo.Id)
        if ($dialogo -eq [IntPtr]::Zero) {
            Write-Output "Nessuna finestra di scelta file aperta: non c'è niente a cui rispondere."
            exit 1
        }

        $titolo = [Finestre]::Titolo($dialogo)

        # I pezzi di un dialogo di sistema si chiamano per numero, non per nome: la casella
        # del percorso è l'Edit 1148 (dentro il ComboBoxEx32), «Apri»/«Salva» è il bottone 1
        # e «Annulla» il 2. Per nome non si troverebbero: UI Automation, di questa finestra,
        # espone solo la parte alta.
        if ($scelte.annulla) {
            $annulla = [Finestre]::Figlia($dialogo, "Button", 2)
            if ($annulla -eq [IntPtr]::Zero) { Write-Output "Non ho trovato «Annulla» in «$titolo»."; exit 1 }
            [Finestre]::SendMessage($annulla, [Finestre]::BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
            Start-Sleep -Milliseconds 800
            Write-Output "Chiusa «$titolo» con «Annulla»."
            exit 0
        }

        $percorso = [string]$scelte.percorso
        if (-not $percorso) { Write-Output "Serve il percorso del file (in forma Windows), oppure «annulla»."; exit 1 }

        $casella = [Finestre]::Figlia($dialogo, "Edit", 1148)
        $conferma = [Finestre]::Figlia($dialogo, "Button", 1)
        if ($casella -eq [IntPtr]::Zero) { Write-Output "Non ho trovato la casella del nome file in «$titolo»."; exit 1 }
        if ($conferma -eq [IntPtr]::Zero) { Write-Output "Non ho trovato il bottone di conferma in «$titolo»."; exit 1 }

        [Finestre]::SetForegroundWindow($dialogo) | Out-Null
        Start-Sleep -Milliseconds 200

        [Finestre]::SendMessage($casella, [Finestre]::WM_SETTEXT, [IntPtr]::Zero, $percorso) | Out-Null
        Start-Sleep -Milliseconds 200
        [Finestre]::SendMessage($conferma, [Finestre]::BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        Start-Sleep -Milliseconds 900

        # Che il testo sia arrivato non si può rileggere da fuori (Windows non racconta il
        # contenuto dei controlli di un altro processo): la prova è che la finestra si chiuda.
        if ([Finestre]::Dialogo([uint32]$processo.Id) -eq [IntPtr]::Zero) {
            Write-Output "Scelto «$percorso»: «$titolo» si è chiusa e l'applicazione ha ripreso il lavoro."
        } else {
            Write-Output "«$titolo» è ancora aperta: il percorso non le è piaciuto (esiste? è il formato che chiede?)."
            exit 1
        }
    }

    "riga" {

        # Una lista in vista dettagli — la coda delle candidature di P1 — UI Automation la
        # espone come **Table senza nome**, con una ListItem per riga e dentro una Text per
        # colonna. Il nome della riga è la sola prima colonna: cercare «Rossi S.p.A.» fra i
        # nomi delle righe non troverebbe niente, perché lì c'è scritto «★☆☆☆☆ 1,0».
        # Perciò la riga si cerca **nel testo delle sue celle**.
        $voluto = [string]$scelte.testo

        $tabelle = @($radice.FindAll($tutti, (New-Object System.Windows.Automation.OrCondition(
            (New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::Table)),
            (New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::List))))) |
            Where-Object { -not $_.Current.IsOffscreen -and $_.Current.FrameworkId -ne "Chrome" })

        if ($tabelle.Count -eq 0) {
            Write-Output "Nel pannello che si vede adesso non c'è nessuna lista."
            exit 1
        }

        $lista = $tabelle[0]

        $condizioneRighe = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::ListItem)

        $righe = @($lista.FindAll($figli, $condizioneRighe))

        if ($righe.Count -eq 0) {
            Write-Output "La lista è vuota: non c'è nessuna riga da scegliere."
            exit 0
        }

        # Il testo intero di una riga: il suo nome più tutte le celle.
        function TestoDellaRiga($riga) {
            $celle = @($riga.FindAll($tutti, (New-Object System.Windows.Automation.PropertyCondition(
                [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
                [System.Windows.Automation.ControlType]::Text))) | ForEach-Object { $_.Current.Name })

            if ($celle.Count -eq 0) { return $riga.Current.Name }
            return ($celle -join " · ")
        }

        # Senza «testo» non si sceglie niente: si racconta cosa c'è, con una freccia sulla
        # riga di adesso. È il gemello di «scegli» sui menù.
        if ([string]::IsNullOrEmpty($voluto)) {

            Write-Output "La lista ha $($righe.Count) righe:"
            foreach ($r in $righe) {
                $suo = $null
                $scelta = $false
                if ($r.TryGetCurrentPattern(
                        [System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$suo)) {
                    $scelta = $suo.Current.IsSelected
                }
                $segno = if ($scelta) { "→" } else { " " }
                Write-Output "  $segno $(TestoDellaRiga $r)"
            }
            exit 0
        }

        $trovata = $righe | Where-Object { (TestoDellaRiga $_) -like "*$voluto*" } | Select-Object -First 1

        if (-not $trovata) {
            Write-Output "Nessuna riga contiene «$voluto». Ci sono:"
            foreach ($r in $righe) { Write-Output "  $(TestoDellaRiga $r)" }
            exit 1
        }

        $suoTesto = TestoDellaRiga $trovata
        $area = $trovata.Current.BoundingRectangle

        if (($area.Width -le 0) -or ($area.Height -le 0)) {
            Write-Output "La riga «$suoTesto» non ha una posizione sullo schermo: non ci posso cliccare."
            exit 1
        }

        [Finestre]::SetForegroundWindow($radice.Current.NativeWindowHandle) | Out-Null
        Start-Sleep -Milliseconds 250

        # Si sceglie col mouse per la stessa ragione dei bottoni e delle tendine: la
        # Select() dello schema cambia la selezione senza far scattare gli eventi
        # dell'applicazione, e allora il collaudo direbbe una bugia.
        $x = [int]($area.X + 40)
        $y = [int]($area.Y + $area.Height / 2)

        [Finestre]::Clic($x, $y)
        Start-Sleep -Milliseconds 400

        if ($scelte.doppio) {
            # Il doppio clic è due clic vicini: il secondo entro il tempo di sistema.
            [Finestre]::Clic($x, $y)
            Start-Sleep -Milliseconds 80
            [Finestre]::Clic($x, $y)
            Start-Sleep -Milliseconds 900
            Write-Output "Doppio clic sulla riga «$suoTesto»."
            exit 0
        }

        # La prova non è aver cliccato: è che la riga adesso risulti scelta.
        $stato = $null
        if ($trovata.TryGetCurrentPattern(
                [System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$stato)) {

            if ($stato.Current.IsSelected) {
                Write-Output "Scelta la riga «$suoTesto»."
            } else {
                Write-Output "Ho cliccato la riga «$suoTesto», ma non risulta scelta: la scelta non ha attecchito."
                exit 1
            }
        } else {
            Write-Output "Cliccata la riga «$suoTesto» (non racconta se è scelta: da qui non posso confermarlo)."
        }
    }

    "rispondi" {

        # Una finestra di messaggio è una finestra di dialogo come quella di scelta file
        # (classe #32770), e come quella non si pilota con UI Automation: si passa dalle
        # finestre native. La differenza è che i suoi bottoni **non hanno un numero che si
        # possa sapere in anticipo** — dipendono da quali bottoni la finestra mostra — e si
        # riconoscono dal testo.
        $finestra = [Finestre]::Dialogo([uint32]$processo.Id)

        if ($finestra -eq [IntPtr]::Zero) {
            Write-Output "Nessuna finestra aperta che aspetti una risposta."
            exit 1
        }

        $titolo = [Finestre]::Titolo($finestra)
        $pezzi = @([Finestre]::Figli($finestra) -split "`n" | Where-Object { $_ })

        $bottoni = @()
        $messaggio = @()

        foreach ($pezzo in $pezzi) {
            $parti = $pezzo -split "\|", 3
            if ($parti.Count -lt 3) { continue }

            if ($parti[0] -eq "Button") {
                $bottoni += [pscustomobject]@{ Id = [int]$parti[1]; Testo = $parti[2].Replace("&", "") }
            } elseif ($parti[0] -eq "Static" -and $parti[2].Trim()) {
                $messaggio += $parti[2].Trim()
            }
        }

        if ($bottoni.Count -eq 0) {
            Write-Output "«$titolo» non ha bottoni: non è una finestra di messaggio (una scelta file si chiude con «scegli_file»)."
            exit 1
        }

        $voluto = [string]$scelte.bottone

        # Senza «bottone» non si risponde: si legge cosa chiede e che scelte dà. È il modo
        # di sapere cosa si sta per confermare invece di premere al buio.
        if ([string]::IsNullOrEmpty($voluto)) {
            Write-Output "«$titolo» chiede: $($messaggio -join " ")"
            Write-Output "Bottoni: $(($bottoni | ForEach-Object { $_.Testo }) -join " · ")"
            exit 0
        }

        $scelto = $bottoni | Where-Object { $_.Testo -eq $voluto } | Select-Object -First 1
        if (-not $scelto) {
            $scelto = $bottoni | Where-Object { $_.Testo -like "*$voluto*" } | Select-Object -First 1
        }

        if (-not $scelto) {
            Write-Output "In «$titolo» non c'è nessun bottone «$voluto». Ci sono: $(($bottoni | ForEach-Object { $_.Testo }) -join " · ")."
            exit 1
        }

        $suo = [Finestre]::Figlia($finestra, "Button", $scelto.Id)
        if ($suo -eq [IntPtr]::Zero) {
            Write-Output "Non ho ritrovato il bottone «$($scelto.Testo)» in «$titolo»."
            exit 1
        }

        [Finestre]::SetForegroundWindow($finestra) | Out-Null
        Start-Sleep -Milliseconds 200
        [Finestre]::SendMessage($suo, [Finestre]::BM_CLICK, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        Start-Sleep -Milliseconds 900

        # La prova è che la finestra si sia chiusa: finché è lì, l'applicazione non ascolta
        # nessun altro e il comando dopo mentirebbe.
        if ([Finestre]::Dialogo([uint32]$processo.Id) -eq [IntPtr]::Zero) {
            Write-Output "Risposto «$($scelto.Testo)» a «$titolo», che si è chiusa."
        } else {
            Write-Output "Ho premuto «$($scelto.Testo)», ma «$titolo» è ancora aperta."
            exit 1
        }
    }

    "ridimensiona" {

        # Perché serve: i difetti di impaginazione che contano si vedono **stretti**. La
        # finestra ha una MinimumSize dichiarata (1150x600, cap. 03.4), e Windows non
        # scende sotto di quella: la misura che si ottiene va perciò sempre riletta, mai
        # data per fatta. E per rimpicciolire una finestra massimizzata bisogna prima
        # ripristinarla, altrimenti SetWindowPos non muove niente.
        $finestra = $processo.MainWindowHandle

        if ($scelte.massimizza) {
            [Finestre]::ShowWindow($finestra, [Finestre]::SW_MAXIMIZE) | Out-Null
        } else {

            if (-not $scelte.larghezza -or -not $scelte.altezza) {
                Write-Output "Servono «larghezza» e «altezza» (oppure «massimizza»)."
                exit 1
            }

            if ([Finestre]::IsZoomed($finestra)) {
                [Finestre]::ShowWindow($finestra, [Finestre]::SW_RESTORE) | Out-Null
                Start-Sleep -Milliseconds 300
            }

            [Finestre]::SetWindowPos($finestra, [IntPtr]::Zero, 0, 0,
                                     [int]$scelte.larghezza, [int]$scelte.altezza,
                                     [Finestre]::SWP_NOMOVE -bor [Finestre]::SWP_NOZORDER) | Out-Null
        }

        # Il tempo di rifare l'impaginazione: senza, si fotografa la finestra a metà strada.
        Start-Sleep -Milliseconds 500

        $fuori = New-Object Finestre+Rettangolo
        $dentro = New-Object Finestre+Rettangolo
        [Finestre]::GetWindowRect($finestra, [ref]$fuori) | Out-Null
        [Finestre]::GetClientRect($finestra, [ref]$dentro) | Out-Null

        $larga = $fuori.Destra - $fuori.Sinistra
        $alta = $fuori.Basso - $fuori.Alto
        $dentroLarga = $dentro.Destra - $dentro.Sinistra
        $dentroAlta = $dentro.Basso - $dentro.Alto

        Write-Output "Finestra $larga x $alta (area utile $dentroLarga x $dentroAlta)."

        if (-not $scelte.massimizza -and $larga -gt [int]$scelte.larghezza) {
            Write-Output "Più larga di quanto chiesto: è la MinimumSize dichiarata dall'applicazione."
        }
    }

    default {
        Write-Output "Azione sconosciuta: «$azione»."
        exit 1
    }
}
