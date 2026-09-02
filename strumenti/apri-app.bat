@echo off
REM Apre l'ESEGUIBILE DI RIFERIMENTO del Desktop, e si accerta che sia partito davvero.
REM
REM A che serve. Aprire quel file sembra la cosa piu' semplice del mondo, e da WSL non
REM lo e': il 2026-09-02 l'assistente ha creduto di aver aperto l'applicazione per
REM cinque minuti mentre non era partito niente. Due trappole insieme, e questo
REM lanciatore esiste per chiuderle tutt'e due.
REM
REM  1. "start" NON dice se ha aperto qualcosa. Invocato da WSL come
REM     cmd.exe /c start "" "C:\...\Desktop\TrovaLavoro.exe", il percorso con gli spazi
REM     si perde per strada nel passaggio fra le due shell: a volte risponde "Accesso
REM     negato", a volte — ed e' il caso peggiore — esce a ZERO senza aprire niente.
REM     Il rimedio non e' un quoting piu' furbo: e' non credere all'esito e andare a
REM     guardare se la finestra c'e'. Lo fa questo file, e per questo va invocato lui
REM     e non "start": un .bat passa come argomento unico, e li' il quoting regge.
REM
REM  2. Di TrovaLavoro.exe ce n'e' piu' D'UNO. Il server MCP del prodotto gira come
REM     TrovaLavoro.exe --mcp e non ha NESSUNA finestra. Chi cerca "il primo processo
REM     che si chiama cosi'" trova quasi sempre lui, e conclude che l'applicazione non
REM     e' partita mentre e' li' aperta. Qui si guarda MainWindowHandle, che il server
REM     non ha e la finestra si': e' la differenza che conta, non il nome.
REM
REM Non apre una seconda finestra se ce n'e' gia' una: due finestre della stessa
REM applicazione si distinguono solo dalla barra del titolo, e un giro di collaudo
REM intero e' gia' finito una volta su quella sbagliata.
REM
REM Non ricompila niente: apre quel che c'e'. Per rifare l'eseguibile col codice di
REM adesso, prima aggiorna-riferimento.bat, qui accanto.
REM
REM Attrezzo di sviluppo: sta fuori dal prodotto, non entra nell'eseguibile.

setlocal

REM --- Dove sta il Desktop --------------------------------------------------------
REM Non e' detto che sia %USERPROFILE%\Desktop: puo' essere spostato o su OneDrive.
set "DESKTOP="
for /f "usebackq delims=" %%D in (`powershell -NoProfile -Command "[Environment]::GetFolderPath('Desktop')" 2^>nul`) do set "DESKTOP=%%D"
if not defined DESKTOP set "DESKTOP=%USERPROFILE%\Desktop"
set "APP=%DESKTOP%\TrovaLavoro.exe"

if not exist "%APP%" (
  echo.
  echo   Sul Desktop non c'e' nessun TrovaLavoro.exe: "%APP%"
  echo   Lo crea aggiorna-riferimento.bat, qui accanto.
  echo.
  endlocal
  exit /b 2
)

REM --- C'e' gia' una finestra aperta? ----------------------------------------------
REM Il filtro e' MainWindowHandle: il server MCP (--mcp) non ne ha, e va saltato.
set "GIAPERTA="
for /f "usebackq delims=" %%P in (`powershell -NoProfile -Command "(Get-Process TrovaLavoro -EA SilentlyContinue ^| Where-Object { $_.MainWindowHandle -ne 0 } ^| Select-Object -First 1 -Expand Id)" 2^>nul`) do set "GIAPERTA=%%P"

if defined GIAPERTA (
  echo.
  echo   L'applicazione e' gia' aperta ^(pid %GIAPERTA%^): non ne apro una seconda.
  echo   Due finestre uguali si distinguono solo dal titolo, ed e' un guaio.
  echo.
  endlocal
  exit /b 0
)

REM --- Si apre ---------------------------------------------------------------------
echo   Apro %APP%
start "" "%APP%"

REM --- ...e si CONTROLLA che sia aperta davvero -------------------------------------
REM Un eseguibile autonomo da 120 MB si estrae prima di mostrare qualcosa: la finestra
REM puo' farsi attendere una decina di secondi. Si aspetta fino a 40, guardando.
set "PID="
for /f "usebackq delims=" %%P in (`powershell -NoProfile -Command "$f=0; for($i=0;$i -lt 40;$i++){ $p=Get-Process TrovaLavoro -EA SilentlyContinue ^| Where-Object { $_.MainWindowHandle -ne 0 } ^| Select-Object -First 1; if($p){ $f=$p.Id; break }; Start-Sleep -Seconds 1 }; $f" 2^>nul`) do set "PID=%%P"

if "%PID%"=="0" set "PID="
if not defined PID (
  echo.
  echo   NON e' partita: dopo 40 secondi non c'e' nessuna finestra.
  echo   Non fidarti di un comando che esce a zero - guarda qui.
  echo   Da provare: aprirla con un doppio clic, e vedere se Windows dice qualcosa.
  echo.
  endlocal
  exit /b 1
)

for /f "usebackq delims=" %%T in (`powershell -NoProfile -Command "(Get-Process -Id %PID%).MainWindowTitle" 2^>nul`) do set "TITOLO=%%T"

echo.
echo ==================== APPLICAZIONE APERTA ====================
echo   file   : %APP%
echo   pid    : %PID%
echo   titolo : %TITOLO%
echo =============================================================
echo.

endlocal
exit /b 0
