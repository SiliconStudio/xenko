REM @echo off
setlocal
set SHARPDIFF=%~dp0..\..\externals\SharpDiff
call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86
msbuild /tv:14.0 /t:Build /verbosity:quiet /clp:ErrorsOnly /fl /flp:logfile=BuildErrors.log;ErrorsOnly "/p:Configuration=Release;Platform=Any CPU" %SHARPDIFF%\SharpDiff.sln
if NOT ERRORLEVEL 0 pause

xcopy /Y %SHARPDIFF%\SharpDiff\Bin\Release\SharpDiff.dll .
xcopy /Y %SHARPDIFF%\SharpDiff\Bin\Release\SharpDiff.pdb .
xcopy /Y %SHARPDIFF%\SharpDiff\Bin\Release\SharpDiff.xml .
if NOT ERRORLEVEL 0  pause
	