pushd ..\..\..\bin\iOS-OpenGLES
..\..\sources\common\deps\Mono.Cecil\monolinker -a SiliconStudio.Xenko.Engine.dll -a SiliconStudio.Xenko.Games.dll -a SiliconStudio.Xenko.Input.dll -a SiliconStudio.Xenko.Graphics.dll -p link OpenTK-1.1 -u copy -b true -d "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\Xamarin.iOS\v1.0"
popd

copy ..\..\..\bin\iOS-OpenGLES\output\OpenTK-1.1.dll .