@echo off

call "%VS110COMNTOOLS%..\..\VC\vcvarsall.bat"

msbuild.exe ..\SiliconStudio.Net.sln /p:Configuration=Release /p:Platform="Mixed Platforms" /p:DeployExtension=false /t:rebuild /p:GenerateDoc=true /p:NoWarn=1591 

setlocal
set SHARPDOC=..\deps\SharpDoc\Bin\SharpDoc.exe
REM set SHARPDOC=..\externals\SharpDoc\Build\Tools\Bin\SharpDoc.exe
%SHARPDOC% -c common_doc_config.xml -w=https://pj.siliconstudio.co.jp/confluence/display/PDX/ -wL=%APPDATA%\XenkoDoc\auth.conf
