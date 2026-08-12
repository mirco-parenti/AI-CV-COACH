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
            if ($tipo -notin @("Button", "Edit", "Tab", "TabItem", "List", "ComboBox", "CheckBox", "DataItem")) { continue }

            $etichetta = $elemento.Current.Name
            if ([string]::IsNullOrWhiteSpace($etichetta)) { $etichetta = "(senza nome)" }

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

        # Una finestra di scelta file aperta cambia tutto: finché non le si risponde
        # l'applicazione non ascolta nessuno, e i controlli elencati qui sopra sono di
        # una finestra che in quel momento non si lascia toccare.
        $dialogo = [Finestre]::Dialogo([uint32]$processo.Id)
        if ($dialogo -ne [IntPtr]::Zero) {
            Write-Output ""
            Write-Output "C'è una finestra aperta che aspetta una risposta: «$([Finestre]::Titolo($dialogo))». Rispondile con «scegli_file»."
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
                Write-Output "Premuto «$etichetta»: ha aperto «$([Finestre]::Titolo($dialogo))», che ora aspetta una risposta (rispondi con «scegli_file»)."
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

    default {
        Write-Output "Azione sconosciuta: «$azione»."
        exit 1
    }
}
