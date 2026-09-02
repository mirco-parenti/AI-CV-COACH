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
REM
REM NIENTE PIPE qui dentro, e non e' una preferenza di stile. Dentro le virgolette di
REM -Command il "^" NON viene consumato da cmd: arriva a PowerShell tale e quale, e
REM "^|" gli rompe il comando in faccia con "Impossibile trovare un parametro
REM posizionale che accetta l'argomento '^'". Scritto con "^|" e con un 2>nul in coda
REM a nascondere l'errore, questo controllo non ha mai funzionato: rispondeva sempre
REM "nessuna finestra", che e' anche la risposta giusta l'80%% delle volte - ed e' per
REM questo che e' campato tre ore. Fuori dalle virgolette il "^" invece serve e
REM funziona: il "2^>nul" delle righe qui sopra sta bene dov'e'. La stessa cosa si
REM dice senza pipe con .Where(), e allora la si dice cosi'.
set "GIAPERTA="
for /f "usebackq delims=" %%P in (`powershell -NoProfile -Command "$v=@(Get-Process TrovaLavoro -EA SilentlyContinue).Where({$_.MainWindowHandle -ne 0}); if($v.Count -gt 0){ $v[0].Id }"`) do set "GIAPERTA=%%P"

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
REM Le due redirezioni staccano l'applicazione dallo stdout di questo file: senza, se
REM lo si invoca dentro una pipe, la pipe resta con uno scrittore vivo e non si chiude.
REM
REM Ma non basta a chi chiama DA WSL, e conviene saperlo prima di perderci del tempo:
REM "cmd.exe /c apri-app.bat" NON TORNA finche' l'applicazione resta aperta, perche'
REM l'interoperabilita' di WSL aspetta anche i discendenti - e questo lanciatore un
REM discendente lo lascia per forza: e' il suo mestiere. Il 2026-09-02 sono stati
REM cinque minuti di attesa a vuoto con l'app gia' aperta e il lavoro gia' finito.
REM Da WSL, percio', va lanciato in background e se ne legge l'uscita da un file.
start "" "%APP%" 1>nul 2>nul

REM --- ...e si CONTROLLA che sia aperta davvero -------------------------------------
REM Il 2026-09-02 questo blocco ha annunciato "NON e' partita" mentre l'applicazione si
REM stava aprendo, e poi una seconda volta mentre era gia' aperta da minuti. E' il falso
REM negativo, cioe' l'esatto contrario del male che il lanciatore esiste per curare, ed
REM e' peggio del male: sul silenzio si va a guardare, su un annuncio sicuro ci si fida.
REM
REM La colpa sembrava del tetto d'attesa, che allora era di 40 secondi - un eseguibile
REM autonomo da 120 MB appena riscritto si riestrae DA ZERO, impronta nuova e niente in
REM cache, con l'antivirus che ci passa sopra. Non era quello: il tetto era innocente e
REM la colpa stava nel "^|", vedi il commento del controllo qui sopra. Vale la pena
REM ricordarlo, perche' la prima diagnosi era plausibile, spiegava i fatti osservati e
REM avrebbe portato a "curare" un numero che non c'entrava niente - lasciando il difetto
REM dov'era, e con l'aria di averlo risolto.
REM
REM Il tetto e' comunque 180 secondi: la prima apertura dopo una ricompilazione e' lenta
REM davvero, e non c'e' motivo di essere avari. L'attesa si VEDE mentre passa, perche'
REM chi guarda uno schermo muto per tre minuti conclude che sia bloccato; e alla fine si
REM dichiara QUANTO ci ha messo davvero, cosi' il prossimo che tocca questo numero lo
REM cambia su una misura e non su un'impressione.
REM
REM I messaggi d'attesa vanno su stderr apposta: "for /f" cattura solo stdout, quindi
REM sullo stdout resta la sola riga finale "pid secondi" e i messaggi arrivano a schermo
REM invece di finire dentro la variabile.
set "PID="
set "ATTESA="
for /f "usebackq tokens=1,2" %%P in (`powershell -NoProfile -Command "$id=0; $n=0; for($i=1;$i -le 180;$i++){ $v=@(Get-Process TrovaLavoro -EA SilentlyContinue).Where({$_.MainWindowHandle -ne 0}); if($v.Count -gt 0){ $id=$v[0].Id; $n=$i; break }; if($i -ge 15 -and ($i - [math]::Floor($i/15)*15) -eq 0){ [Console]::Error.WriteLine('   ...ancora nessuna finestra: aspetto (' + $i + 's su 180)') }; Start-Sleep -Seconds 1 }; $id.ToString() + ' ' + $n.ToString()"`) do (
  set "PID=%%P"
  set "ATTESA=%%Q"
)

if "%PID%"=="0" set "PID="
if not defined PID (
  echo.
  echo   NON e' partita: dopo 180 secondi non c'e' nessuna finestra.
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
echo   attesa : %ATTESA% secondi prima che comparisse la finestra
echo =============================================================
echo.

endlocal
exit /b 0
