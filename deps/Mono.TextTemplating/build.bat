REM @echo off
setlocal
set TEXTTEMPLATING=%~dp0..\..\externals\Mono.TextTemplating
call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86
msbuild /tv:4.0 /t:Build /verbosity:quiet /clp:ErrorsOnly /fl /flp:logfile=BuildErrors.log;ErrorsOnly "/p:Configuration=Release;Platform=Any CPU" %TEXTTEMPLATING%\Mono.TextTemplating.sln
if NOT ERRORLEVEL 0 pause

xcopy /Y %TEXTTEMPLATING%\Mono.TextTemplating\Bin\Release\Mono.TextTemplating.dll .
xcopy /Y %TEXTTEMPLATING%\Mono.TextTemplating\Bin\Release\Mono.TextTemplating.pdb .
xcopy /Y %TEXTTEMPLATING%\Mono.TextTemplating\Bin\Release\Mono.TextTemplating.xml .
if NOT ERRORLEVEL 0  pause
	
