@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\Bin\git.exe" clone https://github.com/assimp/assimp.git %~dp0..\..\externals\assimp
if NOT ERRORLEVEL 0 pause