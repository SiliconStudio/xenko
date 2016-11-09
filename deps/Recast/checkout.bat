@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\Bin\git.exe" clone https://github.com/recastnavigation/recastnavigation.git %~dp0..\..\externals\recast
if NOT ERRORLEVEL 0 pause