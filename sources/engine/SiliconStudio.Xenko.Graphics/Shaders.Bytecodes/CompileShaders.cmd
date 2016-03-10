@echo off
setlocal
set XenkoSdkDir=%~dp0..\..\..\..\
set XenkoSdkBinDir=%XenkoSdkDir%Bin\Windows-Direct3D11\
%XenkoSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --profile=Windows --graphics-platform=Direct3D11 --platform=Windows --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.xkpkg
%XenkoSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --profile=Windows-OpenGL --graphics-platform=OpenGL --platform=Windows --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.xkpkg
%XenkoSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --profile=Windows-OpenGLES --graphics-platform=OpenGLES --platform=Windows --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.xkpkg