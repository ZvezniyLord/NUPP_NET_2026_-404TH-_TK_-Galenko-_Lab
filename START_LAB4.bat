@echo off
setlocal EnableExtensions
title MusicContest - Laboratory 4
cd /d "%~dp0"

echo =====================================================
echo MusicContest - Laboratory 4: automatic demonstration
echo =====================================================
echo.
echo This script installs .NET 8 SDK only if it is missing.
echo Windows may request administrator approval for installation.
echo.

if not exist "MusicContest.sln" (
    echo ERROR: MusicContest.sln was not found beside START_LAB4.bat.
    echo Extract or clone the whole lab4 repository and retry.
    goto :failed
)

if exist "%ProgramFiles%\dotnet\dotnet.exe" (
    set "PATH=%ProgramFiles%\dotnet;%PATH%"
)
if exist "%USERPROFILE%\.dotnet\dotnet.exe" (
    set "PATH=%USERPROFILE%\.dotnet;%PATH%"
)

where dotnet >nul 2>&1
if errorlevel 1 goto :install_sdk
dotnet --list-sdks 2>nul | findstr /B /C:"8." >nul
if errorlevel 1 goto :install_sdk
goto :sdk_ready

:install_sdk
echo [1/6] .NET 8 SDK was not found.
where winget >nul 2>&1
if errorlevel 1 (
    echo ERROR: Windows Package Manager winget is unavailable.
    echo Install .NET 8 SDK manually from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo Then run this file again.
    goto :failed
)
echo Installing the official Microsoft .NET 8 SDK...
winget install --id Microsoft.DotNet.SDK.8 --exact --source winget --accept-package-agreements --accept-source-agreements
if errorlevel 1 (
    echo ERROR: SDK installation failed or was cancelled.
    echo If Windows requested elevation, approve it and retry.
    goto :failed
)
if exist "%ProgramFiles%\dotnet\dotnet.exe" (
    set "PATH=%ProgramFiles%\dotnet;%PATH%"
)
if exist "%USERPROFILE%\.dotnet\dotnet.exe" (
    set "PATH=%USERPROFILE%\.dotnet;%PATH%"
)
where dotnet >nul 2>&1
if errorlevel 1 (
    echo ERROR: dotnet is not available after installation.
    echo Close all terminal windows, reopen this BAT file and retry.
    goto :failed
)
dotnet --list-sdks | findstr /B /C:"8." >nul
if errorlevel 1 (
    echo ERROR: .NET 8 SDK is still not detected.
    echo Restart the terminal or Windows and run this BAT again.
    goto :failed
)

:sdk_ready
echo [1/6] .NET SDK:
dotnet --version
echo.
echo [2/6] Restoring NuGet packages...
dotnet restore "MusicContest.sln"
if errorlevel 1 (
    echo ERROR: dotnet restore failed. Check internet access and NuGet output.
    goto :failed
)
echo.
echo [3/6] Building the Release configuration...
dotnet build "MusicContest.sln" -c Release --no-restore
if errorlevel 1 (
    echo ERROR: Project build failed. Check the compiler output.
    goto :failed
)
echo.
echo [4/6] Preparing SQLite demo data...
echo Existing demo records will be preserved; the app seeds missing records.
dotnet run --project "MusicContest.Console" -c Release --no-build --no-restore -- --lab3-only
if errorlevel 1 (
    echo ERROR: SQLite initialization failed.
    goto :failed
)
echo.
echo [5/6] Checking local ports...
powershell -NoProfile -Command "$p=5084,5085; foreach($port in $p){$c=[Net.Sockets.TcpClient]::new(); try{$c.Connect('127.0.0.1',$port); Write-Host ('Port '+$port+' is already in use.'); exit 1}catch{}finally{$c.Dispose()}}"
if errorlevel 1 (
    echo ERROR: Close the previous web server windows using ports 5084/5085.
    goto :failed
)
echo Starting MVC site and REST API in separate console windows...
start "MusicContest MVC - close to stop" /D "%CD%" cmd /k dotnet run --project MusicContest.MVC -c Release --no-build --no-restore --urls http://127.0.0.1:5085
start "MusicContest REST API - close to stop" /D "%CD%" cmd /k dotnet run --project MusicContest.REST -c Release --no-build --no-restore --urls http://127.0.0.1:5084

echo.
echo [6/6] Waiting for MVC website...
powershell -NoProfile -Command "$ok=$false; for($i=0;$i -lt 40;$i++){try{$r=Invoke-WebRequest -UseBasicParsing 'http://127.0.0.1:5085/' -TimeoutSec 3; if($r.StatusCode -eq 200){$ok=$true;break}}catch{};Start-Sleep -Seconds 1}; if($ok){exit 0}else{exit 1}"
if errorlevel 1 (
    echo ERROR: MVC did not respond. Inspect the "MusicContest MVC" window.
    goto :failed
)
echo Waiting for Swagger UI...
powershell -NoProfile -Command "$ok=$false; for($i=0;$i -lt 40;$i++){try{$r=Invoke-WebRequest -UseBasicParsing 'http://127.0.0.1:5084/swagger/v1/swagger.json' -TimeoutSec 3; if($r.StatusCode -eq 200){$ok=$true;break}}catch{};Start-Sleep -Seconds 1}; if($ok){exit 0}else{exit 1}"
if errorlevel 1 (
    echo ERROR: REST API did not respond. Inspect the "MusicContest REST API" window.
    goto :failed
)

start "" "http://127.0.0.1:5085/"
start "" "http://127.0.0.1:5084/swagger"
echo.
echo SUCCESS: Both applications are running.
echo MVC website: http://127.0.0.1:5085/
echo Swagger API: http://127.0.0.1:5084/swagger
echo.
echo Close BOTH separate server console windows to stop the applications.
echo No changes were pushed to GitHub.
pause
exit /b 0

:failed
echo.
echo The demonstration did not finish. Read the error above.
echo You can safely run START_LAB4.bat again.
pause
exit /b 1
