call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86
pushd ..\..\externals\il-repack
call gradlew repack
popd
copy ..\..\externals\il-repack\build\tmp\repack\ILRepack.exe .
copy ..\..\externals\il-repack\build\tmp\repack\ILRepack.pdb .
