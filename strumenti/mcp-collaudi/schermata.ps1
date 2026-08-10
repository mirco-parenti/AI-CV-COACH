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

Add-Type -AssemblyName System.Windows.Forms, System.Drawing

Add-Type @"
using System;
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

if ($InPrimoPiano) {
    # Attenzione: SW_RESTORE su una finestra *massimizzata* la rimpicciolisce, e si
    # finirebbe per fotografare un'applicazione in una misura che l'utente non ha mai
    # scelto — con difetti di impaginazione che esistono solo nella fotografia. Si
    # ripristina solo ciò che è davvero ridotto a icona.
    if ([Finestre]::IsIconic($maniglia)) {
        [Finestre]::ShowWindow($maniglia, 9) | Out-Null   # SW_RESTORE
    }
    [Finestre]::SetForegroundWindow($maniglia) | Out-Null
    Start-Sleep -Milliseconds 400
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
Write-Output "Finestra ripresa: $larghezza x $altezza."
