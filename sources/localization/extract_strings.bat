@set SOURCES_DIR=C:\Projects\xenko2\sources
@set OUTPUT_DIR=C:\Projects\xenko2\sources\localization\

@cd "%OUTPUT_DIR%"

rem SiliconStudio.Presentation.pot
..\..\Bin\Windows\SiliconStudio.Translation.Extractor.exe --directory=%SOURCES_DIR%\common\presentation\SiliconStudio.Presentation --domain-name=SiliconStudio.Presentation --recursive --preserve-comments --exclude=*.Designer.cs --verbose *.xaml *.cs

rem SiliconStudio.Xenko.Assets.Presentation.pot
..\..\Bin\Windows\SiliconStudio.Translation.Extractor.exe --directory=%SOURCES_DIR%\editor\SiliconStudio.Xenko.Assets.Presentation --domain-name=SiliconStudio.Xenko.Assets.Presentation --recursive --preserve-comments --exclude=*.Designer.cs --verbose *.xaml *.cs

rem SiliconStudio.Assets.Editor.pot
 ..\..\Bin\Windows\SiliconStudio.Translation.Extractor.exe --directory=%SOURCES_DIR%\editor\SiliconStudio.Assets.Editor --domain-name=SiliconStudio.Assets.Editor --recursive --preserve-comments --exclude=*.Designer.cs --verbose *.xaml *.cs

rem Xenko.GameStudio.pot
 ..\..\Bin\Windows\SiliconStudio.Translation.Extractor.exe --directory=%SOURCES_DIR%\editor\SiliconStudio.Xenko.GameStudio --domain-name=Xenko.GameStudio --recursive --preserve-comments --exclude=*.Designer.cs --verbose *.xaml *.cs
