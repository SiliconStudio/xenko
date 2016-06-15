@echo off
setlocal
set CPPNET=%~dp0..\..\externals\CppNet
call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86

rem Build Non-portable version
msbuild /nologo /tv:4.0 /t:Build /verbosity:quiet /clp:ErrorsOnly /fl /flp:logfile=BuildErrors.log;ErrorsOnly "/p:Configuration=Release;Platform=AnyCPU" %CPPNET%\CppNet.csproj
if ERRORLEVEL 1 echo "Cannot build CppNet" && pause

xcopy /Y /Q %CPPNET%\Bin\Release\CppNet.dll . > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.dll" && pause
xcopy /Y /Q %CPPNET%\Bin\Release\CppNet.pdb . > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.pdb" && pause

rem Build portable version for use with CoreCLR
msbuild /nologo /tv:4.0 /t:Build /verbosity:quiet /clp:ErrorsOnly /fl /flp:logfile=BuildErrors.log;ErrorsOnly "/p:Configuration=Release;Platform=AnyCPU" %CPPNET%\CppNet_CoreCLR.csproj
if ERRORLEVEL 1 echo "Cannot build CppNet for CoreCLR" && pause

xcopy /Y /Q %CPPNET%\Bin\CoreCLR\Release\CppNet.dll CoreCLR\ > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.dll to CoreCLR" && pause
xcopy /Y /Q %CPPNET%\Bin\CoreCLR\Release\CppNet.pdb CoreCLR\ > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.pdb to CoreCLR" && pause

echo CppNet build completed successfully
