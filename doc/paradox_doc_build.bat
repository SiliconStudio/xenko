@echo off
setlocal
set SHARPDOC=..\sources\common\deps\SharpDoc\Bin\SharpDoc.exe
REM set SHARPDOC=..\externals\SharpDoc\Build\Tools\Bin\SharpDoc.exe
%SHARPDOC% -c paradox_doc_config.xml -w=https://pj.siliconstudio.co.jp/confluence/display/PDXDOC/ -wL=%APPDATA%\ParadoxDoc\auth.conf
