@echo off
REM Lanciatore per mostrare TrovaLavoro dal vivo, senza passare da Claude Code.
REM
REM Serve dove una chiave cifrata non e' mai stata salvata: dal 2026-08-14 (T6)
REM l'applicazione tiene la sua in segreti.bin dentro la cartella dati, e il file
REM viene prima della variabile d'ambiente ANTHROPIC_API_KEY. Su una cartella dati
REM nuova, pero', quel file non c'e' — e con un doppio clic sull'exe non c'e'
REM nemmeno la variabile: tutto cio' che passa dall'AI si ferma.
REM Qui la chiave si legge dal .env del prototipo e vive solo per questo avvio:
REM non viene copiata da nessuna parte, e non viene mai stampata a schermo.
REM
REM Attrezzo di sviluppo: sta fuori dal prodotto, non entra nell'eseguibile.

setlocal
set "RADICE=%~dp0.."
set "ENVFILE=%RADICE%\HTML+JS\.env"

REM L'applicazione e' quella di riferimento, sul Desktop: un file solo, l'ultima
REM versione, la stessa che avvia l'assistente per le sue prove. La rifa'
REM aggiorna-riferimento.bat, qui accanto.
set "DESKTOP="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "[Environment]::GetFolderPath('Desktop')" 2^>nul`) do set "DESKTOP=%%D"
if not defined DESKTOP set "DESKTOP=%USERPROFILE%\Desktop"
set "APP=%DESKTOP%\TrovaLavoro.exe"

if not exist "%ENVFILE%" (
  echo.
  echo Non trovo il file con la chiave:
  echo   "%ENVFILE%"
  echo.
  pause
  exit /b 1
)

for /f "usebackq tokens=1,* delims==" %%a in ("%ENVFILE%") do (
  if /i "%%a"=="ANTHROPIC_API_KEY" set "ANTHROPIC_API_KEY=%%b"
)

if not defined ANTHROPIC_API_KEY (
  echo.
  echo Nel file .env non c'e' la riga ANTHROPIC_API_KEY=...
  echo.
  pause
  exit /b 1
)

if not exist "%APP%" (
  echo.
  echo Non trovo l'applicazione di riferimento:
  echo   "%APP%"
  echo.
  echo Falla, con un doppio clic su:
  echo   "%~dp0aggiorna-riferimento.bat"
  echo.
  pause
  exit /b 1
)

echo Chiave caricata. Avvio TrovaLavoro...
start "" "%APP%"
endlocal
