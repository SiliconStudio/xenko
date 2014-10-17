@echo off
setlocal
set ParadoxSdkDir=%~dp0..\..\..\..\
set ParadoxSdkBinDir=%ParadoxSdkDir%Bin\Windows-Direct3D11\
%ParadoxSdkBinDir%SiliconStudio.Assets.CompilerApp.exe --platform=Windows --profile=Windows --output-path=%~dp0obj\app_data --build-path=%~dp0obj\build_app_data --package-file=Physics.pdxpkg
