"%ProgramFiles(x86)%\Git\Bin\git.exe" clone git@github.com:sshnet/SSH.NET.git -b master ../../externals/SSH.NET
cd ../../externals/SSH.NET
"%ProgramFiles(x86)%\Git\Bin\git.exe" checkout e359bd8cedd411f2e5579f7b269b91835e800771

if NOT ERRORLEVEL 0 pause
