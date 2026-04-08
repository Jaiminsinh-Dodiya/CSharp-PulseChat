@echo off
echo ============================================
echo   PulseChat - Project Setup Script
echo ============================================
echo.

:: Check for NuGet
where nuget >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [*] Downloading NuGet CLI...
    powershell -Command "Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile 'nuget.exe'"
    if not exist nuget.exe (
        echo [ERROR] Failed to download nuget.exe
        pause
        exit /b 1
    )
    set NUGET=nuget.exe
) else (
    set NUGET=nuget
)

:: Restore NuGet packages
echo.
echo [1/3] Restoring NuGet packages...
%NUGET% restore PulseChat.sln
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] NuGet restore failed!
    pause
    exit /b 1
)
echo [OK] Packages restored.

:: Find MSBuild
echo.
echo [2/3] Locating MSBuild...
set MSBUILD=
for /f "usebackq tokens=*" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe 2^>nul`) do set MSBUILD=%%i

if "%MSBUILD%"=="" (
    echo [WARNING] MSBuild not found via vswhere. Trying default paths...
    if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" (
        set MSBUILD=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe
    ) else if exist "D:\Software\Visual Studio 2022\MSBuild\Current\Bin\MSBuild.exe" (
        set MSBUILD=D:\Software\Visual Studio 2022\MSBuild\Current\Bin\MSBuild.exe
    ) else (
        echo [ERROR] MSBuild not found! Install Visual Studio 2022 with .NET desktop development workload.
        pause
        exit /b 1
    )
)
echo [OK] Found MSBuild: %MSBUILD%

:: Build solution
echo.
echo [3/3] Building solution (Debug)...
"%MSBUILD%" PulseChat.sln /t:Build /p:Configuration=Debug /v:minimal
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [ERROR] Build failed! Check errors above.
    pause
    exit /b 1
)

echo.
echo ============================================
echo   BUILD SUCCESSFUL!
echo ============================================
echo.
echo   Server: PulseChatServer\bin\Debug\PulseChatServer.exe
echo   Client: PulseChatClient\bin\Debug\PulseChatClient.exe
echo.
echo   HOW TO RUN:
echo     1. Start Server:  Double-click PulseChatServer.exe
echo     2. Open Client:   Double-click PulseChatClient.exe (can open multiple)
echo     3. Sign Up ^& Login, then chat!
echo.

:: Clean up downloaded nuget if we fetched it
if exist nuget.exe del nuget.exe

pause
