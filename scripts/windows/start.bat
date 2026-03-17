@echo off
chcp 65001 >nul
setlocal EnableExtensions EnableDelayedExpansion

set "SCRIPT_DIR=%~dp0"
cd /d "%SCRIPT_DIR%"

echo ===================================
echo    PFLAC DEPLOYMENT SCRIPT
echo ===================================

call :require_admin || exit /b 1
call :ensure_git || exit /b 1
call :ensure_wsl || exit /b 1
call :ensure_docker || exit /b 1

cd /d "%SCRIPT_DIR%.."
set "PROJECT_ROOT=%cd%"

set /p NODE_ENV="Enter NODE_ENV (prod/dev): "
set /p DB_USER="Enter DB_USER: "
set /p DB_NAME="Enter DB_NAME: "
set /p DB_PASS="Enter DB_PASS: "
set /p DB_PORT="Enter DB_PORT (e.g., 3306): "

(
    echo NODE_ENV=%NODE_ENV%
    echo DB_USER=%DB_USER%
    echo DB_NAME=%DB_NAME%
    echo DB_PASS=%DB_PASS%
    echo DB_PORT=%DB_PORT%
    echo DB_SERVER=mysql_local
) > "%PROJECT_ROOT%\.env"

if not exist "%PROJECT_ROOT%\pflac_api" (
    echo [INFO] Cloning API...
    git clone https://github.com/fxhxyz4/pflac_api.git "%PROJECT_ROOT%\pflac_api"
    if %errorlevel% neq 0 (
        echo [FATAL] Failed to clone repository.
        pause
        exit /b 1
    )
)

set "SCHEMA_PATH="
if exist "%PROJECT_ROOT%\pflac_api\db\schema.sql" set "SCHEMA_PATH=%PROJECT_ROOT%\pflac_api\db\schema.sql"

if not defined SCHEMA_PATH (
    for %%f in ("%PROJECT_ROOT%\pflac_api\db\*.sql") do (
        set "SCHEMA_PATH=%%~f"
        goto :schema_found
    )
)

echo [WARN] No SQL files in db folder. Searching everywhere in %PROJECT_ROOT%...
for /r "%PROJECT_ROOT%" %%f in (*.sql) do (
    set "SCHEMA_PATH=%%~f"
    goto :schema_found
)

:schema_found
if not defined SCHEMA_PATH (
    echo [FATAL] No .sql files found in %PROJECT_ROOT%.
    pause
    exit /b 1
)
echo [OK] Using schema file: %SCHEMA_PATH%

docker network inspect pflac_network >nul 2>&1
if %errorlevel% neq 0 (
    docker network create pflac_network >nul 2>&1
    if %errorlevel% neq 0 (
        echo [FATAL] Failed to create Docker network.
        pause
        exit /b 1
    )
)

echo [INFO] Starting MySQL...
docker rm -f mysql_local >nul 2>&1
docker run -d --name mysql_local --network pflac_network ^
    -e MYSQL_ROOT_PASSWORD=root ^
    -e MYSQL_DATABASE=%DB_NAME% ^
    -e MYSQL_USER=%DB_USER% ^
    -e MYSQL_PASSWORD=%DB_PASS% ^
    -p %DB_PORT%:3306 ^
    mysql:8 --disable-log-bin

if %errorlevel% neq 0 (
    echo [FATAL] Failed to start MySQL container.
    pause
    exit /b 1
)

echo [INFO] Waiting for MySQL...
set /a MYSQL_WAIT=0
:wait_mysql
docker exec mysql_local mysql -u%DB_USER% -p%DB_PASS% -e "SELECT 1;" %DB_NAME% >nul 2>&1
if %errorlevel% neq 0 (
    set /a MYSQL_WAIT+=1
    if !MYSQL_WAIT! geq 60 (
        echo [FATAL] MySQL did not become ready in time.
        echo [INFO] MySQL logs:
        docker logs mysql_local
        pause
        exit /b 1
    )
    timeout /t 2 /nobreak >nul
    goto :wait_mysql
)
echo [OK] MySQL is online.

echo [INFO] Importing data...
docker exec -i mysql_local mysql -u%DB_USER% -p%DB_PASS% %DB_NAME% < "%SCHEMA_PATH%"
if %errorlevel% neq 0 (
    echo [FATAL] Failed to import SQL schema.
    pause
    exit /b 1
)

echo [INFO] Starting phpMyAdmin...
docker rm -f phpmyadmin_local >nul 2>&1
docker run -d --name phpmyadmin_local --network pflac_network ^
    -e PMA_HOST=mysql_local ^
    -e PMA_PORT=3306 ^
    -e PMA_ARBITRARY=1 ^
    -p 8080:80 ^
    phpmyadmin/phpmyadmin:latest

