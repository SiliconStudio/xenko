call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86
msbuild ..\..\externals\glsl-optimizer\projects\vs2013\glsl_optimizer_lib.vcxproj /Property:Configuration=Release_Dll;Platform="Win32"
msbuild ..\..\externals\glsl-optimizer\projects\vs2013\glsl_optimizer_lib.vcxproj /Property:Configuration=Release_Dll;Platform="x64"
xcopy /Y ..\..\externals\glsl-optimizer\projects\vs2013\build\glsl_optimizer_lib\Win32\Release_Dll\*.dll Windows\x86\glsl_optimizer.dll
xcopy /Y ..\..\externals\glsl-optimizer\projects\vs2013\build\glsl_optimizer_lib\x64\Release_Dll\*.dll Windows\x64\glsl_optimizer.dll
