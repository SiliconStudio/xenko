call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86
@REM These variables are set by VCVarsQueryRegistry.bat and need to be cleared (as of VS2015 RC)
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=Windows;Platform=Win32 /t:Rebuild
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=Windows;Platform=x64 /t:Rebuild
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=WindowsStore;Platform=x64 /t:Rebuild
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=WindowsStore;Platform=Win32 /t:Rebuild
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=WindowsStore;Platform=ARM /t:Rebuild
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=WindowsPhone;Platform=Win32 /t:Rebuild
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=WindowsPhone;Platform=ARM /t:Rebuild
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=Windows10;Platform=x64 /t:Rebuild
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=Windows10;Platform=Win32 /t:Rebuild
msbuild ..\..\core\SiliconStudio.Core.Native\Core.Native.vcxproj /Property:Configuration=Release;SiliconStudioPlatform=Windows10;Platform=ARM /t:Rebuild

xcopy /Y /S ..\..\core\SiliconStudio.Core.Native\bin\*.dll .