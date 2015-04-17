@echo off
setlocal
set ParadoxSdkDir=%~dp0..\..\..\..\
set ParadoxSdkBinDir=%ParadoxSdkDir%Bin\Windows-Direct3D11\
%ParadoxSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --platform=Windows --profile=Windows --graphics-platform=Direct3D11 --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.pdxpkg
%ParadoxSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --platform=Windows --profile=Windows --graphics-platform=OpenGL --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.pdxpkg
%ParadoxSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --platform=Windows --profile=Windows --graphics-platform=OpenGLES --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.pdxpkg
