# Cattura una schermata e la salva in PNG.
#
# Serve al server MCP di collaudo: è il modo in cui l'assistente guarda in faccia
# l'applicazione invece di indovinare cosa mostra. Riprende la sola finestra di
# TrovaLavoro, oppure tutto il desktop con -SchermoIntero.

param(
    [Parameter(Mandatory = $true)][string]$Percorso,
    [switch]$SchermoIntero,
    [switch]$InPrimoPiano
)

$ErrorActionPreference = "Stop"

# Come in «interfaccia.ps1»: senza questa riga PowerShell scrive nella tabella dei
# caratteri di sistema, e chi legge dall'altra parte trova «perch�» al posto di «perché».
# Qui non si era mai vista perché lo script non aveva mai scritto una parola accentata —
# l'ha stanata la falsificazione dell'avvertenza sul primo piano, il 2026-08-29. (Anche
# questo file, come l'altro, va salvato **con il BOM**: PowerShell 5.1 senza quello lo
# legge come ANSI.)
[Console]::OutputEncoding = New-Object System.Text.UTF8Encoding($false)

Add-Type -AssemblyName System.Windows.Forms, System.Drawing

Add-Type @"
using System;
using System.Text;
using System.Runtime.InteropServices;

public class Finestre {
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    public static extern bool IsIconic(IntPtr hWnd);

    // Le tre che servono a sapere se il primo piano l'abbiamo ottenuto davvero. Senza,
    // si fotografa «quel che sta davanti» credendo che sia l'applicazione.
    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);

    [DllImport("user32.dll")]
    public static extern void keybd_event(byte tasto, byte scansione, uint flag, IntPtr extra);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder testo, int quanti);

    public static uint ProcessoDi(IntPtr h) { uint suo; GetWindowThreadProcessId(h, out suo); return suo; }

    public static string Titolo(IntPtr h) {
        StringBuilder s = new StringBuilder(512);
        GetWindowText(h, s, 512);
        return s.ToString();
    }

    /// <summary>Un colpo di ALT: senza, Windows rifiuta il primo piano a chi non ce l'ha già.</summary>
    public static void ColpoDiAlt() {
        keybd_event(0x12, 0, 0, IntPtr.Zero);
        keybd_event(0x12, 0, 0x0002, IntPtr.Zero);
    }
}
"@

function Salva-Immagine([System.Drawing.Rectangle]$Area) {
    $immagine = New-Object System.Drawing.Bitmap($Area.Width, $Area.Height)
    $tela = [System.Drawing.Graphics]::FromImage($immagine)
    $tela.CopyFromScreen($Area.Location, [System.Drawing.Point]::Empty, $Area.Size)
    $immagine.Save($Percorso, [System.Drawing.Imaging.ImageFormat]::Png)
    $tela.Dispose()
    $immagine.Dispose()
}

if ($SchermoIntero) {
    Salva-Immagine ([System.Windows.Forms.SystemInformation]::VirtualScreen)
    Write-Output "Desktop ripreso."
    exit 0
}

$processo = Get-Process -Name "TrovaLavoro" -ErrorAction SilentlyContinue |
            Where-Object { $_.MainWindowHandle -ne 0 } |
            Select-Object -First 1

if (-not $processo) {
    # Nessuna finestra dell'applicazione: si riprende il desktop, così si vede
    # comunque cosa c'è (magari una finestra d'errore che non è la sua).
    Salva-Immagine ([System.Windows.Forms.SystemInformation]::VirtualScreen)
    Write-Output "TrovaLavoro non ha una finestra aperta: ho ripreso il desktop."
    exit 0
}

$maniglia = $processo.MainWindowHandle

# Chi è rimasto davanti nonostante il tentativo: si dice nella risposta, perché una
# fotografia che ritrae la finestra sbagliata sembra un difetto dell'applicazione.
$restatoDavanti = $null

if ($InPrimoPiano) {
    # Attenzione: SW_RESTORE su una finestra *massimizzata* la rimpicciolisce, e si
    # finirebbe per fotografare un'applicazione in una misura che l'utente non ha mai
    # scelto — con difetti di impaginazione che esistono solo nella fotografia. Si
    # ripristina solo ciò che è davvero ridotto a icona.
    if ([Finestre]::IsIconic($maniglia)) {
        [Finestre]::ShowWindow($maniglia, 9) | Out-Null   # SW_RESTORE
    }

    # Il primo piano si chiede e poi si **verifica**: `SetForegroundWindow` da sola
    # Windows la rifiuta a un processo che non è già davanti, e chi chiamava questo
    # script si ritrovava fotografato il terminale credendo di guardare l'applicazione.
    # Il rimedio noto era chiamare due volte; adesso lo fa lo script, col colpo di ALT
    # che scioglie il rifiuto — e se non ci riesce lo **dichiara**, invece di consegnare
    # una fotografia muta di cui non si sa di chi sia.
    for ($colpo = 1; $colpo -le 3; $colpo++) {

        $davanti = [Finestre]::GetForegroundWindow()
        if ($davanti -ne [IntPtr]::Zero -and [Finestre]::ProcessoDi($davanti) -eq [uint32]$processo.Id) {
            $restatoDavanti = $null
            break
        }

        # L'ALT si dà solo quando davanti c'è qualcun altro: all'applicazione già in primo
        # piano quel tasto aprirebbe la barra dei menù.
        [Finestre]::ColpoDiAlt()
        [Finestre]::SetForegroundWindow($maniglia) | Out-Null
        Start-Sleep -Milliseconds (200 * $colpo)

        $davanti = [Finestre]::GetForegroundWindow()
        if ($davanti -ne [IntPtr]::Zero -and [Finestre]::ProcessoDi($davanti) -eq [uint32]$processo.Id) {
            $restatoDavanti = $null
        } else {
            $restatoDavanti = [Finestre]::Titolo($davanti)
        }
    }
}

$bordi = New-Object Finestre+RECT
[Finestre]::GetWindowRect($maniglia, [ref]$bordi) | Out-Null

$larghezza = $bordi.Right - $bordi.Left
$altezza = $bordi.Bottom - $bordi.Top

if ($larghezza -le 0 -or $altezza -le 0) {
    Salva-Immagine ([System.Windows.Forms.SystemInformation]::VirtualScreen)
    Write-Output "La finestra non ha una misura leggibile: ho ripreso il desktop."
    exit 0
}

Salva-Immagine (New-Object System.Drawing.Rectangle($bordi.Left, $bordi.Top, $larghezza, $altezza))

if ($restatoDavanti) {
    Write-Output "ATTENZIONE: non sono riuscita a portare TrovaLavoro davanti — lì c'è rimasta «$restatoDavanti». La fotografia ritrae quel rettangolo dello schermo, quindi può mostrare quella finestra al posto dell'applicazione: guarda «di chi» è, prima di trarne conclusioni."
}

Write-Output "Finestra ripresa: $larghezza x $altezza."
