# Description

CoreFX Ref is providing a simple setup to let you compile an existing VisualStudio project against the CoreCLR/CoreFX target.

It provides the necessary MSBuild configuration files and the CoreFX reference assemblies which can be used in your VisualStudio project to compile against CoreCLR/CoreFX.

# Usage

To use this, update your project to import **configs\CoreCLR.CSharp.targets** instead of the default config which is usually one of **$(MSBuildToolsPath)\Microsoft.CSharp.targets** or **$(MSBuildExtensionsPath32)\Microsoft\Portable\$(TargetFrameworkVersion)\Microsoft.Portable.CSharp.targets**.

In addition make sure to define the following properties **TargetFrameworkRootPath** and **FrameworkPathOverride** to point where you checked out CoreFX Ref.

For example, if you have checked out the code in **C:\CoreFX**, then you should add:
```
  <TargetFrameworkRootPath>C:\CoreFX</TargetFrameworkRootPath>
  <FrameworkPathOverride>$(TargetFrameworkRootPath)\CoreCLR\v5.0</FrameworkPathOverride>
```