if %errorlevel% neq 0 (
    echo [FATAL] Failed to start phpMyAdmin.
    pause
    exit /b 1
)

echo [INFO] Building API...
cd /d "%PROJECT_ROOT%\pflac_api"
docker build -t pflac_api_image .
if %errorlevel% neq 0 (
    echo [FATAL] Docker build failed.
    pause
    exit /b 1
)

echo [INFO] Starting API...
docker rm -f pflac_api_local >nul 2>&1
docker run -d --name pflac_api_local --network pflac_network --env-file "%PROJECT_ROOT%\.env" -p 8000:8000 pflac_api_image
if %errorlevel% neq 0 (
    echo [FATAL] Failed to start API container.
    pause
    exit /b 1
)

echo.
echo ===================================
echo DEPLOYMENT FINISHED
echo API: http://localhost:8000
echo PMA: http://localhost:8080
echo ===================================
pause
exit /b 0

:require_admin
net session >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Run this script as Administrator.
    pause
    exit /b 1
)
exit /b 0

:ensure_git
where git >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Git is not installed or not in PATH.
    pause
    exit /b 1
)
echo [OK] Git found.
exit /b 0

:ensure_wsl
echo [INFO] Checking WSL...

where wsl.exe >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] WSL is not available on this system.
    exit /b 1
)

wsl --install --no-distribution >nul 2>&1
wsl --update >nul 2>&1

wsl -l -q | findstr /R /C:".*" >nul 2>&1
if %errorlevel% neq 0 (
    echo [INFO] Installing Ubuntu for WSL...
    wsl --install -d Ubuntu
    if %errorlevel% neq 0 (
        echo [ERROR] Failed to install Ubuntu WSL distro.
        exit /b 1
    )
    echo [WARN] Ubuntu was installed. Reboot may be required before Docker works correctly.
)

echo [OK] WSL checked.
exit /b 0

:ensure_docker
echo [INFO] Checking Docker Desktop...

where docker >nul 2>&1
if %errorlevel% equ 0 goto :docker_found

echo [INFO] Docker not found. Installing Docker Desktop...

where winget >nul 2>&1
if %errorlevel% equ 0 (
    echo [INFO] Trying winget installation...
    winget install -e --id Docker.DockerDesktop --accept-package-agreements --accept-source-agreements --silent
    if !errorlevel! equ 0 goto :docker_post_install
    echo [WARN] winget install failed, trying official installer...
) else (
    echo [WARN] winget not found, trying official installer...
)

set "DOCKER_INSTALLER=%TEMP%\DockerDesktopInstaller.exe"
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$ProgressPreference='SilentlyContinue'; Invoke-WebRequest -UseBasicParsing 'https://desktop.docker.com/win/main/amd64/Docker%%20Desktop%%20Installer.exe' -OutFile '%DOCKER_INSTALLER%'"
if %errorlevel% neq 0 (
    echo [ERROR] Failed to download Docker Desktop installer.
    exit /b 1
)

echo [INFO] Running Docker Desktop installer...
"%DOCKER_INSTALLER%" install --accept-license --backend=wsl-2
if %errorlevel% neq 0 (
    echo [ERROR] Docker Desktop installer failed.
    exit /b 1
)

:docker_post_install
echo [INFO] Waiting for docker.exe to appear...
set /a _wait=0
:wait_docker_exe
where docker >nul 2>&1
if %errorlevel% equ 0 goto :docker_found
timeout /t 3 /nobreak >nul
set /a _wait+=1
if !_wait! geq 40 (
    echo [ERROR] docker.exe was not found after installation.
    exit /b 1
)
goto :wait_docker_exe

:docker_found
echo [OK] Docker CLI found.

tasklist /FI "IMAGENAME eq Docker Desktop.exe" 2>nul | find /I "Docker Desktop.exe" >nul
if %errorlevel% neq 0 (
    echo [INFO] Starting Docker Desktop...
    if exist "C:\Program Files\Docker\Docker\Docker Desktop.exe" (
        start "" "C:\Program Files\Docker\Docker\Docker Desktop.exe"
    ) else (
        echo [ERROR] Docker Desktop executable not found.
        exit /b 1
    )
)

echo [INFO] Waiting for Docker engine...
set /a _engine_wait=0
:wait_docker_engine
docker version >nul 2>&1
if %errorlevel% equ 0 (
    echo [OK] Docker engine is ready.
    exit /b 0
)
timeout /t 5 /nobreak >nul
set /a _engine_wait+=1
if !_engine_wait! geq 36 (
    echo [ERROR] Docker engine did not become ready in time.
    echo [HINT] Reboot Windows once, then run the script again.
    exit /b 1
)
goto :wait_docker_engine
