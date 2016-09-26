pushd ..\..\externals\NuGet
powershell.exe .\build.ps1 -SkipVS15 -SkipTests -Configuration Release
if NOT ERRORLEVEL 0 pause
popd

copy ..\..\externals\NuGet\src\NuGet.Core\NuGet.Client\bin\release\net45\*.dll .
copy ..\..\externals\NuGet\src\NuGet.Core\NuGet.Protocol.Core.v2\bin\release\net45\*.dll .
copy ..\..\externals\NuGet\src\NuGet.Core\NuGet.Protocol.Core.v3\bin\release\net45\*.dll .
copy ..\..\externals\NuGet\src\NuGet.Core\NuGet.PackageManagement\bin\release\net45\*.dll .
