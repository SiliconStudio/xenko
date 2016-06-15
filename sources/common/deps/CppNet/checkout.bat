@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\cmd\git.exe" clone --recursive ssh://git@tecsigma.siliconstudio.co.jp:7999/pdx/cppnet.git -b master ../../externals/CppNet
pushd ..\..\externals\CppNet
"%ProgramFiles(x86)%\Git\cmd\git.exe" remote add github git@github.com:SiliconStudio/CppNet.git
"%ProgramFiles(x86)%\Git\cmd\git.exe" fetch --all
popd
if ERRORLEVEL 1 echo "Could not checkout CppNet" && pause
