@echo off

set XXMSBUILD="\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe"
set _option=/m /verbosity:normal /p:Configuration=Debug /p:Platform="Mixed Platforms"

set Project=Paradox.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Paradox.Direct3D.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Paradox.Direct3D.SDL.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

set Project=Paradox.Direct3D.CoreCLR.sln
%XXMSBUILD%  %_option% %Project%
if %ERRORLEVEL% != 0 goto error

goto exit

:error
echo "Error while compiling project: " %Project%

:exit

@echo on
