@echo off
REM Pubblicazione di rilascio di TrovaLavoro, con i parametri fissi del cap. 13.2:
REM un solo eseguibile autonomo (runtime .NET incluso), senza compressione e
REM senza DLL a fianco. Il trimming non e' un'opzione: Windows Forms non lo supporta.
REM
REM Dal 2026-08-27 fa anche due cose in piu' (cap. 13.9, reperto D-R1 del giro D):
REM  1. scrive DENTRO l'eseguibile il commit da cui nasce, cosi' che il file sappia
REM     dirsi da solo -- il numero di versione non basta, perche' e' un'etichetta
REM     scritta a mano e il 24 agosto 2026 due file diversi hanno portato lo stesso
REM     "1.0.000";
REM  2. a pubblicazione fatta stampa versione, dimensione e impronta SHA-256, che
REM     sono le tre cose da annotare accanto al tag di rilascio.

setlocal
cd /d "%~dp0"

REM --- Da quale codice sorgente nasce questo eseguibile -------------------------
REM Se git non c'e', o non siamo in un repository, il codice resta vuoto: allora
REM l'attributo non entra nell'eseguibile e l'applicazione dichiara "non dichiarato
REM (compilazione di sviluppo)". Meglio il silenzio dichiarato di un codice inventato.
set "CODICE="
for /f "usebackq delims=" %%G in (`git -C "%~dp0" rev-parse --short HEAD 2^>nul`) do set "CODICE=%%G"

REM Un albero di lavoro sporco NON corrisponde a nessun commit: l'eseguibile lo dice
REM di se stesso, invece di spacciarsi per il commit da cui e' quasi uscito.
set "SPORCO="
for /f "usebackq delims=" %%S in (`git -C "%~dp0" status --porcelain 2^>nul`) do set "SPORCO=1"
if defined SPORCO if defined CODICE set "CODICE=%CODICE%+modificato"

if not defined CODICE (
  echo ATTENZIONE: non so da quale commit nasce questa pubblicazione.
  echo             L'eseguibile lo dichiarera' come compilazione di sviluppo.
)
if defined SPORCO (
  echo ATTENZIONE: ci sono modifiche non committate. L'eseguibile portera' il
  echo             codice "%CODICE%", che non e' un commit pubblicabile.
)

REM --- L'SDK ---------------------------------------------------------------------
REM Serve l'SDK .NET 10. Se quello nel PATH e' piu' vecchio si prova quello
REM installato nel profilo utente, dove finisce l'installazione senza diritti
REM di amministratore.
set "DOTNET=dotnet"
"%DOTNET%" --list-sdks 2>nul | findstr /b /c:"10." >nul
if errorlevel 1 set "DOTNET=%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe"

"%DOTNET%" --list-sdks 2>nul | findstr /b /c:"10." >nul
if errorlevel 1 (
  echo Non trovo l'SDK .NET 10, ne' nel PATH ne' nel profilo utente.
  endlocal
  exit /b 1
)

REM --- La pubblicazione -----------------------------------------------------------
"%DOTNET%" publish TrovaLavoro\TrovaLavoro.vbproj ^
  -c Release ^
  -r win-x64 ^
  -p:PublishSingleFile=true ^
  -p:SelfContained=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:DebugType=none ^
  -p:AllowedReferenceRelatedFileExtensions=none ^
  -p:CodiceSorgente=%CODICE% ^
  -o pubblicazione

if errorlevel 1 (
  echo.
  echo Pubblicazione FALLITA.
  endlocal
  exit /b 1
)

REM --- L'identita' del file che e' appena uscito ------------------------------------
REM Le tre righe che distinguono davvero un eseguibile da un altro. Si annotano
REM accanto al tag di rilascio: da li' in poi "la 1.0" e' un file preciso, non un
REM numero che due file possono portare insieme.
set "EXE=%~dp0pubblicazione\TrovaLavoro.exe"

if not exist "%EXE%" (
  echo.
  echo Pubblicazione FALLITA: l'eseguibile non c'e'.
  endlocal
  exit /b 1
)

set "VERSIONE="
for /f "tokens=2 delims==" %%V in ('findstr /c:"Numero As String" "%~dp0TrovaLavoro\Versione.vb"') do set "VERSIONE=%%V"
set "VERSIONE=%VERSIONE: =%"
set "VERSIONE=%VERSIONE:"=%"

for %%F in ("%EXE%") do set "DIMENSIONE=%%~zF"

set "IMPRONTA="
for /f "skip=1 delims=" %%H in ('certutil -hashfile "%EXE%" SHA256') do (
  if not defined IMPRONTA set "IMPRONTA=%%H"
)

echo.
echo ==================== IDENTITA' DELL'ESEGUIBILE ====================
echo   file       : %EXE%
echo   versione   : %VERSIONE%
if defined CODICE (
  echo   commit     : %CODICE%
) else (
  echo   commit     : non dichiarato ^(compilazione di sviluppo^)
)
echo   dimensione : %DIMENSIONE% byte
echo   SHA-256    : %IMPRONTA%
echo ==================================================================
echo.
if defined SPORCO (
  echo   Questa pubblicazione NON e' rilasciabile: l'albero di lavoro era sporco.
  echo   Committa, ripubblica, e solo allora metti il tag.
) else (
  echo   Annota commit, dimensione e SHA-256 accanto al tag di rilascio.
)

endlocal
