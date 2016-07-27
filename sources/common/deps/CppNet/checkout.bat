@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\cmd\git.exe" clone --recursive git@github.com:SiliconStudio/CppNet.git -b master ../../externals/CppNet
if ERRORLEVEL 1 echo "Could not checkout CppNet" && pause
