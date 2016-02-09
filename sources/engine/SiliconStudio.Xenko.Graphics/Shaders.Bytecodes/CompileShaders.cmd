@echo off
setlocal
set XenkoSdkDir=%~dp0..\..\..\..\
set XenkoSdkBinDir=%XenkoSdkDir%Bin\Windows-Direct3D11\
%XenkoSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --profile=Windows --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.xkpkg
%XenkoSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --profile=Windows-OpenGL --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.xkpkg
%XenkoSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --profile=Windows-OpenGLES --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.xkpkg
%XenkoSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --profile=Windows-Vulkan --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Graphics.xkpkg
