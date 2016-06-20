"%ProgramFiles(x86)%\Git\Bin\git.exe" clone https://github.com/SiliconStudio/freetype.git -b 2.6.3 ../../externals/freetype
if NOT ERRORLEVEL 0 pause
pushd ..\..\externals\freetype
"%ProgramFiles(x86)%\Git\Bin\git.exe" remote add upstream http://git.sv.nongnu.org/r/freetype/freetype2.git
"%ProgramFiles(x86)%\Git\Bin\git.exe" fetch --all
popd
if NOT ERRORLEVEL 0 pause
