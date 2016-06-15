@echo off

if "%1" == "" (
	echo Missing Debug or Release argument
	EXIT /B 1
)

pushd ..\..\externals\SDL2-CS

REM SDL2-CS
call "%PROGRAMFILES(X86)%\Microsoft Visual Studio 14.0\Common7\Tools\VsMSBuildCmd.bat"
msbuild /p:SiliconStudioRuntime="CoreCLR" /p:Configuration="%1" /p:Platform="Any CPU" SDL2-CS.sln
if %ERRORLEVEL% neq 0 (
	echo Error during compilation
	popd
	EXIT /B %ERRORLEVEL%
)

popd

rem Copying assemblies
copy ..\..\externals\SDL2-CS\bin\%1\SDL2-CS.dll .
copy ..\..\externals\SDL2-CS\bin\%1\SDL2-CS.pdb .

