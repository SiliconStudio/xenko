cd ..\..\externals\SSH.NET\src
msbuild /p:Configuration=Release Renci.SshNet.VS2015.sln

copy Renci.SshNet\bin\Release\Renci.SshNet.* ..\..\..\deps\SSH.NET

cd ..\..\..\deps\SSH.NET
