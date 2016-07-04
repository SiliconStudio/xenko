call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86

REM Windows
msbuild ..\..\externals\opentk\OpenTK.sln /Property:Configuration=Release;Platform="Any CPU"
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.dll .
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Release\OpenTK.pdb .

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

mkdir CoreCLR
msbuild ..\..\externals\opentk\source\OpenTK\OpenTK.csproj /Property:Configuration=ReleaseCoreCLR;Platform=AnyCPU
mkdir CoreCLR\Windows
copy /Y ..\..\externals\opentk\Binaries\OpenTK\ReleaseCoreCLR\OpenTK.dll CoreCLR\Windows
copy /Y ..\..\externals\opentk\Binaries\OpenTK\ReleaseCoreCLR\OpenTK.pdb CoreCLR\Windows

msbuild ..\..\externals\opentk\source\OpenTK\OpenTK.csproj /Property:Configuration=ReleaseCoreCLR;Platform=Linux
mkdir CoreCLR\Linux
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Linux\ReleaseCoreCLR\OpenTK.dll CoreCLR\Linux
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Linux\ReleaseCoreCLR\OpenTK.pdb CoreCLR\Linux

msbuild ..\..\externals\opentk\source\OpenTK\OpenTK.csproj /Property:Configuration=ReleaseMinimal;Platform=Linux
mkdir Linux
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Linux\ReleaseMinimal\OpenTK.dll Linux
copy /Y ..\..\externals\opentk\Binaries\OpenTK\Linux\ReleaseMinimal\OpenTK.pdb Linux
