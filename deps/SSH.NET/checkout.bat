"%ProgramFiles(x86)%\Git\Bin\git.exe" clone git@github.com:sshnet/SSH.NET.git -b master ../../externals/SSH.NET
cd ..\..\deps\SSH.NET

if NOT ERRORLEVEL 0 pause
