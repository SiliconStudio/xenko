@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\Bin\git.exe" clone https://github.com/SiliconStudio/glsl-optimizer.git ../../externals/glsl-optimizer -b paradox
if NOT ERRORLEVEL 0 pause
pushd ..\..\externals\glsl-optimizer
"%ProgramFiles(x86)%\Git\Bin\git.exe" remote add upstream https://github.com/aras-p/glsl-optimizer.git
"%ProgramFiles(x86)%\Git\Bin\git.exe" fetch --all
popd
if NOT ERRORLEVEL 0 pause