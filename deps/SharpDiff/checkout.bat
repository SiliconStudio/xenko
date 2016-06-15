@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\Bin\git.exe" clone git@github.com:SiliconStudio/SharpDiff.git ../../externals/SharpDiff
if NOT ERRORLEVEL 0 pause
