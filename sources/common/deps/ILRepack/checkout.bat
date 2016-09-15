@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\cmd\git.exe" clone --recursive https://github.com/SiliconStudio/il-repack.git -b master ../../externals/ILRepack
pushd ..\..\externals\ILRepack
"%ProgramFiles(x86)%\Git\cmd\git.exe" remote add upstream https://github.com/gluck/il-repack.git
"%ProgramFiles(x86)%\Git\cmd\git.exe" fetch upstream
popd
if NOT ERRORLEVEL 0 pause
