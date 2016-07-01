@echo off
setlocal
set CPPNET=%~dp0..\..\externals\CppNet
call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86

pushd %CPPNET

rem We have to delete the lock file otherwise we cannot compile non-UWP target in the same location
if exist project.lock.json del project.lock.json

rem Build Non-portable version
msbuild /nologo /p:Configuration=Release CppNet.sln
if ERRORLEVEL 1 echo "Cannot build CppNet" && pause

rem Build CoreCLR version
msbuild /nologo /p:Configuration=ReleaseCoreCLR CppNet.sln
if ERRORLEVEL 1 echo "Cannot build CppNet for CoreCLR" && pause

rem Build UWP version
rem We have to restore the packages. Note that once it is restored, you will have to delete
rem project.lock.json in order to compile the non-UWP targets

if not exist nuget3.exe wget http://dist.nuget.org/win-x86-commandline/v3.1.0-beta/nuget.exe
rem Special trick as wget from cygwin seems to create a file we cannot execute, copying is sufficient
rem to make the destination file executable
copy nuget.exe nuget3.exe
nuget3.exe restore project.json

msbuild /nologo /p:Configuration=Release CppNet_Uwp.sln
if ERRORLEVEL 1 echo "Cannot build CppNet for CoreCLR" && pause

popd

xcopy /Y /Q %CPPNET%\Bin\Release\CppNet.dll . > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.dll" && pause
xcopy /Y /Q %CPPNET%\Bin\Release\CppNet.pdb . > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.pdb" && pause

xcopy /Y /Q %CPPNET%\Bin\CoreCLR\Release\CppNet.dll CoreCLR\ > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.dll to CoreCLR" && pause
xcopy /Y /Q %CPPNET%\Bin\CoreCLR\Release\CppNet.pdb CoreCLR\ > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.pdb to CoreCLR" && pause

xcopy /Y /Q %CPPNET%\Bin\UWP\Release\CppNet.dll UWP\ > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.dll to UWP" && pause
xcopy /Y /Q %CPPNET%\Bin\UWP\Release\CppNet.pdb UWP\ > nul
if ERRORLEVEL 1  echo "Cannot copy CppNet.pdb to UWP" && pause

echo CppNet build completed successfully
