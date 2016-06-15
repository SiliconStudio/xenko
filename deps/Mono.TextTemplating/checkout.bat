@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\Bin\git.exe" clone git@github.com:SiliconStudio/Mono.TextTemplating.git ../../externals/Mono.TextTemplating
if NOT ERRORLEVEL 0 pause

