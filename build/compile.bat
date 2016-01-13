@echo off

setlocal

set STARTTIME=%TIME%
set __SkipTestBuild=true
set __BuildType=Debug
set __BuildVerbosity=m

:Arg_Loop
rem This does not check for duplicate arguments, the last one will take precedence
if "%1" == "" goto ArgsDone
if /i "%1" == "/?" goto Usage
if /i "%1" == "debug" (set __BuildType=Debug && shift && goto Arg_loop)
if /i "%1" == "release" (set __BuildType=Release && shift && goto Arg_loop)
if /i "%1" == "tests" (set __SkipTestBuild=false && shift && goto Arg_loop)
if /i "%1" == "verbosity:q" (set __BuildVerbosity=q && shift && goto Arg_loop)
if /i "%1" == "verbosity:m" (set __BuildVerbosity=m && shift && goto Arg_loop)
if /i "%1" == "verbosity:n" (set __BuildVerbosity=n && shift && goto Arg_loop)
if /i "%1" == "verbosity:d" (set __BuildVerbosity=d && shift && goto Arg_loop)
echo.
echo Invalid command line argument: %1
echo.
goto Usage

:Usage
echo compile.bat [/? ^| debug ^| release ^| tests ^| verbosity:[q^|m^|n^|d]
echo.
echo   debug   : Build debug version
echo   release : Build release version
echo   tests   : Build tests
echo verbosity : Verbosity level [q]uiet, [m]inimal, [n]ormal or [d]iagnostic. Default is [m]inimal
echo.

goto exit


:ArgsDone
set XXMSBUILD="\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
set _option=/nologo /nr:false /m /verbosity:%__BuildVerbosity% /p:Configuration=%__BuildType% /p:Platform="Mixed Platforms" /p:SiliconStudioPackageBuild=%__SkipTestBuild%

set _extra_option=

set Project=Xenko.sln
call :compile
if %ERRORLEVEL% != 0 goto exit

set Project=Xenko.Direct3D.sln
call :compile
if %ERRORLEVEL% != 0 goto exit

set Project=Xenko.Direct3D.SDL.sln
call :compile
if %ERRORLEVEL% != 0 goto exit

set Project=Xenko.Direct3D.CoreCLR.sln
call :compile
if %ERRORLEVEL% != 0 goto exit

set Project=Xenko.OpenGL.sln
call :compile
if %ERRORLEVEL% != 0 goto exit

set Project=Xenko.OpenGL.CoreCLR.sln
call :compile
if %ERRORLEVEL% != 0 goto exit

set Project=Xenko.WindowsPhone.sln
set _extra_option=/p:Platform="WindowsPhone"
call :compile
if %ERRORLEVEL% != 0 goto exit

set Project=Xenko.WindowsStore.sln
set _extra_option=/p:Platform="WindowsStore"
call :compile
if %ERRORLEVEL% != 0 goto exit

set Project=Xenko.Windows10.sln
set _extra_option=/p:Platform="Windows10"
call :compile
if %ERRORLEVEL% != 0 goto exit

goto exit

rem Compile our solution. The following variables needs to be set:
rem "Project" is the solution name
rem "_option" are the command line options
rem "_extra_option" are the extra command line options
:compile
echo Compiling using command line %XXMSBUILD% %_option% %_extra_option% %Project%
echo.
%XXMSBUILD%  %_option% %_extra_option% %Project%
if %ERRORLEVEL% != 0 (
    echo Error while compiling project: %Project%
    echo Command line was: %XXMSBUILD% %_option% %_extra_option% %Project%
    exit /b 1
) else (
    echo Done compiling project: %Project%
)
echo.
goto :eof

:exit

set ENDTIME=%TIME%

echo.
echo Starting time was: %STARTTIME%
echo Ending time is   : %ENDTIME%

rem convert STARTTIME and ENDTIME to miliseconds
rem The format of %TIME% is HH:MM:SS,CS for example 23:59:59,99
set /A STARTTIME=(1%STARTTIME:~0,2%-100)*3600000 + (1%STARTTIME:~3,2%-100)*60000 + (1%STARTTIME:~6,2%-100)*1000 + (1%STARTTIME:~9,2%-100)*10
set /A ENDTIME=(1%ENDTIME:~0,2%-100)*3600000 + (1%ENDTIME:~3,2%-100)*60000 + (1%ENDTIME:~6,2%-100)*1000 + (1%ENDTIME:~9,2%-100)*10

rem calculating the duration is easy
set /A DURATION=%ENDTIME%-%STARTTIME%

rem we might have measured the time inbetween days
if %ENDTIME% LSS %STARTTIME% set set /A DURATION=%STARTTIME%-%ENDTIME%

set /A DURATION=%DURATION%/1000

rem outputing
echo Duration is      : %DURATION% seconds

endlocal

@echo on
