@echo off
echo Starting PulseChat Server...
echo.
if not exist "PulseChatServer\bin\Debug\PulseChatServer.exe" (
    echo [ERROR] Server not built yet! Run setup.bat first.
    pause
    exit /b 1
)
start "" "PulseChatServer\bin\Debug\PulseChatServer.exe"
