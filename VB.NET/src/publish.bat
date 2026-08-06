@echo off
REM Pubblicazione di rilascio di TrovaLavoro, con i parametri fissi del cap. 13.2:
REM un solo eseguibile autonomo (runtime .NET incluso), senza compressione e
REM senza DLL a fianco. Il trimming non e' un'opzione: Windows Forms non lo supporta.

setlocal
cd /d "%~dp0"

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

"%DOTNET%" publish TrovaLavoro\TrovaLavoro.vbproj ^
  -c Release ^
  -r win-x64 ^
  -p:PublishSingleFile=true ^
  -p:SelfContained=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -p:DebugType=none ^
  -o pubblicazione

if errorlevel 1 (
  echo.
  echo Pubblicazione FALLITA.
  endlocal
  exit /b 1
)

echo.
echo Fatto: "%~dp0pubblicazione\TrovaLavoro.exe"
endlocal
