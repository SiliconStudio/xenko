"%ProgramFiles(x86)%\Git\Bin\git.exe" clone git@github.com:sshnet/SSH.NET.git -b master ../../externals/SSH.NET
cd ..\..\externals\SSH.NET
"%ProgramFiles(x86)%\Git\Bin\git.exe" checkout 4f25cc00a7129bfd001c8d79a4ae3d69655751ca

cd ..\..\deps\SSH.NET

if NOT ERRORLEVEL 0 pause
