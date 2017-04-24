Silicon Studio .NET
===================

SiliconStudio .NET is a collection of shared C#/.NET code that is project independent.

Folders and projects layout
---------------------------

###core###

* __SiliconStudio.Core__:
   Reference counting, dependency property system (PropertyContainer/PropertyKey), low-level serialization, low-level memory operations (Utilities and NativeStream).
* __SiliconStudio.Core.Mathematics__:
   Mathematics library (despite its name, no dependencies on SiliconStudio.Core).
* __SiliconStudio.Core.IO__:
   Virtual File System.
* __SiliconStudio.Core.Serialization__:
   High-level serialization and git-like CAS storage system.
* __SiliconStudio.MicroThreading__:
   Micro-threading library based on C# 5.0 async (a.k.a. stackless programming)
* __SiliconStudio.AssemblyProcessor__:
   Internal tool used to patch assemblies to add various features, such as Serialization auto-generation, various memory/pinning operations, module initializers, etc...
   
###presentation###

* __SiliconStudio.Presentation__: WPF UI library (themes, controls such as propertygrid, behaviors, etc...)
* __SiliconStudio.SampleApp__: Simple property grid example.
* __SiliconStudio.Quantum__: Advanced ViewModel library that gives ability to synchronize view-models over network (w/ diff), and at requested time intervals. That way, view models can be defined within engine without any UI dependencies.

###buildengine###

* __SiliconStudio.BuildEngine.Common__:
   Common parts of the build engine. It can be reused to add new build steps, build commands, and also to build a new custom build engine client.
* __SiliconStudio.BuildEngine__: Default implementation of build engine tool (executable)
* __SiliconStudio.BuildEngine.Monitor__: WPF Display live results of build engine (similar to IncrediBuild)
* __SiliconStudio.BuildEngine.Editor__: WPF Build engine rules editor
and used by most projects.

###shaders###

* __Irony__: Parsing library, used by SiliconStudio.Shaders. Should later be replaced by ANTLR4.
* __SiliconStudio.Shaders__: Shader parsing, type analysis and conversion library (used by HLSL->GLSL and Xenko Shader Language)

###targets###

* MSBuild target files to create easily cross-platform solutions (Android, iOS, WinRT, WinPhone, etc...), and define behaviors and targets globally. Extensible.

----------

Use in your project
-------------------

###Source repository###

There is two options to integrate this repository in your own repository:

* __git subtree__ [documentation](https://github.com/git/git/blob/master/contrib/subtree/git-subtree.txt) and [blog post](http://psionides.eu/2010/02/04/sharing-code-between-projects-with-git-subtree/)
* __git submodule__

###Basic use###

Simply add the projects you want to use directly in your Visual Studio solution.

###Optional: Activate assembly processor###

If you want to use auto-generated `Serialization` code, some of `Utilities` functions or `ModuleInitializer`, you need to use __SiliconStudio.AssemblyProcessor__.

Steps:

* Include both __SiliconStudio.AssemblyProcessor__ and __SiliconStudio.AssemblyProcessor.Common__ in your solution.
* Add either a __SiliconStudio.PostSettings.Local.targets__ or a __YourSolutionName.PostSettings.Local.targets__ in your solution folder, with this content:

```xml
<!-- Build file pre-included automatically by all projects in the solution -->
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <!-- Enable assembly processor -->
    <SiliconStudioAssemblyProcessorGlobal>true</SiliconStudioAssemblyProcessorGlobal>
  </PropertyGroup>
</Project>
```
