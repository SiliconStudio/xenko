set SiliconStudioNativeTarget="..\..\..\..\..\sources\common\core\SiliconStudio.Core.Native\SiliconStudioNative.targets"

call "%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\vc\vcvarsall.bat" x86
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=Windows;Platform=Win32
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=Windows;Platform=x64
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=WindowsStore;Platform=x64
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=WindowsStore;Platform=Win32
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=WindowsStore;Platform=ARM
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=WindowsPhone;Platform=Win32
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=WindowsPhone;Platform=ARM
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=Windows10;Platform=x64
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=Windows10;Platform=Win32
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Debug;SiliconStudioPlatform=Windows10;Platform=ARM
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=Windows;Platform=Win32
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=Windows;Platform=x64
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=WindowsStore;Platform=x64
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=WindowsStore;Platform=Win32
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=WindowsStore;Platform=ARM
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=WindowsPhone;Platform=Win32
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=WindowsPhone;Platform=ARM
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=Windows10;Platform=x64
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=Windows10;Platform=Win32
msbuild ..\..\externals\freetype\builds\windows\vc2013\freetype.vcxproj /Property:SiliconStudioNativeTarget=%SiliconStudioNativeTarget%;Configuration=Release;SiliconStudioPlatform=Windows10;Platform=ARM

xcopy /Y /S ..\..\externals\freetype\builds\windows\vc2013\bin\*.dll .
xcopy /Y /S ..\..\externals\freetype\builds\windows\vc2013\bin\*.pdb .
