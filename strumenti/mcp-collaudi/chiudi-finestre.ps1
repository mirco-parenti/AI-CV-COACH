# Chiude le FINESTRE di TrovaLavoro, e solo quelle.
#
# Il server MCP del prodotto e' un altro TrovaLavoro.exe (riga di comando --mcp):
# un "taskkill /IM TrovaLavoro.exe" li prende tutti e due, e si porta via i tool
# con cui si sta lavorando. Qui si guarda la riga di comando prima di premere; se
# non si riesce a leggerla, il processo si lascia stare.
#
# Stampa quante ne ha chiuse.

$ErrorActionPreference = 'SilentlyContinue'
$chiuse = 0

Get-CimInstance Win32_Process -Filter "name='TrovaLavoro.exe'" |
    Where-Object { $_.CommandLine -and $_.CommandLine -notlike '*--mcp*' } |
    ForEach-Object {
        $processo = Get-Process -Id $_.ProcessId
        if ($processo) {
            $processo.CloseMainWindow() | Out-Null
            Start-Sleep -Milliseconds 400
            if (-not $processo.HasExited) { Stop-Process -Id $_.ProcessId -Force }
            $chiuse = $chiuse + 1
        }
    }

Write-Output $chiuse
