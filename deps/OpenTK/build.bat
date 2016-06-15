call "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\vc\vcvarsall.bat" x86

REM Windows
msbuild ..\..\externals\opentk\OpenTK.sln /Property:Configuration=Release;Platform="Any CPU"
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.dll .
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.pdb .
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.GLControl.dll .
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.GLControl.pdb .

REM Android
msbuild ..\..\externals\opentk\OpenTK.Android.sln /Property:Configuration=Release;Platform="Any CPU"
mkdir Android
copy /Y ..\..\externals\opentk\Binaries\Android\Release\OpenTK-1.1.dll Android
copy /Y ..\..\externals\opentk\Binaries\Android\Release\OpenTK-1.1.dll.mdb Android

REM iOS
msbuild ..\..\externals\opentk\OpenTK.iOS.sln /Property:Configuration=Release;Platform="Any CPU"
mkdir iOS
copy /Y ..\..\externals\opentk\Binaries\iOS\Release\OpenTK-1.1.dll iOS
copy /Y ..\..\externals\opentk\Binaries\iOS\Release\OpenTK-1.1.dll.mdb iOS
