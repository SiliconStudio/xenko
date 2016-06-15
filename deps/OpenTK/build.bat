call "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\vc\vcvarsall.bat" x86
msbuild ..\..\externals\opentk\OpenTK.sln /Property:Configuration=Release;Platform="Any CPU"
copy ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.dll .
copy ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.pdb .
copy ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.GLControl.dll .
copy ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.GLControl.pdb .
