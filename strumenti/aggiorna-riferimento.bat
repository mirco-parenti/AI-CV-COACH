@echo off
REM Rifa' l'ESEGUIBILE DI RIFERIMENTO sul Desktop: un solo file, autonomo, sempre
REM l'ultima versione del codice che c'e' in questo albero di lavoro.
REM
REM A che serve. E' la copia che si apre con un doppio clic quando la sessione con
REM l'assistente e' chiusa, ed e' la stessa che l'assistente avvia per le sue prove
REM dal vivo (lo strumento di collaudo punta li'): cosi' "l'app" e' un file solo, e
REM non ci sono due versioni diverse che si chiamano allo stesso modo.
REM
REM Come e' fatto. Gli stessi parametri del rilascio (cap. 13.2): un eseguibile
REM autonomo col runtime .NET dentro, senza DLL a fianco. La differenza con
REM VB.NET\src\publish.bat e' solo la destinazione e il fatto che qui il codice puo'
REM essere sporco: un riferimento di lavoro non e' un rilascio, e infatti lo dichiara.
REM
REM Due accorgimenti che valgono la pena di sapere:
REM  1. la compilazione intermedia va in %TEMP%, non in bin\Release: quel file la'
REM     dentro lo tiene bloccato il server MCP del prodotto, e senza questo il
REM     comando fallirebbe con MSB3027 per un motivo che col codice non c'entra;
REM  2. prima di sovrascrivere si chiude SOLO l'applicazione che gira dal Desktop,
REM     riconosciuta dal percorso. Mai "taskkill /IM TrovaLavoro.exe": quel nome ce
REM     l'ha anche il server MCP del prodotto, e lo si ammazzerebbe insieme.
REM
REM Attrezzo di sviluppo: sta fuori dal prodotto, non entra nell'eseguibile.

setlocal
set "SRC=%~dp0..\VB.NET\src"

REM --- Dove sta il Desktop --------------------------------------------------------
REM Non e' detto che sia %USERPROFILE%\Desktop: puo' essere spostato o su OneDrive.
set "DESKTOP="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "[Environment]::GetFolderPath('Desktop')" 2^>nul`) do set "DESKTOP=%%D"
if not defined DESKTOP set "DESKTOP=%USERPROFILE%\Desktop"
set "DEST=%DESKTOP%\TrovaLavoro.exe"

REM --- Da quale codice sorgente nasce ----------------------------------------------
REM Il punto dopo %~dp0 non e' un vezzo: quella variabile finisce con una barra
REM rovescia, e "C:\...\strumenti\" arriva a git con la virgoletta inglobata. Git
REM fallisce in silenzio, e l'eseguibile esce senza sapere da dove viene.
set "CODICE="
for /f "usebackq delims=" %%G in (`git -C "%~dp0." rev-parse --short HEAD 2^>nul`) do set "CODICE=%%G"

set "SPORCO="
for /f "usebackq delims=" %%S in (`git -C "%~dp0." status --porcelain 2^>nul`) do set "SPORCO=1"
if defined SPORCO if defined CODICE set "CODICE=%CODICE%+modificato"

REM --- L'SDK -----------------------------------------------------------------------
set "DOTNET=dotnet"
"%DOTNET%" --list-sdks 2>nul | findstr /b /c:"10." >nul
if errorlevel 1 set "DOTNET=%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe"

"%DOTNET%" --list-sdks 2>nul | findstr /b /c:"10." >nul
if errorlevel 1 (
  echo Non trovo l'SDK .NET 10, ne' nel PATH ne' nel profilo utente.
  endlocal
  exit /b 1
)

REM --- Si chiude il riferimento, se e' aperto ---------------------------------------
set "EXEDEST=%DEST%"
powershell -NoProfile -Command "Get-Process TrovaLavoro -ErrorAction SilentlyContinue | Where-Object { $_.Path -eq $env:EXEDEST } | Stop-Process -Force" 2>nul

REM --- La pubblicazione, in una cartella d'appoggio ---------------------------------
set "APPOGGIO=%TEMP%\tl-riferimento-out"
if exist "%APPOGGIO%\TrovaLavoro.exe" del /q "%APPOGGIO%\TrovaLavoro.exe"

"%DOTNET%" publish "%SRC%\TrovaLavoro\TrovaLavoro.vbproj" ^
  -c Release ^
  -r win-x64 ^
  -p:PublishSingleFile=true ^
  -p:SelfContained=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:DebugType=none ^
  -p:AllowedReferenceRelatedFileExtensions=none ^
  -p:CodiceSorgente=%CODICE% ^
  -p:BaseOutputPath=%TEMP%\tl-riferimento\ ^
  -o "%APPOGGIO%" ^
  --nologo

if errorlevel 1 (
  echo.
  echo Pubblicazione FALLITA: sul Desktop resta il riferimento di prima.
  endlocal
  exit /b 1
)

if not exist "%APPOGGIO%\TrovaLavoro.exe" (
  echo.
  echo Pubblicazione FALLITA: l'eseguibile non c'e'. Sul Desktop resta quello di prima.
  endlocal
  exit /b 1
)

copy /y "%APPOGGIO%\TrovaLavoro.exe" "%DEST%" >nul
if errorlevel 1 (
  echo.
  echo Non sono riuscito a scrivere "%DEST%" ^(e' aperto?^).
  endlocal
  exit /b 1
)

REM --- L'identita' del file che sta adesso sul Desktop --------------------------------
set "VERSIONE="
for /f "tokens=2 delims==" %%V in ('findstr /c:"Numero As String" "%SRC%\TrovaLavoro\Versione.vb"') do set "VERSIONE=%%V"
set "VERSIONE=%VERSIONE: =%"
set "VERSIONE=%VERSIONE:"=%"

for %%F in ("%DEST%") do set "DIMENSIONE=%%~zF"

set "IMPRONTA="
for /f "skip=1 delims=" %%H in ('certutil -hashfile "%DEST%" SHA256') do (
  if not defined IMPRONTA set "IMPRONTA=%%H"
)

echo.
echo ============== ESEGUIBILE DI RIFERIMENTO, SUL DESKTOP ==============
echo   file       : %DEST%
echo   versione   : %VERSIONE%
if defined CODICE (
  echo   commit     : %CODICE%
) else (
  echo   commit     : non dichiarato ^(compilazione di sviluppo^)
)
echo   dimensione : %DIMENSIONE% byte
echo   SHA-256    : %IMPRONTA%
echo ===================================================================
if defined SPORCO (
  echo   Nasce da un albero di lavoro con modifiche non committate: e' il
  echo   riferimento di OGGI, non un rilascio. Per un rilascio: publish.bat.
)
echo.

endlocal
