@echo off

echo Processing Windows
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Direct3D.sln -p Windows
echo.

echo Processing Windows with CoreCLR
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Direct3D.CoreCLR.sln -p Windows 
echo.

echo Processing Windows with SDL
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Direct3D.SDL.sln -p Windows
echo.

echo Processing Windows with Vulkan
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Vulkan.sln -p Windows 
echo.

echo Processing Windows with Direct3D12
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Direct3D12.sln -p Windows
echo.

echo Processing Linux
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Linux.sln -p Linux
echo.

echo Processing Linux with Vulkan
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Linux.Vulkan.sln -p Linux
echo.

echo Processing Linux with CoreCLR
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Linux.CoreCLR.sln -p Linux
echo.

echo Processing Linux with Vulkan with CoreCLR
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Linux.Vulkan.CoreCLR.sln -p Linux
echo.

echo Processing Windows with OpenGL
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.OpenGL.sln -p Windows
echo.

echo Processing Windows with OpenGL with CoreCLR
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.OpenGL.CoreCLR.sln -p Windows
echo.

echo Processing Windows with OpenGLES
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.OpenGLES.sln -p Windows
echo.

echo Processing Android
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Android.sln -p Android
echo.

echo Processing iOS
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.iOS.sln -p iOS
echo.

echo Processing WindowsStore
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.WindowsStore.sln -p WindowsStore
echo.

echo Processing WindowsPhone
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.WindowsPhone.sln -p WindowsPhone
echo.

echo Processing Windows10
..\Bin\Windows-Direct3D11\SiliconStudio.Xenko.ProjectGenerator.exe solution Xenko.sln -o Xenko.Windows10.sln -p Windows10
echo.
