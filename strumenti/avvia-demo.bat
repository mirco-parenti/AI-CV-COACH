@echo off
REM Lanciatore per mostrare TrovaLavoro dal vivo, senza passare da Claude Code.
REM
REM Serve perche' l'applicazione prende la chiave API dalla variabile d'ambiente
REM ANTHROPIC_API_KEY (la cifratura su disco e' di T6): con un doppio clic
REM sull'exe la variabile non c'e', e tutto cio' che passa dall'AI si ferma.
REM Qui la chiave si legge dal .env del prototipo e vive solo per questo avvio:
REM non viene copiata da nessuna parte, e non viene mai stampata a schermo.
REM
REM Attrezzo di sviluppo: sta fuori dal prodotto, non entra nell'eseguibile.

setlocal
set "RADICE=%~dp0.."
set "ENVFILE=%RADICE%\HTML+JS\.env"
set "APP=%RADICE%\VB.NET\src\TrovaLavoro\bin\Release\net10.0-windows\TrovaLavoro.exe"

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
  echo Non trovo l'applicazione:
  echo   "%APP%"
  echo.
  echo Compilala prima, dalla cartella VB.NET\src:
  echo   dotnet build TrovaLavoro\TrovaLavoro.vbproj -c Release
  echo.
  pause
  exit /b 1
)

echo Chiave caricata. Avvio TrovaLavoro...
start "" "%APP%"
endlocal
