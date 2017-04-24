@echo OFF
setlocal
set HOME=%USERPROFILE%
"%ProgramFiles(x86)%\Git\Bin\git.exe" clone git@github.com:SiliconStudio/opentk ../../externals/opentk -b develop
if NOT ERRORLEVEL 0 pause
pushd ..\..\externals\opentk
"%ProgramFiles(x86)%\Git\Bin\git.exe" remote add upstream git@github.com:opentk/opentk.git
"%ProgramFiles(x86)%\Git\Bin\git.exe" fetch --all
popd
if NOT ERRORLEVEL 0 pause