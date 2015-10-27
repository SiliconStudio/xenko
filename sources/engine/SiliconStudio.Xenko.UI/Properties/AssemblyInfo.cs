// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SiliconStudio.Paradox.UI")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyProduct("SiliconStudio.Paradox.UI")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("106cd3bf-dec8-4f32-adcc-b089ab95736c")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Paradox.UI.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.UI.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudioParadoxUITests" + SiliconStudio.PublicKeys.Default)]