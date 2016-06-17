Xenko
=======

This is the source code for Xenko Game Engine (http://xenko.com/).

## License

* [Licensing and Contributions](LICENSE.md)

## Community

* Chat with the community at [![Join the chat at https://gitter.im/SiliconStudio/xenko](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/SiliconStudio/xenko?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)
* Ask and answer questions on our QA website: http://answers.xenko.com/
* Discuss on our forums: http://forums.xenko.com/

## Documentation

* [Build Instructions](doc/GettingStarted.md)
* [Git LFS](doc/GitLFS.md): read about our recent switch to LFS (if you need to upgrade a non-LFS Xenko repository or an existing non-LFS Xenko branch)
* [API Reference](http://doc.xenko.com/latest/api/SiliconStudio.Assets.html)
* [Documentation](http://doc.xenko.com/latest)

## Assemblies

[Assembly diagram](http://doc.xenko.com/latest/manual/engine/assemblies-diagrams.html)

* [__SiliconStudio.Xenko.Graphics__](http://doc.xenko.com/latest/manual/graphics/index.html):
   Platform-indepdenent D3D11-like rendering API. Implementations for Direct3D 11 (with feature levels 9.1 and 10), OpenGL 4 and OpenGL ES 2.0.
* __SiliconStudio.Xenko.Games__:
   Windows and game loop management.
* [__SiliconStudio.Xenko.Input__](http://doc.xenko.com/latest/manual/input/index.html):
   Input management, including keyboard, joystick, mouse, touch, gestures.
* __SiliconStudio.Xenko.Engine__:
   Effect system, entity system, particle system, high-level audio engine, etc...
* [__SiliconStudio.Xenko.UI__](http://doc.xenko.com/latest/manual/ui/index.html):
   In-game UI library, similar to WPF (including many UI Controls).
* [__SiliconStudio.Xenko.Shaders__](http://doc.xenko.com/latest/manual/graphics/graphics-reference/effects-and-shaders-reference/shading-language/index.html):
   Xenko shader language, including many new language constructs to make shader programming much more easy/modular.
* [__SiliconStudio.Xenko.Audio__](http://doc.xenko.com/latest/manual/audio/index.html):
   Low-level audio engine.
* __SiliconStudio.Assets__:
   Modular asset project management and pipeline system.
* __SiliconStudio.Xenko.GameStudio__:
   Asset editor for Xenko. Allow asset browsing and editing, and Xenko Asset project editing.
   
We currently do not provide sources for:
* SiliconStudio.Xenko.GameStudio due to a licensed third party library that we use, Telerik. That might be lifted in the future.
* Autodesk Max and Maya plugin (which will be released in the future) due to SDK licensing restrictions.
   
----------

Silicon Studio .NET
===================

SiliconStudio .NET is a collection of shared C#/.NET code that is project independent. It is located inside [sources/common](sources/common) subfolder.

## Folders and projects layout

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
