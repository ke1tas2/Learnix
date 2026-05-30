@echo off
cd /d "%~dp0..\Learnix.API\bin\Debug\net10.0"
echo Starting Learnix API on http://localhost:5199
echo Keep this window open while using the MAUI app.
Learnix.API.exe --urls http://localhost:5199
echo.
echo Learnix API stopped. Press any key to close this window.
pause >nul
