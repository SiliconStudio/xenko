// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Core.MicroThreading")]
[assembly: AssemblyTitle("SiliconStudio.Core.MicroThreading")]
[assembly: AssemblyDescription("SiliconStudio Core Microthreading assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

// Make internals Paradox.Framework.visible to all Paradox.Framework.assemblies
[assembly: InternalsVisibleTo("SiliconStudio.Core.MicroThreading.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Debugger" + SiliconStudio.PublicKeys.Default)]
