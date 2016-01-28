// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SiliconStudio.Xenko.Engine")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyProduct("SiliconStudio.Xenko.Engine")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a9968d1f-7e75-4d89-8411-27390a47e4d0")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

// Make internals Xenko visible to Xenko assemblies
// TODO: Needed for ParameterCollection getters, but it would be better to avoid this kind of dependency.
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Assets" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Assets.Presentation" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine.Shaders" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Assets.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics.Regression" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Debugger" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Audio.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudioXenkoAudioTests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine.Audio.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudioXenkoGraphicsTests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudioXenkoEngineTests" + SiliconStudio.PublicKeys.Default)]