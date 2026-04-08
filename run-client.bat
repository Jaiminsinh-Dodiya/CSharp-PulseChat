@echo off
echo Starting PulseChat Client...
echo.
if not exist "PulseChatClient\bin\Debug\PulseChatClient.exe" (
    echo [ERROR] Client not built yet! Run setup.bat first.
    pause
    exit /b 1
)
start "" "PulseChatClient\bin\Debug\PulseChatClient.exe"
