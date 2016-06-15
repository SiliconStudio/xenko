@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\cmd\git.exe" clone https://github.com/SiliconStudio/SharpYaml.git -b master ../../externals/SharpYaml
if NOT ERRORLEVEL 0 pause