@echo off
setlocal
rmdir /S /Q "%LOCALAPPDATA%\temp\SiliconStudio"
REM -------------------------------------------------------------------
REM Global config
REM -------------------------------------------------------------------
set PARADOX_OUTPUT_DIR=%~dp0\..\Bin\Windows-Direct3D11
set PARADOX_VSIX=%PARADOX_OUTPUT_DIR%\SiliconStudio.Paradox.vsix
REM -------------------------------------------------------------------
REM Build Paradox
REM -------------------------------------------------------------------
IF EXIST "%PARADOX_VSIX%" GOTO :vsixok
echo Error, unable to find Paradox VSIX [%PARADOX_VSIX%]
echo Run 00-BuildParadox.bat before trying to install the VisualStudio package
pause
exit /b 1
:vsixok
"%PARADOX_VSIX%"
