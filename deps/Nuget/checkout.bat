"%ProgramFiles(x86)%\Git\Bin\git.exe" clone https://github.com/NuGet/NuGet.Client.git -b release-3.5.0-rtm ../../externals/NuGet
if NOT ERRORLEVEL 0 pause
