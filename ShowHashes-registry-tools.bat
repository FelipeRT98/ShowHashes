@echo off
setlocal EnableDelayedExpansion

:: Self-elevate if not running as admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\_getadmin.vbs"
    echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\_getadmin.vbs"
    "%temp%\_getadmin.vbs"
    del "%temp%\_getadmin.vbs"
    exit /b
)

set "EXE_NAME=ShowHashes.exe"
set "MENU_NAME=ShowHashes"
set "DISPLAY_NAME=Show Hashes"
set "REG_PATH=HKCR\*\shell\%MENU_NAME%"
set "EXE_NAME=ShowHashes.exe"


echo ----------------------------------------
echo Context Menu Tool: %DISPLAY_NAME%
echo ----------------------------------------

:: Check if registry entry exists
reg query "%REG_PATH%" >nul 2>&1
if %errorlevel%==0 (
    echo [INFO] Registry entry already exists at %REG_PATH%
    echo.
    set /p "REMOVE=Do you want to remove it? (y/n): "
    set "REMOVE=!REMOVE: =!"
    if /i "!REMOVE!"=="y" (
        reg delete "%REG_PATH%" /f >nul
        echo [SUCCESS] Context menu entry removed at %REG_PATH%
    ) else (
        echo [INFO] Skipped removal.
    )
    goto :END
)

:: Get the full path using PowerShell to avoid short path behavior
for /f %%I in ('powershell -command "[System.IO.Path]::GetFullPath('%~dp0%EXE_NAME%')"') do set "EXE_PATH=%%I"

:: Check if executable exists
if not exist "%EXE_PATH%" (
    echo [ERROR] %EXE_PATH% NOT FOUND
    goto :END
) else (
    echo [INFO] %EXE_PATH% FOUND
)

:: Ask user before adding entry
echo [INFO] %REG_PATH% is going to be used as the registry path.
set /p "ADD=Do you want to create the context menu entry? (y/n): "
set "ADD=!ADD: =!"
if /i not "!ADD!"=="y" (
    echo Aborted.
    goto :END
)

:: Add new context menu entry
reg add "%REG_PATH%" /ve /d "%DISPLAY_NAME%" /f >nul
reg add "%REG_PATH%" /v "Icon" /d "\"%EXE_PATH%\"" /f >nul
reg add "%REG_PATH%\command" /ve /d "\"%EXE_PATH%\" \"%%1\"" /f >nul

echo [SUCCESS] Context menu entry added at %REG_PATH%

:END
echo.
echo Press any key to close...
pause >nul
exit /b
