call "%ProgramFiles(x86)%\Microsoft Visual Studio 12.0\vc\vcvarsall.bat" x86
msbuild ..\..\externals\Mono.Cecil\Mono.Cecil.sln /Property:Configuration=net_4_0_Release;Platform="Any CPU"
copy ..\..\externals\Mono.Cecil\bin\net_4_0_Release\*.* .
