REM @echo off
setlocal
set SHARPYAML=%~dp0..\..\externals\SharpYaml
call "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\vc\vcvarsall.bat" x86
msbuild /tv:4.0 /t:Build /verbosity:quiet /clp:ErrorsOnly /fl /flp:logfile=BuildErrors.log;ErrorsOnly "/p:Configuration=Release;Platform=Mixed Platforms" %SHARPYAML%\SharpYaml.sln
if NOT ERRORLEVEL 0 pause

xcopy /Y %SHARPYAML%\SharpYaml\Bin\Release\SharpYaml.dll .
xcopy /Y %SHARPYAML%\SharpYaml\Bin\Release\SharpYaml.pdb .
xcopy /Y %SHARPYAML%\SharpYaml\Bin\Release\SharpYaml.xml .
if NOT ERRORLEVEL 0  pause
	