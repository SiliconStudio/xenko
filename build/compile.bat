@echo off

setlocal

set STARTTIME=%TIME%

set XXMSBUILD="\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
set _option=/m /verbosity:normal /p:Configuration=Debug /p:Platform="Mixed Platforms"

set Project=Xenko.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Xenko.Direct3D.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Xenko.Direct3D.SDL.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Xenko.Direct3D.CoreCLR.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Xenko.OpenGL.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Xenko.OpenGL.CoreCLR.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Xenko.WindowsPhone.sln
%XXMSBUILD%  %_option% /p:Platform="WindowsPhone" %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Xenko.WindowsStore.sln
%XXMSBUILD%  %_option% /p:Platform="WindowsStore" %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Xenko.Windows10.sln
%XXMSBUILD%  %_option% /p:Platform="Windows10" %Project%
if %ERRORLEVEL% != 0 goto error

sleep 2

goto exit

:error
echo "Error while compiling project: " %Project%
echo "Using command line" %XXMSBUILD% %_option% %Project%

:exit

set ENDTIME=%TIME%

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
