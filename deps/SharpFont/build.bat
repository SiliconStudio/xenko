msbuild ..\..\externals\SharpFont\Source\SharpFont.sln /p:Configuration=Debug
msbuild ..\..\externals\SharpFont\Source\SharpFont.sln /p:Configuration=Release

xcopy /Y /S ..\..\externals\SharpFont\Binaries\SharpFont\Portable\Debug\* Portable\Debug\
xcopy /Y /S ..\..\externals\SharpFont\Binaries\SharpFont\Portable\Release\* Portable\Release\
xcopy /Y /S ..\..\externals\SharpFont\Binaries\SharpFont\iOS\Debug\* iOS\Debug\
xcopy /Y /S ..\..\externals\SharpFont\Binaries\SharpFont\iOS\Release\* iOS\Release\
