call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86
msbuild ..\..\externals\il-repack\ILRepack.sln /Property:Configuration=Release;Platform="Any CPU" /target:ILRepack
pushd ..\..\externals\il-repack\ILRepack\bin\Release
ILRepack.exe ILRepack.exe Fasterflect.dll BamlParser.dll Mono.Posix.dll /out:%~dp0\ILRepack.exe
popd
