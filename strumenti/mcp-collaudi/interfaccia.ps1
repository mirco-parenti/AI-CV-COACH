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
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);

    public const uint GIU = 0x0002;   // MOUSEEVENTF_LEFTDOWN
    public const uint SU  = 0x0004;   // MOUSEEVENTF_LEFTUP
    public const uint WM_SETTEXT = 0x000C;
    public const uint WM_GETTEXTLENGTH = 0x000E;
    public const uint BM_CLICK = 0x00F5;

    public static string Classe(IntPtr h) { StringBuilder s = new StringBuilder(256); GetClassName(h, s, 256); return s.ToString(); }
    public static string Titolo(IntPtr h) { StringBuilder s = new StringBuilder(512); GetWindowText(h, s, 512); return s.ToString(); }
    public static int Lunghezza(IntPtr h) { return (int)SendMessage(h, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero); }

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
#>
function Trova([string]$Etichetta, [string[]]$TipiPreferiti = @()) {

    $esatta = New-Object System.Windows.Automation.PropertyCondition(
        [System.Windows.Automation.AutomationElement]::NameProperty, $Etichetta)

    function DelTipo($elemento) {
        if ($TipiPreferiti.Count -eq 0) { return $true }
        $suo = $elemento.Current.ControlType.ProgrammaticName -replace "ControlType\.", ""
        return $TipiPreferiti -contains $suo
    }

    # Due giri: prima si guarda solo fra i tipi che servono, poi — se non c'è niente —
    # fra tutti, perché un nome giusto su un tipo inatteso è meglio di un «non trovato».
    foreach ($soloIPreferiti in @($true, $false)) {

        if ($soloIPreferiti -and $TipiPreferiti.Count -eq 0) { continue }

        foreach ($dove in $radici) {

            foreach ($candidato in $dove.FindAll($tutti, $esatta)) {
                if (-not $soloIPreferiti -or (DelTipo $candidato)) { return $candidato }
            }

            # Ripiego: chi cerca «Analizza» deve trovare anche «Analizza…» o un'etichetta
            # con un'emoji davanti. Si guarda fra tutti i controlli, per contenuto.
            foreach ($candidato in $dove.FindAll($tutti,
                     [System.Windows.Automation.Condition]::TrueCondition)) {
                if ($candidato.Current.Name -notlike "*$Etichetta*") { continue }
                if (-not $soloIPreferiti -or (DelTipo $candidato)) { return $candidato }
            }
        }
    }

    return $null
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

switch ($azione) {

    "elenca" {

        $condizione = New-Object System.Windows.Automation.PropertyCondition(
            [System.Windows.Automation.AutomationElement]::IsOffscreenProperty, $false)

        foreach ($elemento in $radice.FindAll($tutti, $condizione)) {

            $tipo = $elemento.Current.ControlType.ProgrammaticName -replace "ControlType\.", ""
            if ($tipo -notin @("Button", "Edit", "Tab", "TabItem", "List", "ComboBox", "CheckBox", "DataItem")) { continue }

            $etichetta = $elemento.Current.Name
            if ([string]::IsNullOrWhiteSpace($etichetta)) { $etichetta = "(senza nome)" }

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
