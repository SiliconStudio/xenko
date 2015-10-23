@echo off
setlocal
rmdir /S /Q "%LOCALAPPDATA%\temp\SiliconStudio"
REM -------------------------------------------------------------------
REM Global config
REM -------------------------------------------------------------------
set XENKO_SOLUTION=Xenko.iOS.sln
set XENKO_PLATFORM=Configuration=Release;Platform=iPhone
set XENKO_BUILD_LOG=%~n0.log
set XENKO_OUTPUT_DIR=%~dp0\Bin\iOS-AnyCPU-OpenGLES
REM -------------------------------------------------------------------
REM Build Xenko
REM -------------------------------------------------------------------
SET ProgFiles86Root=%ProgramFiles(x86)%
IF NOT "%ProgFiles86Root%"=="" GOTO win64
SET ProgFiles86Root=%ProgramFiles%
:win64
set VS_VCVARSALL=%ProgFiles86Root%\Microsoft Visual Studio 11.0\vc\vcvarsall.bat
IF EXIST "%VS_VCVARSALL%" GOTO :vcvarok
echo Error, unable to find path Visual Studio 2012 Path: [%VS_VCVARSALL%]
exit /b
:vcvarok
call "%VS_VCVARSALL%" x86
echo Building Xenko, Please wait...
REM Explicitly remove the Bin directory to make a full cleanup
IF EXIST "%XENKO_OUTPUT_DIR%" rmdir /S /Q %XENKO_OUTPUT_DIR%
REM msbuild Clean
IF EXIST "%XENKO_BUILD_LOG%" del /F "%XENKO_BUILD_LOG%"
msbuild /nologo /tv:4.0 /t:Clean /verbosity:minimal /fl "/flp:Summary;Verbosity=minimal;logfile=%XENKO_BUILD_LOG%" "/p:%XENKO_PLATFORM%" %XENKO_SOLUTION%
if %ERRORLEVEL% neq 0 GOTO :error_pause
REM msbuild Build
IF EXIST "%XENKO_BUILD_LOG%" del /F "%XENKO_BUILD_LOG%"
msbuild /nologo /tv:4.0 /t:Build /verbosity:minimal /fl "/flp:Summary;Verbosity=minimal;logfile=%XENKO_BUILD_LOG%" "/p:%XENKO_PLATFORM%" %XENKO_SOLUTION%
if %ERRORLEVEL% neq 0 GOTO :error_pause
GOTO :end
:error_pause
echo Check full error log in %XENKO_BUILD_LOG%
pause
:end

