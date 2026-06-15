@echo off
setlocal
cd /d "%~dp0"
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\start-learnix-android.ps1"
echo.
echo Press any key to close this window.
pause >nul
