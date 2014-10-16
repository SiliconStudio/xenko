setlocal
set PARADOX_PATH=%~dp0..\..\..
set ILREPACK=%PARADOX_PATH%\Bin\Windows-Direct3D11\ILRepack.exe
set LAUNCHER_PATH=%~dp0Bin\Release
pushd %LAUNCHER_PATH%
"%ILREPACK%" SiliconStudio.LauncherApp.exe Nuget.exe /out:Paradox.exe
Nuget.exe pack %~dp0SiliconStudio.LauncherApp.nuspec -BasePath %LAUNCHER_PATH%
popd