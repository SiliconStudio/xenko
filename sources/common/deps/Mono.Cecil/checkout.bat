@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\cmd\git.exe" clone https://github.com/SiliconStudio/Mono.Cecil.git -b master ../../externals/Mono.Cecil
if NOT ERRORLEVEL 0 pause